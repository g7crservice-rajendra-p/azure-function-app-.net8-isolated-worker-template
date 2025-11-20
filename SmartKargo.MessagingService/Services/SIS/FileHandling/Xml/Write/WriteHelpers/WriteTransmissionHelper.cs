using Microsoft.Extensions.Logging;
using QidWorkerRole.SIS.Model;
using System.Collections;
using System.Xml;

namespace QidWorkerRole.SIS.FileHandling.Xml.Write.WriteHelpers
{
    public sealed partial class XmlWriterHelper
    {
        /// <summary>
        /// To write the the TransmissionHeader to XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="invoiceList">invoiceList</param>
        public static void WriteTransmissionHeader(XmlTextWriter xmlTextWriter, List<Invoice> invoiceList)
        {
            try
            {
                //Logger.Info("Start of WriteTransmissionHeader.");

                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.InvoiceTransmission);
                xmlTextWriter.WriteAttributeString(XmlConstants.XsiLocalName + XmlConstants.Colon + XmlConstants.SchemaLocationLocalName, XmlConstants.SchemaLocation);
                xmlTextWriter.WriteAttributeString(XmlConstants.XmlnsLocalName, XmlConstants.IataInvoiceStandard);
                xmlTextWriter.WriteAttributeString(XmlConstants.XmlnsLocalName + XmlConstants.Colon + XmlConstants.XsiLocalName, XmlConstants.Xsi);

                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.TransmissionHeader);
                xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.TransmissionDateTime, DateTime.UtcNow.ToString(XmlConstants.TransmissionDateTimeFormat));
                xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.Version, XmlConstants.VersionValue);
                xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.TransmissionID, Guid.NewGuid().ToString());

                if (invoiceList[0].BillingAirline != null)
                {
                    xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.IssuingOrganizationID, invoiceList[0].BillingAirline);
                }

                xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.BillingCategory, "Cargo");

                xmlTextWriter.WriteEndElement();

                //Logger.Info("End of WriteTransmissionHeader.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in WriteTransmissionHeader, Error Message: {0}, Error: {1}", xmlException);
                _staticLogger?.LogError("Error Occurred in WriteTransmissionHeader,  Error Message: {0}", xmlException);
            }
            
        }

        /// <summary>
        /// To Write Total Summary Amounts to XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="invoiceList">invoiceList</param>
        private static void WriteTotalSummaryAmounts(XmlTextWriter xmlTextWriter, IEnumerable<Invoice> invoiceList)
        {
            try
            {
                //Logger.Info("Start of WriteTotalSummaryAmounts.");
    
                var htTotalAmount = new Hashtable();
                var htTotalVatAmount = new Hashtable();
                var htTotalAddOnChargeAmount = new Hashtable();
                var htTotalAmountWithoutVat = new Hashtable();
                var htTotalAmountInClearanceCurrency = new Hashtable();
    
                foreach (var invoice in invoiceList)
                {
                    if (!string.IsNullOrWhiteSpace(invoice.CurrencyofBilling))
                    {
                        var billingCurrency = invoice.CurrencyofBilling;
    
                        if (billingCurrency != null)
                        {
                            if (invoice.InvoiceTotals != null)
                            {
                                if (htTotalAmount.Contains(billingCurrency))
                                {
                                    var totalAmount = Convert.ToDecimal(htTotalAmount[billingCurrency]) + invoice.InvoiceTotals.NetInvoiceBillingTotal;
                                    htTotalAmount.Remove(billingCurrency);
                                    htTotalAmount.Add(billingCurrency, totalAmount);
                                }
                                else
                                {
                                    if (invoice.InvoiceTotals.NetInvoiceBillingTotal != 0)
                                    {
                                        htTotalAmount.Add(billingCurrency, invoice.InvoiceTotals.NetInvoiceBillingTotal);
                                    }
                                    else
                                    {
                                        htTotalAmount.Add(billingCurrency, 0);
                                    }
                                }
    
                                if (htTotalAddOnChargeAmount.Contains(billingCurrency))
                                {
                                    var totalAddOnChargeAmount = Convert.ToDecimal(htTotalAddOnChargeAmount[billingCurrency]) +
                                                                 ((invoice.InvoiceTotals.TotalInterlineServiceChargeAmount +
                                                                 invoice.InvoiceTotals.TotalOtherCharges) / invoice.ListingToBillingRate);
                                    htTotalAddOnChargeAmount.Remove(billingCurrency);
                                    htTotalAddOnChargeAmount.Add(billingCurrency, totalAddOnChargeAmount);
                                }
                                else
                                {
                                    if (invoice.InvoiceTotals.TotalInterlineServiceChargeAmount != 0 || invoice.InvoiceTotals.TotalOtherCharges != 0)
                                    {
                                        htTotalAddOnChargeAmount.Add(billingCurrency, ((invoice.InvoiceTotals.TotalInterlineServiceChargeAmount +
                                                                                        invoice.InvoiceTotals.TotalOtherCharges) / invoice.ListingToBillingRate));
                                    }
                                }
    
                                if (htTotalVatAmount.Contains(billingCurrency))
                                {
                                    var totalVatAmount = Convert.ToDecimal(htTotalVatAmount[billingCurrency]) +
                                                        (invoice.InvoiceTotals.TotalVATAmount / invoice.ListingToBillingRate);
                                    htTotalVatAmount.Remove(billingCurrency);
                                    htTotalVatAmount.Add(billingCurrency, totalVatAmount);
                                }
                                else
                                {
                                    if (invoice.InvoiceTotals.TotalVATAmount != 0)
                                    {
                                        htTotalVatAmount.Add(billingCurrency, (invoice.InvoiceTotals.TotalVATAmount / invoice.ListingToBillingRate));
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(invoice.CurrencyofBilling))
                            {
                                if (invoice.InvoiceTotals != null)
                                {
                                    if (htTotalAmount.Contains(invoice.CurrencyofBilling))
                                    {
                                        var totalAmount = Convert.ToDecimal(htTotalAmount[invoice.CurrencyofBilling]) + invoice.InvoiceTotals.NetInvoiceBillingTotal;
                                        htTotalAmount.Remove(invoice.CurrencyofBilling);
                                        htTotalAmount.Add(invoice.CurrencyofBilling, totalAmount);
                                    }
                                    else
                                    {
                                        if (invoice.InvoiceTotals.NetInvoiceBillingTotal != 0)
                                        {
                                            htTotalAmount.Add(invoice.CurrencyofBilling, invoice.InvoiceTotals.NetInvoiceBillingTotal);
                                        }
                                        else
                                        {
                                            htTotalAmount.Add(invoice.CurrencyofBilling, 0);
                                        }
                                    }
    
                                    if (htTotalAddOnChargeAmount.Contains(invoice.CurrencyofBilling))
                                    {
                                        var totalAddOnChargeAmount = Convert.ToDecimal(htTotalAddOnChargeAmount[invoice.CurrencyofBilling]) +
                                                                    ((invoice.InvoiceTotals.TotalInterlineServiceChargeAmount +
                                                                      invoice.InvoiceTotals.TotalOtherCharges) / invoice.ListingToBillingRate);
                                        htTotalAddOnChargeAmount.Remove(invoice.CurrencyofBilling);
                                        htTotalAddOnChargeAmount.Add(invoice.CurrencyofBilling, totalAddOnChargeAmount);
                                    }
                                    else
                                    {
                                        if (invoice.InvoiceTotals.TotalInterlineServiceChargeAmount != 0 || invoice.InvoiceTotals.TotalOtherCharges != 0)
                                        {
                                            htTotalAddOnChargeAmount.Add(invoice.CurrencyofBilling, ((invoice.InvoiceTotals.TotalInterlineServiceChargeAmount +
                                                                         invoice.InvoiceTotals.TotalOtherCharges) / invoice.ListingToBillingRate));
                                        }
                                    }
    
                                    if (htTotalVatAmount.Contains(invoice.CurrencyofBilling))
                                    {
                                        var totalVatAmount = Convert.ToDecimal(htTotalVatAmount[invoice.CurrencyofBilling]) +
                                                             (invoice.InvoiceTotals.TotalVATAmount / invoice.ListingToBillingRate);
                                        htTotalVatAmount.Remove(invoice.CurrencyofBilling);
                                        htTotalVatAmount.Add(invoice.CurrencyofBilling, totalVatAmount);
                                    }
                                    else
                                    {
                                        if (invoice.InvoiceTotals.TotalVATAmount != 0)
                                        {
                                            htTotalVatAmount.Add(invoice.CurrencyofBilling, (invoice.InvoiceTotals.TotalVATAmount / invoice.ListingToBillingRate));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
    
                foreach (DictionaryEntry item in htTotalAmount)
                {
                    xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.TotalAmount);
                    xmlTextWriter.WriteAttributeString(XmlConstants.CurrencyCode, item.Key.ToString());
                    xmlTextWriter.WriteValue(String.Format("{0:0.000}", item.Value));
                    xmlTextWriter.WriteEndElement();
                }
    
                foreach (DictionaryEntry item in htTotalAddOnChargeAmount)
                {
                    xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.TotalAddOnChargeAmount);
                    xmlTextWriter.WriteAttributeString(XmlConstants.CurrencyCode, item.Key.ToString());
                    xmlTextWriter.WriteValue(String.Format("{0:0.000}", item.Value));
                    xmlTextWriter.WriteEndElement();
                }
    
                foreach (DictionaryEntry item in htTotalVatAmount)
                {
                    xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.TotalVATAmount);
                    xmlTextWriter.WriteAttributeString(XmlConstants.CurrencyCode, item.Key.ToString());
                    xmlTextWriter.WriteValue(String.Format("{0:0.000}", item.Value));
                    xmlTextWriter.WriteEndElement();
                }
                //Logger.Info("End of WriteTotalSummaryAmounts.");
            }
            catch (System.Exception ex)
            {
                _staticLogger?.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}"); 
                throw;
            }
        }

        /// <summary>
        /// To write the TransmissionSummary Nodes To XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="invoiceList">invoiceList</param>
        public static void WriteTransmissionSummary(XmlTextWriter xmlTextWriter, List<Invoice> invoiceList)
        {
            try
            {
                //Logger.Info("Start of WriteTransmissionSummary.");

                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.TransmissionSummary);

                xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.InvoiceCount, invoiceList.Count.ToString());

                WriteTotalSummaryAmounts(xmlTextWriter, invoiceList);

                xmlTextWriter.WriteEndElement();

                //Logger.Info("End of WriteTransmissionSummary.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in WriteTransmissionSummary, Error Message: {0}, Error: {1}",xmlException);
                _staticLogger?.LogError("Error Occurred in WriteTransmissionSummary,  Error Message: {0}", xmlException);
            }

        }
    }
}