using System;
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
    public class InvoiceTotalRecord : InvoiceRecordBase, IClassToRecordConverter<InvoiceTotal>, IRecordToClassConverter<InvoiceTotal>
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
        public string TotalInterlineServiceChargeAmountSign;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalInterlineServiceChargeAmount;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string NetInvoiceTotal;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string NetInvoiceBillingTotal;

        [FieldFixedLength(6), FieldConverter(typeof(PaddingConverter), 6, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string TotalNumberOfBillingRecords;

        [FieldFixedLength(24)]
        public string Filler2;

        [FieldFixedLength(8)]
        public string Filler3;

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
        public string NetInvoiceTotalSign;

        [FieldFixedLength(1)]
        public string NetInvoiceBillingTotalSign;

        [FieldFixedLength(8), FieldConverter(typeof(PaddingConverter), 8, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string TotalNumberOfRecords;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalNetAmountWithoutVAT;

        [FieldFixedLength(1)]
        public string TotalNetAmountWithoutVATSign;

        [FieldFixedLength(280)]
        public string Filler4;

        #endregion

        #region Constructors

        #region Parameterless Constructor

        public InvoiceTotalRecord() { }

        #endregion

        #region Parameterized Constructor

        public InvoiceTotalRecord(InvoiceRecordBase baseRecord)
            : base(baseRecord)
        {
            BillingCode = IdecConstants.FileTotalRecordBillingCode;
        }

        #endregion

        #endregion

        #region Implementation of IClassToRecordConverter<InvoiceTotalRecord> To Write IDEC File.

        /// <summary>
        /// This method converts the InvoiceTotal calss into InvoiceTotalRecord.
        /// </summary>
        /// <param name="invoiceTotal">InvoiceTotal</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ConvertClassToRecord(InvoiceTotal invoiceTotal, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting InvoiceTotal into InvoiceTotalRecord.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiInvoiceTotalRecord;
            BatchSequenceNumber = IdecConstants.InvoiceTotalRecordBatchSequenceNumber;
            RecordSequenceWithinBatch = IdecConstants.InvoiceTotalRecordSequencewithinBatch;
            TotalWeightCharges = Math.Abs(invoiceTotal.TotalWeightCharges).ToString();
            TotalOtherCharges = Math.Abs(invoiceTotal.TotalOtherCharges).ToString();
            TotalInterlineServiceChargeAmountSign = Utilities.GetSignValue(invoiceTotal.TotalInterlineServiceChargeAmount);
            TotalInterlineServiceChargeAmount = Math.Abs(invoiceTotal.TotalInterlineServiceChargeAmount).ToString();
            NetInvoiceTotal = Math.Abs(invoiceTotal.NetInvoiceTotal).ToString();
            NetInvoiceBillingTotal = Math.Abs(invoiceTotal.NetInvoiceBillingTotal).ToString();
            TotalNumberOfBillingRecords = invoiceTotal.TotalNumberOfBillingRecords.ToString();
            TotalValuationCharges = Math.Abs(invoiceTotal.TotalValuationCharges).ToString();
            TotalValuationChargesSign = Utilities.GetSignValue(invoiceTotal.TotalValuationCharges);
            TotalVATAmount = Math.Abs(invoiceTotal.TotalVATAmount).ToString();
            TotalVATAmountSign = Utilities.GetSignValue(invoiceTotal.TotalVATAmount);
            TotalWeightChargesSign = Utilities.GetSignValue(invoiceTotal.TotalWeightCharges);
            TotalOtherChargesSign = Utilities.GetSignValue(invoiceTotal.TotalOtherCharges);
            NetInvoiceTotalSign = Utilities.GetSignValue(invoiceTotal.NetInvoiceTotal);
            NetInvoiceBillingTotalSign = Utilities.GetSignValue(invoiceTotal.NetInvoiceBillingTotal);
            TotalNumberOfRecords = invoiceTotal.TotalNumberOfRecords.ToString();
            TotalNetAmountWithoutVAT = Math.Abs(invoiceTotal.TotalNetAmountWithoutVat).ToString();
            TotalNetAmountWithoutVATSign = Utilities.GetSignValue(invoiceTotal.TotalNetAmountWithoutVat);

            //Logger.Info("End of Converting InvoiceTotal into InvoiceTotalRecord.");
        }

        /// <summary>
        /// To implement the interface.
        /// </summary>
        /// <param name="invoiceTotal">InvoiceTotal</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ProcessNextClass(InvoiceTotal invoiceTotal, ref long recordSequenceNumber)
        {
            //Logger.Info("InvoiceTotal Class does not have childs.");
        }

        #endregion

        #region Implementation of IRecordToClassConverter<InvoiceTotal> To Read IDEC File.

        /// <summary>
        /// To Convert InvoiceTotalRecord into InvoiceTotal.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <returns>InvoiceTotal</returns>
        public InvoiceTotal ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Info("Start of Converting InvoiceTotalRecord into InvoiceTotal.");

            var invoiceTotal = CreateInvoiceTotal();

            ProcessNextRecord(multiRecordEngine, invoiceTotal);

            //Logger.Info("End of Converting InvoiceTotalRecord into InvoiceTotal.");

            return invoiceTotal;
        }

        /// <summary>
        /// Creates InvoiceTotal for InvoiceTotalRecord present in the reading input IDEC File.
        /// </summary>
        /// <returns></returns>
        private InvoiceTotal CreateInvoiceTotal()
        {
            //Logger.Info("Start of Creating InvoiceTotal Class from InvoiceTotalRecord.");

            var invoiceTotal = new InvoiceTotal
            {
                TotalWeightCharges = Utilities.GetActualValueForDecimal(TotalWeightChargesSign, TotalWeightCharges),
                TotalOtherCharges = Utilities.GetActualValueForDecimal(TotalOtherChargesSign, TotalOtherCharges),
                TotalInterlineServiceChargeAmount = Utilities.GetActualValueForDecimal(TotalInterlineServiceChargeAmountSign, TotalInterlineServiceChargeAmount),
                NetInvoiceTotal = Utilities.GetActualValueForDecimal(NetInvoiceTotalSign, NetInvoiceTotal),
                NetInvoiceBillingTotal = Utilities.GetActualValueForDecimal(NetInvoiceBillingTotalSign, NetInvoiceBillingTotal),
                TotalValuationCharges = Utilities.GetActualValueForDecimal(TotalValuationChargesSign, TotalValuationCharges),
                TotalVATAmount = Utilities.GetActualValueForDecimal(TotalVATAmountSign, TotalVATAmount),
                TotalNetAmountWithoutVat = Utilities.GetActualValueForDecimal(TotalNetAmountWithoutVATSign, TotalNetAmountWithoutVAT),
                TotalNumberOfBillingRecords = Convert.ToInt32(TotalNumberOfBillingRecords),
                TotalNumberOfRecords = Convert.ToInt32(TotalNumberOfRecords)
            };

            //Logger.Info("End of Creating InvoiceTotal Class from InvoiceTotalRecord.");

            return invoiceTotal;
        }

        /// <summary>
        /// To Convert Child records of InvoiceTotalRecord into there corresponding Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="invoiceTotal">InvoiceTotal</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, InvoiceTotal invoiceTotal)
        {
            //Logger.Info("Start of Converting Childs of InvoiceTotalRecord into there corresponding Classes.");

            multiRecordEngine.ReadNext();

            //Logger.Info("End of Converting Childs of InvoiceTotalRecord into there corresponding Classes.");
        }

        #endregion

    }
}