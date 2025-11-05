using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    public class RMAWBRecord : InvoiceRecordBase, IRecordToClassConverter<RMAirWayBill>, IClassToRecordConverter<RMAirWayBill>
    {
       
        [FieldFixedLength(11)]
        public string RejectionMemoNumber;

        [FieldFixedLength(5), FieldConverter(typeof(PaddingConverter), 5, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string BreakdownSerialNumber;

        [FieldFixedLength(6), FieldConverter(typeof(DateFormatConverter))]
        public string AWBIssueDate;

        [FieldFixedLength(4), FieldConverter(typeof(PaddingConverter), 4, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string AWBIssuingAirline;

        [FieldFixedLength(7), FieldConverter(typeof(PaddingConverter), 7, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string AWBSerialNumber;

        [FieldFixedLength(1), FieldConverter(typeof(PaddingConverter), 1, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string AWBCheckDigit;

        [FieldFixedLength(4)]
        public string Origin;

        [FieldFixedLength(4)]
        public string Destination;

        [FieldFixedLength(4)]
        public string FromAirport;

        [FieldFixedLength(4)]
        public string ToAirport;

        [FieldFixedLength(6), FieldConverter(typeof(DateFormatConverter))]
        public string DateOfCarriage;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string WeightChargesBilled;

        [FieldFixedLength(1)]
        public string WeightChargesBilledSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string WeightChargesAccepted;

        [FieldFixedLength(1)]
        public string WeightChargesAcceptedSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string WeightChargesDifference;

        [FieldFixedLength(1)]
        public string WeightChargesDifferenceSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string ValuationChargesBilled;

        [FieldFixedLength(1)]
        public string ValuationChargesBilledSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string ValuationChargesAccepted;

        [FieldFixedLength(1)]
        public string ValuationChargesAcceptedSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string ValuationChargesDifference;

        [FieldFixedLength(1)]
        public string ValuationChargesDifferenceSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string OtherChargesAmountBilled;

        [FieldFixedLength(1)]
        public string OtherChargesAmountBilledSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string OtherChargesAmountAccepted;

        [FieldFixedLength(1)]
        public string OtherChargesAmountAcceptedSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string OtherChargesDifference;

        [FieldFixedLength(1)]
        public string OtherChargesDifferenceSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string AmountSubjectToISCAllowed;

        [FieldFixedLength(1)]
        public string AmountSubjectToISCAllowedSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string AmountSubjectToISCAccepted;

        [FieldFixedLength(1)]
        public string AmountSubjectToISCAcceptedSign;

        [FieldFixedLength(5), FieldConverter(typeof(DoubleNumberConverter), 5, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string ISCPercentageAllowed;

        [FieldFixedLength(1)]
        public string ISCPercentageAllowedSign;

        [FieldFixedLength(5), FieldConverter(typeof(DoubleNumberConverter), 5, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string ISCPercentageAccepted;

        [FieldFixedLength(1)]
        public string ISCPercentageAcceptedSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string ISCAmountAllowed;

        [FieldFixedLength(1)]
        public string ISCAmountAllowedSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string ISCAmountAccepted;

        [FieldFixedLength(1)]
        public string ISCAmountAcceptedSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string ISCAmountDifference;

        [FieldFixedLength(1)]
        public string ISCAmountDifferenceSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VATAmountBilled;

        [FieldFixedLength(1)]
        public string VATAmountBilledSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VATAmountAccepted;

        [FieldFixedLength(1)]
        public string VATAmountAcceptedSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VATAmountDifference;

        [FieldFixedLength(1)]
        public string VATAmountDifferenceSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string NetRejectAmount;

        [FieldFixedLength(1)]
        public string NetRejectAmountSign;

        [FieldFixedLength(3)]
        public string CurrencyAdjustmentIndicator;

        [FieldFixedLength(6), FieldConverter(typeof(PaddingConverter), 6, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string BilledWeight;

        [FieldFixedLength(1)]
        public string ProvisoOrReqOrSPA;

        [FieldFixedLength(2), FieldConverter(typeof(PaddingConverter), 2, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string ProratePercent;

        [FieldFixedLength(1)]
        public string PartShipmentIndicator;

        [FieldFixedLength(1)]
        public string KgOrLbIndicator;

        [FieldFixedLength(1)]
        public string CCAIndicator;

        [FieldFixedLength(20)]
        public string OurRef;

        [FieldFixedLength(1)]
        public string AttachmentIndicatorOriginal;

        [FieldFixedLength(1)]
        public string AttachmentIndicatorValidated;

        [FieldFixedLength(4), FieldConverter(typeof(PaddingConverter), 4, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string NumberOfAttachments = "0";

        [FieldFixedLength(10)]
        public string ISValidationFlag;

        [FieldFixedLength(2)]
        public string ReasonCode;

        [FieldFixedLength(10)]
        public string ReferenceField1;

        [FieldFixedLength(10)]
        public string ReferenceField2;

        [FieldFixedLength(10)]
        public string ReferenceField3;

        [FieldFixedLength(10)]
        public string ReferenceField4;

        [FieldFixedLength(20)]
        public string ReferenceField5;

        [FieldFixedLength(20)]
        public string AirlineOwnUse;

        [FieldFixedLength(47)]
        [FieldTrim(TrimMode.Right)]
        public string Filler3;

        [FieldHidden]
        public List<RMAWBOCBreakdownRecord> RMAWBOCBreakdownRecordList = new List<RMAWBOCBreakdownRecord>();

        [FieldHidden]
        public List<RMAwbProrateLadderRecord> RMAwbProrateLadderRecordList = new List<RMAwbProrateLadderRecord>();

        [FieldHidden]
        public List<RMAWBVatRecord> RMAWBVatRecordList = new List<RMAWBVatRecord>();

        public RMAWBRecord() { }

        public RMAWBRecord(RejectionMemoRecord rejectionMemoRecord)
            : base(rejectionMemoRecord)
        {
            RejectionMemoNumber = rejectionMemoRecord.RejectionMemoNumber;
        }

        #region Implementation of IModelConverter<RMAirWayBill>

        /// <summary>
        /// Converts it into mode instance.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine instance.</param>
        /// <returns>Returns mode instance created from record.</returns>
        public RMAirWayBill ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Debug("Converting RMAirWayBill record to model instance.");

            var rMAirWayBillRecord = CreateRMAirWayBillObject();

            // Entire RMAirWayBill object will be populated along with its children (VatBreakdown, TaxBreakdown).
            ProcessNextRecord(multiRecordEngine, rMAirWayBillRecord);

            return rMAirWayBillRecord;
        }

        /// <summary>
        /// Converts child records of RMAirWayBill record into model classes and adds them to the current mode record.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine instance</param>
        /// <param name="rMAirWayBill">Parent model record to which all the child records will added.</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, RMAirWayBill rMAirWayBill)
        {
            multiRecordEngine.ReadNext();

            var rMAirWayBillRecord = rMAirWayBill;
            var proRateLadderCount = 0;
            do
            {
                if (multiRecordEngine.LastRecord is VatRecordBase)
                {
                    var rMAWBVATList = ((IRecordToClassConverter<List<RMAWBVAT>>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    rMAirWayBill.NumberOfChildRecords += 1;

                    var noOfRMAWBVATRecords = rMAWBVATList.Count;
                    for (var i = 0; i < noOfRMAWBVATRecords; i++)
                    {
                        rMAirWayBillRecord.RMAWBVATList.Add(rMAWBVATList[i]);
                    }
                    //Logger.Debug("RMAWBVAT List Object is added to RMAirWayBill Record object.");

                }
                else if (multiRecordEngine.LastRecord is RMAWBOCBreakdownRecord)
                {
                    rMAirWayBill.NumberOfChildRecords += 1;

                    var rMAWBOtherChargesList = ((IRecordToClassConverter<List<RMAWBOtherCharges>>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    var noOfRMAWBOtherChargesRecords = rMAWBOtherChargesList.Count;
                    for (var i = 0; i < noOfRMAWBOtherChargesRecords; i++)
                    {
                        rMAirWayBillRecord.RMAWBOtherChargesList.Add(rMAWBOtherChargesList[i]);
                    }
                    //Logger.Debug("RMAWBOtherCharges List Object is added to RMAirWayBill Record object successfully.");
                }
                else if (multiRecordEngine.LastRecord is RMAwbProrateLadderRecord)
                {
                    rMAirWayBill.NumberOfChildRecords += 1;

                    var rMProrateSlipDetailsList = ((IRecordToClassConverter<RMProrateSlipDetails>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    if (rMProrateSlipDetailsList != null)
                    {
                        rMAirWayBillRecord.TotalProrateAmount = rMProrateSlipDetailsList.TotalProrateAmount;
                        rMAirWayBillRecord.ProrateCalCurrencyId = rMProrateSlipDetailsList.ProrateCalCurrencyId;

                        foreach (var rMAWBProrateLadderList in rMProrateSlipDetailsList.RMAWBProrateLadderList)
                        {
                            rMAWBProrateLadderList.SequenceNumber = ++proRateLadderCount;
                            rMAirWayBillRecord.RMAWBProrateLadderList.Add(rMAWBProrateLadderList);
                        }
                    }
                    //Logger.Debug("RMAwbProrateLadder Record List Object is added to RMAirWayBill Record object successfully.");
                }
                else
                {
                    break;
                }
            } while (multiRecordEngine.LastRecord != null);

        }

        #endregion

        /// <summary>
        /// Creates RMAirWayBill object with information from IDEC invoice record.
        /// </summary>
        /// <returns>Returns RMAirWayBill object.</returns>
        private RMAirWayBill CreateRMAirWayBillObject()
        {
            //Logger.Debug("Creating RMAirWayBill record object.");

            var rMAirWayBill = new RMAirWayBill
            {
                BreakdownSerialNumber = Convert.ToInt32(BreakdownSerialNumber),
                BillingCode = BillingCode,
                AWBCheckDigit = Convert.ToInt32(AWBCheckDigit),
                AWBIssuingAirline = AWBIssuingAirline,
                CurrencyAdjustmentIndicator = CurrencyAdjustmentIndicator.Trim(),
                AWBSerialNumber = Convert.ToInt32(AWBSerialNumber),
                From = FromAirport.Trim(),
                To = ToAirport.Trim(),
                Origin = Origin.Trim(),
                Destination = Destination.Trim(),
                BilledWeight = Convert.ToInt32(BilledWeight),
                ProvisionalReqSpa = ProvisoOrReqOrSPA,
                ProratePercentage = Convert.ToInt32(ProratePercent),
                PartShipmentIndicator = PartShipmentIndicator,
                KgLbIndicator = KgOrLbIndicator,
                BilledWeightCharge = Utilities.GetActualValueForDouble(WeightChargesBilledSign, WeightChargesBilled),
                AcceptedWeightCharge = Utilities.GetActualValueForDouble(WeightChargesAcceptedSign, WeightChargesAccepted),
                WeightChargeDiff = Utilities.GetActualValueForDouble(WeightChargesDifferenceSign, WeightChargesDifference),
                BilledValuationCharge = Utilities.GetActualValueForDouble(ValuationChargesBilledSign, ValuationChargesBilled),
                AcceptedValuationCharge = Utilities.GetActualValueForDouble(ValuationChargesAcceptedSign, ValuationChargesAccepted),
                ValuationChargeDiff = Utilities.GetActualValueForDouble(ValuationChargesDifferenceSign, ValuationChargesDifference),
                BilledOtherCharge = Utilities.GetActualValueForDouble(OtherChargesAmountBilledSign, OtherChargesAmountBilled),
                AcceptedOtherCharge = Utilities.GetActualValueForDouble(OtherChargesAmountAcceptedSign, OtherChargesAmountAccepted),
                OtherChargeDiff = Utilities.GetActualValueForDouble(OtherChargesDifferenceSign, OtherChargesDifference),
                AllowedIscAmount = Utilities.GetActualValueForDouble(ISCAmountAllowedSign, ISCAmountAllowed),
                AcceptedIscAmount = Utilities.GetActualValueForDouble(ISCAmountAcceptedSign, ISCAmountAccepted),
                IscAmountDifference = Utilities.GetActualValueForDouble(ISCAmountDifferenceSign, ISCAmountDifference),
                AllowedAmtSubToIsc = Utilities.GetActualValueForDouble(AmountSubjectToISCAllowedSign, AmountSubjectToISCAllowed),
                AcceptedAmtSubToIsc = Utilities.GetActualValueForDouble(AmountSubjectToISCAcceptedSign, AmountSubjectToISCAccepted),
                BilledVatAmount = Utilities.GetActualValueForDouble(VATAmountBilledSign, VATAmountBilled),
                AcceptedVatAmount = Utilities.GetActualValueForDouble(VATAmountAcceptedSign, VATAmountAccepted),
                VatAmountDifference = Utilities.GetActualValueForDouble(VATAmountDifferenceSign, VATAmountDifference),
                NetRejectAmount = Utilities.GetActualValueForDouble(NetRejectAmountSign, NetRejectAmount),
                NumberOfAttachments = Convert.ToInt32(NumberOfAttachments),
                ReferenceField1 = ReferenceField1,
                ReferenceField2 = ReferenceField2,
                ReferenceField3 = ReferenceField3,
                ReferenceField4 = ReferenceField4,
                ReferenceField5 = ReferenceField5,
                AirlineOwnUse = AirlineOwnUse.Trim(),
                // This field is an output only field and must be blank in case of an input file to SIS.
                ISValidationFlag = string.Empty, // ISValidationFlag.Trim(),
                AttachmentIndicatorOriginal = AttachmentIndicatorOriginal,
                OurReference = OurRef,
                CcaIndicator = CCAIndicator == IdecConstants.TrueValue,
                AllowedIscPercentage = Utilities.GetActualValueForDouble(ISCPercentageAllowedSign, ISCPercentageAllowed),
                AcceptedIscPercentage = Utilities.GetActualValueForDouble(ISCPercentageAcceptedSign, ISCPercentageAccepted),
                ReasonCode = ReasonCode
            };

            // Invoice Date
            DateTime airWaybillIssueDate;

            // To avoid converting year 30 into year 1930
            var cultureInfo = new CultureInfo("en-US");
            cultureInfo.Calendar.TwoDigitYearMax = 2099;

            if (DateTime.TryParseExact(AWBIssueDate, IdecConstants.InvoiceDateFormat, cultureInfo, DateTimeStyles.None, out airWaybillIssueDate))
            {
                rMAirWayBill.AWBDate = airWaybillIssueDate;
            }
            else
            {
                rMAirWayBill.AwbDateDisplayText = AWBIssueDate;
            }

            rMAirWayBill.DateOfCarriageOrTransfer = DateOfCarriage;

            //Logger.Debug("RMAirWayBill record object created.");

            return rMAirWayBill;
        }

        /// <summary>
        /// This method converts the RMAirWayBillRecord model into corresponding record instance 
        /// </summary>
        /// <param name="rMAirWayBill"></param>
        /// <param name="recordSequenceNumber"></param>
        public void ConvertClassToRecord(RMAirWayBill rMAirWayBill, ref long recordSequenceNumber)
        {
            //Logger.Debug("Converting RMAirWayBill Record model to RMAirWayBill Record record instance.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiRMAwbRecord;
            BillingCode = rMAirWayBill.BillingCode;
            BreakdownSerialNumber = rMAirWayBill.BreakdownSerialNumber.ToString();
            AWBCheckDigit = rMAirWayBill.AWBCheckDigit.ToString();
            AWBIssueDate = GetFormattedDate(rMAirWayBill.AWBDate);
            Origin = rMAirWayBill.Origin;
            Destination = rMAirWayBill.Destination;
            FromAirport = rMAirWayBill.From;
            ToAirport = rMAirWayBill.To;
            DateOfCarriage = rMAirWayBill.DateOfCarriageOrTransfer;
            CurrencyAdjustmentIndicator = rMAirWayBill.CurrencyAdjustmentIndicator;
            AWBIssuingAirline = rMAirWayBill.AWBIssuingAirline;
            AWBSerialNumber = rMAirWayBill.AWBSerialNumber.ToString();
            WeightChargesBilled = rMAirWayBill.BilledWeightCharge.HasValue ? Math.Abs(rMAirWayBill.BilledWeightCharge.Value).ToString() : null;
            WeightChargesBilledSign = rMAirWayBill.BilledWeightCharge.HasValue ? Utilities.GetSignValue(rMAirWayBill.BilledWeightCharge.Value) : null;
            WeightChargesAccepted = rMAirWayBill.AcceptedWeightCharge.HasValue ? Math.Abs(rMAirWayBill.AcceptedWeightCharge.Value).ToString() : null;
            WeightChargesAcceptedSign = rMAirWayBill.AcceptedWeightCharge.HasValue ? Utilities.GetSignValue(rMAirWayBill.AcceptedWeightCharge.Value) : null;
            WeightChargesDifference = rMAirWayBill.WeightChargeDiff.HasValue ? Math.Abs(rMAirWayBill.WeightChargeDiff.Value).ToString() : null;
            WeightChargesDifferenceSign = rMAirWayBill.WeightChargeDiff.HasValue ? Utilities.GetSignValue(rMAirWayBill.WeightChargeDiff.Value) : null;
            ValuationChargesBilled = rMAirWayBill.BilledValuationCharge.HasValue ? Math.Abs(rMAirWayBill.BilledValuationCharge.Value).ToString() : null;
            ValuationChargesBilledSign = rMAirWayBill.BilledValuationCharge.HasValue ? Utilities.GetSignValue(rMAirWayBill.BilledValuationCharge.Value) : null;
            ValuationChargesAccepted = rMAirWayBill.AcceptedValuationCharge.HasValue ? Math.Abs(rMAirWayBill.AcceptedValuationCharge.Value).ToString() : null;
            ValuationChargesAcceptedSign = rMAirWayBill.AcceptedValuationCharge.HasValue ? Utilities.GetSignValue(rMAirWayBill.AcceptedValuationCharge.Value) : null;
            ValuationChargesDifference = rMAirWayBill.ValuationChargeDiff.HasValue ? Math.Abs(rMAirWayBill.ValuationChargeDiff.Value).ToString() : null;
            ValuationChargesDifferenceSign = rMAirWayBill.ValuationChargeDiff.HasValue ? Utilities.GetSignValue(rMAirWayBill.ValuationChargeDiff.Value) : null;
            OtherChargesAmountBilled = Math.Abs(rMAirWayBill.BilledOtherCharge).ToString();
            OtherChargesAmountBilledSign = Utilities.GetSignValue(rMAirWayBill.BilledOtherCharge);
            OtherChargesAmountAccepted = Math.Abs(rMAirWayBill.AcceptedOtherCharge).ToString();
            OtherChargesAmountAcceptedSign = Utilities.GetSignValue(rMAirWayBill.AcceptedOtherCharge);
            OtherChargesDifference = Math.Abs(rMAirWayBill.OtherChargeDiff).ToString();
            OtherChargesDifferenceSign = Utilities.GetSignValue(rMAirWayBill.OtherChargeDiff);
            AmountSubjectToISCAllowed = Math.Abs(rMAirWayBill.AllowedAmtSubToIsc).ToString();
            AmountSubjectToISCAllowedSign = Utilities.GetSignValue(rMAirWayBill.AllowedAmtSubToIsc);
            AmountSubjectToISCAccepted = Math.Abs(rMAirWayBill.AcceptedAmtSubToIsc).ToString();
            AmountSubjectToISCAcceptedSign = Utilities.GetSignValue(rMAirWayBill.AcceptedAmtSubToIsc);
            ISCAmountAllowed = Math.Abs(rMAirWayBill.AllowedIscAmount).ToString();
            ISCAmountAllowedSign = Utilities.GetSignValue(rMAirWayBill.AllowedIscAmount);
            ISCAmountAccepted = Math.Abs(rMAirWayBill.AcceptedIscAmount).ToString();
            ISCAmountAcceptedSign = Utilities.GetSignValue(rMAirWayBill.AcceptedIscAmount);
            ISCAmountDifference = Math.Abs(rMAirWayBill.IscAmountDifference).ToString();
            ISCAmountDifferenceSign = Utilities.GetSignValue(rMAirWayBill.IscAmountDifference);
            VATAmountBilled = rMAirWayBill.BilledVatAmount.HasValue ? Math.Abs(rMAirWayBill.BilledVatAmount.Value).ToString() : null;
            VATAmountBilledSign = rMAirWayBill.BilledVatAmount.HasValue ? Utilities.GetSignValue(rMAirWayBill.BilledVatAmount.Value) : null;
            VATAmountAccepted = rMAirWayBill.AcceptedVatAmount.HasValue ? Math.Abs(rMAirWayBill.AcceptedVatAmount.Value).ToString() : null;
            VATAmountAcceptedSign = rMAirWayBill.AcceptedVatAmount.HasValue ? Utilities.GetSignValue(rMAirWayBill.AcceptedVatAmount.Value) : null;
            VATAmountDifference = rMAirWayBill.VatAmountDifference.HasValue ? Math.Abs(rMAirWayBill.VatAmountDifference.Value).ToString() : null;
            VATAmountDifferenceSign = rMAirWayBill.VatAmountDifference.HasValue ? Utilities.GetSignValue(rMAirWayBill.VatAmountDifference.Value) : null;
            NetRejectAmount = Math.Abs(rMAirWayBill.NetRejectAmount).ToString();
            NetRejectAmountSign = Utilities.GetSignValue(rMAirWayBill.NetRejectAmount);
            AttachmentIndicatorOriginal = rMAirWayBill.AttachmentIndicatorOriginal;
            AttachmentIndicatorValidated = rMAirWayBill.AttachmentIndicatorValidated;
            NumberOfAttachments = rMAirWayBill.NumberOfAttachments.ToString();
            AirlineOwnUse = rMAirWayBill.AirlineOwnUse;
            // This field is an output only field and must be blank in case of an input file to SIS.
            ISValidationFlag = string.Empty; // rMAirWayBill.ISValidationFlag;
            ReasonCode = rMAirWayBill.ReasonCode;
            ReferenceField1 = rMAirWayBill.ReferenceField1;
            ReferenceField2 = rMAirWayBill.ReferenceField2;
            ReferenceField3 = rMAirWayBill.ReferenceField3;
            ReferenceField4 = rMAirWayBill.ReferenceField4;
            ReferenceField5 = rMAirWayBill.ReferenceField5;
            BilledWeight = rMAirWayBill.BilledWeight.HasValue ? rMAirWayBill.BilledWeight.Value.ToString() : null;
            ProvisoOrReqOrSPA = rMAirWayBill.ProvisionalReqSpa;
            ProratePercent = rMAirWayBill.ProratePercentage.HasValue ? rMAirWayBill.ProratePercentage.Value.ToString() : null;
            PartShipmentIndicator = rMAirWayBill.PartShipmentIndicator;
            KgOrLbIndicator = rMAirWayBill.KgLbIndicator;
            CCAIndicator = Utilities.GetBooDisplaylValue(rMAirWayBill.CcaIndicator);
            OurRef = rMAirWayBill.OurReference;
            ISCPercentageAllowed = Math.Abs(rMAirWayBill.AllowedIscPercentage).ToString();
            ISCPercentageAllowedSign = Utilities.GetSignValue(rMAirWayBill.AllowedIscPercentage);
            ISCPercentageAccepted = Math.Abs(rMAirWayBill.AcceptedIscPercentage).ToString();
            ISCPercentageAcceptedSign = Utilities.GetSignValue(rMAirWayBill.AcceptedIscPercentage);
            ProcessNextClass(rMAirWayBill, ref recordSequenceNumber);
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

        public void ProcessNextClass(RMAirWayBill rMAirWayBill, ref long recordSequenceNumber)
        {
            //Logger.Debug("Processing RMAirWayBill model child objects.");

            if (rMAirWayBill != null)
            {
                if (rMAirWayBill.RMAWBProrateLadderList.Count > 0)
                {
                    foreach (var rMAWBProrateLadderList in Utilities.GetDividedSubCollections(rMAirWayBill.RMAWBProrateLadderList.OrderBy(i => i.SequenceNumber).ToList(), 8))
                    {
                        var rMAwbProrateLadderRecord = new RMAwbProrateLadderRecord(this);

                        var rMProrateSlipDetails = new RMProrateSlipDetails { TotalProrateAmount = rMAirWayBill.TotalProrateAmount.HasValue ? rMAirWayBill.TotalProrateAmount.Value : 0, ProrateCalCurrencyId = rMAirWayBill.ProrateCalCurrencyId };
                        rMProrateSlipDetails.RMAWBProrateLadderList.AddRange(rMAWBProrateLadderList);

                        rMAwbProrateLadderRecord.ConvertClassToRecord(rMProrateSlipDetails, ref recordSequenceNumber);
                        RMAwbProrateLadderRecordList.Add(rMAwbProrateLadderRecord);
                    }
                }

                if (rMAirWayBill.RMAWBVATList.Count > 0)
                {
                    foreach (var rMAWBVATList in Utilities.GetDividedSubCollections(rMAirWayBill.RMAWBVATList, 2))
                    {
                        var rMAWBVatRecord = new RMAWBVatRecord(this);
                        rMAWBVatRecord.ConvertClassToRecord(rMAWBVATList, ref recordSequenceNumber);
                        RMAWBVatRecordList.Add(rMAWBVatRecord);
                    }
                }

                if (rMAirWayBill.RMAWBOtherChargesList.Count > 0)
                {
                    foreach (var rMAWBOtherChargesList in Utilities.GetDividedSubCollections(rMAirWayBill.RMAWBOtherChargesList, 3))
                    {
                        var rMAWBOCBreakdownRecord = new RMAWBOCBreakdownRecord(this);
                        rMAWBOCBreakdownRecord.ConvertClassToRecord(rMAWBOtherChargesList, ref recordSequenceNumber);
                        RMAWBOCBreakdownRecordList.Add(rMAWBOCBreakdownRecord);
                    }
                }
            }
        }
    }
}