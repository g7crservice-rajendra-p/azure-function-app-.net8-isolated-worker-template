using System.Collections.Generic;
using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class AirWayBill : AirWayBillBase
    {
        public int AirWayBillID { get; set; }
        public int InvoiceHeaderID { get; set; }

        public AirWayBill()
        {
            AWBVATList = new List<AWBVAT>();
            AWBOtherChargesList = new List<AWBOtherCharges>();
        }

        public int BatchSequenceNumber { get; set; }
        public int RecordSequenceWithinBatch { get; set; }
        public double? WeightCharges { get; set; }
        public double OtherCharges { get; set; }
        public double AmountSubjectToInterlineServiceCharge { get; set; }        
        public double InterlineServiceChargePercentage { get; set; }
        public string CurrencyAdjustmentIndicator { get; set; }
        public int? BilledWeight { get; set; }                    
        public string ProvisoReqSPA { get; set; }
        public int? ProratePercentage { get; set; }
        public string PartShipmentIndicator { get; set; }
        public double? ValuationCharges { get; set; }
        public string KGLBIndicator { get; set; }
        public double? VATAmount { get; set; }
        public double InterlineServiceChargeAmount { get; set; }
        public double? AWBTotalAmount { get; set; }
        public bool CCAindicator { get; set; }
        public string OurReference { get; set; }

        public List<AWBVAT> AWBVATList { get; private set; }
        public List<AWBOtherCharges> AWBOtherChargesList { get; set; }
    }
}

