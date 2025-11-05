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
    public class CMAWBRecord : MemoCouponRecordBase, IRecordToClassConverter<CMAirWayBill>, IClassToRecordConverter<CMAirWayBill>
    {
       
        [FieldHidden]
        public List<CMAWBVatRecord> CMAWBVatRecordList = new List<CMAWBVatRecord>();

        [FieldHidden]
        public List<CMAwbProrateLadderRecord> CMAwbProrateLadderRecordList = new List<CMAwbProrateLadderRecord>();

        [FieldHidden]
        public List<CMAWBOCBreakdownRecord> CMAWBOCBreakdownRecordList = new List<CMAWBOCBreakdownRecord>();

        public CMAWBRecord()
        {
        }

        public CMAWBRecord(CreditMemoRecord creditMemoRecord)
            : base(creditMemoRecord)
        {
            BillingCreditMemoNumber = creditMemoRecord.BillingOrCreditMemoNumber;
        }

        /// <summary>
        /// This constructor will set the the properties of InvoiceBase record class
        /// </summary>
        /// <param name="invoiceRecordBase"></param>
        public CMAWBRecord(InvoiceRecordBase invoiceRecordBase)
            : base(invoiceRecordBase)
        { }

        #region Implementation of IModelConverter<CMAirWayBill>

        /// <summary>
        /// Converts it into CMAirWayBill model instance.
        /// </summary>
        /// <param name = "multiRecordEngine">MultiRecordEngine instance.</param>
        /// <returns>Returns CMAirWayBill model instance created from record.</returns>
        public CMAirWayBill ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            var cMAirWayBillRecord = CreateCMAirWayBillRecordObject();

            ProcessNextRecord(multiRecordEngine, cMAirWayBillRecord);

            return cMAirWayBillRecord;
        }

        /// <summary>
        /// Converts child records of current record into model classes and adds them to the current mode record.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="cMAirWayBill">Parent model record to which all the child records will added.</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, CMAirWayBill cMAirWayBill)
        {
            multiRecordEngine.ReadNext();
            var cMAirWayBillRecord = cMAirWayBill;

            var prorateLadderCount = 0;

            do
            {
                if (multiRecordEngine.LastRecord is VatRecordBase)
                {
                    cMAirWayBill.NumberOfChildRecords += 1;

                    var cMAWBVATList = ((IRecordToClassConverter<List<CMAWBVAT>>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    var noOfCMAWBVATListRecords = cMAWBVATList.Count;
                    for (var i = 0; i < noOfCMAWBVATListRecords; i++)
                    {
                        cMAirWayBillRecord.CMAWBVATList.Add(cMAWBVATList[i]);
                    }

                    //Logger.Debug("CMAWBVAT List Object is added to CMAirWayBillRecord object successfully.");
                }
                else if (multiRecordEngine.LastRecord is CMAWBOCBreakdownRecord)
                {
                    cMAirWayBill.NumberOfChildRecords += 1;

                    var cMAWBOtherChargesList = ((IRecordToClassConverter<List<CMAWBOtherCharges>>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    var noOfCMAWBOtherChargesRecords = cMAWBOtherChargesList.Count;
                    for (var i = 0; i < noOfCMAWBOtherChargesRecords; i++)
                    {
                        cMAirWayBillRecord.CMAWBOtherChargesList.Add(cMAWBOtherChargesList[i]);
                    }

                    //Logger.Debug("CMAWBOtherCharges List Object is added to CMAirWayBillRecord object successfully.");
                }
                else if (multiRecordEngine.LastRecord is CMAwbProrateLadderRecord)
                {
                    cMAirWayBill.NumberOfChildRecords += 1;

                    var cMProrateSlipDetails = ((IRecordToClassConverter<CMProrateSlipDetails>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    if (cMProrateSlipDetails != null)
                    {
                        cMAirWayBillRecord.TotalProrateAmount = cMProrateSlipDetails.TotalProrateAmount;
                        cMAirWayBillRecord.ProrateCalCurrencyId = cMProrateSlipDetails.ProrateCalCurrencyId;

                        foreach (var cMAWBProrateLadder in cMProrateSlipDetails.CMAWBProrateLadderList)
                        {
                            cMAWBProrateLadder.SequenceNumber = ++prorateLadderCount;
                            cMAirWayBillRecord.CMAWBProrateLadderList.Add(cMAWBProrateLadder);
                        }
                    }
                    //Logger.Debug("CMAwbProrateLadderRecord List Object is added to CMAirWayBillRecord object successfully.");
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
        private CMAirWayBill CreateCMAirWayBillRecordObject()
        {
            var cMAirWayBill = new CMAirWayBill
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
                ProratePercentage = Convert.ToInt32(ProratePercent),
                PartShipmentIndicator = PartShipmentIndicator,
                KgLbIndicator = KgOrLbIndicator,
                CreditedWeightCharge = (double)Utilities.GetActualValueForDecimal(WeightChargesBilledOrCreditedSign, WeightChargesBilledOrCredited),
                CreditedOtherCharge = (double)Utilities.GetActualValueForDecimal(OtherChargesBilledOrCreditedSign, OtherChargesBilledOrCredited),
                CreditedAmtSubToIsc = (double)Utilities.GetActualValueForDecimal(AmountSubjectedToIscBilledOrCreditedSign, AmountSubjectedToIscBilledOrCredited),
                CreditedIscPercentage = Utilities.GetActualValueForDouble(IscPercentBilledOrCreditedSign, IscPercentBilledOrCredited),
                CreditedValuationCharge = (double)Utilities.GetActualValueForDecimal(ValuationChargesBilledOrCreditedSign, ValuationChargesBilledOrCredited),
                CreditedVatAmount = Utilities.GetActualValueForDouble(VatAmountBilledOrCreditedSign, VatAmountBilledOrCredited),
                CreditedIscAmount = Utilities.GetActualValueForDouble(IscAmountBilledOrCreditedSign, IscAmountBilledOrCredited),
                TotalAmountCredited = (double)Utilities.GetActualValueForDecimal(TotalAmountBilledOrCreditedSign, TotalAmountBilledOrCredited),
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
                cMAirWayBill.AWBDate = awbDate;
            }
            else
            {
                cMAirWayBill.AwbDateDisplayText = AWBIssueDate;
            }

            cMAirWayBill.DateOfCarriageOrTransfer = DateOfCarriage;

            if (AttachmentIndicatorOriginal.Equals("Y"))
            {
                cMAirWayBill.AttachmentIndicatorOriginal = "Y";
            }
            //Logger.Debug("CMAirWayBill Data record object created successfully.");

            return cMAirWayBill;
        }

        /// <summary>
        /// This method converts the CMAirWayBill model into corresponding record instance
        /// </summary>
        /// <param name="cMAirWayBill">CMAirWayBill</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ConvertClassToRecord(CMAirWayBill cMAirWayBill, ref long recordSequenceNumber)
        {
            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiCMAwbRecord;
            BillingCode = cMAirWayBill.BillingCode;
            BreakdownSerialNumber = cMAirWayBill.BreakdownSerialNumber.ToString();
            AWBIssueDate = GetFormattedDate(cMAirWayBill.AWBDate);

            if (!string.IsNullOrWhiteSpace(cMAirWayBill.AWBIssuingAirline))
            {
                AWBIssuingAirline = Utilities.GetNumericMemberCode(cMAirWayBill.AWBIssuingAirline).ToString();
            }

            AWBSerialNumber = cMAirWayBill.AWBSerialNumber.ToString();
            CheckDigit = cMAirWayBill.AWBCheckDigit.ToString();
            Origin = cMAirWayBill.Origin;
            Destination = cMAirWayBill.Destination;
            FromAirport = cMAirWayBill.From;
            ToAirport = cMAirWayBill.To;
            DateOfCarriage = cMAirWayBill.DateOfCarriageOrTransfer;

            WeightChargesBilledOrCredited = cMAirWayBill.CreditedWeightCharge.HasValue ? Math.Abs(cMAirWayBill.CreditedWeightCharge.Value).ToString() : null;
            WeightChargesBilledOrCreditedSign = cMAirWayBill.CreditedWeightCharge.HasValue ? Utilities.GetSignValue(cMAirWayBill.CreditedWeightCharge.Value) : null;
            ValuationChargesBilledOrCredited = cMAirWayBill.CreditedValuationCharge.HasValue ? Math.Abs(cMAirWayBill.CreditedValuationCharge.Value).ToString() : null;
            ValuationChargesBilledOrCreditedSign = cMAirWayBill.CreditedValuationCharge.HasValue ? Utilities.GetSignValue(cMAirWayBill.CreditedValuationCharge.Value) : null;
            OtherChargesBilledOrCredited = Math.Abs(cMAirWayBill.CreditedOtherCharge).ToString();
            OtherChargesBilledOrCreditedSign = Utilities.GetSignValue(cMAirWayBill.CreditedOtherCharge);
            AmountSubjectedToIscBilledOrCredited = Math.Abs(cMAirWayBill.CreditedAmtSubToIsc).ToString();
            AmountSubjectedToIscBilledOrCreditedSign = Utilities.GetSignValue(cMAirWayBill.CreditedAmtSubToIsc);
            IscPercentBilledOrCredited = Math.Abs(cMAirWayBill.CreditedIscPercentage).ToString();
            IscPercentBilledOrCreditedSign = Utilities.GetSignValue(cMAirWayBill.CreditedIscPercentage);
            IscAmountBilledOrCredited = Math.Abs(cMAirWayBill.CreditedIscAmount).ToString();
            IscAmountBilledOrCreditedSign = Utilities.GetSignValue(cMAirWayBill.CreditedIscAmount);
            VatAmountBilledOrCredited = cMAirWayBill.CreditedVatAmount.HasValue ? Math.Abs(cMAirWayBill.CreditedVatAmount.Value).ToString() : null;
            VatAmountBilledOrCreditedSign = Utilities.GetSignValue(cMAirWayBill.CreditedVatAmount);
            TotalAmountBilledOrCredited = Math.Abs(cMAirWayBill.TotalAmountCredited).ToString();
            TotalAmountBilledOrCreditedSign = Utilities.GetSignValue(cMAirWayBill.TotalAmountCredited);
            CurrencyAdjustmentIndicator = cMAirWayBill.CurrencyAdjustmentIndicator;
            BilledWeight = cMAirWayBill.BilledWeight.HasValue ? cMAirWayBill.BilledWeight.Value.ToString() : null;
            ProvisoOrReqOrSpa = cMAirWayBill.ProvisionalReqSpa;
            ProratePercent = cMAirWayBill.ProratePercentage.HasValue ? cMAirWayBill.ProratePercentage.Value.ToString() : null;
            PartShipmentIndicator = cMAirWayBill.PartShipmentIndicator;
            KgOrLbIndicator = cMAirWayBill.KgLbIndicator;
            CCAIndicator = Utilities.GetBooDisplaylValue(cMAirWayBill.CcaIndicator);
            AttachmentIndicatorOriginal = cMAirWayBill.AttachmentIndicatorOriginal;
            AttachmentIndicatorValidated = cMAirWayBill.AttachmentIndicatorValidated;
            NumberOfAttachments = cMAirWayBill.NumberOfAttachments.HasValue ? cMAirWayBill.NumberOfAttachments.Value.ToString() : "0";
            // This field is an output only field and must be blank in case of an input file to SIS.
            ISValidationFlag = string.Empty; // cMAirWayBill.ISValidationFlag;
            ReasonCode = cMAirWayBill.ReasonCode;
            ReferenceField1 = cMAirWayBill.ReferenceField1;
            ReferenceField2 = cMAirWayBill.ReferenceField2;
            ReferenceField3 = cMAirWayBill.ReferenceField3;
            ReferenceField4 = cMAirWayBill.ReferenceField4;
            ReferenceField5 = cMAirWayBill.ReferenceField5;
            AirlineOwnUse = cMAirWayBill.AirlineOwnUse;

            ProcessNextClass(cMAirWayBill, ref recordSequenceNumber);
        }

        /// <summary>
        /// Convert Child models into records
        /// </summary>
        /// <param name="cMAirWayBill">CMAirWayBill</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ProcessNextClass(CMAirWayBill cMAirWayBill, ref long recordSequenceNumber)
        {
            //Logger.Debug("Processing CMAirWayBill model child objects.");

            if (cMAirWayBill != null)
            {
                if (cMAirWayBill.CMAWBProrateLadderList.Count > 0)
                {
                    foreach (var cMAWBProrateLadderList in Utilities.GetDividedSubCollections(cMAirWayBill.CMAWBProrateLadderList.OrderBy(i => i.SequenceNumber).ToList(), 8))
                    {
                        var cMAwbProrateLadderRecord = new CMAwbProrateLadderRecord(this);

                        var cMProrateSlipDetails = new CMProrateSlipDetails { TotalProrateAmount = cMAirWayBill.TotalProrateAmount.HasValue ? cMAirWayBill.TotalProrateAmount.Value : 0, ProrateCalCurrencyId = cMAirWayBill.ProrateCalCurrencyId };
                        cMProrateSlipDetails.CMAWBProrateLadderList.AddRange(cMAWBProrateLadderList);
                        cMAwbProrateLadderRecord.ConvertClassToRecord(cMProrateSlipDetails, ref recordSequenceNumber);

                        CMAwbProrateLadderRecordList.Add(cMAwbProrateLadderRecord);
                    }
                }

                // Add CMAWBVATList in the list
                if (cMAirWayBill.CMAWBVATList.Count > 0)
                {
                    foreach (var cMAWBVATList in Utilities.GetDividedSubCollections(cMAirWayBill.CMAWBVATList, 2))
                    {
                        var cMAWBVatRecord = new CMAWBVatRecord(this);
                        cMAWBVatRecord.ConvertClassToRecord(cMAWBVATList, ref recordSequenceNumber);
                        CMAWBVatRecordList.Add(cMAWBVatRecord);
                    }
                }
                if (cMAirWayBill.CMAWBOtherChargesList.Count > 0)
                {
                    foreach (var cMAWBOtherChargesList in Utilities.GetDividedSubCollections(cMAirWayBill.CMAWBOtherChargesList, 3))
                    {
                        var cMAWBOCBreakdownRecord = new CMAWBOCBreakdownRecord(this);
                        cMAWBOCBreakdownRecord.ConvertClassToRecord(cMAWBOtherChargesList, ref recordSequenceNumber);
                        CMAWBOCBreakdownRecordList.Add(cMAWBOCBreakdownRecord);
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