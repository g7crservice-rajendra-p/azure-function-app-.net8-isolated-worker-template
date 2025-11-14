using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.Write;
using QidWorkerRole.SIS.Model;
using System.Globalization;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    [FixedLengthRecord()]
    public class AWBRecord : InvoiceRecordBase, IClassToRecordConverter<AirWayBill>, IRecordToClassConverter<AirWayBill>
    {
        
        #region Record Properties

        [FieldFixedLength(5), FieldConverter(typeof(PaddingConverter), 5, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string BatchSequenceNumber;

        [FieldFixedLength(5), FieldConverter(typeof(PaddingConverter), 5, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string RecordSequenceWithinBatch;

        [FieldFixedLength(6), FieldConverter(typeof(PaddingConverter), 6, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
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

        [FieldFixedLength(6), FieldConverter(typeof(PaddingConverter), 6, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string DateOfCarriage;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string WeightCharges;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string OtherCharges;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string AmountSubjectToInterlineServiceCharges;

        [FieldFixedLength(5), FieldConverter(typeof(DoubleNumberConverter), 5, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string InterlineServiceChargePercent;

        [FieldFixedLength(1)]
        public string InterlineServiceChargeRateSign;

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

        [FieldFixedLength(4)]
        public string Filler2;

        [FieldFixedLength(10)]
        public string FillingReference;

        [FieldFixedLength(8)]
        public string Filler3;

        [FieldFixedLength(1)]
        public string WeightChargesSign;

        [FieldFixedLength(1)]
        public string OtherChargesSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string ValuationCharges;

        [FieldFixedLength(1)]
        public string ValuationChargesSign;

        [FieldFixedLength(1)]
        public string KgOrLBIndicator;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VATAmount;

        [FieldFixedLength(1)]
        public string VATAmountSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string InterlineServiceChargeAmount;

        [FieldFixedLength(1)]
        public string InterlineServiceChargeAmountSign;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string AWBTotalAmount;

        [FieldFixedLength(1)]
        public string AWBTotalAmountSign;

        [FieldFixedLength(1)]
        public string CCAIndicator;

        [FieldFixedLength(20)]
        public string OurRef;

        [FieldFixedLength(1)]
        public string AttachmentIndicatorOriginal;

        [FieldFixedLength(1)]
        public string AttachmentIndicatorValidated;

        [FieldFixedLength(4), FieldConverter(typeof(PaddingConverter), 4, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string NumberOfAttachments;

        [FieldFixedLength(10)]
        public string ISValidationFlag;

        [FieldFixedLength(2)]
        public string ReasonCode = string.Empty;

        [FieldFixedLength(10)]
        public string ReferenceField1 = string.Empty;

        [FieldFixedLength(10)]
        public string ReferenceField2 = string.Empty;

        [FieldFixedLength(10)]
        public string ReferenceField3 = string.Empty;

        [FieldFixedLength(10)]
        public string ReferenceField4 = string.Empty;

        [FieldFixedLength(20)]
        public string ReferenceField5 = string.Empty;

        [FieldFixedLength(20)]
        public string AirlineOwnUse = string.Empty;

        [FieldFixedLength(1)]
        public string AmountSubjectToInterlineServiceChargesSign;

        [FieldFixedLength(169)]
        public string Filler4;

        [FieldHidden]
        public List<AWBVatBreakdownRecord> AWBVatBreakdownRecordList = new List<AWBVatBreakdownRecord>();

        [FieldHidden]
        public List<AWBOCBreakdownRecord> AWBOCBreakdownRecordList = new List<AWBOCBreakdownRecord>();

        #endregion

        #region Constructors

        #region Parameterless Constructor

        public AWBRecord() { }

        #endregion

        #region Parameterized Constructor
        
        public AWBRecord(InvoiceRecordBase invoiceRecordBase)
            : base(invoiceRecordBase)
        {
            BillingCode = invoiceRecordBase.BillingCode;
        }

        #endregion

        #endregion

        #region Implementation of IClassToRecordConverter<AWBRecord> To Write IDEC File.

        /// <summary>
        /// This method converts the AirWayBill calss into AWBRecord.
        /// </summary>
        /// <param name="airWayBill">AirWayBill</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ConvertClassToRecord(AirWayBill airWayBill, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting AirWayBill into AWBRecord.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiAwbRecord;
            BillingCode = airWayBill.BillingCode;

            BatchSequenceNumber = airWayBill.BatchSequenceNumber.ToString();
            RecordSequenceWithinBatch = airWayBill.RecordSequenceWithinBatch.ToString();
            AWBIssueDate = GetFormattedDate(airWayBill.AWBDate);
            AWBIssuingAirline = Utilities.GetNumericMemberCode(airWayBill.AWBIssuingAirline).ToString();
            AWBSerialNumber = airWayBill.AWBSerialNumber.ToString();
            AWBCheckDigit = airWayBill.AWBCheckDigit.ToString();
            Origin = airWayBill.Origin;
            Destination = airWayBill.Destination;
            FromAirport = airWayBill.From;
            ToAirport = airWayBill.To;
            DateOfCarriage = airWayBill.DateOfCarriageOrTransfer;
            WeightCharges = airWayBill.WeightCharges.HasValue ? Math.Abs(airWayBill.WeightCharges.Value).ToString() : null;
            OtherCharges = Math.Abs(airWayBill.OtherCharges).ToString();
            AmountSubjectToInterlineServiceCharges = Math.Abs(airWayBill.AmountSubjectToInterlineServiceCharge).ToString();
            AmountSubjectToInterlineServiceChargesSign = Utilities.GetSignValue(airWayBill.AmountSubjectToInterlineServiceCharge);
            InterlineServiceChargePercent = Math.Abs(airWayBill.InterlineServiceChargePercentage).ToString();
            InterlineServiceChargeRateSign = Utilities.GetSignValue(airWayBill.InterlineServiceChargePercentage);
            CurrencyAdjustmentIndicator = airWayBill.CurrencyAdjustmentIndicator;
            BilledWeight = airWayBill.BilledWeight.HasValue ? airWayBill.BilledWeight.Value.ToString() : null;
            ProvisoOrReqOrSPA = airWayBill.ProvisoReqSPA;
            ProratePercent = airWayBill.ProratePercentage.HasValue ? airWayBill.ProratePercentage.Value.ToString() : null;
            PartShipmentIndicator = airWayBill.PartShipmentIndicator;
            FillingReference = airWayBill.FilingReference;
            WeightChargesSign = Utilities.GetSignValue(airWayBill.WeightCharges);
            OtherChargesSign = Utilities.GetSignValue(airWayBill.OtherCharges);
            ValuationCharges = airWayBill.ValuationCharges.HasValue ? Math.Abs(airWayBill.ValuationCharges.Value).ToString() : null;
            ValuationChargesSign = Utilities.GetSignValue(airWayBill.ValuationCharges);
            KgOrLBIndicator = airWayBill.KGLBIndicator;
            VATAmount = airWayBill.VATAmount.HasValue ? Math.Abs(airWayBill.VATAmount.Value).ToString() : null;
            VATAmountSign = Utilities.GetSignValue(airWayBill.VATAmount);
            InterlineServiceChargeAmount = Math.Abs(airWayBill.InterlineServiceChargeAmount).ToString();
            InterlineServiceChargeAmountSign = Utilities.GetSignValue(airWayBill.InterlineServiceChargeAmount);
            AWBTotalAmount = airWayBill.AWBTotalAmount.HasValue ? Math.Abs(airWayBill.AWBTotalAmount.Value).ToString() : null;
            AWBTotalAmountSign = Utilities.GetSignValue(airWayBill.AWBTotalAmount);
            CCAIndicator = Utilities.GetBooDisplaylValue(airWayBill.CCAindicator);
            OurRef = airWayBill.OurReference;
            AttachmentIndicatorOriginal = airWayBill.AttachmentIndicatorOriginal;
            AttachmentIndicatorValidated = airWayBill.AttachmentIndicatorValidated;
            NumberOfAttachments = airWayBill.NumberOfAttachments.HasValue ? airWayBill.NumberOfAttachments.Value.ToString() : "0";
            // This field is an output only field and must be blank in case of an input file to SIS.
            ISValidationFlag = string.Empty; // airWayBill.ISValidationFlag;
            ReasonCode = airWayBill.ReasonCode;
            ReferenceField1 = airWayBill.ReferenceField1;
            ReferenceField2 = airWayBill.ReferenceField2;
            ReferenceField3 = airWayBill.ReferenceField3;
            ReferenceField4 = airWayBill.ReferenceField4;
            ReferenceField5 = airWayBill.ReferenceField5;
            AirlineOwnUse = airWayBill.AirlineOwnUse;

            ProcessNextClass(airWayBill, ref recordSequenceNumber);

            //Logger.Info("End of Converting AirWayBill into AWBRecord.");
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

                yearStr = yearStr.PadLeft(4, '0');

                // This is case when year is passed as 4 digit number as 2009.
                if (yearStr.Length == 4)
                {
                    yearStr = yearStr.Substring(2, 2);
                }
                return string.Format("{0}{1}{2}", yearStr.PadLeft(2, '0'), monthStr.PadLeft(2, '0'), dayStr.PadLeft(2, '0'));
            }
            return string.Empty;
        }

        /// <summary>
        /// Convert Child of AirWayBill Class into there corresponding Records.
        /// </summary>
        /// <param name="airWayBill">AirWayBill</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ProcessNextClass(AirWayBill airWayBill, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting Childs of AirWayBill Class into there corresponding Records.");

            if (airWayBill != null)
            {
                if (airWayBill.AWBVATList.Count > 0)
                {
                    foreach (var aWBVATList in Utilities.GetDividedSubCollections(airWayBill.AWBVATList, 2))
                    {
                        var aWBVatBreakdownRecord = new AWBVatBreakdownRecord(this);
                        aWBVatBreakdownRecord.ConvertClassToRecord(aWBVATList, ref recordSequenceNumber);
                        AWBVatBreakdownRecordList.Add(aWBVatBreakdownRecord);
                    }
                }
                if (airWayBill.AWBOtherChargesList.Count > 0)
                {
                    foreach (var aWBOtherChargesList in Utilities.GetDividedSubCollections(airWayBill.AWBOtherChargesList, 3))
                    {
                        var aWBOCBreakdownRecord = new AWBOCBreakdownRecord(this);
                        aWBOCBreakdownRecord.ConvertClassToRecord(aWBOtherChargesList, ref recordSequenceNumber);
                        AWBOCBreakdownRecordList.Add(aWBOCBreakdownRecord);
                    }
                }
            }

            //Logger.Info("End of Converting Childs of AirWayBill Class into there corresponding Records.");
        }

        #endregion

        #region Implementation of IRecordToClassConverter<AirWayBill> To Read IDEC File.

        /// <summary>
        /// To Convert AWBRecord into AirWayBill.
        /// </summary>
        /// <param name="multiRecordEngine">multiRecordEngine</param>
        /// <returns>AirWayBill</returns>
        public AirWayBill ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Info("Start of Converting AWBRecord into AirWayBill.");
         
            var airWayBill = CreateAirWayBill();

            ProcessNextRecord(multiRecordEngine, airWayBill);

            //Logger.Info("End of Converting AWBRecord into AirWayBill.");

            return airWayBill;
        }

        /// <summary>
        /// Creates AirWayBill for AWBRecord present in the reading input IDEC File.
        /// </summary>
        /// <returns>AirWayBill</returns>
        private AirWayBill CreateAirWayBill()
        {
            //Logger.Info("Start of Creating AirWayBill Class from AWBRecord.");

            var airWayBill = new AirWayBill
            {
                BatchSequenceNumber = int.Parse(BatchSequenceNumber),
                RecordSequenceWithinBatch = int.Parse(RecordSequenceWithinBatch),
                AWBIssuingAirline = AWBIssuingAirline,
                AWBSerialNumber = Convert.ToInt32(AWBSerialNumber),
                AWBCheckDigit = int.Parse(AWBCheckDigit),
                Origin = Origin.Trim(),
                Destination = Destination.Trim(),
                From = FromAirport.Trim(),
                To = ToAirport.Trim(),
                DateOfCarriageOrTransfer = DateOfCarriage,
                WeightCharges = Utilities.GetActualValueForDouble(WeightChargesSign, WeightCharges),
                OtherCharges = Utilities.GetActualValueForDouble(OtherChargesSign, OtherCharges),
                AmountSubjectToInterlineServiceCharge = Utilities.GetActualValueForDouble(AmountSubjectToInterlineServiceChargesSign, AmountSubjectToInterlineServiceCharges),
                InterlineServiceChargePercentage = Utilities.GetActualValueForDouble(InterlineServiceChargeRateSign, InterlineServiceChargePercent),
                CurrencyAdjustmentIndicator = CurrencyAdjustmentIndicator.Trim(),
                BilledWeight = Convert.ToInt32(BilledWeight),
                ProvisoReqSPA = ProvisoOrReqOrSPA,
                ProratePercentage = Convert.ToInt32(ProratePercent),
                PartShipmentIndicator = PartShipmentIndicator,
                FilingReference = FillingReference,
                ValuationCharges = Utilities.GetActualValueForDouble(ValuationChargesSign, ValuationCharges),
                KGLBIndicator = KgOrLBIndicator,
                VATAmount = Utilities.GetActualValueForDouble(VATAmountSign, VATAmount),
                InterlineServiceChargeAmount = Utilities.GetActualValueForDouble(InterlineServiceChargeAmountSign, InterlineServiceChargeAmount),
                AWBTotalAmount = Utilities.GetActualValueForDouble(AWBTotalAmountSign, AWBTotalAmount),
                CCAindicator = CCAIndicator == IdecConstants.TrueValue,
                OurReference = OurRef,
                ReasonCode = ReasonCode,
                ReferenceField1 = ReferenceField1,
                ReferenceField2 = ReferenceField2,
                ReferenceField3 = ReferenceField3,
                ReferenceField4 = ReferenceField4,
                ReferenceField5 = ReferenceField5,
                AirlineOwnUse = AirlineOwnUse,
                BillingCode = BillingCode
            };

            // Invoice Date
            DateTime awbDate;

            // To avoid converting year 30 into year 1930
            var cultureInfo = new CultureInfo("en-US");
            cultureInfo.Calendar.TwoDigitYearMax = 2099;

            if (DateTime.TryParseExact(AWBIssueDate, IdecConstants.InvoiceDateFormat, cultureInfo, DateTimeStyles.None, out awbDate))
            {
                airWayBill.AWBDate = awbDate;
            }

            if (AttachmentIndicatorOriginal.Equals("Y"))
            {
                airWayBill.AttachmentIndicatorOriginal = "Y";
            }

            //Logger.Info("End of Creating AirWayBill Class from AWBRecord.");

            return airWayBill;
        }

        /// <summary>
        /// To Convert Child records of AWBRecord into there corresponding Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="airWayBill">AirWayBill</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, AirWayBill airWayBill)
        {
            //Logger.Info("Start of Converting Childs of AWBRecord into there corresponding Classes.");

            multiRecordEngine.ReadNext();

            var airWayBillRecord = airWayBill;
            do
            {
                if (multiRecordEngine.LastRecord is VatRecordBase)
                {
                    airWayBill.NumberOfChildRecords += 1;

                    var aWBVATList = ((IRecordToClassConverter<List<AWBVAT>>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    var aWBVATListCount = aWBVATList.Count;
                    for (var i = 0; i < aWBVATListCount; i++)
                    {
                        airWayBillRecord.AWBVATList.Add(aWBVATList[i]);
                    }
                }
                else if (multiRecordEngine.LastRecord is AWBOCBreakdownRecord)
                {
                    airWayBill.NumberOfChildRecords += 1;

                    var aWBOtherChargeseList = ((IRecordToClassConverter<List<AWBOtherCharges>>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    var aWBOtherChargeseListCount = aWBOtherChargeseList.Count;
                    for (var i = 0; i < aWBOtherChargeseListCount; i++)
                    {
                        airWayBillRecord.AWBOtherChargesList.Add(aWBOtherChargeseList[i]);
                    }
                }
                else
                {
                    break;
                }
            }
            while (multiRecordEngine.LastRecord != null);

            //Logger.Info("End of Converting Childs of AWBRecord into there corresponding Classes.");
        }

        #endregion

    }
}