using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class CMAWBProrateLadder : AWBProrateLadderBase
    {
        public int CMAWBProrateLadderID { get; set; }
        public int CMAirWayBillID { get; set; }
    }
}