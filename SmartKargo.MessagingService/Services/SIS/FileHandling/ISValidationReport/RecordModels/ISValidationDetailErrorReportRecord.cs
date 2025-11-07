using FileHelpers;

namespace QidWorkerRole.SIS.FileHandling.ISValidationReport.RecordModels
{
    [DelimitedRecord(","), IgnoreFirst(1)]
    public class ISValidationDetailErrorReportRecord
    {
        [FieldOrder(1)]
        public string SerialNo { get; set; }

        [FieldOrder(2)]
        public string BillingEntityCode { get; set; }

        [FieldOrder(3)]
        public string ClearanceMonth { get; set; }

        [FieldOrder(4)]
        public string PeriodNumber { get; set; }

        [FieldOrder(5)]
        public string BillingCategory { get; set; }

        [FieldOrder(6)]
        public string BillingFileName { get; set; }

        [FieldOrder(7)]
        public string BillingFileSubmissionDate { get; set; }

        [FieldOrder(8)]
        public string SubmissionFormat { get; set; }

        [FieldOrder(9)]
        public string BilledEntityCode { get; set; }

        [FieldOrder(10)]
        public string InvoiceNumber { get; set; }

        [FieldOrder(11)]
        public string CGOBillingCode { get; set; }

        [FieldOrder(12)]
        public string CGOBlank { get; set; }

        [FieldOrder(13)]
        public string CGOBatchNumber { get; set; }

        [FieldOrder(14)]
        public string CGOSeqNumber { get; set; }

        [FieldOrder(15)]
        public string MainDocNo { get; set; }

        [FieldOrder(16)]
        public string LinkedDocNo { get; set; }

        [FieldOrder(17)]
        public string ErrorCode { get; set; }

        [FieldOrder(18)]
        public string ErrorLevel { get; set; }

        [FieldOrder(19)]
        public string FieldName { get; set; }

        [FieldOrder(20)]
        public string FieldValue { get; set; }

        [FieldOrder(21)]
        public string ErrorDescription { get; set; }

        [FieldOrder(22)]
        public string ErrorStatus { get; set; }
    }
}