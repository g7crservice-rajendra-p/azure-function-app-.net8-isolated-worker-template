using System.Collections.Generic;

namespace QidWorkerRole.SIS.Model
{
    public class Invoice : InvoiceHeader
    {
        public Invoice()
        {
            AirWayBillList = new List<AirWayBill>();
            RejectionMemoList = new List<RejectionMemo>();
            BillingMemoList = new List<BillingMemo>();
            CreditMemoList = new List<CreditMemo>();
            InvoiceTotalVATList = new List<InvoiceTotalVAT>();
            BillingCodeSubTotalList = new List<BillingCodeSubTotal>();
            ReferenceDataList = new List<ReferenceDataModel>();
        }

        public List<ReferenceDataModel> ReferenceDataList { get; set; }

        public List<AirWayBill> AirWayBillList { get; private set; }

        public List<RejectionMemo> RejectionMemoList { get; private set; }

        public List<BillingMemo> BillingMemoList { get; private set; }

        public List<CreditMemo> CreditMemoList { get; private set; }

        public InvoiceTotal InvoiceTotals { get; set; }

        public List<InvoiceTotalVAT> InvoiceTotalVATList { get; private set; }

        public List<BillingCodeSubTotal> BillingCodeSubTotalList { get; private set; }


        public int BatchSequenceNumber { get; set; }

        public int RecordSequenceWithinBatch { get; set; }

        public long NumberOfChildRecords { get; set; }
    }
}