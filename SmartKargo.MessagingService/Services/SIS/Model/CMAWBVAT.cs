using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    /// <summary>
    /// To match with data model to create Entity Framework Context object 
    /// </summary>
    public class CMAWBVAT : VAT
    {
        public int CMAWBVATID { get; set; }
        public int CMAirWayBillID { get; set; }
    }
}