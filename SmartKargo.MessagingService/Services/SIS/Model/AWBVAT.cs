using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    /// <summary>
    /// To match with data model to create Entity Framework Context object 
    /// </summary>
    public class AWBVAT : VAT
    {
        public int AWBVATID { get; set; }
        public int AirWayBillID { get; set; }
    }
}