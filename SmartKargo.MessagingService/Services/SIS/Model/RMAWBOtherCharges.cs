using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class RMAWBOtherCharges : AWBOtherChargesBase
    {
        public int RMAWBOtherChargesID { get; set; }
        public int RMAirWayBillID { get; set; }
    }
}