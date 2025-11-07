using System.Reflection;
using System.Text;
using FileHelpers;

using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.Write;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    [FixedLengthRecord()]
    public class InvoiceFooterRecord : InvoiceRecordBase, IClassToRecordConverter<StringBuilder>, IRecordToClassConverter<StringBuilder>
    {
      
        #region Record Properties

        [FieldFixedLength(1), FieldConverter(typeof(PaddingConverter), 1, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string FooterSerialNo;

        [FieldFixedLength(70)]
        public string FooterDetails1 = string.Empty;

        [FieldFixedLength(70)]
        public string FooterDetails2 = string.Empty;

        [FieldFixedLength(70)]
        public string FooterDetails3 = string.Empty;

        [FieldFixedLength(70)]
        public string FooterDetails4 = string.Empty;

        [FieldFixedLength(70)]
        public string FooterDetails5 = string.Empty;

        [FieldFixedLength(113)]
        public string Filler2;

        #endregion

        #region Constructors

        #region Parameterless Constructor

        public InvoiceFooterRecord() { }

        #endregion

        #region Parameterized Constructor

        public InvoiceFooterRecord(InvoiceHeaderRecord invoiceHeaderRecord)
            : base(invoiceHeaderRecord)
        {
            BillingCode = IdecConstants.FileTotalRecordBillingCode;
        }

        #endregion

        #endregion

        #region Implementation of IClassToRecordConverter<InvoiceFooterRecord> To Write IDEC File.

        /// <summary>
        /// This method converts the InvoiceFooter Details into InvoiceFooterRecord.
        /// </summary>
        /// <param name="stringBuilder">StringBuilder</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ConvertClassToRecord(StringBuilder stringBuilder, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting InvoiceFooter Details into InvoiceFooterRecord.");

            StandardMessageIdentifier = IdecConstants.StandardMessageIdentifier;
            RecordSequenceNumber = recordSequenceNumber++.ToString();
            StandardFieldIdentifier = IdecConstants.SfiInvoiceFooterRecord;
            string footerText = stringBuilder != null ? stringBuilder.ToString().Replace("\r", "").Replace("\n", "") : string.Empty;
            if (footerText.Length > 0)
                FooterDetails1 = footerText.Substring(0, (footerText.Length >= 70) ? 70 : footerText.Length);
            if (footerText.Length > 70)
                FooterDetails2 = footerText.Substring(70, (footerText.Length >= 140) ? 70 : (footerText.Length - 70));
            if (footerText.Length > 140)
                FooterDetails3 = footerText.Substring(140, (footerText.Length >= 210) ? 70 : (footerText.Length - 140));
            if (footerText.Length > 210)
                FooterDetails4 = footerText.Substring(210, (footerText.Length >= 280) ? 70 : (footerText.Length - 210));
            if (footerText.Length > 280)
                FooterDetails5 = footerText.Substring(280, (footerText.Length >= 350) ? 70 : (footerText.Length - 280));

            ProcessNextClass(stringBuilder, ref recordSequenceNumber);

            //Logger.Info("End of Converting InvoiceFooter Details into InvoiceFooterRecord.");
        }

        /// <summary>
        /// To implement the interface.
        /// </summary>
        /// <param name="stringBuilder">StringBuilder</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ProcessNextClass(StringBuilder stringBuilder, ref long recordSequenceNumber)
        {
            //Logger.Info("InvoiceFooter Details does not have childs.");
        }

        #endregion

        #region Implementation of IRecordToClassConverter<InvoiceFooterDetails> To Read IDEC File.

        /// <summary>
        /// To Convert InvoiceFooterRecord into Invoice Footer Details.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <returns>Invoice Footer Details String</returns>
        public StringBuilder ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Info("Start of Converting InvoiceFooterRecord into Invoice Footer Details.");

            var invoiceFooter = CreateInvoiceFooterDetails();

            ProcessNextRecord(multiRecordEngine, invoiceFooter);

            //Logger.Info("End of Converting InvoiceFooterRecord into Invoice Footer Details.");

            return invoiceFooter;
        }

        /// <summary>
        /// Creates Invoice Footer Details for InvoiceFooterRecord present in the reading input IDEC File.
        /// </summary>
        /// <returns>Invoice Footer Details String</returns>
        private StringBuilder CreateInvoiceFooterDetails()
        {
            //Logger.Info("Start of Creating Invoice Footer Details from InvoiceFooterRecord.");

            var invoiceFooter = new StringBuilder();
            if (!string.IsNullOrEmpty(FooterDetails1.Trim()))
            {
                invoiceFooter.Append(FooterDetails1);
                invoiceFooter.Append(FooterDetails2);
                invoiceFooter.Append(FooterDetails3);
                invoiceFooter.Append(FooterDetails4);
                invoiceFooter.Append(FooterDetails5);

                //Logger.Info("End of Creating Invoice Footer Details from InvoiceFooterRecord.");

                return invoiceFooter;
            }
            //Logger.Info("End of Creating Invoice Footer Details from InvoiceFooterRecord.");
            return null;
        }

        /// <summary>
        /// To Convert Child records of InvoiceFooterRecord into there corresponding Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="stringBuilder">StringBuilder</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, StringBuilder stringBuilder)
        {
            //Logger.Info("Start of Converting Childs of InvoiceFooterRecord into there corresponding Classes.");

            multiRecordEngine.ReadNext();

            //Logger.Info("End of Converting Childs of InvoiceFooterRecord into there corresponding Classes.");
        }

        #endregion
        
    }
}
