using System;
using System.Collections.Generic;
using System.Reflection;
using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.FileHandling;
using QidWorkerRole.SIS.Model;

using QidWorkerRole.SIS.FileHandling.Idec.Write;
using QidWorkerRole.SIS.FileHandling.Idec.Read;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    [FixedLengthRecord()]
    public class AWBVatBreakdownRecord : VatRecordBase, IClassToRecordConverter<List<AWBVAT>>, IRecordToClassConverter<List<AWBVAT>>
    {
       
        #region Record Properties

        [FieldFixedLength(11)]
        public string Filler2;

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
        public string Filler4;

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
        public string Filler5;

        #endregion

        #region Constructors

        #region Parameterless Constructor

        public AWBVatBreakdownRecord() { }

        #endregion

        #region Parameterized Constructor

        public AWBVatBreakdownRecord(AWBRecord aWBRecord)
            : base(aWBRecord)
        {
            AWBIssuingAirline = aWBRecord.AWBIssuingAirline;
            AWBSerialNumber = aWBRecord.AWBSerialNumber;
            AWBCheckDigit = aWBRecord.AWBCheckDigit;
            BillingCode = aWBRecord.BillingCode;
        }

        #endregion

        #endregion

        #region Implementation of IClassToRecordConverter<List<AWBVAT>> To Write IDEC File.

        /// <summary>
        /// To Convert AWBVAT into AWBVatBreakdownRecord.
        /// </summary>
        /// <param name="aWBVATList">List of AWBVAT</param>
        /// <param name="recordSequenceNumber">Record Sequence Number</param>
        public void ConvertClassToRecord(List<AWBVAT> aWBVATList, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting AWBVAT into AWBVatBreakdownRecord.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiVatRecord;

            // For VatIdentifier 1 if available.
            if (aWBVATList.Count > 0)
            {
                VatIdentifier1 = aWBVATList[0].VatIdentifier != null ? aWBVATList[0].VatIdentifier : "NL";
                VatLabel1 = aWBVATList[0].VatLabel;
                VatText1 = aWBVATList[0].VatText;
                VatBaseAmount1 = aWBVATList[0].VatBaseAmount.ToString();
                VatBaseAmountSign1 = Utilities.GetSignValue(aWBVATList[0].VatBaseAmount);
                VatPercentage1 = aWBVATList[0].VatPercentage.ToString();
                VatPercentageSign1 = Utilities.GetSignValue(aWBVATList[0].VatPercentage);
                VatCalculatedAmount1 = aWBVATList[0].VatCalculatedAmount.ToString();
                VatCalculatedAmountSign1 = Utilities.GetSignValue(aWBVATList[0].VatCalculatedAmount);

                // For VatIdentifier 2 if available.
                if (aWBVATList.Count > 1)
                {
                    VatIdentifier2 = aWBVATList[1].VatIdentifier != null ? aWBVATList[1].VatIdentifier : "NL";
                    VatLabel2 = aWBVATList[1].VatLabel;
                    VatText2 = aWBVATList[1].VatText;
                    VatBaseAmount2 = aWBVATList[1].VatBaseAmount.ToString();
                    VatBaseAmountSign2 = Utilities.GetSignValue(aWBVATList[1].VatBaseAmount);
                    VatPercentage2 = aWBVATList[1].VatPercentage.ToString();
                    VatPercentageSign2 = Utilities.GetSignValue(aWBVATList[1].VatPercentage);
                    VatCalculatedAmount2 = aWBVATList[1].VatCalculatedAmount.ToString();
                    VatCalculatedAmountSign2 = Utilities.GetSignValue(aWBVATList[1].VatCalculatedAmount);
                }
            }

            ProcessNextClass(aWBVATList, ref recordSequenceNumber);
            
            //Logger.Info("End of Converting AWBVAT into AWBVatBreakdownRecord.");
        }

        /// <summary>
        /// To Convert Childs of AWBVAT into there corresponding Records.
        /// </summary>
        /// <param name="aWBVATList">List of AWBVAT</param>
        /// <param name="recordSequenceNumber">Record Sequence Number</param>
        public void ProcessNextClass(List<AWBVAT> aWBVATList, ref long recordSequenceNumber)
        {
            //Logger.Info("AWBVAT cannot have Childs.");
        }

        #endregion

        #region Implementation of IRecordToClassConverter<List<AWBVAT>> To Read IDEC File.

        /// <summary>
        /// To Convert AWBVatBreakdownRecord into AWBVAT.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <returns>List of AWBVAT</returns>
        public List<AWBVAT> ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Info("Start of Converting AWBVatBreakdownRecord into AWBVAT.");

            var aWBVATList = CreateListOfAWBVAT();

            ProcessNextRecord(multiRecordEngine, aWBVATList);

            //Logger.Info("End of Converting AWBVatBreakdownRecord into AWBVAT.");

            return aWBVATList;
        }

        /// <summary>
        /// Creates List of AWBVAT for all the AWBVatBreakdownRecord present in the reading input IDEC File.
        /// </summary>
        /// <returns>List of AWBVAT.</returns>
        private List<AWBVAT> CreateListOfAWBVAT()
        {
            //Logger.Info("Start of Creating AWBVAT Class from AWBVatBreakdownRecord.");

            var aWBVATList = new List<AWBVAT>();

            // VatIdentifier 1
            if (!string.IsNullOrEmpty(VatIdentifier1.Trim()))
            {
                aWBVATList.Add(new AWBVAT
                {
                    VatIdentifier = VatIdentifier1.Trim(),
                    VatLabel = VatLabel1.Trim(),
                    VatText = VatText1.Trim(),
                    VatBaseAmount = Utilities.GetActualValueForDouble(VatBaseAmountSign1, VatBaseAmount1),
                    VatPercentage = Utilities.GetActualValueForDouble(VatPercentageSign1, VatPercentage1),
                    VatCalculatedAmount = Utilities.GetActualValueForDouble(VatCalculatedAmountSign1, VatCalculatedAmount1)
                });
            }

            // VatIdentifier 2
            if (!string.IsNullOrEmpty(VatIdentifier2.Trim()))
            {
                aWBVATList.Add(new AWBVAT
                {
                    VatIdentifier = VatIdentifier2.Trim(),
                    VatLabel = VatLabel2.Trim(),
                    VatText = VatText2.Trim(),
                    VatBaseAmount = Utilities.GetActualValueForDouble(VatBaseAmountSign2, VatBaseAmount2),
                    VatPercentage = Utilities.GetActualValueForDouble(VatPercentageSign2, VatPercentage2),
                    VatCalculatedAmount = Utilities.GetActualValueForDouble(VatCalculatedAmountSign2, VatCalculatedAmount2)
                });
            }

            //Logger.Info("End of Creating AWBVAT Class from AWBVatBreakdownRecord.");

            return aWBVATList;
        }

        /// <summary>
        /// To Convert Child records of AWBVatBreakdownRecord into there corresponding Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="aWBOtherChargesList">List of AWBVAT</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, List<AWBVAT> aWBVATList)
        {
            multiRecordEngine.ReadNext();
            //Logger.Info("AWBVatBreakdownRecord cannot have Childs.");
        }        

        #endregion
    
    }
}