using FileHelpers;
using Microsoft.Extensions.Logging;
using QidWorkerRole.SIS.Model;
using System.Reflection;
using System.Xml;

namespace QidWorkerRole.SIS.FileHandling.Idec.Read
{
    public class IdecFileReader
    {
        private readonly ILogger<IdecFileReader> _logger;
        public IdecFileReader(ILogger<IdecFileReader> logger)
        {
            _logger = logger;
            // Initialize all the record types in the IDEC file.
            InitializeRecordTypes();

            // Initialize MultiRecordEngine.
            SetupMultiRecordEngine();
        }

        /// <summary>
        /// Collection of record types with SFI as key and Record Class name as value.
        /// </summary>
        private IDictionary<string, Type> _typeSelector;

        /// <summary>
        /// List of Record Types to Initialize MultiRecordEngine.
        /// </summary>
        protected List<Type> RecordTypeList;

        /// <summary>
        /// Initialize all the record types in the IDEC file.
        /// </summary>
        protected void InitializeRecordTypes()
        {
            //Logger.Info("Start of IDEC Record types Initialization.");

            _typeSelector = new Dictionary<string, Type>();

            // Create new instance of record types.
            RecordTypeList = new List<Type>();

            var attrKey = string.Empty;
            var attrName = string.Empty;
            var xmlDocumentIdecResourceType = new XmlDocument();

            try
            {
                Stream stream;
                using (stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(IdecConstants.EmbeddedCargoIdecResourcTypeFileName))
                {
                    if (stream != null)
                    {
                        xmlDocumentIdecResourceType.Load(stream);
                        var idecResourceTypesNode = xmlDocumentIdecResourceType.SelectSingleNode(IdecConstants.IdecResourcTypeFileNameParentNode);

                        if (idecResourceTypesNode != null)
                        {
                            foreach (XmlNode xmlNode in idecResourceTypesNode)
                            {
                                var selectSingleNode = xmlNode.SelectSingleNode("@Name");
                                if (selectSingleNode != null)
                                    attrName = selectSingleNode.Value;
                                var singleNode = xmlNode.SelectSingleNode("@Key");
                                if (singleNode != null)
                                    attrKey = singleNode.Value;

                                if (attrName == string.Empty || attrKey == string.Empty)
                                {
                                    continue;
                                }

                                AddTypeSelector(attrKey, Type.GetType(attrName));
                                RecordTypeList.Add(Type.GetType(attrName));
                            }
                        }
                    }
                }
                //Logger.Info("End of IDEC Record types Initialization.");
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(exception);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
            finally
            {
                xmlDocumentIdecResourceType = null;
            }
        }

        /// <summary>
        /// Adds record type and corresponding SFI.
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

        /// <summary>
        /// MultiRecordEngine to Read different types of records.
        /// </summary>
        protected MultiRecordEngine multiRecordEngine;

        /// <summary>
        /// This function returns the appropriate record type based on SFI. 
        /// </summary>
        /// <param name="multiRecordEngine">The instance of the multi record engine.</param>
        /// <param name="recordString">The contents of the record as a string.</param>
        /// <returns>The type of the record.</returns>
        protected Type DoSelectRecordType(MultiRecordEngine multiRecordEngine, string recordString)
        {
            try
            {
                // Standard field identifier
                if (!String.IsNullOrEmpty(recordString.Trim()) && recordString.Length == IdecConstants.RecordLength)
                {
                    if (recordString.Substring(IdecConstants.SfiStartIndex, 2).Equals(IdecConstants.SfiReferenceData1))
                    {
                        _referenceDataRecord1Count += 1;
                        return (GetTypeSelector(recordString.Substring(IdecConstants.ReferenceDataRecordSerialNumberIndex, 1)));
                    }

                    if (recordString.Substring(IdecConstants.SfiStartIndex, 2).Equals(IdecConstants.SfiReferenceData2))
                    {
                        _referenceDataRecord2Count += 1;
                        return (GetTypeSelector(recordString.Substring(IdecConstants.ReferenceDataRecordSerialNumberIndex, 1)));
                    }

                    _referenceDataRecord1Count = 0;
                    _referenceDataRecord2Count = 0;

                    return (GetTypeSelector(recordString.Substring(IdecConstants.SfiStartIndex, 2)));
                }
                else
                {
                    // clsLog.WriteLogAzure("Record length is not of 500 characters.");
                    _logger.LogWarning("Record length is not of 500 characters.");
                    return null;
                }
            }
            catch (Exception exception)
            {
                // Invalid record sequence number.
                // clsLog.WriteLogAzure(exception);
                _logger.LogError(exception, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
        }

        private string _parentMemoRecord;

        /// <summary>
        /// Store SFI of all the record's for which Vat/Tax can be attached.
        /// </summary>
        private string _parentVATTAX;

        /// <summary>
        /// To get Record type using SFI as key. 
        /// </summary>
        /// <param name="key">Standard Field Identifier.</param>
        /// <returns>Record Type.</returns>
        private Type GetTypeSelector(string key)
        {
            switch (key)
            {
                // Keep Parent Billing Memo Record & Credit Memo Record SFI to decide the next AWB Records type as either Billing or Credit.
                case IdecConstants.SfiBillingMemoRecord:
                case IdecConstants.SfiCreditMemoRecord:
                    _parentMemoRecord = key;
                    _parentVATTAX = key;
                    break;

                // Append Parent Billing/Credit Memo Record SFI to SFI of BMAWB/CMAWB to get either BMAWB or CMAWB type.
                case IdecConstants.SfiCMAwbRecord:
                    // Save following parent record SFI to identify corresponding Tax or Vat records
                    key = _parentMemoRecord + key;
                    _parentVATTAX = key;
                    break;

                // Store following parent record SFI to identify corresponding Tax or Vat records.
                case IdecConstants.SfiAwbRecord:
                case IdecConstants.SfiRMAwbRecord:
                case IdecConstants.SfiRejectionMemoRecord:
                case IdecConstants.SfiBillingCodeSubTotal:
                case IdecConstants.SfiInvoiceTotalRecord:
                    _parentVATTAX = key;
                    break;

                // Append parent Billing/Credit Memo Record SFI to SFI of BMAWBVAT/CMAWBVAT to get either BMAWB or CMAWB type.
                case IdecConstants.SfiVatRecord:
                case IdecConstants.SfiOCBreakdownRecord:
                case IdecConstants.SfiProrateLadderRecord:
                    key = _parentVATTAX + key;
                    break;
            }

            if (_typeSelector.ContainsKey(key))
            {
                return _typeSelector[key];
            }

            return null;
        }

        /// <summary>
        /// Initialize MultiRecordEngine.
        /// </summary>
        protected void SetupMultiRecordEngine()
        {
            //Logger.Info("Start of MultiRecordEngine Initialization.");

            multiRecordEngine = new MultiRecordEngine(DoSelectRecordType, RecordTypeList.ToArray()) { ErrorManager = { ErrorMode = ErrorMode.SaveAndContinue } };

            //Logger.Info("End of MultiRecordEngine Initialization.");
        }

        /// <summary>
        /// Instance of the Invoice Model for intermediate reading while reading file.
        /// </summary>
        protected Invoice ModelInstance { get; set; }

        private int _referenceDataRecord1Count;
        private int _referenceDataRecord2Count;

        /// <summary>
        /// To Read File and returns the List of Invoices.
        /// </summary>
        /// <param name="filePath">File Path of Reading file.</param>
        /// <returns>List of Invoices.</returns>
        public IEnumerable<Invoice> Read(string filePath)
        {
            // Initialize file name property in all the log messages.
            //ThreadContext.Properties["FilePath"] = filePath;

            if (!File.Exists(filePath))
            {
                // clsLog.WriteLogAzure(string.Format("File [{0}] does not exist." + filePath));
                _logger.LogWarning("File [{FilePath}] does not exist.", filePath);

                throw new FileNotFoundException(string.Format("File [{0}] not found.", filePath));
            }

            // Create Instance of Invoice Class.
            ModelInstance = new Invoice();

            return DoRead(filePath, true);
        }

        /// <summary>
        /// Reads the record from IDEC File or Record String using MultiRecordEngine of FileHelpers & Returns a List of Invoice Class. 
        /// </summary>
        /// <param name="data">Data to be read, File of string.</param>
        /// <param name="isFilePath">True for File Path, False for String.</param>
        /// <returns>List of Invoices.</returns>
        private IEnumerable<Invoice> DoRead(string data, bool isFilePath)
        {
            try
            {
                // Check whether it is safe to proceed.
                if ((multiRecordEngine == null) || ((multiRecordEngine != null) && (multiRecordEngine.RecordSelector == null)))
                {
                    throw new InvalidOperationException("MultiRecordEngine is not initialized.");
                }
    
                if (isFilePath)
                {
                    // Begin reading the file.
                    multiRecordEngine.BeginReadFile(data);
                }
                else
                {
                    multiRecordEngine.BeginReadString(data);
                }
    
                // Read each record - till the end of the file.
                if (multiRecordEngine.ReadNext() != null)
                {
                    //if (Logger.IsInfoEnabled)
                    //{
                    //    Logger.DebugFormat(string.Format("Record of type [{0}] found.", multiRecordEngine.LastRecord.GetType().Name));
                    //}
    
                    // Read the record hierarchy and convert it into a class.
                    return ReadRecordHierarchy(multiRecordEngine);
                }
    
                return null;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        /// <summary>
        /// To Convert hirachical records into there corresponding classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <returns>List of Invoice</returns>
        protected IEnumerable<Invoice> ReadRecordHierarchy(MultiRecordEngine multiRecordEngine)
        {
            //Logger.Info("Start of ReadRecordHierarchy.");

            do
            {
                // Type cast record to model converter of class type.
                var record = multiRecordEngine.LastRecord as IRecordToClassConverter<Invoice>;

                // Ignore next record till valid record found.
                if (record == null)
                {
                    while (multiRecordEngine.ReadNext() != null)
                    {
                        // Ignore next record till valid record found.
                        if (!(multiRecordEngine.LastRecord is IRecordToClassConverter<Invoice>))
                        { continue; }

                        record = multiRecordEngine.LastRecord as IRecordToClassConverter<Invoice>;
                        break;
                    }
                }

                if (record != null)
                {
                    // Create Class for valid record.
                    ModelInstance = new Invoice();

                    // Convert record to Class.
                    ModelInstance = record.ConvertRecordToClass(multiRecordEngine);
                }
                else
                {
                    // set class as null for invalid record.
                    ModelInstance = null;
                }

                yield return ModelInstance;

            } while (multiRecordEngine.LastRecord != null);

            //Logger.Info("End of ReadRecordHierarchy.");
        }

        /// <summary>
        /// To Read the record content string and return a List of the Classes.
        /// </summary>
        /// <param name="content">Record string to be parsed.</param>
        /// <returns>List of Invoices.</returns>
        public IEnumerable<Invoice> ReadString(string content)
        {
            // Create new Invoice Class instance.
            ModelInstance = new Invoice();

            return DoRead(content, isFilePath: false);
        }
    }
}