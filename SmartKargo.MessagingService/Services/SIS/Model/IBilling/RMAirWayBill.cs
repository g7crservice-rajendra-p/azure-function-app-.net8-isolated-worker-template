using System;
using System.Collections.Generic;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model.IBilling
{
    public partial class RMAirWayBill : ModelBase
    {
        public RMAirWayBill()
        {
            BillingInterlineRMAWBOCList = new List<RMAirWayBillOC>();
            BillingInterlineRMAWBVATList = new List<RMAirWayBillVAT>();
            BillingInterlineRMAWBProrateLadderList = new List<RMAirWayBillProrateLadder>();
        }

        public int ID { get; set; }
        public string BillingAirline { get; set; }
        public string BilledAirline { get; set; }
        public string BillingCode { get; set; }
        public decimal BreakdownSrNo { get; set; }
        public System.DateTime AWBDate { get; set; }
        public string AWBIssuingAirline { get; set; }
        public decimal AWBSrNo { get; set; }
        public decimal AWBCheckDigit { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public System.DateTime TransferDate { get; set; }
        public Nullable<decimal> WtChargesBilled { get; set; }
        public Nullable<decimal> WtChargesAccepted { get; set; }
        public Nullable<decimal> WtChargesDiff { get; set; }
        public Nullable<decimal> ValChargesBilled { get; set; }
        public Nullable<decimal> ValChargesAccepted { get; set; }
        public Nullable<decimal> ValChargesDiff { get; set; }
        public Nullable<decimal> OCAmtBilled { get; set; }
        public Nullable<decimal> OCAmtAccepted { get; set; }
        public Nullable<decimal> OCDiff { get; set; }
        public Nullable<decimal> AmtSubISCAllowed { get; set; }
        public Nullable<decimal> AmtSubISCAccepted { get; set; }
        public Nullable<decimal> ISCPerAllowed { get; set; }
        public Nullable<decimal> ISCPerAccepted { get; set; }
        public Nullable<decimal> ISCAmtAllowed { get; set; }
        public Nullable<decimal> ISCAmtAccepted { get; set; }
        public Nullable<decimal> ISCAmtDiff { get; set; }
        public Nullable<decimal> VATAmtBilled { get; set; }
        public Nullable<decimal> VATAmtAccepted { get; set; }
        public Nullable<decimal> VATAmtDiff { get; set; }
        public Nullable<decimal> NetRejectAmt { get; set; }
        public string CurrencyAdjInd { get; set; }
        public Nullable<decimal> BilledActualFlownWt { get; set; }
        public string ProvisoReqSPA { get; set; }
        public Nullable<decimal> ProratePer { get; set; }
        public string PartShipmentInd { get; set; }
        public string KGLBInd { get; set; }
        public string CCAInd { get; set; }
        public string OurRef { get; set; }
        public string AttachmentIndOri { get; set; }
        public string AttachmentIndValidated { get; set; }
        public Nullable<decimal> NoOfAttachments { get; set; }
        public string ISValidationFlag { get; set; }
        public string ReasonCode { get; set; }
        public string RefField { get; set; }
        public string AirlineOwnUse { get; set; }
        public string AcceptRejectStatus { get; set; }
        public byte AWBStatusID { get; set; }
        public string RMBillingCode { get; set; }
        public decimal BatchNo { get; set; }
        public decimal SeqNo { get; set; }
        public string RMNumber { get; set; }
        public decimal RejectionStage { get; set; }
        public string RMReasonCode { get; set; }
        public string RMAirlineOwnUse { get; set; }
        public string YourInvNo { get; set; }
        public Nullable<decimal> YourInvBillingYear { get; set; }
        public Nullable<decimal> YourInvBillingMonth { get; set; }
        public Nullable<decimal> YourInvBillingPeriod { get; set; }
        public string YourRMNumber { get; set; }
        public string BMCMIndicator { get; set; }
        public string YourBMCMNumber { get; set; }
        public string RMAttachmentIndOri { get; set; }
        public string RMAttachmentIndValidated { get; set; }
        public Nullable<decimal> RMNoOfAttachments { get; set; }
        public string RMISValidationFlag { get; set; }
        public string RMOurRef { get; set; }
        public string ReasonRemarks { get; set; }
        public string CorrespondenceRefNo { get; set; }
        public Nullable<int> RejectedAWBID { get; set; }
        public string RejectedFromTable { get; set; }
        public Nullable<bool> IsSIS { get; set; }
        public Nullable<int> RemarkID { get; set; }
        public Nullable<long> AWBID { get; set; }
        public string AWBNumber { get; set; }
        public List<RMAirWayBillOC> BillingInterlineRMAWBOCList { get; set; }
        public List<RMAirWayBillVAT> BillingInterlineRMAWBVATList { get; set; }
        public List<RMAirWayBillProrateLadder> BillingInterlineRMAWBProrateLadderList { get; set; }
    }
}
