using System;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model.IBilling
{
    public partial class RMAirWayBillVAT : ModelBase
    {
        public int ID { get; set; }
        public int BillingInterlineRMAWBID { get; set; }
        public string VATIdentifier { get; set; }
        public string VATLabel { get; set; }
        public string VATText { get; set; }
        public Nullable<decimal> VATBaseAmt { get; set; }
        public decimal VATPer { get; set; }
        public Nullable<decimal> VATCalculatedAmt { get; set; }
    }
}
