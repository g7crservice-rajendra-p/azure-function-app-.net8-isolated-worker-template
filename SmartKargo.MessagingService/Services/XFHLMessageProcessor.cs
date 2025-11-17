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
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
using static QidWorkerRole.MessageData;
using static QidWorkerRole.SCMExceptionHandlingWorkRole;


namespace QidWorkerRole
{
    public class XFHLMessageProcessor
    {
        //SCMExceptionHandlingWorkRole scmException = new SCMExceptionHandlingWorkRole();

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<FDMMessageProcessor> _logger;
        private GenericFunction _genericFunction;
        public XFHLMessageProcessor(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<FDMMessageProcessor> logger,
            GenericFunction genericFunction
        )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
        }

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
            try
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }
        #endregion
        #endregion


        #region validateAndInsertFHLData
        //public async Task<bool> validateAndInsertFHLData(ref MessageData.fhlinfo fhl, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.customsextrainfo[] customextrainfo, int REFNo, string strMessage, string strMessageFrom, string strFromID, string strStatus)
        public async Task<(bool success, MessageData.fhlinfo fhl, MessageData.consignmnetinfo[] consinfo, MessageData.customsextrainfo[] customextrainfo)> validateAndInsertFHLData(MessageData.fhlinfo fhl, MessageData.consignmnetinfo[] consinfo, MessageData.customsextrainfo[] customextrainfo, int REFNo, string strMessage, string strMessageFrom, string strFromID, string strStatus)
        {
            bool flag = false;

            //GenericFunction _genericFunction = new GenericFunction();
            AWBOperations? objOpsAuditLog = null;
            try
            {
                bool isAWBPresent = false;
                string AWBNum = fhl.awbnum;
                string AWBPrefix = fhl.airlineprefix;

                //SQLServer db = new SQLServer();

                await _genericFunction.UpdateInboxFromMessageParameter(REFNo, AWBPrefix + "-" + AWBNum, string.Empty, string.Empty, string.Empty, "FHL", strMessageFrom, DateTime.Parse("1900-01-01"));

                #region Check AWB Present or Not

                DataSet? ds = new DataSet();

                //string[] pname = new string[] { "AWBNumber" };
                //object[] values = new object[] { AWBNum };
                //SqlDbType[] ptype = new SqlDbType[] { SqlDbType.VarChar };

                SqlParameter[] sqlParameters = new SqlParameter[] {
                     new("@AWBNumber", SqlDbType.VarChar) { Value = AWBNum },
                };

                //ds = db.SelectRecords("sp_getawbdetails", pname, values, ptype);
                ds = await _readWriteDao.SelectRecords("sp_getawbdetails", sqlParameters);

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
                    return (false, fhl, consinfo, customextrainfo);
                    //return false;

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
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref ex);
                flag = false;
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            //return flag;
            return (flag, fhl, consinfo, customextrainfo);
        }
        #endregion


        #region HAWB Details Save
        public async Task<bool> PutHAWBDetails(string MAWBNo, string HAWBNo, int HAWBPcs, float HAWBWt, string Description, string CustID, string CustName,
            string CustAddress, string CustCity, string Zipcode, string Origin, string Destination, string SHC,
            string HAWBPrefix, string AWBPrefix, string FltOrigin, string FltDest, string ArrivalStatus, string FlightNo,
            string FlightDt, string ConsigneeName, string ConsigneeAddress, string ConsigneeCity, string ConsigneeState, string ConsigneeCountry, string ConsigneePostalCode,
            string CustState, string CustCountry, string UOM, string SLAC, string ConsigneeID, string ShipperEmail, string ShipperTelephone, string ConsigneeEmail, string ConsigneeTelephone)
        {
            DataSet? ds = new DataSet();

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
            try
            {

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@MAWBNo", SqlDbType.VarChar) { Value = MAWBNo },
                    new SqlParameter("@HAWBNo", SqlDbType.VarChar) { Value = HAWBNo },
                    new SqlParameter("@HAWBPcs", SqlDbType.Int) { Value = HAWBPcs },
                    new SqlParameter("@HAWBWt", SqlDbType.Float) { Value = HAWBWt },
                    new SqlParameter("@Description", SqlDbType.VarChar) { Value = Description },
                    new SqlParameter("@CustID", SqlDbType.VarChar) { Value = CustID },
                    new SqlParameter("@CustName", SqlDbType.VarChar) { Value = CustName },
                    new SqlParameter("@CustAddress", SqlDbType.VarChar) { Value = CustAddress },
                    new SqlParameter("@CustCity", SqlDbType.VarChar) { Value = CustCity },
                    new SqlParameter("@Zipcode", SqlDbType.VarChar) { Value = Zipcode },
                    new SqlParameter("@Origin", SqlDbType.VarChar) { Value = Origin },
                    new SqlParameter("@Destination", SqlDbType.VarChar) { Value = Destination },
                    new SqlParameter("@SHC", SqlDbType.VarChar) { Value = SHC },
                    new SqlParameter("@HAWBPrefix", SqlDbType.VarChar) { Value = HAWBPrefix },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                    new SqlParameter("@ArrivalStatus", SqlDbType.VarChar) { Value = ArrivalStatus },
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@FlightDt", SqlDbType.DateTime) { Value = string.IsNullOrEmpty(FlightDt) ? DateTime.Now : DateTime.Parse(FlightDt) },
                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = FltOrigin },
                    new SqlParameter("@flightDest", SqlDbType.VarChar) { Value = FltDest },
                    new SqlParameter("@ConsigneeName", SqlDbType.VarChar) { Value = ConsigneeName },
                    new SqlParameter("@ConsigneeAddress", SqlDbType.VarChar) { Value = ConsigneeAddress },
                    new SqlParameter("@ConsigneeCity", SqlDbType.VarChar) { Value = ConsigneeCity },
                    new SqlParameter("@ConsigneeState", SqlDbType.VarChar) { Value = ConsigneeState },
                    new SqlParameter("@ConsigneeCountry", SqlDbType.VarChar) { Value = ConsigneeCountry },
                    new SqlParameter("@ConsigneePostalCode", SqlDbType.VarChar) { Value = ConsigneePostalCode },
                    new SqlParameter("@CustState", SqlDbType.VarChar) { Value = CustState },
                    new SqlParameter("@CustCountry", SqlDbType.VarChar) { Value = CustCountry },
                    new SqlParameter("@UOM", SqlDbType.VarChar) { Value = UOM },
                    new SqlParameter("@SLAC", SqlDbType.Int) { Value = string.IsNullOrEmpty(SLAC) ? "0" : SLAC },
                    new SqlParameter("@ConsigneeID", SqlDbType.VarChar) { Value = ConsigneeID },
                    new SqlParameter("@ShipperEmail", SqlDbType.VarChar) { Value = ShipperEmail },
                    new SqlParameter("@ShipperTelephone", SqlDbType.VarChar) { Value = ShipperTelephone },
                    new SqlParameter("@ConsigneeEmail", SqlDbType.VarChar) { Value = ConsigneeEmail },
                    new SqlParameter("@ConsigneeTelephone", SqlDbType.VarChar) { Value = ConsigneeTelephone }
                };

                //if (da.ExecuteProcedure("SP_PutHAWBDetails_V2", paramname, paramtype, paramvalue))
                if (await _readWriteDao.ExecuteNonQueryAsync("SP_PutHAWBDetails_V2", parameters))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }
        #endregion

    }
}
