using System.Collections.Generic;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class BMAirWayBill : AirWayBillBase
    {
        public int BMAirWayBillId { get; set; }
        public int BillingMemoID { get; set; }

        public BMAirWayBill()
        {
            BMAWBOtherChargesList = new List<BMAWBOtherCharges>();
            BMAWBProrateLadderList = new List<BMAWBProrateLadder>();
            BMAWBVATList = new List<BMAWBVAT>();
        }

        public double? BilledWeightCharge { get; set; }

        public double? BilledValuationCharge { get; set; }

        public double BilledOtherCharge { get; set; }

        public double BilledAmtSubToIsc { get; set; }

        public double BilledIscPercentage { get; set; }

        public double BilledIscAmount { get; set; }

        public double? BilledVatAmount { get; set; }

        public double TotalAmount { get; set; }

        public string CurrencyAdjustmentIndicator { get; set; }

        public int? BilledWeight { get; set; }

        public string ProvisionalReqSpa { get; set; }

        public int? PrpratePercentage { get; set; }

        public string PartShipmentIndicator { get; set; }

        public string KgLbIndicator { get; set; }

        public bool CcaIndicator { get; set; }

        //PRORATE LADDER HEADER FIELD.
        public string ProrateCalCurrencyId { get; set; }
        public double? TotalProrateAmount { get; set; }
        public string AwbDateDisplayText { get; set; }

        public List<BMAWBOtherCharges> BMAWBOtherChargesList { get; private set; }

        public List<BMAWBProrateLadder> BMAWBProrateLadderList { get; set; }

        public List<BMAWBVAT> BMAWBVATList { get; set; }
    }
}
