namespace QidWorkerRole.SIS.FileHandling.Xml
{
    /// <summary>
    /// Constants used to identify XML nodes.
    /// </summary>
    public class XmlConstants
    {
        #region XML File Constants

        #region File Header

        internal const string Prefix = "";
        internal const string InvoiceTransmission = "InvoiceTransmission";
        internal const string XsiLocalName = "xsi";
        internal const string Colon = ":";
        internal const string SchemaLocationLocalName = "schemaLocation";
        internal const string SchemaLocation = "http://www.IATA.com/IATAAviationInvoiceStandard http://www.iata.org/services/finance/sis/Documents/schemas/IATA_IS_XML_Invoice_Standard_V3.6.xsd";
        internal const string XmlnsLocalName = "xmlns";
        internal const string IataInvoiceStandard = "http://www.IATA.com/IATAAviationInvoiceStandard";
        internal const string Xsi = "http://www.w3.org/2001/XMLSchema-instance";

        #endregion

        #region TransmissionHeader

        internal const string TransmissionHeader = "TransmissionHeader";
        internal const string TransmissionDateTime = "TransmissionDateTime";
        internal const string TransmissionDateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
        internal const string Version = "Version";
        internal const string VersionValue = "IATA:ISXMLInvoiceV3.6";
        internal const string TransmissionID = "TransmissionID";
        internal const string IssuingOrganizationID = "IssuingOrganizationID";
        internal const string ReceivingOrganizationID = "ReceivingOrganizationID"; 
        internal const string BillingCategory = "BillingCategory";

        #endregion

        #region Invoice

        #region InvoiceHeader

        internal const string Invoice = "Invoice";
        internal const string InvoiceHeader = "InvoiceHeader";
        internal const string InvoiceNumber = "InvoiceNumber";
        internal const string InvoiceDate = "InvoiceDate";
        internal const string InvoiceType = "InvoiceType";
        internal const string ChargeCategory = "ChargeCategory";
        internal const string InvoiceDateFormat = "yyMMdd";

        #region SellerOrganization / BuyerOrganization

        internal const string SellerOrganization = "SellerOrganization";
        internal const string BuyerOrganization = "BuyerOrganization";
        internal const string OrganizationID = "OrganizationID";
        internal const string OrganizationDesignator = "OrganizationDesignator";
        internal const string LocationID = "LocationID";
        internal const string OrganizationName1 = "OrganizationName1";
        internal const string OrganizationName2 = "OrganizationName2";
        internal const string TaxRegistrationID = "TaxRegistrationID";
        internal const string AdditionalTaxRegistrationId = "AdditionalTaxRegistrationID";
        internal const string CompanyRegistrationId = "CompanyRegistrationID";
        internal const string Address = "Address";
        internal const string AddressLine1 = "AddressLine1";
        internal const string AddressLine2 = "AddressLine2";
        internal const string AddressLine3 = "AddressLine3";
        internal const string CityName = "CityName";
        internal const string SubdivisionCode = "SubdivisionCode";
        internal const string SubdivisionName = "SubdivisionName";
        internal const string CountryCode = "CountryCode";
        internal const string CountryName = "CountryName";
        internal const string PostalCode = "PostalCode";

        #endregion

        #region PaymentTerms

        internal const string PaymentTerms = "PaymentTerms";
        internal const string CurrencyCode = "CurrencyCode";
        internal const string ClearanceCurrencyCode = "ClearanceCurrencyCode";
        internal const string ExchangeRate = "ExchangeRate";
        internal const string SettlementMonthPeriod = "SettlementMonthPeriod";
        internal const string SettlementMethod = "SettlementMethod";
        internal const string NetDueDate = "NetDueDate";
        internal const string ChAgreementIndicator = "CHAgreementIndicator";

        #endregion

        #region ISDetails
        
        internal const string ISDetails = "ISDetails";
        internal const string DigitalSignatureFlag = "DigitalSignatureFlag";
        internal const string SuspendedFlag = "SuspendedFlag";

        #endregion

        internal const string Language = "InvoiceTemplateLanguage";

        #endregion

        #region LineItem

        internal const string LineItem = "LineItem";
        internal const string LineItemNumber = "LineItemNumber";
        internal const string ChargeCode = "ChargeCode";
        internal const string Description = "Description";
        internal const string ChargeAmount = "ChargeAmount";
        internal const string Weight = "Weight";
        internal const string Valuation = "Valuation";

        internal const string Tax = "Tax";
        internal const string TaxType = "TaxType";
        internal const string VAT = "VAT";
        internal const string TaxAmount = "TaxAmount";
        internal const string Accepted = "Accepted";
        internal const string Difference = "Difference";

        internal const string TaxBreakdown = "TaxBreakdown";
        internal const string TaxLabel = "TaxLabel";
        internal const string TaxIdentifier = "TaxIdentifier";
        internal const string TaxableAmount = "TaxableAmount";
        internal const string TaxPercent = "TaxPercent";
        internal const string TaxText = "TaxText";
        internal const string TaxCode = "TaxCode";

        internal const string AddOnChargeCode = "AddOnChargeCode";
        internal const string AddOnCharges = "AddOnCharges";
        internal const string AddOnChargeName = "AddOnChargeName";

        internal const string OC = "OC";
        internal const string OtherChargesAllowed = "OtherChargesAllowed";
        internal const string OtherChargesAccepted = "OtherChargesAccepted";
        internal const string OtherChargesDifference = "OtherChargesDifference";
        internal const string AddOnChargePercentage = "AddOnChargePercentage";
        internal const string AddOnChargeAmount = "AddOnChargeAmount";

        internal const string IscAllowed = "ISCAllowed";
        internal const string IscAccepted = "ISCAccepted";
        internal const string IscDifference = "ISCDifference";

        internal const string AmountSubjectToISCAllowed = "AmountSubjectToISCAllowed";
        internal const string AmountSubjectToISCAccepted = "AmountSubjectToISCAccepted";
        internal const string AmountSubjectToISCDifference = "AmountSubjectToISCDifference";

        internal const string TotalNetAmount = "TotalNetAmount";
        internal const string DetailCount = "DetailCount";

        #endregion

        #region LineItemDetail

        internal const string LineItemDetail = "LineItemDetail";
        internal const string DetailNumber = "DetailNumber";
        internal const string BatchSequenceNumber = "BatchSequenceNumber";
        internal const string RecordSequenceWithinBatch = "RecordSequenceWithinBatch";
        internal const string WeightBilled = "WeightBilled";
        internal const string ValuationBilled = "ValuationBilled";
        internal const string WeightAccepted = "WeightAccepted";
        internal const string WeightDifference = "WeightDifference";
        internal const string ValuationAccepted = "ValuationAccepted";
        internal const string ValuationDifference = "ValuationDifference";
        internal const string Billed = "Billed";

        internal const string ReasonCode = "ReasonCode";
        internal const string ReasonDescription = "ReasonDescription";
        internal const string NewLine = "\n";
        internal const string OurRef = "OurRef";
        internal const string AirlineOwnUse20AN = "AirlineOwnUse20AN";
        internal const string IsValidationFlag = "ISValidationFlag";
        internal const string BreakdownSerialNumber = "BreakdownSerialNumber";
        internal const string YourInvoiceNumber = "YourInvoiceNumber";
        internal const string YourInvoiceBillingDate = "YourInvoiceBillingDate";
        internal const string CorrespondenceRefNumber = "CorrespondenceRefNumber";
        internal const string BillingDateFormat = "yyyy-MM-dd";

        #region AirWaybill / RMAirWaybill / BMAirWaybill / CMAirWaybill

        internal const string AirWaybillDetails = "AirWaybillDetails";
        internal const string AWBDate = "AWBDate";
        internal const string AWBIssuingAirline = "AWBIssuingAirline";
        internal const string AWBSerialNumber = "AWBSerialNumber";
        internal const string AWBCheckDigit = "AWBCheckDigit";
        internal const string OriginAirportCode = "OriginAirportCode";
        internal const string DestinationAirportCode = "DestinationAirportCode";
        internal const string FromAirportCode = "FromAirportCode";
        internal const string ToAirportOrPointOfTransferCode = "ToAirportOrPointOfTransferCode";
        internal const string DateOfCarriageOrTransfer = "DateOfCarriageOrTransfer";
        internal const string CurrAdjustmentIndicator = "CurrAdjustmentIndicator";
        internal const string BilledWeight = "BilledWeight";
        internal const string ProvisoReqSPA = "ProvisoReqSPA";
        internal const string ProratePercentage = "ProratePercentage";
        internal const string PartShipmentIndicator = "PartShipmentIndicator";
        internal const string FilingReference = "FilingReference";
        internal const string KgLbIndicator = "KgLbIndicator";
        internal const string CCAIndicator = "CCAIndicator";
        internal const string ReferenceField10AN = "ReferenceField10AN";
        internal const string AirWaybillBreakdown = "AirWaybillBreakdown";
        internal const string BillingCode = "BillingCode";
        internal const string ReferenceField20AN = "ReferenceField20AN";

        #endregion

        #region RejectionMemo

        internal const string RejectionMemoDetails = "RejectionMemoDetails";
        internal const string RejectionMemoNumber = "RejectionMemoNumber";
        internal const string RejectionStage = "RejectionStage";
        internal const string YourRejectionMemoNumber = "YourRejectionMemoNumber";
        internal const string FimBmCmIndicator = "FIMBMCMIndicator";
        internal const string LinkedFimBmCmNumber = "LinkedFIMBillingCreditMemoNumber";

        #endregion

        #region Billing Memo

        internal const string BillingMemoDetails = "BillingMemoDetails";
        internal const string BillingMemoNumber = "BillingMemoNumber"; 

        #endregion

        #region Credit Memo.

        internal const string CreditMemoDetails = "CreditMemoDetails";
        internal const string CreditMemoNumber = "CreditMemoNumber";

        #endregion

        #region Prorate Ladder

        internal const string ProrateLadderBreakdown = "ProrateLadderBreakdown";
        internal const string CurrencyOfProrateCalculation = "CurrencyOfProrateCalculation";
        internal const string FromSector = "FromSector";
        internal const string ToSector = "ToSector";
        internal const string CarrierPrefix = "CarrierPrefix";
        internal const string ProvisoReqSPAFlag = "ProvisoReqSPAFlag";
        internal const string ProrateFactor = "ProrateFactor";
        internal const string PercentShare = "PercentShare";
        internal const string Amount = "Amount";
        
        #endregion

        #region Attachment

        internal const string Attachment = "Attachment";
        internal const string AttachmentIndicatorOriginal = "AttachmentIndicatorOriginal";
        internal const string AttachmentIndicatorValidated = "AttachmentIndicatorValidated";
        internal const string NumberOfAttachments = "NumberOfAttachments";

        #endregion

        #endregion

        #region InvoiceSummary

        internal const string InvoiceSummary = "InvoiceSummary";
        internal const string LineItemCount = "LineItemCount";
        internal const string TotalLineItemAmount = "TotalLineItemAmount";
        internal const string TotalAmountWithoutVAT = "TotalAmountWithoutVAT";
        internal const string TotalAmountInClearanceCurrency = "TotalAmountInClearanceCurrency";
        internal const string LegalText = "LegalText";

        #endregion

        #endregion

        #region TransmissionSummary

        internal const string TransmissionSummary = "TransmissionSummary";
        internal const string InvoiceCount = "InvoiceCount";
        internal const string TotalAmount = "TotalAmount";
        internal const string TotalAddOnChargeAmount = "TotalAddOnChargeAmount";
        internal const string TotalVATAmount = "TotalVATAmount";
        internal const string TotalTaxAmount = "TotalTaxAmount";

        #endregion

        #endregion

        #region Other Constants

        internal const string Y = "Y";
        internal const string N = "N";        
        internal const string Name = "Name";

        #endregion

        #region C# Class Name Constants

        internal const string AirWayBill = "AirWayBill";        
        internal const string RejectionMemo = "RejectionMemo";
        internal const string RMAirWayBill = "RMAirWayBill";
        internal const string BillingMemo = "BillingMemo";
        internal const string BMAirWayBill = "BMAirWayBill";
        internal const string CreditMemo = "CreditMemo";
        internal const string CMAirWayBill = "CMAirWayBill";

        #endregion

    }
}