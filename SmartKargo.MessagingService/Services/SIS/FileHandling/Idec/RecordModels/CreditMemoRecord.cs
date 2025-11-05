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
    public class CreditMemoRecord : MemoRecordBase, IRecordToClassConverter<CreditMemo>, IClassToRecordConverter<CreditMemo>
    {
        
        [FieldHidden]
        private int _reasonRemarkSerialNumber;

        [FieldHidden]
        public List<ReasonBreakdownRecord> ReasonBreakdownRecordList = new List<ReasonBreakdownRecord>();

        [FieldHidden]
        public List<CMAWBRecord> CMAWBRecordList = new List<CMAWBRecord>();

        [FieldHidden]
        public List<CMVatRecord> CMVatRecordList = new List<CMVatRecord>();

        public CreditMemoRecord()
        {
        }

        /// <summary>
        /// To set common properties
        /// </summary>
        /// <param name="invoiceRecordBase"></param>
        public CreditMemoRecord(InvoiceRecordBase invoiceRecordBase)
            : base(invoiceRecordBase)
        { }

        /// <summary>
        /// converts it into mode instance.
        /// </summary>
        /// <param name = "multiRecordEngine">MultiRecordEngine instance.</param>
        /// <returns>Returns mode instance created from record.</returns>
        public CreditMemo ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            var creditMemo = CreateCreditMemoDataObject();

            ProcessNextRecord(multiRecordEngine, creditMemo);

            return creditMemo;
        }

        /// <summary>
        /// Converts child records of current record into model classes and adds them to the current mode record.
        /// </summary>
        /// <param name = "multiRecordEngine">MultiRecordEngine instance</param>
        /// <param name = "creditMemo">Parent model record to which all the child records will added.</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, CreditMemo creditMemo)
        {
            var creditMemoRecord = creditMemo;
            multiRecordEngine.ReadNext();
            _reasonRemarkSerialNumber = 0;

            do
            {
                if (multiRecordEngine.LastRecord is VatRecordBase)
                {
                    creditMemo.NumberOfChildRecords += 1;

                    var cMVATList = ((IRecordToClassConverter<List<CMVAT>>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    var noOfCMVATRecords = cMVATList.Count;
                    for (var i = 0; i < noOfCMVATRecords; i++)
                    {
                        creditMemoRecord.CMVATList.Add(cMVATList[i]);
                    }
                    //Logger.Debug("CMVAT List Object is added to a Credit Memo Record object successfully.");
                }

                else if (multiRecordEngine.LastRecord is CMAWBRecord)
                {
                    creditMemo.NumberOfChildRecords += 1;

                    var cMAirWayBill = (multiRecordEngine.LastRecord as IRecordToClassConverter<CMAirWayBill>).ConvertRecordToClass(multiRecordEngine);
                    creditMemoRecord.CMAirWayBillList.Add(cMAirWayBill);

                    //Logger.Debug("CMAWBRecord Object is added to Credit Memo Record object successfully.");
                }

                else if (multiRecordEngine.LastRecord is ReasonBreakdownRecord)
                {
                    creditMemo.NumberOfChildRecords += 1;

                    var reasonBreakdown = ((IRecordToClassConverter<ReasonBreakdown>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    creditMemoRecord.ReasonRemarks = creditMemoRecord.ReasonRemarks != null ? string.Format("{0}{1}", creditMemoRecord.ReasonRemarks, reasonBreakdown.ReasonRemarks) : reasonBreakdown.ReasonRemarks;
                    if (reasonBreakdown.RemarkSerialNumber == _reasonRemarkSerialNumber + 1)
                    {
                        _reasonRemarkSerialNumber += 1;
                    }

                    //Logger.Debug("Credit Memo ReasonBreakdownRecord Object is added to Credit Memo Record object successfully.");
                }
                else
                {
                    break;
                }
            }
            while (multiRecordEngine.LastRecord != null);
        }

        /// <summary>
        /// Creates Credit Memo Record object with information from IDEC invoice record.
        /// </summary>
        /// <returns>Returns Credit Memo Record object.</returns>
        private CreditMemo CreateCreditMemoDataObject()
        {
            //Logger.Debug("Creating Credit Memo record object.");

            var yourInvoiceBillingYear = 0;
            var yourInvoiceBillingMonth = 0;
            var yourInvoiceBillingPeriod = 0;

            var creditMemo = new CreditMemo
            {
                BatchSequenceNumber = Convert.ToInt32(BatchSequenceNumber.Trim()),
                RecordSequenceWithinBatch = Convert.ToInt32(RecordSequenceWithinBatch.Trim()),
                CreditMemoNumber = BillingOrCreditMemoNumber.Trim(),
                BillingCode = BillingCode,
                ReasonCode = ReasonCode.Trim(),
                OurRef = OurRef.Trim(),
                CorrespondenceRefNumber = CorrespondenceRefNumber.Trim(),
                YourInvoiceNumber = YourInvoiceNumber.Trim(),
                TotalWeightCharges = Utilities.GetActualValueForDecimal(TotalWeightChargesAmountBilledOrCreditedSign, TotalWeightChargesAmountBilledOrCredited),
                TotalValuationAmt = Utilities.GetActualValueForDecimal(TotalValuationAmountBilledOrCreditedSign, TotalValuationAmountBilledOrCredited),
                TotalOtherChargeAmt = Utilities.GetActualValueForDecimal(TotalOtherChargesAmountBilledOrCreditedSign, TotalOtherChargesAmountBilledOrCredited),
                TotalIscAmountCredited = Utilities.GetActualValueForDecimal(TotalISCAmountBilledOrCreditedSign, TotalISCAmountBilledOrCredited),
                TotalVatAmountCredited = Utilities.GetActualValueForDecimal(TotalVATAmountBilledOrCreditedSign, TotalVATAmountBilledOrCredited),
                NetAmountCredited = Utilities.GetActualValueForDecimal(NetAmountBilledOrCreditedSign, NetAmountBilledOrCredited),
                AttachmentIndicatorOriginal = AttachmentIndicatorOriginal == IdecConstants.TrueValue,
                NumberOfAttachments = Convert.ToInt32(NumberOfAttachments),
                AirlineOwnUse = AirlineOwnUse.Trim()
            };

            if (!String.IsNullOrEmpty(YourInvoiceBillingDate) && (Convert.ToInt32(YourInvoiceBillingDate) != 0))
            {
                // Your Invoice Credit Date
                DateTime yourInvoiceBillingDate;
                // To avoid converting year 30 into year 1930
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

            creditMemo.YourInvoiceBillingYear = yourInvoiceBillingYear;
            creditMemo.YourInvoiceBillingMonth = yourInvoiceBillingMonth;
            creditMemo.YourInvoiceBillingPeriod = yourInvoiceBillingPeriod;

            //Logger.Debug("Credit Memo record object created.");

            return creditMemo;
        }

        /// <summary>
        /// This method converts the CreditMemo model into corresponding record instance
        /// </summary>
        public void ConvertClassToRecord(CreditMemo creditMemo, ref long recordSequenceNumber)
        {
            //Logger.Debug("Converting CreditMemo model to creditMemoRecord record instance.");
            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiCreditMemoRecord;
            BatchSequenceNumber = creditMemo.BatchSequenceNumber.ToString();
            RecordSequenceWithinBatch = creditMemo.RecordSequenceWithinBatch.ToString();
            BillingOrCreditMemoNumber = creditMemo.CreditMemoNumber;
            BillingCode = creditMemo.BillingCode;
            ReasonCode = creditMemo.ReasonCode;
            OurRef = creditMemo.OurRef;

            if (creditMemo.ReasonCode != null && (creditMemo.ReasonCode.Equals("6A") || creditMemo.ReasonCode.Equals("6B")))
            {
                CorrespondenceRefNumber = creditMemo.CorrespondenceRefNumber;
            }

            YourInvoiceNumber = string.IsNullOrEmpty(creditMemo.YourInvoiceNumber) ? string.Empty : creditMemo.YourInvoiceNumber.Trim();

            if (!string.IsNullOrEmpty(creditMemo.YourInvoiceNumber))
            {
                YourInvoiceBillingDate = Utilities.GetFormattedPeriod(creditMemo.YourInvoiceBillingMonth, creditMemo.YourInvoiceBillingYear, creditMemo.YourInvoiceBillingPeriod);
            }

            TotalWeightChargesAmountBilledOrCredited = creditMemo.TotalWeightCharges.HasValue ? Math.Abs(creditMemo.TotalWeightCharges.Value).ToString() : null;
            TotalWeightChargesAmountBilledOrCreditedSign = creditMemo.TotalWeightCharges.HasValue ? Utilities.GetSignValue(creditMemo.TotalWeightCharges.Value) : null;
            TotalValuationAmountBilledOrCredited = creditMemo.TotalValuationAmt.HasValue ? Math.Abs(creditMemo.TotalValuationAmt.Value).ToString() : null;
            TotalValuationAmountBilledOrCreditedSign = creditMemo.TotalValuationAmt.HasValue ? Utilities.GetSignValue(creditMemo.TotalValuationAmt.Value) : null;
            TotalOtherChargesAmountBilledOrCredited = Math.Abs(creditMemo.TotalOtherChargeAmt).ToString();
            TotalOtherChargesAmountBilledOrCreditedSign = Utilities.GetSignValue(creditMemo.TotalOtherChargeAmt);
            TotalISCAmountBilledOrCredited = Math.Abs(creditMemo.TotalIscAmountCredited).ToString();
            TotalISCAmountBilledOrCreditedSign = Utilities.GetSignValue(creditMemo.TotalIscAmountCredited);
            TotalVATAmountBilledOrCredited = creditMemo.TotalVatAmountCredited.HasValue ? Math.Abs(creditMemo.TotalVatAmountCredited.Value).ToString() : null;
            TotalVATAmountBilledOrCreditedSign = creditMemo.TotalVatAmountCredited.HasValue ? Utilities.GetSignValue(creditMemo.TotalVatAmountCredited.Value) : null;
            NetAmountBilledOrCredited = creditMemo.NetAmountCredited.HasValue ? Math.Abs(creditMemo.NetAmountCredited.Value).ToString() : null;
            NetAmountBilledOrCreditedSign = creditMemo.NetAmountCredited.HasValue ? Utilities.GetSignValue(creditMemo.NetAmountCredited.Value) : null;
            AttachmentIndicatorOriginal = Utilities.GetBooDisplaylValue(creditMemo.AttachmentIndicatorOriginal);
            AttachmentIndicatorValidated = creditMemo.AttachmentIndicatorValidated.HasValue ? Utilities.GetBooDisplaylValue(creditMemo.AttachmentIndicatorValidated.Value) : null; //TODO: Update this field(pax_billing_memo.attchmnt_ind_validated) when validated in Db.
            NumberOfAttachments = creditMemo.NumberOfAttachments.HasValue ? creditMemo.NumberOfAttachments.Value.ToString() : "0"; //TODO: Update this field(pax_billing_memo.attchmnts_no) when validated in Db.
            AirlineOwnUse = creditMemo.AirlineOwnUse;
            ISValidationFlag = creditMemo.ISValidationFlag;

            ProcessNextClass(creditMemo, ref recordSequenceNumber);
        }

        /// <summary>
        /// This method adds the child records of parent record to corresponding list by calling respective functions.
        /// </summary>
        public void ProcessNextClass(CreditMemo creditMemo, ref long recordSequenceNumber)
        {
            //Logger.Debug("Processing CreditMemo model child objects.");
            if (creditMemo != null)
            {
                // Reason breakdown
                string cMReasonRemarks = creditMemo.ReasonRemarks != null ? creditMemo.ReasonRemarks.Replace("\r", "").Replace("\n", "").Trim() : string.Empty;
                int remarksLength = cMReasonRemarks.Trim().Length;
                int serialNumber = 0;
                while (remarksLength > 0)
                {
                    var reasonBreakdownRecord = new ReasonBreakdownRecord(this);
                    var reasonBreakdown = new ReasonBreakdown { ReasonRemarks = cMReasonRemarks.Substring(serialNumber * 400, remarksLength), RemarkSerialNumber = ++serialNumber };
                    reasonBreakdownRecord.ConvertClassToRecord(reasonBreakdown, ref recordSequenceNumber);
                    ReasonBreakdownRecordList.Add(reasonBreakdownRecord);
                    remarksLength = (remarksLength >= 400) ? remarksLength - 400 : 0;
                }

                if (creditMemo.CMVATList.Count > 0)
                {
                    // Add CMVATRecord in the list
                    foreach (var cMVATList in Utilities.GetDividedSubCollections(creditMemo.CMVATList, 2))
                    {
                        var cMVatRecord = new CMVatRecord(this);
                        cMVatRecord.ConvertClassToRecord(cMVATList, ref recordSequenceNumber);
                        CMVatRecordList.Add(cMVatRecord);
                    }
                }

                if (creditMemo.CMAirWayBillList.Count > 0)
                {
                    foreach (var cMAirWayBillList in creditMemo.CMAirWayBillList.OrderBy(c => c.BreakdownSerialNumber))
                    {
                        var cMAWBRecord = new CMAWBRecord(this);
                        cMAWBRecord.ConvertClassToRecord(cMAirWayBillList, ref recordSequenceNumber);
                        CMAWBRecordList.Add(cMAWBRecord);
                    }
                }
            }
        }
    }
}