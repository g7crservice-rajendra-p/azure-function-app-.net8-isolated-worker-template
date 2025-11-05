namespace QidWorkerRole.SIS.Model.Base
{
    public class AWBOtherChargesBase : ModelBase
    {
        public string OtherChargeCode { get; set; }

        public double? OtherChargeCodeValue { get; set; }

        public string OtherChargeVatLabel { get; set; }

        public string OtherChargeVatText { get; set; }

        public double? OtherChargeVatBaseAmount { get; set; }

        public double? OtherChargeVatPercentage { get; set; }

        public double? OtherChargeVatCalculatedAmount { get; set; }
    }
}