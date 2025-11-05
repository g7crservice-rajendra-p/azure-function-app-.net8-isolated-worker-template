using System.Collections.Generic;
using System.Reflection;
using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.Model;

using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.Write;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    [FixedLengthRecord()]
    public class CMVatRecord : VatRecordBase, IRecordToClassConverter<List<CMVAT>>, IClassToRecordConverter<List<CMVAT>>
    {
        
        [FieldFixedLength(11)]
        public string CreditMemoNumber = string.Empty;

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

        public CMVatRecord()
        { }

        public CMVatRecord(CreditMemoRecord creditMemoRecord)
            : base(creditMemoRecord)
        {
            CreditMemoNumber = creditMemoRecord.BillingOrCreditMemoNumber;
            BillingCode = creditMemoRecord.BillingCode;
        }

        #region Implementation of IModelConverter<CMVAT>

        /// <summary>
        /// Converts it into mode instance.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine instance.</param>
        /// <returns>Returns mode instance created from record.</returns>
        public List<CMVAT> ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            var creditMemoVats = CreateCreditMemoVatList();

            ProcessNextRecord(multiRecordEngine, creditMemoVats);

            return creditMemoVats;
        }

        /// <summary>
        /// Converts child records of current record into model classes and adds them to the current mode record.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine instance</param>
        /// <param name="cMVATList">Parent model record to which all the child records will added.</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, List<CMVAT> cMVATList)
        {
            multiRecordEngine.ReadNext();
        }

        #endregion

        /// <summary>
        /// This method creates Credit Memo Vat List object for all the Vat records in Idec record read from the input file.
        /// </summary>
        /// <returns>List of Credit Memo Vat List object.</returns>
        private List<CMVAT> CreateCreditMemoVatList()
        {
            var cMVATList = new List<CMVAT>();
            //Logger.Debug("Creating CMVAT list for Credit Memo object.");

            if (!string.IsNullOrEmpty(VatIdentifier1.Trim()))
            {
                cMVATList.Add(new CMVAT
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
                cMVATList.Add(new CMVAT
                {
                    VatIdentifier = VatIdentifier2.Trim(),
                    VatLabel = VatLabel2.Trim(),
                    VatText = VatText2.Trim(),
                    VatBaseAmount = Utilities.GetActualValueForDouble(VatBaseAmountSign2, VatBaseAmount2),
                    VatPercentage = Utilities.GetActualValueForDouble(VatPercentageSign2, VatPercentage2),
                    VatCalculatedAmount = Utilities.GetActualValueForDouble(VatCalculatedAmountSign2, VatCalculatedAmount2)
                });
            }
            return cMVATList;
        }

        public void ConvertClassToRecord(List<CMVAT> cMVATList, ref long recordSequenceNumber)
        {
            //Logger.Debug("Converting CMVAT model to CMVAT Record record instance.");
            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiVatRecord;

            if (cMVATList.Count > 0)
            {
                VatIdentifier1 = cMVATList[0].VatIdentifier != null ? cMVATList[0].VatIdentifier : "NL";
                VatLabel1 = cMVATList[0].VatLabel;
                VatText1 = cMVATList[0].VatText;
                VatBaseAmount1 = cMVATList[0].VatBaseAmount.ToString();
                VatBaseAmountSign1 = Utilities.GetSignValue(cMVATList[0].VatBaseAmount);
                VatPercentage1 = cMVATList[0].VatPercentage.ToString();
                VatPercentageSign1 = Utilities.GetSignValue(cMVATList[0].VatPercentage);
                VatCalculatedAmount1 = cMVATList[0].VatCalculatedAmount.ToString();
                VatCalculatedAmountSign1 = Utilities.GetSignValue(cMVATList[0].VatCalculatedAmount);

                if (cMVATList.Count > 1)
                {
                    VatIdentifier2 = cMVATList[1].VatIdentifier != null ? cMVATList[1].VatIdentifier : "NL";
                    VatLabel2 = cMVATList[1].VatLabel;
                    VatText2 = cMVATList[1].VatText;
                    VatBaseAmount2 = cMVATList[1].VatBaseAmount.ToString();
                    VatBaseAmountSign2 = Utilities.GetSignValue(cMVATList[1].VatBaseAmount);
                    VatPercentage2 = cMVATList[1].VatPercentage.ToString();
                    VatPercentageSign2 = Utilities.GetSignValue(cMVATList[1].VatPercentage);
                    VatCalculatedAmount2 = cMVATList[1].VatCalculatedAmount.ToString();
                    VatCalculatedAmountSign2 = Utilities.GetSignValue(cMVATList[1].VatCalculatedAmount);
                }
            }
            ProcessNextClass(cMVATList, ref recordSequenceNumber);
        }

        public void ProcessNextClass(List<CMVAT> cMVATList, ref long recordSequenceNumber)
        {
        }
    }
}