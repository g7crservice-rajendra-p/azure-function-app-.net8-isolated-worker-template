namespace QidWorkerRole.SIS.Model.Base
{
    public abstract class MemoBase : ModelBase
    {
        public int BatchSequenceNumber { get; set; }

        public int RecordSequenceWithinBatch { get; set; }

        public string YourInvoiceNumber { get; set; }

        public int YourInvoiceBillingYear { get; set; }

        public int YourInvoiceBillingMonth { get; set; }

        public int YourInvoiceBillingPeriod { get; set; }

        public string BillingCode { get; set; }

        public bool AttachmentIndicatorOriginal { get; set; }

        public bool? AttachmentIndicatorValidated { get; set; }

        public int? NumberOfAttachments { get; set; }

        public string AirlineOwnUse { get; set; }

        public string ISValidationFlag { get; set; }

        public string ReasonRemarks { get; set; }

        /// <summary>
        /// Our reference 
        /// </summary>
        public string OurRef { get; set; }

        /// <summary>
        /// Number of child records required in case of IDEC validations.
        /// </summary>
        public long NumberOfChildRecords { get; set; }
    }
}
