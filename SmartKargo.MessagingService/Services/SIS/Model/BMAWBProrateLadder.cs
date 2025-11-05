using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class BMAWBProrateLadder : AWBProrateLadderBase
    {
        public int BMAWBProrateLadderID { get; set; }
        public int BMAirWayBillID { get; set; }
    }
}
