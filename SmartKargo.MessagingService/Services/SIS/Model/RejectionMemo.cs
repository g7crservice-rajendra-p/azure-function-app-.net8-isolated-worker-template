using System.Collections.Generic;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class RejectionMemo : MemoBase
    {
        public int RejectionMemoID { get; set; }
        public int InvoiceHeaderID { get; set; }

        public RejectionMemo()
        {
            RMAirWayBillList = new List<RMAirWayBill>();
            RMVATList = new List<RMVAT>();
        }

        public int RejectionStage { get; set; }

        public string YourRejectionNumber { get; set; }

        public string RejectionMemoNumber { get; set; }

        public string YourBillingMemoNumber { get; set; }

        public decimal? BilledTotalWeightCharge { get; set; }

        public decimal? AcceptedTotalWeightCharge { get; set; }

        public decimal? TotalWeightChargeDifference { get; set; }

        public decimal? BilledTotalValuationCharge { get; set; }

        public decimal? AcceptedTotalValuationCharge { get; set; }

        public decimal? TotalValuationChargeDifference { get; set; }

        public decimal? BilledTotalOtherChargeAmount { get; set; }

        public decimal? AcceptedTotalOtherChargeAmount { get; set; }

        public decimal? TotalOtherChargeDifference { get; set; }

        public decimal? AllowedTotalIscAmount { get; set; }

        public decimal? AcceptedTotalIscAmount { get; set; }

        public decimal? TotalIscAmountDifference { get; set; }

        public decimal? BilledTotalVatAmount { get; set; }

        public decimal? AcceptedTotalVatAmount { get; set; }

        public double? TotalVatAmountDifference { get; set; }

        public decimal? TotalNetRejectAmount { get; set; }

        public string ReasonCode { get; set; }

        public string BMCMIndicator { get; set; }

        public string CorrespondenceRefNo { get; set; }

        public List<RMAirWayBill> RMAirWayBillList { get; set; }

        public List<RMVAT> RMVATList { get; set; }
    }
}