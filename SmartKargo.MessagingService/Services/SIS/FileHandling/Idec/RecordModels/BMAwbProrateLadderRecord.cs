using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.Write;
using QidWorkerRole.SIS.Model;
using QidWorkerRole.SIS.Model.SupportingModels;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    public class BMAwbProrateLadderRecord : InvoiceRecordBase, IRecordToClassConverter<BMProrateSlipDetails>, IClassToRecordConverter<BMProrateSlipDetails>
    {
        
        [FieldFixedLength(11)]
        public string BillingMemoNumber;

        [FieldFixedLength(6)]
        public string Filler2;

        [FieldFixedLength(4), FieldConverter(typeof(PaddingConverter), 4, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string AwbIssuingAirline;

        [FieldFixedLength(7), FieldConverter(typeof(PaddingConverter), 7, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string AwbSerialNumber;

        [FieldFixedLength(1), FieldConverter(typeof(PaddingConverter), 1, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string AwbCheckDigit;

        [FieldFixedLength(3)]
        public string ProrateCalCurrencyId;

        [FieldFixedLength(1)]
        public string TotalAmountSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalAmount;

        [FieldFixedLength(4)]
        public string FromSector1;

        [FieldFixedLength(4)]
        public string ToSector1;

        [FieldFixedLength(3)]
        public string CarrierPrefix1;

        [FieldFixedLength(1)]
        public string ProvisoReqSpa1;

        [FieldFixedLength(10), FieldConverter(typeof(PaddingConverter), 10, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string ProrateFactor1;

        [FieldFixedLength(6), FieldConverter(typeof(DoubleNumberConverter), 6, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string PercentShare1;

        [FieldFixedLength(1)]
        public string AmountSign1;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string Amount1;

        [FieldFixedLength(10)]
        public string Filler3;

        [FieldFixedLength(4)]
        public string FromSector2;

        [FieldFixedLength(4)]
        public string ToSector2;

        [FieldFixedLength(3)]
        public string CarrierPrefix2;

        [FieldFixedLength(1)]
        public string ProvisoReqSpa2;

        [FieldFixedLength(10), FieldConverter(typeof(PaddingConverter), 10, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string ProrateFactor2;

        [FieldFixedLength(6), FieldConverter(typeof(DoubleNumberConverter), 6, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string PercentShare2;

        [FieldFixedLength(1)]
        public string AmountSign2;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string Amount2;

        [FieldFixedLength(10)]
        public string Filler4;

        [FieldFixedLength(4)]
        public string FromSector3;

        [FieldFixedLength(4)]
        public string ToSector3;

        [FieldFixedLength(3)]
        public string CarrierPrefix3;

        [FieldFixedLength(1)]
        public string ProvisoReqSpa3;

        [FieldFixedLength(10), FieldConverter(typeof(PaddingConverter), 10, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string ProrateFactor3;

        [FieldFixedLength(6), FieldConverter(typeof(DoubleNumberConverter), 6, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string PercentShare3;

        [FieldFixedLength(1)]
        public string AmountSign3;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string Amount3;

        [FieldFixedLength(10)]
        public string Filler5;

        [FieldFixedLength(4)]
        public string FromSector4;

        [FieldFixedLength(4)]
        public string ToSector4;

        [FieldFixedLength(3)]
        public string CarrierPrefix4;

        [FieldFixedLength(1)]
        public string ProvisoReqSpa4;

        [FieldFixedLength(10), FieldConverter(typeof(PaddingConverter), 10, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string ProrateFactor4;

        [FieldFixedLength(6), FieldConverter(typeof(DoubleNumberConverter), 6, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string PercentShare4;

        [FieldFixedLength(1)]
        public string AmountSign4;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string Amount4;

        [FieldFixedLength(10)]
        public string Filler6;

        [FieldFixedLength(4)]
        public string FromSector5;

        [FieldFixedLength(4)]
        public string ToSector5;

        [FieldFixedLength(3)]
        public string CarrierPrefix5;

        [FieldFixedLength(1)]
        public string ProvisoReqSpa5;

        [FieldFixedLength(10), FieldConverter(typeof(PaddingConverter), 10, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string ProrateFactor5;

        [FieldFixedLength(6), FieldConverter(typeof(DoubleNumberConverter), 6, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string PercentShare5;

        [FieldFixedLength(1)]
        public string AmountSign5;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string Amount5;

        [FieldFixedLength(10)]
        public string Filler7;

        [FieldFixedLength(4)]
        public string FromSector6;

        [FieldFixedLength(4)]
        public string ToSector6;

        [FieldFixedLength(3)]
        public string CarrierPrefix6;

        [FieldFixedLength(1)]
        public string ProvisoReqSpa6;

        [FieldFixedLength(10), FieldConverter(typeof(PaddingConverter), 10, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string ProrateFactor6;

        [FieldFixedLength(6), FieldConverter(typeof(DoubleNumberConverter), 6, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string PercentShare6;

        [FieldFixedLength(1)]
        public string AmountSign6;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string Amount6;

        [FieldFixedLength(10)]
        public string Filler8;

        [FieldFixedLength(4)]
        public string FromSector7;

        [FieldFixedLength(4)]
        public string ToSector7;

        [FieldFixedLength(3)]
        public string CarrierPrefix7;

        [FieldFixedLength(1)]
        public string ProvisoReqSpa7;

        [FieldFixedLength(10), FieldConverter(typeof(PaddingConverter), 10, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string ProrateFactor7;

        [FieldFixedLength(6), FieldConverter(typeof(DoubleNumberConverter), 6, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string PercentShare7;

        [FieldFixedLength(1)]
        public string AmountSign7;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string Amount7;

        [FieldFixedLength(10)]
        public string Filler9;

        [FieldFixedLength(4)]
        public string FromSector8;

        [FieldFixedLength(4)]
        public string ToSector8;

        [FieldFixedLength(3)]
        public string CarrierPrefix8;

        [FieldFixedLength(1)]
        public string ProvisoReqSpa8;

        [FieldFixedLength(10), FieldConverter(typeof(PaddingConverter), 10, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string ProrateFactor8;

        [FieldFixedLength(6), FieldConverter(typeof(DoubleNumberConverter), 6, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string PercentShare8;

        [FieldFixedLength(1)]
        public string AmountSign8;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string Amount8;

        [FieldFixedLength(30)]
        public string Filler10;

        public BMAwbProrateLadderRecord()
        { }

        public BMAwbProrateLadderRecord(BMAWBRecord bMAWBRecord)
            : base(bMAWBRecord)
        {
            BillingMemoNumber = bMAWBRecord.BillingCreditMemoNumber;
            AwbIssuingAirline = bMAWBRecord.AWBIssuingAirline;
            AwbSerialNumber = bMAWBRecord.AWBSerialNumber;
            AwbCheckDigit = bMAWBRecord.CheckDigit;
            BillingCode = bMAWBRecord.BillingCode;
        }

        /// <summary>
        /// This method creates Prorate Ladder Record object.
        /// </summary>
        /// <returns>List of BMProrateSlipDetails objects.</returns>
        private BMProrateSlipDetails CreateProrateLadderBreakdownObjects()
        {
            var bMProrateSlipDetails = new BMProrateSlipDetails { TotalProrateAmount = Convert.ToDouble(TotalAmount), ProrateCalCurrencyId = ProrateCalCurrencyId };

            var bMAWBProrateLadderList = new List<BMAWBProrateLadder>();

            if (!string.IsNullOrWhiteSpace(FromSector1) && !string.IsNullOrWhiteSpace(ToSector1))
            {
                bMAWBProrateLadderList.Add(new BMAWBProrateLadder
                {
                    FromSector = FromSector1.Trim(),
                    ToSector = ToSector1.Trim(),
                    CarrierPrefix = CarrierPrefix1.Trim(),
                    ProvisoReqSpa = ProvisoReqSpa1,
                    ProrateFactor = Convert.ToInt32(ProrateFactor1),
                    PercentShare = Convert.ToDouble(PercentShare1),
                    TotalAmount = Utilities.GetActualValueForDouble(AmountSign1, Amount1)
                });
            }

            if (!string.IsNullOrWhiteSpace(FromSector2) && !string.IsNullOrWhiteSpace(ToSector2))
            {
                bMAWBProrateLadderList.Add(new BMAWBProrateLadder
                {
                    FromSector = FromSector2.Trim(),
                    ToSector = ToSector2.Trim(),
                    CarrierPrefix = CarrierPrefix2.Trim(),
                    ProvisoReqSpa = ProvisoReqSpa2,
                    ProrateFactor = Convert.ToInt32(ProrateFactor2),
                    PercentShare = Convert.ToDouble(PercentShare2),
                    TotalAmount = Utilities.GetActualValueForDouble(AmountSign2, Amount2)
                });
            }

            if (!string.IsNullOrWhiteSpace(FromSector3) && !string.IsNullOrWhiteSpace(ToSector3))
            {
                bMAWBProrateLadderList.Add(new BMAWBProrateLadder
                {
                    FromSector = FromSector3.Trim(),
                    ToSector = ToSector3.Trim(),
                    CarrierPrefix = CarrierPrefix3.Trim(),
                    ProvisoReqSpa = ProvisoReqSpa3,
                    ProrateFactor = Convert.ToInt32(ProrateFactor3),
                    PercentShare = Convert.ToDouble(PercentShare3),
                    TotalAmount = Utilities.GetActualValueForDouble(AmountSign3, Amount3)
                });
            }

            if (!string.IsNullOrWhiteSpace(FromSector4) && !string.IsNullOrWhiteSpace(ToSector4))
            {
                bMAWBProrateLadderList.Add(new BMAWBProrateLadder
                {
                    FromSector = FromSector4.Trim(),
                    ToSector = ToSector4.Trim(),
                    CarrierPrefix = CarrierPrefix4.Trim(),
                    ProvisoReqSpa = ProvisoReqSpa4,
                    ProrateFactor = Convert.ToInt32(ProrateFactor4),
                    PercentShare = Convert.ToDouble(PercentShare4),
                    TotalAmount = Utilities.GetActualValueForDouble(AmountSign4, Amount4)
                });
            }

            if (!string.IsNullOrWhiteSpace(FromSector5) && !string.IsNullOrWhiteSpace(ToSector5))
            {
                bMAWBProrateLadderList.Add(new BMAWBProrateLadder
                {
                    FromSector = FromSector5.Trim(),
                    ToSector = ToSector5.Trim(),
                    CarrierPrefix = CarrierPrefix5.Trim(),
                    ProvisoReqSpa = ProvisoReqSpa5,
                    ProrateFactor = Convert.ToInt32(ProrateFactor5),
                    PercentShare = Convert.ToDouble(PercentShare5),
                    TotalAmount = Utilities.GetActualValueForDouble(AmountSign5, Amount5)
                });
            }

            if (!string.IsNullOrWhiteSpace(FromSector6) && !string.IsNullOrWhiteSpace(ToSector6))
            {
                bMAWBProrateLadderList.Add(new BMAWBProrateLadder
                {
                    FromSector = FromSector6.Trim(),
                    ToSector = ToSector6.Trim(),
                    CarrierPrefix = CarrierPrefix6.Trim(),
                    ProvisoReqSpa = ProvisoReqSpa6,
                    ProrateFactor = Convert.ToInt32(ProrateFactor6),
                    PercentShare = Convert.ToDouble(PercentShare6),
                    TotalAmount = Utilities.GetActualValueForDouble(AmountSign6, Amount6)
                });
            }

            if (!string.IsNullOrWhiteSpace(FromSector7) && !string.IsNullOrWhiteSpace(ToSector7))
            {
                bMAWBProrateLadderList.Add(new BMAWBProrateLadder
                {
                    FromSector = FromSector7.Trim(),
                    ToSector = ToSector7.Trim(),
                    CarrierPrefix = CarrierPrefix7.Trim(),
                    ProvisoReqSpa = ProvisoReqSpa7,
                    ProrateFactor = Convert.ToInt32(ProrateFactor7),
                    PercentShare = Convert.ToDouble(PercentShare7),
                    TotalAmount = Utilities.GetActualValueForDouble(AmountSign7, Amount7)
                });
            }

            if (!string.IsNullOrWhiteSpace(FromSector8) && !string.IsNullOrWhiteSpace(ToSector8))
            {
                bMAWBProrateLadderList.Add(new BMAWBProrateLadder
                {
                    FromSector = FromSector8.Trim(),
                    ToSector = ToSector8.Trim(),
                    CarrierPrefix = CarrierPrefix8.Trim(),
                    ProvisoReqSpa = ProvisoReqSpa8,
                    ProrateFactor = Convert.ToInt32(ProrateFactor8),
                    PercentShare = Convert.ToDouble(PercentShare8),
                    TotalAmount = Utilities.GetActualValueForDouble(AmountSign8, Amount8)
                });
            }

            bMProrateSlipDetails.BMAWBProrateLadderList.AddRange(bMAWBProrateLadderList);

            return bMProrateSlipDetails;
        }

        public BMProrateSlipDetails ConvertRecordToClass(FileHelpers.MultiRecordEngine multiRecordEngine)
        {
            var couponRecordVatObjects = CreateProrateLadderBreakdownObjects();

            ProcessNextRecord(multiRecordEngine, couponRecordVatObjects);

            return couponRecordVatObjects;
        }

        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, BMProrateSlipDetails bMProrateSlipDetails)
        {
            multiRecordEngine.ReadNext();
        }

        public void ConvertClassToRecord(BMProrateSlipDetails bMProrateSlipDetails, ref long recordSequenceNumber)
        {
            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiProrateLadderRecord;

            ProrateCalCurrencyId = bMProrateSlipDetails.ProrateCalCurrencyId;
            TotalAmountSign = Utilities.GetSignValue(bMProrateSlipDetails.TotalProrateAmount);
            TotalAmount = Math.Abs(bMProrateSlipDetails.TotalProrateAmount).ToString();

            if (bMProrateSlipDetails.BMAWBProrateLadderList.Count > 0)
            {
                FromSector1 = bMProrateSlipDetails.BMAWBProrateLadderList[0].FromSector;
                ToSector1 = bMProrateSlipDetails.BMAWBProrateLadderList[0].ToSector;
                CarrierPrefix1 = bMProrateSlipDetails.BMAWBProrateLadderList[0].CarrierPrefix;
                ProvisoReqSpa1 = bMProrateSlipDetails.BMAWBProrateLadderList[0].ProvisoReqSpa;
                ProrateFactor1 = bMProrateSlipDetails.BMAWBProrateLadderList[0].ProrateFactor.HasValue ? bMProrateSlipDetails.BMAWBProrateLadderList[0].ProrateFactor.Value.ToString() : null;
                PercentShare1 = bMProrateSlipDetails.BMAWBProrateLadderList[0].PercentShare.HasValue ? Math.Abs(bMProrateSlipDetails.BMAWBProrateLadderList[0].PercentShare.Value).ToString() : null;
                AmountSign1 = bMProrateSlipDetails.BMAWBProrateLadderList[0].TotalAmount.HasValue ? Utilities.GetSignValue(bMProrateSlipDetails.BMAWBProrateLadderList[0].TotalAmount.Value) : null;
                Amount1 = bMProrateSlipDetails.BMAWBProrateLadderList[0].TotalAmount.HasValue ? Math.Abs(bMProrateSlipDetails.BMAWBProrateLadderList[0].TotalAmount.Value).ToString() : null;

                if (bMProrateSlipDetails.BMAWBProrateLadderList.Count > 1)
                {
                    FromSector2 = bMProrateSlipDetails.BMAWBProrateLadderList[1].FromSector;
                    ToSector2 = bMProrateSlipDetails.BMAWBProrateLadderList[1].ToSector;
                    CarrierPrefix2 = bMProrateSlipDetails.BMAWBProrateLadderList[1].CarrierPrefix;
                    ProvisoReqSpa2 = bMProrateSlipDetails.BMAWBProrateLadderList[1].ProvisoReqSpa;
                    ProrateFactor2 = bMProrateSlipDetails.BMAWBProrateLadderList[1].ProrateFactor.HasValue ? bMProrateSlipDetails.BMAWBProrateLadderList[1].ProrateFactor.Value.ToString() : null;
                    PercentShare2 = bMProrateSlipDetails.BMAWBProrateLadderList[1].PercentShare.HasValue ? Math.Abs(bMProrateSlipDetails.BMAWBProrateLadderList[1].PercentShare.Value).ToString() : null;
                    AmountSign2 = bMProrateSlipDetails.BMAWBProrateLadderList[1].TotalAmount.HasValue ? Utilities.GetSignValue(bMProrateSlipDetails.BMAWBProrateLadderList[1].TotalAmount.Value) : null;
                    Amount2 = bMProrateSlipDetails.BMAWBProrateLadderList[1].TotalAmount.HasValue ? Math.Abs(bMProrateSlipDetails.BMAWBProrateLadderList[1].TotalAmount.Value).ToString() : null;
                }

                if (bMProrateSlipDetails.BMAWBProrateLadderList.Count > 2)
                {
                    FromSector3 = bMProrateSlipDetails.BMAWBProrateLadderList[2].FromSector;
                    ToSector3 = bMProrateSlipDetails.BMAWBProrateLadderList[2].ToSector;
                    CarrierPrefix3 = bMProrateSlipDetails.BMAWBProrateLadderList[2].CarrierPrefix;
                    ProvisoReqSpa3 = bMProrateSlipDetails.BMAWBProrateLadderList[2].ProvisoReqSpa;
                    ProrateFactor3 = bMProrateSlipDetails.BMAWBProrateLadderList[2].ProrateFactor.HasValue ? bMProrateSlipDetails.BMAWBProrateLadderList[2].ProrateFactor.Value.ToString() : null;
                    PercentShare3 = bMProrateSlipDetails.BMAWBProrateLadderList[2].PercentShare.HasValue ? Math.Abs(bMProrateSlipDetails.BMAWBProrateLadderList[2].PercentShare.Value).ToString() : null;
                    AmountSign3 = bMProrateSlipDetails.BMAWBProrateLadderList[2].TotalAmount.HasValue ? Utilities.GetSignValue(bMProrateSlipDetails.BMAWBProrateLadderList[2].TotalAmount.Value) : null;
                    Amount3 = bMProrateSlipDetails.BMAWBProrateLadderList[2].TotalAmount.HasValue ? Math.Abs(bMProrateSlipDetails.BMAWBProrateLadderList[2].TotalAmount.Value).ToString() : null;
                }

                if (bMProrateSlipDetails.BMAWBProrateLadderList.Count > 3)
                {
                    FromSector4 = bMProrateSlipDetails.BMAWBProrateLadderList[3].FromSector;
                    ToSector4 = bMProrateSlipDetails.BMAWBProrateLadderList[3].ToSector;
                    CarrierPrefix4 = bMProrateSlipDetails.BMAWBProrateLadderList[3].CarrierPrefix;
                    ProvisoReqSpa4 = bMProrateSlipDetails.BMAWBProrateLadderList[3].ProvisoReqSpa;
                    ProrateFactor4 = bMProrateSlipDetails.BMAWBProrateLadderList[3].ProrateFactor.HasValue ? bMProrateSlipDetails.BMAWBProrateLadderList[3].ProrateFactor.Value.ToString() : null;
                    PercentShare4 = bMProrateSlipDetails.BMAWBProrateLadderList[3].PercentShare.HasValue ? Math.Abs(bMProrateSlipDetails.BMAWBProrateLadderList[3].PercentShare.Value).ToString() : null;
                    AmountSign4 = bMProrateSlipDetails.BMAWBProrateLadderList[3].TotalAmount.HasValue ? Utilities.GetSignValue(bMProrateSlipDetails.BMAWBProrateLadderList[3].TotalAmount.Value) : null;
                    Amount4 = bMProrateSlipDetails.BMAWBProrateLadderList[3].TotalAmount.HasValue ? Math.Abs(bMProrateSlipDetails.BMAWBProrateLadderList[3].TotalAmount.Value).ToString() : null;
                }

                if (bMProrateSlipDetails.BMAWBProrateLadderList.Count > 4)
                {
                    FromSector5 = bMProrateSlipDetails.BMAWBProrateLadderList[4].FromSector;
                    ToSector5 = bMProrateSlipDetails.BMAWBProrateLadderList[4].ToSector;
                    CarrierPrefix5 = bMProrateSlipDetails.BMAWBProrateLadderList[4].CarrierPrefix;
                    ProvisoReqSpa5 = bMProrateSlipDetails.BMAWBProrateLadderList[4].ProvisoReqSpa;
                    ProrateFactor5 = bMProrateSlipDetails.BMAWBProrateLadderList[4].ProrateFactor.HasValue ? bMProrateSlipDetails.BMAWBProrateLadderList[4].ProrateFactor.Value.ToString() : null;
                    PercentShare5 = bMProrateSlipDetails.BMAWBProrateLadderList[4].PercentShare.HasValue ? Math.Abs(bMProrateSlipDetails.BMAWBProrateLadderList[4].PercentShare.Value).ToString() : null;
                    AmountSign5 = bMProrateSlipDetails.BMAWBProrateLadderList[4].TotalAmount.HasValue ? Utilities.GetSignValue(bMProrateSlipDetails.BMAWBProrateLadderList[4].TotalAmount.Value) : null;
                    Amount5 = bMProrateSlipDetails.BMAWBProrateLadderList[4].TotalAmount.HasValue ? Math.Abs(bMProrateSlipDetails.BMAWBProrateLadderList[4].TotalAmount.Value).ToString() : null;
                }

                if (bMProrateSlipDetails.BMAWBProrateLadderList.Count > 5)
                {
                    FromSector6 = bMProrateSlipDetails.BMAWBProrateLadderList[5].FromSector;
                    ToSector6 = bMProrateSlipDetails.BMAWBProrateLadderList[5].ToSector;
                    CarrierPrefix6 = bMProrateSlipDetails.BMAWBProrateLadderList[5].CarrierPrefix;
                    ProvisoReqSpa6 = bMProrateSlipDetails.BMAWBProrateLadderList[5].ProvisoReqSpa;
                    ProrateFactor6 = bMProrateSlipDetails.BMAWBProrateLadderList[5].ProrateFactor.HasValue ? bMProrateSlipDetails.BMAWBProrateLadderList[5].ProrateFactor.Value.ToString() : null;
                    PercentShare6 = bMProrateSlipDetails.BMAWBProrateLadderList[5].PercentShare.HasValue ? Math.Abs(bMProrateSlipDetails.BMAWBProrateLadderList[5].PercentShare.Value).ToString() : null;
                    AmountSign6 = bMProrateSlipDetails.BMAWBProrateLadderList[5].TotalAmount.HasValue ? Utilities.GetSignValue(bMProrateSlipDetails.BMAWBProrateLadderList[5].TotalAmount.Value) : null;
                    Amount6 = bMProrateSlipDetails.BMAWBProrateLadderList[5].TotalAmount.HasValue ? Math.Abs(bMProrateSlipDetails.BMAWBProrateLadderList[5].TotalAmount.Value).ToString() : null;
                }

                if (bMProrateSlipDetails.BMAWBProrateLadderList.Count > 6)
                {
                    FromSector7 = bMProrateSlipDetails.BMAWBProrateLadderList[6].FromSector;
                    ToSector7 = bMProrateSlipDetails.BMAWBProrateLadderList[6].ToSector;
                    CarrierPrefix7 = bMProrateSlipDetails.BMAWBProrateLadderList[6].CarrierPrefix;
                    ProvisoReqSpa7 = bMProrateSlipDetails.BMAWBProrateLadderList[6].ProvisoReqSpa;
                    ProrateFactor7 = bMProrateSlipDetails.BMAWBProrateLadderList[6].ProrateFactor.HasValue ? bMProrateSlipDetails.BMAWBProrateLadderList[6].ProrateFactor.Value.ToString() : null;
                    PercentShare7 = bMProrateSlipDetails.BMAWBProrateLadderList[6].PercentShare.HasValue ? Math.Abs(bMProrateSlipDetails.BMAWBProrateLadderList[6].PercentShare.Value).ToString() : null;
                    AmountSign7 = bMProrateSlipDetails.BMAWBProrateLadderList[6].TotalAmount.HasValue ? Utilities.GetSignValue(bMProrateSlipDetails.BMAWBProrateLadderList[6].TotalAmount.Value) : null;
                    Amount7 = bMProrateSlipDetails.BMAWBProrateLadderList[6].TotalAmount.HasValue ? Math.Abs(bMProrateSlipDetails.BMAWBProrateLadderList[6].TotalAmount.Value).ToString() : null;
                }

                if (bMProrateSlipDetails.BMAWBProrateLadderList.Count > 7)
                {
                    FromSector8 = bMProrateSlipDetails.BMAWBProrateLadderList[7].FromSector;
                    ToSector8 = bMProrateSlipDetails.BMAWBProrateLadderList[7].ToSector;
                    CarrierPrefix8 = bMProrateSlipDetails.BMAWBProrateLadderList[7].CarrierPrefix;
                    ProvisoReqSpa8 = bMProrateSlipDetails.BMAWBProrateLadderList[7].ProvisoReqSpa;
                    ProrateFactor8 = bMProrateSlipDetails.BMAWBProrateLadderList[7].ProrateFactor.HasValue ? bMProrateSlipDetails.BMAWBProrateLadderList[7].ProrateFactor.Value.ToString() : null;
                    PercentShare8 = bMProrateSlipDetails.BMAWBProrateLadderList[7].PercentShare.HasValue ? Math.Abs(bMProrateSlipDetails.BMAWBProrateLadderList[7].PercentShare.Value).ToString() : null;
                    AmountSign8 = bMProrateSlipDetails.BMAWBProrateLadderList[7].TotalAmount.HasValue ? Utilities.GetSignValue(bMProrateSlipDetails.BMAWBProrateLadderList[7].TotalAmount.Value) : null;
                    Amount8 = bMProrateSlipDetails.BMAWBProrateLadderList[7].TotalAmount.HasValue ? Math.Abs(bMProrateSlipDetails.BMAWBProrateLadderList[7].TotalAmount.Value).ToString() : null;
                }
            }
            ProcessNextClass(bMProrateSlipDetails, ref recordSequenceNumber);
        }

        public void ProcessNextClass(BMProrateSlipDetails bMProrateSlipDetails, ref long recordSequenceNumber)
        {
            //Logger.Debug("Processing BMProrateSlipDetails model child objects.");
        }
    }
}