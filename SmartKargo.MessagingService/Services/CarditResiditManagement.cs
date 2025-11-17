#region CarditResiditManagement Message Processor Class Description
/* CarditResiditManagement Message Processor Class Description.
      * Company              :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright              :   Copyright © 20156QID SmartKargo(I)	Pvt. Ltd.
      * Purpose                : 
      * Created By           :   Shrishail Ashtage
      * Created On           :  
      * Approved By         :
      * Approved Date      :
      * Modified By          :  
      * Modified On          :   
      * Description           :   
     */
#endregion
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Configuration;
using System.Data;

namespace QidWorkerRole
{
    public class CarditResiditManagement
    {

        static string strConnection = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        const string PAGE_NAME = "CarditResiditManagementProcessor";
        SCMExceptionHandlingWorkRole scmException = new SCMExceptionHandlingWorkRole();
        string AgentCode = string.Empty, AgentName = string.Empty;
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<CarditResiditManagement> _logger;
        private readonly GenericFunction _genericFunction;

        public CarditResiditManagement(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<CarditResiditManagement> logger,
            GenericFunction genericFunction)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
        }

        #region Make CarditClass and CarditConsignmentclass
        /// <summary>
        /// Cardit Calss        
        /// shrishail Ashtage
        /// 2015-12-16
        /// </summary>
        public class CarditDetail
        {
            public string MessageHeader;
            public string MessageRefNumber;
            public string MessageType;
            public string MessageVersion;
            public string EventType;

            public string BagNumber;
            public string MessageFunctionCode;
            public string MailType;
            public string LegRate;
            public string MailClass;
            public string AWBOrigin;
            public string AWBDestination;
            public int TotalPieces = 0;
            public decimal TotalGrossWeight = 0;
            public string StageQualifier;
            public string FlightNo;
            public string ModeOfTransport;
            public string FlightOrigin;
            public string FlightDestination;
            public DateTime MessageArrivalDate;
            public DateTime MessageFlightDate;

            public string HNDOrg_Code;
            public string HNDOrg_LocSource;
            public string HNDOrg_LocName;

            public string HNDdest_Code;
            public string HNDdest_LocSource;
            public string HNDdest_LocName;
            public string PartnerCode;
            public string PAWBPrefix;
            public string PAWBNo;
            public List<CarditCosnsignmentDetail> objCarditCosnsignmentDetail;
            public int MsgSeqNo;
        }


        public class CarditCosnsignmentDetail
        {
            public string PackegeId;
            public string PageDocNumber = "";
            public int PackagePcs = 0;
            public string PackageWeightCode = "K";
            public decimal PackageGrossWeight = 0;
            public string ReceptacleType = "";
            public string ReceptacleHndlingClass = "";
        }


        #endregion


        public async Task<(bool, string)> EncodeAndSaveCarditMessage(string strMessage, int Srno)
        {
            bool status = false;
            CarditDetail cardit;
            CarditCosnsignmentDetail Package;
            string ErrorDesc = string.Empty;
            var carditMessage = new List<CarditDetail>();
            bool checkPackge = false;
            string strRouteDetails = string.Empty;

            string strDesignatorCode = string.Empty;

            MessageData.FltRoute[] fltroute = new MessageData.FltRoute[] { };
            MessageData.FltRoute flight = new MessageData.FltRoute("");

            MessageData.FltRouteDate[] flightRoutedate = new MessageData.FltRouteDate[] { };
            MessageData.FltRouteDate flight2 = new MessageData.FltRouteDate("");

            DataSet dsDesCode = new DataSet();
            dsDesCode = await GetDesigCode();
            if (dsDesCode != null && dsDesCode.Tables.Count > 0 && dsDesCode.Tables[0].Rows.Count > 0 && dsDesCode.Tables[0].Rows[0][0].ToString() != "")
            {
                strDesignatorCode = dsDesCode.Tables[0].Rows[0]["DesignatorCode"].ToString();

            }

            try
            {
                string[] msg = strMessage.Split('\'');
                if (msg.Length > 1)
                {
                    string[] arrUNH = strMessage.Replace("UNH+", "$UNH+").Split('$');
                    if (arrUNH.Length > 1)
                    {
                        for (int y = 1; y < arrUNH.Length; y++)
                        {
                            checkPackge = false;
                            cardit = new CarditDetail();
                            var packgeConsignment = new List<CarditCosnsignmentDetail>();
                            Package = new CarditCosnsignmentDetail();

                            string[] splitmessage = arrUNH[y].Split('\'');

                            if (arrUNH[0].Contains("UNB"))
                            {
                                string[] strinUNBTag = arrUNH[0].Split('+');
                                cardit.MessageHeader = strinUNBTag[0];
                                if (strinUNBTag.Length > 5)
                                    cardit.MsgSeqNo = Convert.ToInt32(strinUNBTag[5].Split('\'')[0]);
                            }
                            bool isSetFlight = false;
                            for (int K = 0; K < splitmessage.Length; K++)
                            {
                                if (splitmessage[K].Length > 2 && splitmessage[K].Contains("UNH"))
                                {
                                    string[] strMessageTag = splitmessage[K].Split('+');
                                    if (strMessageTag.Length > 2)
                                    {
                                        cardit.MessageRefNumber = strMessageTag[1];
                                        string[] strMessageType = strMessageTag[2].Split(':');
                                        if (strMessageType.Length >= 1)
                                        {
                                            cardit.MessageType = strMessageType[0];
                                            cardit.MessageVersion = strMessageType[1];
                                        }
                                    }
                                }
                                if (splitmessage[K].Length > 3 && splitmessage[K].Contains("BGM+"))
                                {
                                    string[] strBGNTag = splitmessage[K].Split('+');
                                    if (strBGNTag.Length > 2)
                                        cardit.BagNumber = strBGNTag[2];
                                    if (strBGNTag.Length > 3)
                                        cardit.MessageFunctionCode = strBGNTag[3];
                                }

                                if (splitmessage[K].Length > 3 && splitmessage[K].Contains("DTM") && splitmessage[K - 1].Contains("BGM+"))
                                {
                                    string[] strinDTMTag = splitmessage[K].Split('+');
                                    if (strinDTMTag.Length > 1)
                                    {
                                        string[] strDatePart = strinDTMTag[1].Split(':');
                                        string Year = strDatePart[1].Substring(0, 2);
                                        string Month = strDatePart[1].Substring(2, 2);
                                        string Date = strDatePart[1].Substring(4, 2);
                                        string hours = string.Empty;
                                        string minutes = string.Empty;
                                        if (strDatePart[1].Length == 10)
                                        {
                                            hours = strDatePart[1].Substring(6, 2);
                                            minutes = strDatePart[1].Substring(8, 2);
                                            cardit.MessageArrivalDate = DateTime.Parse("20" + Year + "/" + Month + "/" + Date + " " + hours + ":" + minutes);
                                        }
                                        else
                                        {
                                            cardit.MessageArrivalDate = DateTime.Parse("20" + Year + "/" + Month + "/" + Date);
                                        }
                                        cardit.MessageFlightDate = cardit.MessageArrivalDate;
                                    }
                                }
                                if (splitmessage[K].Length > 3 && (splitmessage[K].Contains("TSR") || splitmessage[K].Contains("TCC")))
                                {
                                    string[] strMailClass = splitmessage[K].Split('+');
                                    if (strMailClass.Length > 1)
                                        cardit.MailClass = strMailClass[1];
                                    if (strMailClass.Length > 4)
                                        cardit.MailClass = strMailClass[4];
                                }

                                if (splitmessage[K].Length > 3 && splitmessage[K].Contains("FTX"))
                                {
                                    string[] strMailType = splitmessage[K].Split('+');
                                    if (strMailType.Length > 3 && strMailType[1] == "ABK")
                                        cardit.MailType = strMailType[3];
                                }

                                if (splitmessage[K].Length > 3 && splitmessage[K].Contains("EQN"))
                                {
                                    string[] strTotalPcs = splitmessage[K].Split('+');
                                    if (strTotalPcs.Length > 1)
                                    {
                                        string[] StrTotal = strTotalPcs[1].Split(':');
                                        cardit.TotalPieces = int.Parse(StrTotal[0].ToString() == "" ? "0" : StrTotal[0].ToString());
                                    }
                                }
                                if (splitmessage[K].Length > 3 && splitmessage[K].Contains("QTY"))
                                {
                                    string[] strWeight = splitmessage[K].Split('+');
                                    if (strWeight.Length > 1)
                                    {
                                        string[] StrTotal = strWeight[1].Split(':');
                                        cardit.TotalGrossWeight = decimal.Parse(StrTotal[1].ToString() == "" ? "0" : StrTotal[1].ToString());
                                    }
                                }
                                if (splitmessage[K].Length > 3 && (splitmessage[K].Contains("RFF+ERN") || splitmessage[K].Contains("RFF+AWN")))
                                {
                                    string[] strREFTag = splitmessage[K].Split('+');
                                    if (strREFTag.Length > 1)
                                    {
                                        string[] MessageODTag = strREFTag[1].Split(':');

                                        if (MessageODTag.Length > 1 && MessageODTag[0] == "ERN")
                                            cardit.AWBOrigin = MessageODTag[1];

                                        if (MessageODTag.Length > 1 && MessageODTag[0] == "AWN")
                                            cardit.AWBDestination = MessageODTag[1];
                                    }

                                }

                                if (splitmessage[K].Length > 3 && splitmessage[K].Contains("RFF+AAM"))
                                {
                                    string[] strPAWB = splitmessage[K].Split('+');
                                    if (strPAWB.Length > 1)
                                    {
                                        string[] PAWB = strPAWB[1].Split(':');
                                        if (PAWB[1].Length > 1)
                                        {
                                            cardit.PAWBPrefix = PAWB[1].Substring(0, 3);
                                            cardit.PAWBNo = PAWB[1].Substring(3, 8);
                                        }
                                    }
                                }

                                if (splitmessage[K].Length > 3 && splitmessage[K].Contains("TDT"))
                                {
                                    string[] strTDTTag = splitmessage[K].Split('+');
                                    if (strTDTTag.Length > 1)
                                        cardit.StageQualifier = strTDTTag[1];

                                    if (strTDTTag.Length > 2)
                                    {
                                        if (strTDTTag[1] != "Z90")
                                        {
                                            if (string.IsNullOrEmpty(cardit.FlightNo))
                                            {
                                                cardit.FlightNo = strTDTTag[2];
                                                cardit.PartnerCode = strTDTTag[2].Substring(0, 2);
                                                if (strTDTTag.Length > 3)
                                                    cardit.ModeOfTransport = strTDTTag[3];
                                                isSetFlight = true;
                                            }
                                            else if (strTDTTag[2].Substring(0, 2) == strDesignatorCode && strTDTTag[1] == "20")
                                            {
                                                cardit.FlightNo = strTDTTag[2];
                                                cardit.PartnerCode = strTDTTag[2].Substring(0, 2);
                                                if (strTDTTag.Length > 3)
                                                    cardit.ModeOfTransport = strTDTTag[3];
                                                isSetFlight = true;
                                            }
                                            //else if (strTDTTag[2].Substring(0, 2) == strDesignatorCode && strTDTTag[1] == "20")
                                            //{
                                            //    cardit.FlightNo = strTDTTag[2];
                                            //    cardit.PartnerCode = strTDTTag[2].Substring(0, 2);
                                            //    if (strTDTTag.Length > 3)
                                            //        cardit.ModeOfTransport = strTDTTag[3];
                                            //    isSetFlight = true;
                                            //}
                                            else if (strTDTTag[2].Substring(0, 2) == strDesignatorCode)
                                            {
                                                cardit.FlightNo = strTDTTag[2];
                                                cardit.PartnerCode = strTDTTag[2].Substring(0, 2);
                                                if (strTDTTag.Length > 3)
                                                    cardit.ModeOfTransport = strTDTTag[3];
                                                isSetFlight = true;
                                            }
                                            //else if (strTDTTag[2].Substring(0, 2) == strDesignatorCode)
                                            //{
                                            //    cardit.FlightNo = strTDTTag[2];
                                            //    cardit.PartnerCode = strTDTTag[2].Substring(0, 2);
                                            //    if (strTDTTag.Length > 3)
                                            //        cardit.ModeOfTransport = strTDTTag[3];
                                            //    isSetFlight = true;
                                            //}
                                        }
                                    }
                                }

                                if (K == 8 && splitmessage[K].Length > 5 && splitmessage[K].Contains("LOC+84"))
                                {
                                    string[] strinLOCTag = splitmessage[K].Split('+');
                                    if (strinLOCTag.Length > 1)
                                    {
                                        string[] strFlightorigin = strinLOCTag[1].Split(':');
                                        if (strFlightorigin.Length > 1)
                                            cardit.HNDOrg_Code = strFlightorigin[0].ToString();
                                        cardit.HNDOrg_LocSource = strFlightorigin[1].ToString();
                                        cardit.HNDOrg_LocName = strFlightorigin[3].ToString();
                                    }
                                }

                                if (K == 9 && splitmessage[K].Length > 5 && splitmessage[K].Contains("LOC"))
                                {
                                    string[] strinLOCTag = splitmessage[K].Split('+');
                                    if (strinLOCTag.Length > 1)
                                    {
                                        string[] strFlightorigin = strinLOCTag[1].Split(':');
                                        if (strFlightorigin.Length > 1)
                                            cardit.HNDOrg_Code = strFlightorigin[0].ToString();
                                        cardit.HNDOrg_LocSource = strFlightorigin[1].ToString();
                                        cardit.HNDOrg_LocName = strFlightorigin[2].ToString();

                                    }
                                }

                                if (K == 11 && splitmessage[K].Length > 5 && splitmessage[K].Contains("LOC"))
                                {
                                    string[] strinLOCTag = splitmessage[K].Split('+');
                                    if (strinLOCTag.Length > 1)
                                    {
                                        for (int i = 1; i < strinLOCTag.Length; i++)
                                        {
                                            string[] strFlightorigin = strinLOCTag[i].Split(':');
                                            if (strFlightorigin.Length > 1)
                                            {
                                                cardit.HNDdest_Code = strFlightorigin[0].ToString();
                                                cardit.HNDdest_LocSource = strFlightorigin[1].ToString();
                                                cardit.HNDdest_LocName = strFlightorigin[2].ToString();
                                            }
                                        }

                                    }
                                }


                                if (splitmessage[K].Length > 5 && splitmessage[K].Contains("LOC"))
                                {
                                    string[] strinLOCTag = splitmessage[K].Split('+');

                                    if (strinLOCTag.Length > 2)
                                    {
                                        for (int i = 0; i < strinLOCTag.Length; i++)
                                        {
                                            if (strinLOCTag[i].Contains(":") && isSetFlight)
                                            {
                                                string[] strFlightOrgDest = strinLOCTag[i].Split(':');

                                                if ((splitmessage[K - 1].Contains("TDT+") || splitmessage[K - 1].Contains("TSR+")))
                                                {
                                                    cardit.FlightOrigin = strFlightOrgDest[0].ToString();
                                                    // flight.fltdept= strFlightOrgDest[0].ToString();
                                                }
                                                else
                                                {
                                                    cardit.FlightDestination = strFlightOrgDest[0].ToString();
                                                    //flight.fltarrival = strFlightOrgDest[0].ToString();
                                                    isSetFlight = false;
                                                }

                                                if (!string.IsNullOrEmpty(cardit.FlightOrigin) && !string.IsNullOrEmpty(cardit.FlightDestination))
                                                {
                                                    if (cardit.FlightOrigin != cardit.FlightDestination)
                                                    {
                                                        if (cardit.FlightNo.Substring(0, 2) == strDesignatorCode)
                                                        {
                                                            flight.fltdept = cardit.FlightOrigin;
                                                            flight.fltarrival = cardit.FlightDestination;
                                                            flight.fltnum = cardit.FlightNo;
                                                            flight.BagNumber = cardit.BagNumber;

                                                            Array.Resize(ref fltroute, fltroute.Length + 1);
                                                            fltroute[fltroute.Length - 1] = flight;
                                                        }

                                                    }

                                                }

                                            }

                                        }
                                    }
                                }

                                if (splitmessage[K].Length > 5 && splitmessage[K].Contains("DTM")
                                    && splitmessage[K - 2].Contains("LOC+") && splitmessage[K - 1].Contains("LOC+"))
                                {
                                    string[] strinLOCTag = splitmessage[K].Split('+');
                                    if (strinLOCTag.Length > 1)
                                    {
                                        string[] strDatePart = strinLOCTag[1].Split(':');
                                        string Year = strDatePart[1].Substring(0, 2);
                                        string Month = strDatePart[1].Substring(2, 2);
                                        string Date = strDatePart[1].Substring(4, 2);

                                        string hours = string.Empty;
                                        string minutes = string.Empty;
                                        if (strDatePart[1].Length == 10)
                                        {
                                            hours = strDatePart[1].Substring(6, 2);
                                            minutes = strDatePart[1].Substring(8, 2);
                                            cardit.MessageFlightDate = DateTime.Parse("20" + Year + "/" + Month + "/" + Date + " " + hours + ":" + minutes);

                                            flight2.FltDate = cardit.MessageFlightDate.ToString();

                                            Array.Resize(ref flightRoutedate, flightRoutedate.Length + 1);
                                            flightRoutedate[flightRoutedate.Length - 1] = flight2;
                                        }
                                        else
                                        {
                                            cardit.MessageFlightDate = DateTime.Parse("20" + Year + "/" + Month + "/" + Date);
                                            flight.date = cardit.MessageFlightDate.ToString();

                                            flight2.FltDate = cardit.MessageFlightDate.ToString();

                                            Array.Resize(ref flightRoutedate, flightRoutedate.Length + 1);
                                            flightRoutedate[flightRoutedate.Length - 1] = flight2;

                                        }
                                    }
                                }

                                if (splitmessage[K].Length > 2
                                    && (splitmessage[K].Contains("GID")
                                        || splitmessage[K].Contains("ID")
                                        || splitmessage[K].Contains("PCI")
                                        || splitmessage[K].Contains("CNI")
                                        || splitmessage[K].Contains("MEA")
                                        || splitmessage[K].Contains("DOC")
                                        || splitmessage[K].Contains("FTX+INS")))
                                {
                                    //if (splitmessage[K].Contains("GID") || splitmessage[K].Contains("ID"))
                                    //{
                                    //    if (!checkPackge)
                                    //    {
                                    //        checkPackge = true;
                                    //        string[] strGIDTag = splitmessage[K].Split('+');
                                    //        if (strGIDTag.Length > 2)
                                    //            Package.PackagePcs = int.Parse(strGIDTag[2].Substring(0, 1));
                                    //    }
                                    //    else
                                    //    {
                                    //        packgeConsignment.Add(Package);
                                    //        Package = new CarditCosnsignmentDetail();
                                    //        string[] strGIDTag = splitmessage[K].Split('+');
                                    //        if (strGIDTag.Length > 2)
                                    //            Package.PackagePcs = int.Parse(strGIDTag[2].Substring(0, 1));
                                    //    }
                                    //}
                                    // Above code is for V1.2

                                    if (splitmessage[K].Contains("GID") || splitmessage[K].Contains("ID"))
                                    {
                                        int cntPCS = 1;
                                        string[] strGIDTag = splitmessage[K].Split('+');
                                        if (strGIDTag.Length > 2)
                                        {
                                            Package.ReceptacleType = strGIDTag[2].Split(':')[1];
                                            Package.PackagePcs = cntPCS++;
                                        }
                                    }
                                    if (splitmessage[K].Contains("PCI") || splitmessage[K].Contains("CNI"))
                                    {
                                        if (!checkPackge)
                                        {
                                            checkPackge = true;
                                            string[] strPackageID = splitmessage[K].ToString().Split('+');
                                            if (strPackageID.Length > 2)
                                            {
                                                string[] strCarditPackage = strPackageID[2].Split(':');
                                                if (strCarditPackage.Length > 0)
                                                {
                                                    Package.PackegeId = strCarditPackage[0].ToString();
                                                    //Package.PackegeId = strCarditPackage[1].ToString();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            packgeConsignment.Add(Package);
                                            Package = new CarditCosnsignmentDetail();
                                            string[] strPackageID = splitmessage[K].ToString().Split('+');
                                            if (strPackageID.Length > 2)
                                            {
                                                string[] strCarditPackage = strPackageID[2].Split(':');
                                                if (strCarditPackage.Length > 0)
                                                {
                                                    Package.PackegeId = strCarditPackage[0].ToString();
                                                }
                                            }
                                        }
                                    }
                                    if (splitmessage[K].Contains("MEA"))
                                    {
                                        string[] strWeightID = splitmessage[K].ToString().Split('+');
                                        string[] PackageWait = strWeightID[3].Split(':');
                                        if (PackageWait.Length > 0)
                                            Package.PackageWeightCode = PackageWait[0].ToString() == "KGM" ? "K" : "L";
                                        if (PackageWait.Length > 1)
                                            Package.PackageGrossWeight = decimal.Parse(PackageWait[1].ToString() == "" ? "0" : PackageWait[1].ToString());

                                    }
                                    if (splitmessage[K].Contains("DOC"))
                                    {
                                        string[] strDocumentID = splitmessage[K].ToString().Split('+');
                                        if (strDocumentID.Length > 1)
                                        {
                                            string[] strDocNo = strDocumentID[1].Split(':');
                                            if (strDocNo.Length > 2)
                                                Package.PageDocNumber = strDocNo[2].ToString();
                                        }
                                    }
                                    if (splitmessage[K].Contains("FTX+INS"))
                                    {
                                        string[] strArrRecHndCls = splitmessage[K].ToString().Split('+');
                                        if (strArrRecHndCls.Length > 3)
                                            Package.ReceptacleHndlingClass = strArrRecHndCls[3];
                                    }
                                }
                            }

                            packgeConsignment.Add(Package);
                            cardit.objCarditCosnsignmentDetail = packgeConsignment;
                            carditMessage.Add(cardit);
                        }
                    }

                    //SQLServer dbCardit = new SQLServer();
                    int CarditID = 0;
                    int FlgRouteID = 0;

                    string FltorgNumber = string.Empty;
                    string FltDestination = string.Empty;


                    DataSet dsPAWB = new DataSet();

                    foreach (CarditDetail cd in carditMessage)
                    {
                        strRouteDetails = string.Empty;
                        int MailPcs = 0;
                        decimal MailWeight = 0;
                        if (cd.objCarditCosnsignmentDetail.Count > 0)
                        {
                            MailPcs = cd.objCarditCosnsignmentDetail.Sum(item => item.PackagePcs);
                            MailWeight = cd.objCarditCosnsignmentDetail.Sum(item => item.PackageGrossWeight);
                        }

                        //string[] Param = new string[]{
                        //    "BagNumber",
                        //    "MailType",
                        //    "MailClass",
                        //    "AWBOrigin",
                        //    "AWBDestination",
                        //    "TotalPieces",
                        //    "TotalGrossWeight",
                        //    "FlightNo",
                        //    "FlightOrigin",
                        //    "FlightDestination",
                        //    "MessageArrivalDate",
                        //    "MessageFlightDate",
                        //    "CreatedOn",
                        //    "CreatedBy",
                        //    "PartnerCode",
                        //    "HNDOrgCode",
                        //    "HNDOrgLocSource",
                        //    "HNDOrgLocName",
                        //    "NDdestCode",
                        //    "HNDdestLocSource",
                        //    "HNDdestLocName",
                        //    "PAWB",
                        //    "StageQualifier",
                        //    "ModeOfTransport",
                        //    "MsgSeqNo",
                        //    "MessageFunctionCode"
                        //};
                        //SqlDbType[] ParamSqlType = new SqlDbType[] {
                        //    SqlDbType.VarChar,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.Int,
                        //    SqlDbType.Decimal,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.DateTime,
                        //    SqlDbType.DateTime,
                        //    SqlDbType.DateTime,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.VarChar,
                        //    SqlDbType.Int,
                        //    SqlDbType.VarChar
                        //};
                        //object[] ParamValue = {
                        //    cd.BagNumber,
                        //    cd.MailType == null ? "Cardit" : cd.MailType,
                        //    cd.MailClass,
                        //    cd.AWBOrigin,
                        //    cd.AWBDestination == null ? cd.FlightDestination : cd.AWBDestination,
                        //    //cd.TotalPieces == 0 ? MailPcs : cd.TotalPieces,
                        //    // cd.TotalGrossWeight == 0 ? MailWeight : cd.TotalGrossWeight,
                        //    MailPcs,
                        //    MailWeight,                           
                        //    cd.FlightNo,
                        //    cd.FlightOrigin,
                        //    cd.FlightDestination,
                        //    cd.MessageArrivalDate,
                        //    cd.MessageFlightDate,
                        //    DateTime.Now,
                        //    "CARDIT",
                        //    cd.PartnerCode,
                        //    cd.HNDOrg_Code,
                        //    cd.HNDOrg_LocSource,
                        //    cd.HNDOrg_LocName,
                        //    cd.HNDdest_Code,
                        //    cd.HNDdest_LocSource,
                        //    cd.HNDdest_LocName,
                        //    cd.PAWBNo == null ? "" : cd.PAWBPrefix + "-" + cd.PAWBNo,
                        //    cd.StageQualifier,
                        //    cd.ModeOfTransport,
                        //    cd.MsgSeqNo,
                        //    cd.MessageFunctionCode
                        //};

                        SqlParameter[] parameters =
                        {
                            new SqlParameter("@BagNumber", SqlDbType.VarChar) { Value = cd.BagNumber },
                            new SqlParameter("@MailType", SqlDbType.VarChar) { Value = cd.MailType ?? "Cardit" },
                            new SqlParameter("@MailClass", SqlDbType.VarChar) { Value = cd.MailClass },
                            new SqlParameter("@AWBOrigin", SqlDbType.VarChar) { Value = cd.AWBOrigin },
                            new SqlParameter("@AWBDestination", SqlDbType.VarChar) { Value = cd.AWBDestination ?? cd.FlightDestination },
                            new SqlParameter("@TotalPieces", SqlDbType.Int) { Value = MailPcs },
                            new SqlParameter("@TotalGrossWeight", SqlDbType.Decimal) { Value = MailWeight },
                            new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = cd.FlightNo },
                            new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = cd.FlightOrigin },
                            new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = cd.FlightDestination },
                            new SqlParameter("@MessageArrivalDate", SqlDbType.DateTime) { Value = cd.MessageArrivalDate },
                            new SqlParameter("@MessageFlightDate", SqlDbType.DateTime) { Value = cd.MessageFlightDate },
                            new SqlParameter("@CreatedOn", SqlDbType.DateTime) { Value = DateTime.Now },
                            new SqlParameter("@CreatedBy", SqlDbType.VarChar) { Value = "CARDIT" },
                            new SqlParameter("@PartnerCode", SqlDbType.VarChar) { Value = cd.PartnerCode },
                            new SqlParameter("@HNDOrgCode", SqlDbType.VarChar) { Value = cd.HNDOrg_Code },
                            new SqlParameter("@HNDOrgLocSource", SqlDbType.VarChar) { Value = cd.HNDOrg_LocSource },
                            new SqlParameter("@HNDOrgLocName", SqlDbType.VarChar) { Value = cd.HNDOrg_LocName },
                            new SqlParameter("@NDdestCode", SqlDbType.VarChar) { Value = cd.HNDdest_Code },
                            new SqlParameter("@HNDdestLocSource", SqlDbType.VarChar) { Value = cd.HNDdest_LocSource },
                            new SqlParameter("@HNDdestLocName", SqlDbType.VarChar) { Value = cd.HNDdest_LocName },
                            new SqlParameter("@PAWB", SqlDbType.VarChar) { Value = cd.PAWBNo == null ? "" : $"{cd.PAWBPrefix}-{cd.PAWBNo}" },
                            new SqlParameter("@StageQualifier", SqlDbType.VarChar) { Value = cd.StageQualifier },
                            new SqlParameter("@ModeOfTransport", SqlDbType.VarChar) { Value = cd.ModeOfTransport },
                            new SqlParameter("@MsgSeqNo", SqlDbType.Int) { Value = cd.MsgSeqNo },
                            new SqlParameter("@MessageFunctionCode", SqlDbType.VarChar) { Value = cd.MessageFunctionCode }
                        };

                        //SQLServer db = new SQLServer();
                        //CarditID = db.GetIntegerByProcedure("dbo.SaveCarditMessage", Param, ParamValue, ParamSqlType);

                        CarditID = await _readWriteDao.GetIntegerByProcedureAsync("dbo.SaveCarditMessage", parameters);
                        if (CarditID < 1)
                        {
                            //clsLog.WriteLogAzure("Error saving CarditMessage:" + db.LastErrorDescription);
                            _logger.LogWarning("Error on saving in CarditMessage");
                        }
                        else
                        {
                            foreach (CarditCosnsignmentDetail cPg in cd.objCarditCosnsignmentDetail)
                            {
                                //string[] RName = new string[]{
                                //    "CarditID",
                                //    "PackageId",
                                //    "MessageID",
                                //    "PackageGrossWeight",
                                //    "PackageWeightCode",
                                //    "ConsID",
                                //    "AWBPrefix",
                                //    "AWBNumber",
                                //    "ReceptacleType",
                                //    "ReceptacleHndlingClass"
                                //};
                                //SqlDbType[] RType = new SqlDbType[] {
                                //    SqlDbType.Int,
                                //    SqlDbType.VarChar,
                                //    SqlDbType.VarChar,
                                //    SqlDbType.Decimal,
                                //    SqlDbType.VarChar,
                                //    SqlDbType.VarChar,
                                //    SqlDbType.VarChar,
                                //    SqlDbType.VarChar,
                                //    SqlDbType.VarChar,
                                //    SqlDbType.VarChar
                                //};
                                //object[] RValues = { 
                                //    CarditID,
                                //    cPg.PackegeId,
                                //    cd.MessageRefNumber,
                                //    cPg.PackageGrossWeight,
                                //    cPg.PackageWeightCode,
                                //    cd.BagNumber,
                                //    cd.PAWBPrefix,
                                //    cd.PAWBNo,
                                //    cPg.ReceptacleType,
                                //    cPg.ReceptacleHndlingClass
                                //};
                                //int SuccessSNo = db.GetIntegerByProcedure("dbo.SaveCarditPackageInformation", RName, RValues, RType);

                                SqlParameter[] parametersR =
                                {
                                    new SqlParameter("@CarditID", SqlDbType.Int) { Value = CarditID },
                                    new SqlParameter("@PackageId", SqlDbType.VarChar) { Value = cPg.PackegeId },
                                    new SqlParameter("@MessageID", SqlDbType.VarChar) { Value = cd.MessageRefNumber },
                                    new SqlParameter("@PackageGrossWeight", SqlDbType.Decimal) { Value = cPg.PackageGrossWeight },
                                    new SqlParameter("@PackageWeightCode", SqlDbType.VarChar) { Value = cPg.PackageWeightCode },
                                    new SqlParameter("@ConsID", SqlDbType.VarChar) { Value = cd.BagNumber },
                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = cd.PAWBPrefix },
                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = cd.PAWBNo },
                                    new SqlParameter("@ReceptacleType", SqlDbType.VarChar) { Value = cPg.ReceptacleType },
                                    new SqlParameter("@ReceptacleHndlingClass", SqlDbType.VarChar) { Value = cPg.ReceptacleHndlingClass }
                                };


                                int SuccessSNo = await _readWriteDao.GetIntegerByProcedureAsync("dbo.SaveCarditPackageInformation", parametersR);
                                if (SuccessSNo < 1)
                                    //clsLog.WriteLogAzure("Error saving tblpomcontCARDIT Table :" + db.LastErrorDescription);
                                    _logger.LogWarning("Error on saving in tblpomcontCARDIT Table");
                            }

                            for (int lstIndex = 0; lstIndex < fltroute.Length; lstIndex++)
                            {

                                if (cd.BagNumber == fltroute[lstIndex].BagNumber)
                                {

                                    //    string[] RName1 = new string[]{
                                    //    "CarditID",
                                    //    "PartnerCode",
                                    //    "FlightOrigin",
                                    //    "FlightDestination",
                                    //    "FlightNo",
                                    //    "FlightDate",
                                    //    "PiecesCount",
                                    //    "StageQualifier",
                                    //    "ModeOfTransport",
                                    //    "TotalGrossWeight"
                                    //};
                                    //    SqlDbType[] RType1 = new SqlDbType[] {
                                    //    SqlDbType.Int,
                                    //    SqlDbType.VarChar,
                                    //    SqlDbType.VarChar,
                                    //    SqlDbType.VarChar,
                                    //    SqlDbType.VarChar,
                                    //    SqlDbType.DateTime,
                                    //    SqlDbType.Int,
                                    //    SqlDbType.VarChar,
                                    //    SqlDbType.VarChar,
                                    //    SqlDbType.Decimal

                                    //};
                                    //    object[] RValues1 = {
                                    //    CarditID,
                                    //    strDesignatorCode,
                                    //    fltroute[lstIndex].fltdept,
                                    //    fltroute[lstIndex].fltarrival,
                                    //    fltroute[lstIndex].fltnum,
                                    //    flightRoutedate[lstIndex].FltDate,
                                    //  //  cd.TotalPieces,
                                    //  MailPcs,
                                    //    "",
                                    //    "",
                                    //  //  cd.TotalGrossWeight
                                    //  MailWeight
                                    //};

                                    SqlParameter[] parametersRR =
                                    {
                                        new SqlParameter("@CarditID", SqlDbType.Int) { Value = CarditID },
                                        new SqlParameter("@PartnerCode", SqlDbType.VarChar) { Value = strDesignatorCode },
                                        new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = fltroute[lstIndex].fltdept },
                                        new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = fltroute[lstIndex].fltarrival },
                                        new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = fltroute[lstIndex].fltnum },
                                        new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = flightRoutedate[lstIndex].FltDate },
                                        new SqlParameter("@PiecesCount", SqlDbType.Int) { Value = MailPcs },
                                        new SqlParameter("@StageQualifier", SqlDbType.VarChar) { Value = "" },
                                        new SqlParameter("@ModeOfTransport", SqlDbType.VarChar) { Value = "" },
                                        new SqlParameter("@TotalGrossWeight", SqlDbType.Decimal) { Value = MailWeight }
                                     };
                                    if (fltroute[lstIndex].fltnum.Substring(0, 2) == strDesignatorCode)
                                    {

                                        if (!(lstIndex > 0 && cd.BagNumber == fltroute[lstIndex - 1].BagNumber))
                                        {
                                            FltorgNumber = fltroute[lstIndex].fltdept;
                                        }

                                        FltDestination = fltroute[lstIndex].fltarrival;


                                        strRouteDetails = strRouteDetails + "@" + fltroute[lstIndex].fltnum + "," + fltroute[lstIndex].fltdept + "," + fltroute[lstIndex].fltarrival + "," + "0" + "," + flightRoutedate[lstIndex].FltDate + ",";
                                        //SQLServer db1 = new SQLServer();
                                        FlgRouteID = await _readWriteDao.GetIntegerByProcedureAsync("dbo.SaveCarditRouteInfo", parametersRR);
                                        if (FlgRouteID < 1)
                                        {
                                            //clsLog.WriteLogAzure("Error saving SaveCarditRouteInfo:" + db.LastErrorDescription);
                                            _logger.LogInformation("Error on saving SaveCarditRouteInfo:");
                                        }
                                    }
                                }
                            }


                            #region : Saving AWBInformation :
                            if (!string.IsNullOrEmpty(cd.PAWBNo))
                            {
                                DataSet dsAgent = new DataSet();
                                dsAgent = await GetAgentCode(cd.AWBOrigin, DateTime.Now);
                                if (dsAgent != null && dsAgent.Tables.Count > 0 && dsAgent.Tables[0].Rows.Count > 0 && dsAgent.Tables[0].Rows[0][0].ToString() != "")
                                {
                                    AgentCode = dsAgent.Tables[0].Rows[0]["AgentCode"].ToString();
                                    AgentName = dsAgent.Tables[0].Rows[0]["AgentName"].ToString();
                                }

                                #region : Parameters to save the booking data through POMail :
                                //string[] ParamName = new string[]
                                //{
                                //    "AirlinePrefix","AWBNum","Origin","Dest","PcsCount","Weight","Volume","ComodityCode","ComodityDesc","CarrierCode","FlightNum",
                                //    "FlightDate","FlightOrigin","FlightDest","ShipperName","ShipperAddr","ShipperPlace","ShipperState","ShipperCountryCode","ShipperContactNo",
                                //    "ConsName","ConsAddr","ConsPlace","ConsState","ConsCountryCode","ConsContactNo","CustAccNo","IATACargoAgentCode","CustName","SystemDate",
                                //    "MeasureUnit","Length","Breadth","Height","PartnerStatus","REFNo","UpdatedBy","SpecialHandelingCode","ChargeableWeight","AgentCode","AgentName"
                                //};
                                //object[] ParamValues = new object[]
                                //{
                                //    cd.PAWBPrefix,cd.PAWBNo,cd.AWBOrigin,cd.AWBDestination == null ? cd.FlightDestination : cd.AWBDestination,MailPcs,MailWeight,"0","POMAIL-EMS",
                                //    "POMAIL-EMS","","","","","","","","","","","","","","","","","","","","", DateTime.Now,
                                //    "","","","","",0,"CARDIT","",Convert.ToDecimal(MailWeight),AgentCode,AgentName
                                //};
                                //SqlDbType[] ParamType = new SqlDbType[]
                                //{
                                //    SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,
                                //    SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,
                                //    SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,
                                //    SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.DateTime,SqlDbType.VarChar,SqlDbType.VarChar,
                                //    SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.Int,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.Decimal,SqlDbType.VarChar,
                                //    SqlDbType.VarChar
                                //};

                                SqlParameter[] ParamName =
                                {
                                    new SqlParameter("@AirlinePrefix", SqlDbType.VarChar) { Value = cd.PAWBPrefix },
                                    new SqlParameter("@AWBNum", SqlDbType.VarChar) { Value = cd.PAWBNo },
                                    new SqlParameter("@Origin", SqlDbType.VarChar) { Value = cd.AWBOrigin },
                                    new SqlParameter("@Dest", SqlDbType.VarChar) { Value = cd.AWBDestination ?? cd.FlightDestination },
                                    new SqlParameter("@PcsCount", SqlDbType.VarChar) { Value = MailPcs },
                                    new SqlParameter("@Weight", SqlDbType.VarChar) { Value = MailWeight },
                                    new SqlParameter("@Volume", SqlDbType.VarChar) { Value = "0" },
                                    new SqlParameter("@ComodityCode", SqlDbType.VarChar) { Value = "POMAIL-EMS" },
                                    new SqlParameter("@ComodityDesc", SqlDbType.VarChar) { Value = "POMAIL-EMS" },
                                    new SqlParameter("@CarrierCode", SqlDbType.VarChar) { Value = "" },
                                    new SqlParameter("@FlightNum", SqlDbType.VarChar) { Value = "" },
                                    new SqlParameter("@FlightDate", SqlDbType.VarChar) { Value = "" },
                                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = "" },
                                    new SqlParameter("@FlightDest", SqlDbType.VarChar) { Value = "" },
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
                                    new SqlParameter("@REFNo", SqlDbType.VarChar) { Value = 0 },
                                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "CARDIT" },
                                    new SqlParameter("@SpecialHandelingCode", SqlDbType.VarChar) { Value = "" },
                                    new SqlParameter("@ChargeableWeight", SqlDbType.Decimal) { Value = Convert.ToDecimal(MailWeight) },
                                    new SqlParameter("@AgentCode", SqlDbType.VarChar) { Value = AgentCode },
                                    new SqlParameter("@AgentName", SqlDbType.VarChar) { Value = AgentName }
                                    };

                                #endregion Parameters to save the booking data through POMail

                                string ProcedureName = "spInsertBookingDataPOMAIL";
                                //QID.DataAccess.SQLServer sqlServer = new QID.DataAccess.SQLServer();
                                //bool flag = sqlServer.InsertData(ProcedureName, ParamName, ParamType, ParamValues);

                                bool flag = await _readWriteDao.ExecuteNonQueryAsync(ProcedureName, ParamName);

                                if (flag)
                                {
                                    //string[] parname = new string[] { "AWBNum", "AWBPrefix" };
                                    //object[] parobject = new object[] { cd.PAWBNo, cd.PAWBPrefix };
                                    //SqlDbType[] partype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };

                                    SqlParameter[] paramAWBNum =
                                    {
                                        new SqlParameter("@AWBNum", SqlDbType.VarChar) { Value = cd.PAWBNo },
                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = cd.PAWBPrefix }
                                    };
                                    if (await _readWriteDao.ExecuteNonQueryAsync("spDeleteAWBRouteFFR", paramAWBNum))
                                    {
                                        string strOrigin = cd.AWBOrigin;
                                        string strdestination = cd.AWBDestination == null ? cd.FlightDestination : cd.AWBDestination;
                                        string strFltNumber = cd.FlightNo;
                                        DateTime dtFlightDate = cd.MessageFlightDate;
                                        if (flag)
                                        {
                                            #region : Parameters to save Route Information :
                                            //string[] ParamNames = new string[]
                                            //        {
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
                                            //            "AWBPrefix"
                                            //        };
                                            //SqlDbType[] DataTypes = new SqlDbType[]
                                            //        {
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
                                            //            SqlDbType.VarChar
                                            //        };
                                            //object[] ParamValuesRoute = new object[]
                                            //                {

                                            //                    cd.PAWBNo,
                                            //                    strOrigin,
                                            //                    strdestination,
                                            //                    strFltNumber,
                                            //                    dtFlightDate.ToShortDateString(),
                                            //                    "C",
                                            //                    "CARDIT",
                                            //                    DateTime.Now,
                                            //                    0,
                                            //                    0,
                                            //                    dtFlightDate.ToShortDateString(),
                                            //                    cd.PAWBPrefix
                                            //                };

                                            SqlParameter[] ParamValuesRoute =
                                            {
                                                new SqlParameter("@AWBNumber", SqlDbType.VarChar)      { Value = cd.PAWBNo },
                                                new SqlParameter("@FltOrigin", SqlDbType.VarChar)      { Value = strOrigin },
                                                new SqlParameter("@FltDestination", SqlDbType.VarChar) { Value = strdestination },
                                                new SqlParameter("@FltNumber", SqlDbType.VarChar)      { Value = strFltNumber },
                                                new SqlParameter("@FltDate", SqlDbType.DateTime)       { Value = dtFlightDate },
                                                new SqlParameter("@Status", SqlDbType.VarChar)         { Value = "C" },
                                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar)      { Value = "CARDIT" },
                                                new SqlParameter("@UpdatedOn", SqlDbType.DateTime)     { Value = DateTime.Now },
                                                new SqlParameter("@IsFFR", SqlDbType.Bit)             { Value = 0 },
                                                new SqlParameter("@REFNo", SqlDbType.Int)             { Value = 0 },
                                                new SqlParameter("@date", SqlDbType.DateTime)         { Value = dtFlightDate },
                                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar)     { Value = cd.PAWBPrefix }
                                            };

                                            #endregion Parameters to save Route Information

                                            if (!await _readWriteDao.ExecuteNonQueryAsync("spSaveFFRAWBRoute", ParamValuesRoute))
                                                //clsLog.WriteLogAzure("Error saving spSaveFFRAWBRoute :" + db.LastErrorDescription);

                                                _logger.LogWarning("Error on saving spSaveFFRAWBRoute");
                                            else
                                                //clsLog.WriteLogAzure("Data saving Sucessfully spSaveFFRAWBRoute :" + db.LastErrorDescription);
                                                _logger.LogInformation("Data saving Sucessfully spSaveFFRAWBRoute ");
                                            #region Calculate Rate Process
                                            try
                                            {
                                                //string[] ParamName1 = new string[]
                                                //{
                                                //    "AWBNumber","AWBPrefix","UpdatedBy","UpdatedOn","ValidateMin","UpdateBooking","RouteFrom","UpdateBilling"
                                                //};

                                                //object[] ParamValue1 = new object[]
                                                //{
                                                //    cd.PAWBNo,cd.PAWBPrefix,"CARDIT",DateTime.Now,1,1,"B",0
                                                //};
                                                //SqlDbType[] ParamType1 = new SqlDbType[]
                                                //{
                                                //    SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.DateTime,SqlDbType.Bit,SqlDbType.Bit,SqlDbType.VarChar,SqlDbType.Bit
                                                //};

                                                SqlParameter[] ParamValue1 =
                                                {
                                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar)   { Value = cd.PAWBNo },
                                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar)   { Value = cd.PAWBPrefix },
                                                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar)   { Value = "CARDIT" },
                                                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime)  { Value = DateTime.Now },
                                                    new SqlParameter("@ValidateMin", SqlDbType.Bit)     { Value = 1 },
                                                    new SqlParameter("@UpdateBooking", SqlDbType.Bit)   { Value = 1 },
                                                    new SqlParameter("@RouteFrom", SqlDbType.VarChar)   { Value = "B" },
                                                    new SqlParameter("@UpdateBilling", SqlDbType.Bit)   { Value = 0 }
                                                };

                                                if (!await _readWriteDao.ExecuteNonQueryAsync("sp_CalculateAWBRatesReprocess", ParamValue1))
                                                    // clsLog.WriteLogAzure("Error saving sp_CalculateAWBRatesReprocess :" + db.LastErrorDescription);
                                                    _logger.LogWarning("Error on saving sp_CalculateAWBRatesReprocess");
                                                else
                                                    //clsLog.WriteLogAzure("Data saved in sp_CalculateAWBRatesReprocess :" + db.LastErrorDescription);
                                                    _logger.LogInformation("Data saved in sp_CalculateAWBRatesReprocess");
                                            }
                                            catch (Exception ex)
                                            {
                                                //clsLog.WriteLogAzure("Data saved in sp_CalculateAWBRatesReprocess :" + ex.InnerException);
                                                _logger.LogError(ex, "Data saved in sp_CalculateAWBRatesReprocess :");
                                            }
                                            #endregion Calculate Rate Process

                                        }
                                    }
                                }
                            }
                            #endregion Saving AWBInformation
                        }

                        string strAWBOrg = string.Empty;
                        string strAWBDest = string.Empty;
                        string AWBNo = string.Empty;

                        string FltOrg = String.Empty;

                        FltOrg = cd.FlightOrigin;

                        strAWBOrg = cd.AWBOrigin;
                        strAWBDest = cd.AWBDestination == null ? cd.FlightDestination : cd.AWBDestination;

                        if (!string.IsNullOrEmpty(strAWBOrg) && !string.IsNullOrEmpty(strAWBDest))
                        {
                            if (strAWBOrg.Length > 3)
                            {
                                strAWBOrg = strAWBOrg.Substring(2, 3);
                            }

                            if (strAWBDest.Length > 3)
                            {
                                strAWBDest = strAWBDest.Substring(2, 3);
                            }


                            dsPAWB = await GenerateMailPAWB(cd.BagNumber, FltorgNumber, FltDestination, "0518", "INTL POMAIL", MailPcs, MailWeight, strRouteDetails, "MAL", "POMailList", "POMailList");


                            ErrorDesc = string.Empty;
                            if (dsPAWB != null && dsPAWB.Tables.Count > 0 && dsPAWB.Tables[0].Rows.Count > 0)
                            {
                                if (dsPAWB.Tables[0].Columns.Contains("ErrorDesc"))
                                {
                                    ErrorDesc = Convert.ToString(dsPAWB.Tables[0].Rows[0]["ErrorDesc"]);
                                    if (ErrorDesc.Length > 1)
                                    {
                                        status = false;
                                        //clsLog.WriteLogAzure(ErrorDesc + db.LastErrorDescription);
                                        _logger.LogWarning(ErrorDesc);
                                    }
                                }

                                string awbNumber = string.Empty;
                                DataSet dsAWB = new DataSet();
                                dsPAWB = setTableNameToDataSetTable(dsPAWB);
                                try
                                {
                                    AWBNo = Convert.ToString(dsPAWB.Tables["tblResult"].Rows[0]["AWBNumber"]);
                                    if (AWBNo != "" && AWBNo != "0")
                                    {
                                        status = true;
                                        dsAWB = await UpdatePAWBToConsignment(Convert.ToString(cd.BagNumber), "MAL", AWBNo);

                                    }
                                }
                                catch (Exception)
                                {
                                    AWBNo = "";
                                }
                            }
                        }
                        else
                        {
                            //GenericFunction gf = new GenericFunction();                           

                            await _genericFunction.UpdateErrorMessageToInbox(Srno, "Bag# " + cd.BagNumber + " orgin or destination not valid.");

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                ErrorDesc = ex.ToString();
                status = false;
                //clsLog.WriteLogAzure("Error saving Ssaving Cardit Message :" + ex.ToString());
                _logger.LogError(ex, "Error on saving Ssaving Cardit Message");

            }
            //Errormsg = ErrorDesc;
            //return status;
            return (status, ErrorDesc);
        }


        public async Task<bool> EncodeAndSaveResditMessage(string strMessage)
        {
            bool status = false;
            CarditDetail cardit = new CarditDetail();
            CarditCosnsignmentDetail Package = new CarditCosnsignmentDetail();
            var packgeConsignment = new List<CarditCosnsignmentDetail>();
            var carditMessage = new List<CarditDetail>();
            bool checkPackge = false;

            try
            {

                string[] splitmessage = strMessage.Replace("'", "$").Split('$');
                if (splitmessage.Length > 1)
                {
                    for (int K = 0; K < splitmessage.Length; K++)
                    {
                        if (splitmessage[K].ToString().Contains("UNB"))
                        {
                            string[] strinUNBTag = splitmessage[0].Split('+');
                            cardit.MessageHeader = strinUNBTag[0];
                        }
                        //if (splitmessage[K].Length > 2 && splitmessage[K].Contains("UNH"))
                        //{
                        //    string[] strMessageTag = splitmessage[K].Split('+');
                        //    if (strMessageTag.Length > 2)
                        //    {
                        //        string[] strMessageType = strMessageTag[2].Split(':');
                        //        if (strMessageType.Length >= 1)
                        //        {
                        //            cardit.MessageType = strMessageType[0];
                        //            cardit.MessageVersion = strMessageType[1];
                        //        }
                        //    }
                        //}

                        if (splitmessage[K].Length > 2 && splitmessage[K].Contains("STS++"))
                        {
                            string[] strMessageTag = splitmessage[K].Split('+');
                            if (strMessageTag.Length > 2)
                            {
                                string[] strMessageType = strMessageTag[2].Split(':');
                                if (strMessageType.Length >= 1)
                                {
                                    cardit.MessageType = strMessageType[0];
                                    if (strMessageType[0] == "74")
                                    {
                                        cardit.MessageType = "RESDIT74";
                                    }
                                    else if (strMessageType[0] == "24")
                                    {
                                        cardit.MessageType = "RESDIT24";
                                    }
                                    else if (strMessageType[0] == "21")
                                    {
                                        cardit.MessageType = "RESDIT21";
                                    }
                                    else if (strMessageType[0] == "40")
                                    {
                                        cardit.MessageType = "RESDIT40";
                                    }

                                }
                            }
                        }

                        if (K == 3 && splitmessage[K].Length > 3 && splitmessage[K].Contains("CNI++"))
                        {
                            string[] strBGNTag = splitmessage[K].Split('+');
                            if (strBGNTag.Length > 1)
                                cardit.BagNumber = strBGNTag[2];
                        }

                        if (K == 3 && splitmessage[K].Length > 3 && splitmessage[K].Contains("DTM"))
                        {
                            string[] strinDTMTag = splitmessage[K].Split('+');
                            if (strinDTMTag.Length > 1)
                            {
                                string[] strDatePart = strinDTMTag[1].Split(':');
                                string Year = strDatePart[1].Substring(0, 2);
                                string Month = strDatePart[1].Substring(2, 2);
                                string Date = strDatePart[1].Substring(4, 2);
                                cardit.MessageArrivalDate = DateTime.Parse("20" + Year + "/" + Month + "/" + Date);
                            }
                        }

                        if (splitmessage[K].Length > 5 && splitmessage[K].Contains("LOC+48"))
                        {
                            string[] strinLOCTag = splitmessage[K].Split('+');
                            if (strinLOCTag.Length > 1)
                            {
                                string[] strFlightorigin = strinLOCTag[2].Split(':');
                                if (strFlightorigin.Length > 1)
                                    cardit.HNDOrg_Code = strFlightorigin[0].ToString();
                            }
                        }

                        if (K == 7 && splitmessage[K].Length > 5 && splitmessage[K].Contains("LOC") && cardit.MessageType == "RESDIT24")
                        {
                            string[] strinLOCTag = splitmessage[K].Split('+');
                            if (strinLOCTag.Length > 1)
                            {
                                string[] strFlightorigin = strinLOCTag[2].Split(':');
                                if (strFlightorigin.Length > 1)
                                    cardit.AWBOrigin = strFlightorigin[0].ToString();
                            }
                        }

                        if (K == 9 && splitmessage[K].Length > 5 && splitmessage[K].Contains("LOC") && cardit.MessageType == "RESDIT21")
                        {
                            string[] strinLOCTag = splitmessage[K].Split('+');
                            if (strinLOCTag.Length > 1)
                            {
                                string[] strFlightorigin = strinLOCTag[2].Split(':');
                                if (strFlightorigin.Length > 1)
                                    cardit.AWBOrigin = strFlightorigin[0].ToString();
                            }
                        }


                        if (K == 14 && splitmessage[K].Length > 5 && splitmessage[K].Contains("LOC"))
                        {
                            string[] strinLOCTag = splitmessage[K].Split('+');
                            if (strinLOCTag.Length > 1)
                            {
                                for (int i = 0; i < strinLOCTag.Length; i++)
                                {

                                    if (strinLOCTag[i].Contains(":"))
                                    {
                                        string[] strFlightorigin = strinLOCTag[i].Split(':');
                                        cardit.FlightOrigin = strFlightorigin[1].ToString();
                                    }
                                }
                            }
                        }

                        if (K == 15 && splitmessage[K].Length > 5 && splitmessage[K].Contains("LOC"))
                        {
                            string[] strinLOCTag = splitmessage[K].Split('+');

                            for (int i = 0; i < strinLOCTag.Length; i++)
                            {
                                if (strinLOCTag[i].Contains(":"))
                                {
                                    string[] strFlightDestination = strinLOCTag[i].Split(':');
                                    cardit.FlightDestination = strFlightDestination[1].ToString();
                                }
                            }

                        }

                        if (splitmessage[K].Length > 5 && splitmessage[K].Contains("DTM+7"))
                        {
                            string[] strinLOCTag = splitmessage[K].Trim().Split(':');
                            if (strinLOCTag.Length > 1)
                            {
                                string[] strDatePart = strinLOCTag[1].Split(':');

                                string Year = strDatePart[0].Substring(0, 2);
                                string Month = strDatePart[0].Substring(2, 2);
                                string Date = strDatePart[0].Substring(4, 2);
                                // cardit.MessageFlightDate = DateTime.Parse(Year + "/" + Month + "/" + Date);
                                cardit.MessageFlightDate = DateTime.Parse(Month + "/" + Date + "/" + Year);
                            }
                        }

                        if (splitmessage[K].Length > 2 && (splitmessage[K].Contains("GID") || splitmessage[K].Contains("ID") || splitmessage[K].Contains("PCI") || splitmessage[K].Contains("CNI") || splitmessage[K].Contains("MEA") || splitmessage[K].Contains("DOC")))
                        {

                            if (splitmessage[K].Contains("GID") || splitmessage[K].Contains("ID"))
                            {

                                int cntPCS = 1;
                                string[] strGIDTag = splitmessage[K].Split('+');
                                if (strGIDTag.Length > 2)

                                    Package.PackagePcs = cntPCS++;
                            }

                            if (splitmessage[K].Contains("PCI"))
                            {
                                if (!checkPackge)
                                {
                                    checkPackge = true;
                                    string[] strPackageID = splitmessage[K].ToString().Split('+');
                                    if (strPackageID.Length > 2)
                                    {
                                        string[] strCarditPackage = strPackageID[2].Split(':');
                                        //if (K == 8 && splitmessage[K].Length > 5 && cardit.MessageType == "RESDIT40")
                                        //{

                                        //}
                                        if (strCarditPackage.Length > 0)
                                        {
                                            Package.PackegeId = strCarditPackage[0].ToString();
                                            //Package.PackegeId = strCarditPackage[1].ToString();
                                        }
                                    }
                                }
                                else
                                {
                                    packgeConsignment.Add(Package);
                                    Package = new CarditCosnsignmentDetail();
                                    string[] strPackageID = splitmessage[K].ToString().Split('+');
                                    if (strPackageID.Length > 2)
                                    {
                                        string[] strCarditPackage = strPackageID[2].Split(':');
                                        if (strCarditPackage.Length > 0)
                                        {
                                            Package.PackegeId = strCarditPackage[1].ToString();
                                        }
                                    }
                                }
                            }

                        }



                        if (K > 10 && cardit.MessageType == "RESDIT74" && splitmessage[K].Contains(":"))
                        {
                            packgeConsignment.Add(Package);
                            Package = new CarditCosnsignmentDetail();
                            string[] strPackageID = splitmessage[K].ToString().Split(':');
                            if (strPackageID.Length > 1)
                            {
                                string[] strCarditPackage = strPackageID[1].Split(':');
                                if (strCarditPackage.Length > 0)
                                {
                                    Package.PackegeId = strCarditPackage[0].ToString();
                                }
                            }
                        }
                        else
                        {
                            if (K > 12 && splitmessage[K].Length > 1 && splitmessage[K].Contains(":"))
                            {
                                packgeConsignment.Add(Package);
                                Package = new CarditCosnsignmentDetail();
                                string[] strPackageID = splitmessage[K].ToString().Split(':');
                                if (strPackageID.Length > 1)
                                {
                                    string[] strCarditPackage = strPackageID[1].Split(':');
                                    if (strCarditPackage.Length > 0)
                                    {
                                        Package.PackegeId = strCarditPackage[0].ToString();
                                    }
                                }
                            }

                            if (K > 8 && splitmessage[K].Length > 1 && splitmessage[K].Contains(":") && cardit.MessageType == "RESDIT40")
                            {
                                packgeConsignment.Add(Package);
                                Package = new CarditCosnsignmentDetail();
                                string[] strPackageID = splitmessage[K].ToString().Split(':');
                                if (strPackageID.Length > 1)
                                {
                                    string[] strCarditPackage = strPackageID[1].Split(':');
                                    if (strCarditPackage.Length > 0)
                                    {
                                        Package.PackegeId = strCarditPackage[0].ToString();
                                    }
                                }
                            }

                        }

                    }
                    packgeConsignment.Add(Package);
                    carditMessage.Add(cardit);
                    //int MailPcs = 0;
                    //decimal MailWeight = 0;
                    //if (packgeConsignment.Count > 0)
                    //{

                    //    MailPcs = packgeConsignment.Sum(item => item.PackagePcs);
                    //    MailWeight = packgeConsignment.Sum(item => item.PackageGrossWeight);

                    //}
                    //SQLServer dbCardit = new SQLServer();
                    //int CarditID = 0;
                    string strConsID = string.Empty;
                    string strRecID = string.Empty;
                    string strEventType = string.Empty;
                    string strMsgStn = string.Empty;
                    DateTime dtCreatedOn = DateTime.Now;

                    foreach (var RecID in packgeConsignment)
                    {
                        string result = RecID.PackegeId;
                        strRecID = strRecID + "," + result;
                        strRecID = strRecID.TrimStart(',');
                    }

                    foreach (CarditDetail Rcd in carditMessage)
                    {
                        strConsID = Rcd.BagNumber;
                        strEventType = Rcd.MessageType;
                        if (strEventType == "RESDIT24" || strEventType == "RESDIT21")
                        {
                            strMsgStn = Rcd.AWBOrigin;
                        }
                        else
                        {
                            strMsgStn = Rcd.HNDOrg_Code;
                        }

                        dtCreatedOn = Rcd.MessageFlightDate;
                    }

                    //string[] Param = new string[]
                    //                                        {   "ConsID",
                    //                                            "Receptacle",
                    //                                            "Event",
                    //                                            "MSGStn",
                    //                                            "CreatedOn",                                                                
                    //                                        };

                    //SqlDbType[] ParamSqlType = new SqlDbType[]
                    //                                        {   SqlDbType.VarChar,
                    //                                            SqlDbType.VarChar,
                    //                                            SqlDbType.VarChar,  
                    //                                            SqlDbType.VarChar,
                    //                                            SqlDbType.DateTime,                                                               
                    //                                        };

                    //object[] ParamValue = { strConsID, strRecID, strEventType, strMsgStn, dtCreatedOn };

                    SqlParameter[] parameters =
                    {
                        new SqlParameter("@ConsID", SqlDbType.VarChar)   { Value = strConsID },
                        new SqlParameter("@Receptacle", SqlDbType.VarChar) { Value = strRecID },
                        new SqlParameter("@Event", SqlDbType.VarChar)    { Value = strEventType },
                        new SqlParameter("@MSGStn", SqlDbType.VarChar)  { Value = strMsgStn },
                        new SqlParameter("@CreatedOn", SqlDbType.DateTime) { Value = dtCreatedOn }
                    };

                    //SQLServer db = new SQLServer();
                    //int Success = db.GetIntegerByProcedure("SaveRESDITPerReceptacle", Param, ParamValue, ParamSqlType);

                    int Success = await _readWriteDao.GetIntegerByProcedureAsync("SaveRESDITPerReceptacle", parameters);

                    if (Success == 0)
                    {
                        //clsLog.WriteLogAzure("Error saving CarditMessage:" + db.LastErrorDescription);
                        _logger.LogWarning("Error on saving CarditMessage:");
                    }
                }
                status = true;
            }
            catch (Exception ex)
            {
                ///SCMExceptionHandling.logexception(ref ex);
                status = false;
                //clsLog.WriteLogAzure("Error saving Ssaving Cardit Message :" + ex.ToString());
                _logger.LogError(ex, "Error on saving Ssaving Cardit Message.");
            }
            return status;
        }

        #region Get Agent Code
        public async Task<DataSet?> GetAgentCode(string origin, DateTime tranDate)
        {
            //SQLServer da = new SQLServer();
            DataSet? objDs = null;

            try
            {
                //string[] pname = new string[2];
                //object[] pvalue = new object[2];
                //SqlDbType[] ptype = new SqlDbType[2];

                //pname[0] = "Station";
                //pname[1] = "TranDate";

                //ptype[0] = SqlDbType.VarChar;
                //ptype[1] = SqlDbType.DateTime;

                //pvalue[0] = origin;
                //pvalue[1] = tranDate;

                SqlParameter[] parameters =
                {
                    new SqlParameter("@Station", SqlDbType.VarChar)  { Value = origin },
                    new SqlParameter("@TranDate", SqlDbType.DateTime) { Value = tranDate }
                };
                //objDs = da.SelectRecords("spGetPOMAILAgentCode", pname, pvalue, ptype);

                objDs = await _readWriteDao.SelectRecords("spGetPOMAILAgentCode", parameters);

                return objDs;
            }
            catch (Exception ex)
            {
                //objDs = null;
                //clsLog.WriteLogAzure("Error saving spGetPOMAILAgentCode :" + ex.ToString());
                _logger.LogError(ex, "Error on saving spGetPOMAILAgentCode");
                // SCMExceptionHandling.logexception(ref ex);
                return null;
            }
            //finally
            //{
            //    if (objDs != null)
            //        objDs.Dispose();
            //}
        }
        #endregion

        public async Task<DataSet?> GetDesigCode()
        {
            //SQLServer da = new SQLServer();
            DataSet? objDs = null;

            try
            {
                //string[] pname = new string[2];
                //object[] pvalue = new object[2];
                //SqlDbType[] ptype = new SqlDbType[2];

                //pname[0] = "Station";
                //pname[1] = "TranDate";

                //ptype[0] = SqlDbType.VarChar;
                //ptype[1] = SqlDbType.DateTime;

                //pvalue[0] = origin;
                //pvalue[1] = tranDate;

                //objDs = da.SelectRecords("uspGetDesignatorCode");

                objDs = await _readWriteDao.SelectRecords("uspGetDesignatorCode");

                return objDs;
            }
            catch (Exception ex)
            {
                objDs = null;
                //clsLog.WriteLogAzure("Error on saving spGetPOMAILAgentCode :" + ex.ToString());
                _logger.LogError(ex, "Error on saving spGetPOMAILAgentCode");
                // SCMExceptionHandling.logexception(ref ex);
                return null;
            }
            finally
            {
                if (objDs != null)
                    objDs.Dispose();
            }
        }

        public async Task<DataSet?> GenerateMailPAWB(string consignmentID, string station, string destination, string commodityCode, string commodityDesc, int pcs, decimal wgt, string fltDetails, string AWBprefix, string LoginName, string callFrom)
        {
            //SQLServer da = new SQLServer();
            DataSet? dsPOMailDetails = new DataSet();

            try
            {
                //string[] param = {
                //    "ConsignmentID"
                //    , "Station"
                //    , "Destination"
                //    , "Commcode"
                //    , "Commdesc"
                //    , "Pieces"
                //    , "TotWgt"
                //    , "MullegFlts"
                //    , "AWBPrefix"
                //    , "LoginName"
                //    , "callFrom"

                //};
                //SqlDbType[] types = {
                //    SqlDbType.VarChar
                //    , SqlDbType.VarChar
                //    , SqlDbType.VarChar
                //    , SqlDbType.VarChar
                //    , SqlDbType.VarChar
                //    , SqlDbType.Int
                //    , SqlDbType.Decimal
                //    , SqlDbType.VarChar
                //    , SqlDbType.VarChar
                //    , SqlDbType.VarChar
                //    , SqlDbType.VarChar
                //    , SqlDbType.VarChar

                //};
                //object[] values = {
                //     consignmentID
                //    , station
                //    , destination
                //    , commodityCode
                //    , commodityDesc
                //    , pcs
                //    , wgt
                //    , fltDetails
                //    , AWBprefix
                //    , LoginName
                //    , callFrom

                //};
                //dsPOMailDetails = da.SelectRecords("dbo.uspSavePAWBMailBooking", param, values, types);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@ConsignmentID", SqlDbType.VarChar) { Value = consignmentID },
                    new SqlParameter("@Station", SqlDbType.VarChar)       { Value = station },
                    new SqlParameter("@Destination", SqlDbType.VarChar)   { Value = destination },
                    new SqlParameter("@Commcode", SqlDbType.VarChar)      { Value = commodityCode },
                    new SqlParameter("@Commdesc", SqlDbType.VarChar)      { Value = commodityDesc },
                    new SqlParameter("@Pieces", SqlDbType.Int)            { Value = pcs },
                    new SqlParameter("@TotWgt", SqlDbType.Decimal)       { Value = wgt },
                    new SqlParameter("@MullegFlts", SqlDbType.VarChar)    { Value = fltDetails },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar)     { Value = AWBprefix },
                    new SqlParameter("@LoginName", SqlDbType.VarChar)     { Value = LoginName },
                    new SqlParameter("@callFrom", SqlDbType.VarChar)      { Value = callFrom }
                };

                dsPOMailDetails = await _readWriteDao.SelectRecords("dbo.uspSavePAWBMailBooking", parameters);

                if (dsPOMailDetails != null && dsPOMailDetails.Tables.Count > 0 && dsPOMailDetails.Tables[0].Rows.Count > 0)
                    return dsPOMailDetails;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure("Data saved in uspSavePAWBMailBooking :" + ex.InnerException);
                _logger.LogError(ex, "Data saved in uspSavePAWBMailBooking");
            }

            return null;
        }

        public DataSet? setTableNameToDataSetTable(DataSet dsTables)
        {
            string sTableName = "";
            if (dsTables != null && dsTables.Tables.Count > 0)
            {
                for (int i = 0; i < dsTables.Tables.Count; i++)
                {
                    try
                    {
                        if (dsTables.Tables[i].Columns.Contains("TABLENAME"))
                        {
                            sTableName = Convert.ToString(dsTables.Tables[i].Rows[0]["TABLENAME"]);
                            if (sTableName != null && sTableName != "")
                            {
                                dsTables.Tables[i].TableName = dsTables.Tables[i].Rows[0]["TABLENAME"].ToString();
                                dsTables.Tables[i].Columns.Remove("TABLENAME");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //clsLog.WriteLogAzure("setTableNameToDataSetTable :" + ex.InnerException);
                        _logger.LogError(ex, "Error on setTableNameToDataSetTable");
                    }
                }
            }

            return dsTables;
        }

        public async Task<DataSet?> UpdatePAWBToConsignment(string ConsignmentID, string AWBPrefix, string AWBNumber)
        {
            //SQLServer da = new SQLServer();
            DataSet? dsPOMailDetails = new DataSet();

            try
            {
                //string[] param = {
                //    "ConsignmentID"
                //    ,"AWBPrefix"
                //    , "AWBNumber"

                //};
                //SqlDbType[] types = {
                //    SqlDbType.VarChar
                //    , SqlDbType.VarChar
                //    , SqlDbType.VarChar
                //};
                //object[] values = {
                //    ConsignmentID
                //    , AWBPrefix
                //    , AWBNumber
                //};

                SqlParameter[] parameters =
                {
                    new SqlParameter("@ConsignmentID", SqlDbType.VarChar) { Value = ConsignmentID },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar)     { Value = AWBPrefix },
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar)     { Value = AWBNumber }
                };

                dsPOMailDetails = await _readWriteDao.SelectRecords("dbo.uspUpdatePAWBConsignment", parameters);

                if (dsPOMailDetails != null && dsPOMailDetails.Tables.Count > 0 && dsPOMailDetails.Tables[0].Rows.Count > 0)
                    return dsPOMailDetails;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure("Data saved in uspUpdatePAWBConsignment :" + ex.InnerException);
                _logger.LogError(ex, "Error on Data saved in uspUpdatePAWBConsignment");
            }

            return null;
        }
    }
}
