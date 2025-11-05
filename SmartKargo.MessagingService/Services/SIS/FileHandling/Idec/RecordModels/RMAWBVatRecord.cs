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
    public class RMAWBVatRecord : VatRecordBase, IRecordToClassConverter<List<RMAWBVAT>>, IClassToRecordConverter<List<RMAWBVAT>>
    {
        
        [FieldFixedLength(11)]
        public string RejectionMemoNumber = string.Empty;

        [FieldFixedLength(4), FieldConverter(typeof(PaddingConverter), 4, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string AWBIssuingAirline;

        [FieldFixedLength(7), FieldConverter(typeof(PaddingConverter), 7, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string AWBSerialNumber;

        [FieldFixedLength(1), FieldConverter(typeof(PaddingConverter), 1, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string AWBCheckDigit;

        [FieldFixedLength(50)]
        public string Filler3;

        [FieldFixedLength(2)]
        public string VatIdentifier1 = string.Empty;

        [FieldFixedLength(5)]
        public string VatLabel1 = string.Empty;

        [FieldFixedLength(50)]
        public string VatText1 = string.Empty;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatBaseAmount1 = string.Empty;

        [FieldFixedLength(1)]
        public string VatBaseAmountSign1 = string.Empty;

        [FieldFixedLength(5), FieldConverter(typeof(DoubleNumberConverter), 5, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatPercentage1 = string.Empty;

        [FieldFixedLength(1)]
        public string VatPercentageSign1 = string.Empty;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatCalculatedAmount1 = string.Empty;

        [FieldFixedLength(1)]
        public string VatCalculatedAmountSign1 = string.Empty;

        [FieldFixedLength(50)]
        public string Filler4 = string.Empty;

        [FieldFixedLength(2)]
        public string VatIdentifier2 = string.Empty;

        [FieldFixedLength(5)]
        public string VatLabel2 = string.Empty;

        [FieldFixedLength(50)]
        public string VatText2 = string.Empty;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatBaseAmount2 = "0";

        [FieldFixedLength(1)]
        public string VatBaseAmountSign2 = string.Empty;

        [FieldFixedLength(5), FieldConverter(typeof(DoubleNumberConverter), 5, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatPercentage2 = "0";

        [FieldFixedLength(1)]
        public string VatPercentageSign2 = string.Empty;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatCalculatedAmount2 = "0";

        [FieldFixedLength(1)]
        public string VatCalculatedAmountSign2 = string.Empty;

        [FieldFixedLength(167)]
        public string Filler5;

        public RMAWBVatRecord() { }

        /// <summary>
        /// To set the common base properties
        /// </summary>
        /// <param name="rMAWBRecord"></param>
        public RMAWBVatRecord(RMAWBRecord rMAWBRecord)
            : base(rMAWBRecord)
        {
            RejectionMemoNumber = rMAWBRecord.RejectionMemoNumber;
            AWBIssuingAirline = rMAWBRecord.AWBIssuingAirline;
            AWBSerialNumber = rMAWBRecord.AWBSerialNumber;
            AWBCheckDigit = rMAWBRecord.AWBCheckDigit;
            BillingCode = rMAWBRecord.BillingCode;
        }

        #region Implementation of IModelConverter<RMAWBVAT>

        /// <summary>
        /// Converts into mode instance.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine instance.</param>
        /// <returns>Returns mode instance created from record.</returns>
        public List<RMAWBVAT> ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Debug("Converting RMAWBVAT record to model instance.");

            // Create new RMAWBVAT List object.
            var rMAWBVATList = CreateRMAWBVATList();

            ProcessNextRecord(multiRecordEngine, rMAWBVATList);

            return rMAWBVATList;
        }

        /// <summary>
        /// Converts child records of RMAWBVAT record into model classes and adds them to the current mode record.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine instance</param>
        /// <param name="rMAWBVATList">Parent model record to which all the child records will added.</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, List<RMAWBVAT> rMAWBVATList)
        {
            multiRecordEngine.ReadNext();
        }

        #endregion

        /// <summary>
        /// This method creates RMAWBVAT List object for all the Vat records in Idec record read from the input file.
        /// </summary>
        /// <returns>List of RMAWBVAT List object.</returns>
        private List<RMAWBVAT> CreateRMAWBVATList()
        {
            var rMAWBVATList = new List<RMAWBVAT>();
            //Logger.Debug("Creating RMAWBVAT object.");

            if (!string.IsNullOrEmpty(VatIdentifier1.Trim()))
            {
                rMAWBVATList.Add(new RMAWBVAT
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
                rMAWBVATList.Add(new RMAWBVAT
                {
                    VatIdentifier = VatIdentifier2.Trim(),
                    VatLabel = VatLabel2.Trim(),
                    VatText = VatText2.Trim(),
                    VatBaseAmount = Utilities.GetActualValueForDouble(VatBaseAmountSign2, VatBaseAmount2),
                    VatPercentage = Utilities.GetActualValueForDouble(VatPercentageSign2, VatPercentage2),
                    VatCalculatedAmount = Utilities.GetActualValueForDouble(VatCalculatedAmountSign2, VatCalculatedAmount2)
                });
            }
            //Logger.Debug("RMAWBVAT object created successfully.");

            return rMAWBVATList;
        }

        /// <summary>
        /// This method converts the RMAWBVAT model into corresponding record instance 
        /// </summary>
        /// <param name="rMAWBVATList"></param>
        /// <param name="recordSequenceNumber"></param>
        public void ConvertClassToRecord(List<RMAWBVAT> rMAWBVATList, ref long recordSequenceNumber)
        {
            //Logger.Debug("Converting RMAWBVAT model to RMAWBVATRecord instance.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiVatRecord;

            if (rMAWBVATList.Count > 0)
            {
                VatIdentifier1 = rMAWBVATList[0].VatIdentifier != null ? rMAWBVATList[0].VatIdentifier : "NL";
                VatLabel1 = rMAWBVATList[0].VatLabel;
                VatText1 = rMAWBVATList[0].VatText;
                VatBaseAmount1 = rMAWBVATList[0].VatBaseAmount.ToString();
                VatBaseAmountSign1 = Utilities.GetSignValue(rMAWBVATList[0].VatBaseAmount);
                VatPercentage1 = rMAWBVATList[0].VatPercentage.ToString();
                VatPercentageSign1 = Utilities.GetSignValue(rMAWBVATList[0].VatPercentage);
                VatCalculatedAmount1 = rMAWBVATList[0].VatCalculatedAmount.ToString();
                VatCalculatedAmountSign1 = Utilities.GetSignValue(rMAWBVATList[0].VatCalculatedAmount);

                if (rMAWBVATList.Count > 1)
                {
                    VatIdentifier2 = rMAWBVATList[1].VatIdentifier != null ? rMAWBVATList[1].VatIdentifier : "NL";
                    VatLabel2 = rMAWBVATList[1].VatLabel;
                    VatText2 = rMAWBVATList[1].VatText;
                    VatBaseAmount2 = rMAWBVATList[1].VatBaseAmount.ToString();
                    VatBaseAmountSign2 = Utilities.GetSignValue(rMAWBVATList[1].VatBaseAmount);
                    VatPercentage2 = rMAWBVATList[1].VatPercentage.ToString();
                    VatPercentageSign2 = Utilities.GetSignValue(rMAWBVATList[1].VatPercentage);
                    VatCalculatedAmount2 = rMAWBVATList[1].VatCalculatedAmount.ToString();
                    VatCalculatedAmountSign2 = Utilities.GetSignValue(rMAWBVATList[1].VatCalculatedAmount);
                }
            }
            ProcessNextClass(rMAWBVATList, ref recordSequenceNumber);
        }

        /// <summary>
        /// This method adds the child records of RMAWBVATRecord to corresponding list by calling respective functions.
        /// </summary>
        /// <param name="rMAWBVATList"></param>
        /// <param name="recordSequenceNumber"></param>
        public void ProcessNextClass(List<RMAWBVAT> rMAWBVATList, ref long recordSequenceNumber)
        {
            //Logger.Debug("Processing RMAWBVAT model child objects.");
        }
    }
}