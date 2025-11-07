using System.Collections.Generic;

namespace QidWorkerRole.SIS.FileHandling.Xml.Read.SupportingModels
{
    public class TransmissionSummary
    {
        public TransmissionSummary()
        {
            TotalInvoiceAmount = new List<decimal>();
        }

        public int InvoiceCount { get; set; }

        public List<decimal> TotalInvoiceAmount { get; private set; }
    }
}