using QidWorkerRole.SIS.FileHandling.Xml.Read.ReadHelpers;
using QidWorkerRole.SIS.FileHandling.Xml.Read.SupportingModels;
using QidWorkerRole.SIS.Model;
using System.Xml;

namespace QidWorkerRole.SIS.FileHandling.Xml.Read
{
    public class XmlFileReader
    {

        private XmlTextReader xmlTextReader;
        private string IssuingOrganizationId { get; set; }

        protected string HeaderModelName { get; set; }
        protected string DetailModelName { get; set; }
        protected string SmmaryModelName { get; set; }

        /// <summary>
        /// Parameterized Constructor to Initialize Reader.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public XmlFileReader(string filePath)
        {
            HeaderModelName = XmlConstants.TransmissionHeader;
            DetailModelName = XmlConstants.Invoice;
            SmmaryModelName = XmlConstants.TransmissionSummary;

            if (!File.Exists(filePath))
            {
                clsLog.WriteLogAzure(string.Format("File [{0}] does not exist." + filePath));
                throw new FileNotFoundException(string.Format("File [{0}] not found.", filePath));
            }

            xmlTextReader = new XmlTextReader(filePath);
        }

        /// <summary>
        /// To Check XmlTextReader is Intialized or Not.
        /// </summary>
        private void CheckXmlReaderIntialization()
        {
            if (xmlTextReader == null)
            {
                throw new InvalidOperationException("XmlTextReader not initialized.");
            }
        }

        /// <summary>
        /// To read the TransmissionHeader in the XML File.
        /// </summary>
        /// <returns></returns>
        public TransmissionHeader ReadTransmissionHeader()
        {
            TransmissionHeader transmissionHeader = new TransmissionHeader();

            try
            {
                //Logger.Info("Start of ReadTransmissionHeader.");

                CheckXmlReaderIntialization();


                while (xmlTextReader.Read())
                {
                    if (xmlTextReader.NodeType == XmlNodeType.Element)
                    {
                        if (HeaderModelName.Equals(xmlTextReader.LocalName))
                        {
                            transmissionHeader = ReadTransmissionHelper.ReadTransmissionHeader(xmlTextReader, transmissionHeader);

                            if (!string.IsNullOrEmpty(transmissionHeader.IssuingOrganizationId))
                            {
                                IssuingOrganizationId = transmissionHeader.IssuingOrganizationId;
                            }

                            break;
                        }
                    }
                }

                //Logger.Info("End of ReadTransmissionHeader.");
                return transmissionHeader;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Error Occurred in ReadTransmissionHeader", exception);
                return transmissionHeader;
            }
        }

        public IEnumerable<Invoice> ReadInvoice()
        {
            //Logger.Info("Start of ReadInvoice.");

            CheckXmlReaderIntialization();

            while (xmlTextReader.Read())
            {
                if (xmlTextReader.NodeType == XmlNodeType.Element)
                {
                    if (IsSummaryStarted(xmlTextReader, SmmaryModelName))
                    {
                        break;
                    }
                    if (DetailModelName.Equals(xmlTextReader.LocalName))
                    {
                        Invoice invoice = new Invoice();
                        invoice.CreatedBy = "XML File Reader";

                        // var fileRecordSequenceNumber = new Dictionary<int, Dictionary<int, int>>();
                        XmlReaderHelper.ReadInvoice(xmlTextReader, invoice);//, out fileRecordSequenceNumber);

                        yield return invoice;
                    }
                }
            }
            //Logger.Info("End of ReadInvoice.");
        }

        private static bool IsSummaryStarted(XmlTextReader xmlTextReader, string readerCondition)
        {
            bool isContinue = false;

            if (xmlTextReader.LocalName == "" || (xmlTextReader.NodeType == XmlNodeType.EndElement && xmlTextReader.LocalName != readerCondition))
            {
                isContinue = true;
            }
            else if (xmlTextReader.LocalName == readerCondition)
            {
                isContinue = true;
            }

            return isContinue;
        }

        /// <summary>
        /// To read the TransmissionSummary in the XML File.
        /// </summary>
        /// <returns></returns>
        public TransmissionSummary ReadTransmissionSummary()
        {
            TransmissionSummary transmissionSummary = new TransmissionSummary();

            try
            {
                //Logger.Info("Start of ReadTransmissionSummary.");

                CheckXmlReaderIntialization();


                if (xmlTextReader.NodeType == XmlNodeType.Element)
                {
                    if (SmmaryModelName.Equals(xmlTextReader.LocalName))
                    {
                        ReadTransmissionHelper.ReadTransmissionSummary(xmlTextReader, transmissionSummary);
                    }
                }
                return transmissionSummary;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Error Occurred in ReadTransmissionSummary", exception);
                return transmissionSummary;
            }
            finally
            {
                if (xmlTextReader != null)
                {
                    xmlTextReader.Close();
                }
                //Logger.Info("End of ReadTransmissionSummary.");
            }
        }

    }
}