#region XFBL Message Processor Class Description
/* XFBL Message Processor Class Description.
      * Company       :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright     :   Copyright © 2017 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose       : 
      * Created By    :   Yoginath
      * Created On    :   03-08-2017
      * Approved By   :
      * Approved Date :
      * Modified By   :  
      * Modified On   :   
      * Description   :   
     */
#endregion
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Configuration;
using System.Data;

namespace QidWorkerRole
{
    public class XFBLMessageProcessor
    {
        #region :: Variable Declaration ::
        string unloadingportsequence = string.Empty;
        string uldsequencenum = string.Empty;
        string awbref = string.Empty;
        static string strConnection = Convert.ToString(ConfigurationManager.ConnectionStrings["ConStr"]);
        const string PAGE_NAME = "XFBLMessageProcessor";
        SCMExceptionHandlingWorkRole scm = new SCMExceptionHandlingWorkRole();
        #endregion

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<XFBLMessageProcessor> _logger;
        private readonly GenericFunction _genericFunction;

        #region Constructor
        public XFBLMessageProcessor(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<XFBLMessageProcessor> logger,
            GenericFunction genericFunction)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
        }
        #endregion
        #region :: Private Methods ::
        private void DecodeConsigmentDetails(DataSet fblXmlDataSet, ref MessageData.consignmnetinfo[] consinfo, ref string awbprefix, ref string awbnumber, ref MessageData.dimensionnfo[] dimensioinfo, ref MessageData.unloadingport[] unloadingport, ref MessageData.fblinfo fbldata, ref MessageData.ULDinfo[] uld, ref MessageData.consignmentorigininfo[] consorginfo)
        {
            DataRow[] drs;
            string[] awbnumberprefix;
            string movementprioritycode = string.Empty;
            for (int row = 0; row < fblXmlDataSet.Tables["AssociatedTransportCargo"].Rows.Count; row++)
            {
                MessageData.consignmnetinfo consig = new MessageData.consignmnetinfo("");
                drs = fblXmlDataSet.Tables["TransportContractDocument"].Select("IncludedMasterConsignment_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    awbnumberprefix = Convert.ToString(drs[0]["ID"]).Split('-');
                    consig.airlineprefix = awbnumberprefix[0];
                    consig.awbnum = awbnumberprefix[1];
                }

                drs = fblXmlDataSet.Tables["OriginLocation"].Select("IncludedMasterConsignment_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    consig.origin = Convert.ToString(drs[0]["ID"]);
                    //Convert.ToString(drs[0]["Name"]);
                }

                drs = fblXmlDataSet.Tables["FinalDestinationLocation"].Select("IncludedMasterConsignment_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    consig.dest = Convert.ToString(drs[0]["ID"]);
                    //Convert.ToString(drs[0]["Name"]);
                }

                drs = fblXmlDataSet.Tables["IncludedMasterConsignment"].Select("IncludedMasterConsignment_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    consig.densityindicator = "DG";
                    consig.densitygrp = Convert.ToString(drs[0]["DensityGroupCode"]);
                    consig.pcscnt = Convert.ToString(drs[0]["TotalPieceQuantity"]);
                    consig.manifestdesc = Convert.ToString(drs[0]["SummaryDescription"]);
                    consig.consigntype = Convert.ToString(drs[0]["TransportSplitDescription"]);
                    movementprioritycode = Convert.ToString(drs[0]["MovementPriorityCode"]);
                }

                drs = fblXmlDataSet.Tables["GrossWeightMeasure"].Select("IncludedMasterConsignment_Id=" + Convert.ToString(row));
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

                //if (consig.consigntype.Equals("T"))
                //{//total pieces
                consig.numshp = consig.pcscnt;
                //}

                drs = fblXmlDataSet.Tables["HandlingInstructions"].Select("IncludedMasterConsignment_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    consig.splhandling = Convert.ToString(drs[0]["Description"]);
                    consig.shpdesccode = Convert.ToString(drs[0]["DescriptionCode"]);
                }

                drs = fblXmlDataSet.Tables["GrossVolumeMeasure"].Select("IncludedMasterConsignment_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    consig.volumecode = Convert.ToString(drs[0]["unitCode"]);
                    consig.volumeamt = Convert.ToString(drs[0]["GrossVolumeMeasure_Text"]);
                }

                if (unloadingportsequence.Length > 0)
                    consig.portsequence = unloadingportsequence;
                if (uldsequencenum.Length > 0)
                    consig.uldsequence = uldsequencenum;

                awbprefix = consig.airlineprefix;
                awbnumber = consig.awbnum;
                Array.Resize(ref consinfo, consinfo.Length + 1);
                consinfo[consinfo.Length - 1] = consig;
                awbref = consinfo.Length.ToString();

                //point of unloading                
                MessageData.unloadingport unloading = new MessageData.unloadingport("");
                drs = fblXmlDataSet.Tables["OccurrenceArrivalLocation"].Select("ArrivalEvent_Id=0");
                if (drs.Length > 0)
                {
                    unloading.unloadingairport = Convert.ToString(drs[0]["ID"]);
                    //Convert.ToString(drs[0]["Name"]);
                    //Convert.ToString(drs[0]["TypeCode"]); 
                }

                drs = fblXmlDataSet.Tables["AssociatedTransportCargo"].Select("AssociatedTransportCargo_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    unloading.nilcargocode = Convert.ToString(drs[0]["TypeCode"]);
                }

                Array.Resize(ref unloadingport, unloadingport.Length + 1);
                unloadingport[unloadingport.Length - 1] = unloading;

                // Dimendion info               
                MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");

                drs = fblXmlDataSet.Tables["TransportLogisticsPackage_GrossWeightMeasure"].Select("TransportLogisticsPackage_Id=0");
                if (drs.Length > 0)
                {
                    dimension.weightcode = drs[0].Table.Columns.Contains("unitCode") ? Convert.ToString(drs[0]["unitCode"]).Equals("KGM") ? "K" : "L" : "K";
                    dimension.weight = drs[0].Table.Columns.Contains("TransportLogisticsPackage_GrossWeightMeasure_Text") ? Convert.ToString(drs[0]["TransportLogisticsPackage_GrossWeightMeasure_Text"]) : "0";
                }

                drs = fblXmlDataSet.Tables["WidthMeasure"].Select("LinearSpatialDimension_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    dimension.mesurunitcode = drs[0].Table.Columns.Contains("unitCode") ? Convert.ToString(drs[0]["unitCode"]) : "";
                    dimension.width = drs[0].Table.Columns.Contains("WidthMeasure_Text") ? Convert.ToString(drs[0]["WidthMeasure_Text"]) : "";
                }
                drs = fblXmlDataSet.Tables["LengthMeasure"].Select("LinearSpatialDimension_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    dimension.mesurunitcode = drs[0].Table.Columns.Contains("unitCode") ? Convert.ToString(drs[0]["unitCode"]) : "";
                    dimension.length = drs[0].Table.Columns.Contains("LengthMeasure_Text") ? Convert.ToString(drs[0]["LengthMeasure_Text"]) : "";
                }
                drs = fblXmlDataSet.Tables["HeightMeasure"].Select("LinearSpatialDimension_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    dimension.mesurunitcode = drs[0].Table.Columns.Contains("unitCode") ? Convert.ToString(drs[0]["unitCode"]) : "";
                    dimension.height = drs[0].Table.Columns.Contains("HeightMeasure_Text") ? Convert.ToString(drs[0]["HeightMeasure_Text"]) : "";
                }

                drs = fblXmlDataSet.Tables["TransportLogisticsPackage"].Select("TransportLogisticsPackage_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    dimension.piecenum = drs[0].Table.Columns.Contains("ItemQuantity") ? Convert.ToString(drs[0]["ItemQuantity"]) : "0";
                }

                dimension.AWBPrefix = awbprefix;
                dimension.AWBNumber = awbnumber;
                Array.Resize(ref dimensioinfo, dimensioinfo.Length + 1);
                dimensioinfo[dimensioinfo.Length - 1] = dimension;

                // ULD Specification
                fbldata.noofuld = "1";

                Array.Resize(ref uld, uld.Length + 1);

                MessageData.ULDinfo ulddata = new MessageData.ULDinfo("");
                drs = fblXmlDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Select("UtilizedUnitLoadTransportEquipment_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    ulddata.uldsrno = drs[0].Table.Columns.Contains("ID") ? Convert.ToString(drs[0]["ID"]) : "";
                    //Convert.ToString(drs[0]["LoadedPackageQuantity"]);
                    ulddata.uldtype = drs[0].Table.Columns.Contains("CharacteristicCode") ? Convert.ToString(drs[0]["CharacteristicCode"]) : "";
                    ulddata.uldloadingindicator = drs[0].Table.Columns.Contains("OperationalStatusCode") ? Convert.ToString(drs[0]["OperationalStatusCode"]) : "";
                }
                if (fblXmlDataSet.Tables.Contains("OperatingParty_PrimaryID"))
                {
                    drs = fblXmlDataSet.Tables["OperatingParty_PrimaryID"].Select("OperatingParty_Id=" + Convert.ToString(row));
                    if (drs.Length > 0)
                    {
                        //Convert.ToString(drs[0]["schemeAgencyID"]);
                        ulddata.uldowner = drs[0].Table.Columns.Contains("OperatingParty_PrimaryID_Text") ? Convert.ToString(drs[0]["OperatingParty_PrimaryID_Text"]) : "";
                    }
                }
                ulddata.uldno = ulddata.uldtype + ulddata.uldsrno + ulddata.uldowner;
                drs = fblXmlDataSet.Tables["UtilizedUnitLoadTransportEquipment_GrossWeightMeasure"].Select("UtilizedUnitLoadTransportEquipment_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    ulddata.uldweightcode = drs[0].Table.Columns.Contains("unitCode") ? Convert.ToString(drs[0]["unitCode"]) : "";
                    ulddata.uldweight = drs[0].Table.Columns.Contains("UtilizedUnitLoadTransportEquipment_GrossWeightMeasure_Text") ? Convert.ToString(drs[0]["UtilizedUnitLoadTransportEquipment_GrossWeightMeasure_Text"]) : "";
                }

                ulddata.AWBPrefix = awbprefix;
                ulddata.AWBNumber = awbnumber;
                uld[row] = ulddata;

                MessageData.consignmentorigininfo consorg = new MessageData.consignmentorigininfo();
                drs = fblXmlDataSet.Tables["ForwardingAgentParty"].Select("PreCarriageTransportMovement_Id=0");
                if (drs.Length > 0)
                {
                    consorg.abbrivatedname = drs[0].Table.Columns.Contains("Name") ? Convert.ToString(drs[0]["Name"]) : "";
                }
                drs = fblXmlDataSet.Tables["PreCarriageTransportMovement"].Select("PreCarriageTransportMovement_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    consorg.carriercode = drs[0].Table.Columns.Contains("ID") ? Convert.ToString(drs[0]["ID"]).Substring(0, 2) : "";
                    consorg.flightnum = drs[0].Table.Columns.Contains("ID") ? Convert.ToString(drs[0]["ID"]) : "";
                }
                drs = fblXmlDataSet.Tables["PreCarriageEvent"].Select("PreCarriageTransportMovement_Id=" + Convert.ToString(row));
                if (drs.Length > 0)
                {
                    if (drs[0].Table.Columns.Contains("ID"))
                    {
                        string[] fltDatesplit = Convert.ToString(drs[0]["ArrivalOccurrenceDateTime"]).Split('T');
                        consorg.day = Convert.ToString(fltDatesplit[0]);
                        consorg.month = Convert.ToDateTime(fltDatesplit[0]).ToString("MMM");
                    }
                    //Convert.ToString(drs[0]["ArrivalOccurrenceDateTime"]);
                    //Convert.ToString(drs[0]["ArrivalDateTimeTypeCode"]);
                }

                consorg.airportcode = "";
                consorg.movementprioritycode = movementprioritycode;

                Array.Resize(ref consorginfo, consorginfo.Length + 1);
                consorginfo[consorginfo.Length - 1] = consorg;
            }

        }

        private string[] StringSplitter(string str)
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
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                strarr = null;
            }
            return strarr;
        }

        private void ReProcessConsigment(ref MessageData.consignmnetinfo[] FFMConsig, ref MessageData.consignmnetinfo[] consinfo)
        {
            try
            {
                bool AWBMatch = false;
                Array.Resize(ref consinfo, consinfo.Length + 1);
                Array.Copy(FFMConsig, consinfo, 1);
                for (int i = 1; i < FFMConsig.Length; i++)
                {
                    AWBMatch = false;
                    for (int j = 0; j < consinfo.Length; j++)
                    {
                        if (consinfo[j].awbnum.Equals(FFMConsig[i].awbnum) && consinfo[j].origin.Equals(FFMConsig[i].origin) && consinfo[j].dest.Equals(FFMConsig[i].dest))
                        {
                            AWBMatch = true;
                            consinfo[j].weight = Convert.ToString((Convert.ToDecimal(consinfo[j].weight) + Convert.ToDecimal(FFMConsig[i].weight)));
                            consinfo[j].pcscnt = Convert.ToString((Convert.ToDecimal(consinfo[j].pcscnt) + Convert.ToDecimal(FFMConsig[i].pcscnt)));
                            //consinfo[j].numshp = (Convert.ToDecimal(consinfo[j].numshp) + Convert.ToDecimal(FFMConsig[i].numshp)).ToString();

                        }
                    }
                    if (!AWBMatch)
                    {
                        Array.Resize(ref consinfo, consinfo.Length + 1);
                        Array.Copy(FFMConsig, i, consinfo, consinfo.Length - 1, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                //  //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private async Task<DataSet?> GenertateAWBDimensions(string AWBNumber, int AWBPieces, DataSet Dimensions, decimal AWBWt, string UserName, DateTime TimeStamp, bool IsCreate, string AWBPrefix)
        {
            //SQLServer da = new SQLServer();
            DataSet? ds = null;
            try
            {
                System.Text.StringBuilder strDimensions = new System.Text.StringBuilder();

                if (Dimensions != null && Dimensions.Tables.Count > 0 && Dimensions.Tables[0].Rows.Count > 0)
                {
                    for (int intCount = 0; intCount < Dimensions.Tables[0].Rows.Count; intCount++)
                    {
                        strDimensions.Append("Insert into #tblPieceInfo(PieceNo, IdentificationNo, Length, Breath, Height, Vol, Wt, Units, PieceType, BagNo, ULDNo, Location, FlightNo, FlightDate) values (");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["PieceNo"]);
                        strDimensions.Append(",'");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["IdentificationNo"]);
                        strDimensions.Append("',");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Length"]);
                        strDimensions.Append(",");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Breath"]);
                        strDimensions.Append(",");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Height"]);
                        strDimensions.Append(",");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Vol"]);
                        strDimensions.Append(",");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Wt"]);
                        strDimensions.Append(",'");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Units"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["PieceType"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["BagNo"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["ULDNo"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Location"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["FlightNo"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["FlightDate"]);
                        strDimensions.Append("'); ");
                    }
                }

                //string[] PName = new string[] { "AWBNumber", "Pieces", "PieceInfo", "UserName", "TimeStamp", "IsCreate", "AWBWeight", "AWBPrefix" };
                //object[] PValue = new object[] { AWBNumber, AWBPieces, strDimensions.ToString(), UserName, TimeStamp, IsCreate, AWBWt, AWBPrefix };
                //SqlDbType[] PType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Bit, SqlDbType.Decimal, SqlDbType.VarChar };
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@AWBNumber", AWBNumber),
                    new SqlParameter("@Pieces", AWBPieces),
                    new SqlParameter("@PieceInfo", strDimensions.ToString()),
                    new SqlParameter("@UserName", UserName),
                    new SqlParameter("@TimeStamp", TimeStamp),
                    new SqlParameter("@IsCreate", IsCreate),
                    new SqlParameter("@AWBWeight", AWBWt),
                    new SqlParameter("@AWBPrefix", AWBPrefix)
                };
                //ds = da.SelectRecords("sp_StoreCourierDetails", PName, PValue, PType);
                ds = await _readWriteDao.SelectRecords("sp_StoreCourierDetails", sqlParameters);
                //PName = null;
                //PType = null;
                //PValue = null;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                ds = null;
            }
            finally
            {
                //da = null;
            }
            return ds;
        }
        #endregion Private Methods

        /*Not in use*/
        #region :: Public Methods ::
        //public void GenerateAutoFBLMessage()
        //{
        //    try
        //    {
        //        SQLServer db = new SQLServer(); ;
        //        DataSet ds = null;
        //        bool flag = false;
        //        do
        //        {
        //            flag = false;
        //            ds = db.SelectRecords("Messaging.uspGetFlightsForFBL");
        //            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        //            {
        //                flag = true;
        //                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //                {
        //                    DataRow dr = ds.Tables[0].Rows[i];
        //                    FBRMessageProcessor Fbr = new FBRMessageProcessor();
        //                    Fbr.GenerateFBLMessage(dr["Source"].ToString(), dr["Dest"].ToString(), dr["FlightID"].ToString(), dr["Date"].ToString());
        //                }


        //            }
        //        } while (flag);
        //        db = null;
        //        GC.Collect();
        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //    }
        //}

        //public void SaveandUpdagteFBLMessageinDatabase(ref MessageData.fblinfo fbldata, ref MessageData.unloadingport[] unloadingport, ref MessageData.dimensionnfo[] objDimension, ref MessageData.ULDinfo[] uld, ref MessageData.otherserviceinfo[] othinfoarray, ref MessageData.consignmentorigininfo[] consigmnentOrigin, ref MessageData.consignmnetinfo[] consinfo, int RefNo, string strMessage, string strmessageFrom, string strFromID, string strStatus)
        public async Task<(MessageData.fblinfo fbldata, MessageData.unloadingport[] unloadingport, MessageData.dimensionnfo[] objDimension, MessageData.ULDinfo[] uld, MessageData.otherserviceinfo[] othinfoarray, MessageData.consignmentorigininfo[] consigmnentOrigin)> SaveandUpdagteFBLMessageinDatabase(MessageData.fblinfo fbldata, MessageData.unloadingport[] unloadingport, MessageData.dimensionnfo[] objDimension, MessageData.ULDinfo[] uld, MessageData.otherserviceinfo[] othinfoarray, MessageData.consignmentorigininfo[] consigmnentOrigin, MessageData.consignmnetinfo[] consinfo, int RefNo, string strMessage, string strmessageFrom, string strFromID, string strStatus)
        {

            try
            {
                //SQLServer dtb = new SQLServer();
                //GenericFunction gf = new GenericFunction();

                //AuditLog log = new AuditLog();
                string flightnum = fbldata.carriercode + fbldata.fltnum;
                DateTime flightdate = new DateTime();
                DateTime date = new DateTime();
                //flightdate = DateTime.Parse(DateTime.Parse("1." + fbldata.month + " 2008").Month.ToString().PadLeft(2, '0') + "/" + fbldata.date.PadLeft(2, '0') + "/" + System.DateTime.Today.Year);
                //date = DateTime.Parse(DateTime.Parse("1." + fbldata.month + " 2008").Month.ToString().PadLeft(2, '0') + "/" + fbldata.date.PadLeft(2, '0') + "/" + System.DateTime.Today.Year);
                flightdate = DateTime.Parse(fbldata.date);

                string source = string.Empty, dest = string.Empty;
                //string[] PName = new string[] { "flightnum", "date" };
                //SqlDbType[] PType = new SqlDbType[] { SqlDbType.NVarChar, SqlDbType.VarChar };
                //object[] PValue = new object[] { flightnum, flightdate };
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@flightnum", flightnum),
                    new SqlParameter("@date", flightdate)
                };
                //DataSet ds = dtb.SelectRecords("spGetDestCodeForFFM", PName, PValue, PType);
                DataSet? ds = await _readWriteDao.SelectRecords("spGetDestCodeForFFM", sqlParameters);
                if (ds != null)
                {
                    if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        source = ds.Tables[0].Rows[0]["source"].ToString();
                        dest = ds.Tables[0].Rows[0]["Dest"].ToString();

                    }
                }
                if (source.Length < 1)
                    source = fbldata.fltairportcode;//ffmdata.fltairportcode;


                _genericFunction.UpdateInboxFromMessageParameter(RefNo, string.Empty, flightnum, source, dest, "XFBL", strmessageFrom == "" ? strFromID : strmessageFrom, flightdate);

                #region Reprocess the Consigment Info--commented for GHA logic
                for (int k = 0; k < unloadingport.Length; k++)
                {
                    dest = unloadingport[k].unloadingairport;
                    //Reprocess ConsigmentInfo
                    MessageData.consignmnetinfo[] FFMConsig = new MessageData.consignmnetinfo[consinfo.Length];
                    Array.Copy(consinfo, FFMConsig, consinfo.Length);
                    consinfo = new MessageData.consignmnetinfo[0];
                    ReProcessConsigment(ref FFMConsig, ref consinfo);
                }
                #endregion


                if (consinfo.Length > 0)
                {
                    for (int i = 0; i < consinfo.Length; i++)
                    {
                        #region Add AWB details
                        string AWBNum = consinfo[i].awbnum;
                        string AWBPrefix = consinfo[i].airlineprefix;

                        bool isAWBPresent = false;
                        DataSet dsCheck = new DataSet();

                        //string[] pcname = new string[] { "AWBnumber", "AWBPrefix", "RefNo", "MessageType" };
                        //object[] pcvalues = new object[] { AWBNum, AWBPrefix, RefNo, "XFBL" };
                        //SqlDbType[] pctypes = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar };

                        sqlParameters = new SqlParameter[]
                        {
                            new SqlParameter("@AWBnumber", AWBNum),
                            new SqlParameter("@AWBPrefix", AWBPrefix),
                            new SqlParameter("@RefNo", RefNo),
                            new SqlParameter("@MessageType", "XFBL")
                        };

                        //DataSet dscheck = dtb.SelectRecords("spCheckStatusofAWB", pcname, pcvalues, pctypes);

                        DataSet? dscheck = await _readWriteDao.SelectRecords("spCheckStatusofAWB", sqlParameters);
                        if (dscheck != null && dscheck.Tables != null && dscheck.Tables.Count > 0 && dscheck.Tables[0].Rows.Count > 0)
                        {
                            if (dscheck.Tables[0].Columns.Count == 1 && dscheck.Tables[0].Rows[0]["Status"].ToString().ToUpper() == "FALSE")
                            {
                                //inBox = new InBox();
                                //inBox.Subject = "XFBL";
                                //inBox.Body = strMessage;
                                //inBox.FromiD = string.Empty;
                                //inBox.ToiD = string.Empty;
                                //inBox.RecievedOn = DateTime.UtcNow;
                                //inBox.IsProcessed = true;
                                //inBox.Status = strStatus;
                                //inBox.FromiD = strFromID;
                                //inBox.Type = "XFBL";
                                //inBox.UpdatedBy = strmessageFrom == "" ? strFromID : strmessageFrom;
                                //inBox.UpdatedOn = DateTime.UtcNow;
                                //inBox.AWBNumber = string.Empty;
                                //inBox.FlightNumber = flightnum;
                                //inBox.FlightOrigin = source;
                                //inBox.FlightDestination = dest;
                                //inBox.FlightDate = flightdate;
                                //inBox.MessageCategory = "CIMP";
                                //inBox.Error = "Invalid AWBNo";
                                //log.SaveLog(LogType.InMessage, string.Empty, string.Empty, inBox);
                                continue;
                            }

                            if (dscheck.Tables[0].Rows[0]["Status"].ToString().ToUpper() == "TRUE" && dscheck.Tables[0].Rows[0]["AWBStatus"].ToString().ToUpper() != "B")
                                continue;


                        }
                        //inBox = new InBox();
                        //inBox.Subject = "XFBL";
                        //inBox.Body = strMessage;
                        //inBox.FromiD = string.Empty;
                        //inBox.ToiD = string.Empty;
                        //inBox.RecievedOn = DateTime.UtcNow;
                        //inBox.IsProcessed = true;
                        //inBox.Status = strStatus;
                        //inBox.FromiD = strFromID;
                        //inBox.Type = "XFBL";
                        //inBox.UpdatedBy = strmessageFrom == "" ? strFromID : strmessageFrom;
                        //inBox.UpdatedOn = DateTime.UtcNow;
                        //inBox.AWBNumber = string.Empty;
                        //inBox.FlightNumber = flightnum;
                        //inBox.FlightOrigin = source;
                        //inBox.FlightDestination = dest;
                        //inBox.FlightDate = flightdate;
                        //inBox.MessageCategory = "CIMP";
                        //log.SaveLog(LogType.InMessage, string.Empty, string.Empty, inBox);

                        //string[] pname = new string[] { "AWBNumber", "AWBPrefix" };
                        //object[] values = new object[] { AWBNum, AWBPrefix };
                        //SqlDbType[] ptype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
                        //dsCheck = dtb.SelectRecords("sp_getawbdetails", pname, values, ptype);

                        //if (dsCheck != null)
                        //{
                        //    if (dsCheck.Tables.Count > 0)
                        //    {
                        //        if (dsCheck.Tables[0].Rows.Count > 0)
                        //        {
                        //            if (dsCheck.Tables[0].Rows[0]["AWBNumber"].ToString().Equals(AWBNum, StringComparison.OrdinalIgnoreCase))
                        //            {
                        //                isAWBPresent = true;
                        //            }
                        //        }
                        //    }
                        //}

                        int row = 0;
                        if (objDimension.Length > 0 || uld.Length > 0)
                        {
                            DataSet? dsDimension = await GenertateAWBDimensions(AWBNum, Convert.ToInt16(consinfo[i].pcscnt), null, Convert.ToDecimal(consinfo[i].weight), "MSG", System.DateTime.Now, false, AWBPrefix);

                            for (int j = 0; j < objDimension.Length; j++)
                            {
                                if (objDimension[j].AWBPrefix == consinfo[i].airlineprefix && objDimension[j].AWBNumber == consinfo[i].awbnum)
                                {
                                    if (objDimension[j].mesurunitcode.Trim() != "")
                                    {
                                        if (objDimension[j].mesurunitcode.Trim().ToUpper() == "CMT")
                                        {
                                            objDimension[j].mesurunitcode = "Cms";
                                        }
                                        else if (objDimension[j].mesurunitcode.Trim().ToUpper() == "INH")
                                        {
                                            objDimension[j].mesurunitcode = "Inches";
                                        }
                                    }
                                    if (objDimension[j].length.Trim() == "")
                                    {
                                        objDimension[j].length = "0";
                                    }
                                    if (objDimension[j].width.Trim() == "")
                                    {
                                        objDimension[j].width = "0";
                                    }
                                    if (objDimension[j].height.Trim() == "")
                                    {
                                        objDimension[j].height = "0";
                                    }
                                    if (objDimension[j].piecenum.Trim() == "")
                                    {
                                        objDimension[j].piecenum = "0";
                                    }



                                    for (int cnt = row; cnt < (row + Convert.ToInt16(objDimension[j].piecenum)); cnt++)
                                    {
                                        dsDimension.Tables[0].Rows[cnt]["Length"] = Convert.ToInt16(objDimension[j].length);
                                        dsDimension.Tables[0].Rows[cnt]["Breath"] = Convert.ToInt16(objDimension[j].width);
                                        dsDimension.Tables[0].Rows[cnt]["Height"] = Convert.ToInt16(objDimension[j].height);
                                        dsDimension.Tables[0].Rows[cnt]["Units"] = objDimension[j].mesurunitcode;
                                    }
                                    row = row + Convert.ToInt16(objDimension[j].piecenum);

                                }
                            }
                            row = 0;
                            if (uld.Length > 0)
                            {
                                for (int u = 0; u < uld.Length; u++)
                                {
                                    if (uld[u].AWBNumber == AWBNum && uld[u].AWBPrefix == AWBPrefix && !string.IsNullOrEmpty(uld[u].uldno))
                                    {
                                        dsDimension.Tables[0].Rows[row]["ULDNo"] = uld[u].uldno;
                                        dsDimension.Tables[0].Rows[row]["PieceType"] = "ULD";

                                    }
                                }
                            }

                            await GenertateAWBDimensions(AWBNum, Convert.ToInt16(consinfo[i].pcscnt), dsDimension, Convert.ToDecimal(consinfo[i].weight), "XFBL", System.DateTime.Now, true, AWBPrefix);

                        }





                        //dtb = new SQLServer();
                        if (!isAWBPresent)
                        {

                            decimal VolumeWt = 0;
                            if (consinfo[i].volumecode != "" && Convert.ToDecimal(consinfo[i].volumeamt == "" ? "0" : consinfo[i].volumeamt) > 0)
                            {
                                switch (consinfo[i].volumecode.ToUpper())
                                {
                                    case "MC":
                                        VolumeWt = decimal.Parse(string.Format("{0:0.00}", Convert.ToDecimal(Convert.ToDecimal(consinfo[i].volumeamt == "" ? "0" : consinfo[i].volumeamt) * decimal.Parse("166.67"))));
                                        break;
                                    case "CI":
                                        VolumeWt =
                                            decimal.Parse(string.Format("{0:0.00}", Convert.ToDecimal((Convert.ToDecimal(consinfo[i].volumeamt == "" ? "0" : consinfo[i].volumeamt) /
                                                                                          decimal.Parse("366")))));
                                        break;
                                    case "CF":
                                        VolumeWt =
                                            decimal.Parse(string.Format("{0:0.00}",
                                                                        Convert.ToDecimal(Convert.ToDecimal(consinfo[i].volumeamt == "" ? "0" : consinfo[i].volumeamt) *
                                                                                          decimal.Parse("4.7194"))));
                                        break;
                                    case "CC":
                                        VolumeWt =
                                           decimal.Parse(string.Format("{0:0.00}",
                                                                       Convert.ToDecimal(((Convert.ToDecimal(consinfo[i].volumeamt == "" ? "0" : consinfo[i].volumeamt) /
                                                                                          decimal.Parse("6000"))))));
                                        break;
                                    default:
                                        VolumeWt = Convert.ToDecimal(Convert.ToDecimal(consinfo[i].volumeamt == "" ? "0" : consinfo[i].volumeamt));
                                        break;
                                }
                            }
                            decimal ChargeableWeight = 0;
                            if (VolumeWt > 0)
                            {
                                if (Convert.ToDecimal(consinfo[i].weight == "" ? "0" : consinfo[i].weight) > VolumeWt)
                                    ChargeableWeight = Convert.ToDecimal(consinfo[i].weight == "" ? "0" : consinfo[i].weight);
                                else
                                    ChargeableWeight = VolumeWt;
                            }
                            else
                                ChargeableWeight = Convert.ToDecimal(consinfo[i].weight == "" ? "0" : consinfo[i].weight);


                            //if origin is not present in XFBL message for consignmennt, then take flt origin present in message as origin by priyanka
                            consinfo[i].origin = consinfo[i].origin.Length < 3 ? fbldata.fltairportcode : consinfo[i].origin;





                            //string[] paramname = new string[] { "AirlinePrefix", "AWBNum", "Origin", "Dest", "PcsCount", "Weight", "Volume", "ComodityCode", "ComodityDesc", "CarrierCode", "FlightNum", "FlightDate", "FlightOrigin", "FlightDest", "ShipperName", "ShipperAddr", "ShipperPlace", "ShipperState", "ShipperCountryCode", "ShipperContactNo", "ConsName", "ConsAddr", "ConsPlace", "ConsState", "ConsCountryCode", "ConsContactNo", "CustAccNo", "IATACargoAgentCode", "CustName", "SystemDate", "MeasureUnit", "Length", "Breadth", "Height", "PartnerStatus", "REFNo", "UpdatedBy", "ChargeableWeight" };

                            //object[] paramvalue = new object[] { consinfo[i].airlineprefix,consinfo[i].awbnum,consinfo[i].origin,consinfo[i].dest,consinfo[i].pcscnt,consinfo[i].weight,consinfo[i].volumeamt ,"",consinfo[i].manifestdesc , "", flightnum, flightdate, source,dest,"",
                            //                             "","","", "","", "","", "","","","","", "", "", DateTime.Now.ToString("yyyy-MM-dd"),"", "", "", "", "" ,RefNo,"XFBL",ChargeableWeight};

                            //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime,
                            //                                  SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                            //                                  SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.Int,SqlDbType.VarChar,SqlDbType.Decimal };

                            SqlParameter[] parameters = new SqlParameter[]
                            {
                                new SqlParameter("@AirlinePrefix", SqlDbType.VarChar) { Value = consinfo[i].airlineprefix },
                                new SqlParameter("@AWBNum", SqlDbType.VarChar) { Value = consinfo[i].awbnum },
                                new SqlParameter("@Origin", SqlDbType.VarChar) { Value = consinfo[i].origin },
                                new SqlParameter("@Dest", SqlDbType.VarChar) { Value = consinfo[i].dest },
                                new SqlParameter("@PcsCount", SqlDbType.VarChar) { Value = consinfo[i].pcscnt },
                                new SqlParameter("@Weight", SqlDbType.VarChar) { Value = consinfo[i].weight },
                                new SqlParameter("@Volume", SqlDbType.VarChar) { Value = consinfo[i].volumeamt },
                                new SqlParameter("@ComodityCode", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ComodityDesc", SqlDbType.VarChar) { Value = consinfo[i].manifestdesc },
                                new SqlParameter("@CarrierCode", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@FlightNum", SqlDbType.VarChar) { Value = flightnum },
                                new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = flightdate },
                                new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = source },
                                new SqlParameter("@FlightDest", SqlDbType.VarChar) { Value = dest },
                                new SqlParameter("@ShipperName", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ShipperAddr", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ShipperPlace", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ShipperState", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ShipperCountryCode", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ShipperContactNo", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ConsName", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ConsAddr", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ConsPlace", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ConsState", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ConsCountryCode", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@ConsContactNo", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@CustAccNo", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@IATACargoAgentCode", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@CustName", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@SystemDate", SqlDbType.DateTime) { Value = DateTime.Now },
                                new SqlParameter("@MeasureUnit", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@Length", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@Breadth", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@Height", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@PartnerStatus", SqlDbType.VarChar) { Value = "" },
                                new SqlParameter("@REFNo", SqlDbType.Int) { Value = RefNo },
                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "XFBL" },
                                new SqlParameter("@ChargeableWeight", SqlDbType.Decimal) { Value = ChargeableWeight }
                            };

                            string procedure = "spInsertBookingDataFromFFR";
                            //if (!dtb.InsertData(procedure, paramname, paramtype, paramvalue))
                            if (!await _readWriteDao.ExecuteNonQueryAsync(procedure, parameters))
                                // clsLog.WriteLogAzure("Error in XFBL AWB Add Error for:" + consinfo[i].awbnum);
                                _logger.LogWarning("Error in XFBL AWB Add Error for: {0}", consinfo[i].awbnum);


                            // Audit log
                            //objOpsAuditLog = new AWBOperations();
                            //objOpsAuditLog.AWBID = 0;
                            //objOpsAuditLog.AWBPrefix = AWBPrefix.Trim();
                            //objOpsAuditLog.AWBNumber = AWBNum;
                            //objOpsAuditLog.Origin = consinfo[i].origin.ToUpper();
                            //objOpsAuditLog.Destination = consinfo[i].dest.ToUpper();
                            //objOpsAuditLog.FlightNo = flightnum;
                            //objOpsAuditLog.FlightDate = flightdate;
                            //objOpsAuditLog.FlightOrigin = source;
                            //objOpsAuditLog.FlightDestination = dest;
                            //objOpsAuditLog.BookedPcs = Convert.ToInt32(consinfo[i].pcscnt);
                            //objOpsAuditLog.BookedWgt = Convert.ToDouble(consinfo[i].weight);
                            //objOpsAuditLog.UOM = consinfo[i].weightcode;
                            //objOpsAuditLog.Createdon = DateTime.UtcNow;
                            //objOpsAuditLog.Updatedon = DateTime.UtcNow;
                            //objOpsAuditLog.Createdby = strmessageFrom;
                            //objOpsAuditLog.Updatedby = strmessageFrom;
                            //objOpsAuditLog.Action = "Booked";
                            //objOpsAuditLog.Message = "AWB Booked Through XFBL";
                            //objOpsAuditLog.Description = consinfo[i].manifestdesc;

                            //log = new AuditLog();
                            //log.SaveLog(LogType.AWBOperations, string.Empty, string.Empty, objOpsAuditLog);
                        }
                        #endregion

                        #region MakeAWBRoute through XFBL Message
                        //string[] paramnm = new string[] { "AWBNum", "AWBPrefix" };
                        //object[] paramobj = new object[] { AWBNum, AWBPrefix };
                        //SqlDbType[] paramtyp = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
                        SqlParameter[] sqlParams = new SqlParameter[]
                        {
                            new SqlParameter("@AWBNum", AWBNum),
                            new SqlParameter("@AWBPrefix", AWBPrefix)
                        };
                        //if (dtb.ExecuteProcedure("spDeleteAWBRouteFFR", paramnm, paramtyp, paramobj))
                        if (await _readWriteDao.ExecuteNonQueryAsync("spDeleteAWBRouteFFR", sqlParams))
                        {
                            MessageData.FltRoute[] fltroute = new MessageData.FltRoute[0];
                            MessageData.FltRoute flight = new MessageData.FltRoute("");
                            if (!(consinfo[i].origin == source && dest == consinfo[i].dest))
                            {
                                //   MakeAWBFlightRoute = true;
                                if (consinfo[i].origin.ToUpper() == source.ToUpper())
                                {

                                    flight.carriercode = flightnum.Substring(0, 2);
                                    flight.fltnum = flightnum;
                                    flight.date = flightdate.ToString("MM/dd/yyyy");
                                    flight.fltdept = consinfo[i].origin.ToUpper();
                                    flight.fltarrival = dest;

                                    Array.Resize(ref fltroute, fltroute.Length + 1);
                                    fltroute[fltroute.Length - 1] = flight;

                                    if (dest.ToUpper() != consinfo[i].dest.ToUpper())
                                    {
                                        flight.carriercode = string.Empty;
                                        flight.fltnum = string.Empty;
                                        flight.date = flightdate.ToString("MM/dd/yyyy");
                                        flight.fltdept = dest;
                                        flight.fltarrival = consinfo[i].dest.ToUpper();
                                        Array.Resize(ref fltroute, fltroute.Length + 1);
                                        fltroute[fltroute.Length - 1] = flight;
                                    }
                                }

                                if (consinfo[i].origin.ToUpper() != source.ToUpper())
                                {
                                    flight.carriercode = flightnum.Substring(0, 2);
                                    flight.fltnum = flightnum;
                                    flight.date = flightdate.ToString("MM/dd/yyyy");
                                    flight.fltdept = consinfo[i].origin.ToUpper();
                                    flight.fltarrival = source;
                                    Array.Resize(ref fltroute, fltroute.Length + 1);
                                    fltroute[fltroute.Length - 1] = flight;

                                    if (source.ToUpper() != consinfo[i].dest.ToUpper())
                                    {
                                        flight.carriercode = flightnum.Substring(0, 2);
                                        flight.fltnum = flightnum;
                                        flight.date = flightdate.ToString("MM/dd/yyyy");
                                        flight.fltdept = source;
                                        flight.fltarrival = dest;
                                        Array.Resize(ref fltroute, fltroute.Length + 1);
                                        fltroute[fltroute.Length - 1] = flight;
                                    }

                                    if (dest.ToUpper() != consinfo[i].dest.ToUpper())
                                    {
                                        flight.carriercode = string.Empty;
                                        flight.fltnum = string.Empty;
                                        flight.date = flightdate.ToString("MM/dd/yyyy");
                                        flight.fltdept = dest;
                                        flight.fltarrival = consinfo[i].dest.ToUpper();
                                        Array.Resize(ref fltroute, fltroute.Length + 1);
                                        fltroute[fltroute.Length - 1] = flight;
                                    }
                                }

                            }
                            else
                            {
                                if (consinfo[i].origin.ToUpper() == source.ToUpper() && dest.ToUpper() == consinfo[i].dest.ToUpper().ToUpper())
                                {
                                    flight.carriercode = flightnum.Substring(0, 2);
                                    flight.fltnum = flightnum;
                                    flight.date = flightdate.ToString("MM/dd/yyyy");
                                    flight.fltdept = consinfo[i].origin.ToUpper();
                                    flight.fltarrival = dest;
                                    Array.Resize(ref fltroute, fltroute.Length + 1);
                                    fltroute[fltroute.Length - 1] = flight;
                                }
                            }


                            if (fltroute.Length > 0)
                            {
                                for (int route = 0; route < fltroute.Length; route++)
                                {

                                    //dtb = new SQLServer();
                                    // string[] RName = new string[]
                                    //{
                                    //                           "AWBNumber", "FltOrigin",  "FltDestination",  "FltNumber", "FltDate",  "Status", "UpdatedBy", "UpdatedOn",
                                    //                            "IsFFR","REFNo", "date", "AWBPrefix"
                                    //};
                                    // SqlDbType[] RType = new SqlDbType[]
                                    //     {
                                    //                             SqlDbType.VarChar, SqlDbType.VarChar,  SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar,
                                    //                             SqlDbType.DateTime,  SqlDbType.Bit, SqlDbType.Int,  SqlDbType.DateTime, SqlDbType.VarChar
                                    //     };

                                    // object[] RValues = new object[]
                                    //     {
                                    //     AWBNum,fltroute[route].fltdept, fltroute[route].fltarrival,fltroute[route].fltnum, flightdate, "Q", "XFBL",DateTime.Now,1, 0, date,AWBPrefix
                                    //     };
                                    SqlParameter[] sqlPara = new SqlParameter[] {
                                        new SqlParameter("@AWBNumber",SqlDbType.VarChar) { Value = AWBNum },
                                        new SqlParameter("@FltOrigin",SqlDbType.VarChar) { Value = fltroute[route].fltdept },
                                        new SqlParameter("@FltDestination",SqlDbType.VarChar) { Value = fltroute[route].fltarrival },
                                        new SqlParameter("@FltNumber",SqlDbType.VarChar) { Value = fltroute[route].fltnum },
                                        new SqlParameter("@FltDate",SqlDbType.DateTime) { Value = flightdate },
                                        new SqlParameter("@Status",SqlDbType.VarChar) { Value = "Q" },
                                        new SqlParameter("@UpdatedBy",SqlDbType.VarChar) { Value = "XFBL" },
                                        new SqlParameter("@UpdatedOn",SqlDbType.DateTime) { Value = DateTime.Now },
                                        new SqlParameter("@IsFFR",SqlDbType.Bit) { Value = 1 },
                                        new SqlParameter("@REFNo",SqlDbType.Int) { Value = 0 },
                                        new SqlParameter("@date",SqlDbType.DateTime) { Value = date },
                                        new SqlParameter("@AWBPrefix",SqlDbType.VarChar) { Value = AWBPrefix }
                                    };

                                    //if (!dtb.UpdateData("spSaveFFRAWBRoute", RName, RType, RValues))
                                    if (!await _readWriteDao.ExecuteNonQueryAsync("spSaveFFRAWBRoute", sqlPara))
                                        //clsLog.WriteLogAzure("Error in Save AWB Route XFBL " + dtb.LastErrorDescription);
                                        // clsLog.WriteLogAzure("Error in Save AWB Route XFBL for:" );
                                        _logger.LogWarning("Error in Save AWB Route XFBL for:");

                                    #region Save AWBNo On Audit Log
                                    //string[] CNname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public" };
                                    //SqlDbType[] CType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit };
                                    //object[] CValues = new object[] { AWBPrefix, AWBNum, consinfo[i].origin, consinfo[i].dest, consinfo[i].pcscnt, consinfo[i].weight, flightnum, flightdate, fltroute[route].fltdept, fltroute[route].fltarrival, "Booked", "XFBL", "AWB Flight Information", "XFBL", DateTime.Today.ToString(), 1 };

                                    SqlParameter[] sqlParametersAWBP = new SqlParameter[]
                                    {
                                        new SqlParameter("@AWBPrefix", AWBPrefix),
                                        new SqlParameter("@AWBNumber", AWBNum),
                                        new SqlParameter("@Origin", consinfo[i].origin),
                                        new SqlParameter("@Destination", consinfo[i].dest),
                                        new SqlParameter("@Pieces", consinfo[i].pcscnt),
                                        new SqlParameter("@Weight", consinfo[i].weight),
                                        new SqlParameter("@FlightNo", flightnum),
                                        new SqlParameter("@FlightDate", flightdate),
                                        new SqlParameter("@FlightOrigin", fltroute[route].fltdept),
                                        new SqlParameter("@FlightDestination", fltroute[route].fltarrival),
                                        new SqlParameter("@Action", "Booked"),
                                        new SqlParameter("@Message", "XFBL"),
                                        new SqlParameter("@Description", "AWB Flight Information"),
                                        new SqlParameter("@UpdatedBy", "XFBL"),
                                        new SqlParameter("@UpdatedOn", DateTime.Today),
                                        new SqlParameter("@Public", 1)
                                    };
                                    //if (!dtb.ExecuteProcedure("SPAddAWBAuditLog", CNname, CType, CValues))
                                    if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", sqlParametersAWBP))
                                        //clsLog.WriteLog("AWB Audit log  for:" + AWBNum + Environment.NewLine + "Error: " + dtb.LastErrorDescription);
                                        // clsLog.WriteLogAzure("AWB Audit log  for:" + AWBNum);
                                        _logger.LogWarning("AWB Audit log  for: {0}", AWBNum);
                                    #endregion
                                }
                            }
                        }
                    }
                    #endregion
                }
                return (fbldata, unloadingport, objDimension, uld, othinfoarray, consigmnentOrigin);

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return (fbldata, unloadingport, objDimension, uld, othinfoarray, consigmnentOrigin); ;
            }

        }

        public bool DecodeReceiveFBLMessage(string fblmsg, ref MessageData.fblinfo fbldata, ref MessageData.unloadingport[] unloadingport, ref MessageData.dimensionnfo[] dimensioinfo, ref MessageData.ULDinfo[] uld, ref MessageData.otherserviceinfo[] othinfo, ref MessageData.consignmentorigininfo[] consorginfo, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.otherserviceinfo[] othinfoarray)
        {
            bool flag = false;
            try
            {
                string AWBPrefix = "", AWBNumber = "";
                var fblXmlDataSet = new DataSet();
                var tx = new StringReader(fblmsg);
                fblXmlDataSet.ReadXml(tx);

                fbldata.fblversion = Convert.ToString(fblXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"]);

                //flight data
                fbldata.messagesequencenum = Convert.ToString(fblXmlDataSet.Tables["LogisticsTransportMovement"].Rows[0]["SequenceNumeric"]);
                string frieghtbookedid = Convert.ToString(fblXmlDataSet.Tables["BusinessHeaderDocument"].Rows[0]["ID"]);
                string flightnumber = Convert.ToString(fblXmlDataSet.Tables["LogisticsTransportMovement"].Rows[0]["ID"]);
                fbldata.carriercode = flightnumber.Substring(0, 2);
                fbldata.fltnum = flightnumber.Substring(2);
                frieghtbookedid = frieghtbookedid.Replace(flightnumber, "");
                fbldata.fltairportcode = frieghtbookedid.Substring(8, 3);
                //fbldata.aircraftregistration = msg[4];
                frieghtbookedid = frieghtbookedid.Substring(0, 8);
                fbldata.date = frieghtbookedid.Substring(4, 2) + "/" + frieghtbookedid.Substring(6, 2) + "/" + frieghtbookedid.Substring(0, 4);
                //fbldata.month = msg[2].Substring(2);                   

                //onwards check consignment details
                DecodeConsigmentDetails(fblXmlDataSet, ref consinfo, ref AWBPrefix, ref AWBNumber, ref dimensioinfo, ref unloadingport, ref fbldata, ref uld, ref consorginfo);

                //Special Service request
                fbldata.specialservicereq1 = "";

                // Other service info
                Array.Resize(ref othinfoarray, othinfoarray.Length + 1);
                othinfoarray[othinfoarray.Length - 1].otherserviceinfo1 = "";
                othinfoarray[othinfoarray.Length - 1].consigref = awbref;
                // Last line
                flag = true;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;
        }
        #endregion Public Methods

    }
}
