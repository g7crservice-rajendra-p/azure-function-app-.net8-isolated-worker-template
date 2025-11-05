using System;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model.ISValidationReport
{
    public class ISValidationSummaryReport : ModelBase
    {
        public int ID { get; set; }

        /// <summary>
        /// Serial Number
        /// </summary>
        public int SerialNo { get; set; }

        /// <summary>
        /// The numeric airline code or Alphanumeric Supplier Code of the Billing Entity, 4 Alphanumeric
        /// </summary>
        public string BillingEntityCode { get; set; }

        /// <summary>
        /// Billing Year
        /// </summary>
        public int BillingYear { get; set; }

        /// <summary>
        /// Billing Month
        /// </summary>
        public int BillingMonth { get; set; }

        /// <summary>
        /// Billing Period
        /// </summary>
        public int BillingPeriod { get; set; }

        /// <summary>
        /// The Billing Category, 1 Alphabet. e.g. C – Cargo
        /// </summary>
        public char BillingCategory { get; set; }

        /// <summary>
        /// The File Name with extension, 50 Alphanumeric
        /// </summary>
        public string BillingFileName { get; set; }

        /// <summary>
        /// The date of file submission
        /// </summary>
        public DateTime BillingFileSubmissionDate { get; set; }

        /// <summary>
        /// Format of the Billing File, 10 Alphanumeric. e.g. IS-XML, IS-IDEC
        /// </summary>
        public string SubmissionFormat { get; set; }

        /// <summary>
        /// The numeric airline code or Alphanumeric Supplier Code of the Billed Entity, 4 Alphanumeric
        /// </summary>
        public string BilledEntityCode { get; set; }

        /// <summary>
        /// The Invoice Number, 10 Alphanumeric
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// The Billing Currency Code, 3 Alphanumeric
        /// </summary>
        public string CurrencyOfBilling { get; set; }

        /// <summary>
        /// The Billing Currency Amount, Numeric(13,3)
        /// </summary>
        public decimal InvoiceAmountInBillingCurrency { get; set; }

        /// <summary>
        /// Invoice Status, 1 Alphabet
        /// Valid values are: Z - Sanity Check Error
        ///                   X - Error – Non Correctable
        ///                   C - Error – Correctable
        ///                   W - Warning
        /// </summary>
        public char InvoiceStatus { get; set; }

        /// <summary>
        /// Error at Invoice Level, 1 Alphabet. e.g. Y,N
        /// </summary>
        public char ErrorAtInvoiceLevel { get; set; }

        /// <summary>
        /// Total Number of Billing Records, 8 Numeric
        /// </summary>
        public int TotalNoOfBillingRecords { get; set; }

        /// <summary>
        /// Total Number of billing records successfully validated, 8 Numeric
        /// </summary>
        public int TotalNoOfSuccessfullyValidatedRecords { get; set; }

        /// <summary>
        /// Total Number of billing records in Error, 8 Numeric
        /// </summary>
        public int TotalNoOfRecordsInValidationError { get; set; }

        /// <summary>
        /// Validation report for Invoice ID from SK SIS Invoice Header table
        /// </summary>
        public int ValidationForInvoiceID { get; set; }
    }
}