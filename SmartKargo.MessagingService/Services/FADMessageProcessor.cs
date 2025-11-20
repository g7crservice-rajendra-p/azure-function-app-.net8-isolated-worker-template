//namespace QidWorkerRole
//{
//    public class FADMessageProcessor
//    {
//        #region :: Public Methods ::

//        /// <summary>
//        /// Method to decode FAD incomming message and set decoded parameters
//        /// to the variables declared in struct
//        /// </summary>
//        /// <param name="refNO">tblInbox SrNo</param>
//        /// <param name="fadmsg">Message String</param>
//        /// <param name="fsadata"></param>
//        /// <param name="discrepancyinfo">Struct variable contail discrepancy data</param>
//        /// <param name="fsanodes"></param>
//        /// <param name="custominfo"></param>
//        /// <param name="uld"></param>
//        /// <param name="othinfoarray">Other service information</param>
//        /// <param name="disadviceaddress">Discrepancy advice address</param>
//        /// <returns>Returns boolean value true when decode success otherwise false</returns>
//        public bool DecodeReceivedFADMessage(int refNO, string fadmsg, ref MessageData.FSAInfo fsadata, ref MessageData.discrepancydetailsinfo discrepancyinfo, ref MessageData.CommonStruct[] fsanodes, ref MessageData.customsextrainfo[] custominfo, ref MessageData.ULDinfo[] uld, ref MessageData.otherserviceinfo[] othinfoarray, MessageData.discrepancyadviceaddress disadviceaddress)
//        {
//            string awbRef = string.Empty;
//            bool isDecodeSuccess = false;
//            string lastrec = string.Empty;
//            try
//            {
//                if (fadmsg.StartsWith(MessageData.MessageTypeName.FAD, StringComparison.OrdinalIgnoreCase) || fadmsg.StartsWith(MessageData.MessageTypeName.FDA, StringComparison.OrdinalIgnoreCase))
//                {
//                    string[] fadMessage = fadmsg.Split('$');
//                    if (fadMessage.Length > 2)
//                    {
//                        isDecodeSuccess = true;
//                        string[] disStatus = new string[] { "FDAW", "FDCA", "MSAW", "MSCA", "FDAV", "FDMB", "MSAV", "MSMB", "DFLD", "OFLD", "OVCD", "SSPD" };
//                        for (int i = 2; i < fadMessage.Length; i++)
//                        {
//                            if (fadMessage[i].Length > 0)
//                            {
//                                string[] msg = fadMessage[i].Split('/');
//                                if (fadMessage[0].StartsWith(MessageData.MessageTypeName.FAD, StringComparison.OrdinalIgnoreCase) || fadMessage[0].StartsWith(MessageData.MessageTypeName.FDA, StringComparison.OrdinalIgnoreCase))
//                                {
//                                    #region : Line 1 message identifier:
//                                    try
//                                    {
//                                        fsadata.fsaversion = msg[1];
//                                    }
//                                    catch (Exception ex)
//                                    {
//                                        isDecodeSuccess = false;
//                                        //clsLog.WriteLogAzure(ex);
//                                    }
//                                    #endregion Line 1
//                                }
//                                else if (msg[0].Contains('-'))
//                                {
//                                    #region : Line 2 awb consigment details :
//                                    string[] currentLineConsignmentText = fadMessage[i].Split('/');
//                                    string airlinePrefix = string.Empty;
//                                    string awbNo = string.Empty;
//                                    string origin = string.Empty;
//                                    string destination = string.Empty;
//                                    int indexofKORL = 0;
//                                    try
//                                    {
//                                        if (currentLineConsignmentText.Length > 0)
//                                        {
//                                            fsadata.airlineprefix = currentLineConsignmentText[0] != "" ? currentLineConsignmentText[0].Substring(0, 3) : "";
//                                            fsadata.awbnum = currentLineConsignmentText[0] != "" ? currentLineConsignmentText[0].Substring(4, 8) : "";
//                                            fsadata.origin = currentLineConsignmentText[0] != "" ? currentLineConsignmentText[0].Substring(12, 3) : "";
//                                            fsadata.dest = currentLineConsignmentText[0] != "" ? currentLineConsignmentText[0].Substring(15, 3) : "";
//                                        }
//                                        if (currentLineConsignmentText.Length > 1)
//                                        {
//                                            if (currentLineConsignmentText[1].Contains("K"))
//                                            {
//                                                indexofKORL = currentLineConsignmentText[1].LastIndexOf('K');
//                                                fsadata.weightcode = "K";
//                                            }
//                                            else if (currentLineConsignmentText[1].Contains("L"))
//                                            {
//                                                indexofKORL = currentLineConsignmentText[1].LastIndexOf('L');
//                                                fsadata.weightcode = "L";
//                                            }

//                                            if (!currentLineConsignmentText[1].Contains("K") && (!currentLineConsignmentText[1].Contains("L")) && (currentLineConsignmentText[1].Substring(0, 1).Contains("T")))
//                                            {
//                                                fsadata.consigntype = currentLineConsignmentText[1] != "" ? (currentLineConsignmentText[1].Substring(0, 1)) : "";

//                                                fsadata.pcscnt = currentLineConsignmentText[1] != "" ? currentLineConsignmentText[1].Substring(1) : "0";
//                                            }

//                                            else if (((currentLineConsignmentText[1].Contains("K")) || (currentLineConsignmentText[1].Contains("L"))) && (currentLineConsignmentText[1].Substring(0, 1).Contains("T")) && (!currentLineConsignmentText[1].Substring(1).Contains("T")))
//                                            {
//                                                fsadata.consigntype = currentLineConsignmentText[1] != ""
//                                                                 ? (currentLineConsignmentText[1].Substring(0, 1))
//                                                                 : ("");

//                                                fsadata.pcscnt = currentLineConsignmentText[1] != "" ? (currentLineConsignmentText[1].Substring(1, indexofKORL - 1)) : "0";

//                                                fsadata.weight = currentLineConsignmentText[1] != "" ? (currentLineConsignmentText[1].Substring(indexofKORL + 1)) : ("0.0");
//                                            }

//                                            else if (((currentLineConsignmentText[1].Contains("K")) || (currentLineConsignmentText[1].Contains("L"))) && (currentLineConsignmentText[1].Substring(1).Contains("T")))
//                                            {
//                                                int indexOfLastT = currentLineConsignmentText[1].LastIndexOf('T');
//                                                fsadata.consigntype = currentLineConsignmentText[1] != ""
//                                                                 ? (currentLineConsignmentText[1].Substring(0, 1))
//                                                                 : ("");

//                                                fsadata.pcscnt = currentLineConsignmentText[1] != "" ? (currentLineConsignmentText[1].Substring(1, indexofKORL - 1)) : "0";


//                                                fsadata.weight = currentLineConsignmentText[1] != ""
//                                                             ? (currentLineConsignmentText[1].Substring(indexofKORL + 1,
//                                                                                                          (indexOfLastT) - (indexofKORL + 1)))
//                                                             : ("0.0");
//                                                fsadata.totalpcscnt = currentLineConsignmentText[1] != "" ? (currentLineConsignmentText[1].Substring(indexOfLastT + 1)) : "0";
//                                            }
//                                        }
//                                    }
//                                    catch (Exception ex)
//                                    {
//                                        isDecodeSuccess = false;
//                                        clsLog.WriteLogAzure(ex);
//                                    }
//                                    #endregion Line 2 awb consigment details
//                                }
//                                else if (Array.Exists<string>(disStatus, element => element == msg[0]))
//                                {
//                                    #region : Line 3 Decode Discrepancy Status :
//                                    string[] DiscrepancyDetailsText = fadMessage[i].Split('/');
//                                    int arrLength = DiscrepancyDetailsText.Length;
//                                    try
//                                    {
//                                        discrepancyinfo.discrepancycode = DiscrepancyDetailsText[0].Trim();
//                                        discrepancyinfo.airportcode = DiscrepancyDetailsText[1].Trim();
//                                        if (arrLength == 5)
//                                        {
//                                            discrepancyinfo.carriercode = DiscrepancyDetailsText[2].Trim().Substring(1, 2);
//                                            discrepancyinfo.flightnum = DiscrepancyDetailsText[2].Trim().Substring(3);
//                                            discrepancyinfo.day = DiscrepancyDetailsText[3].Trim().Substring(1, 2);
//                                            discrepancyinfo.month = DiscrepancyDetailsText[3].Trim().Substring(3);
//                                            discrepancyinfo.fltdep = DiscrepancyDetailsText[4].Trim().Substring(1, 3);
//                                            discrepancyinfo.fltarrival = DiscrepancyDetailsText[4].Trim().Substring(4);
//                                        }
//                                        else if (arrLength > 2 && arrLength < 5)
//                                        {
//                                            isDecodeSuccess = false;
//                                        }

//                                    }
//                                    catch (Exception ex)
//                                    {
//                                        isDecodeSuccess = false;
//                                        clsLog.WriteLogAzure(ex);
//                                    }
//                                    #endregion Decode Discrepancy Status
//                                }
//                                else if (msg[0].StartsWith("OSI", StringComparison.OrdinalIgnoreCase))
//                                {
//                                    #region : Line 4 Other Service Information 1 :
//                                    string[] msgOtherServceInfo = fadMessage[i].Split('/');
//                                    try
//                                    {
//                                        lastrec = msg[0];
//                                        if (msg[1].Length > 0)
//                                        {
//                                            Array.Resize(ref othinfoarray, othinfoarray.Length + 1);
//                                            othinfoarray[othinfoarray.Length - 1].otherserviceinfo1 = msgOtherServceInfo[1];
//                                            othinfoarray[othinfoarray.Length - 1].consigref = awbRef;
//                                        }
//                                    }
//                                    catch (Exception ex)
//                                    {
//                                        isDecodeSuccess = false;
//                                        clsLog.WriteLogAzure(ex);
//                                    }
//                                    #endregion Other Service Information 1
//                                }
//                                else if (msg[0].StartsWith("/"))
//                                {
//                                    #region : Line 4 Other Service Information 2 :
//                                    string[] msgOtherServceInfo = fadMessage[i].Split('/');
//                                    try
//                                    {
//                                        othinfoarray[othinfoarray.Length - 1].otherserviceinfo2 = msgOtherServceInfo[1];
//                                    }
//                                    catch (Exception ex)
//                                    {
//                                        isDecodeSuccess = false;
//                                        clsLog.WriteLogAzure(ex);
//                                    }
//                                    #endregion Line 4 Other Service Information 2
//                                }
//                                else
//                                {
//                                    #region : Line 5 Discrepancy Advice Address :
//                                    string[] msgDiscrepancyAdviceAddress = fadMessage[i].Split('/');
//                                    try
//                                    {
//                                        if (msgDiscrepancyAdviceAddress.Length > 0)
//                                        {
//                                            disadviceaddress.discrepancyAdviceAddressLineIdentifier = msgDiscrepancyAdviceAddress[0];
//                                            disadviceaddress.discrepancyAdviceAddressAirportCode = msgDiscrepancyAdviceAddress[1].Substring(1, 3);
//                                            disadviceaddress.discrepancyAdviceAddressOfficeFunctionDesignator = msgDiscrepancyAdviceAddress[1].Substring(4, 2);
//                                            disadviceaddress.discrepancyAdviceAddressCompanyDesignator = msgDiscrepancyAdviceAddress[1].Substring(6);
//                                        }
//                                    }
//                                    catch (Exception ex)
//                                    {
//                                        isDecodeSuccess = false;
//                                        clsLog.WriteLogAzure(ex);
//                                    }
//                                    #endregion Line 5Discrepancy Advice Address
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                isDecodeSuccess = false;
//                clsLog.WriteLogAzure(ex);
//            }
//            return isDecodeSuccess;
//        }
        
//        #endregion Public Methods
//    }
//}
