using System;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.Logging;
using QidWorkerRole.SIS.Model;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.FileHandling.Xml.Write.WriteHelpers
{
    public sealed partial class XmlWriterHelper
    {
        /// <summary>
        /// To Write Vat.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="classObject">classObject</param>
        /// <param name="lineItemDetailType">lineItemDetailType</param>
        private static void WriteVat(XmlTextWriter xmlTextWriter, object classObject, string lineItemDetailType)
        {
            try
            {
                //Logger.InfoFormat("Start Of WriteVat for {0}", lineItemDetailType);

                switch (lineItemDetailType)
                {
                    case XmlConstants.AirWayBill:
                        var airWayBill = classObject as AirWayBill;
                        if (airWayBill != null && airWayBill.VATAmount.HasValue && airWayBill.VATAmount != 0)
                        {
                            xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Tax);
                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxType, XmlConstants.VAT);
                            WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Billed, String.Format("{0:0.000}", airWayBill.VATAmount));

                            foreach (var aWBVAT in airWayBill.AWBVATList)
                            {
                                WriteVatDetails(xmlTextWriter, aWBVAT);
                            }

                            foreach (var aWBOtherCharges in airWayBill.AWBOtherChargesList)
                            {
                                if (aWBOtherCharges != null)
                                {
                                    WriteOtherChargeTax(xmlTextWriter, aWBOtherCharges, XmlConstants.AirWayBill);
                                }
                            }
                            xmlTextWriter.WriteEndElement();
                        }
                        break;
                    case XmlConstants.RejectionMemo:
                        var rejectionMemo = classObject as RejectionMemo;
                        if (rejectionMemo != null)
                        {
                            if ((rejectionMemo.BilledTotalVatAmount.HasValue && rejectionMemo.BilledTotalVatAmount != 0)
                                || (rejectionMemo.AcceptedTotalVatAmount.HasValue && rejectionMemo.AcceptedTotalVatAmount != 0)
                                || (rejectionMemo.TotalVatAmountDifference.HasValue && rejectionMemo.TotalVatAmountDifference != 0)
                                || (rejectionMemo.RMVATList.Count != 0))
                            {
                                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Tax);

                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxType, XmlConstants.VAT);
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Billed, String.Format("{0:0.000}", rejectionMemo.BilledTotalVatAmount));
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Accepted, String.Format("{0:0.000}", rejectionMemo.AcceptedTotalVatAmount));
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Difference, String.Format("{0:0.000}", rejectionMemo.TotalVatAmountDifference));

                                foreach (var vat in rejectionMemo.RMVATList)
                                {
                                    WriteVatDetails(xmlTextWriter, vat);
                                }

                                xmlTextWriter.WriteEndElement();
                            }
                        }
                        break;
                    case XmlConstants.RMAirWayBill:
                        var rMAirWayBill = classObject as RMAirWayBill;
                        if (rMAirWayBill != null)
                        {
                            if (rMAirWayBill.RMAWBVATList != null || rMAirWayBill.RMAWBOtherChargesList != null)
                            {
                                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Tax);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxType, XmlConstants.VAT);
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Billed, String.Format("{0:0.000}", rMAirWayBill.BilledVatAmount));
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Accepted, String.Format("{0:0.000}", rMAirWayBill.AcceptedVatAmount));
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Difference, String.Format("{0:0.000}", rMAirWayBill.VatAmountDifference));

                                if (rMAirWayBill.RMAWBVATList != null)
                                    foreach (var rMAWBVAT in rMAirWayBill.RMAWBVATList)
                                    {
                                        WriteVatDetails(xmlTextWriter, rMAWBVAT);
                                    }

                                foreach (var rMAWBOtherCharges in rMAirWayBill.RMAWBOtherChargesList)
                                {
                                    if (rMAWBOtherCharges != null)
                                    {
                                        WriteOtherChargeTax(xmlTextWriter, rMAWBOtherCharges, XmlConstants.RMAirWayBill);
                                    }
                                }
                                xmlTextWriter.WriteEndElement();
                            }
                        }
                        break;
                    case XmlConstants.BillingMemo:
                        var billingMemo = classObject as BillingMemo;
                        if (billingMemo != null)
                        {
                            if ((billingMemo.BilledTotalVatAmount.HasValue && billingMemo.BilledTotalVatAmount != 0) || billingMemo.BMVATList.Count > 0)
                            {
                                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Tax);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxType, XmlConstants.VAT);
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Billed, String.Format("{0:0.000}", billingMemo.BilledTotalVatAmount));

                                foreach (var vat in billingMemo.BMVATList)
                                {
                                    WriteVatDetails(xmlTextWriter, vat);
                                }

                                xmlTextWriter.WriteEndElement();
                            }
                        }
                        break;
                    case XmlConstants.BMAirWayBill:
                        var bMAirWayBill = classObject as BMAirWayBill;
                        if (bMAirWayBill != null)
                        {
                            if (bMAirWayBill.BilledVatAmount.HasValue && bMAirWayBill.BilledVatAmount != 0)
                            {
                                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Tax);

                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxType, XmlConstants.VAT);
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Billed, String.Format("{0:0.000}", bMAirWayBill.BilledVatAmount));

                                foreach (var bMAWBVAT in bMAirWayBill.BMAWBVATList)
                                {
                                    WriteVatDetails(xmlTextWriter, bMAWBVAT);
                                }

                                foreach (var bMAWBOtherCharges in bMAirWayBill.BMAWBOtherChargesList)
                                {
                                    if (bMAWBOtherCharges != null)
                                    {
                                        WriteOtherChargeTax(xmlTextWriter, bMAWBOtherCharges, XmlConstants.BMAirWayBill);
                                    }
                                }
                                xmlTextWriter.WriteEndElement();
                            }
                        }
                        break;
                    case XmlConstants.CreditMemo:
                        var creditMemo = classObject as CreditMemo;
                        if (creditMemo != null)
                        {
                            if ((creditMemo.TotalVatAmountCredited.HasValue && creditMemo.TotalVatAmountCredited != 0) || creditMemo.CMVATList.Count > 0)
                            {
                                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Tax);

                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxType, XmlConstants.VAT);
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Billed, String.Format("{0:0.000}", creditMemo.TotalVatAmountCredited));

                                foreach (var vat in creditMemo.CMVATList)
                                {
                                    WriteVatDetails(xmlTextWriter, vat);
                                }
                                xmlTextWriter.WriteEndElement();
                            }
                        }
                        break;
                    case XmlConstants.CMAirWayBill:
                        var cMAirWayBill = classObject as CMAirWayBill;
                        if (cMAirWayBill != null && cMAirWayBill.CreditedVatAmount.HasValue && cMAirWayBill.CreditedVatAmount != 0)
                        {
                            xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Tax);
                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxType, XmlConstants.VAT);
                            WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Billed, String.Format("{0:0.000}", cMAirWayBill.CreditedVatAmount));

                            foreach (var cMAWBVAT in cMAirWayBill.CMAWBVATList)
                            {
                                WriteVatDetails(xmlTextWriter, cMAWBVAT);
                            }

                            foreach (var cMAWBOtherCharges in cMAirWayBill.CMAWBOtherChargesList)
                            {
                                if (cMAWBOtherCharges != null)
                                {
                                    WriteOtherChargeTax(xmlTextWriter, cMAWBOtherCharges, XmlConstants.CMAirWayBill);
                                }
                            }
                            xmlTextWriter.WriteEndElement();
                        }
                        break;
                }
                //Logger.InfoFormat("End Of WriteVat for {0}", lineItemDetailType);
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in WriteVat for {0}, Error Message: {1}, Error: {2}", xmlException);
                _staticLogger?.LogError("Error Occurred in WriteVat for {0},  Error Message: {1}", lineItemDetailType, xmlException);
            }
        }

        /// <summary>
        /// To Write Vat Details
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="vAT">VAT</param>
        /// <param name="isLineItem">isLineItem</param>
        private static void WriteVatDetails(XmlTextWriter xmlTextWriter, VAT vAT, bool isLineItem = false)
        {
            try
            {
                //Logger.Info("Start of WriteVatDetails");

                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.TaxBreakdown);

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxLabel, vAT.VatLabel);
                if (vAT.VatIdentifier != null)
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxIdentifier, vAT.VatIdentifier);
                }
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxableAmount, String.Format("{0:0.000}", vAT.VatBaseAmount));

                var percentAfterDecimal = 0;
                if (vAT.VatPercentage != 0.0)
                {
                    var arrDecimalSplit = vAT.VatPercentage.ToString().Split('.');
                    if (arrDecimalSplit.Length > 1 && arrDecimalSplit[1].Length >= 3)
                    {
                        percentAfterDecimal = Convert.ToInt32(arrDecimalSplit[1].ToList().ElementAt(2).ToString());
                    }
                }

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxPercent, percentAfterDecimal != 0 ? String.Format("{0:0.000}", vAT.VatPercentage)
                                                                                                       : String.Format("{0:0.00}", vAT.VatPercentage));

                if (isLineItem)
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxAmount, String.Format("{0:0.000}", vAT.VatCalculatedAmount));
                }
                else
                {
                    WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Billed, String.Format("{0:0.000}", vAT.VatCalculatedAmount));
                }
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxText, vAT.VatText);

                xmlTextWriter.WriteEndElement();

                //Logger.Info("End of WriteVatDetails");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in WriteVatDetails, Error Message: {0}, Error: {1}", xmlException);
                _staticLogger?.LogError("Error Occurred in WriteVatDetails,  Error Message: {0}", xmlException);
            }
        }

        /// <summary>
        /// To Write OtherCharge Tax
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="classObject">classObject</param>
        /// <param name="lineItemDetailType">lineItemDetailType</param>
        private static void WriteOtherChargeTax(XmlTextWriter xmlTextWriter, object classObject, string lineItemDetailType)
        {
            try
            {
                //Logger.InfoFormat("Start of WriteOtherChargeTax for {0}", lineItemDetailType);

                switch (lineItemDetailType)
                {
                    case XmlConstants.AirWayBill:
                        var aWBOtherCharges = classObject as AWBOtherCharges;

                        if (aWBOtherCharges != null)
                        {
                            if (aWBOtherCharges.OtherChargeVatCalculatedAmount != null)
                            {
                                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.TaxBreakdown);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxCode, aWBOtherCharges.OtherChargeCode);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxLabel, aWBOtherCharges.OtherChargeVatLabel);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxIdentifier, XmlConstants.OC);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxableAmount, String.Format("{0:0.000}", aWBOtherCharges.OtherChargeVatBaseAmount));
                                
                                var percentAfterDecimal = 0;
                                if (aWBOtherCharges.OtherChargeVatPercentage != 0.0)
                                {
                                    var arrDecimalSplit = aWBOtherCharges.OtherChargeVatPercentage.ToString().Split('.');

                                    if (arrDecimalSplit.Length > 1 && arrDecimalSplit[1].Length >= 3)
                                    {
                                        percentAfterDecimal = Convert.ToInt32(arrDecimalSplit[1].ToList().ElementAt(2).ToString());
                                    }
                                }
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxPercent, percentAfterDecimal != 0
                                                                                              ? String.Format("{0:0.000}", aWBOtherCharges.OtherChargeVatPercentage)
                                                                                              : String.Format("{0:0.00}", aWBOtherCharges.OtherChargeVatPercentage));

                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Billed, String.Format("{0:0.000}", aWBOtherCharges.OtherChargeVatCalculatedAmount));
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxText, aWBOtherCharges.OtherChargeVatText);

                                xmlTextWriter.WriteEndElement();
                            }
                        }
                        break;
                    case XmlConstants.RMAirWayBill:
                        var rMAWBOtherCharges = classObject as RMAWBOtherCharges;
                        if (rMAWBOtherCharges != null)
                        {
                            if (rMAWBOtherCharges.OtherChargeVatCalculatedAmount != null)
                            {
                                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.TaxBreakdown);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxCode, rMAWBOtherCharges.OtherChargeCode);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxLabel, rMAWBOtherCharges.OtherChargeVatLabel);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxIdentifier, XmlConstants.OC);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxableAmount, String.Format("{0:0.000}", rMAWBOtherCharges.OtherChargeVatBaseAmount));
                                
                                var percentAfterDecimal = 0;
                                if (rMAWBOtherCharges.OtherChargeVatPercentage != 0.0)
                                {
                                    var arrDecimalSplit = rMAWBOtherCharges.OtherChargeVatPercentage.ToString().Split('.');

                                    if (arrDecimalSplit.Length > 1 && arrDecimalSplit[1].Length >= 3)
                                    {
                                        percentAfterDecimal = Convert.ToInt32(arrDecimalSplit[1].ToList().ElementAt(2).ToString());
                                    }
                                }

                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxPercent, percentAfterDecimal != 0
                                                                                              ? String.Format("{0:0.000}", rMAWBOtherCharges.OtherChargeVatPercentage)
                                                                                              : String.Format("{0:0.00}", rMAWBOtherCharges.OtherChargeVatPercentage));

                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Billed, String.Format("{0:0.000}", rMAWBOtherCharges.OtherChargeVatCalculatedAmount));
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxText, rMAWBOtherCharges.OtherChargeVatText);

                                xmlTextWriter.WriteEndElement();
                            }
                        }
                        break;
                    case XmlConstants.BMAirWayBill:
                        var bMAWBOtherCharges = classObject as BMAWBOtherCharges;
                        if (bMAWBOtherCharges != null)
                        {
                            if (bMAWBOtherCharges.OtherChargeVatCalculatedAmount != null)
                            {
                                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.TaxBreakdown);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxCode, bMAWBOtherCharges.OtherChargeCode);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxLabel, bMAWBOtherCharges.OtherChargeVatLabel);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxIdentifier, XmlConstants.OC);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxableAmount, String.Format("{0:0.000}", bMAWBOtherCharges.OtherChargeVatBaseAmount));
                                
                                var percentAfterDecimal = 0;
                                if (bMAWBOtherCharges.OtherChargeVatPercentage != 0.0)
                                {
                                    var arrDecimalSplit = bMAWBOtherCharges.OtherChargeVatPercentage.ToString().Split('.');

                                    if (arrDecimalSplit.Length > 1 && arrDecimalSplit[1].Length >= 3)
                                    {
                                        percentAfterDecimal = Convert.ToInt32(arrDecimalSplit[1].ToList().ElementAt(2).ToString());
                                    }
                                }

                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxPercent, percentAfterDecimal != 0
                                                                                              ? String.Format("{0:0.000}", bMAWBOtherCharges.OtherChargeVatPercentage)
                                                                                              : String.Format("{0:0.00}", bMAWBOtherCharges.OtherChargeVatPercentage));

                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Billed, String.Format("{0:0.000}", bMAWBOtherCharges.OtherChargeVatCalculatedAmount));
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxText, bMAWBOtherCharges.OtherChargeVatText);

                                xmlTextWriter.WriteEndElement();
                            }
                        }
                        break;
                    case XmlConstants.CMAirWayBill:
                        var cMAWBOtherCharges = classObject as CMAWBOtherCharges;
                        if (cMAWBOtherCharges != null)
                        {
                            if (cMAWBOtherCharges.OtherChargeVatCalculatedAmount != null)
                            {
                                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.TaxBreakdown);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxCode, cMAWBOtherCharges.OtherChargeCode);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxLabel, cMAWBOtherCharges.OtherChargeVatLabel);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxIdentifier, XmlConstants.OC);
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxableAmount, String.Format("{0:0.000}", cMAWBOtherCharges.OtherChargeVatBaseAmount));
                                
                                var percentAfterDecimal = 0;
                                if (cMAWBOtherCharges.OtherChargeVatPercentage != 0.0)
                                {
                                    var arrDecimalSplit = cMAWBOtherCharges.OtherChargeVatPercentage.ToString().Split('.');

                                    if (arrDecimalSplit.Length > 1 && arrDecimalSplit[1].Length >= 3)
                                    {
                                        percentAfterDecimal = Convert.ToInt32(arrDecimalSplit[1].ToList().ElementAt(2).ToString());
                                    }
                                }

                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxPercent, percentAfterDecimal != 0
                                                                                              ? String.Format("{0:0.000}", cMAWBOtherCharges.OtherChargeVatPercentage)
                                                                                              : String.Format("{0:0.00}", cMAWBOtherCharges.OtherChargeVatPercentage));

                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Billed, String.Format("{0:0.000}", cMAWBOtherCharges.OtherChargeVatCalculatedAmount));
                                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxText, cMAWBOtherCharges.OtherChargeVatText);

                                xmlTextWriter.WriteEndElement();
                            }
                        }
                        break;
                }
                //Logger.InfoFormat("End of WriteOtherChargeTax for {0}", lineItemDetailType);
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in WriteOtherChargeTax for {0}, Error Message: {1}, Error: {2}", xmlException);
                _staticLogger?.LogError("Error Occurred in WriteOtherChargeTax for {0},  Error Message: {1}", lineItemDetailType, xmlException);
            }
        }

        /// <summary>
        /// To write InvoiceTotalVAT to XML File.
        /// </summary>
        /// <param name="xmlTextWriter"></param>
        /// <param name="invoiceTotalVAT"></param>
        /// <param name="isLineItem"></param>
        private static void WriteInvoiceTotalVAT(XmlTextWriter xmlTextWriter, InvoiceTotalVAT invoiceTotalVAT, bool isLineItem = false)
        {
            try
            {
                //Logger.Info("Start of WriteInvoiceTotalVAT.");

                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.TaxBreakdown);

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxLabel, invoiceTotalVAT.VatLabel);

                if (invoiceTotalVAT.VatIdentifier != null)
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxIdentifier, invoiceTotalVAT.VatIdentifier);
                }

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxableAmount, String.Format("{0:0.000}", invoiceTotalVAT.VatBaseAmount));

                var percentAfterDecimal = 0;
                if (invoiceTotalVAT.VatPercentage != 0.0)
                {
                    var arrDecimalSplit = invoiceTotalVAT.VatPercentage.ToString().Split('.');

                    if (arrDecimalSplit.Length > 1 && arrDecimalSplit[1].Length >= 3)
                    {
                        percentAfterDecimal = Convert.ToInt32(arrDecimalSplit[1].ToList().ElementAt(2).ToString());
                    }
                }

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxPercent,
                                                     percentAfterDecimal != 0 ? String.Format("{0:0.000}", invoiceTotalVAT.VatPercentage)
                                                                              : String.Format("{0:0.00}", invoiceTotalVAT.VatPercentage));

                if (isLineItem)
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxAmount, String.Format("{0:0.000}", invoiceTotalVAT.VatCalculatedAmount));
                }
                else
                {
                    WriteAttributeNodeValue(xmlTextWriter, XmlConstants.TaxAmount, XmlConstants.Billed, String.Format("{0:0.000}", invoiceTotalVAT.VatCalculatedAmount));
                }
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxText, invoiceTotalVAT.VatText);

                xmlTextWriter.WriteEndElement();

                //Logger.Info("End of WriteInvoiceTotalVAT.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in WriteInvoiceTotalVAT, Error Message: {0}, Error: {1}", xmlException);
                _staticLogger?.LogError("Error Occurred in WriteInvoiceTotalVAT,  Error Message: {0}", xmlException);
            }
        }

    }
}