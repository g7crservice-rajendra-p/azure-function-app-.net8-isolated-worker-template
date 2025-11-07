using System;
using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base
{
    public abstract class RecordBase
    {
        [FieldFixedLength(3)]
        public string StandardMessageIdentifier = string.Empty;

        [FieldFixedLength(8), FieldConverter(typeof(PaddingConverter), 8, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string RecordSequenceNumber = string.Empty;

        [FieldFixedLength(2)]
        public string StandardFieldIdentifier = string.Empty;

        [FieldHidden]
        public int ClearancePeriod = 0;

        [FieldHidden]
        public string ClearanceMonth = string.Empty;

        [FieldHidden]
        public int SettlementType = 0;

        [FieldHidden]
        public int SourceBillingCode = 0;

        [FieldHidden]
        public string FileName = string.Empty;

        [FieldHidden]
        public DateTime FileSubmissionDate = DateTime.Today;

        [FieldHidden]
        public long PreviousRecordSequenceNumber = 0;
    }
}
