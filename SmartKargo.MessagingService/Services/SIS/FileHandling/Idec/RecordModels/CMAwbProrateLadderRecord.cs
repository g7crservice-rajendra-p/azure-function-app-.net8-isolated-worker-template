using System;
using System.Collections.Generic;
using System.Reflection;
using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.Model;
using QidWorkerRole.SIS.Model.SupportingModels;

using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.Write;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    public class CMAwbProrateLadderRecord : InvoiceRecordBase, IRecordToClassConverter<CMProrateSlipDetails>, IClassToRecordConverter<CMProrateSlipDetails>
    {
        
        [FieldFixedLength(11)]
        public string CreditMemoNumber;

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

        public CMAwbProrateLadderRecord()
        {
        }

        public CMAwbProrateLadderRecord(CMAWBRecord cMAWBRecord)
            : base(cMAWBRecord)
        {
            CreditMemoNumber = cMAWBRecord.BillingCreditMemoNumber;
            AwbIssuingAirline = cMAWBRecord.AWBIssuingAirline;
            AwbSerialNumber = cMAWBRecord.AWBSerialNumber;
            AwbCheckDigit = cMAWBRecord.CheckDigit;
            BillingCode = cMAWBRecord.BillingCode;
        }

        /// <summary>
        /// This method creates Prorate Ladder Record Vat objects for all the Vat records in Idec record read from the input file.
        /// </summary>
        /// <returns>List of Prorate Ladder Record Vat objects.</returns>
        private CMProrateSlipDetails CreateProrateLadderBreakdownObjects()
        {
            var cMProrateSlipDetails = new CMProrateSlipDetails { TotalProrateAmount = Convert.ToDouble(TotalAmount), ProrateCalCurrencyId = ProrateCalCurrencyId };

            var createProrateLadderBreakdownObjects = new List<CMAWBProrateLadder>();

            //Logger.Debug("Creating Prorate Ladder Object list for Prorate Ladder Record object.");

            // If vat id is not empty then only create Vat object.
            if (!string.IsNullOrWhiteSpace(FromSector1) && !string.IsNullOrWhiteSpace(ToSector1))
            {
                createProrateLadderBreakdownObjects.Add(new CMAWBProrateLadder
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
                createProrateLadderBreakdownObjects.Add(new CMAWBProrateLadder
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
                createProrateLadderBreakdownObjects.Add(new CMAWBProrateLadder
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
                createProrateLadderBreakdownObjects.Add(new CMAWBProrateLadder
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
                createProrateLadderBreakdownObjects.Add(new CMAWBProrateLadder
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
                createProrateLadderBreakdownObjects.Add(new CMAWBProrateLadder
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
                createProrateLadderBreakdownObjects.Add(new CMAWBProrateLadder
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
                createProrateLadderBreakdownObjects.Add(new CMAWBProrateLadder
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

            cMProrateSlipDetails.CMAWBProrateLadderList.AddRange(createProrateLadderBreakdownObjects);
            return cMProrateSlipDetails;
        }

        public CMProrateSlipDetails ConvertRecordToClass(FileHelpers.MultiRecordEngine multiRecordEngine)
        {
            var couponRecordVatObjects = CreateProrateLadderBreakdownObjects();

            ProcessNextRecord(multiRecordEngine, couponRecordVatObjects);

            return couponRecordVatObjects;
        }

        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, CMProrateSlipDetails cMProrateSlipDetails)
        {
            multiRecordEngine.ReadNext();
        }

        public void ConvertClassToRecord(CMProrateSlipDetails cMProrateSlipDetails, ref long recordSequenceNumber)
        {
            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiProrateLadderRecord;

            ProrateCalCurrencyId = cMProrateSlipDetails.ProrateCalCurrencyId;
            TotalAmountSign = Utilities.GetSignValue(cMProrateSlipDetails.TotalProrateAmount);
            TotalAmount = Math.Abs(cMProrateSlipDetails.TotalProrateAmount).ToString();

            if (cMProrateSlipDetails.CMAWBProrateLadderList.Count > 0)
            {
                FromSector1 = cMProrateSlipDetails.CMAWBProrateLadderList[0].FromSector;
                ToSector1 = cMProrateSlipDetails.CMAWBProrateLadderList[0].ToSector;
                CarrierPrefix1 = cMProrateSlipDetails.CMAWBProrateLadderList[0].CarrierPrefix;
                ProvisoReqSpa1 = cMProrateSlipDetails.CMAWBProrateLadderList[0].ProvisoReqSpa;
                ProrateFactor1 = cMProrateSlipDetails.CMAWBProrateLadderList[0].ProrateFactor.HasValue ? cMProrateSlipDetails.CMAWBProrateLadderList[0].ProrateFactor.Value.ToString() : null;
                PercentShare1 = cMProrateSlipDetails.CMAWBProrateLadderList[0].PercentShare.HasValue ? Math.Abs(cMProrateSlipDetails.CMAWBProrateLadderList[0].PercentShare.Value).ToString() : null;
                AmountSign1 = cMProrateSlipDetails.CMAWBProrateLadderList[0].TotalAmount.HasValue ? Utilities.GetSignValue(cMProrateSlipDetails.CMAWBProrateLadderList[0].TotalAmount.Value) : null;
                Amount1 = cMProrateSlipDetails.CMAWBProrateLadderList[0].TotalAmount.HasValue ? Math.Abs(cMProrateSlipDetails.CMAWBProrateLadderList[0].TotalAmount.Value).ToString() : null;

                if (cMProrateSlipDetails.CMAWBProrateLadderList.Count > 1)
                {
                    FromSector2 = cMProrateSlipDetails.CMAWBProrateLadderList[1].FromSector;
                    ToSector2 = cMProrateSlipDetails.CMAWBProrateLadderList[1].ToSector;
                    CarrierPrefix2 = cMProrateSlipDetails.CMAWBProrateLadderList[1].CarrierPrefix;
                    ProvisoReqSpa2 = cMProrateSlipDetails.CMAWBProrateLadderList[1].ProvisoReqSpa;
                    ProrateFactor2 = cMProrateSlipDetails.CMAWBProrateLadderList[1].ProrateFactor.HasValue ? cMProrateSlipDetails.CMAWBProrateLadderList[1].ProrateFactor.Value.ToString() : null;
                    PercentShare2 = cMProrateSlipDetails.CMAWBProrateLadderList[1].PercentShare.HasValue ? Math.Abs(cMProrateSlipDetails.CMAWBProrateLadderList[1].PercentShare.Value).ToString() : null;
                    AmountSign2 = cMProrateSlipDetails.CMAWBProrateLadderList[1].TotalAmount.HasValue ? Utilities.GetSignValue(cMProrateSlipDetails.CMAWBProrateLadderList[1].TotalAmount.Value) : null;
                    Amount2 = cMProrateSlipDetails.CMAWBProrateLadderList[1].TotalAmount.HasValue ? Math.Abs(cMProrateSlipDetails.CMAWBProrateLadderList[1].TotalAmount.Value).ToString() : null;
                }

                if (cMProrateSlipDetails.CMAWBProrateLadderList.Count > 2)
                {
                    FromSector3 = cMProrateSlipDetails.CMAWBProrateLadderList[2].FromSector;
                    ToSector3 = cMProrateSlipDetails.CMAWBProrateLadderList[2].ToSector;
                    CarrierPrefix3 = cMProrateSlipDetails.CMAWBProrateLadderList[2].CarrierPrefix;
                    ProvisoReqSpa3 = cMProrateSlipDetails.CMAWBProrateLadderList[2].ProvisoReqSpa;
                    ProrateFactor3 = cMProrateSlipDetails.CMAWBProrateLadderList[2].ProrateFactor.HasValue ? cMProrateSlipDetails.CMAWBProrateLadderList[2].ProrateFactor.Value.ToString() : null;
                    PercentShare3 = cMProrateSlipDetails.CMAWBProrateLadderList[2].PercentShare.HasValue ? Math.Abs(cMProrateSlipDetails.CMAWBProrateLadderList[2].PercentShare.Value).ToString() : null;
                    AmountSign3 = cMProrateSlipDetails.CMAWBProrateLadderList[2].TotalAmount.HasValue ? Utilities.GetSignValue(cMProrateSlipDetails.CMAWBProrateLadderList[2].TotalAmount.Value) : null;
                    Amount3 = cMProrateSlipDetails.CMAWBProrateLadderList[2].TotalAmount.HasValue ? Math.Abs(cMProrateSlipDetails.CMAWBProrateLadderList[2].TotalAmount.Value).ToString() : null;
                }

                if (cMProrateSlipDetails.CMAWBProrateLadderList.Count > 3)
                {
                    FromSector4 = cMProrateSlipDetails.CMAWBProrateLadderList[3].FromSector;
                    ToSector4 = cMProrateSlipDetails.CMAWBProrateLadderList[3].ToSector;
                    CarrierPrefix4 = cMProrateSlipDetails.CMAWBProrateLadderList[3].CarrierPrefix;
                    ProvisoReqSpa4 = cMProrateSlipDetails.CMAWBProrateLadderList[3].ProvisoReqSpa;
                    ProrateFactor4 = cMProrateSlipDetails.CMAWBProrateLadderList[3].ProrateFactor.HasValue ? cMProrateSlipDetails.CMAWBProrateLadderList[3].ProrateFactor.Value.ToString() : null;
                    PercentShare4 = cMProrateSlipDetails.CMAWBProrateLadderList[3].PercentShare.HasValue ? Math.Abs(cMProrateSlipDetails.CMAWBProrateLadderList[3].PercentShare.Value).ToString() : null;
                    AmountSign4 = cMProrateSlipDetails.CMAWBProrateLadderList[3].TotalAmount.HasValue ? Utilities.GetSignValue(cMProrateSlipDetails.CMAWBProrateLadderList[3].TotalAmount.Value) : null;
                    Amount4 = cMProrateSlipDetails.CMAWBProrateLadderList[3].TotalAmount.HasValue ? Math.Abs(cMProrateSlipDetails.CMAWBProrateLadderList[3].TotalAmount.Value).ToString() : null;
                }

                if (cMProrateSlipDetails.CMAWBProrateLadderList.Count > 4)
                {
                    FromSector5 = cMProrateSlipDetails.CMAWBProrateLadderList[4].FromSector;
                    ToSector5 = cMProrateSlipDetails.CMAWBProrateLadderList[4].ToSector;
                    CarrierPrefix5 = cMProrateSlipDetails.CMAWBProrateLadderList[4].CarrierPrefix;
                    ProvisoReqSpa5 = cMProrateSlipDetails.CMAWBProrateLadderList[4].ProvisoReqSpa;
                    ProrateFactor5 = cMProrateSlipDetails.CMAWBProrateLadderList[4].ProrateFactor.HasValue ? cMProrateSlipDetails.CMAWBProrateLadderList[4].ProrateFactor.Value.ToString() : null;
                    PercentShare5 = cMProrateSlipDetails.CMAWBProrateLadderList[4].PercentShare.HasValue ? Math.Abs(cMProrateSlipDetails.CMAWBProrateLadderList[4].PercentShare.Value).ToString() : null;
                    AmountSign5 = cMProrateSlipDetails.CMAWBProrateLadderList[4].TotalAmount.HasValue ? Utilities.GetSignValue(cMProrateSlipDetails.CMAWBProrateLadderList[4].TotalAmount.Value) : null;
                    Amount5 = cMProrateSlipDetails.CMAWBProrateLadderList[4].TotalAmount.HasValue ? Math.Abs(cMProrateSlipDetails.CMAWBProrateLadderList[4].TotalAmount.Value).ToString() : null;
                }

                if (cMProrateSlipDetails.CMAWBProrateLadderList.Count > 5)
                {
                    FromSector6 = cMProrateSlipDetails.CMAWBProrateLadderList[5].FromSector;
                    ToSector6 = cMProrateSlipDetails.CMAWBProrateLadderList[5].ToSector;
                    CarrierPrefix6 = cMProrateSlipDetails.CMAWBProrateLadderList[5].CarrierPrefix;
                    ProvisoReqSpa6 = cMProrateSlipDetails.CMAWBProrateLadderList[5].ProvisoReqSpa;
                    ProrateFactor6 = cMProrateSlipDetails.CMAWBProrateLadderList[5].ProrateFactor.HasValue ? cMProrateSlipDetails.CMAWBProrateLadderList[5].ProrateFactor.Value.ToString() : null;
                    PercentShare6 = cMProrateSlipDetails.CMAWBProrateLadderList[5].PercentShare.HasValue ? Math.Abs(cMProrateSlipDetails.CMAWBProrateLadderList[5].PercentShare.Value).ToString() : null;
                    AmountSign6 = cMProrateSlipDetails.CMAWBProrateLadderList[5].TotalAmount.HasValue ? Utilities.GetSignValue(cMProrateSlipDetails.CMAWBProrateLadderList[5].TotalAmount.Value) : null;
                    Amount6 = cMProrateSlipDetails.CMAWBProrateLadderList[5].TotalAmount.HasValue ? Math.Abs(cMProrateSlipDetails.CMAWBProrateLadderList[5].TotalAmount.Value).ToString() : null;
                }

                if (cMProrateSlipDetails.CMAWBProrateLadderList.Count > 6)
                {
                    FromSector7 = cMProrateSlipDetails.CMAWBProrateLadderList[6].FromSector;
                    ToSector7 = cMProrateSlipDetails.CMAWBProrateLadderList[6].ToSector;
                    CarrierPrefix7 = cMProrateSlipDetails.CMAWBProrateLadderList[6].CarrierPrefix;
                    ProvisoReqSpa7 = cMProrateSlipDetails.CMAWBProrateLadderList[6].ProvisoReqSpa;
                    ProrateFactor7 = cMProrateSlipDetails.CMAWBProrateLadderList[6].ProrateFactor.HasValue ? cMProrateSlipDetails.CMAWBProrateLadderList[6].ProrateFactor.Value.ToString() : null;
                    PercentShare7 = cMProrateSlipDetails.CMAWBProrateLadderList[6].PercentShare.HasValue ? Math.Abs(cMProrateSlipDetails.CMAWBProrateLadderList[6].PercentShare.Value).ToString() : null;
                    AmountSign7 = cMProrateSlipDetails.CMAWBProrateLadderList[6].TotalAmount.HasValue ? Utilities.GetSignValue(cMProrateSlipDetails.CMAWBProrateLadderList[6].TotalAmount.Value) : null;
                    Amount7 = cMProrateSlipDetails.CMAWBProrateLadderList[6].TotalAmount.HasValue ? Math.Abs(cMProrateSlipDetails.CMAWBProrateLadderList[6].TotalAmount.Value).ToString() : null;
                }

                if (cMProrateSlipDetails.CMAWBProrateLadderList.Count > 7)
                {
                    FromSector8 = cMProrateSlipDetails.CMAWBProrateLadderList[7].FromSector;
                    ToSector8 = cMProrateSlipDetails.CMAWBProrateLadderList[7].ToSector;
                    CarrierPrefix8 = cMProrateSlipDetails.CMAWBProrateLadderList[7].CarrierPrefix;
                    ProvisoReqSpa8 = cMProrateSlipDetails.CMAWBProrateLadderList[7].ProvisoReqSpa;
                    ProrateFactor8 = cMProrateSlipDetails.CMAWBProrateLadderList[7].ProrateFactor.HasValue ? cMProrateSlipDetails.CMAWBProrateLadderList[7].ProrateFactor.Value.ToString() : null;
                    PercentShare8 = cMProrateSlipDetails.CMAWBProrateLadderList[7].PercentShare.HasValue ? Math.Abs(cMProrateSlipDetails.CMAWBProrateLadderList[7].PercentShare.Value).ToString() : null;
                    AmountSign8 = cMProrateSlipDetails.CMAWBProrateLadderList[7].TotalAmount.HasValue ? Utilities.GetSignValue(cMProrateSlipDetails.CMAWBProrateLadderList[7].TotalAmount.Value) : null;
                    Amount8 = cMProrateSlipDetails.CMAWBProrateLadderList[7].TotalAmount.HasValue ? Math.Abs(cMProrateSlipDetails.CMAWBProrateLadderList[7].TotalAmount.Value).ToString() : null;
                }
            }
            ProcessNextClass(cMProrateSlipDetails, ref recordSequenceNumber);
        }

        public void ProcessNextClass(CMProrateSlipDetails cMProrateSlipDetails, ref long recordSequenceNumber)
        {
        }
    }
}