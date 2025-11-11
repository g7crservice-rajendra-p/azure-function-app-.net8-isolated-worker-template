using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
//using QID.DataAccess;
using QidWorkerRole;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace QidWorkerRole
{
    //class FDMMessageProcessor
    public class FDMMessageProcessor// made public as it's used in ASMcs service file
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<FDMMessageProcessor> _logger;

        #region Constructor
        public FDMMessageProcessor(ISqlDataHelperFactory sqlDataHelperFactory,ILogger<FDMMessageProcessor> logger)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
        }
        #endregion
        public FDM objFDM = null;
        DataSet ds;
        //SQLServer db = new SQLServer();

        #region FDM Class
        [Serializable]
        public class FDM
        {
            //Message Type (Note : Three Characters). 
            public SMI StandardMessageIdentifier = new SMI();

            public DEP DEP = new DEP();

            #region Overriding ToString Method for FDM
            public override string ToString()
            {

                try
                {
                    StringBuilder sbFDM = new StringBuilder();
                    //Message Identifier
                    sbFDM.AppendLine(StandardMessageIdentifier.StandardMessageIdentifier);

                    if (!String.IsNullOrEmpty(DEP.ComponentIdentifier))
                    {
                        String strTemp = "";

                        strTemp += DEP.ComponentIdentifier;
                        if (!String.IsNullOrEmpty(DEP.ImportingCarrier))
                        {
                            strTemp += "/" + DEP.ImportingCarrier;
                        }
                        if (!String.IsNullOrEmpty(DEP.FlightNumber))
                        {
                            strTemp += DEP.FlightNumber;
                        }
                        if (!String.IsNullOrEmpty(DEP.DateOfScheduledArrival))
                        {
                            strTemp += "/" + DEP.DateOfScheduledArrival;
                        }
                        if (!String.IsNullOrEmpty(DEP.LiftoffDate))
                        {
                            strTemp += "/" + DEP.LiftoffDate + DEP.LiftoffTime;
                        }
                        if (!String.IsNullOrEmpty(DEP.ActualImportingCarrier) || !String.IsNullOrEmpty(DEP.ActualFlightNumber))
                        {
                            strTemp += "/" + DEP.ActualImportingCarrier + DEP.ActualFlightNumber;
                        }
                        sbFDM.AppendLine(strTemp);
                    }

                    return sbFDM.ToString();
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex);
                    return string.Empty;
                }
            }
            #endregion

            public FDM Encode(DataSet ds)
            {
                try
                {
                    FDM FDM = new FDM();
                    if (ds != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                foreach (DataRow row in ds.Tables[0].Rows)
                                {

                                    FDM.StandardMessageIdentifier.StandardMessageIdentifier = row["StandardMessageIdentifier"].ToString();
                                }

                                foreach (DataRow row in ds.Tables[1].Rows)
                                {

                                    FDM.DEP.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FDM.DEP.ImportingCarrier = row["ImportingCarrier"].ToString();
                                    FDM.DEP.FlightNumber = row["FlightNumber"].ToString();
                                    FDM.DEP.DateOfScheduledArrival = row["DateOfScheduledArrival"].ToString();
                                    FDM.DEP.LiftoffDate = row["LiftoffDate"].ToString();
                                    FDM.DEP.LiftoffTime = row["LiftoffTime"].ToString();
                                    FDM.DEP.ActualImportingCarrier = row["ActualImportingCarrier"].ToString();
                                    FDM.DEP.ActualFlightNumber = row["ActualFlightNumber"].ToString();
                                }
                                return FDM;

                            }

                        }

                    }
                    return null;
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex);
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

        #region Update Method for FDM object
        public void readQueryValuesFDM(FDM objFDM, ref object[] QueryValues, string EncodedMessage, string AWBPrefix, string AWBNumber, string FlightNo, DateTime FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {

                SMI objSMI = objFDM.StandardMessageIdentifier;
                //AWB1 objAWB1 = null;
                //CCL objCCL = null;
                //WBL objWBL = null;
                //ARR1 objARR = null;
                //AGT objAGT = null;
                //SHP objSHP = null;
                //CNE objCNE = null;
                //TRN objTRN = null;
                //CSD objCSD = null;
                //RFA objRFA = null;
                DEP objDEP = objFDM.DEP;
                //ERR objERR = null;
                //ERF objERF = null;





                //String strMsg = txtMsgType.Text.Trim().ToUpper();

                //--------------------AWB----------------------------------
                //if (objAWB1 != null) 
                //{
                //    QueryValues[valNo++] = objAWB1.AWBPrefix.Trim();
                //    QueryValues[valNo++] = objAWB1.AWBNumber.Trim();
                //    QueryValues[valNo++] = objSMI.StandardMessageIdentifier;
                //    QueryValues[valNo++] = objAWB1.HAWBNumber.Trim();
                //    QueryValues[valNo++] = objAWB1.ConsolidationIdentifier.Trim();
                //    QueryValues[valNo++] = objAWB1.PackageTrackingIdentifier;

                //    QueryValues[valNo++] = "";//   only for fsq ans fsc txtPartArrival.Text.Trim();
                //}
                //else
                //{
                QueryValues[valNo++] = AWBPrefix;
                QueryValues[valNo++] = AWBNumber;
                QueryValues[valNo++] = objSMI.StandardMessageIdentifier;
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                //}


                //-----------------CCL-----------------------

                //if (objCCL != null) 
                //{
                //    QueryValues[valNo++] = objCCL.AirportOfArrival;
                //    QueryValues[valNo++] = objCCL.CargoTerminalOperator;
                //}
                //else
                //{
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                //}



                //---------------------WBL-----------




                //if (objWBL != null)  //Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.
                //{


                //    QueryValues[valNo++] = objWBL.AirportOfOrigin.Trim();
                //    QueryValues[valNo++] = objWBL.PermitToProceedDestAirport;
                //    QueryValues[valNo++] = objWBL.NumberOfPieces;
                //    QueryValues[valNo++] = objWBL.WeightCode.Trim();
                //    QueryValues[valNo++] = objWBL.Weight;
                //    QueryValues[valNo++] = objWBL.CargoDescription;

                //}
                //else
                //{
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "0";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "0";
                QueryValues[valNo++] = "";
                //}

                //------------------ARR------------------------------
                //if (objARR != null)
                //{
                //    QueryValues[valNo++] = objARR.ScheduledArrivalDate;
                //    QueryValues[valNo++] = objARR.PartArrivalReference;
                //    QueryValues[valNo++] = objARR.BoardedQuantityIdentifier;
                //    QueryValues[valNo++] = objARR.BoardedPieceCount;
                //    QueryValues[valNo++] = objARR.Weight;
                //    QueryValues[valNo++] = objARR.WeightCode;
                //    QueryValues[valNo++] = objARR.ImportingCarrier;
                //    QueryValues[valNo++] = objARR.FlightNumber;
                //    QueryValues[valNo++] = objARR.PartArrivalReference;

                //}
                //else
                //{
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "0";
                QueryValues[valNo++] = "0";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";

                //}
                //------------------------HLD---------------

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";




                //-------------------------CED--------------------------

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";



                //----------------------AGT----------------------
                //if (objAGT != null)
                //{
                //    QueryValues[valNo++] = objAGT.AirAMSParticipantCode;
                //}
                //else
                //{
                QueryValues[valNo++] = "";
                //}


                //---------------SHP-------------------------------

                //if (objSHP != null)
                //{
                //    QueryValues[valNo++] = objSHP.Name;
                //    QueryValues[valNo++] = objSHP.StreetAddress;
                //    QueryValues[valNo++] = objSHP.City;
                //    QueryValues[valNo++] = objSHP.State;
                //    QueryValues[valNo++] = objSHP.CountryCode;
                //    QueryValues[valNo++] = objSHP.PostalCode;
                //}
                //else
                //{
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                //}


                //-----------------CNE-------------------------

                //if (objCNE != null)
                //{
                //    QueryValues[valNo++] = objCNE.Name;
                //    QueryValues[valNo++] = objCNE.StreetAddress;
                //    QueryValues[valNo++] = objCNE.City;
                //    QueryValues[valNo++] = objCNE.State;
                //    QueryValues[valNo++] = objCNE.CountryCode;
                //    QueryValues[valNo++] = objCNE.PostalCode;

                //}
                //else
                //{

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";

                //}


                //------------------TRN--------------------------
                //if (objTRN != null)
                //{
                //    QueryValues[valNo++] = objTRN.DestinationAirport;
                //    QueryValues[valNo++] = objTRN.DomesticInternationIdentifier;
                //    QueryValues[valNo++] = objTRN.BondedCarrierID;
                //    QueryValues[valNo++] = objTRN.OnwardCarrier;
                //    QueryValues[valNo++] = objTRN.BondedPremisesIdentifier;
                //    QueryValues[valNo++] = objTRN.InBondControlNumber;

                //}
                //else
                //{
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";

                //}

                QueryValues[valNo++] = "";

                //-------------------------CSD------------------------------

                //if (objCSD != null)
                //{

                //    QueryValues[valNo++] = objCSD.DeclaredValue;
                //    QueryValues[valNo++] = objCSD.ISOCurrencyCode;
                //    QueryValues[valNo++] = objCSD.HarmonizedCommodityCode;

                //}
                //else
                //{
                QueryValues[valNo++] = "0";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";


                //}

                //-----------------------FDA--------------------------------

                QueryValues[valNo++] = "";

                //-------------------RFA-------------------------------------
                //if (objRFA != null)
                //{
                //    QueryValues[valNo++] = objRFA.AmendmentCode;
                //    QueryValues[valNo++] = objRFA.AmendmentExplanation;
                //}
                //else
                //{
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                //}


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

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";



                //----------------------------CSN---------------------------

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";


                //------------------ERR-----------------------------------

                //if (objERR != null)
                //{
                //    QueryValues[valNo++] = objERR.ErrorCode;
                //    QueryValues[valNo++] = objERR.ErrorMessageText;
                //}
                //else
                //{
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                //}



                //-------------------FSQ----------------------------------

                QueryValues[valNo++] = "";




                //-------------------FSC-----------------------------------

                QueryValues[valNo++] = "";


                //------------------TXT------------------------------------

                QueryValues[valNo++] = "";

                //------------------ERF------------------------------------
                //if (objERF != null)
                //{
                //    QueryValues[valNo++] = objERF.ImportingCarrier;
                //    QueryValues[valNo++] = objERF.FlightNumber;
                //    QueryValues[valNo++] = objERF.Date;
                //}
                //else
                //{
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                //    }

                //------------------COMMON---------------------------------------
                QueryValues[valNo++] = EncodedMessage;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = FlightNo;
                QueryValues[valNo++] = FlightDate;
                //if (objCCL != null)
                //{
                //    QueryValues[valNo++] = objCCL.AirportOfArrival;
                //}
                //else
                //{
                QueryValues[valNo++] = "";
                //}

                QueryValues[valNo++] = string.Empty;

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
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
                var parameters = new SqlParameter[]
                {
                    new("@AWBPrefix", SqlDbType.VarChar) { Value = QueryValues[0] },
                    new("@AWBNumber", SqlDbType.VarChar) { Value = QueryValues[1] },
                    new("@MessageType", SqlDbType.VarChar) { Value = QueryValues[2] },
                    new("@HAWBNumber", SqlDbType.VarChar) { Value = QueryValues[3] },
                    new("@ConsolidationIdentifier", SqlDbType.VarChar) { Value = QueryValues[4] },
                    new("@PackageTrackingIdentifier", SqlDbType.VarChar) { Value = QueryValues[5] },
                    new("@AWBPartArrivalReference", SqlDbType.VarChar) { Value = QueryValues[6] },
                    new("@ArrivalAirport", SqlDbType.VarChar) { Value = QueryValues[7] },
                    new("@AirCarrier", SqlDbType.VarChar) { Value = QueryValues[8] },
                    new("@Origin", SqlDbType.VarChar) { Value = QueryValues[9] },
                    new("@DestinionCode", SqlDbType.VarChar) { Value = QueryValues[10] },
                    new("@WBLNumberOfPieces", SqlDbType.Int) { Value = QueryValues[11] },
                    new("@WBLWeightIndicator", SqlDbType.VarChar) { Value = QueryValues[12] },
                    new("@WBLWeight", SqlDbType.Decimal) { Value = QueryValues[13] },
                    new("@WBLCargoDescription", SqlDbType.VarChar) { Value = QueryValues[14] },
                    new("@ArrivalDate", SqlDbType.VarChar) { Value = QueryValues[15] },
                    new("@PartArrivalReference", SqlDbType.VarChar) { Value = QueryValues[16] },
                    new("@BoardedQuantityIdentifier", SqlDbType.VarChar) { Value = QueryValues[17] },
                    new("@BoardedPieceCount", SqlDbType.Int) { Value = QueryValues[18] },
                    new("@BoardedWeight", SqlDbType.Decimal) { Value = QueryValues[19] },
                    new("@ARRWeightCode", SqlDbType.VarChar) { Value = QueryValues[20] },
                    new("@ImportingCarrier", SqlDbType.VarChar) { Value = QueryValues[21] },
                    new("@FlightNumber", SqlDbType.VarChar) { Value = QueryValues[22] },
                    new("@ARRPartArrivalReference", SqlDbType.VarChar) { Value = QueryValues[23] },
                    new("@RequestType", SqlDbType.VarChar) { Value = QueryValues[24] },
                    new("@RequestExplanation", SqlDbType.VarChar) { Value = QueryValues[25] },
                    new("@EntryType", SqlDbType.VarChar) { Value = QueryValues[26] },
                    new("@EntryNumber", SqlDbType.VarChar) { Value = QueryValues[27] },
                    new("@AMSParticipantCode", SqlDbType.VarChar) { Value = QueryValues[28] },
                    new("@ShipperName", SqlDbType.VarChar) { Value = QueryValues[29] },
                    new("@ShipperAddress", SqlDbType.VarChar) { Value = QueryValues[30] },
                    new("@ShipperCity", SqlDbType.VarChar) { Value = QueryValues[31] },
                    new("@ShipperState", SqlDbType.VarChar) { Value = QueryValues[32] },
                    new("@ShipperCountry", SqlDbType.VarChar) { Value = QueryValues[33] },
                    new("@ShipperPostalCode", SqlDbType.VarChar) { Value = QueryValues[34] },
                    new("@ConsigneeName", SqlDbType.VarChar) { Value = QueryValues[35] },
                    new("@ConsigneeAddress", SqlDbType.VarChar) { Value = QueryValues[36] },
                    new("@ConsigneeCity", SqlDbType.VarChar) { Value = QueryValues[37] },
                    new("@ConsigneeState", SqlDbType.VarChar) { Value = QueryValues[38] },
                    new("@ConsigneeCountry", SqlDbType.VarChar) { Value = QueryValues[39] },
                    new("@ConsigneePostalCode", SqlDbType.VarChar) { Value = QueryValues[40] },
                    new("@TransferDestAirport", SqlDbType.VarChar) { Value = QueryValues[41] },
                    new("@DomesticIdentifier", SqlDbType.VarChar) { Value = QueryValues[42] },
                    new("@BondedCarrierID", SqlDbType.VarChar) { Value = QueryValues[43] },
                    new("@OnwardCarrier", SqlDbType.VarChar) { Value = QueryValues[44] },
                    new("@BondedPremisesIdentifier", SqlDbType.VarChar) { Value = QueryValues[45] },
                    new("@InBondControlNumber", SqlDbType.VarChar) { Value = QueryValues[46] },
                    new("@OriginOfGoods", SqlDbType.VarChar) { Value = QueryValues[47] },
                    new("@DeclaredValue", SqlDbType.BigInt) { Value = QueryValues[48] },
                    new("@CurrencyCode", SqlDbType.VarChar) { Value = QueryValues[49] },
                    new("@CommodityCode", SqlDbType.VarChar) { Value = QueryValues[50] },
                    new("@LineIdentifier", SqlDbType.VarChar) { Value = QueryValues[51] },
                    new("@AmendmentCode", SqlDbType.VarChar) { Value = QueryValues[52] },
                    new("@AmendmentExplanation", SqlDbType.VarChar) { Value = QueryValues[53] },
                    new("@DeptImportingCarrier", SqlDbType.VarChar) { Value = QueryValues[54] },
                    new("@DeptFlightNumber", SqlDbType.VarChar) { Value = QueryValues[55] },
                    new("@DeptScheduledArrivalDate", SqlDbType.VarChar) { Value = QueryValues[56] },
                    new("@LiftoffDate", SqlDbType.VarChar) { Value = QueryValues[57] },
                    new("@LiftoffTime", SqlDbType.VarChar) { Value = QueryValues[58] },
                    new("@DeptActualImportingCarrier", SqlDbType.VarChar) { Value = QueryValues[59] },
                    new("@DeptActualFlightNumber", SqlDbType.VarChar) { Value = QueryValues[60] },
                    new("@ASNStatusCode", SqlDbType.VarChar) { Value = QueryValues[61] },
                    new("@ASNActionExplanation", SqlDbType.VarChar) { Value = QueryValues[62] },
                    new("@CSNActionCode", SqlDbType.VarChar) { Value = QueryValues[63] },
                    new("@CSNPieces", SqlDbType.Int) { Value = QueryValues[64] },
                    new("@TransactionDate", SqlDbType.VarChar) { Value = QueryValues[65] },
                    new("@TransactionTime", SqlDbType.VarChar) { Value = QueryValues[66] },
                    new("@CSNEntryType", SqlDbType.VarChar) { Value = QueryValues[67] },
                    new("@CSNEntryNumber", SqlDbType.VarChar) { Value = QueryValues[68] },
                    new("@CSNRemarks", SqlDbType.VarChar) { Value = QueryValues[69] },
                    new("@ErrorCode", SqlDbType.VarChar) { Value = QueryValues[70] },
                    new("@ErrorMessage", SqlDbType.VarChar) { Value = QueryValues[71] },
                    new("@StatusRequestCode", SqlDbType.VarChar) { Value = QueryValues[72] },
                    new("@StatusAnswerCode", SqlDbType.VarChar) { Value = QueryValues[73] },
                    new("@Information", SqlDbType.VarChar) { Value = QueryValues[74] },
                    new("@ERFImportingCarrier", SqlDbType.VarChar) { Value = QueryValues[75] },
                    new("@ERFFlightNumber", SqlDbType.VarChar) { Value = QueryValues[76] },
                    new("@ERFDate", SqlDbType.VarChar) { Value = QueryValues[77] },
                    new("@Message", SqlDbType.NVarChar) { Value = QueryValues[78] },
                    new("@UpdatedOn", SqlDbType.DateTime) { Value = QueryValues[79] },
                    new("@UpdatedBy", SqlDbType.VarChar) { Value = QueryValues[80] },
                    new("@CreatedOn", SqlDbType.DateTime) { Value = QueryValues[81] },
                    new("@CreatedBy", SqlDbType.VarChar) { Value = QueryValues[82] },
                    new("@FlightNo", SqlDbType.VarChar) { Value = QueryValues[83] },
                    new("@FlightDate", SqlDbType.DateTime) { Value = QueryValues[84] },
                    new("@ControlLocation", SqlDbType.VarChar) { Value = QueryValues[85] },
                    new("@WBLArrivalDatePermitToProceed", SqlDbType.VarChar) { Value = QueryValues[86] },
                };

                if (MessageType == "FRI" || MessageType == "FXI" || MessageType == "FRC" || MessageType == "FXC" || MessageType == "FRX" || MessageType == "FXX" || MessageType == "FDM" || MessageType == "FER" || MessageType == "FSQ" || MessageType == "FSN" || MessageType == "PSN" || MessageType == "PER" || MessageType == "PRI")
                {
                    //if (db.InsertData("SP_UpdateOutboxCustomsMessage", QueryNames, QueryTypes, QueryValues))
                    if (await _readWriteDao.ExecuteNonQueryAsync("SP_UpdateOutboxCustomsMessage", parameters))
                    { return true; }
                    else
                    { return false; }
                }
                else
                {
                    //if (db.InsertData("SP_UpdateInboxCustomsMessage", QueryNames, QueryTypes, QueryValues))
                    if (await _readWriteDao.ExecuteNonQueryAsync("SP_UpdateOutboxCustomsMessage", parameters))
                    { return true; }
                    else
                    { return false; }
                }

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return false;
            }
        }
        #endregion

        #region Encoding FDM Message
        public FDM EncodingFDMMessage(object[] QueryValues)
        {
            //SQLServer db = new SQLServer();
            DataSet Dset = new DataSet("Dset_CustomsImportsBAL_EncodingFDMMessage");
            try
            {
                //string[] QueryNames = { "AWBNumber", "FlightNo", "FlightDate", "FlightOrigin" };
                //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar };

                //Dset = db.SelectRecords("sp_GetFDMDataAutoMsg_HAWB", QueryNames, QueryValues, QueryTypes);

                var parameters = new SqlParameter[]
                {
                 new("@AWBNumber", SqlDbType.VarChar) { Value = QueryValues[0] },
                 new("@FlightNo", SqlDbType.VarChar) { Value = QueryValues[1] },
                 new("@FlightDate", SqlDbType.DateTime) { Value = QueryValues[2] },
                 new("@FlightOrigin", SqlDbType.VarChar) { Value = QueryValues[3] }
                };
                if (Dset != null && Dset.Tables.Count > 0)
                {
                    FDM FDM = new FDM();
                    FDM = FDM.Encode(Dset);
                    //db = null;
                    return FDM;
                }
                else
                {
                    //db = null;
                    return null;
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                //db = null;
                return null;

            }
            //finally
            //{
            //    if (Dset != null)
            //        Dset.Dispose();
            //}
        }
        #endregion

        public async Task<DataSet> CheckCustomsAWBAvailabilityFDM(object[] QueryValues)
        {
            try
            {
                //string[] QueryNames = new string[3];
                //SqlDbType[] QueryTypes = new SqlDbType[3];

                //QueryNames[0] = "FlightNo";
                //QueryNames[1] = "FlightDate";
                //QueryNames[2] = "FlightOrigin";

                //QueryTypes[0] = SqlDbType.VarChar;
                //QueryTypes[1] = SqlDbType.DateTime;
                //QueryTypes[2] = SqlDbType.VarChar;
                var parameters = new SqlParameter[]
                {
                 new("@FlightNo", SqlDbType.VarChar) { Value = QueryValues[0] },
                 new("@FlightDate", SqlDbType.DateTime) { Value = QueryValues[1] },
                 new("@FlightOrigin", SqlDbType.VarChar) { Value = QueryValues[2] },
                };

                //ds = db.SelectRecords("sp_CheckCustomsApplicabilityFDM", QueryNames, QueryValues, QueryTypes);
                ds = await _readWriteDao.SelectRecords("sp_CheckCustomsApplicabilityFDM", parameters);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {

                    return ds;

                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return null;
            }
        }
        public async Task<DateTime> GettingLocalFlightDate(object[] QueryValues)
        {
            try
            {
                //string[] QueryNames = new string[3];
                //SqlDbType[] QueryTypes = new SqlDbType[3];

                //QueryNames[0] = "FlightNo";
                //QueryNames[1] = "FlightDate";
                //QueryNames[2] = "FlightOrigin";

                //QueryTypes[0] = SqlDbType.VarChar;
                //QueryTypes[1] = SqlDbType.DateTime;
                //QueryTypes[2] = SqlDbType.VarChar;
                var parameters = new SqlParameter[]
                {
                 new("@FlightNo", SqlDbType.VarChar) { Value = QueryValues[0] },
                 new("@FlightDate", SqlDbType.DateTime) { Value = QueryValues[1] },
                 new("@FlightOrigin", SqlDbType.VarChar) { Value = QueryValues[2] },
                };

                //ds = db.SelectRecords("uspGetLocalFlightDate", QueryNames, QueryValues, QueryTypes);
                ds = await _readWriteDao.SelectRecords("uspGetLocalFlightDate", parameters);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return Convert.ToDateTime(ds.Tables[0].Rows[0]["FlightDate"].ToString());
                }
                else
                {
                    return Convert.ToDateTime(QueryValues[1]);
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return Convert.ToDateTime(QueryValues[1]);
            }
        }


        public async Task<bool> GenerateFDMMessage(string FlightNo, DateTime FlightDate, string FlightOrigin)
        {
            try
            {
                #region FDM On Flight Level
                //string[] QueryNames = { "FlightNo", "FlightDate", "FlightOrigin" };
                //object[] QueryVal = { ddlFightDesignator.Text.Trim() + txtFlightID.Text.Trim(), txtFlightDate.Value, (Session["Station"] != null ? Session["Station"].ToString() : string.Empty) };
                object[] QueryVals = { FlightNo, FlightDate, FlightOrigin };
                //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar };

                //Getting the Local Flight Date to work with


                DataSet dCust = new DataSet("Customsimports_btnSendFDM_dCust");
                FlightDate = await GettingLocalFlightDate(QueryVals);

                object[] QueryVal = { FlightNo, FlightDate, FlightOrigin };

                dCust = await CheckCustomsAWBAvailabilityFDM(QueryVal);


                if (dCust.Tables[0].Rows.Count > 0 && dCust.Tables[0].Rows[0]["Validate"].ToString() == "True")
                {
                    object[] QueryValues = new object[87];
                    QueryVal = null;
                    //object[] QueryValFDM = { string.Empty, FlightNo, txtFlightDate.Value, (Session["Station"] != null ? Session["Station"].ToString() : string.Empty) };
                    object[] QueryValFDM = { string.Empty, FlightNo, FlightDate, FlightOrigin, true };
                    FDM sbFDM = EncodingFDMMessage(QueryValFDM);

                    readQueryValuesFDM(sbFDM, ref QueryValues, sbFDM.ToString(), string.Empty, string.Empty, FlightNo, FlightDate, "AutoGeneratedMessage", DateTime.UtcNow);
                    if (sbFDM != null && await UpdateCustomsMessages(QueryValues, sbFDM.StandardMessageIdentifier.StandardMessageIdentifier))
                    {
                        if (sbFDM.ToString() != "")
                        {
                            #region Wrap SITA Address

                            GenericFunction GF = new GenericFunction();
                            string SitaMessageHeader = string.Empty, SFTPHeaderSITAddress = string.Empty, FWBMessageversion = string.Empty, Emailaddress = string.Empty, error = string.Empty;
                            DataSet dsmessage = GF.GetSitaAddressandMessageVersion(FlightNo.Substring(0, 2), "FDM", "AIR", FlightOrigin, string.Empty, FlightNo, string.Empty);

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
                                        SitaMessageHeader = GF.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());

                                    if (dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Trim().Length > 0)
                                        SFTPHeaderSITAddress = GF.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());
                                }

                            }

                            if (SitaMessageHeader != "")
                            {
                                GF.SaveMessageOutBox("FDM", SitaMessageHeader + "\r\n" + sbFDM.ToString().ToUpper(), "SITAFTP", "SITAFTP", FlightOrigin, "", FlightNo, FlightDate.ToString(), "");
                                clsLog.WriteLogAzure("FDM message in SaveMessageOutBox SitaMessageHeader" + DateTime.Now);
                            }
                            if (SFTPHeaderSITAddress.Trim().Length > 0)
                            {
                                GF.SaveMessageOutBox("FDM", SFTPHeaderSITAddress + "\r\n" + sbFDM.ToString().ToUpper(), "SFTP", "SFTP", FlightOrigin, "", FlightNo, FlightDate.ToString(), "");
                                clsLog.WriteLogAzure("FDM message in SaveMessageOutBox SFTPHeaderSITAddress" + DateTime.Now);
                            }
                            if (Emailaddress.Trim().Length > 0)
                            {
                                GF.SaveMessageOutBox("FDM", sbFDM.ToString().ToUpper(), string.Empty, Emailaddress, FlightOrigin, "", FlightNo, FlightDate.ToString(), "");
                                clsLog.WriteLogAzure("FDM message in SaveMessageOutBox Email" + DateTime.Now);
                            }

                            #endregion
                        }

                        clsLog.WriteLogAzure("FDM Message generated successfully:" + FlightNo + "-" + FlightDate.ToString());
                        return true;
                    }
                    else
                        return false;

                }
                else
                {
                    clsLog.WriteLogAzure("FDM cannot be sent as the flight is not applicable for customs:" + FlightNo + "-" + FlightDate.ToString());
                    return false;
                }


                #endregion
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return false;
            }
        }

    }

}
