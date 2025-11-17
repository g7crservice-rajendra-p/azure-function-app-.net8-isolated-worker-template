using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;

namespace QidWorkerRole
{
    public class SSM
    {
        public string messageType = string.Empty;
        public string messageIdentifier = string.Empty, messageTimeMode = string.Empty, frequency = string.Empty;
        public string originalMessage = string.Empty;
        public string airportOfArrival = string.Empty;
        public string airportOfDepart = string.Empty;
        public string aircraftType = string.Empty;
        public string passangerReservation = string.Empty;
        public string flightNo = string.Empty, SecondfltNo = string.Empty;
        public string schedDateOfDepart = string.Empty;
        public string schedDateOfArrival = string.Empty;
        public string schedTimeOfDepart = string.Empty;
        public string schedTimeOfArrival = string.Empty;
        public string registrationNo = string.Empty;
        public string serviceType = string.Empty;
        public int dateVariationDep = 0;
        public int dateVariationArr = 0;
        public string newFlightNumber = string.Empty;
        public int legNumber = 1;
        public string POL = string.Empty;
        public string[] arrMonths = new string[] { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
        public string[] arrNEW = new string[] { "NEW" };
        public string[] arrCNL = new string[] { "CNL" };
        public string[] arrRIN = new string[] { "RIN" };
        public string[] arrRPL = new string[] { "RPL", "RPL/ADM", "RPL/ADM/CON/EQT/TIM" };
        public string[] arrADM = new string[] { "ADM" };
        public string[] arrEQT = new string[] { "EQT", "CON", "EQT/ADM", "EQT/ADM/CON" };
        public string[] arrRRT = new string[] { "RRT", "RRT/TIM", "RRT/ADM/TIM", "RPL/RRT/TIM", "RPL/ADM/RRT/TIM" };
        public string[] arrTIM = new string[] { "TIM", "TIM/ADM" };
        public string[] arrFLT = new string[] { "FLT" };


        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<SSM> _logger;
        private readonly GenericFunction _genericFunction;
        private readonly ASM _asm;


        #region Constructor
        public SSM(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<SSM> logger,
            GenericFunction genericFunction,
            ASM asm)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
            _asm = asm;
        }
        #endregion
        public void ToSSM(string strMessage, int srno, string strOriginalMessage, string strMessageFrom, out bool isProcessFlag)
        {
            isProcessFlag = false;
            try
            {
                string[] arrLine;
                arrLine = Array.ConvertAll(strMessage.Split('$'), p => p.Trim());
                originalMessage = strMessage.Replace("$", "\r\n");
                messageType = arrLine[0];

                if (arrLine.Length < 3 || messageType != "SSM")
                    return;

                string[] arrActionIndentifier = new string[arrLine.Length];
                for (int i = 0; i < arrLine.Length; i++)
                    arrActionIndentifier[i] = arrLine[i].Split(' ')[0].Trim();

                if (arrActionIndentifier.Intersect(arrNEW).Any())
                    parseNEW(arrLine, srno);//parseNEWRevised(arrLine, srno, originalMessage);
                else if (arrActionIndentifier.Intersect(arrCNL).Any())
                    parseCNL(arrLine, srno);
                else if (arrActionIndentifier.Intersect(arrRPL).Any())
                    parseRPL(arrLine, srno);
                else if (arrActionIndentifier.Intersect(arrTIM).Any())
                    parseTIM(arrLine, srno);
                else if (arrActionIndentifier.Intersect(arrFLT).Any())
                    parseFLT(arrLine, srno);
                else if (arrActionIndentifier.Intersect(arrADM).Any())
                    parseADM(arrLine, srno);
                else if (arrActionIndentifier.Intersect(arrEQT).Any())
                {
                    string messageID = "EQT";
                    messageID = arrActionIndentifier.Intersect(arrEQT).Count() > 0 && arrActionIndentifier.Intersect(arrEQT).Single().Trim().Length > 2 && arrActionIndentifier.Intersect(arrEQT).Single().Trim() == "CON" ? "CON" : "EQT";
                    parseEQT(arrLine, srno, messageID);
                }
                else
                {
                    //GenericFunction genericFunction = new GenericFunction();

                    _genericFunction.UpdateErrorMessageToInbox(srno, "Un-Supported SSM Message", "SSM", true, originalMessage);
                    return;
                }
                isProcessFlag = true;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private async Task parseNEW(string[] arrLine, int srno)
        {
            try
            {
                SetVariablesToDefaultValues();
                legNumber = 1;
                int scheduleID = 0;
                string thirdLine = string.Empty, scheduleIDs = string.Empty;
                int indxFlightInfo = 0;
                messageIdentifier = "NEW";

                for (int i = 0; i < arrLine.Length; i++)
                {
                    if (arrLine[i].Trim() == "LT")
                        messageTimeMode = "LT";
                    if (arrNEW.Contains(arrLine[i].Trim()))
                    {
                        indxFlightInfo = i + 1;
                        break;
                    }
                }
                flightNo = arrLine[indxFlightInfo].Split(' ')[0];
                DataTable dtFlightInfo = new DataTable();
                dtFlightInfo = CreateFlightInfoDataTable();
                DataTable dtPeriodFrequency = new DataTable();
                dtPeriodFrequency = CreatePeriodFrequencyDataTable();

                ///12NOV 30NOV 1234567/W2 6/SQ103C/1
                ///01DEC 31DEC 1234567/W2 6/SQ103C/1
                for (int i = indxFlightInfo + 1; i < arrLine.Length; i++)
                {
                    string[] strMessages = arrLine[i].Trim().Split(' ');
                    string frequencyDate = getDate(strMessages[0]);
                    bool isFrequencyDateValid = false;
                    DateTime frequencyDateTime;
                    if (DateTime.TryParse(frequencyDate, out frequencyDateTime) && frequencyDate != "2001-01-01 12:00:00")
                    {
                        isFrequencyDateValid = true;
                    }

                    if (strMessages[0].Trim().Length == 1 || !isFrequencyDateValid)
                        break;
                    if (ValidatePeriodRequencyLine(arrLine[i].Trim()))
                    {
                        schedDateOfDepart = strMessages[0];
                        schedDateOfArrival = strMessages[1];
                        frequency = strMessages[2].Split('/')[0];
                        dtPeriodFrequency.Rows.Add(
                            flightNo
                            , schedDateOfDepart
                            , schedDateOfArrival
                            , frequency
                        );
                    }
                    indxFlightInfo++;
                }
                for (int i = 0; i < dtPeriodFrequency.Rows.Count; i++)
                {
                    for (int j = indxFlightInfo + 1; j < arrLine.Length; j++)
                    {
                        string[] strMessages = arrLine[j].Trim().Split(' ');
                        if (strMessages[0] == "SI" || strMessages[0] == "//")
                            break;
                        int orgInfoLen = 0, destInfoLen = 0;
                        ///SIN0730/0/0735 KUL0820/0/0820
                        if (strMessages.Length > 1)
                        {
                            orgInfoLen = strMessages[0].Split('/')[0].Length;
                            destInfoLen = strMessages[1].Split('/')[0].Length;
                        }
                        if (orgInfoLen == 7 && destInfoLen == 7)
                        {
                            dateVariationDep = dateVariationArr = 0;
                            string[] source = strMessages[0].Split('/');
                            airportOfDepart = strMessages[0].Substring(0, 3);
                            schedTimeOfDepart = strMessages[0].Split('/')[0].Substring(3);
                            if (source.Length > 1 && (source[1].Length == 1 || source[1].Length == 2))
                            {
                                dateVariationDep = Convert.ToInt32(source[1].Substring(source[1].Length - 1));
                                dateVariationDep = source[1].Substring(0, 1) == "M" ? -dateVariationDep : dateVariationDep;
                            }
                            string[] dest = strMessages[1].Split('/');
                            airportOfArrival = strMessages[1].Substring(0, 3);
                            schedTimeOfArrival = strMessages[1].Split('/')[0].Substring(3);
                            if (dest.Length > 1 && (dest[1].Length == 1 || dest[1].Length == 2))
                            {
                                dateVariationArr = Convert.ToInt32(dest[1].Substring(dest[1].Length - 1));
                                dateVariationArr = dest[1].Substring(0, 1) == "M" ? -dateVariationArr : dateVariationArr;
                            }

                            schedDateOfDepart = dtPeriodFrequency.Rows[i]["SchDateOfDeparture"].ToString();
                            schedDateOfArrival = dtPeriodFrequency.Rows[i]["SchDateOfArrival"].ToString();
                            frequency = dtPeriodFrequency.Rows[i]["Frequency"].ToString();

                            dtFlightInfo.Rows.Add(
                                flightNo
                                , airportOfDepart
                                , airportOfArrival
                                , schedDateOfDepart
                                , schedDateOfArrival
                                , schedTimeOfDepart
                                , schedTimeOfArrival
                                , dateVariationDep
                                , dateVariationArr
                                , frequency
                                , serviceType
                                , aircraftType
                            );
                        }
                        ///C 330 F10Y100/FO.F10Y120
                        else if (strMessages[0].Trim().Length == 1)
                        {
                            serviceType = strMessages[0];
                            aircraftType = strMessages[1];
                        }
                    }
                }
                DataTable dtUniqueFlightInfo = RemoveDuplicatesRecords(dtFlightInfo);
                DataTable dtRearrangeFlightInfo = dtUniqueFlightInfo;
                dtUniqueFlightInfo = null;
                dtUniqueFlightInfo = RearrangeFlightInfo(dtRearrangeFlightInfo);
                flightNo = string.Empty;
                schedDateOfDepart = string.Empty;
                string POL = string.Empty, freqRotated = string.Empty, depDate = string.Empty, arrDate = string.Empty, orgDepTime = string.Empty;
                int legDepDayDiff = 0;
                //for (int i = 0; i < dtUniqueFlightInfo.Rows.Count; i++)
                for (int i = 0; i < 1; i++)
                {
                    bool isLastLeg = false;
                    if ((flightNo == string.Empty && schedDateOfDepart == string.Empty) ||
                        flightNo != dtUniqueFlightInfo.Rows[i]["FlightNumber"].ToString().Trim()
                        || schedDateOfDepart != dtUniqueFlightInfo.Rows[i]["SchDateOfDeparture"].ToString().Trim())
                    {
                        isLastLeg = false;
                        scheduleIDs = string.Empty;
                        frequency = string.Empty;
                        legNumber = 1;
                    }
                    if ((i == dtUniqueFlightInfo.Rows.Count - 1)
                    || (dtUniqueFlightInfo.Rows.Count - 1 > i && (dtUniqueFlightInfo.Rows[i]["FlightNumber"].ToString() != dtUniqueFlightInfo.Rows[i + 1]["FlightNumber"].ToString()
                    || dtUniqueFlightInfo.Rows[i]["SchDateOfDeparture"].ToString() != dtUniqueFlightInfo.Rows[i + 1]["SchDateOfDeparture"].ToString())))
                        isLastLeg = true;

                    flightNo = dtUniqueFlightInfo.Rows[i]["FlightNumber"].ToString();
                    airportOfDepart = dtUniqueFlightInfo.Rows[i]["AirportOfDepart"].ToString();
                    airportOfArrival = dtUniqueFlightInfo.Rows[i]["AirportOfArrival"].ToString();
                    schedDateOfDepart = dtUniqueFlightInfo.Rows[i]["SchDateOfDeparture"].ToString();
                    schedDateOfArrival = dtUniqueFlightInfo.Rows[i]["SchDateOfArrival"].ToString();
                    schedTimeOfDepart = dtUniqueFlightInfo.Rows[i]["SchedTimeOfDepart"].ToString();
                    schedTimeOfArrival = dtUniqueFlightInfo.Rows[i]["SchedTimeOfArrival"].ToString();
                    dateVariationDep = Convert.ToInt32(dtUniqueFlightInfo.Rows[i]["DateVariationDep"].ToString());
                    dateVariationArr = Convert.ToInt32(dtUniqueFlightInfo.Rows[i]["dateVariationArr"].ToString());
                    //frequency = freqRotated == string.Empty ? dtUniqueFlightInfo.Rows[i]["Frequency"].ToString() : freqRotated;
                    frequency = dtUniqueFlightInfo.Rows[i]["Frequency"].ToString();
                    serviceType = dtUniqueFlightInfo.Rows[i]["ServiceType"].ToString();
                    aircraftType = dtUniqueFlightInfo.Rows[i]["AircraftType"].ToString();
                    POL = dtUniqueFlightInfo.Rows[i]["POL"].ToString();
                    depDate = DateTime.ParseExact(Convert.ToDateTime(dtUniqueFlightInfo.Rows[i]["DepDate"]).ToString("yyyy-MM-dd"), "yyyy-MM-dd", null).ToString();
                    legDepDayDiff = Convert.ToInt32(dtRearrangeFlightInfo.Rows[i]["LegDepDayDiff"].ToString() == "" ? "0" : dtRearrangeFlightInfo.Rows[i]["LegDepDayDiff"].ToString());
                    orgDepTime = dtUniqueFlightInfo.Rows[i]["OrgDepTime"].ToString();
                    DataSet? dsScheduleDetails = new DataSet();
                    dsScheduleDetails = await SaveSSMDetails(srno, scheduleID, isLastLeg, scheduleIDs, POL, depDate, orgDepTime);
                    //if (dsScheduleDetails != null && dsScheduleDetails.Tables.Count > 0 && dsScheduleDetails.Tables[0].Rows.Count > 0 && dsScheduleDetails.Tables[0].Columns.Contains("ScheduleID"))
                    //{
                    //    scheduleIDs = dsScheduleDetails.Tables[0].Rows[0]["ScheduleIDs"].ToString();
                    //    freqRotated = dsScheduleDetails.Tables[0].Rows[0]["Frequency"].ToString();
                    //}
                    legNumber += 1;
                }

                //RefreshScheduleByScheduleID(scheduleIDs, "SSM/NEW");
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        /*Not in use*/
        //private void parseNEWRevised(string[] arrLine, int srno, string messageBody)
        //{
        //    try
        //    {
        //        SetVariablesToDefaultValues();
        //        //legNumber = 1;
        //        //int scheduleID = 0;

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
        //        flightNo = arrLine[indxFlightInfo].Split(' ')[0];
        //        DataTable dtFlightInfo = new DataTable();
        //        dtFlightInfo = CreateNewFlightDataTable();
        //        DataTable dtPeriodFrequency = new DataTable();
        //        dtPeriodFrequency = CreatePeriodFreqDataTable();

        //        ///12NOV 30NOV 1234567/W2 6/SQ103C/1
        //        ///01DEC 31DEC 1234567/W2 6/SQ103C/1
        //        for (int i = indxFlightInfo + 1; i < arrLine.Length; i++)
        //        {
        //            string[] strMessages = arrLine[i].Trim().Split(' ');

        //            if (strMessages[0].Trim().Length == 1)
        //                break;
        //            if (ValidatePeriodRequencyLine(arrLine[i].Trim()))
        //            {
        //                schedDateOfDepart = strMessages[0];
        //                schedDateOfArrival = strMessages[1];
        //                frequency = strMessages[2].Split('/')[0];
        //                dtPeriodFrequency.Rows.Add(
        //                    flightNo
        //                    , schedDateOfDepart
        //                    , schedDateOfArrival
        //                    , frequency
        //                );
        //            }
        //            indxFlightInfo++;
        //        }
        //        //int flightInfoRowIndex = 0;
        //        for (int i = 0; i < dtPeriodFrequency.Rows.Count; i++)
        //        {
        //            legNumber = 1;
        //            //bool isNewRowAddedToFlightInfoTable = false;
        //            for (int j = indxFlightInfo + 1; j < arrLine.Length; j++)
        //            {
        //                //DataRow drNewFlightInfo = dtFlightInfo.NewRow();
        //                string[] strMessages = arrLine[j].Trim().Split(' ');
        //                if (strMessages[0] == "SI" || strMessages[0] == "//")
        //                    break;
        //                int orgInfoLen = 0, destInfoLen = 0;
        //                ///SIN0730/0/0735 KUL0820/0/0820
        //                if (strMessages.Length > 1)
        //                {
        //                    orgInfoLen = strMessages[0].Split('/')[0].Length;
        //                    destInfoLen = strMessages[1].Split('/')[0].Length;
        //                }
        //                if (orgInfoLen == 7 && destInfoLen == 7)
        //                {
        //                    //if (isNewRowAddedToFlightInfoTable)
        //                    //    isNewRowAddedToFlightInfoTable = false;
        //                    //else
        //                    //{
        //                    //    //dtFlightInfo.Rows.Add(drNewFlightInfo);
        //                    //    flightInfoRowIndex++;
        //                    //}

        //                    dateVariationDep = dateVariationArr = 0;
        //                    //dtFlightInfo.Rows[flightInfoRowIndex]["STDDateVariation"] = 0;
        //                    //dtFlightInfo.Rows[flightInfoRowIndex]["STADateVariation"] = 0;

        //                    string[] source = strMessages[0].Split('/');
        //                    airportOfDepart = strMessages[0].Substring(0, 3);
        //                    schedTimeOfDepart = strMessages[0].Split('/')[0].Substring(3);

        //                    //dtFlightInfo.Rows[flightInfoRowIndex]["Source"] = strMessages[0].Substring(0, 3);
        //                    //dtFlightInfo.Rows[flightInfoRowIndex]["SchDepTime"] = strMessages[0].Split('/')[0].Substring(3);

        //                    if (source.Length > 1 && (source[1].Length == 1 || source[1].Length == 2))
        //                    {
        //                        dateVariationDep = Convert.ToInt32(source[1].Substring(source[1].Length - 1));
        //                        dateVariationDep = source[1].Substring(0, 1) == "M" ? -dateVariationDep : dateVariationDep;
        //                        //dtFlightInfo.Rows[flightInfoRowIndex]["STDDateVariation"] = dateVariationDep;
        //                    }
        //                    string[] dest = strMessages[1].Split('/');
        //                    airportOfArrival = strMessages[1].Substring(0, 3);
        //                    schedTimeOfArrival = strMessages[1].Split('/')[0].Substring(3);
        //                    //dtFlightInfo.Rows[flightInfoRowIndex]["Dest"] = strMessages[0].Substring(0, 3);
        //                    //dtFlightInfo.Rows[flightInfoRowIndex]["SchArrTime"] = strMessages[1].Split('/')[0].Substring(3);
        //                    if (dest.Length > 1 && (dest[1].Length == 1 || dest[1].Length == 2))
        //                    {
        //                        dateVariationArr = Convert.ToInt32(dest[1].Substring(dest[1].Length - 1));
        //                        dateVariationArr = dest[1].Substring(0, 1) == "M" ? -dateVariationArr : dateVariationArr;
        //                        //dtFlightInfo.Rows[flightInfoRowIndex]["STADateVariation"] = dateVariationArr;
        //                    }

        //                    schedDateOfDepart = dtPeriodFrequency.Rows[i]["FromDate"].ToString();
        //                    schedDateOfArrival = dtPeriodFrequency.Rows[i]["ToDate"].ToString();
        //                    frequency = dtPeriodFrequency.Rows[i]["Frequency"].ToString();
        //                    //dtFlightInfo.Rows[flightInfoRowIndex]["FromDate"] = dtPeriodFrequency.Rows[i]["FromDate"].ToString();
        //                    //dtFlightInfo.Rows[flightInfoRowIndex]["ToDate"] = dtPeriodFrequency.Rows[i]["ToDate"].ToString();
        //                    //dtFlightInfo.Rows[flightInfoRowIndex]["Frequency"] = dtPeriodFrequency.Rows[i]["Frequency"].ToString();

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
        //                        , ""
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
        //                    //isNewRowAddedToFlightInfoTable = true;
        //                }
        //            }
        //        }

        //       DataTable dtUniqueFlightInfo = RemoveDuplicatesRecords(dtFlightInfo);
        //        SQLServer sqlServer = new SQLServer();
        //        SqlParameter[] sqlParameter = new SqlParameter []{ 
        //            new  SqlParameter("@FlightInfoTableType", dtUniqueFlightInfo)
        //        };
        //        DataSet? dsFlightinfo = new DataSet();
        //        dsFlightinfo = await _readWriteDao.SelectRecords("Messaging.uspSSMNEW", sqlParameter);

        //        /* 
        //        DataTable dtRearrangeFlightInfo = dtUniqueFlightInfo;
        //        dtUniqueFlightInfo = null;
        //        dtUniqueFlightInfo = RearrangeFlightInfo(dtRearrangeFlightInfo);
        //        flightNo = string.Empty;
        //        schedDateOfDepart = string.Empty;
        //        string POL = string.Empty, freqRotated = string.Empty, depDate = string.Empty, arrDate = string.Empty, orgDepTime = string.Empty;
        //        int legDepDayDiff = 0;
        //        for (int i = 0; i < dtUniqueFlightInfo.Rows.Count; i++)
        //        {
        //            bool isLastLeg = false;
        //            if ((flightNo == string.Empty && schedDateOfDepart == string.Empty) ||
        //                flightNo != dtUniqueFlightInfo.Rows[i]["FlightNumber"].ToString().Trim()
        //                || schedDateOfDepart != dtUniqueFlightInfo.Rows[i]["SchDateOfDeparture"].ToString().Trim())
        //            {
        //                isLastLeg = false;
        //                scheduleIDs = string.Empty;
        //                frequency = string.Empty;
        //                legNumber = 1;
        //            }
        //            if ((i == dtUniqueFlightInfo.Rows.Count - 1)
        //            || (dtUniqueFlightInfo.Rows.Count - 1 > i && (dtUniqueFlightInfo.Rows[i]["FlightNumber"].ToString() != dtUniqueFlightInfo.Rows[i + 1]["FlightNumber"].ToString()
        //            || dtUniqueFlightInfo.Rows[i]["SchDateOfDeparture"].ToString() != dtUniqueFlightInfo.Rows[i + 1]["SchDateOfDeparture"].ToString())))
        //                isLastLeg = true;

        //            flightNo = dtUniqueFlightInfo.Rows[i]["FlightNumber"].ToString();
        //            airportOfDepart = dtUniqueFlightInfo.Rows[i]["AirportOfDepart"].ToString();
        //            airportOfArrival = dtUniqueFlightInfo.Rows[i]["AirportOfArrival"].ToString();
        //            schedDateOfDepart = dtUniqueFlightInfo.Rows[i]["SchDateOfDeparture"].ToString();
        //            schedDateOfArrival = dtUniqueFlightInfo.Rows[i]["SchDateOfArrival"].ToString();
        //            schedTimeOfDepart = dtUniqueFlightInfo.Rows[i]["SchedTimeOfDepart"].ToString();
        //            schedTimeOfArrival = dtUniqueFlightInfo.Rows[i]["SchedTimeOfArrival"].ToString();
        //            dateVariationDep = Convert.ToInt32(dtUniqueFlightInfo.Rows[i]["DateVariationDep"].ToString());
        //            dateVariationArr = Convert.ToInt32(dtUniqueFlightInfo.Rows[i]["dateVariationArr"].ToString());
        //            //frequency = freqRotated == string.Empty ? dtUniqueFlightInfo.Rows[i]["Frequency"].ToString() : freqRotated;
        //            frequency = dtUniqueFlightInfo.Rows[i]["Frequency"].ToString();
        //            serviceType = dtUniqueFlightInfo.Rows[i]["ServiceType"].ToString();
        //            aircraftType = dtUniqueFlightInfo.Rows[i]["AircraftType"].ToString();
        //            POL = dtUniqueFlightInfo.Rows[i]["POL"].ToString();
        //            depDate = DateTime.ParseExact(Convert.ToDateTime(dtUniqueFlightInfo.Rows[i]["DepDate"]).ToString("yyyy-MM-dd"), "yyyy-MM-dd", null).ToString();
        //            legDepDayDiff = Convert.ToInt32(dtRearrangeFlightInfo.Rows[i]["LegDepDayDiff"].ToString() == "" ? "0" : dtRearrangeFlightInfo.Rows[i]["LegDepDayDiff"].ToString());
        //            orgDepTime = dtUniqueFlightInfo.Rows[i]["OrgDepTime"].ToString();
        //            DataSet dsScheduleDetails = new DataSet();
        //            dsScheduleDetails = SaveSSMDetails(srno, scheduleID, isLastLeg, scheduleIDs, POL, depDate, orgDepTime);
        //            if (dsScheduleDetails != null && dsScheduleDetails.Tables.Count > 0 && dsScheduleDetails.Tables[0].Rows.Count > 0 && dsScheduleDetails.Tables[0].Columns.Contains("ScheduleID"))
        //            {
        //                scheduleIDs = dsScheduleDetails.Tables[0].Rows[0]["ScheduleIDs"].ToString();
        //                freqRotated = dsScheduleDetails.Tables[0].Rows[0]["Frequency"].ToString();
        //            }
        //            legNumber += 1;
        //        }

        //        RefreshScheduleByScheduleID(scheduleIDs, "SSM/NEW");*/
        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //    }
        //}
        private DataTable RearrangeFlightInfo(DataTable dtRearrangeFlightInfo)
        {
            string flightNo = string.Empty, schedDateOfDepart = string.Empty, depTime = string.Empty, arrTime = string.Empty, orgDepTime = string.Empty;
            DateTime depDate = new DateTime(2001, 1, 1), prevDepDate = new DateTime(2001, 1, 1), arrDate = new DateTime(2001, 1, 1);
            int legDepDayDiff = 0;
            int legNumber = 1;
            try
            {
                string POL = string.Empty;
                for (int i = 0; i < dtRearrangeFlightInfo.Rows.Count; i++)
                {
                    bool isLastLeg = false;

                    if ((flightNo == string.Empty && schedDateOfDepart == string.Empty) ||
                        flightNo != dtRearrangeFlightInfo.Rows[i]["FlightNumber"].ToString().Trim()
                        || schedDateOfDepart != dtRearrangeFlightInfo.Rows[i]["SchDateOfDeparture"].ToString().Trim())
                    {
                        legNumber = 1;
                        isLastLeg = false;
                        legDepDayDiff = 0;
                        POL = dtRearrangeFlightInfo.Rows[i]["AirportOfDepart"].ToString().Trim();
                        depDate = getDateTime(dtRearrangeFlightInfo.Rows[i]["SchDateOfDeparture"].ToString().Trim()
                            , dtRearrangeFlightInfo.Rows[i]["SchedTimeOfDepart"].ToString().Trim());
                        arrDate = getDateTime(dtRearrangeFlightInfo.Rows[i]["SchDateOfDeparture"].ToString().Trim()
                            , dtRearrangeFlightInfo.Rows[i]["SchedTimeOfArrival"].ToString().Trim());
                        if (depDate > arrDate)
                            arrDate = arrDate.AddDays(1);
                        dtRearrangeFlightInfo.Rows[i]["DepDate"] = depDate;
                        dtRearrangeFlightInfo.Rows[i]["ArrDate"] = arrDate;
                        dtRearrangeFlightInfo.Rows[i]["OrgDepTime"] = dtRearrangeFlightInfo.Rows[i]["SchedTimeOfDepart"].ToString();
                        orgDepTime = dtRearrangeFlightInfo.Rows[i]["SchedTimeOfDepart"].ToString();
                    }
                    else
                    {
                        prevDepDate = depDate;
                        depDate = getDateTime(arrDate.Day.ToString().PadLeft(2, '0') + arrDate.ToString("MMM")
                            , dtRearrangeFlightInfo.Rows[i]["SchedTimeOfDepart"].ToString().Trim());

                        arrDate = getDateTime(depDate.Day.ToString().PadLeft(2, '0') + depDate.ToString("MMM")
                            , dtRearrangeFlightInfo.Rows[i]["SchedTimeOfArrival"].ToString().Trim());
                        if (depDate > arrDate)
                            arrDate = arrDate.AddDays(1);

                        dtRearrangeFlightInfo.Rows[i]["DepDate"] = depDate;
                        dtRearrangeFlightInfo.Rows[i]["ArrDate"] = arrDate;
                        legDepDayDiff = (depDate.Date - prevDepDate.Date).Days;
                        dtRearrangeFlightInfo.Rows[i]["LegDepDayDiff"] = legDepDayDiff;
                        dtRearrangeFlightInfo.Rows[i]["Frequency"] = RotateFrequency(dtRearrangeFlightInfo.Rows[i]["Frequency"].ToString().Trim(), legDepDayDiff);
                        dtRearrangeFlightInfo.Rows[i]["OrgDepTime"] = orgDepTime;
                    }
                    dtRearrangeFlightInfo.Rows[i]["POL"] = POL;
                    if ((i == dtRearrangeFlightInfo.Rows.Count - 1)
                        || (dtRearrangeFlightInfo.Rows.Count - 1 > i
                        && (dtRearrangeFlightInfo.Rows[i]["FlightNumber"].ToString() != dtRearrangeFlightInfo.Rows[i + 1]["FlightNumber"].ToString()
                        || dtRearrangeFlightInfo.Rows[i]["SchDateOfDeparture"].ToString() != dtRearrangeFlightInfo.Rows[i + 1]["SchDateOfDeparture"].ToString())))
                    {
                        isLastLeg = true;
                        dtRearrangeFlightInfo.Rows[i]["POU"] = dtRearrangeFlightInfo.Rows[i]["AirportOfArrival"];
                    }
                    dtRearrangeFlightInfo.Rows[i]["LegNumber"] = legNumber++;
                    dtRearrangeFlightInfo.Rows[i]["IsLastLeg"] = isLastLeg;

                    flightNo = dtRearrangeFlightInfo.Rows[i]["FlightNumber"].ToString().Trim();
                    schedDateOfDepart = dtRearrangeFlightInfo.Rows[i]["SchDateOfDeparture"].ToString().Trim();
                }
                string POU = string.Empty;
                for (int i = dtRearrangeFlightInfo.Rows.Count - 1; i >= 0; i--)
                {
                    if (Convert.ToBoolean(dtRearrangeFlightInfo.Rows[i]["IsLastLeg"].ToString()))
                        POU = dtRearrangeFlightInfo.Rows[i]["POU"].ToString();
                    else
                        dtRearrangeFlightInfo.Rows[i]["POU"] = POU;
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dtRearrangeFlightInfo;
        }

        private string RotateFrequency(string frequency, int rotateUpTo)
        {
            uint freq = Convert.ToUInt32(frequency);
            string reversestring = string.Empty, rotatedFreq = frequency;
            frequency = "";
            int rem;
            try
            {
                while (freq != 0)
                {
                    rem = Convert.ToInt32(freq % 10);
                    if (rem + rotateUpTo > 7)
                        frequency += (rem + rotateUpTo - 7).ToString();
                    else
                        frequency += (rem + rotateUpTo).ToString();
                    freq = (freq / 10);
                }

                char[] arrFreq = frequency.ToCharArray();
                Array.Reverse(arrFreq);
                reversestring = new string(arrFreq);

                rotatedFreq = reversestring.Contains("1") ? reversestring.Substring(reversestring.IndexOf('1')) + reversestring.Split('1')[0] : reversestring;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return rotatedFreq;
        }

        private void parseCNL(string[] arrLine, int srno)
        {
            try
            {
                SetVariablesToDefaultValues();
                messageIdentifier = "CNL";
                int indxFlightInfo = 0;

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
                    ///J 333 C12Y365.C12Y365 RP-C8123
                    if (ValidatePeriodRequencyLine(arrLine[i].Trim()))
                        break;
                    flightNo = flightNo == "" ? arrLine[i] : flightNo + "," + arrLine[i];
                    indxFlightInfo++;
                }
                string[] arrFlightNo = flightNo.Split(',');
                for (int flt = 0; flt < arrFlightNo.Length; flt++)
                {
                    ///5J14
                    flightNo = arrFlightNo[flt];
                    for (int i = indxFlightInfo; i < arrLine.Length; i++)
                    {
                        ///01FEB19 30MAR19 12567
                        string[] freqInfo = arrLine[i].Trim().Split(' ');
                        ///SI FreeText
                        if (freqInfo[0] == "SI" || freqInfo[0] == "//")
                            break;
                        schedDateOfDepart = freqInfo[0];
                        schedDateOfArrival = freqInfo[1];
                        if (freqInfo.Length > 2)
                            frequency = freqInfo[2].Split('/')[0];
                        SaveSSMDetails(srno);
                    }
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private void parseEQT(string[] arrLine, int srno, string msgID)
        {
            try
            {
                SetVariablesToDefaultValues();
                messageIdentifier = msgID;
                string thirdLine = string.Empty;
                int indxFlightInfo = 0, indxAircraftInfo = 0;

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
                for (int i = indxFlightInfo; i < arrLine.Length; i++)
                {
                    int n;
                    bool isNumeric = int.TryParse(arrLine[i].Split(' ')[0].Substring(0, 2), out n);
                    if (isNumeric)
                        break;
                    flightNo = flightNo == "" ? arrLine[i].Split(' ')[0] : flightNo + "," + arrLine[i].Split(' ')[0];
                    indxFlightInfo++;
                }
                string[] arrFlightNo = flightNo.Split(',');
                DataTable dtFlightInfo = new DataTable();
                dtFlightInfo = CreateFlightInfoDataTable();
                int fltindx = 0;
                for (int flt = 0; flt < arrFlightNo.Length; flt++)
                {
                    ///5J14
                    flightNo = arrFlightNo[flt];
                    for (int i = indxFlightInfo; i < arrLine.Length; i++)
                    {
                        string[] strMessages = arrLine[i].Trim().Split(' ');
                        string[] arrOnD = strMessages[0].Split('/');
                        if (strMessages[0] == "SI" || strMessages[0] == "//")
                            break;
                        if (strMessages.Length == 1 && arrOnD.Length == 2 && arrOnD[0].Length == 3 && arrOnD[1].Length == 3)
                        {
                            for (int z = 0; z < dtFlightInfo.Rows.Count; z++)
                            {
                                dtFlightInfo.Rows[z]["AirportOfDepart"] = arrOnD[0];
                                dtFlightInfo.Rows[z]["AirportOfArrival"] = arrOnD[1];
                            }
                            break;
                        }
                        else if (strMessages[0].Trim().Length == 1)
                        {
                            serviceType = strMessages[0];
                            aircraftType = strMessages[1];
                            for (int z = fltindx; z < dtFlightInfo.Rows.Count; z++)
                            {
                                dtFlightInfo.Rows[z]["ServiceType"] = serviceType;
                                dtFlightInfo.Rows[z]["AircraftType"] = aircraftType;
                                fltindx++;
                            }
                        }
                        else
                        {

                            if (ValidatePeriodRequencyLine(arrLine[i].Trim()))
                            {
                                schedDateOfDepart = strMessages[0];
                                schedDateOfArrival = strMessages[1];
                                frequency = strMessages[2].Split('/')[0];
                                dtFlightInfo.Rows.Add(
                                    flightNo
                                    , ""
                                    , ""
                                    , schedDateOfDepart
                                    , schedDateOfArrival
                                    , ""
                                    , ""
                                    , ""
                                    , ""
                                    , frequency
                                );
                            }
                        }
                    }
                }
                for (int i = 0; i < dtFlightInfo.Rows.Count; i++)
                {
                    flightNo = dtFlightInfo.Rows[i]["FlightNumber"].ToString();
                    airportOfDepart = dtFlightInfo.Rows[i]["AirportOfDepart"].ToString();
                    airportOfArrival = dtFlightInfo.Rows[i]["AirportOfArrival"].ToString();
                    schedDateOfDepart = dtFlightInfo.Rows[i]["SchDateOfDeparture"].ToString();
                    schedDateOfArrival = dtFlightInfo.Rows[i]["SchDateOfArrival"].ToString();
                    frequency = dtFlightInfo.Rows[i]["Frequency"].ToString();
                    serviceType = dtFlightInfo.Rows[i]["ServiceType"].ToString();
                    SaveSSMDetails(srno);
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private void parseRPL(string[] arrLine, int srno)
        {
            try
            {
                SetVariablesToDefaultValues();
                string thirdLine = string.Empty;
                int indxFlightInfo = 0;
                messageIdentifier = "RPL";

                for (int i = 0; i < arrLine.Length; i++)
                {
                    if (arrLine[i].Trim() == "LT")
                        messageTimeMode = "LT";
                    if (arrRPL.Contains(arrLine[i].Trim()))
                    {
                        indxFlightInfo = i + 1;
                        break;
                    }
                }
                for (int i = indxFlightInfo; i < arrLine.Length; i++)
                {
                    if (ValidatePeriodRequencyLine(arrLine[i].Trim()))
                        break;
                    flightNo = flightNo == "" ? arrLine[i].Split(' ')[0] : flightNo + "," + arrLine[i].Split(' ')[0];
                    indxFlightInfo++;
                }
                string[] arrFlightNo = flightNo.Split(',');
                DataTable dtFlightInfo = new DataTable();
                dtFlightInfo = CreateFlightInfoDataTable();
                DataTable dtPeriodFrequency = new DataTable();
                dtPeriodFrequency = CreatePeriodFrequencyDataTable();

                for (int flt = 0; flt < arrFlightNo.Length; flt++)
                {
                    flightNo = arrFlightNo[flt];
                    ///12NOV 30NOV 1234567/W2 6/SQ103C/1
                    ///01DEC 31DEC 1234567/W2 6/SQ103C/1
                    for (int i = indxFlightInfo; i < arrLine.Length; i++)
                    {
                        string[] strMessages = arrLine[i].Trim().Split(' ');
                        if (strMessages[0].Trim().Length == 1)
                            break;
                        if (ValidatePeriodRequencyLine(arrLine[i].Trim()))
                        {
                            schedDateOfDepart = strMessages[0];
                            schedDateOfArrival = strMessages[1];
                            frequency = strMessages[2].Split('/')[0];
                            dtPeriodFrequency.Rows.Add(
                                flightNo
                                , schedDateOfDepart
                                , schedDateOfArrival
                                , frequency
                            );
                        }
                        indxFlightInfo++;
                    }
                    for (int i = 0; i < dtPeriodFrequency.Rows.Count; i++)
                    {
                        for (int j = indxFlightInfo; j < arrLine.Length; j++)
                        {
                            string[] strMessages = arrLine[j].Trim().Split(' ');
                            if (strMessages[0] == "SI" || strMessages[0] == "//")
                                break;
                            int orgInfoLen = 0, destInfoLen = 0;
                            ///SIN0730/0/0735 KUL0820/0/0820
                            if (strMessages.Length > 1)
                            {
                                orgInfoLen = strMessages[0].Split('/')[0].Length;
                                destInfoLen = strMessages[1].Split('/')[0].Length;
                            }
                            if (orgInfoLen == 7 && destInfoLen == 7)
                            {
                                dateVariationDep = dateVariationArr = 0;
                                string[] source = strMessages[0].Split('/');
                                airportOfDepart = strMessages[0].Substring(0, 3);
                                schedTimeOfDepart = strMessages[0].Split('/')[0].Substring(3);
                                if (source.Length > 1 && (source[1].Length == 1 || source[1].Length == 2))
                                {
                                    dateVariationDep = Convert.ToInt32(source[1].Substring(source[1].Length - 1));
                                    dateVariationDep = source[1].Substring(0, 1) == "M" ? -dateVariationDep : dateVariationDep;
                                }
                                string[] dest = strMessages[1].Split('/');
                                airportOfArrival = strMessages[1].Substring(0, 3);
                                schedTimeOfArrival = strMessages[1].Split('/')[0].Substring(3);
                                if (dest.Length > 1 && (dest[1].Length == 1 || dest[1].Length == 2))
                                {
                                    dateVariationArr = Convert.ToInt32(dest[1].Substring(dest[1].Length - 1));
                                    dateVariationArr = dest[1].Substring(0, 1) == "M" ? -dateVariationArr : dateVariationArr;
                                }

                                schedDateOfDepart = dtPeriodFrequency.Rows[i]["SchDateOfDeparture"].ToString();
                                schedDateOfArrival = dtPeriodFrequency.Rows[i]["SchDateOfArrival"].ToString();
                                frequency = dtPeriodFrequency.Rows[i]["Frequency"].ToString();

                                dtFlightInfo.Rows.Add(
                                    flightNo
                                    , airportOfDepart
                                    , airportOfArrival
                                    , schedDateOfDepart
                                    , schedDateOfArrival
                                    , schedTimeOfDepart
                                    , schedTimeOfArrival
                                    , dateVariationDep
                                    , dateVariationArr
                                    , frequency
                                    , serviceType
                                    , aircraftType
                                );
                            }
                            ///C 330 F10Y100/FO.F10Y120
                            else if (strMessages[0].Trim().Length == 1)
                            {
                                serviceType = strMessages[0];
                                aircraftType = strMessages[1];
                            }
                        }
                    }
                }
                for (int i = 0; i < dtFlightInfo.Rows.Count; i++)
                {
                    flightNo = dtFlightInfo.Rows[i]["FlightNumber"].ToString();
                    airportOfDepart = dtFlightInfo.Rows[i]["AirportOfDepart"].ToString();
                    airportOfArrival = dtFlightInfo.Rows[i]["AirportOfArrival"].ToString();
                    schedDateOfDepart = dtFlightInfo.Rows[i]["SchDateOfDeparture"].ToString();
                    schedDateOfArrival = dtFlightInfo.Rows[i]["SchDateOfArrival"].ToString();
                    schedTimeOfDepart = dtFlightInfo.Rows[i]["SchedTimeOfDepart"].ToString();
                    schedTimeOfArrival = dtFlightInfo.Rows[i]["SchedTimeOfArrival"].ToString();
                    dateVariationDep = Convert.ToInt32(dtFlightInfo.Rows[i]["DateVariationDep"].ToString());
                    dateVariationArr = Convert.ToInt32(dtFlightInfo.Rows[i]["dateVariationArr"].ToString());
                    frequency = dtFlightInfo.Rows[i]["Frequency"].ToString();
                    serviceType = dtFlightInfo.Rows[i]["ServiceType"].ToString();
                    aircraftType = dtFlightInfo.Rows[i]["AircraftType"].ToString();
                    SaveSSMDetails(srno);
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private void parseTIM(string[] arrLine, int srno)
        {
            try
            {
                SetVariablesToDefaultValues();
                messageIdentifier = "TIM";
                int indxFlightInfo = 0;

                for (int i = 0; i < arrLine.Length; i++)
                {
                    if (arrLine[i].Trim() == "LT")
                        messageTimeMode = "LT";
                    if (arrLine[i].Trim() == "TIM" || arrLine[i].Trim() == "TIM COMM" || arrLine[i].Trim() == "TIM/ADM")
                    {
                        indxFlightInfo = i + 1;
                        break;
                    }
                }
                if (arrLine.Length > indxFlightInfo + 1)
                    flightNo = arrLine[indxFlightInfo];
                else
                {
                    //GenericFunction genericFunction = new GenericFunction();
                    _genericFunction.UpdateErrorMessageToInbox(srno, "Invalid format", "SSM/TIM");
                    return;
                }

                DataTable dtPeriodFrequency = new DataTable();
                dtPeriodFrequency = CreatePeriodFrequencyDataTable();

                for (int i = indxFlightInfo + 1; i < arrLine.Length; i++)
                {
                    if (!ValidatePeriodRequencyLine(arrLine[i].Trim()))
                        break;
                    schedDateOfDepart = arrLine[i].Split(' ')[0];
                    schedDateOfArrival = arrLine[i].Split(' ')[1];
                    if (arrLine[i].Split(' ').Length > 2)
                        frequency = arrLine[i].Split(' ')[2].Split('/')[0]; ;
                    dtPeriodFrequency.Rows.Add(
                           flightNo
                           , schedDateOfDepart
                           , schedDateOfArrival
                           , frequency
                       );
                    indxFlightInfo++;
                }
                for (int j = 0; j < dtPeriodFrequency.Rows.Count; j++)
                {
                    flightNo = dtPeriodFrequency.Rows[j]["FlightNumber"].ToString();
                    schedDateOfDepart = dtPeriodFrequency.Rows[j]["SchDateOfDeparture"].ToString();
                    schedDateOfArrival = dtPeriodFrequency.Rows[j]["SchDateOfArrival"].ToString();
                    frequency = dtPeriodFrequency.Rows[j]["Frequency"].ToString();
                    for (int i = indxFlightInfo + 1; i < arrLine.Length; i++)
                    {
                        String[] strMessages = arrLine[i].Trim().Split(' ');
                        ///KUL0001/1/2345 CGO0510/1/0515 7/FDC/CD/YS/MS/LS
                        int orgInfoLen = 0, destInfoLen = 0;
                        if (strMessages.Length > 1)
                        {
                            orgInfoLen = strMessages[0].Split('/')[0].Length;
                            destInfoLen = strMessages[1].Split('/')[0].Length;
                        }
                        if (orgInfoLen == 7 && destInfoLen == 7)
                        {
                            dateVariationDep = dateVariationArr = 0;
                            string[] source = strMessages[0].Split('/');
                            airportOfDepart = strMessages[0].Substring(0, 3);
                            schedTimeOfDepart = strMessages[0].Split('/')[0].Substring(3);
                            if (source.Length > 1 && (source[1].Length == 1 || source[1].Length == 2))
                            {
                                dateVariationDep = Convert.ToInt32(source[1].Substring(source[1].Length - 1));
                                dateVariationDep = source[1].Substring(0, 1) == "M" ? -dateVariationDep : dateVariationDep;
                            }
                            string[] dest = strMessages[1].Split('/');
                            airportOfArrival = strMessages[1].Substring(0, 3);
                            schedTimeOfArrival = strMessages[1].Split('/')[0].Substring(3);
                            if (dest.Length > 1 && (dest[1].Length == 1 || dest[1].Length == 2))
                            {
                                dateVariationArr = Convert.ToInt32(dest[1].Substring(dest[1].Length - 1));
                                dateVariationArr = dest[1].Substring(0, 1) == "M" ? -dateVariationArr : dateVariationArr;
                            }
                            SaveSSMDetails(srno);
                            break;
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

        private void parseFLT(string[] arrLine, int srno)
        {
            SetVariablesToDefaultValues();
            try
            {
                messageIdentifier = "FLT";
                String thirdLine = string.Empty;
                int indxFlightInfo = 0;
                for (int i = 0; i < arrLine.Length; i++)
                {
                    if (arrLine[i].Trim().Length > 2)
                    {
                        string[] arrActionIdentifier = arrLine[i].Trim().Split(' ');
                        if (arrActionIdentifier[0] == "FLT")
                        {
                            indxFlightInfo = i + 1;
                            break;
                        }
                        if (i == 1 && (arrActionIdentifier[0] == "UTC" || arrActionIdentifier[0] == "LT"))
                        {
                            messageTimeMode = arrActionIdentifier[0];
                        }
                    }
                }
                if (arrLine.Length >= 3)
                {
                    flightNo = arrLine[indxFlightInfo].Split(' ')[0];
                    if (arrLine.Length > indxFlightInfo + 1)
                    {
                        string[] freqInfo = arrLine[indxFlightInfo + 1].Split(' ');
                        schedDateOfDepart = freqInfo[0];
                        schedDateOfArrival = freqInfo[1];
                        frequency = freqInfo[2].Split('/')[0];
                    }
                    if (arrLine.Length > indxFlightInfo + 2)
                    {
                        string[] Flight = arrLine[indxFlightInfo + 2].Split(' ');
                        newFlightNumber = Flight[0];
                    }
                    SaveSSMDetails(srno);
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private void parseADM(string[] arrLine, int srno)
        {
            SetVariablesToDefaultValues();
            try
            {
                messageIdentifier = "ADM";
                String thirdLine = string.Empty;
                int indxFlightInfo = 0;
                for (int i = 0; i < arrLine.Length; i++)
                {
                    if (arrLine[i].Trim().Length > 2)
                    {
                        string[] arrActionIdentifier = arrLine[i].Trim().Split(' ');
                        if (arrActionIdentifier[0] == "ADM")
                        {
                            indxFlightInfo = i + 1;
                            break;
                        }
                        if (i == 1 && arrActionIdentifier[0] == "LT")
                        {
                            messageTimeMode = arrActionIdentifier[0];
                        }
                    }
                }
                if (arrLine.Length >= 3)
                {
                    flightNo = arrLine[indxFlightInfo].Split(' ')[0];
                    if (arrLine.Length > indxFlightInfo + 1)
                    {
                        string[] freqInfo = arrLine[indxFlightInfo + 1].Split(' ');
                        schedDateOfDepart = freqInfo[0];
                        schedDateOfArrival = freqInfo[1];
                        frequency = freqInfo[2].Split('/')[0];
                    }
                    SaveSSMDetails(srno);
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private async Task<DataSet?> SaveSSMDetails(int srno, int scheduleID = 0, bool isLastLeg = false, string scheduleIDs = "", string POL = "", string depDate = ""
            , string orgDepTime = "")
        {
            DataSet? dsScheduleDetails = new DataSet();
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
                            getDate(DateTime.Now.ToString("ddMMMyy") ),// DateTime.Now.ToShortDateString()
                            srno,
                            messageTimeMode,
                            legNumber,
                            frequency,
                            dateVariationDep,
                            dateVariationArr,
                            scheduleID,
                            isLastLeg,
                            scheduleIDs,
                            POL,
                            getDate(depDate),
                            orgDepTime,
                            newFlightNumber
                        };

                //ASM asm = new ASM();

                dsScheduleDetails = await _asm.UpdateToDatatabse(QueryValues);

                for (int i = 0; i < dsScheduleDetails.Tables.Count; i++)
                {
                    if (dsScheduleDetails.Tables[i].Columns.Contains("ScheduleIDs") && dsScheduleDetails.Tables[i].Columns.Contains("IsRefreshSchedule"))
                    {
                        if (dsScheduleDetails.Tables[i].Rows[0]["ScheduleIDs"].ToString().Trim() != string.Empty
                            && Convert.ToBoolean(dsScheduleDetails.Tables[i].Rows[0]["IsRefreshSchedule"].ToString()))
                        {
                            RefreshScheduleByScheduleID(dsScheduleDetails.Tables[i].Rows[0]["ScheduleIDs"].ToString().Trim(), messageType + "/" + messageIdentifier);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dsScheduleDetails;
        }

        private string getDate(int p)
        {
            try
            {
                DateTime dt = DateTime.Now;

                if (!String.IsNullOrEmpty(schedDateOfArrival) && schedDateOfArrival.Trim().Length == 7)
                {
                    dt = DateTime.ParseExact(schedDateOfArrival, "ddMMMyy", null);
                }
                else
                {

                    if (!String.IsNullOrEmpty(schedDateOfDepart) && schedDateOfDepart.Trim().Length == 7)
                    {
                        dt = DateTime.ParseExact(schedDateOfDepart, "ddMMMyy", null);
                    }

                    else if (!String.IsNullOrEmpty(schedDateOfArrival) && schedDateOfArrival.Trim().Length > 5)
                    {
                        dt = Convert.ToDateTime(schedDateOfArrival);
                    }
                    else if (!String.IsNullOrEmpty(schedDateOfDepart) && schedDateOfDepart.Trim().Length > 7)
                    {
                        dt = DateTime.ParseExact(schedDateOfDepart, "ddMMMyy", null);
                    }

                }
                return new DateTime(DateTime.Now.Year, dt.Month, p).ToShortDateString();
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return new DateTime(DateTime.Now.Year, DateTime.Now.Month, p).ToShortDateString();
            }
        }

        private string getDate(string schedDateOfArrival)
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
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return tempDate.ToString("yyyy-MM-dd hh:mm:ss");
        }

        /// <summary>
        /// Method to concatinate and convert date and time into date time
        /// </summary>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private DateTime getDateTime(string date, string time)
        {
            DateTime tempDate = new DateTime(2001, 1, 1);
            try
            {
                if (time.Trim().Length == 4)
                    time = time.Substring(0, 2) + ":" + time.Substring(2, 2);

                if (date.Trim().Length == 7)
                    return DateTime.ParseExact(date, "ddMMMyy", null);//.ToString("yyyy-MM-dd hh:mm:ss");
                if (date.Trim().Length > 5)
                    return DateTime.ParseExact(date, "ddMMMyy", null);
                if (date.Trim().Length == 2)
                {
                    return DateTime.ParseExact(DateTime.Now.Year.ToString("0000") + "-" + DateTime.Now.Month.ToString("00") + "-" + date, "yyyy-MM-dd", null);//.ToString("yyyy-MM-dd");
                }
                if (date.Trim().Length == 5)
                {
                    // int month = DateTime.ParseExact(schedDateOfArrival.Substring(2, 3), "MMMM", null);

                    string Mnt = DateTime.ParseExact(date.Substring(2, 3), "MMM", System.Globalization.CultureInfo.InvariantCulture).Month.ToString("00");

                    return DateTime.ParseExact(DateTime.Now.Year.ToString("0000") + "-" + Mnt + "-" + date.Substring(0, 2) + " " + time, "yyyy-MM-dd HH:mm", null);//.ToString("yyyy-MM-dd");
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return tempDate;//.ToString("yyyy-MM-dd hh:mm:ss");
        }
        public bool ValidatePeriodRequencyLine(string PerodFrequncy)
        {
            bool isPerodFrequncyValid = false;
            try
            {
                string fromDate = string.Empty, toDate = string.Empty, freq = string.Empty;
                bool isYearNumeric = true, isDayNumeric = true;
                int n;
                string[] arrPerodFrequncy = PerodFrequncy.Split(' ');
                fromDate = arrPerodFrequncy[0];
                toDate = arrPerodFrequncy[1];
                freq = arrPerodFrequncy[2].Split('/')[0];

                isDayNumeric = int.TryParse(fromDate.Substring(0, 2), out n);
                if (fromDate.Length == 7)
                    isYearNumeric = int.TryParse(fromDate.Substring(5, 2), out n);
                if (arrMonths.Contains(fromDate.Substring(2, 3)) && isDayNumeric && isYearNumeric)
                {
                    isDayNumeric = int.TryParse(toDate.Substring(0, 2), out n);
                    if (toDate.Length == 7)
                        isYearNumeric = int.TryParse(toDate.Substring(5, 2), out n);
                    if (arrMonths.Contains(toDate.Substring(2, 3)) && isDayNumeric && isYearNumeric && freq.Trim().Length > 0)
                        isPerodFrequncyValid = true;
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                isPerodFrequncyValid = false;
            }
            return isPerodFrequncyValid;
        }

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
                //onwordFlight = string.Empty;
                schedDateOfDepart = string.Empty;
                schedDateOfArrival = string.Empty;
                schedTimeOfDepart = string.Empty;
                schedTimeOfArrival = string.Empty;
                registrationNo = string.Empty;
                serviceType = string.Empty;
                legNumber = 0;
                dateVariationDep = 0;
                dateVariationArr = 0;
                newFlightNumber = string.Empty;
                POL = string.Empty;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

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
                dtFlightInfo.Columns.Add("DateVariationDep", typeof(string));
                dtFlightInfo.Columns.Add("DateVariationArr", typeof(string));
                dtFlightInfo.Columns.Add("Frequency", typeof(string));
                dtFlightInfo.Columns.Add("ServiceType", typeof(string));
                dtFlightInfo.Columns.Add("AircraftType", typeof(string));
                dtFlightInfo.Columns.Add("POL", typeof(string));
                dtFlightInfo.Columns.Add("POU", typeof(string));
                dtFlightInfo.Columns.Add("LegNumber", typeof(int));
                dtFlightInfo.Columns.Add("IsLastLeg", typeof(bool));
                dtFlightInfo.Columns.Add("DepDate", typeof(DateTime));
                dtFlightInfo.Columns.Add("ArrDate", typeof(DateTime));
                dtFlightInfo.Columns.Add("LegDepDayDiff", typeof(int));
                dtFlightInfo.Columns.Add("OrgDepTime", typeof(string));
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dtFlightInfo;
        }

        public DataTable CreateNewFlightDataTable()
        {
            DataTable dtFlightInfo = new DataTable();
            try
            {
                dtFlightInfo.Columns.Add("ID", typeof(int));
                dtFlightInfo.Columns.Add("InboxSrNo", typeof(int));
                dtFlightInfo.Columns.Add("LegSeqNumber", typeof(int));
                dtFlightInfo.Columns.Add("MsgType", typeof(string));
                dtFlightInfo.Columns.Add("MsgID", typeof(string));
                dtFlightInfo.Columns.Add("FlightID", typeof(string));
                dtFlightInfo.Columns.Add("FromDate", typeof(string));
                dtFlightInfo.Columns.Add("ToDate", typeof(string));
                dtFlightInfo.Columns.Add("Source", typeof(string));
                dtFlightInfo.Columns.Add("Dest", typeof(string));
                dtFlightInfo.Columns.Add("Frequency", typeof(string));
                dtFlightInfo.Columns.Add("SchDepTime", typeof(string));
                dtFlightInfo.Columns.Add("SchArrTime", typeof(string));
                dtFlightInfo.Columns.Add("TimeMode", typeof(string));
                dtFlightInfo.Columns.Add("STDDateVariation", typeof(int));
                dtFlightInfo.Columns.Add("STADateVariation", typeof(int));
                dtFlightInfo.Columns.Add("PSchDepTime", typeof(string));
                dtFlightInfo.Columns.Add("PSchArrTime", typeof(string));
                dtFlightInfo.Columns.Add("FlightType", typeof(string));
                dtFlightInfo.Columns.Add("AircraftType", typeof(string));
                dtFlightInfo.Columns.Add("TailNumber", typeof(string));
                dtFlightInfo.Columns.Add("PassengerReservationInfo", typeof(string));
                dtFlightInfo.Columns.Add("MessageBody", typeof(string));

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dtFlightInfo;
        }
        private DataTable CreatePeriodFrequencyDataTable()
        {
            DataTable dtFlightInfo = new DataTable();
            try
            {
                dtFlightInfo.Columns.Add("FlightNumber", typeof(string));
                dtFlightInfo.Columns.Add("SchDateOfDeparture", typeof(string));
                dtFlightInfo.Columns.Add("SchDateOfArrival", typeof(string));
                dtFlightInfo.Columns.Add("Frequency", typeof(string));
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dtFlightInfo;
        }

        public DataTable CreatePeriodFreqDataTable()
        {
            DataTable dtFlightInfo = new DataTable();
            try
            {
                dtFlightInfo.Columns.Add("FlightNumber", typeof(string));
                dtFlightInfo.Columns.Add("FromDate", typeof(string));
                dtFlightInfo.Columns.Add("ToDate", typeof(string));
                dtFlightInfo.Columns.Add("Frequency", typeof(string));
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dtFlightInfo;
        }

        public DataTable RemoveDuplicatesRecords(DataTable dt)
        {
            DataTable dt2 = new DataTable();
            try
            {
                var UniqueRows = dt.AsEnumerable().Distinct(DataRowComparer.Default);
                dt2 = UniqueRows.CopyToDataTable();
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dt2;
        }

        private async Task RefreshScheduleByScheduleID(string scheduleIDs, string updatedBy)
        {
            try
            {
                int[] arrScheduleID = Array.ConvertAll(scheduleIDs.Split(','), s => int.Parse(s));
                if (arrScheduleID.Length > 0)
                {
                    //SQLServer db = new SQLServer();
                    //string[] paramNames = new string[] { "ScheduleID", "UpdatedBy" };
                    //SqlDbType[] paramTypes = new SqlDbType[] { SqlDbType.Int, SqlDbType.VarChar };
                    SqlParameter[] sqlParams = new SqlParameter[] {
                        new SqlParameter("@ScheduleID", SqlDbType.Int),
                        new SqlParameter("@UpdatedBy", SqlDbType.VarChar)
                    };
                    for (int i = 0; i < arrScheduleID.Length; i++)
                    {
                        object[] paramValues = new object[] { arrScheduleID[i], updatedBy };
                        DataSet dsRefreshSchedule = new DataSet();
                        //dsRefreshSchedule = db.SelectRecords("dbo.uspRefreshAirlineScheduleRouteForecast", paramNames, paramValues, paramTypes);
                        dsRefreshSchedule = await _readWriteDao.SelectRecords("dbo.uspRefreshAirlineScheduleRouteForecast", sqlParams);

                    }
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }
    }
}