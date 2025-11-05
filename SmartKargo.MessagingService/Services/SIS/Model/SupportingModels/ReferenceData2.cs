using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model.SupportingModels
{
    /// <summary>
    /// All the properties from this Class are available in ReferenceData Class
    /// This model is only used to Write the file.
    /// </summary>
    public class ReferenceData2 : ModelBase
    {
        public int InvoiceHeaderID { get; set; }

        public string CityName { get; set; }
        public string SubdivisionCode { get; set; }
        public string SubdivisionName { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string PostalCode { get; set; }

        public bool IsBillingMember { get; set; }
    }
}