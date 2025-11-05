using System;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model.IBilling
{
    public class BMAirWayBillProrateLadder : ModelBase
    {
        public int ID { get; set; }
        public int BillingInterlineBMAWBID { get; set; }
        public string CurrencyOfProrateCalculation { get; set; }
        public decimal TotalAmount { get; set; }
        public string FromSector { get; set; }
        public string ToSector { get; set; }
        public string CarrierPrefix { get; set; }
        public string ProvisoReqSPA { get; set; }
        public Nullable<decimal> ProrateFactor { get; set; }
        public Nullable<decimal> PercentShare { get; set; }
    }
}