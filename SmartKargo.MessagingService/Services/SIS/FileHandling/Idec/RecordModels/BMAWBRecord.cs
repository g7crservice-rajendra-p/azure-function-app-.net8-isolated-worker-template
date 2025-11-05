using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.Model.SupportingModels;
using QidWorkerRole.SIS.Model;

using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.Write;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    [FixedLengthRecord()]
    public class BMAWBRecord : MemoCouponRecordBase, IRecordToClassConverter<BMAirWayBill>, IClassToRecordConverter<BMAirWayBill>
    {

        [FieldHidden]
        public List<BMAWBVatRecord> BMAWBVatRecordList = new List<BMAWBVatRecord>();

        [FieldHidden]
        public List<BMAwbProrateLadderRecord> BMAwbProrateLadderRecordList = new List<BMAwbProrateLadderRecord>();

        [FieldHidden]
        public List<BMAWBOCBreakdownRecord> BMAWBOCBreakdownRecordList = new List<BMAWBOCBreakdownRecord>();

        public BMAWBRecord(InvoiceRecordBase invoiceRecordBase)
            : base(invoiceRecordBase)
        { }

        public BMAWBRecord()
        {
        }

        public BMAWBRecord(BillingMemoRecord billingMemoRecord)
            : base(billingMemoRecord)
        {
            BillingCreditMemoNumber = billingMemoRecord.BillingOrCreditMemoNumber;
        }

        #region Implementation of IModelConverter<BMAirWayBill>

        /// <summary>
        /// converts it into mode instance.
        /// </summary>
        /// <param name = "multiRecordEngine">MultiRecordEngine instance.</param>
        /// <returns>Returns mode instance created from record.</returns>
        public BMAirWayBill ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            var bMAirWayBill = CreateBMAirWayBillObject();

            ProcessNextRecord(multiRecordEngine, bMAirWayBill);

            return bMAirWayBill;
        }

        /// <summary>
        /// Converts child records of current record into model classes and adds them to the current mode record.
        /// </summary>
        /// <param name = "multiRecordEngine">MultiRecordEngine instance</param>
        /// <param name = "bMAirWayBill">Parent model record to which all the child records will added.</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, BMAirWayBill bMAirWayBill)
        {
            multiRecordEngine.ReadNext();
            var bMAirWayBillRecord = bMAirWayBill;
            var proRateLadderCount = 0;
            do
            {
                if (multiRecordEngine.LastRecord is VatRecordBase)
                {
                    bMAirWayBill.NumberOfChildRecords += 1;

                    var bMAWBVATList = ((IRecordToClassConverter<List<BMAWBVAT>>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    var noOfBMAWBVATList = bMAWBVATList.Count;
                    for (var i = 0; i < noOfBMAWBVATList; i++)
                    {
                        bMAirWayBillRecord.BMAWBVATList.Add(bMAWBVATList[i]);
                    }

                    //Logger.Debug("BMAWBVAT List Object is added to BMAirWayBill Record object successfully.");
                }
                else if (multiRecordEngine.LastRecord is BMAWBOCBreakdownRecord)
                {
                    bMAirWayBill.NumberOfChildRecords += 1;

                    var bMAWBOtherChargesList = ((IRecordToClassConverter<List<BMAWBOtherCharges>>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    var noOfBMAWBOtherChargesRecords = bMAWBOtherChargesList.Count;
                    for (var i = 0; i < noOfBMAWBOtherChargesRecords; i++)
                    {
                        bMAirWayBillRecord.BMAWBOtherChargesList.Add(bMAWBOtherChargesList[i]);
                    }

                    //Logger.Debug("BMAWBOtherCharges List Object is added to BMAirWayBill Data Record object successfully.");
                }
                else if (multiRecordEngine.LastRecord is BMAwbProrateLadderRecord)
                {
                    bMAirWayBill.NumberOfChildRecords += 1;

                    var bMProrateSlipDetails = ((IRecordToClassConverter<BMProrateSlipDetails>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    if (bMProrateSlipDetails != null)
                    {
                        bMAirWayBillRecord.TotalProrateAmount = bMProrateSlipDetails.TotalProrateAmount;
                        bMAirWayBillRecord.ProrateCalCurrencyId = bMProrateSlipDetails.ProrateCalCurrencyId;

                        foreach (var bMAWBProrateLadder in bMProrateSlipDetails.BMAWBProrateLadderList)
                        {
                            bMAWBProrateLadder.SequenceNumber = ++proRateLadderCount;
                            bMAirWayBillRecord.BMAWBProrateLadderList.Add(bMAWBProrateLadder);
                        }
                    }

                    //Logger.Debug("BMAwbProrateLadderRecord List Object is added to BMAirWayBill Data Record object successfully.");
                }
                else
                {
                    break;
                }
            }
            while (multiRecordEngine.LastRecord != null);
        }

        #endregion

        /// <summary>
        /// Creates coupon record object with information from IDEC coupon record.
        /// </summary>
        /// <returns>Returns Coupon Record object.</returns>
        private BMAirWayBill CreateBMAirWayBillObject()
        {
            var bMAirWayBill = new BMAirWayBill
                                 {
                                     AWBSerialNumber = Convert.ToInt32(AWBSerialNumber),
                                     AWBIssuingAirline = AWBIssuingAirline,
                                     CurrencyAdjustmentIndicator = CurrencyAdjustmentIndicator.Trim(),
                                     BillingCode = BillingCode,
                                     From = FromAirport.Trim(),
                                     To = ToAirport.Trim(),
                                     Origin = Origin.Trim(),
                                     Destination = Destination.Trim(),
                                     BilledWeight = Convert.ToInt32(BilledWeight),
                                     ProvisionalReqSpa = ProvisoOrReqOrSpa,
                                     PrpratePercentage = Convert.ToInt32(ProratePercent),
                                     PartShipmentIndicator = PartShipmentIndicator,
                                     KgLbIndicator = KgOrLbIndicator,
                                     BilledWeightCharge = Utilities.GetActualValueForDouble(WeightChargesBilledOrCreditedSign, WeightChargesBilledOrCredited),
                                     BilledOtherCharge = Utilities.GetActualValueForDouble(OtherChargesBilledOrCreditedSign, OtherChargesBilledOrCredited),
                                     BilledAmtSubToIsc = Utilities.GetActualValueForDouble(AmountSubjectedToIscBilledOrCreditedSign, AmountSubjectedToIscBilledOrCredited),
                                     BilledIscPercentage = Utilities.GetActualValueForDouble(IscPercentBilledOrCreditedSign, IscPercentBilledOrCredited),
                                     BilledValuationCharge = Utilities.GetActualValueForDouble(ValuationChargesBilledOrCreditedSign, ValuationChargesBilledOrCredited),
                                     BilledVatAmount = Utilities.GetActualValueForDouble(VatAmountBilledOrCreditedSign, VatAmountBilledOrCredited),
                                     BilledIscAmount = Utilities.GetActualValueForDouble(IscAmountBilledOrCreditedSign, IscAmountBilledOrCredited),
                                     TotalAmount = Utilities.GetActualValueForDouble(TotalAmountBilledOrCreditedSign, TotalAmountBilledOrCredited),
                                     ReasonCode = ReasonCode,
                                     ReferenceField1 = ReferenceField1,
                                     ReferenceField2 = ReferenceField2,
                                     ReferenceField3 = ReferenceField3,
                                     ReferenceField4 = ReferenceField4,
                                     ReferenceField5 = ReferenceField5,
                                     AirlineOwnUse = AirlineOwnUse,
                                     AWBCheckDigit = int.Parse(CheckDigit),
                                     CcaIndicator = CCAIndicator == IdecConstants.TrueValue,
                                     BreakdownSerialNumber = int.Parse(BreakdownSerialNumber)
                                 };
            // Invoice Date
            DateTime awbDate;

            // To avoid converting year 30 into year 1930
            var cultureInfo = new CultureInfo("en-US");
            cultureInfo.Calendar.TwoDigitYearMax = 2099;

            if (DateTime.TryParseExact(AWBIssueDate, IdecConstants.InvoiceDateFormat, cultureInfo, DateTimeStyles.None, out awbDate))
            {
                bMAirWayBill.AWBDate = awbDate;
            }
            else
            {
                bMAirWayBill.AwbDateDisplayText = AWBIssueDate;
            }

            bMAirWayBill.DateOfCarriageOrTransfer = DateOfCarriage;

            if (AttachmentIndicatorOriginal.Equals("Y"))
            {
                bMAirWayBill.AttachmentIndicatorOriginal = "Y";
            }
            //Logger.Debug("BMAirWayBill Record object created successfully.");

            return bMAirWayBill;
        }

        /// <summary>
        ///  This method converts the BMAirWayBill model into corresponding record instance
        /// </summary>
        /// <param name="bMAirWayBill">BMAirWayBill</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ConvertClassToRecord(BMAirWayBill bMAirWayBill, ref long recordSequenceNumber)
        {
            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiBMAwbRecord;
            BreakdownSerialNumber = bMAirWayBill.BreakdownSerialNumber.ToString();
            BillingCode = bMAirWayBill.BillingCode;
            AWBIssueDate = GetFormattedDate(bMAirWayBill.AWBDate);

            if (!string.IsNullOrWhiteSpace(bMAirWayBill.AWBIssuingAirline))
            {
                AWBIssuingAirline = Utilities.GetNumericMemberCode(bMAirWayBill.AWBIssuingAirline).ToString();
            }

            AWBSerialNumber = bMAirWayBill.AWBSerialNumber.ToString();
            CheckDigit = bMAirWayBill.AWBCheckDigit.ToString();
            Origin = bMAirWayBill.Origin;
            Destination = bMAirWayBill.Destination;
            FromAirport = bMAirWayBill.From;
            ToAirport = bMAirWayBill.To;
            DateOfCarriage = bMAirWayBill.DateOfCarriageOrTransfer;
            WeightChargesBilledOrCredited = bMAirWayBill.BilledWeightCharge.HasValue ? Math.Abs(bMAirWayBill.BilledWeightCharge.Value).ToString() : null;
            WeightChargesBilledOrCreditedSign = bMAirWayBill.BilledWeightCharge.HasValue ? Utilities.GetSignValue(bMAirWayBill.BilledWeightCharge.Value) : null;
            ValuationChargesBilledOrCredited = bMAirWayBill.BilledValuationCharge.HasValue ? Math.Abs(bMAirWayBill.BilledValuationCharge.Value).ToString() : null;
            ValuationChargesBilledOrCreditedSign = bMAirWayBill.BilledValuationCharge.HasValue ? Utilities.GetSignValue(bMAirWayBill.BilledValuationCharge.Value) : null;
            OtherChargesBilledOrCredited = Math.Abs(bMAirWayBill.BilledOtherCharge).ToString();
            OtherChargesBilledOrCreditedSign = Utilities.GetSignValue(bMAirWayBill.BilledOtherCharge);
            AmountSubjectedToIscBilledOrCredited = Math.Abs(bMAirWayBill.BilledAmtSubToIsc).ToString();
            AmountSubjectedToIscBilledOrCreditedSign = Utilities.GetSignValue(bMAirWayBill.BilledAmtSubToIsc);
            IscPercentBilledOrCredited = Math.Abs(bMAirWayBill.BilledIscPercentage).ToString();
            IscPercentBilledOrCreditedSign = Utilities.GetSignValue(bMAirWayBill.BilledIscPercentage);
            IscAmountBilledOrCredited = Math.Abs(bMAirWayBill.BilledIscAmount).ToString();
            IscAmountBilledOrCreditedSign = Utilities.GetSignValue(bMAirWayBill.BilledIscAmount);
            VatAmountBilledOrCredited = bMAirWayBill.BilledVatAmount.HasValue ? Math.Abs(bMAirWayBill.BilledVatAmount.Value).ToString() : null;
            VatAmountBilledOrCreditedSign = bMAirWayBill.BilledVatAmount.HasValue ? Utilities.GetSignValue(bMAirWayBill.BilledVatAmount.Value) : null;
            TotalAmountBilledOrCredited = Math.Abs(bMAirWayBill.TotalAmount).ToString();
            TotalAmountBilledOrCreditedSign = Utilities.GetSignValue(bMAirWayBill.TotalAmount);
            CurrencyAdjustmentIndicator = bMAirWayBill.CurrencyAdjustmentIndicator;
            BilledWeight = bMAirWayBill.BilledWeight.HasValue ? bMAirWayBill.BilledWeight.Value.ToString() : null;
            ProvisoOrReqOrSpa = bMAirWayBill.ProvisionalReqSpa;
            ProratePercent = bMAirWayBill.PrpratePercentage.HasValue ? bMAirWayBill.PrpratePercentage.Value.ToString() : null;
            PartShipmentIndicator = bMAirWayBill.PartShipmentIndicator;
            KgOrLbIndicator = bMAirWayBill.KgLbIndicator;
            CCAIndicator = Utilities.GetBooDisplaylValue(bMAirWayBill.CcaIndicator);
            AttachmentIndicatorOriginal = bMAirWayBill.AttachmentIndicatorOriginal;
            AttachmentIndicatorValidated = bMAirWayBill.AttachmentIndicatorValidated;
            NumberOfAttachments = bMAirWayBill.NumberOfAttachments.HasValue ? bMAirWayBill.NumberOfAttachments.Value.ToString() : "0";
            // This field is an output only field and must be blank in case of an input file to SIS.
            ISValidationFlag = string.Empty; // bMAirWayBill.ISValidationFlag;
            ReasonCode = bMAirWayBill.ReasonCode;
            ReferenceField1 = bMAirWayBill.ReferenceField1;
            ReferenceField2 = bMAirWayBill.ReferenceField2;
            ReferenceField3 = bMAirWayBill.ReferenceField3;
            ReferenceField4 = bMAirWayBill.ReferenceField4;
            ReferenceField5 = bMAirWayBill.ReferenceField5;
            AirlineOwnUse = bMAirWayBill.AirlineOwnUse;

            ProcessNextClass(bMAirWayBill, ref recordSequenceNumber);
        }

        /// <summary>
        /// Convert Child models into records
        /// </summary>
        /// <param name="bMAirWayBill">BMAirWayBill</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ProcessNextClass(BMAirWayBill bMAirWayBill, ref long recordSequenceNumber)
        {
            if (bMAirWayBill != null)
            {
                if (bMAirWayBill.BMAWBProrateLadderList.Count > 0)
                {
                    foreach (var bMAWBProrateLadderList in Utilities.GetDividedSubCollections(bMAirWayBill.BMAWBProrateLadderList.OrderBy(i => i.SequenceNumber).ToList(), 8))
                    {
                        var bMAwbProrateLadderRecord = new BMAwbProrateLadderRecord(this);

                        var bMProrateSlipDetails = new BMProrateSlipDetails { TotalProrateAmount = bMAirWayBill.TotalProrateAmount.HasValue ? bMAirWayBill.TotalProrateAmount.Value : 0, ProrateCalCurrencyId = bMAirWayBill.ProrateCalCurrencyId };
                        bMProrateSlipDetails.BMAWBProrateLadderList.AddRange(bMAWBProrateLadderList);
                        bMAwbProrateLadderRecord.ConvertClassToRecord(bMProrateSlipDetails, ref recordSequenceNumber);

                        BMAwbProrateLadderRecordList.Add(bMAwbProrateLadderRecord);
                    }
                }

                if (bMAirWayBill.BMAWBVATList.Count > 0)
                {
                    // Add BMAirWayBill in the list
                    foreach (var bMAWBVATList in Utilities.GetDividedSubCollections(bMAirWayBill.BMAWBVATList, 2))
                    {
                        var bMAWBVatRecord = new BMAWBVatRecord(this);
                        bMAWBVatRecord.ConvertClassToRecord(bMAWBVATList, ref recordSequenceNumber);
                        BMAWBVatRecordList.Add(bMAWBVatRecord);
                    }
                }

                if (bMAirWayBill.BMAWBOtherChargesList.Count > 0)
                {
                    foreach (var bMAWBOtherChargesList in Utilities.GetDividedSubCollections(bMAirWayBill.BMAWBOtherChargesList, 3))
                    {
                        var bMAWBOCBreakdownRecord = new BMAWBOCBreakdownRecord(this);
                        bMAWBOCBreakdownRecord.ConvertClassToRecord(bMAWBOtherChargesList, ref recordSequenceNumber);
                        BMAWBOCBreakdownRecordList.Add(bMAWBOCBreakdownRecord);
                    }
                }
            }
        }

        /// <summary>
        /// gets date in the format 'YYMMDD'
        /// </summary>
        /// <param name="dateTime">dateTime</param>
        /// <returns>YYMMDD</returns>
        private static string GetFormattedDate(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                string yearStr = dateTime.Value.Year.ToString();
                string monthStr = dateTime.Value.Month.ToString();
                string dayStr = dateTime.Value.Day.ToString();

                // This is case when year is passed as 4 digit number as 2009.
                if (yearStr.Length == 4)
                {
                    yearStr = yearStr.Substring(2, 2);
                }

                return string.Format("{0}{1}{2}", yearStr.PadLeft(2, '0'), monthStr.PadLeft(2, '0'), dayStr.PadLeft(2, '0'));
            }
            return string.Empty;
        }
    }
}