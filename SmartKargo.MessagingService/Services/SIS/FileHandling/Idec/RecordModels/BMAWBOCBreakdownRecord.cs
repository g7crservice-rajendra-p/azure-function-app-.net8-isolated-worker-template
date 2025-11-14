using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.Write;
using QidWorkerRole.SIS.Model;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    [FixedLengthRecord()]
    public class BMAWBOCBreakdownRecord : InvoiceRecordBase, IRecordToClassConverter<List<BMAWBOtherCharges>>, IClassToRecordConverter<List<BMAWBOtherCharges>>
    {
        [FieldFixedLength(11)]
        public string BillingMemoNumber;

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

        public BMAWBOCBreakdownRecord()
        { }

        public BMAWBOCBreakdownRecord(BMAWBRecord bMAWBRecord)
            : base(bMAWBRecord)
        {
            BillingMemoNumber = bMAWBRecord.BillingCreditMemoNumber;
            AWBIssuingAirline = bMAWBRecord.AWBIssuingAirline;
            AWBSerialNumber = bMAWBRecord.AWBSerialNumber;
            AWBCheckDigit = bMAWBRecord.CheckDigit;
            BillingCode = bMAWBRecord.BillingCode;
        }

        public List<BMAWBOtherCharges> ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            var couponRecordVatObjects = CreateBMAWBOCBreakdownObjects();

            ProcessNextRecord(multiRecordEngine, couponRecordVatObjects);

            return couponRecordVatObjects;
        }

        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, List<BMAWBOtherCharges> parent)
        {
            multiRecordEngine.ReadNext();
        }

        /// <summary>
        /// This method creates BMAWBOtherCharges objects.
        /// </summary>
        /// <returns>List of OC Record Vat objects.</returns>
        private List<BMAWBOtherCharges> CreateBMAWBOCBreakdownObjects()
        {
            var bmawbocBreakdownObjects = new List<BMAWBOtherCharges>();

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

                bmawbocBreakdownObjects.Add(new BMAWBOtherCharges
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

                bmawbocBreakdownObjects.Add(new BMAWBOtherCharges
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

                bmawbocBreakdownObjects.Add(new BMAWBOtherCharges
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
            return bmawbocBreakdownObjects;
        }

        /// <summary>
        /// This method converts the BMAWBOtherCharges model into BMAWBOtherCharges record instance 
        /// </summary>
        /// <param name="bMAWBOtherChargesList"></param>
        /// <param name="recordSequenceNumber"></param>
        public void ConvertClassToRecord(List<BMAWBOtherCharges> bMAWBOtherChargesList, ref long recordSequenceNumber)
        {
            //Logger.Debug("Converting BMAWBOtherCharges model to BMAWBOtherCharges record instance.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiOcRecord;

            if (bMAWBOtherChargesList.Count > 0)
            {
                OtherChargeCode1 = bMAWBOtherChargesList[0].OtherChargeCode;
                VatLabel1 = bMAWBOtherChargesList[0].OtherChargeVatLabel;
                VatText1 = bMAWBOtherChargesList[0].OtherChargeVatText;
                VatCalculatedAmountSign1 = Utilities.GetSignValue(bMAWBOtherChargesList[0].OtherChargeVatCalculatedAmount);
                if (bMAWBOtherChargesList[0].OtherChargeVatCalculatedAmount.HasValue)
                    VatCalculatedAmount1 = Math.Abs(bMAWBOtherChargesList[0].OtherChargeVatCalculatedAmount.Value).ToString();
                VatPercentageSign1 = Utilities.GetSignValue(bMAWBOtherChargesList[0].OtherChargeVatPercentage);
                if (bMAWBOtherChargesList[0].OtherChargeVatPercentage.HasValue)
                    VatPercentage1 = Math.Abs(bMAWBOtherChargesList[0].OtherChargeVatPercentage.Value).ToString();
                VatBaseAmountSign1 = Utilities.GetSignValue(bMAWBOtherChargesList[0].OtherChargeVatBaseAmount);
                if (bMAWBOtherChargesList[0].OtherChargeVatBaseAmount.HasValue)
                    VatBaseAmount1 = Math.Abs(bMAWBOtherChargesList[0].OtherChargeVatBaseAmount.Value).ToString();
                OtherChargeCodeValueSign1 = bMAWBOtherChargesList[0].OtherChargeCodeValue.HasValue ? Utilities.GetSignValue(bMAWBOtherChargesList[0].OtherChargeCodeValue.Value) : null;
                OtherChargeCodeValue1 = bMAWBOtherChargesList[0].OtherChargeCodeValue.HasValue ? Math.Abs(bMAWBOtherChargesList[0].OtherChargeCodeValue.Value).ToString() : null;

                if (bMAWBOtherChargesList.Count > 1)
                {
                    OtherChargeCode2 = bMAWBOtherChargesList[1].OtherChargeCode;
                    VatLabel2 = bMAWBOtherChargesList[1].OtherChargeVatLabel;
                    VatText2 = bMAWBOtherChargesList[1].OtherChargeVatText;
                    VatCalculatedAmountSign2 = Utilities.GetSignValue(bMAWBOtherChargesList[1].OtherChargeVatCalculatedAmount);
                    if (bMAWBOtherChargesList[1].OtherChargeVatCalculatedAmount.HasValue)
                        VatCalculatedAmount2 = Math.Abs(bMAWBOtherChargesList[1].OtherChargeVatCalculatedAmount.Value).ToString();
                    VatPercentageSign2 = Utilities.GetSignValue(bMAWBOtherChargesList[1].OtherChargeVatPercentage);
                    if (bMAWBOtherChargesList[1].OtherChargeVatPercentage.HasValue)
                        VatPercentage2 = Math.Abs(bMAWBOtherChargesList[1].OtherChargeVatPercentage.Value).ToString();
                    VatBaseAmountSign2 = Utilities.GetSignValue(bMAWBOtherChargesList[1].OtherChargeVatBaseAmount);
                    if (bMAWBOtherChargesList[1].OtherChargeVatBaseAmount.HasValue)
                        VatBaseAmount2 = Math.Abs(bMAWBOtherChargesList[1].OtherChargeVatBaseAmount.Value).ToString();
                    OtherChargeCodeValueSign2 = bMAWBOtherChargesList[1].OtherChargeCodeValue.HasValue ? Utilities.GetSignValue(bMAWBOtherChargesList[1].OtherChargeCodeValue.Value) : null;
                    OtherChargeCodeValue2 = bMAWBOtherChargesList[1].OtherChargeCodeValue.HasValue ? Math.Abs(bMAWBOtherChargesList[1].OtherChargeCodeValue.Value).ToString() : null;

                }

                if (bMAWBOtherChargesList.Count > 2)
                {
                    OtherChargeCode3 = bMAWBOtherChargesList[2].OtherChargeCode;
                    VatLabel3 = bMAWBOtherChargesList[2].OtherChargeVatLabel;
                    VatText3 = bMAWBOtherChargesList[2].OtherChargeVatText;
                    VatCalculatedAmountSign3 = Utilities.GetSignValue(bMAWBOtherChargesList[2].OtherChargeVatCalculatedAmount);
                    if (bMAWBOtherChargesList[2].OtherChargeVatCalculatedAmount.HasValue)
                        VatCalculatedAmount3 = Math.Abs(bMAWBOtherChargesList[2].OtherChargeVatCalculatedAmount.Value).ToString();
                    VatPercentageSign3 = Utilities.GetSignValue(bMAWBOtherChargesList[2].OtherChargeVatPercentage);
                    if (bMAWBOtherChargesList[2].OtherChargeVatPercentage.HasValue)
                        VatPercentage3 = Math.Abs(bMAWBOtherChargesList[2].OtherChargeVatPercentage.Value).ToString();
                    VatBaseAmountSign3 = Utilities.GetSignValue(bMAWBOtherChargesList[2].OtherChargeVatBaseAmount);
                    if (bMAWBOtherChargesList[2].OtherChargeVatBaseAmount.HasValue)
                        VatBaseAmount3 = Math.Abs(bMAWBOtherChargesList[2].OtherChargeVatBaseAmount.Value).ToString();
                    OtherChargeCodeValueSign3 = bMAWBOtherChargesList[2].OtherChargeCodeValue.HasValue ? Utilities.GetSignValue(bMAWBOtherChargesList[2].OtherChargeCodeValue.Value) : null;
                    OtherChargeCodeValue3 = bMAWBOtherChargesList[2].OtherChargeCodeValue.HasValue ? Math.Abs(bMAWBOtherChargesList[2].OtherChargeCodeValue.Value).ToString() : null;
                }
            }

            ProcessNextClass(bMAWBOtherChargesList, ref recordSequenceNumber);
        }

        /// <summary>
        /// This method adds the child records of BMAWBOtherCharges record to corresponding list by calling respective functions.
        /// </summary>
        /// <param name="bMAWBOtherChargesList"></param>
        /// <param name="recordSequenceNumber"></param>
        public void ProcessNextClass(List<BMAWBOtherCharges> bMAWBOtherChargesList, ref long recordSequenceNumber)
        {
            //Logger.Debug("Processing BMAWBOtherCharges model child objects.");
        }
    }
}