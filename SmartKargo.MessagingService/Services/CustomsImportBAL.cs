using System;
using System.Text;
using QID.DataAccess;
using System.Data;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using QidWorkerRole;

namespace BAL
{
    public class CustomsImportBAL
    {
        public FRI objFRI = null;
        public FXI objFXI = null;
        public FRC objFRC = null;
        public FSN objFSN = null;
        public FDM objFDM = null;
        public FRX objFRX = null;
        public FER objFER = null;
        public PRI objPRI = null;
        public PSN objPSN = null;
        public FSQ objFSQ = null;


        SQLServer db = new SQLServer();
        DataSet ds;

        #region Listing Custom AWB's
        public DataSet GetCustomsAWBList(object[] QueryValues)
        {
            try
            {
                string[] QueryNames = new string[7];
                SqlDbType[] QueryTypes = new SqlDbType[7];

                QueryNames[0] = "AWBPrefix";
                QueryNames[1] = "AWBNumber";
                QueryNames[2] = "FlightNumber";
                QueryNames[3] = "FlightDate";
                QueryNames[4] = "ULD";
                QueryNames[5] = "DestAirport";
                QueryNames[6] = "ListType";

                QueryTypes[0] = SqlDbType.VarChar;
                QueryTypes[1] = SqlDbType.VarChar;
                QueryTypes[2] = SqlDbType.VarChar;
                QueryTypes[3] = SqlDbType.DateTime;
                QueryTypes[4] = SqlDbType.VarChar;
                QueryTypes[5] = SqlDbType.VarChar;
                QueryTypes[6] = SqlDbType.VarChar;

                ds = db.SelectRecords("SP_GetCustomsAWBList", QueryNames, QueryValues, QueryTypes);
                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            return ds;
                        }
                        else
                        {
                            return null;
                        }

                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return null;
            }
        }
        #endregion

        #region Updating Customs Messages

        public bool UpdateCustomsMessages(object[] QueryValues, string MessageType)
        {
            try
            {
                string[] QueryNames = new string[101];
                SqlDbType[] QueryTypes = new SqlDbType[101];
                int i = 0;
                QueryNames[i++] = "AWBPrefix";
                QueryNames[i++] = "AWBNumber";
                QueryNames[i++] = "MessageType";
                QueryNames[i++] = "HAWBNumber";
                QueryNames[i++] = "ConsolidationIdentifier";
                QueryNames[i++] = "PackageTrackingIdentifier";
                QueryNames[i++] = "AWBPartArrivalReference";
                QueryNames[i++] = "ArrivalAirport";
                QueryNames[i++] = "AirCarrier";
                QueryNames[i++] = "Origin";
                QueryNames[i++] = "DestinionCode";
                QueryNames[i++] = "WBLNumberOfPieces";
                QueryNames[i++] = "WBLWeightIndicator";
                QueryNames[i++] = "WBLWeight";
                QueryNames[i++] = "WBLCargoDescription";
                QueryNames[i++] = "ArrivalDate";
                QueryNames[i++] = "PartArrivalReference";
                QueryNames[i++] = "BoardedQuantityIdentifier";
                QueryNames[i++] = "BoardedPieceCount";
                QueryNames[i++] = "BoardedWeight";
                QueryNames[i++] = "ARRWeightCode";
                QueryNames[i++] = "ImportingCarrier";
                QueryNames[i++] = "FlightNumber";
                QueryNames[i++] = "ARRPartArrivalReference";
                QueryNames[i++] = "RequestType";
                QueryNames[i++] = "RequestExplanation";
                QueryNames[i++] = "EntryType";
                QueryNames[i++] = "EntryNumber";
                QueryNames[i++] = "AMSParticipantCode";
                QueryNames[i++] = "ShipperName";
                QueryNames[i++] = "ShipperAddress";
                QueryNames[i++] = "ShipperCity";
                QueryNames[i++] = "ShipperState";
                QueryNames[i++] = "ShipperCountry";
                QueryNames[i++] = "ShipperPostalCode";
                QueryNames[i++] = "ConsigneeName";
                QueryNames[i++] = "ConsigneeAddress";
                QueryNames[i++] = "ConsigneeCity";
                QueryNames[i++] = "ConsigneeState";
                QueryNames[i++] = "ConsigneeCountry";
                QueryNames[i++] = "ConsigneePostalCode";
                QueryNames[i++] = "TransferDestAirport";
                QueryNames[i++] = "DomesticIdentifier";
                QueryNames[i++] = "BondedCarrierID";
                QueryNames[i++] = "OnwardCarrier";
                QueryNames[i++] = "BondedPremisesIdentifier";
                QueryNames[i++] = "InBondControlNumber";
                QueryNames[i++] = "OriginOfGoods";
                QueryNames[i++] = "DeclaredValue";
                QueryNames[i++] = "CurrencyCode";
                QueryNames[i++] = "CommodityCode";
                QueryNames[i++] = "LineIdentifier";
                QueryNames[i++] = "AmendmentCode";
                QueryNames[i++] = "AmendmentExplanation";
                QueryNames[i++] = "DeptImportingCarrier";
                QueryNames[i++] = "DeptFlightNumber";
                QueryNames[i++] = "DeptScheduledArrivalDate";
                QueryNames[i++] = "LiftoffDate";
                QueryNames[i++] = "LiftoffTime";
                QueryNames[i++] = "DeptActualImportingCarrier";
                QueryNames[i++] = "DeptActualFlightNumber";
                QueryNames[i++] = "ASNStatusCode";
                QueryNames[i++] = "ASNActionExplanation";
                QueryNames[i++] = "CSNActionCode";
                QueryNames[i++] = "CSNPieces";
                QueryNames[i++] = "TransactionDate";
                QueryNames[i++] = "TransactionTime";
                QueryNames[i++] = "CSNEntryType";
                QueryNames[i++] = "CSNEntryNumber";
                QueryNames[i++] = "CSNRemarks";
                QueryNames[i++] = "ErrorCode";
                QueryNames[i++] = "ErrorMessage";
                QueryNames[i++] = "StatusRequestCode";
                QueryNames[i++] = "StatusAnswerCode";
                QueryNames[i++] = "Information";
                QueryNames[i++] = "ERFImportingCarrier";
                QueryNames[i++] = "ERFFlightNumber";
                QueryNames[i++] = "ERFDate";
                QueryNames[i++] = "Message";
                QueryNames[i++] = "UpdatedOn";
                QueryNames[i++] = "UpdatedBy";
                QueryNames[i++] = "CreatedOn";
                QueryNames[i++] = "CreatedBy";
                QueryNames[i++] = "FlightNo";
                QueryNames[i++] = "FlightDate";
                QueryNames[i++] = "ControlLocation";
                QueryNames[i++] = "WBLArrivalDatePermitToProceed";

                //---------------------------OPI------------------
                QueryNames[i++] = "PartyType";
                QueryNames[i++] = "PartyInfoType";
                QueryNames[i++] = "PartyInfo";
                QueryNames[i++] = "OPIName";
                QueryNames[i++] = "OPIStreetAddress";
                QueryNames[i++] = "OPICity";
                QueryNames[i++] = "OPIState";
                QueryNames[i++] = "OPICountryCode";
                QueryNames[i++] = "OPIPostalCode";
                QueryNames[i++] = "OPITelephoneNumber";
                //---------------------------OCI------------------
                QueryNames[i++] = "OCICountryCode";
                QueryNames[i++] = "InformationIdentifier";
                QueryNames[i++] = "CustomsInfoIdentifier";
                QueryNames[i++] = "SupplementaryInfo";

                int j = 0;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.Int;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.Decimal;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.Int;
                QueryTypes[j++] = SqlDbType.Decimal;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.BigInt;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.DateTime;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.DateTime;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                /////////////////////////////Newly ADDED Columns for OCI & OPI/////////////////////////////////////////////////
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;

                if (MessageType == "FRI" || MessageType == "FXI" || MessageType == "FRC" || MessageType == "FXC" ||
                    MessageType == "FRX" || MessageType == "FXX" || MessageType == "FDM" || MessageType == "FER" ||
                    MessageType == "FSQ" || MessageType == "FSN" || MessageType == "PSN" || MessageType == "PER" || MessageType == "PRI")
                {
                    if (db.InsertData("SP_UpdateOutboxCustomsMessage", QueryNames, QueryTypes, QueryValues))
                    { return true; }
                    else
                    { return false; }
                }
                else
                {
                    if (db.InsertData("SP_UpdateInboxCustomsMessage", QueryNames, QueryTypes, QueryValues))
                    { return true; }
                    else
                    { return false; }
                }

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return false;
            }
        }

        public bool UpdateCustomsMessages_PRI(object[] QueryValues, string MessageType)
        {
            try
            {
                string[] QueryNames = new string[101];
                SqlDbType[] QueryTypes = new SqlDbType[101];
                int i = 0;
                QueryNames[i++] = "AWBPrefix";
                QueryNames[i++] = "AWBNumber";
                QueryNames[i++] = "MessageType";
                QueryNames[i++] = "HAWBNumber";
                QueryNames[i++] = "ConsolidationIdentifier";
                QueryNames[i++] = "PackageTrackingIdentifier";
                QueryNames[i++] = "AWBPartArrivalReference";
                QueryNames[i++] = "ArrivalAirport";
                QueryNames[i++] = "AirCarrier";
                QueryNames[i++] = "Origin";
                QueryNames[i++] = "DestinionCode";
                QueryNames[i++] = "WBLNumberOfPieces";
                QueryNames[i++] = "WBLWeightIndicator";
                QueryNames[i++] = "WBLWeight";
                QueryNames[i++] = "WBLCargoDescription";
                QueryNames[i++] = "ArrivalDate";
                QueryNames[i++] = "PartArrivalReference";
                QueryNames[i++] = "BoardedQuantityIdentifier";
                QueryNames[i++] = "BoardedPieceCount";
                QueryNames[i++] = "BoardedWeight";
                QueryNames[i++] = "ARRWeightCode";
                QueryNames[i++] = "ImportingCarrier";
                QueryNames[i++] = "FlightNumber";
                QueryNames[i++] = "ARRPartArrivalReference";
                QueryNames[i++] = "RequestType";
                QueryNames[i++] = "RequestExplanation";
                QueryNames[i++] = "EntryType";
                QueryNames[i++] = "EntryNumber";
                QueryNames[i++] = "AMSParticipantCode";
                QueryNames[i++] = "ShipperName";
                QueryNames[i++] = "ShipperAddress";
                QueryNames[i++] = "ShipperCity";
                QueryNames[i++] = "ShipperState";
                QueryNames[i++] = "ShipperCountry";
                QueryNames[i++] = "ShipperPostalCode";
                QueryNames[i++] = "ConsigneeName";
                QueryNames[i++] = "ConsigneeAddress";
                QueryNames[i++] = "ConsigneeCity";
                QueryNames[i++] = "ConsigneeState";
                QueryNames[i++] = "ConsigneeCountry";
                QueryNames[i++] = "ConsigneePostalCode";
                QueryNames[i++] = "TransferDestAirport";
                QueryNames[i++] = "DomesticIdentifier";
                QueryNames[i++] = "BondedCarrierID";
                QueryNames[i++] = "OnwardCarrier";
                QueryNames[i++] = "BondedPremisesIdentifier";
                QueryNames[i++] = "InBondControlNumber";
                QueryNames[i++] = "OriginOfGoods";
                QueryNames[i++] = "DeclaredValue";
                QueryNames[i++] = "CurrencyCode";
                QueryNames[i++] = "CommodityCode";
                QueryNames[i++] = "LineIdentifier";
                QueryNames[i++] = "AmendmentCode";
                QueryNames[i++] = "AmendmentExplanation";
                QueryNames[i++] = "DeptImportingCarrier";
                QueryNames[i++] = "DeptFlightNumber";
                QueryNames[i++] = "DeptScheduledArrivalDate";
                QueryNames[i++] = "LiftoffDate";
                QueryNames[i++] = "LiftoffTime";
                QueryNames[i++] = "DeptActualImportingCarrier";
                QueryNames[i++] = "DeptActualFlightNumber";
                QueryNames[i++] = "ASNStatusCode";
                QueryNames[i++] = "ASNActionExplanation";
                QueryNames[i++] = "CSNActionCode";
                QueryNames[i++] = "CSNPieces";
                QueryNames[i++] = "TransactionDate";
                QueryNames[i++] = "TransactionTime";
                QueryNames[i++] = "CSNEntryType";
                QueryNames[i++] = "CSNEntryNumber";
                QueryNames[i++] = "CSNRemarks";
                QueryNames[i++] = "ErrorCode";
                QueryNames[i++] = "ErrorMessage";
                QueryNames[i++] = "StatusRequestCode";
                QueryNames[i++] = "StatusAnswerCode";
                QueryNames[i++] = "Information";
                QueryNames[i++] = "ERFImportingCarrier";
                QueryNames[i++] = "ERFFlightNumber";
                QueryNames[i++] = "ERFDate";
                QueryNames[i++] = "Message";
                QueryNames[i++] = "UpdatedOn";
                QueryNames[i++] = "UpdatedBy";
                QueryNames[i++] = "CreatedOn";
                QueryNames[i++] = "CreatedBy";
                QueryNames[i++] = "FlightNo";
                QueryNames[i++] = "FlightDate";
                QueryNames[i++] = "ControlLocation";
                QueryNames[i++] = "WBLArrivalDatePermitToProceed";

                //---------------------------OPI------------------
                QueryNames[i++] = "PartyType";
                QueryNames[i++] = "PartyInfoType";
                QueryNames[i++] = "PartyInfo";
                QueryNames[i++] = "OPIName";
                QueryNames[i++] = "OPIStreetAddress";
                QueryNames[i++] = "OPICity";
                QueryNames[i++] = "OPIState";
                QueryNames[i++] = "OPICountryCode";
                QueryNames[i++] = "OPIPostalCode";
                QueryNames[i++] = "OPITelephoneNumber";
                //---------------------------OCI------------------
                QueryNames[i++] = "OCICountryCode";
                QueryNames[i++] = "InformationIdentifier";
                QueryNames[i++] = "CustomsInfoIdentifier";
                QueryNames[i++] = "SupplementaryInfo";

                int j = 0;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.Int;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.Decimal;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.Int;
                QueryTypes[j++] = SqlDbType.Decimal;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.BigInt;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.DateTime;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.DateTime;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                /////////////////////////////Newly ADDED Columns for OCI & OPI/////////////////////////////////////////////////
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;
                QueryTypes[j++] = SqlDbType.VarChar;

                if (MessageType == "FRI" || MessageType == "FXI" || MessageType == "FRC" || MessageType == "FXC" || MessageType == "FRX" || MessageType == "FXX" || MessageType == "FDM" || MessageType == "FER" || MessageType == "FSQ" || MessageType == "FSN" || MessageType == "PSN" || MessageType == "PER" || MessageType == "PRI")
                {
                    if (db.InsertData("SP_UpdateOutboxCustomsMessage", QueryNames, QueryTypes, QueryValues))
                    { return true; }
                    else
                    { return false; }
                }
                else
                {
                    if (db.InsertData("SP_UpdateInboxCustomsMessage", QueryNames, QueryTypes, QueryValues))
                    { return true; }
                    else
                    { return false; }
                }

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return false;
            }
        }


        #endregion

        #region Fetch Customs AMS Data AWBWise

        public DataSet FetchCustomsAWBDetails(object[] QueryValues)
        {
            try
            {
                string[] QueryNames = new string[6];
                //object[] QueryValues = new object[2];
                SqlDbType[] QueryTypes = new SqlDbType[6];

                QueryNames[0] = "AWBPrefix";
                QueryNames[1] = "AWBNumber";
                QueryNames[2] = "FlightNo";
                QueryNames[3] = "FlightDate";
                QueryNames[4] = "HAWBNumber";
                QueryNames[5] = "FlightOrigin";

                QueryTypes[0] = SqlDbType.VarChar;
                QueryTypes[1] = SqlDbType.VarChar;
                QueryTypes[2] = SqlDbType.VarChar;
                QueryTypes[3] = SqlDbType.DateTime;
                QueryTypes[4] = SqlDbType.VarChar;
                QueryTypes[5] = SqlDbType.VarChar;


                ds = db.SelectRecords("SP_GetCustomsMessagingData", QueryNames, QueryValues, QueryTypes);
                if (ds != null && ds.Tables.Count > 1 && ds.Tables[2].Rows.Count > 0)
                {
                    return ds;
                }
                else
                { return null; }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return null;
            }
        }
        #endregion

        #region Check Customs AWB Availability

        public DataSet CheckCustomsAWBAvailability(object[] QueryValues)
        {
            try
            {
                string[] QueryNames = new string[4];
                SqlDbType[] QueryTypes = new SqlDbType[4];

                QueryNames[0] = "AWBNumber";
                QueryNames[1] = "FlightNo";
                QueryNames[2] = "FlightDate";
                QueryNames[3] = "FlightOrigin";

                QueryTypes[0] = SqlDbType.VarChar;
                QueryTypes[1] = SqlDbType.VarChar;
                QueryTypes[2] = SqlDbType.DateTime;
                QueryTypes[3] = SqlDbType.VarChar;


                ds = db.SelectRecords("sp_CheckCustomsApplicability", QueryNames, QueryValues, QueryTypes);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return ds;
                }
                else
                { return null; }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return null;
            }
        }

        public DataSet CheckCustomsAWBAvailabilityFDM(object[] QueryValues)
        {
            try
            {
                string[] QueryNames = new string[3];
                SqlDbType[] QueryTypes = new SqlDbType[3];

                QueryNames[0] = "FlightNo";
                QueryNames[1] = "FlightDate";
                QueryNames[2] = "FlightOrigin";

                QueryTypes[0] = SqlDbType.VarChar;
                QueryTypes[1] = SqlDbType.DateTime;
                QueryTypes[2] = SqlDbType.VarChar;


                ds = db.SelectRecords("sp_CheckCustomsApplicabilityFDM", QueryNames, QueryValues, QueryTypes);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return ds;
                }
                else
                { return null; }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return null;
            }
        }


        public DataSet CheckAWBSentToCustoms(object[] QueryValues)
        {
            try
            {
                string[] QueryNames = new string[3];
                SqlDbType[] QueryTypes = new SqlDbType[3];

                QueryNames[0] = "AWBOrigin";
                QueryNames[1] = "Station";
                QueryNames[2] = "AWBNumber";

                QueryTypes[0] = SqlDbType.VarChar;
                QueryTypes[1] = SqlDbType.VarChar;
                QueryTypes[2] = SqlDbType.VarChar;


                ds = db.SelectRecords("SP_CheckAWBSentToCustoms", QueryNames, QueryValues, QueryTypes);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return ds;
                }
                else
                { return null; }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return null;
            }
        }

        #endregion

        #region Encoding Messages

        #region Encoding FRI Message
        public FRI EncodingFRIMessage(object[] QueryValues)
        {
            DataSet Dset = null;
            try
            {
                string[] QueryNames = { "AWBNumber", "FlightNo", "FlightDate", "HAWBNumber", "FlightOrigin" };
                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar };
                StringBuilder[] sb = new StringBuilder[0];
                SQLServer db = new SQLServer();
                Dset = new DataSet("Dset_CustomsImportBAL_EncodingFRIMessage");
                Dset = db.SelectRecords("sp_GetFRIDataAutoMsg_HAWB", QueryNames, QueryValues, QueryTypes);
                if (Dset != null && Dset.Tables.Count > 0)
                {
                    FRI EncodeFRI = new FRI();
                    EncodeFRI = EncodeFRI.Encode(Dset);
                    return EncodeFRI;

                }
                else
                {
                    db = null;
                    return null;
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                db = null;
                return null;
            }
            finally
            {
                if (Dset != null)
                    Dset.Dispose();
            }
        }
        #endregion

        #region Encoding FDM Message
        public FDM EncodingFDMMessage(object[] QueryValues)
        {
            DataSet Dset = null;
            try
            {
                string[] QueryNames = { "AWBNumber", "FlightNo", "FlightDate", "FlightOrigin" };
                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar };

                SQLServer db = new SQLServer();
                Dset = new DataSet("Dset_CustomsImportsBAL_EncodingFDMMessage");
                Dset = db.SelectRecords("sp_GetFDMDataAutoMsg_HAWB", QueryNames, QueryValues, QueryTypes);
                if (Dset != null && Dset.Tables.Count > 0)
                {
                    FDM FDM = new FDM();
                    FDM = FDM.Encode(Dset);
                    db = null;
                    Dset.Dispose();
                    return FDM;

                }
                else
                {
                    db = null;
                    return null;
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                db = null;
                return null;

            }
            finally
            {
                if (Dset != null)
                    Dset.Dispose();
            }
        }
        #endregion

        #region Encoding FSN Message
        public FSN EncodingFSNMessage(object[] QueryValues)
        {
            DataSet Dset = new DataSet("Dset_CustomsImportBAL_EncodingFSNMessage");
            try
            {
                string[] QueryNames = { "AWBNumber", "FlightNo", "FlightDate", "HAWBNumber", "FlightOrigin" };
                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar };
                StringBuilder sb = new StringBuilder();
                SQLServer db = new SQLServer();
                Dset = db.SelectRecords("sp_GetFSNDataAutoMsg_HAWB", QueryNames, QueryValues, QueryTypes);
                if (Dset != null && Dset.Tables.Count > 0)
                {
                    FSN FSN = new FSN();
                    FSN = FSN.Encode(Dset);
                    return FSN;

                }
                else
                {
                    db = null;
                    return null;
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                db = null;
                return null;
            }
            finally
            {
                if (Dset != null)
                    Dset.Dispose();
            }
        }
        #endregion

        #region Encoding FRC Message
        public FRC EncodingFRCMessage(object[] QueryValues)
        {
            SQLServer db = new SQLServer();
            DataSet Dset = new DataSet("Dset_CustomsImportBAL_EncodingFRCMessage");

            try
            {
                string[] QueryNames = { "AWBNumber", "FlightNo", "FlightDate", "HAWBNumber", "FlightOrigin" };
                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar };
                StringBuilder sb = new StringBuilder();
                Dset = db.SelectRecords("sp_GetFRCDataAutoMsg_HAWB", QueryNames, QueryValues, QueryTypes);
                if (Dset != null && Dset.Tables.Count > 0)
                {
                    FRC FRC = new FRC();
                    FRC = FRC.Encode(Dset);
                    return FRC;


                }
                else
                {
                    db = null;
                    return null;
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                db = null;
                return null;
            }
            finally
            {
                if (Dset != null)
                    Dset.Dispose();
            }
        }
        public FRC EncodingFRCMessage(object[] QueryValues, string Event)
        {
            SQLServer db = new SQLServer();
            DataSet Dset = new DataSet("Dset_CustomsImportBAL_EncodingFRCMessage");
            try
            {
                string[] QueryNames = { "AWBNumber", "FlightNo", "FlightDate", "HAWBNumber", "FlightOrigin" };
                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar };
                StringBuilder sb = new StringBuilder();

                string StoredProcedure = string.Empty;


                switch (Event)
                {
                    case "Arrival":
                        {
                            StoredProcedure = "sp_GetFRCDataAutoMsg_HAWB_Import";
                            break;
                        }
                    case "Booking":
                        {
                            StoredProcedure = "sp_GetFRCDataAutoMsg_HAWB_Booking";
                            break;
                        }
                    default:
                        {
                            StoredProcedure = "sp_GetFRCDataAutoMsg_HAWB";
                            break;
                        }
                }


                Dset = db.SelectRecords(StoredProcedure, QueryNames, QueryValues, QueryTypes);
                if (Dset != null && Dset.Tables.Count > 0)
                {
                    FRC FRC = new FRC();
                    FRC = FRC.Encode(Dset);
                    return FRC;
                }
                else
                {
                    db = null;
                    return null;
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                db = null;
                return null;
            }
            finally
            {
                if (Dset != null)
                    Dset.Dispose();
            }
        }
        #endregion

        #region Encoding FRX Message
        public FRX EncodingFRXMessage(object[] QueryValues)
        {
            DataSet Dset = new DataSet("Dset_CustomsImportBAL_EncodingFRXMessage");
            try
            {
                string[] QueryNames = { "AWBNumber", "FlightNo", "FlightDate", "HAWBNumber", "FlightOrigin" };
                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar };
                StringBuilder sb = new StringBuilder();
                SQLServer db = new SQLServer();
                Dset = db.SelectRecords("sp_GetFRXDataAutoMsg_HAWB", QueryNames, QueryValues, QueryTypes);
                if (Dset != null && Dset.Tables.Count > 0)
                {
                    FRX FRX = new FRX();
                    FRX = FRX.Encode(Dset);
                    return FRX;

                }
                else
                {
                    db = null;
                    return null;
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                db = null;
                return null;

            }
            finally
            {
                if (Dset != null)
                    Dset.Dispose();
            }
        }
        public FRX EncodingFRXMessage(object[] QueryValues, string Event)
        {
            DataSet Dset = new DataSet("Dset_CustomsImportBAL_EncodingFRXMessage");
            try
            {
                string[] QueryNames = { "AWBNumber", "FlightNo", "FlightDate", "HAWBNumber", "FlightOrigin" };
                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar };
                StringBuilder sb = new StringBuilder();
                SQLServer db = new SQLServer();

                string StoredProcedure = string.Empty;


                switch (Event)
                {
                    case "Arrival":
                        {
                            StoredProcedure = "sp_GetFRXDataAutoMsg_HAWB_Import";
                            break;
                        }
                    case "Booking":
                        {
                            StoredProcedure = "sp_GetFRXDataAutoMsg_HAWB_HAWB_Booking";
                            break;
                        }
                    default:
                        {
                            StoredProcedure = "sp_GetFRXDataAutoMsg_HAWB";
                            break;
                        }
                }

                Dset = db.SelectRecords(StoredProcedure, QueryNames, QueryValues, QueryTypes);
                if (Dset != null && Dset.Tables.Count > 0)
                {
                    FRX FRX = new FRX();
                    FRX = FRX.Encode(Dset);
                    return FRX;

                }
                else
                {
                    db = null;
                    return null;
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                db = null;
                return null;

            }
            finally
            {
                if (Dset != null)
                    Dset.Dispose();
            }
        }
        #endregion


        #region Encoding FSQ Message
        public FSQ EncodingFSQMessage(object[] QueryValues)
        {
            DataSet Dset = new DataSet("Dset_CustomsImportBAL_EncodingFSQMessage");
            try
            {
                string[] QueryNames = { "AWBNumber", "FlightNo", "FlightDate", "HAWBNumber", "FlightOrigin" };
                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar };
                StringBuilder sb = new StringBuilder();
                SQLServer db = new SQLServer();
                Dset = db.SelectRecords("sp_GetFSQDataAutoMsg_HAWB", QueryNames, QueryValues, QueryTypes);
                if (Dset != null && Dset.Tables.Count > 0)
                {
                    FSQ FSQ = new FSQ();
                    FSQ = FSQ.Encode(Dset);
                    return FSQ;


                }
                else
                {
                    db = null;
                    return null;
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                db = null;
                return null;

            }
            finally
            {
                if (Dset != null)
                    Dset.Dispose();
            }
        }
        #endregion

        #region Encoding PRI Messag
        public PRI EncodingPRIMessage(object[] QueryValues)
        {
            DataSet Dset = new DataSet("Dset_CustomsImportBAL_EncodingPRIMessage");
            try
            {
                string[] QueryNames = { "AWBNumber", "FlightNo", "FlightDate", "HAWBNumber" };
                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar };
                StringBuilder[] sb = new StringBuilder[0];
                SQLServer db = new SQLServer();
                Dset = db.SelectRecords("sp_GetPRIDataAutoMsg_HAWB", QueryNames, QueryValues, QueryTypes);
                if (Dset != null && Dset.Tables.Count > 0)
                {
                    PRI EncodePRI = new PRI();
                    EncodePRI = EncodePRI.Encode(Dset);
                    return EncodePRI;


                }
                else
                {
                    db = null;
                    return null;
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                db = null;
                return null;

            }
            finally
            {
                if (Dset != null)
                    Dset.Dispose();
            }
        }
        #endregion

        #region Encoding PSN Message
        public PSN EncodingPSNMessage(object[] QueryValues)
        {
            DataSet Dset = new DataSet("Dset_CustomsImportBAL_EncodingPSNMessage");
            try
            {
                string[] QueryNames = { "AWBNumber", "FlightNo", "FlightDate", "HAWBNumber", "FlightOrigin" };
                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar };
                StringBuilder sb = new StringBuilder();
                SQLServer db = new SQLServer();
                Dset = db.SelectRecords("sp_GetPSNDataAutoMsg_HAWB", QueryNames, QueryValues, QueryTypes);
                if (Dset != null && Dset.Tables.Count > 0)
                {
                    PSN PSN = new PSN();
                    PSN = PSN.Encode(Dset);
                    return PSN;

                }
                else
                {
                    db = null;
                    return null;
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                db = null;
                return null;

            }
            finally
            {
                if (Dset != null)
                    Dset.Dispose();
            }
        }
        #endregion


        #endregion

        #region Check FRX Message Validity after Offloading

        public DataSet CheckFRXValidityOffload(object[] QueryValues)
        {
            try
            {
                string[] QueryNames = new string[3];
                SqlDbType[] QueryTypes = new SqlDbType[3];

                QueryNames[0] = "AWBNumber";
                QueryNames[1] = "FlightNo";
                QueryNames[2] = "FlightDate";

                QueryTypes[0] = SqlDbType.VarChar;
                QueryTypes[1] = SqlDbType.VarChar;
                QueryTypes[2] = SqlDbType.VarChar;

                ds = db.SelectRecords("sp_CheckFRXValidityOffload", QueryNames, QueryValues, QueryTypes);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return ds;

                }
                else
                { return null; }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return null;
            }
        }
        #endregion

        #region Get Customs Messages Email ID's

        public DataSet GetCustomMessagesMailID(object[] QueryValues)
        {
            DataSet ds = new DataSet("ds_CustomsImportBAL_GetCustomMessagesMailID");
            try
            {
                string[] QueryNames = { "MessageType", "FlightNo", "FlightDate" };
                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                ds = db.SelectRecords("sp_GetCustomsMessageEmailID", QueryNames, QueryValues, QueryTypes);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {

                    return ds;


                }
                else
                { return null; }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return null;
            }
            finally
            {
                if (ds != null)
                    ds.Dispose();
            }
        }
        #endregion

        #region Fetch Customs ACAS Data AWBWise

        public DataSet FetchCustomsACASAWBDetails(object[] QueryValues)
        {
            try
            {
                string[] QueryNames = new string[6];
                //object[] QueryValues = new object[2];
                SqlDbType[] QueryTypes = new SqlDbType[6];

                QueryNames[0] = "AWBPrefix";
                QueryNames[1] = "AWBNumber";
                QueryNames[2] = "FlightNo";
                QueryNames[3] = "FlightDate";
                QueryNames[4] = "HAWBNumber";
                QueryNames[5] = "FlightOrigin";

                QueryTypes[0] = SqlDbType.VarChar;
                QueryTypes[1] = SqlDbType.VarChar;
                QueryTypes[2] = SqlDbType.VarChar;
                QueryTypes[3] = SqlDbType.DateTime;
                QueryTypes[4] = SqlDbType.VarChar;
                QueryTypes[5] = SqlDbType.VarChar;


                ds = db.SelectRecords("SP_GetCustomsACASMessagingData", QueryNames, QueryValues, QueryTypes);
                if (ds != null && ds.Tables.Count > 1 && ds.Tables[2].Rows.Count > 0)
                {
                    return ds;

                }
                else
                { return null; }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return null;
            }
        }
        #endregion

        #region Customs Objects

        #region FRI Class
        [Serializable]
        public class FRI
        {
            //Message Type (Note : Three Characters). 
            public SMI StandardMessageIdentifier = new SMI();
            //Cargo Control Location details CCL
            public CCL CCLDetails = new CCL();
            //AWB Details
            public AWB1[] AWBDetails = new AWB1[0];
            //WBL Details
            public WBL[] WBLDetails = new WBL[0];
            //Arrival Details
            public ARR1[] ArrivalDetails = new ARR1[0];
            //Agent Details
            public AGT AgentDetails = new AGT();
            //Shipper Details
            public SHP ShipperDetails = new SHP();
            //Consignee Details
            public CNE ConsigneeDetails = new CNE();
            //Transfer Details
            public TRN[] TransferDetails = new TRN[0];
            //CBP Shipment Description Details
            public CSD CSDDetails = new CSD();
            //FDA Freight Indicatior
            public FDA FDA = new FDA();

            #region Overriding ToString Method for FRI
            public override string ToString()
            {

                try
                {
                    StringBuilder sbFRI = new StringBuilder();
                    //Message Identifier
                    sbFRI.AppendLine(StandardMessageIdentifier.StandardMessageIdentifier);
                    //Cargo Control Location Details
                    if (CCLDetails.AirportOfArrival != string.Empty && CCLDetails.CargoTerminalOperator != string.Empty)
                    {
                        sbFRI.AppendLine(CCLDetails.AirportOfArrival.ToString() + CCLDetails.CargoTerminalOperator);
                    }
                    //AWB Details
                    foreach (AWB1 AWBs in AWBDetails)
                    {
                        sbFRI.AppendLine(AWBs.AWBPrefix + "-" + AWBs.AWBNumber + ((AWBs.ConsolidationIdentifier != string.Empty || AWBs.HAWBNumber != string.Empty) ? "-" + AWBs.ConsolidationIdentifier + AWBs.HAWBNumber : string.Empty) + (AWBs.PackageTrackingIdentifier != string.Empty ? "/" + AWBs.PackageTrackingIdentifier : string.Empty));
                    }
                    //Waybill Details
                    foreach (WBL WBLDet in WBLDetails)
                    {
                        sbFRI.AppendLine(WBLDet.ComponentIdentifier + "/" + WBLDet.AirportOfOrigin + WBLDet.PermitToProceedDestAirport + "/" + WBLDet.ShipmentDescriptionCode + WBLDet.NumberOfPieces + "/" + WBLDet.WeightCode + WBLDet.Weight + "/" + WBLDet.CargoDescription + (WBLDet.ArrivalDatePermitToProceed != string.Empty ? "/" + WBLDet.ArrivalDatePermitToProceed : string.Empty));

                    }
                    //Arrival Details
                    foreach (ARR1 ArrivalDet in ArrivalDetails)
                    {
                        sbFRI.AppendLine(ArrivalDet.ComponentIdentifier + "/" + ArrivalDet.ImportingCarrier + ArrivalDet.FlightNumber + "/" + ArrivalDet.ScheduledArrivalDate +
                            ((ArrivalDet.PartArrivalReference != string.Empty) ? "-" + ArrivalDet.PartArrivalReference : string.Empty) + ((ArrivalDet.BoardedQuantityIdentifier != string.Empty && ArrivalDet.BoardedPieceCount > 0) ? "/" + ArrivalDet.BoardedQuantityIdentifier + ArrivalDet.BoardedPieceCount.ToString() : string.Empty) +
                            ((ArrivalDet.WeightCode != string.Empty && ArrivalDet.Weight > 0) ? "/" + ArrivalDet.WeightCode + ArrivalDet.Weight.ToString() : string.Empty));
                    }
                    //Agent Details
                    if (AgentDetails.AirAMSParticipantCode != string.Empty)
                    {
                        sbFRI.AppendLine(AgentDetails.ComponentIdentifier + "/" + AgentDetails.AirAMSParticipantCode);
                    }

                    //Shipper Details
                    if (ShipperDetails.Name != string.Empty && ShipperDetails.City != string.Empty && ShipperDetails.CountryCode != string.Empty)
                    {
                        sbFRI.AppendLine(ShipperDetails.ComponentIdentifier + "/" + (ShipperDetails.Name.ToString().Length > 35 ? ShipperDetails.Name.ToString().Substring(0, 35) : ShipperDetails.Name));
                        sbFRI.AppendLine("/" + (ShipperDetails.StreetAddress.Length > 35 ? ShipperDetails.StreetAddress.ToString().Substring(0, 35) : ShipperDetails.StreetAddress));
                        sbFRI.AppendLine("/" + ShipperDetails.City + (ShipperDetails.State != string.Empty ? "/" + ShipperDetails.State : string.Empty));
                        sbFRI.AppendLine("/" + ShipperDetails.CountryCode + (ShipperDetails.PostalCode != string.Empty ? "/" + ShipperDetails.PostalCode : string.Empty));

                    }
                    //Consignee Details
                    if (ConsigneeDetails.Name != string.Empty && ConsigneeDetails.City != string.Empty && ConsigneeDetails.CountryCode != string.Empty)
                    {
                        sbFRI.AppendLine(ConsigneeDetails.ComponentIdentifier + "/" + (ConsigneeDetails.Name.Length > 35 ? ConsigneeDetails.Name.ToString().Substring(0, 35) : ConsigneeDetails.Name));
                        sbFRI.AppendLine("/" + (ConsigneeDetails.StreetAddress.Length > 35 ? ConsigneeDetails.StreetAddress.ToString().Substring(0, 35) : ConsigneeDetails.StreetAddress));
                        sbFRI.AppendLine("/" + ConsigneeDetails.City + (ConsigneeDetails.State != string.Empty ? "/" + ConsigneeDetails.State : string.Empty));
                        sbFRI.AppendLine("/" + ConsigneeDetails.CountryCode + (ConsigneeDetails.PostalCode != string.Empty ? "/" + ConsigneeDetails.PostalCode : string.Empty) + (ConsigneeDetails.TelephoneNumber != string.Empty ? "/" + ConsigneeDetails.TelephoneNumber : string.Empty));

                    }
                    //Transfer Details
                    foreach (TRN TransDetails in TransferDetails)
                    {
                        if (TransDetails.DestinationAirport != string.Empty)
                        {
                            sbFRI.AppendLine(TransDetails.ComponentIdentifier + "/" + TransDetails.DestinationAirport + (TransDetails.DestinationAirport != "000" ? "-" + TransDetails.DomesticInternationIdentifier : string.Empty) +
                                (TransDetails.DestinationAirport != "000" ? "/" + TransDetails.BondedCarrierID + TransDetails.OnwardCarrier : string.Empty) +
                                (TransDetails.DestinationAirport != "000" ? "/" + TransDetails.BondedPremisesIdentifier + TransDetails.InBondControlNumber : string.Empty));
                        }
                    }
                    //CBP Shipment Description CSD
                    if (CSDDetails.DeclaredValue != 0 && CSDDetails.ISOCurrencyCode != string.Empty)
                    {
                        sbFRI.AppendLine(CSDDetails.ComponentIdentifier + "/" + CSDDetails.OriginOfGoods + "/" + CSDDetails.DeclaredValue.ToString() +
                            "-" + CSDDetails.ISOCurrencyCode + (CSDDetails.HarmonizedCommodityCode != string.Empty ? "/" + CSDDetails.HarmonizedCommodityCode : string.Empty));
                    }

                    //FDA Freight Indicator
                    if (FDA.FDADetails != string.Empty)
                    {
                        sbFRI.AppendLine(FDA.FDADetails);
                    }

                    return sbFRI.ToString();
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
                    return string.Empty;
                }
            }
            #endregion

            public FRI Encode(DataSet ds)
            {
                try
                {
                    FRI FRI = new FRI();
                    if (ds != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                foreach (DataRow row in ds.Tables[0].Rows)
                                {

                                    FRI.StandardMessageIdentifier.StandardMessageIdentifier = row["StandardMessageIdentifier"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[1].Rows)
                                {

                                    FRI.CCLDetails.AirportOfArrival = row["AirportOfArrival"].ToString();
                                    FRI.CCLDetails.CargoTerminalOperator = row["CargoTerminalOperator"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[2].Rows)
                                {

                                    Array.Resize(ref FRI.AWBDetails, FRI.AWBDetails.Length + 1);
                                    FRI.AWBDetails[FRI.AWBDetails.Length - 1] = new AWB1();
                                    FRI.AWBDetails[FRI.AWBDetails.Length - 1].AWBPrefix = row["AWBPrefix"].ToString();
                                    FRI.AWBDetails[FRI.AWBDetails.Length - 1].AWBNumber = row["AWBNumber"].ToString();
                                    FRI.AWBDetails[FRI.AWBDetails.Length - 1].ConsolidationIdentifier = row["ConsolidationIdentifier"].ToString();
                                    FRI.AWBDetails[FRI.AWBDetails.Length - 1].HAWBNumber = row["HAWBNumber"].ToString();
                                    FRI.AWBDetails[FRI.AWBDetails.Length - 1].PackageTrackingIdentifier = row["PackageTrackingIdentifier"].ToString();

                                }
                                foreach (DataRow row in ds.Tables[3].Rows)
                                {
                                    Array.Resize(ref FRI.WBLDetails, FRI.WBLDetails.Length + 1);
                                    FRI.WBLDetails[FRI.WBLDetails.Length - 1] = new WBL();
                                    FRI.WBLDetails[FRI.WBLDetails.Length - 1].ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FRI.WBLDetails[FRI.WBLDetails.Length - 1].AirportOfOrigin = row["AirportOfOrigin"].ToString();
                                    FRI.WBLDetails[FRI.WBLDetails.Length - 1].ArrivalDatePermitToProceed = row["ArrivalDatePermitToProceed"].ToString();
                                    FRI.WBLDetails[FRI.WBLDetails.Length - 1].CargoDescription = row["CargoDescription"].ToString();
                                    FRI.WBLDetails[FRI.WBLDetails.Length - 1].NumberOfPieces = row["NumberOfPieces"].ToString() != string.Empty ? Convert.ToInt32(row["NumberOfPieces"]) : 0;
                                    FRI.WBLDetails[FRI.WBLDetails.Length - 1].PermitToProceedDestAirport = row["PermitToProceedDestAirport"].ToString();
                                    FRI.WBLDetails[FRI.WBLDetails.Length - 1].ShipmentDescriptionCode = row["ShipmentDescriptionCode"].ToString();
                                    FRI.WBLDetails[FRI.WBLDetails.Length - 1].Weight = row["Weight"].ToString() != string.Empty ? Double.Parse(row["Weight"].ToString()) : 0;
                                    FRI.WBLDetails[FRI.WBLDetails.Length - 1].WeightCode = row["WeightCode"].ToString();

                                }
                                foreach (DataRow row in ds.Tables[4].Rows)
                                {
                                    Array.Resize(ref FRI.ArrivalDetails, FRI.ArrivalDetails.Length + 1);
                                    FRI.ArrivalDetails[FRI.ArrivalDetails.Length - 1] = new ARR1();
                                    FRI.ArrivalDetails[FRI.ArrivalDetails.Length - 1].BoardedPieceCount = row["BoardedPieceCount"].ToString() != string.Empty ? Convert.ToInt32(row["BoardedPieceCount"]) : 0;
                                    FRI.ArrivalDetails[FRI.ArrivalDetails.Length - 1].BoardedQuantityIdentifier = row["BoardedQuantityIdentifier"].ToString();
                                    FRI.ArrivalDetails[FRI.ArrivalDetails.Length - 1].ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FRI.ArrivalDetails[FRI.ArrivalDetails.Length - 1].FlightNumber = row["FlightNumber"].ToString();
                                    FRI.ArrivalDetails[FRI.ArrivalDetails.Length - 1].ImportingCarrier = row["ImportingCarrier"].ToString();
                                    FRI.ArrivalDetails[FRI.ArrivalDetails.Length - 1].PartArrivalReference = row["PartArrivalReference"].ToString();
                                    FRI.ArrivalDetails[FRI.ArrivalDetails.Length - 1].ScheduledArrivalDate = row["ScheduledArrivalDate"].ToString();
                                    FRI.ArrivalDetails[FRI.ArrivalDetails.Length - 1].Weight = row["Weight"].ToString() != string.Empty ? Double.Parse(row["Weight"].ToString()) : 0;
                                    FRI.ArrivalDetails[FRI.ArrivalDetails.Length - 1].WeightCode = row["WeightCode"].ToString();

                                }
                                foreach (DataRow row in ds.Tables[5].Rows)
                                {
                                    FRI.AgentDetails.AirAMSParticipantCode = row["AirAMSParticipantCode"].ToString();
                                    FRI.AgentDetails.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[6].Rows)
                                {
                                    FRI.ShipperDetails.City = row["City"].ToString();
                                    FRI.ShipperDetails.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FRI.ShipperDetails.CountryCode = row["CountryCode"].ToString();
                                    FRI.ShipperDetails.Name = row["Name"].ToString();
                                    FRI.ShipperDetails.PostalCode = row["PostalCode"].ToString();
                                    FRI.ShipperDetails.State = row["State"].ToString();
                                    FRI.ShipperDetails.StreetAddress = row["StreetAddress"].ToString();
                                }

                                foreach (DataRow row in ds.Tables[7].Rows)
                                {
                                    FRI.ConsigneeDetails.City = row["City"].ToString();
                                    FRI.ConsigneeDetails.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FRI.ConsigneeDetails.CountryCode = row["CountryCode"].ToString();
                                    FRI.ConsigneeDetails.Name = row["Name"].ToString();
                                    FRI.ConsigneeDetails.PostalCode = row["PostalCode"].ToString();
                                    FRI.ConsigneeDetails.State = row["State"].ToString();
                                    FRI.ConsigneeDetails.StreetAddress = row["StreetAddress"].ToString();
                                    FRI.ConsigneeDetails.TelephoneNumber = row["TelephoneNumber"].ToString();
                                }

                                foreach (DataRow row in ds.Tables[8].Rows)
                                {
                                    Array.Resize(ref FRI.TransferDetails, FRI.TransferDetails.Length + 1);
                                    FRI.TransferDetails[FRI.TransferDetails.Length - 1] = new TRN();
                                    FRI.TransferDetails[FRI.TransferDetails.Length - 1].BondedCarrierID = row["BondedCarrierID"].ToString();
                                    FRI.TransferDetails[FRI.TransferDetails.Length - 1].BondedPremisesIdentifier = row["BondedPremisesIdentifier"].ToString();
                                    FRI.TransferDetails[FRI.TransferDetails.Length - 1].ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FRI.TransferDetails[FRI.TransferDetails.Length - 1].DestinationAirport = row["DestinationAirport"].ToString();
                                    FRI.TransferDetails[FRI.TransferDetails.Length - 1].DomesticInternationIdentifier = row["DomesticInternationIdentifier"].ToString();
                                    FRI.TransferDetails[FRI.TransferDetails.Length - 1].InBondControlNumber = row["InBondControlNumber"].ToString();
                                    FRI.TransferDetails[FRI.TransferDetails.Length - 1].OnwardCarrier = row["OnwardCarrier"].ToString();

                                }

                                foreach (DataRow row in ds.Tables[9].Rows)
                                {
                                    FRI.CSDDetails.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FRI.CSDDetails.DeclaredValue = row["DeclaredValue"].ToString() != string.Empty ? Double.Parse(row["DeclaredValue"].ToString()) : 0;
                                    FRI.CSDDetails.HarmonizedCommodityCode = row["HarmonizedCommodityCode"].ToString();
                                    FRI.CSDDetails.ISOCurrencyCode = row["ISOCurrencyCode"].ToString();
                                    FRI.CSDDetails.OriginOfGoods = row["OriginOfGoods"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[10].Rows)
                                {
                                    FRI.FDA.FDADetails = row["FDADetails"].ToString();
                                }

                                return FRI;

                            }

                        }

                    }
                    return null;
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
                    return null;
                }
            }


        }
        #endregion

        #region FXI Class
        [Serializable]
        public class FXI
        {
            //Message Type (Note : Three Characters). 
            public SMI StandardMessageIdentifier = new SMI();
            //Cargo Control Location details CCL
            public CCL CCLDetails = new CCL();
            //AWB Details
            public AWB1[] AWBDetails = new AWB1[0];
            //WBL Details
            public WBL[] WBLDetails = new WBL[0];
            //Arrival Details
            public ARR1[] ArrivalDetails = new ARR1[0];
            //Agent Details
            public AGT AgentDetails = new AGT();
            //Shipper Details
            public SHP ShipperDetails = new SHP();
            //Consignee Details
            public CNE ConsigneeDetails = new CNE();
            //Transfer Details
            public TRN[] TransferDetails = new TRN[0];
            //CBP Shipment Description Details
            public CSD CSDDetails = new CSD();
            //FDA Freight Indicatior
            public FDA FDA = new FDA();

            public CED CED = new CED();

            #region Overriding ToString Method for FXI
            public override string ToString()
            {

                try
                {
                    StringBuilder sbFXI = new StringBuilder();
                    //Message Identifier
                    sbFXI.AppendLine(StandardMessageIdentifier.StandardMessageIdentifier);
                    //Cargo Control Location Details
                    if (CCLDetails.AirportOfArrival != string.Empty && CCLDetails.CargoTerminalOperator != string.Empty)
                    {
                        sbFXI.AppendLine(CCLDetails.AirportOfArrival.ToString() + CCLDetails.CargoTerminalOperator);
                    }
                    //AWB Details
                    foreach (AWB1 AWBs in AWBDetails)
                    {
                        sbFXI.AppendLine(AWBs.AWBPrefix + "-" + AWBs.AWBNumber + ((AWBs.ConsolidationIdentifier != string.Empty || AWBs.HAWBNumber != string.Empty) ? "-" + AWBs.ConsolidationIdentifier + AWBs.HAWBNumber : string.Empty) + (AWBs.PackageTrackingIdentifier != string.Empty ? "/" + AWBs.PackageTrackingIdentifier : string.Empty));
                    }
                    //Waybill Details
                    foreach (WBL WBLDet in WBLDetails)
                    {
                        sbFXI.AppendLine(WBLDet.ComponentIdentifier + "/" + WBLDet.AirportOfOrigin + WBLDet.PermitToProceedDestAirport + "/" + WBLDet.ShipmentDescriptionCode + WBLDet.NumberOfPieces + "/" + WBLDet.WeightCode + WBLDet.Weight + "/" + WBLDet.CargoDescription + (WBLDet.ArrivalDatePermitToProceed != string.Empty ? "/" + WBLDet.ArrivalDatePermitToProceed : string.Empty));

                    }
                    //Arrival Details
                    foreach (ARR1 ArrivalDet in ArrivalDetails)
                    {
                        sbFXI.AppendLine(ArrivalDet.ComponentIdentifier + "/" + ArrivalDet.ImportingCarrier + ArrivalDet.FlightNumber + "/" + ArrivalDet.ScheduledArrivalDate +
                           ((ArrivalDet.PartArrivalReference != string.Empty) ? "-" + ArrivalDet.PartArrivalReference : string.Empty) + ((ArrivalDet.BoardedQuantityIdentifier != string.Empty && ArrivalDet.BoardedPieceCount > 0) ? "/" + ArrivalDet.BoardedQuantityIdentifier + ArrivalDet.BoardedPieceCount.ToString() : string.Empty) +
                           ((ArrivalDet.WeightCode != string.Empty && ArrivalDet.Weight > 0) ? "/" + ArrivalDet.WeightCode + ArrivalDet.Weight.ToString() : string.Empty));
                    }
                    //Agent Details
                    if (AgentDetails.AirAMSParticipantCode != string.Empty)
                    {
                        sbFXI.AppendLine(AgentDetails.ComponentIdentifier + "/" + AgentDetails.AirAMSParticipantCode);
                    }

                    //Shipper Details
                    if (ShipperDetails.Name != string.Empty && ShipperDetails.City != string.Empty && ShipperDetails.CountryCode != string.Empty)
                    {
                        sbFXI.AppendLine(ShipperDetails.ComponentIdentifier + "/" + ShipperDetails.Name);
                        sbFXI.AppendLine("/" + ShipperDetails.StreetAddress);
                        sbFXI.AppendLine("/" + ShipperDetails.City + (ShipperDetails.State != string.Empty ? "/" + ShipperDetails.State : string.Empty));
                        sbFXI.AppendLine("/" + ShipperDetails.CountryCode + (ShipperDetails.PostalCode != string.Empty ? "/" + ShipperDetails.PostalCode : string.Empty));

                    }
                    //Consignee Details
                    if (ConsigneeDetails.Name != string.Empty && ConsigneeDetails.City != string.Empty && ConsigneeDetails.CountryCode != string.Empty)
                    {
                        sbFXI.AppendLine(ConsigneeDetails.ComponentIdentifier + "/" + ConsigneeDetails.Name);
                        sbFXI.AppendLine("/" + ConsigneeDetails.StreetAddress);
                        sbFXI.AppendLine("/" + ConsigneeDetails.City + (ConsigneeDetails.State != string.Empty ? "/" + ConsigneeDetails.State : string.Empty));
                        sbFXI.AppendLine("/" + ConsigneeDetails.CountryCode + (ConsigneeDetails.PostalCode != string.Empty ? "/" + ConsigneeDetails.PostalCode : string.Empty) + (ConsigneeDetails.TelephoneNumber != string.Empty ? "/" + ConsigneeDetails.TelephoneNumber : string.Empty));

                    }
                    //Transfer Details
                    foreach (TRN TransDetails in TransferDetails)
                    {
                        if (TransDetails.DestinationAirport != string.Empty)
                        {
                            sbFXI.AppendLine(TransDetails.ComponentIdentifier + "/" + TransDetails.DestinationAirport + (TransDetails.DestinationAirport != "000" || TransDetails.DomesticInternationIdentifier != string.Empty ? "-" + TransDetails.DomesticInternationIdentifier : string.Empty) +
                                (TransDetails.BondedCarrierID != string.Empty || TransDetails.OnwardCarrier != string.Empty ? "/" + TransDetails.BondedCarrierID + TransDetails.OnwardCarrier : string.Empty) +
                                (TransDetails.BondedPremisesIdentifier != string.Empty || TransDetails.InBondControlNumber != string.Empty ? "/" + TransDetails.BondedPremisesIdentifier + TransDetails.InBondControlNumber : string.Empty));
                        }
                    }
                    //CBP Shipment Description CSD
                    if (CSDDetails.DeclaredValue != 0 && CSDDetails.ISOCurrencyCode != string.Empty)
                    {
                        sbFXI.AppendLine(CSDDetails.ComponentIdentifier + "/" + CSDDetails.OriginOfGoods + "/" + CSDDetails.DeclaredValue.ToString() +
                            "-" + CSDDetails.ISOCurrencyCode + (CSDDetails.HarmonizedCommodityCode != string.Empty ? "/" + CSDDetails.HarmonizedCommodityCode : string.Empty));
                    }

                    //FDA Freight Indicator
                    if (FDA.FDADetails != string.Empty)
                    {
                        sbFXI.AppendLine(FDA.FDADetails);
                    }

                    return sbFXI.ToString();
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
                    return string.Empty;
                }
            }
            #endregion




        }
        #endregion


        #region FRC Class
        [Serializable]
        public class FRC
        {
            //Message Type (Note : Three Characters). 
            public SMI StandardMessageIdentifier = new SMI();
            //Cargo Control Location details CCL
            public CCL CCLDetails = new CCL();
            //AWB Details
            public AWB1[] AWBDetails = new AWB1[0];
            //WBL Details
            public WBL[] WBLDetails = new WBL[0];
            //Arrival Details
            public ARR1[] ArrivalDetails = new ARR1[0];
            //Agent Details
            public AGT AgentDetails = new AGT();
            //Shipper Details
            public SHP ShipperDetails = new SHP();
            //Consignee Details
            public CNE ConsigneeDetails = new CNE();
            //Transfer Details
            public TRN[] TransferDetails = new TRN[0];
            //CBP Shipment Description Details
            public CSD CSDDetails = new CSD();
            //FDA Freight Indicatior
            public FDA FDA = new FDA();

            //RFA	Reason for Amendment
            public RFA RFA = new RFA();

            #region Overriding ToString Method for FRC
            public override string ToString()
            {

                try
                {
                    StringBuilder sbFRC = new StringBuilder();
                    //Message Identifier
                    sbFRC.AppendLine(StandardMessageIdentifier.StandardMessageIdentifier);
                    //Cargo Control Location Details
                    if (CCLDetails.AirportOfArrival != string.Empty && CCLDetails.CargoTerminalOperator != string.Empty)
                    {
                        sbFRC.AppendLine(CCLDetails.AirportOfArrival.ToString() + CCLDetails.CargoTerminalOperator);
                    }
                    //AWB Details
                    foreach (AWB1 AWBs in AWBDetails)
                    {
                        sbFRC.AppendLine(AWBs.AWBPrefix + "-" + AWBs.AWBNumber + ((AWBs.ConsolidationIdentifier != string.Empty || AWBs.HAWBNumber != string.Empty) ? "-" + AWBs.ConsolidationIdentifier + AWBs.HAWBNumber : string.Empty) + (AWBs.PackageTrackingIdentifier != string.Empty ? "/" + AWBs.PackageTrackingIdentifier : string.Empty));
                    }
                    //Waybill Details
                    foreach (WBL WBLDet in WBLDetails)
                    {
                        sbFRC.AppendLine(WBLDet.ComponentIdentifier + "/"
                                            + WBLDet.AirportOfOrigin + WBLDet.PermitToProceedDestAirport + "/"
                                            + WBLDet.ShipmentDescriptionCode + WBLDet.NumberOfPieces + "/"
                                            + WBLDet.WeightCode + WBLDet.Weight + "/"
                                            + WBLDet.CargoDescription + (WBLDet.ArrivalDatePermitToProceed != string.Empty ? "/" + WBLDet.ArrivalDatePermitToProceed : string.Empty));

                    }
                    //Arrival Details
                    foreach (ARR1 ArrivalDet in ArrivalDetails)
                    {
                        sbFRC.AppendLine(ArrivalDet.ComponentIdentifier + "/" + ArrivalDet.ImportingCarrier + ArrivalDet.FlightNumber + "/" + ArrivalDet.ScheduledArrivalDate +
                               ((ArrivalDet.PartArrivalReference != string.Empty) ? "-" + ArrivalDet.PartArrivalReference : string.Empty) + ((ArrivalDet.BoardedQuantityIdentifier != string.Empty && ArrivalDet.BoardedPieceCount > 0) ? "/" + ArrivalDet.BoardedQuantityIdentifier + ArrivalDet.BoardedPieceCount.ToString() : string.Empty) +
                               ((ArrivalDet.WeightCode != string.Empty && ArrivalDet.Weight > 0) ? "/" + ArrivalDet.WeightCode + ArrivalDet.Weight.ToString() : string.Empty));
                    }
                    //Agent Details
                    if (AgentDetails.AirAMSParticipantCode != string.Empty)
                    {
                        sbFRC.AppendLine(AgentDetails.ComponentIdentifier + "/" + AgentDetails.AirAMSParticipantCode);
                    }

                    //Shipper Details
                    if (ShipperDetails.Name != string.Empty && ShipperDetails.City != string.Empty && ShipperDetails.CountryCode != string.Empty)
                    {
                        sbFRC.AppendLine(ShipperDetails.ComponentIdentifier + "/" + (ShipperDetails.Name.ToString().Length > 35 ? ShipperDetails.Name.ToString().Substring(0, 35) : ShipperDetails.Name));
                        sbFRC.AppendLine("/" + (ShipperDetails.StreetAddress.Length > 35 ? ShipperDetails.StreetAddress.ToString().Substring(0, 35) : ShipperDetails.StreetAddress));
                        sbFRC.AppendLine("/" + ShipperDetails.City + (ShipperDetails.State != string.Empty ? "/" + ShipperDetails.State : string.Empty));
                        sbFRC.AppendLine("/" + ShipperDetails.CountryCode + (ShipperDetails.PostalCode != string.Empty ? "/" + ShipperDetails.PostalCode : string.Empty));

                    }
                    //Consignee Details
                    if (ConsigneeDetails.Name != string.Empty && ConsigneeDetails.City != string.Empty && ConsigneeDetails.CountryCode != string.Empty)
                    {
                        sbFRC.AppendLine(ConsigneeDetails.ComponentIdentifier + "/" + (ConsigneeDetails.Name.Length > 35 ? ConsigneeDetails.Name.ToString().Substring(0, 35) : ConsigneeDetails.Name));
                        sbFRC.AppendLine("/" + (ConsigneeDetails.StreetAddress.Length > 35 ? ConsigneeDetails.StreetAddress.ToString().Substring(0, 35) : ConsigneeDetails.StreetAddress));
                        sbFRC.AppendLine("/" + ConsigneeDetails.City + (ConsigneeDetails.State != string.Empty ? "/" + ConsigneeDetails.State : string.Empty));
                        sbFRC.AppendLine("/" + ConsigneeDetails.CountryCode + (ConsigneeDetails.PostalCode != string.Empty ? "/" + ConsigneeDetails.PostalCode : string.Empty) + (ConsigneeDetails.TelephoneNumber != string.Empty ? "/" + ConsigneeDetails.TelephoneNumber : string.Empty));

                    }
                    //Transfer Details
                    foreach (TRN TransDetails in TransferDetails)
                    {
                        if (TransDetails.DestinationAirport != string.Empty)
                        {
                            sbFRC.AppendLine(TransDetails.ComponentIdentifier + "/" + TransDetails.DestinationAirport + (TransDetails.DestinationAirport != "000" ? "-" + TransDetails.DomesticInternationIdentifier : string.Empty) +
                                (TransDetails.DestinationAirport != "000" ? "/" + TransDetails.BondedCarrierID + TransDetails.OnwardCarrier : string.Empty) +
                                (TransDetails.DestinationAirport != "000" ? "/" + TransDetails.BondedPremisesIdentifier + TransDetails.InBondControlNumber : string.Empty));
                        }
                    }
                    //CBP Shipment Description CSD
                    if (CSDDetails.DeclaredValue != 0 && CSDDetails.ISOCurrencyCode != string.Empty)
                    {
                        sbFRC.AppendLine(CSDDetails.ComponentIdentifier + "/" + CSDDetails.OriginOfGoods + "/" + CSDDetails.DeclaredValue.ToString() +
                            "-" + CSDDetails.ISOCurrencyCode + (CSDDetails.HarmonizedCommodityCode != string.Empty ? "/" + CSDDetails.HarmonizedCommodityCode : string.Empty));
                    }

                    //FDA Freight Indicator
                    if (FDA.FDADetails != string.Empty)
                    {
                        sbFRC.AppendLine(FDA.FDADetails);
                    }

                    if (RFA != null)
                    {
                        try
                        {
                            String strTemp = "";
                            if (!String.IsNullOrEmpty(RFA.ComponentIdentifier))
                            {
                                strTemp = RFA.ComponentIdentifier;
                                if (!String.IsNullOrEmpty(RFA.AmendmentCode))
                                {
                                    strTemp += "/" + RFA.AmendmentCode;
                                }
                                if (!String.IsNullOrEmpty(RFA.AmendmentExplanation))
                                {
                                    strTemp += "/" + RFA.AmendmentExplanation;
                                }
                            }
                            if (!String.IsNullOrEmpty(strTemp))
                            {
                                sbFRC.AppendLine(strTemp);
                            }
                        }
                        catch (Exception ex)
                        {
                            clsLog.WriteLogAzure(ex.Message);
                        }
                    }

                    return sbFRC.ToString();
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
                    return string.Empty;
                }
            }
            #endregion

            public FRC Encode(DataSet ds)
            {
                try
                {
                    FRC FRC = new FRC();
                    if (ds != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                foreach (DataRow row in ds.Tables[0].Rows)
                                {

                                    FRC.StandardMessageIdentifier.StandardMessageIdentifier = row["StandardMessageIdentifier"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[1].Rows)
                                {

                                    FRC.CCLDetails.AirportOfArrival = row["AirportOfArrival"].ToString();
                                    FRC.CCLDetails.CargoTerminalOperator = row["CargoTerminalOperator"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[2].Rows)
                                {

                                    Array.Resize(ref FRC.AWBDetails, FRC.AWBDetails.Length + 1);
                                    FRC.AWBDetails[FRC.AWBDetails.Length - 1] = new AWB1();
                                    FRC.AWBDetails[FRC.AWBDetails.Length - 1].AWBPrefix = row["AWBPrefix"].ToString();
                                    FRC.AWBDetails[FRC.AWBDetails.Length - 1].AWBNumber = row["AWBNumber"].ToString();
                                    FRC.AWBDetails[FRC.AWBDetails.Length - 1].ConsolidationIdentifier = row["ConsolidationIdentifier"].ToString();
                                    FRC.AWBDetails[FRC.AWBDetails.Length - 1].HAWBNumber = row["HAWBNumber"].ToString();
                                    FRC.AWBDetails[FRC.AWBDetails.Length - 1].PackageTrackingIdentifier = row["PackageTrackingIdentifier"].ToString();

                                }
                                foreach (DataRow row in ds.Tables[3].Rows)
                                {
                                    Array.Resize(ref FRC.WBLDetails, FRC.WBLDetails.Length + 1);
                                    FRC.WBLDetails[FRC.WBLDetails.Length - 1] = new WBL();
                                    FRC.WBLDetails[FRC.WBLDetails.Length - 1].ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FRC.WBLDetails[FRC.WBLDetails.Length - 1].AirportOfOrigin = row["AirportOfOrigin"].ToString();
                                    FRC.WBLDetails[FRC.WBLDetails.Length - 1].ArrivalDatePermitToProceed = row["ArrivalDatePermitToProceed"].ToString();
                                    FRC.WBLDetails[FRC.WBLDetails.Length - 1].CargoDescription = row["CargoDescription"].ToString();
                                    FRC.WBLDetails[FRC.WBLDetails.Length - 1].NumberOfPieces = row["NumberOfPieces"].ToString() != string.Empty ? Convert.ToInt32(row["NumberOfPieces"]) : 0;
                                    FRC.WBLDetails[FRC.WBLDetails.Length - 1].PermitToProceedDestAirport = row["PermitToProceedDestAirport"].ToString();
                                    FRC.WBLDetails[FRC.WBLDetails.Length - 1].ShipmentDescriptionCode = row["ShipmentDescriptionCode"].ToString();
                                    FRC.WBLDetails[FRC.WBLDetails.Length - 1].Weight = row["Weight"].ToString() != string.Empty ? Double.Parse(row["Weight"].ToString()) : 0;
                                    FRC.WBLDetails[FRC.WBLDetails.Length - 1].WeightCode = row["WeightCode"].ToString();

                                }
                                foreach (DataRow row in ds.Tables[4].Rows)
                                {
                                    Array.Resize(ref FRC.ArrivalDetails, FRC.ArrivalDetails.Length + 1);
                                    FRC.ArrivalDetails[FRC.ArrivalDetails.Length - 1] = new ARR1();
                                    FRC.ArrivalDetails[FRC.ArrivalDetails.Length - 1].BoardedPieceCount = row["BoardedPieceCount"].ToString() != string.Empty ? Convert.ToInt32(row["BoardedPieceCount"]) : 0;
                                    FRC.ArrivalDetails[FRC.ArrivalDetails.Length - 1].BoardedQuantityIdentifier = row["BoardedQuantityIdentifier"].ToString();
                                    FRC.ArrivalDetails[FRC.ArrivalDetails.Length - 1].ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FRC.ArrivalDetails[FRC.ArrivalDetails.Length - 1].FlightNumber = row["FlightNumber"].ToString();
                                    FRC.ArrivalDetails[FRC.ArrivalDetails.Length - 1].ImportingCarrier = row["ImportingCarrier"].ToString();
                                    FRC.ArrivalDetails[FRC.ArrivalDetails.Length - 1].PartArrivalReference = row["PartArrivalReference"].ToString();
                                    FRC.ArrivalDetails[FRC.ArrivalDetails.Length - 1].ScheduledArrivalDate = row["ScheduledArrivalDate"].ToString();
                                    FRC.ArrivalDetails[FRC.ArrivalDetails.Length - 1].Weight = row["Weight"].ToString() != string.Empty ? Double.Parse(row["Weight"].ToString()) : 0;
                                    FRC.ArrivalDetails[FRC.ArrivalDetails.Length - 1].WeightCode = row["WeightCode"].ToString();

                                }

                                foreach (DataRow row in ds.Tables[5].Rows)
                                {
                                    FRC.AgentDetails.AirAMSParticipantCode = row["AirAMSParticipantCode"].ToString();
                                    FRC.AgentDetails.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                }

                                foreach (DataRow row in ds.Tables[6].Rows)
                                {
                                    FRC.ShipperDetails.City = row["City"].ToString();
                                    FRC.ShipperDetails.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FRC.ShipperDetails.CountryCode = row["CountryCode"].ToString();
                                    FRC.ShipperDetails.Name = row["Name"].ToString();
                                    FRC.ShipperDetails.PostalCode = row["PostalCode"].ToString();
                                    FRC.ShipperDetails.State = row["State"].ToString();
                                    FRC.ShipperDetails.StreetAddress = row["StreetAddress"].ToString();
                                }

                                foreach (DataRow row in ds.Tables[7].Rows)
                                {
                                    FRC.ConsigneeDetails.City = row["City"].ToString();
                                    FRC.ConsigneeDetails.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FRC.ConsigneeDetails.CountryCode = row["CountryCode"].ToString();
                                    FRC.ConsigneeDetails.Name = row["Name"].ToString();
                                    FRC.ConsigneeDetails.PostalCode = row["PostalCode"].ToString();
                                    FRC.ConsigneeDetails.State = row["State"].ToString();
                                    FRC.ConsigneeDetails.StreetAddress = row["StreetAddress"].ToString();
                                    FRC.ConsigneeDetails.TelephoneNumber = row["TelephoneNumber"].ToString();
                                }

                                foreach (DataRow row in ds.Tables[8].Rows)
                                {
                                    Array.Resize(ref FRC.TransferDetails, FRC.TransferDetails.Length + 1);
                                    FRC.TransferDetails[FRC.TransferDetails.Length - 1] = new TRN();
                                    FRC.TransferDetails[FRC.TransferDetails.Length - 1].BondedCarrierID = row["BondedCarrierID"].ToString();
                                    FRC.TransferDetails[FRC.TransferDetails.Length - 1].BondedPremisesIdentifier = row["BondedPremisesIdentifier"].ToString();
                                    FRC.TransferDetails[FRC.TransferDetails.Length - 1].ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FRC.TransferDetails[FRC.TransferDetails.Length - 1].DestinationAirport = row["DestinationAirport"].ToString();
                                    FRC.TransferDetails[FRC.TransferDetails.Length - 1].DomesticInternationIdentifier = row["DomesticInternationIdentifier"].ToString();
                                    FRC.TransferDetails[FRC.TransferDetails.Length - 1].InBondControlNumber = row["InBondControlNumber"].ToString();
                                    FRC.TransferDetails[FRC.TransferDetails.Length - 1].OnwardCarrier = row["OnwardCarrier"].ToString();

                                }

                                foreach (DataRow row in ds.Tables[9].Rows)
                                {
                                    FRC.CSDDetails.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FRC.CSDDetails.DeclaredValue = row["DeclaredValue"].ToString() != string.Empty ? Double.Parse(row["DeclaredValue"].ToString()) : 0;
                                    FRC.CSDDetails.HarmonizedCommodityCode = row["HarmonizedCommodityCode"].ToString();
                                    FRC.CSDDetails.ISOCurrencyCode = row["ISOCurrencyCode"].ToString();
                                    FRC.CSDDetails.OriginOfGoods = row["OriginOfGoods"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[10].Rows)
                                {
                                    FRC.FDA.FDADetails = row["FDADetails"].ToString();
                                }

                                foreach (DataRow row in ds.Tables[11].Rows)
                                {
                                    try
                                    {
                                        FRC.RFA.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                        FRC.RFA.AmendmentCode = row["AmendmentCode"].ToString();
                                        FRC.RFA.AmendmentExplanation = row["AmendmentExplanation"].ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        clsLog.WriteLogAzure(ex.Message); ;
                                    }
                                }

                                return FRC;

                            }

                        }

                    }
                    return null;
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
                    return null;
                }
            }


        }
        #endregion


        #region FSN Class
        [Serializable]
        public class FSN
        {
            //Message Type (Note : Three Characters). 
            public SMI StandardMessageIdentifier = new SMI();
            //Cargo Control Location details CCL
            public CCL CCLDetails = new CCL();
            //AWB Details
            public AWB1[] AWBDetails = new AWB1[0];

            public ARR2[] ArrivalDetails = new ARR2[0];

            public ASN ASN = new ASN();

            #region Overriding ToString Method for FSN
            public override string ToString()
            {

                try
                {
                    StringBuilder sbFRI = new StringBuilder();
                    //Message Identifier
                    sbFRI.AppendLine(StandardMessageIdentifier.StandardMessageIdentifier);
                    //Cargo Control Location Details
                    if (CCLDetails.AirportOfArrival != string.Empty && CCLDetails.CargoTerminalOperator != string.Empty)
                    {
                        sbFRI.AppendLine(CCLDetails.AirportOfArrival.ToString() + CCLDetails.CargoTerminalOperator);
                    }
                    //AWB Details
                    foreach (AWB1 AWBs in AWBDetails)
                    {
                        sbFRI.AppendLine(AWBs.AWBPrefix + "-" + AWBs.AWBNumber + ((AWBs.ConsolidationIdentifier != string.Empty || AWBs.HAWBNumber != string.Empty) ? "-" + AWBs.ConsolidationIdentifier + AWBs.HAWBNumber : string.Empty) + (AWBs.PackageTrackingIdentifier != string.Empty ? "/" + AWBs.PackageTrackingIdentifier : string.Empty));
                    }

                    //Arrival Details
                    foreach (ARR2 ArrivalDet in ArrivalDetails)
                    {
                        sbFRI.AppendLine(ArrivalDet.ComponentIdentifier + "/" + ArrivalDet.ImportingCarrier + ArrivalDet.FlightNumber + "/" + ArrivalDet.ScheduledArrivalDate + (ArrivalDet.PartArrivalReference != string.Empty ? "-" + ArrivalDet.PartArrivalReference : String.Empty));
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
                        sbFRI.AppendLine(strTemp);
                    }

                    return sbFRI.ToString();
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
                    return string.Empty;
                }
            }
            #endregion

            public FSN Encode(DataSet ds)
            {
                try
                {
                    FSN FSN = new FSN();
                    if (ds != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                foreach (DataRow row in ds.Tables[0].Rows)
                                {

                                    FSN.StandardMessageIdentifier.StandardMessageIdentifier = row["StandardMessageIdentifier"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[1].Rows)
                                {

                                    FSN.CCLDetails.AirportOfArrival = row["AirportOfArrival"].ToString();
                                    FSN.CCLDetails.CargoTerminalOperator = row["CargoTerminalOperator"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[2].Rows)
                                {

                                    Array.Resize(ref FSN.AWBDetails, FSN.AWBDetails.Length + 1);
                                    FSN.AWBDetails[FSN.AWBDetails.Length - 1] = new AWB1();
                                    FSN.AWBDetails[FSN.AWBDetails.Length - 1].AWBPrefix = row["AWBPrefix"].ToString();
                                    FSN.AWBDetails[FSN.AWBDetails.Length - 1].AWBNumber = row["AWBNumber"].ToString();
                                    FSN.AWBDetails[FSN.AWBDetails.Length - 1].ConsolidationIdentifier = row["ConsolidationIdentifier"].ToString();
                                    FSN.AWBDetails[FSN.AWBDetails.Length - 1].HAWBNumber = row["HAWBNumber"].ToString();
                                    FSN.AWBDetails[FSN.AWBDetails.Length - 1].PackageTrackingIdentifier = row["PackageTrackingIdentifier"].ToString();

                                }

                                foreach (DataRow row in ds.Tables[3].Rows)
                                {
                                    Array.Resize(ref FSN.ArrivalDetails, FSN.ArrivalDetails.Length + 1);
                                    FSN.ArrivalDetails[FSN.ArrivalDetails.Length - 1] = new ARR2();
                                    FSN.ArrivalDetails[FSN.ArrivalDetails.Length - 1].ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FSN.ArrivalDetails[FSN.ArrivalDetails.Length - 1].FlightNumber = row["FlightNumber"].ToString();
                                    FSN.ArrivalDetails[FSN.ArrivalDetails.Length - 1].ImportingCarrier = row["ImportingCarrier"].ToString();
                                    FSN.ArrivalDetails[FSN.ArrivalDetails.Length - 1].PartArrivalReference = row["PartArrivalReference"].ToString();
                                    FSN.ArrivalDetails[FSN.ArrivalDetails.Length - 1].ScheduledArrivalDate = row["ScheduledArrivalDate"].ToString();
                                }

                                foreach (DataRow row in ds.Tables[4].Rows)
                                {
                                    FSN.ASN.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FSN.ASN.StatusCode = row["StatusCode"].ToString();
                                    FSN.ASN.ActionExplanation = row["ActionExplanation"].ToString();
                                }

                                return FSN;

                            }

                        }

                    }
                    return null;
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
                    return null;
                }
            }


        }
        #endregion

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
                    clsLog.WriteLogAzure(ex.Message);
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
                    clsLog.WriteLogAzure(ex.Message);
                    return null;
                }
            }


        }
        #endregion

        #region FRX Class
        [Serializable]
        public class FRX
        {
            //Message Type (Note : Three Characters). 
            public SMI StandardMessageIdentifier = new SMI();
            //Cargo Control Location details CCL
            public CCL CCLDetails = new CCL();
            //AWB Details
            public AWB1[] AWBDetails = new AWB1[0];

            public ARR2[] ArrivalDetails = new ARR2[0];

            public RFA RFA = new RFA();

            #region Overriding ToString Method for FRX
            public override string ToString()
            {

                try
                {
                    StringBuilder sbFRX = new StringBuilder();
                    //Message Identifier
                    sbFRX.AppendLine(StandardMessageIdentifier.StandardMessageIdentifier);
                    //Cargo Control Location Details
                    if (CCLDetails.AirportOfArrival != string.Empty && CCLDetails.CargoTerminalOperator != string.Empty)
                    {
                        sbFRX.AppendLine(CCLDetails.AirportOfArrival.ToString() + CCLDetails.CargoTerminalOperator);
                    }
                    //AWB Details
                    foreach (AWB1 AWBs in AWBDetails)
                    {
                        sbFRX.AppendLine(AWBs.AWBPrefix + "-" + AWBs.AWBNumber + ((AWBs.ConsolidationIdentifier != string.Empty || AWBs.HAWBNumber != string.Empty) ? "-" + AWBs.ConsolidationIdentifier + AWBs.HAWBNumber : string.Empty) + (AWBs.PackageTrackingIdentifier != string.Empty ? "/" + AWBs.PackageTrackingIdentifier : string.Empty));
                    }

                    //Arrival Details
                    foreach (ARR2 ArrivalDet in ArrivalDetails)
                    {
                        sbFRX.AppendLine(ArrivalDet.ComponentIdentifier + "/" + ArrivalDet.ImportingCarrier + ArrivalDet.FlightNumber + "/" + ArrivalDet.ScheduledArrivalDate + (ArrivalDet.PartArrivalReference != string.Empty ? "-" + ArrivalDet.PartArrivalReference : String.Empty));
                        //sbFRX.AppendLine(ArrivalDet.ComponentIdentifier + "/" + ArrivalDet.ImportingCarrier + ArrivalDet.FlightNumber + "/" + ArrivalDet.ScheduledArrivalDate + (ArrivalDet.PartArrivalReference != string.Empty ? "-" + ArrivalDet.PartArrivalReference + "/" + "B" + ArrivalDet.BoardedPieceCount.ToString() + "/" + ArrivalDet.WeightCode + ArrivalDet.Weight.ToString() : string.Empty));
                    }

                    if (RFA != null)
                    {
                        try
                        {
                            String strTemp = "";
                            if (!String.IsNullOrEmpty(RFA.ComponentIdentifier))
                            {
                                strTemp = RFA.ComponentIdentifier;
                                if (!String.IsNullOrEmpty(RFA.AmendmentCode))
                                {
                                    strTemp += "/" + RFA.AmendmentCode;
                                }
                                if (!String.IsNullOrEmpty(RFA.AmendmentExplanation))
                                {
                                    strTemp += "/" + RFA.AmendmentExplanation;
                                }
                            }
                            if (!String.IsNullOrEmpty(strTemp))
                            {
                                sbFRX.AppendLine(strTemp);
                            }
                        }
                        catch (Exception ex)
                        {
                            clsLog.WriteLogAzure(ex.Message);
                        }
                    }
                    return sbFRX.ToString();
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
                    return string.Empty;
                }
            }
            #endregion

            public FRX Encode(DataSet ds)
            {
                try
                {
                    FRX FRX = new FRX();
                    if (ds != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                foreach (DataRow row in ds.Tables[0].Rows)
                                {

                                    FRX.StandardMessageIdentifier.StandardMessageIdentifier = row["StandardMessageIdentifier"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[1].Rows)
                                {

                                    FRX.CCLDetails.AirportOfArrival = row["AirportOfArrival"].ToString();
                                    FRX.CCLDetails.CargoTerminalOperator = row["CargoTerminalOperator"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[2].Rows)
                                {

                                    Array.Resize(ref FRX.AWBDetails, FRX.AWBDetails.Length + 1);
                                    FRX.AWBDetails[FRX.AWBDetails.Length - 1] = new AWB1();
                                    FRX.AWBDetails[FRX.AWBDetails.Length - 1].AWBPrefix = row["AWBPrefix"].ToString();
                                    FRX.AWBDetails[FRX.AWBDetails.Length - 1].AWBNumber = row["AWBNumber"].ToString();
                                    FRX.AWBDetails[FRX.AWBDetails.Length - 1].ConsolidationIdentifier = row["ConsolidationIdentifier"].ToString();
                                    FRX.AWBDetails[FRX.AWBDetails.Length - 1].HAWBNumber = row["HAWBNumber"].ToString();
                                    FRX.AWBDetails[FRX.AWBDetails.Length - 1].PackageTrackingIdentifier = row["PackageTrackingIdentifier"].ToString();

                                }

                                foreach (DataRow row in ds.Tables[3].Rows)
                                {
                                    Array.Resize(ref FRX.ArrivalDetails, FRX.ArrivalDetails.Length + 1);
                                    FRX.ArrivalDetails[FRX.ArrivalDetails.Length - 1] = new ARR2();
                                    FRX.ArrivalDetails[FRX.ArrivalDetails.Length - 1].ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FRX.ArrivalDetails[FRX.ArrivalDetails.Length - 1].FlightNumber = row["FlightNumber"].ToString();
                                    FRX.ArrivalDetails[FRX.ArrivalDetails.Length - 1].ImportingCarrier = row["ImportingCarrier"].ToString();
                                    FRX.ArrivalDetails[FRX.ArrivalDetails.Length - 1].PartArrivalReference = row["PartArrivalReference"].ToString();
                                    FRX.ArrivalDetails[FRX.ArrivalDetails.Length - 1].ScheduledArrivalDate = row["ScheduledArrivalDate"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[4].Rows)
                                {
                                    try
                                    {
                                        FRX.RFA.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                        FRX.RFA.AmendmentCode = row["AmendmentCode"].ToString();
                                        FRX.RFA.AmendmentExplanation = row["AmendmentExplanation"].ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        clsLog.WriteLogAzure(ex.Message);
                                    }
                                }

                                return FRX;

                            }

                        }

                    }
                    return null;
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
                    return null;
                }
            }


        }
        #endregion

        #region FER

        [Serializable]
        public class FER
        {
            //Message Type (Note : Three Characters). 
            public SMI StandardMessageIdentifier = new SMI();
            //AWB Details
            public AWB1[] AWBDetails = new AWB1[0];
            public ERF ERF = new ERF();
            public ERR[] ERR = new ERR[0];

            #region Overriding ToString Method for FER
            public override string ToString()
            {

                try
                {
                    StringBuilder sbFER = new StringBuilder();
                    //Message Identifier
                    sbFER.AppendLine(StandardMessageIdentifier.StandardMessageIdentifier);
                    sbFER.AppendLine(ERF.ImportingCarrier + ERF.FlightNumber + "/" + ERF.Date);  //  before date '/' is compulsory.
                    //AWB Details
                    foreach (AWB1 AWBs in AWBDetails)
                    {
                        try
                        {
                            if (ERR != null && ERR.Length > 0 && ERR[0].ErrorCode == "007")
                            {
                                sbFER.AppendLine("000" + "-" + "00000000"); //+ ((AWBs.ConsolidationIdentifier != string.Empty || AWBs.HAWBNumber != string.Empty) ? "-" + AWBs.ConsolidationIdentifier + AWBs.HAWBNumber : string.Empty) + (AWBs.PackageTrackingIdentifier != string.Empty ? "/" + AWBs.PackageTrackingIdentifier : string.Empty));
                            }
                            else
                            {
                                sbFER.AppendLine(AWBs.AWBPrefix + "-" + AWBs.AWBNumber + ((AWBs.ConsolidationIdentifier != string.Empty || AWBs.HAWBNumber != string.Empty) ? "-" + AWBs.ConsolidationIdentifier + AWBs.HAWBNumber : string.Empty) + (AWBs.PackageTrackingIdentifier != string.Empty ? "/" + AWBs.PackageTrackingIdentifier : string.Empty));
                            }
                        }
                        catch (Exception ex)
                        {
                            clsLog.WriteLogAzure(ex.Message);
                        }
                    }
                    foreach (ERR ERRLoop in ERR)
                    {
                        if (!String.IsNullOrEmpty(ERRLoop.ComponentIdentifier))
                        {
                            sbFER.AppendLine(ERRLoop.ComponentIdentifier + "/" + ERRLoop.ErrorCode + ERRLoop.ErrorMessageText);
                        }
                    }



                    return sbFER.ToString();
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
                    return string.Empty;
                }
            }
            #endregion


        }
        #endregion

        #region FSQ
        [Serializable]
        public class FSQ
        {
            //Message Type (Note : Three Characters). 
            public SMI StandardMessageIdentifier = new SMI();
            //Cargo Control Location details CCL
            public CCL CCLDetails = new CCL();
            //AWB Details
            public AWB2[] AWBDetails = new AWB2[0];
            //FSQ Details
            public FSQSub FSQSub = new FSQSub();

            #region Overriding ToString Method for FSQ
            public override string ToString()
            {

                try
                {
                    StringBuilder sbFSQ = new StringBuilder();
                    //Message Identifier
                    sbFSQ.AppendLine(StandardMessageIdentifier.StandardMessageIdentifier);
                    //Cargo Control Location Details
                    if (CCLDetails.AirportOfArrival != string.Empty && CCLDetails.CargoTerminalOperator != string.Empty)
                    {
                        sbFSQ.AppendLine(CCLDetails.AirportOfArrival.ToString() + CCLDetails.CargoTerminalOperator);
                    }
                    //AWB Details
                    foreach (AWB2 AWBs in AWBDetails)
                    {
                        sbFSQ.AppendLine(AWBs.AWBPrefix + "-" + AWBs.AWBNumber + ((AWBs.HAWBNumber != string.Empty) ? "-" + AWBs.HAWBNumber : string.Empty) + (AWBs.ArrivalReference != string.Empty ? "-" + AWBs.ArrivalReference : string.Empty));
                    }

                    //FSQ Details
                    if (FSQSub != null)
                    {
                        sbFSQ.AppendLine(FSQSub.ComponentIdentifier + "/" + FSQSub.StatusRequestCode);
                    }

                    return sbFSQ.ToString();
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
                    return string.Empty;
                }
            }
            #endregion

            public FSQ Encode(DataSet ds)
            {

                try
                {
                    FSQ FSQ = new FSQ();
                    if (ds != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                foreach (DataRow row in ds.Tables[0].Rows)
                                {

                                    FSQ.StandardMessageIdentifier.StandardMessageIdentifier = row["StandardMessageIdentifier"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[1].Rows)
                                {

                                    FSQ.CCLDetails.AirportOfArrival = row["AirportOfArrival"].ToString();
                                    FSQ.CCLDetails.CargoTerminalOperator = row["CargoTerminalOperator"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[2].Rows)
                                {

                                    Array.Resize(ref FSQ.AWBDetails, FSQ.AWBDetails.Length + 1);
                                    FSQ.AWBDetails[FSQ.AWBDetails.Length - 1] = new AWB2();

                                    FSQ.AWBDetails[FSQ.AWBDetails.Length - 1].AWBPrefix = row["AWBPrefix"].ToString();
                                    FSQ.AWBDetails[FSQ.AWBDetails.Length - 1].AWBNumber = row["AWBNumber"].ToString();

                                    FSQ.AWBDetails[FSQ.AWBDetails.Length - 1].HAWBNumber = row["HAWBNumber"].ToString();
                                    FSQ.AWBDetails[FSQ.AWBDetails.Length - 1].ArrivalReference = row["ArrivalReference"].ToString();

                                }

                                foreach (DataRow row in ds.Tables[3].Rows)
                                {
                                    FSQ.FSQSub.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    FSQ.FSQSub.StatusRequestCode = row["StatusRequestCode"].ToString();
                                }

                                return FSQ;

                            }

                        }

                    }
                    return null;
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
                    return null;
                }

            }


        }
        #endregion

        #region PRI Class
        [Serializable]
        public class PRI
        {
            //Message Type (Note : Three Characters). 
            public SMI StandardMessageIdentifier = new SMI();
            //Cargo Control Location details CCL
            public CCL CCLDetails = new CCL();
            //AWB Details
            public AWB1[] AWBDetails = new AWB1[0];
            //WBL Details
            public WBL[] WBLDetails = new WBL[0];
            //Arrival Details
            public ARR1[] ArrivalDetails = new ARR1[0];
            //Agent Details
            public AGT AgentDetails = new AGT();
            //Shipper Details
            public SHP ShipperDetails = new SHP();
            //Consignee Details
            public CNE ConsigneeDetails = new CNE();
            //Other Party Information Details
            public OPI OPIDetails = new OPI();
            //Transfer Details
            public TRN[] TransferDetails = new TRN[0];
            //CBP Shipment Description Details
            public CSD CSDDetails = new CSD();
            //FDA Freight Indicatior
            public FDA FDA = new FDA();
            //Other Customs Information Details
            public OCI OCIDetails = new OCI();

            #region Overriding ToString Method for PRI
            public override string ToString()
            {

                try
                {
                    StringBuilder sbPRI = new StringBuilder();
                    //Message Identifier
                    sbPRI.AppendLine(StandardMessageIdentifier.StandardMessageIdentifier);
                    //Cargo Control Location Details
                    if (CCLDetails.AirportOfArrival != string.Empty && CCLDetails.CargoTerminalOperator != string.Empty)
                    {
                        sbPRI.AppendLine(CCLDetails.AirportOfArrival.ToString() + CCLDetails.CargoTerminalOperator);
                    }
                    //AWB Details
                    foreach (AWB1 AWBs in AWBDetails)
                    {
                        sbPRI.AppendLine(AWBs.AWBPrefix + "-" + AWBs.AWBNumber + ((AWBs.ConsolidationIdentifier != string.Empty || AWBs.HAWBNumber != string.Empty) ? "-" + AWBs.ConsolidationIdentifier + AWBs.HAWBNumber : string.Empty) + (AWBs.PackageTrackingIdentifier != string.Empty ? "/" + AWBs.PackageTrackingIdentifier : string.Empty));
                    }
                    //Waybill Details
                    foreach (WBL WBLDet in WBLDetails)
                    {
                        sbPRI.AppendLine(WBLDet.ComponentIdentifier + "/" + WBLDet.AirportOfOrigin + WBLDet.PermitToProceedDestAirport + "/" + WBLDet.ShipmentDescriptionCode + WBLDet.NumberOfPieces + "/" + WBLDet.WeightCode + WBLDet.Weight + "/" + WBLDet.CargoDescription + (WBLDet.ArrivalDatePermitToProceed != string.Empty ? "/" + WBLDet.ArrivalDatePermitToProceed : string.Empty));

                    }
                    //Arrival Details
                    foreach (ARR1 ArrivalDet in ArrivalDetails)
                    {
                        sbPRI.AppendLine(ArrivalDet.ComponentIdentifier + "/" + ArrivalDet.ImportingCarrier + ArrivalDet.FlightNumber + "/" + ArrivalDet.ScheduledArrivalDate +
                            ((ArrivalDet.PartArrivalReference != string.Empty) ? "-" + ArrivalDet.PartArrivalReference : string.Empty) + ((ArrivalDet.BoardedQuantityIdentifier != string.Empty && ArrivalDet.BoardedPieceCount > 0) ? "/" + ArrivalDet.BoardedQuantityIdentifier + ArrivalDet.BoardedPieceCount.ToString() : string.Empty) +
                            ((ArrivalDet.WeightCode != string.Empty && ArrivalDet.Weight > 0) ? "/" + ArrivalDet.WeightCode + ArrivalDet.Weight.ToString() : string.Empty));
                    }
                    //Agent Details
                    //if (AgentDetails.AirAMSParticipantCode != string.Empty)
                    //{
                    //    sbPRI.AppendLine(AgentDetails.ComponentIdentifier + "/" + AgentDetails.AirAMSParticipantCode);
                    //}

                    //Shipper Details
                    if (ShipperDetails.Name != string.Empty && ShipperDetails.City != string.Empty && ShipperDetails.CountryCode != string.Empty)
                    {
                        sbPRI.AppendLine(ShipperDetails.ComponentIdentifier + "/" + (ShipperDetails.Name.ToString().Length > 35 ? ShipperDetails.Name.ToString().Substring(0, 35) : ShipperDetails.Name));
                        sbPRI.AppendLine("/" + (ShipperDetails.StreetAddress.Length > 35 ? ShipperDetails.StreetAddress.ToString().Substring(0, 35) : ShipperDetails.StreetAddress));
                        sbPRI.AppendLine("/" + ShipperDetails.City + (ShipperDetails.State != string.Empty ? "/" + ShipperDetails.State : string.Empty));
                        sbPRI.AppendLine("/" + ShipperDetails.CountryCode + (ShipperDetails.PostalCode != string.Empty ? "/" + ShipperDetails.PostalCode : string.Empty));

                    }
                    //Consignee Details
                    if (ConsigneeDetails.Name != string.Empty && ConsigneeDetails.City != string.Empty && ConsigneeDetails.CountryCode != string.Empty)
                    {
                        sbPRI.AppendLine(ConsigneeDetails.ComponentIdentifier + "/" + (ConsigneeDetails.Name.Length > 35 ? ConsigneeDetails.Name.ToString().Substring(0, 35) : ConsigneeDetails.Name));
                        sbPRI.AppendLine("/" + (ConsigneeDetails.StreetAddress.Length > 35 ? ConsigneeDetails.StreetAddress.ToString().Substring(0, 35) : ConsigneeDetails.StreetAddress));
                        sbPRI.AppendLine("/" + ConsigneeDetails.City + (ConsigneeDetails.State != string.Empty ? "/" + ConsigneeDetails.State : string.Empty));
                        sbPRI.AppendLine("/" + ConsigneeDetails.CountryCode + (ConsigneeDetails.PostalCode != string.Empty ? "/" + ConsigneeDetails.PostalCode : string.Empty) + (ConsigneeDetails.TelephoneNumber != string.Empty ? "/" + ConsigneeDetails.TelephoneNumber : string.Empty));

                    }
                    //Other Party Information Details
                    if (OPIDetails.PartyType != string.Empty && OPIDetails.Name != string.Empty && OPIDetails.City != string.Empty && OPIDetails.CountryCode != string.Empty)
                    {
                        sbPRI.AppendLine(OPIDetails.ComponentIdentifier + "/" + OPIDetails.PartyType + "/" + (OPIDetails.Name.Length > 35 ? OPIDetails.Name.ToString().Substring(0, 35) : OPIDetails.Name));
                        sbPRI.AppendLine("/" + (OPIDetails.StreetAddress.Length > 35 ? OPIDetails.StreetAddress.ToString().Substring(0, 35) : OPIDetails.StreetAddress));
                        sbPRI.AppendLine("/" + OPIDetails.City + (OPIDetails.State != string.Empty ? "/" + OPIDetails.State : string.Empty));
                        sbPRI.AppendLine("/" + OPIDetails.CountryCode + (OPIDetails.PostalCode != string.Empty ? "/" + OPIDetails.PostalCode : string.Empty) + (OPIDetails.TelephoneNumber != string.Empty ? "/" + OPIDetails.TelephoneNumber : string.Empty));
                        if (OPIDetails.PartyInfoType != string.Empty && OPIDetails.PartyInfo != string.Empty)
                        {
                            sbPRI.AppendLine("/" + OPIDetails.PartyInfoType + "/" + OPIDetails.PartyInfo);
                        }
                    }
                    //Transfer Details
                    foreach (TRN TransDetails in TransferDetails)
                    {
                        if (TransDetails.DestinationAirport != string.Empty)
                        {
                            sbPRI.AppendLine(TransDetails.ComponentIdentifier + "/" + TransDetails.DestinationAirport + (TransDetails.DestinationAirport != "000" ? "-" + TransDetails.DomesticInternationIdentifier : string.Empty) +
                                (TransDetails.BondedCarrierID != string.Empty || TransDetails.OnwardCarrier != string.Empty ? "/" + TransDetails.BondedCarrierID + TransDetails.OnwardCarrier : string.Empty) +
                                (TransDetails.BondedPremisesIdentifier != string.Empty || TransDetails.InBondControlNumber != string.Empty ? "/" + TransDetails.BondedPremisesIdentifier + TransDetails.InBondControlNumber : string.Empty));
                        }
                    }
                    //CBP Shipment Description CSD
                    if (CSDDetails.DeclaredValue != 0 && CSDDetails.ISOCurrencyCode != string.Empty)
                    {
                        sbPRI.AppendLine(CSDDetails.ComponentIdentifier + "/" + CSDDetails.OriginOfGoods + "/" + CSDDetails.DeclaredValue.ToString() +
                            "-" + CSDDetails.ISOCurrencyCode + (CSDDetails.HarmonizedCommodityCode != string.Empty ? "/" + CSDDetails.HarmonizedCommodityCode : string.Empty));
                    }

                    //FDA Freight Indicator
                    if (FDA.FDADetails != string.Empty)
                    {
                        sbPRI.AppendLine(FDA.FDADetails);
                    }

                    //Other Customs Information OCI
                    if (OCIDetails.CustomsInfo != string.Empty)
                    {
                        sbPRI.AppendLine(OCIDetails.ComponentIdentifier + "/" + OCIDetails.CountryCode + "/" + OCIDetails.InformationIdentifier +
                            "/" + OCIDetails.CustomsInfo + (OCIDetails.SupplementaryInfo != string.Empty ? "/" + OCIDetails.SupplementaryInfo : string.Empty));
                    }

                    return sbPRI.ToString();
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
                    return string.Empty;
                }
            }
            #endregion

            public PRI Encode(DataSet ds)
            {
                try
                {
                    PRI PRI = new PRI();
                    if (ds != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                foreach (DataRow row in ds.Tables[0].Rows)
                                {

                                    PRI.StandardMessageIdentifier.StandardMessageIdentifier = row["StandardMessageIdentifier"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[1].Rows)
                                {

                                    PRI.CCLDetails.AirportOfArrival = row["AirportOfArrival"].ToString();
                                    PRI.CCLDetails.CargoTerminalOperator = row["CargoTerminalOperator"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[2].Rows)
                                {

                                    Array.Resize(ref PRI.AWBDetails, PRI.AWBDetails.Length + 1);
                                    PRI.AWBDetails[PRI.AWBDetails.Length - 1] = new AWB1();
                                    PRI.AWBDetails[PRI.AWBDetails.Length - 1].AWBPrefix = row["AWBPrefix"].ToString();
                                    PRI.AWBDetails[PRI.AWBDetails.Length - 1].AWBNumber = row["AWBNumber"].ToString();
                                    PRI.AWBDetails[PRI.AWBDetails.Length - 1].ConsolidationIdentifier = row["ConsolidationIdentifier"].ToString();
                                    PRI.AWBDetails[PRI.AWBDetails.Length - 1].HAWBNumber = row["HAWBNumber"].ToString();
                                    PRI.AWBDetails[PRI.AWBDetails.Length - 1].PackageTrackingIdentifier = row["PackageTrackingIdentifier"].ToString();

                                }
                                foreach (DataRow row in ds.Tables[3].Rows)
                                {
                                    Array.Resize(ref PRI.WBLDetails, PRI.WBLDetails.Length + 1);
                                    PRI.WBLDetails[PRI.WBLDetails.Length - 1] = new WBL();
                                    PRI.WBLDetails[PRI.WBLDetails.Length - 1].ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    PRI.WBLDetails[PRI.WBLDetails.Length - 1].AirportOfOrigin = row["AirportOfOrigin"].ToString();
                                    PRI.WBLDetails[PRI.WBLDetails.Length - 1].ArrivalDatePermitToProceed = row["ArrivalDatePermitToProceed"].ToString();
                                    PRI.WBLDetails[PRI.WBLDetails.Length - 1].CargoDescription = row["CargoDescription"].ToString();
                                    PRI.WBLDetails[PRI.WBLDetails.Length - 1].NumberOfPieces = row["NumberOfPieces"].ToString() != string.Empty ? Convert.ToInt32(row["NumberOfPieces"]) : 0;
                                    PRI.WBLDetails[PRI.WBLDetails.Length - 1].PermitToProceedDestAirport = row["PermitToProceedDestAirport"].ToString();
                                    PRI.WBLDetails[PRI.WBLDetails.Length - 1].ShipmentDescriptionCode = row["ShipmentDescriptionCode"].ToString();
                                    PRI.WBLDetails[PRI.WBLDetails.Length - 1].Weight = row["Weight"].ToString() != string.Empty ? Double.Parse(row["Weight"].ToString()) : 0;
                                    PRI.WBLDetails[PRI.WBLDetails.Length - 1].WeightCode = row["WeightCode"].ToString();

                                }
                                foreach (DataRow row in ds.Tables[4].Rows)
                                {
                                    Array.Resize(ref PRI.ArrivalDetails, PRI.ArrivalDetails.Length + 1);
                                    PRI.ArrivalDetails[PRI.ArrivalDetails.Length - 1] = new ARR1();
                                    PRI.ArrivalDetails[PRI.ArrivalDetails.Length - 1].BoardedPieceCount = row["BoardedPieceCount"].ToString() != string.Empty ? Convert.ToInt32(row["BoardedPieceCount"]) : 0;
                                    PRI.ArrivalDetails[PRI.ArrivalDetails.Length - 1].BoardedQuantityIdentifier = row["BoardedQuantityIdentifier"].ToString();
                                    PRI.ArrivalDetails[PRI.ArrivalDetails.Length - 1].ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    PRI.ArrivalDetails[PRI.ArrivalDetails.Length - 1].FlightNumber = row["FlightNumber"].ToString();
                                    PRI.ArrivalDetails[PRI.ArrivalDetails.Length - 1].ImportingCarrier = row["ImportingCarrier"].ToString();
                                    PRI.ArrivalDetails[PRI.ArrivalDetails.Length - 1].PartArrivalReference = row["PartArrivalReference"].ToString();
                                    PRI.ArrivalDetails[PRI.ArrivalDetails.Length - 1].ScheduledArrivalDate = row["ScheduledArrivalDate"].ToString();
                                    PRI.ArrivalDetails[PRI.ArrivalDetails.Length - 1].Weight = row["Weight"].ToString() != string.Empty ? Double.Parse(row["Weight"].ToString()) : 0;
                                    PRI.ArrivalDetails[PRI.ArrivalDetails.Length - 1].WeightCode = row["WeightCode"].ToString();

                                }
                                foreach (DataRow row in ds.Tables[5].Rows)
                                {
                                    PRI.AgentDetails.AirAMSParticipantCode = row["AirAMSParticipantCode"].ToString();
                                    PRI.AgentDetails.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[6].Rows)
                                {
                                    PRI.ShipperDetails.City = row["City"].ToString();
                                    PRI.ShipperDetails.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    PRI.ShipperDetails.CountryCode = row["CountryCode"].ToString();
                                    PRI.ShipperDetails.Name = row["Name"].ToString();
                                    PRI.ShipperDetails.PostalCode = row["PostalCode"].ToString();
                                    PRI.ShipperDetails.State = row["State"].ToString();
                                    PRI.ShipperDetails.StreetAddress = row["StreetAddress"].ToString();
                                }

                                foreach (DataRow row in ds.Tables[7].Rows)
                                {
                                    PRI.ConsigneeDetails.City = row["City"].ToString();
                                    PRI.ConsigneeDetails.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    PRI.ConsigneeDetails.CountryCode = row["CountryCode"].ToString();
                                    PRI.ConsigneeDetails.Name = row["Name"].ToString();
                                    PRI.ConsigneeDetails.PostalCode = row["PostalCode"].ToString();
                                    PRI.ConsigneeDetails.State = row["State"].ToString();
                                    PRI.ConsigneeDetails.StreetAddress = row["StreetAddress"].ToString();
                                    PRI.ConsigneeDetails.TelephoneNumber = row["TelephoneNumber"].ToString();
                                }

                                foreach (DataRow row in ds.Tables[8].Rows)
                                {
                                    Array.Resize(ref PRI.TransferDetails, PRI.TransferDetails.Length + 1);
                                    PRI.TransferDetails[PRI.TransferDetails.Length - 1] = new TRN();
                                    PRI.TransferDetails[PRI.TransferDetails.Length - 1].BondedCarrierID = row["BondedCarrierID"].ToString();
                                    PRI.TransferDetails[PRI.TransferDetails.Length - 1].BondedPremisesIdentifier = row["BondedPremisesIdentifier"].ToString();
                                    PRI.TransferDetails[PRI.TransferDetails.Length - 1].ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    PRI.TransferDetails[PRI.TransferDetails.Length - 1].DestinationAirport = row["DestinationAirport"].ToString();
                                    PRI.TransferDetails[PRI.TransferDetails.Length - 1].DomesticInternationIdentifier = row["DomesticInternationIdentifier"].ToString();
                                    PRI.TransferDetails[PRI.TransferDetails.Length - 1].InBondControlNumber = row["InBondControlNumber"].ToString();
                                    PRI.TransferDetails[PRI.TransferDetails.Length - 1].OnwardCarrier = row["OnwardCarrier"].ToString();

                                }

                                foreach (DataRow row in ds.Tables[9].Rows)
                                {
                                    PRI.CSDDetails.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    PRI.CSDDetails.DeclaredValue = row["DeclaredValue"].ToString() != string.Empty ? Double.Parse(row["DeclaredValue"].ToString()) : 0;
                                    PRI.CSDDetails.HarmonizedCommodityCode = row["HarmonizedCommodityCode"].ToString();
                                    PRI.CSDDetails.ISOCurrencyCode = row["ISOCurrencyCode"].ToString();
                                    PRI.CSDDetails.OriginOfGoods = row["OriginOfGoods"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[10].Rows)
                                {
                                    PRI.FDA.FDADetails = row["FDADetails"].ToString();
                                }


                                if (ds.Tables.Count > 11)
                                {
                                    foreach (DataRow row in ds.Tables[11].Rows)
                                    {
                                        PRI.OPIDetails.City = row["City"].ToString();
                                        PRI.OPIDetails.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                        PRI.OPIDetails.CountryCode = row["CountryCode"].ToString();
                                        PRI.OPIDetails.Name = row["Name"].ToString();
                                        PRI.OPIDetails.PostalCode = row["PostalCode"].ToString();
                                        PRI.OPIDetails.State = row["State"].ToString();
                                        PRI.OPIDetails.StreetAddress = row["StreetAddress"].ToString();
                                        PRI.OPIDetails.TelephoneNumber = row["TelephoneNumber"].ToString();
                                        PRI.OPIDetails.PartyType = row["PartyType"].ToString();
                                        PRI.OPIDetails.PartyInfoType = row["PartyInfoType"].ToString();
                                        PRI.OPIDetails.PartyInfo = row["PartyInfo"].ToString();
                                    }
                                }

                                if (ds.Tables.Count > 12)
                                {
                                    foreach (DataRow row in ds.Tables[12].Rows)
                                    {
                                        PRI.OCIDetails.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                        PRI.OCIDetails.CountryCode = row["CountryCode"].ToString();
                                        PRI.OCIDetails.InformationIdentifier = row["InformationIdentifier"].ToString();
                                        PRI.OCIDetails.CustomsInfo = row["CustomsInfo"].ToString();
                                        PRI.OCIDetails.SupplementaryInfo = row["SupplementaryInfo"].ToString();
                                    }
                                }

                                return PRI;

                            }

                        }

                    }
                    return null;
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
                    return null;
                }
            }


        }
        #endregion

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
                    StringBuilder sbFRI = new StringBuilder();
                    //Message Identifier
                    sbFRI.AppendLine(StandardMessageIdentifier.StandardMessageIdentifier);
                    //Cargo Control Location Details
                    if (CCLDetails.AirportOfArrival != string.Empty && CCLDetails.CargoTerminalOperator != string.Empty)
                    {
                        sbFRI.AppendLine(CCLDetails.AirportOfArrival.ToString() + CCLDetails.CargoTerminalOperator);
                    }
                    //AWB Details
                    foreach (AWB1 AWBs in AWBDetails)
                    {
                        sbFRI.AppendLine(AWBs.AWBPrefix + "-" + AWBs.AWBNumber + ((AWBs.ConsolidationIdentifier != string.Empty || AWBs.HAWBNumber != string.Empty) ? "-" + AWBs.ConsolidationIdentifier + AWBs.HAWBNumber : string.Empty) + (AWBs.PackageTrackingIdentifier != string.Empty ? "/" + AWBs.PackageTrackingIdentifier : string.Empty));
                    }

                    //Arrival Details
                    foreach (ARR2 ArrivalDet in ArrivalDetails)
                    {
                        sbFRI.AppendLine(ArrivalDet.ComponentIdentifier + "/" + ArrivalDet.ImportingCarrier + ArrivalDet.FlightNumber + "/" + ArrivalDet.ScheduledArrivalDate + (ArrivalDet.PartArrivalReference != string.Empty ? "-" + ArrivalDet.PartArrivalReference : String.Empty));
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
                        sbFRI.AppendLine(strTemp);
                    }

                    return sbFRI.ToString();
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
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
                    clsLog.WriteLogAzure(ex.Message);
                    return null;
                }
            }


        }
        #endregion

        #region PER

        [Serializable]
        public class PER
        {
            //Message Type (Note : Three Characters). 
            public SMI StandardMessageIdentifier = new SMI();
            //AWB Details
            public AWB1[] AWBDetails = new AWB1[0];
            public ERF ERF = new ERF();
            public ERR ERR = new ERR();
            #region Overriding ToString Method for PER
            public override string ToString()
            {

                try
                {
                    StringBuilder sbPER = new StringBuilder();
                    //Message Identifier
                    sbPER.AppendLine(StandardMessageIdentifier.StandardMessageIdentifier);

                    //AWB Details
                    foreach (AWB1 AWBs in AWBDetails)
                    {
                        sbPER.AppendLine(AWBs.AWBPrefix + "-" + AWBs.AWBNumber + ((AWBs.ConsolidationIdentifier != string.Empty || AWBs.HAWBNumber != string.Empty) ? "-" + AWBs.ConsolidationIdentifier + AWBs.HAWBNumber : string.Empty) + (AWBs.PackageTrackingIdentifier != string.Empty ? "/" + AWBs.PackageTrackingIdentifier : string.Empty));
                    }
                    if (!String.IsNullOrEmpty(ERR.ComponentIdentifier))
                    {
                        sbPER.AppendLine(ERR.ComponentIdentifier + "/" + ERR.ErrorCode + ERR.ErrorMessageText);
                    }

                    sbPER.AppendLine(ERF.ImportingCarrier + ERF.FlightNumber + "/" + ERF.Date);  //  before date '/' is compulsory.


                    return sbPER.ToString();
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
                    return string.Empty;
                }
            }
            #endregion

            public PER Encode(DataSet ds)
            {
                try
                {
                    PER PER = new PER();
                    if (ds != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                foreach (DataRow row in ds.Tables[0].Rows)
                                {

                                    PER.StandardMessageIdentifier.StandardMessageIdentifier = row["StandardMessageIdentifier"].ToString();
                                }
                                foreach (DataRow row in ds.Tables[1].Rows)
                                {

                                    try
                                    {
                                        ERF.ImportingCarrier = row["ImportingCarrier"].ToString();
                                        ERF.FlightNumber = row["FlightNumber"].ToString();
                                        ERF.Date = row["Date"].ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        clsLog.WriteLogAzure(ex.Message);
                                    }

                                }
                                foreach (DataRow row in ds.Tables[2].Rows)
                                {

                                    Array.Resize(ref PER.AWBDetails, PER.AWBDetails.Length + 1);
                                    PER.AWBDetails[PER.AWBDetails.Length - 1] = new AWB1();
                                    PER.AWBDetails[PER.AWBDetails.Length - 1].AWBPrefix = row["AWBPrefix"].ToString();
                                    PER.AWBDetails[PER.AWBDetails.Length - 1].AWBNumber = row["AWBNumber"].ToString();
                                    PER.AWBDetails[PER.AWBDetails.Length - 1].ConsolidationIdentifier = row["ConsolidationIdentifier"].ToString();
                                    PER.AWBDetails[PER.AWBDetails.Length - 1].HAWBNumber = row["HAWBNumber"].ToString();
                                    PER.AWBDetails[PER.AWBDetails.Length - 1].PackageTrackingIdentifier = row["PackageTrackingIdentifier"].ToString();

                                }

                                foreach (DataRow row in ds.Tables[3].Rows)
                                {
                                    ERR.ComponentIdentifier = row["ComponentIdentifier"].ToString();
                                    ERR.ErrorCode = row["ErrorCode"].ToString();
                                    ERR.ErrorMessageText = row["ErrorMessage"].ToString();
                                }

                                return PER;

                            }

                        }

                    }
                    return null;
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure(ex.Message);
                    return null;
                }
            }


        }
        #endregion

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

        #region Other Party Information (OPI) Class
        public class OPI
        {
            //Must be OPI.
            public string ComponentIdentifier = "OPI";
            //The type of party.
            public string PartyType = string.Empty;
            //Name of the Other Party
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
            //Hyphens may be used.
            public string TelephoneNumber = string.Empty;
            //Party Information Type
            public string PartyInfoType = string.Empty;
            //Party Information
            public string PartyInfo = string.Empty;

        }
        #endregion

        #region Other Customs Information (OCI) Class
        public class OCI
        {
            //Must be OCI.
            public string ComponentIdentifier = "OCI";
            //ISO Country Code
            public string CountryCode = string.Empty;
            //Information Identifier
            public string InformationIdentifier = string.Empty;
            //Customs, Security and Regulatory Control Information Identifier
            public string CustomsInfo = string.Empty;
            //Supplementary Customs, Security and Regulatory Control Information
            public string SupplementaryInfo = string.Empty;

        }
        #endregion

        #endregion
        public PRI[] EncodingBulkPRIMessage(object[] QueryValues)
        {
            DataSet Dset = new DataSet("Dset_CustomsImportBAL_EncodingPRIMessage");
            string key = String.Empty;

            try
            {
                string[] QueryNames = { "AWBNumber", "FlightNo", "FlightDate", "HAWBNumber" };
                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar };
                StringBuilder[] sb = new StringBuilder[0];
                SQLServer db = new SQLServer();

                Dset = db.SelectRecords("sp_GetBulkPRIDataAutoMsg_HAWB", QueryNames, QueryValues, QueryTypes);

                if (Dset != null)
                {
                    if (Dset.Tables.Count > 0)
                    {
                        PRI[] priList = new PRI[Dset.Tables[0].Rows.Count];

                        for (int rCount = 0; rCount < Dset.Tables[0].Rows.Count; rCount++)
                        {
                            DataSet Dset2 = new DataSet("Dset_CustomsImportBAL_EncodingPRIMessage");
                            key = Convert.ToString(Dset.Tables[0].Rows[rCount]["RowKey"]);

                            for (int tCount = 0; tCount < Dset.Tables.Count; tCount++)
                            {
                                DataTable dt = Dset.Tables[tCount].Clone();
                                DataRow[] drList = Dset.Tables[tCount].Select("RowKey = '" + key + "'");

                                foreach (DataRow dr in drList)
                                    dt.ImportRow(dr);

                                Dset2.Tables.Add(dt);
                                dt = null;
                            }

                            PRI EncodePRI = new PRI();
                            EncodePRI = EncodePRI.Encode(Dset2);
                            priList[rCount] = EncodePRI;
                            Dset2 = null;
                        }

                        return priList;
                    }
                    else
                    {
                        db = null;
                        Dset.Dispose();
                        return null;
                    }
                }
                else
                {
                    db = null;
                    return null;
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                db = null;
                return null;
            }
            finally
            {
                Dset = null;
            }
        }

        #region Serialize & DeSerialize Methods
        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="fileName"></param>

        public void SerializeObject<T>(T serializableObject, string fileName)
        {
            //if (serializableObject == null) { return; }
            if (object.Equals(serializableObject, default(T))) { return; }
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(AppDomain.CurrentDomain.BaseDirectory + "\\CASS\\" + fileName);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                //Log exception here
            }
        }


        /// <summary>
        /// Deserializes an xml file into an object list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(AppDomain.CurrentDomain.BaseDirectory + "\\CASS\\" + fileName)) { return default(T); }

            T objectOut = default(T);

            try
            {
                string attributeXml = string.Empty;

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(AppDomain.CurrentDomain.BaseDirectory + "\\CASS\\" + fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                //Log exception here
            }

            return objectOut;
        }

        #endregion

        #region Update Method for FRI object Based
        public void readQueryValuesFRI(CustomsImportBAL.FRI objFRI, ref object[] QueryValues, string EncodedMessage, string FlightNo, DateTime FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {

                CustomsImportBAL.SMI objSMI = objFRI.StandardMessageIdentifier;
                CustomsImportBAL.AWB1 objAWB1 = objFRI.AWBDetails[objFRI.AWBDetails.Length - 1];
                CustomsImportBAL.CCL objCCL = objFRI.CCLDetails;
                CustomsImportBAL.WBL objWBL = objFRI.WBLDetails[objFRI.WBLDetails.Length - 1];
                CustomsImportBAL.ARR1 objARR = objFRI.ArrivalDetails[objFRI.ArrivalDetails.Length - 1];
                CustomsImportBAL.AGT objAGT = objFRI.AgentDetails;
                CustomsImportBAL.SHP objSHP = objFRI.ShipperDetails;
                CustomsImportBAL.CNE objCNE = objFRI.ConsigneeDetails;
                CustomsImportBAL.TRN objTRN = objFRI.TransferDetails.Length > 0 ? objFRI.TransferDetails[objFRI.TransferDetails.Length - 1] : null;
                CustomsImportBAL.CSD objCSD = objFRI.CSDDetails;
                CustomsImportBAL.FDA objFDA = objFRI.FDA;
                CustomsImportBAL.RFA objRFA = null;
                CustomsImportBAL.DEP objDEP = null;







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
                    QueryValues[valNo++] = objCCL.CargoTerminalOperator;//txtImportingCarrier.Text.Trim();
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
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                }

                //------------------ARR------------------------------
                if (objARR != null)
                {
                    QueryValues[valNo++] = objARR.ScheduledArrivalDate;
                    QueryValues[valNo++] = objARR.PartArrivalReference;
                    QueryValues[valNo++] = objARR.BoardedQuantityIdentifier;
                    QueryValues[valNo++] = objARR.BoardedPieceCount;
                    QueryValues[valNo++] = objARR.Weight;
                    //New field added for arrival weight code
                    QueryValues[valNo++] = objARR.WeightCode;
                    //end
                    QueryValues[valNo++] = objARR.ImportingCarrier;
                    QueryValues[valNo++] = objARR.FlightNumber;
                    QueryValues[valNo++] = objARR.PartArrivalReference;

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
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";


                }

                //-----------------------FDA--------------------------------

                QueryValues[valNo++] = objFDA.FDADetails;

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

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";



                //-------------------FSQ----------------------------------

                QueryValues[valNo++] = "";



                //-------------------FSC-----------------------------------

                QueryValues[valNo++] = "";


                //------------------TXT------------------------------------

                QueryValues[valNo++] = "";


                //------------------ERF------------------------------------
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";

                //------------------COMMON---------------------------------------
                QueryValues[valNo++] = EncodedMessage;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = FlightNo;
                QueryValues[valNo++] = FlightDate;
                if (objCCL != null)
                {
                    QueryValues[valNo++] = objCCL.AirportOfArrival;
                }
                else
                {
                    QueryValues[valNo++] = "";
                }
                //WBL Permit to Proceed Arrival Date
                QueryValues[valNo++] = objWBL != null ? objWBL.ArrivalDatePermitToProceed : string.Empty;

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
            }
            //return null;
        }
        #endregion

        #region Update Method for FDM object
        public void readQueryValuesFDM(CustomsImportBAL.FDM objFDM, ref object[] QueryValues, string EncodedMessage, string AWBPrefix, string AWBNumber, string FlightNo, DateTime FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {

                CustomsImportBAL.SMI objSMI = objFDM.StandardMessageIdentifier;
                // CustomsImportBAL.AWB1 objAWB1 = null;
                //  CustomsImportBAL.CCL objCCL = null;
                //CustomsImportBAL.WBL objWBL = null;
                //CustomsImportBAL.ARR1 objARR = null;
                //CustomsImportBAL.AGT objAGT = null;
                //CustomsImportBAL.SHP objSHP = null;
                //CustomsImportBAL.CNE objCNE = null;
                //CustomsImportBAL.TRN objTRN = null;
                //CustomsImportBAL.CSD objCSD = null;
                //CustomsImportBAL.RFA objRFA = null;
                CustomsImportBAL.DEP objDEP = objFDM.DEP;
                // CustomsImportBAL.ERR objERR = null;
                // CustomsImportBAL.ERF objERF = null;





                //String strMsg = txtMsgType.Text.Trim().ToUpper();

                //--------------------AWB----------------------------------
                //if (objAWB1 != null)  //Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.
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




                //if (objWBL != null)
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
                // }


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
                //}

                //------------------COMMON---------------------------------------
                QueryValues[valNo++] = EncodedMessage;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = FlightNo;
                QueryValues[valNo++] = FlightDate;
                //if (objCCL != null)
                ////{
                //    QueryValues[valNo++] = objCCL.AirportOfArrival;
                //}
                //else
                //{
                QueryValues[valNo++] = "";
                // }

                //QueryValues[valNo++] = objWBL != null ? objWBL.ArrivalDatePermitToProceed : string.Empty;

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            //return null;
        }
        #endregion

        #region Update Method for FRC object
        public void readQueryValuesFRC(CustomsImportBAL.FRC objFRC, ref object[] QueryValues, string EncodedMessage, string FlightNo, DateTime FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {
                CustomsImportBAL.SMI objSMI = objFRC.StandardMessageIdentifier;
                CustomsImportBAL.AWB1 objAWB1 = objFRC.AWBDetails[objFRC.AWBDetails.Length - 1];
                CustomsImportBAL.CCL objCCL = objFRC.CCLDetails;
                CustomsImportBAL.WBL objWBL = objFRC.WBLDetails[objFRC.WBLDetails.Length - 1];
                CustomsImportBAL.ARR1 objARR = objFRC.ArrivalDetails[objFRC.ArrivalDetails.Length - 1];
                CustomsImportBAL.AGT objAGT = objFRC.AgentDetails;
                CustomsImportBAL.SHP objSHP = objFRC.ShipperDetails;
                CustomsImportBAL.CNE objCNE = objFRC.ConsigneeDetails;
                CustomsImportBAL.TRN objTRN = objFRC.TransferDetails.Length > 0 ? objFRC.TransferDetails[0] : null;
                CustomsImportBAL.CSD objCSD = objFRC.CSDDetails;
                CustomsImportBAL.RFA objRFA = objFRC.RFA; ;
                CustomsImportBAL.DEP objDEP = null;
                CustomsImportBAL.FDA objFDA = objFRC.FDA;





                //--------------------AWB----------------------------------
                if (objAWB1 != null)
                {
                    QueryValues[valNo++] = objAWB1.AWBPrefix.Trim();
                    QueryValues[valNo++] = objAWB1.AWBNumber.Trim();
                    QueryValues[valNo++] = objFRC.StandardMessageIdentifier.StandardMessageIdentifier;
                    QueryValues[valNo++] = objAWB1.HAWBNumber.Trim();
                    QueryValues[valNo++] = objAWB1.ConsolidationIdentifier.Trim();
                    QueryValues[valNo++] = objAWB1.PackageTrackingIdentifier;

                    QueryValues[valNo++] = "";//   only for fsq ans fsc txtPartArrival.Text.Trim();
                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = objFRC.StandardMessageIdentifier.StandardMessageIdentifier;
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
                    QueryValues[valNo++] = "0";
                    QueryValues[valNo++] = "";
                }

                //------------------ARR------------------------------
                if (objARR != null)
                {
                    QueryValues[valNo++] = objARR.ScheduledArrivalDate;
                    QueryValues[valNo++] = objARR.PartArrivalReference;
                    QueryValues[valNo++] = objARR.BoardedQuantityIdentifier;
                    QueryValues[valNo++] = objARR.BoardedPieceCount;
                    QueryValues[valNo++] = objARR.Weight;
                    QueryValues[valNo++] = objARR.WeightCode;
                    QueryValues[valNo++] = objARR.ImportingCarrier;
                    QueryValues[valNo++] = objARR.FlightNumber;
                    QueryValues[valNo++] = objARR.PartArrivalReference;

                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "0";
                    QueryValues[valNo++] = "0";
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

                QueryValues[valNo++] = objFDA.FDADetails;

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

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";

                //----------------------------CSN---------------------------

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "0";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";



                //------------------ERR-----------------------------------

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";




                //-------------------FSQ----------------------------------

                QueryValues[valNo++] = "";


                //-------------------FSC-----------------------------------

                QueryValues[valNo++] = "";


                //------------------TXT------------------------------------

                QueryValues[valNo++] = "";

                //------------------ERF------------------------------------

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";

                //------------------COMMON---------------------------------------
                QueryValues[valNo++] = EncodedMessage;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = FlightNo;
                QueryValues[valNo++] = FlightDate;
                if (objCCL != null)
                {
                    QueryValues[valNo++] = objCCL.AirportOfArrival;
                }
                else
                {
                    QueryValues[valNo++] = "";
                }
                //WBL Permit to Proceed Arrival Date
                QueryValues[valNo++] = objWBL != null ? objWBL.ArrivalDatePermitToProceed : string.Empty;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
            }
            //return null;
        }
        #endregion

        #region Update Method for FRX object
        public void readQueryValuesFRX(CustomsImportBAL.FRX objFRX, ref object[] QueryValues, string EncodedMessage, string FlightNo, DateTime FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {
                CustomsImportBAL.SMI objSMI = objFRX.StandardMessageIdentifier;
                CustomsImportBAL.AWB1 objAWB1 = objFRX.AWBDetails[objFRX.AWBDetails.Length - 1];
                CustomsImportBAL.CCL objCCL = objFRX.CCLDetails;
                CustomsImportBAL.WBL objWBL = null;
                CustomsImportBAL.ARR2 objARR = objFRX.ArrivalDetails[objFRX.ArrivalDetails.Length - 1];
                CustomsImportBAL.AGT objAGT = null;
                CustomsImportBAL.SHP objSHP = null;
                CustomsImportBAL.CNE objCNE = null;
                CustomsImportBAL.TRN objTRN = null;
                CustomsImportBAL.CSD objCSD = null;
                CustomsImportBAL.RFA objRFA = objFRX.RFA; ;
                CustomsImportBAL.DEP objDEP = null;
                CustomsImportBAL.ERR objERR = null;
                CustomsImportBAL.ERF objERF = null;






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


                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";




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
                QueryValues[valNo++] = FlightDate;
                if (objCCL != null)
                {
                    QueryValues[valNo++] = objCCL.AirportOfArrival;
                }
                else
                {
                    QueryValues[valNo++] = "";
                }
                //WBL Permit to Proceed Arrival Date
                QueryValues[valNo++] = objWBL != null ? objWBL.ArrivalDatePermitToProceed : string.Empty;

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
            }
            //return null;
        }
        #endregion

        #region Update Method for FSN object
        public void readQueryValuesFSN(CustomsImportBAL.FSN objFSN, ref object[] QueryValues, string EncodedMessage, string FlightNo, DateTime FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {
                CustomsImportBAL.SMI objSMI = objFSN.StandardMessageIdentifier;
                CustomsImportBAL.AWB1 objAWB1 = objFSN.AWBDetails[0];
                CustomsImportBAL.CCL objCCL = objFSN.CCLDetails;
                CustomsImportBAL.WBL objWBL = null;
                CustomsImportBAL.ARR2 objARR = objFSN.ArrivalDetails[0];
                CustomsImportBAL.ASN objASN = objFSN.ASN; ;
                CustomsImportBAL.AGT objAGT = null;
                CustomsImportBAL.SHP objSHP = null;
                CustomsImportBAL.CNE objCNE = null;
                CustomsImportBAL.TRN objTRN = null;
                CustomsImportBAL.CSD objCSD = null;
                CustomsImportBAL.RFA objRFA = null;
                CustomsImportBAL.DEP objDEP = null;
                CustomsImportBAL.ERR objERR = null;
                CustomsImportBAL.ERF objERF = null;

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
                QueryValues[valNo++] = FlightDate;
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
                clsLog.WriteLogAzure(ex.Message);
            }

        }
        #endregion

        #region Update Method for FER Message
        public void readQueryValuesFER(CustomsImportBAL.FER objFER, ref object[] QueryValues, string EncodedMessage, string FlightNo, DateTime FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {

                CustomsImportBAL.SMI objSMI = objFER.StandardMessageIdentifier;
                CustomsImportBAL.AWB1 objAWB1 = objFER.AWBDetails[0];
                if (objFER.AWBDetails.Length > 1)
                    objAWB1 = objFER.AWBDetails[objFER.AWBDetails.Length - 1];
                CustomsImportBAL.CCL objCCL = null;
                CustomsImportBAL.WBL objWBL = null;
                CustomsImportBAL.ARR1 objARR = null;
                CustomsImportBAL.AGT objAGT = null;
                CustomsImportBAL.SHP objSHP = null;
                CustomsImportBAL.CNE objCNE = null;
                CustomsImportBAL.TRN objTRN = null;
                CustomsImportBAL.CSD objCSD = null;
                CustomsImportBAL.RFA objRFA = null;
                CustomsImportBAL.DEP objDEP = null;
                CustomsImportBAL.ERR objERR = objFER.ERR[0];
                if (objFER.ERR.Length > 1)
                    objERR = objFER.ERR[objFER.ERR.Length - 1];
                CustomsImportBAL.ERF objERF = objFER.ERF;







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


                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";



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
                    QueryValues[valNo++] = "0";
                    QueryValues[valNo++] = "";
                }

                //------------------ARR------------------------------
                if (objARR != null)
                {
                    QueryValues[valNo++] = objARR.ScheduledArrivalDate;
                    QueryValues[valNo++] = objARR.PartArrivalReference;
                    QueryValues[valNo++] = objARR.BoardedQuantityIdentifier;
                    QueryValues[valNo++] = objARR.BoardedPieceCount;
                    QueryValues[valNo++] = objARR.Weight;
                    QueryValues[valNo++] = objARR.WeightCode;
                    QueryValues[valNo++] = objARR.ImportingCarrier;
                    QueryValues[valNo++] = objARR.FlightNumber;
                    QueryValues[valNo++] = objARR.PartArrivalReference;

                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "0";
                    QueryValues[valNo++] = "0";
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


                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";


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
                QueryValues[valNo++] = FlightDate;
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
                clsLog.WriteLogAzure(ex.Message);
            }

        }
        #endregion

        #region Update Method for FSQ Message
        public void readQueryValuesFSQ(CustomsImportBAL.FSQ objFSQ, ref object[] QueryValues, string EncodedMessage, string FlightNo, DateTime FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {

                CustomsImportBAL.SMI objSMI = objFSQ.StandardMessageIdentifier;
                CustomsImportBAL.AWB2 objAWB1 = objFSQ.AWBDetails[0];
                if (objFSQ.AWBDetails.Length > 1)
                    objAWB1 = objFSQ.AWBDetails[objFSQ.AWBDetails.Length - 1];
                CustomsImportBAL.CCL objCCL = objFSQ.CCLDetails;
                CustomsImportBAL.WBL objWBL = null;
                CustomsImportBAL.ARR1 objARR = null;
                CustomsImportBAL.AGT objAGT = null;
                CustomsImportBAL.SHP objSHP = null;
                CustomsImportBAL.CNE objCNE = null;
                CustomsImportBAL.TRN objTRN = null;
                CustomsImportBAL.CSD objCSD = null;
                CustomsImportBAL.RFA objRFA = null;
                CustomsImportBAL.DEP objDEP = null;
                CustomsImportBAL.ERR objERR = null;
                CustomsImportBAL.ERF objERF = null;
                CustomsImportBAL.FSQSub objFSQSub = objFSQ.FSQSub;






                //--------------------AWB----------------------------------
                if (objAWB1 != null)
                {
                    QueryValues[valNo++] = objAWB1.AWBPrefix.Trim();
                    QueryValues[valNo++] = objAWB1.AWBNumber.Trim();
                    QueryValues[valNo++] = objSMI.StandardMessageIdentifier;
                    QueryValues[valNo++] = objAWB1.HAWBNumber.Trim();
                    QueryValues[valNo++] = string.Empty;
                    QueryValues[valNo++] = string.Empty;

                    QueryValues[valNo++] = objAWB1.ArrivalReference;//   only for fsq ans fsc txtPartArrival.Text.Trim();
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
                    QueryValues[valNo++] = objCCL.CargoTerminalOperator;//txtImportingCarrier.Text.Trim();
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
                    QueryValues[valNo++] = "0";
                    QueryValues[valNo++] = "";
                }

                //------------------ARR------------------------------
                if (objARR != null)
                {
                    QueryValues[valNo++] = objARR.ScheduledArrivalDate;
                    QueryValues[valNo++] = objARR.PartArrivalReference;
                    QueryValues[valNo++] = objARR.BoardedQuantityIdentifier;
                    QueryValues[valNo++] = objARR.BoardedPieceCount;
                    QueryValues[valNo++] = objARR.Weight;
                    QueryValues[valNo++] = objARR.WeightCode;
                    QueryValues[valNo++] = objARR.ImportingCarrier;
                    QueryValues[valNo++] = objARR.FlightNumber;
                    QueryValues[valNo++] = objARR.PartArrivalReference;

                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "0";
                    QueryValues[valNo++] = "0";
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

                QueryValues[valNo++] = "";// ddlDescriptionCountryCode.Text.Trim();

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
                    QueryValues[valNo++] = objRFA.AmendmentCode;//.SelectedItem.Value;
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


                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";


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
                    QueryValues[valNo++] = objERR.ErrorCode; //.SelectedItem.Text.Trim();
                    QueryValues[valNo++] = objERR.ErrorMessageText;
                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                }



                //-------------------FSQ----------------------------------
                if (objFSQ.FSQSub != null)
                    QueryValues[valNo++] = objFSQ.FSQSub.StatusRequestCode;
                else
                    QueryValues[valNo++] = string.Empty;


                //-------------------FSC-----------------------------------
                QueryValues[valNo++] = "";


                //------------------TXT------------------------------------

                QueryValues[valNo++] = "";

                //------------------ERF------------------------------------
                if (objERF != null)
                {
                    QueryValues[valNo++] = objERF.ImportingCarrier; //.Text.Trim();
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
                QueryValues[valNo++] = FlightDate;
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
                clsLog.WriteLogAzure(ex.Message);
            }
            //return null;
        }
        #endregion

        #region Update Method for PRI object Based
        public void readQueryValuesPRI(CustomsImportBAL.PRI objPRI, ref object[] QueryValues, string EncodedMessage, string FlightNo, DateTime FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {

                CustomsImportBAL.SMI objSMI = objPRI.StandardMessageIdentifier;
                CustomsImportBAL.AWB1 objAWB1 = objPRI.AWBDetails[objPRI.AWBDetails.Length - 1];
                CustomsImportBAL.CCL objCCL = objPRI.CCLDetails;
                CustomsImportBAL.WBL objWBL = objPRI.WBLDetails[objPRI.WBLDetails.Length - 1];
                CustomsImportBAL.ARR1 objARR = objPRI.ArrivalDetails[objPRI.ArrivalDetails.Length - 1];
                CustomsImportBAL.AGT objAGT = objPRI.AgentDetails;
                CustomsImportBAL.SHP objSHP = objPRI.ShipperDetails;
                CustomsImportBAL.CNE objCNE = objPRI.ConsigneeDetails;
                CustomsImportBAL.TRN objTRN = objPRI.TransferDetails.Length > 0 ? objPRI.TransferDetails[objPRI.TransferDetails.Length - 1] : null;
                //CustomsImportBAL.TRN objTRN = objPRI.TransferDetails[objPRI.TransferDetails.Length-1];// null;
                CustomsImportBAL.CSD objCSD = objPRI.CSDDetails;
                CustomsImportBAL.FDA objFDA = objPRI.FDA;
                CustomsImportBAL.RFA objRFA = null;
                CustomsImportBAL.DEP objDEP = null;
                CustomsImportBAL.OPI objOPI = objPRI.OPIDetails;
                CustomsImportBAL.OCI objOCI = objPRI.OCIDetails;


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
                    QueryValues[valNo++] = objCCL.CargoTerminalOperator;//txtImportingCarrier.Text.Trim();
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
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                }

                //------------------ARR------------------------------
                if (objARR != null)
                {
                    QueryValues[valNo++] = objARR.ScheduledArrivalDate;
                    QueryValues[valNo++] = objARR.PartArrivalReference;
                    QueryValues[valNo++] = objARR.BoardedQuantityIdentifier;
                    QueryValues[valNo++] = objARR.BoardedPieceCount;
                    QueryValues[valNo++] = objARR.Weight;
                    //New field added for arrival weight code
                    QueryValues[valNo++] = objARR.WeightCode;
                    //end
                    QueryValues[valNo++] = objARR.ImportingCarrier;
                    QueryValues[valNo++] = objARR.FlightNumber;
                    QueryValues[valNo++] = objARR.PartArrivalReference;

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

                QueryValues[valNo++] = "";// ddlDescriptionCountryCode.Text.Trim();

                //-------------------------CSD------------------------------

                if (objCSD != null)
                {

                    QueryValues[valNo++] = objCSD.DeclaredValue;
                    QueryValues[valNo++] = objCSD.ISOCurrencyCode;
                    QueryValues[valNo++] = objCSD.HarmonizedCommodityCode;

                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";


                }

                //-----------------------FDA--------------------------------

                QueryValues[valNo++] = objFDA.FDADetails;

                //-------------------RFA-------------------------------------
                if (objRFA != null)
                {
                    QueryValues[valNo++] = objRFA.AmendmentCode;//.SelectedItem.Value;
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

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";



                //-------------------FSQ----------------------------------

                QueryValues[valNo++] = "";



                //-------------------FSC-----------------------------------

                QueryValues[valNo++] = "";


                //------------------TXT------------------------------------

                QueryValues[valNo++] = "";


                //------------------ERF------------------------------------
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";

                //------------------COMMON---------------------------------------
                QueryValues[valNo++] = EncodedMessage;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = FlightNo;
                QueryValues[valNo++] = FlightDate;
                if (objCCL != null)
                {
                    QueryValues[valNo++] = objCCL.AirportOfArrival;
                }
                else
                {
                    QueryValues[valNo++] = "";
                }

                QueryValues[valNo++] = objWBL != null ? objWBL.ArrivalDatePermitToProceed : string.Empty;

                //---------------------------OPI------------------
                QueryValues[valNo++] = objOPI.PartyType;
                QueryValues[valNo++] = objOPI.PartyInfoType;
                QueryValues[valNo++] = objOPI.PartyInfo;
                QueryValues[valNo++] = objOPI.Name;
                QueryValues[valNo++] = objOPI.StreetAddress;
                QueryValues[valNo++] = objOPI.City;
                QueryValues[valNo++] = objOPI.State;
                QueryValues[valNo++] = objOPI.CountryCode;
                QueryValues[valNo++] = objOPI.PostalCode;
                QueryValues[valNo++] = objOPI.TelephoneNumber;

                //---------------------------OCI------------------
                QueryValues[valNo++] = objOCI.CountryCode;
                QueryValues[valNo++] = objOCI.InformationIdentifier;
                QueryValues[valNo++] = objOCI.CustomsInfo;
                QueryValues[valNo++] = objOCI.SupplementaryInfo;



            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
            }
            //return null;
        }
        #endregion

        #region Update Method for FRI object Based
        public void readQueryValuesFRI(CustomsImportBAL.FRI objFRI, ref object[] QueryValues, string EncodedMessage, string FlightNo, string FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {

                CustomsImportBAL.SMI objSMI = objFRI.StandardMessageIdentifier;
                CustomsImportBAL.AWB1 objAWB1 = objFRI.AWBDetails[objFRI.AWBDetails.Length - 1];
                CustomsImportBAL.CCL objCCL = objFRI.CCLDetails;
                CustomsImportBAL.WBL objWBL = objFRI.WBLDetails[objFRI.WBLDetails.Length - 1];
                CustomsImportBAL.ARR1 objARR = objFRI.ArrivalDetails[objFRI.ArrivalDetails.Length - 1];
                CustomsImportBAL.AGT objAGT = objFRI.AgentDetails;
                CustomsImportBAL.SHP objSHP = objFRI.ShipperDetails;
                CustomsImportBAL.CNE objCNE = objFRI.ConsigneeDetails;
                CustomsImportBAL.TRN objTRN = objFRI.TransferDetails.Length > 0 ? objFRI.TransferDetails[objFRI.TransferDetails.Length - 1] : null;
                //CustomsImportBAL.TRN objTRN = objFRI.TransferDetails[objFRI.TransferDetails.Length-1];// null;
                CustomsImportBAL.CSD objCSD = objFRI.CSDDetails;
                CustomsImportBAL.FDA objFDA = objFRI.FDA;
                CustomsImportBAL.RFA objRFA = null;
                CustomsImportBAL.DEP objDEP = null;



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
                    QueryValues[valNo++] = objCCL.CargoTerminalOperator;//txtImportingCarrier.Text.Trim();
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
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                }

                //------------------ARR------------------------------
                if (objARR != null)
                {
                    QueryValues[valNo++] = objARR.ScheduledArrivalDate;
                    QueryValues[valNo++] = objARR.PartArrivalReference;
                    QueryValues[valNo++] = objARR.BoardedQuantityIdentifier;
                    QueryValues[valNo++] = objARR.BoardedPieceCount;
                    QueryValues[valNo++] = objARR.Weight;
                    //New field added for arrival weight code
                    QueryValues[valNo++] = objARR.WeightCode;
                    //end
                    QueryValues[valNo++] = objARR.ImportingCarrier;
                    QueryValues[valNo++] = objARR.FlightNumber;
                    QueryValues[valNo++] = objARR.PartArrivalReference;

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
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";


                }

                //-----------------------FDA--------------------------------

                QueryValues[valNo++] = objFDA.FDADetails;

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

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";



                //-------------------FSQ----------------------------------

                QueryValues[valNo++] = "";



                //-------------------FSC-----------------------------------

                QueryValues[valNo++] = "";


                //------------------TXT------------------------------------

                QueryValues[valNo++] = "";


                //------------------ERF------------------------------------
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";

                //------------------COMMON---------------------------------------
                QueryValues[valNo++] = EncodedMessage;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = FlightNo;
                QueryValues[valNo++] = FlightDate;
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
                clsLog.WriteLogAzure(ex.Message);
            }

        }
        #endregion

        #region Update Method for FDM object
        public void readQueryValuesFDM(CustomsImportBAL.FDM objFDM, ref object[] QueryValues, string EncodedMessage, string AWBPrefix, string AWBNumber, string FlightNo, string FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {

                CustomsImportBAL.SMI objSMI = objFDM.StandardMessageIdentifier;
                //CustomsImportBAL.AWB1 objAWB1 = null;
                //CustomsImportBAL.CCL objCCL = null;
                // CustomsImportBAL.WBL objWBL = null;
                //CustomsImportBAL.ARR1 objARR = null;
                //CustomsImportBAL.AGT objAGT = null;
                //CustomsImportBAL.SHP objSHP = null;
                //CustomsImportBAL.CNE objCNE = null;
                //CustomsImportBAL.TRN objTRN = null;
                //CustomsImportBAL.CSD objCSD = null;
                //CustomsImportBAL.RFA objRFA = null;
                CustomsImportBAL.DEP objDEP = objFDM.DEP;
                //CustomsImportBAL.ERR objERR = null;
                //CustomsImportBAL.ERF objERF = null;





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

                //if (objCCL != null)  //Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.
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




                //if (objWBL != null)
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

                //    }

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
                //if (objERF != null)  //Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.
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
                //}

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
                clsLog.WriteLogAzure(ex.Message);
            }

        }
        #endregion

        #region Update Method for FRC object
        public void readQueryValuesFRC(CustomsImportBAL.FRC objFRC, ref object[] QueryValues, string EncodedMessage, string FlightNo, string FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {
                CustomsImportBAL.SMI objSMI = objFRC.StandardMessageIdentifier;
                CustomsImportBAL.AWB1 objAWB1 = objFRC.AWBDetails[objFRC.AWBDetails.Length - 1];
                CustomsImportBAL.CCL objCCL = objFRC.CCLDetails;
                CustomsImportBAL.WBL objWBL = objFRC.WBLDetails[objFRC.WBLDetails.Length - 1];
                CustomsImportBAL.ARR1 objARR = objFRC.ArrivalDetails[objFRC.ArrivalDetails.Length - 1];
                CustomsImportBAL.AGT objAGT = null;
                CustomsImportBAL.SHP objSHP = objFRC.ShipperDetails;
                CustomsImportBAL.CNE objCNE = objFRC.ConsigneeDetails;
                CustomsImportBAL.TRN objTRN = objFRC.TransferDetails.Length > 0 ? objFRC.TransferDetails[0] : null;
                CustomsImportBAL.CSD objCSD = objFRC.CSDDetails;
                CustomsImportBAL.RFA objRFA = objFRC.RFA; ;
                CustomsImportBAL.DEP objDEP = null;
                CustomsImportBAL.FDA objFDA = objFRC.FDA;





                //--------------------AWB----------------------------------
                if (objAWB1 != null)
                {
                    QueryValues[valNo++] = objAWB1.AWBPrefix.Trim();
                    QueryValues[valNo++] = objAWB1.AWBNumber.Trim();
                    QueryValues[valNo++] = objFRC.StandardMessageIdentifier.StandardMessageIdentifier;
                    QueryValues[valNo++] = objAWB1.HAWBNumber.Trim();
                    QueryValues[valNo++] = objAWB1.ConsolidationIdentifier.Trim();
                    QueryValues[valNo++] = objAWB1.PackageTrackingIdentifier;

                    QueryValues[valNo++] = "";//   only for fsq ans fsc txtPartArrival.Text.Trim();
                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = objFRC.StandardMessageIdentifier.StandardMessageIdentifier;
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
                    QueryValues[valNo++] = "0";
                    QueryValues[valNo++] = "";
                }

                //------------------ARR------------------------------
                if (objARR != null)
                {
                    QueryValues[valNo++] = objARR.ScheduledArrivalDate;
                    QueryValues[valNo++] = objARR.PartArrivalReference;
                    QueryValues[valNo++] = objARR.BoardedQuantityIdentifier;
                    QueryValues[valNo++] = objARR.BoardedPieceCount;
                    QueryValues[valNo++] = objARR.Weight;
                    QueryValues[valNo++] = objARR.WeightCode;
                    QueryValues[valNo++] = objARR.ImportingCarrier;
                    QueryValues[valNo++] = objARR.FlightNumber;
                    QueryValues[valNo++] = objARR.PartArrivalReference;

                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "0";
                    QueryValues[valNo++] = "0";
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

                QueryValues[valNo++] = objFDA.FDADetails;

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

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";

                //----------------------------CSN---------------------------

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "0";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";



                //------------------ERR-----------------------------------

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";




                //-------------------FSQ----------------------------------

                QueryValues[valNo++] = "";


                //-------------------FSC-----------------------------------

                QueryValues[valNo++] = "";


                //------------------TXT------------------------------------

                QueryValues[valNo++] = "";

                //------------------ERF------------------------------------

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";

                //------------------COMMON---------------------------------------
                QueryValues[valNo++] = EncodedMessage;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = FlightNo;
                QueryValues[valNo++] = FlightDate;
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
                clsLog.WriteLogAzure(ex.Message);
            }
            //return null;
        }
        #endregion

        #region Update Method for FRX object
        public void readQueryValuesFRX(CustomsImportBAL.FRX objFRX, ref object[] QueryValues, string EncodedMessage, string FlightNo, string FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {
                CustomsImportBAL.SMI objSMI = objFRX.StandardMessageIdentifier;
                CustomsImportBAL.AWB1 objAWB1 = objFRX.AWBDetails[objFRX.AWBDetails.Length - 1];
                CustomsImportBAL.CCL objCCL = objFRX.CCLDetails;
                CustomsImportBAL.WBL objWBL = null;
                CustomsImportBAL.ARR2 objARR = objFRX.ArrivalDetails[objFRX.ArrivalDetails.Length - 1];
                CustomsImportBAL.AGT objAGT = null;
                CustomsImportBAL.SHP objSHP = null;
                CustomsImportBAL.CNE objCNE = null;
                CustomsImportBAL.TRN objTRN = null;
                CustomsImportBAL.CSD objCSD = null;
                CustomsImportBAL.RFA objRFA = objFRX.RFA; ;
                CustomsImportBAL.DEP objDEP = null;
                CustomsImportBAL.ERR objERR = null;
                CustomsImportBAL.ERF objERF = null;






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


                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";




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
                QueryValues[valNo++] = FlightDate;
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
                clsLog.WriteLogAzure(ex.Message);
            }

        }
        #endregion

        #region Update Method for FSN object
        public void readQueryValuesFSN(CustomsImportBAL.FSN objFSN, ref object[] QueryValues, string EncodedMessage, string FlightNo, string FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {
                CustomsImportBAL.SMI objSMI = objFSN.StandardMessageIdentifier;
                CustomsImportBAL.AWB1 objAWB1 = objFSN.AWBDetails[0];
                CustomsImportBAL.CCL objCCL = objFSN.CCLDetails;
                CustomsImportBAL.WBL objWBL = null;
                CustomsImportBAL.ARR2 objARR = objFSN.ArrivalDetails[0];
                CustomsImportBAL.ASN objASN = objFSN.ASN; ;
                CustomsImportBAL.AGT objAGT = null;
                CustomsImportBAL.SHP objSHP = null;
                CustomsImportBAL.CNE objCNE = null;
                CustomsImportBAL.TRN objTRN = null;
                CustomsImportBAL.CSD objCSD = null;
                CustomsImportBAL.RFA objRFA = null;
                CustomsImportBAL.DEP objDEP = null;
                CustomsImportBAL.ERR objERR = null;
                CustomsImportBAL.ERF objERF = null;

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
                QueryValues[valNo++] = FlightDate;
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
                clsLog.WriteLogAzure(ex.Message);
            }

        }
        #endregion

        #region Update Method for FER Message
        public void readQueryValuesFER(CustomsImportBAL.FER objFER, ref object[] QueryValues, string EncodedMessage, string FlightNo, string FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {

                CustomsImportBAL.SMI objSMI = objFER.StandardMessageIdentifier;
                CustomsImportBAL.AWB1 objAWB1 = objFER.AWBDetails[0];
                if (objFER.AWBDetails.Length > 1)
                    objAWB1 = objFER.AWBDetails[objFER.AWBDetails.Length - 1];
                CustomsImportBAL.CCL objCCL = null;
                CustomsImportBAL.WBL objWBL = null;
                CustomsImportBAL.ARR1 objARR = null;
                CustomsImportBAL.AGT objAGT = null;
                CustomsImportBAL.SHP objSHP = null;
                CustomsImportBAL.CNE objCNE = null;
                CustomsImportBAL.TRN objTRN = null;
                CustomsImportBAL.CSD objCSD = null;
                CustomsImportBAL.RFA objRFA = null;
                CustomsImportBAL.DEP objDEP = null;
                CustomsImportBAL.ERR objERR = objFER.ERR[0];
                if (objFER.ERR.Length > 1)
                    objERR = objFER.ERR[objFER.ERR.Length - 1];
                CustomsImportBAL.ERF objERF = objFER.ERF;







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


                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";



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
                    QueryValues[valNo++] = "0";
                    QueryValues[valNo++] = "";
                }

                //------------------ARR------------------------------
                if (objARR != null)
                {
                    QueryValues[valNo++] = objARR.ScheduledArrivalDate;
                    QueryValues[valNo++] = objARR.PartArrivalReference;
                    QueryValues[valNo++] = objARR.BoardedQuantityIdentifier;
                    QueryValues[valNo++] = objARR.BoardedPieceCount;
                    QueryValues[valNo++] = objARR.Weight;
                    QueryValues[valNo++] = objARR.WeightCode;
                    QueryValues[valNo++] = objARR.ImportingCarrier;
                    QueryValues[valNo++] = objARR.FlightNumber;
                    QueryValues[valNo++] = objARR.PartArrivalReference;

                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "0";
                    QueryValues[valNo++] = "0";
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


                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";


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
                QueryValues[valNo++] = FlightDate;
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
                clsLog.WriteLogAzure(ex.Message);
            }
            //return null;
        }
        #endregion

        #region Update Method for FSQ Message
        public void readQueryValuesFSQ(CustomsImportBAL.FSQ objFSQ, ref object[] QueryValues, string EncodedMessage, string FlightNo, string FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {

                CustomsImportBAL.SMI objSMI = objFSQ.StandardMessageIdentifier;
                CustomsImportBAL.AWB2 objAWB1 = objFSQ.AWBDetails[0];
                if (objFSQ.AWBDetails.Length > 1)
                    objAWB1 = objFSQ.AWBDetails[objFSQ.AWBDetails.Length - 1];
                CustomsImportBAL.CCL objCCL = objFSQ.CCLDetails;
                CustomsImportBAL.WBL objWBL = null;
                CustomsImportBAL.ARR1 objARR = null;
                CustomsImportBAL.AGT objAGT = null;
                CustomsImportBAL.SHP objSHP = null;
                CustomsImportBAL.CNE objCNE = null;
                CustomsImportBAL.TRN objTRN = null;
                CustomsImportBAL.CSD objCSD = null;
                CustomsImportBAL.RFA objRFA = null;
                CustomsImportBAL.DEP objDEP = null;
                CustomsImportBAL.ERR objERR = null;
                CustomsImportBAL.ERF objERF = null;
                CustomsImportBAL.FSQSub objFSQSub = objFSQ.FSQSub;






                //--------------------AWB----------------------------------
                if (objAWB1 != null)
                {
                    QueryValues[valNo++] = objAWB1.AWBPrefix.Trim();
                    QueryValues[valNo++] = objAWB1.AWBNumber.Trim();
                    QueryValues[valNo++] = objSMI.StandardMessageIdentifier;
                    QueryValues[valNo++] = objAWB1.HAWBNumber.Trim();
                    QueryValues[valNo++] = string.Empty;
                    QueryValues[valNo++] = string.Empty;

                    QueryValues[valNo++] = objAWB1.ArrivalReference;//   only for fsq ans fsc txtPartArrival.Text.Trim();
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
                    QueryValues[valNo++] = "0";
                    QueryValues[valNo++] = "";
                }

                //------------------ARR------------------------------
                if (objARR != null)
                {
                    QueryValues[valNo++] = objARR.ScheduledArrivalDate;
                    QueryValues[valNo++] = objARR.PartArrivalReference;
                    QueryValues[valNo++] = objARR.BoardedQuantityIdentifier;
                    QueryValues[valNo++] = objARR.BoardedPieceCount;
                    QueryValues[valNo++] = objARR.Weight;
                    QueryValues[valNo++] = objARR.WeightCode;
                    QueryValues[valNo++] = objARR.ImportingCarrier;
                    QueryValues[valNo++] = objARR.FlightNumber;
                    QueryValues[valNo++] = objARR.PartArrivalReference;

                }
                else
                {
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "0";
                    QueryValues[valNo++] = "0";
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


                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";


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
                if (objFSQ.FSQSub != null)
                    QueryValues[valNo++] = objFSQ.FSQSub.StatusRequestCode;
                else
                    QueryValues[valNo++] = string.Empty;


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
                QueryValues[valNo++] = FlightDate;
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
                clsLog.WriteLogAzure(ex.Message);
            }
            //return null;
        }
        #endregion

        #region Update Method for PRI object Based
        public void readQueryValuesPRI(CustomsImportBAL.PRI objPRI, ref object[] QueryValues, string EncodedMessage, string FlightNo, string FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {

                CustomsImportBAL.SMI objSMI = objPRI.StandardMessageIdentifier;
                CustomsImportBAL.AWB1 objAWB1 = objPRI.AWBDetails[objPRI.AWBDetails.Length - 1];
                CustomsImportBAL.CCL objCCL = objPRI.CCLDetails;
                CustomsImportBAL.WBL objWBL = objPRI.WBLDetails[objPRI.WBLDetails.Length - 1];
                CustomsImportBAL.ARR1 objARR = objPRI.ArrivalDetails[objPRI.ArrivalDetails.Length - 1];
                CustomsImportBAL.AGT objAGT = objPRI.AgentDetails;
                CustomsImportBAL.SHP objSHP = objPRI.ShipperDetails;
                CustomsImportBAL.CNE objCNE = objPRI.ConsigneeDetails;
                CustomsImportBAL.TRN objTRN = objPRI.TransferDetails.Length > 0 ? objPRI.TransferDetails[objPRI.TransferDetails.Length - 1] : null;
                CustomsImportBAL.CSD objCSD = objPRI.CSDDetails;
                CustomsImportBAL.FDA objFDA = objPRI.FDA;
                CustomsImportBAL.RFA objRFA = null;
                CustomsImportBAL.DEP objDEP = null;


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
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                }

                //------------------ARR------------------------------
                if (objARR != null)
                {
                    QueryValues[valNo++] = objARR.ScheduledArrivalDate;
                    QueryValues[valNo++] = objARR.PartArrivalReference;
                    QueryValues[valNo++] = objARR.BoardedQuantityIdentifier;
                    QueryValues[valNo++] = objARR.BoardedPieceCount;
                    QueryValues[valNo++] = objARR.Weight;
                    //New field added for arrival weight code
                    QueryValues[valNo++] = objARR.WeightCode;
                    //end
                    QueryValues[valNo++] = objARR.ImportingCarrier;
                    QueryValues[valNo++] = objARR.FlightNumber;
                    QueryValues[valNo++] = objARR.PartArrivalReference;

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
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";
                    QueryValues[valNo++] = "";


                }

                //-----------------------FDA--------------------------------

                QueryValues[valNo++] = objFDA.FDADetails;

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

                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";



                //-------------------FSQ----------------------------------

                QueryValues[valNo++] = "";



                //-------------------FSC-----------------------------------

                QueryValues[valNo++] = "";


                //------------------TXT------------------------------------

                QueryValues[valNo++] = "";


                //------------------ERF------------------------------------
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";
                QueryValues[valNo++] = "";

                //------------------COMMON---------------------------------------
                QueryValues[valNo++] = EncodedMessage;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = UpdatedOn;
                QueryValues[valNo++] = UpdatedBy;
                QueryValues[valNo++] = FlightNo;
                QueryValues[valNo++] = FlightDate;
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
                clsLog.WriteLogAzure(ex.Message);
            }

        }
        #endregion

        #region Update Method for PSN object
        public void readQueryValuesPSN(CustomsImportBAL.PSN objPSN, ref object[] QueryValues, string EncodedMessage, string FlightNo, DateTime FlightDate, string UpdatedBy, DateTime UpdatedOn)
        {
            int valNo = 0;
            try
            {
                CustomsImportBAL.SMI objSMI = objPSN.StandardMessageIdentifier;
                CustomsImportBAL.AWB1 objAWB1 = objPSN.AWBDetails[0];
                CustomsImportBAL.CCL objCCL = objPSN.CCLDetails;
                CustomsImportBAL.WBL objWBL = null;
                CustomsImportBAL.ARR2 objARR = null;
                CustomsImportBAL.ASN objASN = objPSN.ASN; ;
                CustomsImportBAL.AGT objAGT = null;
                CustomsImportBAL.SHP objSHP = null;
                CustomsImportBAL.CNE objCNE = null;
                CustomsImportBAL.TRN objTRN = null;
                CustomsImportBAL.CSD objCSD = null;
                CustomsImportBAL.RFA objRFA = null;
                CustomsImportBAL.DEP objDEP = null;
                CustomsImportBAL.ERR objERR = null;
                CustomsImportBAL.ERF objERF = null;

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
                QueryValues[valNo++] = FlightDate;
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
                clsLog.WriteLogAzure(ex.Message);
            }
            //return null;
        }
        #endregion

        #region AMS Message Header

        public string AMSMessageHeader(string Message, DateTime CurrentTime, string MessageNature)
        {
            try
            {
                if (Message != string.Empty)
                {
                    SQLServer db = new SQLServer();

                    DataSet ds = new DataSet("dsAMSMessageHeader");
                    ds = db.SelectRecords("spGetAMSHeaderData");

                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        if (MessageNature == "O")
                        {
                            StringBuilder sb = new StringBuilder();
                            string OriginatorCode = ds.Tables[0].Rows[0]["OriginatorCode"].ToString();
                            string SOH = ds.Tables[0].Rows[0]["SOH"].ToString();
                            string STX = ds.Tables[0].Rows[0]["STX"].ToString();
                            string ETX = ds.Tables[0].Rows[0]["ETX"].ToString();
                            //sb.AppendLine();
                            sb = sb.AppendLine(SOH + "QK " + OriginatorCode);
                            sb = sb.AppendLine(".WASUCCR " + CurrentTime.ToString("HHMMssmm"));
                            sb = sb.AppendLine(STX);
                            sb = sb.AppendLine(Message + ETX);
                            return sb.ToString();
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();
                            string OriginatorCode = ds.Tables[0].Rows[0]["OriginatorCode"].ToString();
                            string SOH = ds.Tables[0].Rows[0]["SOH"].ToString();
                            string STX = ds.Tables[0].Rows[0]["STX"].ToString();
                            string ETX = ds.Tables[0].Rows[0]["ETX"].ToString();
                            string SenderIdentification = ds.Tables[0].Rows[0]["SenderIdentification"].ToString();
                            sb = sb.AppendLine(SOH + ".WASUCCR");
                            sb = sb.AppendLine("." + OriginatorCode + SenderIdentification);
                            sb = sb.AppendLine(STX);
                            sb = sb.AppendLine(Message + ETX);
                            return sb.ToString();
                        }

                    }
                    if (ds != null)
                    { ds.Dispose(); }
                    if (db != null)
                        db = null;

                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return string.Empty;
            }
        }
        #endregion

        #region Getting FRX AWB's
        public DataSet GetFRXAWB(string FlightNo, DateTime FlightDate, string FlightOrigin)
        {
            try
            {
                SQLServer sb = new SQLServer();
                object[] QueryValues = { FlightNo, FlightDate, FlightOrigin };
                string[] QueryNames = { "FlightNo", "FlightDate", "FlightOrigin" };
                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar };

                DataSet ds = new DataSet("ds_CustomsImportBAL_GetFRXAWB");
                ds = sb.SelectRecords("sp_GetFRXAWB", QueryNames, QueryValues, QueryTypes);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return ds;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return null;
            }
        }
        public DataSet GetFRXAWB(string FlightNo, DateTime FlightDate, string FlightOrigin, string Event)
        {
            try
            {
                SQLServer sb = new SQLServer();
                object[] QueryValues = { FlightNo, FlightDate, FlightOrigin };
                string[] QueryNames = { "FlightNo", "FlightDate", "FlightOrigin" };
                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar };
                string StoredProcedure = (Event == "Arrival" ? "sp_GetFRXAWB_Import" : "sp_GetFRXAWB");
                DataSet ds = new DataSet("ds_CustomsImportBAL_GetFRXAWB");
                ds = sb.SelectRecords(StoredProcedure, QueryNames, QueryValues, QueryTypes);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return ds;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return null;
            }
        }
        #endregion

        #region Getting FRC AWB's
        public DataSet GetFRCAWB(string FlightNo, DateTime FlightDate, string FlightOrigin)
        {
            try
            {
                SQLServer sb = new SQLServer();
                object[] QueryValues = { FlightNo, FlightDate, FlightOrigin };
                string[] QueryNames = { "FlightNo", "FlightDate", "FlightOrigin" };
                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar };

                DataSet ds = new DataSet("ds_CustomsImportBAL_GetFRCAWB");
                ds = sb.SelectRecords("sp_GetFRCAWB", QueryNames, QueryValues, QueryTypes);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return ds;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return null;
            }
        }
        #endregion

        #region Getting Master Data for Dropdowns
        public DataSet GetDropDownMasters()
        {
            try
            {
                SQLServer sb = new SQLServer();
                DataSet ds = new DataSet("ds_CustomsImportBAL_GetDropDownMasters");
                ds = sb.SelectRecords("sp_GetMasterDataCustoms");
                sb = null;
                if (ds != null && ds.Tables.Count > 0)
                {
                    return ds;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return null;
            }
        }
        #endregion

        #region Getting Customs Audit Records
        public DataSet GetCustomsAuditList(string AWBNumber)
        {
            try
            {
                SQLServer sb = new SQLServer();
                DataSet ds = new DataSet("ds_CustomsImportBAL_GetCustomsAuditList");
                ds = sb.SelectRecords("SP_GetCustomsAuditDetails", "AWBNumber", AWBNumber, SqlDbType.VarChar);
                sb = null;
                if (ds != null && ds.Tables.Count > 0)
                {
                    return ds;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return null;
            }
        }
        #endregion

        public DataSet SP_GetAllStations()
        {
            try
            {
                SQLServer da = new SQLServer();
                DataSet ds = da.SelectRecords("SP_GetAllStations");

                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        return (ds);
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return (null);
            }
            return (null);
        }

        public DataSet SP_GetFlightDesignators()
        {
            try
            {
                SQLServer da = new SQLServer();
                DataSet ds = da.SelectRecords("SP_GetFlightDesignators");

                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        return (ds);
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return (null);
            }
            return (null);
        }

        public DataSet Sp_GetEntryCodes()
        {
            try
            {
                SQLServer da = new SQLServer();
                DataSet ds = da.SelectRecords("Sp_GetEntryCodes");

                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        return (ds);
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return (null);
            }
            return (null);
        }

        public DataSet Sp_GetAmendmentCodes()
        {
            try
            {
                SQLServer da = new SQLServer();
                DataSet ds = da.SelectRecords("Sp_GetAmendmentCodes");

                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        return (ds);
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return (null);
            }
            return (null);
        }

        public DataSet Sp_GetCustomsCountryCode()
        {
            try
            {
                SQLServer da = new SQLServer();
                DataSet ds = da.SelectRecords("Sp_GetCustomsCountryCode");

                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        return (ds);
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return (null);
            }
            return (null);
        }

        public DataSet Sp_GetActionCodes()
        {
            try
            {
                SQLServer da = new SQLServer();
                DataSet ds = da.SelectRecords("Sp_GetActionCodes");

                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        return (ds);
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return (null);
            }
            return (null);
        }

        public DataSet Sp_GetErrorCodes()
        {
            try
            {
                SQLServer da = new SQLServer();
                DataSet ds = da.SelectRecords("Sp_GetErrorCodes");

                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        return (ds);
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return (null);
            }
            return (null);
        }

        public DataSet Sp_GetStatusCodes()
        {
            try
            {
                SQLServer da = new SQLServer();
                DataSet ds = da.SelectRecords("Sp_GetStatusCodes");

                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        return (ds);
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return (null);
            }
            return (null);
        }

        public DataSet SP_GETCustomsCurrencyCode()
        {
            try
            {
                SQLServer da = new SQLServer();
                DataSet ds = da.SelectRecords("SP_GETCustomsCurrencyCode");

                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        return (ds);
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex.Message);
                return (null);
            }
            return (null);
        }

    }

}
