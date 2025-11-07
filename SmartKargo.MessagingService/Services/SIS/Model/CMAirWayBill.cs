using System.Collections.Generic;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class CMAirWayBill : AirWayBillBase
    {
        public int CMAirWayBillID { get; set; }
        public int CreditMemoID { get; set; }

        public CMAirWayBill()
        {
            CMAWBVATList = new List<CMAWBVAT>();
            CMAWBOtherChargesList = new List<CMAWBOtherCharges>();
            CMAWBProrateLadderList = new List<CMAWBProrateLadder>();
        }

        public double? CreditedWeightCharge { get; set; }

        public double? CreditedValuationCharge { get; set; }

        public double CreditedOtherCharge { get; set; }

        public double CreditedAmtSubToIsc { get; set; }

        public double CreditedIscPercentage { get; set; }

        public double CreditedIscAmount { get; set; }

        public double? CreditedVatAmount { get; set; }

        public double TotalAmountCredited { get; set; }

        public string CurrencyAdjustmentIndicator { get; set; }

        public int? BilledWeight { get; set; }

        public string ProvisionalReqSpa { get; set; }

        public int? ProratePercentage { get; set; }

        public string PartShipmentIndicator { get; set; }

        public string KgLbIndicator { get; set; }

        public bool CcaIndicator { get; set; }

        //PRORATE LADDER HEADER FIELD.
        public string ProrateCalCurrencyId { get; set; }
        public double? TotalProrateAmount { get; set; }
        public string AwbDateDisplayText { get; set; }

        public List<CMAWBOtherCharges> CMAWBOtherChargesList { get; private set; }

        public List<CMAWBProrateLadder> CMAWBProrateLadderList { get; set; }

        public List<CMAWBVAT> CMAWBVATList { get; set; }
    }
}
