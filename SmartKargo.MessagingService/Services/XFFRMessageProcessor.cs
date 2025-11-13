#region XFFR Message Processor Class Description
/* XFFR Message Processor Class Description.
      * Company        :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright      :   Copyright © 2017 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose        : 
      * Created By     :   Yoginath
      * Created On     :   07/31/2017
      * Approved By    :
      * Approved Date  :
      * Modified By    :  
      * Modified On    :   
      * Description    :   
     */
#endregion
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Configuration;
using System.Data;
using System.Text;

namespace QidWorkerRole
{
    public class XFFRMessageProcessor
    {

        //#region :: Variable Declaration ::
        //SCMExceptionHandlingWorkRole scm = new SCMExceptionHandlingWorkRole();

        static string unloadingportsequence = string.Empty;
        static string uldsequencenum = string.Empty;
        static string awbref = string.Empty;
        const string PAGE_NAME = "cls_Encode_Decode";

        //#endregion Variable Declaration


        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<XFFRMessageProcessor> _logger;
        private GenericFunction _genericFunction;
        public XFFRMessageProcessor(
             ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<XFFRMessageProcessor> logger,
            GenericFunction genericFunction
        )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
        }

        #region :: Public Methods ::
        public bool DecodeFFRReceiveMessage(string ffrmsg, ref MessageData.ffrinfo data, ref MessageData.ULDinfo[] uld, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.FltRoute[] fltroute, ref MessageData.dimensionnfo[] objDimension)
        {
            const string FUN_NAME = "DecodeXFFRMessage";
            bool flag = false;
            string AWBPrefix = "", AWBNumber = "";
            var ffrXmlDataSet = new DataSet();

            var tx = new StringReader(ffrmsg);
            ffrXmlDataSet.ReadXml(tx);

            try
            {

                flag = true;
                data.ffrversionnum = Convert.ToString(ffrXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"]);

                //Decode consigment info
                DecodeConsigmentDetails(ffrXmlDataSet, ref consinfo, ref AWBPrefix, ref AWBNumber);

                //flight details
                MessageData.FltRoute flight = new MessageData.FltRoute("");
                flight.carriercode = Convert.ToString(ffrXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"]).Substring(0, 2);
                flight.fltnum = Convert.ToString(ffrXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"]).Substring(2);
                string sfltdate = Convert.ToString(ffrXmlDataSet.Tables["DepartureEvent"].Rows[0]["ScheduledOccurrenceDateTime"]);
                string[] fltDatesplit = sfltdate.Split('T');
                flight.date = Convert.ToString(fltDatesplit[0]);
                flight.month = Convert.ToDateTime(fltDatesplit[0]).ToString("MMM");
                if (ffrXmlDataSet.Tables.Contains("OccurrenceDepartureLocation"))
                {
                    if (ffrXmlDataSet.Tables.Contains("ID"))
                    {
                        DataRow[] drDestination = ffrXmlDataSet.Tables["ID"].Select("OccurrenceDepartureLocation_ID=0");
                        if (drDestination.Length > 0)
                        {
                            flight.fltdept = Convert.ToString(drDestination[0]["schemeID"]);
                        }
                    }
                }

                if (ffrXmlDataSet.Tables.Contains("OccurrenceArrivalLocation"))
                {
                    if (ffrXmlDataSet.Tables.Contains("ID"))
                    {
                        DataRow[] drDestination = ffrXmlDataSet.Tables["ID"].Select("OccurrenceArrivalLocation_ID=0");
                        if (drDestination.Length > 0)
                        {
                            flight.fltarrival = Convert.ToString(drDestination[0]["schemeID"]);
                        }
                    }
                }

                flight.spaceallotmentcode = Convert.ToString(ffrXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["SpaceAllocationcode"]);
                flight.allotidentification = Convert.ToString(ffrXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["AllotmentID"]);
                Array.Resize(ref fltroute, fltroute.Length + 1);
                fltroute[fltroute.Length - 1] = flight;

                ////ULD Specification
                //int uldnum = 0;
                ////data.noofuld = msg[1];
                //uld[uldnum].uldtype = Convert.ToString(ffrXmlDataSet.Tables["AssociatedUnitLoadTransportEquipment"].Rows[0]["CharacteristicCode"]);
                //uld[uldnum].uldsrno = Convert.ToString(ffrXmlDataSet.Tables["AssociatedUnitLoadTransportEquipment"].Rows[0]["ID"]);
                //if (ffrXmlDataSet.Tables.Contains("PrimaryID"))
                //{
                //    DataRow[] drGross = ffrXmlDataSet.Tables["PrimaryID"].Select("OperatingParty_ID=0");
                //    if (drGross.Length > 0)
                //    {
                //        uld[uldnum].uldowner = Convert.ToString(drGross[0]["schemeAgencyID"]);
                //    }
                //}
                //uld[uldnum].uldloadingindicator = Convert.ToString(ffrXmlDataSet.Tables["LoadingInstructions"].Rows[0]["DescriptionCode"]);
                //if (ffrXmlDataSet.Tables.Contains("AssociatedUnitLoadTransportEquipment_GrossWeightMeasure"))
                //{
                //    DataRow[] drGross = ffrXmlDataSet.Tables["AssociatedUnitLoadTransportEquipment_GrossWeightMeasure"].Select("AssociatedUnitLoadTransportEquipment_ID=0");
                //    if (drGross.Length > 0)
                //    {
                //        uld[uldnum].uldweightcode = Convert.ToString(drGross[0]["unitCode"]);
                //        uld[uldnum].uldweight = Convert.ToString(drGross[0]["AssociatedUnitLoadTransportEquipment_GrossWeightMeasure_Text"]);
                //    }
                //}

                //Special Service request
                //lastrec = msg[0];
                //line = 0;
                data.specialservicereq1 = Convert.ToString(ffrXmlDataSet.Tables["HandlingSSRInstructions"].Rows[0]["Description"]);

                //Other service info
                //lastrec = msg[0];
                //line = 0;
                data.otherserviceinfo1 = Convert.ToString(ffrXmlDataSet.Tables["HandlingOSIInstructions"].Rows[0]["Description"]);

                //booking reference
                //data.bookingrefairport = msg[1].Substring(0, 3);
                //data.officefundesignation = msg[1].Substring(3, 2);
                //data.companydesignator = msg[1].Substring(5, 2);
                //data.participentidetifier = msg[2].Length > 0 ? msg[2] : null;
                //data.participentcode = msg[3].Length > 0 ? msg[3] : null;
                //data.participentairportcity = msg[4].Length > 0 ? msg[4] : null;


                //Dimendion info                
                MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
                if (ffrXmlDataSet.Tables.Contains("TransportLogisticsPackage_GrossWeightMeasure"))
                {
                    DataRow[] drGross = ffrXmlDataSet.Tables["TransportLogisticsPackage_GrossWeightMeasure"].Select("TransportLogisticsPackage_ID=0");
                    if (drGross.Length > 0)
                    {
                        dimension.weightcode = Convert.ToString(drGross[0]["unitCode"]);
                        dimension.weight = Convert.ToString(drGross[0]["TransportLogisticsPackage_GrossWeightMeasure_Text"]);
                    }
                }

                if (ffrXmlDataSet.Tables.Contains("WidthMeasure"))
                {
                    if (ffrXmlDataSet.Tables["WidthMeasure"].Columns.Contains("unitCode"))
                        dimension.mesurunitcode = Convert.ToString(ffrXmlDataSet.Tables["WidthMeasure"].Rows[0]["unitCode"]);

                    if (ffrXmlDataSet.Tables["WidthMeasure"].Columns.Contains("WidthMeasure_Text"))
                        dimension.width = Convert.ToString(ffrXmlDataSet.Tables["WidthMeasure"].Rows[0]["WidthMeasure_Text"]);
                }

                if (ffrXmlDataSet.Tables.Contains("LengthMeasure"))
                {
                    if (ffrXmlDataSet.Tables["LengthMeasure"].Columns.Contains("unitCode"))
                        dimension.mesurunitcode = Convert.ToString(ffrXmlDataSet.Tables["LengthMeasure"].Rows[0]["unitCode"]);

                    if (ffrXmlDataSet.Tables["LengthMeasure"].Columns.Contains("LengthMeasure_Text"))
                        dimension.length = Convert.ToString(ffrXmlDataSet.Tables["LengthMeasure"].Rows[0]["LengthMeasure_Text"]);
                }

                if (ffrXmlDataSet.Tables.Contains("HeightMeasure"))
                {
                    if (ffrXmlDataSet.Tables["HeightMeasure"].Columns.Contains("unitCode"))
                        dimension.mesurunitcode = Convert.ToString(ffrXmlDataSet.Tables["HeightMeasure"].Rows[0]["unitCode"]);

                    if (ffrXmlDataSet.Tables["HeightMeasure"].Columns.Contains("HeightMeasure_Text"))
                        dimension.height = Convert.ToString(ffrXmlDataSet.Tables["HeightMeasure"].Rows[0]["HeightMeasure_Text"]);
                }

                dimension.piecenum = Convert.ToString(ffrXmlDataSet.Tables["TransportLogisticsPackage"].Rows[0]["ItemQuantity"]);
                Array.Resize(ref objDimension, objDimension.Length + 1);
                objDimension[objDimension.Length - 1] = dimension;

                // Product information
                if (ffrXmlDataSet.Tables.Contains("ApplicableLogisticsServiceCharge"))
                {
                    data.servicecode = Convert.ToString(ffrXmlDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows[0]["ServiceTypeCode"]);
                }
                if (ffrXmlDataSet.Tables.Contains("ApplicableFreightRateServiceCharge"))
                {
                    data.rateclasscode = Convert.ToString(ffrXmlDataSet.Tables["ApplicableFreightRateServiceCharge"].Rows[0]["CategoryCode"]);
                    data.commoditycode = Convert.ToString(ffrXmlDataSet.Tables["ApplicableFreightRateServiceCharge"].Rows[0]["CommodityItemID"]);
                }

                //Shipper Infor
                //lastrec = msg[0];
                //line = 0;
                data.shipperaccnum = Convert.ToString(ffrXmlDataSet.Tables["ConsignorParty"].Rows[0]["AccountID"]);
                data.shippername = Convert.ToString(ffrXmlDataSet.Tables["ConsignorParty"].Rows[0]["Name"]).Length > 0 ? Convert.ToString(ffrXmlDataSet.Tables["ConsignorParty"].Rows[0]["Name"]) : "";
                data.shipperadd = Convert.ToString(ffrXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["StreetName"]).Length > 0 ? Convert.ToString(ffrXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["StreetName"]) : "";
                data.shipperplace = Convert.ToString(ffrXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CityName"]).Length > 0 ? Convert.ToString(ffrXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CityName"]) : "";
                data.shipperstate = Convert.ToString(ffrXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountrySubDivisionID"]).Length > 0 ? Convert.ToString(ffrXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountrySubDivisionID"]) : "";
                data.shippercountrycode = Convert.ToString(ffrXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountryID"]).Length > 0 ? Convert.ToString(ffrXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountryID"]) : "";
                data.shipperpostcode = Convert.ToString(ffrXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["PostOfficeBox"]).Length > 0 ? Convert.ToString(ffrXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["PostOfficeBox"]) : "";

                if (ffrXmlDataSet.Tables.Contains("DirectTelephoneCommunication"))
                {
                    if (ffrXmlDataSet.Tables["DirectTelephoneCommunication"].Columns.Contains("CompleteNumber"))
                    {
                        DataRow[] drCompleteNumber = ffrXmlDataSet.Tables["DirectTelephoneCommunication"].Select("DefinedTradeContact_ID=0");
                        if (drCompleteNumber.Length > 0)
                        {
                            data.shippercontactidentifier = "TE";
                            data.shippercontactnum = Convert.ToString(drCompleteNumber[0]["CompleteNumber"]);
                        }
                    }
                }

                if (ffrXmlDataSet.Tables.Contains("FaxCommunication"))
                {
                    if (ffrXmlDataSet.Tables["FaxCommunication"].Columns.Contains("CompleteNumber"))
                    {
                        DataRow[] drFaxCommunication = ffrXmlDataSet.Tables["DirectTelephoneCommunication"].Select("DefinedTradeContact_ID=0");
                        if (drFaxCommunication.Length > 0)
                        {
                            data.shippercontactidentifier = "FX";
                            data.shippercontactnum = Convert.ToString(drFaxCommunication[0]["CompleteNumber"]);
                        }
                    }
                }
                if (ffrXmlDataSet.Tables.Contains("TelexCommunication"))
                {
                    if (ffrXmlDataSet.Tables["TelexCommunication"].Columns.Contains("CompleteNumber"))
                    {
                        DataRow[] drTelex = ffrXmlDataSet.Tables["TelexCommunication"].Select("DefinedTradeContact_ID=0");
                        if (drTelex.Length > 0)
                        {
                            data.shippercontactidentifier = "TL";
                            data.shippercontactnum = Convert.ToString(drTelex[0]["CompleteNumber"]);
                        }
                    }
                }

                // Consignee
                //lastrec = msg[0];
                //line = 0;
                data.consaccnum = Convert.ToString(ffrXmlDataSet.Tables["ConsigneeParty"].Rows[0]["AccountID"]);
                data.consname = Convert.ToString(ffrXmlDataSet.Tables["ConsigneeParty"].Rows[0]["Name"]).Length > 0 ? Convert.ToString(ffrXmlDataSet.Tables["ConsigneeParty"].Rows[0]["Name"]) : "";
                data.consadd = Convert.ToString(ffrXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["StreetName"]).Length > 0 ? Convert.ToString(ffrXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["StreetName"]) : "";
                data.consplace = Convert.ToString(ffrXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CityName"]).Length > 0 ? Convert.ToString(ffrXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CityName"]) : "";
                data.consstate = Convert.ToString(ffrXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountrySubDivisionID"]).Length > 0 ? Convert.ToString(ffrXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountrySubDivisionID"]) : "";
                data.conscountrycode = Convert.ToString(ffrXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountryID"]).Length > 0 ? Convert.ToString(ffrXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountryID"]) : "";
                data.conspostcode = Convert.ToString(ffrXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["PostOfficeBox"]).Length > 0 ? Convert.ToString(ffrXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["PostOfficeBox"]) : "";
                if (ffrXmlDataSet.Tables.Contains("DirectTelephoneCommunication"))
                {
                    if (ffrXmlDataSet.Tables["DirectTelephoneCommunication"].Columns.Contains("CompleteNumber"))
                    {
                        DataRow[] drConsigneeNumber = ffrXmlDataSet.Tables["DirectTelephoneCommunication"].Select("DefinedTradeContact_ID=1");
                        if (drConsigneeNumber.Length > 0)
                        {
                            data.conscontactidentifier = "TE";
                            data.conscontactnum = Convert.ToString(drConsigneeNumber[0]["CompleteNumber"]);
                        }
                    }
                }

                if (ffrXmlDataSet.Tables.Contains("FaxCommunication"))
                {
                    if (ffrXmlDataSet.Tables["FaxCommunication"].Columns.Contains("CompleteNumber"))
                    {
                        DataRow[] drFaxCommunication = ffrXmlDataSet.Tables["FaxCommunication"].Select("DefinedTradeContact_ID=1");
                        if (drFaxCommunication.Length > 0)
                        {
                            data.conscontactidentifier = "FX";
                            data.conscontactnum = Convert.ToString(drFaxCommunication[0]["CompleteNumber"]);
                        }
                    }
                }
                if (ffrXmlDataSet.Tables.Contains("TelexCommunication"))
                {
                    if (ffrXmlDataSet.Tables["TelexCommunication"].Columns.Contains("CompleteNumber"))
                    {
                        DataRow[] drTelex = ffrXmlDataSet.Tables["TelexCommunication"].Select("DefinedTradeContact_ID=1");
                        if (drTelex.Length > 0)
                        {
                            data.conscontactidentifier = "TL";
                            data.conscontactnum = Convert.ToString(drTelex[0]["CompleteNumber"]);
                        }
                    }
                }

                // Customer Identification
                //lastrec = msg[0];
                data.custaccnum = Convert.ToString(ffrXmlDataSet.Tables["RequestorParty"].Rows[0]["AccountID"]).Length > 0 ? Convert.ToString(ffrXmlDataSet.Tables["RequestorParty"].Rows[0]["AccountID"]) : "";
                data.iatacargoagentcode = Convert.ToString(ffrXmlDataSet.Tables["RequestorParty"].Rows[0]["CargoAgentID"]).Length > 0 ? Convert.ToString(ffrXmlDataSet.Tables["RequestorParty"].Rows[0]["CargoAgentID"]) : "";
                DataRow[] drCargoLocation = ffrXmlDataSet.Tables["SpecifiedCargoAgentLocation"].Select("RequestorParty_ID=0");
                if (drCargoLocation.Length > 0)
                    data.cargoagentcasscode = Convert.ToString(drCargoLocation[0]["ID"]).Length > 0 ? Convert.ToString(drCargoLocation[0]["ID"]) : "";
                data.participentidetifier = "";
                data.custname = Convert.ToString(ffrXmlDataSet.Tables["RequestorParty"].Rows[0]["Name"]).Length > 0 ? Convert.ToString(ffrXmlDataSet.Tables["RequestorParty"].Rows[0]["Name"]) : "";
                data.custplace = Convert.ToString(ffrXmlDataSet.Tables["RequestorParty_PostalStructuredAddress"].Rows[0]["CityName"]).Length > 0 ? Convert.ToString(ffrXmlDataSet.Tables["RequestorParty_PostalStructuredAddress"].Rows[0]["CityName"]) : "";

                //shipment refence info
                //data.shiprefnum = msg[1].Length > 0 ? msg[1] : null;
                //data.supplemetryshipperinfo1 = msg[2].Length > 0 ? msg[2] : null;
                //data.supplemetryshipperinfo2 = msg[3].Length > 0 ? msg[3] : null;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
            }
            return flag;
        }

        public void DecodeConsigmentDetails(DataSet ffrXmlDataSet, ref MessageData.consignmnetinfo[] consinfo, ref string awbprefix, ref string awbnumber)
        {
            const string FUN_NAME = "Decodeconsigmentdetails";
            try
            {
                MessageData.consignmnetinfo consig = new MessageData.consignmnetinfo("");
                //consinfo[num] = new MessageData.consignmnetinfo("");

                string[] arrawb = Convert.ToString(ffrXmlDataSet.Tables["BusinessHeaderDocument"].Rows[0]["ID"]).Split('-');
                consig.airlineprefix = Convert.ToString(arrawb[0]);
                consig.awbnum = Convert.ToString(arrawb[1]);
                consig.origin = Convert.ToString(ffrXmlDataSet.Tables["OriginLocation"].Rows[0]["ID"]);
                consig.dest = Convert.ToString(ffrXmlDataSet.Tables["FinalDestinationLocation"].Rows[0]["ID"]);

                consig.consigntype = Convert.ToString(ffrXmlDataSet.Tables["MasterConsignment"].Rows[0]["ShipmentTypeCode"]);
                consig.pcscnt = Convert.ToString(ffrXmlDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"]);
                if (Convert.ToString(ffrXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Rows[0]["UnitCode"]).Equals("KGM"))
                {
                    consig.weightcode = "K";
                }
                else
                {
                    consig.weightcode = "L";
                }
                consig.weight = Convert.ToString(ffrXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Rows[0]["IncludedTareGrossWeightMeasure_Text"]);

                if (consig.consigntype.Equals("T"))
                {
                    //total pieces
                    consig.numshp = consig.pcscnt;
                    consig.shpdesccode = consig.consigntype;
                    //consig.numshp = strarr[k + 1];     
                }

                //consig.densityindicator = strarr[k];
                consig.densitygrp = Convert.ToString(ffrXmlDataSet.Tables["MasterConsignment"].Rows[0]["DensityGroupCode"]);

                consig.volumecode = Convert.ToString(ffrXmlDataSet.Tables["GrossVolumeMeasure"].Rows[0]["UnitCode"]);
                consig.volumeamt = Convert.ToString(ffrXmlDataSet.Tables["GrossVolumeMeasure"].Rows[0]["GrossVolumeMeasure_Text"]);

                //consig.manifestdesc = msg[2];
                consig.splhandling = Convert.ToString(ffrXmlDataSet.Tables["HandlingSPHInstructions"].Rows[0]["Description"]);
                //for (int j = 3; j < msg.Length; j++)
                //    consig.splhandling = consig.splhandling + msg[j] + ",";

                if (unloadingportsequence.Length > 0)
                    consig.portsequence = unloadingportsequence;
                if (uldsequencenum.Length > 0)
                    consig.uldsequence = uldsequencenum;

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

        //public async Task<bool> ValidaeSaveFFRMessage(MessageData.ffrinfo objFFRData, MessageData.consignmnetinfo[] objConsInfo, MessageData.FltRoute[] objRouteInfo, MessageData.dimensionnfo[] objDimension, int RefNo, string strFFRMessage, string strMessageFrom, string strFromID, string strStatus, out string ErrorMsg)
        public async Task<(bool success, string ErrorMsg)> ValidaeSaveFFRMessage(MessageData.ffrinfo objFFRData, MessageData.consignmnetinfo[] objConsInfo, MessageData.FltRoute[] objRouteInfo, MessageData.dimensionnfo[] objDimension, int RefNo, string strFFRMessage, string strMessageFrom, string strFromID, string strStatus, string ErrorMsg)
        {
            string conStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
            bool flag = false;
            string awbnum = string.Empty, AWBPrefix = string.Empty, strErrorMessage = string.Empty, strAWBOrigin = string.Empty, strAWBDestination = string.Empty, strFlightNo = string.Empty, strFlightOrigin = string.Empty, strFlightDestination = string.Empty;
            int awbPcs = 0;
            decimal awbWeight = 0;
            XFNMMessageProcessor xfnmMessageProcessor = new XFNMMessageProcessor();
            ErrorMsg = string.Empty;
            try
            {
                //AuditLog log = new AuditLog();

                //SQLServer dtb = new SQLServer();

                if (objConsInfo.Length > 0)
                {
                    for (int i = 0; i < objConsInfo.Length; i++)
                    {


                        awbnum = objConsInfo[i].awbnum;
                        AWBPrefix = objConsInfo[i].airlineprefix;
                        strAWBOrigin = objConsInfo[i].origin;
                        strAWBDestination = objConsInfo[i].dest;
                        awbPcs = int.Parse(objConsInfo[i].pcscnt);
                        awbWeight = Convert.ToDecimal(objConsInfo[i].weight);

                        //GenericFunction gf = new GenericFunction();

                        await _genericFunction.UpdateInboxFromMessageParameter(RefNo, AWBPrefix + "-" + awbnum, string.Empty, string.Empty, string.Empty, "FFR", "FFR", DateTime.Parse("1900-01-01"));

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
                            //strErrorMessage = "AWB Pieces should be greater than Zero";
                            //string[] QueryNames = { "ErrorMessage", "MessageSNo" };
                            //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.Int };
                            //object[] QueryValues = { strErrorMessage, RefNo };
                            //if (dtb.UpdateData("spUpdateStatusformessage", QueryNames, QueryTypes, QueryValues))
                            //    xfnmMessageProcessor.GenerateXFNMMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, "", "", "", "XFFR");
                            //else
                            ErrorMsg = "AWB Pieces should be greater than Zero";
                            // xfnmMessageProcessor.GenerateXFNMMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, "", "", "", "XFFR");

                            //strErrorMessage = string.Empty;

                            //return flag = false;
                            flag = false;
                            return (flag, ErrorMsg);
                        }

                        if (objConsInfo[i].weight == "" || objConsInfo[i].weight == "0")
                        {
                            //strErrorMessage = "AWB GrossWeight  should be greater than Zero";
                            //string[] QueryNames = { "ErrorMessage", "MessageSNo" };
                            //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.Int };
                            //object[] QueryValues = { strErrorMessage, RefNo };
                            //if (dtb.UpdateData("spUpdateStatusformessage", QueryNames, QueryTypes, QueryValues))
                            //    xfnmMessageProcessor.GenerateXFNMMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, "", "", "", "XFFR");
                            //else
                            ErrorMsg = "AWB GrossWeight  should be greater than Zero";
                            //xfnmMessageProcessor.GenerateXFNMMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, "", "", "", "XFFR");
                            //strErrorMessage = string.Empty;
                            //return flag = false;
                            flag = false;
                            return (flag, ErrorMsg);
                        }



                        DataSet? dsawb = new DataSet();
                        dsawb = await CheckValidateFFRMessage(AWBPrefix, awbnum, strAWBOrigin, strAWBDestination, "FFR");
                        if (dsawb != null && dsawb.Tables.Count > 0 && dsawb.Tables[0].Rows.Count > 0)
                        {
                            //strErrorMessage = dsawb.Tables[0].Rows[0]["ErrorMessage"].ToString();
                            //string[] QueryNames = { "ErrorMessage", "MessageSNo" };
                            //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.Int };
                            //object[] QueryValues = { strErrorMessage, RefNo };

                            //if (dtb.UpdateData("spUpdateStatusformessage", QueryNames, QueryTypes, QueryValues))
                            //    xfnmMessageProcessor.GenerateXFNMMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, "", "", "", "XFFR");
                            //else
                            ErrorMsg = dsawb.Tables[0].Rows[0]["ErrorMessage"].ToString();
                            //xfnmMessageProcessor.GenerateXFNMMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, "", "", "", "XFFR");
                            //strErrorMessage = string.Empty;
                            //return flag = false;

                            flag = false;
                            return (flag, ErrorMsg);
                        }


                        bool originMatch = false, destinationMatch = false;
                        if (objRouteInfo.Length > 0)
                        {
                            for (int s = 0; s < objRouteInfo.Length; s++)
                            {

                                string FlightOrigin = objRouteInfo[s].fltdept;
                                string FlightDestination = objRouteInfo[s].fltarrival;
                                if (FlightOrigin == strAWBOrigin)
                                    originMatch = true;
                                if (FlightDestination == strAWBDestination)
                                    destinationMatch = true;
                                if (originMatch && destinationMatch)
                                    break;
                            }
                            if (!(originMatch && destinationMatch))
                            {
                                //strErrorMessage = "Flight Routing is  not Complete.FFR message  Rejected For " + awbnum;
                                //string[] QueryNames = { "ErrorMessage", "MessageSNo" };
                                //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.Int };
                                //object[] QueryValues = { strErrorMessage, RefNo };

                                //if (dtb.UpdateData("spUpdateStatusformessage", QueryNames, QueryTypes, QueryValues))
                                //    xfnmMessageProcessor.GenerateXFNMMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, "", "", "", "XFFR");
                                //else
                                ErrorMsg = "Flight Routing is  not Complete.FFR message  Rejected For " + awbnum;
                                // xfnmMessageProcessor.GenerateXFNMMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, "", "", "", "XFFR");
                                //return flag = false;

                                flag = false;
                                return (flag, ErrorMsg);

                            }
                        }
                        else
                        {
                            //strErrorMessage = "Wrong Message format of FFR Message.FFR Message   Rejected For " + awbnum;
                            //string[] QueryNames = { "ErrorMessage", "MessageSNo" };
                            //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.Int };
                            //object[] QueryValues = { strErrorMessage, RefNo };

                            //if (dtb.UpdateData("spUpdateStatusformessage", QueryNames, QueryTypes, QueryValues))
                            //    xfnmMessageProcessor.GenerateXFNMMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, "", "", "", "XFFR");
                            //else
                            ErrorMsg = "Wrong Message format of FFR Message.FFR Message   Rejected For " + awbnum;
                            //xfnmMessageProcessor.GenerateXFNMMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, "", "", "", "XFFR");
                            //return flag = false;

                            flag = false;
                            return (flag, ErrorMsg);
                        }



                        //InBox inBox = null;
                        //inBox = new InBox();
                        //inBox.Subject = "FFR";
                        //inBox.Body = strFFRMessage;
                        //inBox.FromiD = string.Empty;
                        //inBox.ToiD = string.Empty;
                        //inBox.RecievedOn = DateTime.Now;
                        //inBox.IsProcessed = true;
                        ////inBox.Status = "ReProcessed";
                        //inBox.Status = strStatus;
                        //inBox.FromiD = strFromID;
                        //inBox.Type = "FFR";
                        //inBox.UpdatedBy = strMessageFrom;
                        //inBox.UpdatedOn = DateTime.Now;
                        //inBox.AWBNumber = AWBPrefix + '-' + awbnum;
                        //inBox.FlightNumber = string.Empty;
                        //inBox.FlightOrigin = string.Empty;
                        //inBox.FlightDestination = string.Empty;
                        //inBox.FlightDate = DateTime.Now;
                        //inBox.MessageCategory = "CIMP";
                        //inBox.Error = strErrorMessage;
                        //log.SaveLog(LogType.InMessage, string.Empty, string.Empty, inBox);

                        //string[] paramname = new string[] { "AirlinePrefix", "AWBNum", "Origin", "Dest", "PcsCount", "Weight", "Volume", "ComodityCode", "ComodityDesc", "CarrierCode", "FlightNum", "FlightDate", "FlightOrigin", "FlightDest", "ShipperName", "ShipperAddr", "ShipperPlace", "ShipperState", "ShipperCountryCode", "ShipperContactNo", "ConsName", "ConsAddr", "ConsPlace", "ConsState", "ConsCountryCode", "ConsContactNo", "CustAccNo", "IATACargoAgentCode", "CustName", "SystemDate", "MeasureUnit", "Length", "Breadth", "Height", "PartnerStatus", "REFNo", "UpdatedBy", "SpecialHandelingCode", "WeightCode", "ChargeableWeight", "ShipperPincode", "ConsingneePinCode" };

                        //object[] paramvalue = new object[] { objConsInfo[i].airlineprefix, objConsInfo[i].awbnum, objConsInfo[i].origin, objConsInfo[i].dest, objConsInfo[i].pcscnt, objConsInfo[i].weight, objConsInfo[i].volumeamt, objConsInfo[i].commodity, objConsInfo[i].manifestdesc, "", objRouteInfo[0].carriercode + objRouteInfo[0].fltnum, "", "", "", objFFRData.shippername, objFFRData.shipperadd.Trim(','), objFFRData.shipperplace.Trim(','), objFFRData.shipperstate, objFFRData.shippercountrycode, objFFRData.shippercontactnum, objFFRData.consname, objFFRData.consadd.Trim(','), objFFRData.consplace.Trim(','), objFFRData.consstate, objFFRData.conscountrycode, objFFRData.conscontactnum, objFFRData.custaccnum, objFFRData.iatacargoagentcode, objFFRData.custname, System.DateTime.Now.ToString("yyyy-MM-dd"), "", "", "", "", "", RefNo, "FFR", objConsInfo[i].splhandling, objConsInfo[i].weightcode, ChargeableWeight, objFFRData.shipperpostcode, objFFRData.conspostcode };

                        //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Decimal, SqlDbType.VarChar, SqlDbType.VarChar };

                        SqlParameter[] sqlParams = new SqlParameter[]
                        {
                            new SqlParameter("@AirlinePrefix", SqlDbType.VarChar) { Value = objConsInfo[i].airlineprefix },
                            new SqlParameter("@AWBNum", SqlDbType.VarChar) { Value = objConsInfo[i].awbnum },
                            new SqlParameter("@Origin", SqlDbType.VarChar) { Value = objConsInfo[i].origin },
                            new SqlParameter("@Dest", SqlDbType.VarChar) { Value = objConsInfo[i].dest },
                            new SqlParameter("@PcsCount", SqlDbType.VarChar) { Value = objConsInfo[i].pcscnt },
                            new SqlParameter("@Weight", SqlDbType.VarChar) { Value = objConsInfo[i].weight },
                            new SqlParameter("@Volume", SqlDbType.VarChar) { Value = objConsInfo[i].volumeamt },
                            new SqlParameter("@ComodityCode", SqlDbType.VarChar) { Value = objConsInfo[i].commodity },
                            new SqlParameter("@ComodityDesc", SqlDbType.VarChar) { Value = objConsInfo[i].manifestdesc },
                            new SqlParameter("@CarrierCode", SqlDbType.VarChar) { Value = "" },
                            new SqlParameter("@FlightNum", SqlDbType.VarChar) { Value = objRouteInfo[0].carriercode + objRouteInfo[0].fltnum },
                            new SqlParameter("@FlightDate", SqlDbType.VarChar) { Value = "" },
                            new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = "" },
                            new SqlParameter("@FlightDest", SqlDbType.VarChar) { Value = "" },
                            new SqlParameter("@ShipperName", SqlDbType.VarChar) { Value = objFFRData.shippername },
                            new SqlParameter("@ShipperAddr", SqlDbType.VarChar) { Value = objFFRData.shipperadd.Trim(',') },
                            new SqlParameter("@ShipperPlace", SqlDbType.VarChar) { Value = objFFRData.shipperplace.Trim(',') },
                            new SqlParameter("@ShipperState", SqlDbType.VarChar) { Value = objFFRData.shipperstate },
                            new SqlParameter("@ShipperCountryCode", SqlDbType.VarChar) { Value = objFFRData.shippercountrycode },
                            new SqlParameter("@ShipperContactNo", SqlDbType.VarChar) { Value = objFFRData.shippercontactnum },
                            new SqlParameter("@ConsName", SqlDbType.VarChar) { Value = objFFRData.consname },
                            new SqlParameter("@ConsAddr", SqlDbType.VarChar) { Value = objFFRData.consadd.Trim(',') },
                            new SqlParameter("@ConsPlace", SqlDbType.VarChar) { Value = objFFRData.consplace.Trim(',') },
                            new SqlParameter("@ConsState", SqlDbType.VarChar) { Value = objFFRData.consstate },
                            new SqlParameter("@ConsCountryCode", SqlDbType.VarChar) { Value = objFFRData.conscountrycode },
                            new SqlParameter("@ConsContactNo", SqlDbType.VarChar) { Value = objFFRData.conscontactnum },
                            new SqlParameter("@CustAccNo", SqlDbType.VarChar) { Value = objFFRData.custaccnum },
                            new SqlParameter("@IATACargoAgentCode", SqlDbType.VarChar) { Value = objFFRData.iatacargoagentcode },
                            new SqlParameter("@CustName", SqlDbType.VarChar) { Value = objFFRData.custname },
                            new SqlParameter("@SystemDate", SqlDbType.DateTime) { Value = DateTime.Now.ToString("yyyy-MM-dd") },
                            new SqlParameter("@MeasureUnit", SqlDbType.VarChar) { Value = "" },
                            new SqlParameter("@Length", SqlDbType.VarChar) { Value = "" },
                            new SqlParameter("@Breadth", SqlDbType.VarChar) { Value = "" },
                            new SqlParameter("@Height", SqlDbType.VarChar) { Value = "" },
                            new SqlParameter("@PartnerStatus", SqlDbType.VarChar) { Value = "" },
                            new SqlParameter("@REFNo", SqlDbType.Int) { Value = RefNo },
                            new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FFR" },
                            new SqlParameter("@SpecialHandelingCode", SqlDbType.VarChar) { Value = objConsInfo[i].splhandling },
                            new SqlParameter("@WeightCode", SqlDbType.VarChar) { Value = objConsInfo[i].weightcode },
                            new SqlParameter("@ChargeableWeight", SqlDbType.Decimal) { Value = ChargeableWeight },
                            new SqlParameter("@ShipperPincode", SqlDbType.VarChar) { Value = objFFRData.shipperpostcode },
                            new SqlParameter("@ConsingneePinCode", SqlDbType.VarChar) { Value = objFFRData.conspostcode }
                        };


                        string procedure = "spInsertBookingDataFromFFR";

                        //flag = dtb.InsertData(procedure, paramname, paramtype, paramvalue);
                        flag = await _readWriteDao.ExecuteNonQueryAsync(procedure, sqlParams);

                    }
                }
                string FlightDate = string.Empty;

                if (flag)
                {

                    #region Save AWBNo on Audit Log

                    DateTime flightDate = DateTime.UtcNow;
                    string flightdate = DateTime.Now.ToString("dd/MM/yyyy");
                    if (objRouteInfo.Length > 0)
                    {
                        strFlightNo = objRouteInfo[0].carriercode + objRouteInfo[0].fltnum;
                        strFlightOrigin = objRouteInfo[0].fltdept;
                        strFlightDestination = objRouteInfo[0].fltarrival;

                        if (objRouteInfo[0].date != "")
                            //flightdate = objRouteInfo[0].date + "/" + DateTime.Now.ToString("MM/yyyy");
                            flightdate = Convert.ToDateTime(objRouteInfo[0].date).ToString("dd/MM/yyyy");
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

                    //objOpsAuditLog = new AWBOperations();
                    //objOpsAuditLog.AWBID = 0;
                    //objOpsAuditLog.AWBPrefix = AWBPrefix.Trim();
                    //objOpsAuditLog.AWBNumber = awbnum;
                    //objOpsAuditLog.Origin = objConsInfo[0].origin.ToUpper();
                    //objOpsAuditLog.Destination = objConsInfo[0].dest.ToUpper();
                    //objOpsAuditLog.FlightNo = strFlightNo;
                    //objOpsAuditLog.FlightDate = flightDate;
                    //objOpsAuditLog.FlightOrigin = string.Empty;
                    //objOpsAuditLog.FlightDestination = string.Empty;
                    //objOpsAuditLog.BookedPcs = Convert.ToInt32(objConsInfo[0].pcscnt);
                    //objOpsAuditLog.BookedWgt = Convert.ToDouble(objConsInfo[0].weight);
                    //objOpsAuditLog.UOM = objConsInfo[0].weightcode;
                    //objOpsAuditLog.Createdon = DateTime.UtcNow;
                    //objOpsAuditLog.Updatedon = DateTime.UtcNow;
                    //objOpsAuditLog.Createdby = strMessageFrom;
                    //objOpsAuditLog.Updatedby = strMessageFrom;
                    //objOpsAuditLog.Action = "Booked";
                    //objOpsAuditLog.Message = "AWB Booked Through FFR";
                    //objOpsAuditLog.Description = objConsInfo[0].manifestdesc;
                    //log = new AuditLog();
                    //log.SaveLog(LogType.AWBOperations, string.Empty, string.Empty, objOpsAuditLog);

                    //string[] CNname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public" };
                    //SqlDbType[] CType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit };
                    //object[] CValues = new object[] { objConsInfo[0].airlineprefix, objConsInfo[0].awbnum, objConsInfo[0].origin, objConsInfo[0].dest, objConsInfo[0].pcscnt, objConsInfo[0].weight, strFlightNo, flightDate, strFlightOrigin, strFlightDestination, "Booked", "FFR", "AWB Booked Through FFR Message", "FFR", DateTime.Now.ToString(), 1 };

                    SqlParameter[] sqlParamsAudit = new SqlParameter[]
                    {
                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = objConsInfo[0].airlineprefix },
                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = objConsInfo[0].awbnum },
                        new SqlParameter("@Origin", SqlDbType.VarChar) { Value = objConsInfo[0].origin },
                        new SqlParameter("@Destination", SqlDbType.VarChar) { Value = objConsInfo[0].dest },
                        new SqlParameter("@Pieces", SqlDbType.VarChar) { Value = objConsInfo[0].pcscnt },
                        new SqlParameter("@Weight", SqlDbType.VarChar) { Value = objConsInfo[0].weight },
                        new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = strFlightNo },
                        new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = flightDate },
                        new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = strFlightOrigin },
                        new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = strFlightDestination },
                        new SqlParameter("@Action", SqlDbType.VarChar) { Value = "Booked" },
                        new SqlParameter("@Message", SqlDbType.VarChar) { Value = "FFR" },
                        new SqlParameter("@Description", SqlDbType.VarChar) { Value = "AWB Booked Through FFR Message" },
                        new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FFR" },
                        new SqlParameter("@UpdatedOn", SqlDbType.VarChar) { Value = DateTime.Now.ToString() },
                        new SqlParameter("@Public", SqlDbType.Bit) { Value = 1 }
                    };

                    //if (!dtb.ExecuteProcedure("SPAddAWBAuditLog", CNname, CType, CValues))
                    if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", sqlParamsAudit))
                        clsLog.WriteLog("AWB Audit log  for:" + awbnum + Environment.NewLine);



                    #endregion

                    #region Save AWB Routing
                    //Save routing information.
                    string status = "Q";
                    bool val = true;
                    if (objRouteInfo.Length > 0)
                    {

                        //string[] paramname = new string[] { "AWBNum", "AWBPrefix" };
                        //object[] paramobject = new object[] { awbnum, AWBPrefix };
                        //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };

                        SqlParameter[] sqlParamsDelete = new SqlParameter[]
                        {
                            new SqlParameter("@AWBNum", SqlDbType.VarChar) { Value = awbnum },
                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                        };


                        //if (dtb.ExecuteProcedure("spDeleteAWBRouteFFR", paramname, paramtype, paramobject))
                        if (await _readWriteDao.ExecuteNonQueryAsync("spDeleteAWBRouteFFR", sqlParamsDelete))
                        {
                            #region route insert Loop
                            for (int lstIndex = 0; lstIndex < objRouteInfo.Length; lstIndex++)
                            {
                                #region Switch FlightMonth
                                //string FlightMonth = "";
                                //switch (objRouteInfo[lstIndex].month.Trim().ToUpper())
                                //{
                                //    case "JAN":
                                //        {
                                //            FlightMonth = "01";
                                //            break;
                                //        }
                                //    case "FEB":
                                //        {
                                //            FlightMonth = "02";
                                //            break;
                                //        }
                                //    case "MAR":
                                //        {
                                //            FlightMonth = "03";
                                //            break;
                                //        }
                                //    case "APR":
                                //        {
                                //            FlightMonth = "04";
                                //            break;
                                //        }
                                //    case "MAY":
                                //        {
                                //            FlightMonth = "05";
                                //            break;
                                //        }
                                //    case "JUN":
                                //        {
                                //            FlightMonth = "06";
                                //            break;
                                //        }
                                //    case "JUL":
                                //        {
                                //            FlightMonth = "07";
                                //            break;
                                //        }
                                //    case "AUG":
                                //        {
                                //            FlightMonth = "08";
                                //            break;
                                //        }
                                //    case "SEP":
                                //        {
                                //            FlightMonth = "09";
                                //            break;
                                //        }
                                //    case "OCT":
                                //        {
                                //            FlightMonth = "10";
                                //            break;
                                //        }
                                //    case "NOV":
                                //        {
                                //            FlightMonth = "11";
                                //            break;
                                //        }
                                //    case "DEC":
                                //        {
                                //            FlightMonth = "12";
                                //            break;
                                //        }
                                //    default:
                                //        {
                                //            FlightMonth = "00";
                                //            break;
                                //        }
                                //}
                                //FlightDate = FlightMonth.PadLeft(2, '0') + "/" + objRouteInfo[lstIndex].date.PadLeft(2, '0') + "/" + System.DateTime.Now.Year.ToString();
                                FlightDate = Convert.ToDateTime(objRouteInfo[lstIndex].date).ToString("MM/dd/yyyy");

                                //******** Modified by Vishal on 28 DEC 2015 to resolve issue of next year in flight date.
                                //Find out if flight date with current year is less than server date time by at least 100 days.
                                DateTime dtFlightDate = DateTime.Now;
                                if (DateTime.TryParseExact(FlightDate, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out dtFlightDate))
                                {
                                    if (DateTime.Now.AddDays(-100) > dtFlightDate)
                                    {   //Advance year in flight date to next year.
                                        dtFlightDate = dtFlightDate.AddYears(1);
                                        FlightDate = dtFlightDate.ToString("MM/dd/yyyy");
                                    }
                                }
                                //string date = FlightMonth.PadLeft(2, '0') + "/" + objRouteInfo[lstIndex].date.PadLeft(2, '0') + "/" + System.DateTime.Now.Year.ToString();

                                string date = dtFlightDate.ToString("MM/dd/yyyy");

                                //******** Modified by Vishal on 28 DEC 2015 to resolve issue of next year in flight date.

                                #endregion

                                #region Route Status

                                if (objRouteInfo[lstIndex].spaceallotmentcode.Trim().Equals("KK", StringComparison.OrdinalIgnoreCase))
                                    status = "C";
                                else if (objRouteInfo[lstIndex].spaceallotmentcode.Trim().Equals("XX", StringComparison.OrdinalIgnoreCase))
                                    status = "X";
                                else if (objRouteInfo[lstIndex].spaceallotmentcode.Trim().Equals("LL", StringComparison.OrdinalIgnoreCase))
                                    status = "Q";
                                else
                                    status = "Q";

                                #endregion


                                //code to check weither flight is valid or not and active

                                //     string[] parms = new string[]
                                //     {
                                //             "FltOrigin",
                                //             "FltDestination",
                                //             "FlightNo",
                                //             "flightDate",
                                //             "AWBNumber",
                                //             "AWBPrefix",
                                //             "RefNo"
                                //     };

                                //     SqlDbType[] dataType = new SqlDbType[]
                                //{
                                //             SqlDbType.VarChar,
                                //             SqlDbType.VarChar,
                                //             SqlDbType.VarChar,
                                //             SqlDbType.DateTime,
                                //             SqlDbType.VarChar,
                                //             SqlDbType.VarChar,
                                //             SqlDbType.Int
                                //};


                                //     object[] value = new object[]
                                //{

                                //             objRouteInfo[lstIndex].fltdept,
                                //             objRouteInfo[lstIndex].fltarrival,
                                //             objRouteInfo[lstIndex].carriercode+objRouteInfo[lstIndex].fltnum,
                                //             DateTime.Parse(FlightDate),
                                //             awbnum,
                                //             AWBPrefix,
                                //             RefNo
                                //};

                                SqlParameter[] sqlParamsCheck = new SqlParameter[]
                                {
                                    new SqlParameter("@FltOrigin", SqlDbType.VarChar) { Value = objRouteInfo[lstIndex].fltdept },
                                    new SqlParameter("@FltDestination", SqlDbType.VarChar) { Value = objRouteInfo[lstIndex].fltarrival },
                                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = objRouteInfo[lstIndex].carriercode+objRouteInfo[lstIndex].fltnum },
                                    new SqlParameter("@flightDate", SqlDbType.DateTime) { Value = DateTime.Parse(FlightDate) },
                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                    new SqlParameter("@RefNo", SqlDbType.Int) { Value = RefNo }
                                };


                                //DataSet dsdata = dtb.SelectRecords("spCheckValidFlights", parms, value, dataType);
                                DataSet? dsdata = await _readWriteDao.SelectRecords("spCheckValidFlights", sqlParamsCheck);

                                //val = dtb.InsertData("spCheckValidFlights", parms, dataType, value);
                                int schedid = 0;

                                if (dsdata != null && dsdata.Tables[0].Rows[0][0].ToString() == "0")
                                {
                                    val = false;
                                    schedid = Convert.ToInt32(dsdata.Tables[0].Rows[0][0]);
                                    break;
                                }


                                //    string[] paramNames = new string[]
                                //{
                                //            "AWBNumber",
                                //            "FltOrigin",
                                //            "FltDestination",
                                //            "FltNumber",
                                //            "FltDate",
                                //            "Status",
                                //            "UpdatedBy",
                                //            "UpdatedOn",
                                //            "IsFFR",
                                //            "REFNo",
                                //            "date",
                                //            "AWBPrefix",
                                //            "schedid",
                                //             "allotmentcode",
                                //            "voluemcode",
                                //            "volume"
                                //};
                                //    SqlDbType[] dataTypes = new SqlDbType[]
                                //{
                                //            SqlDbType.VarChar,
                                //            SqlDbType.VarChar,
                                //            SqlDbType.VarChar,
                                //            SqlDbType.VarChar,
                                //            SqlDbType.DateTime,
                                //            SqlDbType.VarChar,
                                //            SqlDbType.VarChar,
                                //            SqlDbType.DateTime,
                                //            SqlDbType.Bit,
                                //            SqlDbType.Int,
                                //            SqlDbType.DateTime,
                                //            SqlDbType.VarChar,
                                //            SqlDbType.Int,
                                //             SqlDbType.VarChar,
                                //            SqlDbType.VarChar,
                                //            SqlDbType.VarChar
                                //};

                                //    object[] values = new object[]
                                //{
                                //            awbnum,
                                //            objRouteInfo[lstIndex].fltdept,
                                //            objRouteInfo[lstIndex].fltarrival,
                                //            objRouteInfo[lstIndex].carriercode+objRouteInfo[lstIndex].fltnum,
                                //            DateTime.Parse(FlightDate),
                                //            status,
                                //            "FFR",
                                //            DateTime.Now,
                                //            1,
                                //            RefNo,
                                //            DateTime.Parse(FlightDate),
                                //            AWBPrefix,
                                //            schedid,
                                //            (objRouteInfo[lstIndex].allotidentification!=null?objRouteInfo[lstIndex].allotidentification:""),
                                //            (objConsInfo[0].volumecode!=null?objConsInfo[0].volumecode:""),
                                //            (objConsInfo[0].volumeamt!=null ?  objConsInfo[0].volumeamt:"")

                                //};

                                SqlParameter[] sqlParamsInsertRoute = new SqlParameter[]
                                {
                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                    new SqlParameter("@FltOrigin", SqlDbType.VarChar) { Value = objRouteInfo[lstIndex].fltdept },
                                    new SqlParameter("@FltDestination", SqlDbType.VarChar) { Value = objRouteInfo[lstIndex].fltarrival },
                                    new SqlParameter("@FltNumber", SqlDbType.VarChar) { Value = objRouteInfo[lstIndex].carriercode+objRouteInfo[lstIndex].fltnum },
                                    new SqlParameter("@FltDate", SqlDbType.DateTime) { Value = DateTime.Parse(FlightDate) },
                                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = status },
                                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FFR" },
                                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = DateTime.Now },
                                    new SqlParameter("@IsFFR", SqlDbType.Bit) { Value = 1 },
                                    new SqlParameter("@REFNo", SqlDbType.Int) { Value = RefNo },
                                    new SqlParameter("@date", SqlDbType.DateTime) { Value = DateTime.Parse(FlightDate) },
                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                    new SqlParameter("@schedid", SqlDbType.Int) { Value = schedid },
                                    new SqlParameter("@allotmentcode", SqlDbType.VarChar) { Value = (objRouteInfo[lstIndex].allotidentification!=null?objRouteInfo[lstIndex].allotidentification:"") },
                                    new SqlParameter("@voluemcode", SqlDbType.VarChar) { Value = (objConsInfo[0].volumecode!=null?objConsInfo[0].volumecode:"") },
                                    new SqlParameter("@volume", SqlDbType.VarChar) { Value = (objConsInfo[0].volumeamt!=null ?  objConsInfo[0].volumeamt:"") }
                                };

                                //if (!dtb.UpdateData("spSaveFFRAWBRoute", paramNames, dataTypes, values))

                                if (!await _readWriteDao.ExecuteNonQueryAsync("spSaveFFRAWBRoute", sqlParamsInsertRoute))
                                    clsLog.WriteLogAzure("Error in Save AWB Route FFR ");


                                //objOpsAuditLog = new AWBOperations();
                                //objOpsAuditLog.AWBID = 0;
                                //objOpsAuditLog.AWBPrefix = AWBPrefix.Trim();
                                //objOpsAuditLog.AWBNumber = awbnum;
                                //objOpsAuditLog.Origin = strAWBOrigin;
                                //objOpsAuditLog.Destination = strAWBDestination;
                                //objOpsAuditLog.FlightNo = objRouteInfo[lstIndex].carriercode + objRouteInfo[lstIndex].fltnum;
                                //objOpsAuditLog.FlightDate = DateTime.Parse(FlightDate);
                                //objOpsAuditLog.FlightOrigin = objRouteInfo[lstIndex].fltdept.ToUpper();
                                //objOpsAuditLog.FlightDestination = objRouteInfo[lstIndex].fltarrival.ToUpper();
                                //objOpsAuditLog.BookedPcs = awbPcs;
                                //objOpsAuditLog.BookedWgt = Convert.ToDouble(awbWeight);
                                //objOpsAuditLog.UOM = objConsInfo[0].weightcode;
                                //objOpsAuditLog.Createdon = DateTime.UtcNow;
                                //objOpsAuditLog.Updatedon = DateTime.UtcNow;
                                //objOpsAuditLog.Createdby = strMessageFrom;
                                //objOpsAuditLog.Updatedby = strMessageFrom;
                                //objOpsAuditLog.Action = "Booked";
                                //objOpsAuditLog.Message = "AWB Flight Information";
                                //objOpsAuditLog.Description = objConsInfo[0].manifestdesc;

                                //log = new AuditLog();
                                //log.SaveLog(LogType.AWBOperations, string.Empty, string.Empty, objOpsAuditLog);

                                //string[] CANname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public" };
                                //SqlDbType[] CAType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit };
                                //object[] CAValues = new object[] { objConsInfo[0].airlineprefix, objConsInfo[0].awbnum, objConsInfo[0].origin, objConsInfo[0].dest, objConsInfo[0].pcscnt, objConsInfo[0].weight, objRouteInfo[lstIndex].carriercode + objRouteInfo[lstIndex].fltnum, DateTime.Parse(FlightDate), objRouteInfo[lstIndex].fltdept, objRouteInfo[lstIndex].fltarrival, "Booked", "FFR", "AWB Flight Information", "FFR", DateTime.Now.ToString(), 1 };

                                SqlParameter[] sqlParamsAuditRoute = new SqlParameter[]
                                {
                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = objConsInfo[0].airlineprefix },
                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = objConsInfo[0].awbnum },
                                    new SqlParameter("@Origin", SqlDbType.VarChar) { Value = objConsInfo[0].origin },
                                    new SqlParameter("@Destination", SqlDbType.VarChar) { Value = objConsInfo[0].dest },
                                    new SqlParameter("@Pieces", SqlDbType.VarChar) { Value = objConsInfo[0].pcscnt },
                                    new SqlParameter("@Weight", SqlDbType.VarChar) { Value = objConsInfo[0].weight },
                                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = objRouteInfo[lstIndex].carriercode + objRouteInfo[lstIndex].fltnum },
                                    new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = DateTime.Parse(FlightDate) },
                                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = objRouteInfo[lstIndex].fltdept },
                                    new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = objRouteInfo[lstIndex].fltarrival },
                                    new SqlParameter("@Action", SqlDbType.VarChar) { Value = "Booked" },
                                    new SqlParameter("@Message", SqlDbType.VarChar) { Value = "FFR" },
                                    new SqlParameter("@Description", SqlDbType.VarChar) { Value = "AWB Flight Information" },
                                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FFR" },
                                    new SqlParameter("@UpdatedOn", SqlDbType.VarChar) { Value = DateTime.Now.ToString() },
                                    new SqlParameter("@Public", SqlDbType.Bit) { Value = 1 }
                                };

                                //if (!dtb.ExecuteProcedure("SPAddAWBAuditLog", CANname, CAType, CAValues))
                                if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", sqlParamsAuditRoute))
                                    clsLog.WriteLog("AWB Audit log  for:" + awbnum + Environment.NewLine);


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

                            #region Deleting AWB Data if No Route Present
                            if (val == true)
                            {

                                //string[] QueryNames = { "AWBPrefix", "AWBNumber" };
                                //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                                //object[] QueryValues = { AWBPrefix, awbnum };

                                SqlParameter[] sqlParamsDeleteAWBDetails = new SqlParameter[]
                                {
                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum }
                                };
                                //if (!dtb.UpdateData("spDeleteAWBDetailsNoRoute", QueryNames, QueryTypes, QueryValues))
                                if (!await _readWriteDao.ExecuteNonQueryAsync("spDeleteAWBDetailsNoRoute", sqlParamsDeleteAWBDetails))
                                {
                                    clsLog.WriteLogAzure("Error in Deleting AWB Details ");
                                }
                            }



                            #endregion

                            #endregion

                        }
                    }

                    #endregion Save AWB Routing


                    if (val)
                    {
                        #region AWB Dimensions
                        if (objDimension.Length > 0)
                        {
                            //Badiuz khan
                            //Description: Delete Dimension if Dimension 

                            //string[] dparam = { "AWBPrefix", "AWBNumber" };
                            //SqlDbType[] dbparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                            //object[] dbparamvalues = { AWBPrefix, awbnum };

                            SqlParameter[] sqlParamsDeleteDimension = new SqlParameter[]
                            {
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum }
                            };

                            //if (!dtb.InsertData("SpDeleteDimensionThroughMessage", dparam, dbparamtypes, dbparamvalues))
                            if (!await _readWriteDao.ExecuteNonQueryAsync("SpDeleteDimensionThroughMessage", sqlParamsDeleteDimension))
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

                                    //string[] param = { "AWBNumber", "RowIndex", "Length", "Breadth", "Height", "PcsCount", "MeasureUnit", "AWBPrefix", "Weight", "UpdatedBy" };
                                    //SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Decimal, SqlDbType.Decimal, SqlDbType.Decimal, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Decimal, SqlDbType.VarChar };
                                    Decimal DimWeight = 0;


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

                                    //object[] value =
                                    //    {
                                    //        awbnum,"1",objDimension[i].length,objDimension[i].width,objDimension[i].height,
                                    //         objDimension[i].piecenum,objDimension[i].mesurunitcode,AWBPrefix,decimal.TryParse(objDimension[i].weight,out DimWeight)==true?Convert.ToDecimal(objDimension[i].weight):0,"FFR"
                                    //};

                                    SqlParameter[] sqlParamsInsertDimension = new SqlParameter[]
                                  {
                                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                        new SqlParameter("@RowIndex", SqlDbType.Int) { Value = 1 },
                                        new SqlParameter("@Length", SqlDbType.Decimal) { Value = objDimension[i].length },
                                        new SqlParameter("@Breadth", SqlDbType.Decimal) { Value = objDimension[i].width },
                                        new SqlParameter("@Height", SqlDbType.Decimal) { Value = objDimension[i].height },
                                        new SqlParameter("@PcsCount", SqlDbType.Int) { Value = objDimension[i].piecenum },
                                        new SqlParameter("@MeasureUnit", SqlDbType.VarChar) { Value = objDimension[i].mesurunitcode },
                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                        new SqlParameter("@Weight", SqlDbType.Decimal) { Value = decimal.TryParse(objDimension[i].weight,out DimWeight)==true?Convert.ToDecimal(objDimension[i].weight):0 },
                                        new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FFR" }
                                  };

                                    //if (!dtb.InsertData("SP_SaveAWBDimensions_FFR", param, dbtypes, value))
                                    if (!await _readWriteDao.ExecuteNonQueryAsync("SP_SaveAWBDimensions_FFR", sqlParamsInsertDimension))
                                        clsLog.WriteLogAzure("Error  Delete Dimension Through Message :" + awbnum);
                                }
                            }
                        }
                        else
                        {
                            //string[] dDparam = { "AWBPrefix", "AWBNumber" };
                            //SqlDbType[] dDbparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                            //object[] dDbparamvalues = { AWBPrefix, awbnum };

                            SqlParameter[] sqlParamsDeleteDimension = new SqlParameter[]
                            {
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum }
                            };

                            //if (!dtb.InsertData("SpDeleteDimensionThroughMessage", dDparam, dDbparamtypes, dDbparamvalues))
                            if (!await _readWriteDao.ExecuteNonQueryAsync("SpDeleteDimensionThroughMessage", sqlParamsDeleteDimension))
                                clsLog.WriteLogAzure("Error  Delete Dimension Through Message :" + awbnum);
                        }
                        #endregion

                        #region ProcessRateFunction
                        DataSet? dsrateCheck = await CheckAirlineForRateProcessing(AWBPrefix, "FFR");
                        if (dsrateCheck != null && dsrateCheck.Tables.Count > 0 && dsrateCheck.Tables[0].Rows.Count > 0)
                        {

                            //string[] CRNname = new string[] { "AWBNumber", "AWBPrefix", "UpdatedBy", "UpdatedOn", "ValidateMin", "UpdateBooking", "RouteFrom", "UpdateBilling" };
                            //SqlDbType[] CRType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Bit, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.Bit };
                            //object[] CRValues = new object[] { awbnum, AWBPrefix, "FFR", System.DateTime.Now, 1, 1, "B", 0 };

                            SqlParameter[] sqlParamsCalculateRates = new SqlParameter[]
                            {
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FFR" },
                                new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = System.DateTime.Now },
                                new SqlParameter("@ValidateMin", SqlDbType.Bit) { Value = 1 },
                                new SqlParameter("@UpdateBooking", SqlDbType.Bit) { Value = 1 },
                                new SqlParameter("@RouteFrom", SqlDbType.VarChar) { Value = "B" },
                                new SqlParameter("@UpdateBilling", SqlDbType.Bit) { Value = 0 }
                            };

                            //if (!dtb.ExecuteProcedure("sp_CalculateAWBRatesReprocess", CRNname, CRType, CRValues))
                            if (!await _readWriteDao.ExecuteNonQueryAsync("sp_CalculateAWBRatesReprocess", sqlParamsCalculateRates))
                            {
                                clsLog.WriteLog("Rates Not Calculated for:" + awbnum + Environment.NewLine);
                            }

                        }
                        #endregion

                        #region Capacity Update

                        //string[] cparam = { "AWBPrefix", "AWBNumber" };
                        //SqlDbType[] cparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                        //object[] cparamvalues = { AWBPrefix, awbnum };

                        SqlParameter[] sqlParamsUpdateCapacity = new SqlParameter[]
                        {
                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                            new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum }
                        };
                        //if (!dtb.InsertData("UpdateCapacitythroughMessage", cparam, cparamtypes, cparamvalues))

                        if (!await _readWriteDao.ExecuteNonQueryAsync("UpdateCapacitythroughMessage", sqlParamsUpdateCapacity))
                            clsLog.WriteLogAzure("Error  on Update capacity Plan :" + awbnum);

                        #endregion

                        // if (flag)
                        // xfnmMessageProcessor.GenerateXFNMMessage(strFFRMessage, "FFR Recevied and will conform the booking ASAP for  this AWBNo  " + AWBPrefix + '-' + awbnum, AWBPrefix, awbnum, "", "", "", "XFFR");
                        //GenerateFMAMessage(strFFRMessage, "FFR Recevied and will conform the booking ASAP for  this AWBNo  " + AWBPrefix + '-' + awbnum);
                        #region : Refresh Capacity :
                        for (int lstIndex = 0; lstIndex < objRouteInfo.Length; lstIndex++)
                        {
                            if ((objRouteInfo[lstIndex].carriercode + objRouteInfo[lstIndex].fltnum).Trim() != string.Empty && flightDate != null && objRouteInfo[lstIndex].fltdept.Trim() != string.Empty)
                            {
                                //SQLServer sqlServer = new SQLServer();
                                SqlParameter[] sqlParameter = new SqlParameter[]{
                                    new SqlParameter("@FlightID",(objRouteInfo[lstIndex].carriercode + objRouteInfo[lstIndex].fltnum).Trim())
                                    , new SqlParameter("@FlightDate",FlightDate)
                                    , new SqlParameter("@Source",objRouteInfo[lstIndex].fltdept.Trim())
                                 };
                                //DataSet dsRefreshCapacity = sqlServer.SelectRecords("uspRefreshCapacity", sqlParameter);

                                DataSet? dsRefreshCapacity = await _readWriteDao.SelectRecords("uspRefreshCapacity", sqlParameter);

                            }
                        }
                        #endregion
                    }



                }

            }
            catch (Exception)
            {
                ErrorMsg = string.Empty;
            }
            //return flag;
            return (flag, ErrorMsg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strRecord"></param>
        public async Task<DataSet?> CheckValidateFFRMessage(string AirlinePrefix, string AWBNo, string AWBOrigin, string AWBDestination, string UpdateBy)
        {
            //string conStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
            //SQLServer da = new SQLServer();
            //string[] Pname = new string[5];
            //object[] Pvalue = new object[5];
            //SqlDbType[] Ptype = new SqlDbType[5];

            DataSet? ds = new DataSet();
            try
            {
                //Pname[0] = "AirlinePrefix";
                //Ptype[0] = SqlDbType.VarChar;
                //Pvalue[0] = AirlinePrefix;

                //Pname[1] = "AWBNo";
                //Ptype[1] = SqlDbType.VarChar;
                //Pvalue[1] = AWBNo;

                //Pname[2] = "AWBOrigin";
                //Ptype[2] = SqlDbType.VarChar;
                //Pvalue[2] = AWBOrigin;

                //Pname[3] = "AWBDestination";
                //Ptype[3] = SqlDbType.VarChar;
                //Pvalue[3] = AWBDestination;

                //Pname[4] = "UpdateBy";
                //Ptype[4] = SqlDbType.VarChar;
                //Pvalue[4] = UpdateBy;

                SqlParameter[] sqlParams = new SqlParameter[]
                {
                    new SqlParameter("@AirlinePrefix", SqlDbType.VarChar) { Value = AirlinePrefix },
                    new SqlParameter("@AWBNo", SqlDbType.VarChar) { Value = AWBNo },
                    new SqlParameter("@AWBOrigin", SqlDbType.VarChar) { Value = AWBOrigin },
                    new SqlParameter("@AWBDestination", SqlDbType.VarChar) { Value = AWBDestination },
                    new SqlParameter("@UpdateBy", SqlDbType.VarChar) { Value = UpdateBy }
                };


                //ds = da.SelectRecords("SPValidateFFRAWB", Pname, Pvalue, Ptype);
                ds = await _readWriteDao.SelectRecords("SPValidateFFRAWB", sqlParams);

                return ds;

            }
            catch (Exception)
            {
                //scm.logexception(ref ex);
                //clsLog.WriteLogAzure(ex, "BLExpManifest", "GetAwbTabdetails_GHA");
                return ds = null;
            }
            //finally
            //{
            //    da = null;
            //    if (ds != null)
            //        ds.Dispose();
            //    Pname = null;
            //    Pvalue = null;
            //    Ptype = null;
            //}
        }

        public async Task<DataSet?> CheckAirlineForRateProcessing(string AirlinePrefix, string MessageType)
        {
            DataSet? dssitaMessage = new DataSet();
            try
            {
                //string conStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
                //SQLServer dtb = new SQLServer();

                //string[] paramname = new string[] { "AirlinePrefix", "MessageType" };
                //object[] paramvalue = new object[] { AirlinePrefix, MessageType };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };

                SqlParameter[] sqlParams = new SqlParameter[]
                {
                    new SqlParameter("@AirlinePrefix", SqlDbType.VarChar) { Value = AirlinePrefix },
                    new SqlParameter("@MessageType", SqlDbType.VarChar) { Value = MessageType }
                };

                //dssitaMessage = dtb.SelectRecords("CheckAirlineForRateProcessing", paramname, paramvalue, paramtype);
                dssitaMessage = await _readWriteDao.SelectRecords("CheckAirlineForRateProcessing", sqlParams);
            }
            catch (Exception)
            {
                //scm.logexception(ref ex);
                dssitaMessage = null;
            }
            return dssitaMessage;


        }
        #endregion :: Public Methods ::
    }
}
