using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class RMVAT : VAT
    {
        public int RMVATID { get; set; }
        public int RejectionMemoID { get; set; }
    }
}