using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base
{
    [FixedLengthRecord()]
    public abstract class InvoiceRecordBase : RecordBase
    {
        public InvoiceRecordBase() { }

        public InvoiceRecordBase(InvoiceRecordBase parentRecord)
        {
            StandardMessageIdentifier = IdecConstants.StandardMessageIdentifier;
            BilledAirlineCode = parentRecord.BilledAirlineCode;
            BillingAirlineCode = parentRecord.BillingAirlineCode;
            InvoiceNumber = parentRecord.InvoiceNumber;
        }

        [FieldFixedLength(4), FieldConverter(typeof(PaddingConverter), 4, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string BillingAirlineCode;

        [FieldFixedLength(4), FieldConverter(typeof(PaddingConverter), 4, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string BilledAirlineCode;

        [FieldFixedLength(1)]
        public string BillingCode;

        [FieldFixedLength(10)]
        public string InvoiceNumber;

        [FieldFixedLength(4)]
        public string Filler1;
    }
}
