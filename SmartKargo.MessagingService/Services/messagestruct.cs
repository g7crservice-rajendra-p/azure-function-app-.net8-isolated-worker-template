using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
namespace QidWorkerRole
{
    public class MessageData
    {
        public struct ffrinfo
        {
            public string ffrversionnum;

            //4 
            public string noofuld;
            //5 
            public string specialservicereq1;
            public string specialservicereq2;
            //6 
            public string otherserviceinfo1;
            public string otherserviceinfo2;
            //7
            public string bookingrefairport;
            public string officefundesignation;
            public string companydesignator;
            public string bookingfileref;
            public string participentidetifier;
            public string participentcode;
            public string participentairportcity;

            //9-productinfo
            public string servicecode;
            public string rateclasscode;
            public string commoditycode;
            public string producttype;
            //10 shipper info
            public string shipperaccnum;
            public string shippername;
            public string shipperadd;
            public string shipperplace;
            public string shipperstate;
            public string shippercountrycode;
            public string shipperpostcode;
            public string shippercontactidentifier;
            public string shippercontactnum;

            //11 consinee
            public string consaccnum;
            public string consname;
            public string consadd;
            public string consplace;
            public string consstate;
            public string conscountrycode;
            public string conspostcode;
            public string conscontactidentifier;
            public string conscontactnum;

            //12 customer identification
            public string custaccnum;
            public string iatacargoagentcode;
            public string cargoagentcasscode;
            public string custparticipentidentifier;
            public string custname;
            public string custplace;

            //13 shipment refernece info
            public string shiprefnum;
            public string supplemetryshipperinfo1;
            public string supplemetryshipperinfo2;

            public ffrinfo(string val)
            {
                ffrversionnum = val;

                //4 
                noofuld = val;
                //5 
                specialservicereq1 = val;
                specialservicereq2 = val;
                //6 
                otherserviceinfo1 = val;
                otherserviceinfo2 = val;
                //7
                bookingrefairport = val;
                officefundesignation = val;
                companydesignator = val;
                bookingfileref = val;
                participentidetifier = val;
                participentcode = val;
                participentairportcity = val;

                //9-productinfo
                servicecode = val;
                rateclasscode = val;
                commoditycode = val;
                producttype = val;
                //10 shipper info
                shipperaccnum = val;
                shippername = val;
                shipperadd = val;
                shipperplace = val;
                shipperstate = val;
                shippercountrycode = val;
                shipperpostcode = val;
                shippercontactidentifier = val;
                shippercontactnum = val;

                //11 consinee
                consaccnum = val;
                consname = val;
                consadd = val;
                consplace = val;
                consstate = val;
                conscountrycode = val;
                conspostcode = val;
                conscontactidentifier = val;
                conscontactnum = val;

                //12 customer identification
                custaccnum = val;
                iatacargoagentcode = val;
                cargoagentcasscode = val;
                custparticipentidentifier = val;
                custname = val;
                custplace = val;

                //13 shipment refernece info
                shiprefnum = val;
                supplemetryshipperinfo1 = val;
                supplemetryshipperinfo2 = val;
            }

        }

        public struct ffainfo
        {
            public string ffaversionnum;

            //2 consignment details
            public string airlineprefix;
            public string awbnum;
            public string origin;
            public string dest;
            public string consigntype;
            public string pcscnt;
            public string weightcode;
            public string weight;
            public string shpdesccode;
            public string numshp;
            public string manifestdesc;
            public string splhandling;
            //3 flight details
            public string carriercode;
            public string fltnum;
            public string date;
            public string month;
            public string fltdept;
            public string fltarrival;
            public string spaceallotmentcode;
            //4 
            public string specialservicereq1;
            public string specialservicereq2;
            //5 
            public string otherserviceinfo1;
            public string otherserviceinfo2;
            //6
            public string originsitaaddress;
            public string bookingrefairport;
            public string officefundesignation;
            public string companydesignator;
            public string bookingfileref;
            public string participentidetifier;
            public string participentcode;
            public string participentairportcity;

            //7 shipment refernece info
            public string shiprefnum;
            public string supplemetryshipperinfo1;
            public string supplemetryshipperinfo2;
            #region Constructor
            public ffainfo(string val)
            {
                ffaversionnum = val;
                //2 consignment details
                airlineprefix = val;
                awbnum = val;
                origin = val;
                dest = val;
                consigntype = val;
                pcscnt = val;
                weightcode = val;
                weight = val;
                shpdesccode = val;
                numshp = val;
                manifestdesc = val;
                splhandling = val;
                //3 flight details
                carriercode = val;
                fltnum = val;
                date = val;
                month = val;
                fltdept = val;
                fltarrival = val;
                spaceallotmentcode = val;
                //4 
                specialservicereq1 = val;
                specialservicereq2 = val;
                //5 
                otherserviceinfo1 = val;
                otherserviceinfo2 = val;
                //6
                originsitaaddress = val;
                bookingrefairport = val;
                officefundesignation = val;
                companydesignator = val;
                bookingfileref = val;
                participentidetifier = val;
                participentcode = val;
                participentairportcity = val;

                //7 shipment refernece info
                shiprefnum = val;
                supplemetryshipperinfo1 = val;
                supplemetryshipperinfo2 = val;
            }
            #endregion
        }

        public struct UCMInfo
        {
            public string FltNo;
            public string OutFltNo;
            public string Date;
            public string FltRegNo;
            public string StationCode;
            public UCMInfo(string str)
            {
                FltNo = str;
                Date = str;
                FltRegNo = str;
                StationCode = str;
                OutFltNo = str;
            }
        }

        public struct ULDinfo
        {//uldloadingindicator-use as cargo indicator for UCM
            public string uldno;
            public string uldtype;
            public string uldsrno;
            public string uldowner;
            public string uldloadingindicator;
            public string uldweightcode;
            public string uldweight;
            public string uldremark;
            public string portsequence;
            public string refuld;
            public string stationcode;
            public string movement;
            public string AWBNumber;
            public string AWBPrefix;
            public string IsBUP;
            //Added for UWS and NTM
            public string loadcatagorycode1;
            public string loadcategorycode2;
            public string contorcode;
            public string contornumber;
            public string specialloadcode;
            public string specialloadremark;
            public string loadingpositioncode;
            public string loadingposition;
            public string ulddestination;
            public string volumecode;
            public string remark;
            public string loadingpriority;
            public string flightdest;
            public ULDinfo(string str)
            {
                this.uldno = str;
                this.uldtype = str;
                this.uldsrno = str;
                this.uldowner = str;
                this.uldloadingindicator = str;
                this.uldweight = str;
                this.uldweightcode = str;
                this.uldremark = str;
                this.portsequence = str;
                this.refuld = str;
                this.stationcode = str;
                this.movement = str;
                this.AWBNumber = str;
                this.AWBPrefix = str;
                this.IsBUP = str;
                this.loadcatagorycode1 = str;
                this.loadcategorycode2 = str;
                this.contorcode = str;
                this.contornumber = str;
                this.specialloadcode = str;
                this.specialloadremark = str;
                this.loadingpositioncode = str;
                this.loadingposition = str;
                this.ulddestination = str;
                this.volumecode = str;
                this.remark = str;
                this.loadingpriority = str;
                this.flightdest = str;
            }

        }

        public struct fblinfo
        {
            public string fblversion;
            //2 line flight ID & point of loadng            
            public string messagesequencenum;
            public string carriercode;
            public string fltnum;
            public string date;
            public string month;
            public string fltairportcode;
            public string aircraftregistration;

            //4 consigment info
            public string totalconsignment;

            //7 ULD info
            public string noofuld;

            //8 special service request
            public string specialservicereq1;
            public string specialservicereq2;

            //10 
            public string endmesgcode;

            public fblinfo(string str)
            {
                fblversion = str;
                //2 line flight ID & point of loadng            
                messagesequencenum = str;
                carriercode = str;
                fltnum = str;
                date = str;
                month = str;
                fltairportcode = str;
                aircraftregistration = str;

                //4 consigment info
                totalconsignment = str;

                //7 ULD info
                noofuld = str;

                //8 special service request
                specialservicereq1 = str;
                specialservicereq2 = str;

                //10 
                endmesgcode = str;
            }

        }

        public struct unloadingport
        {
            // point of unloading
            public string unloadingairport;
            public string nilcargocode;
            public string day;
            public string month;
            public string time;
            public string day1;
            public string month1;
            public string time1;
            public string sequencenum;
            public unloadingport(string str)
            {
                unloadingairport = str;
                nilcargocode = str;
                day = str;
                month = str;
                time = str;
                day1 = str;
                month1 = str;
                time1 = str;
                sequencenum = str;
            }
        }

        public struct consignmentorigininfo
        {
            // consignment origin info
            public string abbrivatedname;
            public string carriercode;
            public string flightnum;
            public string day;
            public string month;
            public string airportcode;
            public string movementprioritycode;

            public consignmentorigininfo(string str)
            {
                abbrivatedname = str;
                carriercode = str;
                flightnum = str;
                day = str;
                month = str;
                airportcode = str;
                movementprioritycode = str;
            }
        }

        public struct dimensionnfo
        {
            public string weightcode;
            public string weight;
            public string mesurunitcode;
            public string length;
            public string width;
            public string height;
            public string piecenum;
            public string consigref;
            public string AWBPrefix;
            public string AWBNumber;
            public string dims_slac;
            public string PieceType;
            public string UldNo;

            public dimensionnfo(string str)
            {
                weightcode = str;
                weight = str;
                mesurunitcode = str;
                length = str;
                width = str;
                height = str;
                piecenum = str;
                consigref = str;
                AWBPrefix = str;
                AWBNumber = str;
                dims_slac = str;
                PieceType = str;
                UldNo = str;
            }
        }

        public struct movementinfo
        {
            public string AirportCode;
            public string CarrierCode;
            public string FlightNumber;
            public string FlightDay;
            public string FlightMonth;
            public string PriorityorVolumecode;
            public string consigref;

            public movementinfo(string str)
            {
                AirportCode = str;
                CarrierCode = str;
                FlightNumber = str;
                FlightDay = str;
                FlightMonth = str;
                PriorityorVolumecode = str;
                consigref = str;
            }
        }

        public struct otherserviceinfo
        {
            public string otherserviceinfo1;
            public string otherserviceinfo2;
            public string consigref;
            public otherserviceinfo(string str)
            {
                otherserviceinfo1 = str;
                otherserviceinfo2 = str;
                consigref = str;
            }
        }

        public struct customsextrainfo
        {
            public string IsoCountryCodeOci;
            public string InformationIdentifierOci;
            public string CsrIdentifierOci;
            public string SupplementaryCsrIdentifierOci;
            public string consigref;
            public string OCIInfo;
            public customsextrainfo(string str)
            {
                IsoCountryCodeOci = str;
                InformationIdentifierOci = str;
                CsrIdentifierOci = str;
                SupplementaryCsrIdentifierOci = str;
                consigref = str;
                OCIInfo = str;
            }
        }

        public struct ffminfo
        {
            //line 1
            public string ffmversionnum;
            //line 2
            public string messagesequencenum;
            public string carriercode;
            public string fltnum;
            public string fltdate;
            public string month;
            public string time;
            public string fltairportcode;
            public string aircraftregistration;
            public string countrycode;
            public string fltdate1;
            public string fltmonth1;
            public string flttime1;
            public string fltairportcode1;
            //line 8
            public string customorigincode;
            //8 special service request
            public string specialservicereq1;
            public string specialservicereq2;
            //line 18
            public string endmesgcode;
            public ffminfo(string str)
            {
                ffmversionnum = str;
                //line 2
                messagesequencenum = str;
                carriercode = str;
                fltnum = str;
                fltdate = str;
                month = str;
                time = str;
                fltairportcode = str;
                aircraftregistration = str;
                countrycode = str;
                fltdate1 = str;
                fltmonth1 = str;
                flttime1 = str;
                fltairportcode1 = str;
                specialservicereq1 = str;
                specialservicereq2 = str;
                //line 8
                customorigincode = str;
                //line 18
                endmesgcode = str;
            }
        }

        public struct fwbinfo
        {
            public string fwbversionnum;
            //2 consignment details
            public string airlineprefix;
            public string awbnum;
            public string origin;
            public string dest;
            public string consigntype;
            public string pcscnt;
            public string weightcode;
            public string weight;
            public string volumecode;
            public string volumeamt;
            public string densityindicator;
            public string densitygrp;
            //3 flight booking details
            public string carriercode;
            public string fltnum;
            public string fltday;
            //4 routing
            public string routingairportcitycode;
            public string routingcarriercode;
            //5 shipper info
            public string shipperaccnum;
            public string shippername;
            public string shipperadd;
            public string shipperplace;
            public string shipperstate;
            public string shippercountrycode;
            public string shipperpostcode;
            public string shippercontactidentifier;
            public string shippercontactnum;
            public string shipperfaxnum;
            public string shippertelexnum;
            public string shipperadd2;
            public string shippername2;
            //6 consignee info
            public string consaccnum;
            public string consname;
            public string consadd;
            public string consplace;
            public string consstate;
            public string conscountrycode;
            public string conspostcode;
            public string conscontactidentifier;
            public string conscontactnum;
            public string consfaxnum;
            public string constelexnum;
            public string consadd2;
            public string consname2;
            //7 agent info
            public string agentaccnum;
            public string agentIATAnumber;
            public string agentCASSaddress;
            public string agentParticipentIdentifier;
            public string agentname;
            public string agentplace;
            //8 special service request
            public string specialservicereq1;
            public string specialservicereq2;
            //9 also notify
            public string notifyname;
            public string notifyadd;
            public string notifyplace;
            public string notifystate;
            public string notifycountrycode;
            public string notifypostcode;
            public string notifycontactidentifier;
            public string notifycontactnum;

            //10 accounting info
            public string accountinginfoidentifier;
            public string accountinginfo;

            //11 charge declaration
            public string currency;
            public string chargecode;
            public string chargedec;
            public string declaredvalue;
            public string declaredcustomvalue;
            public string insuranceamount;

            //14 prepaid charge
            public string PPweightCharge;
            public string PPValuationCharge;
            public string PPTaxesCharge;
            public string PPOCDA;
            public string PPOCDC;
            public string PPTotalCharges;

            //15 collect charge
            public string CCweightCharge;
            public string CCValuationCharge;
            public string CCTaxesCharge;
            public string CCOCDA;
            public string CCOCDC;
            public string CCTotalCharges;

            //16 shipper certification
            public string shippersignature;

            //17 carrier execution
            public string carrierdate;
            public string carriermonth;
            public string carrieryear;
            public string carrierplace;
            public string carriersignature;

            //19 cc charges
            public string cccurrencycode;
            public string ccexchangerate;
            public string ccchargeamt;

            //20 sender ref
            public string senderairport;
            public string senderofficedesignator;
            public string sendercompanydesignator;
            public string senderFileref;
            public string senderParticipentIdentifier;
            public string senderParticipentCode;
            public string senderPariticipentAirport;

            //21 custom origin
            public string customorigincode;

            //22 commission info
            public string commisioncassindicator;
            public string commisionCassSettleAmt;

            //23 sales incentive info
            public string saleschargeamt;
            public string salescassindicator;

            //24 agent ref data
            public string agentfileref;

            //25 special handling detials
            public string splhandling;

            //26 nominated handling party
            public string handlingname;
            public string handlingplace;

            //27 shipment ref info
            public string shiprefnum;
            public string supplemetryshipperinfo1;
            public string supplemetryshipperinfo2;

            //28 other participent info
            public string othparticipentname;
            public string othairport;
            public string othofficedesignator;
            public string othcompanydesignator;
            public string othfilereference;
            public string othparticipentidentifier;
            public string othparticipentcode;
            public string othparticipentairport;
            //
            public string fwbPurposecode;
            public string handinginfo;
            public string updatedondate;
            public string updatedontime;
            public string Recivedontime;
            public string ContentCode;
            public string Content;
            public string Recoverytime;
            public string Recoverytimedate;
            public string ProductID;
            public string SCI;
            public string ConsignorParty_PrimaryID;
            public string shippercity;
            public string shipperfaxno;
            public string conscity;


            //public string handinginfo;
            //public string fwbPurposecode;
            //public string updatedondate;
            //public string updatedontime;
            //public string Content;
            //public string Recoverytime;

            public fwbinfo(string str)
            {
                fwbversionnum = str;
                //2 consignment details
                airlineprefix = str;
                awbnum = str;
                origin = str;
                dest = str;
                consigntype = str;
                pcscnt = str;
                weightcode = str;
                weight = str;
                volumecode = str;
                volumeamt = str;
                densityindicator = str;
                densitygrp = str;
                //3 flight booking details
                carriercode = str;
                fltnum = str;
                fltday = str;
                //4 routing
                routingairportcitycode = str;
                routingcarriercode = str;
                //5 shipper info
                shipperaccnum = str;
                shippername = str;
                shipperadd = str;
                shipperplace = str;
                shipperstate = str;
                shippercountrycode = str;
                shipperpostcode = str;
                shippercontactidentifier = str;
                shippercontactnum = str;
                shipperfaxnum = str;
                shippertelexnum = str;
                shipperadd2 = str;
                shippername2 = str;
                //6 consignee info
                consaccnum = str;
                consname = str;
                consadd = str;
                consplace = str;
                consstate = str;
                conscountrycode = str;
                conspostcode = str;
                conscontactidentifier = str;
                conscontactnum = str;
                consfaxnum = str;
                constelexnum = str;
                consadd2 = str;
                consname2 = str;
                //7 agent info
                agentaccnum = str;
                agentIATAnumber = str;
                agentCASSaddress = str;
                agentParticipentIdentifier = str;
                agentname = str;
                agentplace = str;
                //8 special service request
                specialservicereq1 = str;
                specialservicereq2 = str;
                //9 also notify
                notifyname = str;
                notifyadd = str;
                notifyplace = str;
                notifystate = str;
                notifycountrycode = str;
                notifypostcode = str;
                notifycontactidentifier = str;
                notifycontactnum = str;

                //10 accounting info
                accountinginfoidentifier = str;
                accountinginfo = str;


                //11 charge declaration
                currency = str;
                chargecode = str;
                chargedec = str;
                declaredvalue = str;
                declaredcustomvalue = str;
                insuranceamount = str;

                //14 prepaid charge

                PPweightCharge = str;
                PPValuationCharge = str;
                PPTaxesCharge = str;
                PPOCDA = str;
                PPOCDC = str;
                PPTotalCharges = str;

                //15 collect charge
                CCweightCharge = str;
                CCValuationCharge = str;
                CCTaxesCharge = str;
                CCOCDA = str;
                CCOCDC = str;
                CCTotalCharges = str;

                //16 shipper certification
                shippersignature = str;

                //17 carrier execution
                carrierdate = str;
                carriermonth = str;
                carrieryear = str;
                carrierplace = str;
                carriersignature = str;

                //19 cc charges
                cccurrencycode = str;
                ccexchangerate = str;
                ccchargeamt = str;

                //20 sender ref
                senderairport = str;
                senderofficedesignator = str;
                sendercompanydesignator = str;
                senderFileref = str;
                senderParticipentIdentifier = str;
                senderParticipentCode = str;
                senderPariticipentAirport = str;

                //21 custom origin
                customorigincode = str;

                //22 commission info
                commisioncassindicator = str;
                commisionCassSettleAmt = str;

                //23 sales incentive info
                saleschargeamt = str;
                salescassindicator = str;

                //24 agent ref data
                agentfileref = str;

                //25 special handling detials
                splhandling = str;

                //26 nominated handling party
                handlingname = str;
                handlingplace = str;

                //27 shipment ref info
                shiprefnum = str;
                supplemetryshipperinfo1 = str;
                supplemetryshipperinfo2 = str;

                //28 other participent info
                othparticipentname = str;
                othairport = str;
                othofficedesignator = str;
                othcompanydesignator = str;
                othfilereference = str;
                othparticipentidentifier = str;
                othparticipentcode = str;
                othparticipentairport = str;
                //
                fwbPurposecode = str;
                handinginfo = str;
                updatedondate = str;
                updatedontime = str;
                Recivedontime = str;
                ContentCode = str;
                Content = str;
                Recoverytime = str;
                Recoverytimedate = str;
                ProductID = str;
                SCI = str;
                ConsignorParty_PrimaryID = str;
                shippercity = str;
                shipperfaxno = str;
                conscity = str;
            }

        }

        public struct othercharges
        {
            public string indicator;
            public string otherchargecode;
            public string entitlementcode;
            public string chargeamt;

            public othercharges(string str)
            {
                indicator = str;
                otherchargecode = str;
                entitlementcode = str;
                chargeamt = str;
            }
        }

        public struct RateDescription
        {
            public string linenum;
            public string pcsidentifier;
            public string numofpcs;
            public string weightindicator;
            public string weight;
            public string rateclasscode;
            public string commoditynumber;
            public string awbweight;
            public string rateidentiier;
            public string chargerate;
            public string tarrifidentifier;
            public string chargeamt;
            public string goodsnature;
            public string goodsnature1;
            public string ngnc;
            public string volcode;
            public string volamt;
            public string unit;
            public string length;
            public string width;
            public string height;
            public string pcscnt;
            public string uldtype;
            public string uldserialnum;
            public string uldowner;
            public string slac;
            public string hermonisedcomoditycode;
            public string isocountrycode;
            public string isconsole;
            public string ProductType;

            public string noofposition;
            public string noofpositionpo;
            public string currencyexchange;


            public RateDescription(string str)
            {
                linenum = str;
                pcsidentifier = str;
                numofpcs = str;
                weightindicator = str;
                weight = str;
                rateclasscode = str;
                commoditynumber = str;
                awbweight = str;
                rateidentiier = str;
                chargerate = str;
                tarrifidentifier = str;
                chargeamt = str;
                goodsnature = str;
                goodsnature1 = str;
                ngnc = str;
                volcode = str;
                volamt = str;
                unit = str;
                length = str;
                width = str;
                height = str;
                uldtype = str;
                uldserialnum = str;
                uldowner = str;
                slac = str;
                hermonisedcomoditycode = str;
                isocountrycode = str;
                pcscnt = str;
                isconsole = str;
                ProductType = str;
                noofposition = str;
                noofpositionpo = str;
                currencyexchange = str;

            }

        }

        //badiuz khan
        //2016-02-15
        public struct AWBBuildBUP
        {
            public string UldType;
            public string ULDNo;
            public string ULDOwnerCode;
            public string SlacCount;
            public string BUPWt;

            public AWBBuildBUP(string str)
            {
                UldType = str;
                ULDNo = str;
                ULDOwnerCode = str;
                SlacCount = str;
                BUPWt = str;
            }
        }

        public struct CommonStruct
        {
            public string messageprefix;
            public string carriercode;
            public string seccarriercode;
            public string flightnum;
            public string fltday;
            public string fltmonth;
            public string flttime;
            public string fltorg;
            public string fltdest;
            public string pcsindicator;
            public string numofpcs;
            public string weightcode;
            public string weight;
            public string volumecode;
            public string volumeamt;
            public string densityindicator;
            public string densitygroup;
            public string name;
            public string daychangeindicator;
            public string timeindicator;
            public string depttime;
            public string arrivaltime;
            public string transfermanifestnumber;
            public string airportcode;
            public string infocode;
            public string updatedonday;
            public string updatedontime;

            public CommonStruct(string str)
            {
                messageprefix = str;
                carriercode = str;
                seccarriercode = str;
                flightnum = str;
                fltday = str;
                fltmonth = str;
                flttime = str;
                fltorg = str;
                fltdest = str;
                pcsindicator = str;
                numofpcs = str;
                weightcode = str;
                weight = str;
                volumecode = str;
                volumeamt = str;
                densityindicator = str;
                densitygroup = str;
                name = str;
                daychangeindicator = str;
                timeindicator = str;
                depttime = str;
                arrivaltime = str;
                transfermanifestnumber = str;
                airportcode = str;
                infocode = str;
                updatedonday = str;
                updatedontime = str;
            }

        }

        public struct FSAInfo
        {
            public string fsaversion;

            //2 consignment details
            public string airlineprefix;
            public string awbnum;
            public string origin;
            public string dest;
            public string consigntype;
            public string pcscnt;
            public string weightcode;
            public string weight;
            public string totalpcscnt;

            public FSAInfo(string str)
            {
                fsaversion = str;
                //2 consignment details
                airlineprefix = str;
                awbnum = str;
                origin = str;
                dest = str;
                consigntype = str;
                pcscnt = str;
                weightcode = str;
                weight = str;
                totalpcscnt = str;
            }
        }


        public struct fhlinfo
        {
            public string fhlversionnum;

            //2Master AWB Consignment Detail 
            public string airlineprefix;
            public string awbnum;
            public string origin;
            public string dest;
            public string consigntype;
            public string pcscnt;
            public string weightcode;
            public string weight;

            //5 shipper info
            public string shipperaccnum;
            public string shippername;
            public string shipperadd;
            public string shipperplace;
            public string shipperstate;
            public string shippercountrycode;
            public string shipperpostcode;
            public string shippercontactidentifier;
            public string shippercontactnum;
            public string shipperadd2;
            public string shippername2;

            //6 consignee info
            public string consaccnum;
            public string consname;
            public string consadd;
            public string consplace;
            public string consstate;
            public string conscountrycode;
            public string conspostcode;
            public string conscontactidentifier;
            public string conscontactnum;
            public string consadd2;
            public string consname2;


            //11 charge declaration
            public string currency;
            public string chargecode;
            public string chargedec;
            public string declaredvalue;
            public string declaredcustomvalue;
            public string insuranceamount;
            public string declaredcarriervalue;

            public fhlinfo(string str)
            {

                fhlversionnum = str;

                //2
                airlineprefix = str;
                awbnum = str;
                origin = str;
                dest = str;
                consigntype = str;
                pcscnt = str;
                weightcode = str;
                weight = str;

                //5 shipper info
                shipperaccnum = str;
                shippername = str;
                shipperadd = str;
                shipperplace = str;
                shipperstate = str;
                shippercountrycode = str;
                shipperpostcode = str;
                shippercontactidentifier = str;
                shippercontactnum = str;
                shipperadd2 = str;
                shippername2 = str;

                //6 consignee info
                consaccnum = str;
                consname = str;
                consadd = str;
                consplace = str;
                consstate = str;
                conscountrycode = str;
                conspostcode = str;
                conscontactidentifier = str;
                conscontactnum = str;
                consadd2 = str;
                consname2 = str;

                //11 charge declaration
                currency = str;
                chargecode = str;
                chargedec = str;
                declaredvalue = str;
                declaredcustomvalue = str;
                insuranceamount = str;
                declaredcarriervalue = str;
            }
        }

        public struct consignmnetinfo
        {
            public string airlineprefix;
            public string awbnum;
            public string origin;
            public string dest;
            public string consigntype;
            public string pcscnt;
            public string weightcode;
            public string weight;
            public string volumecode;
            public string volumeamt;
            public string densityindicator;
            public string densitygrp;
            public string shpdesccode;
            public string numshp;
            public string manifestdesc;
            public string splhandling;
            public string portsequence;
            public string uldsequence;
            public string customref;
            public string customorigincode;//8
            public string freetextGoodDesc;
            public string commodity;
            public string slac;
            public string TotalConsignmentType;
            public string AWBPieces;

            public string DimsString;
            public string IsBup;

            public string scaleweight;
            public string CartNumber;
            public string Remark;
            public string loadcatcode1;
            public string loadingpriority;

            public consignmnetinfo(string str)
            {
                DimsString = str;
                airlineprefix = str;
                awbnum = str;
                origin = str;
                dest = str;
                consigntype = str;
                pcscnt = str;
                weightcode = str;
                weight = str;
                volumecode = str;
                volumeamt = str;
                densityindicator = str;
                densitygrp = str;
                shpdesccode = str;
                numshp = str;
                manifestdesc = str;
                splhandling = str;
                portsequence = str;
                uldsequence = str;
                customref = str;
                customorigincode = str;
                freetextGoodDesc = str;
                commodity = str;
                slac = str;
                TotalConsignmentType = str;
                AWBPieces = str;
                IsBup = str;
                scaleweight = str;
                CartNumber = str;
                Remark = str;
                loadcatcode1 = str;
                loadingpriority = str;

            }
        }

        public struct FltRouteDate
        {

            public string FltDate;

            public FltRouteDate(string val)
            {
                FltDate = val;

            }
        }

        public struct FltRoute
        {
            public string carriercode;
            public string fltnum;
            public string date;
            public string month;
            public string fltdept;
            public string fltarrival;
            public string spaceallotmentcode;
            public string allotidentification;
            /// <summary>
            /// set after flight validation (Format: MM/dd/yyyy)
            /// </summary>
            public string flightdate;
            /// <summary>
            /// set after flight validation
            /// </summary>
            public string scheduleid;
            public string Routesquencenumber;

            public string inputdeptDatetime;
            public string inputarrivaldatetime;
            public string BagNumber;

            public FltRoute(string val)
            {
                //3 flight details
                carriercode = val;
                fltnum = val;
                date = val;
                month = val;
                fltdept = val;
                fltarrival = val;
                spaceallotmentcode = val;
                allotidentification = val;
                flightdate = val;
                scheduleid = val;
                Routesquencenumber = val;
                inputdeptDatetime = val;
                inputarrivaldatetime = val;
                BagNumber = val;
            }
        }

        /// <summary>
        /// Added by prashant for discrepancy information for FAD Message
        /// </summary>
        public struct discrepancydetailsinfo
        {
            public string discrepancycode;
            public string airportcode;
            public string carriercode;
            public string flightnum;
            public string day;
            public string month;
            public string fltdep;
            public string fltarrival;
            public string discrepancyAdviceAddressLineIdentifier;
            public string discrepancyAdviceAddressAirportCode;
            public string discrepancyAdviceAddressOfficeFunctionDesignator;
            public string discrepancyAdviceAddressCompanyDesignator;
            public discrepancydetailsinfo(string val)
            {
                discrepancycode = val;
                airportcode = val;
                carriercode = val;
                flightnum = val;
                day = val;
                month = val;
                fltdep = val;
                fltarrival = val;
                discrepancyAdviceAddressLineIdentifier = val;
                discrepancyAdviceAddressAirportCode = val;
                discrepancyAdviceAddressOfficeFunctionDesignator = val;
                discrepancyAdviceAddressCompanyDesignator = val;
            }

        }

        /// <summary>
        /// Added by prashant for Discrepancy AdviceAddress information for FAD Message
        /// </summary>
        public struct discrepancyadviceaddress
        {

            public string discrepancyAdviceAddressLineIdentifier;
            public string discrepancyAdviceAddressAirportCode;
            public string discrepancyAdviceAddressOfficeFunctionDesignator;
            public string discrepancyAdviceAddressCompanyDesignator;
            public discrepancyadviceaddress(string val)
            {
                discrepancyAdviceAddressLineIdentifier = val;
                discrepancyAdviceAddressAirportCode = val;
                discrepancyAdviceAddressOfficeFunctionDesignator = val;
                discrepancyAdviceAddressCompanyDesignator = val;
            }

        }

        public struct frpinfo
        {
            public string remarks;
            public DateTime bookingdate;
            public string executedat;
            public string user;
        }

        #region Class for Custom Messaging
        public class CustomMessage
        {
            public string AWBPrefix;
            public string AWBNumber;
            public string MessageType;
            public string HAWBNumber;
            public string ConsolidationIdentifier;
            public string PackageTrackingIdentifier;
            public string AWBPartArrivalReference;
            public string ArrivalAirport;
            public string AirCarrier;
            public string Origin;
            public string DestinionCode;
            public string WBLNumberOfPieces;
            public string WBLWeightIndicator;
            public string WBLWeight;
            public string WBLCargoDescription;
            public string ArrivalDate;
            public string PartArrivalReference;
            public string BoardedQuantityIdentifier;
            public string BoardedPieceCount;
            public string BoardedWeight;
            public string ArrWeightCode;
            public string ImportingCarrier;
            public string FlightNumber;
            public string ARRPartArrivalReference;
            public string RequestType;
            public string RequestExplanation;
            public string EntryType;
            public string EntryNumber;
            public string AMSParticipantCode;
            public string ShipperName;
            public string ShipperAddress;
            public string ShipperCity;
            public string ShipperState;
            public string ShipperCountry;
            public string ShipperPostalCode;
            public string ConsigneeName;
            public string ConsigneeAddress;
            public string ConsigneeCity;
            public string ConsigneeState;
            public string ConsigneeCountry;
            public string ConsigneePostalCode;
            public string TransferDestAirport;
            public string DomesticIdentifier;
            public string BondedCarrierID;
            public string OnwardCarrier;
            public string BondedPremisesIdentifier;
            public string InBondControlNumber;
            public string OriginOfGoods;
            public string DeclaredValue;
            public string CurrencyCode;
            public string CommodityCode;
            public string LineIdentifier;
            public string AmendmentCode;
            public string AmendmentExplanation;
            public string DeptImportingCarrier;
            public string DeptFlightNumber;
            public string DeptScheduledArrivalDate;
            public string LiftoffDate;
            public string LiftoffTime;
            public string DeptActualImportingCarrier;
            public string DeptActualFlightNumber;
            public string ASNStatusCode;
            public string ASNActionExplanation;
            public string CSNActionCode;
            public string CSNPieces;
            public string TransactionDate;
            public string TransactionTime;
            public string CSNEntryType;
            public string CSNEntryNumber;
            public string CSNRemarks;
            public string ErrorCode;
            public string ErrorMessage;
            public string StatusRequestCode;
            public string StatusAnswerCode;
            public string Information;
            public string ERFImportingCarrier;
            public string ERFFlightNumber;
            public string ERFDate;
            public string Message;
            public string UpdatedOn;
            public string UpdatedBy;
            public string CreatedOn;
            public string CreatedBy;
            public string FlightNo;
            public DateTime FlightDate;
            public string ControlLocation;


        }
        #endregion


        #region FSB Message Class

        /// <summary>
        /// FSB Message
        /// Badiuz khan
        /// 2016-01-15
        /// Description:Class add for FSB Message
        /// </summary>
        public class FSBAWBInformation
        {
            public string MessageHeader;
            public string MessageType;
            public string MessageVersion;
            public string AirlinePrefix;
            public string AWBNo;
            public string AWBOrigin;
            public string AWBDestination;
            public int TotalAWbPiececs;
            public string WeightCode;
            public decimal GrossWeight;
            public string NatureofGoods1;
            public string NatureofGoods2;
            public string VolumeCode;
            public decimal AWBVolume;
            public string OtherServiceInformation;
            public string CustomOrigin;
            public string ShipmentSendgerReference1;
            public string ShipmentSendgerReference2;
            public string ShipmentReferenceInformation;
            public string RefereceOriginTag;
            public string RefereceFileTag;
            public string ShipmentReferenceNumber;
            public string ShipmentSuplementyInformation;
            public string ShipmentSuplementyInformation1;
        }

        public class RouteInformation
        {
            public string Carriercode;
            public string FlightOrigin;
            public string FlightDestination;
        }

        public class ShipperInformation
        {
            public string ShipperAccountNo;
            public string ShipperName;
            public string ShipperStreetAddress;
            public string ShipperPlace;
            public string ShipperState;
            public string ShipperCountrycode;
            public string ShipperPostalCode;
            public string ShipperContactIdentifier;
            public string ShipperContactNumber;

        }

        public class ConsigneeInformation
        {
            public string ConsigneeAccountNo;
            public string ConsigneeName;
            public string ConsigneeStreetAddress;
            public string ConsigneePlace;
            public string ConsigneeState;
            public string ConsigneeCountrycode;
            public string ConsigneePostalCode;
            public string ConsigneeContactIdentifier;
            public string ConsigneeContactNumber;
        }


        public struct FSBDimensionInformation
        {
            public string DimWeightcode;
            public decimal DimGrossWeight;
            public string DimUnitCode;
            public int DimLength;
            public int DimWidth;
            public int DimHeight;
            public int DimPieces;

        }

        public class AWBBUPInformation
        {
            public string ULDNo;
            public string SlacCount;

        }


        #endregion

        #region FSB Message Class

        /// <summary>
        /// FSB Message
        /// Badiuz khan
        /// 2016-01-15
        /// Description:Class add for FSB Message
        /// </summary>
        public class FWRInformation
        {
            public string MessageHeader;
            public string MessageType;
            public string MessageVersion;
            public string AirlinePrefix;
            public string AWBNo;
            public string RefereceOriginTag;
            public string RefereceFileTag;
            public string ShipmentReferenceNumber;
            public string ShipmentSuplementyInformation;
            public string ShipmentSuplementyInformation1;
        }

        #endregion

        #region FBR Message Class

        /// <summary>
        /// FBR Message
        /// Badiuz khan
        /// 2016-05-08
        /// Description:Class add for FBR Message
        /// </summary>
        public class FBRInformation
        {
            public string MessageHeader;
            public string MessageType;
            public string MessageVersion;
            public string FlightNo;
            public string FlightDate;
            public string FlightOrigin;
            public string FlightDestination;
            public string RefereceFileTag;
            public string ShipmentReferenceNumber;
            public string ShipmentSuplementyInformation;
            public string ShipmentSuplementyInformation1;
        }

        #endregion

        /// <summary>
        /// Structure for FNA Message
        /// </summary>
        public struct FNA
        {

            public string MsgVersion;
            public string AckInfo;
            public string originalmessage;
            public string AWBnumber;
            public string AWBPrefix;
            public string Origin;
            public string Destination;
            public string MessageType;

            public FNA(string val)
            {
                MsgVersion = val;
                AckInfo = val;
                originalmessage = val;
                AWBnumber = val;
                AWBPrefix = val;
                Origin = val;
                Destination = val;
                MessageType = val;
            }
        }

        public struct MessageTypeName
        {
            public const string APPROVEDUSER = "APPROVEDUSER";
            public const string BDCUSTOM = "BDCUSTOM";
            public const string ClosedInvoice = "ClosedInvoice";
            public const string CPM = "CPM";
            public const string FBL = "FBL";
            public const string FDM = "FDM";
            public const string FFA = "FFA";
            public const string FFM = "FFM";
            public const string FFR = "FFR";
            public const string FHL = "FHL";
            public const string FMA = "FMA";
            public const string FNA = "FNA";
            public const string FRIFRC = "FRI/FRC";
            public const string FRIFRCFRXFSN = "FRI/FRC/FRX/FSN";
            public const string FSB = "FSB";
            public const string FSU = "FSU";
            public const string FSUDEP = "FSU/DEP";
            public const string FSUDLV = "FSU/DLV";
            public const string FSUNFD = "FSU/NFD";
            public const string FSURCF = "FSU/RCF";
            public const string FSURCS = "FSU/RCS";
            public const string FSURCT = "FSU/RCT";
            public const string FWB = "FWB";
            public const string FWR = "FWR";
            public const string Interface = "Interface";
            public const string KNOWNSHPTOADMIN = "KNOWNSHPTOADMIN";
            public const string KNOWNSHPTOAGT = "KNOWNSHPTOAGT";
            public const string LoadControlFlyware = "LoadControl/Flyware";
            public const string SK2CS = "SK2CS";
            public const string MXCustom = "MXCustom";
            public const string NEWAGTTOADMIN = "NEWAGTTOADMIN";
            public const string NEWAGTTOAGT = "NEWAGTTOAGT";
            public const string NEWUSERTOADMIN = "NEWUSERTOADMIN";
            public const string NEWUSERTOUSER = "NEWUSERTOUSER";
            public const string NTM = "NTM";
            public const string OMCUSTOM_M = "OMCUSTOM_M";
            public const string OMCUSTOM_H = "OMCUSTOM_H";
            public const string PRI = "PRI";
            public const string REJECTEDUSER = "REJECTEDUSER";
            public const string SPOTRATE = "SPOTRATE";
            public const string UCM = "UCM";
            public const string UWS = "UWS";
            public const string VOLCPL = "VOLCPL";
            public const string XDLV = "XDLV";
            public const string XFFM = "XFFM";
            public const string XFFR = "XFFR";
            public const string XFHL = "XFHL";
            public const string XFSU = "XFSU";
            public const string XFWB = "XFWB";
            public const string XFZB = "XFZB";
            //public const string XMLCustomMessage = "XMLCustomMessage";
            public const string CLAIMS = "CLAIMS";
            public const string FAD = "FAD";
            public const string FDA = "FDA";

            public const string RATELINE = "RateLine";
            public const string AGENT = "Agent";
            public const string AGENTGENERALTAB = "AgentGeneralTab";
            public const string SHIPPERCONSIGNEE = "Shipper/Consignee";
            public const string OTHERCHARGES = "OtherCharges";

            public const string FLIGHTCAPACITY = "FlightCapacity";
            public const string SCHEDULEUPLOAD = "ScheduleUpload";
            public const string SSM = "SSM";
            public const string ASM = "ASM";
            public const string AGENTUPDATE = "AgentUpdate";
            public const string CAPACITYALLOCATION = "CapacityAllocation";
            public const string TAXLINE = "TaxLine";
            public const string USER = "User";
            public const string CARGOLOADXML = "CargoLoadXML";
            public const string FLIGHTPAXINFORMATIONUPLOAD = "FlightPaxInformationUpload";
            public const string FLIGHTPAXFORECASTUPLOAD = "FlightPaxForecastUpload";
            public const string EXCHANGERATESFROMTOUPLOAD = "ExchangeRatesFromToUpload";

            public const string DIMS_Cubiscan = "DIMS_Cubiscan";
            public const string JPEG_Cubiscan = "JPEG_Cubiscan";
            public const string PHCUSTOMREGISTRY = "PHCustomRegistry";
            public const string PHCUSTOM_M = "PHCUSTOM_M";
            public const string SISFILES = "SISFILES";
            public const string EXCELUPLOADBOOKINGFFR = "ExcelUploadBookingFFR";
        }

        public struct SCMInfo
        {
            public string Date;
            public string StationCode;
            public string uldno;
            public string uldtype;
            public string uldsrno;
            public string uldowner;
            public string movement;
            public string uldstatus;
            public SCMInfo(string str)
            {
                Date = str;
                StationCode = str;
                uldno = str;
                uldtype = str;
                uldsrno = str;
                uldowner = str;
                movement = str;
                uldstatus = str;
            }
        }

        #region NTM and UWS

        public struct NTMULDinfo
        {//uldloadingindicator-use as cargo indicator for UCM

            public string uldno;
            public string uldtype;
            public string uldsrno;
            public string uldowner;
            public string uldloadingindicator;
            public string uldweightcode;


            public string uldweight;
            public string uldremark;
            public string portsequence;

            public string refuld;
            public string stationcode;
            public string movement;
            public int sequencenumber;
            public int noofAWBs;

            //Added for UWS and NTM
            public string loadcatagorycode1;
            public string loadcategorycode2;
            public string contorcode;
            public string contornumber;
            public string specialloadcode;
            public string specialloadremark;
            public string loadingpositioncode;
            public string loadingposition;
            public string ulddestination;
            public string volumecode;
            public string remark;
            public string loadingpriority;
            //public int noofAWBs;
            public string ISDGR;
            public string ISSpecial;

            public NTMULDinfo(string str)
            {

                ISDGR = str;
                ISSpecial = str;
                //noOfAWBS = Convert.ToInt32(str);
                sequencenumber = Convert.ToInt32(str);
                //UWS and NTM
                noofAWBs = Convert.ToInt32(str);
                loadingpriority = str;
                remark = str;
                volumecode = str;
                loadingpositioncode = str;
                ulddestination = str;
                loadingposition = str;
                contorcode = str;
                contornumber = str;
                specialloadremark = str;
                specialloadcode = str;
                loadcategorycode2 = str;
                loadcatagorycode1 = str;
                //End

                this.uldno = str;
                this.uldtype = str;
                this.uldsrno = str;
                this.uldowner = str;
                this.uldloadingindicator = str;
                this.uldweight = str;
                this.uldweightcode = str;
                this.uldremark = str;
                this.portsequence = str;
                this.refuld = str;
                this.stationcode = str;
                this.movement = str;
            }
        }

        public struct UWSinfo
        {
            public string pol;
            public string carriercode;
            public string fltnum;
            public string fltdate;
            public string month;
            public string time;
            public string fltairportcode;
            public string aircraftregistration;
            public string countrycode;
            public string fltdate1;
            public string fltmonth1;
            public string flttime1;
            public string fltairportcode1;
            public string messageseqnumber;
            public string messageendindicator;


            public UWSinfo(string str)
            {
                //line 2
                messageseqnumber = str;
                pol = str;
                carriercode = str;
                fltnum = str;
                fltdate = str;
                month = str;
                time = str;
                fltairportcode = str;
                aircraftregistration = str;
                countrycode = str;
                fltdate1 = str;
                fltmonth1 = str;
                flttime1 = str;
                fltairportcode1 = str;
                messageendindicator = str;
            }
        }
        public struct NTMInfo

        {
            public string ffaversionnum;

            //2 consignment details
            public string airlineprefix;
            public string awbnum;
            public string origin;
            public string dest;
            public string consigntype;
            public string pcscnt;
            public string weightcode;
            public string weight;
            public string shpdesccode;
            public string numshp;
            public string manifestdesc;
            public string splhandling;
            //3 flight details
            public string carriercode;
            public string fltnum;
            public string date;
            public string month;
            public string fltdept;
            public string fltarrival;
            public string spaceallotmentcode;
            //4 
            public string specialservicereq1;
            public string specialservicereq2;
            //5 
            public string otherserviceinfo1;
            public string otherserviceinfo2;
            //6
            public string bookingrefairport;
            public string officefundesignation;
            public string companydesignator;
            public string bookingfileref;
            public string participentidetifier;
            public string participentcode;
            public string participentairportcity;

            //7 shipment refernece info
            public string shiprefnum;
            public string supplemetryshipperinfo1;
            public string supplemetryshipperinfo2;

            public string classorDiv;
            public string unidref;
            public string unidnumber;
            public string subsidaryrisk;
            public string subsideryrisk2;
            public string packageinfo;
            public string noofpackages;
            public string netquanitity;
            public string radioactivequantity;
            public string packinggroup;
            public string impcode;
            public string drillcode;
            public string caocode;
            public string uldposition;
            public string shippername;
            public string technicalname;
            public string awbhandinginfo;
            public string uldno;
            public string description;
            public string specialhandlingcode;
            public string Ergcode;
            public string remark;

            #region Constructor

            public NTMInfo(string val)
            {
                remark = val;
                Ergcode = val;
                specialhandlingcode = val;
                description = val;
                classorDiv = val;
                unidref = val;
                unidnumber = val;
                subsidaryrisk = val;
                subsideryrisk2 = val;
                packageinfo = val;
                noofpackages = val;
                netquanitity = val;
                radioactivequantity = val;
                packinggroup = val;
                impcode = val;
                drillcode = val;
                caocode = val;
                uldposition = val;
                shippername = val;
                technicalname = val;
                awbhandinginfo = val;
                uldno = val;

                ffaversionnum = val;
                //2 consignment details
                airlineprefix = val;
                awbnum = val;
                origin = val;
                dest = val;
                consigntype = val;
                pcscnt = val;
                weightcode = val;
                weight = val;
                shpdesccode = val;
                numshp = val;
                manifestdesc = val;
                splhandling = val;
                //3 flight details
                carriercode = val;

                fltnum = val;
                date = val;

                month = val;
                fltdept = val;
                fltarrival = val;
                spaceallotmentcode = val;
                //4 
                specialservicereq1 = val;
                specialservicereq2 = val;
                //5 
                otherserviceinfo1 = val;
                otherserviceinfo2 = val;
                //6
                bookingrefairport = val;
                officefundesignation = val;
                companydesignator = val;
                bookingfileref = val;
                participentidetifier = val;
                participentcode = val;
                participentairportcity = val;


                //7 shipment refernece info
                shiprefnum = val;
                supplemetryshipperinfo1 = val;
                supplemetryshipperinfo2 = val;
            }
            #endregion
        }

        public struct CSNInfo
        {
            public string versionNumber;
            public string awbNumber;
            public string awbPrefix;
            public string csnNumber;
            public string flightDay;
            public string flightMonth;
            public string flightNumber;
            public string pol;
            public string pou;
            public string country;
            public string IMPCode;
            public string code;
            public string MRNCode;
            public string customStatusCode;
            public string customsActionCode;
            public string customsNotification;
            public string notificationDay;
            public string notificationMonth;
            public string notificationTime;
            public string customsEntryNumber;
            public string numberOfPieces;
            public string IsMaster;
            public string ISHouse;
            public string HousePCS;
            public string HouseWt;
            public string HouseNo;
            public string weightcode;
            public CSNInfo(string str)
            {
                versionNumber = str;
                awbNumber = str;
                awbPrefix = str;
                csnNumber = str;
                flightDay = str;
                flightMonth = str;
                flightNumber = str;
                pou = str;
                pol = str;
                country = str;
                IMPCode = str;
                code = str;
                MRNCode = str;
                customStatusCode = str;
                customsActionCode = str;
                customsNotification = str;
                notificationDay = str;
                notificationMonth = str;
                notificationTime = str;
                customsEntryNumber = str;
                numberOfPieces = str;
                IsMaster = str;
                ISHouse = str;
                HousePCS = str;
                HouseWt = str;
                HouseNo = str;
                weightcode = str;
            }
        }

      

        public struct NTMconsignmnetinfo

        {
            public string ffaversionnum;
            public string uom;
            //2 consignment details
            public string airlineprefix;
            public string awbnum;
            public string origin;
            public string dest;
            public string consigntype;
            public string pcscnt;
            public string weightcode;
            public string weight;
            public string shpdesccode;
            public string numshp;
            public string manifestdesc;
            public string splhandling;
            //3 flight details
            public string carriercode;
            public string fltnum;
            public string date;
            public string month;
            public string fltdept;
            public string fltarrival;
            public string spaceallotmentcode;
            //4 
            public string specialservicereq1;
            public string specialservicereq2;
            //5 
            public string otherserviceinfo1;
            public string otherserviceinfo2;
            //6
            public string bookingrefairport;
            public string officefundesignation;
            public string companydesignator;
            public string bookingfileref;
            public string participentidetifier;
            public string participentcode;
            public string participentairportcity;
            //7 shipment refernece info
            public string shiprefnum;
            public string supplemetryshipperinfo1;
            public string supplemetryshipperinfo2;
            public string DGRorSPecialCargo;
            public string classorDiv;
            public string unidref;
            public string unidnumber;
            public string subsidaryrisk;
            public string subsideryrisk2;
            public string packageinfo;
            public string noofpackages;
            public string netquanitity;
            public string radioactivequantity;
            public string packinggroup;
            public string impcode;
            public string drillcode;
            public string caocode;
            public string uldposition;
            public string shippername;
            public string technicalname;
            public string awbhandinginfo;
            public string uldno;
            public string description;
            public string specialhandlingcode;
            public string Ergcode;
            public string remark;
            public int uldrefseqno;
            public string dgrclass;

            public NTMconsignmnetinfo(string val)
            {
                DGRorSPecialCargo = val;
                uom = val;
                remark = val;
                Ergcode = val;
                dgrclass = val;
                uldrefseqno = Convert.ToInt32(val);
                specialhandlingcode = val;
                description = val;
                classorDiv = val;
                unidref = val;
                unidnumber = val;
                subsidaryrisk = val;
                subsideryrisk2 = val;
                packageinfo = val;
                noofpackages = val;
                netquanitity = val;
                radioactivequantity = val;
                packinggroup = val;
                impcode = val;
                drillcode = val;
                caocode = val;
                uldposition = val;
                shippername = val;
                technicalname = val;
                awbhandinginfo = val;
                uldno = val;

                ffaversionnum = val;
                //2 consignment details
                airlineprefix = val;
                awbnum = val;
                origin = val;
                dest = val;
                consigntype = val;
                pcscnt = val;
                weightcode = val;
                weight = val;
                shpdesccode = val;
                numshp = val;
                manifestdesc = val;
                splhandling = val;
                //3 flight details
                carriercode = val;
                fltnum = val;
                date = val;
                month = val;
                fltdept = val;
                fltarrival = val;
                spaceallotmentcode = val;
                //4 
                specialservicereq1 = val;
                specialservicereq2 = val;
                //5 
                otherserviceinfo1 = val;
                otherserviceinfo2 = val;
                //6
                bookingrefairport = val;
                officefundesignation = val;
                companydesignator = val;
                bookingfileref = val;
                participentidetifier = val;
                participentcode = val;
                participentairportcity = val;
                //7 shipment refernece info
                shiprefnum = val;
                supplemetryshipperinfo1 = val;
                supplemetryshipperinfo2 = val;
            }



        }

        public struct LDMInfo
        {
            public string flightno;
            public string flightdate;
            public string tailno;
            public string paxcapacity;
            public string paxcount;
            public string bagweight;
            public string flightdest;

            public LDMInfo(string str)
            {
                flightno = str;
                flightdate = str;
                tailno = str;
                paxcapacity = str;
                paxcount = str;
                bagweight = str;
                flightdest = str;
            }
        }
        #endregion
    }
}