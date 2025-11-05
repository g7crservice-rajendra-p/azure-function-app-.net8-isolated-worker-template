using System;
using System.Collections.Generic;
using QidWorkerRole.SIS.Model.Base;
using System.Xml.Linq;
using System.Linq;

namespace QidWorkerRole.SIS.Model.IBilling
{
    public class BMAirWayBill : ModelBase
    {
        public BMAirWayBill()
        {
            BillingInterlineBMAWBOCList = new List<BMAirWayBillOC>();
            BillingInterlineBMAWBVATList = new List<BMAirWayBillVAT>();
            BillingInterlineBMAWBProrateLadderList = new List<BMAirWayBillProrateLadder>();
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
        public Nullable<decimal> WtChargesBilled { get; set; }
        public Nullable<decimal> ValChargesBilled { get; set; }
        public Nullable<decimal> OCAmtBilled { get; set; }
        public Nullable<decimal> AmtSubISCBilled { get; set; }
        public Nullable<decimal> ISCPerBilled { get; set; }
        public Nullable<decimal> ISCAmtBilled { get; set; }
        public Nullable<decimal> VATAmtBilled { get; set; }
        public Nullable<decimal> TotalAmtBilled { get; set; }
        public string CurrencyAdjInd { get; set; }
        public decimal BilledWt { get; set; }
        public string ProvisoReqSpa { get; set; }
        public Nullable<decimal> ProratePer { get; set; }
        public string PartShipmentInd { get; set; }
        public string KGLBInd { get; set; }
        public string CCaIndi { get; set; }
        public string AttachmentIndOri { get; set; }
        public string AttachmentIndValidated { get; set; }
        public Nullable<decimal> NoOfAttachments { get; set; }
        public string ISValidationFlag { get; set; }
        public string ReasonCode { get; set; }
        public string RefField { get; set; }
        public string AirlineOwnUse { get; set; }
        public string AcceptRejectStatus { get; set; }
        public byte AWBStatusID { get; set; }
        public string BMBillingCode { get; set; }
        public decimal BatchNo { get; set; }
        public decimal SeqNo { get; set; }
        public string BMNumber { get; set; }
        public string BMReasonCode { get; set; }
        public string OurRef { get; set; }
        public string CorrespondenceRefNo { get; set; }
        public string YourInvNo { get; set; }
        public Nullable<decimal> YourInvBillingYear { get; set; }
        public Nullable<decimal> YourInvBillingMonth { get; set; }
        public Nullable<decimal> YourInvBillingPeriod { get; set; }
        public string BMAttachmentIndOri { get; set; }
        public string BMAttachmentIndValidated { get; set; }
        public Nullable<decimal> BMNoOfAttachments { get; set; }
        public string BMAirlineOwnUse { get; set; }
        public string BMISValidationFlag { get; set; }
        public string ReasonRemarks { get; set; }
        public bool IsSIS { get; set; }
        public int RemarkID { get; set; }
        public long? AWBID { get; set; }
        public string AWBNumber { get; set; }
        public List<BMAirWayBillOC> BillingInterlineBMAWBOCList { get; set; }
        public List<BMAirWayBillVAT> BillingInterlineBMAWBVATList { get; set; }
        public List<BMAirWayBillProrateLadder> BillingInterlineBMAWBProrateLadderList { get; set; }
    }
}