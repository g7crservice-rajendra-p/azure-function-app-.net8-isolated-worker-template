using System;
using System.Collections.Generic;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model.IBilling
{
    public class AirWayBill : ModelBase
    {
        public AirWayBill()
        {
            AirWayBillOCList = new List<AirWayBillOC>();
            AirWayBillVATList = new List<AirWayBillVAT>();
        }

        public int ID { get; set; }
        public Nullable<int> BillingInterlineInvoiceID { get; set; }
        public string BillingAirline { get; set; }
        public string BilledAirline { get; set; }
        public string BillingCode { get; set; }
        public Nullable<decimal> BatchSequenceNumber { get; set; }
        public Nullable<decimal> RecordSequencewithinBatch { get; set; }
        public System.DateTime AWBDate { get; set; }
        public string AWBIssuingAirline { get; set; }
        public decimal AWBSerialNumber { get; set; }
        public decimal AWBCheckDigit { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public System.DateTime DateOfCarriage { get; set; }
        public Nullable<decimal> WeightCharges { get; set; }
        public Nullable<decimal> OtherCharges { get; set; }
        public Nullable<decimal> AmountSubjectToInterlineServiceCharge { get; set; }
        public Nullable<decimal> InterlineServiceChargePercentage { get; set; }
        public string CurrencyAdjustmentIndicator { get; set; }
        public Nullable<decimal> BilledWeight { get; set; }
        public string ProvisoReqSPA { get; set; }
        public Nullable<decimal> ProratePercentage { get; set; }
        public string PartShipmentIndicator { get; set; }
        public Nullable<decimal> ValuationCharges { get; set; }
        public string KGLBIndicator { get; set; }
        public Nullable<decimal> VATAmount { get; set; }
        public Nullable<decimal> InterlineServiceChargeAmount { get; set; }
        public Nullable<decimal> AWBTotalAmount { get; set; }
        public string CCAindicator { get; set; }
        public string OurReference { get; set; }
        public string AttachmentIndicatorOriginal { get; set; }
        public string AttachmentIndicatorValidated { get; set; }
        public Nullable<decimal> NumberOfAttachments { get; set; }
        public string ReasonCode { get; set; }
        public string ReferenceField1 { get; set; }
        public string ReferenceField2 { get; set; }
        public string ReferenceField3 { get; set; }
        public string ReferenceField4 { get; set; }
        public string ReferenceField5 { get; set; }
        public string AirlineOwnUse { get; set; }
        public string PayableReceiveable { get; set; }
        public int AWBStatusID { get; set; }
        public long? AWBID { get; set; }
        public string AWBNumber { get; set; }
        public List<AirWayBillOC> AirWayBillOCList { get; set; }
        public List<AirWayBillVAT> AirWayBillVATList { get; set; }
    }
}