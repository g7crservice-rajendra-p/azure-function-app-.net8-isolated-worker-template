namespace QidWorkerRole.SIS.FileHandling.Xml.Read.SupportingModels
{
    class AddOnChargeAmountsDetails
    {
        public double InterlineServiceChargeAmount { get; set; }

        public double InterlineServiceChargePercentage { get; set; }

        public double IscAccepted { get; set; }

        public double IscAcceptedPercentage { get; set; }

        public double IscDifference { get; set; }


        public double OtherCharges { get; set; }

        public string OtherChargesAllowedCode { get; set; }

        public double OtherChargesAccepted { get; set; }

        public string OtherChargesAccptedCode { get; set; }

        public double OtherChargesDifference { get; set; }


        public double AmountSubjectToInterlineServiceCharge { get; set; }

        public double AmountSubjectToIscAccepted { get; set; }

        public double AmountSubjectToIscDifference { get; set; }
    }
}
