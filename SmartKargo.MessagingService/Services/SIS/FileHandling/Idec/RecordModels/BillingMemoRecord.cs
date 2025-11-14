using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.Write;
using QidWorkerRole.SIS.Model;
using QidWorkerRole.SIS.Model.SupportingModels;
using System.Globalization;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    [FixedLengthRecord()]
    public class BillingMemoRecord : MemoRecordBase, IClassToRecordConverter<BillingMemo>, IRecordToClassConverter<BillingMemo>
    {
        #region Record Properties

        [FieldHidden]
        private int _reasonRemarkSerialNumber;

        [FieldHidden]
        public List<ReasonBreakdownRecord> ReasonBreakdownRecordList = new List<ReasonBreakdownRecord>();

        [FieldHidden]
        public List<BMAWBRecord> BMAWBRecordList = new List<BMAWBRecord>();

        [FieldHidden]
        public List<BMVatRecord> BMVatRecordList = new List<BMVatRecord>();

        #endregion

        #region Constructors

        #region Parameterless Constructor

        public BillingMemoRecord() { }

        #endregion

        #region Parameterized Constructor

        public BillingMemoRecord(InvoiceRecordBase invoiceRecordBase)
            : base(invoiceRecordBase)
        { }

        #endregion

        #endregion

        #region Implementation of IClassToRecordConverter<BillingMemo> To Write IDEC File.

        /// <summary>
        /// This method converts the BillingMemo calss into BillingMemoRecord.
        /// </summary>
        /// <param name="billingMemo">billingMemo</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ConvertClassToRecord(BillingMemo billingMemo, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting BillingMemo into BillingMemoRecord.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiBillingMemoRecord;
            BatchSequenceNumber = billingMemo.BatchSequenceNumber.ToString();
            RecordSequenceWithinBatch = billingMemo.RecordSequenceWithinBatch.ToString();
            BillingOrCreditMemoNumber = billingMemo.BillingMemoNumber;
            ReasonCode = billingMemo.ReasonCode;
            OurRef = billingMemo.OurRef;
            CorrespondenceRefNumber = billingMemo.CorrespondenceReferenceNumber;
            BillingCode = billingMemo.BillingCode;
            YourInvoiceNumber = string.IsNullOrEmpty(billingMemo.YourInvoiceNumber) ? string.Empty : billingMemo.YourInvoiceNumber.Trim();

            if (!string.IsNullOrEmpty(billingMemo.YourInvoiceNumber))
            {
                YourInvoiceBillingDate = Utilities.GetFormattedPeriod(billingMemo.YourInvoiceBillingMonth, billingMemo.YourInvoiceBillingYear, billingMemo.YourInvoiceBillingPeriod);
            }

            TotalWeightChargesAmountBilledOrCredited = billingMemo.BilledTotalWeightCharge.HasValue ? Math.Abs(billingMemo.BilledTotalWeightCharge.Value).ToString() : null;
            TotalWeightChargesAmountBilledOrCreditedSign = billingMemo.BilledTotalWeightCharge.HasValue ? Utilities.GetSignValue(billingMemo.BilledTotalWeightCharge.Value) : null;
            TotalValuationAmountBilledOrCredited = billingMemo.BilledTotalValuationAmount.HasValue ? Math.Abs(billingMemo.BilledTotalValuationAmount.Value).ToString() : null;
            TotalValuationAmountBilledOrCreditedSign = billingMemo.BilledTotalValuationAmount.HasValue ? Utilities.GetSignValue(billingMemo.BilledTotalValuationAmount.Value) : null;
            TotalOtherChargesAmountBilledOrCredited = Math.Abs(billingMemo.BilledTotalOtherChargeAmount).ToString();
            TotalOtherChargesAmountBilledOrCreditedSign = Utilities.GetSignValue(billingMemo.BilledTotalOtherChargeAmount);
            TotalISCAmountBilledOrCredited = Math.Abs(billingMemo.BilledTotalIscAmount).ToString();
            TotalISCAmountBilledOrCreditedSign = Utilities.GetSignValue(billingMemo.BilledTotalIscAmount);
            TotalVATAmountBilledOrCredited = billingMemo.BilledTotalVatAmount.HasValue ? Math.Abs(billingMemo.BilledTotalVatAmount.Value).ToString() : null;
            TotalVATAmountBilledOrCreditedSign = billingMemo.BilledTotalVatAmount.HasValue ? Utilities.GetSignValue(billingMemo.BilledTotalVatAmount.Value) : null;
            NetAmountBilledOrCredited = billingMemo.NetBilledAmount.HasValue ? Math.Abs(billingMemo.NetBilledAmount.Value).ToString() : null;
            NetAmountBilledOrCreditedSign = billingMemo.NetBilledAmount.HasValue ? Utilities.GetSignValue(billingMemo.NetBilledAmount.Value) : null;
            AttachmentIndicatorOriginal = Utilities.GetBooDisplaylValue(billingMemo.AttachmentIndicatorOriginal);
            AttachmentIndicatorValidated = billingMemo.AttachmentIndicatorValidated.HasValue ? Utilities.GetBooDisplaylValue(billingMemo.AttachmentIndicatorValidated.Value) : null;
            NumberOfAttachments = billingMemo.NumberOfAttachments.HasValue ? billingMemo.NumberOfAttachments.Value.ToString() : "0";
            AirlineOwnUse = billingMemo.AirlineOwnUse;
            ISValidationFlag = billingMemo.ISValidationFlag;

            ProcessNextClass(billingMemo, ref recordSequenceNumber);

            //Logger.Info("End of Converting BillingMemo into BillingMemoRecord.");
        }

        /// <summary>
        /// Convert Child of BillingMemo Class into there corresponding Records.
        /// </summary>
        /// <param name="billingMemo">billingMemo</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ProcessNextClass(BillingMemo billingMemo, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting Childs of BillingMemo Class into there corresponding Records.");

            if (billingMemo != null)
            {
                // Reason breakdown
                string bMReasonRemarks = billingMemo.ReasonRemarks != null ? billingMemo.ReasonRemarks.Replace("\r", "").Replace("\n", "").Trim() : string.Empty;
                int remarksLength = bMReasonRemarks.Trim().Length;
                int serialNumber = 0;
                while (remarksLength > 0)
                {
                    var reasonBreakdownRecord = new ReasonBreakdownRecord(this);
                    var reasonBreakdown = new ReasonBreakdown { ReasonRemarks = bMReasonRemarks.Substring(serialNumber * 400, remarksLength), RemarkSerialNumber = ++serialNumber };
                    reasonBreakdownRecord.ConvertClassToRecord(reasonBreakdown, ref recordSequenceNumber);
                    ReasonBreakdownRecordList.Add(reasonBreakdownRecord);
                    remarksLength = (remarksLength >= 400) ? remarksLength - 400 : 0;
                }

                if (billingMemo.BMVATList.Count > 0)
                {
                    // Add BMCVatRecord in the list
                    foreach (var bMVATList in Utilities.GetDividedSubCollections(billingMemo.BMVATList, 2))
                    {
                        var bMVatRecord = new BMVatRecord(this);
                        bMVatRecord.ConvertClassToRecord(bMVATList, ref recordSequenceNumber);
                        BMVatRecordList.Add(bMVatRecord);
                    }
                }

                if (billingMemo.BMAirWayBillList.Count > 0)
                {
                    // Add BMAwbRecord in the list
                    foreach (var bMAirWayBillList in billingMemo.BMAirWayBillList.OrderBy(c => c.BreakdownSerialNumber))
                    {
                        var bMAWBRecord = new BMAWBRecord(this);
                        bMAWBRecord.ConvertClassToRecord(bMAirWayBillList, ref recordSequenceNumber);
                        BMAWBRecordList.Add(bMAWBRecord);
                    }
                }
            }

            //Logger.Info("End of Converting Childs of BillingMemo Class into there corresponding Records.");
        }

        #endregion

        #region Implementation of IRecordToClassConverter<BillingMemo> To Write IDEC File.

        /// <summary>
        /// To Convert BillingMemoRecord into AirWayBill.
        /// </summary>
        /// <param name="multiRecordEngine">multiRecordEngine</param>
        /// <returns>BillingMemo</returns>
        public BillingMemo ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Info("Start of Converting BillingMemoRecord into BillingMemo.");

            var billingMemoRecord = CreateBillingMemo();

            ProcessNextRecord(multiRecordEngine, billingMemoRecord);

            //Logger.Info("End of Converting BillingMemoRecord into BillingMemo.");

            return billingMemoRecord;
        }

        /// <summary>
        /// Creates BillingMemo for BillingMemoRecord present in the reading input IDEC File.
        /// </summary>
        /// <returns>BillingMemo</returns>
        private BillingMemo CreateBillingMemo()
        {
            //Logger.Info("Start of Creating BillingMemo Class from BillingMemoRecord.");

            var yourInvoiceBillingYear = 0;
            var yourInvoiceBillingMonth = 0;
            var yourInvoiceBillingPeriod = 0;

            var billingMemo = new BillingMemo
            {
                BatchSequenceNumber = Convert.ToInt32(BatchSequenceNumber.Trim()),
                RecordSequenceWithinBatch = Convert.ToInt32(RecordSequenceWithinBatch.Trim()),
                BillingMemoNumber = BillingOrCreditMemoNumber.Trim(),
                BillingCode = BillingCode,
                ReasonCode = ReasonCode.Trim(),
                OurRef = OurRef.Trim(),
                CorrespondenceReferenceNumber = CorrespondenceRefNumber.Trim(),
                YourInvoiceNumber = YourInvoiceNumber.Trim(),
                BilledTotalWeightCharge = Utilities.GetActualValueForDecimal(TotalWeightChargesAmountBilledOrCreditedSign, TotalWeightChargesAmountBilledOrCredited),
                BilledTotalValuationAmount = Utilities.GetActualValueForDecimal(TotalValuationAmountBilledOrCreditedSign, TotalValuationAmountBilledOrCredited),
                BilledTotalOtherChargeAmount = Utilities.GetActualValueForDecimal(TotalOtherChargesAmountBilledOrCreditedSign, TotalOtherChargesAmountBilledOrCredited),
                BilledTotalIscAmount = Utilities.GetActualValueForDecimal(TotalISCAmountBilledOrCreditedSign, TotalISCAmountBilledOrCredited),
                BilledTotalVatAmount = Utilities.GetActualValueForDecimal(TotalVATAmountBilledOrCreditedSign, TotalVATAmountBilledOrCredited),
                NetBilledAmount = Utilities.GetActualValueForDecimal(NetAmountBilledOrCreditedSign, NetAmountBilledOrCredited),
                AttachmentIndicatorOriginal = AttachmentIndicatorOriginal == IdecConstants.TrueValue,
                NumberOfAttachments = Convert.ToInt32(NumberOfAttachments),
                AirlineOwnUse = AirlineOwnUse.Trim()
            };

            if (!String.IsNullOrEmpty(YourInvoiceBillingDate) && (Convert.ToInt32(YourInvoiceBillingDate) != 0))
            {
                // Your Invoice Billing Date
                DateTime yourInvoiceBillingDate;

                //To avoid converting year 30 into year 1930
                var cultureInfo = new CultureInfo("en-US");
                cultureInfo.Calendar.TwoDigitYearMax = 2099;

                if (DateTime.TryParseExact(YourInvoiceBillingDate, IdecConstants.InvoiceDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out yourInvoiceBillingDate))
                {
                    yourInvoiceBillingYear = yourInvoiceBillingDate.Year;
                    yourInvoiceBillingMonth = yourInvoiceBillingDate.Month;
                    yourInvoiceBillingPeriod = yourInvoiceBillingDate.Day;
                }
                else
                {
                    yourInvoiceBillingYear = Convert.ToInt32(YourInvoiceBillingDate.Substring(0, 2));
                    yourInvoiceBillingMonth = Convert.ToInt32(YourInvoiceBillingDate.Substring(2, 2));
                    yourInvoiceBillingPeriod = Convert.ToInt32(YourInvoiceBillingDate.Substring(4, 2));
                }
            }

            billingMemo.YourInvoiceBillingYear = yourInvoiceBillingYear;
            billingMemo.YourInvoiceBillingMonth = yourInvoiceBillingMonth;
            billingMemo.YourInvoiceBillingPeriod = yourInvoiceBillingPeriod;

            //Logger.Info("End of Creating BillingMemo Class from BillingMemoRecord.");

            return billingMemo;
        }

        /// <summary>
        /// To Convert Child records of BillingMemoRecord into there corresponding Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="billingMemo">BillingMemo</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, BillingMemo billingMemo)
        {
            //Logger.Info("Start of Converting Childs of BillingMemoRecord into there corresponding Classes.");

            var billigMemoRecord = billingMemo;

            multiRecordEngine.ReadNext();

            _reasonRemarkSerialNumber = 0;

            do
            {
                if (multiRecordEngine.LastRecord is VatRecordBase)
                {
                    billingMemo.NumberOfChildRecords += 1;

                    var bMVAT = ((IRecordToClassConverter<List<BMVAT>>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    var noOfVatRecords = bMVAT.Count;
                    for (var i = 0; i < noOfVatRecords; i++)
                    {
                        billigMemoRecord.BMVATList.Add(bMVAT[i]);
                    }
                    //Logger.Debug("Vat List Object is added to a Memo Record object successfully.");
                }

                else if (multiRecordEngine.LastRecord is BMAWBRecord)
                {
                    billingMemo.NumberOfChildRecords += 1;

                    var bMAirWayBill = (multiRecordEngine.LastRecord as IRecordToClassConverter<BMAirWayBill>).ConvertRecordToClass(multiRecordEngine);
                    billigMemoRecord.BMAirWayBillList.Add(bMAirWayBill);

                    //Logger.Debug("Billing Memo AWB Breakdown Object is added to Billing Memo Record object successfully.");
                }

                else if (multiRecordEngine.LastRecord is ReasonBreakdownRecord)
                {
                    billingMemo.NumberOfChildRecords += 1;

                    var bMReasonBreakdown = ((IRecordToClassConverter<ReasonBreakdown>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    billigMemoRecord.ReasonRemarks = billigMemoRecord.ReasonRemarks != null ? string.Format("{0}{1}", billigMemoRecord.ReasonRemarks, bMReasonBreakdown.ReasonRemarks) : bMReasonBreakdown.ReasonRemarks;
                    if (bMReasonBreakdown.RemarkSerialNumber == _reasonRemarkSerialNumber + 1)
                    {
                        _reasonRemarkSerialNumber += 1;
                    }

                    //Logger.Debug("Billing Memo Reason Breakdown Object is added to Billing Memo Record object successfully.");
                }
                else
                {
                    break;
                }
            }
            while (multiRecordEngine.LastRecord != null);

            //Logger.Info("End of Converting Childs of BillingMemoRecord into there corresponding Classes.");

        }

        #endregion

    }
}