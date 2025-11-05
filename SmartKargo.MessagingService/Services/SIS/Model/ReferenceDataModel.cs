using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model
{
    public class ReferenceDataModel : ModelBase
    {
        public int ReferenceDataID { get; set; }
        public string AirlineCode { get; set; }
        public string CompanyLegalName { get; set; }        
        public string TaxVATRegistrationID { get; set; }
        public string AdditionalTaxVATRegistrationID { get; set; }
        public string CompanyRegistrationID { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string CityName { get; set; }
        public string SubDivisionCode { get; set; }
        public string SubDivisionName { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string PostalCode { get; set; }

        public string OrganizationDesignator { get; set; }
        public bool IsBillingMember { get; set; }
    }
}