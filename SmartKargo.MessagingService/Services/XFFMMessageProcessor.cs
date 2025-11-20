#region XFFM Message Processor Class Description
/* XFFM Message Processor Class Description.
      * Company              :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright            :   Copyright © 2017 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose              :   XFFM Message Processor Class
      * Created By           :   Yoginath
      * Created On           :   2017-06-20
      * Approved By          :
      * Approved Date        :
      * Modified By          :  
      * Modified On          :   
      * Description          :   
     */
#endregion
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using OpenPop.Mime;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using static Google.Protobuf.Reflection.FieldOptions.Types;

namespace QidWorkerRole
{
    public class XFFMMessageProcessor
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<XFFMMessageProcessor> _logger;
        private readonly GenericFunction _genericFunction;

        #region Constructor
        public XFFMMessageProcessor(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<XFFMMessageProcessor> logger, GenericFunction genericFunction)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
        }
        #endregion
        #region :: Variable Declaration ::
        static string unloadingportsequence = "";
        static string uldsequencenum = "";
        static string awbref = "";
        #endregion Variable Declaration



        #region :: Public Methods ::
        /// <summary>
        /// Decode XFFM message to get the AWB Consignment, Route etc. Information
        /// </summary>
        /// <param name="RefNo">Message SrNo from tblInbox</param>
        /// <param name="ffmmsg">XFFM Message</param>
        /// <param name="ffmdata">Array contains flight information</param>
        /// <param name="unloadingport">Array contains unloading port</param>
        /// <param name="consinfo">Array contains consignment inforamtion</param>
        /// <param name="dimensioinfo">Array contains AWB dimensions</param>
        /// <param name="uld">Array contains ULD details</param>
        /// <param name="othinfoarray">Array contains OSI information</param>
        /// <param name="custominfo">Array contains custom information</param>
        /// <param name="movementinfo">Array contains  flight movement information i.e. flight number, O&D and date etc.</param>
        /// <returns>Returns true if message is decoded successfully</returns>
        public bool DecodeReceiveFFMMessage(int RefNo, string ffmmsg, ref MessageData.ffminfo ffmdata, ref MessageData.unloadingport[] unloadingport, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.dimensionnfo[] dimensioinfo, ref MessageData.ULDinfo[] uld, ref MessageData.otherserviceinfo[] othinfoarray, ref MessageData.customsextrainfo[] custominfo, ref MessageData.movementinfo[] movementinfo)
        {
            bool flag = false;
            try
            {
                //string lastrec = "NA";
                uldsequencenum = "";
                unloadingportsequence = "";
                string AWBPrefix = "", AWBNumber = "";
                var ffmXmlDataSet = new DataSet();

                var tx = new StringReader(ffmmsg);
                ffmXmlDataSet.ReadXml(tx);
                //UploadMasters.UploadMasterCommon obj = new UploadMasters.UploadMasterCommon();
                //obj.ExportDataSet(ffmXmlDataSet, "E:\\");

                //Message Header Document
                ffmdata.endmesgcode = "";
                ffmdata.ffmversionnum = Convert.ToString(ffmXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"]);

                ffmdata.messagesequencenum = "";
                string fltac = Convert.ToString(ffmXmlDataSet.Tables["BusinessHeaderDocument"].Rows[0]["ID"]);
                string fltno = Convert.ToString(ffmXmlDataSet.Tables["LogisticsTransportMovement"].Rows[0]["ID"]);
                ffmdata.carriercode = fltno.Substring(0, 2);
                ffmdata.fltnum = fltno.Substring(2);// Convert.ToString(ffmXmlDataSet.Tables["LogisticsTransportMovement"].Rows[0]["ID"]);
                string sfltdate = Convert.ToString(ffmXmlDataSet.Tables["DepartureEvent"].Rows[0]["DepartureOccurrenceDateTime"]);
                string[] fltDatesplit = sfltdate.Split('T');
                ffmdata.fltdate = Convert.ToString(fltDatesplit[0]);
                ffmdata.month = Convert.ToDateTime(fltDatesplit[0]).ToString("MMM");
                ffmdata.time = Convert.ToString(fltDatesplit[1]);
                ffmdata.fltairportcode = fltac.Substring(fltac.Length - 3, 3);
                ffmdata.aircraftregistration = Convert.ToString(ffmXmlDataSet.Tables["RegistrationCountry"].Rows[0]["ID"]);

                //point of unloading
                MessageData.unloadingport unloading = new MessageData.unloadingport("");
                unloading.unloadingairport = Convert.ToString(ffmXmlDataSet.Tables["OccurrenceArrivalLocation"].Rows[0]["ID"]);
                //unloading.nilcargocode = Convert.ToString(ffmXmlDataSet.Tables["AssociatedTransportCargo"].Rows[0]["TypeCode"]);
                string unloadingdate = Convert.ToString(ffmXmlDataSet.Tables["ArrivalEvent"].Rows[0]["ArrivalOccurrenceDateTime"]);
                string[] unloadingDatesplit = unloadingdate.Split('T');
                unloading.day = Convert.ToString(Convert.ToDateTime(unloadingDatesplit[0]).Day);
                unloading.month = Convert.ToString(Convert.ToDateTime(unloadingDatesplit[0]).ToString("MMM"));
                unloading.time = Convert.ToString(Convert.ToDateTime(unloadingDatesplit[1]).ToString("HH:mm"));
                string loadingdate = Convert.ToString(ffmXmlDataSet.Tables["ArrivalEvent"].Rows[0]["DepartureOccurrenceDateTime"]);
                string[] loadingDatesplit = loadingdate.Split('T');
                unloading.day1 = Convert.ToString(Convert.ToDateTime(loadingDatesplit[0]).Day);
                unloading.month1 = Convert.ToString(Convert.ToDateTime(loadingDatesplit[0]).ToString("MMM"));
                unloading.time1 = Convert.ToString(Convert.ToDateTime(loadingDatesplit[1]).ToString("HH:mm"));

                Array.Resize(ref unloadingport, unloadingport.Length + 1);
                unloadingport[unloadingport.Length - 1] = unloading;
                //for sequence app
                unloadingport[unloadingport.Length - 1].sequencenum = unloadingport.Length.ToString();
                unloadingportsequence = unloadingport.Length.ToString();
                uldsequencenum = "";


                for (int i = 0; i < ffmXmlDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows.Count; i++)
                {
                    //ULD Specification
                    MessageData.ULDinfo ulddata = new MessageData.ULDinfo("");
                    DataRow[] drs;
                    drs = ffmXmlDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=" + Convert.ToString(i));
                    if (drs.Length > 0)
                    {
                        ulddata.uldsrno = Convert.ToString(drs[0]["schemeAgencyID"]);
                        ulddata.uldowner = Convert.ToString(drs[0]["PrimaryID_Text"]);
                    }
                    ulddata.uldtype = Convert.ToString(ffmXmlDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[i]["CharacteristicCode"]);
                    if (Convert.ToString(ffmXmlDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[i]["ID"]).Equals("BULK"))
                    {
                        ulddata.uldno = "BULK";
                    }
                    else
                    {
                        ulddata.uldno = ulddata.uldtype + Convert.ToString(ffmXmlDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[i]["ID"]) + ulddata.uldowner;
                    }
                    ulddata.uldloadingindicator = Convert.ToString(ffmXmlDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[i]["OperationalStatusCode"]);
                    ulddata.uldremark = Convert.ToString(ffmXmlDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[i]["LoadingRemark"]);

                    Array.Resize(ref uld, uld.Length + 1);
                    uld[uld.Length - 1] = ulddata;
                    if (int.Parse(unloadingportsequence) > 0)
                    {
                        uld[uld.Length - 1].portsequence = unloadingportsequence;
                        uld[uld.Length - 1].refuld = uld.Length.ToString();
                        uldsequencenum = uld.Length.ToString();
                    }


                    //onwards check consignment details
                    //DecodeConsigmentDetails(str[i], ref consinfo, ref AWBPrefix, ref AWBNumber);
                    for (int j = 0; j < ffmXmlDataSet.Tables["IncludedMasterConsignment"].Rows.Count; j++)
                    {
                        if (ffmXmlDataSet.Tables["IncludedMasterConsignment"].Rows[j]["AssociatedTransportCargo_Id"].Equals(ffmXmlDataSet.Tables["UtilizedUnitLoadTransportEquipment"].Rows[i]["AssociatedTransportCargo_Id"]))
                        {
                            MessageData.consignmnetinfo consig = new MessageData.consignmnetinfo("");
                            string awbNumberid = Convert.ToString(ffmXmlDataSet.Tables["TransportContractDocument"].Rows[j]["ID"]);
                            string[] decmes = awbNumberid.Split('-');
                            consig.airlineprefix = decmes[0];
                            consig.awbnum = decmes[1];
                            consig.origin = Convert.ToString(ffmXmlDataSet.Tables["OriginLocation"].Rows[j]["ID"]);
                            consig.dest = Convert.ToString(ffmXmlDataSet.Tables["FinalDestinationLocation"].Rows[j]["ID"]);
                            consig.consigntype = Convert.ToString(ffmXmlDataSet.Tables["IncludedMasterConsignment"].Rows[j]["TransportSplitDescription"]);
                            consig.pcscnt = Convert.ToString(ffmXmlDataSet.Tables["IncludedMasterConsignment"].Rows[j]["TotalPieceQuantity"]);
                            //DataRow[] drs;
                            drs = ffmXmlDataSet.Tables["GrossWeightMeasure"].Select("IncludedMasterConsignment_Id=" + Convert.ToString(j));
                            if (drs.Length > 0)
                            {
                                consig.weightcode = Convert.ToString(drs[0]["unitCode"]);
                                consig.weight = Convert.ToString(drs[0]["GrossWeightMeasure_Text"]);
                            }
                            //if (Convert.ToString(ffmXmlDataSet.Tables["IncludedMasterConsignment"].Rows[i]["TransportSplitDescription"]).Equals("T"))
                            //{
                            consig.numshp = consig.pcscnt;
                            consig.shpdesccode = Convert.ToString(ffmXmlDataSet.Tables["IncludedMasterConsignment"].Rows[j]["TransportSplitDescription"]);
                            //}
                            //consig.densityindicator =Convert.ToString(ffmXmlDataSet.Tables["IncludedMasterConsignment"].Rows[i][""]);
                            consig.densitygrp = Convert.ToString(ffmXmlDataSet.Tables["IncludedMasterConsignment"].Rows[j]["DensityGroupCode"]);
                            drs = ffmXmlDataSet.Tables["GrossVolumeMeasure"].Select("IncludedMasterConsignment_Id=" + Convert.ToString(i));
                            if (drs.Length > 0)
                            {
                                consig.volumecode = Convert.ToString(drs[0]["UnitCode"]);
                                consig.volumeamt = Convert.ToString(drs[0]["GrossVolumeMeasure_Text"]);
                            }
                            consig.manifestdesc = Convert.ToString(ffmXmlDataSet.Tables["IncludedMasterConsignment"].Rows[j]["SummaryDescription"]);
                            consig.splhandling = Convert.ToString(ffmXmlDataSet.Tables["HandlingSPHInstructions"].Rows[j]["Description"]);

                            if (unloadingportsequence.Length > 0)
                                consig.portsequence = unloadingportsequence;
                            if (uldsequencenum.Length > 0)
                                consig.uldsequence = uldsequencenum;
                            AWBPrefix = consig.airlineprefix;
                            AWBNumber = consig.awbnum;
                            Array.Resize(ref consinfo, consinfo.Length + 1);
                            consinfo[consinfo.Length - 1] = consig;
                            awbref = consinfo.Length.ToString();

                            // Dimension info
                            Array.Resize(ref dimensioinfo, dimensioinfo.Length + 1);
                            MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
                            drs = ffmXmlDataSet.Tables["GrossWeightMeasure"].Select("IncludedMasterConsignment_Id=" + Convert.ToString(j));
                            if (drs.Length > 0)
                            {
                                dimension.weightcode = Convert.ToString(drs[0]["unitCode"]);
                                dimension.weight = Convert.ToString(drs[0]["GrossWeightMeasure_Text"]);
                            }
                            dimension.mesurunitcode = Convert.ToString(ffmXmlDataSet.Tables["WidthMeasure"].Rows[j]["unitCode"]);
                            if (ffmXmlDataSet.Tables["LengthMeasure"].Columns.Contains("LengthMeasure_Text"))
                            {
                                dimension.length = Convert.ToString(ffmXmlDataSet.Tables["LengthMeasure"].Rows[j]["LengthMeasure_Text"]);
                            }
                            else
                            { dimension.length = string.Empty; }

                            if (ffmXmlDataSet.Tables["WidthMeasure"].Columns.Contains("WidthMeasure_Text"))
                            {
                                dimension.width = Convert.ToString(ffmXmlDataSet.Tables["WidthMeasure"].Rows[j]["WidthMeasure_Text"]);
                            }
                            else
                            { dimension.width = string.Empty; }
                            if (ffmXmlDataSet.Tables["HeightMeasure"].Columns.Contains("HeightMeasure_Text"))
                            {
                                dimension.height = Convert.ToString(ffmXmlDataSet.Tables["HeightMeasure"].Rows[j]["HeightMeasure_Text"]);
                            }
                            else
                            { dimension.height = string.Empty; }

                            dimension.piecenum = Convert.ToString(ffmXmlDataSet.Tables["TransportLogisticsPackage"].Rows[j]["ItemQuantity"]);
                            dimension.consigref = awbref;
                            dimension.AWBPrefix = AWBPrefix;
                            dimension.AWBNumber = AWBNumber;
                            dimensioinfo[i] = dimension;

                            //Other service info
                            Array.Resize(ref othinfoarray, othinfoarray.Length + 1);
                            othinfoarray[othinfoarray.Length - 1].otherserviceinfo1 = Convert.ToString(ffmXmlDataSet.Tables["HandlingOSIInstructions"].Rows[j]["Description"]);
                            othinfoarray[othinfoarray.Length - 1].consigref = awbref;

                            //COR
                            consinfo[int.Parse(awbref) - 1].customorigincode = Convert.ToString(ffmXmlDataSet.Tables["AssociatedConsignmentCustomsProcedure"].Rows[j]["GoodsStatusCode"]);

                            //custom extra info
                            MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
                            custom.IsoCountryCodeOci = Convert.ToString(ffmXmlDataSet.Tables["IncludedCustomsNote1"].Rows[j]["CountryID"]);
                            custom.InformationIdentifierOci = Convert.ToString(ffmXmlDataSet.Tables["IncludedCustomsNote1"].Rows[j]["SubjectCode"]);
                            custom.CsrIdentifierOci = Convert.ToString(ffmXmlDataSet.Tables["IncludedCustomsNote1"].Rows[j]["ContentCode"]);
                            custom.SupplementaryCsrIdentifierOci = Convert.ToString(ffmXmlDataSet.Tables["IncludedCustomsNote1"].Rows[j]["Content"]);
                            custom.consigref = awbref;
                            Array.Resize(ref custominfo, custominfo.Length + 1);
                            custominfo[custominfo.Length - 1] = custom;

                            ////special custom information
                            //consinfo[int.Parse(awbref) - 1].customorigincode = msg[2];
                            //consinfo[int.Parse(awbref) - 1].customref = msg[1];

                            MessageData.movementinfo movement = new MessageData.movementinfo("");
                            try
                            {
                                movement.AirportCode = Convert.ToString(ffmXmlDataSet.Tables["OccurrenceDestinationLocation"].Rows[j]["ID"]);
                                drs = ffmXmlDataSet.Tables["PrimaryID"].Select("CarrierParty_Id=" + Convert.ToString(j));
                                if (drs.Length > 0)
                                {
                                    movement.CarrierCode = Convert.ToString(drs[0]["schemeAgencyID"]);
                                }
                                drs = ffmXmlDataSet.Tables["OnCarriageTransportMovement"].Select("OnCarriageTransportMovement_Id=" + Convert.ToString(j) + " AND IncludedMasterConsignment_Id=" + Convert.ToString(j));
                                if (drs.Length > 0)
                                {
                                    movement.FlightNumber = Convert.ToString(drs[0]["ID"]).Substring(3);
                                }

                                drs = ffmXmlDataSet.Tables["OnCarriageEvent"].Select("OnCarriageTransportMovement_Id=" + Convert.ToString(j));
                                if (drs.Length > 0)
                                {
                                    string[] deptDatesplit = Convert.ToString(drs[0]["DepartureOccurrenceDateTime"]).Split('T');
                                    movement.FlightDay = Convert.ToString(Convert.ToDateTime(deptDatesplit[0]).Day);
                                    movement.FlightMonth = Convert.ToString(Convert.ToDateTime(deptDatesplit[0]).ToString("MMM"));
                                }
                                movement.consigref = awbref;
                            }
                            catch (Exception ex)
                            {
                                //clsLog.WriteLogAzure(ex);
                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                            }
                            Array.Resize(ref movementinfo, movementinfo.Length + 1);
                            movementinfo[movementinfo.Length - 1] = movement;

                            ////BUP

                            ////if (bupsplhandcode[1].ToString() == "BUP")
                            ////    consinfo[consinfo.Length - 1].IsBup = "true";
                            //consinfo[consinfo.Length - 1].IsBup = "true";
                            //uld[Convert.ToInt16(uldsequencenum) - 1].AWBNumber = consinfo[consinfo.Length - 1].awbnum;
                            //uld[Convert.ToInt16(uldsequencenum) - 1].AWBPrefix = consinfo[consinfo.Length - 1].airlineprefix;

                            ////if (str[i].ToString().Contains("BUP"))
                            ////{
                            ////    consinfo[consinfo.Length - 1].splhandling = (str[i].Replace("/BUP", "/")).Replace('/', ',').TrimStart(',');
                            ////    uld[Convert.ToInt16(uldsequencenum) - 1].IsBUP = "true";
                            ////    uld[Convert.ToInt16(uldsequencenum) - 1].AWBNumber = consinfo[consinfo.Length - 1].awbnum;
                            ////    uld[Convert.ToInt16(uldsequencenum) - 1].AWBPrefix = consinfo[consinfo.Length - 1].airlineprefix;
                            ////}

                            ////else
                            //consinfo[consinfo.Length - 1].splhandling = Convert.ToString(ffmXmlDataSet.Tables["HandlingSPHInstructions"].Rows[i]["Description"]); 
                        }

                    }
                }

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

        /// <summary>
        /// Split the string  by validating number and character
        /// </summary>
        /// <param name="str">String to split</param>
        /// <returns>Array of number and string elements</returns>
        public string[] StringSplitter(string str)
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
                        {
                            ///number                            
                            if (lastchr == 'N')
                                k--;
                            strarr[k] = strarr[k] + arr[j].ToString();
                            lastchr = 'N';
                        }
                        if (char.IsLetter(arr[j]))
                        {
                            ///letter
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

        /// <summary>
        /// Method to validate the XFFM message and save the all operantional data that already decoded
        /// </summary>
        /// <param name="ffmdata">Array contains flight information</param>
        /// <param name="consinfo"></param>
        /// <param name="unloadingport">Array contains unloading port</param>
        /// <param name="objDimension">Array contains AWB dimensions</param>
        /// <param name="uld">Array contains ULD details</param>
        /// <param name="REFNo">Message SrNo from tblInbox</param>
        /// <param name="strMessage">XFFM message string</param>
        /// <param name="strMessageFrom">Updated By</param>
        /// <param name="strFromID">From emailID</param>
        /// <param name="strStatus">Message status</param>
        //public void SaveValidateFFMMessage(ref MessageData.ffminfo ffmdata, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.unloadingport[] unloadingport, ref MessageData.dimensionnfo[] objDimension, ref MessageData.ULDinfo[] uld, int REFNo, string strMessage, string strMessageFrom, string strFromID, string strStatus)
        public async Task SaveValidateFFMMessage(
         MessageData.ffminfo ffmdata,
         MessageData.consignmnetinfo[] consinfo,
         MessageData.unloadingport[] unloadingport,
         MessageData.dimensionnfo[] objDimension,
         MessageData.ULDinfo[] uld,
         int REFNo,
         string strMessage,
         string strMessageFrom,
         string strFromID,
         string strStatus)
        {
            string source = string.Empty, dest = string.Empty;
            int ffmSequenceNo = 1, ManifestID = 0;
            DataTable dtWeightUpdate = new DataTable("dtWeightUpdate");
            DateTime flightdate = new DateTime();
            try
            {
                //SQLServer sqlServer = new SQLServer();
                string flightnum = ffmdata.carriercode + ffmdata.fltnum;

                flightdate = Convert.ToDateTime(Convert.ToDateTime(ffmdata.fltdate).ToString("MM/dd/yyyy"));
                //DateTime.ParseExact(ffmdata.fltdate, "MM/dd/yyyy", null);                 
                //DateTime.Parse(DateTime.Parse("1." + ffmdata.month + " 2008").Month.ToString().PadLeft(2, '0') + "/" + ffmdata.fltdate.PadLeft(2, '0') + "/" + +System.DateTime.Today.Year);

                //string[] PName = new string[] { "flightnum", "date" };
                //SqlDbType[] PType = new SqlDbType[] { SqlDbType.NVarChar, SqlDbType.DateTime };
                //object[] PValue = new object[] { flightnum, flightdate };
                //DataSet ds = sqlServer.SelectRecords("spGetDestCodeForFFM", PName, PValue, PType);
                var parameters = new SqlParameter[]
                {
                    new("@flightnum", SqlDbType.NVarChar) { Value = flightnum },
                    new("@date", SqlDbType.DateTime)      { Value = flightdate }
                };
                DataSet? ds = await _readWriteDao.SelectRecords("spGetDestCodeForFFM", parameters);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    source = ds.Tables[0].Rows[0]["source"].ToString();
                    dest = ds.Tables[0].Rows[0]["Dest"].ToString();
                }
                if (ffmdata.fltairportcode != "")
                    source = ffmdata.fltairportcode;

                //string[] Pname = new string[5];
                //object[] Pvalue = new object[5];
                //SqlDbType[] Ptype = new SqlDbType[5];

                //Pname[0] = "FLTNo";
                //Pname[1] = "FltDate";
                //Pname[2] = "TailNo";
                //Pname[3] = "FlightOrigin";
                //Pname[4] = "FlightDestination";

                //Ptype[0] = SqlDbType.VarChar;
                //Ptype[1] = SqlDbType.DateTime;
                //Ptype[2] = SqlDbType.VarChar;
                //Ptype[3] = SqlDbType.VarChar;
                //Ptype[4] = SqlDbType.VarChar;

                //Pvalue[0] = ffmdata.carriercode + ffmdata.fltnum;
                //Pvalue[1] = flightdate;
                //Pvalue[2] = ffmdata.aircraftregistration;
                //Pvalue[3] = source;
                //Pvalue[4] = dest;

                //sqlServer = new SQLServer();
                //sqlServer.InsertData("SPSaveTailNumber", Pname, Ptype, Pvalue);
                var parametersSTN = new SqlParameter[]
                {
                    new("@FLTNo", SqlDbType.VarChar)           { Value = ffmdata.carriercode + ffmdata.fltnum },
                    new("@FltDate", SqlDbType.DateTime)        { Value = flightdate },
                    new("@TailNo", SqlDbType.VarChar)          { Value = ffmdata.aircraftregistration },
                    new("@FlightOrigin", SqlDbType.VarChar)    { Value = source },
                    new("@FlightDestination", SqlDbType.VarChar){ Value = dest }
                };
                await _readWriteDao.ExecuteNonQueryAsync("SPSaveTailNumber", parametersSTN);
                if (unloadingport.Length > 0)
                {
                    for (int k = 0; k < unloadingport.Length; k++)
                    {
                        if (unloadingport[k].nilcargocode.Trim() == "")
                        {

                            dest = unloadingport[k].unloadingairport;
                            //GenericFunction genericFunction = new GenericFunction();
                            //genericFunction.UpdateInboxFromMessageParameter(REFNo, string.Empty, flightnum, source, dest, "XFFM", strMessageFrom == "" ? strFromID : strMessageFrom, flightdate);
                            await _genericFunction.UpdateInboxFromMessageParameter(REFNo, string.Empty, flightnum, source, dest, "XFFM", strMessageFrom == "" ? strFromID : strMessageFrom, flightdate);

                            ManifestID = await ExportManifestSummary(ffmdata.carriercode + ffmdata.fltnum, ffmdata.fltairportcode, unloadingport[k].unloadingairport, flightdate, ffmdata.aircraftregistration, REFNo);
                            if (ManifestID > 0)
                            {
                                //sqlServer = new SQLServer();
                                //ffmSequenceNo = int.Parse(ffmdata.messagesequencenum == "" ? "0" : ffmdata.messagesequencenum);
                                //string[] PStatusName = new string[] { "ManifestID", "FFMStatus", "FlightNumber", "FlightDate", "FlightOrigin", "FlightDestination", "FFMSequenceNo" };
                                //object[] PStatusValues = new object[] { ManifestID, ffmdata.endmesgcode, ffmdata.carriercode + ffmdata.fltnum, flightdate, ffmdata.fltairportcode, unloadingport[k].unloadingairport, ffmSequenceNo };
                                //SqlDbType[] sqlStatusType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int };
                                //sqlServer.InsertData("MSG_uSPCheckFFMFlightStatus", PStatusName, sqlStatusType, PStatusValues);
                                var parametersFFMFS = new SqlParameter[]
                                {
                                    new("@ManifestID", SqlDbType.VarChar)         { Value = ManifestID },
                                    new("@FFMStatus", SqlDbType.VarChar)          { Value = ffmdata.endmesgcode },
                                    new("@FlightNumber", SqlDbType.VarChar)       { Value = ffmdata.carriercode + ffmdata.fltnum },
                                    new("@FlightDate", SqlDbType.DateTime)        { Value = flightdate },
                                    new("@FlightOrigin", SqlDbType.VarChar)       { Value = ffmdata.fltairportcode },
                                    new("@FlightDestination", SqlDbType.VarChar)  { Value = unloadingport[k].unloadingairport },
                                    new("@FFMSequenceNo", SqlDbType.Int)          { Value = ffmSequenceNo }
                                };
                                await _readWriteDao.ExecuteNonQueryAsync("MSG_uSPCheckFFMFlightStatus", parametersFFMFS);
                                MessageData.consignmnetinfo[] ReceivedConsigInfo = new MessageData.consignmnetinfo[consinfo.Length];
                                Array.Copy(consinfo, ReceivedConsigInfo, consinfo.Length);

                                MessageData.consignmnetinfo[] FFMConsig = new MessageData.consignmnetinfo[consinfo.Length];
                                Array.Copy(consinfo, FFMConsig, consinfo.Length);

                                if (consinfo.Length > 0)
                                {
                                    DataTable dtAWBDetail = new DataTable();
                                    dtAWBDetail.Columns.Add("AirlinePrefix");
                                    dtAWBDetail.Columns.Add("AWBNo");
                                    dtAWBDetail.Columns.Add("Origin");
                                    dtAWBDetail.Columns.Add("Destination");
                                    dtAWBDetail.Columns.Add("AWbPieceCode");

                                    dtAWBDetail.Columns.Add("AWBPieces", SqlDbType.Int.GetType());
                                    dtAWBDetail.Columns.Add("FFMPiecesCode");
                                    dtAWBDetail.Columns.Add("FFMPieces", SqlDbType.Int.GetType());

                                    dtAWBDetail.Columns.Add("GrossWt");
                                    dtAWBDetail.Columns.Add("VolumeCode");
                                    dtAWBDetail.Columns.Add("VolumeWt");

                                    for (int i = 0; i < consinfo.Length; i++)
                                    {
                                        DataRow drawb = dtAWBDetail.NewRow();
                                        drawb["AirlinePrefix"] = consinfo[i].airlineprefix;
                                        drawb["AWBNo"] = consinfo[i].awbnum;
                                        drawb["Origin"] = consinfo[i].origin;
                                        drawb["Destination"] = consinfo[i].dest;
                                        drawb["AWBPieces"] = consinfo[i].numshp;
                                        if (consinfo[i].consigntype != "T")
                                        {
                                            drawb["AWbPieceCode"] = consinfo[i].shpdesccode;
                                            drawb["FFMPiecesCode"] = consinfo[i].consigntype;
                                            drawb["FFMPieces"] = consinfo[i].pcscnt;
                                        }
                                        else
                                        {
                                            drawb["AWbPieceCode"] = consinfo[i].consigntype;
                                        }
                                        drawb["GrossWt"] = consinfo[i].weight;
                                        drawb["VolumeWt"] = consinfo[i].volumeamt;
                                        drawb["VolumeCode"] = consinfo[i].volumecode;
                                        dtAWBDetail.Rows.Add(drawb);
                                    }
                                    dtWeightUpdate.Columns.Add("AirlinePrefix");
                                    dtWeightUpdate.Columns.Add("AWBNo");
                                    dtWeightUpdate.Columns.Add("Origin");
                                    dtWeightUpdate.Columns.Add("Destination");
                                    dtWeightUpdate.Columns.Add("AWbPieceCode");
                                    dtWeightUpdate.Columns.Add("AWBPieces", SqlDbType.Int.GetType());
                                    dtWeightUpdate.Columns.Add("FFMPiecesCode");
                                    dtWeightUpdate.Columns.Add("FFMPieces", SqlDbType.Int.GetType());
                                    dtWeightUpdate.Columns.Add("GrossWt");
                                    dtWeightUpdate.Columns.Add("VolumeCode");
                                    dtWeightUpdate.Columns.Add("VolumeWt");
                                    DataRow[] drs = dtAWBDetail.Select("", "AWBNo");
                                    decimal gross = 0;
                                    int pieces = 0, ffmPieces = 0;
                                    decimal volume = 0;

                                    string awbNo = drs[0]["AWBNo"].ToString();
                                    DataRow drn = dtWeightUpdate.NewRow();
                                    drn["AirlinePrefix"] = drs[0]["AirlinePrefix"].ToString();
                                    drn["AWBNo"] = drs[0]["AWBNo"].ToString();
                                    drn["Origin"] = drs[0]["Origin"].ToString();
                                    drn["Destination"] = drs[0]["Destination"].ToString();
                                    drn["AWbPieceCode"] = drs[0]["AWbPieceCode"].ToString() == ""
                                                              ? ""
                                                              : drs[0]["AWbPieceCode"].ToString();
                                    drn["FFMPiecesCode"] = drs[0]["FFMPiecesCode"].ToString();
                                    drn["AWBPieces"] = drs[0]["AWBPieces"].ToString() == ""
                                                       ? "0"
                                                       : drs[0]["AWBPieces"].ToString();
                                    drn["FFMPieces"] = drs[0]["FFMPieces"].ToString() == ""
                                                        ? "0"
                                                        : drs[0]["FFMPieces"].ToString();
                                    drn["VolumeCode"] = drs[0]["VolumeCode"].ToString() == ""
                                                        ? ""
                                                        : drs[0]["VolumeCode"].ToString();
                                    foreach (DataRow dr in drs)
                                    {
                                        if (awbNo == dr["AWBNo"].ToString())
                                        {
                                            gross +=
                                                Convert.ToDecimal(dr["GrossWt"].ToString() == ""
                                                                      ? "0.0"
                                                                      : dr["GrossWt"].ToString());

                                            ffmPieces +=
                                                int.Parse(dr["FFMPieces"].ToString() == "" ? "0" : dr["FFMPieces"].ToString());

                                            volume +=
                                                Convert.ToDecimal(drs[0]["VolumeWt"].ToString() == ""
                                                                      ? "0.0"
                                                                      : drs[0]["VolumeWt"].ToString());
                                            drn["AWbPieceCode"] = dr["AWbPieceCode"].ToString();

                                            pieces = int.Parse(drs[0]["AWBPieces"].ToString() == "" ? "0" : drs[0]["AWBPieces"].ToString());

                                            drn["FFMPiecesCode"] = drs[0]["FFMPiecesCode"].ToString();

                                            drn["FFMPieces"] = ffmPieces;

                                            drn["GrossWt"] =
                                                Convert.ToDecimal(drn["GrossWt"].ToString() == ""
                                                                      ? "0.0"
                                                                      : drn["GrossWt"].ToString()) +
                                                Convert.ToDecimal(dr["GrossWt"].ToString() == "" ? "0" : dr["GrossWt"].ToString());

                                            drn["VolumeWt"] = Convert.ToDecimal(drn["VolumeWt"].ToString() == ""
                                                                      ? "0.0"
                                                                      : drn["VolumeWt"].ToString()) +
                                                Convert.ToDecimal(dr["VolumeWt"].ToString() == "" ? "0" : dr["VolumeWt"].ToString());

                                        }
                                        else
                                        {
                                            dtWeightUpdate.Rows.Add(drn);
                                            drn = dtWeightUpdate.NewRow();
                                            gross =
                                                Convert.ToDecimal(dr["GrossWt"].ToString() == "" ? "0" : dr["GrossWt"].ToString());
                                            pieces =
                                                Convert.ToInt16(dr["AWBPieces"].ToString() == "" ? "0" : dr["AWBPieces"].ToString());
                                            volume =
                                                Convert.ToDecimal(dr["VolumeWt"].ToString() == ""
                                                                      ? "0"
                                                                      : dr["VolumeWt"].ToString());
                                            ffmPieces = Convert.ToInt16(dr["FFMPieces"].ToString() == "" ? "0" : dr["FFMPieces"].ToString());

                                            awbNo = dr["AWBNo"].ToString();
                                            drn["AirlinePrefix"] = dr["AirlinePrefix"].ToString();
                                            drn["AWBNo"] = dr["AWBNo"].ToString();
                                            drn["Origin"] = dr["Origin"].ToString();
                                            drn["Destination"] = dr["Destination"].ToString();
                                            drn["AWbPieceCode"] = dr["AWbPieceCode"].ToString();
                                            drn["AWBPieces"] = dr["AWBPieces"].ToString();
                                            drn["FFMPiecesCode"] = dr["FFMPiecesCode"].ToString();
                                            drn["FFMPieces"] = dr["FFMPieces"].ToString() == "" ? "0" : dr["FFMPieces"].ToString();
                                            drn["GrossWt"] = gross;
                                            drn["VolumeWt"] = volume;
                                            drn["VolumeCode"] = dr["volumeCode"].ToString();
                                        }
                                    }
                                    dtWeightUpdate.Rows.Add(drn);

                                    #region : Add Consigment Details booking table :
                                    if (consinfo.Length > 0)
                                    {
                                        #region : Club Pieces and Weight :
                                        MessageData.consignmnetinfo[] FFMAuditLogConsig = new MessageData.consignmnetinfo[consinfo.Length];
                                        Array.Copy(consinfo, FFMAuditLogConsig, consinfo.Length);
                                        MessageData.consignmnetinfo[] auditconsinfo = new MessageData.consignmnetinfo[0];
                                        ReProcessConsigment(ref FFMAuditLogConsig, ref auditconsinfo);
                                        #endregion
                                        for (int i = 0; i < auditconsinfo.Length; i++)
                                        {
                                            string AWBNum = consinfo[i].awbnum;
                                            string AWBPrefix = consinfo[i].airlineprefix;
                                            string MftPcs = consinfo[i].pcscnt;
                                            string MftWt = consinfo[i].weight;
                                            string ConsignmentType = consinfo[i].consigntype;

                                            #region Add AWB details and Audit Log
                                            if (!await IsAWBPresent(AWBNum, AWBPrefix))
                                            {
                                                //string[] paramname = new string[] { "AirlinePrefix", "AWBNum", "Origin", "Dest", "PcsCount", "Weight", "Volume", "ComodityCode", "ComodityDesc", "CarrierCode", "FlightNum", "FlightDate", "FlightOrigin", "FlightDest", "ShipperName", "ShipperAddr", "ShipperPlace", "ShipperState", "ShipperCountryCode", "ShipperContactNo", "ConsName", "ConsAddr", "ConsPlace", "ConsState", "ConsCountryCode", "ConsContactNo", "CustAccNo", "IATACargoAgentCode", "CustName", "SystemDate", "MeasureUnit", "Length", "Breadth", "Height", "PartnerStatus", "REFNo", "UpdatedBy", "WeightCode" };

                                                //object[] paramvalue = new object[] { consinfo[i].airlineprefix, consinfo[i].awbnum, consinfo[i].origin, consinfo[i].dest, consinfo[i].pcscnt, consinfo[i].weight, consinfo[i].volumeamt, "9999", consinfo[i].manifestdesc, "", flightnum, flightdate.ToString("dd/MM/yyyy"), source, dest, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", DateTime.UtcNow.ToString("yyyy-MM-dd"), "", "", "", "", "", REFNo, "XFFM", consinfo[i].weightcode };

                                                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                                                //              SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                                                //              SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.Int,SqlDbType.VarChar,SqlDbType.VarChar };
                                                var parametersBDFFR = new SqlParameter[]
                                                {
                                                    new("@AirlinePrefix", SqlDbType.VarChar)       { Value = consinfo[i].airlineprefix },
                                                    new("@AWBNum", SqlDbType.VarChar)              { Value = consinfo[i].awbnum },
                                                    new("@Origin", SqlDbType.VarChar)              { Value = consinfo[i].origin },
                                                    new("@Dest", SqlDbType.VarChar)                { Value = consinfo[i].dest },
                                                    new("@PcsCount", SqlDbType.VarChar)            { Value = consinfo[i].pcscnt },
                                                    new("@Weight", SqlDbType.VarChar)              { Value = consinfo[i].weight },
                                                    new("@Volume", SqlDbType.VarChar)              { Value = consinfo[i].volumeamt },
                                                    new("@ComodityCode", SqlDbType.VarChar)        { Value = "9999" },
                                                    new("@ComodityDesc", SqlDbType.VarChar)        { Value = consinfo[i].manifestdesc },
                                                    new("@CarrierCode", SqlDbType.VarChar)         { Value = "" },
                                                    new("@FlightNum", SqlDbType.VarChar)           { Value = flightnum },
                                                    new("@FlightDate", SqlDbType.VarChar)          { Value = flightdate.ToString("dd/MM/yyyy") },
                                                    new("@FlightOrigin", SqlDbType.VarChar)        { Value = source },
                                                    new("@FlightDest", SqlDbType.VarChar)          { Value = dest },
                                                    new("@ShipperName", SqlDbType.VarChar)         { Value = "" },
                                                    new("@ShipperAddr", SqlDbType.VarChar)         { Value = "" },
                                                    new("@ShipperPlace", SqlDbType.VarChar)        { Value = "" },
                                                    new("@ShipperState", SqlDbType.VarChar)        { Value = "" },
                                                    new("@ShipperCountryCode", SqlDbType.VarChar)  { Value = "" },
                                                    new("@ShipperContactNo", SqlDbType.VarChar)    { Value = "" },
                                                    new("@ConsName", SqlDbType.VarChar)            { Value = "" },
                                                    new("@ConsAddr", SqlDbType.VarChar)            { Value = "" },
                                                    new("@ConsPlace", SqlDbType.VarChar)           { Value = "" },
                                                    new("@ConsState", SqlDbType.VarChar)           { Value = "" },
                                                    new("@ConsCountryCode", SqlDbType.VarChar)     { Value = "" },
                                                    new("@ConsContactNo", SqlDbType.VarChar)       { Value = "" },
                                                    new("@CustAccNo", SqlDbType.VarChar)           { Value = "" },
                                                    new("@IATACargoAgentCode", SqlDbType.VarChar)  { Value = "" },
                                                    new("@CustName", SqlDbType.VarChar)            { Value = "" },
                                                    new("@SystemDate", SqlDbType.DateTime)         { Value = DateTime.UtcNow.ToString("yyyy-MM-dd") },
                                                    new("@MeasureUnit", SqlDbType.VarChar)         { Value = "" },
                                                    new("@Length", SqlDbType.VarChar)              { Value = "" },
                                                    new("@Breadth", SqlDbType.VarChar)             { Value = "" },
                                                    new("@Height", SqlDbType.VarChar)              { Value = "" },
                                                    new("@PartnerStatus", SqlDbType.VarChar)       { Value = "" },
                                                    new("@REFNo", SqlDbType.Int)                   { Value = REFNo },
                                                    new("@UpdatedBy", SqlDbType.VarChar)           { Value = "XFFM" },
                                                    new("@WeightCode", SqlDbType.VarChar)          { Value = consinfo[i].weightcode }
                                                };
                                                //sqlServer = new SQLServer();
                                                string procedure = "spInsertBookingDataFromFFR";

                                                //if (!sqlServer.InsertData(procedure, paramname, paramtype, paramvalue))
                                                if (!await _readWriteDao.ExecuteNonQueryAsync(procedure, parametersBDFFR))
                                                    // clsLog.WriteLogAzure("Error in XFFM AWB Add Error for:" + consinfo[i].awbnum);
                                                    _logger.LogWarning("Error in XFFM AWB Add Error for: {0}", consinfo[i].awbnum);

                                                else
                                                {
                                                    #region Add Audit Log
                                                    for (int j = 0; j < auditconsinfo.Length; j++)
                                                    {
                                                        if (AWBNum == auditconsinfo[j].awbnum && AWBPrefix == auditconsinfo[j].airlineprefix)
                                                        {
                                                            //sqlServer = new SQLServer();
                                                            string[] arrMilestone = new string[3] { "Booked", "Executed", "Accepted" };
                                                            for (int z = 0; z < arrMilestone.Length; z++)
                                                            {
                                                                //string[] CNname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination"
                                                                //        , "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public", "ULDNo" };
                                                                //SqlDbType[] CType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar
                                                                //        , SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar };
                                                                //object[] CValues = new object[] { auditconsinfo[j].airlineprefix, auditconsinfo[j].awbnum, auditconsinfo[j].origin, auditconsinfo[j].dest, auditconsinfo[j].pcscnt, auditconsinfo[j].weight, ffmdata.carriercode + ffmdata.fltnum, flightdate, ffmdata.fltairportcode, unloadingport[k].unloadingairport, arrMilestone[z], "AWB " +arrMilestone[z],
                                                                //                      "AWB " +arrMilestone[z] + " through XFFM", "XFFM", DateTime.UtcNow.ToString(), 1, string.Empty };
                                                                var parametersAWBAL = new SqlParameter[]
                                                                {
                                                                    new("@AWBPrefix", SqlDbType.VarChar)          { Value = auditconsinfo[j].airlineprefix },
                                                                    new("@AWBNumber", SqlDbType.VarChar)          { Value = auditconsinfo[j].awbnum },
                                                                    new("@Origin", SqlDbType.VarChar)             { Value = auditconsinfo[j].origin },
                                                                    new("@Destination", SqlDbType.VarChar)        { Value = auditconsinfo[j].dest },
                                                                    new("@Pieces", SqlDbType.VarChar)             { Value = auditconsinfo[j].pcscnt },
                                                                    new("@Weight", SqlDbType.VarChar)             { Value = auditconsinfo[j].weight },
                                                                    new("@FlightNo", SqlDbType.VarChar)           { Value = ffmdata.carriercode + ffmdata.fltnum },
                                                                    new("@FlightDate", SqlDbType.DateTime)        { Value = flightdate },
                                                                    new("@FlightOrigin", SqlDbType.VarChar)       { Value = ffmdata.fltairportcode },
                                                                    new("@FlightDestination", SqlDbType.VarChar)  { Value = unloadingport[k].unloadingairport },
                                                                    new("@Action", SqlDbType.VarChar)             { Value = arrMilestone[z] },
                                                                    new("@Message", SqlDbType.VarChar)            { Value = "AWB " + arrMilestone[z] },
                                                                    new("@Description", SqlDbType.VarChar)        { Value = "AWB " + arrMilestone[z] + " through XFFM" },
                                                                    new("@UpdatedBy", SqlDbType.VarChar)          { Value = "XFFM" },
                                                                    new("@UpdatedOn", SqlDbType.VarChar)         { Value = DateTime.UtcNow.ToString() },
                                                                    new("@Public", SqlDbType.Bit)                 { Value = 1 },
                                                                    new("@ULDNo", SqlDbType.VarChar)              { Value = string.Empty }
                                                                };
                                                                //if (!sqlServer.ExecuteProcedure("SPAddAWBAuditLog", CNname, CType, CValues))
                                                                if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", parametersAWBAL))
                                                                    //removed sqlServer.LastErrorDescription not required
                                                                    //clsLog.WriteLog("AWB Audit log  for:" + consinfo[i].awbnum + Environment.NewLine + "Error: " + sqlServer.LastErrorDescription);
                                                                    _logger.LogWarning("AWB Audit log  for: {0}" , consinfo[i].awbnum + Environment.NewLine + "Error: ");
                                                            }
                                                        }
                                                    }
                                                    #endregion Add Audit Log
                                                }
                                            }
                                            else
                                            {
                                                #region Save AWBNO On Audit Log for acceptance
                                                //sqlServer = new SQLServer();
                                                //string[] CNname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public", "ULDNo" };
                                                //SqlDbType[] CType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar };
                                                //object[] CValues = new object[] { consinfo[i].airlineprefix, consinfo[i].awbnum, consinfo[i].origin, consinfo[i].dest, consinfo[i].pcscnt, consinfo[i].weight, ffmdata.carriercode + ffmdata.fltnum, flightdate, ffmdata.fltairportcode, unloadingport[k].unloadingairport, "Accepted", "AWB Accepted", "AWB Accepted by XFFM", "XFFM", DateTime.UtcNow.ToString(), 1, string.Empty };
                                                SqlParameter[] cnParameters = new SqlParameter[]
                                                {
                                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = consinfo[i].airlineprefix },
                                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = consinfo[i].awbnum },
                                                    new SqlParameter("@Origin", SqlDbType.VarChar) { Value = consinfo[i].origin },
                                                    new SqlParameter("@Destination", SqlDbType.VarChar) { Value = consinfo[i].dest },
                                                    new SqlParameter("@Pieces", SqlDbType.VarChar) { Value = consinfo[i].pcscnt },
                                                    new SqlParameter("@Weight", SqlDbType.VarChar) { Value = consinfo[i].weight },
                                                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = ffmdata.carriercode + ffmdata.fltnum },
                                                    new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = flightdate },
                                                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = ffmdata.fltairportcode },
                                                    new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = unloadingport[k].unloadingairport },
                                                    new SqlParameter("@Action", SqlDbType.VarChar) { Value = "Accepted" },
                                                    new SqlParameter("@Message", SqlDbType.VarChar) { Value = "AWB Accepted" },
                                                    new SqlParameter("@Description", SqlDbType.VarChar) { Value = "AWB Accepted by XFFM" },
                                                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "XFFM" },
                                                    new SqlParameter("@UpdatedOn", SqlDbType.VarChar) { Value = DateTime.UtcNow.ToString() },
                                                    new SqlParameter("@Public", SqlDbType.Bit) { Value = 1 },
                                                    new SqlParameter("@ULDNo", SqlDbType.VarChar) { Value = string.Empty }
                                                };

                                                //if (!sqlServer.ExecuteProcedure("SPAddAWBAuditLog", CNname, CType, CValues))
                                                if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", cnParameters))
                                                {
                                                    //clsLog.WriteLog("AWB Audit log  for:" + consinfo[i].awbnum + Environment.NewLine + "Error: " + sqlServer.LastErrorDescription);
                                                    _logger.LogWarning("AWB Audit log  for: {0}" , consinfo[i].awbnum + Environment.NewLine + "Error: ");
                                                }
                                                #endregion Save AWBNO On Audit Log for acceptance
                                            }

                                            #endregion Add AWB details and Audit Log
                                        }
                                    }
                                    #endregion Add Consigment Details booking table

                                    consinfo = new MessageData.consignmnetinfo[ReceivedConsigInfo.Length];
                                    Array.Copy(ReceivedConsigInfo, consinfo, ReceivedConsigInfo.Length);

                                    #region Check Availabe ULD data if Present insert into ExportManifestULDAWB Association
                                    if (consinfo.Length > 0)
                                    {
                                        for (int i = 0; i < consinfo.Length; i++)
                                        {
                                            #region Store in Manifest ULD Tables
                                            int refNum = Convert.ToInt16(consinfo[i].portsequence) - 1;
                                            int ULDRef = 0;
                                            string ULDNo = "BULK";

                                            int TotalAWBPcs = 0;
                                            if (consinfo[i].numshp != "")
                                                TotalAWBPcs = int.Parse(consinfo[i].numshp);
                                            else
                                                TotalAWBPcs = int.Parse(consinfo[i].pcscnt);

                                            if (consinfo[i].uldsequence.Length > 0)
                                            {
                                                ULDRef = Convert.ToInt16(consinfo[i].uldsequence);
                                                if (ULDRef > 0)
                                                    ULDNo = uld[ULDRef - 1].uldno;
                                            }

                                            if (!await ExportManifestDetails(ULDNo, ffmdata.fltairportcode, unloadingport[refNum].unloadingairport, consinfo[i].origin, consinfo[i].dest, consinfo[i].awbnum, consinfo[i].splhandling.Trim(','), consinfo[i].volumeamt == "" ? "0.01" : consinfo[i].volumeamt, consinfo[i].pcscnt, consinfo[i].weight, consinfo[i].manifestdesc, flightdate, ManifestID, flightnum, consinfo[i].airlineprefix, consinfo[i].weightcode == "" ? "L" : consinfo[i].weightcode, ffmSequenceNo, TotalAWBPcs, REFNo, consinfo[i].IsBup == "true" ? true : false))
                                                //clsLog.WriteLogAzure("Error in XFFM Manifest Details:" + consinfo[i].awbnum);
                                                _logger.LogWarning("Error in XFFM Manifest Details: {0}" , consinfo[i].awbnum);
                                            else
                                            {
                                                if (!await ExportManifestULDAWBAssociation(ULDNo, ffmdata.fltairportcode, unloadingport[refNum].unloadingairport, consinfo[i].origin, consinfo[i].dest, consinfo[i].awbnum, consinfo[i].splhandling.Trim(',') == "" ? "GEN" : consinfo[i].splhandling.Trim(','), consinfo[i].volumeamt == "" ? "0" : consinfo[i].volumeamt, consinfo[i].pcscnt, consinfo[i].weight, consinfo[i].manifestdesc, flightdate, ManifestID, consinfo[i].airlineprefix, ffmdata.carriercode + ffmdata.fltnum, consinfo[i].numshp, consinfo[i].weight, consinfo[i].consigntype, TotalAWBPcs, ffmSequenceNo, consinfo[i].weightcode, REFNo, consinfo[i].IsBup == "true" ? true : false))
                                                    //clsLog.WriteLogAzure("Error in XFFM Manifest Details:" + consinfo[i].awbnum);
                                                    _logger.LogWarning("Error in XFFM Manifest Details: {0}" , consinfo[i].awbnum);

                                                #region Status Message in Table
                                                //sqlServer = new SQLServer();
                                                //string[] PVName = new string[] { "AWBPrefix", "AWBNumber", "MType", "desc", "date", "time", "refno", "FlightNo", "FlightDate", "PCS", "WT", "UOM", "UpdatedBy", "UpdatedOn" };
                                                //object[] PValues = new object[] { consinfo[i].airlineprefix, consinfo[i].awbnum, "XFFM", ffmdata.fltairportcode + "-" + flightnum + "-" + flightdate, "", "", 0, ffmdata.carriercode + ffmdata.fltnum, flightdate, TotalAWBPcs, consinfo[i].weight == "" ? "0" : consinfo[i].weight, consinfo[i].weightcode, "XFFM", flightdate };
                                                //SqlDbType[] sqlType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Int, SqlDbType.Decimal, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime };
                                                //sqlServer.InsertData("spInsertAWBMessageStatus", PVName, sqlType, PValues);
                                                var statusParameters = new SqlParameter[]
                                                {
                                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar)   { Value = consinfo[i].airlineprefix },
                                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar)   { Value =  consinfo[i].awbnum },
                                                    new SqlParameter("@MType", SqlDbType.VarChar)       { Value = "XFFM" },
                                                    new SqlParameter("@desc", SqlDbType.VarChar)        { Value = ffmdata.fltairportcode + "-" + flightnum + "-" + flightdate },
                                                    new SqlParameter("@date", SqlDbType.NVarChar)       { Value = "" },
                                                    new SqlParameter("@time", SqlDbType.NVarChar)       { Value = "" },
                                                    new SqlParameter("@refno", SqlDbType.Int)           { Value = 0 },
                                                    new SqlParameter("@FlightNo", SqlDbType.VarChar)    { Value = ffmdata.carriercode + ffmdata.fltnum },
                                                    new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = flightdate },
                                                    new SqlParameter("@PCS", SqlDbType.Int)             { Value = TotalAWBPcs },
                                                    new SqlParameter("@WT", SqlDbType.Decimal)          { Value = consinfo[i].weight == "" ? "0" : consinfo[i].weight },
                                                    new SqlParameter("@UOM", SqlDbType.VarChar)        { Value = consinfo[i].weightcode },
                                                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar)   { Value = "XFFM" },
                                                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime)  { Value = flightdate }
                                                };
                                                await _readWriteDao.ExecuteNonQueryAsync("spInsertAWBMessageStatus", statusParameters);
                                                #endregion Status Message in Table

                                                #region Save AWBNO On Audit Log
                                                //sqlServer = new SQLServer();
                                                //string[] CNname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public", "ULDNo" };
                                                //SqlDbType[] CType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar };
                                                //object[] CValues = new object[] { consinfo[i].airlineprefix, consinfo[i].awbnum, consinfo[i].origin, consinfo[i].dest, consinfo[i].pcscnt, consinfo[i].weight, ffmdata.carriercode + ffmdata.fltnum, flightdate, ffmdata.fltairportcode, unloadingport[k].unloadingairport, "Departed", "AWB Departed", "AWB Departed In (" + ULDNo.ToUpper() + ")", "XFFM", DateTime.UtcNow.ToString(), 1, ULDNo.ToUpper() };
                                                var CValues = new SqlParameter[]
                                                {
                                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar)          { Value = consinfo[i].airlineprefix },
                                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar)          { Value = consinfo[i].awbnum },
                                                    new SqlParameter("@Origin", SqlDbType.VarChar)             { Value = consinfo[i].origin },
                                                    new SqlParameter("@Destination", SqlDbType.VarChar)        { Value = consinfo[i].dest },
                                                    new SqlParameter("@Pieces", SqlDbType.VarChar)             { Value = consinfo[i].pcscnt },
                                                    new SqlParameter("@Weight", SqlDbType.VarChar)             { Value = consinfo[i].weight },
                                                    new SqlParameter("@FlightNo", SqlDbType.VarChar)           { Value = ffmdata.carriercode + ffmdata.fltnum },
                                                    new SqlParameter("@FlightDate", SqlDbType.DateTime)        { Value = flightdate },
                                                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar)       { Value = ffmdata.fltairportcode },
                                                    new SqlParameter("@FlightDestination", SqlDbType.VarChar)  { Value = unloadingport[k].unloadingairport },
                                                    new SqlParameter("@Action", SqlDbType.VarChar)             { Value = "Departed" },
                                                    new SqlParameter("@Message", SqlDbType.VarChar)            { Value = "AWB Departed" },
                                                    new SqlParameter("@Description", SqlDbType.VarChar)        { Value = "AWB Departed In (" + ULDNo.ToUpper() + ")" },
                                                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar)          { Value = "XFFM" },
                                                    new SqlParameter("@UpdatedOn", SqlDbType.VarChar)         { Value = DateTime.UtcNow.ToString() },
                                                    new SqlParameter("@Public", SqlDbType.Bit)                 { Value = 1 },
                                                    new SqlParameter("@ULDNo", SqlDbType.VarChar)              { Value = ULDNo.ToUpper() }
                                                };

                                                //if (!sqlServer.ExecuteProcedure("SPAddAWBAuditLog", CNname, CType, CValues))
                                                if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", CValues))
                                                {
                                                    //clsLog.WriteLog("AWB Audit log  for:" + consinfo[i].awbnum + Environment.NewLine + "Error: " + sqlServer.LastErrorDescription);
                                                    _logger.LogWarning("AWB Audit log  for: {0}" , consinfo[i].awbnum + Environment.NewLine + "Error: ");
                                                }
                                                #endregion Save AWBNO On Audit Log
                                            }
                                            #endregion
                                        }
                                    }
                                    #endregion

                                    #region Reprocess the Consigment Info
                                    FFMConsig = new MessageData.consignmnetinfo[consinfo.Length];
                                    Array.Copy(consinfo, FFMConsig, consinfo.Length);
                                    consinfo = new MessageData.consignmnetinfo[0];
                                    ReProcessConsigment(ref FFMConsig, ref consinfo);
                                    #endregion Reprocess the Consigment Info

                                    #region Add Consigment Details booking table
                                    if (consinfo.Length > 0)
                                    {
                                        for (int i = 0; i < consinfo.Length; i++)
                                        {
                                            string AWBNum = consinfo[i].awbnum;
                                            string AWBPrefix = consinfo[i].airlineprefix;
                                            string MftPcs = consinfo[i].pcscnt;
                                            string MftWt = consinfo[i].weight;
                                            string ConsignmentType = consinfo[i].consigntype;

                                            int strAwbPcs = 0;
                                            if (consinfo[i].numshp != "")
                                                strAwbPcs = int.Parse(consinfo[i].numshp == "" ? "0" : consinfo[i].numshp);
                                            else
                                                strAwbPcs = int.Parse(consinfo[i].pcscnt == "" ? "0" : consinfo[i].pcscnt);
                                            #region AWB Dimensions
                                            try
                                            {
                                                int row = 0;
                                                if (objDimension.Length > 0 || uld.Length > 0)
                                                {
                                                    DataSet dsDimension = await GenertateAWBDimensions(AWBNum, strAwbPcs, null, Convert.ToDecimal(consinfo[i].weight), "XFFM", DateTime.Now, false, AWBPrefix);
                                                    if (dsDimension != null && dsDimension.Tables.Count > 0 && dsDimension.Tables[0].Rows.Count > 0)
                                                    {
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
                                                        await GenertateAWBDimensions(AWBNum, Convert.ToInt16(consinfo[i].pcscnt), dsDimension, Convert.ToDecimal(consinfo[i].weight), "XFFM", System.DateTime.Now, true, AWBPrefix);
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                //clsLog.WriteLogAzure(ex);
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");

                                            }
                                            #endregion AWB Dimensions
                                        }
                                    }

                                    #endregion

                                    for (int route = 0; route < dtWeightUpdate.Rows.Count; route++)
                                    {
                                        //sqlServer = new SQLServer();
                                        //string[] pchangeAWB = new string[] { "AWBNumber", "AWBPrefix", "AWBOrigin", "AWBDestination" };
                                        //SqlDbType[] PchangeAWBtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                                        //object[] pchangeAWbValue = new object[] { dtWeightUpdate.Rows[route]["AWBNo"].ToString(), dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString(), dtWeightUpdate.Rows[route]["Origin"].ToString(), dtWeightUpdate.Rows[route]["Destination"].ToString() };
                                        var pchangeAWB = new SqlParameter[]
                                        {
                                            new SqlParameter("@AWBNumber", SqlDbType.VarChar)    { Value = dtWeightUpdate.Rows[route]["AWBNo"].ToString() },
                                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar)    { Value = dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() },
                                            new SqlParameter("@AWBOrigin", SqlDbType.VarChar)    { Value = dtWeightUpdate.Rows[route]["Origin"].ToString() },
                                            new SqlParameter("@AWBDestination", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["Destination"].ToString() }
                                        };
                                        //if (!sqlServer.UpdateData("MSG_uspUpdateShipmentDestinationthroughFFM", pchangeAWB, PchangeAWBtype, pchangeAWbValue))
                                        if (!await _readWriteDao.ExecuteNonQueryAsync("MSG_uspUpdateShipmentDestinationthroughFFM", pchangeAWB))
                                            //clsLog.WriteLogAzure("Error in Update AWB Destination" + sqlServer.LastErrorDescription);
                                            _logger.LogWarning("Error in Update AWB Destination");

                                        decimal ChargeableWeight = 0, TotalChargeableWeight = 0;
                                        ChargeableWeight = Convert.ToDecimal(dtWeightUpdate.Rows[route]["GrossWt"].ToString() == "" ? "0" : dtWeightUpdate.Rows[route]["GrossWt"]);

                                        //sqlServer = new SQLServer();
                                        //string[] cPname = new string[] { "AWBNumber", "AWBPrefix" };
                                        //SqlDbType[] checkPtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
                                        //object[] ccheckpavlue = new object[] { dtWeightUpdate.Rows[route]["AWBNo"].ToString(), dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() };
                                        var cPname = new SqlParameter[]
                                        {
                                            new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AWBNo"].ToString() },
                                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() }
                                        };

                                        //DataSet dscheckawbn = sqlServer.SelectRecords("MSG_uSPCheckBookingDoneByEDIMessage", cPname, ccheckpavlue, checkPtype);
                                        DataSet dscheckawbn = await _readWriteDao.SelectRecords("MSG_uSPCheckBookingDoneByEDIMessage", cPname);

                                        if (dscheckawbn != null && dscheckawbn.Tables.Count > 0 && dscheckawbn.Tables[0].Rows.Count > 0)
                                        {
                                            if (dscheckawbn.Tables[0].Rows[0]["AWBCreatedStatus"].ToString() == "1")
                                            {
                                                #region Make AWB Route through XFFM Message
                                                MessageData.FltRoute[] fltroute = new MessageData.FltRoute[0];
                                                MessageData.FltRoute flight = new MessageData.FltRoute("");
                                                if (!(dtWeightUpdate.Rows[route]["Origin"].ToString() == source && dest == dtWeightUpdate.Rows[route]["Destination"].ToString()))
                                                {
                                                    if (dtWeightUpdate.Rows[route]["Origin"].ToString() == source)
                                                    {
                                                        flight.carriercode = flightnum.Substring(0, 2);
                                                        flight.fltnum = flightnum;
                                                        flight.date = flightdate.ToString("MM/dd/yyyy");
                                                        flight.fltdept = dtWeightUpdate.Rows[route]["Origin"].ToString();
                                                        flight.fltarrival = dest;

                                                        Array.Resize(ref fltroute, fltroute.Length + 1);
                                                        fltroute[fltroute.Length - 1] = flight;

                                                        if (dest != dtWeightUpdate.Rows[route]["Destination"].ToString())
                                                        {
                                                            flight.carriercode = string.Empty;
                                                            flight.fltnum = string.Empty;
                                                            flight.date = flightdate.ToString("MM/dd/yyyy");
                                                            flight.fltdept = dest;
                                                            flight.fltarrival = dtWeightUpdate.Rows[route]["Destination"].ToString();
                                                            Array.Resize(ref fltroute, fltroute.Length + 1);
                                                            fltroute[fltroute.Length - 1] = flight;
                                                        }
                                                    }

                                                    if (dtWeightUpdate.Rows[route]["Origin"].ToString() != source)
                                                    {
                                                        flight.carriercode = string.Empty;
                                                        flight.fltnum = string.Empty;
                                                        flight.date = flightdate.ToString("MM/dd/yyyy");
                                                        flight.fltdept = dtWeightUpdate.Rows[route]["Origin"].ToString();
                                                        flight.fltarrival = source;
                                                        Array.Resize(ref fltroute, fltroute.Length + 1);
                                                        fltroute[fltroute.Length - 1] = flight;

                                                        if (source != dtWeightUpdate.Rows[route]["Destination"].ToString())
                                                        {
                                                            flight.carriercode = flightnum.Substring(0, 2);
                                                            flight.fltnum = flightnum;
                                                            flight.date = flightdate.ToString("MM/dd/yyyy");
                                                            flight.fltdept = source;
                                                            flight.fltarrival = dest;
                                                            Array.Resize(ref fltroute, fltroute.Length + 1);
                                                            fltroute[fltroute.Length - 1] = flight;
                                                        }

                                                        if (dest != dtWeightUpdate.Rows[route]["destination"].ToString())
                                                        {
                                                            flight.carriercode = string.Empty;
                                                            flight.fltnum = string.Empty;
                                                            flight.date = flightdate.ToString("MM/dd/yyyy");
                                                            flight.fltdept = dest;
                                                            flight.fltarrival = dtWeightUpdate.Rows[route]["destination"].ToString();
                                                            Array.Resize(ref fltroute, fltroute.Length + 1);
                                                            fltroute[fltroute.Length - 1] = flight;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (dtWeightUpdate.Rows[route]["Origin"].ToString() == source && dest == dtWeightUpdate.Rows[route]["Destination"].ToString())
                                                    {
                                                        flight.carriercode = flightnum.Substring(0, 2);
                                                        flight.fltnum = flightnum;
                                                        flight.date = flightdate.ToString("MM/dd/yyyy");
                                                        flight.fltdept = dtWeightUpdate.Rows[route]["Origin"].ToString();
                                                        flight.fltarrival = dest;
                                                        Array.Resize(ref fltroute, fltroute.Length + 1);
                                                        fltroute[fltroute.Length - 1] = flight;
                                                    }
                                                }
                                                if (fltroute.Length > 0)
                                                {
                                                    for (int row = 0; row < fltroute.Length; row++)
                                                    {
                                                        int RemainingPcs = 0, strManifestPcs = 0;
                                                        if (dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString() == "P" || dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString() == "D")
                                                        {
                                                            strManifestPcs = int.Parse(dtWeightUpdate.Rows[route]["FFMPieces"].ToString());
                                                            RemainingPcs = int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString()) - int.Parse(dtWeightUpdate.Rows[route]["FFMPieces"].ToString());
                                                        }
                                                        else
                                                        {
                                                            strManifestPcs = int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString());
                                                        }
                                                        //sqlServer = new SQLServer();
                                                        //string[] RName = new string[]
                                                        //{
                                                        //      "AWBNumber", "FltOrigin","FltDestination","FltNumber","FltDate","Status","UpdatedBy","UpdatedOn","IsFFR", "REFNo"
                                                        //      ,"date","AWBPrefix",  "MftPcs","MftWt", "ConsignmentType","FFMFlightOrigin","FFMFlightDestination","RemainingPcs"
                                                        //      ,"FFMPiecesCode","TotalAWBPcs","ManifestID", "FFMSequenceNo","ChargeableWeight","AWBOrigin", "AWBDestination","Volume","VolumeUnit"
                                                        //};
                                                        //SqlDbType[] RType = new SqlDbType[]
                                                        //{

                                                        //SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime,  SqlDbType.Bit, SqlDbType.Int
                                                        //,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.Int
                                                        //,SqlDbType.VarChar,SqlDbType.Int,SqlDbType.Int,SqlDbType.Int, SqlDbType.Decimal,SqlDbType.VarChar,SqlDbType.VarChar
                                                        //, SqlDbType.Decimal,SqlDbType.VarChar
                                                        //};
                                                        //object[] RValue = new object[]
                                                        //{
                                                        //      dtWeightUpdate.Rows[route ]["AWBNo"].ToString(), fltroute[row].fltdept, fltroute[row].fltarrival,fltroute[row].fltnum, fltroute[row].date, "C",  "XFFM",  DateTime.Now, 1,   0
                                                        //      , flightdate, dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString(),strManifestPcs,Convert.ToDecimal(dtWeightUpdate.Rows[route]["GrossWt"].ToString()),dtWeightUpdate.Rows[route]["AWbPieceCode"].ToString(), source,dest,RemainingPcs
                                                        //      , dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString(),int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString()),ManifestID,ffmSequenceNo,ChargeableWeight, dtWeightUpdate.Rows[route]["Origin"].ToString(), dtWeightUpdate.Rows[route]["Destination"].ToString()
                                                        //      , Convert.ToDecimal(dtWeightUpdate.Rows[route]["VolumeWt"].ToString().Trim() == string.Empty ? "0" : dtWeightUpdate.Rows[route]["VolumeWt"].ToString().Trim())
                                                        //      , dtWeightUpdate.Rows[route]["VolumeCode"].ToString()
                                                        //};
                                                        var RParameters = new SqlParameter[] {
                                                            new SqlParameter("@AWBNumber", SqlDbType.VarChar)         { Value = dtWeightUpdate.Rows[route ]["AWBNo"].ToString() },
                                                            new SqlParameter("@FltOrigin", SqlDbType.VarChar)        { Value = fltroute[row].fltdept },
                                                            new SqlParameter("@FltDestination", SqlDbType.VarChar)   { Value = fltroute[row].fltarrival },
                                                            new SqlParameter("@FltNumber", SqlDbType.VarChar)        { Value = fltroute[row].fltnum },
                                                            new SqlParameter("@FltDate", SqlDbType.VarChar)          { Value = fltroute[row].date },
                                                            new SqlParameter("@Status", SqlDbType.VarChar)           { Value = "C" },
                                                            new SqlParameter("@UpdatedBy", SqlDbType.VarChar)        { Value = "XFFM" },
                                                            new SqlParameter("@UpdatedOn", SqlDbType.DateTime)       { Value = DateTime.Now },
                                                            new SqlParameter("@IsFFR", SqlDbType.Bit)                { Value = 1 },
                                                            new SqlParameter("@REFNo", SqlDbType.Int)                { Value = 0 },
                                                            new SqlParameter("@date", SqlDbType.VarChar)             { Value = flightdate },
                                                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar)        { Value = dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() },
                                                            new SqlParameter("@MftPcs", SqlDbType.VarChar)          { Value = strManifestPcs },
                                                            new SqlParameter("@MftWt", SqlDbType.VarChar)           { Value = Convert.ToDecimal(dtWeightUpdate.Rows[route]["GrossWt"].ToString()) },
                                                            new SqlParameter("@ConsignmentType", SqlDbType.VarChar)  { Value = dtWeightUpdate.Rows[route]["AWbPieceCode"].ToString() },
                                                            new SqlParameter("@FFMFlightOrigin", SqlDbType.VarChar) { Value = source },
                                                            new SqlParameter("@FFMFlightDestination", SqlDbType.VarChar) { Value = dest },
                                                            new SqlParameter("@RemainingPcs", SqlDbType.Int)         { Value = RemainingPcs },
                                                            new SqlParameter("@FFMPiecesCode", SqlDbType.VarChar)    { Value = dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString () },
                                                            new SqlParameter("@TotalAWBPcs", SqlDbType.Int)         { Value = int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString()) },
                                                            new SqlParameter("@ManifestID", SqlDbType.Int)          { Value = ManifestID },
                                                            new SqlParameter("@FFMSequenceNo", SqlDbType.Int)       { Value = ffmSequenceNo },
                                                            new SqlParameter("@ChargeableWeight", SqlDbType.Decimal) { Value = ChargeableWeight },
                                                            new SqlParameter("@AWBOrigin", SqlDbType.VarChar)       { Value = dtWeightUpdate.Rows[route]["Origin"].ToString() },
                                                            new SqlParameter("@AWBDestination", SqlDbType.VarChar)  { Value = dtWeightUpdate.Rows[route]["Destination"].ToString() },
                                                            new SqlParameter("@Volume", SqlDbType.Decimal)          { Value = Convert.ToDecimal(dtWeightUpdate.Rows[route]["VolumeWt"].ToString().Trim() == string.Empty ? "0" : dtWeightUpdate.Rows[route]["VolumeWt"].ToString().Trim()) },
                                                            new SqlParameter("@VolumeUnit", SqlDbType.VarChar)      { Value = dtWeightUpdate.Rows[route]["VolumeCode"].ToString() }

                                                        };

                                                        //sqlServer = new SQLServer();
                                                        //if (!sqlServer.UpdateData("MSG_spSaveFFMAWBRoute", RName, RType, RValue))
                                                        if (!await _readWriteDao.ExecuteNonQueryAsync("MSG_spSaveFFMAWBRoute", RParameters))
                                                            //clsLog.WriteLogAzure("Error in Save AWB Route XFFM " + sqlServer.LastErrorDescription);
                                                            _logger.LogWarning("Error in Save AWB Route XFFM ");
                                                    }
                                                }
                                                #endregion Make AWB Route through XFFM Message
                                            }
                                            else
                                            {
                                                #region Update AWB Route Information
                                                ///we need to add code, if FFR contain 1 leg and XFFM contains transit route
                                                ///in this case we need to form a route.
                                                int RemainingPcs = 0, strManifestPcs = 0;
                                                if (dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString() == "P" || dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString() == "D")
                                                {
                                                    strManifestPcs = int.Parse(dtWeightUpdate.Rows[route]["FFMPieces"].ToString());
                                                    RemainingPcs = int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString()) - int.Parse(dtWeightUpdate.Rows[route]["FFMPieces"].ToString());
                                                }
                                                else
                                                {
                                                    strManifestPcs = int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString());
                                                }

                                                string[] RName = new string[]
                                                      {
                                                               "AWBPrefix","AWBNumber","FltOrigin","FltDestination","FltNumber","FltDate","Status","MftPcs","MftWt","ConsignmentType",
                                                               "UpdatedBy","UpdatedOn","RemainingPcs","FFMPiecesCode","Date","TotalAWBPcs","ManifestID", "FFMSequenceNo","ChargeableWeight","REFNo","Volume","VolumeUnit","AWBOrigin"
                                                      };
                                                //SqlDbType[] RType = new SqlDbType[]
                                                //{
                                                //       SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.VarChar,
                                                //       SqlDbType.VarChar,SqlDbType.Int,SqlDbType.Decimal,SqlDbType.VarChar,SqlDbType.VarChar, SqlDbType.DateTime,SqlDbType.Int,
                                                //       SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.Int,SqlDbType.Int,SqlDbType.Int,SqlDbType.Decimal,SqlDbType.VarChar,SqlDbType.Decimal ,SqlDbType.VarChar,SqlDbType.VarChar
                                                //};

                                                //object[] RValue = new object[]
                                                //{
                                                //         dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString(),dtWeightUpdate.Rows[route]["AWBNo"].ToString(),source, dest,flightnum,flightdate, "C",  strManifestPcs
                                                //         ,decimal.Parse(dtWeightUpdate.Rows[route]["GrossWt"].ToString()), dtWeightUpdate.Rows[route]["AWbPieceCode"].ToString()
                                                //         ,"XFFM",DateTime.Now,RemainingPcs,dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString(),flightdate
                                                //         ,int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString()),ManifestID,ffmSequenceNo,ChargeableWeight,REFNo
                                                //         ,decimal.Parse(dtWeightUpdate.Rows[route]["VolumeWt"].ToString()),dtWeightUpdate.Rows[route]["VolumeCode"].ToString(),dtWeightUpdate.Rows[route]["Origin"].ToString()
                                                // };
                                                var RValue = new SqlParameter[]
                                                {
                                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar)        { Value = dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() },
                                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar)        { Value = dtWeightUpdate.Rows[route]["AWBNo"].ToString() },
                                                    new SqlParameter("@FltOrigin", SqlDbType.VarChar)       { Value = source },
                                                    new SqlParameter("@FltDestination", SqlDbType.VarChar)  { Value = dest },
                                                    new SqlParameter("@FltNumber", SqlDbType.VarChar)       { Value = flightnum },
                                                    new SqlParameter("@FltDate", SqlDbType.VarChar)         { Value = flightdate },
                                                    new SqlParameter("@Status", SqlDbType.VarChar)          { Value = "C" },
                                                    new SqlParameter("@MftPcs", SqlDbType.Int)              { Value = strManifestPcs },
                                                    new SqlParameter("@MftWt", SqlDbType.Decimal)           { Value = decimal.Parse(dtWeightUpdate.Rows[route]["GrossWt"].ToString()) },
                                                    new SqlParameter("@ConsignmentType", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AWbPieceCode"].ToString() },
                                                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar)       { Value = "XFFM" },
                                                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime)      { Value = DateTime.Now },
                                                    new SqlParameter("@RemainingPcs", SqlDbType.Int)        { Value = RemainingPcs },
                                                    new SqlParameter("@FFMPiecesCode", SqlDbType.VarChar)   { Value = dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString() },
                                                    new SqlParameter("@Date", SqlDbType.VarChar)            { Value = flightdate },
                                                    new SqlParameter("@TotalAWBPcs", SqlDbType.Int)         { Value = int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString()) },
                                                    new SqlParameter("@ManifestID", SqlDbType.Int)          { Value = ManifestID },
                                                    new SqlParameter("@FFMSequenceNo", SqlDbType.Int)       { Value = ffmSequenceNo },
                                                    new SqlParameter("@ChargeableWeight", SqlDbType.Decimal) { Value = ChargeableWeight },
                                                    new SqlParameter("@REFNo", SqlDbType.VarChar)           { Value = REFNo },
                                                    new SqlParameter("@Volume", SqlDbType.Decimal)          { Value = decimal.Parse(dtWeightUpdate.Rows[route]["VolumeWt"].ToString()) },
                                                    new SqlParameter("@VolumeUnit", SqlDbType.VarChar)      { Value = dtWeightUpdate.Rows[route]["VolumeCode"].ToString() },
                                                    new SqlParameter("@AWBOrigin", SqlDbType.VarChar)       { Value = dtWeightUpdate.Rows[route]["Origin"].ToString() }
                                                };

                                                //sqlServer = new SQLServer();
                                                //if (!sqlServer.UpdateData("MSG_uSpUpdateAWBRouteInformationthroughFFM", RName, RType, RValue))
                                                if (!await _readWriteDao.ExecuteNonQueryAsync("MSG_uSpUpdateAWBRouteInformationthroughFFM", RValue))
                                                    //clsLog.WriteLogAzure("Error in Save AWB Route XFFM " + sqlServer.LastErrorDescription);
                                                    _logger.LogWarning("Error in Save AWB Route XFFM ");
                                                #endregion Update AWB Route Information
                                            }
                                        }
                                        #region Update Weight and Pieces in Awbsummarymaster and AWBRoutemaster
                                        //string[] PVName = new string[] { "AWBPrefix", "AWBNumber", "TotalAWBPcs", "TotalAWBWeight", "VolumeCode", "VolumeWeight", "ManifestID", "ConsignmentType", "ChargeableWeight", "SystemDate" };
                                        //object[] PValues = new object[] { dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString(), dtWeightUpdate.Rows[route]["AWBNo"].ToString(), int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString()), decimal.Parse(dtWeightUpdate.Rows[route]["GrossWt"].ToString()), dtWeightUpdate.Rows[route]["VolumeCode"].ToString(), decimal.Parse(dtWeightUpdate.Rows[route]["VolumeWt"].ToString()), ManifestID, (dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString() == "" ? "T" : dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString()), TotalChargeableWeight, DateTime.UtcNow };
                                        //SqlDbType[] sqlType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Decimal, SqlDbType.VarChar, SqlDbType.Decimal, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Decimal, SqlDbType.DateTime };
                                        var pvValues = new SqlParameter[]
                                        {
                                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar)        { Value = dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() },
                                            new SqlParameter("@AWBNumber", SqlDbType.VarChar)        { Value = dtWeightUpdate.Rows[route]["AWBNo"].ToString() },
                                            new SqlParameter("@TotalAWBPcs", SqlDbType.Int)         { Value = int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString()) },
                                            new SqlParameter("@TotalAWBWeight", SqlDbType.Decimal)   { Value = decimal.Parse(dtWeightUpdate.Rows[route]["GrossWt"].ToString()) },
                                            new SqlParameter("@VolumeCode", SqlDbType.VarChar)      { Value = dtWeightUpdate.Rows[route]["VolumeCode"].ToString() },
                                            new SqlParameter("@VolumeWeight", SqlDbType.Decimal)    { Value = decimal.Parse(dtWeightUpdate.Rows[route]["VolumeWt"].ToString()) },
                                            new SqlParameter("@ManifestID", SqlDbType.Int)          { Value = ManifestID },
                                            new SqlParameter("@ConsignmentType", SqlDbType.VarChar) { Value = (dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString() == "" ? "T" : dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString()) },
                                            new SqlParameter("@ChargeableWeight", SqlDbType.Decimal) { Value = TotalChargeableWeight },
                                            new SqlParameter("@SystemDate", SqlDbType.DateTime)     { Value = DateTime.UtcNow }
                                        };

                                        //sqlServer = new SQLServer();

                                        //sqlServer.InsertData("MSG_spUpdateTotalWeightAndPiecesfromFFM", PVName, sqlType, PValues);
                                        await _readWriteDao.ExecuteNonQueryAsync("MSG_spUpdateTotalWeightAndPiecesfromFFM", pvValues);
                                        #endregion

                                        #region Billing on Acceptance
                                        if (consinfo.Length > 0)
                                        {
                                            for (int i = 0; i < consinfo.Length; i++)
                                            {
                                                //clsLog.WriteLogAzure("Billing Entry for :" + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum);
                                                _logger.LogWarning("Billing Entry for :" + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum);
                                                #region Prepare Parameters
                                                object[] AWBInfo = new object[7];
                                                int irow = 0;
                                                AWBInfo.SetValue("AC", irow);
                                                irow++;
                                                AWBInfo.SetValue(dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() + "-" + dtWeightUpdate.Rows[route]["AWBNo"].ToString(), irow);
                                                irow++;
                                                AWBInfo.SetValue(flightnum, irow);
                                                irow++;
                                                AWBInfo.SetValue(flightdate, irow);
                                                irow++;
                                                AWBInfo.SetValue(source, irow);
                                                irow++;
                                                string UserName = strMessageFrom;
                                                AWBInfo.SetValue(UserName, irow);
                                                irow++;
                                                AWBInfo.SetValue(DateTime.UtcNow, irow);
                                                #endregion Prepare Parameters
                                                string res = "";
                                                res = await InsertAWBDataInBilling(AWBInfo);
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }
                        }
                        else
                        {
                            #region NIL Flight
                            ManifestID = await ExportManifestSummary(ffmdata.carriercode + ffmdata.fltnum, ffmdata.fltairportcode, unloadingport[k].unloadingairport, flightdate, ffmdata.aircraftregistration, REFNo);
                            if (ManifestID > 0)
                            {
                                //sqlServer = new SQLServer();
                                //ffmSequenceNo = int.Parse(ffmdata.messagesequencenum == "" ? "0" : ffmdata.messagesequencenum);
                                //string[] PStatusName = new string[] { "ManifestID", "FFMStatus", "FlightNumber", "FlightDate", "FlightOrigin", "FlightDestination", "FFMSequenceNo" };
                                //object[] PStatusValues = new object[] { ManifestID, ffmdata.endmesgcode, ffmdata.carriercode + ffmdata.fltnum, flightdate, ffmdata.fltairportcode, unloadingport[k].unloadingairport, ffmSequenceNo };
                                //SqlDbType[] sqlStatusType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int };
                                
                                var psValues = new SqlParameter[]
                                {
                                    new SqlParameter("@ManifestID", SqlDbType.VarChar)        { Value = ManifestID },
                                    new SqlParameter("@FFMStatus", SqlDbType.VarChar)        { Value = ffmdata.endmesgcode },
                                    new SqlParameter("@FlightNumber", SqlDbType.VarChar)     { Value = ffmdata.carriercode + ffmdata.fltnum },
                                    new SqlParameter("@FlightDate", SqlDbType.DateTime)      { Value = flightdate },
                                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar)     { Value = ffmdata.fltairportcode },
                                    new SqlParameter("@FlightDestination", SqlDbType.VarChar){ Value = unloadingport[k].unloadingairport },
                                    new SqlParameter("@FFMSequenceNo", SqlDbType.Int)        { Value = ffmSequenceNo }
                                };



                                //sqlServer.InsertData("MSG_uSPCheckFFMFlightStatus", PStatusName, sqlStatusType, PStatusValues);
                                await _readWriteDao.ExecuteNonQueryAsync("MSG_uSPCheckFFMFlightStatus", psValues);
                            }
                            #endregion NIL Flight
                        }
                    }
                }
                #region ULD
                string origin = "";
                string destination = "";
                string movement = "OUT";
                string loading = "";

                origin = ffmdata.fltairportcode;

                for (int i = 0; i < uld.Length; i++)
                {
                    try
                    {
                        int refdest = int.Parse(uld[i].portsequence) - 1;
                        destination = unloadingport[refdest].unloadingairport;
                    }
                    catch (Exception ex)
                    {
                        //clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                    try
                    {
                        loading = uld[i].uldloadingindicator.Length > 0 ? uld[i].uldloadingindicator : "";
                    }
                    catch (Exception ex)
                    {
                        //clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                    //string[] paramname = new string[]
                    //{   "ULDNo",
                    //        "LocatedOn",
                    //        "MovType",
                    //        "CargoIndic",
                    //        "Ori",
                    //        "dest",
                    //        "FltNo",
                    //        "UpdateBy",
                    //         "IsBUP",
                    //        "AWBNumber",
                    //        "AWBPrefix"
                    //};

                    //object[] paramvalue = new object[]
                    //{   uld[i].uldno.Trim(),
                    //        flightdate,
                    //        movement,
                    //        loading,
                    //        origin,
                    //        destination,
                    //        ffmdata.carriercode+ffmdata.fltnum,
                    //        "XFFM",
                    //        uld[i].IsBUP.Length>0? Convert.ToBoolean(uld[i].IsBUP):false,
                    //       uld[i].AWBNumber.ToString(),
                    //       uld[i].AWBPrefix.ToString()

                    //};

                    //SqlDbType[] paramtype = new SqlDbType[]
                    //{
                    //        SqlDbType.NVarChar,
                    //         SqlDbType.DateTime,
                    //         SqlDbType.NVarChar,
                    //         SqlDbType.NVarChar,
                    //         SqlDbType.NVarChar,
                    //         SqlDbType.NVarChar,
                    //        SqlDbType.NVarChar,
                    //        SqlDbType.NVarChar,
                    //         SqlDbType.Bit,
                    //       SqlDbType.VarChar,
                    //       SqlDbType.VarChar
                    //};
                    var paramvalue = new SqlParameter[]
                    {
                        new SqlParameter("@ULDNo", SqlDbType.NVarChar)        { Value = uld[i].uldno.Trim() },
                        new SqlParameter("@LocatedOn", SqlDbType.DateTime)    { Value = flightdate },
                        new SqlParameter("@MovType", SqlDbType.NVarChar)      { Value = movement },
                        new SqlParameter("@CargoIndic", SqlDbType.NVarChar)   { Value = loading },
                        new SqlParameter("@Ori", SqlDbType.NVarChar)          { Value = origin },
                        new SqlParameter("@dest", SqlDbType.NVarChar)         { Value = destination },
                        new SqlParameter("@FltNo", SqlDbType.NVarChar)       { Value = ffmdata.carriercode + ffmdata.fltnum },
                        new SqlParameter("@UpdateBy", SqlDbType.NVarChar)     { Value = "XFFM" },
                        new SqlParameter("@IsBUP", SqlDbType.Bit)             { Value = uld[i].IsBUP.Length > 0 ? Convert.ToBoolean(uld[i].IsBUP) : false },
                        new SqlParameter("@AWBNumber", SqlDbType.VarChar)     { Value = uld[i].AWBNumber.ToString() },
                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar)     { Value = uld[i].AWBPrefix.ToString() }
                    };
                    //sqlServer = new SQLServer();
                    string procedure = "spUpdateviaUCMMsgFFM";
                    //if (!sqlServer.InsertData(procedure, paramname, paramtype, paramvalue))
                    if (!await _readWriteDao.ExecuteNonQueryAsync(procedure, paramvalue))
                    {
                        //clsLog.WriteLogAzure("Error in XFFM ULD:" + sqlServer.LastErrorDescription);
                        _logger.LogWarning("Error in XFFM ULD:");
                    }
                }
                #endregion

                #region Billing on depart
                if (consinfo.Length > 0)
                {
                    for (int i = 0; i < consinfo.Length; i++)
                    {
                        // clsLog.WriteLogAzure("Billing Entry for :" + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum);
                        _logger.LogInformation("Billing Entry for : {0} - {1}", consinfo[i].airlineprefix, consinfo[i].awbnum);
                        #region Prepare Parameters
                        object[] AWBInfo = new object[7];
                        int irow = 0;

                        AWBInfo.SetValue("DP", irow);
                        irow++;

                        AWBInfo.SetValue(consinfo[i].airlineprefix + "-" + consinfo[i].awbnum, irow);
                        irow++;

                        AWBInfo.SetValue(flightnum, irow);
                        irow++;

                        AWBInfo.SetValue(flightdate, irow);
                        irow++;

                        AWBInfo.SetValue(source, irow);
                        irow++;

                        string UserName = strMessageFrom;
                        AWBInfo.SetValue(UserName, irow);
                        irow++;

                        AWBInfo.SetValue(DateTime.UtcNow, irow);
                        #endregion Prepare Parameters

                        string res = "";
                        ///Normal Billing
                        res = await InsertAWBDataInBilling(AWBInfo);
                    }
                }
                #endregion

                #region : Refresh Capacity :
                if ((ffmdata.carriercode + ffmdata.fltnum).Trim() != string.Empty && flightdate != null && source.Trim() != string.Empty)
                {
                    //sqlServer = new SQLServer();
                    //SqlParameter[] sqlParameter = new SqlParameter[]{
                    //new SqlParameter("@FlightID",ffmdata.carriercode + ffmdata.fltnum)
                    //    , new SqlParameter("@FlightDate",flightdate)
                    //    , new SqlParameter("@Source",source)
                    //};
                    var sqlParameter = new SqlParameter[]
                    {
                        new SqlParameter("@FlightID", SqlDbType.VarChar)    { Value = (ffmdata.carriercode + ffmdata.fltnum) },
                        new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = flightdate },
                        new SqlParameter("@Source", SqlDbType.VarChar)      { Value = source}
                    };
                    //DataSet dsRefreshCapacity = sqlServer.SelectRecords("uspRefreshCapacity", sqlParameter);
                    DataSet? dsRefreshCapacity = await _readWriteDao.SelectRecords("uspRefreshCapacity", sqlParameter);
                }
                #endregion Refresh Capacity
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }
        #endregion Public Methods

        #region :: Private Methods ::
        /// <summary>
        /// Method to decode consignment details
        /// </summary>
        /// <param name="inputstr">String containing consingment details</param>
        /// <param name="consinfo">Reference array that returns the consignment info</param>
        /// <param name="awbprefix">AWB Prefix</param>
        /// <param name="awbnumber">AWB Number</param>
        private void DecodeConsigmentDetails(string inputstr, ref MessageData.consignmnetinfo[] consinfo, ref string awbprefix, ref string awbnumber)
        {
            try
            {
                MessageData.consignmnetinfo consig = new MessageData.consignmnetinfo("");
                string[] msg = inputstr.Split('/');
                string[] decmes = msg[0].Split('-');

                if (decmes.Length > 0)
                    consig.airlineprefix = decmes[0];
                if (decmes.Length > 1)
                {
                    string[] sptarr = StringSplitter(decmes[1]);
                    if (sptarr.Length > 0)
                    {
                        try
                        {
                            consig.awbnum = sptarr[0];
                            if (sptarr[1].Length == 3)
                            {
                                consig.origin = "";
                                consig.dest = sptarr[1];
                            }
                            else if (sptarr[1].Length == 6)
                            {
                                consig.origin = sptarr[1].Substring(0, 3);
                                consig.dest = sptarr[1].Substring(3); ;
                            }
                        }
                        catch (Exception ex)
                        {
                            //clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }
                    }
                    else
                    {
                        try
                        {
                            consig.awbnum = decmes[1].Substring(0, decmes[1].Length - 6);
                            consig.origin = decmes[1].Substring(decmes[1].Length - 6, 3);
                            consig.dest = decmes[1].Substring(decmes[1].Length - 3, 3);
                        }
                        catch (Exception ex)
                        {
                            //clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }
                    }
                }
                //1
                if (msg[1].Length > 0)
                {
                    try
                    {
                        int k = 0;
                        char lastchr = 'A';
                        char[] arr = msg[1].ToCharArray();
                        string[] strarr = new string[arr.Length];
                        for (int j = 0; j < arr.Length; j++)
                        {
                            if ((char.IsNumber(arr[j])) || (arr[j].Equals('.')))
                            {
                                ///number                            
                                if (lastchr == 'N')
                                    k--;
                                strarr[k] = strarr[k] + arr[j].ToString();
                                lastchr = 'N';
                            }
                            if (char.IsLetter(arr[j]))
                            {
                                ///letter
                                if (lastchr == 'L')
                                    k--;
                                strarr[k] = strarr[k] + arr[j].ToString();
                                lastchr = 'L';
                            }
                            k++;
                        }
                        consig.consigntype = strarr[0];
                        consig.pcscnt = strarr[1];
                        consig.weightcode = strarr[2];
                        consig.weight = strarr[3];
                        if (consig.consigntype.Equals("T"))
                        {
                            ///total pieces
                            consig.numshp = consig.pcscnt;
                        }
                        for (k = 4; k < strarr.Length; k += 2)
                        {
                            if (strarr[k] != null)
                            {
                                if (strarr[k] == "T")
                                {
                                    consig.shpdesccode = strarr[k];
                                    consig.numshp = strarr[k + 1];
                                    k = strarr.Length + 1;
                                }
                                else if (strarr[k] == "DG")
                                {
                                    consig.densityindicator = strarr[k];
                                    consig.densitygrp = strarr[k + 1];
                                }
                                else
                                {
                                    consig.volumecode = strarr[k];
                                    consig.volumeamt = strarr[k + 1];
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                }
                if (msg.Length > 2)
                {
                    try
                    {
                        consig.manifestdesc = msg[2];
                    }
                    catch (Exception ex)
                    {
                        //clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }

                }
                if (msg.Length > 3)
                {
                    try
                    {
                        consig.splhandling = "";
                        for (int j = 3; j < msg.Length; j++)
                            consig.splhandling = consig.splhandling + msg[j] + ",";
                    }
                    catch (Exception ex)
                    {
                        //clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                }
                try
                {
                    if (unloadingportsequence.Length > 0)
                        consig.portsequence = unloadingportsequence;
                    if (uldsequencenum.Length > 0)
                        consig.uldsequence = uldsequencenum;
                }
                catch (Exception ex)
                {
                    //clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                }
                awbprefix = consig.airlineprefix;
                awbnumber = consig.awbnum;
                Array.Resize(ref consinfo, consinfo.Length + 1);
                consinfo[consinfo.Length - 1] = consig;
                awbref = consinfo.Length.ToString();
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        /// <summary>
        /// Method to club pices, weight and volume for same AWB
        /// </summary>
        /// <param name="FFMConsig">Split consignment information</param>
        /// <param name="consinfo">Clubed consignment information</param>
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
                            consinfo[j].weight = (Convert.ToDecimal(consinfo[j].weight) + Convert.ToDecimal(FFMConsig[i].weight)).ToString();
                            consinfo[j].pcscnt = (Convert.ToDecimal(consinfo[j].pcscnt) + Convert.ToDecimal(FFMConsig[i].pcscnt)).ToString();
                            consinfo[j].volumeamt = Convert.ToString(Convert.ToDecimal(consinfo[j].volumeamt) + Convert.ToDecimal(FFMConsig[i].volumeamt));
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
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        /// <summary>
        /// Method to save AWB dimensions
        /// </summary>
        /// <param name="AWBNumber">AWB Number</param>
        /// <param name="AWBPieces">AWB Pices</param>
        /// <param name="Dimensions">AWB Dimensions</param>
        /// <param name="AWBWt">AWB Weight</param>
        /// <param name="UserName">Updated by Name i.e. XFFM</param>
        /// <param name="TimeStamp">Current date time</param>
        /// <param name="IsCreate">To create dimentions or not</param>
        /// <param name="AWBPrefix">AWB Prefix</param>
        /// <returns></returns>
        private async Task<DataSet> GenertateAWBDimensions(string AWBNumber, int AWBPieces, DataSet Dimensions, decimal AWBWt, string UserName, DateTime TimeStamp, bool IsCreate, string AWBPrefix)
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
                var PName = new SqlParameter[]
                {
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar)   { Value = AWBNumber },
                    new SqlParameter("@Pieces", SqlDbType.Int)          { Value = AWBPieces },
                    new SqlParameter("@PieceInfo", SqlDbType.VarChar)   { Value = strDimensions.ToString() },
                    new SqlParameter("@UserName", SqlDbType.VarChar)    { Value = UserName },
                    new SqlParameter("@TimeStamp", SqlDbType.DateTime)  { Value = TimeStamp },
                    new SqlParameter("@IsCreate", SqlDbType.Bit)        { Value = IsCreate },
                    new SqlParameter("@AWBWeight", SqlDbType.Decimal)   { Value = AWBWt },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar)   { Value = AWBPrefix }
                };
                //ds = da.SelectRecords("sp_StoreCourierDetails", PName, PValue, PType);
                ds = await _readWriteDao.SelectRecords("sp_StoreCourierDetails", PName);
                PName = null;
                //PValue = null;
                //PType = null;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                ds = null;
            }
            //finally
            //{
            //    //da = null;
            //}
            return ds;
        }

        /// <summary>
        /// Methdo to save the flight manifest summary
        /// </summary>
        /// <param name="FlightNo">Flight number</param>
        /// <param name="POL">Point of loading</param>
        /// <param name="POU">Point of unloading</param>
        /// <param name="FltDate">Flight date</param>
        /// <param name="AircraftRegistration">Aircraft ragistration</param>
        /// <param name="REFNo">Message SrNo from tblInbox</param>
        /// <returns>Manifest ID</returns>
        private async Task<int> ExportManifestSummary(string FlightNo, string POL, string POU, DateTime FltDate, string AircraftRegistration, int REFNo)
        {
            int ID = 0;
            try
            {
                //SQLServer slqServer = new SQLServer();
                //string[] param = { "FLTno", "POL", "POU", "FLTDate", "TailNo", "REFNo" };
                //SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.Int };
                //object[] values = { FlightNo, POL, POU, FltDate, AircraftRegistration, REFNo };
                var param = new SqlParameter[]
                 {
                    new SqlParameter("@FLTno", SqlDbType.VarChar)        { Value = FlightNo },
                    new SqlParameter("@POL", SqlDbType.VarChar)          { Value = POL },
                    new SqlParameter("@POU", SqlDbType.VarChar)          { Value = POU },
                    new SqlParameter("@FLTDate", SqlDbType.DateTime)     { Value = FltDate },
                    new SqlParameter("@TailNo", SqlDbType.VarChar)       { Value = AircraftRegistration },
                    new SqlParameter("@REFNo", SqlDbType.Int)           { Value = REFNo }
                 };

                //ID = slqServer.GetIntegerByProcedure("spExpManifestSummaryFFM", param, values, sqldbtypes);
                ID = await _readWriteDao.GetIntegerByProcedureAsync("spExpManifestSummaryFFM", param);


                if (ID < 1)
                {
                    //clsLog.WriteLogAzure("Error saving ExportFFM:" + slqServer.LastErrorDescription);
                    _logger.LogWarning("Error saving ExportFFM:");
                }
            }
            catch (Exception ex)
            {
                ////clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Exception in ExportManifestSummary");
                ID = 0;
            }
            return ID;
        }

        /// <summary>
        /// Method to save the flight manifest details
        /// </summary>
        /// <param name="POL">Point of loading</param>
        /// <param name="POU">Point of uploading</param>
        /// <param name="ORG">Flight origin</param>
        /// <param name="DES">Flight destination</param>
        /// <param name="AWBno">AWB Number</param>
        /// <param name="SCC"></param>
        /// <param name="VOL">Volume</param>
        /// <param name="PCS">Pices</param>
        /// <param name="WGT">Weight</param>
        /// <param name="Desc">Manifest descroption</param>
        /// <param name="FltDate">Flight date</param>
        /// <param name="ManifestID">Flight manifest ID</param>
        /// <returns>If record saved successfully returns true</returns>
        /// No reference found for this method, commented out.
        //private bool ExportManifestDetails(string POL, string POU, string ORG, string DES, string AWBno, string SCC, string VOL, string PCS, string WGT, string Desc, DateTime FltDate, int ManifestID)
        //{
        //    bool res;
        //    try
        //    {
        //        SQLServer db = new SQLServer();
        //        string[] param = { "POL", "POU", "ORG", "DES", "AWBno", "SCC", "VOL", "PCS", "WGT", "Desc", "FLTDate", "ManifestID" };
        //        SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Int };
        //        object[] values = { POL, POU, ORG, DES, AWBno, SCC, VOL, PCS, WGT, Desc, FltDate, ManifestID };
        //        if (db.InsertData("spExpManifestDetailsFFM", param, sqldbtypes, values))
        //        {
        //            res = true;
        //        }
        //        else
        //        {
        //            clsLog.WriteLogAzure("Failes ManifDetails Save:" + AWBno + " Error: " + db.LastErrorDescription);
        //            res = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //        res = false;
        //    }
        //    return res;
        //}

        /// <summary>
        /// Method to save the flight manifest details
        /// </summary>
        /// <param name="ULDNo">ULD number</param>
        /// <param name="POL">Point of loading</param>
        /// <param name="POU">Point of uploading</param>
        /// <param name="ORG">Flight origin</param>
        /// <param name="DES">Flight destination</param>
        /// <param name="AWBno">AWB Number</param>
        /// <param name="SCC"></param>
        /// <param name="VOL">Volume</param>
        /// <param name="PCS">Manifested Pices</param>
        /// <param name="WGT">Manifisted Weight</param>
        /// <param name="Desc">Manifest descroption</param>
        /// <param name="FltDate">Flight date</param>
        /// <param name="ManifestID">Flight manifest ID</param>
        /// <param name="FlightNo">Flight number</param>
        /// <param name="AWBPrefix">AWB Prefix</param>
        /// <param name="weightcode">Weight code</param>
        /// <param name="fffmSequenceSNo"></param>
        /// <param name="AwbPcs">AWB Pices</param>
        /// <param name="REFNo">Message SrNo from tblInbox</param>
        /// <returns>If record saved successfully returns true</returns>
        private async Task<bool> ExportManifestDetails(string ULDNo, string POL, string POU, string ORG, string DES, string AWBno, string SCC, string VOL, string PCS, string WGT, string Desc, DateTime FltDate, int ManifestID, string FlightNo, string AWBPrefix, string weightcode, int fffmSequenceSNo, int AwbPcs, int REFNo = 0, bool IsBUP = false)
        {
            bool res;
            try
            {
                //SQLServer db = new SQLServer();
                //string[] param = { "POL", "POU", "ORG", "DES", "AWBno", "SCC", "VOL", "PCS", "WGT", "Desc", "FLTDate", "ManifestID", "AWBPrefix", "FlightNo", "ULDNo", "TotalAWBPcs", "UOM", "FFMSequenceSNo", "REFNo", "IsBUP" };
                //SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.Bit };
                //object[] values = { POL, POU, ORG, DES, AWBno, SCC, VOL, PCS, WGT, Desc, FltDate, ManifestID, AWBPrefix, FlightNo, ULDNo, AwbPcs, weightcode, fffmSequenceSNo, REFNo, IsBUP };
                var param = new SqlParameter[] {
                    new SqlParameter("@POL", SqlDbType.VarChar)        { Value = POL },
                    new SqlParameter("@POU", SqlDbType.VarChar)        { Value = POU },
                    new SqlParameter("@ORG", SqlDbType.VarChar)        { Value = ORG },
                    new SqlParameter("@DES", SqlDbType.VarChar)        { Value = DES },
                    new SqlParameter("@AWBno", SqlDbType.VarChar)      { Value = AWBno },
                    new SqlParameter("@SCC", SqlDbType.VarChar)        { Value = SCC },
                    new SqlParameter("@VOL", SqlDbType.VarChar)        { Value = VOL },
                    new SqlParameter("@PCS", SqlDbType.VarChar)        { Value = PCS },
                    new SqlParameter("@WGT", SqlDbType.VarChar)        { Value = WGT },
                    new SqlParameter("@Desc", SqlDbType.VarChar)       { Value = Desc },
                    new SqlParameter("@FLTDate", SqlDbType.DateTime)   { Value = FltDate },
                    new SqlParameter("@ManifestID", SqlDbType.Int)     { Value = ManifestID },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar)  { Value = AWBPrefix },
                    new SqlParameter("@FlightNo", SqlDbType.VarChar)   { Value = FlightNo },
                    new SqlParameter("@ULDNo", SqlDbType.VarChar)      { Value = ULDNo },
                    new SqlParameter("@TotalAWBPcs", SqlDbType.Int)    { Value = AwbPcs },
                    new SqlParameter("@UOM", SqlDbType.VarChar)        { Value = weightcode },
                    new SqlParameter("@FFMSequenceSNo", SqlDbType.Int) { Value = fffmSequenceSNo },
                    new SqlParameter("@REFNo", SqlDbType.Int)          { Value = REFNo },
                    new SqlParameter("@IsBUP", SqlDbType.Bit)          { Value = IsBUP }
                };

                //if (db.InsertData("spExpManifestDetailsFFM", param, sqldbtypes, values))
                if (await _readWriteDao.ExecuteNonQueryAsync("spExpManifestDetailsFFM", param))
                    res = true;
                else
                {
                    //clsLog.WriteLogAzure("Failes ManifDetails Save:" + AWBno + " Error: " + db.LastErrorDescription);
                    _logger.LogWarning("Failes ManifDetails Save: {0}" , AWBno);
                    res = false;
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                res = false;
            }
            return res;
        }

        private async Task<bool> ExportManifestULDAWBAssociation(string ULDNo, string POL, string POU, string ORG, string DES, string AWBno, string SCC, string VOL, string PCS, string WGT, string Desc, string FltDate, int ManifestID, string awbprefix, string flightno, string BkdPcs, string BkdWt, string ConsignmentType)
        {
            bool res;
            try
            {
                //SQLServer db = new SQLServer();
                //string[] param = { "ULDNo", "POL", "POU", "ORG", "DES", "AWBno", "SCC", "VOL", "PCS", "WGT", "Desc", "FLTDate", "ManifestID", "AWBPrefix", "FlightNo", "BkdPcs", "BkdWt", "Source", "ConsignmentType" };
                //SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                //                             SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.BigInt, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar };
                //object[] values = { ULDNo, POL, POU, ORG, DES, AWBno, SCC, VOL, PCS, WGT, Desc, FltDate, ManifestID, awbprefix, flightno, BkdPcs, BkdWt, "M", ConsignmentType };
                var param = new SqlParameter[]
                {
                    new SqlParameter("@ULDNo", SqlDbType.VarChar)        { Value = ULDNo },
                    new SqlParameter("@POL", SqlDbType.VarChar)          { Value = POL },
                    new SqlParameter("@POU", SqlDbType.VarChar)          { Value = POU },
                    new SqlParameter("@ORG", SqlDbType.VarChar)          { Value = ORG },
                    new SqlParameter("@DES", SqlDbType.VarChar)          { Value = DES },
                    new SqlParameter("@AWBno", SqlDbType.VarChar)        { Value = AWBno },
                    new SqlParameter("@SCC", SqlDbType.VarChar)          { Value = SCC },
                    new SqlParameter("@VOL", SqlDbType.VarChar)          { Value = VOL },
                    new SqlParameter("@PCS", SqlDbType.VarChar)          { Value = PCS },
                    new SqlParameter("@WGT", SqlDbType.VarChar)          { Value = WGT },
                    new SqlParameter("@Desc", SqlDbType.VarChar)         { Value = Desc },
                    new SqlParameter("@FLTDate", SqlDbType.VarChar)     { Value = FltDate },
                    new SqlParameter("@ManifestID", SqlDbType.BigInt)    { Value = ManifestID },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar)    { Value = awbprefix },
                    new SqlParameter("@FlightNo", SqlDbType.VarChar)     { Value = flightno },
                    new SqlParameter("@BkdPcs", SqlDbType.VarChar)       { Value = BkdPcs },
                    new SqlParameter("@BkdWt", SqlDbType.VarChar)        { Value = BkdWt },
                    new SqlParameter("@Source", SqlDbType.VarChar)       { Value = "M" },
                    new SqlParameter("@ConsignmentType", SqlDbType.VarChar) { Value = ConsignmentType }
                };

                //if (db.InsertData("spExpManifestULDAWBFFM", param, sqldbtypes, values))
                if (await _readWriteDao.ExecuteNonQueryAsync("spExpManifestULDAWBFFM", param))
                {
                    res = true;
                }
                else
                {
                    //clsLog.WriteLog("Failes ManifDetails Save:" + AWBno + " Error: " + db.LastErrorDescription);
                    _logger.LogWarning("Failes ManifDetails Save: {0}" , AWBno);

                    res = false;
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                res = false;
            }
            return res;
        }

        private async Task<bool> ExportManifestULDAWBAssociation(string ULDNo, string POL, string POU, string ORG, string DES, string AWBno, string SCC, string VOL, string PCS, string WGT, string Desc, DateTime FltDate, int ManifestID, string awbprefix, string flightno, string BkdPcs, string BkdWt, string ConsignmentType, int TotalAWBPcs, int ffmSequenceSNo, string WeightCode, int REFNo = 0, bool IsBUP = false)
        {
            bool res;
            try
            {
                //SQLServer db = new SQLServer();
                //string[] param = { "ULDNo", "POL", "POU", "ORG", "DES", "AWBno", "SCC", "VOL", "PCS", "WGT", "Desc", "FLTDate", "ManifestID", "AWBPrefix", "FlightNo", "BkdPcs", "BkdWt", "Source", "ConsignmentType", "TotalAWBPcs", "FFMSequenceSNo", "Uom", "REFNo", "IsBUP" };
                //SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.BigInt, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Bit };
                //object[] values = { ULDNo, POL, POU, ORG, DES, AWBno, SCC, VOL, PCS, WGT, Desc, FltDate, ManifestID, awbprefix, flightno, BkdPcs, BkdWt, "M", ConsignmentType, TotalAWBPcs, ffmSequenceSNo, WeightCode, REFNo, IsBUP };
                var param = new SqlParameter[]
                {
                    new SqlParameter("@ULDNo", SqlDbType.VarChar)        { Value = ULDNo },
                    new SqlParameter("@POL", SqlDbType.VarChar)          { Value = POL },
                    new SqlParameter("@POU", SqlDbType.VarChar)          { Value = POU },
                    new SqlParameter("@ORG", SqlDbType.VarChar)          { Value = ORG },
                    new SqlParameter("@DES", SqlDbType.VarChar)          { Value = DES },
                    new SqlParameter("@AWBno", SqlDbType.VarChar)        { Value = AWBno },
                    new SqlParameter("@SCC", SqlDbType.VarChar)          { Value = SCC },
                    new SqlParameter("@VOL", SqlDbType.VarChar)          { Value = VOL },
                    new SqlParameter("@PCS", SqlDbType.VarChar)          { Value = PCS },
                    new SqlParameter("@WGT", SqlDbType.VarChar)          { Value = WGT },
                    new SqlParameter("@Desc", SqlDbType.VarChar)         { Value = Desc },
                    new SqlParameter("@FLTDate", SqlDbType.DateTime)     { Value = FltDate },
                    new SqlParameter("@ManifestID", SqlDbType.BigInt)    { Value = ManifestID },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar)    { Value = awbprefix },
                    new SqlParameter("@FlightNo", SqlDbType.VarChar)     { Value = flightno },
                    new SqlParameter("@BkdPcs", SqlDbType.VarChar)       { Value = BkdPcs },
                    new SqlParameter("@BkdWt", SqlDbType.VarChar)        { Value = BkdWt },
                    new SqlParameter("@Source", SqlDbType.VarChar)       { Value = "M" },
                    new SqlParameter("@ConsignmentType", SqlDbType.VarChar) { Value = ConsignmentType },
                    new SqlParameter("@TotalAWBPcs", SqlDbType.Int)      { Value = TotalAWBPcs },
                    new SqlParameter("@FFMSequenceSNo", SqlDbType.Int)   { Value = ffmSequenceSNo },
                    new SqlParameter("@Uom", SqlDbType.VarChar)          { Value = WeightCode },
                    new SqlParameter("@REFNo", SqlDbType.Int)            { Value = REFNo },
                    new SqlParameter("@IsBUP", SqlDbType.Bit)            { Value = IsBUP }
                };
                //if (db.InsertData("spExpManifestULDAWBFFM", param, sqldbtypes, values))
                if (await _readWriteDao.ExecuteNonQueryAsync("spExpManifestULDAWBFFM", param))
                    res = true;
                else
                {
                    //clsLog.WriteLog("Failes ManifDetails Save:" + AWBno + " Error: " + db.LastErrorDescription);
                    _logger.LogWarning("Failes ManifDetails Save: {0}" , AWBno);
                    res = false;
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                res = false;
            }
            return res;
        }
        //No reference found for this method, commented out.
        //private bool ULDawbAssociation(string FltNo, string POL, string POU, string AWBno, string PCS, string WGT, DateTime FltDate, string ULDNo)
        //{
        //    bool res;
        //    try
        //    {
        //        SQLServer db = new SQLServer();
        //        string[] param = { "ULDtripid", "ULDNo", "AWBNumber", "POL", "POU", "FltNo", "Pcs", "Wgt", "AvlPcs", "AvlWgt", "Updatedon", "Updatedby", "Status", "Manifested", "FltDate" };

        //        int _pcs = int.Parse(PCS);
        //        float _wgt = float.Parse(WGT);
        //        SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar
        //                                     , SqlDbType.Int, SqlDbType.Float, SqlDbType.Int, SqlDbType.Float,SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.Bit,SqlDbType.Bit, SqlDbType.DateTime };
        //        object[] values = { "", ULDNo, AWBno, POL, POU, FltNo, 0, 0, _pcs, _wgt, DateTime.Now, "XFFM", false, false, FltDate };


        //        if (db.InsertData("SPImpManiSaveUldAwbAssociation", param, sqldbtypes, values))
        //        {
        //            res = true;
        //        }
        //        else
        //        {
        //            clsLog.WriteLogAzure("Failes ULDAWBAssociation Save:" + AWBno + " Error: " + db.LastErrorDescription);
        //            res = false;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //        res = false;
        //    }
        //    return res;
        //}

        private async Task<string> InsertAWBDataInBilling(object[] AWBInfo)
        {
            //SQLServer da = new SQLServer();
            try
            {
                //string[] ColumnNames = new string[7];
                //SqlDbType[] DataType = new SqlDbType[7];
                //Object[] Values = new object[7];
                //int i = 0;

                //ColumnNames.SetValue("EventFlag", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("AWBPrefixNumberList", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("FlightNumber", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("FlightDate", i);
                //DataType.SetValue(SqlDbType.DateTime, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("StationCode", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("UserName", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("UpdatedOn", i);
                //DataType.SetValue(SqlDbType.DateTime, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);

                var PName = new SqlParameter[7]
                {
                    new SqlParameter("@EventFlag", SqlDbType.VarChar)            { Value = AWBInfo.GetValue(0) },
                    new SqlParameter("@AWBPrefixNumberList", SqlDbType.VarChar)  { Value = AWBInfo.GetValue(1) },
                    new SqlParameter("@FlightNumber", SqlDbType.VarChar)         { Value = AWBInfo.GetValue(2) },
                    new SqlParameter("@FlightDate", SqlDbType.DateTime)          { Value = AWBInfo.GetValue(3) },
                    new SqlParameter("@StationCode", SqlDbType.VarChar)          { Value = AWBInfo.GetValue(4) },
                    new SqlParameter("@UserName", SqlDbType.VarChar)             { Value = AWBInfo.GetValue(5) },
                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime)           { Value = AWBInfo.GetValue(6) }
                };

                //string res = da.GetStringByProcedure("USP_InsertBulkAWBDataInBilling", ColumnNames, Values, DataType);
                string res = await _readWriteDao.GetStringByProcedureAsync("USP_InsertBulkAWBDataInBilling", PName);
                return res;

            }

            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return "error";
            }
        }
        //No reference found for this method, commented out.
        //private string InsertAWBDataInInterlineInvoice(object[] AWBInfo)
        //{
        //    SQLServer da = new SQLServer();
        //    try
        //    {
        //        string[] ColumnNames = new string[4];
        //        SqlDbType[] DataType = new SqlDbType[4];
        //        Object[] Values = new object[4];
        //        int i = 0;

        //        ColumnNames.SetValue("AWBNumber", i);
        //        DataType.SetValue(SqlDbType.VarChar, i);
        //        Values.SetValue(AWBInfo.GetValue(i), i);
        //        i++;

        //        ColumnNames.SetValue("BillingFlag", i);
        //        DataType.SetValue(SqlDbType.VarChar, i);
        //        Values.SetValue(AWBInfo.GetValue(i), i);
        //        i++;

        //        ColumnNames.SetValue("UserName", i);
        //        DataType.SetValue(SqlDbType.VarChar, i);
        //        Values.SetValue(AWBInfo.GetValue(i), i);
        //        i++;

        //        ColumnNames.SetValue("UpdatedOn", i);
        //        DataType.SetValue(SqlDbType.DateTime, i);
        //        Values.SetValue(AWBInfo.GetValue(i), i);

        //        string res = da.GetStringByProcedure("SP_InsertAWBDataInInterlineInvoice", ColumnNames, Values, DataType);
        //        return res;

        //    }

        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //        return "error";
        //    }
        //}
        //No reference found for this method, commented out.
        //private string InsertAWBDataInInterlineCreditNote(object[] AWBInfo)
        //{
        //    SQLServer da = new SQLServer();
        //    try
        //    {
        //        string[] ColumnNames = new string[4];
        //        SqlDbType[] DataType = new SqlDbType[4];
        //        Object[] Values = new object[4];
        //        int i = 0;

        //        ColumnNames.SetValue("AWBNumber", i);
        //        DataType.SetValue(SqlDbType.VarChar, i);
        //        Values.SetValue(AWBInfo.GetValue(i), i);
        //        i++;

        //        ColumnNames.SetValue("BillingFlag", i);
        //        DataType.SetValue(SqlDbType.VarChar, i);
        //        Values.SetValue(AWBInfo.GetValue(i), i);
        //        i++;

        //        ColumnNames.SetValue("UserName", i);
        //        DataType.SetValue(SqlDbType.VarChar, i);
        //        Values.SetValue(AWBInfo.GetValue(i), i);
        //        i++;

        //        ColumnNames.SetValue("UpdatedOn", i);
        //        DataType.SetValue(SqlDbType.DateTime, i);
        //        Values.SetValue(AWBInfo.GetValue(i), i);

        //        string res = da.GetStringByProcedure("SP_InsertAWBDataInInterlineCreditNote", ColumnNames, Values, DataType);
        //        return res;

        //    }

        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //        return "error";
        //    }
        //}

        /// <summary>
        /// Check whether the awb is present or not
        /// </summary>
        /// <param name="AWBNum">AWB Number</param>
        /// <param name="AWBPrefix">Airline Prefix</param>
        /// <returns>Return true if AWB is present</returns>
        private async Task<bool> IsAWBPresent(string AWBNum, string AWBPrefix)
        {
            bool isAWBPresent = false;
            try
            {
                DataSet dsCheck = new DataSet();
                //SQLServer sqlServer = new SQLServer();
                //string[] pname = new string[] { "AWBNumber", "AWBPrefix" };
                //object[] values = new object[] { AWBNum, AWBPrefix };
                //SqlDbType[] ptype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
                var pname = new SqlParameter[]
                {
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWBNum },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                };
                //dsCheck = sqlServer.SelectRecords("sp_getawbdetails", pname, values, ptype);
                dsCheck = await _readWriteDao.SelectRecords("sp_getawbdetails", pname);
                if (dsCheck != null)
                {
                    if (dsCheck.Tables.Count > 0)
                    {
                        if (dsCheck.Tables[0].Rows.Count > 0)
                        {
                            if (dsCheck.Tables[0].Rows[0]["AWBNumber"].ToString().Equals(AWBNum, StringComparison.OrdinalIgnoreCase))
                            {
                                isAWBPresent = true;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return isAWBPresent;
        }
        #endregion Private Methods
        public async Task<(string Message, string CustomsName)> GenerateXFFMMessage(string flightNo, DateTime flightDate, string flightOrigin)
        {
            StringBuilder sbXFFMMessage = new StringBuilder();
            string customsName = string.Empty;
            try
            {
                //GenericFunction generalfunction = new GenericFunction();
                DataSet dsFfMessage = await GetRecordForAWBToGenerateXFFMMessage(flightNo, flightDate, flightOrigin);
                if (dsFfMessage != null && dsFfMessage.Tables.Count > 0 && dsFfMessage.Tables[0].Rows.Count > 0)
                {
                    if (dsFfMessage.Tables.Count > 6)
                    {
                        customsName = dsFfMessage.Tables[6].Rows[0]["CustomsName"].ToString();
                    }

                    XmlDocument xmlXFFM = new XmlDocument();
                    xmlXFFM.XmlResolver = null;

                    XmlSchema schema = new XmlSchema();
                    schema.Namespaces.Add("xsd", "http://www.w3.org/2001/XMLSchema");
                    schema.Namespaces.Add("rsm", "iata:flightmanifest:1");
                    schema.Namespaces.Add("ccts", "urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2");
                    schema.Namespaces.Add("udt", "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8");
                    schema.Namespaces.Add("ram", "iata:datamodel:3");
                    schema.Namespaces.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    //schema.Namespaces.Add("schemaLocation", "iata:waybill:1 Waybill_1.xsd");
                    xmlXFFM.Schemas.Add(schema);

                    XmlElement FlightManifest = xmlXFFM.CreateElement("rsm:FlightManifest");
                    FlightManifest.SetAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");
                    FlightManifest.SetAttribute("xmlns:rsm", "iata:flightmanifest:1");
                    FlightManifest.SetAttribute("xmlns:ccts", "urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2");
                    FlightManifest.SetAttribute("xmlns:udt", "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8");
                    FlightManifest.SetAttribute("xmlns:ram", "iata:datamodel:3");
                    FlightManifest.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    FlightManifest.SetAttribute("schemaLocation", "iata:flightmanifest:1 flightmanifest_1.xsd");
                    xmlXFFM.AppendChild(FlightManifest);
                    #region MessageHeaderDocument
                    if (dsFfMessage != null && dsFfMessage.Tables != null && dsFfMessage.Tables.Count > 0 && dsFfMessage.Tables[0].Rows.Count > 0)
                    {

                        XmlElement MessageHeaderDocument = xmlXFFM.CreateElement("rsm:MessageHeaderDocument");
                        FlightManifest.AppendChild(MessageHeaderDocument);

                        XmlElement MessageHeaderDocument_ID = xmlXFFM.CreateElement("ram:ID");
                        MessageHeaderDocument_ID.InnerText = Convert.ToString(dsFfMessage.Tables[0].Rows[0]["ID"]);
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_ID);

                        XmlElement MessageHeaderDocument_Name = xmlXFFM.CreateElement("ram:Name");
                        MessageHeaderDocument_Name.InnerText = Convert.ToString(dsFfMessage.Tables[0].Rows[0]["MessageName"]);
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_Name);

                        XmlElement MessageHeaderDocument_TypeCode = xmlXFFM.CreateElement("ram:TypeCode");
                        MessageHeaderDocument_TypeCode.InnerText = Convert.ToString(dsFfMessage.Tables[0].Rows[0]["MessageType"]);
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_TypeCode);

                        XmlElement MessageHeaderDocument_IssueDateTime = xmlXFFM.CreateElement("ram:IssueDateTime");
                        MessageHeaderDocument_IssueDateTime.InnerText = Convert.ToString(dsFfMessage.Tables[0].Rows[0]["MessageIssueDateTime"]);
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_IssueDateTime);

                        XmlElement MessageHeaderDocument_PurposeCode = xmlXFFM.CreateElement("ram:PurposeCode");
                        MessageHeaderDocument_PurposeCode.InnerText = Convert.ToString(dsFfMessage.Tables[0].Rows[0]["Purposeofthemessage"]);
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_PurposeCode);

                        XmlElement MessageHeaderDocument_VersionID = xmlXFFM.CreateElement("ram:VersionID");
                        MessageHeaderDocument_VersionID.InnerText = Convert.ToString(dsFfMessage.Tables[0].Rows[0]["Version"]);
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_VersionID);

                        XmlElement MessageHeaderDocument_ConversationID = xmlXFFM.CreateElement("ram:ConversationID");
                        MessageHeaderDocument_ConversationID.InnerText = Convert.ToString(dsFfMessage.Tables[0].Rows[0]["UniqureSrNo"]);
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_ConversationID);

                        XmlElement MessageHeaderDocument_SenderParty = xmlXFFM.CreateElement("ram:SenderParty");
                        //MessageHeaderDocument_SenderParty.InnerText = "";
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_SenderParty);

                        XmlElement MessageHeaderDocument_SenderParty_PrimaryID = xmlXFFM.CreateElement("ram:PrimaryID");
                        MessageHeaderDocument_SenderParty_PrimaryID.SetAttribute("schemeID", Convert.ToString(dsFfMessage.Tables[0].Rows[0]["SenderIdentification"]));
                        MessageHeaderDocument_SenderParty_PrimaryID.InnerText = Convert.ToString(dsFfMessage.Tables[0].Rows[0]["SenderQualifier"]);
                        MessageHeaderDocument_SenderParty.AppendChild(MessageHeaderDocument_SenderParty_PrimaryID);

                        XmlElement MessageHeaderDocument_RecipientParty = xmlXFFM.CreateElement("ram:RecipientParty");
                        //MessageHeaderDocument_RecipientParty.InnerText = "";
                        MessageHeaderDocument.AppendChild(MessageHeaderDocument_RecipientParty);

                        XmlElement MessageHeaderDocument_RecipientParty_PrimaryID = xmlXFFM.CreateElement("ram:PrimaryID");
                        MessageHeaderDocument_RecipientParty_PrimaryID.SetAttribute("schemeID", Convert.ToString(dsFfMessage.Tables[0].Rows[0]["RecipientQualifier"]));
                        MessageHeaderDocument_RecipientParty_PrimaryID.InnerText = Convert.ToString(dsFfMessage.Tables[0].Rows[0]["RecipientAddress"]);
                        MessageHeaderDocument_RecipientParty.AppendChild(MessageHeaderDocument_RecipientParty_PrimaryID);
                    }
                    #endregion MessageHeaderDocument
                    #region BusinessHeaderDocument
                    XmlElement BusinessHeaderDocument = xmlXFFM.CreateElement("rsm:BusinessHeaderDocument");
                    FlightManifest.AppendChild(BusinessHeaderDocument);

                    XmlElement BusinessHeaderDocument_ID = xmlXFFM.CreateElement("ram:ID");
                    BusinessHeaderDocument_ID.InnerText = Convert.ToString(dsFfMessage.Tables[1].Rows[0]["FlightInformation"]);
                    BusinessHeaderDocument.AppendChild(BusinessHeaderDocument_ID);

                    #region IncludedHeaderNote
                    XmlElement IncludedHeaderNote = xmlXFFM.CreateElement("ram:IncludedHeaderNote");
                    BusinessHeaderDocument.AppendChild(IncludedHeaderNote);

                    XmlElement IncludedHeaderNote_ContentCode = xmlXFFM.CreateElement("ram:ContentCode");
                    IncludedHeaderNote_ContentCode.InnerText = Convert.ToString(dsFfMessage.Tables[1].Rows[0]["HeaderNote"]);
                    IncludedHeaderNote.AppendChild(IncludedHeaderNote_ContentCode);

                    XmlElement IncludedHeaderNote_Content = xmlXFFM.CreateElement("ram:Content");
                    IncludedHeaderNote_Content.InnerText = Convert.ToString(dsFfMessage.Tables[1].Rows[0]["HeaderNoteText"]);
                    IncludedHeaderNote.AppendChild(IncludedHeaderNote_Content);

                    #endregion
                    #endregion

                    #region LogisticsTransportMovement
                    XmlElement LogisticsTransportMovement = xmlXFFM.CreateElement("rsm:LogisticsTransportMovement");
                    FlightManifest.AppendChild(LogisticsTransportMovement);

                    XmlElement LogisticsTransportMovement_StageCode = xmlXFFM.CreateElement("ram:StageCode");
                    LogisticsTransportMovement_StageCode.InnerText = Convert.ToString(dsFfMessage.Tables[1].Rows[0]["ModeOfTransport"]);
                    LogisticsTransportMovement.AppendChild(LogisticsTransportMovement_StageCode);

                    XmlElement LogisticsTransportMovement_ModeCode = xmlXFFM.CreateElement("ram:ModeCode");
                    LogisticsTransportMovement_ModeCode.InnerText = Convert.ToString(dsFfMessage.Tables[1].Rows[0]["TransportModeCode"]);
                    LogisticsTransportMovement.AppendChild(LogisticsTransportMovement_ModeCode);

                    XmlElement LogisticsTransportMovement_Mode = xmlXFFM.CreateElement("ram:Mode");
                    LogisticsTransportMovement_Mode.InnerText = Convert.ToString(dsFfMessage.Tables[1].Rows[0]["AirTransportMode"]);
                    LogisticsTransportMovement.AppendChild(LogisticsTransportMovement_Mode);

                    XmlElement LogisticsTransportMovement_ID = xmlXFFM.CreateElement("ram:ID");
                    LogisticsTransportMovement_ID.InnerText = Convert.ToString(dsFfMessage.Tables[1].Rows[0]["TransportID"]);
                    LogisticsTransportMovement.AppendChild(LogisticsTransportMovement_ID);

                    XmlElement LogisticsTransportMovement_SequenceNumeric = xmlXFFM.CreateElement("ram:SequenceNumeric");
                    LogisticsTransportMovement_SequenceNumeric.InnerText = Convert.ToString(dsFfMessage.Tables[1].Rows[0]["SeqOfTheTransport"]);
                    LogisticsTransportMovement.AppendChild(LogisticsTransportMovement_SequenceNumeric);

                    XmlElement LogisticsTransportMovement_TotalGrossWeightMeasure = xmlXFFM.CreateElement("ram:TotalGrossWeightMeasure");
                    //LogisticsTransportMovement_TotalGrossWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsFfMessage.Tables[1].Rows[0]["MeasureUnit"]));
                    LogisticsTransportMovement_TotalGrossWeightMeasure.InnerText = Convert.ToString(dsFfMessage.Tables[1].Rows[0]["FlightLodedGrossWeigtht"]);
                    LogisticsTransportMovement.AppendChild(LogisticsTransportMovement_TotalGrossWeightMeasure);

                    XmlElement LogisticsTransportMovement_TotalGrossVolumeMeasure = xmlXFFM.CreateElement("ram:TotalGrossVolumeMeasure");
                    //LogisticsTransportMovement_TotalGrossVolumeMeasure.SetAttribute("unitCode", Convert.ToString(dsFfMessage.Tables[1].Rows[0]["VolumeMeasureUnit"]));
                    LogisticsTransportMovement_TotalGrossVolumeMeasure.InnerText = Convert.ToString(dsFfMessage.Tables[1].Rows[0]["FlightLodedVolWeigtht"]);
                    LogisticsTransportMovement.AppendChild(LogisticsTransportMovement_TotalGrossVolumeMeasure);


                    XmlElement LogisticsTransportMovement_TotalPackageQuantity = xmlXFFM.CreateElement("ram:TotalPackageQuantity");
                    LogisticsTransportMovement_TotalPackageQuantity.InnerText = Convert.ToString(dsFfMessage.Tables[1].Rows[0]["NoofPackages"]);
                    LogisticsTransportMovement.AppendChild(LogisticsTransportMovement_TotalPackageQuantity);

                    XmlElement LogisticsTransportMovement_TotalPieceQuantity = xmlXFFM.CreateElement("ram:TotalPieceQuantity");
                    LogisticsTransportMovement_TotalPieceQuantity.InnerText = Convert.ToString(dsFfMessage.Tables[1].Rows[0]["TotalFLightPcs"]);
                    LogisticsTransportMovement.AppendChild(LogisticsTransportMovement_TotalPieceQuantity);

                    XmlElement MasterResponsibleTransportPerson = xmlXFFM.CreateElement("ram:MasterResponsibleTransportPerson");
                    LogisticsTransportMovement.AppendChild(MasterResponsibleTransportPerson);

                    XmlElement MasterResponsibleTransportPerson_Name = xmlXFFM.CreateElement("ram:Name");
                    MasterResponsibleTransportPerson_Name.InnerText = Convert.ToString(dsFfMessage.Tables[1].Rows[0]["NameofCaptain"]);
                    MasterResponsibleTransportPerson.AppendChild(MasterResponsibleTransportPerson_Name);

                    XmlElement UsedLogisticsTransportMeans = xmlXFFM.CreateElement("ram:UsedLogisticsTransportMeans");
                    LogisticsTransportMovement.AppendChild(UsedLogisticsTransportMeans);

                    XmlElement UsedLogisticsTransportMeans_Name = xmlXFFM.CreateElement("ram:Name");
                    UsedLogisticsTransportMeans_Name.InnerText = Convert.ToString(dsFfMessage.Tables[1].Rows[0]["TransportName"]);
                    UsedLogisticsTransportMeans.AppendChild(UsedLogisticsTransportMeans_Name);

                    XmlElement RegistrationCountry = xmlXFFM.CreateElement("ram:RegistrationCountry");
                    UsedLogisticsTransportMeans.AppendChild(RegistrationCountry);


                    XmlElement RegistrationCountry_ID = xmlXFFM.CreateElement("ram:ID");
                    RegistrationCountry_ID.InnerText = Convert.ToString(dsFfMessage.Tables[1].Rows[0]["AirCraftRegistrationCountryCode"]);
                    RegistrationCountry.AppendChild(RegistrationCountry_ID);

                    #region DepartureEvent
                    XmlElement DepartureEvent = xmlXFFM.CreateElement("ram:DepartureEvent");
                    LogisticsTransportMovement.AppendChild(DepartureEvent);

                    XmlElement DepartureEvent_DepartureOccurrenceDateTime = xmlXFFM.CreateElement("ram:DepartureOccurrenceDateTime");
                    DepartureEvent_DepartureOccurrenceDateTime.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["DepartureOccurrenceDateTime"]);
                    DepartureEvent.AppendChild(DepartureEvent_DepartureOccurrenceDateTime);

                    XmlElement DepartureEvent_DepartureDateTimeTypeCode = xmlXFFM.CreateElement("ram:DepartureDateTimeTypeCode");
                    DepartureEvent_DepartureDateTimeTypeCode.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["DepartureDateTimeTypeCode"]);
                    DepartureEvent.AppendChild(DepartureEvent_DepartureDateTimeTypeCode);

                    XmlElement OccurrenceDepartureLocation = xmlXFFM.CreateElement("ram:OccurrenceDepartureLocation");
                    DepartureEvent.AppendChild(OccurrenceDepartureLocation);

                    XmlElement OccurrenceDepartureLocation_ID = xmlXFFM.CreateElement("ram:ID");
                    OccurrenceDepartureLocation_ID.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["DepartureAirportCode"]);
                    OccurrenceDepartureLocation.AppendChild(OccurrenceDepartureLocation_ID);

                    XmlElement OccurrenceDepartureLocation_Name = xmlXFFM.CreateElement("ram:Name");
                    OccurrenceDepartureLocation_Name.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["DepartureAirportName"]);
                    OccurrenceDepartureLocation.AppendChild(OccurrenceDepartureLocation_Name);

                    XmlElement OccurrenceDepartureLocation_TypeCode = xmlXFFM.CreateElement("ram:TypeCode");
                    OccurrenceDepartureLocation_TypeCode.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["DepartureTypeCode"]);
                    OccurrenceDepartureLocation.AppendChild(OccurrenceDepartureLocation_TypeCode);

                    #endregion DepartureEvent
                    #region IncludedCustomsNote
                    XmlElement IncludedCustomsNote = xmlXFFM.CreateElement("ram:IncludedCustomsNote");
                    LogisticsTransportMovement.AppendChild(IncludedCustomsNote);

                    XmlElement IncludedCustomsNote_ContentCode = xmlXFFM.CreateElement("ram:ContentCode");
                    IncludedCustomsNote_ContentCode.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["DepartureContentCode"]);
                    IncludedCustomsNote.AppendChild(IncludedCustomsNote_ContentCode);

                    XmlElement IncludedCustomsNote_Content = xmlXFFM.CreateElement("ram:Content");
                    IncludedCustomsNote_Content.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["DepartureContent"]);
                    IncludedCustomsNote.AppendChild(IncludedCustomsNote_Content);

                    XmlElement IncludedCustomsNote_SubjectCode = xmlXFFM.CreateElement("ram:SubjectCode");
                    IncludedCustomsNote_SubjectCode.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["SubjectCode"]);
                    IncludedCustomsNote.AppendChild(IncludedCustomsNote_SubjectCode);

                    XmlElement IncludedCustomsNote_CountryID = xmlXFFM.CreateElement("ram:CountryID");
                    IncludedCustomsNote_CountryID.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["DepartureCountryID"]);
                    IncludedCustomsNote.AppendChild(IncludedCustomsNote_CountryID);
                    #endregion IncludedCustomsNote

                    XmlElement RelatedConsignmentCustomsProcedure = xmlXFFM.CreateElement("ram:RelatedConsignmentCustomsProcedure");
                    LogisticsTransportMovement.AppendChild(RelatedConsignmentCustomsProcedure);

                    XmlElement RelatedConsignmentCustomsProcedure_GoodsStatusCode = xmlXFFM.CreateElement("ram:GoodsStatusCode");
                    RelatedConsignmentCustomsProcedure_GoodsStatusCode.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["GoodsStatusCode"]);
                    RelatedConsignmentCustomsProcedure.AppendChild(RelatedConsignmentCustomsProcedure_GoodsStatusCode);
                    #endregion LogisticsTransportMovement

                    #region ArrivalEvent
                    XmlElement ArrivalEvent = xmlXFFM.CreateElement("rsm:ArrivalEvent");
                    FlightManifest.AppendChild(ArrivalEvent);

                    XmlElement ArrivalOccurrenceDateTime = xmlXFFM.CreateElement("ram:ArrivalOccurrenceDateTime");
                    ArrivalOccurrenceDateTime.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["ArrivalOccurrenceDateTime"]);
                    ArrivalEvent.AppendChild(ArrivalOccurrenceDateTime);

                    XmlElement ArrivalDateTimeTypeCode = xmlXFFM.CreateElement("ram:ArrivalDateTimeTypeCode");
                    ArrivalDateTimeTypeCode.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["Actualarrivedcode"]);
                    ArrivalEvent.AppendChild(ArrivalDateTimeTypeCode);

                    XmlElement DepartureOccurrenceDateTime = xmlXFFM.CreateElement("ram:DepartureOccurrenceDateTime");
                    DepartureOccurrenceDateTime.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["DepartureOccurrenceDateTime"]);
                    ArrivalEvent.AppendChild(DepartureOccurrenceDateTime);

                    XmlElement DepartureDateTimeTypeCode = xmlXFFM.CreateElement("ram:DepartureDateTimeTypeCode");
                    DepartureDateTimeTypeCode.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["DepartureDateTimeTypeCode"]);
                    ArrivalEvent.AppendChild(DepartureDateTimeTypeCode);

                    XmlElement OccurrenceArrivalLocation = xmlXFFM.CreateElement("ram:OccurrenceArrivalLocation");
                    ArrivalEvent.AppendChild(OccurrenceArrivalLocation);

                    XmlElement OccurrenceArrivalLocation_ID = xmlXFFM.CreateElement("ram:ID");
                    OccurrenceArrivalLocation_ID.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["ArrivalCode"]);
                    OccurrenceArrivalLocation.AppendChild(OccurrenceArrivalLocation_ID);

                    XmlElement OccurrenceArrivalLocation_Name = xmlXFFM.CreateElement("ram:Name");
                    OccurrenceArrivalLocation_Name.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["ArrivalAirportName"]);
                    OccurrenceArrivalLocation.AppendChild(OccurrenceArrivalLocation_Name);

                    XmlElement OccurrenceArrivalLocation_TypeCode = xmlXFFM.CreateElement("ram:TypeCode");
                    OccurrenceArrivalLocation_TypeCode.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["ArrivalTypeCode"]);
                    OccurrenceArrivalLocation.AppendChild(OccurrenceArrivalLocation_TypeCode);

                    XmlElement OccurrenceArrivalLocation_FirstArrivalCountryID = xmlXFFM.CreateElement("ram:FirstArrivalCountryID");
                    OccurrenceArrivalLocation_FirstArrivalCountryID.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["ArrivalcountryCode"]);
                    OccurrenceArrivalLocation.AppendChild(OccurrenceArrivalLocation_FirstArrivalCountryID);

                    XmlElement AssociatedTransportCargo = xmlXFFM.CreateElement("ram:AssociatedTransportCargo");
                    ArrivalEvent.AppendChild(AssociatedTransportCargo);

                    XmlElement AssociatedTransportCargo_TypeCode = xmlXFFM.CreateElement("ram:TypeCode");
                    AssociatedTransportCargo_TypeCode.InnerText = Convert.ToString(dsFfMessage.Tables[3].Rows[0]["TypeOfCargo"]);
                    AssociatedTransportCargo.AppendChild(AssociatedTransportCargo_TypeCode);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment = xmlXFFM.CreateElement("ram:UtilizedUnitLoadTransportEquipment");
                    //AssociatedTransportCargo.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_ID = xmlXFFM.CreateElement("ram:ID");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_ID.InnerText = Convert.ToString(dsFfMessage.Tables[3].Rows[0]["ULDNo"]);
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_ID);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_GrossWeightMeasure = xmlXFFM.CreateElement("ram:GrossWeightMeasure");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_GrossWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsFfMessage.Tables[3].Rows[0]["UOM"]));
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_GrossWeightMeasure.InnerText = Convert.ToString(dsFfMessage.Tables[3].Rows[0]["FlightGrossweight"]);
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_GrossWeightMeasure);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_GrossVolumeMeasure = xmlXFFM.CreateElement("ram:GrossVolumeMeasure");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_GrossVolumeMeasure.SetAttribute("unitCode", Convert.ToString(dsFfMessage.Tables[3].Rows[0]["UOM"]));
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_GrossVolumeMeasure.InnerText = Convert.ToString(dsFfMessage.Tables[3].Rows[0]["FlightVolWeight"]);
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_GrossVolumeMeasure);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_PieceQuantity = xmlXFFM.CreateElement("ram:PieceQuantity");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_PieceQuantity.InnerText = Convert.ToString(dsFfMessage.Tables[3].Rows[0]["PieceQuantity"]);
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_PieceQuantity);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_BuildTypeCode = xmlXFFM.CreateElement("ram:BuildTypeCode");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_BuildTypeCode.InnerText = Convert.ToString(dsFfMessage.Tables[3].Rows[0]["BuildTypeCode"]);
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_BuildTypeCode);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_CharacteristicCode = xmlXFFM.CreateElement("ram:CharacteristicCode");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_CharacteristicCode.InnerText = Convert.ToString(dsFfMessage.Tables[3].Rows[0]["CharacteristicCode"]);
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_CharacteristicCode);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_UsedCapacityCode = xmlXFFM.CreateElement("ram:UsedCapacityCode");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_UsedCapacityCode.InnerText = Convert.ToString(dsFfMessage.Tables[3].Rows[0]["UsedCapacityCode"]);
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_UsedCapacityCode);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OperationalStatusCode = xmlXFFM.CreateElement("ram:OperationalStatusCode");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OperationalStatusCode.InnerText = Convert.ToString(dsFfMessage.Tables[3].Rows[0]["OperationalStatusCode"]);
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OperationalStatusCode);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_LoadingRemark = xmlXFFM.CreateElement("ram:LoadingRemark");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_LoadingRemark.InnerText = Convert.ToString(dsFfMessage.Tables[3].Rows[0]["LoadingRemark"]);
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_LoadingRemark);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_PositioningEvent = xmlXFFM.CreateElement("ram:PositioningEvent");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_PositioningEvent);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_PositioningEvent_OccurrencePositioningLocation = xmlXFFM.CreateElement("ram:OccurrencePositioningLocation");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_PositioningEvent.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_PositioningEvent_OccurrencePositioningLocation);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_PositioningEvent_OccurrencePositioningLocation_ID = xmlXFFM.CreateElement("ram:ID");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_PositioningEvent_OccurrencePositioningLocation_ID.InnerText = Convert.ToString(dsFfMessage.Tables[3].Rows[0]["POL"]);
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_PositioningEvent_OccurrencePositioningLocation.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_PositioningEvent_OccurrencePositioningLocation_ID);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OperatingParty = xmlXFFM.CreateElement("ram:OperatingParty");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OperatingParty);


                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OperatingParty_ID = xmlXFFM.CreateElement("ram:PrimaryID");
                    ////AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OperatingParty_ID.SetAttribute("schemeAgencyID", Convert.ToString(dsFfMessage.Tables[3].Rows[0]["ULDNo"]));
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OperatingParty_ID.InnerText = Convert.ToString(dsFfMessage.Tables[3].Rows[0]["ULDOwnerCode"]);
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OperatingParty.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OperatingParty_ID);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement = xmlXFFM.CreateElement("ram:OnCarriageTransportMovement");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_ID = xmlXFFM.CreateElement("ram:ID");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_ID.InnerText = Convert.ToString(dsFfMessage.Tables[3].Rows[0]["CarriageTransportMovementID"]);
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_ID);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_CarrierParty = xmlXFFM.CreateElement("ram:CarrierParty");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_CarrierParty);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_CarrierParty_PrimaryID = xmlXFFM.CreateElement("ram:PrimaryID");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_CarrierParty_PrimaryID.SetAttribute("schemeAgencyID", Convert.ToString(dsFfMessage.Tables[3].Rows[0]["ULDNo"]));
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_CarrierParty_PrimaryID.InnerText = Convert.ToString(dsFfMessage.Tables[3].Rows[0]["CarrierParty_Text"]);
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_CarrierParty.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_CarrierParty_PrimaryID);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_OnCarriageEvent = xmlXFFM.CreateElement("ram:OnCarriageEvent");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_OnCarriageEvent);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_OnCarriageEvent_DepartureOccurrenceDateTime = xmlXFFM.CreateElement("ram:DepartureOccurrenceDateTime");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_OnCarriageEvent_DepartureOccurrenceDateTime.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["DepartureOccurrenceDateTime"]);
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_OnCarriageEvent.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_OnCarriageEvent_DepartureOccurrenceDateTime);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_OnCarriageEvent_DepartureDateTimeTypeCode = xmlXFFM.CreateElement("ram:DepartureDateTimeTypeCode");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_OnCarriageEvent_DepartureDateTimeTypeCode.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["DepartureDateTimeTypeCode"]);
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_OnCarriageEvent.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_OnCarriageEvent_DepartureDateTimeTypeCode);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_ArrivalDestinationEvent = xmlXFFM.CreateElement("ram:ArrivalDestinationEvent");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_ArrivalDestinationEvent);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_ArrivalDestinationEvent_OccurrenceDestinationLocation = xmlXFFM.CreateElement("ram:OccurrenceDestinationLocation");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_ArrivalDestinationEvent.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_ArrivalDestinationEvent_OccurrenceDestinationLocation);

                    //XmlElement AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_ArrivalDestinationEvent_OccurrenceDestinationLocation_ID = xmlXFFM.CreateElement("ram:ID");
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_ArrivalDestinationEvent_OccurrenceDestinationLocation_ID.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["ArrivalCode"]);
                    //AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_ArrivalDestinationEvent_OccurrenceDestinationLocation.AppendChild(AssociatedTransportCargo_UtilizedUnitLoadTransportEquipment_OnCarriageTransportMovement_ArrivalDestinationEvent_OccurrenceDestinationLocation_ID);

                    if (dsFfMessage != null && dsFfMessage.Tables.Count > 4 && dsFfMessage.Tables[4].Rows.Count > 0 && Convert.ToString(dsFfMessage.Tables[3].Rows[0]["TypeOfCargo"]) != "NIL")
                    {
                        for (int i = 0; i < dsFfMessage.Tables[4].Rows.Count; i++)
                        {

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment = xmlXFFM.CreateElement("ram:IncludedMasterConsignment");
                            AssociatedTransportCargo.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_GrossWeightMeasure = xmlXFFM.CreateElement("ram:GrossWeightMeasure");
                            //AssociatedTransportCargo_IncludedMasterConsignment_GrossWeightMeasure.SetAttribute("UnitCode", Convert.ToString(dsFfMessage.Tables[4].Rows[i]["ManifestedUOM"]));
                            AssociatedTransportCargo_IncludedMasterConsignment_GrossWeightMeasure.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["ManifestedGrossWeight"]);
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_GrossWeightMeasure);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_GrossVolumeMeasure = xmlXFFM.CreateElement("ram:GrossVolumeMeasure");
                            //AssociatedTransportCargo_IncludedMasterConsignment_GrossVolumeMeasure.SetAttribute("UnitCode", Convert.ToString(dsFfMessage.Tables[4].Rows[i]["VolumeUnit"]));
                            AssociatedTransportCargo_IncludedMasterConsignment_GrossVolumeMeasure.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["VolWeight"]);
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_GrossVolumeMeasure);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_DensityGroupCode = xmlXFFM.CreateElement("ram:DensityGroupCode");
                            AssociatedTransportCargo_IncludedMasterConsignment_DensityGroupCode.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["DensityGroup"]);
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_DensityGroupCode);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_PackageQuantity = xmlXFFM.CreateElement("ram:PackageQuantity");
                            AssociatedTransportCargo_IncludedMasterConsignment_PackageQuantity.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["PackageQuantity"]);
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_PackageQuantity);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_TotalPieceQuantity = xmlXFFM.CreateElement("ram:TotalPieceQuantity");
                            AssociatedTransportCargo_IncludedMasterConsignment_TotalPieceQuantity.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["TotalPieceQuantity"]);
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_TotalPieceQuantity);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_SummaryDescription = xmlXFFM.CreateElement("ram:SummaryDescription");
                            AssociatedTransportCargo_IncludedMasterConsignment_SummaryDescription.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["SummaryDescription"]);
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_SummaryDescription);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_TransportSplitDescription = xmlXFFM.CreateElement("ram:TransportSplitDescription");
                            AssociatedTransportCargo_IncludedMasterConsignment_TransportSplitDescription.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["TransportSplitDescription"]);
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_TransportSplitDescription);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_MovementPriorityCode = xmlXFFM.CreateElement("ram:MovementPriorityCode");
                            AssociatedTransportCargo_IncludedMasterConsignment_MovementPriorityCode.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["MovementPriorityCode"]);
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_MovementPriorityCode);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_TransportContractDocument = xmlXFFM.CreateElement("ram:TransportContractDocument");
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_TransportContractDocument);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_TransportContractDocument_ID = xmlXFFM.CreateElement("ram:ID");
                            AssociatedTransportCargo_IncludedMasterConsignment_TransportContractDocument_ID.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["AWBNumber"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_TransportContractDocument.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_TransportContractDocument_ID);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_OriginLocation = xmlXFFM.CreateElement("ram:OriginLocation");
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_OriginLocation);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_OriginLocation_ID = xmlXFFM.CreateElement("ram:ID");
                            AssociatedTransportCargo_IncludedMasterConsignment_OriginLocation_ID.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["AWBOrigin"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_OriginLocation.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_OriginLocation_ID);


                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_OriginLocation_Name = xmlXFFM.CreateElement("ram:Name");
                            AssociatedTransportCargo_IncludedMasterConsignment_OriginLocation_Name.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["OriginAirportName"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_OriginLocation.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_OriginLocation_Name);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_FinalDestinationLocation = xmlXFFM.CreateElement("ram:FinalDestinationLocation");
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_FinalDestinationLocation);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_FinalDestinationLocation_ID = xmlXFFM.CreateElement("ram:ID");
                            AssociatedTransportCargo_IncludedMasterConsignment_FinalDestinationLocation_ID.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["AWBDestination"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_FinalDestinationLocation.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_FinalDestinationLocation_ID);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_FinalDestinationLocation_Name = xmlXFFM.CreateElement("ram:Name");
                            AssociatedTransportCargo_IncludedMasterConsignment_FinalDestinationLocation_Name.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["DestinationAirportName"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_FinalDestinationLocation.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_FinalDestinationLocation_Name);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_HandlingSPHInstructions = xmlXFFM.CreateElement("ram:HandlingSPHInstructions");
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_HandlingSPHInstructions);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_HandlingSPHInstructions_Description = xmlXFFM.CreateElement("ram:Description");
                            AssociatedTransportCargo_IncludedMasterConsignment_HandlingSPHInstructions_Description.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["SCHDescribtion"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_HandlingSPHInstructions.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_HandlingSPHInstructions_Description);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_HandlingSPHInstructions_DescriptionCode = xmlXFFM.CreateElement("ram:DescriptionCode");
                            AssociatedTransportCargo_IncludedMasterConsignment_HandlingSPHInstructions_DescriptionCode.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["SHCDescriptionCode"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_HandlingSPHInstructions.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_HandlingSPHInstructions_DescriptionCode);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_HandlingSSRInstructions = xmlXFFM.CreateElement("ram:HandlingSSRInstructions");
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_HandlingSSRInstructions);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_HandlingSSRInstructions_Description = xmlXFFM.CreateElement("ram:Description");
                            AssociatedTransportCargo_IncludedMasterConsignment_HandlingSSRInstructions_Description.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["HandlingInformationDescription"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_HandlingSSRInstructions.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_HandlingSSRInstructions_Description);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_HandlingSSRInstructions_DescriptionCode = xmlXFFM.CreateElement("ram:DescriptionCode");
                            AssociatedTransportCargo_IncludedMasterConsignment_HandlingSSRInstructions_DescriptionCode.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["HandlingInformationCode"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_HandlingSSRInstructions.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_HandlingSSRInstructions_DescriptionCode);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_HandlingOSIInstructions = xmlXFFM.CreateElement("ram:HandlingOSIInstructions");
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_HandlingOSIInstructions);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_HandlingOSIInstructions_Description = xmlXFFM.CreateElement("ram:Description");
                            AssociatedTransportCargo_IncludedMasterConsignment_HandlingOSIInstructions_Description.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["HandlingOSIInstructions"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_HandlingOSIInstructions.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_HandlingOSIInstructions_Description);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_HandlingOSIInstructions_DescriptionCode = xmlXFFM.CreateElement("ram:DescriptionCode");
                            AssociatedTransportCargo_IncludedMasterConsignment_HandlingOSIInstructions_DescriptionCode.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["HandlingOSIInstructionsDescription"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_HandlingOSIInstructions.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_HandlingOSIInstructions_DescriptionCode);

                            //XmlElement AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1 = xmlXFFM.CreateElement("ram:IncludedCustomsNote1");
                            //AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1);

                            //XmlElement AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1_ContentCode = xmlXFFM.CreateElement("ram:ContentCode");
                            //AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1_ContentCode.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["ContentCode"]);
                            //AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1_ContentCode);

                            //XmlElement AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1_Content = xmlXFFM.CreateElement("ram:Content");
                            //AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1_Content.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["Content"]);
                            //AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1_Content);

                            //XmlElement AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1_SubjectCode = xmlXFFM.CreateElement("ram:SubjectCode");
                            //AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1_SubjectCode.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["SubjectCode"]);
                            //AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1_SubjectCode);

                            //XmlElement AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1_CountryID = xmlXFFM.CreateElement("ram:CountryID");
                            //AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1_CountryID.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["DestinationCountryCode"]);
                            //AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_IncludedCustomsNote1_CountryID);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_AssociatedConsignmentCustomsProcedure = xmlXFFM.CreateElement("ram:AssociatedConsignmentCustomsProcedure");
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_AssociatedConsignmentCustomsProcedure);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_AssociatedConsignmentCustomsProcedure_GoodsStatusCode = xmlXFFM.CreateElement("ram:GoodsStatusCode");
                            AssociatedTransportCargo_IncludedMasterConsignment_AssociatedConsignmentCustomsProcedure_GoodsStatusCode.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["GoodsStatusCode"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_AssociatedConsignmentCustomsProcedure.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_AssociatedConsignmentCustomsProcedure_GoodsStatusCode);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage = xmlXFFM.CreateElement("ram:TransportLogisticsPackage");
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_ItemQuantity = xmlXFFM.CreateElement("ram:ItemQuantity");
                            AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_ItemQuantity.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["TransportLogisticsPackageAcceptedPCs"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_ItemQuantity);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_GrossWeightMeasure = xmlXFFM.CreateElement("ram:GrossWeightMeasure");
                            //AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_GrossWeightMeasure.SetAttribute("UnitCode", Convert.ToString(dsFfMessage.Tables[4].Rows[i]["AcceptedUOM"]));
                            AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_GrossWeightMeasure.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["AcceptedGrossWeigh"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_GrossWeightMeasure);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension = xmlXFFM.CreateElement("ram:LinearSpatialDimension");
                            AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension_WidthMeasure = xmlXFFM.CreateElement("ram:WidthMeasure");
                            //AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension_WidthMeasure.SetAttribute("unitCode", Convert.ToString(dsFfMessage.Tables[4].Rows[i]["MeasureUnit"]));
                            AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension_WidthMeasure.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["Width"]) == "" ? "0" : Convert.ToString(dsFfMessage.Tables[4].Rows[i]["Width"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension_WidthMeasure);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension_LengthMeasure = xmlXFFM.CreateElement("ram:LengthMeasure");
                            //AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension_LengthMeasure.SetAttribute("unitCode", Convert.ToString(dsFfMessage.Tables[4].Rows[i]["MeasureUnit"]));
                            AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension_LengthMeasure.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["Length"]) == "" ? "0" : Convert.ToString(dsFfMessage.Tables[4].Rows[i]["Length"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension_LengthMeasure);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension_HeightMeasure = xmlXFFM.CreateElement("ram:HeightMeasure");
                            //AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension_HeightMeasure.SetAttribute("unitCode", Convert.ToString(dsFfMessage.Tables[4].Rows[i]["MeasureUnit"]));
                            AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension_HeightMeasure.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["Height"]) == "" ? "0" : Convert.ToString(dsFfMessage.Tables[4].Rows[i]["Height"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_TransportLogisticsPackage_LinearSpatialDimension_HeightMeasure);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement = xmlXFFM.CreateElement("ram:OnCarriageTransportMovement");
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_ID = xmlXFFM.CreateElement("ram:ID");
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_ID.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["NextFlight"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_ID);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_CarrierParty = xmlXFFM.CreateElement("ram:CarrierParty");
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_CarrierParty);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_CarrierParty_PrimaryID = xmlXFFM.CreateElement("ram:PrimaryID");
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_CarrierParty_PrimaryID.SetAttribute("schemeAgencyID", Convert.ToString(dsFfMessage.Tables[3].Rows[0]["CarriageTransportMovementID"]));
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_CarrierParty_PrimaryID.InnerText = Convert.ToString(dsFfMessage.Tables[3].Rows[0]["CarrierParty_Text"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_CarrierParty.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_CarrierParty_PrimaryID);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_OnCarriageEvent = xmlXFFM.CreateElement("ram:OnCarriageEvent");
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_OnCarriageEvent);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_OnCarriageEvent_DepartureOccurrenceDateTime = xmlXFFM.CreateElement("ram:DepartureOccurrenceDateTime");
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_OnCarriageEvent_DepartureOccurrenceDateTime.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["DepartureOccurrenceDateTime"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_OnCarriageEvent.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_OnCarriageEvent_DepartureOccurrenceDateTime);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_OnCarriageEvent_DepartureDateTimeTypeCode = xmlXFFM.CreateElement("ram:DepartureDateTimeTypeCode");
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_OnCarriageEvent_DepartureDateTimeTypeCode.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["DepartureDateTimeTypeCode"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_OnCarriageEvent.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_OnCarriageEvent_DepartureDateTimeTypeCode);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_ArrivalDestinationEvent = xmlXFFM.CreateElement("ram:ArrivalDestinationEvent");
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_ArrivalDestinationEvent);


                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_ArrivalDestinationEvent_OccurrenceDestinationLocation = xmlXFFM.CreateElement("ram:OccurrenceDestinationLocation");
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_ArrivalDestinationEvent.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_ArrivalDestinationEvent_OccurrenceDestinationLocation);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_ArrivalDestinationEvent_OccurrenceDestinationLocation_ID = xmlXFFM.CreateElement("ram:ID");
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_ArrivalDestinationEvent_OccurrenceDestinationLocation_ID.InnerText = Convert.ToString(dsFfMessage.Tables[2].Rows[0]["ArrivalCode"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_ArrivalDestinationEvent_OccurrenceDestinationLocation.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_ArrivalDestinationEvent_OccurrenceDestinationLocation_ID);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_ArrivalDestinationEvent_OccurrenceDestinationLocation_Name = xmlXFFM.CreateElement("ram:Name");
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_ArrivalDestinationEvent_OccurrenceDestinationLocation_Name.InnerText = Convert.ToString(dsFfMessage.Tables[4].Rows[i]["OccurrenceDestinationLocation_Name"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_ArrivalDestinationEvent_OccurrenceDestinationLocation.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_OnCarriageTransportMovement_ArrivalDestinationEvent_OccurrenceDestinationLocation_Name);


                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_IncludedMasterConsignmentItem = xmlXFFM.CreateElement("ram:IncludedMasterConsignmentItem");
                            AssociatedTransportCargo_IncludedMasterConsignment.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_IncludedMasterConsignmentItem);

                            XmlElement AssociatedTransportCargo_IncludedMasterConsignment_IncludedMasterConsignmentItem_TypeCode = xmlXFFM.CreateElement("ram:TypeCode");
                            AssociatedTransportCargo_IncludedMasterConsignment_IncludedMasterConsignmentItem_TypeCode.InnerText = Convert.ToString(dsFfMessage.Tables[5].Rows[0]["CommodityCode"]);
                            AssociatedTransportCargo_IncludedMasterConsignment_IncludedMasterConsignmentItem.AppendChild(AssociatedTransportCargo_IncludedMasterConsignment_IncludedMasterConsignmentItem_TypeCode);
                        }
                    }

                    #endregion
                    sbXFFMMessage = new StringBuilder(xmlXFFM.OuterXml);
                    sbXFFMMessage.Replace("<", "<ram:");
                    sbXFFMMessage.Replace("<ram:/", "</ram:");
                    sbXFFMMessage.Replace("<ram:FlightManifest", "<rsm:FlightManifest");
                    sbXFFMMessage.Replace("</ram:FlightManifest", "</rsm:FlightManifest");

                    sbXFFMMessage.Replace("<ram:ArrivalEvent", "<rsm:ArrivalEvent");
                    sbXFFMMessage.Replace("</ram:ArrivalEvent", "</rsm:ArrivalEvent");
                    sbXFFMMessage.Replace("<ram:LogisticsTransportMovement", "<rsm:LogisticsTransportMovement");
                    sbXFFMMessage.Replace("</ram:LogisticsTransportMovement", "</rsm:LogisticsTransportMovement");

                    sbXFFMMessage.Replace("schemaLocation", "xsi:schemaLocation");

                    sbXFFMMessage.Replace("<ram:Waybill", "<rsm::FlightManifest");
                    sbXFFMMessage.Replace("</ram:Waybill", "</rsm::FlightManifest");
                    sbXFFMMessage.Replace("<ram:MessageHeaderDocument>", "<rsm:MessageHeaderDocument>");
                    sbXFFMMessage.Replace("</ram:MessageHeaderDocument>", "</rsm:MessageHeaderDocument>");
                    sbXFFMMessage.Replace("<ram:BusinessHeaderDocument>", "<rsm:BusinessHeaderDocument>");
                    sbXFFMMessage.Replace("</ram:BusinessHeaderDocument>", "</rsm:BusinessHeaderDocument>");
                    sbXFFMMessage.Replace("<ram:MasterConsignment>", "<rsm:MasterConsignment>");
                    sbXFFMMessage.Replace("</ram:MasterConsignment>", "</rsm:MasterConsignment>");

                    ///Remove the empty tags from XML
                    var document = System.Xml.Linq.XDocument.Parse(sbXFFMMessage.ToString());
                    //var emptyNodes = document.Descendants().Where(e => e.IsEmpty || String.IsNullOrWhiteSpace(e.Value));
                    //foreach (var emptyNode in emptyNodes.ToArray())
                    //{
                    //    emptyNode.Remove();
                    //}
                    sbXFFMMessage = new StringBuilder(document.ToString());
                    //sbXFFMMessage.Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
                    if (customsName.ToUpper() == "DAKAR")
                    {
                        sbXFFMMessage.Replace("<rsm:FlightManifest xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:rsm=\"iata:flightmanifest:1\" xmlns:ccts=\"urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2\" xmlns:udt=\"urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8\" xmlns:ram=\"iata:datamodel:3\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"iata:flightmanifest:1 flightmanifest_1.xsd\">", "");
                        sbXFFMMessage.Replace("<rsm:FlightManifest>", "");
                        sbXFFMMessage.Replace("<ram:FlightManifest>", "");
                        sbXFFMMessage.Replace("</rsm:FlightManifest>", "");
                        sbXFFMMessage.Replace("</ram:FlightManifest>", "");
                        sbXFFMMessage.Replace("rsm:", "");
                        sbXFFMMessage.Replace("ram:", "");
                    }
                }
                else
                {
                    sbXFFMMessage.Append("No Data available in the system to generate message.");
                }

            }
            catch (Exception ex)
            {
                sbXFFMMessage.Append("Error Occured while generating: " + ex.Message);
                // clsLog.WriteLogAzure("Error on Generate XFFMB Message Method:" + ex.ToString());
                _logger.LogError("Error on Generate XFFMB Message Method: {0}" , ex);
            }
            //return sbXFFMMessage.ToString();
            return (sbXFFMMessage?.ToString(), customsName);

        }

        private async Task<DataSet> GetRecordForAWBToGenerateXFFMMessage(string flightNo, DateTime flightDate, string flightOrigin)
        {
            DataSet? dsFfm = new DataSet();
            try
            {
                //SQLServer da = new SQLServer();
                //string[] paramname = new string[] { "FlightNo", "FlightDate", "FlightOrigin" };
                //object[] paramvalue = new object[] { flightNo, flightDate, flightOrigin };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar };
                var paramname = new SqlParameter[] {
                    new SqlParameter("FlightNo", flightNo),
                    new SqlParameter("FlightDate", flightDate),
                    new SqlParameter("FlightOrigin", flightOrigin)
                };
                //dsFfm = da.SelectRecords("Messaging.GetRecordMakeXFFMMessage", paramname, paramvalue, paramtype);
                dsFfm = await _readWriteDao.SelectRecords("Messaging.GetRecordMakeXFFMMessage", paramname);
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dsFfm;
        }
    }
}
