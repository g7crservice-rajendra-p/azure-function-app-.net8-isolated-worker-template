using System;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace QidWorkerRole.SIS.FileHandling.Xml.Write.WriteHelpers
{
    public sealed partial class XmlWriterHelper
    {
        /// <summary>
        /// To Write the addonCharges to the XML File. Common Method to be called.
        /// </summary>
        /// <param name="xmlTextWriter">xmlTextWriter</param>
        /// <param name="addOnChargeName">addOnChargeName</param>
        /// <param name="addOnChargeAmount">addOnChargeAmount</param>
        /// <param name="addOnChargePercent">addOnChargePercent</param>
        
        private static ILoggerFactory? _loggerFactory;
        private static ILogger<XmlWriterHelper> _staticLogger => _loggerFactory?.CreateLogger<XmlWriterHelper>();

        public XmlWriterHelper(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
        
        private static void WriteAddonCharges(XmlTextWriter xmlTextWriter, string addOnChargeName, double addOnChargeAmount, string addOnChargePercent = null)
        {
            try
            {
                //Logger.Info("Start of WriteAddonCharges.");

                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.AddOnCharges);

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.AddOnChargeName, addOnChargeName);

                if (addOnChargePercent != null)
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.AddOnChargePercentage, String.Format("{0:0.00}", Convert.ToDouble(addOnChargePercent)));
                }

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.AddOnChargeAmount, String.Format("{0:0.000}", addOnChargeAmount));

                xmlTextWriter.WriteEndElement();

                //Logger.Info("End of WriteAddonCharges.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in WriteAddonCharges", xmlException);
                _staticLogger?.LogError("Error Occurred in WriteAddonCharges {0}", xmlException);
            }
        }

        /// <summary>
        ///To write the addonCharge for Other Charges.
        /// </summary>
        /// <param name="xmlTextWriter"></param>
        /// <param name="addonChargeName"></param>
        /// <param name="addonChargeCode"></param>
        /// <param name="addonChargeAmount"></param>
        /// <param name="addonChargePercentage"></param>
        private static void WriteOtherChargeAddonCharges(XmlTextWriter xmlTextWriter, string addonChargeName, string addonChargeCode, decimal addonChargeAmount, string addonChargePercentage = null)
        {
            try
            {
                //Logger.Info("Start of WriteOtherChargeAddonCharges.");

                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.AddOnCharges);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.AddOnChargeName, addonChargeName);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.AddOnChargeCode, addonChargeCode);

                if (addonChargePercentage != null)
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.AddOnChargePercentage, String.Format("{0:0.00}", Convert.ToDouble(addonChargePercentage)));
                }

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.AddOnChargeAmount, String.Format("{0:0.000}", addonChargeAmount));

                xmlTextWriter.WriteEndElement();

                //Logger.Info("End of WriteOtherChargeAddonCharges.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in WriteOtherChargeAddonCharges", xmlException);
                _staticLogger?.LogError("Error Occurred in WriteOtherChargeAddonCharges {0}", xmlException);
            }
        }
    }
}