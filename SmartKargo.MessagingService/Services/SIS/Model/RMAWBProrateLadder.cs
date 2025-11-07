using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class RMAWBProrateLadder : AWBProrateLadderBase
    {
        public int RMAWBProrateLadderID { get; set; }
        public int RMAirWayBillID { get; set; }
    }
}