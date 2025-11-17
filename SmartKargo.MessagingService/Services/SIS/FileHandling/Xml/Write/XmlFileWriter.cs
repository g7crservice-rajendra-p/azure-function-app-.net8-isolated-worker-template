using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

using QidWorkerRole.SIS.Model;
using System.IO;
using System.Text;
using QidWorkerRole.SIS.FileHandling.Xml.Write.WriteHelpers;
using Microsoft.Extensions.Logging;

namespace QidWorkerRole.SIS.FileHandling.Xml.Write
{
    /// <summary>
    /// Class to write Xml File.
    /// </summary>
    public class XmlFileWriter
    {
        private XmlTextWriter xmlTextWriter;
        private readonly ILogger<XmlFileWriter> _logger;
        private string FilePath { get; set; }

        /// <summary>
        /// To initialize Xml writer.
        /// </summary>
        /// <param name="filePath"></param>
        public void Init(string filePath)
    
        {
            //Logger.Debug("Initializing IS-XML writer engine.");
            if (File.Exists(filePath))
            {
                //Logger.Debug(string.Format("Deleting Xml file [{0}]", filePath));
                File.Delete(filePath);
            }
            xmlTextWriter = new XmlTextWriter(filePath, Encoding.ASCII);
            xmlTextWriter.Formatting = Formatting.Indented;
            FilePath = filePath;
        }

         public XmlFileWriter(ILogger<XmlFileWriter> logger)
            {
                _logger = logger;
            }

        /// <summary>
        /// To check whether XML writer is initialized or not.
        /// </summary>
        private void CheckXmlWriterIntialization()
        {
            if (xmlTextWriter == null)
            {
                throw new InvalidOperationException("XmlTextWriter not initialized.");
            }
        }

        /// <summary>
        /// To create Xml file.
        /// </summary>
        /// <param name="invoiceList"></param>
        public void WriteXMLFile(List<Invoice> invoiceList)
        {
            try
            {
                //Logger.Info("Start of WriteXMLFile.");

                CheckXmlWriterIntialization();

                if (invoiceList.Count != 0)
                {
                    xmlTextWriter.WriteStartDocument();

                    XmlWriterHelper.WriteTransmissionHeader(xmlTextWriter, invoiceList);

                    foreach (Invoice invoice in invoiceList.OrderBy(inv => inv.InvoiceHeaderID))
                    {
                        XmlWriterHelper.WriteInvoice(xmlTextWriter, invoice);
                    }

                    XmlWriterHelper.WriteTransmissionSummary(xmlTextWriter, invoiceList);

                    xmlTextWriter.WriteEndDocument();
                }

                xmlTextWriter.Close();

                //Logger.Info("End of WriteXMLFile.");
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure("Error Occurred in WriteXMLFile", exception);

                // clsLog.WriteLogAzure("File is Deleted due to exception", exception);
                _logger.LogError("File is Deleted due to exception {0}", exception);
                _logger.LogWarning("Error Occurred in WriteXMLFile");
                xmlTextWriter.Close();

                File.Delete(FilePath);

            }
        }
    }
}