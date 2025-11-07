using System.Collections.Generic;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class BillingCodeSubTotal : ModelBase
    {
        public int BillingCodeSubTotalID { get; set; }
        public int InvoiceHeaderID { get; set; }

        public BillingCodeSubTotal()
        {
            BillingCodeSubTotalVATList = new List<BillingCodeSubTotalVAT>();
        }

        public string BillingCode { get; set; }
        public decimal TotalWeightCharge { get; set; }
        public decimal TotalOtherCharge { get; set; }
        public decimal TotalIscAmount { get; set; }
        public decimal BillingCodeSbTotal { get; set; }
        public int NumberOfBillingRecords { get; set; }
        public decimal TotalValuationCharge { get; set; }
        public decimal TotalVatAmount { get; set; }
        public int TotalNumberOfRecords { get; set; }
        public string BillingCodeSubTotalDesc { get; set; }

        public long NumberOfChildRecords { get; set; }
        public decimal TotalNetAmount { get; set; }

        public List<BillingCodeSubTotalVAT> BillingCodeSubTotalVATList { get; set; }
    }
}