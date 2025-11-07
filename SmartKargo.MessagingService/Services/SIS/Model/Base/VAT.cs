namespace QidWorkerRole.SIS.Model.Base
{
    public abstract class VAT : ModelBase
    {
        public string VatIdentifier { get; set; }

        public string VatLabel { get; set; }

        public string VatText { get; set; }

        public double VatBaseAmount { get; set; }

        public double VatPercentage { get; set; }

        public double VatCalculatedAmount { get; set; }
    }
}