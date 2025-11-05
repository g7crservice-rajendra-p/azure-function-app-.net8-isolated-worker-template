using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class BMAWBOtherCharges : AWBOtherChargesBase
    {
        public int BMAWBOtherChargesID { get; set; }
        public int BMAirWayBillID { get; set; }
    }
}