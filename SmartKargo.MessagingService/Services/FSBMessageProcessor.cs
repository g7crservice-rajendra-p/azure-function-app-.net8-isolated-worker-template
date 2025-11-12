#region FSB Message Processor Class Description
/* FSB Message Processor Class Description.
      * Company              :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright              :   Copyright © 2015 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose                : 
      * Created By           :   Badiuzzaman Khan
      * Created On           :   2016-03-02
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


namespace QidWorkerRole
{

    public class FSBMessageProcessor
    {
        //string ErrorMessage = string.Empty;
        //string strConnection = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        //SCMExceptionHandlingWorkRole scmException = new SCMExceptionHandlingWorkRole();

        private readonly ILogger<FHLMessageProcessor> _logger;
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly GenericFunction _genericFunction;
        public FSBMessageProcessor(
            ILogger<FHLMessageProcessor> logger,
            ISqlDataHelperDao readWriteDao,
            GenericFunction genericFunction
           )
        {
            _logger = logger;
            _readWriteDao = readWriteDao;
            _genericFunction = genericFunction;
        }

        #region FSB Message Decoding for Update Dateabse
        /// <summary>
        /// Created By:Badiuz khan
        /// Created On:2016-01-15
        /// Description:Decoding FSB Message 
        /// </summary>

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="fsbMessage"></param>
        /// <param name="fsbShipper"></param>
        /// <param name="fsbConsignee"></param>
        /// <param name="RouteIformation"></param>
        /// <param name="Dimensionformation"></param>
        /// <param name="bublistinformation"></param>
        /// <returns></returns>
        public bool DecodingFSBMessge(string strMessage, MessageData.FSBAWBInformation fsbMessage, MessageData.ShipperInformation fsbShipper, MessageData.ConsigneeInformation fsbConsignee, List<MessageData.RouteInformation> RouteIformation, List<MessageData.FSBDimensionInformation> Dimensionformation, List<MessageData.AWBBUPInformation> bublistinformation)
        {
            bool flag = true;

            MessageData.FSBDimensionInformation Dimension = new MessageData.FSBDimensionInformation();
            MessageData.AWBBUPInformation bupInformation = new MessageData.AWBBUPInformation();
            MessageData.RouteInformation Route = new MessageData.RouteInformation();
            try
            {
                var fsbLine = new StringReader(strMessage.Replace("$", "\r\n").Replace("$", "\n"));
                int lineNo = 1;
                string lineText;
                while ((lineText = fsbLine.ReadLine()) != null)
                {

                    if (lineText.Length > 0)
                    {
                        switch (lineNo)
                        {
                            case 1:
                                string[] currentLineText = lineText.Split('/');
                                string fwbName = string.Empty;
                                string fwbVersion = string.Empty;
                                if (currentLineText.Length >= 1)
                                {
                                    if (currentLineText[0].Length == 3)
                                        fsbMessage.MessageType = currentLineText[0] != "" ? currentLineText[0] : "";
                                }
                                if (currentLineText.Length >= 2)
                                {
                                    if (currentLineText[1].Length > 0)
                                    {
                                        fsbMessage.MessageVersion = currentLineText[1] != "" ? currentLineText[1] : "";
                                    }

                                }
                                break;
                            case 2:
                                string[] currentlineText = lineText.Split('/');
                                char piecesCode = ' ';
                                int indexofKORL = 0;

                                if (currentlineText.Length >= 1)
                                {
                                    if (currentlineText[0].Length >= 12)
                                    {
                                        int indexOfHyphen = currentlineText[0].IndexOf('-');
                                        if (indexOfHyphen == 3)
                                        {
                                            fsbMessage.AirlinePrefix = currentlineText[0].Substring(0, 3);
                                            fsbMessage.AWBNo = currentlineText[0].Substring(4, 8);
                                        }

                                    }
                                    if (currentlineText[0].Substring(12).Length == 6)
                                    {
                                        fsbMessage.AWBOrigin = currentlineText[0].Substring(12, 3);
                                        fsbMessage.AWBDestination = currentlineText[0].Substring(15, 3);
                                    }

                                }

                                if (currentlineText.Length >= 2)
                                {
                                    if (currentlineText[1].Length > 1)
                                    {
                                        if (currentlineText[1].Contains("K"))
                                        {
                                            indexofKORL = currentlineText[1].LastIndexOf('K');
                                            fsbMessage.WeightCode = "K";
                                        }
                                        else if (currentlineText[1].Contains("L"))
                                        {
                                            indexofKORL = currentlineText[1].LastIndexOf('L');
                                            fsbMessage.WeightCode = "L";
                                        }

                                        piecesCode = currentlineText[1] != ""
                                                         ? char.Parse(currentlineText[1].Substring(0, 1))
                                                         : ' ';
                                        if (!currentlineText[1].Contains("K") && (!currentlineText[1].Contains("L")))
                                        {
                                            fsbMessage.TotalAWbPiececs = currentlineText[1] != ""
                                                         ? int.Parse(currentlineText[1].Substring(1))
                                                         : 0;
                                        }

                                        else if (((currentlineText[1].Contains("K")) || (currentlineText[1].Contains("L"))) &&
                                                 (!currentlineText[1].Substring(1).Contains("T")))
                                        {
                                            fsbMessage.TotalAWbPiececs = currentlineText[1] != ""
                                                         ? int.Parse(currentlineText[1].Substring(1, indexofKORL - 1))
                                                         : 0;

                                            fsbMessage.GrossWeight = currentlineText[1] != ""
                                                         ? decimal.Parse(currentlineText[1].Substring(indexofKORL + 1))
                                                         : decimal.Parse("0.0");
                                        }

                                    }
                                }
                                break;
                            default:
                                if (lineText.Trim().Length > 2)
                                {
                                    var tagName = lineText.Substring(0, 3);
                                    switch (tagName)
                                    {
                                        case "RTG":


                                            string[] currentLineRouteText = lineText.Split('/');

                                            if (currentLineRouteText.Length >= 2)
                                            {
                                                for (int k = 1; k < currentLineRouteText.Length; k++)
                                                {
                                                    Route = new MessageData.RouteInformation();
                                                    if (k == 1)
                                                        Route.FlightOrigin = fsbMessage.AWBOrigin;
                                                    else
                                                        Route.FlightOrigin = currentLineRouteText[k - 1].Substring(0, 3);

                                                    if (currentLineRouteText[k].Length == 5)
                                                    {
                                                        Route.Carriercode = currentLineRouteText[k].Substring(3, 2);
                                                        Route.FlightDestination = currentLineRouteText[k].Substring(0, 3);

                                                    }
                                                    RouteIformation.Add(Route);
                                                }
                                            }

                                            break;
                                        case "SHP":
                                            try
                                            {
                                                string currentline = string.Empty;
                                                currentline = ReadFile(tagName, strMessage.Replace("$", "\r\n").Replace("$", "\n"));
                                                string[] currentShipperLineText = currentline.Split('#');
                                                string shpTag = string.Empty;
                                                if (currentShipperLineText.Length >= 1)
                                                {
                                                    string[] currentLineTextSplit = currentShipperLineText[0].Split('/');
                                                    if (currentLineTextSplit.Length >= 1)
                                                    {
                                                        if (currentLineTextSplit[0].Length == 3)
                                                        {
                                                            shpTag = currentLineTextSplit[0] != "" ? currentLineTextSplit[0] : "";
                                                        }
                                                    }
                                                    if (currentLineTextSplit.Length >= 2)
                                                    {
                                                        fsbShipper.ShipperAccountNo = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                    }
                                                }
                                                if (currentShipperLineText.Length >= 2)
                                                {
                                                    string[] currentLineTextSplit = currentShipperLineText[1].Split('/');
                                                    if (currentLineTextSplit.Length >= 2)
                                                    {
                                                        fsbShipper.ShipperName = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                    }
                                                }

                                                if (currentShipperLineText.Length >= 3)
                                                {
                                                    string[] currentLineTextSplit = currentShipperLineText[2].Split('/');
                                                    if (currentLineTextSplit.Length >= 2)
                                                    {

                                                        fsbShipper.ShipperStreetAddress = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                    }
                                                }

                                                if (currentShipperLineText.Length >= 4)
                                                {

                                                    string[] currentLineTextSplit = currentShipperLineText[3].Split('/');
                                                    if (currentLineTextSplit.Length >= 2)
                                                    {
                                                        fsbShipper.ShipperPlace = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                    }
                                                    if (currentLineTextSplit.Length >= 3)
                                                    {
                                                        fsbShipper.ShipperState = currentLineTextSplit[2] != "" ? currentLineTextSplit[2] : "";
                                                    }
                                                }

                                                if (currentShipperLineText.Length >= 5)
                                                {

                                                    string[] currentLineTextSplit = currentShipperLineText[4].Split('/');
                                                    if (currentLineTextSplit.Length >= 2)
                                                    {
                                                        fsbShipper.ShipperCountrycode = currentLineTextSplit[1] != "" ? currentLineTextSplit[1].Substring(0, 2) : "";
                                                    }
                                                    if (currentLineTextSplit.Length >= 3)
                                                    {
                                                        fsbShipper.ShipperPostalCode = currentLineTextSplit[2] != "" ? currentLineTextSplit[2] : "";
                                                    }
                                                    if (currentLineTextSplit.Length >= 4)
                                                    {
                                                        fsbShipper.ShipperContactIdentifier = currentLineTextSplit[3] != "" ? currentLineTextSplit[3] : "";
                                                    }
                                                    if (currentLineTextSplit.Length >= 5)
                                                    {
                                                        fsbShipper.ShipperContactNumber = currentLineTextSplit[4] != "" ? currentLineTextSplit[4] : "";
                                                    }
                                                }

                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure("Error in Decoding FSB Message Shipper(SHP) TAG " + ex.ToString());
                                                _logger.LogError("Error in Decoding FSB Message Shipper(SHP) TAG {0}" , ex);
                                            }
                                            break;
                                        case "CNE":
                                            try
                                            {
                                                string currentline = string.Empty;
                                                currentline = ReadFile(tagName, strMessage.Replace("$", "\r\n").Replace("$", "\n"));
                                                string[] currentConsigneeLineText = currentline.Split('#');
                                                string cneTag = string.Empty;
                                                if (currentConsigneeLineText.Length >= 1)
                                                {
                                                    string[] currentLineTextSplit = currentConsigneeLineText[0].Split('/');
                                                    if (currentLineTextSplit.Length >= 1)
                                                    {
                                                        if (currentLineTextSplit[0].Length == 3)
                                                        {
                                                            cneTag = currentLineTextSplit[0] != "" ? currentLineTextSplit[0] : "";
                                                        }
                                                    }
                                                    if (currentLineTextSplit.Length >= 2)
                                                    {
                                                        fsbConsignee.ConsigneeAccountNo = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                    }
                                                }

                                                if (currentConsigneeLineText.Length >= 2)
                                                {

                                                    string[] currentLineTextSplit = currentConsigneeLineText[1].Split('/');
                                                    if (currentLineTextSplit.Length >= 2)
                                                    {
                                                        fsbConsignee.ConsigneeName = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                    }
                                                }

                                                if (currentConsigneeLineText.Length >= 3)
                                                {

                                                    string[] currentLineTextSplit = currentConsigneeLineText[2].Split('/');
                                                    if (currentLineTextSplit.Length >= 2)
                                                    {
                                                        fsbConsignee.ConsigneeStreetAddress = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                    }
                                                }
                                                if (currentConsigneeLineText.Length >= 4)
                                                {

                                                    string[] currentLineTextSplit = currentConsigneeLineText[3].Split('/');
                                                    if (currentLineTextSplit.Length >= 2)
                                                    {
                                                        fsbConsignee.ConsigneePlace = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                    }
                                                    if (currentLineTextSplit.Length >= 3)
                                                    {
                                                        fsbConsignee.ConsigneeState = currentLineTextSplit[2] != "" ? currentLineTextSplit[2] : "";
                                                    }
                                                }
                                                if (currentConsigneeLineText.Length >= 5)
                                                {
                                                    string[] currentLineTextSplit = currentConsigneeLineText[4].Split('/');
                                                    if (currentLineTextSplit.Length >= 2)
                                                    {
                                                        fsbConsignee.ConsigneeCountrycode = currentLineTextSplit[1] != "" ? currentLineTextSplit[1].Substring(0, 2) : "";
                                                    }
                                                    if (currentLineTextSplit.Length >= 3)
                                                    {
                                                        fsbConsignee.ConsigneePostalCode = currentLineTextSplit[2] != "" ? currentLineTextSplit[2] : "";
                                                    }
                                                    if (currentLineTextSplit.Length >= 4)
                                                    {
                                                        fsbConsignee.ConsigneeContactIdentifier = currentLineTextSplit[3] != "" ? currentLineTextSplit[3] : "";
                                                    }
                                                    if (currentLineTextSplit.Length >= 5)
                                                    {
                                                        fsbConsignee.ConsigneeContactNumber = currentLineTextSplit[4] != "" ? currentLineTextSplit[4] : "";
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure("Error in Decoding FSB Message Consigneee(CNE) TAG " + ex.ToString());
                                                _logger.LogError("Error in Decoding FSB Message Consigneee(CNE) TAG {0}", ex);
                                            }
                                            break;
                                        case "SSR":
                                            try
                                            {
                                                string currentSSRline = string.Empty;
                                                currentSSRline = ReadFile(tagName, strMessage.Replace("$", "\r\n").Replace("$", "\n"));
                                                string[] currentSSRLineText = currentSSRline.Split('#');
                                                if (currentSSRLineText.Length >= 1)
                                                {
                                                    string[] currentLineTextSplit = currentSSRLineText[0].Split('/');
                                                    if (currentLineTextSplit.Length >= 2)
                                                    {
                                                        fsbMessage.ShipmentSendgerReference1 = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                    }
                                                }

                                                if (currentSSRLineText.Length >= 2)
                                                {

                                                    string[] currentLineTextSplit = currentSSRLineText[1].Split('/');
                                                    if (currentLineTextSplit.Length >= 2)
                                                    {
                                                        fsbMessage.ShipmentSendgerReference2 = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure("Error in Decoding FSB Message SSR TAG " + ex.ToString());
                                                _logger.LogError("Error in Decoding FSB Message SSR TAG {0}" , ex);
                                            }
                                            break;
                                        case "WBL":
                                            try
                                            {
                                                string[] currentLineWBLText = lineText.Split('/');
                                                if (currentLineWBLText.Length <= 3)
                                                {
                                                    if (currentLineWBLText.Length > 1)
                                                    {
                                                        if (currentLineWBLText.Length == 2)
                                                            fsbMessage.NatureofGoods1 = currentLineWBLText[1];

                                                        if (currentLineWBLText[1].Length == 1)
                                                        {
                                                            string strWBLIdentifier = currentLineWBLText[1].ToUpper();
                                                            switch (strWBLIdentifier.ToUpper())
                                                            {

                                                                case "G":
                                                                    fsbMessage.NatureofGoods1 = currentLineWBLText[2];
                                                                    break;
                                                                case "C":
                                                                    fsbMessage.NatureofGoods1 = currentLineWBLText[2];
                                                                    break;
                                                                case "V":
                                                                    if (currentLineWBLText[2] != "")
                                                                    {
                                                                        fsbMessage.VolumeCode = currentLineWBLText[2].Substring(0, 2);
                                                                        fsbMessage.AWBVolume = decimal.Parse(currentLineWBLText[2].Substring(2));
                                                                    }
                                                                    break;
                                                                case "U":
                                                                    bupInformation = new MessageData.AWBBUPInformation();
                                                                    bupInformation.ULDNo = currentLineWBLText[2];
                                                                    break;
                                                                case "S":
                                                                    bupInformation.SlacCount = currentLineWBLText[2];
                                                                    bublistinformation.Add(bupInformation);
                                                                    break;
                                                                default:
                                                                    break;

                                                            }
                                                        }
                                                    }
                                                }
                                                if (currentLineWBLText.Length >= 4)
                                                {
                                                    try
                                                    {
                                                        Dimension = new MessageData.FSBDimensionInformation();
                                                        if (currentLineWBLText.Length >= 2)
                                                        {
                                                            if (currentLineWBLText[1].ToUpper() == "D")
                                                            {
                                                                if (currentLineWBLText.Length >= 3)
                                                                {
                                                                    if (currentLineWBLText[2] != "")
                                                                    {
                                                                        Dimension.DimWeightcode = currentLineWBLText[2].ToString().Substring(0, 1);
                                                                        Dimension.DimGrossWeight = decimal.Parse(currentLineWBLText[2].ToString().Substring(1));
                                                                    }

                                                                }
                                                                if (currentLineWBLText.Length >= 4)
                                                                {
                                                                    Dimension.DimUnitCode = currentLineWBLText[3].ToString().Substring(0, 3);
                                                                    string[] strDimTag = currentLineWBLText[3].ToString().Split('-');
                                                                    Dimension.DimLength = int.Parse(strDimTag[0].Substring(3));
                                                                    Dimension.DimWidth = int.Parse(strDimTag[1]);
                                                                    Dimension.DimHeight = int.Parse(strDimTag[2]);

                                                                }
                                                                if (currentLineWBLText.Length >= 5)
                                                                {
                                                                    Dimension.DimPieces = int.Parse(currentLineWBLText[4].ToString());
                                                                }

                                                                Dimensionformation.Add(Dimension);
                                                            }

                                                        }
                                                    }
                                                    catch (Exception ex) {
                                                        // clsLog.WriteLogAzure("Error in Decoding FSB Message WBL Dimesnion TAG " + ex.ToString());
                                                        _logger.LogError("Error in Decoding FSB Message WBL Dimesnion TAG {0}" , ex);
                                                     }
                                                }
                                            }
                                            catch (Exception ex) {
                                                // clsLog.WriteLogAzure("Error in Decoding FSB Message WBL TAG " + ex.ToString());
                                                _logger.LogError("Error in Decoding FSB Message WBL TAG {0}" , ex);
                                             }
                                            break;
                                        case "OSI":
                                            try
                                            {
                                                string[] currentLineOSIText = lineText.Split('/');
                                                if (currentLineOSIText.Length > 1)
                                                    fsbMessage.OtherServiceInformation = currentLineOSIText[1];
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure("Error in Decoding FSB Message OSI TAG " + ex.ToString());
                                                _logger.LogError("Error in Decoding FSB Message OSI TAG {0}" , ex);
                                            }
                                            break;
                                        case "COR":
                                            try
                                            {
                                                string[] currentLineCustomeText = lineText.Split('/');
                                                if (currentLineCustomeText.Length > 1)
                                                    fsbMessage.CustomOrigin = currentLineCustomeText[1];
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure("Error in Decoding FSB Message COR TAG " + ex.ToString());
                                                _logger.LogError("Error in Decoding FSB Message COR TAG {0}",ex);
                                            }
                                            break;
                                        case "REF":
                                            try
                                            {
                                                string[] currentLineReferenceText = lineText.Split('/');
                                                if (currentLineReferenceText.Length >= 1)
                                                    fsbMessage.RefereceOriginTag = currentLineReferenceText[1];
                                                if (currentLineReferenceText.Length >= 2)
                                                    fsbMessage.RefereceFileTag = currentLineReferenceText[2];
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure("Error in Decoding FSB Message REF TAG " + ex.ToString());
                                                _logger.LogError("Error in Decoding FSB Message REF TAG {0}" ,ex);
                                            }
                                            break;
                                        case "SRI":
                                            try
                                            {
                                                string[] currentLineSRIText = lineText.Split('/');
                                                if (currentLineSRIText.Length >= 1)
                                                    fsbMessage.ShipmentReferenceNumber = currentLineSRIText[1];
                                                if (currentLineSRIText.Length >= 2)
                                                    fsbMessage.ShipmentSuplementyInformation = currentLineSRIText[2];
                                                if (currentLineSRIText.Length >= 3)
                                                    fsbMessage.ShipmentSuplementyInformation1 = currentLineSRIText[3];
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure("Error in Decoding FSB Message SRI TAG " + ex.ToString());
                                                _logger.LogError("Error in Decoding FSB Message SRI TAG {0}" , ex);
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                break;
                        }
                        lineNo++;

                    }
                }
                flag = true;
            }
            catch (Exception ex)
            {
                flag = false;
                //SCMExceptionHandling.logexception(ref ex);
                // clsLog.WriteLogAzure("Error in Decoding FSB Message " + ex.ToString());
                _logger.LogError("Error in Decoding FSB Message {0}" , ex.ToString);
            }
            return flag;
        }

        private string ReadFile(string tagName, string strMessage)
        {

            try
            {
                var fsbLine = new StringReader(strMessage);
                string lineText;
                var tagText = string.Empty;
                var readLine = false;
                while ((lineText = fsbLine.ReadLine()) != null)
                {
                    if (readLine)
                    {
                        if (lineText.Trim().Length > 0)
                            if (lineText.Substring(0, 1) == "/")
                                tagText += "#" + lineText;
                            else
                                break;
                    }
                    if (lineText.Trim().Length > 2)
                        if (lineText.Substring(0, 3) == tagName)
                        {
                            tagText = lineText;
                            readLine = true;
                        }
                }
                return tagText;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                throw;
            }
        }

        #endregion


        #region validateSaveFSBMessage
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fsbMessage"></param>
        /// <param name="fsbShipper"></param>
        /// <param name="fsbConsignee"></param>
        /// <param name="RouteIformation"></param>
        /// <param name="Dimensionformation"></param>
        /// <param name="bublistinformation"></param>
        /// <returns></returns>
        public async Task<bool> ValidateAndSaveFSBMessage(MessageData.FSBAWBInformation fsbMessage, MessageData.ShipperInformation fsbShipper, MessageData.ConsigneeInformation fsbConsignee, List<MessageData.RouteInformation> RouteIformation, List<MessageData.FSBDimensionInformation> Dimensionformation, List<MessageData.AWBBUPInformation> bublistinformation, int messageId, string strMessage, string strMessageFrom, string strFromID, string strStatus)
        {
            //SCMExceptionHandlingWorkRole scmexception = new SCMExceptionHandlingWorkRole();
            //SQLServer dtb = new SQLServer();

            AWBOperations objOpsAuditLog = null;
            bool MessageStatus = false;
            try
            {

                string AWbNo = string.Empty, AWBPrefix = string.Empty, strNatureofGoods = string.Empty;
                decimal VolumeWt = 0;
                string flightdate = DateTime.Now.ToString("dd/MM/yyyy");
                AWbNo = fsbMessage.AWBNo;
                AWBPrefix = fsbMessage.AirlinePrefix;

                //GenericFunction gf = new GenericFunction();

                //InBox inBox = null;
                //inBox = new InBox();
                //inBox.Subject = "FSB";
                //inBox.Body = strMessage;
                //inBox.FromiD = string.Empty;
                //inBox.ToiD = string.Empty;
                //inBox.RecievedOn = DateTime.UtcNow;
                //inBox.IsProcessed = true;
                ////inBox.Status = "ReProcessed";
                //inBox.Status = strStatus;
                //inBox.FromiD = strFromID;
                //inBox.Type = "FSB";
                //inBox.UpdatedBy = strMessageFrom;
                //inBox.UpdatedOn = DateTime.UtcNow;
                //inBox.AWBNumber = AWBPrefix + '-' + AWbNo;
                //inBox.FlightNumber = string.Empty;
                //inBox.FlightOrigin = string.Empty;
                //inBox.FlightDestination = string.Empty;
                //inBox.FlightDate = DateTime.Now;
                //inBox.MessageCategory = "CIMP";
                //AuditLog log = new AuditLog();
                //log.SaveLog(LogType.InMessage, string.Empty, string.Empty, inBox);


                _genericFunction.UpdateInboxFromMessageParameter(messageId, AWBPrefix + "-" + AWbNo, string.Empty, string.Empty, string.Empty, "FSB", strMessageFrom, DateTime.Parse("1900-01-01"));
                bool saveStatus = false;

                if (fsbMessage.VolumeCode != null)
                {
                    switch (fsbMessage.VolumeCode)
                    {
                        case "MC":
                            VolumeWt = decimal.Parse(string.Format("{0:0.00}", Convert.ToDecimal(Convert.ToDecimal(fsbMessage.AWBVolume.ToString() == "" ? "0" : fsbMessage.AWBVolume.ToString()) * decimal.Parse("166.66"))));
                            break;
                        default:
                            VolumeWt = fsbMessage.AWBVolume;
                            break;
                    }
                }
                if (fsbMessage.NatureofGoods1 != null)
                    strNatureofGoods = fsbMessage.NatureofGoods1;
                else if (fsbMessage.NatureofGoods2 != null)
                {
                    if (strNatureofGoods != "")
                        strNatureofGoods += "," + fsbMessage.NatureofGoods2;
                    else
                        strNatureofGoods = fsbMessage.NatureofGoods2;
                }


                //string[] paramname = new string[] { "AirlinePrefix", "AWBNum", "Origin", "Dest", "PcsCount", "Weight", "Volume", "ComodityCode", "ComodityDesc", "CarrierCode", "FlightNum", "FlightDate", "FlightOrigin", "FlightDest",
                //                                        "ShipperName", "ShipperAddr", "ShipperPlace", "ShipperState", "ShipperCountryCode", "ShipperContactNo", "ConsName", "ConsAddr", "ConsPlace",
                //                                        "ConsState", "ConsCountryCode", "ConsContactNo", "CustAccNo", "IATACargoAgentCode", "CustName", "SystemDate", "MeasureUnit", "Length", "Breadth", "Height", "PartnerStatus","REFNo",
                //                                    "UpdatedBy","SpecialHandelingCode","Paymode","ShipperPincode","ConsingneePinCode","WeightCode"};

                //object[] paramvalue = new object[] {AWBPrefix,AWbNo,fsbMessage.AWBOrigin, fsbMessage.AWBDestination,fsbMessage.TotalAWbPiececs, fsbMessage.GrossWeight,VolumeWt, "",strNatureofGoods,"","",flightdate, "","",
                //    fsbShipper.ShipperName==null?"":fsbShipper.ShipperName,fsbShipper.ShipperStreetAddress==null?"":fsbShipper.ShipperStreetAddress, fsbShipper.ShipperPlace==null?"":fsbShipper.ShipperPlace, fsbShipper.ShipperState==null?"":fsbShipper.ShipperState, fsbShipper.ShipperCountrycode==null?"":fsbShipper.ShipperCountrycode,  fsbShipper.ShipperContactNumber==null?"":fsbShipper.ShipperContactNumber, fsbConsignee.ConsigneeName==null?"":fsbConsignee.ConsigneeName, fsbConsignee.ConsigneeStreetAddress==null?"":fsbConsignee.ConsigneeStreetAddress, fsbConsignee.ConsigneePlace==null?"":fsbConsignee.ConsigneePlace,
                //    fsbConsignee.ConsigneeState==null?"":fsbConsignee.ConsigneeState,fsbConsignee.ConsigneeCountrycode==null?"":fsbConsignee.ConsigneeCountrycode,fsbConsignee.ConsigneeContactNumber==null?"":fsbConsignee.ConsigneeContactNumber, fsbConsignee.ConsigneeAccountNo==null?"":fsbConsignee.ConsigneeAccountNo, "","", System.DateTime.Now.ToString("yyyy-MM-dd"),"", "", "", "", "",0,
                //                                    "FSB","","",fsbShipper.ShipperPostalCode==null?"":fsbShipper.ShipperPostalCode,fsbConsignee.ConsigneePostalCode==null?"":fsbConsignee.ConsigneePostalCode,fsbMessage.WeightCode};

                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                //                                              SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                //                                              SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.Int,
                //                                            SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar};

                //saveStatus = dtb.InsertData("spInsertBookingDataFromFFR", paramname, paramtype, paramvalue);

                SqlParameter[] parameters =
            {
                new SqlParameter("@AirlinePrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                new SqlParameter("@AWBNum", SqlDbType.VarChar) { Value = AWbNo },
                new SqlParameter("@Origin", SqlDbType.VarChar) { Value = fsbMessage.AWBOrigin },
                new SqlParameter("@Dest", SqlDbType.VarChar) { Value = fsbMessage.AWBDestination },
                new SqlParameter("@PcsCount", SqlDbType.VarChar) { Value = fsbMessage.TotalAWbPiececs },
                new SqlParameter("@Weight", SqlDbType.VarChar) { Value = fsbMessage.GrossWeight },
                new SqlParameter("@Volume", SqlDbType.VarChar) { Value = VolumeWt },
                new SqlParameter("@ComodityCode", SqlDbType.VarChar) { Value = "" },
                new SqlParameter("@ComodityDesc", SqlDbType.VarChar) { Value = strNatureofGoods },
                new SqlParameter("@CarrierCode", SqlDbType.VarChar) { Value = "" },
                new SqlParameter("@FlightNum", SqlDbType.VarChar) { Value = "" },
                new SqlParameter("@FlightDate", SqlDbType.VarChar) { Value = flightdate },
                new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = "" },
                new SqlParameter("@FlightDest", SqlDbType.VarChar) { Value = "" },

                new SqlParameter("@ShipperName", SqlDbType.VarChar) { Value = fsbShipper.ShipperName ?? "" },
                new SqlParameter("@ShipperAddr", SqlDbType.VarChar) { Value = fsbShipper.ShipperStreetAddress ?? "" },
                new SqlParameter("@ShipperPlace", SqlDbType.VarChar) { Value = fsbShipper.ShipperPlace ?? "" },
                new SqlParameter("@ShipperState", SqlDbType.VarChar) { Value = fsbShipper.ShipperState ?? "" },
                new SqlParameter("@ShipperCountryCode", SqlDbType.VarChar) { Value = fsbShipper.ShipperCountrycode ?? "" },
                new SqlParameter("@ShipperContactNo", SqlDbType.VarChar) { Value = fsbShipper.ShipperContactNumber ?? "" },

                new SqlParameter("@ConsName", SqlDbType.VarChar) { Value = fsbConsignee.ConsigneeName ?? "" },
                new SqlParameter("@ConsAddr", SqlDbType.VarChar) { Value = fsbConsignee.ConsigneeStreetAddress ?? "" },
                new SqlParameter("@ConsPlace", SqlDbType.VarChar) { Value = fsbConsignee.ConsigneePlace ?? "" },
                new SqlParameter("@ConsState", SqlDbType.VarChar) { Value = fsbConsignee.ConsigneeState ?? "" },
                new SqlParameter("@ConsCountryCode", SqlDbType.VarChar) { Value = fsbConsignee.ConsigneeCountrycode ?? "" },
                new SqlParameter("@ConsContactNo", SqlDbType.VarChar) { Value = fsbConsignee.ConsigneeContactNumber ?? "" },
                new SqlParameter("@CustAccNo", SqlDbType.VarChar) { Value = fsbConsignee.ConsigneeAccountNo ?? "" },

                new SqlParameter("@IATACargoAgentCode", SqlDbType.VarChar) { Value = "" },
                new SqlParameter("@CustName", SqlDbType.VarChar) { Value = "" },
                new SqlParameter("@SystemDate", SqlDbType.DateTime) { Value = DateTime.Now.ToString("yyyy-MM-dd") },
                new SqlParameter("@MeasureUnit", SqlDbType.VarChar) { Value = "" },
                new SqlParameter("@Length", SqlDbType.VarChar) { Value = "" },
                new SqlParameter("@Breadth", SqlDbType.VarChar) { Value = "" },
                new SqlParameter("@Height", SqlDbType.VarChar) { Value = "" },
                new SqlParameter("@PartnerStatus", SqlDbType.Int) { Value = 0 },
                new SqlParameter("@REFNo", SqlDbType.VarChar) { Value = "FSB" },
                new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "" },
                new SqlParameter("@SpecialHandelingCode", SqlDbType.VarChar) { Value = "" },
                new SqlParameter("@Paymode", SqlDbType.VarChar) { Value = "" },
                new SqlParameter("@ShipperPincode", SqlDbType.VarChar) { Value = fsbShipper.ShipperPostalCode ?? "" },
                new SqlParameter("@ConsingneePinCode", SqlDbType.VarChar) { Value = fsbConsignee.ConsigneePostalCode ?? "" },
                new SqlParameter("@WeightCode", SqlDbType.VarChar) { Value = fsbMessage.WeightCode }
            };

                saveStatus = await _readWriteDao.ExecuteNonQueryAsync("spInsertBookingDataFromFFR", parameters);

                if (saveStatus)
                {
                    objOpsAuditLog = new AWBOperations();
                    objOpsAuditLog.AWBID = 0;
                    objOpsAuditLog.AWBPrefix = AWBPrefix.Trim();
                    objOpsAuditLog.AWBNumber = AWbNo;
                    objOpsAuditLog.Origin = fsbMessage.AWBOrigin.ToUpper();
                    objOpsAuditLog.Destination = fsbMessage.AWBDestination.ToUpper();
                    objOpsAuditLog.FlightNo = string.Empty;
                    objOpsAuditLog.FlightDate = DateTime.UtcNow;
                    objOpsAuditLog.FlightOrigin = string.Empty;
                    objOpsAuditLog.FlightDestination = string.Empty;
                    objOpsAuditLog.BookedPcs = Convert.ToInt32(fsbMessage.TotalAWbPiececs);
                    objOpsAuditLog.BookedWgt = Convert.ToDouble(fsbMessage.GrossWeight);
                    objOpsAuditLog.UOM = fsbMessage.WeightCode;
                    objOpsAuditLog.Createdon = DateTime.UtcNow;
                    objOpsAuditLog.Updatedon = DateTime.UtcNow;
                    objOpsAuditLog.Createdby = strMessageFrom;
                    objOpsAuditLog.Updatedby = strMessageFrom;
                    objOpsAuditLog.Action = "Booked";
                    objOpsAuditLog.Message = "AWB Booked Through FWB";
                    objOpsAuditLog.Description = string.Empty;

                    //log = new AuditLog();
                    //log.SaveLog(LogType.AWBOperations, string.Empty, string.Empty, objOpsAuditLog);



                    #region Save AWB Routing

                    if (RouteIformation.Count > 0)
                    {

                        //string[] paramna = new string[] { "AWBNum", "AWBPrefix" };
                        //object[] paramobj = new object[] { AWbNo, AWBPrefix };
                        //SqlDbType[] paramty = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };

                        SqlParameter[] parameters1 =
                        {
                            new SqlParameter("@AWBNum", SqlDbType.VarChar) { Value = AWbNo },
                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                        };

                        //if (dtb.ExecuteProcedure("spDeleteAWBRouteFFR", paramna, paramty, paramobj))

                        if (await _readWriteDao.ExecuteNonQueryAsync("spDeleteAWBRouteFFR", parameters1))
                        {
                            #region route insert Loop
                            for (int route = 0; route < RouteIformation.Count; route++)
                            {

                                //string[] paramNames = new string[]
                                //{
                                //        "AWBNumber", "FltOrigin", "FltDestination", "FltNumber","FltDate", "Status",
                                //        "UpdatedBy","UpdatedOn","IsFFR", "REFNo","date","AWBPrefix"
                                //};
                                //SqlDbType[] dataTypes = new SqlDbType[]
                                //{   SqlDbType.VarChar,
                                //        SqlDbType.VarChar,
                                //        SqlDbType.VarChar,
                                //        SqlDbType.VarChar,
                                //        SqlDbType.DateTime,
                                //        SqlDbType.VarChar,
                                //        SqlDbType.VarChar,
                                //        SqlDbType.DateTime,
                                //        SqlDbType.Bit,
                                //        SqlDbType.Int,
                                //        SqlDbType.DateTime,
                                //        SqlDbType.VarChar
                                //};

                                //object[] values = new object[]
                                //{
                                //        AWbNo,
                                //        RouteIformation[route].FlightOrigin,
                                //        RouteIformation[route].FlightDestination,
                                //        RouteIformation[route].Carriercode ,
                                //        DateTime.Now.ToString(),
                                //        "C",
                                //        "FSB",
                                //        System.DateTime.Now,
                                //        1,
                                //        0,
                                //        DateTime.Now.ToString(),
                                //       AWBPrefix

                                //};

                                SqlParameter[] sqlParameters = new SqlParameter[]
                                {
                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar)      { Value = AWbNo },
                                    new SqlParameter("@FltOrigin", SqlDbType.VarChar)       { Value = RouteIformation[route].FlightOrigin },
                                    new SqlParameter("@FltDestination", SqlDbType.VarChar)  { Value = RouteIformation[route].FlightDestination },
                                    new SqlParameter("@FltNumber", SqlDbType.VarChar)       { Value = RouteIformation[route].Carriercode },
                                    new SqlParameter("@FltDate", SqlDbType.DateTime)        { Value = DateTime.Now.ToString() },
                                    new SqlParameter("@Status", SqlDbType.VarChar)          { Value = "C" },
                                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar)       { Value = "FSB" },
                                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime)      { Value = DateTime.Now },
                                    new SqlParameter("@IsFFR", SqlDbType.Bit)               { Value = 1 },
                                    new SqlParameter("@REFNo", SqlDbType.Int)               { Value = 0 },
                                    new SqlParameter("@Date", SqlDbType.DateTime)           { Value = DateTime.Now.ToString() },
                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar)       { Value = AWBPrefix }
                                };


                                //if (!dtb.UpdateData("spSaveFFRAWBRoute", paramNames, dataTypes, values))
                                if (!await _readWriteDao.ExecuteNonQueryAsync("spSaveFFRAWBRoute", sqlParameters))
                                {
                                    //clsLog.WriteLogAzure("Error in Save AWB Route FSB " + dtb.LastErrorDescription);
                                    // clsLog.WriteLogAzure("Error in Save AWB Route FSB ");
                                    _logger.LogWarning("Error in Save AWB Route FSB ");
                                }

                                objOpsAuditLog = new AWBOperations();
                                objOpsAuditLog.AWBID = 0;
                                objOpsAuditLog.AWBPrefix = AWBPrefix.Trim();
                                objOpsAuditLog.AWBNumber = AWbNo;
                                objOpsAuditLog.Origin = fsbMessage.AWBOrigin.ToUpper();
                                objOpsAuditLog.Destination = fsbMessage.AWBDestination.ToUpper();
                                objOpsAuditLog.FlightNo = RouteIformation[route].Carriercode;
                                objOpsAuditLog.FlightDate = DateTime.Now;
                                objOpsAuditLog.FlightOrigin = RouteIformation[route].FlightOrigin;
                                objOpsAuditLog.FlightDestination = RouteIformation[route].FlightDestination;
                                objOpsAuditLog.BookedPcs = fsbMessage.TotalAWbPiececs;
                                objOpsAuditLog.BookedWgt = Convert.ToDouble(fsbMessage.GrossWeight);
                                objOpsAuditLog.UOM = fsbMessage.WeightCode;
                                objOpsAuditLog.Createdon = DateTime.UtcNow;
                                objOpsAuditLog.Updatedon = DateTime.UtcNow;
                                objOpsAuditLog.Createdby = strMessageFrom;
                                objOpsAuditLog.Updatedby = strMessageFrom;
                                objOpsAuditLog.Action = "Booked";
                                objOpsAuditLog.Message = "AWB Flight Information";
                                objOpsAuditLog.Description = string.Empty;

                                //log = new AuditLog();
                                //log.SaveLog(LogType.AWBOperations, string.Empty, string.Empty, objOpsAuditLog);

                                //string[] CANname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public" };
                                //SqlDbType[] CAType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit };
                                //object[] CAValues = new object[] { AWBPrefix, AWbNo, fsbMessage.AWBOrigin, fsbMessage.AWBDestination, fsbMessage.TotalAWbPiececs, fsbMessage.GrossWeight, RouteIformation[route].Carriercode, DateTime.Now.ToString(), RouteIformation[route].FlightOrigin, RouteIformation[route].FlightDestination, "Booked", "FSB", "AWB Flight Information", "FSB", DateTime.Now.ToString(), 1 };

                                SqlParameter[] sqlParamsAuditLog =
                                [
                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar)        { Value = AWBPrefix },
                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar)        { Value = AWbNo },
                                    new SqlParameter("@Origin", SqlDbType.VarChar)           { Value = fsbMessage.AWBOrigin },
                                    new SqlParameter("@Destination", SqlDbType.VarChar)      { Value = fsbMessage.AWBDestination },
                                    new SqlParameter("@Pieces", SqlDbType.VarChar)          { Value = fsbMessage.TotalAWbPiececs },
                                    new SqlParameter("@Weight", SqlDbType.VarChar)          { Value = fsbMessage.GrossWeight },
                                    new SqlParameter("@FlightNo", SqlDbType.VarChar)        { Value = RouteIformation[route].Carriercode },
                                    new SqlParameter("@FlightDate", SqlDbType.DateTime)     { Value = DateTime.Now.ToString() },
                                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar)    { Value = RouteIformation[route].FlightOrigin },
                                    new SqlParameter("@FlightDestination", SqlDbType.VarChar){ Value = RouteIformation[route].FlightDestination },
                                    new SqlParameter("@Action", SqlDbType.VarChar)          { Value = "Booked" },
                                    new SqlParameter("@Message", SqlDbType.VarChar)         { Value = "FSB" },
                                    new SqlParameter("@Description", SqlDbType.VarChar)     { Value = "AWB Flight Information" },
                                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar)       { Value = "FSB" },
                                    new SqlParameter("@UpdatedOn", SqlDbType.VarChar)       { Value = DateTime.Now.ToString() },
                                    new SqlParameter("@Public", SqlDbType.Bit)              { Value = 1 }
                                ];

                                //if (!dtb.ExecuteProcedure("SPAddAWBAuditLog", CANname, CAType, CAValues))
                                if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", sqlParamsAuditLog))
                                    // clsLog.WriteLog("AWB Audit log  for:" + AWbNo + Environment.NewLine);
                                    _logger.LogWarning("AWB Audit log  for: {0}" , AWbNo + Environment.NewLine);
                            }

                            #region Deleting AWB Data if No Route Present

                            //string[] QueryNames = { "AWBPrefix", "AWBNumber" };
                            //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                            //object[] QueryValues = { AWBPrefix, AWbNo };

                            SqlParameter[] sqlParamsDeleteAWB =
                            {
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWbNo }
                            };

                            //if (!dtb.UpdateData("spDeleteAWBDetailsNoRoute", QueryNames, QueryTypes, QueryValues))
                            if (!await _readWriteDao.ExecuteNonQueryAsync("spDeleteAWBDetailsNoRoute", sqlParamsDeleteAWB))
                                // clsLog.WriteLogAzure("Error in Deleting AWB Details ");
                                _logger.LogWarning("Error in Deleting AWB Details ");


                            #endregion

                            #endregion

                        }
                    }

                    #endregion Save AWB Routing

                    #region AWB Dimensions


                    if (Dimensionformation.Count > 0)
                    {
                        //Badiuz khan
                        //Description: Delete Dimension if Dimension 
                        //string[] dparam = { "AWBPrefix", "AWBNumber" };
                        //SqlDbType[] dbparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                        //object[] dbparamvalues = { AWBPrefix, AWbNo };

                        SqlParameter[] sqlParamsDeleteDimension =
                        {
                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                            new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWbNo }
                        };

                        //if (!dtb.InsertData("SpDeleteDimensionThroughMessage", dparam, dbparamtypes, dbparamvalues))
                        if (!await _readWriteDao.ExecuteNonQueryAsync("SpDeleteDimensionThroughMessage", sqlParamsDeleteDimension))
                            // clsLog.WriteLogAzure("Error  Delete Dimension Through Message :" + AWbNo);
                            _logger.LogError("Error  Delete Dimension Through Message :{0}" , AWbNo);
                        else
                        {

                            for (int i = 0; i < Dimensionformation.Count; i++)
                            {
                                string DimunitCode = string.Empty;
                                if (Dimensionformation[i].DimUnitCode.Trim() != "")
                                {
                                    if (Dimensionformation[i].DimUnitCode.Trim().ToUpper() == "CMT")
                                        DimunitCode = "Cms";
                                    else
                                        DimunitCode = "Inches";
                                }

                                //string[] param = { "AWBNumber", "RowIndex", "Length", "Breadth", "Height", "PcsCount", "MeasureUnit", "AWBPrefix", "Weight", "WeightCode" };
                                //SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Decimal, SqlDbType.VarChar };

                                //object[] value ={AWbNo,"1",Dimensionformation[i].DimLength,Dimensionformation[i].DimWidth,Dimensionformation[i].DimHeight,
                                //            Dimensionformation[i].DimPieces,DimunitCode,AWBPrefix,Dimensionformation[i].DimGrossWeight,Dimensionformation[i].DimWeightcode};

                                SqlParameter[] sqlParamsInsertDimension =
                                {
                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar)   { Value = AWbNo },
                                    new SqlParameter("@RowIndex", SqlDbType.Int)        { Value = "1" },
                                    new SqlParameter("@Length", SqlDbType.Int)          { Value = Dimensionformation[i].DimLength },
                                    new SqlParameter("@Breadth", SqlDbType.Int)         { Value = Dimensionformation[i].DimWidth },
                                    new SqlParameter("@Height", SqlDbType.Int)          { Value = Dimensionformation[i].DimHeight },
                                    new SqlParameter("@PcsCount", SqlDbType.Int)       { Value = Dimensionformation[i].DimPieces },
                                    new SqlParameter("@MeasureUnit", SqlDbType.VarChar) { Value = DimunitCode },
                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar)   { Value = AWBPrefix },
                                    new SqlParameter("@Weight", SqlDbType.Decimal)      { Value = Dimensionformation[i].DimGrossWeight },
                                    new SqlParameter("@WeightCode", SqlDbType.VarChar)  { Value = Dimensionformation[i].DimWeightcode }
                                };

                                //if (!dtb.InsertData("SP_SaveAWBDimensions_FFR", param, dbtypes, value))
                                if (!await _readWriteDao.ExecuteNonQueryAsync("SP_SaveAWBDimensions_FFR", sqlParamsInsertDimension))
                                {
                                    // clsLog.WriteLogAzure("Error Saving  Dimension Through Message :" + AWbNo);
                                    _logger.LogWarning("Error Saving  Dimension Through Message : {0}" , AWbNo);
                                }
                            }
                        }
                    }

                    #endregion

                    #region FWB Message with BUP Shipment
                    //Badiuz khan
                    //Description: Save Bup through FWB

                    if (bublistinformation.Count > 0)
                    {

                        int uldslacPcs = 0;
                        for (int k = 0; k < bublistinformation.Count; k++)
                        {
                            if (bublistinformation[k].ULDNo.Length == 10)
                            {

                                string uldno = bublistinformation[k].ULDNo;
                                if (bublistinformation[k].SlacCount != "")
                                    uldslacPcs = int.Parse(bublistinformation[k].SlacCount);

                                //string[] param = { "AWBPrefix", "AWBNumber", "ULDNo", "SlacPcs", "PcsCount", "Volume", "GrossWeight" };
                                //SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.Decimal, SqlDbType.Decimal };
                                //object[] value = { AWBPrefix, AWbNo, uldno, uldslacPcs, fsbMessage.TotalAWbPiececs, VolumeWt, fsbMessage.GrossWeight };

                                SqlParameter[] sqlParamsInsertBUP =
                                {
                                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar)   { Value = AWBPrefix },
                                    new SqlParameter("@AWBNumber", SqlDbType.VarChar)   { Value = AWbNo },
                                    new SqlParameter("@ULDNo", SqlDbType.VarChar)       { Value = uldno },
                                    new SqlParameter("@SlacPcs", SqlDbType.Int)         { Value = uldslacPcs },
                                    new SqlParameter("@PcsCount", SqlDbType.Int)       { Value = fsbMessage.TotalAWbPiececs },
                                    new SqlParameter("@Volume", SqlDbType.Decimal)      { Value = VolumeWt },
                                    new SqlParameter("@GrossWeight", SqlDbType.Decimal) { Value = fsbMessage.GrossWeight }
                                };

                                //if (!dtb.InsertData("SaveandUpdateShippperBUPThroughFWB", param, dbtypes, value))
                                if (!await _readWriteDao.ExecuteNonQueryAsync("SaveandUpdateShippperBUPThroughFWB", sqlParamsInsertBUP))
                                    // clsLog.WriteLogAzure("BUP ULD is not Updated  for:" + AWbNo + Environment.NewLine);
                                    _logger.LogWarning("BUP ULD is not Updated  for: {0}" , AWbNo + Environment.NewLine);
                            }
                        }
                    }

                    #endregion
                }
                clsLog.WriteLogAzure("FSB Message Processing for " + AWbNo);
                MessageStatus = true;

            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref ex);
                // clsLog.WriteLogAzure("Error on FSB Message Processing " + ex.ToString());
                _logger.LogError("Error on FSB Message Processing {0}" , ex);
                MessageStatus = false;
            }
            return MessageStatus;
        }
        #endregion  
    }
}
