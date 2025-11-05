using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class InvoiceTotal : ModelBase
    {
        /// <summary>
        /// Invoice Total Id.
        /// </summary>
        public int InvoiceTotalID { get; set; }

        /// <summary>
        /// File Header Id [Primary Key of FileHeader].
        /// </summary>
        public int InvoiceHeaderID { get; set; }

        /// <summary>
        /// Sum of Total Weight Charges of all Sub Total records within the Invoice.
        /// Amount in Listing currency with three decimal places.
        /// </summary>
        public decimal TotalWeightCharges { get; set; }

        /// <summary>
        /// Sum of Total Other Charges of all Sub Total records within the Invoice.
        /// Amount in Listing currency with three decimal places.
        /// </summary>
        public decimal TotalOtherCharges { get; set; }

        /// <summary>
        /// Sum of Total Interline Service Charge Amount of all Sub Total records within the Invoice.
        /// Amount in Listing currency with three decimal places.
        /// </summary>
        public decimal TotalInterlineServiceChargeAmount { get; set; }

        /// <summary>
        /// Sum of Billing Code Sub Total of all Sub Total records within the Invoice.
        /// Amount in Listing currency with three decimal places.
        /// </summary>
        public decimal NetInvoiceTotal { get; set; }

        /// <summary>
        /// Net Invoice Total of Invoice Total / (Devided by) Listing to Billing Rate from Invoice Header rounded to three decimal places.
        /// Amount in Billing currency with three decimal places.
        /// </summary>
        public decimal NetInvoiceBillingTotal { get; set; }

        /// <summary>
        /// Sum of Total Number of Billing Records values of all Sub Total records within the Invoice.
        /// </summary>
        public int TotalNumberOfBillingRecords { get; set; }

        /// <summary>
        /// Sum of Total Valuation charges of all  Sub Total records within the Invoice.
        /// Amount in Listing currency with three decimal places.
        /// </summary>
        public decimal TotalValuationCharges { get; set; }

        /// <summary>
        /// Sum Total VAT Amount of all  Sub Total records within the Invoice.
        /// Amount in Listing currency with three decimal places.
        /// </summary>
        public decimal TotalVATAmount { get; set; }

        /// <summary>
        /// Sum of Total Number of Records of all Billing Code Sub Total record in the invoice,
        /// Including the breakdown records at an Invoice level and the Invoice Header.
        /// </summary>
        public int TotalNumberOfRecords { get; set; }

        /// <summary>
        /// Net Invoice Total of Invoice Total – (Minus) Total Vat Amount of Invoice Total.
        /// Amount in Currency of Listing with three decimal precision.
        /// </summary>
        public decimal TotalNetAmountWithoutVat { get; set; }

    }
}