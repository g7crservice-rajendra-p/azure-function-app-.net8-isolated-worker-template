using System;
using System.Collections.Generic;
using System.Reflection;
using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.Model;

using QidWorkerRole.SIS.FileHandling.Idec.Write;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    [FixedLengthRecord()]
    public class AWBOCBreakdownRecord : InvoiceRecordBase, IClassToRecordConverter<List<AWBOtherCharges>>, IRecordToClassConverter<List<AWBOtherCharges>>
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

        [FieldFixedLength(14)]
        public string Filler3;

        [FieldFixedLength(2)]
        public string OtherChargeCode1;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string OtherChargeCodeValue1;

        [FieldFixedLength(1)]
        public string OtherChargeCodeValueSign1;

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

        [FieldFixedLength(2)]
        public string OtherChargeCode2;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string OtherChargeCodeValue2;

        [FieldFixedLength(1)]
        public string OtherChargeCodeValueSign2;

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

        [FieldFixedLength(2)]
        public string OtherChargeCode3;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string OtherChargeCodeValue3;

        [FieldFixedLength(1)]
        public string OtherChargeCodeValueSign3;

        [FieldFixedLength(5)]
        public string VatLabel3;

        [FieldFixedLength(50)]
        public string VatText3;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatBaseAmount3;

        [FieldFixedLength(1)]
        public string VatBaseAmountSign3;

        [FieldFixedLength(5), FieldConverter(typeof(DoubleNumberConverter), 5, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatPercentage3;

        [FieldFixedLength(1)]
        public string VatPercentageSign3;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatCalculatedAmount3;

        [FieldFixedLength(1)]
        public string VatCalculatedAmountSign3;

        [FieldFixedLength(130)]
        public string Filler4;

        #endregion

        #region Constructors

        #region Parameterless Constructor

        public AWBOCBreakdownRecord()
        { }

        #endregion

        #region Parameterized Constructor

        public AWBOCBreakdownRecord(AWBRecord aWBRecord)
            : base(aWBRecord)
        {
            AWBIssuingAirline = aWBRecord.AWBIssuingAirline;
            AWBSerialNumber = aWBRecord.AWBSerialNumber;
            AWBCheckDigit = aWBRecord.AWBCheckDigit;
            BillingCode = aWBRecord.BillingCode;
        }

        #endregion

        #endregion

        #region Implementation of IClassToRecordConverter<List<AWBOtherCharges>> To Write IDEC File.

        /// <summary>
        /// To Convert AWBOtherCharges into AWBOCBreakdownRecord.
        /// </summary>
        /// <param name="aWBOtherChargesList">List of AWBOtherCharges</param>
        /// <param name="recordSequenceNumber">Record Sequence Number</param>
        public void ConvertClassToRecord(List<AWBOtherCharges> aWBOtherChargesList, ref long recordSequenceNumber)
        {
             //Logger.Info("Start of Converting AWBOtherCharges into AWBOCBreakdownRecord.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiOcRecord;

            // For Other Charge Code 1 if available.
            if (aWBOtherChargesList.Count > 0)
            {
                OtherChargeCode1 = aWBOtherChargesList[0].OtherChargeCode;
                VatLabel1 = aWBOtherChargesList[0].OtherChargeVatLabel;
                VatText1 = aWBOtherChargesList[0].OtherChargeVatText;
                VatCalculatedAmountSign1 = Utilities.GetSignValue(aWBOtherChargesList[0].OtherChargeVatCalculatedAmount);
                VatCalculatedAmount1 = aWBOtherChargesList[0].OtherChargeVatCalculatedAmount.HasValue ? Math.Abs(aWBOtherChargesList[0].OtherChargeVatCalculatedAmount.Value).ToString() : null;
                VatPercentageSign1 = Utilities.GetSignValue(aWBOtherChargesList[0].OtherChargeVatPercentage);
                VatPercentage1 = aWBOtherChargesList[0].OtherChargeVatPercentage.HasValue ? Math.Abs(aWBOtherChargesList[0].OtherChargeVatPercentage.Value).ToString() : null;
                VatBaseAmountSign1 = Utilities.GetSignValue(aWBOtherChargesList[0].OtherChargeVatBaseAmount);
                VatBaseAmount1 = aWBOtherChargesList[0].OtherChargeVatBaseAmount.HasValue ? Math.Abs(aWBOtherChargesList[0].OtherChargeVatBaseAmount.Value).ToString() : null;
                OtherChargeCodeValueSign1 = Utilities.GetSignValue(aWBOtherChargesList[0].OtherChargeCodeValue);
                OtherChargeCodeValue1 = aWBOtherChargesList[0].OtherChargeCodeValue.HasValue ? Math.Abs(aWBOtherChargesList[0].OtherChargeCodeValue.Value).ToString() : null;

                // For Other Charge Code 2 if available.
                if (aWBOtherChargesList.Count > 1)
                {
                    OtherChargeCode2 = aWBOtherChargesList[1].OtherChargeCode;
                    VatLabel2 = aWBOtherChargesList[1].OtherChargeVatLabel;
                    VatText2 = aWBOtherChargesList[1].OtherChargeVatText;
                    VatCalculatedAmountSign2 = Utilities.GetSignValue(aWBOtherChargesList[1].OtherChargeVatCalculatedAmount);
                    VatCalculatedAmount2 = aWBOtherChargesList[1].OtherChargeVatCalculatedAmount.HasValue ? Math.Abs(aWBOtherChargesList[1].OtherChargeVatCalculatedAmount.Value).ToString() : null;
                    VatPercentageSign2 = Utilities.GetSignValue(aWBOtherChargesList[1].OtherChargeVatPercentage);
                    VatPercentage2 = aWBOtherChargesList[1].OtherChargeVatPercentage.HasValue ? Math.Abs(aWBOtherChargesList[1].OtherChargeVatPercentage.Value).ToString() : null;
                    VatBaseAmountSign2 = Utilities.GetSignValue(aWBOtherChargesList[1].OtherChargeVatBaseAmount);
                    VatBaseAmount2 = aWBOtherChargesList[1].OtherChargeVatBaseAmount.HasValue ? Math.Abs(aWBOtherChargesList[1].OtherChargeVatBaseAmount.Value).ToString() : null;
                    OtherChargeCodeValueSign2 = Utilities.GetSignValue(aWBOtherChargesList[1].OtherChargeCodeValue);
                    OtherChargeCodeValue2 = aWBOtherChargesList[1].OtherChargeCodeValue.HasValue ? Math.Abs(aWBOtherChargesList[1].OtherChargeCodeValue.Value).ToString() : null;

                    // For Other Charge Code 3 if available.
                    if (aWBOtherChargesList.Count > 2)
                    {
                        OtherChargeCode3 = aWBOtherChargesList[2].OtherChargeCode;
                        VatLabel3 = aWBOtherChargesList[2].OtherChargeVatLabel;
                        VatText3 = aWBOtherChargesList[2].OtherChargeVatText;
                        VatCalculatedAmountSign3 = Utilities.GetSignValue(aWBOtherChargesList[2].OtherChargeVatCalculatedAmount);
                        VatCalculatedAmount3 = aWBOtherChargesList[2].OtherChargeVatCalculatedAmount.HasValue ? Math.Abs(aWBOtherChargesList[2].OtherChargeVatCalculatedAmount.Value).ToString() : null;
                        VatPercentageSign3 = Utilities.GetSignValue(aWBOtherChargesList[2].OtherChargeVatPercentage);
                        VatPercentage3 = aWBOtherChargesList[2].OtherChargeVatPercentage.HasValue ? Math.Abs(aWBOtherChargesList[2].OtherChargeVatPercentage.Value).ToString() : null;
                        VatBaseAmountSign3 = Utilities.GetSignValue(aWBOtherChargesList[2].OtherChargeVatBaseAmount);
                        VatBaseAmount3 = aWBOtherChargesList[2].OtherChargeVatBaseAmount.HasValue ? Math.Abs(aWBOtherChargesList[2].OtherChargeVatBaseAmount.Value).ToString() : null;
                        OtherChargeCodeValueSign3 = Utilities.GetSignValue(aWBOtherChargesList[2].OtherChargeCodeValue);
                        OtherChargeCodeValue3 = aWBOtherChargesList[2].OtherChargeCodeValue.HasValue ? Math.Abs(aWBOtherChargesList[2].OtherChargeCodeValue.Value).ToString() : null;
                    }
                }
            }

            ProcessNextClass(aWBOtherChargesList, ref recordSequenceNumber);

            //Logger.Info("End of Converting AWBOtherCharges into AWBOCBreakdownRecord.");
        }

        /// <summary>
        /// To Convert Childs of AWBOtherCharges into there corresponding Records.
        /// </summary>
        /// <param name="aWBOtherChargesList">List of AWBOtherCharges</param>
        /// <param name="recordSequenceNumber">Record Sequence Number</param>
        public void ProcessNextClass(List<AWBOtherCharges> aWBOtherChargesList, ref long recordSequenceNumber)
        {
            //Logger.Info("AWBOtherCharges cannot have Childs.");
        }

        #endregion

        #region Implementation of IRecordToClassConverter<List<AWBOtherCharges>> To Read IDEC File.

        /// <summary>
        /// To Convert AWBOCBreakdownRecord into AWBOtherCharges.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <returns>List of AWBOtherCharges</returns>
        public List<AWBOtherCharges> ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Info("Start of Converting AWBOCBreakdownRecord into AWBOtherCharges.");

            var aWBOtherChargesList = CreateListOfAWBOtherCharges();

            ProcessNextRecord(multiRecordEngine, aWBOtherChargesList);

            //Logger.Info("End of Converting AWBOCBreakdownRecord into AWBOtherCharges.");

            return aWBOtherChargesList;
        }

        /// <summary>
        /// Creates List of AWBOtherCharges for all the AWBOtherChargesRecords present in the reading input IDEC File.
        /// </summary>
        /// <returns>List of AWBOtherCharges.</returns>
        private List<AWBOtherCharges> CreateListOfAWBOtherCharges()
        {
            //Logger.Info("Start of Creating AWBOtherCharges Class from AWBOCBreakdownRecord.");

            var aWBOtherCharges = new List<AWBOtherCharges>();

            // OtherChargeCode 1
            if (!string.IsNullOrWhiteSpace(OtherChargeCode1))
            {
                double? vCalAmt1;
                double? vBaseAmt1;
                double? vPerAmt1;

                if (string.IsNullOrWhiteSpace(VatLabel1) && string.IsNullOrWhiteSpace(VatText1) && Convert.ToDouble(VatCalculatedAmount1) == 0 && Convert.ToDouble(VatBaseAmount1) == 0 && Convert.ToDouble(VatPercentage1) == 0)
                {
                    VatLabel1 = null;
                    VatText1 = null;
                    vCalAmt1 = null;
                    vBaseAmt1 = null;
                    vPerAmt1 = null;
                }
                else
                {
                    vCalAmt1 = Utilities.GetActualValueForDouble(VatCalculatedAmountSign1, VatCalculatedAmount1);
                    vBaseAmt1 = Utilities.GetActualValueForDouble(VatBaseAmountSign1, VatBaseAmount1);
                    vPerAmt1 = Utilities.GetActualValueForDouble(VatPercentageSign1, VatPercentage1);
                }

                aWBOtherCharges.Add(new AWBOtherCharges
                {
                    OtherChargeCode = OtherChargeCode1,
                    OtherChargeVatLabel = VatLabel1,
                    OtherChargeVatText = VatText1,
                    OtherChargeVatCalculatedAmount = vCalAmt1,
                    OtherChargeVatPercentage = vPerAmt1,
                    OtherChargeVatBaseAmount = vBaseAmt1,
                    OtherChargeCodeValue = Utilities.GetActualValueForDouble(OtherChargeCodeValueSign1, OtherChargeCodeValue1)
                });
            }

            // OtherChargeCode 2
            if (!string.IsNullOrWhiteSpace(OtherChargeCode2))
            {
                double? vCalAmt2;
                double? vBaseAmt2;
                double? vPerAmt2;

                if (string.IsNullOrWhiteSpace(VatLabel2) && string.IsNullOrWhiteSpace(VatText2) && Convert.ToDouble(VatCalculatedAmount2) == 0 && Convert.ToDouble(VatBaseAmount2) == 0 && Convert.ToDouble(VatPercentage2) == 0)
                {
                    VatLabel2 = null;
                    VatText2 = null;
                    vCalAmt2 = null;
                    vBaseAmt2 = null;
                    vPerAmt2 = null;
                }
                else
                {
                    vCalAmt2 = Utilities.GetActualValueForDouble(VatCalculatedAmountSign2, VatCalculatedAmount2);
                    vBaseAmt2 = Utilities.GetActualValueForDouble(VatBaseAmountSign2, VatBaseAmount2);
                    vPerAmt2 = Utilities.GetActualValueForDouble(VatPercentageSign2, VatPercentage2);
                }

                aWBOtherCharges.Add(new AWBOtherCharges
                {
                    OtherChargeCode = OtherChargeCode2,
                    OtherChargeVatLabel = VatLabel2,
                    OtherChargeVatText = VatText2,
                    OtherChargeVatCalculatedAmount = vCalAmt2,
                    OtherChargeVatPercentage = vPerAmt2,
                    OtherChargeVatBaseAmount = vBaseAmt2,
                    OtherChargeCodeValue = Utilities.GetActualValueForDouble(OtherChargeCodeValueSign2, OtherChargeCodeValue2)
                });
            }

            // OtherChargeCode 3
            if (!string.IsNullOrWhiteSpace(OtherChargeCode3))
            {
                double? vCalAmt3;
                double? vBaseAmt3;
                double? vPerAmt3;

                if (string.IsNullOrWhiteSpace(VatLabel3) && string.IsNullOrWhiteSpace(VatText3) && Convert.ToDouble(VatCalculatedAmount3) == 0 && Convert.ToDouble(VatBaseAmount3) == 0 && Convert.ToDouble(VatPercentage3) == 0)
                {
                    VatLabel3 = null;
                    VatText3 = null;
                    vCalAmt3 = null;
                    vBaseAmt3 = null;
                    vPerAmt3 = null;
                }
                else
                {
                    vCalAmt3 = Utilities.GetActualValueForDouble(VatCalculatedAmountSign3, VatCalculatedAmount3);
                    vBaseAmt3 = Utilities.GetActualValueForDouble(VatBaseAmountSign3, VatBaseAmount3);
                    vPerAmt3 = Utilities.GetActualValueForDouble(VatPercentageSign3, VatPercentage3);
                }

                aWBOtherCharges.Add(new AWBOtherCharges
                {
                    OtherChargeCode = OtherChargeCode3,
                    OtherChargeVatLabel = VatLabel3,
                    OtherChargeVatText = VatText3,
                    OtherChargeVatCalculatedAmount = vCalAmt3,
                    OtherChargeVatPercentage = vPerAmt3,
                    OtherChargeVatBaseAmount = vBaseAmt3,
                    OtherChargeCodeValue = Utilities.GetActualValueForDouble(OtherChargeCodeValueSign3, OtherChargeCodeValue3)
                });
            }

            //Logger.Info("End of Creating AWBOtherCharges Class from AWBOCBreakdownRecord.");

            return aWBOtherCharges;
        }

        /// <summary>
        /// To Convert Child records of AWBOCBreakdownRecord into there corresponding Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="aWBOtherChargesList">List of AWBOtherCharges</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, List<AWBOtherCharges> aWBOtherChargesList)
        {
            multiRecordEngine.ReadNext();
            //Logger.Info("AWBOCBreakdownRecord cannot have Childs.");
        }

        #endregion

    }
}