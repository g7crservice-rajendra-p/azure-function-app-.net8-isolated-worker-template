using System;
using System.Collections.Generic;
using System.Linq;
using ModelClass = QidWorkerRole.SIS.Model;
using DbEntity = QidWorkerRole.SIS.DAL;

namespace QidWorkerRole.SIS.DAL
{
    /// <summary>
    /// Performs all Database Update Operations.
    /// </summary>
    public class UpdateDBData
    {
        public SIS.DAL.SISDBEntities _sisDB;

        public UpdateDBData()
        {
            _sisDB = new SIS.DAL.SISDBEntities();
        }

        /// <summary>
        /// Update File Name and File Path in File Header.
        /// </summary>
        /// <param name="fileHeaderID">File Header ID</param>
        /// <param name="fileName">File Name</param>
        /// <param name="filePath">File Path</param>
        /// <returns>True/False</returns>
        public bool UpdateDataAfterFileGenerated(int? fileHeaderID, string fileName, string filePath, string logFileBlobUrl, string updatedBy)
        {
            QidWorkerRole.SIS.DAL.FileHeader updatingFileHeader = _sisDB.FileHeaders.First(fh => fh.FileHeaderID == fileHeaderID);
            updatingFileHeader.FileName = fileName;
            updatingFileHeader.FilePath = filePath;
            updatingFileHeader.LogFilePath = logFileBlobUrl;
            updatingFileHeader.FileStatusID = 1; // File Status 1 is File Generated.
            updatingFileHeader.CreatedBy = updatedBy;
            updatingFileHeader.CreatedOn = DateTime.UtcNow;
            updatingFileHeader.LastUpdatedBy = "File Writer";

            foreach (var updatingInvoiceHeader in _sisDB.InvoiceHeaders.Where(ih => ih.FileHeaderID == fileHeaderID))
            {
                // Update FileGenerated as InvoiceStatusId.
                updatingInvoiceHeader.InvoiceStatusId = 2; // Invoice Status 2 is File Generated.
                updatingFileHeader.LastUpdatedBy = updatedBy;
                updatingFileHeader.LastUpdatedOn = DateTime.UtcNow;


                // update AWBStatusID as 5 (File Generated) in AirWayBill
                foreach (var updatingAirWayBill in _sisDB.AirWayBills.Where(awb => awb.InvoiceHeaderID == updatingInvoiceHeader.InvoiceHeaderID))
                {
                    updatingAirWayBill.AWBStatusID = 5; // AWB Status 5 is File Generated.
                    updatingFileHeader.LastUpdatedBy = updatedBy;
                    updatingFileHeader.LastUpdatedOn = DateTime.UtcNow;
                }

                foreach (var updatingRM in _sisDB.RejectionMemoes.Where(rm => rm.InvoiceHeaderID == updatingInvoiceHeader.InvoiceHeaderID))
                {
                    // update AWBStatusID as 5 (File Generated) in RMAirWayBill
                    foreach (var updatingRMAwb in _sisDB.RMAirWayBills.Where(rmawb => rmawb.RejectionMemoID == updatingRM.RejectionMemoID))
                    {
                        updatingRMAwb.AWBStatusID = 5; // AWB Status 5 is File Generated.
                        updatingFileHeader.LastUpdatedBy = updatedBy;
                        updatingFileHeader.LastUpdatedOn = DateTime.UtcNow;
                    }
                }

                foreach (var updatingBM in _sisDB.BillingMemoes.Where(bm => bm.InvoiceHeaderID == updatingInvoiceHeader.InvoiceHeaderID))
                {
                    // update AWBStatusID as 5 (File Generated) in BMAirWayBill
                    foreach (var updatingBMAwb in _sisDB.BMAirWayBills.Where(bmawb => bmawb.BillingMemoID == updatingBM.BillingMemoID))
                    {
                        updatingBMAwb.AWBStatusID = 5; // AWB Status 5 is File Generated.
                        updatingFileHeader.LastUpdatedBy = updatedBy;
                        updatingFileHeader.LastUpdatedOn = DateTime.UtcNow;
                    }
                }

                foreach (var updatingCM in _sisDB.CreditMemoes.Where(cm => cm.InvoiceHeaderID == updatingInvoiceHeader.InvoiceHeaderID))
                {
                    // update AWBStatusID as 5 (File Generated) in CMAirWayBill
                    foreach (var updatingCMAwb in _sisDB.CMAirWayBills.Where(cmawb => cmawb.CreditMemoID == updatingCM.CreditMemoID))
                    {
                        updatingCMAwb.AWBStatusID = 5; // AWB Status 5 is File Generated.
                        updatingFileHeader.LastUpdatedBy = updatedBy;
                        updatingFileHeader.LastUpdatedOn = DateTime.UtcNow;
                    }
                }
            }

            _sisDB.SaveChanges();

            return true;
        }


        public bool UpdateDataAfterNonSISFileUpload(int? fileHeaderID, string fileName, string filePath, string logFileBlobUrl, string updatedBy)
        {
            QidWorkerRole.SIS.DAL.FileHeader updatingFileHeader = _sisDB.FileHeaders.First(fh => fh.FileHeaderID == fileHeaderID);
            updatingFileHeader.FileName = fileName;
            updatingFileHeader.FilePath = filePath;
            updatingFileHeader.LogFilePath = logFileBlobUrl;
            updatingFileHeader.FileStatusID = 4; // Is Validated IsPayable=1
            updatingFileHeader.CreatedBy = updatedBy;
            updatingFileHeader.CreatedOn = DateTime.UtcNow;
            updatingFileHeader.LastUpdatedBy = updatedBy;

            foreach (var updatingInvoiceHeader in _sisDB.InvoiceHeaders.Where(ih => ih.FileHeaderID == fileHeaderID))
            {
                // Update Is Validated as InvoiceStatusId.
                updatingInvoiceHeader.InvoiceStatusId = 5; // Is Validated IsPayable=1
                updatingFileHeader.LastUpdatedBy = updatedBy;
                updatingFileHeader.LastUpdatedOn = DateTime.UtcNow;


                // update AWBStatusID as 5 (Is Validated) in AirWayBill
                foreach (var updatingAirWayBill in _sisDB.AirWayBills.Where(awb => awb.InvoiceHeaderID == updatingInvoiceHeader.InvoiceHeaderID))
                {
                    updatingAirWayBill.AWBStatusID = 8; // Is Validated IsPayable=1
                    updatingFileHeader.LastUpdatedBy = updatedBy;
                    updatingFileHeader.LastUpdatedOn = DateTime.UtcNow;
                }

                foreach (var updatingRM in _sisDB.RejectionMemoes.Where(rm => rm.InvoiceHeaderID == updatingInvoiceHeader.InvoiceHeaderID))
                {
                    // update AWBStatusID as 5 (Is Validated) in RMAirWayBill
                    foreach (var updatingRMAwb in _sisDB.RMAirWayBills.Where(rmawb => rmawb.RejectionMemoID == updatingRM.RejectionMemoID))
                    {
                        updatingRMAwb.AWBStatusID = 8; // Is Validated IsPayable=1
                        updatingFileHeader.LastUpdatedBy = updatedBy;
                        updatingFileHeader.LastUpdatedOn = DateTime.UtcNow;
                    }
                }

                foreach (var updatingBM in _sisDB.BillingMemoes.Where(bm => bm.InvoiceHeaderID == updatingInvoiceHeader.InvoiceHeaderID))
                {
                    // update AWBStatusID as 5 (Is Validated) in BMAirWayBill
                    foreach (var updatingBMAwb in _sisDB.BMAirWayBills.Where(bmawb => bmawb.BillingMemoID == updatingBM.BillingMemoID))
                    {
                        updatingBMAwb.AWBStatusID = 8; // Is Validated IsPayable=1
                        updatingFileHeader.LastUpdatedBy = updatedBy;
                        updatingFileHeader.LastUpdatedOn = DateTime.UtcNow;
                    }
                }

                foreach (var updatingCM in _sisDB.CreditMemoes.Where(cm => cm.InvoiceHeaderID == updatingInvoiceHeader.InvoiceHeaderID))
                {
                    // update AWBStatusID as 5 (Is Validated) in CMAirWayBill
                    foreach (var updatingCMAwb in _sisDB.CMAirWayBills.Where(cmawb => cmawb.CreditMemoID == updatingCM.CreditMemoID))
                    {
                        updatingCMAwb.AWBStatusID = 8; // Is Validated IsPayable=1
                        updatingFileHeader.LastUpdatedBy = updatedBy;
                        updatingFileHeader.LastUpdatedOn = DateTime.UtcNow;
                    }
                }
            }

            _sisDB.SaveChanges();

            return true;
        }

        /// <summary>
        /// Update Received File Path in File Header.
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <param name="fileBlobUrl">File Path</param>
        /// <returns>True/False</returns>
        public bool UpdateReceivedFileHeaderData(int newFileHeaderId, string fileBlobUrl, string logFileBlobUrl)
        {
            QidWorkerRole.SIS.DAL.FileHeader updatingFileHeader = _sisDB.FileHeaders.FirstOrDefault(fh => fh.FileHeaderID == newFileHeaderId);

            if (updatingFileHeader != null)
            {
                updatingFileHeader.LogFilePath = logFileBlobUrl;
                updatingFileHeader.IsProcessed = true;
                _sisDB.SaveChanges();

                return true;
            }
            return false;
        }

        #region Update Receivables File(s)/Invoice(s)/AWB(s) Status From File Generated(1) to File Uploaded(2)

        /// <summary>
        /// Update Status to File Uploaded
        /// </summary>
        /// <param name="fileHeaderID"> File Header ID</param>
        /// <param name="userName"> Updated By User Name </param>
        /// <returns></returns>
        public bool UpdateStatusToFileUploaded(int fileHeaderID, string userName)
        {
            using (var context = _sisDB)
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (fileHeaderID > 0)
                        {
                            #region File

                            DbEntity.FileHeader updatingFileHeader = _sisDB.FileHeaders.FirstOrDefault(fh => fh.FileHeaderID == fileHeaderID && fh.FileStatusID == 1);

                            if (updatingFileHeader != null)
                            {
                                #region Invoice

                                foreach (var updatingInvoiceHeader in _sisDB.InvoiceHeaders.Where(ih => ih.FileHeaderID == updatingFileHeader.FileHeaderID && ih.InvoiceStatusId == 2))
                                {
                                    #region AirWayBill

                                    foreach (var updatingAirWayBill in _sisDB.AirWayBills.Where(awb => awb.InvoiceHeaderID == updatingInvoiceHeader.InvoiceHeaderID && awb.AWBStatusID == 5))
                                    {
                                        updatingAirWayBill.AWBStatusID = 6; // File Uploaded.
                                        updatingAirWayBill.LastUpdatedBy = userName;
                                        updatingAirWayBill.LastUpdatedOn = DateTime.UtcNow;
                                        _sisDB.Entry(updatingAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                    }

                                    #endregion

                                    #region Rejection Memo

                                    foreach (var rejectionMemo in _sisDB.RejectionMemoes.Where(rm => rm.InvoiceHeaderID == updatingInvoiceHeader.InvoiceHeaderID))
                                    {
                                        #region RM AirWayBill

                                        foreach (var updatingRMAirWayBill in _sisDB.RMAirWayBills.Where(rmawb => rmawb.RejectionMemoID == rejectionMemo.RejectionMemoID && rmawb.AWBStatusID == 5))
                                        {
                                            updatingRMAirWayBill.AWBStatusID = 6; // File Uploaded.
                                            updatingRMAirWayBill.LastUpdatedBy = userName;
                                            updatingRMAirWayBill.LastUpdatedOn = DateTime.UtcNow;
                                            _sisDB.Entry(updatingRMAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                        }

                                        #endregion
                                    }

                                    #endregion

                                    #region Billing Memo

                                    foreach (var billingMemo in _sisDB.BillingMemoes.Where(bm => bm.InvoiceHeaderID == updatingInvoiceHeader.InvoiceHeaderID))
                                    {
                                        #region BM AirWayBill

                                        foreach (var updatingBMAirWayBill in _sisDB.BMAirWayBills.Where(bmawb => bmawb.BillingMemoID == billingMemo.BillingMemoID && bmawb.AWBStatusID == 5))
                                        {
                                            updatingBMAirWayBill.AWBStatusID = 6; // File Uploaded.
                                            updatingBMAirWayBill.LastUpdatedBy = userName;
                                            updatingBMAirWayBill.LastUpdatedOn = DateTime.UtcNow;
                                            _sisDB.Entry(updatingBMAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                        }

                                        #endregion
                                    }

                                    #endregion

                                    #region Credit Memo

                                    foreach (var creditMemo in _sisDB.CreditMemoes.Where(cm => cm.InvoiceHeaderID == updatingInvoiceHeader.InvoiceHeaderID))
                                    {
                                        #region CM AirWayBill

                                        foreach (var updatingCMAirWayBill in _sisDB.CMAirWayBills.Where(cmawb => cmawb.CreditMemoID == creditMemo.CreditMemoID && cmawb.AWBStatusID == 5))
                                        {
                                            updatingCMAirWayBill.AWBStatusID = 6; // File Uploaded.
                                            updatingCMAirWayBill.LastUpdatedBy = userName;
                                            updatingCMAirWayBill.LastUpdatedOn = DateTime.UtcNow;
                                            _sisDB.Entry(updatingCMAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                        }

                                        #endregion
                                    }

                                    #endregion

                                    updatingInvoiceHeader.InvoiceStatusId = 3; // File Uploaded.
                                    updatingInvoiceHeader.LastUpdatedBy = userName;
                                    updatingInvoiceHeader.LastUpdatedOn = DateTime.UtcNow;
                                    _sisDB.Entry(updatingInvoiceHeader).State = System.Data.Entity.EntityState.Modified;
                                }

                                #endregion

                                updatingFileHeader.FileStatusID = 2; // File Uploaded.
                                updatingFileHeader.LastUpdatedBy = userName;
                                updatingFileHeader.LastUpdatedOn = DateTime.UtcNow;
                                updatingFileHeader.ReadWriteOnSFTP = DateTime.UtcNow;
                                updatingFileHeader.IsProcessed = true;
                                _sisDB.Entry(updatingFileHeader).State = System.Data.Entity.EntityState.Modified;

                            }

                            _sisDB.SaveChanges();
                            dbContextTransaction.Commit();

                            SIS.SISBAL objSISBAL = new SIS.SISBAL();
                            string strMsgKey = string.Empty;
                            objSISBAL.CreateInterlineAuditLog("FileUploaded", fileHeaderID.ToString(), userName, DateTime.Now, strMsgKey);
                            return true;

                            #endregion
                        }
                        return false;
                    }
                    catch (Exception)
                    {
                        dbContextTransaction.Rollback();
                        return false;
                    }
                }
            }
        }

        #endregion

        #region Update IBilling.AirWayBill

        /// <summary>
        /// To Update AirWayBill in IBilling tables
        /// </summary>
        /// <param name="billingInterlineAirWayBill"> billingInterlineAirWayBill </param>
        /// <returns></returns>
        public bool UpdateIBillingAirWayBill(ModelClass.IBilling.AirWayBill billingInterlineAirWayBill)
        {
            using (var context = _sisDB)
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (billingInterlineAirWayBill.ID > 0)
                        {
                            DbEntity.BillingInterlineAirWayBill updatingIBillingAirWayBill = _sisDB.BillingInterlineAirWayBills.FirstOrDefault(iawb => iawb.ID == billingInterlineAirWayBill.ID);

                            if (updatingIBillingAirWayBill != null)
                            {
                                #region AWB Other Charges

                                foreach (var billingInterlineAWBOC in billingInterlineAirWayBill.AirWayBillOCList)
                                {
                                    DbEntity.BillingInterlineAWBOC updatingIBillingAirWayBillOC = _sisDB.BillingInterlineAWBOCs.FirstOrDefault(iawboc => iawboc.ID == billingInterlineAWBOC.ID);

                                    if (updatingIBillingAirWayBillOC != null)
                                    {
                                        updatingIBillingAirWayBillOC.OtherChargeCodeValue = billingInterlineAWBOC.OtherChargeCodeValue == null ? 0 : Convert.ToDecimal(billingInterlineAWBOC.OtherChargeCodeValue);
                                        updatingIBillingAirWayBillOC.VATBaseAmount = billingInterlineAWBOC.VATBaseAmount;
                                        updatingIBillingAirWayBillOC.VATPercentage = billingInterlineAWBOC.VATPercentage;
                                        updatingIBillingAirWayBillOC.VATCalculatedAmount = (((billingInterlineAWBOC.VATBaseAmount != null ? billingInterlineAWBOC.VATBaseAmount : 0)
                                                                                            *
                                                                                            (billingInterlineAWBOC.VATPercentage != null ? billingInterlineAWBOC.VATPercentage : 0))
                                                                                            / 100);
                                        updatingIBillingAirWayBillOC.LastUpdatedOn = billingInterlineAWBOC.LastUpdatedOn;
                                        updatingIBillingAirWayBillOC.LastUpdatedBy = billingInterlineAWBOC.LastUpdatedBy;

                                        _sisDB.Entry(updatingIBillingAirWayBillOC).State = System.Data.Entity.EntityState.Modified;
                                        _sisDB.SaveChanges();
                                    }
                                }

                                #endregion

                                #region VAT

                                foreach (var billingInterlineAWBVAT in billingInterlineAirWayBill.AirWayBillVATList)
                                {
                                    DbEntity.BillingInterlineAWBVAT updatingIBillingAWBVAT = _sisDB.BillingInterlineAWBVATs.FirstOrDefault(iawbvat => iawbvat.ID == billingInterlineAWBVAT.ID);

                                    if (updatingIBillingAWBVAT != null)
                                    {
                                        updatingIBillingAWBVAT.VATBaseAmount = billingInterlineAWBVAT.VATBaseAmount == null ? 0 : Convert.ToDecimal(billingInterlineAWBVAT.VATBaseAmount);
                                        updatingIBillingAWBVAT.VATPercentage = billingInterlineAWBVAT.VATPercentage == null ? 0 : Convert.ToDecimal(billingInterlineAWBVAT.VATPercentage);
                                        updatingIBillingAWBVAT.VATCalculatedAmount = (((billingInterlineAWBVAT.VATBaseAmount == null ? 0 : Convert.ToDecimal(billingInterlineAWBVAT.VATBaseAmount))
                                                                                        *
                                                                                        (billingInterlineAWBVAT.VATPercentage == null ? 0 : Convert.ToDecimal(billingInterlineAWBVAT.VATPercentage)))
                                                                                        / 100);
                                        updatingIBillingAWBVAT.LastUpdatedOn = billingInterlineAWBVAT.LastUpdatedOn;
                                        updatingIBillingAWBVAT.LastUpdatedBy = billingInterlineAWBVAT.LastUpdatedBy;

                                        _sisDB.Entry(updatingIBillingAWBVAT).State = System.Data.Entity.EntityState.Modified;
                                        _sisDB.SaveChanges();
                                    }
                                }

                                #endregion

                                #region AWB

                                updatingIBillingAirWayBill.WeightCharges = billingInterlineAirWayBill.WeightCharges == null ? 0 : Convert.ToDecimal(billingInterlineAirWayBill.WeightCharges);
                                updatingIBillingAirWayBill.ValuationCharges = billingInterlineAirWayBill.ValuationCharges;
                                updatingIBillingAirWayBill.OtherCharges = _sisDB.BillingInterlineAWBOCs.Where(awboc => awboc.BillingInterlineAirWayBillID == updatingIBillingAirWayBill.ID).Count() > 0
                                                                            ? _sisDB.BillingInterlineAWBOCs.Where(awboc => awboc.BillingInterlineAirWayBillID == updatingIBillingAirWayBill.ID).Sum(oc => oc.OtherChargeCodeValue)
                                                                            : 0;
                                //Changed By Kalyani on 31 Jan 2017.AmountSubjectToInterlineServiceCharge=(weight+val) charge as per validation file from SIS else nornal from textbox 
                                updatingIBillingAirWayBill.AmountSubjectToInterlineServiceCharge = billingInterlineAirWayBill.BillingCode.Equals("P") ? billingInterlineAirWayBill.WeightCharges + billingInterlineAirWayBill.ValuationCharges : billingInterlineAirWayBill.AmountSubjectToInterlineServiceCharge;
                                updatingIBillingAirWayBill.InterlineServiceChargePercentage = billingInterlineAirWayBill.InterlineServiceChargePercentage == null ? 0 : Convert.ToDecimal(billingInterlineAirWayBill.InterlineServiceChargePercentage);
                                updatingIBillingAirWayBill.InterlineServiceChargeAmount = (((billingInterlineAirWayBill.AmountSubjectToInterlineServiceCharge != null ? billingInterlineAirWayBill.AmountSubjectToInterlineServiceCharge : 0)
                                                                                                *
                                                                                            (billingInterlineAirWayBill.InterlineServiceChargePercentage != null ? billingInterlineAirWayBill.InterlineServiceChargePercentage : 0)
                                                                                           ) / 100);
                                //AWB VAT
                                decimal decVat = (_sisDB.BillingInterlineAWBVATs.Where(awbvat => awbvat.BillingInterlineAirWayBillID == updatingIBillingAirWayBill.ID).Count() > 0)
                                                                        ? Convert.ToDecimal(_sisDB.BillingInterlineAWBVATs.Where(awbvat => awbvat.BillingInterlineAirWayBillID == updatingIBillingAirWayBill.ID).Sum(vat => vat.VATCalculatedAmount))
                                                                        : 0;
                                //VAT on OCs
                                decimal decVatONOc = _sisDB.BillingInterlineAWBOCs.Where(awboc => awboc.BillingInterlineAirWayBillID == updatingIBillingAirWayBill.ID && awboc.VATLabel == "VAT").Count() > 0
                                                                        ? Convert.ToDecimal(_sisDB.BillingInterlineAWBOCs.Where(awboc => awboc.BillingInterlineAirWayBillID == updatingIBillingAirWayBill.ID && awboc.VATLabel == "VAT").Sum(ocvat => ocvat.VATCalculatedAmount))
                                                                        : 0;

                                updatingIBillingAirWayBill.VATAmount = decVat + decVatONOc;

                                _sisDB.Entry(updatingIBillingAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                _sisDB.SaveChanges();

                                // Update AWB Total
                                updatingIBillingAirWayBill.AWBTotalAmount = updatingIBillingAirWayBill.WeightCharges
                                                                            + updatingIBillingAirWayBill.ValuationCharges
                                                                            + updatingIBillingAirWayBill.OtherCharges
                                                                            + updatingIBillingAirWayBill.InterlineServiceChargeAmount
                                                                            + updatingIBillingAirWayBill.VATAmount;

                                _sisDB.Entry(updatingIBillingAirWayBill).State = System.Data.Entity.EntityState.Modified;
                                _sisDB.SaveChanges();

                                #endregion
                            }

                            dbContextTransaction.Commit();
                            return true;
                        }
                        return false;
                    }
                    catch (Exception)
                    {
                        dbContextTransaction.Rollback();
                        return false;
                    }
                }
            }
        }

        #endregion

        public bool UpdateSISFileProcessed(int fileHeaderID)
        {
            QidWorkerRole.SIS.DAL.FileHeader updatingFileHeader = _sisDB.FileHeaders.FirstOrDefault(fh => fh.FileHeaderID == fileHeaderID);

            if (updatingFileHeader != null)
            {
                updatingFileHeader.IsProcessed = true;
                _sisDB.SaveChanges();

                return true;
            }
            return false;
        }
        public bool UpdateSISReceivableFileProcessed(int fileHeaderID)
        {
            QidWorkerRole.SIS.DAL.FileHeader updatingFileHeader = _sisDB.FileHeaders.FirstOrDefault(fh => fh.FileHeaderID == fileHeaderID);

            if (updatingFileHeader != null)
            {
                updatingFileHeader.IsProcessed = true;
                updatingFileHeader.ReadWriteOnSFTP = DateTime.UtcNow;
                _sisDB.SaveChanges();

                return true;
            }
            return false;
        }
    }
}