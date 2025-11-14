using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.Write;
using QidWorkerRole.SIS.Model;
using QidWorkerRole.SIS.Model.SupportingModels;
using System.Globalization;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    [FixedLengthRecord()]
    public class RejectionMemoRecord : InvoiceRecordBase, IClassToRecordConverter<RejectionMemo>, IRecordToClassConverter<RejectionMemo>
    {
       
        #region Record Properties

        [FieldHidden]
        private int _reasonRemarkSerialNumber;

        [FieldFixedLength(5), FieldConverter(typeof(PaddingConverter), 5, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string BatchSequenceNumber;

        [FieldFixedLength(5), FieldConverter(typeof(PaddingConverter), 5, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string RecordSequenceWithinBatch;

        [FieldFixedLength(11)]
        public string RejectionMemoNumber;

        [FieldFixedLength(1), FieldConverter(typeof(PaddingConverter), 1, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string RejectionStage;

        [FieldFixedLength(2)]
        public string RejectionReasonCode;

        [FieldFixedLength(20)]
        public string AirlineOwnUse;

        [FieldFixedLength(10)]
        public string YourInvoiceNumber;

        [FieldFixedLength(4)]
        public string Filler2;

        [FieldFixedLength(6), FieldConverter(typeof(PaddingConverter), 6, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string YourInvoiceBillingMonth;

        [FieldFixedLength(11)]
        public string YourRejectionMemoNumber;

        [FieldFixedLength(11)]
        public string YourBillingMemoNumber;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalWeightChargesBilled;

        [FieldFixedLength(1)]
        public string TotalWeightChargesBilledSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalWeightChargesAccepted;

        [FieldFixedLength(1)]
        public string TotalWeightChargesAcceptedSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalWeightChargesDifference;

        [FieldFixedLength(1)]
        public string TotalWeightChargesDifferenceSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalValuationChargesBilled;

        [FieldFixedLength(1)]
        public string TotalValuationChargesBilledSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalValuationChargesAccepted;

        [FieldFixedLength(1)]
        public string TotalValuationChargesAcceptedSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalValuationChargesDifference;

        [FieldFixedLength(1)]
        public string TotalValuationChargesDifferenceSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalOtherChargesAmountBilled;

        [FieldFixedLength(1)]
        public string TotalOtherChargesAmountBilledSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalOtherChargesAmountAccepted;

        [FieldFixedLength(1)]
        public string TotalOtherChargesAmountAcceptedSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalOtherChargesDifference;

        [FieldFixedLength(1)]
        public string TotalOtherChargesDifferenceSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalISCAmountAllowed;

        [FieldFixedLength(1)]
        public string TotalISCAmountAllowedSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalISCAmountAccepted;

        [FieldFixedLength(1)]
        public string TotalISCAmountAcceptedSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalISCAmountDifference;

        [FieldFixedLength(1)]
        public string TotalISCAmountDifferenceSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalVATAmountBilled;

        [FieldFixedLength(1)]
        public string TotalVATAmountBilledSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalVATAmountAccepted;

        [FieldFixedLength(1)]
        public string TotalVATAmountAcceptedSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalVATAmountDifference;

        [FieldFixedLength(1)]
        public string TotalVATAmountDifferenceSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalNetRejectAmount;

        [FieldFixedLength(1)]
        public string TotalNetRejectAmountSign;

        [FieldFixedLength(1)]
        public string AttachmentIndicatorOriginal;

        [FieldFixedLength(1)]
        public string AttachmentIndicatorValidated;

        [FieldFixedLength(4), FieldConverter(typeof(PaddingConverter), 4, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string NumberOfAttachments;

        [FieldFixedLength(10)]
        public string ISValidationFlag;

        [FieldFixedLength(1)]
        public string BmCmIndicator;

        [FieldFixedLength(20)]
        public string OurRef;

        [FieldFixedLength(85)]
        public string Filler3;

        [FieldHidden]
        public List<ReasonBreakdownRecord> ReasonBreakdownRecordList = new List<ReasonBreakdownRecord>();

        [FieldHidden]
        public List<RMAWBRecord> RMAWBRecordList = new List<RMAWBRecord>();

        [FieldHidden]
        public List<RMVatRecord> RMVatRecordList = new List<RMVatRecord>();

        #endregion

        #region Constructors

        #region Parameterless Constructor

        public RejectionMemoRecord() { }

        #endregion

        #region Parameterized Constructor

        public RejectionMemoRecord(InvoiceHeaderRecord invoiceHeaderRecord)
            : base(invoiceHeaderRecord)
        { }

        #endregion

        #endregion

        #region Implementation of IClassToRecordConverter<RejectionMemo> To Write IDEC File.

        /// <summary>
        /// This method converts the RejectionMemo calss into RejectionMemoRecord.
        /// </summary>
        /// <param name="rejectionMemo">rejectionMemo</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ConvertClassToRecord(RejectionMemo rejectionMemo, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting RejectionMemo into RejectionMemoRecord.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiRejectionMemoRecord;
            BatchSequenceNumber = rejectionMemo.BatchSequenceNumber.ToString();
            RecordSequenceWithinBatch = rejectionMemo.RecordSequenceWithinBatch.ToString();
            RejectionMemoNumber = string.IsNullOrEmpty(rejectionMemo.RejectionMemoNumber) ? string.Empty : rejectionMemo.RejectionMemoNumber.Trim();
            RejectionStage = rejectionMemo.RejectionStage.ToString();
            RejectionReasonCode = rejectionMemo.ReasonCode;
            BillingCode = rejectionMemo.BillingCode;
            AirlineOwnUse = rejectionMemo.AirlineOwnUse;
            YourInvoiceNumber = string.IsNullOrEmpty(rejectionMemo.YourInvoiceNumber) ? string.Empty : rejectionMemo.YourInvoiceNumber.Trim();
            YourInvoiceBillingMonth = GetFormattedDate(rejectionMemo.YourInvoiceBillingMonth, rejectionMemo.YourInvoiceBillingYear, rejectionMemo.YourInvoiceBillingPeriod);
            YourRejectionMemoNumber = string.IsNullOrEmpty(rejectionMemo.YourRejectionNumber) ? string.Empty : rejectionMemo.YourRejectionNumber.Trim();
            YourBillingMemoNumber = rejectionMemo.YourBillingMemoNumber;
            TotalWeightChargesBilled = rejectionMemo.BilledTotalWeightCharge.HasValue ? Math.Abs(rejectionMemo.BilledTotalWeightCharge.Value).ToString() : null;
            TotalWeightChargesBilledSign = rejectionMemo.BilledTotalWeightCharge.HasValue ? Utilities.GetSignValue(rejectionMemo.BilledTotalWeightCharge.Value) : null;
            TotalWeightChargesAccepted = rejectionMemo.AcceptedTotalWeightCharge.HasValue ? Math.Abs(rejectionMemo.AcceptedTotalWeightCharge.Value).ToString() : null;
            TotalWeightChargesAcceptedSign = rejectionMemo.AcceptedTotalWeightCharge.HasValue ? Utilities.GetSignValue(rejectionMemo.AcceptedTotalWeightCharge.Value) : null;
            TotalWeightChargesDifference = rejectionMemo.TotalWeightChargeDifference.HasValue ? Math.Abs(rejectionMemo.TotalWeightChargeDifference.Value).ToString() : null;
            TotalWeightChargesDifferenceSign = rejectionMemo.TotalWeightChargeDifference.HasValue ? Utilities.GetSignValue(rejectionMemo.TotalWeightChargeDifference.Value) : null;
            TotalValuationChargesBilled = rejectionMemo.BilledTotalValuationCharge.HasValue ? Math.Abs(rejectionMemo.BilledTotalValuationCharge.Value).ToString() : null;
            TotalValuationChargesBilledSign = rejectionMemo.BilledTotalValuationCharge.HasValue ? Utilities.GetSignValue(rejectionMemo.BilledTotalValuationCharge.Value) : null;
            TotalValuationChargesAccepted = rejectionMemo.AcceptedTotalValuationCharge.HasValue ? Math.Abs(rejectionMemo.AcceptedTotalValuationCharge.Value).ToString() : null;
            TotalValuationChargesAcceptedSign = rejectionMemo.AcceptedTotalValuationCharge.HasValue ? Utilities.GetSignValue(rejectionMemo.AcceptedTotalValuationCharge.Value) : null;
            TotalValuationChargesDifference = rejectionMemo.TotalValuationChargeDifference.HasValue ? Math.Abs(rejectionMemo.TotalValuationChargeDifference.Value).ToString() : null;
            TotalValuationChargesDifferenceSign = rejectionMemo.TotalValuationChargeDifference.HasValue ? Utilities.GetSignValue(rejectionMemo.TotalValuationChargeDifference.Value) : null;
            TotalOtherChargesAmountBilled = rejectionMemo.BilledTotalOtherChargeAmount.HasValue ? Math.Abs(rejectionMemo.BilledTotalOtherChargeAmount.Value).ToString() : null;
            TotalOtherChargesAmountBilledSign = rejectionMemo.BilledTotalOtherChargeAmount.HasValue ? Utilities.GetSignValue(rejectionMemo.BilledTotalOtherChargeAmount.Value) : null;
            TotalOtherChargesAmountAccepted = rejectionMemo.AcceptedTotalOtherChargeAmount.HasValue ? Math.Abs(rejectionMemo.AcceptedTotalOtherChargeAmount.Value).ToString() : null;
            TotalOtherChargesAmountAcceptedSign = rejectionMemo.AcceptedTotalOtherChargeAmount.HasValue ? Utilities.GetSignValue(rejectionMemo.AcceptedTotalOtherChargeAmount.Value) : null;
            TotalOtherChargesDifference = rejectionMemo.TotalOtherChargeDifference.HasValue ? Math.Abs(rejectionMemo.TotalOtherChargeDifference.Value).ToString() : null;
            TotalOtherChargesDifferenceSign = rejectionMemo.TotalOtherChargeDifference.HasValue ? Utilities.GetSignValue(rejectionMemo.TotalOtherChargeDifference.Value) : null;
            TotalISCAmountAllowed = rejectionMemo.AllowedTotalIscAmount.HasValue ? Math.Abs(rejectionMemo.AllowedTotalIscAmount.Value).ToString() : null;
            TotalISCAmountAllowedSign = rejectionMemo.AllowedTotalIscAmount.HasValue ? Utilities.GetSignValue(rejectionMemo.AllowedTotalIscAmount.Value) : null;
            TotalISCAmountAccepted = rejectionMemo.AcceptedTotalIscAmount.HasValue ? Math.Abs(rejectionMemo.AcceptedTotalIscAmount.Value).ToString() : null;
            TotalISCAmountAcceptedSign = rejectionMemo.AcceptedTotalIscAmount.HasValue ? Utilities.GetSignValue(rejectionMemo.AcceptedTotalIscAmount.Value) : null;
            TotalISCAmountDifference = rejectionMemo.TotalIscAmountDifference.HasValue ? Math.Abs(rejectionMemo.TotalIscAmountDifference.Value).ToString() : null;
            TotalISCAmountDifferenceSign = rejectionMemo.TotalIscAmountDifference.HasValue ? Utilities.GetSignValue(rejectionMemo.TotalIscAmountDifference.Value) : null;
            TotalVATAmountBilled = rejectionMemo.BilledTotalVatAmount.HasValue ? Math.Abs(rejectionMemo.BilledTotalVatAmount.Value).ToString() : null;
            TotalVATAmountBilledSign = rejectionMemo.BilledTotalVatAmount.HasValue ? Utilities.GetSignValue(rejectionMemo.BilledTotalVatAmount.Value) : null;
            TotalVATAmountAccepted = rejectionMemo.AcceptedTotalVatAmount.HasValue ? Math.Abs(rejectionMemo.AcceptedTotalVatAmount.Value).ToString() : null;
            TotalVATAmountAcceptedSign = rejectionMemo.AcceptedTotalVatAmount.HasValue ? Utilities.GetSignValue(rejectionMemo.AcceptedTotalVatAmount.Value) : null;
            TotalVATAmountDifference = rejectionMemo.TotalVatAmountDifference.HasValue ? Math.Abs(rejectionMemo.TotalVatAmountDifference.Value).ToString() : null;
            TotalVATAmountDifferenceSign = rejectionMemo.TotalVatAmountDifference.HasValue ? Utilities.GetSignValue(rejectionMemo.TotalVatAmountDifference.Value) : null;
            TotalNetRejectAmount = rejectionMemo.TotalNetRejectAmount.HasValue ? Math.Abs(rejectionMemo.TotalNetRejectAmount.Value).ToString() : null;
            TotalNetRejectAmountSign = rejectionMemo.TotalNetRejectAmount.HasValue ? Utilities.GetSignValue(rejectionMemo.TotalNetRejectAmount.Value) : null;
            AttachmentIndicatorOriginal = Utilities.GetBooDisplaylValue(rejectionMemo.AttachmentIndicatorOriginal);
            AttachmentIndicatorValidated = rejectionMemo.AttachmentIndicatorValidated.HasValue ? Utilities.GetBooDisplaylValue(rejectionMemo.AttachmentIndicatorValidated.Value) : null; // TODO: Update this field(pax_rejection_memo.attchmnt_ind_validated) when validated in Db.
            NumberOfAttachments = rejectionMemo.NumberOfAttachments.ToString();
            ISValidationFlag = rejectionMemo.ISValidationFlag;
            BmCmIndicator = rejectionMemo.BMCMIndicator;
            OurRef = rejectionMemo.OurRef;
            ProcessNextClass(rejectionMemo, ref recordSequenceNumber);

            //Logger.Info("End of Converting RejectionMemo into RejectionMemoRecord.");
        }

        /// <summary>
        /// Gets date in the format 'YYMMPP'
        /// </summary>
        /// <param name="year">year</param>
        /// <param name="month">month</param>
        /// <param name="periodNo">periodNo</param>
        /// <returns>'YYMMPP'</returns>
        private static string GetFormattedDate(int year, int month, int periodNo)
        {
            string yearStr = year.ToString();
            string monthStr = month.ToString();
            string periodStr = periodNo.ToString();

            // This is case when year is passed as 4 digit number as 2009.
            if (yearStr.Length == 4)
            {
                yearStr = yearStr.Substring(2, 2);
            }

            return string.Format("{0}{1}{2}", yearStr.PadLeft(2, '0'), monthStr.PadLeft(2, '0'), periodStr.PadLeft(2, '0'));
        }

        /// <summary>
        /// Convert Child of RejectionMemo Class into there corresponding Records.
        /// </summary>
        /// <param name="rejectionMemo">rejectionMemo</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ProcessNextClass(RejectionMemo rejectionMemo, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting Childs of RejectionMemo Class into there corresponding Records.");

            if (rejectionMemo != null)
            {
                // Reason breakdown
                string rMReasonRemarks = rejectionMemo.ReasonRemarks != null ? rejectionMemo.ReasonRemarks.Replace("\r", "").Replace("\n", "").Trim() : string.Empty;
                var remarksLength = rMReasonRemarks.Trim().Length;
                int serialNumber = 0;
                while (remarksLength > 0)
                {
                    var reasonBreakdownRecord = new ReasonBreakdownRecord(this);
                    var reasonBreakdown = new ReasonBreakdown { ReasonRemarks = rMReasonRemarks.Substring(serialNumber * 400, remarksLength), RemarkSerialNumber = ++serialNumber };
                    reasonBreakdownRecord.ConvertClassToRecord(reasonBreakdown, ref recordSequenceNumber);
                    ReasonBreakdownRecordList.Add(reasonBreakdownRecord);
                    remarksLength = (remarksLength >= 400) ? remarksLength - 400 : 0;
                }

                if (rejectionMemo.RMVATList.Count > 0)
                {
                    // Add RMVatRecord in the list
                    foreach (var rMVATList in Utilities.GetDividedSubCollections(rejectionMemo.RMVATList, 2))
                    {
                        var rMVatRecord = new RMVatRecord(this);
                        rMVatRecord.ConvertClassToRecord(rMVATList, ref recordSequenceNumber);
                        RMVatRecordList.Add(rMVatRecord);
                    }
                }

                if (rejectionMemo.RMAirWayBillList.Count > 0)
                {
                    // Add RMCouponRecord in the list
                    foreach (var rMAirWayBillList in rejectionMemo.RMAirWayBillList.OrderBy(c => c.BreakdownSerialNumber))
                    {
                        var rMAWBRecord = new RMAWBRecord(this);
                        rMAWBRecord.ConvertClassToRecord(rMAirWayBillList, ref recordSequenceNumber);
                        RMAWBRecordList.Add(rMAWBRecord);
                    }
                }
            }

            //Logger.Info("End of Converting Childs of RejectionMemo Class into there corresponding Records.");
        }

        #endregion

        #region Implementation of IRecordToClassConverter<RejectionMemo> To Write IDEC File.

        /// <summary>
        /// To Convert RejectionMemoRecord into AirWayBill.
        /// </summary>
        /// <param name="multiRecordEngine">multiRecordEngine</param>
        /// <returns>RejectionMemo</returns>
        public RejectionMemo ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Info("Start of Converting RejectionMemoRecord into RejectionMemo.");

            var rejectionMemoRecord = CreateRejectionMemo();

            ProcessNextRecord(multiRecordEngine, rejectionMemoRecord);

            //Logger.Info("End of Converting RejectionMemoRecord into RejectionMemo.");

            return rejectionMemoRecord;
        }

        /// <summary>
        /// Creates RejectionMemo for RejectionMemoRecord present in the reading input IDEC File.
        /// </summary>
        /// <returns>RejectionMemo</returns>
        private RejectionMemo CreateRejectionMemo()
        {
            //Logger.Info("Start of Creating RejectionMemo Class from RejectionMemoRecord.");

            var yourInvoiceBillingYear = 0;
            var yourInvoiceBillingMonth = 0;
            var yourInvoiceBillingPeriod = 0;

            var rejectionMemo = new RejectionMemo
            {
                BatchSequenceNumber = Convert.ToInt32(BatchSequenceNumber.Trim()),
                RecordSequenceWithinBatch = Convert.ToInt32(RecordSequenceWithinBatch.Trim()),
                RejectionMemoNumber = RejectionMemoNumber.Trim(),
                RejectionStage = Convert.ToInt32(RejectionStage.Trim()),
                ReasonCode = RejectionReasonCode.Trim(),
                BillingCode = BillingCode,
                YourInvoiceNumber = YourInvoiceNumber.Trim(),
                YourRejectionNumber = YourRejectionMemoNumber.Trim(),
                BilledTotalWeightCharge = Utilities.GetActualValueForDecimal(TotalWeightChargesBilledSign, TotalWeightChargesBilled),
                AcceptedTotalWeightCharge = Utilities.GetActualValueForDecimal(TotalWeightChargesAcceptedSign, TotalWeightChargesAccepted),
                TotalWeightChargeDifference = Utilities.GetActualValueForDecimal(TotalWeightChargesDifferenceSign, TotalWeightChargesDifference),
                BilledTotalValuationCharge = Utilities.GetActualValueForDecimal(TotalValuationChargesBilledSign, TotalValuationChargesBilled),
                AcceptedTotalValuationCharge = Utilities.GetActualValueForDecimal(TotalValuationChargesAcceptedSign, TotalValuationChargesAccepted),
                TotalValuationChargeDifference = Utilities.GetActualValueForDecimal(TotalValuationChargesDifferenceSign, TotalValuationChargesDifference),
                BilledTotalOtherChargeAmount = Utilities.GetActualValueForDecimal(TotalOtherChargesAmountBilledSign, TotalOtherChargesAmountBilled),
                AcceptedTotalOtherChargeAmount = Utilities.GetActualValueForDecimal(TotalOtherChargesAmountAcceptedSign, TotalOtherChargesAmountAccepted),
                TotalOtherChargeDifference = Utilities.GetActualValueForDecimal(TotalOtherChargesDifferenceSign, TotalOtherChargesDifference),
                AllowedTotalIscAmount = Utilities.GetActualValueForDecimal(TotalISCAmountAllowedSign, TotalISCAmountAllowed),
                AcceptedTotalIscAmount = Utilities.GetActualValueForDecimal(TotalISCAmountAcceptedSign, TotalISCAmountAccepted),
                TotalIscAmountDifference = Utilities.GetActualValueForDecimal(TotalISCAmountDifferenceSign, TotalISCAmountDifference),
                BilledTotalVatAmount = Utilities.GetActualValueForDecimal(TotalVATAmountBilledSign, TotalVATAmountBilled),
                AcceptedTotalVatAmount = Utilities.GetActualValueForDecimal(TotalVATAmountAcceptedSign, TotalVATAmountAccepted),
                TotalVatAmountDifference = Utilities.GetActualValueForDouble(TotalVATAmountDifferenceSign, TotalVATAmountDifference),
                TotalNetRejectAmount = Utilities.GetActualValueForDecimal(TotalNetRejectAmountSign, TotalNetRejectAmount),
                NumberOfAttachments = Convert.ToInt32(NumberOfAttachments),
                AirlineOwnUse = AirlineOwnUse.Trim(),
                ISValidationFlag = ISValidationFlag.Trim(),
                AttachmentIndicatorOriginal = AttachmentIndicatorOriginal == IdecConstants.TrueValue,
                BMCMIndicator = BmCmIndicator,
                YourBillingMemoNumber = YourBillingMemoNumber.Trim(),
                OurRef = OurRef
            };

            DateTime yourInvoiceBillingDate;

            // To avoid converting year 30 into year 1930
            var cultureInfo = new CultureInfo("en-US");
            cultureInfo.Calendar.TwoDigitYearMax = 2099;

            if (DateTime.TryParseExact(YourInvoiceBillingMonth.Trim(), IdecConstants.InvoiceDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out yourInvoiceBillingDate))
            {
                yourInvoiceBillingYear = yourInvoiceBillingDate.Year;
                yourInvoiceBillingMonth = yourInvoiceBillingDate.Month;
                yourInvoiceBillingPeriod = yourInvoiceBillingDate.Day;
            }
            else
            {
                if (YourInvoiceBillingMonth.Trim().Length == 6)
                {
                    var year = YourInvoiceBillingMonth.Trim().Substring(0, 2);
                    var month = YourInvoiceBillingMonth.Trim().Substring(2, 2);
                    var period = YourInvoiceBillingMonth.Trim().Substring(4, 2);
                    int billingYear;
                    int billingMonth;
                    int billingPeriod;
                    if (int.TryParse(year, out billingYear) && int.TryParse(month, out billingMonth) && int.TryParse(period, out billingPeriod))
                    {
                        yourInvoiceBillingYear = billingYear;
                        yourInvoiceBillingMonth = billingMonth;
                        yourInvoiceBillingPeriod = billingPeriod;
                    }
                }
            }

            rejectionMemo.YourInvoiceBillingYear = yourInvoiceBillingYear;
            rejectionMemo.YourInvoiceBillingMonth = yourInvoiceBillingMonth;
            rejectionMemo.YourInvoiceBillingPeriod = yourInvoiceBillingPeriod;

            //Logger.Info("End of Creating RejectionMemo Class from RejectionMemoRecord.");

            return rejectionMemo;
        }

        /// <summary>
        /// To Convert Child records of RejectionMemoRecord into there corresponding Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="rejectionMemo">RejectionMemo</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, RejectionMemo rejectionMemo)
        {
            //Logger.Info("Start of Converting Childs of RejectionMemoRecord into there corresponding Classes.");

            var rejectionMemoRecord = rejectionMemo;

            multiRecordEngine.ReadNext();
            _reasonRemarkSerialNumber = 0;

            do
            {
                if (multiRecordEngine.LastRecord is VatRecordBase)
                {
                    rejectionMemo.NumberOfChildRecords += 1;

                    var rMVATList = ((IRecordToClassConverter<List<RMVAT>>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    int noOfVatRecords = rMVATList.Count;
                    for (int i = 0; i < noOfVatRecords; i++)
                    {
                        rejectionMemoRecord.RMVATList.Add(rMVATList[i]);
                    }
                    //Logger.Debug("Vat List Object is added to Rejection Memo Record object successfully.");
                }

                else if (multiRecordEngine.LastRecord is RMAWBRecord)
                {
                    rejectionMemo.NumberOfChildRecords += 1;

                    var rMAirWayBillList = (multiRecordEngine.LastRecord as IRecordToClassConverter<RMAirWayBill>).ConvertRecordToClass(multiRecordEngine);
                    rejectionMemoRecord.RMAirWayBillList.Add(rMAirWayBillList);

                    //Logger.Debug("Rejection Memo AWB Breakdown Object is added to Rejection Memo Record object successfully.");
                }
                else if (multiRecordEngine.LastRecord is ReasonBreakdownRecord)
                {
                    rejectionMemo.NumberOfChildRecords += 1;

                    var rMReasonBreakdown = (multiRecordEngine.LastRecord as IRecordToClassConverter<ReasonBreakdown>).ConvertRecordToClass(multiRecordEngine);

                    //Reason breakdown should not be all blanks.
                    if (!string.IsNullOrEmpty(rMReasonBreakdown.ReasonRemarks.Trim()))
                    {
                        if (rMReasonBreakdown.RemarkSerialNumber == _reasonRemarkSerialNumber + 1)
                        {
                            _reasonRemarkSerialNumber += 1;
                        }

                        rejectionMemoRecord.ReasonRemarks = rejectionMemoRecord.ReasonRemarks != null ? string.Format("{0}{1}", rejectionMemoRecord.ReasonRemarks, rMReasonBreakdown.ReasonRemarks) : rMReasonBreakdown.ReasonRemarks;
                    }
                    //Logger.Debug("Rejection Memo Reason Breakdown Object is added to Rejection Memo Record object successfully.");
                }
                else
                {
                    break;
                }
            } while (multiRecordEngine.LastRecord != null);

            //Logger.Info("End of Converting Childs of RejectionMemoRecord into there corresponding Classes.");
        }

        #endregion

    }
}