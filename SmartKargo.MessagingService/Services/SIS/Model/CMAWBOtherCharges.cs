using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class CMAWBOtherCharges : AWBOtherChargesBase
    {
        public int CMAWBOtherChargesID { get; set; }
        public int CMAirWayBillID { get; set; }
    }
}