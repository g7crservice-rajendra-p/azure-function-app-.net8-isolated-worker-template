using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.Write;
using QidWorkerRole.SIS.Model;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    [FixedLengthRecord()]
    public class RMVatRecord : VatRecordBase, IRecordToClassConverter<List<RMVAT>>, IClassToRecordConverter<List<RMVAT>>
    {
       
        [FieldFixedLength(11)]
        public string RejectionMemoNumber = string.Empty;

        [FieldFixedLength(4)]
        public string Filler3;

        [FieldFixedLength(7)]
        public string Filler4;

        [FieldFixedLength(1)]
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

        public RMVatRecord() { }

        /// <summary>
        ///  To set the common base properties
        /// </summary>
        /// <param name="rejectionMemoRecord"></param>
        public RMVatRecord(RejectionMemoRecord rejectionMemoRecord)
            : base(rejectionMemoRecord)
        {
            this.RejectionMemoNumber = rejectionMemoRecord.RejectionMemoNumber;
            this.BillingCode = rejectionMemoRecord.BillingCode;
        }

        #region Implementation of IModelConverter<RejectionMemoVat>

        /// <summary>
        /// Converts it into mode instance.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine instance.</param>
        /// <returns>Returns mode instance created from record.</returns>
        public List<RMVAT> ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Debug("Converting RMVAT record to model instance.");

            // Create new RMVAT List object.
            var rMVATList = CreateRMVATList();

            ProcessNextRecord(multiRecordEngine, rMVATList);

            return rMVATList;
        }

        /// <summary>
        /// Converts child records of current record into model classes and adds them to the current mode record.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine instance</param>
        /// <param name="rMVATList">Parent model record to which all the child records will added.</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, List<RMVAT> rMVATList)
        {
            multiRecordEngine.ReadNext();
        }

        #endregion

        /// <summary>
        /// This method creates RMVAT List object for all the Vat records in Idec record read from the input file.
        /// </summary>
        /// <returns>RMVAT List object.</returns>
        private List<RMVAT> CreateRMVATList()
        {
            var rMVATList = new List<RMVAT>();

            //Logger.Debug("Creating RMVAT list.");

            if (!string.IsNullOrEmpty(VatIdentifier1.Trim()))
            {
                rMVATList.Add(new RMVAT()
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
                rMVATList.Add(new RMVAT
                {
                    VatIdentifier = VatIdentifier2.Trim(),
                    VatLabel = VatLabel2.Trim(),
                    VatText = VatText2.Trim(),
                    VatBaseAmount = Utilities.GetActualValueForDouble(VatBaseAmountSign2, VatBaseAmount2),
                    VatPercentage = Utilities.GetActualValueForDouble(VatPercentageSign2, VatPercentage2),
                    VatCalculatedAmount = Utilities.GetActualValueForDouble(VatCalculatedAmountSign2, VatCalculatedAmount2)
                });
            }

            //Logger.Debug("RMVAT list created successfully.");

            return rMVATList;
        }

        /// <summary>
        /// This method converts the RMVAT model into RMVATRecord instance 
        /// </summary>
        public void ConvertClassToRecord(List<RMVAT> rMVATList, ref long recordSequenceNumber)
        {
            //Logger.Debug("Converting RMVAT model to RMVatRecord record instance.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiVatRecord;

            if (rMVATList.Count > 0)
            {
                VatIdentifier1 = rMVATList[0].VatIdentifier != null ? rMVATList[0].VatIdentifier : "NL";
                VatLabel1 = rMVATList[0].VatLabel;
                VatText1 = rMVATList[0].VatText;
                VatBaseAmount1 = Math.Abs(rMVATList[0].VatBaseAmount).ToString();
                VatBaseAmountSign1 = Utilities.GetSignValue(rMVATList[0].VatBaseAmount);
                VatPercentage1 = Math.Abs(rMVATList[0].VatPercentage).ToString();
                VatPercentageSign1 = Utilities.GetSignValue(rMVATList[0].VatPercentage);
                VatCalculatedAmount1 = Math.Abs(rMVATList[0].VatCalculatedAmount).ToString();
                VatCalculatedAmountSign1 = Utilities.GetSignValue(rMVATList[0].VatCalculatedAmount);

                if (rMVATList.Count > 1)
                {
                    VatIdentifier2 = rMVATList[1].VatIdentifier != null ? rMVATList[1].VatIdentifier : "NL";
                    VatLabel2 = rMVATList[1].VatLabel;
                    VatText2 = rMVATList[1].VatText;
                    VatBaseAmount2 = Math.Abs(rMVATList[1].VatBaseAmount).ToString();
                    VatBaseAmountSign2 = Utilities.GetSignValue(rMVATList[1].VatBaseAmount);
                    VatPercentage2 = Math.Abs(rMVATList[1].VatPercentage).ToString();
                    VatPercentageSign2 = Utilities.GetSignValue(rMVATList[1].VatPercentage);
                    VatCalculatedAmount2 = Math.Abs(rMVATList[1].VatCalculatedAmount).ToString();
                    VatCalculatedAmountSign2 = Utilities.GetSignValue(rMVATList[1].VatCalculatedAmount);
                }
            }
            ProcessNextClass(rMVATList, ref recordSequenceNumber);
        }

        /// <summary>
        /// This method adds the child records of RMVAT record to corresponding list by calling respective functions.
        /// </summary>
        /// <param name="rMVATList"></param>
        /// <param name="recordSequenceNumber"></param>
        public void ProcessNextClass(List<RMVAT> rMVATList, ref long recordSequenceNumber)
        {
            //Logger.Debug("Processing RMVAT model child objects.");
        }
    }
}