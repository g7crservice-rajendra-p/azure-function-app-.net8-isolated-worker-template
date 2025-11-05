using System.Collections.Generic;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class CreditMemo : MemoBase
    {
        public int CreditMemoID { get; set; }
        public int InvoiceHeaderID { get; set; }

        public CreditMemo()
        {
            CMAirWayBillList = new List<CMAirWayBill>();
            CMVATList = new List<CMVAT>();
        }

        public string CorrespondenceRefNumber { get; set; }

        public string CreditMemoNumber { get; set; }

        public string ReasonCode { get; set; }

        public decimal? TotalWeightCharges { get; set; }

        public decimal? TotalValuationAmt { get; set; }

        public decimal TotalOtherChargeAmt { get; set; }

        public decimal? NetAmountCredited { get; set; }

        public decimal TotalIscAmountCredited { get; set; }

        public decimal? TotalVatAmountCredited { get; set; }

        public List<CMVAT> CMVATList { get; set; }

        public List<CMAirWayBill> CMAirWayBillList { get; set; }
    }
}
