using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.Write;
using QidWorkerRole.SIS.Model;
using QidWorkerRole.SIS.Model.SupportingModels;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    [FixedLengthRecord()]
    public class ReferenceData1Record : InvoiceRecordBase, IClassToRecordConverter<ReferenceDataModel>, IRecordToClassConverter<ReferenceDataModel>
    {
       
        #region Record Properties

        [FieldFixedLength(1), FieldConverter(typeof(PaddingConverter), 1, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string RecordSerialNo;

        [FieldFixedLength(100)]
        public string CompanyLegalName;

        [FieldFixedLength(25)]
        public string TaxRegistrationId;

        [FieldFixedLength(25)]
        public string AdditionalTaxRegId;

        [FieldFixedLength(25)]
        public string CompanyRegistrationId;

        [FieldFixedLength(70)]
        public string AddressLine1;

        [FieldFixedLength(70)]
        public string AddressLine2;

        [FieldFixedLength(70)]
        public string AddressLine3;

        [FieldFixedLength(78)]
        [FieldTrim(TrimMode.Right)]
        public string Filler2;

        [FieldHidden]
        public ReferenceData2Record ReferenceData2Record;
        
        #endregion

        #region Constructors

        #region Parameterless Constructor

        public ReferenceData1Record()
        { }

        #endregion

        #region Parameterized Constructor

        public ReferenceData1Record(InvoiceRecordBase invoiceRecordBase)
            : base(invoiceRecordBase)
        { }

        #endregion

        #endregion

        #region Implementation of IClassToRecordConverter<ReferenceDataModel> To Write IDEC File.

        /// <summary>
        /// To Convert ReferenceDataModel into ReferenceData1Record.
        /// </summary>
        /// <param name="referenceDataModel">ReferenceDataModel</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ConvertClassToRecord(ReferenceDataModel referenceDataModel, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting ReferenceDataModel into ReferenceData1Record.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = referenceDataModel.IsBillingMember ? IdecConstants.SfiReferenceData1 : IdecConstants.SfiReferenceData2;
            RecordSerialNo = IdecConstants.ReferenceData1RecordSerialNo.ToString();
            CompanyLegalName = referenceDataModel.CompanyLegalName.Replace(IdecConstants.OrganizationNameSeparator, string.Empty);
            TaxRegistrationId = referenceDataModel.TaxVATRegistrationID;
            AdditionalTaxRegId = referenceDataModel.AdditionalTaxVATRegistrationID;
            CompanyRegistrationId = referenceDataModel.CompanyRegistrationID;
            AddressLine1 = referenceDataModel.AddressLine1;
            AddressLine2 = referenceDataModel.AddressLine2;
            AddressLine3 = referenceDataModel.AddressLine3;

            ProcessNextClass(referenceDataModel, ref recordSequenceNumber);

            //Logger.Info("End of Converting ReferenceDataModel into ReferenceData1Record.");
        }

        /// <summary>
        /// To Convert Childs of ReferenceDataModel into there corresponding Records.
        /// </summary>
        /// <param name="referenceDataModel">ReferenceDataModel</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ProcessNextClass(ReferenceDataModel referenceDataModel, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting childs of ReferenceDataModel into there corresponding records.");

            if (referenceDataModel != null)
            {
                // populate referencedata2
                var oReferenceDataPart2 = new ReferenceData2();
                oReferenceDataPart2.CityName = referenceDataModel.CityName;
                oReferenceDataPart2.CountryCode = referenceDataModel.CountryCode;
                oReferenceDataPart2.CountryName = referenceDataModel.CountryName;
                oReferenceDataPart2.SubdivisionCode = referenceDataModel.SubDivisionCode;
                oReferenceDataPart2.SubdivisionName = referenceDataModel.SubDivisionName;
                oReferenceDataPart2.PostalCode = referenceDataModel.PostalCode;
                oReferenceDataPart2.IsBillingMember = referenceDataModel.IsBillingMember;

                var oReferenceData2Record = new ReferenceData2Record(this);
                oReferenceData2Record.ConvertClassToRecord(oReferenceDataPart2, ref recordSequenceNumber);
                ReferenceData2Record = oReferenceData2Record;
            }

            //Logger.Info("End of Converting childs of ReferenceDataModel into there corresponding records.");
        }

        #endregion

        #region Implementation of IRecordToClassConverter<ReferenceDataModel> To Read IDEC File.

        /// <summary>
        /// To Convert ReferenceData1Record into ReferenceDataModel.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <returns>ReferenceDataModel</returns>
        public ReferenceDataModel ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Info("Start of Converting ReferenceData1Record into ReferenceDataModel.");

            var referenceDataModel = CreateReferenceDataModel();

            ProcessNextRecord(multiRecordEngine, referenceDataModel);

            //Logger.Info("End of Converting ReferenceData1Record into ReferenceDataModel.");

            return referenceDataModel;
        }

        /// <summary>
        /// Creates ReferenceDataModel for all the ReferenceData1Record present in the reading input IDEC File.
        /// </summary>
        /// <returns>ReferenceDataModel</returns>
        private ReferenceDataModel CreateReferenceDataModel()
        {
            //Logger.Info("Start of Creating ReferenceDataModel Class from ReferenceData1Record.");

            var referenceDataModel = new ReferenceDataModel
            {
                CompanyLegalName = CompanyLegalName.Trim(),
                TaxVATRegistrationID = TaxRegistrationId.Trim(),
                AdditionalTaxVATRegistrationID = AdditionalTaxRegId.Trim(),
                CompanyRegistrationID = CompanyRegistrationId.Trim(),
                AddressLine1 = AddressLine1.Trim(),
                AddressLine2 = AddressLine2.Trim(),
                AddressLine3 = AddressLine3.Trim()
            };

            referenceDataModel.IsBillingMember = StandardFieldIdentifier == IdecConstants.SfiReferenceData1;

            //Logger.Info("End of Creating ReferenceDataModel Class from ReferenceData1Record.");

            return referenceDataModel;
        }

        /// <summary>
        /// To Convert Child records of ReferenceData1Record into there corresponding Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="referenceDataModel">ReferenceDataModel</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, ReferenceDataModel referenceDataModel)
        {
            //Logger.Info("Start of Converting Childs of ReferenceData1Record into there corresponding Classes.");

            multiRecordEngine.ReadNext();

            do
            {
                var cargoReferenceData1 = referenceDataModel;

                if (multiRecordEngine.LastRecord is ReferenceData2Record)
                {
                    var referenceData2 = (multiRecordEngine.LastRecord as IRecordToClassConverter<ReferenceData2>).ConvertRecordToClass(multiRecordEngine);

                    cargoReferenceData1.CityName = referenceData2.CityName;
                    cargoReferenceData1.CountryCode = referenceData2.CountryCode;
                    cargoReferenceData1.CountryName = referenceData2.CountryName;
                    cargoReferenceData1.PostalCode = referenceData2.PostalCode;
                    cargoReferenceData1.SubDivisionCode = referenceData2.SubdivisionCode;
                    cargoReferenceData1.SubDivisionName = referenceData2.SubdivisionName;
                }
                else
                {
                    break;
                }

            } while (multiRecordEngine.LastRecord != null);

            //Logger.Info("End of Converting Childs of ReferenceData1Record into there corresponding Classes.");
        }

        #endregion

    }
}