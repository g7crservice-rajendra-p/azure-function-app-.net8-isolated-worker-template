using System.Collections.Generic;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class RMAirWayBill : AirWayBillBase
    {
        public int RMAirWayBillID { get; set; }
        public int RejectionMemoID { get; set; }

        public RMAirWayBill()
        {
            RMAWBOtherChargesList = new List<RMAWBOtherCharges>();
            RMAWBProrateLadderList = new List<RMAWBProrateLadder>();
            RMAWBVATList = new List<RMAWBVAT>();
        }

        public double? BilledWeightCharge { get; set; }

        public double? AcceptedWeightCharge { get; set; }

        public double? WeightChargeDiff { get; set; }

        public double? BilledValuationCharge { get; set; }

        public double? AcceptedValuationCharge { get; set; }

        public double? ValuationChargeDiff { get; set; }

        public double BilledOtherCharge { get; set; }

        public double AcceptedOtherCharge { get; set; }

        public double OtherChargeDiff { get; set; }

        public double AllowedAmtSubToIsc { get; set; }

        public double AcceptedAmtSubToIsc { get; set; }

        public double AllowedIscPercentage { get; set; }

        public double AcceptedIscPercentage { get; set; }

        public double AllowedIscAmount { get; set; }

        public double AcceptedIscAmount { get; set; }

        public double IscAmountDifference { get; set; }

        public double? BilledVatAmount { get; set; }

        public double? AcceptedVatAmount { get; set; }

        public double? VatAmountDifference { get; set; }

        public double NetRejectAmount { get; set; }

        public string CurrencyAdjustmentIndicator { get; set; }

        public int? BilledWeight { get; set; }

        public string ProvisionalReqSpa { get; set; }

        public int? ProratePercentage { get; set; }

        public string PartShipmentIndicator { get; set; }

        public string KgLbIndicator { get; set; }

        public bool CcaIndicator { get; set; }

        public string OurReference { get; set; }

        //PRORATE LADDER HEADER FIELD.
        public string ProrateCalCurrencyId { get; set; }
        public double? TotalProrateAmount { get; set; }
        public string AwbDateDisplayText { get; set; }

        public List<RMAWBOtherCharges> RMAWBOtherChargesList { get; private set; }

        public List<RMAWBProrateLadder> RMAWBProrateLadderList { get; set; }

        public List<RMAWBVAT> RMAWBVATList { get; set; }
    }
}
