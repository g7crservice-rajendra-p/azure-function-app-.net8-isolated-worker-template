using System;
using System.Collections.Generic;
using QidWorkerRole.SIS.Model.Base;
using System.Xml.Linq;
using System.Linq;

namespace QidWorkerRole.SIS.Model.IBilling
{
    public class CMAirWayBill : ModelBase
    {
        public CMAirWayBill()
        {
            BillingInterlineCMAWBOCList = new List<CMAirWayBillOC>();
            BillingInterlineCMAWBVATList = new List<CMAirWayBillVAT>();
            BillingInterlineCMAWBProrateLadderList = new List<CMAirWayBillProrateLadder>();
        }

        public int ID { get; set; }
        public string BillingAirline { get; set; }
        public string BilledAirline { get; set; }
        public string BillingCode { get; set; }
        public decimal BreakdownSrNo { get; set; }
        public Nullable<System.DateTime> AWBDate { get; set; }
        public string AWBIssuingAirline { get; set; }
        public decimal AWBSrNo { get; set; }
        public decimal AWBCheckDigit { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public System.DateTime TransferDate { get; set; }
        public Nullable<decimal> WtChargesCredited { get; set; }
        public Nullable<decimal> ValChargesCredited { get; set; }
        public Nullable<decimal> OCAmtCredited { get; set; }
        public Nullable<decimal> AmtSubISCCredited { get; set; }
        public Nullable<decimal> ISCPerCredited { get; set; }
        public Nullable<decimal> ISCAmtCredited { get; set; }
        public Nullable<decimal> VATAmtCredited { get; set; }
        public Nullable<decimal> TotalAmtCredited { get; set; }
        public string CurrencyAdjInd { get; set; }
        public decimal BilledWt { get; set; }
        public string ProvisoReqSPA { get; set; }
        public Nullable<decimal> ProratePer { get; set; }
        public string PartShipmentInd { get; set; }
        public string KGLBInd { get; set; }
        public string CCaInd { get; set; }
        public string AttachmentIndOri { get; set; }
        public string AttachmentIndValidated { get; set; }
        public Nullable<decimal> NoOfAttachments { get; set; }
        public string ISValidationFlag { get; set; }
        public string ReasonCode { get; set; }
        public string RefField { get; set; }
        public string AirlineOwnUse { get; set; }
        public byte AWBStatusID { get; set; }
        public string CMBillingCode { get; set; }
        public decimal BatchNo { get; set; }
        public decimal SeqNo { get; set; }
        public string CMNumber { get; set; }
        public string CMReasonCode { get; set; }
        public string OurRef { get; set; }
        public string CorrespondenceRefNo { get; set; }
        public string YourInvNo { get; set; }
        public Nullable<decimal> YourInvBillingYear { get; set; }
        public Nullable<decimal> YourInvBillingMonth { get; set; }
        public Nullable<decimal> YourInvBillingPeriod { get; set; }
        public string CMAttachmentIndOri { get; set; }
        public string CMAttachmentIndValidated { get; set; }
        public Nullable<decimal> CMNoOfAttachments { get; set; }
        public string CMAirlineOwnUse { get; set; }
        public string CMISValidationFlag { get; set; }
        public string ReasonRemarks { get; set; }
        public bool IsSIS { get; set; }
        public int RemarkID { get; set; }
        public long? AWBID { get; set; }
        public string AWBNumber { get; set; }
        public List<CMAirWayBillOC> BillingInterlineCMAWBOCList { get; set; }
        public List<CMAirWayBillVAT> BillingInterlineCMAWBVATList { get; set; }
        public List<CMAirWayBillProrateLadder> BillingInterlineCMAWBProrateLadderList { get; set; }
    }
}