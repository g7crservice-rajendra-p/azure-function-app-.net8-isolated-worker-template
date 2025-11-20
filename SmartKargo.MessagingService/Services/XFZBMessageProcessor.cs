#region XFWB Message Processor Class Description
/* XFWB Message Processor Class Description.
      * Company              :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright            :   Copyright © 2017 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose              :   XFZB Message Processor Class
      * Created By           :   Yoginath
      * Created On           :   2017-06-21
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
using System.Xml.Schema;
using static QidWorkerRole.SCMExceptionHandlingWorkRole;


namespace QidWorkerRole
{
    public class XFZBMessageProcessor
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<XFZBMessageProcessor> _logger;
        private readonly GenericFunction _genericFunction;

        //string strConnection = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        //SCMExceptionHandlingWorkRole scmException = new SCMExceptionHandlingWorkRole();

        #region Constructor
        public XFZBMessageProcessor(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<XFZBMessageProcessor> logger, GenericFunction genericFunction, SCMExceptionHandlingWorkRole scmExceptionHandlingWorkerRole)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;

        }
        #endregion


        #region Decode FHL message
        public bool DecodeReceiveFHLMessage(string fhlmsg, ref MessageData.fhlinfo fhldata, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.customsextrainfo[] custominfo)
        {
            string awbref = string.Empty;
            bool flag = false;
            try
            {
                //string lastrec = "NA", innerrec = "NA";
                //int line = 0;
                int version = 4;
                DataRow[] drs;

                var fzbXmlDataSet = new DataSet();

                var tx = new StringReader(fhlmsg);
                fzbXmlDataSet.ReadXml(tx);

                //UploadMasters.UploadMasterCommon obj = new UploadMasters.UploadMasterCommon();
                //obj.ExportDataSet(fzbXmlDataSet, "E:\\");

                fhldata.fhlversionnum = Convert.ToString(fzbXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"]);
                version = Convert.ToInt16(fhldata.fhlversionnum);

                //awb consigment details
                string[] decmes = Convert.ToString(fzbXmlDataSet.Tables["TransportContractDocument"].Rows[0]["ID"]).Split('-');
                fhldata.airlineprefix = decmes[0];
                fhldata.awbnum = decmes[1];
                fhldata.origin = Convert.ToString(fzbXmlDataSet.Tables["MasterConsignment_OriginLocation"].Rows[0]["ID"]);
                fhldata.dest = Convert.ToString(fzbXmlDataSet.Tables["MasterConsignment_FinalDestinationLocation"].Rows[0]["ID"]);
                //fhldata.consigntype = strarr[0];
                fhldata.pcscnt = Convert.ToString(fzbXmlDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"]);
                fhldata.weightcode = Convert.ToString(fzbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Rows[0]["unitCode"]);
                fhldata.weight = Convert.ToString(fzbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Rows[0]["IncludedTareGrossWeightMeasure_Text"]);

                //onwards check consignment details
                MessageData.consignmnetinfo consig = new MessageData.consignmnetinfo("");
                consig.awbnum = Convert.ToString(fzbXmlDataSet.Tables["BusinessHeaderDocument"].Rows[0]["ID"]);
                consig.origin = Convert.ToString(fzbXmlDataSet.Tables["OriginLocation"].Rows[0]["ID"]);
                consig.dest = Convert.ToString(fzbXmlDataSet.Tables["FinalDestinationLocation"].Rows[0]["ID"]);
                consig.consigntype = "";
                consig.pcscnt = Convert.ToString(fzbXmlDataSet.Tables["IncludedHouseConsignmentItem"].Rows[0]["PieceQuantity"]);
                drs = fzbXmlDataSet.Tables["GrossWeightMeasure"].Select("IncludedHouseConsignmentItem_Id=0");
                if (drs.Length > 0)
                {
                    consig.weightcode = Convert.ToString(drs[0]["unitCode"]);
                    consig.weight = Convert.ToString(drs[0]["GrossWeightMeasure_Text"]);
                }
                consig.manifestdesc = Convert.ToString(fzbXmlDataSet.Tables["NatureIdentificationTransportCargo"].Rows[0]["Identification"]);
                consig.slac = Convert.ToString(fzbXmlDataSet.Tables["IncludedHouseConsignmentItem"].Rows[0]["PackageQuantity"]);
                Array.Resize(ref consinfo, consinfo.Length + 1);
                consinfo[consinfo.Length - 1] = consig;
                //Free Text
                consinfo[consinfo.Length - 1].freetextGoodDesc = Convert.ToString(fzbXmlDataSet.Tables["NatureIdentificationTransportCargo"].Rows[0]["Identification"]);
                //Harmonised Tariff Schedule
                consinfo[consinfo.Length - 1].commodity = Convert.ToString(fzbXmlDataSet.Tables["TypeCode"].Rows[0]["TypeCode_Text"]);
                //custom extra info
                MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
                if (fzbXmlDataSet.Tables.Contains("IncludedCustomsNote"))
                {
                    custom.IsoCountryCodeOci = Convert.ToString(fzbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["CountryID"]);
                    custom.InformationIdentifierOci = Convert.ToString(fzbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["SubjectCode"]);
                    custom.CsrIdentifierOci = Convert.ToString(fzbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["ContentCode"]);
                    custom.SupplementaryCsrIdentifierOci = Convert.ToString(fzbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["Content"]);
                    custom.consigref = awbref;
                    Array.Resize(ref custominfo, custominfo.Length + 1);
                    custominfo[custominfo.Length - 1] = custom;
                }
                //Charge declaration
                if (fzbXmlDataSet.Tables.Contains("ActualAmount"))
                {
                    fhldata.currency = Convert.ToString(fzbXmlDataSet.Tables["ActualAmount"].Rows[0]["currencyID"]);
                }
                if (fzbXmlDataSet.Tables.Contains("ApplicableLogisticsAllowanceCharge"))
                {
                    fhldata.chargecode = Convert.ToString(fzbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["ID"]);
                    fhldata.chargedec = Convert.ToString(fzbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["Reason"]);
                }
                if (fzbXmlDataSet.Tables.Contains("DeclaredValueForCarriageAmount"))
                {
                    fhldata.declaredvalue = Convert.ToString(fzbXmlDataSet.Tables["DeclaredValueForCarriageAmount"].Rows[0]["DeclaredValueForCarriageAmount_Text"]);
                }
                if (fzbXmlDataSet.Tables.Contains("DeclaredValueForCustomsAmount"))
                {
                    fhldata.declaredcustomvalue = Convert.ToString(fzbXmlDataSet.Tables["DeclaredValueForCustomsAmount"].Rows[0]["DeclaredValueForCustomsAmount_Text"]);
                }
                if (fzbXmlDataSet.Tables.Contains("InsuranceValueAmount"))
                {
                    fhldata.insuranceamount = Convert.ToString(fzbXmlDataSet.Tables["InsuranceValueAmount"].Rows[0]["InsuranceValueAmount_Text"]);
                }


                //Shipper Info
                fhldata.shippername = Convert.ToString(fzbXmlDataSet.Tables["ConsignorParty"].Rows[0]["Name"]);
                //fhldata.shippername2 ="";
                fhldata.shipperadd = Convert.ToString(fzbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["StreetName"]);
                //fhldata.shipperadd2 ="";
                fhldata.shipperplace = Convert.ToString(fzbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountryName"]);
                fhldata.shipperstate = Convert.ToString(fzbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountrySubDivisionID"]);
                fhldata.shippercountrycode = Convert.ToString(fzbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountryID"]);
                fhldata.shipperpostcode = Convert.ToString(fzbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["PostcodeCode"]);
                //fhldata.shippercontactidentifier = "";
                fhldata.shippercontactnum = Convert.ToString(fzbXmlDataSet.Tables["DirectTelephoneCommunication"].Rows[0]["CompleteNumber"]);

                //Consignee
                fhldata.consname = Convert.ToString(fzbXmlDataSet.Tables["ConsigneeParty"].Rows[0]["Name"]);
                //fhldata.consname2 ="";
                fhldata.consadd = Convert.ToString(fzbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["StreetName"]);
                //fhldata.consadd2 ="";
                fhldata.consplace = Convert.ToString(fzbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountryName"]);
                fhldata.consstate = Convert.ToString(fzbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountrySubDivisionID"]);
                fhldata.conscountrycode = Convert.ToString(fzbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountryID"]);
                fhldata.conspostcode = Convert.ToString(fzbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["PostcodeCode"]);
                //fhldata.conscontactidentifier = "";
                fhldata.conscontactnum = Convert.ToString(fzbXmlDataSet.Tables["DirectTelephoneCommunication"].Rows[1]["CompleteNumber"]);
                if (fzbXmlDataSet.Tables.Contains("HandlingSSRInstructions"))
                {
                    consinfo[consinfo.Length - 1].splhandling = Convert.ToString(fzbXmlDataSet.Tables["HandlingSSRInstructions"].Rows[0]["Description"]);
                }

                flag = true;
            }
            catch (Exception)
            {
                flag = false;
            }
            return flag;
        }



        #region Decode Consigment Details
        //No reference found for this method
        //private void DecodeFHLConsigmentDetails(string inputstr, ref MessageData.consignmnetinfo[] consinfo, int version)
        //{
        //    MessageData.consignmnetinfo consig = new MessageData.consignmnetinfo("");
        //    try
        //    {
        //        string[] msg = inputstr.Split('/');
        //        consig.awbnum = msg[1];
        //        consig.origin = msg[2].Substring(0, 3);
        //        consig.dest = msg[2].Substring(3);
        //        consig.consigntype = "";
        //        consig.pcscnt = msg[3];//int.Parse(strarr[1]);
        //        consig.weightcode = msg[4].Substring(0, 1);
        //        consig.weight = msg[4].Substring(1);
        //        if (version == 2)
        //        {
        //            if (msg.Length > 4)
        //            {
        //                consig.manifestdesc = msg[5];
        //            }
        //        }
        //        else
        //        {
        //            if (msg.Length > 4)
        //            {
        //                consig.slac = msg[5];
        //            }
        //            if (msg.Length > 5)
        //            {
        //                consig.manifestdesc = msg[6];
        //            }
        //        }

        //    }
        //    catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //    Array.Resize(ref consinfo, consinfo.Length + 1);
        //    consinfo[consinfo.Length - 1] = consig;
        //}
        #endregion
        #endregion


        #region validateAndInsertFHLData
        //public bool validateAndInsertFHLData(ref MessageData.fhlinfo fhl, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.customsextrainfo[] customextrainfo, int REFNo, string strMessage, string strMessageFrom, string strFromID, string strStatus)
        public async Task<(bool success, MessageData.fhlinfo fhl, MessageData.consignmnetinfo[] consinfo, MessageData.customsextrainfo[] extra)>
        ValidateAndInsertFHLData(
        MessageData.fhlinfo fhl,
        MessageData.consignmnetinfo[] consinfo,
        MessageData.customsextrainfo[] customextrainfo,
        int REFNo, string strMessage, string strMessageFrom,
        string strFromID, string strStatus)
        {
            bool flag = false;
            //GenericFunction gf = new GenericFunction();
            AWBOperations objOpsAuditLog = null;
            try
            {
                bool isAWBPresent = false;
                string AWBNum = fhl.awbnum;
                string AWBPrefix = fhl.airlineprefix;
                //SQLServer db = new SQLServer();

                //gf.UpdateInboxFromMessageParameter(REFNo, AWBPrefix + "-" + AWBNum, string.Empty, string.Empty, string.Empty, "XFZB", strMessageFrom, DateTime.Parse("1900-01-01"));
                await _genericFunction.UpdateInboxFromMessageParameter(REFNo, AWBPrefix + "-" + AWBNum, string.Empty, string.Empty, string.Empty, "XFZB", strMessageFrom, DateTime.Parse("1900-01-01"));

                #region Check AWB Present or Not
                DataSet ds = new DataSet();
                //string[] pname = new string[] { "AWBNumber" };
                //object[] values = new object[] { AWBNum };
                //SqlDbType[] ptype = new SqlDbType[] { SqlDbType.VarChar };
                SqlParameter[] pname = new SqlParameter[] {
                    new SqlParameter("@AWBNumber",AWBNum)
                };
                //ds = db.SelectRecords("sp_getawbdetails", pname, values, ptype);
                ds = await _readWriteDao.SelectRecords("sp_getawbdetails", pname);

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
                    //db.SelectRecords("uspUpdateMsgFromInbox", sqlParameter);
                    await _readWriteDao.SelectRecords("uspUpdateMsgFromInbox", sqlParameter);
                    //return false;
                    return (false, fhl, consinfo, customextrainfo);
                    ///Below code is commented by prashantz on 7-Mar-2017 to resolve JIRA#  
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


                    flag = await PutHAWBDetails(AWBNum, HAWBNo, HAWBPcs, HAWBWt, description, CustID, CustName, CustAddress, City, Zipcode, Origin, Destination, SHC, HAWBPrefix, AWBPrefix, "", "", "", "", "",
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
            //return flag;
            return (flag, fhl, consinfo, customextrainfo);
        }
        #endregion

        //#region PutHAWBDetails
        //private bool PutHAWBDetails(string MAWBNo, string HAWBNo, int HAWBPcs, float HAWBWt, string Description, string CustID, string CustName, string CustAddress, string CustCity, string Zipcode, string Origin, string Destination, string SHC, string HAWBPrefix, string AWBPrefix)
        //{
        //    DataSet ds = new DataSet();
        //    SQLServer da = new SQLServer();

        //    string[] paramname = new string[15];
        //    paramname[0] = "MAWBNo";
        //    paramname[1] = "HAWBNo";
        //    paramname[2] = "HAWBPcs";
        //    paramname[3] = "HAWBWt";
        //    paramname[4] = "Description";
        //    paramname[5] = "CustID";
        //    paramname[6] = "CustName";
        //    paramname[7] = "CustAddress";
        //    paramname[8] = "CustCity";
        //    paramname[9] = "Zipcode";
        //    paramname[10] = "Origin";
        //    paramname[11] = "Destination";
        //    paramname[12] = "SHC";
        //    paramname[13] = "HAWBPrefix";
        //    paramname[14] = "AWBPrefix";
        //    object[] paramvalue = new object[15];
        //    paramvalue[0] = MAWBNo;
        //    paramvalue[1] = HAWBNo;
        //    paramvalue[2] = HAWBPcs;
        //    paramvalue[3] = HAWBWt;
        //    paramvalue[4] = Description;
        //    paramvalue[5] = CustID;
        //    paramvalue[6] = CustName;
        //    paramvalue[7] = CustAddress;
        //    paramvalue[8] = CustCity;
        //    paramvalue[9] = Zipcode;
        //    paramvalue[10] = Origin;
        //    paramvalue[11] = Destination;
        //    paramvalue[12] = SHC;
        //    paramvalue[13] = HAWBPrefix;
        //    paramvalue[14] = AWBPrefix;
        //    SqlDbType[] paramtype = new SqlDbType[15];
        //    paramtype[0] = SqlDbType.VarChar;
        //    paramtype[1] = SqlDbType.VarChar;
        //    paramtype[2] = SqlDbType.Int;
        //    paramtype[3] = SqlDbType.Float;
        //    paramtype[4] = SqlDbType.VarChar;
        //    paramtype[5] = SqlDbType.VarChar;
        //    paramtype[6] = SqlDbType.VarChar;
        //    paramtype[7] = SqlDbType.VarChar;
        //    paramtype[8] = SqlDbType.VarChar;
        //    paramtype[9] = SqlDbType.VarChar;
        //    paramtype[10] = SqlDbType.VarChar;
        //    paramtype[11] = SqlDbType.VarChar;
        //    paramtype[12] = SqlDbType.VarChar;
        //    paramtype[13] = SqlDbType.VarChar;
        //    paramtype[14] = SqlDbType.VarChar;
        //    if (da.ExecuteProcedure("SP_PutHAWBDetails", paramname, paramtype, paramvalue))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
        //#endregion


        #region HAWB Details Save
        //        public bool PutHAWBDetails(string MAWBNo, string HAWBNo, int HAWBPcs, float HAWBWt, string Description, string CustID, string CustName,
        //string CustAddress, string CustCity, string Zipcode, string Origin, string Destination, string SHC,
        //string HAWBPrefix, string AWBPrefix, string FltOrigin, string FltDest, string ArrivalStatus, string FlightNo,
        //string FlightDt, string ConsigneeName, string ConsigneeAddress, string ConsigneeCity, string ConsigneeState, string ConsigneeCountry, string ConsigneePostalCode,
        //string CustState, string CustCountry, string UOM, string SLAC, string ConsigneeID, string ShipperEmail, string ShipperTelephone, string ConsigneeEmail, string ConsigneeTelephone)
        public async Task<bool> PutHAWBDetails(string MAWBNo, string HAWBNo, int HAWBPcs, float HAWBWt, string Description, string CustID, string CustName,
        string CustAddress, string CustCity, string Zipcode, string Origin, string Destination, string SHC,
        string HAWBPrefix, string AWBPrefix, string FltOrigin, string FltDest, string ArrivalStatus, string FlightNo,
        string FlightDt, string ConsigneeName, string ConsigneeAddress, string ConsigneeCity, string ConsigneeState, string ConsigneeCountry, string ConsigneePostalCode,
        string CustState, string CustCountry, string UOM, string SLAC, string ConsigneeID, string ShipperEmail, string ShipperTelephone, string ConsigneeEmail, string ConsigneeTelephone)
        {
            DataSet ds = new DataSet();
            //SQLServer da = new SQLServer();

            //string[] paramname = new string[35];
            //paramname[0] = "MAWBNo";
            //paramname[1] = "HAWBNo";
            //paramname[2] = "HAWBPcs";
            //paramname[3] = "HAWBWt";
            //paramname[4] = "Description";
            //paramname[5] = "CustID";
            //paramname[6] = "CustName";
            //paramname[7] = "CustAddress";
            //paramname[8] = "CustCity";
            //paramname[9] = "Zipcode";
            //paramname[10] = "Origin";
            //paramname[11] = "Destination";
            //paramname[12] = "SHC";
            //paramname[13] = "HAWBPrefix";
            //paramname[14] = "AWBPrefix";
            //paramname[15] = "ArrivalStatus";
            //paramname[16] = "FlightNo";
            //paramname[17] = "FlightDt";
            //paramname[18] = "FlightOrigin";
            //paramname[19] = "flightDest";
            //paramname[20] = "ConsigneeName";
            //paramname[21] = "ConsigneeAddress";
            //paramname[22] = "ConsigneeCity";
            //paramname[23] = "ConsigneeState";
            //paramname[24] = "ConsigneeCountry";
            //paramname[25] = "ConsigneePostalCode";
            //paramname[26] = "CustState";
            //paramname[27] = "CustCountry";
            //paramname[28] = "UOM";
            //paramname[29] = "SLAC";
            //paramname[30] = "ConsigneeID";
            //paramname[31] = "ShipperEmail";
            //paramname[32] = "ShipperTelephone";
            //paramname[33] = "ConsigneeEmail";
            //paramname[34] = "ConsigneeTelephone";


            //object[] paramvalue = new object[35];
            //paramvalue[0] = MAWBNo;
            //paramvalue[1] = HAWBNo;
            //paramvalue[2] = HAWBPcs;
            //paramvalue[3] = HAWBWt;
            //paramvalue[4] = Description;
            //paramvalue[5] = CustID;
            //paramvalue[6] = CustName;
            //paramvalue[7] = CustAddress;
            //paramvalue[8] = CustCity;
            //paramvalue[9] = Zipcode;
            //paramvalue[10] = Origin;
            //paramvalue[11] = Destination;
            //paramvalue[12] = SHC;
            //paramvalue[13] = HAWBPrefix;
            //paramvalue[14] = AWBPrefix;
            //paramvalue[15] = ArrivalStatus;
            //paramvalue[16] = FlightNo;
            //if (FlightDt == "")
            //{
            //    // FlightDt = DateTime.Now.ToString();
            //    paramvalue[17] = DateTime.Now.ToString();
            //    //paramvalue[17] = FlightDt;
            //}
            //else
            //{
            //    paramvalue[17] = FlightDt;
            //}
            //paramvalue[18] = FltOrigin;
            //paramvalue[19] = FltDest;
            //paramvalue[20] = ConsigneeName;
            //paramvalue[21] = ConsigneeAddress;
            //paramvalue[22] = ConsigneeCity;
            //paramvalue[23] = ConsigneeState;
            //paramvalue[24] = ConsigneeCountry;
            //paramvalue[25] = ConsigneePostalCode;
            //paramvalue[26] = CustState;
            //paramvalue[27] = CustCountry;
            //paramvalue[28] = UOM;
            //paramvalue[29] = SLAC != string.Empty ? SLAC : "0";
            //paramvalue[30] = ConsigneeID;
            //paramvalue[31] = ShipperEmail;
            //paramvalue[32] = ShipperTelephone;
            //paramvalue[33] = ConsigneeEmail;
            //paramvalue[34] = ConsigneeTelephone;


            //SqlDbType[] paramtype = new SqlDbType[35];
            //paramtype[0] = SqlDbType.VarChar;
            //paramtype[1] = SqlDbType.VarChar;
            //paramtype[2] = SqlDbType.Int;
            //paramtype[3] = SqlDbType.Float;
            //paramtype[4] = SqlDbType.VarChar;
            //paramtype[5] = SqlDbType.VarChar;
            //paramtype[6] = SqlDbType.VarChar;
            //paramtype[7] = SqlDbType.VarChar;
            //paramtype[8] = SqlDbType.VarChar;
            //paramtype[9] = SqlDbType.VarChar;
            //paramtype[10] = SqlDbType.VarChar;
            //paramtype[11] = SqlDbType.VarChar;
            //paramtype[12] = SqlDbType.VarChar;
            //paramtype[13] = SqlDbType.VarChar;
            //paramtype[14] = SqlDbType.VarChar;
            //paramtype[15] = SqlDbType.VarChar;
            //paramtype[16] = SqlDbType.VarChar;
            //paramtype[17] = SqlDbType.DateTime;
            //paramtype[18] = SqlDbType.VarChar;
            //paramtype[19] = SqlDbType.VarChar;
            //paramtype[20] = SqlDbType.VarChar;
            //paramtype[21] = SqlDbType.VarChar;
            //paramtype[22] = SqlDbType.VarChar;
            //paramtype[23] = SqlDbType.VarChar;
            //paramtype[24] = SqlDbType.VarChar;
            //paramtype[25] = SqlDbType.VarChar;
            //paramtype[26] = SqlDbType.VarChar;
            //paramtype[27] = SqlDbType.VarChar;
            //paramtype[28] = SqlDbType.VarChar;
            //paramtype[29] = SqlDbType.Int;
            //paramtype[30] = SqlDbType.VarChar;
            //paramtype[31] = SqlDbType.VarChar;
            //paramtype[32] = SqlDbType.VarChar;
            //paramtype[33] = SqlDbType.VarChar;
            //paramtype[34] = SqlDbType.VarChar;
            SqlParameter[] paramname = new SqlParameter[] {
                new SqlParameter("@MAWBNo",SqlDbType.VarChar){ Value = MAWBNo },
                new SqlParameter("@HAWBNo",SqlDbType.VarChar){ Value = HAWBNo },
                new SqlParameter("@HAWBPcs",SqlDbType.Int){ Value = HAWBPcs },
                new SqlParameter("@HAWBWt",SqlDbType.Float){ Value = HAWBWt },
                new SqlParameter("@Description",SqlDbType.VarChar){ Value = Description },
                new SqlParameter("@CustID",SqlDbType.VarChar){ Value = CustID },
                new SqlParameter("@CustName",SqlDbType.VarChar){ Value = CustName },
                new SqlParameter("@CustAddress",SqlDbType.VarChar){ Value = CustAddress },
                new SqlParameter("@CustCity",SqlDbType.VarChar){ Value = CustCity },
                new SqlParameter("@Zipcode",SqlDbType.VarChar){ Value = Zipcode },
                new SqlParameter("@Origin",SqlDbType.VarChar){ Value = Origin },
                new SqlParameter("@Destination",SqlDbType.VarChar){ Value = Destination },
                new SqlParameter("@SHC",SqlDbType.VarChar){ Value = SHC },
                new SqlParameter("@HAWBPrefix",SqlDbType.VarChar){ Value = HAWBPrefix },
                new SqlParameter("@AWBPrefix",SqlDbType.VarChar){ Value = AWBPrefix },
                new SqlParameter("@ArrivalStatus",SqlDbType.VarChar){ Value = ArrivalStatus },
                new SqlParameter("@FlightNo",SqlDbType.VarChar){ Value = FlightNo },
                new SqlParameter("@FlightDt",SqlDbType.DateTime){ Value = FlightDt == "" ? DateTime.Now.ToString() : FlightDt },
                new SqlParameter("@FlightOrigin",SqlDbType.VarChar){ Value = FltOrigin },
                new SqlParameter("@flightDest",SqlDbType.VarChar){ Value = FltDest },
                new SqlParameter("@ConsigneeName",SqlDbType.VarChar){ Value = ConsigneeName },
                new SqlParameter("@ConsigneeAddress",SqlDbType.VarChar){ Value = ConsigneeAddress },
                new SqlParameter("@ConsigneeCity",SqlDbType.VarChar){ Value = ConsigneeCity },
                new SqlParameter("@ConsigneeState",SqlDbType.VarChar){ Value = ConsigneeState },
                new SqlParameter("@ConsigneeCountry",SqlDbType.VarChar){ Value = ConsigneeCountry },
                new SqlParameter("@ConsigneePostalCode",SqlDbType.VarChar){ Value = ConsigneePostalCode },
                new SqlParameter("@CustState",SqlDbType.VarChar){ Value = CustState },
                new SqlParameter("@CustCountry",SqlDbType.VarChar){ Value = CustCountry },
                new SqlParameter("@UOM",SqlDbType.VarChar){ Value = UOM },
                new SqlParameter("@SLAC",SqlDbType.Int){ Value = SLAC != string.Empty ? SLAC : "0" },
                new SqlParameter("@ConsigneeID",SqlDbType.VarChar){ Value = ConsigneeID },
                new SqlParameter("@ShipperEmail",SqlDbType.VarChar){ Value = ShipperEmail },
                new SqlParameter("@ShipperTelephone",SqlDbType.VarChar){ Value = ShipperTelephone },
                new SqlParameter("@ConsigneeEmail",SqlDbType.VarChar){ Value = ConsigneeEmail },
                new SqlParameter("@ConsigneeTelephone",SqlDbType.VarChar){ Value = ConsigneeTelephone }
            };

            //if (da.ExecuteProcedure("SP_PutHAWBDetails_V2", paramname, paramtype, paramvalue))
            if (await _readWriteDao.ExecuteNonQueryAsync("SP_PutHAWBDetails_V2", paramname))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region to generate FHL
        /// <summary>
        /// Used to generate FHL Message
        /// </summary>
        /// <param name="DepartureAirport"></param>
        /// <param name="FlightNo"></param>
        /// <param name="FlightDate"></param>
        /// <param name="username"></param>
        /// <param name="itdate"></param>
        /// <param name="AWBNumbers"></param>

        /// No reference found for this method
        //public void GenerateFHL(string DepartureAirport, string flightdest, string FlightNo, string FlightDate, DateTime itdate)
        //{
        //    string SitaMessageHeader = string.Empty, FHLMessageversion = string.Empty, Emailaddress = string.Empty, error = string.Empty;
        //    GenericFunction gf = new GenericFunction();
        //    MessageData.fblinfo objFBLInfo = new MessageData.fblinfo("");
        //    MessageData.unloadingport[] objUnloadingPort = new MessageData.unloadingport[0];
        //    MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
        //    FHLMessageProcessor FHL = new FHLMessageProcessor();

        //    DataSet dsData = gf.GetRecordforGenerateFBLMessage(DepartureAirport, flightdest, FlightNo, FlightDate);




        //    if (dsData != null && dsData.Tables.Count > 1 && dsData.Tables[0].Rows.Count > 0)
        //    {
        //        DataSet dsmessage = gf.GetSitaAddressandMessageVersion(FlightNo.Substring(0, 2), "FHL", "AIR", DepartureAirport, "", "", string.Empty);
        //        if (dsmessage != null && dsmessage.Tables[0].Rows.Count > 0)
        //        {
        //            Emailaddress = dsmessage.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
        //            string MessageCommunicationType = dsmessage.Tables[0].Rows[0]["MsgCommType"].ToString();
        //            FHLMessageversion = dsmessage.Tables[0].Rows[0]["MessageVersion"].ToString();
        //            if (MessageCommunicationType.Equals("ALL", StringComparison.OrdinalIgnoreCase) || MessageCommunicationType.Equals("SITA", StringComparison.OrdinalIgnoreCase))
        //                SitaMessageHeader = gf.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());
        //        }

        //        DataTable dt = new DataTable();

        //        if (dsData.Tables[1] != null && dsData.Tables[1].Rows.Count > 0)
        //        {

        //            dt = dsData.Tables[1];
        //            dt = GenericFunction.SelectDistinct(dt, "AWBNumbers");

        //            for (int i = 0; i < dt.Rows.Count; i++)
        //            {
        //                string FHLMsg = EncodeFHLForSend(dt.Rows[i]["AWBNumbers"].ToString().Substring(4, 8), dt.Rows[i]["AWBNumbers"].ToString().Substring(0, 3), ref error, FHLMessageversion);

        //                try
        //                {
        //                    if (FHLMsg.Length > 3)
        //                    {
        //                        clsLog.WriteLogAzure(" in FHLMsg len >0" + DateTime.Now + DateTime.Now + SitaMessageHeader + Emailaddress);

        //                        if (SitaMessageHeader != "")
        //                        {
        //                            gf.SaveMessageOutBox("FHL", SitaMessageHeader + "\r\n" + FHLMsg, "SITAFTP", "SITAFTP", "", "", "", "", dt.Rows[i]["AWBNumbers"].ToString());
        //                            clsLog.WriteLogAzure(" in SaveMessageOutBox SitaMessageHeader" + DateTime.Now);
        //                        }
        //                        //cls_BL.addMsgToOutBox("SITA:FFR", SitaMessageHeader.ToString() + "\r\n" + Msg, "", "SITAFTP", Session["UserName"].ToString(), Convert.ToDateTime(Session["IT"].ToString()), "FFR", txtAwbPrefix.Text.ToString() + "-" + AWBNumber);
        //                        if (Emailaddress != "")
        //                        {
        //                            clsLog.WriteLogAzure(" in SaveMessageOutBox  Emailaddress" + DateTime.Now);
        //                            gf.SaveMessageOutBox("FHL", FHLMsg, string.Empty, Emailaddress, "", "", "", "", dt.Rows[i]["AWBNumbers"].ToString());
        //                        }

        //                    }

        //                }
        //                catch (Exception ex)
        //                {
        //                    clsLog.WriteLogAzure("Error :", ex);
        //                }
        //            }
        //        }
        //    }
        //}

        #region Encode FHLForSend
        //No reference found for this method
        //public static string EncodeFHLForSend(string AWBNo, string AWBPrefix, ref string Error, string MsgVer)
        //{
        //    string FHLMsg = "";
        //    try
        //    {
        //        DataSet ds = new DataSet();
        //        SQLServer da = new SQLServer();
        //        string[] paramname = new string[] { "MAWBNo", "MAWBPrefix" };
        //        object[] paramvalue = new object[] { AWBNo, AWBPrefix };
        //        SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
        //        ds = da.SelectRecords("spGetHAWBSummary", paramname, paramvalue, paramtype);


        //        MessageData.fhlinfo fhl = new MessageData.fhlinfo("");
        //        MessageData.consignmnetinfo[] objTempConsInfo = new MessageData.consignmnetinfo[1];
        //        MessageData.customsextrainfo[] custominfo = new MessageData.customsextrainfo[0];
        //        if (ds != null)
        //        {
        //            if (ds.Tables.Count > 0)
        //            {
        //                string WeightCode = string.Empty;
        //                if (ds.Tables[0].Rows.Count > 0)
        //                {
        //                    //Master AWB number
        //                    DataRow dr = ds.Tables[0].Rows[0];
        //                    if (MsgVer.Length > 0)
        //                    {
        //                        fhl.fhlversionnum = MsgVer;
        //                    }
        //                    else
        //                    {
        //                        fhl.fhlversionnum = "4";
        //                    }
        //                    fhl.airlineprefix = dr["AWBPrefix"].ToString();
        //                    fhl.awbnum = dr["AWBNumber"].ToString();
        //                    fhl.origin = dr["OriginCode"].ToString();
        //                    fhl.dest = dr["DestinationCode"].ToString();
        //                    fhl.consigntype = "T";
        //                    fhl.pcscnt = dr["PiecesCount"].ToString();
        //                    fhl.weightcode = dr["UOM"].ToString();
        //                    fhl.weight = dr["GrossWeight"].ToString();
        //                    WeightCode = dr["UOM"].ToString();
        //                }
        //                if (ds.Tables[1].Rows.Count > 0)
        //                {
        //                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
        //                    {
        //                        try
        //                        {
        //                            DataRow dr = ds.Tables[1].Rows[i];
        //                            objTempConsInfo[0] = new MessageData.consignmnetinfo("");
        //                            objTempConsInfo[0].awbnum = dr["HAWBNo"].ToString();
        //                            objTempConsInfo[0].origin = dr["Origin"].ToString().ToUpper();
        //                            objTempConsInfo[0].dest = dr["Destination"].ToString();
        //                            objTempConsInfo[0].consigntype = "";
        //                            objTempConsInfo[0].pcscnt = dr["HAWBPcs"].ToString();
        //                            objTempConsInfo[0].weightcode = WeightCode != "" ? WeightCode : "K";
        //                            try
        //                            {
        //                                objTempConsInfo[0].weight = dr["HAWBWt"].ToString();
        //                                if ((Convert.ToDouble(dr["HAWBWt"].ToString()) - Convert.ToInt32(dr["HAWBWt"].ToString())) == 0)
        //                                {
        //                                    objTempConsInfo[0].weight = Convert.ToInt32(dr["HAWBWt"].ToString()).ToString();
        //                                }
        //                            }
        //                            catch (Exception)
        //                            {
        //                                //BAL.SCMException.logexception(ref ex);
        //                            }
        //                            objTempConsInfo[0].manifestdesc = dr["Description"].ToString();
        //                            objTempConsInfo[0].splhandling = dr["SHC"].ToString().Length > 0 ? dr["SHC"].ToString() : "";
        //                            objTempConsInfo[0].slac = dr["SLAC"].ToString();
        //                            if (dr["Description"].ToString().Length > 0)
        //                            {
        //                                objTempConsInfo[0].freetextGoodDesc = dr["Description"].ToString().ToUpper();
        //                            }
        //                            fhl.shippername = dr["ShipperName"].ToString();
        //                            string ShipAdd = dr["ShipperAddress"].ToString() + dr["ShipperAdd2"].ToString();
        //                            if (ShipAdd.Length > 35)
        //                            {
        //                                fhl.shipperadd = ShipAdd.Substring(0, 35);
        //                            }
        //                            else
        //                            {
        //                                fhl.shipperadd = dr["ShipperAddress"].ToString() + dr["ShipperAdd2"].ToString();
        //                            }
        //                            fhl.shipperplace = dr["ShipperCity"].ToString();
        //                            fhl.shipperstate = dr["ShipperState"].ToString();
        //                            fhl.shippercountrycode = dr["ShipperCountry"].ToString();
        //                            fhl.shipperpostcode = dr["ShipperPincode"].ToString();
        //                            fhl.shippercontactnum = dr["ShipperTelephone"].ToString();

        //                            //6 consignee info                    
        //                            fhl.consname = dr["ConsigneeName"].ToString();
        //                            string consAdd = dr["ConsigneeAddress"].ToString() + dr["ConsigneeAddress2"].ToString();
        //                            if (consAdd.Length > 35)
        //                            {
        //                                fhl.consadd = consAdd.Substring(0, 35);
        //                            }
        //                            else
        //                            {
        //                                fhl.consadd = dr["ConsigneeAddress"].ToString() + dr["ConsigneeAddress2"].ToString();
        //                            }
        //                            fhl.consplace = dr["ConsigneeCity"].ToString();
        //                            fhl.consstate = dr["ConsigneeState"].ToString();
        //                            fhl.conscountrycode = dr["ConsigneeCountry"].ToString();
        //                            fhl.conspostcode = dr["ConsigneePincode"].ToString();
        //                            fhl.conscontactnum = dr["ConsigneeTelephone"].ToString();

        //                            FHLMsg = cls_Encode_Decode.EncodeFHLforsend(ref fhl, ref objTempConsInfo, ref custominfo);


        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            clsLog.WriteLogAzure("Error :", ex);
        //                        }
        //                    }


        //                }

        //                else
        //                {
        //                    Error = "Required Data Not Availabe for Message";
        //                    return FHLMsg;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        Error = ex.Message;
        //    }
        //    return FHLMsg;
        //}
        #endregion

        public async Task<String> GenerateXFZBMessage(string awbPrefix, string awbNumber, string hawbNumber)
        {
            StringBuilder sbXFZBMessage = new StringBuilder();
            try
            {
                //GenericFunction generalFuncation = new GenericFunction();

                DataSet dsFzbMessage = await GetRecordforHAWBToGenerateXFZBMessage(awbPrefix, awbNumber, hawbNumber);

                if (dsFzbMessage != null && dsFzbMessage.Tables.Count > 0 && dsFzbMessage.Tables[1].Rows.Count > 0 && dsFzbMessage.Tables[2].Rows.Count > 0 && dsFzbMessage.Tables[3].Rows.Count > 0 && dsFzbMessage.Tables[4].Rows.Count > 0 && dsFzbMessage.Tables[5].Rows.Count > 0 && dsFzbMessage.Tables[7].Rows.Count > 0)
                {

                    XmlDocument xmlXFZB = new XmlDocument();
                    xmlXFZB.XmlResolver = null;

                    XmlSchema schema = new XmlSchema();
                    schema.Namespaces.Add("xsd", "http://www.w3.org/2001/XMLSchema");
                    schema.Namespaces.Add("rsm", "iata:housewaybill:1");
                    schema.Namespaces.Add("ccts", "urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2");
                    schema.Namespaces.Add("udt", "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8");
                    schema.Namespaces.Add("ram", "iata:datamodel:3");
                    schema.Namespaces.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    //schema.Namespaces.Add("schemaLocation", "iata:waybill:1 Waybill_1.xsd");
                    xmlXFZB.Schemas.Add(schema);

                    XmlElement HouseWaybill = xmlXFZB.CreateElement("rsm:HouseWaybill");
                    HouseWaybill.SetAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");
                    HouseWaybill.SetAttribute("xmlns:rsm", "iata:housewaybill:1");
                    HouseWaybill.SetAttribute("xmlns:ccts", "urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2");
                    HouseWaybill.SetAttribute("xmlns:udt", "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8");
                    HouseWaybill.SetAttribute("xmlns:ram", "iata:datamodel:3");
                    HouseWaybill.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    //Waybill.SetAttribute("xsi:schemaLocation", "iata:waybill:1 Waybill_1.xsd");
                    HouseWaybill.SetAttribute("schemaLocation", "iata:housewaybill:1 housewaybill_1.xsd");
                    xmlXFZB.AppendChild(HouseWaybill);
                    #region MessageHeaderDocument
                    if (dsFzbMessage != null && dsFzbMessage.Tables != null && dsFzbMessage.Tables.Count > 0 && dsFzbMessage.Tables[0].Rows.Count > 0)
                    {

                        XmlElement MessageHeaderDocument = xmlXFZB.CreateElement("rsm:MessageHeaderDocument");
                        HouseWaybill.AppendChild(MessageHeaderDocument);

                        XmlElement MessageHeaderDocument_ID = xmlXFZB.CreateElement("ram:ID");
                        MessageHeaderDocument_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[0].Rows[0]["ID"]);
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_ID);

                        XmlElement MessageHeaderDocument_Name = xmlXFZB.CreateElement("ram:Name");
                        MessageHeaderDocument_Name.InnerText = Convert.ToString(dsFzbMessage.Tables[0].Rows[0]["Name"]);
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_Name);

                        XmlElement MessageHeaderDocument_TypeCode = xmlXFZB.CreateElement("ram:TypeCode");
                        MessageHeaderDocument_TypeCode.InnerText = Convert.ToString(dsFzbMessage.Tables[0].Rows[0]["TypeCode"]);
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_TypeCode);

                        XmlElement MessageHeaderDocument_IssueDateTime = xmlXFZB.CreateElement("ram:IssueDateTime");
                        MessageHeaderDocument_IssueDateTime.InnerText = Convert.ToString(dsFzbMessage.Tables[0].Rows[0]["IssueDateTime"]);
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_IssueDateTime);

                        XmlElement MessageHeaderDocument_PurposeCode = xmlXFZB.CreateElement("ram:PurposeCode");
                        MessageHeaderDocument_PurposeCode.InnerText = Convert.ToString(dsFzbMessage.Tables[0].Rows[0]["PurposeCode"]);
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_PurposeCode);

                        XmlElement MessageHeaderDocument_VersionID = xmlXFZB.CreateElement("ram:VersionID");
                        MessageHeaderDocument_VersionID.InnerText = Convert.ToString(dsFzbMessage.Tables[0].Rows[0]["VersionID"]);
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_VersionID);

                        XmlElement MessageHeaderDocument_ConversationID = xmlXFZB.CreateElement("ram:ConversationID");
                        MessageHeaderDocument_ConversationID.InnerText = Convert.ToString(dsFzbMessage.Tables[0].Rows[0]["ConversationID"]);
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_ConversationID);

                        XmlElement MessageHeaderDocument_SenderParty = xmlXFZB.CreateElement("ram:SenderParty");
                        //MessageHeaderDocument_SenderParty.InnerText = "";
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_SenderParty);

                        XmlElement MessageHeaderDocument_SenderParty_PrimaryID = xmlXFZB.CreateElement("ram:PrimaryID");
                        MessageHeaderDocument_SenderParty_PrimaryID.SetAttribute("schemeID", Convert.ToString(dsFzbMessage.Tables[0].Rows[0]["SenderParty_PrimaryID"]));
                        MessageHeaderDocument_SenderParty_PrimaryID.InnerText = Convert.ToString(dsFzbMessage.Tables[0].Rows[0]["SenderParty_PrimaryIDText"]);
                        MessageHeaderDocument_SenderParty.AppendChild(MessageHeaderDocument_SenderParty_PrimaryID);

                        XmlElement MessageHeaderDocument_RecipientParty = xmlXFZB.CreateElement("ram:RecipientParty");
                        //MessageHeaderDocument_RecipientParty.InnerText = "";
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_RecipientParty);

                        XmlElement MessageHeaderDocument_RecipientParty_PrimaryID = xmlXFZB.CreateElement("ram:PrimaryID");
                        MessageHeaderDocument_RecipientParty_PrimaryID.SetAttribute("schemeID", Convert.ToString(dsFzbMessage.Tables[0].Rows[0]["RecipientParty_PrimaryID"]));
                        MessageHeaderDocument_RecipientParty_PrimaryID.InnerText = Convert.ToString(dsFzbMessage.Tables[0].Rows[0]["RecipientParty_PrimaryIDText"]);
                        MessageHeaderDocument_RecipientParty.AppendChild(MessageHeaderDocument_RecipientParty_PrimaryID);
                    }

                    #endregion MessageHeaderDocument
                    #region BusinessHeaderDocument

                    XmlElement BusinessHeaderDocument = xmlXFZB.CreateElement("rsm:BusinessHeaderDocument");
                    HouseWaybill.AppendChild(BusinessHeaderDocument);

                    XmlElement BusinessHeaderDocument_ID = xmlXFZB.CreateElement("ram:ID");
                    BusinessHeaderDocument_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["HAWBno"]);
                    BusinessHeaderDocument.AppendChild(BusinessHeaderDocument_ID);

                    XmlElement BusinessHeaderDocument_IncludedHeaderNote = xmlXFZB.CreateElement("rsm:IncludedHeaderNote");
                    BusinessHeaderDocument.AppendChild(BusinessHeaderDocument_IncludedHeaderNote);

                    XmlElement BusinessHeaderDocument_IncludedHeaderNote_ContentCode = xmlXFZB.CreateElement("ram:ContentCode");
                    BusinessHeaderDocument_IncludedHeaderNote_ContentCode.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["HeaderNote"]);
                    BusinessHeaderDocument_IncludedHeaderNote.AppendChild(BusinessHeaderDocument_IncludedHeaderNote_ContentCode);

                    XmlElement BusinessHeaderDocument_IncludedHeaderNote_Content = xmlXFZB.CreateElement("ram:Content");
                    BusinessHeaderDocument_IncludedHeaderNote_Content.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["HeaderText"]);
                    BusinessHeaderDocument_IncludedHeaderNote.AppendChild(BusinessHeaderDocument_IncludedHeaderNote_Content);

                    XmlElement BusinessHeaderDocument_SignatoryConsignorAuthentication = xmlXFZB.CreateElement("rsm:SignatoryConsignorAuthentication");
                    BusinessHeaderDocument.AppendChild(BusinessHeaderDocument_SignatoryConsignorAuthentication);

                    XmlElement BusinessHeaderDocument_SignatoryConsignorAuthentication_Signatory = xmlXFZB.CreateElement("ram:Signatory");
                    BusinessHeaderDocument_SignatoryConsignorAuthentication_Signatory.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperName"]);
                    BusinessHeaderDocument_SignatoryConsignorAuthentication.AppendChild(BusinessHeaderDocument_SignatoryConsignorAuthentication_Signatory);

                    XmlElement BusinessHeaderDocument_SignatoryCarrierAuthentication = xmlXFZB.CreateElement("rsm:SignatoryCarrierAuthentication");
                    BusinessHeaderDocument.AppendChild(BusinessHeaderDocument_SignatoryCarrierAuthentication);

                    XmlElement BusinessHeaderDocument_SignatoryCarrierAuthentication_ActualDateTime = xmlXFZB.CreateElement("ram:ActualDateTime");
                    BusinessHeaderDocument_SignatoryCarrierAuthentication_ActualDateTime.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["CarrierDeclarationDate"]);
                    BusinessHeaderDocument_SignatoryCarrierAuthentication.AppendChild(BusinessHeaderDocument_SignatoryCarrierAuthentication_ActualDateTime);

                    XmlElement BusinessHeaderDocument_SignatoryCarrierAuthentication_Signatory = xmlXFZB.CreateElement("ram:Signatory");
                    BusinessHeaderDocument_SignatoryCarrierAuthentication_Signatory.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["CarrierDeclarationSignature"]);
                    BusinessHeaderDocument_SignatoryCarrierAuthentication.AppendChild(BusinessHeaderDocument_SignatoryCarrierAuthentication_Signatory);

                    XmlElement BusinessHeaderDocument_SignatoryCarrierAuthentication_IssueAuthenticationLocation = xmlXFZB.CreateElement("ram:IssueAuthenticationLocation");

                    BusinessHeaderDocument_SignatoryCarrierAuthentication.AppendChild(BusinessHeaderDocument_SignatoryCarrierAuthentication_IssueAuthenticationLocation);

                    XmlElement BusinessHeaderDocument_SignatoryCarrierAuthentication_IssueAuthenticationLocation_Name = xmlXFZB.CreateElement("ram:Name");
                    BusinessHeaderDocument_SignatoryCarrierAuthentication_IssueAuthenticationLocation_Name.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["CarrierDeclarationPlace"]);
                    BusinessHeaderDocument_SignatoryCarrierAuthentication_IssueAuthenticationLocation.AppendChild(BusinessHeaderDocument_SignatoryCarrierAuthentication_IssueAuthenticationLocation_Name);

                    #endregion BusinessHeaderDocument

                    #region MasterConsignment
                    XmlElement MasterConsignment = xmlXFZB.CreateElement("rsm:MasterConsignment");
                    HouseWaybill.AppendChild(MasterConsignment);

                    XmlElement MasterConsignment_IncludedTareGrossWeightMeasure = xmlXFZB.CreateElement("ram:IncludedTareGrossWeightMeasure");
                    MasterConsignment_IncludedTareGrossWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["UOM"]));
                    MasterConsignment_IncludedTareGrossWeightMeasure.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["GrossWeight"]);
                    MasterConsignment.AppendChild(MasterConsignment_IncludedTareGrossWeightMeasure);

                    XmlElement MasterConsignment_TotalPieceQuantity = xmlXFZB.CreateElement("ram:TotalPieceQuantity");
                    MasterConsignment_TotalPieceQuantity.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["PiecesCount"]);
                    MasterConsignment.AppendChild(MasterConsignment_TotalPieceQuantity);

                    XmlElement MasterConsignment_TransportContractDocument = xmlXFZB.CreateElement("ram:TransportContractDocument");
                    MasterConsignment.AppendChild(MasterConsignment_TransportContractDocument);

                    XmlElement MasterConsignment_TransportContractDocument_ID = xmlXFZB.CreateElement("ram:ID");
                    MasterConsignment_TransportContractDocument_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["AWBNumber"]);
                    MasterConsignment_TransportContractDocument.AppendChild(MasterConsignment_TransportContractDocument_ID);

                    XmlElement MasterConsignment_OriginLocation = xmlXFZB.CreateElement("ram:OriginLocation");
                    MasterConsignment.AppendChild(MasterConsignment_OriginLocation);

                    XmlElement MasterConsignment_OriginLocation_ID = xmlXFZB.CreateElement("ram:ID");
                    MasterConsignment_OriginLocation_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["OriginCode"]);
                    MasterConsignment_OriginLocation.AppendChild(MasterConsignment_OriginLocation_ID);

                    XmlElement MasterConsignment_OriginLocation_Name = xmlXFZB.CreateElement("ram:Name");
                    MasterConsignment_OriginLocation_Name.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["AirportName"]);
                    MasterConsignment_OriginLocation.AppendChild(MasterConsignment_OriginLocation_Name);

                    XmlElement MasterConsignment_FinalDestinationLocation = xmlXFZB.CreateElement("ram:FinalDestinationLocation");
                    MasterConsignment.AppendChild(MasterConsignment_FinalDestinationLocation);


                    XmlElement MasterConsignment_FinalDestinationLocation_ID = xmlXFZB.CreateElement("ram:ID");
                    MasterConsignment_FinalDestinationLocation_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["DestinationCode"]);
                    MasterConsignment_FinalDestinationLocation.AppendChild(MasterConsignment_FinalDestinationLocation_ID);

                    XmlElement MasterConsignment_FinalDestinationLocation_Name = xmlXFZB.CreateElement("ram:Name");
                    MasterConsignment_FinalDestinationLocation_Name.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["DestinationAirport"]);
                    MasterConsignment_FinalDestinationLocation.AppendChild(MasterConsignment_FinalDestinationLocation_Name);

                    XmlElement MasterConsignment_IncludedHouseConsignment = xmlXFZB.CreateElement("ram:IncludedHouseConsignment");
                    MasterConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ID = xmlXFZB.CreateElement("ram:ID");
                    MasterConsignment_IncludedHouseConsignment_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["HAWBno"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_ID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_AdditionalID = xmlXFZB.CreateElement("ram:AdditionalID");
                    MasterConsignment_IncludedHouseConsignment_AdditionalID.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["AdditionalID"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_AdditionalID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedReferenceID = xmlXFZB.CreateElement("ram:AssociatedReferenceID");
                    MasterConsignment_IncludedHouseConsignment_AssociatedReferenceID.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["AssociatedReferenceID"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedReferenceID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_NilCarriageValueIndicator = xmlXFZB.CreateElement("ram:NilCarriageValueIndicator");
                    MasterConsignment_IncludedHouseConsignment_NilCarriageValueIndicator.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["NilCarriageValueIndicator"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_NilCarriageValueIndicator);

                    XmlElement MasterConsignment_IncludedHouseConsignment_NilCustomsValueIndicator = xmlXFZB.CreateElement("ram:NilCustomsValueIndicator");
                    MasterConsignment_IncludedHouseConsignment_NilCustomsValueIndicator.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["NilCustomsValueIndicator"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_NilCustomsValueIndicator);

                    XmlElement MasterConsignment_IncludedHouseConsignment_DeclaredValueForCustomsAmount = xmlXFZB.CreateElement("ram:DeclaredValueForCustomsAmount");
                    MasterConsignment_IncludedHouseConsignment_DeclaredValueForCustomsAmount.SetAttribute("currencyID", Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["DVCustomCurrency"]));
                    MasterConsignment_IncludedHouseConsignment_DeclaredValueForCustomsAmount.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["DVForCustom"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_DeclaredValueForCustomsAmount);

                    XmlElement MasterConsignment_IncludedHouseConsignment_NilInsuranceValueIndicator = xmlXFZB.CreateElement("ram:NilInsuranceValueIndicator");
                    MasterConsignment_IncludedHouseConsignment_NilInsuranceValueIndicator.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["NilInsuranceValueIndicator"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_NilInsuranceValueIndicator);

                    XmlElement MasterConsignment_IncludedHouseConsignment_TotalChargePrepaidIndicator = xmlXFZB.CreateElement("ram:TotalChargePrepaidIndicator");
                    MasterConsignment_IncludedHouseConsignment_TotalChargePrepaidIndicator.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["TotalChargePrepaidIndicator"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_TotalChargePrepaidIndicator);

                    XmlElement MasterConsignment_IncludedHouseConsignment_TotalDisbursementPrepaidIndicator = xmlXFZB.CreateElement("ram:TotalDisbursementPrepaidIndicator");
                    MasterConsignment_IncludedHouseConsignment_TotalDisbursementPrepaidIndicator.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["TotalDisbursementPrepaidIndicator"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_TotalDisbursementPrepaidIndicator);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_DeclaredValueForCarriageAmount = xmlXFZB.CreateElement("ram:DeclaredValueForCarriageAmount");
                    //MasterConsignment_IncludedHouseConsignment_DeclaredValueForCarriageAmount.SetAttribute("currencyID", Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["DVCarriageCurrency"]));
                    //MasterConsignment_IncludedHouseConsignment_DeclaredValueForCarriageAmount.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["DVForCarriage"]);
                    //MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_DeclaredValueForCarriageAmount);

                    XmlElement MasterConsignment_IncludedHouseConsignment_AgentTotalDisbursementAmount = xmlXFZB.CreateElement("ram:AgentTotalDisbursementAmount");
                    MasterConsignment_IncludedHouseConsignment_AgentTotalDisbursementAmount.SetAttribute("currencyID", Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["DVCarriageCurrency"]));
                    MasterConsignment_IncludedHouseConsignment_AgentTotalDisbursementAmount.InnerText = Convert.ToString(dsFzbMessage.Tables[9].Rows[0]["AgentTotalDisbursementAmount"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_AgentTotalDisbursementAmount);

                    XmlElement MasterConsignment_IncludedHouseConsignment_CarrierTotalDisbursementAmount = xmlXFZB.CreateElement("ram:CarrierTotalDisbursementAmount");
                    MasterConsignment_IncludedHouseConsignment_CarrierTotalDisbursementAmount.SetAttribute("currencyID", Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["DVCarriageCurrency"]));
                    MasterConsignment_IncludedHouseConsignment_CarrierTotalDisbursementAmount.InnerText = Convert.ToString(dsFzbMessage.Tables[9].Rows[0]["CarrierTotalDisbursementAmount"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_CarrierTotalDisbursementAmount);

                    XmlElement MasterConsignment_IncludedHouseConsignment_TotalPrepaidChargeAmount = xmlXFZB.CreateElement("ram:TotalPrepaidChargeAmount");
                    MasterConsignment_IncludedHouseConsignment_TotalPrepaidChargeAmount.SetAttribute("currencyID", Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["DVCarriageCurrency"]));
                    MasterConsignment_IncludedHouseConsignment_TotalPrepaidChargeAmount.InnerText = Convert.ToString(dsFzbMessage.Tables[9].Rows[0]["TotalPrepaidChargeAmount"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_TotalPrepaidChargeAmount);

                    XmlElement MasterConsignment_IncludedHouseConsignment_TotalCollectChargeAmount = xmlXFZB.CreateElement("ram:TotalCollectChargeAmount");
                    //MasterConsignment_IncludedHouseConsignment_TotalCollectChargeAmount.SetAttribute("currencyID", Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["DVCarriageCurrency"]));
                    MasterConsignment_IncludedHouseConsignment_TotalCollectChargeAmount.InnerText = "0";
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_TotalCollectChargeAmount);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedTareGrossWeightMeasure = xmlXFZB.CreateElement("ram:IncludedTareGrossWeightMeasure");
                    MasterConsignment_IncludedHouseConsignment_IncludedTareGrossWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["UOM"]));
                    MasterConsignment_IncludedHouseConsignment_IncludedTareGrossWeightMeasure.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["HouseGrossWeight"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedTareGrossWeightMeasure);

                    XmlElement MasterConsignment_IncludedHouseConsignment_GrossVolumeMeasure = xmlXFZB.CreateElement("ram:GrossVolumeMeasure");
                    MasterConsignment_IncludedHouseConsignment_GrossVolumeMeasure.SetAttribute("unitCode", Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["UOM"]));
                    MasterConsignment_IncludedHouseConsignment_GrossVolumeMeasure.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["HAWBWt"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_GrossVolumeMeasure);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignmentItemQuantity = xmlXFZB.CreateElement("ram:ConsignmentItemQuantity");
                    MasterConsignment_IncludedHouseConsignment_ConsignmentItemQuantity.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["HousePieces"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignmentItemQuantity);

                    XmlElement MasterConsignment_IncludedHouseConsignment_PackageQuantity = xmlXFZB.CreateElement("ram:PackageQuantity");
                    MasterConsignment_IncludedHouseConsignment_PackageQuantity.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["HousePackageQuantity"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_PackageQuantity);

                    XmlElement MasterConsignment_IncludedHouseConsignment_TotalPieceQuantity = xmlXFZB.CreateElement("ram:TotalPieceQuantity");
                    MasterConsignment_IncludedHouseConsignment_TotalPieceQuantity.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["HousePieces"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_TotalPieceQuantity);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SummaryDescription = xmlXFZB.CreateElement("ram:SummaryDescription");
                    MasterConsignment_IncludedHouseConsignment_SummaryDescription.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["Description"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_SummaryDescription);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightRateTypeCode = xmlXFZB.CreateElement("ram:FreightRateTypeCode");
                    MasterConsignment_IncludedHouseConsignment_FreightRateTypeCode.InnerText = Convert.ToString(dsFzbMessage.Tables[7].Rows[0]["RateClass"]);
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightRateTypeCode);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_InsuranceValueAmount = xmlXFZB.CreateElement("ram:InsuranceValueAmount");
                    //MasterConsignment_IncludedHouseConsignment_InsuranceValueAmount.SetAttribute("currencyID", Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["InsuranceCurrency"]));
                    //MasterConsignment_IncludedHouseConsignment_InsuranceValueAmount.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["InsuranceAmount"]);
                    //MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_InsuranceValueAmount);


                    //XmlElement MasterConsignment_IncludedHouseConsignment_WeightTotalChargeAmount = xmlXFZB.CreateElement("ram:WeightTotalChargeAmount");
                    //MasterConsignment_IncludedHouseConsignment_WeightTotalChargeAmount.SetAttribute("currencyID", Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["DVCarriageCurrency"]));
                    //MasterConsignment_IncludedHouseConsignment_WeightTotalChargeAmount.InnerText = Convert.ToString(dsFzbMessage.Tables[9].Rows[0]["WeightTotalChargeAmount"]);
                    //MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_WeightTotalChargeAmount);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_ValuationTotalChargeAmount = xmlXFZB.CreateElement("ram:ValuationTotalChargeAmount");
                    //MasterConsignment_IncludedHouseConsignment_ValuationTotalChargeAmount.SetAttribute("currencyID", Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["DVCarriageCurrency"]));
                    //MasterConsignment_IncludedHouseConsignment_ValuationTotalChargeAmount.InnerText = Convert.ToString(dsFzbMessage.Tables[9].Rows[0]["ValuationTotalChargeAmount"]);
                    //MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_ValuationTotalChargeAmount);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_TaxTotalChargeAmount = xmlXFZB.CreateElement("ram:TaxTotalChargeAmount");
                    //MasterConsignment_IncludedHouseConsignment_TaxTotalChargeAmount.SetAttribute("currencyID", Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["DVCarriageCurrency"]));
                    //MasterConsignment_IncludedHouseConsignment_TaxTotalChargeAmount.InnerText = Convert.ToString(dsFzbMessage.Tables[9].Rows[0]["TaxTotalChargeAmount"]);
                    //MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_TaxTotalChargeAmount);


                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty = xmlXFZB.CreateElement("ram:ConsignorParty");

                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_PrimaryID = xmlXFZB.CreateElement("ram:PrimaryID");
                    //MasterConsignment_IncludedHouseConsignment_ConsignorParty_PrimaryID.SetAttribute("schemeAgencyID", Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["CustID"]));
                    //MasterConsignment_IncludedHouseConsignment_ConsignorParty_PrimaryID.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["CustName"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_PrimaryID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_AdditionalID = xmlXFZB.CreateElement("ram:AdditionalID");
                    //MasterConsignment_IncludedHouseConsignment_ConsignorParty_AdditionalID.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperStandardID"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_AdditionalID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_Name = xmlXFZB.CreateElement("ram:Name");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_Name.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperName"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_Name);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_AccountID = xmlXFZB.CreateElement("ram:AccountID");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_AccountID.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperAccCode"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_AccountID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress = xmlXFZB.CreateElement("ram:PostalStructuredAddress");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_PostcodeCode = xmlXFZB.CreateElement("ram:PostcodeCode");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_PostcodeCode.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperPincode"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_PostcodeCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_StreetName = xmlXFZB.CreateElement("ram:StreetName");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_StreetName.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperAddress"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_StreetName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CityName = xmlXFZB.CreateElement("ram:CityName");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CityName.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperCity"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CityName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CountryID = xmlXFZB.CreateElement("ram:CountryID");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CountryID.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperCountry"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CountryID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CountryName = xmlXFZB.CreateElement("ram:CountryName");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CountryName.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperCountry"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CountryName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CountrySubDivisionName = xmlXFZB.CreateElement("ram:CountrySubDivisionName");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CountrySubDivisionName.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperRegionName"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CountrySubDivisionName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_PostOfficeBox = xmlXFZB.CreateElement("ram:PostOfficeBox");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_PostOfficeBox.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperPincode"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_PostOfficeBox);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CityID = xmlXFZB.CreateElement("ram:CityID");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CityID.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperCity"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CityID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CountrySubDivisionID = xmlXFZB.CreateElement("ram:CountrySubDivisionID");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CountrySubDivisionID.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperRegionName"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_PostalStructuredAddress_CountrySubDivisionID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact = xmlXFZB.CreateElement("ram:DefinedTradeContact");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_PersonName = xmlXFZB.CreateElement("ram:PersonName");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_PersonName.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["CustName"]);
                    //MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_PersonName.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigneeCountry"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_PersonName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_DepartmentName = xmlXFZB.CreateElement("ram:DepartmentName");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_DepartmentName.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperDepartnmentName"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_DepartmentName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_DirectTelephoneCommunication = xmlXFZB.CreateElement("ram:DirectTelephoneCommunication");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_DirectTelephoneCommunication);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_DirectTelephoneCommunication_CompleteNumber = xmlXFZB.CreateElement("ram:CompleteNumber");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_DirectTelephoneCommunication_CompleteNumber.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperTelephone"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_DirectTelephoneCommunication.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_DirectTelephoneCommunication_CompleteNumber);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_FaxCommunication = xmlXFZB.CreateElement("ram:FaxCommunication");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_FaxCommunication);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_FaxCommunication_CompleteNumber = xmlXFZB.CreateElement("ram:CompleteNumber");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_FaxCommunication_CompleteNumber.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShippFaxNo"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_FaxCommunication.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_FaxCommunication_CompleteNumber);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_URIEmailCommunication = xmlXFZB.CreateElement("ram:URIEmailCommunication");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_URIEmailCommunication);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_URIEmailCommunication_URIID = xmlXFZB.CreateElement("ram:URIID");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_URIEmailCommunication_URIID.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperEmailId"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_URIEmailCommunication.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_URIEmailCommunication_URIID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_TelexCommunication = xmlXFZB.CreateElement("ram:TelexCommunication");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_TelexCommunication);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_TelexCommunication_CompleteNumber = xmlXFZB.CreateElement("ram:CompleteNumber");
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_TelexCommunication_CompleteNumber.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperTelex"]);
                    MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_TelexCommunication.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsignorParty_DefinedTradeContact_TelexCommunication_CompleteNumber);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty = xmlXFZB.CreateElement("ram:ConsigneeParty");
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PrimaryID = xmlXFZB.CreateElement("ram:PrimaryID");
                    //MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PrimaryID.SetAttribute("schemeAgencyID", Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["ConsigneeID"]));
                    //MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PrimaryID.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["ConsigneeName"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PrimaryID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_AdditionalID = xmlXFZB.CreateElement("ram:AdditionalID");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_AdditionalID.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigneeStandardID"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_AdditionalID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_Name = xmlXFZB.CreateElement("ram:Name");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_Name.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["ConsigneeName"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_Name);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_AccountID = xmlXFZB.CreateElement("ram:AccountID");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_AccountID.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigAccCode"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_AccountID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress = xmlXFZB.CreateElement("ram:PostalStructuredAddress");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_PostcodeCode = xmlXFZB.CreateElement("ram:PostcodeCode");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_PostcodeCode.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigneePincode"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_PostcodeCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_StreetName = xmlXFZB.CreateElement("ram:StreetName");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_StreetName.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigneeAddress"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_StreetName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CityName = xmlXFZB.CreateElement("ram:CityName");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CityName.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigneeCity"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CityName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CountryID = xmlXFZB.CreateElement("ram:CountryID");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CountryID.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigneeCountry"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CountryID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CountryName = xmlXFZB.CreateElement("ram:CountryName");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CountryName.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigneeCountry"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CountryName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CountrySubDivisionName = xmlXFZB.CreateElement("ram:CountrySubDivisionName");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CountrySubDivisionName.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigneeRegionName"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CountrySubDivisionName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_PostOfficeBox = xmlXFZB.CreateElement("ram:PostOfficeBox");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_PostOfficeBox.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigneePincode"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_PostOfficeBox);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CityID = xmlXFZB.CreateElement("ram:CityID");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CityID.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigneeCity"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CityID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CountrySubDivisionID = xmlXFZB.CreateElement("ram:CountrySubDivisionID");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CountrySubDivisionID.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigneeCountry"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_PostalStructuredAddress_CountrySubDivisionID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact = xmlXFZB.CreateElement("ram:DefinedTradeContact");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_PersonName = xmlXFZB.CreateElement("ram:PersonName");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_PersonName.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperName"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_PersonName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_DepartmentName = xmlXFZB.CreateElement("ram:DepartmentName");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_DepartmentName.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigneeDepartnmentName"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_DepartmentName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_DirectTelephoneCommunication = xmlXFZB.CreateElement("ram:DirectTelephoneCommunication");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_DirectTelephoneCommunication);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_DirectTelephoneCommunication_CompleteNumber = xmlXFZB.CreateElement("ram:CompleteNumber");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_DirectTelephoneCommunication_CompleteNumber.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigneeTelephone"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_DirectTelephoneCommunication.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_DirectTelephoneCommunication_CompleteNumber);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_FaxCommunication = xmlXFZB.CreateElement("ram:FaxCommunication");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_FaxCommunication);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_FaxCommunication_CompleteNumber = xmlXFZB.CreateElement("ram:CompleteNumber");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_FaxCommunication_CompleteNumber.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigneeFaxNo"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_FaxCommunication.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_FaxCommunication_CompleteNumber);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_URIEmailCommunication = xmlXFZB.CreateElement("ram:URIEmailCommunication");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_URIEmailCommunication);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_URIEmailCommunication_URIID = xmlXFZB.CreateElement("ram:URIID");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_URIEmailCommunication_URIID.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigEmailId"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_URIEmailCommunication.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_URIEmailCommunication_URIID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_TelexCommunication = xmlXFZB.CreateElement("ram:TelexCommunication");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_TelexCommunication);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_TelexCommunication_CompleteNumber = xmlXFZB.CreateElement("ram:CompleteNumber");
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_TelexCommunication_CompleteNumber.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ConsigneeTelex"]);
                    MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_TelexCommunication.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_TelexCommunication_CompleteNumber);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty = xmlXFZB.CreateElement("ram:FreightForwarderParty");
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PrimaryID = xmlXFZB.CreateElement("ram:PrimaryID");
                    //MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PrimaryID.SetAttribute("schemeAgencyID", Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["ShippingAgentCode"]));
                    //MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PrimaryID.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["ShippingAgentName"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PrimaryID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_AdditionalID = xmlXFZB.CreateElement("ram:AdditionalID");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_AdditionalID.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["ShippingAgentCode"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_AdditionalID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_Name = xmlXFZB.CreateElement("ram:Name");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_Name.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["ShippingAgentName"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_Name);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_AccountID = xmlXFZB.CreateElement("ram:AccountID");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_AccountID.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["AgentAccountCode"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_AccountID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress = xmlXFZB.CreateElement("ram:PostalStructuredAddress");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_PostcodeCode = xmlXFZB.CreateElement("ram:PostcodeCode");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_PostcodeCode.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["PostalZIP"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_PostcodeCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_StreetName = xmlXFZB.CreateElement("ram:StreetName");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_StreetName.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["AgentAddress"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_StreetName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CityName = xmlXFZB.CreateElement("ram:CityName");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CityName.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["City"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CityName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CountryID = xmlXFZB.CreateElement("ram:CountryID");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CountryID.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["Country"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CountryID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CountryName = xmlXFZB.CreateElement("ram:CountryName");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CountryName.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["CountryName"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CountryName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CountrySubDivisionName = xmlXFZB.CreateElement("ram:CountrySubDivisionName");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CountrySubDivisionName.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["State"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CountrySubDivisionName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_PostOfficeBox = xmlXFZB.CreateElement("ram:PostOfficeBox");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_PostOfficeBox.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["PostalZIP"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_PostOfficeBox);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CityID = xmlXFZB.CreateElement("ram:CityID");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CityID.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["Station"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CityID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CountrySubDivisionID = xmlXFZB.CreateElement("ram:CountrySubDivisionID");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CountrySubDivisionID.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["State"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_PostalStructuredAddress_CountrySubDivisionID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact = xmlXFZB.CreateElement("ram:DefinedTradeContact");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_PersonName = xmlXFZB.CreateElement("ram:PersonName");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_PersonName.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["ShippingAgentName"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_PersonName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_DepartmentName = xmlXFZB.CreateElement("ram:DepartmentName");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_DepartmentName.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["DepartmentName"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_DepartmentName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_DirectTelephoneCommunication = xmlXFZB.CreateElement("ram:DirectTelephoneCommunication");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_DirectTelephoneCommunication);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_DirectTelephoneCommunication_CompleteNumber = xmlXFZB.CreateElement("ram:CompleteNumber");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_DirectTelephoneCommunication_CompleteNumber.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["AgentPhone"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_DirectTelephoneCommunication.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_DirectTelephoneCommunication_CompleteNumber);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_FaxCommunication = xmlXFZB.CreateElement("ram:FaxCommunication");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_FaxCommunication);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_FaxCommunication_CompleteNumber = xmlXFZB.CreateElement("ram:CompleteNumber");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_FaxCommunication_CompleteNumber.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["AgentFax"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_FaxCommunication.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_FaxCommunication_CompleteNumber);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_URIEmailCommunication = xmlXFZB.CreateElement("ram:URIEmailCommunication");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_URIEmailCommunication);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_URIEmailCommunication_URIID = xmlXFZB.CreateElement("ram:URIID");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_URIEmailCommunication_URIID.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["AgentEmailID"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_URIEmailCommunication.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_URIEmailCommunication_URIID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_TelexCommunication = xmlXFZB.CreateElement("ram:TelexCommunication");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_TelexCommunication);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_TelexCommunication_CompleteNumber = xmlXFZB.CreateElement("ram:CompleteNumber");
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_TelexCommunication_CompleteNumber.InnerText = Convert.ToString(dsFzbMessage.Tables[4].Rows[0]["AgentSitaAddress"]);
                    MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_TelexCommunication.AppendChild(MasterConsignment_IncludedHouseConsignment_FreightForwarderParty_DefinedTradeContact_TelexCommunication_CompleteNumber);

                    XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty = xmlXFZB.CreateElement("ram:AssociatedParty");
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_PrimaryID = xmlXFZB.CreateElement("ram:PrimaryID");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_PrimaryID.SetAttribute("schemeAgencyID", Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_schemeAgencyID"]));
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_PrimaryID.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_PrimaryID_Text"]);
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_PrimaryID);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_AdditionalID = xmlXFZB.CreateElement("ram:AdditionalID");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_AdditionalID.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_AdditionalID"]);
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_AdditionalID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_Name = xmlXFZB.CreateElement("ram:Name");
                    MasterConsignment_IncludedHouseConsignment_AssociatedParty_Name.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_Name"]);
                    MasterConsignment_IncludedHouseConsignment_AssociatedParty.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_Name);

                    XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_RoleCode = xmlXFZB.CreateElement("ram:RoleCode");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_RoleCode.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_RoleCode"]);
                    MasterConsignment_IncludedHouseConsignment_AssociatedParty_RoleCode.InnerText = "NI";
                    MasterConsignment_IncludedHouseConsignment_AssociatedParty.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_RoleCode);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_Role = xmlXFZB.CreateElement("ram:Role");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_Role.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_Role"]);
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_Role);

                    XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress = xmlXFZB.CreateElement("ram:PostalStructuredAddress");
                    MasterConsignment_IncludedHouseConsignment_AssociatedParty.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress);

                    XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_PostcodeCode = xmlXFZB.CreateElement("ram:PostcodeCode");
                    MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_PostcodeCode.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_PostcodeCode"]);
                    MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_PostcodeCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_StreetName = xmlXFZB.CreateElement("ram:StreetName");
                    MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_StreetName.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_StreetName"]);
                    MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_StreetName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CityName = xmlXFZB.CreateElement("ram:CityName");
                    MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CityName.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_CityName"]);
                    MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CityName);

                    XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CountryID = xmlXFZB.CreateElement("ram:CountryID");
                    MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CountryID.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_CountryID"]);
                    MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CountryID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CountryName = xmlXFZB.CreateElement("ram:CountryName");
                    MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CountryName.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_CountryName"]);
                    MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CountryName);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CountrySubDivisionName = xmlXFZB.CreateElement("ram:CountrySubDivisionName");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CountrySubDivisionName.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_CountrySubDivisionName"]);
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CountrySubDivisionName);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_PostOfficeBox = xmlXFZB.CreateElement("ram:PostOfficeBox");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_PostOfficeBox.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_PostOfficeBox"]);
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_PostOfficeBox);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CityID = xmlXFZB.CreateElement("ram:CityID");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CityID.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_CityID"]);
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CityID);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CountrySubDivisionID = xmlXFZB.CreateElement("ram:CountrySubDivisionID");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CountrySubDivisionID.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_CountrySubDivisionID"]);
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_PostalStructuredAddress_CountrySubDivisionID);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_SpecifiedAddressLocation = xmlXFZB.CreateElement("ram:SpecifiedAddressLocation");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_SpecifiedAddressLocation);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_SpecifiedAddressLocation_ID = xmlXFZB.CreateElement("ram:ID");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_SpecifiedAddressLocation_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_SpecifiedAddressLocation_ID"]);
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_SpecifiedAddressLocation.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_SpecifiedAddressLocation_ID);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_SpecifiedAddressLocation_Name = xmlXFZB.CreateElement("ram:Name");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_SpecifiedAddressLocation_Name.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_SpecifiedAddressLocation_Name"]);
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_SpecifiedAddressLocation.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_SpecifiedAddressLocation_Name);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_SpecifiedAddressLocation_TypeCode = xmlXFZB.CreateElement("ram:TypeCode");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_SpecifiedAddressLocation_TypeCode.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_SpecifiedAddressLocation_TypeCode"]);
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_SpecifiedAddressLocation.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_SpecifiedAddressLocation_TypeCode);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact = xmlXFZB.CreateElement("ram:DefinedTradeContact");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_PersonName = xmlXFZB.CreateElement("ram:PersonName");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_PersonName.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_DefinedTradeContact_PersonName"]);
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_PersonName);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_DepartmentName = xmlXFZB.CreateElement("ram:DepartmentName");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_DepartmentName.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_DefinedTradeContact_DepartmentName"]);
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_DepartmentName);


                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_DirectTelephoneCommunication = xmlXFZB.CreateElement("ram:DirectTelephoneCommunication");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_ConsigneeParty_DefinedTradeContact_DirectTelephoneCommunication);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_DirectTelephoneCommunication_CompleteNumber = xmlXFZB.CreateElement("ram:CompleteNumber");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_DirectTelephoneCommunication_CompleteNumber.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_DirectTelephoneCommunication"]);
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_DirectTelephoneCommunication.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_DirectTelephoneCommunication_CompleteNumber);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_FaxCommunication = xmlXFZB.CreateElement("ram:FaxCommunication");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_FaxCommunication);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_FaxCommunication_CompleteNumber = xmlXFZB.CreateElement("ram:CompleteNumber");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_FaxCommunication_CompleteNumber.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_FaxCommunication"]);
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_FaxCommunication.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_FaxCommunication_CompleteNumber);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_URIEmailCommunication = xmlXFZB.CreateElement("ram:URIEmailCommunication");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_URIEmailCommunication);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_URIEmailCommunication_URIID = xmlXFZB.CreateElement("ram:URIID");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_URIEmailCommunication_URIID.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_URIEmailCommunication"]);
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_URIEmailCommunication.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_URIEmailCommunication_URIID);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_TelexCommunication = xmlXFZB.CreateElement("ram:TelexCommunication");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_TelexCommunication);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_TelexCommunication_CompleteNumber = xmlXFZB.CreateElement("ram:CompleteNumber");
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_TelexCommunication_CompleteNumber.InnerText = Convert.ToString(dsFzbMessage.Tables[11].Rows[0]["AP_TelexCommunication"]);
                    //MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_TelexCommunication.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedParty_DefinedTradeContact_TelexCommunication_CompleteNumber);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ApplicableTransportCargoInsurance = xmlXFZB.CreateElement("ram:ApplicableTransportCargoInsurance");
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_ApplicableTransportCargoInsurance);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ApplicableTransportCargoInsurance_CoverageInsuranceParty = xmlXFZB.CreateElement("ram:CoverageInsuranceParty");
                    MasterConsignment_IncludedHouseConsignment_ApplicableTransportCargoInsurance.AppendChild(MasterConsignment_IncludedHouseConsignment_ApplicableTransportCargoInsurance_CoverageInsuranceParty);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ApplicableTransportCargoInsurance_CoverageInsuranceParty_Role = xmlXFZB.CreateElement("ram:Role");
                    MasterConsignment_IncludedHouseConsignment_ApplicableTransportCargoInsurance_CoverageInsuranceParty_Role.InnerText = Convert.ToString(dsFzbMessage.Tables[9].Rows[0]["CoverageInsuranceParty_Role"]);
                    MasterConsignment_IncludedHouseConsignment_ApplicableTransportCargoInsurance_CoverageInsuranceParty.AppendChild(MasterConsignment_IncludedHouseConsignment_ApplicableTransportCargoInsurance_CoverageInsuranceParty_Role);

                    XmlElement MasterConsignment_IncludedHouseConsignment_OriginLocation = xmlXFZB.CreateElement("ram:OriginLocation");
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_OriginLocation);

                    XmlElement MasterConsignment_IncludedHouseConsignment_OriginLocation_ID = xmlXFZB.CreateElement("ram:ID");
                    MasterConsignment_IncludedHouseConsignment_OriginLocation_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["OriginCode"]);
                    MasterConsignment_IncludedHouseConsignment_OriginLocation.AppendChild(MasterConsignment_IncludedHouseConsignment_OriginLocation_ID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_OriginLocation_Name = xmlXFZB.CreateElement("ram:Name");
                    MasterConsignment_IncludedHouseConsignment_OriginLocation_Name.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["AirportName"]);
                    MasterConsignment_IncludedHouseConsignment_OriginLocation.AppendChild(MasterConsignment_IncludedHouseConsignment_OriginLocation_Name);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FinalDestinationLocation = xmlXFZB.CreateElement("ram:FinalDestinationLocation");
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_FinalDestinationLocation);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FinalDestinationLocation_ID = xmlXFZB.CreateElement("ram:ID");
                    MasterConsignment_IncludedHouseConsignment_FinalDestinationLocation_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["DestinationCode"]);
                    MasterConsignment_IncludedHouseConsignment_FinalDestinationLocation.AppendChild(MasterConsignment_IncludedHouseConsignment_FinalDestinationLocation_ID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_FinalDestinationLocation_Name = xmlXFZB.CreateElement("ram:Name");
                    MasterConsignment_IncludedHouseConsignment_FinalDestinationLocation_Name.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["DestinationAirport"]);
                    MasterConsignment_IncludedHouseConsignment_FinalDestinationLocation.AppendChild(MasterConsignment_IncludedHouseConsignment_FinalDestinationLocation_Name);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement = xmlXFZB.CreateElement("ram:SpecifiedLogisticsTransportMovement");
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_StageCode = xmlXFZB.CreateElement("ram:StageCode");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_StageCode.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["ModeOfTransport"]);
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_StageCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ModeCode = xmlXFZB.CreateElement("ram:ModeCode");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ModeCode.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["TransportModeCode"]);
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ModeCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_Mode = xmlXFZB.CreateElement("ram:Mode");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_Mode.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["AirTransportMode"]);
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_Mode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ID = xmlXFZB.CreateElement("ram:ID");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[5].Rows[0]["FltNumber"]);
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_SequenceNumeric = xmlXFZB.CreateElement("ram:SequenceNumeric");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_SequenceNumeric.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["SpecifiedLogsTransMov_SeqNumeric"]);
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_SequenceNumeric);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_UsedLogisticsTransportMeans = xmlXFZB.CreateElement("ram:UsedLogisticsTransportMeans");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_UsedLogisticsTransportMeans);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_UsedLogisticsTransportMeans_Name = xmlXFZB.CreateElement("ram:Name");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_UsedLogisticsTransportMeans_Name.InnerText = Convert.ToString(dsFzbMessage.Tables[5].Rows[0]["PartnerName"]);
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_UsedLogisticsTransportMeans.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_UsedLogisticsTransportMeans_Name);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent = xmlXFZB.CreateElement("ram:ArrivalEvent");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_ScheduledOccurrenceDateTime = xmlXFZB.CreateElement("ram:ScheduledOccurrenceDateTime");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_ScheduledOccurrenceDateTime.InnerText = Convert.ToString(dsFzbMessage.Tables[5].Rows[0]["FltDate"]);
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_ScheduledOccurrenceDateTime);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_OccurrenceArrivalLocation = xmlXFZB.CreateElement("ram:OccurrenceArrivalLocation");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_OccurrenceArrivalLocation);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_OccurrenceArrivalLocation_ID = xmlXFZB.CreateElement("ram:ID");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_OccurrenceArrivalLocation_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[5].Rows[0]["FltDestination"]);
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_OccurrenceArrivalLocation.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_OccurrenceArrivalLocation_ID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_OccurrenceArrivalLocation_Name = xmlXFZB.CreateElement("ram:Name");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_OccurrenceArrivalLocation_Name.InnerText = Convert.ToString(dsFzbMessage.Tables[5].Rows[0]["DestinationAirportName"]);
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_OccurrenceArrivalLocation.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_OccurrenceArrivalLocation_Name);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_OccurrenceArrivalLocation_TypeCode = xmlXFZB.CreateElement("ram:TypeCode");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_OccurrenceArrivalLocation_TypeCode.InnerText = Convert.ToString(dsFzbMessage.Tables[5].Rows[0]["TypeCode"]);
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_OccurrenceArrivalLocation.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_ArrivalEvent_OccurrenceArrivalLocation_TypeCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent = xmlXFZB.CreateElement("ram:DepartureEvent");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_ScheduledOccurrenceDateTime = xmlXFZB.CreateElement("ram:ScheduledOccurrenceDateTime");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_ScheduledOccurrenceDateTime.InnerText = Convert.ToString(dsFzbMessage.Tables[5].Rows[0]["FltDate"]);
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_ScheduledOccurrenceDateTime);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_OccurrenceArrivalLocation = xmlXFZB.CreateElement("ram:OccurrenceDepartureLocation");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_OccurrenceArrivalLocation);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_OccurrenceArrivalLocation_ID = xmlXFZB.CreateElement("ram:ID");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_OccurrenceArrivalLocation_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[5].Rows[0]["FltOrigin"]);
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_OccurrenceArrivalLocation.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_OccurrenceArrivalLocation_ID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_OccurrenceArrivalLocation_Name = xmlXFZB.CreateElement("ram:Name");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_OccurrenceArrivalLocation_Name.InnerText = Convert.ToString(dsFzbMessage.Tables[5].Rows[0]["OriginAirportName"]);
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_OccurrenceArrivalLocation.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_OccurrenceArrivalLocation_Name);

                    XmlElement MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_OccurrenceArrivalLocation_TypeCode = xmlXFZB.CreateElement("ram:TypeCode");
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_OccurrenceArrivalLocation_TypeCode.InnerText = Convert.ToString(dsFzbMessage.Tables[5].Rows[0]["TypeCode"]);
                    MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_OccurrenceArrivalLocation.AppendChild(MasterConsignment_IncludedHouseConsignment_SpecifiedLogisticsTransportMovement_DepartureEvent_OccurrenceArrivalLocation_TypeCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment = xmlXFZB.CreateElement("ram:UtilizedLogisticsTransportEquipment");
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment);

                    XmlElement MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment_ID = xmlXFZB.CreateElement("ram:ID");
                    MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[6].Rows[0]["VehicleNo"]);
                    MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment.AppendChild(MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment_ID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment_CharacteristicCode = xmlXFZB.CreateElement("ram:CharacteristicCode");
                    MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment_CharacteristicCode.InnerText = Convert.ToString(dsFzbMessage.Tables[6].Rows[0]["VehType"]);
                    MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment.AppendChild(MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment_CharacteristicCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment_Characteristic = xmlXFZB.CreateElement("ram:Characteristic");
                    MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment_Characteristic.InnerText = Convert.ToString(dsFzbMessage.Tables[6].Rows[0]["VehicleCapacity"]);
                    MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment.AppendChild(MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment_Characteristic);


                    XmlElement MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment_AffixedLogisticsSeal = xmlXFZB.CreateElement("ram:AffixedLogisticsSeal");
                    MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment.AppendChild(MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment_AffixedLogisticsSeal);


                    XmlElement MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment_AffixedLogisticsSeal_ID = xmlXFZB.CreateElement("ram:ID");
                    MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment_AffixedLogisticsSeal_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["VehcialeSealNo"]);
                    MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment_AffixedLogisticsSeal.AppendChild(MasterConsignment_IncludedHouseConsignment_UtilizedLogisticsTransportEquipment_AffixedLogisticsSeal_ID);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_HandlingSPHInstructions = xmlXFZB.CreateElement("ram:HandlingSPHInstructions");
                    //MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_HandlingSPHInstructions);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_HandlingSPHInstructions_Description = xmlXFZB.CreateElement("ram:Description");
                    //MasterConsignment_IncludedHouseConsignment_HandlingSPHInstructions_Description.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["SHCDescription"]);
                    //MasterConsignment_IncludedHouseConsignment_HandlingSPHInstructions.AppendChild(MasterConsignment_IncludedHouseConsignment_HandlingSPHInstructions_Description);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_HandlingSPHInstructions_DescriptionCode = xmlXFZB.CreateElement("ram:DescriptionCode");
                    //MasterConsignment_IncludedHouseConsignment_HandlingSPHInstructions_DescriptionCode.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["SHCCodes"]);
                    //MasterConsignment_IncludedHouseConsignment_HandlingSPHInstructions.AppendChild(MasterConsignment_IncludedHouseConsignment_HandlingSPHInstructions_DescriptionCode);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_HandlingSSRInstructions = xmlXFZB.CreateElement("ram:HandlingSSRInstructions");
                    //MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_HandlingSSRInstructions);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_HandlingSSRInstructions_Description = xmlXFZB.CreateElement("ram:Description");
                    //MasterConsignment_IncludedHouseConsignment_HandlingSSRInstructions_Description.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["SHCCodes"]);
                    //MasterConsignment_IncludedHouseConsignment_HandlingSSRInstructions.AppendChild(MasterConsignment_IncludedHouseConsignment_HandlingSSRInstructions_Description);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_HandlingSSRInstructions_DescriptionCode = xmlXFZB.CreateElement("ram:DescriptionCode");
                    //MasterConsignment_IncludedHouseConsignment_HandlingSSRInstructions_DescriptionCode.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["HandlingInfo"]);
                    //MasterConsignment_IncludedHouseConsignment_HandlingSSRInstructions.AppendChild(MasterConsignment_IncludedHouseConsignment_HandlingSSRInstructions_DescriptionCode);


                    //XmlElement MasterConsignment_IncludedHouseConsignment_HandlingOSIInstructions = xmlXFZB.CreateElement("ram:HandlingOSIInstructions");
                    //MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_HandlingOSIInstructions);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_HandlingOSIInstructions_Description = xmlXFZB.CreateElement("ram:Description");
                    //MasterConsignment_IncludedHouseConsignment_HandlingOSIInstructions_Description.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["SHCCodes"]);
                    //MasterConsignment_IncludedHouseConsignment_HandlingOSIInstructions.AppendChild(MasterConsignment_IncludedHouseConsignment_HandlingOSIInstructions_Description);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_HandlingOSIInstructions_DescriptionCode = xmlXFZB.CreateElement("ram:DescriptionCode");
                    //MasterConsignment_IncludedHouseConsignment_HandlingOSIInstructions_DescriptionCode.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["HandlingInfo"]);
                    //MasterConsignment_IncludedHouseConsignment_HandlingOSIInstructions.AppendChild(MasterConsignment_IncludedHouseConsignment_HandlingOSIInstructions_DescriptionCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedAccountingNote = xmlXFZB.CreateElement("ram:IncludedAccountingNote");
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedAccountingNote);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedAccountingNote_ContentCode = xmlXFZB.CreateElement("ram:ContentCode");
                    MasterConsignment_IncludedHouseConsignment_IncludedAccountingNote_ContentCode.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["AccCode"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedAccountingNote.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedAccountingNote_ContentCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedAccountingNote_Content = xmlXFZB.CreateElement("ram:Content");
                    MasterConsignment_IncludedHouseConsignment_IncludedAccountingNote_Content.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["AccountInfo"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedAccountingNote.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedAccountingNote_Content);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote = xmlXFZB.CreateElement("ram:IncludedCustomsNote");
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_ContentCode = xmlXFZB.CreateElement("ram:ContentCode");
                    MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_ContentCode.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["IncludedCustomsNote_ContentCode"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_ContentCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_Content = xmlXFZB.CreateElement("ram:Content");
                    MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_Content.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["IncludedCustomsNote_Content"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_Content);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_SubjectCode = xmlXFZB.CreateElement("ram:SubjectCode");
                    MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_SubjectCode.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["IncludedCustomsNote_SubjectCode"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_SubjectCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_CountryID = xmlXFZB.CreateElement("ram:CountryID");
                    MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_CountryID.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["IncludedCustomsNote_Country"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_CountryID);


                    XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedReferenceDocument = xmlXFZB.CreateElement("ram:AssociatedReferenceDocument");
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedReferenceDocument);

                    XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedReferenceDocument_ID = xmlXFZB.CreateElement("ram:ID");
                    MasterConsignment_IncludedHouseConsignment_AssociatedReferenceDocument_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["AssociatedRefDocID_Master"]);
                    MasterConsignment_IncludedHouseConsignment_AssociatedReferenceDocument.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedReferenceDocument_ID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_IssueDateTime = xmlXFZB.CreateElement("ram:IssueDateTime");
                    MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_IssueDateTime.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["AssociatedRefDoc_IssueDateTime_Master"]);
                    MasterConsignment_IncludedHouseConsignment_AssociatedReferenceDocument.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_IssueDateTime);


                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_TypeCode = xmlXFZB.CreateElement("ram:TypeCode");
                    MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_TypeCode.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["AssociatedRefDoc_TypeCode_Master"]);
                    MasterConsignment_IncludedHouseConsignment_AssociatedReferenceDocument.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_TypeCode);


                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_Name = xmlXFZB.CreateElement("ram:Name");
                    MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_Name.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["AssociatedRefDocName_Master"]);
                    MasterConsignment_IncludedHouseConsignment_AssociatedReferenceDocument.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedCustomsNote_Name);


                    XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedConsignmentCustomsProcedure = xmlXFZB.CreateElement("ram:AssociatedConsignmentCustomsProcedure");
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedConsignmentCustomsProcedure);

                    XmlElement MasterConsignment_IncludedHouseConsignment_AssociatedConsignmentCustomsProcedure_GoodsStatusCode = xmlXFZB.CreateElement("ram:GoodsStatusCode");
                    MasterConsignment_IncludedHouseConsignment_AssociatedConsignmentCustomsProcedure_GoodsStatusCode.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["SCI"]);
                    MasterConsignment_IncludedHouseConsignment_AssociatedConsignmentCustomsProcedure.AppendChild(MasterConsignment_IncludedHouseConsignment_AssociatedConsignmentCustomsProcedure_GoodsStatusCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ApplicableOriginCurrencyExchange = xmlXFZB.CreateElement("ram:ApplicableOriginCurrencyExchange");
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_ApplicableOriginCurrencyExchange);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ApplicableOriginCurrencyExchange_SourceCurrencyCode = xmlXFZB.CreateElement("ram:SourceCurrencyCode");
                    MasterConsignment_IncludedHouseConsignment_ApplicableOriginCurrencyExchange_SourceCurrencyCode.InnerText = Convert.ToString(dsFzbMessage.Tables[7].Rows[0]["Currency"]);
                    MasterConsignment_IncludedHouseConsignment_ApplicableOriginCurrencyExchange.AppendChild(MasterConsignment_IncludedHouseConsignment_ApplicableOriginCurrencyExchange_SourceCurrencyCode);


                    XmlElement MasterConsignment_IncludedHouseConsignment_ApplicableDestinationCurrencyExchange = xmlXFZB.CreateElement("ram:ApplicableDestinationCurrencyExchange");
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_ApplicableDestinationCurrencyExchange);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ApplicableDestinationCurrencyExchange_TargetCurrencyCode = xmlXFZB.CreateElement("ram:TargetCurrencyCode");
                    MasterConsignment_IncludedHouseConsignment_ApplicableDestinationCurrencyExchange_TargetCurrencyCode.InnerText = Convert.ToString(dsFzbMessage.Tables[7].Rows[0]["BaseCurrency"]);
                    MasterConsignment_IncludedHouseConsignment_ApplicableDestinationCurrencyExchange.AppendChild(MasterConsignment_IncludedHouseConsignment_ApplicableDestinationCurrencyExchange_TargetCurrencyCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ApplicableDestinationCurrencyExchange_MarketID = xmlXFZB.CreateElement("ram:MarketID");
                    MasterConsignment_IncludedHouseConsignment_ApplicableDestinationCurrencyExchange_MarketID.InnerText = Convert.ToString(dsFzbMessage.Tables[7].Rows[0]["ConvRateQualifier"]);
                    MasterConsignment_IncludedHouseConsignment_ApplicableDestinationCurrencyExchange.AppendChild(MasterConsignment_IncludedHouseConsignment_ApplicableDestinationCurrencyExchange_MarketID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ApplicableDestinationCurrencyExchange_ConversionRate = xmlXFZB.CreateElement("ram:ConversionRate");
                    MasterConsignment_IncludedHouseConsignment_ApplicableDestinationCurrencyExchange_ConversionRate.InnerText = Convert.ToString(dsFzbMessage.Tables[7].Rows[0]["ConvFactor"]);
                    MasterConsignment_IncludedHouseConsignment_ApplicableDestinationCurrencyExchange.AppendChild(MasterConsignment_IncludedHouseConsignment_ApplicableDestinationCurrencyExchange_ConversionRate);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsServiceCharge = xmlXFZB.CreateElement("ram:ApplicableLogisticsServiceCharge");
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsServiceCharge);

                    XmlElement MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsServiceCharge_ServiceTypeCode = xmlXFZB.CreateElement("ram:ServiceTypeCode");
                    //MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsServiceCharge_ServiceTypeCode.InnerText = Convert.ToString(dsFzbMessage.Tables[7].Rows[0]["ShipmentType"]);
                    MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsServiceCharge_ServiceTypeCode.InnerText = "";
                    MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsServiceCharge.AppendChild(MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsServiceCharge_ServiceTypeCode);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge = xmlXFZB.CreateElement("ram:ApplicableLogisticsAllowanceCharge");
                    //MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge_ID = xmlXFZB.CreateElement("ram:ID");
                    //MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[7].Rows[0]["ID"]);
                    //MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge.AppendChild(MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge_ID);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge_Reason = xmlXFZB.CreateElement("ram:Reason");
                    //MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge_Reason.InnerText = Convert.ToString(dsFzbMessage.Tables[7].Rows[0]["Reason"]);
                    //MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge.AppendChild(MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge_Reason);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge_ActualAmount = xmlXFZB.CreateElement("ram:ActualAmount");
                    //MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge_ActualAmount.SetAttribute("currencyID", Convert.ToString(dsFzbMessage.Tables[7].Rows[0]["ActualAmount_currencyID"]));
                    //MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge_ActualAmount.InnerText = Convert.ToString(dsFzbMessage.Tables[7].Rows[0]["ActualAmount_Text"]);
                    //MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge.AppendChild(MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge_ActualAmount);

                    //XmlElement MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge_PartyTypeCode = xmlXFZB.CreateElement("ram:PartyTypeCode");
                    //MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge_PartyTypeCode.InnerText = Convert.ToString(dsFzbMessage.Tables[7].Rows[0]["PartyTypeCode"]);
                    //MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge.AppendChild(MasterConsignment_IncludedHouseConsignment_ApplicableLogisticsAllowanceCharge_PartyTypeCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem = xmlXFZB.CreateElement("ram:IncludedHouseConsignmentItem");
                    MasterConsignment_IncludedHouseConsignment.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_SequenceNumeric = xmlXFZB.CreateElement("ram:SequenceNumeric");
                    //MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_SequenceNumeric.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["SequenceNumeric"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_SequenceNumeric.InnerText = "1";
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_SequenceNumeric);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TypeCode = xmlXFZB.CreateElement("ram:TypeCode");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TypeCode.SetAttribute("listAgencyID", Convert.ToString(dsFzbMessage.Tables[7].Rows[0]["TypeCode_listAgencyID"]));
                    //MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TypeCode.InnerText = Convert.ToString(dsFzbMessage.Tables[7].Rows[0]["CommodityCode"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TypeCode);


                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_GrossWeightMeasure = xmlXFZB.CreateElement("ram:GrossWeightMeasure");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_GrossWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["UOM"]));
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_GrossWeightMeasure.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["HAWBWt"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_GrossWeightMeasure);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_GrossVolumeMeasure = xmlXFZB.CreateElement("ram:GrossVolumeMeasure");
                    //MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_GrossVolumeMeasure.SetAttribute("unitCode", Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["UOM"]));
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_GrossVolumeMeasure.InnerText = Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["HAWBWt"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_GrossVolumeMeasure);


                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TotalChargeAmount = xmlXFZB.CreateElement("ram:TotalChargeAmount");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TotalChargeAmount.SetAttribute("currencyID", Convert.ToString(dsFzbMessage.Tables[7].Rows[0]["Currency"]));
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TotalChargeAmount.InnerText = Convert.ToString(dsFzbMessage.Tables[7].Rows[0]["Total"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TotalChargeAmount);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_PackageQuantity = xmlXFZB.CreateElement("ram:PackageQuantity");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_PackageQuantity.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["HousePackageQuantity"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_PackageQuantity);


                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_PieceQuantity = xmlXFZB.CreateElement("ram:PieceQuantity");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_PieceQuantity.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["HousePieces"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_PieceQuantity);


                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_VolumetricFactor = xmlXFZB.CreateElement("ram:VolumetricFactor");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_VolumetricFactor.InnerText = Convert.ToString(dsFzbMessage.Tables[8].Rows[0]["VolumetricFactor"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_VolumetricFactor);


                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_Information = xmlXFZB.CreateElement("ram:Information");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_Information.InnerText = Convert.ToString(dsFzbMessage.Tables[8].Rows[0]["DimUOM"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_Information);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_NatureIdentificationTransportCargo = xmlXFZB.CreateElement("ram:NatureIdentificationTransportCargo");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_NatureIdentificationTransportCargo);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_NatureIdentificationTransportCargo_Identification = xmlXFZB.CreateElement("ram:Identification");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_NatureIdentificationTransportCargo_Identification.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["Description"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_NatureIdentificationTransportCargo.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_NatureIdentificationTransportCargo_Identification);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_OriginCountry = xmlXFZB.CreateElement("ram:OriginCountry");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_OriginCountry);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_OriginCountry_ID = xmlXFZB.CreateElement("ram:ID");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_OriginCountry_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[3].Rows[0]["ShipperCountry"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_OriginCountry.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_OriginCountry_ID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment = xmlXFZB.CreateElement("ram:AssociatedUnitLoadTransportEquipment");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_ID = xmlXFZB.CreateElement("ram:ID");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[8].Rows[0]["ULDSNo"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_ID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_TareWeightMeasure = xmlXFZB.CreateElement("ram:TareWeightMeasure");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_TareWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["UOM"]));
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_TareWeightMeasure.InnerText = Convert.ToString(dsFzbMessage.Tables[10].Rows[0]["TareWeight"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_TareWeightMeasure);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_LoadedPackageQuantity = xmlXFZB.CreateElement("ram:LoadedPackageQuantity");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_LoadedPackageQuantity.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["HousePackageQuantity"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_LoadedPackageQuantity);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_CharacteristicCode = xmlXFZB.CreateElement("ram:CharacteristicCode");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_CharacteristicCode.InnerText = Convert.ToString(dsFzbMessage.Tables[8].Rows[0]["ULDType"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_CharacteristicCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_OperatingParty = xmlXFZB.CreateElement("ram:OperatingParty");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_OperatingParty);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_OperatingParty_PrimaryID = xmlXFZB.CreateElement("ram:PrimaryID");
                    //MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_OperatingParty_PrimaryID.SetAttribute("schemeAgencyID", Convert.ToString(dsFzbMessage.Tables[10].Rows[0]["ULDSNo"]));
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_OperatingParty_PrimaryID.SetAttribute("schemeAgencyID", "1");
                    //MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_OperatingParty_PrimaryID.InnerText = Convert.ToString(dsFzbMessage.Tables[5].Rows[0]["PartnerCode"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_OperatingParty_PrimaryID.InnerText = "1";
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_OperatingParty.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedUnitLoadTransportEquipment_OperatingParty_PrimaryID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage = xmlXFZB.CreateElement("ram:TransportLogisticsPackage");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_ItemQuantity = xmlXFZB.CreateElement("ram:ItemQuantity");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_ItemQuantity.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["HousePieces"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_ItemQuantity);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_GrossWeightMeasure = xmlXFZB.CreateElement("ram:GrossWeightMeasure");
                    //MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_GrossWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["UOM"]));
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_GrossWeightMeasure.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["HouseGrossWeight"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_GrossWeightMeasure);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension = xmlXFZB.CreateElement("ram:LinearSpatialDimension");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension_WidthMeasure = xmlXFZB.CreateElement("ram:WidthMeasure");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension_WidthMeasure.SetAttribute("unitCode", Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["WidthMeasure_unitCode"]));
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension_WidthMeasure.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["WidthMeasure_Text"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension_WidthMeasure);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension_LengthMeasure = xmlXFZB.CreateElement("ram:LengthMeasure");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension_LengthMeasure.SetAttribute("unitCode", Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["LengthMeasure_unitCode"]));
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension_LengthMeasure.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["LengthMeasure_Text"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension_LengthMeasure);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension_HeightMeasure = xmlXFZB.CreateElement("ram:HeightMeasure");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension_HeightMeasure.SetAttribute("unitCode", Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["HeightMeasure_unitCode"]));
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension_HeightMeasure.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["HeightMeasure_Text"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_TransportLogisticsPackage_LinearSpatialDimension_HeightMeasure);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge = xmlXFZB.CreateElement("ram:ApplicableFreightRateServiceCharge");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge);


                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_CategoryCode = xmlXFZB.CreateElement("ram:CategoryCode");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_CategoryCode.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["CategoryCode"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_CategoryCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_CommodityItemID = xmlXFZB.CreateElement("ram:CommodityItemID");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_CommodityItemID.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["CommodityItemIDDescription"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_CommodityItemID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_ChargeableWeightMeasure = xmlXFZB.CreateElement("ram:ChargeableWeightMeasure");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_ChargeableWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["UOM"]));
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_ChargeableWeightMeasure.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["HouseGrossWeight"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_ChargeableWeightMeasure);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_AppliedRate = xmlXFZB.CreateElement("ram:AppliedRate");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_AppliedRate.InnerText = Convert.ToString(dsFzbMessage.Tables[1].Rows[0]["AppliedRate"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_AppliedRate);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_AppliedAmount = xmlXFZB.CreateElement("ram:AppliedAmount");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_AppliedAmount.SetAttribute("currencyID", Convert.ToString(dsFzbMessage.Tables[2].Rows[0]["DVCarriageCurrency"]));
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_AppliedAmount.InnerText = Convert.ToString(dsFzbMessage.Tables[9].Rows[0]["AppliedAmount"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_ApplicableFreightRateServiceCharge_AppliedAmount);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument = xmlXFZB.CreateElement("ram:AssociatedReferenceDocument");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument);


                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument_ID = xmlXFZB.CreateElement("ram:ID");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[12].Rows[0]["AssociatedReferenceDocumentID"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument_ID);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument_IssueDateTime = xmlXFZB.CreateElement("ram:IssueDateTime");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument_IssueDateTime.InnerText = Convert.ToString(dsFzbMessage.Tables[12].Rows[0]["AssociatedReferenceDocument_IssueDateTime"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument_IssueDateTime);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument_TypeCode = xmlXFZB.CreateElement("ram:TypeCode");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument_TypeCode.InnerText = Convert.ToString(dsFzbMessage.Tables[12].Rows[0]["AssociatedReferenceDocument_TypeCode"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument_TypeCode);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument_Name = xmlXFZB.CreateElement("ram:Name");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument_Name.InnerText = Convert.ToString(dsFzbMessage.Tables[12].Rows[0]["AssociatedReferenceDocumentName"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_AssociatedReferenceDocument_Name);

                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_SpecifiedRateCombinationPointLocation = xmlXFZB.CreateElement("ram:SpecifiedRateCombinationPointLocation");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_SpecifiedRateCombinationPointLocation);


                    XmlElement MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_SpecifiedRateCombinationPointLocation_ID = xmlXFZB.CreateElement("ram:ID");
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_SpecifiedRateCombinationPointLocation_ID.InnerText = Convert.ToString(dsFzbMessage.Tables[7].Rows[0]["SpecifiedRateCombinationPointLocation_ID"]);
                    MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_SpecifiedRateCombinationPointLocation.AppendChild(MasterConsignment_IncludedHouseConsignment_IncludedHouseConsignmentItem_SpecifiedRateCombinationPointLocation_ID);

                    #endregion


                    sbXFZBMessage = new StringBuilder(xmlXFZB.OuterXml);
                    sbXFZBMessage.Replace("<", "<ram:");
                    sbXFZBMessage.Replace("<ram:/", "</ram:");
                    sbXFZBMessage.Replace("<ram:![CDATA", "<![CDATA");
                    sbXFZBMessage.Replace("<ram:HouseWaybill", "<rsm:HouseWaybill");
                    sbXFZBMessage.Replace("</ram:HouseWaybill", "</rsm:HouseWaybill");
                    sbXFZBMessage.Replace("<ram:Waybill", "<rsm:Waybill");
                    sbXFZBMessage.Replace("</ram:Waybill", "</rsm:Waybill");
                    sbXFZBMessage.Replace("<ram:MessageHeaderDocument>", "<rsm:MessageHeaderDocument>");
                    sbXFZBMessage.Replace("</ram:MessageHeaderDocument>", "</rsm:MessageHeaderDocument>");
                    sbXFZBMessage.Replace("<ram:BusinessHeaderDocument>", "<rsm:BusinessHeaderDocument>");
                    sbXFZBMessage.Replace("</ram:BusinessHeaderDocument>", "</rsm:BusinessHeaderDocument>");
                    sbXFZBMessage.Replace("<ram:MasterConsignment>", "<rsm:MasterConsignment>");
                    sbXFZBMessage.Replace("</ram:MasterConsignment>", "</rsm:MasterConsignment>");
                    sbXFZBMessage.Replace("schemaLocation", "xsi:schemaLocation");
                    ////Replacing duplicate Nodes
                    //sbXFZBMessage.Replace("ram:MasterConsignment_OriginLocation", "ram:OriginLocation");
                    //sbXFZBMessage.Replace("ram:MasterConsignment_FinalDestinationLocation", "ram:FinalDestinationLocation");
                    //sbXFZBMessage.Replace("ram:IncludedHouseConsignment_IncludedTareGrossWeightMeasure", "ram:IncludedTareGrossWeightMeasure");
                    //sbXFZBMessage.Replace("ram:ConsignorParty_PostalStructuredAddress", "ram:PostalStructuredAddress");
                    //sbXFZBMessage.Replace("ram:ConsigneeParty_PostalStructuredAddress", "ram:PostalStructuredAddress");
                    //sbXFZBMessage.Replace("ram:FreightForwarderParty_PostalStructuredAddress", "ram:PostalStructuredAddress");
                    //sbXFZBMessage.Replace("ram:AssociatedParty_PostalStructuredAddress", "ram:PostalStructuredAddress");

                    ////Append Header to XML
                    //StringBuilder xmlHeader = new StringBuilder();
                    //xmlHeader.Append(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"");
                    ////strXMLHeader.Append("xmlns:rsm=\"iata:housewaybill:1\"");
                    //xmlHeader.Append(" xmlns:ccts=\"urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2\"");
                    //xmlHeader.Append(" xmlns:udt=\"urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8\"");
                    //xmlHeader.Append(" xmlns:ram=\"iata:datamodel:3\"");
                    //xmlHeader.Append(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
                    //xmlHeader.Append(" xsi:schemaLocation=\"iata:housewaybill:1 HouseWaybill_1.xsd\"");

                    //sbXFZBMessage.Insert(17, xmlHeader);

                    /////Remove the empty tags from XML
                    var document = System.Xml.Linq.XDocument.Parse(sbXFZBMessage.ToString());
                    //var emptyNodes = document.Descendants().Where(e => e.IsEmpty || String.IsNullOrWhiteSpace(e.Value));
                    //foreach (var emptyNode in emptyNodes.ToArray())
                    //{
                    //    emptyNode.Remove();
                    //}
                    sbXFZBMessage = new StringBuilder(document.ToString());

                    //sbXFZBMessage.Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
                    //dsFzbMessage.Dispose();

                }
                else
                {
                    sbXFZBMessage.Append("No Message format available in the system.");
                }

            }
            catch (Exception ex)
            {
                sbXFZBMessage.Append("Error Occured while generating: " + ex.Message);
                //clsLog.WriteLogAzure("Error on Generate XFWB Message Method:" + ex.ToString());
                _logger.LogError("Error on Generate XFZB Message Method:" + ex.ToString());

            }


            return sbXFZBMessage.ToString();


        }


        private async Task<DataSet?> GetRecordforHAWBToGenerateXFZBMessage(string awbPrefix, string awbNumber, string hawbNumber)
        {
            DataSet ?dsFwb = new DataSet();
            try
            {
                //SQLServer da = new SQLServer();
                //string[] paramname = new string[] { "AWBPrefix", "AWBNumber", "HAWBNumber" };
                //object[] paramvalue = new object[] { awbPrefix, awbNumber, hawbNumber };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                SqlParameter[] sqlParams = [
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = awbPrefix },
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbNumber },
                    new SqlParameter("@HAWBNumber", SqlDbType.VarChar) { Value = hawbNumber }
                    ];

                //dsFwb = da.SelectRecords("Messaging.GetRecordMakeXFZBMessage", paramname, paramvalue, paramtype);
               dsFwb = await _readWriteDao.SelectRecords("Messaging.GetRecordMakeXFZBMessage", sqlParams);
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure("Error on Get Record for AWB ToGenerate XFZBMessage Method:" + ex.ToString());
                _logger.LogError("Error on Get Record for AWB ToGenerate XFZBMessage Method:" + ex.ToString());
            }
            return dsFwb;
        }
        #endregion
    }
}
