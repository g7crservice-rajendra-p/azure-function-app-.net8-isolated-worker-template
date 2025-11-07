using System;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class InvoiceHeader : ModelBase
    {
        /// <summary>
        /// File Header Id.
        /// </summary>
        public int InvoiceHeaderID { get; set; }

        /// <summary>
        /// File Header Id [Primary Key of FileHeader].
        /// </summary>
        public int FileHeaderID { get; set; }

        /// <summary>
        /// Airline Numeric Code, In case of Alphanumeric carrier accounting codes,
        /// the alphabetic character (which is usually the first character of the accounting code) will be translated into numeric as follows:
        /// A = 10, B = 11, C = 12,… S=28, T=29, U=30… Z=35
        /// So if the Airline accounting code = A31 it will be represented as 1031 (applicable for all Billing/Billed Airline fields)
        /// </summary>
        public string BillingAirline { get; set; }

        /// <summary>
        /// Airline Numeric Code, In case of Alphanumeric carrier accounting codes,
        /// the alphabetic character (which is usually the first character of the accounting code) will be translated into numeric as follows:
        /// A = 10, B = 11, C = 12,… S=28, T=29, U=30… Z=35
        /// So if the Airline accounting code = A31 it will be represented as 1031 (applicable for all Billing/Billed Airline fields)
        /// </summary>
        public string BilledAirline { get; set; }

        /// <summary>
        /// Not be duplicated by the billing airline within a calendar year. 
        /// (Invoice number is case insensitive. Hence invoice numbers ABC12345 and abc12345 are considered to be the same)
        /// Can have only alphabets or numbers ((A-Z), (a-z), (0-9)).
        /// No special characters like hyphen, dot, slash, space, etc is allowed.
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Valid Billing Year from SIS Calender.
        /// </summary>
        public int BillingYear { get; set; }

        /// <summary>
        /// Valid Billing Month from SIS Calender.
        /// </summary>
        public int BillingMonth { get; set; }

        /// <summary>
        /// Valid Billing Period from SIS Calender.
        /// </summary>
        public int PeriodNumber { get; set; }

        /// <summary>
        /// Vaild ISO Alpha Currency Code.
        /// </summary>
        public string CurrencyofListing { get; set; }

        /// <summary>
        /// Vaild ISO Alpha Currency Code.
        /// </summary>
        public string CurrencyofBilling { get; set; }

        /// <summary>
        /// One of the below:
        /// I - ICH
        /// A - ACH Billings
        /// M - ACH Inter-clearance Billings or ACH Billings following RAM rules
        /// B - Bilateral Settlement
        /// R - Adjustments due to Protest
        /// P - Proforma Invoice
        /// X - ICH Multiple Agreements
        /// </summary>
        public string SettlementMethodIndicator { get; set; }

        /// <summary>
        ///  Y - Yes,  N - No,  D - As defined in the Airline Profile in SIS.
        /// </summary>
        public string DigitalSignatureFlag { get; set; }

        /// <summary>
        /// It should not be greater than the Current Billing Period closure date of SIS Calender.
        /// </summary>
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// It will be 1, when - Currency of Listing and Currency of Billing are same.
        /// If Currency of Listing and Currency of Billing are different and
        /// Settlement Method Indicator is I, A or M then exchange rate should be as published in the Five Day Rates Master for the Billing Month.
        /// </summary>
        public decimal? ListingToBillingRate { get; set; }

        /// <summary>
        /// It will be populated with “Y” in case the billed or billing airline is suspended from the Clearing House in SIS Profile.
        /// </summary>
        public string SuspendedInvoiceFlag { get; set; }

        /// <summary>
        /// The ID should exist in the Airline profile of the Billing Airline in SIS.
        /// </summary>
        public string BillingAirlineLocationID { get; set; }

        /// <summary>
        /// The ID should exist in the Airline profile of the Billing Airline in SIS.
        /// </summary>
        public string BilledAirlineLocationID { get; set; }

        /// <summary>
        /// IV: Invoice, Net Invoice total amount in the Invoice Total Record should be positive.
        /// CN: Credit Note, Net Invoice total amount in the Invoice Total Record should be negative.
        /// </summary>
        public string InvoiceType { get; set; }

        /// <summary>
        /// Valid 2 character language code as defined in ISO 3166-1 and a valid language supported by SIS.
        /// Accepted values "EN", "ES" and "FR".
        /// </summary>
        public string InvoiceTemplateLanguage { get; set; }

        /// <summary>
        /// All blanks if no date needs to be provided.
        /// Can be optionally provided only when Settlement Method is I, A, M or X.
        /// Not be provided for Bilateral Settlement Methods.
        /// Format is YYMMDD.
        /// </summary>
        public string ChDueDate { get; set; }

        /// <summary>
        /// Non-blank and valid value when Settlement Method is X.
        /// Optionally provided when Settlement Method is I, A or M.
        /// Should not be provided for Bilateral Settlement Methods
        /// </summary>
        public string ChAgreementIndicator { get; set; }

        /// <summary>
        /// Maximum 700 A/N are allowed. (It is also called Legal Text.)
        /// </summary>
        public string InvoiceFooterDetails { get; set; }

        /// <summary>
        /// To represent the current invoice status.
        /// 0: Open, 1: Closed, 3: Submitted.
        /// </summary>
        public int? InvoiceStatusId { get; set; }

        /// <summary>
        /// To idendify that it is received by file.
        /// 0: Not received from file
        /// 1: Received from file
        /// </summary>
        public bool IsReceivedFromFile { get; set; }

        /// <summary>
        /// To Identify if Airline is SIS or Non SIS.
        /// 0: Non SIS
        /// 1: SIS
        /// </summary>
        public bool IsSIS { get; set; }

    }
}