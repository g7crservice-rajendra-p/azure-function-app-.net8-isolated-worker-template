using System;
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
    public class RMAWBOCBreakdownRecord : InvoiceRecordBase, IRecordToClassConverter<List<RMAWBOtherCharges>>, IClassToRecordConverter<List<RMAWBOtherCharges>>
    {
        
        [FieldFixedLength(11)]
        public string RejectionMemoNumber;

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

        public RMAWBOCBreakdownRecord()
        { }

        public RMAWBOCBreakdownRecord(RMAWBRecord rMAWBRecord)
            : base(rMAWBRecord)
        {
            RejectionMemoNumber = rMAWBRecord.RejectionMemoNumber;
            AWBIssuingAirline = rMAWBRecord.AWBIssuingAirline;
            AWBSerialNumber = rMAWBRecord.AWBSerialNumber;
            AWBCheckDigit = rMAWBRecord.AWBCheckDigit;
            BillingCode = rMAWBRecord.BillingCode;
        }

        public List<RMAWBOtherCharges> ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Debug("Converting RMAWBOtherCharges record to model instance.");

            var rMAWBOtherChargesList = CreateRMAWBOtherChargesList();

            ProcessNextRecord(multiRecordEngine, rMAWBOtherChargesList);

            return rMAWBOtherChargesList;
        }

        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, List<RMAWBOtherCharges> rMAWBOtherChargesList)
        {
            multiRecordEngine.ReadNext();
        }

        /// <summary>
        /// Creates RMAWBOtherCharges objects.
        /// </summary>
        /// <returns>List RMAWBOtherCharges.</returns>
        private List<RMAWBOtherCharges> CreateRMAWBOtherChargesList()
        {
            var rMAWBOtherChargesList = new List<RMAWBOtherCharges>();
            //Logger.Debug("Creating RMAWBOtherCharges list.");

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

                rMAWBOtherChargesList.Add(new RMAWBOtherCharges
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

                rMAWBOtherChargesList.Add(new RMAWBOtherCharges
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
                rMAWBOtherChargesList.Add(new RMAWBOtherCharges
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

            //Logger.Debug("RMAWBOtherCharges Object list created successfully.");

            return rMAWBOtherChargesList;
        }

        /// <summary>
        /// This method converts the RMAWBOtherCharges model into RMAWBOtherCharges record instance 
        /// </summary>
        /// <param name="rMAWBOtherChargesList"></param>
        /// <param name="recordSequenceNumber"></param>
        public void ConvertClassToRecord(List<RMAWBOtherCharges> rMAWBOtherChargesList, ref long recordSequenceNumber)
        {
            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiOcRecord;

            if (rMAWBOtherChargesList.Count > 0)
            {
                OtherChargeCode1 = rMAWBOtherChargesList[0].OtherChargeCode;
                OtherChargeCodeValue1 = rMAWBOtherChargesList[0].OtherChargeCodeValue.HasValue ? Math.Abs(rMAWBOtherChargesList[0].OtherChargeCodeValue.Value).ToString() : null;
                OtherChargeCodeValueSign1 = rMAWBOtherChargesList[0].OtherChargeCodeValue.HasValue ? Utilities.GetSignValue(rMAWBOtherChargesList[0].OtherChargeCodeValue.Value) : null;
                VatLabel1 = rMAWBOtherChargesList[0].OtherChargeVatLabel;
                VatText1 = rMAWBOtherChargesList[0].OtherChargeVatText;
                if (rMAWBOtherChargesList[0].OtherChargeVatBaseAmount.HasValue)
                    VatBaseAmount1 = Math.Abs(rMAWBOtherChargesList[0].OtherChargeVatBaseAmount.Value).ToString();
                VatBaseAmountSign1 = Utilities.GetSignValue(rMAWBOtherChargesList[0].OtherChargeVatBaseAmount);
                if (rMAWBOtherChargesList[0].OtherChargeVatPercentage.HasValue)
                    VatPercentage1 = Math.Abs(rMAWBOtherChargesList[0].OtherChargeVatPercentage.Value).ToString();
                VatPercentageSign1 = Utilities.GetSignValue(rMAWBOtherChargesList[0].OtherChargeVatPercentage);
                VatCalculatedAmountSign1 = Utilities.GetSignValue(rMAWBOtherChargesList[0].OtherChargeVatCalculatedAmount);
                if (rMAWBOtherChargesList[0].OtherChargeVatCalculatedAmount.HasValue)
                    VatCalculatedAmount1 = Math.Abs(rMAWBOtherChargesList[0].OtherChargeVatCalculatedAmount.Value).ToString();

                if (rMAWBOtherChargesList.Count > 1)
                {
                    OtherChargeCode2 = rMAWBOtherChargesList[1].OtherChargeCode;
                    OtherChargeCodeValue2 = rMAWBOtherChargesList[1].OtherChargeCodeValue.HasValue ? Math.Abs(rMAWBOtherChargesList[1].OtherChargeCodeValue.Value).ToString() : null;
                    OtherChargeCodeValueSign2 = rMAWBOtherChargesList[1].OtherChargeCodeValue.HasValue ? Utilities.GetSignValue(rMAWBOtherChargesList[1].OtherChargeCodeValue.Value) : null;
                    VatLabel2 = rMAWBOtherChargesList[1].OtherChargeVatLabel;
                    VatText2 = rMAWBOtherChargesList[1].OtherChargeVatText;
                    if (rMAWBOtherChargesList[1].OtherChargeVatBaseAmount.HasValue)
                        VatBaseAmount2 = Math.Abs(rMAWBOtherChargesList[1].OtherChargeVatBaseAmount.Value).ToString();
                    VatBaseAmountSign2 = Utilities.GetSignValue(rMAWBOtherChargesList[1].OtherChargeVatBaseAmount);
                    if (rMAWBOtherChargesList[1].OtherChargeVatPercentage.HasValue)
                        VatPercentage2 = Math.Abs(rMAWBOtherChargesList[1].OtherChargeVatPercentage.Value).ToString();
                    VatPercentageSign2 = Utilities.GetSignValue(rMAWBOtherChargesList[1].OtherChargeVatPercentage);
                    VatCalculatedAmountSign2 = Utilities.GetSignValue(rMAWBOtherChargesList[1].OtherChargeVatCalculatedAmount);
                    if (rMAWBOtherChargesList[1].OtherChargeVatCalculatedAmount.HasValue)
                        VatCalculatedAmount2 = Math.Abs(rMAWBOtherChargesList[1].OtherChargeVatCalculatedAmount.Value).ToString();
                }

                if (rMAWBOtherChargesList.Count > 2)
                {
                    OtherChargeCode3 = rMAWBOtherChargesList[2].OtherChargeCode;
                    OtherChargeCodeValue3 = rMAWBOtherChargesList[2].OtherChargeCodeValue.HasValue ? Math.Abs(rMAWBOtherChargesList[2].OtherChargeCodeValue.Value).ToString() : null;
                    OtherChargeCodeValueSign3 = rMAWBOtherChargesList[2].OtherChargeCodeValue.HasValue ? Utilities.GetSignValue(rMAWBOtherChargesList[2].OtherChargeCodeValue.Value) : null;
                    VatLabel3 = rMAWBOtherChargesList[2].OtherChargeVatLabel;
                    VatText3 = rMAWBOtherChargesList[2].OtherChargeVatText;
                    if (rMAWBOtherChargesList[2].OtherChargeVatBaseAmount.HasValue)
                        VatBaseAmount3 = Math.Abs(rMAWBOtherChargesList[2].OtherChargeVatBaseAmount.Value).ToString();
                    VatBaseAmountSign3 = Utilities.GetSignValue(rMAWBOtherChargesList[2].OtherChargeVatBaseAmount);
                    if (rMAWBOtherChargesList[2].OtherChargeVatPercentage.HasValue)
                        VatPercentage3 = Math.Abs(rMAWBOtherChargesList[2].OtherChargeVatPercentage.Value).ToString();
                    VatPercentageSign3 = Utilities.GetSignValue(rMAWBOtherChargesList[2].OtherChargeVatPercentage);
                    VatCalculatedAmountSign3 = Utilities.GetSignValue(rMAWBOtherChargesList[2].OtherChargeVatCalculatedAmount);
                    if (rMAWBOtherChargesList[2].OtherChargeVatCalculatedAmount.HasValue)
                        VatCalculatedAmount3 = Math.Abs(rMAWBOtherChargesList[2].OtherChargeVatCalculatedAmount.Value).ToString();
                }
            }
            ProcessNextClass(rMAWBOtherChargesList, ref recordSequenceNumber);
        }

        /// <summary>
        /// This method adds the child records of RMAWBOtherCharges record to corresponding list by calling respective functions.
        /// </summary>
        /// <param name="rMAWBOtherChargesList"></param>
        /// <param name="recordSequenceNumber"></param>
        public void ProcessNextClass(List<RMAWBOtherCharges> rMAWBOtherChargesList, ref long recordSequenceNumber)
        {
            //Logger.Debug("Processing RMAWBOtherCharges model child objects.");
        }
    }
}