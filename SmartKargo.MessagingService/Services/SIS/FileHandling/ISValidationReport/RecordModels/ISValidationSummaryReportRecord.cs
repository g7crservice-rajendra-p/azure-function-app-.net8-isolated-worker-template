using FileHelpers;

namespace QidWorkerRole.SIS.FileHandling.ISValidationReport.RecordModels
{
    [DelimitedRecord(","), IgnoreFirst(1)]
    public class ISValidationSummaryReportRecord
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
        public string CurrencyOfBilling { get; set; }

        [FieldOrder(12)]
        public string InvoiceAmountInBillingCurrency { get; set; }

        [FieldOrder(13)]
        public string InvoiceStatus { get; set; }

        [FieldOrder(14)]
        public string ErrorAtInvoiceLevel { get; set; }

        [FieldOrder(15)]
        public string TotalNoOfBillingRecords { get; set; }

        [FieldOrder(16)]
        public string TotalNoOfSuccessfullyValidatedRecords { get; set; }

        [FieldOrder(17)]
        public string TotalNoOfRecordsInValidationError { get; set; }

        [FieldOrder(18)]
        public string ExtraColumn { get; set; }
    }
}