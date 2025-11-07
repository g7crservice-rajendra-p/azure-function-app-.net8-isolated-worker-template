using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model.IBilling
{
    public class CMAirWayBillVAT : ModelBase
    {
        public int ID { get; set; }
        public int BillingInterlineCMAWBID { get; set; }
        public string VATIdentifier { get; set; }
        public string VATLabel { get; set; }
        public string VATText { get; set; }
        public decimal VATBaseAmt { get; set; }
        public decimal VATPer { get; set; }
        public decimal VATCalculatedAmt { get; set; }
    }
}