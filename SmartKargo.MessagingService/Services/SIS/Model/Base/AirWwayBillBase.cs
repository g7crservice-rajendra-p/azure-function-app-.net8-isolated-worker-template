using System;

namespace QidWorkerRole.SIS.Model.Base
{
    public class AirWayBillBase : ModelBase
    {
        public string BillingCode { get; set; }
        public DateTime? AWBDate { get; set; }
        public string AWBIssuingAirline { get; set; }
        public int AWBSerialNumber { get; set; }
        public int AWBCheckDigit { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string DateOfCarriageOrTransfer { get; set; }
        public string AttachmentIndicatorOriginal { get; set; }
        public string AttachmentIndicatorValidated { get; set; }
        public int? NumberOfAttachments { get; set; }
        public string ISValidationFlag { get; set; }
        public string ReasonCode { get; set; }
        public string ReferenceField1 { get; set; }
        public string ReferenceField2 { get; set; }
        public string ReferenceField3 { get; set; }
        public string ReferenceField4 { get; set; }
        public string ReferenceField5 { get; set; }
        public string AirlineOwnUse { get; set; } 


        public int BreakdownSerialNumber { get; set; }
        public string FilingReference { get; set; }
        public long? AWBID { get; set; }
        public string AWBNumber { get; set; }

        /// <summary>
        /// Number of child records required in case of IDEC validations.
        /// </summary>
        public long NumberOfChildRecords { get; set; }
    }
}