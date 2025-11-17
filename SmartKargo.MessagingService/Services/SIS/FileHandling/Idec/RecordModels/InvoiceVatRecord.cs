using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.Write;
using QidWorkerRole.SIS.Model;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    public class InvoiceVatRecord : VatRecordBase, IClassToRecordConverter<List<InvoiceTotalVAT>>, IRecordToClassConverter<List<InvoiceTotalVAT>>
    {
        
        #region Record Properties

        [FieldFixedLength(11)]
        public string Filler3;

        [FieldFixedLength(4)]
        public string Filler4;

        [FieldFixedLength(7)]
        public string Filler5;

        [FieldFixedLength(1)]
        public string Filler6;

        [FieldFixedLength(50)]
        public string Filler7;

        [FieldFixedLength(2)]
        public string VatIdentifier1;

        [FieldFixedLength(5)]
        public string VatLabel1;

        [FieldFixedLength(50)]
        public string VatText1;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatBaseAmount1;

        [FieldFixedLength(1)]
        public string VatBaseAmountSign1;

        [FieldFixedLength(5), FieldConverter(typeof(DoubleNumberConverter), 5, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatPercentage1;

        [FieldFixedLength(1)]
        public string VatPercentageSign1;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatCalculatedAmount1;

        [FieldFixedLength(1)]
        public string VatCalculatedAmountSign1;

        [FieldFixedLength(50)]
        public string Filler8;

        [FieldFixedLength(2)]
        public string VatIdentifier2;

        [FieldFixedLength(5)]
        public string VatLabel2;

        [FieldFixedLength(50)]
        public string VatText2;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatBaseAmount2;

        [FieldFixedLength(1)]
        public string VatBaseAmountSign2;

        [FieldFixedLength(5), FieldConverter(typeof(DoubleNumberConverter), 5, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatPercentage2;

        [FieldFixedLength(1)]
        public string VatPercentageSign2;

        [FieldFixedLength(11), FieldConverter(typeof(DoubleNumberConverter), 11, 3, IdecConstants.PaddingCharacterZero), FieldNullValue("0")]
        public string VatCalculatedAmount2;

        [FieldFixedLength(1)]
        public string VatCalculatedAmountSign2;

        [FieldFixedLength(167)]
        public string Filler9;

        #endregion

        #region Constructors

        #region Parameterless Constructor

        public InvoiceVatRecord() { }

        #endregion

        #region Parameterized Constructor

        public InvoiceVatRecord(InvoiceRecordBase invoiceRecordBase)
            : base(invoiceRecordBase)
        {
        }

        #endregion

        #endregion

        #region Implementation of IClassToRecordConverter<List<InvoiceTotalVAT>> To Write IDEC File.

        /// <summary>
        /// This method converts the InvoiceTotalVAT calsses into InvoiceVatRecord.
        /// </summary>
        /// <param name="invoiceTotalVATList">List of InvoiceTotalVAT</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ConvertClassToRecord(List<InvoiceTotalVAT> invoiceTotalVATList, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting InvoiceTotalVAT into InvoiceVatRecord.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiVatRecord;
            BillingCode = IdecConstants.FileTotalRecordBillingCode;

            if (invoiceTotalVATList.Count > 0)
            {
                VatIdentifier1 = invoiceTotalVATList[0].VatIdentifier != null ? invoiceTotalVATList[0].VatIdentifier : "NL";
                VatLabel1 = invoiceTotalVATList[0].VatLabel;
                VatText1 = invoiceTotalVATList[0].VatText;
                VatBaseAmount1 = invoiceTotalVATList[0].VatBaseAmount.ToString();
                VatBaseAmountSign1 = Utilities.GetSignValue(invoiceTotalVATList[0].VatBaseAmount);
                VatPercentage1 = invoiceTotalVATList[0].VatPercentage.ToString();
                VatPercentageSign1 = Utilities.GetSignValue(invoiceTotalVATList[0].VatPercentage);
                VatCalculatedAmount1 = invoiceTotalVATList[0].VatCalculatedAmount.ToString();
                VatCalculatedAmountSign1 = Utilities.GetSignValue(invoiceTotalVATList[0].VatCalculatedAmount);

                if (invoiceTotalVATList.Count > 1)
                {
                    VatIdentifier2 = invoiceTotalVATList[1].VatIdentifier != null ? invoiceTotalVATList[1].VatIdentifier : "NL";
                    VatLabel2 = invoiceTotalVATList[1].VatLabel;
                    VatText2 = invoiceTotalVATList[1].VatText;
                    VatBaseAmount2 = invoiceTotalVATList[1].VatBaseAmount.ToString();
                    VatBaseAmountSign2 = Utilities.GetSignValue(invoiceTotalVATList[1].VatBaseAmount);
                    VatPercentage2 = invoiceTotalVATList[1].VatPercentage.ToString();
                    VatPercentageSign2 = Utilities.GetSignValue(invoiceTotalVATList[1].VatPercentage);
                    VatCalculatedAmount2 = invoiceTotalVATList[1].VatCalculatedAmount.ToString();
                    VatCalculatedAmountSign2 = Utilities.GetSignValue(invoiceTotalVATList[1].VatCalculatedAmount);
                }
            }

            //Logger.Info("End of Converting InvoiceTotalVAT into InvoiceVatRecord.");

        }

        /// <summary>
        /// To implement the interface.
        /// </summary>
        /// <param name="invoiceTotalVATList">List of InvoiceTotalVAT</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ProcessNextClass(List<InvoiceTotalVAT> invoiceTotalVATList, ref long recordSequenceNumber)
        {
            //Logger.Info("InvoiceTotalVAT Class does not have childs.");
        }

        #endregion

        #region Implementation of IRecordToClassConverter<List<InvoiceTotalVAT>> To Read IDEC File.

        /// <summary>
        /// To Convert InvoiceVatRecord into InvoiceTotalVAT.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <returns>List of InvoiceTotalVAT</returns>
        public List<InvoiceTotalVAT> ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Info("Start of Converting InvoiceVatRecord into InvoiceTotalVAT.");

            var invoiceTotalVat = CreateInvoiceTotalVatList();

            ProcessNextRecord(multiRecordEngine, invoiceTotalVat);

            //Logger.Info("End of Converting InvoiceVatRecord into InvoiceTotalVAT.");

            return invoiceTotalVat;
        }

        /// <summary>
        /// Creates InvoiceTotalVAT for InvoiceVatRecord present in the reading input IDEC File.
        /// </summary>
        /// <returns>List of InvoiceTotalVAT</returns>
        private List<InvoiceTotalVAT> CreateInvoiceTotalVatList()
        {
            //Logger.Info("Start of Creating InvoiceTotalVAT Class from InvoiceVatRecord.");

            var invoiceTotalVATList = new List<InvoiceTotalVAT>();

            if (!string.IsNullOrEmpty(VatIdentifier1.Trim()))
            {
                invoiceTotalVATList.Add(new InvoiceTotalVAT
                {
                    VatIdentifier = VatIdentifier1.Trim(),
                    VatLabel = VatLabel1.Trim(),
                    VatText = VatText1.Trim(),
                    VatBaseAmount = Utilities.GetActualValueForDouble(VatBaseAmountSign1, VatBaseAmount1),
                    VatPercentage = Utilities.GetActualValueForDouble(VatPercentageSign1, VatPercentage1),
                    VatCalculatedAmount = Utilities.GetActualValueForDouble(VatCalculatedAmountSign1, VatCalculatedAmount1)
                });
            }

            if (!string.IsNullOrEmpty(VatIdentifier2.Trim()))
            {
                invoiceTotalVATList.Add(new InvoiceTotalVAT
                {
                    VatIdentifier = VatIdentifier2.Trim(),
                    VatLabel = VatLabel2.Trim(),
                    VatText = VatText2.Trim(),
                    VatBaseAmount = Utilities.GetActualValueForDouble(VatBaseAmountSign2, VatBaseAmount2),
                    VatPercentage = Utilities.GetActualValueForDouble(VatPercentageSign2, VatPercentage2),
                    VatCalculatedAmount = Utilities.GetActualValueForDouble(VatCalculatedAmountSign2, VatCalculatedAmount2)
                });
            }

            //Logger.Info("End of Creating InvoiceTotalVAT Class from InvoiceVatRecord.");

            return invoiceTotalVATList;
        }

        /// <summary>
        /// To Convert Child records of InvoiceVatRecord into there corresponding Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="invoiceTotalVATList">List of InvoiceTotalVAT</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, List<InvoiceTotalVAT> invoiceTotalVATList)
        {
            //Logger.Info("Start of Converting Childs of InvoiceVatRecord into there corresponding Classes.");

            multiRecordEngine.ReadNext();

            //Logger.Info("End of Converting Childs of InvoiceVatRecord into there corresponding Classes.");
        }

        #endregion

    }
}