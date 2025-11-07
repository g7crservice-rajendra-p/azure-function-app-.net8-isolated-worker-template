using System.Collections.Generic;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class BillingMemo : MemoBase
    {
        public int BillingMemoID { get; set; }
        public int InvoiceHeaderID { get; set; }

        public BillingMemo()
        {
            BMAirWayBillList = new List<BMAirWayBill>();
            BMVATList = new List<BMVAT>();
        }

        public string BillingMemoNumber { get; set; }

        public string ReasonCode { get; set; }

        public string CorrespondenceReferenceNumber { get; set; }

        public decimal? BilledTotalWeightCharge { get; set; }

        public decimal? BilledTotalValuationAmount { get; set; }

        public decimal BilledTotalOtherChargeAmount { get; set; }

        public decimal BilledTotalIscAmount { get; set; }

        public decimal? BilledTotalVatAmount { get; set; }

        public decimal? NetBilledAmount { get; set; }

        public List<BMAirWayBill> BMAirWayBillList { get; set; }

        public List<BMVAT> BMVATList { get; set; }
    }
}
