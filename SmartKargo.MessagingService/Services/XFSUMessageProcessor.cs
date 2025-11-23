#region XFSUMessageProcessor Class Description
/* XFSUMessageProcessor Class Description.
      * Company              :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright            :   Copyright © 2017 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose              :   XFSUMessageProcessor class generate xml string and insert into database and generate XFSU Message.
      * Created By           :   Yoginath
      * Created On           :   2017-07-05 
      * Approved By          :
      * Approved Date        :
      * Modified By          :  
      * Modified On          :   
      * Description          :   
     */
#endregion
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using QidWorkerRole.BAL;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace QidWorkerRole
{
    public class XFSUMessageProcessor
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<XFSUMessageProcessor> _logger;
        private readonly GenericFunction _genericFunction;
        private readonly cls_SCMBL _cl_SCMBL;
        private readonly FFRMessageProcessor _ffrMessageProcessor;
        private readonly CustomsImportBAL _objCustoms;

        #region Constructor
        public XFSUMessageProcessor(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<XFSUMessageProcessor> logger, 
            GenericFunction genericFunction,
            cls_SCMBL cl_SCMBL,
            FFRMessageProcessor fFRMessageProcessor, 
            CustomsImportBAL objCustoms)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
            _cl_SCMBL = cl_SCMBL;
            _ffrMessageProcessor = fFRMessageProcessor;
            _objCustoms = objCustoms;
        }
        #endregion

        //GenericFunction genericFunction = new GenericFunction();

        #region :: Public Methods ::

        public async Task GenerateAndSendXFSUMessages()
        {
            try
            {
                DataSet dsAWBRecords = new DataSet();
                dsAWBRecords = await GetAWBRecordsToAutoSendXFSUMessage();

                if (dsAWBRecords != null && dsAWBRecords.Tables != null && dsAWBRecords.Tables.Count > 0 && dsAWBRecords.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsAWBRecords.Tables[0].Rows.Count; i++)
                    {
                        await GenerateXFSUMessageofTheAWB(Convert.ToString(dsAWBRecords.Tables[0].Rows[i]["AWBPrefix"]), Convert.ToString(dsAWBRecords.Tables[0].Rows[i]["AWBNumber"]), Convert.ToString(dsAWBRecords.Tables[0].Rows[i]["Status"]));
                        await _genericFunction.updateAWBStatusMSG(Convert.ToString(dsAWBRecords.Tables[0].Rows[i]["TID"]));
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }
        public async Task<string> GenerateXFSUMessageofTheAWBV3(string AWBPrefix, string AWBNumber, string orgDest,
         string messageType, string doNumber, string flightNo = "", string flightDate = "1900-01-01", int DLVpcs = 0, double DLVWt = 0.00, string EventDate = "1900-01-01")
        {
            StringBuilder sbgenerateXFSUMessage = new StringBuilder();

            try
            {
                DataSet dsxfsuMessage = new DataSet();
                dsxfsuMessage = await GetAWBRecordforGenerateXFSUMessage(AWBPrefix, AWBNumber, orgDest, messageType, doNumber, flightNo, flightDate, DLVpcs, DLVWt);

                if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 0 && dsxfsuMessage.Tables[0].Rows.Count > 0 && dsxfsuMessage.Tables[1].Rows.Count > 0)
                {
                    XmlDocument xmlXFSUV3 = new XmlDocument();

                    XmlSchema schema = new XmlSchema();
                    schema.Namespaces.Add("xsd", "http://www.w3.org/2001/XMLSchema");
                    schema.Namespaces.Add("rsm", "iata:statusmessage:1");
                    schema.Namespaces.Add("ccts", "urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2");
                    schema.Namespaces.Add("udt", "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8");
                    schema.Namespaces.Add("ram", "iata:datamodel:3");
                    schema.Namespaces.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    //schema.Namespaces.Add("schemaLocation", "iata:waybill:1 Waybill_1.xsd");
                    xmlXFSUV3.Schemas.Add(schema);

                    XmlElement StatusMessage = xmlXFSUV3.CreateElement("rsm:StatusMessage");
                    StatusMessage.SetAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");
                    StatusMessage.SetAttribute("xmlns:rsm", "iata:statusmessage:1");
                    StatusMessage.SetAttribute("xmlns:ccts", "urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2");
                    StatusMessage.SetAttribute("xmlns:udt", "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8");
                    StatusMessage.SetAttribute("xmlns:ram", "iata:datamodel:3");
                    StatusMessage.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    //StatusMessage.SetAttribute("xsi:schemaLocation", "iata:waybill:1 Waybill_1.xsd");
                    StatusMessage.SetAttribute("schemaLocation", "iata:statusmessage:1 StatusMessage_1.xsd");
                    xmlXFSUV3.AppendChild(StatusMessage);

                    #region MessageHeaderDocument

                    XmlElement MessageHeaderDocument = xmlXFSUV3.CreateElement("rsm:MessageHeaderDocument");
                    StatusMessage.AppendChild(MessageHeaderDocument);

                    XmlElement MessageHeaderDocument_ID = xmlXFSUV3.CreateElement("ram:ID");
                    MessageHeaderDocument_ID.InnerText = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["ReferenceNumber"]);
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_ID);

                    XmlElement MessageHeaderDocument_Name = xmlXFSUV3.CreateElement("ram:Name");
                    MessageHeaderDocument_Name.InnerText = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageName"]);
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_Name);

                    XmlElement MessageHeaderDocument_TypeCode = xmlXFSUV3.CreateElement("ram:TypeCode");
                    MessageHeaderDocument_TypeCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageTypeCode"]);
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_TypeCode);

                    XmlElement MessageHeaderDocument_IssueDateTime = xmlXFSUV3.CreateElement("ram:IssueDateTime");
                    MessageHeaderDocument_IssueDateTime.InnerText = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageCreatedDate"]);
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_IssueDateTime);

                    XmlElement MessageHeaderDocument_PurposeCode = xmlXFSUV3.CreateElement("ram:PurposeCode");
                    MessageHeaderDocument_PurposeCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["PurposeCode"]);
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_PurposeCode);

                    XmlElement MessageHeaderDocument_VersionID = xmlXFSUV3.CreateElement("ram:VersionID");
                    MessageHeaderDocument_VersionID.InnerText = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["VersionNumber"]);
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_VersionID);

                    XmlElement MessageHeaderDocument_ConversationID = xmlXFSUV3.CreateElement("ram:ConversationID");
                    MessageHeaderDocument_ConversationID.InnerText = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["ConversionID"]);
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_ConversationID);

                    XmlElement MessageHeaderDocument_SenderParty = xmlXFSUV3.CreateElement("ram:SenderParty");
                    //MessageHeaderDocument_SenderParty.InnerText = "";
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_SenderParty);

                    XmlElement MessageHeaderDocument_SenderParty_PrimaryID = xmlXFSUV3.CreateElement("ram:PrimaryID");
                    MessageHeaderDocument_SenderParty_PrimaryID.SetAttribute("schemeID", Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["SenderParty_PrimaryID"]));
                    MessageHeaderDocument_SenderParty_PrimaryID.InnerText = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["SenderParty_PrimaryIDText"]);
                    MessageHeaderDocument_SenderParty.AppendChild(MessageHeaderDocument_SenderParty_PrimaryID);


                    XmlElement MessageHeaderDocument_SenderParty_SenderQualifier = xmlXFSUV3.CreateElement("ram:SenderQualifier");
                    MessageHeaderDocument_SenderParty_SenderQualifier.InnerText = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["SenderQualifier"]);
                    MessageHeaderDocument_SenderParty.AppendChild(MessageHeaderDocument_SenderParty_SenderQualifier);

                    XmlElement MessageHeaderDocument_SenderParty_SenderIdentification = xmlXFSUV3.CreateElement("ram:SenderIdentification");
                    MessageHeaderDocument_SenderParty_SenderIdentification.InnerText = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["SenderIdentification"]);
                    MessageHeaderDocument_SenderParty.AppendChild(MessageHeaderDocument_SenderParty_SenderIdentification);

                    XmlElement MessageHeaderDocument_RecipientParty = xmlXFSUV3.CreateElement("ram:RecipientParty");
                    //MessageHeaderDocument_RecipientParty.InnerText = "";
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_RecipientParty);

                    XmlElement MessageHeaderDocument_RecipientParty_PrimaryID = xmlXFSUV3.CreateElement("ram:PrimaryID");
                    MessageHeaderDocument_RecipientParty_PrimaryID.SetAttribute("schemeID", Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["RecipientParty_PrimaryID"]));
                    MessageHeaderDocument_RecipientParty_PrimaryID.InnerText = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["RecipientParty_PrimaryIDText"]);
                    MessageHeaderDocument_RecipientParty.AppendChild(MessageHeaderDocument_RecipientParty_PrimaryID);

                    XmlElement MessageHeaderDocument_RecipientParty_RecipientQualifier = xmlXFSUV3.CreateElement("ram:RecipientQualifier");
                    MessageHeaderDocument_RecipientParty_RecipientQualifier.InnerText = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["RecipientQualifier"]);
                    MessageHeaderDocument_RecipientParty.AppendChild(MessageHeaderDocument_RecipientParty_RecipientQualifier);

                    XmlElement MessageHeaderDocument_RecipientParty_RecipientIdentification = xmlXFSUV3.CreateElement("ram:RecipientIdentification");
                    MessageHeaderDocument_RecipientParty_RecipientIdentification.InnerText = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["RecipientIdentification"]);
                    MessageHeaderDocument_RecipientParty.AppendChild(MessageHeaderDocument_RecipientParty_RecipientIdentification);


                    #endregion MessageHeaderDocument

                    #region BusinessHeaderDocument
                    XmlElement BusinessHeaderDocument = xmlXFSUV3.CreateElement("rsm:BusinessHeaderDocument");
                    StatusMessage.AppendChild(BusinessHeaderDocument);

                    XmlElement BusinessHeaderDocument_ID = xmlXFSUV3.CreateElement("ram:ID");
                    BusinessHeaderDocument_ID.InnerText = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["NoofStatusUpdate"]);
                    BusinessHeaderDocument.AppendChild(BusinessHeaderDocument_ID);

                    XmlElement BusinessHeaderDocument_Reference = xmlXFSUV3.CreateElement("ram:Reference");
                    BusinessHeaderDocument_Reference.InnerText = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["Reference"]);
                    BusinessHeaderDocument.AppendChild(BusinessHeaderDocument_Reference);
                    #endregion



                    if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 1 && dsxfsuMessage.Tables[1].Rows.Count > 0)
                    {
                        #region MasterConsignment
                        XmlElement MasterConsignment = xmlXFSUV3.CreateElement("rsm:MasterConsignment");
                        StatusMessage.AppendChild(MasterConsignment);

                        XmlElement MC_GrossWeightMeasure = xmlXFSUV3.CreateElement("ram:GrossWeightMeasure");
                        MC_GrossWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedUOM"]));
                        MC_GrossWeightMeasure.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedWeight"]);
                        MasterConsignment.AppendChild(MC_GrossWeightMeasure);

                        XmlElement MC_TotalGrossWeightMeasure = xmlXFSUV3.CreateElement("ram:TotalGrossWeightMeasure");
                        MC_TotalGrossWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBUnitGrossWeight"]));
                        MC_TotalGrossWeightMeasure.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBGrossWeight"]);
                        MasterConsignment.AppendChild(MC_TotalGrossWeightMeasure);

                        XmlElement MC_PieceQuantity = xmlXFSUV3.CreateElement("ram:PieceQuantity");
                        MC_PieceQuantity.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedPieces"]);
                        MasterConsignment.AppendChild(MC_PieceQuantity);

                        XmlElement MC_TotalPieceQuantity = xmlXFSUV3.CreateElement("ram:TotalPieceQuantity");
                        MC_TotalPieceQuantity.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBPieces"]);
                        MasterConsignment.AppendChild(MC_TotalPieceQuantity);

                        XmlElement MC_TransportSplitDescription = xmlXFSUV3.CreateElement("ram:TransportSplitDescription");
                        MC_TransportSplitDescription.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["PartConsignment"]);
                        MasterConsignment.AppendChild(MC_TransportSplitDescription);

                        XmlElement MC_TransportContractDocument = xmlXFSUV3.CreateElement("ram:TransportContractDocument");
                        MasterConsignment.AppendChild(MC_TransportContractDocument);

                        XmlElement MC_TransportSplitDescription_ID = xmlXFSUV3.CreateElement("ram:ID");
                        MC_TransportSplitDescription_ID.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBNumber"]);
                        MC_TransportContractDocument.AppendChild(MC_TransportSplitDescription_ID);

                        XmlElement MC_TransportSplitDescription_Name = xmlXFSUV3.CreateElement("ram:Name");
                        MC_TransportSplitDescription_Name.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AirwayBillDocumentName"]);
                        MC_TransportContractDocument.AppendChild(MC_TransportSplitDescription_Name);

                        XmlElement MC_TransportSplitDescription_TypeCode = xmlXFSUV3.CreateElement("ram:TypeCode");
                        MC_TransportSplitDescription_TypeCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBDocumentTypeCode"]);
                        MC_TransportContractDocument.AppendChild(MC_TransportSplitDescription_TypeCode);

                        //OriginLocation
                        XmlElement MC_OriginLocation = xmlXFSUV3.CreateElement("ram:OriginLocation");
                        MasterConsignment.AppendChild(MC_OriginLocation);

                        XmlElement MC_OriginLocation_ID = xmlXFSUV3.CreateElement("ram:ID");
                        MC_OriginLocation_ID.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBOrigin"]);
                        MC_OriginLocation.AppendChild(MC_OriginLocation_ID);

                        XmlElement MC_OriginLocation_Name = xmlXFSUV3.CreateElement("ram:Name");
                        //MC_OriginLocation_Name.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["OriginAirportName"]);
                        XmlCDataSection cdata_MC_OriginLocation_Name = xmlXFSUV3.CreateCDataSection(Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["OriginAirportName"]));
                        MC_OriginLocation_Name.AppendChild(cdata_MC_OriginLocation_Name);
                        MC_OriginLocation.AppendChild(MC_OriginLocation_Name);

                        // FinalDestinationLocation
                        XmlElement MC_FinalDestinationLocation = xmlXFSUV3.CreateElement("ram:FinalDestinationLocation");
                        MasterConsignment.AppendChild(MC_FinalDestinationLocation);

                        XmlElement MC_FinalDestinationLocation_ID = xmlXFSUV3.CreateElement("ram:ID");
                        MC_FinalDestinationLocation_ID.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWbDestination"]);
                        MC_FinalDestinationLocation.AppendChild(MC_FinalDestinationLocation_ID);

                        XmlElement MC_FinalDestinationLocation_Name = xmlXFSUV3.CreateElement("ram:Name");
                        //MC_FinalDestinationLocation_Name.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DestinationAirportName"]);
                        XmlCDataSection cdata_MC_FinalDestinationLocation_Name = xmlXFSUV3.CreateCDataSection(Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DestinationAirportName"]));
                        MC_FinalDestinationLocation_Name.AppendChild(cdata_MC_FinalDestinationLocation_Name);
                        MC_FinalDestinationLocation.AppendChild(MC_FinalDestinationLocation_Name);

                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 3 && dsxfsuMessage.Tables[3].Rows.Count > 0)
                        {//LOOP
                         //RoutingLocation
                            XmlElement MC_RoutingLocation = xmlXFSUV3.CreateElement("ram:RoutingLocation");
                            MasterConsignment.AppendChild(MC_RoutingLocation);

                            XmlElement MC_RoutingLocation_ID = xmlXFSUV3.CreateElement("ram:ID");
                            MC_RoutingLocation_ID.InnerText = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FltDestination"]);
                            MC_RoutingLocation.AppendChild(MC_RoutingLocation_ID);

                            XmlElement MC_RoutingLocation_Name = xmlXFSUV3.CreateElement("ram:Name");
                            //MC_RoutingLocation_Name.InnerText = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["AirportName"]);
                            XmlCDataSection cdata_MC_RoutingLocation_Name = xmlXFSUV3.CreateCDataSection(Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["AirportName"]));
                            MC_RoutingLocation_Name.AppendChild(cdata_MC_RoutingLocation_Name);
                            MC_RoutingLocation.AppendChild(MC_RoutingLocation_Name);
                        }

                        #region ReportedStatus
                        //ReportedStatus
                        XmlElement MC_ReportedStatus = xmlXFSUV3.CreateElement("ram:ReportedStatus");
                        MasterConsignment.AppendChild(MC_ReportedStatus);

                        XmlElement ReasonCode = xmlXFSUV3.CreateElement("ram:ReasonCode");
                        ReasonCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["MessageCode"]);
                        MC_ReportedStatus.AppendChild(ReasonCode);

                        #region AssociatedStatusConsignment
                        //AssociatedStatusConsignment
                        XmlElement RS_AssociatedStatusConsignment = xmlXFSUV3.CreateElement("ram:AssociatedStatusConsignment");
                        MC_ReportedStatus.AppendChild(RS_AssociatedStatusConsignment);

                        XmlElement AssocStatusCons_GrossWeightMeasure = xmlXFSUV3.CreateElement("ram:GrossWeightMeasure");
                        AssocStatusCons_GrossWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DeliveredUOM"]));
                        AssocStatusCons_GrossWeightMeasure.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DeliveredWeight"]);
                        RS_AssociatedStatusConsignment.AppendChild(AssocStatusCons_GrossWeightMeasure);

                        if (dsxfsuMessage.Tables[1].Rows[0]["NoOfPosition"].ToString() == "2.00")
                        {
                            XmlElement AssocStatusCons_GrossVolumeMeasure = xmlXFSUV3.CreateElement("ram:GrossVolumeMeasure");
                            AssocStatusCons_GrossVolumeMeasure.SetAttribute("unitCode", Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBVolumeMeasue"]));
                            AssocStatusCons_GrossVolumeMeasure.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBVolume"]);
                            RS_AssociatedStatusConsignment.AppendChild(AssocStatusCons_GrossVolumeMeasure);
                        }
                        else
                        {
                            XmlElement AssociatedStatusConsignment_GrossVolumeMeasure = xmlXFSUV3.CreateElement("ram:GrossVolumeMeasure");
                            AssociatedStatusConsignment_GrossVolumeMeasure.SetAttribute("unitCode", Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["NoOfPosition_text"]));
                            AssociatedStatusConsignment_GrossVolumeMeasure.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["NoOfPosition"]);
                            RS_AssociatedStatusConsignment.AppendChild(AssociatedStatusConsignment_GrossVolumeMeasure);
                        }

                        XmlElement AssocStatusCons_DensityGroupCode = xmlXFSUV3.CreateElement("ram:DensityGroupCode");
                        AssocStatusCons_DensityGroupCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DensityCode"]);
                        RS_AssociatedStatusConsignment.AppendChild(AssocStatusCons_DensityGroupCode);

                        XmlElement AssocStatusCons_PieceQuantity = xmlXFSUV3.CreateElement("ram:PieceQuantity");
                        AssocStatusCons_PieceQuantity.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DeliveredPieces"]);
                        RS_AssociatedStatusConsignment.AppendChild(AssocStatusCons_PieceQuantity);

                        XmlElement AssocStatusCons_TransportSplitDescription = xmlXFSUV3.CreateElement("ram:TransportSplitDescription");
                        AssocStatusCons_TransportSplitDescription.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["PartConsignment"]);
                        RS_AssociatedStatusConsignment.AppendChild(AssocStatusCons_TransportSplitDescription);

                        XmlElement AssocStatusCons_DiscrepancyDescriptionCode = xmlXFSUV3.CreateElement("ram:DiscrepancyDescriptionCode");
                        AssocStatusCons_DiscrepancyDescriptionCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBDiscrepancyCode"]);
                        RS_AssociatedStatusConsignment.AppendChild(AssocStatusCons_DiscrepancyDescriptionCode);

                        XmlElement AssocStatusCons_StatusDescription = xmlXFSUV3.CreateElement("ram:StatusDescription");
                        AssocStatusCons_StatusDescription.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["StatusDescription"]);
                        RS_AssociatedStatusConsignment.AppendChild(AssocStatusCons_StatusDescription);

                        //AssociatedManifestDocument
                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 2 && dsxfsuMessage.Tables[2].Rows.Count > 0)
                        {

                            XmlElement AStatusCons_AssociatedManifestDocument = xmlXFSUV3.CreateElement("ram:AssociatedManifestDocument");
                            RS_AssociatedStatusConsignment.AppendChild(AStatusCons_AssociatedManifestDocument);

                            XmlElement AssociatedManifestDocument_ID = xmlXFSUV3.CreateElement("ram:ID");
                            AssociatedManifestDocument_ID.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ManifestNumber"]);
                            AStatusCons_AssociatedManifestDocument.AppendChild(AssociatedManifestDocument_ID);
                            //ApplicableLogisticsServiceCharge
                            XmlElement AStatusCons_ApplicableLogisticsServiceCharge = xmlXFSUV3.CreateElement("ram:ApplicableLogisticsServiceCharge");
                            RS_AssociatedStatusConsignment.AppendChild(AStatusCons_ApplicableLogisticsServiceCharge);

                            XmlElement ApplicableLogisticsServiceCharge_ServiceTypeCode = xmlXFSUV3.CreateElement("ram:ServiceTypeCode");
                            ApplicableLogisticsServiceCharge_ServiceTypeCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ProductType"]);
                            AStatusCons_ApplicableLogisticsServiceCharge.AppendChild(ApplicableLogisticsServiceCharge_ServiceTypeCode);

                            #region SpecifiedLogisticsTransportMovement
                            for (int j = 0; j < dsxfsuMessage.Tables[2].Rows.Count; j++)
                            {
                                XmlElement SpecifiedLogisticsTransportMovement = xmlXFSUV3.CreateElement("ram:SpecifiedLogisticsTransportMovement");
                                RS_AssociatedStatusConsignment.AppendChild(SpecifiedLogisticsTransportMovement);

                                XmlElement SpecLogTransMov_StageCode = xmlXFSUV3.CreateElement("ram:StageCode");
                                SpecLogTransMov_StageCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["ModeOfTransport"]);
                                SpecifiedLogisticsTransportMovement.AppendChild(SpecLogTransMov_StageCode);
                                XmlElement SpecLogTransMov_ID = xmlXFSUV3.CreateElement("ram:ID");
                                SpecLogTransMov_ID.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["FlightNumber"]);
                                SpecifiedLogisticsTransportMovement.AppendChild(SpecLogTransMov_ID);


                                XmlElement SpecLogTransMov_Squencenumber = xmlXFSUV3.CreateElement("ram:SequenceNumeric");
                                SpecLogTransMov_Squencenumber.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["SpecifiedLogisticsTransportMovement_SeqNum"]);
                                SpecifiedLogisticsTransportMovement.AppendChild(SpecLogTransMov_Squencenumber);

                                XmlElement SLTMov_UsedLogisticsTransportMeans = xmlXFSUV3.CreateElement("ram:UsedLogisticsTransportMeans");
                                SpecifiedLogisticsTransportMovement.AppendChild(SLTMov_UsedLogisticsTransportMeans);

                                XmlElement SLTMov_UsedLogisticsTransportMeans_Name = xmlXFSUV3.CreateElement("ram:Name");
                                // SLTMov_UsedLogisticsTransportMeans_Name.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["CarrierName"]);
                                XmlCDataSection cdata_SLTMov_UsedLogisticsTransportMeans_Name = xmlXFSUV3.CreateCDataSection(Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["CarrierName"]));
                                SLTMov_UsedLogisticsTransportMeans_Name.AppendChild(cdata_SLTMov_UsedLogisticsTransportMeans_Name);
                                SLTMov_UsedLogisticsTransportMeans.AppendChild(SLTMov_UsedLogisticsTransportMeans_Name);

                                XmlElement SLTMov_UsedLogisticsTransportMeans_Type = xmlXFSUV3.CreateElement("ram:Type");
                                SLTMov_UsedLogisticsTransportMeans_Type.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["AirCraftTypeMaster"]);
                                SLTMov_UsedLogisticsTransportMeans.AppendChild(SLTMov_UsedLogisticsTransportMeans_Type);

                                XmlElement SLTMov_ScheduledArrivalEvent = xmlXFSUV3.CreateElement("ram:ScheduledArrivalEvent");
                                SpecifiedLogisticsTransportMovement.AppendChild(SLTMov_ScheduledArrivalEvent);

                                XmlElement SLTMov_ScheduledArrivalEvent_ScheduledOccurrenceDateTime = xmlXFSUV3.CreateElement("ram:ScheduledOccurrenceDateTime");
                                SLTMov_ScheduledArrivalEvent_ScheduledOccurrenceDateTime.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["FlightScheduleArrivlaTime"]);
                                SLTMov_ScheduledArrivalEvent.AppendChild(SLTMov_ScheduledArrivalEvent_ScheduledOccurrenceDateTime);

                                XmlElement SLTMov_ArrivalEvent = xmlXFSUV3.CreateElement("ram:ArrivalEvent");
                                SpecifiedLogisticsTransportMovement.AppendChild(SLTMov_ArrivalEvent);

                                XmlElement SLTMov_ArrivalEvent_ArrivalOccurrenceDateTime = xmlXFSUV3.CreateElement("ram:ArrivalOccurrenceDateTime");
                                SLTMov_ArrivalEvent_ArrivalOccurrenceDateTime.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["FlightArriveTime"]);
                                SLTMov_ArrivalEvent.AppendChild(SLTMov_ArrivalEvent_ArrivalOccurrenceDateTime);

                                XmlElement SLTMov_ArrivalEvent_ArrivalDateTimeTypeCode = xmlXFSUV3.CreateElement("ram:ArrivalDateTimeTypeCode");
                                SLTMov_ArrivalEvent_ArrivalDateTimeTypeCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["ScheduleArrivalIndicator"]);
                                SLTMov_ArrivalEvent.AppendChild(SLTMov_ArrivalEvent_ArrivalDateTimeTypeCode);

                                XmlElement SLTMov_ArrivalEvent_ScheduledOccurrenceDateTime = xmlXFSUV3.CreateElement("ram:ScheduledOccurrenceDateTime");
                                SLTMov_ArrivalEvent_ScheduledOccurrenceDateTime.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["FlightArriveTime"]);
                                SLTMov_ArrivalEvent.AppendChild(SLTMov_ArrivalEvent_ScheduledOccurrenceDateTime);


                                XmlElement ArrivalEvent_OccurrenceArrivalLocation = xmlXFSUV3.CreateElement("ram:OccurrenceArrivalLocation");
                                SLTMov_ArrivalEvent.AppendChild(ArrivalEvent_OccurrenceArrivalLocation);

                                XmlElement OccurrenceArrivalLocation_ID = xmlXFSUV3.CreateElement("ram:ID");
                                OccurrenceArrivalLocation_ID.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["FltDestination"]);
                                ArrivalEvent_OccurrenceArrivalLocation.AppendChild(OccurrenceArrivalLocation_ID);
                                XmlElement SLTMov_DepartureEvent = xmlXFSUV3.CreateElement("ram:DepartureEvent");
                                SpecifiedLogisticsTransportMovement.AppendChild(SLTMov_DepartureEvent);

                                XmlElement SLTMov_DepartureEvent_DepartureOccurrenceDateTime = xmlXFSUV3.CreateElement("ram:DepartureOccurrenceDateTime");
                                SLTMov_DepartureEvent_DepartureOccurrenceDateTime.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["FlightScheduleDepartureTime"]);
                                SLTMov_DepartureEvent.AppendChild(SLTMov_DepartureEvent_DepartureOccurrenceDateTime);

                                XmlElement SLTMov_DepartureEvent_DepartureDateTimeTypeCode = xmlXFSUV3.CreateElement("ram:DepartureDateTimeTypeCode");
                                SLTMov_DepartureEvent_DepartureDateTimeTypeCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["EstimatedDepTime"]);
                                SLTMov_DepartureEvent.AppendChild(SLTMov_DepartureEvent_DepartureDateTimeTypeCode);
                                XmlElement SLTMov_DepartureEvent_ScheduledOccurrenceDateTime = xmlXFSUV3.CreateElement("ram:ScheduledOccurrenceDateTime");
                                SLTMov_DepartureEvent_ScheduledOccurrenceDateTime.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["FlightScheduleDepartureTime"]);
                                SLTMov_DepartureEvent.AppendChild(SLTMov_DepartureEvent_ScheduledOccurrenceDateTime);

                                XmlElement DepartureEvent_OccurrenceDepartureLocation = xmlXFSUV3.CreateElement("ram:OccurrenceDepartureLocation");
                                SLTMov_DepartureEvent.AppendChild(DepartureEvent_OccurrenceDepartureLocation);

                                XmlElement OccurrenceDepartureLocation_ID = xmlXFSUV3.CreateElement("ram:ID");
                                OccurrenceDepartureLocation_ID.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["FltOrigin"]);
                                DepartureEvent_OccurrenceDepartureLocation.AppendChild(OccurrenceDepartureLocation_ID);

                                XmlElement SLTMov_CarrierParty = xmlXFSUV3.CreateElement("ram:CarrierParty");
                                SpecifiedLogisticsTransportMovement.AppendChild(SLTMov_CarrierParty);

                                XmlElement SLTMov_CarrierParty_PrimaryID = xmlXFSUV3.CreateElement("ram:PrimaryID");
                                SLTMov_CarrierParty_PrimaryID.SetAttribute("schemeAgencyID", Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["CarrierCode"]));
                                SLTMov_CarrierParty_PrimaryID.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["CarrierName"]);
                                SLTMov_CarrierParty.AppendChild(SLTMov_CarrierParty_PrimaryID);

                                //SpecifiedLocation
                                XmlElement SLTMov_SpecifiedLocation = xmlXFSUV3.CreateElement("ram:SpecifiedLocation");
                                SpecifiedLogisticsTransportMovement.AppendChild(SLTMov_SpecifiedLocation);

                                XmlElement SLTMov_SpecifiedLocation_ID = xmlXFSUV3.CreateElement("ram:ID");
                                SLTMov_SpecifiedLocation_ID.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["FlightDestlocation"]);
                                SLTMov_SpecifiedLocation.AppendChild(SLTMov_SpecifiedLocation_ID);

                                XmlElement SLTMov_SpecifiedLocation_Name = xmlXFSUV3.CreateElement("ram:Name");
                                // SLTMov_SpecifiedLocation_Name.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightLocationAirportName"]);
                                XmlCDataSection cdata_SLTMov_SpecifiedLocation_Name = xmlXFSUV3.CreateCDataSection(Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["FlightLocationAirportName"]));
                                SLTMov_SpecifiedLocation_Name.AppendChild(cdata_SLTMov_SpecifiedLocation_Name);
                                SLTMov_SpecifiedLocation.AppendChild(SLTMov_SpecifiedLocation_Name);

                                XmlElement SLTMov_SpecifiedLocation_TypeCode = xmlXFSUV3.CreateElement("ram:TypeCode");
                                SLTMov_SpecifiedLocation_TypeCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["LocationType"]);
                                SLTMov_SpecifiedLocation.AppendChild(SLTMov_SpecifiedLocation_TypeCode);

                                XmlElement SLTMov_SpecifiedLocation_FlightStatusTypeCode = xmlXFSUV3.CreateElement("ram:FlightStatusTypeCode");
                                SLTMov_SpecifiedLocation_FlightStatusTypeCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["FlightStatusTypeCode"]);
                                SLTMov_SpecifiedLocation.AppendChild(SLTMov_SpecifiedLocation_FlightStatusTypeCode);

                                //SpecifiedEvent
                                XmlElement SLTMov_SpecifiedEvent = xmlXFSUV3.CreateElement("ram:SpecifiedEvent");
                                SpecifiedLogisticsTransportMovement.AppendChild(SLTMov_SpecifiedEvent);

                                XmlElement SLTMov_SpecifiedEvent_OccurrenceDateTime = xmlXFSUV3.CreateElement("ram:OccurrenceDateTime");
                                //SLTMov_SpecifiedEvent_OccurrenceDateTime.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["SpecifiedEvent"]);
                                SLTMov_SpecifiedEvent_OccurrenceDateTime.InnerText = Convert.ToString(EventDate);
                                SLTMov_SpecifiedEvent.AppendChild(SLTMov_SpecifiedEvent_OccurrenceDateTime);

                                XmlElement SLTMov_SpecifiedEvent_DateTimeTypeCode = xmlXFSUV3.CreateElement("ram:DateTimeTypeCode");
                                SLTMov_SpecifiedEvent_DateTimeTypeCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["DateTimeTypeCode"]);
                                SLTMov_SpecifiedEvent.AppendChild(SLTMov_SpecifiedEvent_DateTimeTypeCode);

                                XmlElement SLTMov_SpecifiedEvent_Extra = xmlXFSUV3.CreateElement("ram:SpecifiedEvent");
                                SpecifiedLogisticsTransportMovement.AppendChild(SLTMov_SpecifiedEvent_Extra);

                                XmlElement SLTMov_SpecifiedEvent_OccurrenceDateTime_Extra = xmlXFSUV3.CreateElement("ram:OccurrenceDateTime");
                                SLTMov_SpecifiedEvent_OccurrenceDateTime_Extra.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["SpecifiedEvent_NewDate"]);
                                SLTMov_SpecifiedEvent_Extra.AppendChild(SLTMov_SpecifiedEvent_OccurrenceDateTime_Extra);

                                XmlElement SLTMov_SpecifiedEvent_DateTimeTypeCode_extra = xmlXFSUV3.CreateElement("ram:DateTimeTypeCode");
                                SLTMov_SpecifiedEvent_DateTimeTypeCode_extra.InnerText = Convert.ToString(dsxfsuMessage.Tables[2].Rows[j]["SpecifiedEvent_NewTypecode"]);
                                SLTMov_SpecifiedEvent_Extra.AppendChild(SLTMov_SpecifiedEvent_DateTimeTypeCode_extra);

                                #endregion
                            }
                        }

                        XmlElement NotifiedParty = xmlXFSUV3.CreateElement("ram:NotifiedParty");
                        RS_AssociatedStatusConsignment.AppendChild(NotifiedParty);

                        XmlElement NotifiedParty_Name = xmlXFSUV3.CreateElement("ram:Name");
                        //NotifiedParty_Name.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["NotifyName"]);
                        XmlCDataSection cdata_NotifiedParty_Name = xmlXFSUV3.CreateCDataSection(Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["NotifyName"]));
                        NotifiedParty_Name.AppendChild(cdata_NotifiedParty_Name);
                        NotifiedParty.AppendChild(NotifiedParty_Name);

                        XmlElement DeliveryParty = xmlXFSUV3.CreateElement("ram:DeliveryParty");
                        RS_AssociatedStatusConsignment.AppendChild(DeliveryParty);

                        XmlElement DeliveryParty_Name = xmlXFSUV3.CreateElement("ram:Name");
                        //DeliveryParty_Name.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["ConsigneeName"]);
                        XmlCDataSection cdata_DeliveryParty_Name = xmlXFSUV3.CreateCDataSection(Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["ConsigneeName"]));
                        DeliveryParty_Name.AppendChild(cdata_DeliveryParty_Name);
                        DeliveryParty.AppendChild(DeliveryParty_Name);

                        //AssociatedReceivedFromParty
                        XmlElement AssociatedReceivedFromParty = xmlXFSUV3.CreateElement("ram:AssociatedReceivedFromParty");
                        RS_AssociatedStatusConsignment.AppendChild(AssociatedReceivedFromParty);

                        XmlElement AssociatedReceivedFromParty_PrimaryID = xmlXFSUV3.CreateElement("ram:PrimaryID");
                        AssociatedReceivedFromParty_PrimaryID.SetAttribute("schemeAgencyID", Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_schemeAgencyID"]));
                        AssociatedReceivedFromParty_PrimaryID.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_PrimaryID_Text"]);
                        AssociatedReceivedFromParty.AppendChild(AssociatedReceivedFromParty_PrimaryID);

                        XmlElement AssociatedReceivedFromParty_Name = xmlXFSUV3.CreateElement("ram:Name");
                        AssociatedReceivedFromParty_Name.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssociatedReceivedAirlineName"]);
                        AssociatedReceivedFromParty.AppendChild(AssociatedReceivedFromParty_Name);

                        XmlElement AssociatedReceivedFromParty_RoleCode = xmlXFSUV3.CreateElement("ram:RoleCode");
                        AssociatedReceivedFromParty_RoleCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_RoleCode"]);
                        AssociatedReceivedFromParty.AppendChild(AssociatedReceivedFromParty_RoleCode);

                        XmlElement AssociatedReceivedFromParty_Role = xmlXFSUV3.CreateElement("ram:Role");
                        AssociatedReceivedFromParty_Role.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_Role"]);
                        AssociatedReceivedFromParty.AppendChild(AssociatedReceivedFromParty_Role);

                        //AssociatedTransferredFromParty
                        XmlElement AssociatedTransferredFromParty = xmlXFSUV3.CreateElement("ram:AssociatedTransferredFromParty");
                        RS_AssociatedStatusConsignment.AppendChild(AssociatedTransferredFromParty);

                        XmlElement AssociatedTransferredFromParty_PrimaryID = xmlXFSUV3.CreateElement("ram:PrimaryID");
                        AssociatedTransferredFromParty_PrimaryID.SetAttribute("schemeAgencyID", Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_schemeAgencyID"]));
                        AssociatedTransferredFromParty_PrimaryID.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_PrimaryID_Text"]);
                        AssociatedTransferredFromParty.AppendChild(AssociatedTransferredFromParty_PrimaryID);

                        XmlElement AssociatedTransferredFromParty_Name = xmlXFSUV3.CreateElement("ram:Name");
                        AssociatedTransferredFromParty_Name.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_Name"]);
                        AssociatedTransferredFromParty.AppendChild(AssociatedTransferredFromParty_Name);

                        XmlElement AssociatedTransferredFromParty_RoleCode = xmlXFSUV3.CreateElement("ram:RoleCode");
                        AssociatedTransferredFromParty_RoleCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_RoleCode"]);
                        AssociatedTransferredFromParty.AppendChild(AssociatedTransferredFromParty_RoleCode);

                        XmlElement AssociatedTransferredFromParty_Role = xmlXFSUV3.CreateElement("ram:Role");
                        AssociatedTransferredFromParty_Role.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_Role"]);
                        AssociatedTransferredFromParty.AppendChild(AssociatedTransferredFromParty_Role);

                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 2 && dsxfsuMessage.Tables[8].Rows.Count > 0)
                        {
                            for (int j = 0; j < dsxfsuMessage.Tables[8].Rows.Count; j++)
                            {
                                //HandlingOSIInstructions
                                XmlElement HandlingInstructions = xmlXFSUV3.CreateElement("ram:HandlingInstructions");
                                RS_AssociatedStatusConsignment.AppendChild(HandlingInstructions);

                                XmlElement HandlingInstructions_Description = xmlXFSUV3.CreateElement("ram:Description");
                                // HandlingOSIInstructions_Description.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["HandlingInfo"]);
                                XmlCDataSection cdata_ConsP_EURIID_Description = xmlXFSUV3.CreateCDataSection(Convert.ToString(dsxfsuMessage.Tables[8].Rows[j]["HandlingOSIInstructions_Description"]));
                                HandlingInstructions_Description.AppendChild(cdata_ConsP_EURIID_Description);
                                HandlingInstructions.AppendChild(HandlingInstructions_Description);

                                XmlElement HandlingInstructions_DescriptionCode = xmlXFSUV3.CreateElement("ram:DescriptionCode");
                                HandlingInstructions_DescriptionCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[8].Rows[j]["HandlingInstructions_Code"]);
                                HandlingInstructions.AppendChild(HandlingInstructions_DescriptionCode);
                            }
                        }

                        //HandlingOSIInstructions
                        XmlElement HandlingOSIInstructions = xmlXFSUV3.CreateElement("ram:HandlingOSIInstructions");
                        RS_AssociatedStatusConsignment.AppendChild(HandlingOSIInstructions);

                        XmlElement HandlingOSIInstructions_Description = xmlXFSUV3.CreateElement("ram:Description");
                        // HandlingOSIInstructions_Description.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["HandlingInfo"]);
                        XmlCDataSection cdata_ConsP_EURIID = xmlXFSUV3.CreateCDataSection(Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["HandlingInfo"]));
                        HandlingOSIInstructions_Description.AppendChild(cdata_ConsP_EURIID);
                        HandlingOSIInstructions.AppendChild(HandlingOSIInstructions_Description);

                        XmlElement HandlingOSIInstructions_DescriptionCode = xmlXFSUV3.CreateElement("ram:DescriptionCode");
                        HandlingOSIInstructions_DescriptionCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["SHCCodes"]);
                        HandlingOSIInstructions.AppendChild(HandlingOSIInstructions_DescriptionCode);

                        //IncludedCustomsNote
                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 4 && dsxfsuMessage.Tables[4].Rows.Count > 0)
                        {
                            XmlElement IncludedCustomsNote = xmlXFSUV3.CreateElement("ram:IncludedCustomsNote");
                            RS_AssociatedStatusConsignment.AppendChild(IncludedCustomsNote);

                            XmlElement IncludedCustomsNote_ContentCode = xmlXFSUV3.CreateElement("ram:ContentCode");
                            IncludedCustomsNote_ContentCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["ContentCode"]);
                            IncludedCustomsNote.AppendChild(IncludedCustomsNote_ContentCode);

                            XmlElement IncludedCustomsNote_Content = xmlXFSUV3.CreateElement("ram:Content");
                            IncludedCustomsNote_Content.InnerText = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["Content"]);
                            IncludedCustomsNote.AppendChild(IncludedCustomsNote_Content);

                            XmlElement IncludedCustomsNote_SubjectCode = xmlXFSUV3.CreateElement("ram:SubjectCode");
                            IncludedCustomsNote_SubjectCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["SubjectCode"]);
                            IncludedCustomsNote.AppendChild(IncludedCustomsNote_SubjectCode);

                            XmlElement IncludedCustomsNote_CountryID = xmlXFSUV3.CreateElement("ram:CountryID");
                            IncludedCustomsNote_CountryID.InnerText = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["CountryID"]);
                            IncludedCustomsNote.AppendChild(IncludedCustomsNote_CountryID);


                            //IncludedCustomsNote
                            XmlElement AssociatedConsignmentCustomsProcedure = xmlXFSUV3.CreateElement("ram:AssociatedConsignmentCustomsProcedure");
                            RS_AssociatedStatusConsignment.AppendChild(AssociatedConsignmentCustomsProcedure);

                            XmlElement AssociatedConsignmentCustomsProcedure_GoodsStatusCode = xmlXFSUV3.CreateElement("ram:GoodsStatusCode");
                            AssociatedConsignmentCustomsProcedure_GoodsStatusCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["GoodsStatusCode"]);
                            AssociatedConsignmentCustomsProcedure.AppendChild(AssociatedConsignmentCustomsProcedure_GoodsStatusCode);
                        }

                        #region UtilizedUnitLoadTransportEquipment
                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 5 && dsxfsuMessage.Tables[5].Rows.Count > 0)
                        {
                            //UtilizedUnitLoadTransportEquipment
                            XmlElement UtilizedUnitLoadTransportEquipment = xmlXFSUV3.CreateElement("ram:UtilizedUnitLoadTransportEquipment");
                            RS_AssociatedStatusConsignment.AppendChild(UtilizedUnitLoadTransportEquipment);

                            XmlElement UtilizedUnitLoadTransportEquipment_ID = xmlXFSUV3.CreateElement("ram:ID");
                            UtilizedUnitLoadTransportEquipment_ID.InnerText = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FltDestination"]);
                            UtilizedUnitLoadTransportEquipment.AppendChild(UtilizedUnitLoadTransportEquipment_ID);

                            XmlElement UtilizedUnitLoadTransportEquipment_CharacteristicCode = xmlXFSUV3.CreateElement("ram:CharacteristicCode");
                            UtilizedUnitLoadTransportEquipment_CharacteristicCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FltDestination"]);
                            UtilizedUnitLoadTransportEquipment.AppendChild(UtilizedUnitLoadTransportEquipment_CharacteristicCode);

                            XmlElement UtilizedUnitLoadTransportEquipment_OperationalStatusCode = xmlXFSUV3.CreateElement("ram:OperationalStatusCode");
                            UtilizedUnitLoadTransportEquipment_OperationalStatusCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FltDestination"]);
                            UtilizedUnitLoadTransportEquipment.AppendChild(UtilizedUnitLoadTransportEquipment_OperationalStatusCode);

                            //OperatingParty
                            XmlElement UtilizedUnitLoadTransportEquipment_OperatingParty = xmlXFSUV3.CreateElement("ram:OperatingParty");
                            UtilizedUnitLoadTransportEquipment.AppendChild(UtilizedUnitLoadTransportEquipment_OperatingParty);

                            XmlElement OperatingParty_PrimaryID = xmlXFSUV3.CreateElement("ram:PrimaryID");
                            OperatingParty_PrimaryID.SetAttribute("schemeAgencyID", Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FltDestination"]));
                            OperatingParty_PrimaryID.InnerText = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FltDestination"]);
                            UtilizedUnitLoadTransportEquipment_OperatingParty.AppendChild(OperatingParty_PrimaryID);
                        }

                        //IncludedHouseConsignment
                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 6 && dsxfsuMessage.Tables[6].Rows.Count > 0)
                        {
                            XmlElement IncludedHouseConsignment = xmlXFSUV3.CreateElement("ram:IncludedHouseConsignment");
                            RS_AssociatedStatusConsignment.AppendChild(IncludedHouseConsignment);

                            XmlElement IncludedHouseConsignment_GrossWeightMeasure = xmlXFSUV3.CreateElement("ram:GrossWeightMeasure");
                            IncludedHouseConsignment_GrossWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["UnitofMeasurement"]));
                            IncludedHouseConsignment_GrossWeightMeasure.InnerText = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBWt"]);
                            IncludedHouseConsignment.AppendChild(IncludedHouseConsignment_GrossWeightMeasure);

                            XmlElement IncludedHouseConsignment_TotalGrossWeightMeasure = xmlXFSUV3.CreateElement("ram:TotalGrossWeightMeasure");
                            IncludedHouseConsignment_TotalGrossWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["AcceptedUOM"]));
                            IncludedHouseConsignment_TotalGrossWeightMeasure.InnerText = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["GrossWeight"]);
                            IncludedHouseConsignment.AppendChild(IncludedHouseConsignment_TotalGrossWeightMeasure);

                            XmlElement IncludedHouseConsignment_PieceQuantity = xmlXFSUV3.CreateElement("ram:PieceQuantity");
                            IncludedHouseConsignment_PieceQuantity.InnerText = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBPcs"]);
                            IncludedHouseConsignment.AppendChild(IncludedHouseConsignment_PieceQuantity);

                            XmlElement IncludedHouseConsignment_TotalPieceQuantity = xmlXFSUV3.CreateElement("ram:TotalPieceQuantity");
                            IncludedHouseConsignment_TotalPieceQuantity.InnerText = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["AWBPieces"]);
                            IncludedHouseConsignment.AppendChild(IncludedHouseConsignment_TotalPieceQuantity);

                            XmlElement IncludedHouseConsignment_TransportSplitDescription = xmlXFSUV3.CreateElement("ram:TransportSplitDescription");
                            IncludedHouseConsignment_TransportSplitDescription.InnerText = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["PiecesIndicator"]);
                            IncludedHouseConsignment.AppendChild(IncludedHouseConsignment_TransportSplitDescription);

                            //TransportContractDocument
                            XmlElement IncludedHouseConsignment_TransportContractDocument = xmlXFSUV3.CreateElement("ram:TransportContractDocument");
                            IncludedHouseConsignment.AppendChild(IncludedHouseConsignment_TransportContractDocument);

                            XmlElement TransportContractDocument_ID = xmlXFSUV3.CreateElement("ram:ID");
                            TransportContractDocument_ID.InnerText = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBNo"]);
                            IncludedHouseConsignment.AppendChild(TransportContractDocument_ID);

                            XmlElement TransportContractDocument_TypeCode = xmlXFSUV3.CreateElement("ram:TypeCode");
                            TransportContractDocument_TypeCode.InnerText = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWbTypeCode"]);
                            IncludedHouseConsignment.AppendChild(TransportContractDocument_TypeCode);
                        }
                        #endregion

                        //AssociatedPiece Extra Tag for Piece Reference
                        for (int i = 0; i < dsxfsuMessage.Tables[7].Rows.Count; i++)
                        {
                            XmlElement AssociatedPiece = xmlXFSUV3.CreateElement("ram:AssociatedPiece");
                            RS_AssociatedStatusConsignment.AppendChild(AssociatedPiece);

                            XmlElement AssociatedPiece_Reference = xmlXFSUV3.CreateElement("ram:Reference");
                            AssociatedPiece_Reference.InnerText = Convert.ToString(dsxfsuMessage.Tables[7].Rows[i]["Reference"]);
                            AssociatedPiece.AppendChild(AssociatedPiece_Reference);

                            XmlElement AssociatedPiece_Quantity = xmlXFSUV3.CreateElement("ram:Quantity");
                            AssociatedPiece_Quantity.InnerText = Convert.ToString(dsxfsuMessage.Tables[7].Rows[i]["Quantity"]);
                            AssociatedPiece.AppendChild(AssociatedPiece_Quantity);

                            XmlElement AssociatedPiece_Weight = xmlXFSUV3.CreateElement("ram:Weight");
                            AssociatedPiece_Weight.InnerText = Convert.ToString(dsxfsuMessage.Tables[7].Rows[i]["Weight"]);
                            AssociatedPiece.AppendChild(AssociatedPiece_Weight);
                        }

                        #endregion

                        #endregion


                        #endregion
                    }

                    sbgenerateXFSUMessage = new StringBuilder(xmlXFSUV3.OuterXml);
                    sbgenerateXFSUMessage.Replace("<", "<ram:");
                    sbgenerateXFSUMessage.Replace("<ram:/", "</ram:");
                    sbgenerateXFSUMessage.Replace("<ram:![CDATA", "<![CDATA");
                    sbgenerateXFSUMessage.Replace("<ram:StatusMessage", "<rsm:StatusMessage");
                    sbgenerateXFSUMessage.Replace("</ram:StatusMessage", "</rsm:StatusMessage");
                    sbgenerateXFSUMessage.Replace("<ram:MessageHeaderDocument>", "<rsm:MessageHeaderDocument>");
                    sbgenerateXFSUMessage.Replace("</ram:MessageHeaderDocument>", "</rsm:MessageHeaderDocument>");
                    sbgenerateXFSUMessage.Replace("<ram:BusinessHeaderDocument>", "<rsm:BusinessHeaderDocument>");
                    sbgenerateXFSUMessage.Replace("</ram:BusinessHeaderDocument>", "</rsm:BusinessHeaderDocument>");
                    sbgenerateXFSUMessage.Replace("<ram:MasterConsignment>", "<rsm:MasterConsignment>");
                    sbgenerateXFSUMessage.Replace("</ram:MasterConsignment>", "</rsm:MasterConsignment>");





                    ///Remove the empty tags from XML
                    var document = System.Xml.Linq.XDocument.Parse(sbgenerateXFSUMessage.ToString());
                    var emptyNodes = document.Descendants().Where(e => e.IsEmpty || String.IsNullOrWhiteSpace(e.Value));
                    foreach (var emptyNode in emptyNodes.ToArray())
                    {
                        emptyNode.Remove();
                    }
                    sbgenerateXFSUMessage = new StringBuilder(document.ToString());
                    sbgenerateXFSUMessage.Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
                    //XMLValidator objxMLValidator = new XMLValidator();
                    //string errormsg = objxMLValidator.CTeXMLValidator(sbgenerateXFSUMessage.ToString());
                    //if (errormsg.Length > 1)
                    //{
                    //    sbgenerateXFSUMessage.Clear();
                    //    sbgenerateXFSUMessage.Append(errormsg);
                    //}
                }

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }

            return Convert.ToString(sbgenerateXFSUMessage); ;
        }



        public async Task<DataSet> GetAWBRecordsToAutoSendXFSUMessage()
        {
            DataSet? dsAWBRecords = new DataSet();
            try
            {
                //SQLServer da = new SQLServer();
                //dsAWBRecords = da.SelectRecords("USPGetAWBRecordsToAutoSendXFSUMessage");
                dsAWBRecords = await _readWriteDao.SelectRecords("USPGetAWBRecordsToAutoSendXFSUMessage");

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                dsAWBRecords = null;
            }
            return dsAWBRecords;

        }

        /// <summary>
        /// generate xfsu 
        /// </summary>
        /// <param name="AWBPrefix"></param>
        /// <param name="AWBNumber"></param>
        /// <returns></returns>
        public async Task<string> GenerateXFSUMessageofTheAWB(string AWBPrefix, string AWBNumber, string EventStatus)
        {
            StringBuilder strGenerateXFSUMessage = new StringBuilder();


            try
            {
                DataSet dsxfsuMessage = new DataSet();
                dsxfsuMessage = await GetAWBDetailstoGenerateXFSUMessage(AWBPrefix, AWBNumber, EventStatus);
                //if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 0 && dsxfsuMessage.Tables[0].Columns.Count == 1)
                //{
                //    return strGenerateXFSUMessage.Append(Convert.ToString(dsxfsuMessage.Tables[0].Rows[0][""]));
                //    //  return;
                //}
                if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 0 && dsxfsuMessage.Tables[0].Rows.Count > 0)
                {
                    var xfsuDataSet = new DataSet();
                    DataRow[] drs;
                    var xmlSchema = await _genericFunction.GetXMLMessageData("XFSU");
                    if (xmlSchema != null && xmlSchema.Tables.Count > 0 && xmlSchema.Tables[0].Rows.Count > 0)
                    {
                        string messageXML = Convert.ToString(xmlSchema.Tables[0].Rows[0]["XMLMessageData"]);
                        messageXML = ReplacingNodeNames(messageXML);
                        var txMessage = new StringReader(messageXML);
                        xfsuDataSet.ReadXml(txMessage);

                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 0 && dsxfsuMessage.Tables[0].Rows.Count > 0)
                        {
                            xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["ReferenceNumber"]);
                            xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageName"]);
                            xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageTypeCode"]);
                            xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["IssueDateTime"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageCreatedDate"]);
                            xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["PurposeCode"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["PurposeCode"]);
                            xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["VersionNumber"]);
                            xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["ConversationID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["ConversionID"]);

                            //SenderParty
                            if (xfsuDataSet.Tables.Contains("PrimaryID"))
                            {
                                drs = xfsuDataSet.Tables["PrimaryID"].Select("SenderParty_Id=0");
                                if (drs.Length > 0)
                                {
                                    drs[0]["schemeID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["SenderParty_PrimaryID"]);
                                    drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["SenderParty_PrimaryIDText"]);
                                }
                            }
                            //RecipientParty
                            if (xfsuDataSet.Tables.Contains("PrimaryID"))
                            {
                                drs = xfsuDataSet.Tables["PrimaryID"].Select("RecipientParty_Id=0");
                                if (drs.Length > 0)
                                {
                                    drs[0]["schemeID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["RecipientParty_PrimaryID"]);
                                    drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["RecipientParty_PrimaryIDText"]);
                                }
                            }

                            //BusinessHeaderDocument
                            xfsuDataSet.Tables["BusinessHeaderDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["NoofStatusUpdate"]);

                            //MasterConsignment
                            if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 1 && dsxfsuMessage.Tables[1].Rows.Count > 0)
                            {
                                if (xfsuDataSet.Tables.Contains("MasterConsignment_GrossWeightMeasure"))
                                {
                                    drs = xfsuDataSet.Tables["MasterConsignment_GrossWeightMeasure"].Select("MasterConsignment_Id=0");
                                    if (drs.Length > 0)
                                    {
                                        drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedUOM"]);
                                        drs[0]["MasterConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedWeight"]);
                                    }
                                }

                                if (xfsuDataSet.Tables.Contains("TotalGrossWeightMeasure"))
                                {
                                    drs = xfsuDataSet.Tables["TotalGrossWeightMeasure"].Select("MasterConsignment_Id=0");
                                    if (drs.Length > 0)
                                    {
                                        drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBUnitGrossWeight"]);
                                        drs[0]["TotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBGrossWeight"]);
                                    }
                                }

                                xfsuDataSet.Tables["MasterConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedPieces"]);
                                xfsuDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBPieces"]);
                                xfsuDataSet.Tables["MasterConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["PartConsignment"]);

                                // AWBNumber Section
                                if (xfsuDataSet.Tables.Contains("TransportContractDocument"))
                                {
                                    drs = xfsuDataSet.Tables["TransportContractDocument"].Select("MasterConsignment_Id=0");
                                    if (drs.Length > 0)
                                    {
                                        drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBNumber"]);
                                        drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AiwayBillDocumentName"]);
                                        drs[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBDocumentTypeCode"]);
                                    }
                                }

                                // AWBNumber Origin Section
                                if (xfsuDataSet.Tables.Contains("OriginLocation"))
                                {
                                    drs = xfsuDataSet.Tables["OriginLocation"].Select("MasterConsignment_Id=0");
                                    if (drs.Length > 0)
                                    {
                                        drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBOrigin"]);
                                        drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["OriginAirportName"]);
                                    }
                                }

                                //AWBNumber Destination Section
                                if (xfsuDataSet.Tables.Contains("FinalDestinationLocation"))
                                {
                                    drs = xfsuDataSet.Tables["FinalDestinationLocation"].Select("MasterConsignment_Id=0");
                                    if (drs.Length > 0)
                                    {
                                        drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWbDestination"]);
                                        drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DestinationAirportName"]);
                                    }
                                }

                                if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 2 && dsxfsuMessage.Tables[2].Rows.Count > 0)
                                {//LOOP
                                    //AWBRouter Information
                                    xfsuDataSet.Tables["RoutingLocation"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FltDestination"]);

                                    xfsuDataSet.Tables["RoutingLocation"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["AirportName"]);
                                    // xfsuDataSet.Tables["RoutingLocation"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightLocationAirportName"]);
                                }


                                //AWBStatus Code
                                xfsuDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["MessageCode"]);


                                if (xfsuDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
                                {
                                    drs = xfsuDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                    if (drs.Length > 0)
                                    {
                                        drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedUOM"]);
                                        drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedWeight"]);
                                    }
                                }

                                if (xfsuDataSet.Tables.Contains("GrossVolumeMeasure"))
                                {
                                    drs = xfsuDataSet.Tables["GrossVolumeMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                    if (drs.Length > 0)
                                    {
                                        drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBVolumeMeasue"]);
                                        drs[0]["GrossVolumeMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBVolume"]);
                                    }
                                }

                                xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["DensityGroupCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DensityCode"]);
                                xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedPieces"]);
                                xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["PartConsignment"]);
                                xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["DiscrepancyDescriptionCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBDiscrepancyCode"]);


                                if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 3 && dsxfsuMessage.Tables[3].Rows.Count > 0)
                                {
                                    xfsuDataSet.Tables["AssociatedManifestDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["ManifestNumber"]);

                                    xfsuDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows[0]["ServiceTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["ProductType"]);

                                    xfsuDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FlightNumber"]);

                                    xfsuDataSet.Tables["UsedLogisticsTransportMeans"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["CarrierName"]);

                                    xfsuDataSet.Tables["UsedLogisticsTransportMeans"].Rows[0]["Type"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["AirCraftTypeMaster"]);


                                    xfsuDataSet.Tables["ScheduledArrivalEvent"].Rows[0]["ScheduledOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FlightScheduleArrivlaTime"]);

                                    xfsuDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FlightArriveTime"]);
                                    xfsuDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalDateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["ScheduleArrivalIndicator"]);

                                    xfsuDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FlightScheduleDepartureTime"]);
                                    xfsuDataSet.Tables["DepartureEvent"].Rows[0]["DepartureDateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["EstimatedDepTime"]);

                                    if (xfsuDataSet.Tables.Contains("PrimaryID"))
                                    {
                                        drs = xfsuDataSet.Tables["PrimaryID"].Select("CarrierParty_Id=0");
                                        if (drs.Length > 0)
                                        {
                                            drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["CarrierCode"]);
                                            drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["CarrierName"]);
                                        }
                                    }

                                    xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FlightDestlocation"]);
                                    xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FlightLocationAirportName"]);
                                    xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["LocationType"]);
                                    xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["FlightStatusTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FlightStatusTypeCode"]);

                                    xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["SpecifiedEvent"]);
                                    xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["DateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["DateTimeTypeCode"]);

                                }

                                xfsuDataSet.Tables["NotifiedParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["NotifyName"]);
                                xfsuDataSet.Tables["DeliveryParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["ConsigneeName"]);



                                if (xfsuDataSet.Tables.Contains("PrimaryID"))
                                {
                                    drs = xfsuDataSet.Tables["PrimaryID"].Select("AssociatedReceivedFromParty_Id=0");
                                    if (drs.Length > 0)
                                    {
                                        drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_schemeAgencyID"]);
                                        drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_PrimaryID_Text"]);
                                    }
                                }
                                xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssociatedReceivedAirlineName"]);
                                xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["RoleCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_RoleCode"]);
                                xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Role"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_Role"]);

                                if (xfsuDataSet.Tables.Contains("PrimaryID"))
                                {
                                    drs = xfsuDataSet.Tables["PrimaryID"].Select("AssociatedTransferredFromParty_Id=0");
                                    if (drs.Length > 0)
                                    {
                                        drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_schemeAgencyID"]);
                                        drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_PrimaryID_Text"]);
                                    }
                                }
                                xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_Name"]);
                                xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["RoleCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_RoleCode"]);
                                xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["Role"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_Role"]);


                                xfsuDataSet.Tables["HandlingOSIInstructions"].Rows[0]["Description"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["HandlingInfo"]);
                                xfsuDataSet.Tables["HandlingOSIInstructions"].Rows[0]["DescriptionCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["SHCCodes"]);

                                if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 4 && dsxfsuMessage.Tables[4].Rows.Count > 0)
                                {
                                    xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["ContentCode"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["ContentCode"]);
                                    xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["Content"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["Content"]);
                                    xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["SubjectCode"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["SubjectCode"]);
                                    xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["CountryID"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["CountryID"]);
                                    xfsuDataSet.Tables["AssociatedConsignmentCustomsProcedure"].Rows[0]["GoodsStatusCode"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["GoodsStatusCode"]);
                                }

                                if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 5 && dsxfsuMessage.Tables[5].Rows.Count > 0)
                                {
                                    xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDNumber"]);
                                    xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["CharacteristicCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDType"]);
                                    xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["OperationalStatusCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDHeightIndicator"]);
                                    if (xfsuDataSet.Tables.Contains("PrimaryID"))
                                    {
                                        drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0");
                                        if (drs.Length > 0)
                                        {
                                            drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_schemeAgencyID"]);
                                            drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_PrimaryID_Text"]);
                                        }
                                    }
                                }

                                ///House AWB s
                                if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 6 && dsxfsuMessage.Tables[6].Rows.Count > 0)
                                {
                                    if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_GrossWeightMeasure"))
                                    {
                                        drs = xfsuDataSet.Tables["IncludedHouseConsignment_GrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
                                        if (drs.Length > 0)
                                        {
                                            drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["UnitofMeasurement"]);
                                            drs[0]["IncludedHouseConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBWt"]);
                                        }
                                    }
                                    if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TotalGrossWeightMeasure"))
                                    {
                                        drs = xfsuDataSet.Tables["IncludedHouseConsignment_TotalGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
                                        if (drs.Length > 0)
                                        {
                                            drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["AcceptedUOM"]);
                                            drs[0]["IncludedHouseConsignment_TotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["GrossWeight"]);
                                        }
                                    }

                                    xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBPcs"]);
                                    xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TotalPieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["AWBIeces"]);
                                    xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["PiecesIndicator"]);

                                    if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TransportContractDocument"))
                                    {
                                        drs = xfsuDataSet.Tables["IncludedHouseConsignment_TransportContractDocument"].Select("IncludedHouseConsignment_Id=0");
                                        if (drs.Length > 0)
                                        {
                                            drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBNo"]);
                                            drs[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWbTypeCode"]);
                                        }
                                    }
                                }
                                else
                                {
                                    if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_GrossWeightMeasure"))
                                    {
                                        drs = xfsuDataSet.Tables["IncludedHouseConsignment_GrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
                                        if (drs.Length > 0)
                                        {
                                            drs[0]["unitCode"] = "";
                                            drs[0]["IncludedHouseConsignment_GrossWeightMeasure_Text"] = "";
                                        }
                                    }
                                    if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TotalGrossWeightMeasure"))
                                    {
                                        drs = xfsuDataSet.Tables["IncludedHouseConsignment_TotalGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
                                        if (drs.Length > 0)
                                        {
                                            drs[0]["unitCode"] = "";
                                            drs[0]["IncludedHouseConsignment_TotalGrossWeightMeasure_Text"] = "";
                                        }
                                    }


                                    xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["PieceQuantity"] = "";
                                    xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TotalPieceQuantity"] = "";
                                    xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TransportSplitDescription"] = "";

                                    if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TransportContractDocument"))
                                    {
                                        drs = xfsuDataSet.Tables["IncludedHouseConsignment_TransportContractDocument"].Select("IncludedHouseConsignment_Id=0");
                                        if (drs.Length > 0)
                                        {
                                            drs[0]["ID"] = "";
                                            drs[0]["TypeCode"] = "";
                                        }
                                    }
                                }
                            }

                            //xfsuDataSet.Tables["PrimaryID"].AcceptChanges();

                            string strGeneratMessage = xfsuDataSet.GetXml();
                            xfsuDataSet.Dispose();
                            strGenerateXFSUMessage = new StringBuilder(strGeneratMessage);
                            strGenerateXFSUMessage.Replace(" xmlns: ram = \"iata: datamodel:3\"", "");
                            strGenerateXFSUMessage.Replace(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
                            strGenerateXFSUMessage.Replace(" xmlns:rsm=\"iata: waybill:1\"", "");
                            strGenerateXFSUMessage.Replace(" xmlns:ram=\"iata: datamodel:3\"", "");
                            strGenerateXFSUMessage.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                            strGenerateXFSUMessage.Replace(" xsi:schemaLocation=\"iata: waybill:1 Waybill_1.xsd\"", "");
                            strGenerateXFSUMessage.Replace(" xmlns: ram = \"iata: datamodel:3\"", "");
                            strGenerateXFSUMessage.Replace("xmlns: ram = \"iata: datamodel:3\"", "");
                            strGenerateXFSUMessage.Replace(" xmlns:rsm=\"iata:waybill:1\"", "");
                            strGenerateXFSUMessage.Replace(" xmlns:ram=\"iata:datamodel:3\"", "");

                            //Replace Nodes
                            strGenerateXFSUMessage.Replace("ram:MasterConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
                            strGenerateXFSUMessage.Replace("ram:AssociatedStatusConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
                            strGenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
                            strGenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_TransportContractDocument", "ram:TransportContractDocument");
                            strGenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_TotalGrossWeightMeasure", "ram:TotalGrossWeightMeasure");
                            strGenerateXFSUMessage.Replace("ram:AssociatedStatusConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");

                            strGenerateXFSUMessage.Insert(18, " xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ccts=\"urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2\" xmlns:udt=\"urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8\" xmlns:ram=\"iata:datamodel:3\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"iata:statusmessage:1 StatusMessage_1.xsd\"");
                            strGenerateXFSUMessage.Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");

                            //cls_SCMBL cls_scmbl = new cls_SCMBL();

                            await _cl_SCMBL.addMsgToOutBox("XFSU", Convert.ToString(strGenerateXFSUMessage), "", Convert.ToString(dsxfsuMessage.Tables[7].Rows[0]["PartnerEmailiD"]));


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return strGenerateXFSUMessage.ToString();
        }

        /*Not in user*/
        //public string GenerateXFSURCSMessageofTheAWB(string AWBPrefix, string AWBNumber, string orgDest, string messageType,
        //      string doNumber, string flightNo = "", string flightDate = "1900-01-01", string EventDate = "1900-01-01")
        //{
        //    StringBuilder sbgenerateXFSUMessage = new StringBuilder();
        //    try
        //    {
        //        DataSet dsxfsuMessage = new DataSet();
        //        dsxfsuMessage = GetAWBRecordforGenerateXFSUMessage(AWBPrefix, AWBNumber, orgDest, messageType, string.Empty, flightNo, "01/01/1900", 0, 0.00);

        //        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 0 && dsxfsuMessage.Tables[0].Rows.Count > 0 && dsxfsuMessage.Tables[1].Rows.Count > 0)
        //        {
        //            var xfsuDataSet = new DataSet();
        //            DataRow[] drs;
        //            var xmlSchema =  genericFunction.GetXMLMessageData("XFSU");
        //            if (xmlSchema != null && xmlSchema.Tables.Count > 0 && xmlSchema.Tables[0].Rows.Count > 0)
        //            {
        //                string messageXML = xmlSchema.Tables[0].Rows[0]["XMLMessageData"].ToString();
        //                messageXML = ReplacingNodeNames(messageXML);
        //                var txMessage = new StringReader(messageXML);
        //                xfsuDataSet.ReadXml(txMessage);

        //                if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 0 && dsxfsuMessage.Tables[0].Rows.Count > 0)
        //                {
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["ReferenceNumber"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageName"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageTypeCode"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["IssueDateTime"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageCreatedDate"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["PurposeCode"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["PurposeCode"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["VersionNumber"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["ConversationID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["ConversionID"]);

        //                    //SenderParty
        //                    if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                    {
        //                        drs = xfsuDataSet.Tables["PrimaryID"].Select("SenderParty_Id=0");
        //                        if (drs.Length > 0)
        //                        {
        //                            drs[0]["schemeID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["SenderParty_PrimaryID"]);
        //                            drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["SenderParty_PrimaryIDText"]);
        //                        }
        //                    }
        //                    //RecipientParty
        //                    if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                    {
        //                        drs = xfsuDataSet.Tables["PrimaryID"].Select("RecipientParty_Id=0");
        //                        if (drs.Length > 0)
        //                        {
        //                            drs[0]["schemeID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["RecipientParty_PrimaryID"]);
        //                            drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["RecipientParty_PrimaryIDText"]);
        //                        }
        //                    }

        //                    //BusinessHeaderDocument
        //                    xfsuDataSet.Tables["BusinessHeaderDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["NoofStatusUpdate"]);

        //                    if (xfsuDataSet.Tables["BusinessHeaderDocument"].Columns.Contains("Reference"))
        //                    {
        //                        xfsuDataSet.Tables["BusinessHeaderDocument"].Rows[0]["Reference"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["Reference"]);
        //                    }

        //                    //MasterConsignment
        //                    if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 1 && dsxfsuMessage.Tables[1].Rows.Count > 0)
        //                    {
        //                        if (xfsuDataSet.Tables.Contains("MasterConsignment_GrossWeightMeasure"))
        //                        {
        //                            drs = xfsuDataSet.Tables["MasterConsignment_GrossWeightMeasure"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedUOM"]);
        //                                drs[0]["MasterConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedWeight"]);
        //                            }
        //                        }

        //                        if (xfsuDataSet.Tables.Contains("TotalGrossWeightMeasure"))
        //                        {
        //                            drs = xfsuDataSet.Tables["TotalGrossWeightMeasure"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBUnitGrossWeight"]);
        //                                drs[0]["TotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBGrossWeight"]);
        //                            }
        //                        }

        //                        xfsuDataSet.Tables["MasterConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedPieces"]);
        //                        xfsuDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBPieces"]);
        //                        xfsuDataSet.Tables["MasterConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["PartConsignment"]);

        //                        // AWBNumber Section
        //                        if (xfsuDataSet.Tables.Contains("TransportContractDocument"))
        //                        {
        //                            drs = xfsuDataSet.Tables["TransportContractDocument"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBNumber"]);
        //                                drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AirwayBillDocumentName"]);
        //                                drs[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBDocumentTypeCode"]);
        //                            }
        //                        }

        //                        // AWBNumber Origin Section
        //                        if (xfsuDataSet.Tables.Contains("OriginLocation"))
        //                        {
        //                            drs = xfsuDataSet.Tables["OriginLocation"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBOrigin"]);
        //                                drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["OriginAirportName"]);
        //                            }
        //                        }

        //                        //AWBNumber Destination Section
        //                        if (xfsuDataSet.Tables.Contains("FinalDestinationLocation"))
        //                        {
        //                            drs = xfsuDataSet.Tables["FinalDestinationLocation"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWbDestination"]);
        //                                drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DestinationAirportName"]);
        //                            }
        //                        }

        //                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 3 && dsxfsuMessage.Tables[3].Rows.Count > 0)
        //                        {//LOOP
        //                            //AWBRouter Information
        //                            xfsuDataSet.Tables["RoutingLocation"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FltDestination"]);
        //                            xfsuDataSet.Tables["RoutingLocation"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["AirportName"]);

        //                        }


        //                        //AWBStatus Code
        //                        xfsuDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["MessageCode"]);


        //                        if (xfsuDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
        //                        {
        //                            drs = xfsuDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DeliveredUOM"]);
        //                                drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DeliveredWeight"]);
        //                            }
        //                        }

        //                        if (xfsuDataSet.Tables.Contains("GrossVolumeMeasure"))
        //                        {
        //                            drs = xfsuDataSet.Tables["GrossVolumeMeasure"].Select("AssociatedStatusConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBVolumeMeasue"]);
        //                                drs[0]["GrossVolumeMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBVolume"]);
        //                            }
        //                        }

        //                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["DensityGroupCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DensityCode"]);
        //                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DeliveredPieces"]);
        //                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["PartConsignment"]);
        //                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["DiscrepancyDescriptionCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBDiscrepancyCode"]);
        //                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["StatusDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["StatusDescription"]);


        //                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 2 && dsxfsuMessage.Tables[2].Rows.Count > 0)
        //                        {
        //                            xfsuDataSet.Tables["AssociatedManifestDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ManifestNumber"]);

        //                            xfsuDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows[0]["ServiceTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ProductType"]);

        //                            xfsuDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightNumber"]);

        //                            xfsuDataSet.Tables["UsedLogisticsTransportMeans"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["CarrierName"]);

        //                            xfsuDataSet.Tables["UsedLogisticsTransportMeans"].Rows[0]["Type"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["AirCraftTypeMaster"]);


        //                            xfsuDataSet.Tables["ScheduledArrivalEvent"].Rows[0]["ScheduledOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightScheduleArrivlaTime"]);

        //                            xfsuDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightArriveTime"]);
        //                            xfsuDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalDateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ScheduleArrivalIndicator"]);

        //                            xfsuDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightScheduleDepartureTime"]);
        //                            xfsuDataSet.Tables["DepartureEvent"].Rows[0]["DepartureDateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["EstimatedDepTime"]);

        //                            if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                            {
        //                                drs = xfsuDataSet.Tables["PrimaryID"].Select("CarrierParty_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["CarrierCode"]);
        //                                    drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["CarrierName"]);
        //                                }
        //                            }

        //                            xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightDestlocation"]);
        //                            xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightLocationAirportName"]);
        //                            xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["LocationType"]);
        //                            xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["FlightStatusTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightStatusTypeCode"]);

        //                            //xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["SpecifiedEvent"]);
        //                            xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"] = Convert.ToString(EventDate);
        //                            xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["DateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["DateTimeTypeCode"]);

        //                        }

        //                        xfsuDataSet.Tables["NotifiedParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["NotifyName"]);
        //                        xfsuDataSet.Tables["DeliveryParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["ConsigneeName"]);



        //                        if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                        {
        //                            drs = xfsuDataSet.Tables["PrimaryID"].Select("AssociatedReceivedFromParty_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_schemeAgencyID"]);
        //                                drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_PrimaryID_Text"]);
        //                            }
        //                        }
        //                        xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssociatedReceivedAirlineName"]);
        //                        xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["RoleCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_RoleCode"]);
        //                        xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Role"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_Role"]);

        //                        if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                        {
        //                            drs = xfsuDataSet.Tables["PrimaryID"].Select("AssociatedTransferredFromParty_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_schemeAgencyID"]);
        //                                drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_PrimaryID_Text"]);
        //                            }
        //                        }
        //                        xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_Name"]);
        //                        xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["RoleCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_RoleCode"]);
        //                        xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["Role"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_Role"]);


        //                        xfsuDataSet.Tables["HandlingOSIInstructions"].Rows[0]["Description"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["HandlingInfo"]);
        //                        xfsuDataSet.Tables["HandlingOSIInstructions"].Rows[0]["DescriptionCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["SHCCodes"]);

        //                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 4 && dsxfsuMessage.Tables[4].Rows.Count > 0)
        //                        {
        //                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["ContentCode"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["ContentCode"]);
        //                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["Content"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["Content"]);
        //                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["SubjectCode"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["SubjectCode"]);
        //                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["CountryID"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["CountryID"]);
        //                            xfsuDataSet.Tables["AssociatedConsignmentCustomsProcedure"].Rows[0]["GoodsStatusCode"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["GoodsStatusCode"]);
        //                        }

        //                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 5 && dsxfsuMessage.Tables[5].Rows.Count > 0)
        //                        {
        //                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDNumber"]);
        //                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["CharacteristicCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDType"]);
        //                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["OperationalStatusCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDHeightIndicator"]);
        //                            if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                            {
        //                                drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_schemeAgencyID"]);
        //                                    drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_PrimaryID_Text"]);
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["ID"] = "";
        //                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["CharacteristicCode"] = "";
        //                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["OperationalStatusCode"] = "";
        //                            if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                            {
        //                                drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["schemeAgencyID"] = "";
        //                                    drs[0]["PrimaryID_Text"] = "";
        //                                }
        //                            }
        //                        }

        //                        ///House AWB s
        //                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 6 && dsxfsuMessage.Tables[6].Rows.Count > 0)
        //                        {
        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_GrossWeightMeasure"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_GrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["UnitofMeasurement"]);
        //                                    drs[0]["IncludedHouseConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBWt"]);
        //                                }
        //                            }
        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TotalGrossWeightMeasure"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_TotalGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["AcceptedUOM"]);
        //                                    drs[0]["IncludedHouseConsignment_TotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["GrossWeight"]);
        //                                }
        //                            }

        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBPcs"]);
        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TotalPieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["AWBPieces"]);
        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["PiecesIndicator"]);

        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TransportContractDocument"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_TransportContractDocument"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBNo"]);
        //                                    drs[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWbTypeCode"]);
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_GrossWeightMeasure"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_GrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["unitCode"] = "";
        //                                    drs[0]["IncludedHouseConsignment_GrossWeightMeasure_Text"] = "";
        //                                }
        //                            }
        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TotalGrossWeightMeasure"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_TotalGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["unitCode"] = "";
        //                                    drs[0]["IncludedHouseConsignment_TotalGrossWeightMeasure_Text"] = "";
        //                                }
        //                            }


        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["PieceQuantity"] = "";
        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TotalPieceQuantity"] = "";
        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TransportSplitDescription"] = "";

        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TransportContractDocument"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_TransportContractDocument"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["ID"] = "";
        //                                    drs[0]["TypeCode"] = "";
        //                                }
        //                            }
        //                        }
        //                    }
        //                    string generatMessage = xfsuDataSet.GetXml();
        //                    xfsuDataSet.Dispose();
        //                    sbgenerateXFSUMessage = new StringBuilder(generatMessage);
        //                    sbgenerateXFSUMessage.Replace(" xmlns: ram = \"iata: datamodel:3\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:rsm=\"iata: waybill:1\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:ram=\"iata: datamodel:3\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xsi:schemaLocation=\"iata: waybill:1 Waybill_1.xsd\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns: ram = \"iata: datamodel:3\"", "");
        //                    sbgenerateXFSUMessage.Replace("xmlns: ram = \"iata: datamodel:3\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:rsm=\"iata:waybill:1\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:ram=\"iata:datamodel:3\"", "");

        //                    //Replace Nodes
        //                    sbgenerateXFSUMessage.Replace("ram:MasterConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
        //                    sbgenerateXFSUMessage.Replace("ram:AssociatedStatusConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
        //                    sbgenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
        //                    sbgenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_TransportContractDocument", "ram:TransportContractDocument");
        //                    sbgenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_TotalGrossWeightMeasure", "ram:TotalGrossWeightMeasure");
        //                    sbgenerateXFSUMessage.Replace("ram:AssociatedStatusConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");

        //                    sbgenerateXFSUMessage.Insert(18, " xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ccts=\"urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2\" xmlns:udt=\"urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8\" xmlns:ram=\"iata:datamodel:3\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"iata:statusmessage:1 StatusMessage_1.xsd\"");
        //                    sbgenerateXFSUMessage.Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");

        //                }
        //            }
        //            //else
        //            //{
        //            //    sbgenerateXFSUMessage.Append("No Message format available in the system.");
        //            //}
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //    }
        //    return sbgenerateXFSUMessage.ToString();
        //}

        //public string GenerateXFSUBKDMessageofTheAWB(string AWBPrefix, string AWBNumber, string orgDest, string messageType,
        //     string doNumber, string flightNo = "", string flightDate = "1900-01-01", string EventDate = "1900-01-01")
        //{
        //    StringBuilder sbgenerateXFSUMessage = new StringBuilder();

        //    try
        //    {
        //        DataSet dsxfsuMessage = new DataSet();

        //        dsxfsuMessage = GetAWBRecordforGenerateXFSUMessage(AWBPrefix, AWBNumber, orgDest, messageType, doNumber, flightNo, flightDate, 0, 0.00);

        //        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 0 && dsxfsuMessage.Tables[0].Rows.Count > 0 && dsxfsuMessage.Tables[1].Rows.Count > 0)
        //        {
        //            var xfsuDataSet = new DataSet();
        //            DataRow[] drs;
        //            var xmlSchema = genericFunction.GetXMLMessageData("XFSU");

        //            if (xmlSchema != null && xmlSchema.Tables.Count > 0 && xmlSchema.Tables[0].Rows.Count > 0)
        //            {
        //                string messageXML = xmlSchema.Tables[0].Rows[0]["XMLMessageData"].ToString();
        //                messageXML = ReplacingNodeNames(messageXML);
        //                var txMessage = new StringReader(messageXML);
        //                xfsuDataSet.ReadXml(txMessage);

        //                if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 0 && dsxfsuMessage.Tables[0].Rows.Count > 0)
        //                {
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["NoofStatusUpdate"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageName"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageTypeCode"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["IssueDateTime"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageCreatedDate"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["PurposeCode"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["PurposeCode"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["VersionNumber"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["ConversationID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["ConversionID"]);

        //                    //SenderParty
        //                    if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                    {
        //                        drs = xfsuDataSet.Tables["PrimaryID"].Select("SenderParty_Id=0");
        //                        if (drs.Length > 0)
        //                        {
        //                            drs[0]["schemeID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["SenderParty_PrimaryID"]);
        //                            drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["SenderParty_PrimaryIDText"]);
        //                        }
        //                    }
        //                    //RecipientParty
        //                    if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                    {
        //                        drs = xfsuDataSet.Tables["PrimaryID"].Select("RecipientParty_Id=0");
        //                        if (drs.Length > 0)
        //                        {
        //                            drs[0]["schemeID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["RecipientParty_PrimaryID"]);
        //                            drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["RecipientParty_PrimaryIDText"]);
        //                        }
        //                    }

        //                    //BusinessHeaderDocument
        //                    if (xfsuDataSet.Tables.Contains("BusinessHeaderDocument"))
        //                    {
        //                        xfsuDataSet.Tables["BusinessHeaderDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["NoofStatusUpdate"]);

        //                        if (xfsuDataSet.Tables["BusinessHeaderDocument"].Columns.Contains("Reference"))
        //                        {
        //                            xfsuDataSet.Tables["BusinessHeaderDocument"].Rows[0]["Reference"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["Reference"]);
        //                        }
        //                    }


        //                    //MasterConsignment
        //                    if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 1 && dsxfsuMessage.Tables[1].Rows.Count > 0)
        //                    {
        //                        if (xfsuDataSet.Tables.Contains("MasterConsignment_GrossWeightMeasure"))
        //                        {
        //                            drs = xfsuDataSet.Tables["MasterConsignment_GrossWeightMeasure"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedUOM"]);
        //                                drs[0]["MasterConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedWeight"]);
        //                            }
        //                        }

        //                        if (xfsuDataSet.Tables.Contains("TotalGrossWeightMeasure"))
        //                        {
        //                            drs = xfsuDataSet.Tables["TotalGrossWeightMeasure"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBUnitGrossWeight"]);
        //                                drs[0]["TotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBGrossWeight"]);
        //                            }
        //                        }

        //                        xfsuDataSet.Tables["MasterConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedPieces"]);
        //                        xfsuDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBPieces"]);
        //                        xfsuDataSet.Tables["MasterConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["PartConsignment"]);

        //                        // AWBNumber Section
        //                        if (xfsuDataSet.Tables.Contains("TransportContractDocument"))
        //                        {
        //                            drs = xfsuDataSet.Tables["TransportContractDocument"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBNumber"]);
        //                                drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AirwayBillDocumentName"]);
        //                                drs[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBDocumentTypeCode"]);
        //                            }
        //                        }

        //                        // AWBNumber Origin Section
        //                        if (xfsuDataSet.Tables.Contains("OriginLocation"))
        //                        {
        //                            drs = xfsuDataSet.Tables["OriginLocation"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBOrigin"]);
        //                                drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["OriginAirportName"]);
        //                            }
        //                        }

        //                        //AWBNumber Destination Section
        //                        if (xfsuDataSet.Tables.Contains("FinalDestinationLocation"))
        //                        {
        //                            drs = xfsuDataSet.Tables["FinalDestinationLocation"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWbDestination"]);
        //                                drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DestinationAirportName"]);
        //                            }
        //                        }

        //                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 3 && dsxfsuMessage.Tables[3].Rows.Count > 0)
        //                        {//LOOP

        //                            for (int row = 1; row < dsxfsuMessage.Tables[3].Rows.Count; row++)
        //                            {
        //                                //if (!(xfsuDataSet.Tables["MasterConsignment"].Rows.Count > row))
        //                                //{
        //                                //    DataRow drBolsegment = xfsuDataSet.Tables["MasterConsignment"].NewRow();
        //                                //    drBolsegment["MasterConsignment_Id"] = row;

        //                                //    xfsuDataSet.Tables["MasterConsignment"].Rows.Add(drBolsegment);
        //                                //}
        //                                if (row == 1)
        //                                {
        //                                    drs = xfsuDataSet.Tables["RoutingLocation"].Select("MasterConsignment_Id=0");
        //                                    if (drs.Length > 0)
        //                                    {
        //                                        drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[row]["FltDestination"]);
        //                                        drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[row]["AirportName"]);
        //                                    }
        //                                }
        //                                if (!(xfsuDataSet.Tables["RoutingLocation"].Rows.Count >= row))
        //                                {

        //                                    DataRow drBolsegment = xfsuDataSet.Tables["RoutingLocation"].NewRow();
        //                                    drBolsegment["ID"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[row]["FltDestination"]);
        //                                    drBolsegment["Name"] = dsxfsuMessage.Tables[3].Rows[row]["AirportName"];
        //                                    drBolsegment["MasterConsignment_Id"] = 0;
        //                                    xfsuDataSet.Tables["RoutingLocation"].Rows.Add(drBolsegment);
        //                                }

        //                            }


        //                            //AWBRouter Information
        //                            //xfsuDataSet.Tables["RoutingLocation"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FltDestination"]);
        //                            //xfsuDataSet.Tables["RoutingLocation"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["AirportName"]);


        //                            for (int rowchild = 0; rowchild < dsxfsuMessage.Tables[3].Rows.Count; rowchild++)
        //                            {


        //                                //AWBStatus Code
        //                                if (!(xfsuDataSet.Tables["ReportedStatus"].Rows.Count > rowchild))
        //                                {
        //                                    DataRow drMaster = xfsuDataSet.Tables["ReportedStatus"].NewRow();
        //                                    drMaster["ReasonCode"] = rowchild;
        //                                    drMaster["ReportedStatus_Id"] = rowchild;

        //                                    drMaster["MasterConsignment_Id"] = 0;

        //                                    xfsuDataSet.Tables["ReportedStatus"].Rows.Add(drMaster);
        //                                }
        //                                xfsuDataSet.Tables["ReportedStatus"].Rows[rowchild]["ReasonCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["MessageCode"]);

        //                                if (xfsuDataSet.Tables.Contains("AssociatedStatusConsignment"))
        //                                {

        //                                    if (rowchild == 0)
        //                                    {
        //                                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["DensityGroupCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DensityCode"]);
        //                                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["PCSonflight"]);
        //                                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["PartConsignment"]);
        //                                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["DiscrepancyDescriptionCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBDiscrepancyCode"]);
        //                                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["StatusDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["StatusDescription"]);
        //                                    }
        //                                    if (!(xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows.Count > rowchild))
        //                                    {
        //                                        DataRow drBolsegment = xfsuDataSet.Tables["AssociatedStatusConsignment"].NewRow();
        //                                        drBolsegment["DensityGroupCode"] = rowchild;
        //                                        drBolsegment["PieceQuantity"] = rowchild;
        //                                        drBolsegment["TransportSplitDescription"] = rowchild;
        //                                        drBolsegment["DiscrepancyDescriptionCode"] = rowchild;
        //                                        drBolsegment["StatusDescription"] = rowchild;
        //                                        drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;
        //                                        drBolsegment["ReportedStatus_Id"] = rowchild;

        //                                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows.Add(drBolsegment);
        //                                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[rowchild]["DensityGroupCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DensityCode"]);
        //                                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[rowchild]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[rowchild]["PCSonflight"]);
        //                                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[rowchild]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["PartConsignment"]);
        //                                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[rowchild]["DiscrepancyDescriptionCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBDiscrepancyCode"]);
        //                                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[rowchild]["StatusDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["StatusDescription"]);

        //                                    }

        //                                }


        //                                if (xfsuDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))//AssociatedStatusGrossWeightMeasure
        //                                {
        //                                    if (rowchild == 0)
        //                                    {

        //                                        drs = xfsuDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
        //                                        if (drs.Length > 0)
        //                                        {
        //                                            drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightUOM"]);
        //                                            drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["WTonflight"]);
        //                                        }

        //                                    }
        //                                    if (!(xfsuDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Rows.Count > rowchild))
        //                                    {

        //                                        DataRow drBolsegment = xfsuDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].NewRow();
        //                                        drBolsegment["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[rowchild]["FlightUOM"]);
        //                                        drBolsegment["AssociatedStatusConsignment_GrossWeightMeasure_Text"] = dsxfsuMessage.Tables[2].Rows[rowchild]["WTonflight"];
        //                                        drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;

        //                                        xfsuDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Rows.Add(drBolsegment);
        //                                    }
        //                                }
        //                                if (xfsuDataSet.Tables.Contains("GrossVolumeMeasure"))
        //                                {

        //                                    if (rowchild == 0)
        //                                    {
        //                                        drs = xfsuDataSet.Tables["GrossVolumeMeasure"].Select("AssociatedStatusConsignment_Id=0");
        //                                        if (drs.Length > 0)
        //                                        {
        //                                            drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["fltVolumeunit"]);
        //                                            drs[0]["GrossVolumeMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["fltVolume"]);
        //                                        }
        //                                    }
        //                                    if (!(xfsuDataSet.Tables["GrossVolumeMeasure"].Rows.Count > rowchild))
        //                                    {

        //                                        DataRow drBolsegment = xfsuDataSet.Tables["GrossVolumeMeasure"].NewRow();
        //                                        drBolsegment["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[rowchild]["fltVolumeunit"]);
        //                                        drBolsegment["GrossVolumeMeasure_Text"] = dsxfsuMessage.Tables[2].Rows[rowchild]["fltVolume"];
        //                                        drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;

        //                                        xfsuDataSet.Tables["GrossVolumeMeasure"].Rows.Add(drBolsegment);
        //                                    }


        //                                }


        //                                if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 2
        //                                    && dsxfsuMessage.Tables[2].Rows.Count > 0)
        //                                {
        //                                    if (xfsuDataSet.Tables.Contains("AssociatedManifestDocument"))
        //                                    {
        //                                        if (rowchild == 0)
        //                                        {
        //                                            xfsuDataSet.Tables["AssociatedManifestDocument"].Rows[0]["ID"] =
        //                                                Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ManifestNumber"]);

        //                                        }
        //                                        if (!(xfsuDataSet.Tables["AssociatedManifestDocument"].Rows.Count > rowchild))
        //                                        {
        //                                            DataRow drBolsegment = xfsuDataSet.Tables["AssociatedManifestDocument"].NewRow();
        //                                            drBolsegment["ID"] = rowchild;
        //                                            drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;

        //                                            xfsuDataSet.Tables["AssociatedManifestDocument"].Rows.Add(drBolsegment);

        //                                            xfsuDataSet.Tables["AssociatedManifestDocument"].Rows[rowchild]["ID"]
        //                                                = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ManifestNumber"]);

        //                                        }
        //                                    }
        //                                    if (xfsuDataSet.Tables.Contains("ApplicableLogisticsServiceCharge"))
        //                                    {
        //                                        if (rowchild == 0)
        //                                        {
        //                                            xfsuDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows[0]["ServiceTypeCode"] =
        //                                                Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ProductType"]);

        //                                        }

        //                                        if (!(xfsuDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows.Count > rowchild))
        //                                        {
        //                                            DataRow drBolsegment = xfsuDataSet.Tables["ApplicableLogisticsServiceCharge"].NewRow();
        //                                            drBolsegment["ServiceTypeCode"] = rowchild;
        //                                            drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;

        //                                            xfsuDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows.Add(drBolsegment);

        //                                            xfsuDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows[rowchild]["ServiceTypeCode"]
        //                                                = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ProductType"]);

        //                                        }

        //                                    }


        //                                    if (xfsuDataSet.Tables.Contains("SpecifiedLogisticsTransportMovement"))
        //                                    {
        //                                        if (rowchild == 0)
        //                                        {
        //                                            xfsuDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[rowchild]["ID"] =
        //                                                Convert.ToString(dsxfsuMessage.Tables[2].Rows[rowchild]["FlightNumber"]);
        //                                        }
        //                                        if (!(xfsuDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows.Count > rowchild))
        //                                        {

        //                                            DataRow drBolsegment = xfsuDataSet.Tables["SpecifiedLogisticsTransportMovement"].NewRow();
        //                                            drBolsegment["SpecifiedLogisticsTransportMovement_Id"] = rowchild;
        //                                            drBolsegment["ID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[rowchild]["FlightNumber"]);
        //                                            drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;

        //                                            xfsuDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows.Add(drBolsegment);
        //                                        }

        //                                        if (xfsuDataSet.Tables.Contains("UsedLogisticsTransportMeans"))
        //                                        {
        //                                            if (rowchild == 0)
        //                                            {
        //                                                xfsuDataSet.Tables["UsedLogisticsTransportMeans"].Rows[0]["Name"] =
        //                                            Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["CarrierName"]);

        //                                                xfsuDataSet.Tables["UsedLogisticsTransportMeans"].Rows[0]["Type"] =
        //                                                    Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["AirCraftTypeMaster"]);
        //                                            }
        //                                            if (!(xfsuDataSet.Tables["UsedLogisticsTransportMeans"].Rows.Count > rowchild))
        //                                            {

        //                                                DataRow drBolsegment = xfsuDataSet.Tables["UsedLogisticsTransportMeans"].NewRow();
        //                                                drBolsegment["Name"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[rowchild]["CarrierName"]); ;
        //                                                drBolsegment["Type"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[rowchild]["AirCraftTypeMaster"]);
        //                                                drBolsegment["SpecifiedLogisticsTransportMovement_Id"] = rowchild;

        //                                                xfsuDataSet.Tables["UsedLogisticsTransportMeans"].Rows.Add(drBolsegment);
        //                                            }
        //                                        }
        //                                        if (xfsuDataSet.Tables.Contains("ScheduledArrivalEvent"))
        //                                        {
        //                                            if (rowchild == 0)
        //                                            {

        //                                                xfsuDataSet.Tables["ScheduledArrivalEvent"].Rows[0]["ScheduledOccurrenceDateTime"] =
        //                                                    Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightScheduleArrivlaTime"]);
        //                                            }
        //                                            if (!(xfsuDataSet.Tables["ScheduledArrivalEvent"].Rows.Count > rowchild))
        //                                            {

        //                                                DataRow drBolsegment = xfsuDataSet.Tables["ScheduledArrivalEvent"].NewRow();
        //                                                drBolsegment["ScheduledOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[rowchild]["FlightScheduleArrivlaTime"]); ;

        //                                                drBolsegment["SpecifiedLogisticsTransportMovement_Id"] = rowchild;

        //                                                xfsuDataSet.Tables["ScheduledArrivalEvent"].Rows.Add(drBolsegment);
        //                                            }
        //                                        }
        //                                        if (xfsuDataSet.Tables.Contains("ArrivalEvent"))
        //                                        {
        //                                            if (rowchild == 0)
        //                                            {
        //                                                xfsuDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightArriveTime"]);
        //                                                xfsuDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalDateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ScheduleArrivalIndicator"]);
        //                                            }
        //                                            if (!(xfsuDataSet.Tables["ArrivalEvent"].Rows.Count > rowchild))
        //                                            {

        //                                                DataRow drBolsegment = xfsuDataSet.Tables["ArrivalEvent"].NewRow();
        //                                                drBolsegment["ArrivalOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[rowchild]["FlightArriveTime"]); ;
        //                                                drBolsegment["ArrivalDateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[rowchild]["ScheduleArrivalIndicator"]); ;

        //                                                drBolsegment["SpecifiedLogisticsTransportMovement_Id"] = rowchild;

        //                                                xfsuDataSet.Tables["ArrivalEvent"].Rows.Add(drBolsegment);
        //                                            }

        //                                        }
        //                                        if (xfsuDataSet.Tables.Contains("DepartureEvent"))
        //                                        {
        //                                            if (rowchild == 0)
        //                                            {
        //                                                xfsuDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightScheduleDepartureTime"]);
        //                                                xfsuDataSet.Tables["DepartureEvent"].Rows[0]["DepartureDateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["EstimatedDepTime"]);
        //                                            }
        //                                            if (!(xfsuDataSet.Tables["DepartureEvent"].Rows.Count > rowchild))
        //                                            {

        //                                                DataRow drBolsegment = xfsuDataSet.Tables["DepartureEvent"].NewRow();
        //                                                drBolsegment["DepartureOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[rowchild]["FlightScheduleDepartureTime"]); ;
        //                                                drBolsegment["DepartureDateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[rowchild]["EstimatedDepTime"]); ;

        //                                                drBolsegment["SpecifiedLogisticsTransportMovement_Id"] = rowchild;

        //                                                xfsuDataSet.Tables["DepartureEvent"].Rows.Add(drBolsegment);
        //                                            }
        //                                        }
        //                                        if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                                        {
        //                                            drs = xfsuDataSet.Tables["PrimaryID"].Select("CarrierParty_Id=0");
        //                                            if (drs.Length > 0)
        //                                            {
        //                                                drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["CarrierCode"]);
        //                                                drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["CarrierName"]);
        //                                            }
        //                                        }

        //                                        if (xfsuDataSet.Tables.Contains("SpecifiedLocation"))
        //                                        {
        //                                            if (rowchild == 0)
        //                                            {
        //                                                xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBOrigin"]);
        //                                                xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["OriginAirportName"]);
        //                                                xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["LocationType"]);
        //                                                xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["FlightStatusTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightStatusTypeCode"]);
        //                                            }
        //                                            if (!(xfsuDataSet.Tables["SpecifiedLocation"].Rows.Count > rowchild))
        //                                            {

        //                                                DataRow drBolsegment = xfsuDataSet.Tables["SpecifiedLocation"].NewRow();
        //                                                drBolsegment["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBOrigin"]); ;
        //                                                drBolsegment["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["OriginAirportName"]); ;
        //                                                drBolsegment["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[rowchild]["LocationType"]); ;
        //                                                drBolsegment["FlightStatusTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[rowchild]["FlightStatusTypeCode"]); ;

        //                                                drBolsegment["SpecifiedLogisticsTransportMovement_Id"] = rowchild;

        //                                                xfsuDataSet.Tables["SpecifiedLocation"].Rows.Add(drBolsegment);
        //                                            }
        //                                        }
        //                                        if (xfsuDataSet.Tables.Contains("SpecifiedEvent"))
        //                                        {
        //                                            if (rowchild == 0)
        //                                            {
        //                                                //xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["SpecifiedEvent"]);
        //                                                xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"] = Convert.ToString(EventDate);
        //                                                xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["DateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["DateTimeTypeCode"]);
        //                                            }
        //                                            if (!(xfsuDataSet.Tables["SpecifiedEvent"].Rows.Count > rowchild))
        //                                            {

        //                                                DataRow drBolsegment = xfsuDataSet.Tables["SpecifiedEvent"].NewRow();
        //                                                //drBolsegment["OccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[rowchild]["SpecifiedEvent"]);
        //                                                drBolsegment["OccurrenceDateTime"] = Convert.ToString(EventDate);
        //                                                drBolsegment["DateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[rowchild]["DateTimeTypeCode"]);

        //                                                drBolsegment["SpecifiedLogisticsTransportMovement_Id"] = rowchild;

        //                                                xfsuDataSet.Tables["SpecifiedEvent"].Rows.Add(drBolsegment);
        //                                            }
        //                                        }
        //                                    }
        //                                }

        //                                if (xfsuDataSet.Tables.Contains("NotifiedParty"))
        //                                {

        //                                    if (rowchild == 0)
        //                                    {
        //                                        xfsuDataSet.Tables["NotifiedParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["NotifyName"]);
        //                                    }
        //                                    if (!(xfsuDataSet.Tables["NotifiedParty"].Rows.Count > rowchild))
        //                                    {
        //                                        DataRow drBolsegment = xfsuDataSet.Tables["NotifiedParty"].NewRow();
        //                                        drBolsegment["Name"] = rowchild;

        //                                        drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;

        //                                        xfsuDataSet.Tables["NotifiedParty"].Rows.Add(drBolsegment);
        //                                        xfsuDataSet.Tables["NotifiedParty"].Rows[rowchild]["Name"] =
        //                                            Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["NotifyName"]);

        //                                    }
        //                                }
        //                                if (xfsuDataSet.Tables.Contains("DeliveryParty"))
        //                                {
        //                                    if (rowchild == 0)
        //                                    {
        //                                        xfsuDataSet.Tables["DeliveryParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["ConsigneeName"]);
        //                                    }
        //                                    if (!(xfsuDataSet.Tables["DeliveryParty"].Rows.Count > rowchild))
        //                                    {
        //                                        DataRow drBolsegment = xfsuDataSet.Tables["DeliveryParty"].NewRow();
        //                                        drBolsegment["Name"] = rowchild;

        //                                        drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;

        //                                        xfsuDataSet.Tables["DeliveryParty"].Rows.Add(drBolsegment);
        //                                        xfsuDataSet.Tables["DeliveryParty"].Rows[rowchild]["Name"] =
        //                                            Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["ConsigneeName"]);
        //                                    }
        //                                }

        //                                if (xfsuDataSet.Tables.Contains("AssociatedReceivedFromParty"))
        //                                {
        //                                    if (rowchild == 0)
        //                                    {

        //                                        xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssociatedReceivedAirlineName"]);
        //                                        xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["RoleCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_RoleCode"]);
        //                                        xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Role"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_Role"]);
        //                                    }

        //                                    if (!(xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows.Count > rowchild))
        //                                    {
        //                                        DataRow drBolsegment = xfsuDataSet.Tables["AssociatedReceivedFromParty"].NewRow();
        //                                        drBolsegment["Name"] = rowchild;
        //                                        drBolsegment["RoleCode"] = rowchild;
        //                                        drBolsegment["Role"] = rowchild;
        //                                        drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;

        //                                        xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows.Add(drBolsegment);
        //                                        xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[rowchild]["Name"] =
        //                                            Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssociatedReceivedAirlineName"]);
        //                                        xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[rowchild]["RoleCode"] =
        //                                           Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_RoleCode"]);
        //                                        xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[rowchild]["Role"] =
        //                                           Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_Role"]);

        //                                    }
        //                                    if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                                    {
        //                                        //drs = xfsuDataSet.Tables["PrimaryID"].Select("AssociatedReceivedFromParty_Id=" + rowchild);
        //                                        //if (drs.Length > 0)
        //                                        //{
        //                                        //    drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_schemeAgencyID"]);
        //                                        //    drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_PrimaryID_Text"]);
        //                                        //}

        //                                        if (!(xfsuDataSet.Tables["PrimaryID"].Select("AssociatedReceivedFromParty_Id=0").Length > rowchild))
        //                                        {
        //                                            DataRow drMaster = xfsuDataSet.Tables["PrimaryID"].NewRow();
        //                                            drMaster["AssociatedReceivedFromParty_Id"] = rowchild;
        //                                            xfsuDataSet.Tables["PrimaryID"].Rows.Add(drMaster);

        //                                        }
        //                                        drs = xfsuDataSet.Tables["PrimaryID"].Select("AssociatedReceivedFromParty_Id=" + Convert.ToString(rowchild));
        //                                        if (drs.Length > 0)
        //                                        {
        //                                            drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_schemeAgencyID"]);
        //                                            drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_PrimaryID_Text"]);
        //                                        }
        //                                    }

        //                                }


        //                                if (xfsuDataSet.Tables.Contains("AssociatedTransferredFromParty"))
        //                                {
        //                                    if (rowchild == 0)
        //                                    {
        //                                        xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_Name"]);
        //                                        xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["RoleCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_RoleCode"]);
        //                                        xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["Role"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_Role"]);
        //                                    }

        //                                    if (!(xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows.Count > rowchild))
        //                                    {
        //                                        DataRow drBolsegment = xfsuDataSet.Tables["AssociatedTransferredFromParty"].NewRow();
        //                                        drBolsegment["Name"] = rowchild;
        //                                        drBolsegment["RoleCode"] = rowchild;
        //                                        drBolsegment["Role"] = rowchild;
        //                                        drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;

        //                                        xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows.Add(drBolsegment);
        //                                        xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[rowchild]["Name"] =
        //                                            Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_Name"]);
        //                                        xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[rowchild]["RoleCode"] =
        //                                           Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_RoleCode"]);
        //                                        xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[rowchild]["Role"] =
        //                                           Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_Role"]);

        //                                    }

        //                                    if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                                    {
        //                                        if (!(xfsuDataSet.Tables["PrimaryID"].Select("AssociatedTransferredFromParty_Id=0").Length > rowchild))
        //                                        {
        //                                            DataRow drMaster = xfsuDataSet.Tables["PrimaryID"].NewRow();
        //                                            drMaster["AssociatedTransferredFromParty_Id"] = rowchild;
        //                                            xfsuDataSet.Tables["PrimaryID"].Rows.Add(drMaster);

        //                                        }
        //                                        drs = xfsuDataSet.Tables["PrimaryID"].Select("AssociatedTransferredFromParty_Id=" + rowchild);
        //                                        if (drs.Length > 0)
        //                                        {
        //                                            drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_schemeAgencyID"]);
        //                                            drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_PrimaryID_Text"]);
        //                                        }

        //                                    }

        //                                }

        //                                if (xfsuDataSet.Tables.Contains("HandlingOSIInstructions"))
        //                                {
        //                                    if (rowchild == 0)
        //                                    {

        //                                        xfsuDataSet.Tables["HandlingOSIInstructions"].Rows[0]["Description"] =
        //                                            Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["HandlingInfo"]);
        //                                        xfsuDataSet.Tables["HandlingOSIInstructions"].Rows[0]["DescriptionCode"] =
        //                                            Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["SHCCodes"]);
        //                                    }
        //                                    if (!(xfsuDataSet.Tables["HandlingOSIInstructions"].Rows.Count > rowchild))
        //                                    {
        //                                        DataRow drBolsegment = xfsuDataSet.Tables["HandlingOSIInstructions"].NewRow();
        //                                        drBolsegment["Description"] = rowchild;
        //                                        drBolsegment["DescriptionCode"] = rowchild;

        //                                        drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;

        //                                        xfsuDataSet.Tables["HandlingOSIInstructions"].Rows.Add(drBolsegment);
        //                                        xfsuDataSet.Tables["HandlingOSIInstructions"].Rows[rowchild]["Description"] =
        //                                            Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["HandlingInfo"]);
        //                                        xfsuDataSet.Tables["HandlingOSIInstructions"].Rows[rowchild]["DescriptionCode"] =
        //                                           Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["SHCCodes"]);
        //                                    }


        //                                }

        //                                if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 4 && dsxfsuMessage.Tables[4].Rows.Count > 0)
        //                                {
        //                                    if (xfsuDataSet.Tables.Contains("IncludedCustomsNote"))
        //                                    {
        //                                        if (rowchild == 0)
        //                                        {
        //                                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["ContentCode"] =
        //                                                Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["ContentCode"]);
        //                                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["Content"] =
        //                                                Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["Content"]);
        //                                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["SubjectCode"] =
        //                                                Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["SubjectCode"]);
        //                                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["CountryID"] =
        //                                                Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["CountryID"]);
        //                                        }
        //                                        if (!(xfsuDataSet.Tables["IncludedCustomsNote"].Rows.Count > rowchild))
        //                                        {
        //                                            DataRow drBolsegment = xfsuDataSet.Tables["IncludedCustomsNote"].NewRow();
        //                                            drBolsegment["ContentCode"] = rowchild;
        //                                            drBolsegment["Content"] = rowchild;
        //                                            drBolsegment["SubjectCode"] = rowchild;
        //                                            drBolsegment["CountryID"] = rowchild;
        //                                            drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;

        //                                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows.Add(drBolsegment);
        //                                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[rowchild]["ContentCode"] =
        //                                                Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["ContentCode"]);
        //                                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[rowchild]["Content"] =
        //                                               Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["Content"]);
        //                                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[rowchild]["SubjectCode"] =
        //                                              Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["SubjectCode"]);
        //                                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[rowchild]["CountryID"] =
        //                                              Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["CountryID"]);
        //                                        }

        //                                    }

        //                                    if (xfsuDataSet.Tables.Contains("AssociatedConsignmentCustomsProcedure"))
        //                                    {
        //                                        if (rowchild == 0)
        //                                        {
        //                                            xfsuDataSet.Tables["AssociatedConsignmentCustomsProcedure"].Rows[0]["GoodsStatusCode"] =
        //                                                Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["GoodsStatusCode"]);

        //                                        }
        //                                        if (!(xfsuDataSet.Tables["AssociatedConsignmentCustomsProcedure"].Rows.Count > rowchild))
        //                                        {
        //                                            DataRow drBolsegment = xfsuDataSet.Tables["AssociatedConsignmentCustomsProcedure"].NewRow();
        //                                            drBolsegment["GoodsStatusCode"] = rowchild;

        //                                            drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;

        //                                            xfsuDataSet.Tables["AssociatedConsignmentCustomsProcedure"].Rows.Add(drBolsegment);
        //                                            xfsuDataSet.Tables["AssociatedConsignmentCustomsProcedure"].Rows[rowchild]["GoodsStatusCode"] =
        //                                                Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["GoodsStatusCode"]);

        //                                        }

        //                                    }
        //                                }

        //                                if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 5 && dsxfsuMessage.Tables[5].Rows.Count > 0)
        //                                {
        //                                    if (xfsuDataSet.Tables.Contains("UtilizedUnitLoadTransportEquipment"))
        //                                    {
        //                                        for (int uldrow = 0; dsxfsuMessage.Tables[5].Rows.Count > uldrow; uldrow++)
        //                                        {
        //                                            if (uldrow == 0)
        //                                            {
        //                                                xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["ID"] =
        //                                                    Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDNumber"]);
        //                                                xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["CharacteristicCode"] =
        //                                                    Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDType"]);
        //                                                xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["OperationalStatusCode"] =
        //                                                    Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDHeightIndicator"]);

        //                                            }
        //                                            if (!(xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Select("AssociatedStatusConsignment_Id=" + rowchild).Length > uldrow))

        //                                            //if (!(xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows.Count > uldrow))
        //                                            {
        //                                                DataRow drBolsegment = xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].NewRow();
        //                                                drBolsegment["ID"] = uldrow;
        //                                                drBolsegment["CharacteristicCode"] = uldrow;
        //                                                drBolsegment["OperationalStatusCode"] = uldrow;

        //                                                drBolsegment["UtilizedUnitLoadTransportEquipment_Id"] = rowchild + "" + uldrow;

        //                                                drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;

        //                                                xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows.Add(drBolsegment);

        //                                                //xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[uldrow]["ID"] =
        //                                                //    Convert.ToString(dsxfsuMessage.Tables[5].Rows[uldrow]["ULDNumber"]);
        //                                                //xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[uldrow]["CharacteristicCode"] =
        //                                                //   Convert.ToString(dsxfsuMessage.Tables[5].Rows[uldrow]["ULDType"]);
        //                                                //xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[uldrow]["OperationalStatusCode"] =
        //                                                //   Convert.ToString(dsxfsuMessage.Tables[5].Rows[uldrow]["ULDHeightIndicator"]);
        //                                                drs = xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].
        //                                                    Select("UtilizedUnitLoadTransportEquipment_Id=" + rowchild + "" + uldrow);
        //                                                if (drs.Length > 0)
        //                                                {
        //                                                    drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[uldrow]["ULDNumber"]);
        //                                                    drs[0]["CharacteristicCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[uldrow]["ULDType"]);
        //                                                    drs[0]["OperationalStatusCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[uldrow]["ULDHeightIndicator"]);

        //                                                }


        //                                            }
        //                                            if (xfsuDataSet.Tables.Contains("OperatingParty"))
        //                                            {
        //                                                //if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                                                //{
        //                                                //    drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0");
        //                                                //    if (drs.Length > 0)
        //                                                //    {
        //                                                //        drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_schemeAgencyID"]);
        //                                                //        drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_PrimaryID_Text"]);
        //                                                //    }
        //                                                //}
        //                                                if (!(xfsuDataSet.Tables["OperatingParty"].Select("UtilizedUnitLoadTransportEquipment_Id="
        //                                                    + rowchild + "" + uldrow).Length > uldrow))

        //                                                // if (!(xfsuDataSet.Tables["OperatingParty"].Rows.Count > uldrow))
        //                                                {
        //                                                    DataRow drMaster = xfsuDataSet.Tables["OperatingParty"].NewRow();
        //                                                    drMaster["OperatingParty_Id"] = rowchild + "" + uldrow; ;
        //                                                    drMaster["UtilizedUnitLoadTransportEquipment_Id"] = rowchild + "" + uldrow;

        //                                                    xfsuDataSet.Tables["OperatingParty"].Rows.Add(drMaster);

        //                                                }
        //                                                if (!(xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id="
        //                                                   + rowchild + "" + uldrow).Length > uldrow))
        //                                                //if (!(xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0").Length > uldrow))
        //                                                {
        //                                                    DataRow drMaster = xfsuDataSet.Tables["PrimaryID"].NewRow();
        //                                                    drMaster["OperatingParty_Id"] = rowchild + "" + uldrow; ;
        //                                                    xfsuDataSet.Tables["PrimaryID"].Rows.Add(drMaster);

        //                                                }
        //                                                drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=" + rowchild + "" + uldrow);
        //                                                if (drs.Length > 0)
        //                                                {
        //                                                    drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[uldrow]["OperatingParty_schemeAgencyID"]);
        //                                                    drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[uldrow]["OperatingParty_PrimaryID_Text"]);
        //                                                }
        //                                            }
        //                                        }
        //                                    }

        //                                }
        //                                else
        //                                {
        //                                    if (xfsuDataSet.Tables.Contains("UtilizedUnitLoadTransportEquipment"))
        //                                    {
        //                                        if (rowchild == 0)
        //                                        {
        //                                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["ID"] =
        //                                                "";
        //                                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["CharacteristicCode"] =
        //                                                "";
        //                                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["OperationalStatusCode"] =
        //                                                "";

        //                                        }
        //                                        if (!(xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows.Count > rowchild))
        //                                        {
        //                                            DataRow drBolsegment = xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].NewRow();
        //                                            drBolsegment["ID"] = rowchild;
        //                                            drBolsegment["CharacteristicCode"] = rowchild;
        //                                            drBolsegment["OperationalStatusCode"] = rowchild;
        //                                            drBolsegment["UtilizedUnitLoadTransportEquipment_Id"] = rowchild;

        //                                            drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;

        //                                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows.Add(drBolsegment);

        //                                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[rowchild]["ID"] =
        //                                                "";
        //                                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[rowchild]["CharacteristicCode"] =
        //                                               "";
        //                                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[rowchild]["OperationalStatusCode"] =
        //                                               "";


        //                                        }
        //                                        if (xfsuDataSet.Tables.Contains("OperatingParty"))
        //                                        {
        //                                            //if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                                            //{
        //                                            //    drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0");
        //                                            //    if (drs.Length > 0)
        //                                            //    {
        //                                            //        drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_schemeAgencyID"]);
        //                                            //        drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_PrimaryID_Text"]);
        //                                            //    }
        //                                            //}
        //                                            if (!(xfsuDataSet.Tables["OperatingParty"].Select("UtilizedUnitLoadTransportEquipment_Id=0").Length > rowchild))
        //                                            {


        //                                                DataRow drMaster = xfsuDataSet.Tables["OperatingParty"].NewRow();
        //                                                drMaster["OperatingParty_Id"] = rowchild;
        //                                                drMaster["UtilizedUnitLoadTransportEquipment_Id"] = rowchild;
        //                                                xfsuDataSet.Tables["OperatingParty"].Rows.Add(drMaster);

        //                                            }

        //                                            if (!(xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0").Length > rowchild))
        //                                            {


        //                                                DataRow drMaster = xfsuDataSet.Tables["PrimaryID"].NewRow();
        //                                                drMaster["OperatingParty_Id"] = rowchild;
        //                                                xfsuDataSet.Tables["PrimaryID"].Rows.Add(drMaster);

        //                                            }
        //                                            drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=" + rowchild);
        //                                            if (drs.Length > 0)
        //                                            {

        //                                                drs[0]["schemeAgencyID"] = "";
        //                                                drs[0]["PrimaryID_Text"] = "";
        //                                            }
        //                                        }
        //                                    }
        //                                }


        //                                ///House AWB s
        //                                if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 6 && dsxfsuMessage.Tables[6].Rows.Count > 0)
        //                                {
        //                                    #region Comment
        //                                    //if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_GrossWeightMeasure"))
        //                                    //{
        //                                    //    drs = xfsuDataSet.Tables["IncludedHouseConsignment_GrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                    //    if (drs.Length > 0)
        //                                    //    {
        //                                    //        drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["UnitofMeasurement"]);
        //                                    //        drs[0]["IncludedHouseConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBWt"]);
        //                                    //    }
        //                                    //}
        //                                    //if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TotalGrossWeightMeasure"))
        //                                    //{
        //                                    //    drs = xfsuDataSet.Tables["IncludedHouseConsignment_TotalGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                    //    if (drs.Length > 0)
        //                                    //    {
        //                                    //        drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["AcceptedUOM"]);
        //                                    //        drs[0]["IncludedHouseConsignment_TotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["GrossWeight"]);
        //                                    //    }
        //                                    //}

        //                                    //xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBPcs"]);
        //                                    //xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TotalPieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["AWBPieces"]);
        //                                    //xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["PiecesIndicator"]);

        //                                    //if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TransportContractDocument"))
        //                                    //{
        //                                    //    drs = xfsuDataSet.Tables["IncludedHouseConsignment_TransportContractDocument"].Select("IncludedHouseConsignment_Id=0");
        //                                    //    if (drs.Length > 0)
        //                                    //    {
        //                                    //        drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBNo"]);
        //                                    //        drs[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWbTypeCode"]);
        //                                    //    }
        //                                    //} 
        //                                    #endregion
        //                                    if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment"))
        //                                    {
        //                                        for (int Hawbrow = 0; Hawbrow < dsxfsuMessage.Tables[6].Rows.Count; Hawbrow++)
        //                                        {
        //                                            if (Hawbrow == 0)
        //                                            {
        //                                                xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["PieceQuantity"] =
        //                                                    Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBPcs"]);
        //                                                xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TotalPieceQuantity"] =
        //                                                    Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBPcs"]);
        //                                                xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TransportSplitDescription"] =
        //                                                     Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBTransportSplitDescription"]);

        //                                            }
        //                                            if (!(xfsuDataSet.Tables["IncludedHouseConsignment"].
        //                                                Select("AssociatedStatusConsignment_Id=" + rowchild).Length > Hawbrow))

        //                                            //if (!(xfsuDataSet.Tables["IncludedHouseConsignment"].Rows.Count > rowchild))
        //                                            {
        //                                                DataRow drBolsegment = xfsuDataSet.Tables["IncludedHouseConsignment"].NewRow();
        //                                                drBolsegment["PieceQuantity"] = Hawbrow;
        //                                                drBolsegment["TotalPieceQuantity"] = Hawbrow;
        //                                                drBolsegment["TransportSplitDescription"] = Hawbrow;
        //                                                drBolsegment["IncludedHouseConsignment_Id"] = rowchild + "" + Hawbrow;

        //                                                drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;

        //                                                xfsuDataSet.Tables["IncludedHouseConsignment"].Rows.Add(drBolsegment);

        //                                                //xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[Hawbrow]["PieceQuantity"] =
        //                                                //    Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBPcs"]);
        //                                                //xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[Hawbrow]["TotalPieceQuantity"] =
        //                                                //    Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBPcs"]);
        //                                                //xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[rowchild]["TransportSplitDescription"] =
        //                                                //   Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBTransportSplitDescription"])

        //                                                drs = xfsuDataSet.Tables["IncludedHouseConsignment"].
        //                                                     Select("IncludedHouseConsignment_Id=" + rowchild + "" + Hawbrow);
        //                                                if (drs.Length > 0)
        //                                                {
        //                                                    drs[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBPcs"]);
        //                                                    drs[0]["TotalPieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBPcs"]);
        //                                                    drs[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBTransportSplitDescription"]);

        //                                                }



        //                                            }
        //                                            if (xfsuDataSet.Tables.Contains("HouseGrossWeightMeasure"))
        //                                            {
        //                                                //if (Hawbrow == 0)
        //                                                //{

        //                                                //    //drs = xfsuDataSet.Tables["HouseGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=" +Hawbrow);
        //                                                //    drs = xfsuDataSet.Tables["HouseGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=" +rowchild);
        //                                                //    if (drs.Length > 0)
        //                                                //    {
        //                                                //        drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["UnitofMeasurement"]); ;
        //                                                //        drs[0]["HouseGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBWt"]); ;
        //                                                //    }

        //                                                //}
        //                                                if (!(xfsuDataSet.Tables["HouseGrossWeightMeasure"].
        //                                                Select("IncludedHouseConsignment_Id=" + rowchild + "" + Hawbrow).Length > Hawbrow))
        //                                                //if (!(xfsuDataSet.Tables["HouseGrossWeightMeasure"].Rows.Count > rowchild))
        //                                                {

        //                                                    DataRow drBolsegment = xfsuDataSet.Tables["HouseGrossWeightMeasure"].NewRow();
        //                                                    drBolsegment["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["UnitofMeasurement"]);
        //                                                    drBolsegment["HouseGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBWt"]); ;
        //                                                    drBolsegment["IncludedHouseConsignment_Id"] = rowchild + "" + Hawbrow;

        //                                                    xfsuDataSet.Tables["HouseGrossWeightMeasure"].Rows.Add(drBolsegment);
        //                                                }
        //                                                drs = xfsuDataSet.Tables["HouseGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=" + rowchild + "" + Hawbrow);
        //                                                if (drs.Length > 0)
        //                                                {
        //                                                    drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["UnitofMeasurement"]); ;
        //                                                    drs[0]["HouseGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBWt"]); ;
        //                                                }
        //                                            }

        //                                            if (xfsuDataSet.Tables.Contains("HouseTotalGrossWeightMeasure"))
        //                                            {
        //                                                //if (Hawbrow == 0)
        //                                                //{

        //                                                //    drs = xfsuDataSet.Tables["HouseTotalGrossWeightMeasure"].Select("IncludedHouseConsignment_Id="+rowchild);
        //                                                //    if (drs.Length > 0)
        //                                                //    {
        //                                                //        drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["UnitofMeasurement"]);
        //                                                //        drs[0]["HouseTotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBWt"]);
        //                                                //    }

        //                                                //}
        //                                                if (!(xfsuDataSet.Tables["HouseTotalGrossWeightMeasure"].
        //                                                Select("IncludedHouseConsignment_Id=" + rowchild + "" + Hawbrow).Length > Hawbrow))
        //                                                // if (!(xfsuDataSet.Tables["HouseTotalGrossWeightMeasure"].Rows.Count > rowchild))
        //                                                {

        //                                                    DataRow drBolsegment = xfsuDataSet.Tables["HouseTotalGrossWeightMeasure"].NewRow();
        //                                                    drBolsegment["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["UnitofMeasurement"]); ;
        //                                                    drBolsegment["HouseTotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBWt"]);
        //                                                    drBolsegment["IncludedHouseConsignment_Id"] = rowchild + "" + Hawbrow;

        //                                                    xfsuDataSet.Tables["HouseTotalGrossWeightMeasure"].Rows.Add(drBolsegment);
        //                                                }
        //                                                drs = xfsuDataSet.Tables["HouseTotalGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=" + rowchild + "" + Hawbrow);
        //                                                if (drs.Length > 0)
        //                                                {
        //                                                    drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["UnitofMeasurement"]); ;
        //                                                    drs[0]["HouseTotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBWt"]); ;
        //                                                }
        //                                            }


        //                                            if (xfsuDataSet.Tables.Contains("HouseTransportContractDocument"))
        //                                            {
        //                                                if (Hawbrow == 0)
        //                                                {
        //                                                    xfsuDataSet.Tables["HouseTransportContractDocument"].Rows[0]["ID"] =
        //                                                        Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBNo"]);
        //                                                    xfsuDataSet.Tables["HouseTransportContractDocument"].Rows[0]["TypeCode"] =
        //                                                        Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWbTypeCode"]);
        //                                                }
        //                                                if (!(xfsuDataSet.Tables["HouseTransportContractDocument"].Select("IncludedHouseConsignment_Id="
        //                                                    + rowchild + "" + Hawbrow).Length > Hawbrow))
        //                                                //if (!(xfsuDataSet.Tables["HouseTransportContractDocument"].Rows.Count > rowchild))
        //                                                {

        //                                                    DataRow drBolsegment = xfsuDataSet.Tables["HouseTransportContractDocument"].NewRow();
        //                                                    drBolsegment["ID"] = "";
        //                                                    drBolsegment["TypeCode"] = "";
        //                                                    drBolsegment["IncludedHouseConsignment_Id"] = rowchild + "" + Hawbrow;

        //                                                    xfsuDataSet.Tables["HouseTransportContractDocument"].Rows.Add(drBolsegment);



        //                                                }
        //                                                drs = xfsuDataSet.Tables["HouseTransportContractDocument"].Select("IncludedHouseConsignment_Id=" + rowchild + "" + Hawbrow);
        //                                                if (drs.Length > 0)
        //                                                {
        //                                                    drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBNo"]);
        //                                                    drs[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWbTypeCode"]);
        //                                                }
        //                                            }


        //                                        }
        //                                    }

        //                                }
        //                                else
        //                                {
        //                                    #region comment
        //                                    //if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_GrossWeightMeasure"))
        //                                    //{
        //                                    //    drs = xfsuDataSet.Tables["IncludedHouseConsignment_GrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                    //    if (drs.Length > 0)
        //                                    //    {
        //                                    //        drs[0]["unitCode"] = "";
        //                                    //        drs[0]["IncludedHouseConsignment_GrossWeightMeasure_Text"] = "";
        //                                    //    }
        //                                    //}
        //                                    //if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TotalGrossWeightMeasure"))
        //                                    //{
        //                                    //    drs = xfsuDataSet.Tables["IncludedHouseConsignment_TotalGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                    //    if (drs.Length > 0)
        //                                    //    {
        //                                    //        drs[0]["unitCode"] = "";
        //                                    //        drs[0]["IncludedHouseConsignment_TotalGrossWeightMeasure_Text"] = "";
        //                                    //    }
        //                                    //}


        //                                    //xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["PieceQuantity"] = "";
        //                                    //xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TotalPieceQuantity"] = "";
        //                                    //xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TransportSplitDescription"] = "";

        //                                    //if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TransportContractDocument"))
        //                                    //{
        //                                    //    drs = xfsuDataSet.Tables["IncludedHouseConsignment_TransportContractDocument"].Select("IncludedHouseConsignment_Id=0");
        //                                    //    if (drs.Length > 0)
        //                                    //    {
        //                                    //        drs[0]["ID"] = "";
        //                                    //        drs[0]["TypeCode"] = "";
        //                                    //    }
        //                                    //} 
        //                                    #endregion
        //                                    if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment"))
        //                                    {
        //                                        if (rowchild == 0)
        //                                        {
        //                                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["PieceQuantity"] =
        //                                                "";
        //                                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TotalPieceQuantity"] =
        //                                                "";
        //                                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TransportSplitDescription"] =
        //                                                "";



        //                                        }
        //                                        if (!(xfsuDataSet.Tables["IncludedHouseConsignment"].Rows.Count > rowchild))
        //                                        {
        //                                            DataRow drBolsegment = xfsuDataSet.Tables["IncludedHouseConsignment"].NewRow();
        //                                            drBolsegment["PieceQuantity"] = rowchild;
        //                                            drBolsegment["TotalPieceQuantity"] = rowchild;
        //                                            drBolsegment["TransportSplitDescription"] = rowchild;
        //                                            drBolsegment["IncludedHouseConsignment_Id"] = rowchild;

        //                                            drBolsegment["AssociatedStatusConsignment_Id"] = rowchild;

        //                                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows.Add(drBolsegment);

        //                                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[rowchild]["PieceQuantity"] =
        //                                                "";
        //                                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[rowchild]["TotalPieceQuantity"] =
        //                                               "";
        //                                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[rowchild]["TransportSplitDescription"] =
        //                                               "";


        //                                        }
        //                                        if (xfsuDataSet.Tables.Contains("HouseGrossWeightMeasure"))
        //                                        {
        //                                            if (rowchild == 0)
        //                                            {

        //                                                drs = xfsuDataSet.Tables["HouseGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                                if (drs.Length > 0)
        //                                                {
        //                                                    drs[0]["UnitCode"] = "";
        //                                                    drs[0]["HouseGrossWeightMeasure_Text"] = "";
        //                                                }

        //                                            }
        //                                            if (!(xfsuDataSet.Tables["HouseGrossWeightMeasure"].Rows.Count > rowchild))
        //                                            {

        //                                                DataRow drBolsegment = xfsuDataSet.Tables["HouseGrossWeightMeasure"].NewRow();
        //                                                drBolsegment["UnitCode"] = "";
        //                                                drBolsegment["HouseGrossWeightMeasure_Text"] = "";
        //                                                drBolsegment["IncludedHouseConsignment_Id"] = rowchild;

        //                                                xfsuDataSet.Tables["HouseGrossWeightMeasure"].Rows.Add(drBolsegment);
        //                                            }
        //                                        }
        //                                        //----------------
        //                                        if (xfsuDataSet.Tables.Contains("HouseTotalGrossWeightMeasure"))
        //                                        {
        //                                            if (rowchild == 0)
        //                                            {

        //                                                drs = xfsuDataSet.Tables["HouseTotalGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                                if (drs.Length > 0)
        //                                                {
        //                                                    drs[0]["UnitCode"] = "";
        //                                                    drs[0]["HouseTotalGrossWeightMeasure_Text"] = "";
        //                                                }

        //                                            }
        //                                            if (!(xfsuDataSet.Tables["HouseTotalGrossWeightMeasure"].Rows.Count > rowchild))
        //                                            {

        //                                                DataRow drBolsegment = xfsuDataSet.Tables["HouseTotalGrossWeightMeasure"].NewRow();
        //                                                drBolsegment["UnitCode"] = "";
        //                                                drBolsegment["HouseTotalGrossWeightMeasure_Text"] = "";
        //                                                drBolsegment["IncludedHouseConsignment_Id"] = rowchild;

        //                                                xfsuDataSet.Tables["HouseTotalGrossWeightMeasure"].Rows.Add(drBolsegment);
        //                                            }
        //                                        }
        //                                        if (xfsuDataSet.Tables.Contains("HouseTransportContractDocument"))
        //                                        {
        //                                            if (rowchild == 0)
        //                                            {
        //                                                xfsuDataSet.Tables["HouseTransportContractDocument"].Rows[0]["ID"] = "";
        //                                                xfsuDataSet.Tables["HouseTransportContractDocument"].Rows[0]["TypeCode"] = "";
        //                                            }
        //                                            if (!(xfsuDataSet.Tables["HouseTransportContractDocument"].Rows.Count > rowchild))
        //                                            {

        //                                                DataRow drBolsegment = xfsuDataSet.Tables["HouseTransportContractDocument"].NewRow();
        //                                                drBolsegment["ID"] = "";
        //                                                drBolsegment["TypeCode"] = "";
        //                                                drBolsegment["IncludedHouseConsignment_Id"] = rowchild;

        //                                                xfsuDataSet.Tables["HouseTransportContractDocument"].Rows.Add(drBolsegment);
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }

        //                        }
        //                    }
        //                    string generatMessage = xfsuDataSet.GetXml();
        //                    xfsuDataSet.Dispose();
        //                    sbgenerateXFSUMessage = new StringBuilder(generatMessage);
        //                    sbgenerateXFSUMessage.Replace(" xmlns: ram = \"iata: datamodel:3\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:rsm=\"iata: waybill:1\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:ram=\"iata: datamodel:3\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xsi:schemaLocation=\"iata: waybill:1 Waybill_1.xsd\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns: ram = \"iata: datamodel:3\"", "");
        //                    sbgenerateXFSUMessage.Replace("xmlns: ram = \"iata: datamodel:3\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:rsm=\"iata:waybill:1\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:ram=\"iata:datamodel:3\"", "");

        //                    //Replace Nodes
        //                    sbgenerateXFSUMessage.Replace("ram:MasterConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
        //                    sbgenerateXFSUMessage.Replace("ram:AssociatedStatusGrossWeightMeasure", "ram:GrossWeightMeasure");
        //                    sbgenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
        //                    sbgenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_TransportContractDocument", "ram:TransportContractDocument");
        //                    sbgenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_TotalGrossWeightMeasure", "ram:TotalGrossWeightMeasure");
        //                    sbgenerateXFSUMessage.Replace("ram:AssociatedStatusConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");

        //                    sbgenerateXFSUMessage.Insert(18, " xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ccts=\"urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2\" xmlns:udt=\"urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8\" xmlns:ram=\"iata:datamodel:3\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"iata:statusmessage:1 StatusMessage_1.xsd\"");
        //                    sbgenerateXFSUMessage.Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");

        //                }
        //            }
        //            //else
        //            //{
        //            //    sbgenerateXFSUMessage.Append("No Message format available in the system.");
        //            //}
        //        }
        //        //else
        //        //{
        //        //    sbgenerateXFSUMessage.Append("No Data available in the system to generate message.");
        //        //}
        //    }

        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //    }

        //    return sbgenerateXFSUMessage.ToString();
        //}

        /*Not in use*/
        //     public string GenerateXFSUMANDEPMessageofTheAWB(string AWBPrefix, string AWBNumber, string orgDest, string messageType,
        //string flightNo, string flightDate, string doNumber = "", string EventDate = "1900-01-01")
        //     {
        //         StringBuilder sbgenerateXFSUMessage = new StringBuilder();


        //         try
        //         {
        //             DataSet dsxfsuMessage = new DataSet();
        //             dsxfsuMessage = GetAWBRecordforGenerateXFSUMessage(AWBPrefix, AWBNumber, orgDest, messageType, doNumber, flightNo, flightDate, 0, 0.00);

        //             if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 0 && dsxfsuMessage.Tables[0].Rows.Count > 0 && dsxfsuMessage.Tables[1].Rows.Count > 0)
        //             {
        //                 var xfsuDataSet = new DataSet();
        //                 DataRow[] drs;
        //                 var xmlSchema = genericFunction.GetXMLMessageData("XFSU");
        //                 if (xmlSchema != null && xmlSchema.Tables.Count > 0 && xmlSchema.Tables[0].Rows.Count > 0)
        //                 {
        //                     string messageXML = xmlSchema.Tables[0].Rows[0]["XMLMessageData"].ToString();
        //                     messageXML = ReplacingNodeNames(messageXML);
        //                     var txMessage = new StringReader(messageXML);
        //                     xfsuDataSet.ReadXml(txMessage);

        //                     if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 0 && dsxfsuMessage.Tables[0].Rows.Count > 0)
        //                     {
        //                         xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBNumber"]);
        //                         xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageName"]);
        //                         xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageTypeCode"]);
        //                         xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["IssueDateTime"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageCreatedDate"]);
        //                         xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["PurposeCode"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["PurposeCode"]);
        //                         xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["VersionNumber"]);
        //                         xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["ConversationID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["ConversionID"]);

        //                         //SenderParty
        //                         if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                         {
        //                             drs = xfsuDataSet.Tables["PrimaryID"].Select("SenderParty_Id=0");
        //                             if (drs.Length > 0)
        //                             {
        //                                 drs[0]["schemeID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["SenderParty_PrimaryID"]);
        //                                 drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["SenderParty_PrimaryIDText"]);
        //                             }
        //                         }
        //                         //RecipientParty
        //                         if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                         {
        //                             drs = xfsuDataSet.Tables["PrimaryID"].Select("RecipientParty_Id=0");
        //                             if (drs.Length > 0)
        //                             {
        //                                 drs[0]["schemeID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["RecipientParty_PrimaryID"]);
        //                                 drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["RecipientParty_PrimaryIDText"]);
        //                             }
        //                         }

        //                         //BusinessHeaderDocument
        //                         xfsuDataSet.Tables["BusinessHeaderDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["NoofStatusUpdate"]);

        //                         if (xfsuDataSet.Tables["BusinessHeaderDocument"].Columns.Contains("Reference"))
        //                         {
        //                             xfsuDataSet.Tables["BusinessHeaderDocument"].Rows[0]["Reference"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["Reference"]);
        //                         }

        //                         //MasterConsignment
        //                         if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 1 && dsxfsuMessage.Tables[1].Rows.Count > 0)
        //                         {
        //                             if (xfsuDataSet.Tables.Contains("MasterConsignment_GrossWeightMeasure"))
        //                             {
        //                                 drs = xfsuDataSet.Tables["MasterConsignment_GrossWeightMeasure"].Select("MasterConsignment_Id=0");
        //                                 if (drs.Length > 0)
        //                                 {
        //                                     drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedUOM"]);
        //                                     drs[0]["MasterConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBGrossWeight"]);
        //                                 }
        //                             }

        //                             if (xfsuDataSet.Tables.Contains("TotalGrossWeightMeasure"))
        //                             {
        //                                 drs = xfsuDataSet.Tables["TotalGrossWeightMeasure"].Select("MasterConsignment_Id=0");
        //                                 if (drs.Length > 0)
        //                                 {
        //                                     drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedUOM"]);
        //                                     drs[0]["TotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBGrossWeight"]);
        //                                 }
        //                             }

        //                             xfsuDataSet.Tables["MasterConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBPieces"]);
        //                             xfsuDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBPieces"]);
        //                             xfsuDataSet.Tables["MasterConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["PartConsignment"]);

        //                             // AWBNumber Section
        //                             if (xfsuDataSet.Tables.Contains("TransportContractDocument"))
        //                             {
        //                                 drs = xfsuDataSet.Tables["TransportContractDocument"].Select("MasterConsignment_Id=0");
        //                                 if (drs.Length > 0)
        //                                 {
        //                                     drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBNumber"]);
        //                                     drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AirwayBillDocumentName"]);
        //                                     drs[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBDocumentTypeCode"]);
        //                                 }
        //                             }

        //                             // AWBNumber Origin Section
        //                             if (xfsuDataSet.Tables.Contains("OriginLocation"))
        //                             {
        //                                 drs = xfsuDataSet.Tables["OriginLocation"].Select("MasterConsignment_Id=0");
        //                                 if (drs.Length > 0)
        //                                 {
        //                                     drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBOrigin"]);
        //                                     drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["OriginAirportName"]);
        //                                 }
        //                             }

        //                             //AWBNumber Destination Section
        //                             if (xfsuDataSet.Tables.Contains("FinalDestinationLocation"))
        //                             {
        //                                 drs = xfsuDataSet.Tables["FinalDestinationLocation"].Select("MasterConsignment_Id=0");
        //                                 if (drs.Length > 0)
        //                                 {
        //                                     drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWbDestination"]);
        //                                     drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DestinationAirportName"]);
        //                                 }
        //                             }

        //                             if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 3 && dsxfsuMessage.Tables[3].Rows.Count > 0)
        //                             {//LOOP
        //                                 //AWBRouter Information
        //                                 //xfsuDataSet.Tables["RoutingLocation"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FltDestination"]);
        //                                 //xfsuDataSet.Tables["RoutingLocation"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["AirportName"]);
        //                                 for (int row = 1; row < dsxfsuMessage.Tables[3].Rows.Count; row++)
        //                                 {
        //                                     //if (!(xfsuDataSet.Tables["MasterConsignment"].Rows.Count > row))
        //                                     //{
        //                                     //    DataRow drBolsegment = xfsuDataSet.Tables["MasterConsignment"].NewRow();
        //                                     //    drBolsegment["MasterConsignment_Id"] = row;

        //                                     //    xfsuDataSet.Tables["MasterConsignment"].Rows.Add(drBolsegment);
        //                                     //}
        //                                     if (row == 1)
        //                                     {
        //                                         drs = xfsuDataSet.Tables["RoutingLocation"].Select("MasterConsignment_Id=0");
        //                                         if (drs.Length > 0)
        //                                         {
        //                                             drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[row]["FltDestination"]);
        //                                             drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[row]["AirportName"]);
        //                                         }
        //                                     }
        //                                     if (!(xfsuDataSet.Tables["RoutingLocation"].Rows.Count >= row))
        //                                     {

        //                                         DataRow drBolsegment = xfsuDataSet.Tables["RoutingLocation"].NewRow();
        //                                         drBolsegment["ID"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[row]["FltDestination"]);
        //                                         drBolsegment["Name"] = dsxfsuMessage.Tables[3].Rows[row]["AirportName"];
        //                                         drBolsegment["MasterConsignment_Id"] = 0;
        //                                         xfsuDataSet.Tables["RoutingLocation"].Rows.Add(drBolsegment);
        //                                     }

        //                                 }

        //                             }


        //                             //AWBStatus Code
        //                             xfsuDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["MessageCode"]);


        //                             if (xfsuDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
        //                             {
        //                                 drs = xfsuDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
        //                                 if (drs.Length > 0)
        //                                 {
        //                                     drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedUOM"]);
        //                                     drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["Manifestdweight"]);
        //                                 }
        //                             }

        //                             if (xfsuDataSet.Tables.Contains("GrossVolumeMeasure"))
        //                             {
        //                                 drs = xfsuDataSet.Tables["GrossVolumeMeasure"].Select("AssociatedStatusConsignment_Id=0");
        //                                 if (drs.Length > 0)
        //                                 {
        //                                     drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBVolumeMeasue"]);
        //                                     drs[0]["GrossVolumeMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["VolumetricWeight"]);
        //                                 }
        //                             }

        //                             xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["DensityGroupCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DensityCode"]);
        //                             xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["manifestedpcs"]);
        //                             xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["PartConsignment"]);
        //                             xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["DiscrepancyDescriptionCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBDiscrepancyCode"]);
        //                             xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["StatusDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["StatusDescription"]);


        //                             if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 2 && dsxfsuMessage.Tables[2].Rows.Count > 0)
        //                             {
        //                                 xfsuDataSet.Tables["AssociatedManifestDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ManifestNumber"]);

        //                                 xfsuDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows[0]["ServiceTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ProductType"]);

        //                                 xfsuDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightNumber"]);

        //                                 xfsuDataSet.Tables["UsedLogisticsTransportMeans"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["CarrierName"]);

        //                                 xfsuDataSet.Tables["UsedLogisticsTransportMeans"].Rows[0]["Type"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["AirCraftTypeMaster"]);


        //                                 xfsuDataSet.Tables["ScheduledArrivalEvent"].Rows[0]["ScheduledOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightScheduleArrivlaTime"]);

        //                                 xfsuDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightArriveTime"]);
        //                                 xfsuDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalDateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ScheduleArrivalIndicator"]);

        //                                 xfsuDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightScheduleDepartureTime"]);
        //                                 xfsuDataSet.Tables["DepartureEvent"].Rows[0]["DepartureDateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["EstimatedDepTime"]);

        //                                 if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                                 {
        //                                     drs = xfsuDataSet.Tables["PrimaryID"].Select("CarrierParty_Id=0");
        //                                     if (drs.Length > 0)
        //                                     {
        //                                         drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["CarrierCode"]);
        //                                         drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["CarrierName"]);
        //                                     }
        //                                 }

        //                                 xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightOrigin"]);
        //                                 xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["AirportName"]);
        //                                 xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["LocationType"]);
        //                                 xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["FlightStatusTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightStatusTypeCode"]);

        //                                 //xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["SpecifiedEvent"]);
        //                                 xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"] = Convert.ToString(EventDate);
        //                                 xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["DateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["DateTimeTypeCode"]);

        //                             }

        //                             xfsuDataSet.Tables["NotifiedParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["NotifyName"]);
        //                             xfsuDataSet.Tables["DeliveryParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["ConsigneeName"]);



        //                             if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                             {
        //                                 drs = xfsuDataSet.Tables["PrimaryID"].Select("AssociatedReceivedFromParty_Id=0");
        //                                 if (drs.Length > 0)
        //                                 {
        //                                     drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_schemeAgencyID"]);
        //                                     drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_PrimaryID_Text"]);
        //                                 }
        //                             }
        //                             xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssociatedReceivedAirlineName"]);
        //                             xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["RoleCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_RoleCode"]);
        //                             xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Role"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_Role"]);

        //                             if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                             {
        //                                 drs = xfsuDataSet.Tables["PrimaryID"].Select("AssociatedTransferredFromParty_Id=0");
        //                                 if (drs.Length > 0)
        //                                 {
        //                                     drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_schemeAgencyID"]);
        //                                     drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_PrimaryID_Text"]);
        //                                 }
        //                             }
        //                             xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_Name"]);
        //                             xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["RoleCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_RoleCode"]);
        //                             xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["Role"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_Role"]);


        //                             xfsuDataSet.Tables["HandlingOSIInstructions"].Rows[0]["Description"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["HandlingInfo"]);
        //                             xfsuDataSet.Tables["HandlingOSIInstructions"].Rows[0]["DescriptionCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["SHCCodes"]);

        //                             if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 4 && dsxfsuMessage.Tables[4].Rows.Count > 0)
        //                             {
        //                                 xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["ContentCode"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["ContentCode"]);
        //                                 xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["Content"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["Content"]);
        //                                 xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["SubjectCode"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["SubjectCode"]);
        //                                 xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["CountryID"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["CountryID"]);
        //                                 xfsuDataSet.Tables["AssociatedConsignmentCustomsProcedure"].Rows[0]["GoodsStatusCode"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["GoodsStatusCode"]);
        //                             }

        //                             if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 5 && dsxfsuMessage.Tables[5].Rows.Count > 0)
        //                             {
        //                                 for (int i = 0; i < dsxfsuMessage.Tables[5].Rows.Count; i++)
        //                                 {
        //                                     if (i == 0)
        //                                     {
        //                                         xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDNumber"]);
        //                                         xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["CharacteristicCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDType"]);
        //                                         xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["OperationalStatusCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDHeightIndicator"]);
        //                                         if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                                         {
        //                                             drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0");
        //                                             if (drs.Length > 0)
        //                                             {
        //                                                 drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_schemeAgencyID"]);
        //                                                 drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_PrimaryID_Text"]);
        //                                             }
        //                                         }
        //                                     }
        //                                     if (!(xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Select("UtilizedUnitLoadTransportEquipment_Id=" + i).Length > i))


        //                                     {
        //                                         DataRow drBolsegment = xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].NewRow();
        //                                         drBolsegment["ID"] = i;
        //                                         drBolsegment["CharacteristicCode"] = i;
        //                                         drBolsegment["OperationalStatusCode"] = i;

        //                                         drBolsegment["UtilizedUnitLoadTransportEquipment_Id"] = i;

        //                                         drBolsegment["AssociatedStatusConsignment_Id"] = 0;

        //                                         xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows.Add(drBolsegment);

        //                                         //xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[uldrow]["ID"] =
        //                                         //    Convert.ToString(dsxfsuMessage.Tables[5].Rows[uldrow]["ULDNumber"]);
        //                                         //xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[uldrow]["CharacteristicCode"] =
        //                                         //   Convert.ToString(dsxfsuMessage.Tables[5].Rows[uldrow]["ULDType"]);
        //                                         //xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[uldrow]["OperationalStatusCode"] =
        //                                         //   Convert.ToString(dsxfsuMessage.Tables[5].Rows[uldrow]["ULDHeightIndicator"]);
        //                                         drs = xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].
        //                                             Select("UtilizedUnitLoadTransportEquipment_Id=" + i);
        //                                         if (drs.Length > 0)
        //                                         {
        //                                             drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[i]["ULDNumber"]);
        //                                             drs[0]["CharacteristicCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[i]["ULDType"]);
        //                                             drs[0]["OperationalStatusCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[i]["ULDHeightIndicator"]);

        //                                         }


        //                                     }
        //                                     if (xfsuDataSet.Tables.Contains("OperatingParty"))
        //                                     {
        //                                         //if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                                         //{
        //                                         //    drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0");
        //                                         //    if (drs.Length > 0)
        //                                         //    {
        //                                         //        drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_schemeAgencyID"]);
        //                                         //        drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_PrimaryID_Text"]);
        //                                         //    }
        //                                         //}
        //                                         if (!(xfsuDataSet.Tables["OperatingParty"].Select("UtilizedUnitLoadTransportEquipment_Id="
        //                                             + i).Length > i))

        //                                         // if (!(xfsuDataSet.Tables["OperatingParty"].Rows.Count > uldrow))
        //                                         {
        //                                             DataRow drMaster = xfsuDataSet.Tables["OperatingParty"].NewRow();
        //                                             drMaster["OperatingParty_Id"] = i;
        //                                             drMaster["UtilizedUnitLoadTransportEquipment_Id"] = i;

        //                                             xfsuDataSet.Tables["OperatingParty"].Rows.Add(drMaster);

        //                                         }
        //                                         if (!(xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id="
        //                                            + i).Length > i))
        //                                         //if (!(xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0").Length > uldrow))
        //                                         {
        //                                             DataRow drMaster = xfsuDataSet.Tables["PrimaryID"].NewRow();
        //                                             drMaster["OperatingParty_Id"] = i;
        //                                             xfsuDataSet.Tables["PrimaryID"].Rows.Add(drMaster);

        //                                         }
        //                                         drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=" + i);
        //                                         if (drs.Length > 0)
        //                                         {
        //                                             drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[i]["OperatingParty_schemeAgencyID"]);
        //                                             drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[i]["OperatingParty_PrimaryID_Text"]);
        //                                         }
        //                                     }
        //                                 }
        //                             }
        //                             else
        //                             {
        //                                 xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["ID"] = "";
        //                                 xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["CharacteristicCode"] = "";
        //                                 xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["OperationalStatusCode"] = "";
        //                                 if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                                 {
        //                                     drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0");
        //                                     if (drs.Length > 0)
        //                                     {
        //                                         drs[0]["schemeAgencyID"] = "";
        //                                         drs[0]["PrimaryID_Text"] = "";
        //                                     }
        //                                 }



        //                             }

        //                             ///House AWB s
        //                             if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 6 && dsxfsuMessage.Tables[6].Rows.Count > 0)
        //                             {
        //                                 for (int Hawbrow = 0; Hawbrow < dsxfsuMessage.Tables[6].Rows.Count; Hawbrow++)
        //                                 {
        //                                     if (Hawbrow == 0)
        //                                     {
        //                                         xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["PieceQuantity"] =
        //                                             Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["ManifestedPCS"]);
        //                                         xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TotalPieceQuantity"] =
        //                                             Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBPcs"]);
        //                                         xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TransportSplitDescription"] =
        //                                              Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBTransportSplitDescription"]);

        //                                     }
        //                                     if (!(xfsuDataSet.Tables["IncludedHouseConsignment"].
        //                                         Select("AssociatedStatusConsignment_Id=" + Hawbrow).Length > Hawbrow))

        //                                     //if (!(xfsuDataSet.Tables["IncludedHouseConsignment"].Rows.Count > rowchild))
        //                                     {
        //                                         DataRow drBolsegment = xfsuDataSet.Tables["IncludedHouseConsignment"].NewRow();
        //                                         drBolsegment["PieceQuantity"] = Hawbrow;
        //                                         drBolsegment["TotalPieceQuantity"] = Hawbrow;
        //                                         drBolsegment["TransportSplitDescription"] = Hawbrow;
        //                                         drBolsegment["IncludedHouseConsignment_Id"] = Hawbrow;

        //                                         drBolsegment["AssociatedStatusConsignment_Id"] = 0;

        //                                         xfsuDataSet.Tables["IncludedHouseConsignment"].Rows.Add(drBolsegment);

        //                                         //xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[Hawbrow]["PieceQuantity"] =
        //                                         //    Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBPcs"]);
        //                                         //xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[Hawbrow]["TotalPieceQuantity"] =
        //                                         //    Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBPcs"]);
        //                                         //xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[rowchild]["TransportSplitDescription"] =
        //                                         //   Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBTransportSplitDescription"])

        //                                         drs = xfsuDataSet.Tables["IncludedHouseConsignment"].
        //                                              Select("IncludedHouseConsignment_Id=" + Hawbrow);
        //                                         if (drs.Length > 0)
        //                                         {
        //                                             drs[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["ManifestedPCS"]);
        //                                             drs[0]["TotalPieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBPcs"]);
        //                                             drs[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBTransportSplitDescription"]);

        //                                         }



        //                                     }
        //                                     if (xfsuDataSet.Tables.Contains("HouseGrossWeightMeasure"))
        //                                     {
        //                                         //if (Hawbrow == 0)
        //                                         //{

        //                                         //    //drs = xfsuDataSet.Tables["HouseGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=" +Hawbrow);
        //                                         //    drs = xfsuDataSet.Tables["HouseGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=" +rowchild);
        //                                         //    if (drs.Length > 0)
        //                                         //    {
        //                                         //        drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["UnitofMeasurement"]); ;
        //                                         //        drs[0]["HouseGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBWt"]); ;
        //                                         //    }

        //                                         //}
        //                                         if (!(xfsuDataSet.Tables["HouseGrossWeightMeasure"].
        //                                         Select("IncludedHouseConsignment_Id=" + Hawbrow).Length > Hawbrow))
        //                                         //if (!(xfsuDataSet.Tables["HouseGrossWeightMeasure"].Rows.Count > rowchild))
        //                                         {

        //                                             DataRow drBolsegment = xfsuDataSet.Tables["HouseGrossWeightMeasure"].NewRow();
        //                                             drBolsegment["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["UnitofMeasurement"]);
        //                                             drBolsegment["HouseGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["ManifestedWT"]); ;
        //                                             drBolsegment["IncludedHouseConsignment_Id"] = Hawbrow;

        //                                             xfsuDataSet.Tables["HouseGrossWeightMeasure"].Rows.Add(drBolsegment);
        //                                         }
        //                                         drs = xfsuDataSet.Tables["HouseGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=" + Hawbrow);
        //                                         if (drs.Length > 0)
        //                                         {
        //                                             drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["UnitofMeasurement"]); ;
        //                                             drs[0]["HouseGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBWt"]); ;
        //                                         }
        //                                     }

        //                                     if (xfsuDataSet.Tables.Contains("HouseTotalGrossWeightMeasure"))
        //                                     {
        //                                         //if (Hawbrow == 0)
        //                                         //{

        //                                         //    drs = xfsuDataSet.Tables["HouseTotalGrossWeightMeasure"].Select("IncludedHouseConsignment_Id="+rowchild);
        //                                         //    if (drs.Length > 0)
        //                                         //    {
        //                                         //        drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["UnitofMeasurement"]);
        //                                         //        drs[0]["HouseTotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBWt"]);
        //                                         //    }

        //                                         //}
        //                                         if (!(xfsuDataSet.Tables["HouseTotalGrossWeightMeasure"].
        //                                         Select("IncludedHouseConsignment_Id=" + Hawbrow).Length > Hawbrow))
        //                                         // if (!(xfsuDataSet.Tables["HouseTotalGrossWeightMeasure"].Rows.Count > rowchild))
        //                                         {

        //                                             DataRow drBolsegment = xfsuDataSet.Tables["HouseTotalGrossWeightMeasure"].NewRow();
        //                                             drBolsegment["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["UnitofMeasurement"]); ;
        //                                             drBolsegment["HouseTotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBWt"]);
        //                                             drBolsegment["IncludedHouseConsignment_Id"] = +Hawbrow;

        //                                             xfsuDataSet.Tables["HouseTotalGrossWeightMeasure"].Rows.Add(drBolsegment);
        //                                         }
        //                                         drs = xfsuDataSet.Tables["HouseTotalGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=" + Hawbrow);
        //                                         if (drs.Length > 0)
        //                                         {
        //                                             drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["UnitofMeasurement"]); ;
        //                                             drs[0]["HouseTotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBWt"]); ;
        //                                         }
        //                                     }


        //                                     if (xfsuDataSet.Tables.Contains("HouseTransportContractDocument"))
        //                                     {
        //                                         if (Hawbrow == 0)
        //                                         {
        //                                             xfsuDataSet.Tables["HouseTransportContractDocument"].Rows[0]["ID"] =
        //                                                 Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBNo"]);
        //                                             xfsuDataSet.Tables["HouseTransportContractDocument"].Rows[0]["TypeCode"] =
        //                                                 Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWbTypeCode"]);
        //                                         }
        //                                         if (!(xfsuDataSet.Tables["HouseTransportContractDocument"].Select("IncludedHouseConsignment_Id="
        //                                             + Hawbrow).Length > Hawbrow))
        //                                         //if (!(xfsuDataSet.Tables["HouseTransportContractDocument"].Rows.Count > rowchild))
        //                                         {

        //                                             DataRow drBolsegment = xfsuDataSet.Tables["HouseTransportContractDocument"].NewRow();
        //                                             drBolsegment["ID"] = "";
        //                                             drBolsegment["TypeCode"] = "";
        //                                             drBolsegment["IncludedHouseConsignment_Id"] = +Hawbrow;

        //                                             xfsuDataSet.Tables["HouseTransportContractDocument"].Rows.Add(drBolsegment);



        //                                         }
        //                                         drs = xfsuDataSet.Tables["HouseTransportContractDocument"].Select("IncludedHouseConsignment_Id=" + Hawbrow);
        //                                         if (drs.Length > 0)
        //                                         {
        //                                             drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWBNo"]);
        //                                             drs[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[Hawbrow]["HAWbTypeCode"]);
        //                                         }
        //                                     }


        //                                 }
        //                             }
        //                             else
        //                             {
        //                                 if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_GrossWeightMeasure"))
        //                                 {
        //                                     drs = xfsuDataSet.Tables["IncludedHouseConsignment_GrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                     if (drs.Length > 0)
        //                                     {
        //                                         drs[0]["unitCode"] = "KGM";
        //                                         drs[0]["IncludedHouseConsignment_GrossWeightMeasure_Text"] = "0";
        //                                     }
        //                                 }
        //                                 if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TotalGrossWeightMeasure"))
        //                                 {
        //                                     drs = xfsuDataSet.Tables["IncludedHouseConsignment_TotalGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                     if (drs.Length > 0)
        //                                     {
        //                                         drs[0]["unitCode"] = "KGM";
        //                                         drs[0]["IncludedHouseConsignment_TotalGrossWeightMeasure_Text"] = "0";
        //                                     }
        //                                 }


        //                                 xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["PieceQuantity"] = "";
        //                                 xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TotalPieceQuantity"] = "";
        //                                 xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TransportSplitDescription"] = "";

        //                                 if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TransportContractDocument"))
        //                                 {
        //                                     drs = xfsuDataSet.Tables["IncludedHouseConsignment_TransportContractDocument"].Select("IncludedHouseConsignment_Id=0");
        //                                     if (drs.Length > 0)
        //                                     {
        //                                         drs[0]["ID"] = "";
        //                                         drs[0]["TypeCode"] = "";
        //                                     }
        //                                 }
        //                             }
        //                         }
        //                         string generatMessage = xfsuDataSet.GetXml();
        //                         xfsuDataSet.Dispose();
        //                         sbgenerateXFSUMessage = new StringBuilder(generatMessage);
        //                         sbgenerateXFSUMessage.Replace(" xmlns: ram = \"iata: datamodel:3\"", "");
        //                         sbgenerateXFSUMessage.Replace(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
        //                         sbgenerateXFSUMessage.Replace(" xmlns:rsm=\"iata: waybill:1\"", "");
        //                         sbgenerateXFSUMessage.Replace(" xmlns:ram=\"iata: datamodel:3\"", "");
        //                         sbgenerateXFSUMessage.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
        //                         sbgenerateXFSUMessage.Replace(" xsi:schemaLocation=\"iata: waybill:1 Waybill_1.xsd\"", "");
        //                         sbgenerateXFSUMessage.Replace(" xmlns: ram = \"iata: datamodel:3\"", "");
        //                         sbgenerateXFSUMessage.Replace("xmlns: ram = \"iata: datamodel:3\"", "");
        //                         sbgenerateXFSUMessage.Replace(" xmlns:rsm=\"iata:waybill:1\"", "");
        //                         sbgenerateXFSUMessage.Replace(" xmlns:ram=\"iata:datamodel:3\"", "");

        //                         //Replace Nodes
        //                         sbgenerateXFSUMessage.Replace("ram:MasterConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
        //                         sbgenerateXFSUMessage.Replace("ram:AssociatedStatusConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
        //                         sbgenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
        //                         sbgenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_TransportContractDocument", "ram:TransportContractDocument");
        //                         sbgenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_TotalGrossWeightMeasure", "ram:TotalGrossWeightMeasure");

        //                         sbgenerateXFSUMessage.Insert(18, " xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ccts=\"urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2\" xmlns:udt=\"urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8\" xmlns:ram=\"iata:datamodel:3\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"iata:statusmessage:1 StatusMessage_1.xsd\"");
        //                         sbgenerateXFSUMessage.Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");

        //                     }
        //                 }

        //             }

        //         }
        //         catch (Exception ex)
        //         {
        //             //clsLog.WriteLogAzure(ex);
        //         }
        //         return Convert.ToString(sbgenerateXFSUMessage);


        //     }
        /*Not in use*/
        //public string GenerateXFSUARRMessageofTheAWB(string AWBPrefix, string AWBNumber, string orgDest, string messageType,
        //  string flightNo, string flightDate, string doNumber = "", string EventDate = "1900-01-01")
        //{
        //    StringBuilder sbgenerateXFSUMessage = new StringBuilder();

        //    try
        //    {
        //        DataSet dsxfsuMessage = new DataSet();




        //        dsxfsuMessage = GetAWBRecordforGenerateXFSUMessage(AWBPrefix, AWBNumber, orgDest, messageType, doNumber, flightNo, flightDate.ToString(), 0, 0.00);

        //        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 0 && dsxfsuMessage.Tables[0].Rows.Count > 0 && dsxfsuMessage.Tables[1].Rows.Count > 0)
        //        {
        //            var xfsuDataSet = new DataSet();
        //            DataRow[] drs;
        //            var xmlSchema = genericFunction.GetXMLMessageData("XFSU");
        //            if (xmlSchema != null && xmlSchema.Tables.Count > 0 && xmlSchema.Tables[0].Rows.Count > 0)
        //            {
        //                string messageXML = xmlSchema.Tables[0].Rows[0]["XMLMessageData"].ToString();
        //                messageXML = ReplacingNodeNames(messageXML);
        //                var txMessage = new StringReader(messageXML);
        //                xfsuDataSet.ReadXml(txMessage);

        //                if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 0 && dsxfsuMessage.Tables[0].Rows.Count > 0)
        //                {
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["NoofStatusUpdate"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageName"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageTypeCode"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["IssueDateTime"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageCreatedDate"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["PurposeCode"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["PurposeCode"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["VersionNumber"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["ConversationID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["ConversionID"]);

        //                    //SenderParty
        //                    if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                    {
        //                        drs = xfsuDataSet.Tables["PrimaryID"].Select("SenderParty_Id=0");
        //                        if (drs.Length > 0)
        //                        {
        //                            drs[0]["schemeID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["SenderParty_PrimaryID"]);
        //                            drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["SenderParty_PrimaryIDText"]);
        //                        }
        //                    }
        //                    //RecipientParty
        //                    if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                    {
        //                        drs = xfsuDataSet.Tables["PrimaryID"].Select("RecipientParty_Id=0");
        //                        if (drs.Length > 0)
        //                        {
        //                            drs[0]["schemeID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["RecipientParty_PrimaryID"]);
        //                            drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["RecipientParty_PrimaryIDText"]);
        //                        }
        //                    }

        //                    //BusinessHeaderDocument
        //                    xfsuDataSet.Tables["BusinessHeaderDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["NoofStatusUpdate"]);

        //                    if (xfsuDataSet.Tables["BusinessHeaderDocument"].Columns.Contains("Reference"))
        //                    {
        //                        xfsuDataSet.Tables["BusinessHeaderDocument"].Rows[0]["Reference"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["Reference"]);
        //                    }

        //                    //MasterConsignment
        //                    if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 1 && dsxfsuMessage.Tables[1].Rows.Count > 0)
        //                    {
        //                        if (xfsuDataSet.Tables.Contains("MasterConsignment_GrossWeightMeasure"))
        //                        {
        //                            drs = xfsuDataSet.Tables["MasterConsignment_GrossWeightMeasure"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedUOM"]);
        //                                drs[0]["MasterConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedWeight"]);
        //                            }
        //                        }

        //                        if (xfsuDataSet.Tables.Contains("TotalGrossWeightMeasure"))
        //                        {
        //                            drs = xfsuDataSet.Tables["TotalGrossWeightMeasure"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBUnitGrossWeight"]);
        //                                drs[0]["TotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBGrossWeight"]);
        //                            }
        //                        }

        //                        xfsuDataSet.Tables["MasterConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedPieces"]);
        //                        xfsuDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBPieces"]);
        //                        xfsuDataSet.Tables["MasterConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["PartConsignment"]);

        //                        // AWBNumber Section
        //                        if (xfsuDataSet.Tables.Contains("TransportContractDocument"))
        //                        {
        //                            drs = xfsuDataSet.Tables["TransportContractDocument"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBNumber"]);
        //                                drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AirwayBillDocumentName"]);
        //                                drs[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBDocumentTypeCode"]);
        //                            }
        //                        }

        //                        // AWBNumber Origin Section
        //                        if (xfsuDataSet.Tables.Contains("OriginLocation"))
        //                        {
        //                            drs = xfsuDataSet.Tables["OriginLocation"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBOrigin"]);
        //                                drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["OriginAirportName"]);
        //                            }
        //                        }

        //                        //AWBNumber Destination Section
        //                        if (xfsuDataSet.Tables.Contains("FinalDestinationLocation"))
        //                        {
        //                            drs = xfsuDataSet.Tables["FinalDestinationLocation"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWbDestination"]);
        //                                drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DestinationAirportName"]);
        //                            }
        //                        }

        //                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 3 && dsxfsuMessage.Tables[3].Rows.Count > 0)
        //                        {//LOOP
        //                            //AWBRouter Information
        //                            xfsuDataSet.Tables["RoutingLocation"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FltDestination"]);
        //                            xfsuDataSet.Tables["RoutingLocation"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["AirportName"]);

        //                        }


        //                        //AWBStatus Code
        //                        xfsuDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["MessageCode"]);


        //                        if (xfsuDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
        //                        {
        //                            if (Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["MessageCode"]) == "ARR")
        //                            {
        //                                drs = xfsuDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DeliveredUOM"]);
        //                                    drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["arrivedwt"]);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                drs = xfsuDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DeliveredUOM"]);
        //                                    drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DeliveredWeight"]);
        //                                }
        //                            }
        //                        }

        //                        if (xfsuDataSet.Tables.Contains("GrossVolumeMeasure"))
        //                        {
        //                            drs = xfsuDataSet.Tables["GrossVolumeMeasure"].Select("AssociatedStatusConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBVolumeMeasue"]);
        //                                drs[0]["GrossVolumeMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBVolume"]);
        //                            }
        //                        }

        //                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["DensityGroupCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DensityCode"]);

        //                        if (Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["MessageCode"]) == "ARR")
        //                        {
        //                            xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"] =
        //                                Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["arrivedpcs"]);


        //                        }
        //                        else
        //                        {
        //                            xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DeliveredPieces"]);
        //                        }
        //                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["PartConsignment"]);
        //                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["DiscrepancyDescriptionCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBDiscrepancyCode"]);
        //                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["StatusDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["StatusDescription"]);

        //                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 2 && dsxfsuMessage.Tables[2].Rows.Count > 0)
        //                        {
        //                            xfsuDataSet.Tables["AssociatedManifestDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ManifestNumber"]);

        //                            xfsuDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows[0]["ServiceTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ProductType"]);

        //                            xfsuDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightNumber"]);

        //                            xfsuDataSet.Tables["UsedLogisticsTransportMeans"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["CarrierName"]);

        //                            xfsuDataSet.Tables["UsedLogisticsTransportMeans"].Rows[0]["Type"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["AirCraftTypeMaster"]);


        //                            xfsuDataSet.Tables["ScheduledArrivalEvent"].Rows[0]["ScheduledOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightScheduleArrivlaTime"]);

        //                            xfsuDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightArriveTime"]);
        //                            xfsuDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalDateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ScheduleArrivalIndicator"]);

        //                            xfsuDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightScheduleDepartureTime"]);
        //                            xfsuDataSet.Tables["DepartureEvent"].Rows[0]["DepartureDateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["EstimatedDepTime"]);

        //                            if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                            {
        //                                drs = xfsuDataSet.Tables["PrimaryID"].Select("CarrierParty_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["CarrierCode"]);
        //                                    drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["CarrierName"]);
        //                                }
        //                            }

        //                            xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightDestlocation"]);
        //                            xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightLocationAirportName"]);
        //                            xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["LocationType"]);
        //                            xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["FlightStatusTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightStatusTypeCode"]);

        //                            //xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["SpecifiedEvent"]);
        //                            xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"] = Convert.ToString(EventDate);
        //                            xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["DateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["DateTimeTypeCode"]);

        //                        }

        //                        xfsuDataSet.Tables["NotifiedParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["NotifyName"]);
        //                        xfsuDataSet.Tables["DeliveryParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["ConsigneeName"]);



        //                        if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                        {
        //                            drs = xfsuDataSet.Tables["PrimaryID"].Select("AssociatedReceivedFromParty_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_schemeAgencyID"]);
        //                                drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_PrimaryID_Text"]);
        //                            }
        //                        }
        //                        xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssociatedReceivedAirlineName"]);
        //                        xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["RoleCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_RoleCode"]);
        //                        xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Role"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_Role"]);

        //                        if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                        {
        //                            drs = xfsuDataSet.Tables["PrimaryID"].Select("AssociatedTransferredFromParty_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_schemeAgencyID"]);
        //                                drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_PrimaryID_Text"]);
        //                            }
        //                        }
        //                        xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_Name"]);
        //                        xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["RoleCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_RoleCode"]);
        //                        xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["Role"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_Role"]);


        //                        xfsuDataSet.Tables["HandlingOSIInstructions"].Rows[0]["Description"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["HandlingInfo"]);
        //                        xfsuDataSet.Tables["HandlingOSIInstructions"].Rows[0]["DescriptionCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["SHCCodes"]);

        //                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 4 && dsxfsuMessage.Tables[4].Rows.Count > 0)
        //                        {
        //                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["ContentCode"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["ContentCode"]);
        //                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["Content"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["Content"]);
        //                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["SubjectCode"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["SubjectCode"]);
        //                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["CountryID"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["CountryID"]);
        //                            xfsuDataSet.Tables["AssociatedConsignmentCustomsProcedure"].Rows[0]["GoodsStatusCode"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["GoodsStatusCode"]);
        //                        }

        //                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 5 && dsxfsuMessage.Tables[5].Rows.Count > 0)
        //                        {
        //                            if (Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["MessageCode"]) == "ARR")
        //                            {
        //                                for (int i = 0; i < dsxfsuMessage.Tables[5].Rows.Count; i++)
        //                                {
        //                                    if (i == 0)
        //                                    {
        //                                        xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDNumber"]);
        //                                        xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["CharacteristicCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDType"]);
        //                                        xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["OperationalStatusCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDHeightIndicator"]);
        //                                        if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                                        {
        //                                            drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0");
        //                                            if (drs.Length > 0)
        //                                            {
        //                                                drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_schemeAgencyID"]);
        //                                                drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_PrimaryID_Text"]);
        //                                            }
        //                                        }
        //                                    }
        //                                    if (!(xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Select("UtilizedUnitLoadTransportEquipment_Id=" + i).Length > i))


        //                                    {
        //                                        DataRow drBolsegment = xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].NewRow();
        //                                        drBolsegment["ID"] = i;
        //                                        drBolsegment["CharacteristicCode"] = i;
        //                                        drBolsegment["OperationalStatusCode"] = i;

        //                                        drBolsegment["UtilizedUnitLoadTransportEquipment_Id"] = i;

        //                                        drBolsegment["AssociatedStatusConsignment_Id"] = 0;

        //                                        xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows.Add(drBolsegment);

        //                                        //xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[uldrow]["ID"] =
        //                                        //    Convert.ToString(dsxfsuMessage.Tables[5].Rows[uldrow]["ULDNumber"]);
        //                                        //xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[uldrow]["CharacteristicCode"] =
        //                                        //   Convert.ToString(dsxfsuMessage.Tables[5].Rows[uldrow]["ULDType"]);
        //                                        //xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[uldrow]["OperationalStatusCode"] =
        //                                        //   Convert.ToString(dsxfsuMessage.Tables[5].Rows[uldrow]["ULDHeightIndicator"]);
        //                                        drs = xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].
        //                                            Select("UtilizedUnitLoadTransportEquipment_Id=" + i);
        //                                        if (drs.Length > 0)
        //                                        {
        //                                            drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[i]["ULDNumber"]);
        //                                            drs[0]["CharacteristicCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[i]["ULDType"]);
        //                                            drs[0]["OperationalStatusCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[i]["ULDHeightIndicator"]);

        //                                        }


        //                                    }
        //                                    if (xfsuDataSet.Tables.Contains("OperatingParty"))
        //                                    {
        //                                        //if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                                        //{
        //                                        //    drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0");
        //                                        //    if (drs.Length > 0)
        //                                        //    {
        //                                        //        drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_schemeAgencyID"]);
        //                                        //        drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_PrimaryID_Text"]);
        //                                        //    }
        //                                        //}
        //                                        if (!(xfsuDataSet.Tables["OperatingParty"].Select("UtilizedUnitLoadTransportEquipment_Id="
        //                                            + i).Length > i))

        //                                        // if (!(xfsuDataSet.Tables["OperatingParty"].Rows.Count > uldrow))
        //                                        {
        //                                            DataRow drMaster = xfsuDataSet.Tables["OperatingParty"].NewRow();
        //                                            drMaster["OperatingParty_Id"] = i;
        //                                            drMaster["UtilizedUnitLoadTransportEquipment_Id"] = i;

        //                                            xfsuDataSet.Tables["OperatingParty"].Rows.Add(drMaster);

        //                                        }
        //                                        if (!(xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id="
        //                                           + i).Length > i))
        //                                        //if (!(xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0").Length > uldrow))
        //                                        {
        //                                            DataRow drMaster = xfsuDataSet.Tables["PrimaryID"].NewRow();
        //                                            drMaster["OperatingParty_Id"] = i;
        //                                            xfsuDataSet.Tables["PrimaryID"].Rows.Add(drMaster);

        //                                        }
        //                                        drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=" + i);
        //                                        if (drs.Length > 0)
        //                                        {
        //                                            drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[i]["OperatingParty_schemeAgencyID"]);
        //                                            drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[i]["OperatingParty_PrimaryID_Text"]);
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDNumber"]);
        //                                xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["CharacteristicCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDType"]);
        //                                xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["OperationalStatusCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDHeightIndicator"]);
        //                                if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                                {
        //                                    drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0");
        //                                    if (drs.Length > 0)
        //                                    {
        //                                        drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_schemeAgencyID"]);
        //                                        drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_PrimaryID_Text"]);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["ID"] = "";
        //                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["CharacteristicCode"] = "";
        //                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["OperationalStatusCode"] = "";
        //                            if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                            {
        //                                drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["schemeAgencyID"] = "";
        //                                    drs[0]["PrimaryID_Text"] = "";
        //                                }
        //                            }
        //                        }

        //                        ///House AWB s
        //                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 6 && dsxfsuMessage.Tables[6].Rows.Count > 0)
        //                        {
        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_GrossWeightMeasure"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_GrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["UnitofMeasurement"]);
        //                                    drs[0]["IncludedHouseConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBWt"]);
        //                                }
        //                            }
        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TotalGrossWeightMeasure"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_TotalGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["AcceptedUOM"]);
        //                                    drs[0]["IncludedHouseConsignment_TotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["GrossWeight"]);
        //                                }
        //                            }

        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBPcs"]);
        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TotalPieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["AWBPieces"]);
        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["PiecesIndicator"]);

        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TransportContractDocument"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_TransportContractDocument"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBNo"]);
        //                                    drs[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWbTypeCode"]);
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_GrossWeightMeasure"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_GrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["unitCode"] = "";
        //                                    drs[0]["IncludedHouseConsignment_GrossWeightMeasure_Text"] = "";
        //                                }
        //                            }
        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TotalGrossWeightMeasure"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_TotalGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["unitCode"] = "";
        //                                    drs[0]["IncludedHouseConsignment_TotalGrossWeightMeasure_Text"] = "";
        //                                }
        //                            }


        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["PieceQuantity"] = "";
        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TotalPieceQuantity"] = "";
        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TransportSplitDescription"] = "";

        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TransportContractDocument"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_TransportContractDocument"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["ID"] = "";
        //                                    drs[0]["TypeCode"] = "";
        //                                }
        //                            }
        //                        }
        //                    }
        //                    string generatMessage = xfsuDataSet.GetXml();
        //                    xfsuDataSet.Dispose();
        //                    sbgenerateXFSUMessage = new StringBuilder(generatMessage);
        //                    sbgenerateXFSUMessage.Replace(" xmlns: ram = \"iata: datamodel:3\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:rsm=\"iata: waybill:1\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:ram=\"iata: datamodel:3\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xsi:schemaLocation=\"iata: waybill:1 Waybill_1.xsd\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns: ram = \"iata: datamodel:3\"", "");
        //                    sbgenerateXFSUMessage.Replace("xmlns: ram = \"iata: datamodel:3\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:rsm=\"iata:waybill:1\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:ram=\"iata:datamodel:3\"", "");

        //                    //Replace Nodes
        //                    sbgenerateXFSUMessage.Replace("ram:MasterConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
        //                    sbgenerateXFSUMessage.Replace("ram:AssociatedStatusConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
        //                    sbgenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
        //                    sbgenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_TransportContractDocument", "ram:TransportContractDocument");
        //                    sbgenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_TotalGrossWeightMeasure", "ram:TotalGrossWeightMeasure");

        //                    sbgenerateXFSUMessage.Insert(18, " xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ccts=\"urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2\" xmlns:udt=\"urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8\" xmlns:ram=\"iata:datamodel:3\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"iata:statusmessage:1 StatusMessage_1.xsd\"");
        //                    sbgenerateXFSUMessage.Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");

        //                }
        //            }
        //            else
        //            {
        //                sbgenerateXFSUMessage.Append("No Message format available in the system.");
        //            }
        //        }
        //        else
        //        {
        //            sbgenerateXFSUMessage.Append("No Data available in the system to generate message.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //    }


        //    return Convert.ToString(sbgenerateXFSUMessage);


        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="AWBPrefix"></param>
        /// <param name="AWBNumber"></param>
        /// <param name="EventStatus"></param>
        /// <returns></returns>
        /// 

        /*Not in user*/
        //public string GenerateXFSUDLVMessageofTheAWB(string AWBPrefix, string AWBNumber, string orgDest, string messageType,
        //string doNumber, string flightNo, string flightDate, int DLVpcs, double DLVWt, string EventDate = "1900-01-01")
        //{
        //    StringBuilder sbgenerateXFSUMessage = new StringBuilder();


        //    try
        //    {
        //        DataSet dsxfsuMessage = new DataSet();
        //        dsxfsuMessage = GetAWBRecordforGenerateXFSUMDLVessage(AWBPrefix, AWBNumber, orgDest, messageType,
        //            doNumber, flightNo, flightDate, DLVpcs, DLVWt);

        //        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 0 && dsxfsuMessage.Tables[0].Rows.Count > 0 && dsxfsuMessage.Tables[1].Rows.Count > 0)
        //        {
        //            var xfsuDataSet = new DataSet();
        //            DataRow[] drs;
        //            var xmlSchema = genericFunction.GetXMLMessageData("XFSU");
        //            if (xmlSchema != null && xmlSchema.Tables.Count > 0 && xmlSchema.Tables[0].Rows.Count > 0)
        //            {
        //                string messageXML = xmlSchema.Tables[0].Rows[0]["XMLMessageData"].ToString();
        //                messageXML = ReplacingNodeNames(messageXML);
        //                var txMessage = new StringReader(messageXML);
        //                xfsuDataSet.ReadXml(txMessage);

        //                if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 0 && dsxfsuMessage.Tables[0].Rows.Count > 0)
        //                {
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["ReferenceNumber"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageName"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageTypeCode"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["IssueDateTime"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["MessageCreatedDate"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["PurposeCode"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["PurposeCode"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["VersionNumber"]);
        //                    xfsuDataSet.Tables["MessageHeaderDocument"].Rows[0]["ConversationID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["ConversionID"]);

        //                    //SenderParty
        //                    if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                    {
        //                        drs = xfsuDataSet.Tables["PrimaryID"].Select("SenderParty_Id=0");
        //                        if (drs.Length > 0)
        //                        {
        //                            drs[0]["schemeID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["SenderParty_PrimaryID"]);
        //                            drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["SenderParty_PrimaryIDText"]);
        //                        }
        //                    }
        //                    //RecipientParty
        //                    if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                    {
        //                        drs = xfsuDataSet.Tables["PrimaryID"].Select("RecipientParty_Id=0");
        //                        if (drs.Length > 0)
        //                        {
        //                            drs[0]["schemeID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["RecipientParty_PrimaryID"]);
        //                            drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["RecipientParty_PrimaryIDText"]);
        //                        }
        //                    }

        //                    //BusinessHeaderDocument
        //                    xfsuDataSet.Tables["BusinessHeaderDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["NoofStatusUpdate"]);

        //                    if (xfsuDataSet.Tables["BusinessHeaderDocument"].Columns.Contains("Reference"))
        //                    {
        //                        xfsuDataSet.Tables["BusinessHeaderDocument"].Rows[0]["Reference"] = Convert.ToString(dsxfsuMessage.Tables[0].Rows[0]["Reference"]);
        //                    }

        //                    //MasterConsignment
        //                    if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 1 && dsxfsuMessage.Tables[1].Rows.Count > 0)
        //                    {
        //                        if (xfsuDataSet.Tables.Contains("MasterConsignment_GrossWeightMeasure"))
        //                        {
        //                            drs = xfsuDataSet.Tables["MasterConsignment_GrossWeightMeasure"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedUOM"]);
        //                                drs[0]["MasterConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedWeight"]);
        //                            }
        //                        }

        //                        if (xfsuDataSet.Tables.Contains("TotalGrossWeightMeasure"))
        //                        {
        //                            drs = xfsuDataSet.Tables["TotalGrossWeightMeasure"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBUnitGrossWeight"]);
        //                                drs[0]["TotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBGrossWeight"]);
        //                            }
        //                        }

        //                        xfsuDataSet.Tables["MasterConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AcceptedPieces"]);
        //                        xfsuDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBPieces"]);
        //                        xfsuDataSet.Tables["MasterConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["PartConsignment"]);

        //                        // AWBNumber Section
        //                        if (xfsuDataSet.Tables.Contains("TransportContractDocument"))
        //                        {
        //                            drs = xfsuDataSet.Tables["TransportContractDocument"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBNumber"]);
        //                                drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AirwayBillDocumentName"]);
        //                                drs[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBDocumentTypeCode"]);
        //                            }
        //                        }

        //                        // AWBNumber Origin Section
        //                        if (xfsuDataSet.Tables.Contains("OriginLocation"))
        //                        {
        //                            drs = xfsuDataSet.Tables["OriginLocation"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBOrigin"]);
        //                                drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["OriginAirportName"]);
        //                            }
        //                        }

        //                        //AWBNumber Destination Section
        //                        if (xfsuDataSet.Tables.Contains("FinalDestinationLocation"))
        //                        {
        //                            drs = xfsuDataSet.Tables["FinalDestinationLocation"].Select("MasterConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWbDestination"]);
        //                                drs[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DestinationAirportName"]);
        //                            }
        //                        }

        //                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 3 && dsxfsuMessage.Tables[3].Rows.Count > 0)
        //                        {//LOOP
        //                            //AWBRouter Information
        //                            xfsuDataSet.Tables["RoutingLocation"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["FltDestination"]);
        //                            xfsuDataSet.Tables["RoutingLocation"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[3].Rows[0]["AirportName"]);

        //                        }


        //                        //AWBStatus Code
        //                        xfsuDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["MessageCode"]);


        //                        if (xfsuDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
        //                        {
        //                            drs = xfsuDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DeliveredUOM"]);
        //                                drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DeliveredWeight"]);
        //                            }
        //                        }

        //                        if (xfsuDataSet.Tables.Contains("GrossVolumeMeasure"))
        //                        {
        //                            drs = xfsuDataSet.Tables["GrossVolumeMeasure"].Select("AssociatedStatusConsignment_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["UnitCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBVolumeMeasue"]);
        //                                drs[0]["GrossVolumeMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBVolume"]);
        //                            }
        //                        }

        //                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["DensityGroupCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DensityCode"]);
        //                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["DeliveredPieces"]);
        //                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["PartConsignment"]);
        //                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["DiscrepancyDescriptionCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AWBDiscrepancyCode"]);
        //                        xfsuDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["StatusDescription"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["StatusDescription"]);

        //                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 2 && dsxfsuMessage.Tables[2].Rows.Count > 0)
        //                        {
        //                            xfsuDataSet.Tables["AssociatedManifestDocument"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ManifestNumber"]);

        //                            xfsuDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows[0]["ServiceTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ProductType"]);

        //                            xfsuDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightNumber"]);

        //                            xfsuDataSet.Tables["UsedLogisticsTransportMeans"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["CarrierName"]);

        //                            xfsuDataSet.Tables["UsedLogisticsTransportMeans"].Rows[0]["Type"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["AirCraftTypeMaster"]);


        //                            xfsuDataSet.Tables["ScheduledArrivalEvent"].Rows[0]["ScheduledOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightScheduleArrivlaTime"]);

        //                            xfsuDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightArriveTime"]);
        //                            xfsuDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalDateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["ScheduleArrivalIndicator"]);

        //                            xfsuDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightScheduleDepartureTime"]);
        //                            xfsuDataSet.Tables["DepartureEvent"].Rows[0]["DepartureDateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["EstimatedDepTime"]);

        //                            if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                            {
        //                                drs = xfsuDataSet.Tables["PrimaryID"].Select("CarrierParty_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["CarrierCode"]);
        //                                    drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["CarrierName"]);
        //                                }
        //                            }

        //                            xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightDestlocation"]);
        //                            xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightLocationAirportName"]);
        //                            xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["LocationType"]);
        //                            xfsuDataSet.Tables["SpecifiedLocation"].Rows[0]["FlightStatusTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["FlightStatusTypeCode"]);

        //                            //xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["SpecifiedEvent"]);
        //                            xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"] = Convert.ToString(EventDate);
        //                            xfsuDataSet.Tables["SpecifiedEvent"].Rows[0]["DateTimeTypeCode"] = Convert.ToString(dsxfsuMessage.Tables[2].Rows[0]["DateTimeTypeCode"]);

        //                        }

        //                        xfsuDataSet.Tables["NotifiedParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["NotifyName"]);
        //                        xfsuDataSet.Tables["DeliveryParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["ConsigneeName"]);



        //                        if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                        {
        //                            drs = xfsuDataSet.Tables["PrimaryID"].Select("AssociatedReceivedFromParty_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_schemeAgencyID"]);
        //                                drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_PrimaryID_Text"]);
        //                            }
        //                        }
        //                        xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssociatedReceivedAirlineName"]);
        //                        xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["RoleCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_RoleCode"]);
        //                        xfsuDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Role"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssRecFromParty_Role"]);

        //                        if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                        {
        //                            drs = xfsuDataSet.Tables["PrimaryID"].Select("AssociatedTransferredFromParty_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_schemeAgencyID"]);
        //                                drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_PrimaryID_Text"]);
        //                            }
        //                        }
        //                        xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["Name"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_Name"]);
        //                        xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["RoleCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_RoleCode"]);
        //                        xfsuDataSet.Tables["AssociatedTransferredFromParty"].Rows[0]["Role"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["AssTransferFromParty_Role"]);


        //                        xfsuDataSet.Tables["HandlingOSIInstructions"].Rows[0]["Description"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["HandlingInfo"]);
        //                        xfsuDataSet.Tables["HandlingOSIInstructions"].Rows[0]["DescriptionCode"] = Convert.ToString(dsxfsuMessage.Tables[1].Rows[0]["SHCCodes"]);

        //                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 4 && dsxfsuMessage.Tables[4].Rows.Count > 0)
        //                        {
        //                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["ContentCode"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["ContentCode"]);
        //                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["Content"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["Content"]);
        //                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["SubjectCode"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["SubjectCode"]);
        //                            xfsuDataSet.Tables["IncludedCustomsNote"].Rows[0]["CountryID"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["CountryID"]);
        //                            xfsuDataSet.Tables["AssociatedConsignmentCustomsProcedure"].Rows[0]["GoodsStatusCode"] = Convert.ToString(dsxfsuMessage.Tables[4].Rows[0]["GoodsStatusCode"]);
        //                        }

        //                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 5 && dsxfsuMessage.Tables[5].Rows.Count > 0)
        //                        {
        //                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDNumber"]);
        //                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["CharacteristicCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDType"]);
        //                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["OperationalStatusCode"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["ULDHeightIndicator"]);
        //                            if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                            {
        //                                drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["schemeAgencyID"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_schemeAgencyID"]);
        //                                    drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfsuMessage.Tables[5].Rows[0]["OperatingParty_PrimaryID_Text"]);
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["ID"] = "";
        //                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["CharacteristicCode"] = "";
        //                            xfsuDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["OperationalStatusCode"] = "";
        //                            if (xfsuDataSet.Tables.Contains("PrimaryID"))
        //                            {
        //                                drs = xfsuDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["schemeAgencyID"] = "";
        //                                    drs[0]["PrimaryID_Text"] = "";
        //                                }
        //                            }
        //                        }

        //                        ///House AWB s
        //                        if (dsxfsuMessage != null && dsxfsuMessage.Tables != null && dsxfsuMessage.Tables.Count > 6 && dsxfsuMessage.Tables[6].Rows.Count > 0)
        //                        {
        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_GrossWeightMeasure"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_GrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["UnitofMeasurement"]);
        //                                    drs[0]["IncludedHouseConsignment_GrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBWt"]);
        //                                }
        //                            }
        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TotalGrossWeightMeasure"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_TotalGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["unitCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["AcceptedUOM"]);
        //                                    drs[0]["IncludedHouseConsignment_TotalGrossWeightMeasure_Text"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["GrossWeight"]);
        //                                }
        //                            }

        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["PieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBPcs"]);
        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TotalPieceQuantity"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["AWBPieces"]);
        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TransportSplitDescription"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["PiecesIndicator"]);

        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TransportContractDocument"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_TransportContractDocument"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["ID"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWBNo"]);
        //                                    drs[0]["TypeCode"] = Convert.ToString(dsxfsuMessage.Tables[6].Rows[0]["HAWbTypeCode"]);
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_GrossWeightMeasure"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_GrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["unitCode"] = "";
        //                                    drs[0]["IncludedHouseConsignment_GrossWeightMeasure_Text"] = "";
        //                                }
        //                            }
        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TotalGrossWeightMeasure"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_TotalGrossWeightMeasure"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["unitCode"] = "";
        //                                    drs[0]["IncludedHouseConsignment_TotalGrossWeightMeasure_Text"] = "";
        //                                }
        //                            }


        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["PieceQuantity"] = "";
        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TotalPieceQuantity"] = "";
        //                            xfsuDataSet.Tables["IncludedHouseConsignment"].Rows[0]["TransportSplitDescription"] = "";

        //                            if (xfsuDataSet.Tables.Contains("IncludedHouseConsignment_TransportContractDocument"))
        //                            {
        //                                drs = xfsuDataSet.Tables["IncludedHouseConsignment_TransportContractDocument"].Select("IncludedHouseConsignment_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["ID"] = "";
        //                                    drs[0]["TypeCode"] = "";
        //                                }
        //                            }
        //                        }
        //                    }
        //                    string generatMessage = xfsuDataSet.GetXml();
        //                    xfsuDataSet.Dispose();
        //                    sbgenerateXFSUMessage = new StringBuilder(generatMessage);
        //                    sbgenerateXFSUMessage.Replace(" xmlns: ram = \"iata: datamodel:3\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:rsm=\"iata: waybill:1\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:ram=\"iata: datamodel:3\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xsi:schemaLocation=\"iata: waybill:1 Waybill_1.xsd\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns: ram = \"iata: datamodel:3\"", "");
        //                    sbgenerateXFSUMessage.Replace("xmlns: ram = \"iata: datamodel:3\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:rsm=\"iata:waybill:1\"", "");
        //                    sbgenerateXFSUMessage.Replace(" xmlns:ram=\"iata:datamodel:3\"", "");

        //                    //Replace Nodes
        //                    sbgenerateXFSUMessage.Replace("ram:MasterConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
        //                    sbgenerateXFSUMessage.Replace("ram:AssociatedStatusConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
        //                    sbgenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");
        //                    sbgenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_TransportContractDocument", "ram:TransportContractDocument");
        //                    sbgenerateXFSUMessage.Replace("ram:IncludedHouseConsignment_TotalGrossWeightMeasure", "ram:TotalGrossWeightMeasure");
        //                    sbgenerateXFSUMessage.Replace("ram:AssociatedStatusConsignment_GrossWeightMeasure", "ram:GrossWeightMeasure");

        //                    sbgenerateXFSUMessage.Insert(18, " xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:ccts=\"urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2\" xmlns:udt=\"urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8\" xmlns:ram=\"iata:datamodel:3\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"iata:statusmessage:1 StatusMessage_1.xsd\"");
        //                    sbgenerateXFSUMessage.Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");

        //                }
        //            }
        //            //else
        //            //{
        //            //    sbgenerateXFSUMessage.Append("No Message format available in the system.");
        //            //}
        //        }


        //        //else
        //        //{
        //        //    sbgenerateXFSUMessage.Append("No Data available in the system to generate message.");
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //    }

        //    return sbgenerateXFSUMessage.ToString();
        //}

        public async Task<DataSet> GetAWBRecordforGenerateXFSUMessage(string AWBPrefix, string AWBNumber, string orgDest, string messageType,
    string doNumber, string flightNo, string flightDate, int DLVpcs, double DLVWt)
        {
            DataSet? dsmessage = new DataSet();
            try
            {
                //SQLServer da = new SQLServer();
                //string[] paramname = new string[] { "AWBPrefix", "AWBNumber", "OrgDest", "MessageType", "DoNumber", "FlightNo", "FlightDate", "PC", "WT" };
                //object[] paramvalue = new object[] { AWBPrefix, AWBNumber, orgDest, messageType, doNumber, flightNo, flightDate, DLVpcs, DLVWt };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                //    SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime , SqlDbType.Int, SqlDbType.Decimal, SqlDbType.Int, SqlDbType.Decimal};
                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWBNumber },
                    new SqlParameter("@OrgDest", SqlDbType.VarChar) { Value = orgDest },
                    new SqlParameter("@MessageType", SqlDbType.VarChar) { Value = messageType },
                    new SqlParameter("@DoNumber", SqlDbType.VarChar) { Value = doNumber },
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = flightNo },
                    new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = flightDate },
                    new SqlParameter("@PC", SqlDbType.Int) { Value = DLVpcs },
                    new SqlParameter("@WT", SqlDbType.Decimal) { Value = DLVWt }
                };
                //dsmessage = da.SelectRecords("uspGetAWBRecordToGenerateXFSUMessage", paramname, paramvalue, paramtype);
                dsmessage = await _readWriteDao.SelectRecords("uspGetAWBRecordToGenerateXFSUMessage", sqlParameters);
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                dsmessage = null;
            }
            return dsmessage;
        }

        /*Not in use*/
        //public DataSet GetAWBRecordforGenerateXFSUMDLVessage(string AWBPrefix, string AWBNumber, string orgDest, string messageType,
        //    string doNumber, string flightNo, string flightDate, int DLVpcs, double DLVWt)
        //{
        //    DataSet dsmessage = new DataSet();
        //    try
        //    {
        //        SQLServer da = new SQLServer();
        //        string[] paramname = new string[] { "AWBPrefix", "AWBNumber", "OrgDest", "MessageType", "DoNumber", "FlightNo", "FlightDate", "PC", "WT" };
        //        object[] paramvalue = new object[] { AWBPrefix, AWBNumber, orgDest, messageType, doNumber, flightNo, flightDate, DLVpcs, DLVWt };
        //        SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
        //            SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime , SqlDbType.Int, SqlDbType.Decimal};
        //        dsmessage = da.SelectRecords("uspGetAWBRecordToGenerateAutoXFSUDLVMessage", paramname, paramvalue, paramtype);
        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //        dsmessage = null;
        //    }
        //    return dsmessage;
        //}
        public async Task<DataSet> GetAWBDetailstoGenerateXFSUMessage(string AWBPrefix, string AWBNumber, string EventStatus)
        {
            DataSet? dsmessage = new DataSet();
            try
            {
                //string strConnectionString = Global.GetConnectionString();
                //SQLServer da = new SQLServer();
                //string[] paramname = new string[] { "AWBPrefix", "AWBNumber", "EventStatus" };
                //object[] paramvalue = new object[] { AWBPrefix, AWBNumber, EventStatus };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };

                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWBNumber },
                    new SqlParameter("@EventStatus", SqlDbType.VarChar) { Value = EventStatus }
                };
                //dsmessage = da.SelectRecords("uspGetAWBRecordToGenerateAutoSendXFSUMessage", paramname, paramvalue, paramtype);
                dsmessage = await _readWriteDao.SelectRecords("uspGetAWBRecordToGenerateAutoSendXFSUMessage", sqlParameters);
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
                if (root.Name.Equals("rsm:StatusMessage"))
                {
                    xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:GrossWeightMeasure')]");
                    foreach (XmlNode xmlNode in xmlNodelst)
                    {
                        if (xmlNode.ParentNode.Name.Equals("rsm:MasterConsignment"))
                        {
                            nodeToFind = RenameNode(xmlNode, "ram:MasterConsignment_GrossWeightMeasure");
                        }
                        else if (xmlNode.ParentNode.Name.Equals("ram:AssociatedStatusConsignment"))
                        {
                            nodeToFind = RenameNode(xmlNode, "ram:AssociatedStatusConsignment_GrossWeightMeasure");
                        }
                        else if (xmlNode.ParentNode.Name.Equals("ram:IncludedHouseConsignment"))
                        {
                            nodeToFind = RenameNode(xmlNode, "ram:IncludedHouseConsignment_GrossWeightMeasure");
                        }
                    }

                    xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:TransportContractDocument')]");
                    foreach (XmlNode xmlNode in xmlNodelst)
                    {
                        if (xmlNode.ParentNode.Name.Equals("ram:IncludedHouseConsignment"))
                        {
                            nodeToFind = RenameNode(xmlNode, "ram:IncludedHouseConsignment_TransportContractDocument");
                        }
                    }

                    xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:TotalGrossWeightMeasure')]");
                    foreach (XmlNode xmlNode in xmlNodelst)
                    {
                        if (xmlNode.ParentNode.Name.Equals("ram:IncludedHouseConsignment"))
                        {
                            nodeToFind = RenameNode(xmlNode, "ram:IncludedHouseConsignment_TotalGrossWeightMeasure");
                        }
                    }

                    xmlMsg = doc.OuterXml;
                    xmlMsg = xmlMsg.Replace("MasterConsignment_GrossWeightMeasure", "ram:MasterConsignment_GrossWeightMeasure");
                    xmlMsg = xmlMsg.Replace("AssociatedStatusConsignment_GrossWeightMeasure", "ram:AssociatedStatusConsignment_GrossWeightMeasure");
                    xmlMsg = xmlMsg.Replace("IncludedHouseConsignment_GrossWeightMeasure", "ram:IncludedHouseConsignment_GrossWeightMeasure");
                    xmlMsg = xmlMsg.Replace("IncludedHouseConsignment_TransportContractDocument", "ram:IncludedHouseConsignment_TransportContractDocument");
                    xmlMsg = xmlMsg.Replace("IncludedHouseConsignment_TotalGrossWeightMeasure", "ram:IncludedHouseConsignment_TotalGrossWeightMeasure");
                }

                return xmlMsg;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return xmlMsg;
            }
        }

        #region Decoding Functionality
        /// <summary>
        /// Method to decode XFSU message string and assing decoded data to the array of structure
        /// </summary>
        /// <param name="refNO">SrNo from tblInbox</param>
        /// <param name="fsamsg">Message string</param>
        /// <param name="fsadata">Contains consignment information</param>
        /// <param name="fsanodes">Contains flight information</param>
        /// <param name="custominfo">Other Customs, Security and Regulatory Information</param>
        /// <param name="uld">ULD information</param>
        /// <param name="othinfoarray">Other service information</param>
        /// <returns>Return true when message decoded successfuly</returns>
        public async Task<(
            bool success,
            MessageData.FSAInfo fsadata,
            MessageData.CommonStruct[] fsanodes,
            MessageData.customsextrainfo[] custominfo,
            MessageData.ULDinfo[] uld,
            MessageData.otherserviceinfo[] othinfoarray,
            string ErrorMsg)>
            DecodeReceivedXFSUMessage(int refNO, string fsamsg, MessageData.FSAInfo fsadata,
             MessageData.CommonStruct[] fsanodes, MessageData.customsextrainfo[] custominfo, MessageData.ULDinfo[] uld,
             MessageData.otherserviceinfo[] othinfoarray, string ErrorMsg)
        {
            string awbref = string.Empty;
            bool flag = false;
            string lastrec = string.Empty;
            ErrorMsg = "";
            DataRow[] drs;
            var fsaXmlDataSet = new DataSet();
            var tx = new StringReader(fsamsg);
            fsaXmlDataSet.ReadXml(tx);

            try
            {
                if (fsaXmlDataSet.Tables.Contains("MessageHeaderDocument"))
                {
                    if (fsaXmlDataSet.Tables["MessageHeaderDocument"].Columns.Contains("VersionID"))
                    {
                        fsadata.fsaversion = Convert.ToString(fsaXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"]);
                    }
                }

                //if (fsaXmlDataSet.Tables.Contains("TransportContractDocument"))
                //{
                //    drs = fsaXmlDataSet.Tables["TransportContractDocument"].Select("MasterConsignment_Id=0");
                //    if (drs.Length > 0)
                //    {
                //        string[] awbNumber = Convert.ToString(drs[0]["ID"]).Split('-');
                //        fsadata.airlineprefix = awbNumber[0];
                //        fsadata.awbnum = awbNumber[1];
                //        //Convert.ToString(drs[0]["Name"]);
                //        //Convert.ToString(drs[0]["TypeCode"]);
                //    }
                //}


                string awbNumber = "";

                if (fsaXmlDataSet.Tables.Contains("TransportContractDocument"))
                {
                    if (fsaXmlDataSet.Tables["TransportContractDocument"].Columns.Contains("ID"))
                    {
                        awbNumber = Convert.ToString(fsaXmlDataSet.Tables["TransportContractDocument"].Rows[0]["ID"]) != string.Empty ?
                            Convert.ToString(fsaXmlDataSet.Tables["TransportContractDocument"].Rows[0]["ID"]) : "";
                        string[] decmes = awbNumber.Split('-');
                        fsadata.airlineprefix = decmes[0];
                        fsadata.awbnum = decmes[1];
                    }

                }




                if (fsaXmlDataSet.Tables.Contains("BusinessHeaderDocument"))
                {
                    if (fsaXmlDataSet.Tables["BusinessHeaderDocument"].Columns.Contains("ID"))
                    {
                        awbNumber = Convert.ToString(fsaXmlDataSet.Tables["BusinessHeaderDocument"].Rows[0]["ID"]) != string.Empty ?
                            Convert.ToString(fsaXmlDataSet.Tables["BusinessHeaderDocument"].Rows[0]["ID"]) : "";
                        string[] decmes = awbNumber.Split('-');
                        fsadata.airlineprefix = decmes[0];
                        fsadata.awbnum = decmes[1];
                    }

                }


                string airlinePrefix = string.Empty;
                string AWBNo = string.Empty;
                string origin = string.Empty;
                string destination = string.Empty;


                if (fsaXmlDataSet.Tables.Contains("TotalGrossWeightMeasure"))
                {
                    drs = fsaXmlDataSet.Tables["TotalGrossWeightMeasure"].Select("MasterConsignment_Id=0");
                    if (drs.Length > 0)
                    {
                        if (Convert.ToString(drs[0]["unitCode"]) == "KGM")
                        {
                            fsadata.weightcode = Convert.ToString("K");
                        }
                        else
                        {
                            fsadata.weightcode = Convert.ToString("L");
                        }
                        fsadata.weight = Convert.ToString(drs[0]["TotalGrossWeightMeasure_Text"]);
                    }
                }
                if (fsaXmlDataSet.Tables.Contains("MasterConsignment"))
                {
                    if (fsaXmlDataSet.Tables["MasterConsignment"].Columns.Contains("PieceQuantity"))
                    {
                        fsadata.pcscnt = Convert.ToString(fsaXmlDataSet.Tables["MasterConsignment"].Rows[0]["PieceQuantity"]);
                    }
                    if (fsaXmlDataSet.Tables["MasterConsignment"].Columns.Contains("TransportSplitDescription"))
                    {
                        fsadata.consigntype = Convert.ToString(fsaXmlDataSet.Tables["MasterConsignment"].Rows[0]["TransportSplitDescription"]);
                    }
                    if (fsaXmlDataSet.Tables["MasterConsignment"].Columns.Contains("TotalPieceQuantity"))
                    {
                        fsadata.totalpcscnt = Convert.ToString(fsaXmlDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"]);
                    }


                }

                // AWBNumber Origin Section


                if (fsaXmlDataSet.Tables.Contains("OriginLocation"))
                {
                    if (fsaXmlDataSet.Tables["OriginLocation"].Columns.Contains("ID"))
                    {
                        fsadata.origin = Convert.ToString(fsaXmlDataSet.Tables["OriginLocation"].Rows[0]["ID"]) != String.Empty ?
                            Convert.ToString(fsaXmlDataSet.Tables["OriginLocation"].Rows[0]["ID"]) : "";

                    }
                }



                //AWBNumber Destination Section


                if (fsaXmlDataSet.Tables.Contains("FinalDestinationLocation"))
                {
                    if (fsaXmlDataSet.Tables["FinalDestinationLocation"].Columns.Contains("ID"))
                    {
                        fsadata.dest = Convert.ToString(fsaXmlDataSet.Tables["FinalDestinationLocation"].Rows[0]["ID"]) != string.Empty ?
                            Convert.ToString(fsaXmlDataSet.Tables["FinalDestinationLocation"].Rows[0]["ID"]) : "";

                    }
                }


                MessageData.CommonStruct recdata = new MessageData.CommonStruct("");
                switch (Convert.ToString(fsaXmlDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"]))
                {
                    #region FOH
                    case "FOH":
                        {
                            #region FOH
                            recdata.messageprefix = Convert.ToString(fsaXmlDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"]);
                            if (fsaXmlDataSet.Tables.Contains("DepartureEvent"))
                            {
                                if (fsaXmlDataSet.Tables["DepartureEvent"].Columns.Contains("DepartureOccurrenceDateTime"))
                                {
                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"]).Split('T');
                                    recdata.fltday = fltdate[0];
                                    //recdata.fltmonth = msg[1].Substring(2, 3);
                                    recdata.flttime = fltdate[1];
                                }
                            }
                            else if (fsaXmlDataSet.Tables.Contains("SpecifiedEvent"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedEvent"].Columns.Contains("OccurrenceDateTime"))
                                {

                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"]).Split('T');
                                    if (fltdate.Length > 1)
                                    {
                                        recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                                        recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1].Substring(0, 8);
                                    }
                                    else
                                    {
                                        ErrorMsg = "Seprate date and time with T from IssueDateTime tag";
                                        flag = false;
                                        //return false;
                                        return (flag, fsadata, fsanodes, custominfo, uld, othinfoarray, ErrorMsg);

                                    }
                                }
                            }
                            else
                            {
                                recdata.fltday = DateTime.UtcNow.ToString("MM/dd/yyyy");
                                recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8);
                            }

                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLocation"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedLocation"].Columns.Contains("ID"))
                                    recdata.airportcode = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"]);
                            }
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment"))
                            {
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("TransportSplitDescription"))
                                    recdata.pcsindicator = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"]);
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("PieceQuantity"))
                                    recdata.numofpcs = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"]);
                                recdata.densityindicator = "DG";
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("DensityGroupCode"))
                                {
                                    recdata.densitygroup = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["DensityGroupCode"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {
                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("UnitCode"))
                                    {
                                        recdata.weightcode = Convert.ToString(drs[0]["UnitCode"]);
                                    }
                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("AssociatedStatusConsignment_GrossWeightMeasure_Text"))

                                    {
                                        recdata.weight = Convert.ToString(drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"]);
                                    }
                                }
                            }

                            if (fsaXmlDataSet.Tables.Contains("AssociatedReceivedFromParty"))
                            {
                                if (fsaXmlDataSet.Tables["AssociatedReceivedFromParty"].Columns.Contains("Name"))
                                    recdata.name = Convert.ToString(fsaXmlDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Name"]);
                            }
                            if (fsaXmlDataSet.Tables.Contains("GrossVolumeMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["GrossVolumeMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {
                                    if (fsaXmlDataSet.Tables["GrossVolumeMeasure"].Columns.Contains("UnitCode"))
                                        recdata.volumecode = Convert.ToString(drs[0]["UnitCode"]);
                                    if (fsaXmlDataSet.Tables["GrossVolumeMeasure"].Columns.Contains("GrossVolumeMeasure_Text"))
                                        recdata.volumeamt = Convert.ToString(drs[0]["GrossVolumeMeasure_Text"]);
                                }
                            }

                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                            fsanodes[fsanodes.Length - 1] = recdata;
                            #endregion
                        }
                        break;
                    #endregion

                    #region[RCS]
                    case "RCS":
                        {
                            #region RCS
                            if (fsaXmlDataSet.Tables.Contains("ReportedStatus"))
                            {
                                if (fsaXmlDataSet.Tables["ReportedStatus"].Columns.Contains("ReasonCode"))
                                {
                                    recdata.messageprefix = Convert.ToString(fsaXmlDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("SpecifiedEvent"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedEvent"].Columns.Contains("OccurrenceDateTime"))
                                {
                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"]).Split('T');
                                    recdata.updatedonday = fltdate[0];
                                    recdata.updatedontime = fltdate[1];

                                    recdata.fltday = fltdate[0];
                                    //recdata.fltmonth = currentLineRCSText[1] != "" ? currentLineRCSText[1].Substring(2, 3) : "";
                                    recdata.flttime = fltdate[1];
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLocation"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedLocation"].Columns.Contains("ID"))
                                {
                                    recdata.airportcode = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"]);
                                }
                            }

                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment"))
                            {
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("TransportSplitDescription"))
                                {
                                    recdata.pcsindicator = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"]);
                                    ///Below condition added by prashant on 15-Mar-2017. To resolve JIRA# AS-443.
                                    if (recdata.pcsindicator.Trim().ToUpper() == "P")
                                    {
                                        flag = false;
                                        //GenericFunction genericFunction = new GenericFunction();

                                        await _genericFunction.UpdateErrorMessageToInbox(refNO, "Partial acceptance not allowed");
                                    }
                                }
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("PieceQuantity"))
                                {
                                    recdata.numofpcs = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {
                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("UnitCode"))
                                    {
                                        recdata.weightcode = Convert.ToString(drs[0]["UnitCode"]);
                                    }
                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("AssociatedStatusConsignment_GrossWeightMeasure_Text"))
                                    {
                                        recdata.weight = Convert.ToString(drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"]);
                                    }
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("AssociatedReceivedFromParty"))
                            {
                                if (fsaXmlDataSet.Tables["AssociatedReceivedFromParty"].Columns.Contains("Name"))
                                {
                                    recdata.name = Convert.ToString(fsaXmlDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Name"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("GrossVolumeMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["GrossVolumeMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {
                                    if (fsaXmlDataSet.Tables["GrossVolumeMeasure"].Columns.Contains("UnitCode"))
                                    {
                                        recdata.volumecode = Convert.ToString(drs[0]["UnitCode"]);
                                    }
                                    if (fsaXmlDataSet.Tables["GrossVolumeMeasure"].Columns.Contains("GrossVolumeMeasure_Text"))
                                    {
                                        recdata.volumeamt = Convert.ToString(drs[0]["GrossVolumeMeasure_Text"]);
                                    }
                                }
                            }

                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                            fsanodes[fsanodes.Length - 1] = recdata;

                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLogisticsTransportMovement"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Columns.Contains("ID"))
                                {
                                    recdata.flightnum = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"]);
                                }
                            }
                            #endregion
                        }
                        break;
                    #endregion

                    #region[RCT]
                    case "RCT":
                        {
                            #region RCT
                            if (fsaXmlDataSet.Tables.Contains("ReportedStatus"))
                            {
                                if (fsaXmlDataSet.Tables["ReportedStatus"].Columns.Contains("ReasonCode"))
                                {
                                    recdata.messageprefix = Convert.ToString(fsaXmlDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("PrimaryID"))
                            {
                                drs = fsaXmlDataSet.Tables["PrimaryID"].Select("CarrierParty_Id=0");
                                if (drs.Length > 0)
                                {
                                    recdata.carriercode = Convert.ToString(drs[0]["schemeAgencyID"]);
                                    //Convert.ToString(drs[0]["PrimaryID_Text"]);
                                }
                            }

                            if (fsaXmlDataSet.Tables.Contains("DepartureEvent"))
                            {
                                if (fsaXmlDataSet.Tables["DepartureEvent"].Columns.Contains("DepartureOccurrenceDateTime"))
                                {
                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"]).Split('T');
                                    recdata.fltday = fltdate[0];
                                    //recdata.fltmonth = currentLineRCTText[2] != "" ? currentLineRCTText[2].Substring(2, 3) : "";
                                    recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)";
                                }
                                //recdata.fltorg = currentLineRCTText[3] != "" ? currentLineRCTText[3] : "";
                            }

                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment"))
                            {
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("TransportSplitDescription"))
                                {
                                    recdata.pcsindicator = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"]);
                                }
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("PieceQuantity"))
                                    recdata.numofpcs = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"]);
                            }
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {
                                    if (drs[0].Table.Columns.Contains("UnitCode"))
                                        recdata.weightcode = Convert.ToString(drs[0]["UnitCode"]);
                                    if (drs[0].Table.Columns.Contains("AssociatedStatusConsignment_GrossWeightMeasure_Text"))
                                        recdata.weight = Convert.ToString(drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"]);
                                }
                            }

                            //recdata.seccarriercode = currentLineRCTText[5].ToString();
                            if (fsaXmlDataSet.Tables.Contains("AssociatedReceivedFromParty"))
                            {
                                if (fsaXmlDataSet.Tables["AssociatedReceivedFromParty"].Columns.Contains("Name"))
                                    recdata.name = Convert.ToString(fsaXmlDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Name"]);
                            }
                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                            fsanodes[fsanodes.Length - 1] = recdata;
                            #endregion
                        }
                        break;
                    #endregion


                    #region[DIS]
                    case "DIS":
                        {
                            #region DIS

                            if (fsaXmlDataSet.Tables.Contains("ReportedStatus"))
                            {
                                if (fsaXmlDataSet.Tables["ReportedStatus"].Columns.Contains("ReasonCode"))
                                {
                                    recdata.messageprefix = Convert.ToString(fsaXmlDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"]);
                                }
                            }

                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLogisticsTransportMovement"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Columns.Contains("ID"))
                                {
                                    recdata.flightnum = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("DepartureEvent"))
                            {
                                if (fsaXmlDataSet.Tables["DepartureEvent"].Columns.Contains("DepartureOccurrenceDateTime"))
                                {
                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"]).Split('T');
                                    recdata.fltday = fltdate[0];
                                    //recdata.fltmonth = currentLineRCTText[2] != "" ? currentLineRCTText[2].Substring(2, 3) : "";
                                    recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)";
                                }
                                //recdata.fltorg = currentLineRCTText[3] != "" ? currentLineRCTText[3] : "";
                            }
                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLocation"))
                            {
                                recdata.airportcode = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"]);
                            }
                            //recdata.infocode = currentLineDISText[4] != "" ? currentLineDISText[4] : "";
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment"))
                            {
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("TransportSplitDescription"))
                                {
                                    recdata.pcsindicator = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"]);
                                }
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("PieceQuantity"))
                                    recdata.numofpcs = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"]);
                            }
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {
                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("UnitCode"))
                                    {
                                        recdata.weightcode = Convert.ToString(drs[0]["UnitCode"]);
                                    }
                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("AssociatedStatusConsignment_GrossWeightMeasure_Text"))

                                    {
                                        recdata.weight = Convert.ToString(drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"]);
                                    }
                                }
                            }

                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                            fsanodes[fsanodes.Length - 1] = recdata;
                            #endregion
                        }
                        break;
                    #endregion[DIS]

                    #region Blank 
                    case "AWD":
                    case "CCD":
                    case "DDL":
                    case "AWR":
                    #endregion

                    case "ARR":
                        {
                            #region [ARR]

                            if (fsaXmlDataSet.Tables.Contains("ReportedStatus"))
                            {
                                if (fsaXmlDataSet.Tables["ReportedStatus"].Columns.Contains("ReasonCode"))
                                {
                                    recdata.messageprefix = Convert.ToString(fsaXmlDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("SpecifiedEvent"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedEvent"].Columns.Contains("OccurrenceDateTime"))
                                {
                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"]).Split('T');
                                    if (fltdate.Length > 1)
                                    {
                                        recdata.updatedonday = fltdate[0];
                                        recdata.updatedontime = fltdate[1];
                                    }
                                    else
                                    {
                                        ErrorMsg = "Seprate date and time with T from SpecifiedEvent";
                                        flag = false;
                                        //return false;
                                        return (flag, fsadata, fsanodes, custominfo, uld, othinfoarray, ErrorMsg);
                                    }
                                }
                            }

                            if (fsaXmlDataSet.Tables.Contains("ArrivalEvent"))
                            {
                                if (fsaXmlDataSet.Tables["ArrivalEvent"].Columns.Contains("ArrivalOccurrenceDateTime"))
                                {

                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalOccurrenceDateTime"]).Split('T');

                                    if (fltdate.Length > 1)
                                    {

                                        recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                                        //recdata.fltmonth = DateTime.UtcNow.ToString("MMM").ToUpper();
                                        recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1].Substring(0, 8);
                                    }
                                    else
                                    {
                                        ErrorMsg = "Seprate date and time with T from ArrivalEvent";
                                        flag = false;
                                        //return false;

                                        return (flag, fsadata, fsanodes, custominfo, uld, othinfoarray, ErrorMsg);


                                    }
                                }
                            }
                            else if (fsaXmlDataSet.Tables.Contains("MessageHeaderDocument"))
                            {
                                if (fsaXmlDataSet.Tables["MessageHeaderDocument"].Columns.Contains("IssueDateTime"))
                                {

                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["IssueDateTime"]).Split('T');
                                    if (fltdate.Length > 1)
                                    {
                                        recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                                        recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1].Substring(0, 8);
                                    }
                                    else
                                    {
                                        ErrorMsg = "Seperate date and time with T from IssueDateTime tag";
                                        flag = false;
                                        //return false;
                                        return (flag, fsadata, fsanodes, custominfo, uld, othinfoarray, ErrorMsg);


                                    }
                                }
                            }
                            else
                            {
                                recdata.fltday = DateTime.UtcNow.ToString("MM/dd/yyyy");
                                recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8);
                            }
                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLocation"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedLocation"].Columns.Contains("ID"))
                                {
                                    recdata.fltdest = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLogisticsTransportMovement"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Columns.Contains("ID"))
                                {
                                    recdata.flightnum = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"]);
                                }
                            }


                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment"))
                            {
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("TransportSplitDescription"))
                                {
                                    recdata.pcsindicator = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"]);
                                }
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("PieceQuantity"))
                                {
                                    recdata.numofpcs = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {

                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("UnitCode"))
                                    {
                                        recdata.weightcode = Convert.ToString(drs[0]["UnitCode"]);
                                    }
                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("AssociatedStatusConsignment_GrossWeightMeasure_Text"))

                                    {
                                        recdata.weight = Convert.ToString(drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"]);
                                    }
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("UsedLogisticsTransportMeans"))
                            {
                                if (fsaXmlDataSet.Tables["UsedLogisticsTransportMeans"].Columns.Contains("Name"))
                                {
                                    recdata.name = Convert.ToString(fsaXmlDataSet.Tables["UsedLogisticsTransportMeans"].Rows[0]["Name"]);
                                }
                            }



                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                            fsanodes[fsanodes.Length - 1] = recdata;


                            #endregion
                        }
                        break;

                    #region[TGC]
                    case "TGC":
                        {
                            #region NFD
                            recdata.messageprefix = Convert.ToString(fsaXmlDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"]);
                            string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"]).Split('T');
                            recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                            //recdata.fltmonth = DateTime.UtcNow.ToString("MMM").ToUpper();
                            recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1];
                            recdata.timeindicator = Convert.ToString(fsaXmlDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalDateTimeTypeCode"]);
                            recdata.airportcode = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"]);
                            recdata.pcsindicator = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"]);
                            recdata.numofpcs = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"]);
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {
                                    recdata.weightcode = Convert.ToString(drs[0]["UnitCode"]);
                                    recdata.weight = Convert.ToString(drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"]);
                                }
                            }
                            recdata.name = Convert.ToString(fsaXmlDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Name"]);

                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                            fsanodes[fsanodes.Length - 1] = recdata;
                            #endregion
                        }
                        break;
                    #endregion


                    #region [PRE]
                    case "PRE":
                        {
                            #region MAN/AWR
                            recdata.messageprefix = Convert.ToString(fsaXmlDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"]);
                            if (fsaXmlDataSet.Tables.Contains("PrimaryID"))
                            {
                                drs = fsaXmlDataSet.Tables["PrimaryID"].Select("CarrierParty_Id=0");
                                if (drs.Length > 0)
                                {
                                    recdata.carriercode = Convert.ToString(drs[0]["schemeAgencyID"]);
                                    //Convert.ToString(drs[0]["PrimaryID_Text"]);
                                }
                            }
                            recdata.flightnum = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"]);
                            string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"]).Split('T');
                            recdata.fltday = fltdate[0];
                            //recdata.fltmonth = split[0].Substring(2, 3);
                            recdata.flttime = fltdate[1];
                            //recdata.daychangeindicator = split[1] + ",";
                            //recdata.fltorg = msg[3].Substring(0, 3);
                            recdata.fltdest = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"]);
                            recdata.airportcode = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"]);
                            recdata.pcsindicator = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"]);
                            recdata.numofpcs = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"]);
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {
                                    recdata.weightcode = Convert.ToString(drs[0]["UnitCode"]);
                                    recdata.weight = Convert.ToString(drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"]);
                                }
                            }
                            recdata.timeindicator = Convert.ToString(fsaXmlDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalDateTimeTypeCode"]);
                            recdata.depttime = fltdate[1];
                            string[] fltarrdate = Convert.ToString(fsaXmlDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalOccurrenceDateTime"]).Split('T');
                            recdata.arrivaltime = fltarrdate[1];

                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                            fsanodes[fsanodes.Length - 1] = recdata;
                            #endregion
                        }
                        break;
                    #endregion




                    #region[RCF]
                    case "RCF":
                        {
                            #region FUS/RCF
                            recdata.messageprefix = Convert.ToString(fsaXmlDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"]);
                            if (fsaXmlDataSet.Tables.Contains("CarrierParty"))
                            {
                                if (fsaXmlDataSet.Tables.Contains("PrimaryID"))
                                {
                                    if (fsaXmlDataSet.Tables["PrimaryID"].Columns.Contains("CarrierParty_Id"))
                                    {
                                        drs = fsaXmlDataSet.Tables["PrimaryID"].Select("CarrierParty_Id=0");
                                        if (drs.Length > 0)
                                        {
                                            recdata.carriercode = Convert.ToString(drs[0]["schemeAgencyID"]);
                                            //Convert.ToString(drs[0]["PrimaryID_Text"]);
                                        }
                                    }
                                }
                            }

                            if (fsaXmlDataSet.Tables.Contains("SpecifiedEvent"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedEvent"].Columns.Contains("OccurrenceDateTime"))
                                {
                                    string[] updatedon = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"]).Split('T');
                                    recdata.updatedonday = updatedon[0];
                                    recdata.updatedontime = updatedon[1];
                                }
                            }
                            recdata.flightnum = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"]);
                            //recdata.carriercode = recdata.carriercode.Length < 1 ? recdata.flightnum.Substring(0, 2) : recdata.carriercode;

                            if (fsaXmlDataSet.Tables.Contains("ArrivalEvent"))
                            {
                                string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalOccurrenceDateTime"]).Split('T');
                                recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                                //recdata.fltmonth = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(2, 3) : "";
                                recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1];
                                //recdata.daychangeindicator = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                            }

                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLocation"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedLocation"].Columns.Contains("ID"))
                                {
                                    recdata.fltdest = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment"))
                            {
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("TransportSplitDescription"))
                                    recdata.pcsindicator = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"]);
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("PieceQuantity"))
                                    recdata.numofpcs = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"]);
                            }
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {
                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("UnitCode"))
                                    {
                                        recdata.weightcode = Convert.ToString(drs[0]["UnitCode"]);
                                    }
                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("AssociatedStatusConsignment_GrossWeightMeasure_Text"))

                                    {
                                        recdata.weight = Convert.ToString(drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"]);
                                    }
                                }
                            }

                            if (fsaXmlDataSet.Tables.Contains("ArrivalEvent"))
                            {
                                if (fsaXmlDataSet.Tables["ArrivalEvent"].Columns.Contains("ArrivalOccurrenceDateTime"))
                                {
                                    string[] fltarrdate = Convert.ToString(fsaXmlDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalOccurrenceDateTime"]).Split('T');
                                    recdata.arrivaltime = fltarrdate[1];
                                }
                                if (fsaXmlDataSet.Tables["ArrivalEvent"].Columns.Contains("ArrivalDateTimeTypeCode"))
                                    recdata.timeindicator = Convert.ToString(fsaXmlDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalDateTimeTypeCode"]);
                            }
                            else
                            {
                                recdata.arrivaltime = "00:00:00";
                            }

                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                            fsanodes[fsanodes.Length - 1] = recdata;
                            #endregion
                        }
                        break;
                    #endregion


                    #region BKD
                    case "BKD":
                        {
                            #region BKD
                            recdata.messageprefix = Convert.ToString(fsaXmlDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"]);
                            if (fsaXmlDataSet.Tables.Contains("PrimaryID"))
                            {
                                if (fsaXmlDataSet.Tables["PrimaryID"].Columns.Contains("CarrierParty_Id=0"))
                                {
                                    drs = fsaXmlDataSet.Tables["PrimaryID"].Select("CarrierParty_Id=0");
                                    if (drs.Length > 0)
                                    {
                                        recdata.carriercode = Convert.ToString(drs[0]["schemeAgencyID"]);
                                        //Convert.ToString(drs[0]["PrimaryID_Text"]);
                                    }
                                }
                            }

                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLogisticsTransportMovement"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Columns.Contains("ID"))
                                {
                                    recdata.flightnum = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("SpecifiedEvent"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedEvent"].Columns.Contains("OccurrenceDateTime"))
                                {
                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"]).Split('T');
                                    recdata.updatedonday = fltdate[0];
                                    recdata.updatedontime = fltdate[1];

                                    recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                                    //recdata.fltmonth = Convert.ToString(split[0].ToString()) == "" ? System.DateTime.Now.Month.ToString() : (DateTime.ParseExact(split[0].Substring(2, 3), "MMM", CultureInfo.InvariantCulture).Month).ToString();
                                    recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1];
                                    recdata.depttime = fltdate[1];
                                }
                                if (fsaXmlDataSet.Tables["DepartureEvent"].Columns.Contains("DepartureDateTimeTypeCode"))
                                    recdata.timeindicator = Convert.ToString(fsaXmlDataSet.Tables["DepartureEvent"].Rows[0]["DepartureDateTimeTypeCode"]);
                            }

                            //recdata.daychangeindicator = split[1] + ",";
                            //recdata.fltorg = msg[3].Substring(0, 3);
                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLocation"))
                            {
                                recdata.fltorg = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"]);
                                recdata.fltdest = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[fsaXmlDataSet.Tables["SpecifiedLocation"].Rows.Count - 1]["ID"]);
                            }

                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment"))
                            {
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("TransportSplitDescription"))
                                    recdata.pcsindicator = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"]);
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("PieceQuantity"))
                                    recdata.numofpcs = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"]);
                            }
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {
                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("UnitCode"))
                                    {
                                        recdata.weightcode = Convert.ToString(drs[0]["UnitCode"]);
                                    }
                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("AssociatedStatusConsignment_GrossWeightMeasure_Text"))

                                    {
                                        recdata.weight = Convert.ToString(drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"]);
                                    }
                                }
                            }




                            if (fsaXmlDataSet.Tables.Contains("GrossVolumeMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["GrossVolumeMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {
                                    if (fsaXmlDataSet.Tables["GrossVolumeMeasure"].Columns.Contains("UnitCode"))
                                    {
                                        recdata.volumecode = Convert.ToString(drs[0]["UnitCode"]);
                                    }
                                    if (fsaXmlDataSet.Tables["GrossVolumeMeasure"].Columns.Contains("GrossVolumeMeasure_Text"))
                                    {
                                        recdata.volumeamt = Convert.ToString(drs[0]["GrossVolumeMeasure_Text"]);
                                    }
                                }
                            }

                            recdata.densityindicator = "DG";
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment"))
                            {
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("DensityGroupCode"))
                                {
                                    recdata.densitygroup = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["DensityGroupCode"]);
                                }
                            }
                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                            fsanodes[fsanodes.Length - 1] = recdata;

                            #endregion
                        }
                        break;
                    #endregion

                    #region TRM
                    case "TRM":
                        {
                            #region TRM

                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                            fsanodes[fsanodes.Length - 1] = recdata;
                            #endregion
                        }
                        break;
                    #endregion

                    #region TFD
                    case "TFD":
                        {
                            #region TFD
                            recdata.messageprefix = Convert.ToString(fsaXmlDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"]);
                            if (fsaXmlDataSet.Tables.Contains("PrimaryID"))
                            {
                                drs = fsaXmlDataSet.Tables["PrimaryID"].Select("CarrierParty_Id=0");
                                if (drs.Length > 0)
                                {
                                    recdata.carriercode = Convert.ToString(drs[0]["schemeAgencyID"]);
                                    //Convert.ToString(drs[0]["PrimaryID_Text"]);
                                }
                            }
                            string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"]).Split('T');
                            recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                            //recdata.fltmonth = currentLineTFDText[2] != "" ? currentLineTFDText[2].Substring(2, 3) : "";
                            recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1];
                            recdata.airportcode = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"]);
                            recdata.pcsindicator = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"]);
                            recdata.numofpcs = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"]);
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {
                                    recdata.weightcode = Convert.ToString(drs[0]["UnitCode"]);
                                    recdata.weight = Convert.ToString(drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"]);
                                }
                            }
                            recdata.transfermanifestnumber = Convert.ToString(fsaXmlDataSet.Tables["AssociatedManifestDocument"].Rows[0]["ID"]);


                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                            fsanodes[fsanodes.Length - 1] = recdata;
                            #endregion
                        }
                        break;
                    #endregion

                    #region CRC
                    case "CRC":
                        {
                            #region CRC
                            recdata.messageprefix = Convert.ToString(fsaXmlDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"]);
                            string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"]).Split('T');
                            recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                            //recdata.fltmonth = msg[1].Substring(2, 3);
                            recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1];
                            recdata.airportcode = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"]);
                            recdata.pcsindicator = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"]);
                            recdata.numofpcs = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"]);
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {
                                    recdata.weightcode = Convert.ToString(drs[0]["UnitCode"]);
                                    recdata.weight = Convert.ToString(drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("PrimaryID"))
                            {
                                drs = fsaXmlDataSet.Tables["PrimaryID"].Select("CarrierParty_Id=0");
                                if (drs.Length > 0)
                                {
                                    recdata.carriercode = Convert.ToString(drs[0]["schemeAgencyID"]);
                                    //Convert.ToString(drs[0]["PrimaryID_Text"]);
                                }
                            }
                            recdata.flightnum = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"]);
                            recdata.fltdest = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"]);
                            //recdata.fltorg = msg[6].Substring(3);

                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                            fsanodes[fsanodes.Length - 1] = recdata;
                            #endregion
                        }
                        break;
                    #endregion

                    #region[OCI]
                    case "OCI":
                        {
                            #region OCI
                            MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
                            custom.IsoCountryCodeOci = Convert.ToString(fsaXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["CountryID"]);
                            custom.InformationIdentifierOci = Convert.ToString(fsaXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["SubjectCode"]);
                            custom.CsrIdentifierOci = Convert.ToString(fsaXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["ContentCode"]);
                            custom.SupplementaryCsrIdentifierOci = Convert.ToString(fsaXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["Content"]);
                            custom.consigref = awbref;

                            Array.Resize(ref custominfo, custominfo.Length + 1);
                            custominfo[custominfo.Length - 1] = custom;
                            #endregion
                        }
                        break;
                    #endregion

                    #region[ULD]
                    case "ULD":
                        {
                            #region ULD
                            Array.Resize(ref uld, 1);
                            uld[0].uldtype = Convert.ToString(fsaXmlDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["CharacteristicCode"]);
                            uld[0].uldsrno = Convert.ToString(fsaXmlDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["ID"]);
                            if (fsaXmlDataSet.Tables.Contains("PrimaryID"))
                            {
                                drs = fsaXmlDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0");
                                if (drs.Length > 0)
                                {
                                    uld[0].uldowner = Convert.ToString(drs[0]["schemeAgencyID"]);
                                    //Convert.ToString(drs[0]["PrimaryID_Text"]);
                                }
                            }
                            uld[0].uldloadingindicator = Convert.ToString(fsaXmlDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[0]["OperationalStatusCode"]);

                            #endregion
                        }
                        break;
                    #endregion

                    #region[OSI]
                    case "OSI":
                        {
                            #region OSI
                            Array.Resize(ref othinfoarray, othinfoarray.Length + 1);
                            othinfoarray[othinfoarray.Length - 1].otherserviceinfo1 = Convert.ToString(fsaXmlDataSet.Tables["HandlingOSIInstructions"].Rows[0]["Description"]);
                            #endregion
                        }
                        break;
                    #endregion

                    #region[DLV]
                    case "DLV":
                        {
                            #region FSU/DLV
                            if (fsaXmlDataSet.Tables.Contains("ReportedStatus"))
                            {
                                if (fsaXmlDataSet.Tables["ReportedStatus"].Columns.Contains("ReasonCode"))
                                {
                                    recdata.messageprefix = Convert.ToString(fsaXmlDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"]);
                                }
                            }

                            if (fsaXmlDataSet.Tables.Contains("SpecifiedEvent"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedEvent"].Columns.Contains("OccurrenceDateTime"))
                                {
                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"]).Split('T');
                                    if (fltdate.Length > 1)
                                    {
                                        recdata.updatedonday = fltdate[0];
                                        recdata.updatedontime = fltdate[1];
                                        recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                                        recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1].Substring(0, 8);
                                    }
                                    else
                                    {
                                        ErrorMsg = "Seprate date and time with T from SpecifiedEvent";
                                        flag = false;
                                        //return false;
                                        return (flag, fsadata, fsanodes, custominfo, uld, othinfoarray, ErrorMsg);

                                    }
                                }
                            }

                            if (fsaXmlDataSet.Tables.Contains("DepartureEvent"))
                            {
                                if (fsaXmlDataSet.Tables["DepartureEvent"].Columns.Contains("DepartureOccurrenceDateTime"))
                                {

                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"]).Split('T');

                                    if (fltdate.Length > 1)
                                    {
                                        recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                                        //recdata.fltmonth = DateTime.UtcNow.ToString("MMM").ToUpper();
                                        recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1].Substring(0, 8);
                                    }
                                    else
                                    {
                                        ErrorMsg = "Seprate date and time with T from DepartureEvent";
                                        flag = false;
                                        //return false;
                                        return (flag, fsadata, fsanodes, custominfo, uld, othinfoarray, ErrorMsg);


                                    }
                                }
                            }
                            else if (fsaXmlDataSet.Tables.Contains("MessageHeaderDocument"))
                            {
                                if (fsaXmlDataSet.Tables["MessageHeaderDocument"].Columns.Contains("IssueDateTime"))
                                {

                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["IssueDateTime"]).Split('T');
                                    if (fltdate.Length > 1)
                                    {
                                        recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                                        recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1].Substring(0, 8);
                                    }
                                    else
                                    {
                                        ErrorMsg = "Seperate date and time with T from IssueDateTime tag";
                                        flag = false;
                                        //return false;
                                        return (flag, fsadata, fsanodes, custominfo, uld, othinfoarray, ErrorMsg);


                                    }
                                }
                            }
                            else
                            {
                                recdata.fltday = DateTime.UtcNow.ToString("MM/dd/yyyy");
                                recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8);
                            }
                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLocation"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedLocation"].Columns.Contains("ID"))
                                {
                                    //recdata.fltdest = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"]);
                                    recdata.fltorg = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"]);
                                    recdata.fltdest = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[fsaXmlDataSet.Tables["SpecifiedLocation"].Rows.Count - 1]["ID"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLogisticsTransportMovement"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Columns.Contains("ID"))
                                {
                                    recdata.flightnum = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment"))
                            {
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("TransportSplitDescription"))
                                {
                                    recdata.pcsindicator = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"]);
                                }
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("PieceQuantity"))
                                {
                                    recdata.numofpcs = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {

                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("UnitCode"))
                                    {
                                        recdata.weightcode = Convert.ToString(drs[0]["UnitCode"]);
                                    }
                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("AssociatedStatusConsignment_GrossWeightMeasure_Text"))

                                    {
                                        recdata.weight = Convert.ToString(drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"]);
                                    }
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("AssociatedReceivedFromParty"))
                            {
                                if (fsaXmlDataSet.Tables["AssociatedReceivedFromParty"].Columns.Contains("Name"))
                                {
                                    recdata.name = Convert.ToString(fsaXmlDataSet.Tables["AssociatedReceivedFromParty"].Rows[0]["Name"]);
                                }
                            }
                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                            fsanodes[fsanodes.Length - 1] = recdata;
                            #endregion
                        }
                        break;
                    #endregion

                    #region[MAN]


                    case "MAN":
                        {
                            #region FSU/MAN
                            if (fsaXmlDataSet.Tables.Contains("ReportedStatus"))
                            {
                                if (fsaXmlDataSet.Tables["ReportedStatus"].Columns.Contains("ReasonCode"))
                                {
                                    recdata.messageprefix = Convert.ToString(fsaXmlDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("CarrierParty"))
                            {
                                if (fsaXmlDataSet.Tables.Contains("PrimaryID"))
                                {
                                    if (fsaXmlDataSet.Tables["PrimaryID"].Columns.Contains("CarrierParty_Id"))
                                    {
                                        drs = fsaXmlDataSet.Tables["PrimaryID"].Select("CarrierParty_Id=0");
                                        if (drs.Length > 0)
                                        {
                                            recdata.carriercode = Convert.ToString(drs[0]["schemeAgencyID"]);
                                            //Convert.ToString(drs[0]["PrimaryID_Text"]);
                                        }
                                    }
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLogisticsTransportMovement"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Columns.Contains("ID"))
                                {
                                    recdata.flightnum = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"]);
                                }
                            }

                            //string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"]).Split('T');
                            if (fsaXmlDataSet.Tables.Contains("DepartureEvent"))
                            {
                                if (fsaXmlDataSet.Tables["DepartureEvent"].Columns.Contains("DepartureOccurrenceDateTime"))
                                {

                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"]).Split('T');

                                    if (fltdate.Length > 1)
                                    {

                                        recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                                        //recdata.fltmonth = DateTime.UtcNow.ToString("MMM").ToUpper();
                                        recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1].Substring(0, 8);
                                    }
                                    else
                                    {
                                        ErrorMsg = "Seprate date and time with T from DepartureEvent";
                                        flag = false;
                                        //return false;
                                        return (flag, fsadata, fsanodes, custominfo, uld, othinfoarray, ErrorMsg);


                                    }
                                }
                            }
                            //  recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                            //recdata.fltmonth = DateTime.UtcNow.ToString("MMM").ToUpper();
                            // recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1];
                            //recdata.fltorg = currentMANLineText[3] != "" ? currentMANLineText[3].Substring(0, 3) : "";
                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLocation"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedLocation"].Columns.Contains("ID"))
                                {
                                    recdata.fltdest = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"]);
                                }
                            }

                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment"))
                            {
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("TransportSplitDescription"))
                                {
                                    recdata.pcsindicator = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"]);
                                }
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("PieceQuantity"))
                                {
                                    recdata.numofpcs = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("OriginLocation"))
                            {
                                if (fsaXmlDataSet.Tables["OriginLocation"].Columns.Contains("ID"))
                                {

                                    recdata.fltorg = Convert.ToString(fsaXmlDataSet.Tables["OriginLocation"].Rows[0]["ID"]);
                                }
                            }

                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {
                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("UnitCode"))
                                    {
                                        recdata.weightcode = Convert.ToString(drs[0]["UnitCode"]);
                                    }
                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("AssociatedStatusConsignment_GrossWeightMeasure_Text"))

                                    {
                                        recdata.weight = Convert.ToString(drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"]);
                                    }
                                }
                            }

                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                            fsanodes[fsanodes.Length - 1] = recdata;
                            #endregion
                        }
                        break;
                    #endregion

                    #region[DEP]
                    case "DEP":
                        {
                            #region FSU/DEP
                            if (fsaXmlDataSet.Tables.Contains("ReportedStatus"))
                            {
                                if (fsaXmlDataSet.Tables["ReportedStatus"].Columns.Contains("ReasonCode"))
                                {
                                    recdata.messageprefix = Convert.ToString(fsaXmlDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("CarrierParty"))
                            {
                                if (fsaXmlDataSet.Tables.Contains("PrimaryID"))
                                {
                                    if (fsaXmlDataSet.Tables["PrimaryID"].Columns.Contains("CarrierParty_Id"))
                                    {
                                        drs = fsaXmlDataSet.Tables["PrimaryID"].Select("CarrierParty_Id=0");
                                        if (drs.Length > 0)
                                        {
                                            recdata.carriercode = Convert.ToString(drs[0]["schemeAgencyID"]);
                                            //Convert.ToString(drs[0]["PrimaryID_Text"]);
                                        }
                                    }
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLogisticsTransportMovement"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Columns.Contains("ID"))
                                {
                                    recdata.flightnum = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"]);
                                }
                            }
                            //string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"]).Split('T');
                            //recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                            //recdata.fltmonth = currentLineText[2] != "" ? currentLineText[2].Substring(2, 3) : "";
                            //recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1];
                            //recdata.fltorg = currentLineText[3] != "" ? currentLineText[3].Substring(0, 3) : "";



                            if (fsaXmlDataSet.Tables.Contains("SpecifiedEvent"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedEvent"].Columns.Contains("OccurrenceDateTime"))
                                {
                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"]).Split('T');
                                    if (fltdate.Length > 1)
                                    {
                                        recdata.updatedonday = fltdate[0];
                                        recdata.updatedontime = fltdate[1];

                                        //recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                                        ////recdata.fltmonth = DateTime.UtcNow.ToString("MMM").ToUpper();
                                        //recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1].Substring(0, 8);
                                    }
                                    else
                                    {
                                        ErrorMsg = "Seprate date and time with T from DepartureEvent";
                                        flag = false;
                                        //return false;
                                        return (flag, fsadata, fsanodes, custominfo, uld, othinfoarray, ErrorMsg);

                                    }
                                }
                            }

                            if (fsaXmlDataSet.Tables.Contains("DepartureEvent"))
                            {
                                if (fsaXmlDataSet.Tables["DepartureEvent"].Columns.Contains("DepartureOccurrenceDateTime"))
                                {

                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"]).Split('T');

                                    if (fltdate.Length > 1)
                                    {

                                        recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                                        //recdata.fltmonth = DateTime.UtcNow.ToString("MMM").ToUpper();
                                        recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1].Substring(0, 8);
                                    }
                                    else
                                    {
                                        ErrorMsg = "Seprate date and time with T from DepartureEvent";
                                        flag = false;
                                        //return false;
                                        return (flag, fsadata, fsanodes, custominfo, uld, othinfoarray, ErrorMsg);


                                    }
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLocation"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedLocation"].Columns.Contains("ID"))
                                {
                                    for (int k = 0; k < fsaXmlDataSet.Tables["SpecifiedLocation"].Rows.Count; k++)
                                    {
                                        if (k == 0)
                                        {
                                            recdata.fltorg = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[k]["ID"]);
                                        }
                                        else
                                        { recdata.fltdest = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[k]["ID"]); }
                                    }
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment"))
                            {
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("TransportSplitDescription"))
                                {
                                    recdata.pcsindicator = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"]);
                                }
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("PieceQuantity"))
                                {
                                    recdata.numofpcs = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"]);
                                }
                            }
                            //if (fsaXmlDataSet.Tables.Contains("OriginLocation"))
                            //{
                            //    if (fsaXmlDataSet.Tables["OriginLocation"].Columns.Contains("ID"))
                            //    {
                            //        recdata.fltorg = Convert.ToString(fsaXmlDataSet.Tables["OriginLocation"].Rows[0]["ID"]);
                            //    }
                            //}

                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {
                                    recdata.weightcode = Convert.ToString(drs[0]["UnitCode"]);
                                    recdata.weight = Convert.ToString(drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"]);
                                }
                            }

                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                            fsanodes[fsanodes.Length - 1] = recdata;
                            #endregion
                        }
                        break;
                    #endregion

                    #region NFD
                    case "NFD":
                        {
                            if (fsaXmlDataSet.Tables.Contains("ReportedStatus"))
                            {
                                if (fsaXmlDataSet.Tables["ReportedStatus"].Columns.Contains("ReasonCode"))
                                {
                                    recdata.messageprefix = Convert.ToString(fsaXmlDataSet.Tables["ReportedStatus"].Rows[0]["ReasonCode"]);
                                }
                            }

                            if (fsaXmlDataSet.Tables.Contains("ArrivalEvent"))
                            {
                                if (fsaXmlDataSet.Tables["ArrivalEvent"].Columns.Contains("ArrivalOccurrenceDateTime"))
                                {
                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalOccurrenceDateTime"]).Split('T');
                                    if (fltdate.Length > 1)
                                    {
                                        recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                                        //recdata.fltmonth = DateTime.UtcNow.ToString("MMM").ToUpper();
                                        recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1].Substring(0, 8);
                                    }
                                    else
                                    {
                                        ErrorMsg = "Seprate date and time with T from ArrivalEvent";
                                        flag = false;
                                        //return false;
                                        return (flag, fsadata, fsanodes, custominfo, uld, othinfoarray, ErrorMsg);


                                    }
                                }
                            }

                            if (fsaXmlDataSet.Tables.Contains("DepartureEvent"))
                            {
                                if (fsaXmlDataSet.Tables["DepartureEvent"].Columns.Contains("DepartureOccurrenceDateTime"))
                                {
                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"]).Split('T');
                                    recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                                    //recdata.fltmonth = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(2, 3) : "";
                                    recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1];
                                    //recdata.daychangeindicator = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                }
                            }
                            else if (fsaXmlDataSet.Tables.Contains("SpecifiedEvent"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedEvent"].Columns.Contains("OccurrenceDateTime"))
                                {

                                    string[] fltdate = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedEvent"].Rows[0]["OccurrenceDateTime"]).Split('T');
                                    if (fltdate.Length > 1)
                                    {
                                        recdata.fltday = String.IsNullOrEmpty(fltdate[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : fltdate[0];
                                        recdata.flttime = String.IsNullOrEmpty(fltdate[1]) ? DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)" : fltdate[1].Substring(0, 8);
                                    }
                                    else
                                    {
                                        ErrorMsg = "Seprate date and time with T from OccurrenceDateTime tag";
                                        flag = false;
                                        //return false;
                                        return (flag, fsadata, fsanodes, custominfo, uld, othinfoarray, ErrorMsg);


                                    }
                                }
                            }
                            else
                            {
                                recdata.fltday = DateTime.UtcNow.ToString("MM/dd/yyyy");
                                recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8);
                            }


                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLocation"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedLocation"].Columns.Contains("ID"))
                                {
                                    recdata.fltdest = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLocation"].Rows[0]["ID"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("SpecifiedLogisticsTransportMovement"))
                            {
                                if (fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Columns.Contains("ID"))
                                {
                                    recdata.flightnum = Convert.ToString(fsaXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"]);
                                }
                            }

                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment"))
                            {
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("TransportSplitDescription"))
                                {
                                    recdata.pcsindicator = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["TransportSplitDescription"]);
                                }
                                if (fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Columns.Contains("PieceQuantity"))
                                {
                                    recdata.numofpcs = Convert.ToString(fsaXmlDataSet.Tables["AssociatedStatusConsignment"].Rows[0]["PieceQuantity"]);
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("AssociatedStatusConsignment_GrossWeightMeasure"))
                            {
                                drs = fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Select("AssociatedStatusConsignment_Id=0");
                                if (drs.Length > 0)
                                {

                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("UnitCode"))
                                    {
                                        recdata.weightcode = Convert.ToString(drs[0]["UnitCode"]);
                                    }
                                    if (fsaXmlDataSet.Tables["AssociatedStatusConsignment_GrossWeightMeasure"].Columns.Contains("AssociatedStatusConsignment_GrossWeightMeasure_Text"))

                                    {
                                        recdata.weight = Convert.ToString(drs[0]["AssociatedStatusConsignment_GrossWeightMeasure_Text"]);
                                    }
                                }
                            }
                            if (fsaXmlDataSet.Tables.Contains("UsedLogisticsTransportMeans"))
                            {
                                if (fsaXmlDataSet.Tables["UsedLogisticsTransportMeans"].Columns.Contains("Name"))
                                {
                                    recdata.name = Convert.ToString(fsaXmlDataSet.Tables["UsedLogisticsTransportMeans"].Rows[0]["Name"]);
                                }
                            }

                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                            fsanodes[fsanodes.Length - 1] = recdata;

                        }
                        break;
                    #endregion NFD

                    #region[default]
                    default:
                        {
                            flag = false;
                        }
                        break;
                        #endregion
                }
                flag = true;
            }
            catch (Exception ex)
            {
                flag = false;
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                ErrorMsg = "Error Occured when XML Decoding: [[" + ex.Message + "]];"; //[[" + ex.StackTrace + "]]";
                flag = false;
            }
            if (ErrorMsg == "")
                ErrorMsg = "Error Occured when XML Decoding";
            //return flag;
            return (flag, fsadata, fsanodes, custominfo, uld, othinfoarray, ErrorMsg);


        }


        /// <summary>
        /// Method to save XFSU decoded FSU message
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
        /// <returns>Returns true if XFSU message saved successfuly</returns>
        #region Comment
        //public bool SaveandUpdateXFSUMessage(string strMsg, ref MessageData.FSAInfo fsadata, ref MessageData.CommonStruct[] fsanodes,
        //    ref MessageData.customsextrainfo[] customextrainfo, ref MessageData.ULDinfo[] ulddata, ref MessageData.otherserviceinfo[] othinfoarray, 
        //    int refNo, string strMessage, string strMessageFrom, string strFromID, string strStatus)
        //{
        //    SQLServer dtb = new SQLServer();
        //    bool flag = false;
        //    string strFSUBooking = string.Empty;
        //    Cls_BL objBL = new Cls_BL();
        //    try
        //    {
        //        string awbnum = string.Empty, awbprefix = string.Empty;
        //        GenericFunction gf = new GenericFunction();
        //        gf.UpdateInboxFromMessageParameter(refNo, fsadata.airlineprefix + "-" + fsadata.awbnum, string.Empty, string.Empty, string.Empty, "FSU", "FSU", DateTime.Parse("1900-01-01"));

        //        #region Below Segment of FSU/DIS
        //        ///AWB Discrepancy Recorded through FSU/DIS Message
        //        if (fsadata.awbnum.Length > 0 && fsanodes.Length > 0 && fsadata.awbnum.Length > 0 && fsanodes[0].messageprefix.ToUpper() == "DIS")
        //        {
        //            if (fsanodes.Length > 0)
        //            {
        //                for (int i = 0; i < fsanodes.Length; i++)
        //                {
        //                    DateTime strDiscrepancyDate = new DateTime();
        //                    if (fsanodes[i].fltday != "")
        //                    {
        //                        //string strdate = (fsanodes[i].fltmonth + "/" + fsanodes[i].fltday + "/" + DateTime.Now.Year.ToString());
        //                        //strDiscrepancyDate = DateTime.Parse(strdate);
        //                        strDiscrepancyDate = DateTime.ParseExact(fsanodes[i].fltday, "MM/dd/yyyy", null);
        //                    }

        //                    string[] sqlParameterName = new string[]
        //                    {
        //                         "AWBPrefix",
        //                         "AWBNumber",
        //                         "AWBOrigin",
        //                         "AWBDestination",
        //                        "AWBPieces",
        //                        "AWBGrossWeight" ,
        //                        "FlightNumber",
        //                        "DiscrepancyDate",
        //                        "FlightOrigin",
        //                        "DiscrepancyCode",
        //                        "DiscrepancyPcsCode",
        //                        "DiscrepancyPcs",
        //                        "UOM",
        //                        "Discrepancyweight",
        //                        "UpdatedBy",
        //                        "UpdatedOn",
        //                        "OtherServiceInformation"
        //               };
        //                    object[] sqlParameterValue = new object[] {
        //                       fsadata.airlineprefix,
        //                       fsadata.awbnum,
        //                        fsadata.origin,
        //                        fsadata.dest,
        //                        int.Parse(fsadata.pcscnt==""?"0":fsadata.pcscnt),
        //                        decimal.Parse(fsadata.weight==""?"0":fsadata.weight),
        //                        fsanodes[i].flightnum,
        //                        strDiscrepancyDate,
        //                        fsanodes[0].fltorg,
        //                        fsanodes[0].infocode,
        //                        fsanodes[i].pcsindicator,
        //                        int.Parse(fsanodes[i].numofpcs==""?"0":fsanodes[i].numofpcs),
        //                        fsanodes[i].weightcode,
        //                        decimal.Parse(fsanodes[i].weight == "" ? "0" : fsanodes[i].weight),
        //                        "FSU",
        //                        DateTime.Now,
        //                       ""
        //                };
        //                    SqlDbType[] sqlParameter = new SqlDbType[] {
        //                        SqlDbType.VarChar,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.Int,
        //                        SqlDbType.Decimal,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.DateTime,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.Int,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.Decimal,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.DateTime,
        //                        SqlDbType.VarChar
        //                    };
        //                    if (dtb.InsertData("SpSaveFSUAWBDiscrepancy", sqlParameterName, sqlParameter, sqlParameterValue))
        //                        flag = true;
        //                    else
        //                    {
        //                        flag = false;
        //                        return flag;
        //                    }
        //                }
        //            }
        //        }

        //        #endregion

        //        # region Below Segmnet of FSU/DLV Message
        //        ///Make  AWB Delivered  Throgh  DLV Message 
        //        if (fsadata.awbnum.Length > 0 && fsanodes.Length > 0 && fsadata.awbnum.Length > 0 && fsanodes[0].messageprefix.ToUpper() == "DLV")
        //        {
        //            if (fsanodes.Length > 0)
        //            {
        //                for (int i = 0; i < fsanodes.Length; i++)
        //                {
        //                    DateTime DeliveryDate = new DateTime();
        //                    if (fsanodes[i].fltday != "")
        //                    {
        //                        //string strdate = (fsanodes[i].fltmonth + "/" + fsanodes[i].fltday + "/" + DateTime.Now.Year.ToString());
        //                        DeliveryDate = DateTime.ParseExact(fsanodes[i].fltday, "MM/dd/yyyy", null);
        //                    }
        //                    string[] sqlParameterName = new string[]
        //                    {
        //                        "AWBPrefix",
        //                        "AWBNo",
        //                        "Origin",
        //                        "Destination",
        //                        "AWbPcs",
        //                        "AWbGrossWt" ,
        //                        "PieceCode",
        //                        "Deliverypcs",
        //                        "WeightCode",
        //                        "DeliveryGross",
        //                        "FlightDestination ",
        //                        "Dname",
        //                        "Deliverydate",
        //                        "UpdatedBy",
        //                    };
        //                    object[] sqlParameterValue = new object[]
        //                    {
        //                        fsadata.airlineprefix,
        //                        fsadata.awbnum,
        //                        fsadata.origin,
        //                        fsadata.dest,
        //                        int.Parse(fsadata.pcscnt==""?"0":fsadata.pcscnt),
        //                        decimal.Parse(fsadata.weight==""?"0":fsadata.weight),
        //                        fsanodes[i].pcsindicator,
        //                        fsanodes[i].numofpcs,
        //                        fsanodes[i].weightcode,
        //                        //fsanodes[i].weight,
        //                        decimal.Parse(fsadata.weight==""?"0":fsadata.weight),
        //                        fsanodes[0].fltdest,
        //                        fsanodes[0].name,
        //                        DeliveryDate,
        //                        "FSU",
        //                    };
        //                    SqlDbType[] sqlParameter = new SqlDbType[] {
        //                        SqlDbType.VarChar,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.Int,
        //                        SqlDbType.Decimal,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.Int,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.Decimal,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.DateTime,
        //                        SqlDbType.VarChar
        //                    };
        //                    if (dtb.InsertData("MakeAWBDeliveryorderofFSUMessage", sqlParameterName, sqlParameter, sqlParameterValue))
        //                        flag = true;
        //                    else
        //                    {
        //                        flag = false;
        //                        return flag;
        //                    }
        //                }
        //            }
        //        }
        //        #endregion


        //        #region Below Segment of RCT Message
        //        ///Make  Received Freight from other Airline
        //        if (fsadata.awbnum.Length > 0 && fsanodes.Length > 0 && fsadata.awbnum.Length > 0 && fsanodes[0].messageprefix.ToUpper() == "RCT")
        //        {
        //            if (fsanodes.Length > 0)
        //            {
        //                for (int i = 0; i < fsanodes.Length; i++)
        //                {

        //                    DateTime ReceivedDate = new DateTime();
        //                    string ReceivedTime = string.Empty;
        //                    if (fsanodes[i].fltday != "")
        //                    {
        //                        if (fsanodes[i].fltday.Length > 0 || fsanodes[i].fltmonth.Length > 0)
        //                            ReceivedDate = DateTime.ParseExact(fsanodes[i].fltday, "MM/dd/yyyy", null);
        //                        //ReceivedDate = DateTime.Parse(DateTime.Parse("1." + fsanodes[i].fltmonth + " 2008").Month + "/" + fsanodes[i].fltday + "/" + System.DateTime.Today.Year);
        //                        if (fsanodes[i].flttime.Length > 0)
        //                            ReceivedTime = fsanodes[i].flttime;
        //                        //ReceivedTime = fsanodes[i].flttime.Substring(0, 2) + ":" + fsanodes[i].flttime.Substring(2) + ":00";
        //                    }

        //                    string[] sqlParameterName = new string[]
        //                    {
        //                         "AWBPrefix",
        //                         "AWBNo",
        //                         "Origin",
        //                         "Destination",
        //                        "AWbPcs",
        //                        "AWbWeightCode",
        //                        "AWbGrossWt" ,
        //                         "TransferCarrierCode",
        //                        "ReceivedShipmentDate",
        //                         "ReceivedOrigin",
        //                         "PieceCode",
        //                        "AcceptPieces",
        //                         "WeightCode",
        //                        "AcceptedGrWeight",
        //                        "ReceivedCarrier",
        //                        "Name",
        //                        "UpdatedBy",
        //                        "MessageType"

        //                    };
        //                    object[] sqlParameterValue = new object[] {

        //                        fsadata.airlineprefix,
        //                        fsadata.awbnum,
        //                        fsadata.origin,
        //                        fsadata.dest,

        //                        int.Parse(fsadata.pcscnt==""?"0":fsadata.pcscnt),
        //                        fsadata.weightcode,
        //                        decimal.Parse(fsadata.weight==""?"0":fsadata.weight),

        //                        fsanodes[0].seccarriercode,
        //                        ReceivedDate,
        //                        fsanodes[i].fltorg,

        //                        fsanodes[i].pcsindicator,
        //                        int.Parse(fsanodes[i].numofpcs==""?"0":fsanodes[i].numofpcs),
        //                        fsanodes[i].weightcode,
        //                        decimal.Parse(fsanodes[i].weight==""?"0":fsanodes[i].weight),

        //                        fsanodes[i].carriercode,
        //                        fsanodes[0].name,
        //                        "FSU/RCT",
        //                        "RCT"

        //                    };
        //                    SqlDbType[] sqlParameter = new SqlDbType[]
        //                    {
        //                        SqlDbType.VarChar,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.VarChar,

        //                        SqlDbType.Int,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.Decimal,

        //                        SqlDbType.VarChar,
        //                        SqlDbType.DateTime,
        //                        SqlDbType.VarChar,

        //                        SqlDbType.VarChar,
        //                        SqlDbType.Int,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.Decimal,

        //                       SqlDbType.VarChar,
        //                       SqlDbType.VarChar,
        //                       SqlDbType.VarChar,
        //                       SqlDbType.VarChar
        //                  };
        //                    if (dtb.InsertData("uspSaveAWBThroughFSURCTTFDMessage", sqlParameterName, sqlParameter, sqlParameterValue))
        //                        flag = true;
        //                    else
        //                    {
        //                        flag = false;
        //                        return flag;
        //                    }

        //                }

        //            }
        //        }


        //        #endregion

        //        #region Below Segmnet of FSU/TFD Message
        //        ///Created By :Badiuz khan
        //        ///Created On :2016-05-26
        //        ///Make  Transfer  Freight To  other SPA Airline
        //        if (fsadata.awbnum.Length > 0 && fsanodes.Length > 0 && fsadata.awbnum.Length > 0 && fsanodes[0].messageprefix.ToUpper() == "TFD")
        //        {
        //            if (fsanodes.Length > 0)
        //            {
        //                for (int i = 0; i < fsanodes.Length; i++)
        //                {
        //                    DateTime ReceivedDate = new DateTime();
        //                    string ReceivedTime = string.Empty;
        //                    if (fsanodes[i].fltday != "")
        //                    {
        //                        if (fsanodes[i].fltday.Length > 0 || fsanodes[i].fltmonth.Length > 0)
        //                            ReceivedDate = DateTime.ParseExact(fsanodes[i].fltday, "MM/dd/yyyy", null);
        //                        //ReceivedDate = DateTime.Parse(DateTime.Parse("1." + fsanodes[i].fltmonth + " 2008").Month + "/" + fsanodes[i].fltday + "/" + System.DateTime.Today.Year);
        //                        if (fsanodes[i].flttime.Length > 0)
        //                            ReceivedTime = fsanodes[i].flttime;
        //                        //ReceivedTime = fsanodes[i].flttime.Substring(0, 2) + ":" + fsanodes[i].flttime.Substring(2) + ":00";
        //                    }

        //                    string[] sqlParameterName = new string[]
        //                      {
        //                         "AWBPrefix",
        //                         "AWBNo",
        //                         "Origin",
        //                         "Destination",
        //                         "AWbPcs",
        //                         "AWbWeightCode",
        //                         "AWbGrossWt" ,
        //                         "TransferCarrierCode",
        //                         "ReceivedShipmentDate",
        //                         "ReceivedOrigin",
        //                         "PieceCode",
        //                         "AcceptPieces",
        //                         "WeightCode",
        //                         "AcceptedGrWeight",
        //                         "ReceivedCarrier",
        //                         "Name",
        //                         "UpdatedBy",
        //                         "MessageType",
        //                         "ManifestNumber"

        //                      };
        //                    object[] sqlParameterValue = new object[] {

        //                        fsadata.airlineprefix,
        //                        fsadata.awbnum,
        //                        fsadata.origin,
        //                        fsadata.dest,

        //                        int.Parse(fsadata.pcscnt==""?"0":fsadata.pcscnt),
        //                        fsadata.weightcode,
        //                        decimal.Parse(fsadata.weight==""?"0":fsadata.weight),

        //                        fsanodes[i].carriercode,
        //                        ReceivedDate,
        //                        fsanodes[i].fltorg,

        //                        fsanodes[i].pcsindicator,
        //                        int.Parse(fsanodes[i].numofpcs==""?"0":fsanodes[i].numofpcs),
        //                        fsanodes[i].weightcode,
        //                        decimal.Parse(fsanodes[i].weight==""?"0":fsanodes[i].weight),

        //                        fsanodes[0].seccarriercode,
        //                        fsanodes[0].name,
        //                        "FSU/TFD",
        //                        "TFD",
        //                        int.Parse(fsanodes[0].transfermanifestnumber==""?"0":fsanodes[0].transfermanifestnumber)

        //                    };
        //                    SqlDbType[] sqlParameter = new SqlDbType[]
        //                    {
        //                        SqlDbType.VarChar,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.VarChar,

        //                        SqlDbType.Int,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.Decimal,

        //                        SqlDbType.VarChar,
        //                        SqlDbType.DateTime,
        //                        SqlDbType.VarChar,

        //                        SqlDbType.VarChar,
        //                        SqlDbType.Int,
        //                        SqlDbType.VarChar,
        //                        SqlDbType.Decimal,

        //                       SqlDbType.VarChar,
        //                       SqlDbType.VarChar,
        //                       SqlDbType.VarChar,
        //                       SqlDbType.VarChar,
        //                       SqlDbType.Int
        //                  };
        //                    if (dtb.InsertData("uspSaveAWBThroughFSURCTTFDMessage", sqlParameterName, sqlParameter, sqlParameterValue))
        //                        flag = true;
        //                    else
        //                    {
        //                        flag = false;
        //                        return flag;
        //                    }


        //                }

        //            }
        //        }


        //        #endregion

        //        dtb = new SQLServer();
        //        if (fsadata.awbnum.Length > 0)
        //        {
        //            awbnum = fsadata.awbnum;
        //            awbprefix = fsadata.airlineprefix;
        //            strMsg = strMsg.Replace("\r\n", "$");
        //            strMsg = strMsg.Replace("\n", "$");
        //            string[] splitStr = strMsg.Split('$');
        //            string date = "";

        //            string FlightNo = string.Empty, UOM = string.Empty, UpdatedBy = "FSU", StnCode = string.Empty;
        //            DateTime FlightDate = DateTime.Now, UpdatedON = DateTime.Now;
        //            int PCS = 0;
        //            decimal Wt = 0;
        //            #region UpdateStatus on TblAwbMsg
        //            if (splitStr.Length > 1 && fsanodes.Length > 0)
        //            {
        //                for (int i = 0; i < fsanodes.Length; i++)
        //                {
        //                    if (fsanodes[i].fltday.Length > 0 || fsanodes[i].fltmonth.Length > 0)
        //                    {
        //                        //date = fsanodes[i].fltday + "/" + DateTime.Parse("1." + fsanodes[i].fltmonth + " 2008").Month + "/" + System.DateTime.Today.Year;
        //                        //FlightDate = DateTime.Parse(DateTime.Parse("1." + fsanodes[i].fltmonth + " 2008").Month + "/" + fsanodes[i].fltday + "/" + System.DateTime.Today.Year);
        //                        date = Convert.ToDateTime(fsanodes[i].fltday).ToString("dd/MM/yyyy");
        //                        FlightDate = DateTime.ParseExact(fsanodes[i].fltday, "MM/dd/yyyy", null);
        //                    }
        //                    else
        //                    {
        //                        FlightDate = DateTime.Now;
        //                    }
        //                    //if (fsanodes[i].flttime.Length > 0)
        //                    //{
        //                    //    time = fsanodes[i].flttime + " (UTC)";
        //                    //}

        //                    // time= 
        //                    FlightNo = fsanodes[i].carriercode + fsanodes[i].flightnum;
        //                    UOM = fsanodes[i].weightcode == "" ? "K" : fsanodes[i].weightcode;

        //                    if (fsanodes[i].airportcode != "")
        //                        StnCode = fsanodes[i].airportcode;
        //                    if (fsanodes[i].fltorg != "")
        //                        StnCode = fsanodes[i].fltorg;
        //                    if (fsanodes[i].fltdest != "")
        //                        StnCode = fsanodes[i].fltdest;
        //                    if (fsanodes[i].fltdest != "" && fsanodes[i].fltorg != "")
        //                        StnCode = fsanodes[i].fltorg;






        //                    if (fsanodes[i].numofpcs != "")
        //                        PCS = Convert.ToInt16(fsanodes[i].numofpcs);
        //                    if (fsanodes[i].weight != "")
        //                        Wt = Convert.ToDecimal(fsanodes[i].weight);

        //                    string[] PName = new string[] { "AWBPrefix", "AWBNumber", "MType", "desc", "date", "time", "refno",
        //                        "FlightNo","FlightDate","PCS","WT","UOM","UpdatedBy","UpdatedOn","StnCode"};
        //                    object[] PValues = new object[] { awbprefix, awbnum, fsanodes[i].messageprefix, splitStr[2 + i], date, fsanodes[i].flttime, 0,
        //                    FlightNo,FlightDate,PCS,Wt,UOM,UpdatedBy,UpdatedON,StnCode};
        //                    SqlDbType[] PType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int,
        //                    SqlDbType.VarChar,SqlDbType.DateTime,SqlDbType.Int,SqlDbType.Decimal,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.DateTime,SqlDbType.VarChar};
        //                    if (dtb.InsertData("spInsertAWBMessageStatus", PName, PType, PValues))
        //                        flag = true;
        //                }
        //            }
        //            #endregion

        //            #region Save AWB Record on Audit log Table
        //            if (fsanodes.Length > 0)
        //            {
        //                for (int k = 0; k < fsanodes.Length; k++)
        //                {
        //                    //Updated On:2017-03-24
        //                    //Updated By:Shrishail Ashtage
        //                    //Description:Save AWB Record in AWB OperationAuditlog Table
        //                    string messageStatus = string.Empty, strDescription = string.Empty, strAction = string.Empty;
        //                    switch (fsanodes[k].messageprefix.ToUpper())
        //                    {
        //                        case "BKD":
        //                            messageStatus = "BKD-AWB Booked";
        //                            strDescription = "AWB Booked";
        //                            strAction = "Booked";
        //                            break;
        //                        case "RCS":
        //                            messageStatus = "RCS-AWB HandedOver";
        //                            strDescription = "AWB Accepted.";
        //                            strAction = "Accepted";
        //                            break;
        //                        case "MAN":
        //                            messageStatus = "MAN-AWB Manifested";
        //                            strDescription = "AWB Manifested.";
        //                            strAction = "Manifested";
        //                            break;
        //                        case "DEP":
        //                            messageStatus = "DEP-AWB Departed";
        //                            strDescription = "AWB Departed";
        //                            strAction = "Departed";
        //                            break;
        //                        case "DIS":
        //                            messageStatus = "DIS-AWB Discrepancy";
        //                            switch (fsanodes[k].infocode.ToUpper())
        //                            {
        //                                case "FDAW":
        //                                    strDescription = "Found AWB";
        //                                    strAction = "Found AWB";
        //                                    break;
        //                                case "FDCA":
        //                                    strDescription = "Found Cargo";
        //                                    strAction = "Found Cargo";
        //                                    break;
        //                                case "MSAW":
        //                                    strDescription = "Missing AWB";
        //                                    strAction = "Missing AWB";
        //                                    break;
        //                                case "MSCA":
        //                                    strDescription = "Missing Cargo";
        //                                    strAction = "Missing Cargo";
        //                                    break;
        //                                case "FDAV":
        //                                    strDescription = "Found Mail Document";
        //                                    strAction = "Found Mail Document";
        //                                    break;
        //                                case "FDMB":
        //                                    strDescription = "Found Mailbag";
        //                                    strAction = "Found Mailbag";
        //                                    break;
        //                                case "MSAV":
        //                                    strDescription = "Missing Mail Document";
        //                                    strAction = "Missing Mail Document";
        //                                    break;
        //                                case "MSMB":
        //                                    strDescription = "Missing Mailbag";
        //                                    strAction = "Missing Mailbag";
        //                                    break;
        //                                case "DFLD":
        //                                    strDescription = "Definitely Loaded";
        //                                    strAction = "Definitely Loaded";
        //                                    break;
        //                                case "OFLD":
        //                                    strDescription = "AWB Offloaded";
        //                                    strAction = "Offloaded";
        //                                    break;
        //                                case "OVCD":
        //                                    strDescription = "Overcarried";
        //                                    strAction = "Overcarried";
        //                                    break;
        //                                case "SSPD":
        //                                    strDescription = "Shortshipped";
        //                                    strAction = "Shortshipped";
        //                                    break;
        //                                default:
        //                                    break;
        //                            }
        //                            break;
        //                        case "ARR":
        //                            messageStatus = "ARR-AWB Arrived";
        //                            strDescription = "AWB Arrived";
        //                            strAction = "Arrived";
        //                            break;
        //                        case "RCF":
        //                            messageStatus = "RCF-AWB received";
        //                            strDescription = "AWB received from a given flight";
        //                            strAction = "Arrived";
        //                            break;
        //                        case "DLV":
        //                            messageStatus = "DLV-AWB Delivered";
        //                            strDescription = "AWB Delivered";
        //                            strAction = "Delivered";
        //                            break;
        //                        case "RCT":
        //                            messageStatus = "RCT-Received Freight from Interline Partner(CTM-In generated)";
        //                            strDescription = "Received Freight from Interline Partner";
        //                            //strAction = "Accepted"; 
        //                            strAction = "CTM-In";
        //                            break;
        //                        case "TFD":
        //                            messageStatus = "TFD-Transfer  Freight To  Interline Airline(CTM-Out generated)";
        //                            strDescription = "Transfer  Freight To  Interline Airline";
        //                            strAction = "CTM-Out";
        //                            break;
        //                        case "NFD":
        //                            messageStatus = "Consignment arrived at destination";
        //                            strDescription = "Consignment where consignee or his agent has been informed of its arrival at destination";
        //                            strAction = "Notify to Consignee";
        //                            break;
        //                        case "AWD":
        //                            messageStatus = "Documents delivered to the consignee or agent";
        //                            strDescription = "Consignment arrival documents delivered to the consignee or agent";
        //                            strAction = "Documents delivered";
        //                            break;
        //                        case "PRE":
        //                            messageStatus = "The consignment has been prepared";
        //                            strDescription = "The consignment has been prepared for loading on this flight for transport";// between these locations on this scheduled date";
        //                            strAction = "Consignment Prepared";
        //                            break;
        //                        default:
        //                            messageStatus = "FSU";
        //                            strDescription = "FSU";
        //                            strAction = "FSU";
        //                            break;
        //                    }

        //                    string origin = (fsanodes[k].fltorg).ToString();
        //                    string dest = (fsanodes[k].fltdest).ToString();
        //                    string fltno = (fsanodes[k].carriercode + fsanodes[k].flightnum).ToString();
        //                    string airportcode = (fsanodes[k].airportcode).ToString();
        //                    string messageType = fsanodes[k].messageprefix.ToUpper();
        //                    //DateTime Date = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year));
        //                    DateTime Date = DateTime.ParseExact(fsanodes[k].fltday, "MM/dd/yyyy", null);
        //                    DateTime fltdate = DateTime.Now;

        //                    if (messageType == "DLV" || messageType == "ARR" || messageType == "RCF" || messageType == "MAN")
        //                    {
        //                        airportcode = dest;
        //                        //UpdatedON = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
        //                        UpdatedON = DateTime.Parse((Convert.ToDateTime(fsanodes[k].fltday).ToString("MM/dd/yyyy") + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
        //                        Date = Convert.ToDateTime("1900-01-01");
        //                    }

        //                    if (messageType == "RCS")
        //                    {
        //                        origin = airportcode;
        //                        UpdatedON = DateTime.Parse((Convert.ToDateTime(fsanodes[k].fltday).ToString("MM/dd/yyyy") + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
        //                        Date = Convert.ToDateTime("1900-01-01");
        //                    }

        //                    if (messageType == "TFD" || messageType == "DIS")
        //                    {
        //                        origin = fsanodes[k].airportcode;
        //                        UpdatedON = DateTime.Parse((Convert.ToDateTime(fsanodes[k].fltday).ToString("MM/dd/yyyy") + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
        //                        Date = Convert.ToDateTime("1900-01-01");
        //                    }
        //                    if (messageType == "RCT" || messageType == "NFD" || messageType == "AWD")
        //                    {
        //                        origin = string.Empty;
        //                        dest = fsadata.dest;
        //                        UpdatedON = DateTime.Parse((Convert.ToDateTime(fsanodes[k].fltday).ToString("MM/dd/yyyy") + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
        //                        Date = Convert.ToDateTime("1900-01-01");
        //                    }



        //                    dtb = new SQLServer();
        //                    string[] CNname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public" };
        //                    SqlDbType[] CType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit };
        //                    object[] CValues = new object[] { awbprefix, awbnum, fsadata.origin, fsadata.dest, fsanodes[k].numofpcs, Convert.ToDouble(fsanodes[k].weight == "" ? "0" : fsanodes[k].weight), fltno, Date, origin, dest, strAction, messageStatus, strDescription, "FSU", UpdatedON.ToString(), 1 };

        //                    if (!dtb.ExecuteProcedure("SPAddAWBAuditLog", CNname, CType, CValues))
        //                    {
        //                        clsLog.WriteLog("AWB Audit log  for:" + awbnum + Environment.NewLine + "Error: " + dtb.LastErrorDescription);
        //                    }
        //                }
        //            }
        //            #endregion Save AWB Record on Audit log Table
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        flag = false;
        //        //clsLog.WriteLogAzure(ex);
        //    }
        //    return flag;
        //} 
        #endregion
        public async Task<(bool, MessageData.FSAInfo fsadata, MessageData.CommonStruct[] fsanodes,
             MessageData.customsextrainfo[] customextrainfo, MessageData.ULDinfo[] ulddata, MessageData.otherserviceinfo[] othinfoarray, string ErrorMsg)> SaveandUpdateXFSUMessage(string strMsg, MessageData.FSAInfo fsadata, MessageData.CommonStruct[] fsanodes,
             MessageData.customsextrainfo[] customextrainfo, MessageData.ULDinfo[] ulddata, MessageData.otherserviceinfo[] othinfoarray,
            int refNo, string strMessage, string strMessageFrom, string strFromID, string strStatus, string ErrorMsg)
        {
            //SQLServer dtb = new SQLServer();

            bool flag = false;
            string strFSUBooking = string.Empty;
            //Cls_BL objBL = new Cls_BL();
            bool isAWBPresent = false;
            try
            {
                string awbnum = string.Empty, awbprefix = string.Empty;
                string MessagePrefix = fsanodes.Length > 0 && fsanodes[0].messageprefix.Length > 0 ? "xFSU/" + fsanodes[0].messageprefix.ToUpper() : "xFSU";
                //GenericFunction genericfunction = new GenericFunction();
                //genericfunction.UpdateInboxFromMessageParameter(refNo, fsadata.airlineprefix + "-" + fsadata.awbnum, 
                //    string.Empty, string.Empty, string.Empty, MessagePrefix, "xFSU", DateTime.Parse("1900-01-01"));

                await _genericFunction.UpdateInboxFromMessageParameter(refNo, fsadata.airlineprefix + "-" + fsadata.awbnum,
                    fsanodes[0].flightnum, string.Empty, string.Empty, MessagePrefix, strMessageFrom == "" ? strFromID : strMessageFrom, DateTime.Parse("1900-01-01"));


                #region Check AWB is present or not

                DataSet? dsCheck = new DataSet();
                //dtb = new SQLServer();
                //string[] parametername = new string[] { "AWBNumber", "AWBPrefix", "refNo" };
                //object[] AWBvalues = new object[] { fsadata.awbnum, fsadata.airlineprefix, refNo };

                //SqlDbType[] ptype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int };

                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = fsadata.awbnum },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = fsadata.airlineprefix },
                    new SqlParameter("@refNo", SqlDbType.Int) { Value = refNo }

                };
                //dsCheck = dtb.SelectRecords("sp_getawbdetails", parametername, AWBvalues, ptype);
                dsCheck = await _readWriteDao.SelectRecords("sp_getawbdetails", sqlParameters);
                if (dsCheck != null)
                {
                    if (dsCheck.Tables.Count > 0)
                    {
                        if (dsCheck.Tables[0].Rows.Count > 0)
                        {
                            if (dsCheck.Tables[0].Rows[0]["AWBNumber"].ToString().Equals(fsadata.awbnum, StringComparison.OrdinalIgnoreCase))
                            {
                                isAWBPresent = true;
                            }
                        }
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

                            string[] sqlParameterName = new string[]
                            {
                                 "AWBPrefix",
                                 "AWBNumber",
                                 "AWBOrigin",
                                 "AWBDestination",
                                "AWBPieces",
                                "AWBGrossWeight" ,
                                "FlightNumber",
                                "DiscrepancyDate",
                                "FlightOrigin",
                                "DiscrepancyCode",
                                "DiscrepancyPcsCode",
                                "DiscrepancyPcs",
                                "UOM",
                                "Discrepancyweight",
                                "UpdatedBy",
                                "UpdatedOn",
                                "OtherServiceInformation"
                       };
                            object[] sqlParameterValue = new object[] {
                               fsadata.airlineprefix,
                               fsadata.awbnum,
                                fsadata.origin,
                                fsadata.dest,
                                int.Parse(fsadata.pcscnt==""?"0":fsadata.pcscnt),
                                decimal.Parse(fsadata.weight==""?"0":fsadata.weight),
                                fsanodes[i].flightnum,
                                strDiscrepancyDate,
                                fsanodes[0].fltorg,
                                fsanodes[0].infocode,
                                fsanodes[i].pcsindicator,
                                int.Parse(fsanodes[i].numofpcs==""?"0":fsanodes[i].numofpcs),
                                fsanodes[i].weightcode,
                                decimal.Parse(fsanodes[i].weight == "" ? "0" : fsanodes[i].weight),
                                "FSU",
                                DateTime.Now,
                               ""
                        };
                            SqlDbType[] sqlParameter = new SqlDbType[] {
                                SqlDbType.VarChar,
                                SqlDbType.VarChar,
                                SqlDbType.VarChar,
                                SqlDbType.VarChar,
                                SqlDbType.Int,
                                SqlDbType.Decimal,
                                SqlDbType.VarChar,
                                SqlDbType.DateTime,
                                SqlDbType.VarChar,
                                SqlDbType.VarChar,
                                SqlDbType.VarChar,
                                SqlDbType.Int,
                                SqlDbType.VarChar,
                                SqlDbType.Decimal,
                                SqlDbType.VarChar,
                                SqlDbType.DateTime,
                                SqlDbType.VarChar
                            };
                            SqlParameter[] parameters = new SqlParameter[]
                            {
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar)   { Value = fsadata.airlineprefix },
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar)   { Value = fsadata.awbnum },
                                new SqlParameter("@AWBOrigin", SqlDbType.VarChar)   { Value = fsadata.origin },
                                new SqlParameter("@AWBDestination", SqlDbType.VarChar) { Value = fsadata.dest },
                                new SqlParameter("@AWBPieces", SqlDbType.Int)       { Value = int.Parse(string.IsNullOrEmpty(fsadata.pcscnt) ? "0" : fsadata.pcscnt) },
                                new SqlParameter("@AWBGrossWeight", SqlDbType.Decimal) { Value = decimal.Parse(string.IsNullOrEmpty(fsadata.weight) ? "0" : fsadata.weight) },
                                new SqlParameter("@FlightNumber", SqlDbType.VarChar) { Value = fsanodes[i].flightnum },
                                new SqlParameter("@DiscrepancyDate", SqlDbType.DateTime) { Value = strDiscrepancyDate },
                                new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = fsanodes[0].fltorg },
                                new SqlParameter("@DiscrepancyCode", SqlDbType.VarChar) { Value = fsanodes[0].infocode },
                                new SqlParameter("@DiscrepancyPcsCode", SqlDbType.VarChar) { Value = fsanodes[i].pcsindicator },
                                new SqlParameter("@DiscrepancyPcs", SqlDbType.Int)   { Value = int.Parse(string.IsNullOrEmpty(fsanodes[i].numofpcs) ? "0" : fsanodes[i].numofpcs) },
                                new SqlParameter("@UOM", SqlDbType.VarChar)         { Value = fsanodes[i].weightcode },
                                new SqlParameter("@Discrepancyweight", SqlDbType.Decimal) { Value = decimal.Parse(string.IsNullOrEmpty(fsanodes[i].weight) ? "0" : fsanodes[i].weight) },
                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar)    { Value = "FSU" },
                                new SqlParameter("@UpdatedOn", SqlDbType.DateTime)   { Value = DateTime.Now },
                                new SqlParameter("@OtherServiceInformation", SqlDbType.VarChar ){Value = "" }
                            };
                            //if (dtb.InsertData("SpSaveFSUAWBDiscrepancy", sqlParameterName, sqlParameter, sqlParameterValue))
                            if (await _readWriteDao.ExecuteNonQueryAsync("SpSaveFSUAWBDiscrepancy", parameters))
                                flag = true;
                            else
                            {
                                flag = false;
                                ErrorMsg = "Error while saving Data";
                                //return flag;
                                return (flag, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray, ErrorMsg);
                            }
                        }
                    }
                }

                #endregion

                # region Below Segmnet of FSU/DLV Message
                ///Make  AWB Delivered  Throgh  DLV Message 
                if (fsadata.awbnum.Length > 0 && fsanodes.Length > 0 && fsadata.awbnum.Length > 0 && fsanodes[0].messageprefix.ToUpper() == "DLV")
                {
                    if (!isAWBPresent)
                    {
                        ErrorMsg = "AWB is not present";
                        //return false;
                        return (false, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray, ErrorMsg);
                    }
                    if (fsanodes.Length > 0)
                    {
                        for (int i = 0; i < fsanodes.Length; i++)
                        {
                            DateTime DeliveryDate = DateTime.Now;
                            if (fsanodes[i].fltday != "")
                            {
                                string strdate = (fsanodes[i].fltday);
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
                            //    "updatedon",
                            //    "Inboxsrno"

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
                            //    int.Parse(fsanodes[i].numofpcs==""?fsadata.pcscnt==""?"0":fsadata.pcscnt:fsanodes[i].numofpcs),
                            //    fsanodes[i].weightcode,
                            //    //fsanodes[i].weight,
                            //    //decimal.Parse(fsadata.weight==""?"0":fsadata.weight),
                            //    decimal.Parse(fsanodes[i].weight==""?fsadata.weight==""?"0":fsadata.weight:fsanodes[i].weight),
                            //    fsanodes[0].fltdest== "" ?fsadata.dest:fsanodes[0].fltdest,
                            //    fsanodes[0].name,
                            //    DeliveryDate,
                            //    "XFSU-DLV",
                            //    fsanodes[i].updatedonday!="" ? fsanodes[i].updatedonday + " " + fsanodes[i].updatedontime : fsanodes[i].fltday +" "+ fsanodes[i].flttime,
                            //    refNo,
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
                            //    SqlDbType.VarChar,
                            //    SqlDbType.Int,

                            //};
                            SqlParameter[] parameters = new SqlParameter[]
                            {
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar)   { Value = fsadata.airlineprefix },
                                new SqlParameter("@AWBNo", SqlDbType.VarChar)       { Value = fsadata.awbnum },
                                new SqlParameter("@Origin", SqlDbType.VarChar)      { Value = fsadata.origin },
                                new SqlParameter("@Destination", SqlDbType.VarChar) { Value = fsadata.dest },
                                new SqlParameter("@AWbPcs", SqlDbType.Int)          { Value = int.Parse(string.IsNullOrEmpty(fsadata.pcscnt) ? "0" : fsadata.pcscnt) },
                                new SqlParameter("@AWbGrossWt", SqlDbType.Decimal)  { Value = decimal.Parse(string.IsNullOrEmpty(fsadata.weight) ? "0" : fsadata.weight) },
                                new SqlParameter("@PieceCode", SqlDbType.VarChar)   { Value = fsanodes[i].pcsindicator },
                                new SqlParameter("@Deliverypcs", SqlDbType.Int)     { Value = int.Parse(string.IsNullOrEmpty(fsanodes[i].numofpcs)
                                                                                                ? (string.IsNullOrEmpty(fsadata.pcscnt) ? "0" : fsadata.pcscnt)
                                                                                                : fsanodes[i].numofpcs) },
                                new SqlParameter("@WeightCode", SqlDbType.VarChar)  { Value = fsanodes[i].weightcode },
                                new SqlParameter("@DeliveryGross", SqlDbType.Decimal) { Value = decimal.Parse(string.IsNullOrEmpty(fsanodes[i].weight)
                                                                                                ? (string.IsNullOrEmpty(fsadata.weight) ? "0" : fsadata.weight)
                                                                                                : fsanodes[i].weight) },
                                new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = string.IsNullOrEmpty(fsanodes[0].fltdest) ? fsadata.dest : fsanodes[0].fltdest },
                                new SqlParameter("@Dname", SqlDbType.VarChar)        { Value = fsanodes[0].name },
                                new SqlParameter("@Deliverydate", SqlDbType.DateTime){ Value = DeliveryDate },
                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar)    { Value = "XFSU-DLV" },
                                new SqlParameter("@updatedon", SqlDbType.VarChar)    { Value = !string.IsNullOrEmpty(fsanodes[i].updatedonday)
                                                                                        ? fsanodes[i].updatedonday + " " + fsanodes[i].updatedontime
                                                                                        : fsanodes[i].fltday + " " + fsanodes[i].flttime },
                                new SqlParameter("@Inboxsrno", SqlDbType.Int)        { Value = refNo },
                            };

                            //DataSet dsdata = dtb.SelectRecords("Messaging.uspMakeAWBDeliveryorderofXFSUMessage", sqlParameterName, sqlParameterValue, sqlParameter);
                            DataSet? dsdata = await _readWriteDao.SelectRecords("Messaging.uspMakeAWBDeliveryorderofXFSUMessage", parameters);

                            if (dsdata != null)
                            {
                                if (dsdata.Tables.Count > 0)
                                {
                                    if (dsdata.Tables[0].Rows.Count > 0 && dsdata.Tables[0].Columns.Contains("Errormessage"))
                                    {
                                        flag = false;
                                        ErrorMsg = dsdata.Tables[0].Rows[0]["Errormessage"].ToString();
                                        //return flag;
                                        return (flag, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray, ErrorMsg);

                                    }
                                    else
                                    {
                                        if (dsdata.Tables[0].Rows.Count > 0 && dsdata.Tables[0].Columns.Contains("WarningMessage"))
                                        {
                                            flag = true;
                                            ErrorMsg = "";
                                            //return flag;
                                            return (flag, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray, ErrorMsg);
                                        }
                                        else
                                        {

                                            flag = true;
                                        }

                                    }
                                }
                                else
                                {
                                    flag = false;
                                    ErrorMsg = "Error while saving Data";
                                    //return flag;
                                    return (flag, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray, ErrorMsg);
                                }

                            }
                            else
                            {
                                flag = false;
                                ErrorMsg = "Error while saving Data";
                                //return flag;
                                return (flag, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray, ErrorMsg);
                            }

                            //flag = dtb.InsertData("MakeAWBDeliveryorderofXFSUMessage", sqlParameterName, sqlParameter, sqlParameterValue);
                            //    if (flag)
                            //{
                            //    flag = true;
                            //}

                        }
                    }
                }
                #endregion

                #region  Below Segmnet of RCS and RCT Message
                ///Make  AWB Accepted Throgh  RCS Message 
                bool isAcceptedbyFSURCS = false;
                bool isAcceptedbyFSURCT = false;
                if (!string.IsNullOrEmpty(_genericFunction.ReadValueFromDb("isAcceptedbyFSURCS").Trim()) && Convert.ToBoolean(_genericFunction.ReadValueFromDb("isAcceptedbyFSURCS").Trim()))
                {
                    if (fsadata.awbnum.Length > 0 && fsanodes.Length > 0 && fsadata.awbnum.Length > 0 && fsanodes[0].messageprefix.ToUpper().Trim() == "RCS")
                    {
                        isAcceptedbyFSURCS = true;
                    }
                }
                if (!string.IsNullOrEmpty(_genericFunction.ReadValueFromDb("isAcceptedbyFSURCT").Trim()) && Convert.ToBoolean(_genericFunction.ReadValueFromDb("isAcceptedbyFSURCT").Trim()))
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
                            ErrorMsg = "AWB is not present";
                            //return false;
                            return (false, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray, ErrorMsg);
                        }

                        for (int i = 0; i < fsanodes.Length; i++)
                        {
                            #region : Check AWB is accepted or not :
                            //FFRMessageProcessor ffrMessageProcessor = new FFRMessageProcessor();
                            DataSet? dsAWBStatus = new DataSet();
                            dsAWBStatus = await _ffrMessageProcessor.CheckValidateXFFRMessage(fsadata.airlineprefix, fsadata.awbnum, fsadata.origin, fsadata.dest, "FSU/RCS", "", "");
                            for (int k = 0; k < dsAWBStatus.Tables.Count; k++)
                            {
                                if (dsAWBStatus.Tables[k].Columns.Contains("MessageName") && dsAWBStatus.Tables[k].Columns.Contains("AWBSttus"))
                                {
                                    if (dsAWBStatus.Tables[k].Rows[0]["AWBSttus"].ToString().ToUpper() == "ACCEPTED")
                                    {
                                        //GenericFunction genericFunction = new GenericFunction();
                                        await _genericFunction.UpdateErrorMessageToInbox(refNo, "AWB is already accepted");
                                        ErrorMsg = "AWB is already accepted";
                                        //return true;
                                        return (true, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray, ErrorMsg);
                                    }
                                }
                            }
                            #endregion Check AWB is accepted or not

                            string AcceptedDate = System.DateTime.Now.ToString("yyy/MM/dd");
                            string AcceptedTime = string.Empty;
                            if (fsanodes[i].fltday != "")
                            {
                                if (fsanodes[i].fltday.Length > 0)

                                    //AcceptedDate = DateTime.Parse(DateTime.Parse("1." + fsanodes[i].fltmonth + " 2008").Month + "/" 
                                    //    + fsanodes[i].fltday + "/" + System.DateTime.Today.Year);
                                    //AcceptedDate = System.DateTime.Now.Year.ToString() + "/" + fltMonth.PadLeft(2, '0') 
                                    //    + "/" + objRouteInfo[lstIndex].date.ToString().Substring(3, 2);
                                    AcceptedDate = fsanodes[i].fltday;

                                if (fsanodes[i].flttime.Length > 0)
                                    //AcceptedTime = fsanodes[i].flttime.Substring(0, 2) + ":" + fsanodes[i].flttime.Substring(2) + ":00";
                                    AcceptedTime = fsanodes[i].flttime;

                            }
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
                            SqlParameter[] sqlParameters1 = new SqlParameter[] {
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar)   { Value = fsadata.airlineprefix },
                                new SqlParameter("@AWBNo", SqlDbType.VarChar)       { Value = fsadata.awbnum },
                                new SqlParameter("@Origin", SqlDbType.VarChar)      { Value = fsadata.origin },
                                new SqlParameter("@Destination", SqlDbType.VarChar) { Value = fsadata.dest },
                                new SqlParameter("@AWbPcs", SqlDbType.Int)          { Value = int.Parse(string.IsNullOrEmpty(fsadata.pcscnt) ? "0" : fsadata.pcscnt) },
                                new SqlParameter("@AWbGrossWt", SqlDbType.Decimal)  { Value = decimal.Parse(string.IsNullOrEmpty(fsadata.weight) ? "0" : fsadata.weight) },
                                new SqlParameter("@PieceCode", SqlDbType.VarChar)   { Value = fsanodes[i].pcsindicator },
                                new SqlParameter("@AcceptPieces", SqlDbType.Int)     { Value = int.Parse(string.IsNullOrEmpty(fsanodes[i].numofpcs) ? "0" : fsanodes[i].numofpcs) },
                                new SqlParameter("@WeightCode", SqlDbType.VarChar)  { Value = fsanodes[i].weightcode },
                                new SqlParameter("@AcceptedGrWeight", SqlDbType.Decimal) { Value = decimal.Parse(string.IsNullOrEmpty(fsanodes[i].weight) ? "0" : fsanodes[i].weight) },
                                new SqlParameter("@AccpetedOrigin", SqlDbType.VarChar) { Value = fsanodes[0].airportcode },
                                new SqlParameter("@ShipperName", SqlDbType.VarChar)  { Value = fsanodes[0].name },
                                new SqlParameter("@VolumeCode", SqlDbType.VarChar)   { Value = fsanodes[0].volumecode },
                                new SqlParameter("@VolumeAmount", SqlDbType.Decimal) { Value = decimal.Parse(string.IsNullOrEmpty(fsanodes[0].volumeamt) ? "0" : fsanodes[0].volumeamt) },
                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar)    { Value = MessagePrefix },
                                new SqlParameter("@AcceptedDate", SqlDbType.DateTime){ Value = AcceptedDate },
                                new SqlParameter("@AcceptedTime", SqlDbType.VarChar) { Value = AcceptedTime },
                                new SqlParameter("@refNo", SqlDbType.Int)            { Value = refNo }



                            };
                            //flag = dtb.ExecuteProcedure("MakeAWBAcceptenceThroughFSUMessage", sqlParameterName, sqlParameter, sqlParameterValue);
                            flag = await _readWriteDao.ExecuteNonQueryAsync("MakeAWBAcceptenceThroughFSUMessage", sqlParameters1);
                            if (flag)
                            {
                                flag = true;
                                #region Capacity Update
                                string[] cparam = { "AWBPrefix", "AWBNumber" };
                                SqlDbType[] cparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                                object[] cparamvalues = { fsadata.airlineprefix, fsadata.awbnum };
                                SqlParameter[] parameters = new SqlParameter[] {
                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = fsadata.airlineprefix },
                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = fsadata.awbnum }
                                };
                                //if (!dtb.InsertData("UpdateCapacitythroughMessage", cparam, cparamtypes, cparamvalues))
                                if (!await _readWriteDao.ExecuteNonQueryAsync("UpdateCapacitythroughMessage", parameters))
                                    //clsLog.WriteLogAzure("Error  on Update capacity Plan : {0}" , awbnum);
                                    _logger.LogWarning("Error on Update capacity Plan for AWB: {0}", fsadata.awbnum);

                                #endregion
                            }
                            else
                            {
                                flag = false;
                                ErrorMsg = "Error while saving Data";
                                //return flag;
                                return (flag, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray, ErrorMsg);
                            }
                            #region PRI message on FSU/FSR Access
                            if (flag == true)
                            {


                                //GenericFunction GF = new GenericFunction();
                                //CustomsImportBAL objCustoms = new CustomsImportBAL();
                                #region PRI Message

                                //dsCheck = dtb.SelectRecords("sp_getawbdetails", parametername, AWBvalues, ptype);
                                dsCheck = await _readWriteDao.SelectRecords("sp_getawbdetails", sqlParameters);
                                if (dsCheck != null && dsCheck.Tables.Count > 10 && dsCheck.Tables[10].Rows.Count > 0)
                                {
                                    try
                                    {
                                        //lblStatus.Text = string.Empty;
                                        //CustomsImportBAL objCustoms = new CustomsImportBAL();
                                        //for (int i = 0; i < grdRouting.Rows.Count; i++)
                                        //{
                                        string AWBPrefix = string.Empty;
                                        string AWBno = string.Empty, FlightNo = string.Empty;
                                        DateTime FlightDate = System.DateTime.Now;
                                        if (fsadata.awbnum.Trim() != "")
                                        {
                                            AWBPrefix = fsadata.airlineprefix;
                                            AWBno = fsadata.awbnum;


                                            if (dsCheck != null && dsCheck.Tables.Count > 11 && dsCheck.Tables[11].Rows.Count > 0)
                                            {
                                                FlightNo = dsCheck.Tables[11].Rows[0]["fltnumber"].ToString();
                                                FlightDate = Convert.ToDateTime(dsCheck.Tables[11].Rows[0]["FltDate"].ToString());
                                            }


                                            try
                                            {
                                                object[] QueryValues = new object[4];

                                                QueryValues[0] = AWBPrefix + "-" + AWBno;
                                                QueryValues[1] = FlightNo;
                                                QueryValues[2] = FlightDate;
                                                QueryValues[3] = string.Empty;

                                                DataSet dCust = new DataSet("Exp_Manifest_btnSendPRI_Auto_dCust");
                                                //dCust = objCustoms.CheckCustomsAWBAvailability(QueryValues);
                                                if (dsCheck != null && dsCheck.Tables.Count > 11 && dsCheck.Tables[11].Rows.Count > 0)
                                                {
                                                    foreach (DataRow drow in dsCheck.Tables[12].Rows)
                                                    {
                                                        QueryValues[3] = drow["HAWBNumber"];

                                                        // if (dCust.Tables[14].Rows[0]["Validate"].ToString() == "True")
                                                        // {
                                                        CustomsImportBAL.PRI sbPRI = await _objCustoms.EncodingPRIMessage(QueryValues);

                                                        object[] QueryVal = new object[101];
                                                        _objCustoms.readQueryValuesPRI(sbPRI, ref QueryVal, sbPRI.ToString().ToUpper(), FlightNo, FlightDate, "FSU/RCS", FlightDate);

                                                        if (await _objCustoms.UpdateCustomsMessages(QueryVal, sbPRI.StandardMessageIdentifier.StandardMessageIdentifier))
                                                        {
                                                            if (sbPRI != null)
                                                            {
                                                                if (sbPRI.ToString() != "")
                                                                {
                                                                    #region Wrap SITA Address
                                                                    try
                                                                    {
                                                                        string AirlineCode = FlightNo.Substring(0, 2);
                                                                        //DataSet dsChecmMessageType = gf.GetRecordofSitaAddressandSitaMessageVersionandMessageType(AirlineCode, string.Empty, string.Empty, string.Empty, "PRI");
                                                                        DataSet dsMsgCongig = await _genericFunction.GetSitaAddressandMessageVersion("DY", "PRI", "AIR", string.Empty, string.Empty, string.Empty, string.Empty);
                                                                        if (dsMsgCongig != null && dsMsgCongig.Tables[0].Rows.Count > 0)
                                                                        {

                                                                            string strSITAHeaderType = dsMsgCongig.Tables[0].Rows[0]["SITAHeaderType"].ToString();
                                                                            string MessageHeader = _genericFunction.MakeMailMessageFormat(dsMsgCongig.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsMsgCongig.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsMsgCongig.Tables[0].Rows[0]["MessageID"].ToString(), strSITAHeaderType);

                                                                            await _genericFunction.SaveMessageOutBox(sbPRI.StandardMessageIdentifier.StandardMessageIdentifier, MessageHeader + "\r\n" + sbPRI.ToString().ToUpper(), "SITAFTP", "SITAFTP", "", "", "", "", "");
                                                                        }
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        //clsLog.WriteLogAzure(ex);
                                                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                                    }
                                                                    #endregion
                                                                }
                                                            }

                                                        }
                                                        // }

                                                    }
                                                }
                                                dCust = null;
                                            }
                                            catch (Exception ex)
                                            {
                                                //clsLog.WriteLogAzure(ex);
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                        }
                                        else
                                        {
                                            AWBPrefix = "";
                                            AWBno = "";
                                        }
                                        // }
                                    }
                                    catch (Exception ex)
                                    {
                                        //clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }

                                }
                                #endregion

                            }
                            #endregion
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
                        ErrorMsg = "AWB is not present";
                        //return false;
                        return (false, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray, ErrorMsg);
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
                            SqlParameter[] parameters = new SqlParameter[]
                            {
                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar)       { Value = fsadata.airlineprefix },
                            new SqlParameter("@AWBNo", SqlDbType.VarChar)           { Value = fsadata.awbnum },
                            new SqlParameter("@Origin", SqlDbType.VarChar)          { Value = fsadata.origin },
                            new SqlParameter("@Destination", SqlDbType.VarChar)     { Value = fsadata.dest },

                            new SqlParameter("@AWbPcs", SqlDbType.Int)              { Value = int.Parse(string.IsNullOrEmpty(fsadata.pcscnt) ? "0" : fsadata.pcscnt) },
                            new SqlParameter("@AWbWeightCode", SqlDbType.VarChar)   { Value = fsadata.weightcode },
                            new SqlParameter("@AWbGrossWt", SqlDbType.Decimal)      { Value = decimal.Parse(string.IsNullOrEmpty(fsadata.weight) ? "0" : fsadata.weight) },

                            new SqlParameter("@TransferCarrierCode", SqlDbType.VarChar) { Value = fsanodes[i].carriercode },
                            new SqlParameter("@ReceivedShipmentDate", SqlDbType.DateTime) { Value = ReceivedDate },
                            new SqlParameter("@ReceivedOrigin", SqlDbType.VarChar)   { Value = string.IsNullOrEmpty(fsanodes[i].fltorg?.Trim()) ? fsanodes[i].airportcode.Trim() : fsanodes[i].fltorg.Trim() },

                            new SqlParameter("@PieceCode", SqlDbType.VarChar)        { Value = fsanodes[i].pcsindicator },
                            new SqlParameter("@AcceptPieces", SqlDbType.Int)         { Value = int.Parse(string.IsNullOrEmpty(fsanodes[i].numofpcs) ? "0" : fsanodes[i].numofpcs) },
                            new SqlParameter("@WeightCode", SqlDbType.VarChar)       { Value = fsanodes[i].weightcode },
                            new SqlParameter("@AcceptedGrWeight", SqlDbType.Decimal) { Value = decimal.Parse(string.IsNullOrEmpty(fsanodes[i].weight) ? "0" : fsanodes[i].weight) },

                            new SqlParameter("@ReceivedCarrier", SqlDbType.VarChar)  { Value = fsanodes[0].seccarriercode },
                            new SqlParameter("@Name", SqlDbType.VarChar)             { Value = fsanodes[0].name },
                            new SqlParameter("@UpdatedBy", SqlDbType.VarChar)        { Value = strUpdatedby },
                            new SqlParameter("@MessageType", SqlDbType.VarChar)      { Value = strMessageType },
                            new SqlParameter("@ManifestNumber", SqlDbType.Int)       { Value = int.Parse(string.IsNullOrEmpty(fsanodes[0].transfermanifestnumber) ? "0" : fsanodes[0].transfermanifestnumber) },
                            };

                            //if (dtb.InsertData("uspSaveAWBThroughFSURCTTFDMessage", sqlParameterName, sqlParameter, sqlParameterValue))
                            if (await _readWriteDao.ExecuteNonQueryAsync("uspSaveAWBThroughFSURCTTFDMessage", parameters))
                            {
                                flag = true;
                            }
                            else
                            {
                                flag = false;
                                ErrorMsg = "Error while saving Data";
                                //return flag;
                                return (flag, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray, ErrorMsg);
                            }


                        }

                    }
                }


                #endregion

                #region  Below Segmnet of RCF Message
                if (fsadata.awbnum.Length > 0 && fsanodes.Length > 0 && fsanodes[0].messageprefix.ToUpper() == "RCF")
                {
                    for (int i = 0; i < fsanodes.Length; i++)
                    {
                        //string origin = string.Empty, dest = string.Empty, fltno = string.Empty, airportcode = string.Empty;
                        //DateTime fltdate = DateTime.Now;
                        //string[] PNames = new string[] { "AWBPrefix", "AWBNumber", "Airportcode", "messageType" };
                        //SqlDbType[] PTypes = new System.Data.SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                        //object[] Pvalues = new object[] { fsadata.airlineprefix, fsadata.awbnum, fsanodes[i].fltdest, "RCF" };

                        //DataSet dsr = dtb.SelectRecords("SelectFltNOORGDest", PNames, Pvalues, PTypes);
                        //if (dsr != null && dsr.Tables[0].Rows.Count > 0)
                        //{

                        //    if (dsr.Tables[0].Rows[0]["FltNumber"].ToString() == fltno)
                        //    {
                        //        origin = dsr.Tables[0].Rows[0]["FltOrigin"].ToString();
                        //        dest = dsr.Tables[0].Rows[0]["FltDest"].ToString();
                        //        fltno = dsr.Tables[0].Rows[0]["FltNumber"].ToString();
                        //        fltdate = Convert.ToDateTime(dsr.Tables[0].Rows[0]["FltDate"].ToString());
                        //    }
                        //}

                        //DateTime fltdate = DateTime.Now;
                        ////fltdate = DateTime.Parse((fsanodes[i].fltmonth + "-" + fsanodes[i].fltday + "-" + DateTime.Now.Year));
                        //fltdate = DateTime.Parse((fsanodes[i].fltday + " " + Convert.ToString(fsanodes[i].flttime.Contains("(UTC)") == true ? fsanodes[i].flttime.Substring(0, 8) : fsanodes[i].flttime)));
                        if (fsanodes[i].messageprefix.ToUpper().Trim() == "RCF")
                        {
                            // string[] ParaNames = new string[] { "AWBPrefix", "AWBNumber", "Destination", "ConsignmentType", "Pieces", "Weight", "FlighNumber", "ArrivedDate", "WTCode", "RefNo", "FltOrigin", "FltDestination", "FlightDate" };
                            // SqlDbType[] ParaTypes = new System.Data.SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime };
                            // object[] ParaValues = new object[] { fsadata.airlineprefix, fsadata.awbnum, string.Empty, fsanodes[i].pcsindicator, fsanodes[i].numofpcs, (((fsanodes[i].weight).Length == 0) ? "0" : (fsanodes[i].weight)), fsanodes[i].flightnum, DateTime.Now, fsanodes[i].weightcode, refNo, fsanodes[i].fltorg, fsanodes[i].fltdest, fsanodes[i].fltday };
                            SqlParameter[] parameters = new SqlParameter[] {
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar)    { Value = fsadata.airlineprefix },
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar)    { Value = fsadata.awbnum },
                                new SqlParameter("@Destination", SqlDbType.VarChar)   { Value = string.Empty },
                                new SqlParameter("@ConsignmentType", SqlDbType.VarChar){ Value = fsanodes[i].pcsindicator },
                                new SqlParameter("@Pieces", SqlDbType.Int)           { Value = fsanodes[i].numofpcs },
                                new SqlParameter("@Weight", SqlDbType.VarChar)       { Value = ((fsanodes[i].weight).Length == 0) ? "0" : (fsanodes[i].weight) },
                                new SqlParameter("@FlighNumber", SqlDbType.VarChar)  { Value = fsanodes[i].flightnum },
                                new SqlParameter("@ArrivedDate", SqlDbType.DateTime) { Value = DateTime.Now },
                                new SqlParameter("@WTCode", SqlDbType.VarChar)       { Value = fsanodes[i].weightcode },
                                new SqlParameter("@RefNo", SqlDbType.Int)            { Value = refNo },
                                new SqlParameter("@FltOrigin", SqlDbType.VarChar)    { Value = fsanodes[i].fltorg },
                                new SqlParameter("@FltDestination", SqlDbType.VarChar){ Value = fsanodes[i].fltdest },
                                new SqlParameter("@FlightDate", SqlDbType.DateTime)  { Value = fsanodes[i].fltday }
                            };

                            //if (!dtb.ExecuteProcedure("USPUpdateRCForARRRecord", ParaNames, ParaTypes, ParaValues))
                            //{
                            //    clsLog.WriteLogAzure("Error on RCF -Arrived Falied :" + awbnum);
                            //}
                            DataSet? dsRCFARR = new DataSet();
                            //dsRCFARR = dtb.SelectRecords("USPUpdateRCForARRRecord", ParaNames, ParaValues, ParaTypes);
                            //dsRCFARR = await _readWriteDao.SelectRecords("USPUpdateRCForARRRecord", ParaNames, ParaValues, ParaTypes);
                            dsRCFARR = await _readWriteDao.SelectRecords("USPUpdateRCForARRRecord", parameters);
                            if (dsRCFARR != null && dsRCFARR.Tables.Count > 0)
                            {
                                for (int j = 0; j < dsRCFARR.Tables.Count; j++)
                                {
                                    if (dsRCFARR.Tables[j].Columns.Contains("IsFlightDeparted") && dsRCFARR.Tables[j].Rows[0]["IsFlightDeparted"].ToString() == "False")
                                    {
                                        ErrorMsg = "";
                                        //return true;
                                        return (true, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray, ErrorMsg);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion Below Segmnet of RCF Message

                #region Below Segmnet of FSU/ARR Message
                ///Make  AWB Delivered  Throgh  DLV Message 
                //if (fsadata.awbnum.Length > 0 && fsanodes.Length > 0 && fsadata.awbnum.Length > 0 && fsanodes[0].messageprefix.ToUpper() == "ARR")
                //{
                //    if (!isAWBPresent)
                //    {
                //        ErrorMsg = "AWB is not present";
                //        return false;
                //    }
                //    if (fsanodes.Length > 0)
                //    {
                //        for (int i = 0; i < fsanodes.Length; i++)
                //        {
                //            DateTime ArrivalDate = DateTime.Now;
                //            if (fsanodes[i].arrivaltime != "")
                //            {
                //                string strdate = (fsanodes[i].arrivaltime);
                //                ArrivalDate = DateTime.Parse(strdate);
                //            }
                //            string[] sqlParameterName = new string[]
                //            {
                //                "AWBPrefix",
                //                "AWBNo",
                //                "Origin",
                //                "Destination",
                //                "AWbPcs",
                //                "AWbGrossWt" ,
                //                "PieceCode",
                //                "Arrivalpcs",
                //                "WeightCode",
                //                "ArrivalGross",
                //                "FlightDestination ",
                //                "Dname",
                //                "ArrievalDate",
                //                "UpdatedBy",
                //                "updatedon",
                //                "Inboxsrno"

                //            };
                //            object[] sqlParameterValue = new object[]
                //            {
                //                fsadata.airlineprefix,
                //                fsadata.awbnum,
                //                fsadata.origin,
                //                fsadata.dest,
                //                int.Parse(fsadata.pcscnt==""?"0":fsadata.pcscnt),
                //                decimal.Parse(fsadata.weight==""?"0":fsadata.weight),
                //                fsanodes[i].pcsindicator,
                //                int.Parse(fsanodes[i].numofpcs==""?fsadata.pcscnt==""?"0":fsadata.pcscnt:fsanodes[i].numofpcs),
                //                fsanodes[i].weightcode,
                //                //fsanodes[i].weight,
                //                //decimal.Parse(fsadata.weight==""?"0":fsadata.weight),
                //                decimal.Parse(fsanodes[i].weight==""?fsadata.weight==""?"0":fsadata.weight:fsanodes[i].weight),
                //                fsanodes[0].fltdest== "" ?fsadata.dest:fsanodes[0].fltdest,
                //                fsanodes[0].name,
                //                ArrivalDate,
                //                "XFSU",
                //          fsanodes[i].fltday != ""? fsanodes[i].fltday +" "+ fsanodes[i].flttime: DateTime.Now.ToString("yyyy-MM-dd"),
                //              refNo,




                //        };
                //            SqlDbType[] sqlParameter = new SqlDbType[] {
                //                SqlDbType.VarChar,
                //                SqlDbType.VarChar,
                //                SqlDbType.VarChar,
                //                SqlDbType.VarChar,
                //                SqlDbType.Int,
                //                SqlDbType.Decimal,
                //                SqlDbType.VarChar,
                //                SqlDbType.Int,
                //                SqlDbType.VarChar,
                //                SqlDbType.Decimal,
                //                SqlDbType.VarChar,
                //                SqlDbType.VarChar,
                //                SqlDbType.DateTime,
                //                SqlDbType.VarChar,
                //                SqlDbType.VarChar,
                //                SqlDbType.Int,

                //            };

                //            DataSet dsdata = dtb.SelectRecords("MakeAWBArrivalofXFSUMessage", sqlParameterName, sqlParameterValue, sqlParameter);

                //            if (dsdata != null)
                //            {
                //                if (dsdata.Tables.Count > 0)
                //                {
                //                    if (dsdata.Tables[0].Rows.Count > 0 && dsdata.Tables[0].Columns.Contains("Errormessage"))
                //                    {
                //                        flag = false;
                //                        ErrorMsg = dsdata.Tables[0].Rows[0]["Errormessage"].ToString();
                //                        return flag;
                //                    }
                //                    else
                //                    {
                //                        if (dsdata.Tables[0].Rows.Count > 0 && dsdata.Tables[0].Columns.Contains("WarningMessage"))
                //                        {
                //                            flag = true;
                //                            ErrorMsg = "";
                //                            return flag;
                //                        }
                //                        else
                //                        {

                //                            flag = true;
                //                        }

                //                    }
                //                }
                //                else
                //                {
                //                    flag = false;
                //                    ErrorMsg = "Error while saving Data";
                //                    return flag;
                //                }

                //            }
                //            else
                //            {
                //                flag = false;
                //                ErrorMsg = "Error while saving Data";
                //                return flag;
                //            }

                //            //flag = dtb.InsertData("MakeAWBDeliveryorderofXFSUMessage", sqlParameterName, sqlParameter, sqlParameterValue);
                //            //    if (flag)
                //            //{
                //            //    flag = true;
                //            //}

                //        }
                //    }
                //}
                #endregion




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
                    #region UpdateStatus on TblAwbMsg
                    if (splitStr.Length > 1 && fsanodes.Length > 0)
                    {
                        for (int i = 0; i < fsanodes.Length; i++)
                        {
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
                            FlightNo = fsanodes[i].flightnum;//fsanodes[i].carriercode +
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
                            SqlParameter[] parameters = new SqlParameter[] {
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar)    { Value = awbprefix },
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar)    { Value = awbnum },
                                new SqlParameter("@MType", SqlDbType.VarChar)        { Value = fsanodes[i].messageprefix },
                                new SqlParameter("@desc", SqlDbType.VarChar)         { Value = splitStr[2 + i] },
                                new SqlParameter("@date", SqlDbType.VarChar)         { Value = date },
                                new SqlParameter("@time", SqlDbType.VarChar)         { Value = fsanodes[i].flttime },
                                new SqlParameter("@refno", SqlDbType.Int)            { Value = 0 },
                                new SqlParameter("@FlightNo", SqlDbType.VarChar)     { Value = FlightNo },
                                new SqlParameter("@FlightDate", SqlDbType.DateTime)  { Value = FlightDate },
                                new SqlParameter("@PCS", SqlDbType.Int)              { Value = PCS },
                                new SqlParameter("@WT", SqlDbType.Decimal)           { Value = Wt },
                                new SqlParameter("@UOM", SqlDbType.VarChar)          { Value = UOM },
                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar)    { Value = UpdatedBy },
                                new SqlParameter("@UpdatedOn", SqlDbType.DateTime)   { Value = UpdatedON },
                                new SqlParameter("@StnCode", SqlDbType.VarChar)      { Value = StnCode }
                            };

                            //if (dtb.InsertData("spInsertAWBMessageStatus", PName, PType, PValues))
                            if (await _readWriteDao.ExecuteNonQueryAsync("spInsertAWBMessageStatus", parameters))
                                flag = true;
                        }
                    }
                    #endregion

                    #region Save AWB Record on Audit log Table
                    if (fsanodes.Length > 0)
                    {
                        for (int k = 0; k < fsanodes.Length; k++)
                        {
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
                                    Updatedby = "xFSU-BKD";
                                    break;
                                case "RCS":
                                    messageStatus = "RCS-AWB HandedOver";
                                    strDescription = "AWB Accepted.";
                                    strAction = "Accepted";
                                    Updatedby = "xFSU-RCS";
                                    break;
                                case "MAN":
                                    messageStatus = "MAN-AWB Manifested";
                                    strDescription = "AWB Manifested.";
                                    strAction = "Manifested";
                                    Updatedby = "xFSU-MAN";
                                    break;
                                case "DEP":
                                    messageStatus = "DEP-AWB Departed";
                                    strDescription = "AWB Departed";
                                    strAction = "Departed";
                                    Updatedby = "xFSU-DEP";
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
                                        default:
                                            break;
                                    }
                                    break;
                                case "ARR":
                                    messageStatus = "ARR-AWB Arrived";
                                    strDescription = "Flight Arrived at " + (fsanodes[k].fltday != "" ? fsanodes[k].fltday : DateTime.Now.ToString("yyyy-MM-dd")) + " " + (fsanodes[k].flttime != "" ? fsanodes[k].flttime : DateTime.Now.ToString("HH:mm:ss"));


                                    strAction = "Arrived";
                                    Updatedby = "xFSU-ARR";
                                    break;
                                case "RCF":
                                    messageStatus = "RCF-AWB received";
                                    strDescription = "AWB received from a given flight";
                                    strAction = "Arrived";
                                    Updatedby = "xFSU-RCF";
                                    break;
                                case "DLV":
                                    messageStatus = "DLV-AWB Delivered";
                                    strDescription = "AWB Delivered";
                                    strAction = "Delivered";
                                    Updatedby = "xFSU-DLV";
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
                                    Updatedby = "xFSU-NFD";
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
                                case "FOH":
                                    messageStatus = "Ready for carriage";
                                    strDescription = "Ready for carriage";
                                    strAction = "Ready for carriage";
                                    Updatedby = "xFSU-FOH";
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
                            string fltno = (fsanodes[k].flightnum).ToString();
                            string airportcode = (fsanodes[k].airportcode).ToString();
                            string messageType = fsanodes[k].messageprefix.ToUpper();
                            DateTime Date = DateTime.Now;
                            if (fsanodes[k].fltday != "")
                            {
                                Date = DateTime.Parse((fsanodes[k].fltday));
                            }



                            DateTime fltdate = DateTime.Now;

                            switch (messageType.Trim())
                            {
                                case "DLV":
                                    airportcode = dest;
                                    //UpdatedON = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                    try
                                    {
                                        UpdatedON = DateTime.Parse((fsanodes[k].updatedonday + " " + Convert.ToString(fsanodes[k].updatedontime.Contains("(UTC)") == true ? fsanodes[k].updatedontime.Substring(0, 8) : fsanodes[k].updatedontime)));
                                    }
                                    catch (Exception ex)
                                    {
                                        UpdatedON = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                        // clsLog.WriteLogAzure("DLV date exception:- " + ex);
                                        _logger.LogError("DLV date exception:- {0}", ex);
                                    }
                                    Date = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime))); ;
                                    origin = Convert.ToString(fsanodes[k].fltorg) == "" ? fsadata.origin : Convert.ToString(fsanodes[k].fltorg);
                                    dest = Convert.ToString(fsanodes[k].fltdest) == "" ? fsadata.dest : Convert.ToString(fsanodes[k].fltdest);
                                    break;

                                case "ARR":
                                    airportcode = dest;

                                    UpdatedON = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime))); ;

                                    Date = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime))); ;
                                    //  origin = origin = origin.Length < 1 ? airportcode : origin;
                                    origin = Convert.ToString(fsanodes[k].fltorg) == "" ? fsadata.origin : Convert.ToString(fsanodes[k].fltorg);
                                    dest = Convert.ToString(fsanodes[k].fltdest) == "" ? fsadata.dest : Convert.ToString(fsanodes[k].fltdest);
                                    break;

                                case "RCF":
                                case "MAN":
                                    ////  origin = origin.Length < 1 ? airportcode : origin;
                                    origin = Convert.ToString(fsanodes[k].fltorg) == "" ? fsadata.origin : Convert.ToString(fsanodes[k].fltorg);
                                    dest = Convert.ToString(fsanodes[k].fltdest) == "" ? fsadata.dest : Convert.ToString(fsanodes[k].fltdest);
                                    airportcode = dest;
                                    //UpdatedON = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                    UpdatedON = DateTime.Parse((fsanodes[k].updatedonday + " " + Convert.ToString(fsanodes[k].updatedontime.Contains("(UTC)") == true ? fsanodes[k].updatedontime.Substring(0, 8) : fsanodes[k].updatedontime)));

                                    //Date = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year));//Convert.ToDateTime("1900-01-01");
                                    Date = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                    break;
                                case "RCS":
                                    //  origin = airportcode;
                                    origin = Convert.ToString(fsanodes[k].fltorg) == "" ? fsadata.origin : Convert.ToString(fsanodes[k].fltorg);
                                    dest = Convert.ToString(fsanodes[k].fltdest) == "" ? fsadata.dest : Convert.ToString(fsanodes[k].fltdest);
                                    //UpdatedON = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));                                    

                                    UpdatedON = DateTime.Parse((fsanodes[k].updatedonday + " " + Convert.ToString(fsanodes[k].updatedontime.Contains("(UTC)") == true ? fsanodes[k].updatedontime.Substring(0, 8) : fsanodes[k].updatedontime)));

                                    if (fsanodes[k].fltday != "")
                                    {
                                        Date = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                    }
                                    else
                                    {
                                        Date = DateTime.UtcNow;
                                    }

                                    break;
                                case "TFD":
                                case "DIS":
                                    origin = fsanodes[k].airportcode;
                                    UpdatedON = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                    Date = Convert.ToDateTime("1900-01-01");
                                    break;
                                case "RCT":
                                case "NFD":
                                case "AWD":
                                case "CCD":
                                    //origin = string.Empty;
                                    origin = Convert.ToString(fsanodes[k].fltorg) == "" ? fsadata.origin : Convert.ToString(fsanodes[k].fltorg);
                                    //dest = fsadata.dest;
                                    dest = Convert.ToString(fsanodes[k].fltdest) == "" ? fsadata.dest : Convert.ToString(fsanodes[k].fltdest);
                                    //UpdatedON = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                    //Date = Convert.ToDateTime("1900-01-01");

                                    if (fsanodes[k].fltday != "")
                                    {
                                        UpdatedON = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));

                                        Date = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                    }
                                    else
                                        Date = DateTime.Parse("1900-01-01");

                                    break;
                                case "DEP":
                                    airportcode = Convert.ToString(fsanodes[k].airportcode);
                                    origin = Convert.ToString(fsanodes[k].fltorg) == "" ? fsadata.origin : Convert.ToString(fsanodes[k].fltorg);
                                    dest = fsanodes[k].fltdest.ToString() == "" ? fsadata.dest : fsanodes[k].fltdest.ToString();

                                    //UpdatedON = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                    UpdatedON = DateTime.Parse((fsanodes[k].updatedonday + " " + Convert.ToString(fsanodes[k].updatedontime.Contains("(UTC)") == true ? fsanodes[k].updatedontime.Substring(0, 8) : fsanodes[k].updatedontime)));

                                    strDepartureTime = UpdatedON.ToString("MM/dd/yyyy HH:mm");
                                    //Date = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                    Date = DateTime.Parse((fsanodes[k].updatedonday + " " + Convert.ToString(fsanodes[k].updatedontime.Contains("(UTC)") == true ? fsanodes[k].updatedontime.Substring(0, 8) : fsanodes[k].updatedontime)));
                                    break;

                                case "BKD":
                                    airportcode = Convert.ToString(fsanodes[k].airportcode);
                                    /// origin = origin = origin.Length < 1 ? airportcode : origin;
                                    origin = Convert.ToString(fsanodes[k].fltorg) == "" ? fsadata.origin : Convert.ToString(fsanodes[k].fltorg);
                                    dest = dest == "" ? fsadata.dest : fsanodes[k].fltdest.ToString();

                                    //UpdatedON = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                    UpdatedON = DateTime.Parse((fsanodes[k].updatedonday + " " + Convert.ToString(fsanodes[k].updatedontime.Contains("(UTC)") == true ? fsanodes[k].updatedontime.Substring(0, 8) : fsanodes[k].updatedontime)));
                                    //Date = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year).ToString("yyyy-mm-dd"));
                                    Date = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));

                                    break;
                                case "FOH":

                                    airportcode = Convert.ToString(fsanodes[k].airportcode);
                                    origin = Convert.ToString(fsanodes[k].fltorg) == "" ? fsadata.origin : Convert.ToString(fsanodes[k].fltorg);
                                    dest = Convert.ToString(fsanodes[k].fltdest) == "" ? fsadata.dest : Convert.ToString(fsanodes[k].fltdest);
                                    // UpdatedON = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                    if (fsanodes[k].fltday != "")
                                    {
                                        UpdatedON = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                        //Date = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year).ToString("yyyy-mm-dd"));
                                        Date = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                    }
                                    else
                                    {
                                        UpdatedON = System.DateTime.Now;
                                        Date = DateTime.Parse("1900-01-01");
                                    }

                                    break;


                                default:
                                    airportcode = Convert.ToString(fsanodes[k].airportcode);
                                    origin = Convert.ToString(fsanodes[k].fltorg) == "" ? fsadata.origin : Convert.ToString(fsanodes[k].fltorg);
                                    dest = Convert.ToString(fsanodes[k].fltdest) == "" ? fsadata.dest : Convert.ToString(fsanodes[k].fltdest);
                                    // UpdatedON = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));

                                    UpdatedON = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                    //Date = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year).ToString("yyyy-mm-dd"));
                                    Date = DateTime.Parse((fsanodes[k].fltday + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));

                                    break;
                            }

                            if (messageType == "RCF" && DayChange != Convert.ToDateTime("1900-01-01"))
                            {
                                UpdatedON = DateTime.Parse(DayChange.ToString("MM/dd/yyyy") + " " + (fsanodes[k].flttime));
                            }


                            //dtb = new SQLServer();
                            //string[] CNname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight",
                            //    "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy",
                            //    "UpdatedOn", "Public" };
                            //SqlDbType[] CType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit };
                            //object[] CValues = new object[] { awbprefix, awbnum, fsadata.origin, fsadata.dest, fsanodes[k].numofpcs,
                            //    Convert.ToDouble(fsanodes[k].weight == "" ? "0" : fsanodes[k].weight), fltno, Date, origin, dest,
                            //    strAction, messageStatus, strDescription, Updatedby, strDepartureTime == "" ? UpdatedON.ToString() : strDepartureTime, 1 };
                            SqlParameter[] parametersAudit = new SqlParameter[] {
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar)    { Value = awbprefix },
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar)    { Value = awbnum },
                                new SqlParameter("@Origin", SqlDbType.VarChar)       { Value = fsadata.origin },
                                new SqlParameter("@Destination", SqlDbType.VarChar)  { Value = fsadata.dest },
                                new SqlParameter("@Pieces", SqlDbType.VarChar)       { Value = fsanodes[k].numofpcs },
                                new SqlParameter("@Weight", SqlDbType.VarChar)       { Value = Convert.ToDouble(fsanodes[k].weight == "" ? "0" : fsanodes[k].weight) },
                                new SqlParameter("@FlightNo", SqlDbType.VarChar)     { Value = fltno },
                                new SqlParameter("@FlightDate", SqlDbType.DateTime)  { Value = Date },
                                new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = origin },
                                new SqlParameter("@FlightDestination", SqlDbType.VarChar){ Value = dest },
                                new SqlParameter("@Action", SqlDbType.VarChar)       { Value = strAction },
                                new SqlParameter("@Message", SqlDbType.VarChar)      { Value = messageStatus },
                                new SqlParameter("@Description", SqlDbType.VarChar)  { Value = strDescription },
                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar)    { Value = Updatedby },
                                new SqlParameter("@UpdatedOn", SqlDbType.VarChar)    { Value = strDepartureTime == "" ? UpdatedON.ToString() : strDepartureTime },
                                new SqlParameter("@Public", SqlDbType.Bit)           { Value = 1 }
                            };
                            //if (!dtb.ExecuteProcedure("SPAddAWBAuditLog", CNname, CType, CValues))
                            if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", parametersAudit))
                            {
                                //clsLog.WriteLog("AWB Audit log  for:" + awbnum + Environment.NewLine + "Error: " + dtb.LastErrorDescription);
                                _logger.LogWarning("AWB Audit log  for:{0}", awbnum + Environment.NewLine);
                            }
                        }
                    }
                    #endregion Save AWB Record on Audit log Table
                }
            }
            catch (Exception ex)
            {
                flag = false;
                ErrorMsg = "Error occure while saving data through xFSU: [[" + ex.Message + "]]";

                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on SaveandUpdateXFSUMessage");
            }

            ErrorMsg = "";

            //return flag;
            return (flag, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray, ErrorMsg);
        }

        #endregion

        #endregion :: Public Methods ::

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
