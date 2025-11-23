using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
using System.Text;
using System.Xml;

namespace QidWorkerRole
{
    public class CustomsMessageProcessor
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<CustomsMessageProcessor> _logger;
        private readonly Func<cls_SCMBL> _cls_SCMBLFactory;
        private readonly GenericFunction _genericFunction;
        public CustomsMessageProcessor(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<CustomsMessageProcessor> logger,
            Func<cls_SCMBL> cls_SCMBLFactory,
            GenericFunction genericFunction
        )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _cls_SCMBLFactory = cls_SCMBLFactory;
            _genericFunction = genericFunction;
        }
        /// <summary>
        /// Method to decode customs onformation from XML and save to the database
        /// </summary>
        /// <param name="strMsg">Actual Message</param>
        /// <returns>Flag shows message is processed successfully or not</returns>
        public async Task<bool> DecodeAndSaveCustomsMessage(string message, int messageSerialNumber)
        {
            bool isMessageProcessed = false;
            try
            {
                string originalMessage = message;
                int startIndexOfXMLTag = 0, endIndexOfXMLTag = 0;
                startIndexOfXMLTag = message.IndexOf("<?xml");
                endIndexOfXMLTag = message.IndexOf("?>");
                if (startIndexOfXMLTag == 0 && endIndexOfXMLTag > 5)
                {
                    DataSet dsCustomsResponse = new DataSet();
                    message = message.Substring(endIndexOfXMLTag + 2);
                    var xmlText = new StringReader(message);
                    dsCustomsResponse.ReadXml(xmlText);
                    
                    //GenericFunction genericFunction = new GenericFunction();

                    if (dsCustomsResponse != null && dsCustomsResponse.Tables.Count > 2 && dsCustomsResponse.Tables[2].Rows.Count > 0)
                    {
                        if (dsCustomsResponse.Tables["MessageHeader"].Columns.Contains("ResponseCode") && dsCustomsResponse.Tables["MessageHeader"].Columns.Contains("MessageInID") && dsCustomsResponse.Tables["Codes"].Columns.Contains("Code"))
                        {
                            string messageID = string.Empty, manifestNo = string.Empty, responseCode = string.Empty;
                            messageID = dsCustomsResponse.Tables["MessageHeader"].Rows[0]["MessageInID"].ToString();
                            if (dsCustomsResponse.Tables["MessageHeader"].Columns.Contains("ManifestNo"))
                            {
                                manifestNo = dsCustomsResponse.Tables["MessageHeader"].Rows[0]["ManifestNo"].ToString();
                            }
                            responseCode = dsCustomsResponse.Tables["MessageHeader"].Rows[0]["ResponseCode"].ToString();
                            if (messageID.Trim() != string.Empty)
                            {
                                DataSet? dsResult = new DataSet();
                                SqlParameter[] sqlParameter = new SqlParameter[] {
                                      new SqlParameter("MessageID",messageID)
                                    , new SqlParameter("ResponseMessage",dsCustomsResponse.GetXml().ToString())
                                    , new SqlParameter("ManifestNo",manifestNo)
                                    , new SqlParameter("ResponseCode",responseCode)
                                };

                                //SQLServer sqlServer = new SQLServer();
                                //dsResult = sqlServer.SelectRecords("uspUpdateCustomsResponse", sqlParameter);
                                dsResult = await _readWriteDao.SelectRecords("uspUpdateCustomsResponse", sqlParameter);

                                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                                {
                                    isMessageProcessed = Convert.ToBoolean(dsResult.Tables[0].Rows[0]["Result"].ToString());
                                }
                            }
                        }
                        else
                        {
                            await _genericFunction.UpdateErrorMessageToInbox(messageSerialNumber, "Incomplete response information");
                            isMessageProcessed = false;
                        }
                    }
                    else
                    {
                        await _genericFunction.UpdateErrorMessageToInbox(messageSerialNumber, "XML format not recognised");
                        isMessageProcessed = false;
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                isMessageProcessed = false;
            }
            return isMessageProcessed;
        }

        /// <summary>
        /// Added by prashant
        /// Method to generate cusotm message automatically or manually, in manual case redirect to the respectve page depending on oring station's country code
        /// Configuration value (IsAutoGenerateCustomMessage) is used to switch between auto and manual message
        /// </summary>
        /// <param name="PartnerCode">Country code</param>
        public async Task GenerateCustomMessageXML(string FlightNumber, DateTime FlightDate, string FlightOrigin, string FlightDestination, string ImpExp, string AWBPrefix, string AWBNos, string CreatedBy, DateTime CreatedOn, string PartnerCode)
        {
            try
            {
                DataSet? dsAWBDetails = new DataSet();
                DataSet? dsIsAWBSavedSuccessfully = new DataSet();
                DataSet? dsIsHAWBSavedSuccessfully = new DataSet();

                //cls_SCMBL cls_scmbl = new cls_SCMBL();

                bool IsSavedSuccessfully = true;
                //string PartnerCode = GetCountryCode(FlightOrigin);
                switch (PartnerCode)
                {
                    case "OM":
                        bool IsHouse = false;

                        ///Send message automatically
                        dsIsAWBSavedSuccessfully = await AutoGenerateCustomMessage(FlightNumber, FlightDate, FlightOrigin, FlightDestination, ImpExp, AWBPrefix, AWBNos, CreatedBy, CreatedOn);

                        ///Send message automatically for HAWB
                        dsIsHAWBSavedSuccessfully = await AutoGenerateCustomMessageHouse(FlightNumber, FlightDate, FlightOrigin, FlightDestination, ImpExp, AWBPrefix, AWBNos, CreatedBy, CreatedOn);
                        ///dsIsAWBSavedSuccessfully - contains the value ('true' or 'House AWB not present') for successfull
                        if (dsIsAWBSavedSuccessfully != null)
                        {
                            if (dsIsHAWBSavedSuccessfully.Tables.Count > 0 && dsIsHAWBSavedSuccessfully.Tables[0].Rows.Count > 0)
                            {
                                string SuccessMsg = dsIsHAWBSavedSuccessfully.Tables[0].Rows[0][0].ToString();
                                if (!Boolean.TryParse(SuccessMsg, out IsHouse))
                                {
                                    IsHouse = false;
                                }
                            }
                            foreach (DataTable dt in dsIsAWBSavedSuccessfully.Tables)
                            {
                                if (!Convert.ToBoolean(dt.Rows[0][0].ToString().Trim()))
                                {
                                    IsSavedSuccessfully = false;
                                    break;
                                }
                            }
                        }
                        if (IsSavedSuccessfully)
                        {
                            ///Generate Custom Message for AWB
                            dsAWBDetails = await GetDataSetForXML(FlightNumber, FlightDate, FlightOrigin, FlightDestination, ImpExp, IsHouse);

                            StringBuilder sbAWB = await GenerateXMLForOM(dsAWBDetails);

                            if (await _cls_SCMBLFactory().addMsgToOutBox("OMAN Custom", sbAWB.ToString(), "", "SFTP", "FFM", DateTime.UtcNow, MessageData.MessageTypeName.OMCUSTOM_M, string.Empty, FlightNumber, FlightDate.ToString(), FlightOrigin, FlightDestination))
                            {
                                DataSet? dsUpdateXMLResult = new DataSet();
                                dsUpdateXMLResult = await SaveCustomMessageXML(FlightNumber, FlightDate, ImpExp, sbAWB.ToString(), IsHouse);
                                //Un-Used code
                                //if (dsUpdateXMLResult != null && dsUpdateXMLResult.Tables.Count > 0 && dsUpdateXMLResult.Tables[0].Rows.Count > 0)
                                //{
                                //    if (Convert.ToInt32(dsUpdateXMLResult.Tables[0].Rows[0][0].ToString()) > 0)
                                //    {
                                //        //ShowMessage(ref lblStatus, "Custom Message Sent Successfully..", MessageType.SuccessMessage);
                                //    }
                                //    else
                                //    {
                                //        //ShowMessage(ref lblStatus, "Fail to Save Custom Message XML..", MessageType.SuccessMessage);
                                //    }
                                //}
                            }

                            ///Generate Custom Message for HAWB
                            if (IsHouse)
                            {
                                DataSet? dsHAWBDetails = new DataSet();
                                dsHAWBDetails = await GetDataSetForXML(FlightNumber, FlightDate, FlightOrigin, FlightDestination, ImpExp, IsHouse);
                                StringBuilder sbHABW = await GenerateXMLForOMForHouse(dsHAWBDetails);
                                if (await _cls_SCMBLFactory().addMsgToOutBox("OMAN Custom", sbAWB.ToString(), "", "SFTP", "FFM", FlightDate, MessageData.MessageTypeName.OMCUSTOM_M, string.Empty, FlightNumber, FlightDate.ToString(), FlightOrigin, FlightDestination))
                                {
                                    DataSet? dsUpdateXMLResult = new DataSet();
                                    dsUpdateXMLResult = await SaveCustomMessageXML(FlightNumber, FlightDate, ImpExp, sbHABW.ToString(), IsHouse);
                                    //Un-Used code
                                    //if (dsUpdateXMLResult != null && dsUpdateXMLResult.Tables.Count > 0 && dsUpdateXMLResult.Tables[0].Rows.Count > 0)
                                    //{
                                    //    if (Convert.ToInt32(dsUpdateXMLResult.Tables[0].Rows[0][0].ToString()) > 0)
                                    //    {
                                    //        //ShowMessage(ref lblStatus, "Custom Message Sent Successfully..", MessageType.SuccessMessage);
                                    //    }
                                    //    else
                                    //    {
                                    //        //ShowMessage(ref lblStatus, "Fail to Save Custom Message XML..", MessageType.SuccessMessage);
                                    //    }
                                    //}
                                }
                            }
                        }
                        break;
                    case "BD":
                        ///Send message automatically
                        //dsIsAWBSavedSuccessfully = generalFuncation.AutoGenerateCustomMessageDAC(FlightNumber, FlightDate, FlightOrigin, FlightDestination, ImpExp, AWBPrefix, AWBNos, CreatedBy, CreatedOn);
                        //foreach (DataTable dt in dsIsAWBSavedSuccessfully.Tables)
                        //{
                        //    if (!Convert.ToBoolean(dt.Rows[0][0].ToString().Trim()))
                        //    {
                        //        IsSavedSuccessfully = false;
                        //        break;
                        //    }
                        //}
                        //if (IsSavedSuccessfully)
                        //{
                        //    dsAWBDetails = generalFuncation.GetDataSetForXMLDAC(FlightNumber, FlightDate, FlightOrigin, FlightDestination, ImpExp);
                        //    StringBuilder sb = generalFuncation.GenerateXMLForDAC(dsAWBDetails);
                        //    if (cls_BL.addMsgToOutBox("DHAKA Custom", sb.ToString(), "", "SFTP", Session["UserName"].ToString(), Convert.ToDateTime(Session["IT"].ToString()), "XMLCustomMessage"))
                        //    {
                        //        DataSet dsUpdateXMLResult = new DataSet();
                        //        //To be done : Replace last hard coded parameter
                        //        dsUpdateXMLResult = generalFuncation.SaveCustomMessageXML(FlightNumber, FlightDate, ImpExp, sb.ToString(), false);
                        //        if (dsUpdateXMLResult != null && dsUpdateXMLResult.Tables.Count > 0 && dsUpdateXMLResult.Tables[0].Rows.Count > 0)
                        //        {
                        //            if (Convert.ToInt32(dsUpdateXMLResult.Tables[0].Rows[0][0].ToString()) > 0)
                        //            {
                        //                // ShowMessage(ref lblStatus, "Custom Message Sent Successfully..", MessageType.SuccessMessage);
                        //            }
                        //            else
                        //            {
                        //                //ShowMessage(ref lblStatus, "Fail to Save Custom Message XML..", MessageType.SuccessMessage);
                        //            }
                        //        }
                        //    }
                        //}
                        break;
                    case "PH":
                        bool IsHousePH = false;
                        // clsLog.WriteLogAzure("Send PH_Custom message automatically");
                        _logger.LogInformation("Send PH_Custom message automatically");
                        ///Send message automatically
                        dsIsAWBSavedSuccessfully = await AutoGeneratePHCustomMessage(FlightNumber, FlightDate, FlightOrigin, FlightDestination, ImpExp, AWBPrefix, AWBNos, CreatedBy, CreatedOn);

                        if (dsIsAWBSavedSuccessfully != null)
                        {
                            foreach (DataTable dt in dsIsAWBSavedSuccessfully.Tables)
                            {
                                if (!Convert.ToBoolean(Convert.ToString(dt.Rows[0][0]).Trim()))
                                {
                                    // clsLog.WriteLogAzure("Insufficient data tp send PH_Custom message automatically: " + FlightNumber + ":" + FlightDate.ToString());
                                    _logger.LogInformation("Insufficient data tp send PH_Custom message automatically: {0} : {1} " , FlightNumber , FlightDate);
                                    IsSavedSuccessfully = false;
                                    break;
                                }
                            }
                        }
                        if (IsSavedSuccessfully)
                        {
                            // clsLog.WriteLogAzure("Get data to generate Custom Message for AWB: " + FlightNumber + ":" + FlightDate.ToString());
                            _logger.LogInformation($"Get data to generate Custom Message for AWB: {0} : {1}" , FlightNumber , FlightDate);

                            ///Generate Custom Message for AWB
                            dsAWBDetails = await GetDataSetForPHXML(FlightNumber, FlightDate, FlightOrigin, FlightDestination, ImpExp, IsHousePH);

                            // clsLog.WriteLogAzure("Generate Custom Message: " + FlightNumber + ":" + FlightDate.ToString());
                            _logger.LogInformation("Generate Custom Message: {0} : {1}" , FlightNumber,FlightDate);
                            StringBuilder sbAWB = GenerateXMLforPH(dsAWBDetails);

                            //string subject = FlightNumber.ToLower().Trim() + "_" + FlightDate.Year.ToString()+ FlightDate.Month.ToString().PadLeft(2, '0') + FlightDate.Day.ToString().PadLeft(2, '0') + "_" +  "imp" + "_" + DateTime.UtcNow.Year.ToString() + DateTime.UtcNow.Month.ToString().PadLeft(2,'0') + DateTime.UtcNow.Day.ToString().PadLeft(2, '0') + DateTime.UtcNow.Hour.ToString() + DateTime.UtcNow.Minute.ToString() + ".xml";
                            string subject = FlightNumber.ToLower().Trim() + "_" + FlightDate.Day.ToString().PadLeft(2, '0') + FlightDate.Month.ToString().PadLeft(2, '0') + FlightDate.Year.ToString() + "_" + "imp" + "_" + DateTime.UtcNow.Year.ToString() + DateTime.UtcNow.Month.ToString().PadLeft(2, '0') + DateTime.UtcNow.Day.ToString().PadLeft(2, '0') + DateTime.UtcNow.Hour.ToString() + DateTime.UtcNow.Minute.ToString() + ".xml";

                            //GenericFunction genericFunction = new GenericFunction();
                            if (await _genericFunction.SaveMessageOutBox(subject, sbAWB.ToString(), "", "SFTP", FlightOrigin, FlightDestination, FlightNumber, FlightDate.ToString("yyyy-MM-dd HH:mm:ss"), "", "FFM", MessageData.MessageTypeName.PHCUSTOM_M))
                            {
                                // clsLog.WriteLogAzure("Custom Message Saved to the outbox: " + subject);
                                _logger.LogInformation("Custom Message Saved to the outbox: {0}" , subject);
                                DataSet? dsUpdateXMLResult = new DataSet();
                                dsUpdateXMLResult = await SaveCustomMessageXML(FlightNumber, FlightDate, ImpExp, sbAWB.ToString(), IsHousePH);
                            }
                        }
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");

                throw;
            }
        }

        /// <summary>
        /// Generate XML string for Cebu customs using AWB details
        /// </summary>
        /// <param name="dsAwbDetails">Data set contains the AWB Details</param>
        /// <returns>XML string format</returns>
        private StringBuilder GenerateXMLforPH(DataSet dsAwbDetails)
        {
            StringBuilder sbCebuXml = new StringBuilder(string.Empty);
            try
            {
                //DataSet dsAwbDetails=new DataSet();

                //dsAwbDetails = GetDataSetForXMLCEBU(flightNumber, flightDate, flightOrigin, flightDestination);

                if (dsAwbDetails != null && dsAwbDetails.Tables != null && dsAwbDetails.Tables.Count > 0 && dsAwbDetails.Tables[0].Rows.Count > 0)
                {

                    #region TWM_Manifest

                    XmlDocument xml = new XmlDocument();

                    XmlElement root = xml.CreateElement("TWM");
                    //root.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    xml.AppendChild(root);

                    /* Root Element */
                    XmlElement TWM_Manifest_root = xml.CreateElement("TWM_Manifest");
                    TWM_Manifest_root.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    root.AppendChild(TWM_Manifest_root);

                    /* Identification_segment */
                    XmlElement Identification_segment = xml.CreateElement("Identification_segment");
                    TWM_Manifest_root.AppendChild(Identification_segment);

                    /* Registry_number Element */
                    XmlElement Registry_number = xml.CreateElement("Registry_number");
                    Registry_number.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["ManifestNo"]);//Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["Registry_number"]);
                    Identification_segment.AppendChild(Registry_number);

                    /* Customs_office_segment Element */
                    XmlElement Customs_office_segment = xml.CreateElement("Customs_office_segment");
                    Identification_segment.AppendChild(Customs_office_segment);

                    /* Code */
                    XmlElement Code = xml.CreateElement("Code");
                    Code.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["PortCode"]);
                    Customs_office_segment.AppendChild(Code);

                    /* Customs_office_segment_code */
                    XmlElement Customs_office_segment_code = xml.CreateElement("Customs_office_segment_code");
                    Customs_office_segment_code.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["Customs_office_segment_code"]);
                    Customs_office_segment.AppendChild(Customs_office_segment_code);

                    /* General_segment */
                    XmlElement General_segment = xml.CreateElement("General_segment");
                    TWM_Manifest_root.AppendChild(General_segment);

                    /* Master_information */
                    XmlElement Master_information = xml.CreateElement("Master_information");
                    Master_information.InnerText = "";//Convert.ToString(dsAwbDetails.Tables[0].Rows[0]["Master_information"]);
                    General_segment.AppendChild(Master_information);

                    /* Totals_segment */
                    XmlElement Totals_segment = xml.CreateElement("Totals_segment");
                    General_segment.AppendChild(Totals_segment);

                    /* Total_number_of_bols */
                    XmlElement Total_number_of_bols = xml.CreateElement("Total_number_of_bols");
                    Total_number_of_bols.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows.Count);
                    Totals_segment.AppendChild(Total_number_of_bols);

                    /* Total_number_of_containers */
                    XmlElement Total_number_of_containers = xml.CreateElement("Total_number_of_containers");
                    Total_number_of_containers.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["Total_number_of_containers"]);
                    Totals_segment.AppendChild(Total_number_of_containers);

                    /* Last_discharge */
                    XmlElement Last_discharge = xml.CreateElement("Last_discharge");
                    Last_discharge.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["DepartureDate"]);
                    General_segment.AppendChild(Last_discharge);

                    /* Arrival_segment */
                    XmlElement Arrival_segment = xml.CreateElement("Arrival_segment");
                    General_segment.AppendChild(Arrival_segment);

                    /* Date_of_arrival */
                    XmlElement Date_of_arrival = xml.CreateElement("Date_of_arrival");
                    Date_of_arrival.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["ArrivalDate"]);
                    Arrival_segment.AppendChild(Date_of_arrival);

                    /* Time_of_arrival */
                    XmlElement Time_of_arrival = xml.CreateElement("Time_of_arrival");
                    Time_of_arrival.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["ETA"]);
                    Arrival_segment.AppendChild(Time_of_arrival);

                    /* Departure_segment */
                    XmlElement Departure_segment = xml.CreateElement("Departure_segment");
                    General_segment.AppendChild(Departure_segment);

                    /* Departure_segment_Code */
                    XmlElement Departure_segment_Code = xml.CreateElement("Code");
                    Departure_segment_Code.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["PortofLoading"]);
                    Departure_segment.AppendChild(Departure_segment_Code);

                    /* Destination_segment */
                    XmlElement Destination_segment = xml.CreateElement("Destination_segment");
                    General_segment.AppendChild(Destination_segment);

                    /* Destination_segment_Code */
                    XmlElement Destination_segment_Code = xml.CreateElement("Code");
                    Destination_segment_Code.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["PortofDischarge"]);
                    Destination_segment.AppendChild(Destination_segment_Code);

                    /* Carrier_segment */
                    XmlElement Carrier_segment = xml.CreateElement("Carrier_segment");
                    General_segment.AppendChild(Carrier_segment);

                    /* Carrier_segment_Code */
                    XmlElement Carrier_segment_Code = xml.CreateElement("Code");
                    Carrier_segment_Code.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["Carrier_segment_Code"]);
                    Carrier_segment.AppendChild(Carrier_segment_Code);

                    /* Transport_segment */
                    XmlElement Transport_segment = xml.CreateElement("Transport_segment");
                    General_segment.AppendChild(Transport_segment);

                    /* Name_of_transporter */
                    XmlElement Name_of_transporter = xml.CreateElement("Name_of_transporter");
                    Name_of_transporter.InnerText = Convert.ToString(dsAwbDetails.Tables[0].Rows[0]["FlightNo"]);
                    Transport_segment.AppendChild(Name_of_transporter);

                    /* Place_of_transporter */
                    XmlElement Place_of_transporter = xml.CreateElement("Place_of_transporter");
                    Place_of_transporter.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["PlaceOfTrasporter"]);
                    Transport_segment.AppendChild(Place_of_transporter);

                    /* Mode_of_transport_segment */
                    XmlElement Mode_of_transport_segment = xml.CreateElement("Mode_of_transport_segment");
                    //Mode_of_transport_segment.InnerText = "";
                    Transport_segment.AppendChild(Mode_of_transport_segment);

                    /*  Mode_of_transport_segment_Code */
                    XmlElement Mode_of_transport_segment_Code = xml.CreateElement("Code");
                    Mode_of_transport_segment_Code.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["ModeofTransport"]);
                    Mode_of_transport_segment.AppendChild(Mode_of_transport_segment_Code);

                    /* Nationality_of_transport_segment */
                    XmlElement Nationality_of_transport_segment = xml.CreateElement("Nationality_of_transport_segment");
                    //Nationality_of_transport_segment.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["PortofLoading"]);
                    Transport_segment.AppendChild(Nationality_of_transport_segment);

                    /*  Nationality_of_transport_segment_Code */
                    XmlElement Nationality_of_transport_segment_Code = xml.CreateElement("Code");

                    if (Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["PortofLoading"]) != "")
                    {
                        Nationality_of_transport_segment_Code.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["PortofLoading"]).Substring(0, 2);
                    }

                    Nationality_of_transport_segment.AppendChild(Nationality_of_transport_segment_Code);

                    /* Transporter_registration_segment */
                    XmlElement Transporter_registration_segment = xml.CreateElement("Transporter_registration_segment");
                    //Transporter_registration_segment.InnerText = "";
                    Transport_segment.AppendChild(Transporter_registration_segment);

                    /*  Transporter_registration_segment_Registration_number */
                    XmlElement Transporter_registration_segment_Registration_number = xml.CreateElement("Registration_number");
                    Transporter_registration_segment_Registration_number.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["Registration_number"]);
                    Transporter_registration_segment.AppendChild(Transporter_registration_segment_Registration_number);

                    /*  Transporter_registration_segment_Registration_date */
                    XmlElement Transporter_registration_segment_Registration_date = xml.CreateElement("Registration_date");
                    Transporter_registration_segment_Registration_date.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["ArrivalDate"]);
                    Transporter_registration_segment.AppendChild(Transporter_registration_segment_Registration_date);

                    /* Tonnage_segment */
                    XmlElement Tonnage_segment = xml.CreateElement("Tonnage_segment");
                    General_segment.AppendChild(Tonnage_segment);

                    /*  Gross_tonnage */
                    XmlElement Gross_tonnage = xml.CreateElement("Gross_tonnage");
                    Gross_tonnage.InnerText = Convert.ToString(dsAwbDetails.Tables[6].Rows[0]["GrossWeightManifested"]);
                    Tonnage_segment.AppendChild(Gross_tonnage);

                    /*  Net_tonnage */
                    XmlElement Net_tonnage = xml.CreateElement("Net_tonnage");
                    Net_tonnage.InnerText = Convert.ToString(dsAwbDetails.Tables[6].Rows[0]["Net_tonnage"]);
                    Tonnage_segment.AppendChild(Net_tonnage);
                    #endregion

                    #region TWM_BOL

                    for (int i = 0; i < dsAwbDetails.Tables[3].Rows.Count; i++)

                    {
                        /* Root Element */
                        XmlElement TWM_BOL_root = xml.CreateElement("TWM_BOL");
                        TWM_BOL_root.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                        root.AppendChild(TWM_BOL_root);

                        /* Identification_segment */
                        XmlElement BOL_Identification_segment = xml.CreateElement("Identification_segment");
                        TWM_BOL_root.AppendChild(BOL_Identification_segment);

                        /* Registry_number Element */
                        XmlElement BOL_Registry_number = xml.CreateElement("Registry_number");
                        BOL_Registry_number.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["ManifestNo"]);//Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["Registry_number"]);
                        BOL_Identification_segment.AppendChild(BOL_Registry_number);

                        /* Bol_reference Element */
                        XmlElement Bol_reference = xml.CreateElement("Bol_reference");
                        Bol_reference.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["TransDocRefNo"]);
                        BOL_Identification_segment.AppendChild(Bol_reference);

                        /* Customs_office_segment */
                        XmlElement Bol_Customs_office_segment = xml.CreateElement("Customs_office_segment");
                        //Bol_Customs_office_segment.InnerText = Convert.ToString("73255722");
                        BOL_Identification_segment.AppendChild(Bol_Customs_office_segment);

                        /* Customs_office_segment_Code Element */
                        XmlElement Customs_office_segment_Code = xml.CreateElement("Code");
                        Customs_office_segment_Code.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["PortCode"]);
                        Bol_Customs_office_segment.AppendChild(Customs_office_segment_Code);

                        /* Bol_specific_segment */
                        XmlElement Bol_specific_segment = xml.CreateElement("Bol_specific_segment");
                        TWM_BOL_root.AppendChild(Bol_specific_segment);

                        /* Line_number Element */
                        XmlElement Line_number = xml.CreateElement("Line_number");
                        Line_number.InnerText = (i + 1).ToString();
                        Bol_specific_segment.AppendChild(Line_number);

                        /* Previous_document_reference Element */
                        XmlElement Previous_document_reference = xml.CreateElement("Previous_document_reference");
                        Previous_document_reference.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Prev_doc_ref"]);
                        Bol_specific_segment.AppendChild(Previous_document_reference);

                        /* Bol_Nature Element */
                        XmlElement Bol_Nature = xml.CreateElement("Bol_Nature");
                        Bol_Nature.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Bol_Nature"]);
                        Bol_specific_segment.AppendChild(Bol_Nature);

                        /* Unique_carrier_reference Element */
                        XmlElement Unique_carrier_reference = xml.CreateElement("Unique_carrier_reference");
                        Unique_carrier_reference.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Unique_carrier_reference"]);
                        Bol_specific_segment.AppendChild(Unique_carrier_reference);

                        /* BOL_Total_number_of_containers Element */
                        XmlElement BOL_Total_number_of_containers = xml.CreateElement("Total_number_of_containers");
                        BOL_Total_number_of_containers.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["Total_number_of_containers"]);
                        Bol_specific_segment.AppendChild(BOL_Total_number_of_containers);

                        /* BOL_Total_gross_mass_manifested Element */
                        XmlElement BOL_Total_gross_mass_manifested = xml.CreateElement("Total_gross_mass_manifested");
                        BOL_Total_gross_mass_manifested.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["GrossWeightManifested"]);
                        Bol_specific_segment.AppendChild(BOL_Total_gross_mass_manifested);

                        /* Volume_in_cubic_meters */
                        XmlElement BOL_Volume_in_cubic_meters = xml.CreateElement("Volume_in_cubic_meters");
                        BOL_Volume_in_cubic_meters.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Volume"]);
                        Bol_specific_segment.AppendChild(BOL_Volume_in_cubic_meters);

                        /* Bol_type_segment */
                        XmlElement Bol_type_segment = xml.CreateElement("Bol_type_segment");
                        //Bol_type_segment.InnerText = Convert.ToString("");
                        Bol_specific_segment.AppendChild(Bol_type_segment);

                        /* Bol_type_segment */
                        XmlElement Bol_type_segment_Code = xml.CreateElement("Code");
                        Bol_type_segment_Code.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Bol_type_segment_Code"]);
                        Bol_type_segment.AppendChild(Bol_type_segment_Code);

                        /* Exporter_segment */
                        XmlElement Bol_Exporter_segment = xml.CreateElement("Exporter_segment");
                        //Bol_Exporter_segment.InnerText = Convert.ToString("");
                        Bol_specific_segment.AppendChild(Bol_Exporter_segment);

                        /* Exporter_segment Code */
                        XmlElement Bol_Exporter_segment_Code = xml.CreateElement("Code");
                        Bol_Exporter_segment_Code.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorCustomsCode"]);
                        Bol_Exporter_segment.AppendChild(Bol_Exporter_segment_Code);

                        /* Bol_Exporter_segment_Name */
                        XmlElement Bol_Exporter_segment_Name = xml.CreateElement("Name");
                        Bol_Exporter_segment_Name.SetAttribute("oes1-unsign-content", "true");
                        Bol_Exporter_segment_Name.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorName"]);
                        Bol_Exporter_segment.AppendChild(Bol_Exporter_segment_Name);

                        /* Bol_Exporter_segment_Address */
                        XmlElement Bol_Exporter_segment_Address = xml.CreateElement("Address");
                        Bol_Exporter_segment_Address.SetAttribute("oes1-unsign-content", "true");

                        if (Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorStreet"]) != "")
                            Bol_Exporter_segment_Address.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorStreet"]) + "\n" + Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorCity"]) + "\n" + Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorCountry"]);
                        else if (Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorCity"]) != "")
                            Bol_Exporter_segment_Address.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorCity"]) + "\n" + Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorCountry"]);
                        else if (Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorCountry"]) != "")
                            Bol_Exporter_segment_Address.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorCountry"]);

                        Bol_Exporter_segment.AppendChild(Bol_Exporter_segment_Address);

                        /* Consignee_segment */
                        XmlElement Bol_Consignee_segment = xml.CreateElement("Consignee_segment");
                        //Bol_Consignee_segment.InnerText = Convert.ToString("");
                        Bol_specific_segment.AppendChild(Bol_Consignee_segment);

                        /* Consignee_segment Code */
                        XmlElement Bol_Consignee_segment_Code = xml.CreateElement("Code");
                        Bol_Consignee_segment_Code.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeCustomsCode"]);
                        Bol_Consignee_segment.AppendChild(Bol_Consignee_segment_Code);

                        /* Bol_Consignee_segment_Name */
                        XmlElement Bol_Consignee_segment_Name = xml.CreateElement("Name");
                        Bol_Consignee_segment_Name.SetAttribute("oes1-unsign-content", "true");
                        Bol_Consignee_segment_Name.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeName"]);
                        Bol_Consignee_segment.AppendChild(Bol_Consignee_segment_Name);

                        /* Bol_Consignee_segment_Address */
                        XmlElement Bol_Consignee_segment_Address = xml.CreateElement("Address");
                        Bol_Consignee_segment_Address.SetAttribute("oes1-unsign-content", "true");

                        if (Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeStreet"]) != "")
                            Bol_Consignee_segment_Address.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeStreet"]) + "\n" + Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["consigneeCity"]) + "\n" + Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeCountry"]);
                        else if (Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["consigneeCity"]) != "")
                            Bol_Consignee_segment_Address.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["consigneeCity"]) + "\n" + Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeCountry"]);
                        else if (Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeCountry"]) != "")
                            Bol_Consignee_segment_Address.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeCountry"]);

                        Bol_Consignee_segment.AppendChild(Bol_Consignee_segment_Address);

                        /* Notify_segment */
                        XmlElement Bol_Notify_segment = xml.CreateElement("Notify_segment");
                        //Bol_Notify_segment.InnerText = Convert.ToString("");
                        Bol_specific_segment.AppendChild(Bol_Notify_segment);

                        /* Consignee_segment Code */
                        XmlElement Bol_Notify_segment_Code = xml.CreateElement("Code");
                        Bol_Notify_segment_Code.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeCustomsCode"]);
                        Bol_Notify_segment.AppendChild(Bol_Notify_segment_Code);

                        /* Bol_Consignee_segment_Name */
                        XmlElement Bol_Notify_segment_Name = xml.CreateElement("Name");
                        Bol_Notify_segment_Name.SetAttribute("oes1-unsign-content", "true");
                        Bol_Notify_segment_Name.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeName"]);
                        Bol_Notify_segment.AppendChild(Bol_Notify_segment_Name);

                        /* Bol_Consignee_segment_Address */
                        XmlElement Bol_Notify_segment_Address = xml.CreateElement("Address");
                        Bol_Notify_segment_Address.SetAttribute("oes1-unsign-content", "true");

                        if (Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeStreet"]) != "")
                            Bol_Notify_segment_Address.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeStreet"]) + "\n" + Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["consigneeCity"]) + "\n" + Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeCountry"]);
                        else if (Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["consigneeCity"]) != "")
                            Bol_Notify_segment_Address.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["consigneeCity"]) + "\n" + Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeCountry"]);
                        else if (Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeCountry"]) != "")
                            Bol_Notify_segment_Address.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeCountry"]);

                        Bol_Notify_segment.AppendChild(Bol_Notify_segment_Address);

                        /*Place_of_loading_segment*/
                        XmlElement Bol_Place_of_loading_segment = xml.CreateElement("Place_of_loading_segment");
                        //Bol_Place_of_loading_segment.InnerText = Convert.ToString("");
                        Bol_specific_segment.AppendChild(Bol_Place_of_loading_segment);

                        /* Place_of_loading_segment Code */
                        XmlElement Bol_Place_of_loading_segment_Code = xml.CreateElement("Code");
                        Bol_Place_of_loading_segment_Code.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["PortofLoading"]);
                        Bol_Place_of_loading_segment.AppendChild(Bol_Place_of_loading_segment_Code);

                        /*Place_of_unloading_segment*/
                        XmlElement Bol_Place_of_unloading_segment = xml.CreateElement("Place_of_unloading_segment");
                        //Bol_Place_of_loading_segment.InnerText = Convert.ToString("");
                        Bol_specific_segment.AppendChild(Bol_Place_of_unloading_segment);

                        /* Place_of_unloading_segment Code */
                        XmlElement Bol_Place_of_unloading_segment_Code = xml.CreateElement("Code");
                        Bol_Place_of_unloading_segment_Code.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["PortofDischarge"]);
                        Bol_Place_of_unloading_segment.AppendChild(Bol_Place_of_unloading_segment_Code);

                        /*Packages_segment*/
                        XmlElement Bol_Packages_segment = xml.CreateElement("Packages_segment");
                        //Bol_Packages_segment.InnerText = Convert.ToString("");
                        Bol_specific_segment.AppendChild(Bol_Packages_segment);

                        /* Package_type_code */
                        XmlElement Bol_Package_type_code = xml.CreateElement("Package_type_code");
                        Bol_Package_type_code.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Package_type_code"]);
                        Bol_Packages_segment.AppendChild(Bol_Package_type_code);

                        /* Package_type_code */
                        XmlElement Bol_Number_of_packages = xml.CreateElement("Number_of_packages");
                        Bol_Number_of_packages.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NoOfPackages"]);
                        Bol_Packages_segment.AppendChild(Bol_Number_of_packages);

                        /*Shipping_segment*/
                        XmlElement Bol_Shipping_segment = xml.CreateElement("Shipping_segment");
                        //Bol_Shipping_segment.InnerText = Convert.ToString("");
                        Bol_specific_segment.AppendChild(Bol_Shipping_segment);

                        /*Shipping_marks*/
                        XmlElement Bol_Shipping_marks = xml.CreateElement("Shipping_marks");
                        Bol_Shipping_marks.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Shipping_marks"]);
                        Bol_Shipping_segment.AppendChild(Bol_Shipping_marks);

                        /*Goods_segment*/
                        XmlElement Bol_Goods_segment = xml.CreateElement("Goods_segment");
                        //Bol_Goods_segment.InnerText = Convert.ToString("");
                        Bol_specific_segment.AppendChild(Bol_Goods_segment);

                        /*Goods_description*/
                        XmlElement Bol_Goods_description = xml.CreateElement("Goods_description");
                        Bol_Goods_description.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ItemDescription"]);
                        Bol_Goods_segment.AppendChild(Bol_Goods_description);

                        /*Freight_segment*/
                        XmlElement Bol_Freight_segment = xml.CreateElement("Freight_segment");
                        //Bol_Freight_segment.InnerText = Convert.ToString("");
                        Bol_specific_segment.AppendChild(Bol_Freight_segment);

                        /*Freight_segment_Value*/
                        XmlElement Freight_segment_Value = xml.CreateElement("Value");
                        Freight_segment_Value.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["IATA_MKT_freight"]);
                        Bol_Freight_segment.AppendChild(Freight_segment_Value);

                        /*Freight_segment_Currency*/
                        XmlElement Freight_segment_Currency = xml.CreateElement("Currency");
                        Freight_segment_Currency.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Currency"]);
                        Bol_Freight_segment.AppendChild(Freight_segment_Currency);

                        /*Freight_segment_Indicator_segment*/
                        XmlElement Freight_segment_Indicator_segment = xml.CreateElement("Indicator_segment");
                        //Freight_segment_Indicator_segment.InnerText = Convert.ToString("");
                        Bol_Freight_segment.AppendChild(Freight_segment_Indicator_segment);

                        /*Indicator_segment_Code*/
                        XmlElement Indicator_segment_Code = xml.CreateElement("Code");
                        Indicator_segment_Code.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Indicator_segment_Code"]);
                        Freight_segment_Indicator_segment.AppendChild(Indicator_segment_Code);

                        /*Customs_segment*/
                        XmlElement Bol_Customs_segment = xml.CreateElement("Customs_segment");
                        //Bol_Customs_segment.InnerText = Convert.ToString("");
                        Bol_specific_segment.AppendChild(Bol_Customs_segment);

                        /*Customs_segment_Value*/
                        XmlElement Bol_Customs_segment_Value = xml.CreateElement("Value");
                        Bol_Customs_segment_Value.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Customs_segment_Code"]);
                        Bol_Customs_segment.AppendChild(Bol_Customs_segment_Value);

                        /*Customs_segment_Currency*/
                        XmlElement Bol_Customs_segment_Currency = xml.CreateElement("Currency");
                        Bol_Customs_segment_Currency.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Currency"]);
                        Bol_Customs_segment.AppendChild(Bol_Customs_segment_Currency);

                        /*Transport_segment*/
                        XmlElement Bol_Transport_segment = xml.CreateElement("Transport_segment");
                        //Bol_Customs_segment.InnerText = Convert.ToString("");
                        Bol_specific_segment.AppendChild(Bol_Transport_segment);

                        /*Transport_segment_Value*/
                        XmlElement Bol_Transport_segment_Value = xml.CreateElement("Value");
                        Bol_Transport_segment_Value.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Transport_segment_Code"]);
                        Bol_Transport_segment.AppendChild(Bol_Transport_segment_Value);

                        /*Transport_segment_Currency*/
                        XmlElement Bol_Transport_segment_Currency = xml.CreateElement("Currency");
                        Bol_Transport_segment_Currency.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Currency"]);
                        Bol_Transport_segment.AppendChild(Bol_Transport_segment_Currency);


                        /*Insurance_segment*/
                        XmlElement Bol_Insurance_segment = xml.CreateElement("Insurance_segment");
                        //Bol_Insurance_segment.InnerText = Convert.ToString("");
                        Bol_specific_segment.AppendChild(Bol_Insurance_segment);

                        /*Insurance_segment_Value*/
                        XmlElement Bol_Insurance_segment_Value = xml.CreateElement("Value");
                        Bol_Insurance_segment_Value.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Insurance_segment_Code"]);
                        Bol_Insurance_segment.AppendChild(Bol_Insurance_segment_Value);

                        /*Insurance_segment_Currency*/
                        XmlElement Bol_Insurance_segment_Currency = xml.CreateElement("Currency");
                        Bol_Insurance_segment_Currency.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Currency"]);
                        Bol_Insurance_segment.AppendChild(Bol_Insurance_segment_Currency);

                        /*Seals_segment*/
                        XmlElement Bol_Seals_segment = xml.CreateElement("Seals_segment");
                        //Bol_Seals_segment.InnerText = Convert.ToString("");
                        Bol_specific_segment.AppendChild(Bol_Seals_segment);

                        /*Seals_segment_Number_of_seals*/
                        XmlElement Seals_segment_Number_of_seals = xml.CreateElement("Number_of_seals");
                        Seals_segment_Number_of_seals.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Number_of_seals"]);
                        Bol_Seals_segment.AppendChild(Seals_segment_Number_of_seals);

                        /*Seals_segment_Marks_of_seals*/
                        XmlElement Seals_segment_Marks_of_seals = xml.CreateElement("Marks_of_seals");
                        Seals_segment_Marks_of_seals.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Marks_of_seals"]);
                        Bol_Seals_segment.AppendChild(Seals_segment_Marks_of_seals);

                        /*Sealing_party_code*/
                        XmlElement Bol_Sealing_party_code = xml.CreateElement("Sealing_party_code");
                        Bol_Sealing_party_code.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Sealing_party_code"]);
                        Bol_Seals_segment.AppendChild(Bol_Sealing_party_code);

                        /*Information_segment*/
                        XmlElement Information_segment = xml.CreateElement("Information_segment");
                        //Information_segment.InnerText = Convert.ToString("");
                        Bol_specific_segment.AppendChild(Information_segment);

                        /*Information_part_a*/
                        XmlElement Information_part_a = xml.CreateElement("Information_part_a");
                        Information_part_a.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Information_part_a"]);
                        Information_segment.AppendChild(Information_part_a);

                        /*Operations_segment*/
                        XmlElement Operations_segment = xml.CreateElement("Operations_segment");
                        //Operations_segment.InnerText = Convert.ToString("");
                        Bol_specific_segment.AppendChild(Operations_segment);

                        /*Operations_segment_Location_segment*/
                        XmlElement Operations_segment_Location_segment = xml.CreateElement("Location_segment");
                        //Operations_segment_Location_segment.InnerText = Convert.ToString("");
                        Operations_segment.AppendChild(Operations_segment_Location_segment);

                        /*Operations_segment_Location_segment  Code*/
                        XmlElement Location_segment_Code = xml.CreateElement("Code");
                        Location_segment_Code.InnerText = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["PortofLoading"]);
                        Operations_segment_Location_segment.AppendChild(Location_segment_Code);

                        /*Operations_segment_Location_segment Information*/
                        XmlElement Location_segment_Information = xml.CreateElement("Information");
                        Location_segment_Information.InnerText = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["Location_segment_Information"]);
                        Operations_segment_Location_segment.AppendChild(Location_segment_Information);


                    }
                    #endregion


                    sbCebuXml = new StringBuilder(xml.OuterXml);

                    ///Remove the empty tags from TWM_Manifest_xml
                    var document = System.Xml.Linq.XDocument.Parse(sbCebuXml.ToString());
                    //var emptyNodes = document.Descendants().Where(e => e.IsEmpty || String.IsNullOrWhiteSpace(e.Value));
                    //foreach (var emptyNode in emptyNodes.ToArray())
                    //{
                    //    emptyNode.Remove();
                    //}
                    sbCebuXml = new StringBuilder(document.ToString());

                    sbCebuXml.Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
                }
                else
                {
                    sbCebuXml.AppendLine("No Record to generate custom xml");
                }

                return sbCebuXml;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return sbCebuXml;
        }

        /// <summary>
        /// Select custom message data for selected flight
        /// </summary>
        private async Task<DataSet?> GetDataSetForPHXML(string flightNo, DateTime flightDate, string flightOrigin, string flightDestination, string exportImport, bool isHouse)
        {
            DataSet? dsResult = new DataSet();
            try
            {
                //SQLServer sqlServer = new SQLServer();

                SqlParameter[] sqlParameter = new SqlParameter[] {
                         new SqlParameter("@FltNumber",flightNo)
                        ,new SqlParameter("@FltDate",flightDate)
                        ,new SqlParameter("@FltOrigin",flightOrigin)
                        ,new SqlParameter("@FltDestination",flightDestination)
                        ,new SqlParameter("@CustomeMessageType",exportImport)
                        ,new SqlParameter("@IsHouse",isHouse)
                        ,new SqlParameter("@H_SID","0")

                };

                //dsResult = sqlServer.SelectRecords("uspGeneratePHCustomMessageXML", sqlParameter);
                dsResult = await _readWriteDao.SelectRecords("uspGeneratePHCustomMessageXML", sqlParameter);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dsResult;
        }


        private async Task<DataSet?> AutoGeneratePHCustomMessage(string flightNumber, DateTime flightDate, string flightOrigin,
            string flightDestination, string messageType, string awbPrefix, string awbNos, string createdBy, DateTime createdOn)
        {
            DataSet? dsResult = new DataSet();
            try
            {
                //SQLServer sqlServer = new SQLServer();
                SqlParameter[] sqlParameter = new SqlParameter[] {
                      new SqlParameter("@FlightNo",flightNumber)
                    , new SqlParameter("@FlightDate",flightDate)
                    , new SqlParameter("@FlightOrigin",flightOrigin)
                    , new SqlParameter("@FlightDest",flightDestination)
                    , new SqlParameter("@CustomeMessageType",messageType)
                    , new SqlParameter("@AWBPrefix",awbPrefix)
                    , new SqlParameter("@AWBNos",awbNos)
                    , new SqlParameter("@CreatedBy",createdBy)
                    , new SqlParameter("@CreatedOn",createdOn)

                };
                //dsResult = sqlServer.SelectRecords("uspAutoGeneratePHCustomMessage", sqlParameter);
                dsResult = await _readWriteDao.SelectRecords("uspAutoGeneratePHCustomMessage", sqlParameter);

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dsResult;
        }

        /// <summary>
        /// Call 'uspAutoGenerateCustomMessage' procedure that select the AWB details from respective operations tables and 
        /// Store data into Custom Message tables
        /// </summary>
        /// <returns>Function returns the data set that contains flags indicating the data insertion/updatation operations are fail or success</returns>
        public async Task<DataSet?> AutoGenerateCustomMessage(string flightNumber, DateTime flightDate, string flightOrigin,
            string flightDestination, string messageType, string awbPrefix, string awbNos, string createdBy, DateTime createdOn)
        {
            try
            {
                //SQLServer sqlServer = new SQLServer();
                SqlParameter[] sqlParameter = new SqlParameter[] {
                      new SqlParameter("@FlightNo",flightNumber)
                    , new SqlParameter("@FlightDate",flightDate)
                    , new SqlParameter("@FlightOrigin",flightOrigin)
                    , new SqlParameter("@FlightDest",flightDestination)
                    , new SqlParameter("@CustomeMessageType",messageType)
                    , new SqlParameter("@AWBPrefix",awbPrefix)
                    , new SqlParameter("@AWBNos",awbNos)
                    , new SqlParameter("@CreatedBy",createdBy)
                    , new SqlParameter("@CreatedOn",createdOn)

                };
                //return sqlServer.SelectRecords("uspAutoGenerateCustomMessage", sqlParameter);
                return await _readWriteDao.SelectRecords("uspAutoGenerateCustomMessage", sqlParameter);

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        public async Task<DataSet?> AutoGenerateCustomMessageHouse(string flightNumber, DateTime flightDate, string flightOrigin, string flightDestination,
          string messageType, string awbPrefix, string awbNos, string createdBy, DateTime createdOn)
        {
            try
            {
                //SQLServer sqlServer = new SQLServer();
                SqlParameter[] sqlParameter = new SqlParameter[] {
                      new SqlParameter("@FlightNo",flightNumber)
                    , new SqlParameter("@FlightDate",flightDate)
                    , new SqlParameter("@FlightOrigin",flightOrigin)
                    , new SqlParameter("@FlightDest",flightDestination)
                    , new SqlParameter("@CustomeMessageType",messageType)
                    , new SqlParameter("@AWBPrefix",awbPrefix)
                    , new SqlParameter("@AWBNos",awbNos)
                    , new SqlParameter("@CreatedBy",createdBy)
                    , new SqlParameter("@CreatedOn",createdOn)

                };
                //return sqlServer.SelectRecords("uspAutoGenerateCustomMessageHouse", sqlParameter);
                return await _readWriteDao.SelectRecords("uspAutoGenerateCustomMessageHouse", sqlParameter);

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        /// <summary>
        /// Select the data for given flight to generate the XML file
        /// </summary>
        /// <returns></returns>
        public async Task<DataSet?> GetDataSetForXML(string flightNumber, DateTime flightDate, string flightOrigin, string flightDestination, string exportImport, bool isHouse)
        {
            try
            {
                //SQLServer sqlServer = new SQLServer();
                SqlParameter[] sqlParameter = new SqlParameter[] {
                         new SqlParameter("@FltNumber",flightNumber)
                        ,new SqlParameter("@FltDate",flightDate)
                        ,new SqlParameter("@FltOrigin",flightOrigin)
                        ,new SqlParameter("@FltDestination",flightDestination)
                        ,new SqlParameter("@CustomeMessageType",exportImport)
                        ,new SqlParameter("@IsHouse",isHouse)
                        ,new SqlParameter("@H_SID","0")
                };

                //return sqlServer.SelectRecords("uspGenerateCustomMessageXML", sqlParameter);
                return await _readWriteDao.SelectRecords("uspGenerateCustomMessageXML", sqlParameter);


            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        /// <summary>
        /// Generate XML string for OMAN customs using the AWB details
        /// </summary>
        /// <param name="dsAwbDetails">Data set contains AWB Details</param>
        /// <returns>XML string format</returns>
        public async Task<StringBuilder> GenerateXMLForOM(DataSet dsAwbDetails)
        {
            try
            {
                StringBuilder sb = new StringBuilder(string.Empty);

                //var xmlSchemaTable = GetXMLMessageData("OM").Tables[0];

                var xmlMsgRes = await GetXMLMessageData("OM");

                var xmlSchemaTable = xmlMsgRes?.Tables[0];

                if (xmlSchemaTable != null && xmlSchemaTable.Rows.Count > 0)
                {
                    string strMessageXml = xmlSchemaTable.Rows[0]["XMLMessageData"].ToString();
                    string version = xmlSchemaTable.Rows[0]["Version"].ToString();
                    var nfmXmlDataSet = new DataSet();
                    var tx = new System.IO.StringReader(strMessageXml);
                    nfmXmlDataSet.ReadXml(tx);
                    string flightOrigin = string.Empty, flightDetination = string.Empty;

                    #region : Assign Custom Message data to the XML tags :
                    //Message Header
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["MessageId"] = Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["MessageID"]);
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["ServiceID"] = Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["ServiceID"]);
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["SchemaVersionNo"] = Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["SchemaVersionNo"]);
                    //nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["SchemaVersionNo"] = Version;
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["CompanyCode"] = Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["CompanyCode"]);
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["UserId"] = Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["UserID"]);
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["MessageDate"] = Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["MessageDate"]);
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["MessageType"] = Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["MessageType"]).Trim();
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["VASCode"] = Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["VASCode"]).Trim();
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["ProcessingIndicator"] = Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["ProcessingIndicator"]).Trim();

                    //Manifest Header
                    ///As per discussed with swapnil sir, manifest number should display only after appended
                    if (Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["MessageType"]).Trim() == "A")
                    {
                        nfmXmlDataSet.Tables["ManifestHeader"].Rows[0]["ManifestNo"] = dsAwbDetails.Tables[2].Rows[0]["ManifestNo"].ToString();
                    }

                    nfmXmlDataSet.Tables["ManifestHeader"].Rows[0]["inboundOutbound"] = dsAwbDetails.Tables[2].Rows[0]["ManifestType"].ToString();
                    var inBoundOutBound = dsAwbDetails.Tables[2].Rows[0]["ManifestType"].ToString();
                    nfmXmlDataSet.Tables["ManifestHeader"].Rows[0]["transportMode"] = dsAwbDetails.Tables[2].Rows[0]["ModeofTransport"].ToString();
                    nfmXmlDataSet.Tables["ManifestHeader"].Rows[0]["portDischarge"] = dsAwbDetails.Tables[2].Rows[0]["FinalDestination"].ToString();
                    flightOrigin = dsAwbDetails.Tables[2].Rows[0]["PortofLoading"].ToString();
                    nfmXmlDataSet.Tables["ManifestHeader"].Rows[0]["portLoading"] = dsAwbDetails.Tables[2].Rows[0]["PortofLoading"].ToString();
                    flightDetination = dsAwbDetails.Tables[2].Rows[0]["FinalDestination"].ToString();
                    nfmXmlDataSet.Tables["ManifestHeader"].Rows[0]["finalDestination"] = dsAwbDetails.Tables[2].Rows[0]["FinalDestination"].ToString();
                    if (inBoundOutBound.Trim() == "I")
                    {
                        nfmXmlDataSet.Tables["ManifestHeader"].Rows[0]["eta"] = dsAwbDetails.Tables[2].Rows[0]["ArrivalDate"].ToString();
                    }
                    else
                    {
                        nfmXmlDataSet.Tables["ManifestHeader"].Rows[0]["etd"] = dsAwbDetails.Tables[2].Rows[0]["DepartureDate"].ToString();
                    }

                    nfmXmlDataSet.Tables["ManifestHeader"].Rows[0]["masterName"] = dsAwbDetails.Tables[2].Rows[0]["MasterName"].ToString();
                    nfmXmlDataSet.Tables["ManifestHeader"].Rows[0]["masterNationality"] = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["MasterNationality"]);
                    nfmXmlDataSet.Tables["ManifestHeader"].Rows[0]["conveyanceNo"] = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["ConveyanceNo"]).ToUpper();
                    nfmXmlDataSet.Tables["ManifestHeader"].Rows[0]["conveyanceNationality"] = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["ConveyanceNationality"]);
                    nfmXmlDataSet.Tables["ManifestHeader"].Rows[0]["carrierCode"] = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["CarrierCode"]);
                    nfmXmlDataSet.Tables["ManifestHeader"].Rows[0]["conveyanceName"] = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["ConveyanceName"]);
                    nfmXmlDataSet.Tables["ManifestHeader"].Rows[0]["affectedParties"] = Convert.ToString(dsAwbDetails.Tables[2].Rows[0]["AffectedParties"]);

                    //Transport Document
                    for (int i = 0; i < dsAwbDetails.Tables[3].Rows.Count; i++)
                    {
                        if (!(nfmXmlDataSet.Tables["TransportDocument"].Rows.Count > i))
                        {
                            //Add new row and set ParentID for TransportDocument Node
                            DataRow dr = nfmXmlDataSet.Tables["TransportDocument"].NewRow();
                            dr["TransportDocumentList_Id"] = 0;
                            nfmXmlDataSet.Tables["TransportDocument"].Rows.Add(dr);
                        }
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["sequenceNo"] = (i + 1).ToString();
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["transDocRefNo"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["TransDocRefNo"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["transDocType"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["TransDocType"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["transDocIssueDate"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["TransDocIssueDate"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["transDocIndicator"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["TransDocIndicator"]);
                        //nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["noOfHouse"] = ;
                        ///Consolidator Code - Only for AWB which contains houses
                        if (Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["TransDocType"]).Trim() == "741")
                        {
                            nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consolidatorCode"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsolidatorCode"]);
                        }
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["grossWeightManifested"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["GrossWeightManifested"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["grossWeightLoaded"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["GrossWeightLoaded"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["netWeightManifested"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NetWeightManifested"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["netWeightLoaded"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NetWeightLoaded"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["totalQuantity"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["TotalQuantity"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["totalQuantityUOM"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["TotalQuantityUOM"]).Trim();
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["isOwnershipTransferred"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["IsOwnershipTransferred"]) == "0" ? "N" : "Y";
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["cargoType"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["CargoType"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["UCRNo"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["UCRNo"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consigneeCustomsCode"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeCustomsCode"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consigneeName"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeName"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consigneeStreet"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeStreet"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consigneeCity"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeCity"]) == string.Empty ? (Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeStreet"]) == string.Empty ? flightDetination : Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeStreet"])) : Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeCity"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consigneeSubRegion"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeSubRegion"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consigneeCountry"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeCountry"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consigneePostCode"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneePostCode"]);
                        //Changed as per Mail from Aman on 'Fri 06/10/2016' Sub - FW: Custom Message File for Export and Import - Attachment - Oman Air MM Final
                        //this tag is not required for inbound cargo
                        if (inBoundOutBound.Trim() == "I")
                        {
                            nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consignorCustomsCode"] = string.Empty;
                        }
                        else
                        {
                            nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consignorCustomsCode"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorCustomsCode"]);
                        }
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consignorName"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorName"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consignorStreet"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorStreet"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consignorCity"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorCity"]) == string.Empty ? flightOrigin : Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorCity"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consignorSubRegion"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorSubRegion"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consignorCountry"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorCountry"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consignorPostCode"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["consignorPostCode"]);
                        // Sub - FW: Custom Message File for Export and Import - Attachment - Oman Air MM Final
                        //this tag is not required for inbound cargo
                        if (inBoundOutBound.Trim() == "I")
                        {
                            nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["notifyCustomsCode"] = string.Empty;
                        }
                        else
                        {
                            nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["notifyCustomsCode"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NotifyCustomsCode"]);
                        }

                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["notifyPartyName"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NotifyPartyName"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["notifyPartyStreet"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NotifyPartyStreet"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["notifyPartyCity"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NotifyPartyCity"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["notifyPartySubRegion"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NotifyPartySubRegion"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["notifyPartyCountry"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NotifyPartyCountry"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["notifyPartyPostCode"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NotifyPartyPostCode"]);
                    }
                    //TransDocItem
                    for (int i = 0; i < dsAwbDetails.Tables[3].Rows.Count; i++)
                    {
                        if (!(nfmXmlDataSet.Tables["TransDocItem"].Rows.Count > i))
                        {
                            //Add new row and set ParentID for TransportDocItemList Node
                            DataRow drTranportDocItemList = nfmXmlDataSet.Tables["TranportDocItemList"].NewRow();
                            drTranportDocItemList["TransportDocument_Id"] = i;
                            nfmXmlDataSet.Tables["TranportDocItemList"].Rows.Add(drTranportDocItemList);

                            //Add new row and set ParentID for TransDocItem Node
                            DataRow drTransDocItem = nfmXmlDataSet.Tables["TransDocItem"].NewRow();
                            drTransDocItem["TranportDocItemList_Id"] = i;
                            nfmXmlDataSet.Tables["TransDocItem"].Rows.Add(drTransDocItem);

                            //Add new row and set ParentID for RouteList Node
                            DataRow drRouteList = nfmXmlDataSet.Tables["RouteList"].NewRow();
                            drRouteList["TransportDocument_Id"] = i;
                            nfmXmlDataSet.Tables["RouteList"].Rows.Add(drRouteList);
                        }
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["sequenceNo"] = "1";
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["cargoType"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["CargoTypeItem"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["itemDesc"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ItemDescription"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["marksAndNos"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["MarksAndNos"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["originCountry"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["OriginCountry"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["totalQuantity"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["TotalQuantityItem"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["totalQuantityUOM"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["TotalQuantityUOMItem"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["noOfPackages"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NoOfPackages"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["noOfPackagesUOM"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NoOfPackagesUOM"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["grossWeight"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["GrossWeight"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["undgClass"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["UndgClass"]);
                    }
                    for (int i = 0; i < dsAwbDetails.Tables[4].Rows.Count; i++)
                    {
                        if (!(nfmXmlDataSet.Tables["Route"].Rows.Count > i))
                        {
                            //Add new row and set ParentID for Route Node
                            DataRow dr = nfmXmlDataSet.Tables["Route"].NewRow();
                            dr["RouteList_Id"] = 0;
                            nfmXmlDataSet.Tables["Route"].Rows.Add(dr);
                        }
                        nfmXmlDataSet.Tables["Route"].Rows[i]["sequenceNo"] = Convert.ToString(dsAwbDetails.Tables[4].Rows[i]["SequenceNo"]);
                        nfmXmlDataSet.Tables["Route"].Rows[i]["locationType"] = Convert.ToString(dsAwbDetails.Tables[4].Rows[i]["LocationType"]);
                        nfmXmlDataSet.Tables["Route"].Rows[i]["CountryCode"] = Convert.ToString(dsAwbDetails.Tables[4].Rows[i]["CountryCode"]);
                        nfmXmlDataSet.Tables["Route"].Rows[i]["portCode"] = Convert.ToString(dsAwbDetails.Tables[4].Rows[i]["CountryCode"]) + Convert.ToString(dsAwbDetails.Tables[4].Rows[i]["PortCode"]);
                        nfmXmlDataSet.Tables["Route"].Rows[i]["portType"] = Convert.ToString(dsAwbDetails.Tables[4].Rows[i]["PortType"]);

                        //Change the RouteList_Id for Route table using the HeaderID(FK_TransportDocumentRoute Table) and TransportDocument SerialNumber
                        for (int j = 0; j < dsAwbDetails.Tables[3].Rows.Count; j++)
                        {
                            if (Convert.ToInt32(dsAwbDetails.Tables[3].Rows[j]["SerialNumber"]) == Convert.ToInt32(dsAwbDetails.Tables[4].Rows[i]["HeaderID"]))
                            {
                                nfmXmlDataSet.Tables["Route"].Rows[i]["RouteList_Id"] = Convert.ToString(nfmXmlDataSet.Tables["RouteList"].Rows[j]["RouteList_Id"]);
                            }
                        }

                    }

                    for (int i = 0; i < dsAwbDetails.Tables[5].Rows.Count; i++)
                    {
                        if (!(nfmXmlDataSet.Tables["TripRoute"].Rows.Count > i))
                        {
                            //Add new row and set ParentID for TripRoute Node
                            DataRow dr = nfmXmlDataSet.Tables["TripRoute"].NewRow();
                            dr["TripList_Id"] = "0";
                            nfmXmlDataSet.Tables["TripRoute"].Rows.Add(dr);
                        }
                        nfmXmlDataSet.Tables["TripRoute"].Rows[i]["sequenceNo"] = dsAwbDetails.Tables[5].Rows[i]["SequenceNo"].ToString();
                        nfmXmlDataSet.Tables["TripRoute"].Rows[i]["locationType"] = dsAwbDetails.Tables[5].Rows[i]["LocationType"].ToString();
                        nfmXmlDataSet.Tables["TripRoute"].Rows[i]["CountryCode"] = dsAwbDetails.Tables[5].Rows[i]["CountryCode"].ToString();
                        nfmXmlDataSet.Tables["TripRoute"].Rows[i]["portCode"] = dsAwbDetails.Tables[5].Rows[i]["CountryCode"].ToString() + dsAwbDetails.Tables[5].Rows[i]["PortCode"].ToString();
                    }
                    #endregion

                    string strXml = nfmXmlDataSet.GetXml();
                    sb = new StringBuilder(strXml);

                    sb.Replace(" xmlns:cm=\"http://bayan.gov.om/schema/common\"", "");
                    sb.Replace(" xml:space=\"preserve\"", "");
                    sb.Replace(" xmlns:tr=\"http://bayan.gov.om/schema/transport\"", "");
                    sb.Replace(" xmlns:tt=\"http://bayan.gov.om/schema/item\"", "");
                    sb.Insert(22, " xsi:noNamespaceSchemaLocation=\"..\\..\\Schema\\MasterManifestRequest.xsd\" xmlns:cm=\"http://bayan.gov.om/schema/common\" xmlns:tr=\"http://bayan.gov.om/schema/transport\" xmlns:tt=\"http://bayan.gov.om/schema/item\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:cmf=\"http://bayan.gov.om/schema/common-manifest\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
                    //sb.Insert(22, " xmlns:cm=\"http://bayan.gov.om/schema/common\" xmlns:cmf=\"http://bayan.gov.om/schema/common-manifest\" xmlns:tr=\"http://bayan.gov.om/schema/transport\" xmlns:tt=\"http://bayan.gov.om/schema/item\"");

                    //Remove the empty tags from XML
                    var document = System.Xml.Linq.XDocument.Parse(sb.ToString());
                    var emptyNodes = document.Descendants().Where(e => e.IsEmpty || String.IsNullOrWhiteSpace(e.Value));
                    foreach (var emptyNode in emptyNodes.ToArray())
                    {
                        emptyNode.Remove();
                    }
                    sb = new StringBuilder(document.ToString());
                    sb.Insert(0, "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                }
                return sb;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listType"></param>
        /// <returns></returns>
        public async Task<DataSet?> GetXMLMessageData(string listType)
        {
            try
            {
                //SQLServer da = new SQLServer();
                SqlParameter[] sqlParameter = new SqlParameter[] {
                          new SqlParameter("@ListType",listType)
                 };
                return await _readWriteDao.SelectRecords("uspGetXMLMessageData", sqlParameter);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        public async Task<DataSet?> SaveCustomMessageXML(string flightNo, DateTime flightDate, string customeMessageType, string xmlString, bool isHouse)
        {
            try
            {
                //SQLServer sqlServer = new SQLServer();

                SqlParameter[] sqlParameter = new SqlParameter[] {
                         new SqlParameter("@FltNumber",flightNo)
                         ,new SqlParameter("@FltDate",flightDate)
                         ,new SqlParameter("@CustomeMessageType",customeMessageType)
                         ,new SqlParameter("@Body",xmlString)
                         ,new SqlParameter("@IsHouse",isHouse)
                };

                //return sqlServer.SelectRecords("uspSaveCustomMessageXML", sqlParameter);
                return await _readWriteDao.SelectRecords("uspSaveCustomMessageXML", sqlParameter);

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        /// <summary>
        /// Generate XML string for OMAN customs using the AWB details
        /// </summary>
        /// <param name="dsAwbDetails">Data set contains AWB Details</param>
        /// <returns>XML string format</returns>
        public async Task<StringBuilder> GenerateXMLForOMForHouse(DataSet dsAwbDetails)
        {
            try
            {
                StringBuilder sb = new StringBuilder(string.Empty);

                //var xmlSchemaTable = await GetXMLMessageData("OMHouse").Tables[0];

                var xmlMsgRes = await GetXMLMessageData("OMHouse");

                var xmlSchemaTable = xmlMsgRes?.Tables[0];

                if (xmlSchemaTable != null && xmlSchemaTable.Rows.Count > 0)
                {
                    string strMessageXml = xmlSchemaTable.Rows[0]["XMLMessageData"].ToString();
                    string version = xmlSchemaTable.Rows[0]["Version"].ToString();
                    var nfmXmlDataSet = new DataSet();
                    var tx = new System.IO.StringReader(strMessageXml);
                    nfmXmlDataSet.ReadXml(tx);

                    #region : Assign Custom Message data to the XML tags :
                    //Message Header
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["MessageId"] = DateTime.Now.Year.ToString().Substring(2) + DateTime.Now.Month.ToString("00") + Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["SerialNumber"]);
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["ServiceID"] = Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["ServiceID"]);
                    //nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["SchemaVersionNo"] = Convert.ToString(dsAWBDetails.Tables[1].Rows[0]["SchemaVersionNo"]);
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["SchemaVersionNo"] = version;
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["CompanyCode"] = Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["CompanyCode"]);
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["UserId"] = Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["UserID"]);
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["MessageDate"] = Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["MessageDate"]);
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["MessageType"] = Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["MessageType"]).Trim();
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["VASCode"] = Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["VASCode"]).Trim();
                    nfmXmlDataSet.Tables["MessageHeader"].Rows[0]["ProcessingIndicator"] = Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["ProcessingIndicator"]).Trim();

                    //House Manifest Header
                    var inBoundOutBound = dsAwbDetails.Tables[2].Rows[0]["ManifestType"].ToString();
                    nfmXmlDataSet.Tables["HouseManifestHeader"].Rows[0]["manifestNo"] = dsAwbDetails.Tables[2].Rows[0]["ManifestNo"].ToString();
                    nfmXmlDataSet.Tables["HouseManifestHeader"].Rows[0]["masterTransDocNo"] = dsAwbDetails.Tables[2].Rows[0]["MasterTransDocNo"].ToString();
                    if (Convert.ToString(dsAwbDetails.Tables[1].Rows[0]["MessageType"]).Trim().ToUpper() == "A")
                    {
                        nfmXmlDataSet.Tables["HouseManifestHeader"].Rows[0]["houseManifestNo"] = string.Empty;//To be impliment    
                    }

                    //Transport Document
                    for (int i = 0; i < dsAwbDetails.Tables[3].Rows.Count; i++)
                    {
                        if (!(nfmXmlDataSet.Tables["TransportDocument"].Rows.Count > i))
                        {
                            //Add new row and set ParentID for TransportDocument Node
                            DataRow dr = nfmXmlDataSet.Tables["TransportDocument"].NewRow();
                            dr["TransportDocumentList_Id"] = 0;
                            dr["TransportDocument_Id"] = i;
                            nfmXmlDataSet.Tables["TransportDocument"].Rows.Add(dr);
                        }
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["sequenceNo"] = (i + 1).ToString();
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["transDocRefNo"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["TransDocRefNo"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["transDocIssueDate"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["TransDocIssueDate"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["transDocIndicator"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["TransDocIndicator"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["grossWeightManifested"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["GrossWeightManifested"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["grossWeightLoaded"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["GrossWeightLoaded"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["netWeightManifested"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NetWeightManifested"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["netWeightLoaded"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NetWeightLoaded"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["totalQuantity"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["TotalQuantity"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["totalQuantityUOM"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["TotalQuantityUOM"]).Trim();
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["isFinal"] = "Y";
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["isOwnershipTransferred"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["IsOwnershipTransferred"]) == "0" ? "N" : "Y";
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["UCRNo"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["UCRNo"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consigneeCustomsCode"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeCustomsCode"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consigneeName"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeName"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consigneeStreet"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeStreet"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consigneeCity"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeStreet"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consigneeSubRegion"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeSubRegion"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consigneeCountry"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneeCountry"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consigneePostCode"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsigneePostCode"]);
                        //this tag is not required for inbound cargo
                        if (inBoundOutBound.Trim() == "I")
                        {
                            nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consignorCustomsCode"] = string.Empty;
                        }
                        else
                        {
                            nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consignorCustomsCode"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorCustomsCode"]);
                        }
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consignorName"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorName"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consignorStreet"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorStreet"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consignorCity"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorCity"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consignorSubRegion"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorSubRegion"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consignorCountry"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ConsignorCountry"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["consignorPostCode"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["consignorPostCode"]);
                        //Sub - FW: Custom Message File for Export and Import - Attachment - Oman Air MM Final
                        //this tag is not required for inbound cargo
                        if (inBoundOutBound.Trim() == "I")
                        {
                            nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["notifyCustomsCode"] = string.Empty;
                        }
                        else
                        {
                            nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["notifyCustomsCode"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NotifyCustomsCode"]);
                        }

                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["notifyPartyName"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NotifyPartyName"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["notifyPartyStreet"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NotifyPartyStreet"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["notifyPartyCity"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NotifyPartyCity"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["notifyPartySubRegion"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NotifyPartySubRegion"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["notifyPartyCountry"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NotifyPartyCountry"]);
                        nfmXmlDataSet.Tables["TransportDocument"].Rows[i]["notifyPartyPostCode"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NotifyPartyPostCode"]);
                    }
                    //TransDocItem
                    for (int i = 0; i < dsAwbDetails.Tables[3].Rows.Count; i++)
                    {
                        if (!(nfmXmlDataSet.Tables["TransDocItem"].Rows.Count > i))
                        {
                            //Add new row and set ParentID for TransportDocItemList Node
                            DataRow drTranportDocItemList = nfmXmlDataSet.Tables["TranportDocItemList"].NewRow();
                            drTranportDocItemList["TransportDocument_Id"] = i;
                            nfmXmlDataSet.Tables["TranportDocItemList"].Rows.Add(drTranportDocItemList);

                            //Add new row and set ParentID for TransDocItem Node
                            DataRow drTransDocItem = nfmXmlDataSet.Tables["TransDocItem"].NewRow();
                            drTransDocItem["TranportDocItemList_Id"] = i;
                            nfmXmlDataSet.Tables["TransDocItem"].Rows.Add(drTransDocItem);
                        }
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["sequenceNo"] = "1";
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["itemDesc"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ItemDescription"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["marksAndNos"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["MarksAndNos"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["originCountry"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["OriginCountry"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["totalQuantity"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["TotalQuantityItem"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["totalQuantityUOM"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["ItemDescription"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["noOfPackages"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NoOfPackages"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["noOfPackagesUOM"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["NoOfPackagesUOM"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["grossWeight"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["GrossWeight"]);
                        nfmXmlDataSet.Tables["TransDocItem"].Rows[i]["undgClass"] = Convert.ToString(dsAwbDetails.Tables[3].Rows[i]["UndgClass"]);
                    }
                    #endregion

                    string strXml = nfmXmlDataSet.GetXml();
                    sb = new StringBuilder(strXml);

                    sb.Replace(" xmlns:cm=\"http://bayan.gov.om/schema/common\"", "");
                    sb.Replace(" xml:space=\"preserve\"", "");
                    sb.Replace(" xmlns:tr=\"http://bayan.gov.om/schema/transport\"", "");
                    sb.Replace(" xmlns:tt=\"http://bayan.gov.om/schema/item\"", "");
                    sb.Insert(21, " xmlns:cm=\"http://bayan.gov.om/schema/common\" xmlns:cmf=\"http://bayan.gov.om/schema/common-manifest\" xmlns:tr=\"http://bayan.gov.om/schema/transport\" xmlns:tt=\"http://bayan.gov.om/schema/item\"");

                    //Remove the empty tags from XML
                    var document = System.Xml.Linq.XDocument.Parse(sb.ToString());
                    var emptyNodes = document.Descendants().Where(e => e.IsEmpty || String.IsNullOrWhiteSpace(e.Value));
                    foreach (var emptyNode in emptyNodes.ToArray())
                    {
                        emptyNode.Remove();
                    }
                    sb = new StringBuilder(document.ToString());
                    sb.Insert(0, "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                }
                return sb;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        /// <summary>
        /// Added by prashant on 8 Jun 2016
        /// Method to get the country code using the Station Code
        /// </summary>
        /// <returns>Country Code</returns>
        public async Task<string?> GetCountryCode(string stationCode)
        {
            string countryCode = string.Empty;
            try
            {
                //SQLServer sqlServer = new SQLServer();

                SqlParameter[] sqlParameter = new SqlParameter[] {
                        new SqlParameter("@StationCode",stationCode)
                };

                //DataSet dsCountryCode = sqlServer.SelectRecords("uspGetCountryCode", sqlParameter);

                DataSet? dsCountryCode = await _readWriteDao.SelectRecords("uspGetCountryCode", sqlParameter);

                if (dsCountryCode != null)
                {
                    if (dsCountryCode.Tables.Count > 0 && dsCountryCode.Tables[0].Rows.Count > 0)
                    {
                        countryCode = dsCountryCode.Tables[0].Rows[0]["CountryCode"].ToString();
                    }
                }
                return countryCode;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        ///// <summary>
        ///// Method to get the XML message format
        ///// </summary>
        ///// <param name="listType"></param>
        ///// <returns></returns>
        //public DataSet GetXMLMessageData(string listType)
        //{
        //    DataSet dsGetXMLMessageData = new DataSet();
        //    try
        //    {
        //        SQLServer da = new SQLServer();
        //        SqlParameter[] sqlParameter = new SqlParameter[] {
        //              new SqlParameter("@ListType",listType)
        //     };
        //        dsGetXMLMessageData = da.SelectRecords("uspGetXMLMessageData", sqlParameter);
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //        dsGetXMLMessageData = null;
        //    }
        //    return dsGetXMLMessageData;
        //}
    }
}
