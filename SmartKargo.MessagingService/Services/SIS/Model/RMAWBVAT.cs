using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    /// <summary>
    /// To match with data model to create Entity Framework Context object 
    /// </summary>
    public class RMAWBVAT : VAT
    {
        public int RMAWBVATID { get; set; }
        public int RMAirWayBillID { get; set; }
    }
}