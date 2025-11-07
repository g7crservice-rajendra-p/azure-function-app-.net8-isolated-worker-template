using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Reflection;
using System;
using QidWorkerRole;

namespace QidWorkerRole
{
    class XMLValidator
    {
        private string errormsg = string.Empty;
        public string CTeXMLValidator(string xml, string MessageType)
        {
            try
            {

                //_isValid = true;
                errormsg = string.Empty;
                // Set the validation settings.
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
                //local Debug
                //settings.Schemas.Add(null, @"D:\Project\ERP_DEV_V5\BAL\XMLSchema\CXML-XFWB-3\Waybill Schema\Waybill_1.xsd");
                //for deployment
                //settings.Schemas.Add(null, "~\\XMLSchema\\CXML-XFWB-3\\Waybill Schema\\Waybill_1.xsd");
                //string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + "\\XMLSchema\\CXML-XFWB-3\\Waybill Schema\\Waybill_1.xsd";
                //string applicationBasePath = AppDomain.CurrentDomain.BaseDirectory;
                //string basePath = applicationBasePath + @"BAL\XMLSchema\CXML-XFWB-3\Waybill Schema\Waybill_1.xsd";
                //string strpa = Server.MapPath("~/Reports/SpiceJetPrintLabel.html");

                if (MessageType == "XFWB")//XFWB
                {
                    settings.Schemas.Add(null, @"C:\MessagingService\CXML\CXML-XFWB-3\Waybill Schema\Waybill_1.xsd");
                    //debug//
                    //settings.Schemas.Add(null, @"D:\Yoginath Data\yogi\D\Documents\CXML Message\CXML-XFWB-3\Waybill Schema\Waybill_1.xsd"); 
                }
                else if (MessageType == "XFFR")//XFFR
                {
                    settings.Schemas.Add(null, @"C:\MessagingService\CXML\CXML-XFFR-2\Booking Request\BookingRequest_1.xsd");
                    //debug//settings.Schemas.Add(null, @"D:\Yoginath Data\yogi\D\Documents\CXML Message\CXML-XFFR-2\Booking Request\BookingRequest_1.xsd");
                }
                //clsLog.WriteLogAzure("In CTeXMLValidator() 1");
                //settings.Schemas.Add(null,@"XMLSchema\cte_v3.00.xsd");

                // Create the XmlReader object.
                XmlReader reader = XmlReader.Create(new StringReader(xml), settings);

                // Parse the file. 
                while (reader.Read())
                {
                    if (errormsg.Length > 0)
                    {
                        errormsg = errormsg.Replace("http://www.portalfiscal.inf.br/cte:", "");
                        break;
                    }
                };

                //clsLog.WriteLogAzure("In CTeXMLValidator() error 1 " + errormsg);
            }
            catch (System.Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            return errormsg;
        }

        // Display any warnings or errors.
        private void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Error)
            {
                if (!args.Message.Contains("'Signature'"))
                    errormsg = args.Message;
                return;
            }
        }
    }
}
