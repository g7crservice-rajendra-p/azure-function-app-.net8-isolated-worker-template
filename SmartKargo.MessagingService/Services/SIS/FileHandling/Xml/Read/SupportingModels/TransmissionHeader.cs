using System;

namespace QidWorkerRole.SIS.FileHandling.Xml.Read.SupportingModels
{
    public class TransmissionHeader
    {
        public DateTime TransmissionDateTime { get; set; }

        public string Version { get; set; }

        public string TransmissionId { get; set; }

        public string IssuingOrganizationId { get; set; }

        public string ReceivingOrganizationId { get; set; }

        public string BillingCategory { get; set; }
    }
}