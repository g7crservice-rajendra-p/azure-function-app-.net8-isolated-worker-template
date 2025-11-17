using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels;
using QidWorkerRole.SIS.Model;
using QidWorkerRole.SIS.Model.SupportingModels;
using System.Reflection;
using System.Xml;

namespace QidWorkerRole.SIS.FileHandling.Idec.Write
{
    public class IdecFileWriter
    {
        
        protected MultiRecordEngine multiRecordEngine;

        public IdecFileWriter()
        {
            // Initialize all the record types in the file.
            InitializeRecordTypes();

            // Setup the multi record engine.
            SetupMultiRecordEngine();
        }

        /// <summary>
        /// Collection of record types with SFI as key and Record Class name as value.
        /// </summary>
        private IDictionary<string, Type> _typeSelector;

        /// <summary>
        /// Store SFI of all the record's in file.
        /// </summary>
        protected void InitializeRecordTypes()
        {
            //Logger.Info("Called InitializerecordTypes");
            _typeSelector = new Dictionary<string, Type>();

            // Create new instance of record types.
            recordTypes = new List<Type>();

            var xmlDocumentIdecResourceType = new XmlDocument();

            try
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(IdecConstants.EmbeddedCargoIdecResourcTypeFileName))
                {
                    if (stream != null)
                    {
                        xmlDocumentIdecResourceType.Load(stream);
                        var idecResourceTypesNode = xmlDocumentIdecResourceType.SelectSingleNode(IdecConstants.IdecResourcTypeFileNameParentNode);

                        if (idecResourceTypesNode != null)
                        {
                            foreach (XmlNode resourceTypeNode in idecResourceTypesNode)
                            {
                                var selectSingleNode = resourceTypeNode.SelectSingleNode("@Name");
                                if (selectSingleNode != null)
                                {
                                    var attrName = selectSingleNode.Value;
                                    var singleNode = resourceTypeNode.SelectSingleNode("@Key");
                                    if (singleNode != null)
                                    {
                                        var attrKey = singleNode.Value;

                                        if (attrName == string.Empty || attrKey == string.Empty)
                                        {
                                            continue;
                                        }

                                        AddTypeSelector(attrKey, Type.GetType(attrName));
                                    }
                                    recordTypes.Add(Type.GetType(attrName));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure(exception.Message, exception);
            }
            finally
            {
                xmlDocumentIdecResourceType = null;
            }
        }

        protected List<Type> recordTypes;

        protected void SetupMultiRecordEngine()
        {
            multiRecordEngine = new MultiRecordEngine(recordTypes.ToArray())
            {
                ErrorManager = { ErrorMode = ErrorMode.SaveAndContinue }
            };
        }

        /// <summary>
        /// To Initialize MultiRecordEngine Write operation
        /// </summary>
        /// <param name="filePath">File Path</param>
        public void Init(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            multiRecordEngine.BeginWriteFile(filePath);
        }
        
        /// <summary>
        /// InvoiceHeaderRecord to Write in the file.
        /// </summary>
        protected InvoiceHeaderRecord invoiceHeaderRecord { get; set; }

        public long recordSequenceNumber = 1;

        /// <summary>
        /// This will store Billing Code Total No of records
        /// </summary>
        private int _totalNumberOfRecords = 0;

        /// <summary>
        /// This will store Invoice Total No of records
        /// </summary>
        private int _invTotalNoOfRecords = 0;        

        /// <summary>
        /// To write the File Data into flat file
        /// </summary>
        /// <param name="fileData">File Data</param>
        /// <param name="filePath">File Path</param>
        public void WriteIdecFile(FileData fileData, string filePath)
        {
            try
            {
                //Logger.Info("Start of WriteIdecFile.");

                Init(filePath);

                var fileTotalNoOfRecords = 0;

                //Write FileHeader Record
                var fileHeaderRecord = new FileHeaderRecord();
                fileTotalNoOfRecords++;
                fileHeaderRecord.ConvertClassToRecord(fileData.FileHeader, ref recordSequenceNumber);
                multiRecordEngine.WriteNext(fileHeaderRecord);

                // Write Invoice Records
                foreach (var invoiceHeaderRecord in GetInvoiceHeaderRecords(fileData.InvoiceList))
                {
                    multiRecordEngine.WriteNexts(GetInvoiceHeaderHierarchicalRecords(invoiceHeaderRecord));
                    fileTotalNoOfRecords += _invTotalNoOfRecords;
                }

                // Write FileTotal Record
                if (fileData.FileTotal != null)
                {
                    var fileTotalRecord = new FileTotalRecord();
                    fileTotalNoOfRecords++;
                    fileTotalRecord.ConvertClassToRecord(fileData.FileTotal, ref recordSequenceNumber);
                    multiRecordEngine.WriteNext(fileTotalRecord);
                }
                //Logger.Info("End of WriteIdecFile.");
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Error Occurred in WriteIdecFile", exception);
            }
            finally
            {
                multiRecordEngine.Close();
            }
        }

        /// <summary>
        /// To Get InvoiceHeaderRecords
        /// </summary>
        /// <param name="invoiceList">List of Invoice</param>
        /// <returns>List of InvoiceHeaderRecord</returns>
        protected IEnumerable<InvoiceHeaderRecord> GetInvoiceHeaderRecords(List<Invoice> invoiceList)
        {
            if (invoiceList != null)
            {
                foreach (Invoice invoice in invoiceList)
                {
                    if (invoice != null)
                    {
                        invoiceHeaderRecord = new InvoiceHeaderRecord();

                        (invoiceHeaderRecord as IClassToRecordConverter<Invoice>).ConvertClassToRecord(invoice, ref recordSequenceNumber);

                        yield return invoiceHeaderRecord;
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("invoiceList is null.");
            }
        }

        /// <summary>
        /// To get all child records of InvoiceHeaderRecord record
        /// </summary>
        /// <param name="invoiceHeaderRecord">InvoiceHeaderRecord</param>
        /// <returns>Hierarchical record collection</returns>
        public IEnumerable<object> GetInvoiceHeaderHierarchicalRecords(InvoiceHeaderRecord invoiceHeaderRecord)
        {
            //Logger.Info("Called GetInvoiceHeaderHierarchicalRecords");

            _invTotalNoOfRecords = 0;

            _invTotalNoOfRecords++;
            yield return invoiceHeaderRecord;

            foreach (var referenceData1Record in invoiceHeaderRecord.ReferenceData1RecordList)
            {
                _invTotalNoOfRecords++;
                yield return referenceData1Record;

                if (referenceData1Record.ReferenceData2Record != null)
                {
                    _invTotalNoOfRecords++;
                    yield return referenceData1Record.ReferenceData2Record;
                }
            }

            #region AWB record

            foreach (var aWBRecordList in invoiceHeaderRecord.AWBRecordList.GroupBy(awbRecord => new { awbRecord.BillingCode }))
            {
                _totalNumberOfRecords = 0;
                foreach (var aWBRecord in aWBRecordList)
                {
                    _totalNumberOfRecords++;
                    yield return aWBRecord;

                    if (aWBRecord.AWBVatBreakdownRecordList != null)
                    {
                        foreach (var aWBVatBreakdownRecord in aWBRecord.AWBVatBreakdownRecordList)
                        {
                            _totalNumberOfRecords++;
                            yield return aWBVatBreakdownRecord;
                        }
                    }

                    if (aWBRecord.AWBOCBreakdownRecordList != null)
                    {
                        foreach (var aWBOCBreakdownRecord in aWBRecord.AWBOCBreakdownRecordList)
                        {
                            _totalNumberOfRecords++;
                            yield return aWBOCBreakdownRecord;
                        }
                    }
                }

                var aWBBillingCodeSubTotalRecordList = invoiceHeaderRecord.BillingCodeSubTotalRecordList.Find(i => i.BillingCode == "P");
                if (aWBBillingCodeSubTotalRecordList != null)
                {
                    _totalNumberOfRecords++;
                    _totalNumberOfRecords += aWBBillingCodeSubTotalRecordList.BillingCodeSubTotalVatRecordList.Count;
                    aWBBillingCodeSubTotalRecordList.TotalNumberOfRecords = _totalNumberOfRecords.ToString();

                    yield return aWBBillingCodeSubTotalRecordList;

                    if (aWBBillingCodeSubTotalRecordList.BillingCodeSubTotalVatRecordList != null)
                    {
                        foreach (var aWBBillingCodeSubTotalVatRecord in aWBBillingCodeSubTotalRecordList.BillingCodeSubTotalVatRecordList)
                        {
                            yield return aWBBillingCodeSubTotalVatRecord;
                        }
                    }
                }
            }

            #endregion

            #region Rejection Memo

            foreach (var rejectionMemoRecordList in invoiceHeaderRecord.RejectionMemoRecordList.GroupBy(rmRecord => new { rmRecord.BillingCode }))
            {
                _totalNumberOfRecords = 0;
                foreach (var rejectionMemoRecord in rejectionMemoRecordList)
                {
                    _totalNumberOfRecords++;
                    yield return rejectionMemoRecord;

                    if (rejectionMemoRecord.ReasonBreakdownRecordList != null)
                    {
                        foreach (var rMReasonBreakdownRecord in rejectionMemoRecord.ReasonBreakdownRecordList)
                        {
                            _totalNumberOfRecords++;
                            yield return rMReasonBreakdownRecord;
                        }
                    }

                    if (rejectionMemoRecord.RMVatRecordList != null)
                    {
                        foreach (var rMVatRecord in rejectionMemoRecord.RMVatRecordList)
                        {
                            _totalNumberOfRecords++;
                            yield return rMVatRecord;
                        }
                    }

                    if (rejectionMemoRecord.RMAWBRecordList != null)
                    {
                        foreach (var rMAWBRecord in rejectionMemoRecord.RMAWBRecordList)
                        {
                            _totalNumberOfRecords++;
                            yield return rMAWBRecord;

                            if (rMAWBRecord.RMAwbProrateLadderRecordList != null)
                            {
                                foreach (var rMAwbProrateLadderRecord in rMAWBRecord.RMAwbProrateLadderRecordList)
                                {
                                    _totalNumberOfRecords++;
                                    yield return rMAwbProrateLadderRecord;
                                }
                            }

                            if (rMAWBRecord.RMAWBVatRecordList != null)
                            {
                                foreach (var rMAWBVatRecord in rMAWBRecord.RMAWBVatRecordList)
                                {
                                    _totalNumberOfRecords++;
                                    yield return rMAWBVatRecord;
                                }
                            }

                            if (rMAWBRecord.RMAWBOCBreakdownRecordList != null)
                            {
                                foreach (var rMAWBOCBreakdownRecord in rMAWBRecord.RMAWBOCBreakdownRecordList)
                                {
                                    _totalNumberOfRecords++;
                                    yield return rMAWBOCBreakdownRecord;
                                }
                            }

                        }
                    }
                }

                var rMBillingCodeSubTotalRecordList = invoiceHeaderRecord.BillingCodeSubTotalRecordList.Find(i => i.BillingCode == "R");
                if (rMBillingCodeSubTotalRecordList != null)
                {
                    _totalNumberOfRecords++; //for RM Source Code Total record
                    _totalNumberOfRecords += rMBillingCodeSubTotalRecordList.BillingCodeSubTotalVatRecordList.Count;
                    rMBillingCodeSubTotalRecordList.TotalNumberOfRecords = _totalNumberOfRecords.ToString();

                    yield return rMBillingCodeSubTotalRecordList;

                    if (rMBillingCodeSubTotalRecordList.BillingCodeSubTotalVatRecordList != null)
                    {
                        foreach (var rMbillingCodeSubTotalVatRecord in rMBillingCodeSubTotalRecordList.BillingCodeSubTotalVatRecordList)
                        {
                            yield return rMbillingCodeSubTotalVatRecord;
                        }
                    }
                }
            }

            #endregion

            #region Billing Memo

            foreach (var billingMemoRecordList in invoiceHeaderRecord.BillingMemoRecordList.GroupBy(bmRecord => new { bmRecord.BillingCode }))
            {
                _totalNumberOfRecords = 0;
                foreach (var billingMemoRecord in billingMemoRecordList)
                {
                    _totalNumberOfRecords++;
                    yield return billingMemoRecord;

                    if (billingMemoRecord.ReasonBreakdownRecordList != null)
                    {
                        foreach (var bMReasonBreakdownRecord in billingMemoRecord.ReasonBreakdownRecordList)
                        {
                            _totalNumberOfRecords++;
                            yield return bMReasonBreakdownRecord;
                        }
                    }

                    if (billingMemoRecord.BMVatRecordList != null)
                    {
                        foreach (var bMVatRecord in billingMemoRecord.BMVatRecordList)
                        {
                            _totalNumberOfRecords++;
                            yield return bMVatRecord;
                        }
                    }

                    if (billingMemoRecord.BMAWBRecordList != null)
                    {
                        foreach (var bMAWBRecord in billingMemoRecord.BMAWBRecordList)
                        {
                            _totalNumberOfRecords++;
                            yield return bMAWBRecord;

                            if (bMAWBRecord.BMAwbProrateLadderRecordList != null)
                            {
                                foreach (var bMAwbProrateLadderRecord in bMAWBRecord.BMAwbProrateLadderRecordList)
                                {
                                    _totalNumberOfRecords++;
                                    yield return bMAwbProrateLadderRecord;
                                }
                            }

                            if (bMAWBRecord.BMAWBVatRecordList != null)
                            {
                                foreach (var bMAWBVatRecord in bMAWBRecord.BMAWBVatRecordList)
                                {
                                    _totalNumberOfRecords++;
                                    yield return bMAWBVatRecord;
                                }
                            }

                            if (bMAWBRecord.BMAWBOCBreakdownRecordList != null)
                            {
                                foreach (var bMAWBOCBreakdownRecord in bMAWBRecord.BMAWBOCBreakdownRecordList)
                                {
                                    _totalNumberOfRecords++;
                                    yield return bMAWBOCBreakdownRecord;
                                }
                            }
                        }
                    }
                }

                var bMBillingCodeSubTotalRecordList = invoiceHeaderRecord.BillingCodeSubTotalRecordList.Find(i => i.BillingCode == "B");
                if (bMBillingCodeSubTotalRecordList != null)
                {
                    _totalNumberOfRecords++; //for BM Source Code Total record
                    _totalNumberOfRecords += bMBillingCodeSubTotalRecordList.BillingCodeSubTotalVatRecordList.Count; // BM Source code vat records
                    bMBillingCodeSubTotalRecordList.TotalNumberOfRecords = _totalNumberOfRecords.ToString();

                    yield return bMBillingCodeSubTotalRecordList;

                    if (bMBillingCodeSubTotalRecordList.BillingCodeSubTotalVatRecordList != null)
                    {
                        foreach (var bMBillingCodeSubTotalVatRecord in bMBillingCodeSubTotalRecordList.BillingCodeSubTotalVatRecordList)
                        {
                            yield return bMBillingCodeSubTotalVatRecord;
                        }
                    }
                }
            }

            #endregion

            #region Credit Memo record

            foreach (var creditMemoRecordList in invoiceHeaderRecord.CreditMemoRecordList.GroupBy(cmRecord => new { cmRecord.BillingCode }))
            {
                _totalNumberOfRecords = 0;
                foreach (var creditMemoRecord in creditMemoRecordList)
                {
                    _totalNumberOfRecords++;
                    yield return creditMemoRecord;

                    if (creditMemoRecord.ReasonBreakdownRecordList != null)
                    {
                        foreach (var reasonBreakdownRecord in creditMemoRecord.ReasonBreakdownRecordList)
                        {
                            _totalNumberOfRecords++;
                            yield return reasonBreakdownRecord;
                        }
                    }

                    if (creditMemoRecord.CMVatRecordList != null)
                    {
                        foreach (var cMVatRecord in creditMemoRecord.CMVatRecordList)
                        {
                            _totalNumberOfRecords++;
                            yield return cMVatRecord;
                        }
                    }

                    if (creditMemoRecord.CMAWBRecordList != null)
                    {
                        foreach (var cMAWBRecord in creditMemoRecord.CMAWBRecordList)
                        {
                            _totalNumberOfRecords++;
                            yield return cMAWBRecord;

                            if (cMAWBRecord.CMAwbProrateLadderRecordList != null)
                            {
                                foreach (var cMAwbProrateLadderRecord in cMAWBRecord.CMAwbProrateLadderRecordList)
                                {
                                    _totalNumberOfRecords++;
                                    yield return cMAwbProrateLadderRecord;
                                }
                            }

                            if (cMAWBRecord.CMAWBVatRecordList != null)
                            {
                                foreach (var cMAWBVatRecord in cMAWBRecord.CMAWBVatRecordList)
                                {
                                    _totalNumberOfRecords++;
                                    yield return cMAWBVatRecord;
                                }
                            }

                            if (cMAWBRecord.CMAWBOCBreakdownRecordList != null)
                            {
                                foreach (var cMAWBOCBreakdownRecord in cMAWBRecord.CMAWBOCBreakdownRecordList)
                                {
                                    _totalNumberOfRecords++;
                                    yield return cMAWBOCBreakdownRecord;
                                }
                            }
                        }
                    }
                }

                var cMBillingCodeSubTotalRecordList = invoiceHeaderRecord.BillingCodeSubTotalRecordList.Find(i => i.BillingCode == creditMemoRecordList.Key.BillingCode);
                if (cMBillingCodeSubTotalRecordList != null)
                {
                    _totalNumberOfRecords++; //for CM Source Code Total record
                    _totalNumberOfRecords += cMBillingCodeSubTotalRecordList.BillingCodeSubTotalVatRecordList.Count; // CM Source code vat records
                    cMBillingCodeSubTotalRecordList.TotalNumberOfRecords = _totalNumberOfRecords.ToString();

                    yield return cMBillingCodeSubTotalRecordList;

                    if (cMBillingCodeSubTotalRecordList.BillingCodeSubTotalVatRecordList != null)
                    {
                        foreach (var cMBillingCodeSubTotalVatRecord in cMBillingCodeSubTotalRecordList.BillingCodeSubTotalVatRecordList)
                        {
                            yield return cMBillingCodeSubTotalVatRecord;
                        }
                    }
                }
            }

            #endregion 

            _invTotalNoOfRecords += invoiceHeaderRecord.InvoiceFooterRecordList != null ? invoiceHeaderRecord.InvoiceFooterRecordList.Count : 0;
            _invTotalNoOfRecords += invoiceHeaderRecord.InvoiceVatRecordList != null ? invoiceHeaderRecord.InvoiceVatRecordList.Count : 0;
            _invTotalNoOfRecords += invoiceHeaderRecord.BillingCodeSubTotalRecordList != null ? invoiceHeaderRecord.BillingCodeSubTotalRecordList.Sum(billingCodeTotalRecord => Convert.ToInt32(billingCodeTotalRecord.TotalNumberOfRecords)) : 0;

            if (invoiceHeaderRecord.InvoiceTotalRecord != null)
            {
                // InvoiceTotal record.
                _invTotalNoOfRecords++;
                invoiceHeaderRecord.InvoiceTotalRecord.TotalNumberOfRecords = _invTotalNoOfRecords.ToString();
                yield return invoiceHeaderRecord.InvoiceTotalRecord;
            }

            // InvoiceTotal Vat record.
            if (invoiceHeaderRecord.InvoiceVatRecordList != null)
            {
                foreach (var invoiceVatRecord in invoiceHeaderRecord.InvoiceVatRecordList)
                {
                    yield return invoiceVatRecord;
                }
            }

            // Invoice Footer record
            if (invoiceHeaderRecord.InvoiceFooterRecordList != null)
            {
                foreach (var invoiceFooterRecord in invoiceHeaderRecord.InvoiceFooterRecordList)
                {
                    yield return invoiceFooterRecord;
                }
            }
        }

        /// <summary>
        /// Adds record type and its SFI.
        /// </summary>
        /// <param name="key">Standard field identifier.</param>
        /// <param name="type">Record type</param>
        private void AddTypeSelector(string key, Type type)
        {
            if (_typeSelector != null && !_typeSelector.ContainsKey(key) && type != null)
            {
                _typeSelector.Add(key, type);
            }
        }
        
    }
}