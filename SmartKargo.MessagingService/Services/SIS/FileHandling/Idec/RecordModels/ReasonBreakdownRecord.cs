using System.Reflection;
using System.Text;
using FileHelpers;

using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.Model.SupportingModels;
using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.Idec.Write;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    [FixedLengthRecord()]
    public class ReasonBreakdownRecord : InvoiceRecordBase, IClassToRecordConverter<ReasonBreakdown>, IRecordToClassConverter<ReasonBreakdown>
    {
       
        #region Record Properties

        [FieldFixedLength(11)]
        public string RmBmCmNumber;

        [FieldFixedLength(2), FieldConverter(typeof(PaddingConverter), 2, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter), FieldNullValue("0")]
        public string RemarksSerialNo;

        [FieldFixedLength(80)]
        public string Remarks1;

        [FieldFixedLength(80)]
        public string Remarks2;

        [FieldFixedLength(80)]
        public string Remarks3;

        [FieldFixedLength(80)]
        public string Remarks4;

        [FieldFixedLength(80)]
        public string Remarks5;

        [FieldFixedLength(51)]
        public string Filler2;

        #endregion

        #region Constructors

        #region Parameterless Constructor

        public ReasonBreakdownRecord() { }

        #endregion

        #region Parameterized Constructor

        public ReasonBreakdownRecord(RejectionMemoRecord rejectionMemoRecord)
            : base(rejectionMemoRecord)
        {
            RmBmCmNumber = rejectionMemoRecord.RejectionMemoNumber;
            BillingCode = rejectionMemoRecord.BillingCode;
        }

        public ReasonBreakdownRecord(BillingMemoRecord billingMemoRecord)
            : base(billingMemoRecord)
        {
            RmBmCmNumber = billingMemoRecord.BillingOrCreditMemoNumber;
            BillingCode = billingMemoRecord.BillingCode;
        }

        public ReasonBreakdownRecord(CreditMemoRecord creditMemoRecord)
            : base(creditMemoRecord)
        {
            RmBmCmNumber = creditMemoRecord.BillingOrCreditMemoNumber;
            BillingCode = creditMemoRecord.BillingCode;
        }

        #endregion

        #endregion

        #region Implementation of IClassToRecordConverter<ReasonBreakdown> To Write IDEC File.

        /// <summary>
        /// To Convert ReasonBreakdown into ReasonBreakdownRecord.
        /// </summary>
        /// <param name="reasonBreakdown">ReasonBreakdown</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ConvertClassToRecord(ReasonBreakdown reasonBreakdown, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting ReasonBreakdown into ReasonBreakdownRecord.");

            this.RecordSequenceNumber = recordSequenceNumber++.ToString();
            this.StandardFieldIdentifier = IdecConstants.SfiReasonBreakdownRecord;

            string remarks = reasonBreakdown.ReasonRemarks != null ? reasonBreakdown.ReasonRemarks.Replace("\r", "").Replace("\n", "") : string.Empty;
            int remarksLength = string.IsNullOrEmpty(remarks) ? 0 : remarks.Length;

            RemarksSerialNo = reasonBreakdown.RemarkSerialNumber.ToString();

            if (remarksLength > 0 && !string.IsNullOrEmpty(remarks))
                Remarks1 = remarks.Substring(0, (remarksLength >= 80) ? 80 : remarksLength - 0);
            if (remarksLength > 80 && !string.IsNullOrEmpty(remarks))
                Remarks2 = remarks.Substring(80, (remarksLength >= 160) ? 80 : remarksLength - 80);
            if (remarksLength > 160 && !string.IsNullOrEmpty(remarks))
                Remarks3 = remarks.Substring(160, (remarksLength >= 240) ? 80 : remarksLength - 160);
            if (remarksLength > 240 && !string.IsNullOrEmpty(remarks))
                Remarks4 = remarks.Substring(240, (remarksLength >= 320) ? 80 : remarksLength - 240);
            if (remarksLength > 320 && !string.IsNullOrEmpty(remarks))
                Remarks5 = remarks.Substring(320, (remarksLength >= 400) ? 80 : remarksLength - 320);

            ProcessNextClass(reasonBreakdown, ref recordSequenceNumber);

            //Logger.Info("End of Converting ReasonBreakdown into ReasonBreakdownRecord.");

        }

        /// <summary>
        /// To Convert Childs of ReasonBreakdown into there corresponding Records.
        /// </summary>
        /// <param name="reasonBreakdown">ReasonBreakdown</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ProcessNextClass(ReasonBreakdown reasonBreakdown, ref long recordSequenceNumber)
        {
            //Logger.Info("ReasonBreakdown cannot have Childs.");
        }

        #endregion

        #region Implementation of IRecordToClassConverter<ReasonBreakdown> To Read IDEC File.

        /// <summary>
        /// To Convert ReasonBreakdownRecord into ReasonBreakdown.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <returns>ReasonBreakdown</returns>
        public ReasonBreakdown ConvertRecordToClass(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Info("Start of Converting ReasonBreakdownRecord into ReasonBreakdown.");

            var reasonBreakdown = CreateReasonBreakdown();

            ProcessNextRecord(multiRecordEngine, reasonBreakdown);
            
            //Logger.Info("End of Converting ReasonBreakdownRecord into ReasonBreakdown.");

            return reasonBreakdown;
        }

        /// <summary>
        /// Creates ReasonBreakdown for all the ReasonBreakdownRecord present in the reading input IDEC File.
        /// </summary>
        /// <returns>ReasonBreakdown</returns>
        private ReasonBreakdown CreateReasonBreakdown()
        {
            //Logger.Info("Start of Creating ReasonBreakdown Class from ReasonBreakdownRecord.");

            var reasonRemarks = new StringBuilder();
            reasonRemarks.Append(Remarks1);
            reasonRemarks.Append(Remarks2);
            reasonRemarks.Append(Remarks3);
            reasonRemarks.Append(Remarks4);
            reasonRemarks.Append(Remarks5);

            var reasonBreakdown = new ReasonBreakdown
            {
                ReasonRemarks = reasonRemarks.ToString(),
                RemarkSerialNumber = int.Parse(RemarksSerialNo)
            };

            //Logger.Info("End of Creating ReasonBreakdown Class from ReasonBreakdownRecord.");

            return reasonBreakdown;
        }

        /// <summary>
        /// To Convert Child records of ReasonBreakdownRecord into there corresponding Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="reasonBreakdown">ReasonBreakdown</param>
        public void ProcessNextRecord(MultiRecordEngine multiRecordEngine, ReasonBreakdown reasonBreakdown)
        {
            multiRecordEngine.ReadNext();
            //Logger.Info("ReasonBreakdownRecord cannot have Childs.");
        }

        #endregion
        
    }
}