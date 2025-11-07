using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    /// <summary>
    /// To match with data model to create Entity Framework Context object 
    /// </summary>
    public class BMAWBVAT : VAT
    {
        public int BMAWBVATID { get; set; }
        public int BMAirWayBillID { get; set; }
    }
}
