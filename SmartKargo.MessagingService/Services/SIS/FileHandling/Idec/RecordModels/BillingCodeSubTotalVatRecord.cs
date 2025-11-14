using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.Write;
using QidWorkerRole.SIS.Model;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    public class BillingCodeSubTotalVatRecord : VatRecordBase, IClassToRecordConverter<List<BillingCodeSubTotalVAT>>, IRecordToClassConverter<List<BillingCodeSubTotalVAT>>
    {
        
        #region Record Properties

        [FieldFixedLength(11)]
        public string Filler2;

        [FieldFixedLength(4), FieldConverter(typeof(PaddingConverter), 4, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string Filler3;

        [FieldFixedLength(7), FieldConverter(typeof(PaddingConverter), 7, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string Filler4;

        [FieldFixedLength(1), FieldConverter(typeof(PaddingConverter), 1, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string Filler5;

        [FieldFixedLength(50)]
        public string Filler6;

        [FieldFixedLength(2)]
        public string VatIdentifier1;

        [FieldFixedLength(5)]
        public string VatLabel1;

        [FieldFixedLength(50)]
        public string VatText1;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatBaseAmount1;

        [FieldFixedLength(1)]
        public string VatBaseAmountSign1;

        [FieldFixedLength(5), FieldConverter(typeof(DoubleNumberConverter), 5, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatPercentage1;

        [FieldFixedLength(1)]
        public string VatPercentageSign1;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatCalculatedAmount1;

        [FieldFixedLength(1)]
        public string VatCalculatedAmountSign1;

        [FieldFixedLength(50)]
        public string Filler7;

        [FieldFixedLength(2)]
        public string VatIdentifier2;

        [FieldFixedLength(5)]
        public string VatLabel2;

        [FieldFixedLength(50)]
        public string VatText2;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatBaseAmount2;

        [FieldFixedLength(1)]
        public string VatBaseAmountSign2;

        [FieldFixedLength(5), FieldConverter(typeof(DoubleNumberConverter), 5, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatPercentage2;

        [FieldFixedLength(1)]
        public string VatPercentageSign2;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatCalculatedAmount2;

        [FieldFixedLength(1)]
        public string VatCalculatedAmountSign2;

        [FieldFixedLength(167)]
        public string Filler8;

        #endregion

        #region Constructors

        #region Parameterless Constructor

        public BillingCodeSubTotalVatRecord() { }

        #endregion

        #region Parameterized Constructor

        public BillingCodeSubTotalVatRecord(BillingCodeSubTotalRecord baseRecord)
            : base(baseRecord)
        {
            BillingCode = baseRecord.BillingCode;
        }

        #endregion

        #endregion

        #region Implementation of IClassToRecordConverter<List<BillingCodeSubTotalVAT>> To Write IDEC File.

        /// <summary>
        /// To Convert BillingCodeSubTotalVAT into BillingCodeSubTotalVatRecord.
        /// </summary>
        /// <param name="billingCodeSubTotalVATList">List of BillingCodeSubTotalVAT</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ConvertClassToRecord(List<BillingCodeSubTotalVAT> billingCodeSubTotalVATList, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting BillingCodeSubTotalVAT into BillingCodeSubTotalVatRecord.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiVatRecord;

            if (billingCodeSubTotalVATList.Count > 0)
            {
                VatIdentifier1 = billingCodeSubTotalVATList[0].VatIdentifier != null ? billingCodeSubTotalVATList[0].VatIdentifier : "NL"; // read from master
                VatLabel1 = billingCodeSubTotalVATList[0].VatLabel;
                VatText1 = billingCodeSubTotalVATList[0].VatText;
                VatBaseAmount1 = billingCodeSubTotalVATList[0].VatBaseAmount.ToString();
                VatBaseAmountSign1 = Utilities.GetSignValue(billingCodeSubTotalVATList[0].VatBaseAmount);
                VatPercentage1 = billingCodeSubTotalVATList[0].VatPercentage.ToString();
                VatPercentageSign1 = Utilities.GetSignValue(billingCodeSubTotalVATList[0].VatPercentage);
                VatCalculatedAmount1 = billingCodeSubTotalVATList[0].VatCalculatedAmount.ToString();
                VatCalculatedAmountSign1 = Utilities.GetSignValue(billingCodeSubTotalVATList[0].VatCalculatedAmount);

                if (billingCodeSubTotalVATList.Count > 1)
                {
                    VatIdentifier2 = billingCodeSubTotalVATList[1].VatIdentifier != null ? billingCodeSubTotalVATList[1].VatIdentifier : "NL"; // read from master
                    VatLabel2 = billingCodeSubTotalVATList[1].VatLabel;
                    VatText2 = billingCodeSubTotalVATList[1].VatText;
                    VatBaseAmount2 = billingCodeSubTotalVATList[1].VatBaseAmount.ToString();
                    VatBaseAmountSign2 = Utilities.GetSignValue(billingCodeSubTotalVATList[1].VatBaseAmount);
                    VatPercentage2 = billingCodeSubTotalVATList[1].VatPercentage.ToString();
                    VatPercentageSign2 = Utilities.GetSignValue(billingCodeSubTotalVATList[1].VatPercentage);
                    VatCalculatedAmount2 = billingCodeSubTotalVATList[1].VatCalculatedAmount.ToString();
                    VatCalculatedAmountSign2 = Utilities.GetSignValue(billingCodeSubTotalVATList[1].VatCalculatedAmount);
                }
            }

            //Logger.Info("End of Converting BillingCodeSubTotalVAT into BillingCodeSubTotalVatRecord.");
        }

        /// <summary>
        /// To Convert Childs of BillingCodeSubTotalVAT into there corresponding Records.
        /// </summary>
        /// <param name="billingCodeSubTotalVATList">List of BillingCodeSubTotalVAT</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ProcessNextClass(List<BillingCodeSubTotalVAT> billingCodeSubTotalVATList, ref long recordSequenceNumber)
        {
            //Logger.Info("BillingCodeSubTotalVAT cannot have Childs.");
        }

        #endregion

        #region Implementation of IRecordToClassConverter<List<BillingCodeSubTotalVAT>> To Read IDEC File.

        /// <summary>
        /// To Convert BillingCodeSubTotalVatRecord into BillingCodeSubTotalVAT.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <returns>List of BillingCodeSubTotalVAT</returns>
        public List<BillingCodeSubTotalVAT> ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Info("Start of Converting BillingCodeSubTotalVatRecord into BillingCodeSubTotalVAT.");

            var billingCodeSubTotalVats = CreateBillingCodeSubTotalVATList();

            ProcessNextRecord(multiRecordEngine, billingCodeSubTotalVats);

            //Logger.Info("End of Converting BillingCodeSubTotalVatRecord into BillingCodeSubTotalVAT.");

            return billingCodeSubTotalVats;
        }

        /// <summary>
        /// Creates List of BillingCodeSubTotalVAT for all the BillingCodeSubTotalVatRecord present in the reading input IDEC File.
        /// </summary>
        /// <returns>List of BillingCodeSubTotalVAT</returns>
        private List<BillingCodeSubTotalVAT> CreateBillingCodeSubTotalVATList()
        {
            //Logger.Info("Start of Creating BillingCodeSubTotalVAT Class from BillingCodeSubTotalVatRecord.");

            var billingCodeSubTotalVATList = new List<BillingCodeSubTotalVAT>();

            if (!string.IsNullOrEmpty(VatIdentifier1.Trim()))
            {
                billingCodeSubTotalVATList.Add(new BillingCodeSubTotalVAT
                {
                    VatIdentifier = VatIdentifier1.Trim(),
                    VatLabel = VatLabel1.Trim(),
                    VatText = VatText1.Trim(),
                    VatBaseAmount = Utilities.GetActualValueForDouble(VatBaseAmountSign1, VatBaseAmount1),
                    VatPercentage = Utilities.GetActualValueForDouble(VatPercentageSign1, VatPercentage1),
                    VatCalculatedAmount = Utilities.GetActualValueForDouble(VatCalculatedAmountSign1, VatCalculatedAmount1)
                });
            }

            if (!string.IsNullOrEmpty(VatIdentifier2.Trim()))
            {
                billingCodeSubTotalVATList.Add(new BillingCodeSubTotalVAT
                {
                    VatIdentifier = VatIdentifier2.Trim(),
                    VatLabel = VatLabel2.Trim(),
                    VatText = VatText2.Trim(),
                    VatBaseAmount = Utilities.GetActualValueForDouble(VatBaseAmountSign2, VatBaseAmount2),
                    VatPercentage = Utilities.GetActualValueForDouble(VatPercentageSign2, VatPercentage2),
                    VatCalculatedAmount = Utilities.GetActualValueForDouble(VatCalculatedAmountSign2, VatCalculatedAmount2)
                });
            }

            //Logger.Info("End of Creating BillingCodeSubTotalVAT Class from BillingCodeSubTotalVatRecord.");

            return billingCodeSubTotalVATList;
        }

        /// <summary>
        /// To Convert Child records of BillingCodeSubTotalVatRecord into there corresponding Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="billingCodeSubTotalVATList">List of BillingCodeSubTotalVAT</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, List<BillingCodeSubTotalVAT> billingCodeSubTotalVATList)
        {
            multiRecordEngine.ReadNext();
            //Logger.Info("BillingCodeSubTotalVatRecord cannot have Childs.");
        }        

        #endregion
        
    }
}
