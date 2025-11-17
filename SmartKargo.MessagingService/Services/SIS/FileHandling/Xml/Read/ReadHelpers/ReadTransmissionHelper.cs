using System;
using System.Reflection;

using System.Xml;
using QidWorkerRole.SIS.FileHandling.Xml.Read.SupportingModels;
using Microsoft.Extensions.Logging;

namespace QidWorkerRole.SIS.FileHandling.Xml.Read.ReadHelpers
{
    /// <summary>
    /// To Read Transmission Helper From XML File.
    /// </summary>
    public static class ReadTransmissionHelper
    {
  
        #region Read Transmission Header.
        private static ILoggerFactory? _loggerFactory;
        private static ILogger<XmlReaderHelper> _staticLogger => _loggerFactory?.CreateLogger<XmlReaderHelper>();

        public static void Init(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// To Read the XML File Transmission Header.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="transmissionHeader">TransmissionHeader</param>
        public static TransmissionHeader ReadTransmissionHeader(XmlTextReader xmlTextReader, TransmissionHeader transmissionHeader)
        {
            try
            {
                //Logger.Info("Start of ReadTransmissionHeader.");

                if ((transmissionHeader != null) && (xmlTextReader != null))
                {
                    while (xmlTextReader.Read())
                    {
                        if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.TransmissionHeader)))
                        {
                            break;
                        }

                        if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                        {
                            switch (xmlTextReader.LocalName)
                            {
                                case XmlConstants.TransmissionDateTime:
                                    xmlTextReader.Read();
                                    transmissionHeader.TransmissionDateTime = Convert.ToDateTime(xmlTextReader.Value);
                                    break;
                                case XmlConstants.Version:
                                    xmlTextReader.Read();
                                    transmissionHeader.Version = xmlTextReader.Value;
                                    break;
                                case XmlConstants.TransmissionID:
                                    xmlTextReader.Read();
                                    transmissionHeader.TransmissionId = xmlTextReader.Value;
                                    break;
                                case XmlConstants.IssuingOrganizationID:
                                    xmlTextReader.Read();
                                    transmissionHeader.IssuingOrganizationId = xmlTextReader.Value;
                                    break;
                                case XmlConstants.ReceivingOrganizationID:
                                    xmlTextReader.Read();
                                    transmissionHeader.ReceivingOrganizationId = xmlTextReader.Value;
                                    break;
                                case XmlConstants.BillingCategory:
                                    xmlTextReader.Read();
                                    transmissionHeader.BillingCategory = xmlTextReader.Value;
                                    break;
                            }
                        }
                    }
                }
                //Logger.Info("End of ReadTransmissionHeader.");
                return transmissionHeader;
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadTransmissionHeader", xmlException);
                _staticLogger.LogError("Error Occurred in ReadTransmissionHeader {0}", xmlException);
                return null;
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadTransmissionHeader", exception);
                _staticLogger.LogError("Error Occurred in ReadTransmissionHeader {0}", exception);
                return null;
            }
        }

        #endregion

        #region Read Transmission Summary.

        /// <summary>
        /// To Read the XML File Transmission Summary.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="transmissionSummary">TransmissionSummary</param>
        public static void ReadTransmissionSummary(XmlTextReader xmlTextReader, TransmissionSummary transmissionSummary)
        {
            try
            {
                //Logger.Info("Start of ReadTransmissionSummary.");
                _staticLogger.LogInformation("Start of ReadTransmissionSummary.");
                if ((transmissionSummary != null) && (xmlTextReader != null))
                {
                    while (xmlTextReader.Read())
                    {
                        if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.TransmissionSummary)))
                        {
                            break;
                        }

                        if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                        {
                            switch (xmlTextReader.LocalName)
                            {
                                case XmlConstants.InvoiceCount:
                                    xmlTextReader.Read();
                                    transmissionSummary.InvoiceCount = Convert.ToInt32(xmlTextReader.Value);
                                    break;
                                case XmlConstants.TotalAmount:
                                case XmlConstants.TotalAddOnChargeAmount:
                                case XmlConstants.TotalTaxAmount:
                                case XmlConstants.TotalVATAmount:
                                    if (xmlTextReader.HasAttributes)
                                    {
                                        xmlTextReader.MoveToAttribute(0);
                                        xmlTextReader.Read();
                                        transmissionSummary.TotalInvoiceAmount.Add(Convert.ToDecimal(xmlTextReader.Value));
                                    }
                                    break;
                            }
                        }
                    }
                }
                //Logger.Info("End of ReadTransmissionSummary.");
                _staticLogger.LogInformation("End of ReadTransmissionSummary.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadTransmissionSummary", xmlException);
                _staticLogger.LogError("Error Occurred in ReadTransmissionSummary {0}", xmlException);
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadTransmissionSummary", exception);
                _staticLogger.LogError("Error Occurred in ReadTransmissionSummary {0}", exception);
            }
        }

        #endregion

        /// <summary>
        /// To know whether to continue reading the Xml file or not.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="readerCondition">readerCondition</param>
        /// <returns>True or False.</returns>
        public static bool IsContinue(XmlTextReader xmlTextReader, string readerCondition)
        {
            bool isContinue = true;

            try
            {
                
                if (xmlTextReader.LocalName == readerCondition)
                {
                    isContinue = false;
                }

                return isContinue;
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure("Error Occurred in IsContinue", exception);
                _staticLogger.LogError("Error Occurred in IsContinue {0}", exception);
                return isContinue;
            }
        }
    }
}