using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
using System.Text;

namespace QidWorkerRole
{
    public class PSNMessageProcessor
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<PSNMessageProcessor> _logger;
         private static ILoggerFactory? _loggerFactory;
        private static ILogger<PSNMessageProcessor> _staticLogger => _loggerFactory?.CreateLogger<PSNMessageProcessor>();

        private readonly GenericFunction _genericFunction;

        #region Constructor
        public PSNMessageProcessor(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<PSNMessageProcessor> logger,
            GenericFunction genericFunction,
            ILoggerFactory loggerFactory)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _loggerFactory = loggerFactory;
            _genericFunction = genericFunction;
        }
        #endregion
        //public PSN objPSN = null;
        //SQLServer db = new SQLServer();


        #region PSN Class
        [Serializable]
        public class PSN
        {
            //Message Type (Note : Three Characters). 
            public SMI StandardMessageIdentifier = new SMI();
            //Cargo Control Location details CCL
            public CCL CCLDetails = new CCL();
            //AWB Details
            public AWB1[] AWBDetails = new AWB1[0];

            public ARR2[] ArrivalDetails = new ARR2[0];

            public ASN ASN = new ASN();

            #region Overriding ToString Method for PSN
            public override string ToString()
            {

                try
                {
                    StringBuilder sbPSN = new StringBuilder();
                    //Message Identifier
                    sbPSN.AppendLine(StandardMessageIdentifier.StandardMessageIdentifier);
                    //Cargo Control Location Details
                    if (CCLDetails.AirportOfArrival != string.Empty && CCLDetails.CargoTerminalOperator != string.Empty)
                    {
                        sbPSN.AppendLine(CCLDetails.AirportOfArrival.ToString() + CCLDetails.CargoTerminalOperator);
                    }
                    //AWB Details
                    foreach (AWB1 AWBs in AWBDetails)
                    {
                        sbPSN.AppendLine(AWBs.AWBPrefix + "-" + AWBs.AWBNumber + ((AWBs.ConsolidationIdentifier != string.Empty || AWBs.HAWBNumber != string.Empty) ? "-" + AWBs.ConsolidationIdentifier + AWBs.HAWBNumber : string.Empty) + (AWBs.PackageTrackingIdentifier != string.Empty ? "/" + AWBs.PackageTrackingIdentifier : string.Empty));
                    }

                    //Arrival Details
                    foreach (ARR2 ArrivalDet in ArrivalDetails)
                    {
                        sbPSN.AppendLine(ArrivalDet.ComponentIdentifier + "/" + ArrivalDet.ImportingCarrier + ArrivalDet.FlightNumber + "/" + ArrivalDet.ScheduledArrivalDate + (ArrivalDet.PartArrivalReference != string.Empty ? "-" + ArrivalDet.PartArrivalReference : String.Empty));
                    }

                    if (!String.IsNullOrEmpty(ASN.ComponentIdentifier))
                    {
                        String strTemp = "";

                        strTemp += ASN.ComponentIdentifier;
                        if (!String.IsNullOrEmpty(ASN.StatusCode))
                        {
                            strTemp += ASN.StatusCode;
                        }
                        if (!String.IsNullOrEmpty(ASN.ActionExplanation))
                        {
                            strTemp += ASN.ActionExplanation;
                        }
                        sbPSN.AppendLine(strTemp);
                    }

                    return sbPSN.ToString();
                }
                catch (Exception ex)
                {
                    //clsLog.WriteLogAzure(ex);
                    _staticLogger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    return string.Empty;
                }
            }
            #endregion

            public PSN Encode(DataSet ds)
            {
                try
                {
                    PSN PSN = new PSN();
                    if (ds != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                foreach (DataRow row in ds.Tables[0].Rows)
                                {

                                    PSN.StandardMessageIdentifier.StandardMessageIdentifier = row["StandardMessageIdentifier"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[1].Rows)
                                {

                                    PSN.CCLDetails.AirportOfArrival = row["AirportOfArrival"].ToString();
                                    PSN.CCLDetails.CargoTerminalOperator = row["CargoTerminalOperator"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[2].Rows)
                                {

                                    Array.Resize(ref PSN.AWBDetails, PSN.AWBDetails.Length + 1);
                                    PSN.AWBDetails[PSN.AWBDetails.Length - 1] = new AWB1();
                                    PSN.AWBDetails[PSN.AWBDetails.Length - 1].AWBPrefix = row["AWBPrefix"].ToString();
                                    PSN.AWBDetails[PSN.AWBDetails.Length - 1].AWBNumber = row["AWBNumber"].ToString();
                                    PSN.AWBDetails[PSN.AWBDetails.Length - 1].ConsolidationIdentifier = row["ConsolidationIdentifier"].ToString();
                                    PSN.AWBDetails[PSN.AWBDetails.Length - 1].HAWBNumber = row["HAWBNumber"].ToString();
                                    PSN.AWBDetails[PSN.AWBDetails.Length - 1].PackageTrackingIdentifier = row["PackageTrackingIdentifier"].ToString();

                                }

                                foreach (DataRow row in ds.Tables[3].Rows)
                                {
                                    Array.Resize(ref PSN.ArrivalDetails, PSN.ArrivalDetails.Length + 1);
                                    PSN.ArrivalDetails[PSN.ArrivalDetails.Length - 1] = new ARR2();
                                    PSN.ArrivalDetails[PSN.ArrivalDetails.Length - 1].ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    PSN.ArrivalDetails[PSN.ArrivalDetails.Length - 1].FlightNumber = row["FlightNumber"].ToString();
                                    PSN.ArrivalDetails[PSN.ArrivalDetails.Length - 1].ImportingCarrier = row["ImportingCarrier"].ToString();
                                    PSN.ArrivalDetails[PSN.ArrivalDetails.Length - 1].PartArrivalReference = row["PartArrivalReference"].ToString();
                                    PSN.ArrivalDetails[PSN.ArrivalDetails.Length - 1].ScheduledArrivalDate = row["ScheduledArrivalDate"].ToString();
                                }

                                foreach (DataRow row in ds.Tables[4].Rows)
                                {
                                    PSN.ASN.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    PSN.ASN.StatusCode = row["StatusCode"].ToString();
                                    PSN.ASN.ActionExplanation = row["ActionExplanation"].ToString();
                                }

                                return PSN;

                            }

                        }

                    }
                    return null;
                }
                catch (Exception ex)
                {
                    //clsLog.WriteLogAzure(ex);
                    _staticLogger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    return null;
                }
            }


        }
        #endregion

        #region SubClasses
        #region Standard Message Identifier SMI
        public class SMI
        {
            public string StandardMessageIdentifier = string.Empty;
        }
        #endregion

        #region Cargo Control Location (CCL) class
        public class CCL
        {
            //The IATA code of the first airport of arrival in the United States.
            public string AirportOfArrival = string.Empty;
            //The IATA/ICAO air carrier code for an Air AMS Carrier. FIRMS code for an Air AMS deconsolidator.
            public string CargoTerminalOperator = string.Empty;
        }
        #endregion

        #region AWB Details Class (not FSQ/FSC)
        public class AWB1
        {
            //The standard air carrier prefix.
            public string AWBPrefix = string.Empty;
            //An 8-digit number composed of a 7-digit serial number and the MOD-7 check-digit number.
            public string AWBNumber = string.Empty;
            //The consolidation identifier "M" is used to identify a master air waybill.
            public string ConsolidationIdentifier = string.Empty;
            //The alphanumeric house air waybill number.
            public string HAWBNumber = string.Empty;
            //The alphanumeric field to identify a house air waybill.
            public string PackageTrackingIdentifier = string.Empty;
        }

        #endregion

        #region AWB Air Waybill (FSQ/FSC messages only):
        public class AWB2
        {
            public String AWBPrefix = string.Empty;      //	3AN	M	The standard air carrier prefix. The International Air Transport Association (IATA) may issue air waybill prefixes.
            public String AWBNumber = string.Empty;     //	8N	M	An 8-digit number composed of a 7-digit serial number and the MOD 7 check-digit number.
            public String HAWBNumber = string.Empty;      //	1/12AN	C	The alphanumeric house air waybill number  
            public String ArrivalReference = string.Empty;      //	1A	C	The alpha code referring to a specific part arrival of a split shipment identified to an air waybill.

        }

        #endregion

        #region Waybill Details class
        public class WBL
        {
            //Must be WBL.
            public string ComponentIdentifier = "WBL";
            //The code of the foreign airport from which a shipment began its transportation by air to the U.S. Airport codes are available from the IATA Airline Coding Directory.
            public string AirportOfOrigin = string.Empty;
            //The U.S. airport code of destination when an air waybill is transported by the air carrier under the provisions of a permit to proceed.
            public string PermitToProceedDestAirport = string.Empty;
            //Must be 'T'
            public string ShipmentDescriptionCode = string.Empty;
            //Total number of pieces. If Consolidation, report the cumulative house-level piece count.
            public int NumberOfPieces = 0;
            //K (Kilos) or L (Pounds)
            public string WeightCode = string.Empty;
            //Total Weight. If included, a decimal must be followed by a number.
            public double Weight = 0;
            //Description of the merchandise as listed on the air waybill document.
            public string CargoDescription = string.Empty;
            //Date in NNAAA format where NN is the two character numerical day of the month and AAA is the first three alpha characters of the Month.
            public string ArrivalDatePermitToProceed = string.Empty;
        }
        #endregion


        #region ARR Details Class (not FSC/FSN/FSI/FRX/FXX)
        public class ARR1
        {
            //Must be ARR
            public string ComponentIdentifier = "ARR";
            //Air Carrier Code.
            public string ImportingCarrier = string.Empty;
            //Flight Number assigned by the importing carrier.
            public string FlightNumber = string.Empty;
            //Scheduled arrival date in NNAAA format EX:10DEC.
            public string ScheduledArrivalDate = string.Empty;
            //Alpha Code assigned to one flight when the cargo covered by a single air waybill arrives on more than aircraft & actual boarded piece count is less than total waybill piece count.
            //Also known as Split Indicator
            public string PartArrivalReference = string.Empty;
            //A code 'B' to signify that the following count is the actual boarded quantity.
            public string BoardedQuantityIdentifier = string.Empty;
            //Actual number of pieces boarded on this flight.This value must be greater than zero and less than the total piece count of the air waybill.
            public int BoardedPieceCount = 0;
            //K(Kilos) or L(Pounds)
            public string WeightCode = string.Empty;
            //Weight of the Boarded pieces
            public double Weight = 0;

        }
        #endregion

        #region  Arrival (ARR) (FSC/FSN/FSI/FRX/FXX messages only):
        public class ARR2
        {
            //Must be ARR
            public string ComponentIdentifier = "ARR";
            //Air Carrier Code.
            public string ImportingCarrier = string.Empty;
            //Flight Number assigned by the importing carrier.
            public string FlightNumber = string.Empty;
            //Scheduled arrival date in NNAAA format EX:10DEC.
            public string ScheduledArrivalDate = string.Empty;
            //Alpha Code assigned to one flight when the cargo covered by a single air waybill arrives on more than aircraft & actual boarded piece count is less than total waybill piece count.
            //Also known as Split Indicator
            public string PartArrivalReference = string.Empty;

        }
        #endregion

        #region  Agent (AGT) Class
        public class AGT
        {
            //Must be AGT
            public string ComponentIdentifier = string.Empty;
            //An air carrier code,FIRMS code or ABI filer's Air AMS identifier.
            public string AirAMSParticipantCode = string.Empty;
        }
        #endregion

        #region Shipper (SHP) Class
        public class SHP
        {
            //Must be SHP.
            public string ComponentIdentifier = "SHP";
            //Name of the Shipper
            public string Name = string.Empty;
            //Street Address of the Shipper
            public string StreetAddress = string.Empty;
            //The City,County or Township of the Shipper.
            public string City = string.Empty;
            //The State or Province code of the Shipper.
            public string State = string.Empty;
            //Use a valid International Standards Organization(ISO) Country Code.
            public string CountryCode = string.Empty;
            //The Postal Code of Shipper.
            public string PostalCode = string.Empty;

        }
        #endregion

        #region Consignee (CNE) Class
        public class CNE
        {
            //Must be CNE.
            public string ComponentIdentifier = "CNE";
            //Name of the Consignee.
            public string Name = string.Empty;
            //Street Address of the Consignee
            public string StreetAddress = string.Empty;
            //The City,County or Township of the Consignee.
            public string City = string.Empty;
            //The State or Province code of the Consignee.
            public string State = string.Empty;
            //Use a valid International Standards Organization(ISO) Country Code.
            public string CountryCode = string.Empty;
            //The Postal Code of the Consignee.
            public string PostalCode = string.Empty;
            //Hyphens may be used.
            public string TelephoneNumber = string.Empty;

        }
        #endregion

        #region Transfer Details (TRN)
        public class TRN
        {
            //Must be TRN
            public string ComponentIdentifier = "TRN";
            //The 3-Character IATA U.S. Airport Code of Destination or '000' to cancel previously authorized transfer information.
            public string DestinationAirport = string.Empty;
            //Enter 'I' for International,'D' for Domestic. Enter 'R' when Foreign Cargo Remaining On Board(FROB). Omit when canceling previously accepted Transfer.
            public string DomesticInternationIdentifier = string.Empty;
            //Formats Accepted: NN-NNNNNNNAA or NN-NNNNNNNNN (importer/IRS#); NNN-NN-NNNN(SSN); NNNNNN-NNNNN(CBP assigned). Hyphens Required.
            public string BondedCarrierID = string.Empty;
            //The Air Carrier Code of the Bonded Onward Carrier.
            public string OnwardCarrier = string.Empty;
            //When Transferring freight to the terminal facility of another airline,the air carrier code may be used. When transferring to a deconsolidator,a FIRMS code must be used.
            public string BondedPremisesIdentifier = string.Empty;
            //The 9-digit in-bond control number.
            public string InBondControlNumber = string.Empty;

        }
        #endregion

        #region CBP Shipment Description (CSD)
        public class CSD
        {
            //Must be CSD.
            public string ComponentIdentifier = "CSD";
            //The ISO country code corresponding to the country of origin of the merchandise.
            public string OriginOfGoods = string.Empty;
            //Monetary value of the shipment.
            public double DeclaredValue = 0;
            //The ISO currency code in which the value of the merchandise was declared. 
            //The value of the merchandise in U.S. Dollars is required for in bond & express Consignment Shipments.
            public string ISOCurrencyCode = string.Empty;
            //The classification of the merchandise according to the Harmonized Tarrif Schedule of the United States.
            public string HarmonizedCommodityCode = string.Empty;
        }
        #endregion

        #region FDA Freight Indicator Class
        public class FDA
        {
            //Must be FDA
            public string FDADetails = string.Empty;
        }
        #endregion

        #region RFA Reason for Amendment
        public class RFA
        {
            //C Must be RFA.
            public String ComponentIdentifier = null;
            public String AmendmentCode = null;

            //C	Free format explanation for the amendment code.
            public String AmendmentExplanation = null;
        }

        #endregion

        #region ASN Airline Status Notification
        public class ASN
        {
            public String ComponentIdentifier = null;     //	3A	M	Must be ASN.
            public String StatusCode = null;               //1N	M	Valid status codes are located in Appendix A.
            public String ActionExplanation = null;    //	1-20AN	O	Optional field to explain the reason for the notification.

        }
        #endregion

        #region DEP Departure
        public class DEP
        {
            public String ComponentIdentifier = null;      //3A	M	Must be DEP.
            public String ImportingCarrier = null;	    //2-3AN	M	The carrier code of the airline that sent the DEP message.
            public String FlightNumber = null;     //	3-5AN	M	Valid flight number formats are: three numeric (003), three numeric followed by an alpha character (003A), four numeric (1234), or four numeric followed by an alpha character (1234A).
            public String DateOfScheduledArrival = null;     //	5AN	M	Scheduled date of arrival at the first US airport in NNAAA format.
            public String LiftoffDate = null;     //	5AN	C	Actual departure date in NNAAA format at last foreign airport.  
            public String LiftoffTime = null;     //	4N	C	Actual departure time (GMT) in HHMM (hour, minute) format. 
            public String ActualImportingCarrier = null;     //	2-3AN	M	The carrier code of the actual airline that is carrying the freight.
            public String ActualFlightNumber = null;     //	3-5AN	M	Flight number for actual flight that is carrying the freight.  Valid flight number formats are: three numeric (NNN), three numeric followed by an alpha character (NNNA), four numeric (NNNN), or four numeric following by an alpha character (NNNNA).

        }

        #endregion

        #region Error Report Flight (ERF)

        public class ERF
        {
            public String ImportingCarrier = null;//	2-3AN	C	Air carrier code.  Valid codes can be located in the IATA Coding Directory
            public String FlightNumber = null;//	3N(N)(A)	C	Number assigned by importing carrier.  Format must be NNN, NNNA, NNNN or NNNNA.
            public String Date = null;//	5AN	M	NNAAA format, where the NN is the two-character numerical day of the month and AAA is the first three alpha characters of the month, e.g., DEC equal December.

        }
        #endregion

        #region Error (ERR)

        public class ERR
        {
            public String ComponentIdentifier = null;//	3A	M	Must be ERR.  The ERR line identifier will be repeated for each type of error that is reported. The number of error codes that will be reported is constrained by the maximum number of characters that can be supported in the output message, not to exceed the CRLF of the last complete ERR line.  
            public String ErrorCode = null;  //	3N	M	Valid Error codes are located in Appendix A.
            public String ErrorMessageText = null;//	40AN	M	A brief message describing the error.  Refer to the error codes in Appendix A for further information.  A number of these text messages contain characters that are not supported by the IATA Cargo-IMP message system.

        }
        #endregion

        #region CBP Entry Detail (CED)

        public class CED
        {
            public String ComponentIdentifier = null;
            public String EntryType = null;
            public String EntryNumber = null;

        }
        #endregion

        #region FSQ Freight Status Query
        public class FSQSub
        {
            public String ComponentIdentifier = null;
            public String StatusRequestCode = null;

        }
        #endregion

        #endregion

        #region Update Method for PSN object
        public void readQueryValuesPSN(PSN objPSN, ref object[] QueryValues, string EncodedMessage, string FlightNo, DateTime FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {
                SMI objSMI = objPSN.StandardMessageIdentifier;
                AWB1 objAWB1 = objPSN.AWBDetails[0];
                CCL objCCL = objPSN.CCLDetails;
                WBL objWBL = null;
                ARR2 objARR = null;
                ASN objASN = objPSN.ASN; ;
                AGT objAGT = null;
                SHP objSHP = null;
                CNE objCNE = null;
                TRN objTRN = null;
                CSD objCSD = null;
                RFA objRFA = null;
                DEP objDEP = null;
                ERR objERR = null;
                ERF objERF = null;

                //--------------------AWB----------------------------------
                if (objAWB1 != null)
                {
                    QueryValues[valNo++] = objAWB1.AWBPrefix.Trim();
                    QueryValues[valNo++] = objAWB1.AWBNumber.Trim();
                    QueryValues[valNo++] = objSMI.StandardMessageIdentifier;
                    QueryValues[valNo++] = objAWB1.HAWBNumber.Trim();
                    QueryValues[valNo++] = objAWB1.ConsolidationIdentifier.Trim();
                    QueryValues[valNo++] = objAWB1.PackageTrackingIdentifier;

                    QueryValues[valNo++] = "";//   only for fsq ans fsc txtPartArrival.Text.Trim();
                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                }
                //-----------------CCL-----------------------

                if (objCCL != null)
                {
                    QueryValues[valNo++] = objCCL.AirportOfArrival;
                    QueryValues[valNo++] = objCCL.CargoTerminalOperator;
                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                }

                //---------------------WBL-----------




                if (objWBL != null)
                {


                    QueryValues[valNo++] = objWBL.AirportOfOrigin.Trim();
                    QueryValues[valNo++] = objWBL.PermitToProceedDestAirport;
                    QueryValues[valNo++] = objWBL.NumberOfPieces;
                    QueryValues[valNo++] = objWBL.WeightCode.Trim();
                    QueryValues[valNo++] = objWBL.Weight;
                    QueryValues[valNo++] = objWBL.CargoDescription;

                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "0";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = 0;
                    QueryValues[valNo++] = "";
                }

                //------------------ARR------------------------------
                if (objARR != null)
                {
                    QueryValues[valNo++] = objARR.ScheduledArrivalDate;
                    QueryValues[valNo++] = objARR.PartArrivalReference;
                    QueryValues[valNo++] = "";//objARR.BoardedQuantityIdentifier;
                    QueryValues[valNo++] = 0;// objARR.BoardedPieceCount;
                    QueryValues[valNo++] = 0;//objARR.Weight;
                    QueryValues[valNo++] = "";//objARR.Weight;
                    QueryValues[valNo++] = objARR.ImportingCarrier;
                    QueryValues[valNo++] = objARR.FlightNumber;
                    QueryValues[valNo++] = objARR.PartArrivalReference;

                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = 0;
                    QueryValues[valNo++] = 0;
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";

                }
                //------------------------HLD---------------

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";


                //-------------------------CED--------------------------

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";



                //----------------------AGT----------------------
                if (objAGT != null)
                {
                    QueryValues[valNo++] = objAGT.AirAMSParticipantCode;
                }
                else
                {
                    QueryValues[valNo++] = "";
                }


                //---------------SHP-------------------------------

                if (objSHP != null)
                {
                    QueryValues[valNo++] = objSHP.Name;
                    QueryValues[valNo++] = objSHP.StreetAddress;
                    QueryValues[valNo++] = objSHP.City;
                    QueryValues[valNo++] = objSHP.State;
                    QueryValues[valNo++] = objSHP.CountryCode;
                    QueryValues[valNo++] = objSHP.PostalCode;
                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                }


                //-----------------CNE-------------------------

                if (objCNE != null)
                {
                    QueryValues[valNo++] = objCNE.Name;
                    QueryValues[valNo++] = objCNE.StreetAddress;
                    QueryValues[valNo++] = objCNE.City;
                    QueryValues[valNo++] = objCNE.State;
                    QueryValues[valNo++] = objCNE.CountryCode;
                    QueryValues[valNo++] = objCNE.PostalCode;

                }
                else
                {

                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";

                }


                //------------------TRN--------------------------
                if (objTRN != null)
                {
                    QueryValues[valNo++] = objTRN.DestinationAirport;
                    QueryValues[valNo++] = objTRN.DomesticInternationIdentifier;
                    QueryValues[valNo++] = objTRN.BondedCarrierID;
                    QueryValues[valNo++] = objTRN.OnwardCarrier;
                    QueryValues[valNo++] = objTRN.BondedPremisesIdentifier;
                    QueryValues[valNo++] = objTRN.InBondControlNumber;

                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";

                }

                QueryValues[valNo++] = "";

                //-------------------------CSD------------------------------

                if (objCSD != null)
                {

                    QueryValues[valNo++] = objCSD.DeclaredValue;
                    QueryValues[valNo++] = objCSD.ISOCurrencyCode;
                    QueryValues[valNo++] = objCSD.HarmonizedCommodityCode;

                }
                else
                {
                    QueryValues[valNo++] = "0";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";


                }

                //-----------------------FDA--------------------------------

                QueryValues[valNo++] = "";

                //-------------------RFA-------------------------------------
                if (objRFA != null)
                {
                    QueryValues[valNo++] = objRFA.AmendmentCode;
                    QueryValues[valNo++] = objRFA.AmendmentExplanation;
                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                }


                //-------------------DEP------------------------------------
                if (objDEP != null)
                {
                    QueryValues[valNo++] = objDEP.ImportingCarrier;
                    QueryValues[valNo++] = objDEP.FlightNumber;
                    QueryValues[valNo++] = objDEP.DateOfScheduledArrival;
                    QueryValues[valNo++] = objDEP.LiftoffDate;

                    QueryValues[valNo++] = objDEP.LiftoffTime;

                    QueryValues[valNo++] = objDEP.ActualImportingCarrier;
                    QueryValues[valNo++] = objDEP.ActualFlightNumber;

                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";

                }


                //-------------------------------ASN----------------------------

                if (objASN != null)
                {
                    QueryValues[valNo++] = objASN.StatusCode;
                    QueryValues[valNo++] = objASN.ActionExplanation; ;
                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                }




                //----------------------------CSN---------------------------

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "0";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";


                //------------------ERR-----------------------------------

                if (objERR != null)
                {
                    QueryValues[valNo++] = objERR.ErrorCode;
                    QueryValues[valNo++] = objERR.ErrorMessageText;
                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                }



                //-------------------FSQ----------------------------------

                QueryValues[valNo++] = "";



                //-------------------FSC-----------------------------------

                QueryValues[valNo++] = "";


                //------------------TXT------------------------------------

                QueryValues[valNo++] = "";

                //------------------ERF------------------------------------
                if (objERF != null)
                {
                    QueryValues[valNo++] = objERF.ImportingCarrier;
                    QueryValues[valNo++] = objERF.FlightNumber;
                    QueryValues[valNo++] = objERF.Date;
                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                }

                //------------------COMMON---------------------------------------
                QueryValues[valNo++] = EncodedMessage;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = FlightNo;
                QueryValues[valNo++] = FlightDate == DateTime.MinValue ? "01/01/1900" : FlightDate.ToString();
                if (objCCL != null)
                {
                    QueryValues[valNo++] = objCCL.AirportOfArrival;
                }
                else
                {
                    QueryValues[valNo++] = "";
                }

                QueryValues[valNo++] = objWBL != null ? objWBL.ArrivalDatePermitToProceed : string.Empty;

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            //return null;
        }
        #endregion

        #region Updating Customs Messages
        public async Task<bool> UpdateCustomsMessages(object[] QueryValues, string MessageType)
        {
            try
            {
                //string[] QueryNames = new string[87];
                //SqlDbType[] QueryTypes = new SqlDbType[87];
                //int i = 0;
                //QueryNames[i++] = "AWBPrefix";
                //QueryNames[i++] = "AWBNumber";
                //QueryNames[i++] = "MessageType";
                //QueryNames[i++] = "HAWBNumber";
                //QueryNames[i++] = "ConsolidationIdentifier";
                //QueryNames[i++] = "PackageTrackingIdentifier";
                //QueryNames[i++] = "AWBPartArrivalReference";
                //QueryNames[i++] = "ArrivalAirport";
                //QueryNames[i++] = "AirCarrier";
                //QueryNames[i++] = "Origin";
                //QueryNames[i++] = "DestinionCode";
                //QueryNames[i++] = "WBLNumberOfPieces";
                //QueryNames[i++] = "WBLWeightIndicator";
                //QueryNames[i++] = "WBLWeight";
                //QueryNames[i++] = "WBLCargoDescription";
                //QueryNames[i++] = "ArrivalDate";
                //QueryNames[i++] = "PartArrivalReference";
                //QueryNames[i++] = "BoardedQuantityIdentifier";
                //QueryNames[i++] = "BoardedPieceCount";
                //QueryNames[i++] = "BoardedWeight";
                //QueryNames[i++] = "ARRWeightCode";
                //QueryNames[i++] = "ImportingCarrier";
                //QueryNames[i++] = "FlightNumber";
                //QueryNames[i++] = "ARRPartArrivalReference";
                //QueryNames[i++] = "RequestType";
                //QueryNames[i++] = "RequestExplanation";
                //QueryNames[i++] = "EntryType";
                //QueryNames[i++] = "EntryNumber";
                //QueryNames[i++] = "AMSParticipantCode";
                //QueryNames[i++] = "ShipperName";
                //QueryNames[i++] = "ShipperAddress";
                //QueryNames[i++] = "ShipperCity";
                //QueryNames[i++] = "ShipperState";
                //QueryNames[i++] = "ShipperCountry";
                //QueryNames[i++] = "ShipperPostalCode";
                //QueryNames[i++] = "ConsigneeName";
                //QueryNames[i++] = "ConsigneeAddress";
                //QueryNames[i++] = "ConsigneeCity";
                //QueryNames[i++] = "ConsigneeState";
                //QueryNames[i++] = "ConsigneeCountry";
                //QueryNames[i++] = "ConsigneePostalCode";
                //QueryNames[i++] = "TransferDestAirport";
                //QueryNames[i++] = "DomesticIdentifier";
                //QueryNames[i++] = "BondedCarrierID";
                //QueryNames[i++] = "OnwardCarrier";
                //QueryNames[i++] = "BondedPremisesIdentifier";
                //QueryNames[i++] = "InBondControlNumber";
                //QueryNames[i++] = "OriginOfGoods";
                //QueryNames[i++] = "DeclaredValue";
                //QueryNames[i++] = "CurrencyCode";
                //QueryNames[i++] = "CommodityCode";
                //QueryNames[i++] = "LineIdentifier";
                //QueryNames[i++] = "AmendmentCode";
                //QueryNames[i++] = "AmendmentExplanation";
                //QueryNames[i++] = "DeptImportingCarrier";
                //QueryNames[i++] = "DeptFlightNumber";
                //QueryNames[i++] = "DeptScheduledArrivalDate";
                //QueryNames[i++] = "LiftoffDate";
                //QueryNames[i++] = "LiftoffTime";
                //QueryNames[i++] = "DeptActualImportingCarrier";
                //QueryNames[i++] = "DeptActualFlightNumber";
                //QueryNames[i++] = "ASNStatusCode";
                //QueryNames[i++] = "ASNActionExplanation";
                //QueryNames[i++] = "CSNActionCode";
                //QueryNames[i++] = "CSNPieces";
                //QueryNames[i++] = "TransactionDate";
                //QueryNames[i++] = "TransactionTime";
                //QueryNames[i++] = "CSNEntryType";
                //QueryNames[i++] = "CSNEntryNumber";
                //QueryNames[i++] = "CSNRemarks";
                //QueryNames[i++] = "ErrorCode";
                //QueryNames[i++] = "ErrorMessage";
                //QueryNames[i++] = "StatusRequestCode";
                //QueryNames[i++] = "StatusAnswerCode";
                //QueryNames[i++] = "Information";
                //QueryNames[i++] = "ERFImportingCarrier";
                //QueryNames[i++] = "ERFFlightNumber";
                //QueryNames[i++] = "ERFDate";
                //QueryNames[i++] = "Message";
                //QueryNames[i++] = "UpdatedOn";
                //QueryNames[i++] = "UpdatedBy";
                //QueryNames[i++] = "CreatedOn";
                //QueryNames[i++] = "CreatedBy";
                //QueryNames[i++] = "FlightNo";
                //QueryNames[i++] = "FlightDate";
                //QueryNames[i++] = "ControlLocation";
                //QueryNames[i++] = "WBLArrivalDatePermitToProceed";

                //int j = 0;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.Int;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.Decimal;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.Int;
                //QueryTypes[j++] = SqlDbType.Decimal;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.BigInt;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.DateTime;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.DateTime;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;
                //QueryTypes[j++] = SqlDbType.VarChar;

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = QueryValues[0] },
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = QueryValues[1] },
                    new SqlParameter("@MessageType", SqlDbType.VarChar) { Value = QueryValues[2] },
                    new SqlParameter("@HAWBNumber", SqlDbType.VarChar) { Value = QueryValues[3] },
                    new SqlParameter("@ConsolidationIdentifier", SqlDbType.VarChar) { Value = QueryValues[4] },
                    new SqlParameter("@PackageTrackingIdentifier", SqlDbType.VarChar) { Value = QueryValues[5] },
                    new SqlParameter("@AWBPartArrivalReference", SqlDbType.VarChar) { Value = QueryValues[6] },
                    new SqlParameter("@ArrivalAirport", SqlDbType.VarChar) { Value = QueryValues[7] },
                    new SqlParameter("@AirCarrier", SqlDbType.VarChar) { Value = QueryValues[8] },
                    new SqlParameter("@Origin", SqlDbType.VarChar) { Value = QueryValues[9] },
                    new SqlParameter("@DestinionCode", SqlDbType.VarChar) { Value = QueryValues[10] },
                    new SqlParameter("@WBLNumberOfPieces", SqlDbType.Int) { Value = QueryValues[11] },
                    new SqlParameter("@WBLWeightIndicator", SqlDbType.VarChar) { Value = QueryValues[12] },
                    new SqlParameter("@WBLWeight", SqlDbType.Decimal) { Value = QueryValues[13] },
                    new SqlParameter("@WBLCargoDescription", SqlDbType.VarChar) { Value = QueryValues[14] },
                    new SqlParameter("@ArrivalDate", SqlDbType.VarChar) { Value = QueryValues[15] },
                    new SqlParameter("@PartArrivalReference", SqlDbType.VarChar) { Value = QueryValues[16] },
                    new SqlParameter("@BoardedQuantityIdentifier", SqlDbType.VarChar) { Value = QueryValues[17] },
                    new SqlParameter("@BoardedPieceCount", SqlDbType.Int) { Value = QueryValues[18] },
                    new SqlParameter("@BoardedWeight", SqlDbType.Decimal) { Value = QueryValues[19] },
                    new SqlParameter("@ARRWeightCode", SqlDbType.VarChar) { Value = QueryValues[20] },
                    new SqlParameter("@ImportingCarrier", SqlDbType.VarChar) { Value = QueryValues[21] },
                    new SqlParameter("@FlightNumber", SqlDbType.VarChar) { Value = QueryValues[22] },
                    new SqlParameter("@ARRPartArrivalReference", SqlDbType.VarChar) { Value = QueryValues[23] },
                    new SqlParameter("@RequestType", SqlDbType.VarChar) { Value = QueryValues[24] },
                    new SqlParameter("@RequestExplanation", SqlDbType.VarChar) { Value = QueryValues[25] },
                    new SqlParameter("@EntryType", SqlDbType.VarChar) { Value = QueryValues[26] },
                    new SqlParameter("@EntryNumber", SqlDbType.VarChar) { Value = QueryValues[27] },
                    new SqlParameter("@AMSParticipantCode", SqlDbType.VarChar) { Value = QueryValues[28] },
                    new SqlParameter("@ShipperName", SqlDbType.VarChar) { Value = QueryValues[29] },
                    new SqlParameter("@ShipperAddress", SqlDbType.VarChar) { Value = QueryValues[30] },
                    new SqlParameter("@ShipperCity", SqlDbType.VarChar) { Value = QueryValues[31] },
                    new SqlParameter("@ShipperState", SqlDbType.VarChar) { Value = QueryValues[32] },
                    new SqlParameter("@ShipperCountry", SqlDbType.VarChar) { Value = QueryValues[33] },
                    new SqlParameter("@ShipperPostalCode", SqlDbType.VarChar) { Value = QueryValues[34] },
                    new SqlParameter("@ConsigneeName", SqlDbType.VarChar) { Value = QueryValues[35] },
                    new SqlParameter("@ConsigneeAddress", SqlDbType.VarChar) { Value = QueryValues[36] },
                    new SqlParameter("@ConsigneeCity", SqlDbType.VarChar) { Value = QueryValues[37] },
                    new SqlParameter("@ConsigneeState", SqlDbType.VarChar) { Value = QueryValues[38] },
                    new SqlParameter("@ConsigneeCountry", SqlDbType.VarChar) { Value = QueryValues[39] },
                    new SqlParameter("@ConsigneePostalCode", SqlDbType.VarChar) { Value = QueryValues[40] },
                    new SqlParameter("@TransferDestAirport", SqlDbType.VarChar) { Value = QueryValues[41] },
                    new SqlParameter("@DomesticIdentifier", SqlDbType.VarChar) { Value = QueryValues[42] },
                    new SqlParameter("@BondedCarrierID", SqlDbType.VarChar) { Value = QueryValues[43] },
                    new SqlParameter("@OnwardCarrier", SqlDbType.VarChar) { Value = QueryValues[44] },
                    new SqlParameter("@BondedPremisesIdentifier", SqlDbType.VarChar) { Value = QueryValues[45] },
                    new SqlParameter("@InBondControlNumber", SqlDbType.VarChar) { Value = QueryValues[46] },
                    new SqlParameter("@OriginOfGoods", SqlDbType.VarChar) { Value = QueryValues[47] },
                    new SqlParameter("@DeclaredValue", SqlDbType.VarChar) { Value = QueryValues[48] },
                    new SqlParameter("@CurrencyCode", SqlDbType.VarChar) { Value = QueryValues[49] },
                    new SqlParameter("@CommodityCode", SqlDbType.VarChar) { Value = QueryValues[50] },
                    new SqlParameter("@LineIdentifier", SqlDbType.VarChar) { Value = QueryValues[51] },
                    new SqlParameter("@AmendmentCode", SqlDbType.VarChar) { Value = QueryValues[52] },
                    new SqlParameter("@AmendmentExplanation", SqlDbType.VarChar) { Value = QueryValues[53] },
                    new SqlParameter("@DeptImportingCarrier", SqlDbType.VarChar) { Value = QueryValues[54] },
                    new SqlParameter("@DeptFlightNumber", SqlDbType.VarChar) { Value = QueryValues[55] },
                    new SqlParameter("@DeptScheduledArrivalDate", SqlDbType.VarChar) { Value = QueryValues[56] },
                    new SqlParameter("@LiftoffDate", SqlDbType.VarChar) { Value = QueryValues[57] },
                    new SqlParameter("@LiftoffTime", SqlDbType.VarChar) { Value = QueryValues[58] },
                    new SqlParameter("@DeptActualImportingCarrier", SqlDbType.VarChar) { Value = QueryValues[59] },
                    new SqlParameter("@DeptActualFlightNumber", SqlDbType.VarChar) { Value = QueryValues[60] },
                    new SqlParameter("@ASNStatusCode", SqlDbType.VarChar) { Value = QueryValues[61] },
                    new SqlParameter("@ASNActionExplanation", SqlDbType.VarChar) { Value = QueryValues[62] },
                    new SqlParameter("@CSNActionCode", SqlDbType.VarChar) { Value = QueryValues[63] },
                    new SqlParameter("@CSNPieces", SqlDbType.BigInt) { Value = QueryValues[64] },
                    new SqlParameter("@TransactionDate", SqlDbType.VarChar) { Value = QueryValues[65] },
                    new SqlParameter("@TransactionTime", SqlDbType.VarChar) { Value = QueryValues[66] },
                    new SqlParameter("@CSNEntryType", SqlDbType.VarChar) { Value = QueryValues[67] },
                    new SqlParameter("@CSNEntryNumber", SqlDbType.VarChar) { Value = QueryValues[68] },
                    new SqlParameter("@CSNRemarks", SqlDbType.VarChar) { Value = QueryValues[69] },
                    new SqlParameter("@ErrorCode", SqlDbType.VarChar) { Value = QueryValues[70] },
                    new SqlParameter("@ErrorMessage", SqlDbType.VarChar) { Value = QueryValues[71] },
                    new SqlParameter("@StatusRequestCode", SqlDbType.VarChar) { Value = QueryValues[72] },
                    new SqlParameter("@StatusAnswerCode", SqlDbType.VarChar) { Value = QueryValues[73] },
                    new SqlParameter("@Information", SqlDbType.VarChar) { Value = QueryValues[74] },
                    new SqlParameter("@ERFImportingCarrier", SqlDbType.VarChar) { Value = QueryValues[75] },
                    new SqlParameter("@ERFFlightNumber", SqlDbType.VarChar) { Value = QueryValues[76] },
                    new SqlParameter("@ERFDate", SqlDbType.VarChar) { Value = QueryValues[77] },
                    new SqlParameter("@Message", SqlDbType.VarChar) { Value = QueryValues[78] },
                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = QueryValues[79] },
                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = QueryValues[80] },
                    new SqlParameter("@CreatedOn", SqlDbType.DateTime) { Value = QueryValues[81] },
                    new SqlParameter("@CreatedBy", SqlDbType.VarChar) { Value = QueryValues[82] },
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = QueryValues[83] },
                    new SqlParameter("@FlightDate", SqlDbType.VarChar) { Value = QueryValues[84] },
                    new SqlParameter("@ControlLocation", SqlDbType.VarChar) { Value = QueryValues[85] },
                    new SqlParameter("@WBLArrivalDatePermitToProceed", SqlDbType.VarChar) { Value = QueryValues[86] }
                };

                if (MessageType == "FRI" || MessageType == "FXI" || MessageType == "FRC" || MessageType == "FXC" || MessageType == "FRX" || MessageType == "FXX" || MessageType == "FDM" || MessageType == "FER" || MessageType == "FSQ" || MessageType == "FSN" || MessageType == "PSN" || MessageType == "PER" || MessageType == "PRI")
                {
                    //if (db.InsertData("SP_UpdateOutboxCustomsMessage", QueryNames, QueryTypes, QueryValues))
                    if (await _readWriteDao.ExecuteNonQueryAsync("SP_UpdateOutboxCustomsMessage", sqlParameters))
                    { return true; }
                    else
                    { return false; }
                }
                else
                {
                    //if (db.InsertData("SP_UpdateInboxCustomsMessage", QueryNames, QueryTypes, QueryValues))
                    if (await _readWriteDao.ExecuteNonQueryAsync("SP_UpdateInboxCustomsMessage", sqlParameters))
                    { return true; }
                    else
                    { return false; }
                }

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return false;
            }
        }
        #endregion

        #region Encoding PSN Message
        public async Task<PSN> EncodingPSNMessage(object[] QueryValues)
        {
            try
            {
                //string[] QueryNames = { "AWBNumber", "HAWBNumber" };
                //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                //StringBuilder sb = new StringBuilder();
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = QueryValues[0] },
                    new SqlParameter("@HAWBNumber", SqlDbType.VarChar) { Value = QueryValues[1] }
                };
                //SQLServer db = new SQLServer();
                DataSet? Dset = new DataSet("Dset_CustomsImportBAL_EncodingPSNMessage");
                //Dset = db.SelectRecords("sp_GetPSNDataAutoMsg_HAWB", QueryNames, QueryValues, QueryTypes);
                Dset = await _readWriteDao.SelectRecords("sp_GetPSNDataAutoMsg_HAWB", sqlParameters);
                if (Dset != null)
                {
                    if (Dset.Tables.Count > 0)
                    {
                        PSN PSN = new PSN();
                        PSN = PSN.Encode(Dset);
                        return PSN;
                    }
                    else
                    {
                        //db = null;
                        Dset.Dispose();
                        return null;
                    }
                }
                else
                {
                    //db = null;
                    Dset = null;
                    return null;
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                //db = null;
                return null;

            }
        }
        #endregion

        public async Task<bool> GeneratePSNMessage(string AWBNumber, string HAWBNumber)
        {
            try
            {
                string AutoPSN = string.Empty;
                //GenericFunction genericFunction = new GenericFunction();

                AutoPSN = _genericFunction.ReadValueFromDb("AutoPSN");

                if (AutoPSN != string.Empty && AutoPSN == "false")
                    return false;

                DataSet dCust = new DataSet("Customsimports_btnSendPSN_dCust");

                object[] QueryVal = { AWBNumber, HAWBNumber };

                object[] QueryValues = new object[87];
                PSN sbPSN = await EncodingPSNMessage(QueryVal);

                readQueryValuesPSN(sbPSN, ref QueryValues, sbPSN.ToString(), string.Empty, DateTime.MinValue, "AutoGeneratedMessage", DateTime.UtcNow);
                if (await UpdateCustomsMessages(QueryValues, sbPSN.StandardMessageIdentifier.StandardMessageIdentifier))
                {

                    if (sbPSN != null)
                    {
                        if (sbPSN.ToString() != "")
                        {
                            #region Wrap SITA Address

                            //GenericFunction GF = new GenericFunction();
                            string SitaMessageHeader = string.Empty, SFTPHeaderSITAddress = string.Empty, FWBMessageversion = string.Empty, Emailaddress = string.Empty, error = string.Empty;
                            DataSet? dsmessage = await _genericFunction.GetSitaAddressandMessageVersion(string.Empty, "PSN", "AIR", string.Empty, string.Empty, string.Empty, string.Empty);

                            //DataSet dsChecmMessageType = GF.GetRecordofSitaAddressandSitaMessageVersionandMessageType("", "", "", "", "FDM");
                            //DataSet dsMsgCongig = GF.GetSitaAddressandMessageVersion(AirlineCode, "FDM", "AIR", "", "", "", string.Empty);
                            if (dsmessage != null && dsmessage.Tables[0].Rows.Count > 0)
                            {

                                if (dsmessage != null && dsmessage.Tables[0].Rows.Count > 0)
                                {
                                    Emailaddress = dsmessage.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                                    string MessageCommunicationType = dsmessage.Tables[0].Rows[0]["MsgCommType"].ToString();
                                    FWBMessageversion = dsmessage.Tables[0].Rows[0]["MessageVersion"].ToString();

                                    if (dsmessage.Tables[0].Rows[0]["PatnerSitaID"].ToString().Trim().Length > 0)
                                        SitaMessageHeader = _genericFunction.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());

                                    if (dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Trim().Length > 0)
                                        SFTPHeaderSITAddress = _genericFunction.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());
                                }

                            }

                            if (SitaMessageHeader != "")
                            {
                                _genericFunction.SaveMessageOutBox("PSN", SitaMessageHeader + "\r\n" + sbPSN.ToString().ToUpper(), "SITAFTP", "SITAFTP", string.Empty, "", string.Empty, string.Empty, AWBNumber);
                                // clsLog.WriteLogAzure("PSN message in SaveMessageOutBox SitaMessageHeader" + DateTime.Now);
                                _logger.LogInformation("PSN message in SaveMessageOutBox SitaMessageHeader {0}", DateTime.Now);
                            }
                            if (SFTPHeaderSITAddress.Trim().Length > 0)
                            {
                                _genericFunction.SaveMessageOutBox("PSN", SFTPHeaderSITAddress + "\r\n" + sbPSN.ToString().ToUpper(), "SFTP", "SFTP", string.Empty, "", string.Empty, string.Empty, AWBNumber);
                                // clsLog.WriteLogAzure("PSN message in SaveMessageOutBox SFTPHeaderSITAddress" + DateTime.Now);
                                _logger.LogInformation("PSN message in SaveMessageOutBox SFTPHeaderSITAddress {0}", DateTime.Now);
                            }
                            if (Emailaddress.Trim().Length > 0)
                            {
                                _genericFunction.SaveMessageOutBox("PSN", sbPSN.ToString().ToUpper(), string.Empty, Emailaddress, string.Empty, "", string.Empty, string.Empty, AWBNumber);
                                // clsLog.WriteLogAzure("PSN message in SaveMessageOutBox Email" + DateTime.Now);
                                _logger.LogInformation("PSN message in SaveMessageOutBox Email {0}", DateTime.Now);
                            }

                            #endregion
                        }

                        // clsLog.WriteLogAzure("PSN Message generated successfully:" + AWBNumber + "-" + HAWBNumber);
                        _logger.LogInformation("PSN Message generated successfully: {0}-{1}", AWBNumber, HAWBNumber);
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return false;
            }
        }

    }

}
