using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    /// <summary>
    /// To match with data model to create Entity Framework Context object 
    /// </summary>
    public class CMVAT : VAT
    {
        public int CMVATID { get; set; }
        public int CreditMemoID { get; set; }
    }
}