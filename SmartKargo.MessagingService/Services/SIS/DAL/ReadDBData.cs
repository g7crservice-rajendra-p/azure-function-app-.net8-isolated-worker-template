using Microsoft.Extensions.Logging;
using QidWorkerRole.SIS.Model;
using ModelClass = QidWorkerRole.SIS.Model;

namespace QidWorkerRole.SIS.DAL
{
    /// <summary>
    /// Performs all Database Read Operations.
    /// </summary>
    public class ReadDBData
    {

        //public SIS.DAL.SISDBEntities _sisDB;
        private readonly SISDBEntities _sisDB;
        private readonly ILogger<ReadDBData> _logger;

        public ReadDBData(SISDBEntities sisDB, ILogger<ReadDBData> logger)
        {
            //_sisDB = new SIS.DAL.SISDBEntities();
            _sisDB = sisDB;
            _logger = logger;
        }

        /// <summary>
        /// Get the data to be written in the file
        /// </summary>
        /// <param name="ListInvoiceHeaderID">List of Invoice Numbers</param>
        /// <param name="airlineCode">Billing Airline Code</param>
        /// <returns>File Data</returns>
        public ModelClass.SupportingModels.FileData GetFileData(List<int> ListInvoiceHeaderID, string airlineCode)
        {
            ModelClass.SupportingModels.FileData fileData = new ModelClass.SupportingModels.FileData();

            try
            {

                #region FileHeader

                fileData.FileHeader = new ModelClass.FileHeaderClass();
                fileData.FileHeader.AirlineCode = airlineCode;

                #endregion

                #region InvoiceList

                fileData.InvoiceList = new List<ModelClass.Invoice>();
                fileData.InvoiceList = GetInvoiceList(ListInvoiceHeaderID);

                #endregion

                #region Add entry to FileHeader, FileTotal & update FileHeaderID for each invoice

                QidWorkerRole.SIS.DAL.CreateDBData createDBData = new QidWorkerRole.SIS.DAL.CreateDBData();
                fileData.InvoiceList = createDBData.InsertFileData(fileData.InvoiceList, airlineCode);

                #endregion

                #region FileTotal

                fileData.FileTotal = new ModelClass.FileTotal();
                if (fileData.InvoiceList.Count() > 0)
                {
                    fileData.FileTotal = GetFileTotal(fileData.InvoiceList[0].FileHeaderID);
                    fileData.FileTotal.BillingAirline = airlineCode;
                }

                #endregion

                return fileData;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetFileData()", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetFileData()");
                return fileData;
            }
        }

        /// <summary>
        /// Get the Invoice List from Database.
        /// </summary>
        /// <param name="fileHeaderID">fileHeaderID Foreign Key</param>
        /// <returns>List of Invoice Database Records for the given fileHeaderID</returns>
        public List<ModelClass.Invoice> GetInvoiceList(List<int> listInvoiceHeaderID)
        {
            List<ModelClass.Invoice> invoiceList = new List<Invoice>();

            try
            {

                foreach (var invoiceHeaderID in listInvoiceHeaderID)
                {
                    if (_sisDB.InvoiceHeaders.Where(invh => invh.InvoiceHeaderID == invoiceHeaderID && invh.InvoiceStatusId == 1).FirstOrDefault() != null)
                    {
                        invoiceList.Add(GetInvoice(invoiceHeaderID));
                    }
                }
                return invoiceList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure(exception.Message);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetInvoiceList()");
                return invoiceList;
            }
        }

        /// <summary>
        /// Get the Invoice from Database.
        /// </summary>
        /// <param name="fileHeaderID">fileHeaderID Foreign Key</param>
        /// <returns>Invoice Database Record for the given fileHeaderID</returns>
        public ModelClass.Invoice GetInvoice(int invoiceHeaderID)
        {
            ModelClass.Invoice invoice = new ModelClass.Invoice();

            try
            {

                #region InvoiceHeader

                var invoiceHeaderd = _sisDB.InvoiceHeaders.Where(invh => invh.InvoiceHeaderID == invoiceHeaderID).FirstOrDefault();

                if (invoiceHeaderd != null)
                {
                    invoice.InvoiceHeaderID = invoiceHeaderd.InvoiceHeaderID;
                    // invoice.FileHeaderID = Convert.ToInt32(invoiceHeaderd.FileHeaderID);
                    invoice.BillingAirline = Convert.ToString(invoiceHeaderd.BillingAirline).PadLeft(3, '0');
                    invoice.BilledAirline = Convert.ToString(invoiceHeaderd.BilledAirline).PadLeft(3, '0');
                    invoice.InvoiceNumber = invoiceHeaderd.InvoiceNumber;
                    invoice.BillingYear = Convert.ToInt32(invoiceHeaderd.BillingYear);
                    invoice.BillingMonth = Convert.ToInt32(invoiceHeaderd.BillingMonth);
                    invoice.PeriodNumber = Convert.ToInt32(invoiceHeaderd.PeriodNumber);
                    invoice.CurrencyofListing = invoiceHeaderd.CurrencyofListing;
                    invoice.CurrencyofBilling = invoiceHeaderd.CurrencyofBilling;
                    invoice.SettlementMethodIndicator = invoiceHeaderd.SettlementMethodIndicator;
                    invoice.DigitalSignatureFlag = invoiceHeaderd.DigitalSignatureFlag;
                    invoice.InvoiceDate = invoiceHeaderd.InvoiceDate;
                    invoice.ListingToBillingRate = invoiceHeaderd.ListingtoBillingRate;
                    invoice.SuspendedInvoiceFlag = invoiceHeaderd.SuspendedInvoiceFlag;
                    invoice.BillingAirlineLocationID = invoiceHeaderd.BillingAirlineLocationID;
                    invoice.BilledAirlineLocationID = invoiceHeaderd.BilledAirlineLocationID;
                    invoice.InvoiceType = invoiceHeaderd.InvoiceType;
                    invoice.InvoiceTemplateLanguage = invoiceHeaderd.InvoiceTemplateLanguage;
                    invoice.ChDueDate = invoiceHeaderd.CHDueDate;
                    invoice.ChAgreementIndicator = invoiceHeaderd.CHAgreementIndicator;
                    invoice.InvoiceFooterDetails = invoiceHeaderd.InvoiceFooterDetails;
                    invoice.CreatedBy = invoiceHeaderd.CreatedBy;
                    invoice.CreatedOn = invoiceHeaderd.CreatedOn;
                    invoice.LastUpdatedBy = invoiceHeaderd.LastUpdatedBy;
                    invoice.LastUpdatedOn = invoiceHeaderd.LastUpdatedOn;
                }

                #endregion

                invoice.AirWayBillList.AddRange(GetAirWayBillList(invoice.InvoiceHeaderID));

                invoice.RejectionMemoList.AddRange(GetRejectionMemoList(invoice.InvoiceHeaderID));

                invoice.BillingMemoList.AddRange(GetBillingMemoList(invoice.InvoiceHeaderID));

                invoice.CreditMemoList.AddRange(GetCreditMemoList(invoice.InvoiceHeaderID));

                #region Invoice Other Data

                #region InvoiceTotalVATList

                invoice.InvoiceTotalVATList.AddRange(GetInvoiceTotalVATList(invoice.InvoiceHeaderID));

                #endregion

                #region BillingCodeSubTotalList

                invoice.BillingCodeSubTotalList.AddRange(GetBillingCodeSubTotalList(invoice.InvoiceHeaderID));

                #endregion

                #region ReferenceData

                invoice.ReferenceDataList = new List<ModelClass.ReferenceDataModel>();

                // For Billing Airline
                if (invoice.BillingAirline != null && _sisDB.ReferenceDatas.Where(rd => rd.AirlineCode.Equals(invoice.BillingAirline)).FirstOrDefault() != null)
                {
                    invoice.ReferenceDataList.Add(GetReferenceDataForAirline(invoice.BillingAirline, true));
                }

                // For Billed Airline
                if (invoice.BilledAirline != null && _sisDB.ReferenceDatas.Where(rd => rd.AirlineCode.Equals(invoice.BilledAirline)).FirstOrDefault() != null)
                {
                    invoice.ReferenceDataList.Add(GetReferenceDataForAirline(invoice.BilledAirline, false));
                }

                #endregion

                #region InvoiceTotal

                invoice.InvoiceTotals = new ModelClass.InvoiceTotal();
                invoice.InvoiceTotals = GetInvoiceTotal(invoice.InvoiceHeaderID);

                #endregion

                #endregion

                return invoice;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetInvoice()" + exception.Message);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetInvoice()");
                return invoice;
            }
        }

        #region AirWayBill

        /// <summary>
        /// Get the AirWayBill List from Database.
        /// </summary>
        /// <param name="airWayBillID">invoiceHeaderID Foreign Key</param>
        /// <returns>List of AirWayBill Database Records for the given invoiceHeaderID</returns>
        public List<ModelClass.AirWayBill> GetAirWayBillList(int invoiceHeaderID)
        {
            List<ModelClass.AirWayBill> airWayBillList = new List<ModelClass.AirWayBill>();

            try
            {

                foreach (var awbd in _sisDB.AirWayBills.Where(awb => awb.InvoiceHeaderID == invoiceHeaderID))
                {
                    ModelClass.AirWayBill airWayBill = new ModelClass.AirWayBill();

                    airWayBill.AirWayBillID = awbd.AirWayBillID;
                    airWayBill.InvoiceHeaderID = awbd.InvoiceHeaderID;
                    airWayBill.BillingCode = awbd.BillingCode;
                    airWayBill.AWBDate = awbd.AWBDate;
                    airWayBill.AWBIssuingAirline = Convert.ToString(awbd.AWBIssuingAirline);
                    airWayBill.AWBSerialNumber = Convert.ToInt32(awbd.AWBSerialNumber);
                    airWayBill.AWBCheckDigit = Convert.ToInt32(awbd.AWBCheckDigit);
                    airWayBill.Origin = awbd.Origin;
                    airWayBill.Destination = awbd.Destination;
                    airWayBill.From = awbd.From;
                    airWayBill.To = awbd.To;
                    airWayBill.DateOfCarriageOrTransfer = Convert.ToString(awbd.DateOfCarriage);
                    airWayBill.AttachmentIndicatorOriginal = string.IsNullOrWhiteSpace(awbd.AttachmentIndicatorOriginal) ? "N" : awbd.AttachmentIndicatorOriginal.Equals("N") ? "N" : "Y";
                    airWayBill.AttachmentIndicatorValidated = "N";
                    airWayBill.NumberOfAttachments = Convert.ToInt32(awbd.NumberOfAttachments);
                    airWayBill.ISValidationFlag = awbd.ISValidationFlag;
                    airWayBill.ReasonCode = awbd.ReasonCode;
                    airWayBill.ReferenceField1 = awbd.ReferenceField1;
                    airWayBill.ReferenceField2 = awbd.ReferenceField2;
                    airWayBill.ReferenceField3 = awbd.ReferenceField3;
                    airWayBill.ReferenceField4 = awbd.ReferenceField4;
                    airWayBill.ReferenceField5 = awbd.ReferenceField5;
                    airWayBill.AirlineOwnUse = awbd.AirlineOwnUse;
                    airWayBill.BatchSequenceNumber = Convert.ToInt32(awbd.BatchSequenceNumber);
                    airWayBill.RecordSequenceWithinBatch = Convert.ToInt32(awbd.RecordSequencewithinBatch);
                    airWayBill.WeightCharges = Convert.ToDouble(awbd.WeightCharges);
                    airWayBill.OtherCharges = Convert.ToDouble(awbd.OtherCharges);
                    airWayBill.AmountSubjectToInterlineServiceCharge = Convert.ToDouble(awbd.AmountSubjectToInterlineServiceCharge);
                    airWayBill.InterlineServiceChargePercentage = Convert.ToDouble(awbd.InterlineServiceChargePercentage);
                    airWayBill.CurrencyAdjustmentIndicator = awbd.CurrencyAdjustmentIndicator;
                    airWayBill.BilledWeight = Convert.ToInt32(awbd.BilledWeight);
                    airWayBill.ProvisoReqSPA = awbd.ProvisoReqSPA;
                    airWayBill.ProratePercentage = Convert.ToInt32(awbd.ProratePercentage);
                    airWayBill.PartShipmentIndicator = awbd.PartShipmentIndicator;
                    airWayBill.ValuationCharges = Convert.ToDouble(awbd.ValuationCharges);
                    airWayBill.KGLBIndicator = awbd.KGLBIndicator;
                    airWayBill.VATAmount = Convert.ToDouble(awbd.VATAmount);
                    airWayBill.InterlineServiceChargeAmount = Convert.ToDouble(awbd.InterlineServiceChargeAmount);
                    airWayBill.AWBTotalAmount = Convert.ToDouble(awbd.AWBTotalAmount);
                    airWayBill.CCAindicator = awbd.CCAindicator.ToUpper().Equals("Y") ? true : false;
                    airWayBill.OurReference = awbd.OurReference;
                    airWayBill.CreatedBy = awbd.CreatedBy;
                    airWayBill.CreatedOn = awbd.CreatedOn;
                    airWayBill.LastUpdatedBy = awbd.LastUpdatedBy;
                    airWayBill.LastUpdatedOn = awbd.LastUpdatedOn;

                    airWayBill.AWBVATList.AddRange(GetAWBVATList(airWayBill.AirWayBillID));
                    airWayBill.AWBOtherChargesList.AddRange(GetAWBOtherChargesList(airWayBill.AirWayBillID));

                    airWayBillList.Add(airWayBill);
                }
                return airWayBillList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure(exception.Message);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetAirWayBillList()");
                return airWayBillList;
            }
        }

        /// <summary>
        /// Get the AWBVAT List from Database.
        /// </summary>
        /// <param name="airWayBillID">AirWayBillID Foreign Key</param>
        /// <returns>List of AWBVAT Database Records for the given AirWayBillID</returns>
        public List<ModelClass.AWBVAT> GetAWBVATList(int airWayBillID)
        {
            List<ModelClass.AWBVAT> awbVatList = new List<ModelClass.AWBVAT>();

            try
            {

                foreach (var awbVatd in _sisDB.AWBVATs.Where(awbVat => awbVat.AirWayBillID == airWayBillID))
                {
                    ModelClass.AWBVAT awbVat = new ModelClass.AWBVAT();

                    awbVat.AWBVATID = awbVatd.AWBVATID;
                    awbVat.AirWayBillID = awbVatd.AirWayBillID;
                    awbVat.VatIdentifier = awbVatd.VATIdentifier;
                    awbVat.VatLabel = awbVatd.VATLabel;
                    awbVat.VatText = awbVatd.VATText;
                    awbVat.VatBaseAmount = Convert.ToDouble(awbVatd.VATBaseAmount);
                    awbVat.VatPercentage = Convert.ToDouble(awbVatd.VATPercentage);
                    awbVat.VatCalculatedAmount = Convert.ToDouble(awbVatd.VATCalculatedAmount);
                    awbVat.CreatedBy = awbVatd.CreatedBy;
                    awbVat.CreatedOn = awbVatd.CreatedOn;
                    awbVat.LastUpdatedBy = awbVatd.LastUpdatedBy;
                    awbVat.LastUpdatedOn = awbVatd.LastUpdatedOn;

                    awbVatList.Add(awbVat);
                }
                return awbVatList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetAWBVATList()" + exception.Message);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetAWBVATList()");
                return awbVatList;
            }
        }

        /// <summary>
        /// Get the AWBOtherCharges List from Database.
        /// </summary>
        /// <param name="airWayBillID">AirWayBillID Foreign Key</param>
        /// <returns>List of AWBOtherCharges Database Records for the given AirWayBillID</returns>
        public List<ModelClass.AWBOtherCharges> GetAWBOtherChargesList(int airWayBillID)
        {
            List<ModelClass.AWBOtherCharges> awbOtherChargesList = new List<ModelClass.AWBOtherCharges>();

            try
            {

                foreach (var awbOtherChargesd in _sisDB.AWBOtherCharges.Where(awbOc => awbOc.AirWayBillID == airWayBillID))
                {
                    ModelClass.AWBOtherCharges awbOtherCharges = new ModelClass.AWBOtherCharges();

                    awbOtherCharges.AWBOtherChargesID = awbOtherChargesd.AWBOtherChargesID;
                    awbOtherCharges.AirWayBillID = awbOtherChargesd.AirWayBillID;
                    awbOtherCharges.OtherChargeCode = awbOtherChargesd.OtherChargeCode;
                    awbOtherCharges.OtherChargeCodeValue = Convert.ToDouble(awbOtherChargesd.OtherChargeCodeValue);
                    awbOtherCharges.OtherChargeVatLabel = awbOtherChargesd.VATLabel;
                    awbOtherCharges.OtherChargeVatText = awbOtherChargesd.VATText;
                    awbOtherCharges.OtherChargeVatBaseAmount = Convert.ToDouble(awbOtherChargesd.VATbaseAmount);
                    awbOtherCharges.OtherChargeVatPercentage = Convert.ToDouble(awbOtherChargesd.VATPercentage);
                    awbOtherCharges.OtherChargeVatCalculatedAmount = Convert.ToDouble(awbOtherChargesd.VATCalculatedAmount);
                    awbOtherCharges.CreatedBy = awbOtherChargesd.CreatedBy;
                    awbOtherCharges.CreatedOn = awbOtherChargesd.CreatedOn;
                    awbOtherCharges.LastUpdatedBy = awbOtherChargesd.LastUpdatedBy;
                    awbOtherCharges.LastUpdatedOn = awbOtherChargesd.LastUpdatedOn;

                    awbOtherChargesList.Add(awbOtherCharges);
                }
                return awbOtherChargesList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetAWBOtherChargesList(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetAWBOtherChargesList()");
                return awbOtherChargesList;
            }
        }

        #endregion

        #region RejectionMemo

        /// <summary>
        /// Get the RejectionMemo List from Database.
        /// </summary>
        /// <param name="invoiceHeaderID">invoiceHeaderID Foreign Key</param>
        /// <returns>List of RejectionMemo Database Records for the given invoiceHeaderID</returns>
        public List<ModelClass.RejectionMemo> GetRejectionMemoList(int invoiceHeaderID)
        {
            List<ModelClass.RejectionMemo> rejectionMemoList = new List<ModelClass.RejectionMemo>();

            try
            {

                foreach (var rmd in _sisDB.RejectionMemoes.Where(rm => rm.InvoiceHeaderID == invoiceHeaderID))
                {
                    ModelClass.RejectionMemo rejectionMemo = new ModelClass.RejectionMemo();

                    rejectionMemo.RejectionMemoID = rmd.RejectionMemoID;
                    rejectionMemo.InvoiceHeaderID = rmd.InvoiceHeaderID;
                    rejectionMemo.RejectionStage = Convert.ToInt32(rmd.RejectionStage);
                    rejectionMemo.YourRejectionNumber = rmd.YourRejectionMemoNumber;
                    rejectionMemo.RejectionMemoNumber = rmd.RejectionMemoNumber;
                    rejectionMemo.YourBillingMemoNumber = rmd.YourBMCMNumber;
                    rejectionMemo.BilledTotalWeightCharge = rmd.TotalWeightChargesBilled;
                    rejectionMemo.AcceptedTotalWeightCharge = rmd.TotalWeightChargesAccepted;
                    rejectionMemo.TotalWeightChargeDifference = rmd.TotalWeightChargesDifference;
                    rejectionMemo.BilledTotalValuationCharge = rmd.TotalValuationChargesBilled;
                    rejectionMemo.AcceptedTotalValuationCharge = rmd.TotalValuationChargesAccepted;
                    rejectionMemo.TotalValuationChargeDifference = rmd.TotalValuationChargesDifference;
                    rejectionMemo.BilledTotalOtherChargeAmount = rmd.TotalOtherChargesAmountBilled;
                    rejectionMemo.AcceptedTotalOtherChargeAmount = rmd.TotalOtherChargesAmountAccepted;
                    rejectionMemo.TotalOtherChargeDifference = rmd.TotalOtherChargesDifference;
                    rejectionMemo.AllowedTotalIscAmount = rmd.TotalISCAmountAllowed;
                    rejectionMemo.AcceptedTotalIscAmount = rmd.TotalISCAmountAccepted;
                    rejectionMemo.TotalIscAmountDifference = rmd.TotalISCAmountDifference;
                    rejectionMemo.BilledTotalVatAmount = rmd.TotalVATAmountBilled;
                    rejectionMemo.AcceptedTotalVatAmount = rmd.TotalVATAmountAccepted;
                    rejectionMemo.TotalVatAmountDifference = Convert.ToDouble(rmd.TotalVATAmountDifference);
                    rejectionMemo.TotalNetRejectAmount = rmd.TotalNetRejectAmount;
                    rejectionMemo.ReasonCode = rmd.ReasonCode.PadLeft(2, '0');
                    rejectionMemo.BMCMIndicator = rmd.BMCMIndicator;
                    rejectionMemo.BatchSequenceNumber = Convert.ToInt32(rmd.BatchsequenceNumber);
                    rejectionMemo.RecordSequenceWithinBatch = Convert.ToInt32(rmd.RecordSequenceWithinBatch);
                    rejectionMemo.YourInvoiceNumber = rmd.YourInvoiceNumber;
                    rejectionMemo.YourInvoiceBillingYear = Convert.ToInt32(rmd.YourInvoiceBillingYear);
                    rejectionMemo.YourInvoiceBillingMonth = Convert.ToInt32(rmd.YourInvoiceBillingMonth);
                    rejectionMemo.YourInvoiceBillingPeriod = Convert.ToInt32(rmd.YourInvoiceBillingPeriod);
                    rejectionMemo.BillingCode = rmd.BillingCode;
                    rejectionMemo.AttachmentIndicatorOriginal = string.IsNullOrWhiteSpace(rmd.AttachmentIndicatorOriginal) ? false : rmd.AttachmentIndicatorOriginal.Equals("Y") ? true : false;
                    rejectionMemo.AttachmentIndicatorValidated = false;
                    rejectionMemo.NumberOfAttachments = Convert.ToInt32(rmd.NumberOfAttachments);
                    rejectionMemo.AirlineOwnUse = rmd.AirlineOwnUse;
                    rejectionMemo.ISValidationFlag = rmd.ISValidationFlag;
                    rejectionMemo.ReasonRemarks = rmd.ReasonRemarks;
                    rejectionMemo.OurRef = rmd.OurRef;
                    rejectionMemo.CreatedBy = rmd.CreatedBy;
                    rejectionMemo.CreatedOn = rmd.CreatedOn;
                    rejectionMemo.LastUpdatedBy = rmd.LastUpdatedBy;
                    rejectionMemo.LastUpdatedOn = rmd.LastUpdatedOn;
                    rejectionMemo.CorrespondenceRefNo = rmd.CorrespondenceRefNo;

                    rejectionMemo.RMAirWayBillList.AddRange(GetRMAirWayBillList(rejectionMemo.RejectionMemoID));
                    rejectionMemo.RMVATList.AddRange(GetRMVATList(rejectionMemo.RejectionMemoID));

                    rejectionMemoList.Add(rejectionMemo);
                }
                return rejectionMemoList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetRejectionMemoList()", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetRejectionMemoList()");
                return rejectionMemoList;
            }
        }

        #region RMAirWayBill

        /// <summary>
        /// Get the List of RMAirWayBill from Database.
        /// </summary>
        /// <param name="rejectionMemoID">rejectionMemoID Foreign Key</param>
        /// <returns>List of RMAirWayBill Database Records for the given rejectionMemoID</returns>
        public List<ModelClass.RMAirWayBill> GetRMAirWayBillList(int rejectionMemoID)
        {
            List<ModelClass.RMAirWayBill> rMAirWayBillList = new List<ModelClass.RMAirWayBill>();

            try
            {

                foreach (var rmawbd in _sisDB.RMAirWayBills.Where(rmawb => rmawb.RejectionMemoID == rejectionMemoID))
                {
                    ModelClass.RMAirWayBill rMAirWayBill = new ModelClass.RMAirWayBill();

                    rMAirWayBill.RMAirWayBillID = rmawbd.RMAirWayBillID;
                    rMAirWayBill.RejectionMemoID = rmawbd.RejectionMemoID;
                    rMAirWayBill.BilledWeightCharge = Convert.ToDouble(rmawbd.WeightChargesBilled);
                    rMAirWayBill.AcceptedWeightCharge = Convert.ToDouble(rmawbd.WeightChargesAccepted);
                    rMAirWayBill.WeightChargeDiff = Convert.ToDouble(rmawbd.WeightChargesDifference);
                    rMAirWayBill.BilledValuationCharge = Convert.ToDouble(rmawbd.ValuationChargesBilled);
                    rMAirWayBill.AcceptedValuationCharge = Convert.ToDouble(rmawbd.ValuationChargesAccepted);
                    rMAirWayBill.ValuationChargeDiff = Convert.ToDouble(rmawbd.ValuationChargesDifference);
                    rMAirWayBill.BilledOtherCharge = Convert.ToDouble(rmawbd.OtherChargesAmountBilled);
                    rMAirWayBill.AcceptedOtherCharge = Convert.ToDouble(rmawbd.OtherChargesAmountAccepted);
                    rMAirWayBill.OtherChargeDiff = Convert.ToDouble(rmawbd.OtherChargesDifference);
                    rMAirWayBill.AllowedAmtSubToIsc = Convert.ToDouble(rmawbd.AmountSubjectedToISCAllowed);
                    rMAirWayBill.AcceptedAmtSubToIsc = Convert.ToDouble(rmawbd.AmountSubjectedToISCAccepted);
                    rMAirWayBill.AllowedIscPercentage = Convert.ToDouble(rmawbd.ISCPercentageAllowed);
                    rMAirWayBill.AcceptedIscPercentage = Convert.ToDouble(rmawbd.ISCPercentageAccepted);
                    rMAirWayBill.AllowedIscAmount = Convert.ToDouble(rmawbd.ISCAmountAllowed);
                    rMAirWayBill.AcceptedIscAmount = Convert.ToDouble(rmawbd.ISCAmountAccepted);
                    rMAirWayBill.IscAmountDifference = Convert.ToDouble(rmawbd.ISCAmountDifference);
                    rMAirWayBill.BilledVatAmount = Convert.ToDouble(rmawbd.VATAmountBilled);
                    rMAirWayBill.AcceptedVatAmount = Convert.ToDouble(rmawbd.VATAmountAccepted);
                    rMAirWayBill.VatAmountDifference = Convert.ToDouble(rmawbd.VATAmountDifference);
                    rMAirWayBill.NetRejectAmount = Convert.ToDouble(rmawbd.NetRejectAmount);
                    rMAirWayBill.CurrencyAdjustmentIndicator = rmawbd.CurrencyAdjustmentIndicator;
                    rMAirWayBill.BilledWeight = Convert.ToInt32(rmawbd.BilledActualFlownWeight);
                    rMAirWayBill.ProvisionalReqSpa = rmawbd.ProvisoReqSPA;
                    rMAirWayBill.ProratePercentage = Convert.ToInt32(rmawbd.ProratePercentage);
                    rMAirWayBill.PartShipmentIndicator = rmawbd.PartShipmentIndicator;
                    rMAirWayBill.KgLbIndicator = rmawbd.KGLBIndicator;
                    rMAirWayBill.CcaIndicator = rmawbd.CCAindicator.ToUpper().Equals("Y") ? true : false;
                    rMAirWayBill.OurReference = rmawbd.OurReference;
                    rMAirWayBill.BillingCode = rmawbd.BillingCode;
                    rMAirWayBill.AWBDate = rmawbd.AWBDate;
                    rMAirWayBill.AWBIssuingAirline = Convert.ToString(rmawbd.AWBIssuingAirline);
                    rMAirWayBill.AWBSerialNumber = Convert.ToInt32(rmawbd.AWBSerialNumber);
                    rMAirWayBill.AWBCheckDigit = Convert.ToInt32(rmawbd.AWBCheckDigit);
                    rMAirWayBill.Origin = rmawbd.ConsignmentOrigin;
                    rMAirWayBill.Destination = rmawbd.ConsignmentDestination;
                    rMAirWayBill.From = rmawbd.CarriageFrom;
                    rMAirWayBill.To = rmawbd.CarriageTo;
                    rMAirWayBill.DateOfCarriageOrTransfer = (rmawbd.TransferDate.Year).ToString().Substring(2, 2).PadLeft(2, '0') + rmawbd.TransferDate.Month.ToString().PadLeft(2, '0') + rmawbd.TransferDate.Day.ToString().PadLeft(2, '0');
                    rMAirWayBill.AttachmentIndicatorOriginal = string.IsNullOrWhiteSpace(rmawbd.AttachmentIndicatorOriginal) ? "N" : rmawbd.AttachmentIndicatorOriginal.Equals("Y") ? "Y" : "N";
                    rMAirWayBill.AttachmentIndicatorValidated = "N";
                    rMAirWayBill.NumberOfAttachments = rmawbd.NumberOfAttachments != null ? Convert.ToInt32(rmawbd.NumberOfAttachments) : 0;
                    rMAirWayBill.ISValidationFlag = rmawbd.ISValidationFlag;
                    rMAirWayBill.ReasonCode = rmawbd.ReasonCode.PadLeft(2, '0');
                    rMAirWayBill.ReferenceField1 = rmawbd.ReferenceField1;
                    rMAirWayBill.ReferenceField2 = rmawbd.ReferenceField2;
                    rMAirWayBill.ReferenceField3 = rmawbd.ReferenceField3;
                    rMAirWayBill.ReferenceField4 = rmawbd.ReferenceField4;
                    rMAirWayBill.ReferenceField5 = rmawbd.ReferenceField5;
                    rMAirWayBill.AirlineOwnUse = rmawbd.AirlineOwnUse;
                    rMAirWayBill.BreakdownSerialNumber = Convert.ToInt32(rmawbd.BreakdownSerialNumber);
                    rMAirWayBill.CreatedBy = rmawbd.CreatedBy;
                    rMAirWayBill.CreatedOn = rmawbd.CreatedOn;
                    rMAirWayBill.LastUpdatedBy = rmawbd.LastUpdatedBy;
                    rMAirWayBill.LastUpdatedOn = rmawbd.LastUpdatedOn;

                    rMAirWayBill.RMAWBOtherChargesList.AddRange(GetRMAWBOtherChargesList(rMAirWayBill.RMAirWayBillID));
                    rMAirWayBill.RMAWBProrateLadderList.AddRange(GetRMAWBProrateLadderList(rMAirWayBill.RMAirWayBillID));
                    rMAirWayBill.RMAWBVATList.AddRange(GetRMAWBVATList(rMAirWayBill.RMAirWayBillID));

                    rMAirWayBillList.Add(rMAirWayBill);
                }

                return rMAirWayBillList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetRMAirWayBillList()", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetRMAirWayBillList()");
                return rMAirWayBillList;
            }
        }

        /// <summary>
        /// Get the List of RMAWBOtherCharges from Database.
        /// </summary>
        /// <param name="rMAirWayBillID">rMAirWayBillID Foreign Key</param>
        /// <returns>List of RMAWBOtherCharges Database Records for the given rMAirWayBillID</returns>
        public List<ModelClass.RMAWBOtherCharges> GetRMAWBOtherChargesList(int rMAirWayBillID)
        {
            List<ModelClass.RMAWBOtherCharges> rMAWBOtherChargesList = new List<ModelClass.RMAWBOtherCharges>();

            try
            {

                foreach (var rMAWBOtherChargesd in _sisDB.RMAWBOtherCharges.Where(rmawboc => rmawboc.RMAirWayBillID == rMAirWayBillID))
                {
                    ModelClass.RMAWBOtherCharges rMAWBOtherCharges = new ModelClass.RMAWBOtherCharges();

                    rMAWBOtherCharges.RMAWBOtherChargesID = rMAWBOtherChargesd.RMAWBOtherChargesID;
                    rMAWBOtherCharges.RMAirWayBillID = rMAWBOtherChargesd.RMAirWayBillID;
                    rMAWBOtherCharges.OtherChargeCode = rMAWBOtherChargesd.OtherChargeCode;
                    rMAWBOtherCharges.OtherChargeCodeValue = Convert.ToDouble(rMAWBOtherChargesd.OtherChargeCodeValue);
                    rMAWBOtherCharges.OtherChargeVatLabel = rMAWBOtherChargesd.VATLabel;
                    rMAWBOtherCharges.OtherChargeVatText = rMAWBOtherChargesd.VATText;
                    rMAWBOtherCharges.OtherChargeVatBaseAmount = Convert.ToDouble(rMAWBOtherChargesd.VATbaseamount);
                    rMAWBOtherCharges.OtherChargeVatPercentage = Convert.ToDouble(rMAWBOtherChargesd.VATpercentage);
                    rMAWBOtherCharges.OtherChargeVatCalculatedAmount = Convert.ToDouble(rMAWBOtherChargesd.VATcalculatedamount);
                    rMAWBOtherCharges.CreatedBy = rMAWBOtherChargesd.CreatedBy;
                    rMAWBOtherCharges.CreatedOn = rMAWBOtherChargesd.CreatedOn;
                    rMAWBOtherCharges.LastUpdatedBy = rMAWBOtherChargesd.LastUpdatedBy;
                    rMAWBOtherCharges.LastUpdatedOn = rMAWBOtherChargesd.LastUpdatedOn;

                    rMAWBOtherChargesList.Add(rMAWBOtherCharges);
                }

                return rMAWBOtherChargesList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetRMAWBOtherChargesList()", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetRMAWBOtherChargesList()");
                return rMAWBOtherChargesList;
            }
        }

        /// <summary>
        /// Get the List of RMAWBProrateLadder from Database.
        /// </summary>
        /// <param name="rMAirWayBillID">rMAirWayBillID Foreign Key</param>
        /// <returns>List of RMAWBProrateLadder Database Records for the given rMAirWayBillID</returns>
        public List<ModelClass.RMAWBProrateLadder> GetRMAWBProrateLadderList(int rMAirWayBillID)
        {
            List<ModelClass.RMAWBProrateLadder> rMAWBProrateLadderList = new List<ModelClass.RMAWBProrateLadder>();

            try
            {

                foreach (var rMAWBProrateLadderd in _sisDB.RMAWBProrateLadders.Where(rmawbpl => rmawbpl.RMAirWayBillID == rMAirWayBillID))
                {
                    ModelClass.RMAWBProrateLadder rMAWBProrateLadder = new ModelClass.RMAWBProrateLadder();

                    rMAWBProrateLadder.RMAWBProrateLadderID = rMAWBProrateLadderd.RMAWBProrateLadderID;
                    rMAWBProrateLadder.RMAirWayBillID = rMAWBProrateLadderd.RMAirWayBillID;
                    rMAWBProrateLadder.FromSector = rMAWBProrateLadderd.FromSector;
                    rMAWBProrateLadder.ToSector = rMAWBProrateLadderd.ToSector;
                    rMAWBProrateLadder.CarrierPrefix = rMAWBProrateLadderd.CarrierPrefix;
                    rMAWBProrateLadder.ProvisoReqSpa = rMAWBProrateLadderd.ProvisoReqSPA;
                    rMAWBProrateLadder.ProrateFactor = Convert.ToInt64(rMAWBProrateLadderd.ProrateFactor);
                    rMAWBProrateLadder.PercentShare = Convert.ToDouble(rMAWBProrateLadderd.PercentShare);
                    rMAWBProrateLadder.TotalAmount = Convert.ToDouble(rMAWBProrateLadderd.TotalAmount);
                    rMAWBProrateLadder.CreatedBy = rMAWBProrateLadderd.CreatedBy;
                    rMAWBProrateLadder.CreatedOn = rMAWBProrateLadderd.CreatedOn;
                    rMAWBProrateLadder.LastUpdatedBy = rMAWBProrateLadderd.LastUpdatedBy;
                    rMAWBProrateLadder.LastUpdatedOn = rMAWBProrateLadderd.LastUpdatedOn;

                    rMAWBProrateLadderList.Add(rMAWBProrateLadder);
                }

                return rMAWBProrateLadderList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetRMAWBProrateLadderList()", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetRMAWBProrateLadderList()");
                return rMAWBProrateLadderList;
            }
        }

        /// <summary>
        /// Get the List of RMAWBVAT from Database.
        /// </summary>
        /// <param name="rMAirWayBillID">rMAirWayBillID Foreign Key</param>
        /// <returns>List of RMAWBVAT Database Records for the given rMAirWayBillID</returns>
        public List<ModelClass.RMAWBVAT> GetRMAWBVATList(int rMAirWayBillID)
        {
            List<ModelClass.RMAWBVAT> rMAWBVATList = new List<ModelClass.RMAWBVAT>();

            try
            {

                foreach (var rMAWBVATd in _sisDB.RMAWBVATs.Where(rmawbvat => rmawbvat.RMAirWayBillID == rMAirWayBillID))
                {
                    ModelClass.RMAWBVAT rMAWBVAT = new ModelClass.RMAWBVAT();

                    rMAWBVAT.RMAWBVATID = rMAWBVATd.RMAWBVATID;
                    rMAWBVAT.RMAirWayBillID = rMAWBVATd.RMAirWayBillID;
                    rMAWBVAT.VatIdentifier = rMAWBVATd.VATIdentifier;
                    rMAWBVAT.VatLabel = rMAWBVATd.VATLabel;
                    rMAWBVAT.VatText = rMAWBVATd.VATText;
                    rMAWBVAT.VatBaseAmount = Convert.ToDouble(rMAWBVATd.VATBaseAmount);
                    rMAWBVAT.VatPercentage = Convert.ToDouble(rMAWBVATd.VATPercentage);
                    rMAWBVAT.VatCalculatedAmount = Convert.ToDouble(rMAWBVATd.VATCalculatedAmount);
                    rMAWBVAT.CreatedBy = rMAWBVATd.CreatedBy;
                    rMAWBVAT.CreatedOn = rMAWBVATd.CreatedOn;
                    rMAWBVAT.LastUpdatedBy = rMAWBVATd.LastUpdatedBy;
                    rMAWBVAT.LastUpdatedOn = rMAWBVATd.LastUpdatedOn;

                    rMAWBVATList.Add(rMAWBVAT);
                }

                return rMAWBVATList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetRMAWBVATList(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetRMAWBVATList()");
                return rMAWBVATList;
            }
        }

        #endregion

        /// <summary>
        /// Get the List of RMVAT from Database.
        /// </summary>
        /// <param name="rejectionMemoID">rejectionMemoID Foreign Key</param>
        /// <returns>List of RMVAT Database Records for the given rejectionMemoID</returns>
        public List<ModelClass.RMVAT> GetRMVATList(int rejectionMemoID)
        {
            List<ModelClass.RMVAT> rMVATList = new List<ModelClass.RMVAT>();

            try
            {

                foreach (var rmVatd in _sisDB.RMVATs.Where(rmVat => rmVat.RejectionMemoID == rejectionMemoID))
                {
                    ModelClass.RMVAT rMVAT = new ModelClass.RMVAT();

                    rMVAT.RMVATID = rmVatd.RMVATID;
                    rMVAT.RejectionMemoID = rmVatd.RejectionMemoID;
                    rMVAT.VatIdentifier = rmVatd.VATIdentifier;
                    rMVAT.VatLabel = rmVatd.VATLabel;
                    rMVAT.VatText = rmVatd.VATText;
                    rMVAT.VatBaseAmount = Convert.ToDouble(rmVatd.VATBaseAmount);
                    rMVAT.VatPercentage = Convert.ToDouble(rmVatd.VATPercentage);
                    rMVAT.VatCalculatedAmount = Convert.ToDouble(rmVatd.VATCalculatedAmount);
                    rMVAT.CreatedBy = rmVatd.CreatedBy;
                    rMVAT.CreatedOn = rmVatd.CreatedOn;
                    rMVAT.LastUpdatedBy = rmVatd.LastUpdatedBy;
                    rMVAT.LastUpdatedOn = rmVatd.LastUpdatedOn;

                    rMVATList.Add(rMVAT);
                }

                return rMVATList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetRMVATList()", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetRMVATList()");
                return rMVATList;
            }
        }

        #endregion

        #region BillingMemo

        /// <summary>
        /// Get the BillingMemo List from Database.
        /// </summary>
        /// <param name="invoiceHeaderID">invoiceHeaderID Foreign Key</param>
        /// <returns>List of BillingMemo Database Records for the given invoiceHeaderID</returns>
        public List<ModelClass.BillingMemo> GetBillingMemoList(int invoiceHeaderID)
        {
            List<ModelClass.BillingMemo> billingMemoList = new List<ModelClass.BillingMemo>();

            try
            {

                foreach (var bmd in _sisDB.BillingMemoes.Where(rm => rm.InvoiceHeaderID == invoiceHeaderID))
                {
                    ModelClass.BillingMemo billingMemo = new ModelClass.BillingMemo();

                    billingMemo.BillingMemoID = bmd.BillingMemoID;
                    billingMemo.InvoiceHeaderID = bmd.InvoiceHeaderID;
                    billingMemo.BatchSequenceNumber = Convert.ToInt32(bmd.BatchSequenceNumber);
                    billingMemo.RecordSequenceWithinBatch = Convert.ToInt32(bmd.RecordSequenceWithinBatch);
                    billingMemo.YourInvoiceNumber = bmd.YourInvoiceNumber;
                    billingMemo.YourInvoiceBillingYear = Convert.ToInt32(bmd.YourInvoiceBillingYear);
                    billingMemo.YourInvoiceBillingMonth = Convert.ToInt32(bmd.YourInvoiceBillingMonth);
                    billingMemo.YourInvoiceBillingPeriod = Convert.ToInt32(bmd.YourInvoiceBillingPeriod);
                    billingMemo.BillingCode = bmd.BillingCode;
                    billingMemo.AttachmentIndicatorOriginal = string.IsNullOrWhiteSpace(bmd.AttachmentIndicatorOriginal) ? false : bmd.AttachmentIndicatorOriginal.Equals("Y") ? true : false;
                    billingMemo.AttachmentIndicatorValidated = string.IsNullOrWhiteSpace(bmd.AttachmentIndicatorValidated) ? false : bmd.AttachmentIndicatorValidated.Equals("Y") ? true : false;
                    billingMemo.NumberOfAttachments = bmd.NumberOfAttachments != null ? Convert.ToInt32(bmd.NumberOfAttachments) : 0;
                    billingMemo.AirlineOwnUse = bmd.AirlineOwnUse;
                    billingMemo.ISValidationFlag = bmd.ISValidationFlag;
                    billingMemo.ReasonRemarks = bmd.ReasonRemarks;
                    billingMemo.OurRef = bmd.OurRef;
                    billingMemo.NumberOfChildRecords = bmd.NumberOfAttachments != null ? Convert.ToInt64(bmd.NumberOfAttachments) : 0;
                    billingMemo.BillingMemoNumber = bmd.BillingMemoNumber;
                    billingMemo.ReasonCode = bmd.ReasonCode.PadLeft(2, '0');
                    billingMemo.CorrespondenceReferenceNumber = bmd.CorrespondenceRefNumber != null ? bmd.CorrespondenceRefNumber.Trim() : string.Empty;
                    billingMemo.BilledTotalWeightCharge = bmd.TotalWeightChargesBilled;
                    billingMemo.BilledTotalValuationAmount = bmd.TotalValuationAmountBilled;
                    billingMemo.BilledTotalOtherChargeAmount = bmd.TotalOtherChargeAmountBilled != null ? Convert.ToDecimal(bmd.TotalOtherChargeAmountBilled) : 0;
                    billingMemo.BilledTotalIscAmount = bmd.TotalISCAmountBilled != null ? Convert.ToDecimal(bmd.TotalISCAmountBilled) : 0;
                    billingMemo.BilledTotalVatAmount = bmd.TotalVATAmountBilled;
                    billingMemo.NetBilledAmount = bmd.NetBilledAmount;
                    billingMemo.CreatedBy = bmd.CreatedBy;
                    billingMemo.CreatedOn = bmd.CreatedOn;
                    billingMemo.LastUpdatedBy = bmd.LastUpdatedBy;
                    billingMemo.LastUpdatedOn = bmd.LastUpdatedOn;

                    billingMemo.BMAirWayBillList.AddRange(GetBMAirWayBillList(billingMemo.BillingMemoID));
                    billingMemo.BMVATList.AddRange(GetBMVATList(billingMemo.BillingMemoID));

                    billingMemoList.Add(billingMemo);
                }
                return billingMemoList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetBillingMemoList(), Error Messaage: {0}, Exception: {1}", exception);
                 _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetBillingMemoList()");
                return billingMemoList;
            }
        }

        #region BMAirWayBill

        /// <summary>
        /// Get the List of BMAirWayBill from Database.
        /// </summary>
        /// <param name="billingMemoID">BillingMemoID Foreign Key</param>
        /// <returns>List of BMAirWayBill Database Records for the given billingMemoID</returns>
        public List<ModelClass.BMAirWayBill> GetBMAirWayBillList(int billingMemoID)
        {
            List<ModelClass.BMAirWayBill> bMAirWayBillList = new List<ModelClass.BMAirWayBill>();

            try
            {

                foreach (var bmawbd in _sisDB.BMAirWayBills.Where(bmawb => bmawb.BillingMemoID == billingMemoID))
                {
                    ModelClass.BMAirWayBill bMAirWayBill = new ModelClass.BMAirWayBill();

                    bMAirWayBill.BMAirWayBillId = bmawbd.BMAirWayBillId;
                    bMAirWayBill.BillingMemoID = bmawbd.BillingMemoID;
                    bMAirWayBill.BilledWeightCharge = bmawbd.WeightChargesBilled != null ? Convert.ToDouble(bmawbd.WeightChargesBilled) : 0;
                    bMAirWayBill.BilledValuationCharge = bmawbd.ValuationChargesBilled != null ? Convert.ToDouble(bmawbd.ValuationChargesBilled) : 0;
                    bMAirWayBill.BilledOtherCharge = bmawbd.OtherChargesAmountBilled != null ? Convert.ToDouble(bmawbd.OtherChargesAmountBilled) : 0;
                    bMAirWayBill.BilledAmtSubToIsc = bmawbd.AmountSubjectedToISCBilled != null ? Convert.ToDouble(bmawbd.AmountSubjectedToISCBilled) : 0;
                    bMAirWayBill.BilledIscPercentage = bmawbd.ISCPercentBilled != null ? Convert.ToDouble(bmawbd.ISCPercentBilled) : 0;
                    bMAirWayBill.BilledIscAmount = bmawbd.ISCAmountBilled != null ? Convert.ToDouble(bmawbd.ISCAmountBilled) : 0;
                    bMAirWayBill.BilledVatAmount = bmawbd.VATAmountBilled != null ? Convert.ToDouble(bmawbd.VATAmountBilled) : 0;
                    bMAirWayBill.TotalAmount = bmawbd.TotalAmountBilled != null ? Convert.ToDouble(bmawbd.TotalAmountBilled) : 0;
                    bMAirWayBill.CurrencyAdjustmentIndicator = bmawbd.CurrencyAdjustmentIndicator;
                    bMAirWayBill.BilledWeight = bmawbd.BilledWeight != 0 ? Convert.ToInt32(bmawbd.BilledWeight) : 0;
                    bMAirWayBill.ProvisionalReqSpa = bmawbd.ProvisoReqSpa;
                    bMAirWayBill.PrpratePercentage = bmawbd.ProratePercent != null ? Convert.ToInt32(bmawbd.ProratePercent) : 0;
                    bMAirWayBill.PartShipmentIndicator = bmawbd.PartShipmentIndicator;
                    bMAirWayBill.KgLbIndicator = bmawbd.KGLBIndicator;
                    bMAirWayBill.CcaIndicator = string.IsNullOrWhiteSpace(bmawbd.CCaIndicator) ? false : bmawbd.CCaIndicator.Equals("Y") ? true : false;
                    bMAirWayBill.BillingCode = bmawbd.BillingCode;
                    bMAirWayBill.AWBDate = bmawbd.AWBDate;
                    bMAirWayBill.AWBIssuingAirline = bmawbd.AWBIssuingAirline.ToString();
                    bMAirWayBill.AWBSerialNumber = Convert.ToInt32(bmawbd.AWBSerialNumber);
                    bMAirWayBill.AWBCheckDigit = Convert.ToInt32(bmawbd.AWBCheckDigit);
                    bMAirWayBill.Origin = bmawbd.Origin;
                    bMAirWayBill.Destination = bmawbd.Destination;
                    bMAirWayBill.From = bmawbd.From;
                    bMAirWayBill.To = bmawbd.To;
                    bMAirWayBill.DateOfCarriageOrTransfer = (bmawbd.DateOfCarriage.Year).ToString().Substring(2, 2).PadLeft(2, '0') + bmawbd.DateOfCarriage.Month.ToString().PadLeft(2, '0') + bmawbd.DateOfCarriage.Day.ToString().PadLeft(2, '0');
                    bMAirWayBill.AttachmentIndicatorOriginal = string.IsNullOrWhiteSpace(bmawbd.AttachmentIndicatorOriginal) ? "N" : bmawbd.AttachmentIndicatorOriginal.Equals("Y") ? "Y" : "N";
                    bMAirWayBill.AttachmentIndicatorValidated = "N";
                    bMAirWayBill.NumberOfAttachments = bmawbd.NumberOfAttachments != null ? Convert.ToInt32(bmawbd.NumberOfAttachments) : 0;
                    bMAirWayBill.ISValidationFlag = bmawbd.ISValidationFlag;
                    bMAirWayBill.ReasonCode = bmawbd.ReasonCode.PadLeft(2, '0');
                    bMAirWayBill.ReferenceField1 = bmawbd.ReferenceField1;
                    bMAirWayBill.ReferenceField2 = bmawbd.ReferenceField2;
                    bMAirWayBill.ReferenceField3 = bmawbd.ReferenceField3;
                    bMAirWayBill.ReferenceField4 = bmawbd.ReferenceField4;
                    bMAirWayBill.ReferenceField5 = bmawbd.ReferenceField5;
                    bMAirWayBill.AirlineOwnUse = bmawbd.AirlineOwnUse;
                    bMAirWayBill.BreakdownSerialNumber = Convert.ToInt32(bmawbd.BreakdownSerialNumber);
                    bMAirWayBill.CreatedBy = bmawbd.CreatedBy;
                    bMAirWayBill.CreatedOn = bmawbd.CreatedOn;
                    bMAirWayBill.LastUpdatedBy = bmawbd.LastUpdatedBy;
                    bMAirWayBill.LastUpdatedOn = bmawbd.LastUpdatedOn;

                    bMAirWayBill.BMAWBOtherChargesList.AddRange(GetBMAWBOtherChargesList(bMAirWayBill.BMAirWayBillId));
                    bMAirWayBill.BMAWBProrateLadderList.AddRange(GetBMAWBProrateLadderList(bMAirWayBill.BMAirWayBillId));
                    bMAirWayBill.BMAWBVATList.AddRange(GetBMAWBVATList(bMAirWayBill.BMAirWayBillId));

                    bMAirWayBillList.Add(bMAirWayBill);
                }

                return bMAirWayBillList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetBMAirWayBillList(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetBMAirWayBillList()");
                return bMAirWayBillList;
            }
        }

        /// <summary>
        /// Get the List of BMAWBOtherCharges from Database.
        /// </summary>
        /// <param name="bMAirWayBillID">BMAirWayBillID Foreign Key</param>
        /// <returns>List of BMAWBOtherCharges Database Records for the given bMAirWayBillID</returns>
        public List<ModelClass.BMAWBOtherCharges> GetBMAWBOtherChargesList(int bMAirWayBillID)
        {
            List<ModelClass.BMAWBOtherCharges> bMAWBOtherChargesList = new List<ModelClass.BMAWBOtherCharges>();

            try
            {

                foreach (var bMAWBOtherChargesd in _sisDB.BMAWBOtherCharges.Where(bmawboc => bmawboc.BMAirWayBillID == bMAirWayBillID))
                {
                    ModelClass.BMAWBOtherCharges bMAWBOtherCharges = new ModelClass.BMAWBOtherCharges();

                    bMAWBOtherCharges.BMAWBOtherChargesID = bMAWBOtherChargesd.BMAWBOtherChargesID;
                    bMAWBOtherCharges.BMAirWayBillID = bMAWBOtherChargesd.BMAirWayBillID;
                    bMAWBOtherCharges.OtherChargeCode = bMAWBOtherChargesd.OtherChargeCode;
                    bMAWBOtherCharges.OtherChargeCodeValue = Convert.ToDouble(bMAWBOtherChargesd.OtherChargeCodeValue);
                    bMAWBOtherCharges.OtherChargeVatLabel = bMAWBOtherChargesd.VATLabel;
                    bMAWBOtherCharges.OtherChargeVatText = bMAWBOtherChargesd.VATText;
                    bMAWBOtherCharges.OtherChargeVatBaseAmount = Convert.ToDouble(bMAWBOtherChargesd.VATBaseAmount);
                    bMAWBOtherCharges.OtherChargeVatPercentage = Convert.ToDouble(bMAWBOtherChargesd.VATPercentage);
                    bMAWBOtherCharges.OtherChargeVatCalculatedAmount = Convert.ToDouble(bMAWBOtherChargesd.VATCalculatedAmount);
                    bMAWBOtherCharges.CreatedBy = bMAWBOtherChargesd.CreatedBy;
                    bMAWBOtherCharges.CreatedOn = bMAWBOtherChargesd.CreatedOn;
                    bMAWBOtherCharges.LastUpdatedBy = bMAWBOtherChargesd.LastUpdatedBy;
                    bMAWBOtherCharges.LastUpdatedOn = bMAWBOtherChargesd.LastUpdatedOn;

                    bMAWBOtherChargesList.Add(bMAWBOtherCharges);
                }

                return bMAWBOtherChargesList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetBMAWBOtherChargesList(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetBMAWBOtherChargesList()");
                return bMAWBOtherChargesList;
            }
        }

        /// <summary>
        /// Get the List of BMAWBProrateLadder from Database.
        /// </summary>
        /// <param name="bMAirWayBillID">bMAirWayBillID Foreign Key</param>
        /// <returns>List of BMAWBProrateLadder Database Records for the given bMAirWayBillID</returns>
        public List<ModelClass.BMAWBProrateLadder> GetBMAWBProrateLadderList(int bMAirWayBillID)
        {
            List<ModelClass.BMAWBProrateLadder> bMAWBProrateLadderList = new List<ModelClass.BMAWBProrateLadder>();

            try
            {

                foreach (var bMAWBProrateLadderd in _sisDB.BMAWBProrateLadders.Where(bmawbpl => bmawbpl.BMAirWayBillID == bMAirWayBillID))
                {
                    ModelClass.BMAWBProrateLadder bMAWBProrateLadder = new ModelClass.BMAWBProrateLadder();

                    bMAWBProrateLadder.BMAWBProrateLadderID = bMAWBProrateLadderd.BMAWBProrateLadderID;
                    bMAWBProrateLadder.BMAirWayBillID = bMAWBProrateLadderd.BMAirWayBillID;
                    bMAWBProrateLadder.FromSector = bMAWBProrateLadderd.FromSector;
                    bMAWBProrateLadder.ToSector = bMAWBProrateLadderd.ToSector;
                    bMAWBProrateLadder.CarrierPrefix = bMAWBProrateLadderd.CarrierPrefix;
                    bMAWBProrateLadder.ProvisoReqSpa = bMAWBProrateLadderd.ProvisoReqSpa;
                    bMAWBProrateLadder.ProrateFactor = Convert.ToInt64(bMAWBProrateLadderd.ProrateFactor);
                    bMAWBProrateLadder.PercentShare = Convert.ToDouble(bMAWBProrateLadderd.PercentShare);
                    bMAWBProrateLadder.TotalAmount = Convert.ToDouble(bMAWBProrateLadderd.TotalAmount);
                    bMAWBProrateLadder.CreatedBy = bMAWBProrateLadderd.CreatedBy;
                    bMAWBProrateLadder.CreatedOn = bMAWBProrateLadderd.CreatedOn;
                    bMAWBProrateLadder.LastUpdatedBy = bMAWBProrateLadderd.LastUpdatedBy;
                    bMAWBProrateLadder.LastUpdatedOn = bMAWBProrateLadderd.LastUpdatedOn;

                    bMAWBProrateLadderList.Add(bMAWBProrateLadder);
                }

                return bMAWBProrateLadderList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetBMAWBProrateLadderList(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetBMAWBProrateLadderList()");
                return bMAWBProrateLadderList;
            }
        }

        /// <summary>
        /// Get the List of BMAWBVAT from Database.
        /// </summary>
        /// <param name="bMAirWayBillID">bMAirWayBillID Foreign Key</param>
        /// <returns>List of BMAWBVAT Database Records for the given bMAirWayBillID</returns>
        public List<ModelClass.BMAWBVAT> GetBMAWBVATList(int bMAirWayBillID)
        {
            List<ModelClass.BMAWBVAT> bMAWBVATList = new List<ModelClass.BMAWBVAT>();

            try
            {

                foreach (var bMAWBVATd in _sisDB.BMAWBVATs.Where(bmawbvat => bmawbvat.BMAirWayBillID == bMAirWayBillID))
                {
                    ModelClass.BMAWBVAT bMAWBVAT = new ModelClass.BMAWBVAT();

                    bMAWBVAT.BMAWBVATID = bMAWBVATd.BMAWBVATID;
                    bMAWBVAT.BMAirWayBillID = bMAWBVATd.BMAirWayBillID;
                    bMAWBVAT.VatIdentifier = bMAWBVATd.VATIdentifier;
                    bMAWBVAT.VatLabel = bMAWBVATd.VATLabel;
                    bMAWBVAT.VatText = bMAWBVATd.VATText;
                    bMAWBVAT.VatBaseAmount = Convert.ToDouble(bMAWBVATd.VATBaseAmount);
                    bMAWBVAT.VatPercentage = Convert.ToDouble(bMAWBVATd.VATPercentage);
                    bMAWBVAT.VatCalculatedAmount = Convert.ToDouble(bMAWBVATd.VATCalculatedAmount);
                    bMAWBVAT.CreatedBy = bMAWBVATd.CreatedBy;
                    bMAWBVAT.CreatedOn = bMAWBVATd.CreatedOn;
                    bMAWBVAT.LastUpdatedBy = bMAWBVATd.LastUpdatedBy;
                    bMAWBVAT.LastUpdatedOn = bMAWBVATd.LastUpdatedOn;

                    bMAWBVATList.Add(bMAWBVAT);
                }

                return bMAWBVATList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetBMAWBVATList(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetBMAWBVATList()");
                return bMAWBVATList;
            }
        }

        #endregion

        /// <summary>
        /// Get the List of BMVAT from Database.
        /// </summary>
        /// <param name="billingMemoID">billingMemoID Foreign Key</param>
        /// <returns>List of BMVAT Database Records for the given billingMemoID</returns>
        public List<ModelClass.BMVAT> GetBMVATList(int billingMemoID)
        {
            List<ModelClass.BMVAT> bMVATList = new List<ModelClass.BMVAT>();

            try
            {

                foreach (var bmVatd in _sisDB.BMVATs.Where(bmVat => bmVat.BillingMemoID == billingMemoID))
                {
                    ModelClass.BMVAT bMVAT = new ModelClass.BMVAT();

                    bMVAT.BMVATID = bmVatd.BMVATID;
                    bMVAT.BillingMemoID = bmVatd.BillingMemoID;
                    bMVAT.VatIdentifier = bmVatd.VATIdentifier;
                    bMVAT.VatLabel = bmVatd.VATLabel;
                    bMVAT.VatText = bmVatd.VATText;
                    bMVAT.VatBaseAmount = Convert.ToDouble(bmVatd.VATBaseAmount);
                    bMVAT.VatPercentage = Convert.ToDouble(bmVatd.VATPercentage);
                    bMVAT.VatCalculatedAmount = Convert.ToDouble(bmVatd.VATCalculatedAmount);
                    bMVAT.CreatedBy = bmVatd.CreatedBy;
                    bMVAT.CreatedOn = bmVatd.CreatedOn;
                    bMVAT.LastUpdatedBy = bmVatd.LastUpdatedBy;
                    bMVAT.LastUpdatedOn = bmVatd.LastUpdatedOn;

                    bMVATList.Add(bMVAT);
                }

                return bMVATList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetBMVATList(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetBMVATList()");
                return bMVATList;
            }
        }

        #endregion

        #region CreditMemo

        /// <summary>
        /// Get the CreditMemo List from Database.
        /// </summary>
        /// <param name="invoiceHeaderID">invoiceHeaderID Foreign Key</param>
        /// <returns>List of CreditMemo Database Records for the given invoiceHeaderID</returns>
        public List<ModelClass.CreditMemo> GetCreditMemoList(int invoiceHeaderID)
        {
            List<ModelClass.CreditMemo> creditMemoList = new List<ModelClass.CreditMemo>();

            try
            {

                foreach (var cmd in _sisDB.CreditMemoes.Where(cm => cm.InvoiceHeaderID == invoiceHeaderID))
                {
                    ModelClass.CreditMemo creditMemo = new ModelClass.CreditMemo();

                    creditMemo.CreditMemoID = cmd.CreditMemoID;
                    creditMemo.InvoiceHeaderID = cmd.InvoiceHeaderID;
                    creditMemo.BatchSequenceNumber = Convert.ToInt32(cmd.BatchSequenceNumber);
                    creditMemo.RecordSequenceWithinBatch = Convert.ToInt32(cmd.RecordSequenceWithinBatch);
                    creditMemo.YourInvoiceNumber = cmd.YourInvoiceNumber;
                    creditMemo.YourInvoiceBillingYear = Convert.ToInt32(cmd.YourInvoiceBillingYear);
                    creditMemo.YourInvoiceBillingMonth = Convert.ToInt32(cmd.YourInvoiceBillingMonth);
                    creditMemo.YourInvoiceBillingPeriod = Convert.ToInt32(cmd.YourInvoiceBillingPeriod);
                    creditMemo.BillingCode = cmd.BillingCode;
                    creditMemo.AttachmentIndicatorOriginal = string.IsNullOrWhiteSpace(cmd.AttachmentIndicatorOriginal) ? false : cmd.AttachmentIndicatorOriginal.Equals("Y") ? true : false;
                    creditMemo.AttachmentIndicatorValidated = string.IsNullOrWhiteSpace(cmd.AttachmentIndicatorValidated) ? false : cmd.AttachmentIndicatorValidated.Equals("Y") ? true : false;
                    creditMemo.NumberOfAttachments = cmd.NumberOfAttachments != null ? Convert.ToInt32(cmd.NumberOfAttachments) : 0;
                    creditMemo.AirlineOwnUse = cmd.AirlineOwnUse;
                    creditMemo.ISValidationFlag = cmd.ISValidationFlag;
                    creditMemo.ReasonRemarks = cmd.ReasonRemarks;
                    creditMemo.OurRef = cmd.OurRef;
                    creditMemo.NumberOfChildRecords = cmd.NumberOfAttachments != null ? Convert.ToInt64(cmd.NumberOfAttachments) : 0;
                    creditMemo.CreditMemoNumber = cmd.CreditMemoNumber;
                    creditMemo.ReasonCode = cmd.ReasonCode.PadLeft(2, '0');
                    creditMemo.CorrespondenceRefNumber = cmd.CorrespondenceRefNumber != null ? cmd.CorrespondenceRefNumber.Trim() : string.Empty;
                    creditMemo.TotalWeightCharges = cmd.TotalWeightChargesCredited;
                    creditMemo.TotalValuationAmt = cmd.TotalValuationAmountCredited;
                    creditMemo.TotalOtherChargeAmt = cmd.TotalOtherChargeAmountCredited != null ? Convert.ToDecimal(cmd.TotalOtherChargeAmountCredited) : 0;
                    creditMemo.TotalIscAmountCredited = cmd.TotalISCAmountCredited != null ? Convert.ToDecimal(cmd.TotalISCAmountCredited) : 0;
                    creditMemo.TotalVatAmountCredited = cmd.TotalVATAmountCredited;
                    creditMemo.NetAmountCredited = cmd.NetCreditedAmount;
                    creditMemo.CreatedBy = cmd.CreatedBy;
                    creditMemo.CreatedOn = cmd.CreatedOn;
                    creditMemo.LastUpdatedBy = cmd.LastUpdatedBy;
                    creditMemo.LastUpdatedOn = cmd.LastUpdatedOn;

                    creditMemo.CMAirWayBillList.AddRange(GetCMAirWayBillList(creditMemo.CreditMemoID));
                    creditMemo.CMVATList.AddRange(GetCMVATList(creditMemo.CreditMemoID));

                    creditMemoList.Add(creditMemo);
                }
                return creditMemoList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetCreditMemoList(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetCreditMemoList()");
                return creditMemoList;
            }
        }

        #region CMAirWayBill

        /// <summary>
        /// Get the List of CMAirWayBill from Database.
        /// </summary>
        /// <param name="creditMemoID">CreditMemoID Foreign Key</param>
        /// <returns>List of CMAirWayBill Database Records for the given CreditMemoID</returns>
        public List<ModelClass.CMAirWayBill> GetCMAirWayBillList(int creditMemoID)
        {
            List<ModelClass.CMAirWayBill> cMAirWayBillList = new List<ModelClass.CMAirWayBill>();

            try
            {

                foreach (var cmawbd in _sisDB.CMAirWayBills.Where(cmawb => cmawb.CreditMemoID == creditMemoID))
                {
                    ModelClass.CMAirWayBill cMAirWayBill = new ModelClass.CMAirWayBill();

                    cMAirWayBill.CMAirWayBillID = cmawbd.CMAirWayBillID;
                    cMAirWayBill.CreditMemoID = cmawbd.CreditMemoID;
                    cMAirWayBill.CreditedWeightCharge = cmawbd.WeightChargesCredited != null ? Convert.ToDouble(cmawbd.WeightChargesCredited) : 0;
                    cMAirWayBill.CreditedValuationCharge = cmawbd.ValuationChargesCredited != null ? Convert.ToDouble(cmawbd.ValuationChargesCredited) : 0;
                    cMAirWayBill.CreditedOtherCharge = cmawbd.OtherChargesAmountCredited != null ? Convert.ToDouble(cmawbd.OtherChargesAmountCredited) : 0;
                    cMAirWayBill.CreditedAmtSubToIsc = cmawbd.AmountSubjectedToISCCredited != null ? Convert.ToDouble(cmawbd.AmountSubjectedToISCCredited) : 0;
                    cMAirWayBill.CreditedIscPercentage = cmawbd.ISCPercentCredited != null ? Convert.ToDouble(cmawbd.ISCPercentCredited) : 0;
                    cMAirWayBill.CreditedIscAmount = cmawbd.ISCAmountCredited != null ? Convert.ToDouble(cmawbd.ISCAmountCredited) : 0;
                    cMAirWayBill.CreditedVatAmount = cmawbd.VATAmountCredited != null ? Convert.ToDouble(cmawbd.VATAmountCredited) : 0;
                    cMAirWayBill.TotalAmountCredited = cmawbd.TotalAmountCredited != null ? Convert.ToDouble(cmawbd.TotalAmountCredited) : 0;
                    cMAirWayBill.CurrencyAdjustmentIndicator = cmawbd.CurrencyAdjustmentIndicator;
                    cMAirWayBill.BilledWeight = cmawbd.BilledWeight != 0 ? Convert.ToInt32(cmawbd.BilledWeight) : 0;
                    cMAirWayBill.ProvisionalReqSpa = cmawbd.ProvisoReqSPA;
                    cMAirWayBill.ProratePercentage = cmawbd.ProratePercent != null ? Convert.ToInt32(cmawbd.ProratePercent) : 0;
                    cMAirWayBill.PartShipmentIndicator = cmawbd.PartShipmentIndicator;
                    cMAirWayBill.KgLbIndicator = cmawbd.KGLBIndicator;
                    cMAirWayBill.CcaIndicator = string.IsNullOrWhiteSpace(cmawbd.CCaIndicator) ? false : cmawbd.CCaIndicator.Equals("Y") ? true : false;
                    cMAirWayBill.BillingCode = cmawbd.BillingCode;
                    cMAirWayBill.AWBDate = cmawbd.AWBDate;
                    cMAirWayBill.AWBIssuingAirline = cmawbd.AWBIssuingAirline.ToString();
                    cMAirWayBill.AWBSerialNumber = Convert.ToInt32(cmawbd.AWBSerialNumber);
                    cMAirWayBill.AWBCheckDigit = Convert.ToInt32(cmawbd.AWBCheckDigit);
                    cMAirWayBill.Origin = cmawbd.Origin;
                    cMAirWayBill.Destination = cmawbd.Destination;
                    cMAirWayBill.From = cmawbd.From;
                    cMAirWayBill.To = cmawbd.To;
                    cMAirWayBill.DateOfCarriageOrTransfer = (cmawbd.DateOfCarriage.Year).ToString().Substring(2, 2).PadLeft(2, '0') + cmawbd.DateOfCarriage.Month.ToString().PadLeft(2, '0') + cmawbd.DateOfCarriage.Day.ToString().PadLeft(2, '0');
                    cMAirWayBill.AttachmentIndicatorOriginal = string.IsNullOrWhiteSpace(cmawbd.AttachmentIndicatorOriginal) ? "N" : cmawbd.AttachmentIndicatorOriginal.Equals("Y") ? "Y" : "N";
                    cMAirWayBill.AttachmentIndicatorValidated = "N";
                    cMAirWayBill.NumberOfAttachments = cmawbd.NumberOfAttachments != null ? Convert.ToInt32(cmawbd.NumberOfAttachments) : 0;
                    cMAirWayBill.ISValidationFlag = cmawbd.ISValidationFlag;
                    cMAirWayBill.ReasonCode = cmawbd.ReasonCode.PadLeft(2, '0');
                    cMAirWayBill.ReferenceField1 = cmawbd.ReferenceField1;
                    cMAirWayBill.ReferenceField2 = cmawbd.ReferenceField2;
                    cMAirWayBill.ReferenceField3 = cmawbd.ReferenceField3;
                    cMAirWayBill.ReferenceField4 = cmawbd.ReferenceField4;
                    cMAirWayBill.ReferenceField5 = cmawbd.ReferenceField5;
                    cMAirWayBill.AirlineOwnUse = cmawbd.AirlineOwnUse;
                    cMAirWayBill.BreakdownSerialNumber = Convert.ToInt32(cmawbd.BreakdownSerialNumber);
                    cMAirWayBill.CreatedBy = cmawbd.CreatedBy;
                    cMAirWayBill.CreatedOn = cmawbd.CreatedOn;
                    cMAirWayBill.LastUpdatedBy = cmawbd.LastUpdatedBy;
                    cMAirWayBill.LastUpdatedOn = cmawbd.LastUpdatedOn;

                    cMAirWayBill.CMAWBOtherChargesList.AddRange(GetCMAWBOtherChargesList(cMAirWayBill.CMAirWayBillID));
                    cMAirWayBill.CMAWBProrateLadderList.AddRange(GetCMAWBProrateLadderList(cMAirWayBill.CMAirWayBillID));
                    cMAirWayBill.CMAWBVATList.AddRange(GetCMAWBVATList(cMAirWayBill.CMAirWayBillID));

                    cMAirWayBillList.Add(cMAirWayBill);
                }

                return cMAirWayBillList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetCMAirWayBillList(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetCMAirWayBillList()");
                return cMAirWayBillList;
            }
        }

        /// <summary>
        /// Get the List of CMAWBOtherCharges from Database.
        /// </summary>
        /// <param name="cMAirWayBillID">CMAirWayBillID Foreign Key</param>
        /// <returns>List of CMAWBOtherCharges Database Records for the given cMAirWayBillID</returns>
        public List<ModelClass.CMAWBOtherCharges> GetCMAWBOtherChargesList(int cMAirWayBillID)
        {
            List<ModelClass.CMAWBOtherCharges> cMAWBOtherChargesList = new List<ModelClass.CMAWBOtherCharges>();

            try
            {

                foreach (var cMAWBOtherChargesd in _sisDB.CMAWBOtherCharges.Where(cmawboc => cmawboc.CMAirWayBillID == cMAirWayBillID))
                {
                    ModelClass.CMAWBOtherCharges cMAWBOtherCharges = new ModelClass.CMAWBOtherCharges();

                    cMAWBOtherCharges.CMAWBOtherChargesID = cMAWBOtherChargesd.CMAWBOtherChargesID;
                    cMAWBOtherCharges.CMAirWayBillID = cMAWBOtherChargesd.CMAirWayBillID;
                    cMAWBOtherCharges.OtherChargeCode = cMAWBOtherChargesd.OtherChargeCode;
                    cMAWBOtherCharges.OtherChargeCodeValue = Convert.ToDouble(cMAWBOtherChargesd.OtherChargeCodeValue);
                    cMAWBOtherCharges.OtherChargeVatLabel = cMAWBOtherChargesd.VATLabel;
                    cMAWBOtherCharges.OtherChargeVatText = cMAWBOtherChargesd.VATText;
                    cMAWBOtherCharges.OtherChargeVatBaseAmount = Convert.ToDouble(cMAWBOtherChargesd.VATBaseAmount);
                    cMAWBOtherCharges.OtherChargeVatPercentage = Convert.ToDouble(cMAWBOtherChargesd.VATPercentage);
                    cMAWBOtherCharges.OtherChargeVatCalculatedAmount = Convert.ToDouble(cMAWBOtherChargesd.VATCalculatedAmount);
                    cMAWBOtherCharges.CreatedBy = cMAWBOtherChargesd.CreatedBy;
                    cMAWBOtherCharges.CreatedOn = cMAWBOtherChargesd.CreatedOn;
                    cMAWBOtherCharges.LastUpdatedBy = cMAWBOtherChargesd.LastUpdatedBy;
                    cMAWBOtherCharges.LastUpdatedOn = cMAWBOtherChargesd.LastUpdatedOn;

                    cMAWBOtherChargesList.Add(cMAWBOtherCharges);
                }

                return cMAWBOtherChargesList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetCMAWBOtherChargesList(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetCMAWBOtherChargesList()");
                return cMAWBOtherChargesList;
            }
        }

        /// <summary>
        /// Get the List of CMAWBProrateLadder from Database.
        /// </summary>
        /// <param name="cMAirWayBillID">cMAirWayBillID Foreign Key</param>
        /// <returns>List of CMAWBProrateLadder Database Records for the given cMAirWayBillID</returns>
        public List<ModelClass.CMAWBProrateLadder> GetCMAWBProrateLadderList(int cMAirWayBillID)
        {
            List<ModelClass.CMAWBProrateLadder> cMAWBProrateLadderList = new List<ModelClass.CMAWBProrateLadder>();

            try
            {

                foreach (var cMAWBProrateLadderd in _sisDB.CMAWBProrateLadders.Where(cmawbpl => cmawbpl.CMAirWayBillID == cMAirWayBillID))
                {
                    ModelClass.CMAWBProrateLadder cMAWBProrateLadder = new ModelClass.CMAWBProrateLadder();

                    cMAWBProrateLadder.CMAWBProrateLadderID = cMAWBProrateLadderd.CMAWBProrateLadderID;
                    cMAWBProrateLadder.CMAirWayBillID = cMAWBProrateLadderd.CMAirWayBillID;
                    cMAWBProrateLadder.FromSector = cMAWBProrateLadderd.FromSector;
                    cMAWBProrateLadder.ToSector = cMAWBProrateLadderd.ToSector;
                    cMAWBProrateLadder.CarrierPrefix = cMAWBProrateLadderd.CarrierPrefix;
                    cMAWBProrateLadder.ProvisoReqSpa = cMAWBProrateLadderd.ProvisoReqSpa;
                    cMAWBProrateLadder.ProrateFactor = Convert.ToInt64(cMAWBProrateLadderd.ProrateFactor);
                    cMAWBProrateLadder.PercentShare = Convert.ToDouble(cMAWBProrateLadderd.PercentShare);
                    cMAWBProrateLadder.TotalAmount = Convert.ToDouble(cMAWBProrateLadderd.TotalAmount);
                    cMAWBProrateLadder.CreatedBy = cMAWBProrateLadderd.CreatedBy;
                    cMAWBProrateLadder.CreatedOn = cMAWBProrateLadderd.CreatedOn;
                    cMAWBProrateLadder.LastUpdatedBy = cMAWBProrateLadderd.LastUpdatedBy;
                    cMAWBProrateLadder.LastUpdatedOn = cMAWBProrateLadderd.LastUpdatedOn;

                    cMAWBProrateLadderList.Add(cMAWBProrateLadder);
                }

                return cMAWBProrateLadderList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetCMAWBProrateLadderList(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetCMAWBProrateLadderList()");
                return cMAWBProrateLadderList;
            }
        }

        /// <summary>
        /// Get the List of CMAWBVAT from Database.
        /// </summary>
        /// <param name="cMAirWayBillID">cMAirWayBillID Foreign Key</param>
        /// <returns>List of CMAWBVAT Database Records for the given cMAirWayBillID</returns>
        public List<ModelClass.CMAWBVAT> GetCMAWBVATList(int cMAirWayBillID)
        {
            List<ModelClass.CMAWBVAT> cMAWBVATList = new List<ModelClass.CMAWBVAT>();

            try
            {

                foreach (var cMAWBVATd in _sisDB.CMAWBVATs.Where(cmawbvat => cmawbvat.CMAirWayBillID == cMAirWayBillID))
                {
                    ModelClass.CMAWBVAT cMAWBVAT = new ModelClass.CMAWBVAT();

                    cMAWBVAT.CMAWBVATID = cMAWBVATd.CMAWBVATID;
                    cMAWBVAT.CMAirWayBillID = cMAWBVATd.CMAirWayBillID;
                    cMAWBVAT.VatIdentifier = cMAWBVATd.VATIdentifier;
                    cMAWBVAT.VatLabel = cMAWBVATd.VATLabel;
                    cMAWBVAT.VatText = cMAWBVATd.VATText;
                    cMAWBVAT.VatBaseAmount = Convert.ToDouble(cMAWBVATd.VATBaseAmount);
                    cMAWBVAT.VatPercentage = Convert.ToDouble(cMAWBVATd.VATPercentage);
                    cMAWBVAT.VatCalculatedAmount = Convert.ToDouble(cMAWBVATd.VATCalculatedAmount);
                    cMAWBVAT.CreatedBy = cMAWBVATd.CreatedBy;
                    cMAWBVAT.CreatedOn = cMAWBVATd.CreatedOn;
                    cMAWBVAT.LastUpdatedBy = cMAWBVATd.LastUpdatedBy;
                    cMAWBVAT.LastUpdatedOn = cMAWBVATd.LastUpdatedOn;

                    cMAWBVATList.Add(cMAWBVAT);
                }

                return cMAWBVATList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetCMAWBVATList(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetCMAWBVATList()");
                return cMAWBVATList;
            }
        }

        #endregion

        /// <summary>
        /// Get the List of CMVAT from Database.
        /// </summary>
        /// <param name="creditMemoID">creditMemoID Foreign Key</param>
        /// <returns>List of CMVAT Database Records for the given creditMemoID</returns>
        public List<ModelClass.CMVAT> GetCMVATList(int creditMemoID)
        {
            List<ModelClass.CMVAT> cMVATList = new List<ModelClass.CMVAT>();

            try
            {

                foreach (var cmVatd in _sisDB.CMVATs.Where(cmVat => cmVat.CreditMemoID == creditMemoID))
                {
                    ModelClass.CMVAT cMVAT = new ModelClass.CMVAT();

                    cMVAT.CMVATID = cmVatd.CMVATID;
                    cMVAT.CreditMemoID = cmVatd.CreditMemoID;
                    cMVAT.VatIdentifier = cmVatd.VATIdentifier;
                    cMVAT.VatLabel = cmVatd.VATLabel;
                    cMVAT.VatText = cmVatd.VATText;
                    cMVAT.VatBaseAmount = Convert.ToDouble(cmVatd.VATBaseAmount);
                    cMVAT.VatPercentage = Convert.ToDouble(cmVatd.VATPercentage);
                    cMVAT.VatCalculatedAmount = Convert.ToDouble(cmVatd.VATCalculatedAmount);
                    cMVAT.CreatedBy = cmVatd.CreatedBy;
                    cMVAT.CreatedOn = cmVatd.CreatedOn;
                    cMVAT.LastUpdatedBy = cmVatd.LastUpdatedBy;
                    cMVAT.LastUpdatedOn = cmVatd.LastUpdatedOn;

                    cMVATList.Add(cMVAT);
                }

                return cMVATList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetCMVATList(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetCMVATList()");
                return cMVATList;
            }
        }

        #endregion

        #region Invoice Other Data

        /// <summary>
        /// Get the List of InvoiceTotalVAT from Database.
        /// </summary>
        /// <param name="fileHeaderID">invoiceHeaderID Foreign Key</param>
        /// <returns>List of InvoiceTotalVAT Database Records for the given invoiceHeaderID</returns>
        public List<ModelClass.InvoiceTotalVAT> GetInvoiceTotalVATList(int invoiceHeaderID)
        {
            List<ModelClass.InvoiceTotalVAT> invoiceTotalVATList = new List<ModelClass.InvoiceTotalVAT>();

            try
            {

                foreach (var invoiceTotalVATd in _sisDB.InvoiceTotalVATs.Where(invtv => invtv.InvoiceHeaderID == invoiceHeaderID))
                {
                    ModelClass.InvoiceTotalVAT invoiceTotalVAT = new ModelClass.InvoiceTotalVAT();

                    invoiceTotalVAT.InvoiceTotalVATID = invoiceTotalVATd.InvoiceTotalVATID;
                    invoiceTotalVAT.InvoiceHeaderID = invoiceTotalVATd.InvoiceHeaderID;
                    invoiceTotalVAT.VatIdentifier = invoiceTotalVATd.VATIdentifier;
                    invoiceTotalVAT.VatLabel = invoiceTotalVATd.VATLabel;
                    invoiceTotalVAT.VatText = invoiceTotalVATd.VATText;
                    invoiceTotalVAT.VatBaseAmount = Convert.ToDouble(invoiceTotalVATd.VATBaseAmount);
                    invoiceTotalVAT.VatPercentage = Convert.ToDouble(invoiceTotalVATd.VATPercentage);
                    invoiceTotalVAT.VatCalculatedAmount = Convert.ToDouble(invoiceTotalVATd.VATCalculatedAmount);
                    invoiceTotalVAT.CreatedBy = invoiceTotalVATd.CreatedBy;
                    invoiceTotalVAT.CreatedOn = invoiceTotalVATd.CreatedOn;
                    invoiceTotalVAT.LastUpdatedBy = invoiceTotalVATd.LastUpdatedBy;
                    invoiceTotalVAT.LastUpdatedOn = invoiceTotalVATd.LastUpdatedOn;

                    invoiceTotalVATList.Add(invoiceTotalVAT);
                }

                return invoiceTotalVATList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetInvoiceTotalVATList(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetInvoiceTotalVATList()");
                return invoiceTotalVATList;
            }
        }

        /// <summary>
        /// Get the List of BillingCodeSubTotal from Database.
        /// </summary>
        /// <param name="fileHeaderID">invoiceHeaderID Foreign Key</param>
        /// <returns>List of BillingCodeSubTotal Database Records for the given invoiceHeaderID</returns>
        public List<ModelClass.BillingCodeSubTotal> GetBillingCodeSubTotalList(int invoiceHeaderID)
        {
            List<ModelClass.BillingCodeSubTotal> billingCodeSubTotalList = new List<ModelClass.BillingCodeSubTotal>();

            try
            {

                foreach (var billingCodeSubTotald in _sisDB.BillingCodeSubTotals.Where(bcst => bcst.InvoiceHeaderID == invoiceHeaderID))
                {
                    ModelClass.BillingCodeSubTotal billingCodeSubTotal = new ModelClass.BillingCodeSubTotal();

                    billingCodeSubTotal.BillingCodeSubTotalID = billingCodeSubTotald.BillingCodeSubTotalID;
                    billingCodeSubTotal.InvoiceHeaderID = billingCodeSubTotald.InvoiceHeaderID;
                    billingCodeSubTotal.BillingCode = billingCodeSubTotald.BillingCode;
                    billingCodeSubTotal.TotalWeightCharge = Convert.ToDecimal(billingCodeSubTotald.TotalWeighCharges);
                    billingCodeSubTotal.TotalOtherCharge = Convert.ToDecimal(billingCodeSubTotald.TotalOtherCharges);
                    billingCodeSubTotal.TotalIscAmount = Convert.ToDecimal(billingCodeSubTotald.TotalInterlineServiceCharge);
                    billingCodeSubTotal.BillingCodeSbTotal = Convert.ToDecimal(billingCodeSubTotald.BillingCodeSubTotal1);
                    billingCodeSubTotal.NumberOfBillingRecords = Convert.ToInt32(billingCodeSubTotald.TotalNumberOfBillingRecords);
                    billingCodeSubTotal.TotalValuationCharge = Convert.ToDecimal(billingCodeSubTotald.TotalValuationCharges);
                    billingCodeSubTotal.TotalVatAmount = Convert.ToDecimal(billingCodeSubTotald.TotalVATAmount);
                    billingCodeSubTotal.TotalNumberOfRecords = Convert.ToInt32(billingCodeSubTotald.TotalNumberOfRecords);
                    billingCodeSubTotal.BillingCodeSubTotalDesc = billingCodeSubTotald.BillingCodeSubTotalDescription;
                    billingCodeSubTotal.CreatedBy = billingCodeSubTotald.CreatedBy;
                    billingCodeSubTotal.CreatedOn = billingCodeSubTotald.CreatedOn;
                    billingCodeSubTotal.LastUpdatedBy = billingCodeSubTotald.LastUpdatedBy;
                    billingCodeSubTotal.LastUpdatedOn = billingCodeSubTotald.LastUpdatedOn;

                    billingCodeSubTotal.BillingCodeSubTotalVATList.AddRange(GetBillingCodeSubTotalVATList(billingCodeSubTotal.BillingCodeSubTotalID));

                    billingCodeSubTotalList.Add(billingCodeSubTotal);
                }

                return billingCodeSubTotalList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetBillingCodeSubTotalList(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetBillingCodeSubTotalList()");
                return billingCodeSubTotalList;
            }
        }

        /// <summary>
        /// Get the List of BillingCodeSubTotalVAT from Database.
        /// </summary>
        /// <param name="fileHeaderID">invoiceHeaderID Foreign Key</param>
        /// <returns>List of BillingCodeSubTotalVAT Database Records for the given BillingCodeSubTotalID</returns>
        public List<ModelClass.BillingCodeSubTotalVAT> GetBillingCodeSubTotalVATList(int billingCodeSubTotalID)
        {
            List<ModelClass.BillingCodeSubTotalVAT> billingCodeSubTotalVATList = new List<ModelClass.BillingCodeSubTotalVAT>();

            try
            {

                foreach (var billingCodeSubTotalVATd in _sisDB.BillingCodeSubTotalVATs.Where(bcstv => bcstv.BillingCodeSubTotalID == billingCodeSubTotalID))
                {
                    ModelClass.BillingCodeSubTotalVAT billingCodeSubTotalVAT = new ModelClass.BillingCodeSubTotalVAT();

                    billingCodeSubTotalVAT.BillingCodeSubTotalVATID = billingCodeSubTotalVATd.BillingCodeSubTotalVATID;
                    billingCodeSubTotalVAT.BillingCodeSubTotalID = billingCodeSubTotalVATd.BillingCodeSubTotalID;
                    billingCodeSubTotalVAT.VatIdentifier = billingCodeSubTotalVATd.VATIdentifier;
                    billingCodeSubTotalVAT.VatLabel = billingCodeSubTotalVATd.VATLabel;
                    billingCodeSubTotalVAT.VatText = billingCodeSubTotalVATd.VATText;
                    billingCodeSubTotalVAT.VatBaseAmount = Convert.ToDouble(billingCodeSubTotalVATd.VATBaseAmount);
                    billingCodeSubTotalVAT.VatPercentage = Convert.ToDouble(billingCodeSubTotalVATd.VATPercentage);
                    billingCodeSubTotalVAT.VatCalculatedAmount = Convert.ToDouble(billingCodeSubTotalVATd.VATCalculatedAmount);
                    billingCodeSubTotalVAT.CreatedBy = billingCodeSubTotalVATd.CreatedBy;
                    billingCodeSubTotalVAT.CreatedOn = billingCodeSubTotalVATd.CreatedOn;
                    billingCodeSubTotalVAT.LastUpdatedBy = billingCodeSubTotalVATd.LastUpdatedBy;
                    billingCodeSubTotalVAT.LastUpdatedOn = billingCodeSubTotalVATd.LastUpdatedOn;


                    billingCodeSubTotalVATList.Add(billingCodeSubTotalVAT);
                }

                return billingCodeSubTotalVATList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetBillingCodeSubTotalVATList(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetBillingCodeSubTotalVATList()");
                return billingCodeSubTotalVATList;
            }
        }

        /// <summary>
        /// To fetch the Reference Data for the given Airline.
        /// </summary>
        /// <param name="ArilineCode">ArilineCode</param>
        /// <param name="isBillingMember">isBillingMember</param>
        /// <returns>Reference Data For Airline</returns>
        public ModelClass.ReferenceDataModel GetReferenceDataForAirline(string ArilineCode, bool isBillingMember)
        {
            ModelClass.ReferenceDataModel referenceDataForAirline = new ModelClass.ReferenceDataModel();

            try
            {

                var referenceDataForAirlineDB = _sisDB.ReferenceDatas.Where(rd => rd.AirlineCode.Equals(ArilineCode)).FirstOrDefault();

                if (referenceDataForAirlineDB != null)
                {
                    referenceDataForAirline.ReferenceDataID = referenceDataForAirlineDB.ReferenceDataID;
                    referenceDataForAirline.AirlineCode = Convert.ToString(referenceDataForAirlineDB.AirlineCode).PadLeft(3, '0');
                    referenceDataForAirline.CompanyLegalName = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.CompanyLegalName) ? "CompanyLegalName" : referenceDataForAirlineDB.CompanyLegalName;
                    referenceDataForAirline.TaxVATRegistrationID = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.TaxVATRegistrationID) ? "TVRID" : referenceDataForAirlineDB.TaxVATRegistrationID;
                    referenceDataForAirline.AdditionalTaxVATRegistrationID = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.AdditionalTaxVATRegistrationID) ? "ATVRID" : referenceDataForAirlineDB.AdditionalTaxVATRegistrationID;
                    referenceDataForAirline.CompanyRegistrationID = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.CompanyRegistrationID) ? "CRID" : referenceDataForAirlineDB.CompanyRegistrationID;
                    referenceDataForAirline.AddressLine1 = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.AddressLine1) ? "AddressLine1" : referenceDataForAirlineDB.AddressLine1;
                    referenceDataForAirline.AddressLine2 = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.AddressLine2) ? "AddressLine2" : referenceDataForAirlineDB.AddressLine2;
                    referenceDataForAirline.AddressLine3 = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.AddressLine3) ? "AddressLine3" : referenceDataForAirlineDB.AddressLine3;
                    referenceDataForAirline.CityName = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.CityName) ? "CityName" : referenceDataForAirlineDB.CityName;
                    referenceDataForAirline.SubDivisionCode = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.SubDivisionCode) ? "SDC" : referenceDataForAirlineDB.SubDivisionCode;
                    referenceDataForAirline.SubDivisionName = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.SubDivisionName) ? "SubDivisionName" : referenceDataForAirlineDB.SubDivisionName;
                    referenceDataForAirline.CountryCode = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.CountryCode) ? "CC" : referenceDataForAirlineDB.CountryCode;
                    referenceDataForAirline.CountryName = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.CountryName) ? "CountryName" : referenceDataForAirlineDB.CountryName;
                    referenceDataForAirline.PostalCode = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.PostalCode) ? "PostalCode" : referenceDataForAirlineDB.PostalCode;
                    referenceDataForAirline.OrganizationDesignator = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.OrganizationDesignator) ? "OD" : referenceDataForAirlineDB.OrganizationDesignator;
                    referenceDataForAirline.IsBillingMember = isBillingMember;
                    referenceDataForAirline.CreatedBy = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.CreatedBy) ? "Not Found" : referenceDataForAirlineDB.CreatedBy;
                    referenceDataForAirline.CreatedOn = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.CreatedOn.ToString()) ? DateTime.Now : referenceDataForAirlineDB.CreatedOn;
                    referenceDataForAirline.LastUpdatedBy = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.LastUpdatedBy) ? "Not Found" : referenceDataForAirlineDB.LastUpdatedBy;
                    referenceDataForAirline.LastUpdatedOn = string.IsNullOrWhiteSpace(referenceDataForAirlineDB.LastUpdatedOn.ToString()) ? DateTime.Now : referenceDataForAirlineDB.LastUpdatedOn;
                }
                else
                {
                    referenceDataForAirline.ReferenceDataID = isBillingMember ? 1 : 2;
                    referenceDataForAirline.AirlineCode = ArilineCode.PadLeft(3, '0');
                    referenceDataForAirline.CompanyLegalName = "CompanyLegalName";
                    referenceDataForAirline.TaxVATRegistrationID = "TVIRD";
                    referenceDataForAirline.AdditionalTaxVATRegistrationID = "ATVRID";
                    referenceDataForAirline.CompanyRegistrationID = "CRID";
                    referenceDataForAirline.AddressLine1 = "AddressLine1";
                    referenceDataForAirline.AddressLine2 = "AddressLine2";
                    referenceDataForAirline.AddressLine3 = "AddressLine3";
                    referenceDataForAirline.CityName = "CityName";
                    referenceDataForAirline.SubDivisionCode = "SubDivisionCode";
                    referenceDataForAirline.SubDivisionName = "SubDivisionName";
                    referenceDataForAirline.CountryCode = "CC";
                    referenceDataForAirline.CountryName = "CountryName";
                    referenceDataForAirline.PostalCode = "PostalCode";
                    referenceDataForAirline.OrganizationDesignator = "OD";
                    referenceDataForAirline.IsBillingMember = isBillingMember;
                    referenceDataForAirline.CreatedBy = "Not Found";
                    referenceDataForAirline.CreatedOn = DateTime.Now;
                    referenceDataForAirline.LastUpdatedBy = "Not Found";
                    referenceDataForAirline.LastUpdatedOn = DateTime.Now;
                }

                return referenceDataForAirline;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetReferenceDataForAirline(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetReferenceDataForAirline()");
                return referenceDataForAirline;
            }
        }

        /// <summary>
        /// Get the InvoiceTotal from Database.
        /// </summary>
        /// <param name="fileHeaderID">invoiceHeaderID Foreign Key</param>
        /// <returns>InvoiceTotal Database Record for the given invoiceHeaderID</returns>
        public ModelClass.InvoiceTotal GetInvoiceTotal(int invoiceHeaderID)
        {
            ModelClass.InvoiceTotal invoiceTotal = new ModelClass.InvoiceTotal();

            try
            {

                var invoiceTotald = _sisDB.InvoiceTotals.Where(invt => invt.InvoiceHeaderID == invoiceHeaderID).FirstOrDefault();

                if (invoiceTotald != null)
                {
                    invoiceTotal.InvoiceTotalID = invoiceTotald.InvoiceTotalID;
                    invoiceTotal.InvoiceHeaderID = invoiceTotald.InvoiceHeaderID;
                    invoiceTotal.TotalNumberOfBillingRecords = Convert.ToInt32(invoiceTotald.TotalNumberOfBillingRecords);
                    invoiceTotal.TotalNetAmountWithoutVat = Convert.ToDecimal(invoiceTotald.TotalNetAmountWithoutVAT);
                    invoiceTotal.NetInvoiceBillingTotal = Convert.ToDecimal(invoiceTotald.NetInvoiceBillingTotal);
                    invoiceTotal.TotalVATAmount = Convert.ToDecimal(invoiceTotald.TotalVATAmount);
                    invoiceTotal.NetInvoiceTotal = Convert.ToDecimal(invoiceTotald.NetInvoiceTotal);
                    invoiceTotal.TotalInterlineServiceChargeAmount = Convert.ToDecimal(invoiceTotald.TotalInterlineServiceChargeAmount);
                    invoiceTotal.TotalWeightCharges = Convert.ToDecimal(invoiceTotald.TotalWeightCharges);
                    invoiceTotal.TotalValuationCharges = Convert.ToDecimal(invoiceTotald.TotalValuationCharges);
                    invoiceTotal.TotalOtherCharges = Convert.ToDecimal(invoiceTotald.TotalOtherCharges);
                    invoiceTotal.TotalNumberOfRecords = Convert.ToInt32(invoiceTotald.TotalNumberOfRecords);
                    invoiceTotal.CreatedBy = invoiceTotald.CreatedBy;
                    invoiceTotal.CreatedOn = invoiceTotald.CreatedOn;
                    invoiceTotal.LastUpdatedBy = invoiceTotald.LastUpdatedBy;
                    invoiceTotal.LastUpdatedOn = invoiceTotald.LastUpdatedOn;
                }

                return invoiceTotal;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetInvoiceTotal(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetInvoiceTotal()");
                return invoiceTotal;
            }
        }

        #endregion

        /// <summary>
        /// Get the FileTotal from Database.
        /// </summary>
        /// <param name="fileHeaderID">fileHeaderID Foreign Key</param>
        /// <returns>FileTotal Database Record for the given fileHeaderID</returns>
        public ModelClass.FileTotal GetFileTotal(int fileHeaderID)
        {
            ModelClass.FileTotal fileTotal = new ModelClass.FileTotal();

            try
            {

                var fileTotald = _sisDB.FileTotals.Where(ft => ft.FileHeaderID == fileHeaderID).FirstOrDefault();

                if (fileTotald != null)
                {
                    fileTotal.FileTotalID = fileTotald.FileTotalID;
                    fileTotal.FileHeaderID = Convert.ToInt32(fileTotald.FileHeaderID);
                    fileTotal.TotalWeightCharges = Convert.ToDecimal(fileTotald.TotalWeightCharges);
                    fileTotal.TotalOtherCharges = Convert.ToDecimal(fileTotald.TotalOtherCharges);
                    fileTotal.TotalInterlineServiceChargeAmount = Convert.ToDecimal(fileTotald.TotalInterlineServiceChargeAmount);
                    fileTotal.FileTotalOfNetInvoiceTotal = Convert.ToDecimal(fileTotald.FileTotalOfNetInvoiceTotal);
                    fileTotal.FileTotalOfNetInvoiceBillingTotal = Convert.ToDecimal(fileTotald.FileTotalOfNetInvoiceBillingTotal);
                    fileTotal.TotalNumberOfBillingRecords = fileTotald.TotalNumberOfBillingRecords;
                    fileTotal.TotalValuationCharges = Convert.ToDecimal(fileTotald.TotalValuationCharges);
                    fileTotal.TotalVatAmount = Convert.ToDecimal(fileTotald.TotalVATAmount);
                    fileTotal.TotalNumberOfRecords = fileTotald.TotalNumberOfRecords;
                    fileTotal.CreatedBy = fileTotald.CreatedBy;
                    fileTotal.CreatedOn = fileTotald.CreatedOn;
                    fileTotal.LastUpdatedBy = fileTotald.LastUpdatedBy;
                    fileTotal.LastUpdatedOn = fileTotald.LastUpdatedOn;
                }

                return fileTotal;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetFileTotal(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetFileTotal()");
                return fileTotal;
            }
        }

        /// <summary>
        /// Get the Current Open Billing Period from Database.
        /// </summary>
        /// <param name="dateTime">Date time UTC</param>
        /// <param name="clearingHouse">Clearing House: I or A</param>
        /// <returns>Billing Period from CalanderMaster for the given Date & Clearing House</returns>
        public DateTime GetCurrentOpenBillingPeriod(DateTime dateTime, string clearingHouse)
        {
            try
            {
                var currentOpenBillingPeriod = _sisDB.CalendarMasters.Where(cm => cm.ClearingHouse.Equals(clearingHouse) &&
                                                                      dateTime >= cm.PeriodStartDate &&
                                                                      dateTime <= cm.PeriodEndDate).FirstOrDefault().BillingPeriod;

                return currentOpenBillingPeriod;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetCurrentOpenBillingPeriod(), Error Messaage: {0}, Exception: {1}", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetCurrentOpenBillingPeriod()");
                return Convert.ToDateTime("1900-01-01");
            }
        }

        /// <summary>
        /// To check the file name against original inovice file name which was uploaded to SIS
        /// </summary>
        /// <param name="fileName">Validation file name</param>
        /// <returns>file Status Id </returns>
        public int IsOriginalFileExists(string fileName, ref int receivablesFileID)
        {
            var fileHeader = _sisDB.FileHeaders.FirstOrDefault(fh => fh.FileName.ToUpper().Equals(fileName) && fh.FileInOutDirection == 1);

            if (fileHeader != null)
            {
                receivablesFileID = fileHeader.FileHeaderID;
                return fileHeader.FileStatusID;
            }
            else { return -9; }
        }

        /// <summary>
        /// Method to check File already exist.
        /// </summary>
        /// <param name="fileName"> Payables File Name</param>
        /// <returns></returns>
        public bool IsFileAlreadyExistsWithSameName(string fileName)
        {
            var fileHeader = _sisDB.FileHeaders.FirstOrDefault(fh => fh.FileName.ToUpper().Equals(fileName) && fh.FileInOutDirection == 0 && fh.IsProcessed == true);

            if (fileHeader != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method to check Invoice already exist.
        /// </summary>
        /// <param name="invoiceHeader"> Payables Invoice</param>
        /// <returns></returns>
        public bool IsInvoiceAlreadyExists(ModelClass.InvoiceHeader invoiceHeader)
        {
            var dbInvoiceHeader = _sisDB.InvoiceHeaders.FirstOrDefault(ih => ih.InvoiceNumber.ToUpper().Equals(invoiceHeader.InvoiceNumber) &&
                                                               ih.BillingAirline.Equals(invoiceHeader.BillingAirline) &&
                                                               ih.BilledAirline.Equals(invoiceHeader.BilledAirline) &&
                                                               ih.BillingYear == invoiceHeader.BillingYear);

            if (dbInvoiceHeader != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get InvoiceHeader IDs from input FileHeaderID
        /// </summary>
        /// <param name="fileHeaderId"></param>
        /// <returns></returns>
        public List<int> GetInvoiceHeaderIDFromFileHeaderId(int fileHeaderId)
        {
            List<int> invoiceHeaderIds = new List<int>();
            var invoiceHeaderIdsDB = _sisDB.InvoiceHeaders.Where(ih => ih.FileHeaderID == fileHeaderId).Select(ihid => ihid.InvoiceHeaderID);
            foreach (var invoiceHeaderId in invoiceHeaderIdsDB)
            {
                invoiceHeaderIds.Add(Convert.ToInt32(invoiceHeaderId));
            }

            return invoiceHeaderIds;
        }
        public List<FileHeader> GetUnProcessedSISFiles()
        {
            List<FileHeader> FileHeaderList = new List<FileHeader>();
            try
            {
                foreach (var fileHeader in _sisDB.FileHeaders.Where(fileheader => fileheader.IsProcessed == false && fileheader.ReadWriteOnSFTP != null))
                {
                    FileHeader objFileHeader = new FileHeader
                    {
                        AirlineCode = fileHeader.AirlineCode,
                        FileName = fileHeader.FileName,
                        FilePath = fileHeader.FilePath,
                        FileHeaderID = fileHeader.FileHeaderID
                    };
                    FileHeaderList.Add(objFileHeader);
                }

                return FileHeaderList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetUnProcessedFileData()", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetUnProcessedSISFiles()");
                return FileHeaderList;
            }
        }
        public List<ISValidationFileHeader> GetUnProcessedSISValidationFiles()
        {
            List<ISValidationFileHeader> ValidationFileHeaderList = new List<ISValidationFileHeader>();
            try
            {
                foreach (var validationFileHeader in _sisDB.ISValidationFileHeaders.Where(fileheader => fileheader.IsProcessed == false && fileheader.ReadWriteOnSFTP != null))
                {
                    ISValidationFileHeader valFileHeader = new ISValidationFileHeader
                    {
                        FileName = validationFileHeader.FileName,
                        FilePath = validationFileHeader.FilePath,
                        ID = validationFileHeader.ID,
                        ReceivablesFileHeaderID = validationFileHeader.ReceivablesFileHeaderID
                    };
                    ValidationFileHeaderList.Add(valFileHeader);
                }

                return ValidationFileHeaderList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetUnProcessedSISValidationFiles()", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetUnProcessedSISValidationFiles()");
                return ValidationFileHeaderList;
            }
        }

        public List<FileHeader> GetUnProcessedSISReceivableFiles()
        {
            List<FileHeader> FileHeaderList = new List<FileHeader>();
            try
            {
                foreach (var fileHeader in _sisDB.FileHeaders.Where(fileheader => (fileheader.IsProcessed == false || fileheader.IsProcessed == null) && fileheader.ReadWriteOnSFTP == null && fileheader.FileInOutDirection == 1 && fileheader.FileStatusID == 1))
                {
                    FileHeader objFileHeader = new FileHeader
                    {
                        AirlineCode = fileHeader.AirlineCode,
                        FileName = fileHeader.FileName,
                        FilePath = fileHeader.FilePath,
                        FileHeaderID = fileHeader.FileHeaderID
                    };
                    FileHeaderList.Add(objFileHeader);
                }

                return FileHeaderList;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Exception occurred in ReadDBData.cs, GetUnProcessedSISReceivableFiles()", exception);
                _logger.LogError(exception, "Exception occurred in ReadDBData.cs, GetUnProcessedSISReceivableFiles()");
                return FileHeaderList;
            }
        }
    }
}