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
    [FixedLengthRecord]
    public class BMAWBVatRecord : VatRecordBase, IRecordToClassConverter<List<BMAWBVAT>>, IClassToRecordConverter<List<BMAWBVAT>>
    {
        
        [FieldFixedLength(11)]
        public string BillingMemoNumber = string.Empty;

        [FieldFixedLength(4), FieldConverter(typeof(PaddingConverter), 4, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string AWBIssuingAirline;

        [FieldFixedLength(7), FieldConverter(typeof(PaddingConverter), 7, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string AWBSerialNumber;

        [FieldFixedLength(1), FieldConverter(typeof(PaddingConverter), 1, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string AWBCheckDigit;

        [FieldFixedLength(50)]
        public string Filler3;

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

        public BMAWBVatRecord()
        { }

        public BMAWBVatRecord(BMAWBRecord bMAWBRecord)
            : base(bMAWBRecord)
        {
            BillingMemoNumber = bMAWBRecord.BillingCreditMemoNumber;
            AWBIssuingAirline = bMAWBRecord.AWBIssuingAirline;
            AWBSerialNumber = bMAWBRecord.AWBSerialNumber;
            AWBCheckDigit = bMAWBRecord.CheckDigit;
            BillingCode = bMAWBRecord.BillingCode;
        }

        /// <summary>
        /// converts it into mode instance.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine instance.</param>
        /// <returns>Returns mode instance created from record.</returns>
        public List<BMAWBVAT> ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            var bmawbVatList = CreateBMAWBVatList();

            ProcessNextRecord(multiRecordEngine, bmawbVatList);

            return bmawbVatList;
        }

        /// <summary>
        /// Converts child records of current record into model classes and adds them to the current mode record.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine instance</param>
        /// <param name="bMAWBVATList">Parent model record to which all the child records will added.</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, List<BMAWBVAT> bMAWBVATList)
        {
            multiRecordEngine.ReadNext();
        }

        /// <summary>
        /// This method creates BMAWBVAT List object.
        /// </summary>
        /// <returns>List of BMAWBVAT object.</returns>
        private List<BMAWBVAT> CreateBMAWBVatList()
        {
            var bMAWBVATList = new List<BMAWBVAT>();

            if (!string.IsNullOrEmpty(VatIdentifier1.Trim()))
            {
                bMAWBVATList.Add(new BMAWBVAT
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
                bMAWBVATList.Add(new BMAWBVAT
                {
                    VatIdentifier = VatIdentifier2.Trim(),
                    VatLabel = VatLabel2.Trim(),
                    VatText = VatText2.Trim(),
                    VatBaseAmount = Utilities.GetActualValueForDouble(VatBaseAmountSign2, VatBaseAmount2),
                    VatPercentage = Utilities.GetActualValueForDouble(VatPercentageSign2, VatPercentage2),
                    VatCalculatedAmount = Utilities.GetActualValueForDouble(VatCalculatedAmountSign2, VatCalculatedAmount2)
                });
            }
            //Logger.Debug("Vat Object list for Billing Memo object created successfully.");
            return bMAWBVATList;
        }

        /// <summary>
        ///  This method converts the BillingMemo model into corresponding record instance 
        /// </summary>
        public void ConvertClassToRecord(List<BMAWBVAT> bMAWBVATList, ref long recordSequenceNumber)
        {
            //Logger.Debug("Converting BMAWBVAT model to BMAWBVAT Record instance.");
            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiVatRecord;

            if (bMAWBVATList.Count > 0)
            {
                VatIdentifier1 = bMAWBVATList[0].VatIdentifier != null ? bMAWBVATList[0].VatIdentifier : "NL";
                VatLabel1 = bMAWBVATList[0].VatLabel;
                VatText1 = bMAWBVATList[0].VatText;
                VatBaseAmount1 = bMAWBVATList[0].VatBaseAmount.ToString();
                VatBaseAmountSign1 = Utilities.GetSignValue(bMAWBVATList[0].VatBaseAmount);
                VatPercentage1 = bMAWBVATList[0].VatPercentage.ToString();
                VatPercentageSign1 = Utilities.GetSignValue(bMAWBVATList[0].VatPercentage);
                VatCalculatedAmount1 = bMAWBVATList[0].VatCalculatedAmount.ToString();
                VatCalculatedAmountSign1 = Utilities.GetSignValue(bMAWBVATList[0].VatCalculatedAmount);

                if (bMAWBVATList.Count > 1)
                {
                    VatIdentifier2 = bMAWBVATList[1].VatIdentifier != null ? bMAWBVATList[1].VatIdentifier : "NL";
                    VatLabel2 = bMAWBVATList[1].VatLabel;
                    VatText2 = bMAWBVATList[1].VatText;
                    VatBaseAmount2 = bMAWBVATList[1].VatBaseAmount.ToString();
                    VatBaseAmountSign2 = Utilities.GetSignValue(bMAWBVATList[1].VatBaseAmount);
                    VatPercentage2 = bMAWBVATList[1].VatPercentage.ToString();
                    VatPercentageSign2 = Utilities.GetSignValue(bMAWBVATList[1].VatPercentage);
                    VatCalculatedAmount2 = bMAWBVATList[1].VatCalculatedAmount.ToString();
                    VatCalculatedAmountSign2 = Utilities.GetSignValue(bMAWBVATList[1].VatCalculatedAmount);
                }
            }
            ProcessNextClass(bMAWBVATList, ref recordSequenceNumber);
        }

        /// <summary>
        /// This method adds the child records of BMAWBVAT record to corresponding list by calling respective functions.
        /// </summary>
        /// <param name="bMAWBVATList"></param>
        /// <param name="recordSequenceNumber"></param>
        public void ProcessNextClass(List<BMAWBVAT> bMAWBVATList, ref long recordSequenceNumber)
        {
            //Logger.Debug("Processing BMAWBVAT model child objects.");
        }
    }
}