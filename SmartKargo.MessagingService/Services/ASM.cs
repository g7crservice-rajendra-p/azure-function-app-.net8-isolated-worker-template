using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;

namespace QidWorkerRole
{
    public class ASM
    {
        private readonly ILogger<ASM> _logger;//instance logger
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly GenericFunction _genericFunction;

        #region Constructor
        public ASM(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<ASM>? staticLogger,
            ILogger<ASM> logger,
            GenericFunction genericFunction,
            ILoggerFactory loggerFactory)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
        }
        #endregion Constructor

        public string messageType = "MVT";
        public string messageIdentifier = string.Empty, messageTimeMode = string.Empty;
        public string originalMessage = string.Empty;
        public string airportOfArrival = string.Empty;
        public string airportOfDepart = string.Empty;
        public string aircraftType = string.Empty;
        public string passangerReservation = string.Empty;
        public string flightNo = string.Empty, onwordFlight = string.Empty;
        public string schedDateOfDepart = string.Empty;
        public string schedDateOfArrival = string.Empty;
        public string schedTimeOfDepart = string.Empty;
        public string schedTimeOfArrival = string.Empty;
        public string registrationNo = string.Empty;
        public string serviceType = string.Empty;
        public int dateVariationDep = 0;
        public int dateVariationArr = 0;
        public int legNumber = 0;
        public string[] arrNEW = new string[] { "NEW", "NEW OPER" };
        public string[] arrCNL = new string[] { "CNL", "CNL CREW" };
        public string[] arrRIN = new string[] { "RIN", "RIN COMM" };
        public string[] arrRPL = new string[] { "RPL", "RPL WEAT", "RPL/ADM", "RPL/ADM/CON/EQT/TIM" };
        public string[] arrADM = new string[] { "ADM", "ADM COMM" };
        public string[] arrEQT = new string[] { "EQT", "EQT TECH", "CON", "CON EQUI", "EQT/ADM", "EQT/ADM/CON" };
        public string[] arrRRT = new string[] { "RRT", "RRT OPER", "RRT/TIM", "RRT/ADM/TIM", "RPL/RRT/TIM", "RPL/ADM/RRT/TIM" };
        public string[] arrTIM = new string[] { "TIM", "TIM/ADM", "TIM COMM" };

        //public void ToASM(string strMessage, int srno, string strOriginalMessage, string strMessageFrom, out bool isProcessFlag)
        public async Task<bool> ToASM(string strMessage, int srno, string strOriginalMessage, string strMessageFrom)
        {
            bool isProcessFlag = false;
            try
            {
                string[] arrLine;
                arrLine = Array.ConvertAll(strMessage.Split('$'), p => p.Trim());
                originalMessage = strMessage.Replace("$", "\r\n"); ;
                if (arrLine.Length < 3)
                {
                    //GenericFunction genericFunction = new GenericFunction();
                    //genericFunction.UpdateErrorMessageToInbox(srno, "Invalid format", "ASM");
                    await _genericFunction.UpdateErrorMessageToInbox(srno, "Invalid format", "ASM");
                    return isProcessFlag;
                }
                messageType = arrLine[0];
                if (arrLine.Length > 0 && arrLine.Intersect(arrNEW).Any())
                {
                    await parseNEW(arrLine, srno);
                    //parseNEWRevised(arrLine, srno, originalMessage);

                }
                else if (arrLine.Length > 0 && arrLine.Intersect(arrCNL).Any())
                {
                    await parseCNL(arrLine, srno);
                }
                else if (arrLine.Length > 0 && arrLine.Intersect(arrRPL).Any())
                {
                    await parseRPL(arrLine, srno);
                }
                else if (arrLine.Length > 0 && arrLine.Intersect(arrTIM).Any())
                {
                    await parseTIM(arrLine, srno);
                }
                else if (arrLine.Length > 0 && arrLine.Intersect(arrEQT).Any())
                {
                    string messageID = "EQT";
                    messageID = arrLine.Intersect(arrEQT).Count() > 0 && arrLine.Intersect(arrEQT).Single().Trim().Length > 2 && arrLine.Intersect(arrEQT).Single().Trim() == "CON" || arrLine.Intersect(arrEQT).Single().Trim() == "CON EQUI" ? "CON" : "EQT";
                    await parseEQT(arrLine, srno, messageID);
                }
                else if (arrLine.Length > 0 && arrLine.Intersect(arrRIN).Any())
                {
                    await parseRIN(arrLine, srno);
                }
                else if (arrLine.Length > 0 && arrLine.Intersect(arrADM).Any())
                {
                    await parseADM(arrLine, srno);
                }
                else if (arrLine.Length > 0 && arrLine.Intersect(arrRRT).Any())
                {
                    await parseRRT(arrLine, srno);
                }
                else
                {
                    //genericFunction.UpdateErrorMessageToInbox(srno, "Un-Supported ASM Message", "ASM", true, originalMessage.Replace("$", "\r\n"));
                    await _genericFunction.UpdateErrorMessageToInbox(srno, "Un-Supported ASM Message", "ASM", true, originalMessage.Replace("$", "\r\n"));
                    return isProcessFlag;
                }
                isProcessFlag = true;
            }
            catch (Exception ex)
            {
                isProcessFlag = false;
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on ToASM");
            }
            return isProcessFlag;
        }

        private async Task parseNEW(string[] arrLine, int srno)
        {
            try
            {
                SetVariablesToDefaultValues();
                int scheduleID = 0;
                legNumber = 1;
                string flightInfoLine = string.Empty;
                int indxFlightInfo = 0;
                messageTimeMode = "UTC";

                for (int i = 0; i < arrLine.Length; i++)
                {
                    if (arrLine[i].Trim() == "LT")
                        messageTimeMode = "LT";
                    if (arrNEW.Contains(arrLine[i].Trim()))
                    {
                        messageIdentifier = "NEW";
                        indxFlightInfo = i + 1;
                        break;
                    }
                }
                if (arrLine.Length > indxFlightInfo + 2)
                    flightInfoLine = arrLine[indxFlightInfo];
                else
                {
                    //GenericFunction genericFunction = new GenericFunction();
                    //genericFunction.UpdateErrorMessageToInbox(srno, "Invalid format", "ASM/NEW");
                    await _genericFunction.UpdateErrorMessageToInbox(srno, "Invalid format", "ASM/NEW");
                    return;
                }

                DataTable dtFlightInfo = new DataTable();
                dtFlightInfo = CreateFlightInfoDataTable();

                DataTable dtSaveFlightInfo = new DataTable();
                dtSaveFlightInfo = CreateFlightInfoDataTable();

                DataRow? dr = null;
                for (int i = indxFlightInfo; i < arrLine.Length; i++)
                {
                    String[] strMessages = arrLine[i].Trim().Split(' ');
                    ///J 333 C12Y365.C12Y365 RP-C8123
                    if (strMessages[0].Length == 1 || flightNo != string.Empty)
                        break;
                    flightNo = strMessages[0].Substring(0, flightInfoLine.IndexOf('/'));
                    schedDateOfArrival = schedDateOfDepart = strMessages[0].Substring(strMessages[0].IndexOf('/') + 1);

                    dr = dtFlightInfo.NewRow();
                    dr["FlightNumber"] = flightNo;
                    dr["SchDateOfDeparture"] = schedDateOfDepart;
                    dr["SchDateOfArrival"] = schedDateOfArrival;
                    dtFlightInfo.Rows.Add(dr);

                    //dtFlightInfo.Rows.Add(
                    //        flightNo
                    //        , ""
                    //        , ""
                    //        , schedDateOfDepart
                    //        , schedDateOfArrival
                    //    );
                    indxFlightInfo++;
                }
                DataRow? drSaveFlightInfo = null;
                for (int j = 0; j < dtFlightInfo.Rows.Count; j++)
                {
                    flightNo = dtFlightInfo.Rows[j]["FlightNumber"].ToString();
                    schedDateOfDepart = dtFlightInfo.Rows[j]["SchDateOfDeparture"].ToString();
                    schedDateOfArrival = dtFlightInfo.Rows[j]["SchDateOfArrival"].ToString();

                    for (int i = indxFlightInfo; i < arrLine.Length; i++)
                    {
                        String[] strMessages = arrLine[i].Trim().Split(' ');

                        ///J 333 C12Y365.C12Y365 RP-C8123
                        if (strMessages[0].Length == 1)
                        {
                            serviceType = strMessages[0];
                            aircraftType = strMessages[1];
                            //passangerReservation = strMessages[2];
                            passangerReservation = "";
                            if (strMessages.Length > 3)
                                registrationNo = strMessages[3];
                        }
                        ///KUL010000/2345 CGO010510/0515 7/FDC/CD/YS/MS/LS
                        int orgInfoLen = 0, destInfoLen = 0;
                        if (strMessages.Length > 1)
                        {
                            orgInfoLen = strMessages[0].Split('/')[0].Length;
                            destInfoLen = strMessages[1].Split('/')[0].Length;
                        }
                        if ((orgInfoLen == 7 || orgInfoLen == 9) && (destInfoLen == 7 || destInfoLen == 9))
                        {
                            airportOfDepart = strMessages[0].Substring(0, 3);
                            schedTimeOfDepart = strMessages[0].Split('/')[0].Substring(3);
                            if (schedTimeOfDepart.Length > 4)
                            {
                                schedDateOfDepart = schedDateOfDepart.Trim().Length > 2 ? schedTimeOfDepart.Substring(0, 2) + schedDateOfDepart.Trim().Substring(2) : schedTimeOfDepart.Substring(0, 2);
                                schedTimeOfDepart = schedTimeOfDepart.Substring(2);// remove first 2 char date
                            }

                            airportOfArrival = strMessages[1].Substring(0, 3);
                            schedTimeOfArrival = strMessages[1].Split('/')[0].Substring(3);
                            if (schedTimeOfArrival.Length > 4)
                            {
                                schedDateOfArrival = schedDateOfArrival.Trim().Length > 2 ? schedTimeOfArrival.Substring(0, 2) + schedDateOfArrival.Trim().Substring(2) : schedTimeOfArrival.Substring(0, 2);
                                schedTimeOfArrival = schedTimeOfArrival.Substring(2);// remove first 2 char date
                            }
                        }
                        if (!string.IsNullOrEmpty(airportOfDepart))
                        {
                            drSaveFlightInfo = dtSaveFlightInfo.NewRow();
                            drSaveFlightInfo["FlightNumber"] = flightNo;
                            drSaveFlightInfo["AirportOfDepart"] = airportOfDepart;
                            drSaveFlightInfo["AirportOfArrival"] = airportOfArrival;
                            drSaveFlightInfo["SchDateOfDeparture"] = schedDateOfDepart;
                            drSaveFlightInfo["SchDateOfArrival"] = schedDateOfArrival;
                            drSaveFlightInfo["SchedTimeOfDepart"] = schedTimeOfDepart;
                            drSaveFlightInfo["SchedTimeOfArrival"] = schedTimeOfArrival;
                            drSaveFlightInfo["ServiceType"] = serviceType;
                            drSaveFlightInfo["AircraftType"] = aircraftType;
                            drSaveFlightInfo["RegistrationNo"] = registrationNo;
                            dtSaveFlightInfo.Rows.Add(drSaveFlightInfo);
                            airportOfDepart = string.Empty;
                        }
                    }
                }
                flightNo = string.Empty;
                //for (int i = 0; i < dtSaveFlightInfo.Rows.Count; i++)
                for (int i = 0; i < 1; i++)
                {
                    bool isLastLeg = false;
                    if (flightNo != string.Empty && flightNo != dtSaveFlightInfo.Rows[i]["FlightNumber"].ToString())
                    {
                        isLastLeg = false;
                        scheduleID = 0;
                    }
                    if ((i == dtSaveFlightInfo.Rows.Count - 1)
                        || (dtSaveFlightInfo.Rows.Count - 1 > i && dtSaveFlightInfo.Rows[i]["FlightNumber"].ToString() != dtSaveFlightInfo.Rows[i + 1]["FlightNumber"].ToString()))
                        isLastLeg = true;

                    flightNo = dtSaveFlightInfo.Rows[i]["FlightNumber"].ToString();
                    airportOfDepart = dtSaveFlightInfo.Rows[i]["AirportOfDepart"].ToString();
                    airportOfArrival = dtSaveFlightInfo.Rows[i]["AirportOfArrival"].ToString();
                    schedDateOfDepart = dtSaveFlightInfo.Rows[i]["SchDateOfDeparture"].ToString();
                    schedDateOfArrival = dtSaveFlightInfo.Rows[i]["SchDateOfArrival"].ToString();
                    schedTimeOfDepart = dtSaveFlightInfo.Rows[i]["SchedTimeOfDepart"].ToString();
                    schedTimeOfArrival = dtSaveFlightInfo.Rows[i]["SchedTimeOfArrival"].ToString();
                    serviceType = dtSaveFlightInfo.Rows[i]["ServiceType"].ToString();
                    aircraftType = dtSaveFlightInfo.Rows[i]["AircraftType"].ToString();
                    registrationNo = dtSaveFlightInfo.Rows[i]["RegistrationNo"].ToString();
                    DataSet dsScheduleDetails = new DataSet();
                    dsScheduleDetails = await SaveASMDetails(srno, scheduleID, isLastLeg);
                    //if (dsScheduleDetails != null && dsScheduleDetails.Tables.Count > 0 && dsScheduleDetails.Tables[0].Rows.Count > 0 && dsScheduleDetails.Tables[0].Columns.Contains("ScheduleID"))
                    //{
                    //    scheduleID = Convert.ToInt32(dsScheduleDetails.Tables[0].Rows[0]["ScheduleID"].ToString());
                    //}
                    legNumber += 1;
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on parseNEW");
            }
        }

        /*Not in use*/
        //private async Task parseNEWRevised(string[] arrLine, int srno, string messageBody)
        //{
        //    try
        //    {
        //        //SSM ssm = new SSM();

        //        string frequency = string.Empty;
        //        SetVariablesToDefaultValues();
        //        int rowIncrement = 0;
        //        string thirdLine = string.Empty, scheduleIDs = string.Empty;
        //        int indxFlightInfo = 0;
        //        messageIdentifier = "NEW";

        //        for (int i = 0; i < arrLine.Length; i++)
        //        {
        //            if (arrLine[i].Trim() == "LT")
        //                messageTimeMode = "LT";
        //            if (arrNEW.Contains(arrLine[i].Trim()))
        //            {
        //                indxFlightInfo = i + 1;
        //                break;
        //            }
        //        }
        //        flightNo = arrLine[indxFlightInfo].Split(' ')[0].Split('/')[0];
        //        DataTable dtFlightInfo = new DataTable();
        //        dtFlightInfo = _ssm.CreateNewFlightDataTable();
        //        DataTable dtPeriodFrequency = new DataTable();
        //        dtPeriodFrequency = _ssm.CreatePeriodFreqDataTable();

        //        for (int i = indxFlightInfo; i < arrLine.Length; i++)
        //        {
        //            string[] strMessages = arrLine[i].Split(' ')[0].Split('/');

        //            if (strMessages[0].Trim().Length == 1)
        //                break;
        //            schedDateOfDepart = getDateDDMMMYY(strMessages[1]);
        //            schedDateOfArrival = getDateDDMMMYY(strMessages[1]);
        //            dtPeriodFrequency.Rows.Add(
        //                flightNo
        //                , schedDateOfDepart
        //                , schedDateOfArrival
        //                , (int)Convert.ToDateTime(schedDateOfDepart).DayOfWeek
        //            );
        //            indxFlightInfo++;
        //        }
        //        for (int i = 0; i < dtPeriodFrequency.Rows.Count; i++)
        //        {
        //            legNumber = 1;
        //            for (int j = indxFlightInfo; j < arrLine.Length; j++)
        //            {
        //                string[] strMessages = arrLine[j].Trim().Split(' ');
        //                if (strMessages[0] == "SI" || strMessages[0] == "//")
        //                    break;
        //                int orgInfoLen = 0, destInfoLen = 0;
        //                if (strMessages.Length > 1)
        //                {
        //                    orgInfoLen = strMessages[0].Split('/')[0].Length;
        //                    destInfoLen = strMessages[1].Split('/')[0].Length;
        //                }
        //                if ((orgInfoLen == 7 || orgInfoLen == 9) && (destInfoLen == 7 || destInfoLen == 9))
        //                {
        //                    dateVariationDep = dateVariationArr = 0;

        //                    string[] source = strMessages[0].Split('/');
        //                    airportOfDepart = strMessages[0].Substring(0, 3);
        //                    schedTimeOfDepart = strMessages[0].Split('/')[0].Substring(3);

        //                    //if (source.Length > 1 && (source[1].Length == 1 || source[1].Length == 2))
        //                    //{
        //                    //    dateVariationDep = Convert.ToInt32(source[1].Substring(source[1].Length - 1));
        //                    //    dateVariationDep = source[1].Substring(0, 1) == "M" ? -dateVariationDep : dateVariationDep;
        //                    //}
        //                    string[] dest = strMessages[1].Split('/');
        //                    airportOfArrival = strMessages[1].Substring(0, 3);
        //                    schedTimeOfArrival = strMessages[1].Split('/')[0].Substring(3);
        //                    //if (dest.Length > 1 && (dest[1].Length == 1 || dest[1].Length == 2))
        //                    //{
        //                    //    dateVariationArr = Convert.ToInt32(dest[1].Substring(dest[1].Length - 1));
        //                    //    dateVariationArr = dest[1].Substring(0, 1) == "M" ? -dateVariationArr : dateVariationArr;
        //                    //}

        //                    schedDateOfDepart = dtPeriodFrequency.Rows[i]["FromDate"].ToString();
        //                    schedDateOfArrival = dtPeriodFrequency.Rows[i]["ToDate"].ToString();
        //                    frequency = dtPeriodFrequency.Rows[i]["Frequency"].ToString();
        //                    frequency = ((int)Convert.ToDateTime(schedDateOfDepart).DayOfWeek).ToString() == "0" ? "7" : ((int)Convert.ToDateTime(schedDateOfDepart).DayOfWeek).ToString();
        //                    if (schedTimeOfDepart.Length == 6)
        //                    {
        //                        schedDateOfDepart = schedDateOfDepart.Substring(0, 8) + schedTimeOfDepart.Substring(0, 2);
        //                        schedTimeOfDepart = schedTimeOfDepart.Substring(2);
        //                    }
        //                    if (schedTimeOfArrival.Length == 6)
        //                    {
        //                        schedDateOfArrival = schedDateOfArrival.Substring(0, 8) + schedTimeOfArrival.Substring(0, 2);
        //                        schedTimeOfArrival = schedTimeOfArrival.Substring(2);
        //                    }

        //                    dtFlightInfo.Rows.Add(
        //                        ++rowIncrement
        //                        , srno
        //                        , legNumber
        //                        , messageType
        //                        , messageIdentifier
        //                        , flightNo
        //                        , schedDateOfDepart
        //                        , schedDateOfArrival
        //                        , airportOfDepart
        //                        , airportOfArrival
        //                        , frequency
        //                        , schedTimeOfDepart
        //                        , schedTimeOfArrival
        //                        , messageTimeMode
        //                        , dateVariationDep
        //                        , dateVariationArr
        //                        , ""
        //                        , ""
        //                        , serviceType
        //                        , aircraftType
        //                        , registrationNo
        //                        , ""
        //                        , messageBody
        //                    );
        //                    legNumber++;
        //                }
        //                ///C 330 F10Y100/FO.F10Y120
        //                else if (strMessages[0].Trim().Length == 1)
        //                {
        //                    //dtFlightInfo.Rows.Add(drNewFlightInfo);
        //                    //dtFlightInfo.Rows[flightInfoRowIndex]["FlightType"] = strMessages[0];
        //                    //dtFlightInfo.Rows[flightInfoRowIndex]["AircraftType"] = strMessages[1];
        //                    serviceType = strMessages[0];
        //                    aircraftType = strMessages[1];
        //                    if (strMessages.Length > 3)
        //                        registrationNo = strMessages[3];
        //                    //isNewRowAddedToFlightInfoTable = true;
        //                }
        //            }
        //        }
        //        DataTable dtUniqueFlightInfo = _ssm.RemoveDuplicatesRecords(dtFlightInfo);
        //        //SQLServer sqlServer = new SQLServer();
        //        // dsFlightinfo = sqlServer.SelectRecords("Messaging.uspSSMNEW", sqlParameter);

        //        SqlParameter[] sqlParameter = new SqlParameter[]{
        //            new  SqlParameter("@FlightInfoTableType", dtUniqueFlightInfo)
        //        };
        //        DataSet? dsFlightinfo = new DataSet();

        //        dsFlightinfo = await _readWriteDao.SelectRecords("Messaging.uspSSMNEW", sqlParameter);
        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //        _logger.LogError(ex, "Error on parseNEWRevised");
        //    }
        //}

        private async Task parseCNL(string[] arrLine, int srno)
        {
            try
            {
                SetVariablesToDefaultValues();
                messageIdentifier = "CNL";
                int indxFlightInfo = 0;
                messageTimeMode = "UTC";

                for (int i = 0; i < arrLine.Length; i++)
                {
                    if (arrLine[i].Trim() == "LT")
                        messageTimeMode = "LT";
                    if (arrCNL.Contains(arrLine[i].Trim()))
                    {
                        indxFlightInfo = i + 1;
                        break;
                    }
                }
                for (int i = indxFlightInfo; i < arrLine.Length; i++)
                {
                    ///AA407P/408/27 ORD/LAS
                    string[] strMessages = arrLine[i].Trim().Split(' ');
                    ///SI FreeText
                    if (strMessages[0] == "SI" || strMessages[0] == "//")
                        break;
                    string[] arrFlightInfo = strMessages[0].Split('/');
                    schedDateOfDepart = schedDateOfArrival = arrFlightInfo[arrFlightInfo.Length - 1];
                    flightNo = arrFlightInfo[0];
                    string carrier = flightNo.Substring(0, 2);

                    string[] arrLegInfo = new string[0];
                    if (strMessages.Length > 1)
                    {
                        arrLegInfo = strMessages[1].Split('/');
                    }

                    if (arrLegInfo.Length > 1 && arrLegInfo[0].Length > 1 && arrLegInfo[1].Length > 1)
                    {
                        for (int k = 0; k < arrLegInfo.Length - 1; k++)
                        {
                            if (arrLegInfo[k].Trim().Length > 1 && arrLegInfo[k + 1].Trim().Length > 1)
                            {
                                airportOfDepart = arrLegInfo[k];
                                airportOfArrival = arrLegInfo[k + 1];
                                for (int j = 0; j < arrFlightInfo.Length - 1; j++)
                                {
                                    flightNo = CombineCarrierAndFlightCode(arrFlightInfo[j], carrier);
                                    await SaveASMDetails(srno);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < arrFlightInfo.Length - 1; j++)
                        {
                            flightNo = CombineCarrierAndFlightCode(arrFlightInfo[j], carrier);
                            await SaveASMDetails(srno);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on parseCNL");
            }
        }

        private async Task parseRIN(string[] arrLine, int srno)
        {
            try
            {
                SetVariablesToDefaultValues();
                messageIdentifier = "RIN";
                String thirdLine = string.Empty;
                int indxFlightInfo = 0;
                messageTimeMode = "UTC";

                for (int i = 0; i < arrLine.Length; i++)
                {
                    if (arrLine[i].Trim() == "LT")
                        messageTimeMode = "LT";
                    if (arrRIN.Contains(arrLine[i].Trim()))
                    {
                        indxFlightInfo = i + 1;
                        break;
                    }
                }
                for (int i = indxFlightInfo; i < arrLine.Length; i++)
                {
                    ///AA407P/408/27 ORD/LAS
                    string[] strMessages = arrLine[i].Trim().Split(' ');
                    ///SI FreeText
                    if (strMessages[0] == "SI" || strMessages[0] == "//")
                        break;
                    string[] arrFlightInfo = strMessages[0].Split('/');
                    schedDateOfDepart = schedDateOfArrival = arrFlightInfo[arrFlightInfo.Length - 1];
                    flightNo = arrFlightInfo[0];
                    string carrier = flightNo.Substring(0, 2);
                    if (strMessages.Length > 1 && strMessages[1].Split('/').Length > 1)
                    {
                        airportOfDepart = strMessages[1].Split('/')[0];
                        airportOfArrival = strMessages[1].Split('/')[1];
                    }
                    for (int j = 0; j < arrFlightInfo.Length - 1; j++)
                    {
                        flightNo = CombineCarrierAndFlightCode(arrFlightInfo[j], carrier);
                        await SaveASMDetails(srno);
                    }
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on parseRIN");
            }
        }

        private async Task parseRPL(string[] arrLine, int srno)
        {
            try
            {
                SetVariablesToDefaultValues();
                string flightInfoLine = string.Empty;
                int indxFlightInfo = 0;
                messageTimeMode = "UTC";

                for (int i = 0; i < arrLine.Length; i++)
                {
                    if (arrLine[i].Trim() == "LT")
                        messageTimeMode = "LT";
                    if (arrRPL.Contains(arrLine[i].Trim()))
                    {
                        messageIdentifier = "RPL";
                        indxFlightInfo = i + 1;
                        break;
                    }
                }
                if (arrLine.Length > indxFlightInfo + 1)
                    flightInfoLine = arrLine[indxFlightInfo];
                else
                {
                    //GenericFunction genericFunction = new GenericFunction();
                    //genericFunction.UpdateErrorMessageToInbox(srno, "Invalid format", "ASM/NEW");
                    await _genericFunction.UpdateErrorMessageToInbox(srno, "Invalid format", "ASM/NEW");
                    return;
                }

                DataTable dtFlightInfo = new DataTable();
                dtFlightInfo = CreateFlightInfoDataTable();
                DataRow dr = null;
                for (int i = indxFlightInfo; i < arrLine.Length; i++)
                {
                    String[] strMessages = arrLine[i].Trim().Split(' ');
                    ///J 333 C12Y365.C12Y365 RP-C8123
                    if (strMessages[0].Length == 1)
                        break;
                    flightNo = strMessages[0].Substring(0, flightInfoLine.IndexOf('/'));
                    schedDateOfArrival = schedDateOfDepart = strMessages[0].Substring(strMessages[0].IndexOf('/') + 1);
                    dr = dtFlightInfo.NewRow();
                    dr["FlightNumber"] = flightNo;
                    dr["SchDateOfDeparture"] = schedDateOfDepart;
                    dr["SchDateOfArrival"] = schedDateOfArrival;
                    dtFlightInfo.Rows.Add(dr);
                    //dtFlightInfo.Rows.Add(
                    //        flightNo
                    //        , schedDateOfDepart
                    //        , schedDateOfArrival
                    //    );
                    indxFlightInfo++;
                }
                for (int j = 0; j < dtFlightInfo.Rows.Count; j++)
                {
                    flightNo = dtFlightInfo.Rows[j]["FlightNumber"].ToString();
                    schedDateOfDepart = dtFlightInfo.Rows[j]["SchDateOfDeparture"].ToString();
                    schedDateOfArrival = dtFlightInfo.Rows[j]["SchDateOfArrival"].ToString();
                    for (int i = indxFlightInfo; i < arrLine.Length; i++)
                    {
                        String[] strMessages = arrLine[i].Trim().Split(' ');

                        ///J 333 C12Y365.C12Y365 RP-C8123
                        if (strMessages[0].Length == 1)
                        {
                            serviceType = strMessages[0];
                            aircraftType = strMessages[1];
                            //passangerReservation = strMessages[2];
                            passangerReservation = "";
                            if (strMessages.Length > 3)
                                registrationNo = strMessages[3];
                        }
                        ///KUL010000/2345 CGO010510/0515 7/FDC/CD/YS/MS/LS
                        int orgInfoLen = 0, destInfoLen = 0;
                        if (strMessages.Length > 1)
                        {
                            orgInfoLen = strMessages[0].Split('/')[0].Length;
                            destInfoLen = strMessages[1].Split('/')[0].Length;
                        }
                        if ((orgInfoLen == 7 || orgInfoLen == 9) && (destInfoLen == 7 || destInfoLen == 9))
                        {
                            airportOfDepart = strMessages[0].Substring(0, 3);
                            schedTimeOfDepart = strMessages[0].Split('/')[0].Substring(3);
                            if (schedTimeOfDepart.Length > 4)
                            {
                                //schedDateOfDepart = getDate(Convert.ToInt32(schedTimeOfDepart.Substring(0, 2)));
                                schedDateOfDepart = schedDateOfDepart.Trim().Length > 2 ? schedTimeOfDepart.Substring(0, 2) + schedDateOfDepart.Trim().Substring(2) : schedTimeOfDepart.Substring(0, 2);
                                schedTimeOfDepart = schedTimeOfDepart.Substring(2);// remove first 2 char date
                            }

                            airportOfArrival = strMessages[1].Substring(0, 3);
                            schedTimeOfArrival = strMessages[1].Split('/')[0].Substring(3);
                            if (schedTimeOfArrival.Length > 4)
                            {
                                //schedDateOfArrival = getDate(Convert.ToInt32(schedTimeOfArrival.Substring(0, 2)));
                                schedDateOfArrival = schedDateOfArrival.Trim().Length > 2 ? schedTimeOfArrival.Substring(0, 2) + schedDateOfArrival.Trim().Substring(2) : schedTimeOfArrival.Substring(0, 2);
                                schedTimeOfArrival = schedTimeOfArrival.Substring(2);// remove first 2 char date
                            }
                        }
                        if (!string.IsNullOrEmpty(airportOfDepart))
                        {
                            await SaveASMDetails(srno);
                            airportOfDepart = string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on parseRPL");
            }
        }

        private async Task parseADM(string[] arrLine, int srno)
        {
            try
            {
                SetVariablesToDefaultValues();
                messageIdentifier = "ADM";
                await SaveASMDetails(srno);
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on parseADM");
            }
        }

        private async Task parseEQT(string[] arrLine, int srno, string msgId)
        {
            try
            {
                SetVariablesToDefaultValues();
                messageIdentifier = msgId;
                string thirdLine = string.Empty;///Flight Info Line
                int indxFlightInfo = 0, indxAircraftInfo = 0;
                messageTimeMode = "UTC";

                for (int i = 0; i < arrLine.Length; i++)
                {
                    if (arrLine[i].Trim() == "LT")
                        messageTimeMode = "LT";
                    if (arrEQT.Contains(arrLine[i].Trim()))
                    {
                        indxFlightInfo = i + 1;
                        indxAircraftInfo = i + 2;
                        break;
                    }
                }
                if (arrLine.Length > 3)
                {
                    thirdLine = arrLine[indxFlightInfo];
                    flightNo = thirdLine.Substring(0, thirdLine.IndexOf('/'));
                    string carrier = flightNo.Substring(0, 2);
                    string[] arrFlightInfo = thirdLine.Split(' ')[0].Split('/');
                    schedDateOfDepart = arrFlightInfo[arrFlightInfo.Length - 1];
                    if (thirdLine.Split(' ').Length > 1 && thirdLine.Split(' ')[1].Split('/').Length == 2 && thirdLine.Split(' ')[1].Split('/')[0].Length == 3 && thirdLine.Split(' ')[1].Split('/')[1].Length == 3)
                    {
                        airportOfDepart = thirdLine.Split(' ')[1].Split('/')[0];
                        airportOfArrival = thirdLine.Split(' ')[1].Split('/')[1];
                    }
                    string[] aircraftInfoArr = arrLine[indxAircraftInfo].Split(' ');
                    serviceType = aircraftInfoArr[0];
                    aircraftType = aircraftInfoArr[1];
                    if (aircraftInfoArr.Length > 3)
                        registrationNo = aircraftInfoArr[3];

                    for (int i = 0; i < arrFlightInfo.Length - 1; i++)
                    {
                        flightNo = CombineCarrierAndFlightCode(arrFlightInfo[i].Trim(), carrier);
                        await SaveASMDetails(srno);
                    }
                }
                else
                {
                    //GenericFunction genericFunction = new GenericFunction();
                    //genericFunction.UpdateErrorMessageToInbox(srno, "Invalid format", "ASM/EQT");
                    await _genericFunction.UpdateErrorMessageToInbox(srno, "Invalid format", "ASM/EQT");
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on parseEQT");
            }
        }

        private async Task parseRRT(string[] arrLine, int srno)
        {
            try
            {
                SetVariablesToDefaultValues();
                string flightInfoLine = string.Empty;
                int indxFlightInfo = 0;
                messageTimeMode = "UTC";

                for (int i = 0; i < arrLine.Length; i++)
                {
                    if (arrLine[i].Trim() == "LT")
                        messageTimeMode = "LT";
                    if (arrRRT.Contains(arrLine[i].Trim()))
                    {
                        messageIdentifier = "RRT";
                        indxFlightInfo = i + 1;
                        break;
                    }
                }
                if (arrLine.Length > indxFlightInfo + 1)
                    flightInfoLine = arrLine[indxFlightInfo];
                else
                {
                    //GenericFunction genericFunction = new GenericFunction();
                    //genericFunction.UpdateErrorMessageToInbox(srno, "Invalid format", "ASM/RRT");
                    await _genericFunction.UpdateErrorMessageToInbox(srno, "Invalid format", "ASM/RRT");
                    return;
                }

                DataTable dtFlightInfo = new DataTable();
                dtFlightInfo = CreateFlightInfoDataTable();

                DataRow drFlightInfo = null;
                for (int i = indxFlightInfo; i < arrLine.Length; i++)
                {
                    string[] strMessages = arrLine[i].Trim().Split(' ');
                    ///J 333 C12Y365.C12Y365 RP-C8123
                    if (strMessages[0].Length == 1)
                        break;
                    string[] arrFlightInfo = strMessages[0].Split('/');
                    schedDateOfDepart = schedDateOfArrival = arrFlightInfo[arrFlightInfo.Length - 1];
                    flightNo = arrFlightInfo[0];
                    string carrier = flightNo.Substring(0, 2);
                    for (int j = 0; j < arrFlightInfo.Length - 1; j++)
                    {
                        flightNo = CombineCarrierAndFlightCode(arrFlightInfo[j], carrier);
                        drFlightInfo = dtFlightInfo.NewRow();
                        drFlightInfo["FlightNumber"] = flightNo;
                        drFlightInfo["SchDateOfDeparture"] = schedDateOfDepart;
                        drFlightInfo["SchDateOfArrival"] = schedDateOfArrival;
                        dtFlightInfo.Rows.Add(drFlightInfo);
                        //dtFlightInfo.Rows.Add(
                        //    flightNo
                        //    , schedDateOfDepart
                        //    , schedDateOfArrival
                        //);
                    }
                    indxFlightInfo++;
                }
                for (int j = 0; j < dtFlightInfo.Rows.Count; j++)
                {
                    flightNo = dtFlightInfo.Rows[j]["FlightNumber"].ToString();
                    schedDateOfDepart = dtFlightInfo.Rows[j]["SchDateOfDeparture"].ToString();
                    schedDateOfArrival = dtFlightInfo.Rows[j]["SchDateOfArrival"].ToString();
                    for (int i = indxFlightInfo; i < arrLine.Length; i++)
                    {
                        String[] strMessages = arrLine[i].Trim().Split(' ');

                        ///J 333 C12Y365.C12Y365 RP-C8123
                        if (strMessages[0].Length == 1)
                        {
                            serviceType = strMessages[0];
                            aircraftType = strMessages[1];
                            //passangerReservation = strMessages[2];
                            passangerReservation = "";
                            if (strMessages.Length > 3)
                                registrationNo = strMessages[3];
                        }
                        ///KUL010000/2345 CGO010510/0515 7/FDC/CD/YS/MS/LS
                        int orgInfoLen = 0, destInfoLen = 0;
                        if (strMessages.Length > 1)
                        {
                            orgInfoLen = strMessages[0].Split('/')[0].Length;
                            destInfoLen = strMessages[1].Split('/')[0].Length;
                        }
                        if ((orgInfoLen == 7 || orgInfoLen == 9) && (destInfoLen == 7 || destInfoLen == 9))
                        {
                            airportOfDepart = strMessages[0].Substring(0, 3);
                            schedTimeOfDepart = strMessages[0].Split('/')[0].Substring(3);
                            if (schedTimeOfDepart.Length > 4)
                            {
                                //schedDateOfDepart = getDate(Convert.ToInt32(schedTimeOfDepart.Substring(0, 2)));
                                schedDateOfDepart = schedDateOfDepart.Trim().Length > 2 ? schedTimeOfDepart.Substring(0, 2) + schedDateOfDepart.Trim().Substring(2) : schedTimeOfDepart.Substring(0, 2);
                                schedTimeOfDepart = schedTimeOfDepart.Substring(2);// remove first 2 char date
                            }

                            airportOfArrival = strMessages[1].Substring(0, 3);
                            schedTimeOfArrival = strMessages[1].Split('/')[0].Substring(3);
                            if (schedTimeOfArrival.Length > 4)
                            {
                                //schedDateOfArrival = getDate(Convert.ToInt32(schedTimeOfArrival.Substring(0, 2)));
                                schedDateOfArrival = schedDateOfArrival.Trim().Length > 2 ? schedTimeOfArrival.Substring(0, 2) + schedDateOfArrival.Trim().Substring(2) : schedTimeOfArrival.Substring(0, 2);
                                schedTimeOfArrival = schedTimeOfArrival.Substring(2);// remove first 2 char date
                            }
                        }
                        if (!string.IsNullOrEmpty(airportOfDepart))
                        {
                            legNumber += 1;
                            await SaveASMDetails(srno);
                            airportOfDepart = string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on parseRRT");
            }
        }

        private async Task parseTIM(string[] arrLine, int srno)
        {
            try
            {
                SetVariablesToDefaultValues();
                string flightInfoLine = string.Empty;
                int indxFlightInfo = 0;
                messageTimeMode = "UTC";

                for (int i = 0; i < arrLine.Length; i++)
                {
                    if (arrLine[i].Trim() == "LT")
                        messageTimeMode = "LT";
                    if (arrLine[i].Trim() == "TIM" || arrLine[i].Trim() == "TIM COMM" || arrLine[i].Trim() == "TIM/ADM")
                    {
                        messageIdentifier = "TIM";
                        indxFlightInfo = i + 1;
                        break;
                    }
                }
                if (arrLine.Length > indxFlightInfo + 1)
                    flightInfoLine = arrLine[indxFlightInfo];
                else
                {
                    //GenericFunction genericFunction = new GenericFunction();
                    //genericFunction.UpdateErrorMessageToInbox(srno, "Invalid format", "ASM/TIM");
                    await _genericFunction.UpdateErrorMessageToInbox(srno, "Invalid format", "ASM/TIM");
                    return;
                }

                DataTable dtFlightInfo = new DataTable();
                dtFlightInfo = CreateFlightInfoDataTable();
                DataRow dr = null;
                for (int i = indxFlightInfo; i < arrLine.Length; i++)
                {
                    String[] strMessages = arrLine[i].Trim().Split(' ');
                    ///J 333 C12Y365.C12Y365 RP-C8123
                    int orgInfoLen = 0, destInfoLen = 0;
                    if (strMessages.Length > 1)
                    {
                        orgInfoLen = strMessages[0].Split('/')[0].Length;
                        destInfoLen = strMessages[1].Split('/')[0].Length;
                    }
                    if ((orgInfoLen == 7 || orgInfoLen == 9) && (destInfoLen == 7 || destInfoLen == 9))
                        break;
                    flightNo = strMessages[0].Substring(0, flightInfoLine.IndexOf('/'));
                    schedDateOfArrival = schedDateOfDepart = strMessages[0].Substring(strMessages[0].IndexOf('/') + 1);

                    dr = dtFlightInfo.NewRow();
                    dr["FlightNumber"] = flightNo;
                    dr["SchDateOfDeparture"] = schedDateOfDepart;
                    dr["SchDateOfArrival"] = schedDateOfArrival;
                    dtFlightInfo.Rows.Add(dr);

                    //dtFlightInfo.Rows.Add(
                    //        flightNo
                    //        , schedDateOfDepart
                    //        , schedDateOfArrival
                    //    );
                    indxFlightInfo++;
                }
                for (int j = 0; j < dtFlightInfo.Rows.Count; j++)
                {
                    flightNo = dtFlightInfo.Rows[j]["FlightNumber"].ToString();
                    schedDateOfDepart = dtFlightInfo.Rows[j]["SchDateOfDeparture"].ToString();
                    schedDateOfArrival = dtFlightInfo.Rows[j]["SchDateOfArrival"].ToString();
                    for (int i = indxFlightInfo; i < arrLine.Length; i++)
                    {
                        String[] strMessages = arrLine[i].Trim().Split(' ');

                        ///KUL010000/2345 CGO010510/0515 7/FDC/CD/YS/MS/LS
                        int orgInfoLen = 0, destInfoLen = 0;
                        if (strMessages.Length > 1)
                        {
                            orgInfoLen = strMessages[0].Split('/')[0].Length;
                            destInfoLen = strMessages[1].Split('/')[0].Length;
                        }
                        if ((orgInfoLen == 7 || orgInfoLen == 9) && (destInfoLen == 7 || destInfoLen == 9))
                        {
                            airportOfDepart = strMessages[0].Substring(0, 3);
                            schedTimeOfDepart = strMessages[0].Split('/')[0].Substring(3);
                            if (schedTimeOfDepart.Length > 4)
                            {
                                //schedDateOfDepart = getDate(Convert.ToInt32(schedTimeOfDepart.Substring(0, 2)));
                                schedDateOfDepart = schedDateOfDepart.Trim().Length > 2 ? schedTimeOfDepart.Substring(0, 2) + schedDateOfDepart.Trim().Substring(2) : schedTimeOfDepart.Substring(0, 2);
                                schedTimeOfDepart = schedTimeOfDepart.Substring(2);// remove first 2 char date
                            }

                            airportOfArrival = strMessages[1].Substring(0, 3);
                            schedTimeOfArrival = strMessages[1].Split('/')[0].Substring(3);
                            if (schedTimeOfArrival.Length > 4)
                            {
                                //schedDateOfArrival = getDate(Convert.ToInt32(schedTimeOfArrival.Substring(0, 2)));
                                schedDateOfArrival = schedDateOfArrival.Trim().Length > 2 ? schedTimeOfArrival.Substring(0, 2) + schedDateOfArrival.Trim().Substring(2) : schedTimeOfArrival.Substring(0, 2);
                                schedTimeOfArrival = schedTimeOfArrival.Substring(2);// remove first 2 char date
                            }
                        }
                        if (!string.IsNullOrEmpty(airportOfDepart))
                        {
                            await SaveASMDetails(srno);
                            airportOfDepart = string.Empty;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on parseTIM");
            }
        }

        private async Task<DataSet> SaveASMDetails(int srno, int scheduleID = 0, bool isLastLeg = false)
        {
            DataSet dsScheduleDetails = new DataSet();
            try
            {
                object[] QueryValues = {
                            messageType ,
                            messageIdentifier  ,
                            originalMessage ,
                            flightNo ,
                            registrationNo,
                            aircraftType,
                            passangerReservation==""?"0":passangerReservation ,
                            airportOfDepart ,
                            airportOfArrival ,
                            "",//nextDestAirport,
                            getDate(schedDateOfArrival),
                            getDate(schedDateOfDepart),
                            getDate(schedDateOfArrival),//actualDateOfArrival,
                            getDate(schedDateOfDepart),//actualDateOfDepart,
                            schedTimeOfArrival,
                            schedTimeOfDepart,
                            schedTimeOfArrival,//actualTimeOfArrival,
                            schedTimeOfDepart,//actualTimeOfDepart,
                            "",//onBlockTime,
                            serviceType ,
                            "",//delayCode ,
                            "",//nextInformation ,
                            "",//suppelementoryInfo,
                            getDate(DateTime.Now.ToString("ddMMMyy")),// DateTime.Now.ToShortDateString()
                            srno,
                            messageTimeMode,
                            legNumber,
                            "",
                            dateVariationDep,
                            dateVariationArr,
                            scheduleID,
                            isLastLeg,
                            "",
                            "",
                            getDate(DateTime.Now.ToString("ddMMMyy")),
                            "",
                            ""//New flight number
                        };

                dsScheduleDetails = await UpdateToDatatabse(QueryValues);

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on SaveASMDetails");
            }
            return dsScheduleDetails;
        }

        public async Task<DataSet> UpdateToDatatabse(object[] QueryValues)
        {
            DataSet dsScheduleDetails = new DataSet();
            try
            {
                //string[] QueryNames = new string[37];
                //SqlDbType[] QueryTypes = new SqlDbType[37];
                //SQLServer db = new SQLServer();


                //QueryNames[0] = "MsgType";
                //QueryNames[1] = "msgId";
                //QueryNames[2] = "msgBody";
                //QueryNames[3] = "flightNumber";
                //QueryNames[4] = "tailNumber";
                //QueryNames[5] = "aircraftType";
                //QueryNames[6] = "passangerCount";
                //QueryNames[7] = "airportOfDeparture";
                //QueryNames[8] = "airportOfArrival";
                //QueryNames[9] = "nextDestAirport";
                //QueryNames[10] = "schdDateOfArrival";
                //QueryNames[11] = "schdDateOfDeparture";
                //QueryNames[12] = "actualDateOfArrival";
                //QueryNames[13] = "actualDateOfDeparture";
                //QueryNames[14] = "schdTimeOfArrival";
                //QueryNames[15] = "schdTimeOfDeparture";
                //QueryNames[16] = "actualTimeOfArrival";
                //QueryNames[17] = "actualTimeOfDeparture";
                //QueryNames[18] = "onBlockTime";
                //QueryNames[19] = "serviceType";
                //QueryNames[20] = "delayCode";
                //QueryNames[21] = "nextInformation";
                //QueryNames[22] = "supplemtoryInfo";
                //QueryNames[23] = "createdOn";
                //QueryNames[24] = "srno";
                //QueryNames[25] = "MessageTimeMode";
                //QueryNames[26] = "LegNumber";
                //QueryNames[27] = "SSMFrequency";
                //QueryNames[28] = "DateVariationDep";
                //QueryNames[29] = "DateVariationArr";
                //QueryNames[30] = "ScheduleID";
                //QueryNames[31] = "IsLastLeg";
                //QueryNames[32] = "ScheduleIDs";
                //QueryNames[33] = "POL";
                //QueryNames[34] = "DepDate";
                //QueryNames[35] = "OrgDepTime";
                //QueryNames[36] = "NewFlightNumber";

                //QueryTypes[0] = SqlDbType.VarChar;
                //QueryTypes[1] = SqlDbType.VarChar;
                //QueryTypes[2] = SqlDbType.VarChar;
                //QueryTypes[3] = SqlDbType.VarChar;
                //QueryTypes[4] = SqlDbType.VarChar;
                //QueryTypes[5] = SqlDbType.VarChar;
                //QueryTypes[6] = SqlDbType.Int;
                //QueryTypes[7] = SqlDbType.VarChar;
                //QueryTypes[8] = SqlDbType.VarChar;
                //QueryTypes[9] = SqlDbType.VarChar;
                //QueryTypes[10] = SqlDbType.DateTime;
                //QueryTypes[11] = SqlDbType.DateTime;
                //QueryTypes[12] = SqlDbType.DateTime;
                //QueryTypes[13] = SqlDbType.DateTime;
                //QueryTypes[14] = SqlDbType.VarChar;
                //QueryTypes[15] = SqlDbType.VarChar;
                //QueryTypes[16] = SqlDbType.VarChar;
                //QueryTypes[17] = SqlDbType.VarChar;
                //QueryTypes[18] = SqlDbType.VarChar;
                //QueryTypes[19] = SqlDbType.VarChar;
                //QueryTypes[20] = SqlDbType.VarChar;
                //QueryTypes[21] = SqlDbType.VarChar;
                //QueryTypes[22] = SqlDbType.VarChar;
                //QueryTypes[23] = SqlDbType.DateTime;
                //QueryTypes[24] = SqlDbType.Int;
                //QueryTypes[25] = SqlDbType.VarChar;
                //QueryTypes[26] = SqlDbType.Int;
                //QueryTypes[27] = SqlDbType.VarChar;
                //QueryTypes[28] = SqlDbType.Int;
                //QueryTypes[29] = SqlDbType.Int;
                //QueryTypes[30] = SqlDbType.Int;
                //QueryTypes[31] = SqlDbType.Bit;
                //QueryTypes[32] = SqlDbType.VarChar;
                //QueryTypes[33] = SqlDbType.VarChar;
                //QueryTypes[34] = SqlDbType.DateTime;
                //QueryTypes[35] = SqlDbType.VarChar;
                //QueryTypes[36] = SqlDbType.VarChar;
                //if (QueryValues.Length == 27)
                //{
                //    Array.Resize(ref QueryValues, QueryValues.Length + 1);
                //    QueryValues[QueryValues.Length - 1] = 0;
                //}

                QueryValues[2] = QueryValues[2].ToString().Replace("$", "\r\n");
                //dsScheduleDetails = db.SelectRecords("Messaging.uspAddFlightMovementDetails", QueryNames, QueryValues, QueryTypes);

                var parameters = new SqlParameter[]
                {
                    new("@MsgType", SqlDbType.VarChar) { Value = "" },
                    new("@msgId", SqlDbType.VarChar) { Value = "" },
                    new("@msgBody", SqlDbType.VarChar) { Value = "" },
                    new("@flightNumber", SqlDbType.VarChar) { Value = "" },
                    new("@tailNumber", SqlDbType.VarChar) { Value = "" },
                    new("@aircraftType", SqlDbType.VarChar) { Value = "" },
                    new("@passangerCount", SqlDbType.Int) { Value = 0 },
                    new("@airportOfDeparture", SqlDbType.VarChar) { Value = "" },
                    new("@airportOfArrival", SqlDbType.VarChar) { Value = "" },
                    new("@nextDestAirport", SqlDbType.VarChar) { Value = "" },
                    new("@schdDateOfArrival", SqlDbType.DateTime) { Value = DBNull.Value },
                    new("@schdDateOfDeparture", SqlDbType.DateTime) { Value = DBNull.Value },
                    new("@actualDateOfArrival", SqlDbType.DateTime) { Value = DBNull.Value },
                    new("@actualDateOfDeparture", SqlDbType.DateTime) { Value = DBNull.Value },
                    new("@schdTimeOfArrival", SqlDbType.VarChar) { Value = "" },
                    new("@schdTimeOfDeparture", SqlDbType.VarChar) { Value = "" },
                    new("@actualTimeOfArrival", SqlDbType.VarChar) { Value = "" },
                    new("@actualTimeOfDeparture", SqlDbType.VarChar) { Value = "" },
                    new("@onBlockTime", SqlDbType.VarChar) { Value = "" },
                    new("@serviceType", SqlDbType.VarChar) { Value = "" },
                    new("@delayCode", SqlDbType.VarChar) { Value = "" },
                    new("@nextInformation", SqlDbType.VarChar) { Value = "" },
                    new("@supplemtoryInfo", SqlDbType.VarChar) { Value = "" },
                    new("@createdOn", SqlDbType.DateTime) { Value = DBNull.Value },
                    new("@srno", SqlDbType.Int) { Value = 0 },
                    new("@MessageTimeMode", SqlDbType.VarChar) { Value = "" },
                    new("@LegNumber", SqlDbType.Int) { Value = 0 },
                    new("@SSMFrequency", SqlDbType.VarChar) { Value = "" },
                    new("@DateVariationDep", SqlDbType.Int) { Value = 0 },
                    new("@DateVariationArr", SqlDbType.Int) { Value = 0 },
                    new("@ScheduleID", SqlDbType.Int) { Value = 0 },
                    new("@IsLastLeg", SqlDbType.Bit) { Value = 0 },
                    new("@ScheduleIDs", SqlDbType.VarChar) { Value = "" },
                    new("@POL", SqlDbType.VarChar) { Value = "" },
                    new("@DepDate", SqlDbType.DateTime) { Value = DBNull.Value },
                    new("@OrgDepTime", SqlDbType.VarChar) { Value = "" },
                    new("@NewFlightNumber", SqlDbType.VarChar) { Value = "" },
                };
                for (int i = 0; i < QueryValues.Length; i++)
                {
                    parameters[i].Value = QueryValues[i];
                }
                dsScheduleDetails = await _readWriteDao.SelectRecords("Messaging.uspAddFlightMovementDetails", parameters) ?? new DataSet();
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on UpdateToDatatabse");
            }
            return dsScheduleDetails;
        }

        private string CombineCarrierAndFlightCode(string flightNumber, string carrier)
        {
            try
            {
                if (flightNumber.Length >= 2 && flightNumber.Substring(0, 2) != carrier)
                    flightNumber = carrier + flightNumber;
                else if (flightNumber.Length < 2)
                    flightNumber = carrier + flightNumber;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on CombineCarrierAndFlightCode");
            }
            return flightNumber;
        }

        /*Not in use*/
        //private DataTable ScheduleDataTable()
        //{
        //    var dc1 = new DataColumn();
        //    dc1.Caption = "MessageType";
        //    dc1.ColumnName = "MessageType";
        //    dc1.DataType = Type.GetType("System.String");

        //    var dc2 = new DataColumn();
        //    dc2.Caption = "MessageIdentifier";
        //    dc2.ColumnName = "MessageIdentifier";
        //    dc2.DataType = Type.GetType("System.String");

        //    var dc3 = new DataColumn();
        //    dc3.Caption = "OriginalMessage";
        //    dc3.ColumnName = "OriginalMessage";
        //    dc3.DataType = Type.GetType("System.String");

        //    var dc4 = new DataColumn();
        //    dc4.Caption = "FlightNo";
        //    dc4.ColumnName = "FlightNo";
        //    dc4.DataType = Type.GetType("System.String");

        //    var dc5 = new DataColumn();
        //    dc5.Caption = "RegistrationNo";
        //    dc5.ColumnName = "RegistrationNo";
        //    dc5.DataType = Type.GetType("System.String");

        //    var dc6 = new DataColumn();
        //    dc6.Caption = "AircraftType";
        //    dc6.ColumnName = "AircraftType";
        //    dc6.DataType = Type.GetType("System.String");

        //    var dc7 = new DataColumn();
        //    dc7.Caption = "PassangerReservation";
        //    dc7.ColumnName = "PassangerReservation";
        //    dc7.DataType = Type.GetType("System.Int16");

        //    var dc8 = new DataColumn();
        //    dc8.Caption = "AirportOfDepart";
        //    dc8.ColumnName = "AirportOfDepart";
        //    dc8.DataType = Type.GetType("System.String");

        //    var dc9 = new DataColumn();
        //    dc9.Caption = "AirportOfArrival";
        //    dc9.ColumnName = "AirportOfArrival";
        //    dc9.DataType = Type.GetType("System.String");

        //    var dc10 = new DataColumn();
        //    dc10.Caption = "SchedDateOfArrival";
        //    dc10.ColumnName = "SchedDateOfArrival";
        //    dc10.DataType = Type.GetType("System.String");

        //    var dc11 = new DataColumn();
        //    dc11.Caption = "SchedDateOfDepart";
        //    dc11.ColumnName = "SchedDateOfDepart";
        //    dc11.DataType = Type.GetType("System.String");

        //    var dc12 = new DataColumn();
        //    dc12.Caption = "actualDateOfArrival";
        //    dc12.ColumnName = "actualDateOfArrival";
        //    dc12.DataType = Type.GetType("System.String");

        //    var dc13 = new DataColumn();
        //    dc13.Caption = "ServiceType";
        //    dc13.ColumnName = "ServiceType";
        //    dc13.DataType = Type.GetType("System.String");

        //    var dc14 = new DataColumn();
        //    dc14.Caption = "Date";
        //    dc14.ColumnName = "Date";
        //    dc14.DataType = Type.GetType("System.String");

        //    var dc15 = new DataColumn();
        //    dc15.Caption = "SrNo";
        //    dc15.ColumnName = "SrNo";
        //    dc15.DataType = Type.GetType("System.String");

        //    var dc16 = new DataColumn();
        //    dc16.Caption = "TailNo";
        //    dc16.ColumnName = "TailNo";
        //    dc16.DataType = Type.GetType("System.String");

        //    var dc17 = new DataColumn();
        //    dc17.Caption = "MessageTimeMode";
        //    dc17.ColumnName = "MessageTimeMode";
        //    dc17.DataType = Type.GetType("System.String");



        //    var dt = new DataTable();
        //    dt.Columns.Add(dc1);
        //    dt.Columns.Add(dc2);
        //    dt.Columns.Add(dc3);
        //    dt.Columns.Add(dc4);
        //    dt.Columns.Add(dc5);
        //    dt.Columns.Add(dc6);
        //    dt.Columns.Add(dc7);
        //    dt.Columns.Add(dc8);
        //    dt.Columns.Add(dc9);
        //    dt.Columns.Add(dc10);
        //    dt.Columns.Add(dc11);
        //    dt.Columns.Add(dc12);
        //    dt.Columns.Add(dc13);
        //    dt.Columns.Add(dc14);
        //    dt.Columns.Add(dc15);
        //    dt.Columns.Add(dc16);
        //    dt.Columns.Add(dc17);

        //    return dt;
        //}

        private DataTable CreateFlightInfoDataTable()
        {
            DataTable dtFlightInfo = new DataTable();
            try
            {
                dtFlightInfo.Columns.Add("FlightNumber", typeof(string));
                dtFlightInfo.Columns.Add("AirportOfDepart", typeof(string));
                dtFlightInfo.Columns.Add("AirportOfArrival", typeof(string));
                dtFlightInfo.Columns.Add("SchDateOfDeparture", typeof(string));
                dtFlightInfo.Columns.Add("SchDateOfArrival", typeof(string));
                dtFlightInfo.Columns.Add("SchedTimeOfDepart", typeof(string));
                dtFlightInfo.Columns.Add("SchedTimeOfArrival", typeof(string));
                dtFlightInfo.Columns.Add("ServiceType", typeof(string));
                dtFlightInfo.Columns.Add("AircraftType", typeof(string));
                dtFlightInfo.Columns.Add("RegistrationNo", typeof(string));
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on CreateFlightInfoDataTable");
            }
            return dtFlightInfo;
        }

        /*Not in use*/
        //private String getDate(int p)
        //{
        //    try
        //    {
        //        DateTime dt = DateTime.Now;

        //        if (!String.IsNullOrEmpty(schedDateOfArrival) && schedDateOfArrival.Trim().Length == 7)
        //        {
        //            dt = DateTime.ParseExact(schedDateOfArrival, "ddMMMyy", null);
        //        }
        //        else
        //        {

        //            if (!String.IsNullOrEmpty(schedDateOfDepart) && schedDateOfDepart.Trim().Length == 7)
        //            {
        //                dt = DateTime.ParseExact(schedDateOfDepart, "ddMMMyy", null);
        //            }

        //            else if (!String.IsNullOrEmpty(schedDateOfArrival) && schedDateOfArrival.Trim().Length > 5)
        //            {
        //                dt = Convert.ToDateTime(schedDateOfArrival);
        //            }
        //            else if (!String.IsNullOrEmpty(schedDateOfDepart) && schedDateOfDepart.Trim().Length > 7)
        //            {
        //                dt = DateTime.ParseExact(schedDateOfDepart, "ddMMMyy", null);
        //            }

        //        }
        //        return new DateTime(DateTime.Now.Year, dt.Month, p).ToShortDateString();
        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //        _logger.LogError(ex, "Error on getDate(input is integer type)");
        //        return new DateTime(DateTime.Now.Year, DateTime.Now.Month, p).ToShortDateString();
        //    }
        //}

        private String getDate(string schedDateOfArrival)
        {
            DateTime tempDate = new DateTime(2001, 1, 1);
            try
            {

                if (schedDateOfArrival.Trim().Length == 7)
                    return DateTime.ParseExact(schedDateOfArrival, "ddMMMyy", null).ToString("yyyy-MM-dd hh:mm:ss");
                if (schedDateOfArrival.Trim().Length > 5)
                    return schedDateOfArrival;
                if (schedDateOfArrival.Trim().Length == 2)
                {
                    return DateTime.ParseExact(DateTime.Now.Year.ToString("0000") + "-" + DateTime.Now.Month.ToString("00") + "-" + schedDateOfArrival, "yyyy-MM-dd", null).ToString("yyyy-MM-dd");
                }
                if (schedDateOfArrival.Trim().Length == 5)
                {
                    // int month = DateTime.ParseExact(schedDateOfArrival.Substring(2, 3), "MMMM", null);

                    string Mnt = DateTime.ParseExact(schedDateOfArrival.Substring(2, 3), "MMM", System.Globalization.CultureInfo.InvariantCulture).Month.ToString("00");

                    return DateTime.ParseExact(DateTime.Now.Year.ToString("0000") + "-" + Mnt + "-" + schedDateOfArrival.Substring(0, 2), "yyyy-MM-dd", null).ToString("yyyy-MM-dd");
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on getDate(input schedDateOfArrival is string type)");
            }
            return tempDate.ToString("yyyy-MM-dd hh:mm:ss");
        }

        /*Not in use*/
        //private String getDateDDMMMYY(string schedDateOfArrival)
        //{
        //    try
        //    {

        //        if (schedDateOfArrival.Trim().Length == 7)
        //            return schedDateOfArrival;
        //        else if (schedDateOfArrival.Trim().Length == 5)
        //            return schedDateOfArrival + DateTime.Now.ToString("yy");
        //        else if (schedDateOfArrival.Trim().Length == 2)
        //        {
        //            string currentMonth = GetMonthNameByNumber(DateTime.Now.Month);
        //            return schedDateOfArrival + currentMonth + DateTime.Now.ToString("yy");
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //        _logger.LogError(ex, "Error on getDateDDMMMYY");
        //    }
        //    return "";
        //}

        ///*Not in use*/
        //private string GetMonthNameByNumber(int m)
        //{
        //    string res = string.Empty;
        //    switch (m)
        //    {
        //        case 1:
        //            res = "JAN";
        //            break;
        //        case 2:
        //            res = "FEB";
        //            break;
        //        case 3:
        //            res = "MAR";
        //            break;
        //        case 4:
        //            res = "APR";
        //            break;
        //        case 5:
        //            res = "MAY";
        //            break;
        //        case 6:
        //            res = "JUN";
        //            break;
        //        case 7:
        //            res = "JUL";
        //            break;
        //        case 8:
        //            res = "AUG";
        //            break;
        //        case 9:
        //            res = "SEP";
        //            break;
        //        case 10:
        //            res = "OCT";
        //            break;
        //        case 11:
        //            res = "NOV";
        //            break;
        //        case 12:
        //            res = "DEC";
        //            break;
        //    }
        //    return res;
        //}

        private void SetVariablesToDefaultValues()
        {
            try
            {
                messageIdentifier = string.Empty;
                messageTimeMode = "UTC";
                airportOfArrival = string.Empty;
                airportOfDepart = string.Empty;
                aircraftType = string.Empty;
                passangerReservation = string.Empty;
                flightNo = string.Empty;
                onwordFlight = string.Empty;
                schedDateOfDepart = string.Empty;
                schedDateOfArrival = string.Empty;
                schedTimeOfDepart = string.Empty;
                schedTimeOfArrival = string.Empty;
                registrationNo = string.Empty;
                serviceType = string.Empty;
                legNumber = 0;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on SetVariablesToDefaultValues");
            }
        }

        /*Not in use*/
        //private static string? getConnectionString()
        //{
        //    try
        //    {
        //        string strConnectionString = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        //        if (strConnectionString == null)
        //        {
        //            strConnectionString = "";
        //        }
        //        return strConnectionString;
        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //        _staticLogger?.LogError(ex, "Error on getConnectionString");
        //        return null;
        //    }
        //}
    }

    public class MVT
    {
        private readonly ILogger<ASM> _logger;//instance logger
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly GenericFunction _genericFunction;
        private readonly FDMMessageProcessor _fDMMessageProcessor;

        #region Constructor
        public MVT(ISqlDataHelperFactory sqlDataHelperFactory, ILogger<ASM>? staticLogger, ILogger<ASM> logger, GenericFunction genericFunction, FDMMessageProcessor fDMMessageProcessor)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
            _fDMMessageProcessor = fDMMessageProcessor;
        }
        #endregion
        public String messageType = "MVT";
        public String messageIdentifier = "";
        public String originalMessage = String.Empty;
        public String airportOfArrival = String.Empty;
        public String airportOfDepart = String.Empty;
        public String nextDestAirport = String.Empty;
        public String flightNo = String.Empty;
        public string schedDateOfDepart = string.Empty;
        public string schedDateOfArrival = String.Empty;
        public String schedTimeOfDepart = String.Empty;
        public String schedTimeOfArrival = String.Empty;
        public String actualDateOfDepart = String.Empty;
        public String actualDateOfArrival = String.Empty;
        public String actualTimeOfDepart = String.Empty;
        public String actualTimeOfArrival = String.Empty;
        public String onBlockTime = String.Empty;
        public String registrationNo = String.Empty;
        public String passangerCount = "0";
        public String delayCode = String.Empty;
        public String nextInformation = String.Empty;
        public String suppelementoryInfo = String.Empty;
        public String errorMsg = String.Empty;

        private string getDate(string schedDateOfArrival)
        {
            DateTime tempDate = new DateTime(2001, 1, 1);
            try
            {
                if (schedDateOfArrival.Trim().Length == 7)
                    return DateTime.ParseExact(schedDateOfArrival, "ddMMMyy", null).ToString("yyyy-MM-dd hh:mm:ss");
                if (schedDateOfArrival.Trim().Length > 5)
                    return schedDateOfArrival;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on getDate MVT");
            }
            return tempDate.ToString("yyyy-MM-dd hh:mm:ss");
        }

        //public MVT ToMVT(string strMessage, int srno, string stroriginalMessage, string strMessageFrom, out string msgType, out bool isProcessFlag)
        public async Task<(string msgType, bool isProcessFlag)> ToMVT(string strMessage, int srno, string stroriginalMessage, string strMessageFrom)
        {

            string msgType = "MVT";
            bool isProcessFlag = false;
            string tempMessageIdentifier = string.Empty;
            try
            {
                originalMessage = strMessage;
                string[] arrLine = strMessage.Split('$');
                if (arrLine.Length < 3)
                {
                    errorMsg = "Message is not in correct format.Cant parse correctly.";
                    return (msgType, isProcessFlag);
                }
                messageType = arrLine[0];

                for (int i = 2; i < arrLine.Length; i++)
                {
                    messageIdentifier = arrLine[i].Trim().Substring(0, 2);

                    if (messageIdentifier.ToUpper() == "AA")
                    {
                        tempMessageIdentifier = "AA";
                        ParseArrival(arrLine);
                    }
                    else if (messageIdentifier.ToUpper() == "AD")
                    {
                        tempMessageIdentifier = "AD";
                        await ParseDeparture(arrLine);
                    }
                    else if (messageIdentifier.ToUpper() == "DL" || messageIdentifier.ToUpper() == "NI" || messageIdentifier.ToUpper() == "ED" || messageIdentifier.ToUpper() == "FR")
                    {
                        ParseDaelay(arrLine, tempMessageIdentifier);
                    }
                    else if (messageIdentifier.ToUpper() == "EA")
                    {
                        ParseEstimatedArrival(arrLine, tempMessageIdentifier);
                    }
                }

                if (tempMessageIdentifier != string.Empty)
                {
                    messageIdentifier = tempMessageIdentifier;

                    if (messageType.Trim().ToUpper() != "DIV")
                    {
                        msgType = messageType.Trim().ToUpper() + "/" + messageIdentifier.Trim().ToUpper();
                    }
                    else
                    {
                        msgType = messageType.Trim().ToUpper();
                    }
                }
                object[] QueryValues = {
                    messageType ,
                    messageIdentifier  ,
                    originalMessage ,
                    flightNo ,
                    registrationNo,
                    "" ,  // service
                    passangerCount ,
                    airportOfDepart ,
                    airportOfArrival ,
                    nextDestAirport,
                    getDate(schedDateOfArrival),
                    getDate(schedDateOfDepart),
                    getDate(actualDateOfArrival),
                    getDate(actualDateOfDepart),
                    schedTimeOfArrival,
                    schedTimeOfDepart,
                    actualTimeOfArrival,
                    actualTimeOfDepart,
                    onBlockTime,
                    "",//serviceType ,
                    delayCode ,
                    nextInformation ,
                    suppelementoryInfo,
                    DateTime.Now ,// DateTime.Now.ToShortDateString()
                    srno
                };

                isProcessFlag = await UpdateToDatatabse(QueryValues);

                if (isProcessFlag)
                {
                    DateTime FlightDate = Convert.ToDateTime(getDate(schedDateOfArrival));

                    try
                    {
                        if (msgType == "MVT/AD")
                        {
                            //FDMMessageProcessor FDM = new FDMMessageProcessor();
                            //FDM.GenerateFDMMessage(flightNo, FlightDate, airportOfDepart);
                            await _fDMMessageProcessor.GenerateFDMMessage(flightNo, FlightDate, airportOfDepart);
                        }
                    }
                    catch (Exception ex)
                    {
                        //clsLog.WriteLogAzure(ex);
                        //clsLog.WriteLogAzure("FDM message generation failed on MVT/AD! FlightNo: " + flightNo + " FlightDate: " + FlightDate.ToString() + " Time: " + DateTime.Now);

                        _logger.LogError(ex, "Error on getDate ToMVT UpdateToDatatabse");
                        _logger.LogError("FDM message generation failed on MVT/AD! FlightNo: \" + flightNo + \" FlightDate: \" + FlightDate.ToString() + \" Time: \" + DateTime.Now");
                    }
                }

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on getDate ToMVT");
            }
            return (msgType, isProcessFlag);
        }

        private void ParseEstimatedArrival(string[] arrLine, string actualMessageType)
        {
            try
            {
                //MVT
                //HA0239/30.N477HA.LIH
                //EA300339
                //SI

                #region Line 1
                if (actualMessageType == string.Empty)
                {
                    try
                    {
                        flightNo = arrLine[1].Substring(0, arrLine[1].IndexOf('/'));
                    }
                    catch (Exception ex)
                    {
                        //clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, "Error on ParseEstimatedArrival flightNo");
                    }
                    try
                    {
                        schedDateOfArrival = getDate(Convert.ToInt32(arrLine[1].Substring(arrLine[1].IndexOf('/') + 1, 2)));
                        actualDateOfDepart = getDate(Convert.ToInt32(arrLine[1].Substring(arrLine[1].IndexOf('/') + 1, 2)));
                    }
                    catch (Exception ex)
                    {
                        //clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, "Error on ParseEstimatedArrival schedDateOfArrival or actualDateOfDepart");
                    }
                    try
                    {
                        registrationNo = arrLine[1].Substring(arrLine[1].IndexOf('.') + 1, (arrLine[1].LastIndexOf('.') - arrLine[1].IndexOf('.') - 1));
                    }
                    catch (Exception ex)
                    {
                        //clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, "Error on ParseEstimatedArrival registrationNo");
                    }

                    try
                    {
                        airportOfArrival = arrLine[1].Substring(arrLine[1].LastIndexOf('.') + 1);
                    }
                    catch (Exception ex)
                    {
                        //clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, "Error on ParseEstimatedArrival airportOfArrival");
                    }
                }
                #endregion


                try
                {

                    for (int i = 2; i < arrLine.Length; i++)
                    {
                        try
                        {
                            if (arrLine[i].Length > 3 && arrLine[i].Substring(0, 2).ToUpper() == "SI")
                                suppelementoryInfo = arrLine[i].Substring(2);
                            if (arrLine[i].Length > 3 && arrLine[i].Substring(0, 2).ToUpper() == "DL")
                                delayCode = arrLine[i].Substring(2);
                            if (arrLine[i].Length > 3 && arrLine[i].Substring(0, 2).ToUpper() == "px")
                                passangerCount = arrLine[i].Substring(2);
                            if (arrLine[i].Length > 3 && arrLine[i].Substring(0, 3).ToUpper() == "PAX")
                                passangerCount = arrLine[i].Substring(3);

                            if (actualMessageType == string.Empty)
                            {
                                if (arrLine[i].Length > 3 && arrLine[i].Substring(0, 2).ToUpper() == "EA")    // Div Message
                                {
                                    if (arrLine[i].Trim().Contains(' '))
                                    {
                                        schedTimeOfArrival = arrLine[i].Substring(2, arrLine[i].IndexOf(' ') - 2);
                                        nextDestAirport = arrLine[i].Substring(arrLine[i].IndexOf(' '));
                                    }
                                    else
                                        schedTimeOfArrival = arrLine[i].Substring(2);
                                    if (schedTimeOfArrival.Length > 5)
                                    {
                                        schedDateOfArrival = getDate(Convert.ToInt32(schedTimeOfArrival.Substring(0, 2)));
                                        schedTimeOfArrival = schedTimeOfArrival.Substring(2);
                                    }
                                }
                                if (arrLine[i].Length > 4 && arrLine[i].Substring(0, 2).ToUpper() == "ED")
                                {
                                    schedDateOfDepart = getDate(Convert.ToInt32(arrLine[i].Substring(2, 2)));
                                    schedTimeOfDepart = arrLine[i].Substring(4);
                                }
                            }

                            if (arrLine[i].Length > 4 && arrLine[i].ToUpper().StartsWith("CONT"))    // Div Message
                            {
                                if (arrLine[i].Trim().Contains(' '))
                                {
                                    nextDestAirport = nextDestAirport + "/" + arrLine[i].Substring(arrLine[i].IndexOf(' '));
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            //clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, "Error on ParseEstimatedArrival airportOfArrival");
                        }
                    }
                }
                catch (Exception ex)
                {
                    //clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, "Error on ParseEstimatedArrival for loop section");
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on ParseEstimatedArrival");
            }
        }

        //Not in use
        //private void ParseDiversion(string[] arrLine)
        //{

        //}

        //Not in use
        //private void ParseReturnToRamp(string[] arrLine)
        //{
        //    throw new NotImplementedException();
        //}

        private void ParseDaelay(string[] arrLine, string actualMessageType)
        {
            try
            {
                //MVT
                //HA0181/29.N485HA.ITO
                //ED292325

                #region Line 1
                if (actualMessageType == string.Empty)
                {
                    flightNo = arrLine[1].Substring(0, arrLine[1].IndexOf('/'));

                    schedDateOfArrival = getDate(Convert.ToInt32(arrLine[1].Substring(arrLine[1].IndexOf('/') + 1, 2)));

                    registrationNo = arrLine[1].Substring(arrLine[1].IndexOf('.') + 1, (arrLine[1].LastIndexOf('.') - arrLine[1].IndexOf('.') - 1));

                    airportOfDepart = arrLine[1].Substring(arrLine[1].LastIndexOf('.') + 1);
                }
                #endregion

                for (int i = 2; i < arrLine.Length; i++)
                {
                    if (arrLine[i].Length > 3 && arrLine[i].Substring(0, 2).ToUpper() == "SI")
                    {
                        suppelementoryInfo = arrLine[i].Substring(2);
                    }
                    else if (arrLine[i].Length > 3 && arrLine[i].Substring(0, 2).ToUpper() == "ED" && actualMessageType == string.Empty)
                    {
                        schedDateOfDepart = getDate(Convert.ToInt32(arrLine[i].Substring(2, 2)));
                        schedTimeOfDepart = arrLine[i].Substring(4);
                    }
                    else if (arrLine[i].Length > 3 && arrLine[i].Substring(0, 2).ToUpper() == "NI")
                    {
                        nextInformation = arrLine[i].Substring(2);
                    }
                    else if (arrLine[i].Length > 3 && arrLine[i].Substring(0, 2).ToUpper() == "DL")
                    {
                        delayCode = arrLine[i].Length > 4 ? arrLine[i].Substring(2, 2) : arrLine[i].Substring(2);
                    }
                    else if (arrLine[i].Length > 3 && arrLine[i].Substring(0, 2).ToUpper() == "FR")
                    {
                        delayCode = arrLine[i].Substring(2);
                    }
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on ParseDaelay");
            }
        }

        private async Task ParseDeparture(string[] arrLine)
        {
            try
            {
                //                MVT
                //HA0616/30.N805HC.HNL
                //AD300003/0003 EA300033 LNY
                //SI

                string[] arrFlightInfo;
                #region Line 1
                //try
                //{
                //    flightNo = arrLine[1].Substring(0, arrLine[1].IndexOf('/'));
                //}
                //catch (Exception ex)
                //{
                //    clsLog.WriteLogAzure(ex);
                //}
                //try
                //{
                //    schedDateOfArrival = getDate(Convert.ToInt32(arrLine[1].Substring(arrLine[1].IndexOf('/') + 1, 2)));
                //}
                //catch (Exception ex)
                //{
                //    clsLog.WriteLogAzure(ex);
                //}
                //try
                //{
                //    registrationNo = arrLine[1].Substring(arrLine[1].IndexOf('.') + 1, (arrLine[1].LastIndexOf('.') - arrLine[1].IndexOf('.') - 1));
                //}
                //catch (Exception ex)
                //{
                //    clsLog.WriteLogAzure(ex);
                //}

                //try
                //{
                //    airportOfDepart = arrLine[1].Substring(arrLine[1].LastIndexOf('.') + 1);
                //}
                //catch (Exception ex)
                //{
                //    clsLog.WriteLogAzure(ex);
                //}
                //try
                //{
                //    string originalADString = arrLine[2].Substring(0, (arrLine[2].IndexOf('/'))).Trim();
                //    string replaceString = originalADString.Replace(" ", "");
                //    arrLine[2] = arrLine[2].Replace(originalADString, replaceString);
                //    actualTimeOfDepart = arrLine[2].Substring(2, (arrLine[2].IndexOf('/') - 2)).Trim();
                //    if (actualTimeOfDepart.Length > 4)
                //    {
                //        actualDateOfDepart = getDate(Convert.ToInt32(actualTimeOfDepart.Substring(0, 2)));
                //        if (actualTimeOfDepart.Length > 4)
                //            actualTimeOfDepart = actualTimeOfDepart.Substring(2, 4);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    clsLog.WriteLogAzure(ex);
                //}
                //try
                //{
                //    if (arrLine[2].Trim().Contains(' '))
                //    {
                //        if (arrLine[2].IndexOf('/') < 0)
                //        {
                //            onBlockTime = arrLine[2].Substring(2, arrLine[2].Length - arrLine[2].Trim().IndexOf(' ') - 1);
                //        }
                //        else
                //        {
                //            onBlockTime = arrLine[2].Substring(arrLine[2].IndexOf('/') + 1, arrLine[2].Trim().IndexOf(' ') - arrLine[2].IndexOf('/') - 1);
                //        }
                //    }
                //    else
                //    {
                //        onBlockTime = arrLine[2].Substring(arrLine[2].IndexOf('/') + 1);
                //    }
                //    if (onBlockTime.Length > 5)
                //    {
                //        actualDateOfDepart = getDate(Convert.ToInt32(onBlockTime.Substring(0, 2)));
                //        onBlockTime = onBlockTime.Substring(2);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    clsLog.WriteLogAzure(ex);
                //}
                //try
                //{
                //    if (arrLine[2].Trim().Contains("EA"))
                //    {
                //        schedTimeOfArrival = arrLine[2].Substring(arrLine[2].IndexOf("EA") + 2, arrLine[2].Trim().LastIndexOf(' ') - arrLine[2].IndexOf("EA") - 2).Trim();
                //        if (schedTimeOfArrival.Length > 5)
                //        {
                //            schedDateOfArrival = getDate(Convert.ToInt32(schedTimeOfArrival.Substring(0, 2)));
                //            schedTimeOfArrival = schedTimeOfArrival.Substring(2);
                //        }
                //        nextDestAirport = arrLine[2].Trim().Substring(arrLine[2].Trim().LastIndexOf(' ')).Trim();
                //    }
                //}
                //catch (Exception ex)
                //{
                //    clsLog.WriteLogAzure(ex);
                //}
                //try
                //{
                //    nextDestAirport = arrLine[2].Trim().Substring(arrLine[2].Trim().LastIndexOf(' ')).Trim();
                //}
                //catch (Exception ex)
                //{
                //    clsLog.WriteLogAzure(ex);
                //}
                #endregion
                flightNo = string.Empty;
                actualDateOfDepart = string.Empty;
                registrationNo = string.Empty;
                airportOfDepart = string.Empty;
                actualTimeOfDepart = string.Empty;
                schedTimeOfArrival = string.Empty;
                onBlockTime = string.Empty;
                nextDestAirport = string.Empty;

                if (arrLine.Length > 2)
                {
                    arrFlightInfo = arrLine[1].Split('/');
                    if (arrFlightInfo.Length > 1 && arrFlightInfo[1].Split('.').Length > 2)
                    {
                        flightNo = arrFlightInfo[0].Trim();
                        actualDateOfDepart = getDate(Convert.ToInt32(arrFlightInfo[1].Split('.')[0].Trim()));
                        registrationNo = arrFlightInfo[1].Split('.')[1].Trim();
                        airportOfDepart = arrFlightInfo[1].Split('.')[2].Trim();
                    }
                    else
                    {
                        ////
                    }
                    string[] arrDepartureInfo;
                    arrDepartureInfo = arrLine[2].Split('/');

                    if (arrDepartureInfo.Length > 1)
                        actualTimeOfDepart = arrDepartureInfo[0].Trim().Replace(" ", "").Substring(2);
                    else
                        actualTimeOfDepart = arrLine[2].Split(' ')[0].Substring(2);

                    if (actualTimeOfDepart.Length > 4)
                    {
                        actualDateOfDepart = getDate(Convert.ToInt32(actualTimeOfDepart.Substring(0, 2)));
                        if (actualTimeOfDepart.Length > 4)
                            actualTimeOfDepart = actualTimeOfDepart.Substring(2, 4);
                    }
                    if (arrDepartureInfo.Length > 1)
                    {
                        onBlockTime = arrDepartureInfo[1].Split(' ')[0];
                        if (onBlockTime.Length > 4)
                            onBlockTime = onBlockTime.Substring(2, 4);
                    }
                    if (arrLine[2].Trim().Contains("EA"))
                    {
                        schedTimeOfArrival = arrLine[2].Substring(arrLine[2].IndexOf("EA") + 2, arrLine[2].Trim().LastIndexOf(' ') - arrLine[2].IndexOf("EA") - 2).Trim();
                        if (schedTimeOfArrival.Length > 5)
                        {
                            schedDateOfArrival = getDate(Convert.ToInt32(schedTimeOfArrival.Substring(0, 2)));
                            schedTimeOfArrival = schedTimeOfArrival.Substring(2);
                        }
                        nextDestAirport = arrLine[2].Trim().Substring(arrLine[2].Trim().LastIndexOf(' ')).Trim();
                    }
                }
                else
                {
                    //GenericFunction genericFunction = new GenericFunction();
                    //genericFunction.UpdateErrorMessageToInbox(111, "Invalid message format");
                    await _genericFunction.UpdateErrorMessageToInbox(111, "Invalid message format");
                }
                try
                {
                    for (int i = 3; i < arrLine.Length; i++)
                    {
                        try
                        {
                            if (arrLine[i].Length > 3 && arrLine[i].Substring(0, 2).ToUpper() == "SI")
                                suppelementoryInfo = arrLine[i].Substring(2);
                            if (arrLine[i].Length > 3 && arrLine[i].Substring(0, 2).ToUpper() == "DL")
                                delayCode = arrLine[i].Substring(2);
                            if (arrLine[i].Length > 3 && (arrLine[i].Substring(0, 3).ToUpper() == "PAX" || arrLine[i].Substring(0, 2).ToUpper() == "PX"))
                                passangerCount = arrLine[i].Substring(3);
                        }
                        catch (Exception ex)
                        {
                            //clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, "Error on ParseDeparture for loop section");
                        }
                    }

                }
                catch (Exception ex)
                {
                    //clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, "Error on ParseEstimatedArrival for loop section");
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on ParseEstimatedArrival");
            }
        }

        private void ParseArrival(string[] arrLine)
        {
            try
            {
                //MVT
                //HA0033/29.N385HA.OGG
                //AA2325/292334
                //SI
                #region Line 1

                flightNo = arrLine[1].Substring(0, arrLine[1].IndexOf('/'));

                schedDateOfArrival = getDate(Convert.ToInt32(arrLine[1].Substring(arrLine[1].IndexOf('/') + 1, 2)));

                registrationNo = arrLine[1].Substring(arrLine[1].IndexOf('.') + 1, (arrLine[1].LastIndexOf('.') - arrLine[1].IndexOf('.') - 1));

                airportOfArrival = arrLine[1].Substring(arrLine[1].LastIndexOf('.') + 1);

                #endregion

                actualTimeOfArrival = arrLine[2].Substring(2, (arrLine[2].IndexOf('/') - 2));
                if (actualTimeOfArrival.Length > 5)
                {
                    actualDateOfArrival = getDate(Convert.ToInt32(actualTimeOfArrival.Substring(0, 2)));
                    actualTimeOfArrival = actualTimeOfArrival.Substring(2);
                }

                onBlockTime = arrLine[2].Substring(arrLine[2].IndexOf('/') + 1);
                if (onBlockTime.Length > 5)
                {
                    actualDateOfArrival = getDate(Convert.ToInt32(onBlockTime.Substring(0, 2)));
                    onBlockTime = onBlockTime.Substring(2);
                }

                for (int i = 3; i < arrLine.Length; i++)
                {
                    if (arrLine[i].Length > 3 && arrLine[i].Substring(0, 2).ToUpper() == "SI")
                        suppelementoryInfo = arrLine[i].Substring(2);
                }

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on ParseArrival");
            }
        }

        private string? getDate(int p)
        {
            try
            {
                return new DateTime(DateTime.Now.Year, DateTime.Now.Month, p).ToString("ddMMMyy");
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on getDate MVT Class");
            }
            return null;
        }

        /*Not in use*/
        //private static string? getConnectionString()
        //{
        //    try
        //    {
        //        string strConnectionString = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        //        if (strConnectionString == null)
        //        {
        //            strConnectionString = "";
        //        }
        //        return strConnectionString;
        //    }
        //    catch (Exception ex)
        //    {
        //        _staticLogger?.LogError(ex, "Error on getConnectionString MVT Class");
        //        return null;
        //    }
        //}

        public async Task<bool> UpdateToDatatabse(object[] QueryValues)
        {
            try
            {

                //string[] QueryNames = new string[25];
                //SqlDbType[] QueryTypes = new SqlDbType[25];
                //SQLServer db = new SQLServer();

                //QueryNames[0] = "MsgType";
                //QueryNames[01] = "msgId";
                //QueryNames[02] = "msgBody";
                //QueryNames[03] = "flightNumber";
                //QueryNames[04] = "tailNumber";
                //QueryNames[05] = "aircraftType";
                //QueryNames[06] = "passangerCount";
                //QueryNames[07] = "airportOfDeparture";
                //QueryNames[08] = "airportOfArrival";
                //QueryNames[09] = "nextDestAirport";
                //QueryNames[10] = "schdDateOfArrival";
                //QueryNames[011] = "schdDateOfDeparture";
                //QueryNames[012] = "actualDateOfArrival";
                //QueryNames[013] = "actualDateOfDeparture";
                //QueryNames[014] = "schdTimeOfArrival";
                //QueryNames[015] = "schdTimeOfDeparture";
                //QueryNames[016] = "actualTimeOfArrival";
                //QueryNames[017] = "actualTimeOfDeparture";
                //QueryNames[018] = "onBlockTime";
                //QueryNames[019] = "serviceType";
                //QueryNames[20] = "delayCode";
                //QueryNames[021] = "nextInformation";
                //QueryNames[022] = "supplemtoryInfo";
                //QueryNames[023] = "createdOn";
                //QueryNames[024] = "srno";


                //QueryTypes[0] = SqlDbType.VarChar;
                //QueryTypes[01] = SqlDbType.VarChar;
                //QueryTypes[02] = SqlDbType.VarChar;
                //QueryTypes[03] = SqlDbType.VarChar;
                //QueryTypes[04] = SqlDbType.VarChar;
                //QueryTypes[05] = SqlDbType.VarChar;
                //QueryTypes[06] = SqlDbType.Int;
                //QueryTypes[07] = SqlDbType.VarChar;
                //QueryTypes[08] = SqlDbType.VarChar;
                //QueryTypes[09] = SqlDbType.VarChar;
                //QueryTypes[10] = SqlDbType.DateTime;
                //QueryTypes[011] = SqlDbType.DateTime;
                //QueryTypes[012] = SqlDbType.DateTime;
                //QueryTypes[013] = SqlDbType.DateTime;
                //QueryTypes[014] = SqlDbType.VarChar;
                //QueryTypes[015] = SqlDbType.VarChar;
                //QueryTypes[016] = SqlDbType.VarChar;
                //QueryTypes[017] = SqlDbType.VarChar;
                //QueryTypes[018] = SqlDbType.VarChar;

                //QueryTypes[019] = SqlDbType.VarChar;
                //QueryTypes[20] = SqlDbType.VarChar;
                //QueryTypes[021] = SqlDbType.VarChar;
                //QueryTypes[022] = SqlDbType.VarChar;
                //QueryTypes[023] = SqlDbType.DateTime;
                //QueryTypes[024] = SqlDbType.Int;


                //return db.ExecuteProcedure("Messaging.uspAddFlightMovementDetails", QueryNames, QueryTypes, QueryValues);

                var parameters = new SqlParameter[]
                {
                    new("@MsgType", SqlDbType.VarChar) { Value = "" },
                    new("@msgId", SqlDbType.VarChar) { Value = ""},
                    new("@msgBody", SqlDbType.VarChar) { Value = "" },
                    new("@flightNumber", SqlDbType.VarChar) { Value = "" },
                    new("@tailNumber", SqlDbType.VarChar) { Value = "" },
                    new("@aircraftType", SqlDbType.VarChar) { Value = "" },
                    new("@passangerCount", SqlDbType.Int) { Value = 0 },
                    new("@airportOfDeparture", SqlDbType.VarChar) { Value = "" },
                    new("@airportOfArrival", SqlDbType.VarChar) { Value = "" },
                    new("@nextDestAirport", SqlDbType.VarChar) { Value = "" },
                    new("@schdDateOfArrival", SqlDbType.DateTime) { Value = DBNull.Value },
                    new("@schdDateOfDeparture", SqlDbType.DateTime) { Value = DBNull.Value },
                    new("@actualDateOfArrival", SqlDbType.DateTime) { Value = DBNull.Value },
                    new("@actualDateOfDeparture", SqlDbType.DateTime) { Value = DBNull.Value },
                    new("@schdTimeOfArrival", SqlDbType.VarChar) { Value = "" },
                    new("@schdTimeOfDeparture", SqlDbType.VarChar) { Value = "" },
                    new("@actualTimeOfArrival", SqlDbType.VarChar) { Value = "" },
                    new("@actualTimeOfDeparture", SqlDbType.VarChar) { Value = "" },
                    new("@onBlockTime", SqlDbType.VarChar) { Value = "" },
                    new("@serviceType", SqlDbType.VarChar) { Value = "" },
                    new("@delayCode", SqlDbType.VarChar) { Value = "" },
                    new("@nextInformation", SqlDbType.VarChar) { Value = "" },
                    new("@supplemtoryInfo", SqlDbType.VarChar) { Value = "" },
                    new("@createdOn", SqlDbType.DateTime) { Value = DBNull.Value },
                    new("@srno", SqlDbType.Int) { Value = 0 }
                };
                for (int i = 0; i < QueryValues.Length; i++)
                {
                    parameters[i].Value = QueryValues[i];
                }
                return await _readWriteDao.ExecuteNonQueryAsync("Messaging.uspAddFlightMovementDetails", parameters);

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on UpdateToDatatabse MVT Class");
            }
            return false;
        }
    }
}
