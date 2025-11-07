using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class InvoiceTotalVAT : ModelBase
    {
        public int InvoiceTotalVATID { get; set; }
        public int InvoiceHeaderID { get; set; }

        public string VatIdentifier { get; set; }

        public string VatLabel { get; set; }

        public string VatText { get; set; }

        public double VatBaseAmount { get; set; }

        public double? VatPercentage { get; set; }

        public double? VatCalculatedAmount { get; set; }
    }
}
