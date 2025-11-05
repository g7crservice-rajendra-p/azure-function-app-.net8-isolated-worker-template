using System;
using System.Xml;
using QidWorkerRole.SIS.Model;

namespace QidWorkerRole.SIS.FileHandling.Xml.Read.ReadHelpers
{
    public sealed partial class XmlReaderHelper
    {
        /// <summary>
        /// Read Attachment Details for AirWayBill, BM, BMAWB, CM, CMAWB, RM, RMAWB From XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="classObject">classObject</param>
        private static void ReadAttachmentDetails(XmlTextReader xmlTextReader, object classObject)
        {
            try
            {
                //Logger.Info("Start of Read AttachmentDetails.");

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.Attachment)))
                    {
                        break;
                    }
                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.AttachmentIndicatorOriginal:
                                xmlTextReader.Read();
                                switch (classObject.GetType().Name)
                                {
                                    case XmlConstants.AirWayBill:
                                        if (xmlTextReader.Value.Equals(XmlConstants.Y))
                                        {
                                            ((AirWayBill)classObject).AttachmentIndicatorOriginal = XmlConstants.Y;
                                        }
                                        break;
                                    case XmlConstants.RejectionMemo:
                                        if (xmlTextReader.Value.Equals(XmlConstants.Y))
                                        {
                                            ((RejectionMemo)classObject).AttachmentIndicatorOriginal = true;
                                        }
                                        break;
                                    case XmlConstants.RMAirWayBill:
                                        if (xmlTextReader.Value.Equals(XmlConstants.Y))
                                        {
                                            ((RMAirWayBill)classObject).AttachmentIndicatorOriginal = XmlConstants.Y;
                                        }
                                        break;
                                    case XmlConstants.BillingMemo:
                                        if (xmlTextReader.Value.Equals(XmlConstants.Y))
                                            ((BillingMemo)classObject).AttachmentIndicatorOriginal = true;
                                        break;
                                    case XmlConstants.BMAirWayBill:
                                        if (xmlTextReader.Value.Equals(XmlConstants.Y))
                                            ((BMAirWayBill)classObject).AttachmentIndicatorOriginal = XmlConstants.Y;
                                        break;
                                    case XmlConstants.CreditMemo:
                                        if (xmlTextReader.Value.Equals(XmlConstants.Y))
                                            ((CreditMemo)classObject).AttachmentIndicatorOriginal = true;
                                        break;
                                    case XmlConstants.CMAirWayBill:
                                        if (xmlTextReader.Value.Equals(XmlConstants.Y))
                                            ((CMAirWayBill)classObject).AttachmentIndicatorOriginal = XmlConstants.Y;
                                        break;
                                }
                                break;
                            case XmlConstants.AttachmentIndicatorValidated:
                                xmlTextReader.Read();
                                switch (classObject.GetType().Name)
                                {
                                    case XmlConstants.AirWayBill:
                                        if (xmlTextReader.Value.Equals(XmlConstants.Y))
                                        {
                                            ((AirWayBill)classObject).AttachmentIndicatorValidated = XmlConstants.Y;
                                        }
                                        break;
                                    case XmlConstants.RejectionMemo:
                                        if (xmlTextReader.Value.Equals(XmlConstants.Y))
                                        {
                                            ((RejectionMemo)classObject).AttachmentIndicatorValidated =true;
                                        }
                                        break;
                                    case XmlConstants.RMAirWayBill:
                                        if (xmlTextReader.Value.Equals(XmlConstants.Y))
                                        {
                                            ((RMAirWayBill)classObject).AttachmentIndicatorValidated = XmlConstants.Y;
                                        }
                                        break;
                                }
                                break;
                            case XmlConstants.NumberOfAttachments:
                                xmlTextReader.Read();
                                switch (classObject.GetType().Name)
                                {
                                    case XmlConstants.AirWayBill:
                                        ((AirWayBill)classObject).NumberOfAttachments = Convert.ToInt32(xmlTextReader.Value);
                                        break;
                                    case XmlConstants.RejectionMemo:
                                        ((RejectionMemo)classObject).NumberOfAttachments = Convert.ToInt32(xmlTextReader.Value);
                                        break;
                                    case XmlConstants.RMAirWayBill:
                                        ((RMAirWayBill)classObject).NumberOfAttachments = Convert.ToInt32(xmlTextReader.Value);
                                        break;
                                }
                                break;
                        }
                    }
                }
                //Logger.Info("End of Read AttachmentDetails.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure("Error Occurred in ReadAttachmentDetails", xmlException);
            }
        }
    }
}
