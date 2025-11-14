using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.Write;
using QidWorkerRole.SIS.Model.SupportingModels;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    [FixedLengthRecord()]
    public class ReferenceData2Record : InvoiceRecordBase, IClassToRecordConverter<ReferenceData2>, IRecordToClassConverter<ReferenceData2>
    {
       
        #region Record Properties

        [FieldFixedLength(1)]
        public string RecordSerialNo;

        [FieldFixedLength(50)]
        public string CityName;

        [FieldFixedLength(3)]
        public string SubdivisionCode;

        [FieldFixedLength(50)]
        public string SubdivisionName;

        [FieldFixedLength(2)]
        public string CountryCode;

        [FieldFixedLength(50)]
        public string CountryName;

        [FieldFixedLength(50)]
        public string PostalCode;

        [FieldFixedLength(258)]
        [FieldTrim(TrimMode.Right)]
        public string Filler2;

        #endregion

        #region Constructors

        #region Parameterless Constructor

        public ReferenceData2Record() { }

        #endregion

        #region Parameterized Constructor

        public ReferenceData2Record(InvoiceRecordBase invoiceRecordBase)
            : base(invoiceRecordBase)
        { }

        #endregion

        #endregion

        #region Implementation of IClassToRecordConverter<ReferenceData2> To Write IDEC File.

        /// <summary>
        /// To Convert ReferenceData2 into ReferenceData2Record.
        /// </summary>
        /// <param name="referenceData2">ReferenceData2</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ConvertClassToRecord(ReferenceData2 referenceData2, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting ReferenceData2 into ReferenceData2Record.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = referenceData2.IsBillingMember ? IdecConstants.SfiReferenceData1 : IdecConstants.SfiReferenceData2;
            RecordSerialNo = IdecConstants.ReferenceData2RecordSerialNo.ToString();
            CityName = referenceData2.CityName;
            SubdivisionCode = referenceData2.SubdivisionCode;
            SubdivisionName = referenceData2.SubdivisionName;
            CountryCode = referenceData2.CountryCode;
            CountryName = referenceData2.CountryName;
            PostalCode = referenceData2.PostalCode;

            ProcessNextClass(referenceData2, ref recordSequenceNumber);

            //Logger.Info("End of Converting ReferenceData2 into ReferenceData2Record.");

        }

        /// <summary>
        /// To Convert Childs of ReferenceData2 into there corresponding Records.
        /// </summary>
        /// <param name="referenceData2">ReferenceData2</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ProcessNextClass(ReferenceData2 referenceData2, ref long recordSequenceNumber)
        {
            //Logger.Info("ReferenceData2 cannot have Childs.");
        }

        #endregion

        #region Implementation of IRecordToClassConverter<ReferenceData2> To Read IDEC File.

        /// <summary>
        /// To Convert ReferenceData2Record into ReferenceData2.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <returns>ReferenceData2</returns>
        public ReferenceData2 ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Info("Start of Converting ReferenceData2Record into ReferenceData2.");

            var referenceData2 = CreateReferenceData2();

            ProcessNextRecord(multiRecordEngine, referenceData2);

            //Logger.Info("End of Converting ReferenceData2Record into ReferenceData2.");

            return referenceData2;
        }

        /// <summary>
        /// Creates ReferenceData2 for all the ReferenceData2Record present in the reading input IDEC File.
        /// </summary>
        /// <returns>ReferenceData2</returns>
        private ReferenceData2 CreateReferenceData2()
        {
            //Logger.Info("Start of Creating ReferenceData2 Class from ReferenceData2Record.");

            var referenceData2 = new ReferenceData2
                                    {
                                        CityName = CityName.Trim(),
                                        SubdivisionCode = SubdivisionCode.Trim(),
                                        SubdivisionName = SubdivisionName.Trim(),
                                        CountryCode = CountryCode.Trim(),
                                        CountryName = CountryName.Trim(),
                                        PostalCode = PostalCode.Trim(),
                                        IsBillingMember = StandardFieldIdentifier == IdecConstants.SfiReferenceData1,
                                    };
            
            //Logger.Info("End of Creating ReferenceData2 Class from ReferenceData2Record.");

            return referenceData2;
        }

        /// <summary>
        /// To Convert Child records of ReferenceData2Record into there corresponding Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="referenceData2">ReferenceData2</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, ReferenceData2 referenceData2)
        {
            multiRecordEngine.ReadNext();
            //Logger.Info("ReferenceData2Record cannot have Childs.");
        }

        #endregion

    }
}
