using System.Collections.Generic;

namespace QidWorkerRole.SIS.Model.SupportingModels
{
    public class FileData
    {
        /// <summary>
        /// This will store list of Invoices for one billed airline
        /// </summary>
        public List<Invoice> InvoiceList { get; set; }

        /// <summary>
        /// This will store FileTotal model corresponding to a one Idec file
        /// </summary>
        public FileTotal FileTotal { get; set; }

        /// <summary>
        /// This will store FileHeadet model
        /// </summary>
        public FileHeaderClass FileHeader { get; set; }
    }
}