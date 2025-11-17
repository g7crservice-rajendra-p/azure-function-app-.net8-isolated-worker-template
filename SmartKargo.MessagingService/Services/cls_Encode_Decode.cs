using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Configuration;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
namespace QidWorkerRole
{
    /// <summary>
    /// Summary description for cls_Decode
    /// </summary>
    public class cls_Encode_Decode
    {
        //SCMExceptionHandlingWorkRole scmexception = new SCMExceptionHandlingWorkRole();
        //string conStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();

        #region Variables
        static string unloadingportsequence = "";
        static string uldsequencenum = "";
        static string awbref = "";
        const string PAGE_NAME = "cls_Encode_Decode";
        #endregion

        #region Constructor

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ISqlDataHelperDao _readOnlyDao;
        private readonly ILogger<cls_Encode_Decode> _logger;
         private static ILoggerFactory? _loggerFactory;
        private static ILogger<Cls_BL> _staticLogger => _loggerFactory?.CreateLogger<Cls_BL>();
        private readonly PSNMessageProcessor _pSNMessageProcessor;
        public cls_Encode_Decode(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<cls_Encode_Decode> logger,
            ILoggerFactory loggerFactory,
            PSNMessageProcessor pSNMessageProcessor)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _readOnlyDao = sqlDataHelperFactory.Create(readOnly: true);
            _logger = logger;
            _loggerFactory = loggerFactory;
            _pSNMessageProcessor = pSNMessageProcessor;
        }
        #endregion

        //FFR

        #region Decode FFR
        public bool decodereceivedffr(string ffrmsg, ref MessageData.ffrinfo data, ref MessageData.ULDinfo[] uld, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.FltRoute[] fltroute, ref MessageData.dimensionnfo[] objDimension)
        {
            const string FUN_NAME = "decodereceivedffr";
            bool flag = false;
            string lastrec = "NA";
            int line = 0;
            try
            {
                if (ffrmsg.StartsWith("FFR", StringComparison.OrdinalIgnoreCase))
                {
                    ffrmsg = ffrmsg.Replace("\r\n", "$");
                    string[] str = ffrmsg.Split('$');
                    if (str.Length >= 3)
                    {
                        for (int i = 0; i < str.Length; i++)
                        {
                            flag = true;

                            #region Line 1
                            if (str[i].StartsWith("FFR", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    data.ffrversionnum = msg[1];
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME, "Error while reading ffr header.", "");
                                    _logger.LogError(ex, PAGE_NAME, FUN_NAME, "Error while reading ffr header.");

                                }
                            }
                            #endregion

                            #region Line Consigment Details
                            if (i > 0)
                            {
                                string[] msg = str[i].Split('/');
                                if (msg[0].Contains('-'))
                                {//Decode consigment info
                                    decodeconsigmentdetails(str[i], ref consinfo);
                                }
                            }
                            #endregion

                            #region Line 3 flight details
                            if (i > 1 && !str[i].StartsWith("/") && !str[i].Contains('-'))
                            {
                                try
                                {
                                    MessageData.FltRoute flight = new MessageData.FltRoute("");
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 0 && msg[0].Length > 3)
                                    {
                                        flight.carriercode = msg[0].Substring(0, 2);
                                        flight.fltnum = msg[0].Substring(2);
                                        flight.date = msg[1].Substring(0, 2);
                                        flight.month = msg[1].Substring(2);
                                        flight.fltdept = msg[2].Substring(0, 3);
                                        flight.fltarrival = msg[2].Substring(3);
                                        try
                                        {
                                            flight.spaceallotmentcode = msg[3].Length > 0 ? msg[3] : "";
                                            if (msg[4].Length > 0)
                                            {
                                                flight.allotidentification = msg[4].ToString();
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            // clsLog.WriteLogAzure(ex.Message);
                                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        }
                                        Array.Resize(ref fltroute, fltroute.Length + 1);
                                        fltroute[fltroute.Length - 1] = flight;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex);
                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    continue;
                                }
                            }
                            #endregion

                            #region Line 4 ULD Specification
                            if (str[i].StartsWith("ULD", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    int uldnum = 0;
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        data.noofuld = msg[1];
                                        if (int.Parse(msg[1]) > 0)
                                        {
                                            for (int k = 2; k < msg.Length; k += 2)
                                            {
                                                string[] splitstr = msg[k].Split('-');
                                                uld[uldnum].uldtype = splitstr[0].Substring(0, 3);
                                                uld[uldnum].uldsrno = splitstr[0].Substring(3, splitstr[0].Length - 6);
                                                uld[uldnum].uldowner = splitstr[0].Substring(splitstr[0].Length - 3, 3);
                                                uld[uldnum].uldloadingindicator = splitstr[1];
                                                uld[uldnum].uldweightcode = msg[k + 1].Substring(0, 1);
                                                uld[uldnum].uldweight = msg[k + 1].Substring(1);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                                    _logger.LogError(ex, PAGE_NAME, FUN_NAME);
                                }
                            }
                            #endregion

                            #region Line 5 Special Service request
                            if (str[i].StartsWith("SSR", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    lastrec = msg[0];
                                    line = 0;
                                    if (msg[1].Length > 0)
                                    {
                                        data.specialservicereq1 = msg[1];
                                    }

                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                                    _logger.LogError(ex, PAGE_NAME, FUN_NAME);
                                }
                            }
                            #endregion

                            #region Line 6 Other service info
                            if (str[i].StartsWith("OSI", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    lastrec = msg[0];
                                    line = 0;
                                    if (msg[1].Length > 0)
                                    {
                                        data.otherserviceinfo1 = msg[1];
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                                    _logger.LogError(ex, PAGE_NAME, FUN_NAME);
                                }
                            }
                            #endregion

                            #region Line 7 booking reference
                            if (str[i].StartsWith("REF", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        data.bookingrefairport = msg[1].Substring(0, 3);
                                        data.officefundesignation = msg[1].Substring(3, 2);
                                        data.companydesignator = msg[1].Substring(5, 2);
                                        data.participentidetifier = msg[2].Length > 0 ? msg[2] : null;
                                        data.participentcode = msg[3].Length > 0 ? msg[3] : null;
                                        data.participentairportcity = msg[4].Length > 0 ? msg[4] : null;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                                    _logger.LogError(ex, PAGE_NAME, FUN_NAME);
                                }
                            }
                            #endregion

                            #region Line 8 Dimendion info
                            if (str[i].StartsWith("DIM", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    lastrec = "DIM";
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
                                        if (msg[1].Substring(0, 1).Equals("K", StringComparison.OrdinalIgnoreCase) || msg[1].Substring(0, 1).Equals("L", StringComparison.OrdinalIgnoreCase))
                                        {
                                            dimension.weightcode = msg[1].Substring(0, 1);
                                            dimension.weight = msg[1].Substring(1);
                                        }
                                        if (msg.Length > 0)
                                        {
                                            string select = "";
                                            for (int n = 0; n < msg.Length; n++)
                                            {
                                                if (msg[n].Contains('-'))
                                                {
                                                    select = msg[n];
                                                }
                                            }
                                            string[] dimstr = select.Split('-');
                                            dimension.mesurunitcode = dimstr[0].Substring(0, 3);
                                            dimension.length = dimstr[0].Substring(3);
                                            dimension.width = dimstr[1];
                                            dimension.height = dimstr[2];
                                        }
                                        try
                                        {
                                            int val;
                                            if (int.TryParse(msg[msg.Length - 1], out val))
                                            {
                                                dimension.piecenum = msg[msg.Length - 1];
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // clsLog.WriteLogAzure(ex.Message);
                                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        }
                                        Array.Resize(ref objDimension, objDimension.Length + 1);
                                        objDimension[objDimension.Length - 1] = dimension;

                                    }
                                }
                                catch (Exception e8)
                                {
                                    // clsLog.WriteLogAzure(e8, PAGE_NAME, FUN_NAME);
                                    _logger.LogError(e8, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                }
                            }
                            #endregion

                            #region Line 9 Product information
                            if (str[i].StartsWith("PID", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        data.servicecode = msg[1];
                                        data.rateclasscode = msg[2];
                                        data.commoditycode = msg[3];
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                                    _logger.LogError(ex, PAGE_NAME, FUN_NAME);
                                }
                            }
                            #endregion

                            #region Line 10 Shipper Infor
                            if (str[i].StartsWith("SHP", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    lastrec = msg[0];
                                    line = 0;
                                    if (msg.Length > 1)
                                    {
                                        data.shipperaccnum = msg[1];

                                    }
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                                    _logger.LogError(ex, PAGE_NAME, FUN_NAME);
                                }

                                #endregion

                                #region Line 11 Consignee
                                if (str[i].StartsWith("CNE", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = msg[0];
                                        line = 0;
                                        if (msg.Length > 1)
                                        {
                                            data.consaccnum = msg[1];
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                                        _logger.LogError(ex, PAGE_NAME, FUN_NAME);
                                    }
                                }
                                #endregion

                                #region Line 12 Customer Identification
                                if (str[i].StartsWith("CUS", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = msg[0];
                                        if (msg.Length > 1)
                                        {
                                            data.custaccnum = msg[1].Length > 0 ? msg[1] : "";
                                            data.iatacargoagentcode = msg[2].Length > 0 ? msg[2] : "";
                                            data.cargoagentcasscode = msg[3].Length > 0 ? msg[3] : "";
                                            data.participentidetifier = msg[4].Length > 0 ? msg[4] : "";

                                        }
                                    }
                                    catch (Exception e12)
                                    {
                                        // clsLog.WriteLogAzure(e12, PAGE_NAME, FUN_NAME);
                                        _logger.LogError(e12, PAGE_NAME, FUN_NAME);
                                    }
                                }
                                #endregion

                                #region Line 13 shipment refence info
                                if (str[i].StartsWith("SRI", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 1)
                                        {
                                            data.shiprefnum = msg[1].Length > 0 ? msg[1] : null;
                                            data.supplemetryshipperinfo1 = msg[2].Length > 0 ? msg[2] : null;
                                            data.supplemetryshipperinfo2 = msg[3].Length > 0 ? msg[3] : null;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                                        _logger.LogError(ex, PAGE_NAME, FUN_NAME);
                                    }
                                }
                                #endregion

                                #region Other Info
                                if (str[i].StartsWith("/"))
                                {
                                    string[] msg = str[i].Split('/');
                                    try
                                    {
                                        if (lastrec == "SSR")
                                        {
                                            data.specialservicereq2 = msg[1].Length > 0 ? msg[1] : "";
                                        }
                                        if (lastrec == "OSI")
                                        {
                                            data.otherserviceinfo2 = msg[1].Length > 0 ? msg[1] : "";
                                        }
                                        #region SHP Data
                                        if (lastrec == "SHP")
                                        {
                                            line++;
                                            if (line == 1)
                                            {
                                                data.shippername = msg[1].Length > 0 ? msg[1] : "";
                                            }
                                            if (line == 2)
                                            {
                                                data.shipperadd = msg[1].Length > 0 ? msg[1] : "";

                                            }
                                            if (line == 3)
                                            {
                                                data.shipperplace = msg[1].Length > 0 ? msg[1] : "";
                                                data.shipperstate = msg[2].Length > 0 ? msg[2] : "";
                                            }
                                            if (line == 4)
                                            {
                                                data.shippercountrycode = msg[1].Length > 0 ? msg[1] : "";
                                                data.shipperpostcode = msg[2].Length > 0 ? msg[2] : "";
                                                data.shippercontactidentifier = msg[3].Length > 0 ? msg[3] : "";
                                                data.shippercontactnum = msg[4].Length > 0 ? msg[4] : "";

                                            }

                                        }
                                        #endregion

                                        #region CNE Data
                                        if (lastrec == "CNE")
                                        {
                                            line++;
                                            if (line == 1)
                                            {
                                                data.consname = msg[1].Length > 0 ? msg[1] : "";
                                            }
                                            if (line == 2)
                                            {
                                                data.consadd = msg[1].Length > 0 ? msg[1] : "";
                                            }
                                            if (line == 3)
                                            {
                                                data.consplace = msg[1].Length > 0 ? msg[1] : "";
                                                data.consstate = msg[2].Length > 0 ? msg[2] : "";
                                            }
                                            if (line == 4)
                                            {
                                                data.conscountrycode = msg[1].Length > 0 ? msg[1] : "";
                                                data.conspostcode = msg[2].Length > 0 ? msg[2] : "";
                                                data.conscontactidentifier = msg[3].Length > 0 ? msg[3] : "";
                                                data.conscontactnum = msg[4].Length > 0 ? msg[4] : "";
                                            }

                                        }
                                        #endregion

                                        #region CUS data
                                        if (lastrec == "CUS")
                                        {
                                            line++;
                                            if (line == 1)
                                            {
                                                data.custname = msg[1].Length > 0 ? msg[1] : "";

                                            }
                                            if (line == 2)
                                            {
                                                data.custplace = msg[1].Length > 0 ? msg[1] : "";
                                            }
                                        }
                                        #endregion

                                        #region DIM info
                                        if (lastrec.Equals("DIM", StringComparison.OrdinalIgnoreCase))
                                        {
                                            try
                                            {
                                                if (msg.Length > 1)
                                                {
                                                    MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
                                                    dimension.weightcode = msg[1].Substring(0, 1);
                                                    dimension.weight = msg[1].Substring(1);
                                                    if (msg.Length > 0)
                                                    {
                                                        string[] dimstr = msg[2].Split('-');
                                                        dimension.mesurunitcode = dimstr[0].Substring(0, 3);
                                                        dimension.length = dimstr[0].Substring(3);
                                                        dimension.width = dimstr[1];
                                                        dimension.height = dimstr[2];
                                                    }
                                                    try
                                                    {
                                                        dimension.piecenum = msg[3];
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        //  clsLog.WriteLogAzure(ex.Message);
                                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                    }
                                                    Array.Resize(ref objDimension, objDimension.Length + 1);
                                                    objDimension[objDimension.Length - 1] = dimension;

                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex.Message); }
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                        }

                                        #endregion
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                                        _logger.LogError(ex, PAGE_NAME, FUN_NAME);
                                    }
                                }
                                #endregion
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                _logger.LogError(ex, PAGE_NAME, FUN_NAME);
            }
            return flag;
        }
        #endregion

        #region Encode FFR
        public static string encodeFFRforsend(ref MessageData.ffrinfo data, ref MessageData.ULDinfo[] uld)
        {
            string ffr = null;
            const String FUN_NAME = "encodeFFRforsend";
            try
            {
                #region Line 1
                string line1 = "FFR" + "/" + data.ffrversionnum;
                #endregion

                #region Line 2 AWB Data
                //  string splhandling = "";
                string line2 = string.Empty;
                #endregion

                #region Line 3 Flight Schedule
                string line3 = "";
                #endregion

                #region Line 4
                string line4 = "";
                if (data.noofuld.Length > 0)
                {
                    line4 = "ULD/" + data.noofuld + "/";
                    string uldinfo = null;
                    for (int i = 0; i < int.Parse(data.noofuld); i++)
                    {
                        uldinfo = null;
                        uldinfo = uld[i].uldtype + uld[i].uldsrno + uld[i].uldowner + "-" + uld[i].uldloadingindicator + "/" + uld[i].uldweightcode + uld[i].uldweight;
                        if (uldinfo.Length > 2)
                            line4 = line4 + uldinfo + "/";
                    }
                }
                #endregion

                #region Line 5
                string line5 = "";
                if (data.specialservicereq1.Length > 0 || data.specialservicereq2.Length > 0)
                {
                    line5 = "SSR/" + data.specialservicereq1 + "\r\n" + "/" + data.specialservicereq2;
                }
                #endregion

                #region Line 6
                string line6 = "";
                if (data.otherserviceinfo1.Length > 0 || data.otherserviceinfo2.Length > 0)
                {
                    line6 = "SSR/" + data.otherserviceinfo1 + "\r\n" + "/" + data.otherserviceinfo2;
                }
                #endregion

                #region Line 7
                string line7 = "";
                line7 = "REF/" + data.bookingrefairport + data.officefundesignation + data.companydesignator;
                if (data.bookingfileref.Length > 0)
                {
                    line7 = line7 + "/" + data.bookingfileref;
                }
                if (data.participentidetifier.Length > 0)
                {
                    line7 = line7 + "/" + data.participentidetifier;
                }
                if (data.participentcode.Length > 0)
                {
                    line7 = line7 + "/" + data.participentcode;
                }
                if (data.participentairportcity.Length > 0)
                {
                    line7 = line7 + "/" + data.participentairportcity;
                }
                #endregion

                #region Line 8 Dimension Info
                string line8 = "";


                #endregion

                #region Line 9
                string line9 = "";
                if (data.servicecode.Length > 0 || data.rateclasscode.Length > 0 || data.commoditycode.Length > 0)
                {
                    line9 = "PID/" + data.servicecode + "/" + data.rateclasscode + "/" + data.commoditycode;
                }
                #endregion

                #region Line 10
                string line10 = "";
                string str1 = "", str2 = "", str3 = "", str4 = "";
                try
                {
                    if (data.shippername.Length > 0)
                    {
                        str1 = "/" + data.shippername;
                    }
                    if (data.shipperadd.Length > 0)
                    {
                        str2 = "/" + data.shipperadd;
                    }

                    if (data.shipperplace.Length > 0 || data.shipperstate.Length > 0)
                    {
                        str3 = "/" + data.shipperplace + "/" + data.shipperstate;
                    }
                    if (data.shippercountrycode.Length > 0 || data.shipperpostcode.Length > 0 || data.shippercontactidentifier.Length > 0 || data.shippercontactnum.Length > 0)
                    {
                        str4 = "/" + data.shippercountrycode + "/" + data.shipperpostcode + "/" + data.shippercontactidentifier + "/" + data.shippercontactnum;
                    }

                    if (data.shipperaccnum.Length > 0 || str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
                    {
                        line10 = "SHP/" + data.shipperaccnum;
                        if (str4.Length > 0)
                        {
                            line10 = line10.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                        }
                        else if (str3.Length > 0)
                        {
                            line10 = line10.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
                        }
                        else if (str2.Length > 0)
                        {
                            line10 = line10.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                        }
                        else if (str1.Length > 0)
                        {
                            line10 = line10.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception ex) {
                    // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME); 
                    _staticLogger.LogError(ex, "Error on Line 10");
                     }
                #endregion

                #region Line 11
                string line11 = "";
                str1 = "";
                str2 = "";
                str3 = "";
                str4 = "";
                try
                {
                    if (data.consname.Length > 0)
                    {
                        str1 = "/" + data.consname;
                    }
                    if (data.consadd.Length > 0)
                    {
                        str2 = "/" + data.consadd;
                    }

                    if (data.consplace.Length > 0 || data.consstate.Length > 0)
                    {
                        str3 = "/" + data.custplace + "/" + data.consstate;
                    }
                    if (data.conscountrycode.Length > 0 || data.conspostcode.Length > 0 || data.conscontactidentifier.Length > 0 || data.conscontactnum.Length > 0)
                    {
                        str4 = "/" + data.conscountrycode + "/" + data.conspostcode + "/" + data.conscontactidentifier + "/" + data.conscontactnum;
                    }

                    if (data.consaccnum.Length > 0 || str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
                    {
                        line11 = "CNE/" + data.consaccnum;
                        if (str4.Length > 0)
                        {
                            line11 = line11.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                        }
                        else if (str3.Length > 0)
                        {
                            line11 = line11.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
                        }
                        else if (str2.Length > 0)
                        {
                            line11 = line11.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                        }
                        else if (str1.Length > 0)
                        {
                            line11 = line11.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception ex) {
                    // clsLog.WriteLogAzure(ex.Message); 
                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                     }
                #endregion

                #region Line 12
                string line12 = "";
                str1 = "";
                str2 = "";
                try
                {
                    if (data.custname.Length > 0)
                    {
                        str1 = "/" + data.custname;
                    }
                    if (data.custplace.Length > 0)
                    {
                        str2 = "/" + data.custplace;
                    }
                    if (data.custaccnum.Length > 0 || data.iatacargoagentcode.Length > 0 || data.cargoagentcasscode.Length > 0 || data.participentidetifier.Length > 0 || str1.Length > 0 || str2.Length > 0)
                    {
                        line12 = "CUS/" + data.shipperaccnum + "/" + data.iatacargoagentcode + "/" + data.cargoagentcasscode + "/" + data.participentidetifier;
                        if (str2.Length > 0)
                        {
                            line12 = line12.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                        }
                        else if (str1.Length > 0)
                        {
                            line12 = line12.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception ex) {
                    // clsLog.WriteLogAzure(ex.Message);
                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                #endregion

                #region Line 13
                string line13 = "";
                if (data.shiprefnum.Length > 0 || data.supplemetryshipperinfo1.Length > 0 || data.supplemetryshipperinfo2.Length > 0)
                {
                    line13 = "SRI/" + data.shiprefnum + "/" + data.supplemetryshipperinfo1 + "/" + data.supplemetryshipperinfo2;
                }
                #endregion

                #region BuildFFR
                ffr = line1.Trim('/') + "\r\n" + line2.Trim('/') + "\r\n" + line3.Trim('/');
                if (line4.Length > 0)
                {
                    ffr = ffr + "\r\n" + line4.Trim('/');
                }
                if (line5.Length > 0)
                {
                    ffr = ffr + "\r\n" + line5.Trim('/');
                }
                if (line6.Length > 0)
                {
                    ffr = ffr + "\r\n" + line6.Trim('/');
                }
                if (line7.Length > 0)
                {
                    ffr = ffr + "\r\n" + line7.Trim('/');
                }
                if (line8.Length > 0)
                {
                    ffr = ffr + "\r\n" + line8.Trim('/');
                }
                if (line9.Length > 0)
                {
                    ffr = ffr + "\r\n" + line9.Trim('/');
                }
                if (line10.Length > 0)
                {
                    ffr = ffr + "\r\n" + line10.Trim('/');
                }
                if (line11.Length > 0)
                {
                    ffr = ffr + "\r\n" + line11.Trim('/');
                }
                if (line12.Length > 0)
                {
                    ffr = ffr + "\r\n" + line12.Trim('/');
                }
                if (line13.Length > 0)
                {
                    ffr = ffr + "\r\n" + line13.Trim('/');
                }
                #endregion

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
                ffr = "ERR";
            }
            return ffr;
        }

        public static string encodeFFRforsend(ref MessageData.ffrinfo data, ref MessageData.ULDinfo[] uld, ref MessageData.consignmnetinfo consigment, ref MessageData.FltRoute[] FltRoute, ref MessageData.dimensionnfo[] dimension)
        {
            string ffr = null;
            const string FUN_NAME = "encodeFFRforsend";
            try
            {
                #region Line 1
                string line1 = "FFR" + "/" + data.ffrversionnum;
                #endregion

                #region Line 2 AWB Data


                string line2 = consigment.airlineprefix + "-" + consigment.awbnum + consigment.origin + consigment.dest + "/" + consigment.consigntype + consigment.pcscnt + consigment.weightcode + consigment.weight + consigment.volumecode + consigment.volumeamt + consigment.densityindicator + consigment.densitygrp + consigment.shpdesccode + consigment.numshp + "/" + consigment.manifestdesc + consigment.splhandling;
                #endregion

                #region Line 3 Flight Schedule
                string line3 = "";
                if (FltRoute.Length > 0)
                {
                    for (int i = 0; i < FltRoute.Length; i++)
                    {
                        line3 = line3 + FltRoute[i].carriercode + FltRoute[i].fltnum + "/" + FltRoute[i].date + FltRoute[i].month + "/" + FltRoute[i].fltdept + FltRoute[i].fltarrival + "/" + FltRoute[i].spaceallotmentcode + (FltRoute[i].allotidentification.Length > 0 ? ("/" + FltRoute[i].allotidentification) : "") + "$";
                    }
                }
                line3 = line3.Trim('$');
                line3 = line3.Replace("$", "\r\n");
                //
                #endregion

                #region Line 4
                string line4 = "";
                if (data.noofuld.Length > 0)
                {
                    line4 = "ULD/" + data.noofuld + "/";
                    string uldinfo = null;
                    for (int i = 0; i < int.Parse(data.noofuld); i++)
                    {
                        uldinfo = null;
                        uldinfo = uld[i].uldtype + uld[i].uldsrno + uld[i].uldowner + "-" + uld[i].uldloadingindicator + "/" + uld[i].uldweightcode + uld[i].uldweight;
                        if (uldinfo.Length > 2)
                            line4 = line4 + uldinfo + "/";
                    }
                }
                #endregion

                #region Line 5
                string line5 = "";
                if (data.specialservicereq1.Length > 0 || data.specialservicereq2.Length > 0)
                {
                    line5 = "SSR/" + data.specialservicereq1 + "\r\n" + "/" + data.specialservicereq2;
                }
                #endregion

                #region Line 6
                string line6 = "";
                if (data.otherserviceinfo1.Length > 0 || data.otherserviceinfo2.Length > 0)
                {
                    line6 = "SSR/" + data.otherserviceinfo1 + "\r\n" + "/" + data.otherserviceinfo2;
                }
                #endregion

                #region Line 7
                string line7 = "";
                line7 = "REF/" + data.bookingrefairport + data.officefundesignation + data.companydesignator;
                if (data.bookingfileref.Length > 0)
                {
                    line7 = line7 + "/" + data.bookingfileref;
                }
                if (data.participentidetifier.Length > 0)
                {
                    line7 = line7 + "/" + data.participentidetifier;
                }
                if (data.participentcode.Length > 0)
                {
                    line7 = line7 + "/" + data.participentcode;
                }
                if (data.participentairportcity.Length > 0)
                {
                    line7 = line7 + "/" + data.participentairportcity;
                }
                #endregion

                #region Line 8 Dimension Info
                string line8 = "";
                if (dimension.Length > 0)
                {
                    for (int i = 0; i < dimension.Length; i++)
                    {
                        if (dimension[i].weight.Length > 0 || dimension[i].length.Length > 0 || dimension[i].width.Length > 0 || dimension[i].height.Length > 0 || dimension[i].piecenum.Length > 0)
                        {
                            line8 = line8 + dimension[i].weightcode + dimension[i].weight + "/" + dimension[i].mesurunitcode + dimension[i].length + "-" + dimension[i].width + "-" + dimension[i].height + "/" + dimension[i].piecenum + "$";
                        }
                    }
                }
                line8 = line8.Trim('$');
                if (line8.Length > 0)
                {
                    line8 = "DIM/" + line8.Replace("$", "\r\n");
                }
                #endregion

                #region Line 9
                string line9 = "";
                if (data.servicecode.Length > 0 || data.rateclasscode.Length > 0 || data.commoditycode.Length > 0)
                {
                    // line9 = "PID/" + data.servicecode + "/" + data.rateclasscode + "/" + data.commoditycode;
                    line9 = "PID/" + data.producttype;
                }
                #endregion

                #region Line 10
                string line10 = "";
                string str1 = "", str2 = "", str3 = "", str4 = "";
                try
                {
                    if (data.shippername.Length > 0)
                    {
                        str1 = "/" + data.shippername;
                    }
                    if (data.shipperadd.Length > 0)
                    {
                        str2 = "/" + data.shipperadd;
                    }

                    if (data.shipperplace.Length > 0 || data.shipperstate.Length > 0)
                    {
                        str3 = "/" + data.shipperplace + "/" + data.shipperstate;
                    }
                    if (data.shippercountrycode.Length > 0 || data.shipperpostcode.Length > 0 || data.shippercontactidentifier.Length > 0 || data.shippercontactnum.Length > 0)
                    {
                        str4 = "/" + data.shippercountrycode + "/" + data.shipperpostcode + "/" + data.shippercontactidentifier + "/" + data.shippercontactnum;
                    }

                    if (data.shipperaccnum.Length > 0 || str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
                    {
                        line10 = "SHP/" + data.shipperaccnum;
                        if (str4.Length > 0)
                        {
                            line10 = line10.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                        }
                        else if (str3.Length > 0)
                        {
                            line10 = line10.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
                        }
                        else if (str2.Length > 0)
                        {
                            line10 = line10.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                        }
                        else if (str1.Length > 0)
                        {
                            line10 = line10.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception ex) {
                    // clsLog.WriteLogAzure(ex.Message);
                     _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                      }
                #endregion

                #region Line 11
                string line11 = "";
                str1 = "";
                str2 = "";
                str3 = "";
                str4 = "";
                try
                {
                    if (data.consname.Length > 0)
                    {
                        str1 = "/" + data.consname;
                    }
                    if (data.consadd.Length > 0)
                    {
                        str2 = "/" + data.consadd;
                    }

                    if (data.consplace.Length > 0 || data.consstate.Length > 0)
                    {
                        str3 = "/" + data.custplace + "/" + data.consstate;
                    }
                    if (data.conscountrycode.Length > 0 || data.conspostcode.Length > 0 || data.conscontactidentifier.Length > 0 || data.conscontactnum.Length > 0)
                    {
                        str4 = "/" + data.conscountrycode + "/" + data.conspostcode + "/" + data.conscontactidentifier + "/" + data.conscontactnum;
                    }

                    if (data.consaccnum.Length > 0 || str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
                    {
                        line11 = "CNE/" + data.consaccnum;
                        if (str4.Length > 0)
                        {
                            line11 = line11.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                        }
                        else if (str3.Length > 0)
                        {
                            line11 = line11.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
                        }
                        else if (str2.Length > 0)
                        {
                            line11 = line11.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                        }
                        else if (str1.Length > 0)
                        {
                            line11 = line11.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception ex) {
                    // clsLog.WriteLogAzure(ex.Message); 
                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");}
                #endregion

                #region Line 12
                string line12 = "";
                str1 = "";
                str2 = "";
                try
                {
                    if (data.custname.Length > 0)
                    {
                        str1 = "/" + data.custname;
                    }
                    if (data.custplace.Length > 0)
                    {
                        str2 = "/" + data.custplace;
                    }
                    if (data.custaccnum.Length > 0 || data.iatacargoagentcode.Length > 0 || data.cargoagentcasscode.Length > 0 || data.participentidetifier.Length > 0 || str1.Length > 0 || str2.Length > 0)
                    {
                        line12 = "CUS/" + data.shipperaccnum + "/" + data.iatacargoagentcode + "/" + data.cargoagentcasscode + "/" + data.participentidetifier;
                        if (str2.Length > 0)
                        {
                            line12 = line12.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                        }
                        else if (str1.Length > 0)
                        {
                            line12 = line12.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception ex) {
                    // clsLog.WriteLogAzure(ex.Message); 
                     _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                     }
                #endregion

                #region Line 13
                string line13 = "";
                if (data.shiprefnum.Length > 0 || data.supplemetryshipperinfo1.Length > 0 || data.supplemetryshipperinfo2.Length > 0)
                {
                    line13 = "SRI/" + data.shiprefnum + "/" + data.supplemetryshipperinfo1 + "/" + data.supplemetryshipperinfo2;
                }
                #endregion

                #region BuildFFR
                ffr = line1.Trim('/') + "\r\n" + line2.Trim('/') + "\r\n" + line3.Trim('/');
                if (line4.Length > 0)
                {
                    ffr = ffr + "\r\n" + line4.Trim('/');
                }
                if (line5.Length > 0)
                {
                    ffr = ffr + "\r\n" + line5.Trim('/');
                }
                if (line6.Length > 0)
                {
                    ffr = ffr + "\r\n" + line6.Trim('/');
                }
                if (line7.Length > 0)
                {
                    ffr = ffr + "\r\n" + line7.Trim('/');
                }
                if (line8.Length > 0)
                {
                    ffr = ffr + "\r\n" + line8.Trim('/');
                }
                if (line9.Length > 0)
                {
                    ffr = ffr + "\r\n" + line9.Trim('/');
                }
                if (line10.Length > 0)
                {
                    ffr = ffr + "\r\n" + line10.Trim('/');
                }
                if (line11.Length > 0)
                {
                    ffr = ffr + "\r\n" + line11.Trim('/');
                }
                if (line12.Length > 0)
                {
                    ffr = ffr + "\r\n" + line12.Trim('/');
                }
                if (line13.Length > 0)
                {
                    ffr = ffr + "\r\n" + line13.Trim('/');
                }
                #endregion

            }
            catch (Exception ex)
            {
                ffr = "ERR";
                // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
            }
            return ffr;
        }
        #endregion

        //FFA

        #region Decode FFA
        public bool decodereceivedffa(string ffamsg, ref MessageData.ffainfo ffadata)
        {
            bool flag = false;
            string lastrec = "NA";
            //   int line = 0;
            const string FUN_NAME = "decodereceivedffa";
            try
            {
                if (ffamsg.StartsWith("FFA", StringComparison.OrdinalIgnoreCase))
                {
                    // ffrmsg = ffrmsg.Replace("\r\n","$");
                    //string[] str = Regex.Split(ffamsg, "\r\n");//ffrmsg.Split('$');
                    string[] str = ffamsg.Split('$');
                    if (str.Length > 3)
                    {
                        for (int i = 0; i < str.Length; i++)
                        {
                            flag = true;

                            #region Line 1
                            if (str[i].StartsWith("FFA", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    ffadata.ffaversionnum = msg[1];
                                }
                                catch (Exception ex) {
                                    // clsLog.WriteLogAzure(ex.Message);
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                 }
                            }
                            #endregion

                            #region Line 2
                            if (i == 1)
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    //0th element
                                    string[] decmes = msg[0].Split('-');
                                    ffadata.airlineprefix = decmes[0];
                                    ffadata.awbnum = decmes[1].Substring(0, decmes[1].Length - 6);
                                    ffadata.origin = decmes[1].Substring(decmes[1].Length - 6, 3);
                                    ffadata.dest = decmes[1].Substring(decmes[1].Length - 3, 3);
                                    //1
                                    if (msg[1].Length > 0)
                                    {
                                        int k = 0;
                                        char lastchr = 'A';
                                        char[] arr = msg[1].ToCharArray();
                                        string[] strarr = new string[arr.Length];
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
                                        ffadata.consigntype = strarr[0];
                                        ffadata.pcscnt = strarr[1];//int.Parse(strarr[1]);
                                        ffadata.weightcode = strarr[2];
                                        ffadata.weight = strarr[3];//float.Parse(strarr[3]);
                                        for (k = 4; k < strarr.Length; k += 2)
                                        {
                                            if (strarr[k] != null)
                                            {
                                                if (strarr[k] == "T")
                                                {
                                                    ffadata.shpdesccode = strarr[k];
                                                    ffadata.numshp = strarr[k + 1];
                                                    k = strarr.Length + 1;
                                                }
                                            }
                                        }
                                    }
                                    if (msg.Length > 2)
                                    {
                                        //2
                                        ffadata.manifestdesc = msg[2];

                                    }
                                    if (msg.Length > 2)
                                    {//3
                                        ffadata.splhandling = "";
                                        for (int j = 3; j < msg.Length; j++)
                                            ffadata.splhandling = ffadata.splhandling + msg[j] + ",";
                                    }

                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex);
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    continue;
                                }
                            }
                            #endregion

                            #region Line 3 flight details
                            if (i == 2)
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 0)
                                    {
                                        ffadata.carriercode = msg[0].Substring(0, 2);
                                        ffadata.fltnum = msg[0].Substring(2);
                                        ffadata.date = msg[1].Substring(0, 2);
                                        ffadata.month = msg[1].Substring(2);
                                        ffadata.fltdept = msg[2].Substring(0, 3);
                                        ffadata.fltarrival = msg[2].Substring(3);
                                        ffadata.spaceallotmentcode = msg[3].Length > 0 ? msg[3] : null;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex);
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    continue;
                                }
                            }
                            #endregion

                            #region Line 5 Special Service request
                            if (str[i].StartsWith("SSR", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    lastrec = msg[0];
                                    //   line = 0;
                                    if (msg[1].Length > 0)
                                    {
                                        ffadata.specialservicereq1 = msg[1];
                                    }

                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex.Message); 
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                }
                            }
                            #endregion

                            #region Line 6 Other service info
                            if (str[i].StartsWith("OSI", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    lastrec = msg[0];
                                    //     line = 0;
                                    if (msg[1].Length > 0)
                                    {
                                        ffadata.otherserviceinfo1 = msg[1];
                                    }
                                }
                                catch (Exception ex) {
                                    // clsLog.WriteLogAzure(ex.Message); 
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                }
                            }
                            #endregion

                            #region Line 7 booking reference
                            if (str[i].StartsWith("REF", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        ffadata.bookingrefairport = msg[1].Substring(0, 3);
                                        ffadata.officefundesignation = msg[1].Substring(3, 2);
                                        ffadata.companydesignator = msg[1].Substring(5, 2);
                                        ffadata.participentidetifier = msg[2].Length > 0 ? msg[2] : null;
                                        ffadata.participentcode = msg[3].Length > 0 ? msg[3] : null;
                                        ffadata.participentairportcity = msg[4].Length > 0 ? msg[4] : null;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex.Message); 
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                }
                            }
                            #endregion

                            #region Line 8 shipment refence info
                            if (str[i].StartsWith("SRI", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        ffadata.shiprefnum = msg[1].Length > 0 ? msg[1] : null;
                                        ffadata.supplemetryshipperinfo1 = msg[2].Length > 0 ? msg[2] : null;
                                        ffadata.supplemetryshipperinfo2 = msg[3].Length > 0 ? msg[3] : null;
                                    }
                                }
                                catch (Exception ex) {
                                    // clsLog.WriteLogAzure(ex.Message);
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                 }
                            }
                            #endregion

                            #region Other Info
                            if (str[i].StartsWith("/"))
                            {
                                string[] msg = str[i].Split('/');
                                try
                                {
                                    if (lastrec == "SSR")
                                    {
                                        ffadata.specialservicereq2 = msg[1].Length > 0 ? msg[1] : "";
                                    }
                                    if (lastrec == "OSI")
                                    {
                                        ffadata.otherserviceinfo2 = msg[1].Length > 0 ? msg[1] : "";
                                    }

                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex.Message);
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                 }
                            }
                            #endregion
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                _logger.LogError(ex, PAGE_NAME, FUN_NAME);
            }
            return flag;
        }
        #endregion



        //FBL

        /*Not in use**/
        #region decode FBL
        //private bool decodereceiveFBL(string fblmsg, ref MessageData.fblinfo fbldata, ref MessageData.unloadingport[] unloadingport, ref MessageData.dimensionnfo[] dimensioinfo, ref MessageData.ULDinfo[] uld, ref MessageData.otherserviceinfo othinfo, ref MessageData.consignmentorigininfo[] consorginfo, ref MessageData.consignmnetinfo[] consinfo)
        //{
        //    bool flag = false;
        //    const string FUN_NAME = "decodereceiveFBL";
        //    try
        //    {
        //        string lastrec = "NA";

        //        try
        //        {
        //            if (fblmsg.StartsWith("FBL", StringComparison.OrdinalIgnoreCase))
        //            {

        //                string[] str = Regex.Split(fblmsg, "$");
        //                if (str.Length > 3)
        //                {
        //                    for (int i = 0; i < str.Length; i++)
        //                    {

        //                        flag = true;
        //                        #region Line 1
        //                        if (str[i].StartsWith("FBL", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                fbldata.fblversion = msg[1];
        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region line 2 flight data
        //                        if (i == 1)
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                if (msg.Length > 1)
        //                                {
        //                                    fbldata.messagesequencenum = msg[0];
        //                                    fbldata.carriercode = msg[1].Substring(0, 2);
        //                                    fbldata.fltnum = msg[1].Substring(2);
        //                                    fbldata.date = msg[2].Substring(0, 2);
        //                                    fbldata.month = msg[2].Substring(2);
        //                                    fbldata.fltairportcode = msg[3];
        //                                    fbldata.aircraftregistration = msg[4];
        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region line 3 point of unloading
        //                        if (i >= 2)
        //                        {
        //                            MessageData.unloadingport unloading = new MessageData.unloadingport("");
        //                            if (str[i].Contains('/'))
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                if (msg.Length == 2)
        //                                {
        //                                    if (msg[0].Length > 0 && !msg[0].Equals("SSR", StringComparison.OrdinalIgnoreCase) && !msg[0].Equals("OSI", StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        unloading.unloadingairport = msg[0];
        //                                        unloading.nilcargocode = msg[1];
        //                                        Array.Resize(ref unloadingport, unloadingport.Length + 1);
        //                                        unloadingport[unloadingport.Length - 1] = unloading;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (str[i].Trim().Length == 3)
        //                                {
        //                                    unloading.unloadingairport = str[i];
        //                                    Array.Resize(ref unloadingport, unloadingport.Length + 1);
        //                                    unloadingport[unloadingport.Length - 1] = unloading;
        //                                }
        //                            }
        //                        }
        //                        #endregion

        //                        #region  line 4 onwards check consignment details
        //                        if (i > 1)
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                //0th element
        //                                if (msg[0].Contains('-'))
        //                                {
        //                                    decodeconsigmentdetails(str[i], ref consinfo);
        //                                }

        //                            }
        //                            catch (Exception)
        //                            {
        //                                //clsLog.WriteLogAzure(ex);
        //                                continue;
        //                            }
        //                        }
        //                        #endregion

        //                        #region Line 5 Dimendion info
        //                        if (str[i].StartsWith("DIM", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                int total = msg.Length / 3;
        //                                Array.Resize(ref dimensioinfo, dimensioinfo.Length + total + 1);
        //                                for (int cnt = 0; cnt < total; cnt++)
        //                                {
        //                                    int place = 3 * cnt;
        //                                    MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
        //                                    dimension.weightcode = msg[place + 1].Substring(0, 1);
        //                                    dimension.weight = msg[place + 1].Substring(1);
        //                                    if (msg.Length > 0)
        //                                    {
        //                                        string[] dimstr = msg[place + 2].Split('-');
        //                                        dimension.mesurunitcode = dimstr[0].Substring(0, 3);
        //                                        dimension.length = dimstr[0].Substring(3);
        //                                        dimension.weight = dimstr[1];
        //                                        dimension.height = dimstr[2];
        //                                    }
        //                                    dimension.piecenum = msg[place + 3];
        //                                    dimensioinfo[cnt] = dimension;
        //                                }

        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 7 ULD Specification
        //                        if (str[i].StartsWith("ULD", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                int uldnum = 0;
        //                                string[] msg = str[i].Split('/');
        //                                if (msg.Length > 1)
        //                                {
        //                                    fbldata.noofuld = msg[1];
        //                                    if (int.Parse(msg[1]) > 0)
        //                                    {
        //                                        Array.Resize(ref uld, uld.Length + 1 + int.Parse(msg[1]));
        //                                        for (int k = 2; k < msg.Length; k += 2)
        //                                        {
        //                                            MessageData.ULDinfo ulddata = new MessageData.ULDinfo("");
        //                                            string[] splitstr = msg[k].Split('-');
        //                                            ulddata.uldno = splitstr[0];
        //                                            ulddata.uldtype = splitstr[0].Substring(0, 3);
        //                                            ulddata.uldsrno = splitstr[0].Substring(3, splitstr[0].Length - 6);
        //                                            ulddata.uldowner = splitstr[0].Substring(splitstr[0].Length - 3, 3);
        //                                            ulddata.uldloadingindicator = splitstr[1];
        //                                            ulddata.uldweightcode = msg[k + 1].Substring(0, 1);
        //                                            ulddata.uldweight = msg[k + 1].Substring(1);
        //                                            uld[uldnum++] = ulddata;
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 8 Special Service request
        //                        if (str[i].StartsWith("SSR", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                lastrec = msg[0];
        //                                if (msg[1].Length > 0)
        //                                {
        //                                    fbldata.specialservicereq1 = msg[1];
        //                                }

        //                            }
        //                            catch (Exception ex)
        //                            { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 9 Other service info
        //                        if (str[i].StartsWith("OSI", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                lastrec = msg[0];
        //                                if (msg[1].Length > 0)
        //                                {
        //                                    othinfo.otherserviceinfo1 = msg[1];
        //                                }
        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Last line
        //                        if (i > str.Length - 2)
        //                        {
        //                            if (str[i].Trim().Length == 4 || str[i].Trim().Equals("LAST", StringComparison.OrdinalIgnoreCase) || str[i].Trim().Equals("CONT", StringComparison.OrdinalIgnoreCase))
        //                            {
        //                                fbldata.endmesgcode = str[i].Trim();
        //                            }

        //                        }
        //                        #endregion

        //                        #region Other Info
        //                        if (str[i].StartsWith("/"))
        //                        {
        //                            string[] msg = str[i].Split('/');
        //                            try
        //                            {
        //                                #region line 6 consigment origin info
        //                                if (msg.Length > 0 && msg[0].Length == 0 && lastrec == "NA")
        //                                {
        //                                    MessageData.consignmentorigininfo consorg = new MessageData.consignmentorigininfo();
        //                                    try
        //                                    {
        //                                        consorg.abbrivatedname = msg[1];
        //                                        consorg.carriercode = msg[2].Length > 0 ? msg[2].Substring(0, 2) : "";
        //                                        consorg.flightnum = msg[2].Length > 0 ? msg[2].Substring(2) : "";
        //                                        consorg.day = msg[3].Length > 0 ? msg[3].Substring(0, 2) : "";
        //                                        consorg.month = msg[3].Length > 0 ? msg[3].Substring(2) : "";
        //                                        consorg.airportcode = msg[4];
        //                                        consorg.movementprioritycode = msg[5];
        //                                    }
        //                                    catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                                    Array.Resize(ref consorginfo, consorginfo.Length + 1);
        //                                    consorginfo[consorginfo.Length - 1] = consorg;
        //                                }
        //                                #endregion

        //                                #region SSR 2
        //                                if (lastrec == "SSR")
        //                                {
        //                                    fbldata.specialservicereq2 = msg[1].Length > 0 ? msg[1] : "";
        //                                    lastrec = "NA";
        //                                }
        //                                #endregion

        //                                #region OSI 2
        //                                if (lastrec == "OSI")
        //                                {
        //                                    othinfo.otherserviceinfo2 = msg[1].Length > 0 ? msg[1] : "";
        //                                    lastrec = "NA";
        //                                }
        //                                #endregion
        //                            }
        //                            catch (Exception ex)
        //                            { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
        //            flag = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
        //        flag = false;
        //    }
        //    return flag;
        //}
        #endregion

        #region decode FBL
        public static bool decodereceiveFBL(string fblmsg, ref MessageData.fblinfo fbldata, ref MessageData.unloadingport[] unloadingport, ref MessageData.dimensionnfo[] dimensioinfo, ref MessageData.ULDinfo[] uld, ref MessageData.otherserviceinfo[] othinfo, ref MessageData.consignmentorigininfo[] consorginfo, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.otherserviceinfo[] othinfoarray)
        {
            bool flag = false;
            const string FUN_NAME = "decodereceiveFBL";

            string lastrec = "NA";
            //int line = 0;
            string AWBPrefix = "", AWBNumber = "";
            try
            {
                if (fblmsg.StartsWith("FBL", StringComparison.OrdinalIgnoreCase))
                {

                    string[] str = fblmsg.Split('$');
                    if (str.Length > 3)
                    {
                        for (int i = 0; i < str.Length; i++)
                        {

                            flag = true;
                            #region Line 1
                            if (str[i].StartsWith("FBL", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    fbldata.fblversion = msg[1];
                                }
                                catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
                            }
                            #endregion

                            #region line 2 flight data
                            if (i == 1)
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        fbldata.messagesequencenum = msg[0];
                                        fbldata.carriercode = msg[1].Substring(0, 2);
                                        fbldata.fltnum = msg[1].Substring(2);
                                        fbldata.date = msg[2].Substring(0, 2);
                                        fbldata.month = msg[2].Substring(2);
                                        fbldata.fltairportcode = msg[3];
                                        fbldata.aircraftregistration = msg[4];
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex.Message);
                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");

                                 }
                            }
                            #endregion

                            #region line 3 point of unloading
                            if (i >= 2)
                            {
                                MessageData.unloadingport unloading = new MessageData.unloadingport("");
                                if (str[i].Contains('/') && (!str[i].StartsWith("/")) && (!str[i].Contains(":")) && (!str[i].Contains("-")))
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length == 2)
                                    {
                                        if (msg[0].Length > 0 && !msg[0].StartsWith("/") && !msg[0].Contains('-')
                                            && !msg[0].Equals("SSR", StringComparison.OrdinalIgnoreCase)
                                            && !msg[0].Equals("SCI", StringComparison.OrdinalIgnoreCase)
                                            && !msg[0].Equals("OSI", StringComparison.OrdinalIgnoreCase)
                                            && !msg[0].Equals("ULD", StringComparison.OrdinalIgnoreCase)
                                            && !msg[0].Equals("COR", StringComparison.OrdinalIgnoreCase)
                                             && !msg[0].Equals("OCI", StringComparison.OrdinalIgnoreCase))
                                        {
                                            unloading.unloadingairport = msg[0];
                                            unloading.nilcargocode = msg[1];
                                            Array.Resize(ref unloadingport, unloadingport.Length + 1);
                                            unloadingport[unloadingport.Length - 1] = unloading;
                                        }
                                    }
                                }
                                else
                                {
                                    if (str[i].Trim().Length == 3 && (!str[i].Contains(":")) && (!str[i].StartsWith("/")) && (!str[i].Contains("-")))
                                    {
                                        unloading.unloadingairport = str[i];
                                        Array.Resize(ref unloadingport, unloadingport.Length + 1);
                                        unloadingport[unloadingport.Length - 1] = unloading;
                                    }
                                }
                            }
                            #endregion

                            #region  line 4 onwards check consignment details
                            if (i > 1)
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    //0th element
                                    if (msg[0].Contains('-'))
                                    {
                                        AWBPrefix = "";
                                        AWBNumber = "";
                                        lastrec = "";
                                        decodeconsigmentdetails(str[i], ref consinfo, ref AWBPrefix, ref AWBNumber);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex);
                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    continue;
                                }
                            }
                            #endregion

                            #region Line 5 Dimendion info
                            if (str[i].StartsWith("DIM", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    lastrec = "DIM";
                                    string[] msg = str[i].Split('/');
                                    int place = 0;
                                    MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
                                    try
                                    {
                                        dimension.weightcode = msg[place + 1].Substring(0, 1);
                                        dimension.weight = msg[place + 1].Substring(1);
                                        if (msg.Length > 0)
                                        {
                                            string[] dimstr = msg[place + 2].Split('-');
                                            dimension.mesurunitcode = dimstr[0].Substring(0, 3);
                                            dimension.length = dimstr[0].Substring(3);
                                            dimension.width = dimstr[1];
                                            dimension.height = dimstr[2];
                                        }
                                        dimension.piecenum = msg[place + 3];
                                    }
                                    catch (Exception ex) {
                                        // clsLog.WriteLogAzure(ex.Message); 
                                        _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                    dimension.AWBPrefix = AWBPrefix;
                                    dimension.AWBNumber = AWBNumber;
                                    Array.Resize(ref dimensioinfo, dimensioinfo.Length + 1);
                                    dimensioinfo[dimensioinfo.Length - 1] = dimension;


                                }
                                catch (Exception ex) {
                                    // clsLog.WriteLogAzure(ex.Message);
                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                 }
                            }
                            #endregion

                            #region Line 7 ULD Specification
                            if (str[i].StartsWith("ULD", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    lastrec = "ULD";
                                    int uldnum = 0;
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        fbldata.noofuld = msg[1];
                                        if (int.Parse(msg[1]) > 0)
                                        {
                                            Array.Resize(ref uld, uld.Length + 1 + int.Parse(msg[1]));
                                            for (int k = 2; k < msg.Length; k += 2)
                                            {
                                                MessageData.ULDinfo ulddata = new MessageData.ULDinfo("");
                                                string[] splitstr = msg[k].Split('-');
                                                ulddata.uldno = splitstr[0];
                                                ulddata.uldtype = splitstr[0].Substring(0, 3);
                                                ulddata.uldsrno = splitstr[0].Substring(3, splitstr[0].Length - 6);
                                                ulddata.uldowner = splitstr[0].Substring(splitstr[0].Length - 3, 3);
                                                ulddata.uldloadingindicator = splitstr[1];
                                                ulddata.uldweightcode = msg[k + 1].Substring(0, 1);
                                                ulddata.uldweight = msg[k + 1].Substring(1);
                                                ulddata.AWBPrefix = AWBPrefix;
                                                ulddata.AWBNumber = AWBNumber;
                                                uld[uldnum++] = ulddata;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex.Message);
                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                 }
                            }
                            #endregion

                            #region Line 8 Special Service request
                            if (str[i].StartsWith("SSR", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    lastrec = msg[0];
                                    //line = 0;
                                    if (msg[1].Length > 0)
                                    {
                                        fbldata.specialservicereq1 = msg[1];
                                    }

                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex.Message); 
                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                }
                            }
                            #endregion

                            #region Line 9 Other service info
                            if (str[i].StartsWith("OSI", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    lastrec = msg[0];
                                    //line = 0;
                                    if (msg[1].Length > 0)
                                    {
                                        Array.Resize(ref othinfoarray, othinfoarray.Length + 1);
                                        othinfoarray[othinfoarray.Length - 1].otherserviceinfo1 = msg[1];
                                        othinfoarray[othinfoarray.Length - 1].consigref = awbref;

                                    }

                                }
                                catch (Exception ex) {
                                    // clsLog.WriteLogAzure(ex.Message);
                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                 }
                            }
                            #endregion

                            #region Last line
                            if (i > str.Length - 2)
                            {
                                if (str[i].Trim().Length == 4 || str[i].Trim().Equals("LAST", StringComparison.OrdinalIgnoreCase) || str[i].Trim().Equals("CONT", StringComparison.OrdinalIgnoreCase))
                                {
                                    fbldata.endmesgcode = str[i].Trim();
                                }

                            }
                            #endregion

                            #region Other Info
                            if (str[i].StartsWith("/"))
                            {
                                string[] msg = str[i].Split('/');
                                try
                                {
                                    #region line 6 consigment origin info
                                    if (msg.Length > 0 && msg[0].Length == 0 && lastrec == "NA")
                                    {
                                        MessageData.consignmentorigininfo consorg = new MessageData.consignmentorigininfo();
                                        try
                                        {
                                            consorg.abbrivatedname = msg[1];
                                            consorg.carriercode = msg[2].Length > 0 ? msg[2].Substring(0, 2) : "";
                                            consorg.flightnum = msg[2].Length > 0 ? msg[2].Substring(2) : "";
                                            consorg.day = msg[3].Length > 0 ? msg[3].Substring(0, 2) : "";
                                            consorg.month = msg[3].Length > 0 ? msg[3].Substring(2) : "";
                                            consorg.airportcode = msg[4];
                                            consorg.movementprioritycode = msg[5];
                                        }
                                        catch (Exception ex) {
                                            // clsLog.WriteLogAzure(ex.Message);
                                            _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                         }
                                        Array.Resize(ref consorginfo, consorginfo.Length + 1);
                                        consorginfo[consorginfo.Length - 1] = consorg;
                                    }
                                    #endregion

                                    #region SSR 2
                                    if (lastrec == "SSR")
                                    {
                                        fbldata.specialservicereq2 = msg[1].Length > 0 ? msg[1] : "";
                                        lastrec = "NA";
                                    }
                                    #endregion

                                    #region OSI 2
                                    if (lastrec == "OSI")
                                    {
                                        othinfoarray[othinfoarray.Length - 1].otherserviceinfo2 = msg[1].Length > 0 ? msg[1] : "";
                                        lastrec = "NA";
                                    }
                                    #endregion

                                    #region SSR 2
                                    if (lastrec == "DIM")
                                    {
                                        MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
                                        try
                                        {
                                            lastrec = "DIM";
                                            int place = 0;

                                            dimension.weightcode = msg[place + 1].Substring(0, 1);
                                            dimension.weight = msg[place + 1].Substring(1);
                                            if (msg.Length > 0)
                                            {
                                                string[] dimstr = msg[place + 2].Split('-');
                                                dimension.mesurunitcode = dimstr[0].Substring(0, 3);
                                                dimension.length = dimstr[0].Substring(3);
                                                dimension.width = dimstr[1];
                                                dimension.height = dimstr[2];
                                            }
                                            dimension.piecenum = msg[place + 3];
                                            dimension.AWBPrefix = AWBPrefix;
                                            dimension.AWBNumber = AWBNumber;


                                        }
                                        catch (Exception ex) {
                                            // clsLog.WriteLogAzure(ex.Message);
                                            _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                         }
                                        Array.Resize(ref dimensioinfo, dimensioinfo.Length + 1);
                                        dimensioinfo[dimensioinfo.Length - 1] = dimension;
                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                                    _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
                flag = false;
            }

            return flag;
        }
        #endregion

        #region Decode Consigment Details
        public static void decodeconsigmentdetails(string inputstr, ref MessageData.consignmnetinfo[] consinfo, ref string awbprefix, ref string awbnumber)
        {
            const string FUN_NAME = "decodeconsigmentdetails";
            try
            {
                MessageData.consignmnetinfo consig = new MessageData.consignmnetinfo("");
                string[] msg = inputstr.Split('/');
                string[] decmes = msg[0].Split('-');
                //consinfo[num] = new MessageData.consignmnetinfo("");
                consig.airlineprefix = decmes[0];
                string[] sptarr = stringsplitter(decmes[1]);
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
                        // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                        _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
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
                        // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                        _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
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
                        consig.consigntype = strarr[0];
                        consig.pcscnt = strarr[1];//int.Parse(strarr[1]);
                        consig.weightcode = strarr[2];
                        consig.weight = strarr[3];//float.Parse(strarr[3]);
                        if (consig.consigntype.Equals("T"))
                        {//total pieces
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
                                    //if (consig.consigntype.Equals("S")) 
                                    //{
                                    //    consig.pcscnt = consig.numshp;
                                    //}
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
                        // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                        _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
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
                        // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                        _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
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
                        // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                        _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
                    }
                }
                try
                {
                    if (unloadingportsequence.Length > 0)
                        consig.portsequence = unloadingportsequence;
                    if (uldsequencenum.Length > 0)
                        consig.uldsequence = uldsequencenum;
                }
                catch (Exception ex) {
                    // clsLog.WriteLogAzure(ex.Message);
                    _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
                 }
                awbprefix = consig.airlineprefix;
                awbnumber = consig.awbnum;
                Array.Resize(ref consinfo, consinfo.Length + 1);
                consinfo[consinfo.Length - 1] = consig;
                awbref = consinfo.Length.ToString();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
            }
        }
        #endregion

        #region Decode Consigment Details
        public static void decodeconsigmentdetails(string inputstr, ref MessageData.consignmnetinfo[] consinfo)
        {
            const string FUN_NAME = "decodeconsigmentdetails";
            try
            {
                MessageData.consignmnetinfo consig = new MessageData.consignmnetinfo("");
                string[] msg = inputstr.Split('/');
                string[] decmes = msg[0].Split('-');
                //consinfo[num] = new MessageData.consignmnetinfo("");
                consig.airlineprefix = decmes[0];
                string[] sptarr = stringsplitter(decmes[1]);
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
                        // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                        _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
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
                        // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                        _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
                    }
                }
                //1
                if (msg[1].Length > 0)
                {
                    try
                    {
                        int k = 0;
                        char lastchr = 'A';
                        //Code Added to Remove comma character from Weight
                        msg[1] = msg[1].Replace(",", "");
                        char[] arr = msg[1].ToCharArray();
                        string[] strarr = new string[arr.Length];
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
                        consig.consigntype = strarr[0];
                        consig.pcscnt = strarr[1];//int.Parse(strarr[1]);
                        consig.weightcode = strarr[2];
                        consig.weight = strarr[3];//float.Parse(strarr[3]);
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
                    catch (Exception ex) {
                        // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME); 
                        _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
                    }
                }
                if (msg.Length > 2)
                {
                    try
                    {
                        //2 Manifest Description
                        consig.manifestdesc = msg[2];
                    }
                    catch (Exception ex) {
                        // clsLog.WriteLogAzure(ex.Message);
                        _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
                     }

                }
                if (msg.Length > 3)
                {//3 SHC- special handling code
                    try
                    {
                        consig.splhandling = "";
                        for (int j = 3; j < msg.Length; j++)
                            consig.splhandling = consig.splhandling + msg[j] + ",";
                    }
                    catch (Exception ex) {
                        // clsLog.WriteLogAzure(ex.Message); 
                        _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
                    }
                }
                try
                {
                    if (unloadingportsequence.Length > 0)
                        consig.portsequence = unloadingportsequence;
                    if (uldsequencenum.Length > 0)
                        consig.uldsequence = uldsequencenum;
                }
                catch (Exception ex) {
                    // clsLog.WriteLogAzure(ex.Message);
                    _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
                 }
                Array.Resize(ref consinfo, consinfo.Length + 1);
                consinfo[consinfo.Length - 1] = consig;
                awbref = consinfo.Length.ToString();
            }
            catch (Exception ex) {
                // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                _staticLogger.LogError(ex, PAGE_NAME, FUN_NAME);
             }
        }
        #endregion

        #region encodeFBL
        public static string EncodeFBLforsend(MessageData.fblinfo fbldata, MessageData.unloadingport[] unloadingport, MessageData.consignmnetinfo[] consinfo, MessageData.dimensionnfo[] dimensioinfo, MessageData.consignmentorigininfo[] consorginfo, MessageData.ULDinfo[] uld, MessageData.otherserviceinfo othinfo)
        {
            string fbl = null;
            try
            {
                #region Line 1
                string line1 = "FBL" + "/" + fbldata.fblversion;
                #endregion

                #region Line 2
                string line2 = "";
                line2 = fbldata.messagesequencenum + "/" + fbldata.carriercode + fbldata.fltnum + "/" + fbldata.date + fbldata.month + "/" + fbldata.fltairportcode + (fbldata.aircraftregistration.Length > 1 ? ("/" + fbldata.aircraftregistration) : "");
                #endregion

                #region Line 3
                string line3 = "";
                if (unloadingport.Length > 0)
                {
                    for (int i = 0; i < unloadingport.Length; i++)
                    {
                        line3 = line3 + unloadingport[i].unloadingairport + (unloadingport[i].nilcargocode.Length > 0 ? ("/" + unloadingport[i].nilcargocode) : "") + "$";
                    }
                    line3 = line3.Trim('$');
                    line3 = line3.Replace("$", "\r\n");
                }
                #endregion

                #region line 4
                //string line4 = "";
                //if (consinfo.Length > 0)
                //{
                //    for (int i = 0; i < consinfo.Length; i++)
                //    {
                //        string splhandling = "";
                //        if (consinfo[i].splhandling.Length > 0 && consinfo[i].splhandling != null)
                //        {
                //            splhandling = consinfo[i].splhandling.Replace(",", "");
                //            splhandling = "/" + splhandling;
                //        }
                //        line4 = line4 + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum + consinfo[i].origin + consinfo[i].dest + "/" + consinfo[i].consigntype + consinfo[i].pcscnt + consinfo[i].weightcode + consinfo[i].weight + consinfo[i].volumecode + consinfo[i].volumeamt + consinfo[i].densityindicator + consinfo[i].densitygrp + consinfo[i].TotalConsignmentType + consinfo[i].AWBPieces + consinfo[i].shpdesccode + consinfo[i].numshp + "/" + consinfo[i].manifestdesc + splhandling;
                //        line4 = line4.Trim('/') + "$";
                //        consinfo[i].d

                //    }
                //    line4 = line4.Trim('$');
                //    line4 = line4.Replace("$", "\r\n");
                //}
                #endregion


                #region line 4
                string line4 = "";
                int repeatFBL = -1;
                //Boolean bLastFBL = false;
                if (consinfo.Length > 0)
                {
                    for (int i = 0; i < consinfo.Length; i++)
                    {
                        string splhandling = "";
                        if (consinfo[i].splhandling.Length > 0 && consinfo[i].splhandling != null)
                        {
                            splhandling = consinfo[i].splhandling.Replace(",", "");
                            splhandling = "/" + splhandling;
                        }
                        line4 = line4 + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum + consinfo[i].origin + consinfo[i].dest + "/" + consinfo[i].consigntype + consinfo[i].pcscnt + consinfo[i].weightcode + consinfo[i].weight + consinfo[i].volumecode + consinfo[i].volumeamt + consinfo[i].densityindicator + consinfo[i].densitygrp + consinfo[i].TotalConsignmentType + consinfo[i].AWBPieces + consinfo[i].shpdesccode + consinfo[i].numshp + "/" + consinfo[i].manifestdesc + splhandling;
                        line4 = line4.Trim('/') + "$";
                        line4 = line4 + consinfo[i].DimsString;

                        if (consorginfo.Length > 1 && consorginfo.Length >= i)
                        {

                            if (consorginfo[i].abbrivatedname.Length > 0 || consorginfo[i].carriercode.Length > 0 || consorginfo[i].flightnum.Length > 0 || consorginfo[i].day.Length > 0 || consorginfo[i].month.Length > 0 || consorginfo[i].airportcode.Length > 0 || consorginfo[i].movementprioritycode.Length > 0)
                            {
                                line4 = line4 + "/" + consorginfo[i].abbrivatedname + "/" + consorginfo[i].carriercode + consorginfo[i].flightnum + "/" + consorginfo[i].day + consorginfo[i].month + "/" + consorginfo[i].airportcode + "/" + consorginfo[i].movementprioritycode;
                                line4 = line4.Trim('/') + "$";
                            }

                        }

                        ////add remark info
                        //if (othinfo[i].otherserviceinfo1.Length > 0 || othinfo[i].otherserviceinfo2.Length > 0)
                        //{
                        //    line4 = line4 + "OSI/" + othinfo[i].otherserviceinfo1 + "\r\n" + ((othinfo[i].otherserviceinfo2.Length > 0) ? "/" + othinfo[i].otherserviceinfo2 : "");
                        //}

                        //line4 = line4.Trim('$');
                        line4 = line4.Replace("$", "\r\n");

                        //if (consinfo.Length - 1 <= repeatFBL + 1)
                        //{ bLastFBL = true; }
                        //else
                        if ((i == repeatFBL + 22) && (consinfo.Length - 1 > repeatFBL + 22))
                        {
                            line4 = line4 + "CONT";
                            line4 = line4.Trim('$');
                            line4 = line4.Replace("$", "\r\n");

                            fbl = fbl + line1.Trim('/') + "\r\n" + line2.Trim() + "\r\n" + line3.Trim() + "\r\n" + line4.Trim();
                            fbl = fbl.Trim('\n').Trim('\r') + "\r\n" + "#";
                            fbldata.messagesequencenum = string.IsNullOrEmpty(fbldata.messagesequencenum) ? "0" : Convert.ToString(Convert.ToInt16(fbldata.messagesequencenum) + 1);

                            #region Line 1
                            //line1 = "FBL" + "/" + fbldata.fblversion;
                            #endregion

                            #region Line 2
                            line2 = "";
                            line2 = fbldata.messagesequencenum + "/" + fbldata.carriercode + fbldata.fltnum + "/" + fbldata.date + fbldata.month + "/" + fbldata.fltairportcode + (fbldata.aircraftregistration.Length > 1 ? ("/" + fbldata.aircraftregistration) : "");
                            #endregion

                            #region Line 3
                            //line3 = "";
                            //if (unloadingport.Length > 0)
                            //{
                            //    for (int j = 0; j < unloadingport.Length; j++)
                            //    {
                            //        line3 = line3 + unloadingport[j].unloadingairport + (unloadingport[j].nilcargocode.Length > 0 ? ("/" + unloadingport[j].nilcargocode) : "") + "$";
                            //    }
                            //    line3 = line3.Trim('$');
                            //    line3 = line3.Replace("$", "\r\n");
                            //}
                            #endregion

                            line4 = "";
                            repeatFBL = i;
                        }


                    }
                    line4 = line4.Trim('$');
                    line4 = line4.Replace("$", "\r\n");
                }
                #endregion

                #region Line 5
                string line5 = "";
                if (dimensioinfo.Length > 0)
                {
                    line5 = "DIM/";
                    for (int i = 0; i < dimensioinfo.Length; i++)
                    {
                        if (dimensioinfo[i].height.Length > 0 || dimensioinfo[i].length.Length > 0 || dimensioinfo[i].piecenum.Length > 0 || dimensioinfo[i].weight.Length > 0 || dimensioinfo[i].width.Length > 0)
                        {
                            line5 = line5 + dimensioinfo[i].weightcode + dimensioinfo[i].weight + "/" + dimensioinfo[i].mesurunitcode + dimensioinfo[i].length + "-" + dimensioinfo[i].width + "-" + dimensioinfo[i].height + "/" + dimensioinfo[i].piecenum;
                            line5 = line5.Trim('/') + "$";
                        }
                    }
                    line5 = line5.Trim('$');
                    line5 = line5.Replace("$", "\r\n");
                }
                #endregion

                #region Line6 consignment origin info
                string line6 = "";
                //if (consorginfo.Length > 1)
                //{
                //    for (int i = 0; i < consorginfo.Length; i++)
                //    {

                //        if (consorginfo[i].abbrivatedname.Length > 0 || consorginfo[i].carriercode.Length > 0 || consorginfo[i].flightnum.Length > 0 || consorginfo[i].day.Length > 0 || consorginfo[i].month.Length > 0 || consorginfo[i].airportcode.Length > 0 || consorginfo[i].movementprioritycode.Length > 0)
                //        {
                //            line6 = line6 + consorginfo[i].abbrivatedname + "/" + consorginfo[i].carriercode + consorginfo[i].flightnum + "/" + consorginfo[i].day + consorginfo[i].month + "/" + consorginfo[i].airportcode + "/" + consorginfo[i].movementprioritycode;
                //            line6 = line6.Trim('/') + "$";
                //        }

                //    }
                //    line6 = line6.Trim('$');
                //    line6 = "/" + line6.Replace("$", "\r\n/");
                //}

                #endregion

                #region Line7 ULD
                string line7 = "";
                if (fbldata.noofuld.Length > 0)
                {
                    line7 = "ULD/" + fbldata.noofuld + "/";
                    string uldinfo = null;
                    for (int i = 0; i < int.Parse(fbldata.noofuld); i++)
                    {
                        uldinfo = null;
                        uldinfo = uld[i].uldtype + uld[i].uldsrno + uld[i].uldowner + "-" + uld[i].uldloadingindicator + "/" + uld[i].uldweightcode + uld[i].uldweight;
                        if (uldinfo.Length > 2)
                            line7 = line7 + uldinfo + "/";
                    }
                }
                #endregion

                #region Line 8 SSR
                string line8 = "";
                if (fbldata.specialservicereq1.Length > 0)
                {
                    line8 = "SSR/" + fbldata.specialservicereq1;
                    if (fbldata.specialservicereq2.Length > 0)
                    {
                        line8 = line8 + "\r\n" + "/" + fbldata.specialservicereq2;
                    }
                }
                #endregion

                #region Line 9 other service info
                string line9 = "";
                //if (othinfo.otherserviceinfo1.Length > 0 || othinfo.otherserviceinfo2.Length > 0)
                //{
                //    line9 = "OSI/" + othinfo.otherserviceinfo1 + "\r\n" + "/" + othinfo.otherserviceinfo2;
                //}
                #endregion

                #region BuildFFR
                //if (!bLastFBL)
                //{
                //    fbl = line1.Trim('/') + "\r\n" + line2.Trim() + "\r\n" + line3.Trim() + "\r\n" + line4.Trim();
                //}
                //else
                //{ 
                fbl = fbl + line1.Trim('/') + "\r\n" + line2.Trim() + "\r\n" + line3.Trim() + "\r\n" + line4.Trim();
                //}

                if (line5.Length > 1)
                {
                    fbl = fbl + "\r\n" + line5.Trim();
                }
                if (line6.Length > 1)
                {
                    fbl = fbl + "\r\n" + line6.Trim();
                }
                if (line7.Length > 1)
                {
                    fbl = fbl + "\r\n" + line7.Trim();
                }
                if (line8.Length > 1)
                {
                    fbl = fbl + "\r\n" + line8.Trim();
                }
                if (line9.Length > 1)
                {
                    fbl = fbl + "\r\n" + line9.Trim();
                }
                fbl = fbl.Trim('\n').Trim('\r') + "\r\n" + fbldata.endmesgcode;
                #endregion

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                fbl = "ERR";
            }
            return fbl;
        }
        #endregion

        //FFM

        #region Decode FFM message
        public static bool decodereceiveFFM(string ffmmsg, ref MessageData.ffminfo ffmdata, ref MessageData.unloadingport[] unloadingport, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.dimensionnfo[] dimensioinfo, ref MessageData.ULDinfo[] uld, ref MessageData.otherserviceinfo[] othinfoarray, ref MessageData.customsextrainfo[] custominfo, ref MessageData.movementinfo[] movementinfo)//(string ffmmsg)
        {

            bool flag = false;
            const string FUN_NAME = "decodereceiveFFM";
            try
            {
                string lastrec = "NA";
                //int line = 0;
                uldsequencenum = "";
                unloadingportsequence = "";
                string AWBPrefix = "", AWBNumber = "";
                try
                {
                    if (ffmmsg.StartsWith("FFM", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] str = ffmmsg.Split('$');//Regex.Split(ffmmsg, "\r\n");//ffrmsg.Split('$');
                        if (str.Length > 3)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {
                                if (str[i].StartsWith("CONT", StringComparison.OrdinalIgnoreCase) || str[i].StartsWith("LAST", StringComparison.OrdinalIgnoreCase))
                                {
                                    i = str.Length + 1;
                                    return flag;
                                }
                                flag = true;
                                #region Line 1
                                if (str[i].StartsWith("FFM", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        ffmdata.ffmversionnum = msg[1];
                                    }
                                    catch (Exception ex) {
                                        // clsLog.WriteLogAzure(ex.Message); 
                                        _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region line 2 flight data
                                if (i == 1)
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 1)
                                        {
                                            ffmdata.messagesequencenum = msg[0];
                                            ffmdata.carriercode = msg[1].Substring(0, 2);
                                            ffmdata.fltnum = msg[1].Substring(2);
                                            ffmdata.fltdate = msg[2].Substring(0, 2);
                                            ffmdata.month = msg[2].Substring(2, 3);
                                            ffmdata.time = msg[2].Substring(5).Length > 0 ? msg[2].Substring(5) : "";
                                            ffmdata.fltairportcode = msg[3];
                                            ffmdata.aircraftregistration = msg[4];
                                            if (msg.Length > 4)
                                            {
                                                ffmdata.countrycode = msg[5];
                                                ffmdata.fltdate1 = msg[6].Substring(0, 2);
                                                ffmdata.fltmonth1 = msg[6].Substring(2, 3);
                                                ffmdata.flttime1 = msg[6].Substring(5); ;
                                                ffmdata.fltairportcode1 = msg[7];
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                     }
                                }
                                #endregion

                                #region line 3 point of unloading
                                if (i >= 2)
                                {
                                    MessageData.unloadingport unloading = new MessageData.unloadingport("");
                                    if (str[i].Contains('/') && (!str[i].StartsWith("/")) && (!str[i].Contains("-")))
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length >= 2)
                                        {
                                            if (msg[0].Length > 0 && !msg[0].StartsWith("/") && !msg[0].Contains('-')
                                                && !msg[0].Equals("SSR", StringComparison.OrdinalIgnoreCase)
                                                && !msg[0].Equals("SCI", StringComparison.OrdinalIgnoreCase)
                                                && !msg[0].Equals("OSI", StringComparison.OrdinalIgnoreCase)
                                                && !msg[0].Equals("ULD", StringComparison.OrdinalIgnoreCase)
                                                && !msg[0].Equals("COR", StringComparison.OrdinalIgnoreCase)
                                                && !msg[0].Equals("OCI", StringComparison.OrdinalIgnoreCase)
                                                && !msg[0].Equals("DIM", StringComparison.OrdinalIgnoreCase))
                                            {
                                                unloading.unloadingairport = msg[0];
                                                unloading.nilcargocode = msg[1];
                                                try
                                                {
                                                    if (msg.Length > 2)
                                                    {
                                                        unloading.day = msg[2].Substring(0, 2);
                                                        unloading.month = msg[2].Substring(2, 3);
                                                        unloading.time = msg[2].Substring(5);
                                                        unloading.day1 = msg[3].Substring(0, 2);
                                                        unloading.month1 = msg[3].Substring(2, 3);
                                                        unloading.time1 = msg[3].Substring(5);
                                                    }
                                                }
                                                catch (Exception ex) {
                                                    // clsLog.WriteLogAzure(ex.Message);
                                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                 }
                                                Array.Resize(ref unloadingport, unloadingport.Length + 1);
                                                unloadingport[unloadingport.Length - 1] = unloading;
                                                //for sequence app
                                                unloadingport[unloadingport.Length - 1].sequencenum = unloadingport.Length.ToString();
                                                unloadingportsequence = unloadingport.Length.ToString();
                                                uldsequencenum = "";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (str[i].Trim().Length == 3 && (!str[i].Contains("-")) && (!str[i].Contains("/")))
                                        {
                                            unloading.unloadingairport = str[i];
                                            Array.Resize(ref unloadingport, unloadingport.Length + 1);
                                            unloadingport[unloadingport.Length - 1] = unloading;
                                            //for sequence app
                                            unloadingport[unloadingport.Length - 1].sequencenum = unloadingport.Length.ToString();
                                            unloadingportsequence = unloadingport.Length.ToString();
                                            uldsequencenum = "";
                                        }
                                    }
                                }
                                #endregion

                                #region  line 4 onwards check consignment details
                                if (i > 1)
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        //0th element
                                        if (msg[0].Contains('-'))
                                        {
                                            try
                                            {//Version below 5 - 1 row of AWB information (AWB+SHC)
                                             //V5 - 2 Rows - 1.AWB info 2. SHC
                                                AWBPrefix = "";
                                                AWBNumber = "";
                                                if (Convert.ToInt16(ffmdata.ffmversionnum) > 4)
                                                {

                                                    lastrec = "AWB";
                                                }
                                                else
                                                {
                                                    lastrec = "NA";
                                                }
                                            }
                                            catch (Exception ex) {
                                                // clsLog.WriteLogAzure(ex.Message);
                                                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                             }
                                            // line = 0;
                                            //decodeconsigmentdetails(str[i],ref consinfo);
                                            decodeconsigmentdetails(str[i], ref consinfo, ref AWBPrefix, ref AWBNumber);

                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        continue;
                                    }
                                }
                                #endregion

                                #region Line 5 Dimendion info
                                if (str[i].StartsWith("DIM", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        int total = msg.Length / 3;
                                        Array.Resize(ref dimensioinfo, dimensioinfo.Length + total + 1);
                                        for (int cnt = 0; cnt < total; cnt++)
                                        {
                                            int place = 3 * cnt;
                                            MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
                                            dimension.weightcode = msg[place + 1].Substring(0, 1);
                                            dimension.weight = msg[place + 1].Substring(1);
                                            if (msg.Length > 0)
                                            {
                                                string[] dimstr = msg[place + 2].Split('-');
                                                dimension.mesurunitcode = dimstr[0].Substring(0, 3);
                                                dimension.length = dimstr[0].Substring(3);
                                                dimension.weight = dimstr[1];
                                                dimension.height = dimstr[2];
                                            }
                                            dimension.piecenum = msg[place + 3];
                                            dimension.consigref = awbref;
                                            dimension.AWBPrefix = AWBPrefix;
                                            dimension.AWBNumber = AWBNumber;
                                            dimensioinfo[cnt] = dimension;

                                        }

                                    }
                                    catch (Exception ex) {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                     }
                                }
                                #endregion

                                #region Line 10 ULD Specification
                                if (str[i].StartsWith("ULD", StringComparison.OrdinalIgnoreCase))
                                {
                                    MessageData.ULDinfo ulddata = new MessageData.ULDinfo("");
                                    try
                                    {
                                        string[] msg = str[i].Trim().Split('/');
                                        if (msg.Length > 1)
                                        {

                                            string[] splitstr = msg[1].Split('-');
                                            ulddata.uldno = splitstr[0];
                                            ulddata.uldtype = splitstr[0].Substring(0, 3);
                                            ulddata.uldsrno = splitstr[0].Substring(3, splitstr[0].Length - 5);
                                            ulddata.uldowner = splitstr[0].Substring(splitstr[0].Length - 2, 2);
                                            if (splitstr.Length > 0)
                                            {
                                                ulddata.uldloadingindicator = splitstr[1];
                                            }
                                            if (msg.Length > 1)
                                            {
                                                ulddata.uldremark = msg[2];
                                            }

                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                     }
                                    Array.Resize(ref uld, uld.Length + 1);
                                    uld[uld.Length - 1] = ulddata;
                                    if (int.Parse(unloadingportsequence) > 0)
                                    {
                                        uld[uld.Length - 1].portsequence = unloadingportsequence;
                                        uld[uld.Length - 1].refuld = uld.Length.ToString();
                                        uldsequencenum = uld.Length.ToString();
                                    }
                                }
                                #endregion

                                #region Line 7 Other service info
                                if (str[i].StartsWith("OSI", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = msg[0];
                                        //line = 0;
                                        if (msg[1].Length > 0)
                                        {
                                            Array.Resize(ref othinfoarray, othinfoarray.Length + 1);
                                            othinfoarray[othinfoarray.Length - 1].otherserviceinfo1 = msg[1];
                                            othinfoarray[othinfoarray.Length - 1].consigref = awbref;

                                        }

                                    }
                                    catch (Exception ex) {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                     }
                                }
                                #endregion

                                #region Line 8 COR
                                if (str[i].StartsWith("COR", StringComparison.OrdinalIgnoreCase))
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg[1].Length > 0)
                                    {
                                        consinfo[int.Parse(awbref) - 1].customorigincode = msg[1];
                                    }
                                }
                                #endregion

                                #region Line9 custom extra info
                                if (str[i].StartsWith("OCI", StringComparison.OrdinalIgnoreCase))
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 0)
                                    {
                                        lastrec = "OCI";
                                        MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
                                        custom.IsoCountryCodeOci = msg[1];
                                        custom.InformationIdentifierOci = msg[2];
                                        custom.CsrIdentifierOci = msg[3];
                                        custom.SupplementaryCsrIdentifierOci = msg[4];
                                        custom.consigref = awbref;
                                        Array.Resize(ref custominfo, custominfo.Length + 1);
                                        custominfo[custominfo.Length - 1] = custom;
                                    }
                                }
                                #endregion

                                #region Line11 special custom information
                                if (str[i].StartsWith("SCI", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 0)
                                        {
                                            consinfo[int.Parse(awbref) - 1].customorigincode = msg[2];
                                            consinfo[int.Parse(awbref) - 1].customref = msg[1];

                                        }
                                    }
                                    catch (Exception ex) {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                     }
                                }
                                #endregion

                                #region Last line
                                if (i > str.Length - 3)
                                {
                                    if (str[i].Trim().Length == 4 || str[i].Trim().Equals("LAST", StringComparison.OrdinalIgnoreCase) || str[i].Trim().Equals("CONT", StringComparison.OrdinalIgnoreCase))
                                    {
                                        ffmdata.endmesgcode = str[i].Trim();
                                    }

                                }
                                #endregion

                                #region Other Info
                                if (str[i].StartsWith("/"))
                                {
                                    string[] msg = str[i].Split('/');
                                    try
                                    {
                                        #region line 6 movementinfo info
                                        try
                                        {
                                            if (msg.Length > 0 && msg[0].Length == 0 && lastrec == "NA")
                                            {
                                                lastrec = "MOV";
                                                MessageData.movementinfo movement = new MessageData.movementinfo("");
                                                try
                                                {
                                                    movement.AirportCode = msg[1].Substring(0, 3);
                                                    movement.CarrierCode = "";
                                                    movement.FlightNumber = msg[1].Substring(3);//Carrier+FLT
                                                    movement.FlightDay = msg[2].Substring(0, 2);
                                                    movement.FlightMonth = msg[2].Substring(2);
                                                    movement.consigref = awbref;
                                                }
                                                catch (Exception ex) {
                                                    // clsLog.WriteLogAzure(ex.Message);
                                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                 }
                                                Array.Resize(ref movementinfo, movementinfo.Length + 1);
                                                movementinfo[movementinfo.Length - 1] = movement;
                                            }

                                            if (lastrec == "MOV")
                                            {
                                                if (msg[1].Length > 0)
                                                {
                                                    movementinfo[movementinfo.Length - 1].PriorityorVolumecode = msg[1];

                                                }
                                                lastrec = "NA";
                                            }
                                        }
                                        catch (Exception ex) {
                                            // clsLog.WriteLogAzure(ex.Message);
                                            _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                         }
                                        #endregion

                                        #region SSR 2
                                        if (lastrec == "SSR")
                                        {
                                            ffmdata.specialservicereq2 = msg[1].Length > 0 ? msg[1] : "";
                                            lastrec = "NA";
                                        }
                                        #endregion

                                        #region OSI 2
                                        if (lastrec == "OSI")
                                        {
                                            othinfoarray[othinfoarray.Length - 1].otherserviceinfo2 = msg[1].Length > 0 ? msg[1] : "";
                                            lastrec = "NA";
                                        }
                                        #endregion

                                        #region Splhandling
                                        if (lastrec == "AWB")
                                        {
                                            try
                                            {
                                                if (str[i].Length > 1)
                                                {
                                                    consinfo[consinfo.Length - 1].splhandling = str[i].Replace('/', ',');
                                                }
                                            }
                                            catch (Exception ex) {
                                                // clsLog.WriteLogAzure(ex.Message); 
                                                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                            lastrec = "NA";
                                        }
                                        #endregion

                                        #region OCI
                                        if (lastrec == "OCI")
                                        {
                                            string[] msgdata = str[i].Split('/');
                                            if (msgdata.Length > 0)
                                            {
                                                lastrec = "OCI";
                                                MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
                                                custom.IsoCountryCodeOci = msgdata[1];
                                                custom.InformationIdentifierOci = msgdata[2];
                                                custom.CsrIdentifierOci = msgdata[3];
                                                custom.SupplementaryCsrIdentifierOci = msgdata[4];
                                                Array.Resize(ref custominfo, custominfo.Length + 1);
                                                custominfo[custominfo.Length - 1] = custom;
                                            }
                                        }
                                        #endregion
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message); 
                                        _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                    flag = true;
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;
        }
        #endregion

        #region Encode FFM
        public static string EncodeFFMforsend(ref MessageData.ffminfo ffmdata, ref MessageData.unloadingport[] unloadingport, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.dimensionnfo[] dimensioinfo, ref MessageData.movementinfo[] movementinfo, ref MessageData.otherserviceinfo[] othinfoarray, ref MessageData.customsextrainfo[] custominfo, ref MessageData.ULDinfo[] uld)
        {
            string ffm = null, flightcons = "", uldcons = "";
            const String FUN_NAME = "EncodeFFMforsend";
            try
            {
                #region Line 1
                string line1 = "FFM" + "/" + ffmdata.ffmversionnum;
                #endregion

                #region Line 2
                string line2 = "";
                line2 = ffmdata.messagesequencenum + "/" + ffmdata.carriercode + ffmdata.fltnum + "/" + ffmdata.fltdate + ffmdata.month + ffmdata.time + "/" + ffmdata.fltairportcode + (ffmdata.aircraftregistration.Length > 1 ? ("/" + ffmdata.aircraftregistration) : "");
                if (ffmdata.countrycode.Length > 0 || ffmdata.fltdate1.Length > 0)
                {
                    line2 = line2 + "/" + ffmdata.countrycode + "/" + ffmdata.fltdate1 + ffmdata.fltmonth1 + ffmdata.flttime1 + "/" + ffmdata.fltairportcode1;
                }
                #endregion

                #region Line 3 point of unloading
                string line3 = "";
                if (unloadingport.Length > 0)
                {
                    for (int i = 0; i < unloadingport.Length; i++)
                    {
                        line3 = line3 + "$" + unloadingport[i].unloadingairport + (unloadingport[i].nilcargocode.Length > 0 ? ("/" + unloadingport[i].nilcargocode) : "");
                        if (unloadingport[i].day.Length > 0)
                        {
                            line3 = line3 + "/" + unloadingport[i].day + unloadingport[i].month + unloadingport[i].time;
                        }
                        if (unloadingport[i].day1.Length > 0)
                        {
                            line3 = line3 + "/" + unloadingport[i].day1 + unloadingport[i].month1 + unloadingport[i].time1;
                        }
                        line3 = line3 + "$";
                        flightcons = FFMPartBuilder((i + 1).ToString(), "", ref consinfo, ref dimensioinfo, ref movementinfo, ref othinfoarray, ref custominfo);
                        if (flightcons.Length > 3)
                        {
                            line3 = line3 + flightcons;
                        }
                        if (uld.Length > 0)
                        {
                            string uldstr = "";
                            for (int k = 0; k < uld.Length; k++)
                            {
                                if (uld[k].portsequence.Equals((i + 1).ToString()))
                                {//uld is there
                                    uldstr = uldstr.Trim() + "$ULD/" + uld[k].uldtype + uld[k].uldsrno + uld[k].uldowner + (uld[k].uldloadingindicator.Length > 0 ? ("-" + uld[k].uldloadingindicator) : "") + (uld[k].uldremark.Length > 0 ? ("/" + uld[k].uldremark) : "") + "$";
                                    uldcons = FFMPartBuilder((i + 1).ToString(), (k + 1).ToString(), ref consinfo, ref dimensioinfo, ref movementinfo, ref othinfoarray, ref custominfo);
                                    if (uldcons.Length > 3)
                                    {
                                        uldstr = uldstr.Trim('$') + "\r\n" + uldcons;
                                    }
                                }
                            }
                            uldstr = uldstr.Trim('$');
                            uldstr = uldstr.Replace("$", "\r\n");
                            if (uldstr.Length > 0)
                            {
                                line3 = line3 + "\r\n" + uldstr;
                            }
                        }
                    }
                    line3 = line3.Replace("$$", "$");
                    line3 = line3.Trim('$');
                    line3 = line3.Replace("$", "\r\n");
                }
                #endregion

                #region Line 10 ULD
                //string line10 = "";
                //if (uld.Length > 0)
                //{
                //    line10 = "";                    
                //    for (int i = 0; i < uld.Length; i++)
                //    {

                //        line10 = line10.Trim() + "ULD/" + uld[i].uldtype + uld[i].uldsrno + uld[i].uldowner + (uld[i].uldloadingindicator.Length > 0 ? ("-" + uld[i].uldloadingindicator) : "") + (uld[i].uldremark.Length > 0 ? ("/" + uld[i].uldremark) : "") + "$";                        
                //    }
                //    line10=line10.Trim('$');
                //    line10=line10.Replace("$","\r\n");
                //}
                #endregion

                #region BuildFFM
                ffm = line1.Trim('/') + "\r\n" + line2.Trim() + "\r\n" + line3.Trim() + "\r\n" + ffmdata.endmesgcode;
                #endregion

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                ffm = "ERR";
            }
            return ffm;
        }

        public static string FFMPartBuilder(string flightref, string uldref, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.dimensionnfo[] dimensioinfo, ref MessageData.movementinfo[] movementinfo, ref MessageData.otherserviceinfo[] othinfoarray, ref MessageData.customsextrainfo[] custominfo)
        {
            string output = "";
            try
            {
                #region line 4 Consigment INfo
                string line4 = "";
                if (consinfo.Length > 0)
                {
                    for (int i = 0; i < consinfo.Length; i++)
                    {

                        string splhandling = "";
                        if (consinfo[i].portsequence.Equals(flightref) && consinfo[i].uldsequence.Equals(uldref))
                        {
                            if (consinfo[i].splhandling.Length > 0 && consinfo[i].splhandling != null)
                            {
                                splhandling = consinfo[i].splhandling.Replace(",", "/");
                                splhandling = "\r\n" + splhandling;
                            }
                            line4 = line4 + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum + consinfo[i].origin + consinfo[i].dest + "/" + consinfo[i].consigntype + consinfo[i].pcscnt + consinfo[i].weightcode + consinfo[i].weight + consinfo[i].volumecode + consinfo[i].volumeamt + consinfo[i].densityindicator + consinfo[i].densitygrp + consinfo[i].shpdesccode + consinfo[i].numshp + "/" + consinfo[i].manifestdesc + splhandling;
                            line4 = line4.Trim('/') + "$";

                            #region Line 5 Dimension info
                            string line5 = "";
                            if (dimensioinfo.Length > 0)
                            {
                                line5 = "DIM/";
                                for (int j = 0; j < dimensioinfo.Length; j++)
                                {
                                    if (dimensioinfo[j].consigref.Equals((i + 1).ToString()))
                                    {
                                        if (dimensioinfo[j].height.Length > 0 || dimensioinfo[j].length.Length > 0 || dimensioinfo[j].piecenum.Length > 0 || dimensioinfo[j].weight.Length > 0 || dimensioinfo[j].width.Length > 0)
                                        {
                                            line5 = line5 + dimensioinfo[j].weightcode + dimensioinfo[j].weight + "/" + dimensioinfo[j].mesurunitcode + dimensioinfo[j].length + "-" + dimensioinfo[j].width + "-" + dimensioinfo[j].height + "/" + dimensioinfo[j].piecenum;
                                            line5 = line5.Trim('/') + "$";
                                        }
                                    }
                                }
                                line5 = line5.Trim('$');
                                line5 = line5.Replace("$", "\r\n");
                                if (line5.Length > 0)
                                {
                                    output = output + line5 + "\r\n";
                                }
                            }
                            #endregion

                            #region line6 movement info
                            string line6 = "";
                            if (movementinfo.Length > 0)
                            {
                                for (int k = 0; k < movementinfo.Length; k++)
                                {
                                    if (movementinfo[k].consigref.Equals((i + 1).ToString()))
                                    {
                                        line6 = line6 + "/" + movementinfo[k].AirportCode + movementinfo[k].CarrierCode + movementinfo[k].FlightNumber + "/" + movementinfo[k].FlightDay + movementinfo[k].FlightMonth + "$" + "/" + movementinfo[k].PriorityorVolumecode;
                                    }
                                }
                                line6 = line6.Replace("$", "\r\n");
                                if (line6.Length > 0)
                                {
                                    output = output + line6 + "\r\n";
                                }
                            }

                            #endregion

                            #region Line 7 other service info
                            string line7 = "";
                            if (othinfoarray.Length > 0)
                            {
                                for (int j = 0; j < othinfoarray.Length; j++)
                                {
                                    if (othinfoarray[j].consigref.Equals((i + 1).ToString()))
                                    {
                                        if (othinfoarray[j].otherserviceinfo1.Length > 0)
                                        {
                                            line7 = "OSI/" + othinfoarray[j].otherserviceinfo1 + "$";
                                            if (othinfoarray[j].otherserviceinfo2.Length > 0)
                                            {
                                                line7 = line7 + "/" + othinfoarray[j].otherserviceinfo2 + "$";
                                            }
                                        }
                                    }
                                }
                                line7 = line7.Trim('$');
                                line7 = line7.Replace("$", "\r\n");
                                if (line7.Length > 0)
                                {
                                    output = output + line7 + "\r\n";
                                }
                            }
                            #endregion

                            #region Line 8 Custom origin
                            if (consinfo[i].customorigincode.Length > 0)
                            {
                                output = output + "COR/" + consinfo[i].customorigincode + "\r\n";
                            }
                            #endregion

                            #region Line 9
                            string line9 = "";
                            if (custominfo.Length > 0)
                            {
                                for (int k = 0; k < custominfo.Length; k++)
                                {
                                    if (custominfo[k].consigref.Equals((i + 1).ToString()))
                                    {
                                        line9 = "/" + custominfo[k].IsoCountryCodeOci + "/" + custominfo[k].InformationIdentifierOci + "/" + custominfo[k].CsrIdentifierOci + "/" + custominfo[k].SupplementaryCsrIdentifierOci + "$";
                                    }

                                }
                                line9 = "OCI" + line9.Trim('$');
                                line9 = line9.Replace("$", "\r\n");
                                if (line9.Length > 0)
                                {
                                    output = output + line9 + "\r\n";
                                }
                            }
                            #endregion
                        }


                    }
                    line4 = line4.Trim('$');
                    line4 = line4.Replace("$", "\r\n");
                }
                #endregion
                if (output.Length > 0)
                {
                    output = line4 + "\r\n" + output;
                }
                else
                {
                    output = line4;
                }

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                output = "ERR";
            }
            return output;
        }

        #endregion

        //FSA

        #region Decoed FSA Message
        public static bool decodeReceiveFSA(string fsamsg, ref MessageData.FSAInfo fsadata, ref MessageData.CommonStruct[] fsanodes, ref MessageData.customsextrainfo[] custominfo, ref MessageData.ULDinfo[] uld, ref MessageData.otherserviceinfo[] othinfoarray)
        {
            bool flag = false;
            string lastrec = "";
            try
            {
                if (fsamsg.StartsWith("FSA", StringComparison.OrdinalIgnoreCase) || fsamsg.StartsWith("FSU", StringComparison.OrdinalIgnoreCase))
                {
                    string[] str = fsamsg.Split('$');//Regex.Split(fsamsg, "\r\n");//ffrmsg.Split('$');
                    if (str.Length > 2)
                    {
                        flag = true;

                        #region Line 1
                        if (str[0].StartsWith("FSA", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                string[] msg = str[0].Split('/');
                                fsadata.fsaversion = msg[1];
                            }
                            catch (Exception ex) {
                                // clsLog.WriteLogAzure(ex.Message);
                                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                             }
                        }
                        #endregion

                        #region Line 2 awb consigment details
                        try
                        {
                            string[] msg = str[1].Split('/');
                            //0th element
                            string[] decmes = msg[0].Split('-');
                            fsadata.airlineprefix = decmes[0];
                            fsadata.awbnum = decmes[1].Substring(0, decmes[1].Length - 6);
                            fsadata.origin = decmes[1].Substring(decmes[1].Length - 6, 3);
                            fsadata.dest = decmes[1].Substring(decmes[1].Length - 3, 3);
                            //1
                            if (msg[1].Length > 0)
                            {
                                int k = 0;
                                char lastchr = 'A';
                                char[] arr = msg[1].ToCharArray();
                                string[] strarr = new string[arr.Length];
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
                                fsadata.consigntype = strarr[0];
                                fsadata.pcscnt = strarr[1];//int.Parse(strarr[1]);
                                fsadata.weightcode = strarr[2];
                                fsadata.weight = strarr[3];//float.Parse(strarr[3]);
                                for (k = 4; k < strarr.Length; k += 2)
                                {
                                    if (strarr[k] != null)
                                    {
                                        if (strarr[k] == "T")
                                        {
                                            fsadata.totalpcscnt = strarr[k + 1];
                                        }
                                    }
                                }
                            }


                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex.Message); 
                            _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }
                        #endregion

                        for (int i = 2; i < str.Length; i++)
                        {
                            #region Decode
                            if (str[i].Length > 0)
                            {
                                string[] msg = str[i].Split('/');
                                MessageData.CommonStruct recdata = new MessageData.CommonStruct("");
                                switch (msg[0])
                                {
                                    case "FOH":
                                    case "RCS":
                                        {
                                            #region RCS
                                            if (msg.Length > 0)
                                            {
                                                try
                                                {
                                                    recdata.messageprefix = msg[0];
                                                    recdata.fltday = msg[1].Substring(0, 2);
                                                    recdata.fltmonth = msg[1].Substring(2, 3);
                                                    recdata.flttime = msg[1].Substring(5);
                                                    recdata.airportcode = msg[2];
                                                    string[] arr = stringsplitter(msg[3]);
                                                    if (arr.Length > 1)
                                                    {
                                                        recdata.pcsindicator = arr[0];
                                                        recdata.numofpcs = arr[1];
                                                        recdata.weightcode = arr[2];
                                                        recdata.weight = arr[3];
                                                    }
                                                    if (msg.Length > 4)
                                                    {
                                                        recdata.name = msg[4];
                                                        string[] strarr = stringsplitter(msg[5]);
                                                        if (strarr.Length > 0)
                                                        {
                                                            recdata.volumecode = strarr[0];
                                                            recdata.volumeamt = strarr[1];
                                                            recdata.densityindicator = strarr[2].Substring(0, 2);
                                                            recdata.densitygroup = strarr[2].Substring(2);
                                                        }
                                                    }

                                                }
                                                catch (Exception ex) {
                                                    // clsLog.WriteLogAzure(ex.Message);
                                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                 }
                                                Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                                fsanodes[fsanodes.Length - 1] = recdata;
                                            }
                                            #endregion
                                        }
                                        break;
                                    case "RCT":
                                        {
                                            #region RCT
                                            if (msg.Length > 0)
                                            {
                                                try
                                                {
                                                    recdata.messageprefix = msg[0];
                                                    recdata.carriercode = msg[1];
                                                    recdata.fltday = msg[2].Substring(0, 2);
                                                    recdata.fltmonth = msg[2].Substring(2, 3);
                                                    recdata.flttime = msg[2].Substring(5);
                                                    recdata.airportcode = msg[3];
                                                    string[] arr = stringsplitter(msg[4]);
                                                    if (arr.Length > 1)
                                                    {
                                                        try
                                                        {
                                                            recdata.pcsindicator = arr[0];
                                                            recdata.numofpcs = arr[1];
                                                            recdata.weightcode = arr[2];
                                                            recdata.weight = arr[3].Length > 0 ? arr[3] : "";
                                                        }
                                                        catch (Exception ex) {
                                                            // clsLog.WriteLogAzure(ex.Message);
                                                            _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                         }
                                                    }
                                                    if (msg.Length > 5)
                                                    {
                                                        recdata.seccarriercode = msg[5].Length > 0 ? msg[5] : "";
                                                    }
                                                    if (msg.Length > 6)
                                                    {
                                                        recdata.name = msg[6].Length > 0 ? msg[6] : "";
                                                    }
                                                }
                                                catch (Exception ex) {
                                                    // clsLog.WriteLogAzure(ex.Message);
                                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                 }
                                                Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                                fsanodes[fsanodes.Length - 1] = recdata;
                                            }
                                            #endregion
                                        }
                                        break;
                                    case "DIS":
                                        {
                                            #region DIS
                                            if (msg.Length > 0)
                                            {
                                                try
                                                {
                                                    recdata.messageprefix = msg[0];
                                                    recdata.flightnum = msg[1];
                                                    recdata.fltday = msg[2].Substring(0, 2);
                                                    recdata.fltmonth = msg[2].Substring(2, 3);
                                                    recdata.flttime = msg[2].Substring(5);
                                                    recdata.airportcode = msg[3];
                                                    recdata.infocode = msg[4];//Discrepency Code
                                                    string[] arr = stringsplitter(msg[5]);
                                                    if (arr.Length > 1)
                                                    {
                                                        try
                                                        {
                                                            recdata.pcsindicator = arr[0];
                                                            recdata.numofpcs = arr[1];
                                                            recdata.weightcode = arr[2];
                                                            recdata.weight = arr[3].Length > 0 ? arr[3] : "";
                                                        }
                                                        catch (Exception ex) {
                                                            // clsLog.WriteLogAzure(ex.Message); 
                                                            _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                        }
                                                    }
                                                    if (msg.Length > 6)
                                                    {
                                                        recdata.seccarriercode = msg[6].Length > 0 ? msg[6] : "";
                                                    }
                                                    if (msg.Length > 7)
                                                    {
                                                        recdata.name = msg[7].Length > 0 ? msg[7] : "";
                                                    }
                                                }
                                                catch (Exception ex) {
                                                    // clsLog.WriteLogAzure(ex.Message);
                                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                 }
                                                Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                                fsanodes[fsanodes.Length - 1] = recdata;
                                            }
                                            #endregion
                                        }
                                        break;
                                    case "NFD":
                                    case "AWD":
                                    case "CCD":
                                    case "DLV":
                                    case "DDL":
                                    case "TGC":
                                        {
                                            #region NFD
                                            if (msg.Length > 0)
                                            {
                                                try
                                                {
                                                    recdata.messageprefix = msg[0];
                                                    recdata.fltday = msg[1].Substring(0, 2);
                                                    recdata.fltmonth = msg[1].Substring(2, 3);
                                                    recdata.flttime = msg[1].Substring(5);
                                                    recdata.airportcode = msg[2];
                                                    string[] arr = stringsplitter(msg[3]);
                                                    if (arr.Length > 1)
                                                    {
                                                        try
                                                        {
                                                            recdata.pcsindicator = arr[0];
                                                            recdata.numofpcs = arr[1];
                                                            recdata.weightcode = arr[2];
                                                            recdata.weight = arr[3].Length > 0 ? arr[3] : "";
                                                        }
                                                        catch (Exception ex) {
                                                            // clsLog.WriteLogAzure(ex.Message);
                                                            _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                         }
                                                    }
                                                    if (msg.Length > 4)
                                                    {
                                                        recdata.name = msg[4].Length > 0 ? msg[4] : "";
                                                    }
                                                }
                                                catch (Exception ex) {
                                                    // clsLog.WriteLogAzure(ex.Message);
                                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}"); 
                                                }
                                                Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                                fsanodes[fsanodes.Length - 1] = recdata;
                                            }
                                            #endregion
                                        }
                                        break;


                                    case "RCF":
                                    case "ARR":
                                        {
                                            #region FUS/RCF
                                            try
                                            {
                                                string[] currentLineRCFText = str[i].ToString().Split('/');
                                                string arrTag = string.Empty;
                                                string time = string.Empty;

                                                bool fltpresent = false;
                                                bool fltanddatepresent = false;
                                                //bool onlydatepresent = false;

                                                string airPortCityCode = string.Empty;

                                                string dayChangeIndicator3 = string.Empty;
                                                int indexOfKOrL = 0;
                                                if (currentLineRCFText.Length >= 1)
                                                {
                                                    recdata.messageprefix = currentLineRCFText[0] != "" ? currentLineRCFText[0] : "";
                                                }

                                                if (currentLineRCFText.Length >= 2)
                                                {
                                                    if (currentLineRCFText[1].Length <= 8)
                                                    {
                                                        fltpresent = true;
                                                        recdata.carriercode = currentLineRCFText[1] != "" ? currentLineRCFText[1].Substring(0, 2) : "";
                                                        recdata.flightnum = currentLineRCFText[1] != "" ? currentLineRCFText[1].Substring(2) : "";
                                                    }
                                                    else
                                                    {
                                                        string[] currentLineTextSplit = currentLineRCFText[1].Split('-');
                                                        if (currentLineTextSplit.Length >= 1)
                                                        {
                                                            if (currentLineTextSplit[0].Length == 9)
                                                            {
                                                                recdata.fltday = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(0, 2) : "";
                                                                recdata.fltmonth = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(2, 3) : "";
                                                                recdata.flttime = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(5) : "";
                                                                //onlydatepresent = true;
                                                            }
                                                            else if (currentLineRCFText[0].Length == 8)
                                                            {
                                                                recdata.fltday = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(0, 1) : "";
                                                                recdata.fltmonth = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(1, 3) : "";
                                                                recdata.flttime = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(4) : "";
                                                                //onlydatepresent = true;
                                                            }
                                                        }
                                                        if (currentLineTextSplit.Length >= 2)
                                                        {
                                                            recdata.daychangeindicator = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                        }
                                                    }
                                                }
                                                //we need to check here condition that if both are present

                                                if (fltpresent)
                                                {
                                                    if (currentLineRCFText.Length >= 3)
                                                    {
                                                        string[] currentLineTextSplit = currentLineRCFText[2].Split('-');
                                                        if (currentLineTextSplit.Length >= 1)
                                                        {
                                                            if (currentLineTextSplit[0].Length == 9)
                                                            {
                                                                recdata.fltday = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(0, 2) : "";
                                                                recdata.fltmonth = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(2, 3) : "";
                                                                recdata.flttime = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(5) : "";
                                                                fltanddatepresent = true;
                                                            }
                                                            else if (currentLineRCFText[0].Length == 8)
                                                            {
                                                                recdata.fltday = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(0, 1) : "";
                                                                recdata.fltmonth = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(1, 3) : "";
                                                                recdata.flttime = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(4) : "";
                                                                fltanddatepresent = true;
                                                            }
                                                        }
                                                        if (currentLineTextSplit.Length >= 2)
                                                        {
                                                            recdata.daychangeindicator = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                        }
                                                    }


                                                    if (fltpresent == true && fltanddatepresent == false)
                                                    {
                                                        recdata.fltdest = currentLineRCFText[2] != "" ? currentLineRCFText[2] : "";
                                                    }

                                                    if ((fltanddatepresent == true && fltpresent == true))
                                                    {
                                                        recdata.fltdest = currentLineRCFText[3] != "" ? currentLineRCFText[3] : "";
                                                    }

                                                    //if (currentLineRCFText.Length >= 4)
                                                    //{
                                                    //    recdata.fltdest = currentLineRCFText[3] != "" ? currentLineRCFText[3] : "";
                                                    //}

                                                    if (currentLineRCFText.Length >= 5)
                                                    {
                                                        if (currentLineRCFText[4].Contains("K"))
                                                        {
                                                            indexOfKOrL = currentLineRCFText[4].LastIndexOf('K');
                                                        }
                                                        else if (currentLineRCFText[4].Contains("L"))
                                                        {
                                                            indexOfKOrL = currentLineRCFText[4].LastIndexOf('L');
                                                        }
                                                        if (!currentLineRCFText[4].Contains("K") && (!currentLineRCFText[4].Contains("L")) && (currentLineRCFText[4].Contains("T") || currentLineRCFText[3].Contains("P")))
                                                        {
                                                            recdata.pcsindicator = currentLineRCFText[4] != "" ? currentLineRCFText[4].Substring(0, 1) : "";


                                                            recdata.numofpcs = currentLineRCFText[4] != ""
                                                                                 ? (currentLineRCFText[4].Substring(1))
                                                                                 : "0";
                                                        }
                                                        else if (((currentLineRCFText[4].Contains("K")) || (currentLineRCFText[4].Contains("L"))) || (currentLineRCFText[4].Contains("T") || currentLineRCFText[3].Contains("P")))
                                                        {
                                                            recdata.pcsindicator = currentLineRCFText[4] != ""
                                                                             ? (currentLineRCFText[4].Substring(0, 1))
                                                                             : "";

                                                            recdata.numofpcs = currentLineRCFText[4] != ""
                                                                                 ? (currentLineRCFText[4].Substring(1, indexOfKOrL - 1))
                                                                                 : "0";

                                                            recdata.weightcode = currentLineRCFText[4] != ""
                                                                             ? (currentLineRCFText[4].Substring(indexOfKOrL, 1))
                                                                             : "";

                                                            recdata.weight = currentLineRCFText[4] != ""
                                                                         ? (currentLineRCFText[4].Substring(indexOfKOrL + 1))
                                                                         : "0";
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (currentLineRCFText.Length >= 3)
                                                    {
                                                        recdata.fltdest = currentLineRCFText[2] != "" ? currentLineRCFText[2] : "";
                                                    }

                                                    if (currentLineRCFText.Length >= 4)
                                                    {
                                                        if (currentLineRCFText[3].Contains("K"))
                                                        {
                                                            indexOfKOrL = currentLineRCFText[3].LastIndexOf('K');
                                                        }
                                                        else if (currentLineRCFText[3].Contains("L"))
                                                        {
                                                            indexOfKOrL = currentLineRCFText[3].LastIndexOf('L');
                                                        }
                                                        if (!currentLineRCFText[3].Contains("K") && (!currentLineRCFText[3].Contains("L")) && (currentLineRCFText[3].Contains("T") || currentLineRCFText[3].Contains("P")))
                                                        {
                                                            recdata.pcsindicator = currentLineRCFText[3] != "" ? currentLineRCFText[3].Substring(0, 1) : "";


                                                            recdata.numofpcs = currentLineRCFText[3] != ""
                                                                                 ? (currentLineRCFText[3].Substring(1))
                                                                                 : "0";
                                                        }
                                                        else if (((currentLineRCFText[3].Contains("K")) || (currentLineRCFText[3].Contains("L"))) || (currentLineRCFText[3].Contains("T") || currentLineRCFText[3].Contains("P")))
                                                        {
                                                            recdata.pcsindicator = currentLineRCFText[3] != ""
                                                                             ? (currentLineRCFText[3].Substring(0, 1))
                                                                             : "";

                                                            recdata.numofpcs = currentLineRCFText[3] != ""
                                                                                 ? (currentLineRCFText[3].Substring(1, indexOfKOrL - 1))
                                                                                 : "0";

                                                            recdata.weightcode = currentLineRCFText[3] != ""
                                                                             ? (currentLineRCFText[3].Substring(indexOfKOrL, 1))
                                                                             : "";

                                                            recdata.weight = currentLineRCFText[3] != ""
                                                                         ? (currentLineRCFText[3].Substring(indexOfKOrL + 1))
                                                                         : "0";
                                                        }
                                                    }

                                                }
                                                if (currentLineRCFText.Length >= 6)
                                                {
                                                    string[] currentLineTextSplit = currentLineRCFText[5].Split('-');
                                                    if (currentLineTextSplit.Length >= 1)
                                                    {
                                                        recdata.timeindicator = currentLineTextSplit[0] != ""
                                                                                      ? currentLineTextSplit[0].Substring(0, 1)
                                                                                      : "";

                                                        recdata.arrivaltime = currentLineTextSplit[0] != ""
                                                                              ? currentLineTextSplit[0].Substring(1)
                                                                              : "";
                                                    }

                                                    if (currentLineTextSplit.Length >= 2)
                                                    {
                                                        string dayChangeIndicator2 = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                    }
                                                }

                                                if (currentLineRCFText.Length >= 7)
                                                {
                                                    string[] currentLineTextSplit = currentLineRCFText[6].Split('-');
                                                    if (currentLineTextSplit.Length >= 1)
                                                    {
                                                        string timeIndicator = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(0, 1) : "";
                                                        string timeOfArrival = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(0, 1) : "";
                                                    }
                                                    if (currentLineTextSplit.Length >= 2)
                                                    {
                                                        dayChangeIndicator3 = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure("Error :", ex);
                                                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                            fsanodes[fsanodes.Length - 1] = recdata;
                                            #endregion





                                        }

                                        break;


                                    case "MAN":
                                    case "AWR":
                                    case "DEP":
                                    case "PRE":
                                        {
                                            #region MAN/DEP/PRE
                                            if (msg.Length > 0)
                                            {
                                                try
                                                {
                                                    //0
                                                    recdata.messageprefix = msg[0];
                                                    //1
                                                    recdata.carriercode = msg[1].Substring(0, 2);
                                                    recdata.flightnum = msg[1].Substring(2);
                                                    //2
                                                    string[] split = msg[2].Split('-');
                                                    recdata.fltday = split[0].Substring(0, 2);
                                                    recdata.fltmonth = split[0].Substring(2, 3);
                                                    recdata.flttime = split[0].Substring(5);
                                                    if (split.Length > 1)
                                                    {
                                                        recdata.daychangeindicator = split[1] + ",";
                                                    }
                                                    //3
                                                    if (msg[3].Length > 0)
                                                    {
                                                        try
                                                        {
                                                            if (msg[3].Length > 3)
                                                            {
                                                                recdata.fltorg = msg[3].Substring(0, 3);
                                                                recdata.fltdest = msg[3].Substring(3);
                                                            }
                                                            else
                                                            {
                                                                // recdata.fltorg = msg[3];
                                                                recdata.fltdest = msg[3];
                                                            }
                                                        }
                                                        catch (Exception ex) {
                                                            // clsLog.WriteLogAzure(ex.Message); 
                                                            _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                        }
                                                    }
                                                    //4 PCS Indicator
                                                    if (msg[4].Length > 0)
                                                    {
                                                        try
                                                        {
                                                            string[] arr = stringsplitter(msg[4]);
                                                            if (arr != null && arr.Length > 0)
                                                            {
                                                                recdata.pcsindicator = arr[0];
                                                                recdata.numofpcs = arr[1];
                                                                recdata.weightcode = arr[2];
                                                                recdata.weight = arr[3];

                                                            }

                                                        }
                                                        catch (Exception ex) {
                                                            // clsLog.WriteLogAzure(ex.Message); 
                                                            _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                        }
                                                    }
                                                    try
                                                    {
                                                        if (msg.Length > 5)
                                                        {
                                                            try
                                                            {
                                                                if (msg[5].Contains('-'))
                                                                {
                                                                    string[] strarr = msg[5].Split('-');
                                                                    recdata.timeindicator = recdata.timeindicator + strarr[0].Substring(0, 1) + ",";
                                                                    recdata.depttime = strarr[0].Substring(1);
                                                                    if (strarr.Length > 1)
                                                                    {
                                                                        recdata.daychangeindicator = recdata.daychangeindicator + strarr[1] + ",";
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    recdata.timeindicator = recdata.timeindicator + msg[5].Substring(0, 1) + ",";
                                                                    recdata.depttime = msg[5].Substring(1);
                                                                }
                                                            }
                                                            catch (Exception ex) {
                                                                // clsLog.WriteLogAzure(ex.Message);
                                                                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                             }
                                                        }
                                                        if (msg.Length > 6)
                                                        {
                                                            try
                                                            {
                                                                if (msg[6].Contains('-'))
                                                                {
                                                                    string[] strarr = msg[6].Split('-');
                                                                    recdata.timeindicator = recdata.timeindicator + strarr[0].Substring(0, 1) + ",";
                                                                    recdata.arrivaltime = strarr[0].Substring(1);
                                                                    if (strarr.Length > 1)
                                                                    {
                                                                        recdata.daychangeindicator = recdata.daychangeindicator + strarr[1] + ",";
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    recdata.timeindicator = recdata.timeindicator + msg[6].Substring(0, 1) + ",";
                                                                    recdata.arrivaltime = msg[6].Substring(1);
                                                                }
                                                            }
                                                            catch (Exception ex) {
                                                                // clsLog.WriteLogAzure(ex.Message);
                                                                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                             }
                                                        }
                                                    }
                                                    catch (Exception ex) {
                                                        // clsLog.WriteLogAzure(ex.Message); 
                                                        _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                    }

                                                }
                                                catch (Exception ex) {
                                                    // clsLog.WriteLogAzure(ex.Message);
                                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                 }
                                                Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                                fsanodes[fsanodes.Length - 1] = recdata;
                                            }
                                            #endregion
                                        }
                                        break;

                                    case "BKD":
                                        {
                                            #region BKD
                                            if (msg.Length > 0)
                                            {
                                                try
                                                {
                                                    recdata.messageprefix = msg[0];
                                                    recdata.carriercode = msg[1];
                                                    string[] split = msg[2].Split('-');
                                                    recdata.fltday = split[0].Substring(0, 2);
                                                    recdata.fltmonth = split[0].Substring(2, 3);
                                                    recdata.flttime = split[0].Substring(5);
                                                    recdata.daychangeindicator = split[1] + ",";
                                                    recdata.fltdest = msg[3].Substring(0, 3);
                                                    recdata.fltorg = msg[3].Substring(3);
                                                    string[] arr = stringsplitter(msg[4]);
                                                    if (arr.Length > 0)
                                                    {
                                                        recdata.pcsindicator = arr[0];
                                                        recdata.numofpcs = arr[1];
                                                        recdata.weightcode = arr[2];
                                                        recdata.weight = arr[3];
                                                    }
                                                    if (msg.Length > 5)
                                                    {
                                                        string[] strarr = msg[5].Split('-');
                                                        recdata.timeindicator = recdata.timeindicator + strarr[0].Substring(0, 1) + ",";
                                                        recdata.depttime = strarr[0].Substring(1);
                                                        recdata.daychangeindicator = recdata.daychangeindicator + strarr[1] + ",";
                                                    }
                                                    if (msg.Length > 6)
                                                    {
                                                        string[] strarr = msg[6].Split('-');
                                                        recdata.timeindicator = recdata.timeindicator + strarr[0].Substring(0, 1) + ",";
                                                        recdata.arrivaltime = strarr[0].Substring(1);
                                                        recdata.daychangeindicator = recdata.daychangeindicator + strarr[1] + ",";
                                                    }
                                                    if (msg.Length > 7)
                                                    {
                                                        string[] strarr = stringsplitter(msg[7]);
                                                        if (strarr.Length > 0)
                                                        {
                                                            recdata.volumecode = strarr[0];
                                                            recdata.volumeamt = strarr[1];
                                                            recdata.densityindicator = "DG";
                                                            recdata.densitygroup = strarr[3].Length > 0 ? strarr[3] : "";
                                                        }
                                                    }

                                                }
                                                catch (Exception ex) {
                                                    // clsLog.WriteLogAzure(ex.Message);
                                                    _staticLogger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}"); 
                                                }
                                                Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                                fsanodes[fsanodes.Length - 1] = recdata;
                                            }
                                            #endregion

                                        }
                                        break;
                                    case "TRM":
                                        {
                                            #region TRM
                                            if (msg.Length > 1)
                                            {
                                                try
                                                {
                                                    recdata.messageprefix = msg[0];
                                                    recdata.carriercode = msg[1];
                                                    recdata.airportcode = msg[2];
                                                    string[] arr = stringsplitter(msg[3]);
                                                    if (arr.Length > 0)
                                                    {
                                                        recdata.pcsindicator = arr[0];
                                                        recdata.numofpcs = arr[1];
                                                        recdata.weightcode = arr[2];
                                                        recdata.weight = arr[3];
                                                    }
                                                }
                                                catch (Exception ex) {
                                                    // clsLog.WriteLogAzure(ex.Message);
                                                    _staticLogger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                 }
                                                Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                                fsanodes[fsanodes.Length - 1] = recdata;
                                            }
                                            #endregion
                                        }
                                        break;
                                    case "TFD":
                                        {
                                            #region TFD
                                            if (msg.Length > 0)
                                            {
                                                try
                                                {
                                                    recdata.messageprefix = msg[0];
                                                    recdata.carriercode = msg[1];
                                                    recdata.fltday = msg[2].Substring(0, 2);
                                                    recdata.fltmonth = msg[2].Substring(2, 3);
                                                    recdata.flttime = msg[2].Substring(5);
                                                    recdata.airportcode = msg[3];
                                                    string[] arr = stringsplitter(msg[4]);
                                                    if (arr.Length > 0)
                                                    {
                                                        try
                                                        {
                                                            recdata.pcsindicator = arr[0];
                                                            recdata.numofpcs = arr[1];
                                                            recdata.weightcode = arr[2];
                                                            recdata.weight = arr[3].Length > 0 ? arr[3] : "";
                                                        }
                                                        catch (Exception ex) {
                                                            // clsLog.WriteLogAzure(ex.Message);
                                                            _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                         }
                                                    }
                                                    if (msg.Length > 5)
                                                    {
                                                        recdata.transfermanifestnumber = msg[5].Length > 0 ? msg[5] : "";
                                                    }
                                                    if (msg.Length > 6)
                                                    {
                                                        recdata.seccarriercode = msg[6].Length > 0 ? msg[6] : "";
                                                    }
                                                    if (msg.Length > 7)
                                                    {
                                                        recdata.name = msg[7].Length > 0 ? msg[7] : "";
                                                    }
                                                }
                                                catch (Exception ex) {
                                                    // clsLog.WriteLogAzure(ex.Message);
                                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                 }
                                                Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                                fsanodes[fsanodes.Length - 1] = recdata;
                                            }
                                            #endregion
                                        }
                                        break;

                                    case "CRC":
                                        {
                                            #region CRC
                                            if (msg.Length > 0)
                                            {
                                                try
                                                {
                                                    recdata.messageprefix = msg[0];
                                                    recdata.fltday = msg[1].Substring(0, 2);
                                                    recdata.fltmonth = msg[1].Substring(2, 3);
                                                    recdata.flttime = msg[1].Substring(5);
                                                    recdata.airportcode = msg[2];
                                                    string[] arr = stringsplitter(msg[3]);
                                                    if (arr.Length > 0)
                                                    {
                                                        recdata.pcsindicator = arr[0];
                                                        recdata.numofpcs = arr[1];
                                                        recdata.weightcode = arr[2];
                                                        recdata.weight = arr[3];
                                                    }
                                                    if (msg.Length > 4)
                                                    {
                                                        recdata.carriercode = msg[4].Substring(0, 2);
                                                        recdata.flightnum = msg[4].Substring(2);
                                                    }
                                                    if (msg.Length > 5)
                                                    {
                                                        recdata.fltday = msg[5].Substring(0, 2);
                                                        recdata.fltmonth = msg[5].Substring(2);
                                                    }
                                                    if (msg.Length > 6)
                                                    {
                                                        recdata.fltdest = msg[6].Substring(0, 3);
                                                        recdata.fltorg = msg[6].Substring(3);
                                                    }

                                                }
                                                catch (Exception ex) {
                                                    // clsLog.WriteLogAzure(ex.Message); 
                                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                }
                                                Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                                fsanodes[fsanodes.Length - 1] = recdata;
                                            }
                                            #endregion
                                        }
                                        break;

                                    case "OCI":
                                        {
                                            #region OCI
                                            if (msg.Length > 0)
                                            {
                                                lastrec = "OCI";
                                                MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
                                                try
                                                {
                                                    custom.IsoCountryCodeOci = msg[1];
                                                    custom.InformationIdentifierOci = msg[2];
                                                    custom.CsrIdentifierOci = msg[3];
                                                    custom.SupplementaryCsrIdentifierOci = msg[4];
                                                    custom.consigref = awbref;
                                                }
                                                catch (Exception ex) {
                                                    // clsLog.WriteLogAzure(ex.Message);
                                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                 }
                                                Array.Resize(ref custominfo, custominfo.Length + 1);
                                                custominfo[custominfo.Length - 1] = custom;
                                            }
                                            #endregion
                                        }
                                        break;
                                    case "ULD":
                                        {
                                            #region ULD
                                            try
                                            {
                                                int uldnum = 0;
                                                if (msg.Length > 1)
                                                {
                                                    if (int.Parse(msg[1]) > 0)
                                                    {
                                                        Array.Resize(ref uld, str.Length);
                                                        for (int k = 1; k < msg.Length; k++)
                                                        {
                                                            string[] splitstr = msg[k].Split('-');
                                                            uld[uldnum].uldtype = splitstr[0].Substring(0, 3);
                                                            uld[uldnum].uldsrno = splitstr[0].Substring(3, splitstr[0].Length - 6);
                                                            uld[uldnum].uldowner = splitstr[0].Substring(splitstr[0].Length - 3, 3);
                                                            uld[uldnum].uldloadingindicator = splitstr[1];
                                                            uldnum++;
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex.Message); 
                                                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                            #endregion

                                        }
                                        break;
                                    case "OSI":
                                        {
                                            #region OSI
                                            try
                                            {
                                                lastrec = msg[0];
                                                if (msg[1].Length > 0)
                                                {
                                                    Array.Resize(ref othinfoarray, othinfoarray.Length + 1);
                                                    othinfoarray[othinfoarray.Length - 1].otherserviceinfo1 = msg[1];

                                                }
                                            }
                                            catch (Exception ex) {
                                                // clsLog.WriteLogAzure(ex.Message); 
                                                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                            #endregion

                                        }
                                        break;
                                    default:
                                        {
                                            #region Other Info
                                            if (str[i].StartsWith("/"))
                                            {
                                                string[] msgdata = str[i].Split('/');
                                                try
                                                {
                                                    #region OSI 2
                                                    if (lastrec == "OSI")
                                                    {
                                                        othinfoarray[othinfoarray.Length - 1].otherserviceinfo2 = msgdata[1].Length > 0 ? msgdata[1] : "";
                                                        lastrec = "NA";
                                                    }
                                                    #endregion

                                                    #region OCI
                                                    if (lastrec == "OCI")
                                                    {

                                                        if (msgdata.Length > 0)
                                                        {
                                                            lastrec = "OCI";
                                                            MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
                                                            custom.IsoCountryCodeOci = msgdata[1];
                                                            custom.InformationIdentifierOci = msgdata[2];
                                                            custom.CsrIdentifierOci = msgdata[3];
                                                            custom.SupplementaryCsrIdentifierOci = msgdata[4];
                                                            Array.Resize(ref custominfo, custominfo.Length + 1);
                                                            custominfo[custominfo.Length - 1] = custom;
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                catch (Exception ex)
                                                {
                                                    // clsLog.WriteLogAzure(ex.Message);
                                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                 }
                                            }
                                            #endregion
                                        }
                                        break;
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                flag = false;
                // clsLog.WriteLogAzure(ex);
                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return flag;
        }
        #endregion

        #region StringSplitter
        public static string[] stringsplitter(string str)
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
                // clsLog.WriteLogAzure(ex);
                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                strarr = null;
            }
            return strarr;
        }
        #endregion

        #region EncodeFSA
        public static string EncodeFSAforSend(ref MessageData.FSAInfo fsadata, ref MessageData.CommonStruct[] fsanodes, ref MessageData.customsextrainfo[] custominfo, ref MessageData.ULDinfo[] uld, ref MessageData.otherserviceinfo[] othinfoarray)
        {
            string FSAStr = null;
            try
            {
                #region Line 1
                string line1 = "FSA/" + fsadata.fsaversion;
                #endregion

                #region line 2 consigment detials
                string line2 = "";
                line2 = line2 + fsadata.airlineprefix + "-" + fsadata.awbnum + fsadata.origin + fsadata.dest + "/" + fsadata.consigntype + fsadata.pcscnt + fsadata.weightcode + fsadata.weight + (fsadata.totalpcscnt.Length > 0 ? "T" + fsadata.totalpcscnt : "");
                #endregion

                #region Line 3 Encode message
                string line3 = "";
                if (fsanodes.Length > 0)
                {
                    for (int i = 0; i < fsanodes.Length; i++)
                    {
                        switch (fsanodes[i].messageprefix.Trim())
                        {
                            case "FOH":
                            case "RCS":
                                {
                                    #region RCS
                                    line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + fsanodes[i].flttime + "/" + fsanodes[i].airportcode + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight + (fsanodes[i].name.Length > 0 ? "/" + fsanodes[i].name : "") + "/" + fsanodes[i].volumecode + fsanodes[i].volumeamt + fsanodes[i].daychangeindicator + fsanodes[i].densitygroup;
                                    #endregion
                                }
                                break;
                            case "RCT":
                            case "DIS":
                                {
                                    #region RCT
                                    line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].carriercode + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + fsanodes[i].flttime + "/" + fsanodes[i].airportcode + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight + "/" + (fsanodes[i].name.Length > 0 ? fsanodes[i].name : "");
                                    #endregion
                                }
                                break;
                            case "NFD":
                            case "AWD":
                            case "CCD":
                            case "DLV":
                            case "DDL":
                            case "TGC":
                                {
                                    #region RCT
                                    line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + fsanodes[i].flttime + "/" + fsanodes[i].airportcode + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight + "/" + (fsanodes[i].name.Length > 0 ? fsanodes[i].name : "");
                                    #endregion
                                }
                                break;
                            case "RCF":
                            case "MAN":
                            case "ARR":
                            case "AWR":
                            case "DEP":
                            case "PRE":
                                {
                                    #region RCF/MAN/DEP/PRE
                                    string[] daychange = new string[0];
                                    string[] timechange = new string[0];
                                    if (fsanodes[i].daychangeindicator.Length > 0)
                                    {
                                        daychange = fsanodes[i].daychangeindicator.Split(',');
                                    }

                                    if (fsanodes[i].timeindicator.Length > 0)
                                    {
                                        timechange = fsanodes[i].timeindicator.Split(',');
                                    }

                                    line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].carriercode + fsanodes[i].flightnum + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + fsanodes[i].flttime + (daychange.Length > 0 ? "-" + daychange[0] : "") + "/" + fsanodes[i].airportcode + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight + (timechange.Length > 0 ? "/" + timechange[0] : "") + fsanodes[i].depttime + (daychange.Length > 1 ? "-" + daychange[1] : "") + (timechange.Length > 1 ? "/" + timechange[1] : "") + fsanodes[i].arrivaltime + (daychange.Length > 2 ? "-" + daychange[2] : "");
                                    #endregion
                                }
                                break;
                            case "BKD":
                                {
                                    #region BKD
                                    string[] daychange = new string[0];
                                    string[] timechange = new string[0];
                                    if (fsanodes[i].daychangeindicator.Length > 0)
                                    {
                                        daychange = fsanodes[i].daychangeindicator.Split(',');
                                    }

                                    if (fsanodes[i].timeindicator.Length > 0)
                                    {
                                        timechange = fsanodes[i].timeindicator.Split(',');
                                    }

                                    line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].carriercode + fsanodes[i].flightnum + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + "/" + fsanodes[i].fltdest + fsanodes[i].fltorg + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight + (timechange.Length > 0 ? "/" + timechange[0] : "") + fsanodes[i].depttime + (daychange.Length > 1 ? "-" + daychange[1] : "") + (timechange.Length > 1 ? "/" + timechange[1] : "") + fsanodes[i].arrivaltime + (daychange.Length > 2 ? "-" + daychange[2] : "") + "/" + fsanodes[i].volumecode + fsanodes[i].volumeamt + fsanodes[i].daychangeindicator + fsanodes[i].densitygroup;

                                    #endregion

                                }
                                break;
                            case "TRM":
                                {
                                    #region TRM
                                    line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].fltdest + fsanodes[i].fltorg + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight;
                                    #endregion
                                }
                                break;
                            case "TFD":
                                {
                                    #region TFD
                                    line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].carriercode + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + fsanodes[i].flttime + "/" + fsanodes[i].airportcode + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight + "/" + (fsanodes[i].transfermanifestnumber.Length > 0 ? fsanodes[i].transfermanifestnumber : "") + "/" + fsanodes[i].carriercode + "/" + fsanodes[i].name;
                                    #endregion
                                }
                                break;

                            case "CRC":
                                {
                                    #region CRC
                                    line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + fsanodes[i].flttime + "/" + fsanodes[i].airportcode + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight + "/" + fsanodes[i].carriercode + fsanodes[i].flightnum + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + "/" + fsanodes[i].fltdest + fsanodes[i].fltorg;
                                    #endregion
                                }
                                break;

                        }
                        line3 = line3.Trim('/') + "$";
                    }
                    line3 = line3.Trim('$');
                    line3 = line3.Replace("$", "\r\n");
                }
                #endregion

                #region Line 4 OCI
                string line4 = "";
                if (custominfo.Length > 0)
                {
                    for (int i = 0; i < custominfo.Length; i++)
                    {
                        line4 = "/" + custominfo[i].IsoCountryCodeOci + "/" + custominfo[i].InformationIdentifierOci + "/" + custominfo[i].CsrIdentifierOci + "/" + custominfo[i].SupplementaryCsrIdentifierOci + "$";
                    }
                    line4 = "OCI" + line4.Trim('$');
                    line4 = line4.Replace("$", "\r\n");
                }
                #endregion

                #region Line 5 ULD
                string line5 = "";
                if (uld.Length > 0)
                {
                    line5 = "ULD";
                    for (int i = 0; i < uld.Length; i++)
                    {

                        line5 = line5.Trim() + "/" + uld[i].uldtype + uld[i].uldsrno + uld[i].uldowner + (uld[i].uldloadingindicator.Length > 0 ? ("-" + uld[i].uldloadingindicator) : "");
                    }
                }
                #endregion

                #region Line 6 OSI
                string line6 = "";
                if (othinfoarray.Length > 0)
                {
                    for (int i = 0; i < othinfoarray.Length; i++)
                    {
                        if (othinfoarray[i].otherserviceinfo1.Length > 0)
                        {
                            line6 = "OSI/" + othinfoarray[i].otherserviceinfo1 + "$";
                            if (othinfoarray[i].otherserviceinfo2.Length > 0)
                            {
                                line6 = line6 + "/" + othinfoarray[i].otherserviceinfo2 + "$";
                            }
                        }
                    }
                    line6 = line6.Trim('$');
                    line6 = line6.Replace("$", "\r\n");
                }
                #endregion

                #region Build FSA
                FSAStr = FSAStr + line1.Trim('/') + "\r\n" + line2.Trim('/') + "\r\n" + line3.Trim('/');
                if (line4.Length > 0)
                {
                    FSAStr = FSAStr + "\r\n" + line4.Trim('/');
                }
                if (line5.Length > 0)
                {
                    FSAStr = FSAStr + "\r\n" + line5.Trim('/');
                }
                if (line6.Length > 0)
                {
                    FSAStr = FSAStr + "\r\n" + line6.Trim('/');
                }
                #endregion
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                FSAStr = "ERR";
            }
            return FSAStr;
        }
        #endregion

        //#region EncodeFSU
        //public static string EncodeFSUforSend(ref MessageData.FSAInfo fsadata, ref MessageData.CommonStruct[] fsanodes, ref MessageData.customsextrainfo[] custominfo, ref MessageData.ULDinfo[] uld, ref MessageData.otherserviceinfo[] othinfoarray)
        //{
        //    string FSAStr = null;
        //    try
        //    {
        //        #region Line 1
        //        string line1 = "FSU/" + fsadata.fsaversion;
        //        #endregion

        //        #region line 2 consigment detials
        //        string line2 = "";
        //        line2 = line2 + fsadata.airlineprefix + "-" + fsadata.awbnum + fsadata.origin + fsadata.dest + "/" + fsadata.consigntype + fsadata.pcscnt + fsadata.weightcode + fsadata.weight + (fsadata.totalpcscnt.Length > 0 ? "T" + fsadata.totalpcscnt : "");
        //        #endregion

        //        #region Line 3 Encode message
        //        string line3 = "", str = "";
        //        if (fsanodes.Length > 0)
        //        {
        //            for (int i = 0; i < fsanodes.Length; i++)
        //            {
        //                switch (fsanodes[i].messageprefix.Trim())
        //                {
        //                    case "FOH":
        //                    case "RCS":
        //                        {
        //                            #region RCS
        //                            line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + fsanodes[i].flttime + "/" + fsanodes[i].airportcode + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight + (fsanodes[i].name.Length > 0 ? "/" + fsanodes[i].name : "") + "/" + fsanodes[i].volumecode + fsanodes[i].volumeamt + fsanodes[i].daychangeindicator + fsanodes[i].densitygroup;
        //                            #endregion
        //                        }
        //                        break;
        //                    case "RCT":
        //                        {
        //                            #region RCT
        //                            line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].carriercode + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + fsanodes[i].flttime + "/" + fsanodes[i].airportcode + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight + "/" + (fsanodes[i].name.Length > 0 ? fsanodes[i].name : "");
        //                            #endregion
        //                        }
        //                        break;
        //                    case "DIS":
        //                        {
        //                            #region RCT
        //                            line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].carriercode + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + fsanodes[i].flttime + "/" + fsanodes[i].airportcode + "/" + fsanodes[i].infocode.ToString() + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight + "/" + (fsanodes[i].name.Length > 0 ? fsanodes[i].name : "");
        //                            #endregion
        //                        }
        //                        break;
        //                    case "NFD":
        //                    case "AWD":
        //                    case "CCD":
        //                    case "DLV":
        //                    case "DDL":
        //                    case "TGC":
        //                        {
        //                            #region RCT
        //                            line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + fsanodes[i].flttime + "/" + fsanodes[i].airportcode + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight + "/" + (fsanodes[i].name.Length > 0 ? fsanodes[i].name : "");
        //                            #endregion
        //                        }
        //                        break;
        //                    case "RCF":
        //                    case "MAN":
        //                    case "ARR":
        //                    case "AWR":
        //                    case "DEP":
        //                    case "PRE":
        //                        {
        //                            #region RCF/MAN/DEP/PRE
        //                            string[] daychange = new string[0];
        //                            string[] timechange = new string[0];
        //                            if (fsanodes[i].daychangeindicator.Length > 0)
        //                            {
        //                                daychange = fsanodes[i].daychangeindicator.Split(',');
        //                            }

        //                            if (fsanodes[i].timeindicator.Length > 0)
        //                            {
        //                                timechange = fsanodes[i].timeindicator.Split(',');
        //                            }

        //                            line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].carriercode + fsanodes[i].flightnum + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + fsanodes[i].flttime + (daychange.Length > 0 ? "-" + daychange[0] : "") + "/" + fsanodes[i].airportcode + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight + (timechange.Length > 0 ? "/" + timechange[0] : "") + fsanodes[i].depttime + (daychange.Length > 1 ? "-" + daychange[1] : "") + (timechange.Length > 1 ? "/" + timechange[1] : "") + fsanodes[i].arrivaltime + (daychange.Length > 2 ? "-" + daychange[2] : "");
        //                            #endregion
        //                        }
        //                        break;
        //                    case "BKD":
        //                        {
        //                            #region BKD
        //                            string[] daychange = new string[0];
        //                            string[] timechange = new string[0];
        //                            if (fsanodes[i].daychangeindicator.Length > 0)
        //                            {
        //                                daychange = fsanodes[i].daychangeindicator.Split(',');
        //                            }

        //                            if (fsanodes[i].timeindicator.Length > 0)
        //                            {
        //                                timechange = fsanodes[i].timeindicator.Split(',');
        //                            }

        //                            line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].carriercode + fsanodes[i].flightnum + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + "/" + fsanodes[i].fltdest + fsanodes[i].fltorg + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight + (timechange.Length > 0 ? "/" + timechange[0] : "") + fsanodes[i].depttime + (daychange.Length > 1 ? "-" + daychange[1] : "") + (timechange.Length > 1 ? "/" + timechange[1] : "") + fsanodes[i].arrivaltime + (daychange.Length > 2 ? "-" + daychange[2] : "") + "/" + fsanodes[i].volumecode + fsanodes[i].volumeamt + fsanodes[i].daychangeindicator + fsanodes[i].densitygroup;

        //                            #endregion

        //                        } break;
        //                    case "TRM":
        //                        {
        //                            #region TRM
        //                            line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].fltdest + fsanodes[i].fltorg + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight;
        //                            #endregion
        //                        } break;
        //                    case "TFD":
        //                        {
        //                            #region TFD
        //                            line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].carriercode + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + fsanodes[i].flttime + "/" + fsanodes[i].airportcode + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight + "/" + (fsanodes[i].transfermanifestnumber.Length > 0 ? fsanodes[i].transfermanifestnumber : "") + "/" + fsanodes[i].carriercode + "/" + fsanodes[i].name;
        //                            #endregion
        //                        } break;

        //                    case "CRC":
        //                        {
        //                            #region CRC
        //                            line3 = line3 + fsanodes[i].messageprefix + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + fsanodes[i].flttime + "/" + fsanodes[i].airportcode + "/" + fsanodes[i].pcsindicator + fsanodes[i].numofpcs + fsanodes[i].weightcode + fsanodes[i].weight + "/" + fsanodes[i].carriercode + fsanodes[i].flightnum + "/" + fsanodes[i].fltday + fsanodes[i].fltmonth + "/" + fsanodes[i].fltdest + fsanodes[i].fltorg;
        //                            #endregion
        //                        } break;

        //                }
        //                line3 = line3.Trim('/') + "$";
        //            }
        //            line3 = line3.Trim('$');
        //            line3 = line3.Replace("$", "\r\n");
        //        }
        //        #endregion

        //        #region Line 4 OCI
        //        string line4 = "";
        //        if (custominfo.Length > 0)
        //        {
        //            for (int i = 0; i < custominfo.Length; i++)
        //            {
        //                line4 = "/" + custominfo[i].IsoCountryCodeOci + "/" + custominfo[i].InformationIdentifierOci + "/" + custominfo[i].CsrIdentifierOci + "/" + custominfo[i].SupplementaryCsrIdentifierOci + "$";
        //            }
        //            line4 = "OCI" + line4.Trim('$');
        //            line4 = line4.Replace("$", "\r\n");
        //        }
        //        #endregion

        //        #region Line 5 ULD
        //        string line5 = "";
        //        if (uld.Length > 0)
        //        {
        //            line5 = "ULD";
        //            for (int i = 0; i < uld.Length; i++)
        //            {

        //                line5 = line5.Trim() + "/" + uld[i].uldtype + uld[i].uldsrno + uld[i].uldowner + (uld[i].uldloadingindicator.Length > 0 ? ("-" + uld[i].uldloadingindicator) : "");
        //            }
        //        }
        //        #endregion

        //        #region Line 6 OSI
        //        string line6 = "";
        //        if (othinfoarray.Length > 0)
        //        {
        //            for (int i = 0; i < othinfoarray.Length; i++)
        //            {
        //                if (othinfoarray[i].otherserviceinfo1.Length > 0)
        //                {
        //                    line6 = "OSI/" + othinfoarray[i].otherserviceinfo1 + "$";
        //                    if (othinfoarray[i].otherserviceinfo2.Length > 0)
        //                    {
        //                        line6 = line6 + "/" + othinfoarray[i].otherserviceinfo2 + "$";
        //                    }
        //                }
        //            }
        //            line6 = line6.Trim('$');
        //            line6 = line6.Replace("$", "\r\n");
        //        }
        //        #endregion

        //        #region Build FSA
        //        FSAStr = FSAStr + line1.Trim('/') + "\r\n" + line2.Trim('/') + "\r\n" + line3.Trim('/');
        //        if (line4.Length > 0)
        //        {
        //            FSAStr = FSAStr + "\r\n" + line4.Trim('/');
        //        }
        //        if (line5.Length > 0)
        //        {
        //            FSAStr = FSAStr + "\r\n" + line5.Trim('/');
        //        }
        //        if (line6.Length > 0)
        //        {
        //            FSAStr = FSAStr + "\r\n" + line6.Trim('/');
        //        }
        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure("ERR in FSU Encode:", ex);
        //    }
        //    return FSAStr;
        //}
        //#endregion

        //FWB

        //#region Encode FWB OLD
        ////public static string EncodeFWBForSendOLD(ref MessageData.fwbinfo fwbData, ref MessageData.othercharges[] fwbOtherCharge, ref MessageData.otherserviceinfo othData, ref MessageData.RateDescription[] fwbrate)
        ////{
        ////    string FWBStr = null;
        ////    try
        ////    {
        ////        //FWB
        ////        #region Line 1
        ////        FWBStr = "FWB/" + fwbData.fwbversionnum;
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        #region Line 2
        ////        FWBStr += fwbData.airlineprefix + "-" + fwbData.awbnum + "" + fwbData.origin + "" + fwbData.dest + "/T" + fwbData.pcscnt + "" + fwbData.weightcode + "" + fwbData.weight + "";
        ////        FWBStr += fwbData.volumecode + "" + fwbData.volumeamt + "" + fwbData.densityindicator + "" + fwbData.densitygrp + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //FLT
        ////        #region Line 3
        ////        FWBStr += "FLT/" + fwbData.carriercode + "" + fwbData.fltnum + "/" + fwbData.fltday.PadLeft(2, '0') + "/" + fwbData.carriercode + "" + fwbData.fltnum + "/" + fwbData.fltday.PadLeft(2, '0') + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //RTG
        ////        #region Line 4
        ////        FWBStr += "RTG/" + fwbData.origin + "" + fwbData.carriercode + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //SHP
        ////        #region Line 5
        ////        FWBStr += "SHP/" + fwbData.shipperaccnum + "" + fwbData.shippername + "/" + fwbData.shipperadd + "/" +
        ////               fwbData.shipperplace + "/" + fwbData.shipperstate + "/" + fwbData.shippercountrycode + "/" +
        ////               fwbData.shipperpostcode + "/" + fwbData.shippercontactidentifier + "/" + fwbData.shippercontactnum + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //CNE
        ////        #region Line 6
        ////        FWBStr += "CNE/" + fwbData.consaccnum + "/" + fwbData.consname + "/" + fwbData.consadd + "/" + fwbData.consplace + "/" + fwbData.consstate + "/" +
        ////               fwbData.conscountrycode + "/" + fwbData.conspostcode + "/" + fwbData.conscontactidentifier + "/" + fwbData.conscontactnum + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //AGT
        ////        #region Line 7
        ////        FWBStr += "AGT/" + fwbData.agentaccnum + "/" + fwbData.agentIATAnumber + "/" + fwbData.agentCASSaddress + "/" +
        ////               fwbData.agentParticipentIdentifier + "/" + fwbData.agentname + "/" + fwbData.agentplace + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //SSR
        ////        #region Line 8
        ////        FWBStr += "SSR/" + fwbData.specialservicereq1 + "/" + fwbData.specialservicereq2 + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //NFY
        ////        #region Line 9
        ////        FWBStr += "NFY/" + fwbData.notifyname + "/" + fwbData.notifyadd + "/" + fwbData.notifyplace + "/" +
        ////               fwbData.notifystate + "/" + fwbData.notifycountrycode + "/" + fwbData.notifypostcode + "/" +
        ////               fwbData.notifycontactidentifier + "/" + fwbData.notifycontactnum + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //ACC
        ////        #region Line 10
        ////        FWBStr += "ACC/" + fwbData.accountinginfoidentifier + "/" + fwbData.accountinginfo + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //CVD
        ////        #region Line 11
        ////        FWBStr += "CVD/" + fwbData.currency + "/" + fwbData.chargecode + "/PP/" + fwbData.declaredvalue + "/" + fwbData.declaredcustomvalue + "/" + fwbData.insuranceamount + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //RTD
        ////        #region Line 12     Pending
        ////        FWBStr += "RTD/1/P" + fwbData.pcscnt + "/K" + fwbData.weight + "/C";
        ////        FWBStr += "\r\n";

        ////        #region Line 12-1
        ////        #endregion

        ////        #region Line 12-2
        ////        #endregion

        ////        #region Line 12-3
        ////        #endregion

        ////        #region Line 12-4
        ////        #endregion

        ////        #region Line 12-5
        ////        #endregion

        ////        #region Line 12-6
        ////        #endregion

        ////        #region Line 12-7
        ////        #endregion

        ////        #region Line 12-8
        ////        #endregion
        ////        #endregion

        ////        //OTH
        ////        #region Line 13
        ////        FWBStr += "OTH/P/";
        ////        for (int i = 0; i < othData.Length; i++)
        ////        {
        ////            if (i > 0)
        ////            {
        ////                if (i % 3 == 0)
        ////                {
        ////                    if (i != othData.Length)
        ////                    {
        ////                        FWBStr += "\r\nP";
        ////                    }
        ////                }
        ////            }
        ////            FWBStr += othData[i].otherchargecode + "" + othData[i].entitlementcode + "" + othData[i].chargeamt;
        ////            //if (i % 3 == 0)
        ////            //{
        ////            //    if(i != othData.Length)
        ////            //    {
        ////            //        FWBStr += "\r\nP";
        ////            //    }
        ////            //}
        ////        }
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //PPD
        ////        #region Line 14
        ////        FWBStr += "PPD/WT" + fwbData.PPweightCharge + "/VC" + fwbData.PPValuationCharge + "/TX" + fwbData.PPTaxesCharge + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //COL
        ////        #region Line 15
        ////        FWBStr += "COL/WT" + fwbData.CCweightCharge + "/VC" + fwbData.CCValuationCharge + "/TX" + fwbData.CCTaxesCharge + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //CER
        ////        #region Line 16
        ////        FWBStr += "CER/" + fwbData.shippersignature + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //ISU
        ////        #region Line 17
        ////        FWBStr += "ISU/" + fwbData.carrierdate.PadLeft(2, '0') + "" + fwbData.carriermonth.PadLeft(2, '0') + "" + fwbData.carrieryear.PadLeft(2, '0') + "/" + fwbData.carrierplace + "/" + fwbData.carriersignature + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //OSI
        ////        #region Line 18
        ////        FWBStr += "OSI/" + otherServInfo.otherserviceinfo1 + "/" + otherServInfo.otherserviceinfo2 + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //CDC
        ////        #region Line 19     Pending
        ////        FWBStr += "CDC/";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //REF
        ////        #region Line 20
        ////        FWBStr += "REF/" + fwbData.senderairport + "" + fwbData.senderofficedesignator + "" + fwbData.sendercompanydesignator + "/" + fwbData.senderFileref + "/" +
        ////            fwbData.senderParticipentIdentifier + "/" + fwbData.senderParticipentCode + "/" + fwbData.senderPariticipentAirport + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //COR
        ////        #region Line 21
        ////        FWBStr += "COR/" + fwbData.customorigincode + "";
        ////        #endregion

        ////        //COI
        ////        #region Line 22
        ////        FWBStr += "COI/" + fwbData.commisioncassindicator + "/" + fwbData.commisionCassSettleAmt + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //SII
        ////        #region Line 23
        ////        FWBStr += "SII/" + fwbData.saleschargeamt + "/" + fwbData.salescassindicator + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //ARD
        ////        #region Line 24
        ////        FWBStr += "ARD/" + fwbData.agentfileref + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //SPH
        ////        #region Line 25     Pending
        ////        FWBStr += "SPH/";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //NOM
        ////        #region Line 26     Pending
        ////        FWBStr += "NOM/";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //SRI
        ////        #region Line 27     Pending
        ////        FWBStr += "SRI/";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //OPI
        ////        #region Line 28
        ////        FWBStr += "OPI/" + fwbData.othparticipentname + "/" + fwbData.othparticipentairport + "/" +
        ////            fwbData.othofficedesignator + "" + fwbData.othcompanydesignator + "/" + fwbData.othfilereference + "/" +
        ////            fwbData.othparticipentidentifier + "/" + fwbData.othparticipentcode + "/" + fwbData.othparticipentairport + "";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////        //OCI
        ////        #region Line 29     Pending
        ////        FWBStr += "OCI/";
        ////        FWBStr += "\r\n";
        ////        #endregion

        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        FWBStr = "ERR";
        ////    }
        ////    return FWBStr;
        ////}
        //#endregion

        //#region Decode FWB Message
        //public static bool decodeReceiveFWB(string fwbmsg, ref MessageData.fwbinfo fwbdata, ref MessageData.FltRoute[] fltroute, ref MessageData.othercharges[] fwbOtherCharge, ref MessageData.otherserviceinfo[] othinfoarray, ref MessageData.RateDescription[] fwbrate, ref MessageData.customsextrainfo[] custominfo, ref MessageData.dimensionnfo[] objDimension, ref MessageData.AWBBuildBUP[] objAwbBup)
        //{
        //    bool flag = false;
        //    MessageData.AWBBuildBUP awbBup = new MessageData.AWBBuildBUP("");
        //    try
        //    {
        //        string lastrec = "NA";
        //        int line = 0;//, consignmnetnum = 0; 
        //        //int count=0;
        //        try
        //        {
        //            if (fwbmsg.StartsWith("FWB", StringComparison.OrdinalIgnoreCase))
        //            {
        //                // ffrmsg = ffrmsg.Replace("\r\n","$");
        //                string[] str = fwbmsg.Split('$');
        //                if (str.Length > 3)
        //                {
        //                    for (int i = 0; i < str.Length; i++)
        //                    {

        //                        flag = true;

        //                        #region Line 1
        //                        if (str[i].StartsWith("FWB", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            string[] msg = str[i].Split('/');
        //                            fwbdata.fwbversionnum = msg[1];
        //                        }
        //                        #endregion

        //                        #region Line 2 awb consigment details
        //                        if (i == 1)
        //                        {
        //                            try
        //                            {
        //                                lastrec = "AWB";
        //                                line = 0;
        //                                string[] msg = str[i].Split('/');
        //                                //0th element
        //                                string[] decmes = msg[0].Split('-');
        //                                fwbdata.airlineprefix = decmes[0];
        //                                fwbdata.awbnum = decmes[1].Substring(0, decmes[1].Length - 6);
        //                                fwbdata.origin = decmes[1].Substring(decmes[1].Length - 6, 3);
        //                                fwbdata.dest = decmes[1].Substring(decmes[1].Length - 3, 3);
        //                                //1
        //                                if (msg[1].Length > 0)
        //                                {
        //                                    int k = 0;
        //                                    char lastchr = 'A';
        //                                    char[] arr = msg[1].ToCharArray();
        //                                    string[] strarr = new string[arr.Length];
        //                                    for (int j = 0; j < arr.Length; j++)
        //                                    {
        //                                        if ((char.IsNumber(arr[j])) || (arr[j].Equals('.')))
        //                                        {//number                            
        //                                            if (lastchr == 'N')
        //                                                k--;
        //                                            strarr[k] = strarr[k] + arr[j].ToString();
        //                                            lastchr = 'N';
        //                                        }
        //                                        if (char.IsLetter(arr[j]))
        //                                        {//letter
        //                                            if (lastchr == 'L')
        //                                                k--;
        //                                            strarr[k] = strarr[k] + arr[j].ToString();
        //                                            lastchr = 'L';
        //                                        }
        //                                        k++;
        //                                    }
        //                                    fwbdata.consigntype = strarr[0];
        //                                    fwbdata.pcscnt = strarr[1];//int.Parse(strarr[1]);
        //                                    fwbdata.weightcode = strarr[2];
        //                                    fwbdata.weight = strarr[3];//float.Parse(strarr[3]);
        //                                    for (k = 4; k < strarr.Length; k += 2)
        //                                    {
        //                                        if (strarr[k] != null)
        //                                        {
        //                                            if (strarr[k] == "DG")
        //                                            {
        //                                                fwbdata.densityindicator = strarr[k];
        //                                                fwbdata.densitygrp = strarr[k];
        //                                            }
        //                                            else//if (strarr[k + 1].Length > 3)
        //                                            {
        //                                                fwbdata.volumecode = strarr[k];
        //                                                fwbdata.volumeamt = strarr[k + 1];
        //                                            }
        //                                        }
        //                                    }
        //                                }


        //                            }
        //                            catch (Exception e)
        //                            {
        //                                continue;
        //                            }
        //                        }
        //                        #endregion

        //                        #region Line 3 Flight Booking
        //                        //Added By :Badiuz khan
        //                        //Added On:2015-12-17
        //                        //Description: Correct FLT Tag of  FWB For Saving Record
        //                        if (str[i].StartsWith("FLT", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                if (msg.Length > 1)
        //                                {
        //                                    for (int k = 0; k < msg.Length; k++)
        //                                    {
        //                                        if (msg[k].ToUpper() != "FLT")
        //                                        {
        //                                            if (msg[k].All(char.IsNumber))
        //                                            {
        //                                                if (fwbdata.fltday == "")
        //                                                    fwbdata.fltday = msg[k];
        //                                                else
        //                                                    fwbdata.fltday += "," + msg[k];
        //                                            }
        //                                            else
        //                                            {
        //                                                if (fwbdata.fltnum == "")
        //                                                    fwbdata.fltnum = msg[k];
        //                                                else
        //                                                    fwbdata.fltnum += "," + msg[k];
        //                                            }
        //                                        }
        //                                    }
        //                                    //    fwbdata.carriercode = msg[1].Substring(0, 2);
        //                                    //fwbdata.fltnum = msg[1].Substring(2);
        //                                    //strFightNo = msg[1].Substring(2);
        //                                    //fwbdata.fltday = msg[2];
        //                                    //if (msg.Length > 2)
        //                                    //{
        //                                    //    fwbdata.carriercode = fwbdata.carriercode + "," + msg[3].Substring(0, 2);
        //                                    //    fwbdata.fltnum = fwbdata.fltnum + "," + msg[3].Substring(2);
        //                                    //    fwbdata.fltday = fwbdata.fltday + "," + msg[4];
        //                                    //}
        //                                }
        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 4 Routing
        //                        //Added By :Badiuz khan
        //                        //Added On:2015-10-14
        //                        //Description: Add Rouute Tag of FWB For Saving Record
        //                        if (str[i].StartsWith("RTG", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {

        //                                string[] msg = str[i].Split('/');
        //                                MessageData.FltRoute flight = new MessageData.FltRoute("");
        //                                if (msg.Length >= 2)
        //                                {
        //                                    for (int k = 1; k < msg.Length; k++)
        //                                    {
        //                                        if (fwbdata.fltnum != "")
        //                                        {
        //                                            if (fwbdata.fltnum.Contains(','))
        //                                            {
        //                                                string[] strFight = fwbdata.fltnum.Split(',');
        //                                                if (k == 1)
        //                                                {
        //                                                    flight.carriercode = strFight[0].Substring(0, 2);
        //                                                    flight.fltnum = strFight[0].Substring(2);
        //                                                }
        //                                                else
        //                                                {
        //                                                    flight.carriercode = strFight[k - 1].Substring(0, 2);
        //                                                    flight.fltnum = strFight[k - 1].Substring(2);
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                flight.carriercode = fwbdata.fltnum.Substring(0, 2);
        //                                                flight.fltnum = fwbdata.fltnum.Substring(2);
        //                                            }
        //                                        }
        //                                        if (fwbdata.fltday != "")
        //                                        {
        //                                            if (fwbdata.fltday.Contains(','))
        //                                            {
        //                                                string[] strFightDay = fwbdata.fltday.Split(',');
        //                                                if (k == 1)
        //                                                {
        //                                                    flight.date = strFightDay[0];
        //                                                    flight.month = DateTime.Now.Month.ToString();
        //                                                }
        //                                                else
        //                                                {
        //                                                    flight.date = strFightDay[k - 1];
        //                                                    flight.month = DateTime.Now.Month.ToString();
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                flight.date = fwbdata.fltday;
        //                                                flight.month = DateTime.Now.Month.ToString();
        //                                            }

        //                                        }


        //                                        if (k == 1)
        //                                            flight.fltdept = fwbdata.origin;
        //                                        else
        //                                            flight.fltdept = msg[k - 1].Substring(0, 3);

        //                                        flight.fltarrival = msg[k].Substring(0, 3);

        //                                        //  flight.date = DateTime.Now.Day.ToString();

        //                                        Array.Resize(ref fltroute, fltroute.Length + 1);
        //                                        fltroute[fltroute.Length - 1] = flight;
        //                                    }
        //                                }
        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 5 Shipper Infor
        //                        if (str[i].StartsWith("SHP", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                lastrec = msg[0];
        //                                line = 0;
        //                                if (msg.Length > 1)
        //                                {
        //                                    fwbdata.shipperaccnum = msg[1];

        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 6 Consignee
        //                        if (str[i].StartsWith("CNE", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                lastrec = msg[0];
        //                                line = 0;
        //                                if (msg.Length > 1)
        //                                {
        //                                    fwbdata.consaccnum = msg[1];
        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 7 Agent
        //                        if (str[i].StartsWith("AGT", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                lastrec = msg[0];
        //                                line = 0;
        //                                if (msg.Length > 1)
        //                                {
        //                                    fwbdata.agentaccnum = msg[1];
        //                                    fwbdata.agentIATAnumber = msg[2].Length > 0 ? msg[2] : "";
        //                                    if (msg.Length > 2)
        //                                    {
        //                                        fwbdata.agentCASSaddress = msg[3].Length > 0 ? msg[3] : "";
        //                                    }
        //                                    if (msg.Length > 3)
        //                                    {
        //                                        fwbdata.agentParticipentIdentifier = msg[4].Length > 0 ? msg[4] : "";
        //                                    }
        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 8 Special Service request
        //                        if (str[i].StartsWith("SSR", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                lastrec = msg[0];
        //                                line = 0;
        //                                if (msg[1].Length > 0)
        //                                {
        //                                    fwbdata.specialservicereq1 = msg[1];
        //                                }

        //                            }
        //                            catch (Exception ex)
        //                            { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 9 Notify
        //                        if (str[i].StartsWith("NFY", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                lastrec = msg[0];
        //                                line = 0;
        //                                if (msg.Length > 0)
        //                                {
        //                                    fwbdata.notifyname = msg[1].Length > 0 ? msg[1] : "";
        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 10 Accounting Information
        //                        if (str[i].StartsWith("ACC", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            string[] msg = str[i].Split('/');
        //                            lastrec = msg[0];
        //                            line = 0;
        //                            if (msg.Length > 1)
        //                            {
        //                                fwbdata.accountinginfoidentifier = fwbdata.accountinginfoidentifier + msg[1] + ",";
        //                                fwbdata.accountinginfo = fwbdata.accountinginfo + msg[2] + ",";
        //                            }
        //                        }
        //                        #endregion

        //                        #region Line 11 Charge declaration
        //                        if (str[i].StartsWith("CVD", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                if (msg.Length > 1)
        //                                {
        //                                    fwbdata.currency = msg[1];
        //                                    fwbdata.chargecode = msg[2].Length > 0 ? msg[2] : "";
        //                                    fwbdata.chargedec = msg[3].Length > 0 ? msg[3] : "";
        //                                    fwbdata.declaredvalue = msg[4];
        //                                    fwbdata.declaredcustomvalue = msg[5];
        //                                    fwbdata.insuranceamount = msg[6];
        //                                }
        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 12 Rate Description
        //                        if (str[i].StartsWith("RTD", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            string[] msg = str[i].Split('/');
        //                            if (msg.Length > 1)
        //                            {
        //                                lastrec = msg[0];
        //                                line = 0;
        //                                MessageData.RateDescription rate = new MessageData.RateDescription("");
        //                                try
        //                                {
        //                                    rate.linenum = msg[1];
        //                                    for (int k = 2; k < msg.Length; k++)
        //                                    {
        //                                        if (msg[k].Substring(0, 1).Equals("P", StringComparison.OrdinalIgnoreCase))
        //                                        {
        //                                            rate.pcsidentifier = msg[k].Substring(0, 1);
        //                                            rate.numofpcs = msg[k].Substring(1);
        //                                        }
        //                                        if (msg[k].Substring(0, 1).Equals("K", StringComparison.OrdinalIgnoreCase))
        //                                        {
        //                                            rate.weightindicator = msg[k].Substring(0, 1);
        //                                            rate.weight = msg[k].Substring(1).Length > 0 ? msg[k].Substring(1) : "0";
        //                                        }
        //                                        if (msg[k].Substring(0, 1).Equals("C", StringComparison.OrdinalIgnoreCase))
        //                                        {
        //                                            rate.rateclasscode = msg[k].Substring(1);
        //                                        }
        //                                        if (msg[k].Substring(0, 1).Equals("S", StringComparison.OrdinalIgnoreCase))
        //                                        {
        //                                            rate.commoditynumber = msg[k].Substring(1);
        //                                        }
        //                                        if (msg[k].Substring(0, 1).Equals("W", StringComparison.OrdinalIgnoreCase))
        //                                        {
        //                                            rate.awbweight = msg[k].Substring(1);
        //                                        }
        //                                        if (msg[k].Substring(0, 1).Equals("R", StringComparison.OrdinalIgnoreCase))
        //                                        {
        //                                            rate.chargerate = msg[k].Substring(1);
        //                                        }
        //                                        if (msg[k].Substring(0, 1).Equals("T", StringComparison.OrdinalIgnoreCase))
        //                                        {
        //                                            rate.chargeamt = msg[k].Substring(1);
        //                                        }
        //                                    }
        //                                    Array.Resize(ref fwbrate, fwbrate.Length + 1);
        //                                    fwbrate[fwbrate.Length - 1] = rate;
        //                                }
        //                                catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                            }
        //                        }
        //                        #endregion

        //                        #region Line 13 Other Charges
        //                        if (str[i].StartsWith("OTH", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                lastrec = "OTH";
        //                                string[] msg = str[i].Split('/');
        //                                if (msg.Length > 1)
        //                                {
        //                                    string[] opstr = stringsplitter(msg[2]);
        //                                    for (int k = 0; k < opstr.Length; k = k + 2)
        //                                    {
        //                                        if (opstr[k].Length > 0)
        //                                        {
        //                                            MessageData.othercharges oth = new MessageData.othercharges("");
        //                                            oth.otherchargecode = opstr[k].Substring(0, 2);
        //                                            oth.entitlementcode = opstr[k].Substring(2);
        //                                            oth.chargeamt = opstr[k + 1];
        //                                            Array.Resize(ref fwbOtherCharge, fwbOtherCharge.Length + 1);
        //                                            fwbOtherCharge[fwbOtherCharge.Length - 1] = oth;
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 14 Prepaid Charge Summery
        //                        if (str[i].StartsWith("PPD", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                lastrec = "PPD";
        //                                string[] msg = str[i].Split('/');
        //                                for (int k = 1; k < msg.Length; k++)
        //                                {
        //                                    if (msg[k].Substring(0, 2).Equals("WT", StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        fwbdata.PPweightCharge = msg[k].Substring(2);
        //                                    }
        //                                    if (msg[k].Substring(0, 2).Equals("VC", StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        fwbdata.PPValuationCharge = msg[k].Substring(2);
        //                                    }
        //                                    if (msg[k].Substring(0, 2).Equals("TX", StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        fwbdata.PPTaxesCharge = msg[k].Substring(2);
        //                                    }
        //                                    if (msg[k].Substring(0, 2).Equals("OA", StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        fwbdata.PPOCDA = msg[k].Substring(2);
        //                                    }
        //                                    if (msg[k].Substring(0, 2).Equals("OC", StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        fwbdata.PPOCDC = msg[k].Substring(2);
        //                                    }
        //                                    if (msg[k].Substring(0, 2).Equals("CT", StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        fwbdata.PPTotalCharges = msg[k].Substring(2);
        //                                    }
        //                                }
        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 15 Collect Charge Summery
        //                        if (str[i].StartsWith("COL", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                lastrec = "COL";
        //                                string[] msg = str[i].Split('/');
        //                                for (int k = 1; k < msg.Length; k++)
        //                                {
        //                                    if (msg[k].Substring(0, 2).Equals("WT", StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        fwbdata.CCweightCharge = msg[k].Substring(2);
        //                                    }
        //                                    if (msg[k].Substring(0, 2).Equals("VC", StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        fwbdata.CCValuationCharge = msg[k].Substring(2);
        //                                    }
        //                                    if (msg[k].Substring(0, 2).Equals("TX", StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        fwbdata.CCTaxesCharge = msg[k].Substring(2);
        //                                    }
        //                                    if (msg[k].Substring(0, 2).Equals("OA", StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        fwbdata.CCOCDA = msg[k].Substring(2);
        //                                    }
        //                                    if (msg[k].Substring(0, 2).Equals("OC", StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        fwbdata.CCOCDC = msg[k].Substring(2);
        //                                    }
        //                                    if (msg[k].Substring(0, 2).Equals("CT", StringComparison.OrdinalIgnoreCase))
        //                                    {
        //                                        fwbdata.CCTotalCharges = msg[k].Substring(2);
        //                                    }
        //                                }
        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 16 Shipper Certification
        //                        if (str[i].StartsWith("CER", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            string[] msg = str[i].Split('/');
        //                            if (msg[1].Length > 0)
        //                            {
        //                                fwbdata.shippersignature = msg[1];
        //                            }
        //                        }
        //                        #endregion

        //                        #region Line 17 Carrier Execution
        //                        if (str[i].StartsWith("ISU", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            string[] msg = str[i].Split('/');
        //                            try
        //                            {
        //                                if (msg.Length > 0)
        //                                {

        //                                    fwbdata.carrierdate = msg[1].Substring(0, 2);
        //                                    fwbdata.carriermonth = msg[1].Substring(2, 3);
        //                                    fwbdata.carrieryear = msg[1].Substring(5);
        //                                    fwbdata.carrierplace = msg[2];
        //                                    fwbdata.carriersignature = msg[3];
        //                                }
        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 18 Other service info
        //                        if (str[i].StartsWith("OSI", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                lastrec = msg[0];
        //                                line = 0;
        //                                if (msg[1].Length > 0)
        //                                {
        //                                    Array.Resize(ref othinfoarray, othinfoarray.Length + 1);
        //                                    othinfoarray[othinfoarray.Length - 1].otherserviceinfo1 = msg[1];

        //                                }

        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 19 Charge in destination currency
        //                        if (str[i].StartsWith("CDC", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                if (msg.Length > 0)
        //                                {
        //                                    fwbdata.cccurrencycode = msg[1].Substring(0, 3);
        //                                    fwbdata.ccexchangerate = msg[1].Substring(3);
        //                                    for (int j = 2; j < msg.Length; j++)
        //                                        fwbdata.ccchargeamt += msg[j] + ",";
        //                                }
        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 20 Sender Reference
        //                        if (str[i].StartsWith("REF", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                if (msg.Length > 0)
        //                                {

        //                                    if (msg[1].Length > 1)
        //                                    {
        //                                        try
        //                                        {
        //                                            fwbdata.senderairport = msg[1].Substring(0, 3);
        //                                            fwbdata.senderofficedesignator = msg[1].Substring(3, 2);
        //                                            fwbdata.sendercompanydesignator = msg[1].Substring(5);
        //                                        }
        //                                        catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                                    }
        //                                    fwbdata.senderFileref = msg[2];
        //                                    fwbdata.senderParticipentIdentifier = msg[3];
        //                                    fwbdata.senderParticipentCode = msg[4];
        //                                    fwbdata.senderPariticipentAirport = msg[5];
        //                                }

        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 21 Custom Origin
        //                        if (str[i].StartsWith("COR", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                if (msg[1].Length > 0)
        //                                {
        //                                    fwbdata.customorigincode = msg[1];
        //                                }

        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 22 Commission Information
        //                        if (str[i].StartsWith("COI", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                if (msg.Length > 0)
        //                                {
        //                                    fwbdata.commisioncassindicator = msg[1];
        //                                    for (int k = 2; k < msg.Length; k++)
        //                                        fwbdata.commisionCassSettleAmt += msg[k] + ",";
        //                                }

        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 23 Sales Incentive Info
        //                        if (str[i].StartsWith("SII", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                if (msg[1].Length > 0)
        //                                {
        //                                    fwbdata.saleschargeamt = msg[1];
        //                                    fwbdata.salescassindicator = msg[2];
        //                                }

        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 24 Agent Reference
        //                        if (str[i].StartsWith("ARD", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                if (msg[1].Length > 0)
        //                                {
        //                                    fwbdata.agentfileref = msg[1];
        //                                }

        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 25 Special Handling
        //                        if (str[i].StartsWith("SPH", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                if (msg[1].Length > 0)
        //                                {
        //                                    string temp = str[i].Replace("/", ",");
        //                                    fwbdata.splhandling = temp.Replace("SPH", "");
        //                                }

        //                            }
        //                            catch (Exception ex)
        //                            { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 26 Nominated Handling Party
        //                        if (str[i].StartsWith("NOM", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                if (msg.Length > 0)
        //                                {
        //                                    fwbdata.handlingname = msg[1];
        //                                    fwbdata.handlingplace = msg[2];
        //                                }

        //                            }
        //                            catch (Exception ex)
        //                            { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 27 Shipment Reference Info
        //                        if (str[i].StartsWith("SRI", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                if (msg.Length > 0)
        //                                {
        //                                    fwbdata.shiprefnum = msg[1];
        //                                    fwbdata.supplemetryshipperinfo1 = msg[2];
        //                                    fwbdata.supplemetryshipperinfo2 = msg[3];
        //                                }
        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 28 Other Service Information
        //                        if (str[i].StartsWith("OPI", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                if (msg.Length > 0)
        //                                {
        //                                    lastrec = msg[0];
        //                                    fwbdata.othparticipentname = msg[1];
        //                                }

        //                            }
        //                            catch (Exception ex)
        //                            { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 29 custom extra info
        //                        if (str[i].StartsWith("OCI", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            string[] msg = str[i].Split('/');
        //                            if (msg.Length > 0)
        //                            {
        //                                lastrec = "OCI";
        //                                MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
        //                                custom.IsoCountryCodeOci = msg[1];
        //                                custom.InformationIdentifierOci = msg[2];
        //                                custom.CsrIdentifierOci = msg[3];
        //                                custom.SupplementaryCsrIdentifierOci = msg[4];
        //                                Array.Resize(ref custominfo, custominfo.Length + 1);
        //                                custominfo[custominfo.Length - 1] = custom;
        //                            }
        //                        }
        //                        #endregion

        //                        #region Second Line
        //                        if (str[i].StartsWith("/"))
        //                        {
        //                            string[] msg = str[i].Split('/');
        //                            try
        //                            {
        //                                #region SHP Data
        //                                if (lastrec == "SHP")
        //                                {
        //                                    line++;
        //                                    if (line == 1)
        //                                    {
        //                                        fwbdata.shippername = msg[1].Length > 0 ? msg[1] : "";
        //                                    }
        //                                    if (line == 2)
        //                                    {
        //                                        fwbdata.shipperadd = msg[1].Length > 0 ? msg[1] : "";

        //                                    }
        //                                    if (line == 3)
        //                                    {
        //                                        fwbdata.shipperplace = msg[1].Length > 0 ? msg[1] : "";
        //                                        fwbdata.shipperstate = msg[2].Length > 0 ? msg[2] : "";
        //                                    }
        //                                    if (line == 4)
        //                                    {
        //                                        fwbdata.shippercountrycode = msg[1].Length > 0 ? msg[1] : "";
        //                                        fwbdata.shipperpostcode = msg[2].Length > 0 ? msg[2] : "";
        //                                        fwbdata.shippercontactidentifier = msg[3].Length > 0 ? msg[3] : "";
        //                                        fwbdata.shippercontactnum = msg[4].Length > 0 ? msg[4] : "";

        //                                    }

        //                                }
        //                                #endregion

        //                                #region CNE Data
        //                                if (lastrec == "CNE")
        //                                {
        //                                    line++;
        //                                    if (line == 1)
        //                                    {
        //                                        fwbdata.consname = msg[1].Length > 0 ? msg[1] : "";
        //                                    }
        //                                    if (line == 2)
        //                                    {
        //                                        fwbdata.consadd = msg[1].Length > 0 ? msg[1] : "";
        //                                    }
        //                                    if (line == 3)
        //                                    {
        //                                        fwbdata.consplace = msg[1].Length > 0 ? msg[1] : "";
        //                                        fwbdata.consstate = msg[2].Length > 0 ? msg[2] : "";
        //                                    }
        //                                    if (line == 4)
        //                                    {
        //                                        fwbdata.conscountrycode = msg[1].Length > 0 ? msg[1] : "";
        //                                        fwbdata.conspostcode = msg[2].Length > 0 ? msg[2] : "";
        //                                        fwbdata.conscontactidentifier = msg[3].Length > 0 ? msg[3] : "";
        //                                        fwbdata.conscontactnum = msg[4].Length > 0 ? msg[4] : "";
        //                                    }

        //                                }
        //                                #endregion

        //                                #region AgentData
        //                                if (lastrec == "AGT")
        //                                {
        //                                    line++;
        //                                    if (line == 1)
        //                                    {
        //                                        fwbdata.agentname = msg[1].Length > 0 ? msg[1] : "";
        //                                    }
        //                                    if (line == 2)
        //                                    {
        //                                        fwbdata.agentplace = msg[1].Length > 0 ? msg[1] : "";
        //                                    }
        //                                }
        //                                #endregion

        //                                #region SSR 2
        //                                if (lastrec == "SSR")
        //                                {
        //                                    fwbdata.specialservicereq2 = msg[1].Length > 0 ? msg[1] : "";
        //                                    lastrec = "NA";
        //                                }
        //                                #endregion

        //                                #region Also notify Data
        //                                if (lastrec == "NFY")
        //                                {
        //                                    line++;
        //                                    if (line == 1)
        //                                    {
        //                                        fwbdata.notifyadd = msg[1].Length > 0 ? msg[1] : "";
        //                                    }
        //                                    if (line == 2)
        //                                    {
        //                                        fwbdata.notifyplace = msg[1].Length > 0 ? msg[1] : "";
        //                                        fwbdata.notifystate = msg[2].Length > 0 ? msg[2] : "";
        //                                    }
        //                                    if (line == 3)
        //                                    {
        //                                        fwbdata.notifycountrycode = msg[1].Length > 0 ? msg[1] : "";
        //                                        fwbdata.notifypostcode = msg[2].Length > 0 ? msg[2] : "";
        //                                        fwbdata.notifycontactidentifier = msg[3].Length > 0 ? msg[3] : "";
        //                                        fwbdata.notifycontactnum = msg[4].Length > 0 ? msg[4] : "";
        //                                    }
        //                                }
        //                                #endregion

        //                                #region Account Info
        //                                if (lastrec.Equals("ACC", StringComparison.OrdinalIgnoreCase))
        //                                {
        //                                    if (msg.Length > 1)
        //                                    {
        //                                        fwbdata.accountinginfoidentifier = fwbdata.accountinginfoidentifier + msg[1] + ",";
        //                                        fwbdata.accountinginfo = fwbdata.accountinginfo + msg[2] + ",";
        //                                    }
        //                                }
        //                                #endregion

        //                                #region RateData
        //                                if (lastrec.Equals("RTD", StringComparison.OrdinalIgnoreCase))
        //                                {
        //                                    try
        //                                    {
        //                                        if (msg.Length > 1)
        //                                        {
        //                                            int res, k = 1;
        //                                            if (int.TryParse(msg[k].ToString(), out res))
        //                                            {
        //                                                k++;
        //                                            }
        //                                            if (msg[k].Equals("NG", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbrate[fwbrate.Length - 1].goodsnature = msg[k + 1];
        //                                            }
        //                                            if (msg[k].Equals("NC", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbrate[fwbrate.Length - 1].goodsnature1 = msg[k + 1];
        //                                            }
        //                                            if (msg[k].Equals("ND", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
        //                                                if (msg.Length > 1)
        //                                                {
        //                                                    if (msg[k + 1].Substring(0, 1).Equals("K", StringComparison.OrdinalIgnoreCase) || msg[k + 1].Substring(0, 1).Equals("L", StringComparison.OrdinalIgnoreCase))
        //                                                    {
        //                                                        dimension.weight = msg[k + 1].Substring(1);
        //                                                        k++;
        //                                                    }
        //                                                    if (msg[k + 1].Contains('-'))
        //                                                    {
        //                                                        string[] substr = msg[k + 1].Split('-');
        //                                                        try
        //                                                        {
        //                                                            if (substr.Length > 0)
        //                                                            {
        //                                                                dimension.mesurunitcode = substr[0].Substring(0, 3);
        //                                                                dimension.length = substr[0].Substring(3);
        //                                                                dimension.width = substr[1];
        //                                                                dimension.height = substr[2];
        //                                                                k++;
        //                                                            }
        //                                                        }
        //                                                        catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                                                    }
        //                                                    int val;
        //                                                    if (int.TryParse(msg[k + 1], out val))
        //                                                    {
        //                                                        dimension.piecenum = msg[k + 1];
        //                                                    }
        //                                                    Array.Resize(ref objDimension, objDimension.Length + 1);
        //                                                    objDimension[objDimension.Length - 1] = dimension;
        //                                                }

        //                                            }
        //                                            if (msg[k].Equals("NV", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbrate[fwbrate.Length - 1].volcode = msg[k + 1].Substring(0, 2);
        //                                                fwbrate[fwbrate.Length - 1].volamt = msg[k + 1].Substring(2);
        //                                            }
        //                                            if (msg[k].Equals("NU", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                //  awbBup.UldType = msg[k + 1].Substring(0, 3);
        //                                                awbBup.ULDNo = msg[k + 1].ToString();
        //                                                //awbBup.ULDOwnerCode = msg[k + 1].Substring(msg[k + 1].Length - 2);
        //                                            }
        //                                            if (msg[k].Equals("NS", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                awbBup.SlacCount = msg[k + 1];
        //                                                Array.Resize(ref objAwbBup, objAwbBup.Length + 1);
        //                                                objAwbBup[objAwbBup.Length - 1] = awbBup;
        //                                                awbBup = new MessageData.AWBBuildBUP("");

        //                                            }
        //                                            if (msg[k].Equals("NH", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbrate[fwbrate.Length - 1].hermonisedcomoditycode = msg[k + 1];
        //                                            }
        //                                            if (msg[k].Equals("NO", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbrate[fwbrate.Length - 1].isocountrycode = msg[k + 1];
        //                                            }
        //                                        }
        //                                    }
        //                                    catch (Exception ex)
        //                                    { clsLog.WriteLogAzure(ex.Message); }
        //                                }
        //                                #endregion

        //                                #region Other Charges
        //                                if (lastrec.Equals("OTH", StringComparison.OrdinalIgnoreCase))
        //                                {
        //                                    try
        //                                    {
        //                                        string[] opstr = stringsplitter(msg[2]);
        //                                        for (int k = 0; k < opstr.Length; k = k + 2)
        //                                        {
        //                                            MessageData.othercharges oth = new MessageData.othercharges("");
        //                                            oth.otherchargecode = opstr[k].Substring(0, 2);
        //                                            oth.entitlementcode = opstr[k].Substring(2);
        //                                            oth.chargeamt = opstr[k + 1];
        //                                            Array.Resize(ref fwbOtherCharge, fwbOtherCharge.Length + 1);
        //                                            fwbOtherCharge[fwbOtherCharge.Length - 1] = oth;
        //                                        }
        //                                    }
        //                                    catch (Exception ex)
        //                                    { clsLog.WriteLogAzure(ex.Message); }
        //                                }
        //                                #endregion

        //                                #region Line 14 Collect Charge Summery
        //                                if (lastrec.Equals("PPD", StringComparison.OrdinalIgnoreCase))
        //                                {
        //                                    try
        //                                    {
        //                                        for (int k = 1; k < msg.Length; k++)
        //                                        {
        //                                            if (msg[k].Substring(0, 2).Equals("WT", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbdata.PPweightCharge = msg[k].Substring(2);
        //                                            }
        //                                            if (msg[k].Substring(0, 2).Equals("VC", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbdata.PPValuationCharge = msg[k].Substring(2);
        //                                            }
        //                                            if (msg[k].Substring(0, 2).Equals("TX", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbdata.PPTaxesCharge = msg[k].Substring(2);
        //                                            }
        //                                            if (msg[k].Substring(0, 2).Equals("OA", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbdata.PPOCDA = msg[k].Substring(2);
        //                                            }
        //                                            if (msg[k].Substring(0, 2).Equals("OC", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbdata.PPOCDC = msg[k].Substring(2);
        //                                            }
        //                                            if (msg[k].Substring(0, 2).Equals("CT", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbdata.PPTotalCharges = msg[k].Substring(2);
        //                                            }
        //                                        }
        //                                    }
        //                                    catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                                }
        //                                #endregion

        //                                #region Line 15 Prepaid Charge Summery
        //                                if (lastrec.Equals("COL", StringComparison.OrdinalIgnoreCase))
        //                                {
        //                                    try
        //                                    {
        //                                        for (int k = 1; k < msg.Length; k++)
        //                                        {
        //                                            if (msg[k].Substring(0, 2).Equals("WT", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbdata.CCweightCharge = msg[k].Substring(2);
        //                                            }
        //                                            if (msg[k].Substring(0, 2).Equals("VC", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbdata.CCValuationCharge = msg[k].Substring(2);
        //                                            }
        //                                            if (msg[k].Substring(0, 2).Equals("TX", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbdata.CCTaxesCharge = msg[k].Substring(2);
        //                                            }
        //                                            if (msg[k].Substring(0, 2).Equals("OA", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbdata.CCOCDA = msg[k].Substring(2);
        //                                            }
        //                                            if (msg[k].Substring(0, 2).Equals("OC", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbdata.CCOCDC = msg[k].Substring(2);
        //                                            }
        //                                            if (msg[k].Substring(0, 2).Equals("CT", StringComparison.OrdinalIgnoreCase))
        //                                            {
        //                                                fwbdata.CCTotalCharges = msg[k].Substring(2);
        //                                            }
        //                                        }
        //                                    }
        //                                    catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                                }
        //                                #endregion

        //                                #region Line 18 OSI 2
        //                                if (lastrec == "OSI")
        //                                {
        //                                    othinfoarray[othinfoarray.Length - 1].otherserviceinfo2 = msg[1].Length > 0 ? msg[1] : "";
        //                                    lastrec = "NA";
        //                                }
        //                                #endregion


        //                                #region OCI
        //                                if (lastrec.Equals("OCI", StringComparison.OrdinalIgnoreCase))
        //                                {
        //                                    string[] msgdata = str[i].Split('/');
        //                                    if (msgdata.Length > 0)
        //                                    {
        //                                        MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
        //                                        custom.IsoCountryCodeOci = msgdata[1];
        //                                        custom.InformationIdentifierOci = msgdata[2];
        //                                        custom.CsrIdentifierOci = msgdata[3];
        //                                        custom.SupplementaryCsrIdentifierOci = msgdata[4];
        //                                        Array.Resize(ref custominfo, custominfo.Length + 1);
        //                                        custominfo[custominfo.Length - 1] = custom;
        //                                    }
        //                                }
        //                                #endregion

        //                                #region OPI
        //                                if (lastrec.Equals("OPI", StringComparison.OrdinalIgnoreCase))
        //                                {
        //                                    string[] msgdata = str[i].Split('/');
        //                                    if (msgdata.Length > 0)
        //                                    {
        //                                        fwbdata.othairport = msgdata[1].Substring(0, 3);
        //                                        fwbdata.othofficedesignator = msgdata[1].Substring(3, 2);
        //                                        fwbdata.othcompanydesignator = msgdata[1].Substring(5);
        //                                        fwbdata.othfilereference = msgdata[2];
        //                                        fwbdata.othparticipentidentifier = msgdata[3];
        //                                        fwbdata.othparticipentcode = msgdata[4];
        //                                        fwbdata.othparticipentairport = msgdata[5];
        //                                    }
        //                                }
        //                                #endregion
        //                            }
        //                            catch (Exception ex)
        //                            { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion
        //                    }
        //                }
        //            }
        //            flag = true;
        //        }
        //        catch (Exception ex)
        //        {
        //            flag = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        flag = false;
        //    }
        //    return flag;
        //}
        //#endregion

        //#region EncodeFWBForSend
        //public static string EncodeFWBForSend(ref MessageData.fwbinfo fwbdata, ref MessageData.othercharges[] fwbOtherCharge, ref MessageData.otherserviceinfo[] othinfoarray, ref MessageData.RateDescription[] fwbrate, ref MessageData.customsextrainfo[] custominfo)
        //{
        //    string fwbstr = null;
        //    try
        //    {
        //        //FWB
        //        #region Line 1
        //        string line1 = "FWB/" + fwbdata.fwbversionnum;
        //        #endregion

        //        #region Line 2
        //        string line2 = fwbdata.airlineprefix + "-" + fwbdata.awbnum + fwbdata.origin + fwbdata.dest + "/" + fwbdata.consigntype + fwbdata.pcscnt + fwbdata.weightcode + fwbdata.weight + fwbdata.volumecode + fwbdata.volumeamt + fwbdata.densityindicator + fwbdata.densitygrp;
        //        #endregion line 2
        //        //FLT
        //        #region Line 3
        //        string line3 = "";
        //        if (fwbdata.carriercode.Trim(',').Contains(','))
        //        {
        //            string[] carriersplit = fwbdata.carriercode.Split(',');
        //            string[] fltsplit = fwbdata.fltnum.Split(',');
        //            string[] daysplit = fwbdata.fltday.Split(',');
        //            for (int k = 0; k < carriersplit.Length; k++)
        //            {
        //                line3 = line3 + carriersplit[k] + fltsplit[k] + "/" + daysplit[k] + "/";
        //            }
        //        }
        //        else
        //        {
        //            line3 = fwbdata.carriercode.Trim(',') + fwbdata.fltnum.Trim(',') + "/" + fwbdata.fltday.Trim(',');
        //        }
        //        if (line3.Length > 1)
        //        {
        //            line3 = "FLT/" + line3.Trim('/');
        //        }
        //        #endregion

        //        //RTG
        //        #region Line 4
        //        string line4 = fwbdata.origin + fwbdata.carriercode;
        //        if (line4.Length > 1)
        //        {
        //            line4 = "RTG/" + line4;
        //        }
        //        #endregion

        //        //SHP
        //        #region Line 5
        //        string line5 = "";
        //        string str1 = "", str2 = "", str3 = "", str4 = "";
        //        try
        //        {
        //            if (fwbdata.shippername.Length > 0)
        //            {
        //                str1 = "/" + fwbdata.shippername;
        //            }
        //            if (fwbdata.shipperadd.Length > 0)
        //            {
        //                str2 = "/" + fwbdata.shipperadd;
        //            }

        //            if (fwbdata.shipperplace.Length > 0 || fwbdata.shipperstate.Length > 0)
        //            {
        //                str3 = "/" + fwbdata.shipperplace + "/" + fwbdata.shipperstate;
        //            }
        //            if (fwbdata.shippercountrycode.Length > 0 || fwbdata.shipperpostcode.Length > 0 || fwbdata.shippercontactidentifier.Length > 0 || fwbdata.shippercontactnum.Length > 0)
        //            {
        //                str4 = "/" + fwbdata.shippercountrycode + "/" + fwbdata.shipperpostcode + "/" + fwbdata.shippercontactidentifier + "/" + fwbdata.shippercontactnum;
        //            }

        //            if (fwbdata.shipperaccnum.Length > 0 || str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
        //            {
        //                line5 = "SHP/" + fwbdata.shipperaccnum;
        //                if (str4.Length > 0)
        //                {
        //                    line5 = line5.Trim('/') + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
        //                }
        //                else if (str3.Length > 0)
        //                {
        //                    line5 = line5.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
        //                }
        //                else if (str2.Length > 0)
        //                {
        //                    line5 = line5.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
        //                }
        //                else if (str1.Length > 0)
        //                {
        //                    line5 = line5.Trim() + "\r\n/" + str1.Trim('/');
        //                }
        //            }
        //        }
        //        catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //        #endregion

        //        //CNE
        //        #region Line 6
        //        string line6 = "";
        //        str1 = "";
        //        str2 = "";
        //        str3 = "";
        //        str4 = "";
        //        try
        //        {
        //            if (fwbdata.consname.Length > 0)
        //            {
        //                str1 = "/" + fwbdata.consname;
        //            }
        //            if (fwbdata.consadd.Length > 0)
        //            {
        //                str2 = "/" + fwbdata.consadd;
        //            }

        //            if (fwbdata.consplace.Length > 0 || fwbdata.consstate.Length > 0)
        //            {
        //                str3 = "/" + fwbdata.consplace + "/" + fwbdata.consstate;
        //            }
        //            if (fwbdata.conscountrycode.Length > 0 || fwbdata.conspostcode.Length > 0 || fwbdata.conscontactidentifier.Length > 0 || fwbdata.conscontactnum.Length > 0)
        //            {
        //                str4 = "/" + fwbdata.conscountrycode + "/" + fwbdata.conspostcode + "/" + fwbdata.conscontactidentifier + "/" + fwbdata.conscontactnum;
        //            }

        //            if (fwbdata.consaccnum.Length > 0 || str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
        //            {
        //                line6 = "CNE/" + fwbdata.consaccnum;
        //                if (str4.Length > 0)
        //                {
        //                    line6 = line6.Trim('/') + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
        //                }
        //                else if (str3.Length > 0)
        //                {
        //                    line6 = line6.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
        //                }
        //                else if (str2.Length > 0)
        //                {
        //                    line6 = line6.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
        //                }
        //                else if (str1.Length > 0)
        //                {
        //                    line6 = line6.Trim() + "\r\n/" + str1.Trim('/');
        //                }
        //            }
        //        }
        //        catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //        #endregion

        //        //AGT
        //        #region Line 7
        //        string line7 = "";
        //        str1 = "";
        //        str2 = "";
        //        try
        //        {
        //            if (fwbdata.agentname.Length > 0)
        //            {
        //                str1 = "/" + fwbdata.agentname;
        //            }
        //            if (fwbdata.agentplace.Length > 0)
        //            {
        //                str2 = "/" + fwbdata.agentplace;
        //            }
        //            if (fwbdata.agentaccnum.Length > 0 || fwbdata.agentIATAnumber.Length > 0 || fwbdata.agentCASSaddress.Length > 0 || fwbdata.agentParticipentIdentifier.Length > 0 || str1.Length > 0 || str2.Length > 0)
        //            {
        //                line7 = "AGT/" + fwbdata.agentaccnum + "/" + fwbdata.agentIATAnumber + "/" + fwbdata.agentCASSaddress + "/" + fwbdata.agentParticipentIdentifier;
        //                if (str2.Length > 0)
        //                {
        //                    line7 = line7.Trim('/') + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
        //                }
        //                else if (str1.Length > 0)
        //                {
        //                    line7 = line7.Trim('/') + "\r\n/" + str1.Trim('/');
        //                }
        //            }
        //        }
        //        catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //        #endregion

        //        //SSR
        //        #region Line 8
        //        string line8 = "";
        //        if (fwbdata.specialservicereq1.Length > 0 || fwbdata.specialservicereq2.Length > 0)
        //        {
        //            line8 = "SSR/" + fwbdata.specialservicereq1 + "$" + fwbdata.specialservicereq2;
        //        }
        //        line8 = line8.Trim('$');
        //        line8 = line8.Replace("$", "\r\n");
        //        #endregion

        //        //NFY
        //        #region Line 9
        //        string line9 = "";
        //        str1 = str2 = str3 = str4 = "";
        //        try
        //        {
        //            if (fwbdata.notifyadd.Length > 0)
        //            {
        //                str1 = "/" + fwbdata.notifyadd;
        //            }
        //            if (fwbdata.notifyplace.Length > 0 || fwbdata.notifystate.Length > 0)
        //            {
        //                str2 = "/" + fwbdata.notifyplace + "/" + fwbdata.notifystate;
        //            }
        //            if (fwbdata.notifycountrycode.Length > 0 || fwbdata.notifypostcode.Length > 0 || fwbdata.notifycontactidentifier.Length > 0 || fwbdata.notifycontactnum.Length > 0)
        //            {
        //                str3 = "/" + fwbdata.notifycountrycode + "/" + fwbdata.notifypostcode + "/" + fwbdata.notifycontactidentifier + "/" + fwbdata.notifycontactnum;
        //            }

        //            if (fwbdata.notifyname.Length > 0 || str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
        //            {
        //                line9 = "NFY/" + fwbdata.shipperaccnum;
        //                if (str3.Length > 0)
        //                {
        //                    line9 = line9.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
        //                }
        //                else if (str2.Length > 0)
        //                {
        //                    line9 = line9.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
        //                }
        //                else if (str1.Length > 0)
        //                {
        //                    line9 = line9.Trim() + "\r\n/" + str1.Trim('/');
        //                }
        //            }
        //        }
        //        catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //        #endregion

        //        //ACC
        //        #region Line 10
        //        string line10 = "";
        //        if (fwbdata.accountinginfoidentifier.Length > 0 || fwbdata.accountinginfo.Length > 0)
        //        {
        //            line10 = "ACC/" + fwbdata.accountinginfoidentifier + "/" + fwbdata.accountinginfo + "";
        //        }
        //        #endregion

        //        //CVD
        //        #region Line 11
        //        string line11 = "";
        //        line11 = "CVD/" + fwbdata.currency + "/" + fwbdata.chargecode + "/PP/" + fwbdata.declaredvalue + "/" + fwbdata.declaredcustomvalue + "/" + fwbdata.insuranceamount + "";
        //        #endregion

        //        //RTD
        //        #region Line 12
        //        string line12 = buildRateNode(ref fwbrate);
        //        if (line12 == null)
        //        {
        //            return null;
        //        }
        //        #endregion

        //        //OTH
        //        #region Line 13
        //        string line13 = "";
        //        for (int i = 0; i < fwbOtherCharge.Length; i++)
        //        {
        //            if (i > 0)
        //            {
        //                if (i % 3 == 0)
        //                {
        //                    if (i != fwbOtherCharge.Length)
        //                    {
        //                        line13 += "\r\n/" + fwbOtherCharge[0].indicator + "/";
        //                    }
        //                }
        //            }
        //            line13 += fwbOtherCharge[i].otherchargecode + "" + fwbOtherCharge[i].entitlementcode + "" + fwbOtherCharge[i].chargeamt;
        //            //if (i % 3 == 0)
        //            //{
        //            //    if(i != othData.Length)
        //            //    {
        //            //        FWBStr += "\r\nP";
        //            //    }
        //            //}
        //        }
        //        if (line13.Length > 1)
        //        {
        //            line13 = "OTH/P/" + line13;
        //        }
        //        #endregion

        //        //PPD
        //        #region Line 14
        //        string line14 = "", subline14 = "";

        //        if (fwbdata.PPweightCharge.Length > 0)
        //        {
        //            line14 = line14 + "/WT" + fwbdata.PPweightCharge;
        //        }
        //        if (fwbdata.PPValuationCharge.Length > 0)
        //        {
        //            line14 = line14 + "/VC" + fwbdata.PPValuationCharge;
        //        }
        //        if (fwbdata.PPTaxesCharge.Length > 0)
        //        {
        //            line14 = line14 + "/TX" + fwbdata.PPTaxesCharge;
        //        }
        //        if (fwbdata.PPOCDA.Length > 0)
        //        {
        //            subline14 = subline14 + "/OA" + fwbdata.PPOCDA;
        //        }
        //        if (fwbdata.PPOCDC.Length > 0)
        //        {
        //            subline14 = subline14 + "/OC" + fwbdata.PPOCDC;
        //        }
        //        if (fwbdata.PPTotalCharges.Length > 0)
        //        {
        //            subline14 = subline14 + "/CT" + fwbdata.PPTotalCharges;
        //        }
        //        if (line14.Length > 0 || subline14.Length > 0)
        //        {
        //            line14 = "PPD" + line14 + "$" + subline14;
        //        }
        //        line14 = line14.Trim('$');
        //        line14 = line14.Replace("$", "\r\n");
        //        #endregion

        //        //COL
        //        #region Line 15
        //        string line15 = "", subline15 = "";
        //        if (fwbdata.CCweightCharge.Length > 0)
        //        {
        //            line15 = line15 + "/WT" + fwbdata.CCweightCharge;
        //        }
        //        if (fwbdata.CCValuationCharge.Length > 0)
        //        {
        //            line15 = line15 + "/VC" + fwbdata.CCValuationCharge;
        //        }
        //        if (fwbdata.CCTaxesCharge.Length > 0)
        //        {
        //            line15 = line15 + "/TX" + fwbdata.CCTaxesCharge;
        //        }
        //        if (fwbdata.CCOCDA.Length > 0)
        //        {
        //            subline15 = subline15 + "/OA" + fwbdata.CCOCDA;
        //        }
        //        if (fwbdata.CCOCDC.Length > 0)
        //        {
        //            subline15 = subline15 + "/OC" + fwbdata.CCOCDC;
        //        }
        //        if (fwbdata.CCTotalCharges.Length > 0)
        //        {
        //            subline15 = subline15 + "/CT" + fwbdata.CCTotalCharges;
        //        }
        //        if (line15.Length > 0 || subline15.Length > 0)
        //        {
        //            line15 = "COL" + line15 + "$" + subline15;
        //        }
        //        line15 = line15.Trim('$');
        //        line15 = line15.Replace("$", "\r\n");
        //        #endregion

        //        //CER
        //        #region Line 16
        //        string line16 = "";
        //        if (fwbdata.shippersignature.Length > 0)
        //        {
        //            line16 = "CER/" + fwbdata.shippersignature;
        //        }
        //        #endregion

        //        //ISU
        //        #region Line 17
        //        string line17 = "";
        //        line17 = "ISU/" + fwbdata.carrierdate.PadLeft(2, '0') + fwbdata.carriermonth.PadLeft(2, '0') + fwbdata.carrieryear.PadLeft(2, '0') + "/" + fwbdata.carrierplace + "/" + fwbdata.carriersignature;
        //        #endregion

        //        //OSI
        //        #region Line 18
        //        string line18 = "";
        //        if (othinfoarray.Length > 0)
        //        {
        //            for (int i = 0; i < othinfoarray.Length; i++)
        //            {
        //                if (othinfoarray[i].otherserviceinfo1.Length > 0)
        //                {
        //                    line18 = "OSI/" + othinfoarray[i].otherserviceinfo1 + "$";
        //                    if (othinfoarray[i].otherserviceinfo2.Length > 0)
        //                    {
        //                        line18 = line18 + "/" + othinfoarray[i].otherserviceinfo2 + "$";
        //                    }
        //                }
        //            }
        //            line18 = line18.Trim('$');
        //            line18 = line18.Replace("$", "\r\n");
        //        }
        //        #endregion

        //        //CDC
        //        #region Line 19
        //        string line19 = "";
        //        if (fwbdata.cccurrencycode.Length > 0 || fwbdata.ccexchangerate.Length > 0 || fwbdata.ccchargeamt.Length > 0)
        //        {
        //            string[] exchnagesplit = fwbdata.ccexchangerate.Split(',');
        //            string[] chargesplit = fwbdata.ccchargeamt.Split(',');
        //            if (exchnagesplit.Length == chargesplit.Length)
        //            {
        //                for (int k = 0; k < exchnagesplit.Length; k++)
        //                {
        //                    line19 = line19 + exchnagesplit[k] + "/" + chargesplit[k] + "/";
        //                }
        //            }
        //            line19 = "CDC/" + fwbdata.cccurrencycode + line19.Trim('/');
        //        }
        //        #endregion

        //        //REF
        //        #region Line 20
        //        string line20 = "";
        //        line20 = fwbdata.senderairport + "" + fwbdata.senderofficedesignator + "" + fwbdata.sendercompanydesignator + "/" + fwbdata.senderFileref + "/" + fwbdata.senderParticipentIdentifier + "/" + fwbdata.senderParticipentCode + "/" + fwbdata.senderPariticipentAirport + "";
        //        //line20 = line20.Trim('/');
        //        if (line20.Length > 1)
        //        {
        //            line20 = "REF/" + line20;
        //        }

        //        #endregion

        //        //COR
        //        #region Line 21
        //        string line21 = "";
        //        if (fwbdata.customorigincode.Length > 0)
        //        {
        //            line21 = "COR/" + fwbdata.customorigincode + "";
        //        }
        //        #endregion

        //        //COI
        //        #region Line 22
        //        string line22 = "";
        //        if (fwbdata.commisioncassindicator.Length > 0 || fwbdata.commisionCassSettleAmt.Length > 0)
        //        {
        //            line22 = "COI/" + fwbdata.commisioncassindicator + "/" + fwbdata.commisionCassSettleAmt.Replace(',', '/') + "";
        //        }
        //        #endregion

        //        //SII
        //        #region Line 23
        //        string line23 = "";
        //        if (fwbdata.saleschargeamt.Length > 0 || fwbdata.salescassindicator.Length > 0)
        //        {
        //            line23 = "SII/" + fwbdata.saleschargeamt + "/" + fwbdata.salescassindicator + "";
        //        }
        //        #endregion

        //        //ARD
        //        #region Line 24
        //        string line24 = "";
        //        if (fwbdata.agentfileref.Length > 0)
        //        {
        //            line24 = "ARD/" + fwbdata.agentfileref + "";
        //        }
        //        #endregion

        //        //SPH
        //        #region Line 25
        //        string line25 = "";
        //        if (fwbdata.splhandling.Replace(",", "").Length > 0)
        //        {
        //            line25 = "SPH/" + fwbdata.splhandling.Replace(',', '/');
        //        }
        //        #endregion

        //        //NOM
        //        #region Line 26
        //        string line26 = "";
        //        if (fwbdata.handlingname.Length > 0 || fwbdata.handlingplace.Length > 0)
        //        {
        //            line26 = "NOM/" + fwbdata.handlingname + "/" + fwbdata.handlingplace;
        //        }
        //        #endregion

        //        //SRI
        //        #region Line 27
        //        string line27 = "";
        //        if (fwbdata.shiprefnum.Length > 0 || fwbdata.supplemetryshipperinfo1.Length > 0 || fwbdata.supplemetryshipperinfo2.Length > 0)
        //        {
        //            line27 = "SRI/" + fwbdata.shiprefnum + "/" + fwbdata.supplemetryshipperinfo1 + "/" + fwbdata.supplemetryshipperinfo2;
        //        }
        //        #endregion

        //        //OPI
        //        #region Line 28
        //        str1 = "";
        //        string line28 = "";
        //        if (fwbdata.othairport.Length > 0 || fwbdata.othofficedesignator.Length > 0 || fwbdata.othcompanydesignator.Length > 0 || fwbdata.othfilereference.Length > 0 || fwbdata.othparticipentidentifier.Length > 0 || fwbdata.othparticipentcode.Length > 0 || fwbdata.othparticipentairport.Length > 0)
        //        {
        //            str1 = "/" + fwbdata.othparticipentairport + "/" +
        //            fwbdata.othofficedesignator + "" + fwbdata.othcompanydesignator + "/" + fwbdata.othfilereference + "/" +
        //            fwbdata.othparticipentidentifier + "/" + fwbdata.othparticipentcode + "/" + fwbdata.othparticipentairport + "";
        //            str1 = str1.Trim('/');
        //        }

        //        if (fwbdata.othparticipentname.Length > 0 || str1.Length > 0)
        //        {
        //            line28 = "OPI/" + fwbdata.othparticipentname + "$" + str1;
        //        }
        //        line28 = line28.Trim('$');
        //        line28 = line28.Replace("$", "\r\n");
        //        #endregion

        //        //OCI
        //        #region Line 29
        //        string line29 = "";
        //        if (custominfo.Length > 0)
        //        {
        //            for (int i = 0; i < custominfo.Length; i++)
        //            {
        //                line29 = "/" + custominfo[i].IsoCountryCodeOci + "/" + custominfo[i].InformationIdentifierOci + "/" + custominfo[i].CsrIdentifierOci + "/" + custominfo[i].SupplementaryCsrIdentifierOci + "$";
        //            }
        //            line29 = "OCI" + line4.Trim('$');
        //            line29 = line4.Replace("$", "\r\n");
        //        }
        //        #endregion

        //        #region Build FWB
        //        fwbstr = "";
        //        fwbstr = line1.Trim('/');
        //        if (line2.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line2.Trim('/');
        //        }
        //        if (line3.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line3.Trim('/');
        //        }
        //        fwbstr += "\r\n" + line4.Trim('/') + "\r\n" + line5.Trim('/') + "\r\n" + line6.Trim('/');
        //        if (line7.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line7.Trim('/');
        //        }
        //        if (line8.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line8.Trim('/');
        //        }
        //        if (line9.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line9.Trim('/');
        //        }
        //        if (line10.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line10.Trim('/');
        //        }
        //        fwbstr += "\r\n" + line11.Trim('/') + "\r\n" + line12.Trim('/');
        //        if (line13.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line13.Trim('/');
        //        }
        //        if (line14.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line14.Trim('/');
        //        }
        //        if (line15.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line15.Trim('/');
        //        }
        //        if (line16.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line16.Trim('/');
        //        }
        //        if (line17.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line17.Trim('/');
        //        }
        //        if (line18.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line18.Trim('/');
        //        }
        //        if (line19.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line19.Trim('/');
        //        }
        //        if (line20.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line20.Trim('/');
        //        }
        //        if (line21.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line21.Trim('/');
        //        }
        //        if (line22.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line22.Trim('/');
        //        }
        //        if (line23.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line23.Trim('/');
        //        }
        //        if (line24.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line24.Trim('/');
        //        }
        //        if (line25.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line25.Trim('/');
        //        }
        //        if (line26.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line26.Trim('/');
        //        }
        //        if (line27.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line27.Trim('/');
        //        }
        //        if (line28.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line28.Trim('/');
        //        }
        //        if (line29.Trim('/').Length > 0)
        //        {
        //            fwbstr += "\r\n" + line29.Trim('/');
        //        }
        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        fwbstr = "ERR";
        //    }
        //    return fwbstr;
        //}

        //public static string buildRateNode(ref MessageData.RateDescription[] fwbrate)
        //{
        //    string Ratestr = null;
        //    try
        //    {
        //        string str1, str2, str3, str4, str5, str6, str7, str8;
        //        for (int i = 0; i < fwbrate.Length; i++)
        //        {
        //            int cnt = 1;
        //            str1 = str2 = str3 = str4 = str5 = str6 = str7 = str8 = "";
        //            if (fwbrate[i].goodsnature.Length > 0)
        //            {
        //                if (cnt > 1)
        //                {
        //                    str1 = "/" + (cnt++) + "/NG/" + fwbrate[i].goodsnature;
        //                }
        //                else
        //                {
        //                    str1 = "/NG/" + fwbrate[i].goodsnature;
        //                    cnt++;
        //                }
        //            }
        //            if (fwbrate[i].goodsnature1.Length > 0)
        //            {
        //                if (cnt > 1)
        //                {
        //                    str2 = "/" + (cnt++) + "/NC/" + fwbrate[i].goodsnature1;
        //                }
        //                else
        //                {
        //                    str2 = "/NC/" + fwbrate[i].goodsnature1;
        //                    cnt++;
        //                }
        //            }
        //            if (fwbrate[i].weight.Length > 0 || fwbrate[i].length.Length > 0 || fwbrate[i].width.Length > 0 || fwbrate[i].height.Length > 0 || fwbrate[i].pcscnt.Length > 0)
        //            {
        //                if (cnt > 1)
        //                {
        //                    str3 = "/" + (cnt++) + "/ND/" + fwbrate[i].weightindicator + fwbrate[i].weight + "/" + fwbrate[i].unit + fwbrate[i].length + "-" + fwbrate[i].width + "-" + fwbrate[i].height + "/" + fwbrate[i].pcscnt;
        //                }
        //                else
        //                {
        //                    str3 = "/ND/" + fwbrate[i].weightindicator + fwbrate[i].weight + "/" + fwbrate[i].unit + fwbrate[i].length + "-" + fwbrate[i].width + "-" + fwbrate[i].height + "/" + fwbrate[i].pcscnt;
        //                    cnt++;
        //                }
        //            }
        //            else
        //            {
        //                if (cnt > 1)
        //                {
        //                    str3 = "/" + (cnt++) + "/ND/" + "/" + "NDA";
        //                }
        //            }
        //            str3 = str3.Replace("--", "");
        //            if (fwbrate[i].volcode.Length > 0 || fwbrate[i].volamt.Length > 0)
        //            {
        //                if (cnt > 1)
        //                {
        //                    str4 = "/" + (cnt++) + "/NV/" + fwbrate[i].volcode + fwbrate[i].volamt;
        //                }
        //                else
        //                {
        //                    str4 = "/NV/" + fwbrate[i].volcode + fwbrate[i].volamt;
        //                    cnt++;
        //                }
        //            }
        //            if (fwbrate[i].uldtype.Length > 0 || fwbrate[i].uldserialnum.Length > 0 || fwbrate[i].uldowner.Length > 0)
        //            {
        //                if (cnt > 1)
        //                {
        //                    str5 = "/" + (cnt++) + "/NU/" + fwbrate[i].uldtype + fwbrate[i].uldserialnum + fwbrate[i].uldowner;
        //                }
        //                else
        //                {
        //                    str5 = "/NU/" + fwbrate[i].uldtype + fwbrate[i].uldserialnum + fwbrate[i].uldowner;
        //                    cnt++;

        //                }
        //            }
        //            if (fwbrate[i].slac.Length > 0)
        //            {
        //                if (cnt > 1)
        //                {
        //                    str6 = "/" + (cnt++) + "/NS/" + fwbrate[i].slac;
        //                }
        //                else
        //                {
        //                    str6 = "/NS/" + fwbrate[i].slac;
        //                    cnt++;
        //                }
        //            }
        //            if (fwbrate[i].hermonisedcomoditycode.Length > 0)
        //            {
        //                if (cnt > 1)
        //                {
        //                    str7 = "/" + (cnt++) + "/NH/" + fwbrate[i].hermonisedcomoditycode;
        //                }
        //                else
        //                {
        //                    str7 = "/NH/" + fwbrate[i].hermonisedcomoditycode;
        //                    cnt++;
        //                }
        //            }
        //            if (fwbrate[i].isocountrycode.Length > 0)
        //            {
        //                if (cnt > 1)
        //                {
        //                    str8 = "/" + (cnt++) + "/NO/" + fwbrate[i].isocountrycode;
        //                }
        //                else
        //                {
        //                    str8 = "/NO/" + fwbrate[i].isocountrycode;
        //                    cnt++;
        //                }
        //            }
        //            Ratestr += "RTD/" + (i + 1) + "/" + fwbrate[i].pcsidentifier + fwbrate[i].numofpcs + "/" + fwbrate[i].weightindicator + fwbrate[i].awbweight + "/C" + fwbrate[i].rateclasscode + "/S" + fwbrate[i].commoditynumber + "/W" + fwbrate[i].awbweight + "/R" + fwbrate[i].chargerate + "/T" + fwbrate[i].chargeamt;
        //            Ratestr = Ratestr.Trim('/') + "$" + str1.Trim() + "$" + str2.Trim() + "$" + str3.Trim() + "$" + str4.Trim() + "$" + str5.Trim() + "$" + str6.Trim() + "$" + str7.Trim() + "$" + str8.Trim();
        //            Ratestr = Ratestr.Replace("$$", "$");
        //            Ratestr = Ratestr.Trim('$');
        //            Ratestr = Ratestr.Replace("$", "\r\n");
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Ratestr = null;
        //    }
        //    return Ratestr;
        //}
        //#endregion

        //FHL Messaging
        //#region Decode FHL message
        //public static bool decodereceiveFHL(string fhlmsg, ref MessageData.fhlinfo fhldata, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.customsextrainfo[] custominfo)
        //{

        //    bool flag = false;
        //    try
        //    {
        //        string lastrec = "NA";
        //        int line = 0;
        //        try
        //        {
        //            if (fhlmsg.StartsWith("FHL", StringComparison.OrdinalIgnoreCase))
        //            {
        //                string[] str = fhlmsg.Split('$');
        //                if (str.Length > 3)
        //                {
        //                    for (int i = 0; i < str.Length; i++)
        //                    {

        //                        flag = true;
        //                        #region Line 1 version
        //                        if (str[i].StartsWith("FHL", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                fhldata.fhlversionnum = msg[1];
        //                            }
        //                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 2 awb consigment details
        //                        if (str[i].StartsWith("MBI", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                lastrec = "AWB";
        //                                line = 0;
        //                                string[] msg = str[i].Split('/');
        //                                //0th element
        //                                string[] decmes = msg[1].Split('-');
        //                                fhldata.airlineprefix = decmes[0];
        //                                fhldata.awbnum = decmes[1].Substring(0, decmes[1].Length - 6);
        //                                fhldata.origin = decmes[1].Substring(decmes[1].Length - 6, 3);
        //                                fhldata.dest = decmes[1].Substring(decmes[1].Length - 3, 3);
        //                                //1
        //                                if (msg[2].Length > 0)
        //                                {
        //                                    int k = 0;
        //                                    char lastchr = 'A';
        //                                    char[] arr = msg[2].ToCharArray();
        //                                    string[] strarr = new string[arr.Length];
        //                                    for (int j = 0; j < arr.Length; j++)
        //                                    {
        //                                        if ((char.IsNumber(arr[j])) || (arr[j].Equals('.')))
        //                                        {//number                            
        //                                            if (lastchr == 'N')
        //                                                k--;
        //                                            strarr[k] = strarr[k] + arr[j].ToString();
        //                                            lastchr = 'N';
        //                                        }
        //                                        if (char.IsLetter(arr[j]))
        //                                        {//letter
        //                                            if (lastchr == 'L')
        //                                                k--;
        //                                            strarr[k] = strarr[k] + arr[j].ToString();
        //                                            lastchr = 'L';
        //                                        }
        //                                        k++;
        //                                    }
        //                                    fhldata.consigntype = strarr[0];
        //                                    fhldata.pcscnt = strarr[1];//int.Parse(strarr[1]);
        //                                    fhldata.weightcode = strarr[2];
        //                                    fhldata.weight = strarr[3];//float.Parse(strarr[3]);                                            
        //                                }
        //                            }
        //                            catch (Exception e)
        //                            {
        //                                continue;
        //                            }
        //                        }
        //                        #endregion

        //                        #region  line 3 onwards check consignment details
        //                        if (str[i].StartsWith("HBS", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                lastrec = "AWB";
        //                                line = 0;
        //                                decodeFHLconsigmentdetails(str[i], ref consinfo);
        //                            }
        //                            catch (Exception e)
        //                            {
        //                                continue;
        //                            }
        //                        }
        //                        #endregion

        //                        #region  line 4 Free Text
        //                        if (str[i].StartsWith("TXT", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                lastrec = "TXT";
        //                                line = 0;
        //                                consinfo[consinfo.Length - 1].freetextGoodDesc = consinfo[consinfo.Length - 1].freetextGoodDesc + msg[1] + ",";
        //                            }
        //                            catch (Exception e)
        //                            {
        //                                continue;
        //                            }
        //                        }
        //                        #endregion

        //                        #region  line 4 Harmonised Tariff Schedule
        //                        if (str[i].StartsWith("HTS", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                lastrec = "HTS";
        //                                line = 0;
        //                                consinfo[consinfo.Length - 1].commodity = consinfo[consinfo.Length - 1].commodity + msg[1] + ",";
        //                            }
        //                            catch (Exception e)
        //                            {
        //                                continue;
        //                            }
        //                        }
        //                        #endregion

        //                        #region Line5 custom extra info
        //                        if (str[i].StartsWith("OCI", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            string[] msg = str[i].Split('/');
        //                            if (msg.Length > 0)
        //                            {
        //                                lastrec = "OCI";
        //                                MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
        //                                custom.IsoCountryCodeOci = msg[1];
        //                                custom.InformationIdentifierOci = msg[2];
        //                                custom.CsrIdentifierOci = msg[3];
        //                                custom.SupplementaryCsrIdentifierOci = msg[4];
        //                                custom.consigref = awbref;
        //                                Array.Resize(ref custominfo, custominfo.Length + 1);
        //                                custominfo[custominfo.Length - 1] = custom;
        //                            }
        //                        }
        //                        #endregion

        //                        #region Line 5 Shipper Infor
        //                        if (str[i].StartsWith("SHP", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                lastrec = msg[0];
        //                                line = 0;
        //                                if (msg.Length > 1)
        //                                {
        //                                    fhldata.shippername = msg[1];

        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 6 Consignee
        //                        if (str[i].StartsWith("CNE", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            try
        //                            {
        //                                string[] msg = str[i].Split('/');
        //                                lastrec = msg[0];
        //                                line = 0;
        //                                if (msg.Length > 1)
        //                                {
        //                                    fhldata.consname = msg[1];
        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion

        //                        #region Line 11 Charge declaration
        //                        if (str[i].StartsWith("CVD", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            string[] msg = str[i].Split('/');
        //                            if (msg.Length > 1)
        //                            {
        //                                try
        //                                {
        //                                    fhldata.currency = msg[1];
        //                                    fhldata.chargecode = msg[2].Length > 0 ? msg[2] : "";
        //                                    fhldata.chargedec = msg[3].Length > 0 ? msg[3] : "";
        //                                    fhldata.declaredvalue = msg[4];
        //                                    fhldata.declaredcustomvalue = msg[5];
        //                                    fhldata.insuranceamount = msg[6];
        //                                }
        //                                catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
        //                            }
        //                        }
        //                        #endregion

        //                        //#region Other Info
        //                        //if (str[i].StartsWith("/"))
        //                        //{
        //                        //    string[] msg = str[i].Split('/');
        //                        //    try
        //                        //    {
        //                        //        #region SHP Data
        //                        //        if (lastrec == "SHP")
        //                        //        {
        //                        //            line++;
        //                        //            if (line == 1)
        //                        //            {
        //                        //                fhldata.shipperadd = msg[1].Length > 0 ? msg[1] : "";

        //                        //            }
        //                        //            if (line == 2)
        //                        //            {
        //                        //                fhldata.shipperplace = msg[1].Length > 0 ? msg[1] : "";
        //                        //                fhldata.shipperstate = msg[2].Length > 0 ? msg[2] : "";
        //                        //            }
        //                        //            if (line == 3)
        //                        //            {
        //                        //                fhldata.shippercountrycode = msg[1].Length > 0 ? msg[1] : "";
        //                        //                fhldata.shipperpostcode = msg[2].Length > 0 ? msg[2] : "";
        //                        //                fhldata.shippercontactidentifier = msg[3].Length > 0 ? msg[3] : "";
        //                        //                fhldata.shippercontactnum = msg[4].Length > 0 ? msg[4] : "";

        //                        //            }

        //                        //        }
        //                        //        #endregion

        //                        //        #region CNE Data
        //                        //        if (lastrec == "CNE")
        //                        //        {
        //                        //            line++;
        //                        //            if (line == 1)
        //                        //            {
        //                        //                fhldata.consadd = msg[1].Length > 0 ? msg[1] : "";
        //                        //            }
        //                        //            if (line == 2)
        //                        //            {
        //                        //                fhldata.consplace = msg[1].Length > 0 ? msg[1] : "";
        //                        //                fhldata.consstate = msg[2].Length > 0 ? msg[2] : "";
        //                        //            }
        //                        //            if (line == 3)
        //                        //            {
        //                        //                fhldata.conscountrycode = msg[1].Length > 0 ? msg[1] : "";
        //                        //                fhldata.conspostcode = msg[2].Length > 0 ? msg[2] : "";
        //                        //                fhldata.conscontactidentifier = msg[3].Length > 0 ? msg[3] : "";
        //                        //                fhldata.conscontactnum = msg[4].Length > 0 ? msg[4] : "";
        //                        //            }

        //                        //        }
        //                        //        #endregion




        //                        #region Other Info
        //                        if (str[i].StartsWith("/"))
        //                        {
        //                            string[] msg = str[i].Split('/');
        //                            try
        //                            {
        //                                #region SHP Data
        //                                if (lastrec == "SHP")
        //                                {
        //                                    line++;
        //                                    if (line == 1)
        //                                    {
        //                                        //fhldata.shippername = msg[1].Length > 0 ? msg[1] : "";
        //                                        fhldata.shipperadd = msg[1].Length > 0 ? msg[1] : "";
        //                                    }
        //                                    if (line == 2)
        //                                    {
        //                                        //fhldata.shipperadd = msg[1].Length > 0 ? msg[1] : "";
        //                                        if (msg.Length > 2)
        //                                        {
        //                                            fhldata.shipperplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
        //                                            fhldata.shipperstate = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
        //                                        }
        //                                        else
        //                                            fhldata.shipperplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";

        //                                    }

        //                                    if (line == 3)
        //                                    {
        //                                        int len = msg.Length, p;

        //                                        for (p = 0; p < len; p++)
        //                                        {
        //                                            if (p == 1)
        //                                                fhldata.shippercountrycode = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
        //                                            if (p == 2)
        //                                                fhldata.shipperpostcode = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
        //                                            if (p == 3)
        //                                                fhldata.shippercontactidentifier = msg[3].Length > 0 || msg[3] == null ? msg[3] : "";
        //                                            if (p == 4)
        //                                                fhldata.shippercontactnum = msg[4].Length > 0 || msg[4] == null ? msg[4] : "";
        //                                        }

        //                                    }

        //                                }
        //                                #endregion

        //                                #region CNE Data
        //                                if (lastrec == "CNE")
        //                                {
        //                                    line++;
        //                                    if (line == 1)
        //                                    {
        //                                        //fhldata.consname = msg[1].Length > 0 ? msg[1] : "";
        //                                        fhldata.consadd = msg[1].Length > 0 ? msg[1] : "";
        //                                    }
        //                                    if (line == 2)
        //                                    {
        //                                        //fhldata.consadd = msg[1].Length > 0 ? msg[1] : "";
        //                                        if (msg.Length > 2)
        //                                        {
        //                                            fhldata.consplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
        //                                            fhldata.consstate = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
        //                                        }
        //                                        else
        //                                            fhldata.consplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
        //                                    }

        //                                    if (line == 3)
        //                                    {
        //                                        int p, len = msg.Length;
        //                                        for (p = 0; p < len; p++)
        //                                        {
        //                                            if (p == 1)
        //                                                fhldata.conscountrycode = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
        //                                            if (p == 2)
        //                                                fhldata.conspostcode = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
        //                                            if (p == 3)
        //                                                fhldata.conscontactidentifier = msg[3].Length > 0 || msg[3] == null ? msg[3] : "";
        //                                            if (p == 4)
        //                                                fhldata.conscontactnum = msg[4].Length > 0 || msg[4] == null ? msg[4] : "";

        //                                        }

        //                                    }
        //                                }
        //                                #endregion

        //                                #region Commodity
        //                                if (lastrec == "HTS")
        //                                {
        //                                    if (str[i].Length > 1)
        //                                    {
        //                                        consinfo[consinfo.Length - 1].commodity = consinfo[consinfo.Length - 1].commodity + msg[1] + ",";
        //                                    }
        //                                }
        //                                #endregion

        //                                #region freetextGoodDesc
        //                                if (lastrec == "TXT")
        //                                {
        //                                    if (str[i].Length > 1)
        //                                    {
        //                                        consinfo[consinfo.Length - 1].freetextGoodDesc = consinfo[consinfo.Length - 1].freetextGoodDesc + msg[1];
        //                                    }
        //                                }
        //                                #endregion

        //                                #region Splhandling
        //                                if (lastrec == "AWB")
        //                                {
        //                                    if (str[i].Length > 1)
        //                                    {
        //                                        consinfo[consinfo.Length - 1].splhandling = str[i].Replace('/', ',');
        //                                    }
        //                                    lastrec = "NA";
        //                                }
        //                                #endregion

        //                                #region OCI
        //                                if (lastrec == "OCI")
        //                                {
        //                                    string[] msgdata = str[i].Split('/');
        //                                    if (msgdata.Length > 0)
        //                                    {
        //                                        lastrec = "OCI";
        //                                        MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
        //                                        custom.IsoCountryCodeOci = msgdata[1];
        //                                        custom.InformationIdentifierOci = msgdata[2];
        //                                        custom.CsrIdentifierOci = msgdata[3];
        //                                        custom.SupplementaryCsrIdentifierOci = msgdata[4];
        //                                        Array.Resize(ref custominfo, custominfo.Length + 1);
        //                                        custominfo[custominfo.Length - 1] = custom;
        //                                    }
        //                                }
        //                                #endregion
        //                            }
        //                            catch (Exception ex)
        //                            { clsLog.WriteLogAzure(ex.Message); }
        //                        }
        //                        #endregion
        //                    }
        //                }
        //            }
        //            flag = true;
        //        }
        //        catch (Exception ex)
        //        {
        //            flag = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        flag = false;
        //    }
        //    return flag;
        //}
        //#endregion

        #region Encode FHL
        public static string EncodeFHLforsend(ref MessageData.fhlinfo fhldata, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.customsextrainfo[] custominfo)
        {
            string fhl = null;
            try
            {
                #region Line 1
                string line1 = "FHL" + "/" + fhldata.fhlversionnum;
                #endregion

                #region Line 2
                string line2 = "MBI/" + fhldata.airlineprefix + "-" + fhldata.awbnum + fhldata.origin + fhldata.dest + "/" + fhldata.consigntype + fhldata.pcscnt + fhldata.weightcode + fhldata.weight;
                #endregion line 2

                #region Line3
                string line3 = FHLPartBuilder(ref consinfo, ref custominfo);
                #endregion

                //SHP
                #region Line 5
                string line5 = "";
                string str1 = "", str2 = "", str3 = "", str4 = "";
                try
                {
                    if (fhldata.fhlversionnum.Trim() == "5")
                    {
                        if (fhldata.shippername.Length > 0)
                            str1 = "NAM/" + fhldata.shippername;
                        if (fhldata.shipperadd.Length > 0)
                            str2 = "ADR/" + fhldata.shipperadd;
                        if (fhldata.shipperplace.Length > 0 || fhldata.shipperstate.Length > 0)
                            str3 = "LOC/" + fhldata.shipperplace + "/" + fhldata.shipperstate;
                    }
                    else
                    {
                        if (fhldata.shippername.Length > 0)
                            str1 = "/" + fhldata.shippername;
                        if (fhldata.shipperadd.Length > 0)
                            str2 = "/" + fhldata.shipperadd;
                        if (fhldata.shipperplace.Length > 0 || fhldata.shipperstate.Length > 0)
                            str3 = "/" + fhldata.shipperplace + "/" + fhldata.shipperstate;
                    }
                    if (fhldata.shippercountrycode.Length > 0 || fhldata.shipperpostcode.Length > 0 || fhldata.shippercontactidentifier.Length > 0 || fhldata.shippercontactnum.Length > 0)
                        str4 = "/" + fhldata.shippercountrycode + "/" + fhldata.shipperpostcode + "/" + fhldata.shippercontactidentifier + "/" + fhldata.shippercontactnum;

                    if (str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
                    {
                        line5 = "SHP";
                        if (fhldata.fhlversionnum.Trim() == "5")
                        {
                            if (str4.Length > 0)
                                line5 = line5.Trim() + "\r\n" + str1.Trim('/') + "\r\n" + str2.Trim('/') + "\r\n" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                            else if (str3.Length > 0)
                                line5 = line5.Trim() + "\r\n" + str1.Trim('/') + "\r\n" + str2.Trim('/') + "\r\n" + str3.Trim('/');
                            else if (str2.Length > 0)
                                line5 = line5.Trim() + "\r\n" + str1.Trim('/') + "\r\n" + str2.Trim('/');
                            else if (str1.Length > 0)
                                line5 = line5.Trim() + "\r\n" + str1.Trim('/');
                        }
                        else
                        {
                            if (str4.Length > 0)
                                line5 = line5.Trim() + str1.Trim() + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                            else if (str3.Length > 0)
                                line5 = line5.Trim() + str1.Trim() + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
                            else if (str2.Length > 0)
                                line5 = line5.Trim() + str1.Trim() + "\r\n/" + str2.Trim('/');
                            else if (str1.Length > 0)
                                line5 = line5.Trim() + str1.Trim();
                        }
                    }
                }
                catch (Exception ex) {
                    // clsLog.WriteLogAzure(ex.Message); 
                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                }
                #endregion

                //CNE
                #region Line 6
                string line6 = "";
                str1 = str2 = str3 = str4 = "";
                try
                {
                    if (fhldata.fhlversionnum.Trim() == "5")
                    {
                        if (fhldata.consname.Length > 0)
                            str1 = "NAM/" + fhldata.consname;
                        if (fhldata.consadd.Length > 0)
                            str2 = "ADR/" + fhldata.consadd;
                        if (fhldata.consplace.Length > 0 || fhldata.consstate.Length > 0)
                            str3 = "LOC/" + fhldata.consplace + "/" + fhldata.consstate;
                    }
                    else
                    {
                        if (fhldata.consname.Length > 0)
                            str1 = "/" + fhldata.consname;
                        if (fhldata.consadd.Length > 0)
                            str2 = "/" + fhldata.consadd;
                        if (fhldata.consplace.Length > 0 || fhldata.consstate.Length > 0)
                            str3 = "/" + fhldata.consplace + "/" + fhldata.consstate;
                    }
                    if (fhldata.conscountrycode.Length > 0 || fhldata.conspostcode.Length > 0 || fhldata.conscontactidentifier.Length > 0 || fhldata.conscontactnum.Length > 0)
                        str4 = "/" + fhldata.conscountrycode + "/" + fhldata.conspostcode + "/" + fhldata.conscontactidentifier + "/" + fhldata.conscontactnum;

                    if (str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
                    {
                        line6 = "CNE";
                        if (fhldata.fhlversionnum.Trim() == "5")
                        {
                            if (str4.Length > 0)
                                line6 = line6.Trim() + "\r\n" + str1.Trim('/') + "\r\n" + str2.Trim('/') + "\r\n" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                            else if (str3.Length > 0)
                                line6 = line6.Trim() + "\r\n" + str1.Trim('/') + "\r\n" + str2.Trim('/') + "\r\n" + str3.Trim('/');
                            else if (str2.Length > 0)
                                line6 = line6.Trim() + "\r\n" + str1.Trim('/') + "\r\n" + str2.Trim('/');
                            else if (str1.Length > 0)
                                line6 = line6.Trim() + "\r\n" + str1.Trim('/');
                        }
                        else
                        {
                            if (str4.Length > 0)
                                line6 = line6.Trim() + str1.Trim() + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                            else if (str3.Length > 0)
                                line6 = line6.Trim() + str1.Trim() + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
                            else if (str2.Length > 0)
                                line6 = line6.Trim() + str1.Trim() + "\r\n/" + str2.Trim('/');
                            else if (str1.Length > 0)
                                line6 = line6.Trim() + str1.Trim();
                        }
                    }
                }
                catch (Exception ex) {
                    // clsLog.WriteLogAzure(ex.Message);
                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                 }
                #endregion

                //CVD
                #region Line 9
                string line9 = "";
                if ((fhldata.currency.Length > 0 || fhldata.chargecode.Length > 0 || fhldata.declaredvalue.Length > 0 || fhldata.declaredcustomvalue.Length > 0 || fhldata.insuranceamount.Length > 0) && fhldata.chargecode != string.Empty)
                {
                    line9 = "CVD/" + fhldata.currency + "/" + fhldata.chargecode + "/PP/" + fhldata.declaredvalue + "/" + fhldata.declaredcustomvalue + "/" + fhldata.insuranceamount + "";
                }
                else
                {
                    line9 = "CVD/" + fhldata.currency + "/" + fhldata.chargedec + "/" + fhldata.declaredcarriervalue + "/" + fhldata.declaredcustomvalue + "/" + fhldata.insuranceamount + "";
                }
                #endregion


                #region BuildFHL
                fhl = line1.Trim('/') + "\r\n" + line2.Trim() + "\r\n" + line3.Trim();
                if (line5.Length > 0)
                {
                    fhl = fhl + "\r\n" + line5.Trim('/');
                }
                if (line6.Length > 0)
                {
                    fhl = fhl + "\r\n" + line6.Trim('/');
                }
                if (line9.Length > 0)
                {
                    fhl = fhl + "\r\n" + line9.Trim('/');
                }
                #endregion

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                fhl = "";
            }
            return fhl;
        }

        public static string FHLPartBuilder(ref MessageData.consignmnetinfo[] consinfo, ref MessageData.customsextrainfo[] custominfo)
        {
            string output = "";
            try
            {
                #region line 4 Consigment INfo
                string line4 = "";
                if (consinfo.Length > 0)
                {
                    for (int i = 0; i < consinfo.Length; i++)
                    {
                        string splhandling = "";

                        if (consinfo[i].splhandling.Length > 0 && consinfo[i].splhandling != null)
                        {
                            splhandling = consinfo[i].splhandling.Replace(",", "/");
                            if (splhandling.Length > 0)
                            {
                                splhandling = "$" + "/" + splhandling;
                            }

                            //splhandling = consinfo[i].splhandling.Replace(",", "/");
                            //splhandling = "\r\n" + splhandling;
                        }
                        line4 = line4 + "HBS/" + consinfo[i].airlineprefix + consinfo[i].awbnum + "/" + consinfo[i].origin + consinfo[i].dest + "/" + consinfo[i].consigntype + consinfo[i].pcscnt + "/" + consinfo[i].weightcode + consinfo[i].weight + "/" + consinfo[i].slac + "/" + consinfo[i].manifestdesc + ((splhandling.Length) > 0 ? (splhandling) : "");

                        line4 = line4.Trim('/') + "$";
                        if (consinfo[i].freetextGoodDesc.Length > 0)
                        {
                            string desc = consinfo[i].freetextGoodDesc;
                            int characterSize = 65;
                            int count = (int)Math.Ceiling((double)desc.Length / characterSize);
                            int maxLines = Math.Min(count, 9);
                            for (int j = 0; j < maxLines; j++)
                            {
                                int startIndex = j * characterSize;

                                string freedesc = desc.Substring(startIndex, Math.Min(characterSize, desc.Length - startIndex));
                                if (j == 0)
                                {
                                    line4 += "TXT/" + freedesc + "$";
                                }
                                else
                                {
                                    line4 += "/" + freedesc + "$";
                                }

                            }
                            //line4 += "$";

                        }
                        if (consinfo[i].commodity.Length > 0)
                        {
                            //line4 = line4 + "HTS/" + consinfo[i].commodity + "$";
                            if (consinfo[i].commodity.Trim().Length > 0)
                            {
                                string[] harmonizedcode = consinfo[i].commodity.Trim(',').Split(',');
                                for (int k = 0; k < harmonizedcode.Length; k++)
                                {
                                    if (harmonizedcode[k].Trim().Length > 0)
                                    {
                                        if (k == 0)
                                        {
                                            line4 = line4 + "HTS/" + harmonizedcode[k];
                                        }
                                        else
                                        {
                                            line4 += "\r\n/" + harmonizedcode[k];
                                        }
                                    }
                                }
                                line4 += "$";
                            }
                        }
                        #region Line 9 OCI
                        string line9 = "";
                        if (custominfo.Length > 0)
                        {
                            //for (int k = 0; k < custominfo.Length; k++)
                            //{
                            //    if (custominfo[k].consigref.Equals((i + 1).ToString()))
                            //    {
                            //        line9 = "/" + custominfo[k].IsoCountryCodeOci + "/" + custominfo[k].InformationIdentifierOci + "/" + custominfo[k].CsrIdentifierOci + "/" + custominfo[k].SupplementaryCsrIdentifierOci + "$";
                            //    }

                            //}
                            //line9 = "OCI" + line9.Trim('$');
                            //line9 = line9.Replace("$", "\r\n");
                            //if (line9.Length > 0)
                            //{
                            //    output = output + line9 + "\r\n";
                            //}
                            if (custominfo.Length > 0)
                            {
                                for (int k = 0; k < custominfo.Length; k++)
                                {
                                    line9 += (k == 0 ? "OCI/" : "/") + custominfo[k].IsoCountryCodeOci + "/" + custominfo[k].InformationIdentifierOci + "/" + custominfo[k].CsrIdentifierOci + "/" + custominfo[k].SupplementaryCsrIdentifierOci + "$";
                                }
                                output = line9.Replace("$", "\r\n");
                            }
                        }
                        #endregion


                    }
                    line4 = line4.Trim('$');
                    line4 = line4.Replace("$", "\r\n");
                }
                #endregion

                if (output.Length > 0)
                {
                    output = line4 + "\r\n" + output;
                }
                else
                {
                    output = line4;
                }

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                output = "ERR";
            }
            return output;
        }

        #endregion

        #region Decode Consigment Details
        public static void decodeFHLconsigmentdetails(string inputstr, ref MessageData.consignmnetinfo[] consinfo)
        {
            MessageData.consignmnetinfo consig = new MessageData.consignmnetinfo("");
            try
            {
                string[] msg = inputstr.Split('/');

                //consinfo[num] = new MessageData.consignmnetinfo("");
                //consig.airlineprefix = msg[1].Substring(0, 3);
                consig.awbnum = msg[1];

                consig.origin = msg[2].Substring(0, 3);
                consig.dest = msg[2].Substring(3);

                consig.consigntype = "";
                consig.pcscnt = msg[3];//int.Parse(strarr[1]);
                consig.weightcode = msg[4].Substring(0, 1);
                consig.weight = msg[4].Substring(1);

                if (msg.Length > 4)
                {
                    consig.slac = msg[5];
                }
                if (msg.Length > 5)
                {
                    consig.manifestdesc = msg[6];
                }

            }
            catch (Exception ex) {
                // clsLog.WriteLogAzure(ex.Message);
                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            Array.Resize(ref consinfo, consinfo.Length + 1);
            consinfo[consinfo.Length - 1] = consig;
        }
        #endregion

        #region WriteLog
        public static void WriteLog(String Message)
        {
            try
            {
                string APP_PATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + "\\" + "Alert_Log.txt";
                long length = 0;
                StreamWriter sw1;
                if (File.Exists(APP_PATH))
                {
                    FileInfo file = new FileInfo(APP_PATH);
                    length = file.Length;
                }
                if (length > 10000000)
                    sw1 = new StreamWriter(APP_PATH, false);
                else
                    sw1 = new StreamWriter(APP_PATH, true);
                sw1.WriteLine(Message);
                sw1.Close();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }
        #endregion

        //UCM messaging
        #region Decode UCM message
        public static bool decodereceiveUCM(string UCMmsg, ref MessageData.UCMInfo ucmdata, ref MessageData.ULDinfo[] uld)
        {

            bool flag = false;
            try
            {
                string movement = "", flight = "", AirlinePrefix = "";
                try
                {
                    if (UCMmsg.StartsWith("UCM", StringComparison.OrdinalIgnoreCase))
                    {
                        UCMmsg = UCMmsg.Replace("..", ".");
                        // ffrmsg = ffrmsg.Replace("\r\n","$");
                        string[] str = UCMmsg.Split('$');
                        if (str.Length > 3)
                        {
                            if (str[0].StartsWith("UCM", StringComparison.OrdinalIgnoreCase))
                            {
                                for (int i = 1; i < str.Length; i++)
                                {
                                    #region Line 1
                                    if (i == 1)
                                    {
                                        string[] splitStr = str[i].Split('/');
                                        try
                                        {
                                            for (int j = 0; j < splitStr.Length; j++)
                                            {

                                                if (splitStr.Length > 0 && j == 0)
                                                {
                                                    ucmdata.FltNo = splitStr[0];
                                                    AirlinePrefix = ucmdata.FltNo.ToString().Substring(0, 2);
                                                }
                                                try
                                                {
                                                    if (j > 1)
                                                    {
                                                        int val = int.MaxValue;
                                                        if (int.TryParse(splitStr[j], out val))
                                                        {
                                                            flight = splitStr[j];
                                                            flight = AirlinePrefix + flight;
                                                        }
                                                        if (splitStr[j].Contains(AirlinePrefix))
                                                        {
                                                            flight = splitStr[j];
                                                        }
                                                    }
                                                    if (j >= 1 && splitStr[j].Contains('.'))
                                                    {
                                                        string[] localStr = splitStr[i].Split('.');
                                                        if (localStr.Length > 0)
                                                        {
                                                            ucmdata.Date = localStr[0];
                                                            ucmdata.FltRegNo = localStr[1];
                                                            ucmdata.StationCode = localStr[2];
                                                        }
                                                    }
                                                }
                                                catch (Exception ex) {
                                                    // clsLog.WriteLogAzure(ex.Message);
                                                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                 }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // clsLog.WriteLogAzure(ex.Message); 
                                            _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        }

                                        try
                                        {
                                            if (flight.Length > 0)
                                            {
                                                ucmdata.OutFltNo = flight;
                                            }
                                            else
                                            {
                                                ucmdata.OutFltNo = ucmdata.FltNo;
                                            }
                                        }
                                        catch (Exception ex) {
                                            // clsLog.WriteLogAzure(ex.Message); 
                                            _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        }
                                    }
                                    #endregion

                                    #region ULD movement Portion
                                    if (str[i].StartsWith("IN", StringComparison.OrdinalIgnoreCase) || str[i].StartsWith("OUT", StringComparison.OrdinalIgnoreCase))
                                    {
                                        movement = str[i];
                                    }
                                    #endregion

                                    #region line Starts with dot(.) ULD Data
                                    if (str[i].StartsWith(".", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (str[i].Length > 2)
                                        {
                                            string[] splitStr = str[i].Split('.');
                                            if (splitStr.Length > 0)
                                            {
                                                for (int k = 0; k < splitStr.Length; k++)
                                                {
                                                    if (splitStr[k].Length > 0)
                                                    {
                                                        MessageData.ULDinfo ulddata = new MessageData.ULDinfo("");
                                                        if (splitStr[k].ToString().Contains('/'))
                                                        {
                                                            string[] localStr = splitStr[k].ToString().Split('/');
                                                            ulddata.uldno = localStr[0];
                                                            ulddata.uldtype = localStr[0].Substring(0, 3);
                                                            ulddata.uldsrno = localStr[0].Substring(3, localStr[0].Length - 5);
                                                            ulddata.uldowner = localStr[0].Substring(localStr[0].Length - 2, 2);
                                                            ulddata.movement = movement;
                                                            try
                                                            {
                                                                if (localStr.Length > 0)
                                                                {
                                                                    ulddata.stationcode = localStr[1].Trim().Length == 1 ? string.Empty : localStr[1].Trim();
                                                                    ulddata.uldloadingindicator = localStr[1].Trim().Length == 1 ? localStr[1].Trim() : localStr[2];
                                                                }
                                                            }
                                                            catch (Exception ex) {
                                                                // clsLog.WriteLogAzure(ex.Message);
                                                            _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}"); }
                                                        }
                                                        else
                                                        {
                                                            ulddata.uldno = splitStr[k];
                                                            ulddata.uldtype = splitStr[k].Substring(0, 3);
                                                            ulddata.uldsrno = splitStr[k].Substring(3, splitStr[k].Length - 5);
                                                            ulddata.uldowner = splitStr[k].Substring(splitStr[k].Length - 2, 2);
                                                            ulddata.movement = movement;
                                                        }
                                                        Array.Resize(ref uld, uld.Length + 1);
                                                        uld[uld.Length - 1] = ulddata;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }

                        }
                    }
                    flag = true;
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;
        }
        #endregion

        #region Encode UCM
        private string EncodeUCMforsend(ref MessageData.UCMInfo ucmdata, ref MessageData.ULDinfo[] uld)
        {
            string UCM = null;
            try
            {
                #region Line 1
                string line1 = "UCM";
                #endregion

                #region Line 2
                string line2 = ucmdata.FltNo + "/" + ucmdata.Date + "." + ucmdata.FltRegNo + "." + ucmdata.StationCode;
                line2 = line2.Trim('.');
                line2 = line2.Trim('/');
                #endregion line 2

                #region Line3
                string line3 = "IN";
                #endregion

                #region Line4
                string line4 = "";
                if (uld.Length > 0)
                {
                    for (int i = 0; i < uld.Length; i++)
                    {
                        if (uld[i].movement.Equals("IN", StringComparison.OrdinalIgnoreCase))
                        {
                            if (uld[i].uldno.Length > 0)
                            {
                                line4 = line4 + "." + uld[i].uldno + "/" + uld[i].stationcode + "/" + uld[i].uldloadingindicator;
                                line4 = line4.Trim('/');
                            }
                        }
                        if (((i + 1) % 3) == 0)
                        {
                            line4 = line4 + "$";
                        }
                    }
                    line4 = line4.Trim('$');
                    if (line4.Length > 0)
                    {
                        line4 = line4.Replace("$", "\r\n");
                    }
                    else
                    {
                        line4 = ".N";
                    }
                }

                else
                {
                    line4 = ".N";
                }
                #endregion

                #region Line5
                string line5 = "OUT";
                #endregion

                #region Line6
                string line6 = "";
                if (uld.Length > 0)
                {
                    for (int i = 0; i < uld.Length; i++)
                    {
                        if (uld[i].movement.Equals("OUT", StringComparison.OrdinalIgnoreCase))
                        {
                            if (uld[i].uldno.Length > 0)
                            {
                                line6 = line6 + "." + uld[i].uldno + "/" + uld[i].stationcode + "/" + uld[i].uldloadingindicator;
                                line6 = line6.Trim('/');
                            }
                        }
                        if (((i + 1) % 3) == 0)
                        {
                            line6 = line6 + "$";
                        }
                    }
                    line6 = line6.Trim('$');
                    if (line6.Length > 0)
                    {
                        line6 = line6.Replace("$", "\r\n");
                    }
                    else
                    {
                        line6 = ".N";
                    }
                }

                else
                {
                    line6 = ".N";
                }
                #endregion

                #region BuildUCM
                UCM = line1.Trim('/') + "\r\n" + line2.Trim() + "\r\n" + line3.Trim();
                if (line4.Length > 0)
                {
                    UCM = UCM + "\r\n" + line4.Trim('/');
                }
                if (line5.Length > 0)
                {
                    UCM = UCM + "\r\n" + line5.Trim('/');
                }
                if (line6.Length > 0)
                {
                    UCM = UCM + "\r\n" + line6.Trim('/');
                }
                #endregion

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                UCM = "ERR";
            }
            return UCM;
        }
        #endregion

        //Custom Messaging

        #region Custom Message Decoding


        #region Decode  Message
        public async Task<bool> DecodeCustomsMessage(string MessageBody, string strOriginalMessage, string strMessageFrom, out string msgType)
        {
            msgType = "CUSTOME";
            try
            {
                MessageData.CustomMessage Message = new MessageData.CustomMessage();

                if (MessageBody != null)
                {
                    Message.Message = MessageBody;
                    Message.Message = Message.Message.Replace("$", "\r\n");
                    //MessageBody = MessageBody.Replace("\r\n", "$");
                    char[] charSeparator = new char[] { '$' };
                    //charSeparator[0]='$';
                    string[] CustomMessage = MessageBody.Split(charSeparator, StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    foreach (string str in CustomMessage)
                    {
                        count++;
                        //Getting Message Type
                        if (count == 1)
                        {
                            Message.MessageType = str.Trim().Substring(0, 3);
                            msgType = Message.MessageType;
                            // clsLog.WriteLogAzure(Message.MessageType + " Decoding Started");
                            _logger.LogInformation($"{Message.MessageType} Decoding Started");

                        }

                        #region DECODING FSC Message
                        if (Message.MessageType == "FSC")
                        {
                            if (count == 2)
                            {
                                //Getting AWB data if available
                                if (str.Contains("-"))
                                {
                                    string[] PackageTrackingIdentifier = str.Split('/');
                                    string[] AWBInfo = str.Split('-');
                                    Message.AWBPrefix = AWBInfo[0].Trim();
                                    Message.AWBNumber = AWBInfo[1].Trim().Substring(0, 8);
                                    if (AWBInfo.Length > 2)
                                    {
                                        Message.HAWBNumber = AWBInfo[2].Trim();
                                        Message.ConsolidationIdentifier = AWBInfo[2].Substring(0, 1) == "M" ? AWBInfo[2].Substring(0, 1) : string.Empty;
                                    }
                                    if (PackageTrackingIdentifier.Length > 1)
                                        Message.PartArrivalReference = PackageTrackingIdentifier[0];
                                }
                                //Getting CCL Data if available
                                else
                                {
                                    // Message.DestinionCode = str.Trim().Substring(0, 3).Trim();
                                    Message.ControlLocation = str.Trim().Substring(0, 3).Trim();
                                    Message.ArrivalAirport = str.Trim().Substring(0, 3).Trim();
                                    Message.ImportingCarrier = str.Trim().Substring(3).Trim();
                                }
                            }
                            if (count == 3)
                            {
                                //Getting AWB data if available
                                if (str.Contains("-"))
                                {
                                    string[] PackageTrackingIdentifier = str.Split('/');
                                    string[] AWBInfo = str.Split('-');
                                    Message.AWBPrefix = AWBInfo[0].Trim();
                                    Message.AWBNumber = AWBInfo[1].Trim().Substring(0, 8);
                                    if (AWBInfo.Length > 2)
                                    {
                                        Message.HAWBNumber = AWBInfo[2].Trim();
                                        Message.ConsolidationIdentifier = AWBInfo[2].Substring(0, 1) == "M" ? AWBInfo[2].Substring(0, 1) : string.Empty;
                                    }
                                    if (PackageTrackingIdentifier.Length > 1)
                                        Message.PartArrivalReference = PackageTrackingIdentifier[0];
                                }
                            }
                            //Getting Arrival Details
                            if (str.StartsWith("ARR"))
                            {
                                string[] PartArrivalRefInfo = str.Split('-');
                                string[] ArrivalInfo = str.Split('/');
                                if (ArrivalInfo.Length > 1)
                                {
                                    Message.ImportingCarrier = ArrivalInfo[1].Trim().Substring(0, 2).Trim();
                                    Message.FlightNumber = ArrivalInfo[1].Trim().Substring(2).Trim();
                                    //Message.ArrivalDate = ArrivalInfo[2].Trim();
                                    if (ArrivalInfo.Length > 2)
                                    {
                                        string[] ArrivalInfoSub = ArrivalInfo[2].Split('-');
                                        Message.ArrivalDate = ArrivalInfoSub[0].Trim();
                                    }
                                    Message.FlightNo = Message.ImportingCarrier + Message.FlightNumber;
                                    if (Message.ArrivalDate != "")
                                    {
                                        DateTime dt;
                                        dt = DateTime.ParseExact(Message.ArrivalDate + DateTime.Now.Year.ToString().Substring(2, 2), "ddMMMyy", null);
                                        Message.ArrivalDate = dt.ToString("dd-MMM-yyyy");
                                        Message.FlightDate = dt;
                                        Message.ARRPartArrivalReference = PartArrivalRefInfo.Length > 1 ? PartArrivalRefInfo[1] : string.Empty;
                                    }
                                }


                            }
                            //Getting Freight Status Condition (FSC)
                            if (str.StartsWith("FSC") && str.Contains("/"))
                            {

                                string[] FSC = str.Split('/');
                                if (FSC.Length > 0)
                                {
                                    Message.StatusAnswerCode = FSC[1].Trim();

                                }

                            }
                            //Getting TXT Details
                            if (str.StartsWith("TXT"))
                            {
                                string[] TXTInfo = str.Split('/');
                                Message.Information = TXTInfo[1].Trim();
                            }

                            //Getting Way Bill(WBL) Details
                            if (str.StartsWith("WBL"))
                            {
                                string[] WBLInfo = str.Split('/');
                                if (WBLInfo.Length > 0)
                                {
                                    Message.Origin = WBLInfo[1].ToString().Trim().Substring(0, 3);
                                    if (WBLInfo[1].Length > 3)
                                    {
                                        Message.DestinionCode = WBLInfo[1].ToString().Trim().Substring(3, 3);
                                    }
                                    Message.WBLNumberOfPieces = WBLInfo[2].Trim().Substring(1);
                                    Message.WBLWeightIndicator = WBLInfo[3].Trim().Substring(0, 1);
                                    Message.WBLWeight = WBLInfo[3].Trim().Substring(1);
                                    Message.WBLCargoDescription = WBLInfo[4];
                                }
                            }

                            //Getting Transfer(TRN) Details
                            if (str.StartsWith("TRN"))
                            {
                                string[] TRNInfo = str.Split('/');
                                if (TRNInfo.Length > 0)
                                {
                                    if (TRNInfo[1].Contains('-'))
                                    {
                                        string[] TRNDestAirportInfo = TRNInfo[1].Split('-');
                                        Message.TransferDestAirport = TRNDestAirportInfo[0].Trim();
                                        Message.DomesticIdentifier = TRNDestAirportInfo[1].Trim();
                                    }
                                    else
                                    {
                                        Message.TransferDestAirport = TRNInfo[0].Trim();
                                    }
                                    if (TRNInfo.Length > 2)
                                    {
                                        Message.OnwardCarrier = TRNInfo[2].Length < 4 ? TRNInfo[2] : string.Empty;
                                        Message.BondedCarrierID = TRNInfo[2].Length > 3 ? TRNInfo[2] : string.Empty;
                                    }
                                    if (TRNInfo.Length > 3)
                                    {
                                        Message.BondedPremisesIdentifier = TRNInfo[3].Length < 5 ? TRNInfo[3] : string.Empty;
                                        Message.InBondControlNumber = TRNInfo[3].Length > 4 ? TRNInfo[3] : string.Empty;
                                    }
                                }

                            }
                        }
                        #endregion

                        #region DECODING FSI Message
                        if (Message.MessageType == "FSI")
                        {
                            if (count == 2)
                            {
                                //Getting AWB data if available
                                if (str.Contains("-"))
                                {
                                    string[] AWBInfo = str.Split('-');
                                    Message.AWBPrefix = AWBInfo[0].Trim();
                                    Message.AWBNumber = AWBInfo[1].Trim();
                                }
                                //Getting CCL Data if available
                                else
                                {
                                    // Message.DestinionCode = str.Trim().Substring(0, 3).Trim();
                                    Message.ArrivalAirport = str.Trim().Substring(0, 3).Trim();
                                    Message.ImportingCarrier = str.Trim().Substring(3).Trim();
                                    Message.ControlLocation = str.Trim().Substring(0, 3).Trim();
                                }
                            }
                            if (count == 3)
                            {
                                //Getting AWB data if available
                                if (str.Contains("-"))
                                {
                                    string[] AWBInfo = str.Split('-');
                                    Message.AWBPrefix = AWBInfo[0].Trim();
                                    Message.AWBNumber = AWBInfo[1].Trim();
                                }
                            }
                            //Getting Arrival Details
                            if (str.StartsWith("ARR"))
                            {
                                string[] ArrivalInfo = str.Split('/');
                                Message.ImportingCarrier = ArrivalInfo[1].Trim().Substring(0, 2).Trim();
                                Message.FlightNumber = ArrivalInfo[1].Trim().Substring(2).Trim();
                                Message.ArrivalDate = ArrivalInfo[2].Trim();
                                Message.FlightNo = Message.ImportingCarrier + Message.FlightNumber;
                                if (Message.ArrivalDate != "")
                                {
                                    DateTime dt;
                                    dt = DateTime.ParseExact(Message.ArrivalDate + "14", "ddMMMyy", null);
                                    Message.ArrivalDate = dt.ToString("dd-MMM-yyyy");
                                    Message.FlightDate = dt;
                                }
                            }
                            //Getting (CSN)
                            if (str.StartsWith("CSN"))
                            {

                                string[] CSN = str.Split('/');
                                if (CSN.Length > 0)
                                {
                                    string[] ActionCodeDetails = CSN[1].Split('-');
                                    Message.CSNActionCode = ActionCodeDetails[0].Trim();
                                    Message.CSNPieces = ActionCodeDetails[1].Trim();
                                    Message.TransactionDate = CSN[2].Trim().Substring(0, 5);
                                    Message.TransactionTime = CSN[2].Trim().Substring(5).Trim();
                                    if (CSN.Length > 3 && CSN[3].Trim() != string.Empty)
                                    {
                                        Message.CSNEntryType = CSN[3].Trim().Substring(0, 2);
                                        Message.CSNEntryNumber = CSN[3].Trim().Substring(2);
                                    }
                                    if (CSN.Length > 4)
                                    {
                                        Message.CSNRemarks = CSN[4];
                                    }

                                    //Message.TransactionTime = Message.TransactionTime.Insert(2, ":");


                                }

                            }
                        }
                        #endregion

                        #region DECODING FRH Message
                        if (Message.MessageType == "FRH")
                        {
                            if (count == 2)
                            {
                                //Getting AWB data if available
                                if (str.Contains("-"))
                                {
                                    string[] PackageTrackingIdentifier = str.Split('/');
                                    string[] AWBInfo = str.Split('-');
                                    Message.AWBPrefix = AWBInfo[0].Trim();
                                    Message.AWBNumber = AWBInfo[1].Trim().Substring(0, 8);
                                    if (AWBInfo.Length > 2)
                                    {
                                        Message.HAWBNumber = AWBInfo[2].Trim();
                                        Message.ConsolidationIdentifier = AWBInfo[2].Substring(0, 1) == "M" ? AWBInfo[2].Substring(0, 1) : string.Empty;
                                    }
                                    if (PackageTrackingIdentifier.Length > 1)
                                        Message.PackageTrackingIdentifier = PackageTrackingIdentifier[0];

                                }
                                //Getting CCL Data if available
                                else
                                {
                                    //Message.DestinionCode = str.Trim().Substring(0, 3).Trim();
                                    Message.ArrivalAirport = str.Trim().Substring(0, 3).Trim();
                                    Message.AirCarrier = str.Trim().Substring(3).Trim();
                                    Message.ControlLocation = str.Trim().Substring(0, 3).Trim();

                                }
                            }
                            if (count == 3)
                            {
                                //Getting AWB data if available
                                if (str.Contains("-"))
                                {
                                    string[] PackageTrackingIdentifier = str.Split('/');
                                    string[] AWBInfo = str.Split('-');
                                    Message.AWBPrefix = AWBInfo[0].Trim();
                                    Message.AWBNumber = AWBInfo[1].Trim().Substring(0, 8);
                                    if (AWBInfo.Length > 2)
                                    {
                                        Message.HAWBNumber = AWBInfo[2].Trim();
                                        Message.ConsolidationIdentifier = AWBInfo[2].Substring(0, 1) == "M" ? AWBInfo[2].Substring(0, 1) : string.Empty;
                                    }
                                    if (PackageTrackingIdentifier.Length > 1)
                                        Message.PackageTrackingIdentifier = PackageTrackingIdentifier[0];
                                }
                            }
                            //Getting Arrival Details
                            if (str.StartsWith("ARR"))
                            {
                                string[] PartArrivalRefInfo = str.Split('-');
                                string[] ArrivalInfo = str.Split('/');
                                Message.ImportingCarrier = ArrivalInfo[1].Trim().Substring(0, 2).Trim();
                                Message.FlightNumber = ArrivalInfo[1].Trim().Substring(2).Trim();
                                Message.ArrivalDate = ArrivalInfo[2].Trim();
                                Message.FlightNo = Message.ImportingCarrier + Message.FlightNumber;
                                if (Message.ArrivalDate != "")
                                {
                                    DateTime dt;
                                    dt = DateTime.ParseExact(Message.ArrivalDate + DateTime.Now.Year.ToString().Substring(2, 2), "ddMMMyy", null);
                                    Message.ArrivalDate = dt.ToString("dd-MMM-yyyy");
                                    Message.FlightDate = dt;
                                }
                                Message.ARRPartArrivalReference = PartArrivalRefInfo.Length > 1 ? PartArrivalRefInfo[0] : string.Empty;
                            }
                            //Getting (HLD)
                            if (str.StartsWith("HLD"))
                            {

                                string[] HLD = str.Split('/');
                                if (HLD.Length > 0)
                                {
                                    string[] HoldDetails = HLD[1].Split('/');
                                    Message.RequestType = HoldDetails[0].Trim();
                                    if (HoldDetails.Length > 2)
                                    {
                                        Message.RequestExplanation = HoldDetails[1].Trim();
                                    }

                                }

                            }
                        }
                        #endregion

                        #region DECODING FSN Message
                        if (Message.MessageType == "FSN")
                        {
                            if (count == 2)
                            {
                                //Getting AWB data if available
                                if (str.Contains("-"))
                                {
                                    string[] PackageTrackingIdentifier = str.Split('/');
                                    string[] AWBInfo = str.Split('-');
                                    Message.AWBPrefix = AWBInfo[0].Trim();
                                    Message.AWBNumber = AWBInfo[1].Trim().Substring(0, 8);
                                    if (AWBInfo.Length > 2)
                                    {
                                        Message.HAWBNumber = AWBInfo[2].Trim();
                                        Message.ConsolidationIdentifier = AWBInfo[2].Substring(0, 1) == "M" ? AWBInfo[2].Substring(0, 1) : string.Empty;
                                    }
                                    if (PackageTrackingIdentifier.Length > 1)
                                        Message.PackageTrackingIdentifier = PackageTrackingIdentifier[0];
                                }
                                //Getting CCL Data if available
                                else
                                {
                                    //Message.DestinionCode = str.Trim().Substring(0, 3).Trim();
                                    Message.ArrivalAirport = str.Trim().Substring(0, 3).Trim();
                                    Message.AirCarrier = str.Trim().Substring(3).Trim();
                                    Message.ControlLocation = str.Trim().Substring(0, 3).Trim();
                                }
                            }
                            if (count == 3)
                            {
                                //Getting AWB data if available
                                if (str.Contains("-"))
                                {
                                    string[] PackageTrackingIdentifier = str.Split('/');
                                    string[] AWBInfo = str.Split('-');
                                    Message.AWBPrefix = AWBInfo[0].Trim();
                                    Message.AWBNumber = AWBInfo[1].Trim().Substring(0, 8);
                                    if (AWBInfo.Length > 2)
                                    {
                                        Message.HAWBNumber = AWBInfo[2].Trim();
                                        Message.ConsolidationIdentifier = AWBInfo[2].Substring(0, 1) == "M" ? AWBInfo[2].Substring(0, 1) : string.Empty;
                                    }
                                    if (PackageTrackingIdentifier.Length > 1)
                                        Message.PackageTrackingIdentifier = PackageTrackingIdentifier[0];
                                }
                            }
                            //Getting Arrival Details
                            if (str.StartsWith("ARR"))
                            {
                                string[] PartArrivalRefInfo = str.Split('-');
                                string[] ArrivalInfo = str.Split('/');
                                if (ArrivalInfo.Length > 1)
                                {
                                    Message.ImportingCarrier = ArrivalInfo[1].Trim().Substring(0, 2).Trim();
                                    Message.FlightNumber = ArrivalInfo[1].Trim().Substring(2).Trim();
                                    //Message.ArrivalDate = ArrivalInfo[2].Trim();
                                    if (ArrivalInfo.Length > 2)
                                    {
                                        string[] ArrivalInfoSub = ArrivalInfo[2].Split('-');
                                        Message.ArrivalDate = ArrivalInfoSub[0].Trim();
                                    }
                                    Message.FlightNo = Message.ImportingCarrier + Message.FlightNumber;
                                    if (Message.ArrivalDate != "")
                                    {
                                        DateTime dt;
                                        dt = DateTime.ParseExact(Message.ArrivalDate + DateTime.Now.Year.ToString().Substring(2, 2), "ddMMMyy", null);
                                        Message.ArrivalDate = dt.ToString("dd-MMM-yyyy");
                                        Message.FlightDate = dt;
                                        Message.ARRPartArrivalReference = PartArrivalRefInfo.Length > 1 ? PartArrivalRefInfo[1] : string.Empty;
                                    }
                                }
                            }
                            //Getting (CSN)
                            if (str.StartsWith("CSN"))
                            {

                                string[] CSN = str.Split('/');
                                if (CSN.Length > 0)
                                {
                                    string[] ActionCodeDetails = CSN[1].Split('-');
                                    Message.CSNActionCode = ActionCodeDetails[0].Trim();
                                    Message.CSNPieces = ActionCodeDetails[1].Trim();
                                    Message.TransactionDate = CSN[2].Trim().Substring(0, 5);
                                    Message.TransactionTime = CSN[2].Trim().Substring(5).Trim();
                                    if (CSN.Length > 3 && CSN[3].Trim() != string.Empty)
                                    {
                                        Message.CSNEntryType = CSN[3].Trim().Substring(0, 2);
                                        Message.CSNEntryNumber = CSN[3].Trim().Substring(2);
                                    }
                                    if (CSN.Length > 4)
                                    {
                                        Message.CSNRemarks = CSN[4];
                                    }
                                }

                            }
                            //Validate if the FSN is Incoming or Outgoing
                            if (str.StartsWith("ASN"))
                            {
                                return false;
                            }
                        }
                        #endregion

                        #region DECODING FXH Message
                        if (Message.MessageType == "FXH")
                        {
                            if (count == 2)
                            {
                                //Getting AWB data if available
                                if (str.Contains("-"))
                                {
                                    string[] AWBInfo = str.Split('-');
                                    Message.AWBPrefix = AWBInfo[0].Trim();
                                    Message.AWBNumber = AWBInfo[1].Trim();
                                }
                                //Getting CCL Data if available
                                else
                                {
                                    //Message.DestinionCode = str.Trim().Substring(0, 3).Trim();
                                    Message.ArrivalAirport = str.Trim().Substring(0, 3).Trim();
                                    Message.ImportingCarrier = str.Trim().Substring(3).Trim();
                                    Message.ControlLocation = str.Trim().Substring(0, 3).Trim();
                                }
                            }
                            if (count == 3)
                            {
                                //Getting AWB data if available
                                if (str.Contains("-"))
                                {
                                    string[] AWBInfo = str.Split('-');
                                    Message.AWBPrefix = AWBInfo[0].Trim();
                                    Message.AWBNumber = AWBInfo[1].Trim();
                                }
                            }
                            //Getting Arrival Details
                            if (str.StartsWith("ARR"))
                            {
                                string[] ArrivalInfo = str.Split('/');
                                Message.ImportingCarrier = ArrivalInfo[1].Trim().Substring(0, 2).Trim();
                                Message.FlightNumber = ArrivalInfo[1].Trim().Substring(2).Trim();
                                Message.ArrivalDate = ArrivalInfo[2].Trim();
                                Message.FlightNo = Message.ImportingCarrier + Message.FlightNumber;
                                if (Message.ArrivalDate != "")
                                {
                                    DateTime dt;
                                    dt = DateTime.ParseExact(Message.ArrivalDate + DateTime.Now.Year.ToString().Substring(2, 2), "ddMMMyy", null);
                                    Message.ArrivalDate = dt.ToString("dd-MMM-yyyy");
                                    Message.FlightDate = dt;
                                }
                            }
                            //Getting (HLD)
                            if (str.StartsWith("HLD"))
                            {

                                string[] HLD = str.Split('/');
                                if (HLD.Length > 0)
                                {
                                    string[] HoldDetails = HLD[1].Split('/');
                                    Message.RequestType = HoldDetails[0].Trim();
                                    if (HoldDetails.Length > 2)
                                    {
                                        Message.RequestExplanation = HoldDetails[1].Trim();
                                    }

                                }

                            }
                        }
                        #endregion

                        #region DECODING FER Message
                        if (Message.MessageType == "FER")
                        {
                            if (count == 3)
                            {
                                //Getting AWB data if available
                                if (str.Contains("-"))
                                {
                                    string[] PackageTrackingIdentifier = str.Split('/');
                                    string[] AWBInfo = str.Split('-');
                                    Message.AWBPrefix = AWBInfo[0].Trim();
                                    Message.AWBNumber = AWBInfo[1].Trim().Substring(0, 8);
                                    if (AWBInfo.Length > 2)
                                    {
                                        Message.HAWBNumber = AWBInfo[2].Trim();
                                        Message.ConsolidationIdentifier = AWBInfo[2].Substring(0, 1) == "M" ? AWBInfo[2].Substring(0, 1) : string.Empty;
                                    }
                                    if (PackageTrackingIdentifier.Length > 1)
                                        Message.PackageTrackingIdentifier = PackageTrackingIdentifier[0];

                                }

                            }

                            //Getting ERF Details
                            if (count == 2)
                            {
                                string[] ArrivalInfo = str.Split('/');
                                if (ArrivalInfo[0].Trim().Length > 2)
                                {
                                    Message.ERFImportingCarrier = ArrivalInfo[0].Trim().Substring(0, 2).Trim();
                                    Message.ERFFlightNumber = ArrivalInfo[0].Trim().Substring(2).Trim();
                                }
                                Message.ERFDate = ArrivalInfo[1].Trim();
                                Message.FlightNo = Message.ERFImportingCarrier + Message.ERFFlightNumber;
                                if (Message.ERFDate != "")
                                {
                                    DateTime dt;
                                    dt = DateTime.ParseExact(Message.ERFDate + DateTime.Now.Year.ToString().Substring(2, 2), "ddMMMyy", null);
                                    Message.ERFDate = dt.ToString("dd-MMM-yyyy");
                                    Message.FlightDate = dt;
                                }
                            }

                            //Getting ERF Details
                            if (str.StartsWith("ERR"))
                            {
                                string[] ErrInfo = str.Split('/');

                                Message.ErrorCode = ErrInfo[1].Trim().Substring(0, 3).Trim();
                                Message.ErrorMessage = ErrInfo[1].Trim().Substring(3).Trim();
                            }

                        }
                        #endregion

                        #region DECODING PSN Message
                        if (Message.MessageType == "PSN")
                        {
                            if (count == 2)
                            {
                                //Getting AWB data if available
                                if (str.Contains("-"))
                                {
                                    string[] PackageTrackingIdentifier = str.Split('/');
                                    string[] AWBInfo = str.Split('-');
                                    Message.AWBPrefix = AWBInfo[0].Trim();
                                    Message.AWBNumber = AWBInfo[1].Trim().Substring(0, 8);
                                    if (AWBInfo.Length > 2)
                                    {
                                        Message.HAWBNumber = AWBInfo[2].Trim();
                                        Message.ConsolidationIdentifier = AWBInfo[2].Substring(0, 1) == "M" ? AWBInfo[2].Substring(0, 1) : string.Empty;
                                    }
                                    if (PackageTrackingIdentifier.Length > 1)
                                        Message.PackageTrackingIdentifier = PackageTrackingIdentifier[0];
                                }
                                //Getting CCL Data if available
                                else
                                {
                                    //Message.DestinionCode = str.Trim().Substring(0, 3).Trim();
                                    Message.ArrivalAirport = str.Trim().Substring(0, 3).Trim();
                                    Message.AirCarrier = str.Trim().Substring(3).Trim();
                                    Message.ControlLocation = str.Trim().Substring(0, 3).Trim();
                                }
                            }
                            if (count == 3)
                            {
                                //Getting AWB data if available
                                if (str.Contains("-") && !str.StartsWith("CSN"))
                                {
                                    string[] PackageTrackingIdentifier = str.Split('/');
                                    string[] AWBInfo = str.Split('-');
                                    Message.AWBPrefix = AWBInfo[0].Trim();
                                    Message.AWBNumber = AWBInfo[1].Trim().Substring(0, 8);
                                    if (AWBInfo.Length > 2)
                                    {
                                        Message.HAWBNumber = AWBInfo[2].Trim();
                                        Message.ConsolidationIdentifier = AWBInfo[2].Substring(0, 1) == "M" ? AWBInfo[2].Substring(0, 1) : string.Empty;
                                    }
                                    if (PackageTrackingIdentifier.Length > 1)
                                        Message.PackageTrackingIdentifier = PackageTrackingIdentifier[0];
                                }
                            }
                            //Getting Arrival Details
                            if (str.StartsWith("ARR"))
                            {
                                string[] PartArrivalRefInfo = str.Split('-');
                                string[] ArrivalInfo = str.Split('/');
                                if (ArrivalInfo.Length > 1)
                                {
                                    Message.ImportingCarrier = ArrivalInfo[1].Trim().Substring(0, 2).Trim();
                                    Message.FlightNumber = ArrivalInfo[1].Trim().Substring(2).Trim();
                                    //Message.ArrivalDate = ArrivalInfo[2].Trim();
                                    if (ArrivalInfo.Length > 2)
                                    {
                                        string[] ArrivalInfoSub = ArrivalInfo[2].Split('-');
                                        Message.ArrivalDate = ArrivalInfoSub[0].Trim();
                                    }
                                    Message.FlightNo = Message.ImportingCarrier + Message.FlightNumber;
                                    if (Message.ArrivalDate != "")
                                    {
                                        DateTime dt;
                                        dt = DateTime.ParseExact(Message.ArrivalDate + DateTime.Now.Year.ToString().Substring(2, 2), "ddMMMyy", null);
                                        Message.ArrivalDate = dt.ToString("dd-MMM-yyyy");
                                        Message.FlightDate = dt;
                                        Message.ARRPartArrivalReference = PartArrivalRefInfo.Length > 1 ? PartArrivalRefInfo[1] : string.Empty;
                                    }
                                }
                            }
                            //Getting (CSN)
                            if (str.StartsWith("CSN"))
                            {

                                string[] CSN = str.Split('/');
                                if (CSN.Length > 0)
                                {
                                    string[] ActionCodeDetails = CSN[1].Split('-');
                                    Message.CSNActionCode = ActionCodeDetails[0].Trim();
                                    Message.CSNPieces = ActionCodeDetails[1].Trim();
                                    Message.TransactionDate = CSN[2].Trim().Substring(0, 5);
                                    Message.TransactionTime = CSN[2].Trim().Substring(5).Trim();
                                    if (CSN.Length > 3 && CSN[3].Trim() != string.Empty)
                                    {
                                        Message.CSNEntryType = CSN[3].Trim().Substring(0, 2);
                                        Message.CSNEntryNumber = CSN[3].Trim().Substring(2);
                                    }
                                    if (CSN.Length > 4)
                                    {
                                        Message.CSNRemarks = CSN[4];
                                    }
                                }

                            }
                            //Validate if the FSN is Incoming or Outgoing
                            if (str.StartsWith("ASN"))
                            {
                                return false;
                            }
                        }
                        #endregion

                        #region DECODING PER Message
                        if (Message.MessageType == "PER")
                        {
                            if (count == 3)
                            {
                                //Getting AWB data if available
                                if (str.Contains("-"))
                                {
                                    string[] PackageTrackingIdentifier = str.Split('/');
                                    string[] AWBInfo = str.Split('-');
                                    Message.AWBPrefix = AWBInfo[0].Trim();
                                    Message.AWBNumber = AWBInfo[1].Trim().Substring(0, 8);
                                    if (AWBInfo.Length > 2)
                                    {
                                        Message.HAWBNumber = AWBInfo[2].Trim();
                                        Message.ConsolidationIdentifier = AWBInfo[2].Substring(0, 1) == "M" ? AWBInfo[2].Substring(0, 1) : string.Empty;
                                    }
                                    if (PackageTrackingIdentifier.Length > 1)
                                        Message.PackageTrackingIdentifier = PackageTrackingIdentifier[0];

                                }

                            }

                            //Getting ERF Details
                            if (count == 2)
                            {
                                //Getting AWB data if available
                                if (str.Contains("-"))
                                {
                                    string[] PackageTrackingIdentifier = str.Split('/');
                                    string[] AWBInfo = str.Split('-');
                                    Message.AWBPrefix = AWBInfo[0].Trim();
                                    Message.AWBNumber = AWBInfo[1].Trim().Substring(0, 8);
                                    if (AWBInfo.Length > 2)
                                    {
                                        Message.HAWBNumber = AWBInfo[2].Trim();
                                        Message.ConsolidationIdentifier = AWBInfo[2].Substring(0, 1) == "M" ? AWBInfo[2].Substring(0, 1) : string.Empty;
                                    }
                                    if (PackageTrackingIdentifier.Length > 1)
                                        Message.PackageTrackingIdentifier = PackageTrackingIdentifier[0];

                                }
                                if (str.Contains("/"))
                                {
                                    string[] ArrivalInfo = str.Split('/');
                                    if (ArrivalInfo[0].Trim().Length > 2)
                                    {
                                        Message.ERFImportingCarrier = ArrivalInfo[0].Trim().Substring(0, 2).Trim();
                                        Message.ERFFlightNumber = ArrivalInfo[0].Trim().Substring(2).Trim();
                                    }
                                    Message.ERFDate = ArrivalInfo[1].Trim();
                                    Message.FlightNo = Message.ERFImportingCarrier + Message.ERFFlightNumber;
                                    if (Message.ERFDate != "")
                                    {
                                        DateTime dt;
                                        dt = DateTime.ParseExact(Message.ERFDate + DateTime.Now.Year.ToString().Substring(2, 2), "ddMMMyy", null);
                                        Message.ERFDate = dt.ToString("dd-MMM-yyyy");
                                        Message.FlightDate = dt;
                                    }
                                }
                            }

                            //Getting ERF Details
                            if (str.StartsWith("ERR"))
                            {
                                string[] ErrInfo = str.Split('/');

                                Message.ErrorCode = ErrInfo[1].Trim().Substring(0, 3).Trim();
                                Message.ErrorMessage = ErrInfo[1].Trim().Substring(3).Trim();
                            }

                        }
                        #endregion

                    }
                    if (Message.Message != "")
                    {
                        if (Message.MessageType == "FSI" || Message.MessageType == "FSC" || Message.MessageType == "FRH" || Message.MessageType == "FSN" || Message.MessageType == "FXH" || Message.MessageType == "FER" || Message.MessageType == "PSN" || Message.MessageType == "PER")
                        {


                            if (Message.MessageType == "FSN")
                            {
                                //check and insert messages
                                GenericFunction genericFunction = new GenericFunction();
                                string Emailaddress = string.Empty, SitaMessageHeader = string.Empty, SFTPHeaderSITAddress = string.Empty;
                                DataSet dsmessage = genericFunction.GetSitaAddressandMessageVersion(Message.AirCarrier.Length > 0 ? Message.AirCarrier.Substring(0, 2) : "", "Relay_FSN", "AIR", Message.ArrivalAirport, "", Message.FlightNo, "", Message.AWBPrefix);
                                if (dsmessage != null && dsmessage.Tables[0].Rows.Count > 0)
                                {
                                    Emailaddress = dsmessage.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                                    string MessageCommunicationType = dsmessage.Tables[0].Rows[0]["MsgCommType"].ToString();
                                    if (dsmessage.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 0)
                                        SitaMessageHeader = genericFunction.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());
                                    if (dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 1)
                                        SFTPHeaderSITAddress = genericFunction.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());

                                    //insert message to outbox
                                    if (SitaMessageHeader != "")
                                        genericFunction.SaveMessageOutBox("Relay_FSN", SitaMessageHeader + "\r\n" + strOriginalMessage, "SITAFTP", "SITAFTP", "", "", "", "", Message.AWBPrefix + "-" + Message.AWBNumber);

                                    if (Emailaddress != "")
                                        genericFunction.SaveMessageOutBox("Relay_FSN", strOriginalMessage, "EMAIL", Emailaddress, "", "", "", "", Message.AWBPrefix + "-" + Message.AWBNumber);

                                    if (SFTPHeaderSITAddress != "")

                                        genericFunction.SaveMessageOutBox("Relay_FSN", SFTPHeaderSITAddress + "\r\n" + strOriginalMessage, "SFTP", "SFTP", "", "", "", "", Message.AWBPrefix + "-" + Message.AWBNumber);



                                }
                            }
                            //InBox inBox = null;
                            //inBox = new InBox();
                            //inBox.Subject = Message.MessageType;
                            //inBox.Body = strOriginalMessage;
                            //inBox.FromiD = string.Empty;
                            //inBox.ToiD = string.Empty;
                            //inBox.RecievedOn = DateTime.UtcNow;
                            //inBox.IsProcessed = true;
                            //inBox.Status = "ReProcessed";
                            //inBox.Type = Message.MessageType;
                            //inBox.UpdatedBy = strMessageFrom;
                            //inBox.UpdatedOn = DateTime.UtcNow;
                            //inBox.AWBNumber = Message.AWBPrefix+"-"+ Message.AWBNumber;
                            //inBox.FlightNumber = string.Empty;
                            //inBox.FlightOrigin = string.Empty;
                            //inBox.FlightDestination = string.Empty;
                            //inBox.FlightDate = DateTime.Now;
                            //inBox.MessageCategory = "AMS";
                            //AuditLog log = new AuditLog();
                            //log.SaveLog(LogType.InMessage, string.Empty, string.Empty, inBox);

                            //if (await DecodeCustomMessage(Message))
                            //{
                            //    return true;
                            //}

                            return await DecodeCustomMessage(Message);
                        }
                    }
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return false;
        }
        #endregion

        #region Decoding Messages
        public async Task<bool> DecodeCustomMessage(MessageData.CustomMessage Message)
        {
            try
            {
                #region commented sql params, data types and values

                //Preparing Parameters to save the Message Details Against the AWB
                //string[] QueryNames = new string[86];
                //object[] QueryValues = new object[86];
                //SqlDbType[] QueryTypes = new SqlDbType[86];

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


                //int k = 0;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.Int;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.Decimal;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.Int;
                //QueryTypes[k++] = SqlDbType.Decimal;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.BigInt;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.DateTime;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.DateTime;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.VarChar;
                //QueryTypes[k++] = SqlDbType.DateTime;
                //QueryTypes[k++] = SqlDbType.VarChar;

                //int j = 0;

                //QueryValues[j++] = Message.AWBPrefix;
                //QueryValues[j++] = Message.AWBNumber;
                //QueryValues[j++] = Message.MessageType;
                //QueryValues[j++] = Message.HAWBNumber;
                //QueryValues[j++] = Message.ConsolidationIdentifier;
                //QueryValues[j++] = Message.PackageTrackingIdentifier;
                //QueryValues[j++] = Message.AWBPartArrivalReference;
                //QueryValues[j++] = Message.ArrivalAirport;
                //QueryValues[j++] = Message.AirCarrier;
                //QueryValues[j++] = Message.Origin;
                //QueryValues[j++] = Message.DestinionCode;
                //if (string.IsNullOrEmpty(Message.WBLNumberOfPieces))
                //{ QueryValues[j++] = "0"; }
                //else
                //{
                //    QueryValues[j++] = Message.WBLNumberOfPieces;
                //}

                //QueryValues[j++] = Message.WBLWeightIndicator;
                //if (string.IsNullOrEmpty(Message.WBLWeight))
                //{ QueryValues[j++] = "0"; }
                //else
                //{
                //    QueryValues[j++] = Message.WBLWeight;
                //}
                //QueryValues[j++] = Message.WBLCargoDescription;
                //QueryValues[j++] = Message.ArrivalDate;
                //QueryValues[j++] = Message.PartArrivalReference;
                //QueryValues[j++] = Message.BoardedQuantityIdentifier;
                //if (string.IsNullOrEmpty(Message.BoardedPieceCount))
                //{ QueryValues[j++] = "0"; }
                //else
                //{
                //    QueryValues[j++] = Message.BoardedPieceCount;
                //}
                //if (string.IsNullOrEmpty(Message.BoardedWeight))
                //{
                //    QueryValues[j++] = "0";
                //}
                //else
                //{
                //    QueryValues[j++] = Message.BoardedWeight;
                //}
                //QueryValues[j++] = Message.ArrWeightCode;
                //QueryValues[j++] = Message.ImportingCarrier;
                //QueryValues[j++] = Message.FlightNumber;
                //QueryValues[j++] = Message.ARRPartArrivalReference;
                //QueryValues[j++] = Message.RequestType;
                //QueryValues[j++] = Message.RequestExplanation;
                //QueryValues[j++] = Message.EntryType;
                //QueryValues[j++] = Message.EntryNumber;
                //QueryValues[j++] = Message.AMSParticipantCode;
                //QueryValues[j++] = Message.ShipperName;
                //QueryValues[j++] = Message.ShipperAddress;
                //QueryValues[j++] = Message.ShipperCity;
                //QueryValues[j++] = Message.ShipperState;
                //QueryValues[j++] = Message.ShipperCountry;
                //QueryValues[j++] = Message.ShipperPostalCode;
                //QueryValues[j++] = Message.ConsigneeName;
                //QueryValues[j++] = Message.ConsigneeAddress;
                //QueryValues[j++] = Message.ConsigneeCity;
                //QueryValues[j++] = Message.ConsigneeState;
                //QueryValues[j++] = Message.ConsigneeCountry;
                //QueryValues[j++] = Message.ConsigneePostalCode;
                //QueryValues[j++] = Message.TransferDestAirport;
                //QueryValues[j++] = Message.DomesticIdentifier;
                //QueryValues[j++] = Message.BondedCarrierID;
                //QueryValues[j++] = Message.OnwardCarrier;
                //QueryValues[j++] = Message.BondedPremisesIdentifier;
                //QueryValues[j++] = Message.InBondControlNumber;
                //QueryValues[j++] = Message.OriginOfGoods;
                //if (string.IsNullOrEmpty(Message.DeclaredValue))
                //{ QueryValues[j++] = "0"; }
                //else
                //{
                //    QueryValues[j++] = Message.DeclaredValue;
                //}
                //QueryValues[j++] = Message.CurrencyCode;
                //QueryValues[j++] = Message.CommodityCode;
                //QueryValues[j++] = Message.LineIdentifier;
                //QueryValues[j++] = Message.AmendmentCode;
                //QueryValues[j++] = Message.AmendmentExplanation;
                //QueryValues[j++] = Message.DeptImportingCarrier;
                //QueryValues[j++] = Message.DeptFlightNumber;
                //QueryValues[j++] = Message.DeptScheduledArrivalDate;
                //QueryValues[j++] = Message.LiftoffDate;
                //QueryValues[j++] = Message.LiftoffTime;
                //QueryValues[j++] = Message.DeptActualImportingCarrier;
                //QueryValues[j++] = Message.DeptActualFlightNumber;
                //QueryValues[j++] = Message.ASNStatusCode;
                //QueryValues[j++] = Message.ASNActionExplanation;
                //QueryValues[j++] = Message.CSNActionCode;
                //QueryValues[j++] = Message.CSNPieces;
                //QueryValues[j++] = Message.TransactionDate;
                //QueryValues[j++] = Message.TransactionTime;
                //QueryValues[j++] = Message.CSNEntryType;
                //QueryValues[j++] = Message.CSNEntryNumber;
                //QueryValues[j++] = Message.CSNRemarks;
                //QueryValues[j++] = Message.ErrorCode;
                //QueryValues[j++] = Message.ErrorMessage;
                //QueryValues[j++] = Message.StatusRequestCode;
                //QueryValues[j++] = Message.StatusAnswerCode;
                //QueryValues[j++] = Message.Information;
                //QueryValues[j++] = Message.ERFImportingCarrier;
                //QueryValues[j++] = Message.ERFFlightNumber;
                //QueryValues[j++] = Message.ERFDate;
                //QueryValues[j++] = Message.Message;
                //QueryValues[j++] = DateTime.Now.ToString();
                //QueryValues[j++] = "Air AMS";
                //QueryValues[j++] = DateTime.Now.ToString();
                //QueryValues[j++] = "Air AMS";
                //QueryValues[j++] = Message.FlightNo;
                //QueryValues[j++] = Message.FlightDate == DateTime.MinValue ? "01/01/1900" : Message.FlightDate.ToString();
                //QueryValues[j++] = Message.ControlLocation;

                //SQLServer db = new SQLServer();
                //if (db.InsertData("SP_UpdateInboxCustomsMessage", QueryNames, QueryTypes, QueryValues))

                #endregion

                SqlParameter[] parameters =
                {
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = Message.AWBPrefix },
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = Message.AWBNumber },
                    new SqlParameter("@MessageType", SqlDbType.VarChar) { Value = Message.MessageType },
                    new SqlParameter("@HAWBNumber", SqlDbType.VarChar) { Value = Message.HAWBNumber },
                    new SqlParameter("@ConsolidationIdentifier", SqlDbType.VarChar) { Value = Message.ConsolidationIdentifier },
                    new SqlParameter("@PackageTrackingIdentifier", SqlDbType.VarChar) { Value = Message.PackageTrackingIdentifier },
                    new SqlParameter("@AWBPartArrivalReference", SqlDbType.VarChar) { Value = Message.AWBPartArrivalReference },
                    new SqlParameter("@ArrivalAirport", SqlDbType.VarChar) { Value = Message.ArrivalAirport },
                    new SqlParameter("@AirCarrier", SqlDbType.VarChar) { Value = Message.AirCarrier },
                    new SqlParameter("@Origin", SqlDbType.VarChar) { Value = Message.Origin },
                    new SqlParameter("@DestinionCode", SqlDbType.VarChar) { Value = Message.DestinionCode },
                    new SqlParameter("@WBLNumberOfPieces", SqlDbType.Int) { Value = string.IsNullOrEmpty(Message.WBLNumberOfPieces) ? "0" : Message.WBLNumberOfPieces },
                    new SqlParameter("@WBLWeightIndicator", SqlDbType.VarChar) { Value = Message.WBLWeightIndicator },
                    new SqlParameter("@WBLWeight", SqlDbType.Decimal) { Value = string.IsNullOrEmpty(Message.WBLWeight) ? "0" : Message.WBLWeight},
                    new SqlParameter("@WBLCargoDescription", SqlDbType.VarChar) { Value = Message.WBLCargoDescription },
                    new SqlParameter("@ArrivalDate", SqlDbType.VarChar) { Value = Message.ArrivalDate },
                    new SqlParameter("@PartArrivalReference", SqlDbType.VarChar) { Value = Message.PartArrivalReference },
                    new SqlParameter("@BoardedQuantityIdentifier", SqlDbType.VarChar) { Value = Message.BoardedQuantityIdentifier },
                    new SqlParameter("@BoardedPieceCount", SqlDbType.Int) { Value = string.IsNullOrEmpty(Message.BoardedPieceCount) ? "0" : Message.BoardedPieceCount },
                    new SqlParameter("@BoardedWeight", SqlDbType.Decimal) { Value = string.IsNullOrEmpty(Message.BoardedWeight) ? "0" : Message.BoardedWeight },
                    new SqlParameter("@ARRWeightCode", SqlDbType.VarChar) { Value = Message.ArrWeightCode },
                    new SqlParameter("@ImportingCarrier", SqlDbType.VarChar) { Value = Message.ImportingCarrier },
                    new SqlParameter("@FlightNumber", SqlDbType.VarChar) { Value = Message.FlightNumber },
                    new SqlParameter("@ARRPartArrivalReference", SqlDbType.VarChar) { Value = Message.ARRPartArrivalReference },
                    new SqlParameter("@RequestType", SqlDbType.VarChar) { Value = Message.RequestType },
                    new SqlParameter("@RequestExplanation", SqlDbType.VarChar) { Value = Message.RequestExplanation },
                    new SqlParameter("@EntryType", SqlDbType.VarChar) { Value = Message.EntryType },
                    new SqlParameter("@EntryNumber", SqlDbType.VarChar) { Value = Message.EntryNumber },
                    new SqlParameter("@AMSParticipantCode", SqlDbType.VarChar) { Value = Message.AMSParticipantCode },
                    new SqlParameter("@ShipperName", SqlDbType.VarChar) { Value = Message.ShipperName },
                    new SqlParameter("@ShipperAddress", SqlDbType.VarChar) { Value = Message.ShipperAddress },
                    new SqlParameter("@ShipperCity", SqlDbType.VarChar) { Value = Message.ShipperCity },
                    new SqlParameter("@ShipperState", SqlDbType.VarChar) { Value = Message.ShipperState },
                    new SqlParameter("@ShipperCountry", SqlDbType.VarChar) { Value = Message.ShipperCountry },
                    new SqlParameter("@ShipperPostalCode", SqlDbType.VarChar) { Value = Message.ShipperPostalCode },
                    new SqlParameter("@ConsigneeName", SqlDbType.VarChar) { Value = Message.ConsigneeName },
                    new SqlParameter("@ConsigneeAddress", SqlDbType.VarChar) { Value = Message.ConsigneeAddress },
                    new SqlParameter("@ConsigneeCity", SqlDbType.VarChar) { Value = Message.ConsigneeCity },
                    new SqlParameter("@ConsigneeState", SqlDbType.VarChar) { Value = Message.ConsigneeState },
                    new SqlParameter("@ConsigneeCountry", SqlDbType.VarChar) { Value = Message.ConsigneeCountry },
                    new SqlParameter("@ConsigneePostalCode", SqlDbType.VarChar) { Value = Message.ConsigneePostalCode },
                    new SqlParameter("@TransferDestAirport", SqlDbType.VarChar) { Value = Message.TransferDestAirport },
                    new SqlParameter("@DomesticIdentifier", SqlDbType.VarChar) { Value = Message.DomesticIdentifier },
                    new SqlParameter("@BondedCarrierID", SqlDbType.VarChar) { Value = Message.BondedCarrierID },
                    new SqlParameter("@OnwardCarrier", SqlDbType.VarChar) { Value = Message.OnwardCarrier },
                    new SqlParameter("@BondedPremisesIdentifier", SqlDbType.VarChar) { Value = Message.BondedPremisesIdentifier },
                    new SqlParameter("@InBondControlNumber", SqlDbType.VarChar) { Value = Message.InBondControlNumber },
                    new SqlParameter("@OriginOfGoods", SqlDbType.VarChar) { Value = Message.OriginOfGoods },
                    new SqlParameter("@DeclaredValue", SqlDbType.Decimal) { Value = string.IsNullOrEmpty(Message.DeclaredValue) ? "0" : Message.DeclaredValue },
                    new SqlParameter("@CurrencyCode", SqlDbType.VarChar) { Value = Message.CurrencyCode },
                    new SqlParameter("@CommodityCode", SqlDbType.VarChar) { Value = Message.CommodityCode },
                    new SqlParameter("@LineIdentifier", SqlDbType.VarChar) { Value = Message.LineIdentifier },
                    new SqlParameter("@AmendmentCode", SqlDbType.VarChar) { Value = Message.AmendmentCode },
                    new SqlParameter("@AmendmentExplanation", SqlDbType.VarChar) { Value = Message.AmendmentExplanation },
                    new SqlParameter("@DeptImportingCarrier", SqlDbType.VarChar) { Value = Message.DeptImportingCarrier },
                    new SqlParameter("@DeptFlightNumber", SqlDbType.VarChar) { Value = Message.DeptFlightNumber },
                    new SqlParameter("@DeptScheduledArrivalDate", SqlDbType.VarChar) { Value = Message.DeptScheduledArrivalDate },
                    new SqlParameter("@LiftoffDate", SqlDbType.VarChar) { Value = Message.LiftoffDate },
                    new SqlParameter("@LiftoffTime", SqlDbType.VarChar) { Value = Message.LiftoffTime },
                    new SqlParameter("@DeptActualImportingCarrier", SqlDbType.VarChar) { Value = Message.DeptActualImportingCarrier },
                    new SqlParameter("@DeptActualFlightNumber", SqlDbType.VarChar) { Value = Message.DeptActualFlightNumber },
                    new SqlParameter("@ASNStatusCode", SqlDbType.VarChar) { Value = Message.ASNStatusCode },
                    new SqlParameter("@ASNActionExplanation", SqlDbType.VarChar) { Value = Message.ASNActionExplanation },
                    new SqlParameter("@CSNActionCode", SqlDbType.VarChar) { Value = Message.CSNActionCode },
                    new SqlParameter("@CSNPieces", SqlDbType.VarChar) { Value = Message.CSNPieces },
                    new SqlParameter("@TransactionDate", SqlDbType.VarChar) { Value = Message.TransactionDate },
                    new SqlParameter("@TransactionTime", SqlDbType.VarChar) { Value = Message.TransactionTime },
                    new SqlParameter("@CSNEntryType", SqlDbType.VarChar) { Value = Message.CSNEntryType },
                    new SqlParameter("@CSNEntryNumber", SqlDbType.VarChar) { Value = Message.CSNEntryNumber },
                    new SqlParameter("@CSNRemarks", SqlDbType.VarChar) { Value = Message.CSNRemarks },
                    new SqlParameter("@ErrorCode", SqlDbType.VarChar) { Value = Message.ErrorCode },
                    new SqlParameter("@ErrorMessage", SqlDbType.VarChar) { Value = Message.ErrorMessage },
                    new SqlParameter("@StatusRequestCode", SqlDbType.VarChar) { Value = Message.StatusRequestCode },
                    new SqlParameter("@StatusAnswerCode", SqlDbType.VarChar) { Value = Message.StatusAnswerCode },
                    new SqlParameter("@Information", SqlDbType.VarChar) { Value = Message.Information },
                    new SqlParameter("@ERFImportingCarrier", SqlDbType.VarChar) { Value = Message.ERFImportingCarrier },
                    new SqlParameter("@ERFFlightNumber", SqlDbType.VarChar) { Value = Message.ERFFlightNumber },
                    new SqlParameter("@ERFDate", SqlDbType.VarChar) { Value = Message.ERFDate },
                    new SqlParameter("@Message", SqlDbType.VarChar) { Value = Message.Message },
                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = DateTime.Now.ToString() },
                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "Air AMS" },
                    new SqlParameter("@CreatedOn", SqlDbType.DateTime) { Value = DateTime.Now.ToString() },
                    new SqlParameter("@CreatedBy", SqlDbType.VarChar) { Value = "Air AMS" },
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = Message.FlightNo },
                    new SqlParameter("@FlightDate", SqlDbType.VarChar) { Value = Message.FlightDate == DateTime.MinValue ? "01/01/1900" : Message.FlightDate.ToString() },
                    new SqlParameter("@ControlLocation", SqlDbType.VarChar) { Value = Message.ControlLocation }
                };

                var dbRes = await _readWriteDao.ExecuteNonQueryAsync("SP_UpdateInboxCustomsMessage", parameters);
                if (dbRes)
                {
                    // clsLog.WriteLogAzure(Message.MessageType + " Sent Successfully");
                    _logger.LogInformation($"{Message.MessageType} Sent Successfully");

                    if (Message.MessageType == "PSN" && (Message.CSNActionCode == "6H" || Message.CSNActionCode == "7H" || Message.CSNActionCode == "8H"))
                    {
                        try
                        {
                            //PSNMessageProcessor PSN = new PSNMessageProcessor();
                            //if (PSN.GeneratePSNMessage(Message.AWBPrefix + "-" + Message.AWBNumber, (String.IsNullOrEmpty(Message.HAWBNumber) == true ? string.Empty : Message.HAWBNumber)))

                            var genPsnMegRes = _pSNMessageProcessor.GeneratePSNMessage(Message.AWBPrefix + "-" + Message.AWBNumber, (String.IsNullOrEmpty(Message.HAWBNumber) == true ? string.Empty : Message.HAWBNumber));
                            if (genPsnMegRes)
                            {
                                // clsLog.WriteLogAzure(Message.MessageType + "Auto PSN Outbound Successfully Sent for : " + Message.AWBPrefix + "-" + Message.AWBNumber + "-" + Message.HAWBNumber);
                                _logger.LogInformation($"{Message.MessageType} Auto PSN Outbound Successfully Sent for :  {Message.AWBPrefix} - {Message.AWBNumber} - {Message.HAWBNumber}");
                            }

                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(Message.MessageType + "Auto PSN Outbound Error: ", ex);
                            _logger.LogError(ex,  $"{Message.MessageType} Auto PSN Outbound Error: ");
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(Message.MessageType + "Error: ", ex);
                _logger.LogError($"{Message.MessageType} Error: {ex}");
                return false;
            }
        }
        #endregion
        #endregion


        //#region FSB Message Decoding for Update Dateabse
        ///// <summary>
        ///// Created By:Badiuz khan
        ///// Created On:2016-01-15
        ///// Description:Decoding FSB Message 
        ///// </summary>
        //string ErrorMessage = string.Empty;
        //public static bool DecodingFSBMessge(string strMessage, MessageData.FSBAWBInformation fsbMessage, MessageData.ShipperInformation fsbShipper, MessageData.ConsigneeInformation fsbConsignee, List<MessageData.RouteInformation> RouteIformation, List<MessageData.FSBDimensionInformation> Dimensionformation, List<MessageData.AWBBUPInformation> bublistinformation)
        //{
        //    bool flag = true;

        //    MessageData.FSBDimensionInformation Dimension = new MessageData.FSBDimensionInformation();
        //    MessageData.AWBBUPInformation bupInformation = new MessageData.AWBBUPInformation();
        //    MessageData.RouteInformation Route = new MessageData.RouteInformation();
        //    try
        //    {
        //        var fsbLine = new StringReader(strMessage.Replace("$", "\r\n").Replace("$", "\n"));
        //        int lineNo = 1;
        //        string lineText;
        //        while ((lineText = fsbLine.ReadLine()) != null)
        //        {

        //            if (lineText.Length > 0)
        //            {
        //                switch (lineNo)
        //                {
        //                    case 1:
        //                        string[] currentLineText = lineText.Split('/');
        //                        string fwbName = string.Empty;
        //                        string fwbVersion = string.Empty;
        //                        if (currentLineText.Length >= 1)
        //                        {
        //                            if (currentLineText[0].Length == 3)
        //                                fsbMessage.MessageType = currentLineText[0] != "" ? currentLineText[0] : "";
        //                        }
        //                        if (currentLineText.Length >= 2)
        //                        {
        //                            if (currentLineText[1].Length > 0)
        //                            {
        //                                fsbMessage.MessageVersion = currentLineText[1] != "" ? currentLineText[1] : "";
        //                            }

        //                        }
        //                        break;
        //                    case 2:
        //                        string[] currentlineText = lineText.Split('/');
        //                        char piecesCode = ' ';
        //                        int indexofKORL = 0;

        //                        if (currentlineText.Length >= 1)
        //                        {
        //                            if (currentlineText[0].Length >= 12)
        //                            {
        //                                int indexOfHyphen = currentlineText[0].IndexOf('-');
        //                                if (indexOfHyphen == 3)
        //                                {
        //                                    fsbMessage.AirlinePrefix = currentlineText[0].Substring(0, 3);
        //                                    fsbMessage.AWBNo = currentlineText[0].Substring(4, 8);
        //                                }

        //                            }
        //                            if (currentlineText[0].Substring(12).Length == 6)
        //                            {
        //                                fsbMessage.AWBOrigin = currentlineText[0].Substring(12, 3);
        //                                fsbMessage.AWBDestination = currentlineText[0].Substring(15, 3);
        //                            }

        //                        }

        //                        if (currentlineText.Length >= 2)
        //                        {
        //                            if (currentlineText[1].Length > 1)
        //                            {
        //                                if (currentlineText[1].Contains("K"))
        //                                {
        //                                    indexofKORL = currentlineText[1].LastIndexOf('K');
        //                                    fsbMessage.WeightCode = "K";
        //                                }
        //                                else if (currentlineText[1].Contains("L"))
        //                                {
        //                                    indexofKORL = currentlineText[1].LastIndexOf('L');
        //                                    fsbMessage.WeightCode = "L";
        //                                }

        //                                piecesCode = currentlineText[1] != ""
        //                                                 ? char.Parse(currentlineText[1].Substring(0, 1))
        //                                                 : ' ';
        //                                if (!currentlineText[1].Contains("K") && (!currentlineText[1].Contains("L")))
        //                                {
        //                                    fsbMessage.TotalAWbPiececs = currentlineText[1] != ""
        //                                                 ? int.Parse(currentlineText[1].Substring(1))
        //                                                 : 0;
        //                                }

        //                                else if (((currentlineText[1].Contains("K")) || (currentlineText[1].Contains("L"))) &&
        //                                         (!currentlineText[1].Substring(1).Contains("T")))
        //                                {
        //                                    fsbMessage.TotalAWbPiececs = currentlineText[1] != ""
        //                                                 ? int.Parse(currentlineText[1].Substring(1, indexofKORL - 1))
        //                                                 : 0;

        //                                    fsbMessage.GrossWeight = currentlineText[1] != ""
        //                                                 ? decimal.Parse(currentlineText[1].Substring(indexofKORL + 1))
        //                                                 : decimal.Parse("0.0");
        //                                }

        //                            }
        //                        }
        //                        break;
        //                    default:
        //                        if (lineText.Trim().Length > 2)
        //                        {
        //                            var tagName = lineText.Substring(0, 3);
        //                            switch (tagName)
        //                            {
        //                                case "RTG":
        //                                    try
        //                                    {

        //                                        string[] currentLineRouteText = lineText.Split('/');

        //                                        if (currentLineRouteText.Length >= 2)
        //                                        {
        //                                            for (int k = 1; k < currentLineRouteText.Length; k++)
        //                                            {
        //                                                Route = new MessageData.RouteInformation();
        //                                                if (k == 1)
        //                                                    Route.FlightOrigin = fsbMessage.AWBOrigin;
        //                                                else
        //                                                    Route.FlightOrigin = currentLineRouteText[k - 1].Substring(0, 3);

        //                                                if (currentLineRouteText[k].Length == 5)
        //                                                {
        //                                                    Route.Carriercode = currentLineRouteText[k].Substring(3, 2);
        //                                                    Route.FlightDestination = currentLineRouteText[k].Substring(0, 3);

        //                                                }
        //                                                RouteIformation.Add(Route);
        //                                            }
        //                                        }
        //                                    }
        //                                    catch (Exception ex) { clsLog.WriteLogAzure("Error in Decoding FSB Message Route(RTG) TAG " + ex.ToString()); }
        //                                    break;
        //                                case "SHP":
        //                                    try
        //                                    {
        //                                        string currentline = string.Empty;
        //                                        currentline = ReadFile(tagName, strMessage.Replace("$", "\r\n").Replace("$", "\n"));
        //                                        string[] currentShipperLineText = currentline.Split('#');
        //                                        string shpTag = string.Empty;
        //                                        if (currentShipperLineText.Length >= 1)
        //                                        {
        //                                            string[] currentLineTextSplit = currentShipperLineText[0].Split('/');
        //                                            if (currentLineTextSplit.Length >= 1)
        //                                            {
        //                                                if (currentLineTextSplit[0].Length == 3)
        //                                                {
        //                                                    shpTag = currentLineTextSplit[0] != "" ? currentLineTextSplit[0] : "";
        //                                                }
        //                                            }
        //                                            if (currentLineTextSplit.Length >= 2)
        //                                            {
        //                                                fsbShipper.ShipperAccountNo = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
        //                                            }
        //                                        }
        //                                        if (currentShipperLineText.Length >= 2)
        //                                        {
        //                                            string[] currentLineTextSplit = currentShipperLineText[1].Split('/');
        //                                            if (currentLineTextSplit.Length >= 2)
        //                                            {
        //                                                fsbShipper.ShipperName = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
        //                                            }
        //                                        }

        //                                        if (currentShipperLineText.Length >= 3)
        //                                        {
        //                                            string[] currentLineTextSplit = currentShipperLineText[2].Split('/');
        //                                            if (currentLineTextSplit.Length >= 2)
        //                                            {

        //                                                fsbShipper.ShipperStreetAddress = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
        //                                            }
        //                                        }

        //                                        if (currentShipperLineText.Length >= 4)
        //                                        {

        //                                            string[] currentLineTextSplit = currentShipperLineText[3].Split('/');
        //                                            if (currentLineTextSplit.Length >= 2)
        //                                            {
        //                                                fsbShipper.ShipperPlace = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
        //                                            }
        //                                            if (currentLineTextSplit.Length >= 3)
        //                                            {
        //                                                fsbShipper.ShipperState = currentLineTextSplit[2] != "" ? currentLineTextSplit[2] : "";
        //                                            }
        //                                        }

        //                                        if (currentShipperLineText.Length >= 5)
        //                                        {

        //                                            string[] currentLineTextSplit = currentShipperLineText[4].Split('/');
        //                                            if (currentLineTextSplit.Length >= 2)
        //                                            {
        //                                                fsbShipper.ShipperCountrycode = currentLineTextSplit[1] != "" ? currentLineTextSplit[1].Substring(0, 2) : "";
        //                                            }
        //                                            if (currentLineTextSplit.Length >= 3)
        //                                            {
        //                                                fsbShipper.ShipperPostalCode = currentLineTextSplit[2] != "" ? currentLineTextSplit[2] : "";
        //                                            }
        //                                            if (currentLineTextSplit.Length >= 4)
        //                                            {
        //                                                fsbShipper.ShipperContactIdentifier = currentLineTextSplit[3] != "" ? currentLineTextSplit[3] : "";
        //                                            }
        //                                            if (currentLineTextSplit.Length >= 5)
        //                                            {
        //                                                fsbShipper.ShipperContactNumber = currentLineTextSplit[4] != "" ? currentLineTextSplit[4] : "";
        //                                            }
        //                                        }

        //                                    }
        //                                    catch (Exception ex)
        //                                    {
        //                                        clsLog.WriteLogAzure("Error in Decoding FSB Message Shipper(SHP) TAG " + ex.ToString());
        //                                    }
        //                                    break;
        //                                case "CNE":
        //                                    try
        //                                    {
        //                                        string currentline = string.Empty;
        //                                        currentline = ReadFile(tagName, strMessage.Replace("$", "\r\n").Replace("$", "\n"));
        //                                        string[] currentConsigneeLineText = currentline.Split('#');
        //                                        string cneTag = string.Empty;
        //                                        if (currentConsigneeLineText.Length >= 1)
        //                                        {
        //                                            string[] currentLineTextSplit = currentConsigneeLineText[0].Split('/');
        //                                            if (currentLineTextSplit.Length >= 1)
        //                                            {
        //                                                if (currentLineTextSplit[0].Length == 3)
        //                                                {
        //                                                    cneTag = currentLineTextSplit[0] != "" ? currentLineTextSplit[0] : "";
        //                                                }
        //                                            }
        //                                            if (currentLineTextSplit.Length >= 2)
        //                                            {
        //                                                fsbConsignee.ConsigneeAccountNo = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
        //                                            }
        //                                        }

        //                                        if (currentConsigneeLineText.Length >= 2)
        //                                        {

        //                                            string[] currentLineTextSplit = currentConsigneeLineText[1].Split('/');
        //                                            if (currentLineTextSplit.Length >= 2)
        //                                            {
        //                                                fsbConsignee.ConsigneeName = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
        //                                            }
        //                                        }

        //                                        if (currentConsigneeLineText.Length >= 3)
        //                                        {

        //                                            string[] currentLineTextSplit = currentConsigneeLineText[2].Split('/');
        //                                            if (currentLineTextSplit.Length >= 2)
        //                                            {
        //                                                fsbConsignee.ConsigneeStreetAddress = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
        //                                            }
        //                                        }
        //                                        if (currentConsigneeLineText.Length >= 4)
        //                                        {

        //                                            string[] currentLineTextSplit = currentConsigneeLineText[3].Split('/');
        //                                            if (currentLineTextSplit.Length >= 2)
        //                                            {
        //                                                fsbConsignee.ConsigneePlace = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
        //                                            }
        //                                            if (currentLineTextSplit.Length >= 3)
        //                                            {
        //                                                fsbConsignee.ConsigneeState = currentLineTextSplit[2] != "" ? currentLineTextSplit[2] : "";
        //                                            }
        //                                        }
        //                                        if (currentConsigneeLineText.Length >= 5)
        //                                        {
        //                                            string[] currentLineTextSplit = currentConsigneeLineText[4].Split('/');
        //                                            if (currentLineTextSplit.Length >= 2)
        //                                            {
        //                                                fsbConsignee.ConsigneeCountrycode = currentLineTextSplit[1] != "" ? currentLineTextSplit[1].Substring(0, 2) : "";
        //                                            }
        //                                            if (currentLineTextSplit.Length >= 3)
        //                                            {
        //                                                fsbConsignee.ConsigneePostalCode = currentLineTextSplit[2] != "" ? currentLineTextSplit[2] : "";
        //                                            }
        //                                            if (currentLineTextSplit.Length >= 4)
        //                                            {
        //                                                fsbConsignee.ConsigneeContactIdentifier = currentLineTextSplit[3] != "" ? currentLineTextSplit[3] : "";
        //                                            }
        //                                            if (currentLineTextSplit.Length >= 5)
        //                                            {
        //                                                fsbConsignee.ConsigneeContactNumber = currentLineTextSplit[4] != "" ? currentLineTextSplit[4] : "";
        //                                            }
        //                                        }
        //                                    }
        //                                    catch (Exception ex)
        //                                    {
        //                                        clsLog.WriteLogAzure("Error in Decoding FSB Message Consigneee(CNE) TAG " + ex.ToString());
        //                                    }
        //                                    break;
        //                                case "SSR":
        //                                    try
        //                                    {
        //                                        string currentSSRline = string.Empty;
        //                                        currentSSRline = ReadFile(tagName, strMessage.Replace("$", "\r\n").Replace("$", "\n"));
        //                                        string[] currentSSRLineText = currentSSRline.Split('#');
        //                                        if (currentSSRLineText.Length >= 1)
        //                                        {
        //                                            string[] currentLineTextSplit = currentSSRLineText[0].Split('/');
        //                                            if (currentLineTextSplit.Length >= 2)
        //                                            {
        //                                                fsbMessage.ShipmentSendgerReference1 = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
        //                                            }
        //                                        }

        //                                        if (currentSSRLineText.Length >= 2)
        //                                        {

        //                                            string[] currentLineTextSplit = currentSSRLineText[1].Split('/');
        //                                            if (currentLineTextSplit.Length >= 2)
        //                                            {
        //                                                fsbMessage.ShipmentSendgerReference2 = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
        //                                            }
        //                                        }
        //                                    }
        //                                    catch (Exception ex)
        //                                    {
        //                                        clsLog.WriteLogAzure("Error in Decoding FSB Message SSR TAG " + ex.ToString());
        //                                    }
        //                                    break;
        //                                case "WBL":
        //                                    try
        //                                    {
        //                                        string[] currentLineWBLText = lineText.Split('/');
        //                                        if (currentLineWBLText.Length <= 3)
        //                                        {
        //                                            if (currentLineWBLText.Length > 1)
        //                                            {
        //                                                if (currentLineWBLText.Length == 2)
        //                                                    fsbMessage.NatureofGoods1 = currentLineWBLText[1];

        //                                                if (currentLineWBLText[1].Length == 1)
        //                                                {
        //                                                    string strWBLIdentifier = currentLineWBLText[1].ToUpper();
        //                                                    switch (strWBLIdentifier.ToUpper())
        //                                                    {

        //                                                        case "G":
        //                                                            fsbMessage.NatureofGoods1 = currentLineWBLText[2];
        //                                                            break;
        //                                                        case "C":
        //                                                            fsbMessage.NatureofGoods1 = currentLineWBLText[2];
        //                                                            break;
        //                                                        case "V":
        //                                                            if (currentLineWBLText[2] != "")
        //                                                            {
        //                                                                fsbMessage.VolumeCode = currentLineWBLText[2].Substring(0, 2);
        //                                                                fsbMessage.AWBVolume = decimal.Parse(currentLineWBLText[2].Substring(2));
        //                                                            }
        //                                                            break;
        //                                                        case "U":
        //                                                            bupInformation = new MessageData.AWBBUPInformation();
        //                                                            bupInformation.ULDNo = currentLineWBLText[2];
        //                                                            break;
        //                                                        case "S":
        //                                                            bupInformation.SlacCount = currentLineWBLText[2];
        //                                                            bublistinformation.Add(bupInformation);
        //                                                            break;
        //                                                        default:
        //                                                            break;

        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                        if (currentLineWBLText.Length >= 4)
        //                                        {
        //                                            try
        //                                            {
        //                                                Dimension = new MessageData.FSBDimensionInformation();
        //                                                if (currentLineWBLText.Length >= 2)
        //                                                {
        //                                                    if (currentLineWBLText[1].ToUpper() == "D")
        //                                                    {
        //                                                        if (currentLineWBLText.Length >= 3)
        //                                                        {
        //                                                            if (currentLineWBLText[2] != "")
        //                                                            {
        //                                                                Dimension.DimWeightcode = currentLineWBLText[2].ToString().Substring(0, 1);
        //                                                                Dimension.DimGrossWeight = decimal.Parse(currentLineWBLText[2].ToString().Substring(1));
        //                                                            }

        //                                                        }
        //                                                        if (currentLineWBLText.Length >= 4)
        //                                                        {
        //                                                            Dimension.DimUnitCode = currentLineWBLText[3].ToString().Substring(0, 3);
        //                                                            string[] strDimTag = currentLineWBLText[3].ToString().Split('-');
        //                                                            Dimension.DimLength = int.Parse(strDimTag[0].Substring(3));
        //                                                            Dimension.DimWidth = int.Parse(strDimTag[1]);
        //                                                            Dimension.DimHeight = int.Parse(strDimTag[2]);

        //                                                        }
        //                                                        if (currentLineWBLText.Length >= 5)
        //                                                        {
        //                                                            Dimension.DimPieces = int.Parse(currentLineWBLText[4].ToString());
        //                                                        }

        //                                                        Dimensionformation.Add(Dimension);
        //                                                    }

        //                                                }
        //                                            }
        //                                            catch (Exception ex) { clsLog.WriteLogAzure("Error in Decoding FSB Message WBL Dimesnion TAG " + ex.ToString()); }
        //                                        }
        //                                    }
        //                                    catch (Exception ex) { clsLog.WriteLogAzure("Error in Decoding FSB Message WBL TAG " + ex.ToString()); }
        //                                    break;
        //                                case "OSI":
        //                                    try
        //                                    {
        //                                        string[] currentLineOSIText = lineText.Split('/');
        //                                        if (currentLineOSIText.Length > 1)
        //                                            fsbMessage.OtherServiceInformation = currentLineOSIText[1];
        //                                    }
        //                                    catch (Exception ex)
        //                                    {
        //                                        clsLog.WriteLogAzure("Error in Decoding FSB Message OSI TAG " + ex.ToString());
        //                                    }
        //                                    break;
        //                                case "COR":
        //                                    try
        //                                    {
        //                                        string[] currentLineCustomeText = lineText.Split('/');
        //                                        if (currentLineCustomeText.Length > 1)
        //                                            fsbMessage.CustomOrigin = currentLineCustomeText[1];
        //                                    }
        //                                    catch (Exception ex)
        //                                    {
        //                                        clsLog.WriteLogAzure("Error in Decoding FSB Message COR TAG " + ex.ToString());
        //                                    }
        //                                    break;
        //                                case "REF":
        //                                    try
        //                                    {
        //                                        string[] currentLineReferenceText = lineText.Split('/');
        //                                        if (currentLineReferenceText.Length >= 1)
        //                                            fsbMessage.RefereceOriginTag = currentLineReferenceText[1];
        //                                        if (currentLineReferenceText.Length >= 2)
        //                                            fsbMessage.RefereceFileTag = currentLineReferenceText[2];
        //                                    }
        //                                    catch (Exception ex)
        //                                    {
        //                                        clsLog.WriteLogAzure("Error in Decoding FSB Message REF TAG " + ex.ToString());
        //                                    }
        //                                    break;
        //                                case "SRI":
        //                                    try
        //                                    {
        //                                        string[] currentLineSRIText = lineText.Split('/');
        //                                        if (currentLineSRIText.Length >= 1)
        //                                            fsbMessage.ShipmentReferenceNumber = currentLineSRIText[1];
        //                                        if (currentLineSRIText.Length >= 2)
        //                                            fsbMessage.ShipmentSuplementyInformation = currentLineSRIText[2];
        //                                        if (currentLineSRIText.Length >= 3)
        //                                            fsbMessage.ShipmentSuplementyInformation1 = currentLineSRIText[3];
        //                                    }
        //                                    catch (Exception ex)
        //                                    {
        //                                        clsLog.WriteLogAzure("Error in Decoding FSB Message SRI TAG " + ex.ToString());
        //                                    }
        //                                    break;
        //                                default:
        //                                    break;
        //                            }
        //                        }
        //                        break;
        //                }
        //                lineNo++;

        //            }
        //        }
        //        flag = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        flag = false;
        //        clsLog.WriteLogAzure("Error in Decoding FSB Message " + ex.ToString());
        //    }
        //    return flag;
        //}

        //private static string ReadFile(string tagName, string strMessage)
        //{

        //    var fsbLine = new StringReader(strMessage);
        //    string lineText;
        //    var tagText = string.Empty;
        //    var readLine = false;
        //    while ((lineText = fsbLine.ReadLine()) != null)
        //    {
        //        if (readLine)
        //        {
        //            if (lineText.Trim().Length > 0)
        //                if (lineText.Substring(0, 1) == "/")
        //                    tagText += "#" + lineText;
        //                else
        //                    break;
        //        }
        //        if (lineText.Trim().Length > 2)
        //            if (lineText.Substring(0, 3) == tagName)
        //            {
        //                tagText = lineText;
        //                readLine = true;
        //            }
        //    }
        //    return tagText;
        //}

        //#endregion

        //SCM messaging

        #region Decode SCM message
        public static bool decodereceiveSCM(string SCMmsg, ref MessageData.SCMInfo[] scm)
        {
            bool flag = false;
            try
            {
                string movement = "", date = "", stationcode = "", lastrec = "";
                try
                {
                    if (SCMmsg.StartsWith("SCM", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] str = SCMmsg.Split('$');
                        if (str.Length > 2)
                        {
                            if (str[0].StartsWith("SCM", StringComparison.OrdinalIgnoreCase))
                            {
                                for (int i = 1; i < str.Length; i++)
                                {
                                    #region Line 1
                                    if (i == 1)
                                    {
                                        string[] splitStr = str[i].Split('.');
                                        try
                                        {
                                            //Line 1
                                            for (int j = 0; j < splitStr.Length; j++)
                                            {
                                                if (splitStr.Length > 0 && j == 0)
                                                {
                                                    //for Station Code
                                                    //scmdata.StationCode = splitStr[0];
                                                    stationcode = splitStr[0];
                                                }

                                                if (splitStr[j].Contains('/'))
                                                {
                                                    string[] splitdatetime = splitStr[j].Split('/');
                                                    if (splitdatetime.Length > 0)
                                                    {
                                                        //Date
                                                        //scmdata.Date = Convert.ToDateTime(splitdatetime[0]).ToString("MM/dd/yyyy");
                                                        //Appending Time
                                                        //scmdata.Date = scmdata.Date + " " + splitdatetime[1].Substring(0, 2) + ":" + splitdatetime[1].Substring(2) + ":00";
                                                        date = Convert.ToDateTime(splitdatetime[0]).ToString("MM/dd/yyyy");
                                                        date = date + " " + splitdatetime[1].Substring(0, 2) + ":" + splitdatetime[1].Substring(2) + ":00";
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // clsLog.WriteLogAzure(ex.Message); 
                                            _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        }
                                    }
                                    #endregion

                                    #region Line 2
                                    if (str[i].StartsWith(".", StringComparison.OrdinalIgnoreCase) && lastrec != "SI")
                                    {
                                        string[] splitStr = str[i].Split('.');
                                        string uldtype = string.Empty;
                                        if (splitStr.Length > 0)
                                        {
                                            for (int k = 0; k < splitStr.Length; k++)
                                            {
                                                if (splitStr[k].Length > 0)
                                                {

                                                    if (splitStr[k].ToString().Contains('/') || k == 2)
                                                    {
                                                        string[] localStr = splitStr[k].ToString().Split('/');
                                                        if (localStr[0].Trim().Length > 2)
                                                        {
                                                            for (int l = 0; l < localStr.Length; l++)
                                                            {
                                                                MessageData.SCMInfo scmdata = new MessageData.SCMInfo("");
                                                                scmdata.Date = date;
                                                                scmdata.StationCode = stationcode;
                                                                scmdata.uldno = uldtype + localStr[l];
                                                                scmdata.uldtype = uldtype;
                                                                scmdata.uldsrno = localStr[l].Substring(0, localStr[l].Length - 2);
                                                                scmdata.uldowner = localStr[l].Substring(localStr[l].Length - 2, 2);
                                                                scmdata.movement = movement;
                                                                scmdata.uldstatus = "S";
                                                                Array.Resize(ref scm, scm.Length + 1);
                                                                scm[scm.Length - 1] = scmdata;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!splitStr[k].ToString().Contains('T'))
                                                        {
                                                            uldtype = splitStr[k];
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    #region Line 3                 

                                    if (str[i].StartsWith("SI", StringComparison.OrdinalIgnoreCase) || lastrec == "SI")
                                    {
                                        if (str[i].StartsWith("SI", StringComparison.OrdinalIgnoreCase))
                                        {
                                            i++;
                                        }
                                        lastrec = "SI";
                                        if (i < str.Length)
                                        {
                                            string[] splitStr = str[i].Split('.');
                                            string uldtype = string.Empty;
                                            if (splitStr.Length > 0)
                                            {
                                                for (int k = 0; k < splitStr.Length; k++)
                                                {
                                                    if (splitStr[k].Length > 0)
                                                    {
                                                        if (splitStr[k].ToString().Contains('/') || k == 2)
                                                        {
                                                            string[] localStr = splitStr[k].ToString().Split('/');
                                                            if (localStr[0].Trim().Length > 2)
                                                            {
                                                                for (int l = 0; l < localStr.Length; l++)
                                                                {
                                                                    MessageData.SCMInfo scmdata = new MessageData.SCMInfo("");
                                                                    for (int p = 0; p < scm.Length; p++)
                                                                    {
                                                                        if (scm[p].uldno == uldtype + localStr[l])
                                                                        {
                                                                            scm[p].uldstatus = "D";
                                                                            break;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (!splitStr[k].ToString().Contains('T'))
                                                            {
                                                                uldtype = splitStr[k];
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    /////
                                }
                            }
                        }
                    }
                    flag = true;
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;
        }
        #endregion

        public async Task<string> EncodeUWS(string DepartureAirport, string FlightNo, DateTime FlightDate, string MsgVer, string UWSConfig)
        {
            string UWSMsg = "";
            try
            {
                GenericFunction genericFunction = new GenericFunction();

                bool IsThreeDigitNo = Convert.ToBoolean(genericFunction.GetConfigurationValues("IsThreeDigitNo") == null ? "false" : genericFunction.GetConfigurationValues("IsThreeDigitNo"));
                MessageData.UWSinfo objFFMInfo = new MessageData.UWSinfo();
                MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
                MessageData.consignmnetinfo[] objConsInfoCart = new MessageData.consignmnetinfo[0];
                MessageData.unloadingport[] objUnloadingPort = new MessageData.unloadingport[0];
                MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
                List<string> SI = new List<string>();
                DataSet dsData = new DataSet();
                DataSet ds = new DataSet();
                int count1 = 0, bulkcontr = 0;
                string totalwt = string.Empty, spclhandlingcode = string.Empty, loadingpr = string.Empty, volumecode = string.Empty, Remark = string.Empty
                    , loadcatcode1 = string.Empty, palletcontrcode = string.Empty, palletcontrnumber = string.Empty, dest = string.Empty;

                DataTable bulkinfo = new DataTable();

                ds = await getFFMUnloadingPort(DepartureAirport, FlightNo, FlightDate);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    objFFMInfo.carriercode = FlightNo.Substring(0, 2);

                    objFFMInfo.fltnum = FlightNo.Substring(2, FlightNo.Length - 2);
                    objFFMInfo.fltairportcode = DepartureAirport;
                    bool checknilFlight = false;

                    dsData = await GetRecordforGenerateUWS(DepartureAirport, FlightNo, FlightDate, string.Empty);

                    //start changes to filter ulds weight
                    if (dsData != null && dsData.Tables.Count > 0 && dsData.Tables[0].Rows.Count > 0)
                    {
                        if (dsData != null && dsData.Tables.Count > 0 && dsData.Tables[0].Rows.Count > 0 && dsData.Tables[0].Columns.Count == 4)
                        {
                            MessageData.unloadingport objTempUnloadingPort = new MessageData.unloadingport("");
                            if (!checknilFlight)
                            {
                                count1++;
                                checknilFlight = true;
                                objTempUnloadingPort.nilcargocode = "NIL";
                                objTempUnloadingPort.unloadingairport = dsData.Tables[0].Rows[0]["POU"].ToString();
                                DateTime dtFlight = Convert.ToDateTime(dsData.Tables[0].Rows[0]["FlightDate"]);
                                objFFMInfo.fltdate = dtFlight.ToString("dd");
                                Array.Resize(ref objUnloadingPort, count1);
                                objUnloadingPort[count1 - 1] = objTempUnloadingPort;
                            }
                        }
                        else
                        {
                            DataTable dt = new DataTable();
                            dt.Columns.Add("ULDNo");
                            int size = 0;
                            int leg = 0;

                            DateTime dtFlight = Convert.ToDateTime(dsData.Tables[0].Rows[0]["FlightDate"]);
                            objFFMInfo.fltdate = dtFlight.ToString("dd");

                            #region multiple destination ULDS
                            Array.Resize(ref objUnloadingPort, ds.Tables[0].Rows.Count);

                            MessageData.consignmnetinfo[] LstConsignmentBulk = new MessageData.consignmnetinfo[0];
                            MessageData.consignmnetinfo[] LstConsignmentCart = new MessageData.consignmnetinfo[0];

                            for (int i = 0; i < dsData.Tables[0].Rows.Count; i++)
                            {
                                if (dsData.Tables[0].Rows[i]["LoadingPriority"].ToString() == "")
                                    dsData.Tables[0].Rows[i]["LoadingPriority"] = "ZZZLeastPriority";
                            }
                            foreach (DataRow drdestination in ds.Tables[0].Rows)
                            {
                                MessageData.unloadingport objTempUnloadingPort = new MessageData.unloadingport("");

                                DataRow[] duwsrow = dsData.Tables[0].Select("POU='" + drdestination[0] + "' AND ULDNo<>'BULK'", "LoadingPriority ASC");
                                DataRow[] bulkinfo1 = dsData.Tables[0].Select("POU='" + drdestination[0] + "'AND ULDNo='BULK'", "LoadingPriority ASC");

                                if (duwsrow.Length <= 0 && bulkinfo1.Length <= 0)
                                {
                                    if (!checknilFlight)
                                    {
                                        count1++;
                                        checknilFlight = true;
                                        objTempUnloadingPort.nilcargocode = "NIL";
                                        objTempUnloadingPort.unloadingairport = drdestination[0].ToString();
                                        Array.Resize(ref objUnloadingPort, count1);
                                        objUnloadingPort[count1 - 1] = objTempUnloadingPort;
                                    }
                                }
                                else
                                {
                                    objTempUnloadingPort.unloadingairport = drdestination[0].ToString();
                                    objUnloadingPort[leg++] = objTempUnloadingPort;

                                    checknilFlight = true;
                                    DataTable dtUldRecord = new DataTable();
                                    dtUldRecord.Columns.Add("ULDNo");
                                    dtUldRecord.Columns.Add("LoadingPriority");
                                    foreach (DataRow druldNo in duwsrow)
                                    {
                                        DataRow newCustomersRow = dtUldRecord.NewRow();
                                        newCustomersRow["ULDNo"] = druldNo["ULDNo"].ToString();
                                        newCustomersRow["LoadingPriority"] = druldNo["LoadingPriority"].ToString();
                                        dtUldRecord.Rows.Add(newCustomersRow);
                                    }

                                    //all ulds at that destination first leg and second leg etc
                                    DataTable distinctDT1 = SelectDistinct(dtUldRecord, "ULDNo");
                                    distinctDT1.Columns.Add("LoadingPriority");
                                    foreach (DataRow dr1 in dtUldRecord.Rows)
                                    {
                                        for (int i = 0; i < distinctDT1.Rows.Count; i++)
                                        {
                                            if (dr1["ULDNo"].ToString() == distinctDT1.Rows[i]["ULDNo"].ToString())
                                                distinctDT1.Rows[i]["LoadingPriority"] = dr1["LoadingPriority"].ToString();
                                        }

                                    }
                                    distinctDT1.DefaultView.Sort = "LoadingPriority ASC";
                                    distinctDT1 = (distinctDT1.DefaultView).ToTable();

                                    Array.Resize(ref objULDInfo, size);

                                    #region Distinct ULDs
                                    for (int rx = 0; rx < distinctDT1.Rows.Count; rx++)
                                    {
                                        MessageData.ULDinfo objTempULDInfo = new MessageData.ULDinfo("");
                                        totalwt = spclhandlingcode = volumecode = dest = string.Empty;

                                        ///destination whre POU=dest1
                                        foreach (DataRow dr in duwsrow)
                                        {
                                            if (dr["ULDNo"].ToString() == distinctDT1.Rows[rx]["ULDNo"].ToString())
                                            {
                                                totalwt = dr["ScaleWeight"].ToString();
                                                spclhandlingcode = dr["SHCCodes"].ToString();
                                                volumecode = volumecode + "/" + dr["VolumeCode"];
                                                dest = dr["POU"].ToString();
                                                palletcontrcode = dr["ContourCode"].ToString();
                                                palletcontrnumber = dr["ContourNumber"].ToString();
                                                Remark = dr["Remarks"].ToString();
                                                loadcatcode1 = dr["LoadingCatCode"].ToString();
                                                SI.Add(Remark);
                                                loadingpr = dr["LoadingPriority"].ToString().Replace("ZZZLeastPriority", "");
                                            }
                                        }
                                        size++;
                                        objTempULDInfo.uldsrno = distinctDT1.Rows[rx]["ULDNo"].ToString();
                                        objTempULDInfo.uldweight = (Convert.ToInt64(Convert.ToDouble(totalwt.ToString()))).ToString();
                                        objTempULDInfo.specialloadcode = spclhandlingcode;
                                        objTempULDInfo.volumecode = volumecode;
                                        objTempULDInfo.ulddestination = dest;
                                        objTempULDInfo.loadcatagorycode1 = loadcatcode1;
                                        objTempULDInfo.contorcode = palletcontrcode;
                                        objTempULDInfo.contornumber = palletcontrnumber;
                                        objTempULDInfo.remark = Remark;
                                        objTempULDInfo.loadingpriority = loadingpr;
                                        objTempULDInfo.loadcatagorycode1 = loadcatcode1;

                                        Array.Resize(ref objULDInfo, size);
                                        objULDInfo[size - 1] = objTempULDInfo;
                                    }
                                    #endregion Distinct ULDs

                                    #region Get bulk info
                                    try
                                    {
                                        foreach (DataRow dr in bulkinfo1)
                                        {
                                            bulkcontr++;
                                            MessageData.consignmnetinfo objTempConsInfo = new MessageData.consignmnetinfo("");
                                            objTempConsInfo.airlineprefix = dr["AWBprefix"].ToString();
                                            objTempConsInfo.awbnum = dr["AWBNumber"].ToString();
                                            objTempConsInfo.origin = dr["AWBOrigin"].ToString().Trim();
                                            objTempConsInfo.dest = dr["POU"].ToString().Trim();
                                            objTempConsInfo.consigntype = dr["consigntype"].ToString();
                                            objTempConsInfo.pcscnt = dr["ManifestedPcs"].ToString().Trim();
                                            objTempConsInfo.weightcode = dr["UOM"].ToString();
                                            objTempConsInfo.weight = dr["ManifestedGrWt"].ToString().Trim();
                                            objTempConsInfo.volumecode = dr["VolumeCode"].ToString().ToUpper().Trim();
                                            objTempConsInfo.volumeamt = dr["Volume"].ToString();
                                            objTempConsInfo.splhandling = dr["SHCCodes"].ToString();
                                            objTempConsInfo.scaleweight = dr["ScaleWeight"].ToString();
                                            objTempConsInfo.CartNumber = dr["CartNumber"].ToString();

                                            decimal wt = Convert.ToDecimal(objTempConsInfo.weight);

                                            objTempConsInfo.weight = (Convert.ToInt64(Convert.ToDouble(wt.ToString()))).ToString();//(Convert.ToInt64(Convert.ToDouble(objTempConsInfo.weight.ToString()))).ToString();
                                            objTempConsInfo.Remark = dr["Remarks"].ToString();
                                            objTempConsInfo.loadcatcode1 = dr["LoadingCatCode"].ToString();
                                            SI.Add(objTempConsInfo.Remark);
                                            objTempConsInfo.loadingpriority = dr["LoadingPriority"].ToString().Replace("ZZZLeastPriority", "");

                                            bool CartExists = false;
                                            if (LstConsignmentBulk != null && LstConsignmentBulk.Length > 0)
                                            {
                                                for (int intCount = 0; intCount < LstConsignmentBulk.Length; intCount++)
                                                {
                                                    if (LstConsignmentBulk[intCount].dest == objTempConsInfo.dest)
                                                    {
                                                        if (objTempConsInfo.splhandling.Trim() != string.Empty && !LstConsignmentBulk[intCount].splhandling.Contains(objTempConsInfo.splhandling.Trim()))
                                                        {
                                                            if (LstConsignmentBulk[intCount].splhandling != "")
                                                                LstConsignmentBulk[intCount].splhandling = LstConsignmentBulk[intCount].splhandling + "." + objTempConsInfo.splhandling;
                                                            else
                                                                LstConsignmentBulk[intCount].splhandling = objTempConsInfo.splhandling.Trim();
                                                        }
                                                        LstConsignmentBulk[intCount].weight = (Convert.ToDouble(LstConsignmentBulk[intCount].weight) + Convert.ToDouble(objTempConsInfo.weight)).ToString();
                                                        LstConsignmentBulk[intCount].volumeamt = (Convert.ToDouble(LstConsignmentBulk[intCount].volumeamt) + Convert.ToDouble(objTempConsInfo.volumeamt)).ToString();
                                                        CartExists = true;
                                                    }
                                                }
                                            }
                                            if (CartExists == false)
                                            {
                                                Array.Resize(ref LstConsignmentBulk, LstConsignmentBulk.Length + 1);
                                                LstConsignmentBulk[LstConsignmentBulk.Length - 1] = objTempConsInfo;
                                            }
                                        }
                                        objConsInfo = LstConsignmentBulk;
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                    #endregion Get bulk info

                                    #region Get Cart info
                                    try
                                    {
                                        foreach (DataRow dr in bulkinfo1)
                                        {
                                            bulkcontr++;
                                            MessageData.consignmnetinfo objTempConsInfo = new MessageData.consignmnetinfo("");
                                            objTempConsInfo.airlineprefix = dr["AWBprefix"].ToString();
                                            objTempConsInfo.awbnum = dr["AWBNumber"].ToString();
                                            objTempConsInfo.origin = dr["AWBOrigin"].ToString().Trim();
                                            objTempConsInfo.dest = dr["POU"].ToString().Trim();
                                            objTempConsInfo.consigntype = dr["consigntype"].ToString();
                                            objTempConsInfo.pcscnt = dr["ManifestedPcs"].ToString().Trim();
                                            objTempConsInfo.weightcode = dr["UOM"].ToString();
                                            objTempConsInfo.weight = dr["ManifestedGrWt"].ToString().Trim();
                                            objTempConsInfo.volumecode = dr["VolumeCode"].ToString().ToUpper().Trim();
                                            objTempConsInfo.volumeamt = dr["Volume"].ToString();
                                            objTempConsInfo.splhandling = dr["SHCCodes"].ToString();
                                            objTempConsInfo.scaleweight = dr["ScaleWeight"].ToString();
                                            objTempConsInfo.CartNumber = dr["CartNumber"].ToString();

                                            decimal wt = Convert.ToDecimal(objTempConsInfo.weight);

                                            objTempConsInfo.weight = (Convert.ToInt64(Convert.ToDouble(wt.ToString()))).ToString();
                                            objTempConsInfo.Remark = dr["Remarks"].ToString();
                                            objTempConsInfo.loadcatcode1 = dr["LoadingCatCode"].ToString();
                                            SI.Add(objTempConsInfo.Remark);
                                            objTempConsInfo.loadingpriority = dr["LoadingPriority"].ToString().Replace("ZZZLeastPriority", "");

                                            bool CartExists = false;
                                            if (LstConsignmentCart != null && LstConsignmentCart.Length > 0)
                                            {
                                                for (int intCount = 0; intCount < LstConsignmentCart.Length; intCount++)
                                                {
                                                    if (LstConsignmentCart[intCount].CartNumber == objTempConsInfo.CartNumber)
                                                    {
                                                        if (objTempConsInfo.splhandling.Trim() != string.Empty && !LstConsignmentCart[intCount].splhandling.Contains(objTempConsInfo.splhandling.Trim()))
                                                        {
                                                            if (LstConsignmentCart[intCount].splhandling != "")
                                                                LstConsignmentCart[intCount].splhandling = LstConsignmentCart[intCount].splhandling + "." + objTempConsInfo.splhandling;
                                                            else
                                                                LstConsignmentCart[intCount].splhandling = objTempConsInfo.splhandling.Trim();
                                                        }
                                                        LstConsignmentCart[intCount].weight = (Convert.ToDouble(LstConsignmentCart[intCount].weight) + Convert.ToDouble(objTempConsInfo.weight)).ToString();
                                                        CartExists = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (CartExists == false)
                                            {
                                                Array.Resize(ref LstConsignmentCart, LstConsignmentCart.Length + 1);
                                                LstConsignmentCart[LstConsignmentCart.Length - 1] = objTempConsInfo;
                                            }
                                        }
                                        objConsInfoCart = LstConsignmentCart;
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                    #endregion Get Cart info
                                }
                            }
                            #endregion multiple destination ULDS
                        }
                        try
                        {
                            UWSMsg = EncodeUWSforsend(DepartureAirport, ref objFFMInfo, ref objUnloadingPort, ref objULDInfo, ref objConsInfo, MsgVer, SI, ref objConsInfoCart, UWSConfig, IsThreeDigitNo);
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }
                        if (UWSMsg != null)
                        {
                            if (UWSMsg.Trim() != "" && UWSMsg.Length > 0)
                            {
                                return UWSMsg;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return UWSMsg;
        }

        public static string EncodeUWSforsend(string pol, ref MessageData.UWSinfo UWSdata, ref MessageData.unloadingport[] unloadingport, ref MessageData.ULDinfo[] uld, ref MessageData.consignmnetinfo[] bulkinfo, string str, List<string> suppInfo, ref MessageData.consignmnetinfo[] Cartinfo, string UWSConfig, bool IsThreeDigitNo = false)
        {
            string UWS = null;
            //bool multidest = false;
            string loadcatcode = "P";
            StringBuilder uldstr = new StringBuilder("");
            StringBuilder bulkstr = new StringBuilder("");
            StringBuilder cartstr = new StringBuilder("");
            StringBuilder si = new StringBuilder("");
            StringBuilder strline1 = new StringBuilder("");
            StringBuilder nicargocond = new StringBuilder("");
            try
            {

                #region Line 1
                strline1.Append("UWS" + "$");
                #endregion

                #region Line 2 & 3

                //strline1.Append(UWSdata.carriercode + UWSdata.fltnum + "/" + UWSdata.fltdate + "." + pol);
                //strline1.Append(UWSdata.carriercode + UWSdata.fltnum + "/" + UWSdata.fltdate + "$");
                //strline1.Append("." + pol + "$");

                #endregion
                //Final Message indication (not required if message contains provisional data)

                if (UWSdata.fltnum.Length < 3 && pol == "DXB" && IsThreeDigitNo == true)
                {
                    UWSdata.fltnum = UWSdata.fltnum.PadLeft(3, '0');
                }
                if (str == "1")
                {
                    strline1.Append(UWSdata.carriercode + UWSdata.fltnum + "/" + UWSdata.fltdate + "." + pol + ".FINAL$");
                    loadcatcode = "A";
                }
                else
                {
                    strline1.Append(UWSdata.carriercode + UWSdata.fltnum + "/" + UWSdata.fltdate + "." + pol);
                    strline1.Append("$");
                }

                if (unloadingport.Length > 0)
                {
                    foreach (MessageData.unloadingport up in unloadingport)
                    {
                        if (up.nilcargocode == "NIL")
                        {
                            nicargocond.Append("$" + "-" + up.unloadingairport + "/0" + loadcatcode + "/C");
                        }
                        else
                        {
                            if (bulkinfo.Length <= 0)
                            {
                                bulkstr.Append("BULK" + "$" + "-" + up.unloadingairport + "/0" + loadcatcode + "/C");
                            }
                        }
                    }
                }


                #region Line 3 Load ULD and BULK for single destination

                if (uld.Length > 0)
                {
                    for (int i = 0; i < uld.Length; i++)
                    {
                        if (uld[i].uldno != null && uld[i].uldno != "NULL")
                        {
                            //decimal wt = 0;
                            //if (uld[i].uldweight != null && uld[i].uldweight != "" && Convert.ToDecimal(uld[i].uldweight) > 0)
                            //{
                            //    wt = Convert.ToDecimal(uld[i].uldweight);
                            //    if (wt < 1)
                            //    {
                            //        wt = 1;
                            //    }
                            //}
                            uldstr.Append("-" + uld[i].uldtype + uld[i].uldsrno + uld[i].uldowner + "/" + uld[i].ulddestination + "/" + uld[i].uldweight + (str == "1" ? "A" : "P") + "/" + (uld[i].loadcatagorycode1.Length > 0 ? (uld[i].loadcatagorycode1.ToString()) : "C") + (uld[i].loadingpriority.Length > 0 ? (".PRI" + uld[i].loadingpriority) : "") + (uld[i].specialloadcode.Length > 0 ? ("." + uld[i].specialloadcode.Replace(',', '.')) : ""));
                            //uldstr.Append("-" + uld[i].uldtype + uld[i].uldsrno + uld[i].uldowner + "/" + uld[i].ulddestination + "/" + wt.ToString() + (str == "1" ? "A" : "P") + "/" + (uld[i].loadcatagorycode1.Length > 0 ? (uld[i].loadcatagorycode1.ToString()) : "C") + (uld[i].loadingpriority.Length > 0 ? (".PRI" + uld[i].loadingpriority) : "") + (uld[i].specialloadcode.Length > 0 ? ("." + uld[i].specialloadcode.Replace(',', '.')) : ""));
                            uldstr.Append("$");
                        }
                    }
                }
                if (bulkinfo.Length > 0)
                {
                    bulkstr.Append("BULK$");
                    for (int i = 0; i < bulkinfo.Length; i++)
                    {
                        decimal wt = 0;
                        if (bulkinfo[i].weight != null && bulkinfo[i].weight != "")
                        {
                            wt = Convert.ToDecimal(bulkinfo[i].weight);
                            if (wt < 1)
                            {
                                wt = 1;
                            }
                        }

                        //bulkstr.Append("-" + bulkinfo[i].dest + "/" + bulkinfo[i].weight + (str == "1" ? "A" : "P") + "/" + (bulkinfo[i].loadcatcode1.Length > 0 ? (bulkinfo[i].loadcatcode1.ToString()) : "C") + (bulkinfo[i].loadingpriority.Length > 0 ? (".PRI" + bulkinfo[i].loadingpriority.ToString()) : "") + (bulkinfo[i].splhandling.Length > 0 ? ("." + bulkinfo[i].splhandling.ToString()) : ""));  //+ (bulkinfo[i].loadingpriority.Length > 0 ? ("/" + bulkinfo[i].loadingpriority.ToString()) : "") + (bulkinfo[i].splhandling.Length > 0 ? ("/" + bulkinfo[i].splhandling.ToString()) : "") + (bulkinfo[i].specialloadremark.Length > 0 ? ("/" + bulkinfo[i].specialloadremark.ToString()) : ""));// + ((uld[i].loadcategorycode2).Length > 0 ? ("/" + uld[i].loadcategorycode2) : "") + bulkinfo[i].splhandling + ((uld[i].specialloadremark.Length > 0) ? ("/" + uld[i].specialloadremark) : ""));
                        bulkstr.Append("-" + bulkinfo[i].dest + "/" + wt.ToString() + (str == "1" ? "A" : "P") + "/" + bulkinfo[i].volumeamt + "/" + (bulkinfo[i].loadcatcode1.Length > 0 ? (bulkinfo[i].loadcatcode1.ToString()) : "C") + (bulkinfo[i].loadingpriority.Length > 0 ? (".PRI" + bulkinfo[i].loadingpriority.ToString()) : "") + (bulkinfo[i].splhandling.Length > 0 ? ("." + bulkinfo[i].splhandling.ToString()) : ""));
                        bulkstr.Append("$");
                    }
                }

                #region : Cart :

                //if (Cartinfo.Length > 0)
                //{
                //    cartstr.Append("CAR$");
                //    for (int i = 0; i < Cartinfo.Length; i++)
                //    {
                //        string cartNumber = string.Empty;
                //        if (Cartinfo[i].CartNumber.Contains("-"))
                //        {
                //            cartNumber = Cartinfo[i].CartNumber;
                //            cartNumber = cartNumber.Replace("-", string.Empty);
                //        }
                //        else { cartNumber = Cartinfo[i].CartNumber; }

                //        cartstr.Append("-" + cartNumber + "/" + Cartinfo[i].dest + "/" + Cartinfo[i].weight + (str == "1" ? "A" : "P") + "/" + (Cartinfo[i].loadcatcode1.Length > 0 ? (Cartinfo[i].loadcatcode1.ToString()) : "C") + (Cartinfo[i].loadingpriority.Length > 0 ? (".PRI" + Cartinfo[i].loadingpriority.ToString()) : "") + (Cartinfo[i].splhandling.Length > 0 ? ("." + Cartinfo[i].splhandling.ToString()) : ""));  //+ (bulkinfo[i].loadingpriority.Length > 0 ? ("/" + bulkinfo[i].loadingpriority.ToString()) : "") + (bulkinfo[i].splhandling.Length > 0 ? ("/" + bulkinfo[i].splhandling.ToString()) : "") + (bulkinfo[i].specialloadremark.Length > 0 ? ("/" + bulkinfo[i].specialloadremark.ToString()) : ""));// + ((uld[i].loadcategorycode2).Length > 0 ? ("/" + uld[i].loadcategorycode2) : "") + bulkinfo[i].splhandling + ((uld[i].specialloadremark.Length > 0) ? ("/" + uld[i].specialloadremark) : ""));
                //        cartstr.Append("$");
                //    }
                //}

                #endregion

                #region SiCart

                if (Cartinfo.Length > 0)
                {
                    si.Append("SI" + "  ");

                    for (int i = 0; i < Cartinfo.Length; i++)
                    {
                        string cartNumber = string.Empty;
                        if (Cartinfo[i].CartNumber.Contains("-"))
                        {
                            cartNumber = Cartinfo[i].CartNumber;
                            cartNumber = cartNumber.Replace("-", string.Empty);
                        }
                        else { cartNumber = Cartinfo[i].CartNumber; }

                        decimal wt = 0;
                        if (Cartinfo[i].weight != null && Cartinfo[i].weight != "")
                        {
                            wt = Convert.ToDecimal(Cartinfo[i].weight);
                            if (wt < 1)
                            {
                                wt = 1;
                            }
                        }

                        if (UWSConfig.ToUpper() == "TRUE")
                        {
                            //si.Append("-" + cartNumber + "/" + Cartinfo[i].dest + "/" + Cartinfo[i].weight + (str == "1" ? "A" : "P") + "/" + (Cartinfo[i].loadcatcode1.Length > 0 ? (Cartinfo[i].loadcatcode1.ToString()) : "C") + (Cartinfo[i].loadingpriority.Length > 0 ? (".PRI" + Cartinfo[i].loadingpriority.ToString()) : "") + (Cartinfo[i].splhandling.Length > 0 ? ("." + Cartinfo[i].splhandling.ToString()) : ""));  //+ (bulkinfo[i].loadingpriority.Length > 0 ? ("/" + bulkinfo[i].loadingpriority.ToString()) : "") + (bulkinfo[i].splhandling.Length > 0 ? ("/" + bulkinfo[i].splhandling.ToString()) : "") + (bulkinfo[i].specialloadremark.Length > 0 ? ("/" + bulkinfo[i].specialloadremark.ToString()) : ""));// + ((uld[i].loadcategorycode2).Length > 0 ? ("/" + uld[i].loadcategorycode2) : "") + bulkinfo[i].splhandling + ((uld[i].specialloadremark.Length > 0) ? ("/" + uld[i].specialloadremark) : ""));
                            si.Append("-" + cartNumber + "/" + Cartinfo[i].dest + "/" + wt.ToString() + (str == "1" ? "A" : "P") + "/" + Cartinfo[i].volumeamt + "/" + (Cartinfo[i].loadcatcode1.Length > 0 ? (Cartinfo[i].loadcatcode1.ToString()) : "C") + (Cartinfo[i].loadingpriority.Length > 0 ? (".PRI" + Cartinfo[i].loadingpriority.ToString()) : "") + (Cartinfo[i].splhandling.Length > 0 ? ("." + Cartinfo[i].splhandling.ToString()) : ""));
                        }
                        else
                        {
                            si.Append("-" + cartNumber + "/" + Cartinfo[i].dest + "/" + wt.ToString() + (str == "1" ? "A" : "P") + "/" + (Cartinfo[i].loadcatcode1.Length > 0 ? (Cartinfo[i].loadcatcode1.ToString()) : "C") + (Cartinfo[i].loadingpriority.Length > 0 ? (".PRI" + Cartinfo[i].loadingpriority.ToString()) : "") + (Cartinfo[i].splhandling.Length > 0 ? ("." + Cartinfo[i].splhandling.ToString()) : ""));
                        }
                        si.Append("$");
                    }


                    if (suppInfo.Count > 0 && suppInfo[0] != "")
                    {
                        si.Append("$" + suppInfo[0]);

                        for (int cnt = 1; cnt < suppInfo.Count; cnt++)
                        {
                            si.Append("$" + suppInfo[cnt]);
                        }
                    }

                }
                #endregion

                #region : SI :
                //SI
                //if (suppInfo.Count > 0 && suppInfo[0] != "")
                //{
                //    si.Append("SI" + "  " + suppInfo[0]);

                //    for (int cnt = 1; cnt < suppInfo.Count; cnt++)
                //    {
                //        si.Append("$" + suppInfo[cnt]);
                //    }
                //}
                #endregion

                nicargocond.Replace("$$", "$");
                nicargocond.Replace("$", "\r\n");
                strline1.Replace("$$", "$");
                strline1.Replace("$", "\r\n");
                bulkstr.Replace("$$", "$");
                bulkstr.Replace("$", "\r\n");
                uldstr.Replace("$$", "$");
                uldstr.Replace("$", "\r\n");

                si.Replace("$$", "$");
                si.Replace("$", "\r\n");

                cartstr.Replace("$$", "$");
                cartstr.Replace("$", "\r\n");

                #endregion

                #region BuildUWS

                if (nicargocond.Length > 0)
                {
                    UWS = strline1 + (nicargocond.Length > 0 ? ("BULK" + nicargocond + "\r\n") : "");
                }
                else
                {
                    //UWS = strline1 + uldstr.ToString() + bulkstr + cartstr + si + "\r\n";
                    UWS = strline1 + uldstr.ToString() + bulkstr + si + "\r\n";
                }

                #endregion
            }
            catch (Exception ex)
            {
                UWS = "ERR";
                // clsLog.WriteLogAzure(ex);
                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return UWS;
        }

        public DataTable SelectDistinct(DataTable SourceTable, string FieldName)
        {
           try
           {
             // Create a Datatable â€“ datatype same as FieldName
             DataTable dt = new DataTable(SourceTable.TableName);
             dt.Columns.Add(FieldName, SourceTable.Columns[FieldName].DataType);
             // Loop each row & compare each value with one another
             // Add it to datatable if the values are mismatch
             object LastValue = null;
             foreach (DataRow dr in SourceTable.Select("", FieldName))
             {
                 if (LastValue == null || !(ColumnEqual(LastValue, dr[FieldName])))
                 {
                     LastValue = dr[FieldName];
                     dt.Rows.Add(new object[] { LastValue });
                 }
             }
             return dt;
           }
           catch (System.Exception ex)
           {
            _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            throw;
           }
        }

        private bool ColumnEqual(object A, object B)
        {
            // Compares two values to see if they are equal. Also compares DBNULL.Value.           
            if (A == DBNull.Value && B == DBNull.Value) //  both are DBNull.Value
                return true;
            if (A == DBNull.Value || B == DBNull.Value) //  only one is BNull.Value
                return false;
            return (A.Equals(B));  // value type standard comparison
        }

        public async Task<DataSet> GetRecordforGenerateUWS(string DepartureAirport, string FlightNo, DateTime FlightDate, string FlightDestination)
        {
            DataSet? dsData = new DataSet();
            try
            {
                //SQLServer dtb = new SQLServer();

                //string[] paramname = new string[] { "FltNo",
                //                                "ManifestdateFrom",
                //                                "ManifestdateTo",
                //                                "DepartureAirport" ,
                //                                "FlightDestination"};

                //object[] paramvalue = new object[] { FlightNo,
                //                                 newFlightDate,
                //                                 "",
                //                                 DepartureAirport,FlightDestination };

                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar,
                //                                      SqlDbType.DateTime,
                //                                      SqlDbType.VarChar,
                //                                      SqlDbType.VarChar,SqlDbType.VarChar};

                //dsData = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);

                DateTime newFlightDate = FlightDate;
                string procedure = "GetFlightRecordforUWS_OpsDate";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@FltNo", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@ManifestdateFrom", SqlDbType.DateTime) { Value = newFlightDate },
                    new SqlParameter("@ManifestdateTo", SqlDbType.VarChar) { Value = "" },
                    new SqlParameter("@DepartureAirport", SqlDbType.VarChar) { Value = DepartureAirport },
                    new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = FlightDestination }
                };

                dsData = await _readWriteDao.SelectRecords(procedure, parameters);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dsData;
        }

        public async Task<DataSet?> getFFMUnloadingPort(string DepartureAirport, string FlightNo, DateTime FlightDate)
        {
            DataSet? ds = new DataSet();
            try
            {
                //SQLServer dtb = new SQLServer();
                //string[] pname = new string[3]
                //{
                //"FlightID",
                //"Source",
                //"FlightDate"
                //};
                //object[] pvalue = new object[3]
                //{
                //FlightNo,
                //DepartureAirport,
                //newFlightDate

                //};
                //SqlDbType[] ptype = new SqlDbType[3]
                //{
                //SqlDbType.VarChar,
                //SqlDbType.VarChar,
                //SqlDbType.DateTime
                //};
                //ds = dtb.SelectRecords("spExpManiGetAirlineSch1", pname, pvalue, ptype);

                DateTime newFlightDate = FlightDate;
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@FlightID", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@Source", SqlDbType.VarChar) { Value = DepartureAirport },
                    new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = newFlightDate }
                };

                ds = await _readWriteDao.SelectRecords("spExpManiGetAirlineSch1", parameters);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return ds;
        }

        public async Task<string> EncodeNTM(string DepartureAirport, string FlightNo, DateTime FlightDate, string MsgVer, string aircraftregistrtionno)
        {
            string NTMinfo = "";
            try
            {
                MessageData.UWSinfo objFFMInfo = new MessageData.UWSinfo();
                MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
                MessageData.NTMconsignmnetinfo[] objNTMConInfo = new MessageData.NTMconsignmnetinfo[0];
                MessageData.NTMconsignmnetinfo[] objDGRConInfo = new MessageData.NTMconsignmnetinfo[0];
                MessageData.NTMconsignmnetinfo[] objSpecialCargoInfo = new MessageData.NTMconsignmnetinfo[0];
                MessageData.NTMconsignmnetinfo[] objBulkDGRSpecialinfo = new MessageData.NTMconsignmnetinfo[0];
                MessageData.unloadingport[] objUnloadingPort = new MessageData.unloadingport[0];
                MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
                MessageData.NTMULDinfo[] ntmULDinfo = new MessageData.NTMULDinfo[0];
                MessageData.NTMULDinfo[] uldSpecialinfo = new MessageData.NTMULDinfo[0];
                MessageData.NTMULDinfo[] DGRULDinfo = new MessageData.NTMULDinfo[0];
                MessageData.NTMInfo[] ntmInfo = new MessageData.NTMInfo[0];
                List<string> SI = new List<string>();
                DataSet? dsData = new DataSet();
                DataSet ds = new DataSet();
                GenericFunction genericFunction = new GenericFunction();
                bool IsThreeDigitNo = Convert.ToBoolean(genericFunction.GetConfigurationValues("IsThreeDigitNo") == null ? "false" : genericFunction.GetConfigurationValues("IsThreeDigitNo"));
                int count1 = 0, leg = 0, dgrrecords = 0, spclcargorecords = 0, noofDGRULD = 0, noOfSpecialULD = 0, noOfBulkRecords = 0;

                string NoSpecialCargo = "NIL";
                DataTable bulkinfo = new DataTable();

                ds = await getFFMUnloadingPort(DepartureAirport, FlightNo, FlightDate);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    objFFMInfo.carriercode = FlightNo.Substring(0, 2);

                    //Add code for UTC date change.
                    objFFMInfo.fltnum = FlightNo.Substring(2, FlightNo.Length - 2);
                    objFFMInfo.fltairportcode = DepartureAirport;
                    objFFMInfo.aircraftregistration = aircraftregistrtionno;
                    objFFMInfo.messageseqnumber = "1";

                    bool checknilFlight = false;

                    dsData = await GetRecordforGenerateNTM(DepartureAirport, FlightNo, FlightDate, string.Empty, "2", "Auto ExpToMan");

                    //start changes to filter ulds weight
                    if (dsData != null && dsData.Tables.Count > 0 && dsData.Tables[0].Rows.Count > 0)
                    {
                        if (dsData != null && dsData.Tables.Count > 0 && dsData.Tables[0].Rows.Count > 0 && dsData.Tables[0].Columns.Count == 5)
                        {
                            MessageData.unloadingport objTempUnloadingPort = new MessageData.unloadingport("");

                            if (!checknilFlight)
                            {
                                count1++;
                                checknilFlight = true;
                                objTempUnloadingPort.nilcargocode = "NIL";
                                objTempUnloadingPort.unloadingairport = dsData.Tables[0].Rows[0]["POU"].ToString();
                                //Change code for UTC flightdate
                                DateTime dtFlight = Convert.ToDateTime(dsData.Tables[0].Rows[0]["FlightDate"]);
                                objFFMInfo.month = dtFlight.ToString("MMM").ToUpper();
                                objFFMInfo.fltdate = dtFlight.ToString("dd");//DateTime.Parse(FlightDate).Day.ToString().PadLeft(2, '0');
                                Array.Resize(ref objUnloadingPort, count1);
                                objUnloadingPort[count1 - 1] = objTempUnloadingPort;
                            }
                        }
                        else
                        {
                            DataTable allulds = new DataTable();
                            allulds.Columns.Add("ULDNo");
                            allulds.Columns.Add("POU");

                            DataTable allawbs = new DataTable();
                            allawbs.Columns.Add("AWBNumber");
                            allawbs.Columns.Add("Added");

                            DateTime dtFlight = Convert.ToDateTime(dsData.Tables[0].Rows[0]["FlightDate"]);
                            objFFMInfo.month = dtFlight.ToString("MMM").ToUpper();
                            objFFMInfo.fltdate = dtFlight.ToString("dd");//DateTime.Parse(FlightDate).Day.ToString().PadLeft(2, '0');

                            #region multiple destination ULDS

                            Array.Resize(ref objUnloadingPort, ds.Tables[0].Rows.Count);
                            string AWBNo = "";
                            int uldseqno = 0, noofulds = 0, records = 0;
                            //loop for each destination
                            foreach (DataRow destinations in ds.Tables[0].Rows)
                            {
                                //find out the awbnumber row whose POU=destination
                                DataRow[] allData = dsData.Tables[0].Select("POU='" + destinations[0] + "'"); //AND ULDNo<>'BULK'");
                                DataRow[] allbulkData = dsData.Tables[0].Select("POU='" + destinations[0] + "'"); //AND ULDNo='BULK'");
                                DataRow[] alluldandbulkdata = dsData.Tables[0].Select("POU='" + destinations[0] + "'");

                                foreach (DataRow dr in allData)
                                {
                                    DataRow uldrow = allulds.NewRow();
                                    uldrow["ULDNo"] = dr["ULDNumber"].ToString();
                                    uldrow["POU"] = destinations[0];
                                    allulds.Rows.Add(uldrow);
                                }

                                foreach (DataRow dr in alluldandbulkdata)
                                {
                                    DataRow uldrow = allawbs.NewRow();
                                    uldrow["AWBNumber"] = dr["AWBPrefix"] + "-" + dr["AWBNumber"].ToString();
                                    uldrow["Added"] = "P";
                                    allawbs.Rows.Add(uldrow);
                                }

                                DataTable distictuld = SelectDistinct(allulds, "ULDNo");
                                DataTable distictAWB = SelectDistinct(allawbs, "AWBNumber");

                                foreach (DataRow ulds in distictuld.Rows)
                                {
                                    noofulds++;
                                    MessageData.NTMULDinfo uldinfo = new MessageData.NTMULDinfo();
                                    uldinfo.uldno = ulds["ULDNo"].ToString();
                                    uldinfo.ulddestination = destinations[0].ToString();
                                    Array.Resize(ref ntmULDinfo, noofulds);
                                    ntmULDinfo[noofulds - 1] = uldinfo;
                                }

                                MessageData.unloadingport objTempUnloadingPort = new MessageData.unloadingport("");
                                objTempUnloadingPort.unloadingairport = destinations[0].ToString();
                                objUnloadingPort[leg] = objTempUnloadingPort;
                                leg++;

                                // DataTable distict
                                string unid = string.Empty;
                                for (int k = 0; k < distictuld.Rows.Count; k++)
                                {
                                    string uldno = distictuld.Rows[k]["ULDNo"].ToString();
                                    DataRow[] drULDData = dsData.Tables[0].Select("ULDNumber='" + uldno + "'");
                                    if (drULDData.Length > 0)
                                    {
                                        foreach (DataRow dr in drULDData)
                                        {
                                            MessageData.NTMconsignmnetinfo tempDGRConinfo = new MessageData.NTMconsignmnetinfo();
                                            MessageData.NTMconsignmnetinfo tempspecialConinfo = new MessageData.NTMconsignmnetinfo();

                                            #region DGR
                                            if (dr["isnotoc"].ToString().ToUpper() == "DGR")
                                            {
                                                records++;
                                                AWBNo = (dr["AWBPrefix"] + "-" + dr["AWBNumber"]);
                                                tempDGRConinfo.splhandling = "";
                                                tempDGRConinfo.uldrefseqno = uldseqno;
                                                tempDGRConinfo.dgrclass = "";
                                                tempDGRConinfo.remark = "";
                                                tempDGRConinfo.dest = dr["POU"].ToString();
                                                tempDGRConinfo.airlineprefix = dr["AWBPrefix"].ToString();
                                                tempDGRConinfo.uldno = dr["ULDNumber"].ToString();
                                                SI.Add(tempDGRConinfo.remark.ToString());
                                                tempDGRConinfo.awbnum = AWBNo.Substring(AWBNo.Length - 8);//((Label)gdvULDDetails.Rows[0].FindControl("lblAWBno")).Text;
                                                tempDGRConinfo.shippername = dr["ShippingName"].ToString();
                                                tempDGRConinfo.classorDiv = dr["Class"].ToString();
                                                tempDGRConinfo.dgrclass = dr["Class"].ToString();
                                                if (tempDGRConinfo.dgrclass == "0")
                                                    tempDGRConinfo.dgrclass = "";

                                                unid = dr["UNIDNo"].ToString();
                                                if (unid != null && unid.Length > 0)
                                                {
                                                    if (!unid.Contains("U") || !unid.Contains("I"))
                                                    {
                                                        if (Convert.ToInt32(unid) >= 8000)
                                                        {
                                                            unid = "I" + unid;
                                                        }
                                                        else
                                                        {
                                                            unid = "U" + unid;
                                                        }
                                                    }
                                                    tempDGRConinfo.unidnumber = unid;//;((unid != null && unid != "" && unid.Length > 0) ? (unid.Substring(0, 1) + (unid.Substring(unid.Length - 4, 4))) : "");
                                                }
                                                else
                                                    tempDGRConinfo.unidnumber = "";

                                                tempDGRConinfo.subsidaryrisk = dr["SubRisk"].ToString().ToUpper();
                                                tempDGRConinfo.weight = dr["NOTOCWeight"].ToString(); // (dr["NOTOCWeight"].ToString().Contains(".")) ? (dr["NOTOCWeight"].ToString()).TrimEnd('0').Replace('.', ' ').Trim() : (dr["NOTOCWeight"].ToString()); ; //((Label)gdvULDDetails.Rows[0].FindControl("lblMftPcs")).Text;
                                                tempDGRConinfo.radioactivequantity = dr["RadioActiveMaterial"].ToString().ToUpper();
                                                tempDGRConinfo.noofpackages = dr["NOTOCPieces"].ToString();//change it data needs to populate

                                                if (dr["TransportIndex"].ToString() != "")
                                                    tempDGRConinfo.netquanitity = dr["TransportIndex"].ToString();
                                                else
                                                    tempDGRConinfo.netquanitity = dr["NOTOCWeight"].ToString();

                                                tempDGRConinfo.packinggroup = dr["UNPackageGroup"].ToString();//change it data needs to populate
                                                tempDGRConinfo.technicalname = "Technical Name";
                                                tempDGRConinfo.impcode = dr["IMPCode"].ToString().ToUpper() == string.Empty ? dr["SHCCode"].ToString().ToUpper() : dr["IMPCode"].ToString().ToUpper();
                                                tempDGRConinfo.caocode = dr["CAO"].ToString().ToUpper();
                                                tempDGRConinfo.packageinfo = dr["UNPackageGroup"].ToString().ToUpper();

                                                if (tempDGRConinfo.caocode.Length > 0)
                                                    tempDGRConinfo.caocode = "Y";
                                                else
                                                    tempDGRConinfo.caocode = "N";

                                                tempDGRConinfo.drillcode = dr["DrillCode"].ToString().ToUpper();
                                                tempDGRConinfo.description = "";
                                                tempDGRConinfo.uldposition = dr["Position"].ToString();
                                                tempDGRConinfo.Ergcode = dr["ERGCode"].ToString().ToUpper();

                                                tempDGRConinfo.uom = dr["UOM"].ToString().ToUpper();
                                                switch (tempDGRConinfo.uom)
                                                {
                                                    case "LT":
                                                        tempDGRConinfo.uom = "L";
                                                        break;
                                                    case "TI":
                                                        tempDGRConinfo.uom = "T";
                                                        break;
                                                    case "K":
                                                        tempDGRConinfo.uom = "K";
                                                        break;
                                                    default:
                                                        tempDGRConinfo.uom = "";
                                                        break;
                                                }

                                                Array.Resize(ref objNTMConInfo, records);
                                                objNTMConInfo[records - 1] = tempDGRConinfo;
                                                noofDGRULD++;
                                                dgrrecords++;
                                                MessageData.NTMULDinfo tempuldDGRinfo = new MessageData.NTMULDinfo();
                                                tempuldDGRinfo.uldno = tempDGRConinfo.uldno;
                                                tempuldDGRinfo.ulddestination = tempDGRConinfo.dest;
                                                tempuldDGRinfo.ISDGR = "DGR";
                                                Array.Resize(ref DGRULDinfo, noofDGRULD);
                                                DGRULDinfo[noofDGRULD - 1] = tempuldDGRinfo;
                                                Array.Resize(ref objDGRConInfo, dgrrecords);
                                                objDGRConInfo[dgrrecords - 1] = tempDGRConinfo;
                                            }
                                            #endregion

                                            #region special load
                                            if (dr["isnotoc"].ToString().ToUpper() == "Special Cargo".ToUpper())
                                            {
                                                records++;
                                                AWBNo = (dr["AWBPrefix"] + "-" + dr["AWBNumber"]);
                                                tempspecialConinfo.splhandling = "";
                                                tempspecialConinfo.uldrefseqno = uldseqno;
                                                tempspecialConinfo.dgrclass = "";
                                                tempspecialConinfo.remark = "";
                                                tempspecialConinfo.dest = dr["POU"].ToString();
                                                tempspecialConinfo.airlineprefix = dr["AWBPrefix"].ToString();
                                                tempspecialConinfo.uldno = dr["ULDNumber"].ToString();
                                                SI.Add(tempspecialConinfo.remark.ToString());
                                                tempspecialConinfo.awbnum = AWBNo.Substring(AWBNo.Length - 8);//((Label)gdvULDDetails.Rows[0].FindControl("lblAWBno")).Text;
                                                tempspecialConinfo.shippername = dr["ShippingName"].ToString();
                                                tempspecialConinfo.classorDiv = dr["Class"].ToString();
                                                tempspecialConinfo.dgrclass = dr["Class"].ToString();
                                                if (tempspecialConinfo.dgrclass == "0")
                                                    tempspecialConinfo.dgrclass = "";

                                                unid = dr["UNIDNo"].ToString();
                                                if (unid != null && unid.Length > 0)
                                                {
                                                    if (!unid.Contains("U") || !unid.Contains("I"))
                                                    {
                                                        if (Convert.ToInt32(unid) >= 8000)
                                                        {
                                                            unid = "I" + unid;
                                                        }
                                                        else
                                                        {
                                                            unid = "U" + unid;
                                                        }
                                                    }
                                                    tempspecialConinfo.unidnumber = unid;//;((unid != null && unid != "" && unid.Length > 0) ? (unid.Substring(0, 1) + (unid.Substring(unid.Length - 4, 4))) : "");
                                                }
                                                else
                                                    tempspecialConinfo.unidnumber = "";

                                                tempspecialConinfo.subsidaryrisk = dr["SubRisk"].ToString().ToUpper();
                                                tempspecialConinfo.weight = dr["NOTOCWeight"].ToString(); //((Label)gdvULDDetails.Rows[0].FindControl("lblMftPcs")).Text;
                                                tempspecialConinfo.radioactivequantity = dr["RadioActiveMaterial"].ToString().ToUpper();
                                                tempspecialConinfo.noofpackages = dr["NOTOCPieces"].ToString();//change it data needs to populate
                                                tempspecialConinfo.netquanitity = dr["NOTOCWeight"].ToString();
                                                tempspecialConinfo.packinggroup = "";

                                                //Change pieces and weight
                                                tempspecialConinfo.technicalname = "Technical Name";
                                                tempspecialConinfo.impcode = dr["IMPCode"].ToString().ToUpper() == string.Empty ? dr["SHCCode"].ToString().ToUpper() : dr["IMPCode"].ToString().ToUpper();
                                                tempspecialConinfo.caocode = dr["CAO"].ToString().ToUpper();
                                                tempspecialConinfo.packageinfo = dr["UNPackageGroup"].ToString().ToUpper();

                                                if (tempspecialConinfo.caocode.Length > 0)
                                                    tempspecialConinfo.caocode = "Y";
                                                else
                                                    tempspecialConinfo.caocode = "N";

                                                tempspecialConinfo.drillcode = dr["DrillCode"].ToString().ToUpper();
                                                tempspecialConinfo.description = "";
                                                tempspecialConinfo.uldposition = dr["Position"].ToString();
                                                tempspecialConinfo.Ergcode = dr["ERGCode"].ToString().ToUpper();
                                                tempspecialConinfo.uom = dr["UOM"].ToString().ToUpper();
                                                switch (tempspecialConinfo.uom)
                                                {
                                                    case "LT":
                                                        tempspecialConinfo.uom = "L";
                                                        break;
                                                    case "TI":
                                                        tempspecialConinfo.uom = "T";
                                                        break;

                                                    case "K":
                                                        tempspecialConinfo.uom = "K";
                                                        break;

                                                    default:
                                                        tempspecialConinfo.uom = "";
                                                        break;
                                                }

                                                Array.Resize(ref objNTMConInfo, records);
                                                objNTMConInfo[records - 1] = tempspecialConinfo;
                                                noOfSpecialULD++;
                                                spclcargorecords++;
                                                MessageData.NTMULDinfo tempuldSpecialinfo = new MessageData.NTMULDinfo();
                                                tempuldSpecialinfo.uldno = tempspecialConinfo.uldno;
                                                tempuldSpecialinfo.ulddestination = tempspecialConinfo.dest;
                                                tempuldSpecialinfo.ISSpecial = "Special";
                                                Array.Resize(ref uldSpecialinfo, noOfSpecialULD);
                                                uldSpecialinfo[noOfSpecialULD - 1] = tempuldSpecialinfo;
                                                Array.Resize(ref objSpecialCargoInfo, spclcargorecords);
                                                objSpecialCargoInfo[spclcargorecords - 1] = tempspecialConinfo;
                                            }
                                            #endregion
                                        }
                                    }
                                }
                                objFFMInfo.messageendindicator = "LAST";
                                #endregion
                            }
                        }
                        try
                        {
                            NTMinfo = EncodeNTMforsend(DepartureAirport, ref objFFMInfo, ref objUnloadingPort, SI, ref ntmInfo, ref ntmULDinfo, ref objNTMConInfo, ref objSpecialCargoInfo, ref objBulkDGRSpecialinfo, ref objDGRConInfo, NoSpecialCargo, spclcargorecords, dgrrecords, noOfSpecialULD, noOfBulkRecords, ref uldSpecialinfo, ref DGRULDinfo, IsThreeDigitNo);
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }
                        if (NTMinfo != null)
                        {
                            if (NTMinfo.Trim() != "" && NTMinfo.Length > 0)
                            {
                                return NTMinfo;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return NTMinfo;
        }

        public string EncodeNTMforsend(string pol, ref MessageData.UWSinfo UWSdata, ref MessageData.unloadingport[] unloadingport, List<string> SI, ref MessageData.NTMInfo[] ntmInfo, ref MessageData.NTMULDinfo[] ntmULDinfo, ref MessageData.NTMconsignmnetinfo[] objNTMConInfo, ref MessageData.NTMconsignmnetinfo[] objSpecialCargoInfo, ref MessageData.NTMconsignmnetinfo[] objBulkDGRSpecialinfo, ref MessageData.NTMconsignmnetinfo[] objDGRConInfo, string NoSpecialCargo, int spclcargorecords, int dgrrecords, int noOfSpecialULD, int noOfBulkRecords, ref MessageData.NTMULDinfo[] uldSpecialinfo, ref MessageData.NTMULDinfo[] DGRULDinfo, bool IsThreeDigitNo = false)
        {

            string BUILDNTM = null;
            try
            {
                StringBuilder uldstr = new StringBuilder("");
                StringBuilder bulkstr = new StringBuilder("");
                StringBuilder strline1 = new StringBuilder("");
                StringBuilder dgruldstr = new StringBuilder("");
                StringBuilder specialuldstr = new StringBuilder("");
                StringBuilder bulkdgrspecialstr = new StringBuilder("");
                StringBuilder firstuldinfodgr = new StringBuilder("");
                StringBuilder firstuldinfospecl = new StringBuilder("");

                #region Line 1
                strline1.Append("NTM" + "$");
                #endregion

                #region Line 2
                DateTime dt1 = new DateTime();
                dt1 = Convert.ToDateTime(System.DateTime.Now.ToLongDateString());
                dt1 = System.DateTime.Now;
                string strnew = dt1.ToString("yyyyMMddHHmmss");

                if (UWSdata.fltnum.Length < 3 && pol == "DXB" && IsThreeDigitNo == true)
                {
                    UWSdata.fltnum = UWSdata.fltnum.PadLeft(3, '0');
                }
                strline1.Append(UWSdata.carriercode + UWSdata.fltnum + "/" + UWSdata.fltdate + UWSdata.month + " " + pol + " " + UWSdata.messageseqnumber + "/" + strnew + (UWSdata.aircraftregistration.Length > 0 ? ("/" + UWSdata.aircraftregistration.ToString()) : ""));
                #endregion

                string nillflight = "NIL", subRisk2 = "";
                int displayuldnodgr = 0, displayuldnospcl = 0;
                StringBuilder newstr = new StringBuilder("");
                StringBuilder newbldr = new StringBuilder("");

                if (unloadingport.Length > 0)
                {
                    #region all DGRs in ulds
                    //First need to display all DGR in ulds
                    if (dgrrecords > 0)
                    {
                        nillflight = "NO";
                        foreach (MessageData.NTMULDinfo tempdgruld in ntmULDinfo)
                        {
                            dgruldstr = uldstr.Clear();
                            displayuldnodgr = 0;
                            foreach (MessageData.NTMconsignmnetinfo tempcon in objDGRConInfo)
                            {
                                if (tempdgruld.uldno == tempcon.uldno && tempcon.dest == tempdgruld.ulddestination)
                                {
                                    subRisk2 = "";
                                    if (tempcon.subsideryrisk2 == null)
                                        subRisk2 = "";
                                    else
                                        subRisk2 = tempcon.subsideryrisk2;

                                    //AWB Info
                                    dgruldstr.Append("$" + tempcon.airlineprefix + "-" + tempcon.awbnum + "");

                                    //Product Info
                                    dgruldstr.Append(tempcon.dgrclass.Trim().Length > 0 ? tempcon.dgrclass.Trim().PadRight(4) : "");
                                    dgruldstr.Append(tempcon.unidnumber.Trim().Length > 0 ? tempcon.unidnumber.Trim() + "        " : "");
                                    //dgruldstr.Append(tempcon.unidnumber.Trim().Length > 0 ? tempcon.unidnumber.PadRight(8) : "");
                                    dgruldstr.Append(tempcon.subsidaryrisk.Trim().Length > 0 ? tempcon.subsidaryrisk.Trim() : "");
                                    dgruldstr.Append(subRisk2.Trim().Length > 0 ? subRisk2.Trim() : "");

                                    //package info
                                    dgruldstr.Append(((tempcon.noofpackages.Length > 0) ? (tempcon.noofpackages.Trim()) : "0") + "/");
                                    dgruldstr.Append(((tempcon.netquanitity.Length > 0) ? (tempcon.netquanitity.Trim()) : "0") + "/");

                                    dgruldstr.Append(((tempcon.radioactivequantity.Length > 0) ? (tempcon.radioactivequantity.Trim()) : "") + "/");
                                    dgruldstr.Append(((tempcon.packinggroup.Length > 0) ? (tempcon.packinggroup.Trim()) : "") + "/");
                                    //dgruldstr.Append(((tempcon.impcode.Length > 0) ? (tempcon.impcode.Trim()) : "") + "/");

                                    string Impcode = tempcon.impcode.Trim();
                                    if (Impcode != null && Impcode != "" && Impcode.Length > 3)
                                    {
                                        Impcode = Impcode.Substring(0, 3);
                                    }
                                    dgruldstr.Append(Impcode + "/");

                                    dgruldstr.Append(((tempcon.drillcode.Length > 0) ? (tempcon.drillcode.Trim()) : "") + "/");
                                    dgruldstr.Append(((tempcon.caocode.Length > 0) ? (tempcon.caocode.Trim()) : "") + "/");
                                    dgruldstr.Append(((tempcon.uldposition.Length > 0) ? (tempcon.uldposition.Trim()) : ""));

                                    if (tempcon.shippername.Trim() != string.Empty)
                                        dgruldstr.Append("$" + tempcon.shippername + "$");

                                    if (tempcon.remark.Trim() != string.Empty)
                                        dgruldstr.Append("$OSI/" + tempcon.remark + "$");
                                    displayuldnodgr++;
                                }

                                if (displayuldnodgr == 1)
                                {
                                    firstuldinfodgr.Append("$DNG" + tempdgruld.ulddestination + tempdgruld.uldno);
                                    displayuldnodgr++;
                                }
                            }
                            firstuldinfodgr.Append(dgruldstr);
                        }
                    }
                    #endregion

                    #region all Special Loads in uld
                    //Second part for all Special load ulds
                    if (spclcargorecords > 0)
                    {
                        nillflight = "NO";
                        foreach (MessageData.NTMULDinfo tempspecialuld in ntmULDinfo)
                        {
                            specialuldstr.Clear();
                            displayuldnospcl = 0;
                            foreach (MessageData.NTMconsignmnetinfo tempcons in objSpecialCargoInfo)
                            {
                                if (tempspecialuld.uldno == tempcons.uldno && tempcons.dest == tempspecialuld.ulddestination)
                                {
                                    subRisk2 = "";
                                    if (tempcons.subsideryrisk2 == null)
                                        subRisk2 = "";
                                    else
                                        subRisk2 = tempcons.subsideryrisk2;

                                    //AWB Info
                                    if (tempcons.dgrclass.Trim().Length == 0 && tempcons.unidnumber.Trim().Length == 0 && tempcons.subsidaryrisk.Trim().Length == 0 && subRisk2.Trim().Length == 0)
                                    {
                                        specialuldstr.Append("$" + tempcons.airlineprefix + "-" + tempcons.awbnum + " ");
                                    }
                                    else
                                    {
                                        specialuldstr.Append("$" + tempcons.airlineprefix + "-" + tempcons.awbnum + "");
                                    }
                                    //Product Info
                                    specialuldstr.Append(tempcons.dgrclass.Trim().Length > 0 ? tempcons.dgrclass.Trim().PadRight(4) : "");
                                    specialuldstr.Append(tempcons.unidnumber.Trim().Length > 0 ? tempcons.unidnumber.Trim() + "        " : "");
                                    //specialuldstr.Append(tempcons.unidnumber.Trim().Length > 0 ? tempcons.unidnumber.PadRight(8)  : "");
                                    specialuldstr.Append(tempcons.subsidaryrisk.Trim().Length > 0 ? tempcons.subsidaryrisk.Trim() : "");
                                    specialuldstr.Append(subRisk2.Trim().Length > 0 ? subRisk2.Trim() : "");

                                    //package info
                                    specialuldstr.Append(((tempcons.noofpackages.Length > 0) ? (tempcons.noofpackages.Trim()) : "0") + "/");
                                    specialuldstr.Append(((tempcons.netquanitity.Length > 0) ? (tempcons.netquanitity.Trim()) : "0") + "/");

                                    specialuldstr.Append(((tempcons.radioactivequantity.Length > 0) ? (tempcons.radioactivequantity.Trim()) : "") + "/");
                                    specialuldstr.Append(((tempcons.packinggroup.Length > 0) ? (tempcons.packinggroup.Trim()) : "") + "/");
                                    // specialuldstr.Append(((tempcons.impcode.Length > 0) ? (tempcons.impcode.Trim()) : "") + "/");

                                    string Impcode = tempcons.impcode.Trim();
                                    if (Impcode != null && Impcode != "" && Impcode.Length > 3)
                                    {
                                        Impcode = Impcode.Substring(0, 3);
                                    }
                                    specialuldstr.Append(Impcode + "/");

                                    specialuldstr.Append(((tempcons.drillcode.Length > 0) ? (tempcons.drillcode.Trim()) : "") + "/");
                                    specialuldstr.Append(((tempcons.caocode.Length > 0) ? (tempcons.caocode.Trim()) : "") + "/");
                                    specialuldstr.Append(((tempcons.uldposition.Length > 0) ? (tempcons.uldposition.Trim()) : ""));

                                    if (tempcons.shippername.Trim() != string.Empty)
                                        specialuldstr.Append("$" + tempcons.shippername + "$");

                                    if (tempcons.remark.Trim() != string.Empty)
                                        specialuldstr.Append("$OSI/" + tempcons.remark + "$");

                                    displayuldnospcl++;
                                }

                                if (displayuldnospcl == 1)
                                {
                                    firstuldinfospecl.Append("$OTH" + tempspecialuld.ulddestination + tempspecialuld.uldno);
                                    displayuldnospcl++;
                                }
                            }
                            firstuldinfospecl.Append(specialuldstr);
                        }
                    }
                    #endregion

                    #region bulk dgr or special info
                    //third part for all BULK info
                    foreach (MessageData.NTMconsignmnetinfo tempbulk in objBulkDGRSpecialinfo)
                    {
                        subRisk2 = "";
                        if (tempbulk.subsideryrisk2 == null)
                            subRisk2 = "";
                        else
                            subRisk2 = tempbulk.subsideryrisk2;

                        //AWB Info
                        bulkdgrspecialstr.Append("$" + tempbulk.airlineprefix + "-" + tempbulk.awbnum + "");

                        //Product Info
                        bulkdgrspecialstr.Append(tempbulk.dgrclass.Trim().Length > 0 ? tempbulk.dgrclass.Trim().PadRight(4) : "");
                        bulkdgrspecialstr.Append(tempbulk.unidnumber.Trim().Length > 0 ? tempbulk.unidnumber.Trim() + "        " : "");
                        //bulkdgrspecialstr.Append(tempbulk.unidnumber.Trim().Length > 0 ? tempbulk.unidnumber.PadRight(8) : "");
                        bulkdgrspecialstr.Append(tempbulk.subsidaryrisk.Trim().Length > 0 ? tempbulk.subsidaryrisk.Trim() : "");
                        bulkdgrspecialstr.Append(subRisk2.Trim().Length > 0 ? subRisk2.Trim() : "");

                        //package info
                        bulkdgrspecialstr.Append(((tempbulk.noofpackages.Length > 0) ? (tempbulk.noofpackages.Trim()) : "0") + "/");
                        bulkdgrspecialstr.Append(((tempbulk.netquanitity.Length > 0) ? (tempbulk.netquanitity.Trim()) : "0") + "/");

                        bulkdgrspecialstr.Append(((tempbulk.radioactivequantity.Length > 0) ? (tempbulk.radioactivequantity.Trim()) : "") + "/");
                        bulkdgrspecialstr.Append(((tempbulk.packinggroup.Length > 0) ? (tempbulk.packinggroup.Trim()) : "") + "/");
                        //bulkdgrspecialstr.Append(((tempbulk.impcode.Length > 0) ? (tempbulk.impcode.Trim()) : "") + "/");

                        string Impcode = tempbulk.impcode.Trim();
                        if (Impcode != null && Impcode != "" && Impcode.Length > 3)
                        {
                            Impcode = Impcode.Substring(0, 3);
                        }
                        bulkdgrspecialstr.Append(Impcode + "/");

                        bulkdgrspecialstr.Append(((tempbulk.drillcode.Length > 0) ? (tempbulk.drillcode.Trim()) : "") + "/");
                        bulkdgrspecialstr.Append(((tempbulk.caocode.Length > 0) ? (tempbulk.caocode.Trim()) : "") + "/");
                        bulkdgrspecialstr.Append(((tempbulk.uldposition.Length > 0) ? (tempbulk.uldposition.Trim()) : ""));

                        if (tempbulk.shippername.Trim() != string.Empty)
                            bulkdgrspecialstr.Append("$" + tempbulk.shippername + "$");

                        if (tempbulk.remark.Trim() != string.Empty)
                            bulkdgrspecialstr.Append("$OSI/" + tempbulk.remark + "$");

                        nillflight = "NO";
                    }
                    #endregion
                }

                #region Nil Flight info
                if (unloadingport.Length > 0)
                {
                    foreach (MessageData.unloadingport up in unloadingport)
                    {
                        if (up.nilcargocode == "NIL" || nillflight == "NIL")
                        {
                            strline1.Append("/NIL");
                        }
                    }
                }
                #endregion

                #region Supplimentory info
                StringBuilder sppinfo = new StringBuilder("");
                if (SI.Count > 0 && SI[0] != "")
                {
                    sppinfo.Append("$" + "SI" + "  " + SI[0]);

                    for (int cnt = 1; cnt < SI.Count; cnt++)
                    {
                        sppinfo.Append("$" + SI[cnt]);
                    }
                    sppinfo.Append("$");
                }

                #endregion

                strline1.Replace("$$", "$");
                strline1.Replace("$", "\r\n");

                bulkdgrspecialstr.Replace("$$", "$");
                bulkdgrspecialstr.Replace("$", "\r\n");

                specialuldstr.Replace("$$", "$");
                specialuldstr.Replace("$", "\r\n");

                dgruldstr.Replace("$$", "$");
                dgruldstr.Replace("$", "\r\n");

                firstuldinfospecl.Replace("$$", "$");
                firstuldinfospecl.Replace("$", "\r\n");

                firstuldinfodgr.Replace("$$", "$");
                firstuldinfodgr.Replace("$", "\r\n");

                sppinfo.Replace("$$", "$");
                sppinfo.Replace("$", "\r\n");

                #region BUILDNTM

                //Nil Flight
                if (nillflight == "NIL")
                {
                    BUILDNTM = strline1.ToString() + "\r\n"; //+ "";
                }
                else
                {
                    //First display all dgr uld
                    BUILDNTM = strline1.ToString()
                        + firstuldinfodgr.ToString().TrimEnd('\n').TrimEnd('\r')
                        + firstuldinfospecl.ToString().TrimEnd('\n').TrimEnd('\r')
                        + bulkdgrspecialstr.ToString().TrimEnd('\n').TrimEnd('\r')
                        + sppinfo.ToString().TrimEnd('\n').TrimEnd('\r') + "\r\n"
                        + UWSdata.messageendindicator + "\r\n";
                    //second display all special ulds
                    //third display bulk uld and special
                }
                BUILDNTM = BUILDNTM.Replace("\r\n\r\n", "\r\n");
                #endregion
            }
            catch (Exception ex)
            {
                BUILDNTM = "ERR";
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return BUILDNTM.ToUpper();
        }
        public async Task<DataSet?> GetRecordforGenerateNTM(string DepartureAirport, string FlightNo, DateTime FlightDate, string FlightDestination, string Flag, string UpdatedBy)
        {
            DataSet? dsData = new DataSet();
            try
            {
                //SQLServer dtb = new SQLServer(true);

                //string[] paramname = new string[] { "FlightNo", "FlightDt", "POL", "Flag", "UpdatedBy" };

                //object[] paramvalue = new object[] { FlightNo, newFlightDate, DepartureAirport, Flag, UpdatedBy };

                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };

                //dsData = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);


                DateTime newFlightDate = FlightDate;
                string procedure = "uspGetNOTOCDetails";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@FlightDt", SqlDbType.DateTime) { Value = newFlightDate },
                    new SqlParameter("@POL", SqlDbType.VarChar) { Value = DepartureAirport },
                    new SqlParameter("@Flag", SqlDbType.VarChar) { Value = Flag },
                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = UpdatedBy }
                };

                dsData = await _readOnlyDao.SelectRecords(procedure, parameters);

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dsData;
        }
        public static bool DecodeCSNMessage(string csnMsg, int refNO, ref MessageData.CSNInfo csnInfo, ref MessageData.customsextrainfo[] custominfo, ref DataTable dtOCIInfo)
        {
            bool flag = false;
            try
            {
                string lastrec = string.Empty;
                csnMsg = csnMsg.Replace("\r\n", "$");
                string[] arrCSNMsg = csnMsg.Split('$');
                if (arrCSNMsg.Length >= 6)
                {
                    for (int i = 0; i < arrCSNMsg.Length; i++)
                    {
                        if (arrCSNMsg[i].StartsWith("CSN", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] msg = arrCSNMsg[i].Split('/');
                            csnInfo.versionNumber = msg.Length > 1 ? msg[1].Trim() : "";
                        }
                        if (arrCSNMsg[i].StartsWith("WBI", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] msg = arrCSNMsg[i].Split('/');
                            csnInfo.awbPrefix = msg[1].Split('/')[0].Split('-')[0].Trim();
                            csnInfo.awbNumber = msg[1].Split('/')[0].Split('-')[1].Trim();
                            csnInfo.HouseNo = string.Empty;
                            csnInfo.IsMaster = string.Empty;
                            csnInfo.ISHouse = string.Empty;

                            if (msg.Length > 2 && msg[2].Trim().ToUpper() == "M")
                                csnInfo.IsMaster = msg[2].Trim();

                            if (msg.Length > 3)
                            {
                                csnInfo.HouseNo = msg[3].Trim();
                                csnInfo.ISHouse = "H";
                            }
                        }
                        if (arrCSNMsg[i].StartsWith("FLT", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] msg = arrCSNMsg[i].Split('/');
                            csnInfo.flightNumber = msg[1].Trim().Contains("XXX") ? "" : msg[1].Trim();
                            csnInfo.pol = msg[2].Trim().Contains("XXX") ? "" : msg[2].Trim().Substring(0, 3);
                            csnInfo.pou = msg[2].Trim().Contains("XXX") ? "" : msg[2].Trim().Substring(3, 3);
                            csnInfo.flightDay = msg[3].Trim().Contains("XXX") ? DateTime.Now.Day.ToString().PadLeft(2, '0') : msg[3].Trim().Substring(0, 2);
                            csnInfo.flightMonth = msg[3].Trim().Contains("XXX") ? DateTime.Now.ToString("MMM") : msg[3].Trim().Substring(2, 3);
                        }
                        if (arrCSNMsg[i].StartsWith("CAN", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] msg = arrCSNMsg[i].Split('/');
                            csnInfo.customStatusCode = msg[1].Trim();
                            csnInfo.customsNotification = msg[2].Trim();
                            csnInfo.customsActionCode = msg[2].Trim();
                        }
                        if (arrCSNMsg[i].StartsWith("DTN", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] msg = arrCSNMsg[i].Split('/');
                            csnInfo.notificationDay = msg[1].Trim().Substring(0, 2);
                            csnInfo.notificationMonth = msg[1].Trim().Substring(2, 3);
                            csnInfo.notificationTime = msg[2].Trim();
                        }
                        if (arrCSNMsg[i].StartsWith("CND", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] msg = arrCSNMsg[i].Split('/');
                            csnInfo.customsEntryNumber = msg[1].Trim();
                            csnInfo.numberOfPieces = msg[2].Trim();
                        }
                        if (arrCSNMsg[i].StartsWith("OCI", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] msg = arrCSNMsg[i].Split('/');
                            csnInfo.customsEntryNumber = msg[1].Trim();
                            csnInfo.numberOfPieces = msg[2].Trim();
                            if (msg.Length > 0)
                            {
                                lastrec = "OCI";
                                MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
                                custom.IsoCountryCodeOci = msg[1];
                                custom.InformationIdentifierOci = msg[2];
                                custom.CsrIdentifierOci = msg[3];
                                custom.SupplementaryCsrIdentifierOci = msg[4];
                                Array.Resize(ref custominfo, custominfo.Length + 1);
                                custominfo[custominfo.Length - 1] = custom;
                            }
                        }
                        if (arrCSNMsg[i].StartsWith("/"))
                        {
                            if (lastrec == "OCI")
                            {
                                string[] msg = arrCSNMsg[i].Split('/');
                                MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
                                custom.IsoCountryCodeOci = msg[1];
                                custom.InformationIdentifierOci = msg[2];
                                custom.CsrIdentifierOci = msg[3];
                                custom.SupplementaryCsrIdentifierOci = msg[4];
                                Array.Resize(ref custominfo, custominfo.Length + 1);
                                custominfo[custominfo.Length - 1] = custom;
                            }
                        }
                    }
                }
                dtOCIInfo = OCIInfoTable(custominfo);
                flag = true;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;

        }

        public static DataTable OCIInfoTable(MessageData.customsextrainfo[] custominfo)
        {
            DataTable dtOCIInfo = new DataTable();
            int srno = 0;
            try
            {
                dtOCIInfo.Columns.Add("ID", typeof(int));
                dtOCIInfo.Columns.Add("IsoCountryCodeOci", typeof(string));
                dtOCIInfo.Columns.Add("InformationIdentifierOci", typeof(string));
                dtOCIInfo.Columns.Add("CsrIdentifierOci", typeof(string));
                dtOCIInfo.Columns.Add("SupplementaryCsrIdentifierOci", typeof(string));
                dtOCIInfo.Columns.Add("ParentID", typeof(int));
                foreach (var item in custominfo)
                {
                    srno++;
                    DataRow drOCIInfo = dtOCIInfo.NewRow();
                    drOCIInfo["ID"] = srno;
                    drOCIInfo["IsoCountryCodeOci"] = item.IsoCountryCodeOci;
                    drOCIInfo["InformationIdentifierOci"] = item.InformationIdentifierOci;
                    drOCIInfo["CsrIdentifierOci"] = item.CsrIdentifierOci;
                    drOCIInfo["SupplementaryCsrIdentifierOci"] = item.SupplementaryCsrIdentifierOci;
                    drOCIInfo["ParentID"] = 1;
                    dtOCIInfo.Rows.Add(drOCIInfo);
                }
                dtOCIInfo.AcceptChanges();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _staticLogger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dtOCIInfo;
        }
    }
}