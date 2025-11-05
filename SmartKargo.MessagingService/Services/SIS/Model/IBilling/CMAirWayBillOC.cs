using System;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model.IBilling
{
    public class CMAirWayBillOC : ModelBase
    {
        public int ID { get; set; }
        public int BillingInterlineCMAWBID { get; set; }
        public string OCCode { get; set; }
        public decimal OCCodeValue { get; set; }
        public string VATLabel { get; set; }
        public string VATText { get; set; }
        public Nullable<decimal> VATBaseAmt { get; set; }
        public Nullable<decimal> VATPer { get; set; }
        public Nullable<decimal> VATCalculatedAmt { get; set; }
    }
}