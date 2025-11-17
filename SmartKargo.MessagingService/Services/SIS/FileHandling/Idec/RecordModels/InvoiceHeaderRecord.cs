using FileHelpers;
using log4net;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.Write;
using QidWorkerRole.SIS.Model;
using System.Globalization;
using System.Text;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    [FixedLengthRecord]
    public class InvoiceHeaderRecord : InvoiceRecordBase, IClassToRecordConverter<Invoice>, IRecordToClassConverter<Invoice>
    {
       
        private const string InvoiceNoProperty = "InvoiceNumber";

        #region Record Properties

        [FieldFixedLength(5), FieldConverter(typeof(PaddingConverter), 5, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string BatchSequenceNumber = string.Empty;

        [FieldFixedLength(5), FieldConverter(typeof(PaddingConverter), 5, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string RecordSequenceWithinBatch = string.Empty;

        [FieldFixedLength(4), FieldConverter(typeof(PaddingConverter), 4, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string BillingMonth = string.Empty;

        [FieldFixedLength(2)]
        public string Filler2 = string.Empty;

        [FieldFixedLength(3)]
        public string CurrencyOfListing = string.Empty;

        [FieldFixedLength(3)]
        public string CurrencyOfBilling = string.Empty;

        [FieldFixedLength(2)]
        public string Filler3 = string.Empty;

        [FieldFixedLength(1)]
        public string Filler4 = string.Empty;

        [FieldFixedLength(8)]
        public string Filler5 = string.Empty;

        [FieldFixedLength(1)]
        public string Filler6 = string.Empty;

        [FieldFixedLength(2), FieldConverter(typeof(PaddingConverter), 2, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string PeriodNumber = string.Empty;

        [FieldFixedLength(80)]
        public string Filler7 = string.Empty;

        [FieldFixedLength(8)]
        public string Filler8 = string.Empty;

        [FieldFixedLength(1)]
        public string SettlementMethod = string.Empty;

        [FieldFixedLength(1)]
        public string DigitalSignatureFlag = string.Empty;

        [FieldFixedLength(6), FieldConverter(typeof(DateFormatConverter))]
        public string InvoiceDate = string.Empty;

        [FieldFixedLength(16), FieldConverter(typeof(DoubleNumberConverter), 16, 5, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string ListingToBillingRate = string.Empty;

        [FieldFixedLength(1)]
        public string IsSuspendedInvoice = string.Empty;

        [FieldFixedLength(7)]
        public string BillingAirlineLocationId = string.Empty;

        [FieldFixedLength(7)]
        public string BilledAirlineLocationId = string.Empty;

        [FieldFixedLength(2)]
        public string InvoiceType = string.Empty;

        [FieldFixedLength(2)]
        public string InvoiceTemplateLanguage = string.Empty;

        [FieldFixedLength(6)]
        public string ChDueDate = string.Empty;

        [FieldFixedLength(5)]
        public string ChAgreementIndicator = string.Empty;

        [FieldFixedLength(286)]
        [FieldTrim(TrimMode.Right)]
        public string Filler9 = string.Empty;

        [FieldHidden]
        public List<ReferenceData1Record> ReferenceData1RecordList = new List<ReferenceData1Record>();

        [FieldHidden]
        public List<AWBRecord> AWBRecordList = new List<AWBRecord>();

        [FieldHidden]
        public List<RejectionMemoRecord> RejectionMemoRecordList = new List<RejectionMemoRecord>();

        [FieldHidden]
        public List<BillingMemoRecord> BillingMemoRecordList = new List<BillingMemoRecord>();

        [FieldHidden]
        public List<CreditMemoRecord> CreditMemoRecordList = new List<CreditMemoRecord>();

        [FieldHidden]
        public List<BillingCodeSubTotalRecord> BillingCodeSubTotalRecordList = new List<BillingCodeSubTotalRecord>();

        [FieldHidden]
        public InvoiceTotalRecord InvoiceTotalRecord;

        [FieldHidden]
        public List<InvoiceVatRecord> InvoiceVatRecordList = new List<InvoiceVatRecord>();

        [FieldHidden]
        public List<InvoiceFooterRecord> InvoiceFooterRecordList = new List<InvoiceFooterRecord>();

        //[FieldHidden]
        //private Dictionary<int, Dictionary<int, int>> _fileRecordSequenceNumber = new Dictionary<int, Dictionary<int, int>>();

        #endregion

        #region Implementation of IClassToRecordConverter<InvoiceHeaderRecord> To Write IDEC File.

        /// <summary>
        /// This method converts the Invoice class into InvoiceHeaderRecord.
        /// </summary>
        /// <param name="invoice">Invoice</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ConvertClassToRecord(Invoice invoice, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting Invoice into InvoiceHeaderRecord.");

            StandardMessageIdentifier = IdecConstants.StandardMessageIdentifier;
            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiInvoiceHeader;
            BillingAirlineCode = invoice.BillingAirline != null ? FileHandling.Idec.Utilities.GetNumericMemberCode(invoice.BillingAirline).ToString() : string.Empty;
            BilledAirlineCode = invoice.BilledAirline != null ? FileHandling.Idec.Utilities.GetNumericMemberCode(invoice.BilledAirline).ToString() : string.Empty;
            InvoiceNumber = invoice.InvoiceNumber;
            BatchSequenceNumber = invoice.BatchSequenceNumber.ToString();
            RecordSequenceWithinBatch = invoice.RecordSequenceWithinBatch.ToString();
            BillingMonth = GetFormattedDate(invoice.BillingMonth, invoice.BillingYear);
            CurrencyOfListing = invoice.CurrencyofListing;
            CurrencyOfBilling = invoice.CurrencyofBilling;
            PeriodNumber = invoice.PeriodNumber.ToString();
            SettlementMethod = invoice.SettlementMethodIndicator;
            DigitalSignatureFlag = invoice.DigitalSignatureFlag;
            InvoiceDate = invoice.InvoiceDate.ToString();
            ListingToBillingRate = invoice.ListingToBillingRate.ToString();
            IsSuspendedInvoice = invoice.SuspendedInvoiceFlag;
            BillingAirlineLocationId = invoice.BillingAirlineLocationID ?? string.Empty;
            BilledAirlineLocationId = invoice.BilledAirlineLocationID ?? string.Empty;
            InvoiceType = invoice.InvoiceType;
            ChDueDate = invoice.ChDueDate;
            ChAgreementIndicator = invoice.ChAgreementIndicator ?? string.Empty;
            InvoiceTemplateLanguage = invoice.InvoiceTemplateLanguage;

            ProcessNextClass(invoice, ref recordSequenceNumber);

            //Logger.Info("End of Converting Invoice into InvoiceHeaderRecord.");
        }

        /// <summary>
        /// gets date in the format 'YYMMDD'
        /// </summary>
        /// <param name="month">month</param>
        /// <param name="year">year</param>
        /// <returns>YYMMDD</returns>
        private static string GetFormattedDate(int month, int year)
        {
            var yearStr = year.ToString();
            var monthStr = month.ToString();

            // This is case when year is passed as 4 digit number as 2016 
            if (yearStr.Length == 4)
            {
                yearStr = yearStr.Substring(2, 2);
            }

            return string.Format("{0}{1}", yearStr.PadLeft(2, '0'), monthStr.PadLeft(2, '0'));
        }

        /// <summary>
        /// Convert Child of Invoice Class into there corresponding Records.
        /// </summary>
        /// <param name="invoice">Invoice</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ProcessNextClass(Invoice invoice, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting Childs of Invoice Class into there corresponding Records.");

            if (invoice != null)
            {
                var invoiceLocalCopy = invoice;

                #region Reference Data

                if (invoiceLocalCopy.ReferenceDataList != null)
                {
                    // Add Reference Data 1 Records in the list 
                    foreach (var referenceDataList in invoiceLocalCopy.ReferenceDataList.OrderByDescending(c => c.IsBillingMember))
                    {
                        var referenceData1Record = new ReferenceData1Record(this);
                        referenceData1Record.ConvertClassToRecord(referenceDataList, ref recordSequenceNumber);
                        ReferenceData1RecordList.Add(referenceData1Record);
                    }
                }

                #endregion

                #region AirWayBills & BillingCodeSubTotals of AirWayBills

                if (invoiceLocalCopy.AirWayBillList.Count > 0)
                {
                    foreach (var airWayBillList in invoiceLocalCopy.AirWayBillList.OrderBy(i => i.BillingCode).GroupBy(couponRecord => new { AwbBillingCode = couponRecord.BillingCode }))
                    {
                        #region AirWayBills

                        foreach (var airWayBill in airWayBillList.OrderBy(awbBatch => awbBatch.BatchSequenceNumber).ThenBy(awbSeq => awbSeq.RecordSequenceWithinBatch))
                        {
                            var awbRecord = new AWBRecord(this);
                            awbRecord.ConvertClassToRecord(airWayBill, ref recordSequenceNumber);
                            AWBRecordList.Add(awbRecord);
                        }

                        #endregion

                        #region BillingCodeSubTotals of AirWayBills

                        if (invoiceLocalCopy.BillingCodeSubTotalList != null)
                        {
                            var billingCodeSubTotal = invoiceLocalCopy.BillingCodeSubTotalList.Find(i => i.BillingCode == airWayBillList.Key.AwbBillingCode);

                            // Add BillingCodeSubTotal data
                            if (billingCodeSubTotal != null)
                            {
                                var billingCodeSubTotalRecord = new BillingCodeSubTotalRecord(this);
                                billingCodeSubTotalRecord.ConvertClassToRecord(billingCodeSubTotal, ref recordSequenceNumber);
                                BillingCodeSubTotalRecordList.Add(billingCodeSubTotalRecord);
                            }
                        }

                        #endregion
                    }
                }

                #endregion

                #region RejectionMemos & BillingCodeSubTotals of RejectionMemos

                if (invoiceLocalCopy.RejectionMemoList.Count > 0)
                {
                    foreach (var rejectionMemoList in invoiceLocalCopy.RejectionMemoList.OrderBy(i => i.BillingCode).GroupBy(rmRecord => new { rmRecord.BillingCode }))
                    {
                        #region RejectionMemos

                        foreach (var rejectionMemo in rejectionMemoList.OrderBy(i => i.BatchSequenceNumber).ThenBy(j => j.RecordSequenceWithinBatch))
                        {
                            var rejectionMemoRecord = new RejectionMemoRecord(this);
                            rejectionMemoRecord.ConvertClassToRecord(rejectionMemo, ref recordSequenceNumber);
                            RejectionMemoRecordList.Add(rejectionMemoRecord);
                        }

                        #endregion

                        #region BillingCodeSubTotals of RejectionMemos

                        if (invoiceLocalCopy.BillingCodeSubTotalList != null)
                        {
                            var billingCodeSubTotal = invoiceLocalCopy.BillingCodeSubTotalList.Find(i => i.BillingCode == rejectionMemoList.Key.BillingCode);

                            // Add BillingCodeSubTotal data
                            if (billingCodeSubTotal != null)
                            {
                                var billingCodeSubTotalRecord = new BillingCodeSubTotalRecord(this);
                                billingCodeSubTotalRecord.ConvertClassToRecord(billingCodeSubTotal, ref recordSequenceNumber);
                                BillingCodeSubTotalRecordList.Add(billingCodeSubTotalRecord);
                            }
                        }

                        #endregion

                    }
                }

                #endregion

                #region BillingMemos & BillingCodeSubTotals of BillingMemos

                if (invoiceLocalCopy.BillingMemoList.Count > 0)
                {
                    foreach (var billingMemoList in invoiceLocalCopy.BillingMemoList.OrderBy(i => i.BillingCode).GroupBy(bmRecord => new { bmRecord.BillingCode }))
                    {

                        #region BillingMemos

                        foreach (var billingMemo in billingMemoList.OrderBy(i => i.BatchSequenceNumber).ThenBy(j => j.RecordSequenceWithinBatch))
                        {
                            var billingMemoRecord = new BillingMemoRecord(this);
                            billingMemoRecord.ConvertClassToRecord(billingMemo, ref recordSequenceNumber);
                            BillingMemoRecordList.Add(billingMemoRecord);
                        }

                        #endregion

                        #region BillingCodeSubTotals of BillingMemos

                        if (invoiceLocalCopy.BillingCodeSubTotalList != null)
                        {
                            var billingCodeSubTotal = invoiceLocalCopy.BillingCodeSubTotalList.Find(i => i.BillingCode == billingMemoList.Key.BillingCode);

                            // Add BillingCodeSubTotal data
                            if (billingCodeSubTotal != null)
                            {
                                var billingCodeSubTotalRecord = new BillingCodeSubTotalRecord(this);
                                billingCodeSubTotalRecord.ConvertClassToRecord(billingCodeSubTotal, ref recordSequenceNumber);
                                BillingCodeSubTotalRecordList.Add(billingCodeSubTotalRecord);
                            }
                        }

                        #endregion

                    }
                }

                #endregion

                #region CreditMemos & BillingCodeSubTotals of CreditMemos

                if (invoiceLocalCopy.CreditMemoList.Count > 0)
                {
                    foreach (var creditMemoList in invoiceLocalCopy.CreditMemoList.OrderBy(i => i.BillingCode).GroupBy(cmRecord => new { cmRecord.BillingCode }))
                    {

                        #region CreditMemos

                        foreach (var creditMemo in creditMemoList.OrderBy(i => i.BatchSequenceNumber).ThenBy(j => j.RecordSequenceWithinBatch))
                        {
                            var creditMemoRecord = new CreditMemoRecord(this);
                            creditMemoRecord.ConvertClassToRecord(creditMemo, ref recordSequenceNumber);
                            CreditMemoRecordList.Add(creditMemoRecord);
                        }

                        #endregion

                        #region BillingCodeSubTotals of CreditMemos

                        if (invoiceLocalCopy.BillingCodeSubTotalList != null)
                        {
                            var billingCodeSubTotal = invoiceLocalCopy.BillingCodeSubTotalList.Find(i => i.BillingCode == creditMemoList.Key.BillingCode);

                            //Add BillingCodeSubTotal data
                            if (billingCodeSubTotal != null)
                            {
                                var billingCodeSubTotalRecord = new BillingCodeSubTotalRecord(this);
                                billingCodeSubTotalRecord.ConvertClassToRecord(billingCodeSubTotal, ref recordSequenceNumber);
                                BillingCodeSubTotalRecordList.Add(billingCodeSubTotalRecord);
                            }
                        }

                        #endregion

                    }
                }

                #endregion

                #region InvoiceTotal

                if (invoiceLocalCopy.InvoiceTotals != null)
                {
                    // Add InvoiceTotalRecord data      
                    var invoiceTotalRecord = new InvoiceTotalRecord(this);
                    invoiceTotalRecord.ConvertClassToRecord(invoiceLocalCopy.InvoiceTotals, ref recordSequenceNumber);
                    InvoiceTotalRecord = invoiceTotalRecord;
                }

                #endregion

                #region InvoiceTotalVat

                if (invoiceLocalCopy.InvoiceTotalVATList != null && invoiceLocalCopy.InvoiceTotalVATList.Count > 0)
                {
                    foreach (var invoiceTotalVATList in Utilities.GetDividedSubCollections(invoiceLocalCopy.InvoiceTotalVATList, 2))
                    {
                        var invoiceVatRecord = new InvoiceVatRecord(this);
                        invoiceVatRecord.ConvertClassToRecord(invoiceTotalVATList, ref recordSequenceNumber);
                        InvoiceVatRecordList.Add(invoiceVatRecord);
                    }
                }

                #endregion

                #region Invoice Footer

                var footerTextLength = (invoice.InvoiceFooterDetails != null) ? invoice.InvoiceFooterDetails.Trim().Length : 0;
                var serialNumber = 0;

                while (footerTextLength > 0)
                {
                    var invoiceFooterRecord = new InvoiceFooterRecord(this);
                    var invoiceFooter = new StringBuilder();

                    if (invoice.InvoiceFooterDetails != null)
                    {
                        invoiceFooter.Append(invoice.InvoiceFooterDetails.Substring(serialNumber * 350, footerTextLength));
                    }
                    serialNumber++;
                    invoiceFooterRecord.FooterSerialNo = serialNumber.ToString();
                    invoiceFooterRecord.ConvertClassToRecord(invoiceFooter, ref recordSequenceNumber);
                    InvoiceFooterRecordList.Add(invoiceFooterRecord);
                    footerTextLength = (footerTextLength >= 350) ? footerTextLength - 350 : 0;
                }

                #endregion

            }
            //Logger.Info("End of Converting Childs of Invoice Class into there corresponding Records.");
        }

        #endregion

        #region Implementation of IRecordToClassConverter<Invoice> To Read IDEC File.

        /// <summary>
        /// To Convert InvoiceHeaderRecord into Invoice.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <returns>Invoice</returns>
        public Invoice ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Info("Start of Converting InvoiceHeaderRecord into Invoice.");

            // for logging.
            ThreadContext.Properties[InvoiceNoProperty] = InvoiceNumber;

            var invoice = new Invoice();

            // Create new invoice object.
            invoice = CreateInvoice();

            // _fileRecordSequenceNumber.Clear();

            ProcessNextRecord(multiRecordEngine, invoice);

            //Logger.Info("End of Converting InvoiceHeaderRecord into Invoice.");

            return invoice;
        }

        /// <summary>
        /// Creates Invoice for InvoiceHeaderRecord present in the reading input IDEC File.
        /// </summary>
        /// <returns>Invoice</returns>
        private Invoice CreateInvoice()
        {
            //Logger.Info("Start of Creating Invoice Class from InvoiceHeaderRecord.");

            var cultureInfo = new CultureInfo("en-US");
            cultureInfo.Calendar.TwoDigitYearMax = 2099;

            var invoice = new Invoice
            {
                BillingAirline = Convert.ToString(FileHandling.Idec.Utilities.GetMemberNumericCode(Convert.ToInt32(BillingAirlineCode))),
                BilledAirline = Convert.ToString(FileHandling.Idec.Utilities.GetMemberNumericCode(Convert.ToInt32(BilledAirlineCode))),
                InvoiceNumber = InvoiceNumber.Trim(),
                SettlementMethodIndicator = SettlementMethod,
                DigitalSignatureFlag = DigitalSignatureFlag,
                InvoiceType = InvoiceType,
                BillingAirlineLocationID = BillingAirlineLocationId.Trim(),
                BilledAirlineLocationID = BilledAirlineLocationId.Trim(),
                BatchSequenceNumber = int.Parse(BatchSequenceNumber),
                RecordSequenceWithinBatch = int.Parse(RecordSequenceWithinBatch),
                CurrencyofListing = CurrencyOfListing,
                ListingToBillingRate = decimal.Parse(ListingToBillingRate, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                PeriodNumber = int.Parse(PeriodNumber),
                InvoiceTemplateLanguage = InvoiceTemplateLanguage.Trim(),
                ChDueDate = ChDueDate,
                ChAgreementIndicator = ChAgreementIndicator.Trim()
            };

            invoice.CurrencyofBilling = CurrencyOfBilling;

            var billingYear = 0;
            var billingMonth = 0;

            // Billing date 
            DateTime billingDate;
            // To avoid converting year 30 into year 1930
            if (DateTime.TryParseExact(BillingMonth.Substring(0, 4), IdecConstants.BillingDateFormat, cultureInfo, DateTimeStyles.None, out billingDate))
            {
                billingYear = billingDate.Year;
                billingMonth = billingDate.Month;
            }
            else
            {
                billingYear = Convert.ToInt32(BillingMonth.Substring(0, 2));
                billingMonth = Convert.ToInt32(BillingMonth.Substring(2, 2));
            }

            invoice.BillingYear = billingYear;
            invoice.BillingMonth = billingMonth;

            // Invoice Date
            DateTime invoiceDate;
            if (DateTime.TryParseExact(InvoiceDate, IdecConstants.InvoiceDateFormat, cultureInfo, DateTimeStyles.None, out invoiceDate))
            {
                invoice.InvoiceDate = invoiceDate;
            }

            invoice.CurrencyofListing = CurrencyOfListing;

            //Logger.Info("End of Creating Invoice Class from InvoiceHeaderRecord.");

            return invoice;
        }

        /// <summary>
        /// To Convert Child records of InvoiceHeaderRecord into there corresponding Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="invoice">Invoice</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, Invoice invoice)
        {
            //Logger.Info("Start of Converting Childs of InvoiceHeaderRecord into there corresponding Classes.");

            multiRecordEngine.ReadNext();

            do
            {
                var invoiceLocalCopy = invoice;

                #region ReferenceDataRecords

                if (multiRecordEngine.LastRecord is ReferenceData1Record)
                {
                    var referenceData1Record = multiRecordEngine.LastRecord as ReferenceData1Record;
                    invoiceLocalCopy.NumberOfChildRecords += 2;

                    var referenceDataModel = (multiRecordEngine.LastRecord as IRecordToClassConverter<ReferenceDataModel>).ConvertRecordToClass(multiRecordEngine);

                    invoiceLocalCopy.ReferenceDataList.Add(referenceDataModel);
                }

                #endregion

                #region AWBRecords

                else if (multiRecordEngine.LastRecord is AWBRecord)
                {
                    var awbRecord = multiRecordEngine.LastRecord as AWBRecord;

                    var airWaybill = (multiRecordEngine.LastRecord as IRecordToClassConverter<AirWayBill>).ConvertRecordToClass(multiRecordEngine);

                    //if (_fileRecordSequenceNumber.ContainsKey(airWaybill.BatchSequenceNumber))
                    //{
                    //    _fileRecordSequenceNumber[airWaybill.BatchSequenceNumber].Add(airWaybill.AirWayBillID, airWaybill.RecordSequenceWithinBatch);
                    //}
                    //else
                    //{
                    //    _fileRecordSequenceNumber.Add(airWaybill.BatchSequenceNumber, new Dictionary<int, int>());
                    //    _fileRecordSequenceNumber[airWaybill.BatchSequenceNumber].Add(airWaybill.AirWayBillID, airWaybill.RecordSequenceWithinBatch);
                    //}

                    invoiceLocalCopy.AirWayBillList.Add(airWaybill);
                }

                #endregion

                #region RejectionMemoRecords

                else if (multiRecordEngine.LastRecord is RejectionMemoRecord)
                {
                    var rmRecord = multiRecordEngine.LastRecord as RejectionMemoRecord;

                    var rejectionMemoRecord = (multiRecordEngine.LastRecord as IRecordToClassConverter<RejectionMemo>).ConvertRecordToClass(multiRecordEngine);

                    //if (_fileRecordSequenceNumber.ContainsKey(rejectionMemoRecord.BatchSequenceNumber))
                    //    _fileRecordSequenceNumber[rejectionMemoRecord.BatchSequenceNumber].Add(rejectionMemoRecord.RejectionMemoID, rejectionMemoRecord.RecordSequenceWithinBatch);
                    //else
                    //{
                    //    _fileRecordSequenceNumber.Add(rejectionMemoRecord.BatchSequenceNumber, new Dictionary<int, int>());
                    //    _fileRecordSequenceNumber[rejectionMemoRecord.BatchSequenceNumber].Add(rejectionMemoRecord.RejectionMemoID, rejectionMemoRecord.RecordSequenceWithinBatch);
                    //}

                    invoiceLocalCopy.RejectionMemoList.Add(rejectionMemoRecord);
                }

                #endregion

                #region BillingMemoRecords

                else if (multiRecordEngine.LastRecord is BillingMemoRecord)
                {
                    var bmRecord = multiRecordEngine.LastRecord as BillingMemoRecord;

                    var billMemoRecord = (multiRecordEngine.LastRecord as IRecordToClassConverter<BillingMemo>).ConvertRecordToClass(multiRecordEngine);

                    //if (_fileRecordSequenceNumber.ContainsKey(billMemoRecord.BatchSequenceNumber))
                    //{
                    //    _fileRecordSequenceNumber[billMemoRecord.BatchSequenceNumber].Add(billMemoRecord.BillingMemoID, billMemoRecord.RecordSequenceWithinBatch);
                    //}
                    //else
                    //{
                    //    _fileRecordSequenceNumber.Add(billMemoRecord.BatchSequenceNumber, new Dictionary<int, int>());
                    //    _fileRecordSequenceNumber[billMemoRecord.BatchSequenceNumber].Add(billMemoRecord.BillingMemoID, billMemoRecord.RecordSequenceWithinBatch);
                    //}

                    if (invoiceLocalCopy.BillingMemoList != null)
                    {
                        invoiceLocalCopy.BillingMemoList.Add(billMemoRecord);
                    }

                }

                #endregion

                #region CreditMemoRecords

                else if (multiRecordEngine.LastRecord is CreditMemoRecord)
                {
                    var cmRecord = multiRecordEngine.LastRecord as CreditMemoRecord;

                    var cargoCreditMemo = (multiRecordEngine.LastRecord as IRecordToClassConverter<CreditMemo>).ConvertRecordToClass(multiRecordEngine);

                    //if (_fileRecordSequenceNumber.ContainsKey(cargoCreditMemo.BatchSequenceNumber))
                    //{
                    //    _fileRecordSequenceNumber[cargoCreditMemo.BatchSequenceNumber].Add(cargoCreditMemo.CreditMemoID, cargoCreditMemo.RecordSequenceWithinBatch);
                    //}
                    //else
                    //{
                    //    _fileRecordSequenceNumber.Add(cargoCreditMemo.BatchSequenceNumber, new Dictionary<int, int>());
                    //    _fileRecordSequenceNumber[cargoCreditMemo.BatchSequenceNumber].Add(cargoCreditMemo.CreditMemoID, cargoCreditMemo.RecordSequenceWithinBatch);
                    //}

                    if (invoiceLocalCopy.CreditMemoList != null)
                    {
                        invoiceLocalCopy.CreditMemoList.Add(cargoCreditMemo);
                    }

                }

                #endregion

                #region InvoiceTotalRecord

                else if (multiRecordEngine.LastRecord is InvoiceTotalRecord)
                {
                    var totalRecord = multiRecordEngine.LastRecord as InvoiceTotalRecord;

                    invoiceLocalCopy.NumberOfChildRecords += 1;
                    var invoiceTotalRecord = (multiRecordEngine.LastRecord as IRecordToClassConverter<InvoiceTotal>).ConvertRecordToClass(multiRecordEngine);

                    invoiceLocalCopy.InvoiceTotals = invoiceTotalRecord;
                }

                #endregion

                #region InvoiceVatRecord

                else if (multiRecordEngine.LastRecord is InvoiceVatRecord)
                {
                    var vatRecord = multiRecordEngine.LastRecord as InvoiceVatRecord;

                    var invoiceTotalVatList = (multiRecordEngine.LastRecord as IRecordToClassConverter<List<InvoiceTotalVAT>>).ConvertRecordToClass(multiRecordEngine);

                    var noOfTaxRecords = invoiceTotalVatList.Count;
                    invoiceLocalCopy.NumberOfChildRecords += 1;
                    for (var i = 0; i < noOfTaxRecords; i++)
                    {
                        invoiceLocalCopy.InvoiceTotalVATList.Add(invoiceTotalVatList[i]);
                    }
                }

                #endregion

                #region InvoiceFooterRecord

                else if (multiRecordEngine.LastRecord is InvoiceFooterRecord)
                {
                    var footerRecord = multiRecordEngine.LastRecord as InvoiceFooterRecord;

                    invoiceLocalCopy.NumberOfChildRecords += 1;

                    var invoiceFooter = (multiRecordEngine.LastRecord as IRecordToClassConverter<StringBuilder>).ConvertRecordToClass(multiRecordEngine);
                    if (invoiceFooter != null)
                    {
                        invoiceLocalCopy.InvoiceFooterDetails = invoiceLocalCopy.InvoiceFooterDetails != null ? string.Format("{0}{1}", invoiceLocalCopy.InvoiceFooterDetails, !string.IsNullOrEmpty(invoiceFooter.ToString()) ? invoiceFooter.ToString().Trim() : null) : !string.IsNullOrEmpty(invoiceFooter.ToString()) ? invoiceFooter.ToString().Trim() : null;
                    }
                }

                #endregion InvoiceFooterRecord

                #region BillingCodeSubTotalRecord

                else if (multiRecordEngine.LastRecord is BillingCodeSubTotalRecord)
                {
                    var totalRecord = multiRecordEngine.LastRecord as BillingCodeSubTotalRecord;

                    var billingCodeSubTotal = (multiRecordEngine.LastRecord as IRecordToClassConverter<BillingCodeSubTotal>).ConvertRecordToClass(multiRecordEngine);

                    if (billingCodeSubTotal != null)
                    {
                        invoiceLocalCopy.BillingCodeSubTotalList.Add(billingCodeSubTotal);
                    }
                }

                #endregion

                else
                {
                    break;
                }

            }
            while (multiRecordEngine.LastRecord != null);

            //Logger.Info("End of Converting Childs of InvoiceHeaderRecord into there corresponding Classes.");
        }

        #endregion

    }
}