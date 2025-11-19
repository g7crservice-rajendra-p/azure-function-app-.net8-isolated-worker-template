using System;
using System.Linq;

using System.Reflection;
using System.Xml;
using System.Collections;
using System.Globalization;
using QidWorkerRole.SIS.Model;
using QidWorkerRole.SIS.FileHandling.Xml.Read.SupportingModels;
using Microsoft.Extensions.Logging;

namespace QidWorkerRole.SIS.FileHandling.Xml.Read.ReadHelpers
{
    /// <summary>
    /// To parse Is-Xml elements and generate a model for it.
    /// </summary>
    public sealed partial class XmlReaderHelper
    {
        
        //private static Dictionary<int, Dictionary<int, int>> _fileRecordSequenceNumber = new Dictionary<int, Dictionary<int, int>>();

        /// <summary>
        /// To Read Invoice details from XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="invoice">Invoice</param>
        /// <param name="fileRecordSequenceNumber">fileRecordSequenceNumber</param>
        
        public static void ReadInvoice(XmlTextReader xmlTextReader, Invoice invoice)//, out Dictionary<int, Dictionary<int, int>> fileRecordSequenceNumber)
        {
            //_fileRecordSequenceNumber.Clear();
            try
            {
                //Logger.Info("Start of ReadInvoice.");

                // const int cargoDecimalPlaces = 2;
                var htChargeCode = new Hashtable();
                var lineItemDetailCount = 0;

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.Invoice)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            // Invoice Header Details
                            case XmlConstants.InvoiceNumber:
                                xmlTextReader.Read();
                                invoice.InvoiceNumber = xmlTextReader.Value;
                                break;
                            case XmlConstants.InvoiceDate:
                                xmlTextReader.Read();
                                DateTime date;
                                if (DateTime.TryParseExact(xmlTextReader.Value, XmlConstants.BillingDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                                {
                                    invoice.InvoiceDate = date;
                                }
                                break;
                            case XmlConstants.InvoiceType:
                                xmlTextReader.Read();
                                invoice.InvoiceType = xmlTextReader.Value.Trim().ToLower().Equals("invoice") ? "IV" : "CN";
                                break;
                            case XmlConstants.ChargeCategory:
                                xmlTextReader.Read();
                                // invoice.BillingCategory = EnumUtils.ParseBillingCategory(reader.Value.Trim());                                
                                break;
                            // Reference Data Details
                            // Buyer - Seller Organization details elements
                            case XmlConstants.SellerOrganization:
                                ReadSellerAndBuyerOrganization(xmlTextReader, true, invoice);
                                break;
                            case XmlConstants.BuyerOrganization:
                                ReadSellerAndBuyerOrganization(xmlTextReader, false, invoice);
                                break;
                            case XmlConstants.CurrencyCode:
                                xmlTextReader.Read();
                                invoice.CurrencyofListing = xmlTextReader.Value.ToUpper().Trim();
                                break;
                            case XmlConstants.ClearanceCurrencyCode:
                                xmlTextReader.Read();
                                invoice.CurrencyofBilling = xmlTextReader.Value.ToUpper().Trim();
                                break;
                            case XmlConstants.ExchangeRate:
                                xmlTextReader.Read();
                                invoice.ListingToBillingRate = Convert.ToDecimal(xmlTextReader.Value);
                                break;
                            case XmlConstants.SettlementMonthPeriod:
                                xmlTextReader.Read();
                                DateTime settlementDate;
                                if (DateTime.TryParseExact(xmlTextReader.Value, XmlConstants.InvoiceDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out settlementDate))
                                {
                                    invoice.BillingYear = settlementDate.Year;
                                    invoice.BillingMonth = settlementDate.Month;
                                    invoice.PeriodNumber = settlementDate.Day;
                                }
                                break;
                            case XmlConstants.SettlementMethod:
                                xmlTextReader.Read();
                                invoice.SettlementMethodIndicator = xmlTextReader.Value;
                                break;
                            case XmlConstants.DigitalSignatureFlag:
                                xmlTextReader.Read();
                                invoice.DigitalSignatureFlag = xmlTextReader.Value;
                                break;
                            case XmlConstants.SuspendedFlag:
                                xmlTextReader.Read();
                                invoice.SuspendedInvoiceFlag = xmlTextReader.Value;
                                break;
                            case XmlConstants.Language:
                                xmlTextReader.Read();
                                invoice.InvoiceTemplateLanguage = xmlTextReader.Value;
                                break;
                            case XmlConstants.NetDueDate:
                                xmlTextReader.Read();
                                invoice.ChDueDate = xmlTextReader.Value;
                                break;
                            case XmlConstants.ChAgreementIndicator:
                                xmlTextReader.Read();
                                invoice.ChAgreementIndicator = xmlTextReader.Value;
                                break;
                            case XmlConstants.LineItem:
                                ReadBillingCodeTotal(xmlTextReader, invoice, htChargeCode);
                                break;
                            case XmlConstants.LineItemDetail:
                                lineItemDetailCount++;
                                ReadLineItemDetails(xmlTextReader, invoice, htChargeCode);
                                break;
                            case XmlConstants.InvoiceSummary:
                                ReadInvoiceSummary(xmlTextReader, invoice, lineItemDetailCount);
                                break;
                        }
                    }
                }

                //if (!string.IsNullOrEmpty(invoice.InvoiceFooterDetails))
                //{
                //    invoice.InvoiceFooterDetails = invoice.InvoiceFooterDetails;
                //}

                // fileRecordSequenceNumber = _fileRecordSequenceNumber;

                //Logger.Info("End of ReadInvoice.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadInvoice", xmlException);
                _staticLogger?.LogError(xmlException, "Error Occurred in ReadInvoice");
                throw;
            }
        }

        #region Read Seller And Buyer Organization

        /// <summary>
        /// To Read Seller And Buyer Organization details (Reference Data) from XML File.
        /// </summary>
        /// <param name="xmlTextReader"></param>
        /// <param name="isSeller"></param>
        /// <param name="invoice"></param>
        private static void ReadSellerAndBuyerOrganization(XmlTextReader xmlTextReader, bool isSeller, Invoice invoice)
        {
            try
            {
                //Logger.Info("Start of ReadSellerAndBuyerOrganization.");

                ReferenceDataModel billingReferenceDataModel = new ReferenceDataModel();

                if (isSeller)
                {
                    //Logger.Info("Start of ReadSellerOrganization.");
                    
                    billingReferenceDataModel.IsBillingMember = true;
                }
                else
                {
                    //Logger.Info("Start of ReadBuyerOrganization.");

                    billingReferenceDataModel.IsBillingMember = false;
                }                

                while (xmlTextReader.Read())
                {
                    if (isSeller)
                    {
                        if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.SellerOrganization)))
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.BuyerOrganization)))
                        {
                            break;
                        }
                    }
                    
                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.OrganizationID:
                                xmlTextReader.Read();
                                billingReferenceDataModel.AirlineCode = xmlTextReader.Value;
                                if (isSeller)
                                {
                                    invoice.BillingAirline = billingReferenceDataModel.AirlineCode;
                                }
                                else
                                {
                                    invoice.BilledAirline = billingReferenceDataModel.AirlineCode;
                                }
                                break;
                            case XmlConstants.OrganizationDesignator:
                                xmlTextReader.Read();
                                billingReferenceDataModel.OrganizationDesignator = xmlTextReader.Value;
                                break;
                            case XmlConstants.OrganizationName1:
                                xmlTextReader.Read();
                                billingReferenceDataModel.CompanyLegalName = xmlTextReader.Value;
                                break;
                            case XmlConstants.TaxRegistrationID:
                                xmlTextReader.Read();
                                billingReferenceDataModel.TaxVATRegistrationID = xmlTextReader.Value;
                                break;
                            case XmlConstants.AdditionalTaxRegistrationId:
                                xmlTextReader.Read();
                                billingReferenceDataModel.AdditionalTaxVATRegistrationID = xmlTextReader.Value;
                                break;
                            case XmlConstants.CompanyRegistrationId:
                                xmlTextReader.Read();
                                billingReferenceDataModel.CompanyRegistrationID = xmlTextReader.Value;
                                break;
                            case XmlConstants.AddressLine1:
                                xmlTextReader.Read();
                                billingReferenceDataModel.AddressLine1 = xmlTextReader.Value;
                                break;
                            case XmlConstants.AddressLine2:
                                xmlTextReader.Read();
                                billingReferenceDataModel.AddressLine2 = xmlTextReader.Value;
                                break;
                            case XmlConstants.AddressLine3:
                                xmlTextReader.Read();
                                billingReferenceDataModel.AddressLine3 = xmlTextReader.Value;
                                break;
                            case XmlConstants.CityName:
                                xmlTextReader.Read();
                                billingReferenceDataModel.CityName = xmlTextReader.Value;
                                break;
                            case XmlConstants.SubdivisionCode:
                                xmlTextReader.Read();
                                billingReferenceDataModel.SubDivisionCode = xmlTextReader.Value;
                                break;
                            case XmlConstants.SubdivisionName:
                                xmlTextReader.Read();
                                billingReferenceDataModel.SubDivisionName = xmlTextReader.Value;
                                break;
                            case XmlConstants.CountryCode:
                                xmlTextReader.Read();
                                billingReferenceDataModel.CountryCode = xmlTextReader.Value;
                                break;
                            case XmlConstants.CountryName:
                                xmlTextReader.Read();
                                billingReferenceDataModel.CountryName = xmlTextReader.Value;
                                break;
                            case XmlConstants.PostalCode:
                                xmlTextReader.Read();
                                billingReferenceDataModel.PostalCode = xmlTextReader.Value;
                                break;
                            case XmlConstants.LocationID:
                                xmlTextReader.Read();
                                invoice.BillingAirlineLocationID = xmlTextReader.Value;
                                break;
                        }
                    }
                }
                invoice.ReferenceDataList.Add(billingReferenceDataModel);

                //if (isSeller)
                //{
                //    Logger.Info("End of ReadSellerOrganization.");
                //}
                //else
                //{
                //    Logger.Info("End of ReadBuyerOrganization.");
                //}

                //Logger.Info("Start of ReadSellerAndBuyerOrganization.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadSellerAndBuyerOrganization", xmlException);
                _staticLogger?.LogError("Error Occurred in ReadSellerAndBuyerOrganization {0}", xmlException);
                throw;
            }
        }

        #endregion

        #region Read Lineitem

        /// <summary>
        /// To Read Billing Code Total from XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="invoice">Invoice</param>
        /// <param name="htChargeCode">Hashtable</param>
        private static void ReadBillingCodeTotal(XmlTextReader xmlTextReader, Invoice invoice, Hashtable htChargeCode)
        {
            try
            {
                //Logger.Info("Start of ReadBillingCodeTotal.");

                var billingCodeTotal = new QidWorkerRole.SIS.Model.BillingCodeSubTotal();
                int lineItemNumber = 0;

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.LineItem)))
                    {
                        break;
                    }
                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.LineItemNumber:
                                xmlTextReader.Read();
                                lineItemNumber = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.ChargeCode:
                                xmlTextReader.Read();
                                string billingCode = string.Empty;

                                switch (xmlTextReader.Value)
                                {
                                    case "Prepaid AirWaybill":
                                        billingCode = "P";
                                        break;
                                    case "Collect AirWaybill":
                                        billingCode = "C";
                                        break;
                                    case "Billing Memo":
                                        billingCode = "B";
                                        break;
                                    case "Credit Memo":
                                        billingCode = "T";
                                        break;
                                    case "Rejection Memo":
                                        billingCode = "R";
                                        break;
                                }
                                billingCodeTotal.BillingCode = billingCode;
                                htChargeCode[lineItemNumber] = billingCode;
                                break;

                            case XmlConstants.ChargeAmount:
                                AssignBillingCodeSubTotalChargeAmounts(billingCodeTotal, xmlTextReader);
                                break;
                            case XmlConstants.Description:
                                xmlTextReader.Read();
                                break;
                            case XmlConstants.Tax:
                                ReadLineItemVat(xmlTextReader, billingCodeTotal);
                                break;
                            case XmlConstants.AddOnCharges:
                                ReadBillingCodeTotalAddonCharges(xmlTextReader, billingCodeTotal);
                                break;
                            case XmlConstants.TotalNetAmount:
                                xmlTextReader.Read();
                                billingCodeTotal.TotalNetAmount = Convert.ToDecimal(xmlTextReader.Value);
                                billingCodeTotal.BillingCodeSbTotal = Convert.ToDecimal(xmlTextReader.Value);
                                break;
                            case XmlConstants.DetailCount:
                                xmlTextReader.Read();
                                billingCodeTotal.NumberOfBillingRecords = Convert.ToInt32(xmlTextReader.Value);
                                break;
                        }
                    }
                }

                invoice.BillingCodeSubTotalList.Add(billingCodeTotal);

                //Logger.Info("End of ReadBillingCodeTotal.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadBillingCodeTotal", xmlException);
                _staticLogger?.LogError("Error Occurred in ReadBillingCodeTotal {0}", xmlException);
                throw;
            }
        }

        #endregion

        #region Read Lineitem Detail i.e RM, BM, CM Details.

        /// <summary>
        /// To Read Lineitem Details i.e. RM, BM, CM from XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="invoice">Invoice</param>
        /// <param name="htChargeCode">Hashtable</param>
        private static void ReadLineItemDetails(XmlTextReader xmlTextReader, Invoice invoice, Hashtable htChargeCode)
        {
            try
            {
                //Logger.Info("Start of ReadLineItemDetails.");

                int batchSequenceNumber = 0;
                int recordSequenceNumberWithinBatch = 0;
                int lineItemNumber = 0;

                AirWayBill airWayBill;
                RejectionMemo rejectionMemo;
                BillingMemo billingMemo;
                CreditMemo creditMemo;

                ArrayList vatArrayList = new ArrayList();
                ArrayList otherChargesArrayList = new ArrayList();

                ChargeAmountsDetails chargeAmountsDetails = new ChargeAmountsDetails();
                VatAmountsDetails vatAmountsDetails = new VatAmountsDetails();
                AddOnChargeAmountsDetails addOnChargeAmountsDetails = new AddOnChargeAmountsDetails();

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.LineItemDetail)))
                    {
                        break;
                    }
                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.LineItemNumber:
                                xmlTextReader.Read();
                                lineItemNumber = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.BatchSequenceNumber:
                                xmlTextReader.Read();
                                batchSequenceNumber = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.RecordSequenceWithinBatch:
                                xmlTextReader.Read();
                                recordSequenceNumberWithinBatch = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.ChargeAmount:
                                ReadLineItemDetailChargeAmounts(chargeAmountsDetails, xmlTextReader);
                                break;
                            case XmlConstants.Tax:
                                ReadLineItemDetailsVat(xmlTextReader, vatArrayList, vatAmountsDetails);
                                break;
                            case XmlConstants.AddOnCharges:
                                ReadLineItemdetailAddonCharges(xmlTextReader, addOnChargeAmountsDetails, otherChargesArrayList);
                                break;
                            case XmlConstants.TotalNetAmount:
                                xmlTextReader.Read();
                                chargeAmountsDetails.TotalNetAmount = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.AirWaybillDetails:
                                airWayBill = new AirWayBill
                                {
                                    BatchSequenceNumber = batchSequenceNumber,
                                    RecordSequenceWithinBatch = recordSequenceNumberWithinBatch
                                };
                                //if (_fileRecordSequenceNumber.ContainsKey(batchSequenceNumber))
                                //{
                                //    _fileRecordSequenceNumber[batchSequenceNumber].Add(airWayBill.AirWayBillID, recordSequenceNumberWithinBatch);
                                //}
                                //else
                                //{
                                //    _fileRecordSequenceNumber.Add(batchSequenceNumber, new Dictionary<int, int>());
                                //    _fileRecordSequenceNumber[batchSequenceNumber].Add(airWayBill.AirWayBillID, recordSequenceNumberWithinBatch);
                                //}

                                if (htChargeCode.ContainsKey(lineItemNumber))
                                {
                                    airWayBill.BillingCode = htChargeCode[lineItemNumber].ToString();
                                }

                                ReadAirWayBill(xmlTextReader, airWayBill);
                                AssignChargeAmounts(airWayBill, chargeAmountsDetails);
                                AssignVatDetails(airWayBill, vatArrayList, vatAmountsDetails, otherChargesArrayList);
                                AssignAddOnCharges(airWayBill, addOnChargeAmountsDetails);
                                airWayBill.OtherCharges = ReadOtherChargeDetails(airWayBill, otherChargesArrayList, airWayBill.OtherCharges);
                                invoice.AirWayBillList.Add(airWayBill);
                                break;
                            case XmlConstants.RejectionMemoDetails:
                                rejectionMemo = new RejectionMemo
                                {
                                    BatchSequenceNumber = batchSequenceNumber,
                                    RecordSequenceWithinBatch = recordSequenceNumberWithinBatch
                                };
                                //if (_fileRecordSequenceNumber.ContainsKey(batchSequenceNumber))
                                //{
                                //    _fileRecordSequenceNumber[batchSequenceNumber].Add(rejectionMemo.RejectionMemoID, recordSequenceNumberWithinBatch);
                                //}
                                //else
                                //{
                                //    _fileRecordSequenceNumber.Add(batchSequenceNumber, new Dictionary<int, int>());
                                //    _fileRecordSequenceNumber[batchSequenceNumber].Add(rejectionMemo.RejectionMemoID, recordSequenceNumberWithinBatch);
                                //}

                                if (htChargeCode.ContainsKey(lineItemNumber))
                                {
                                    rejectionMemo.BillingCode = htChargeCode[lineItemNumber].ToString();
                                }
                                ReadRejectionMemo(xmlTextReader, rejectionMemo);
                                AssignChargeAmounts(rejectionMemo, chargeAmountsDetails);
                                AssignVatDetails(rejectionMemo, vatArrayList, vatAmountsDetails, otherChargesArrayList);
                                AssignAddOnCharges(rejectionMemo, addOnChargeAmountsDetails);
                                invoice.RejectionMemoList.Add(rejectionMemo);
                                break;
                            case XmlConstants.BillingMemoDetails:
                                billingMemo = new BillingMemo
                                {
                                    BatchSequenceNumber = batchSequenceNumber,
                                    RecordSequenceWithinBatch = recordSequenceNumberWithinBatch
                                };
                                //if (_fileRecordSequenceNumber.ContainsKey(batchSequenceNumber))
                                //{
                                //    _fileRecordSequenceNumber[batchSequenceNumber].Add(billingMemo.BillingMemoID, recordSequenceNumberWithinBatch);
                                //}
                                //else
                                //{
                                //    _fileRecordSequenceNumber.Add(batchSequenceNumber, new Dictionary<int, int>());
                                //    _fileRecordSequenceNumber[batchSequenceNumber].Add(billingMemo.BillingMemoID, recordSequenceNumberWithinBatch);
                                //}

                                if (htChargeCode.ContainsKey(lineItemNumber))
                                {
                                    billingMemo.BillingCode = htChargeCode[lineItemNumber].ToString();
                                }
                                ReadBillingMemo(xmlTextReader, billingMemo);
                                AssignChargeAmounts(billingMemo, chargeAmountsDetails);
                                AssignVatDetails(billingMemo, vatArrayList, vatAmountsDetails, otherChargesArrayList);
                                AssignAddOnCharges(billingMemo, addOnChargeAmountsDetails);
                                invoice.BillingMemoList.Add(billingMemo);
                                break;
                            case XmlConstants.CreditMemoDetails:
                                creditMemo = new CreditMemo
                                {
                                    BatchSequenceNumber = batchSequenceNumber,
                                    RecordSequenceWithinBatch = recordSequenceNumberWithinBatch
                                };
                                //if (_fileRecordSequenceNumber.ContainsKey(batchSequenceNumber))
                                //{
                                //    _fileRecordSequenceNumber[batchSequenceNumber].Add(creditMemo.CreditMemoID, recordSequenceNumberWithinBatch);
                                //}
                                //else
                                //{
                                //    _fileRecordSequenceNumber.Add(batchSequenceNumber, new Dictionary<int, int>());
                                //    _fileRecordSequenceNumber[batchSequenceNumber].Add(creditMemo.CreditMemoID, recordSequenceNumberWithinBatch);
                                //}

                                if (htChargeCode.ContainsKey(lineItemNumber))
                                {
                                    creditMemo.BillingCode = htChargeCode[lineItemNumber].ToString();
                                }
                                ReadCreditMemo(xmlTextReader, creditMemo);
                                AssignChargeAmounts(creditMemo, chargeAmountsDetails);
                                AssignVatDetails(creditMemo, vatArrayList, vatAmountsDetails, otherChargesArrayList);
                                AssignAddOnCharges(creditMemo, addOnChargeAmountsDetails);
                                invoice.CreditMemoList.Add(creditMemo);
                                break; 
                        }
                    }
                }
                //Logger.Info("End of ReadLineItemDetails.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadLineItemDetails", xmlException);
                _staticLogger?.LogError("Error Occurred in ReadLineItemDetails {0}", xmlException);
                throw;

            }
        }

        #region Read AirWayBill

        /// <summary>
        /// To Read AirWayBill from XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="airWayBill">AirWayBill</param>
        private static void ReadAirWayBill(XmlTextReader xmlTextReader, AirWayBill airWayBill)
        {
            try
            {
                //Logger.Info("Start of ReadAirWayBill.");
                
                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.AirWaybillDetails)))
                    {
                        break;
                    }
                    
                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.AWBDate:
                                xmlTextReader.Read();
                                DateTime awbDate;
                                if (DateTime.TryParseExact(xmlTextReader.Value, XmlConstants.BillingDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out awbDate))
                                {
                                    airWayBill.AWBDate = awbDate;
                                }
                                break;
                            case XmlConstants.AWBIssuingAirline:
                                xmlTextReader.Read();
                                airWayBill.AWBIssuingAirline = xmlTextReader.Value;
                                break;
                            case XmlConstants.AWBSerialNumber:
                                xmlTextReader.Read();
                                airWayBill.AWBSerialNumber = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.AWBCheckDigit:
                                xmlTextReader.Read();
                                airWayBill.AWBCheckDigit = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.OriginAirportCode:
                                xmlTextReader.Read();
                                airWayBill.Origin = xmlTextReader.Value;
                                break;
                            case XmlConstants.DestinationAirportCode:
                                xmlTextReader.Read();
                                airWayBill.Destination = xmlTextReader.Value;
                                break;
                            case XmlConstants.FromAirportCode:
                                xmlTextReader.Read();
                                airWayBill.From = xmlTextReader.Value;
                                break;
                            case XmlConstants.ToAirportOrPointOfTransferCode:
                                xmlTextReader.Read();
                                airWayBill.To = xmlTextReader.Value;
                                break;
                            case XmlConstants.DateOfCarriageOrTransfer:
                                xmlTextReader.Read();
                                DateTime dateOfCarriageOrTransfer;
                                if (DateTime.TryParseExact(xmlTextReader.Value, XmlConstants.BillingDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateOfCarriageOrTransfer))
                                {
                                    //airWayBill.DateOfCarriageOrTransfer = dateOfCarriageOrTransfer.Year.ToString("yy") + dateOfCarriageOrTransfer.Month.ToString().PadLeft(2, '0') + dateOfCarriageOrTransfer.Day.ToString().PadLeft(2, '0');
                                    airWayBill.DateOfCarriageOrTransfer = dateOfCarriageOrTransfer.ToString("yyMMdd");
                                }
                                break;
                            case XmlConstants.CurrAdjustmentIndicator:
                                xmlTextReader.Read();
                                airWayBill.CurrencyAdjustmentIndicator = xmlTextReader.Value;
                                break;
                            case XmlConstants.BilledWeight:
                                xmlTextReader.Read();
                                airWayBill.BilledWeight = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.ProvisoReqSPA:
                                xmlTextReader.Read();
                                airWayBill.ProvisoReqSPA = xmlTextReader.Value;
                                break;
                            case XmlConstants.ProratePercentage:
                                xmlTextReader.Read();
                                airWayBill.ProratePercentage = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.PartShipmentIndicator:
                                xmlTextReader.Read();
                                airWayBill.PartShipmentIndicator = xmlTextReader.Value;
                                break;
                            case XmlConstants.KgLbIndicator:
                                xmlTextReader.Read();
                                airWayBill.KGLBIndicator = xmlTextReader.Value;
                                break;
                            case XmlConstants.CCAIndicator:
                                xmlTextReader.Read();
                                airWayBill.CCAindicator = xmlTextReader.Value.Equals(XmlConstants.Y) ? true : false;
                                break;
                            case XmlConstants.OurRef:
                                xmlTextReader.Read();
                                airWayBill.OurReference = xmlTextReader.Value;
                                break;
                            case XmlConstants.Attachment:
                                xmlTextReader.Read();
                                ReadAttachmentDetails(xmlTextReader, airWayBill);
                                break;
                            case XmlConstants.IsValidationFlag:
                                xmlTextReader.Read();
                                airWayBill.ISValidationFlag = xmlTextReader.Value;
                                break;
                            case XmlConstants.ReasonCode:
                                xmlTextReader.Read();
                                airWayBill.ReasonCode = xmlTextReader.Value;
                                break;
                            case XmlConstants.ReferenceField10AN:
                                xmlTextReader.Read();
                                if (string.IsNullOrEmpty(airWayBill.ReferenceField1))
                                {
                                    airWayBill.ReferenceField1 = xmlTextReader.Value;
                                }
                                else if (string.IsNullOrEmpty(airWayBill.ReferenceField2))
                                {
                                    airWayBill.ReferenceField2 = xmlTextReader.Value;
                                }
                                else if (string.IsNullOrEmpty(airWayBill.ReferenceField3))
                                {
                                    airWayBill.ReferenceField3 = xmlTextReader.Value;
                                }
                                else if (string.IsNullOrEmpty(airWayBill.ReferenceField4))
                                {
                                    airWayBill.ReferenceField4 = xmlTextReader.Value;
                                }
                                break;
                            case XmlConstants.ReferenceField20AN:
                                xmlTextReader.Read();
                                airWayBill.ReferenceField5 = xmlTextReader.Value;
                                break;
                            case XmlConstants.AirlineOwnUse20AN:
                                xmlTextReader.Read();
                                airWayBill.AirlineOwnUse = xmlTextReader.Value;
                                break;
                            case XmlConstants.FilingReference:
                                xmlTextReader.Read();
                                airWayBill.FilingReference = xmlTextReader.Value;
                                break;
                        }
                    }
                }
                //Logger.Info("End of ReadAirWayBill.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadAirWayBill", xmlException);
                _staticLogger?.LogError("Error Occurred in ReadAirWayBill {0}", xmlException);
                throw;
            }
        }
        
        #endregion
        
        #region Read RejectionMemo

        /// <summary>
        /// To Read RejectionMemo from XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="rejectionMemo">RejectionMemo</param>
        private static void ReadRejectionMemo(XmlTextReader xmlTextReader, RejectionMemo rejectionMemo)
        {
            try
            {
                //Logger.Info("Start of ReadRejectionMemo.");

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.RejectionMemoDetails)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.RejectionMemoNumber:
                                xmlTextReader.Read();
                                rejectionMemo.RejectionMemoNumber = string.IsNullOrEmpty(xmlTextReader.Value) ? string.Empty : xmlTextReader.Value.Trim();
                                break;
                            case XmlConstants.RejectionStage:
                                xmlTextReader.Read();
                                rejectionMemo.RejectionStage = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.ReasonCode:
                                xmlTextReader.Read();
                                rejectionMemo.ReasonCode = xmlTextReader.Value;
                                break;
                            case XmlConstants.ReasonDescription:
                                xmlTextReader.Read();
                                if (string.IsNullOrWhiteSpace(rejectionMemo.ReasonRemarks))
                                {
                                    rejectionMemo.ReasonRemarks = xmlTextReader.Value;
                                }
                                else
                                {
                                    rejectionMemo.ReasonRemarks = rejectionMemo.ReasonRemarks + xmlTextReader.Value;
                                }
                                break;
                            case XmlConstants.OurRef:
                                xmlTextReader.Read();
                                rejectionMemo.OurRef = xmlTextReader.Value;
                                break;
                            case XmlConstants.YourInvoiceNumber:
                                xmlTextReader.Read();
                                rejectionMemo.YourInvoiceNumber = string.IsNullOrEmpty(xmlTextReader.Value) ? string.Empty : xmlTextReader.Value.Trim();
                                break;
                            case XmlConstants.YourInvoiceBillingDate:
                                xmlTextReader.Read();
                                DateTime invoiceBillingDate;
                                if (DateTime.TryParseExact(xmlTextReader.Value, XmlConstants.InvoiceDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out invoiceBillingDate))
                                {
                                    rejectionMemo.YourInvoiceBillingYear = invoiceBillingDate.Year;
                                    rejectionMemo.YourInvoiceBillingMonth = invoiceBillingDate.Month;
                                    rejectionMemo.YourInvoiceBillingPeriod = invoiceBillingDate.Day;
                                }
                                break;
                            case XmlConstants.YourRejectionMemoNumber:
                                xmlTextReader.Read();
                                rejectionMemo.YourRejectionNumber = string.IsNullOrEmpty(xmlTextReader.Value) ? string.Empty : xmlTextReader.Value.Trim();
                                break;
                            case XmlConstants.FimBmCmIndicator:
                                xmlTextReader.Read();
                                rejectionMemo.BMCMIndicator = xmlTextReader.Value;
                                break;
                            case XmlConstants.LinkedFimBmCmNumber:
                                xmlTextReader.Read();
                                if (!string.IsNullOrEmpty(xmlTextReader.Value))
                                {
                                    rejectionMemo.YourBillingMemoNumber = xmlTextReader.Value.Trim();
                                }
                                else
                                {
                                    rejectionMemo.YourBillingMemoNumber = xmlTextReader.Value;
                                }
                                break;
                            case XmlConstants.Attachment:
                                xmlTextReader.Read();
                                ReadAttachmentDetails(xmlTextReader, rejectionMemo);
                                break;
                            case XmlConstants.IsValidationFlag:
                                xmlTextReader.Read();
                                rejectionMemo.ISValidationFlag = xmlTextReader.Value;
                                break;
                            case XmlConstants.AirlineOwnUse20AN:
                                xmlTextReader.Read();
                                rejectionMemo.AirlineOwnUse = xmlTextReader.Value;
                                break;
                            case XmlConstants.AirWaybillBreakdown:
                                RMAirWayBill rMAirWayBill = new RMAirWayBill();
                                ReadRMAirWayBill(xmlTextReader, rMAirWayBill);
                                rejectionMemo.RMAirWayBillList.Add(rMAirWayBill);
                                break;
                        }
                    }
                }
                //Logger.Info("End of ReadRejectionMemo.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadRejectionMemo", xmlException);
                _staticLogger?.LogError("Error Occurred in ReadRejectionMemo {0}", xmlException);
                throw;
            }
        }

        #region Read RMAirWayBill

        /// <summary>
        /// To Read RMAirWayBill from XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="rMAirWayBill">RMAirWayBill</param>
        private static void ReadRMAirWayBill(XmlTextReader xmlTextReader, RMAirWayBill rMAirWayBill)
        {
            try
            {
                //Logger.Info("Start of ReadRMAirWayBill.");

                ArrayList arrVat = new ArrayList();
                ArrayList arrOtherCharges = new ArrayList();
                var proRateLadderCount = 0;
                ChargeAmountsDetails chargeAmounts = new ChargeAmountsDetails();
                VatAmountsDetails vatAmounts = new VatAmountsDetails();
                AddOnChargeAmountsDetails addOnChargeAmounts = new AddOnChargeAmountsDetails();

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.AirWaybillBreakdown)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.BreakdownSerialNumber:
                                xmlTextReader.Read();
                                rMAirWayBill.BreakdownSerialNumber = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.AWBDate:
                                xmlTextReader.Read();
                                DateTime awbDate;
                                if (DateTime.TryParseExact(xmlTextReader.Value, XmlConstants.BillingDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out awbDate))
                                {
                                    rMAirWayBill.AWBDate = awbDate;
                                }
                                else
                                {
                                    rMAirWayBill.AwbDateDisplayText = xmlTextReader.Value;
                                }
                                break;
                            case XmlConstants.BillingCode:
                                xmlTextReader.Read();
                                //string billingCode = string.Empty;

                                //switch (xmlTextReader.Value)
                                //{
                                //    case "Prepaid AirWaybill":
                                //        billingCode = "P";
                                //        break;
                                //    case "Collect AirWaybill":
                                //        billingCode = "C";
                                //        break;
                                //}
                                //rMAirWayBill.BillingCode = billingCode;
                                rMAirWayBill.BillingCode = xmlTextReader.Value;
                                break;
                            case XmlConstants.AWBIssuingAirline:
                                xmlTextReader.Read();
                                rMAirWayBill.AWBIssuingAirline = xmlTextReader.Value;
                                break;
                            case XmlConstants.AWBSerialNumber:
                                xmlTextReader.Read();
                                rMAirWayBill.AWBSerialNumber = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.AWBCheckDigit:
                                xmlTextReader.Read();
                                rMAirWayBill.AWBCheckDigit = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.ChargeAmount:
                                ReadLineItemDetailChargeAmounts(chargeAmounts, xmlTextReader);
                                break;
                            case XmlConstants.Tax:
                                ReadLineItemDetailsVat(xmlTextReader, arrVat, vatAmounts);
                                break;
                            case XmlConstants.AddOnCharges:
                                ReadLineItemdetailAddonCharges(xmlTextReader, addOnChargeAmounts, arrOtherCharges);
                                break;
                            case XmlConstants.TotalNetAmount:
                                xmlTextReader.Read();
                                rMAirWayBill.NetRejectAmount = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.OriginAirportCode:
                                xmlTextReader.Read();
                                rMAirWayBill.Origin = xmlTextReader.Value;
                                break;
                            case XmlConstants.DestinationAirportCode:
                                xmlTextReader.Read();
                                rMAirWayBill.Destination = xmlTextReader.Value;
                                break;
                            case XmlConstants.FromAirportCode:
                                xmlTextReader.Read();
                                rMAirWayBill.From = xmlTextReader.Value;
                                break;
                            case XmlConstants.ToAirportOrPointOfTransferCode:
                                xmlTextReader.Read();
                                rMAirWayBill.To = xmlTextReader.Value;
                                break;
                            case XmlConstants.DateOfCarriageOrTransfer:
                                xmlTextReader.Read();
                                DateTime dateOfCarriageOrTransfer;
                                if (DateTime.TryParseExact(xmlTextReader.Value, XmlConstants.BillingDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateOfCarriageOrTransfer))
                                {
                                    //rMAirWayBill.DateOfCarriageOrTransfer = dateOfCarriageOrTransfer.Year.ToString("yy") + dateOfCarriageOrTransfer.Month.ToString().PadLeft(2, '0') + dateOfCarriageOrTransfer.Day.ToString().PadLeft(2, '0');
                                    rMAirWayBill.DateOfCarriageOrTransfer = dateOfCarriageOrTransfer.ToString("yyMMdd");
                                }
                                break;
                            case XmlConstants.CurrAdjustmentIndicator:
                                xmlTextReader.Read();
                                rMAirWayBill.CurrencyAdjustmentIndicator = xmlTextReader.Value;
                                break;
                            case XmlConstants.BilledWeight:
                                xmlTextReader.Read();
                                rMAirWayBill.BilledWeight = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.ProvisoReqSPA:
                                xmlTextReader.Read();
                                rMAirWayBill.ProvisionalReqSpa = xmlTextReader.Value;
                                break;
                            case XmlConstants.ProratePercentage:
                                xmlTextReader.Read();
                                rMAirWayBill.ProratePercentage = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.PartShipmentIndicator:
                                xmlTextReader.Read();
                                rMAirWayBill.PartShipmentIndicator = xmlTextReader.Value;
                                break;
                            case XmlConstants.KgLbIndicator:
                                xmlTextReader.Read();
                                rMAirWayBill.KgLbIndicator = xmlTextReader.Value;
                                break;
                            case XmlConstants.CCAIndicator:
                                xmlTextReader.Read();
                                if (xmlTextReader.Value.Equals(XmlConstants.Y))
                                {
                                    rMAirWayBill.CcaIndicator = true;
                                }
                                else
                                {
                                    rMAirWayBill.CcaIndicator = false;
                                }
                                break;
                            case XmlConstants.ReasonCode:
                                xmlTextReader.Read();
                                rMAirWayBill.ReasonCode = xmlTextReader.Value;
                                break;
                            case XmlConstants.IsValidationFlag:
                                xmlTextReader.Read();
                                rMAirWayBill.ISValidationFlag = xmlTextReader.Value;
                                break;
                            case XmlConstants.ReferenceField10AN:
                                xmlTextReader.Read();
                                if (string.IsNullOrEmpty(rMAirWayBill.ReferenceField1))
                                {
                                    rMAirWayBill.ReferenceField1 = xmlTextReader.Value;
                                }
                                else if (string.IsNullOrEmpty(rMAirWayBill.ReferenceField2))
                                {
                                    rMAirWayBill.ReferenceField2 = xmlTextReader.Value;
                                }
                                else if (string.IsNullOrEmpty(rMAirWayBill.ReferenceField3))
                                {
                                    rMAirWayBill.ReferenceField3 = xmlTextReader.Value;
                                }
                                else if (string.IsNullOrEmpty(rMAirWayBill.ReferenceField4))
                                {
                                    rMAirWayBill.ReferenceField4 = xmlTextReader.Value;
                                }
                                break;
                            case XmlConstants.ReferenceField20AN:
                                xmlTextReader.Read();
                                rMAirWayBill.ReferenceField5 = xmlTextReader.Value;
                                break;
                            case XmlConstants.AirlineOwnUse20AN:
                                xmlTextReader.Read();
                                rMAirWayBill.AirlineOwnUse = xmlTextReader.Value;
                                break;
                            case XmlConstants.ProrateLadderBreakdown:
                                QidWorkerRole.SIS.Model.RMAWBProrateLadder rMAWBProrateLadder = new QidWorkerRole.SIS.Model.RMAWBProrateLadder { SequenceNumber = ++proRateLadderCount };
                                ReadRMAWBProrateLadder(xmlTextReader, rMAWBProrateLadder, rMAirWayBill);
                                rMAirWayBill.RMAWBProrateLadderList.Add(rMAWBProrateLadder);
                                break;
                            case XmlConstants.Attachment:
                                ReadAttachmentDetails(xmlTextReader, rMAirWayBill);
                                break;
                        }
                    }
                }
                AssignChargeAmounts(rMAirWayBill, chargeAmounts);
                AssignVatDetails(rMAirWayBill, arrVat, vatAmounts, arrOtherCharges);
                AssignAddOnCharges(rMAirWayBill, addOnChargeAmounts);
                rMAirWayBill.OtherChargeDiff = ReadOtherChargeDetails(rMAirWayBill, arrOtherCharges, rMAirWayBill.OtherChargeDiff);

                //Logger.Info("End of ReadRMAirWayBill.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadRMAirWayBill", xmlException);
                _staticLogger?.LogError("Error Occurred in ReadRMAirWayBill {0}", xmlException);
                throw;
            }
        }

        #endregion

        #region Read RMAWBProrateLadder

        /// <summary>
        /// To Read RMAWBProrateLadder from XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="rMAWBProrateLadder">RMAWBProrateLadder</param>
        /// <param name="rMAirWayBill">RMAirWayBill</param>
        private static void ReadRMAWBProrateLadder(XmlTextReader xmlTextReader, QidWorkerRole.SIS.Model.RMAWBProrateLadder rMAWBProrateLadder, QidWorkerRole.SIS.Model.RMAirWayBill rMAirWayBill)
        {
            try
            {
                //Logger.Info("Start of ReadRMAWBProrateLadder.");

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.ProrateLadderBreakdown)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.CurrencyOfProrateCalculation:
                                xmlTextReader.Read();
                                rMAWBProrateLadder.CurrencyofProrateCalculation = xmlTextReader.Value;
                                break;
                            case XmlConstants.TotalAmount:
                                xmlTextReader.Read();
                                rMAirWayBill.TotalProrateAmount = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.FromSector:
                                xmlTextReader.Read();
                                rMAWBProrateLadder.FromSector = xmlTextReader.Value;
                                break;
                            case XmlConstants.ToSector:
                                xmlTextReader.Read();
                                rMAWBProrateLadder.ToSector = xmlTextReader.Value;
                                break;
                            case XmlConstants.CarrierPrefix:
                                xmlTextReader.Read();
                                rMAWBProrateLadder.CarrierPrefix = xmlTextReader.Value;
                                break;
                            case XmlConstants.ProvisoReqSPAFlag:
                                xmlTextReader.Read();
                                rMAWBProrateLadder.ProvisoReqSpa = xmlTextReader.Value;
                                break;
                            case XmlConstants.ProrateFactor:
                                xmlTextReader.Read();
                                rMAWBProrateLadder.ProrateFactor = Convert.ToInt64(xmlTextReader.Value);
                                break;
                            case XmlConstants.PercentShare:
                                xmlTextReader.Read();
                                rMAWBProrateLadder.PercentShare = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.Amount:
                                xmlTextReader.Read();
                                rMAWBProrateLadder.TotalAmount = Convert.ToDouble(xmlTextReader.Value);
                                break;
                        }
                    }
                }
                //Logger.Info("End of ReadRMAWBProrateLadder.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadRMAWBProrateLadder", xmlException);
                _staticLogger?.LogError("Error Occurred in ReadRMAWBProrateLadder {0}", xmlException);
                throw;
            }
        }

        #endregion

        #endregion

        #region Read BillingMemo

        /// <summary>
        /// To Read BillingMemo from XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="billingMemo">BillingMemo</param>
        private static void ReadBillingMemo(XmlTextReader xmlTextReader, BillingMemo billingMemo)
        {
            try
            {
                //Logger.Info("Start of ReadBillingMemo.");

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.BillingMemoDetails)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.BillingMemoNumber:
                                xmlTextReader.Read();
                                billingMemo.BillingMemoNumber = xmlTextReader.Value;
                                break;
                            case XmlConstants.ReasonCode:
                                xmlTextReader.Read();
                                billingMemo.ReasonCode = xmlTextReader.Value;
                                break;
                            case XmlConstants.ReasonDescription:
                                xmlTextReader.Read();
                                if (billingMemo.ReasonRemarks == null)
                                {
                                    billingMemo.ReasonRemarks = xmlTextReader.Value;
                                }
                                else
                                {
                                    billingMemo.ReasonRemarks = billingMemo.ReasonRemarks + xmlTextReader.Value;
                                }
                                break;
                            case XmlConstants.CorrespondenceRefNumber:
                                xmlTextReader.Read();
                                billingMemo.CorrespondenceReferenceNumber =xmlTextReader.Value.Trim();
                                break;
                            case XmlConstants.OurRef:
                                xmlTextReader.Read();
                                billingMemo.OurRef = xmlTextReader.Value;
                                break;
                            case XmlConstants.YourInvoiceNumber:
                                xmlTextReader.Read();
                                billingMemo.YourInvoiceNumber = string.IsNullOrEmpty(xmlTextReader.Value) ? string.Empty : xmlTextReader.Value.Trim();
                                break;
                            case XmlConstants.YourInvoiceBillingDate:
                                xmlTextReader.Read();
                                DateTime yourInvoiceBillingDate;
                                if (DateTime.TryParseExact(xmlTextReader.Value, XmlConstants.InvoiceDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out yourInvoiceBillingDate))
                                {
                                    billingMemo.YourInvoiceBillingYear = yourInvoiceBillingDate.Year;
                                    billingMemo.YourInvoiceBillingMonth = yourInvoiceBillingDate.Month;
                                    billingMemo.YourInvoiceBillingPeriod = yourInvoiceBillingDate.Day;
                                }
                                break;
                            case XmlConstants.Attachment:
                                ReadAttachmentDetails(xmlTextReader, billingMemo);
                                break;
                            case XmlConstants.AirlineOwnUse20AN:
                                xmlTextReader.Read();
                                billingMemo.AirlineOwnUse = xmlTextReader.Value;
                                break;
                            case XmlConstants.IsValidationFlag:
                                xmlTextReader.Read();
                                billingMemo.ISValidationFlag = xmlTextReader.Value;
                                break;
                            case XmlConstants.AirWaybillBreakdown:
                                BMAirWayBill bMAirWayBill = new BMAirWayBill();
                                ReadBMAirWayBill(xmlTextReader, bMAirWayBill);
                                billingMemo.BMAirWayBillList.Add(bMAirWayBill);
                                break;
                        }
                    }
                }
                //Logger.Info("End of ReadBillingMemo.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadBillingMemo", xmlException);
                _staticLogger?.LogError("Error Occurred in ReadBillingMemo {0}", xmlException);
                throw;
            }
        }

        #region Read BMAirWayBill

        /// <summary>
        /// To Read BMAirWayBill from XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="bMAirWayBill">BMAirWayBill</param>
        private static void ReadBMAirWayBill(XmlTextReader xmlTextReader, BMAirWayBill bMAirWayBill)
        {
            try
            {
                //Logger.Info("Start of ReadBMAirWayBill.");

                ArrayList vatArrayList = new ArrayList();
                ArrayList OtherChargesArrayList = new ArrayList();
                ChargeAmountsDetails chargeAmountsDetails = new ChargeAmountsDetails();
                VatAmountsDetails vatAmountsDetails = new VatAmountsDetails();
                AddOnChargeAmountsDetails addOnChargeAmountsDetails = new AddOnChargeAmountsDetails();
                var proRateLadderCount = 0;

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.AirWaybillBreakdown)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.BreakdownSerialNumber:
                                xmlTextReader.Read();
                                bMAirWayBill.BreakdownSerialNumber = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.AWBDate:
                                xmlTextReader.Read();
                                DateTime awbDate;
                                if (DateTime.TryParseExact(xmlTextReader.Value, XmlConstants.BillingDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out awbDate))
                                {
                                    bMAirWayBill.AWBDate = awbDate;
                                }
                                else
                                {
                                    bMAirWayBill.AwbDateDisplayText = xmlTextReader.Value;
                                }
                                break;
                            case XmlConstants.BillingCode:
                                xmlTextReader.Read();
                                //string billingCode = string.Empty;
                                //switch (xmlTextReader.Value)
                                //{
                                //    case "Prepaid AirWaybill":
                                //        billingCode = "P";
                                //        break;
                                //    case "Collect AirWaybill":
                                //        billingCode = "C";
                                //        break;
                                //}
                                //bMAirWayBill.BillingCode = billingCode;
                                bMAirWayBill.BillingCode = xmlTextReader.Value;
                                break;
                            case XmlConstants.AWBIssuingAirline:
                                xmlTextReader.Read();
                                bMAirWayBill.AWBIssuingAirline = xmlTextReader.Value;
                                break;
                            case XmlConstants.AWBSerialNumber:
                                xmlTextReader.Read();
                                bMAirWayBill.AWBSerialNumber = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.AWBCheckDigit:
                                xmlTextReader.Read();
                                bMAirWayBill.AWBCheckDigit = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.ChargeAmount:
                                ReadLineItemDetailChargeAmounts(chargeAmountsDetails, xmlTextReader);
                                break;
                            case XmlConstants.Tax:
                                ReadLineItemDetailsVat(xmlTextReader, vatArrayList, vatAmountsDetails);
                                break;
                            case XmlConstants.AddOnCharges:
                                ReadLineItemdetailAddonCharges(xmlTextReader, addOnChargeAmountsDetails, OtherChargesArrayList);
                                break;
                            case XmlConstants.TotalNetAmount:
                                xmlTextReader.Read();
                                bMAirWayBill.TotalAmount = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.OriginAirportCode:
                                xmlTextReader.Read();
                                bMAirWayBill.Origin = xmlTextReader.Value;
                                break;
                            case XmlConstants.DestinationAirportCode:
                                xmlTextReader.Read();
                                bMAirWayBill.Destination = xmlTextReader.Value;
                                break;
                            case XmlConstants.FromAirportCode:
                                xmlTextReader.Read();
                                bMAirWayBill.From = xmlTextReader.Value;
                                break;
                            case XmlConstants.ToAirportOrPointOfTransferCode:
                                xmlTextReader.Read();
                                bMAirWayBill.To = xmlTextReader.Value;
                                break;
                            case XmlConstants.DateOfCarriageOrTransfer:
                                xmlTextReader.Read();
                                DateTime dateOfCarriageOrTransfer;
                                if (DateTime.TryParseExact(xmlTextReader.Value, XmlConstants.BillingDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateOfCarriageOrTransfer))
                                {
                                    //bMAirWayBill.DateOfCarriageOrTransfer = dateOfCarriageOrTransfer.Year.ToString("yy") + dateOfCarriageOrTransfer.Month.ToString().PadLeft(2, '0') + dateOfCarriageOrTransfer.Day.ToString().PadLeft(2, '0');
                                    bMAirWayBill.DateOfCarriageOrTransfer = dateOfCarriageOrTransfer.ToString("yyMMdd");
                                }
                                break;
                            case XmlConstants.CurrAdjustmentIndicator:
                                xmlTextReader.Read();
                                bMAirWayBill.CurrencyAdjustmentIndicator = xmlTextReader.Value;
                                break;
                            case XmlConstants.BilledWeight:
                                xmlTextReader.Read();
                                bMAirWayBill.BilledWeight = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.ProvisoReqSPA:
                                xmlTextReader.Read();
                                bMAirWayBill.ProvisionalReqSpa = xmlTextReader.Value;
                                break;
                            case XmlConstants.ProratePercentage:
                                xmlTextReader.Read();
                                bMAirWayBill.PrpratePercentage = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.PartShipmentIndicator:
                                xmlTextReader.Read();
                                bMAirWayBill.PartShipmentIndicator = xmlTextReader.Value;
                                break;
                            case XmlConstants.KgLbIndicator:
                                xmlTextReader.Read();
                                bMAirWayBill.KgLbIndicator = xmlTextReader.Value;
                                break;
                            case XmlConstants.CCAIndicator:
                                xmlTextReader.Read();
                                if (xmlTextReader.Value.Equals(XmlConstants.Y))
                                {
                                    bMAirWayBill.CcaIndicator = true;
                                }
                                else
                                {
                                    bMAirWayBill.CcaIndicator = false;
                                }
                                break;
                            case XmlConstants.ReasonCode:
                                xmlTextReader.Read();
                                bMAirWayBill.ReasonCode = xmlTextReader.Value;
                                break;
                            case XmlConstants.IsValidationFlag:
                                xmlTextReader.Read();
                                bMAirWayBill.ISValidationFlag = xmlTextReader.Value;
                                break;
                            case XmlConstants.ReferenceField10AN:
                                xmlTextReader.Read();
                                if (string.IsNullOrEmpty(bMAirWayBill.ReferenceField1))
                                {
                                    bMAirWayBill.ReferenceField1 = xmlTextReader.Value;
                                }
                                else if (string.IsNullOrEmpty(bMAirWayBill.ReferenceField2))
                                {
                                    bMAirWayBill.ReferenceField2 = xmlTextReader.Value;
                                }
                                else if (string.IsNullOrEmpty(bMAirWayBill.ReferenceField3))
                                {
                                    bMAirWayBill.ReferenceField3 = xmlTextReader.Value;
                                }
                                else if (string.IsNullOrEmpty(bMAirWayBill.ReferenceField4))
                                {
                                    bMAirWayBill.ReferenceField4 = xmlTextReader.Value;
                                }
                                break;
                            case XmlConstants.ReferenceField20AN:
                                xmlTextReader.Read();
                                bMAirWayBill.ReferenceField5 = xmlTextReader.Value;
                                break;
                            case XmlConstants.AirlineOwnUse20AN:
                                xmlTextReader.Read();
                                bMAirWayBill.AirlineOwnUse = xmlTextReader.Value;
                                break;
                            case XmlConstants.ProrateLadderBreakdown:
                                BMAWBProrateLadder bMAWBProrateLadder = new BMAWBProrateLadder { SequenceNumber = ++proRateLadderCount };
                                ReadBMAWBProrateLadder(xmlTextReader, bMAWBProrateLadder, bMAirWayBill);
                                bMAirWayBill.BMAWBProrateLadderList.Add(bMAWBProrateLadder);
                                break;
                            case XmlConstants.Attachment:
                                ReadAttachmentDetails(xmlTextReader, bMAirWayBill);
                                break;
                        }
                    }
                }
                AssignChargeAmounts(bMAirWayBill, chargeAmountsDetails);
                AssignVatDetails(bMAirWayBill, vatArrayList, vatAmountsDetails, OtherChargesArrayList);
                AssignAddOnCharges(bMAirWayBill, addOnChargeAmountsDetails);
                bMAirWayBill.BilledOtherCharge = ReadOtherChargeDetails(bMAirWayBill, OtherChargesArrayList, bMAirWayBill.BilledOtherCharge);

                //Logger.Info("End of ReadBMAirWayBill.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadBMAirWayBill", xmlException);
                _staticLogger?.LogError("Error Occurred in ReadBMAirWayBill {0}", xmlException);
                throw;
            }
        }

        #endregion

        #region Read BMAWBProrateLadder

        /// <summary>
        /// To Read BMAWBProrateLadder from XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="bMAWBProrateLadder">BMAWBProrateLadder</param>
        /// <param name="bMAirWayBill">BMAirWayBill</param>
        private static void ReadBMAWBProrateLadder(XmlTextReader xmlTextReader, BMAWBProrateLadder bMAWBProrateLadder, BMAirWayBill bMAirWayBill)
        {
            try
            {
                //Logger.Info("Start of ReadBMAWBProrateLadder.");

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.ProrateLadderBreakdown)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.CurrencyOfProrateCalculation:
                                xmlTextReader.Read();
                                bMAWBProrateLadder.CurrencyofProrateCalculation = xmlTextReader.Value;
                                break;
                            case XmlConstants.TotalAmount:
                                xmlTextReader.Read();
                                bMAirWayBill.TotalProrateAmount = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.FromSector:
                                xmlTextReader.Read();
                                bMAWBProrateLadder.FromSector = xmlTextReader.Value;
                                break;
                            case XmlConstants.ToSector:
                                xmlTextReader.Read();
                                bMAWBProrateLadder.ToSector = xmlTextReader.Value;
                                break;
                            case XmlConstants.CarrierPrefix:
                                xmlTextReader.Read();
                                bMAWBProrateLadder.CarrierPrefix = xmlTextReader.Value;
                                break;
                            case XmlConstants.ProvisoReqSPAFlag:
                                xmlTextReader.Read();
                                bMAWBProrateLadder.ProvisoReqSpa = xmlTextReader.Value;
                                break;
                            case XmlConstants.ProrateFactor:
                                xmlTextReader.Read();
                                bMAWBProrateLadder.ProrateFactor = Convert.ToInt64(xmlTextReader.Value);
                                break;
                            case XmlConstants.PercentShare:
                                xmlTextReader.Read();
                                bMAWBProrateLadder.PercentShare = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.Amount:
                                xmlTextReader.Read();
                                bMAWBProrateLadder.TotalAmount = Convert.ToDouble(xmlTextReader.Value);
                                break;
                        }
                    }
                }
                //Logger.Info("End of ReadBMAWBProrateLadder.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadBMAWBProrateLadder", xmlException);
                _staticLogger?.LogError("Error Occurred in ReadBMAWBProrateLadder {0}", xmlException);
                throw;
            }
        }

        #endregion

        #endregion

        #region Read CreditMemo

        /// <summary>
        /// To Read CreditMemo from XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="creditMemo">CreditMemo</param>
        private static void ReadCreditMemo(XmlTextReader xmlTextReader, CreditMemo creditMemo)
        {
            try
            {
                //Logger.Info("Start of ReadCreditMemo.");

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.CreditMemoDetails)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.CreditMemoNumber:
                                xmlTextReader.Read();
                                creditMemo.CreditMemoNumber = xmlTextReader.Value;
                                break;
                            case XmlConstants.ReasonCode:
                                xmlTextReader.Read();
                                creditMemo.ReasonCode = xmlTextReader.Value;
                                break;
                            case XmlConstants.ReasonDescription:
                                xmlTextReader.Read();
                                if (creditMemo.ReasonRemarks == null)
                                {
                                    creditMemo.ReasonRemarks = xmlTextReader.Value;
                                }
                                else
                                {
                                    creditMemo.ReasonRemarks = creditMemo.ReasonRemarks + xmlTextReader.Value;
                                }
                                break;
                            case XmlConstants.CorrespondenceRefNumber:
                                xmlTextReader.Read();
                                creditMemo.CorrespondenceRefNumber = Convert.ToString(xmlTextReader.Value);
                                break;
                            case XmlConstants.OurRef:
                                xmlTextReader.Read();
                                creditMemo.OurRef = xmlTextReader.Value;
                                break;
                            case XmlConstants.YourInvoiceNumber:
                                xmlTextReader.Read();
                                creditMemo.YourInvoiceNumber = string.IsNullOrEmpty(xmlTextReader.Value) ? string.Empty : xmlTextReader.Value.Trim();
                                break;
                            case XmlConstants.YourInvoiceBillingDate:
                                xmlTextReader.Read();
                                DateTime yourInvoiceBillingDate;
                                if (DateTime.TryParseExact(xmlTextReader.Value, XmlConstants.InvoiceDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out yourInvoiceBillingDate))
                                {
                                    creditMemo.YourInvoiceBillingYear = yourInvoiceBillingDate.Year;
                                    creditMemo.YourInvoiceBillingMonth = yourInvoiceBillingDate.Month;
                                    creditMemo.YourInvoiceBillingPeriod = yourInvoiceBillingDate.Day;
                                }
                                break;
                            case XmlConstants.Attachment:
                                ReadAttachmentDetails(xmlTextReader, creditMemo);
                                break;
                            case XmlConstants.AirlineOwnUse20AN:
                                xmlTextReader.Read();
                                creditMemo.AirlineOwnUse = xmlTextReader.Value;
                                break;
                            case XmlConstants.IsValidationFlag:
                                xmlTextReader.Read();
                                creditMemo.ISValidationFlag = xmlTextReader.Value;
                                break;
                            case XmlConstants.AirWaybillBreakdown:
                                CMAirWayBill cMAirWayBill = new CMAirWayBill();
                                ReadCMAirWayBill(xmlTextReader, cMAirWayBill);
                                creditMemo.CMAirWayBillList.Add(cMAirWayBill);
                                break;
                        }
                    }
                }
                //Logger.Info("End of ReadCreditMemo.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadCreditMemo", xmlException);
                _staticLogger?.LogError("Error Occurred in ReadCreditMemo {0}", xmlException);
                throw;
            }
        }

        #region Read CMAirWayBill

        /// <summary>
        /// To Read CMAirWayBill from XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="cMAirWayBill">CMAirWayBill</param>
        private static void ReadCMAirWayBill(XmlTextReader xmlTextReader, CMAirWayBill cMAirWayBill)
        {
            try
            {
                //Logger.Info("Start of ReadCMAirWayBill.");

                ArrayList vatArrayList = new ArrayList();
                ArrayList otherChargesArrayList = new ArrayList();
                ChargeAmountsDetails chargeAmountsDetails = new ChargeAmountsDetails();
                VatAmountsDetails vatAmountsDetails = new VatAmountsDetails();
                AddOnChargeAmountsDetails addOnChargeAmountsDetails = new AddOnChargeAmountsDetails();
                var proRateLadderCount = 0;

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.AirWaybillBreakdown)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.BreakdownSerialNumber:
                                xmlTextReader.Read();
                                cMAirWayBill.BreakdownSerialNumber = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.AWBDate:
                                xmlTextReader.Read();
                                DateTime awbDate;
                                if (DateTime.TryParseExact(xmlTextReader.Value, XmlConstants.BillingDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out awbDate))
                                {
                                    cMAirWayBill.AWBDate = awbDate;
                                }
                                else
                                {
                                    cMAirWayBill.AwbDateDisplayText = xmlTextReader.Value;
                                }
                                break;
                            case XmlConstants.BillingCode:
                                xmlTextReader.Read();
                                //string billingCode = string.Empty;
                                //switch (xmlTextReader.Value)
                                //{
                                //    case "Prepaid AirWaybill":
                                //        billingCode = "P";
                                //        break;
                                //    case "Collect AirWaybill":
                                //        billingCode = "C";
                                //        break;
                                //}
                                //cMAirWayBill.BillingCode = billingCode;
                                cMAirWayBill.BillingCode = xmlTextReader.Value;
                                break;
                            case XmlConstants.AWBIssuingAirline:
                                xmlTextReader.Read();
                                cMAirWayBill.AWBIssuingAirline = xmlTextReader.Value;
                                break;
                            case XmlConstants.AWBSerialNumber:
                                xmlTextReader.Read();
                                cMAirWayBill.AWBSerialNumber = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.AWBCheckDigit:
                                xmlTextReader.Read();
                                cMAirWayBill.AWBCheckDigit = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.ChargeAmount:
                                ReadLineItemDetailChargeAmounts(chargeAmountsDetails, xmlTextReader);
                                break;
                            case XmlConstants.Tax:
                                ReadLineItemDetailsVat(xmlTextReader, vatArrayList, vatAmountsDetails);
                                break;
                            case XmlConstants.AddOnCharges:
                                ReadLineItemdetailAddonCharges(xmlTextReader, addOnChargeAmountsDetails, otherChargesArrayList);
                                break;
                            case XmlConstants.TotalNetAmount:
                                xmlTextReader.Read();
                                cMAirWayBill.TotalAmountCredited = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.OriginAirportCode:
                                xmlTextReader.Read();
                                cMAirWayBill.Origin = xmlTextReader.Value;
                                break;
                            case XmlConstants.DestinationAirportCode:
                                xmlTextReader.Read();
                                cMAirWayBill.Destination = xmlTextReader.Value;
                                break;
                            case XmlConstants.FromAirportCode:
                                xmlTextReader.Read();
                                cMAirWayBill.From = xmlTextReader.Value;
                                break;
                            case XmlConstants.ToAirportOrPointOfTransferCode:
                                xmlTextReader.Read();
                                cMAirWayBill.To = xmlTextReader.Value;
                                break;
                            case XmlConstants.DateOfCarriageOrTransfer:
                                xmlTextReader.Read();
                                DateTime dateOfCarriageOrTransfer;
                                if (DateTime.TryParseExact(xmlTextReader.Value, XmlConstants.BillingDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateOfCarriageOrTransfer))
                                {
                                    //cMAirWayBill.DateOfCarriageOrTransfer = dateOfCarriageOrTransfer.Year.ToString("yy") + dateOfCarriageOrTransfer.Month.ToString().PadLeft(2, '0') + dateOfCarriageOrTransfer.Day.ToString().PadLeft(2, '0');
                                    cMAirWayBill.DateOfCarriageOrTransfer = dateOfCarriageOrTransfer.ToString("yyMMdd");
                                }
                                break;
                            case XmlConstants.CurrAdjustmentIndicator:
                                xmlTextReader.Read();
                                cMAirWayBill.CurrencyAdjustmentIndicator = xmlTextReader.Value;
                                break;
                            case XmlConstants.BilledWeight:
                                xmlTextReader.Read();
                                cMAirWayBill.BilledWeight = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.ProvisoReqSPA:
                                xmlTextReader.Read();
                                cMAirWayBill.ProvisionalReqSpa = xmlTextReader.Value;
                                break;
                            case XmlConstants.ProratePercentage:
                                xmlTextReader.Read();
                                cMAirWayBill.ProratePercentage = Convert.ToInt32(xmlTextReader.Value);
                                break;
                            case XmlConstants.PartShipmentIndicator:
                                xmlTextReader.Read();
                                cMAirWayBill.PartShipmentIndicator = xmlTextReader.Value;
                                break;
                            case XmlConstants.KgLbIndicator:
                                xmlTextReader.Read();
                                cMAirWayBill.KgLbIndicator = xmlTextReader.Value;
                                break;
                            case XmlConstants.CCAIndicator:
                                xmlTextReader.Read();
                                if (xmlTextReader.Value.Equals(XmlConstants.Y))
                                {
                                    cMAirWayBill.CcaIndicator = true;
                                }
                                else
                                {
                                    cMAirWayBill.CcaIndicator = false;
                                }
                                break;
                            case XmlConstants.ReasonCode:
                                xmlTextReader.Read();
                                cMAirWayBill.ReasonCode = xmlTextReader.Value;
                                break;
                            case XmlConstants.IsValidationFlag:
                                xmlTextReader.Read();
                                cMAirWayBill.ISValidationFlag = xmlTextReader.Value;
                                break;
                            case XmlConstants.ReferenceField10AN:
                                xmlTextReader.Read();
                                if (string.IsNullOrEmpty(cMAirWayBill.ReferenceField1))
                                {
                                    cMAirWayBill.ReferenceField1 = xmlTextReader.Value;
                                }
                                else if (string.IsNullOrEmpty(cMAirWayBill.ReferenceField2))
                                {
                                    cMAirWayBill.ReferenceField2 = xmlTextReader.Value;
                                }
                                else if (string.IsNullOrEmpty(cMAirWayBill.ReferenceField3))
                                {
                                    cMAirWayBill.ReferenceField3 = xmlTextReader.Value;
                                }
                                else if (string.IsNullOrEmpty(cMAirWayBill.ReferenceField4))
                                {
                                    cMAirWayBill.ReferenceField4 = xmlTextReader.Value;
                                }
                                break;
                            case XmlConstants.ReferenceField20AN:
                                xmlTextReader.Read();
                                cMAirWayBill.ReferenceField5 = xmlTextReader.Value;
                                break;
                            case XmlConstants.AirlineOwnUse20AN:
                                xmlTextReader.Read();
                                cMAirWayBill.AirlineOwnUse = xmlTextReader.Value;
                                break;
                            case XmlConstants.ProrateLadderBreakdown:
                                CMAWBProrateLadder cMAWBProrateLadder = new CMAWBProrateLadder { SequenceNumber = ++proRateLadderCount };
                                ReadCMAWBProrateLadder(xmlTextReader, cMAWBProrateLadder, cMAirWayBill);
                                cMAirWayBill.CMAWBProrateLadderList.Add(cMAWBProrateLadder);
                                break;
                            case XmlConstants.Attachment:
                                ReadAttachmentDetails(xmlTextReader, cMAirWayBill);
                                break;
                        }
                    }
                }
                AssignChargeAmounts(cMAirWayBill, chargeAmountsDetails);
                AssignVatDetails(cMAirWayBill, vatArrayList, vatAmountsDetails, otherChargesArrayList);
                AssignAddOnCharges(cMAirWayBill, addOnChargeAmountsDetails);
                cMAirWayBill.CreditedOtherCharge = ReadOtherChargeDetails(cMAirWayBill, otherChargesArrayList, cMAirWayBill.CreditedOtherCharge);

                //Logger.Info("End of ReadCMAirWayBill.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadCMAirWayBill", xmlException);
                _staticLogger?.LogError("Error Occurred in ReadCMAirWayBill {0}", xmlException);
                throw;
            }
        }

        #endregion

        #region Read CMAWBProrateLadder

        /// <summary>
        /// To Read CMAWBProrateLadder from XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="cMAWBProrateLadder">CMAWBProrateLadder</param>
        /// <param name="cMAirWayBill">CMAirWayBill</param>
        private static void ReadCMAWBProrateLadder(XmlTextReader xmlTextReader, CMAWBProrateLadder cMAWBProrateLadder, CMAirWayBill cMAirWayBill)
        {
            try
            {
                //Logger.Info("Start of ReadCMAWBProrateLadder.");

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.ProrateLadderBreakdown)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.CurrencyOfProrateCalculation:
                                xmlTextReader.Read();
                                cMAWBProrateLadder.CurrencyofProrateCalculation = xmlTextReader.Value;
                                break;
                            case XmlConstants.TotalAmount:
                                xmlTextReader.Read();
                                cMAirWayBill.TotalProrateAmount = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.FromSector:
                                xmlTextReader.Read();
                                cMAWBProrateLadder.FromSector = xmlTextReader.Value;
                                break;
                            case XmlConstants.ToSector:
                                xmlTextReader.Read();
                                cMAWBProrateLadder.ToSector = xmlTextReader.Value;
                                break;
                            case XmlConstants.CarrierPrefix:
                                xmlTextReader.Read();
                                cMAWBProrateLadder.CarrierPrefix = xmlTextReader.Value;
                                break;
                            case XmlConstants.ProvisoReqSPAFlag:
                                xmlTextReader.Read();
                                cMAWBProrateLadder.ProvisoReqSpa = xmlTextReader.Value;
                                break;
                            case XmlConstants.ProrateFactor:
                                xmlTextReader.Read();
                                cMAWBProrateLadder.ProrateFactor = Convert.ToInt64(xmlTextReader.Value);
                                break;
                            case XmlConstants.PercentShare:
                                xmlTextReader.Read();
                                cMAWBProrateLadder.PercentShare = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.Amount:
                                xmlTextReader.Read();
                                cMAWBProrateLadder.TotalAmount = Convert.ToDouble(xmlTextReader.Value);
                                break;
                        }
                    }
                }
                //Logger.Info("End of ReadCMAWBProrateLadder.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadCMAWBProrateLadder: ", xmlException);
                _staticLogger?.LogError("Error Occurred in ReadCMAWBProrateLadder: {0}", xmlException);
                throw;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Read Invoice Summary

        /// <summary>
        /// To Read Invoice Summary from XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="invoice">Invoice</param>
        /// <param name="lineItemDetailCount">lineItemDetailCount</param>
        private static void ReadInvoiceSummary(XmlTextReader xmlTextReader, Invoice invoice, int lineItemDetailCount)
        {
            try
            {
                //Logger.Info("Start of ReadInvoiceSummary.");

                var invoiceTotalRecord = new InvoiceTotal
                {
                    TotalNumberOfBillingRecords = invoice.BillingCodeSubTotalList.Sum(i => i.NumberOfBillingRecords),
                    TotalNumberOfRecords = lineItemDetailCount
                };
                bool isFirstLegalText = false;
                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.InvoiceSummary)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            //case ModelConstants.TotalLineItemAmount:
                            //    xmlTextReader.Read();
                            //    invoiceTotalRecord.TotalLineItemAmount = Convert.ToDecimal(xmlTextReader.Value);
                            //    break;
                            case XmlConstants.TaxBreakdown:
                                InvoiceTotalVAT invoiceTotalVAT = new InvoiceTotalVAT();
                                ReadInvoiceTotalVAT(xmlTextReader, invoiceTotalVAT);
                                invoice.InvoiceTotalVATList.Add(invoiceTotalVAT);
                                break;
                            case XmlConstants.AddOnCharges:
                                ReadInvoiceSummaryAddonCharges(xmlTextReader, invoiceTotalRecord);
                                break;
                            case XmlConstants.TotalVATAmount:
                                xmlTextReader.Read();
                                invoiceTotalRecord.TotalVATAmount = Convert.ToDecimal(xmlTextReader.Value);
                                break;
                            case XmlConstants.TotalAmountWithoutVAT:
                                xmlTextReader.Read();
                                invoiceTotalRecord.TotalNetAmountWithoutVat = Convert.ToDecimal(xmlTextReader.Value);
                                break;
                            case XmlConstants.TotalAmount:
                                xmlTextReader.Read();
                                invoiceTotalRecord.NetInvoiceTotal = Convert.ToDecimal(xmlTextReader.Value);
                                break;
                            case XmlConstants.TotalAmountInClearanceCurrency:
                                xmlTextReader.Read();
                                invoiceTotalRecord.NetInvoiceBillingTotal = Convert.ToDecimal(xmlTextReader.Value);
                                break;
                            case XmlConstants.LegalText:
                                xmlTextReader.Read();
                                if (!isFirstLegalText)
                                {
                                    isFirstLegalText = true;
                                    invoice.InvoiceFooterDetails = string.Empty;
                                }
                                invoice.InvoiceFooterDetails += xmlTextReader.Value.Replace(XmlConstants.NewLine, " ");
                                break;
                        }
                    }
                }

                invoice.InvoiceTotals = invoiceTotalRecord;

                //Logger.Info("End of ReadInvoiceSummary.");
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in ReadInvoiceSummary", xmlException);
                _staticLogger?.LogError("Error Occurred in ReadInvoiceSummary {0}", xmlException);
                throw;
            }
        }

        #endregion

    }
}
