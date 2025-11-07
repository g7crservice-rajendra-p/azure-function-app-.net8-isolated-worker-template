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
    public class BMVatRecord : VatRecordBase, IRecordToClassConverter<List<BMVAT>>, IClassToRecordConverter<List<BMVAT>>
    {
        
        [FieldFixedLength(11)]
        public string BillingMemoNumber = string.Empty;

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

        public BMVatRecord()
        { }

        public BMVatRecord(BillingMemoRecord billingMemoRecord)
            : base(billingMemoRecord)
        {
            BillingMemoNumber = billingMemoRecord.BillingOrCreditMemoNumber;
            BillingCode = billingMemoRecord.BillingCode;
        }

        /// <summary>
        /// converts it into mode instance.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine instance.</param>
        /// <returns>Returns mode instance created from record.</returns>
        public List<BMVAT> ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            var billingMemoVatList = CreateBillingMemoVatList();

            ProcessNextRecord(multiRecordEngine, billingMemoVatList);

            return billingMemoVatList;
        }

        /// <summary>
        /// Converts child records of current record into model classes and adds them to the current mode record.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine instance</param>
        /// <param name="bMVATList">Parent model record to which all the child records will added.</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, List<BMVAT> bMVATList)
        {
            multiRecordEngine.ReadNext();
        }

        /// <summary>
        /// This method creates BMVAT List object.
        /// </summary>
        /// <returns>List of BMVAT object.</returns>
        private List<BMVAT> CreateBillingMemoVatList()
        {
            var bMVATList = new List<BMVAT>();

            if (!string.IsNullOrEmpty(VatIdentifier1.Trim()))
            {
                bMVATList.Add(new BMVAT
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
                bMVATList.Add(new BMVAT
                {
                    VatIdentifier = VatIdentifier2.Trim(),
                    VatLabel = VatLabel2.Trim(),
                    VatText = VatText2.Trim(),
                    VatBaseAmount = Utilities.GetActualValueForDouble(VatBaseAmountSign2, VatBaseAmount2),
                    VatPercentage = Utilities.GetActualValueForDouble(VatPercentageSign2, VatPercentage2),
                    VatCalculatedAmount = Utilities.GetActualValueForDouble(VatCalculatedAmountSign2, VatCalculatedAmount2)
                });
            }

            return bMVATList;
        }

        /// <summary>
        ///  This method converts the BMVAT model into BMVAT record instance 
        /// </summary>
        public void ConvertClassToRecord(List<BMVAT> bMVATList, ref long recordSequenceNumber)
        {
            //Logger.Debug("Converting BMVAT model to BMVAT record instance.");
            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiVatRecord;

            if (bMVATList.Count > 0)
            {
                VatIdentifier1 = bMVATList[0].VatIdentifier != null ? bMVATList[0].VatIdentifier : "NL";
                VatLabel1 = bMVATList[0].VatLabel;
                VatText1 = bMVATList[0].VatText;
                VatBaseAmount1 = bMVATList[0].VatBaseAmount.ToString();
                VatBaseAmountSign1 = Utilities.GetSignValue(bMVATList[0].VatBaseAmount);
                VatPercentage1 = bMVATList[0].VatPercentage.ToString();
                VatPercentageSign1 = Utilities.GetSignValue(bMVATList[0].VatPercentage);
                VatCalculatedAmount1 = bMVATList[0].VatCalculatedAmount.ToString();
                VatCalculatedAmountSign1 = Utilities.GetSignValue(bMVATList[0].VatCalculatedAmount);

                if (bMVATList.Count > 1)
                {
                    VatIdentifier2 = bMVATList[1].VatIdentifier != null ? bMVATList[1].VatIdentifier : "NL";
                    VatLabel2 = bMVATList[1].VatLabel;
                    VatText2 = bMVATList[1].VatText;
                    VatBaseAmount2 = bMVATList[1].VatBaseAmount.ToString();
                    VatBaseAmountSign2 = Utilities.GetSignValue(bMVATList[1].VatBaseAmount);
                    VatPercentage2 = bMVATList[1].VatPercentage.ToString();
                    VatPercentageSign2 = Utilities.GetSignValue(bMVATList[1].VatPercentage);
                    VatCalculatedAmount2 = bMVATList[1].VatCalculatedAmount.ToString();
                    VatCalculatedAmountSign2 = Utilities.GetSignValue(bMVATList[1].VatCalculatedAmount);
                }
            }
            ProcessNextClass(bMVATList, ref recordSequenceNumber);
        }

        /// <summary>
        /// This method adds the child records of BMVAT record to corresponding list by calling respective functions.
        /// </summary>
        /// <param name="bMVATList"></param>
        /// <param name="recordSequenceNumber"></param>
        public void ProcessNextClass(List<BMVAT> bMVATList, ref long recordSequenceNumber)
        {
        }
    }
}