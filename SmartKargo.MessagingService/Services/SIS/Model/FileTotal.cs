using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    /// <summary>
    /// This class represents File Total Record
    /// </summary>
    public class FileTotal : ModelBase
    {
        /// <summary>
        /// File Total Id.
        /// </summary>
        public int FileTotalID { get; set; }

        /// <summary>
        /// File Header Id [Primary Key of FileHeader].
        /// </summary>
        public int? FileHeaderID { get; set; }

        /// <summary>
        /// Sum of the Total Weight Charges from InvoiceTotal. Ignore signs.
        /// </summary>
        public decimal TotalWeightCharges { get; set; }

        /// <summary>
        /// Sum of the Total Other Charges from InvoiceTotal. Ignore signs.
        /// </summary>
        public decimal TotalOtherCharges { get; set; }

        /// <summary>
        /// Sum of the Total ISC Amount from InvoiceTotal. Ignore signs.
        /// </summary>
        public decimal TotalInterlineServiceChargeAmount { get; set; }

        /// <summary>
        /// Sum of the Net Invoice Total Amount from InvoiceTotal. Ignore signs.
        /// </summary>
        public decimal FileTotalOfNetInvoiceTotal { get; set; }

        /// <summary>
        /// Sum of the Net Invoice Billing Total Amount from InvoiceTotal. Ignore signs.
        /// </summary>
        public decimal FileTotalOfNetInvoiceBillingTotal { get; set; }

        /// <summary>
        /// Sum of the Total Number of AWB Records from all InvoiceTotal.
        /// </summary>
        public decimal? TotalNumberOfBillingRecords { get; set; }

        /// <summary>
        /// Sum of the Total Valuation Charges from InvoiceTotal. Ignore signs.
        /// </summary>
        public decimal TotalValuationCharges { get; set; }

        /// <summary>
        /// Sum of the Total VAT Amount from InvoiceTotal. Ignore signs.
        /// </summary>
        public decimal TotalVatAmount { get; set; }

        /// <summary>
        /// Sum of all Total Number of Records count of the Invoice Total record including the File Header and File Total record.
        /// </summary>
        public decimal? TotalNumberOfRecords { get; set; }


        /// <summary>
        /// Batch Sequence Number.
        /// </summary>
        public int BatchSequenceNumber { get; set; }

        /// <summary>
        /// Record Sequence within Batch.
        /// </summary>
        public int RecordSequenceWithinBatch { get; set; }

        /// <summary>
        /// Billed Airline.
        /// </summary>
        public string BillingAirline { get; set; }
    }
}
