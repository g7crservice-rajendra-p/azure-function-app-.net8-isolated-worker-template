using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;
using System.Data;
using System.Text;
using System.Xml;

namespace QidWorkerRole
{
    public class cls_SCMBL
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<cls_SCMBL> _logger;
        private readonly GenericFunction _genericFunction;
        private readonly CIMPMessageValidation _cIMPMessageValidation;
        private readonly cls_Encode_Decode _cls_Encode_Decode;
        private readonly CarditResiditManagement _carditResiditManagement;
        private readonly CGOProcessor _cGOProcessor;
        private readonly MVT _mVT;
        private readonly ASM _aSm;
        public cls_SCMBL(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<cls_SCMBL> logger,
            GenericFunction genericFunction,
            CIMPMessageValidation cIMPMessageValidation,
            cls_Encode_Decode cls_Encode_Decode,
            CarditResiditManagement carditResiditManagement,
            CGOProcessor cGOProcessor,
            MVT mVT,
            ASM aSM
        )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
            _cIMPMessageValidation = cIMPMessageValidation;
            _cls_Encode_Decode = cls_Encode_Decode;
            _carditResiditManagement = carditResiditManagement;
            _cGOProcessor = cGOProcessor;
            _mVT = mVT;
            _aSm = aSM;
        }


        //#region :: Variables Declaration ::
        //public static string conStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        //SCMExceptionHandlingWorkRole scmException = new SCMExceptionHandlingWorkRole();
        //#endregion Variables Declaration

        #region :: Private Methods ::
        private async Task<bool> InsertFFRToOutBox(string Body, string FromId, string ToID)
        {
            //SQLServer dtb = new SQLServer();
            DataSet? ds = new DataSet();
            DataSet? objDS = null;
            bool flag = false;
            try
            {
                //string[] Pname = new string[3];
                //object[] Pvalue = new object[3];
                //SqlDbType[] Ptype = new SqlDbType[3];

                //Pname[0] = "body";
                //Ptype[0] = SqlDbType.VarChar;
                //Pvalue[0] = Body;

                //Pname[1] = "fromId";
                //Ptype[1] = SqlDbType.VarChar;
                //Pvalue[1] = FromId;

                //Pname[2] = "toId";
                //Ptype[2] = SqlDbType.VarChar;
                //Pvalue[2] = ToID;

                //objDS = dtb.SelectRecords("spSaveFFRToOutbox", Pname, Pvalue, Ptype);

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@body", SqlDbType.VarChar) { Value = Body },
                    new SqlParameter("@fromId", SqlDbType.VarChar) { Value = FromId },
                    new SqlParameter("@toId", SqlDbType.VarChar) { Value = ToID }
                };

                objDS = await _readWriteDao.SelectRecords("spSaveFFRToOutbox", parameters);

                if (objDS != null)
                {
                    if (objDS.Tables.Count > 0)
                    {
                        if (objDS.Tables[0].Rows.Count > 0)
                        {
                            flag = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                flag = false;
            }
            //finally
            //{
            //    dtb = null;
            //    objDS = null;
            //}
            //GC.Collect();

            return flag;
        }

        private async Task<DataSet?> GetAWBDetailsForFSA(string AWBNo, string msg, string AWBPrefix)
        {
            DataSet? dsAWB = new DataSet();
            try
            {
                //SQLServer dtb = new SQLServer();
                //string[] paramname = new string[] { "AWBNo", "msg", "AWBPrefix" };
                //object[] paramvalue = new object[] { AWBNo, msg, AWBPrefix };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                //dsAWB = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);

                string procedure = "spGetAWBDetailsForFSA1";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@AWBNo", SqlDbType.VarChar) { Value = AWBNo },
                    new SqlParameter("@msg", SqlDbType.VarChar) { Value = msg },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                };

                dsAWB = await _readWriteDao.SelectRecords(procedure, parameters);

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                dsAWB = null;
            }
            return dsAWB;
        }

        private async Task<DataSet?> getFlightDetailsForFSA(string AWBNo, string FlightNo, string AWBPrefix)
        {
            DataSet? dsFlight = new DataSet();
            try
            {
                //SQLServer dtb = new SQLServer();
                //string[] paramname = new string[] { "AWBNo", "FlightNo", "AWBPrefix" };
                //object[] paramvalue = new object[] { AWBNo, FlightNo, AWBPrefix };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                //dsFlight = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);

                string procedure = "spGetFlightDetailsForFSA";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@AWBNo", SqlDbType.VarChar) { Value = AWBNo },
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                };

                dsFlight = await _readWriteDao.SelectRecords(procedure, parameters);

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                dsFlight = null;
            }
            return dsFlight;
        }

        private async Task<bool> addMsgToOutBox(string subject, string Msg, string FromEmailID, string ToEmailID, string agent, int refNo)
        {
            bool flag = false;
            try
            {
                //SQLServer dtb = new SQLServer();

                //string[] paramname = new string[] { "Subject",
                //                                "Body",
                //                                "FromEmailID",
                //                                "ToEmailID",
                //                                "CreatedOn",
                //                                "Agentcode",
                //                                "refNo"};

                //object[] paramvalue = new object[] {subject,
                //                                Msg,
                //                                FromEmailID,
                //                                ToEmailID,
                //                                System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //                                agent,
                //                                refNo};

                //SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.DateTime,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.Int};

                //flag = dtb.InsertData(procedure, paramname, paramtype, paramvalue);

                string procedure = "spInsertMsgToOutbox";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@Subject", SqlDbType.VarChar) { Value = subject },
                    new SqlParameter("@Body", SqlDbType.VarChar) { Value = Msg },
                    new SqlParameter("@FromEmailID", SqlDbType.VarChar) { Value = FromEmailID },
                    new SqlParameter("@ToEmailID", SqlDbType.VarChar) { Value = ToEmailID },
                    new SqlParameter("@CreatedOn", SqlDbType.DateTime) { Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                    new SqlParameter("@Agentcode", SqlDbType.VarChar) { Value = agent },
                    new SqlParameter("@refNo", SqlDbType.Int) { Value = refNo }
                };

                flag = await _readWriteDao.ExecuteNonQueryAsync(procedure, parameters);

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                flag = false;
            }
            return flag;
        }

        private async Task<bool> InsertFFAIntoOutbox(object[] RateCardInfo)
        {
            bool flag = false;
            try
            {
                //SQLServer da = new SQLServer();
                //DataSet ds = new DataSet();


                //string[] ColumnNames = new string[3];
                //SqlDbType[] DataType = new SqlDbType[3];
                //Object[] Values = new object[3];
                //int i = 0;

                //i = 0;
                ////0
                //ColumnNames.SetValue("body", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(RateCardInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("username", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(RateCardInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("result", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue("", i);

                //flag = da.UpdateData("spSaveFFAToOutbox", ColumnNames, DataType, Values);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@body", SqlDbType.VarChar) { Value = RateCardInfo.GetValue(0) },
                    new SqlParameter("@username", SqlDbType.VarChar) { Value = RateCardInfo.GetValue(1) },
                    new SqlParameter("@result", SqlDbType.VarChar) { Value = "" }
                };

                flag = await _readWriteDao.ExecuteNonQueryAsync("spSaveFFAToOutbox", parameters);

                return flag;
            }

            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return flag;
            }
        }

        private async Task<bool> validateAndInsertUCMData(MessageData.UCMInfo ucmdata, MessageData.ULDinfo[] uld)
        {
            bool flag = false;
            try
            {
                //SQLServer dtb = new SQLServer();
                string date = System.DateTime.Now.ToString("yyyy-MM-dd");

                int res;
                if (int.TryParse(ucmdata.Date.ToString(), out res))
                {
                    date = ucmdata.Date.PadLeft(2, '0') + "/" + System.DateTime.Now.Month.ToString().PadLeft(2, '0') + "/" + System.DateTime.Now.Year.ToString();
                }
                string strmonth = string.Empty;
                string switchMonth = ucmdata.Date.Trim().Length > 2 ? ucmdata.Date.Trim().Substring(2).ToUpper() : System.DateTime.Now.ToString("MMM").ToUpper();
                switch (switchMonth)
                {
                    case "JAN":
                        {
                            strmonth = "01";
                            break;
                        }
                    case "FEB":
                        {
                            strmonth = "02";
                            break;
                        }
                    case "MAR":
                        {
                            strmonth = "03";
                            break;
                        }
                    case "APR":
                        {
                            strmonth = "04";
                            break;
                        }
                    case "MAY":
                        {
                            strmonth = "05";
                            break;
                        }
                    case "JUN":
                        {
                            strmonth = "06";
                            break;
                        }
                    case "JUL":
                        {
                            strmonth = "07";
                            break;
                        }
                    case "AUG":
                        {
                            strmonth = "08";
                            break;
                        }
                    case "SEP":
                        {
                            strmonth = "09";
                            break;
                        }
                    case "OCT":
                        {
                            strmonth = "10";
                            break;
                        }
                    case "NOV":
                        {
                            strmonth = "11";
                            break;
                        }
                    case "DEC":
                        {
                            strmonth = "12";
                            break;
                        }
                }
                date = strmonth + "/" + Convert.ToString(ucmdata.Date).Substring(0, 2) + "/" + System.DateTime.Now.Year.ToString();

                #region ULD
                string origin = "";
                string dest = "";
                string flight = "";
                for (int i = 0; i < uld.Length; i++)
                {
                    if (uld[i].movement.Equals("IN", StringComparison.OrdinalIgnoreCase))
                    {
                        origin = uld[i].stationcode.Trim();
                        dest = ucmdata.StationCode.Trim();
                        flight = ucmdata.FltNo.Trim();
                    }
                    if (uld[i].movement.Equals("OUT", StringComparison.OrdinalIgnoreCase))
                    {
                        origin = ucmdata.StationCode.Trim();
                        dest = uld[i].stationcode.Trim();
                        flight = ucmdata.OutFltNo.Trim();
                    }

                    //if (uld[i].movement.Equals("IN", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    if (!string.IsNullOrEmpty(uld[i].stationcode.Trim()))
                    //    {
                    //        origin = uld[i].stationcode.Trim();
                    //    }
                    //    else
                    //    {
                    //        origin = ucmdata.StationCode.Trim();
                    //    }                     
                    //    dest = "";
                    //    flight = ucmdata.FltNo.Trim();
                    //}
                    //if (uld[i].movement.Equals("OUT", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    //origin = ucmdata.StationCode.Trim();
                    //    origin = "";
                    //    dest = uld[i].stationcode.Trim();
                    //    flight = ucmdata.OutFltNo.Trim();
                    //}

                    //string[] paramname = new string[]
                    //{   "ULDNo",
                    //    "LocatedOn",
                    //    "MovType",
                    //    "CargoIndic",
                    //    "Ori",
                    //    "dest",
                    //    "FltNo"
                    //};

                    //object[] paramvalue = new object[]
                    //{   uld[i].uldno.Trim(),
                    //    Convert.ToDateTime(date),
                    //    uld[i].movement.Trim(),
                    //    uld[i].uldloadingindicator.Trim(),
                    //    origin,
                    //    dest,
                    //    flight
                    //};

                    //SqlDbType[] paramtype = new SqlDbType[]
                    //{  SqlDbType.NVarChar,
                    //     SqlDbType.DateTime,
                    //     SqlDbType.NVarChar,
                    //     SqlDbType.NVarChar,
                    //     SqlDbType.NVarChar,
                    //     SqlDbType.NVarChar,
                    //    SqlDbType.NVarChar
                    //};

                    //string procedure = "spUpdateviaUCMMsgFFM";
                    //flag = dtb.InsertData(procedure, paramname, paramtype, paramvalue);


                    SqlParameter[] parameters =
                    {
                        new SqlParameter("@ULDNo", SqlDbType.NVarChar) { Value = uld[i].uldno.Trim() },
                        new SqlParameter("@LocatedOn", SqlDbType.DateTime) { Value = Convert.ToDateTime(date) },
                        new SqlParameter("@MovType", SqlDbType.NVarChar) { Value = uld[i].movement.Trim() },
                        new SqlParameter("@CargoIndic", SqlDbType.NVarChar) { Value = uld[i].uldloadingindicator.Trim() },
                        new SqlParameter("@Ori", SqlDbType.NVarChar) { Value = origin },
                        new SqlParameter("@dest", SqlDbType.NVarChar) { Value = dest },
                        new SqlParameter("@FltNo", SqlDbType.NVarChar) { Value = flight }
                    };

                    flag = await _readWriteDao.ExecuteNonQueryAsync("spUpdateviaUCMMsgFFM", parameters);

                }
                #endregion
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                flag = false;
            }
            return flag;
        }

        private static void ReProcessConsigment(ref MessageData.consignmnetinfo[] FFMConsig, ref MessageData.consignmnetinfo[] consinfo)
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
                        }
                    }
                    if (!AWBMatch)
                    {
                        Array.Resize(ref consinfo, consinfo.Length + 1);
                        Array.Copy(FFMConsig, i, consinfo, consinfo.Length - 1, 1);
                    }
                }
            }
            catch (Exception ex) { clsLog.WriteLogAzure(ex); }
        }

        private XmlNode RenameNode(XmlNode e, string newName)
        {
            XmlDocument doc = e.OwnerDocument;
            XmlNode newNode = doc.CreateNode(e.NodeType, newName, null);
            while (e.HasChildNodes)
            {
                newNode.AppendChild(e.FirstChild);
            }
            XmlAttributeCollection ac = e.Attributes;
            while (ac.Count > 0)
            {
                newNode.Attributes.Append(ac[0]);
            }
            XmlNode parent = e.ParentNode;
            parent.ReplaceChild(newNode, e);
            return newNode;
        }

        private async Task<bool> validateAndInsertSCMData(MessageData.SCMInfo[] scm)
        {
            bool flag = false;
            try
            {
                //SQLServer dtb = new SQLServer();

                //DataTable dt = new DataTable();
                //DataRow row;
                //dt.Columns.Add("ULDNo", typeof(string));
                //dt.Columns.Add("StationCode", typeof(string));
                //dt.Columns.Add("Date", typeof(string));
                //dt.Columns.Add("ULDType", typeof(string));
                //dt.Columns.Add("ULDSrNo", typeof(string));
                //dt.Columns.Add("ULDOwner", typeof(string));
                //dt.Columns.Add("Movement", typeof(string));
                //dt.Columns.Add("UldStatus", typeof(string));

                //foreach (var item in scm)
                //{
                //    row = dt.NewRow();
                //    row["ULDNo"] = item.uldno;
                //    row["StationCode"] = item.StationCode;
                //    row["Date"] = item.Date;
                //    row["ULDType"] = item.uldtype;
                //    row["ULDSrNo"] = item.uldsrno;
                //    row["ULDOwner"] = item.uldowner;
                //    row["Movement"] = item.movement;
                //    row["UldStatus"] = item.uldstatus;
                //    dt.Rows.Add(row);

                //}

                //string[] paramname = new string[] { "SCMData" };

                //object[] paramvalue = new object[] { dt };

                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.Structured };

                //string procedure = "spUpdateviaSCMmsg";
                //flag = dtb.InsertData(procedure, paramname, paramtype, paramvalue);


                // Build DataTable for SCM data
                DataTable dt = new DataTable();
                dt.Columns.Add("ULDNo", typeof(string));
                dt.Columns.Add("StationCode", typeof(string));
                dt.Columns.Add("Date", typeof(string));
                dt.Columns.Add("ULDType", typeof(string));
                dt.Columns.Add("ULDSrNo", typeof(string));
                dt.Columns.Add("ULDOwner", typeof(string));
                dt.Columns.Add("Movement", typeof(string));
                dt.Columns.Add("UldStatus", typeof(string));

                foreach (var item in scm)
                {
                    DataRow row = dt.NewRow();
                    row["ULDNo"] = item.uldno;
                    row["StationCode"] = item.StationCode;
                    row["Date"] = item.Date;
                    row["ULDType"] = item.uldtype;
                    row["ULDSrNo"] = item.uldsrno;
                    row["ULDOwner"] = item.uldowner;
                    row["Movement"] = item.movement;
                    row["UldStatus"] = item.uldstatus;
                    dt.Rows.Add(row);
                }

                // Define table-valued parameter
                SqlParameter[] parameters =
                {
                    new SqlParameter("@SCMData", SqlDbType.Structured)
                    {
                        TypeName = "SCMULDDataType",
                        Value = dt
                    }
                };

                flag = await _readWriteDao.ExecuteNonQueryAsync("spUpdateviaSCMmsg", parameters);


                if (!flag)
                {
                    clsLog.WriteLogAzure("Error in Updating SCM Data");
                }

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                flag = false;
            }
            return flag;
        }


        #endregion Private Methods

        #region :: Public Methods ::
        public async Task<DataSet?> getAcceptedBookingData(string pName)
        {
            DataSet? ds = new DataSet();
            try
            {
                //SQLServer dtb = new SQLServer();
                //ds = dtb.SelectRecords(pName);
                //string procedure = pName;

                ds = await _readWriteDao.SelectRecords(pName);

                //dtb = null;
                //GC.Collect();
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            return ds;
        }

        //public async Task<bool> addBookingFromMsg(string strMsg, int refNO, string strMessageFrom, out string msgType, string strFromID, string strStatus, string PIMAAddress, out string Errmsg)
        public async Task<(bool flag, string msgType, string Errmsg)> addBookingFromMsg(string strMsg, int refNO, string strMessageFrom, string strFromID, string strStatus, string PIMAAddress)

        {
            bool flag = false, fltlevel = false;
            string msgType = string.Empty;
            string Errmsg = string.Empty;
            string strOriginalMessage = string.Empty, awbnumber = string.Empty;
            string ErrorMsg = string.Empty, carrierCode = string.Empty;
            DateTime flightdate = DateTime.UtcNow;

            //GenericFunction genericFunction = new GenericFunction();
            try
            {
                if (strMsg != null)
                {
                    if (strMsg.Trim() != "")
                    {
                        strOriginalMessage = strMsg.Trim();

                        #region Remove Extra Characters From Message
                        string tempMessage = string.Empty;
                        tempMessage = strMsg;
                        string strRmvHdr = strMsg;
                        string[] arrTempMessage;
                        string[] arrFHLMessage;
                        string dis;


                        if (tempMessage.Contains("DISCLAIMER"))
                        {
                            dis = tempMessage.Substring(tempMessage.IndexOf("DISCLAIMER"), tempMessage.Length - tempMessage.IndexOf("DISCLAIMER"));
                            tempMessage = tempMessage.Replace(dis, "");
                            if (tempMessage.Contains("*"))
                            {
                                tempMessage = tempMessage.Substring(0, (tempMessage.IndexOf("*")));
                            }
                            if (tempMessage.Contains("--"))    // 807-120256565
                            {
                                tempMessage = tempMessage.Substring(0, (tempMessage.IndexOf("--")));
                            }
                            tempMessage = tempMessage.Replace("\r\n\r\n", "$");
                            tempMessage = tempMessage.Replace("\n\n", "$").Trim('$');
                            arrTempMessage = tempMessage.Split('$');
                            strMsg = tempMessage;
                        }
                        else
                        {
                            tempMessage = tempMessage.Replace("\r\n\r\n", "$");

                            tempMessage = tempMessage.Replace("\n\n", "$").Trim('$');
                            arrTempMessage = tempMessage.Split('$');
                        }
                        if (arrTempMessage.Length > 1)
                        {
                            var gMAILMailINServer = ConfigCache.Get("GMAILMailINServer").ToUpper();

                            if (arrTempMessage[0].Contains("TEXT/PLAIN") && arrTempMessage[0].Contains("CONTENT-TYPE"))
                            {
                                string msg = string.Empty;
                                for (int i = 1; i < arrTempMessage.Length; i++)
                                {
                                    string tmp = arrTempMessage[i].Replace("\r\n", "$");
                                    tmp = arrTempMessage[i].Replace("\n", "$");
                                    string[] arrTmp = tmp.Split('$');
                                    if (arrTmp.Length > 1 && arrTmp[arrTmp.Length - 1].StartsWith("---"))
                                    {
                                        if (arrTempMessage[i].Contains("\r\n"))
                                            arrTempMessage[i] = arrTempMessage[i].Remove(arrTempMessage[i].LastIndexOf("\r\n"));
                                        else
                                            arrTempMessage[i] = arrTempMessage[i].Remove(arrTempMessage[i].LastIndexOf("\n"));
                                        msg = msg + arrTempMessage[i] + "$";
                                    }
                                    else if (arrTmp.Length > 1)
                                        msg = msg + arrTempMessage[i] + "$";
                                }
                                strMsg = msg.Trim('$');
                            }
                            //else if (_genericFunction.GetConfigurationValues("GMAILMailINServer").ToUpper() == "OUTLOOK.OFFICE365.COM")
                            else if (gMAILMailINServer == "OUTLOOK.OFFICE365.COM")

                            {
                                if ((arrTempMessage[0].Trim().StartsWith("ATTENTION:", StringComparison.OrdinalIgnoreCase)
                                    || arrTempMessage[0].Trim().StartsWith("CAUTION:", StringComparison.OrdinalIgnoreCase)) && arrTempMessage.Length > 1)
                                {
                                    if (arrTempMessage[1].Contains("\r\n") && arrTempMessage[1].Contains("***"))
                                        strMsg = arrTempMessage[1].Remove(arrTempMessage[1].LastIndexOf("\r\n")).Trim();
                                    //else if (arrTempMessage.Length == 3 && (arrTempMessage[0].Trim().StartsWith("ATTENTION:", StringComparison.OrdinalIgnoreCase)
                                    //|| arrTempMessage[0].Trim().StartsWith("CAUTION:", StringComparison.OrdinalIgnoreCase)) && arrTempMessage[2].Contains("***"))
                                    //{
                                    //    strMsg = arrTempMessage[1].Trim();
                                    //}
                                    else
                                        strMsg = arrTempMessage[1].Trim();
                                }
                            }
                            else if (arrTempMessage[0].Trim().StartsWith("FFR", StringComparison.OrdinalIgnoreCase))
                            {
                                strMsg = arrTempMessage[0].Trim();
                            }
                            else if (arrTempMessage[0].Trim().StartsWith("FWB", StringComparison.OrdinalIgnoreCase))
                            {
                                strMsg = arrTempMessage[0].Trim();
                            }
                            else if (arrTempMessage[0].Trim().StartsWith("FBL", StringComparison.OrdinalIgnoreCase))
                            {
                                strMsg = arrTempMessage[0].Trim();
                            }
                        }

                        if (tempMessage.ToUpper().Contains("THIS MESSAGE WAS SENT FROM OUTSIDE OF YOUR ORGANIZATION."))
                        {
                            string[] arrRemoveHeader;
                            strRmvHdr = strRmvHdr.Replace("\r\n\r\n", "$");
                            strRmvHdr = strRmvHdr.Replace("\n\n", "$").Trim('$');
                            arrRemoveHeader = strRmvHdr.Split('$');
                            for (int i = 0; i < arrRemoveHeader.Length; i++)
                            {
                                if (!arrRemoveHeader[i].ToUpper().Contains("THIS MESSAGE WAS SENT FROM OUTSIDE OF YOUR ORGANIZATION.")
                                    && !arrRemoveHeader[i].ToUpper().Contains("YOU DON'T OFTEN GET EMAIL FROM")
                                    && arrRemoveHeader[i].Trim().Length > 3)
                                {
                                    strMsg = arrRemoveHeader[i].Trim();
                                    break;
                                }
                            }
                        }

                        tempMessage = strMsg.Replace("FHL/", "#FHL/");
                        arrFHLMessage = tempMessage.Split('#');

                        #endregion

                        if (!strMsg.Trim().StartsWith("ASM", StringComparison.OrdinalIgnoreCase) && !strMsg.Trim().StartsWith("SSM", StringComparison.OrdinalIgnoreCase))
                        {
                            strMsg = strMsg.Replace("\r\n", "$");
                            strMsg = strMsg.Replace("\n", "$");
                            strMsg = strMsg.Replace("$$", "$");
                            strMsg = strMsg.Trim('$');
                        }
                        if (strMsg.Trim().StartsWith("FFR", StringComparison.OrdinalIgnoreCase))
                        {
                            #region FFR
                            msgType = "FFR";
                            MessageData.ffrinfo objFFRData = new MessageData.ffrinfo("");
                            MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
                            MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
                            MessageData.FltRoute[] objRouteInfo = new MessageData.FltRoute[0];
                            MessageData.dimensionnfo[] objDimension = new MessageData.dimensionnfo[0];
                            FFRMessageProcessor ffrMessage = new FFRMessageProcessor();
                            flag = ffrMessage.DecodeFFRReceiveMessage(refNO, strMsg, ref objFFRData, ref objULDInfo, ref objConsInfo, ref objRouteInfo, ref objDimension, out ErrorMsg);

                            if (flag == true)
                                flag = ffrMessage.ValidaeSaveFFRMessage(objFFRData, objConsInfo, objRouteInfo, objDimension, refNO, strOriginalMessage, strMessageFrom, strFromID, strStatus, out ErrorMsg, objULDInfo);

                            if (!flag)
                            {
                                Errmsg = ErrorMsg;
                            }

                            //string autoReprocessFFROnExpectedRoute = genericFunction.ReadValueFromDb("AutoReprocessFFROnExpectedRoute");

                            string autoReprocessFFROnExpectedRoute = ConfigCache.Get("AutoReprocessFFROnExpectedRoute");

                            if (ErrorMsg.ToUpper() == "FLIGHT NUMBER IS INVALID" && autoReprocessFFROnExpectedRoute.ToUpper() == "TRUE")
                            {
                                if (!flag)
                                {
                                    ErrorMsg = string.Empty;
                                    Errmsg = string.Empty;
                                    //ReconstructFFRoute(objConsInfo, ref objRouteInfo, refNO);
                                    objRouteInfo = await ReconstructFFRoute(objConsInfo, objRouteInfo, refNO);

                                    flag = ffrMessage.ValidaeSaveFFRMessage(objFFRData, objConsInfo, objRouteInfo, objDimension, refNO, strOriginalMessage, strMessageFrom, strFromID, strStatus, out ErrorMsg, objULDInfo);
                                    Errmsg = ErrorMsg;
                                }
                            }

                            #endregion
                        }
                        else if (strMsg.Trim().StartsWith("FWB", StringComparison.OrdinalIgnoreCase))
                        {
                            #region FWB
                            msgType = "FWB";
                            MessageData.fwbinfo fwbdata = new MessageData.fwbinfo("");
                            MessageData.FltRoute[] fltroute = new MessageData.FltRoute[0];
                            MessageData.othercharges[] OtherCharges = new MessageData.othercharges[0];
                            MessageData.otherserviceinfo[] othinfoarray = new MessageData.otherserviceinfo[0];
                            MessageData.RateDescription[] fwbrates = new MessageData.RateDescription[0];
                            MessageData.customsextrainfo[] customextrainfo = new MessageData.customsextrainfo[0];
                            MessageData.dimensionnfo[] objDimension = new MessageData.dimensionnfo[0];
                            MessageData.AWBBuildBUP[] objAwbBup = new MessageData.AWBBuildBUP[0];
                            FWBMessageProcessor fwbProcessor = new FWBMessageProcessor();

                            //CIMPMessageValidation cimpMessageValidation = new CIMPMessageValidation();

                            flag = fwbProcessor.DecodeReceiveFWBMessage(strMsg, ref fwbdata, ref fltroute, ref OtherCharges, ref othinfoarray, ref fwbrates,
                                ref customextrainfo, ref objDimension, ref objAwbBup, refNO, out ErrorMsg);
                            if (flag == true && _cIMPMessageValidation.ValidateFWB(strMsg, refNO, out ErrorMsg))
                                flag = fwbProcessor.SaveandValidateFWBMessage(fwbdata, fltroute, OtherCharges, othinfoarray, fwbrates, customextrainfo, objDimension, refNO, objAwbBup, strOriginalMessage, strMessageFrom, strFromID, strStatus, PIMAAddress, out ErrorMsg);
                            else
                                flag = false;

                            if (!flag)
                            {
                                Errmsg = ErrorMsg;
                                if (Errmsg.Trim().Length > 0)
                                {
                                    FNAMessageProcessor fnaMessageProcessor = new FNAMessageProcessor();
                                    string commtype = string.Empty;
                                    if (strFromID.Contains("SITA"))
                                        commtype = "SITAFTP";
                                    else
                                        commtype = "EMAIL";
                                    fnaMessageProcessor.GenerateFNAMessage(strOriginalMessage, Errmsg, fwbdata.airlineprefix, fwbdata.awbnum, strMessageFrom == "" ? strFromID : strMessageFrom, commtype, PIMAAddress);
                                }
                            }

                            #endregion
                        }
                        else if (strMsg.Trim().StartsWith("FFM", StringComparison.OrdinalIgnoreCase))
                        {
                            #region FFM
                            strMsg = strMsg.Replace("$LAST$", "$LAST#");
                            strMsg = strMsg.Replace("$CONT$", "$CONT#");
                            strMsg = strMsg.Split('#')[0];
                            msgType = "FFM";
                            flag = false;
                            MessageData.ffminfo ffmdata = new MessageData.ffminfo("");
                            MessageData.unloadingport[] unloadingport = new MessageData.unloadingport[0];
                            MessageData.consignmnetinfo[] consinfo = new MessageData.consignmnetinfo[0];
                            MessageData.dimensionnfo[] dimensioinfo = new MessageData.dimensionnfo[0];
                            MessageData.ULDinfo[] uld = new MessageData.ULDinfo[0];
                            MessageData.otherserviceinfo[] othinfoarray = new MessageData.otherserviceinfo[0];
                            MessageData.customsextrainfo[] custominfo = new MessageData.customsextrainfo[0];
                            MessageData.movementinfo[] movementinfo = new MessageData.movementinfo[0];
                            FFMMessageProcessor ffmMessage = new FFMMessageProcessor();
                            //string ErrorMsg = string.Empty;
                            flag = ffmMessage.DecodeReceiveFFMMessage(refNO, strMsg, ref ffmdata, ref unloadingport, ref consinfo, ref dimensioinfo, ref uld, ref othinfoarray, ref custominfo, ref movementinfo, out ErrorMsg);
                            bool IsRelayFFM = false;
                            fltlevel = true;

                            //if (bool.TryParse(genericFunction.GetConfigurationValues("IsRelay_FFM"), out IsRelayFFM))

                            if (bool.TryParse(ConfigCache.Get("IsRelay_FFM"), out IsRelayFFM))
                            {
                                carrierCode = ffmdata.carriercode;
                                flightdate = DateTime.Parse(DateTime.Parse("1." + ffmdata.month + " 2008").Month.ToString().PadLeft(2, '0') + "/" + ffmdata.fltdate.PadLeft(2, '0') + "/" + +System.DateTime.Today.Year);
                                RelayMessages("FFM", strMsg, carrierCode, flightdate, fltlevel, ffmdata.fltairportcode, unloadingport[0].unloadingairport, carrierCode + ffmdata.fltnum, awbnumber);
                            }
                            if (flag == true)
                            {
                                flag = ffmMessage.SaveValidateFFMMessage(ref ffmdata, ref consinfo, ref unloadingport, ref dimensioinfo, ref uld, refNO, strOriginalMessage, strMessageFrom, strFromID, strStatus, strMsg, PIMAAddress, out ErrorMsg);
                            }
                            if (!flag)
                            {
                                Errmsg = ErrorMsg;
                            }

                            #endregion
                        }
                        else if (strMsg.Trim().StartsWith("FSU", StringComparison.OrdinalIgnoreCase)
                          || strMsg.Trim().StartsWith("FSA", StringComparison.OrdinalIgnoreCase))
                        {
                            #region FSU / FSA
                            msgType = "FSU";
                            MessageData.FSAInfo fsadata = new MessageData.FSAInfo("");
                            MessageData.CommonStruct[] fsanodes = new MessageData.CommonStruct[0];
                            MessageData.customsextrainfo[] customextrainfo = new MessageData.customsextrainfo[0];
                            MessageData.ULDinfo[] ulddata = new MessageData.ULDinfo[0];
                            MessageData.otherserviceinfo[] othinfoarray = new MessageData.otherserviceinfo[0];
                            FSUMessageProcessor fsumeessage = new FSUMessageProcessor();
                            flag = fsumeessage.DecodeReceivedFSUMessage(refNO, strMsg, ref fsadata, ref fsanodes, ref customextrainfo, ref ulddata, ref othinfoarray);

                            if (flag == true)
                                flag = fsumeessage.SaveandUpdateFSUMessage(strMsg, ref fsadata, ref fsanodes, ref customextrainfo, ref ulddata, ref othinfoarray, refNO, strOriginalMessage, strMessageFrom, strFromID, strStatus, out ErrorMsg);

                            if (!flag)
                            {
                                Errmsg = ErrorMsg;
                            }
                            #endregion
                        }
                        else if (strMsg.Trim().StartsWith("FHL", StringComparison.OrdinalIgnoreCase))
                        {
                            #region FHL
                            msgType = "FHL";
                            MessageData.fhlinfo fhl = new MessageData.fhlinfo("");
                            MessageData.consignmnetinfo[] consinfo;//= new MessageData.consignmnetinfo[0];
                            MessageData.customsextrainfo[] customextrainfo = new MessageData.customsextrainfo[0];
                            FHLMessageProcessor fhlMessage = new FHLMessageProcessor();
                            if (arrFHLMessage.Length > 1)
                            {
                                if (arrFHLMessage[1].Contains("FHL"))
                                {
                                    for (int i = 1; i < arrFHLMessage.Length; i++)
                                    {
                                        strMsg = arrFHLMessage[i];
                                        strMsg = strMsg.Replace("\r\n", "$");
                                        strMsg = strMsg.Replace("\n", "$");
                                        strMsg = strMsg.Replace("$$", "$");
                                        strMsg = strMsg.Trim('$');
                                        consinfo = new MessageData.consignmnetinfo[0];
                                        flag = fhlMessage.DecodeReceiveFHLMessage(strMsg, ref fhl, ref consinfo, ref customextrainfo, out ErrorMsg);

                                        if (flag == true)
                                            flag = fhlMessage.validateAndInsertFHLData(ref fhl, ref consinfo, ref customextrainfo, refNO, strOriginalMessage, strMessageFrom, strFromID, strStatus);

                                        if (!flag)
                                        {
                                            Errmsg = ErrorMsg;
                                        }
                                    }

                                }
                            }

                            #endregion
                        }
                        else if (strMsg.Trim().StartsWith("FFA", StringComparison.OrdinalIgnoreCase))
                        {
                            #region FFA
                            msgType = "FFA";
                            FFAMessageProcessor ffaProcessor = new FFAMessageProcessor();
                            MessageData.ffainfo objFFAData = new MessageData.ffainfo("");
                            MessageData.ffainfo[] flightinfo = new MessageData.ffainfo[0];

                            flag = ffaProcessor.DecodeReceivedFFAMessage(strMsg, ref objFFAData, ref flightinfo);

                            if (flag)
                                ffaProcessor.SaveandUpdateFFAMessage(objFFAData, refNO, flightinfo);

                            #endregion
                        }

                        else if (strMsg.Trim().StartsWith("FNA", StringComparison.OrdinalIgnoreCase) || strMsg.Trim().StartsWith("FMA", StringComparison.OrdinalIgnoreCase))
                        {
                            if (strMsg.Trim().StartsWith("FNA", StringComparison.OrdinalIgnoreCase))
                            {
                                msgType = "FNA";
                            }
                            else
                            {
                                msgType = "FMA";
                            }
                            FNAMessageProcessor fnaprocessor = new FNAMessageProcessor();
                            MessageData.FNA objfnadata = new MessageData.FNA("");
                            flag = fnaprocessor.DecodeFNAMessage(strMsg, ref objfnadata);
                            if (flag)
                                fnaprocessor.SaveAndValidateFNAMessage(refNO, objfnadata);
                        }
                        else if (strMsg.Trim().StartsWith("FBR", StringComparison.OrdinalIgnoreCase) || strMsg.Trim().StartsWith("FBR/"))
                        {
                            #region FWR Decoding and save the record
                            msgType = "FBR";
                            FBRMessageProcessor fbrProcessor = new FBRMessageProcessor();
                            MessageData.FBRInformation fbrMessageInformation = new MessageData.FBRInformation();

                            flag = fbrProcessor.DecodingFBRMessge(strMsg, fbrMessageInformation);
                            if (flag == true)
                                flag = fbrProcessor.ValidatandGenerateFBLMessage(fbrMessageInformation, strOriginalMessage, refNO, strMessageFrom, strFromID, strStatus);
                            #endregion
                        }

                        else if (strMsg.Trim().StartsWith("FBL", StringComparison.OrdinalIgnoreCase))
                        {
                            #region FBL Message
                            msgType = "FBL";
                            flag = false;
                            MessageData.fblinfo fbldata = new MessageData.fblinfo("");
                            MessageData.unloadingport[] unloadingport = new MessageData.unloadingport[0];
                            MessageData.consignmnetinfo[] consinfo = new MessageData.consignmnetinfo[0];
                            MessageData.dimensionnfo[] dimensioinfo = new MessageData.dimensionnfo[0];
                            MessageData.ULDinfo[] uld = new MessageData.ULDinfo[0];
                            MessageData.otherserviceinfo[] othinfoarray = new MessageData.otherserviceinfo[0];
                            MessageData.customsextrainfo[] custominfo = new MessageData.customsextrainfo[0];
                            MessageData.consignmentorigininfo[] consigmnentOrigin = new MessageData.consignmentorigininfo[0];
                            MessageData.movementinfo[] movementinfo = new MessageData.movementinfo[0];
                            FBLMessageProcessor fblProcessor = new FBLMessageProcessor();
                            flag = fblProcessor.DecodeReceiveFBLMessage(strMsg, ref fbldata, ref unloadingport, ref dimensioinfo, ref uld, ref othinfoarray, ref consigmnentOrigin, ref consinfo, ref othinfoarray);

                            if (flag == true)
                                flag = fblProcessor.SaveandUpdagteFBLMessageinDatabase(ref fbldata, ref unloadingport, ref dimensioinfo, ref uld, ref othinfoarray, ref consigmnentOrigin, ref consinfo, refNO, strOriginalMessage, strMessageFrom, strFromID, strStatus, out ErrorMsg);

                            if (!flag)
                            {
                                Errmsg = ErrorMsg;
                            }
                            #endregion
                        }

                        else if (strMsg.Trim().StartsWith("FSB", StringComparison.OrdinalIgnoreCase) || strMsg.Trim().StartsWith("FSB/"))
                        {
                            #region FSB Decoding and save the record
                            msgType = "FSB";
                            MessageData.FSBAWBInformation fsbMessage = new MessageData.FSBAWBInformation();
                            MessageData.ShipperInformation fsbShipper = new MessageData.ShipperInformation();
                            MessageData.ConsigneeInformation fsbConsignee = new MessageData.ConsigneeInformation();
                            var RouteIformation = new List<MessageData.RouteInformation>();
                            var Dimensionformation = new List<MessageData.FSBDimensionInformation>();
                            var bublistinformation = new List<MessageData.AWBBUPInformation>();
                            FSBMessageProcessor fsbProcessor = new FSBMessageProcessor();
                            flag = fsbProcessor.DecodingFSBMessge(strMsg, fsbMessage, fsbShipper, fsbConsignee, RouteIformation, Dimensionformation, bublistinformation);
                            if (flag == true)
                                flag = fsbProcessor.ValidateAndSaveFSBMessage(fsbMessage, fsbShipper, fsbConsignee, RouteIformation, Dimensionformation, bublistinformation, refNO, strOriginalMessage, strMessageFrom, strFromID, strStatus);
                            else
                                clsLog.WriteLogAzure("FSB Message not reprossed");
                            #endregion
                        }
                        else if (strMsg.Trim().StartsWith("FWR", StringComparison.OrdinalIgnoreCase) || strMsg.Trim().StartsWith("FWR/"))
                        {
                            #region FWR Decoding and save the record
                            msgType = "FWR";
                            FWRMessageProcessor fwrProcessor = new FWRMessageProcessor();
                            MessageData.FWRInformation fwrMessageInformation = new MessageData.FWRInformation();
                            flag = fwrProcessor.DecodingFWRMessge(strMsg, fwrMessageInformation);
                            if (flag == true)
                                flag = fwrProcessor.ValidateAndSendFWBMessage(fwrMessageInformation, refNO, strOriginalMessage, strMessageFrom, strFromID, strStatus);

                            #endregion
                        }
                        else if (strMsg.Trim().StartsWith("FSR", StringComparison.OrdinalIgnoreCase) || strMsg.Trim().StartsWith("FSR/"))
                        {
                            msgType = "FSR";
                            FSRMessageProcessor fsrMessageProcessor = new FSRMessageProcessor();
                            strMessageFrom = strFromID.Contains("@") ? strFromID : strMessageFrom;
                            fsrMessageProcessor.DecodeFSR(strMsg.Trim(), strMessageFrom, PIMAAddress);
                        }
                        else if (strMsg.Trim().StartsWith("UCM", StringComparison.OrdinalIgnoreCase))
                        {
                            #region UCM
                            msgType = "UCM";
                            MessageData.ULDinfo[] uld = new MessageData.ULDinfo[0];
                            MessageData.UCMInfo ucmdata = new MessageData.UCMInfo("");
                            flag = cls_Encode_Decode.decodereceiveUCM(strMsg, ref ucmdata, ref uld);
                            if (flag == true)
                            {
                                flag = await validateAndInsertUCMData(ucmdata, uld);
                            }
                            #endregion
                        }
                        else if (strMsg.Trim().StartsWith("FSC", StringComparison.OrdinalIgnoreCase) ||
                            strMsg.Trim().StartsWith("FSI", StringComparison.OrdinalIgnoreCase) ||
                            strMsg.Trim().StartsWith("FRH", StringComparison.OrdinalIgnoreCase) ||
                            strMsg.Trim().StartsWith("FSN", StringComparison.OrdinalIgnoreCase) ||
                            strMsg.Trim().StartsWith("FXH", StringComparison.OrdinalIgnoreCase) || strMsg.Trim().StartsWith("FER", StringComparison.OrdinalIgnoreCase)
                            || strMsg.Trim().StartsWith("PER", StringComparison.OrdinalIgnoreCase)
                            || strMsg.Trim().StartsWith("PSN", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                //cls_Encode_Decode objDecode = new cls_Encode_Decode();
                                await _cls_Encode_Decode.DecodeCustomsMessage(strMsg, strOriginalMessage, strMessageFrom, out msgType);
                            }
                            catch (Exception ex)
                            {
                                clsLog.WriteLogAzure(ex);
                            }
                        }
                        else if (strMsg.Trim().StartsWith("CPM", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                msgType = "CPM";
                            }
                            catch (Exception ex)
                            { clsLog.WriteLogAzure(ex); }
                        }
                        else if (strMsg.Trim().StartsWith("UNB"))
                        {
                            //CarditResiditManagement crdmanage = new CarditResiditManagement();
                            //flag = crdmanage.EncodeAndSaveCarditMessage(CarditMessage, refNO, out Errormsg);

                            string Errormsg = string.Empty;
                            msgType = "CARDIT";
                            string CarditMessage = strMsg.Replace("$", "");

                            (flag, Errormsg) = await _carditResiditManagement.EncodeAndSaveCarditMessage(CarditMessage, refNO);

                            if (!flag)
                            {
                                Errmsg = Errormsg;
                            }
                        }
                        else if (strMsg.Trim().StartsWith("CGO", StringComparison.OrdinalIgnoreCase))
                        {
                            //CGOProcessor cgoMsgPrcsr = new CGOProcessor();

                            string cgoMessage = strMsg;

                            /*Removed ActCapactiy as it is not used*/
                            //float ActCapactiy = 0;
                            //flag = cgoMsgPrcsr.DecodeReceiveCGOMessage(cgoMessage, ref ActCapactiy);

                            flag = await _cGOProcessor.DecodeReceiveCGOMessage(cgoMessage);
                        }
                        else if (strMsg.Trim().StartsWith("UNB") && (strMsg.Trim().Contains("STS++74")) || (strMsg.Trim().Contains("STS++24")) || (strMsg.Trim().Contains("STS++21")) || (strMsg.Trim().Contains("STS++40")))
                        {
                            //CarditResiditManagement crdmanage = new CarditResiditManagement();

                            string CarditMessage = strMsg.Replace("$", "");
                            flag = await _carditResiditManagement.EncodeAndSaveResditMessage(CarditMessage);
                        }
                        else if (strMsg.Trim().StartsWith("FRP", StringComparison.OrdinalIgnoreCase))
                        {
                            #region FRP
                            string errorMessage = string.Empty;
                            msgType = "FRP";
                            FRPMessageProcessor frpMessageProcessor = new FRPMessageProcessor();
                            MessageData.fwbinfo[] fwbinfo = new MessageData.fwbinfo[1];
                            MessageData.frpinfo[] frpinfo = new MessageData.frpinfo[1];

                            flag = frpMessageProcessor.DecodeFRPReceiveMessage(strMsg, ref fwbinfo, ref frpinfo, ref errorMessage);
                            if (flag == true)
                                flag = frpMessageProcessor.ValidateAndSaveFRPMessage(fwbinfo, frpinfo);

                            #endregion
                        }
                        else if (strMsg.Trim().StartsWith("<?xml") || strMsg.Trim().StartsWith("<rsm:"))
                        {
                            #region XML message

                            string xmlMsg = strMsg.Replace("$", "");
                            ///Oman Custom Response Message:
                            if (strMsg.ToUpper().Contains("<MASTERMANIFESTRESPONSE>"))
                            {
                                flag = false;
                                msgType = MessageData.MessageTypeName.OMCUSTOM_M;
                                CustomsMessageProcessor customsMessageProcessor = new CustomsMessageProcessor();
                                flag = customsMessageProcessor.DecodeAndSaveCustomsMessage(xmlMsg, refNO);

                            }

                            else
                            {
                                #region CXML Messages

                                string xmlMessageName = string.Empty;
                                string originalXMLMsg = xmlMsg;
                                xmlMsg = ReplacingNodeNames(xmlMsg, ref xmlMessageName);
                                if (xmlMessageName.Equals("rsm:FlightManifest"))
                                {
                                    msgType = "XFFM";
                                    flag = false;
                                    MessageData.ffminfo ffmdata = new MessageData.ffminfo("");
                                    MessageData.unloadingport[] unloadingport = new MessageData.unloadingport[0];
                                    MessageData.consignmnetinfo[] consinfo = new MessageData.consignmnetinfo[0];
                                    MessageData.dimensionnfo[] dimensioinfo = new MessageData.dimensionnfo[0];
                                    MessageData.ULDinfo[] uld = new MessageData.ULDinfo[0];
                                    MessageData.otherserviceinfo[] othinfoarray = new MessageData.otherserviceinfo[0];
                                    MessageData.customsextrainfo[] custominfo = new MessageData.customsextrainfo[0];
                                    MessageData.movementinfo[] movementinfo = new MessageData.movementinfo[0];
                                    XFFMMessageProcessor xffmMessageprocessor = new XFFMMessageProcessor();

                                    flag = xffmMessageprocessor.DecodeReceiveFFMMessage(refNO, xmlMsg, ref ffmdata, ref unloadingport, ref consinfo, ref dimensioinfo, ref uld, ref othinfoarray, ref custominfo, ref movementinfo);

                                    if (flag == true)
                                        xffmMessageprocessor.SaveValidateFFMMessage(ref ffmdata, ref consinfo, ref unloadingport, ref dimensioinfo, ref uld, refNO, originalXMLMsg, strMessageFrom, strFromID, strStatus);


                                }
                                else if (xmlMessageName.Equals("rsm:Waybill"))
                                {
                                    msgType = "XFWB";
                                    MessageData.fwbinfo fwbdata = new MessageData.fwbinfo("");
                                    MessageData.FltRoute[] fltroute = new MessageData.FltRoute[0];
                                    MessageData.othercharges[] OtherCharges = new MessageData.othercharges[0];
                                    MessageData.otherserviceinfo[] othinfoarray = new MessageData.otherserviceinfo[0];
                                    MessageData.RateDescription[] fwbrates = new MessageData.RateDescription[0];
                                    MessageData.customsextrainfo[] customextrainfo = new MessageData.customsextrainfo[0];
                                    MessageData.dimensionnfo[] objDimension = new MessageData.dimensionnfo[0];
                                    MessageData.AWBBuildBUP[] objAwbBup = new MessageData.AWBBuildBUP[0];
                                    XFWBMessageProcessor xfwbProcessor = new XFWBMessageProcessor();
                                    flag = xfwbProcessor.DecodeReceiveFWBMessage(xmlMsg, ref fwbdata, ref fltroute,
                                         ref OtherCharges, ref othinfoarray, ref fwbrates, ref customextrainfo, ref objDimension, ref objAwbBup, out ErrorMsg);
                                    if (flag == true)
                                        flag = xfwbProcessor.SaveandValidateFWBMessage(fwbdata, fltroute, OtherCharges, othinfoarray, fwbrates, customextrainfo, objDimension, refNO, objAwbBup, originalXMLMsg, strMessageFrom, strFromID, strStatus, out ErrorMsg);

                                    if (!flag)
                                    {
                                        Errmsg = ErrorMsg;
                                    }

                                }
                                else if (xmlMessageName.Equals("rsm:HouseWaybill"))
                                {
                                    msgType = "XFZB";
                                    MessageData.fhlinfo fhl = new MessageData.fhlinfo("");
                                    MessageData.consignmnetinfo[] consinfo = new MessageData.consignmnetinfo[0];
                                    MessageData.customsextrainfo[] customextrainfo = new MessageData.customsextrainfo[0];
                                    XFZBMessageProcessor xfzbMessage = new XFZBMessageProcessor();

                                    flag = xfzbMessage.DecodeReceiveFHLMessage(xmlMsg, ref fhl, ref consinfo, ref customextrainfo);
                                    if (flag == true)
                                        flag = xfzbMessage.validateAndInsertFHLData(ref fhl, ref consinfo, ref customextrainfo, refNO, originalXMLMsg, strMessageFrom, strFromID, strStatus);

                                }
                                else if (xmlMessageName.Equals("rsm:BookingRequest"))
                                {
                                    msgType = "XFFR";
                                    MessageData.ffrinfo objFFRData = new MessageData.ffrinfo("");
                                    MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
                                    MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
                                    MessageData.FltRoute[] objRouteInfo = new MessageData.FltRoute[0];
                                    MessageData.dimensionnfo[] objDimension = new MessageData.dimensionnfo[0];
                                    XFFRMessageProcessor xffrMessage = new XFFRMessageProcessor();
                                    flag = xffrMessage.DecodeFFRReceiveMessage(xmlMsg, ref objFFRData, ref objULDInfo, ref objConsInfo, ref objRouteInfo, ref objDimension);

                                    if (flag == true)
                                        flag = xffrMessage.ValidaeSaveFFRMessage(objFFRData, objConsInfo, objRouteInfo, objDimension, refNO, originalXMLMsg, strMessageFrom, strFromID, strStatus, out ErrorMsg);

                                    if (!flag)
                                    {
                                        Errmsg = ErrorMsg;
                                    }

                                }
                                else if (xmlMessageName.Equals("rsm:HouseManifest"))
                                {
                                    msgType = "XFHL";
                                    MessageData.fhlinfo fhl = new MessageData.fhlinfo("");
                                    MessageData.consignmnetinfo[] consinfo = new MessageData.consignmnetinfo[0];
                                    MessageData.customsextrainfo[] customextrainfo = new MessageData.customsextrainfo[0];
                                    XFHLMessageProcessor xfhlMessage = new XFHLMessageProcessor();
                                    flag = xfhlMessage.DecodeReceiveFHLMessage(xmlMsg, ref fhl, ref consinfo, ref customextrainfo);

                                    if (flag == true)
                                        flag = xfhlMessage.validateAndInsertFHLData(ref fhl, ref consinfo, ref customextrainfo, refNO, originalXMLMsg, strMessageFrom, strFromID, strStatus);

                                }
                                else if (xmlMessageName.Equals("rsm:FreightBookedList"))
                                {
                                    msgType = "XFBL";
                                    flag = false;
                                    MessageData.fblinfo fbldata = new MessageData.fblinfo("");
                                    MessageData.unloadingport[] unloadingport = new MessageData.unloadingport[0];
                                    MessageData.consignmnetinfo[] consinfo = new MessageData.consignmnetinfo[0];
                                    MessageData.dimensionnfo[] dimensioinfo = new MessageData.dimensionnfo[0];
                                    MessageData.ULDinfo[] uld = new MessageData.ULDinfo[0];
                                    MessageData.otherserviceinfo[] othinfoarray = new MessageData.otherserviceinfo[0];
                                    MessageData.customsextrainfo[] custominfo = new MessageData.customsextrainfo[0];
                                    MessageData.consignmentorigininfo[] consigmnentOrigin = new MessageData.consignmentorigininfo[0];
                                    MessageData.movementinfo[] movementinfo = new MessageData.movementinfo[0];
                                    XFBLMessageProcessor xfblProcessor = new XFBLMessageProcessor();
                                    flag = xfblProcessor.DecodeReceiveFBLMessage(xmlMsg, ref fbldata, ref unloadingport, ref dimensioinfo, ref uld, ref othinfoarray, ref consigmnentOrigin, ref consinfo, ref othinfoarray);

                                    if (flag == true)
                                        xfblProcessor.SaveandUpdagteFBLMessageinDatabase(ref fbldata, ref unloadingport, ref dimensioinfo, ref uld, ref othinfoarray, ref consigmnentOrigin, ref consinfo, refNO, originalXMLMsg, strMessageFrom, strFromID, strStatus);

                                }
                                else if (xmlMessageName.Equals("rsm:Response"))
                                {
                                    msgType = "XFNM";
                                    XFNMMessageProcessor xfnmprocessor = new XFNMMessageProcessor();
                                    MessageData.FNA objfnadata = new MessageData.FNA("");
                                    flag = xfnmprocessor.DecodeXFNMMessage(xmlMsg, ref objfnadata);
                                    if (flag)
                                        xfnmprocessor.SaveAndValidateFNAMessage(refNO, objfnadata);
                                }
                                else if (xmlMessageName.Equals("rsm:StatusMessage"))
                                {
                                    msgType = "XFSU";
                                    MessageData.FSAInfo fsadata = new MessageData.FSAInfo("");
                                    MessageData.CommonStruct[] fsanodes = new MessageData.CommonStruct[0];
                                    MessageData.customsextrainfo[] customextrainfo = new MessageData.customsextrainfo[0];
                                    MessageData.ULDinfo[] ulddata = new MessageData.ULDinfo[0];
                                    MessageData.otherserviceinfo[] othinfoarray = new MessageData.otherserviceinfo[0];
                                    XFSUMessageProcessor xfsumessage = new XFSUMessageProcessor();
                                    flag = xfsumessage.DecodeReceivedXFSUMessage(refNO, xmlMsg, ref fsadata, ref fsanodes, ref customextrainfo, ref ulddata, ref othinfoarray, out ErrorMsg);

                                    if (flag == true)
                                        flag = xfsumessage.SaveandUpdateXFSUMessage(xmlMsg, ref fsadata, ref fsanodes, ref customextrainfo, ref ulddata, ref othinfoarray, refNO, strOriginalMessage, strMessageFrom, strFromID, strStatus, out ErrorMsg);

                                }
                                #endregion
                            }
                            #endregion
                        }
                        else if (strMsg.Trim().StartsWith("SCM", StringComparison.OrdinalIgnoreCase))
                        {
                            #region UCM
                            msgType = "SCM";
                            //MessageData.ULDinfo[] uld = new MessageData.ULDinfo[0];
                            MessageData.SCMInfo[] scm = new MessageData.SCMInfo[0];
                            flag = cls_Encode_Decode.decodereceiveSCM(strMsg, ref scm);
                            if (flag == true)
                            {
                                flag = false;
                                flag = await validateAndInsertSCMData(scm);
                            }
                            #endregion
                        }
                        else if (strMsg.Trim().StartsWith("CSN/", StringComparison.OrdinalIgnoreCase))
                        {
                            msgType = "CSN";
                            MessageData.CSNInfo csnInfo = new MessageData.CSNInfo("");
                            MessageData.customsextrainfo[] custominfo = new MessageData.customsextrainfo[0];
                            DataTable dtOCIInfo = new DataTable();
                            flag = cls_Encode_Decode.DecodeCSNMessage(strMsg, refNO, ref csnInfo, ref custominfo, ref dtOCIInfo);
                            if (flag == true)
                            {
                                //flag = ValidateAndSaveCSNMessage(refNO, ref csnInfo, ref custominfo, strMessageFrom, strFromID, dtOCIInfo);

                                (flag, csnInfo, custominfo) = await ValidateAndSaveCSNMessage(refNO, csnInfo, custominfo, strMessageFrom, strFromID, dtOCIInfo);

                            }

                        }
                        else if (strMsg.Trim().StartsWith("SSM", StringComparison.OrdinalIgnoreCase))
                        {
                            msgType = "SSM";
                            strMsg = strMsg.Trim().Replace("\r\n\r\n", "$");
                            strMsg = strMsg.Trim().Replace("\n\n", "$");
                            string[] SSMBodyLine = strMsg.Trim().Split('$');
                            for (int i = 0; i < SSMBodyLine.Length; i++)
                            {
                                if (SSMBodyLine[i].ToString().Trim() != string.Empty)
                                {
                                    SSM objSSM = new SSM();
                                    strMsg = SSMBodyLine[i].ToString().Replace("\r\n", "$");
                                    strMsg = strMsg.Replace("\n", "$");
                                    strMsg = strMsg.Replace("$$", "$");
                                    strMsg = strMsg.Trim('$');
                                    objSSM.ToSSM(strMsg, refNO, strOriginalMessage, strMessageFrom, out flag);
                                }
                            }
                        }
                        else if (strMsg.Trim().StartsWith("LDM", StringComparison.OrdinalIgnoreCase))
                        {
                            #region LDM
                            msgType = "LDM";
                            flag = false;
                            MessageData.LDMInfo ldm = new MessageData.LDMInfo("");
                            LDMMessageProcessor ldmProcessor = new LDMMessageProcessor();
                            flag = ldmProcessor.DecodeReceiveLDMMessage(strMsg, ref ldm, refNO);
                            if (flag == true)
                            {

                                flag = ldmProcessor.SaveandValidateLDMMessage(refNO, ref ldm);
                            }
                            #endregion
                        }
                        else
                        {
                            string[] bodyLine = strMsg.Split('$');
                            if (strMsg.Trim().StartsWith("ASM", StringComparison.OrdinalIgnoreCase))
                            {
                                msgType = "ASM";
                                strMsg = strMsg.Trim().Replace("\r\n\r\n", "$");
                                strMsg = strMsg.Trim().Replace("\n\n", "$");
                                string[] asmBodyLine = strMsg.Trim().Split('$');
                                for (int i = 0; i < asmBodyLine.Length; i++)
                                {
                                    if (asmBodyLine[i].ToString().Trim() != string.Empty)
                                    {
                                        //ASM objAsm = new ASM();
                                        strMsg = asmBodyLine[i].ToString().Replace("\r\n", "$");
                                        strMsg = strMsg.Replace("\n", "$");
                                        strMsg = strMsg.Replace("$$", "$");
                                        strMsg = strMsg.Trim('$');
                                        _aSm.ToASM(strMsg, refNO, strOriginalMessage, strMessageFrom, out flag);
                                    }
                                }
                            }
                            else if (bodyLine[0].ToUpper() == "MVT" || bodyLine[0].ToUpper() == "DIV")
                            {
                                //MVT objMvt = new MVT();
                                //objMvt.ToMVT(strMsg, refNO, strOriginalMessage, strMessageFrom, out msgType, out flag);

                                (msgType, flag) = await _mVT.ToMVT(strMsg, refNO, strOriginalMessage, strMessageFrom);
                            }
                            else
                            {
                                Errmsg = "Unsupported message format";
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                flag = false;
            }
            //return flag;
            return (flag, msgType, Errmsg);
        }

        //private void ReconstructFFRoute(MessageData.consignmnetinfo[] objConsInfo, ref MessageData.FltRoute[] objRouteInfo, int srno)
        private async Task<MessageData.FltRoute[]> ReconstructFFRoute(MessageData.consignmnetinfo[] objConsInfo, MessageData.FltRoute[] objRouteInfo, int srno)
        {
            try
            {
                string awbOrigin = string.Empty, awbDestination = string.Empty, airlinePrefix = string.Empty, fltMonth = string.Empty, fltDate = string.Empty;

                //SQLServer sqlServer = new SQLServer();

                DataSet? dsRouteDetails = new DataSet();
                MessageData.FltRoute[] objRouteInfo_Temp = [];
                MessageData.FltRoute flight_Temp = new("");
                DateTime flightDate = System.DateTime.Now;

                #region : Switch Flight Month :
                if (objRouteInfo.Length > 0)
                {
                    DateTime dtFlightDate = DateTime.Now;
                    try
                    {
                        switch (objRouteInfo[0].month.Trim().ToUpper())
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
                            default:
                                {
                                    fltMonth = "00";
                                    break;
                                }
                        }
                        fltDate = fltMonth.PadLeft(2, '0') + "/" + objRouteInfo[0].date.PadLeft(2, '0') + "/" + System.DateTime.Now.Year.ToString();

                        if (DateTime.TryParseExact(fltDate, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out dtFlightDate))
                        {
                            if (DateTime.Now.AddDays(-100) > dtFlightDate)
                            {   ///Advance year in flight date to next year.
                                dtFlightDate = dtFlightDate.AddYears(1);
                                fltDate = dtFlightDate.ToString("MM/dd/yyyy");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsLog.WriteLogAzure(ex);
                        fltDate = dtFlightDate.ToString("MM/dd/yyyy");
                    }
                }
                #endregion Switch Flight Month

                awbOrigin = objConsInfo[0].origin;
                awbDestination = objConsInfo[0].dest;
                airlinePrefix = objConsInfo[0].airlineprefix;

                SqlParameter[] sqlParameter = [
                      new SqlParameter("AWBOrigin", awbOrigin)
                    , new SqlParameter ("AWBDestination", awbDestination)
                    , new SqlParameter("SrNo", srno)
                    , new SqlParameter("AirlinePrefix", airlinePrefix)
                    , new SqlParameter("FlightDate", DateTime.Parse(fltDate))
                ];

                //dsRouteDetails = sqlServer.SelectRecords("Messaging.uspReconstructFFRRoute", sqlParameter);

                dsRouteDetails = await _readWriteDao.SelectRecords("Messaging.uspReconstructFFRRoute", sqlParameter);


                if (dsRouteDetails != null && dsRouteDetails.Tables.Count > 0 && dsRouteDetails.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsRouteDetails.Tables[0].Rows.Count; i++)
                    {
                        flight_Temp.carriercode = dsRouteDetails.Tables[0].Rows[i]["Carrier"].ToString();
                        flight_Temp.fltnum = dsRouteDetails.Tables[0].Rows[i]["FlightNum"].ToString();
                        flight_Temp.date = dsRouteDetails.Tables[0].Rows[i]["Day"].ToString();
                        flight_Temp.month = dsRouteDetails.Tables[0].Rows[i]["Month"].ToString();
                        flight_Temp.fltdept = dsRouteDetails.Tables[0].Rows[i]["Source"].ToString();
                        flight_Temp.fltarrival = dsRouteDetails.Tables[0].Rows[i]["Dest"].ToString();
                        flight_Temp.spaceallotmentcode = "NN";

                        Array.Resize(ref objRouteInfo_Temp, objRouteInfo_Temp.Length + 1);
                        objRouteInfo_Temp[objRouteInfo_Temp.Length - 1] = flight_Temp;
                    }
                }

                //if (objRouteInfo_Temp.Length > 0)
                //{
                //    objRouteInfo = objRouteInfo_Temp;
                //}

                // RETURN the new array (or original if empty)
                return objRouteInfo_Temp.Length > 0 ? objRouteInfo_Temp : objRouteInfo;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return objRouteInfo; // return original on error
            }
        }

        public void RelayMessages(string MessageName, String strMsg, string carrierCode, DateTime flightdate, bool FltLevel, string org = "", string dest = "", string fltnumber = "", string awbnumber = "")
        {

            //GenericFunction genericFunction = new GenericFunction();
            #region Relay Messages

            string MessageVersion = "8", SitaMessageHeader = string.Empty, error = string.Empty, strEmailid = string.Empty, strSITAHeaderType = string.Empty, SFTPHeaderSITAddress = string.Empty;

            DataSet dsconfiguration = _genericFunction.GetSitaAddressandMessageVersion(carrierCode, MessageName, "AIR", org, dest, fltnumber, string.Empty);

            if (dsconfiguration != null && dsconfiguration.Tables[0].Rows.Count > 0)
            {

                strEmailid = dsconfiguration.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                MessageVersion = dsconfiguration.Tables[0].Rows[0]["MessageVersion"].ToString();
                strSITAHeaderType = dsconfiguration.Tables[0].Rows[0]["SITAHeaderType"].ToString();
                if (dsconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 1)
                {
                    SitaMessageHeader = _genericFunction.MakeMailMessageFormat(dsconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), strSITAHeaderType);
                }
                if (dsconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                {
                    SFTPHeaderSITAddress = _genericFunction.MakeMailMessageFormat(dsconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), strSITAHeaderType);
                }
            }


            try
            {
                if (strMsg.Length > 3)
                {


                    if (SitaMessageHeader.Trim().Length > 0)
                        _genericFunction.SaveMessageOutBox("SITA" + MessageName, SitaMessageHeader.ToString() + "\r\n" + strMsg, "", "SITAFTP", org, dest, fltnumber, flightdate.ToString());

                    if (SFTPHeaderSITAddress.Trim().Length > 0)
                        _genericFunction.SaveMessageOutBox("SFTP" + MessageName, SFTPHeaderSITAddress.ToString() + "\r\n" + strMsg, "", "SFTP", org, dest, fltnumber, flightdate.ToString());

                    if (strEmailid.Trim().Length > 0)
                        _genericFunction.SaveMessageOutBox(MessageName, strMsg, "", strEmailid, org, dest, fltnumber, flightdate.ToString());

                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }


            //  }
            #endregion
            // }

            //end


        }




        public async Task<bool> elcodeFFRAndPrepareMsg(DataSet dsFFR, string fromEmailID, string toEmailID)
        {
            bool flag = false;
            try
            {

                MessageData.ffrinfo objFFRInfo = new MessageData.ffrinfo("");
                MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];

                if (dsFFR != null)
                {
                    if (dsFFR.Tables.Count > 0)
                    {
                        if (dsFFR.Tables[0].Rows.Count > 0)
                        {
                            DataRow drAWBRateMaster = dsFFR.Tables[0].Rows[0];
                            DataRow drAWBRouteMaster = dsFFR.Tables[1].Rows[0];
                            DataRow drAWBShipperConsigneeDetails = dsFFR.Tables[2].Rows[0];
                            DataRow drAWBSummaryMaster = dsFFR.Tables[3].Rows[0];


                            DateTime dtTo = new DateTime();
                            string dt = (drAWBRouteMaster["FltDate"].ToString());


                            dtTo = DateTime.ParseExact(drAWBRouteMaster["FltDate"].ToString(), "dd/MM/yyyy", null);
                            string day = dt.Substring(0, 2);
                            string mon = dtTo.ToString("MMM");
                            string yr = dt.Substring(6, 4);

                            #region PrepareFFRStructureObject

                            //line 1 
                            objFFRInfo.ffrversionnum = "6";

                            //line 4
                            objFFRInfo.noofuld = "";
                            //line 5 
                            objFFRInfo.specialservicereq1 = "";
                            objFFRInfo.specialservicereq2 = "";
                            //line 6
                            objFFRInfo.otherserviceinfo1 = "";
                            objFFRInfo.otherserviceinfo2 = "";
                            //line 7
                            objFFRInfo.bookingrefairport = drAWBSummaryMaster["OriginCode"].ToString();
                            objFFRInfo.officefundesignation = "FF";
                            objFFRInfo.companydesignator = "XX";
                            objFFRInfo.bookingfileref = "";
                            objFFRInfo.participentidetifier = "";
                            objFFRInfo.participentcode = "";
                            objFFRInfo.participentairportcity = "";

                            //line 9 
                            objFFRInfo.servicecode = "";
                            objFFRInfo.rateclasscode = "";
                            objFFRInfo.commoditycode = "";

                            //line 10                      
                            objFFRInfo.shipperaccnum = "";
                            objFFRInfo.shippername = drAWBShipperConsigneeDetails["ShipperName"].ToString();
                            objFFRInfo.shipperadd = drAWBShipperConsigneeDetails["ShipperAddress"].ToString();
                            objFFRInfo.shipperplace = "";//drAWBShipperConsigneeDetails["ShipperAddress"].ToString();
                            objFFRInfo.shipperstate = "";
                            objFFRInfo.shippercountrycode = drAWBShipperConsigneeDetails["ShipperCountry"].ToString().Substring(0, 2);
                            objFFRInfo.shipperpostcode = "";
                            objFFRInfo.shippercontactidentifier = "TE";
                            objFFRInfo.shippercontactnum = drAWBShipperConsigneeDetails["ShipperTelephone"].ToString();

                            //line 11
                            objFFRInfo.consaccnum = "";
                            objFFRInfo.consname = drAWBShipperConsigneeDetails["ConsigneeName"].ToString();
                            objFFRInfo.consadd = drAWBShipperConsigneeDetails["ConsigneeAddress"].ToString();
                            objFFRInfo.consplace = drAWBShipperConsigneeDetails["ConsigneeAddress"].ToString();
                            objFFRInfo.consstate = "";
                            objFFRInfo.conscountrycode = drAWBShipperConsigneeDetails["ConsigneeCountry"].ToString().Substring(0, 2);
                            objFFRInfo.conspostcode = "";
                            objFFRInfo.conscontactidentifier = "TE";
                            objFFRInfo.conscontactnum = drAWBShipperConsigneeDetails["ConsigneeTelephone"].ToString();

                            //line 12
                            objFFRInfo.custaccnum = "";
                            objFFRInfo.iatacargoagentcode = drAWBSummaryMaster["AgentCode"].ToString();
                            objFFRInfo.cargoagentcasscode = "";
                            objFFRInfo.custparticipentidentifier = "";
                            objFFRInfo.custname = "";
                            objFFRInfo.custplace = "";
                            //line 13
                            objFFRInfo.shiprefnum = "";
                            objFFRInfo.supplemetryshipperinfo1 = "";
                            objFFRInfo.supplemetryshipperinfo2 = "";


                            #endregion

                            string strMsg = cls_Encode_Decode.encodeFFRforsend(ref objFFRInfo, ref objULDInfo);
                            if (strMsg != null)
                            {
                                if (strMsg.Trim() != "")
                                {
                                    flag = await InsertFFRToOutBox(strMsg, fromEmailID, toEmailID);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            return flag;
        }

        public async Task<bool> EncodeFBLForSend(string POL, string FlightNo, string FlightDate, string FromEmailID, string ToEmailID)
        {
            MessageData.fblinfo objFBLInfo = new MessageData.fblinfo("");
            MessageData.unloadingport[] objUnloadingPort = new MessageData.unloadingport[0];
            MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
            int count1 = 0;
            int count2 = 0;
            bool flag = false;
            try
            {
                DataSet? dsData = new DataSet();

                dsData = await getFBLData(POL, FlightNo, FlightDate);

                if (dsData != null)
                {
                    if (dsData.Tables.Count > 1)
                    {
                        objFBLInfo.fblversion = "3";
                        objFBLInfo.messagesequencenum = "1";
                        objFBLInfo.carriercode = FlightNo.Substring(0, 2);
                        objFBLInfo.fltnum = FlightNo.Substring(2, FlightNo.Length - 2);
                        DateTime dtFlight = System.DateTime.Now;
                        objFBLInfo.date = System.DateTime.Now.Day.ToString().PadLeft(2, '0');
                        try
                        {
                            dtFlight = DateTime.ParseExact(FlightDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);//, "dd/MM/yyyy", null);
                            objFBLInfo.date = DateTime.ParseExact(FlightDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).Day.ToString().PadLeft(2, '0');
                        }
                        catch (Exception ex)
                        { clsLog.WriteLogAzure(ex); }
                        objFBLInfo.month = dtFlight.ToString("MMM").ToUpper();
                        objFBLInfo.fltairportcode = POL;
                        objFBLInfo.endmesgcode = "LAST";

                        //flight details
                        if (dsData.Tables[0].Rows.Count > 0)
                        {
                            count1 = 1;
                            foreach (DataRow dr in dsData.Tables[0].Rows)
                            {
                                MessageData.unloadingport objTempUnloadingPort = new MessageData.unloadingport("");
                                objTempUnloadingPort.unloadingairport = dr[2].ToString().ToUpper();
                                Array.Resize(ref objUnloadingPort, count1);
                                objUnloadingPort[count1 - 1] = objTempUnloadingPort;
                                count1++;
                            }
                        }
                        //awb details
                        if (dsData.Tables[1].Rows.Count > 0)
                        {
                            count2 = 1;
                            foreach (DataRow dr in dsData.Tables[1].Rows)
                            {
                                MessageData.consignmnetinfo objTempConsInfo = new MessageData.consignmnetinfo("");
                                string AWBNumber = dr[0].ToString().Trim();
                                objTempConsInfo.airlineprefix = dr["Prefix"].ToString().Trim();//Prefix
                                objTempConsInfo.awbnum = dr["AWBNumber"].ToString().Trim();
                                objTempConsInfo.origin = dr["OrginCode"].ToString().Trim().ToUpper();
                                objTempConsInfo.dest = dr["DestinationCode"].ToString().Trim().ToUpper();
                                objTempConsInfo.consigntype = "T";
                                objTempConsInfo.pcscnt = dr["AWBPcs"].ToString().Trim();
                                objTempConsInfo.weightcode = dr["UOM"].ToString().Trim();
                                objTempConsInfo.weight = dr["ChargedWeight"].ToString().Trim();
                                objTempConsInfo.manifestdesc = dr["CommDesc"].ToString().Trim().ToUpper();
                                objTempConsInfo.splhandling = dr["SHCCodes"].ToString().Trim().ToUpper();

                                Array.Resize(ref objConsInfo, count2);
                                objConsInfo[count2 - 1] = objTempConsInfo;
                                count2++;
                            }
                        }
                        if (count1 > 0 && count2 > 0)
                        {
                            MessageData.dimensionnfo[] objDimenInfo = new MessageData.dimensionnfo[0];
                            MessageData.consignmentorigininfo[] objConsOriginInfo = new MessageData.consignmentorigininfo[0];
                            MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
                            MessageData.otherserviceinfo objOtherInfo = new MessageData.otherserviceinfo("");
                            string FBLMsg = cls_Encode_Decode.EncodeFBLforsend(objFBLInfo, objUnloadingPort, objConsInfo, objDimenInfo, objConsOriginInfo, objULDInfo, objOtherInfo);
                            if (FBLMsg != null)
                            {
                                if (FBLMsg.Trim() != "")
                                {
                                    //bool flag = false;
                                    flag = await addMsgToOutBox("FBL", FBLMsg, FromEmailID, ToEmailID);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure("Exception in FBL Encode:" + ex.Message);
            }
            return flag;
        }

        public async Task<bool> EncodeSendingFBL(string POL, string FlightNo, string FlightDate, string FromEmailID, string ToEmailID)
        {
            MessageData.fblinfo objFBLInfo = new MessageData.fblinfo("");
            MessageData.unloadingport[] objUnloadingPort = new MessageData.unloadingport[0];
            MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
            int count1 = 0;
            int count2 = 0;
            bool flag = false;
            try
            {
                DataSet? dsData = new DataSet();

                dsData = await getFBLData(POL, FlightNo, DateTime.Parse(FlightDate).ToString("MM/dd/yyyy"));

                if (dsData != null)
                {
                    if (dsData.Tables.Count > 1)
                    {
                        objFBLInfo.fblversion = "3";
                        objFBLInfo.messagesequencenum = "1";
                        objFBLInfo.carriercode = FlightNo.Substring(0, 2);
                        objFBLInfo.fltnum = FlightNo.Substring(2, FlightNo.Length - 2);
                        DateTime dtFlight = DateTime.Parse(FlightDate);//, "dd/MM/yyyy", null);
                        objFBLInfo.date = DateTime.Parse(FlightDate).Day.ToString().PadLeft(2, '0');
                        objFBLInfo.month = dtFlight.ToString("MMM");
                        objFBLInfo.fltairportcode = POL;
                        objFBLInfo.endmesgcode = "LAST";

                        //flight details
                        if (dsData.Tables[0].Rows.Count > 0)
                        {
                            count1 = 1;
                            foreach (DataRow dr in dsData.Tables[0].Rows)
                            {
                                MessageData.unloadingport objTempUnloadingPort = new MessageData.unloadingport("");
                                objTempUnloadingPort.unloadingairport = dr[2].ToString();
                                Array.Resize(ref objUnloadingPort, count1);
                                objUnloadingPort[count1 - 1] = objTempUnloadingPort;
                                count1++;
                            }
                        }
                        //awb details
                        if (dsData.Tables[1].Rows.Count > 0)
                        {
                            count2 = 1;
                            foreach (DataRow dr in dsData.Tables[1].Rows)
                            {
                                MessageData.consignmnetinfo objTempConsInfo = new MessageData.consignmnetinfo("");
                                string AWBNumber = dr[0].ToString().Trim();
                                objTempConsInfo.airlineprefix = AWBNumber.Substring(0, 2);
                                objTempConsInfo.awbnum = AWBNumber.Substring(2, AWBNumber.Length - 2);
                                objTempConsInfo.origin = dr[11].ToString().Trim();
                                objTempConsInfo.dest = dr[5].ToString().Trim();
                                objTempConsInfo.consigntype = "T";
                                objTempConsInfo.pcscnt = dr[3].ToString().Trim();
                                objTempConsInfo.weightcode = dr["UOM"].ToString().Trim();
                                objTempConsInfo.weight = dr[4].ToString().Trim();
                                objTempConsInfo.manifestdesc = dr[8].ToString().Trim() + "-" + dr[6].ToString().Trim();
                                objTempConsInfo.manifestdesc = dr["SHCCodes"].ToString().Trim();
                                Array.Resize(ref objConsInfo, count2);
                                objConsInfo[count2 - 1] = objTempConsInfo;
                                count2++;
                            }
                        }
                        if (count1 > 0 && count2 > 0)
                        {
                            MessageData.dimensionnfo[] objDimenInfo = new MessageData.dimensionnfo[0];
                            MessageData.consignmentorigininfo[] objConsOriginInfo = new MessageData.consignmentorigininfo[0];
                            MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
                            MessageData.otherserviceinfo objOtherInfo = new MessageData.otherserviceinfo("");
                            string FBLMsg = cls_Encode_Decode.EncodeFBLforsend(objFBLInfo, objUnloadingPort, objConsInfo, objDimenInfo, objConsOriginInfo, objULDInfo, objOtherInfo);
                            if (FBLMsg != null)
                            {
                                if (FBLMsg.Trim() != "")
                                {
                                    //bool flag = false;
                                    flag = await addMsgToOutBox("FBL", FBLMsg, FromEmailID, ToEmailID);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            return flag;
        }

        public async Task<bool> EncodeFFMForSend(string DepartureAirport, string FlightNo, string FlightDate, string FromEmailID, string ToEmailID)
        {
            bool flag = false;
            try
            {
                MessageData.ffminfo objFFMInfo = new MessageData.ffminfo("");
                MessageData.unloadingport[] objUnloadingPort = new MessageData.unloadingport[0];
                MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
                int count1 = 0;
                int count2 = 0;
                DataSet? dsData = new DataSet();
                DataSet? ds = new DataSet();
                ds = await getFFMUnloadingPort(DepartureAirport, FlightNo, DateTime.Parse(FlightDate).ToString("MM/dd/yyyy"));
                dsData = await getFFMData(DepartureAirport, FlightNo, DateTime.Parse(FlightDate).ToString("MM/dd/yyyy"));
                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            count1 = 1;
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                MessageData.unloadingport objTempUnloadingPort = new MessageData.unloadingport("");
                                objTempUnloadingPort.unloadingairport = dr[0].ToString();
                                Array.Resize(ref objUnloadingPort, count1);
                                objUnloadingPort[count1 - 1] = objTempUnloadingPort;
                                count1++;
                            }
                        }
                    }
                }
                if (dsData != null)
                {
                    if (dsData.Tables.Count > 0)
                    {
                        objFFMInfo.ffmversionnum = "8";
                        objFFMInfo.messagesequencenum = "1";
                        objFFMInfo.carriercode = FlightNo.Substring(0, 2);
                        objFFMInfo.fltnum = FlightNo.Substring(2, FlightNo.Length - 2);
                        DateTime dtFlight = DateTime.Parse(FlightDate);
                        objFFMInfo.fltdate = dtFlight.ToString("dd");
                        objFFMInfo.month = dtFlight.ToString("MMM");
                        objFFMInfo.fltairportcode = DepartureAirport;
                        objFFMInfo.endmesgcode = "LAST";

                        //flight details

                        //awb details
                        if (dsData.Tables[1].Rows.Count > 0)
                        {
                            count2 = 1;
                            foreach (DataRow dr in dsData.Tables[1].Rows)
                            {
                                MessageData.consignmnetinfo objTempConsInfo = new MessageData.consignmnetinfo("");
                                string AWBNumber = dr[5].ToString().Trim();
                                objTempConsInfo.airlineprefix = AWBNumber.Substring(0, 2);
                                objTempConsInfo.awbnum = AWBNumber.Substring(2, AWBNumber.Length - 2);
                                objTempConsInfo.origin = dr[13].ToString().Trim();
                                objTempConsInfo.dest = dr[3].ToString().Trim();
                                objTempConsInfo.consigntype = "T";
                                objTempConsInfo.pcscnt = dr[7].ToString().Trim();
                                objTempConsInfo.weightcode = dr["UOM"].ToString().Trim();
                                objTempConsInfo.weight = dr[8].ToString().Trim();
                                objTempConsInfo.manifestdesc = dr[6].ToString().Trim() + "-" + dr[12].ToString().Trim();

                                for (int k = 0; k < objUnloadingPort.Length; k++)
                                {
                                    if (objUnloadingPort[k].unloadingairport.Equals(dr[3].ToString().Trim(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        objTempConsInfo.portsequence = (k + 1).ToString();
                                    }
                                }
                                Array.Resize(ref objConsInfo, count2);
                                objConsInfo[count2 - 1] = objTempConsInfo;
                                count2++;
                            }
                        }
                        if (count1 > 0 && count2 > 0)
                        {
                            MessageData.dimensionnfo[] objDimenInfo = new MessageData.dimensionnfo[0];
                            MessageData.consignmentorigininfo[] objConsOriginInfo = new MessageData.consignmentorigininfo[0];
                            MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
                            MessageData.otherserviceinfo[] objOtherInfo = new MessageData.otherserviceinfo[0];
                            MessageData.movementinfo[] objMovemInfo = new MessageData.movementinfo[0];
                            MessageData.customsextrainfo[] objcustInfo = new MessageData.customsextrainfo[0];
                            string FFMMsg = cls_Encode_Decode.EncodeFFMforsend(ref objFFMInfo, ref objUnloadingPort, ref objConsInfo, ref objDimenInfo,
                                                                               ref objMovemInfo, ref objOtherInfo, ref objcustInfo, ref objULDInfo);
                            if (FFMMsg != null)
                            {
                                if (FFMMsg.Trim() != "")
                                {
                                    flag = await addMsgToOutBox("FFM", FFMMsg, FromEmailID, ToEmailID);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            return flag;
        }

        public async Task<string> EncodeFFM(string DepartureAirport, string FlightNo, DateTime FlightDate, string MsgVer)
        {
            string FFMMsg = string.Empty;
            try
            {
                MessageData.ffminfo objFFMInfo = new MessageData.ffminfo("");
                MessageData.unloadingport[] objUnloadingPort = new MessageData.unloadingport[0];
                MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
                MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
                string ffmCountMessage = string.Empty, ffmLastMessage = string.Empty;
                int count1 = 0;
                int count2 = 0;
                DataSet dsData? = new DataSet();
                DataSet? ds = new DataSet();
                ds = await GetFlightInformationforFFM(DepartureAirport, FlightNo, FlightDate);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    count1 = 1;
                    if (MsgVer.Length > 0 && MsgVer != "")
                        objFFMInfo.ffmversionnum = MsgVer.Trim();
                    else
                        objFFMInfo.ffmversionnum = "3";

                    objFFMInfo.messagesequencenum = "1";
                    objFFMInfo.carriercode = ds.Tables[0].Rows[0]["CarrierCode"].ToString();
                    objFFMInfo.fltnum = ds.Tables[0].Rows[0]["Flightno"].ToString();
                    objFFMInfo.fltdate = ds.Tables[0].Rows[0]["FlighDeptDay"].ToString(); ///dtFlight.ToString("dd");//DateTime.Parse(FlightDate).Day.ToString().PadLeft(2, '0');
                    objFFMInfo.month = ds.Tables[0].Rows[0]["FlightMonth"].ToString();  // dtFlight.ToString("MMM").ToUpper();
                    if (MsgVer == "8")
                    {
                        objFFMInfo.time = ds.Tables[0].Rows[0]["FlightDepartureTime"].ToString();
                        objFFMInfo.countrycode = ds.Tables[0].Rows[0]["ArrivalCountryCode"].ToString();
                        // objFFMInfo.countrycode = ds.Tables[0].Rows[0]["ArrivalCountryCode"].ToString();

                        objFFMInfo.fltdate1 = ds.Tables[0].Rows[0]["FlightArrivalDay"].ToString();
                        objFFMInfo.fltmonth1 = ds.Tables[0].Rows[0]["FlightArrivalMonth"].ToString();
                        objFFMInfo.flttime1 = ds.Tables[0].Rows[0]["FlightArrivalTime"].ToString();
                        objFFMInfo.fltairportcode1 = ds.Tables[0].Rows[0]["DepartureStation"].ToString();


                    }
                    if (ds.Tables[0].Rows[0]["AirCraftRegistration"].ToString() != "")
                        objFFMInfo.aircraftregistration = ds.Tables[0].Rows[0]["AirCraftRegistration"].ToString();

                    objFFMInfo.fltairportcode = DepartureAirport;

                    count2 = 1;
                    //flight details

                    foreach (DataRow drdestination in ds.Tables[0].Rows)
                    {
                        MessageData.unloadingport objTempUnloadingPort = new MessageData.unloadingport("");
                        dsData = await GetRecordforGenerateFFM(DepartureAirport, FlightNo, FlightDate, drdestination["DepartureStation"].ToString());
                        //if (dsData != null && dsData.Tables.Count > 2 && dsData.Tables[3].Rows.Count >= 22)
                        //{
                        //    string Message = ManualGenerateFFM(dsData, DepartureAirport, FlightNo, FlightDate, drdestination["DepartureStation"].ToString(), objFFMInfo.ffmversionnum);
                        //    if (Message != "")
                        //        return Message;
                        //}

                        if (dsData != null && dsData.Tables.Count > 0 && dsData.Tables[3].Rows.Count <= 0)
                        {
                            // objUnloadingPort[0].nilcargocode = "NIL";
                            //  objTempUnloadingPort.nilcargocode = "NIL";
                            //if (ds.Tables[0].Rows.Count == 1)   //Remove this conditional structure or edit its code blocks so that they're not all the same.
                            //{
                            objTempUnloadingPort.nilcargocode = "NIL";
                            objTempUnloadingPort.unloadingairport = drdestination[0].ToString().ToUpper();
                            Array.Resize(ref objUnloadingPort, count1);
                            objUnloadingPort[count1 - 1] = objTempUnloadingPort;
                            //}
                            //else     // same code in IF-Else
                            //{
                            //    objTempUnloadingPort.nilcargocode = "NIL";
                            //    objTempUnloadingPort.unloadingairport = drdestination[0].ToString().ToUpper();
                            //    Array.Resize(ref objUnloadingPort, count1);
                            //    objUnloadingPort[count1 - 1] = objTempUnloadingPort;
                            //}

                            objTempUnloadingPort.unloadingairport = drdestination["DepartureStation"].ToString();
                            if (MsgVer == "8")
                            {
                                objTempUnloadingPort.day = ds.Tables[0].Rows[0]["FlightArrivalDay"].ToString();
                                objTempUnloadingPort.month = ds.Tables[0].Rows[0]["FlightArrivalMonth"].ToString();
                                objTempUnloadingPort.time = ds.Tables[0].Rows[0]["FlightArrivalTime"].ToString();
                            }
                            Array.Resize(ref objUnloadingPort, count1);
                            objUnloadingPort[count1 - 1] = objTempUnloadingPort;
                            count1++;
                        }
                        else
                        {
                            if (MsgVer == "8")
                            {
                                if (objTempUnloadingPort.day != "" && objTempUnloadingPort.month != "" && objTempUnloadingPort.time != "")
                                {
                                    objTempUnloadingPort.day = ds.Tables[0].Rows[0]["FlightArrivalDay"].ToString();
                                    objTempUnloadingPort.month = ds.Tables[0].Rows[0]["FlightArrivalMonth"].ToString();
                                    objTempUnloadingPort.time = ds.Tables[0].Rows[0]["FlightArrivalTime"].ToString();
                                }
                            }
                            objTempUnloadingPort.unloadingairport = drdestination["DepartureStation"].ToString();
                            Array.Resize(ref objUnloadingPort, count1);
                            objUnloadingPort[count1 - 1] = objTempUnloadingPort;
                            count1++;

                            try
                            {
                                if (dsData != null && dsData.Tables.Count > 2)
                                {
                                    if (dsData.Tables[2].Rows.Count > 0)
                                    {
                                        Array.Resize(ref objULDInfo, dsData.Tables[2].Rows.Count);
                                        for (int k = 0; k < dsData.Tables[2].Rows.Count; k++)
                                        {
                                            MessageData.ULDinfo objTempULDInfo = new MessageData.ULDinfo("");
                                            objTempULDInfo.uldsrno = dsData.Tables[2].Rows[k]["ULDNo"].ToString();
                                            objTempULDInfo.refuld = k.ToString();
                                            for (int cnt = 0; cnt < objUnloadingPort.Length; cnt++)
                                            {
                                                if (objUnloadingPort[cnt].unloadingairport.Equals(dsData.Tables[2].Rows[k]["POU"].ToString().Trim(), StringComparison.OrdinalIgnoreCase))
                                                {
                                                    objTempULDInfo.portsequence = (cnt + 1).ToString();
                                                }
                                            }
                                            objULDInfo[k] = objTempULDInfo;

                                        }
                                    }
                                }
                            }

                            catch (Exception ex) { clsLog.WriteLogAzure(ex); }

                            if (dsData != null && dsData.Tables.Count > 0)
                            {
                                if (dsData.Tables[3].Rows.Count > 0)
                                {
                                    for (int row = 0; row < dsData.Tables[3].Rows.Count; row++)
                                    {
                                        MessageData.consignmnetinfo objTempConsInfo = new MessageData.consignmnetinfo("");
                                        string AWBNumber = dsData.Tables[3].Rows[row]["AWBNumber"].ToString().Trim().Replace("-", "");
                                        if (AWBNumber.Length > 0)
                                        {

                                            objTempConsInfo.airlineprefix = AWBNumber.Substring(0, 3);
                                            objTempConsInfo.awbnum = AWBNumber.Substring(3, AWBNumber.Length - 3);
                                            objTempConsInfo.origin = dsData.Tables[3].Rows[row]["Org"].ToString().Trim();
                                            objTempConsInfo.dest = dsData.Tables[3].Rows[row]["AWBDest"].ToString().Trim();


                                            objTempConsInfo.pcscnt = dsData.Tables[3].Rows[row]["PCS"].ToString().Trim();
                                            objTempConsInfo.weightcode = dsData.Tables[3].Rows[row]["UOM"].ToString();
                                            objTempConsInfo.weight = dsData.Tables[3].Rows[row]["GrossWgt"].ToString().Trim();
                                            objTempConsInfo.volumecode = dsData.Tables[3].Rows[row]["VolumeCode"].ToString().ToUpper().Trim();
                                            objTempConsInfo.volumeamt = dsData.Tables[3].Rows[row]["Vol"].ToString();
                                            objTempConsInfo.splhandling = dsData.Tables[3].Rows[row]["SHCCodes"].ToString();
                                            if (dsData.Tables[3].Rows[row]["consigntype"].ToString() != "T")
                                            {

                                                string strManifestPCs = dsData.Tables[3].Compute("Sum(PCS)", "AWBNumber ='" + dsData.Tables[3].Rows[row]["AWBNumber"].ToString() + "'").ToString();
                                                int numberOfRecords = dsData.Tables[3].Select("AWBNumber ='" + dsData.Tables[3].Rows[row]["AWBNumber"].ToString() + "'").Length;
                                                if (int.Parse(strManifestPCs) < int.Parse(dsData.Tables[3].Rows[row]["AWBPcs"].ToString()) && numberOfRecords == 1)
                                                    objTempConsInfo.consigntype = "P";
                                                else if ((int.Parse(strManifestPCs)) < int.Parse(dsData.Tables[3].Rows[row]["AWBPcs"].ToString()) && numberOfRecords > 1)
                                                    objTempConsInfo.consigntype = "D";
                                                else if ((int.Parse(strManifestPCs)) == int.Parse(dsData.Tables[3].Rows[row]["AWBPcs"].ToString()) && numberOfRecords > 1)
                                                    objTempConsInfo.consigntype = "S";

                                                objTempConsInfo.shpdesccode = "T";
                                                objTempConsInfo.numshp = dsData.Tables[3].Rows[row]["AWBPcs"].ToString().Trim();

                                            }
                                            else
                                            {
                                                objTempConsInfo.consigntype = "T";
                                            }
                                            try
                                            {
                                                int length = 14;
                                                if (dsData.Tables[3].Rows[row][12].ToString().Length < 14)
                                                {
                                                    length = dsData.Tables[3].Rows[row]["Desc"].ToString().Length;
                                                }
                                                objTempConsInfo.manifestdesc = dsData.Tables[3].Rows[row]["Desc"].ToString().Substring(0, length); //dr[6].ToString().Trim() + "-" + dr[12].ToString().Trim();
                                            }
                                            catch (Exception ex)
                                            {
                                                clsLog.WriteLogAzure(ex);
                                            }
                                            try
                                            {
                                                for (int k = 0; k < objULDInfo.Length; k++)
                                                {
                                                    if (objULDInfo[k].uldsrno.Equals(dsData.Tables[3].Rows[row]["ULDNo"].ToString().Trim(), StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        objTempConsInfo.uldsequence = objULDInfo[k].refuld.ToString();
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                clsLog.WriteLogAzure(ex);
                                                objTempConsInfo.uldsequence = "";
                                            }
                                            for (int k = 0; k < objUnloadingPort.Length; k++)
                                            {
                                                if (objUnloadingPort[k].nilcargocode == "")
                                                {

                                                    if (objUnloadingPort[k].unloadingairport.Equals(dsData.Tables[3].Rows[row]["POU"].ToString().Trim(), StringComparison.OrdinalIgnoreCase) || objUnloadingPort[k].unloadingairport.Equals(dsData.Tables[3].Rows[row]["AWBDest"].ToString().Trim(), StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        objTempConsInfo.portsequence = (k + 1).ToString();
                                                    }
                                                }
                                            }
                                            Array.Resize(ref objConsInfo, count2);
                                            objConsInfo[count2 - 1] = objTempConsInfo;
                                            count2++;
                                        }

                                    }
                                }
                            }
                        }
                    }

                    objFFMInfo.endmesgcode = "LAST";
                    MessageData.dimensionnfo[] objDimenInfo = new MessageData.dimensionnfo[0];
                    MessageData.consignmentorigininfo[] objConsOriginInfo = new MessageData.consignmentorigininfo[0];
                    MessageData.otherserviceinfo[] objOtherInfo = new MessageData.otherserviceinfo[0];
                    MessageData.movementinfo[] objMovemInfo = new MessageData.movementinfo[0];
                    MessageData.customsextrainfo[] objcustInfo = new MessageData.customsextrainfo[0];

                    FFMMsg = cls_Encode_Decode.EncodeFFMforsend(ref objFFMInfo, ref objUnloadingPort, ref objConsInfo, ref objDimenInfo,
                                                                      ref objMovemInfo, ref objOtherInfo, ref objcustInfo, ref objULDInfo);
                    // }

                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }

            return FFMMsg;
        }

        public async Task<DataSet?> GetFlightInformationforFFM(string FlightOrigin, string FlightNo, DateTime FlightDate)
        {
            DataSet? ds = new DataSet();
            try
            {
                //SQLServer dtb = new SQLServer();
                //string[] pname = new string[3]
                //{
                //    "FlightNo",
                //    "FlightDate",
                //    "FlightOrigin"
                //};
                //object[] pvalue = new object[3]
                //{
                //    FlightNo,
                //    FlightDate,
                //    FlightOrigin
                // };
                //SqlDbType[] ptype = new SqlDbType[3]
                //{
                //    SqlDbType.VarChar,
                //    SqlDbType.DateTime,
                //    SqlDbType.VarChar
                //};
                //ds = dtb.SelectRecords("GetFlightInformationforFFM", pname, pvalue, ptype);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = FlightDate },
                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = FlightOrigin }
                };
                ds = await _readWriteDao.SelectRecords("GetFlightInformationforFFM", parameters);

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            return ds;
        }

        public async Task<DataSet?> GetRecordforGenerateFFM(string DepartureAirport, string FlightNo, DateTime FlightDate, string FlightDestination)
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
                string procedure = "GetFlightRecordforFFM";

                SqlParameter[] parameters =
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
                clsLog.WriteLogAzure(ex);
            }
            return dsData;
        }

        public async Task<bool> EncodeFSAForSend(string AirlinePrefix, string AWBNo, string CarrierCode, string FlightNo, string OperType, string FromEmailID, string ToEmailID, string AWBPrefix)
        {
            bool flag = false;
            try
            {
                DataSet? dsAWB = await GetAWBDetailsForFSA(AWBNo, OperType, AWBPrefix);
                DataSet? dsFlight = await getFlightDetailsForFSA(AWBNo, CarrierCode.Trim() + FlightNo.Trim(), AWBPrefix);
                if (dsAWB != null)
                {
                    if (dsAWB.Tables.Count > 0)
                    {
                        if (dsAWB.Tables[0].Rows.Count > 0)
                        {
                            DataRow drAWB = dsAWB.Tables[0].Rows[0];
                            DataRow drFlight = dsFlight.Tables[0].Rows[0];
                            MessageData.FSAInfo objFSA = new MessageData.FSAInfo("");
                            MessageData.CommonStruct[] objFSANode = new MessageData.CommonStruct[0];
                            MessageData.CommonStruct objComnStruct = new MessageData.CommonStruct("");
                            MessageData.customsextrainfo[] objCustomInfo = new MessageData.customsextrainfo[0];
                            MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
                            MessageData.otherserviceinfo[] objOtherSercInfo = new MessageData.otherserviceinfo[0];

                            #region PrepareStructure

                            try
                            {
                                objFSA.airlineprefix = AirlinePrefix;
                                objFSA.fsaversion = "14";
                                objFSA.awbnum = AWBNo;
                                objFSA.origin = drAWB["OriginCode"].ToString().ToUpper();
                                objFSA.dest = drAWB["DestinationCode"].ToString().ToUpper();
                                objFSA.pcscnt = drAWB["PiecesCount"].ToString();
                                objFSA.consigntype = "T";
                                objFSA.weight = drAWB["GrossWeight"].ToString();
                                objFSA.weightcode = drAWB["UOM"].ToString();

                                DateTime FlightDate = DateTime.ParseExact(drFlight["FltDate"].ToString(), "dd/MM/yyyy", null);

                                #region SwitchCase
                                switch (OperType)
                                {
                                    case "RCT":
                                    case "DIS":
                                    case "FOH":
                                    case "RCS":
                                        {
                                            #region RCS
                                            objComnStruct.messageprefix = OperType.Trim();
                                            objComnStruct.carriercode = CarrierCode.Trim();
                                            objComnStruct.fltday = FlightDate.ToString("dd");
                                            objComnStruct.fltmonth = FlightDate.ToString("MMM");
                                            objComnStruct.flttime = "";
                                            objComnStruct.airportcode = drFlight["FltOrigin"].ToString();
                                            objComnStruct.pcsindicator = "T";
                                            objComnStruct.numofpcs = drAWB["PiecesCount"].ToString();
                                            objComnStruct.weightcode = drAWB["UOM"].ToString();
                                            objComnStruct.weight = drAWB["GrossWeight"].ToString();
                                            objComnStruct.name = "";
                                            #endregion
                                        }
                                        break;
                                    case "NFD":
                                    case "AWD":
                                    case "CCD":
                                    case "DLV":
                                    case "DDL":
                                    case "TGC":
                                    case "TFD":
                                        {
                                            #region NFD
                                            objComnStruct.messageprefix = OperType.Trim();
                                            objComnStruct.carriercode = CarrierCode.Trim();
                                            objComnStruct.fltday = FlightDate.ToString("dd");
                                            objComnStruct.fltmonth = FlightDate.ToString("MMM");
                                            objComnStruct.flttime = "";
                                            objComnStruct.airportcode = drFlight["FltOrigin"].ToString();
                                            objComnStruct.pcsindicator = "T";
                                            objComnStruct.numofpcs = drAWB["PiecesCount"].ToString();
                                            objComnStruct.weightcode = drAWB["UOM"].ToString();
                                            objComnStruct.weight = drAWB["GrossWeight"].ToString();
                                            objComnStruct.name = "";
                                            objComnStruct.transfermanifestnumber = "";
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
                                            objComnStruct.messageprefix = OperType.Trim();
                                            objComnStruct.carriercode = CarrierCode.Trim();
                                            objComnStruct.flightnum = FlightNo.Trim();
                                            objComnStruct.fltday = FlightDate.ToString("dd");
                                            objComnStruct.fltmonth = FlightDate.ToString("MMM");
                                            objComnStruct.airportcode = drFlight["FltOrigin"].ToString();
                                            objComnStruct.pcsindicator = "T";
                                            objComnStruct.numofpcs = drAWB["PiecesCount"].ToString();
                                            objComnStruct.weightcode = drAWB["UOM"].ToString();
                                            objComnStruct.weight = drAWB["GrossWeight"].ToString();
                                            objComnStruct.daychangeindicator = "";
                                            objComnStruct.timeindicator = "S,S";
                                            objComnStruct.depttime = drFlight["SchDeptTime"].ToString().Replace(":", "");
                                            objComnStruct.arrivaltime = drFlight["SchArrTime"].ToString().Replace(":", "");
                                            #endregion
                                        }
                                        break;
                                    case "BKD":
                                        {
                                            #region BKD
                                            objComnStruct.messageprefix = OperType.Trim();
                                            objComnStruct.carriercode = CarrierCode.Trim();
                                            objComnStruct.flightnum = FlightNo.Trim();
                                            objComnStruct.fltday = FlightDate.ToString("dd");
                                            objComnStruct.fltmonth = FlightDate.ToString("MMM");
                                            objComnStruct.fltorg = drFlight["FltOrigin"].ToString();
                                            objComnStruct.fltdest = drFlight["FltDestination"].ToString();
                                            objComnStruct.pcsindicator = "T";
                                            objComnStruct.numofpcs = drAWB["PiecesCount"].ToString();
                                            objComnStruct.weightcode = drAWB["UOM"].ToString();
                                            objComnStruct.weight = drAWB["GrossWeight"].ToString();
                                            objComnStruct.daychangeindicator = "";
                                            objComnStruct.depttime = drFlight["SchDeptTime"].ToString().Replace(":", "");
                                            objComnStruct.timeindicator = "S,S";
                                            objComnStruct.arrivaltime = drFlight["SchArrTime"].ToString().Replace(":", "");
                                            objComnStruct.volumecode = "";
                                            objComnStruct.volumeamt = "";
                                            objComnStruct.densityindicator = "";
                                            objComnStruct.densitygroup = "";
                                            #endregion

                                        }
                                        break;
                                    case "TRM":
                                        {
                                            #region TRM
                                            objComnStruct.messageprefix = OperType.Trim();
                                            objComnStruct.carriercode = CarrierCode.Trim();
                                            objComnStruct.fltorg = drFlight["FltOrigin"].ToString();
                                            objComnStruct.fltdest = drFlight["FltDestination"].ToString();
                                            objComnStruct.pcsindicator = "T";
                                            objComnStruct.numofpcs = drAWB["PiecesCount"].ToString();
                                            objComnStruct.weightcode = drAWB["UOM"].ToString();
                                            objComnStruct.weight = drAWB["GrossWeight"].ToString();
                                            #endregion
                                        }
                                        break;


                                    case "CRC":
                                        {
                                            #region CRC
                                            objComnStruct.messageprefix = OperType.Trim();
                                            objComnStruct.fltday = FlightDate.ToString("dd");
                                            objComnStruct.fltmonth = FlightDate.ToString("MMM");
                                            objComnStruct.airportcode = drFlight["FltOrigin"].ToString();
                                            objComnStruct.pcsindicator = "T";
                                            objComnStruct.numofpcs = drAWB["PiecesCount"].ToString();
                                            objComnStruct.weightcode = drAWB["UOM"].ToString();
                                            objComnStruct.weight = drAWB["GrossWeight"].ToString();
                                            objComnStruct.carriercode = CarrierCode.Trim();
                                            objComnStruct.flightnum = FlightNo.Trim();
                                            objComnStruct.fltorg = drFlight["FltOrigin"].ToString();
                                            objComnStruct.fltdest = drFlight["FltDestination"].ToString();
                                            #endregion
                                        }
                                        break;

                                }
                                #endregion


                            }
                            catch (Exception ex)
                            { clsLog.WriteLogAzure(ex); }

                            Array.Resize(ref objFSANode, objFSANode.Length + 1);
                            objFSANode[objFSANode.Length - 1] = objComnStruct;
                            #endregion

                            string msgFSA = cls_Encode_Decode.EncodeFSAforSend(ref objFSA, ref objFSANode, ref objCustomInfo, ref objULDInfo, ref objOtherSercInfo);

                            flag = await addMsgToOutBox("FSA", msgFSA, FromEmailID, ToEmailID);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            return flag;
        }

        public async Task<DataSet?> getFBLData(string POL, string FlightNo, string FlightDate)
        {
            //bool flag = false;
            DataSet? dsData = new DataSet();
            try
            {
                //SQLServer dtb = new SQLServer();

                //string[] paramname = new string[] {"FlightNo",
                //                                "FltDate" };

                //object[] paramvalue = new object[] {FlightNo,
                //                                 FlightDate};

                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar,
                //                                      SqlDbType.VarChar };

                //dsData = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);

                string procedure = "spGetFBLDataForSend";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@FltDate", SqlDbType.VarChar) { Value = FlightDate }
                };

                // Async version
                dsData = await _readWriteDao.SelectRecords(procedure, parameters);
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            return dsData;
        }

        public async Task<DataSet?> getFBLDataORG(string POL, string FlightNo, string FlightDate)
        {
            //bool flag = false;
            DataSet? dsData = new DataSet();
            try
            {
                //SQLServer dtb = new SQLServer();

                //string[] paramname = new string[] { "POL",
                //                                "FlightNo",
                //                                "FltDate" };

                //object[] paramvalue = new object[] { POL,
                //                                 FlightNo,
                //                                 newFlightDate.Month.ToString().PadLeft(2,'0')+"/"+ newFlightDate.Day.ToString().PadLeft(2,'0')+"/"+newFlightDate.Year.ToString()};

                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar,
                //                                      SqlDbType.VarChar,
                //                                      SqlDbType.DateTime };

                //dsData = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);

                DateTime newFlightDate = DateTime.Parse(FlightDate);

                string procedure = "spGetFBLDataForSend";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@POL", SqlDbType.VarChar) { Value = POL },
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@FltDate", SqlDbType.DateTime)
                    {
                        Value =newFlightDate.Month.ToString().PadLeft(2,'0')+"/"+ newFlightDate.Day.ToString().PadLeft(2,'0')+"/"+newFlightDate.Year.ToString()
                    }
                };

                dsData = await _readWriteDao.SelectRecords(procedure, parameters);
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            return dsData;
        }

        public async Task<DataSet?> getFFMUnloadingPort(string DepartureAirport, string FlightNo, string FlightDate)
        {
            DataSet? ds = new DataSet();
            try
            {
                //    SQLServer dtb = new SQLServer();
                //    string[] pname = new string[3]
                //{
                //    "FlightID",
                //    "Source",
                //    "FlightDate"
                //};
                //    object[] pvalue = new object[3]
                //{
                //    FlightNo,
                //    DepartureAirport,
                //    newFlightDate.Month.ToString().PadLeft(2,'0')+"/"+ newFlightDate.Day.ToString().PadLeft(2,'0')+"/"+newFlightDate.Year.ToString()

                //};
                //    SqlDbType[] ptype = new SqlDbType[3]
                //{
                //    SqlDbType.VarChar,
                //    SqlDbType.VarChar,
                //    SqlDbType.DateTime
                //};
                //    ds = dtb.SelectRecords("spExpManiGetAirlineSch1", pname, pvalue, ptype);

                DateTime newFlightDate = DateTime.Parse(FlightDate);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@FlightID", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@Source", SqlDbType.VarChar) { Value = DepartureAirport },
                    new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = newFlightDate.Month.ToString().PadLeft(2,'0')+"/"+ newFlightDate.Day.ToString().PadLeft(2,'0')+"/"+newFlightDate.Year.ToString() }
                };
                ds = await _readWriteDao.SelectRecords("spExpManiGetAirlineSch1", parameters);

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            return ds;
        }

        public async Task<DataSet> getUpdatedFFMData(string DepartureAirport, string FlightNo, string FlightDate)
        {
            DataSet? ds = new DataSet();
            DataSet? dsData = new DataSet();
            try
            {
                //    SQLServer dtb = new SQLServer();
                //    string[] pname = new string[3]
                //{
                //    "FlightID",
                //    "Source",
                //    "FlightDate"
                //};
                //    object[] pvalue = new object[3]
                //{
                //    FlightNo,
                //    DepartureAirport,
                //    newFlightDate.Month.ToString().PadLeft(2,'0')+"/"+ newFlightDate.Day.ToString().PadLeft(2,'0')+"/"+newFlightDate.Year.ToString()

                //};
                //    SqlDbType[] ptype = new SqlDbType[3]
                //{
                //    SqlDbType.VarChar,
                //    SqlDbType.VarChar,
                //    SqlDbType.DateTime
                //};
                //    ds = dtb.SelectRecords("spExpManiGetAirlineSch1", pname, pvalue, ptype);

                DateTime newFlightDate = DateTime.Parse(FlightDate);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@FlightID", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@Source", SqlDbType.VarChar) { Value = DepartureAirport },
                    new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = newFlightDate.Month.ToString().PadLeft(2,'0')+"/"+ newFlightDate.Day.ToString().PadLeft(2,'0')+"/"+newFlightDate.Year.ToString() }
                };
                ds = await _readWriteDao.SelectRecords("spExpManiGetAirlineSch1", parameters);

                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                DataRow dr = ds.Tables[0].Rows[i];
                                string procedure = "spGetFFMDataForSend";

                                //string[] paramname = new string[] { "FltNo",
                                //                "ManifestdateFrom",
                                //                "ManifestdateTo",
                                //                "DepartureAirport",
                                //                "dest"};

                                //object[] paramvalue = new object[] { FlightNo,
                                //                 newFlightDate.Month.ToString().PadLeft(2,'0')+"/"+ newFlightDate.Day.ToString().PadLeft(2,'0')+"/"+newFlightDate.Year.ToString(),
                                //                 "",
                                //                 DepartureAirport,
                                //                 dr[0].ToString()};

                                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar,
                                //                      SqlDbType.DateTime,
                                //                      SqlDbType.VarChar,
                                //                      SqlDbType.VarChar};

                                //dsData = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);

                                SqlParameter[] parameters1 =
                                {
                                    new SqlParameter("@FltNo", SqlDbType.VarChar) { Value = FlightNo },
                                    new SqlParameter("@ManifestdateFrom", SqlDbType.DateTime) { Value = newFlightDate },
                                    new SqlParameter("@ManifestdateTo", SqlDbType.VarChar) { Value = "" },
                                    new SqlParameter("@DepartureAirport", SqlDbType.VarChar) { Value = DepartureAirport },
                                    new SqlParameter("@dest", SqlDbType.VarChar) { Value = dr[0].ToString() }
                                };

                                dsData = await _readWriteDao.SelectRecords(procedure, parameters1);

                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            return dsData;
        }

        public async Task<bool> addMsgToOutBox(string subject, string Msg, string FromEmailID, string ToEmailID)
        {
            bool flag = false;
            try
            {
                string procedure = "spInsertMsgToOutbox";

                //SQLServer dtb = new SQLServer();

                //string[] paramname = new string[] { "Subject",
                //                                "Body",
                //                                "FromEmailID",
                //                                "ToEmailID",
                //                                "CreatedOn" };

                //object[] paramvalue = new object[] {subject,
                //                                Msg,
                //                                FromEmailID,
                //                                ToEmailID,
                //                                System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")};

                //SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.DateTime };

                //flag = dtb.InsertData(procedure, paramname, paramtype, paramvalue);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@Subject", SqlDbType.VarChar) { Value = subject },
                    new SqlParameter("@Body", SqlDbType.VarChar) { Value = Msg },
                    new SqlParameter("@FromEmailID", SqlDbType.VarChar) { Value = FromEmailID },
                    new SqlParameter("@ToEmailID", SqlDbType.VarChar) { Value = ToEmailID },
                    new SqlParameter("@CreatedOn", SqlDbType.DateTime) { Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                flag = await _readWriteDao.ExecuteNonQueryAsync(procedure, parameters);
                if (!flag)
                {
                    //clsLog.WriteLogAzure("Error in addMsgToOutBox:" + dtb.LastErrorDescription);
                    clsLog.WriteLogAzure("Error in addMsgToOutBox:");

                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure("Error:" + ex.Message);
                flag = false;
            }
            return flag;
        }

        public async Task<bool> addMsgToOutBox(string subject, string Msg, string FromEmailID, string ToEmailID, string UpdatedBy, DateTime? UpdatedOn, string Type = "", string AWBNumber = "", string FlightNumber = "", string FlightDate = "1900-01-01", string FlightOrigin = "", string FlightDestination = "")
        {
            bool flag = false;
            string CarrierCode = FlightNumber.Substring(0, 2);
            try
            {
                string procedure = "spInsertMsgToOutbox";
                //    SQLServer dtb = new SQLServer();
                //    string[] paramname = new string[] { "Subject",
                //                                    "Body",
                //                                    "FromEmailID",
                //                                    "ToEmailID",
                //                                    "CreatedOn",
                //                                    "Type",
                //                                    "CreatedBy",
                //                                    "FlightNumber",
                //                                    "FlightOrigin",
                //                                    "FlightDestination",
                //                                    "AWBNumber",
                //                                    "FlightDate",
                //                                    "CarrierCode"

                //                                   };

                //    object[] paramvalue = new object[] {subject,
                //                                    Msg,
                //                                    FromEmailID,
                //                                    ToEmailID,
                //                                    UpdatedOn,
                //                                    Type,
                //                                    UpdatedBy,
                //                                    FlightNumber,
                //                                    FlightOrigin,
                //                                    FlightDestination,
                //                                    AWBNumber,
                //                                    FlightDate,
                //                                    CarrierCode

                //                                    };

                //    SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.DateTime,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.VarChar ,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.DateTime,
                //                                         SqlDbType.VarChar
                //};


                SqlParameter[] parameters =
                {
                    new SqlParameter("@Subject", SqlDbType.VarChar) { Value = subject },
                    new SqlParameter("@Body", SqlDbType.VarChar) { Value = Msg },
                    new SqlParameter("@FromEmailID", SqlDbType.VarChar) { Value = FromEmailID },
                    new SqlParameter("@ToEmailID", SqlDbType.VarChar) { Value = ToEmailID },
                    new SqlParameter("@CreatedOn", SqlDbType.DateTime) { Value = UpdatedOn },
                    new SqlParameter("@Type", SqlDbType.VarChar) { Value = Type },
                    new SqlParameter("@CreatedBy", SqlDbType.VarChar) { Value = UpdatedBy },
                    new SqlParameter("@FlightNumber", SqlDbType.VarChar) { Value = FlightNumber },
                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = FlightOrigin },
                    new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = FlightDestination },
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWBNumber },
                    new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = FlightDate },
                    new SqlParameter("@CarrierCode", SqlDbType.VarChar) { Value = CarrierCode }
                };

                //flag = dtb.InsertData(procedure, paramname, paramtype, paramvalue);

                flag = await _readWriteDao.ExecuteNonQueryAsync(procedure, parameters);

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                flag = false;
            }
            return flag;
        }

        public async Task<bool> InsertFFAInOutbox(string arr, string UserName)
        {
            bool flag = false;
            try
            {

                #region Prepare Parameters
                object[] RateCardInfo = new object[2];
                int i = 0;

                RateCardInfo.SetValue(arr, i);
                i++;

                //string UserName = Session["UserName"].ToString();
                RateCardInfo.SetValue(UserName, i);

                #endregion Prepare Parameters

                //string res = "";
                flag = await InsertFFAIntoOutbox(RateCardInfo);
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                flag = false;
            }
            return flag;
        }

        public async Task<DataSet> getFFMData(string DepartureAirport, string FlightNo, string FlightDate)
        {
            DataSet? dsData = new DataSet();
            try
            {
                DateTime newFlightDate = DateTime.Parse(FlightDate);

                //SQLServer dtb = new SQLServer();
                string procedure = "spGetFFMDataForSend";//"spGetFFMDataForSend";

                //string[] paramname = new string[] { "FltNo",
                //                                "ManifestdateFrom",
                //                                "ManifestdateTo",
                //                                "DepartureAirport" };

                //object[] paramvalue = new object[] { FlightNo,
                //                                 newFlightDate.Month.ToString().PadLeft(2,'0')+"/"+ newFlightDate.Day.ToString().PadLeft(2,'0')+"/"+newFlightDate.Year.ToString(),
                //                                 "",
                //                                 DepartureAirport };

                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar,
                //                                      SqlDbType.DateTime,
                //                                      SqlDbType.VarChar,
                //                                      SqlDbType.VarChar};

                //dsData = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@FltNo", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@ManifestdateFrom", SqlDbType.DateTime)
                    {
                        Value = newFlightDate.Month.ToString().PadLeft(2,'0')+"/"+ newFlightDate.Day.ToString().PadLeft(2,'0')+"/"+newFlightDate.Year.ToString()
                    },
                    new SqlParameter("@ManifestdateTo", SqlDbType.VarChar) { Value = "" },
                    new SqlParameter("@DepartureAirport", SqlDbType.VarChar) { Value = DepartureAirport }
                };

                dsData = await _readWriteDao.SelectRecords(procedure, parameters);
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            return dsData;
        }

        public async Task<bool> EncodeFHLForSend(string AWBNo, string FromEmailID, string ToEmailID)
        {
            bool flag = false;
            try
            {
                DataSet? ds = new DataSet();
                ds = await GetFHLData(AWBNo);

                MessageData.fhlinfo fhl = new MessageData.fhlinfo("");
                MessageData.consignmnetinfo[] objTempConsInfo = new MessageData.consignmnetinfo[0];
                MessageData.customsextrainfo[] custominfo = new MessageData.customsextrainfo[0];
                if (ds != null)
                {
                    string WeightCode = string.Empty;
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            //Master AWB number
                            DataRow dr = ds.Tables[0].Rows[0];
                            fhl.airlineprefix = dr["AWBPrefix"].ToString();
                            fhl.awbnum = dr["AWBNumber"].ToString();
                            fhl.origin = dr["OriginCode"].ToString();
                            fhl.dest = dr["DestinationCode"].ToString();
                            fhl.consigntype = "T";
                            fhl.pcscnt = dr["PiecesCount"].ToString();
                            fhl.weightcode = dr["UOM"].ToString();
                            fhl.weight = dr["GrossWeight"].ToString();
                            WeightCode = dr["UOM"].ToString();
                        }
                        if (ds.Tables[1].Rows.Count > 0)
                        {
                            //shipper-consignee info
                            DataRow dr = ds.Tables[1].Rows[0];
                            //5 shipper info

                            fhl.shippername = dr["ShipperName"].ToString();
                            fhl.shipperadd = dr["ShipperAddress"].ToString() + dr["ShipperAdd2"].ToString();
                            fhl.shipperplace = dr["ShipperCity"].ToString();
                            fhl.shipperstate = dr["ShipperState"].ToString();
                            fhl.shippercountrycode = dr["ShipperCountry"].ToString();
                            fhl.shipperpostcode = dr["ShipperPincode"].ToString();
                            fhl.shippercontactnum = dr["ShipperTelephone"].ToString();

                            //6 consignee info                    
                            fhl.consname = dr["ConsigneeName"].ToString();
                            fhl.consadd = dr["ConsigneeAddress"].ToString() + dr["ConsigneeAddress2"].ToString();
                            fhl.consplace = dr["ConsigneeCity"].ToString();
                            fhl.consstate = dr["ConsigneeState"].ToString();
                            fhl.conscountrycode = dr["ConsigneeCountry"].ToString();
                            fhl.conspostcode = dr["ConsigneePincode"].ToString();
                            fhl.conscontactnum = dr["ConsigneeTelephone"].ToString();

                        }
                        if (ds.Tables[2].Rows.Count > 0)
                        {
                            //Consignment info(houseAWB numbers)

                            Array.Resize(ref objTempConsInfo, ds.Tables[2].Rows.Count);
                            int i = 0;
                            foreach (DataRow dr in ds.Tables[2].Rows)
                            {
                                objTempConsInfo[i] = new MessageData.consignmnetinfo("");
                                objTempConsInfo[i].awbnum = dr["HAWBNo"].ToString();
                                objTempConsInfo[i].origin = dr["Origin"].ToString();
                                objTempConsInfo[i].dest = dr["Destination"].ToString();
                                objTempConsInfo[i].consigntype = "";
                                objTempConsInfo[i].pcscnt = dr["HAWBPcs"].ToString();
                                objTempConsInfo[i].weightcode = WeightCode != string.Empty ? WeightCode : "K";
                                objTempConsInfo[i].weight = dr["HAWBWt"].ToString();
                                objTempConsInfo[i].manifestdesc = dr["Description"].ToString();
                                objTempConsInfo[i].splhandling = dr["SHC"].ToString().Length > 0 ? dr["SHC"].ToString() : "";
                                i++;
                            }

                            string retval = cls_Encode_Decode.EncodeFHLforsend(ref fhl, ref objTempConsInfo, ref custominfo);
                            if (retval.Length > 0)
                            {
                                flag = await addMsgToOutBox("FHL", retval, FromEmailID, ToEmailID);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                flag = false;
            }
            return flag;
        }

        public async Task<DataSet?> GetFHLData(string MAWBNo)
        {
            try
            {
                DataSet? ds = new DataSet();

                //SQLServer da = new SQLServer();
                //string[] paramname = new string[1];
                //paramname[0] = "MAWBNo";
                //object[] paramvalue = new object[1];
                //paramvalue[0] = MAWBNo;
                //SqlDbType[] paramtype = new SqlDbType[1];
                //paramtype[0] = SqlDbType.VarChar;
                //ds = da.SelectRecords("spGetFHLData", paramname, paramvalue, paramtype);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@MAWBNo", SqlDbType.VarChar) { Value = MAWBNo }
                };
                ds = await _readWriteDao.SelectRecords("spGetFHLData", parameters);

                return ds;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return null;
            }
        }

        public async Task<bool> EncodeFFRForSendOLD(DataSet ds, int refNO)
        {
            bool flag = false;
            try
            {
                MessageData.ffrinfo objFFRInfo = new MessageData.ffrinfo("");
                MessageData.consignmnetinfo consigment = new MessageData.consignmnetinfo("");
                MessageData.FltRoute[] fltRoute = new MessageData.FltRoute[0];
                MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
                MessageData.dimensionnfo[] dimension = new MessageData.dimensionnfo[0];
                string agentcode = "";
                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow drAWBRateMaster = ds.Tables[1].Rows[0];

                            DataRow drAWBSummaryMaster = ds.Tables[0].Rows[0];

                            #region PrepareFFRStructureObject

                            //line 1 
                            objFFRInfo.ffrversionnum = "6";

                            #region Consigment Section
                            //line 2

                            consigment.airlineprefix = drAWBSummaryMaster["AWBPrefix"].ToString();
                            consigment.awbnum = drAWBSummaryMaster["AWBNumber"].ToString(); ;
                            consigment.origin = drAWBSummaryMaster["OriginCode"].ToString();
                            consigment.dest = drAWBSummaryMaster["DestinationCode"].ToString();
                            consigment.consigntype = "T";
                            consigment.pcscnt = drAWBRateMaster["Pieces"].ToString();
                            consigment.weightcode = drAWBSummaryMaster["UOM"].ToString();
                            consigment.weight = drAWBSummaryMaster["GrossWeight"].ToString();
                            consigment.volumecode = "";
                            consigment.volumeamt = "";
                            consigment.densityindicator = "";
                            consigment.densitygrp = "";
                            consigment.shpdesccode = "";
                            consigment.numshp = "";//drAWBRateMaster["Pieces"].ToString();
                            //objFFRInfo.manifestdesc = drAWBRateMaster["CommodityDesc"].ToString().Length > 1 ? drAWBRateMaster["CommodityDesc"].ToString() : "GEN";
                            consigment.manifestdesc = drAWBRateMaster["CommodityCode"].ToString().Length > 0 ? drAWBRateMaster["CommodityCode"].ToString() : "GEN";
                            consigment.splhandling = "";
                            #endregion


                            //line 3
                            #region FLTROUTE
                            if (ds.Tables.Count > 3)
                            {
                                if (ds.Tables[3].Rows.Count > 0)
                                {
                                    for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                                    {
                                        DataRow drAWBRouteMaster = ds.Tables[3].Rows[i];
                                        MessageData.FltRoute route = new MessageData.FltRoute("");
                                        try
                                        {
                                            DateTime dtTo = new DateTime();

                                            string dt = (drAWBRouteMaster["FltDate"].ToString());
                                            //dt = dt + " " + DateTime.Now.ToShortTimeString();
                                            //dtfrom = DateTime.ParseExact(dt,"dd-MM-yyyy",null);
                                            dtTo = DateTime.ParseExact(drAWBRouteMaster["FltDate"].ToString(), "dd/MM/yyyy", null);
                                            //ToDt = dt.ToString();
                                            string day = dt.Substring(0, 2);
                                            string mon = dtTo.ToString("MMM");
                                            //string mon = dt.Substring(3, 2);
                                            string yr = dt.Substring(6, 4);
                                            route.date = day.ToString();
                                            route.month = mon.ToString();

                                        }
                                        catch (Exception ex) { clsLog.WriteLogAzure(ex); }
                                        route.carriercode = "";
                                        route.fltnum = drAWBRouteMaster["FltNumber"].ToString();

                                        route.fltdept = drAWBRouteMaster["FltOrigin"].ToString();
                                        route.fltarrival = drAWBRouteMaster["FltDestination"].ToString();
                                        route.spaceallotmentcode = "";
                                        route.allotidentification = "";
                                        try
                                        {
                                            string AWBStatus = "";
                                            AWBStatus = drAWBRouteMaster["Status"].ToString();
                                            if (AWBStatus.Trim() != "")
                                            {
                                                if (AWBStatus.Trim().Equals("Q", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    route.spaceallotmentcode = "LL";
                                                }
                                                else if (AWBStatus.Trim().Equals("C", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    route.spaceallotmentcode = "KK";
                                                }
                                            }
                                            else
                                            {
                                                route.spaceallotmentcode = drAWBRouteMaster["Status"].ToString(); ;
                                            }
                                        }
                                        catch (Exception ex)
                                        { clsLog.WriteLogAzure(ex); }

                                        Array.Resize(ref fltRoute, fltRoute.Length + 1);
                                        fltRoute[fltRoute.Length - 1] = route;
                                    }
                                }
                            }
                            #endregion

                            //line 4
                            objFFRInfo.noofuld = "";
                            //line 5 
                            objFFRInfo.specialservicereq1 = "";
                            objFFRInfo.specialservicereq2 = "";
                            //line 6
                            objFFRInfo.otherserviceinfo1 = "";
                            objFFRInfo.otherserviceinfo2 = "";
                            //line 7
                            objFFRInfo.bookingrefairport = drAWBSummaryMaster["OriginCode"].ToString();
                            objFFRInfo.officefundesignation = "FF";
                            objFFRInfo.companydesignator = "XX";
                            objFFRInfo.bookingfileref = "";
                            objFFRInfo.participentidetifier = "";
                            objFFRInfo.participentcode = "";
                            objFFRInfo.participentairportcity = "";
                            // objFFRInfo.participentairportcity = drAWBSummaryMaster["OriginCode"].ToString();
                            // objFFRInfo.participentcode = "";
                            // objFFRInfo.participentidetifier = "";

                            //line 8
                            #region Dimension
                            try
                            {
                                if (ds.Tables.Count > 2)
                                {
                                    if (ds.Tables[2].Rows.Count > 0)
                                    {
                                        for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                                        {
                                            MessageData.dimensionnfo dim = new MessageData.dimensionnfo("");
                                            DataRow drAWBDimensions = ds.Tables[2].Rows[i];
                                            dim.weightcode = "";
                                            dim.weight = "";
                                            dim.mesurunitcode = "";
                                            if (drAWBDimensions["MeasureUnit"].ToString().Trim().ToUpper() == "CMS")
                                            {
                                                dim.mesurunitcode = "CMT";
                                            }
                                            else if (drAWBDimensions["MeasureUnit"].ToString().Trim().ToUpper() == "INCHES")
                                            {
                                                dim.mesurunitcode = "INH";
                                            }
                                            dim.length = drAWBDimensions["Length"].ToString();
                                            dim.width = drAWBDimensions["Breadth"].ToString();
                                            dim.height = drAWBDimensions["Height"].ToString();
                                            dim.piecenum = drAWBDimensions["PcsCount"].ToString();
                                            Array.Resize(ref dimension, dimension.Length + 1);
                                            dimension[dimension.Length - 1] = dim;

                                        }
                                    }
                                }
                            }
                            catch (Exception ex) { clsLog.WriteLogAzure(ex); }

                            #endregion
                            //line 9 
                            objFFRInfo.servicecode = "";
                            objFFRInfo.rateclasscode = "";
                            objFFRInfo.commoditycode = "";

                            try
                            {
                                if (ds.Tables[6].Rows.Count > 0)
                                {
                                    DataRow drAWBShipperConsigneeDetails = ds.Tables[6].Rows[0];
                                    objFFRInfo.shipperaccnum = "";//[ShipperAccCode]
                                    objFFRInfo.shippername = drAWBShipperConsigneeDetails["ShipperName"].ToString();
                                    objFFRInfo.shipperadd = drAWBShipperConsigneeDetails["ShipperAddress"].ToString() + " " + drAWBShipperConsigneeDetails["ShipperAdd2"].ToString();
                                    objFFRInfo.shipperplace = drAWBShipperConsigneeDetails["ShipperCity"].ToString();
                                    objFFRInfo.shipperstate = drAWBShipperConsigneeDetails["ShipperState"].ToString();
                                    objFFRInfo.shippercountrycode = drAWBShipperConsigneeDetails["ShipperCountry"].ToString().Substring(0, 2);
                                    objFFRInfo.shipperpostcode = drAWBShipperConsigneeDetails["ShipperPincode"].ToString();
                                    objFFRInfo.shippercontactidentifier = "TE";
                                    objFFRInfo.shippercontactnum = drAWBShipperConsigneeDetails["ShipperTelephone"].ToString();

                                    //line 11
                                    objFFRInfo.consaccnum = "";//[ConsigAccCode]
                                    objFFRInfo.consname = drAWBShipperConsigneeDetails["ConsigneeName"].ToString();
                                    objFFRInfo.consadd = drAWBShipperConsigneeDetails["ConsigneeAddress"].ToString() + " " + drAWBShipperConsigneeDetails["ConsigneeAddress2"].ToString(); ;
                                    objFFRInfo.consplace = drAWBShipperConsigneeDetails["ConsigneeCity"].ToString();
                                    objFFRInfo.consstate = drAWBShipperConsigneeDetails["ConsigneeState"].ToString();
                                    objFFRInfo.conscountrycode = drAWBShipperConsigneeDetails["ConsigneeCountry"].ToString().Substring(0, 2);
                                    objFFRInfo.conspostcode = drAWBShipperConsigneeDetails["ConsigneePincode"].ToString(); ;
                                    objFFRInfo.conscontactidentifier = "TE";
                                    objFFRInfo.conscontactnum = drAWBShipperConsigneeDetails["ConsigneeTelephone"].ToString();
                                }
                            }
                            catch (Exception ex) { clsLog.WriteLogAzure(ex); }
                            //line 12
                            objFFRInfo.custaccnum = "";
                            objFFRInfo.iatacargoagentcode = ""; //drAWBSummaryMaster["AgentCode"].ToString();
                            objFFRInfo.cargoagentcasscode = "";
                            objFFRInfo.custparticipentidentifier = "";
                            objFFRInfo.custname = "";
                            objFFRInfo.custplace = "";
                            agentcode = drAWBSummaryMaster["AgentCode"].ToString();
                            //line 13
                            objFFRInfo.shiprefnum = "";
                            objFFRInfo.supplemetryshipperinfo1 = "";
                            objFFRInfo.supplemetryshipperinfo2 = "";


                            #endregion

                            string strMsg = cls_Encode_Decode.encodeFFRforsend(ref objFFRInfo, ref objULDInfo, ref consigment, ref fltRoute, ref dimension);
                            if (strMsg != null)
                            {
                                if (strMsg.Trim() != "")
                                {
                                    flag = await addMsgToOutBox("FFR", strMsg, "swapnil@qidtech.com", "", agentcode, refNO);
                                    //InsertFFRToOutBox(strMsg, "swapnil@qidtech.com", "",agentcode,refNO);
                                    //flag = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            { clsLog.WriteLogAzure(ex); }
            return flag;
        }

        public async Task<bool> EncodeFHLForSend(DataSet ds, int refNo)//(string AWBNo, string FromEmailID, string ToEmailID)
        {
            bool flag = false;
            try
            {
                //   ds = GetFHLData(AWBNo);
                string agentcode = "";
                string WeightCode = string.Empty;
                MessageData.fhlinfo fhl = new MessageData.fhlinfo("");
                MessageData.consignmnetinfo[] objTempConsInfo = new MessageData.consignmnetinfo[0];
                MessageData.customsextrainfo[] custominfo = new MessageData.customsextrainfo[0];
                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {//Master AWB number
                            DataRow dr = ds.Tables[0].Rows[0];
                            fhl.airlineprefix = dr["AWBPrefix"].ToString();
                            fhl.awbnum = dr["AWBNumber"].ToString();
                            fhl.origin = dr["OriginCode"].ToString();
                            fhl.dest = dr["DestinationCode"].ToString();
                            fhl.consigntype = "T";
                            fhl.pcscnt = dr["PiecesCount"].ToString();
                            fhl.weightcode = dr["UOM"].ToString();
                            fhl.weight = dr["GrossWeight"].ToString();
                            agentcode = dr["AgentCode"].ToString();
                            WeightCode = dr["UOM"].ToString();
                        }
                        if (ds.Tables[6].Rows.Count > 0)
                        {
                            //shipper-consignee info
                            DataRow dr = ds.Tables[6].Rows[0];

                            //5 shipper info

                            fhl.shippername = dr["ShipperName"].ToString();
                            fhl.shipperadd = dr["ShipperAddress"].ToString();
                            fhl.shipperplace = "";
                            fhl.shipperstate = "";
                            fhl.shippercountrycode = dr["ShipperCountry"].ToString();
                            fhl.shipperpostcode = "";
                            fhl.shippercontactnum = dr["ShipperTelephone"].ToString();

                            //6 consignee info                    
                            fhl.consname = dr["ConsigneeName"].ToString();
                            fhl.consadd = dr["ConsigneeAddress"].ToString();
                            fhl.consplace = "";
                            fhl.consstate = "";
                            fhl.conscountrycode = dr["ConsigneeCountry"].ToString();
                            fhl.conspostcode = "";
                            fhl.conscontactnum = dr["ConsigneeTelephone"].ToString();

                        }
                        if (ds.Tables[10].Rows.Count > 0)
                        {
                            //Consignment info(houseAWB numbers)

                            Array.Resize(ref objTempConsInfo, ds.Tables[10].Rows.Count);
                            int i = 0;
                            foreach (DataRow dr in ds.Tables[10].Rows)
                            {
                                objTempConsInfo[i] = new MessageData.consignmnetinfo("");
                                objTempConsInfo[i].awbnum = dr["HAWBNo"].ToString();
                                objTempConsInfo[i].origin = dr["Origin"].ToString();
                                objTempConsInfo[i].dest = dr["Destination"].ToString();
                                objTempConsInfo[i].consigntype = "";
                                objTempConsInfo[i].pcscnt = dr["HAWBPcs"].ToString();
                                objTempConsInfo[i].weightcode = WeightCode != string.Empty ? WeightCode : "K";
                                objTempConsInfo[i].weight = dr["HAWBWt"].ToString();
                                objTempConsInfo[i].manifestdesc = dr["Description"].ToString();
                                objTempConsInfo[i].splhandling = dr["SHC"].ToString().Length > 0 ? dr["SHC"].ToString() : "";
                                i++;
                            }

                            string retval = cls_Encode_Decode.EncodeFHLforsend(ref fhl, ref objTempConsInfo, ref custominfo);
                            if (retval.Length > 0)
                            {
                                flag = await addMsgToOutBox("FHL", retval, "swapnil@qidtech.com", "", agentcode, refNo);
                            }
                        }
                        else
                        {
                            //string[] PName = new string[] { "Error", "refNo" };
                            //object[] PValue = new object[] { "Required Data Not Availabe for Message", refNo };
                            //SqlDbType[] PType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.Int };


                            //SQLServer objSQL = new SQLServer();
                            //if (!objSQL.ExecuteProcedure("spOutboxErrorUpdate", PName, PType, PValue))
                            //{
                            //}

                            SqlParameter[] parameters =
                            {
                                new SqlParameter("@Error", SqlDbType.VarChar) { Value = "Required Data Not Availabe for Message" },
                                new SqlParameter("@refNo", SqlDbType.Int) { Value = refNo }
                            };

                            var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spOutboxErrorUpdate", parameters);
                            if (!dbRes)
                            {
                                clsLog.WriteLogAzure("Error on calling spOutboxErrorUpdate in EncodeFHLForSend");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                flag = false;
            }
            return flag;
        }

        public async Task<bool> EncodeFBLForSend(string POL, string FlightNo, string FlightDate, string FromEmailID, string ToEmailID, int refNo)
        {
            MessageData.fblinfo objFBLInfo = new MessageData.fblinfo("");
            MessageData.unloadingport[] objUnloadingPort = new MessageData.unloadingport[0];
            MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
            int count1 = 0;
            int count2 = 0;
            bool flag = false;
            try
            {
                DataSet? dsData = new DataSet();

                dsData = await getFBLData(POL, FlightNo, DateTime.Parse(FlightDate).ToString("MM/dd/yyyy"));

                if (dsData != null)
                {
                    if (dsData.Tables.Count > 1)
                    {
                        objFBLInfo.fblversion = "3";
                        objFBLInfo.messagesequencenum = "1";
                        objFBLInfo.carriercode = FlightNo.Substring(0, 2);
                        objFBLInfo.fltnum = FlightNo.Substring(2, FlightNo.Length - 2);
                        //objFBLInfo.date = DateTime.Parse(FlightDate).Day.ToString();
                        //objFBLInfo.month = DateTime.Parse(FlightDate).Month.ToString("");
                        DateTime dtFlight = DateTime.Parse(FlightDate);//, "dd/MM/yyyy", null);
                        objFBLInfo.date = DateTime.Parse(FlightDate).Day.ToString().PadLeft(2, '0');
                        objFBLInfo.month = dtFlight.ToString("MMM");
                        objFBLInfo.fltairportcode = POL;
                        objFBLInfo.endmesgcode = "LAST";

                        //flight details
                        if (dsData.Tables[0].Rows.Count > 0)
                        {
                            count1 = 1;
                            foreach (DataRow dr in dsData.Tables[0].Rows)
                            {
                                MessageData.unloadingport objTempUnloadingPort = new MessageData.unloadingport("");
                                objTempUnloadingPort.unloadingairport = dr[2].ToString();
                                Array.Resize(ref objUnloadingPort, count1);
                                objUnloadingPort[count1 - 1] = objTempUnloadingPort;
                                count1++;
                            }
                        }
                        //awb details
                        if (dsData.Tables[1].Rows.Count > 0)
                        {
                            count2 = 1;
                            foreach (DataRow dr in dsData.Tables[1].Rows)
                            {
                                MessageData.consignmnetinfo objTempConsInfo = new MessageData.consignmnetinfo("");
                                string AWBNumber = dr[0].ToString().Trim();
                                objTempConsInfo.airlineprefix = AWBNumber.Substring(0, 2);
                                objTempConsInfo.awbnum = AWBNumber.Substring(2, AWBNumber.Length - 2);
                                objTempConsInfo.origin = dr[11].ToString().Trim();
                                objTempConsInfo.dest = dr[5].ToString().Trim();
                                objTempConsInfo.consigntype = "T";
                                objTempConsInfo.pcscnt = dr[3].ToString().Trim();
                                objTempConsInfo.weightcode = dr["UOM"].ToString();
                                objTempConsInfo.weight = dr[4].ToString().Trim();
                                objTempConsInfo.manifestdesc = dr[8].ToString().Trim() + "-" + dr[6].ToString().Trim();

                                Array.Resize(ref objConsInfo, count2);
                                objConsInfo[count2 - 1] = objTempConsInfo;
                                count2++;
                            }
                        }
                        if (count1 > 0 && count2 > 0)
                        {
                            MessageData.dimensionnfo[] objDimenInfo = new MessageData.dimensionnfo[0];
                            MessageData.consignmentorigininfo[] objConsOriginInfo = new MessageData.consignmentorigininfo[0];
                            MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
                            MessageData.otherserviceinfo objOtherInfo = new MessageData.otherserviceinfo("");
                            string FBLMsg = cls_Encode_Decode.EncodeFBLforsend(objFBLInfo, objUnloadingPort, objConsInfo, objDimenInfo, objConsOriginInfo, objULDInfo, objOtherInfo);
                            if (FBLMsg != null)
                            {
                                if (FBLMsg.Trim() != "")
                                {
                                    //bool flag = false;
                                    flag = await addMsgToOutBox("FBL", FBLMsg, FromEmailID, ToEmailID, "", refNo);
                                }
                            }
                        }
                        else
                        {
                            //string[] PName = new string[] { "Error", "refNo" };
                            //object[] PValue = new object[] { "Required Data Not Availabe for Message", refNo };
                            //SqlDbType[] PType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.Int };
                            //SQLServer objSQL = new SQLServer();
                            //if (!objSQL.ExecuteProcedure("spOutboxErrorUpdate", PName, PType, PValue))
                            //{
                            //}

                            SqlParameter[] parameters =
                                                      {
                                new SqlParameter("@Error", SqlDbType.VarChar) { Value = "Required Data Not Availabe for Message" },
                                new SqlParameter("@refNo", SqlDbType.Int) { Value = refNo }
                            };

                            var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spOutboxErrorUpdate", parameters);
                            if (!dbRes)
                            {
                                clsLog.WriteLogAzure("Error on calling spOutboxErrorUpdate in EncodeFBLForSend");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            { clsLog.WriteLogAzure(ex); }
            return flag;
        }

        public async Task<bool> EncodeFFMForSend(string DepartureAirport, string FlightNo, string FlightDate, string FromEmailID, string ToEmailID, int refNo)
        {
            bool flag = false;
            try
            {
                MessageData.ffminfo objFFMInfo = new MessageData.ffminfo("");
                MessageData.unloadingport[] objUnloadingPort = new MessageData.unloadingport[0];
                MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
                int count1 = 0;
                int count2 = 0;
                //bool flag = false;
                DataSet? dsData = new DataSet();
                DataSet? ds = new DataSet();
                ds = await getFFMUnloadingPort(DepartureAirport, FlightNo, DateTime.Parse(FlightDate).ToString("MM/dd/yyyy"));
                dsData = await getFFMData(DepartureAirport, FlightNo, DateTime.Parse(FlightDate).ToString("MM/dd/yyyy"));
                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)//(dsData.Tables[0].Rows.Count > 0)
                        {
                            count1 = 1;
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                MessageData.unloadingport objTempUnloadingPort = new MessageData.unloadingport("");
                                objTempUnloadingPort.unloadingairport = dr[0].ToString();
                                Array.Resize(ref objUnloadingPort, count1);
                                objUnloadingPort[count1 - 1] = objTempUnloadingPort;
                                count1++;
                            }
                        }
                    }
                }
                if (dsData != null)
                {
                    if (dsData.Tables.Count > 0)
                    {
                        objFFMInfo.ffmversionnum = "8";
                        objFFMInfo.messagesequencenum = "1";
                        objFFMInfo.carriercode = FlightNo.Substring(0, 2);
                        objFFMInfo.fltnum = FlightNo.Substring(2, FlightNo.Length - 2);
                        //objFFMInfo.date = DateTime.Parse(FlightDate).Day.ToString();
                        //objFFMInfo.month = DateTime.Parse(FlightDate).Month.ToString("");
                        DateTime dtFlight = DateTime.Parse(FlightDate);//, "dd/MM/yyyy", null);
                        objFFMInfo.fltdate = dtFlight.ToString("dd");//DateTime.Parse(FlightDate).Day.ToString().PadLeft(2, '0');
                        objFFMInfo.month = dtFlight.ToString("MMM");
                        objFFMInfo.fltairportcode = DepartureAirport;
                        objFFMInfo.endmesgcode = "LAST";

                        //flight details

                        //awb details
                        if (dsData.Tables[1].Rows.Count > 0)
                        {
                            count2 = 1;
                            foreach (DataRow dr in dsData.Tables[1].Rows)
                            {
                                MessageData.consignmnetinfo objTempConsInfo = new MessageData.consignmnetinfo("");
                                string AWBNumber = dr[5].ToString().Trim();
                                objTempConsInfo.airlineprefix = AWBNumber.Substring(0, 2);
                                objTempConsInfo.awbnum = AWBNumber.Substring(2, AWBNumber.Length - 2);
                                objTempConsInfo.origin = dr[13].ToString().Trim();
                                objTempConsInfo.dest = dr[3].ToString().Trim();
                                objTempConsInfo.consigntype = "T";
                                objTempConsInfo.pcscnt = dr[7].ToString().Trim();
                                objTempConsInfo.weightcode = dr["UOM"].ToString();
                                objTempConsInfo.weight = dr[8].ToString().Trim();
                                objTempConsInfo.manifestdesc = dr[6].ToString().Trim() + "-" + dr[12].ToString().Trim();

                                for (int k = 0; k < objUnloadingPort.Length; k++)
                                {
                                    if (objUnloadingPort[k].unloadingairport.Equals(dr[3].ToString().Trim(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        objTempConsInfo.portsequence = (k + 1).ToString();
                                    }
                                }
                                Array.Resize(ref objConsInfo, count2);
                                objConsInfo[count2 - 1] = objTempConsInfo;
                                count2++;
                            }
                        }
                        if (count1 > 0 && count2 > 0)
                        {
                            MessageData.dimensionnfo[] objDimenInfo = new MessageData.dimensionnfo[0];
                            MessageData.consignmentorigininfo[] objConsOriginInfo = new MessageData.consignmentorigininfo[0];
                            MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
                            MessageData.otherserviceinfo[] objOtherInfo = new MessageData.otherserviceinfo[0];
                            MessageData.movementinfo[] objMovemInfo = new MessageData.movementinfo[0];
                            MessageData.customsextrainfo[] objcustInfo = new MessageData.customsextrainfo[0];
                            //string FFMMsg = "";
                            string FFMMsg = cls_Encode_Decode.EncodeFFMforsend(ref objFFMInfo, ref objUnloadingPort, ref objConsInfo, ref objDimenInfo,
                                                                               ref objMovemInfo, ref objOtherInfo, ref objcustInfo, ref objULDInfo);
                            if (FFMMsg != null)
                            {
                                if (FFMMsg.Trim() != "")
                                {
                                    //bool flag = false;
                                    flag = await addMsgToOutBox("FFM", FFMMsg, FromEmailID, ToEmailID, "", refNo);
                                }
                            }
                        }
                        else
                        {
                            //string[] PName = new string[] { "Error", "refNo" };
                            //object[] PValue = new object[] { "Required Data Not Availabe for Message", refNo };
                            //SqlDbType[] PType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.Int };
                            //SQLServer objSQL = new SQLServer();
                            //if (!objSQL.ExecuteProcedure("spOutboxErrorUpdate", PName, PType, PValue))
                            //{
                            //}

                            SqlParameter[] parameters =
                            {
                                  new SqlParameter("@Error", SqlDbType.VarChar) { Value = "Required Data Not Availabe for Message" },
                                  new SqlParameter("@refNo", SqlDbType.Int) { Value = refNo }
                            };

                            var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spOutboxErrorUpdate", parameters);
                            if (!dbRes)
                            {
                                clsLog.WriteLogAzure("Error on calling spOutboxErrorUpdate in EncodeFHLForSend");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            { clsLog.WriteLogAzure(ex); }
            return flag;
        }


        /*Not in use*/
        //public int ExportManifestSummary(string FlightNo, string POL, string POU, DateTime FltDate)
        //{
        //    int ID = 0;
        //    try
        //    {
        //        SQLServer db = new SQLServer(); ;
        //        string[] param = { "FLTno", "POL", "POU", "FLTDate" };
        //        SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime };
        //        object[] values = { FlightNo, POL, POU, FltDate };

        //        ID = db.GetIntegerByProcedure("spExpManifestSummaryFFM", param, values, sqldbtypes);
        //        if (ID < 1)
        //        {
        //            clsLog.WriteLogAzure("Error saving ExportFFM:" + db.LastErrorDescription);
        //        }
        //        //res = db.InsertData("SPImpManiSaveManifest", param, sqldbtypes, values);


        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure("Error saving ExpFFM:" + ex.Message);
        //        ID = 0;

        //    }
        //    return ID;
        //}

        /*Not in use*/

        //public bool ExportManifestDetails(string POL, string POU, string ORG, string DES, string AWBno, string SCC, string VOL, string PCS, string WGT, string Desc, DateTime FltDate, int ManifestID)
        //{
        //    bool res;
        //    try
        //    {

        //        SQLServer db = new SQLServer(); ;
        //        string[] param = { "POL", "POU", "ORG", "DES", "AWBno", "SCC", "VOL", "PCS", "WGT", "Desc", "FLTDate", "ManifestID" };
        //        SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Int };
        //        object[] values = { POL, POU, ORG, DES, AWBno, SCC, VOL, PCS, WGT, Desc, FltDate, ManifestID };
        //        //res = db.InsertData("spExpManifestDetailsFFM", param, sqldbtypes, values);
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
        //        clsLog.WriteLogAzure(ex);
        //        res = false;
        //    }
        //    return res;
        //}

        /*Not in use*/

        //public bool ExportManifestDetails(string ULDNo, string POL, string POU, string ORG, string DES, string AWBno, string SCC, string VOL, string PCS, string WGT, string Desc, DateTime FltDate, int ManifestID, string FlightNo, string AWBPrefix)
        //{
        //    bool res;
        //    try
        //    {

        //        SQLServer db = new SQLServer(); ;
        //        string[] param = { "POL", "POU", "ORG", "DES", "AWBno", "SCC", "VOL", "PCS", "WGT", "Desc", "FLTDate", "ManifestID", "AWBPrefix", "FlightNo", "ULDNo" };
        //        SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
        //        object[] values = { POL, POU, ORG, DES, AWBno, SCC, VOL, PCS, WGT, Desc, FltDate, ManifestID, AWBPrefix, FlightNo, ULDNo };
        //        //res = db.InsertData("spExpManifestDetailsFFM", param, sqldbtypes, values);
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
        //        clsLog.WriteLogAzure(ex);
        //        res = false;
        //    }
        //    return res;
        //}

        /*Not in use*/

        //public bool ExportManifestULDAWBAssociation(string ULDNo, string POL, string POU, string ORG, string DES, string AWBno, string SCC, string VOL, string PCS, string WGT, string Desc, DateTime FltDate, int ManifestID, string awbprefix, string flightno, string BkdPcs, string BkdWt, string ConsignmentType)
        //{
        //    bool res;
        //    try
        //    {
        //        SQLServer db = new SQLServer(); ;
        //        string[] param = { "ULDNo", "POL", "POU", "ORG", "DES", "AWBno", "SCC", "VOL", "PCS", "WGT", "Desc", "FLTDate", "ManifestID", "AWBPrefix", "FlightNo", "BkdPcs", "BkdWt", "Source", "ConsignmentType" };
        //        SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
        //                                     SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.BigInt, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar };
        //        object[] values = { ULDNo, POL, POU, ORG, DES, AWBno, SCC, VOL, PCS, WGT, Desc, FltDate, ManifestID, awbprefix, flightno, BkdPcs, BkdWt, "M", ConsignmentType };
        //        //res = db.InsertData("spExpManifestULDAWBFFM", param, sqldbtypes, values);
        //        if (db.InsertData("spExpManifestULDAWBFFM", param, sqldbtypes, values))
        //        {
        //            res = true;
        //        }
        //        else
        //        {
        //            clsLog.WriteLog("Failes ManifDetails Save:" + AWBno + " Error: " + db.LastErrorDescription);
        //            res = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLog("Error saving EXPULDAWB:" + ex.Message);
        //        res = false;
        //    }
        //    return res;
        //}

        /*Not in use*/

        //booked,accepted,manifested pcs are same
        //public bool ExportManifestULDAWBAssociation_old(string ULDNo, string POL, string POU, string ORG, string DES, string AWBno, string SCC, string VOL, string PCS, string WGT, string Desc, string FltDate, int ManifestID, string awbprefix, string flightno)
        //{
        //    bool res;
        //    try
        //    {
        //        SQLServer db = new SQLServer(); ;
        //        string[] param = { "ULDNo", "POL", "POU", "ORG", "DES", "AWBno", "SCC", "VOL", "PCS", "WGT", "Desc", "FLTDate", "ManifestID", "AWBPrefix", "FlightNo" };
        //        SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.BigInt, SqlDbType.VarChar, SqlDbType.VarChar };
        //        object[] values = { ULDNo, POL, POU, ORG, DES, AWBno, SCC, VOL, PCS, WGT, Desc, FltDate, ManifestID, awbprefix, flightno };
        //        //res = db.InsertData("spExpManifestULDAWBFFM", param, sqldbtypes, values);
        //        if (db.InsertData("spExpManifestULDAWBFFM", param, sqldbtypes, values))
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
        //        clsLog.WriteLogAzure("Error saving EXPULDAWB:" + ex.Message);
        //        res = false;
        //    }
        //    return res;
        //}

        /*Not in use*/

        //public bool ExportManifestULDAWBAssociation(string ULDNo, string POL, string POU, string ORG, string DES, string AWBno, string SCC, string VOL, string PCS, string WGT, string Desc, string FltDate, int ManifestID, string awbprefix, string flightno, string BkdPcs, string BkdWt)
        //{
        //    bool res;
        //    try
        //    {
        //        SQLServer db = new SQLServer(); ;
        //        string[] param = { "ULDNo", "POL", "POU", "ORG", "DES", "AWBno", "SCC", "VOL", "PCS", "WGT", "Desc", "FLTDate", "ManifestID", "AWBPrefix", "FlightNo", "BkdPcs", "BkdWt" };
        //        SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.BigInt, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
        //        object[] values = { ULDNo, POL, POU, ORG, DES, AWBno, SCC, VOL, PCS, WGT, Desc, FltDate, ManifestID, awbprefix, flightno, BkdPcs, BkdWt };
        //        //res = db.InsertData("spExpManifestULDAWBFFM", param, sqldbtypes, values);
        //        if (db.InsertData("spExpManifestULDAWBFFM", param, sqldbtypes, values))
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
        //        clsLog.WriteLogAzure("Error saving EXPULDAWB:" + ex.Message);
        //        res = false;
        //    }
        //    return res;
        //}

        /*Not in use*/

        //public bool ExportManifestULDAWBAssociation(string ULDNo, string POL, string POU, string ORG, string DES, string AWBno, string SCC, string VOL, string PCS, string WGT, string Desc, DateTime FltDate, int ManifestID)
        //{
        //    bool res;
        //    try
        //    {
        //        SQLServer db = new SQLServer(); ;
        //        string[] param = { "ULDNo", "POL", "POU", "ORG", "DES", "AWBno", "SCC", "VOL", "PCS", "WGT", "Desc", "FLTDate", "ManifestID" };
        //        SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Int };
        //        object[] values = { ULDNo, POL, POU, ORG, DES, AWBno, SCC, VOL, PCS, WGT, Desc, FltDate, ManifestID };
        //        //res = db.InsertData("spExpManifestULDAWBFFM", param, sqldbtypes, values);
        //        if (db.InsertData("spExpManifestULDAWBFFM", param, sqldbtypes, values))
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
        //        clsLog.WriteLogAzure("Error saving EXPULDAWB:" + ex.Message);
        //        res = false;
        //    }
        //    return res;
        //}

        /*Not in use*/

        //public static bool StoreImportFFM_Summary(string FlightNo, string POL, string POU, DateTime FltDate)
        //{
        //    bool res;

        //    try
        //    {
        //        SQLServer db = new SQLServer(); ;
        //        string[] param = { "FLTno", "POL", "POU", "FLTDate" };
        //        SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime };
        //        object[] values = { FlightNo, POL, POU, FltDate };

        //        //res = db.InsertData("SPImpManiSaveManifest", param, sqldbtypes, values);
        //        if (db.InsertData("SPImpManiSaveManifest", param, sqldbtypes, values))
        //        {
        //            res = true;
        //        }
        //        else
        //        {
        //            clsLog.WriteLogAzure("Failes ManifSummary Save:" + FlightNo + " Error: " + db.LastErrorDescription);
        //            res = false;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure("Error in FFM ManifestSummary" + ex.Message);
        //        res = false;
        //    }
        //    return res;

        //}

        /*Not in use*/
        //public static bool StoreImportFFM_Details(string POL, string POU, string ORG, string DES, string AWBno, string SCC, string VOL, string PCS, string WGT, string Desc, DateTime FltDate, string uldno)
        //{
        //    bool res = false;

        //    try
        //    {

        //        SQLServer db = new SQLServer(); ;
        //        string[] param = { "POL", "POU", "ORG", "DES", "AWBno", "SCC", "VOL", "PCS", "WGT", "Desc", "FLTDate", "ULDNo" };
        //        SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar };
        //        object[] values = { POL, POU, ORG, DES, AWBno, SCC, VOL, PCS, WGT, Desc, FltDate, uldno };

        //        if (db.InsertData("SPImpManiSaveManifestDetails", param, sqldbtypes, values))
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
        //        clsLog.WriteLogAzure("Error in FFM Manifest Details:" + ex.Message);
        //        res = false;
        //    }
        //    return res;

        //}

        /*Not in use*/
        //public static bool ULDawbAssociation(string FltNo, string POL, string POU, string AWBno, string PCS, string WGT, DateTime FltDate, string ULDNo)
        //{
        //    bool res;
        //    try
        //    {
        //        SQLServer db = new SQLServer(); ;
        //        string[] param = { "ULDtripid", "ULDNo", "AWBNumber", "POL", "POU", "FltNo", "Pcs", "Wgt", "AvlPcs", "AvlWgt", "Updatedon", "Updatedby", "Status", "Manifested", "FltDate" };

        //        int _pcs = int.Parse(PCS);
        //        float _wgt = float.Parse(WGT);
        //        SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar
        //                                     , SqlDbType.Int, SqlDbType.Float, SqlDbType.Int, SqlDbType.Float,SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.Bit,SqlDbType.Bit, SqlDbType.DateTime };
        //        object[] values = { "", ULDNo, AWBno, POL, POU, FltNo, 0, 0, _pcs, _wgt, DateTime.Now, "FFM", false, false, FltDate };


        //        //res = db.InsertData("SPImpManiSaveUldAwbAssociation", param, sqldbtypes, values);
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
        //        clsLog.WriteLogAzure("Error in FFM ULDAWBAssociation:" + ex.Message);
        //        res = false;
        //    }
        //    return res;
        //}

        /*Not in use*/

        //Jet Messaging specific system
        //public bool EncodeFFRForSend(DataSet ds, int refNO)
        //{
        //    bool flag = false;
        //    try
        //    {
        //        MessageData.ffrinfo objFFRInfo = new MessageData.ffrinfo("");
        //        MessageData.consignmnetinfo consigment = new MessageData.consignmnetinfo("");
        //        MessageData.FltRoute[] fltRoute = new MessageData.FltRoute[0];
        //        MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
        //        MessageData.dimensionnfo[] dimension = new MessageData.dimensionnfo[0];
        //        string agentcode = "", awbnum = "";
        //        if (ds != null)
        //        {
        //            if (ds.Tables.Count > 0)
        //            {
        //                if (ds.Tables[0].Rows.Count > 0)
        //                {
        //                    DataRow drAWBRateMaster = ds.Tables[1].Rows[0];

        //                    DataRow drAWBSummaryMaster = ds.Tables[0].Rows[0];

        //                    #region PrepareFFRStructureObject

        //                    //line 1 
        //                    objFFRInfo.ffrversionnum = "6";

        //                    #region Consigment Section
        //                    //line 2
        //                    awbnum = drAWBSummaryMaster["AWBNumber"].ToString();
        //                    consigment.airlineprefix = drAWBSummaryMaster["AWBPrefix"].ToString();
        //                    consigment.awbnum = drAWBSummaryMaster["AWBNumber"].ToString();
        //                    consigment.origin = drAWBSummaryMaster["OriginCode"].ToString();
        //                    consigment.dest = drAWBSummaryMaster["DestinationCode"].ToString(); ;
        //                    consigment.consigntype = "T";
        //                    consigment.pcscnt = drAWBRateMaster["Pieces"].ToString();
        //                    consigment.weightcode = drAWBSummaryMaster["UOM"].ToString();
        //                    consigment.weight = drAWBSummaryMaster["GrossWeight"].ToString();
        //                    consigment.volumecode = "";
        //                    consigment.volumeamt = "";
        //                    consigment.densityindicator = "";
        //                    consigment.densitygrp = "";
        //                    consigment.shpdesccode = "";
        //                    consigment.numshp = "";//drAWBRateMaster["Pieces"].ToString();
        //                    try
        //                    {
        //                        consigment.manifestdesc = drAWBRateMaster["CodeDescription"].ToString().Length > 1 ? drAWBRateMaster["CommodityDesc"].ToString().Substring(0, 14) : "";
        //                    }
        //                    catch (Exception ex) { clsLog.WriteLogAzure(ex); }
        //                    //consigment.manifestdesc = drAWBRateMaster["CommodityCode"].ToString().Length > 0 ? drAWBRateMaster["CommodityCode"].ToString() : "GEN";
        //                    consigment.splhandling = "";
        //                    #endregion


        //                    //line 3
        //                    #region FLTROUTE
        //                    if (ds.Tables.Count > 3)
        //                    {
        //                        if (ds.Tables[3].Rows.Count > 0)
        //                        {
        //                            for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
        //                            {
        //                                DataRow drAWBRouteMaster = ds.Tables[3].Rows[i];
        //                                MessageData.FltRoute route = new MessageData.FltRoute("");
        //                                try
        //                                {
        //                                    DateTime dtTo = new DateTime();
        //                                    string dt = (drAWBRouteMaster["FltDate"].ToString());
        //                                    //dt = dt + " " + DateTime.Now.ToShortTimeString();
        //                                    //dtfrom = DateTime.ParseExact(dt,"dd-MM-yyyy",null);
        //                                    dtTo = DateTime.ParseExact(drAWBRouteMaster["FltDate"].ToString(), "dd/MM/yyyy", null);
        //                                    //ToDt = dt.ToString();
        //                                    string day = dt.Substring(0, 2);
        //                                    string mon = dtTo.ToString("MMM");
        //                                    //string mon = dt.Substring(3, 2);
        //                                    string yr = dt.Substring(6, 4);
        //                                    route.date = day.ToString();
        //                                    route.month = mon.ToString();

        //                                }
        //                                catch (Exception ex) { clsLog.WriteLogAzure(ex); }
        //                                route.carriercode = "";
        //                                route.fltnum = drAWBRouteMaster["FltNumber"].ToString();

        //                                route.fltdept = drAWBRouteMaster["FltOrigin"].ToString();
        //                                route.fltarrival = drAWBRouteMaster["FltDestination"].ToString();
        //                                route.spaceallotmentcode = "";
        //                                route.allotidentification = "";
        //                                try
        //                                {
        //                                    string AWBStatus = "";
        //                                    AWBStatus = drAWBRouteMaster["Status"].ToString();
        //                                    if (AWBStatus.Trim() != "")
        //                                    {
        //                                        if (AWBStatus.Trim().Equals("Q", StringComparison.OrdinalIgnoreCase))
        //                                        {
        //                                            route.spaceallotmentcode = "LL";
        //                                        }
        //                                        else if (AWBStatus.Trim().Equals("C", StringComparison.OrdinalIgnoreCase))
        //                                        {
        //                                            route.spaceallotmentcode = "KK";
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        route.spaceallotmentcode = drAWBRouteMaster["Status"].ToString(); ;
        //                                    }
        //                                }
        //                                catch (Exception ex)
        //                                { clsLog.WriteLogAzure(ex); }

        //                                Array.Resize(ref fltRoute, fltRoute.Length + 1);
        //                                fltRoute[fltRoute.Length - 1] = route;
        //                            }
        //                        }
        //                    }
        //                    #endregion

        //                    //line 4
        //                    objFFRInfo.noofuld = "";
        //                    //line 5 
        //                    objFFRInfo.specialservicereq1 = "";
        //                    objFFRInfo.specialservicereq2 = "";
        //                    //line 6
        //                    objFFRInfo.otherserviceinfo1 = "";
        //                    objFFRInfo.otherserviceinfo2 = "";
        //                    //line 7

        //                    objFFRInfo.bookingrefairport = drAWBSummaryMaster["OriginCode"].ToString();
        //                    objFFRInfo.officefundesignation = "FF";
        //                    objFFRInfo.companydesignator = "XX";
        //                    objFFRInfo.bookingfileref = "";
        //                    objFFRInfo.participentidetifier = "";
        //                    objFFRInfo.participentcode = "";
        //                    objFFRInfo.participentairportcity = "";
        //                    // objFFRInfo.participentairportcity = drAWBSummaryMaster["OriginCode"].ToString();
        //                    // objFFRInfo.participentcode = "";
        //                    // objFFRInfo.participentidetifier = "";

        //                    //line 8
        //                    #region Dimension
        //                    //please don't send the dimensions for Auto FFR Setting
        //                    try
        //                    {
        //                        /*
        //                        if (ds.Tables.Count > 2)
        //                        {

        //                            if (ds.Tables[2].Rows.Count > 0)
        //                            {
        //                                for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
        //                                {
        //                                    MessageData.dimensionnfo dim = new MessageData.dimensionnfo("");
        //                                    DataRow drAWBDimensions = ds.Tables[2].Rows[i];
        //                                    dim.weightcode = "";
        //                                    dim.weight = "";
        //                                    dim.mesurunitcode = "";
        //                                    if (drAWBDimensions["MeasureUnit"].ToString().Trim().ToUpper() == "CMS")
        //                                    {
        //                                        dim.mesurunitcode = "CMT";
        //                                    }
        //                                    else if (drAWBDimensions["MeasureUnit"].ToString().Trim().ToUpper() == "INCHES")
        //                                    {
        //                                        dim.mesurunitcode = "INH";
        //                                    }
        //                                    dim.length = drAWBDimensions["Length"].ToString();
        //                                    dim.width = drAWBDimensions["Breadth"].ToString();
        //                                    dim.height = drAWBDimensions["Height"].ToString();
        //                                    dim.piecenum = drAWBDimensions["PcsCount"].ToString();
        //                                    Array.Resize(ref dimension, dimension.Length + 1);
        //                                    dimension[dimension.Length - 1] = dim;

        //                                }
        //                            }
        //                        }*/
        //                    }
        //                    catch (Exception ex) { clsLog.WriteLogAzure(ex); }
        //                    #endregion
        //                    //line 9 
        //                    objFFRInfo.servicecode = "";
        //                    objFFRInfo.rateclasscode = "";
        //                    objFFRInfo.commoditycode = "";
        //                    //line 10    
        //                    try
        //                    {
        //                        if (ds.Tables[6].Rows.Count > 0)
        //                        {
        //                            DataRow drAWBShipperConsigneeDetails = ds.Tables[6].Rows[0];
        //                            objFFRInfo.shipperaccnum = "";//[ShipperAccCode]
        //                            objFFRInfo.shippername = drAWBShipperConsigneeDetails["ShipperName"].ToString();
        //                            objFFRInfo.shipperadd = drAWBShipperConsigneeDetails["ShipperAddress"].ToString() + " " + drAWBShipperConsigneeDetails["ShipperAdd2"].ToString();
        //                            objFFRInfo.shipperplace = drAWBShipperConsigneeDetails["ShipperCity"].ToString();
        //                            objFFRInfo.shipperstate = drAWBShipperConsigneeDetails["ShipperState"].ToString();
        //                            objFFRInfo.shippercountrycode = drAWBShipperConsigneeDetails["ShipperCountry"].ToString().Substring(0, 2);
        //                            objFFRInfo.shipperpostcode = drAWBShipperConsigneeDetails["ShipperPincode"].ToString();
        //                            objFFRInfo.shippercontactidentifier = "TE";
        //                            objFFRInfo.shippercontactnum = drAWBShipperConsigneeDetails["ShipperTelephone"].ToString();

        //                            //line 11
        //                            objFFRInfo.consaccnum = "";//[ConsigAccCode]
        //                            objFFRInfo.consname = drAWBShipperConsigneeDetails["ConsigneeName"].ToString();
        //                            objFFRInfo.consadd = drAWBShipperConsigneeDetails["ConsigneeAddress"].ToString() + " " + drAWBShipperConsigneeDetails["ConsigneeAddress2"].ToString(); ;
        //                            objFFRInfo.consplace = drAWBShipperConsigneeDetails["ConsigneeCity"].ToString();
        //                            objFFRInfo.consstate = drAWBShipperConsigneeDetails["ConsigneeState"].ToString();
        //                            objFFRInfo.conscountrycode = drAWBShipperConsigneeDetails["ConsigneeCountry"].ToString().Substring(0, 2);
        //                            objFFRInfo.conspostcode = drAWBShipperConsigneeDetails["ConsigneePincode"].ToString(); ;
        //                            objFFRInfo.conscontactidentifier = "TE";
        //                            objFFRInfo.conscontactnum = drAWBShipperConsigneeDetails["ConsigneeTelephone"].ToString();
        //                        }
        //                    }
        //                    catch (Exception ex) { clsLog.WriteLogAzure(ex); }
        //                    //line 12
        //                    objFFRInfo.custaccnum = "";
        //                    try
        //                    {
        //                        objFFRInfo.iatacargoagentcode = RemoveSpecialCharacters(drAWBSummaryMaster["AgentCode"].ToString()).Substring(0, 7);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        clsLog.WriteLogAzure(ex);
        //                        objFFRInfo.iatacargoagentcode = "";
        //                    }
        //                    objFFRInfo.cargoagentcasscode = "";
        //                    objFFRInfo.custparticipentidentifier = "";
        //                    objFFRInfo.custname = "";
        //                    objFFRInfo.custplace = "";
        //                    agentcode = drAWBSummaryMaster["AgentCode"].ToString();
        //                    //line 13
        //                    objFFRInfo.shiprefnum = "";
        //                    objFFRInfo.supplemetryshipperinfo1 = "";
        //                    objFFRInfo.supplemetryshipperinfo2 = "";


        //                    #endregion

        //                    string strMsg = cls_Encode_Decode.encodeFFRforsend(ref objFFRInfo, ref objULDInfo, ref consigment, ref fltRoute, ref dimension);
        //                    if (strMsg != null)
        //                    {
        //                        if (strMsg.Trim() != "")
        //                        {
        //                            FTP objFTP = new FTP();
        //                            if (!objFTP.Saveon72FTP(strMsg, awbnum))
        //                            {
        //                                clsLog.WriteLogAzure("Error of AWB upload on FTP:" + awbnum);
        //                            }
        //                            flag = addMsgToOutBox("FFR", strMsg, "swapnil@qidtech.com", "", agentcode, refNO);


        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure("Error in EncodeFFR" + ex.Message);
        //    }
        //    return flag;
        //}

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                // if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')

                if ((c >= '0' && c <= '9') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /*Not in use*/

        //public static DataSet GenertateAWBDimensions(string AWBNumber, int AWBPieces, DataSet Dimensions, decimal AWBWt, string UserName, DateTime TimeStamp, bool IsCreate, string AWBPrefix)
        //{
        //    SQLServer da = new SQLServer();
        //    DataSet ds = null;
        //    try
        //    {
        //        System.Text.StringBuilder strDimensions = new System.Text.StringBuilder();

        //        if (Dimensions != null && Dimensions.Tables.Count > 0 && Dimensions.Tables[0].Rows.Count > 0)
        //        {
        //            for (int intCount = 0; intCount < Dimensions.Tables[0].Rows.Count; intCount++)
        //            {
        //                strDimensions.Append("Insert into #tblPieceInfo(PieceNo, IdentificationNo, Length, Breath, Height, Vol, Wt, Units, PieceType, BagNo, ULDNo, Location, FlightNo, FlightDate) values (");
        //                strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["PieceNo"]);
        //                strDimensions.Append(",'");
        //                strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["IdentificationNo"]);
        //                strDimensions.Append("',");
        //                strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Length"]);
        //                strDimensions.Append(",");
        //                strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Breath"]);
        //                strDimensions.Append(",");
        //                strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Height"]);
        //                strDimensions.Append(",");
        //                strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Vol"]);
        //                strDimensions.Append(",");
        //                strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Wt"]);
        //                strDimensions.Append(",'");
        //                strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Units"]);
        //                strDimensions.Append("','");
        //                strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["PieceType"]);
        //                strDimensions.Append("','");
        //                strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["BagNo"]);
        //                strDimensions.Append("','");
        //                strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["ULDNo"]);
        //                strDimensions.Append("','");
        //                strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Location"]);
        //                strDimensions.Append("','");
        //                strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["FlightNo"]);
        //                strDimensions.Append("','");
        //                strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["FlightDate"]);
        //                strDimensions.Append("'); ");
        //            }
        //        }

        //        string[] PName = new string[] { "AWBNumber", "Pieces", "PieceInfo", "UserName", "TimeStamp", "IsCreate", "AWBWeight", "AWBPrefix" };
        //        object[] PValue = new object[] { AWBNumber, AWBPieces, strDimensions.ToString(), UserName, TimeStamp, IsCreate, AWBWt, AWBPrefix };
        //        SqlDbType[] PType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Bit, SqlDbType.Decimal, SqlDbType.VarChar };
        //        ds = da.SelectRecords("sp_StoreCourierDetails", PName, PValue, PType);
        //        PName = null;
        //        PValue = null;
        //        PType = null;
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //        ds = null;
        //    }
        //    finally
        //    {
        //        da = null;
        //    }
        //    return ds;
        //}

        /*Not in use*/

        //public static string InsertAWBDataInBilling(object[] AWBInfo)
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

        //        string res = da.GetStringByProcedure("SP_InsertAWBDataInBilling_V2", ColumnNames, Values, DataType);
        //        return res;

        //    }

        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //        return "error";
        //    }
        //}

        /*Not in use*/

        //public static string InsertAWBDataInInterlineInvoice(object[] AWBInfo)
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
        //        clsLog.WriteLogAzure(ex);
        //        return "error";
        //    }
        //}

        /*Not in use*/

        //public static string InsertAWBDataInInterlineCreditNote(object[] AWBInfo)
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
        //        clsLog.WriteLogAzure(ex);
        //        return "error";
        //    }
        //}

        /*Not in use*/

        //public static bool PutHAWBDetails(string MAWBNo, string HAWBNo, int HAWBPcs, float HAWBWt, string Description, string CustID, string CustName,
        //                                  string CustAddress, string CustCity, string Zipcode, string Origin, string Destination, string SHC,
        //                                  string HAWBPrefix, string AWBPrefix, string FltOrigin, string FltDest, string ArrivalStatus, string FlightNo,
        //                                  string FlightDt, string ConsigneeName, string ConsigneeAddress, string ConsigneeCity, string ConsigneeState, string ConsigneeCountry, string ConsigneePostalCode,
        //                                  string CustState, string CustCountry, string UOM, string SLAC, string ConsigneeID, string ShipperEmail, string ShipperTelephone, string ConsigneeEmail, string ConsigneeTelephone)
        //{
        //    DataSet ds = new DataSet();
        //    SQLServer da = new SQLServer();

        //    string[] paramname = new string[35];
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
        //    paramname[15] = "ArrivalStatus";
        //    paramname[16] = "FlightNo";
        //    paramname[17] = "FlightDt";
        //    paramname[18] = "FlightOrigin";
        //    paramname[19] = "flightDest";
        //    paramname[20] = "ConsigneeName";
        //    paramname[21] = "ConsigneeAddress";
        //    paramname[22] = "ConsigneeCity";
        //    paramname[23] = "ConsigneeState";
        //    paramname[24] = "ConsigneeCountry";
        //    paramname[25] = "ConsigneePostalCode";
        //    paramname[26] = "CustState";
        //    paramname[27] = "CustCountry";
        //    paramname[28] = "UOM";
        //    paramname[29] = "SLAC";
        //    paramname[30] = "ConsigneeID";
        //    paramname[31] = "ShipperEmail";
        //    paramname[32] = "ShipperTelephone";
        //    paramname[33] = "ConsigneeEmail";
        //    paramname[34] = "ConsigneeTelephone";


        //    object[] paramvalue = new object[35];
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
        //    paramvalue[15] = ArrivalStatus;
        //    paramvalue[16] = FlightNo;
        //    if (FlightDt == "")
        //    {
        //        FlightDt = DateTime.Now.ToString();
        //    }
        //    else
        //    {
        //        paramvalue[17] = FlightDt;
        //    }
        //    paramvalue[18] = FltOrigin;
        //    paramvalue[19] = FltDest;
        //    paramvalue[20] = ConsigneeName;
        //    paramvalue[21] = ConsigneeAddress;
        //    paramvalue[22] = ConsigneeCity;
        //    paramvalue[23] = ConsigneeState;
        //    paramvalue[24] = ConsigneeCountry;
        //    paramvalue[25] = ConsigneePostalCode;
        //    paramvalue[26] = CustState;
        //    paramvalue[27] = CustCountry;
        //    paramvalue[28] = UOM;
        //    paramvalue[29] = SLAC != string.Empty ? SLAC : "0";
        //    paramvalue[30] = ConsigneeID;
        //    paramvalue[31] = ShipperEmail;
        //    paramvalue[32] = ShipperTelephone;
        //    paramvalue[33] = ConsigneeEmail;
        //    paramvalue[34] = ConsigneeTelephone;


        //    SqlDbType[] paramtype = new SqlDbType[35];
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
        //    paramtype[15] = SqlDbType.VarChar;
        //    paramtype[16] = SqlDbType.VarChar;
        //    paramtype[17] = SqlDbType.DateTime;
        //    paramtype[18] = SqlDbType.VarChar;
        //    paramtype[19] = SqlDbType.VarChar;
        //    paramtype[20] = SqlDbType.VarChar;
        //    paramtype[21] = SqlDbType.VarChar;
        //    paramtype[22] = SqlDbType.VarChar;
        //    paramtype[23] = SqlDbType.VarChar;
        //    paramtype[24] = SqlDbType.VarChar;
        //    paramtype[25] = SqlDbType.VarChar;
        //    paramtype[26] = SqlDbType.VarChar;
        //    paramtype[27] = SqlDbType.VarChar;
        //    paramtype[28] = SqlDbType.VarChar;
        //    paramtype[29] = SqlDbType.Int;
        //    paramtype[30] = SqlDbType.VarChar;
        //    paramtype[31] = SqlDbType.VarChar;
        //    paramtype[32] = SqlDbType.VarChar;
        //    paramtype[33] = SqlDbType.VarChar;
        //    paramtype[34] = SqlDbType.VarChar;

        //    if (da.ExecuteProcedure("SP_PutHAWBDetails_V2", paramname, paramtype, paramvalue))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        /*Not in use*/

        //public static void AutoForwardEmail(int refno, string Origin)
        //{
        //    try
        //    {
        //        DataSet ds = new DataSet();
        //        SQLServer da = new SQLServer();
        //        string[] PName = new string[] { "Origin", "InboxID" };
        //        SqlDbType[] PType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.Int };
        //        object[] PValue = new object[] { Origin, refno };
        //        da.ExecuteProcedure("spAutoForwardMsgs", PName, PType, PValue);
        //    }
        //    catch (Exception ex) { clsLog.WriteLogAzure(ex); }
        //}

        /*Not in use*/

        //public static bool ValidateAndSaveFSBMessage(MessageData.FSBAWBInformation fsbMessage, MessageData.ShipperInformation fsbShipper, MessageData.ConsigneeInformation fsbConsignee, List<MessageData.RouteInformation> RouteIformation, List<MessageData.FSBDimensionInformation> Dimensionformation, List<MessageData.AWBBUPInformation> bublistinformation)
        //{

        //    SQLServer dtb = new SQLServer();
        //    bool MessageStatus = false;
        //    try
        //    {

        //        string AWbNo = string.Empty, AWBPrefix = string.Empty, strNatureofGoods = string.Empty;
        //        decimal VolumeWt = 0;
        //        string flightdate = System.DateTime.Now.ToString("dd/MM/yyyy");
        //        AWbNo = fsbMessage.AWBNo;
        //        AWBPrefix = fsbMessage.AirlinePrefix;
        //        bool saveStatus = false;

        //        if (fsbMessage.VolumeCode != null)
        //        {
        //            switch (fsbMessage.VolumeCode)
        //            {
        //                case "MC":
        //                    VolumeWt = decimal.Parse(String.Format("{0:0.00}", Convert.ToDecimal(Convert.ToDecimal(fsbMessage.AWBVolume.ToString() == "" ? "0" : fsbMessage.AWBVolume.ToString()) * decimal.Parse("166.66"))));
        //                    break;
        //                default:
        //                    VolumeWt = fsbMessage.AWBVolume;
        //                    break;
        //            }
        //        }
        //        if (fsbMessage.NatureofGoods1 != null)
        //            strNatureofGoods = fsbMessage.NatureofGoods1;
        //        else if (fsbMessage.NatureofGoods2 != null)
        //        {
        //            if (strNatureofGoods != "")
        //                strNatureofGoods += "," + fsbMessage.NatureofGoods2;
        //            else
        //                strNatureofGoods = fsbMessage.NatureofGoods2;
        //        }


        //        string[] paramname = new string[] { "AirlinePrefix", "AWBNum", "Origin", "Dest", "PcsCount", "Weight", "Volume", "ComodityCode", "ComodityDesc", "CarrierCode", "FlightNum", "FlightDate", "FlightOrigin", "FlightDest",
        //                                                "ShipperName", "ShipperAddr", "ShipperPlace", "ShipperState", "ShipperCountryCode", "ShipperContactNo", "ConsName", "ConsAddr", "ConsPlace",
        //                                                "ConsState", "ConsCountryCode", "ConsContactNo", "CustAccNo", "IATACargoAgentCode", "CustName", "SystemDate", "MeasureUnit", "Length", "Breadth", "Height", "PartnerStatus","REFNo",
        //                                            "UpdatedBy","SpecialHandelingCode","Paymode","ShipperPincode","ConsingneePinCode"};

        //        object[] paramvalue = new object[] {AWBPrefix,AWbNo,fsbMessage.AWBOrigin, fsbMessage.AWBDestination,fsbMessage.TotalAWbPiececs, fsbMessage.GrossWeight,VolumeWt, "",strNatureofGoods,"","",flightdate, "","",
        //            fsbShipper.ShipperName==null?"":fsbShipper.ShipperName,fsbShipper.ShipperStreetAddress==null?"":fsbShipper.ShipperStreetAddress, fsbShipper.ShipperPlace==null?"":fsbShipper.ShipperPlace, fsbShipper.ShipperState==null?"":fsbShipper.ShipperState, fsbShipper.ShipperCountrycode==null?"":fsbShipper.ShipperCountrycode,  fsbShipper.ShipperContactNumber==null?"":fsbShipper.ShipperContactNumber, fsbConsignee.ConsigneeName==null?"":fsbConsignee.ConsigneeName, fsbConsignee.ConsigneeStreetAddress==null?"":fsbConsignee.ConsigneeStreetAddress, fsbConsignee.ConsigneePlace==null?"":fsbConsignee.ConsigneePlace,
        //            fsbConsignee.ConsigneeState==null?"":fsbConsignee.ConsigneeState,fsbConsignee.ConsigneeCountrycode==null?"":fsbConsignee.ConsigneeCountrycode,fsbConsignee.ConsigneeContactNumber==null?"":fsbConsignee.ConsigneeContactNumber, fsbConsignee.ConsigneeAccountNo==null?"":fsbConsignee.ConsigneeAccountNo, "","", System.DateTime.Now.ToString("yyyy-MM-dd"),"", "", "", "", "",0,
        //                                            "FSB","","",fsbShipper.ShipperPostalCode==null?"":fsbShipper.ShipperPostalCode,fsbConsignee.ConsigneePostalCode==null?"":fsbConsignee.ConsigneePostalCode};

        //        SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
        //                                                      SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
        //                                                      SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.Int,
        //                                                    SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar};





        //        saveStatus = dtb.InsertData("spInsertBookingDataFromFFR", paramname, paramtype, paramvalue);
        //        if (saveStatus)
        //        {

        //            #region Save AWB Routing
        //            try
        //            {
        //                if (RouteIformation.Count > 0)
        //                {
        //                    if (dtb.ExecuteProcedure("spDeleteAWBRouteFFR", "AWBNum", SqlDbType.VarChar, AWbNo))
        //                    {
        //                        #region route insert Loop
        //                        for (int route = 0; route < RouteIformation.Count; route++)
        //                        {

        //                            string[] paramNames = new string[]
        //                            {   "AWBNumber", "FltOrigin", "FltDestination", "FltNumber",
        //                                "FltDate",
        //                                "Status",
        //                                "UpdatedBy",
        //                                "UpdatedOn",
        //                                "IsFFR",
        //                                "REFNo",
        //                                "date",
        //                                "AWBPrefix"
        //                            };
        //                            SqlDbType[] dataTypes = new SqlDbType[]
        //                            {   SqlDbType.VarChar,
        //                                SqlDbType.VarChar,
        //                                SqlDbType.VarChar,
        //                                SqlDbType.VarChar,
        //                                SqlDbType.DateTime,
        //                                SqlDbType.VarChar,
        //                                SqlDbType.VarChar,
        //                                SqlDbType.DateTime,
        //                                SqlDbType.Bit,
        //                                SqlDbType.Int,
        //                                SqlDbType.DateTime,
        //                                SqlDbType.VarChar
        //                            };

        //                            object[] values = new object[]
        //                            {
        //                                AWbNo,
        //                                RouteIformation[route].FlightOrigin,
        //                                RouteIformation[route].FlightDestination,
        //                                RouteIformation[route].Carriercode ,
        //                                DateTime.Now,
        //                                "C",
        //                                "FSB",
        //                                System.DateTime.Now,
        //                                1,
        //                                0,
        //                                DateTime.Now,
        //                               AWBPrefix

        //                            };
        //                            if (!dtb.UpdateData("spSaveFFRAWBRoute", paramNames, dataTypes, values))
        //                            {
        //                                clsLog.WriteLogAzure("Error in Save AWB Route FFR " + dtb.LastErrorDescription);
        //                            }
        //                        }

        //                        #region Deleting AWB Data if No Route Present
        //                        try
        //                        {
        //                            string[] QueryNames = { "AWBPrefix", "AWBNumber" };
        //                            SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar };
        //                            object[] QueryValues = { AWBPrefix, AWbNo };
        //                            if (!dtb.UpdateData("spDeleteAWBDetailsNoRoute", QueryNames, QueryTypes, QueryValues))
        //                            {
        //                                clsLog.WriteLogAzure("Error in Deleting AWB Details " + dtb.LastErrorDescription);
        //                            }

        //                        }
        //                        catch (Exception ex)
        //                        { clsLog.WriteLogAzure(ex); }
        //                        #endregion

        //                        #endregion

        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                clsLog.WriteLogAzure("Error in Save AWB Route FFR:" + ex.Message);
        //            }
        //            #endregion Save AWB Routing

        //            #region AWB Dimensions
        //            try
        //            {

        //                if (Dimensionformation.Count > 0)
        //                {
        //                    //Badiuz khan
        //                    //Description: Delete Dimension if Dimension 
        //                    string[] dparam = { "AWBPrefix", "AWBNumber" };
        //                    SqlDbType[] dbparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar };
        //                    object[] dbparamvalues = { AWBPrefix, AWbNo };

        //                    if (!dtb.InsertData("SpDeleteDimensionThroughMessage", dparam, dbparamtypes, dbparamvalues))
        //                        clsLog.WriteLogAzure("Error  Delete Dimension Through Message :" + AWbNo);
        //                    else
        //                    {

        //                        for (int i = 0; i < Dimensionformation.Count; i++)
        //                        {
        //                            string DimunitCode = string.Empty;
        //                            if (Dimensionformation[i].DimUnitCode.Trim() != "")
        //                            {
        //                                if (Dimensionformation[i].DimUnitCode.Trim().ToUpper() == "CMT")
        //                                    DimunitCode = "CMS";
        //                                else
        //                                    DimunitCode = "INCHES";
        //                            }
        //                            string[] param = { "AWBNumber", "RowIndex", "Length", "Breadth", "Height", "PcsCount", "MeasureUnit", "AWBPrefix", "Weight" };
        //                            SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Decimal };


        //                            object[] value ={AWbNo,"1",Dimensionformation[i].DimLength,Dimensionformation[i].DimWidth,Dimensionformation[i].DimHeight,
        //                                    Dimensionformation[i].DimPieces,DimunitCode,AWBPrefix,Dimensionformation[i].DimGrossWeight};

        //                            if (!dtb.InsertData("SP_SaveAWBDimensions_FFR", param, dbtypes, value))
        //                            {
        //                                clsLog.WriteLogAzure("Error Saving  Dimension Through Message :" + AWbNo);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception ex) { clsLog.WriteLogAzure(ex); }
        //            #endregion

        //            #region FWB Message with BUP Shipment
        //            //Badiuz khan
        //            //Description: Save Bup through FWB

        //            if (bublistinformation.Count > 0)
        //            {

        //                int uldslacPcs = 0;
        //                for (int k = 0; k < bublistinformation.Count; k++)
        //                {
        //                    if (bublistinformation[k].ULDNo.Length == 10)
        //                    {
        //                        string UldType = bublistinformation[k].ULDNo.Substring(0, 3);
        //                        string uldno = bublistinformation[k].ULDNo;
        //                        string uldOwnerCode = bublistinformation[k].ULDNo.Substring(8, 2);
        //                        if (bublistinformation[k].SlacCount != "")
        //                            uldslacPcs = int.Parse(bublistinformation[k].SlacCount);
        //                        string[] param = { "AWBPrefix", "AWBNumber", "UldType", "ULDNo", "ULdOwnerCode", "SlacPcs", "PcsCount", "Volume", "GrossWeight" };
        //                        SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.Decimal, SqlDbType.Decimal };
        //                        object[] value = { AWBPrefix, AWbNo, UldType, uldno, uldOwnerCode, uldslacPcs, fsbMessage.TotalAWbPiececs, VolumeWt, fsbMessage.GrossWeight };

        //                        if (!dtb.InsertData("SaveandUpdateShippperBUPThroughFWB", param, dbtypes, value))
        //                        {
        //                            clsLog.WriteLogAzure("BUP ULD is not Updated  for:" + AWbNo + Environment.NewLine + "Error : " + dtb.LastErrorDescription);
        //                        }
        //                    }
        //                }
        //            }

        //            #endregion
        //        }
        //        clsLog.WriteLogAzure("FSB Message Processing for " + AWbNo);
        //        MessageStatus = true;

        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure("Error on FSB Message Processing " + ex.ToString());
        //        MessageStatus = false;
        //    }
        //    return MessageStatus;
        //}

        public string ReplacingNodeNames(string xmlMsg, ref string xmlMessageName)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlMsg);
            XmlNode nodeToFind;
            XmlElement root = doc.DocumentElement;
            xmlMessageName = root.Name;
            XmlNodeList xmlNodelst;
            if (xmlMessageName.Equals("rsm:FlightManifest"))
            {
                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:IncludedCustomsNote')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("ram:IncludedMasterConsignment"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:IncludedCustomsNote1");
                    }
                }
                xmlMsg = doc.OuterXml;
                xmlMsg = xmlMsg.Replace("IncludedCustomsNote1", "ram:IncludedCustomsNote1");
            }
            else if (xmlMessageName.Equals("rsm:HouseWaybill"))
            {
                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:OriginLocation')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("rsm:MasterConsignment"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:MasterConsignment_OriginLocation");
                    }
                }

                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:FinalDestinationLocation')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("rsm:MasterConsignment"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:MasterConsignment_FinalDestinationLocation");
                    }
                }

                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:IncludedTareGrossWeightMeasure')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("ram:IncludedHouseConsignment"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:IncludedHouseConsignment_IncludedTareGrossWeightMeasure");
                    }
                }


                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:PostalStructuredAddress')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("ram:ConsignorParty"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:ConsignorParty_PostalStructuredAddress");
                    }
                    else if (xmlNode.ParentNode.Name.Equals("ram:ConsigneeParty"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:ConsigneeParty_PostalStructuredAddress");
                    }
                    else if (xmlNode.ParentNode.Name.Equals("ram:FreightForwarderParty"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:FreightForwarderParty_PostalStructuredAddress");
                    }
                    else if (xmlNode.ParentNode.Name.Equals("ram:AssociatedParty"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:AssociatedParty_PostalStructuredAddress");
                    }
                }

                xmlMsg = doc.OuterXml;
                xmlMsg = xmlMsg.Replace("MasterConsignment_OriginLocation", "ram:MasterConsignment_OriginLocation");
                xmlMsg = xmlMsg.Replace("MasterConsignment_FinalDestinationLocation", "ram:MasterConsignment_FinalDestinationLocation");
                xmlMsg = xmlMsg.Replace("IncludedHouseConsignment_IncludedTareGrossWeightMeasure", "ram:IncludedHouseConsignment_IncludedTareGrossWeightMeasure");
                xmlMsg = xmlMsg.Replace("ConsignorParty_PostalStructuredAddress", "ram:ConsignorParty_PostalStructuredAddress");
                xmlMsg = xmlMsg.Replace("ConsigneeParty_PostalStructuredAddress", "ram:ConsigneeParty_PostalStructuredAddress");
                xmlMsg = xmlMsg.Replace("FreightForwarderParty_PostalStructuredAddress", "ram:FreightForwarderParty_PostalStructuredAddress");
                xmlMsg = xmlMsg.Replace("AssociatedParty_PostalStructuredAddress", "ram:AssociatedParty_PostalStructuredAddress");
            }
            else if (xmlMessageName.Equals("rsm:Waybill"))
            {
                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:PostalStructuredAddress')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("ram:ConsignorParty"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:ConsignorParty_PostalStructuredAddress");
                    }
                    else if (xmlNode.ParentNode.Name.Equals("ram:ConsigneeParty"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:ConsigneeParty_PostalStructuredAddress");
                    }
                    else if (xmlNode.ParentNode.Name.Equals("ram:AssociatedParty"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:AssociatedParty_PostalStructuredAddress");
                    }
                }

                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:GrossVolumeMeasure')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("ram:IncludedMasterConsignmentItem"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:IncludedMasterConsignmentItem_GrossVolumeMeasure");
                    }
                }
                xmlMsg = doc.OuterXml;
                xmlMsg = xmlMsg.Replace("ConsignorParty_PostalStructuredAddress", "ram:ConsignorParty_PostalStructuredAddress");
                xmlMsg = xmlMsg.Replace("ConsigneeParty_PostalStructuredAddress", "ram:ConsigneeParty_PostalStructuredAddress");
                xmlMsg = xmlMsg.Replace("AssociatedParty_PostalStructuredAddress", "ram:AssociatedParty_PostalStructuredAddress");
                xmlMsg = xmlMsg.Replace("IncludedMasterConsignmentItem_GrossVolumeMeasure", "ram:IncludedMasterConsignmentItem_GrossVolumeMeasure");
            }
            else if (xmlMessageName.Equals("rsm:BookingRequest"))
            {
                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:PostalStructuredAddress')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("ram:ConsignorParty"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:ConsignorParty_PostalStructuredAddress");
                    }
                    else if (xmlNode.ParentNode.Name.Equals("ram:ConsigneeParty"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:ConsigneeParty_PostalStructuredAddress");
                    }
                    else if (xmlNode.ParentNode.Name.Equals("ram:RequestorParty"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:RequestorParty_PostalStructuredAddress");
                    }
                    else if (xmlNode.ParentNode.Name.Equals("ram:AssociatedParty"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:AssociatedParty_PostalStructuredAddress");
                    }
                }

                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:GrossWeightMeasure')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("ram:IncludedMasterConsignmentItem"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:IncludedMasterConsignmentItem_GrossWeightMeasure");
                    }
                    else if (xmlNode.ParentNode.Name.Equals("ram:AssociatedUnitLoadTransportEquipment"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:AssociatedUnitLoadTransportEquipment_GrossWeightMeasure");
                    }
                    else if (xmlNode.ParentNode.Name.Equals("ram:TransportLogisticsPackage"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:TransportLogisticsPackage_GrossWeightMeasure");
                    }
                }
                xmlMsg = doc.OuterXml;
                xmlMsg = xmlMsg.Replace("ConsignorParty_PostalStructuredAddress", "ram:ConsignorParty_PostalStructuredAddress");
                xmlMsg = xmlMsg.Replace("ConsigneeParty_PostalStructuredAddress", "ram:ConsigneeParty_PostalStructuredAddress");
                xmlMsg = xmlMsg.Replace("RequestorParty_PostalStructuredAddress", "ram:RequestorParty_PostalStructuredAddress");
                xmlMsg = xmlMsg.Replace("AssociatedParty_PostalStructuredAddress", "ram:AssociatedParty_PostalStructuredAddress");
                xmlMsg = xmlMsg.Replace("IncludedMasterConsignmentItem_GrossWeightMeasure", "ram:IncludedMasterConsignmentItem_GrossWeightMeasure");
                xmlMsg = xmlMsg.Replace("AssociatedUnitLoadTransportEquipment_GrossWeightMeasure", "ram:AssociatedUnitLoadTransportEquipment_GrossWeightMeasure");
                xmlMsg = xmlMsg.Replace("TransportLogisticsPackage_GrossWeightMeasure", "ram:TransportLogisticsPackage_GrossWeightMeasure");
            }
            else if (root.Name.Equals("rsm:HouseManifest"))
            {
                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:TransportContractDocument')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("rsm:MasterConsignment"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:MasterConsignment_TransportContractDocument");
                    }
                }

                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:OriginLocation')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("rsm:MasterConsignment"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:MasterConsignment_OriginLocation");
                    }
                }

                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:FinalDestinationLocation')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("rsm:MasterConsignment"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:MasterConsignment_FinalDestinationLocation");
                    }
                }

                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:IncludedCustomsNote')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("rsm:MasterConsignment"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:MasterConsignment_IncludedCustomsNote");
                    }
                }

                xmlMsg = doc.OuterXml;
                xmlMsg = xmlMsg.Replace("MasterConsignment_TransportContractDocument", "ram:MasterConsignment_TransportContractDocument");
                xmlMsg = xmlMsg.Replace("MasterConsignment_OriginLocation", "ram:MasterConsignment_OriginLocation");
                xmlMsg = xmlMsg.Replace("MasterConsignment_FinalDestinationLocation", "ram:MasterConsignment_FinalDestinationLocation");
                xmlMsg = xmlMsg.Replace("MasterConsignment_IncludedCustomsNote", "ram:MasterConsignment_IncludedCustomsNote");

            }
            else if (root.Name.Equals("rsm:FreightBookedList"))
            {
                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:IncludedCustomsNote')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("rsm:LogisticsTransportMovement"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:LogisticsTransportMovement_IncludedCustomsNote");
                    }
                }

                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:GrossWeightMeasure')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("ram:TransportLogisticsPackage"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:TransportLogisticsPackage_GrossWeightMeasure");
                    }
                    else if (xmlNode.ParentNode.Name.Equals("ram:UtilizedUnitLoadTransportEquipment"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:UtilizedUnitLoadTransportEquipment_GrossWeightMeasure");
                    }
                }

                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:PrimaryID')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("ram:OperatingParty"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:OperatingParty_PrimaryID");
                    }
                }

                xmlMsg = doc.OuterXml;
                xmlMsg = xmlMsg.Replace("LogisticsTransportMovement_IncludedCustomsNote", "ram:LogisticsTransportMovement_IncludedCustomsNote");
                xmlMsg = xmlMsg.Replace("TransportLogisticsPackage_GrossWeightMeasure", "ram:TransportLogisticsPackage_GrossWeightMeasure");
                xmlMsg = xmlMsg.Replace("UtilizedUnitLoadTransportEquipment_GrossWeightMeasure", "ram:UtilizedUnitLoadTransportEquipment_GrossWeightMeasure");
                xmlMsg = xmlMsg.Replace("OperatingParty_PrimaryID", "ram:OperatingParty_PrimaryID");
            }
            else if (root.Name.Equals("rsm:StatusMessage"))
            {
                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:GrossWeightMeasure')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("rsm:MasterConsignment"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:MasterConsignment_GrossWeightMeasure");
                    }
                    else if (xmlNode.ParentNode.Name.Equals("ram:AssociatedStatusConsignment"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:AssociatedStatusConsignment_GrossWeightMeasure");
                    }
                    else if (xmlNode.ParentNode.Name.Equals("ram:IncludedHouseConsignment"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:IncludedHouseConsignment_GrossWeightMeasure");
                    }
                }

                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:TransportContractDocument')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("ram:IncludedHouseConsignment"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:IncludedHouseConsignment_TransportContractDocument");
                    }
                }

                xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:TotalGrossWeightMeasure')]");
                foreach (XmlNode xmlNode in xmlNodelst)
                {
                    if (xmlNode.ParentNode.Name.Equals("ram:IncludedHouseConsignment"))
                    {
                        nodeToFind = RenameNode(xmlNode, "ram:IncludedHouseConsignment_TotalGrossWeightMeasure");
                    }
                }

                xmlMsg = doc.OuterXml;
                xmlMsg = xmlMsg.Replace("MasterConsignment_GrossWeightMeasure", "ram:MasterConsignment_GrossWeightMeasure");
                xmlMsg = xmlMsg.Replace("AssociatedStatusConsignment_GrossWeightMeasure", "ram:AssociatedStatusConsignment_GrossWeightMeasure");
                xmlMsg = xmlMsg.Replace("IncludedHouseConsignment_GrossWeightMeasure", "ram:IncludedHouseConsignment_GrossWeightMeasure");
                xmlMsg = xmlMsg.Replace("IncludedHouseConsignment_TransportContractDocument", "ram:IncludedHouseConsignment_TransportContractDocument");
                xmlMsg = xmlMsg.Replace("IncludedHouseConsignment_TotalGrossWeightMeasure", "ram:IncludedHouseConsignment_TotalGrossWeightMeasure");
            }
            return xmlMsg;
        }

        #endregion Public Methods

        /*Not in use*/

        //internal void StoreXMLMessage(string strMessage, string fileName)
        //{
        //    try
        //    {
        //        DataSet dsFlightXMLData = new DataSet();
        //        var xmlText = new StringReader(strMessage);
        //        dsFlightXMLData.ReadXml(xmlText);
        //        if (dsFlightXMLData != null && dsFlightXMLData.Tables.Count > 0)
        //        {
        //            if (dsFlightXMLData.Tables.Contains("FLIGHT"))
        //            {
        //                string flightNumber = string.Empty, flightAirlineCode = string.Empty, fltDay = string.Empty, fltMonth = string.Empty, fltYear = string.Empty, registrationNumber = string.Empty, origin = string.Empty, destination = string.Empty, payload = string.Empty, cargoCapacity = string.Empty;
        //                DateTime flightDate = new DateTime(2001, 1, 1);
        //                int flight_id = 0;
        //                for (int i = 0; i < dsFlightXMLData.Tables["FLIGHT"].Rows.Count; i++)
        //                {
        //                    flight_id = Convert.ToInt32(dsFlightXMLData.Tables["FLIGHT"].Rows[i]["FLIGHT_id"].ToString());
        //                    if (dsFlightXMLData.Tables.Contains("FltNum") && dsFlightXMLData.Tables.Contains("FltAlc"))
        //                    {
        //                        flightNumber = dsFlightXMLData.Tables["FltNum"].Select("FLIGHT_id=" + flight_id)[0]["FltNum_Text"].ToString();
        //                        flightAirlineCode = dsFlightXMLData.Tables["FltAlc"].Select("FLIGHT_id=" + flight_id)[0]["FltAlc_Text"].ToString();
        //                    }
        //                    if (dsFlightXMLData.Tables.Contains("FltDay") && dsFlightXMLData.Tables.Contains("FltMonth") && dsFlightXMLData.Tables.Contains("FltYear"))
        //                    {
        //                        fltDay = dsFlightXMLData.Tables["FltDay"].Select("FLIGHT_id=" + flight_id)[0]["FltDay_Text"].ToString();
        //                        fltMonth = dsFlightXMLData.Tables["FltMonth"].Select("FLIGHT_id=" + flight_id)[0]["FltMonth_Text"].ToString();
        //                        fltYear = dsFlightXMLData.Tables["FltYear"].Select("FLIGHT_id=" + flight_id)[0]["FltYear_Text"].ToString();
        //                        flightDate = new DateTime(Convert.ToInt32(fltYear), Convert.ToInt32(fltMonth), Convert.ToInt32(fltDay));
        //                    }
        //                    if (dsFlightXMLData.Tables.Contains("OrigIata"))
        //                    {
        //                        origin = dsFlightXMLData.Tables["OrigIata"].Select("FLIGHT_id=" + flight_id)[0]["OrigIata_Text"].ToString();
        //                    }
        //                    if (dsFlightXMLData.Tables.Contains("DestIata"))
        //                    {
        //                        destination = dsFlightXMLData.Tables["DestIata"].Select("FLIGHT_id=" + flight_id)[0]["DestIata_Text"].ToString();
        //                    }
        //                    if (dsFlightXMLData.Tables.Contains("RegistNum"))
        //                    {
        //                        registrationNumber = dsFlightXMLData.Tables["RegistNum"].Select("FLIGHT_id=" + flight_id)[0]["RegistNum_Text"].ToString();
        //                    }

        //                    if (dsFlightXMLData.Tables.Contains("Cargo"))
        //                    {
        //                        cargoCapacity = dsFlightXMLData.Tables["Cargo"].Select("FLIGHT_id=" + flight_id)[0]["Cargo_Text"].ToString();
        //                    }

        //                    StringBuilder msgBody = new StringBuilder();
        //                    msgBody.Append("Flight Number: " + flightNumber + "\r\n");
        //                    msgBody.Append("flight Airline Code: " + flightAirlineCode + "\r\n");
        //                    msgBody.Append("flightDate: " + flightDate.ToShortDateString() + "\r\n");
        //                    msgBody.Append("origin: " + origin + "\r\n");
        //                    msgBody.Append("destination: " + destination + "\r\n");
        //                    msgBody.Append("registrationNumber: " + registrationNumber + "\r\n");
        //                    msgBody.Append("cargoCapacity: " + cargoCapacity + "\r\n");

        //                    string[] paramname = new string[] {
        //                        "FlightNumber"
        //                        , "FlightAirlineCode"
        //                        , "FlightDate"
        //                        , "Origin"
        //                        , "Destination"
        //                        , "RegistrationNumber"
        //                        , "cargoCapacity"
        //                        , "MsgBody"
        //                        , "FileName"
        //                    };

        //                    object[] paramvalue = new object[] {
        //                        flightNumber
        //                        , flightAirlineCode
        //                        , flightDate
        //                        , origin
        //                        , destination
        //                        , registrationNumber
        //                        , cargoCapacity.Trim() == string.Empty ? 0 : Convert.ToDecimal(cargoCapacity)
        //                        , msgBody.ToString()
        //                        , fileName
        //                    };

        //                    SqlDbType[] paramtype = new SqlDbType[] {
        //                        SqlDbType.VarChar
        //                        , SqlDbType.VarChar
        //                        , SqlDbType.DateTime
        //                        , SqlDbType.VarChar
        //                        , SqlDbType.VarChar
        //                        , SqlDbType.VarChar
        //                        , SqlDbType.Decimal
        //                        , SqlDbType.VarChar
        //                        , SqlDbType.VarChar
        //                    };
        //                    SQLServer sqlServer = new SQLServer();
        //                    DataSet dsResult = new DataSet();
        //                    dsResult = sqlServer.SelectRecords("Messaging.uspCargoLoadXML", paramname, paramvalue, paramtype);
        //                }
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //    }
        //}

        public async Task<DataSet?> GetChildHAWB(string awbPrefix, string awbNumber, string flightNo, DateTime flightDate, bool manifestedHawb, string POL = "", bool IsXFZB = false)
        {
            try
            {
                //SQLServer _db = new SQLServer();
                //string[] queryName = { "AWBPrefix", "AWBNumber", "FlightNo", "FlightDate", "ManifestedHAWB", "POL", "IsXFZB" };
                //SqlDbType[] queryType = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.Bit };
                //object[] queryValue = { awbPrefix, awbNumber, flightNo, flightDate, manifestedHawb, POL, IsXFZB };
                //DataSet ds = _db.SelectRecords("spGetChildHAWB_Manifest", queryName, queryValue, queryType);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = awbPrefix },
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbNumber },
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = flightNo },
                    new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = flightDate },
                    new SqlParameter("@ManifestedHAWB", SqlDbType.Bit) { Value = manifestedHawb },
                    new SqlParameter("@POL", SqlDbType.VarChar) { Value = POL },
                    new SqlParameter("@IsXFZB", SqlDbType.Bit) { Value = IsXFZB }
                };

                DataSet? ds = await _readWriteDao.SelectRecords("spGetChildHAWB_Manifest", parameters);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return ds;
                }
                return null;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return null;
            }
        }

        //private bool ValidateAndSaveCSNMessage(int refNO, ref MessageData.CSNInfo csnInfo, ref MessageData.customsextrainfo[] custominfo, string strMessageFrom, string strFromID, DataTable dtOCIInfo)
        private async Task<(bool Success, MessageData.CSNInfo csnInfo, MessageData.customsextrainfo[] custominfo)> ValidateAndSaveCSNMessage(int refNO, MessageData.CSNInfo csnInfo, MessageData.customsextrainfo[] custominfo, string strMessageFrom, string strFromID, DataTable dtOCIInfo)
        {
            bool flag = false;
            //GenericFunction genericFunction = new GenericFunction();
            string flightDate = string.Empty;
            flightDate = csnInfo.flightMonth.PadLeft(2, '0') + "/" + csnInfo.flightDay.PadLeft(2, '0') + "/" + System.DateTime.Now.Year.ToString();

            try
            {
                _genericFunction.UpdateInboxFromMessageParameter(refNO, csnInfo.awbPrefix + "-" + csnInfo.awbNumber, csnInfo.flightNumber, csnInfo.pol, csnInfo.pou, "CSN", strMessageFrom == "" ? strFromID : strMessageFrom, DateTime.Parse(flightDate));

                SqlParameter[] sqlParameter = [
                    new SqlParameter("VersionNumber", csnInfo.versionNumber)
                    , new SqlParameter("AwbPrefix", csnInfo.awbPrefix)
                    , new SqlParameter("AWBNumber", csnInfo.awbNumber)
                    , new SqlParameter("IsMaster", csnInfo.IsMaster)
                    , new SqlParameter("IsHouse", csnInfo.ISHouse)
                    , new SqlParameter("FlightNumber", csnInfo.flightNumber)
                    , new SqlParameter("FlightDay", csnInfo.flightDay)
                    , new SqlParameter("FlightMonth", csnInfo.flightMonth)
                    , new SqlParameter("POL", csnInfo.pol)
                    , new SqlParameter("POU", csnInfo.pou)
                    , new SqlParameter("CustomStatusCode", csnInfo.customStatusCode)
                    , new SqlParameter("CustomsNotification", csnInfo.customsNotification)
                    , new SqlParameter("CustomsActionCode", csnInfo.customsActionCode)
                    , new SqlParameter("NotificationDay", csnInfo.notificationDay)
                    , new SqlParameter("NotificationMonth", csnInfo.notificationMonth)
                    , new SqlParameter("NotificationTime", csnInfo.notificationTime)
                    , new SqlParameter("CustomsEntryNumber", csnInfo.customsEntryNumber)
                    , new SqlParameter("NumberOfPieces", csnInfo.numberOfPieces)
                    , new SqlParameter("OCIInfoType", dtOCIInfo)
                ];

                //SQLServer sqlServer = new SQLServer();
                //sqlServer.SelectRecords("Messaging.uspSaveCSNMessage", sqlParameter);

                await _readWriteDao.SelectRecords("Messaging.uspSaveCSNMessage", sqlParameter);
                flag = true;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                flag = false;
            }

            //return flag;

            // Return success flag + the (potentially unchanged) objects
            return (flag, csnInfo, custominfo);
        }





    }
}
