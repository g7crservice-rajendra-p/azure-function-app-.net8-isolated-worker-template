using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ModelClass = QidWorkerRole.SIS.Model;
using DbEntity = QidWorkerRole.SIS.DAL;
using System.Data.Entity.Validation;



namespace QidWorkerRole.SIS.DAL
{
    /// <summary>
    /// Performs all Database Create/Insert Operations.
    /// </summary>
    public class CreateDBData
    {
        // For Logging.


        public SIS.DAL.SISDBEntities _sisDB;

        // Constants for Last Updated By and Created By
        private const string ByFileReader = "FileReaderSISAutomation";
        private const string ByFileWriter = "FileWriterSISAutomation";

        public CreateDBData()
        {
            _sisDB = new SIS.DAL.SISDBEntities();
        }

        /// <summary>
        /// Insert File Header and File Total Data.
        /// </summary>
        /// <param name="invoiceList">List of Invoices</param>
        /// <param name="airlineCode">Billing Airline Code</param>
        /// <returns>Updated FileHeaderID List of Invoices.</returns>
        public List<ModelClass.Invoice> InsertFileData(List<ModelClass.Invoice> invoiceList, string airlineCode)
        {
            DbEntity.FileHeader newFileHeader = new DbEntity.FileHeader();

            newFileHeader.AirlineCode = airlineCode;
            newFileHeader.VersionNumber = 0320;
            newFileHeader.FileInOutDirection = 1;
            newFileHeader.CreatedBy = ByFileWriter;
            newFileHeader.CreatedOn = DateTime.UtcNow;
            newFileHeader.LastUpdatedBy = ByFileWriter;
            newFileHeader.LastUpdatedOn = DateTime.UtcNow;

            _sisDB.FileHeaders.Add(newFileHeader);
            _sisDB.SaveChanges();

            int newFileHeaderId = _sisDB.FileHeaders.Max(fh => fh.FileHeaderID);

            //Logger.InfoFormat("End of Insert File Header Data for FileHeaderID: {0}", newFileHeaderId);

            List<ModelClass.Invoice> newInvoiceList = new List<ModelClass.Invoice>();

            DbEntity.FileTotal newFileTotal = new DbEntity.FileTotal();
            newFileTotal.FileHeaderID = newFileHeaderId;
            newFileTotal.TotalWeightCharges = 0;
            newFileTotal.TotalOtherCharges = 0;
            newFileTotal.TotalInterlineServiceChargeAmount = 0;
            newFileTotal.FileTotalOfNetInvoiceTotal = 0;
            newFileTotal.FileTotalOfNetInvoiceBillingTotal = 0;
            newFileTotal.TotalNumberOfBillingRecords = 0;
            newFileTotal.TotalValuationCharges = 0;
            newFileTotal.TotalVATAmount = 0;
            newFileTotal.TotalNumberOfRecords = 2;
            newFileTotal.CreatedBy = ByFileWriter;
            newFileTotal.CreatedOn = DateTime.UtcNow;
            newFileTotal.LastUpdatedBy = ByFileWriter;
            newFileTotal.LastUpdatedOn = DateTime.UtcNow;

            foreach (var invoice in invoiceList)
            {
                DbEntity.InvoiceHeader updatingInvoiceHeader = _sisDB.InvoiceHeaders.First(ih => ih.InvoiceHeaderID == invoice.InvoiceHeaderID);
                updatingInvoiceHeader.FileHeaderID = newFileHeaderId;
                _sisDB.SaveChanges();

                invoice.FileHeaderID = newFileHeaderId;

                newInvoiceList.Add(invoice);

                // Update newFileTotal Values.
                newFileTotal.TotalWeightCharges = newFileTotal.TotalWeightCharges + invoice.InvoiceTotals.TotalWeightCharges;
                newFileTotal.TotalOtherCharges = newFileTotal.TotalOtherCharges + invoice.InvoiceTotals.TotalOtherCharges;
                newFileTotal.TotalInterlineServiceChargeAmount = newFileTotal.TotalInterlineServiceChargeAmount + invoice.InvoiceTotals.TotalInterlineServiceChargeAmount;
                newFileTotal.FileTotalOfNetInvoiceTotal = newFileTotal.FileTotalOfNetInvoiceTotal + invoice.InvoiceTotals.NetInvoiceTotal;
                newFileTotal.FileTotalOfNetInvoiceBillingTotal = newFileTotal.FileTotalOfNetInvoiceBillingTotal + invoice.InvoiceTotals.NetInvoiceBillingTotal;
                newFileTotal.TotalNumberOfBillingRecords = newFileTotal.TotalNumberOfBillingRecords + invoice.InvoiceTotals.TotalNumberOfBillingRecords;
                newFileTotal.TotalValuationCharges = newFileTotal.TotalValuationCharges + invoice.InvoiceTotals.TotalValuationCharges;
                newFileTotal.TotalVATAmount = newFileTotal.TotalVATAmount + invoice.InvoiceTotals.TotalVATAmount;
                newFileTotal.TotalNumberOfRecords = newFileTotal.TotalNumberOfRecords + invoice.InvoiceTotals.TotalNumberOfRecords;
            }

            // add newFileTotal and save.
            _sisDB.FileTotals.Add(newFileTotal);
            _sisDB.SaveChanges();

            return newInvoiceList;
        }


        /// <summary>
        /// To Insert received file data to Databasae.
        /// </summary>
        /// <param name="fileData">FileData</param>
        /// <param name="newFileHeaderId"></param>
        /// <returns></returns>
        public bool InsertReceivedFileData(ModelClass.SupportingModels.FileData fileData, string CreatedBy, out int newFileHeaderIdRetrived)
        {
            using (var context = _sisDB)
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        #region FileHeader

                        var fileHeader = _sisDB.FileHeaders.FirstOrDefault(fh => fh.FileName.ToUpper().Equals(fileData.FileHeader.FileName) && fh.FileInOutDirection == 0 && fh.IsProcessed == false);
                        int newFileHeaderId = 0;
                        if (fileHeader != null)
                        {
                            newFileHeaderId = fileHeader.FileHeaderID;
                        }
                        else
                        {
                            DbEntity.FileHeader newFileHeader = new DbEntity.FileHeader();

                            newFileHeader.AirlineCode = fileData.FileHeader.AirlineCode;
                            newFileHeader.VersionNumber = fileData.FileHeader.VersionNumber;
                            newFileHeader.FileInOutDirection = fileData.FileHeader.FileInOutDirection;
                            newFileHeader.FileName = fileData.FileHeader.FileName;
                            newFileHeader.FilePath = fileData.FileHeader.FilePath;
                            newFileHeader.CreatedBy = fileData.FileHeader.CreatedBy;
                            newFileHeader.CreatedOn = fileData.FileHeader.CreatedOn;
                            newFileHeader.LastUpdatedBy = fileData.FileHeader.LastUpdatedBy;
                            newFileHeader.LastUpdatedOn = DateTime.UtcNow;
                            newFileHeader.FileStatusID = fileData.FileHeader.FileStatusId;

                            _sisDB.FileHeaders.Add(newFileHeader);
                            _sisDB.SaveChanges();

                            //Logger.InfoFormat("End of Insert File Header Data for FileName: {0}", fileData.FileHeader.FileName);

                            #endregion


                            newFileHeaderId = newFileHeader.FileHeaderID;
                        }
                        newFileHeaderIdRetrived = newFileHeaderId;
                        #region FileTotal

                        DbEntity.FileTotal newFileTotal = new DbEntity.FileTotal
                        {
                            FileHeaderID = newFileHeaderIdRetrived,
                            TotalWeightCharges = 0,
                            TotalOtherCharges = 0,
                            TotalInterlineServiceChargeAmount = 0,
                            FileTotalOfNetInvoiceTotal = 0,
                            FileTotalOfNetInvoiceBillingTotal = 0,
                            TotalNumberOfBillingRecords = 0,
                            TotalValuationCharges = 0,
                            TotalVATAmount = 0,
                            TotalNumberOfRecords = 0,
                            CreatedBy = CreatedBy,
                            CreatedOn = DateTime.UtcNow,
                            LastUpdatedBy = ByFileReader,
                            LastUpdatedOn = DateTime.UtcNow
                        };

                        #endregion

                        #region Invoice

                        foreach (var invoice in fileData.InvoiceList)
                        {
                            #region ReferenceData

                            if (invoice.ReferenceDataList != null && invoice.ReferenceDataList.Count > 0)
                            {
                                List<ModelClass.ReferenceDataModel> updateReferenceDataModelList = new List<ModelClass.ReferenceDataModel>();
                                foreach (var referenceData in invoice.ReferenceDataList)
                                {
                                    referenceData.AirlineCode = referenceData.IsBillingMember ? invoice.BillingAirline : invoice.BilledAirline;
                                    updateReferenceDataModelList.Add(referenceData);
                                }

                                foreach (var referenceData in updateReferenceDataModelList)
                                {
                                    var tempRDAirlineCode = referenceData.AirlineCode;
                                    DbEntity.ReferenceData tempRefData = _sisDB.ReferenceDatas.Where(rd => rd.AirlineCode.Equals(tempRDAirlineCode)).FirstOrDefault();

                                    // Do not add Reference Data because, its already present in the database for the Airline.
                                    if (tempRefData == null)
                                    {
                                        DbEntity.ReferenceData newReferenceData = new DbEntity.ReferenceData
                                        {
                                            AirlineCode = referenceData.AirlineCode,
                                            CompanyLegalName = referenceData.CompanyLegalName,
                                            TaxVATRegistrationID = referenceData.TaxVATRegistrationID,
                                            AdditionalTaxVATRegistrationID = referenceData.AdditionalTaxVATRegistrationID,
                                            CompanyRegistrationID = referenceData.CompanyRegistrationID,
                                            AddressLine1 = referenceData.AddressLine1,
                                            AddressLine2 = referenceData.AddressLine2,
                                            AddressLine3 = referenceData.AddressLine3,
                                            CityName = referenceData.CityName,
                                            SubDivisionCode = referenceData.SubDivisionCode,
                                            SubDivisionName = referenceData.SubDivisionName,
                                            CountryCode = referenceData.CountryCode,
                                            CountryName = referenceData.CountryName,
                                            PostalCode = referenceData.PostalCode,
                                            OrganizationDesignator = referenceData.OrganizationDesignator,
                                            CreatedBy = CreatedBy,
                                            CreatedOn = DateTime.UtcNow,
                                            LastUpdatedBy = ByFileReader,
                                            LastUpdatedOn = DateTime.UtcNow
                                        };

                                        _sisDB.ReferenceDatas.Add(newReferenceData);
                                        _sisDB.SaveChanges();
                                    }
                                }
                            }

                            //Logger.InfoFormat("End of Insert Reference Data for InvoiceNumber: {0}, FileName: {1}", invoice.InvoiceNumber, fileData.FileHeader.FileName);

                            #endregion

                            #region InvoiceHeader

                            DbEntity.InvoiceHeader newInvoiceHeader = new DbEntity.InvoiceHeader
                            {
                                FileHeaderID = newFileHeaderId,
                                BillingAirline = invoice.BillingAirline,
                                BilledAirline = invoice.BilledAirline,
                                InvoiceNumber = invoice.InvoiceNumber,
                                BillingYear = invoice.BillingYear,
                                BillingMonth = invoice.BillingMonth,
                                PeriodNumber = invoice.PeriodNumber,
                                CurrencyofListing = invoice.CurrencyofListing,
                                CurrencyofBilling = invoice.CurrencyofBilling,
                                SettlementMethodIndicator = invoice.SettlementMethodIndicator,
                                DigitalSignatureFlag = invoice.DigitalSignatureFlag,
                                InvoiceDate = invoice.InvoiceDate,
                                ListingtoBillingRate = invoice.ListingToBillingRate,
                                SuspendedInvoiceFlag = invoice.SuspendedInvoiceFlag,
                                BillingAirlineLocationID = invoice.BillingAirlineLocationID,
                                BilledAirlineLocationID = invoice.BilledAirlineLocationID,
                                InvoiceType = invoice.InvoiceType,
                                InvoiceTemplateLanguage = invoice.InvoiceTemplateLanguage,
                                CHDueDate = invoice.ChDueDate,
                                CHAgreementIndicator = invoice.ChAgreementIndicator,
                                InvoiceFooterDetails = invoice.InvoiceFooterDetails,
                                CreatedBy = CreatedBy,
                                CreatedOn = DateTime.UtcNow,
                                LastUpdatedBy = ByFileReader,
                                LastUpdatedOn = DateTime.UtcNow,
                                InvoiceStatusId = invoice.InvoiceStatusId,
                                IsReceivedFromFile = invoice.IsReceivedFromFile,
                                IsSIS = invoice.IsSIS
                            };

                            _sisDB.InvoiceHeaders.Add(newInvoiceHeader);
                            _sisDB.SaveChanges();

                            //Logger.InfoFormat("End of Insert InvoiceHeader for InvoiceNumber: {0}, FileName: {1}", invoice.InvoiceNumber, fileData.FileHeader.FileName);

                            // Update newFileTotal Values.
                            newFileTotal.TotalWeightCharges = newFileTotal.TotalWeightCharges + invoice.InvoiceTotals.TotalWeightCharges;
                            newFileTotal.TotalOtherCharges = newFileTotal.TotalOtherCharges + invoice.InvoiceTotals.TotalOtherCharges;
                            newFileTotal.TotalInterlineServiceChargeAmount = newFileTotal.TotalInterlineServiceChargeAmount + invoice.InvoiceTotals.TotalInterlineServiceChargeAmount;
                            newFileTotal.FileTotalOfNetInvoiceTotal = newFileTotal.FileTotalOfNetInvoiceTotal + invoice.InvoiceTotals.NetInvoiceTotal;
                            newFileTotal.FileTotalOfNetInvoiceBillingTotal = newFileTotal.FileTotalOfNetInvoiceBillingTotal + invoice.InvoiceTotals.NetInvoiceBillingTotal;
                            newFileTotal.TotalNumberOfBillingRecords = newFileTotal.TotalNumberOfBillingRecords + invoice.InvoiceTotals.TotalNumberOfBillingRecords;
                            newFileTotal.TotalValuationCharges = newFileTotal.TotalValuationCharges + invoice.InvoiceTotals.TotalValuationCharges;
                            newFileTotal.TotalVATAmount = newFileTotal.TotalVATAmount + invoice.InvoiceTotals.TotalVATAmount;
                            newFileTotal.TotalNumberOfRecords = newFileTotal.TotalNumberOfRecords + invoice.InvoiceTotals.TotalNumberOfRecords;

                            #endregion

                            int newInvoiceHeaderId = _sisDB.InvoiceHeaders.FirstOrDefault(ih => ih.FileHeaderID == newFileHeaderId && ih.InvoiceNumber.Equals(newInvoiceHeader.InvoiceNumber)
                                                                                                                                   && ih.BillingAirline.Equals(newInvoiceHeader.BillingAirline)).InvoiceHeaderID;

                            #region AirWayBill, AwbOtherCharges & AwbVat

                            if (invoice.AirWayBillList != null && invoice.AirWayBillList.Count > 0)
                            {
                                foreach (var airWayBill in invoice.AirWayBillList)
                                {
                                    #region AirWayBill

                                    DbEntity.AirWayBill newAirWayBill = new DbEntity.AirWayBill();

                                    newAirWayBill.InvoiceHeaderID = newInvoiceHeaderId;
                                    newAirWayBill.BillingCode = airWayBill.BillingCode;
                                    newAirWayBill.BatchSequenceNumber = airWayBill.BatchSequenceNumber;
                                    newAirWayBill.RecordSequencewithinBatch = airWayBill.RecordSequenceWithinBatch;
                                    newAirWayBill.AWBDate = Convert.ToDateTime(airWayBill.AWBDate);
                                    newAirWayBill.AWBIssuingAirline = airWayBill.AWBIssuingAirline;
                                    newAirWayBill.AWBSerialNumber = airWayBill.AWBSerialNumber;
                                    newAirWayBill.AWBCheckDigit = airWayBill.AWBCheckDigit;
                                    newAirWayBill.Origin = airWayBill.Origin;
                                    newAirWayBill.Destination = airWayBill.Destination;
                                    newAirWayBill.From = airWayBill.From;
                                    newAirWayBill.To = airWayBill.To;
                                    newAirWayBill.DateOfCarriage = Convert.ToDecimal(airWayBill.DateOfCarriageOrTransfer);
                                    newAirWayBill.WeightCharges = Convert.ToDecimal(airWayBill.WeightCharges);
                                    newAirWayBill.OtherCharges = Convert.ToDecimal(airWayBill.OtherCharges);
                                    newAirWayBill.AmountSubjectToInterlineServiceCharge = Convert.ToDecimal(airWayBill.AmountSubjectToInterlineServiceCharge);
                                    newAirWayBill.InterlineServiceChargePercentage = Convert.ToDecimal(airWayBill.InterlineServiceChargePercentage);
                                    newAirWayBill.CurrencyAdjustmentIndicator = airWayBill.CurrencyAdjustmentIndicator;
                                    newAirWayBill.BilledWeight = Convert.ToDecimal(airWayBill.BilledWeight);
                                    newAirWayBill.ProvisoReqSPA = airWayBill.ProvisoReqSPA;
                                    newAirWayBill.ProratePercentage = airWayBill.ProratePercentage;
                                    newAirWayBill.PartShipmentIndicator = airWayBill.PartShipmentIndicator;
                                    newAirWayBill.ValuationCharges = Convert.ToDecimal(airWayBill.ValuationCharges);
                                    newAirWayBill.KGLBIndicator = airWayBill.KGLBIndicator;
                                    newAirWayBill.VATAmount = Convert.ToDecimal(airWayBill.VATAmount);
                                    newAirWayBill.InterlineServiceChargeAmount = Convert.ToDecimal(airWayBill.InterlineServiceChargeAmount);
                                    newAirWayBill.AWBTotalAmount = Convert.ToDecimal(airWayBill.AWBTotalAmount);
                                    newAirWayBill.CCAindicator = airWayBill.CCAindicator ? "Y" : string.Empty;
                                    newAirWayBill.OurReference = airWayBill.OurReference;
                                    newAirWayBill.AttachmentIndicatorOriginal = airWayBill.AttachmentIndicatorOriginal;
                                    newAirWayBill.AttachmentIndicatorValidated = airWayBill.AttachmentIndicatorValidated;
                                    newAirWayBill.NumberOfAttachments = airWayBill.NumberOfAttachments;
                                    newAirWayBill.ISValidationFlag = airWayBill.ISValidationFlag;
                                    newAirWayBill.ReasonCode = airWayBill.ReasonCode;
                                    newAirWayBill.ReferenceField1 = airWayBill.ReferenceField1;
                                    newAirWayBill.ReferenceField2 = airWayBill.ReferenceField2;
                                    newAirWayBill.ReferenceField3 = airWayBill.ReferenceField3;
                                    newAirWayBill.ReferenceField4 = airWayBill.ReferenceField4;
                                    newAirWayBill.ReferenceField5 = airWayBill.ReferenceField5;
                                    newAirWayBill.AirlineOwnUse = airWayBill.AirlineOwnUse;
                                    newAirWayBill.CreatedBy = CreatedBy;
                                    newAirWayBill.CreatedOn = DateTime.UtcNow;
                                    newAirWayBill.LastUpdatedBy = ByFileReader;
                                    newAirWayBill.LastUpdatedOn = DateTime.UtcNow;
                                    newAirWayBill.AWBStatusID = 8; // 8 = IS Validated

                                    _sisDB.AirWayBills.Add(newAirWayBill);
                                    _sisDB.SaveChanges();

                                    #endregion

                                    int newAirWayBillId = _sisDB.AirWayBills.FirstOrDefault(awb => awb.InvoiceHeaderID == newInvoiceHeaderId && awb.AWBSerialNumber == newAirWayBill.AWBSerialNumber).AirWayBillID;

                                    #region AWBOtherCharges

                                    if (airWayBill.AWBOtherChargesList != null && airWayBill.AWBOtherChargesList.Count > 0)
                                    {
                                        foreach (var aWBOtherCharges in airWayBill.AWBOtherChargesList)
                                        {
                                            DbEntity.AWBOtherCharge newAWBOtherCharge = new DbEntity.AWBOtherCharge();

                                            newAWBOtherCharge.AirWayBillID = newAirWayBillId;
                                            newAWBOtherCharge.OtherChargeCode = aWBOtherCharges.OtherChargeCode;
                                            newAWBOtherCharge.OtherChargeCodeValue = Convert.ToDecimal(aWBOtherCharges.OtherChargeCodeValue);
                                            newAWBOtherCharge.VATLabel = aWBOtherCharges.OtherChargeVatLabel;
                                            newAWBOtherCharge.VATText = aWBOtherCharges.OtherChargeVatText;
                                            newAWBOtherCharge.VATbaseAmount = Convert.ToDecimal(aWBOtherCharges.OtherChargeVatBaseAmount);
                                            newAWBOtherCharge.VATPercentage = Convert.ToDecimal(aWBOtherCharges.OtherChargeVatPercentage);
                                            newAWBOtherCharge.VATCalculatedAmount = Convert.ToDecimal(aWBOtherCharges.OtherChargeVatCalculatedAmount);
                                            newAWBOtherCharge.CreatedBy = CreatedBy;
                                            newAWBOtherCharge.CreatedOn = DateTime.UtcNow;
                                            newAWBOtherCharge.LastUpdatedBy = ByFileReader;
                                            newAWBOtherCharge.LastUpdatedOn = DateTime.UtcNow;

                                            _sisDB.AWBOtherCharges.Add(newAWBOtherCharge);
                                            _sisDB.SaveChanges();
                                        }
                                    }

                                    #endregion

                                    #region AWBVAT

                                    if (airWayBill.AWBVATList != null && airWayBill.AWBVATList.Count > 0)
                                    {
                                        foreach (var aWBVAT in airWayBill.AWBVATList)
                                        {
                                            DbEntity.AWBVAT newAWBVAT = new DbEntity.AWBVAT();

                                            newAWBVAT.AirWayBillID = newAirWayBillId;
                                            newAWBVAT.VATIdentifier = aWBVAT.VatIdentifier;
                                            newAWBVAT.VATLabel = aWBVAT.VatLabel;
                                            newAWBVAT.VATText = aWBVAT.VatText;
                                            newAWBVAT.VATBaseAmount = Convert.ToDecimal(aWBVAT.VatBaseAmount);
                                            newAWBVAT.VATPercentage = Convert.ToDecimal(aWBVAT.VatPercentage);
                                            newAWBVAT.VATCalculatedAmount = Convert.ToDecimal(aWBVAT.VatCalculatedAmount);
                                            newAWBVAT.CreatedBy = CreatedBy;
                                            newAWBVAT.CreatedOn = DateTime.UtcNow;
                                            newAWBVAT.LastUpdatedBy = ByFileReader;
                                            newAWBVAT.LastUpdatedOn = DateTime.UtcNow;

                                            _sisDB.AWBVATs.Add(newAWBVAT);
                                            _sisDB.SaveChanges();
                                        }
                                    }

                                    #endregion

                                }
                            }

                            #endregion

                            #region RejectionMemo

                            if (invoice.RejectionMemoList != null && invoice.RejectionMemoList.Count > 0)
                            {
                                foreach (var rejectionMemo in invoice.RejectionMemoList)
                                {
                                    DbEntity.RejectionMemo newRejectionMemo = new DbEntity.RejectionMemo();

                                    newRejectionMemo.InvoiceHeaderID = newInvoiceHeaderId;
                                    newRejectionMemo.BillingCode = rejectionMemo.BillingCode;
                                    newRejectionMemo.BatchsequenceNumber = rejectionMemo.BatchSequenceNumber;
                                    newRejectionMemo.RecordSequenceWithinBatch = rejectionMemo.RecordSequenceWithinBatch;
                                    newRejectionMemo.RejectionMemoNumber = rejectionMemo.RejectionMemoNumber;
                                    newRejectionMemo.RejectionStage = rejectionMemo.RejectionStage;
                                    newRejectionMemo.ReasonCode = rejectionMemo.ReasonCode;
                                    newRejectionMemo.AirlineOwnUse = rejectionMemo.AirlineOwnUse;
                                    newRejectionMemo.YourInvoiceNumber = rejectionMemo.YourInvoiceNumber;
                                    newRejectionMemo.YourInvoiceBillingYear = rejectionMemo.YourInvoiceBillingYear;
                                    newRejectionMemo.YourInvoiceBillingMonth = rejectionMemo.YourInvoiceBillingMonth;
                                    newRejectionMemo.YourInvoiceBillingPeriod = rejectionMemo.YourInvoiceBillingPeriod;
                                    newRejectionMemo.YourRejectionMemoNumber = rejectionMemo.YourRejectionNumber;
                                    newRejectionMemo.YourBMCMNumber = rejectionMemo.YourBillingMemoNumber;
                                    newRejectionMemo.TotalWeightChargesBilled = rejectionMemo.BilledTotalWeightCharge;
                                    newRejectionMemo.TotalWeightChargesAccepted = rejectionMemo.AcceptedTotalWeightCharge;
                                    newRejectionMemo.TotalWeightChargesDifference = rejectionMemo.TotalWeightChargeDifference;
                                    newRejectionMemo.TotalValuationChargesBilled = rejectionMemo.BilledTotalValuationCharge;
                                    newRejectionMemo.TotalValuationChargesAccepted = rejectionMemo.AcceptedTotalValuationCharge;
                                    newRejectionMemo.TotalValuationChargesDifference = rejectionMemo.TotalValuationChargeDifference;
                                    newRejectionMemo.TotalOtherChargesAmountBilled = rejectionMemo.BilledTotalOtherChargeAmount;
                                    newRejectionMemo.TotalOtherChargesAmountAccepted = rejectionMemo.AcceptedTotalOtherChargeAmount;
                                    newRejectionMemo.TotalOtherChargesDifference = rejectionMemo.TotalOtherChargeDifference;
                                    newRejectionMemo.TotalISCAmountAllowed = rejectionMemo.AllowedTotalIscAmount;
                                    newRejectionMemo.TotalISCAmountAccepted = rejectionMemo.AcceptedTotalIscAmount;
                                    newRejectionMemo.TotalISCAmountDifference = rejectionMemo.TotalIscAmountDifference;
                                    newRejectionMemo.TotalVATAmountBilled = rejectionMemo.BilledTotalVatAmount;
                                    newRejectionMemo.TotalVATAmountAccepted = rejectionMemo.AcceptedTotalVatAmount;
                                    newRejectionMemo.TotalVATAmountDifference = Convert.ToDecimal(rejectionMemo.TotalVatAmountDifference);
                                    newRejectionMemo.TotalNetRejectAmount = Convert.ToDecimal(rejectionMemo.TotalNetRejectAmount);
                                    newRejectionMemo.AttachmentIndicatorOriginal = rejectionMemo.AttachmentIndicatorOriginal ? "Y" : string.Empty;
                                    newRejectionMemo.AttachmentIndicatorValidated = Convert.ToBoolean(rejectionMemo.AttachmentIndicatorValidated) ? "Y" : string.Empty;
                                    newRejectionMemo.NumberOfAttachments = rejectionMemo.NumberOfAttachments;
                                    newRejectionMemo.ISValidationFlag = rejectionMemo.ISValidationFlag;
                                    newRejectionMemo.BMCMIndicator = rejectionMemo.BMCMIndicator;
                                    newRejectionMemo.OurRef = rejectionMemo.OurRef;
                                    newRejectionMemo.ReasonRemarks = rejectionMemo.ReasonRemarks;
                                    newRejectionMemo.CreatedBy = CreatedBy;
                                    newRejectionMemo.CreatedOn = DateTime.UtcNow;
                                    newRejectionMemo.LastUpdatedBy = ByFileReader;
                                    newRejectionMemo.LastUpdatedOn = DateTime.UtcNow;
                                    newRejectionMemo.CorrespondenceRefNo = rejectionMemo.CorrespondenceRefNo;

                                    _sisDB.RejectionMemoes.Add(newRejectionMemo);
                                    _sisDB.SaveChanges();

                                    //Logger.InfoFormat("End of Insert RejectionMemo for RejectionMemoNumber: {0}, InvoiceNumber: {1}, FileName: {2}", rejectionMemo.RejectionMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);

                                    int newRejectionMemoId = _sisDB.RejectionMemoes.FirstOrDefault(rm => rm.InvoiceHeaderID == newInvoiceHeaderId && rm.RejectionMemoNumber.Equals(newRejectionMemo.RejectionMemoNumber)).RejectionMemoID;

                                    #region RMVAT

                                    if (rejectionMemo.RMVATList != null && rejectionMemo.RMVATList.Count > 0)
                                    {
                                        foreach (var rMVAT in rejectionMemo.RMVATList)
                                        {
                                            DbEntity.RMVAT newRMVAT = new DbEntity.RMVAT();

                                            newRMVAT.RejectionMemoID = newRejectionMemoId;
                                            newRMVAT.VATIdentifier = rMVAT.VatIdentifier;
                                            newRMVAT.VATLabel = rMVAT.VatLabel;
                                            newRMVAT.VATText = rMVAT.VatText;
                                            newRMVAT.VATBaseAmount = Convert.ToDecimal(rMVAT.VatBaseAmount);
                                            newRMVAT.VATPercentage = Convert.ToDecimal(rMVAT.VatPercentage);
                                            newRMVAT.VATCalculatedAmount = Convert.ToDecimal(rMVAT.VatCalculatedAmount);
                                            newRMVAT.CreatedBy = CreatedBy;
                                            newRMVAT.CreatedOn = DateTime.UtcNow;
                                            newRMVAT.LastUpdatedBy = ByFileReader;
                                            newRMVAT.LastUpdatedOn = DateTime.UtcNow;

                                            _sisDB.RMVATs.Add(newRMVAT);
                                            _sisDB.SaveChanges();

                                            //Logger.InfoFormat("End of Insert RMVAT for RMVAT Identifier: {0}, RejectionMemo: {1}, InvoiceNumber: {2}, FileName: {3}", rMVAT.VatIdentifier, rejectionMemo.RejectionMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);
                                        }
                                    }

                                    #endregion

                                    #region RMAirWayBill

                                    if (rejectionMemo.RMAirWayBillList != null && rejectionMemo.RMAirWayBillList.Count > 0)
                                    {
                                        foreach (var rMAirWayBill in rejectionMemo.RMAirWayBillList)
                                        {
                                            DbEntity.RMAirWayBill newRMAirWayBill = new DbEntity.RMAirWayBill();

                                            newRMAirWayBill.RejectionMemoID = newRejectionMemoId;
                                            newRMAirWayBill.BillingCode = rMAirWayBill.BillingCode;
                                            newRMAirWayBill.BreakdownSerialNumber = rMAirWayBill.BreakdownSerialNumber;
                                            newRMAirWayBill.AWBDate = Convert.ToDateTime(rMAirWayBill.AWBDate);
                                            newRMAirWayBill.AWBIssuingAirline = rMAirWayBill.AWBIssuingAirline;
                                            newRMAirWayBill.AWBSerialNumber = rMAirWayBill.AWBSerialNumber;
                                            newRMAirWayBill.AWBCheckDigit = rMAirWayBill.AWBCheckDigit;
                                            newRMAirWayBill.ConsignmentOrigin = rMAirWayBill.Origin;
                                            newRMAirWayBill.ConsignmentDestination = rMAirWayBill.Destination;
                                            newRMAirWayBill.CarriageFrom = rMAirWayBill.From;
                                            newRMAirWayBill.CarriageTo = rMAirWayBill.To;
                                            newRMAirWayBill.TransferDate = new DateTime(Convert.ToInt32("20" + rMAirWayBill.DateOfCarriageOrTransfer.Substring(0, 2)),
                                                                                        Convert.ToInt32(rMAirWayBill.DateOfCarriageOrTransfer.Substring(2, 2)),
                                                                                        Convert.ToInt32(rMAirWayBill.DateOfCarriageOrTransfer.Substring(4, 2)));
                                            newRMAirWayBill.WeightChargesBilled = Convert.ToDecimal(rMAirWayBill.BilledWeightCharge);
                                            newRMAirWayBill.WeightChargesAccepted = Convert.ToDecimal(rMAirWayBill.AcceptedWeightCharge);
                                            newRMAirWayBill.WeightChargesDifference = Convert.ToDecimal(rMAirWayBill.WeightChargeDiff);
                                            newRMAirWayBill.ValuationChargesBilled = Convert.ToDecimal(rMAirWayBill.BilledValuationCharge);
                                            newRMAirWayBill.ValuationChargesAccepted = Convert.ToDecimal(rMAirWayBill.AcceptedValuationCharge);
                                            newRMAirWayBill.ValuationChargesDifference = Convert.ToDecimal(rMAirWayBill.ValuationChargeDiff);
                                            newRMAirWayBill.OtherChargesAmountBilled = Convert.ToDecimal(rMAirWayBill.BilledOtherCharge);
                                            newRMAirWayBill.OtherChargesAmountAccepted = Convert.ToDecimal(rMAirWayBill.AcceptedOtherCharge);
                                            newRMAirWayBill.OtherChargesDifference = Convert.ToDecimal(rMAirWayBill.OtherChargeDiff);
                                            newRMAirWayBill.AmountSubjectedToISCAllowed = Convert.ToDecimal(rMAirWayBill.AllowedAmtSubToIsc);
                                            newRMAirWayBill.AmountSubjectedToISCAccepted = Convert.ToDecimal(rMAirWayBill.AcceptedAmtSubToIsc);
                                            newRMAirWayBill.ISCPercentageAllowed = Convert.ToDecimal(rMAirWayBill.AllowedIscPercentage);
                                            newRMAirWayBill.ISCPercentageAccepted = Convert.ToDecimal(rMAirWayBill.AcceptedIscPercentage);
                                            newRMAirWayBill.ISCAmountAllowed = Convert.ToDecimal(rMAirWayBill.AllowedIscAmount);
                                            newRMAirWayBill.ISCAmountAccepted = Convert.ToDecimal(rMAirWayBill.AcceptedIscAmount);
                                            newRMAirWayBill.ISCAmountDifference = Convert.ToDecimal(rMAirWayBill.IscAmountDifference);
                                            newRMAirWayBill.VATAmountBilled = Convert.ToDecimal(rMAirWayBill.BilledVatAmount);
                                            newRMAirWayBill.VATAmountAccepted = Convert.ToDecimal(rMAirWayBill.AcceptedVatAmount);
                                            newRMAirWayBill.VATAmountDifference = Convert.ToDecimal(rMAirWayBill.VatAmountDifference);
                                            newRMAirWayBill.NetRejectAmount = Convert.ToDecimal(rMAirWayBill.NetRejectAmount);
                                            newRMAirWayBill.CurrencyAdjustmentIndicator = rMAirWayBill.CurrencyAdjustmentIndicator;
                                            newRMAirWayBill.BilledActualFlownWeight = Convert.ToDecimal(rMAirWayBill.BilledWeight);
                                            newRMAirWayBill.ProvisoReqSPA = rMAirWayBill.ProvisionalReqSpa;
                                            newRMAirWayBill.ProratePercentage = rMAirWayBill.ProratePercentage;
                                            newRMAirWayBill.PartShipmentIndicator = rMAirWayBill.PartShipmentIndicator;
                                            newRMAirWayBill.KGLBIndicator = rMAirWayBill.KgLbIndicator;
                                            newRMAirWayBill.CCAindicator = rMAirWayBill.CcaIndicator ? "Y" : String.Empty;
                                            newRMAirWayBill.OurReference = rMAirWayBill.OurReference;
                                            newRMAirWayBill.AttachmentIndicatorOriginal = rMAirWayBill.AttachmentIndicatorOriginal;
                                            newRMAirWayBill.AttachmentIndicatorValidated = rMAirWayBill.AttachmentIndicatorValidated;
                                            newRMAirWayBill.NumberOfAttachments = rMAirWayBill.NumberOfAttachments;
                                            newRMAirWayBill.ISValidationFlag = rMAirWayBill.ISValidationFlag;
                                            newRMAirWayBill.ReasonCode = rMAirWayBill.ReasonCode;
                                            newRMAirWayBill.ReferenceField1 = rMAirWayBill.ReferenceField1;
                                            newRMAirWayBill.ReferenceField2 = rMAirWayBill.ReferenceField2;
                                            newRMAirWayBill.ReferenceField3 = rMAirWayBill.ReferenceField3;
                                            newRMAirWayBill.ReferenceField4 = rMAirWayBill.ReferenceField4;
                                            newRMAirWayBill.ReferenceField5 = rMAirWayBill.ReferenceField5;
                                            newRMAirWayBill.AirlineOwnUse = rMAirWayBill.AirlineOwnUse;
                                            newRMAirWayBill.CreatedBy = CreatedBy;
                                            newRMAirWayBill.CreatedOn = DateTime.UtcNow;
                                            newRMAirWayBill.LastUpdatedBy = ByFileReader;
                                            newRMAirWayBill.LastUpdatedOn = DateTime.UtcNow;
                                            newRMAirWayBill.AWBStatusID = 8; // 8 = IS Validated

                                            _sisDB.RMAirWayBills.Add(newRMAirWayBill);
                                            _sisDB.SaveChanges();

                                            //Logger.InfoFormat("End of Insert RMAirWayBill for AWBSerialNumber: {0}, RejectionMemo: {1}, InvoiceNumber: {2}, FileName: {3}", rMAirWayBill.AWBSerialNumber, rejectionMemo.RejectionMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);

                                            int newRMAirWayBillId = _sisDB.RMAirWayBills.FirstOrDefault(rmawb => rmawb.RejectionMemoID == newRejectionMemoId && rmawb.AWBSerialNumber == newRMAirWayBill.AWBSerialNumber).RMAirWayBillID;

                                            #region RMAWBOtherCharges

                                            if (rMAirWayBill.RMAWBOtherChargesList != null && rMAirWayBill.RMAWBOtherChargesList.Count > 0)
                                            {
                                                foreach (var rMAWBOtherCharges in rMAirWayBill.RMAWBOtherChargesList)
                                                {
                                                    DbEntity.RMAWBOtherCharge newRMAWBOtherCharge = new DbEntity.RMAWBOtherCharge();

                                                    newRMAWBOtherCharge.RMAirWayBillID = newRMAirWayBillId;
                                                    newRMAWBOtherCharge.OtherChargeCode = rMAWBOtherCharges.OtherChargeCode;
                                                    newRMAWBOtherCharge.OtherChargeCodeValue = Convert.ToDecimal(rMAWBOtherCharges.OtherChargeCodeValue);
                                                    newRMAWBOtherCharge.VATLabel = rMAWBOtherCharges.OtherChargeVatLabel;
                                                    newRMAWBOtherCharge.VATText = rMAWBOtherCharges.OtherChargeVatText;
                                                    newRMAWBOtherCharge.VATbaseamount = Convert.ToDecimal(rMAWBOtherCharges.OtherChargeVatBaseAmount);
                                                    newRMAWBOtherCharge.VATpercentage = Convert.ToDecimal(rMAWBOtherCharges.OtherChargeVatPercentage);
                                                    newRMAWBOtherCharge.VATcalculatedamount = Convert.ToDecimal(rMAWBOtherCharges.OtherChargeVatCalculatedAmount);
                                                    newRMAWBOtherCharge.CreatedBy = CreatedBy;
                                                    newRMAWBOtherCharge.CreatedOn = DateTime.UtcNow;
                                                    newRMAWBOtherCharge.LastUpdatedBy = ByFileReader;
                                                    newRMAWBOtherCharge.LastUpdatedOn = DateTime.UtcNow;

                                                    _sisDB.RMAWBOtherCharges.Add(newRMAWBOtherCharge);
                                                    _sisDB.SaveChanges();

                                                    //Logger.InfoFormat("End of Insert RMAWBOtherCharges for AWBSerialNumber: {0}, RejectionMemo: {1}, InvoiceNumber: {2}, FileName: {3}", rMAirWayBill.AWBSerialNumber, rejectionMemo.RejectionMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);
                                                }
                                            }

                                            #endregion

                                            #region RMAWBProrateLadder

                                            if (rMAirWayBill.RMAWBProrateLadderList != null && rMAirWayBill.RMAWBProrateLadderList.Count > 0)
                                            {
                                                foreach (var rMAWBProrateLadder in rMAirWayBill.RMAWBProrateLadderList)
                                                {
                                                    DbEntity.RMAWBProrateLadder newRMAWBProrateLadder = new DbEntity.RMAWBProrateLadder();

                                                    newRMAWBProrateLadder.RMAirWayBillID = newRMAirWayBillId;
                                                    newRMAWBProrateLadder.CurrencyofProrateCalculation = rMAWBProrateLadder.CurrencyofProrateCalculation;
                                                    newRMAWBProrateLadder.TotalAmount = Convert.ToDecimal(rMAWBProrateLadder.TotalAmount);
                                                    newRMAWBProrateLadder.FromSector = rMAWBProrateLadder.FromSector;
                                                    newRMAWBProrateLadder.ToSector = rMAWBProrateLadder.ToSector;
                                                    newRMAWBProrateLadder.CarrierPrefix = rMAWBProrateLadder.CarrierPrefix;
                                                    newRMAWBProrateLadder.ProvisoReqSPA = rMAWBProrateLadder.ProvisoReqSpa;
                                                    newRMAWBProrateLadder.ProrateFactor = rMAWBProrateLadder.ProrateFactor;
                                                    newRMAWBProrateLadder.PercentShare = Convert.ToDecimal(rMAWBProrateLadder.PercentShare);
                                                    newRMAWBProrateLadder.CreatedBy = CreatedBy;
                                                    newRMAWBProrateLadder.CreatedOn = DateTime.UtcNow;
                                                    newRMAWBProrateLadder.LastUpdatedBy = ByFileReader;
                                                    newRMAWBProrateLadder.LastUpdatedOn = DateTime.UtcNow;

                                                    _sisDB.RMAWBProrateLadders.Add(newRMAWBProrateLadder);
                                                    _sisDB.SaveChanges();

                                                    //Logger.InfoFormat("End of Insert RMAWBProrateLadder for AWBSerialNumber: {0}, RejectionMemo: {1}, InvoiceNumber: {2}, FileName: {3}", rMAirWayBill.AWBSerialNumber, rejectionMemo.RejectionMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);
                                                }
                                            }

                                            #endregion

                                            #region RMAWBVAT

                                            if (rMAirWayBill.RMAWBVATList != null && rMAirWayBill.RMAWBVATList.Count > 0)
                                            {
                                                foreach (var rMAWBVAT in rMAirWayBill.RMAWBVATList)
                                                {
                                                    DbEntity.RMAWBVAT newRMAWBVAT = new DbEntity.RMAWBVAT();

                                                    newRMAWBVAT.RMAirWayBillID = newRMAirWayBillId;
                                                    newRMAWBVAT.VATIdentifier = rMAWBVAT.VatIdentifier;
                                                    newRMAWBVAT.VATLabel = rMAWBVAT.VatLabel;
                                                    newRMAWBVAT.VATText = rMAWBVAT.VatText;
                                                    newRMAWBVAT.VATBaseAmount = Convert.ToDecimal(rMAWBVAT.VatBaseAmount);
                                                    newRMAWBVAT.VATPercentage = Convert.ToDecimal(rMAWBVAT.VatPercentage);
                                                    newRMAWBVAT.VATCalculatedAmount = Convert.ToDecimal(rMAWBVAT.VatCalculatedAmount);
                                                    newRMAWBVAT.CreatedBy = CreatedBy;
                                                    newRMAWBVAT.CreatedOn = DateTime.UtcNow;
                                                    newRMAWBVAT.LastUpdatedBy = ByFileReader;
                                                    newRMAWBVAT.LastUpdatedOn = DateTime.UtcNow;

                                                    _sisDB.RMAWBVATs.Add(newRMAWBVAT);
                                                    _sisDB.SaveChanges();

                                                    //Logger.InfoFormat("End of Insert RMAWBVAT for AWBSerialNumber: {0}, RejectionMemo: {1}, InvoiceNumber: {2}, FileName: {3}",
                                                    // rMAirWayBill.AWBSerialNumber, rejectionMemo.RejectionMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);
                                                }
                                            }

                                            #endregion

                                        }
                                    }

                                    #endregion

                                }
                            }

                            #endregion

                            #region BillingMemo

                            if (invoice.BillingMemoList != null && invoice.BillingMemoList.Count > 0)
                            {
                                foreach (var billingMemo in invoice.BillingMemoList)
                                {
                                    DbEntity.BillingMemo newBillingMemo = new DbEntity.BillingMemo();

                                    newBillingMemo.InvoiceHeaderID = newInvoiceHeaderId;
                                    newBillingMemo.BillingCode = billingMemo.BillingCode;
                                    newBillingMemo.BatchSequenceNumber = billingMemo.BatchSequenceNumber;
                                    newBillingMemo.RecordSequenceWithinBatch = billingMemo.RecordSequenceWithinBatch;
                                    newBillingMemo.BillingMemoNumber = billingMemo.BillingMemoNumber;
                                    newBillingMemo.ReasonCode = billingMemo.ReasonCode;
                                    newBillingMemo.OurRef = string.IsNullOrWhiteSpace(billingMemo.OurRef) ? " " : billingMemo.OurRef;
                                    newBillingMemo.CorrespondenceRefNumber = billingMemo.CorrespondenceReferenceNumber;
                                    newBillingMemo.YourInvoiceNumber = string.IsNullOrWhiteSpace(billingMemo.YourInvoiceNumber) ? " " : billingMemo.YourInvoiceNumber;
                                    newBillingMemo.YourInvoiceBillingYear = Convert.ToDecimal(billingMemo.YourInvoiceBillingYear);
                                    newBillingMemo.YourInvoiceBillingMonth = Convert.ToDecimal(billingMemo.YourInvoiceBillingMonth);
                                    newBillingMemo.YourInvoiceBillingPeriod = Convert.ToDecimal(billingMemo.YourInvoiceBillingPeriod);
                                    newBillingMemo.TotalWeightChargesBilled = billingMemo.BilledTotalWeightCharge;
                                    newBillingMemo.TotalValuationAmountBilled = billingMemo.BilledTotalValuationAmount;
                                    newBillingMemo.TotalOtherChargeAmountBilled = billingMemo.BilledTotalOtherChargeAmount;
                                    newBillingMemo.TotalISCAmountBilled = billingMemo.BilledTotalIscAmount;
                                    newBillingMemo.TotalVATAmountBilled = billingMemo.BilledTotalVatAmount;
                                    newBillingMemo.NetBilledAmount = billingMemo.NetBilledAmount;
                                    newBillingMemo.AttachmentIndicatorOriginal = Convert.ToBoolean(billingMemo.AttachmentIndicatorOriginal) ? "Y" : "N";
                                    newBillingMemo.AttachmentIndicatorValidated = Convert.ToBoolean(billingMemo.AttachmentIndicatorValidated) ? "Y" : "N";
                                    newBillingMemo.NumberOfAttachments = billingMemo.NumberOfAttachments;
                                    newBillingMemo.AirlineOwnUse = billingMemo.AirlineOwnUse;
                                    newBillingMemo.ISValidationFlag = billingMemo.ISValidationFlag;
                                    newBillingMemo.ReasonRemarks = billingMemo.ReasonRemarks;
                                    newBillingMemo.CreatedBy = CreatedBy;
                                    newBillingMemo.CreatedOn = DateTime.UtcNow;
                                    newBillingMemo.LastUpdatedBy = ByFileReader;
                                    newBillingMemo.LastUpdatedOn = DateTime.UtcNow;

                                    _sisDB.BillingMemoes.Add(newBillingMemo);
                                    _sisDB.SaveChanges();

                                    //Logger.InfoFormat("End of Insert BillingMemo for BillingMemoNumber: {0}, InvoiceNumber: {1}, FileName: {2}", billingMemo.BillingMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);

                                    int newBillingMemoId = _sisDB.BillingMemoes.FirstOrDefault(rm => rm.InvoiceHeaderID == newInvoiceHeaderId && rm.BillingMemoNumber.Equals(newBillingMemo.BillingMemoNumber)).BillingMemoID;

                                    #region BMVAT

                                    if (billingMemo.BMVATList != null && billingMemo.BMVATList.Count > 0)
                                    {
                                        foreach (var bMVAT in billingMemo.BMVATList)
                                        {
                                            DbEntity.BMVAT newBMVAT = new DbEntity.BMVAT();

                                            newBMVAT.BillingMemoID = newBillingMemoId;
                                            newBMVAT.VATIdentifier = bMVAT.VatIdentifier;
                                            newBMVAT.VATLabel = bMVAT.VatLabel;
                                            newBMVAT.VATText = bMVAT.VatText;
                                            newBMVAT.VATBaseAmount = Convert.ToDecimal(bMVAT.VatBaseAmount);
                                            newBMVAT.VATPercentage = Convert.ToDecimal(bMVAT.VatPercentage);
                                            newBMVAT.VATCalculatedAmount = Convert.ToDecimal(bMVAT.VatCalculatedAmount);
                                            newBMVAT.CreatedBy = CreatedBy;
                                            newBMVAT.CreatedOn = DateTime.UtcNow;
                                            newBMVAT.LastUpdatedBy = ByFileReader;
                                            newBMVAT.LastUpdatedOn = DateTime.UtcNow;

                                            _sisDB.BMVATs.Add(newBMVAT);
                                            _sisDB.SaveChanges();

                                            //Logger.InfoFormat("End of Insert BMVAT for BMVAT Identifier: {0}, BillingMemo: {1}, InvoiceNumber: {2}, FileName: {3}", bMVAT.VatIdentifier, billingMemo.BillingMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);
                                        }
                                    }

                                    #endregion

                                    #region BMAirWayBill

                                    if (billingMemo.BMAirWayBillList != null && billingMemo.BMAirWayBillList.Count > 0)
                                    {
                                        foreach (var bMAirWayBill in billingMemo.BMAirWayBillList)
                                        {
                                            DbEntity.BMAirWayBill newBMAirWayBill = new DbEntity.BMAirWayBill();

                                            newBMAirWayBill.BillingMemoID = newBillingMemoId;
                                            newBMAirWayBill.BillingCode = bMAirWayBill.BillingCode;
                                            newBMAirWayBill.BreakdownSerialNumber = bMAirWayBill.BreakdownSerialNumber;
                                            newBMAirWayBill.AWBDate = bMAirWayBill.AWBDate;
                                            newBMAirWayBill.AWBIssuingAirline = bMAirWayBill.AWBIssuingAirline;
                                            newBMAirWayBill.AWBSerialNumber = bMAirWayBill.AWBSerialNumber;
                                            newBMAirWayBill.AWBCheckDigit = bMAirWayBill.AWBCheckDigit;
                                            newBMAirWayBill.Origin = bMAirWayBill.Origin;
                                            newBMAirWayBill.Destination = bMAirWayBill.Destination;
                                            newBMAirWayBill.From = bMAirWayBill.From;
                                            newBMAirWayBill.To = bMAirWayBill.To;
                                            newBMAirWayBill.DateOfCarriage = new DateTime(Convert.ToInt32("20" + bMAirWayBill.DateOfCarriageOrTransfer.Substring(0, 2)),
                                                                                        Convert.ToInt32(bMAirWayBill.DateOfCarriageOrTransfer.Substring(2, 2)),
                                                                                        Convert.ToInt32(bMAirWayBill.DateOfCarriageOrTransfer.Substring(4, 2)));
                                            newBMAirWayBill.WeightChargesBilled = Convert.ToDecimal(bMAirWayBill.BilledWeightCharge);
                                            newBMAirWayBill.ValuationChargesBilled = Convert.ToDecimal(bMAirWayBill.BilledValuationCharge);
                                            newBMAirWayBill.OtherChargesAmountBilled = Convert.ToDecimal(bMAirWayBill.BilledOtherCharge);
                                            newBMAirWayBill.AmountSubjectedToISCBilled = Convert.ToDecimal(bMAirWayBill.BilledAmtSubToIsc);
                                            newBMAirWayBill.ISCPercentBilled = Convert.ToDecimal(bMAirWayBill.BilledIscPercentage);
                                            newBMAirWayBill.ISCAmountBilled = Convert.ToDecimal(bMAirWayBill.BilledIscAmount);
                                            newBMAirWayBill.VATAmountBilled = Convert.ToDecimal(bMAirWayBill.BilledVatAmount);
                                            newBMAirWayBill.TotalAmountBilled = Convert.ToDecimal(bMAirWayBill.TotalAmount);
                                            newBMAirWayBill.CurrencyAdjustmentIndicator = bMAirWayBill.CurrencyAdjustmentIndicator;
                                            newBMAirWayBill.BilledWeight = bMAirWayBill.BilledWeight != null ? Convert.ToInt32(bMAirWayBill.BilledWeight) : 0;
                                            newBMAirWayBill.ProvisoReqSpa = bMAirWayBill.ProvisionalReqSpa;
                                            newBMAirWayBill.ProratePercent = bMAirWayBill.PrpratePercentage;
                                            newBMAirWayBill.PartShipmentIndicator = bMAirWayBill.PartShipmentIndicator;
                                            newBMAirWayBill.KGLBIndicator = bMAirWayBill.KgLbIndicator;
                                            newBMAirWayBill.CCaIndicator = bMAirWayBill.CcaIndicator ? "Y" : "N";
                                            newBMAirWayBill.AttachmentIndicatorOriginal = string.IsNullOrWhiteSpace(bMAirWayBill.AttachmentIndicatorOriginal) ? "N" : bMAirWayBill.AttachmentIndicatorOriginal.Equals("Y") ? "Y" : "N";
                                            newBMAirWayBill.AttachmentIndicatorValidated = string.IsNullOrWhiteSpace(bMAirWayBill.AttachmentIndicatorValidated) ? "N" : bMAirWayBill.AttachmentIndicatorValidated.Equals("Y") ? "Y" : "N";
                                            newBMAirWayBill.NumberOfAttachments = Convert.ToInt32(bMAirWayBill.NumberOfAttachments);
                                            newBMAirWayBill.ISValidationFlag = bMAirWayBill.ISValidationFlag;
                                            newBMAirWayBill.ReasonCode = bMAirWayBill.ReasonCode;
                                            newBMAirWayBill.ReferenceField1 = bMAirWayBill.ReferenceField1;
                                            newBMAirWayBill.ReferenceField2 = bMAirWayBill.ReferenceField2;
                                            newBMAirWayBill.ReferenceField3 = bMAirWayBill.ReferenceField3;
                                            newBMAirWayBill.ReferenceField4 = bMAirWayBill.ReferenceField4;
                                            newBMAirWayBill.ReferenceField5 = bMAirWayBill.ReferenceField5;
                                            newBMAirWayBill.AirlineOwnUse = bMAirWayBill.AirlineOwnUse;
                                            newBMAirWayBill.CreatedBy = CreatedBy;
                                            newBMAirWayBill.CreatedOn = DateTime.UtcNow;
                                            newBMAirWayBill.LastUpdatedBy = ByFileReader;
                                            newBMAirWayBill.LastUpdatedOn = DateTime.UtcNow;
                                            newBMAirWayBill.AWBStatusID = 8; // 8 = IS Validated

                                            _sisDB.BMAirWayBills.Add(newBMAirWayBill);
                                            _sisDB.SaveChanges();

                                            //Logger.InfoFormat("End of Insert BMAirWayBill for AWBSerialNumber: {0}, BillingMemo: {1}, InvoiceNumber: {2}, FileName: {3}",
                                            //       bMAirWayBill.AWBSerialNumber, billingMemo.BillingMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);

                                            int newBMAirWayBillId = _sisDB.BMAirWayBills.FirstOrDefault(bmawb => bmawb.BillingMemoID == newBillingMemoId && bmawb.AWBSerialNumber == newBMAirWayBill.AWBSerialNumber).BMAirWayBillId;

                                            #region BMAWBOtherCharges

                                            if (bMAirWayBill.BMAWBOtherChargesList != null && bMAirWayBill.BMAWBOtherChargesList.Count > 0)
                                            {
                                                foreach (var bMAWBOtherCharges in bMAirWayBill.BMAWBOtherChargesList)
                                                {
                                                    DbEntity.BMAWBOtherCharge newBMAWBOtherCharge = new DbEntity.BMAWBOtherCharge();

                                                    newBMAWBOtherCharge.BMAirWayBillID = newBMAirWayBillId;
                                                    newBMAWBOtherCharge.OtherChargeCode = bMAWBOtherCharges.OtherChargeCode;
                                                    newBMAWBOtherCharge.OtherChargeCodeValue = Convert.ToDecimal(bMAWBOtherCharges.OtherChargeCodeValue);
                                                    newBMAWBOtherCharge.VATLabel = bMAWBOtherCharges.OtherChargeVatLabel;
                                                    newBMAWBOtherCharge.VATText = bMAWBOtherCharges.OtherChargeVatText;
                                                    newBMAWBOtherCharge.VATBaseAmount = Convert.ToDecimal(bMAWBOtherCharges.OtherChargeVatBaseAmount);
                                                    newBMAWBOtherCharge.VATPercentage = Convert.ToDecimal(bMAWBOtherCharges.OtherChargeVatPercentage);
                                                    newBMAWBOtherCharge.VATCalculatedAmount = Convert.ToDecimal(bMAWBOtherCharges.OtherChargeVatCalculatedAmount);
                                                    newBMAWBOtherCharge.CreatedBy = CreatedBy;
                                                    newBMAWBOtherCharge.CreatedOn = DateTime.UtcNow;
                                                    newBMAWBOtherCharge.LastUpdatedBy = ByFileReader;
                                                    newBMAWBOtherCharge.LastUpdatedOn = DateTime.UtcNow;

                                                    _sisDB.BMAWBOtherCharges.Add(newBMAWBOtherCharge);
                                                    _sisDB.SaveChanges();

                                                    //Logger.InfoFormat("End of Insert BMAWBOtherCharges for AWBSerialNumber: {0}, BillingMemo: {1}, InvoiceNumber: {2}, FileName: {3}",
                                                    //bMAirWayBill.AWBSerialNumber, billingMemo.BillingMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);
                                                }
                                            }

                                            #endregion

                                            #region BMAWBProrateLadder

                                            if (bMAirWayBill.BMAWBProrateLadderList != null && bMAirWayBill.BMAWBProrateLadderList.Count > 0)
                                            {
                                                foreach (var bMAWBProrateLadder in bMAirWayBill.BMAWBProrateLadderList)
                                                {
                                                    DbEntity.BMAWBProrateLadder newBMAWBProrateLadder = new DbEntity.BMAWBProrateLadder();

                                                    newBMAWBProrateLadder.BMAirWayBillID = newBMAirWayBillId;
                                                    newBMAWBProrateLadder.CurrencyOfProrateCalculation = bMAWBProrateLadder.CurrencyofProrateCalculation;
                                                    newBMAWBProrateLadder.TotalAmount = Convert.ToDecimal(bMAWBProrateLadder.TotalAmount);
                                                    newBMAWBProrateLadder.FromSector = bMAWBProrateLadder.FromSector;
                                                    newBMAWBProrateLadder.ToSector = bMAWBProrateLadder.ToSector;
                                                    newBMAWBProrateLadder.CarrierPrefix = bMAWBProrateLadder.CarrierPrefix;
                                                    newBMAWBProrateLadder.ProvisoReqSpa = bMAWBProrateLadder.ProvisoReqSpa;
                                                    newBMAWBProrateLadder.ProrateFactor = bMAWBProrateLadder.ProrateFactor;
                                                    newBMAWBProrateLadder.PercentShare = Convert.ToDecimal(bMAWBProrateLadder.PercentShare);
                                                    newBMAWBProrateLadder.CreatedBy = CreatedBy;
                                                    newBMAWBProrateLadder.CreatedOn = DateTime.UtcNow;
                                                    newBMAWBProrateLadder.LastUpdatedBy = ByFileReader;
                                                    newBMAWBProrateLadder.LastUpdatedOn = DateTime.UtcNow;

                                                    _sisDB.BMAWBProrateLadders.Add(newBMAWBProrateLadder);
                                                    _sisDB.SaveChanges();

                                                    //Logger.InfoFormat("End of Insert BMAWBProrateLadder for AWBSerialNumber: {0}, BillingMemo: {1}, InvoiceNumber: {2}, FileName: {3}",
                                                    //bMAirWayBill.AWBSerialNumber, billingMemo.BillingMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);
                                                }
                                            }

                                            #endregion

                                            #region BMAWBVAT

                                            if (bMAirWayBill.BMAWBVATList != null && bMAirWayBill.BMAWBVATList.Count > 0)
                                            {
                                                foreach (var bMAWBVAT in bMAirWayBill.BMAWBVATList)
                                                {
                                                    DbEntity.BMAWBVAT newBMAWBVAT = new DbEntity.BMAWBVAT();

                                                    newBMAWBVAT.BMAirWayBillID = newBMAirWayBillId;
                                                    newBMAWBVAT.VATIdentifier = bMAWBVAT.VatIdentifier;
                                                    newBMAWBVAT.VATLabel = bMAWBVAT.VatLabel;
                                                    newBMAWBVAT.VATText = bMAWBVAT.VatText;
                                                    newBMAWBVAT.VATBaseAmount = Convert.ToDecimal(bMAWBVAT.VatBaseAmount);
                                                    newBMAWBVAT.VATPercentage = Convert.ToDecimal(bMAWBVAT.VatPercentage);
                                                    newBMAWBVAT.VATCalculatedAmount = Convert.ToDecimal(bMAWBVAT.VatCalculatedAmount);
                                                    newBMAWBVAT.CreatedBy = CreatedBy;
                                                    newBMAWBVAT.CreatedOn = DateTime.UtcNow;
                                                    newBMAWBVAT.LastUpdatedBy = ByFileReader;
                                                    newBMAWBVAT.LastUpdatedOn = DateTime.UtcNow;

                                                    _sisDB.BMAWBVATs.Add(newBMAWBVAT);
                                                    _sisDB.SaveChanges();

                                                    //Logger.InfoFormat("End of Insert BMAWBVAT for AWBSerialNumber: {0}, BillingMemo: {1}, InvoiceNumber: {2}, FileName: {3}",
                                                    //bMAirWayBill.AWBSerialNumber, billingMemo.BillingMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);
                                                }
                                            }

                                            #endregion

                                        }
                                    }

                                    #endregion

                                }
                            }

                            #endregion

                            #region CreditMemo

                            if (invoice.CreditMemoList != null && invoice.CreditMemoList.Count > 0)
                            {
                                foreach (var creditMemo in invoice.CreditMemoList)
                                {
                                    DbEntity.CreditMemo newCreditMemo = new DbEntity.CreditMemo();

                                    newCreditMemo.InvoiceHeaderID = newInvoiceHeaderId;
                                    newCreditMemo.BillingCode = creditMemo.BillingCode;
                                    newCreditMemo.BatchSequenceNumber = creditMemo.BatchSequenceNumber;
                                    newCreditMemo.RecordSequenceWithinBatch = creditMemo.RecordSequenceWithinBatch;
                                    newCreditMemo.CreditMemoNumber = creditMemo.CreditMemoNumber;
                                    newCreditMemo.ReasonCode = creditMemo.ReasonCode;
                                    newCreditMemo.OurRef = string.IsNullOrWhiteSpace(creditMemo.OurRef) ? " " : creditMemo.OurRef;
                                    newCreditMemo.CorrespondenceRefNumber = creditMemo.CorrespondenceRefNumber;
                                    newCreditMemo.YourInvoiceNumber = string.IsNullOrWhiteSpace(creditMemo.YourInvoiceNumber) ? " " : creditMemo.YourInvoiceNumber;
                                    newCreditMemo.YourInvoiceBillingYear = Convert.ToDecimal(creditMemo.YourInvoiceBillingYear);
                                    newCreditMemo.YourInvoiceBillingMonth = Convert.ToDecimal(creditMemo.YourInvoiceBillingMonth);
                                    newCreditMemo.YourInvoiceBillingPeriod = Convert.ToDecimal(creditMemo.YourInvoiceBillingPeriod);
                                    newCreditMemo.TotalWeightChargesCredited = creditMemo.TotalWeightCharges;
                                    newCreditMemo.TotalValuationAmountCredited = creditMemo.TotalValuationAmt;
                                    newCreditMemo.TotalOtherChargeAmountCredited = creditMemo.TotalOtherChargeAmt;
                                    newCreditMemo.TotalISCAmountCredited = creditMemo.TotalIscAmountCredited;
                                    newCreditMemo.TotalVATAmountCredited = creditMemo.TotalVatAmountCredited;
                                    newCreditMemo.NetCreditedAmount = creditMemo.NetAmountCredited;
                                    newCreditMemo.AttachmentIndicatorOriginal = Convert.ToBoolean(creditMemo.AttachmentIndicatorOriginal) ? "Y" : "N";
                                    newCreditMemo.AttachmentIndicatorValidated = Convert.ToBoolean(creditMemo.AttachmentIndicatorValidated) ? "Y" : "N";
                                    newCreditMemo.NumberOfAttachments = creditMemo.NumberOfAttachments;
                                    newCreditMemo.AirlineOwnUse = creditMemo.AirlineOwnUse;
                                    newCreditMemo.ISValidationFlag = creditMemo.ISValidationFlag;
                                    newCreditMemo.ReasonRemarks = creditMemo.ReasonRemarks;
                                    newCreditMemo.CreatedBy = CreatedBy;
                                    newCreditMemo.CreatedOn = DateTime.UtcNow;
                                    newCreditMemo.LastUpdatedBy = ByFileReader;
                                    newCreditMemo.LastUpdatedOn = DateTime.UtcNow;

                                    _sisDB.CreditMemoes.Add(newCreditMemo);
                                    _sisDB.SaveChanges();

                                    //Logger.InfoFormat("End of Insert CreditMemo for CreditMemoNumber: {0}, InvoiceNumber: {1}, FileName: {2}", creditMemo.CreditMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);

                                    int newCreditMemoId = _sisDB.CreditMemoes.FirstOrDefault(cm => cm.InvoiceHeaderID == newInvoiceHeaderId && cm.CreditMemoNumber.Equals(newCreditMemo.CreditMemoNumber)).CreditMemoID;

                                    #region CMVAT

                                    if (creditMemo.CMVATList != null && creditMemo.CMVATList.Count > 0)
                                    {
                                        foreach (var cMVAT in creditMemo.CMVATList)
                                        {
                                            DbEntity.CMVAT newCMVAT = new DbEntity.CMVAT();

                                            newCMVAT.CreditMemoID = newCreditMemoId;
                                            newCMVAT.VATIdentifier = cMVAT.VatIdentifier;
                                            newCMVAT.VATLabel = cMVAT.VatLabel;
                                            newCMVAT.VATText = cMVAT.VatText;
                                            newCMVAT.VATBaseAmount = Convert.ToDecimal(cMVAT.VatBaseAmount);
                                            newCMVAT.VATPercentage = Convert.ToDecimal(cMVAT.VatPercentage);
                                            newCMVAT.VATCalculatedAmount = Convert.ToDecimal(cMVAT.VatCalculatedAmount);
                                            newCMVAT.CreatedBy = CreatedBy;
                                            newCMVAT.CreatedOn = DateTime.UtcNow;
                                            newCMVAT.LastUpdatedBy = ByFileReader;
                                            newCMVAT.LastUpdatedOn = DateTime.UtcNow;

                                            _sisDB.CMVATs.Add(newCMVAT);
                                            _sisDB.SaveChanges();

                                            //Logger.InfoFormat("End of Insert CMVAT for CMVAT Identifier: {0}, CreditMemo: {1}, InvoiceNumber: {2}, FileName: {3}", cMVAT.VatIdentifier, creditMemo.CreditMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);
                                        }
                                    }

                                    #endregion

                                    #region CMAirWayBill

                                    if (creditMemo.CMAirWayBillList != null && creditMemo.CMAirWayBillList.Count > 0)
                                    {
                                        foreach (var cMAirWayBill in creditMemo.CMAirWayBillList)
                                        {
                                            DbEntity.CMAirWayBill newCMAirWayBill = new DbEntity.CMAirWayBill();

                                            newCMAirWayBill.CreditMemoID = newCreditMemoId;
                                            newCMAirWayBill.BillingCode = cMAirWayBill.BillingCode;
                                            newCMAirWayBill.BreakdownSerialNumber = cMAirWayBill.BreakdownSerialNumber;
                                            newCMAirWayBill.AWBDate = cMAirWayBill.AWBDate;
                                            newCMAirWayBill.AWBIssuingAirline = cMAirWayBill.AWBIssuingAirline;
                                            newCMAirWayBill.AWBSerialNumber = cMAirWayBill.AWBSerialNumber;
                                            newCMAirWayBill.AWBCheckDigit = cMAirWayBill.AWBCheckDigit;
                                            newCMAirWayBill.Origin = cMAirWayBill.Origin;
                                            newCMAirWayBill.Destination = cMAirWayBill.Destination;
                                            newCMAirWayBill.From = cMAirWayBill.From;
                                            newCMAirWayBill.To = cMAirWayBill.To;
                                            newCMAirWayBill.DateOfCarriage = new DateTime(Convert.ToInt32("20" + cMAirWayBill.DateOfCarriageOrTransfer.Substring(0, 2)),
                                                                                        Convert.ToInt32(cMAirWayBill.DateOfCarriageOrTransfer.Substring(2, 2)),
                                                                                        Convert.ToInt32(cMAirWayBill.DateOfCarriageOrTransfer.Substring(4, 2)));
                                            newCMAirWayBill.WeightChargesCredited = Convert.ToDecimal(cMAirWayBill.CreditedWeightCharge);
                                            newCMAirWayBill.ValuationChargesCredited = Convert.ToDecimal(cMAirWayBill.CreditedValuationCharge);
                                            newCMAirWayBill.OtherChargesAmountCredited = Convert.ToDecimal(cMAirWayBill.CreditedOtherCharge);
                                            newCMAirWayBill.AmountSubjectedToISCCredited = Convert.ToDecimal(cMAirWayBill.CreditedAmtSubToIsc);
                                            newCMAirWayBill.ISCPercentCredited = Convert.ToDecimal(cMAirWayBill.CreditedIscPercentage);
                                            newCMAirWayBill.ISCAmountCredited = Convert.ToDecimal(cMAirWayBill.CreditedIscAmount);
                                            newCMAirWayBill.VATAmountCredited = Convert.ToDecimal(cMAirWayBill.CreditedVatAmount);
                                            newCMAirWayBill.TotalAmountCredited = Convert.ToDecimal(cMAirWayBill.TotalAmountCredited);
                                            newCMAirWayBill.CurrencyAdjustmentIndicator = cMAirWayBill.CurrencyAdjustmentIndicator;
                                            newCMAirWayBill.BilledWeight = cMAirWayBill.BilledWeight != null ? Convert.ToInt32(cMAirWayBill.BilledWeight) : 0;
                                            newCMAirWayBill.ProvisoReqSPA = cMAirWayBill.ProvisionalReqSpa;
                                            newCMAirWayBill.ProratePercent = cMAirWayBill.ProratePercentage;
                                            newCMAirWayBill.PartShipmentIndicator = cMAirWayBill.PartShipmentIndicator;
                                            newCMAirWayBill.KGLBIndicator = cMAirWayBill.KgLbIndicator;
                                            newCMAirWayBill.CCaIndicator = cMAirWayBill.CcaIndicator ? "Y" : "N";
                                            newCMAirWayBill.AttachmentIndicatorOriginal = string.IsNullOrWhiteSpace(cMAirWayBill.AttachmentIndicatorOriginal) ? "N" : cMAirWayBill.AttachmentIndicatorOriginal.Equals("Y") ? "Y" : "N";
                                            newCMAirWayBill.AttachmentIndicatorValidated = string.IsNullOrWhiteSpace(cMAirWayBill.AttachmentIndicatorValidated) ? "N" : cMAirWayBill.AttachmentIndicatorValidated.Equals("Y") ? "Y" : "N";
                                            newCMAirWayBill.NumberOfAttachments = Convert.ToInt32(cMAirWayBill.NumberOfAttachments);
                                            newCMAirWayBill.ISValidationFlag = cMAirWayBill.ISValidationFlag;
                                            newCMAirWayBill.ReasonCode = cMAirWayBill.ReasonCode;
                                            newCMAirWayBill.ReferenceField1 = cMAirWayBill.ReferenceField1;
                                            newCMAirWayBill.ReferenceField2 = cMAirWayBill.ReferenceField2;
                                            newCMAirWayBill.ReferenceField3 = cMAirWayBill.ReferenceField3;
                                            newCMAirWayBill.ReferenceField4 = cMAirWayBill.ReferenceField4;
                                            newCMAirWayBill.ReferenceField5 = cMAirWayBill.ReferenceField5;
                                            newCMAirWayBill.AirlineOwnUse = cMAirWayBill.AirlineOwnUse;
                                            newCMAirWayBill.CreatedBy = CreatedBy;
                                            newCMAirWayBill.CreatedOn = DateTime.UtcNow;
                                            newCMAirWayBill.LastUpdatedBy = ByFileReader;
                                            newCMAirWayBill.LastUpdatedOn = DateTime.UtcNow;
                                            newCMAirWayBill.AWBStatusID = 8; // 8 = IS Validated

                                            _sisDB.CMAirWayBills.Add(newCMAirWayBill);
                                            _sisDB.SaveChanges();

                                            //Logger.InfoFormat("End of Insert CMAirWayBill for AWBSerialNumber: {0}, CreditMemo: {1}, InvoiceNumber: {2}, FileName: {3}",
                                            //        cMAirWayBill.AWBSerialNumber, creditMemo.CreditMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);

                                            int newCMAirWayBillId = _sisDB.CMAirWayBills.FirstOrDefault(cmawb => cmawb.CreditMemoID == newCreditMemoId && cmawb.AWBSerialNumber == newCMAirWayBill.AWBSerialNumber).CMAirWayBillID;

                                            #region CMAWBOtherCharges

                                            if (cMAirWayBill.CMAWBOtherChargesList != null && cMAirWayBill.CMAWBOtherChargesList.Count > 0)
                                            {
                                                foreach (var cMAWBOtherCharges in cMAirWayBill.CMAWBOtherChargesList)
                                                {
                                                    DbEntity.CMAWBOtherCharge newCMAWBOtherCharge = new DbEntity.CMAWBOtherCharge();

                                                    newCMAWBOtherCharge.CMAirWayBillID = newCMAirWayBillId;
                                                    newCMAWBOtherCharge.OtherChargeCode = cMAWBOtherCharges.OtherChargeCode;
                                                    newCMAWBOtherCharge.OtherChargeCodeValue = Convert.ToDecimal(cMAWBOtherCharges.OtherChargeCodeValue);
                                                    newCMAWBOtherCharge.VATLabel = cMAWBOtherCharges.OtherChargeVatLabel;
                                                    newCMAWBOtherCharge.VATText = cMAWBOtherCharges.OtherChargeVatText;
                                                    newCMAWBOtherCharge.VATBaseAmount = Convert.ToDecimal(cMAWBOtherCharges.OtherChargeVatBaseAmount);
                                                    newCMAWBOtherCharge.VATPercentage = Convert.ToDecimal(cMAWBOtherCharges.OtherChargeVatPercentage);
                                                    newCMAWBOtherCharge.VATCalculatedAmount = Convert.ToDecimal(cMAWBOtherCharges.OtherChargeVatCalculatedAmount);
                                                    newCMAWBOtherCharge.CreatedBy = CreatedBy;
                                                    newCMAWBOtherCharge.CreatedOn = DateTime.UtcNow;
                                                    newCMAWBOtherCharge.LastUpdatedBy = ByFileReader;
                                                    newCMAWBOtherCharge.LastUpdatedOn = DateTime.UtcNow;

                                                    _sisDB.CMAWBOtherCharges.Add(newCMAWBOtherCharge);
                                                    _sisDB.SaveChanges();

                                                    //Logger.InfoFormat("End of Insert CMAWBOtherCharges for AWBSerialNumber: {0}, CreditMemo: {1}, InvoiceNumber: {2}, FileName: {3}",
                                                    //cMAirWayBill.AWBSerialNumber, creditMemo.CreditMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);
                                                }
                                            }

                                            #endregion

                                            #region CMAWBProrateLadder

                                            if (cMAirWayBill.CMAWBProrateLadderList != null && cMAirWayBill.CMAWBProrateLadderList.Count > 0)
                                            {
                                                foreach (var cMAWBProrateLadder in cMAirWayBill.CMAWBProrateLadderList)
                                                {
                                                    DbEntity.CMAWBProrateLadder newCMAWBProrateLadder = new DbEntity.CMAWBProrateLadder();

                                                    newCMAWBProrateLadder.CMAirWayBillID = newCMAirWayBillId;
                                                    newCMAWBProrateLadder.CurrencyOfProrateCalculation = cMAWBProrateLadder.CurrencyofProrateCalculation;
                                                    newCMAWBProrateLadder.TotalAmount = Convert.ToDecimal(cMAWBProrateLadder.TotalAmount);
                                                    newCMAWBProrateLadder.FromSector = cMAWBProrateLadder.FromSector;
                                                    newCMAWBProrateLadder.ToSector = cMAWBProrateLadder.ToSector;
                                                    newCMAWBProrateLadder.CarrierPrefix = cMAWBProrateLadder.CarrierPrefix;
                                                    newCMAWBProrateLadder.ProvisoReqSpa = cMAWBProrateLadder.ProvisoReqSpa;
                                                    newCMAWBProrateLadder.ProrateFactor = cMAWBProrateLadder.ProrateFactor;
                                                    newCMAWBProrateLadder.PercentShare = Convert.ToDecimal(cMAWBProrateLadder.PercentShare);
                                                    newCMAWBProrateLadder.CreatedBy = CreatedBy;
                                                    newCMAWBProrateLadder.CreatedOn = DateTime.UtcNow;
                                                    newCMAWBProrateLadder.LastUpdatedBy = ByFileReader;
                                                    newCMAWBProrateLadder.LastUpdatedOn = DateTime.UtcNow;

                                                    _sisDB.CMAWBProrateLadders.Add(newCMAWBProrateLadder);
                                                    _sisDB.SaveChanges();

                                                    //Logger.InfoFormat("End of Insert CMAWBProrateLadder for AWBSerialNumber: {0}, CreditMemo: {1}, InvoiceNumber: {2}, FileName: {3}",
                                                    //cMAirWayBill.AWBSerialNumber, creditMemo.CreditMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);
                                                }
                                            }

                                            #endregion

                                            #region CMAWBVAT

                                            if (cMAirWayBill.CMAWBVATList != null && cMAirWayBill.CMAWBVATList.Count > 0)
                                            {
                                                foreach (var cMAWBVAT in cMAirWayBill.CMAWBVATList)
                                                {
                                                    DbEntity.CMAWBVAT newCMAWBVAT = new DbEntity.CMAWBVAT();

                                                    newCMAWBVAT.CMAirWayBillID = newCMAirWayBillId;
                                                    newCMAWBVAT.VATIdentifier = cMAWBVAT.VatIdentifier;
                                                    newCMAWBVAT.VATLabel = cMAWBVAT.VatLabel;
                                                    newCMAWBVAT.VATText = cMAWBVAT.VatText;
                                                    newCMAWBVAT.VATBaseAmount = Convert.ToDecimal(cMAWBVAT.VatBaseAmount);
                                                    newCMAWBVAT.VATPercentage = Convert.ToDecimal(cMAWBVAT.VatPercentage);
                                                    newCMAWBVAT.VATCalculatedAmount = Convert.ToDecimal(cMAWBVAT.VatCalculatedAmount);
                                                    newCMAWBVAT.CreatedBy = CreatedBy;
                                                    newCMAWBVAT.CreatedOn = DateTime.UtcNow;
                                                    newCMAWBVAT.LastUpdatedBy = ByFileReader;
                                                    newCMAWBVAT.LastUpdatedOn = DateTime.UtcNow;

                                                    _sisDB.CMAWBVATs.Add(newCMAWBVAT);
                                                    _sisDB.SaveChanges();

                                                    //Logger.InfoFormat("End of Insert CMAWBVAT for AWBSerialNumber: {0}, CreditMemo: {1}, InvoiceNumber: {2}, FileName: {3}",
                                                    //cMAirWayBill.AWBSerialNumber, creditMemo.CreditMemoNumber, invoice.InvoiceNumber, fileData.FileHeader.FileName);
                                                }
                                            }

                                            #endregion

                                        }
                                    }

                                    #endregion

                                }
                            }

                            #endregion

                            #region InvoiceTotal

                            if (invoice.InvoiceTotals != null)
                            {
                                DbEntity.InvoiceTotal newInvoiceTotal = new DbEntity.InvoiceTotal();

                                newInvoiceTotal.InvoiceHeaderID = newInvoiceHeaderId;
                                newInvoiceTotal.TotalWeightCharges = invoice.InvoiceTotals.TotalWeightCharges;
                                newInvoiceTotal.TotalOtherCharges = invoice.InvoiceTotals.TotalOtherCharges;
                                newInvoiceTotal.TotalInterlineServiceChargeAmount = invoice.InvoiceTotals.TotalInterlineServiceChargeAmount;
                                newInvoiceTotal.NetInvoiceTotal = invoice.InvoiceTotals.NetInvoiceTotal;
                                newInvoiceTotal.NetInvoiceBillingTotal = invoice.InvoiceTotals.NetInvoiceBillingTotal;
                                newInvoiceTotal.TotalNumberOfBillingRecords = invoice.InvoiceTotals.TotalNumberOfBillingRecords;
                                newInvoiceTotal.TotalValuationCharges = invoice.InvoiceTotals.TotalValuationCharges;
                                newInvoiceTotal.TotalVATAmount = invoice.InvoiceTotals.TotalVATAmount;
                                newInvoiceTotal.TotalNumberOfRecords = invoice.InvoiceTotals.TotalNumberOfRecords;
                                newInvoiceTotal.TotalNetAmountWithoutVAT = invoice.InvoiceTotals.TotalNetAmountWithoutVat;
                                newInvoiceTotal.CreatedBy = CreatedBy;
                                newInvoiceTotal.CreatedOn = DateTime.UtcNow;
                                newInvoiceTotal.LastUpdatedBy = ByFileReader;
                                newInvoiceTotal.LastUpdatedOn = DateTime.UtcNow;

                                _sisDB.InvoiceTotals.Add(newInvoiceTotal);
                                _sisDB.SaveChanges();
                            }

                            #endregion

                            #region InvoiceTotalVAT

                            if (invoice.InvoiceTotalVATList != null && invoice.InvoiceTotalVATList.Count > 0)
                            {
                                foreach (var invoiceTotalVAT in invoice.InvoiceTotalVATList)
                                {
                                    DbEntity.InvoiceTotalVAT newInvoiceTotalVAT = new DbEntity.InvoiceTotalVAT();

                                    newInvoiceTotalVAT.InvoiceHeaderID = newInvoiceHeaderId;
                                    newInvoiceTotalVAT.VATIdentifier = invoiceTotalVAT.VatIdentifier;
                                    newInvoiceTotalVAT.VATLabel = invoiceTotalVAT.VatLabel;
                                    newInvoiceTotalVAT.VATText = invoiceTotalVAT.VatText;
                                    newInvoiceTotalVAT.VATBaseAmount = Convert.ToDecimal(invoiceTotalVAT.VatBaseAmount);
                                    newInvoiceTotalVAT.VATPercentage = Convert.ToDecimal(invoiceTotalVAT.VatPercentage);
                                    newInvoiceTotalVAT.VATCalculatedAmount = Convert.ToDecimal(invoiceTotalVAT.VatCalculatedAmount);
                                    newInvoiceTotalVAT.CreatedBy = CreatedBy;
                                    newInvoiceTotalVAT.CreatedOn = DateTime.UtcNow;
                                    newInvoiceTotalVAT.LastUpdatedBy = ByFileReader;
                                    newInvoiceTotalVAT.LastUpdatedOn = DateTime.UtcNow;

                                    _sisDB.InvoiceTotalVATs.Add(newInvoiceTotalVAT);
                                    _sisDB.SaveChanges();
                                }
                            }

                            #endregion

                            #region BillingCodeSubTotal

                            if (invoice.BillingCodeSubTotalList != null && invoice.BillingCodeSubTotalList.Count > 0)
                            {
                                foreach (var billingCodeSubTotal in invoice.BillingCodeSubTotalList)
                                {
                                    DbEntity.BillingCodeSubTotal newBillingCodeSubTotal = new DbEntity.BillingCodeSubTotal();

                                    newBillingCodeSubTotal.InvoiceHeaderID = newInvoiceHeaderId;
                                    newBillingCodeSubTotal.BillingCode = billingCodeSubTotal.BillingCode;
                                    newBillingCodeSubTotal.TotalWeighCharges = billingCodeSubTotal.TotalWeightCharge;
                                    newBillingCodeSubTotal.TotalOtherCharges = billingCodeSubTotal.TotalOtherCharge;
                                    newBillingCodeSubTotal.TotalInterlineServiceCharge = billingCodeSubTotal.TotalIscAmount;
                                    newBillingCodeSubTotal.BillingCodeSubTotal1 = billingCodeSubTotal.BillingCodeSbTotal;
                                    newBillingCodeSubTotal.TotalNumberOfBillingRecords = billingCodeSubTotal.NumberOfBillingRecords;
                                    newBillingCodeSubTotal.TotalValuationCharges = billingCodeSubTotal.TotalValuationCharge;
                                    newBillingCodeSubTotal.TotalVATAmount = billingCodeSubTotal.TotalVatAmount;
                                    newBillingCodeSubTotal.TotalNumberOfRecords = billingCodeSubTotal.TotalNumberOfRecords;
                                    newBillingCodeSubTotal.BillingCodeSubTotalDescription = billingCodeSubTotal.BillingCodeSubTotalDesc;
                                    newBillingCodeSubTotal.CreatedBy = CreatedBy;
                                    newBillingCodeSubTotal.CreatedOn = DateTime.UtcNow;
                                    newBillingCodeSubTotal.LastUpdatedBy = ByFileReader;
                                    newBillingCodeSubTotal.LastUpdatedOn = DateTime.UtcNow;

                                    _sisDB.BillingCodeSubTotals.Add(newBillingCodeSubTotal);
                                    _sisDB.SaveChanges();

                                    #region BillingCodeSubTotalVAT

                                    if (billingCodeSubTotal.BillingCodeSubTotalVATList != null && billingCodeSubTotal.BillingCodeSubTotalVATList.Count > 0)
                                    {
                                        int newBillingCodeSubTotalId = _sisDB.BillingCodeSubTotals.FirstOrDefault(bcst => bcst.InvoiceHeaderID == newInvoiceHeaderId).BillingCodeSubTotalID;

                                        foreach (var billingCodeSubTotalVAT in billingCodeSubTotal.BillingCodeSubTotalVATList)
                                        {
                                            DbEntity.BillingCodeSubTotalVAT newBillingCodeSubTotalVAT = new DbEntity.BillingCodeSubTotalVAT();

                                            newBillingCodeSubTotalVAT.BillingCodeSubTotalID = newBillingCodeSubTotalId;
                                            newBillingCodeSubTotalVAT.VATIdentifier = billingCodeSubTotalVAT.VatIdentifier;
                                            newBillingCodeSubTotalVAT.VATLabel = billingCodeSubTotalVAT.VatLabel;
                                            newBillingCodeSubTotalVAT.VATText = billingCodeSubTotalVAT.VatText;
                                            newBillingCodeSubTotalVAT.VATBaseAmount = Convert.ToDecimal(billingCodeSubTotalVAT.VatBaseAmount);
                                            newBillingCodeSubTotalVAT.VATPercentage = Convert.ToDecimal(billingCodeSubTotalVAT.VatPercentage);
                                            newBillingCodeSubTotalVAT.VATCalculatedAmount = Convert.ToDecimal(billingCodeSubTotalVAT.VatCalculatedAmount);
                                            newBillingCodeSubTotalVAT.CreatedBy = CreatedBy;
                                            newBillingCodeSubTotalVAT.CreatedOn = DateTime.UtcNow;
                                            newBillingCodeSubTotalVAT.LastUpdatedBy = ByFileReader;
                                            newBillingCodeSubTotalVAT.LastUpdatedOn = DateTime.UtcNow;

                                            _sisDB.BillingCodeSubTotalVATs.Add(newBillingCodeSubTotalVAT);
                                            _sisDB.SaveChanges();
                                        }
                                    }

                                    #endregion

                                }
                            }

                            #endregion

                        }

                        _sisDB.FileTotals.Add(newFileTotal);
                        _sisDB.SaveChanges();

                        #endregion

                        context.SaveChanges();
                        dbContextTransaction.Commit();

                        return true;
                    }
                    catch (DbEntityValidationException exception)
                    {
                        dbContextTransaction.Rollback();
                        newFileHeaderIdRetrived = 0;
                        clsLog.WriteLogAzure("Exception occured in InsertReceivedFileData: ");
                        foreach (var item in ((System.Data.Entity.Validation.DbEntityValidationException)exception).EntityValidationErrors.FirstOrDefault().ValidationErrors)
                        {
                            clsLog.WriteLogAzure("Entity Validation Error: {0}" + item.ErrorMessage.ToString());
                        }

                        return false;
                    }
                    catch (Exception exception)
                    {
                        dbContextTransaction.Rollback();
                        newFileHeaderIdRetrived = 0;
                        clsLog.WriteLogAzure(exception.InnerException);

                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// To Update Batch Number, Sequence Number and Breakdown Serial Numbers for the Invoices to be written into file.
        /// </summary>
        /// <param name="fileData">FileData</param>
        /// <returns></returns>
        public bool UpdateBatchSequenceNumbers(List<int> listInvoiceHeaderID)
        {
            if (listInvoiceHeaderID != null)
            {
                listInvoiceHeaderID.Sort();

                int batchNo = 1;
                int seqNo = 1;

                foreach (int invoiceId in listInvoiceHeaderID)
                {
                    var firstOrDefault = _sisDB.InvoiceHeaders.FirstOrDefault(ih => ih.InvoiceHeaderID == invoiceId);
                    if (firstOrDefault != null)
                    {
                        foreach (var billingCodeTotal in firstOrDefault.BillingCodeSubTotals)
                        {
                            var lineItem = billingCodeTotal;

                            #region Airwaybill

                            var airWayBillList = (from airWayBills in firstOrDefault.AirWayBills
                                                  where airWayBills.BillingCode == lineItem.BillingCode
                                                  select airWayBills).ToList();

                            foreach (var newAirWayBill in airWayBillList)
                            {
                                newAirWayBill.BatchSequenceNumber = batchNo;
                                newAirWayBill.RecordSequencewithinBatch = seqNo;

                                _sisDB.Entry(newAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                _sisDB.SaveChanges();

                                seqNo = seqNo + 1;
                            }

                            #endregion Airwaybill

                            #region Rejection Memo

                            var rejectionMemoList = (from rejMemo in firstOrDefault.RejectionMemoes
                                                     where rejMemo.BillingCode == lineItem.BillingCode
                                                     select rejMemo).ToList();

                            foreach (var newRejectionMemo in rejectionMemoList)
                            {
                                newRejectionMemo.BatchsequenceNumber = batchNo;
                                newRejectionMemo.RecordSequenceWithinBatch = seqNo;
                                seqNo = seqNo + 1;

                                #region RM Airwaybill

                                int brkDwnSrNoRM = 1;
                                foreach (var rMAirWayBill in newRejectionMemo.RMAirWayBills)
                                {
                                    rMAirWayBill.BreakdownSerialNumber = brkDwnSrNoRM;
                                    _sisDB.Entry(rMAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                    brkDwnSrNoRM = brkDwnSrNoRM + 1;
                                }

                                #endregion RM Airwaybill

                                _sisDB.Entry(newRejectionMemo).State = System.Data.Entity.EntityState.Modified;
                                _sisDB.SaveChanges();
                            }

                            #endregion Rejection Memo

                            #region Billing Memo

                            var billingMemoList = (from billingMemo in firstOrDefault.BillingMemoes
                                                   where billingMemo.BillingCode == lineItem.BillingCode
                                                   select billingMemo).ToList();

                            foreach (var newBillingMemo in billingMemoList)
                            {
                                newBillingMemo.BatchSequenceNumber = batchNo;
                                newBillingMemo.RecordSequenceWithinBatch = seqNo;
                                seqNo = seqNo + 1;

                                #region BM Airwaybill

                                int brkDwnSrNoBM = 1;
                                foreach (var bMAirWayBill in newBillingMemo.BMAirWayBills)
                                {
                                    bMAirWayBill.BreakdownSerialNumber = brkDwnSrNoBM;
                                    _sisDB.Entry(bMAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                    brkDwnSrNoBM = brkDwnSrNoBM + 1;
                                }

                                #endregion BM Airwaybill

                                _sisDB.Entry(newBillingMemo).State = System.Data.Entity.EntityState.Modified;
                                _sisDB.SaveChanges();
                            }

                            #endregion Billing Memo

                            #region Credit Memo

                            var creditMemoList = (from creditMemo in firstOrDefault.CreditMemoes
                                                  where creditMemo.BillingCode == lineItem.BillingCode
                                                  select creditMemo).ToList();

                            foreach (var newCreditMemo in creditMemoList)
                            {
                                newCreditMemo.BatchSequenceNumber = batchNo;
                                newCreditMemo.RecordSequenceWithinBatch = seqNo;
                                seqNo = seqNo + 1;

                                #region CM Airwaybill

                                int brkDwnSrNoCM = 1;
                                foreach (var cMAirWayBill in newCreditMemo.CMAirWayBills)
                                {
                                    cMAirWayBill.BreakdownSerialNumber = brkDwnSrNoCM;
                                    _sisDB.Entry(cMAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                    brkDwnSrNoCM = brkDwnSrNoCM + 1;
                                }

                                #endregion CM Airwaybill

                                _sisDB.Entry(newCreditMemo).State = System.Data.Entity.EntityState.Modified;
                                _sisDB.SaveChanges();
                            }

                            #endregion Credit Memo
                        }
                    }
                    batchNo = batchNo + 1;
                }
            }
            _sisDB.Dispose();
            return true;
        }

        #region IS Validation report update status and insert.

        /// <summary>
        /// Method to Update the Status at AWB/Invoice/File level & Load Validation report into database.
        /// </summary>
        /// <param name="listISValidationSummaryReportR1">R1 Report</param>
        /// <param name="listISValidationDetailErrorReporR2">R2 Report</param>
        /// <param name="userName">Created/Updated By</param>
        /// <param name="rejectionOnValidationFailure">File/Invoice In Error Flag</param>
        /// <param name="onlineCorrectionAllowed">Online Correction Allowed Flag</param>
        /// <returns></returns>
        public bool UpdateStatusAndInsertISValidationReportR1R2(List<ModelClass.ISValidationReport.ISValidationSummaryReport> listISValidationSummaryReportR1,
                                                                List<ModelClass.ISValidationReport.ISValidationDetailErrorReport> listISValidationDetailErrorReporR2,
                                                                string userName, int rejectionOnValidationFailure, bool onlineCorrectionAllowed, ref string strAzulOracleInvList)
        {


            using (var context = _sisDB)
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        #region Update statuses in DB & Update Id in list.
                        clsLog.WriteLogAzure("UpdateStatusAndInsertISValidationReportR1R2 start");
                        bool fileStatusErrorInValidation = false;

                        var isValidationSummaryReport = listISValidationSummaryReportR1.FirstOrDefault();
                        if (isValidationSummaryReport != null)
                        {
                            string iSValidationSummaryReportR1FileName = isValidationSummaryReport.BillingFileName.ToString().ToUpper();

                            if (iSValidationSummaryReportR1FileName.Substring(iSValidationSummaryReportR1FileName.Length - 4).ToUpper().Equals(".XML"))
                            {
                                iSValidationSummaryReportR1FileName = iSValidationSummaryReportR1FileName.Replace(".XML", ".ZIP");
                            }
                            else
                            {
                                iSValidationSummaryReportR1FileName = iSValidationSummaryReportR1FileName.Replace(".DAT", ".ZIP");
                            }

                            // Get the File Header entry for which Validation report is.
                            DbEntity.FileHeader fileHeader = _sisDB.FileHeaders.FirstOrDefault(fh => fh.FileName.Equals(iSValidationSummaryReportR1FileName));

                            if (fileHeader != null)
                            {
                                List<int> listIHIDs = _sisDB.InvoiceHeaders.Where(ih => ih.FileHeaderID == fileHeader.FileHeaderID).Select(inh => inh.InvoiceHeaderID).ToList();

                                for (int ih = 0; ih < listIHIDs.Count(); ih++)
                                {
                                    int invId = listIHIDs[ih];

                                    List<DbEntity.InvoiceHeader> listinvoiceHeaders = _sisDB.InvoiceHeaders.Where(inh => inh.InvoiceHeaderID == invId).ToList();

                                    List<DbEntity.InvoiceHeader> listInvoiceHeadersToUpdate = new List<InvoiceHeader>();
                                    foreach (var listinvoiceHeader in listinvoiceHeaders)
                                    {
                                        listInvoiceHeadersToUpdate.Add(listinvoiceHeader);
                                    }

                                    foreach (var invoiceHeader in listInvoiceHeadersToUpdate)
                                    {
                                        if (invoiceHeader.InvoiceHeaderID > 0)
                                        {
                                            for (int i = 0; i < listISValidationSummaryReportR1.Count; i++)
                                            {
                                                if (invoiceHeader.InvoiceNumber == listISValidationSummaryReportR1[i].InvoiceNumber &&
                                                    invoiceHeader.BillingAirline == listISValidationSummaryReportR1[i].BillingEntityCode.PadLeft(3, '0') &&
                                                    invoiceHeader.BilledAirline == listISValidationSummaryReportR1[i].BilledEntityCode.PadLeft(3, '0'))
                                                {
                                                    // Update InvoiceHeaderID in R1 list
                                                    listISValidationSummaryReportR1[i].ValidationForInvoiceID = invoiceHeader.InvoiceHeaderID;

                                                    // Update InvoiceStausId in Database.
                                                    switch (listISValidationSummaryReportR1[i].InvoiceStatus)
                                                    {
                                                        case 'Z': // Sanity Check Error.
                                                            invoiceHeader.InvoiceStatusId = 4; // Error-In-Validation
                                                            fileStatusErrorInValidation = true;
                                                            break;
                                                        case 'X': // Error - Non Correctable
                                                            invoiceHeader.InvoiceStatusId = 4; // Error-In-Validation
                                                            fileStatusErrorInValidation = true;
                                                            break;
                                                        case 'C': // 
                                                            if (onlineCorrectionAllowed)
                                                            {
                                                                invoiceHeader.InvoiceStatusId = 5; // IS Validated
                                                                strAzulOracleInvList = strAzulOracleInvList + "," + invoiceHeader.InvoiceNumber.ToString();
                                                            }
                                                            else
                                                            {
                                                                invoiceHeader.InvoiceStatusId = 4; // Error-In-Validation
                                                                fileStatusErrorInValidation = true;
                                                            }
                                                            break;
                                                        default:
                                                            {
                                                                invoiceHeader.InvoiceStatusId = 5; // IS Validated
                                                                strAzulOracleInvList = strAzulOracleInvList + "," + invoiceHeader.InvoiceNumber.ToString();
                                                            }
                                                            break;
                                                    }
                                                }
                                            }



                                            // Airwaybill
                                            var airWayBillIds = _sisDB.AirWayBills.Where(awb => awb.InvoiceHeaderID == invoiceHeader.InvoiceHeaderID).Select(a => a.AirWayBillID).ToList();

                                            for (int awbNo = 0; awbNo < airWayBillIds.Count(); awbNo++)
                                            {
                                                int awBillID = airWayBillIds[awbNo];
                                                DbEntity.AirWayBill newAirWayBill = _sisDB.AirWayBills.FirstOrDefault(awb => awb.AirWayBillID == awBillID);

                                                if (listISValidationDetailErrorReporR2.Count > 0)
                                                {
                                                    // For R2 Report
                                                    for (int i = 0; i < listISValidationDetailErrorReporR2.Count; i++)
                                                    {
                                                        if (listISValidationDetailErrorReporR2[i].CGOBatchNumber == newAirWayBill.BatchSequenceNumber &&
                                                            listISValidationDetailErrorReporR2[i].CGOSeqNumber == newAirWayBill.RecordSequencewithinBatch &&
                                                            listISValidationDetailErrorReporR2[i].CGOBillingCode == newAirWayBill.BillingCode &&
                                                            listISValidationDetailErrorReporR2[i].ValidationForAWBID == 0 &&
                                                            string.IsNullOrWhiteSpace(listISValidationDetailErrorReporR2[i].ValidationForAWBIDFromTable))
                                                        {
                                                            listISValidationDetailErrorReporR2[i].ValidationForAWBID = awBillID;
                                                            listISValidationDetailErrorReporR2[i].ValidationForAWBIDFromTable = "SINWAWB";

                                                            newAirWayBill.AWBStatusID = 7; // Error-In-Vaidation                                                        
                                                        }
                                                        else
                                                        {
                                                            newAirWayBill.AWBStatusID = 8; // IS-Validated
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    newAirWayBill.AWBStatusID = 8; // IS-Validated
                                                }

                                                newAirWayBill.LastUpdatedBy = userName;
                                                newAirWayBill.LastUpdatedOn = DateTime.UtcNow;
                                                _sisDB.Entry(newAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                                _sisDB.SaveChanges();
                                                clsLog.WriteLogAzure("UpdateStatusAndInsertISValidationReportR1R2 AirWayBill Saved");
                                            }

                                            // Rejection Memo
                                            var rejectionMemoIDs = _sisDB.RejectionMemoes.Where(rm => rm.InvoiceHeaderID == invoiceHeader.InvoiceHeaderID).Select(r => r.RejectionMemoID).ToList();
                                            for (int rmNo = 0; rmNo < rejectionMemoIDs.Count(); rmNo++)
                                            {
                                                int rmId = rejectionMemoIDs[rmNo];
                                                DbEntity.RejectionMemo newRejectionMemo = _sisDB.RejectionMemoes.FirstOrDefault(rm => rm.RejectionMemoID == rmId);

                                                // RM Airwaybill
                                                var rMAwbIDs = _sisDB.RMAirWayBills.Where(rmawb => rmawb.RejectionMemoID == newRejectionMemo.RejectionMemoID).Select(ra => ra.RMAirWayBillID).ToList();

                                                for (int brkDwnSrNo = 0; brkDwnSrNo < rMAwbIDs.Count(); brkDwnSrNo++)
                                                {
                                                    int rmawbId = rMAwbIDs[brkDwnSrNo];
                                                    DbEntity.RMAirWayBill rMAirWayBill = _sisDB.RMAirWayBills.FirstOrDefault(rmawb => rmawb.RMAirWayBillID == rmawbId);

                                                    if (listISValidationDetailErrorReporR2.Count > 0)
                                                    {
                                                        // For R2 Report
                                                        for (int i = 0; i < listISValidationDetailErrorReporR2.Count; i++)
                                                        {
                                                            if (listISValidationDetailErrorReporR2[i].CGOBatchNumber == newRejectionMemo.BatchsequenceNumber &&
                                                                listISValidationDetailErrorReporR2[i].CGOSeqNumber == newRejectionMemo.RecordSequenceWithinBatch &&
                                                                listISValidationDetailErrorReporR2[i].MainDocNo == string.Concat(rMAirWayBill.AWBIssuingAirline, rMAirWayBill.AWBSerialNumber.ToString().PadLeft(7, '0')) &&
                                                                listISValidationDetailErrorReporR2[i].CGOBillingCode == rMAirWayBill.BillingCode &&
                                                                listISValidationDetailErrorReporR2[i].ValidationForAWBID == 0 &&
                                                                string.IsNullOrWhiteSpace(listISValidationDetailErrorReporR2[i].ValidationForAWBIDFromTable))
                                                            {
                                                                listISValidationDetailErrorReporR2[i].ValidationForAWBID = rmawbId;
                                                                listISValidationDetailErrorReporR2[i].ValidationForAWBIDFromTable = "SIRMAWB";

                                                                rMAirWayBill.AWBStatusID = 7; // Error-In-Vaidation
                                                            }
                                                            else
                                                            {
                                                                rMAirWayBill.AWBStatusID = 8; // IS-Validated
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        rMAirWayBill.AWBStatusID = 8; // IS-Validated
                                                    }

                                                    rMAirWayBill.LastUpdatedBy = userName;
                                                    rMAirWayBill.LastUpdatedOn = DateTime.UtcNow;
                                                    _sisDB.Entry(rMAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                                    _sisDB.SaveChanges();
                                                }
                                            }

                                            // Billing Memo
                                            var billlingMemoIDs = _sisDB.BillingMemoes.Where(bm => bm.InvoiceHeaderID == invoiceHeader.InvoiceHeaderID).Select(b => b.BillingMemoID).ToList();
                                            for (int bmNo = 0; bmNo < billlingMemoIDs.Count(); bmNo++)
                                            {
                                                int bmId = billlingMemoIDs[bmNo];
                                                DbEntity.BillingMemo newBillingMemo = _sisDB.BillingMemoes.FirstOrDefault(bm => bm.BillingMemoID == bmId);

                                                // BM Airwaybill
                                                var bMAwbIDs = _sisDB.BMAirWayBills.Where(bmawb => bmawb.BillingMemoID == newBillingMemo.BillingMemoID).Select(ba => ba.BMAirWayBillId).ToList();
                                                for (int brkDwnSrNo = 0; brkDwnSrNo < bMAwbIDs.Count(); brkDwnSrNo++)
                                                {
                                                    int bmawbId = bMAwbIDs[brkDwnSrNo];
                                                    DbEntity.BMAirWayBill bMAirWayBill = _sisDB.BMAirWayBills.FirstOrDefault(bmawb => bmawb.BMAirWayBillId == bmawbId);

                                                    if (listISValidationDetailErrorReporR2.Count > 0)
                                                    {
                                                        // For R2 Report
                                                        for (int i = 0; i < listISValidationDetailErrorReporR2.Count; i++)
                                                        {
                                                            if (listISValidationDetailErrorReporR2[i].CGOBatchNumber == newBillingMemo.BatchSequenceNumber &&
                                                                listISValidationDetailErrorReporR2[i].CGOSeqNumber == newBillingMemo.RecordSequenceWithinBatch &&
                                                                listISValidationDetailErrorReporR2[i].MainDocNo == string.Concat(bMAirWayBill.AWBIssuingAirline, bMAirWayBill.AWBSerialNumber.ToString().PadLeft(7, '0')) &&
                                                                listISValidationDetailErrorReporR2[i].CGOBillingCode == bMAirWayBill.BillingCode &&
                                                                listISValidationDetailErrorReporR2[i].ValidationForAWBID == 0 &&
                                                                string.IsNullOrWhiteSpace(listISValidationDetailErrorReporR2[i].ValidationForAWBIDFromTable))
                                                            {
                                                                listISValidationDetailErrorReporR2[i].ValidationForAWBID = bmawbId;
                                                                listISValidationDetailErrorReporR2[i].ValidationForAWBIDFromTable = "SIBMAWB";

                                                                bMAirWayBill.AWBStatusID = 7; // Error-In-Vaidation
                                                            }
                                                            else
                                                            {
                                                                bMAirWayBill.AWBStatusID = 8; // IS-Validated
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        bMAirWayBill.AWBStatusID = 8; // IS-Validated
                                                    }

                                                    bMAirWayBill.LastUpdatedBy = userName;
                                                    bMAirWayBill.LastUpdatedOn = DateTime.UtcNow;
                                                    _sisDB.Entry(bMAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                                    _sisDB.SaveChanges();
                                                }
                                            }

                                            // Credit Memo
                                            var creditMemoIDs = _sisDB.CreditMemoes.Where(cm => cm.InvoiceHeaderID == invoiceHeader.InvoiceHeaderID).Select(c => c.CreditMemoID).ToList();
                                            for (int cmNo = 0; cmNo < creditMemoIDs.Count(); cmNo++)
                                            {
                                                int cmId = creditMemoIDs[cmNo];
                                                DbEntity.CreditMemo newCreditMemo = _sisDB.CreditMemoes.FirstOrDefault(cm => cm.CreditMemoID == cmId);

                                                // CM Airwaybill
                                                var cMAwbIDs = _sisDB.CMAirWayBills.Where(cmawb => cmawb.CreditMemoID == newCreditMemo.CreditMemoID).Select(ca => ca.CMAirWayBillID).ToList();
                                                for (int brkDwnSrNo = 0; brkDwnSrNo < cMAwbIDs.Count(); brkDwnSrNo++)
                                                {
                                                    int cmawbId = cMAwbIDs[brkDwnSrNo];
                                                    DbEntity.CMAirWayBill cMAirWayBill = _sisDB.CMAirWayBills.FirstOrDefault(cmawb => cmawb.CMAirWayBillID == cmawbId);

                                                    if (listISValidationDetailErrorReporR2.Count > 0)
                                                    {
                                                        // For R2 Report
                                                        for (int i = 0; i < listISValidationDetailErrorReporR2.Count; i++)
                                                        {
                                                            if (listISValidationDetailErrorReporR2[i].CGOBatchNumber == newCreditMemo.BatchSequenceNumber &&
                                                                listISValidationDetailErrorReporR2[i].CGOSeqNumber == newCreditMemo.RecordSequenceWithinBatch &&
                                                                listISValidationDetailErrorReporR2[i].MainDocNo == string.Concat(cMAirWayBill.AWBIssuingAirline, cMAirWayBill.AWBSerialNumber.ToString().PadLeft(7, '0')) &&
                                                                listISValidationDetailErrorReporR2[i].CGOBillingCode == cMAirWayBill.BillingCode &&
                                                                listISValidationDetailErrorReporR2[i].ValidationForAWBID == 0 &&
                                                                string.IsNullOrWhiteSpace(listISValidationDetailErrorReporR2[i].ValidationForAWBIDFromTable))
                                                            {
                                                                listISValidationDetailErrorReporR2[i].ValidationForAWBID = cmawbId;
                                                                listISValidationDetailErrorReporR2[i].ValidationForAWBIDFromTable = "SICMAWB";

                                                                cMAirWayBill.AWBStatusID = 7; // Error-In-Vaidation
                                                            }
                                                            else
                                                            {
                                                                cMAirWayBill.AWBStatusID = 8; // IS-Validated
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        cMAirWayBill.AWBStatusID = 8; // IS-Validated
                                                    }

                                                    cMAirWayBill.LastUpdatedBy = userName;
                                                    cMAirWayBill.LastUpdatedOn = DateTime.UtcNow;
                                                    _sisDB.Entry(cMAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                                    _sisDB.SaveChanges();
                                                }
                                            }
                                        }
                                    }
                                }

                                // update file status
                                if (fileStatusErrorInValidation)
                                {
                                    // Update file status
                                    fileHeader.FileStatusID = 3; // Error-In-Validation.

                                    // Reject File In Error
                                    if (rejectionOnValidationFailure == 0)
                                    {
                                        // Update all the Invoices/AWBs status in database as Error-In-Validation within that file.
                                        List<int> listInvoiceHeaderID = _sisDB.InvoiceHeaders.Where(ih => ih.FileHeaderID == fileHeader.FileHeaderID).Select(ih => ih.InvoiceHeaderID).ToList();

                                        for (int ih = 0; ih < listInvoiceHeaderID.Count(); ih++)
                                        {
                                            int invId = listInvoiceHeaderID[ih];

                                            DbEntity.InvoiceHeader invoiceHeader = _sisDB.InvoiceHeaders.FirstOrDefault(inh => inh.InvoiceHeaderID == invId);

                                            if (invoiceHeader.InvoiceHeaderID > 0)
                                            {
                                                var airWayBillIds = _sisDB.AirWayBills.Where(awb => awb.InvoiceHeaderID == invoiceHeader.InvoiceHeaderID).Select(a => a.AirWayBillID).ToList();

                                                for (int awbNo = 0; awbNo < airWayBillIds.Count(); awbNo++)
                                                {
                                                    int awBillID = airWayBillIds[awbNo];
                                                    DbEntity.AirWayBill newAirWayBill = _sisDB.AirWayBills.FirstOrDefault(awb => awb.AirWayBillID == awBillID);
                                                    newAirWayBill.AWBStatusID = 7; // Error-In-Vaidation
                                                    newAirWayBill.LastUpdatedBy = userName;
                                                    newAirWayBill.LastUpdatedOn = DateTime.UtcNow;
                                                    _sisDB.Entry(newAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                                    _sisDB.SaveChanges();
                                                }

                                                var rejectionMemoIDs = _sisDB.RejectionMemoes.Where(rm => rm.InvoiceHeaderID == invoiceHeader.InvoiceHeaderID).Select(r => r.RejectionMemoID).ToList();

                                                for (int rmNo = 0; rmNo < rejectionMemoIDs.Count(); rmNo++)
                                                {
                                                    int rmId = rejectionMemoIDs[rmNo];
                                                    DbEntity.RejectionMemo newRejectionMemo = _sisDB.RejectionMemoes.FirstOrDefault(rm => rm.RejectionMemoID == rmId);

                                                    var rMAwbIDs = _sisDB.RMAirWayBills.Where(rmawb => rmawb.RejectionMemoID == newRejectionMemo.RejectionMemoID).Select(ra => ra.RMAirWayBillID).ToList();

                                                    for (int brkDwnSrNo = 0; brkDwnSrNo < rMAwbIDs.Count(); brkDwnSrNo++)
                                                    {
                                                        int rmawbId = rMAwbIDs[brkDwnSrNo];
                                                        DbEntity.RMAirWayBill rMAirWayBill = _sisDB.RMAirWayBills.FirstOrDefault(rmawb => rmawb.RMAirWayBillID == rmawbId);

                                                        rMAirWayBill.AWBStatusID = 7; // Error-In-Vaidation
                                                        rMAirWayBill.LastUpdatedBy = userName;
                                                        rMAirWayBill.LastUpdatedOn = DateTime.UtcNow;
                                                        _sisDB.Entry(rMAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                                        _sisDB.SaveChanges();
                                                    }
                                                }

                                                var billlingMemoIDs = _sisDB.BillingMemoes.Where(bm => bm.InvoiceHeaderID == invoiceHeader.InvoiceHeaderID).Select(b => b.BillingMemoID).ToList();

                                                for (int bmNo = 0; bmNo < billlingMemoIDs.Count(); bmNo++)
                                                {
                                                    int bmId = billlingMemoIDs[bmNo];
                                                    DbEntity.BillingMemo newBillingMemo = _sisDB.BillingMemoes.FirstOrDefault(bm => bm.BillingMemoID == bmId);

                                                    var bMAwbIDs = _sisDB.BMAirWayBills.Where(bmawb => bmawb.BillingMemoID == newBillingMemo.BillingMemoID).Select(ba => ba.BMAirWayBillId).ToList();

                                                    for (int brkDwnSrNo = 0; brkDwnSrNo < bMAwbIDs.Count(); brkDwnSrNo++)
                                                    {
                                                        int bmawbId = bMAwbIDs[brkDwnSrNo];
                                                        DbEntity.BMAirWayBill bMAirWayBill = _sisDB.BMAirWayBills.FirstOrDefault(bmawb => bmawb.BMAirWayBillId == bmawbId);

                                                        bMAirWayBill.AWBStatusID = 7; // Error-In-Vaidation
                                                        bMAirWayBill.LastUpdatedBy = userName;
                                                        bMAirWayBill.LastUpdatedOn = DateTime.UtcNow;
                                                        _sisDB.Entry(bMAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                                        _sisDB.SaveChanges();
                                                    }
                                                }

                                                var creditMemoIDs = _sisDB.CreditMemoes.Where(cm => cm.InvoiceHeaderID == invoiceHeader.InvoiceHeaderID).Select(c => c.CreditMemoID).ToList();

                                                for (int cmNo = 0; cmNo < creditMemoIDs.Count(); cmNo++)
                                                {
                                                    int cmId = creditMemoIDs[cmNo];
                                                    DbEntity.CreditMemo newCreditMemo = _sisDB.CreditMemoes.FirstOrDefault(cm => cm.CreditMemoID == cmId);

                                                    var cMAwbIDs = _sisDB.CMAirWayBills.Where(cmawb => cmawb.CreditMemoID == newCreditMemo.CreditMemoID).Select(ca => ca.CMAirWayBillID).ToList();

                                                    for (int brkDwnSrNo = 0; brkDwnSrNo < cMAwbIDs.Count(); brkDwnSrNo++)
                                                    {
                                                        int cmawbId = cMAwbIDs[brkDwnSrNo];
                                                        DbEntity.CMAirWayBill cMAirWayBill = _sisDB.CMAirWayBills.FirstOrDefault(cmawb => cmawb.CMAirWayBillID == cmawbId);

                                                        cMAirWayBill.AWBStatusID = 7; // Error-In-Vaidation
                                                        cMAirWayBill.LastUpdatedBy = userName;
                                                        cMAirWayBill.LastUpdatedOn = DateTime.UtcNow;
                                                        _sisDB.Entry(cMAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                                        _sisDB.SaveChanges();
                                                    }
                                                }
                                            }

                                            invoiceHeader.InvoiceStatusId = 4; // Error-In-Validation
                                            invoiceHeader.LastUpdatedBy = userName;
                                            invoiceHeader.LastUpdatedOn = DateTime.UtcNow;
                                            _sisDB.Entry(invoiceHeader).State = System.Data.Entity.EntityState.Modified;
                                            _sisDB.SaveChanges();
                                            clsLog.WriteLogAzure("UpdateStatusAndInsertISValidationReportR1R2 InvoiceHeader Saved");
                                        }
                                    }
                                    // Reject Invoice In Error
                                    // else 
                                    // { // do nothing.}
                                }
                                else
                                {
                                    fileHeader.FileStatusID = 4; // IS-Validated.
                                }

                                _sisDB.Entry(fileHeader).State = System.Data.Entity.EntityState.Modified;
                            }
                            else
                            {
                                clsLog.WriteLogAzure("File {0} not found in System." + iSValidationSummaryReportR1FileName);
                            }
                        }
                        _sisDB.SaveChanges();
                        clsLog.WriteLogAzure("UpdateStatusAndInsertISValidationReportR1R2 FileHeader Saved");
                        #endregion

                        #region Insert R1 to DB

                        if (listISValidationSummaryReportR1.Count > 0)
                        {
                            foreach (var iSValidationSummaryReportR1 in listISValidationSummaryReportR1)
                            {
                                DbEntity.ISValidationSummaryReport newISValidationSummaryReport = new DbEntity.ISValidationSummaryReport();

                                newISValidationSummaryReport.SerialNo = iSValidationSummaryReportR1.SerialNo;
                                newISValidationSummaryReport.BillingEntityCode = iSValidationSummaryReportR1.BillingEntityCode;
                                newISValidationSummaryReport.BillingYear = iSValidationSummaryReportR1.BillingYear;
                                newISValidationSummaryReport.BillingMonth = iSValidationSummaryReportR1.BillingMonth;
                                newISValidationSummaryReport.BillingPeriod = iSValidationSummaryReportR1.BillingPeriod;
                                newISValidationSummaryReport.BillingCategory = iSValidationSummaryReportR1.BillingCategory.ToString();
                                newISValidationSummaryReport.BillingFileName = iSValidationSummaryReportR1.BillingFileName;
                                newISValidationSummaryReport.BillingFileSubmissionDate = iSValidationSummaryReportR1.BillingFileSubmissionDate;
                                newISValidationSummaryReport.SubmissionFormat = iSValidationSummaryReportR1.SubmissionFormat;
                                newISValidationSummaryReport.BilledEntityCode = iSValidationSummaryReportR1.BilledEntityCode;
                                newISValidationSummaryReport.InvoiceNumber = iSValidationSummaryReportR1.InvoiceNumber;
                                newISValidationSummaryReport.CurrencyOfBilling = iSValidationSummaryReportR1.CurrencyOfBilling;
                                newISValidationSummaryReport.InvoiceAmountInBillingCurrency = iSValidationSummaryReportR1.InvoiceAmountInBillingCurrency;
                                newISValidationSummaryReport.InvoiceStatus = iSValidationSummaryReportR1.InvoiceStatus.ToString();
                                newISValidationSummaryReport.ErrorAtInvoiceLevel = iSValidationSummaryReportR1.ErrorAtInvoiceLevel.ToString();
                                newISValidationSummaryReport.TotalNoOfBillingRecords = iSValidationSummaryReportR1.TotalNoOfBillingRecords;
                                newISValidationSummaryReport.TotalNoOfSuccessfullyValidatedRecords = iSValidationSummaryReportR1.TotalNoOfSuccessfullyValidatedRecords;
                                newISValidationSummaryReport.TotalNoOfRecordsInValidationError = iSValidationSummaryReportR1.TotalNoOfRecordsInValidationError;
                                newISValidationSummaryReport.ValidationForInvoiceID = iSValidationSummaryReportR1.ValidationForInvoiceID;
                                newISValidationSummaryReport.CreatedBy = userName;
                                newISValidationSummaryReport.CreatedOn = DateTime.UtcNow;
                                newISValidationSummaryReport.LastUpdatedBy = userName;
                                newISValidationSummaryReport.LastUpdatedOn = DateTime.UtcNow;

                                _sisDB.ISValidationSummaryReports.Add(newISValidationSummaryReport);
                            }
                        }

                        #endregion

                        #region Insert R2 to DB

                        if (listISValidationDetailErrorReporR2.Count > 0)
                        {
                            foreach (var iSValidationDetailErrorReporR2 in listISValidationDetailErrorReporR2)
                            {
                                DbEntity.ISValidationDetailErrorReport newISValidationDetailErrorReport = new DbEntity.ISValidationDetailErrorReport();

                                newISValidationDetailErrorReport.SerialNo = iSValidationDetailErrorReporR2.SerialNo;
                                newISValidationDetailErrorReport.BillingEntityCode = iSValidationDetailErrorReporR2.BillingEntityCode;
                                newISValidationDetailErrorReport.BillingYear = iSValidationDetailErrorReporR2.BillingYear;
                                newISValidationDetailErrorReport.BillingMonth = iSValidationDetailErrorReporR2.BillingMonth;
                                newISValidationDetailErrorReport.BillingPeriod = iSValidationDetailErrorReporR2.BillingPeriod;
                                newISValidationDetailErrorReport.BillingCategory = iSValidationDetailErrorReporR2.BillingCategory.ToString();
                                newISValidationDetailErrorReport.BillingFileName = iSValidationDetailErrorReporR2.BillingFileName;
                                newISValidationDetailErrorReport.BillingFileSubmissionDate = iSValidationDetailErrorReporR2.BillingFileSubmissionDate;
                                newISValidationDetailErrorReport.SubmissionFormat = iSValidationDetailErrorReporR2.SubmissionFormat;
                                newISValidationDetailErrorReport.BilledEntityCode = iSValidationDetailErrorReporR2.BilledEntityCode;
                                newISValidationDetailErrorReport.InvoiceNumber = iSValidationDetailErrorReporR2.InvoiceNumber;
                                newISValidationDetailErrorReport.CGOBillingCode = iSValidationDetailErrorReporR2.CGOBillingCode;
                                newISValidationDetailErrorReport.CGOBlank = iSValidationDetailErrorReporR2.CGOBlank;
                                newISValidationDetailErrorReport.CGOBatchNumber = iSValidationDetailErrorReporR2.CGOBatchNumber;
                                newISValidationDetailErrorReport.CGOSeqNumber = iSValidationDetailErrorReporR2.CGOSeqNumber;
                                newISValidationDetailErrorReport.MainDocNo = iSValidationDetailErrorReporR2.MainDocNo;
                                newISValidationDetailErrorReport.LinkedDocNo = iSValidationDetailErrorReporR2.LinkedDocNo;
                                newISValidationDetailErrorReport.ErrorCode = iSValidationDetailErrorReporR2.ErrorCode;
                                newISValidationDetailErrorReport.ErrorLevel = iSValidationDetailErrorReporR2.ErrorLevel;
                                newISValidationDetailErrorReport.FieldName = iSValidationDetailErrorReporR2.FieldName;
                                newISValidationDetailErrorReport.FieldValue = iSValidationDetailErrorReporR2.FieldValue;
                                newISValidationDetailErrorReport.ErrorDescription = iSValidationDetailErrorReporR2.ErrorDescription;
                                newISValidationDetailErrorReport.ErrorStatus = iSValidationDetailErrorReporR2.ErrorStatus.ToString();
                                newISValidationDetailErrorReport.ValidationForAWBID = iSValidationDetailErrorReporR2.ValidationForAWBID;
                                newISValidationDetailErrorReport.ValidationForAWBIDFromTable = iSValidationDetailErrorReporR2.ValidationForAWBIDFromTable;
                                newISValidationDetailErrorReport.CreatedBy = userName;
                                newISValidationDetailErrorReport.CreatedOn = DateTime.UtcNow;
                                newISValidationDetailErrorReport.LastUpdatedBy = userName;
                                newISValidationDetailErrorReport.LastUpdatedOn = DateTime.UtcNow;

                                _sisDB.ISValidationDetailErrorReports.Add(newISValidationDetailErrorReport);
                            }
                        }

                        #endregion

                        context.SaveChanges();
                        dbContextTransaction.Commit();
                        clsLog.WriteLogAzure("UpdateStatusAndInsertISValidationReportR1R2 Final Commit");
                        return true;
                    }
                    catch (Exception exception)
                    {
                        dbContextTransaction.Rollback();
                        clsLog.WriteLogAzure("Error Occurred in UpdateStatusAndInsertISValidationReportR1R2", exception.InnerException);
                        return false;
                    }
                }
            }
        }

        #endregion

        public bool CreateReceivedISValidationFileHeaderData(string isValidationFileName, string isValidationFileBlobUrl, string isValidationLogFileBlobUrl, int receivablesFileID, string userName)
        {
            using (var context = _sisDB)
            {
                try
                {

                    var dbISValidationFile = _sisDB.ISValidationFileHeaders.FirstOrDefault(iv => iv.FileName.ToUpper() == isValidationFileName &&
                                                               iv.ReceivablesFileHeaderID == receivablesFileID);
                    // if file already exist then return
                    if (dbISValidationFile != null)
                    {
                        dbISValidationFile.IsProcessed = true;
                        dbISValidationFile.LogFilePath = isValidationLogFileBlobUrl;
                        _sisDB.SaveChanges();
                        context.SaveChanges();
                        return true;
                    }
                    DbEntity.ISValidationFileHeader newISValidationFileHeader = new ISValidationFileHeader
                    {
                        FileName = isValidationFileName,
                        FilePath = isValidationFileBlobUrl,
                        LogFilePath = isValidationLogFileBlobUrl,
                        ReceivablesFileHeaderID = receivablesFileID,
                        CreatedBy = userName,
                        CreatedOn = DateTime.UtcNow,
                        LastUpdatedBy = userName,
                        LastUpdatedOn = DateTime.UtcNow,
                        ReadWriteOnSFTP = DateTime.UtcNow,
                        IsProcessed = false
                    };

                    _sisDB.ISValidationFileHeaders.Add(newISValidationFileHeader);
                    _sisDB.SaveChanges();
                    context.SaveChanges();

                    return true;
                }
                catch (Exception exception)
                {
                    clsLog.WriteLogAzure("Error Occurred in CreateReceivedISValidationFileHeaderData", exception.InnerException);
                    return false;
                }
            }
        }

        public bool InsertSISFileHeaderData(ModelClass.SupportingModels.FileData fileData, out int newFileHeaderIdRetrived)
        {
            newFileHeaderIdRetrived = 0;
            using (var context = _sisDB)
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var dbFileHeader = _sisDB.FileHeaders.FirstOrDefault(fh => fh.FileName.ToUpper() == fileData.FileHeader.FileName);
                        // if file already exist then return
                        if (dbFileHeader != null)
                        {
                            return true;
                        }
                        else
                        {
                            DbEntity.FileHeader newFileHeader = new DbEntity.FileHeader
                            {
                                AirlineCode = fileData.FileHeader.AirlineCode,
                                VersionNumber = fileData.FileHeader.VersionNumber,
                                FileInOutDirection = fileData.FileHeader.FileInOutDirection,
                                FileName = fileData.FileHeader.FileName,
                                FilePath = fileData.FileHeader.FilePath,
                                CreatedBy = fileData.FileHeader.CreatedBy,
                                CreatedOn = fileData.FileHeader.CreatedOn,
                                LastUpdatedBy = fileData.FileHeader.LastUpdatedBy,
                                LastUpdatedOn = DateTime.UtcNow,
                                FileStatusID = fileData.FileHeader.FileStatusId,
                                ReadWriteOnSFTP = fileData.FileHeader.ReadWriteOnSFTP,
                                IsProcessed = Convert.ToBoolean(fileData.FileHeader.IsProcessed)
                            };

                            _sisDB.FileHeaders.Add(newFileHeader);
                            _sisDB.SaveChanges();

                            newFileHeaderIdRetrived = newFileHeader.FileHeaderID;
                        }
                        context.SaveChanges();
                        dbContextTransaction.Commit();

                        return true;
                    }
                    catch (DbEntityValidationException exception)
                    {
                        dbContextTransaction.Rollback();
                        newFileHeaderIdRetrived = 0;
                        clsLog.WriteLogAzure("Exception occured in InsertReceivedFileData: ");
                        foreach (var item in ((System.Data.Entity.Validation.DbEntityValidationException)exception).EntityValidationErrors.FirstOrDefault().ValidationErrors)
                        {
                            clsLog.WriteLogAzure("Entity Validation Error: {0}" + item.ErrorMessage.ToString());
                        }

                        return false;
                    }
                    catch (Exception exception)
                    {
                        dbContextTransaction.Rollback();
                        newFileHeaderIdRetrived = 0;
                        clsLog.WriteLogAzure(exception.InnerException);

                        return false;
                    }
                }
            }
        }
    }
}