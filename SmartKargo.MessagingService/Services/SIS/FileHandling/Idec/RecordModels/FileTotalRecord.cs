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
    [FixedLengthRecord]
    public class FileTotalRecord : InvoiceRecordBase, IClassToRecordConverter<FileTotal>, IRecordToClassConverter<FileTotal>
    {
        
        #region Record Properties

        [FieldFixedLength(5), FieldConverter(typeof(PaddingConverter), 5, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter)]
        public string BatchSequenceNumber;

        [FieldFixedLength(5), FieldConverter(typeof(PaddingConverter), 5, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter)]
        public string RecordSequenceWithinBatch;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalWeightCharges;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalOtherCharges;

        [FieldFixedLength(1)]
        public string Filler2;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalInterlineServiceChargeAmount;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string FileTotalOfNetInvoiceTotal;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string FileTotalOfNetInvoiceBillingTotal;

        [FieldFixedLength(6), FieldConverter(typeof(PaddingConverter), 6, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter)]
        public string TotalNumberOfBillingRecords;

        [FieldFixedLength(24)]
        public string Filler3;

        [FieldFixedLength(8)]
        public string Filler4;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalValuationCharges;

        [FieldFixedLength(1)]
        public string Filler5;

        [FieldFixedLength(15), FieldConverter(typeof(DoubleNumberConverter), 15, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string TotalVATAmount;

        [FieldFixedLength(5)]
        public string Filler6;

        [FieldFixedLength(8), FieldConverter(typeof(PaddingConverter), 8, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter)]
        public string TotalNumberOfRecords;

        [FieldFixedLength(296)]
        public string Filler7;

        #endregion

        #region Parameterless Constructor

        public FileTotalRecord()
        {
            StandardMessageIdentifier = IdecConstants.StandardMessageIdentifier;
            BilledAirlineCode = IdecConstants.FileTotalRecordBilledAirlineCode;
            BillingCode = IdecConstants.FileTotalRecordBillingCode;
            InvoiceNumber = IdecConstants.FileTotalRecordInvoiceNumber;
            Filler1 = IdecConstants.FileTotalRecordFiller;
        }

        #endregion

        #region Implementation of IClassToRecordConverter<FileTotalRecord> To Write IDEC File.

        /// <summary>
        /// This method converts the FileTotal class into FileTotalRecord.
        /// </summary>
        /// <param name="fileTotal">FileTotal</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ConvertClassToRecord(FileTotal fileTotal, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting FileTotal into FileTotalRecord.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiFileTotalRecord;

            BillingAirlineCode = fileTotal.BillingAirline != null ? FileHandling.Idec.Utilities.GetNumericMemberCode(fileTotal.BillingAirline).ToString() : string.Empty; 
            BatchSequenceNumber = IdecConstants.FileTotalRecordBatchSequenceNumber;
            RecordSequenceWithinBatch = IdecConstants.FileTotalRecordSequencewithinBatch;
            TotalWeightCharges = fileTotal.TotalWeightCharges.ToString();
            TotalOtherCharges = fileTotal.TotalOtherCharges.ToString();
            TotalInterlineServiceChargeAmount = fileTotal.TotalInterlineServiceChargeAmount.ToString();
            FileTotalOfNetInvoiceTotal = fileTotal.FileTotalOfNetInvoiceTotal.ToString();
            FileTotalOfNetInvoiceBillingTotal = fileTotal.FileTotalOfNetInvoiceBillingTotal.ToString();
            TotalNumberOfBillingRecords = fileTotal.TotalNumberOfBillingRecords.ToString();
            TotalValuationCharges = fileTotal.TotalValuationCharges.ToString();
            TotalVATAmount = fileTotal.TotalVatAmount.ToString();
            TotalNumberOfRecords = RecordSequenceNumber + 2;

            //Logger.Info("End of Converting FileTotal into FileTotalRecord.");
        }

        /// <summary>
        /// To implement the interface.
        /// </summary>
        /// <param name="fileTotal">FileTotal</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ProcessNextClass(FileTotal fileTotal, ref long recordSequenceNumber)
        {
            //Logger.Info("FileTotal Class does not have childs.");
        }

        #endregion

        #region Implementation of IRecordToClassConverter<FileTotal> To Read IDEC File.

        /// <summary>
        /// To Convert FileTotalRecord into FileTotal.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <returns>FileTotal</returns>
        public FileTotal ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Info("Start of Converting FileTotalRecord into FileTotal.");

            var fileTotal = CreateFileTotal();

            ProcessNextRecord(multiRecordEngine, fileTotal);

            //Logger.Info("End of Converting FileTotalRecord into FileTotal.");

            return fileTotal;
        }

        /// <summary>
        /// Creates FileTotal for FileTotalRecord present in the reading input IDEC File.
        /// </summary>
        /// <returns>FileTotal</returns>
        private FileTotal CreateFileTotal()
        {
            //Logger.Info("Start of Creating FileTotal Class from FileTotalRecord.");

            var fileTotal = new FileTotal()
            {
                BatchSequenceNumber = Convert.ToInt32(BatchSequenceNumber),
                RecordSequenceWithinBatch = Convert.ToInt32(RecordSequenceWithinBatch),
                TotalInterlineServiceChargeAmount = Convert.ToDecimal(TotalInterlineServiceChargeAmount),
                TotalVatAmount = Convert.ToDecimal(TotalVATAmount),
                TotalWeightCharges = Convert.ToDecimal(TotalWeightCharges),
                TotalOtherCharges = Convert.ToDecimal(TotalOtherCharges),
                TotalValuationCharges = Convert.ToDecimal(TotalValuationCharges),
                FileTotalOfNetInvoiceTotal = Convert.ToDecimal(FileTotalOfNetInvoiceTotal),
                FileTotalOfNetInvoiceBillingTotal = Convert.ToDecimal(FileTotalOfNetInvoiceBillingTotal),
                TotalNumberOfBillingRecords = Convert.ToDecimal(TotalNumberOfBillingRecords),
                TotalNumberOfRecords = Convert.ToDecimal(TotalNumberOfRecords),
            };

            //Logger.Info("End of Creating FileTotal Class from FileTotalRecord.");

            return fileTotal;
        }

        /// <summary>
        /// To Convert Child records of FileTotalRecord into there corresponding Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="fileTotal">FileTotal</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, FileTotal fileTotal)
        {
            //Logger.Info("Start of Converting Childs of FileTotalRecord into there corresponding Classes.");

            multiRecordEngine.ReadNext();

            //Logger.Info("End of Converting Childs of FileTotalRecord into there corresponding Classes.");
        }

        #endregion      

    }
}