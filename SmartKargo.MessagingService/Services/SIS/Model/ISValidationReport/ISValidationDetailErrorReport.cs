using QidWorkerRole.SIS.Model.Base;
using System;

namespace QidWorkerRole.SIS.Model.ISValidationReport
{
    public class ISValidationDetailErrorReport : ModelBase
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
        /// The Billing Category, 1 Alphabet. e.g. C – Miscellaneous
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
        /// Cargo Billing Code, 25 Alphanumeric
        /// </summary>
        public string CGOBillingCode { get; set; }

        /// <summary>
        /// CGO(Blank), 25 Alphanumeric
        /// </summary>
        public string CGOBlank { get; set; }

        /// <summary>
        /// Cargo Batch Number, 5 Numeric
        /// </summary>
        public int CGOBatchNumber { get; set; }

        /// <summary>
        /// Cargo Sequence Number, 5 Numeric
        /// </summary>
        public int CGOSeqNumber { get; set; }

        /// <summary>
        /// Main Document Number, 20 Alphanumeric
        /// </summary>
        public string MainDocNo { get; set; }

        /// <summary>
        /// Linked Documnet Number, 20 Alphanumeric
        /// </summary>
        public string LinkedDocNo { get; set; }

        /// <summary>
        /// The Error Code, 20 Alphanumeric
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Level at which error is occurred, 50 Alphanumeric. e.g.File, Invoice, Airwaybill, etc.
        /// </summary>
        public string ErrorLevel { get; set; }

        /// <summary>
        /// Then name of the field in Error, 50 Alphanumeric
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// The value of the field in Error, 100 Alphanumeric
        /// </summary>
        public string FieldValue { get; set; }

        /// <summary>
        /// Error Description, 200 Alphanumeric
        /// </summary>
        public string ErrorDescription { get; set; }

        /// <summary>
        /// Error Status, 1 Alphabet
        /// Valid values are: Z - Sanity Check Error
        ///                   X - Error – Non Correctable
        ///                   C - Error – Correctable
        ///                   W - Warning
        /// </summary>
        public char ErrorStatus { get; set; }

        /// <summary>
        /// Validation report for AWBID
        /// </summary>
        public int ValidationForAWBID { get; set; }

        /// <summary>
        /// Validation report for AWBID from table
        /// </summary>
        public string ValidationForAWBIDFromTable { get; set; }
    }
}