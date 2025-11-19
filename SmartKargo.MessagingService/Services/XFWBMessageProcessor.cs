#region XFWB Message Processor Class Description
/* XFWB Message Processor Class Description.
      * Company              :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright            :   Copyright © 2017 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose              :  XFWB Message Processor Class
      * Created By           :   Yoginath
      * Created On           :   2017-06-16
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

namespace QidWorkerRole
{
    public class XFWBMessageProcessor
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<XFWBMessageProcessor> _logger;
        private readonly FFRMessageProcessor _fFRMessageProcessor;
        private readonly GenericFunction _genericFunction;
        private readonly XFNMMessageProcessor _xFNMMessageProcessor;
        public XFWBMessageProcessor(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<XFWBMessageProcessor> logger,
            FFRMessageProcessor fFRMessageProcessor,
            GenericFunction genericFunction,
            XFNMMessageProcessor xFNMMessageProcessor
        )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _fFRMessageProcessor = fFRMessageProcessor;
            _genericFunction = genericFunction;
            _xFNMMessageProcessor = xFNMMessageProcessor;
        }
        /// <summary>
        /// Decoding Received XFWB xml message
        /// </summary>
        /// <param name="fwbmsg"></param>
        /// <param name="fwbdata"></param>
        /// <param name="fltroute"></param>
        /// <param name="fwbOtherCharge"></param>
        /// <param name="othinfoarray"></param>
        /// <param name="fwbrate"></param>
        /// <param name="custominfo"></param>
        /// <param name="objDimension"></param>
        /// <param name="objAwbBup"></param>
        /// <returns></returns>

        public bool DecodeReceiveFWBMessage(string fwbmsg, ref MessageData.fwbinfo fwbdata,
       ref MessageData.FltRoute[] fltroute,
       ref MessageData.othercharges[] fwbOtherCharge, ref MessageData.otherserviceinfo[] othinfoarray,

       ref MessageData.RateDescription[] fwbrate, ref MessageData.customsextrainfo[] custominfo,
       ref MessageData.dimensionnfo[] objDimension, ref MessageData.AWBBuildBUP[] objAwbBup, out string ErrorMsg)
        {
            bool flag = false;
            MessageData.AWBBuildBUP awbBup = new MessageData.AWBBuildBUP("");
            ErrorMsg = "";
            DataRow[] drs;
            try
            {
                string strFightNo = string.Empty;
                var fwbXmlDataSet = new DataSet();

                var tx = new StringReader(fwbmsg);
                fwbXmlDataSet.ReadXml(tx);

                if (fwbXmlDataSet.Tables.Contains("MessageHeaderDocument"))
                {
                    if (fwbXmlDataSet.Tables["MessageHeaderDocument"].Columns.Contains("VersionID"))
                    {
                        fwbdata.fwbversionnum = Convert.ToString(fwbXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"]) : "";
                    }
                }
                if (fwbXmlDataSet.Tables.Contains("MessageHeaderDocument"))
                {
                    if (fwbXmlDataSet.Tables["MessageHeaderDocument"].Columns.Contains("PurposeCode"))
                    {
                        fwbdata.fwbPurposecode = Convert.ToString(fwbXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["PurposeCode"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["PurposeCode"]) : "";
                    }

                }



                if (fwbXmlDataSet.Tables.Contains("HandlingSSRInstructions"))
                {
                    if (fwbXmlDataSet.Tables["HandlingSSRInstructions"].Columns.Contains("Description"))
                    {
                        fwbdata.handinginfo = Convert.ToString(fwbXmlDataSet.Tables["HandlingSSRInstructions"].Rows[0]["Description"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["HandlingSSRInstructions"].Rows[0]["Description"]) : "";
                        fwbdata.specialservicereq1 = Convert.ToString(fwbXmlDataSet.Tables["HandlingSSRInstructions"].Rows[0]["Description"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["HandlingSSRInstructions"].Rows[0]["Description"]) : "";

                    }
                }
                //awb consigment details       
                string awbNumber = "";
                if (fwbXmlDataSet.Tables.Contains("BusinessHeaderDocument"))
                {
                    if (fwbXmlDataSet.Tables["BusinessHeaderDocument"].Columns.Contains("ID"))
                    {
                        awbNumber = Convert.ToString(fwbXmlDataSet.Tables["BusinessHeaderDocument"].Rows[0]["ID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["BusinessHeaderDocument"].Rows[0]["ID"]) : "";
                    }
                    if (fwbXmlDataSet.Tables["BusinessHeaderDocument"].Columns.Contains("SenderAssignedID"))
                    {
                        if (fwbXmlDataSet.Tables["BusinessHeaderDocument"].Rows[0]["SenderAssignedID"].ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
                        {
                            fwbdata.senderFileref = "0";
                        }
                        else
                        {
                            fwbdata.senderFileref = Convert.ToString(fwbXmlDataSet.Tables["BusinessHeaderDocument"].Rows[0]["SenderAssignedID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["BusinessHeaderDocument"].Rows[0]["SenderAssignedID"]) : "0";

                        }
                    }
                }
                string[] decmes = awbNumber.Split('-');
                fwbdata.airlineprefix = decmes[0];
                fwbdata.awbnum = decmes[1];
                if (fwbXmlDataSet.Tables["MessageHeaderDocument"].Columns.Contains("IssueDateTime"))
                {

                    string[] Updatedon = Convert.ToString(fwbXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["IssueDateTime"]).Split('T');
                    if (Updatedon.Length > 1)
                    {

                        fwbdata.updatedondate = String.IsNullOrEmpty(Updatedon[0]) ? DateTime.UtcNow.ToShortDateString() : Updatedon[0];
                        //fwbdata.updatedontime = String.IsNullOrEmpty(Updatedon[1]) ? DateTime.UtcNow.ToShortTimeString().Substring(0,8)  : Updatedon[1];
                        fwbdata.updatedontime = String.IsNullOrEmpty(Updatedon[1]) ? DateTime.UtcNow.ToShortTimeString().Substring(0, 8) : Updatedon[1].Substring(0, 8);
                        fwbdata.Recivedontime = String.IsNullOrEmpty(Updatedon[1]) ? DateTime.UtcNow.ToShortTimeString().Substring(0, 12) : Updatedon[1].Substring(0, Updatedon[1].Length);


                    }
                    else
                    {
                        ErrorMsg = "Seprate date and time with T from IssueDateTime";
                        flag = false;
                        return flag;

                    }

                }
                if ((fwbdata.fwbPurposecode).Equals("Deletion", StringComparison.OrdinalIgnoreCase))
                {
                    flag = true;
                    ErrorMsg = "";
                    return true;


                }
                if ((fwbdata.fwbPurposecode).Equals("CANCELLED", StringComparison.OrdinalIgnoreCase))
                {
                    flag = true;
                    ErrorMsg = "";
                    return true;


                }
                //fwbdata.dest = Convert.ToString(fwbXmlDataSet.Tables["FinalDestinationLocation"].Rows[0]["ID"]);
                if (fwbXmlDataSet.Tables.Contains("IncludedHeaderNote"))
                {

                    if (fwbXmlDataSet.Tables["IncludedHeaderNote"].Columns.Contains("ContentCode"))
                    {
                        fwbdata.ContentCode = Convert.ToString(fwbXmlDataSet.Tables["IncludedHeaderNote"].Rows[0]["ContentCode"])
                            != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedHeaderNote"].Rows[0]["ContentCode"]) : "0";
                    }
                    if (fwbXmlDataSet.Tables["IncludedHeaderNote"].Columns.Contains("Content"))
                    {
                        fwbdata.Content = Convert.ToString(fwbXmlDataSet.Tables["IncludedHeaderNote"].Rows[0]["Content"])
                            != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedHeaderNote"].Rows[0]["Content"]) : "0";
                    }
                }
                if (fwbXmlDataSet.Tables.Contains("AssociatedReferenceDocument"))
                {

                    if (fwbXmlDataSet.Tables["AssociatedReferenceDocument"].Columns.Contains("IssueDateTime"))
                    {
                        string Recoverytime = Convert.ToString(fwbXmlDataSet.Tables["AssociatedReferenceDocument"].Rows[0]["IssueDateTime"]);
                        if (Recoverytime.ToString().Contains('T'))
                        {
                            string[] Updatedon = Convert.ToString(fwbXmlDataSet.Tables["AssociatedReferenceDocument"].Rows[0]["IssueDateTime"]).Split('T');
                            if (Updatedon.Length > 1)
                            {

                                fwbdata.Recoverytime = String.IsNullOrEmpty(Updatedon[1]) ? DateTime.UtcNow.ToShortTimeString().Substring(0, 8) : Updatedon[1].Substring(0, 8);
                                fwbdata.Recoverytimedate = String.IsNullOrEmpty(Updatedon[0]) ? DateTime.UtcNow.ToShortTimeString() : Updatedon[0];
                            }
                        }
                        else
                        {
                            fwbdata.Recoverytimedate = Recoverytime;

                        }


                    }

                }
                if (fwbXmlDataSet.Tables.Contains("MasterConsignment"))
                {
                    if (fwbXmlDataSet.Tables["MasterConsignment"].Columns.Contains("ProductID"))
                    {
                        if (fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["ProductID"].ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                        {
                            fwbdata.ProductID = "0";
                        }

                        else
                        {
                            fwbdata.ProductID = Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["ProductID"]) != string.Empty ?
                                Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["ProductID"]) : "0";
                        }
                    }

                    if (fwbXmlDataSet.Tables["MasterConsignment"].Columns.Contains("PackageQuantity") ||
                        fwbXmlDataSet.Tables["MasterConsignment"].Columns.Contains("TotalPieceQuantity"))
                    {
                        int PackageQuantity = 0, TotalPieceQuantity = 0;

                        if (fwbXmlDataSet.Tables["MasterConsignment"].Columns.Contains("PackageQuantity"))
                        {
                            if (fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["PackageQuantity"].ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
                            {
                                PackageQuantity = 0;

                            }
                            else
                            {
                                PackageQuantity = Convert.ToInt32(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["PackageQuantity"]) != 0 ?
                                    Convert.ToInt32(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["PackageQuantity"]) : 0;
                            }

                        }
                        if (fwbXmlDataSet.Tables["MasterConsignment"].Columns.Contains("TotalPieceQuantity"))
                        {
                            if (fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"].ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
                            {
                                TotalPieceQuantity = 0;

                            }
                            else
                            {
                                TotalPieceQuantity = Convert.ToInt32(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"]) != 0 ?
                               Convert.ToInt32(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"]) : 0;
                            }
                        }
                        if (PackageQuantity < TotalPieceQuantity)
                        { fwbdata.consigntype = "P"; }
                        else
                        { fwbdata.consigntype = "T"; }
                    }


                    if (fwbXmlDataSet.Tables["MasterConsignment"].Columns.Contains("TotalPieceQuantity"))
                    {
                        if (fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"].ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
                        {
                            fwbdata.pcscnt = "0";
                        }
                        else
                        {
                            fwbdata.pcscnt = Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"])
                            != String.Empty ? Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"]) : "0";
                        }
                    }
                    if (fwbXmlDataSet.Tables["MasterConsignment"].Columns.Contains("DensityGroupCode"))
                    {
                        if (fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["DensityGroupCode"].ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
                        {
                            fwbdata.densitygrp = "0";
                        }
                        else
                        {
                            fwbdata.densitygrp = Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["DensityGroupCode"])
                            != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["DensityGroupCode"]) : "0";
                        }

                    }
                    if (fwbXmlDataSet.Tables["MasterConsignment"].Columns.Contains("ID"))
                    {
                        fwbdata.shiprefnum = Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["ID"])
                            != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["ID"]) : "";
                    }
                    if (fwbXmlDataSet.Tables["MasterConsignment"].Columns.Contains("AdditionalID"))
                    {
                        fwbdata.supplemetryshipperinfo1 = Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["AdditionalID"])
                            != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["AdditionalID"]) : "0";
                    }

                    //fwbdata.supplemetryshipperinfo2 = Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["AdditionalID"]);

                    //Other Service Information
                    if (fwbXmlDataSet.Tables["MasterConsignment"].Columns.Contains("AssociatedReferenceID"))
                    {
                        fwbdata.othparticipentname = Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["AssociatedReferenceID"])
                            != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["AssociatedReferenceID"]) : "";
                    }
                }
                if (fwbXmlDataSet.Tables.Contains("AssociatedConsignmentCustomsProcedure"))
                {
                    if (fwbXmlDataSet.Tables["AssociatedConsignmentCustomsProcedure"].Columns.Contains("GoodsStatusCode"))
                    {
                        fwbdata.SCI = Convert.ToString(fwbXmlDataSet.Tables["AssociatedConsignmentCustomsProcedure"].Rows[0]["GoodsStatusCode"])
                            != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["AssociatedConsignmentCustomsProcedure"].Rows[0]["GoodsStatusCode"]) : "0";
                    }

                }
                //fwbdata.densityindicator = Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0][""]);
                if (fwbXmlDataSet.Tables.Contains("GrossVolumeMeasure"))
                {
                    if (fwbXmlDataSet.Tables["GrossVolumeMeasure"].Columns.Contains("unitCode"))
                    {

                        fwbdata.volumecode = Convert.ToString(fwbXmlDataSet.Tables["GrossVolumeMeasure"].Rows[0]["unitCode"])
                            != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["GrossVolumeMeasure"].Rows[0]["unitCode"]) : "0";
                    }

                    if (fwbXmlDataSet.Tables["GrossVolumeMeasure"].Columns.Contains("GrossVolumeMeasure_Text"))
                    {
                        if (fwbXmlDataSet.Tables["GrossVolumeMeasure"].Rows[0]["GrossVolumeMeasure_Text"].ToString().Equals("NULL", StringComparison.OrdinalIgnoreCase))
                        {
                            fwbdata.volumeamt = "0";
                        }
                        else
                        {
                            fwbdata.volumeamt = Convert.ToString(fwbXmlDataSet.Tables["GrossVolumeMeasure"].Rows[0]["GrossVolumeMeasure_Text"]);
                        }
                    }
                }
                //Flight Booking
                //fwbdata.fltday = Convert.ToString(fwbXmlDataSet.Tables["DepartureEvent"].Rows[0]["ScheduledOccurrenceDateTime"]);

                //Routing


                MessageData.FltRoute flight = new MessageData.FltRoute("");

                if (fwbXmlDataSet.Tables.Contains("OriginLocation"))
                {
                    if (fwbXmlDataSet.Tables["OriginLocation"].Columns.Contains("ID"))
                    {
                        fwbdata.origin = Convert.ToString(fwbXmlDataSet.Tables["OriginLocation"].Rows[0]["ID"]) != String.Empty ? Convert.ToString(fwbXmlDataSet.Tables["OriginLocation"].Rows[0]["ID"]) : "";

                        flight.fltdept = Convert.ToString(fwbXmlDataSet.Tables["OriginLocation"].Rows[0]["ID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["OriginLocation"].Rows[0]["ID"]) : "";
                    }
                }
                if (fwbXmlDataSet.Tables.Contains("FinalDestinationLocation"))
                {
                    if (fwbXmlDataSet.Tables["FinalDestinationLocation"].Columns.Contains("ID"))
                    {
                        flight.fltarrival = Convert.ToString(fwbXmlDataSet.Tables["FinalDestinationLocation"].Rows[0]["ID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["FinalDestinationLocation"].Rows[0]["ID"]) : "";
                        fwbdata.dest = Convert.ToString(fwbXmlDataSet.Tables["FinalDestinationLocation"].Rows[0]["ID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["FinalDestinationLocation"].Rows[0]["ID"]) : "";

                    }
                }
                if (fwbXmlDataSet.Tables.Contains("SpecifiedLogisticsTransportMovement"))
                {
                    for (int i = 0; i < fwbXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows.Count; i++)
                    {
                        try
                        {
                            if (fwbXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Columns.Contains("ID"))
                            {
                                fwbdata.fltnum = Convert.ToString(fwbXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[i]["ID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[i]["ID"]) : "XXXXXX";
                                flight.fltnum = fwbdata.fltnum;
                            }


                            flight.carriercode = fwbdata.fltnum.Substring(0, 2);
                            //flight.fltnum = fwbdata.fltnum.Substring(2);
                            string sfltdate = "", sfltarrivaldate = "";

                            if (fwbXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Columns.Contains("SequenceNumeric"))
                            {
                                flight.Routesquencenumber = Convert.ToString(fwbXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[i]["SequenceNumeric"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[i]["SequenceNumeric"]) : "1";
                            }
                            if (fwbXmlDataSet.Tables.Contains("UsedLogisticsTransportMeans"))
                            {
                                if (fwbXmlDataSet.Tables["UsedLogisticsTransportMeans"].Columns.Contains("Name"))
                                {
                                    flight.carriercode = fwbXmlDataSet.Tables["UsedLogisticsTransportMeans"].Rows[i]["Name"].ToString().ToUpper();
                                }
                            }

                            if (fwbXmlDataSet.Tables.Contains("DepartureEvent"))
                            {
                                if (fwbXmlDataSet.Tables["DepartureEvent"].Columns.Contains("ScheduledOccurrenceDateTime"))
                                {
                                    //fwbdata.updatedondate = String.IsNullOrEmpty(Updatedon[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : Updatedon[0];
                                    //fwbdata.updatedontime = String.IsNullOrEmpty(Updatedon[1]) ? DateTime.UtcNow.ToShortTimeString().Substring(0,8)  : Updatedon[1];
                                    //fwbdata.updatedontime = String.IsNullOrEmpty(Updatedon[1]) ? DateTime.UtcNow.ToShortTimeString().Substring(0, 8) : Updatedon[1].Substring(0, 8);

                                    sfltdate = Convert.ToString(fwbXmlDataSet.Tables["DepartureEvent"].Rows[i]["ScheduledOccurrenceDateTime"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["DepartureEvent"].Rows[i]["ScheduledOccurrenceDateTime"]) : DateTime.Now.ToString("yyyy-MM-dd");
                                    string[] fltDatesplit = sfltdate.Split('T');
                                    fwbdata.fltday = Convert.ToString(fltDatesplit[0]);
                                    flight.date = Convert.ToString(fltDatesplit[0]);
                                    flight.month = Convert.ToDateTime(fltDatesplit[0]).ToString("MMM");
                                    flight.inputdeptDatetime = String.IsNullOrEmpty(fltDatesplit[0]) && String.IsNullOrEmpty(fltDatesplit[1]) ?
                                       DateTime.UtcNow.ToString("yyyy-MM-dd") + " " + DateTime.UtcNow.ToString("HH:MM:ss") : fltDatesplit[0] + " " + fltDatesplit[1];



                                }
                                if (fwbXmlDataSet.Tables.Contains("OccurrenceDepartureLocation"))
                                {
                                    if (fwbXmlDataSet.Tables["OccurrenceDepartureLocation"].Columns.Contains("ID"))
                                    {
                                        flight.fltdept = Convert.ToString(fwbXmlDataSet.Tables["OccurrenceDepartureLocation"].Rows[i]["ID"]) !=
                                            string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["OccurrenceDepartureLocation"].Rows[i]["ID"]) : "";

                                    }
                                    else
                                    {
                                        if (fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Columns.Contains("ID"))
                                        {
                                            flight.fltdept = Convert.ToString(fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Rows[i - 1]["ID"]) !=
                                                string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Rows[i - 1]["ID"]) : "";

                                        }
                                    }
                                }
                                else
                                {
                                    if (fwbXmlDataSet.Tables.Contains("OccurrenceArrivalLocation"))
                                    {
                                        if (i == 0)
                                        {
                                            if (fwbXmlDataSet.Tables["OriginLocation"].Columns.Contains("ID"))
                                            {
                                                //flight.fltdept = Convert.ToString(fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Rows[i]["ID"]) !=
                                                //    string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Rows[i]["ID"]) : "";
                                                flight.fltdept = Convert.ToString(fwbXmlDataSet.Tables["OriginLocation"].Rows[0]["ID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["OriginLocation"].Rows[0]["ID"]) : "";

                                            }
                                        }
                                        else
                                        {
                                            if (fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Columns.Contains("ID"))
                                            {
                                                flight.fltdept = Convert.ToString(fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Rows[i - 1]["ID"]) !=
                                                    string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Rows[i - 1]["ID"]) : "";

                                            }

                                        }

                                    }

                                }



                            }

                            if (fwbXmlDataSet.Tables.Contains("ArrivalEvent"))
                            {
                                if (fwbXmlDataSet.Tables["ArrivalEvent"].Columns.Contains("ScheduledOccurrenceDateTime"))
                                {
                                    //fwbdata.updatedondate = String.IsNullOrEmpty(Updatedon[0]) ? DateTime.UtcNow.ToShortDateString().Substring(3, 2) : Updatedon[0];
                                    //fwbdata.updatedontime = String.IsNullOrEmpty(Updatedon[1]) ? DateTime.UtcNow.ToShortTimeString().Substring(0,8)  : Updatedon[1];
                                    //fwbdata.updatedontime = String.IsNullOrEmpty(Updatedon[1]) ? DateTime.UtcNow.ToShortTimeString().Substring(0, 8) : Updatedon[1].Substring(0, 8);

                                    sfltarrivaldate = Convert.ToString(fwbXmlDataSet.Tables["ArrivalEvent"].Rows[i]["ScheduledOccurrenceDateTime"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ArrivalEvent"].Rows[i]["ScheduledOccurrenceDateTime"]) : DateTime.Now.ToString("yyyy-MM-dd");
                                    string[] fltarrivalDatesplit = sfltarrivaldate.Split('T');

                                    flight.inputarrivaldatetime = String.IsNullOrEmpty(fltarrivalDatesplit[0]) && String.IsNullOrEmpty(fltarrivalDatesplit[1]) ?
                                       DateTime.UtcNow.ToString("yyyy-MM-dd") + " " + DateTime.UtcNow.ToString("HH:MM:ss") : fltarrivalDatesplit[0] + " " + fltarrivalDatesplit[1];



                                }

                            }

                            if (fwbXmlDataSet.Tables.Contains("OccurrenceArrivalLocation"))
                            {
                                //if (fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Columns.Contains("ID"))
                                //{
                                //    fwbdata.dest = Convert.ToString(fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Rows[i]["ID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Rows[i]["ID"]) : "";
                                //}



                                if (fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Columns.Contains("ID"))
                                {
                                    flight.fltarrival = Convert.ToString(fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Rows[i]["ID"]) !=
                                        string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Rows[i]["ID"]) : "";

                                }

                            }
                            Array.Resize(ref fltroute, fltroute.Length + 1);
                            fltroute[fltroute.Length - 1] = flight;

                        }
                        catch (Exception ex)
                        {
                            //clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }
                    }
                    if (fwbXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Columns.Contains("SequenceNumeric"))
                    {
                        Array.Sort(fltroute, (x, y) => x.Routesquencenumber.CompareTo(y.Routesquencenumber));
                    }

                }

                //Agent
                if (fwbXmlDataSet.Tables.Contains("FreightForwarderParty"))
                {
                    if (fwbXmlDataSet.Tables["FreightForwarderParty"].Columns.Contains("Name"))
                    {
                        fwbdata.agentaccnum = Convert.ToString(fwbXmlDataSet.Tables["FreightForwarderParty"].Rows[0]["Name"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["FreightForwarderParty"].Rows[0]["Name"]) : "0";
                    }
                    if (fwbXmlDataSet.Tables["FreightForwarderParty"].Columns.Contains("CargoAgentID"))
                    {
                        fwbdata.agentIATAnumber = Convert.ToString(fwbXmlDataSet.Tables["FreightForwarderParty"].Rows[0]["CargoAgentID"])
                            != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["FreightForwarderParty"].Rows[0]["CargoAgentID"]) : "0";
                    }
                    if (fwbXmlDataSet.Tables["FreightForwarderParty"].Columns.Contains("AdditionalID"))
                    {
                        fwbdata.agentParticipentIdentifier = Convert.ToString(fwbXmlDataSet.Tables["FreightForwarderParty"].Rows[0]["AdditionalID"])
                            != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["FreightForwarderParty"].Rows[0]["AdditionalID"]) : "0";
                    }
                }

                if (fwbXmlDataSet.Tables.Contains("SpecifiedCargoAgentLocation"))
                {
                    if (fwbXmlDataSet.Tables["SpecifiedCargoAgentLocation"].Columns.Contains("ID"))
                    {
                        fwbdata.agentCASSaddress = Convert.ToString(fwbXmlDataSet.Tables["SpecifiedCargoAgentLocation"].Rows[0]["ID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["SpecifiedCargoAgentLocation"].Rows[0]["ID"]) : "0";

                    }
                }

                //Special Service request


                //Notify
                //fwbdata.notifyname =//Identification of individual or company involved in the movement of a consignment 

                //Accounting Info
                if (fwbXmlDataSet.Tables.Contains("IncludedAccountingNote"))
                {
                    if (fwbXmlDataSet.Tables["IncludedAccountingNote"].Columns.Contains("ContentCode"))
                    {
                        fwbdata.accountinginfoidentifier = Convert.ToString(fwbXmlDataSet.Tables["IncludedAccountingNote"].Rows[0]["ContentCode"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedAccountingNote"].Rows[0]["ContentCode"]) : "";
                    }
                    if (fwbXmlDataSet.Tables["IncludedAccountingNote"].Columns.Contains("Content"))
                    {
                        fwbdata.accountinginfo = Convert.ToString(fwbXmlDataSet.Tables["IncludedAccountingNote"].Rows[0]["Content"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedAccountingNote"].Rows[0]["Content"]) : "";
                    }
                }
                //Charge declaration
                if (fwbXmlDataSet.Tables.Contains("ActualAmount"))
                {
                    if (fwbXmlDataSet.Tables["ActualAmount"].Columns.Contains("currencyID"))
                    {
                        fwbdata.currency = Convert.ToString(fwbXmlDataSet.Tables["ActualAmount"].Rows[0]["currencyID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ActualAmount"].Rows[0]["currencyID"]) : "0";
                    }
                }

                //if (fwbXmlDataSet.Tables.Contains("ApplicableLogisticsAllowanceCharge"))
                //{
                //    if (fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Columns.Contains("ID"))
                //    {
                //        fwbdata.chargecode = Convert.ToString(fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["ID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["ID"]) : "0";
                //    }
                //    if (fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Columns.Contains("Reason"))
                //    {
                //        fwbdata.chargedec = Convert.ToString(fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["Reason"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["Reason"]) : "0";
                //    }
                //}
                if (fwbXmlDataSet.Tables.Contains("ApplicableLogisticsServiceCharge"))
                {
                    if (fwbXmlDataSet.Tables["ApplicableLogisticsServiceCharge"].Columns.Contains("TransportPaymentMethodCode"))
                    {
                        fwbdata.chargecode = Convert.ToString(fwbXmlDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows[0]["TransportPaymentMethodCode"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows[0]["TransportPaymentMethodCode"]) : "PX";
                    }
                    if (fwbXmlDataSet.Tables["ApplicableLogisticsServiceCharge"].Columns.Contains("ServiceTypeCode"))
                    {
                        fwbdata.chargedec = Convert.ToString(fwbXmlDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows[0]["ServiceTypeCode"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows[0]["ServiceTypeCode"]) : "0";
                    }
                }
                if (fwbXmlDataSet.Tables.Contains("DeclaredValueForCarriageAmount"))
                {

                    if (fwbXmlDataSet.Tables["DeclaredValueForCarriageAmount"].Columns.Contains("DeclaredValueForCarriageAmount_Text"))
                    {
                        if (fwbXmlDataSet.Tables["DeclaredValueForCarriageAmount"].Columns.Contains("DeclaredValueForCarriageAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                        {
                            fwbdata.declaredvalue = "0";
                        }
                        else
                        {
                            fwbdata.declaredvalue = Convert.ToString(fwbXmlDataSet.Tables["DeclaredValueForCarriageAmount"].Rows[0]["DeclaredValueForCarriageAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["DeclaredValueForCarriageAmount"].Rows[0]["DeclaredValueForCarriageAmount_Text"]) : "0";

                        }
                    }
                }
                if (fwbXmlDataSet.Tables.Contains("DeclaredValueForCustomsAmount"))
                {
                    if (fwbXmlDataSet.Tables["DeclaredValueForCustomsAmount"].Columns.Contains("DeclaredValueForCustomsAmount_Text"))
                    {
                        if (fwbXmlDataSet.Tables["DeclaredValueForCustomsAmount"].Columns.Contains("DeclaredValueForCustomsAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                        {

                            fwbdata.declaredcustomvalue = "0";
                        }
                        else
                        {

                            fwbdata.declaredcustomvalue = Convert.ToString(fwbXmlDataSet.Tables["DeclaredValueForCustomsAmount"].Rows[0]["DeclaredValueForCustomsAmount_Text"])
                            != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["DeclaredValueForCustomsAmount"].Rows[0]["DeclaredValueForCustomsAmount_Text"]) : "0";
                        }
                    }
                }
                if (fwbXmlDataSet.Tables.Contains("InsuranceValueAmount"))
                {
                    if (fwbXmlDataSet.Tables["InsuranceValueAmount"].Columns.Contains("InsuranceValueAmount_Text"))
                    {
                        if (fwbXmlDataSet.Tables["InsuranceValueAmount"].Columns.Contains("InsuranceValueAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                        {
                            fwbdata.insuranceamount = "0";
                        }
                        else
                        {
                            fwbdata.insuranceamount = Convert.ToString(fwbXmlDataSet.Tables["InsuranceValueAmount"].Rows[0]["InsuranceValueAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["InsuranceValueAmount"].Rows[0]["InsuranceValueAmount_Text"]) : "0";

                        }
                    }
                }
                //Rate Description
                MessageData.RateDescription rate = new MessageData.RateDescription("");

                if (fwbXmlDataSet.Tables.Contains("IncludedMasterConsignmentItem"))
                {
                    if (fwbXmlDataSet.Tables["IncludedMasterConsignmentItem"].Columns.Contains("SequenceNumeric"))
                    {
                        if (fwbXmlDataSet.Tables["IncludedMasterConsignmentItem"].Columns.Contains("SequenceNumeric").ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
                        {
                            rate.linenum = "0";
                        }
                        else
                        {
                            rate.linenum = Convert.ToString(fwbXmlDataSet.Tables["IncludedMasterConsignmentItem"].Rows[0]["SequenceNumeric"])
                            != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedMasterConsignmentItem"].Rows[0]["SequenceNumeric"]) : "0";
                        }
                    }
                    if (fwbXmlDataSet.Tables["IncludedMasterConsignmentItem"].Columns.Contains("PieceQuantity"))
                    { //rate.pcsidentifier = Convert.ToString(fwbXmlDataSet.Tables[""].Rows[0][""]);
                        if (fwbXmlDataSet.Tables["IncludedMasterConsignmentItem"].Columns.Contains("PieceQuantity").ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
                        {
                            rate.numofpcs = "0";
                        }
                        else
                        {
                            rate.numofpcs = Convert.ToString(fwbXmlDataSet.Tables["IncludedMasterConsignmentItem"].Rows[0]["PieceQuantity"])
                                != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedMasterConsignmentItem"].Rows[0]["PieceQuantity"]) : "0";
                        }
                    }
                }
                if (fwbXmlDataSet.Tables.Contains("IncludedTareGrossWeightMeasure"))
                {
                    if (fwbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Columns.Contains("unitCode"))
                    {
                        rate.weightindicator = Convert.ToString(fwbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Rows[0]["unitCode"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Rows[0]["unitCode"]) : "0";
                        fwbdata.weightcode = Convert.ToString(fwbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Rows[0]["unitCode"]) != String.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Rows[0]["unitCode"]) : "0";
                    }
                    if (fwbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Columns.Contains("IncludedTareGrossWeightMeasure_Text"))
                    {
                        if (fwbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Columns.Contains("IncludedTareGrossWeightMeasure_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                        {

                            rate.weight = "0";
                            fwbdata.weight = "0";
                            ErrorMsg = "IncludedTareGrossWeightMeasure should not be 0";
                            flag = false;
                            return flag;
                        }
                        else
                        {
                            rate.weight = Convert.ToString(fwbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Rows[0]["IncludedTareGrossWeightMeasure_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Rows[0]["IncludedTareGrossWeightMeasure_Text"]) : "0";
                            fwbdata.weight = Convert.ToString(fwbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Rows[0]["IncludedTareGrossWeightMeasure_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Rows[0]["IncludedTareGrossWeightMeasure_Text"]) : "0";
                            if (Convert.ToDecimal(fwbdata.weight) == 0)
                            {
                                ErrorMsg = "IncludedTareGrossWeightMeasure should not be 0";
                                flag = false;
                                return flag;
                            }
                        }
                    }
                }

                if (fwbXmlDataSet.Tables.Contains("ApplicableFreightRateServiceCharge"))
                {
                    if (fwbXmlDataSet.Tables["ApplicableFreightRateServiceCharge"].Columns.Contains("CategoryCode"))
                    {
                        rate.rateclasscode = Convert.ToString(fwbXmlDataSet.Tables["ApplicableFreightRateServiceCharge"].Rows[0]["CategoryCode"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ApplicableFreightRateServiceCharge"].Rows[0]["CategoryCode"]) : "0";
                    }
                    if (fwbXmlDataSet.Tables["ApplicableFreightRateServiceCharge"].Columns.Contains("CommodityItemID"))
                    {
                        rate.commoditynumber = Convert.ToString(fwbXmlDataSet.Tables["ApplicableFreightRateServiceCharge"].Rows[0]["CommodityItemID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ApplicableFreightRateServiceCharge"].Rows[0]["CommodityItemID"]) : "0";
                    }
                    if (fwbXmlDataSet.Tables["ApplicableFreightRateServiceCharge"].Columns.Contains("AppliedRate"))
                    {
                        if (fwbXmlDataSet.Tables["ApplicableFreightRateServiceCharge"].Columns.Contains("AppliedRate").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))

                        {
                            rate.chargerate = "0";
                        }
                        else
                        {
                            rate.chargerate = Convert.ToString(fwbXmlDataSet.Tables["ApplicableFreightRateServiceCharge"].Rows[0]["AppliedRate"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ApplicableFreightRateServiceCharge"].Rows[0]["AppliedRate"]) : "0";

                        }
                    }
                }

                if (fwbXmlDataSet.Tables.Contains("ChargeableWeightMeasure"))
                {
                    if (fwbXmlDataSet.Tables["ChargeableWeightMeasure"].Columns.Contains("ChargeableWeightMeasure_Text"))
                    {
                        if (fwbXmlDataSet.Tables["ChargeableWeightMeasure"].Columns.Contains("ChargeableWeightMeasure_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                        {
                            rate.awbweight = "0";
                        }
                        else
                        {
                            rate.awbweight = Convert.ToString(fwbXmlDataSet.Tables["ChargeableWeightMeasure"].Rows[0]["ChargeableWeightMeasure_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ChargeableWeightMeasure"].Rows[0]["ChargeableWeightMeasure_Text"]) : "0";

                        }
                    }
                }

                //if (fwbXmlDataSet.Tables.Contains("IncludedMasterConsignmentItem_GrossVolumeMeasure"))
                //{
                //    if (fwbXmlDataSet.Tables["IncludedMasterConsignmentItem_GrossVolumeMeasure"].Columns.Contains("IncludedMasterConsignmentItem_GrossVolumeMeasure_Text"))
                //    {
                //        if (fwbXmlDataSet.Tables["IncludedMasterConsignmentItem_GrossVolumeMeasure"].Columns.Contains("IncludedMasterConsignmentItem_GrossVolumeMeasure_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                //        {

                //            rate.noofposition = "0";
                //        }
                //        else
                //        {
                //            rate.noofposition = Convert.ToString(fwbXmlDataSet.Tables["IncludedMasterConsignmentItem_GrossVolumeMeasure"].
                //         Rows[0]["IncludedMasterConsignmentItem_GrossVolumeMeasure_Text"])
                //         != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedMasterConsignmentItem_GrossVolumeMeasure"].
                //         Rows[0]["IncludedMasterConsignmentItem_GrossVolumeMeasure_Text"]) : "0";
                //        }
                //    }
                //    if (fwbXmlDataSet.Tables["IncludedMasterConsignmentItem_GrossVolumeMeasure"].Columns.Contains("UnitCode"))
                //    {
                //        rate.noofpositionpo = Convert.ToString(fwbXmlDataSet.Tables["IncludedMasterConsignmentItem_GrossVolumeMeasure"].
                //            Rows[0]["UnitCode"])
                //            != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedMasterConsignmentItem_GrossVolumeMeasure"].
                //            Rows[0]["UnitCode"]) : "0";
                //    }
                //}

                if (fwbXmlDataSet.Tables.Contains("GrossVolumeMeasure"))
                {
                    if (fwbXmlDataSet.Tables["GrossVolumeMeasure"].Columns.Contains("GrossVolumeMeasure_Text"))
                    {
                        if (fwbXmlDataSet.Tables["GrossVolumeMeasure"].Columns.Contains("GrossVolumeMeasure_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                        {

                            rate.noofposition = "0";
                        }
                        else
                        {
                            rate.noofposition = Convert.ToString(fwbXmlDataSet.Tables["GrossVolumeMeasure"].
                         Rows[0]["GrossVolumeMeasure_Text"])
                         != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["GrossVolumeMeasure"].
                         Rows[0]["GrossVolumeMeasure_Text"]) : "0";
                        }
                    }
                    if (fwbXmlDataSet.Tables["GrossVolumeMeasure"].Columns.Contains("UnitCode"))
                    {
                        rate.noofpositionpo = Convert.ToString(fwbXmlDataSet.Tables["GrossVolumeMeasure"].
                            Rows[0]["UnitCode"])
                            != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["GrossVolumeMeasure"].
                            Rows[0]["UnitCode"]) : "0";
                    }
                }


                if (fwbXmlDataSet.Tables.Contains("AppliedAmount"))
                {
                    if (fwbXmlDataSet.Tables["AppliedAmount"].Columns.Contains("AppliedAmount_Text"))
                    {
                        if (fwbXmlDataSet.Tables["AppliedAmount"].Columns.Contains("AppliedAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                        {
                            rate.chargeamt = "0";
                        }
                        else
                        {
                            rate.chargeamt = Convert.ToString(fwbXmlDataSet.Tables["AppliedAmount"].Rows[0]["AppliedAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["AppliedAmount"].Rows[0]["AppliedAmount_Text"]) : "0";

                        }
                    }
                }
                if (fwbXmlDataSet.Tables.Contains("NatureIdentificationTransportCargo"))
                {
                    if (fwbXmlDataSet.Tables["NatureIdentificationTransportCargo"].Columns.Contains("Identification"))
                    {
                        rate.goodsnature = Convert.ToString(fwbXmlDataSet.Tables["NatureIdentificationTransportCargo"].Rows[0]["Identification"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["NatureIdentificationTransportCargo"].Rows[0]["Identification"]) : "0";

                    }
                }
                if (fwbXmlDataSet.Tables.Contains("ApplicableOriginCurrencyExchange"))
                {
                    if (fwbXmlDataSet.Tables["ApplicableOriginCurrencyExchange"].Columns.Contains("SourceCurrencyCode"))
                    {
                        rate.currencyexchange = Convert.ToString(fwbXmlDataSet.Tables["ApplicableOriginCurrencyExchange"].Rows[0]["SourceCurrencyCode"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ApplicableOriginCurrencyExchange"].Rows[0]["SourceCurrencyCode"]) : "0";
                    }
                }
                Array.Resize(ref fwbrate, fwbrate.Length + 1);
                fwbrate[fwbrate.Length - 1] = rate;

                //Other Charges
                MessageData.othercharges oth = new MessageData.othercharges("");
                if (fwbXmlDataSet.Tables.Contains("ApplicableLogisticsAllowanceCharge"))
                {
                    for (int i = 0; i < fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows.Count; i++)
                    {
                        //dimension.fltdept = Convert.ToString(fwbXmlDataSet.Tables["OriginLocation"].Rows[0]["ID"]);
                        try
                        {
                            if (fwbXmlDataSet.Tables.Contains("ApplicableLogisticsAllowanceCharge"))
                            {
                                if (fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Columns.Contains("ID"))
                                {
                                    oth.otherchargecode = Convert.ToString(fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[i]["ID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[i]["ID"]) : "0";
                                }
                                if (fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Columns.Contains("PartyTypeCode"))
                                {
                                    oth.entitlementcode = Convert.ToString(fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[i]["PartyTypeCode"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[i]["PartyTypeCode"]) : "0";
                                }

                            }
                            if (fwbXmlDataSet.Tables.Contains("ActualAmount"))
                            {
                                if (fwbXmlDataSet.Tables["ActualAmount"].Columns.Contains("ActualAmount_Text"))
                                {
                                    if (fwbXmlDataSet.Tables["ActualAmount"].Columns.Contains("ActualAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                                    {
                                        oth.chargeamt = "0";
                                    }
                                    else
                                    {
                                        oth.chargeamt = Convert.ToString(fwbXmlDataSet.Tables["ActualAmount"].Rows[i]["ActualAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ActualAmount"].Rows[i]["ActualAmount_Text"]) : "0";

                                    }
                                }
                            }



                            Array.Resize(ref fwbOtherCharge, fwbOtherCharge.Length + 1);
                            fwbOtherCharge[fwbOtherCharge.Length - 1] = oth;
                        }
                        catch (Exception ex)
                        {
                            //clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }
                    }
                }
                if (fwbXmlDataSet.Tables.Contains("ApplicablePrepaidCollectMonetarySummation"))
                {
                    if (fwbXmlDataSet.Tables["ApplicablePrepaidCollectMonetarySummation"].Columns.Contains("PrepaidIndicator"))
                    {
                        if (Convert.ToBoolean(fwbXmlDataSet.Tables["ApplicablePrepaidCollectMonetarySummation"].Rows[0]["PrepaidIndicator"]))

                        {
                            //Prepaid Charge Summary
                            if (fwbXmlDataSet.Tables.Contains("WeightChargeTotalAmount"))
                            {
                                if (fwbXmlDataSet.Tables["WeightChargeTotalAmount"].Columns.Contains("WeightChargeTotalAmount_Text"))
                                {
                                    if (fwbXmlDataSet.Tables["WeightChargeTotalAmount"].Columns.Contains("WeightChargeTotalAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                                    {
                                        fwbdata.PPweightCharge = "0";
                                    }
                                    else
                                    {
                                        fwbdata.PPweightCharge = Convert.ToString(fwbXmlDataSet.Tables["WeightChargeTotalAmount"].Rows[0]["WeightChargeTotalAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["WeightChargeTotalAmount"].Rows[0]["WeightChargeTotalAmount_Text"]) : "0";

                                    }
                                }
                            }

                            if (fwbXmlDataSet.Tables.Contains("ValuationChargeTotalAmount"))
                            {
                                if (fwbXmlDataSet.Tables["ValuationChargeTotalAmount"].Columns.Contains("ValuationChargeTotalAmount_Text"))
                                {
                                    if (fwbXmlDataSet.Tables["ValuationChargeTotalAmount"].Columns.Contains("ValuationChargeTotalAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                                    {
                                        fwbdata.PPValuationCharge = "0";
                                    }
                                    else
                                    {
                                        fwbdata.PPValuationCharge = Convert.ToString(fwbXmlDataSet.Tables["ValuationChargeTotalAmount"].Rows[0]["ValuationChargeTotalAmount_Text"])
                                        != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ValuationChargeTotalAmount"].Rows[0]["ValuationChargeTotalAmount_Text"]) : "0";
                                    }
                                }
                            }
                            if (fwbXmlDataSet.Tables.Contains("TaxTotalAmount"))
                            {
                                if (fwbXmlDataSet.Tables["TaxTotalAmount"].Columns.Contains("TaxTotalAmount_Text"))
                                {
                                    if (fwbXmlDataSet.Tables["TaxTotalAmount"].Columns.Contains("TaxTotalAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                                    {
                                        fwbdata.PPTaxesCharge = "0";

                                    }
                                    else
                                    {
                                        fwbdata.PPTaxesCharge = Convert.ToString(fwbXmlDataSet.Tables["TaxTotalAmount"].Rows[0]["TaxTotalAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["TaxTotalAmount"].Rows[0]["TaxTotalAmount_Text"]) : "0";

                                    }
                                }
                            }
                            if (fwbXmlDataSet.Tables.Contains("AgentTotalDuePayableAmount"))
                            {
                                if (fwbXmlDataSet.Tables["AgentTotalDuePayableAmount"].Columns.Contains("AgentTotalDuePayableAmount_Text"))
                                {
                                    if (fwbXmlDataSet.Tables["AgentTotalDuePayableAmount"].Columns.Contains("AgentTotalDuePayableAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                                    {
                                        fwbdata.PPOCDA = "0";
                                    }
                                    else
                                    {
                                        fwbdata.PPOCDA = Convert.ToString(fwbXmlDataSet.Tables["AgentTotalDuePayableAmount"].Rows[0]["AgentTotalDuePayableAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["AgentTotalDuePayableAmount"].Rows[0]["AgentTotalDuePayableAmount_Text"]) : "0";

                                    }
                                }
                            }
                            if (fwbXmlDataSet.Tables.Contains("CarrierTotalDuePayableAmount"))
                            {
                                if (fwbXmlDataSet.Tables["CarrierTotalDuePayableAmount"].Columns.Contains("CarrierTotalDuePayableAmount_Text"))
                                {
                                    if (fwbXmlDataSet.Tables["CarrierTotalDuePayableAmount"].Columns.Contains("CarrierTotalDuePayableAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                                    {
                                        fwbdata.PPOCDC = "0";
                                    }
                                    else
                                    {
                                        fwbdata.PPOCDC = Convert.ToString(fwbXmlDataSet.Tables["CarrierTotalDuePayableAmount"].Rows[0]["CarrierTotalDuePayableAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["CarrierTotalDuePayableAmount"].Rows[0]["CarrierTotalDuePayableAmount_Text"]) : "0";

                                    }
                                }
                            }
                            if (fwbXmlDataSet.Tables.Contains("GrandTotalAmount"))
                            {
                                if (fwbXmlDataSet.Tables["GrandTotalAmount"].Columns.Contains("GrandTotalAmount_Text"))
                                {
                                    if (fwbXmlDataSet.Tables["GrandTotalAmount"].Columns.Contains("GrandTotalAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                                    {
                                        fwbdata.PPTotalCharges = "0";
                                    }
                                    else
                                    {
                                        fwbdata.PPTotalCharges = Convert.ToString(fwbXmlDataSet.Tables["GrandTotalAmount"].Rows[0]["GrandTotalAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["GrandTotalAmount"].Rows[0]["GrandTotalAmount_Text"]) : "0";

                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    //Collect Charge Summary

                    if (fwbXmlDataSet.Tables.Contains("WeightChargeTotalAmount"))
                    {
                        if (fwbXmlDataSet.Tables["WeightChargeTotalAmount"].Columns.Contains("WeightChargeTotalAmount_Text"))
                        {
                            if (fwbXmlDataSet.Tables["WeightChargeTotalAmount"].Columns.Contains("WeightChargeTotalAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                            {
                                fwbdata.CCweightCharge = "0";
                            }
                            else
                            {
                                fwbdata.CCweightCharge = Convert.ToString(fwbXmlDataSet.Tables["WeightChargeTotalAmount"].Rows[0]["WeightChargeTotalAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["WeightChargeTotalAmount"].Rows[0]["WeightChargeTotalAmount_Text"]) : "0";

                            }
                        }
                    }
                    if (fwbXmlDataSet.Tables.Contains("ValuationChargeTotalAmount"))
                    {


                        if (fwbXmlDataSet.Tables["ValuationChargeTotalAmount"].Columns.Contains("ValuationChargeTotalAmount_Text"))
                        {
                            if (fwbXmlDataSet.Tables["ValuationChargeTotalAmount"].Columns.Contains("ValuationChargeTotalAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                            {
                                fwbdata.CCValuationCharge = "0";
                            }
                            else
                            {
                                fwbdata.CCValuationCharge = Convert.ToString(fwbXmlDataSet.Tables["ValuationChargeTotalAmount"].Rows[0]["ValuationChargeTotalAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ValuationChargeTotalAmount"].Rows[0]["ValuationChargeTotalAmount_Text"]) : "0";

                            }
                        }
                    }
                    if (fwbXmlDataSet.Tables.Contains("TaxTotalAmount"))
                    {
                        if (fwbXmlDataSet.Tables["TaxTotalAmount"].Columns.Contains("TaxTotalAmount_Text"))
                        {
                            if (fwbXmlDataSet.Tables["TaxTotalAmount"].Columns.Contains("TaxTotalAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                            {
                                fwbdata.CCTaxesCharge = "0";
                            }
                            else
                            {
                                fwbdata.CCTaxesCharge = Convert.ToString(fwbXmlDataSet.Tables["TaxTotalAmount"].Rows[0]["TaxTotalAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["TaxTotalAmount"].Rows[0]["TaxTotalAmount_Text"]) : "0";

                            }
                        }
                    }

                    if (fwbXmlDataSet.Tables.Contains("AgentTotalDuePayableAmount"))
                    {
                        if (fwbXmlDataSet.Tables["AgentTotalDuePayableAmount"].Columns.Contains("AgentTotalDuePayableAmount_Text"))
                        {

                            if (fwbXmlDataSet.Tables["AgentTotalDuePayableAmount"].Columns.Contains("AgentTotalDuePayableAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                            {
                                fwbdata.CCOCDA = "0";
                            }
                            else
                            {
                                fwbdata.CCOCDA = Convert.ToString(fwbXmlDataSet.Tables["AgentTotalDuePayableAmount"].Rows[0]["AgentTotalDuePayableAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["AgentTotalDuePayableAmount"].Rows[0]["AgentTotalDuePayableAmount_Text"]) : "0";

                            }
                        }
                    }
                    if (fwbXmlDataSet.Tables.Contains("CarrierTotalDuePayableAmount"))
                    {
                        if (fwbXmlDataSet.Tables["CarrierTotalDuePayableAmount"].Columns.Contains("CarrierTotalDuePayableAmount_Text"))
                        {
                            fwbdata.CCOCDC = Convert.ToString(fwbXmlDataSet.Tables["CarrierTotalDuePayableAmount"].Rows[0]["CarrierTotalDuePayableAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["CarrierTotalDuePayableAmount"].Rows[0]["CarrierTotalDuePayableAmount_Text"]) : "0";
                        }
                    }
                    if (fwbXmlDataSet.Tables.Contains("GrandTotalAmount"))
                    {
                        if (fwbXmlDataSet.Tables["GrandTotalAmount"].Columns.Contains("GrandTotalAmount_Text"))
                        {
                            if (fwbXmlDataSet.Tables["GrandTotalAmount"].Columns.Contains("GrandTotalAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                            {
                                fwbdata.CCTotalCharges = "";
                            }
                            else
                            {
                                fwbdata.CCTotalCharges = Convert.ToString(fwbXmlDataSet.Tables["GrandTotalAmount"].Rows[0]["GrandTotalAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["GrandTotalAmount"].Rows[0]["GrandTotalAmount_Text"]) : "0";

                            }
                        }
                    }
                }

                ////Shipper Certification
                string actualdate = "";
                if (fwbXmlDataSet.Tables.Contains("SignatoryConsignorAuthentication"))
                {
                    if (fwbXmlDataSet.Tables["SignatoryConsignorAuthentication"].Columns.Contains("Signatory"))
                    {
                        fwbdata.shippersignature = Convert.ToString(fwbXmlDataSet.Tables["SignatoryConsignorAuthentication"].Rows[0]["Signatory"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["SignatoryConsignorAuthentication"].Rows[0]["Signatory"]) : "0";
                    }


                }
                //Carrier Execution
                if (fwbXmlDataSet.Tables.Contains("SignatoryCarrierAuthentication"))
                {
                    if (fwbXmlDataSet.Tables["SignatoryCarrierAuthentication"].Columns.Contains("ActualDateTime"))
                    {
                        actualdate = Convert.ToString(fwbXmlDataSet.Tables["SignatoryCarrierAuthentication"].Rows[0]["ActualDateTime"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["SignatoryCarrierAuthentication"].Rows[0]["ActualDateTime"]) : "0";
                        string[] actualDatesplit = actualdate.Split('T');
                        flight.date = Convert.ToString(actualDatesplit[0]);
                        flight.month = Convert.ToDateTime(actualDatesplit[0]).ToString("MMM");
                        fwbdata.carrierdate = Convert.ToString(actualDatesplit[0]);
                        fwbdata.carriermonth = Convert.ToDateTime(actualDatesplit[0]).ToString("MMM");
                        fwbdata.carrieryear = Convert.ToDateTime(actualDatesplit[0]).ToString("yyyy");
                    }
                    if (fwbXmlDataSet.Tables["SignatoryCarrierAuthentication"].Columns.Contains("Signatory"))
                    {
                        fwbdata.carriersignature = Convert.ToString(fwbXmlDataSet.Tables["SignatoryCarrierAuthentication"].Rows[0]["Signatory"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["SignatoryCarrierAuthentication"].Rows[0]["Signatory"]) : "";

                    }
                    if (fwbXmlDataSet.Tables["SignatoryCarrierAuthentication"].Columns.Contains("Name"))
                    {
                        fwbdata.carrierplace = Convert.ToString(fwbXmlDataSet.Tables["SignatoryCarrierAuthentication"].Rows[0]["Name"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["SignatoryCarrierAuthentication"].Rows[0]["Name"]) : "";
                    }
                    else
                    {
                        if (fwbXmlDataSet.Tables.Contains("IssueAuthenticationLocation"))
                        {
                            if (fwbXmlDataSet.Tables["IssueAuthenticationLocation"].Columns.Contains("SignatoryCarrierAuthentication_Id") &&
                                fwbXmlDataSet.Tables["IssueAuthenticationLocation"].Columns.Contains("Name"))
                            {
                                foreach (DataRow drowlocation in fwbXmlDataSet.Tables["IssueAuthenticationLocation"].Rows)
                                {
                                    if (Convert.ToString(drowlocation["SignatoryCarrierAuthentication_Id"]) == "0")
                                        fwbdata.carrierplace = Convert.ToString(drowlocation["Name"]);
                                }
                            }
                        }
                    }
                }

                //if (fwbXmlDataSet.Tables.Contains("IssueAuthenticationLocation"))
                //{
                //    if (fwbXmlDataSet.Tables["IssueAuthenticationLocation"].Columns.Contains("Name"))
                //    {
                //        fwbdata.carrierplace = Convert.ToString(fwbXmlDataSet.Tables["IssueAuthenticationLocation"].Rows[0]["Name"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IssueAuthenticationLocation"].Rows[0]["Name"]) : "";
                //    }
                //}

                //Other service info
                if (fwbXmlDataSet.Tables.Contains("HandlingOSIInstructions"))
                {
                    if (fwbXmlDataSet.Tables["HandlingOSIInstructions"].Columns.Contains("Description"))
                    {
                        Array.Resize(ref othinfoarray, othinfoarray.Length + 1);
                        othinfoarray[othinfoarray.Length - 1].otherserviceinfo1 = Convert.ToString(fwbXmlDataSet.Tables["HandlingOSIInstructions"].Rows[0]["Description"]);
                    }
                }
                Array.Resize(ref fwbOtherCharge, fwbOtherCharge.Length + 1);
                fwbOtherCharge[fwbOtherCharge.Length - 1] = oth;
                //Charge in destination currency
                if (fwbXmlDataSet.Tables.Contains("ApplicableDestinationCurrencyExchange"))
                {
                    if (fwbXmlDataSet.Tables["ApplicableDestinationCurrencyExchange"].Columns.Contains("TargetCurrencyCode"))
                    {
                        fwbdata.cccurrencycode = Convert.ToString(fwbXmlDataSet.Tables["ApplicableDestinationCurrencyExchange"].Rows[0]["TargetCurrencyCode"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ApplicableDestinationCurrencyExchange"].Rows[0]["TargetCurrencyCode"]) : "0";
                    }
                    if (fwbXmlDataSet.Tables["ApplicableDestinationCurrencyExchange"].Columns.Contains("ConversionRate"))
                    {
                        if (fwbXmlDataSet.Tables["ApplicableDestinationCurrencyExchange"].Columns.Contains("ConversionRate").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                        {
                            fwbdata.ccexchangerate = "0";
                        }
                        else
                        {
                            fwbdata.ccexchangerate = Convert.ToString(fwbXmlDataSet.Tables["ApplicableDestinationCurrencyExchange"].Rows[0]["ConversionRate"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ApplicableDestinationCurrencyExchange"].Rows[0]["ConversionRate"]) : "0";

                        }
                    }
                }


                string CollectAppliedAmount_Text = "";
                string DestinationAppliedAmount_Text = "";
                string TotalAppliedAmount_Text = "";

                if (fwbXmlDataSet.Tables.Contains("CollectAppliedAmount"))
                {
                    if (fwbXmlDataSet.Tables["CollectAppliedAmount"].Columns.Contains("CollectAppliedAmount_Text"))
                    {
                        if (fwbXmlDataSet.Tables["CollectAppliedAmount"].Columns.Contains("CollectAppliedAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                        {
                            CollectAppliedAmount_Text = "0";
                        }
                        else
                        {
                            CollectAppliedAmount_Text = Convert.ToString(fwbXmlDataSet.Tables["CollectAppliedAmount"].Rows[0]["CollectAppliedAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["CollectAppliedAmount"].Rows[0]["CollectAppliedAmount_Text"]) : "0";

                        }
                    }
                }

                if (fwbXmlDataSet.Tables.Contains("DestinationAppliedAmount"))
                {
                    if (fwbXmlDataSet.Tables["DestinationAppliedAmount"].Columns.Contains("DestinationAppliedAmount_Text"))
                    {
                        if (fwbXmlDataSet.Tables["DestinationAppliedAmount"].Columns.Contains("DestinationAppliedAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                        {
                            DestinationAppliedAmount_Text = "0";
                        }
                        else
                        {
                            DestinationAppliedAmount_Text = Convert.ToString(fwbXmlDataSet.Tables["DestinationAppliedAmount"].Rows[0]["DestinationAppliedAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["DestinationAppliedAmount"].Rows[0]["DestinationAppliedAmount_Text"]) : "0";

                        }
                    }
                }
                if (fwbXmlDataSet.Tables.Contains("TotalAppliedAmount"))
                {
                    if (fwbXmlDataSet.Tables["TotalAppliedAmount"].Columns.Contains("TotalAppliedAmount_Text"))
                    {
                        if (fwbXmlDataSet.Tables["TotalAppliedAmount"].Columns.Contains("TotalAppliedAmount_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                        {
                            TotalAppliedAmount_Text = "0";
                        }
                        else
                        {
                            TotalAppliedAmount_Text = (Convert.ToString(fwbXmlDataSet.Tables["TotalAppliedAmount"].Rows[0]["TotalAppliedAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["TotalAppliedAmount"].Rows[0]["TotalAppliedAmount_Text"]) : "0");

                        }
                    }
                }

                fwbdata.ccchargeamt = CollectAppliedAmount_Text + "," + DestinationAppliedAmount_Text + "," + TotalAppliedAmount_Text;


                //fwbdata.ccchargeamt =
                //    (Convert.ToString(fwbXmlDataSet.Tables["CollectAppliedAmount"].Rows[0]["CollectAppliedAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["CollectAppliedAmount"].Rows[0]["CollectAppliedAmount_Text"]) : "0")
                //    + "," + (Convert.ToString(fwbXmlDataSet.Tables["DestinationAppliedAmount"].Rows[0]["DestinationAppliedAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["DestinationAppliedAmount"].Rows[0]["DestinationAppliedAmount_Text"]) : "0")
                //    + "," + (Convert.ToString(fwbXmlDataSet.Tables["TotalAppliedAmount"].Rows[0]["TotalAppliedAmount_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["TotalAppliedAmount"].Rows[0]["TotalAppliedAmount_Text"]) : "0");


                ////Sender Reference
                //fwbdata.senderairport = msg[1].Substring(0, 3);
                //fwbdata.senderofficedesignator = msg[1].Substring(3, 2);
                //fwbdata.sendercompanydesignator = msg[1].Substring(5);


                //fwbdata.senderParticipentIdentifier = msg[3];
                //fwbdata.senderParticipentCode = msg[4];
                //fwbdata.senderPariticipentAirport = msg[5];

                //Custom Origin
                if (fwbXmlDataSet.Tables.Contains("IncludedCustomsNote"))
                {
                    if (fwbXmlDataSet.Tables["IncludedCustomsNote"].Columns.Contains("CountryID"))
                    {
                        fwbdata.customorigincode = Convert.ToString(fwbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["CountryID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["CountryID"]) : "0";
                    }
                }
                ////Commission Information
                //fwbdata.commisioncassindicator = Convert.ToString(fwbXmlDataSet.Tables[""].Rows[0][""]);
                //for (int k = 2; k < msg.Length; k++)
                //    fwbdata.commisionCassSettleAmt += msg[k] + ",";

                ////Sales Incentive Info
                //fwbdata.saleschargeamt = msg[1];
                //fwbdata.salescassindicator = msg[2];

                ////Agent Reference
                //fwbdata.agentfileref = msg[1];

                //Special Handling
                if (fwbXmlDataSet.Tables.Contains("HandlingSPHInstructions"))
                {
                    if (fwbXmlDataSet.Tables["HandlingSPHInstructions"].Columns.Contains("DescriptionCode"))
                    {
                        fwbdata.splhandling = Convert.ToString(fwbXmlDataSet.Tables["HandlingSPHInstructions"].Rows[0]["DescriptionCode"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["HandlingSPHInstructions"].Rows[0]["DescriptionCode"]) : "";
                    }
                }
                ////Nominated Handling Party
                //fwbdata.handlingname = msg[1];
                //fwbdata.handlingplace = msg[2];

                //Shipment Reference Info


                //custom extra info
                if (fwbXmlDataSet.Tables.Contains("IncludedCustomsNote"))
                {
                    MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
                    if (fwbXmlDataSet.Tables["IncludedCustomsNote"].Columns.Contains("CountryID"))
                    {
                        custom.IsoCountryCodeOci = Convert.ToString(fwbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["CountryID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["CountryID"]) : "0";
                    }
                    if (fwbXmlDataSet.Tables["IncludedCustomsNote"].Columns.Contains("SubjectCode"))
                    {
                        custom.InformationIdentifierOci = Convert.ToString(fwbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["SubjectCode"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["SubjectCode"]) : "0";
                    }
                    if (fwbXmlDataSet.Tables["IncludedCustomsNote"].Columns.Contains("ContentCode"))
                    {
                        custom.CsrIdentifierOci = Convert.ToString(fwbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["ContentCode"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["ContentCode"]) : "0";
                    }

                    if (fwbXmlDataSet.Tables["IncludedCustomsNote"].Columns.Contains("Content"))
                    {
                        custom.SupplementaryCsrIdentifierOci = Convert.ToString(fwbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["Content"])
                            != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["Content"]) : "0";
                    }
                    Array.Resize(ref custominfo, custominfo.Length + 1);
                    custominfo[custominfo.Length - 1] = custom;
                }


                //SHP  //Shipper Info
                if (fwbXmlDataSet.Tables.Contains("ConsignorParty"))
                {
                    if (fwbXmlDataSet.Tables["ConsignorParty"].Columns.Contains("AccountID"))
                    {
                        fwbdata.shipperaccnum = Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty"].Rows[0]["AccountID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty"].Rows[0]["AccountID"]) : "0";
                    }
                    if (fwbXmlDataSet.Tables["ConsignorParty"].Columns.Contains("Name"))
                    {
                        fwbdata.shippername = Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty"].Rows[0]["Name"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty"].Rows[0]["Name"]) : "";
                        //fwbdata.shippername2 =

                    }

                    if (fwbXmlDataSet.Tables["ConsignorParty"].Columns.Contains("PrimaryID"))
                    {
                        fwbdata.ConsignorParty_PrimaryID = Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty"].Rows[0]["PrimaryID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty"].Rows[0]["PrimaryID"]) : "";
                    }

                }

                if (fwbXmlDataSet.Tables.Contains("ConsignorParty_PostalStructuredAddress"))
                {
                    if (fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Columns.Contains("StreetName"))
                    {
                        fwbdata.shipperadd = Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["StreetName"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["StreetName"]) : "";
                    }
                    //fwbdata.shipperadd2 =
                    if (fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Columns.Contains("CountryName"))
                    {
                        fwbdata.shipperplace = Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountryName"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountryName"]) : "";
                    }
                    if (fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Columns.Contains("CountrySubDivisionID"))
                    {
                        fwbdata.shipperstate = Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountrySubDivisionID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountrySubDivisionID"]) : "";

                    }
                    if (fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Columns.Contains("CountryID"))
                    {
                        fwbdata.shippercountrycode = Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountryID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountryID"]) : "0";
                    }
                    if (fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Columns.Contains("CityName"))
                    {

                        fwbdata.shippercity = Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CityName"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CityName"]) : "";
                    }
                    if (fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Columns.Contains("PostcodeCode"))
                    {
                        fwbdata.shipperpostcode = Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["PostcodeCode"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["PostcodeCode"]) : "0";
                        //fwbdata.shippercontactidentifier = Convert.ToString(fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountryID"]);
                    }
                }
                if (fwbXmlDataSet.Tables.Contains("DirectTelephoneCommunication"))
                {
                    //if (fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Columns.Contains("CompleteNumber") &&
                    //    fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Rows.Count == 2)
                    //{
                    fwbdata.shippercontactnum = Convert.ToString(fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Rows[0]["CompleteNumber"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Rows[0]["CompleteNumber"]) : "0";
                    //}
                    //else
                    //{
                    //    fwbdata.shippercontactnum = Convert.ToString(fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Rows[0]["CompleteNumber"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Rows[0]["CompleteNumber"]) : "0";

                    //}



                }
                if (fwbXmlDataSet.Tables.Contains("FaxCommunication"))
                {

                    //if (fwbXmlDataSet.Tables["FaxCommunication"].Columns.Contains("CompleteNumber") && fwbXmlDataSet.Tables["FaxCommunication"].Rows.Count == 2)
                    //{
                    fwbdata.shipperfaxno = Convert.ToString(fwbXmlDataSet.Tables["FaxCommunication"].Rows[0]["CompleteNumber"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["FaxCommunication"].Rows[0]["CompleteNumber"]) : "0";
                    //}
                    //else
                    //{
                    //    fwbdata.shipperfaxno = Convert.ToString(fwbXmlDataSet.Tables["FaxCommunication"].Rows[0]["CompleteNumber"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["FaxCommunication"].Rows[0]["CompleteNumber"]) : "0";

                    //}
                }
                //CNE
                if (fwbXmlDataSet.Tables.Contains("ConsigneeParty"))
                {

                    if (fwbXmlDataSet.Tables["ConsigneeParty"].Columns.Contains("AccountID"))
                    {
                        fwbdata.consaccnum = Convert.ToString(fwbXmlDataSet.Tables["ConsigneeParty"].Rows[0]["AccountID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsigneeParty"].Rows[0]["AccountID"]) : "0";
                    }
                    if (fwbXmlDataSet.Tables["ConsigneeParty"].Columns.Contains("Name"))
                    {
                        fwbdata.consname = Convert.ToString(fwbXmlDataSet.Tables["ConsigneeParty"].Rows[0]["Name"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsigneeParty"].Rows[0]["Name"]) : "";
                    }
                }

                //fwbdata.consname2 =
                if (fwbXmlDataSet.Tables.Contains("ConsigneeParty_PostalStructuredAddress"))
                {
                    if (fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Columns.Contains("StreetName"))
                    {
                        fwbdata.consadd = Convert.ToString(fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["StreetName"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["StreetName"]) : "";
                    }
                    if (fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Columns.Contains("CountryName"))
                    {  //fwbdata.consadd2 =
                        fwbdata.consplace = Convert.ToString(fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountryName"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountryName"]) : "";
                    }
                    if (fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Columns.Contains("CountrySubDivisionID"))
                    {
                        fwbdata.consstate = Convert.ToString(fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountrySubDivisionID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountrySubDivisionID"]) : "";
                    }
                    if (fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Columns.Contains("CountryID"))
                    {
                        fwbdata.conscountrycode = Convert.ToString(fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountryID"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountryID"]) : "0";
                    }
                    if (fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Columns.Contains("PostcodeCode"))
                    {
                        fwbdata.conspostcode = Convert.ToString(fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["PostcodeCode"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["PostcodeCode"]) : "0";
                    }
                    if (fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Columns.Contains("CityName"))
                    {
                        fwbdata.conscity = Convert.ToString(fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CityName"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CityName"]) : "";
                    }
                }

                if (fwbXmlDataSet.Tables.Contains("DirectTelephoneCommunication"))
                {
                    //fwbdata.conscontactidentifier = msg[3].Length > 0 ||msg[3]==null? msg[3] : "";
                    if (fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Columns.Contains("CompleteNumber") &&
                        fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Rows.Count == 2)
                    {
                        fwbdata.conscontactnum = Convert.ToString(fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Rows[1]["CompleteNumber"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Rows[1]["CompleteNumber"]) : "";
                    }
                    else
                    {
                        fwbdata.conscontactnum = Convert.ToString(fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Rows[0]["CompleteNumber"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Rows[0]["CompleteNumber"]) : "";

                    }
                }

                //Dimenshion

                MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
                if (fwbXmlDataSet.Tables.Contains("TransportLogisticsPackage"))
                {
                    for (int i = 0; i < fwbXmlDataSet.Tables["TransportLogisticsPackage"].Rows.Count; i++)
                    {
                        //dimension.fltdept = Convert.ToString(fwbXmlDataSet.Tables["OriginLocation"].Rows[0]["ID"]);
                        try
                        {
                            if (fwbXmlDataSet.Tables.Contains("WidthMeasure"))
                            {
                                //fwbdata.conscontactidentifier = msg[3].Length > 0 ||msg[3]==null? msg[3] : "";
                                if (fwbXmlDataSet.Tables["WidthMeasure"].Columns.Contains("WidthMeasure_Text"))
                                {
                                    if (fwbXmlDataSet.Tables["WidthMeasure"].Columns.Contains("WidthMeasure_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                                    {
                                        dimension.width = "0";
                                    }
                                    else
                                    {
                                        dimension.width = Convert.ToString(fwbXmlDataSet.Tables["WidthMeasure"].Rows[i]["WidthMeasure_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["WidthMeasure"].Rows[i]["WidthMeasure_Text"]) : "0";

                                    }
                                }
                            }
                            if (fwbXmlDataSet.Tables.Contains("LengthMeasure"))
                            {
                                //fwbdata.conscontactidentifier = msg[3].Length > 0 ||msg[3]==null? msg[3] : "";
                                if (fwbXmlDataSet.Tables["LengthMeasure"].Columns.Contains("LengthMeasure_Text"))
                                {
                                    if (fwbXmlDataSet.Tables["LengthMeasure"].Columns.Contains("LengthMeasure_Text").ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
                                    {
                                        dimension.height = "0";
                                    }
                                    else
                                    {
                                        dimension.height = Convert.ToString(fwbXmlDataSet.Tables["LengthMeasure"].Rows[i]["LengthMeasure_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["LengthMeasure"].Rows[i]["LengthMeasure_Text"]) : "0";

                                    }
                                }
                            }
                            if (fwbXmlDataSet.Tables.Contains("HeightMeasure"))
                            {
                                //fwbdata.conscontactidentifier = msg[3].Length > 0 ||msg[3]==null? msg[3] : "";

                                if (fwbXmlDataSet.Tables["HeightMeasure"].Columns.Contains("HeightMeasure_Text"))
                                {
                                    if (fwbXmlDataSet.Tables["HeightMeasure"].Columns.Contains("HeightMeasure_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                                    {
                                        dimension.length = "0";
                                    }
                                    else
                                    {
                                        dimension.length = Convert.ToString(fwbXmlDataSet.Tables["HeightMeasure"].Rows[i]["HeightMeasure_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["HeightMeasure"].Rows[i]["HeightMeasure_Text"]) : "0";

                                    }
                                }
                                if (fwbXmlDataSet.Tables["HeightMeasure"].Columns.Contains("HeightMeasure_Text"))
                                {
                                    dimension.mesurunitcode = Convert.ToString(fwbXmlDataSet.Tables["HeightMeasure"].Rows[i]["unitCode"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["HeightMeasure"].Rows[i]["unitCode"]) : "0";
                                }
                            }
                            if (fwbXmlDataSet.Tables.Contains("TransportLogisticsPackage"))
                            {
                                //fwbdata.conscontactidentifier = msg[3].Length > 0 ||msg[3]==null? msg[3] : "";
                                if (fwbXmlDataSet.Tables["TransportLogisticsPackage"].Columns.Contains("ItemQuantity"))
                                {
                                    if (fwbXmlDataSet.Tables["TransportLogisticsPackage"].Columns.Contains("ItemQuantity").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                                    {
                                        dimension.piecenum = "0";
                                    }
                                    else
                                    {
                                        dimension.piecenum = Convert.ToString(fwbXmlDataSet.Tables["TransportLogisticsPackage"].Rows[i]["ItemQuantity"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["TransportLogisticsPackage"].Rows[i]["ItemQuantity"]) : "0";

                                    }
                                }
                            }
                            if (fwbXmlDataSet.Tables.Contains("GrossWeightMeasure"))
                            {
                                //fwbdata.conscontactidentifier = msg[3].Length > 0 ||msg[3]==null? msg[3] : "";
                                drs = fwbXmlDataSet.Tables["GrossWeightMeasure"].Select("TransportLogisticsPackage_Id=" + i);
                                //if (fwbXmlDataSet.Tables["GrossWeightMeasure"].Columns.Contains("GrossWeightMeasure_Text"))
                                //{
                                //    if (fwbXmlDataSet.Tables["GrossWeightMeasure"].Columns.Contains("GrossWeightMeasure_Text").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                                //    {
                                //        dimension.weight = "0";
                                //    }
                                //    else
                                //    {
                                //        dimension.weight = Convert.ToString(fwbXmlDataSet.Tables["GrossWeightMeasure"].Rows[i]["GrossWeightMeasure_Text"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["GrossWeightMeasure"].Rows[i]["GrossWeightMeasure_Text"]) : "0";

                                //    }
                                //}
                                //if (fwbXmlDataSet.Tables["GrossWeightMeasure"].Columns.Contains("unitCode"))
                                //{
                                //    dimension.weightcode = Convert.ToString(fwbXmlDataSet.Tables["GrossWeightMeasure"].Rows[i]["unitCode"]) != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["GrossWeightMeasure"].Rows[i]["unitCode"]) : "0";
                                //}
                                if (drs.Length > 0)
                                {
                                    dimension.weightcode = Convert.ToString(drs[0]["unitCode"]);
                                    if (Convert.ToString(drs[0]["GrossWeightMeasure_Text"]).ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                                    {
                                        dimension.weight = "0";
                                    }
                                    else
                                    {
                                        dimension.weight = Convert.ToString(drs[0]["GrossWeightMeasure_Text"])
                                            != string.Empty ? Convert.ToString(drs[0]["GrossWeightMeasure_Text"]) : "0";

                                    }

                                    //fsadata.weight = Convert.ToString(drs[0]["TotalGrossWeightMeasure_Text"]);
                                }


                            }
                            if (fwbXmlDataSet.Tables.Contains("MasterConsignment"))
                            {
                                if (fwbXmlDataSet.Tables["MasterConsignment"].Columns.Contains("PackageQuantity"))
                                {
                                    if (fwbXmlDataSet.Tables["MasterConsignment"].Columns.Contains("PackageQuantity").ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
                                    {
                                        dimension.dims_slac = "0";
                                    }
                                    else
                                    {
                                        dimension.dims_slac = Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["PackageQuantity"])
                                         != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["PackageQuantity"]) : "0";
                                    }
                                }
                                else
                                {
                                    if (fwbXmlDataSet.Tables["MasterConsignment"].Columns.Contains("TotalPieceQuantity"))
                                    {
                                        if (fwbXmlDataSet.Tables["MasterConsignment"].Columns.Contains("TotalPieceQuantity").ToString().Equals("Null", StringComparison.OrdinalIgnoreCase))
                                        {
                                            dimension.dims_slac = "0";
                                        }
                                        else
                                        {
                                            dimension.dims_slac = Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"])
                                                != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"]) : "0";
                                        }
                                    }
                                }
                            }
                            dimension.AWBPrefix = decmes[0];
                            dimension.AWBNumber = decmes[1];

                            Array.Resize(ref objDimension, objDimension.Length + 1);
                            objDimension[objDimension.Length - 1] = dimension;
                        }
                        catch (Exception ex)
                        {
                            //clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }

                    }
                }

                //BUP
                MessageData.AWBBuildBUP dims_BUP = new MessageData.AWBBuildBUP("");
                if (fwbXmlDataSet.Tables.Contains("AssociatedUnitLoadTransportEquipment"))
                {
                    for (int i = 0; i < fwbXmlDataSet.Tables["AssociatedUnitLoadTransportEquipment"].Rows.Count; i++)
                    {
                        //dimension.fltdept = Convert.ToString(fwbXmlDataSet.Tables["OriginLocation"].Rows[0]["ID"]);
                        try
                        {
                            //fwbdata.conscontactidentifier = msg[3].Length > 0 ||msg[3]==null? msg[3] : "";
                            if (fwbXmlDataSet.Tables["AssociatedUnitLoadTransportEquipment"].Columns.Contains("ID"))
                            {
                                dims_BUP.ULDNo = Convert.ToString(fwbXmlDataSet.Tables["AssociatedUnitLoadTransportEquipment"].Rows[i]["ID"])
                                    != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["AssociatedUnitLoadTransportEquipment"].Rows[i]["ID"]) : "0";
                            }
                            if (fwbXmlDataSet.Tables["AssociatedUnitLoadTransportEquipment"].Columns.Contains("LoadedPackageQuantity"))
                            {
                                if (fwbXmlDataSet.Tables["AssociatedUnitLoadTransportEquipment"].Rows[i]["LoadedPackageQuantity"].ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
                                {
                                    dims_BUP.SlacCount = "0";
                                }
                                else
                                {
                                    dims_BUP.SlacCount = Convert.ToString(fwbXmlDataSet.Tables["AssociatedUnitLoadTransportEquipment"].Rows[i]["LoadedPackageQuantity"])
                                        != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["AssociatedUnitLoadTransportEquipment"].Rows[i]["LoadedPackageQuantity"]) : "0";
                                }
                            }
                            if (fwbXmlDataSet.Tables.Contains("TareWeightMeasure"))
                            {
                                if (fwbXmlDataSet.Tables["TareWeightMeasure"].Columns.Contains("TareWeightMeasure_Text"))
                                {
                                    if (fwbXmlDataSet.Tables["TareWeightMeasure"].Columns.Contains("TareWeightMeasure_Text").ToString().Equals("null", StringComparison.OrdinalIgnoreCase))

                                    {
                                        dims_BUP.BUPWt = "0";
                                    }
                                    else
                                    {
                                        dims_BUP.BUPWt = Convert.ToString(fwbXmlDataSet.Tables["TareWeightMeasure"].Rows[i]["TareWeightMeasure_Text"])
                                           != string.Empty ? Convert.ToString(fwbXmlDataSet.Tables["TareWeightMeasure"].Rows[i]["TareWeightMeasure_Text"]) : "0";
                                    }


                                }
                            }



                            Array.Resize(ref objAwbBup, objAwbBup.Length + 1);
                            objAwbBup[objAwbBup.Length - 1] = dims_BUP;
                        }
                        catch (Exception ex)
                        {
                            //clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }

                    }
                }




                flag = true;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                ErrorMsg = "Error Occured when XML Decoding: [[" + ex.Message + "]];"; //[[" + ex.StackTrace + "]]";
                flag = false;
            }

            if (ErrorMsg == "")
                ErrorMsg = "Error Occured when XML Decoding";
            return flag;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
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
                strarr = null;
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return strarr;
        }

        /// <summary>
        /// Method to save the operation data through XFWB
        /// </summary>
        /// <param name="fwbdata"></param>
        /// <param name="fltroute"></param>
        /// <param name="OtherCharges"></param>
        /// <param name="othinfoarray"></param>
        /// <param name="fwbrates"></param>
        /// <param name="customextrainfo"></param>
        /// <param name="objDimension"></param>
        /// <param name="REFNo"></param>
        /// <param name="objAWBBup"></param>
        /// <returns></returns>
        /// 
        #region  comment
        //public bool SaveandValidateFWBMessage(MessageData.fwbinfo fwbdata, MessageData.FltRoute[] fltroute, MessageData.othercharges[] OtherCharges, MessageData.otherserviceinfo[] othinfoarray, MessageData.RateDescription[] fwbrates, MessageData.customsextrainfo[] customextrainfo, MessageData.dimensionnfo[] objDimension, int REFNo, MessageData.AWBBuildBUP[] objAWBBup, string strMessage, string strMessageFrom, string strFromID, string strStatus, out string ErrorMsg)
        //{
        //    bool flag = false;
        //    try
        //    {
        //        ErrorMsg = string.Empty;
        //        FFRMessageProcessor ffR = new FFRMessageProcessor();
        //        SQLServer dtb = new SQLServer();
        //        MessageData.FltRoute[] objRouteInfo = new MessageData.FltRoute[0];
        //        string awbnum = fwbdata.awbnum;
        //        string AWBPrefix = fwbdata.airlineprefix;
        //        string flightnum = "NA", commcode = "", commtype = string.Empty;
        //        string flightdate = System.DateTime.Now.ToString("dd/MM/yyyy");
        //        string strFlightNo = string.Empty, strFlightOrigin = string.Empty, strFlightDestination = string.Empty;
        //        bool val = true;

        //        GenericFunction gf = new GenericFunction();
        //        XFNMMessageProcessor fna = new XFNMMessageProcessor();
        //        gf.UpdateInboxFromMessageParameter(REFNo, AWBPrefix + "-" + awbnum, string.Empty, string.Empty, string.Empty, "XFWB", strMessageFrom == "" ? strFromID : strMessageFrom, DateTime.Parse("1900-01-01"));


        //        if (strFromID.Contains("SITA"))
        //        {
        //            commtype = "SITAFTP";
        //        }
        //        else
        //        {
        //            commtype = "EMAIL";
        //        }

        //        if (fltroute.Length > 0)
        //        {
        //            flightnum = fltroute[0].carriercode + fltroute[0].fltnum;
        //            strFlightOrigin = fltroute[0].fltdept;
        //            strFlightDestination = fltroute[0].fltarrival;
        //            if (fltroute[0].date != "")
        //                //flightdate = Convert.ToDateTime(fltroute[0].date).ToString("dd/MM/yyyy");// + "/" + DateTime.Now.ToString("MM/yyyy");

        //                flightdate = Convert.ToDateTime(fltroute[0].date).ToString("yyyy/MM/dd");// + "/" + DateTime.Now.ToString("MM/yyyy");

        //            else
        //                //flightdate = DateTime.Now.ToString("dd/MM/yyyy");
        //                flightdate = DateTime.Now.ToString("yyyy/MM/dd");

        //        }
        //        else
        //        {

        //            if (fwbdata.fltnum.Length > 0 && !(fwbdata.fltnum.Contains(',')))
        //            {
        //                flightnum = fwbdata.fltnum;
        //                flightdate = Convert.ToDateTime(fwbdata.fltday).ToString("dd/MM/yyyy"); //.PadLeft(2, '0') + "/" + DateTime.Now.ToString("MM/yyyy");
        //            }
        //        }
        //        if (fwbrates[0].goodsnature.Length > 1)
        //            commcode = fwbrates[0].goodsnature;

        //        if (commcode != "")
        //        {
        //            if (fwbrates[0].goodsnature1.Length > 1)
        //                commcode += "," + fwbrates[0].goodsnature1;
        //        }
        //        else
        //            commcode = fwbrates[0].goodsnature1;

        //        DataSet dsawb = new DataSet();
        //        string strErrorMessage = string.Empty;
        //        dsawb = ffR.CheckValidateXFFRMessage(AWBPrefix, awbnum, fwbdata.origin, fwbdata.dest, "XFWB");
        //        if (dsawb != null && dsawb.Tables.Count > 0 && dsawb.Tables[0].Rows.Count > 0)
        //        {
        //            if (dsawb.Tables.Count > 0 && dsawb.Tables[0].Rows.Count > 0 && dsawb.Tables[0].Columns.Count == 2)
        //            {
        //                if (dsawb.Tables[0].Rows[0]["MessageName"].ToString() == "XFWB" && dsawb.Tables[0].Rows[0]["AWBSttus"].ToString().ToUpper() == "ACCEPTED")
        //                {
        //                    string[] PFWB = new string[] { "AirlinePrefix", "AWBNum", "ShipperName", "ShipperAddr", "ShipperPlace", "ShipperState", "ShipperCountryCode", "ShipperContactNo", "ShipperPincode", "ConsName", "ConsAddr", "ConsPlace", "ConsState", "ConsCountryCode", "ConsContactNo", "ConsingneePinCode", "CustAccNo", "IATACargoAgentCode", "CustName", "REFNo", "UpdatedBy" };

        //                    SqlDbType[] ParamSqlType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar };

        //                    object[] paramValue = { fwbdata.airlineprefix, fwbdata.awbnum, fwbdata.shippername, fwbdata.shipperadd.Trim(','), fwbdata.shipperplace.Trim(','), fwbdata.shipperstate, fwbdata.shippercountrycode, fwbdata.shippercontactnum, fwbdata.shipperpostcode, fwbdata.consname, fwbdata.consadd.Trim(','), fwbdata.consplace.Trim(','), fwbdata.consstate, fwbdata.conscountrycode, fwbdata.conscontactnum, fwbdata.conspostcode, fwbdata.agentaccnum, fwbdata.agentIATAnumber, fwbdata.agentname, REFNo, "XFWB" };


        //                    fna.GenerateXFNMMessage(strMessage, "AWB is Already Accepted, We will only update SHP/CNE info", AWBPrefix, awbnum, strMessageFrom == "" ? strFromID : strMessageFrom, commtype);

        //                    string strProcedure = "Messaging.uspUpdateShipperConsigneeforFWB";
        //                    flag = dtb.InsertData(strProcedure, PFWB, ParamSqlType, paramValue);
        //                    if (flag)
        //                        return flag = true;
        //                    else
        //                        return flag = false;

        //                }
        //            }
        //            else
        //            {
        //                //strErrorMessage = dsawb.Tables[0].Rows[0]["ErrorMessage"].ToString();
        //                //string[] QueryNames = { "ErrorMessage", "MessageSNo" };
        //                //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.Int };
        //                //object[] QueryValues = { strErrorMessage, REFNo };
        //                ErrorMsg = dsawb.Tables[0].Rows[0]["ErrorMessage"].ToString();
        //                //if (dtb.UpdateData("spUpdateStatusformessage", QueryNames, QueryTypes, QueryValues))
        //                //{
        //                fna.GenerateXFNMMessage(strMessage, strErrorMessage, fwbdata.airlineprefix, fwbdata.awbnum, strMessageFrom == "" ? strFromID : strMessageFrom, commtype);
        //                //}
        //                //strErrorMessage = string.Empty;

        //                return flag = false;
        //            }
        //        }

        //        fna.GenerateXFNMMessage(strMessage, "We will book/execute AWB  " + fwbdata.airlineprefix + "-" + fwbdata.awbnum + " Shortly.", fwbdata.airlineprefix, fwbdata.awbnum, strMessageFrom == "" ? strFromID : strMessageFrom, commtype);

        //        string strAWbIssueDate = string.Empty;
        //        if (fwbdata.carrierdate != "" && fwbdata.carriermonth != "" && fwbdata.carrieryear != "")
        //        {

        //            //int month = DateTime.Parse("1." + (fwbdata.carriermonth.ToString().PadLeft(2, '0')) + " 2008").Month;
        //            //strAWbIssueDate = month + "/" + fwbdata.carrierdate.PadLeft(2, '0') + "/" + "20" + fwbdata.carrieryear;
        //            //strAWbIssueDate =Convert.ToDateTime(fwbdata.carrierdate).ToString("MM/dd/yyyy");
        //            strAWbIssueDate = Convert.ToDateTime(fwbdata.carrierdate).ToString("yyyy/MM/dd");

        //        }
        //        else
        //        {
        //            //strAWbIssueDate = System.DateTime.Now.ToString("MM/dd/yyyy");
        //            strAWbIssueDate = System.DateTime.Now.ToString("yyyy/MM/dd");

        //        }

        //        #region AWB
        //        string VolumeAmount = string.Empty;
        //        try
        //        {
        //            VolumeAmount = (fwbdata.volumeamt.Length > 0 ? fwbdata.volumeamt : fwbrates[0].volamt);
        //        }
        //        catch (Exception)
        //        {
        //            VolumeAmount = fwbdata.volumeamt;
        //        }
        //        string[] paramname = new string[] { "AirlinePrefix", "AWBNum", "Origin", "Dest", "PcsCount", "Weight", "Volume", "ComodityCode", "ComodityDesc", "CarrierCode", "FlightNum", "FlightDate", "FlightOrigin", "FlightDest", "ShipperName", "ShipperAddr", "ShipperPlace", "ShipperState", "ShipperCountryCode", "ShipperContactNo", "ConsName", "ConsAddr", "ConsPlace", "ConsState", "ConsCountryCode", "ConsContactNo", "CustAccNo", "IATACargoAgentCode", "CustName", "SystemDate", "MeasureUnit", "Length", "Breadth", "Height", "PartnerStatus", "REFNo", "UpdatedBy", "SpecialHandelingCode", "Paymode", "ShipperPincode", "ConsingneePinCode", "WeightCode", "AWBIssueDate" };

        //        object[] paramvalue = new object[] {fwbdata.airlineprefix,fwbdata.awbnum,fwbdata.origin, fwbdata.dest,fwbdata.pcscnt, fwbdata.weight,
        //            VolumeAmount, "", commcode,fwbdata.carriercode,flightnum,
        //            flightdate,
        //           //DateTime.Now,
        //            strFlightOrigin,strFlightDestination, fwbdata.shippername.Trim(' '),
        //                                                 fwbdata.shipperadd.Trim(','), fwbdata.shipperplace.Trim(','), fwbdata.shipperstate, fwbdata.shippercountrycode, fwbdata.shippercontactnum, fwbdata.consname.Trim(' '), fwbdata.consadd.Trim(','), fwbdata.consplace.Trim(','), fwbdata.consstate, fwbdata.conscountrycode,
        //                                                 fwbdata.conscontactnum, fwbdata.agentaccnum, fwbdata.agentIATAnumber, fwbdata.agentname, DateTime.Now.ToString("yyyy-MM-dd"),"", "", "", "", "",REFNo, "XFWB",fwbdata.splhandling,fwbdata.chargecode,fwbdata.shipperpostcode,fwbdata.conspostcode,fwbdata.weightcode,
        //            strAWbIssueDate
        //            //DateTime.Now
        //        };

        //        SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
        //                                                      SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
        //                                                      SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.Int,
        //                                                    SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.DateTime};



        //        string procedure = "Messaging.uspInsertBookingDataFromFFR";
        //        flag = dtb.InsertData(procedure, paramname, paramtype, paramvalue);
        //        #endregion

        //        if (flag)
        //        {

        //            if (fltroute.Length > 0)
        //            {
        //                strFlightNo = fltroute[0].carriercode + fltroute[0].fltnum;
        //                strFlightOrigin = fltroute[0].fltdept;
        //                strFlightDestination = fltroute[0].fltarrival;
        //            }

        //            DateTime flightDate = DateTime.UtcNow;
        //            try
        //            {
        //                flightDate = DateTime.ParseExact(flightdate, "dd/MM/yyyy", null);

        //            }
        //            catch (Exception)
        //            {
        //                flightDate = DateTime.UtcNow;
        //            }

        //            string[] CNname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public" };
        //            SqlDbType[] CType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit };
        //            object[] CValues = new object[] { fwbdata.airlineprefix, fwbdata.awbnum, fwbdata.origin, fwbdata.dest, fwbdata.pcscnt, fwbdata.weight, strFlightNo, flightDate, strFlightOrigin, strFlightDestination, "Booked", "AWB Booked", "AWB Booked Through XFWB", "XFWB", DateTime.UtcNow.ToString(), 1 };
        //            if (!dtb.ExecuteProcedure("SPAddAWBAuditLog", CNname, CType, CValues, 600))
        //                clsLog.WriteLog("AWB Audit log  for:" + fwbdata.awbnum + Environment.NewLine + "Error: " + dtb.LastErrorDescription);



        //            #region Check AWB Present or Not
        //            bool isAWBPresent = false;
        //            string strAWBStatus = string.Empty;
        //            string strFltNo = string.Empty;
        //            DataSet dsCheck = new DataSet();
        //            dtb = new SQLServer();
        //            string[] parametername = new string[] { "AWBNumber", "AWBPrefix" };
        //            object[] AWBvalues = new object[] { awbnum, AWBPrefix };
        //            SqlDbType[] ptype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
        //            dsCheck = dtb.SelectRecords("sp_getawbdetails", parametername, AWBvalues, ptype);
        //            if (dsCheck != null)
        //            {
        //                if (dsCheck.Tables.Count > 0)
        //                {
        //                    if (dsCheck.Tables[0].Rows.Count > 0)
        //                    {
        //                        if (dsCheck.Tables[0].Rows[0]["AWBNumber"].ToString().Equals(awbnum, StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            strAWBStatus = dsCheck.Tables[0].Rows[0]["AWBStatus"].ToString();
        //                        }
        //                        if (dsCheck.Tables[3].Rows.Count > 0)
        //                        {
        //                            strFltNo = dsCheck.Tables[3].Rows[0]["FltNumber"].ToString();
        //                            if (strFltNo.Length > 2)
        //                            {
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            if (dsCheck != null)
        //            {
        //                if (dsCheck.Tables.Count > 0)
        //                {
        //                    if (dsCheck.Tables[0].Rows.Count > 0)
        //                    {
        //                        if (dsCheck.Tables[0].Rows[0]["AWBNumber"].ToString().Equals(awbnum, StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            isAWBPresent = true;
        //                        }
        //                    }
        //                }
        //            }
        //            #endregion

        //            #region Save AWB Routing
        //            string[] pname = new string[] { "AWBnumber", "AWBPrefix" };
        //            object[] pvalues = new object[] { awbnum, AWBPrefix };
        //            SqlDbType[] ptypes = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
        //            //DataSet ds = dtb.SelectRecords("spRouteUpateOrNot", pname, pvalues, ptypes);
        //            //if (ds != null && ds.Tables[0].Rows.Count > 0 && (ds.Tables[0].Rows[0]["Value"].ToString() == "TRUE"))
        //            //{
        //            bool isRouteUpdate = false;
        //            // GenericFunction genericFunction = new GenericFunction();
        //            isRouteUpdate = Convert.ToBoolean(gf.ReadValueFromDb("UpdateRouteThroughFWB") == string.Empty ? "false" : gf.ReadValueFromDb("UpdateRouteThroughFWB"));
        //            if ((isRouteUpdate && isAWBPresent) || !isAWBPresent)
        //            {
        //                string status = "C";
        //                if (fltroute.Length > 0)
        //                {
        //                    string[] parname = new string[] { "AWBNum", "AWBPrefix" };
        //                    object[] parobject = new object[] { awbnum, AWBPrefix };
        //                    SqlDbType[] partype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };

        //                    if (dtb.ExecuteProcedure("Messaging.uspDeleteAWBRouteFFR", parname, partype, parobject))
        //                    {
        //                        DateTime dtFlightDate = DateTime.Now;
        //                        for (int lstIndex = 0; lstIndex < fltroute.Length; lstIndex++)
        //                        {
        //                            if (fltroute[lstIndex].date != "")
        //                                dtFlightDate = Convert.ToDateTime(Convert.ToDateTime(fltroute[lstIndex].date).ToString("MM/dd/yyyy"));
        //                            //dtFlightDate = DateTime.Parse((DateTime.Now.Month.ToString().PadLeft(2, '0') + "/" + fltroute[lstIndex].date.PadLeft(2, '0') + "/" + System.DateTime.Now.Year.ToString()));

        //                            //code to check weither flight is valid or not and active
        //                            ///Addeed by prashantz to resolve JIRA# CEBV4-1080
        //                            if (fltroute[lstIndex].fltdept.Trim().ToUpper() != fltroute[lstIndex].fltarrival.Trim().ToUpper())
        //                            {
        //                                string[] parms = new string[]
        //                                {
        //                                    "FltOrigin",
        //                                    "FltDestination",
        //                                    "FlightNo",
        //                                    "flightDate",
        //                                    "AWBNumber",
        //                                    "AWBPrefix",
        //                                    "RefNo"
        //                                };
        //                                SqlDbType[] dataType = new SqlDbType[]
        //                                {
        //                                    SqlDbType.VarChar,
        //                                    SqlDbType.VarChar,
        //                                    SqlDbType.VarChar,
        //                                    SqlDbType.DateTime,
        //                                    SqlDbType.VarChar,
        //                                    SqlDbType.VarChar,
        //                                    SqlDbType.Int
        //                                };
        //                                object[] value = new object[]
        //                                {

        //                                    fltroute[lstIndex].fltdept,
        //                                    fltroute[lstIndex].fltarrival,
        //                                    fltroute[lstIndex].carriercode+fltroute[lstIndex].fltnum,
        //                                    dtFlightDate,
        //                                    awbnum,
        //                                    AWBPrefix,
        //                                    REFNo
        //                                };
        //                                DataSet dsdata = dtb.SelectRecords("spCheckValidFlights", parms, value, dataType);
        //                                int schedid = 0;
        //                                if (dsdata != null && dsdata.Tables[0].Rows[0][0].ToString() == "0")
        //                                {
        //                                    val = false;
        //                                    schedid = Convert.ToInt32(dsdata.Tables[0].Rows[0][0]);
        //                                    break;
        //                                }

        //                                string[] paramNames = new string[]
        //                                {
        //                                    "AWBNumber",
        //                                    "FltOrigin",
        //                                    "FltDestination",
        //                                    "FltNumber",
        //                                    "FltDate",
        //                                    "Status",
        //                                    "UpdatedBy",
        //                                    "UpdatedOn",
        //                                    "IsFFR",
        //                                    "REFNo",
        //                                    "date",
        //                                    "AWBPrefix",
        //                                    "carrierCode",
        //                                     "schedid"
        //                                };
        //                                SqlDbType[] dataTypes = new SqlDbType[]
        //                                {
        //                                    SqlDbType.VarChar,
        //                                    SqlDbType.VarChar,
        //                                    SqlDbType.VarChar,
        //                                    SqlDbType.VarChar,
        //                                    SqlDbType.DateTime,
        //                                    SqlDbType.VarChar,
        //                                    SqlDbType.VarChar,
        //                                    SqlDbType.DateTime,
        //                                    SqlDbType.Bit,
        //                                    SqlDbType.Int,
        //                                    SqlDbType.DateTime,
        //                                    SqlDbType.VarChar,
        //                                    SqlDbType.VarChar,
        //                                    SqlDbType.Int
        //                                };

        //                                object[] values = new object[]
        //                                {
        //                                    awbnum,
        //                                    fltroute[lstIndex].fltdept,
        //                                    fltroute[lstIndex].fltarrival,
        //                                    fltroute[lstIndex].carriercode+fltroute[lstIndex].fltnum,
        //                                    dtFlightDate,
        //                                    status,
        //                                    "XFWB",
        //                                     DateTime.Now,
        //                                    1,
        //                                    0,
        //                                    dtFlightDate,
        //                                    AWBPrefix,
        //                                    fltroute[lstIndex].carriercode,
        //                                    schedid
        //                                };


        //                                if (!dtb.UpdateData("spSaveFFRAWBRoute", paramNames, dataTypes, values))
        //                                    clsLog.WriteLogAzure("Error in Save AWB Route XFWB " + dtb.LastErrorDescription);
        //                                string[] CANname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public" };
        //                                SqlDbType[] CAType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit };
        //                                object[] CAValues = new object[] { fwbdata.airlineprefix, fwbdata.awbnum, fwbdata.origin, fwbdata.dest, fwbdata.pcscnt, fwbdata.weight, fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum, dtFlightDate.ToString(), fltroute[lstIndex].fltdept, fltroute[lstIndex].fltarrival, "Booked", "AWB Booked", "AWB Flight Information", "XFWB", DateTime.UtcNow.ToString(), 1 };
        //                                if (!dtb.ExecuteProcedure("SPAddAWBAuditLog", CANname, CAType, CAValues))
        //                                    clsLog.WriteLog("AWB Audit log  for:" + fwbdata.awbnum + Environment.NewLine + "Error: " + dtb.LastErrorDescription);
        //                            }
        //                        }
        //                        if (val)
        //                        {
        //                            string[] QueryNames = { "AWBPrefix", "AWBNumber" };
        //                            SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar };
        //                            object[] QueryValues = { AWBPrefix, awbnum };
        //                            if (!dtb.UpdateData("spDeleteAWBDetailsNoRoute", QueryNames, QueryTypes, QueryValues))
        //                                clsLog.WriteLogAzure("Error in Deleting AWB Details " + dtb.LastErrorDescription);
        //                        }

        //                    }
        //                }

        //            }

        //            #endregion Save AWB Routing
        //            if (val)
        //            {
        //                #region Rate Decription
        //                string freight = "0", paymode = "PX", valcharge = "0", tax = "0", OCDA = "0", OCDC = "0", total = "0", currency = string.Empty;
        //                decimal DeclareCarriageValue = 0, DeclareCustomValue = 0;
        //                currency = fwbdata.currency;
        //                paymode = fwbdata.chargecode == "" ? fwbdata.chargedec : fwbdata.chargecode;

        //                if (fwbdata.declaredvalue != "")
        //                {
        //                    DeclareCarriageValue = decimal.Parse(fwbdata.declaredvalue == "NVD" ? "0" : fwbdata.declaredvalue);
        //                }
        //                if (fwbdata.declaredcustomvalue != "")
        //                {
        //                    DeclareCustomValue = decimal.Parse(fwbdata.declaredcustomvalue == "NCV" ? "0" : fwbdata.declaredcustomvalue);
        //                }


        //                if (fwbdata.PPweightCharge.Length > 0 || fwbdata.PPValuationCharge.Length > 0 || fwbdata.PPTaxesCharge.Length > 0 ||
        //                    fwbdata.PPOCDA.Length > 0 || fwbdata.PPOCDC.Length > 0 || fwbdata.PPTotalCharges.Length > 0)
        //                {
        //                    freight = fwbdata.PPweightCharge.Length > 0 ? fwbdata.PPweightCharge : "0";
        //                    valcharge = fwbdata.PPValuationCharge.Length > 0 ? fwbdata.PPValuationCharge : "0";
        //                    tax = fwbdata.PPTaxesCharge.Length > 0 ? fwbdata.PPTaxesCharge : "0";
        //                    OCDC = fwbdata.PPOCDC.Length > 0 ? fwbdata.PPOCDC : "0";
        //                    OCDA = fwbdata.PPOCDA.Length > 0 ? fwbdata.PPOCDA : "0";
        //                    total = fwbdata.PPTotalCharges.Length > 0 ? fwbdata.PPTotalCharges : "0";
        //                }

        //                if (fwbdata.CCweightCharge.Length > 0 || fwbdata.CCValuationCharge.Length > 0 || fwbdata.CCTaxesCharge.Length > 0 ||
        //                    fwbdata.CCOCDA.Length > 0 || fwbdata.CCOCDC.Length > 0 || fwbdata.CCTotalCharges.Length > 0)
        //                {
        //                    freight = fwbdata.CCweightCharge.Length > 0 ? fwbdata.CCweightCharge : "0";
        //                    valcharge = fwbdata.CCValuationCharge.Length > 0 ? fwbdata.CCValuationCharge : "0";
        //                    tax = fwbdata.CCTaxesCharge.Length > 0 ? fwbdata.CCTaxesCharge : "0";
        //                    OCDC = fwbdata.CCOCDC.Length > 0 ? fwbdata.CCOCDC : "0";
        //                    OCDA = fwbdata.CCOCDA.Length > 0 ? fwbdata.CCOCDA : "0";
        //                    total = fwbdata.CCTotalCharges.Length > 0 ? fwbdata.CCTotalCharges : "0";
        //                }

        //                for (int i = 0; i < fwbrates.Length; i++)
        //                {
        //                    fwbrates[i].chargeamt = fwbrates[i].chargeamt.Length > 0 ? fwbrates[i].chargeamt : "0";
        //                    fwbrates[i].awbweight = fwbrates[i].awbweight.Length > 0 ? fwbrates[i].awbweight : "0";
        //                    fwbrates[i].weight = fwbrates[i].weight.Length > 0 ? fwbrates[i].weight : "0";
        //                    fwbrates[i].chargerate = fwbrates[i].chargerate.Length > 0 ? fwbrates[i].chargerate : freight;
        //                    fwbrates[i].rateclasscode = fwbrates[i].rateclasscode;
        //                    string[] param = new string[]
        //                    {
        //                    "AWBNumber",
        //                    "CommCode",
        //                    "PayMode",
        //                    "Pcs",
        //                    "Wt",
        //                    "FrIATA",
        //                    "FrMKT",
        //                    "ValCharge",
        //                    "OcDueCar",
        //                    "OcDueAgent",
        //                    "SpotRate",
        //                    "DynRate",
        //                    "ServiceTax",
        //                    "Total",
        //                    "RatePerKg",
        //                    "Currency",
        //                    "AWBPrefix",
        //                    "ChargeableWeight",
        //                    "DeclareCarriageValue",
        //                    "DeclareCustomValue",
        //                      "RateClass"
        //                    };
        //                    SqlDbType[] dbtypes = new SqlDbType[]
        //                    {
        //                    SqlDbType.VarChar,
        //                    SqlDbType.VarChar,
        //                    SqlDbType.VarChar,
        //                    SqlDbType.Int,
        //                    SqlDbType.Float,
        //                    SqlDbType.Float,
        //                    SqlDbType.Float,
        //                    SqlDbType.Float,
        //                    SqlDbType.Float,
        //                    SqlDbType.Float,
        //                    SqlDbType.Float,
        //                    SqlDbType.Float,
        //                    SqlDbType.Float,
        //                    SqlDbType.Float,
        //                    SqlDbType.Decimal,
        //                    SqlDbType.VarChar,
        //                    SqlDbType.VarChar,
        //                    SqlDbType.Float,
        //                    SqlDbType.Float,
        //                    SqlDbType.Float,
        //                    SqlDbType.VarChar,
        //                    };
        //                    object[] values = new object[]
        //                    {
        //                    awbnum,
        //                    commcode,
        //                    paymode,
        //                    Convert.ToInt16(fwbrates[i].numofpcs),
        //                    float.Parse(fwbrates[i].weight),
        //                    float.Parse(freight),
        //                    float.Parse(fwbrates[i].chargeamt),
        //                    float.Parse(valcharge),
        //                    float.Parse(OCDC),
        //                    float.Parse(OCDA),
        //                    0,
        //                    0,
        //                    float.Parse(tax),
        //                    float.Parse(total),
        //                    Convert.ToDecimal(fwbrates[i].chargerate),
        //                    currency,
        //                    AWBPrefix,
        //                   float.Parse(fwbrates[i].awbweight),
        //                    DeclareCarriageValue,
        //                    DeclareCustomValue,
        //                    fwbrates[i].rateclasscode
        //                    };

        //                    if (!dtb.UpdateData("SP_SaveAWBRatesviaMsg", param, dbtypes, values))
        //                        clsLog.WriteLogAzure("Error Saving XFWB rates for:" + awbnum);

        //                }

        //                #endregion

        //                #region Other Charges
        //                //check for other charge exists in systme or not


        //                for (int i = 0; i < OtherCharges.Length; i++)
        //                {
        //                    string[] param = { "AWBNumber", "ChargeHeadCode", "ChargeType", "DiscountPercent",
        //                               "CommPercent", "TaxPercent", "Discount", "Comission", "Tax","Charge","CommCode","AWBPrefix"};
        //                    SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Float,
        //                                    SqlDbType.Float, SqlDbType.Float, SqlDbType.Float, SqlDbType.Float, SqlDbType.Float,SqlDbType.Float,SqlDbType.VarChar,SqlDbType.VarChar};

        //                    object[] values = { awbnum, OtherCharges[i].otherchargecode, "D" + OtherCharges[i].entitlementcode, 0, 0, 0, 0, 0, 0, OtherCharges[i].chargeamt, commcode, AWBPrefix };

        //                    if (!dtb.InsertData("SP_SaveAWBOCRatesDetails", param, dbtypes, values))
        //                        clsLog.WriteLogAzure("Error Saving XFWB OCRates for:" + awbnum);

        //                }
        //                #endregion

        //                #region AWB Dimensions

        //                if (objDimension.Length > 0)
        //                {
        //                    //Badiuz khan
        //                    //Description: Delete Dimension if Dimension 
        //                    string[] dparam = { "AWBPrefix", "AWBNumber" };
        //                    SqlDbType[] dbparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar };
        //                    object[] dbparamvalues = { AWBPrefix, awbnum };

        //                    if (!dtb.InsertData("SpDeleteDimensionThroughMessage", dparam, dbparamtypes, dbparamvalues))
        //                        clsLog.WriteLogAzure("Error  Delete Dimension Through Message :" + awbnum);
        //                    else
        //                    {

        //                        for (int i = 0; i < objDimension.Length; i++)
        //                        {
        //                            if (objDimension[i].mesurunitcode.Trim() != "")
        //                            {
        //                                if (objDimension[i].mesurunitcode.Trim().ToUpper() == "CMT")
        //                                {
        //                                    objDimension[i].mesurunitcode = "Cms";
        //                                }
        //                                else if (objDimension[i].mesurunitcode.Trim().ToUpper() == "INH")
        //                                {
        //                                    objDimension[i].mesurunitcode = "Inches";
        //                                }
        //                            }
        //                            if (objDimension[i].length.Trim() == "")
        //                            {
        //                                objDimension[i].length = "0";
        //                            }
        //                            if (objDimension[i].width.Trim() == "")
        //                            {
        //                                objDimension[i].width = "0";
        //                            }
        //                            if (objDimension[i].height.Trim() == "")
        //                            {
        //                                objDimension[i].height = "0";
        //                            }

        //                            string[] param = { "AWBNumber", "RowIndex", "Length", "Breadth", "Height", "PcsCount", "MeasureUnit", "AWBPrefix", "Weight", "WeightCode", "UpdatedBy" };
        //                            SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Decimal, SqlDbType.VarChar, SqlDbType.VarChar };
        //                            Decimal DimWeight = 0;

        //                            object[] value ={awbnum,"1",objDimension[i].length,objDimension[i].width,objDimension[i].height,
        //                                    objDimension[i].piecenum,objDimension[i].mesurunitcode,AWBPrefix,Decimal.TryParse(objDimension[i].weight,out DimWeight)==true?Convert.ToDecimal(objDimension[i].weight):0,objDimension[i].weightcode,"XFWB"};

        //                            if (!dtb.InsertData("SP_SaveAWBDimensions_FFR", param, dbtypes, value))
        //                            {
        //                                clsLog.WriteLogAzure("Error Saving  Dimension Through Message :" + awbnum);
        //                            }
        //                        }
        //                    }
        //                }

        //                #endregion

        //                #region XFWB Message with BUP Shipment
        //                //Badiuz khan
        //                //Description: Save Bup through XFWB
        //                decimal VolumeWt = 0;
        //                if (objAWBBup.Length > 0)
        //                {
        //                    if (fwbrates[0].volcode != "")
        //                    {
        //                        switch (fwbrates[0].volcode.ToUpper())
        //                        {
        //                            case "MC":
        //                                VolumeWt = decimal.Parse(String.Format("{0:0.00}", Convert.ToDecimal(Convert.ToDecimal(fwbrates[0].volamt == "" ? "0" : fwbrates[0].volamt) * decimal.Parse("166.66"))));
        //                                break;
        //                            default:
        //                                VolumeWt = Convert.ToDecimal(fwbrates[0].volamt == "" ? "0" : fwbrates[0].volamt);
        //                                break;
        //                        }
        //                    }

        //                    for (int k = 0; k < objAWBBup.Length; k++)
        //                    {
        //                        if (objAWBBup[k].ULDNo != "" && objAWBBup[k].ULDNo != null)
        //                        {
        //                            string uldno = objAWBBup[k].ULDNo;
        //                            int uldslacPcs = int.Parse(objAWBBup[k].SlacCount == "" ? "0" : objAWBBup[k].SlacCount);
        //                            string[] param = { "AWBPrefix", "AWBNumber", "ULDNo", "SlacPcs", "PcsCount", "Volume", "GrossWeight" };
        //                            SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.Decimal, SqlDbType.Decimal };
        //                            object[] value = { AWBPrefix, awbnum, uldno, uldslacPcs, fwbdata.pcscnt, VolumeWt, decimal.Parse(fwbdata.weight == "" ? "0" : fwbdata.weight) };

        //                            if (!dtb.InsertData("SaveandUpdateShippperBUPThroughFWB", param, dbtypes, value))
        //                            {
        //                                string str = dtb.LastErrorDescription.ToString();
        //                                clsLog.WriteLogAzure("BUP ULD is not Updated  for:" + awbnum + Environment.NewLine + "Error : " + dtb.LastErrorDescription);

        //                            }
        //                        }
        //                    }
        //                }

        //                #endregion

        //                #region ProcessRateFunction

        //                DataSet dsrateCheck = ffR.CheckAirlineForRateProcessing(AWBPrefix, "XFWB");
        //                if (dsrateCheck != null && dsrateCheck.Tables.Count > 0 && dsrateCheck.Tables[0].Rows.Count > 0)
        //                {
        //                    string[] CRNname = new string[] { "AWBNumber", "AWBPrefix", "UpdatedBy", "UpdatedOn", "ValidateMin", "UpdateBooking", "RouteFrom", "UpdateBilling" };
        //                    SqlDbType[] CRType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Bit, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.Bit };
        //                    object[] CRValues = new object[] { awbnum, AWBPrefix, "XFWB", System.DateTime.Now, 1, 1, "B", 0 };
        //                    //if (!dtb.ExecuteProcedure("sp_CalculateFreightChargesforMessage", "AWBNumber", SqlDbType.VarChar, awbnum))
        //                    if (!dtb.ExecuteProcedure("sp_CalculateAWBRatesReprocess", CRNname, CRType, CRValues))
        //                    {
        //                        clsLog.WriteLogAzure("Rates Not Calculated for:" + awbnum + Environment.NewLine + "Error: " + dtb.LastErrorDescription);
        //                    }
        //                }

        //                #endregion

        //                string[] QueryName = { "AWBNumber", "Status", "AWBPrefix", "UserName" };
        //                SqlDbType[] QueryType = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
        //                object[] QueryValue = { awbnum, "E", AWBPrefix, "XFWB" };
        //                if (!dtb.UpdateData("UpdateStatustoExecuted", QueryName, QueryType, QueryValue))
        //                    clsLog.WriteLogAzure("Error in updating AWB status" + dtb.LastErrorDescription);

        //                #region capacity
        //                string[] cparam = { "AWBPrefix", "AWBNumber" };
        //                SqlDbType[] cparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar };
        //                object[] cparamvalues = { AWBPrefix, awbnum };

        //                if (!dtb.InsertData("UpdateCapacitythroughMessage", cparam, cparamtypes, cparamvalues))
        //                    clsLog.WriteLogAzure("Error  on Update capacity Plan :" + awbnum);

        //                #endregion

        //            }
        //        }
        //        else
        //        {
        //            clsLog.WriteLogAzure("Error while save XFWB Message:" + awbnum + "-" + dtb.LastErrorDescription);
        //        }

        //    }

        //    catch (Exception ex)
        //    {
        //        //SCMExceptionHandling.logexception(ref ex);
        //        clsLog.WriteLogAzure("Error on XFWB  message:" + ex.ToString());
        //        ErrorMsg = string.Empty;
        //        flag = false;
        //    }
        //    return flag;
        //}
        #endregion

        public async Task<(bool success, string ErrorMsg)> SaveandValidateFWBMessage(MessageData.fwbinfo fwbdata, MessageData.FltRoute[] fltroute,
MessageData.othercharges[] OtherCharges, MessageData.otherserviceinfo[] othinfoarray,
MessageData.RateDescription[] fwbrates, MessageData.customsextrainfo[] customextrainfo,
MessageData.dimensionnfo[] objDimension, int REFNo, MessageData.AWBBuildBUP[] objAWBBup,
string strMessage, string strMessageFrom, string strFromID, string strStatus, string ErrorMsg)
        {
            bool flag = false, isUpdateDIMSWeight = false;
            string Priority = string.Empty;
            try
            {
                ErrorMsg = string.Empty;

                //FFRMessageProcessor ffR = new FFRMessageProcessor();
                //SQLServer dtb = new SQLServer();

                MessageData.FltRoute[] objRouteInfo = new MessageData.FltRoute[0];
                string awbnum = fwbdata.awbnum; string Numberofposition = "0";
                string AWBPrefix = fwbdata.airlineprefix;
                string flightnum = "NA", commcode = "", commtype = string.Empty;
                string flightdate = System.DateTime.Now.ToString("dd/MM/yyyy");
                string strFlightNo = string.Empty, strFlightOrigin = string.Empty, strFlightDestination = string.Empty;
                bool val = true;
                string Slac = string.Empty;
                string AWBOriginAirportCode = string.Empty, AWBDestAirportCode = string.Empty;
                string fltDate = string.Empty;
                string FltOrg = string.Empty, FltDest = string.Empty;
                string strErrorMessage = string.Empty;
                string fltMonth = "";

                //GenericFunction gf = new GenericFunction();

                //XFNMMessageProcessor fna = new XFNMMessageProcessor();

                await _genericFunction.UpdateInboxFromMessageParameter(REFNo, AWBPrefix + "-" + awbnum, fltroute[0].fltnum, string.Empty,
                    string.Empty, "XFWB", strMessageFrom == "" ? strFromID : strMessageFrom, DateTime.Parse("1900-01-01"));


                if (strFromID.Contains("SITA"))
                {
                    commtype = "SITAFTP";
                }
                else
                {
                    commtype = "EMAIL";
                }

                if ((fwbdata.fwbPurposecode).Equals("Deletion", StringComparison.OrdinalIgnoreCase) || (fwbdata.fwbPurposecode).Equals("CANCELLED", StringComparison.OrdinalIgnoreCase))
                {
                    DataSet? dsCheck = new DataSet();
                    string errormessage = string.Empty;

                    //string[] parametername = new string[] { "AWBNumber", "AWBPrefix" };
                    //object[] AWBvalues = new object[] { awbnum, AWBPrefix };
                    //SqlDbType[] ptype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };

                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                    };

                    //dsCheck = dtb.SelectRecords("sp_getawbdetails", parametername, AWBvalues, ptype);
                    dsCheck = await _readWriteDao.SelectRecords("sp_getawbdetails", parameters);

                    if (dsCheck != null)
                    {
                        if (dsCheck.Tables.Count > 0)
                        {
                            if (dsCheck.Tables[0].Rows.Count > 0)
                            {
                                if ((fwbdata.fwbPurposecode).Equals("Deletion", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (dsCheck.Tables[0].Rows[0]["AWBNumber"].ToString().Equals(awbnum, StringComparison.OrdinalIgnoreCase) &&
                                        !dsCheck.Tables[0].Rows[0]["AWBStatus"].ToString().Equals("E", StringComparison.OrdinalIgnoreCase))
                                    {

                                        //if (!SetAWBStatus(fwbdata.awbnum, "D", ref errormessage, DateTime.Now, "xFWB",
                                        //    Convert.ToDateTime(fwbdata.updatedondate + " " + fwbdata.updatedontime),
                                        //fwbdata.airlineprefix, false, "", 0, fwbdata.origin,
                                        //Convert.ToDateTime(fwbdata.updatedondate + " " + fwbdata.Recivedontime), REFNo, fwbdata.fwbPurposecode))

                                        bool success = false;

                                        (success, errormessage) = await SetAWBStatus(fwbdata.awbnum, "D", errormessage, DateTime.Now, "xFWB",
                                            Convert.ToDateTime(fwbdata.updatedondate + " " + fwbdata.updatedontime),
                                        fwbdata.airlineprefix, false, "", 0, fwbdata.origin,
                                        Convert.ToDateTime(fwbdata.updatedondate + " " + fwbdata.Recivedontime), REFNo, fwbdata.fwbPurposecode);

                                        if (!success)
                                        {

                                            // clsLog.WriteLogAzure("Error for deleteing AWB throught XFWB:" + errormessage + " " + awbnum);
                                            _logger.LogWarning($"Error for deleteing AWB throught XFWB: {errormessage} {awbnum}");
                                            ErrorMsg = "Error for deleteing AWB throught XFWB:" + errormessage + " " + awbnum;
                                            flag = false;
                                        }
                                        else
                                        {
                                            flag = true;
                                        }

                                    }
                                    else
                                    {
                                        ErrorMsg = " Error for deleteing AWB throught XFWB because AWB is in Execute status:" + awbnum;
                                        flag = false;
                                    }
                                }
                                else
                                {
                                    if (dsCheck.Tables[0].Rows[0]["AWBNumber"].ToString().Equals(awbnum, StringComparison.OrdinalIgnoreCase))
                                    {
                                        //string[] paramname = new string[] { "AirlinePrefix", "AWBNo", };

                                        //object[] paramvalue = new object[] { fwbdata.airlineprefix, fwbdata.awbnum };

                                        //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };

                                        SqlParameter[] sqlParameters = new SqlParameter[]
                                        {
                                            new SqlParameter("@AirlinePrefix", SqlDbType.VarChar) { Value = fwbdata.airlineprefix },
                                            new SqlParameter("@AWBNo", SqlDbType.VarChar) { Value = fwbdata.awbnum }
                                        };

                                        string procedure = "Messaging.uspUpdateRouteFromXFWB";

                                        //flag = dtb.InsertData(procedure, paramname, paramtype, paramvalue);
                                        flag = await _readWriteDao.ExecuteNonQueryAsync(procedure, sqlParameters);

                                        if (!flag)
                                        {

                                            // clsLog.WriteLogAzure("Error for CANCELLED AWB throught XFWB:" + errormessage + " " + awbnum);
                                            _logger.LogWarning($"Error for CANCELLED AWB throught XFWB: {errormessage} {awbnum}");
                                            ErrorMsg = "Error for CANCELLED AWB throught XFWB:" + errormessage + " " + awbnum;
                                            flag = false;
                                        }
                                        else
                                        {
                                            DateTime flightDate = DateTime.UtcNow;
                                            try
                                            {
                                                flightDate = DateTime.ParseExact(flightdate, "dd/MM/yyyy", null);

                                            }
                                            catch (Exception)
                                            {
                                                flightDate = DateTime.UtcNow;
                                            }

                                            // string[] CANname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight",
                                            //"FlightNo","FlightDate","FlightOrigin","FlightDestination" ,"Action", "Message", "Description",
                                            // "UpdatedBy", "UpdatedOn", "Public", "Station" };
                                            // SqlDbType[] CAType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                                            //     SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar,
                                            // SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                                            // SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar };
                                            // //object[] CAValues = new object[] { fwbdata.airlineprefix, fwbdata.awbnum, fwbdata.origin,
                                            // //fwbdata.dest, fwbdata.pcscnt, fwbdata.weight, fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum,
                                            // //dtFlightDate_format, fltroute[lstIndex].fltdept, fltroute[lstIndex].fltarrival,
                                            // //"Booked", "AWB Booked", "AWB Flight Information", "xFWB", DateTime.UtcNow.ToString("yyyy-MM-dd"), 1 };
                                            // object[] CAValues = new object[] { fwbdata.airlineprefix, fwbdata.awbnum, fwbdata.origin,
                                            // fwbdata.dest, fwbdata.pcscnt, fwbdata.weight,"",(fwbdata.updatedondate+" "+fwbdata.updatedontime),fwbdata.origin,fwbdata.dest,
                                            // "Cancelled", "AWB Cancelled", "AWB Cancelled Through xFWB", "xFWB",(fwbdata.updatedondate+" "+fwbdata.updatedontime)
                                            //     //DateTime.UtcNow.ToString()
                                            // , 1,fwbdata.carrierplace };

                                            SqlParameter[] sqlParams = new SqlParameter[]
                                            {
                                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = fwbdata.airlineprefix },
                                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = fwbdata.awbnum },
                                                new SqlParameter("@Origin", SqlDbType.VarChar) { Value = fwbdata.origin },
                                                new SqlParameter("@Destination", SqlDbType.VarChar) { Value = fwbdata.dest },
                                                new SqlParameter("@Pieces", SqlDbType.VarChar) { Value = fwbdata.pcscnt },
                                                new SqlParameter("@Weight", SqlDbType.VarChar) { Value = fwbdata.weight },
                                                new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = "" },
                                                new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = (fwbdata.updatedondate+" "+fwbdata.updatedontime) },
                                                new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = fwbdata.origin },
                                                new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = fwbdata.dest },
                                                new SqlParameter("@Action", SqlDbType.VarChar) { Value = "Cancelled" },
                                                new SqlParameter("@Message", SqlDbType.VarChar) { Value = "AWB Cancelled" },
                                                new SqlParameter("@Description", SqlDbType.VarChar) { Value = "AWB Cancelled Through xFWB" },
                                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "xFWB" },
                                                new SqlParameter("@UpdatedOn", SqlDbType.VarChar) { Value = (fwbdata.updatedondate+" "+fwbdata.updatedontime) },
                                                //new SqlParameter("@UpdatedOn", SqlDbType.VarChar) { Value = DateTime.UtcNow.ToString() },
                                                new SqlParameter("@Public", SqlDbType.Bit) { Value = 1 },
                                                new SqlParameter("@Station", SqlDbType.VarChar) { Value = fwbdata.carrierplace }
                                            };

                                            //if (!dtb.ExecuteProcedure("SPAddAWBAuditLog", CANname, CAType, CAValues))
                                            if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", sqlParams))
                                                // clsLog.WriteLog("AWB Audit log  for:" + fwbdata.awbnum + Environment.NewLine);
                                                _logger.LogWarning($"AWB Audit log for: {fwbdata.awbnum+ Environment.NewLine}");

                                            flag = true;
                                        }

                                    }


                                }

                            }
                        }
                        else
                        {
                            ErrorMsg = "Error for " + (fwbdata.fwbPurposecode) + " AWB throught XFWB because AWB entered is not Physical AWB: " + awbnum;
                            flag = false;
                        }
                    }



                }
                else
                {

                    if (fltroute.Length > 0)
                    {
                        #region : Check flight extsts or not in schedule :
                        bool isCheckValidFlight = false;
                        DataSet? dsawbFlt = new DataSet();

                        isCheckValidFlight = Convert.ToBoolean(_genericFunction.ReadValueFromDb("ChkFltPresentAndAWBStatus") == string.Empty ? "false" : _genericFunction.ReadValueFromDb("ChkFltPresentAndAWBStatus"));
                        if (isCheckValidFlight)
                        {
                            bool isFlightValid = false;
                            for (int lstIndex = 0; lstIndex < fltroute.Length; lstIndex++)
                            {
                                if (fltroute[lstIndex].carriercode.Trim().Length + fltroute[lstIndex].fltnum.Trim().Length > 2)
                                {

                                    #region : Switch Flight Month :

                                    fltMonth = fltroute[lstIndex].month.Trim().ToUpper();
                                    switch (fltroute[lstIndex].month.Trim().ToUpper())
                                    {
                                        case "JAN":
                                            {
                                                fltMonth = "01";
                                                break;
                                            }
                                        case "FEB":
                                            {
                                                fltMonth = "02";
                                                break;
                                            }
                                        case "MAR":
                                            {
                                                fltMonth = "03";
                                                break;
                                            }
                                        case "APR":
                                            {
                                                fltMonth = "04";
                                                break;
                                            }
                                        case "MAY":
                                            {
                                                fltMonth = "05";
                                                break;
                                            }
                                        case "JUN":
                                            {
                                                fltMonth = "06";
                                                break;
                                            }
                                        case "JUL":
                                            {
                                                fltMonth = "07";
                                                break;
                                            }
                                        case "AUG":
                                            {
                                                fltMonth = "08";
                                                break;
                                            }
                                        case "SEP":
                                            {
                                                fltMonth = "09";
                                                break;
                                            }
                                        case "OCT":
                                            {
                                                fltMonth = "10";
                                                break;
                                            }
                                        case "NOV":
                                            {
                                                fltMonth = "11";
                                                break;
                                            }
                                        case "DEC":
                                            {
                                                fltMonth = "12";
                                                break;
                                            }
                                    }
                                    //fltDate = fltMonth.PadLeft(2, '0') + "/" + fltroute[lstIndex].date.ToString().PadLeft(2, '0') + "/" + System.DateTime.Now.Year.ToString();
                                    //fltDate = fltMonth.PadLeft(2, '0') + "/" + fltroute[lstIndex].date.ToString().Substring(8, 2) + "/" + System.DateTime.Now.Year.ToString();
                                    //fltDate = System.DateTime.Now.Year.ToString() + "/" + fltMonth.PadLeft(2, '0') + "/" + fltroute[lstIndex].date.ToString().Substring(8, 2);
                                    fltDate = fltroute[lstIndex].date.ToString().Substring(0, 4) + "/" + fltMonth.PadLeft(2, '0') + "/" + fltroute[lstIndex].date.ToString().Substring(8, 2);
                                    //string a = fltMonth.PadLeft(2, '0');
                                    //string b = fltroute[lstIndex].date.ToString().Substring(8, 2);
                                    //string c= System.DateTime.Now.Year.ToString();
                                    ///Find out if flight date with current year is less than server date time by at least 100 days.
                                    DateTime dtFlightDate = DateTime.Now;
                                    //if (DateTime.TryParseExact(fltDate, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out dtFlightDate))
                                    //{
                                    //    if (DateTime.Now.AddDays(-100) > dtFlightDate)
                                    //    {   ///Advance year in flight date to next year.
                                    //        dtFlightDate = dtFlightDate.AddYears(1);
                                    //        fltDate = dtFlightDate.ToString("MM/dd/yyyy");
                                    //    }
                                    //}

                                    //string date = dtFlightDate.ToString("MM/dd/yyyy");
                                    string date = dtFlightDate.ToString("yyyy/MM/dd");

                                    #endregion Switch Flight Month

                                    if (fltroute.Length > 1)
                                    {
                                        if (lstIndex == 0)
                                        {
                                            dsawbFlt = await _fFRMessageProcessor.CheckValidateXFFRMessage(AWBPrefix, awbnum, fwbdata.origin, fwbdata.dest, "xFWB", fltroute[lstIndex].fltdept, fltroute[lstIndex].fltarrival, fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum, fltDate);
                                            if (dsawbFlt != null && dsawbFlt.Tables.Count > 0 && dsawbFlt.Tables[0].Columns.Contains("ErrorMessage"))
                                            {
                                                strErrorMessage = dsawbFlt.Tables[0].Rows[0]["ErrorMessage"].ToString();
                                                ErrorMsg = strErrorMessage;
                                                //return flag = false;
                                                return (false, ErrorMsg);
                                            }
                                            else
                                            {
                                                //FltOrg = dsawbFlt.Tables[0].Rows[0]["AWBOriginAirportCode"].ToString();
                                                ////FltDest = dsawbFlt.Tables[0].Rows[0]["AWBDestAirportCode"].ToString();
                                                //FltDest = fltroute[lstIndex].fltarrival;
                                                //if (FltOrg.Trim() != string.Empty)
                                                //{
                                                //    fltroute[lstIndex].fltdept = FltOrg;
                                                //}

                                                FltOrg = fltroute[lstIndex].fltdept;
                                                FltDest = fltroute[lstIndex].fltarrival;
                                                if (FltDest == "")
                                                {
                                                    FltDest = dsawbFlt.Tables[0].Rows[0]["AWBDestAirportCode"].ToString();
                                                }
                                                if (FltOrg == "")
                                                {
                                                    FltOrg = dsawbFlt.Tables[0].Rows[0]["AWBOriginAirportCode"].ToString();

                                                }
                                            }
                                        }
                                        else
                                        {
                                            FltOrg = fltroute[lstIndex].fltdept;
                                            FltDest = fltroute[lstIndex].fltarrival;
                                        }
                                    }
                                    else
                                    {
                                        dsawbFlt = await _fFRMessageProcessor.CheckValidateXFFRMessage(AWBPrefix, awbnum, fwbdata.origin, fwbdata.dest, "xFWB", fltroute[lstIndex].fltdept, fltroute[fltroute.Length - 1].fltarrival, fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum, fltDate);

                                        if (dsawbFlt != null && dsawbFlt.Tables.Count > 0 && dsawbFlt.Tables[0].Columns.Contains("ErrorMessage"))
                                        {
                                            strErrorMessage = dsawbFlt.Tables[0].Rows[0]["ErrorMessage"].ToString();
                                            ErrorMsg = strErrorMessage;
                                            //return flag = false;
                                            return (false, ErrorMsg);

                                        }
                                        else
                                        {

                                            FltOrg = fltroute[lstIndex].fltdept;
                                            FltDest = fltroute[lstIndex].fltarrival;
                                            if (FltDest == "")
                                            {
                                                FltDest = dsawbFlt.Tables[0].Rows[0]["AWBDestAirportCode"].ToString();
                                            }
                                            if (FltOrg == "")
                                            {
                                                FltOrg = dsawbFlt.Tables[0].Rows[0]["AWBOriginAirportCode"].ToString();

                                            }
                                        }

                                    }
                                }
                                else
                                {
                                    fltDate = DateTime.Now.ToShortDateString();

                                    if (fltroute.Length > 1)
                                    {
                                        if (lstIndex == 0)
                                        {
                                            //clsLog.WriteLogAzure("AWBPrefix: " + AWBPrefix + ";\r\n awbnum:" + awbnum + ";\r\n fwbdata.origin:" + fwbdata.origin + ";\r\n fwbdata.dest:" + fwbdata.dest + ";\r\n fltroute[lstIndex].fltdept:" + fltroute[lstIndex].fltdept + ";\r\n fltroute[lstIndex].fltarrival" + fltroute[lstIndex].fltarrival + ";\r\n fltroute[lstIndex].carriercode:" + fltroute[lstIndex].carriercode + ";\r\n fltroute[lstIndex].fltnum:" + fltroute[lstIndex].fltnum + ";\r\n fltDate:" + fltDate);

                                            dsawbFlt = await _fFRMessageProcessor.CheckValidateXFFRMessage(AWBPrefix, awbnum, fwbdata.origin, fwbdata.dest, "xFWB", fltroute[lstIndex].fltdept, fltroute[lstIndex].fltarrival, fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum, fltDate);
                                            if (dsawbFlt != null && dsawbFlt.Tables.Count > 0 && dsawbFlt.Tables[0].Columns.Contains("ErrorMessage"))
                                            {
                                                strErrorMessage = dsawbFlt.Tables[1].Rows[0]["ErrorMessage"].ToString();
                                                ErrorMsg = strErrorMessage;
                                                //return flag = false;
                                                return (false, ErrorMsg);
                                            }
                                            else
                                            {
                                                FltOrg = dsawbFlt.Tables[0].Rows[0]["AWBOriginAirportCode"].ToString();

                                                FltDest = fltroute[lstIndex].fltarrival;
                                                if (FltOrg.Trim() != string.Empty)
                                                {
                                                    fltroute[lstIndex].fltdept = FltOrg;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            FltOrg = fltroute[lstIndex].fltdept;
                                            FltDest = fltroute[lstIndex].fltarrival;
                                        }
                                    }
                                    else
                                    {
                                        dsawbFlt = await _fFRMessageProcessor.CheckValidateXFFRMessage(AWBPrefix, awbnum, fwbdata.origin, fwbdata.dest, "xFWB", fltroute[lstIndex].fltdept, fltroute[fltroute.Length - 1].fltarrival, fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum, fltDate);

                                        if (dsawbFlt != null && dsawbFlt.Tables.Count > 0 && dsawbFlt.Tables[0].Columns.Contains("ErrorMessage"))
                                        {
                                            strErrorMessage = dsawbFlt.Tables[1].Rows[0]["ErrorMessage"].ToString();
                                            ErrorMsg = strErrorMessage;
                                            //return flag = false;

                                            return (false, ErrorMsg);
                                        }
                                        else
                                        {
                                            FltOrg = dsawbFlt.Tables[0].Rows[0]["AWBOriginAirportCode"].ToString();
                                            FltDest = fltroute[lstIndex].fltarrival;
                                        }
                                    }
                                }

                                #region : Check Valid Flights :

                                //string[] parms = new string[]
                                //    {
                                //        "FltOrigin",
                                //        "FltDestination",
                                //        "FlightNo",
                                //        "flightDate",
                                //        "AWBNumber",
                                //        "AWBPrefix",
                                //        "RefNo"
                                //    };
                                //SqlDbType[] dataType = new SqlDbType[]
                                //    {
                                //        SqlDbType.VarChar,
                                //        SqlDbType.VarChar,
                                //        SqlDbType.VarChar,
                                //        SqlDbType.DateTime,
                                //        SqlDbType.VarChar,
                                //        SqlDbType.VarChar,
                                //        SqlDbType.Int
                                //    };
                                //object[] value = new object[]
                                //    {

                                //        //fltroute[lstIndex].fltdept,
                                //        //fltroute[lstIndex].fltarrival,
                                //       FltOrg,
                                //       FltDest,
                                //       fltroute[lstIndex].fltnum,
                                //        DateTime.Parse(fltDate),
                                //        string.Empty,
                                //        string.Empty,
                                //        REFNo
                                //    };

                                SqlParameter[] sqlParams = new SqlParameter[]
                                {
                                    new SqlParameter("@FltOrigin", SqlDbType.VarChar) { Value = FltOrg },
                                    new SqlParameter("@FltDestination", SqlDbType.VarChar) { Value = FltDest },
                                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = fltroute[lstIndex].fltnum },
                                    new SqlParameter("@flightDate", SqlDbType.DateTime) { Value = DateTime.Parse(fltDate) },
                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = string.Empty },
                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = string.Empty },
                                    new SqlParameter("@RefNo", SqlDbType.Int) { Value = REFNo }
                                };

                                //start changes by ajay jira --CM-221                   
                                if (await _genericFunction.FlightDisabled(fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum, DateTime.Parse(fltDate)))
                                {
                                    ErrorMsg = "Flight is Disabled!";
                                    //return false;
                                    return (false, ErrorMsg);
                                }
                                //end changes by ajay jira --CM-221

                                //DataSet dsdata = dtb.SelectRecords("GetScheduleid", parms, value, dataType);
                                DataSet? dsdata = await _readWriteDao.SelectRecords("GetScheduleid", sqlParams);

                                if (dsdata != null && dsdata.Tables.Count > 0)
                                {
                                    for (int i = 0; i < dsdata.Tables.Count; i++)
                                    {
                                        if (dsdata.Tables[i].Columns.Contains("ScheduleID") && dsdata.Tables[i].Rows[0]["ScheduleID"].ToString() != "0")
                                        {
                                            isFlightValid = true;
                                            break;
                                        }
                                        if (dsdata.Tables[i].Columns.Contains("ScheduleID") && dsdata.Tables[i].Rows[0]["ScheduleID"].ToString() == "0")
                                        {
                                            isFlightValid = true;
                                            break;
                                        }
                                    }
                                }
                                if (isFlightValid)
                                {
                                    break;
                                }
                                #endregion Check Valid Flights


                            }
                            if (!isFlightValid)
                            {
                                //return isFlightValid;
                                return (isFlightValid, ErrorMsg);
                            }
                        }
                        #endregion

                        //flightnum = fltroute[fltroute.Length - 1].carriercode + fltroute[fltroute.Length - 1].fltnum;

                        //strFlightOrigin = fltroute[0].fltdept;
                        //strFlightDestination = fltroute[fltroute.Length - 1].fltarrival;

                        flightnum = fltroute[0].fltnum;

                        strFlightOrigin = fltroute[0].fltdept;
                        strFlightDestination = fltroute[0].fltarrival;
                        if (fltroute[0].date != "")
                        {
                            //flightdate = fltroute[0].date + "/" + DateTime.Now.ToString("MM/yyyy");
                            //fltDate = fltroute[0].date.ToString().Substring(8, 2) + "/" + DateTime.Now.ToString("MM/yyyy");
                            //fltDate = DateTime.Now.ToString("yyyy/MM") + "-" + fltroute[0].date.ToString().Substring(8, 2);
                            //fltDate = System.DateTime.Now.Year.ToString() + "/" + fltroute[0].date.ToString().Substring(5, 2)
                            //  + "/" + fltroute[0].date.ToString().Substring(8, 2);
                            fltDate = fltroute[0].date.ToString().Substring(0, 4) + "/" + fltroute[0].date.ToString().Substring(5, 2)
                              + "/" + fltroute[0].date.ToString().Substring(8, 2);
                            //flightdate = fltroute[0].date.ToString().Substring(8, 2) + "-" + DateTime.Now.ToString("MM/yyyy");
                            //flightdate = fltroute[0].date.ToString().Substring(8, 2) + "/" + fltroute[0].date.ToString().Substring(5, 2) + "/" +
                            //    System.DateTime.Now.Year.ToString();
                            flightdate = fltroute[0].date.ToString().Substring(8, 2) + "/" + fltroute[0].date.ToString().Substring(5, 2) + "/" +
                               fltroute[0].date.ToString().Substring(0, 4);


                        }
                        else
                            //flightdate = DateTime.Now.ToString("dd/MM/yyyy");
                            flightdate = DateTime.Now.ToString("yyyy/MM/dd");

                    }
                    else
                    {

                        if (fwbdata.fltnum.Length > 0 && !(fwbdata.fltnum.Contains(',')))
                        {
                            flightnum = fwbdata.fltnum;
                            //flightdate = fwbdata.fltday.PadLeft(2, '0') + "/" + DateTime.Now.ToString("MM/yyyy");
                            //flightdate = DateTime.Now.ToString("yyyy/MM") + "/" + fwbdata.fltday.PadLeft(2, '0');
                            flightdate = fltroute[0].date.ToString().Substring(8, 2) + "/" + fltroute[0].date.ToString().Substring(5, 2) + "/" +
                             fltroute[0].date.ToString().Substring(0, 4);

                        }
                    }
                    if (fwbrates[0].goodsnature.Length > 1)
                        commcode = fwbrates[0].goodsnature;

                    if (commcode != "")
                    {
                        if (fwbrates[0].goodsnature1.Length > 1)
                            commcode += "," + fwbrates[0].goodsnature1;
                    }
                    else
                        commcode = fwbrates[0].goodsnature1;



                    DataSet? dsawb = new DataSet();

                    dsawb = await _fFRMessageProcessor.CheckValidateXFFRMessage(AWBPrefix, awbnum, fwbdata.origin, fwbdata.dest, "xFWB",
                        fltroute[0].fltdept, fltroute[fltroute.Length - 1].fltarrival, fltroute[0].carriercode + fltroute[fltroute.Length - 1].fltnum,
                        fltDate, fltroute[0].carriercode + fltroute[0].fltnum, REFNo, fwbdata.agentParticipentIdentifier);

                    if (dsawb != null && dsawb.Tables.Count > 1 && dsawb.Tables[1].Rows.Count > 0)
                    {
                        if (dsawb.Tables.Count > 0 && dsawb.Tables[1].Rows.Count > 0 && dsawb.Tables[1].Columns.Count == 2 && dsawb.Tables[1].Columns.Contains("AWBSttus"))
                        {
                            if (dsawb.Tables[1].Rows[0]["MessageName"].ToString() == "xFWB" && (dsawb.Tables[1].Rows[0]["AWBSttus"].ToString().ToUpper() == "ACCEPTED" || dsawb.Tables[1].Rows[1]["AWBSttus"].ToString().ToUpper() == "ACCEPTED"))
                            {
                                //string[] PFWB = new string[] { "AirlinePrefix", "AWBNum", "ShipperName", "ShipperAddr", "ShipperPlace",
                                //"ShipperState", "ShipperCountryCode", "ShipperContactNo", "ShipperPincode", "ConsName",
                                //"ConsAddr", "ConsPlace", "ConsState", "ConsCountryCode", "ConsContactNo",
                                //"ConsingneePinCode", "CustAccNo", "IATACargoAgentCode", "CustName",
                                //"REFNo", "UpdatedBy", "ComodityCode", "ComodityDesc", "ChargedWeight","ShippFaxNo" };

                                //SqlDbType[] ParamSqlType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar,
                                //SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                                //SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                                //SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                                //SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                                //SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar,
                                //SqlDbType.VarChar, SqlDbType.Decimal,SqlDbType.VarChar };

                                //object[] paramValue = { fwbdata.airlineprefix, fwbdata.awbnum, fwbdata.shippername,
                                //fwbdata.shipperadd.Trim(','), fwbdata.shippercity,
                                //fwbdata.shipperstate, fwbdata.shippercountrycode,
                                //fwbdata.shippercontactnum, fwbdata.shipperpostcode,
                                //fwbdata.consname, fwbdata.consadd.Trim(','),
                                //fwbdata.conscity, fwbdata.consstate, fwbdata.conscountrycode,
                                //fwbdata.conscontactnum, fwbdata.conspostcode, fwbdata.agentaccnum,
                                //fwbdata.agentIATAnumber, fwbdata.agentname, REFNo, "xFWB", "",
                                //commcode,
                                //0,fwbdata.shipperfaxno };

                                SqlParameter[] sqlParams = new SqlParameter[]
                               {
                                    new SqlParameter("@AirlinePrefix", SqlDbType.VarChar) { Value = fwbdata.airlineprefix },
                                    new SqlParameter("@AWBNum", SqlDbType.VarChar) { Value = fwbdata.awbnum },
                                    new SqlParameter("@ShipperName", SqlDbType.VarChar) { Value = fwbdata.shippername },
                                    new SqlParameter("@ShipperAddr", SqlDbType.VarChar) { Value = fwbdata.shipperadd.Trim(',') },
                                    new SqlParameter("@ShipperPlace", SqlDbType.VarChar) { Value = fwbdata.shippercity },
                                    new SqlParameter("@ShipperState", SqlDbType.VarChar) { Value = fwbdata.shipperstate },
                                    new SqlParameter("@ShipperCountryCode", SqlDbType.VarChar) { Value = fwbdata.shippercountrycode },
                                    new SqlParameter("@ShipperContactNo", SqlDbType.VarChar) { Value = fwbdata.shippercontactnum },
                                    new SqlParameter("@ShipperPincode", SqlDbType.VarChar) { Value = fwbdata.shipperpostcode },
                                    new SqlParameter("@ConsName", SqlDbType.VarChar) { Value = fwbdata.consname },
                                    new SqlParameter("@ConsAddr", SqlDbType.VarChar) { Value = fwbdata.consadd.Trim(',') },
                                    new SqlParameter("@ConsPlace", SqlDbType.VarChar) { Value = fwbdata.conscity },
                                    new SqlParameter("@ConsState", SqlDbType.VarChar) { Value = fwbdata.consstate },
                                    new SqlParameter("@ConsCountryCode", SqlDbType.VarChar) { Value = fwbdata.conscountrycode },
                                    new SqlParameter("@ConsContactNo", SqlDbType.VarChar) { Value = fwbdata.conscontactnum },
                                    new SqlParameter("@ConsingneePinCode", SqlDbType.VarChar) { Value = fwbdata.conspostcode },
                                    new SqlParameter("@CustAccNo", SqlDbType.VarChar) { Value = fwbdata.agentaccnum },
                                    new SqlParameter("@IATACargoAgentCode", SqlDbType.VarChar) { Value = fwbdata.agentIATAnumber },
                                    new SqlParameter("@CustName", SqlDbType.VarChar) { Value = fwbdata.agentname },
                                    new SqlParameter("@REFNo", SqlDbType.Int) { Value = REFNo },
                                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "xFWB" },
                                    new SqlParameter("@ComodityCode", SqlDbType.VarChar) { Value = "" },
                                    new SqlParameter("@ComodityDesc", SqlDbType.VarChar) { Value = commcode },
                                    new SqlParameter("@ChargedWeight", SqlDbType.Decimal) { Value = 0 },
                                    new SqlParameter("@ShippFaxNo", SqlDbType.VarChar) { Value = fwbdata.shipperfaxno }
                               };

                                _xFNMMessageProcessor.GenerateXFNMMessage(strMessage, "AWB is Already Accepted, We will only update SHP/CNE info", AWBPrefix, awbnum, strMessageFrom == "" ? strFromID : strMessageFrom, commtype);

                                string strProcedure = "Messaging.uspUpdateShipperConsigneeforXFWB";

                                //flag = dtb.InsertData(strProcedure, PFWB, ParamSqlType, paramValue);
                                flag = await _readWriteDao.ExecuteNonQueryAsync(strProcedure, sqlParams);

                                ErrorMsg = "AWB " + AWBPrefix + "-" + awbnum + " is Already Accepted";

                                if (flag)
                                {
                                    #region ProcessRateFunction
                                    DataSet? dsrateCheck = await _fFRMessageProcessor.CheckAirlineForRateProcessing(AWBPrefix, "xFWB");
                                    if (dsrateCheck != null && dsrateCheck.Tables.Count > 0 && dsrateCheck.Tables[0].Rows.Count > 0)
                                    {
                                        //string[] CRNname = new string[] { "AWBNumber", "AWBPrefix", "UpdatedBy", "UpdatedOn", "ValidateMin", "UpdateBooking", "RouteFrom", "UpdateBilling" };
                                        //SqlDbType[] CRType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Bit, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.Bit };
                                        //object[] CRValues = new object[] { fwbdata.awbnum, fwbdata.airlineprefix, "xFWB", System.DateTime.Now, 1, 1, "B", 0 };

                                        SqlParameter[] sqlParamsRate = [
                                            new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = fwbdata.awbnum },
                                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = fwbdata.airlineprefix },
                                            new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "xFWB" },
                                            new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = System.DateTime.Now },
                                            new SqlParameter("@ValidateMin", SqlDbType.Bit) { Value = 1 },
                                            new SqlParameter("@UpdateBooking", SqlDbType.Bit) { Value = 1 },
                                            new SqlParameter("@RouteFrom", SqlDbType.VarChar) { Value = "B" },
                                            new SqlParameter("@UpdateBilling", SqlDbType.Bit) { Value = 0 }
                                        ];

                                        //if (!dtb.ExecuteProcedure("sp_CalculateAWBRatesReprocess", CRNname, CRType, CRValues))
                                        if (!await _readWriteDao.ExecuteNonQueryAsync("sp_CalculateAWBRatesReprocess", sqlParamsRate))
                                        {
                                            // clsLog.WriteLogAzure("Rates Not Calculated for:" + awbnum + Environment.NewLine);
                                            _logger.LogWarning("Rates Not Calculated for:{0}" , awbnum+Environment.NewLine);
                                        }
                                    }
                                    #endregion

                                }
                                else
                                {
                                    //return flag = false;
                                    return (false, ErrorMsg);
                                }

                                //return flag = false;
                                return (false, ErrorMsg);

                            }
                        }
                        else
                        {
                            strErrorMessage = dsawb.Tables[1].Rows[0]["ErrorMessage"].ToString();

                            if (!strErrorMessage.Contains("AWBNo has mismatch Origin/Destination."))
                            {
                                if (dsawb.Tables.Count > 0 && dsawb.Tables[1].Rows.Count > 0 && dsawb.Tables[1].Columns.Count == 3 && dsawb.Tables[1].Columns.Contains("AWBSttus"))
                                {
                                    if (dsawb.Tables[1].Rows[0]["MessageName"].ToString() == "xFWB" && dsawb.Tables[1].Rows[0]["AWBSttus"].ToString().ToUpper() == "EXECUTED")
                                    {
                                        //        string[] PFWB = new string[] { "AirlinePrefix", "AWBNum", "ShipperName", "ShipperAddr", "ShipperPlace",
                                        //"ShipperState", "ShipperCountryCode", "ShipperContactNo", "ShipperPincode", "ConsName",
                                        //"ConsAddr", "ConsPlace", "ConsState", "ConsCountryCode", "ConsContactNo",
                                        //"ConsingneePinCode", "CustAccNo", "IATACargoAgentCode", "CustName",
                                        //"REFNo", "UpdatedBy", "ComodityCode", "ComodityDesc", "ChargedWeight","ShippFaxNo" };

                                        //        SqlDbType[] ParamSqlType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar,
                                        //SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                                        //SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                                        //SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                                        //SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                                        //SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar,
                                        //SqlDbType.VarChar, SqlDbType.Decimal,SqlDbType.VarChar };

                                        //        object[] paramValue = { fwbdata.airlineprefix, fwbdata.awbnum, fwbdata.shippername,
                                        //fwbdata.shipperadd.Trim(','), fwbdata.shippercity,
                                        //fwbdata.shipperstate, fwbdata.shippercountrycode,
                                        //fwbdata.shippercontactnum, fwbdata.shipperpostcode,
                                        //fwbdata.consname, fwbdata.consadd.Trim(','),
                                        //fwbdata.conscity, fwbdata.consstate, fwbdata.conscountrycode,
                                        //fwbdata.conscontactnum, fwbdata.conspostcode, fwbdata.agentaccnum,
                                        //fwbdata.agentIATAnumber, fwbdata.agentname, REFNo, "xFWB", "",
                                        //commcode,
                                        //0,fwbdata.shipperfaxno };

                                        SqlParameter[] sqlParams = [
                                            new SqlParameter("@AirlinePrefix", SqlDbType.VarChar) { Value = fwbdata.airlineprefix },
                                            new SqlParameter("@AWBNum", SqlDbType.VarChar) { Value = fwbdata.awbnum },
                                            new SqlParameter("@ShipperName", SqlDbType.VarChar) { Value = fwbdata.shippername },
                                            new SqlParameter("@ShipperAddr", SqlDbType.VarChar) { Value = fwbdata.shipperadd.Trim(',') },
                                            new SqlParameter("@ShipperPlace", SqlDbType.VarChar) { Value = fwbdata.shippercity },
                                            new SqlParameter("@ShipperState", SqlDbType.VarChar) { Value = fwbdata.shipperstate },
                                            new SqlParameter("@ShipperCountryCode", SqlDbType.VarChar) { Value = fwbdata.shippercountrycode },
                                            new SqlParameter("@ShipperContactNo", SqlDbType.VarChar) { Value = fwbdata.shippercontactnum },
                                            new SqlParameter("@ShipperPincode", SqlDbType.VarChar) { Value = fwbdata.shipperpostcode },
                                            new SqlParameter("@ConsName", SqlDbType.VarChar) { Value = fwbdata.consname },
                                            new SqlParameter("@ConsAddr", SqlDbType.VarChar) { Value = fwbdata.consadd.Trim(',') },
                                            new SqlParameter("@ConsPlace", SqlDbType.VarChar) { Value = fwbdata.conscity },
                                            new SqlParameter("@ConsState", SqlDbType.VarChar) { Value = fwbdata.consstate },
                                            new SqlParameter("@ConsCountryCode", SqlDbType.VarChar) { Value = fwbdata.conscountrycode },
                                            new SqlParameter("@ConsContactNo", SqlDbType.VarChar) { Value = fwbdata.conscontactnum },
                                            new SqlParameter("@ConsingneePinCode", SqlDbType.VarChar) { Value = fwbdata.conspostcode },
                                            new SqlParameter("@CustAccNo", SqlDbType.VarChar) { Value = fwbdata.agentaccnum },
                                            new SqlParameter("@IATACargoAgentCode", SqlDbType.VarChar) { Value = fwbdata.agentIATAnumber },
                                            new SqlParameter("@CustName", SqlDbType.VarChar) { Value = fwbdata.agentname },
                                            new SqlParameter("@REFNo", SqlDbType.Int){ Value = REFNo },
                                            new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "xFWB" },
                                            new SqlParameter("@ComodityCode", SqlDbType.VarChar) { Value = "" },
                                            new SqlParameter("@ComodityDesc", SqlDbType.VarChar) { Value = commcode },
                                            new SqlParameter("@ChargedWeight", SqlDbType.Decimal) { Value = 0 },
                                            new SqlParameter("@ShippFaxNo", SqlDbType.VarChar) { Value = fwbdata.shipperfaxno }
                                        ];

                                        string strProcedure = "Messaging.uspUpdateShipperConsigneeforXFWB";
                                        //flag = dtb.InsertData(strProcedure, PFWB, ParamSqlType, paramValue);
                                        flag = await _readWriteDao.ExecuteNonQueryAsync(strProcedure, sqlParams);
                                    }
                                }
                                ErrorMsg = strErrorMessage;
                                _xFNMMessageProcessor.GenerateXFNMMessage(strMessage, strErrorMessage, fwbdata.airlineprefix,
                                    fwbdata.awbnum, strMessageFrom == "" ? strFromID : strMessageFrom, commtype);
                                strErrorMessage = string.Empty;
                                //return flag = false;
                                return (false, ErrorMsg);
                            }

                        }
                    }

                    if (dsawb != null && dsawb.Tables.Count > 0 && dsawb.Tables[0].Rows.Count > 0 && dsawb.Tables[0].Columns.Contains("AWBOriginAirportCode"))
                    {
                        AWBOriginAirportCode = dsawb.Tables[0].Rows[0]["AWBOriginAirportCode"].ToString();
                        AWBDestAirportCode = dsawb.Tables[0].Rows[0]["AWBDestAirportCode"].ToString();
                    }

                    _xFNMMessageProcessor.GenerateXFNMMessage(strMessage, "We will book/execute AWB  " + fwbdata.airlineprefix + "-" + fwbdata.awbnum + " Shortly.", fwbdata.airlineprefix, fwbdata.awbnum, strMessageFrom == "" ? strFromID : strMessageFrom, commtype);

                    string strAWbIssueDate = string.Empty;
                    if (fwbdata.carrierdate != "" && fwbdata.carriermonth != "" && fwbdata.carrieryear != "")
                    {

                        int month = DateTime.Parse("1." + (fwbdata.carriermonth.ToString().PadLeft(2, '0')) + " 2008").Month;
                        //strAWbIssueDate = month + "/" + fwbdata.carrierdate.PadLeft(2, '0') + "/" + "20" + fwbdata.carrieryear;
                        strAWbIssueDate = fwbdata.carrierdate.ToString();

                    }
                    else
                    {
                        //strAWbIssueDate = System.DateTime.Now.ToString("MM/dd/yyyy");
                        strAWbIssueDate = System.DateTime.Now.ToString("yyyy/MM/dd");

                    }

                    #region AWB
                    string VolumeAmount = string.Empty, volcode = string.Empty;

                    try
                    {
                        VolumeAmount = (fwbdata.volumeamt.Length > 0 ? fwbdata.volumeamt : fwbrates[0].volamt);
                        volcode = (fwbdata.volumecode.Length > 0 ? fwbdata.volumecode : fwbrates[0].volcode);
                    }
                    catch (Exception)
                    {
                        VolumeAmount = fwbdata.volumeamt;
                        volcode = (fwbdata.volumecode.Length > 0 ? fwbdata.volumecode : fwbrates[0].volcode);
                    }

                    if (VolumeAmount == "")
                    {
                        VolumeAmount = "0";
                    }

                    double VolumeWt = 0;
                    //if (fwbdata.volumecode != "" && Convert.ToDecimal(fwbdata.volumeamt == "" ? "0" : fwbdata.volumeamt) > 0)
                    //{
                    if (volcode != "" && Convert.ToDecimal(VolumeAmount == "" ? "0" : VolumeAmount) > 0)
                    {
                        switch (volcode.ToUpper())
                        {
                            case "MC":
                                VolumeWt = double.Parse(String.Format("{0:0.00}", Convert.ToDecimal(Convert.ToDecimal(VolumeAmount == "" ? "0" : VolumeAmount) * decimal.Parse("166.67"))));
                                break;

                            case "CI":
                                VolumeWt =
                                    double.Parse(String.Format("{0:0.00}", Convert.ToDecimal((Convert.ToDecimal(VolumeAmount == "" ? "0" : VolumeAmount) /
                                                                                  decimal.Parse("366")))));
                                break;
                            case "CF":
                                VolumeWt =
                                    double.Parse(String.Format("{0:0.00}",
                                                                Convert.ToDecimal(Convert.ToDecimal(VolumeAmount == "" ? "0" : VolumeAmount) *
                                                                                  decimal.Parse("4.7194"))));
                                break;
                            case "CC":
                                VolumeWt =
                                   double.Parse(String.Format("{0:0.00}",
                                                               Convert.ToDecimal(((Convert.ToDecimal(VolumeAmount == "" ? "0" : VolumeAmount) /
                                                                                  decimal.Parse("6000"))))));
                                break;
                            default:
                                //VolumeWt = Convert.ToDecimal(Convert.ToDecimal(VolumeAmount == "" ? "0" : VolumeAmount));
                                VolumeWt =
                                   double.Parse(String.Format("{0:0.00}", Convert.ToDecimal(VolumeAmount == "" ? "0" : VolumeAmount)));

                                break;

                        }
                    }

                    double ChargeableWeight = 0;
                    if (VolumeWt > 0)
                    {
                        if (Convert.ToDouble(fwbdata.weight == "" ? "0" : fwbdata.weight) > VolumeWt)
                            ChargeableWeight = Convert.ToDouble(fwbdata.weight == "" ? "0" : fwbdata.weight);
                        else
                            ChargeableWeight = VolumeWt;
                    }
                    else
                    {
                        ChargeableWeight = Convert.ToDouble(fwbdata.weight == "" ? "0" : fwbdata.weight);
                    }


                    if (objAWBBup.Length > 0)
                    {
                        if (objAWBBup[0].SlacCount != "" && objAWBBup[0].SlacCount != null)
                        {
                            Slac = objAWBBup[0].SlacCount;
                        }
                    }
                    Numberofposition = "0";
                    if ((fwbrates[0].noofpositionpo).Equals("PO", StringComparison.OrdinalIgnoreCase))
                    {
                        Numberofposition = fwbrates[0].noofposition != string.Empty ? fwbrates[0].noofposition : "0";
                    }

                    #region Check AWB is present or not
                    bool isAWBPresent = false;
                    DataSet? dsCheck = new DataSet();

                    //dtb = new SQLServer();

                    //string[] parametername = new string[] { "AWBNumber", "AWBPrefix" };
                    //object[] AWBvalues = new object[] { awbnum, AWBPrefix };
                    //SqlDbType[] ptype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };

                    SqlParameter[] sqlParameter = [
                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                    ];

                    //dsCheck = dtb.SelectRecords("sp_getawbdetails", parametername, AWBvalues, ptype);
                    dsCheck = await _readWriteDao.SelectRecords("sp_getawbdetails", sqlParameter);

                    if (dsCheck != null)
                    {
                        if (dsCheck.Tables.Count > 0)
                        {
                            if (dsCheck.Tables[0].Rows.Count > 0)
                            {
                                if (dsCheck.Tables[0].Rows[0]["AWBNumber"].ToString().Equals(awbnum, StringComparison.OrdinalIgnoreCase))
                                {
                                    isAWBPresent = true;
                                }
                            }
                        }
                    }
                    #endregion Check AWB is present or not

                    if ((fwbdata.updatedondate == string.Empty && fwbdata.Recivedontime == string.Empty))
                    {
                        fwbdata.updatedondate = DateTime.Now.ToString("yyyy-MM-dd");
                        fwbdata.Recivedontime = DateTime.Now.ToString("HH:mm:ss");
                    }

                    if ((fwbdata.Recoverytimedate == string.Empty))
                    {
                        fwbdata.Recoverytimedate = "1900-01-01";

                    }
                    if (fwbdata.Recoverytime == string.Empty)
                    {
                        fwbdata.Recoverytime = "00:00:00";
                    }


                    //      string[] paramname = new string[] { "AirlinePrefix", "AWBNum", "Origin", "Dest", "PcsCount", "Weight", "Volume", "ComodityCode"
                    //      , "ComodityDesc", "CarrierCode", "FlightNum", "FlightDate", "FlightOrigin", "FlightDest", "ShipperName", "ShipperAddr",
                    //      "ShipperPlace"
                    //      , "ShipperState", "ShipperCountryCode", "ShipperContactNo", "ConsName", "ConsAddr", "ConsPlace", "ConsState", "ConsCountryCode"
                    //      , "ConsContactNo", "CustAccNo", "IATACargoAgentCode", "CustName", "SystemDate", "MeasureUnit", "Length", "Breadth", "Height"
                    //      , "PartnerStatus", "REFNo", "UpdatedBy", "SpecialHandelingCode", "Paymode", "ShipperPincode", "ConsingneePinCode", "WeightCode"
                    //      , "AWBIssueDate","VolumeWt","VolumeCode","ChargeableWeight", "Slac","ISConsole","Remark",
                    //      "ProductID","handinginfo","SCI","ShipperAccountCode","ConsAccountCode","NoofPosition","ConsignorParty_PrimaryID ","Purposecode","RefNumber",
                    //      "Recivedon","RecoveryTime"};


                    //      object[] paramvalue = new object[] {fwbdata.airlineprefix,fwbdata.awbnum,AWBOriginAirportCode, AWBDestAirportCode,
                    //      fwbdata.pcscnt, fwbdata.weight,
                    //      VolumeAmount, fwbrates[0].commoditynumber, commcode,fwbdata.carriercode,flightnum,flightdate, strFlightOrigin,strFlightDestination,
                    //      fwbdata.shippername.Trim(' '),
                    //                                           fwbdata.shipperadd.Trim(','), fwbdata.shippercity.Trim(','),
                    //      fwbdata.shipperstate, fwbdata.shippercountrycode, fwbdata.shippercontactnum, fwbdata.consname.Trim(' '),
                    //      fwbdata.consadd.Trim(','), fwbdata.conscity, fwbdata.consstate, fwbdata.conscountrycode,
                    //                                           fwbdata.conscontactnum, fwbdata.agentaccnum, fwbdata.agentIATAnumber,
                    //      fwbdata.agentname, DateTime.Now.ToString("yyyy-MM-dd"),"", "", "", "", "",REFNo, "xFWB",
                    //      fwbdata.splhandling,fwbdata.chargecode,fwbdata.shipperpostcode,fwbdata.conspostcode,
                    //      fwbdata.weightcode,strAWbIssueDate,VolumeWt,fwbdata.volumecode,ChargeableWeight,
                    //      Slac,"False",fwbdata.Content,fwbdata.ProductID, fwbdata.handinginfo,fwbdata.SCI,
                    //      fwbdata.shipperaccnum,fwbdata.consaccnum,Numberofposition,fwbdata.ConsignorParty_PrimaryID,fwbdata.fwbPurposecode,REFNo,
                    //          fwbdata.updatedondate + " " +fwbdata.Recivedontime, fwbdata.Recoverytimedate + " " +fwbdata.Recoverytime
                    //};

                    //      SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                    //                                                SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                    //                                                SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                    //          SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                    //          SqlDbType.Int,
                    //                                              SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,
                    //      SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.DateTime,SqlDbType.Decimal,
                    //      SqlDbType.VarChar,SqlDbType.Decimal,SqlDbType.VarChar,SqlDbType.Bit,SqlDbType.VarChar,SqlDbType.VarChar
                    //  ,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,
                    //          SqlDbType.VarChar,SqlDbType.Int,SqlDbType.DateTime2,SqlDbType.DateTime };

                    var parameters = new SqlParameter[]
                    {

                        new SqlParameter("@AirlinePrefix", SqlDbType.VarChar) { Value = fwbdata.airlineprefix },
                        new SqlParameter("@AWBNum", SqlDbType.VarChar) { Value = fwbdata.awbnum },
                        new SqlParameter("@Origin", SqlDbType.VarChar) { Value = AWBOriginAirportCode },
                        new SqlParameter("@Dest", SqlDbType.VarChar) { Value = AWBDestAirportCode },
                        new SqlParameter("@PcsCount", SqlDbType.VarChar) { Value = fwbdata.pcscnt },
                        new SqlParameter("@Weight", SqlDbType.VarChar) { Value = fwbdata.weight },
                        new SqlParameter("@Volume", SqlDbType.VarChar) { Value = VolumeAmount },
                        new SqlParameter("@ComodityCode", SqlDbType.VarChar) { Value = fwbrates[0].commoditynumber },
                        new SqlParameter("@ComodityDesc", SqlDbType.VarChar) { Value = commcode },
                        new SqlParameter("@CarrierCode", SqlDbType.VarChar) { Value = fwbdata.carriercode },
                        new SqlParameter("@FlightNum", SqlDbType.VarChar) { Value = flightnum },
                        new SqlParameter("@FlightDate", SqlDbType.VarChar) { Value = flightdate },
                        new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = strFlightOrigin },
                        new SqlParameter("@FlightDest", SqlDbType.VarChar) { Value = strFlightDestination },
                        new SqlParameter("@ShipperName", SqlDbType.VarChar) { Value = fwbdata.shippername.Trim() },
                        new SqlParameter("@ShipperAddr", SqlDbType.VarChar) { Value = fwbdata.shipperadd.Trim(',') },
                        new SqlParameter("@ShipperPlace", SqlDbType.VarChar) { Value = fwbdata.shippercity.Trim(',') },
                        new SqlParameter("@ShipperState", SqlDbType.VarChar) { Value = fwbdata.shipperstate },
                        new SqlParameter("@ShipperCountryCode", SqlDbType.VarChar) { Value = fwbdata.shippercountrycode },
                        new SqlParameter("@ShipperContactNo", SqlDbType.VarChar) { Value = fwbdata.shippercontactnum },
                        new SqlParameter("@ConsName", SqlDbType.VarChar) { Value = fwbdata.consname.Trim() },
                        new SqlParameter("@ConsAddr", SqlDbType.VarChar) { Value = fwbdata.consadd.Trim(',') },
                        new SqlParameter("@ConsPlace", SqlDbType.VarChar) { Value = fwbdata.conscity },
                        new SqlParameter("@ConsState", SqlDbType.VarChar) { Value = fwbdata.consstate },
                        new SqlParameter("@ConsCountryCode", SqlDbType.VarChar) { Value = fwbdata.conscountrycode },
                        new SqlParameter("@ConsContactNo", SqlDbType.VarChar) { Value = fwbdata.conscontactnum },
                        new SqlParameter("@CustAccNo", SqlDbType.VarChar) { Value = fwbdata.agentaccnum },
                        new SqlParameter("@IATACargoAgentCode", SqlDbType.VarChar) { Value = fwbdata.agentIATAnumber },
                        new SqlParameter("@CustName", SqlDbType.VarChar) { Value = fwbdata.agentname },
                        new SqlParameter("@SystemDate", SqlDbType.DateTime) { Value = DateTime.Now.ToString("yyyy-MM-dd") },
                        new SqlParameter("@MeasureUnit", SqlDbType.VarChar) { Value = "" },
                        new SqlParameter("@Length", SqlDbType.VarChar) { Value = "" },
                        new SqlParameter("@Breadth", SqlDbType.VarChar) { Value = "" },
                        new SqlParameter("@Height", SqlDbType.VarChar) { Value = "" },
                        new SqlParameter("@PartnerStatus", SqlDbType.VarChar) { Value = "" },
                        new SqlParameter("@REFNo", SqlDbType.Int) { Value = REFNo },
                        new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "xFWB" },
                        new SqlParameter("@SpecialHandelingCode", SqlDbType.VarChar) { Value = fwbdata.splhandling },
                        new SqlParameter("@Paymode", SqlDbType.VarChar) { Value = fwbdata.chargecode },
                        new SqlParameter("@ShipperPincode", SqlDbType.VarChar) { Value = fwbdata.shipperpostcode },
                        new SqlParameter("@ConsingneePinCode", SqlDbType.VarChar) { Value = fwbdata.conspostcode },
                        new SqlParameter("@WeightCode", SqlDbType.VarChar) { Value = fwbdata.weightcode },
                        new SqlParameter("@AWBIssueDate", SqlDbType.DateTime) { Value = strAWbIssueDate },
                        new SqlParameter("@VolumeWt", SqlDbType.Decimal) { Value = VolumeWt },
                        new SqlParameter("@VolumeCode", SqlDbType.VarChar) { Value = fwbdata.volumecode },
                        new SqlParameter("@ChargeableWeight", SqlDbType.Decimal) { Value = ChargeableWeight },
                        new SqlParameter("@Slac", SqlDbType.VarChar) { Value = Slac },
                        new SqlParameter("@ISConsole", SqlDbType.Bit) { Value = "False" },
                        new SqlParameter("@Remark", SqlDbType.VarChar) { Value = fwbdata.Content },
                        new SqlParameter("@ProductID", SqlDbType.VarChar) { Value = fwbdata.ProductID },
                        new SqlParameter("@handinginfo", SqlDbType.VarChar) { Value = fwbdata.handinginfo },
                        new SqlParameter("@SCI", SqlDbType.VarChar) { Value = fwbdata.SCI },
                        new SqlParameter("@ShipperAccountCode", SqlDbType.VarChar) { Value = fwbdata.shipperaccnum },
                        new SqlParameter("@ConsAccountCode", SqlDbType.VarChar) { Value = fwbdata.consaccnum },
                        new SqlParameter("@NoofPosition", SqlDbType.VarChar) { Value = Numberofposition },
                        new SqlParameter("@ConsignorParty_PrimaryID", SqlDbType.VarChar) { Value = fwbdata.ConsignorParty_PrimaryID },
                        new SqlParameter("@Purposecode", SqlDbType.VarChar) { Value = fwbdata.fwbPurposecode },
                        new SqlParameter("@RefNumber", SqlDbType.Int) { Value = REFNo },
                        new SqlParameter("@Recivedon", SqlDbType.DateTime2) { Value = fwbdata.updatedondate + " " + fwbdata.Recivedontime },
                        new SqlParameter("@RecoveryTime", SqlDbType.DateTime) { Value = fwbdata.Recoverytimedate + " " + fwbdata.Recoverytime }
                    };




                    //string procedure = "Messaging.uspInsertBookingDataFromFFR";
                    string procedure = "Messaging.uspInsertBookingDataFromXFWB";

                    // flag = dtb.InsertData(procedure, paramname, paramtype, paramvalue);
                    //DataSet dsdata1 = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);
                    DataSet? dsdata1 = await _readWriteDao.SelectRecords(procedure, parameters);

                    #endregion
                    if (dsdata1 != null)
                    {
                        if (dsdata1.Tables.Count > 0)
                        {


                            #region comment code for add entry in audit log
                            //for (int lstIndex1 = 0; lstIndex1 < fltroute.Length; lstIndex1++)
                            //{

                            //    {
                            //        if (fltroute.Length > 0)
                            //        {
                            //            //strFlightNo = fltroute[fltroute.Length - 1].carriercode + fltroute[fltroute.Length - 1].fltnum;
                            //            //strFlightOrigin = fltroute[0].fltdept;
                            //            //strFlightDestination = fltroute[fltroute.Length - 1].fltarrival;
                            //            strFlightNo = fltroute[lstIndex1].carriercode + fltroute[lstIndex1].fltnum;
                            //            strFlightOrigin = fltroute[lstIndex1].fltdept;
                            //            strFlightDestination = fltroute[lstIndex1].fltarrival;
                            //        }

                            //        string date_format = System.DateTime.Now.ToString("yyyy/MM/dd");
                            //        string updatedon = System.DateTime.Now.ToString("yyyy/MM/dd HH:MM:SS");
                            //        try
                            //        {

                            //            //date_format = Convert.ToDateTime(fltroute[0].date.ToString().Substring(8, 2) + "-" + DateTime.Now.ToString("MM/yyyy"))
                            //            //    .ToString("yyyy/MM/dd");
                            //            //date_format = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM") + "/" + fltroute[lstIndex1].date.ToString().Substring(8, 2))
                            //            //    .ToString("yyyy/MM/dd");
                            //            date_format = Convert.ToDateTime(DateTime.Now.ToString("yyyy") + "/"
                            //                + fltroute[lstIndex1].date.ToString().Substring(5, 2) + "/"
                            //                + fltroute[lstIndex1].date.ToString().Substring(8, 2))
                            //               .ToString("yyyy/MM/dd");
                            //            updatedon = fwbdata.updatedondate + " " + fwbdata.updatedontime;



                            //        }
                            //        catch (Exception ex)
                            //        {
                            //            clsLog.WriteLogAzure("Error on FWB  message:" + ex.ToString());
                            //            date_format = System.DateTime.Now.ToString("yyyy/MM/dd");
                            //            updatedon = System.DateTime.Now.ToString("yyyy/MM/dd HH:MM:SS");
                            //        }



                            //        string[] CNname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight",
                            //"FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public" };
                            //        SqlDbType[] CType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit };
                            //        //        object[] CValues = new object[] { fwbdata.airlineprefix, fwbdata.awbnum, fwbdata.origin,
                            //        //fwbdata.dest, fwbdata.pcscnt, fwbdata.weight, strFlightNo,
                            //        //date_format, strFlightOrigin, strFlightDestination, "Booked", "AWB Booked", "AWB Booked Through xFWB",
                            //        //"xFWB", DateTime.UtcNow.ToString("yyyy/MM/dd"), 1 };

                            //        object[] CValues = new object[] { fwbdata.airlineprefix, fwbdata.awbnum, fwbdata.origin,
                            //fwbdata.dest, fwbdata.pcscnt, fwbdata.weight, strFlightNo,
                            //date_format, strFlightOrigin, strFlightDestination, "Booked", "AWB Booked", "AWB Booked Through xFWB",
                            //"xFWB", fwbdata.updatedondate +" " + fwbdata.updatedontime, 1 };
                            //      if (!dtb.ExecuteProcedure("SPAddAWBAuditLog", CNname, CType, CValues, 600))
                            //            clsLog.WriteLog("AWB Audit log  for:" + fwbdata.awbnum + Environment.NewLine + "Error: " + dtb.LastErrorDescription);

                            //    }
                            //} 
                            #endregion



                            #region Save AWB Routing
                            bool isRouteUpdate = false;
                            // GenericFunction genericFunction = new GenericFunction();
                            isRouteUpdate = Convert.ToBoolean(_genericFunction.ReadValueFromDb("UpdateRouteThroughFWB") == string.Empty ? "false" : _genericFunction.ReadValueFromDb("UpdateRouteThroughFWB"));
                            if ((isRouteUpdate && isAWBPresent) || !isAWBPresent)
                            {
                                string status = "C";

                                if (fltroute.Length > 0)
                                {
                                    //string[] parname = new string[] { "AWBNum", "AWBPrefix" };
                                    //object[] parobject = new object[] { awbnum, AWBPrefix };
                                    //SqlDbType[] partype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };

                                    SqlParameter[] sqlParams = [
                                        new SqlParameter("@AWBNum", SqlDbType.VarChar) { Value = awbnum },
                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                                    ];

                                    //if (dtb.ExecuteProcedure("Messaging.uspDeleteAWBRouteXFFR", parname, partype, parobject))
                                    if (await _readWriteDao.ExecuteNonQueryAsync("Messaging.uspDeleteAWBRouteXFFR", sqlParams))
                                    {
                                        string dtFlightDate = DateTime.Now.ToString("yyyy/MM/dd");



                                        for (int lstIndex = 0; lstIndex < fltroute.Length; lstIndex++)
                                        {
                                            if (fltroute[lstIndex].date != "")
                                                //dtFlightDate = DateTime.Parse((DateTime.Now.Month.ToString().PadLeft(2, '0') + "/" + fltroute[lstIndex].date.PadLeft(2, '0') + "/" + System.DateTime.Now.Year.ToString()));
                                                //fltDate =  DateTime.Now.ToString("yyyy/MM") +"/"+ fltroute[0].date.ToString().Substring(8, 2) ;
                                                //
                                                //fltDate = System.DateTime.Now.Year.ToString() + "/" + fltMonth.PadLeft(2, '0') + "/" + fltroute[lstIndex].date.ToString().Substring(8, 2);
                                                //fltDate = DateTime.Now.ToString("yyyy/MM") + "/" + fltroute[0].date.ToString().Substring(8, 2);
                                                // string dtFltdte = dtFlightDate.ToString("MM/dd/yyyy");
                                                // fltDate = System.DateTime.Now.Year.ToString() + "/" + fltroute[lstIndex].date.ToString().Substring(5, 2) + "/" + fltroute[lstIndex].date.ToString().Substring(8, 2);


                                                fltDate = fltroute[lstIndex].date.ToString().Substring(0, 4) + "/" + fltroute[lstIndex].date.ToString().Substring(5, 2) + "/" + fltroute[lstIndex].date.ToString().Substring(8, 2);

                                            DataSet? dsAWBRflt = new DataSet();

                                            if (fltroute.Length > 1)
                                            {
                                                if (lstIndex == 0)
                                                {
                                                    dsAWBRflt = await _fFRMessageProcessor.CheckValidateXFFRMessage(AWBPrefix, awbnum, fwbdata.origin, fwbdata.dest, "xFWB", fltroute[lstIndex].fltdept, fltroute[lstIndex].fltarrival, fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum, fltDate);
                                                    //FltOrg = dsAWBRflt.Tables[0].Rows[0]["AWBOriginAirportCode"].ToString();
                                                    ////FltDest = dsAWBRflt.Tables[0].Rows[0]["AWBDestAirportCode"].ToString();
                                                    //FltDest = fltroute[lstIndex].fltarrival;
                                                    FltOrg = fltroute[lstIndex].fltdept;
                                                    FltDest = fltroute[lstIndex].fltarrival;
                                                }
                                                else
                                                {
                                                    FltOrg = fltroute[lstIndex].fltdept;
                                                    FltDest = fltroute[lstIndex].fltarrival;
                                                }
                                            }
                                            else
                                            {
                                                dsAWBRflt = await _fFRMessageProcessor.CheckValidateXFFRMessage(AWBPrefix, awbnum, fwbdata.origin, fwbdata.dest, "xFWB", fltroute[lstIndex].fltdept, fltroute[fltroute.Length - 1].fltarrival, fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum, fltDate);
                                                FltOrg = dsAWBRflt.Tables[0].Rows[0]["AWBOriginAirportCode"].ToString();
                                                FltDest = dsAWBRflt.Tables[0].Rows[0]["AWBDestAirportCode"].ToString();
                                            }


                                            //code to check weither flight is valid or not and active
                                            ///Addeed by prashantz to resolve JIRA# CEBV4-1080
                                            if (fltroute[lstIndex].fltdept.Trim().ToUpper() != fltroute[lstIndex].fltarrival.Trim().ToUpper())
                                            {
                                                int schedid = 0;
                                                if (fltroute[lstIndex].fltnum.Trim() != string.Empty)
                                                {

                                                    //    string[] parms = new string[]
                                                    //    {
                                                    //"FltOrigin",
                                                    //"FltDestination",
                                                    //"FlightNo",
                                                    //"flightDate",
                                                    //"AWBNumber",
                                                    //"AWBPrefix",
                                                    //"RefNo"
                                                    //    };
                                                    //    SqlDbType[] dataType = new SqlDbType[]
                                                    //    {
                                                    //SqlDbType.VarChar,
                                                    //SqlDbType.VarChar,
                                                    //SqlDbType.VarChar,
                                                    //SqlDbType.DateTime,
                                                    //SqlDbType.VarChar,
                                                    //SqlDbType.VarChar,
                                                    //SqlDbType.Int
                                                    //    };
                                                    //    object[] value = new object[]
                                                    //    {
                                                    //FltOrg,
                                                    //FltDest,
                                                    //fltroute[lstIndex].fltnum,
                                                    //fltDate,
                                                    //awbnum,
                                                    //AWBPrefix,
                                                    //REFNo
                                                    //    };

                                                    SqlParameter[] sqlParameters = new SqlParameter[]
                                                    {
                                                        new SqlParameter("@FltOrigin", SqlDbType.VarChar) { Value = FltOrg },
                                                        new SqlParameter("@FltDestination", SqlDbType.VarChar) { Value = FltDest },
                                                        new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = fltroute[lstIndex].fltnum },
                                                        new SqlParameter("@flightDate", SqlDbType.DateTime) { Value = fltDate },
                                                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                                        new SqlParameter("@RefNo", SqlDbType.Int) { Value = REFNo }
                                                    };

                                                    //DataSet dsdata = dtb.SelectRecords("GetScheduleid", parms, value, dataType);
                                                    DataSet? dsdata = await _readWriteDao.SelectRecords("GetScheduleid", sqlParameters);

                                                    if (dsdata != null && dsdata.Tables.Count > 0 && dsdata.Tables[0].Rows[0][0].ToString() == "0")
                                                    {
                                                        val = true;
                                                        //continue;
                                                        schedid = 0;

                                                    }
                                                    else if (dsdata != null && dsdata.Tables.Count > 0 && dsdata.Tables[0].Rows.Count > 0)
                                                    {
                                                        val = true;
                                                        schedid = Convert.ToInt32(dsdata.Tables[0].Rows[0]["ScheduleID"]);
                                                    }
                                                }

                                                //    string[] paramNames = new string[]
                                                //    {
                                                //"AWBNumber",
                                                //"FltOrigin",
                                                //"FltDestination",
                                                //"FltNumber",
                                                //"FltDate",
                                                //"Status",
                                                //"UpdatedBy",
                                                //"UpdatedOn",
                                                //"IsFFR",
                                                //"REFNo",
                                                //"date",
                                                //"AWBPrefix",
                                                //"carrierCode",
                                                // "schedid",
                                                // "voluemcode",
                                                // "volume",
                                                // "NoOfPosition",
                                                // "inputdeptDatetime",
                                                // "inputarrivaldatetime"

                                                //    };
                                                //    SqlDbType[] dataTypes = new SqlDbType[]
                                                //    {
                                                //SqlDbType.VarChar,
                                                //SqlDbType.VarChar,
                                                //SqlDbType.VarChar,
                                                //SqlDbType.VarChar,
                                                //SqlDbType.DateTime,
                                                //SqlDbType.VarChar,
                                                //SqlDbType.VarChar,
                                                //SqlDbType.DateTime,
                                                //SqlDbType.Bit,
                                                //SqlDbType.Int,
                                                //SqlDbType.DateTime,
                                                //SqlDbType.VarChar,
                                                //SqlDbType.VarChar,
                                                //SqlDbType.Int,
                                                //SqlDbType.VarChar,
                                                //SqlDbType.Decimal,
                                                //SqlDbType.VarChar,
                                                //SqlDbType.DateTime,
                                                //SqlDbType.DateTime


                                                //    };

                                                //    object[] values = new object[]
                                                //    {
                                                //awbnum,
                                                ////fltroute[lstIndex].fltdept,
                                                ////fltroute[lstIndex].fltarrival,
                                                //FltOrg,
                                                //FltDest,
                                                //fltroute[lstIndex].fltnum,
                                                //fltDate,
                                                //status,
                                                //"xFWB",
                                                // DateTime.Now,
                                                //1,
                                                //0,
                                                //dtFlightDate,
                                                //AWBPrefix,
                                                //fltroute[lstIndex].carriercode,
                                                //schedid,
                                                //volcode,
                                                //VolumeAmount==""?"0":VolumeAmount,
                                                // Numberofposition,
                                                // fltroute[lstIndex].inputdeptDatetime,

                                                // fltroute[lstIndex].inputarrivaldatetime
                                                //};

                                                SqlParameter[] sqlParamsInsert = new SqlParameter[]
                                                {
                                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                                    new SqlParameter("@FltOrigin", SqlDbType.VarChar) { Value = FltOrg },
                                                    new SqlParameter("@FltDestination", SqlDbType.VarChar) { Value = FltDest },
                                                    new SqlParameter("@FltNumber", SqlDbType.VarChar) { Value = fltroute[lstIndex].fltnum },
                                                    new SqlParameter("@FltDate", SqlDbType.DateTime) { Value = fltDate },
                                                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = status },
                                                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "xFWB" },
                                                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = DateTime.Now },
                                                    new SqlParameter("@IsFFR", SqlDbType.Bit) { Value = 1 },
                                                    new SqlParameter("@REFNo", SqlDbType.Int) { Value = 0 },
                                                    new SqlParameter("@date", SqlDbType.DateTime) { Value = dtFlightDate },
                                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                                    new SqlParameter("@carrierCode", SqlDbType.VarChar) { Value = fltroute[lstIndex].carriercode },
                                                    new SqlParameter("@schedid", SqlDbType.Int) { Value = schedid },
                                                    new SqlParameter("@voluemcode", SqlDbType.VarChar) { Value = volcode },
                                                    new SqlParameter("@volume", SqlDbType.Decimal) { Value = VolumeAmount == "" ? "0" : VolumeAmount },
                                                    new SqlParameter("@NoOfPosition", SqlDbType.VarChar) { Value = Numberofposition },
                                                    new SqlParameter("@inputdeptDatetime", SqlDbType.DateTime) { Value = fltroute[lstIndex].inputdeptDatetime },
                                                    new SqlParameter("@inputarrivaldatetime", SqlDbType.DateTime) { Value = fltroute[lstIndex].inputarrivaldatetime }
                                                };


                                                string dtFlightDate_format = Convert.ToDateTime(dtFlightDate).ToString("yyyy/MM/dd");

                                                //if (!dtb.UpdateData("Messaging.uspSaveXFFRAWBRoute", paramNames, dataTypes, values))
                                                if (!await _readWriteDao.ExecuteNonQueryAsync("Messaging.uspSaveXFFRAWBRoute", sqlParamsInsert))
                                                {
                                                    // clsLog.WriteLogAzure("Error in Save AWB Route FWB");
                                                    _logger.LogWarning("Error in Save AWB Route FWB");
                                                }

                                                //    string[] CANname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight",
                                                //"FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description",
                                                //"UpdatedBy", "UpdatedOn", "Public", "Station" };
                                                //    SqlDbType[] CAType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                                                //SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime,
                                                //SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                                                //SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar };
                                                //    //object[] CAValues = new object[] { fwbdata.airlineprefix, fwbdata.awbnum, fwbdata.origin,
                                                //    //fwbdata.dest, fwbdata.pcscnt, fwbdata.weight, fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum,
                                                //    //dtFlightDate_format, fltroute[lstIndex].fltdept, fltroute[lstIndex].fltarrival,
                                                //    //"Booked", "AWB Booked", "AWB Flight Information", "xFWB", DateTime.UtcNow.ToString("yyyy-MM-dd"), 1 };
                                                //    object[] CAValues = new object[] { fwbdata.airlineprefix, fwbdata.awbnum, fwbdata.origin,
                                                //fwbdata.dest, fwbdata.pcscnt, fwbdata.weight, fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum,
                                                //dtFlightDate_format, fltroute[lstIndex].fltdept, fltroute[lstIndex].fltarrival,
                                                //"Booked", "AWB Booked", "AWB Booked Through xFWB", "xFWB",  fwbdata.updatedondate + " " + fwbdata.updatedontime, 1,fwbdata.carrierplace };

                                                SqlParameter[] sqlParamsAudit = new SqlParameter[]
                                                {
                                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = fwbdata.airlineprefix },
                                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = fwbdata.awbnum },
                                                    new SqlParameter("@Origin", SqlDbType.VarChar) { Value = fwbdata.origin },
                                                    new SqlParameter("@Destination", SqlDbType.VarChar) { Value = fwbdata.dest },
                                                    new SqlParameter("@Pieces", SqlDbType.VarChar) { Value = fwbdata.pcscnt },
                                                    new SqlParameter("@Weight", SqlDbType.VarChar) { Value = fwbdata.weight },
                                                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum },
                                                    new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = dtFlightDate_format },
                                                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = fltroute[lstIndex].fltdept },
                                                    new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = fltroute[lstIndex].fltarrival },
                                                    new SqlParameter("@Action", SqlDbType.VarChar) { Value = "Booked" },
                                                    new SqlParameter("@Message", SqlDbType.VarChar) { Value = "AWB Booked" },
                                                    new SqlParameter("@Description", SqlDbType.VarChar) { Value = "AWB Booked Through xFWB" },
                                                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "xFWB" },
                                                    new SqlParameter("@UpdatedOn", SqlDbType.VarChar) { Value = fwbdata.updatedondate + " " + fwbdata.updatedontime },
                                                    new SqlParameter("@Public", SqlDbType.Bit) { Value = 1 },
                                                    new SqlParameter("@Station", SqlDbType.VarChar) { Value = fwbdata.carrierplace }
                                                };

                                                //if (!dtb.ExecuteProcedure("SPAddAWBAuditLog", CANname, CAType, CAValues))
                                                if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", sqlParamsAudit))
                                                {
                                                    // clsLog.WriteLog("AWB Audit log  for:" + fwbdata.awbnum + Environment.NewLine);
                                                    _logger.LogWarning("AWB Audit log  for: {0}" , fwbdata.awbnum + Environment.NewLine);
                                                }
                                            }
                                        }
                                        if (val)
                                        {

                                            //string[] QueryNames = { "AWBPrefix", "AWBNumber" };
                                            //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                                            //object[] QueryValues = { AWBPrefix, awbnum };

                                            SqlParameter[] sqlParamsDelete = new SqlParameter[]
                                            {
                                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum }
                                            };

                                            //if (!dtb.UpdateData("spDeleteAWBDetailsNoRoute", QueryNames, QueryTypes, QueryValues))
                                            if (!await _readWriteDao.ExecuteNonQueryAsync("spDeleteAWBDetailsNoRoute", sqlParamsDelete))
                                            {
                                                // clsLog.WriteLogAzure("Error in Deleting AWB Details");
                                                _logger.LogWarning("Error in Deleting AWB Details");
                                            }
                                        }

                                    }
                                }

                            }

                            #endregion Save AWB Routing
                            if (val)
                            {
                                #region Rate Decription
                                string freight = "0", paymode = "PX", valcharge = "0", tax = "0", OCDA = "0", OCDC = "0", total = "0", currency = string.Empty, insuranceamount = "0";
                                decimal DeclareCarriageValue = 0, DeclareCustomValue = 0;

                                paymode = fwbdata.chargecode == "" ? fwbdata.chargedec : fwbdata.chargecode;

                                if (fwbdata.declaredvalue != "")
                                {
                                    DeclareCarriageValue = decimal.Parse(fwbdata.declaredvalue == "NVD" ? "0" : fwbdata.declaredvalue);
                                }
                                if (fwbdata.declaredcustomvalue != "")
                                {
                                    DeclareCustomValue = decimal.Parse(fwbdata.declaredcustomvalue == "NCV" ? "0" : fwbdata.declaredcustomvalue);
                                }
                                if (fwbdata.insuranceamount != "")
                                {
                                    insuranceamount = fwbdata.insuranceamount.Trim() == "" ? "0" :
                                         (fwbdata.insuranceamount.Trim() == "X" || fwbdata.insuranceamount.Trim() == "XX" || fwbdata.insuranceamount.Trim() == "XXX") ? "0" : fwbdata.insuranceamount;
                                }
                                if (fwbdata.PPweightCharge.Length > 0 || fwbdata.PPValuationCharge.Length > 0 || fwbdata.PPTaxesCharge.Length > 0 ||
                                    fwbdata.PPOCDA.Length > 0 || fwbdata.PPOCDC.Length > 0 || fwbdata.PPTotalCharges.Length > 0)
                                {
                                    freight = fwbdata.PPweightCharge.Length > 0 ? fwbdata.PPweightCharge : "0";
                                    valcharge = fwbdata.PPValuationCharge.Length > 0 ? fwbdata.PPValuationCharge : "0";
                                    tax = fwbdata.PPTaxesCharge.Length > 0 ? fwbdata.PPTaxesCharge : "0";
                                    OCDC = fwbdata.PPOCDC.Length > 0 ? fwbdata.PPOCDC : "0";
                                    OCDA = fwbdata.PPOCDA.Length > 0 ? fwbdata.PPOCDA : "0";
                                    total = fwbdata.PPTotalCharges.Length > 0 ? fwbdata.PPTotalCharges : "0";
                                }

                                if (fwbdata.CCweightCharge.Length > 0 || fwbdata.CCValuationCharge.Length > 0 || fwbdata.CCTaxesCharge.Length > 0 ||
                                    fwbdata.CCOCDA.Length > 0 || fwbdata.CCOCDC.Length > 0 || fwbdata.CCTotalCharges.Length > 0)
                                {
                                    freight = fwbdata.CCweightCharge.Length > 0 ? fwbdata.CCweightCharge : "0";
                                    valcharge = fwbdata.CCValuationCharge.Length > 0 ? fwbdata.CCValuationCharge : "0";
                                    tax = fwbdata.CCTaxesCharge.Length > 0 ? fwbdata.CCTaxesCharge : "0";
                                    OCDC = fwbdata.CCOCDC.Length > 0 ? fwbdata.CCOCDC : "0";
                                    OCDA = fwbdata.CCOCDA.Length > 0 ? fwbdata.CCOCDA : "0";
                                    total = fwbdata.CCTotalCharges.Length > 0 ? fwbdata.CCTotalCharges : "0";
                                }

                                for (int i = 0; i < fwbrates.Length; i++)
                                {
                                    fwbrates[i].chargeamt = fwbrates[i].chargeamt.Length > 0 ? fwbrates[i].chargeamt : "0";
                                    fwbrates[i].awbweight = fwbrates[i].awbweight.Length > 0 ? fwbrates[i].awbweight : "0";
                                    fwbrates[i].weight = fwbrates[i].weight.Length > 0 ? fwbrates[i].weight : "0";
                                    fwbrates[i].chargerate = fwbrates[i].chargerate.Length > 0 ? fwbrates[i].chargerate : freight;
                                    //fwbrates[i].rateclasscode = fwbrates[i].rateclasscode;
                                    currency = fwbrates[i].currencyexchange;


                                    if (fwbrates[i].awbweight.Length > 1)
                                        Priority = "RTW";
                                    else if (objDimension.Length > 0)
                                        Priority = "DIMS";
                                    else if (fwbrates[i].volamt.Length > 0)
                                        Priority = "Volume";
                                    else if (float.Parse(fwbdata.weight) > 1)
                                        Priority = "GrossWt";

                                    switch (Priority)
                                    {
                                        case "RTW":
                                            ChargeableWeight = Convert.ToDouble(fwbrates[i].awbweight);
                                            break;
                                        case "DIMS":
                                            ChargeableWeight = 0;
                                            break;
                                        case "Volume":
                                            ChargeableWeight = Convert.ToDouble(ChargeableWeight);
                                            break;
                                        case "GrossWt":
                                            ChargeableWeight = Convert.ToDouble(fwbdata.weight);
                                            break;

                                    }

                                    //string[] param = new string[]
                                    //{
                                    //    "AWBNumber",
                                    //    "CommCode",
                                    //    "PayMode",
                                    //    "Pcs",
                                    //    "Wt",
                                    //    "FrIATA",
                                    //    "FrMKT",
                                    //    "ValCharge",
                                    //    "OcDueCar",
                                    //    "OcDueAgent",
                                    //    "SpotRate",
                                    //    "DynRate",
                                    //    "ServiceTax",
                                    //    "Total",
                                    //    "RatePerKg",
                                    //    "Currency",
                                    //    "AWBPrefix",
                                    //    "ChargeableWeight",
                                    //    "DeclareCarriageValue",
                                    //    "DeclareCustomValue",
                                    //    "RateClass",
                                    //    "insuranceamount"
                                    //};
                                    //SqlDbType[] dbtypes = new SqlDbType[]
                                    //{
                                    //    SqlDbType.VarChar,
                                    //    SqlDbType.VarChar,
                                    //    SqlDbType.VarChar,
                                    //    SqlDbType.Int,
                                    //    SqlDbType.Float,
                                    //    SqlDbType.Float,
                                    //    SqlDbType.Float,
                                    //    SqlDbType.Float,
                                    //    SqlDbType.Float,
                                    //    SqlDbType.Float,
                                    //    SqlDbType.Float,
                                    //    SqlDbType.Float,
                                    //    SqlDbType.Float,
                                    //    SqlDbType.Float,
                                    //    SqlDbType.Decimal,
                                    //    SqlDbType.VarChar,
                                    //    SqlDbType.VarChar,
                                    //    SqlDbType.Float,
                                    //    SqlDbType.Float,
                                    //    SqlDbType.Float,
                                    //    SqlDbType.VarChar,
                                    //    SqlDbType.Float,
                                    //};
                                    //object[] values = new object[]
                                    //{
                                    //    awbnum,
                                    //    commcode,
                                    //    paymode,
                                    //    Convert.ToInt16(fwbrates[i].numofpcs),
                                    //    float.Parse(fwbrates[i].weight),
                                    //    float.Parse(freight),
                                    //    float.Parse(fwbrates[i].chargeamt),
                                    //    float.Parse(valcharge),
                                    //    float.Parse(OCDC),
                                    //    float.Parse(OCDA),
                                    //    0,
                                    //    0,
                                    //    float.Parse(tax),
                                    //    float.Parse(total),
                                    //    Convert.ToDecimal(fwbrates[i].chargerate),
                                    //    currency,
                                    //    AWBPrefix,
                                    //    //float.Parse(fwbrates[i].awbweight),
                                    //    ChargeableWeight,
                                    //    DeclareCarriageValue,
                                    //    DeclareCustomValue,
                                    //    fwbrates[i].rateclasscode,
                                    //    float.Parse(insuranceamount)
                                    //};

                                    SqlParameter[] sqlParamsFWBRates = [
                                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                        new SqlParameter("@CommCode", SqlDbType.VarChar) { Value = commcode },
                                        new SqlParameter("@PayMode", SqlDbType.VarChar) { Value = paymode },
                                        new SqlParameter("@Pcs", SqlDbType.Int) { Value = Convert.ToInt16(fwbrates[i].numofpcs) },
                                        new SqlParameter("@Wt", SqlDbType.Float) { Value = float.Parse(fwbrates[i].weight) },
                                        new SqlParameter("@FrIATA", SqlDbType.Float) { Value = float.Parse(freight) },
                                        new SqlParameter("@FrMKT", SqlDbType.Float) { Value = float.Parse(fwbrates[i].chargeamt) },
                                        new SqlParameter("@ValCharge", SqlDbType.Float) { Value = float.Parse(valcharge) },
                                        new SqlParameter("@OcDueCar", SqlDbType.Float) { Value = float.Parse(OCDC) },
                                        new SqlParameter("@OcDueAgent", SqlDbType.Float) { Value = float.Parse(OCDA) },
                                        new SqlParameter("@SpotRate", SqlDbType.Float) { Value = 0 },
                                        new SqlParameter("@DynRate", SqlDbType.Float) { Value = 0 },
                                        new SqlParameter("@ServiceTax", SqlDbType.Float) { Value = float.Parse(tax) },
                                        new SqlParameter("@Total", SqlDbType.Float) { Value = float.Parse(total) },
                                        new SqlParameter("@RatePerKg", SqlDbType.Decimal) { Value = Convert.ToDecimal(fwbrates[i].chargerate) },
                                        new SqlParameter("@Currency", SqlDbType.VarChar) { Value = currency },
                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                        new SqlParameter("@ChargeableWeight", SqlDbType.Float) { Value = ChargeableWeight },
                                        new SqlParameter("@DeclareCarriageValue", SqlDbType.Float) { Value = DeclareCarriageValue },
                                        new SqlParameter("@DeclareCustomValue", SqlDbType.Float) { Value = DeclareCustomValue },
                                        new SqlParameter("@RateClass", SqlDbType.VarChar) { Value = fwbrates[i].rateclasscode },
                                        new SqlParameter("@insuranceamount", SqlDbType.Float) { Value = float.Parse(insuranceamount) },
                                       ];

                                    //if (!dtb.UpdateData("SP_SaveAWBRatesviaMsg", param, dbtypes, values))
                                    if (!await _readWriteDao.ExecuteNonQueryAsync("SP_SaveAWBRatesviaMsg", sqlParamsFWBRates))
                                        // clsLog.WriteLogAzure("Error Saving FWB rates for:" + awbnum);
                                        _logger.LogWarning("Error Saving FWB rates for: {0}" , awbnum);

                                }

                                #endregion

                                #region Other Charges
                                //check for other charge exists in systme or not


                                for (int i = 0; i < OtherCharges.Length; i++)
                                {
                                    double chargeamount = OtherCharges[i].chargeamt.Length > 0 ? Convert.ToDouble(OtherCharges[i].chargeamt) : 0;

                                    //    string[] param = { "AWBNumber", "ChargeHeadCode", "ChargeType", "DiscountPercent",
                                    //       "CommPercent", "TaxPercent", "Discount", "Comission", "Tax","Charge","CommCode","AWBPrefix"};
                                    //    SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Float,
                                    //            SqlDbType.Float, SqlDbType.Float, SqlDbType.Float, SqlDbType.Float, SqlDbType.Float,SqlDbType.Float,SqlDbType.VarChar,SqlDbType.VarChar};

                                    //    object[] values = { awbnum, OtherCharges[i].otherchargecode, "D" + OtherCharges[i].entitlementcode, 0, 0, 0, 0, 0, 0,
                                    //chargeamount, commcode, AWBPrefix };

                                    SqlParameter[] sqlParamsOtherCharges = new SqlParameter[]
                                    {
                                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                        new SqlParameter("@ChargeHeadCode", SqlDbType.VarChar) { Value = OtherCharges[i].otherchargecode },
                                        new SqlParameter("@ChargeType", SqlDbType.VarChar) { Value = "D" + OtherCharges[i].entitlementcode },
                                        new SqlParameter("@DiscountPercent", SqlDbType.Float) { Value = 0 },
                                        new SqlParameter("@CommPercent", SqlDbType.Float) { Value = 0 },
                                        new SqlParameter("@TaxPercent", SqlDbType.Float) { Value = 0 },
                                        new SqlParameter("@Discount", SqlDbType.Float) { Value = 0 },
                                        new SqlParameter("@Comission", SqlDbType.Float) { Value = 0 },
                                        new SqlParameter("@Tax", SqlDbType.Float) { Value = 0 },
                                        new SqlParameter("@Charge", SqlDbType.Float) { Value = chargeamount },
                                        new SqlParameter("@CommCode", SqlDbType.VarChar) { Value = commcode },
                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                                    };

                                    //if (!dtb.InsertData("SP_SaveAWBOCRatesDetails", param, dbtypes, values))
                                    if (!await _readWriteDao.ExecuteNonQueryAsync("SP_SaveAWBOCRatesDetails", sqlParamsOtherCharges))
                                        // clsLog.WriteLogAzure("Error Saving FWB OCRates for:" + awbnum);
                                        _logger.LogWarning("Error Saving FWB OCRates for: {0}" , awbnum);

                                }
                                #endregion

                                #region AWB Dimensions

                                if (objDimension.Length > 0)
                                {
                                    decimal totalDimsWt = 0;
                                    if (objDimension.Length > 0)
                                    {
                                        for (int j = 0; j < objDimension.Length; j++)
                                        {
                                            totalDimsWt = totalDimsWt + Convert.ToDecimal(objDimension[j].weight.Trim() == string.Empty ? "0" : objDimension[j].weight.Trim());
                                        }
                                    }

                                    if (fwbdata.weight != "")
                                    {

                                        if (totalDimsWt == Convert.ToDecimal(fwbdata.weight))
                                        {
                                            isUpdateDIMSWeight = true;
                                        }
                                    }
                                    //Badiuz khan
                                    //Description: Delete Dimension if Dimension 

                                    //string[] dparam = { "AWBPrefix", "AWBNumber" };
                                    //SqlDbType[] dbparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                                    //object[] dbparamvalues = { AWBPrefix, awbnum };

                                    SqlParameter[] sqlParamsDeleteDims = new SqlParameter[]
                                    {
                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum }
                                    };

                                    //if (!dtb.InsertData("Messaging.uspDeleteDimensionThroughXMLMessage", dparam, dbparamtypes, dbparamvalues))
                                    if (!await _readWriteDao.ExecuteNonQueryAsync("Messaging.uspDeleteDimensionThroughXMLMessage", sqlParamsDeleteDims))
                                        // clsLog.WriteLogAzure("Error  Delete Dimension Through Message :" + awbnum);
                                        _logger.LogWarning("Error  Delete Dimension Through Message: {0}" , awbnum);
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

                                            if (!isUpdateDIMSWeight)
                                            {
                                                if (i == 0)
                                                {
                                                    objDimension[i].weight = fwbdata.weight;
                                                }
                                                else
                                                {
                                                    objDimension[i].weight = "0";
                                                }
                                            }

                                            //    string[] param = { "AWBNumber", "RowIndex", "Length", "Breadth", "Height",
                                            //"PcsCount", "MeasureUnit", "AWBPrefix", "Weight", "WeightCode", "UpdatedBy" ,"SLAC"};
                                            //    SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Decimal, SqlDbType.Decimal,
                                            //SqlDbType.Decimal, SqlDbType.Int, SqlDbType.VarChar,
                                            //SqlDbType.VarChar, SqlDbType.Decimal, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.Int };
                                            //    Decimal DimWeight = 0;

                                            //    object[] value ={awbnum,"1",objDimension[i].length,objDimension[i].width,objDimension[i].height,
                                            //    objDimension[i].piecenum,objDimension[i].mesurunitcode,AWBPrefix,
                                            //Decimal.TryParse(objDimension[i].weight,out DimWeight)==true?Convert.ToDecimal(objDimension[i].weight):0,
                                            //objDimension[i].weightcode,"xFWB",objDimension[i].dims_slac};

                                            Decimal DimWeight = 0;
                                            SqlParameter[] sqlParamsDims = new SqlParameter[]
                                            {
                                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                                new SqlParameter("@RowIndex", SqlDbType.Int) { Value = "1" },
                                                new SqlParameter("@Length", SqlDbType.Decimal) { Value = objDimension[i].length },
                                                new SqlParameter("@Breadth", SqlDbType.Decimal) { Value = objDimension[i].width },
                                                new SqlParameter("@Height", SqlDbType.Decimal) { Value = objDimension[i].height },
                                                new SqlParameter("@PcsCount", SqlDbType.Int) { Value = objDimension[i].piecenum },
                                                new SqlParameter("@MeasureUnit", SqlDbType.VarChar) { Value = objDimension[i].mesurunitcode },
                                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                                new SqlParameter("@Weight", SqlDbType.Decimal) { Value = Decimal.TryParse(objDimension[i].weight,out DimWeight)==true?Convert.ToDecimal(objDimension[i].weight):0 },
                                                new SqlParameter("@WeightCode", SqlDbType.VarChar) { Value = objDimension[i].weightcode },
                                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "xFWB" },
                                                new SqlParameter("@SLAC", SqlDbType.Int) { Value = objDimension[i].dims_slac }
                                            };


                                            //object[] value ={awbnum,"1",10,20,30,
                                            //        120,2,AWBPrefix,DimWeight,"FWB"};

                                            //if (!dtb.InsertData("Messaging.uspSaveAWBDimensionsXFFR", param, dbtypes, value))
                                            if (!await _readWriteDao.ExecuteNonQueryAsync("Messaging.uspSaveAWBDimensionsXFFR", sqlParamsDims))
                                            {
                                                // clsLog.WriteLogAzure("Error Saving  Dimension Through Message :" + awbnum);
                                                _logger.LogError("Error Saving  Dimension Through Message: {0}", awbnum);
                                            }
                                        }
                                    }
                                }

                                #endregion

                                #region FWB Message with BUP Shipment
                                //Badiuz khan
                                //Description: Save Bup through FWB
                                // decimal VolumeWt = 0;
                                if (objAWBBup.Length > 0)
                                {

                                    if (objDimension.Length == 0)
                                    {
                                        //string[] dparam = { "AWBPrefix", "AWBNumber" };
                                        //SqlDbType[] dbparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                                        //object[] dbparamvalues = { AWBPrefix, awbnum };

                                        SqlParameter[] sqlParamsDeleteDims = new SqlParameter[]
                                        {
                                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                            new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum }
                                        };

                                        //if (!dtb.InsertData("Messaging.uspDeleteDimensionThroughXMLMessage", dparam, dbparamtypes, dbparamvalues))
                                        if (!await _readWriteDao.ExecuteNonQueryAsync("Messaging.uspDeleteDimensionThroughXMLMessage", sqlParamsDeleteDims))
                                            // clsLog.WriteLogAzure("Error  Delete Dimension Through Message :" + awbnum);
                                            _logger.LogError("Error  Delete Dimension Through Message: {0}", awbnum);
                                    }


                                    if (fwbrates[0].volcode != "")
                                    {
                                        switch (fwbrates[0].volcode.ToUpper())
                                        {
                                            case "MC":
                                                VolumeWt = double.Parse(String.Format("{0:0.00}", Convert.ToDecimal(Convert.ToDecimal(fwbrates[0].volamt == "" ? "0" : fwbrates[0].volamt) * decimal.Parse("166.66"))));
                                                break;
                                            default:
                                                VolumeWt = Convert.ToDouble(fwbrates[0].volamt == "" ? "0" : fwbrates[0].volamt);
                                                break;
                                        }
                                    }

                                    for (int k = 0; k < objAWBBup.Length; k++)
                                    {
                                        if (objAWBBup[k].ULDNo != "" && objAWBBup[k].ULDNo != null)
                                        {
                                            string uldno = objAWBBup[k].ULDNo;
                                            int uldslacPcs = 0;
                                            double uldbupwt = 0.00;

                                            if (objAWBBup[k].SlacCount != "")
                                            {
                                                uldslacPcs = int.Parse(objAWBBup[k].SlacCount == "" ? "0" : objAWBBup[k].SlacCount);
                                            }
                                            if (objAWBBup[k].BUPWt != "")
                                            {
                                                uldbupwt = double.Parse(objAWBBup[k].BUPWt == "" ? fwbdata.weight : objAWBBup[k].BUPWt);
                                            }

                                            //    string[] param = { "AWBPrefix", "AWBNumber", "ULDNo", "SlacPcs", "PcsCount", "Volume", "GrossWeight" };
                                            //    SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int,
                                            //SqlDbType.Int, SqlDbType.Decimal, SqlDbType.Decimal };
                                            //    object[] value = { AWBPrefix, awbnum, uldno, uldslacPcs, fwbdata.pcscnt, VolumeWt,
                                            //uldbupwt };

                                            SqlParameter[] sqlParamsBUP = new SqlParameter[]
                                            {
                                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                                new SqlParameter("@ULDNo", SqlDbType.VarChar) { Value = uldno },
                                                new SqlParameter("@SlacPcs", SqlDbType.Int) { Value = uldslacPcs },
                                                new SqlParameter("@PcsCount", SqlDbType.Int) { Value = fwbdata.pcscnt },
                                                new SqlParameter("@Volume", SqlDbType.Decimal) { Value = VolumeWt },
                                                new SqlParameter("@GrossWeight", SqlDbType.Decimal) { Value = uldbupwt }
                                            };

                                            //if (!dtb.InsertData("SaveandUpdateShippperBUPThroughFWB", param, dbtypes, value))

                                            //if (!dtb.InsertData("Messaging.uspSaveandUpdateShippperBUPThroughXFWB", param, dbtypes, value))
                                            if (!await _readWriteDao.ExecuteNonQueryAsync("Messaging.uspSaveandUpdateShippperBUPThroughXFWB", sqlParamsBUP))
                                            {
                                                //string str = dtb.LastErrorDescription.ToString();
                                                // clsLog.WriteLogAzure("BUP ULD is not Updated  for:" + awbnum + Environment.NewLine);
                                                _logger.LogError("BUP ULD is not Updated  for: {0}" , awbnum + Environment.NewLine);

                                            }
                                        }
                                    }
                                }

                                #endregion

                                #region ProcessRateFunction

                                DataSet? dsrateCheck = await _fFRMessageProcessor.CheckAirlineForRateProcessing(AWBPrefix, "xFWB");
                                if (dsrateCheck != null && dsrateCheck.Tables.Count > 0 && dsrateCheck.Tables[0].Rows.Count > 0)
                                {
                                    //string[] CRNname = new string[] { "AWBNumber", "AWBPrefix", "UpdatedBy", "UpdatedOn", "ValidateMin", "UpdateBooking", "RouteFrom", "UpdateBilling" };
                                    //SqlDbType[] CRType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Bit, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.Bit };
                                    //object[] CRValues = new object[] { awbnum, AWBPrefix, "xFWB", System.DateTime.Now, 1, 1, "B", 0 };

                                    SqlParameter[] sqlParamsCalculateRates = new SqlParameter[]
                                    {
                                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                        new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "xFWB" },
                                        new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = System.DateTime.Now },
                                        new SqlParameter("@ValidateMin", SqlDbType.Bit) { Value = 1 },
                                        new SqlParameter("@UpdateBooking", SqlDbType.Bit) { Value = 1 },
                                        new SqlParameter("@RouteFrom", SqlDbType.VarChar) { Value = "B" },
                                        new SqlParameter("@UpdateBilling", SqlDbType.Bit) { Value = 0 }
                                    };
                                    //if (!dtb.ExecuteProcedure("sp_CalculateFreightChargesforMessage", "AWBNumber", SqlDbType.VarChar, awbnum))

                                    //if (!dtb.ExecuteProcedure("sp_CalculateAWBRatesReprocess", CRNname, CRType, CRValues))
                                    if (!await _readWriteDao.ExecuteNonQueryAsync("sp_CalculateAWBRatesReprocess", sqlParamsCalculateRates))
                                    {
                                        // clsLog.WriteLogAzure("Rates Not Calculated for:" + awbnum + Environment.NewLine);
                                        _logger.LogError("Rates Not Calculated for: {0}" , awbnum + Environment.NewLine);
                                    }
                                }

                                #endregion

                                //string[] QueryName = { "AWBNumber", "Status", "AWBPrefix", "UserName", "AWBIssueDate", "ChargeableWeight", "Priority" };
                                //SqlDbType[] QueryType = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Decimal, SqlDbType.VarChar };
                                //object[] QueryValue = { awbnum, "E", AWBPrefix, "xFWB", strAWbIssueDate, ChargeableWeight, Priority };

                                SqlParameter[] sqlParamsUpdateStatus = new SqlParameter[]
                                {
                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = "E" },
                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                    new SqlParameter("@UserName", SqlDbType.VarChar) { Value = "xFWB" },
                                    new SqlParameter("@AWBIssueDate", SqlDbType.DateTime) { Value = strAWbIssueDate },
                                    new SqlParameter("@ChargeableWeight", SqlDbType.Decimal) { Value = ChargeableWeight },
                                    new SqlParameter("@Priority", SqlDbType.VarChar) { Value = Priority }
                                };

                                //if (!dtb.UpdateData("UpdateStatustoExecuted", QueryName, QueryType, QueryValue))
                                if (!await _readWriteDao.ExecuteNonQueryAsync("UpdateStatustoExecuted", sqlParamsUpdateStatus))
                                    // clsLog.WriteLogAzure("Error in updating AWB status");
                                    _logger.LogError("Error in updating AWB status");

                                #region capacity
                                //string[] cparam = { "AWBPrefix", "AWBNumber" };
                                //SqlDbType[] cparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                                //object[] cparamvalues = { AWBPrefix, awbnum };

                                SqlParameter[] sqlParamsCapacity = new SqlParameter[]
                                {
                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum }
                                };

                                //if (!dtb.InsertData("UpdateCapacitythroughMessage", cparam, cparamtypes, cparamvalues))
                                if (!await _readWriteDao.ExecuteNonQueryAsync("UpdateCapacitythroughMessage", sqlParamsCapacity))
                                    // clsLog.WriteLogAzure("Error  on Update capacity Plan :" + awbnum);
                                    _logger.LogError("Error  on Update capacity Plan :{0}" , awbnum);

                                #endregion

                            }


                        }
                    }
                    else
                    {
                        //clsLog.WriteLogAzure("Error while save FWB Message:" + awbnum + "-" + dtb.LastErrorDescription);
                        // clsLog.WriteLogAzure("Error while save FWB Message:" + awbnum);
                        _logger.LogWarning("Error while save FWB Message: {0}" , awbnum);

                    }
                }

            }

            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref ex);
                // clsLog.WriteLogAzure("Error on FWB  message:" + ex.ToString());
                _logger.LogError("Error on FWB  message: {0}" , ex);
                ErrorMsg = "Error while saving AWB data through xFWB";
                flag = false;
            }
            //return flag;
            return (flag, ErrorMsg);
        }


        public async Task<(bool success, string errormessage)> SetAWBStatus(string awbNumber, string status, string errormessage, DateTime executionDt,
          string userName, DateTime currentDt, string awbPrefix, bool validateData, string paymentMode, int updateOpsData, string executedAt,
          DateTime Recivedon, int Refno, string purposecode)
        {
            //SQLServer da = new SQLServer();
            DataSet? dsResult = null;
            try
            {
                //string[] param = { "AWBNumber", "Status", "ExecutionDt", "UserName", "TimeStamp", "AWBPrefix",
                //    "ValidateData", "PaymentMode", "UpdateOPSData", "ExecutedAt","Recivedon","srno" , "purposecode"};
                //SqlDbType[] sqldbtype = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar,
                //    SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.Bit,
                //    SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar,SqlDbType.DateTime,SqlDbType.Int,SqlDbType.VarChar };
                //object[] values = { awbNumber, status, executionDt, userName, currentDt, awbPrefix,
                //    validateData, paymentMode, updateOpsData, executedAt,Recivedon,Refno,purposecode };

                SqlParameter[] sqlParams = new SqlParameter[]
                {
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbNumber },
                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = status },
                    new SqlParameter("@ExecutionDt", SqlDbType.DateTime) { Value = executionDt },
                    new SqlParameter("@UserName", SqlDbType.VarChar) { Value = userName },
                    new SqlParameter("@TimeStamp", SqlDbType.DateTime) { Value = currentDt },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = awbPrefix },
                    new SqlParameter("@ValidateData", SqlDbType.Bit) { Value = validateData },
                    new SqlParameter("@PaymentMode", SqlDbType.VarChar) { Value = paymentMode },
                    new SqlParameter("@UpdateOPSData", SqlDbType.Bit) { Value = updateOpsData },
                    new SqlParameter("@ExecutedAt", SqlDbType.VarChar) { Value = executedAt },
                    new SqlParameter("@Recivedon", SqlDbType.DateTime) { Value = Recivedon },
                    new SqlParameter("@srno", SqlDbType.Int) { Value = Refno },
                    new SqlParameter("@purposecode", SqlDbType.VarChar) { Value = purposecode }
                };

                //dsResult = da.SelectRecords("Messaging.uspDeleteBookingDataFromXFWB", param, values, sqldbtype);
                dsResult = await _readWriteDao.SelectRecords("Messaging.uspDeleteBookingDataFromXFWB", sqlParams);
                if (dsResult != null)
                {
                    if (dsResult.Tables.Count != 0)
                    {

                        if (dsResult.Tables[0].Rows.Count != 0)
                        {
                            if (dsResult.Tables[0].Rows[0][0].ToString() == "Y")
                            {
                                //return true;
                                return (true, errormessage);
                            }
                            else
                            {
                                errormessage = " " + dsResult.Tables[0].Rows[0][1];
                                //return false;
                                return (false, errormessage);
                            }
                        }
                        else
                        {
                            errormessage = "msgErrorSetAWBStatus1";
                            //return false;
                            return (false, errormessage);
                        }

                    }
                    else
                    {
                        errormessage = "msgErrorSetAWBStatus2";
                        //return false;
                        return (false, errormessage);
                    }
                }
                else
                {
                    errormessage = "msgErrorSetAWBStatus3";
                    //return false;
                    return (false, errormessage);
                }

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                dsResult = null;
                errormessage = "msgExceptionError";
                //return false;
                return (false, errormessage);
            }
            finally
            {
                if (dsResult != null)
                {
                    dsResult.Dispose();
                }
            }

        }

        //public string GenerateXFWBMessage(string awbPrefix, string awbNumber)
        //{
        //    StringBuilder generateMessage = new StringBuilder();
        //    try
        //    {

        //        DataSet dsFWBMessage = GetRecordforAWBToGenerateXFWBMessage(awbPrefix, awbNumber);

        //        GenericFunction generalfunction = new GenericFunction();

        //        if (dsFWBMessage != null && dsFWBMessage.Tables.Count > 0 && dsFWBMessage.Tables[0].Rows.Count > 0 && dsFWBMessage.Tables[1].Rows.Count > 0 && dsFWBMessage.Tables[2].Rows.Count > 0 && dsFWBMessage.Tables[3].Rows.Count > 0 && dsFWBMessage.Tables[5].Rows.Count > 0)
        //        {
        //            var xmlSchemaTable = generalfunction.GetXMLMessageData("XFWB").Tables[0];
        //            if (xmlSchemaTable != null && xmlSchemaTable.Rows.Count > 0)
        //            {
        //                string messageXml = Convert.ToString(xmlSchemaTable.Rows[0]["XMLMessageData"]);
        //                var fwbXmlDataSet = new DataSet();
        //                messageXml = ReplacingNodeNames(messageXml);
        //                var tx = new StringReader(messageXml);
        //                fwbXmlDataSet.ReadXml(tx);

        //                // FWB Message Header Segment
        //                fwbXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["ID"]);
        //                fwbXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["Name"] = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["NAME"]);
        //                fwbXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["TypeCode"] = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["TypeCode"]);
        //                fwbXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["IssueDateTime"] = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["IssueDateTime"]);
        //                fwbXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["PurposeCode"] = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["PurposeCode"]);
        //                fwbXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"] = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["VersionID"]);
        //                fwbXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["ConversationID"] = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["ConversationID"]);

        //                DataRow[] drs;
        //                //SenderParty
        //                if (fwbXmlDataSet.Tables.Contains("PrimaryID"))
        //                {
        //                    drs = fwbXmlDataSet.Tables["PrimaryID"].Select("SenderParty_Id=0");
        //                    if (drs.Length > 0)
        //                    {
        //                        drs[0]["schemeID"] = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["SenderParty_PrimaryID"]);
        //                        drs[0]["PrimaryID_Text"] = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["SenderParty_PrimaryIDText"]);
        //                    }
        //                }
        //                //RecipientParty
        //                if (fwbXmlDataSet.Tables.Contains("PrimaryID"))
        //                {
        //                    drs = fwbXmlDataSet.Tables["PrimaryID"].Select("RecipientParty_Id=0");
        //                    if (drs.Length > 0)
        //                    {
        //                        drs[0]["schemeID"] = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["RecipientParty_PrimaryID"]);
        //                        drs[0]["PrimaryID_Text"] = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["RecipientParty_PrimaryIDText"]);
        //                    }
        //                }
        //                //FWB Message BusinessHeaderDocument Segment

        //                fwbXmlDataSet.Tables["BusinessHeaderDocument"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["AWBNumber"]);
        //                fwbXmlDataSet.Tables["BusinessHeaderDocument"].Rows[0]["SenderAssignedID"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["BHD_SenderAssignedID"]);
        //                fwbXmlDataSet.Tables["IncludedHeaderNote"].Rows[0]["ContentCode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["HeaderNote"]);
        //                fwbXmlDataSet.Tables["IncludedHeaderNote"].Rows[0]["Content"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["HeaderText"]);
        //                fwbXmlDataSet.Tables["SignatoryConsignorAuthentication"].Rows[0]["Signatory"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperName"]);
        //                fwbXmlDataSet.Tables["SignatoryCarrierAuthentication"].Rows[0]["ActualDateTime"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["CarrierDeclarationDate"]);
        //                fwbXmlDataSet.Tables["SignatoryCarrierAuthentication"].Rows[0]["Signatory"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["CarrierDeclarationSignature"]);
        //                fwbXmlDataSet.Tables["IssueAuthenticationLocation"].Rows[0]["Name"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["CarrierDeclarationPlace"]);

        //                //Master Consignment Segment

        //                fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["MasterConsignment_ID"]);
        //                fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["AdditionalID"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["MasterConsignment_AdditionalID"]);
        //                fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["FreightForwarderAssignedID"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["FreightForwarderAssignedID"]);
        //                fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["AssociatedReferenceID"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["AssociatedReferenceID"]);

        //                if (fwbXmlDataSet.Tables.Contains("DeclaredValueForCarriageAmount"))
        //                {
        //                    if (fwbXmlDataSet.Tables["DeclaredValueForCarriageAmount"].Columns.Contains("currencyID"))
        //                        fwbXmlDataSet.Tables["DeclaredValueForCarriageAmount"].Rows[0]["currencyID"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["DVCarriageCurrency"]);

        //                    if (fwbXmlDataSet.Tables["DeclaredValueForCarriageAmount"].Columns.Contains("DeclaredValueForCarriageAmount_Text"))
        //                        fwbXmlDataSet.Tables["DeclaredValueForCarriageAmount"].Rows[0]["DeclaredValueForCarriageAmount_Text"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["DVForCarriage"]);

        //                }

        //                if (fwbXmlDataSet.Tables.Contains("DeclaredValueForCustomsAmount"))
        //                {
        //                    if (fwbXmlDataSet.Tables["DeclaredValueForCustomsAmount"].Columns.Contains("currencyID"))
        //                        fwbXmlDataSet.Tables["DeclaredValueForCustomsAmount"].Rows[0]["currencyID"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["DVCustomCurrency"]);

        //                    if (fwbXmlDataSet.Tables["DeclaredValueForCustomsAmount"].Columns.Contains("DeclaredValueForCustomsAmount_Text"))
        //                        fwbXmlDataSet.Tables["DeclaredValueForCustomsAmount"].Rows[0]["DeclaredValueForCustomsAmount_Text"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["DVForCustom"]);
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("InsuranceValueAmount"))
        //                {
        //                    if (fwbXmlDataSet.Tables["InsuranceValueAmount"].Columns.Contains("currencyID"))
        //                        fwbXmlDataSet.Tables["InsuranceValueAmount"].Rows[0]["currencyID"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["InsuranceCurrency"]);

        //                    if (fwbXmlDataSet.Tables["InsuranceValueAmount"].Columns.Contains("InsuranceValueAmount_Text"))
        //                        fwbXmlDataSet.Tables["InsuranceValueAmount"].Rows[0]["InsuranceValueAmount_Text"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["InsuranceAmount"]);
        //                }



        //                fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["NilCarriageValueIndicator"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["NilCarriageValueIndicator"]);
        //                fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["NilCustomsValueIndicator"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["NilCustomsValueIndicator"]);
        //                fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["NilInsuranceValueIndicator"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["NilInsuranceValueIndicator"]);


        //                fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["TotalChargePrepaidIndicator"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["TotalChargePrepaidIndicator"]);
        //                fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["TotalDisbursementPrepaidIndicator"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["TotalDisbursementPrepaidIndicator"]);

        //                if (fwbXmlDataSet.Tables.Contains("IncludedTareGrossWeightMeasure"))
        //                {
        //                    if (fwbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Columns.Contains("UnitCode"))
        //                        fwbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Rows[0]["UnitCode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["UOM"]);

        //                    if (fwbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Columns.Contains("IncludedTareGrossWeightMeasure_Text"))
        //                        fwbXmlDataSet.Tables["IncludedTareGrossWeightMeasure"].Rows[0]["IncludedTareGrossWeightMeasure_Text"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["GrossWeight"]);
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("GrossVolumeMeasure"))
        //                {
        //                    drs = fwbXmlDataSet.Tables["GrossVolumeMeasure"].Select("MasterConsignment_Id=0");
        //                    if (drs.Length > 0)
        //                    {
        //                        drs[0]["unitCode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["UOM"]);
        //                        drs[0]["GrossVolumeMeasure_Text"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["VolumetricWeight"]);
        //                    }
        //                }

        //                //fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["DensityGroupCode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["DensityGroup"]) == "" ? "XXXXXX" : Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["DensityGroup"]);

        //                fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["PackageQuantity"] = "";// Convert.ToString(dsFWBMessage.Tables[9].Rows[0]["SLAC"]);
        //                fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["TotalPieceQuantity"] = "";// Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["PiecesCount"]);

        //                fwbXmlDataSet.Tables["MasterConsignment"].Rows[0]["ProductID"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["ProductType"]);

        //                //Consignor Party
        //                if (fwbXmlDataSet.Tables.Contains("PrimaryID"))
        //                {
        //                    drs = fwbXmlDataSet.Tables["PrimaryID"].Select("ConsignorParty_Id=0");
        //                    if (drs.Length > 0)
        //                    {
        //                        drs[0]["schemeAgencyID"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperAccCode"]);
        //                        drs[0]["PrimaryID_Text"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperName"]);
        //                    }
        //                }


        //                fwbXmlDataSet.Tables["ConsignorParty"].Rows[0]["AdditionalID"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperStandardID"]);
        //                fwbXmlDataSet.Tables["ConsignorParty"].Rows[0]["Name"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperName"]);
        //                fwbXmlDataSet.Tables["ConsignorParty"].Rows[0]["AccountID"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperAccCode"]);

        //                fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["PostcodeCode"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperAccCode"]);

        //                fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["StreetName"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperAddress"]);

        //                fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CityName"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperCity"]);

        //                fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountryID"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperCountry"]);

        //                fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountryName"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperCountry"]);

        //                fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountrySubDivisionName"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperRegionName"]);

        //                fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["PostOfficeBox"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperPOBox"]);

        //                fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CityID"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperCity"]);

        //                fwbXmlDataSet.Tables["ConsignorParty_PostalStructuredAddress"].Rows[0]["CountrySubDivisionID"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperRegionName"]);

        //                if (fwbXmlDataSet.Tables.Contains("DefinedTradeContact"))
        //                {
        //                    drs = fwbXmlDataSet.Tables["DefinedTradeContact"].Select("ConsignorParty_Id=0");
        //                    if (drs.Length > 0)
        //                    {
        //                        drs[0]["PersonName"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperName"]);
        //                        drs[0]["DepartmentName"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperDepartnmentName"]);
        //                    }
        //                }


        //                if (fwbXmlDataSet.Tables.Contains("DirectTelephoneCommunication"))
        //                {
        //                    if (fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Columns.Contains("CompleteNumber"))
        //                        fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Rows[0]["CompleteNumber"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperTelephone"]);
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("FaxCommunication"))
        //                {
        //                    if (fwbXmlDataSet.Tables["FaxCommunication"].Columns.Contains("CompleteNumber"))
        //                        fwbXmlDataSet.Tables["FaxCommunication"].Rows[0]["CompleteNumber"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperFaxNo"]);
        //                }
        //                if (fwbXmlDataSet.Tables.Contains("URIEmailCommunication"))
        //                {
        //                    if (fwbXmlDataSet.Tables["URIEmailCommunication"].Columns.Contains("URIID"))
        //                        fwbXmlDataSet.Tables["URIEmailCommunication"].Rows[0]["URIID"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperEmailId"]);

        //                }
        //                if (fwbXmlDataSet.Tables.Contains("TelexCommunication"))
        //                {
        //                    if (fwbXmlDataSet.Tables["TelexCommunication"].Columns.Contains("CompleteNumber"))
        //                        fwbXmlDataSet.Tables["TelexCommunication"].Rows[0]["CompleteNumber"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperTelex"]);

        //                }


        //                // Consignee Detail of AWB

        //                if (fwbXmlDataSet.Tables.Contains("PrimaryID"))
        //                {
        //                    drs = fwbXmlDataSet.Tables["PrimaryID"].Select("ConsigneeParty_Id=0");
        //                    if (drs.Length > 0)
        //                    {
        //                        drs[0]["schemeAgencyID"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigAccCode"]);
        //                        drs[0]["PrimaryID_Text"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeName"]);
        //                    }
        //                }

        //                fwbXmlDataSet.Tables["ConsigneeParty"].Rows[0]["AdditionalID"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeStandardID"]);

        //                fwbXmlDataSet.Tables["ConsigneeParty"].Rows[0]["Name"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeName"]);

        //                fwbXmlDataSet.Tables["ConsigneeParty"].Rows[0]["AccountID"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigAccCode"]);

        //                fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["PostcodeCode"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneePincode"]);

        //                fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["StreetName"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeAddress"]);

        //                fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CityName"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeCity"]);

        //                fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountryID"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeCountry"]);

        //                fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountryName"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeCountry"]);

        //                fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountrySubDivisionName"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeRegionName"]);

        //                fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["PostOfficeBox"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneePincode"]);

        //                fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CityID"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeCity"]);

        //                fwbXmlDataSet.Tables["ConsigneeParty_PostalStructuredAddress"].Rows[0]["CountrySubDivisionID"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeCountry"]);


        //                if (fwbXmlDataSet.Tables.Contains("DefinedTradeContact"))
        //                {
        //                    drs = fwbXmlDataSet.Tables["DefinedTradeContact"].Select("ConsigneeParty_Id=0");
        //                    if (drs.Length > 0)
        //                    {
        //                        drs[0]["PersonName"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeCountry"]);
        //                        drs[0]["DepartmentName"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeDepartnmentName"]);
        //                    }
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("DirectTelephoneCommunication"))
        //                {
        //                    if (fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Columns.Contains("CompleteNumber"))
        //                        fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Rows[1]["CompleteNumber"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeTelephone"]);

        //                }

        //                if (fwbXmlDataSet.Tables.Contains("FaxCommunication"))
        //                {
        //                    if (fwbXmlDataSet.Tables["FaxCommunication"].Columns.Contains("CompleteNumber"))
        //                        fwbXmlDataSet.Tables["FaxCommunication"].Rows[1]["CompleteNumber"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeFaxNo"]);

        //                }
        //                if (fwbXmlDataSet.Tables.Contains("URIEmailCommunication"))
        //                {
        //                    if (fwbXmlDataSet.Tables["URIEmailCommunication"].Columns.Contains("URIID"))
        //                        fwbXmlDataSet.Tables["URIEmailCommunication"].Rows[1]["URIID"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigEmailId"]);

        //                }
        //                if (fwbXmlDataSet.Tables.Contains("TelexCommunication"))
        //                {
        //                    if (fwbXmlDataSet.Tables["TelexCommunication"].Columns.Contains("CompleteNumber"))
        //                        fwbXmlDataSet.Tables["TelexCommunication"].Rows[1]["CompleteNumber"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeTelex"]);

        //                }


        //                //FreightForwarderParty
        //                #region FreightForwarderParty

        //                if (fwbXmlDataSet.Tables.Contains("PrimaryID"))
        //                {
        //                    drs = fwbXmlDataSet.Tables["PrimaryID"].Select("FreightForwarderParty_Id=0");
        //                    if (drs.Length > 0)
        //                    {
        //                        drs[0]["schemeAgencyID"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["AgentAccountCode"]);
        //                        drs[0]["PrimaryID_Text"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["ShippingAgentName"]);
        //                    }
        //                }

        //                fwbXmlDataSet.Tables["FreightForwarderParty"].Rows[0]["AdditionalID"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["ShippingAgentCode"]);

        //                fwbXmlDataSet.Tables["FreightForwarderParty"].Rows[0]["Name"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["ShippingAgentName"]);

        //                fwbXmlDataSet.Tables["FreightForwarderParty"].Rows[0]["AccountID"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["AgentAccountCode"]);

        //                fwbXmlDataSet.Tables["FreightForwarderParty"].Rows[0]["CargoAgentID"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["IATAAgentCode"]);

        //                fwbXmlDataSet.Tables["FreightForwarderAddress"].Rows[0]["PostcodeCode"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["PostalZIP"]);

        //                fwbXmlDataSet.Tables["FreightForwarderAddress"].Rows[0]["CityName"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["City"]);

        //                fwbXmlDataSet.Tables["FreightForwarderAddress"].Rows[0]["StreetName"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["AgentAddress"]);

        //                fwbXmlDataSet.Tables["FreightForwarderAddress"].Rows[0]["CityName"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["City"]);

        //                fwbXmlDataSet.Tables["FreightForwarderAddress"].Rows[0]["CountryID"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["Country"]);

        //                fwbXmlDataSet.Tables["FreightForwarderAddress"].Rows[0]["CountryName"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["CountryName"]);

        //                fwbXmlDataSet.Tables["FreightForwarderAddress"].Rows[0]["CountrySubDivisionName"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["State"]);

        //                fwbXmlDataSet.Tables["FreightForwarderAddress"].Rows[0]["PostOfficeBox"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["PostalZIP"]);

        //                fwbXmlDataSet.Tables["FreightForwarderAddress"].Rows[0]["CityID"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["Station"]);

        //                fwbXmlDataSet.Tables["FreightForwarderAddress"].Rows[0]["CountrySubDivisionID"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["State"]);




        //                if (fwbXmlDataSet.Tables.Contains("SpecifiedCargoAgentLocation"))
        //                {
        //                    fwbXmlDataSet.Tables["SpecifiedCargoAgentLocation"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["CassId"]);
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("DefinedTradeContact"))
        //                {
        //                    drs = fwbXmlDataSet.Tables["DefinedTradeContact"].Select("FreightForwarderParty_Id=0");
        //                    if (drs.Length > 0)
        //                    {
        //                        drs[0]["PersonName"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["ShippingAgentName"]);
        //                        drs[0]["DepartmentName"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["DepartmentName"]);
        //                    }
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("DirectTelephoneCommunication"))
        //                {
        //                    if (fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Columns.Contains("CompleteNumber"))
        //                        fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Rows[2]["CompleteNumber"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["AgentPhone"]);
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("FaxCommunication"))
        //                {
        //                    if (fwbXmlDataSet.Tables["FaxCommunication"].Columns.Contains("CompleteNumber"))
        //                        fwbXmlDataSet.Tables["FaxCommunication"].Rows[2]["CompleteNumber"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["AgentFax"]);
        //                }
        //                if (fwbXmlDataSet.Tables.Contains("URIEmailCommunication"))
        //                {
        //                    if (fwbXmlDataSet.Tables["URIEmailCommunication"].Columns.Contains("URIID"))
        //                        fwbXmlDataSet.Tables["URIEmailCommunication"].Rows[2]["URIID"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["AgentEmailID"]);

        //                }
        //                if (fwbXmlDataSet.Tables.Contains("TelexCommunication"))
        //                {
        //                    if (fwbXmlDataSet.Tables["TelexCommunication"].Columns.Contains("CompleteNumber"))
        //                        fwbXmlDataSet.Tables["TelexCommunication"].Rows[2]["CompleteNumber"] = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["AgentSitaAddress"]);

        //                }
        //                #endregion
        //                //AssociatedParty
        //                #region AssociatedParty

        //                if (fwbXmlDataSet.Tables.Contains("PrimaryID"))
        //                {
        //                    drs = fwbXmlDataSet.Tables["PrimaryID"].Select("AssociatedParty_Id=0");
        //                    if (drs.Length > 0)
        //                    {
        //                        drs[0]["schemeAgencyID"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_schemeAgencyID"]);
        //                        drs[0]["PrimaryID_Text"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_PrimaryID_Text"]);
        //                    }
        //                }
        //                fwbXmlDataSet.Tables["AssociatedParty"].Rows[0]["AdditionalID"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_AdditionalID"]);
        //                fwbXmlDataSet.Tables["AssociatedParty"].Rows[0]["Name"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_Name"]);
        //                fwbXmlDataSet.Tables["AssociatedParty"].Rows[0]["RoleCode"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_RoleCode"]);
        //                fwbXmlDataSet.Tables["AssociatedParty"].Rows[0]["Role"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_Role"]);

        //                fwbXmlDataSet.Tables["AssociatedParty_PostalStructuredAddress"].Rows[0]["PostcodeCode"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_PostcodeCode"]);
        //                fwbXmlDataSet.Tables["AssociatedParty_PostalStructuredAddress"].Rows[0]["StreetName"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_StreetName"]);
        //                fwbXmlDataSet.Tables["AssociatedParty_PostalStructuredAddress"].Rows[0]["CityName"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_CityName"]);
        //                fwbXmlDataSet.Tables["AssociatedParty_PostalStructuredAddress"].Rows[0]["CountryID"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_CountryID"]);
        //                fwbXmlDataSet.Tables["AssociatedParty_PostalStructuredAddress"].Rows[0]["CountryName"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_CountryName"]);
        //                fwbXmlDataSet.Tables["AssociatedParty_PostalStructuredAddress"].Rows[0]["CountrySubDivisionName"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_CountrySubDivisionName"]);
        //                fwbXmlDataSet.Tables["AssociatedParty_PostalStructuredAddress"].Rows[0]["PostOfficeBox"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_PostOfficeBox"]);
        //                fwbXmlDataSet.Tables["AssociatedParty_PostalStructuredAddress"].Rows[0]["CityID"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_CityID"]);
        //                fwbXmlDataSet.Tables["AssociatedParty_PostalStructuredAddress"].Rows[0]["CountrySubDivisionID"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_CountrySubDivisionID"]);

        //                fwbXmlDataSet.Tables["SpecifiedAddressLocation"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_SpecifiedAddressLocation_ID"]);
        //                fwbXmlDataSet.Tables["SpecifiedAddressLocation"].Rows[0]["Name"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_SpecifiedAddressLocation_Name"]);
        //                fwbXmlDataSet.Tables["SpecifiedAddressLocation"].Rows[0]["TypeCode"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_SpecifiedAddressLocation_TypeCode"]);

        //                if (fwbXmlDataSet.Tables.Contains("DefinedTradeContact"))
        //                {
        //                    drs = fwbXmlDataSet.Tables["DefinedTradeContact"].Select("AssociatedParty_Id=0");
        //                    if (drs.Length > 0)
        //                    {
        //                        drs[0]["PersonName"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_DefinedTradeContact_PersonName"]);
        //                        drs[0]["DepartmentName"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_DefinedTradeContact_DepartmentName"]);
        //                    }
        //                }
        //                //AssociatedParty
        //                fwbXmlDataSet.Tables["DirectTelephoneCommunication"].Rows[3]["CompleteNumber"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_DirectTelephoneCommunication"]);

        //                fwbXmlDataSet.Tables["FaxCommunication"].Rows[3]["CompleteNumber"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_FaxCommunication"]);

        //                fwbXmlDataSet.Tables["URIEmailCommunication"].Rows[3]["URIID"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_URIEmailCommunication"]);

        //                fwbXmlDataSet.Tables["TelexCommunication"].Rows[3]["CompleteNumber"] = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_TelexCommunication"]);
        //                #endregion
        //                ///////////////////////////////////////////////////////////////
        //                if (fwbXmlDataSet.Tables.Contains("OriginLocation"))
        //                {
        //                    fwbXmlDataSet.Tables["OriginLocation"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["OriginCode"]);

        //                    fwbXmlDataSet.Tables["OriginLocation"].Rows[0]["Name"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["OriginAirport"]);
        //                }


        //                if (fwbXmlDataSet.Tables.Contains("FinalDestinationLocation"))
        //                {
        //                    fwbXmlDataSet.Tables["FinalDestinationLocation"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["DestinationCode"]);

        //                    fwbXmlDataSet.Tables["FinalDestinationLocation"].Rows[0]["Name"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["DestinationAirport"]);
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("SpecifiedLogisticsTransportMovement"))
        //                {
        //                    fwbXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["StageCode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["ModeOfTransport"]);

        //                    fwbXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ModeCode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["TransportModeCode"]);

        //                    fwbXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["Mode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["AirTransportMode"]);

        //                    fwbXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[3].Rows[0]["FltNumber"]);

        //                    fwbXmlDataSet.Tables["SpecifiedLogisticsTransportMovement"].Rows[0]["SequenceNumeric"] = Convert.ToString(dsFWBMessage.Tables[3].Rows[0]["SpecifiedLogisticsTransportMovement_SeqNum"]);

        //                }

        //                if (fwbXmlDataSet.Tables.Contains("UsedLogisticsTransportMeans"))
        //                {
        //                    if (dsFWBMessage.Tables.Count > 2 && dsFWBMessage.Tables[3].Rows.Count > 0)
        //                    {
        //                        fwbXmlDataSet.Tables["UsedLogisticsTransportMeans"].Rows[0]["Name"] = Convert.ToString(dsFWBMessage.Tables[3].Rows[0]["PartnerName"]);
        //                    }
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("ArrivalEvent"))
        //                {
        //                    if (dsFWBMessage.Tables.Count > 2 && dsFWBMessage.Tables[3].Rows.Count > 0)
        //                    {
        //                        fwbXmlDataSet.Tables["ArrivalEvent"].Rows[0]["ScheduledOccurrenceDateTime"] = Convert.ToString(dsFWBMessage.Tables[3].Rows[0]["FltDate"]);
        //                    }
        //                }
        //                if (fwbXmlDataSet.Tables.Contains("OccurrenceArrivalLocation"))
        //                {
        //                    if (dsFWBMessage.Tables.Count > 2 && dsFWBMessage.Tables[3].Rows.Count > 0)
        //                    {
        //                        fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[3].Rows[0]["FltDestination"]);
        //                        fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Rows[0]["Name"] = Convert.ToString(dsFWBMessage.Tables[3].Rows[0]["DestinationAirportName"]);
        //                        fwbXmlDataSet.Tables["OccurrenceArrivalLocation"].Rows[0]["TypeCode"] = Convert.ToString(dsFWBMessage.Tables[3].Rows[0]["TypeCode"]);
        //                    }
        //                }
        //                if (fwbXmlDataSet.Tables.Contains("DepartureEvent"))
        //                {

        //                    if (dsFWBMessage.Tables.Count > 2 && dsFWBMessage.Tables[3].Rows.Count > 0)
        //                    {
        //                        fwbXmlDataSet.Tables["DepartureEvent"].Rows[0]["ScheduledOccurrenceDateTime"] = Convert.ToString(dsFWBMessage.Tables[3].Rows[0]["FltDate"]);
        //                    }

        //                }
        //                if (fwbXmlDataSet.Tables.Contains("OccurrenceDepartureLocation"))
        //                {
        //                    if (dsFWBMessage.Tables.Count > 2 && dsFWBMessage.Tables[3].Rows.Count > 0)
        //                    {
        //                        fwbXmlDataSet.Tables["OccurrenceDepartureLocation"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[3].Rows[0]["FltOrigin"]);

        //                        fwbXmlDataSet.Tables["OccurrenceDepartureLocation"].Rows[0]["Name"] = Convert.ToString(dsFWBMessage.Tables[3].Rows[0]["OriginAirportName"]);

        //                        fwbXmlDataSet.Tables["OccurrenceDepartureLocation"].Rows[0]["TypeCode"] = Convert.ToString(dsFWBMessage.Tables[3].Rows[0]["TypeCode"]);

        //                    }
        //                }
        //                if (fwbXmlDataSet.Tables.Contains("UtilizedLogisticsTransportEquipment"))
        //                {
        //                    if (dsFWBMessage.Tables.Count > 2 && dsFWBMessage.Tables[4].Rows.Count > 0)
        //                    {
        //                        fwbXmlDataSet.Tables["UtilizedLogisticsTransportEquipment"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[4].Rows[0]["VehicleNo"]);

        //                        fwbXmlDataSet.Tables["UtilizedLogisticsTransportEquipment"].Rows[0]["CharacteristicCode"] = Convert.ToString(dsFWBMessage.Tables[4].Rows[0]["VehType"]);

        //                        fwbXmlDataSet.Tables["UtilizedLogisticsTransportEquipment"].Rows[0]["Characteristic"] = Convert.ToString(dsFWBMessage.Tables[4].Rows[0]["VehicleCapacity"]);
        //                    }

        //                }

        //                if (fwbXmlDataSet.Tables.Contains("AffixedLogisticsSeal"))
        //                {
        //                    fwbXmlDataSet.Tables["AffixedLogisticsSeal"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["VehcialeSealNo"]);
        //                }
        //                if (fwbXmlDataSet.Tables.Contains("HandlingSPHInstructions"))
        //                {
        //                    fwbXmlDataSet.Tables["HandlingSPHInstructions"].Rows[0]["Description"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["SHCDescription"]);

        //                    fwbXmlDataSet.Tables["HandlingSPHInstructions"].Rows[0]["DescriptionCode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["SHCCodes"]);

        //                }
        //                if (fwbXmlDataSet.Tables.Contains("HandlingSSRInstructions"))
        //                {
        //                    fwbXmlDataSet.Tables["HandlingSSRInstructions"].Rows[0]["Description"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["HandlingInfo"]);

        //                    fwbXmlDataSet.Tables["HandlingSSRInstructions"].Rows[0]["DescriptionCode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["SHCCodes"]);

        //                }
        //                if (fwbXmlDataSet.Tables.Contains("HandlingOSIInstructions"))
        //                {
        //                    fwbXmlDataSet.Tables["HandlingOSIInstructions"].Rows[0]["Description"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["HandlingInfo"]);

        //                    fwbXmlDataSet.Tables["HandlingOSIInstructions"].Rows[0]["DescriptionCode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["SHCCodes"]);

        //                }
        //                if (fwbXmlDataSet.Tables.Contains("IncludedAccountingNote"))
        //                {
        //                    fwbXmlDataSet.Tables["IncludedAccountingNote"].Rows[0]["ContentCode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["AccCode"]);

        //                    fwbXmlDataSet.Tables["IncludedAccountingNote"].Rows[0]["Content"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["AccountInfo"]);

        //                }
        //                if (fwbXmlDataSet.Tables.Contains("IncludedCustomsNote"))
        //                {
        //                    fwbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["ContentCode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["IncludedCustomsNote_ContentCode"]);

        //                    fwbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["Content"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["IncludedCustomsNote_Content"]);

        //                    fwbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["SubjectCode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["IncludedCustomsNote_SubjectCode"]);

        //                    fwbXmlDataSet.Tables["IncludedCustomsNote"].Rows[0]["CountryID"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["IncludedCustomsNote_Country"]);

        //                }

        //                //AssociatedReferenceDocument
        //                if (fwbXmlDataSet.Tables.Contains("AssociatedReferenceDocument"))
        //                {
        //                    fwbXmlDataSet.Tables["AssociatedReferenceDocument"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[12].Rows[0]["AssociatedReferenceDocumentID"]);
        //                    fwbXmlDataSet.Tables["AssociatedReferenceDocument"].Rows[0]["IssueDateTime"] = Convert.ToString(dsFWBMessage.Tables[12].Rows[0]["AssociatedReferenceDocument_IssueDateTime"]);
        //                    fwbXmlDataSet.Tables["AssociatedReferenceDocument"].Rows[0]["TypeCode"] = Convert.ToString(dsFWBMessage.Tables[12].Rows[0]["AssociatedReferenceDocument_TypeCode"]);
        //                    fwbXmlDataSet.Tables["AssociatedReferenceDocument"].Rows[0]["Name"] = Convert.ToString(dsFWBMessage.Tables[12].Rows[0]["AssociatedReferenceDocumentName"]);
        //                }


        //                if (fwbXmlDataSet.Tables.Contains("AssociatedConsignmentCustomsProcedure"))
        //                {
        //                    fwbXmlDataSet.Tables["AssociatedConsignmentCustomsProcedure"].Rows[0]["GoodsStatusCode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["SCI"]);
        //                }


        //                if (fwbXmlDataSet.Tables.Contains("ApplicableOriginCurrencyExchange"))
        //                {
        //                    fwbXmlDataSet.Tables["ApplicableOriginCurrencyExchange"].Rows[0]["SourceCurrencyCode"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]);
        //                }
        //                if (fwbXmlDataSet.Tables.Contains("ApplicableDestinationCurrencyExchange"))
        //                {
        //                    if (dsFWBMessage.Tables.Count > 4 && dsFWBMessage.Tables[5].Rows.Count > 0)
        //                    {
        //                        fwbXmlDataSet.Tables["ApplicableDestinationCurrencyExchange"].Rows[0]["TargetCurrencyCode"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["BaseCurrency"]);
        //                        fwbXmlDataSet.Tables["ApplicableDestinationCurrencyExchange"].Rows[0]["MarketID"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["ConvRateQualifier"]);
        //                        fwbXmlDataSet.Tables["ApplicableDestinationCurrencyExchange"].Rows[0]["ConversionRate"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["ConvFactor"]);
        //                    }
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("ApplicableLogisticsServiceCharge"))
        //                {
        //                    if (dsFWBMessage.Tables.Count > 4 && dsFWBMessage.Tables[5].Rows.Count > 0)
        //                    {
        //                        fwbXmlDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows[0]["TransportPaymentMethodCode"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["PayMode"]);
        //                        fwbXmlDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows[0]["ServiceTypeCode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["ShipmentType"]);
        //                    }
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("ApplicableLogisticsAllowanceCharge"))
        //                {
        //                    fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["ChargeCode"]);
        //                    fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["AdditionalID"] = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["OCSubCode"]);
        //                    fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["PrepaidIndicator"] = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["PrepaidIndicator"]);
        //                    fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["LocationTypeCode"] = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["LocationTypeCode"]);
        //                    fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["Reason"] = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["ChargeHeadCode"]);
        //                    fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["PartyTypeCode"] = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["ChargeType"]);
        //                    //fwbXmlDataSet.Tables["ApplicableLogisticsServiceCharge"].Rows[0]["ActualAmount"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["ConvFactor"]);
        //                    if (fwbXmlDataSet.Tables.Contains("ActualAmount"))
        //                    {
        //                        if (fwbXmlDataSet.Tables["ActualAmount"].Columns.Contains("currencyID"))
        //                            fwbXmlDataSet.Tables["ActualAmount"].Rows[0]["currencyID"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]);

        //                        if (fwbXmlDataSet.Tables["ActualAmount"].Columns.Contains("ActualAmount_Text"))
        //                            fwbXmlDataSet.Tables["ActualAmount"].Rows[0]["ActualAmount_Text"] = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["Charge"]);
        //                    }
        //                    fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["TimeBasisQuantity"] = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["ChargeStorageTime"]);
        //                    fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["ItemBasisQuantity"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["PiecesCount"]);
        //                    fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["ServiceDate"] = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["UpdatedOn"]);
        //                    fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["SpecialServiceDescription"] = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["ChargeHeadCode"]);
        //                    fwbXmlDataSet.Tables["ApplicableLogisticsAllowanceCharge"].Rows[0]["SpecialServiceTime"] = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["UpdatedOn"]);

        //                }


        //                //Ratingn Part
        //                if (fwbXmlDataSet.Tables.Contains("ApplicableRating"))
        //                {

        //                    fwbXmlDataSet.Tables["ApplicableRating"].Rows[0]["TypeCode"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["RateType"]);

        //                    if (fwbXmlDataSet.Tables.Contains("TotalChargeAmount"))
        //                    {
        //                        fwbXmlDataSet.Tables["TotalChargeAmount"].Rows[0]["currencyID"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]);
        //                        fwbXmlDataSet.Tables["TotalChargeAmount"].Rows[0]["TotalChargeAmount_Text"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["FrtMKT"]);

        //                    }
        //                    fwbXmlDataSet.Tables["ApplicableRating"].Rows[0]["ConsignmentItemQuantity"] = Convert.ToString(dsFWBMessage.Tables[5].Rows.Count);

        //                    if (fwbXmlDataSet.Tables.Contains("IncludedMasterConsignmentItem"))
        //                    {
        //                        fwbXmlDataSet.Tables["IncludedMasterConsignmentItem"].Rows[0]["SequenceNumeric"] = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["SequenceNumeric"]);//Convert.ToString(dsFWBMessage.Tables[7].Rows.Count);

        //                        if (fwbXmlDataSet.Tables.Contains("TypeCode"))
        //                        {
        //                            fwbXmlDataSet.Tables["TypeCode"].Rows[0]["TypeCode_Text"] = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["CommCode"]);
        //                            fwbXmlDataSet.Tables["TypeCode"].Rows[0]["listAgencyID"] = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["TypeCode_listAgencyID"]);
        //                        }

        //                        if (fwbXmlDataSet.Tables.Contains("GrossWeightMeasure"))
        //                        {
        //                            drs = fwbXmlDataSet.Tables["GrossWeightMeasure"].Select("IncludedMasterConsignmentItem_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["unitCode"] = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["UOM"]);
        //                                drs[0]["GrossWeightMeasure_Text"] = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["GWeight"]);
        //                            }
        //                        }

        //                        if (fwbXmlDataSet.Tables.Contains("IncludedMasterConsignmentItem_GrossVolumeMeasure"))
        //                        {
        //                            drs = fwbXmlDataSet.Tables["IncludedMasterConsignmentItem_GrossVolumeMeasure"].Select("IncludedMasterConsignmentItem_Id=0");
        //                            if (drs.Length > 0)
        //                            {
        //                                drs[0]["unitCode"] = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["UOM"]);
        //                                drs[0]["IncludedMasterConsignmentItem_GrossVolumeMeasure_Text"] = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["GrossVolumeMeasure"]);
        //                            }
        //                        }

        //                        fwbXmlDataSet.Tables["IncludedMasterConsignmentItem"].Rows[0]["PackageQuantity"] = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["Pieces"]);
        //                        fwbXmlDataSet.Tables["IncludedMasterConsignmentItem"].Rows[0]["PieceQuantity"] = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["Pieces"]);
        //                        fwbXmlDataSet.Tables["IncludedMasterConsignmentItem"].Rows[0]["VolumetricFactor"] = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["VolumetricFactor"]); // dsFWBMessage.Tables[7].Rows[0]["Pieces"];
        //                        fwbXmlDataSet.Tables["IncludedMasterConsignmentItem"].Rows[0]["Information"] = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["DimUOM"]);

        //                        if (fwbXmlDataSet.Tables.Contains("NatureIdentificationTransportCargo"))
        //                        {

        //                            fwbXmlDataSet.Tables["NatureIdentificationTransportCargo"].Rows[0]["Identification"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["CommodityDesc"]);
        //                        }
        //                        if (fwbXmlDataSet.Tables.Contains("OriginCountry"))
        //                        {
        //                            fwbXmlDataSet.Tables["OriginCountry"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperCountry"]);
        //                        }
        //                        if (fwbXmlDataSet.Tables.Contains("AssociatedUnitLoadTransportEquipment"))
        //                        {
        //                            fwbXmlDataSet.Tables["AssociatedUnitLoadTransportEquipment"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["ULDSNo"]);
        //                            fwbXmlDataSet.Tables["AssociatedUnitLoadTransportEquipment"].Rows[0]["CharacteristicCode"] = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["ULDType"]);
        //                            fwbXmlDataSet.Tables["AssociatedUnitLoadTransportEquipment"].Rows[0]["LoadedPackageQuantity"] = Convert.ToString(dsFWBMessage.Tables[9].Rows[0]["SLAC"]);

        //                            if (fwbXmlDataSet.Tables.Contains("TareWeightMeasure"))
        //                            {
        //                                if (fwbXmlDataSet.Tables["TareWeightMeasure"].Columns.Contains("unitCode"))
        //                                    fwbXmlDataSet.Tables["TareWeightMeasure"].Rows[0]["unitCode"] = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["UnitCode"]);

        //                                if (fwbXmlDataSet.Tables["TareWeightMeasure"].Columns.Contains("TareWeightMeasure_Text"))
        //                                    fwbXmlDataSet.Tables["TareWeightMeasure"].Rows[0]["TareWeightMeasure_Text"] = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["TareWeight"]);
        //                            }
        //                            //OperatingParty
        //                            if (fwbXmlDataSet.Tables.Contains("PrimaryID"))
        //                            {
        //                                drs = fwbXmlDataSet.Tables["PrimaryID"].Select("OperatingParty_Id=0");
        //                                if (drs.Length > 0)
        //                                {
        //                                    drs[0]["schemeAgencyID"] = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["ULDSNo"]);
        //                                    drs[0]["PrimaryID_Text"] = Convert.ToString(dsFWBMessage.Tables[3].Rows[0]["PartnerCode"]);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }

        //                fwbXmlDataSet.Tables["TransportLogisticsPackage"].Rows[0]["ItemQuantity"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["PiecesCount"]);

        //                if (fwbXmlDataSet.Tables.Contains("GrossWeightMeasure"))
        //                {
        //                    drs = fwbXmlDataSet.Tables["GrossWeightMeasure"].Select("TransportLogisticsPackage_Id=0");
        //                    if (drs.Length > 0)
        //                    {
        //                        drs[0]["unitCode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["UOM"]);
        //                        drs[0]["GrossWeightMeasure_Text"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["GrossWeight"]);
        //                    }
        //                }

        //                //LinearSpatialDim
        //                if (fwbXmlDataSet.Tables.Contains("WidthMeasure"))
        //                {
        //                    if (fwbXmlDataSet.Tables["WidthMeasure"].Columns.Contains("unitCode"))
        //                        fwbXmlDataSet.Tables["WidthMeasure"].Rows[0]["unitCode"] = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["MeasureUnit"]);

        //                    if (fwbXmlDataSet.Tables["WidthMeasure"].Columns.Contains("WidthMeasure_Text"))
        //                        fwbXmlDataSet.Tables["WidthMeasure"].Rows[0]["WidthMeasure_Text"] = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["Width"]);
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("LengthMeasure"))
        //                {
        //                    if (fwbXmlDataSet.Tables["LengthMeasure"].Columns.Contains("unitCode"))
        //                        fwbXmlDataSet.Tables["LengthMeasure"].Rows[0]["unitCode"] = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["MeasureUnit"]);

        //                    if (fwbXmlDataSet.Tables["LengthMeasure"].Columns.Contains("LengthMeasure_Text"))
        //                        fwbXmlDataSet.Tables["LengthMeasure"].Rows[0]["LengthMeasure_Text"] = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["Length"]);
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("HeightMeasure"))
        //                {
        //                    if (fwbXmlDataSet.Tables["HeightMeasure"].Columns.Contains("unitCode"))
        //                        fwbXmlDataSet.Tables["HeightMeasure"].Rows[0]["unitCode"] = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["MeasureUnit"]);

        //                    if (fwbXmlDataSet.Tables["HeightMeasure"].Columns.Contains("HeightMeasure_Text"))
        //                        fwbXmlDataSet.Tables["HeightMeasure"].Rows[0]["HeightMeasure_Text"] = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["Height"]);
        //                }

        //                fwbXmlDataSet.Tables["ApplicableFreightRateServiceCharge"].Rows[0]["CategoryCode"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["RateClass"]);
        //                fwbXmlDataSet.Tables["ApplicableFreightRateServiceCharge"].Rows[0]["CommodityItemID"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["CommodityCode"]);
        //                fwbXmlDataSet.Tables["ApplicableFreightRateServiceCharge"].Rows[0]["AppliedRate"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["FrtMKT"]);

        //                if (fwbXmlDataSet.Tables.Contains("ChargeableWeightMeasure"))
        //                {
        //                    if (fwbXmlDataSet.Tables["ChargeableWeightMeasure"].Columns.Contains("unitCode"))
        //                        fwbXmlDataSet.Tables["ChargeableWeightMeasure"].Rows[0]["unitCode"] = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["UOM"]);

        //                    if (fwbXmlDataSet.Tables["ChargeableWeightMeasure"].Columns.Contains("ChargeableWeightMeasure_Text"))
        //                        fwbXmlDataSet.Tables["ChargeableWeightMeasure"].Rows[0]["ChargeableWeightMeasure_Text"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["ChargedWeight"]);
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("AppliedAmount"))
        //                {
        //                    if (fwbXmlDataSet.Tables["AppliedAmount"].Columns.Contains("currencyID"))
        //                        fwbXmlDataSet.Tables["AppliedAmount"].Rows[0]["currencyID"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]);

        //                    if (fwbXmlDataSet.Tables["AppliedAmount"].Columns.Contains("AppliedAmount_Text"))
        //                        fwbXmlDataSet.Tables["AppliedAmount"].Rows[0]["AppliedAmount_Text"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["FrtMKT"]);
        //                }

        //                fwbXmlDataSet.Tables["SpecifiedRateCombinationPointLocation"].Rows[0]["ID"] = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["SpecifiedRateCombinationPointLocation_ID"]);

        //                //ApplicableUnitLoadDeviceRateClass
        //                fwbXmlDataSet.Tables["ApplicableUnitLoadDeviceRateClass"].Rows[0]["TypeCode"] = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["Shape"]);
        //                fwbXmlDataSet.Tables["ApplicableUnitLoadDeviceRateClass"].Rows[0]["BasisCode"] = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["MKTRateClass"]);
        //                fwbXmlDataSet.Tables["ApplicableUnitLoadDeviceRateClass"].Rows[0]["AppliedPercent"] = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["Discount"]);
        //                fwbXmlDataSet.Tables["ApplicableUnitLoadDeviceRateClass"].Rows[0]["ReferenceID"] = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["ReferenceID"]);
        //                fwbXmlDataSet.Tables["ApplicableUnitLoadDeviceRateClass"].Rows[0]["ReferenceTypeCode"] = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["ReferenceTypeCode"]);


        //                if (fwbXmlDataSet.Tables.Contains("ApplicableTotalRating"))
        //                {

        //                    fwbXmlDataSet.Tables["ApplicableTotalRating"].Rows[0]["TypeCode"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["RateType"]);

        //                }
        //                if (fwbXmlDataSet.Tables.Contains("CollectAppliedAmount"))
        //                {
        //                    fwbXmlDataSet.Tables["CollectAppliedAmount"].Rows[0]["currencyID"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]);
        //                    fwbXmlDataSet.Tables["CollectAppliedAmount"].Rows[0]["CollectAppliedAmount_Text"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["FrtMKT"]);

        //                }
        //                if (fwbXmlDataSet.Tables.Contains("DestinationAppliedAmount"))
        //                {
        //                    fwbXmlDataSet.Tables["DestinationAppliedAmount"].Rows[0]["currencyID"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]);
        //                    fwbXmlDataSet.Tables["DestinationAppliedAmount"].Rows[0]["DestinationAppliedAmount_Text"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["OtherCharges"]);

        //                }

        //                if (fwbXmlDataSet.Tables.Contains("TotalAppliedAmount"))
        //                {
        //                    fwbXmlDataSet.Tables["TotalAppliedAmount"].Rows[0]["currencyID"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]);
        //                    fwbXmlDataSet.Tables["TotalAppliedAmount"].Rows[0]["TotalAppliedAmount_Text"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Total"]);

        //                }
        //                if (fwbXmlDataSet.Tables.Contains("ApplicablePrepaidCollectMonetarySummation"))
        //                {
        //                    fwbXmlDataSet.Tables["ApplicablePrepaidCollectMonetarySummation"].Rows[0]["PrepaidIndicator"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["PrepaidIndicator"]);
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("WeightChargeTotalAmount"))
        //                {
        //                    fwbXmlDataSet.Tables["WeightChargeTotalAmount"].Rows[0]["currencyID"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]);
        //                    fwbXmlDataSet.Tables["WeightChargeTotalAmount"].Rows[0]["WeightChargeTotalAmount_Text"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["FrtMKT"]);

        //                }
        //                if (fwbXmlDataSet.Tables.Contains("ValuationChargeTotalAmount"))
        //                {
        //                    if (dsFWBMessage.Tables.Count > 5 && dsFWBMessage.Tables[6].Rows.Count > 0)
        //                    {
        //                        fwbXmlDataSet.Tables["ValuationChargeTotalAmount"].Rows[0]["currencyID"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]);
        //                        fwbXmlDataSet.Tables["ValuationChargeTotalAmount"].Rows[0]["ValuationChargeTotalAmount_Text"] = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["Charge"]);
        //                    }
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("TaxTotalAmount"))
        //                {
        //                    if (dsFWBMessage.Tables.Count > 4 && dsFWBMessage.Tables[5].Rows.Count > 0)
        //                    {
        //                        fwbXmlDataSet.Tables["TaxTotalAmount"].Rows[0]["currencyID"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]);
        //                        fwbXmlDataSet.Tables["TaxTotalAmount"].Rows[0]["TaxTotalAmount_Text"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["TAX"]);
        //                    }
        //                }
        //                if (fwbXmlDataSet.Tables.Contains("AgentTotalDuePayableAmount"))
        //                {
        //                    if (dsFWBMessage.Tables.Count > 4 && dsFWBMessage.Tables[5].Rows.Count > 0)
        //                    {
        //                        fwbXmlDataSet.Tables["AgentTotalDuePayableAmount"].Rows[0]["currencyID"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]);
        //                        fwbXmlDataSet.Tables["AgentTotalDuePayableAmount"].Rows[0]["AgentTotalDuePayableAmount_Text"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["OCDueAgent"]);
        //                    }
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("CarrierTotalDuePayableAmount"))
        //                {
        //                    if (dsFWBMessage.Tables.Count > 4 && dsFWBMessage.Tables[5].Rows.Count > 0)
        //                    {
        //                        fwbXmlDataSet.Tables["CarrierTotalDuePayableAmount"].Rows[0]["currencyID"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]);
        //                        fwbXmlDataSet.Tables["CarrierTotalDuePayableAmount"].Rows[0]["CarrierTotalDuePayableAmount_Text"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["OCDueCar"]);
        //                    }
        //                }

        //                if (fwbXmlDataSet.Tables.Contains("GrandTotalAmount"))
        //                {
        //                    if (dsFWBMessage.Tables.Count > 4 && dsFWBMessage.Tables[5].Rows.Count > 0)
        //                    {
        //                        fwbXmlDataSet.Tables["GrandTotalAmount"].Rows[0]["currencyID"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]);
        //                        fwbXmlDataSet.Tables["GrandTotalAmount"].Rows[0]["GrandTotalAmount_Text"] = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Total"]);
        //                    }
        //                }

        //                string generateMessage1 = fwbXmlDataSet.GetXml();
        //                generateMessage = new StringBuilder(generateMessage1);
        //                generateMessage.Replace(" xmlns: ram = \"iata: datamodel:3\"", "");
        //                generateMessage.Replace(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
        //                generateMessage.Replace(" xmlns:rsm=\"iata: waybill:1\"", "");
        //                generateMessage.Replace(" xmlns:ram=\"iata: datamodel:3\"", "");
        //                generateMessage.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
        //                generateMessage.Replace(" xsi:schemaLocation=\"iata: waybill:1 Waybill_1.xsd\"", "");
        //                generateMessage.Replace(" xmlns: ram = \"iata: datamodel:3\"", "");
        //                generateMessage.Replace("xmlns: ram = \"iata: datamodel:3\"", "");
        //                generateMessage.Replace(" xmlns:rsm=\"iata:waybill:1\"", "");
        //                generateMessage.Replace(" xmlns:ram=\"iata:datamodel:3\"", "");

        //                //Replacing duplicate Nodes
        //                generateMessage.Replace("ram:ConsignorParty_PostalStructuredAddress", "ram:PostalStructuredAddress");
        //                generateMessage.Replace("ram:ConsigneeParty_PostalStructuredAddress", "ram:PostalStructuredAddress");
        //                generateMessage.Replace("ram:AssociatedParty_PostalStructuredAddress", "ram:PostalStructuredAddress");
        //                generateMessage.Replace("ram:IncludedMasterConsignmentItem_GrossVolumeMeasure", "ram:GrossVolumeMeasure");


        //                generateMessage.Insert(12, " xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:rsm=\"iata:waybill:1\" xmlns:ram=\"iata:datamodel:3\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"iata:waybill:1 Waybill_1.xsd\"");
        //                generateMessage.Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
        //                //XMLValidator objxMLValidator = new XMLValidator();

        //                /////Remove the empty tags from XML
        //                //var document = System.Xml.Linq.XDocument.Parse(generateMessage.ToString());
        //                //var emptyNodes = document.Descendants().Where(e => e.IsEmpty || String.IsNullOrWhiteSpace(e.Value));
        //                //foreach (var emptyNode in emptyNodes.ToArray())
        //                //{
        //                //    emptyNode.Remove();
        //                //}
        //                //generateMessage = new StringBuilder(document.ToString());

        //                //string errormsg = objxMLValidator.CTeXMLValidator(generateMessage.ToString());
        //                //if (errormsg.Length > 1)
        //                //{
        //                //    generateMessage.Clear();
        //                //    generateMessage.Append(errormsg);
        //                //}
        //                fwbXmlDataSet.Dispose();

        //            }
        //            else
        //            {
        //                generateMessage.Append("No Message format available in the system.");
        //            }

        //        }

        //        else
        //        {
        //            generateMessage.Append("No Data available in the system to generate message.");
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure("Error on Generate XFWB Message Method:" + ex.ToString());

        //    }
        //    generateMessage.Replace("PostalStructuredAddress1", "PostalStructuredAddress");
        //    return Convert.ToString(generateMessage);
        //}

        private string ReplacingNodeNames(string xmlMsg)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlMsg);
                XmlElement root = doc.DocumentElement;
                if (root != null && root.Name.Equals("rsm:Waybill"))
                {
                    XmlNodeList xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:PostalStructuredAddress')]");
                    if (xmlNodelst != null)
                    {
                        foreach (XmlNode xmlNode in xmlNodelst)
                        {
                            if (xmlNode.ParentNode != null && xmlNode.ParentNode.Name.Equals("ram:ConsignorParty"))
                            {
                                RenameNode(xmlNode, "ram:ConsignorParty_PostalStructuredAddress");
                            }
                            else if (xmlNode.ParentNode != null && xmlNode.ParentNode.Name.Equals("ram:ConsigneeParty"))
                            {
                                RenameNode(xmlNode, "ram:ConsigneeParty_PostalStructuredAddress");
                            }
                            else if (xmlNode.ParentNode != null && xmlNode.ParentNode.Name.Equals("ram:AssociatedParty"))
                            {
                                RenameNode(xmlNode, "ram:AssociatedParty_PostalStructuredAddress");
                            }
                        }
                    }
    
                    xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:GrossVolumeMeasure')]");
                    if (xmlNodelst != null)
                    {
                        foreach (XmlNode xmlNode in xmlNodelst)
                        {
                            if (xmlNode.ParentNode != null && xmlNode.ParentNode.Name.Equals("ram:IncludedMasterConsignmentItem"))
                            {
                                RenameNode(xmlNode, "ram:IncludedMasterConsignmentItem_GrossVolumeMeasure");
                            }
                        }
                    }
    
                    xmlMsg = doc.OuterXml;
                    xmlMsg = xmlMsg.Replace("ConsignorParty_PostalStructuredAddress", "ram:ConsignorParty_PostalStructuredAddress");
                    xmlMsg = xmlMsg.Replace("ConsigneeParty_PostalStructuredAddress", "ram:ConsigneeParty_PostalStructuredAddress");
                    xmlMsg = xmlMsg.Replace("AssociatedParty_PostalStructuredAddress", "ram:AssociatedParty_PostalStructuredAddress");
                    xmlMsg = xmlMsg.Replace("IncludedMasterConsignmentItem_GrossVolumeMeasure", "ram:IncludedMasterConsignmentItem_GrossVolumeMeasure");
                }
                return xmlMsg;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        public async Task<string> GenerateXFWBMessageV3(string awbPrefix, string awbNumber, string customsName)
        {
            StringBuilder generateMessage = new StringBuilder();
            DataSet? dsFWBMessage = await GetRecordforAWBToGenerateXFWBMessage(awbPrefix, awbNumber);

            //GenericFunction generalfunction = new GenericFunction();
            try
            {
                if (dsFWBMessage != null && dsFWBMessage.Tables.Count > 0 && dsFWBMessage.Tables[0].Rows.Count > 0 && dsFWBMessage.Tables[1].Rows.Count > 0 && dsFWBMessage.Tables[3].Rows.Count > 0 && dsFWBMessage.Tables[5].Rows.Count > 0)
                {

                    XmlDocument xmlXFWBV3 = new XmlDocument();
                    xmlXFWBV3.XmlResolver = null;
                    XmlDocument xmlxFWBCdata = new XmlDocument();
                    xmlxFWBCdata.XmlResolver = null;

                    XmlSchema schema = new XmlSchema();
                    schema.Namespaces.Add("xsd", "http://www.w3.org/2001/XMLSchema");
                    schema.Namespaces.Add("rsm", "iata:waybill:1");
                    schema.Namespaces.Add("ccts", "urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2");
                    schema.Namespaces.Add("udt", "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8");
                    schema.Namespaces.Add("ram", "iata:datamodel:3");
                    schema.Namespaces.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    //schema.Namespaces.Add("schemaLocation", "iata:waybill:1 Waybill_1.xsd");
                    xmlXFWBV3.Schemas.Add(schema);

                    XmlElement Waybill = xmlXFWBV3.CreateElement("rsm:Waybill");
                    Waybill.SetAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");
                    Waybill.SetAttribute("xmlns:rsm", "iata:waybill:1");
                    Waybill.SetAttribute("xmlns:ccts", "urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2");
                    Waybill.SetAttribute("xmlns:udt", "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8");
                    Waybill.SetAttribute("xmlns:ram", "iata:datamodel:3");
                    Waybill.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    //Waybill.SetAttribute("xsi:schemaLocation", "iata:waybill:1 Waybill_1.xsd");
                    Waybill.SetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "iata:waybill:1 Waybill_1.xsd");
                    xmlXFWBV3.AppendChild(Waybill);
                    #region MessageHeaderDocument

                    XmlElement MessageHeaderDocument = xmlXFWBV3.CreateElement("rsm:MessageHeaderDocument");
                    Waybill.AppendChild(MessageHeaderDocument);

                    XmlElement MessageHeaderDocument_ID = xmlXFWBV3.CreateElement("ram:ID");
                    MessageHeaderDocument_ID.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["AWBNumber"]);
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_ID);

                    XmlElement MessageHeaderDocument_Name = xmlXFWBV3.CreateElement("ram:Name");
                    MessageHeaderDocument_Name.InnerText = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["NAME"]);
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_Name);

                    XmlElement MessageHeaderDocument_TypeCode = xmlXFWBV3.CreateElement("ram:TypeCode");
                    MessageHeaderDocument_TypeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["TypeCode"]);
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_TypeCode);

                    XmlElement MessageHeaderDocument_IssueDateTime = xmlXFWBV3.CreateElement("ram:IssueDateTime");
                    MessageHeaderDocument_IssueDateTime.InnerText = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["IssueDateTime"]);
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_IssueDateTime);

                    XmlElement MessageHeaderDocument_PurposeCode = xmlXFWBV3.CreateElement("ram:PurposeCode");
                    MessageHeaderDocument_PurposeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["PurposeCode"]);
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_PurposeCode);

                    XmlElement MessageHeaderDocument_VersionID = xmlXFWBV3.CreateElement("ram:VersionID");
                    MessageHeaderDocument_VersionID.InnerText = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["VersionID"]);
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_VersionID);

                    XmlElement MessageHeaderDocument_ConversationID = xmlXFWBV3.CreateElement("ram:ConversationID");
                    MessageHeaderDocument_ConversationID.InnerText = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["ConversationID"]);
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_ConversationID);

                    XmlElement MessageHeaderDocument_SenderParty = xmlXFWBV3.CreateElement("ram:SenderParty");
                    //MessageHeaderDocument_SenderParty.InnerText = "";
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_SenderParty);

                    XmlElement MessageHeaderDocument_SenderParty_PrimaryID = xmlXFWBV3.CreateElement("ram:PrimaryID");
                    MessageHeaderDocument_SenderParty_PrimaryID.SetAttribute("schemeID", Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["SenderParty_PrimaryID"]));
                    MessageHeaderDocument_SenderParty_PrimaryID.InnerText = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["SenderParty_PrimaryIDText"]);
                    MessageHeaderDocument_SenderParty.AppendChild(MessageHeaderDocument_SenderParty_PrimaryID);

                    //XmlElement MessageHeaderDocument_SenderParty_SenderQualifier = xmlXFWBV3.CreateElement("ram:SenderQualifier");
                    //MessageHeaderDocument_SenderParty_SenderQualifier.InnerText = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["SenderQualifier"]);
                    //MessageHeaderDocument_SenderParty.AppendChild(MessageHeaderDocument_SenderParty_SenderQualifier);

                    //XmlElement MessageHeaderDocument_SenderParty_SenderIdentification = xmlXFWBV3.CreateElement("ram:SenderIdentification");
                    //MessageHeaderDocument_SenderParty_SenderIdentification.InnerText = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["SenderIdentification"]);
                    //MessageHeaderDocument_SenderParty.AppendChild(MessageHeaderDocument_SenderParty_SenderIdentification);

                    XmlElement MessageHeaderDocument_RecipientParty = xmlXFWBV3.CreateElement("ram:RecipientParty");
                    //MessageHeaderDocument_RecipientParty.InnerText = "";
                    MessageHeaderDocument.AppendChild(MessageHeaderDocument_RecipientParty);

                    XmlElement MessageHeaderDocument_RecipientParty_PrimaryID = xmlXFWBV3.CreateElement("ram:PrimaryID");
                    MessageHeaderDocument_RecipientParty_PrimaryID.SetAttribute("schemeID", Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["RecipientParty_PrimaryID"]));
                    MessageHeaderDocument_RecipientParty_PrimaryID.InnerText = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["RecipientParty_PrimaryIDText"]);
                    MessageHeaderDocument_RecipientParty.AppendChild(MessageHeaderDocument_RecipientParty_PrimaryID);

                    //XmlElement MessageHeaderDocument_RecipientParty_RecipientQualifier = xmlXFWBV3.CreateElement("ram:RecipientQualifier");
                    //MessageHeaderDocument_RecipientParty_RecipientQualifier.InnerText = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["RecipientQualifier"]);
                    //MessageHeaderDocument_RecipientParty.AppendChild(MessageHeaderDocument_RecipientParty_RecipientQualifier);

                    //XmlElement MessageHeaderDocument_RecipientParty_RecipientIdentification = xmlXFWBV3.CreateElement("ram:RecipientIdentification");
                    //MessageHeaderDocument_RecipientParty_RecipientIdentification.InnerText = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["RecipientIdentification"]);
                    //MessageHeaderDocument_RecipientParty.AppendChild(MessageHeaderDocument_RecipientParty_RecipientIdentification);

                    #endregion MessageHeaderDocument

                    #region BusinessHeaderDocument

                    XmlElement BusinessHeaderDocument = xmlXFWBV3.CreateElement("rsm:BusinessHeaderDocument");
                    Waybill.AppendChild(BusinessHeaderDocument);

                    XmlElement BusinessHeaderDocument_ID = xmlXFWBV3.CreateElement("ram:ID");
                    BusinessHeaderDocument_ID.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["AWBNumber"]);
                    BusinessHeaderDocument.AppendChild(BusinessHeaderDocument_ID);

                    XmlElement BusinessHeaderDocument_SenderAssignedID = xmlXFWBV3.CreateElement("ram:SenderAssignedID");
                    BusinessHeaderDocument_SenderAssignedID.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["BHD_SenderAssignedID"]);
                    BusinessHeaderDocument.AppendChild(BusinessHeaderDocument_SenderAssignedID);

                    XmlElement BusinessHeaderDocument_IncludedHeaderNote = xmlXFWBV3.CreateElement("ram:IncludedHeaderNote");
                    //BusinessHeaderDocument_BusinessHeaderDocument_IncludedHeaderNote.InnerText = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["ID"]);
                    BusinessHeaderDocument.AppendChild(BusinessHeaderDocument_IncludedHeaderNote);

                    XmlElement BusinessHeaderDocument_IncludedHeaderNote_ContentCode = xmlXFWBV3.CreateElement("ram:ContentCode");
                    BusinessHeaderDocument_IncludedHeaderNote_ContentCode.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["HeaderNote"]);
                    BusinessHeaderDocument_IncludedHeaderNote.AppendChild(BusinessHeaderDocument_IncludedHeaderNote_ContentCode);

                    XmlElement BusinessHeaderDocument_IncludedHeaderNote_Content = xmlXFWBV3.CreateElement("ram:Content");
                    BusinessHeaderDocument_IncludedHeaderNote_Content.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["HeaderText"]);
                    //XmlCDataSection cdata_BusinessHeaderDocument_IncludedHeaderNote_Content = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["HeaderText"]));
                    //BusinessHeaderDocument_IncludedHeaderNote_Content.AppendChild(cdata_BusinessHeaderDocument_IncludedHeaderNote_Content);
                    BusinessHeaderDocument_IncludedHeaderNote.AppendChild(BusinessHeaderDocument_IncludedHeaderNote_Content);

                    XmlElement BusinessHeaderDocument_SignatoryConsignorAuthentication = xmlXFWBV3.CreateElement("ram:SignatoryConsignorAuthentication");
                    //BusinessHeaderDocument_SignatoryConsignorAuthentication.InnerText = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["ID"]);
                    BusinessHeaderDocument.AppendChild(BusinessHeaderDocument_SignatoryConsignorAuthentication);

                    XmlElement BHDoc_SignatoryConsignorAuthentication_Signatory = xmlXFWBV3.CreateElement("ram:Signatory");
                    //XmlCDataSection cdata_BHDoc_SignatoryConsignorAuthentication_Signatory = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperName"]));
                    //BHDoc_SignatoryConsignorAuthentication_Signatory.AppendChild(cdata_BHDoc_SignatoryConsignorAuthentication_Signatory);
                    BHDoc_SignatoryConsignorAuthentication_Signatory.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperName"]);
                    BusinessHeaderDocument_SignatoryConsignorAuthentication.AppendChild(BHDoc_SignatoryConsignorAuthentication_Signatory);

                    XmlElement BHDoc_SignatoryCarrierAuthentication = xmlXFWBV3.CreateElement("ram:SignatoryCarrierAuthentication");
                    //BusinessHeaderDocument_SignatoryConsignorAuthentication.InnerText = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["ID"]);
                    BusinessHeaderDocument.AppendChild(BHDoc_SignatoryCarrierAuthentication);

                    XmlElement BHDoc_SignatoryCarrierAuthentication_ActualDateTime = xmlXFWBV3.CreateElement("ram:ActualDateTime");
                    BHDoc_SignatoryCarrierAuthentication_ActualDateTime.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["CarrierDeclarationDate"]);
                    BHDoc_SignatoryCarrierAuthentication.AppendChild(BHDoc_SignatoryCarrierAuthentication_ActualDateTime);

                    XmlElement BHDoc_SignatoryCarrierAuthentication_Signatory = xmlXFWBV3.CreateElement("ram:Signatory");
                    //XmlCDataSection cdata_BHDoc_SignatoryCarrierAuthentication_Signatory = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["CarrierDeclarationSignature"]));
                    //BHDoc_SignatoryCarrierAuthentication_Signatory.AppendChild(cdata_BHDoc_SignatoryCarrierAuthentication_Signatory);
                    BHDoc_SignatoryCarrierAuthentication_Signatory.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["CarrierDeclarationSignature"]);
                    BHDoc_SignatoryCarrierAuthentication.AppendChild(BHDoc_SignatoryCarrierAuthentication_Signatory);

                    XmlElement BHDoc_SignatoryCarrierAuth_IssueAuthenticationLocation = xmlXFWBV3.CreateElement("ram:IssueAuthenticationLocation");
                    //BusinessHeaderDocument_SignatoryConsignorAuthentication.InnerText = Convert.ToString(dsFWBMessage.Tables[8].Rows[0]["ID"]);
                    BHDoc_SignatoryCarrierAuthentication.AppendChild(BHDoc_SignatoryCarrierAuth_IssueAuthenticationLocation);

                    XmlElement BHDoc_SignatoryCarrierAuth_IssueAuthenticationLocation_Name = xmlXFWBV3.CreateElement("ram:Name");
                    BHDoc_SignatoryCarrierAuth_IssueAuthenticationLocation_Name.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["CarrierDeclarationPlace"]);
                    BHDoc_SignatoryCarrierAuth_IssueAuthenticationLocation.AppendChild(BHDoc_SignatoryCarrierAuth_IssueAuthenticationLocation_Name);

                    #endregion BusinessHeaderDocument

                    #region MasterConsignment

                    XmlElement MasterConsignment = xmlXFWBV3.CreateElement("rsm:MasterConsignment");
                    Waybill.AppendChild(MasterConsignment);

                    XmlElement MasterConsignment_ID = xmlXFWBV3.CreateElement("ram:ID");
                    MasterConsignment_ID.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["MasterConsignment_ID"]);
                    MasterConsignment.AppendChild(MasterConsignment_ID);

                    XmlElement MC_AdditionalID = xmlXFWBV3.CreateElement("ram:AdditionalID");
                    MC_AdditionalID.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["MasterConsignment_AdditionalID"]);
                    MasterConsignment.AppendChild(MC_AdditionalID);

                    XmlElement MC_FreightForwarderAssignedID = xmlXFWBV3.CreateElement("ram:FreightForwarderAssignedID");
                    MC_FreightForwarderAssignedID.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["FreightForwarderAssignedID"]);
                    MasterConsignment.AppendChild(MC_FreightForwarderAssignedID);

                    XmlElement MC_AssociatedReferenceID = xmlXFWBV3.CreateElement("ram:AssociatedReferenceID");
                    MC_AssociatedReferenceID.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["AssociatedReferenceID"]);
                    MasterConsignment.AppendChild(MC_AssociatedReferenceID);

                    XmlElement MC_NilCarriageValueIndicator = xmlXFWBV3.CreateElement("ram:NilCarriageValueIndicator");
                    MC_NilCarriageValueIndicator.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["NilCarriageValueIndicator"]);
                    MasterConsignment.AppendChild(MC_NilCarriageValueIndicator);

                    XmlElement MC_DeclaredValueForCarriageAmount = xmlXFWBV3.CreateElement("ram:DeclaredValueForCarriageAmount");
                    MC_DeclaredValueForCarriageAmount.SetAttribute("currencyID", Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["DVCarriageCurrency"]));
                    MC_DeclaredValueForCarriageAmount.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["DVForCarriage"]);
                    MasterConsignment.AppendChild(MC_DeclaredValueForCarriageAmount);

                    XmlElement MC_NilCustomsValueIndicator = xmlXFWBV3.CreateElement("ram:NilCustomsValueIndicator");
                    MC_NilCustomsValueIndicator.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["NilCustomsValueIndicator"]);
                    MasterConsignment.AppendChild(MC_NilCustomsValueIndicator);

                    XmlElement MC_DeclaredValueForCustomsAmount = xmlXFWBV3.CreateElement("ram:DeclaredValueForCustomsAmount");
                    MC_DeclaredValueForCustomsAmount.SetAttribute("currencyID", Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["DVCustomCurrency"]));
                    MC_DeclaredValueForCustomsAmount.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["DVForCustom"]);
                    MasterConsignment.AppendChild(MC_DeclaredValueForCustomsAmount);

                    XmlElement MC_NilInsuranceValueIndicator = xmlXFWBV3.CreateElement("ram:NilInsuranceValueIndicator");
                    MC_NilInsuranceValueIndicator.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["NilInsuranceValueIndicator"]);
                    MasterConsignment.AppendChild(MC_NilInsuranceValueIndicator);

                    XmlElement MC_InsuranceValueAmount = xmlXFWBV3.CreateElement("ram:InsuranceValueAmount");
                    MC_InsuranceValueAmount.SetAttribute("currencyID", Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["InsuranceCurrency"]));
                    MC_InsuranceValueAmount.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["InsuranceAmount"]);
                    MasterConsignment.AppendChild(MC_InsuranceValueAmount);

                    XmlElement MC_TotalChargePrepaidIndicator = xmlXFWBV3.CreateElement("ram:TotalChargePrepaidIndicator");
                    MC_TotalChargePrepaidIndicator.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["TotalChargePrepaidIndicator"]);
                    MasterConsignment.AppendChild(MC_TotalChargePrepaidIndicator);

                    XmlElement MC_TotalDisbursementPrepaidIndicator = xmlXFWBV3.CreateElement("ram:TotalDisbursementPrepaidIndicator");
                    MC_TotalDisbursementPrepaidIndicator.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["TotalDisbursementPrepaidIndicator"]);
                    MasterConsignment.AppendChild(MC_TotalDisbursementPrepaidIndicator);

                    XmlElement MC_IncludedTareGrossWeightMeasure = xmlXFWBV3.CreateElement("ram:IncludedTareGrossWeightMeasure");
                    MC_IncludedTareGrossWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["UOM"]));
                    MC_IncludedTareGrossWeightMeasure.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["GrossWeight"]);
                    MasterConsignment.AppendChild(MC_IncludedTareGrossWeightMeasure);
                    double Numberofposition = Convert.ToDouble(dsFWBMessage.Tables[0].Rows[0]["NoOfPosition"].ToString() != "" ? dsFWBMessage.Tables[0].Rows[0]["NoOfPosition"].ToString() : "0");
                    if (Numberofposition <= 0)
                    {
                        XmlElement MC_GrossVolumeMeasure = xmlXFWBV3.CreateElement("ram:GrossVolumeMeasure");
                        MC_GrossVolumeMeasure.SetAttribute("unitCode", Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["UOM"]));
                        MC_GrossVolumeMeasure.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["VolumetricWeight"]);
                        MasterConsignment.AppendChild(MC_GrossVolumeMeasure);
                    }
                    else
                    {
                        XmlElement MC_GrossVolumeMeasure = xmlXFWBV3.CreateElement("ram:GrossVolumeMeasure");
                        MC_GrossVolumeMeasure.SetAttribute("unitCode", Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["NoOfPosition_text"]));
                        MC_GrossVolumeMeasure.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["NoOfPosition"]);
                        MasterConsignment.AppendChild(MC_GrossVolumeMeasure);
                    }
                    XmlElement MC_DensityGroupCode = xmlXFWBV3.CreateElement("ram:DensityGroupCode");
                    MC_DensityGroupCode.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["DensityGroup"]);
                    MasterConsignment.AppendChild(MC_DensityGroupCode);

                    XmlElement MC_PackageQuantity = xmlXFWBV3.CreateElement("ram:PackageQuantity");
                    MC_PackageQuantity.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["PiecesCount"]);
                    MasterConsignment.AppendChild(MC_PackageQuantity);

                    XmlElement MC_TotalPieceQuantity = xmlXFWBV3.CreateElement("ram:TotalPieceQuantity");
                    MC_TotalPieceQuantity.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["PiecesCount"]);
                    MasterConsignment.AppendChild(MC_TotalPieceQuantity);

                    XmlElement MC_ProductID = xmlXFWBV3.CreateElement("ram:ProductID");
                    MC_ProductID.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["ProductType"]);
                    MasterConsignment.AppendChild(MC_ProductID);

                    #region ConsignorParty

                    XmlElement ConsignorParty = xmlXFWBV3.CreateElement("ram:ConsignorParty");
                    MasterConsignment.AppendChild(ConsignorParty);

                    XmlElement ConrP_PrimaryID = xmlXFWBV3.CreateElement("ram:PrimaryID");
                    ConrP_PrimaryID.SetAttribute("schemeAgencyID", "1");
                    ConrP_PrimaryID.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["ConsigneePrimaryID"]);
                    ConsignorParty.AppendChild(ConrP_PrimaryID);

                    XmlElement ConrP_AdditionalID = xmlXFWBV3.CreateElement("ram:AdditionalID");
                    ConrP_AdditionalID.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperStandardID"]);
                    ConsignorParty.AppendChild(ConrP_AdditionalID);

                    XmlElement ConrP_Name = xmlXFWBV3.CreateElement("ram:Name");
                    //XmlCDataSection cdata_ConrP_Name = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperName"]));
                    //ConrP_Name.AppendChild(cdata_ConrP_Name);
                    ConrP_Name.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperName"]);
                    ConsignorParty.AppendChild(ConrP_Name);

                    XmlElement ConrP_AccountID = xmlXFWBV3.CreateElement("ram:AccountID");
                    ConrP_AccountID.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperAccCode"]);
                    ConsignorParty.AppendChild(ConrP_AccountID);

                    XmlElement ConrP_PostalStructuredAddress = xmlXFWBV3.CreateElement("ram:PostalStructuredAddress");
                    ConsignorParty.AppendChild(ConrP_PostalStructuredAddress);

                    XmlElement ConrP_PostcodeCode = xmlXFWBV3.CreateElement("ram:PostcodeCode");
                    ConrP_PostcodeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperPincode"]);
                    ConrP_PostalStructuredAddress.AppendChild(ConrP_PostcodeCode);

                    XmlElement ConrP_StreetName = xmlXFWBV3.CreateElement("ram:StreetName");
                    //XmlCDataSection cdata_ConrP_StreetName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperAddress"]));
                    //ConrP_StreetName.AppendChild(cdata_ConrP_StreetName);
                    ConrP_StreetName.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperAddress"]);
                    ConrP_PostalStructuredAddress.AppendChild(ConrP_StreetName);

                    XmlElement ConrP_CityName = xmlXFWBV3.CreateElement("ram:CityName");
                    //ConrP_CityName.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperCity"]);
                    //XmlCDataSection cdata_ConrP_CityName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["Shippercityname"]));
                    //ConrP_CityName.AppendChild(cdata_ConrP_CityName);
                    ConrP_CityName.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["Shippercityname"]);
                    ConrP_PostalStructuredAddress.AppendChild(ConrP_CityName);

                    XmlElement ConrP_CountryID = xmlXFWBV3.CreateElement("ram:CountryID");
                    ConrP_CountryID.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperCountry"]);
                    ConrP_PostalStructuredAddress.AppendChild(ConrP_CountryID);

                    XmlElement ConrP_CountryName = xmlXFWBV3.CreateElement("ram:CountryName");
                    //XmlCDataSection cdata_ConrP_CountryName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperRegionName"]));
                    //ConrP_CountryName.AppendChild(cdata_ConrP_CountryName);
                    ConrP_CountryName.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperRegionName"]);
                    ConrP_PostalStructuredAddress.AppendChild(ConrP_CountryName);

                    XmlElement ConrP_CountrySubDivisionName = xmlXFWBV3.CreateElement("ram:CountrySubDivisionName");
                    //XmlCDataSection cdata_ConrP_CountrySubDivisionName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperRegionName"]));
                    //ConrP_CountrySubDivisionName.AppendChild(cdata_ConrP_CountrySubDivisionName);
                    ConrP_CountrySubDivisionName.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperRegionName"]);
                    ConrP_PostalStructuredAddress.AppendChild(ConrP_CountrySubDivisionName);

                    XmlElement ConrP_PostOfficeBox = xmlXFWBV3.CreateElement("ram:PostOfficeBox");
                    ConrP_PostOfficeBox.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperPOBox"]);
                    ConrP_PostalStructuredAddress.AppendChild(ConrP_PostOfficeBox);

                    XmlElement ConrP_CityID = xmlXFWBV3.CreateElement("ram:CityID");
                    ConrP_CityID.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperCity"]);
                    ConrP_PostalStructuredAddress.AppendChild(ConrP_CityID);

                    XmlElement ConrP_CountrySubDivisionID = xmlXFWBV3.CreateElement("ram:CountrySubDivisionID");
                    ConrP_CountrySubDivisionID.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperRegionName"]);
                    ConrP_PostalStructuredAddress.AppendChild(ConrP_CountrySubDivisionID);


                    XmlElement ConrP_DefinedTradeContact = xmlXFWBV3.CreateElement("ram:DefinedTradeContact");
                    ConsignorParty.AppendChild(ConrP_DefinedTradeContact);

                    XmlElement ConrP_PersonName = xmlXFWBV3.CreateElement("ram:PersonName");
                    //XmlCDataSection cdata_ConrP_PersonName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperName"]));
                    //ConrP_PersonName.AppendChild(cdata_ConrP_PersonName);
                    ConrP_PersonName.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperName"]);
                    ConrP_DefinedTradeContact.AppendChild(ConrP_PersonName);

                    XmlElement ConrP_DepartmentName = xmlXFWBV3.CreateElement("ram:DepartmentName");
                    ConrP_DepartmentName.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperDepartnmentName"]);
                    ConrP_DefinedTradeContact.AppendChild(ConrP_DepartmentName);

                    XmlElement ConrP_DirectTelephoneCommunication = xmlXFWBV3.CreateElement("ram:DirectTelephoneCommunication");
                    ConrP_DefinedTradeContact.AppendChild(ConrP_DirectTelephoneCommunication);

                    XmlElement ConrP_TCompleteNumber = xmlXFWBV3.CreateElement("ram:CompleteNumber");
                    ConrP_TCompleteNumber.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperTelephone"]);
                    ConrP_DirectTelephoneCommunication.AppendChild(ConrP_TCompleteNumber);

                    XmlElement ConrP_FaxCommunication = xmlXFWBV3.CreateElement("ram:FaxCommunication");
                    ConrP_DefinedTradeContact.AppendChild(ConrP_FaxCommunication);

                    XmlElement ConrP_FCompleteNumber = xmlXFWBV3.CreateElement("ram:CompleteNumber");
                    ConrP_FCompleteNumber.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperFaxNo"]);
                    ConrP_FaxCommunication.AppendChild(ConrP_FCompleteNumber);

                    XmlElement ConrP_URIEmailCommunication = xmlXFWBV3.CreateElement("ram:URIEmailCommunication");
                    ConrP_DefinedTradeContact.AppendChild(ConrP_URIEmailCommunication);

                    XmlElement ConrP_EURIID = xmlXFWBV3.CreateElement("ram:URIID");
                    //XmlCDataSection cdata_ConrP_EURIID = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperEmailId"]));
                    //ConrP_EURIID.AppendChild(cdata_ConrP_EURIID);
                    ConrP_EURIID.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperEmailId"]);
                    ConrP_URIEmailCommunication.AppendChild(ConrP_EURIID);

                    XmlElement ConrP_TelexCommunication = xmlXFWBV3.CreateElement("ram:TelexCommunication");
                    ConrP_DefinedTradeContact.AppendChild(ConrP_TelexCommunication);

                    XmlElement ConrP_XCompleteNumber = xmlXFWBV3.CreateElement("ram:CompleteNumber");
                    ConrP_XCompleteNumber.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperTelex"]);
                    ConrP_TelexCommunication.AppendChild(ConrP_XCompleteNumber);

                    #endregion

                    #region ConsigneeParty

                    XmlElement ConsigneeParty = xmlXFWBV3.CreateElement("ram:ConsigneeParty");
                    MasterConsignment.AppendChild(ConsigneeParty);

                    XmlElement ConsP_PrimaryID = xmlXFWBV3.CreateElement("ram:PrimaryID");
                    ConsP_PrimaryID.SetAttribute("schemeAgencyID", "1");
                    ConsP_PrimaryID.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigAccCode"]);
                    ConsigneeParty.AppendChild(ConsP_PrimaryID);

                    XmlElement ConsP_AdditionalID = xmlXFWBV3.CreateElement("ram:AdditionalID");
                    ConsP_AdditionalID.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeStandardID"]);
                    ConsigneeParty.AppendChild(ConsP_AdditionalID);

                    XmlElement ConsP_Name = xmlXFWBV3.CreateElement("ram:Name");
                    //XmlCDataSection cdata_ConsP_Name = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeName"]));
                    //ConsP_Name.AppendChild(cdata_ConsP_Name);
                    ConsP_Name.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeName"]);
                    ConsigneeParty.AppendChild(ConsP_Name);

                    XmlElement ConsP_AccountID = xmlXFWBV3.CreateElement("ram:AccountID");
                    ConsP_AccountID.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigAccCode"]);
                    ConsigneeParty.AppendChild(ConsP_AccountID);

                    XmlElement ConsP_PostalStructuredAddress = xmlXFWBV3.CreateElement("ram:PostalStructuredAddress");
                    ConsigneeParty.AppendChild(ConsP_PostalStructuredAddress);

                    XmlElement ConsP_PostcodeCode = xmlXFWBV3.CreateElement("ram:PostcodeCode");
                    ConsP_PostcodeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneePincode"]);
                    ConsP_PostalStructuredAddress.AppendChild(ConsP_PostcodeCode);

                    XmlElement ConsP_StreetName = xmlXFWBV3.CreateElement("ram:StreetName");
                    //XmlCDataSection cdata_ConsP_StreetName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeAddress"]));
                    //ConsP_StreetName.AppendChild(cdata_ConsP_StreetName);
                    ConsP_StreetName.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeAddress"]);
                    ConsP_PostalStructuredAddress.AppendChild(ConsP_StreetName);

                    XmlElement ConsP_CityName = xmlXFWBV3.CreateElement("ram:CityName");
                    //XmlCDataSection cdata_ConsP_CityName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["consigneecityname"]));
                    //ConsP_CityName.AppendChild(cdata_ConsP_CityName);
                    ConsP_CityName.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["consigneecityname"]);
                    ConsP_PostalStructuredAddress.AppendChild(ConsP_CityName);

                    XmlElement ConsP_CountryID = xmlXFWBV3.CreateElement("ram:CountryID");
                    ConsP_CountryID.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeCountry"]);
                    ConsP_PostalStructuredAddress.AppendChild(ConsP_CountryID);

                    XmlElement ConsP_CountryName = xmlXFWBV3.CreateElement("ram:CountryName");
                    //XmlCDataSection cdata_ConsP_CountryName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeRegionName"]));
                    //ConsP_CountryName.AppendChild(cdata_ConsP_CountryName);
                    ConsP_CountryName.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeRegionName"]);
                    ConsP_PostalStructuredAddress.AppendChild(ConsP_CountryName);

                    XmlElement ConsP_CountrySubDivisionName = xmlXFWBV3.CreateElement("ram:CountrySubDivisionName");
                    //XmlCDataSection cdata_ConsP_CountrySubDivisionName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeRegionName"]));
                    //ConsP_CountrySubDivisionName.AppendChild(cdata_ConsP_CountrySubDivisionName);
                    ConsP_CountrySubDivisionName.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeRegionName"]);
                    ConsP_PostalStructuredAddress.AppendChild(ConsP_CountrySubDivisionName);

                    XmlElement ConsP_PostOfficeBox = xmlXFWBV3.CreateElement("ram:PostOfficeBox");
                    ConsP_PostOfficeBox.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneePincode"]);
                    ConsP_PostalStructuredAddress.AppendChild(ConsP_PostOfficeBox);

                    XmlElement ConsP_CityID = xmlXFWBV3.CreateElement("ram:CityID");
                    ConsP_CityID.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeCity"]);
                    ConsP_PostalStructuredAddress.AppendChild(ConsP_CityID);

                    XmlElement ConsP_CountrySubDivisionID = xmlXFWBV3.CreateElement("ram:CountrySubDivisionID");
                    ConsP_CountrySubDivisionID.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeCountry"]);
                    ConsP_PostalStructuredAddress.AppendChild(ConsP_CountrySubDivisionID);


                    XmlElement ConsP_DefinedTradeContact = xmlXFWBV3.CreateElement("ram:DefinedTradeContact");
                    ConsigneeParty.AppendChild(ConsP_DefinedTradeContact);

                    XmlElement ConsP_PersonName = xmlXFWBV3.CreateElement("ram:PersonName");
                    //XmlCDataSection cdata_ConsP_PersonName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeName"]));
                    //ConsP_PersonName.AppendChild(cdata_ConsP_PersonName);
                    ConsP_PersonName.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeName"]);
                    ConsP_DefinedTradeContact.AppendChild(ConsP_PersonName);

                    XmlElement ConsP_DepartmentName = xmlXFWBV3.CreateElement("ram:DepartmentName");
                    ConsP_DepartmentName.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeDepartnmentName"]);
                    ConsP_DefinedTradeContact.AppendChild(ConsP_DepartmentName);

                    XmlElement ConsP_DirectTelephoneCommunication = xmlXFWBV3.CreateElement("ram:DirectTelephoneCommunication");
                    ConsP_DefinedTradeContact.AppendChild(ConsP_DirectTelephoneCommunication);

                    XmlElement ConsP_TCompleteNumber = xmlXFWBV3.CreateElement("ram:CompleteNumber");
                    ConsP_TCompleteNumber.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeTelephone"]);
                    ConsP_DirectTelephoneCommunication.AppendChild(ConsP_TCompleteNumber);

                    XmlElement ConsP_FaxCommunication = xmlXFWBV3.CreateElement("ram:FaxCommunication");
                    ConsP_DefinedTradeContact.AppendChild(ConsP_FaxCommunication);

                    XmlElement ConsP_FCompleteNumber = xmlXFWBV3.CreateElement("ram:CompleteNumber");
                    ConsP_FCompleteNumber.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeFaxNo"]);
                    ConsP_FaxCommunication.AppendChild(ConsP_FCompleteNumber);

                    XmlElement ConsP_URIEmailCommunication = xmlXFWBV3.CreateElement("ram:URIEmailCommunication");
                    ConsP_DefinedTradeContact.AppendChild(ConsP_URIEmailCommunication);

                    XmlElement ConsP_EURIID = xmlXFWBV3.CreateElement("ram:URIID");
                    //XmlCDataSection cdata_ConsP_EURIID = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigEmailId"]));
                    //ConsP_EURIID.AppendChild(cdata_ConsP_EURIID);
                    ConsP_EURIID.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigEmailId"]);
                    ConsP_URIEmailCommunication.AppendChild(ConsP_EURIID);

                    XmlElement ConsP_TelexCommunication = xmlXFWBV3.CreateElement("ram:TelexCommunication");
                    ConsP_DefinedTradeContact.AppendChild(ConsP_TelexCommunication);

                    XmlElement ConsP_XCompleteNumber = xmlXFWBV3.CreateElement("ram:CompleteNumber");
                    ConsP_XCompleteNumber.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ConsigneeTelex"]);
                    ConsP_TelexCommunication.AppendChild(ConsP_XCompleteNumber);

                    #endregion

                    #region FreightForwarderParty
                    if (dsFWBMessage.Tables[2].Rows.Count > 0)
                    {
                        XmlElement FreightForwarderParty = xmlXFWBV3.CreateElement("ram:FreightForwarderParty");
                        MasterConsignment.AppendChild(FreightForwarderParty);

                        XmlElement FFP_PrimaryID = xmlXFWBV3.CreateElement("ram:PrimaryID");
                        FFP_PrimaryID.SetAttribute("schemeAgencyID", "1");
                        FFP_PrimaryID.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["AgentAccountCode"]);
                        FreightForwarderParty.AppendChild(FFP_PrimaryID);

                        XmlElement FFP_AdditionalID = xmlXFWBV3.CreateElement("ram:AdditionalID");
                        FFP_AdditionalID.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["ShippingAgentCode"]);
                        FreightForwarderParty.AppendChild(FFP_AdditionalID);

                        XmlElement FFP_Name = xmlXFWBV3.CreateElement("ram:Name");
                        FFP_Name.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["ShippingAgentName"]);
                        //XmlCDataSection cdata_FFP_Name = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["ShippingAgentName"]));
                        //FFP_Name.AppendChild(cdata_FFP_Name);
                        FreightForwarderParty.AppendChild(FFP_Name);

                        XmlElement FFP_AccountID = xmlXFWBV3.CreateElement("ram:AccountID");
                        FFP_AccountID.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["AgentAccountCode"]);
                        FreightForwarderParty.AppendChild(FFP_AccountID);

                        XmlElement FFP_CargoAgentID = xmlXFWBV3.CreateElement("ram:CargoAgentID");
                        FFP_CargoAgentID.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["IATAAgentCode"]);
                        FreightForwarderParty.AppendChild(FFP_CargoAgentID);

                        XmlElement FFP_FreightForwarderAddress = xmlXFWBV3.CreateElement("ram:FreightForwarderAddress");
                        FreightForwarderParty.AppendChild(FFP_FreightForwarderAddress);

                        XmlElement FFP_PostcodeCode = xmlXFWBV3.CreateElement("ram:PostcodeCode");
                        FFP_PostcodeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["PostalZIP"]);
                        FFP_FreightForwarderAddress.AppendChild(FFP_PostcodeCode);

                        XmlElement FFP_StreetName = xmlXFWBV3.CreateElement("ram:StreetName");
                        FFP_StreetName.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["AgentAddress"]);
                        //XmlCDataSection cdata_FFP_StreetName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["AgentAddress"]));
                        //FFP_StreetName.AppendChild(cdata_FFP_StreetName);
                        FFP_FreightForwarderAddress.AppendChild(FFP_StreetName);

                        XmlElement FFP_CityName = xmlXFWBV3.CreateElement("ram:CityName");
                        FFP_CityName.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["City"]);
                        //XmlCDataSection cdata_FFP_CityName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["City"]));
                        //FFP_CityName.AppendChild(cdata_FFP_CityName);
                        FFP_FreightForwarderAddress.AppendChild(FFP_CityName);

                        XmlElement FFP_CountryID = xmlXFWBV3.CreateElement("ram:CountryID");
                        FFP_CountryID.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["Country"]);
                        FFP_FreightForwarderAddress.AppendChild(FFP_CountryID);

                        XmlElement FFP_CountryName = xmlXFWBV3.CreateElement("ram:CountryName");
                        FFP_CountryName.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["CountryName"]);
                        FFP_FreightForwarderAddress.AppendChild(FFP_CountryName);

                        XmlElement FFP_CountrySubDivisionName = xmlXFWBV3.CreateElement("ram:CountrySubDivisionName");
                        FFP_CountrySubDivisionName.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["State"]);
                        //XmlCDataSection cdata_FFP_CountrySubDivisionName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["State"]));
                        //FFP_CountrySubDivisionName.AppendChild(cdata_FFP_CountrySubDivisionName);
                        FFP_FreightForwarderAddress.AppendChild(FFP_CountrySubDivisionName);

                        XmlElement FFP_PostOfficeBox = xmlXFWBV3.CreateElement("ram:PostOfficeBox");
                        FFP_PostOfficeBox.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["PostalZIP"]);
                        FFP_FreightForwarderAddress.AppendChild(FFP_PostOfficeBox);

                        XmlElement FFP_CityID = xmlXFWBV3.CreateElement("ram:CityID");
                        FFP_CityID.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["Station"]);
                        FFP_FreightForwarderAddress.AppendChild(FFP_CityID);

                        XmlElement FFP_CountrySubDivisionID = xmlXFWBV3.CreateElement("ram:CountrySubDivisionID");
                        FFP_CountrySubDivisionID.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["State"]);
                        //XmlCDataSection cdata_FFP_CountrySubDivisionID = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["State"]));
                        //FFP_CountrySubDivisionID.AppendChild(cdata_FFP_CountrySubDivisionID);
                        FFP_FreightForwarderAddress.AppendChild(FFP_CountrySubDivisionID);

                        XmlElement FFP_SpecifiedCargoAgentLocation = xmlXFWBV3.CreateElement("ram:SpecifiedCargoAgentLocation");
                        FreightForwarderParty.AppendChild(FFP_SpecifiedCargoAgentLocation);

                        XmlElement FFP_SpecifiedCargoAgentLocation_ID = xmlXFWBV3.CreateElement("ram:ID");
                        FFP_SpecifiedCargoAgentLocation_ID.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["CassId"]);
                        FFP_SpecifiedCargoAgentLocation.AppendChild(FFP_SpecifiedCargoAgentLocation_ID);

                        XmlElement FFP_DefinedTradeContact = xmlXFWBV3.CreateElement("ram:DefinedTradeContact");
                        FreightForwarderParty.AppendChild(FFP_DefinedTradeContact);

                        XmlElement FFP_PersonName = xmlXFWBV3.CreateElement("ram:PersonName");
                        FFP_PersonName.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["ShippingAgentName"]);
                        //XmlCDataSection cdata_FFP_FFP_PersonName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["ShippingAgentName"]));
                        //FFP_PersonName.AppendChild(cdata_FFP_FFP_PersonName);
                        FFP_DefinedTradeContact.AppendChild(FFP_PersonName);

                        XmlElement FFP_DepartmentName = xmlXFWBV3.CreateElement("ram:DepartmentName");
                        FFP_DepartmentName.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["DepartmentName"]);
                        FFP_DefinedTradeContact.AppendChild(FFP_DepartmentName);

                        XmlElement FFP_DirectTelephoneCommunication = xmlXFWBV3.CreateElement("ram:DirectTelephoneCommunication");
                        FFP_DefinedTradeContact.AppendChild(FFP_DirectTelephoneCommunication);

                        XmlElement FFP_TCompleteNumber = xmlXFWBV3.CreateElement("ram:CompleteNumber");
                        FFP_TCompleteNumber.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["AgentPhone"]);
                        FFP_DirectTelephoneCommunication.AppendChild(FFP_TCompleteNumber);

                        XmlElement FFP_FaxCommunication = xmlXFWBV3.CreateElement("ram:FaxCommunication");
                        FFP_DefinedTradeContact.AppendChild(FFP_FaxCommunication);

                        XmlElement FFP_FCompleteNumber = xmlXFWBV3.CreateElement("ram:CompleteNumber");
                        FFP_FCompleteNumber.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["AgentFax"]);
                        FFP_FaxCommunication.AppendChild(FFP_FCompleteNumber);

                        XmlElement FFP_URIEmailCommunication = xmlXFWBV3.CreateElement("ram:URIEmailCommunication");
                        FFP_DefinedTradeContact.AppendChild(FFP_URIEmailCommunication);

                        XmlElement FFP_EURIID = xmlXFWBV3.CreateElement("ram:URIID");
                        FFP_EURIID.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["AgentEmailID"]);
                        //XmlCDataSection cdata_FFP_EURIID_ = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["AgentEmailID"]));
                        //FFP_EURIID.AppendChild(cdata_FFP_EURIID_);
                        FFP_URIEmailCommunication.AppendChild(FFP_EURIID);

                        XmlElement FFP_TelexCommunication = xmlXFWBV3.CreateElement("ram:TelexCommunication");
                        FFP_DefinedTradeContact.AppendChild(FFP_TelexCommunication);

                        XmlElement FFP_XCompleteNumber = xmlXFWBV3.CreateElement("ram:CompleteNumber");
                        FFP_XCompleteNumber.InnerText = Convert.ToString(dsFWBMessage.Tables[2].Rows[0]["AgentSitaAddress"]);
                        FFP_TelexCommunication.AppendChild(FFP_XCompleteNumber);
                    }
                    #endregion

                    #region AssociatedParty

                    XmlElement AssociatedParty = xmlXFWBV3.CreateElement("ram:AssociatedParty");
                    MasterConsignment.AppendChild(AssociatedParty);

                    XmlElement AssocP_PrimaryID = xmlXFWBV3.CreateElement("ram:PrimaryID");
                    AssocP_PrimaryID.SetAttribute("schemeAgencyID", "1");
                    AssocP_PrimaryID.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_schemeAgencyID"]);
                    AssociatedParty.AppendChild(AssocP_PrimaryID);

                    XmlElement AssocP_AdditionalID = xmlXFWBV3.CreateElement("ram:AdditionalID");
                    AssocP_AdditionalID.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_AdditionalID"]);
                    AssociatedParty.AppendChild(AssocP_AdditionalID);

                    XmlElement AssocP_Name = xmlXFWBV3.CreateElement("ram:Name");
                    AssocP_Name.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_Name"]);
                    //XmlCDataSection cdata_AssocP_Name = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_Name"]));
                    //AssocP_Name.AppendChild(cdata_AssocP_Name);
                    AssociatedParty.AppendChild(AssocP_Name);

                    XmlElement AssocP_RoleCode = xmlXFWBV3.CreateElement("ram:RoleCode");
                    AssocP_RoleCode.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_RoleCode"]);
                    AssociatedParty.AppendChild(AssocP_RoleCode);

                    XmlElement AssocP_Role = xmlXFWBV3.CreateElement("ram:Role");
                    AssocP_Role.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_Role"]);
                    AssociatedParty.AppendChild(AssocP_Role);


                    XmlElement AssocP_PostalStructuredAddress = xmlXFWBV3.CreateElement("ram:PostalStructuredAddress");
                    AssociatedParty.AppendChild(AssocP_PostalStructuredAddress);

                    XmlElement AssocP_PostcodeCode = xmlXFWBV3.CreateElement("ram:PostcodeCode");
                    AssocP_PostcodeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_PostcodeCode"]);
                    AssocP_PostalStructuredAddress.AppendChild(AssocP_PostcodeCode);

                    XmlElement AssocP_StreetName = xmlXFWBV3.CreateElement("ram:StreetName");
                    AssocP_StreetName.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_StreetName"]);
                    //XmlCDataSection cdata_AssocP_StreetName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_StreetName"]));
                    //AssocP_StreetName.AppendChild(cdata_AssocP_StreetName);
                    AssocP_PostalStructuredAddress.AppendChild(AssocP_StreetName);

                    XmlElement AssocP_CityName = xmlXFWBV3.CreateElement("ram:CityName");
                    AssocP_CityName.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_CityName"]);
                    //XmlCDataSection cdata_AssocP_CityName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_CityName"]));
                    //AssocP_CityName.AppendChild(cdata_AssocP_CityName);
                    AssocP_PostalStructuredAddress.AppendChild(AssocP_CityName);

                    XmlElement AssocP_CountryID = xmlXFWBV3.CreateElement("ram:CountryID");
                    AssocP_CountryID.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_CountryID"]);
                    AssocP_PostalStructuredAddress.AppendChild(AssocP_CountryID);

                    XmlElement AssocP_CountryName = xmlXFWBV3.CreateElement("ram:CountryName");
                    AssocP_CountryName.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_CountryName"]);
                    //XmlCDataSection cdata_AssocP_CountryName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_CityName"]));
                    //AssocP_CountryName.AppendChild(cdata_AssocP_CountryName);
                    AssocP_PostalStructuredAddress.AppendChild(AssocP_CountryName);

                    XmlElement AssocP_CountrySubDivisionName = xmlXFWBV3.CreateElement("ram:CountrySubDivisionName");
                    AssocP_CountrySubDivisionName.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_CountrySubDivisionName"]);
                    //XmlCDataSection cdata_AssocP_CountrySubDivisionName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_CountrySubDivisionName"]));
                    //AssocP_CountrySubDivisionName.AppendChild(cdata_AssocP_CountrySubDivisionName);
                    AssocP_PostalStructuredAddress.AppendChild(AssocP_CountrySubDivisionName);

                    XmlElement AssocP_PostOfficeBox = xmlXFWBV3.CreateElement("ram:PostOfficeBox");
                    AssocP_PostOfficeBox.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_PostOfficeBox"]);
                    AssocP_PostalStructuredAddress.AppendChild(AssocP_PostOfficeBox);

                    XmlElement AssocP_CityID = xmlXFWBV3.CreateElement("ram:CityID");
                    AssocP_CityID.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_CityID"]);
                    AssocP_PostalStructuredAddress.AppendChild(AssocP_CityID);

                    XmlElement AssocP_CountrySubDivisionID = xmlXFWBV3.CreateElement("ram:CountrySubDivisionID");
                    AssocP_CountrySubDivisionID.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_CountrySubDivisionID"]);
                    AssocP_PostalStructuredAddress.AppendChild(AssocP_CountrySubDivisionID);


                    XmlElement AssocP_DefinedTradeContact = xmlXFWBV3.CreateElement("ram:DefinedTradeContact");
                    AssociatedParty.AppendChild(AssocP_DefinedTradeContact);

                    XmlElement AssocP_PersonName = xmlXFWBV3.CreateElement("ram:PersonName");
                    AssocP_PersonName.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_DefinedTradeContact_PersonName"]);
                    //XmlCDataSection cdata_AssocP_PersonName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_DefinedTradeContact_PersonName"]));
                    //AssocP_PersonName.AppendChild(cdata_AssocP_PersonName);
                    AssocP_DefinedTradeContact.AppendChild(AssocP_PersonName);

                    XmlElement AssocP_DepartmentName = xmlXFWBV3.CreateElement("ram:DepartmentName");
                    AssocP_DepartmentName.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_DefinedTradeContact_DepartmentName"]);
                    //XmlCDataSection cdata_AssocP_DepartmentName = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_DefinedTradeContact_DepartmentName"]));
                    //AssocP_DepartmentName.AppendChild(cdata_AssocP_DepartmentName);
                    AssocP_DefinedTradeContact.AppendChild(AssocP_DepartmentName);

                    XmlElement AssocP_DirectTelephoneCommunication = xmlXFWBV3.CreateElement("ram:DirectTelephoneCommunication");
                    AssocP_DefinedTradeContact.AppendChild(AssocP_DirectTelephoneCommunication);

                    XmlElement AssocP_TCompleteNumber = xmlXFWBV3.CreateElement("ram:CompleteNumber");
                    AssocP_TCompleteNumber.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_DirectTelephoneCommunication"]);
                    AssocP_DirectTelephoneCommunication.AppendChild(AssocP_TCompleteNumber);

                    XmlElement AssocP_FaxCommunication = xmlXFWBV3.CreateElement("ram:FaxCommunication");
                    AssocP_DefinedTradeContact.AppendChild(AssocP_FaxCommunication);

                    XmlElement AssocP_FCompleteNumber = xmlXFWBV3.CreateElement("ram:CompleteNumber");
                    AssocP_FCompleteNumber.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_FaxCommunication"]);
                    AssocP_FaxCommunication.AppendChild(AssocP_FCompleteNumber);

                    XmlElement AssocP_URIEmailCommunication = xmlXFWBV3.CreateElement("ram:URIEmailCommunication");
                    AssocP_DefinedTradeContact.AppendChild(AssocP_URIEmailCommunication);

                    XmlElement AssocP_EURIID = xmlXFWBV3.CreateElement("ram:URIID");
                    AssocP_EURIID.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_URIEmailCommunication"]);
                    //XmlCDataSection cdata_AssocP_EURIID = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_URIEmailCommunication"]));
                    //AssocP_EURIID.AppendChild(cdata_AssocP_EURIID);
                    AssocP_URIEmailCommunication.AppendChild(AssocP_EURIID);

                    XmlElement AssocP_TelexCommunication = xmlXFWBV3.CreateElement("ram:TelexCommunication");
                    AssocP_DefinedTradeContact.AppendChild(AssocP_TelexCommunication);

                    XmlElement AssocP_XCompleteNumber = xmlXFWBV3.CreateElement("ram:CompleteNumber");
                    AssocP_XCompleteNumber.InnerText = Convert.ToString(dsFWBMessage.Tables[11].Rows[0]["AP_TelexCommunication"]);
                    AssocP_TelexCommunication.AppendChild(AssocP_XCompleteNumber);

                    #endregion

                    #region OriginLocation
                    XmlElement OriginLocation = xmlXFWBV3.CreateElement("ram:OriginLocation");
                    MasterConsignment.AppendChild(OriginLocation);

                    XmlElement OriginLocation_ID = xmlXFWBV3.CreateElement("ram:ID");
                    OriginLocation_ID.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["OriginCode"]);
                    OriginLocation.AppendChild(OriginLocation_ID);

                    XmlElement OriginLocation_Name = xmlXFWBV3.CreateElement("ram:Name");
                    OriginLocation_Name.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["OriginAirport"]);
                    //XmlCDataSection cdata_OriginLocation_Name = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["OriginAirport"]));
                    //OriginLocation_Name.AppendChild(cdata_OriginLocation_Name);
                    OriginLocation.AppendChild(OriginLocation_Name);
                    #endregion

                    #region DestinationLocation

                    XmlElement FinalDestinationLocation = xmlXFWBV3.CreateElement("ram:FinalDestinationLocation");
                    MasterConsignment.AppendChild(FinalDestinationLocation);

                    XmlElement FinalDestinationLocation_ID = xmlXFWBV3.CreateElement("ram:ID");
                    FinalDestinationLocation_ID.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["DestinationCode"]);
                    FinalDestinationLocation.AppendChild(FinalDestinationLocation_ID);

                    XmlElement FinalDestinationLocation_Name = xmlXFWBV3.CreateElement("ram:Name");
                    FinalDestinationLocation_Name.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["DestinationAirport"]);
                    //XmlCDataSection cdata_FinalDestinationLocation_Name = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["DestinationAirport"]));
                    //FinalDestinationLocation_Name.AppendChild(cdata_FinalDestinationLocation_Name);
                    FinalDestinationLocation.AppendChild(FinalDestinationLocation_Name);

                    #endregion

                    for (int i = 0; i < dsFWBMessage.Tables[3].Rows.Count; i++)
                    {
                        #region SpecifiedLogisticsTransportMovement
                        //SpecifiedLogisticsTransportMovement Loop
                        XmlElement SpecifiedLogisticsTransportMovement = xmlXFWBV3.CreateElement("ram:SpecifiedLogisticsTransportMovement");
                        MasterConsignment.AppendChild(SpecifiedLogisticsTransportMovement);

                        XmlElement SLogTranMov_StageCode = xmlXFWBV3.CreateElement("ram:StageCode");
                        SLogTranMov_StageCode.InnerText = Convert.ToString(dsFWBMessage.Tables[3].Rows[i]["ModeOfTransport"]);
                        SpecifiedLogisticsTransportMovement.AppendChild(SLogTranMov_StageCode);

                        XmlElement SLogTranMov_ModeCode = xmlXFWBV3.CreateElement("ram:ModeCode");
                        SLogTranMov_ModeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["TransportModeCode"]);
                        SpecifiedLogisticsTransportMovement.AppendChild(SLogTranMov_ModeCode);

                        XmlElement SLogTranMov_Mode = xmlXFWBV3.CreateElement("ram:Mode");
                        SLogTranMov_Mode.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["AirTransportMode"]);
                        SpecifiedLogisticsTransportMovement.AppendChild(SLogTranMov_Mode);

                        XmlElement SLogTranMov_ID = xmlXFWBV3.CreateElement("ram:ID");
                        SLogTranMov_ID.InnerText = Convert.ToString(dsFWBMessage.Tables[3].Rows[i]["FltNumber"]);
                        SpecifiedLogisticsTransportMovement.AppendChild(SLogTranMov_ID);

                        XmlElement SLogTranMov_SequenceNumeric = xmlXFWBV3.CreateElement("ram:SequenceNumeric");
                        SLogTranMov_SequenceNumeric.InnerText = Convert.ToString(dsFWBMessage.Tables[3].Rows[i]["SpecifiedLogisticsTransportMovement_SeqNum"]);
                        SpecifiedLogisticsTransportMovement.AppendChild(SLogTranMov_SequenceNumeric);

                        XmlElement SLogTranMov_UsedLogisticsTransportMeans = xmlXFWBV3.CreateElement("ram:UsedLogisticsTransportMeans");
                        SpecifiedLogisticsTransportMovement.AppendChild(SLogTranMov_UsedLogisticsTransportMeans);

                        XmlElement ULogTransMeans_Name = xmlXFWBV3.CreateElement("ram:Name");
                        ULogTransMeans_Name.InnerText = Convert.ToString(dsFWBMessage.Tables[3].Rows[i]["PartnerName"]);
                        SLogTranMov_UsedLogisticsTransportMeans.AppendChild(ULogTransMeans_Name);

                        XmlElement SLogTranMov_ArrivalEvent = xmlXFWBV3.CreateElement("ram:ArrivalEvent");
                        SpecifiedLogisticsTransportMovement.AppendChild(SLogTranMov_ArrivalEvent);

                        XmlElement ArrivalEvent_ScheduledOccurrenceDateTime = xmlXFWBV3.CreateElement("ram:ScheduledOccurrenceDateTime");
                        ArrivalEvent_ScheduledOccurrenceDateTime.InnerText = Convert.ToString(dsFWBMessage.Tables[3].Rows[i]["FlightScheduleArrivlaTime"]);
                        SLogTranMov_ArrivalEvent.AppendChild(ArrivalEvent_ScheduledOccurrenceDateTime);

                        XmlElement ArrivalEvent_OccurrenceArrivalLocation = xmlXFWBV3.CreateElement("ram:OccurrenceArrivalLocation");
                        SLogTranMov_ArrivalEvent.AppendChild(ArrivalEvent_OccurrenceArrivalLocation);

                        XmlElement OccurrenceArrivalLocation_ID = xmlXFWBV3.CreateElement("ram:ID");
                        OccurrenceArrivalLocation_ID.InnerText = Convert.ToString(dsFWBMessage.Tables[3].Rows[i]["FltDestination"]);
                        ArrivalEvent_OccurrenceArrivalLocation.AppendChild(OccurrenceArrivalLocation_ID);

                        XmlElement OccurrenceArrivalLocation_Name = xmlXFWBV3.CreateElement("ram:Name");
                        OccurrenceArrivalLocation_Name.InnerText = Convert.ToString(dsFWBMessage.Tables[3].Rows[i]["DestinationAirportName"]);
                        //XmlCDataSection cdata_OccurrenceArrivalLocation_Name = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[3].Rows[i]["DestinationAirportName"]));
                        //OccurrenceArrivalLocation_Name.AppendChild(cdata_OccurrenceArrivalLocation_Name);
                        ArrivalEvent_OccurrenceArrivalLocation.AppendChild(OccurrenceArrivalLocation_Name);

                        XmlElement OccurrenceArrivalLocation_TypeCode = xmlXFWBV3.CreateElement("ram:TypeCode");
                        OccurrenceArrivalLocation_TypeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[3].Rows[i]["TypeCode"]);
                        ArrivalEvent_OccurrenceArrivalLocation.AppendChild(OccurrenceArrivalLocation_TypeCode);

                        XmlElement SLogTranMov_DepartureEvent = xmlXFWBV3.CreateElement("ram:DepartureEvent");
                        SpecifiedLogisticsTransportMovement.AppendChild(SLogTranMov_DepartureEvent);

                        XmlElement DepartureEvent_ScheduledOccurrenceDateTime = xmlXFWBV3.CreateElement("ram:ScheduledOccurrenceDateTime");
                        DepartureEvent_ScheduledOccurrenceDateTime.InnerText = Convert.ToString(dsFWBMessage.Tables[3].Rows[i]["FlightScheduleDepartureTime"]);
                        SLogTranMov_DepartureEvent.AppendChild(DepartureEvent_ScheduledOccurrenceDateTime);

                        XmlElement DepartureEvent_OccurrenceDepartureLocation = xmlXFWBV3.CreateElement("ram:OccurrenceDepartureLocation");
                        SLogTranMov_DepartureEvent.AppendChild(DepartureEvent_OccurrenceDepartureLocation);

                        XmlElement OccurrenceDepartureLocation_ID = xmlXFWBV3.CreateElement("ram:ID");
                        OccurrenceDepartureLocation_ID.InnerText = Convert.ToString(dsFWBMessage.Tables[3].Rows[i]["FltOrigin"]);
                        DepartureEvent_OccurrenceDepartureLocation.AppendChild(OccurrenceDepartureLocation_ID);

                        XmlElement OccurrenceDepartureLocation_Name = xmlXFWBV3.CreateElement("ram:Name");
                        OccurrenceDepartureLocation_Name.InnerText = Convert.ToString(dsFWBMessage.Tables[3].Rows[i]["OriginAirportName"]);
                        //XmlCDataSection cdata_OccurrenceDepartureLocation_Name = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[3].Rows[i]["OriginAirportName"]));
                        //OccurrenceDepartureLocation_Name.AppendChild(cdata_OccurrenceDepartureLocation_Name);
                        DepartureEvent_OccurrenceDepartureLocation.AppendChild(OccurrenceDepartureLocation_Name);

                        XmlElement OccurrenceDepartureLocation_TypeCode = xmlXFWBV3.CreateElement("ram:TypeCode");
                        OccurrenceDepartureLocation_TypeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[3].Rows[i]["TypeCode"]);
                        DepartureEvent_OccurrenceDepartureLocation.AppendChild(OccurrenceDepartureLocation_TypeCode);

                        //SpecifiedLogisticsTransportMovement End
                        #endregion
                    }

                    #region UtilizedLogisticsTransportEquipment
                    //UtilizedLogisticsTransportEquipment
                    XmlElement UtilizedLogisticsTransportEquipment = xmlXFWBV3.CreateElement("ram:UtilizedLogisticsTransportEquipment");
                    MasterConsignment.AppendChild(UtilizedLogisticsTransportEquipment);

                    XmlElement ULogTransEquip_ID = xmlXFWBV3.CreateElement("ram:ID");
                    ULogTransEquip_ID.InnerText = Convert.ToString(dsFWBMessage.Tables[4].Rows[0]["VehicleNo"]);
                    UtilizedLogisticsTransportEquipment.AppendChild(ULogTransEquip_ID);

                    XmlElement ULogTransEquip_CharacteristicCode = xmlXFWBV3.CreateElement("ram:CharacteristicCode");
                    ULogTransEquip_CharacteristicCode.InnerText = Convert.ToString(dsFWBMessage.Tables[4].Rows[0]["VehType"]);
                    UtilizedLogisticsTransportEquipment.AppendChild(ULogTransEquip_CharacteristicCode);

                    XmlElement ULogTransEquip_Characteristic = xmlXFWBV3.CreateElement("ram:Characteristic");
                    ULogTransEquip_Characteristic.InnerText = Convert.ToString(dsFWBMessage.Tables[4].Rows[0]["VehicleCapacity"]);
                    UtilizedLogisticsTransportEquipment.AppendChild(ULogTransEquip_Characteristic);

                    XmlElement ULogTransEquip_AffixedLogisticsSeal = xmlXFWBV3.CreateElement("ram:AffixedLogisticsSeal");
                    UtilizedLogisticsTransportEquipment.AppendChild(ULogTransEquip_AffixedLogisticsSeal);

                    XmlElement AffixedLogisticsSeal_ID = xmlXFWBV3.CreateElement("ram:ID");
                    AffixedLogisticsSeal_ID.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["VehcialeSealNo"]);
                    ULogTransEquip_AffixedLogisticsSeal.AppendChild(AffixedLogisticsSeal_ID);
                    #endregion

                    #region HandlingSPHInstructions
                    if (dsFWBMessage.Tables[14].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsFWBMessage.Tables[14].Rows.Count; i++)
                        {
                            XmlElement MasterConsignment_HandlingSPHInstructions = xmlXFWBV3.CreateElement("ram:HandlingSPHInstructions");
                            //MasterConsignment_HandlingSPHInstructions.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["MasterConsignment_ID"]);
                            MasterConsignment.AppendChild(MasterConsignment_HandlingSPHInstructions);

                            XmlElement HandlingSPHInstructions_Description = xmlXFWBV3.CreateElement("ram:Description");
                            HandlingSPHInstructions_Description.InnerText = Convert.ToString(dsFWBMessage.Tables[14].Rows[i]["HSPHISHCDescription"]);
                            //XmlCDataSection cdata_HandlingSPHInstructions_Description = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[14].Rows[i]["HSPHISHCDescription"]));
                            //HandlingSPHInstructions_Description.AppendChild(cdata_HandlingSPHInstructions_Description);
                            MasterConsignment_HandlingSPHInstructions.AppendChild(HandlingSPHInstructions_Description);

                            XmlElement HandlingSPHInstructions_DescriptionCode = xmlXFWBV3.CreateElement("ram:DescriptionCode");
                            HandlingSPHInstructions_DescriptionCode.InnerText = Convert.ToString(dsFWBMessage.Tables[14].Rows[i]["HSPHISHCcode"]);
                            MasterConsignment_HandlingSPHInstructions.AppendChild(HandlingSPHInstructions_DescriptionCode);
                        }
                    }




                    #endregion

                    #region HandlingSSRInstructions
                    XmlElement MasterConsignment_HandlingSSRInstructions = xmlXFWBV3.CreateElement("ram:HandlingSSRInstructions");
                    //MasterConsignment_HandlingSPHInstructions.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["MasterConsignment_ID"]);
                    MasterConsignment.AppendChild(MasterConsignment_HandlingSSRInstructions);

                    XmlElement HandlingSSRInstructions_Description = xmlXFWBV3.CreateElement("ram:Description");
                    HandlingSSRInstructions_Description.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["HandlingInfo"]);
                    //XmlCDataSection cdata_HandlingSSRInstructions_Description = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["HandlingInfo"]));
                    //HandlingSSRInstructions_Description.AppendChild(cdata_HandlingSSRInstructions_Description);
                    MasterConsignment_HandlingSSRInstructions.AppendChild(HandlingSSRInstructions_Description);

                    XmlElement HandlingSSRInstructions_DescriptionCode = xmlXFWBV3.CreateElement("ram:DescriptionCode");
                    HandlingSSRInstructions_DescriptionCode.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["SHCCodes"]);
                    MasterConsignment_HandlingSSRInstructions.AppendChild(HandlingSSRInstructions_DescriptionCode);
                    #endregion

                    #region HandlingOSIInstructions
                    if (dsFWBMessage.Tables[13].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsFWBMessage.Tables[13].Rows.Count; i++)
                        {

                            XmlElement MasterConsignment_HandlingOSIInstructions = xmlXFWBV3.CreateElement("ram:HandlingOSIInstructions");
                            MasterConsignment.AppendChild(MasterConsignment_HandlingOSIInstructions);

                            XmlElement HandlingOSIInstructions_Description = xmlXFWBV3.CreateElement("ram:Description");
                            HandlingOSIInstructions_Description.InnerText = Convert.ToString(dsFWBMessage.Tables[13].Rows[i]["HandlingOSIInstructions_Description"]);
                            MasterConsignment_HandlingOSIInstructions.AppendChild(HandlingOSIInstructions_Description);

                            XmlElement HandlingOSIInstructions_DescriptionCode = xmlXFWBV3.CreateElement("ram:DescriptionCode");
                            HandlingOSIInstructions_DescriptionCode.InnerText = Convert.ToString(dsFWBMessage.Tables[13].Rows[i]["HandlingOSIInstructions_DescriptionCode"]);
                            MasterConsignment_HandlingOSIInstructions.AppendChild(HandlingOSIInstructions_DescriptionCode);
                        }
                    }
                    #endregion

                    #region IncludedAccountingNote
                    XmlElement MasterConsignment_IncludedAccountingNote = xmlXFWBV3.CreateElement("ram:IncludedAccountingNote");
                    MasterConsignment.AppendChild(MasterConsignment_IncludedAccountingNote);

                    XmlElement IncludedAccountingNote_ContentCode = xmlXFWBV3.CreateElement("ram:ContentCode");
                    IncludedAccountingNote_ContentCode.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["AccCode"]);
                    MasterConsignment_IncludedAccountingNote.AppendChild(IncludedAccountingNote_ContentCode);

                    XmlElement IncludedAccountingNote_Content = xmlXFWBV3.CreateElement("ram:Content");
                    IncludedAccountingNote_Content.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["AccountInfo"]);
                    //XmlCDataSection cdata_IncludedAccountingNote_Content = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["AccountInfo"]));
                    //IncludedAccountingNote_Content.AppendChild(cdata_IncludedAccountingNote_Content);
                    MasterConsignment_IncludedAccountingNote.AppendChild(IncludedAccountingNote_Content);
                    #endregion

                    #region IncludedCustomsNote
                    XmlElement MasterConsignment_IncludedCustomsNote = xmlXFWBV3.CreateElement("ram:IncludedCustomsNote");
                    MasterConsignment.AppendChild(MasterConsignment_IncludedCustomsNote);

                    XmlElement IncludedCustomsNote_ContentCode = xmlXFWBV3.CreateElement("ram:ContentCode");
                    IncludedCustomsNote_ContentCode.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["IncludedCustomsNote_ContentCode"]);
                    MasterConsignment_IncludedCustomsNote.AppendChild(IncludedCustomsNote_ContentCode);

                    XmlElement IncludedCustomsNote_Content = xmlXFWBV3.CreateElement("ram:Content");
                    IncludedCustomsNote_Content.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["IncludedCustomsNote_Content"]);
                    MasterConsignment_IncludedCustomsNote.AppendChild(IncludedCustomsNote_Content);

                    XmlElement IncludedCustomsNote_SubjectCode = xmlXFWBV3.CreateElement("ram:SubjectCode");
                    IncludedCustomsNote_SubjectCode.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["IncludedCustomsNote_SubjectCode"]);
                    MasterConsignment_IncludedCustomsNote.AppendChild(IncludedCustomsNote_SubjectCode);

                    XmlElement IncludedCustomsNote_CountryID = xmlXFWBV3.CreateElement("ram:CountryID");
                    IncludedCustomsNote_CountryID.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["IncludedCustomsNote_Country"]);
                    MasterConsignment_IncludedCustomsNote.AppendChild(IncludedCustomsNote_CountryID);
                    #endregion

                    #region AssociatedReferenceDocument
                    //XmlElement MasterConsignment_AssociatedReferenceDocument = xmlXFWBV3.CreateElement("ram:AssociatedReferenceDocument");
                    //MasterConsignment.AppendChild(MasterConsignment_AssociatedReferenceDocument);

                    //XmlElement AssociatedReferenceDocument_ID = xmlXFWBV3.CreateElement("ram:ID");
                    //AssociatedReferenceDocument_ID.InnerText = Convert.ToString(dsFWBMessage.Tables[12].Rows[0]["AssociatedReferenceDocumentID"]);
                    //MasterConsignment_AssociatedReferenceDocument.AppendChild(AssociatedReferenceDocument_ID);

                    //XmlElement AssociatedReferenceDocument_IssueDateTime = xmlXFWBV3.CreateElement("ram:IssueDateTime");
                    //AssociatedReferenceDocument_IssueDateTime.InnerText = Convert.ToString(dsFWBMessage.Tables[12].Rows[0]["AssociatedReferenceDocument_IssueDateTime"]);
                    //MasterConsignment_AssociatedReferenceDocument.AppendChild(AssociatedReferenceDocument_IssueDateTime);

                    //XmlElement AssociatedReferenceDocument_TypeCode = xmlXFWBV3.CreateElement("ram:TypeCode");
                    //AssociatedReferenceDocument_TypeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[12].Rows[0]["AssociatedReferenceDocument_TypeCode"]);
                    //MasterConsignment_AssociatedReferenceDocument.AppendChild(AssociatedReferenceDocument_TypeCode);

                    //XmlElement AssociatedReferenceDocument_Name = xmlXFWBV3.CreateElement("ram:Name");
                    //AssociatedReferenceDocument_Name.InnerText = Convert.ToString(dsFWBMessage.Tables[12].Rows[0]["AssociatedReferenceDocumentName"]);
                    //MasterConsignment_AssociatedReferenceDocument.AppendChild(AssociatedReferenceDocument_Name);
                    #endregion

                    #region AssociatedConsignmentCustomsProcedure
                    //XmlElement MasterConsignment_AssociatedConsignmentCustomsProcedure = xmlXFWBV3.CreateElement("ram:AssociatedConsignmentCustomsProcedure");
                    //MasterConsignment.AppendChild(MasterConsignment_AssociatedConsignmentCustomsProcedure);

                    //XmlElement AssociatedConsignmentCustomsProcedure_GoodsStatusCode = xmlXFWBV3.CreateElement("ram:GoodsStatusCode");
                    //AssociatedConsignmentCustomsProcedure_GoodsStatusCode.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["SCI"]);
                    //MasterConsignment_AssociatedConsignmentCustomsProcedure.AppendChild(AssociatedConsignmentCustomsProcedure_GoodsStatusCode);
                    #endregion

                    #region ApplicableOriginCurrencyExchange
                    XmlElement MasterConsignment_ApplicableOriginCurrencyExchange = xmlXFWBV3.CreateElement("ram:ApplicableOriginCurrencyExchange");
                    MasterConsignment.AppendChild(MasterConsignment_ApplicableOriginCurrencyExchange);

                    XmlElement ApplicableOriginCurrencyExchange_SourceCurrencyCode = xmlXFWBV3.CreateElement("ram:SourceCurrencyCode");
                    ApplicableOriginCurrencyExchange_SourceCurrencyCode.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]);
                    MasterConsignment_ApplicableOriginCurrencyExchange.AppendChild(ApplicableOriginCurrencyExchange_SourceCurrencyCode);
                    #endregion

                    #region ApplicableDestinationCurrencyExchange
                    XmlElement MasterConsignment_ApplicableDestinationCurrencyExchange = xmlXFWBV3.CreateElement("ram:ApplicableDestinationCurrencyExchange");
                    MasterConsignment.AppendChild(MasterConsignment_ApplicableDestinationCurrencyExchange);

                    XmlElement ApplicableDestinationCurrencyExchange_TargetCurrencyCode = xmlXFWBV3.CreateElement("ram:TargetCurrencyCode");
                    ApplicableDestinationCurrencyExchange_TargetCurrencyCode.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["BaseCurrency"]);
                    MasterConsignment_ApplicableDestinationCurrencyExchange.AppendChild(ApplicableDestinationCurrencyExchange_TargetCurrencyCode);

                    XmlElement ApplicableDestinationCurrencyExchange_MarketID = xmlXFWBV3.CreateElement("ram:MarketID");
                    ApplicableDestinationCurrencyExchange_MarketID.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["ConvRateQualifier"]);
                    MasterConsignment_ApplicableDestinationCurrencyExchange.AppendChild(ApplicableDestinationCurrencyExchange_MarketID);

                    XmlElement ApplicableDestinationCurrencyExchange_ConversionRate = xmlXFWBV3.CreateElement("ram:ConversionRate");
                    ApplicableDestinationCurrencyExchange_ConversionRate.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["ConvFactor"]);
                    MasterConsignment_ApplicableDestinationCurrencyExchange.AppendChild(ApplicableDestinationCurrencyExchange_ConversionRate);
                    #endregion

                    #region ApplicableLogisticsServiceCharge
                    XmlElement MasterConsignment_ApplicableLogisticsServiceCharge = xmlXFWBV3.CreateElement("ram:ApplicableLogisticsServiceCharge");
                    MasterConsignment.AppendChild(MasterConsignment_ApplicableLogisticsServiceCharge);

                    XmlElement ApplicableLogisticsServiceCharge_TransportPaymentMethodCode = xmlXFWBV3.CreateElement("ram:TransportPaymentMethodCode");
                    ApplicableLogisticsServiceCharge_TransportPaymentMethodCode.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["PayMode"]);
                    MasterConsignment_ApplicableLogisticsServiceCharge.AppendChild(ApplicableLogisticsServiceCharge_TransportPaymentMethodCode);

                    XmlElement ApplicableLogisticsServiceCharge_ServiceTypeCode = xmlXFWBV3.CreateElement("ram:ServiceTypeCode");
                    ApplicableLogisticsServiceCharge_ServiceTypeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["ShipmentType"]);
                    MasterConsignment_ApplicableLogisticsServiceCharge.AppendChild(ApplicableLogisticsServiceCharge_ServiceTypeCode);
                    #endregion

                    #region ApplicableLogisticsAllowanceCharge
                    //XmlElement MasterConsignment_ApplicableLogisticsAllowanceCharge = xmlXFWBV3.CreateElement("ram:ApplicableLogisticsAllowanceCharge");
                    //MasterConsignment.AppendChild(MasterConsignment_ApplicableLogisticsAllowanceCharge);

                    //XmlElement ApplicableLogisticsAllowanceCharge_ID = xmlXFWBV3.CreateElement("ram:ID");
                    //ApplicableLogisticsAllowanceCharge_ID.InnerText = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["ChargeCode"]);
                    //MasterConsignment_ApplicableLogisticsAllowanceCharge.AppendChild(ApplicableLogisticsAllowanceCharge_ID);

                    //XmlElement ApplicableLogisticsAllowanceCharge_AdditionalID = xmlXFWBV3.CreateElement("ram:AdditionalID");
                    //ApplicableLogisticsAllowanceCharge_AdditionalID.InnerText = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["OCSubCode"]);
                    //MasterConsignment_ApplicableLogisticsAllowanceCharge.AppendChild(ApplicableLogisticsAllowanceCharge_AdditionalID);

                    //XmlElement ApplicableLogisticsAllowanceCharge_PrepaidIndicator = xmlXFWBV3.CreateElement("ram:PrepaidIndicator");
                    //ApplicableLogisticsAllowanceCharge_PrepaidIndicator.InnerText = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["PrepaidIndicator"]);
                    //MasterConsignment_ApplicableLogisticsAllowanceCharge.AppendChild(ApplicableLogisticsAllowanceCharge_PrepaidIndicator);

                    //XmlElement ApplicableLogisticsAllowanceCharge_LocationTypeCode = xmlXFWBV3.CreateElement("ram:LocationTypeCode");
                    //ApplicableLogisticsAllowanceCharge_LocationTypeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["LocationTypeCode"]);
                    //MasterConsignment_ApplicableLogisticsAllowanceCharge.AppendChild(ApplicableLogisticsAllowanceCharge_LocationTypeCode);

                    //XmlElement ApplicableLogisticsAllowanceCharge_Reason = xmlXFWBV3.CreateElement("ram:Reason");
                    //ApplicableLogisticsAllowanceCharge_Reason.InnerText = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["ChargeHeadCode"]);
                    //MasterConsignment_ApplicableLogisticsAllowanceCharge.AppendChild(ApplicableLogisticsAllowanceCharge_Reason);

                    //XmlElement ApplicableLogisticsAllowanceCharge_PartyTypeCode = xmlXFWBV3.CreateElement("ram:PartyTypeCode");
                    //ApplicableLogisticsAllowanceCharge_PartyTypeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["ChargeType"]);
                    //MasterConsignment_ApplicableLogisticsAllowanceCharge.AppendChild(ApplicableLogisticsAllowanceCharge_PartyTypeCode);

                    //XmlElement ApplicableLogisticsAllowanceCharge_ActualAmount = xmlXFWBV3.CreateElement("ram:ActualAmount");
                    //ApplicableLogisticsAllowanceCharge_ActualAmount.SetAttribute("currencyID", Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]));
                    //ApplicableLogisticsAllowanceCharge_ActualAmount.InnerText = "";// Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["Charge"]);
                    //MasterConsignment_ApplicableLogisticsAllowanceCharge.AppendChild(ApplicableLogisticsAllowanceCharge_ActualAmount);

                    //XmlElement ApplicableLogisticsAllowanceCharge_TimeBasisQuantity = xmlXFWBV3.CreateElement("ram:TimeBasisQuantity");
                    //ApplicableLogisticsAllowanceCharge_TimeBasisQuantity.InnerText = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["ChargeStorageTime"]);
                    //MasterConsignment_ApplicableLogisticsAllowanceCharge.AppendChild(ApplicableLogisticsAllowanceCharge_TimeBasisQuantity);

                    //XmlElement ApplicableLogisticsAllowanceCharge_ItemBasisQuantity = xmlXFWBV3.CreateElement("ram:ItemBasisQuantity");
                    //ApplicableLogisticsAllowanceCharge_ItemBasisQuantity.InnerText = "";// Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["PiecesCount"]);
                    //MasterConsignment_ApplicableLogisticsAllowanceCharge.AppendChild(ApplicableLogisticsAllowanceCharge_ItemBasisQuantity);

                    //XmlElement ApplicableLogisticsAllowanceCharge_ServiceDate = xmlXFWBV3.CreateElement("ram:ServiceDate");
                    //ApplicableLogisticsAllowanceCharge_ServiceDate.InnerText = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["UpdatedOn"]);
                    //MasterConsignment_ApplicableLogisticsAllowanceCharge.AppendChild(ApplicableLogisticsAllowanceCharge_ServiceDate);

                    //XmlElement ApplicableLogisticsAllowanceCharge_SpecialServiceDescription = xmlXFWBV3.CreateElement("ram:SpecialServiceDescription");
                    //ApplicableLogisticsAllowanceCharge_SpecialServiceDescription.InnerText = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["ChargeHeadCode"]);
                    //MasterConsignment_ApplicableLogisticsAllowanceCharge.AppendChild(ApplicableLogisticsAllowanceCharge_SpecialServiceDescription);

                    //XmlElement ApplicableLogisticsAllowanceCharge_SpecialServiceTime = xmlXFWBV3.CreateElement("ram:SpecialServiceTime");
                    //ApplicableLogisticsAllowanceCharge_SpecialServiceTime.InnerText = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["UpdatedOn"]);
                    //MasterConsignment_ApplicableLogisticsAllowanceCharge.AppendChild(ApplicableLogisticsAllowanceCharge_SpecialServiceTime);
                    #endregion

                    #region ApplicableRating
                    XmlElement MasterConsignment_ApplicableRating = xmlXFWBV3.CreateElement("ram:ApplicableRating");
                    MasterConsignment.AppendChild(MasterConsignment_ApplicableRating);

                    XmlElement ApplicableRating_TypeCode = xmlXFWBV3.CreateElement("ram:TypeCode");
                    ApplicableRating_TypeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["RateType"]);
                    MasterConsignment_ApplicableRating.AppendChild(ApplicableRating_TypeCode);

                    XmlElement ApplicableRating_TotalChargeAmount = xmlXFWBV3.CreateElement("ram:TotalChargeAmount");
                    ApplicableRating_TotalChargeAmount.SetAttribute("currencyID", Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]));
                    ApplicableRating_TotalChargeAmount.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["FrtMKT"]);
                    MasterConsignment_ApplicableRating.AppendChild(ApplicableRating_TotalChargeAmount);

                    XmlElement ApplicableRating_ConsignmentItemQuantity = xmlXFWBV3.CreateElement("ram:ConsignmentItemQuantity");
                    ApplicableRating_ConsignmentItemQuantity.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows.Count);
                    MasterConsignment_ApplicableRating.AppendChild(ApplicableRating_ConsignmentItemQuantity);

                    #region IncludedMasterConsignmentItem
                    //IncludedMasterConsignmentItem
                    XmlElement ApplicableRating_IncludedMasterConsignmentItem = xmlXFWBV3.CreateElement("ram:IncludedMasterConsignmentItem");
                    MasterConsignment_ApplicableRating.AppendChild(ApplicableRating_IncludedMasterConsignmentItem);

                    XmlElement AR_IncludedMasterConsItem_SequenceNumeric = xmlXFWBV3.CreateElement("ram:SequenceNumeric");
                    AR_IncludedMasterConsItem_SequenceNumeric.InnerText = Convert.ToString(dsFWBMessage.Tables[10].Rows.Count);
                    ApplicableRating_IncludedMasterConsignmentItem.AppendChild(AR_IncludedMasterConsItem_SequenceNumeric);

                    XmlElement AR_IncludedMasterConsItem_TypeCode = xmlXFWBV3.CreateElement("ram:TypeCode");
                    AR_IncludedMasterConsItem_TypeCode.SetAttribute("listAgencyID", Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["TypeCode_listAgencyID"]));
                    AR_IncludedMasterConsItem_TypeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["CommCode"]);
                    ApplicableRating_IncludedMasterConsignmentItem.AppendChild(AR_IncludedMasterConsItem_TypeCode);

                    XmlElement AR_IncludedMasterConsItem_GrossWeightMeasure = xmlXFWBV3.CreateElement("ram:GrossWeightMeasure");
                    AR_IncludedMasterConsItem_GrossWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["UOM"]));
                    AR_IncludedMasterConsItem_GrossWeightMeasure.InnerText = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["GWeight"]);
                    ApplicableRating_IncludedMasterConsignmentItem.AppendChild(AR_IncludedMasterConsItem_GrossWeightMeasure);

                    XmlElement AR_IncludedMasterConsItem_GrossVolumeMeasure = xmlXFWBV3.CreateElement("ram:GrossVolumeMeasure");
                    AR_IncludedMasterConsItem_GrossVolumeMeasure.SetAttribute("unitCode", Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["UOM"]));
                    AR_IncludedMasterConsItem_GrossVolumeMeasure.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["VolumetricWeight"]);
                    ApplicableRating_IncludedMasterConsignmentItem.AppendChild(AR_IncludedMasterConsItem_GrossVolumeMeasure);

                    XmlElement AR_IncludedMasterConsItem_PackageQuantity = xmlXFWBV3.CreateElement("ram:PackageQuantity");
                    AR_IncludedMasterConsItem_PackageQuantity.InnerText = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["Pieces"]);
                    ApplicableRating_IncludedMasterConsignmentItem.AppendChild(AR_IncludedMasterConsItem_PackageQuantity);

                    XmlElement AR_IncludedMasterConsItem_PieceQuantity = xmlXFWBV3.CreateElement("ram:PieceQuantity");
                    AR_IncludedMasterConsItem_PieceQuantity.InnerText = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["Pieces"]);
                    ApplicableRating_IncludedMasterConsignmentItem.AppendChild(AR_IncludedMasterConsItem_PieceQuantity);

                    XmlElement AR_IncludedMasterConsItem_VolumetricFactor = xmlXFWBV3.CreateElement("ram:VolumetricFactor");
                    AR_IncludedMasterConsItem_VolumetricFactor.InnerText = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["VolumetricFactor"]);
                    ApplicableRating_IncludedMasterConsignmentItem.AppendChild(AR_IncludedMasterConsItem_VolumetricFactor);

                    XmlElement AR_IncludedMasterConsItem_Information = xmlXFWBV3.CreateElement("ram:Information");
                    AR_IncludedMasterConsItem_Information.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["UOM"]);
                    ApplicableRating_IncludedMasterConsignmentItem.AppendChild(AR_IncludedMasterConsItem_Information);

                    //NatureIdentificationTransportCargo
                    XmlElement AR_IMasterConsItem_NatureIdentificationTransportCargo = xmlXFWBV3.CreateElement("ram:NatureIdentificationTransportCargo");
                    ApplicableRating_IncludedMasterConsignmentItem.AppendChild(AR_IMasterConsItem_NatureIdentificationTransportCargo);

                    XmlElement AR_IMCItem_NatureIdentTranCargo_Identification = xmlXFWBV3.CreateElement("ram:Identification");
                    AR_IMCItem_NatureIdentTranCargo_Identification.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["CommodityDesc"]);
                    //XmlCDataSection cdata_AR_IMCItem_NatureIdentTranCargo_Identification = xmlXFWBV3.CreateCDataSection(Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["CommodityDesc"]));
                    //AR_IMCItem_NatureIdentTranCargo_Identification.AppendChild(cdata_AR_IMCItem_NatureIdentTranCargo_Identification);
                    AR_IMasterConsItem_NatureIdentificationTransportCargo.AppendChild(AR_IMCItem_NatureIdentTranCargo_Identification);

                    //OriginCountry
                    XmlElement AR_IMCItem_OriginCountry = xmlXFWBV3.CreateElement("ram:OriginCountry");
                    ApplicableRating_IncludedMasterConsignmentItem.AppendChild(AR_IMCItem_OriginCountry);

                    XmlElement AR_IMCItem_OriginCountry_ID = xmlXFWBV3.CreateElement("ram:ID");
                    AR_IMCItem_OriginCountry_ID.InnerText = Convert.ToString(dsFWBMessage.Tables[1].Rows[0]["ShipperCountry"]);
                    AR_IMCItem_OriginCountry.AppendChild(AR_IMCItem_OriginCountry_ID);
                    #endregion

                    #region AssociatedUnitLoadTransportEquipment
                    //AssociatedUnitLoadTransportEquipment
                    if (dsFWBMessage.Tables[7].Rows.Count > 0)
                    {
                        if (Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["ULDSNo"]).Length > 1)
                        {
                            XmlElement AR_IMCItem_AssociatedUnitLoadTransportEquipmenty = xmlXFWBV3.CreateElement("ram:AssociatedUnitLoadTransportEquipment");
                            ApplicableRating_IncludedMasterConsignmentItem.AppendChild(AR_IMCItem_AssociatedUnitLoadTransportEquipmenty);

                            XmlElement AR_IMCItem_AssocULoadTranEq_ID = xmlXFWBV3.CreateElement("ram:ID");
                            AR_IMCItem_AssocULoadTranEq_ID.InnerText = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["ULDSNo"]);
                            AR_IMCItem_AssociatedUnitLoadTransportEquipmenty.AppendChild(AR_IMCItem_AssocULoadTranEq_ID);

                            XmlElement AR_IMCItem_AssocULoadTranEq_TareWeightMeasure = xmlXFWBV3.CreateElement("ram:TareWeightMeasure");
                            AR_IMCItem_AssocULoadTranEq_TareWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["UnitCode"]));
                            AR_IMCItem_AssocULoadTranEq_TareWeightMeasure.InnerText = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["TareWeight"]);
                            AR_IMCItem_AssociatedUnitLoadTransportEquipmenty.AppendChild(AR_IMCItem_AssocULoadTranEq_TareWeightMeasure);

                            XmlElement AR_IMCItem_AssocULoadTranEq_LoadedPackageQuantity = xmlXFWBV3.CreateElement("ram:LoadedPackageQuantity");
                            AR_IMCItem_AssocULoadTranEq_LoadedPackageQuantity.InnerText = Convert.ToString(dsFWBMessage.Tables[9].Rows[0]["SLAC"]);
                            AR_IMCItem_AssociatedUnitLoadTransportEquipmenty.AppendChild(AR_IMCItem_AssocULoadTranEq_LoadedPackageQuantity);

                            XmlElement AR_IMCItem_AssocULoadTranEq_CharacteristicCode = xmlXFWBV3.CreateElement("ram:CharacteristicCode");
                            AR_IMCItem_AssocULoadTranEq_CharacteristicCode.InnerText = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["ULDType"]);
                            AR_IMCItem_AssociatedUnitLoadTransportEquipmenty.AppendChild(AR_IMCItem_AssocULoadTranEq_CharacteristicCode);

                            XmlElement AR_IMCItem_AssocULoadTranEq_OperatingParty = xmlXFWBV3.CreateElement("ram:OperatingParty");
                            AR_IMCItem_AssociatedUnitLoadTransportEquipmenty.AppendChild(AR_IMCItem_AssocULoadTranEq_OperatingParty);

                            XmlElement AR_IMCItem_AULTEq_OperatingParty_PrimaryID = xmlXFWBV3.CreateElement("ram:PrimaryID");
                            AR_IMCItem_AULTEq_OperatingParty_PrimaryID.SetAttribute("schemeAgencyID", Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["ULDSNo"]));
                            AR_IMCItem_AULTEq_OperatingParty_PrimaryID.InnerText = Convert.ToString(dsFWBMessage.Tables[3].Rows[0]["PartnerCode"]);
                            AR_IMCItem_AssocULoadTranEq_OperatingParty.AppendChild(AR_IMCItem_AULTEq_OperatingParty_PrimaryID);
                        }
                    }
                    #endregion

                    #region TransportLogisticsPackage
                    //TransportLogisticsPackage

                    XmlElement AR_IMCItem_TransportLogisticsPackage = xmlXFWBV3.CreateElement("ram:TransportLogisticsPackage");
                    ApplicableRating_IncludedMasterConsignmentItem.AppendChild(AR_IMCItem_TransportLogisticsPackage);

                    XmlElement AR_IMCItem_TransportLogisticsPackage_ItemQuantity = xmlXFWBV3.CreateElement("ram:ItemQuantity");
                    AR_IMCItem_TransportLogisticsPackage_ItemQuantity.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["PiecesCount"]);
                    AR_IMCItem_TransportLogisticsPackage.AppendChild(AR_IMCItem_TransportLogisticsPackage_ItemQuantity);

                    XmlElement AR_IMCItem_TransportLogisticsPackage_GrossWeightMeasure = xmlXFWBV3.CreateElement("ram:GrossWeightMeasure");
                    AR_IMCItem_TransportLogisticsPackage_GrossWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["UOM"]));
                    AR_IMCItem_TransportLogisticsPackage_GrossWeightMeasure.InnerText = Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["GrossWeight"]);
                    AR_IMCItem_TransportLogisticsPackage.AppendChild(AR_IMCItem_TransportLogisticsPackage_GrossWeightMeasure);
                    if (dsFWBMessage.Tables[7].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsFWBMessage.Tables[7].Rows.Count; i++)
                        {
                            //LinearSpatialDimension
                            XmlElement AR_IMCItem_TLogPac_LinearSpatialDimension = xmlXFWBV3.CreateElement("ram:LinearSpatialDimension");
                            AR_IMCItem_TransportLogisticsPackage.AppendChild(AR_IMCItem_TLogPac_LinearSpatialDimension);

                            XmlElement AR_IMCItem_TLogPac_LinearSpaDimension_WidthMeasure = xmlXFWBV3.CreateElement("ram:WidthMeasure");
                            //AR_IMCItem_TLogPac_LinearSpaDimension_WidthMeasure.SetAttribute("unitCode", Convert.ToString(dsFWBMessage.Tables[7].Rows[i]["MeasureUnit"]));
                            AR_IMCItem_TLogPac_LinearSpaDimension_WidthMeasure.InnerText = Convert.ToString(dsFWBMessage.Tables[7].Rows[i]["Width"]) == "" ? "0" : Convert.ToString(dsFWBMessage.Tables[7].Rows[i]["Width"]);
                            AR_IMCItem_TLogPac_LinearSpatialDimension.AppendChild(AR_IMCItem_TLogPac_LinearSpaDimension_WidthMeasure);

                            XmlElement AR_IMCItem_TLogPac_LinearSpaDimension_LengthMeasure = xmlXFWBV3.CreateElement("ram:LengthMeasure");
                            //AR_IMCItem_TLogPac_LinearSpaDimension_LengthMeasure.SetAttribute("unitCode", Convert.ToString(dsFWBMessage.Tables[7].Rows[i]["MeasureUnit"]));
                            AR_IMCItem_TLogPac_LinearSpaDimension_LengthMeasure.InnerText = Convert.ToString(dsFWBMessage.Tables[7].Rows[i]["Length"]) == "" ? "0" : Convert.ToString(dsFWBMessage.Tables[7].Rows[i]["Length"]);
                            AR_IMCItem_TLogPac_LinearSpatialDimension.AppendChild(AR_IMCItem_TLogPac_LinearSpaDimension_LengthMeasure);

                            XmlElement AR_IMCItem_TLogPac_LinearSpaDimension_HeightMeasure = xmlXFWBV3.CreateElement("ram:HeightMeasure");
                            //AR_IMCItem_TLogPac_LinearSpaDimension_HeightMeasure.SetAttribute("unitCode", Convert.ToString(dsFWBMessage.Tables[7].Rows[i]["MeasureUnit"]));
                            AR_IMCItem_TLogPac_LinearSpaDimension_HeightMeasure.InnerText = Convert.ToString(dsFWBMessage.Tables[7].Rows[i]["Height"]) == "" ? "0" : Convert.ToString(dsFWBMessage.Tables[7].Rows[i]["Height"]);
                            AR_IMCItem_TLogPac_LinearSpatialDimension.AppendChild(AR_IMCItem_TLogPac_LinearSpaDimension_HeightMeasure);
                        }
                    }
                    #endregion

                    #region ApplicableFreightRateServiceCharge
                    //ApplicableFreightRateServiceCharge
                    XmlElement AR_ApplicableFreightRateServiceCharge = xmlXFWBV3.CreateElement("ram:ApplicableFreightRateServiceCharge");
                    ApplicableRating_IncludedMasterConsignmentItem.AppendChild(AR_ApplicableFreightRateServiceCharge);

                    XmlElement AR_AppFreightRateServChrg_CategoryCode = xmlXFWBV3.CreateElement("ram:CategoryCode");
                    AR_AppFreightRateServChrg_CategoryCode.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["RateClass"]);
                    AR_ApplicableFreightRateServiceCharge.AppendChild(AR_AppFreightRateServChrg_CategoryCode);

                    XmlElement AR_AppFreightRateServChrg_CommodityItemID = xmlXFWBV3.CreateElement("ram:CommodityItemID");
                    AR_AppFreightRateServChrg_CommodityItemID.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["CommodityCode"]);
                    AR_ApplicableFreightRateServiceCharge.AppendChild(AR_AppFreightRateServChrg_CommodityItemID);

                    XmlElement AR_AppFreightRateServChrg_ChargeableWeightMeasure = xmlXFWBV3.CreateElement("ram:ChargeableWeightMeasure");
                    AR_AppFreightRateServChrg_ChargeableWeightMeasure.SetAttribute("unitCode", Convert.ToString(dsFWBMessage.Tables[0].Rows[0]["UOM"]));
                    AR_AppFreightRateServChrg_ChargeableWeightMeasure.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["ChargedWeight"]);
                    AR_ApplicableFreightRateServiceCharge.AppendChild(AR_AppFreightRateServChrg_ChargeableWeightMeasure);

                    XmlElement AR_AppFreightRateServChrg_AppliedRate = xmlXFWBV3.CreateElement("ram:AppliedRate");
                    AR_AppFreightRateServChrg_AppliedRate.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["FrtMKT"]);
                    AR_ApplicableFreightRateServiceCharge.AppendChild(AR_AppFreightRateServChrg_AppliedRate);

                    XmlElement AR_AppFreightRateServChrg_AppliedAmount = xmlXFWBV3.CreateElement("ram:AppliedAmount");
                    AR_AppFreightRateServChrg_AppliedAmount.SetAttribute("currencyID", Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]));
                    AR_AppFreightRateServChrg_AppliedAmount.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["FrtMKT"]);
                    AR_ApplicableFreightRateServiceCharge.AppendChild(AR_AppFreightRateServChrg_AppliedAmount);
                    #endregion

                    #region SpecifiedRateCombinationPointLocation
                    //SpecifiedRateCombinationPointLocation
                    XmlElement AR_SpecifiedRateCombinationPointLocation = xmlXFWBV3.CreateElement("ram:SpecifiedRateCombinationPointLocation");
                    ApplicableRating_IncludedMasterConsignmentItem.AppendChild(AR_SpecifiedRateCombinationPointLocation);

                    XmlElement AR_SpecifiedRateCombinationPointLocation_ID = xmlXFWBV3.CreateElement("ram:ID");
                    AR_SpecifiedRateCombinationPointLocation_ID.InnerText = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["SpecifiedRateCombinationPointLocation_ID"]);
                    AR_SpecifiedRateCombinationPointLocation.AppendChild(AR_SpecifiedRateCombinationPointLocation_ID);
                    #endregion

                    #region ApplicableUnitLoadDeviceRateClass
                    //ApplicableUnitLoadDeviceRateClass
                    XmlElement AR_ApplicableUnitLoadDeviceRateClass = xmlXFWBV3.CreateElement("ram:ApplicableUnitLoadDeviceRateClass");
                    ApplicableRating_IncludedMasterConsignmentItem.AppendChild(AR_ApplicableUnitLoadDeviceRateClass);
                    if (dsFWBMessage.Tables[7].Rows.Count > 0)
                    {
                        XmlElement AR_ApplicableUnitLoadDeviceRateClass_TypeCode = xmlXFWBV3.CreateElement("ram:TypeCode");
                        AR_ApplicableUnitLoadDeviceRateClass_TypeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[7].Rows[0]["Shape"]);
                        AR_ApplicableUnitLoadDeviceRateClass.AppendChild(AR_ApplicableUnitLoadDeviceRateClass_TypeCode);
                    }
                    XmlElement AR_ApplicableUnitLoadDeviceRateClass_BasisCode = xmlXFWBV3.CreateElement("ram:BasisCode");
                    AR_ApplicableUnitLoadDeviceRateClass_BasisCode.InnerText = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["MKTRateClass"]);
                    AR_ApplicableUnitLoadDeviceRateClass.AppendChild(AR_ApplicableUnitLoadDeviceRateClass_BasisCode);

                    XmlElement AR_ApplicableUnitLoadDeviceRateClass_AppliedPercent = xmlXFWBV3.CreateElement("ram:AppliedPercent");
                    AR_ApplicableUnitLoadDeviceRateClass_AppliedPercent.InnerText = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["Discount"]);
                    AR_ApplicableUnitLoadDeviceRateClass.AppendChild(AR_ApplicableUnitLoadDeviceRateClass_AppliedPercent);

                    XmlElement AR_ApplicableUnitLoadDeviceRateClass_ReferenceID = xmlXFWBV3.CreateElement("ram:ReferenceID");
                    AR_ApplicableUnitLoadDeviceRateClass_ReferenceID.InnerText = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["ReferenceID"]);
                    AR_ApplicableUnitLoadDeviceRateClass.AppendChild(AR_ApplicableUnitLoadDeviceRateClass_ReferenceID);

                    XmlElement AR_ApplicableUnitLoadDeviceRateClass_ReferenceTypeCode = xmlXFWBV3.CreateElement("ram:ReferenceTypeCode");
                    AR_ApplicableUnitLoadDeviceRateClass_ReferenceTypeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[10].Rows[0]["ReferenceTypeCode"]);
                    AR_ApplicableUnitLoadDeviceRateClass.AppendChild(AR_ApplicableUnitLoadDeviceRateClass_ReferenceTypeCode);
                    #endregion

                    #endregion

                    #region ApplicableTotalRating
                    XmlElement MasterConsignment_ApplicableTotalRating = xmlXFWBV3.CreateElement("ram:ApplicableTotalRating");
                    MasterConsignment.AppendChild(MasterConsignment_ApplicableTotalRating);

                    XmlElement MC_ApplicableTotalRating_TypeCode = xmlXFWBV3.CreateElement("ram:TypeCode");
                    MC_ApplicableTotalRating_TypeCode.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["RateType"]);
                    MasterConsignment_ApplicableTotalRating.AppendChild(MC_ApplicableTotalRating_TypeCode);

                    #region ApplicableDestinationCurrencyServiceCharge
                    //ApplicableDestinationCurrencyServiceCharge
                    XmlElement AppTotalRating_ApplicableDestinationCurrencyServiceCharge = xmlXFWBV3.CreateElement("ram:ApplicableDestinationCurrencyServiceCharge");
                    MasterConsignment_ApplicableTotalRating.AppendChild(AppTotalRating_ApplicableDestinationCurrencyServiceCharge);

                    XmlElement AppTotalRating_AppDestCurSerChrg_CollectAppliedAmount = xmlXFWBV3.CreateElement("ram:CollectAppliedAmount");
                    AppTotalRating_AppDestCurSerChrg_CollectAppliedAmount.SetAttribute("currencyID", Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]));
                    AppTotalRating_AppDestCurSerChrg_CollectAppliedAmount.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["FrtMKT"]);
                    AppTotalRating_ApplicableDestinationCurrencyServiceCharge.AppendChild(AppTotalRating_AppDestCurSerChrg_CollectAppliedAmount);

                    XmlElement AppTotalRating_AppDestCurSerChrg_DestinationAppliedAmount = xmlXFWBV3.CreateElement("ram:DestinationAppliedAmount");
                    AppTotalRating_AppDestCurSerChrg_DestinationAppliedAmount.SetAttribute("currencyID", Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]));
                    AppTotalRating_AppDestCurSerChrg_DestinationAppliedAmount.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["OtherCharges"]);
                    AppTotalRating_ApplicableDestinationCurrencyServiceCharge.AppendChild(AppTotalRating_AppDestCurSerChrg_DestinationAppliedAmount);

                    XmlElement AppTotalRating_AppDestCurSerChrg_TotalAppliedAmount = xmlXFWBV3.CreateElement("ram:TotalAppliedAmount");
                    AppTotalRating_AppDestCurSerChrg_TotalAppliedAmount.SetAttribute("currencyID", Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]));
                    AppTotalRating_AppDestCurSerChrg_TotalAppliedAmount.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Total"]);
                    AppTotalRating_ApplicableDestinationCurrencyServiceCharge.AppendChild(AppTotalRating_AppDestCurSerChrg_TotalAppliedAmount);
                    #endregion


                    #region ApplicablePrepaidCollectMonetarySummation
                    //ApplicablePrepaidCollectMonetarySummation
                    XmlElement AppTotalRating_ApplicablePrepaidCollectMonetarySummation = xmlXFWBV3.CreateElement("ram:ApplicablePrepaidCollectMonetarySummation");
                    MasterConsignment_ApplicableTotalRating.AppendChild(AppTotalRating_ApplicablePrepaidCollectMonetarySummation);

                    XmlElement AppTotalRating_AppPpCollectMonetarySummation_PrepaidIndicator = xmlXFWBV3.CreateElement("ram:PrepaidIndicator");
                    AppTotalRating_AppPpCollectMonetarySummation_PrepaidIndicator.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["PrepaidIndicator"]);
                    AppTotalRating_ApplicablePrepaidCollectMonetarySummation.AppendChild(AppTotalRating_AppPpCollectMonetarySummation_PrepaidIndicator);

                    XmlElement AppTotalRating_AppPpCollectMonetarySummation_WeightChargeTotalAmount = xmlXFWBV3.CreateElement("ram:WeightChargeTotalAmount");
                    AppTotalRating_AppPpCollectMonetarySummation_WeightChargeTotalAmount.SetAttribute("currencyID", Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]));
                    AppTotalRating_AppPpCollectMonetarySummation_WeightChargeTotalAmount.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["FrtIATA"]);
                    AppTotalRating_ApplicablePrepaidCollectMonetarySummation.AppendChild(AppTotalRating_AppPpCollectMonetarySummation_WeightChargeTotalAmount);

                    XmlElement AppTotalRating_AppPpCollectMonetarySummation_ValuationChargeTotalAmount = xmlXFWBV3.CreateElement("ram:ValuationChargeTotalAmount");
                    AppTotalRating_AppPpCollectMonetarySummation_ValuationChargeTotalAmount.SetAttribute("currencyID", Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]));
                    AppTotalRating_AppPpCollectMonetarySummation_ValuationChargeTotalAmount.InnerText = Convert.ToString(dsFWBMessage.Tables[6].Rows[0]["Charge"]);
                    AppTotalRating_ApplicablePrepaidCollectMonetarySummation.AppendChild(AppTotalRating_AppPpCollectMonetarySummation_ValuationChargeTotalAmount);

                    XmlElement AppTotalRating_AppPpCollectMonetarySummation_TaxTotalAmount = xmlXFWBV3.CreateElement("ram:TaxTotalAmount");
                    AppTotalRating_AppPpCollectMonetarySummation_TaxTotalAmount.SetAttribute("currencyID", Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]));
                    AppTotalRating_AppPpCollectMonetarySummation_TaxTotalAmount.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["IATAServiceTax"]);
                    AppTotalRating_ApplicablePrepaidCollectMonetarySummation.AppendChild(AppTotalRating_AppPpCollectMonetarySummation_TaxTotalAmount);

                    XmlElement AppTotalRating_AppPpCollectMonetarySummation_AgentTotalDuePayableAmount = xmlXFWBV3.CreateElement("ram:AgentTotalDuePayableAmount");
                    AppTotalRating_AppPpCollectMonetarySummation_AgentTotalDuePayableAmount.SetAttribute("currencyID", Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]));
                    AppTotalRating_AppPpCollectMonetarySummation_AgentTotalDuePayableAmount.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["OCDueAgent"]);
                    AppTotalRating_ApplicablePrepaidCollectMonetarySummation.AppendChild(AppTotalRating_AppPpCollectMonetarySummation_AgentTotalDuePayableAmount);

                    XmlElement AppTotalRating_AppPpCollectMonetarySummation_CarrierTotalDuePayableAmount = xmlXFWBV3.CreateElement("ram:CarrierTotalDuePayableAmount");
                    AppTotalRating_AppPpCollectMonetarySummation_CarrierTotalDuePayableAmount.SetAttribute("currencyID", Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]));
                    AppTotalRating_AppPpCollectMonetarySummation_CarrierTotalDuePayableAmount.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["OCDueCar"]);
                    AppTotalRating_ApplicablePrepaidCollectMonetarySummation.AppendChild(AppTotalRating_AppPpCollectMonetarySummation_CarrierTotalDuePayableAmount);

                    XmlElement AppTotalRating_AppPpCollectMonetarySummation_GrandTotalAmount = xmlXFWBV3.CreateElement("ram:GrandTotalAmount");
                    AppTotalRating_AppPpCollectMonetarySummation_GrandTotalAmount.SetAttribute("currencyID", Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Currency"]));
                    AppTotalRating_AppPpCollectMonetarySummation_GrandTotalAmount.InnerText = Convert.ToString(dsFWBMessage.Tables[5].Rows[0]["Total"]);
                    AppTotalRating_ApplicablePrepaidCollectMonetarySummation.AppendChild(AppTotalRating_AppPpCollectMonetarySummation_GrandTotalAmount);
                    #endregion


                    #endregion

                    #endregion

                    generateMessage = new StringBuilder(xmlXFWBV3.OuterXml);
                    generateMessage.Replace("<", "<ram:");
                    generateMessage.Replace("<ram:/", "</ram:");
                    generateMessage.Replace("<ram:![CDATA", "<![CDATA");
                    generateMessage.Replace("<ram:Waybill", "<rsm:Waybill");
                    generateMessage.Replace("</ram:Waybill", "</rsm:Waybill");
                    generateMessage.Replace("<ram:MessageHeaderDocument>", "<rsm:MessageHeaderDocument>");
                    generateMessage.Replace("</ram:MessageHeaderDocument>", "</rsm:MessageHeaderDocument>");
                    generateMessage.Replace("<ram:BusinessHeaderDocument>", "<rsm:BusinessHeaderDocument>");
                    generateMessage.Replace("</ram:BusinessHeaderDocument>", "</rsm:BusinessHeaderDocument>");
                    generateMessage.Replace("<ram:MasterConsignment>", "<rsm:MasterConsignment>");
                    generateMessage.Replace("</ram:MasterConsignment>", "</rsm:MasterConsignment>");

                    ///Remove the empty tags from XML
                    var document = System.Xml.Linq.XDocument.Parse(generateMessage.ToString());
                    //var emptyNodes = document.Descendants().Where(e => e.IsEmpty || String.IsNullOrWhiteSpace(e.Value));
                    //foreach (var emptyNode in emptyNodes.ToArray())
                    //{
                    //    emptyNode.Remove();
                    //}
                    generateMessage = new StringBuilder(document.ToString());
                    //generateMessage.Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
                    //XMLValidator objxMLValidator = new XMLValidator();
                    //string errormsg = objxMLValidator.CTeXMLValidator(generateMessage.ToString());
                    //if (errormsg.Length > 1)
                    //{
                    //    generateMessage.Clear();
                    //    generateMessage.Append(errormsg);
                    //}
                    if (customsName.ToUpper() == "DAKAR")
                    {
                        generateMessage.Replace("<rsm:Waybill xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:rsm=\"iata:waybill:1\" xmlns:ccts=\"urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2\" xmlns:udt=\"urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8\" xmlns:ram=\"iata:datamodel:3\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"iata:waybill:1 Waybill_1.xsd\">", "");
                        generateMessage.Replace("<rsm:Waybill>", "");
                        generateMessage.Replace("<ram:Waybill>", "");
                        generateMessage.Replace("</rsm:Waybill>", "");
                        generateMessage.Replace("</ram:Waybill>", "");
                        generateMessage.Replace("rsm:", "");
                        generateMessage.Replace("ram:", "");
                    }
                }
                else
                {
                    generateMessage.Append("No Data available in the system to generate message.");
                }
            }
            catch (Exception ex)
            {
                generateMessage.Append("Error Occured while generating: " + ex.Message);
                // clsLog.WriteLogAzure("Error on Generate XFWB Message Method:" + ex.ToString());
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                _logger.LogWarning("Error on Generate XFWB Message MMethod");
            }


            return generateMessage.ToString();
        }


        #region :: Private Methods::
        /// <summary>
        /// Get the Record For AWB Data to Generate XFWB message
        /// </summary>
        /// <param name="awbPrefix">AWB Prefix</param>
        /// <param name="awbNumber">AWB Number</param>
        /// <returns></returns>

        private async Task<DataSet?> GetRecordforAWBToGenerateXFWBMessage(string awbPrefix, string awbNumber)
        {
            DataSet? dsFwb = new DataSet();
            try
            {
                //SQLServer da = new SQLServer();
                //string[] paramname = new string[] { "AWBPrefix", "AWBNumber" };
                //object[] paramvalue = new object[] { awbPrefix, awbNumber };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };

                var parameters = new SqlParameter[]
                {
                     new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = awbPrefix },
                     new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbNumber }
                };


                //dsFwb = da.SelectRecords("Messaging.GetRecordMakeXFWBMessage", paramname, paramvalue, paramtype);
                dsFwb = await _readWriteDao.SelectRecords("Messaging.GetRecordMakeXFWBMessage", parameters);

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Error on Get Record for AWB ToGenerate XFWBMessage Method:" + ex.ToString());
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                _logger.LogWarning("Erro in Get Record for AWB ToGenerate XFWBMessage Method");
            }
            return dsFwb;
        }

        private XmlNode RenameNode(XmlNode e, string newName)
        {
            try
            {
                XmlDocument doc = e.OwnerDocument;
                if (doc != null)
                {
                    XmlNode newNode = doc.CreateNode(e.NodeType, newName, null);
                    while (e.HasChildNodes)
                    {
                        newNode.AppendChild(e.FirstChild);
                    }
                    XmlAttributeCollection ac = e.Attributes;
                    while (ac != null && ac.Count > 0)
                    {
                        if (newNode.Attributes != null)
                        {
                            newNode.Attributes.Append(ac[0]);
                        }
                    }
                    XmlNode parent = e.ParentNode;
                    if (parent != null)
                    {
                        parent.ReplaceChild(newNode, e);
                    }
    
                    return newNode;
                }
                return null;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        #endregion Private Methods




    }
}
