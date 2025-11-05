using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using QidWorkerRole.SIS.Model;


namespace QidWorkerRole.SIS.FileHandling.Xml.Write.WriteHelpers
{
    /// <summary>
    /// Class to write Invoice to XML File.
    /// </summary>
    public sealed partial class XmlWriterHelper
    {
        
        #region Invoice node

        /// <summary>
        /// To Write Invoice to the XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="invoice">Invoice</param>
        public static void WriteInvoice(XmlTextWriter xmlTextWriter, Invoice invoice)
        {
            try
            {
                //Logger.InfoFormat("Start of WriteInvoice, Invoice Number: {0}", invoice.InvoiceNumber);

                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Invoice);

                // Invoice Header 
                WriteInvoiceHeader(xmlTextWriter, invoice);

                // Line Item
                WriteLineItem(xmlTextWriter, invoice);

                // Line Item Details
                WriteLineItemDetail(xmlTextWriter, invoice);

                // Invoice Summary
                WriteInvoiceSummaryModel(xmlTextWriter, invoice);

                xmlTextWriter.WriteEndElement();

                //Logger.InfoFormat("End of WriteInvoice, Invoice Number: {0}", invoice.InvoiceNumber);
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        #endregion

        #region InvoiceHeader Node.

        /// <summary>
        /// To Write Invoice Header to the XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="invoice">Invoice</param>
        private static void WriteInvoiceHeader(XmlTextWriter xmlTextWriter, Invoice invoice)
        {
            //Logger.Info("Start of WriteInvoiceHeader.");

            xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.InvoiceHeader);
            WriteSpecifiedElement(xmlTextWriter, XmlConstants.InvoiceNumber, invoice.InvoiceNumber);
            WriteSpecifiedElement(xmlTextWriter, XmlConstants.InvoiceDate, invoice.InvoiceDate.ToString(XmlConstants.BillingDateFormat));

            WriteSpecifiedElement(xmlTextWriter, XmlConstants.InvoiceType, invoice.InvoiceType.ToLower().Equals("iv") ? "Invoice" : "CreditNote");

            WriteSpecifiedElement(xmlTextWriter, XmlConstants.ChargeCategory, "Cargo");

            // Seller-Byer nodes.
            WriteBuyerAndSeller(xmlTextWriter, invoice);

            // PaymentTerms detail.
            WritePaymentTerms(xmlTextWriter, invoice);

            // IS Details 
            WriteIsDetails(xmlTextWriter, invoice);

            WriteSpecifiedElement(xmlTextWriter, XmlConstants.Language, invoice.InvoiceTemplateLanguage);

            xmlTextWriter.WriteEndElement();
        }

        /// <summary>
        /// To Write Buyer and Seller to the XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="invoice">Invoice</param>
        private static void WriteBuyerAndSeller(XmlTextWriter xmlTextWriter, Invoice invoice)
        {
            //Logger.Info("Start of WriteBuyerAndSeller");

            #region SellerOrganization

            xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.SellerOrganization);

            if (invoice.BillingAirline != null)
            {
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.OrganizationID, invoice.BillingAirline);
            }

            foreach (var billingMemberInfo in invoice.ReferenceDataList.Where(billingMemberInfo => billingMemberInfo.IsBillingMember))
            {
                if (!string.IsNullOrWhiteSpace(billingMemberInfo.OrganizationDesignator))
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.OrganizationDesignator, billingMemberInfo.OrganizationDesignator);
                }

                // WriteSpecifiedElement(xmlTextWriter, XmlConstants.LocationID, invoice.BillingAirlineLocationID);

                //WriteReferenceData(xmlTextWriter, billingMemberInfo);
            }

            xmlTextWriter.WriteEndElement();

            #endregion

            #region BuyerOrganization

            xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.BuyerOrganization);

            if (invoice.BilledAirline != null)
            {
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.OrganizationID, invoice.BilledAirline);
            }

            foreach (var billedMemberInfo in invoice.ReferenceDataList.Where(billedMemberInfo => !billedMemberInfo.IsBillingMember))
            {
                if (!string.IsNullOrWhiteSpace(billedMemberInfo.OrganizationDesignator))
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.OrganizationDesignator, billedMemberInfo.OrganizationDesignator);
                }

                // WriteSpecifiedElement(xmlTextWriter, XmlConstants.LocationID, invoice.BilledAirlineLocationID);

                //WriteReferenceData(xmlTextWriter, billedMemberInfo);
            }

            xmlTextWriter.WriteEndElement();

            #endregion

            //Logger.Info("End of WriteBuyerAndSeller");
        }

        /// <summary>
        /// To Write Reference Data to the XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="referenceDataModel">ReferenceDataModel</param>
        private static void WriteReferenceData(XmlTextWriter xmlTextWriter, ReferenceDataModel referenceDataModel)
        {
            //Logger.Info("Start of WriteReferenceData.");

            if (referenceDataModel.CompanyLegalName != null)
            {
                if (referenceDataModel.CompanyLegalName.Contains("!!!"))
                {
                    var strMemberLocationInfo = referenceDataModel.CompanyLegalName.Split(new[] { "!!!" }, StringSplitOptions.None);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.OrganizationName1, strMemberLocationInfo[0]);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.OrganizationName2, strMemberLocationInfo[1]);
                }
                else if (referenceDataModel.CompanyLegalName.Length > 50)
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.OrganizationName1, referenceDataModel.CompanyLegalName.Substring(0, 50));

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.OrganizationName2,
                                          referenceDataModel.CompanyLegalName.Length < 100
                                            ? referenceDataModel.CompanyLegalName.Substring(50)
                                            : referenceDataModel.CompanyLegalName.Substring(50, referenceDataModel.CompanyLegalName.Length - 50));
                }
                else
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.OrganizationName1, referenceDataModel.CompanyLegalName);
                }
            }

            //WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxRegistrationID, referenceDataModel.TaxVATRegistrationID);
            //WriteSpecifiedElement(xmlTextWriter, XmlConstants.AdditionalTaxRegistrationId, referenceDataModel.AdditionalTaxVATRegistrationID);
            //WriteSpecifiedElement(xmlTextWriter, XmlConstants.CompanyRegistrationId, referenceDataModel.CompanyRegistrationID);

            if ((!string.IsNullOrEmpty(referenceDataModel.AddressLine1)))
            {
                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Address);

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.AddressLine1, referenceDataModel.AddressLine1);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.AddressLine2, referenceDataModel.AddressLine2);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.AddressLine3, referenceDataModel.AddressLine3);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.CityName, referenceDataModel.CityName);
                //WriteSpecifiedElement(xmlTextWriter, XmlConstants.SubdivisionCode, referenceDataModel.SubDivisionCode);
                //WriteSpecifiedElement(xmlTextWriter, XmlConstants.SubdivisionName, referenceDataModel.SubDivisionName);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.CountryCode, referenceDataModel.CountryCode);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.CountryName, referenceDataModel.CountryName);
                //WriteSpecifiedElement(xmlTextWriter, XmlConstants.PostalCode, referenceDataModel.PostalCode);

                xmlTextWriter.WriteEndElement();
            }

            //Logger.Info("End of WriteReferenceData.");
        }

        /// <summary>
        /// To Write Payment Terms to the XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="invoice">Invoice</param>
        private static void WritePaymentTerms(XmlTextWriter xmlTextWriter, Invoice invoice)
        {
            try
            {
                //Logger.Info("Start of WritePaymentTerms.");

                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.PaymentTerms);

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.CurrencyCode, invoice.CurrencyofListing);

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.ClearanceCurrencyCode, invoice.CurrencyofBilling);

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.ExchangeRate, String.Format("{0:0.00000}", invoice.ListingToBillingRate));

                if (invoice.BillingYear != 0 && invoice.BillingMonth != 0 && invoice.PeriodNumber != 0)
                {
                    var billingYear = invoice.BillingYear.ToString();
                    if (billingYear.Length > 2)
                    {
                        billingYear = invoice.BillingYear.ToString().Substring(2);
                    }
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.SettlementMonthPeriod, billingYear + invoice.BillingMonth.ToString().PadLeft(2, '0') + invoice.PeriodNumber.ToString().PadLeft(2, '0'));
                }

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.SettlementMethod, invoice.SettlementMethodIndicator);

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.NetDueDate, invoice.ChDueDate);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.ChAgreementIndicator, invoice.ChAgreementIndicator);

                xmlTextWriter.WriteEndElement();

                //Logger.Info("End of WritePaymentTerms.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        /// <summary>
        /// To Write IS Details to the XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="invoice">Invoice</param>
        private static void WriteIsDetails(XmlTextWriter xmlTextWriter, Invoice invoice)
        {
            try
            {
                //Logger.Info("Start of WriteIsDetails.");

                if (invoice.DigitalSignatureFlag != null)
                {
                    xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.ISDetails);

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.DigitalSignatureFlag, invoice.DigitalSignatureFlag);

                    if (invoice.SuspendedInvoiceFlag != null)
                    {
                        if (invoice.SuspendedInvoiceFlag.Equals("Y"))
                        {
                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.SuspendedFlag, XmlConstants.Y);
                        }
                    }
                    xmlTextWriter.WriteEndElement();
                }
                //Logger.Info("End of WriteIsDetails.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        #endregion

        #region LineItem node

        /// <summary>
        /// To write the LineItem to the XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="invoice">Invoice</param>
        private static void WriteLineItem(XmlTextWriter xmlTextWriter, Invoice invoice)
        {
            try
            {
                //Logger.Info("Start of WriteLineItem.");

                var lineItemNumber = 0;
                foreach (var lineItemTotal in invoice.BillingCodeSubTotalList)
                {
                    var billingCodeTotal = lineItemTotal;

                    var detailCount = invoice.AirWayBillList.Count(sourcode => sourcode.BillingCode.Equals(billingCodeTotal.BillingCode));
                    detailCount += invoice.BillingMemoList.Count(sourcode => sourcode.BillingCode.Equals(billingCodeTotal.BillingCode));
                    detailCount += invoice.CreditMemoList.Count(sourcode => sourcode.BillingCode.Equals(billingCodeTotal.BillingCode));
                    detailCount += invoice.RejectionMemoList.Count(sourcode => sourcode.BillingCode.Equals(billingCodeTotal.BillingCode));

                    lineItemNumber++;

                    xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.LineItem);

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.LineItemNumber, lineItemNumber.ToString());

                    var tempChargeCode = string.Empty;
                    //var tempDescription = string.Empty;
                    switch (billingCodeTotal.BillingCode)
                    {
                        case "P":
                            tempChargeCode = "Prepaid AirWaybill";
                            //tempDescription = "PrepaidAirWaybill";
                            break;
                        case "C":
                            tempChargeCode = "Collect AirWaybill";
                            //tempDescription = "CollectAirWaybill";
                            break;
                        case "B":
                            tempChargeCode = "Billing Memo";
                            //tempDescription = "BillingMemo";
                            break;
                        case "T":
                            tempChargeCode = "Credit Memo";
                            //tempDescription = "CreditMemo";
                            break;
                        case "R":
                            tempChargeCode = "Rejection Memo";
                            //tempDescription = "RejectionMemo";
                            break;
                    }

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ChargeCode, tempChargeCode);
                    //WriteSpecifiedElement(xmlTextWriter, XmlConstants.Description, tempDescription);

                    WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.Weight, String.Format("{0:0.000}", billingCodeTotal.TotalWeightCharge));
                    WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.Valuation, String.Format("{0:0.000}", billingCodeTotal.TotalValuationCharge));

                    if (billingCodeTotal.BillingCodeSubTotalVATList.Count > 0)
                    {
                        xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Tax);
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxType, XmlConstants.VAT);
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxAmount, String.Format("{0:0.000}", billingCodeTotal.TotalVatAmount));
                        foreach (var billingCodeTotalVat in billingCodeTotal.BillingCodeSubTotalVATList)
                        {
                            WriteVatDetails(xmlTextWriter, billingCodeTotalVat, true);
                        }

                        xmlTextWriter.WriteEndElement();
                    }

                    WriteAddonCharges(xmlTextWriter, XmlConstants.IscAllowed, Convert.ToDouble(billingCodeTotal.TotalIscAmount));
                    WriteAddonCharges(xmlTextWriter, XmlConstants.OtherChargesAllowed, Convert.ToDouble(billingCodeTotal.TotalOtherCharge));

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.TotalNetAmount, String.Format("{0:0.000}", billingCodeTotal.BillingCodeSbTotal));
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.DetailCount, String.Format("{0:0}", detailCount));

                    xmlTextWriter.WriteEndElement();
                }
                //Logger.Info("End of WriteLineItem.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        #endregion

        #region Line Item Detail node

        /// <summary>
        /// To write LineItemDetail to the XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="invoice">Invoice</param>
        private static void WriteLineItemDetail(XmlTextWriter xmlTextWriter, Invoice invoice)
        {
            try
            {
                //Logger.Info("Start of WriteLineItemDetail.");

                int lineItemNumber = 0;
                int detailNumber;

                foreach (var billingCodeTotal in invoice.BillingCodeSubTotalList)
                {
                    lineItemNumber++;
                    detailNumber = 0;

                    var lineItem = billingCodeTotal;

                    #region Air Way Bill
                     
                    var airWayBillList = (from airWayBills in invoice.AirWayBillList
                                          where airWayBills.BillingCode == lineItem.BillingCode
                                          select airWayBills).ToList().OrderBy(i => i.BatchSequenceNumber).ThenBy(i => i.RecordSequenceWithinBatch);
                    
                    if (airWayBillList.Count() > 0)
                    {
                        foreach (var airWayBill in airWayBillList)
                        {
                            detailNumber++;
                            xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.LineItemDetail);
                            
                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.DetailNumber, detailNumber.ToString());
                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.LineItemNumber, lineItemNumber.ToString());
                            
                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.BatchSequenceNumber, airWayBill.BatchSequenceNumber.ToString());
                            
                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.RecordSequenceWithinBatch, airWayBill.RecordSequenceWithinBatch.ToString());
                            
                            WriteChargeAmount(xmlTextWriter, airWayBill);
                            
                            WriteVat(xmlTextWriter, airWayBill, XmlConstants.AirWayBill);
                            
                            WriteAddonCharges(xmlTextWriter, XmlConstants.IscAllowed, airWayBill.InterlineServiceChargeAmount, airWayBill.InterlineServiceChargePercentage.ToString());
                            
                            if (airWayBill.AWBOtherChargesList.Count == 0)
                            {
                                WriteAddonCharges(xmlTextWriter, XmlConstants.OtherChargesAllowed, airWayBill.OtherCharges);
                            }
                            else
                            {
                                foreach (var AWBOtherCharges in airWayBill.AWBOtherChargesList)
                                {
                                    if (AWBOtherCharges != null)
                                    {
                                        if (AWBOtherCharges.OtherChargeCodeValue != null)
                                        {
                                            WriteOtherChargeAddonCharges(xmlTextWriter, XmlConstants.OtherChargesAllowed,
                                                                         AWBOtherCharges.OtherChargeCode, Convert.ToDecimal(AWBOtherCharges.OtherChargeCodeValue));
                                        }
                                    }
                                }
                            }
                            WriteAddonCharges(xmlTextWriter, XmlConstants.AmountSubjectToISCAllowed, airWayBill.AmountSubjectToInterlineServiceCharge);
                            
                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.TotalNetAmount, String.Format("{0:0.000}", airWayBill.AWBTotalAmount));
                            
                            WriteAirWayBill(xmlTextWriter, airWayBill);
                            
                            xmlTextWriter.WriteEndElement();
                        }
                    }
                    
                    #endregion
                    
                    #region Rejection Memo

                    var rejectionMemoList = (from rejMemo in invoice.RejectionMemoList
                                             where rejMemo.BillingCode == lineItem.BillingCode
                                             select rejMemo).ToList().OrderBy(i => i.BatchSequenceNumber).ThenBy(i => i.RecordSequenceWithinBatch);

                    if (rejectionMemoList.Count() > 0)
                    {
                        foreach (var rejectionMemo in rejectionMemoList)
                        {
                            detailNumber++;
                            xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.LineItemDetail);

                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.DetailNumber, detailNumber.ToString());
                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.LineItemNumber, lineItemNumber.ToString());

                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.BatchSequenceNumber, rejectionMemo.BatchSequenceNumber.ToString());

                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.RecordSequenceWithinBatch, rejectionMemo.RecordSequenceWithinBatch.ToString());

                            WriteChargeAmount(xmlTextWriter, rejectionMemo);
                            WriteVat(xmlTextWriter, rejectionMemo, XmlConstants.RejectionMemo);

                            WriteAddonCharges(xmlTextWriter, XmlConstants.IscAllowed, Convert.ToDouble(rejectionMemo.AllowedTotalIscAmount));
                            WriteAddonCharges(xmlTextWriter, XmlConstants.IscAccepted, Convert.ToDouble(rejectionMemo.AcceptedTotalIscAmount));
                            WriteAddonCharges(xmlTextWriter, XmlConstants.IscDifference, Convert.ToDouble(rejectionMemo.TotalIscAmountDifference));

                            WriteAddonCharges(xmlTextWriter, XmlConstants.OtherChargesAllowed, Convert.ToDouble(rejectionMemo.BilledTotalOtherChargeAmount));
                            WriteAddonCharges(xmlTextWriter, XmlConstants.OtherChargesAccepted, Convert.ToDouble(rejectionMemo.AcceptedTotalOtherChargeAmount));
                            WriteAddonCharges(xmlTextWriter, XmlConstants.OtherChargesDifference, Convert.ToDouble(rejectionMemo.TotalOtherChargeDifference));

                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.TotalNetAmount, String.Format("{0:0.000}", rejectionMemo.TotalNetRejectAmount));

                            WriteRejectionMemo(xmlTextWriter, rejectionMemo);

                            xmlTextWriter.WriteEndElement();
                        }
                    }

                    #endregion

                    #region Billing Memo

                    var billingMemoList = (from billingMemo in invoice.BillingMemoList
                                           where billingMemo.BillingCode == lineItem.BillingCode
                                           select billingMemo).ToList().OrderBy(i => i.BatchSequenceNumber).ThenBy(i => i.RecordSequenceWithinBatch);

                    if (billingMemoList.Count() > 0)
                    {
                        foreach (var billingMemo in billingMemoList)
                        {
                            detailNumber++;
                            xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.LineItemDetail);

                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.DetailNumber, detailNumber.ToString());
                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.LineItemNumber, lineItemNumber.ToString());

                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.BatchSequenceNumber, billingMemo.BatchSequenceNumber.ToString());

                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.RecordSequenceWithinBatch, billingMemo.RecordSequenceWithinBatch.ToString());

                            WriteChargeAmount(xmlTextWriter, billingMemo);

                            WriteVat(xmlTextWriter, billingMemo, XmlConstants.BillingMemo);

                            WriteAddonCharges(xmlTextWriter, XmlConstants.IscAllowed, Convert.ToDouble(billingMemo.BilledTotalIscAmount));
                            WriteAddonCharges(xmlTextWriter, XmlConstants.OtherChargesAllowed, Convert.ToDouble(billingMemo.BilledTotalOtherChargeAmount));

                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.TotalNetAmount, String.Format("{0:0.000}", billingMemo.NetBilledAmount));

                            WriteBillingMemo(xmlTextWriter, billingMemo);

                            xmlTextWriter.WriteEndElement();
                        }
                    }

                    #endregion

                    #region Credit Memo

                    var creditMemoList = (from creditMemo in invoice.CreditMemoList
                                          where creditMemo.BillingCode == lineItem.BillingCode
                                          select creditMemo).ToList().OrderBy(i => i.BatchSequenceNumber).ThenBy(i => i.RecordSequenceWithinBatch);

                    if (creditMemoList.Count() > 0)
                    {
                        foreach (var creditMemo in creditMemoList)
                        {
                            detailNumber++;
                            xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.LineItemDetail);

                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.DetailNumber, detailNumber.ToString());
                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.LineItemNumber, lineItemNumber.ToString());

                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.BatchSequenceNumber, creditMemo.BatchSequenceNumber.ToString());

                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.RecordSequenceWithinBatch, creditMemo.RecordSequenceWithinBatch.ToString());

                            WriteChargeAmount(xmlTextWriter, creditMemo);

                            WriteVat(xmlTextWriter, creditMemo, XmlConstants.CreditMemo);

                            WriteAddonCharges(xmlTextWriter, XmlConstants.IscAllowed, Convert.ToDouble(creditMemo.TotalIscAmountCredited));
                            WriteAddonCharges(xmlTextWriter, XmlConstants.OtherChargesAllowed, Convert.ToDouble(creditMemo.TotalOtherChargeAmt));

                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.TotalNetAmount, String.Format("{0:0.000}", creditMemo.NetAmountCredited));

                            WriteCreditMemoDetails(xmlTextWriter, creditMemo);

                            xmlTextWriter.WriteEndElement();
                        }
                    }

                    #endregion 

                }
                //Logger.Info("End of WriteLineItemDetail.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        /// <summary>
        /// To Write AirWaybill To the XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="airWayBill">AirWayBill</param>
        private static void WriteAirWayBill(XmlTextWriter xmlTextWriter, AirWayBill airWayBill)
        {
            try
            {
                //Logger.Info("Start of WriteAirWayBill.");

                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.AirWaybillDetails);

                if (airWayBill.AWBDate != null)
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.AWBDate, Convert.ToDateTime(airWayBill.AWBDate).ToString("yyyy-MM-dd"));
                }

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.AWBIssuingAirline, airWayBill.AWBIssuingAirline);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.AWBSerialNumber, Convert.ToString(airWayBill.AWBSerialNumber).PadLeft(7, '0'));
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.AWBCheckDigit, airWayBill.AWBCheckDigit.ToString());
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.OriginAirportCode, airWayBill.Origin);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.DestinationAirportCode, airWayBill.Destination);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.FromAirportCode, airWayBill.From);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.ToAirportOrPointOfTransferCode, airWayBill.To);

                if (airWayBill.DateOfCarriageOrTransfer != null)
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.DateOfCarriageOrTransfer,
                             new DateTime(Convert.ToInt32("20" + airWayBill.DateOfCarriageOrTransfer.Substring(0, 2)),
                                          Convert.ToInt32(airWayBill.DateOfCarriageOrTransfer.Substring(2, 2)),
                                          Convert.ToInt32(airWayBill.DateOfCarriageOrTransfer.Substring(4, 2))).ToString("yyyy-MM-dd"));
                }

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.CurrAdjustmentIndicator, airWayBill.CurrencyAdjustmentIndicator);

                if (airWayBill.BilledWeight != null)
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.BilledWeight, airWayBill.BilledWeight.ToString());
                }

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.ProvisoReqSPA, airWayBill.ProvisoReqSPA);

                if (airWayBill.ProratePercentage != null)
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ProratePercentage, airWayBill.ProratePercentage.ToString());
                }

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.PartShipmentIndicator, airWayBill.PartShipmentIndicator);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.FilingReference, airWayBill.FilingReference);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.KgLbIndicator, airWayBill.KGLBIndicator);

                xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.CCAIndicator, (airWayBill.CCAindicator) ? XmlConstants.Y : XmlConstants.N);

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.OurRef, airWayBill.OurReference);

                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Attachment);

                xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.AttachmentIndicatorOriginal, airWayBill.AttachmentIndicatorOriginal);

                //if (!string.IsNullOrWhiteSpace(airWayBill.AttachmentIndicatorValidated) && airWayBill.AttachmentIndicatorValidated.Equals("Y"))
                //{
                //    xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.AttachmentIndicatorValidated, airWayBill.AttachmentIndicatorValidated);
                //}

                //if (airWayBill.NumberOfAttachments.HasValue)
                //{
                //    WriteSpecifiedElement(xmlTextWriter, XmlConstants.NumberOfAttachments, airWayBill.NumberOfAttachments.ToString());
                //}

                xmlTextWriter.WriteEndElement();

                // This field is an output only field and must be blank in case of an input file to SIS.
                // WriteSpecifiedElement(xmlTextWriter, XmlConstants.IsValidationFlag, airWayBill.ISValidationFlag);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReasonCode, airWayBill.ReasonCode.PadLeft(2, '0'));
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField10AN, airWayBill.ReferenceField1);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField10AN, airWayBill.ReferenceField2);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField10AN, airWayBill.ReferenceField3);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField10AN, airWayBill.ReferenceField4);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField20AN, airWayBill.ReferenceField5);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.AirlineOwnUse20AN, airWayBill.AirlineOwnUse);

                xmlTextWriter.WriteEndElement();

                //Logger.Info("End of WriteAirWayBill.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        #region RejectionMemo

        /// <summary>
        /// To Write Rejection Memo to XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="rejectionMemo">RejectionMemo</param>
        private static void WriteRejectionMemo(XmlTextWriter xmlTextWriter, RejectionMemo rejectionMemo)
        {
            try
            {
                //Logger.Info("Start of WriteRejectionMemo.");

                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.RejectionMemoDetails);

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.RejectionMemoNumber, string.IsNullOrEmpty(rejectionMemo.RejectionMemoNumber) ? string.Empty : rejectionMemo.RejectionMemoNumber.Trim());
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.RejectionStage, rejectionMemo.RejectionStage.ToString());
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReasonCode, rejectionMemo.ReasonCode.PadLeft(2, '0'));

                if (rejectionMemo.ReasonRemarks != null)
                {
                    string trimmedDescription = rejectionMemo.ReasonRemarks.Replace(XmlConstants.NewLine, " ").Trim();
                    for (int i = 0; i < trimmedDescription.Length; i += 80)
                    {
                        var dataToWrite = trimmedDescription.Substring(i, Math.Min(80, trimmedDescription.Length - i));

                        if (!string.IsNullOrWhiteSpace(dataToWrite))
                        {
                            WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReasonDescription, dataToWrite);
                        }
                    }
                }

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.OurRef, rejectionMemo.OurRef);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.YourInvoiceNumber, string.IsNullOrEmpty(rejectionMemo.YourInvoiceNumber) ? string.Empty : rejectionMemo.YourInvoiceNumber.Trim());

                if (rejectionMemo.YourInvoiceBillingYear > 0 && rejectionMemo.YourInvoiceBillingMonth > 0 && rejectionMemo.YourInvoiceBillingPeriod > 0)
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.YourInvoiceBillingDate, string.Format("{0}{1}{2}", rejectionMemo.YourInvoiceBillingYear.ToString().Substring(2, 2).PadLeft(2, '0'), rejectionMemo.YourInvoiceBillingMonth.ToString().PadLeft(2, '0'), rejectionMemo.YourInvoiceBillingPeriod.ToString().PadLeft(2, '0')));
                }
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.YourRejectionMemoNumber, string.IsNullOrEmpty(rejectionMemo.YourRejectionNumber) ? string.Empty : rejectionMemo.YourRejectionNumber.Trim());

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.FimBmCmIndicator, rejectionMemo.BMCMIndicator);

                if (!string.IsNullOrEmpty(rejectionMemo.YourBillingMemoNumber))
                {
                    rejectionMemo.YourBillingMemoNumber = rejectionMemo.YourBillingMemoNumber.Trim();
                }

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.LinkedFimBmCmNumber, rejectionMemo.YourBillingMemoNumber);

                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Attachment);

                xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.AttachmentIndicatorOriginal, (rejectionMemo.AttachmentIndicatorOriginal) ? XmlConstants.Y : XmlConstants.N);

                //if (rejectionMemo.AttachmentIndicatorValidated.HasValue)
                //{
                //    xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.AttachmentIndicatorValidated,
                //                                    (rejectionMemo.AttachmentIndicatorValidated.Value) ? XmlConstants.Y : XmlConstants.N);
                //}

                //if (rejectionMemo.NumberOfAttachments.HasValue)
                //{
                //    WriteSpecifiedElement(xmlTextWriter, XmlConstants.NumberOfAttachments, rejectionMemo.NumberOfAttachments.ToString());
                //}

                xmlTextWriter.WriteEndElement();

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.AirlineOwnUse20AN, rejectionMemo.AirlineOwnUse);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.IsValidationFlag, rejectionMemo.ISValidationFlag);

                WriteRMAirWayBill(xmlTextWriter, rejectionMemo.RMAirWayBillList);
                xmlTextWriter.WriteEndElement();

                //Logger.Info("End of WriteRejectionMemo.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        /// <summary>
        /// To Write RMAirWayBill to the XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="rMAirWayBillList">List of RMAirWayBill</param>
        private static void WriteRMAirWayBill(XmlTextWriter xmlTextWriter, IEnumerable<RMAirWayBill> rMAirWayBillList)
        {
            try
            {
                //Logger.Info("Start of WriteRMAirWayBill.");

                foreach (var rMAirWayBill in rMAirWayBillList.OrderBy(i => i.BreakdownSerialNumber))
                {
                    xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.AirWaybillBreakdown);

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.BreakdownSerialNumber, rMAirWayBill.BreakdownSerialNumber.ToString());

                    if (rMAirWayBill.AWBDate != null)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.AWBDate, Convert.ToDateTime(rMAirWayBill.AWBDate).ToString("yyyy-MM-dd"));
                    }

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.BillingCode, rMAirWayBill.BillingCode);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.AWBIssuingAirline, rMAirWayBill.AWBIssuingAirline);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.AWBSerialNumber, Convert.ToString(rMAirWayBill.AWBSerialNumber).PadLeft(7, '0'));
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.AWBCheckDigit, rMAirWayBill.AWBCheckDigit.ToString());

                    WriteChargeAmount(xmlTextWriter, rMAirWayBill);
                    WriteVat(xmlTextWriter, rMAirWayBill, XmlConstants.RMAirWayBill);

                    WriteAddonCharges(xmlTextWriter, XmlConstants.IscAllowed, rMAirWayBill.AllowedIscAmount, rMAirWayBill.AllowedIscPercentage.ToString());
                    WriteAddonCharges(xmlTextWriter, XmlConstants.IscAccepted, rMAirWayBill.AcceptedIscAmount, rMAirWayBill.AcceptedIscPercentage.ToString());
                    WriteAddonCharges(xmlTextWriter, XmlConstants.IscDifference, rMAirWayBill.IscAmountDifference);
                    WriteAddonCharges(xmlTextWriter, XmlConstants.OtherChargesAllowed, rMAirWayBill.BilledOtherCharge);
                    WriteAddonCharges(xmlTextWriter, XmlConstants.OtherChargesAccepted, rMAirWayBill.AcceptedOtherCharge);

                    if (rMAirWayBill.RMAWBOtherChargesList.Count == 0)
                    {
                        WriteAddonCharges(xmlTextWriter, XmlConstants.OtherChargesDifference, rMAirWayBill.OtherChargeDiff);
                    }
                    else
                    {
                        foreach (var otherChargeBrkDwn in rMAirWayBill.RMAWBOtherChargesList)
                        {
                            if (otherChargeBrkDwn != null)
                            {
                                if (otherChargeBrkDwn.OtherChargeCodeValue != null)
                                {
                                    WriteOtherChargeAddonCharges(xmlTextWriter, XmlConstants.OtherChargesDifference, otherChargeBrkDwn.OtherChargeCode, Convert.ToDecimal(otherChargeBrkDwn.OtherChargeCodeValue));
                                }
                            }
                        }
                    }

                    WriteAddonCharges(xmlTextWriter, XmlConstants.AmountSubjectToISCAllowed, rMAirWayBill.AllowedAmtSubToIsc);
                    WriteAddonCharges(xmlTextWriter, XmlConstants.AmountSubjectToISCAccepted, rMAirWayBill.AcceptedAmtSubToIsc);

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.TotalNetAmount, String.Format("{0:0.000}", rMAirWayBill.NetRejectAmount));

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.OriginAirportCode, rMAirWayBill.Origin);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.DestinationAirportCode, rMAirWayBill.Destination);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.FromAirportCode, rMAirWayBill.From);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ToAirportOrPointOfTransferCode, rMAirWayBill.To);

                    if (!string.IsNullOrWhiteSpace(rMAirWayBill.DateOfCarriageOrTransfer))
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.DateOfCarriageOrTransfer,
                                                             new DateTime(Convert.ToInt32("20" + rMAirWayBill.DateOfCarriageOrTransfer.Substring(0, 2)),
                                                                 Convert.ToInt32(rMAirWayBill.DateOfCarriageOrTransfer.Substring(2, 2)),
                                                                 Convert.ToInt32(rMAirWayBill.DateOfCarriageOrTransfer.Substring(4, 2))).ToString("yyyy-MM-dd"));
                    }

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.CurrAdjustmentIndicator, rMAirWayBill.CurrencyAdjustmentIndicator);

                    if (rMAirWayBill.BilledWeight.HasValue)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.BilledWeight, rMAirWayBill.BilledWeight.Value.ToString());
                    }

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ProvisoReqSPA, rMAirWayBill.ProvisionalReqSpa);

                    if (rMAirWayBill.ProratePercentage.HasValue)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.ProratePercentage, rMAirWayBill.ProratePercentage.Value.ToString());
                    }

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.PartShipmentIndicator, rMAirWayBill.PartShipmentIndicator);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.KgLbIndicator, rMAirWayBill.KgLbIndicator);
                    xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.CCAIndicator, (rMAirWayBill.CcaIndicator) ? XmlConstants.Y : XmlConstants.N);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.OurRef, rMAirWayBill.OurReference);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReasonCode, rMAirWayBill.ReasonCode.PadLeft(2, '0'));
                    // This field is an output only field and must be blank in case of an input file to SIS.
                    // WriteSpecifiedElement(xmlTextWriter, XmlConstants.IsValidationFlag, rMAirWayBill.ISValidationFlag);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField10AN, rMAirWayBill.ReferenceField1);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField10AN, rMAirWayBill.ReferenceField2);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField10AN, rMAirWayBill.ReferenceField3);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField10AN, rMAirWayBill.ReferenceField4);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField20AN, rMAirWayBill.ReferenceField5);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.AirlineOwnUse20AN, rMAirWayBill.AirlineOwnUse);

                    WriteRMAWBProrateLadder(xmlTextWriter, rMAirWayBill.RMAWBProrateLadderList, rMAirWayBill.ProrateCalCurrencyId, rMAirWayBill.TotalProrateAmount);

                    xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Attachment);

                    xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.AttachmentIndicatorOriginal, rMAirWayBill.AttachmentIndicatorOriginal);

                    //if (!string.IsNullOrWhiteSpace(rMAirWayBill.AttachmentIndicatorValidated) && rMAirWayBill.AttachmentIndicatorValidated.Equals("Y"))
                    //{
                    //    xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.AttachmentIndicatorValidated, rMAirWayBill.AttachmentIndicatorValidated);
                    //}

                    //if (rMAirWayBill.NumberOfAttachments.HasValue)
                    //{
                    //    WriteSpecifiedElement(xmlTextWriter, XmlConstants.NumberOfAttachments, rMAirWayBill.NumberOfAttachments.ToString());
                    //}

                    xmlTextWriter.WriteEndElement();
                    xmlTextWriter.WriteEndElement();
                }
                //Logger.Info("End of WriteRMAirWayBill.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        /// <summary>
        /// To Write RMAWBProrateLadder to XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="rMAWBProrateLadderList">List of RMAWBProrateLadder</param>
        /// <param name="prorateCalCurrencyId">prorateCalCurrencyId</param>
        /// <param name="prorateTotalAmount">prorateTotalAmount</param>
        private static void WriteRMAWBProrateLadder(XmlTextWriter xmlTextWriter, IEnumerable<RMAWBProrateLadder> rMAWBProrateLadderList, string prorateCalCurrencyId, double? prorateTotalAmount)
        {
            try
            {
                //Logger.Info("Start of WriteRMAWBProrateLadder.");

                foreach (var rMAWBProrateLadder in rMAWBProrateLadderList.OrderBy(i => i.RMAWBProrateLadderID))
                {
                    xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.ProrateLadderBreakdown);

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.CurrencyOfProrateCalculation, prorateCalCurrencyId);

                    if (prorateTotalAmount != null)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.TotalAmount, String.Format("{0:0.000}", prorateTotalAmount));
                    }

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.FromSector, rMAWBProrateLadder.FromSector);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ToSector, rMAWBProrateLadder.ToSector);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.CarrierPrefix, rMAWBProrateLadder.CarrierPrefix);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ProvisoReqSPAFlag, rMAWBProrateLadder.ProvisoReqSpa);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ProrateFactor, rMAWBProrateLadder.ProrateFactor.ToString());
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.PercentShare, String.Format("{0:0.00}", rMAWBProrateLadder.PercentShare));
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.Amount, String.Format("{0:0.000}", rMAWBProrateLadder.TotalAmount));

                    xmlTextWriter.WriteEndElement();
                }
                //Logger.Info("End of WriteRMAWBProrateLadder.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        #endregion

        #region BillingMemo

        /// <summary>
        /// To Write Billing Memo to XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="rejectionMemo">BillingMemo</param>
        private static void WriteBillingMemo(XmlTextWriter xmlTextWriter, BillingMemo billingMemo)
        {
            try
            {
                //Logger.Info("Start of WriteBillingMemo.");

                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.BillingMemoDetails);

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.BillingMemoNumber, billingMemo.BillingMemoNumber);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReasonCode, billingMemo.ReasonCode.PadLeft(2, '0'));

                if (billingMemo.ReasonRemarks != null)
                {
                    string trimmedDescription = billingMemo.ReasonRemarks.Replace(XmlConstants.NewLine, " ").Trim();
                    for (int i = 0; i < trimmedDescription.Length; i += 80)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReasonDescription, trimmedDescription.Substring(i, Math.Min(80, trimmedDescription.Length - i)));
                    }
                }


                WriteSpecifiedElement(xmlTextWriter, XmlConstants.CorrespondenceRefNumber, billingMemo.CorrespondenceReferenceNumber);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.OurRef, billingMemo.OurRef);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.YourInvoiceNumber, string.IsNullOrEmpty(billingMemo.YourInvoiceNumber) ? string.Empty : billingMemo.YourInvoiceNumber.Trim());

                if (billingMemo.YourInvoiceBillingYear > 0 && billingMemo.YourInvoiceBillingMonth > 0 && billingMemo.YourInvoiceBillingPeriod > 0)
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.YourInvoiceBillingDate, string.Format("{0}{1}{2}", billingMemo.YourInvoiceBillingYear.ToString().Substring(2, 2).PadLeft(2, '0'), billingMemo.YourInvoiceBillingMonth.ToString().PadLeft(2, '0'), billingMemo.YourInvoiceBillingPeriod.ToString().PadLeft(2, '0')));
                }

                // Attachment
                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Attachment);

                xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.AttachmentIndicatorOriginal, (billingMemo.AttachmentIndicatorOriginal) ? XmlConstants.Y : XmlConstants.N);
                //if (billingMemo.AttachmentIndicatorValidated.HasValue)
                //{
                //    xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.AttachmentIndicatorValidated, (billingMemo.AttachmentIndicatorValidated.Value) ? XmlConstants.Y : XmlConstants.N);
                //}
                //if (billingMemo.NumberOfAttachments.HasValue)
                //{
                //    WriteSpecifiedElement(xmlTextWriter, XmlConstants.NumberOfAttachments, billingMemo.NumberOfAttachments.ToString());
                //}

                xmlTextWriter.WriteEndElement();

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.AirlineOwnUse20AN, billingMemo.AirlineOwnUse);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.IsValidationFlag, billingMemo.ISValidationFlag);

                WriteBMAirWayBill(xmlTextWriter, billingMemo.BMAirWayBillList);

                xmlTextWriter.WriteEndElement();

                //Logger.Info("End of WriteBillingMemo.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        /// <summary>
        /// To Write BMAirWayBill to the XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="bMAirWayBillList">List of bMAirWayBill</param>
        private static void WriteBMAirWayBill(XmlTextWriter xmlTextWriter, IEnumerable<BMAirWayBill> bMAirWayBillList)
        {
            try
            {
                //Logger.Info("Start of WriteBMAirWayBill.");

                foreach (var bMAirWayBill in bMAirWayBillList.OrderBy(i => i.BreakdownSerialNumber))
                {
                    xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.AirWaybillBreakdown);

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.BreakdownSerialNumber, bMAirWayBill.BreakdownSerialNumber.ToString());

                    if (bMAirWayBill.AWBDate != null)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.AWBDate, Convert.ToDateTime(bMAirWayBill.AWBDate).ToString("yyyy-MM-dd"));
                    }

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.BillingCode, bMAirWayBill.BillingCode);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.AWBIssuingAirline, bMAirWayBill.AWBIssuingAirline);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.AWBSerialNumber, Convert.ToString(bMAirWayBill.AWBSerialNumber).PadLeft(7, '0'));
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.AWBCheckDigit, bMAirWayBill.AWBCheckDigit.ToString());

                    WriteChargeAmount(xmlTextWriter, bMAirWayBill);

                    WriteVat(xmlTextWriter, bMAirWayBill, XmlConstants.BMAirWayBill);

                    WriteAddonCharges(xmlTextWriter, XmlConstants.IscAllowed, bMAirWayBill.BilledIscAmount, bMAirWayBill.BilledIscPercentage.ToString());

                    if (bMAirWayBill.BMAWBOtherChargesList.Count == 0)
                    {
                        WriteAddonCharges(xmlTextWriter, XmlConstants.OtherChargesAllowed, bMAirWayBill.BilledOtherCharge);
                    }
                    else
                    {
                        foreach (var otherChargeBrkDwn in bMAirWayBill.BMAWBOtherChargesList)
                        {
                            if (otherChargeBrkDwn != null)
                            {
                                if (otherChargeBrkDwn.OtherChargeCodeValue != null)
                                {
                                    WriteOtherChargeAddonCharges(xmlTextWriter, XmlConstants.OtherChargesAllowed, otherChargeBrkDwn.OtherChargeCode, Convert.ToDecimal(otherChargeBrkDwn.OtherChargeCodeValue));
                                }
                            }
                        }
                    }

                    WriteAddonCharges(xmlTextWriter, XmlConstants.AmountSubjectToISCAllowed, bMAirWayBill.BilledAmtSubToIsc);

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.TotalNetAmount, String.Format("{0:0.000}", bMAirWayBill.TotalAmount));
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.OriginAirportCode, bMAirWayBill.Origin);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.DestinationAirportCode, bMAirWayBill.Destination);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.FromAirportCode, bMAirWayBill.From);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ToAirportOrPointOfTransferCode, bMAirWayBill.To);

                    if (bMAirWayBill.DateOfCarriageOrTransfer != null)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.DateOfCarriageOrTransfer,
                                              new DateTime(Convert.ToInt32("20" + bMAirWayBill.DateOfCarriageOrTransfer.Substring(0, 2)),
                                                           Convert.ToInt32(bMAirWayBill.DateOfCarriageOrTransfer.Substring(2, 2)),
                                                           Convert.ToInt32(bMAirWayBill.DateOfCarriageOrTransfer.Substring(4, 2))).ToString("yyyy-MM-dd"));
                    }

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.CurrAdjustmentIndicator, bMAirWayBill.CurrencyAdjustmentIndicator);

                    if (bMAirWayBill.BilledWeight != null)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.BilledWeight, bMAirWayBill.BilledWeight.ToString());
                    }

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ProvisoReqSPA, bMAirWayBill.ProvisionalReqSpa);

                    if (bMAirWayBill.PrpratePercentage != null)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.ProratePercentage, bMAirWayBill.PrpratePercentage.ToString());
                    }

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.PartShipmentIndicator, bMAirWayBill.PartShipmentIndicator);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.KgLbIndicator, bMAirWayBill.KgLbIndicator);
                    xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.CCAIndicator, (bMAirWayBill.CcaIndicator) ? XmlConstants.Y : XmlConstants.N);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReasonCode, bMAirWayBill.ReasonCode.PadLeft(2, '0'));
                    // This field is an output only field and must be blank in case of an input file to SIS.
                    // WriteSpecifiedElement(xmlTextWriter, XmlConstants.IsValidationFlag, bMAirWayBill.ISValidationFlag);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField10AN, bMAirWayBill.ReferenceField1);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField10AN, bMAirWayBill.ReferenceField2);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField10AN, bMAirWayBill.ReferenceField3);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField10AN, bMAirWayBill.ReferenceField4);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField20AN, bMAirWayBill.ReferenceField5);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.AirlineOwnUse20AN, bMAirWayBill.AirlineOwnUse);

                    WriteBMAWBProrateLadder(xmlTextWriter, bMAirWayBill.BMAWBProrateLadderList, bMAirWayBill.ProrateCalCurrencyId, bMAirWayBill.TotalProrateAmount);

                    xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Attachment);
                    xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.AttachmentIndicatorOriginal, bMAirWayBill.AttachmentIndicatorOriginal);

                    //if (!string.IsNullOrWhiteSpace(bMAirWayBill.AttachmentIndicatorValidated) && bMAirWayBill.AttachmentIndicatorValidated.Equals("Y"))
                    //{
                    //    xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.AttachmentIndicatorValidated, bMAirWayBill.AttachmentIndicatorValidated);
                    //}

                    //if (bMAirWayBill.NumberOfAttachments.HasValue)
                    //{
                    //    WriteSpecifiedElement(xmlTextWriter, XmlConstants.NumberOfAttachments, bMAirWayBill.NumberOfAttachments.ToString());
                    //}

                    xmlTextWriter.WriteEndElement();

                    xmlTextWriter.WriteEndElement();
                }
                //Logger.Info("End of WriteBMAirWayBill.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        /// <summary>
        /// To Write BMAWBProrateLadder to XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="rMAWBProrateLadderList">List of bMAWBProrateLadder</param>
        /// <param name="prorateCalCurrencyId">prorateCalCurrencyId</param>
        /// <param name="prorateTotalAmount">prorateTotalAmount</param>
        private static void WriteBMAWBProrateLadder(XmlTextWriter xmlTextWriter, IEnumerable<BMAWBProrateLadder> bMAWBProrateLadderList, string prorateCalCurrencyId, double? prorateTotalAmount)
        {
            try
            {
                //Logger.Info("Start of WriteBMAWBProrateLadder.");

                foreach (var bMAWBProrateLadder in bMAWBProrateLadderList.OrderBy(i => i.BMAWBProrateLadderID))
                {
                    xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.ProrateLadderBreakdown);

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.CurrencyOfProrateCalculation, prorateCalCurrencyId);

                    if (prorateTotalAmount != null)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.TotalAmount, String.Format("{0:0.000}", prorateTotalAmount));
                    }

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.FromSector, bMAWBProrateLadder.FromSector);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ToSector, bMAWBProrateLadder.ToSector);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.CarrierPrefix, bMAWBProrateLadder.CarrierPrefix);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ProvisoReqSPAFlag, bMAWBProrateLadder.ProvisoReqSpa);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ProrateFactor, bMAWBProrateLadder.ProrateFactor.ToString());
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.PercentShare, String.Format("{0:0.00}", bMAWBProrateLadder.PercentShare));
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.Amount, String.Format("{0:0.000}", bMAWBProrateLadder.TotalAmount));

                    xmlTextWriter.WriteEndElement();
                }
                //Logger.Info("End of WriteBMAWBProrateLadder.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        #endregion

        #region Credit Memo

        /// <summary>
        /// To Write Credit Memo to XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="rejectionMemo">CreditMemo</param>
        private static void WriteCreditMemoDetails(XmlTextWriter xmlTextWriter, CreditMemo creditMemo)
        {
            try
            {
                //Logger.Info("Start of WriteCreditMemoDetails.");

                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.CreditMemoDetails);

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.CreditMemoNumber, creditMemo.CreditMemoNumber);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReasonCode, creditMemo.ReasonCode.PadLeft(2, '0'));

                if (creditMemo.ReasonRemarks != null)
                {
                    string trimmedDescription = creditMemo.ReasonRemarks.Replace(XmlConstants.NewLine, " ").Trim();
                    for (int i = 0; i < trimmedDescription.Length; i += 80)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReasonDescription, trimmedDescription.Substring(i, Math.Min(80, trimmedDescription.Length - i)));
                    }
                }

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.CorrespondenceRefNumber, creditMemo.CorrespondenceRefNumber);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.OurRef, creditMemo.OurRef);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.YourInvoiceNumber, string.IsNullOrEmpty(creditMemo.YourInvoiceNumber) ? string.Empty : creditMemo.YourInvoiceNumber.Trim());

                if (creditMemo.YourInvoiceBillingYear > 0 && creditMemo.YourInvoiceBillingMonth > 0 && creditMemo.YourInvoiceBillingPeriod > 0)
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.YourInvoiceBillingDate, string.Format("{0}{1}{2}", creditMemo.YourInvoiceBillingYear.ToString().Substring(2, 2).PadLeft(2, '0'), creditMemo.YourInvoiceBillingMonth.ToString().PadLeft(2, '0'), creditMemo.YourInvoiceBillingPeriod.ToString().PadLeft(2, '0')));
                }

                xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Attachment);

                xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.AttachmentIndicatorOriginal, (creditMemo.AttachmentIndicatorOriginal) ? XmlConstants.Y : XmlConstants.N);
                //if (creditMemo.AttachmentIndicatorValidated.HasValue)
                //{
                //    xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.AttachmentIndicatorValidated,
                //                                    (creditMemo.AttachmentIndicatorValidated.Value) ? XmlConstants.Y : XmlConstants.N);
                //}
                //if (creditMemo.NumberOfAttachments.HasValue)
                //{
                //    WriteSpecifiedElement(xmlTextWriter, XmlConstants.NumberOfAttachments, creditMemo.NumberOfAttachments.ToString());
                //}

                xmlTextWriter.WriteEndElement();

                WriteSpecifiedElement(xmlTextWriter, XmlConstants.AirlineOwnUse20AN, creditMemo.AirlineOwnUse);
                WriteSpecifiedElement(xmlTextWriter, XmlConstants.IsValidationFlag, creditMemo.ISValidationFlag);

                WriteCMAirWayBill(xmlTextWriter, creditMemo.CMAirWayBillList);

                xmlTextWriter.WriteEndElement();

                //Logger.Info("End of WriteCreditMemoDetails.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        /// <summary>
        /// To Write CMAirWayBill to the XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="bMAirWayBillList">List of CMAirWayBill</param>
        private static void WriteCMAirWayBill(XmlTextWriter xmlTextWriter, IEnumerable<CMAirWayBill> cMAirWayBillList)
        {
            try
            {
                //Logger.Info("Start of WriteCMAirWayBill.");

                foreach (var cMAirWayBill in cMAirWayBillList.OrderBy(i => i.BreakdownSerialNumber))
                {

                    xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.AirWaybillBreakdown);

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.BreakdownSerialNumber, cMAirWayBill.BreakdownSerialNumber.ToString());

                    if (cMAirWayBill.AWBDate != null)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.AWBDate, Convert.ToDateTime(cMAirWayBill.AWBDate).ToString("yyyy-MM-dd"));
                    }

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.BillingCode, cMAirWayBill.BillingCode);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.AWBIssuingAirline, cMAirWayBill.AWBIssuingAirline);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.AWBSerialNumber, Convert.ToString(cMAirWayBill.AWBSerialNumber).PadLeft(7, '0'));
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.AWBCheckDigit, cMAirWayBill.AWBCheckDigit.ToString());

                    WriteChargeAmount(xmlTextWriter, cMAirWayBill);

                    WriteVat(xmlTextWriter, cMAirWayBill, XmlConstants.CMAirWayBill);

                    WriteAddonCharges(xmlTextWriter, XmlConstants.IscAllowed, cMAirWayBill.CreditedIscAmount, cMAirWayBill.CreditedIscPercentage.ToString());

                    if (cMAirWayBill.CMAWBOtherChargesList.Count == 0)
                    {
                        WriteAddonCharges(xmlTextWriter, XmlConstants.OtherChargesAllowed, cMAirWayBill.CreditedOtherCharge);
                    }
                    else
                    {
                        foreach (var otherChargeBrkDwn in cMAirWayBill.CMAWBOtherChargesList)
                        {
                            if (otherChargeBrkDwn != null)
                            {
                                if (otherChargeBrkDwn.OtherChargeCodeValue != null)
                                {
                                    WriteOtherChargeAddonCharges(xmlTextWriter, XmlConstants.OtherChargesAllowed, otherChargeBrkDwn.OtherChargeCode, Convert.ToDecimal(otherChargeBrkDwn.OtherChargeCodeValue));
                                }
                            }
                        }
                    }

                    WriteAddonCharges(xmlTextWriter, XmlConstants.AmountSubjectToISCAllowed, cMAirWayBill.CreditedAmtSubToIsc);

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.TotalNetAmount, String.Format("{0:0.000}", cMAirWayBill.TotalAmountCredited));
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.OriginAirportCode, cMAirWayBill.Origin);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.DestinationAirportCode, cMAirWayBill.Destination);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.FromAirportCode, cMAirWayBill.From);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ToAirportOrPointOfTransferCode, cMAirWayBill.To);

                    if (cMAirWayBill.DateOfCarriageOrTransfer != null)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.DateOfCarriageOrTransfer, 
                                              new DateTime(Convert.ToInt32("20" + cMAirWayBill.DateOfCarriageOrTransfer.Substring(0, 2)),
                                                           Convert.ToInt32(cMAirWayBill.DateOfCarriageOrTransfer.Substring(2, 2)),
                                                           Convert.ToInt32(cMAirWayBill.DateOfCarriageOrTransfer.Substring(4, 2))).ToString("yyyy-MM-dd"));
                    }

                    if (cMAirWayBill.CurrencyAdjustmentIndicator != null)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.CurrAdjustmentIndicator, cMAirWayBill.CurrencyAdjustmentIndicator);
                    }

                    if (cMAirWayBill.BilledWeight != null)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.BilledWeight, cMAirWayBill.BilledWeight.ToString());
                    }

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ProvisoReqSPA, cMAirWayBill.ProvisionalReqSpa);

                    if (cMAirWayBill.ProratePercentage != null)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.ProratePercentage, cMAirWayBill.ProratePercentage.ToString());
                    }

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.PartShipmentIndicator, cMAirWayBill.PartShipmentIndicator);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.KgLbIndicator, cMAirWayBill.KgLbIndicator);
                    xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.CCAIndicator, (cMAirWayBill.CcaIndicator) ? XmlConstants.Y : XmlConstants.N);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReasonCode, cMAirWayBill.ReasonCode.PadLeft(2, '0'));
                    // This field is an output only field and must be blank in case of an input file to SIS.
                    // WriteSpecifiedElement(xmlTextWriter, XmlConstants.IsValidationFlag, cMAirWayBill.ISValidationFlag);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField10AN, cMAirWayBill.ReferenceField1);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField10AN, cMAirWayBill.ReferenceField2);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField10AN, cMAirWayBill.ReferenceField3);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField10AN, cMAirWayBill.ReferenceField4);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ReferenceField20AN, cMAirWayBill.ReferenceField5);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.AirlineOwnUse20AN, cMAirWayBill.AirlineOwnUse);

                    WriteCMAWBProrateLadder(xmlTextWriter, cMAirWayBill.CMAWBProrateLadderList, cMAirWayBill.ProrateCalCurrencyId, cMAirWayBill.TotalProrateAmount);

                    xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Attachment);
                    xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.AttachmentIndicatorOriginal, cMAirWayBill.AttachmentIndicatorOriginal);

                    //if (!string.IsNullOrWhiteSpace(cMAirWayBill.AttachmentIndicatorValidated) && cMAirWayBill.AttachmentIndicatorValidated.Equals("Y"))
                    //{
                    //    xmlTextWriter.WriteElementString(XmlConstants.Prefix + XmlConstants.AttachmentIndicatorValidated, cMAirWayBill.AttachmentIndicatorValidated);
                    //}

                    //if (cMAirWayBill.NumberOfAttachments.HasValue)
                    //{
                    //    WriteSpecifiedElement(xmlTextWriter, XmlConstants.NumberOfAttachments, cMAirWayBill.NumberOfAttachments.ToString());
                    //}

                    xmlTextWriter.WriteEndElement();
                    xmlTextWriter.WriteEndElement();
                }
                //Logger.Info("End of WriteCMAirWayBill.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        /// <summary>
        /// To Write CMAWBProrateLadder to XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="rMAWBProrateLadderList">List of CMAWBProrateLadder</param>
        /// <param name="prorateCalCurrencyId">prorateCalCurrencyId</param>
        /// <param name="prorateTotalAmount">prorateTotalAmount</param>
        private static void WriteCMAWBProrateLadder(XmlTextWriter xmlTextWriter, IEnumerable<CMAWBProrateLadder> cMAWBProrateLadderList, string prorateCalCurrencyId, double? prorateTotalAmount)
        {
            try
            {
                //Logger.Info("Start of WriteCMAWBProrateLadder.");

                foreach (var cMAWBProrateLadder in cMAWBProrateLadderList.OrderBy(i => i.CMAWBProrateLadderID))
                {
                    xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.ProrateLadderBreakdown);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.CurrencyOfProrateCalculation, prorateCalCurrencyId);

                    if (prorateTotalAmount != null)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.TotalAmount, String.Format("{0:0.000}", prorateTotalAmount));
                    }

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.FromSector, cMAWBProrateLadder.FromSector);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ToSector, cMAWBProrateLadder.ToSector);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.CarrierPrefix, cMAWBProrateLadder.CarrierPrefix);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ProvisoReqSPAFlag, cMAWBProrateLadder.ProvisoReqSpa);
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.ProrateFactor, cMAWBProrateLadder.ProrateFactor.ToString());
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.PercentShare, String.Format("{0:0.00}", cMAWBProrateLadder.PercentShare));
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.Amount, String.Format("{0:0.000}", cMAWBProrateLadder.TotalAmount));

                    xmlTextWriter.WriteEndElement();
                }
                //Logger.Info("End of WriteCMAWBProrateLadder.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        #endregion

        #endregion

        #region Invoice Summary Node

        /// <summary>
        /// To write the InvoiceTotal int InvoiceSummary node of XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="invoice">Invoice</param>
        private static void WriteInvoiceSummaryModel(XmlTextWriter xmlTextWriter, Invoice invoice)
        {
            try
            {
                //Logger.Info("Start of WriteInvoiceSummaryModel.");

                if (invoice.InvoiceTotals != null)
                {
                    xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.InvoiceSummary);

                    if (invoice.BillingCodeSubTotalList != null)
                    {
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.LineItemCount, invoice.BillingCodeSubTotalList.Count().ToString());
                    }

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.TotalLineItemAmount, String.Format("{0:0.000}", invoice.InvoiceTotals.TotalWeightCharges + invoice.InvoiceTotals.TotalValuationCharges));

                    if (invoice.InvoiceTotalVATList.Count > 0)
                    {
                        xmlTextWriter.WriteStartElement(XmlConstants.Prefix + XmlConstants.Tax);

                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxType, XmlConstants.VAT);

                        double? totalVatBreakDownAmount = invoice.InvoiceTotalVATList.Sum(amount => amount.VatCalculatedAmount);
                        WriteSpecifiedElement(xmlTextWriter, XmlConstants.TaxAmount, String.Format("{0:0.000}", totalVatBreakDownAmount));

                        foreach (var invoiceTotalVat in invoice.InvoiceTotalVATList)
                        {
                            WriteInvoiceTotalVAT(xmlTextWriter, invoiceTotalVat, true);
                        }

                        xmlTextWriter.WriteEndElement();
                    }

                    WriteAddonCharges(xmlTextWriter, XmlConstants.IscAllowed, Convert.ToDouble(invoice.InvoiceTotals.TotalInterlineServiceChargeAmount));
                    WriteAddonCharges(xmlTextWriter, XmlConstants.OtherChargesAllowed, Convert.ToDouble(invoice.InvoiceTotals.TotalOtherCharges));

                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.TotalVATAmount, String.Format("{0:0.000}", invoice.InvoiceTotals.TotalVATAmount));
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.TotalAmountWithoutVAT, String.Format("{0:0.000}", invoice.InvoiceTotals.TotalNetAmountWithoutVat));
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.TotalAmount, String.Format("{0:0.000}", invoice.InvoiceTotals.NetInvoiceTotal));
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.TotalAmountInClearanceCurrency, String.Format("{0:0.000}", invoice.InvoiceTotals.NetInvoiceBillingTotal));

                    WriteLegalText(xmlTextWriter, invoice.InvoiceFooterDetails);

                    xmlTextWriter.WriteEndElement();
                }

                //Logger.Info("End of WriteInvoiceSummaryModel.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        /// <summary>
        /// To Write Legal Text to XML File.
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="legalText"></param>
        private static void WriteLegalText(XmlTextWriter xmlTextWriter, string legalText)
        {
            try
            {
                //Logger.Info("Start of WriteLegalText.");

                if (string.IsNullOrEmpty(legalText))
                {
                    return;
                }

                string trimmedlegalText = legalText.Trim();
                int legalTextLength = trimmedlegalText.Length;
                int startPoint = 0;

                while (legalTextLength > 0)
                {
                    WriteSpecifiedElement(xmlTextWriter, XmlConstants.LegalText, trimmedlegalText.Substring(startPoint, (trimmedlegalText.Length >= startPoint + 70) ? 70 : trimmedlegalText.Length - startPoint));
                    legalTextLength -= 70;
                    startPoint += 70;
                }

                //Logger.Info("End of WriteLegalText.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure(xmlException);
            }
        }

        #endregion

        # region Common Methods

        /// <summary>
        /// To write the specified xml element.
        /// </summary>
        /// <param name="xmlTextWriter"></param>
        /// <param name="elementName"></param>
        /// <param name="elementValue"></param>
        /// <param name="checkForEmpty"></param>
        private static void WriteSpecifiedElement(XmlTextWriter xmlTextWriter, string elementName, string elementValue, bool checkForEmpty = false)
        {
            if (string.IsNullOrWhiteSpace(elementValue))
            {
                return;
            }
            if (checkForEmpty)
            {
                if ((elementValue != "0") && (elementValue != "0.000") && (elementValue != "0001-01-01"))
                {
                    elementName = XmlConstants.Prefix + elementName;
                    xmlTextWriter.WriteElementString(elementName, elementValue);
                }
            }
            else
            {
                elementName = XmlConstants.Prefix + elementName;
                xmlTextWriter.WriteElementString(elementName, elementValue);
            }
        }

        /// <summary>
        /// Write Node with attribute
        /// </summary>
        /// <param name="xmlTextWriter"></param>
        /// <param name="startElement"></param>
        /// <param name="attributeName"></param>
        /// <param name="value"></param>
        private static void WriteAttributeNodeValue(XmlTextWriter xmlTextWriter, string startElement, string attributeName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }
            xmlTextWriter.WriteStartElement(XmlConstants.Prefix + startElement);
            xmlTextWriter.WriteAttributeString(XmlConstants.Name, attributeName);
            xmlTextWriter.WriteValue(value);
            xmlTextWriter.WriteEndElement();
        }

        #endregion

    }
}