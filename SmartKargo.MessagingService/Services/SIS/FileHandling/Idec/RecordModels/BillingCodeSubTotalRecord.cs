using System;
using System.Collections.Generic;
using System.Reflection;
using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.Model;

using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.Write;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    [FixedLengthRecord()]
    public class BillingCodeSubTotalRecord : InvoiceRecordBase, IClassToRecordConverter<BillingCodeSubTotal>, IRecordToClassConverter<BillingCodeSubTotal>
    {
        
        #region Record Properties

        [FieldFixedLength(5), FieldConverter(typeof(PaddingConverter), 5, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string BatchSequenceNumber;

        [FieldFixedLength(5), FieldConverter(typeof(PaddingConverter), 5, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string RecordSequenceWithinBatch;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalWeightCharges;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalOtherCharges;

        [FieldFixedLength(1)]
        public string TotalInterlineServiceChargeSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalInterlineServiceCharge;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string BillingCodeSubTotal;

        [FieldFixedLength(15)]
        public string Filler2;

        [FieldFixedLength(6), FieldConverter(typeof(PaddingConverter), 6, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string TotalnumberOfBillingRecords;

        [FieldFixedLength(24)]
        public string Filler3;

        [FieldFixedLength(8)]
        public string Filler4;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalValuationCharges;

        [FieldFixedLength(1)]
        public string TotalValuationChargesSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalVATAmount;

        [FieldFixedLength(1)]
        public string TotalVATAmountSign;

        [FieldFixedLength(1)]
        public string TotalWeightChargesSign;

        [FieldFixedLength(1)]
        public string TotalOtherChargesSign;

        [FieldFixedLength(1)]
        public string BillingCodeSubTotalSign;

        [FieldFixedLength(8), FieldConverter(typeof(PaddingConverter), 8, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string TotalNumberOfRecords;

        [FieldFixedLength(100)]
        public string BillingCodeSubTotalDescription;

        [FieldFixedLength(197)]
        public string Filler5;

        [FieldHidden]
        public List<BillingCodeSubTotalVatRecord> BillingCodeSubTotalVatRecordList = new List<BillingCodeSubTotalVatRecord>();

        #endregion

        #region Constructors

        #region Parameterless Constructor

        public BillingCodeSubTotalRecord() { }

        #endregion

        #region Parameterized Constructor

        public BillingCodeSubTotalRecord(InvoiceRecordBase invoiceRecordBase)
            : base(invoiceRecordBase)
        { }

        #endregion

        #endregion

        #region Implementation of IClassToRecordConverter<BillingCodeSubTotal> To Write IDEC File.

        /// <summary>
        /// To Convert BillingCodeSubTotal into BillingCodeSubTotalRecord.
        /// </summary>
        /// <param name="billingCodeSubTotal">BillingCodeSubTotal</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ConvertClassToRecord(BillingCodeSubTotal billingCodeSubTotal, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting BillingCodeSubTotal into BillingCodeSubTotalRecord.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiBillingCodeSubTotal;
            BillingCode = billingCodeSubTotal.BillingCode;
            BatchSequenceNumber = IdecConstants.SourceCodeTotalBatchSequenceNumber;
            RecordSequenceWithinBatch = IdecConstants.SourceCodeTotalRecordSequencewithinBatch;
            TotalWeightCharges = Math.Abs(billingCodeSubTotal.TotalWeightCharge).ToString();
            TotalOtherCharges = Math.Abs(billingCodeSubTotal.TotalOtherCharge).ToString();
            TotalInterlineServiceChargeSign = Utilities.GetSignValue(billingCodeSubTotal.TotalIscAmount);
            TotalInterlineServiceCharge = Math.Abs(billingCodeSubTotal.TotalIscAmount).ToString();
            BillingCodeSubTotal = Math.Abs(billingCodeSubTotal.BillingCodeSbTotal).ToString();
            TotalnumberOfBillingRecords = billingCodeSubTotal.NumberOfBillingRecords.ToString();
            TotalValuationCharges = Math.Abs(billingCodeSubTotal.TotalValuationCharge).ToString();
            TotalValuationChargesSign = Utilities.GetSignValue(billingCodeSubTotal.TotalValuationCharge);
            TotalVATAmount = Math.Abs(billingCodeSubTotal.TotalVatAmount).ToString();
            TotalVATAmountSign = Utilities.GetSignValue(billingCodeSubTotal.TotalVatAmount);
            TotalWeightChargesSign = Utilities.GetSignValue(billingCodeSubTotal.TotalWeightCharge);
            TotalOtherChargesSign = Utilities.GetSignValue(billingCodeSubTotal.TotalOtherCharge);
            BillingCodeSubTotalSign = Utilities.GetSignValue(billingCodeSubTotal.BillingCodeSbTotal);
            TotalNumberOfRecords = billingCodeSubTotal.TotalNumberOfRecords.ToString();
            BillingCodeSubTotalDescription = billingCodeSubTotal.BillingCode;
            Filler2 = IdecConstants.BillingCodeTotalFiller;

            ProcessNextClass(billingCodeSubTotal, ref recordSequenceNumber);

            //Logger.Info("End of Converting BillingCodeSubTotal into BillingCodeSubTotalRecord.");
        }

        /// <summary>
        /// To Convert Childs of BillingCodeSubTotal into there corresponding Records.
        /// </summary>
        /// <param name="billingCodeSubTotal">BillingCodeSubTotal</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ProcessNextClass(BillingCodeSubTotal billingCodeSubTotal, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting Childs of BillingCodeSubTotal Class into there corresponding Records.");

            if (billingCodeSubTotal != null)
            {
                if (billingCodeSubTotal.BillingCodeSubTotalVATList != null && billingCodeSubTotal.BillingCodeSubTotalVATList.Count > 0)
                {
                    foreach (var billingCodeSubTotalVATList in Utilities.GetDividedSubCollections(billingCodeSubTotal.BillingCodeSubTotalVATList, 2))
                    {
                        var billingCodeSubTotalVatRecord = new BillingCodeSubTotalVatRecord(this);
                        billingCodeSubTotalVatRecord.ConvertClassToRecord(billingCodeSubTotalVATList, ref recordSequenceNumber);
                        BillingCodeSubTotalVatRecordList.Add(billingCodeSubTotalVatRecord);
                    }
                }
            }

            //Logger.Info("End of Converting Childs of BillingCodeSubTotal Class into there corresponding Records.");
        }

        #endregion

        #region Implementation of IRecordToClassConverter<BillingCodeSubTotal> To Read IDEC File.

        /// <summary>
        /// To Convert BillingCodeSubTotalRecord into BillingCodeSubTotal.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <returns></returns>
        public BillingCodeSubTotal ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Info("Start of Converting BillingCodeSubTotalRecord into BillingCodeSubTotal.");

            var billingCodeSubTotal = CreateBillingCodeSubTotal();

            ProcessNextRecord(multiRecordEngine, billingCodeSubTotal);

            //Logger.Info("End of Converting BillingCodeSubTotalRecord into BillingCodeSubTotal.");

            return billingCodeSubTotal;
        }

        /// <summary>
        /// Creates BillingCodeSubTotal for BillingCodeSubTotalRecord present in the reading input IDEC File.
        /// </summary>
        /// <returns>BillingCodeSubTotal</returns>
        private BillingCodeSubTotal CreateBillingCodeSubTotal()
        {
            //Logger.Info("Start of Creating BillingCodeSubTotal Class from BillingCodeSubTotalRecord.");

            var billingCodeSubTotal = new BillingCodeSubTotal
            {
                BillingCode = BillingCode,
                TotalWeightCharge = Utilities.GetActualValueForDecimal(TotalWeightChargesSign, TotalWeightCharges),
                TotalOtherCharge = Utilities.GetActualValueForDecimal(TotalOtherChargesSign, TotalOtherCharges),
                TotalIscAmount = Utilities.GetActualValueForDecimal(TotalInterlineServiceChargeSign, TotalInterlineServiceCharge),
                BillingCodeSbTotal = Utilities.GetActualValueForDecimal(BillingCodeSubTotalSign, BillingCodeSubTotal),
                NumberOfBillingRecords = Convert.ToInt32(TotalnumberOfBillingRecords),
                TotalValuationCharge = Utilities.GetActualValueForDecimal(TotalValuationChargesSign, TotalValuationCharges),
                TotalVatAmount = Utilities.GetActualValueForDecimal(TotalVATAmountSign, TotalVATAmount),
                TotalNumberOfRecords = Convert.ToInt32(TotalNumberOfRecords),
                BillingCodeSubTotalDesc = BillingCodeSubTotalDescription
            };

            //Logger.Info("End of Creating BillingCodeSubTotal Class from BillingCodeSubTotalRecord.");

            return billingCodeSubTotal;
        }

        /// <summary>
        /// To Convert Child records of BillingCodeSubTotalRecord into there corresponding Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="billingCodeSubTotal">BillingCodeSubTotal</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, BillingCodeSubTotal billingCodeSubTotal)
        {
            //Logger.Info("Start of Converting Childs of BillingCodeSubTotalRecord into there corresponding Classes.");

            multiRecordEngine.ReadNext();

            do
            {
                if (multiRecordEngine.LastRecord is VatRecordBase)
                {
                    var billingCodeSubTotalVATList = ((IRecordToClassConverter<List<BillingCodeSubTotalVAT>>)multiRecordEngine.LastRecord).ConvertRecordToClass(multiRecordEngine);

                    var noOfbillingCodeSubTotalVAT = billingCodeSubTotalVATList.Count;

                    if (noOfbillingCodeSubTotalVAT > 0)
                    {
                        billingCodeSubTotal.NumberOfChildRecords += 1;
                    }

                    for (var i = 0; i < noOfbillingCodeSubTotalVAT; i++)
                    {
                        billingCodeSubTotal.BillingCodeSubTotalVATList.Add(billingCodeSubTotalVATList[i]);
                    }
                }
                else
                {
                    break;
                }

            } while (multiRecordEngine.LastRecord != null);

            //Logger.Info("End of Converting Childs of BillingCodeSubTotalRecord into there corresponding Classes.");
        }

        #endregion

    }
}