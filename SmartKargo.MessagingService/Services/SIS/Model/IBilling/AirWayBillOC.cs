using System;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model.IBilling
{
    public class AirWayBillOC : ModelBase
    {
        public int ID { get; set; }
        public Nullable<int> BillingInterlineAirWayBillID { get; set; }
        public string OtherChargeCode { get; set; }
        public Nullable<decimal> OtherChargeCodeValue { get; set; }
        public string VATLabel { get; set; }
        public string VATText { get; set; }
        public Nullable<decimal> VATBaseAmount { get; set; }
        public Nullable<decimal> VATPercentage { get; set; }
        public Nullable<decimal> VATCalculatedAmount { get; set; }
    }
}