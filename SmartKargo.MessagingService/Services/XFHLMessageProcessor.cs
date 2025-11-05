#region XFHL Message Processor Class Description
/* XFHLMessage Processor Class Description.
      * Company              :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright            :   Copyright © 2017 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose              : 
      * Created By           :   Yoginath
      * Created On           :   02-08-2017
      * Approved By          :
      * Approved Date        :
      * Modified By          :  
      * Modified On          :   
      * Description          :   
     */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;
using System.IO;
using System.Reflection;
using System.Data;
using QID.DataAccess;
using System.Configuration;
using QidWorkerRole;
using System.Data.SqlClient;


namespace QidWorkerRole
{
    public class XFHLMessageProcessor
    {
        //SCMExceptionHandlingWorkRole scmException = new SCMExceptionHandlingWorkRole();

        #region Decode XFHL message
        public bool DecodeReceiveFHLMessage(string fhlmsg, ref MessageData.fhlinfo fhldata, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.customsextrainfo[] custominfo)
        {
            string awbref = string.Empty;
            bool flag = false;
            var fhlXmlDataSet = new DataSet();
            var tx = new StringReader(fhlmsg);
            fhlXmlDataSet.ReadXml(tx);

            try
            {
                int version = 4;

                // version
                fhldata.fhlversionnum = Convert.ToString(fhlXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"]);
                version = Convert.ToInt16(Convert.ToString(fhlXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"]));

                //awb consigment details

                string[] awbnumberprefix = Convert.ToString(fhlXmlDataSet.Tables["MasterConsignment_TransportContractDocument"].Rows[0]["ID"]).Split('-');
                fhldata.airlineprefix = Convert.ToString(awbnumberprefix[0]);
                fhldata.awbnum = Convert.ToString(awbnumberprefix[1]);
                fhldata.origin = Convert.ToString(fhlXmlDataSet.Tables["MasterConsignment_OriginLocation"].Rows[0]["ID"]);
                fhldata.dest = Convert.ToString(fhlXmlDataSet.Tables["MasterConsignment_FinalDestinationLocation"].Rows[0]["ID"]);
                fhldata.consigntype = Convert.ToString("T");
                fhldata.pcscnt = Convert.ToString(fhlXmlDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"]);

                if (fhlXmlDataSet.Tables.Contains("IncludedTareGrossWeightMeasure"))
                {
                    if (fhlXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Columns.Contains("UnitCode"))
                        fhldata.weightcode = Convert.ToString(fhlXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Rows[0]["UnitCode"]);

                    if (fhlXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Columns.Contains("IncludedTareGrossWeightMeasure_Text"))
                        fhldata.weight = Convert.ToString(fhlXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Rows[0]["IncludedTareGrossWeightMeasure_Text"]);
                }

                //onwards check consignment details
                DecodeFHLConsigmentDetails(fhlXmlDataSet, ref consinfo, ref custominfo);

                ////Shipper Info                
                //fhldata.shippername = msg[1];
                //fhldata.shippername = msg[1].Length > 0 ? msg[1] : "";
                //fhldata.shippername2 = msg[1].ToString();
                //fhldata.shipperadd = msg[1].Length > 0 ? msg[1] : "";
                //fhldata.shipperadd2 = msg[1].ToString();
                //fhldata.shipperplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
                //fhldata.shipperstate = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
                //fhldata.shippercountrycode = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
                //fhldata.shipperpostcode = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
                //fhldata.shippercontactidentifier = msg[3].Length > 0 || msg[3] == null ? msg[3] : "";
                //fhldata.shippercontactnum = msg[4].Length > 0 || msg[4] == null ? msg[4] : "";

                //// Consignee                
                //fhldata.consname = msg[1];
                //fhldata.consname = msg[1].Length > 0 ? msg[1] : "";
                //fhldata.consname2 = msg[1].ToString();
                //fhldata.consadd = msg[1].Length > 0 ? msg[1] : "";
                //fhldata.consadd2 = msg[1].ToString();
                //fhldata.consplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
                //fhldata.consstate = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
                //fhldata.conscountrycode = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
                //fhldata.conspostcode = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
                //fhldata.conscontactidentifier = msg[3].Length > 0 || msg[3] == null ? msg[3] : "";
                //fhldata.conscontactnum = msg[4].Length > 0 || msg[4] == null ? msg[4] : "";

                ////Charge declaration
                //fhldata.currency = msg[1];
                //fhldata.chargecode = msg[2].Length > 0 ? msg[2] : "";
                //fhldata.chargedec = msg[3].Length > 0 ? msg[3] : "";
                //fhldata.declaredvalue = msg[4];
                //fhldata.declaredcustomvalue = msg[5];
                //fhldata.insuranceamount = msg[6];

                flag = true;

            }
            catch (Exception)
            {
                //SCMExceptionHandling.logexception(ref ex);
                flag = false;
            }
            return flag;
        }



        #region Decode Consigment Details
        private void DecodeFHLConsigmentDetails(DataSet fhlXmlDataSet, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.customsextrainfo[] custominfo)
        {
            DataRow[] drs;
            for (int row = 0; row < fhlXmlDataSet.Tables["IncludedHouseConsignment"].Rows.Count; row++)
            {
                MessageData.consignmnetinfo consig = new MessageData.consignmnetinfo("");
                drs = fhlXmlDataSet.Tables["TransportContractDocument"].Select("IncludedHouseConsignment_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    consig.awbnum = Convert.ToString(drs[0]["ID"]);
                }

                drs = fhlXmlDataSet.Tables["OriginLocation"].Select("IncludedHouseConsignment_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    consig.origin = Convert.ToString(drs[0]["ID"]);
                    //Convert.ToString(drs[0]["Name"]);
                }

                drs = fhlXmlDataSet.Tables["FinalDestinationLocation"].Select("IncludedHouseConsignment_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    consig.dest = Convert.ToString(drs[0]["ID"]);
                    //Convert.ToString(drs[0]["Name"]);
                }

                consig.consigntype = "";
                consig.pcscnt = Convert.ToString(fhlXmlDataSet.Tables["IncludedHouseConsignment"].Rows[row]["TotalPieceQuantity"]);
                drs = fhlXmlDataSet.Tables["GrossWeightMeasure"].Select("IncludedHouseConsignment_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    if (Convert.ToString(drs[0]["unitCode"]).Equals("KGM"))
                    {
                        consig.weightcode = "K";
                    }
                    else
                    {
                        consig.weightcode = "L";
                    }
                    consig.weight = Convert.ToString(drs[0]["GrossWeightMeasure_Text"]);
                }

                consig.manifestdesc = Convert.ToString(fhlXmlDataSet.Tables["IncludedHouseConsignment"].Rows[row]["SummaryDescription"]);
                consig.slac = Convert.ToString(fhlXmlDataSet.Tables["IncludedHouseConsignment"].Rows[row]["PackageQuantity"]);

                //Free Text
                consig.freetextGoodDesc = Convert.ToString(fhlXmlDataSet.Tables["IncludedHouseConsignment"].Rows[row]["SummaryDescription"]);

                // Harmonised Tariff Schedule                
                //consinfo[consinfo.Length - 1].commodity = consinfo[consinfo.Length - 1].commodity + msg[1] + ",";

                //Splhandling
                drs = fhlXmlDataSet.Tables["HandlingSPHInstructions"].Select("IncludedHouseConsignment_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    //Convert.ToString(drs[0]["Description"]);
                    consig.splhandling = Convert.ToString(drs[0]["DescriptionCode"]);
                }


                Array.Resize(ref consinfo, consinfo.Length + 1);
                consinfo[consinfo.Length - 1] = consig;

                //custom extra info                
                MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
                drs = fhlXmlDataSet.Tables["IncludedCustomsNote"].Select("IncludedHouseConsignment_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    custom.CsrIdentifierOci = Convert.ToString(drs[0]["ContentCode"]);
                    custom.SupplementaryCsrIdentifierOci = Convert.ToString(drs[0]["Content"]);
                    custom.InformationIdentifierOci = Convert.ToString(drs[0]["SubjectCode"]);
                    custom.IsoCountryCodeOci = Convert.ToString(drs[0]["CountryID"]);
                }
                custom.consigref = "";
                Array.Resize(ref custominfo, custominfo.Length + 1);
                custominfo[custominfo.Length - 1] = custom;


            }
        }
        #endregion
        #endregion


        #region validateAndInsertFHLData
        public bool validateAndInsertFHLData(ref MessageData.fhlinfo fhl, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.customsextrainfo[] customextrainfo, int REFNo, string strMessage, string strMessageFrom, string strFromID, string strStatus)
        {
            bool flag = false;
            GenericFunction gf = new GenericFunction();
            AWBOperations objOpsAuditLog = null;
            try
            {
                bool isAWBPresent = false;
                string AWBNum = fhl.awbnum;
                string AWBPrefix = fhl.airlineprefix;
                SQLServer db = new SQLServer();

                gf.UpdateInboxFromMessageParameter(REFNo, AWBPrefix + "-" + AWBNum, string.Empty, string.Empty, string.Empty, "FHL", strMessageFrom, DateTime.Parse("1900-01-01"));

                #region Check AWB Present or Not
                DataSet ds = new DataSet();
                string[] pname = new string[] { "AWBNumber" };
                object[] values = new object[] { AWBNum };
                SqlDbType[] ptype = new SqlDbType[] { SqlDbType.VarChar };
                ds = db.SelectRecords("sp_getawbdetails", pname, values, ptype);

                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            isAWBPresent = true;
                        }
                    }
                }
                #endregion

                #region Add AWB details
                if (!isAWBPresent)
                {
                    SqlParameter[] sqlParameter = new SqlParameter[] { 
                        new SqlParameter("@RefNo",REFNo)
                        ,new SqlParameter("@Status","Processed")
                        ,new SqlParameter("@Error","AWB number not present")
                        ,new SqlParameter("@UpdatedOn",DateTime.Now)
                    };
                    db.SelectRecords("uspUpdateMsgFromInbox", sqlParameter);
                    return false;
                    ///Below code is commented by prashantz on 7-Mar-2017 to resolve JIRA# CEBV4-944
                    //string[] paramname = new string[] { "AirlinePrefix", "AWBNum", "Origin", "Dest", "PcsCount", "Weight", "Volume", "ComodityCode", "ComodityDesc", "CarrierCode", "FlightNum", "FlightDate", "FlightOrigin", "FlightDest", "ShipperName", "ShipperAddr", "ShipperPlace", "ShipperState", "ShipperCountryCode", "ShipperContactNo", "ConsName", "ConsAddr", "ConsPlace", "ConsState", "ConsCountryCode", "ConsContactNo", "CustAccNo", "IATACargoAgentCode", "CustName", "SystemDate", "MeasureUnit", "Length", "Breadth", "Height", "PartnerStatus", "REFNo" };

                    //object[] paramvalue = new object[] { fhl.airlineprefix, fhl.awbnum, fhl.origin, fhl.dest, fhl.pcscnt, fhl.weight, "", "GEN", "GEN", "", "NA", System.DateTime.Now.ToString("dd/MM/yyyy"), fhl.origin, fhl.dest, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", System.DateTime.Now.ToString("yyyy-MM-dd"), "", "", "", "", "", REFNo };

                    //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                    //                                          SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                    //                                          SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.Int };

                    //string procedure = "spInsertBookingDataFromFFR";
                    //flag = db.InsertData(procedure, paramname, paramtype, paramvalue);
                }
                #endregion

                #region Add HAWB Details
                for (int i = 0; i < consinfo.Length; i++)
                {

                    consinfo[i].weight = consinfo[i].weight.Length > 0 ? consinfo[i].weight : "0";
                    consinfo[i].pcscnt = consinfo[i].pcscnt.Length > 0 ? consinfo[i].pcscnt : "0";
                    string HAWBNo = consinfo[i].awbnum;
                    int HAWBPcs = Convert.ToInt16(consinfo[i].pcscnt.ToString());
                    float HAWBWt = float.Parse(consinfo[i].weight.ToString());
                    string Origin = consinfo[i].origin;
                    string Destination = consinfo[i].dest;
                    string description = consinfo[i].manifestdesc;
                    string commodity = consinfo[i].commodity;
                    string txtDesc = consinfo[i].freetextGoodDesc;
                    string SHC = consinfo[i].splhandling;
                    string CustID = "";
                    string CustName = fhl.shippername;
                    string CustAddress = fhl.shipperadd.Trim(',');
                    string City = fhl.shipperplace.Trim(',');
                    string Zipcode = fhl.shipperpostcode;
                    string HAWBPrefix = consinfo[i].airlineprefix;
                    string slac = consinfo[i].pcscnt.ToString();


                    slac = consinfo[i].slac.Length > 0 ? consinfo[i].slac : consinfo[i].pcscnt;


                    flag = PutHAWBDetails(AWBNum, HAWBNo, HAWBPcs, HAWBWt, description, CustID, CustName, CustAddress, City, Zipcode, Origin, Destination, SHC, HAWBPrefix, AWBPrefix, "", "", "", "", "",
                        fhl.consname, fhl.consadd.Trim(','), fhl.consplace.Trim(','), fhl.consstate, fhl.conscountrycode.Trim(','), fhl.conspostcode, fhl.shipperstate, fhl.shippercountrycode, "", slac, "", "",
                        fhl.shippercontactnum, "", fhl.conscontactnum);


                }
                #endregion

                objOpsAuditLog = new AWBOperations();
                objOpsAuditLog.AWBID = 0;
                objOpsAuditLog.AWBPrefix = AWBPrefix.Trim();
                objOpsAuditLog.AWBNumber = AWBNum;
                objOpsAuditLog.Origin = consinfo[0].origin.ToUpper();
                objOpsAuditLog.Destination = consinfo[0].dest.ToUpper();
                objOpsAuditLog.FlightNo = string.Empty;
                objOpsAuditLog.FlightDate = DateTime.UtcNow;
                objOpsAuditLog.FlightOrigin = string.Empty;
                objOpsAuditLog.FlightDestination = string.Empty;
                objOpsAuditLog.BookedPcs = Convert.ToInt32(fhl.pcscnt);
                objOpsAuditLog.BookedWgt = Convert.ToDouble(fhl.weight);
                objOpsAuditLog.UOM = fhl.weightcode;
                objOpsAuditLog.Createdon = DateTime.UtcNow;
                objOpsAuditLog.Updatedon = DateTime.UtcNow;
                objOpsAuditLog.Createdby = strMessageFrom;
                objOpsAuditLog.Updatedby = strMessageFrom;
                objOpsAuditLog.Action = "House";
                objOpsAuditLog.Message = "House Added";
                objOpsAuditLog.Description = consinfo[0].manifestdesc;

                #region Autoforward Email
                //try
                //{

                //    gf.AutoForwardEmail(REFNo, fhl.origin);
                //}
                //catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
                #endregion
            }
            catch (Exception)
            {
                //SCMExceptionHandling.logexception(ref ex);
                flag = false;
            }
            return flag;
        }
        #endregion


        #region HAWB Details Save
        public bool PutHAWBDetails(string MAWBNo, string HAWBNo, int HAWBPcs, float HAWBWt, string Description, string CustID, string CustName,
string CustAddress, string CustCity, string Zipcode, string Origin, string Destination, string SHC,
string HAWBPrefix, string AWBPrefix, string FltOrigin, string FltDest, string ArrivalStatus, string FlightNo,
string FlightDt, string ConsigneeName, string ConsigneeAddress, string ConsigneeCity, string ConsigneeState, string ConsigneeCountry, string ConsigneePostalCode,
string CustState, string CustCountry, string UOM, string SLAC, string ConsigneeID, string ShipperEmail, string ShipperTelephone, string ConsigneeEmail, string ConsigneeTelephone)
        {
            DataSet ds = new DataSet();
            SQLServer da = new SQLServer();

            string[] paramname = new string[35];
            paramname[0] = "MAWBNo";
            paramname[1] = "HAWBNo";
            paramname[2] = "HAWBPcs";
            paramname[3] = "HAWBWt";
            paramname[4] = "Description";
            paramname[5] = "CustID";
            paramname[6] = "CustName";
            paramname[7] = "CustAddress";
            paramname[8] = "CustCity";
            paramname[9] = "Zipcode";
            paramname[10] = "Origin";
            paramname[11] = "Destination";
            paramname[12] = "SHC";
            paramname[13] = "HAWBPrefix";
            paramname[14] = "AWBPrefix";
            paramname[15] = "ArrivalStatus";
            paramname[16] = "FlightNo";
            paramname[17] = "FlightDt";
            paramname[18] = "FlightOrigin";
            paramname[19] = "flightDest";
            paramname[20] = "ConsigneeName";
            paramname[21] = "ConsigneeAddress";
            paramname[22] = "ConsigneeCity";
            paramname[23] = "ConsigneeState";
            paramname[24] = "ConsigneeCountry";
            paramname[25] = "ConsigneePostalCode";
            paramname[26] = "CustState";
            paramname[27] = "CustCountry";
            paramname[28] = "UOM";
            paramname[29] = "SLAC";
            paramname[30] = "ConsigneeID";
            paramname[31] = "ShipperEmail";
            paramname[32] = "ShipperTelephone";
            paramname[33] = "ConsigneeEmail";
            paramname[34] = "ConsigneeTelephone";


            object[] paramvalue = new object[35];
            paramvalue[0] = MAWBNo;
            paramvalue[1] = HAWBNo;
            paramvalue[2] = HAWBPcs;
            paramvalue[3] = HAWBWt;
            paramvalue[4] = Description;
            paramvalue[5] = CustID;
            paramvalue[6] = CustName;
            paramvalue[7] = CustAddress;
            paramvalue[8] = CustCity;
            paramvalue[9] = Zipcode;
            paramvalue[10] = Origin;
            paramvalue[11] = Destination;
            paramvalue[12] = SHC;
            paramvalue[13] = HAWBPrefix;
            paramvalue[14] = AWBPrefix;
            paramvalue[15] = ArrivalStatus;
            paramvalue[16] = FlightNo;
            if (FlightDt == "")
            {
                // FlightDt = DateTime.Now.ToString();
                paramvalue[17] = DateTime.Now.ToString();
                //paramvalue[17] = FlightDt;
            }
            else
            {
                paramvalue[17] = FlightDt;
            }
            paramvalue[18] = FltOrigin;
            paramvalue[19] = FltDest;
            paramvalue[20] = ConsigneeName;
            paramvalue[21] = ConsigneeAddress;
            paramvalue[22] = ConsigneeCity;
            paramvalue[23] = ConsigneeState;
            paramvalue[24] = ConsigneeCountry;
            paramvalue[25] = ConsigneePostalCode;
            paramvalue[26] = CustState;
            paramvalue[27] = CustCountry;
            paramvalue[28] = UOM;
            paramvalue[29] = SLAC != string.Empty ? SLAC : "0";
            paramvalue[30] = ConsigneeID;
            paramvalue[31] = ShipperEmail;
            paramvalue[32] = ShipperTelephone;
            paramvalue[33] = ConsigneeEmail;
            paramvalue[34] = ConsigneeTelephone;


            SqlDbType[] paramtype = new SqlDbType[35];
            paramtype[0] = SqlDbType.VarChar;
            paramtype[1] = SqlDbType.VarChar;
            paramtype[2] = SqlDbType.Int;
            paramtype[3] = SqlDbType.Float;
            paramtype[4] = SqlDbType.VarChar;
            paramtype[5] = SqlDbType.VarChar;
            paramtype[6] = SqlDbType.VarChar;
            paramtype[7] = SqlDbType.VarChar;
            paramtype[8] = SqlDbType.VarChar;
            paramtype[9] = SqlDbType.VarChar;
            paramtype[10] = SqlDbType.VarChar;
            paramtype[11] = SqlDbType.VarChar;
            paramtype[12] = SqlDbType.VarChar;
            paramtype[13] = SqlDbType.VarChar;
            paramtype[14] = SqlDbType.VarChar;
            paramtype[15] = SqlDbType.VarChar;
            paramtype[16] = SqlDbType.VarChar;
            paramtype[17] = SqlDbType.DateTime;
            paramtype[18] = SqlDbType.VarChar;
            paramtype[19] = SqlDbType.VarChar;
            paramtype[20] = SqlDbType.VarChar;
            paramtype[21] = SqlDbType.VarChar;
            paramtype[22] = SqlDbType.VarChar;
            paramtype[23] = SqlDbType.VarChar;
            paramtype[24] = SqlDbType.VarChar;
            paramtype[25] = SqlDbType.VarChar;
            paramtype[26] = SqlDbType.VarChar;
            paramtype[27] = SqlDbType.VarChar;
            paramtype[28] = SqlDbType.VarChar;
            paramtype[29] = SqlDbType.Int;
            paramtype[30] = SqlDbType.VarChar;
            paramtype[31] = SqlDbType.VarChar;
            paramtype[32] = SqlDbType.VarChar;
            paramtype[33] = SqlDbType.VarChar;
            paramtype[34] = SqlDbType.VarChar;

            if (da.ExecuteProcedure("SP_PutHAWBDetails_V2", paramname, paramtype, paramvalue))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

    }
}
