using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base
{
    public abstract class MemoRecordBase : InvoiceRecordBase
    {
        [FieldFixedLength(5), FieldConverter(typeof(PaddingConverter), 5, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string BatchSequenceNumber = string.Empty;

        [FieldFixedLength(5), FieldConverter(typeof(PaddingConverter), 5, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string RecordSequenceWithinBatch = string.Empty;

        [FieldFixedLength(11)]
        public string BillingOrCreditMemoNumber = string.Empty;

        [FieldFixedLength(2)]
        public string ReasonCode = string.Empty;

        [FieldFixedLength(20)]
        public string OurRef = string.Empty;

        [FieldFixedLength(11), FieldConverter(typeof(PaddingConverter), 11, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string CorrespondenceRefNumber = string.Empty;

        [FieldFixedLength(10)]
        public string YourInvoiceNumber = string.Empty;

        [FieldFixedLength(4)]
        public string Filler2 = string.Empty;

        [FieldFixedLength(6), FieldConverter(typeof(PaddingConverter), 6, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string YourInvoiceBillingDate = string.Empty;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalWeightChargesAmountBilledOrCredited = string.Empty;

        [FieldFixedLength(1)]
        public string TotalWeightChargesAmountBilledOrCreditedSign = string.Empty;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalValuationAmountBilledOrCredited = string.Empty;

        [FieldFixedLength(1)]
        public string TotalValuationAmountBilledOrCreditedSign = string.Empty;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalOtherChargesAmountBilledOrCredited = string.Empty;

        [FieldFixedLength(1)]
        public string TotalOtherChargesAmountBilledOrCreditedSign = string.Empty;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalISCAmountBilledOrCredited = string.Empty;

        [FieldFixedLength(1)]
        public string TotalISCAmountBilledOrCreditedSign = string.Empty;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalVATAmountBilledOrCredited = string.Empty;

        [FieldFixedLength(1)]
        public string TotalVATAmountBilledOrCreditedSign = string.Empty;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string NetAmountBilledOrCredited = string.Empty;

        [FieldFixedLength(1)]
        public string NetAmountBilledOrCreditedSign = string.Empty;

        [FieldFixedLength(1)]
        public string AttachmentIndicatorOriginal = string.Empty;

        [FieldFixedLength(1)]
        public string AttachmentIndicatorValidated = string.Empty;

        [FieldFixedLength(4), FieldConverter(typeof(PaddingConverter), 4, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string NumberOfAttachments = "0";

        [FieldFixedLength(20)]
        public string AirlineOwnUse = string.Empty;

        [FieldFixedLength(10)]
        public string ISValidationFlag = string.Empty;

        [FieldFixedLength(258)]
        public string Filler3 = string.Empty;

        public MemoRecordBase() { }

        /// <summary>
        /// To set common properties
        /// </summary>
        /// <param name="invoiceRecordBase"></param>
        public MemoRecordBase(InvoiceRecordBase invoiceRecordBase)
            : base(invoiceRecordBase)
        { }
    }
}