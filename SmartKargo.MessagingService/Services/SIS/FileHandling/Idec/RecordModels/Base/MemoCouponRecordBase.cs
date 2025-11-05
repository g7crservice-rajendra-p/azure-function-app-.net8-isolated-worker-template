using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base
{
    public abstract class MemoCouponRecordBase : InvoiceRecordBase
    {
        [FieldFixedLength(11)]
        public string BillingCreditMemoNumber = string.Empty;

        [FieldFixedLength(5), FieldConverter(typeof(PaddingConverter), 5, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string BreakdownSerialNumber = string.Empty;

        [FieldFixedLength(6), FieldConverter(typeof(DateFormatConverter))]
        public string AWBIssueDate = string.Empty;

        [FieldFixedLength(4), FieldConverter(typeof(PaddingConverter), 4, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string AWBIssuingAirline = string.Empty;

        [FieldFixedLength(7), FieldConverter(typeof(PaddingConverter), 7, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string AWBSerialNumber = string.Empty;

        [FieldFixedLength(1)]
        public string CheckDigit = string.Empty;

        [FieldFixedLength(4)]
        public string Origin = string.Empty;

        [FieldFixedLength(4)]
        public string Destination = string.Empty;

        [FieldFixedLength(4)]
        public string FromAirport = string.Empty;

        [FieldFixedLength(4)]
        public string ToAirport = string.Empty;

        [FieldFixedLength(6), FieldConverter(typeof(DateFormatConverter))]
        public string DateOfCarriage;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string WeightChargesBilledOrCredited = string.Empty;

        [FieldFixedLength(1)]
        public string WeightChargesBilledOrCreditedSign = string.Empty;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string ValuationChargesBilledOrCredited = string.Empty;

        [FieldFixedLength(1)]
        public string ValuationChargesBilledOrCreditedSign = string.Empty;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string OtherChargesBilledOrCredited = string.Empty;

        [FieldFixedLength(1)]
        public string OtherChargesBilledOrCreditedSign = string.Empty;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string AmountSubjectedToIscBilledOrCredited = string.Empty;

        [FieldFixedLength(1)]
        public string AmountSubjectedToIscBilledOrCreditedSign = string.Empty;

        [FieldFixedLength(5), FieldConverter(typeof(DoubleNumberConverter), 5, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string IscPercentBilledOrCredited = string.Empty;

        [FieldFixedLength(1)]
        public string IscPercentBilledOrCreditedSign = string.Empty;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string IscAmountBilledOrCredited = string.Empty;

        [FieldFixedLength(1)]
        public string IscAmountBilledOrCreditedSign = string.Empty;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatAmountBilledOrCredited = string.Empty;

        [FieldFixedLength(1)]
        public string VatAmountBilledOrCreditedSign = string.Empty;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalAmountBilledOrCredited = string.Empty;

        [FieldFixedLength(1)]
        public string TotalAmountBilledOrCreditedSign = string.Empty;

        [FieldFixedLength(3)]
        public string CurrencyAdjustmentIndicator;

        [FieldFixedLength(6), FieldConverter(typeof(PaddingConverter), 6, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string BilledWeight;

        [FieldFixedLength(1)]
        public string ProvisoOrReqOrSpa;

        [FieldFixedLength(2), FieldConverter(typeof(PaddingConverter), 2, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string ProratePercent;

        [FieldFixedLength(1)]
        public string PartShipmentIndicator;

        [FieldFixedLength(1)]
        public string KgOrLbIndicator;

        [FieldFixedLength(1)]
        public string CCAIndicator;

        [FieldFixedLength(1)]
        public string AttachmentIndicatorOriginal = string.Empty;

        [FieldFixedLength(1)]
        public string AttachmentIndicatorValidated = string.Empty;

        [FieldFixedLength(4), FieldConverter(typeof(PaddingConverter), 4, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string NumberOfAttachments = "0";

        [FieldFixedLength(10)]
        public string ISValidationFlag = string.Empty;

        [FieldFixedLength(2)]
        public string ReasonCode = string.Empty;

        [FieldFixedLength(10)]
        public string ReferenceField1 = string.Empty;

        [FieldFixedLength(10)]
        public string ReferenceField2 = string.Empty;

        [FieldFixedLength(10)]
        public string ReferenceField3 = string.Empty;

        [FieldFixedLength(10)]
        public string ReferenceField4 = string.Empty;

        [FieldFixedLength(20)]
        public string ReferenceField5 = string.Empty;

        [FieldFixedLength(20)]
        public string AirlineOwnUse = string.Empty;

        [FieldFixedLength(205)]
        [FieldTrim(TrimMode.Right)]
        public string Filler3 = string.Empty;

        public MemoCouponRecordBase() { }

        /// <summary>
        /// To set common properties
        /// </summary>
        /// <param name="baseRecord"></param>
        public MemoCouponRecordBase(InvoiceRecordBase baseRecord) : base(baseRecord) { }
    }
}
