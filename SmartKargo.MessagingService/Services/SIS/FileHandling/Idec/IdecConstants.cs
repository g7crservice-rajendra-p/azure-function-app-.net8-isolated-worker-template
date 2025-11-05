namespace QidWorkerRole.SIS.FileHandling.Idec
{
    internal class IdecConstants
    {
        internal const string StandardMessageIdentifier = "CBD";
        internal const string SfiFileHeader = "00";
        internal const string SfiInvoiceHeader = "15";
        internal const string SfiReferenceData1 = "16";
        internal const string SfiReferenceData2 = "17";
        internal const string SfiAwbRecord = "25";
        internal const string SfiRMAwbRecord = "26";
        internal const string SfiBMAwbRecord = "27";
        internal const string SfiCMAwbRecord = "27";
        internal const string SfiVatBreakdownRecord = "28";
        internal const string SfiVatRecord = "28";
        internal const string SfiOCBreakdownRecord = "29";
        internal const string SfiOcRecord = "29";
        internal const string SfiSourceCodeTotalRecord = "30";
        internal const string SfiRejectionMemoRecord = "35";
        internal const string SfiReasonBreakdownRecord = "36";
        internal const string SfiProrateLadderRecord = "37";
        internal const string SfiBillingCodeSubTotal = "45";
        internal const string SfiBillingMemoRecord = "55";
        internal const string SfiCreditMemoRecord = "56";
        internal const string SfiInvoiceTotalRecord = "65";
        internal const string SfiInvoiceFooterRecord = "66";
        internal const string SfiFileTotalRecord = "75";

        internal const string EmbeddedCargoIdecResourcTypeFileName = "QidWorkerRole.SIS.FileHandling.Idec.ResourceFiles.IdecFileResourcType.xml";
        internal const string IdecResourcTypeFileNameParentNode = "IS/idecFileResourceTypes";
        internal const string InvoiceDateFormat = "yyMMdd";
        internal const string BillingDateFormat = "yyMM";
        internal const int RecordLength = 500;

        internal const int ReferenceDataRecordSerialNumberIndex = 36;
        internal const int RecordSequeceNumberIndex = 03;

        internal const string TrueValue = "Y";
        internal const string FalseValue = "N";
        internal const string AmountPlusSign = "P";
        internal const string AmountNegativeSign = "M";
        public const int RJZF_PaddingConverter = 0;
        internal const int ReferenceData1RecordSerialNo = 1;
        internal const int ReferenceData2RecordSerialNo = 2;
        internal const char PaddingCharacterZero = '0';
        internal const string DateFormat = "yyMMdd";
        internal const string OrganizationNameSeparator = "!!!";

        public const string SourceCodeTotalBatchSequenceNumber = "99999";
        public const string SourceCodeTotalRecordSequencewithinBatch = "99999";
        public const string InvoiceTotalRecordBatchSequenceNumber = "99999";
        public const string InvoiceTotalRecordSequencewithinBatch = "99999";
        public const string FileTotalRecordBatchSequenceNumber = "99999";
        public const string FileTotalRecordSequencewithinBatch = "99999";
        public const string FileTotalRecordBilledAirlineCode = "9999";
        public const string FileTotalRecordBillingCode = "9";
        public const string FileTotalRecordInvoiceNumber = "9999999999";
        public const string FileTotalRecordFiller = "9999";
        internal const string BillingCodeTotalFiller = "999999999999999";
        public const string FileHeaderRecordVersionNo = "0320";

        // Read
        internal const int BillingMemberIndex = 13;
        internal const int BilledMemberIndex = 17;
        internal const int BillingCodeIndex = 21;
        internal const int SfiStartIndex = 11;

    }
}