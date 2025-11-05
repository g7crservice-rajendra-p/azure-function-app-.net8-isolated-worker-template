using System;
using System.Xml;
using QidWorkerRole.SIS.Model;
using System.Collections;
using QidWorkerRole.SIS.FileHandling.Xml.Read.SupportingModels;

namespace QidWorkerRole.SIS.FileHandling.Xml.Read.ReadHelpers
{
    public sealed partial class XmlReaderHelper
    {
        /// <summary>
        /// To Read VAT for BillingCodeSubTotal From XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="billingCodeSubTotal">BillingCodeSubTotal</param>
        private static void ReadLineItemVat(XmlTextReader xmlTextReader, BillingCodeSubTotal billingCodeSubTotal)
        {
            try
            {
                //Logger.Info("Start of ReadLineItemVat.");

                var taxType = string.Empty;

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.Tax)))
                    {
                        break;
                    }

                    if (xmlTextReader.LocalName.Equals(XmlConstants.TaxBreakdown))
                    {
                        ReadLineItemVatDetails(xmlTextReader, billingCodeSubTotal);
                        continue;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.TaxAmount:
                                xmlTextReader.Read();
                                billingCodeSubTotal.TotalVatAmount = Convert.ToDecimal(xmlTextReader.Value);
                                break;
                        }
                    }
                }
                //Logger.Info("End of ReadLineItemVat.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure("Error Occurred in ReadLineItemVat", xmlException);
            }
        }

        /// <summary>
        /// To Read VAT Details for BillingCodeSubTotal From XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="billingCodeSubTotal">BillingCodeSubTotal</param>
        private static void ReadLineItemVatDetails(XmlTextReader xmlTextReader, BillingCodeSubTotal billingCodeSubTotal)
        {
            try
            {
                //Logger.Info("Start of ReadLineItemVatDetails.");

                BillingCodeSubTotalVAT billingCodeSubTotalVAT = new BillingCodeSubTotalVAT();
                billingCodeSubTotalVAT.VatLabel = string.Empty;
                billingCodeSubTotalVAT.VatText = string.Empty;

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.TaxBreakdown)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.TaxLabel:
                                xmlTextReader.Read();
                                billingCodeSubTotalVAT.VatLabel = xmlTextReader.Value;
                                break;
                            case XmlConstants.TaxIdentifier:
                                xmlTextReader.Read();
                                billingCodeSubTotalVAT.VatIdentifier = xmlTextReader.Value;
                                break;
                            case XmlConstants.TaxableAmount:
                                xmlTextReader.Read();
                                billingCodeSubTotalVAT.VatBaseAmount = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.TaxPercent:
                                xmlTextReader.Read();
                                billingCodeSubTotalVAT.VatPercentage = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.TaxAmount:
                                xmlTextReader.Read();
                                billingCodeSubTotalVAT.VatCalculatedAmount = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.TaxText:
                                xmlTextReader.Read();
                                billingCodeSubTotalVAT.VatText = xmlTextReader.Value;
                                break;
                        }
                    }
                }
                billingCodeSubTotal.BillingCodeSubTotalVATList.Add(billingCodeSubTotalVAT);

                //Logger.Info("End of ReadLineItemVatDetails.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure("Error Occurred in ReadLineItemVatDetails", xmlException);
            }
        }

        /// <summary>
        /// To Read Line Item Details Vat From XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="vatArrayList">vatArrayList</param>
        /// <param name="vatAmountsDetails">VatAmountsDetails</param>
        private static void ReadLineItemDetailsVat(XmlTextReader xmlTextReader, ArrayList vatArrayList, VatAmountsDetails vatAmountsDetails)
        {
            try
            {
                //Logger.Info("Start of ReadLineItemDetailsVat.");

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.Tax)))
                    {
                        break;
                    }

                    if (xmlTextReader.LocalName.Equals(XmlConstants.TaxBreakdown))
                    {
                        ReadLineItemDetailVatDetails(xmlTextReader, vatArrayList);
                        continue;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.TaxAmount:
                                AssignTaxAmount(xmlTextReader, vatAmountsDetails);
                                break;
                        }
                    }
                }
                //Logger.Info("End of ReadLineItemDetailsVat.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure("Error Occurred in ReadLineItemDetailsVat", xmlException);
            }
        }

        /// <summary>
        /// To Read Line Item Details Vat Details From XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="vatArrayList">vatArrayList</param>
        private static void ReadLineItemDetailVatDetails(XmlTextReader xmlTextReader, ArrayList vatArrayList)
        {
            try
            {
                //Logger.Info("Start of ReadLineItemDetailVatDetails.");

                VatDetails vatBreakDowns = new VatDetails();
                vatBreakDowns.VatLabel = string.Empty;
                vatBreakDowns.VatText = string.Empty;
                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.TaxBreakdown)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.TaxCode:
                                xmlTextReader.Read();
                                vatBreakDowns.VatCode = xmlTextReader.Value;
                                break;
                            case XmlConstants.TaxLabel:
                                xmlTextReader.Read();
                                vatBreakDowns.VatLabel = xmlTextReader.Value;
                                break;
                            case XmlConstants.TaxIdentifier:
                                xmlTextReader.Read();
                                vatBreakDowns.VatIdentifier = xmlTextReader.Value;
                                break;
                            case XmlConstants.TaxableAmount:
                                xmlTextReader.Read();
                                vatBreakDowns.VatBaseAmount = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.TaxPercent:
                                xmlTextReader.Read();
                                vatBreakDowns.VatPercentage = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.TaxAmount:
                                if (xmlTextReader.HasAttributes)
                                {
                                    xmlTextReader.MoveToAttribute(0);
                                    xmlTextReader.Read();
                                    vatBreakDowns.VatCalculatedAmount = Convert.ToDouble(xmlTextReader.Value);
                                }
                                break;
                            case XmlConstants.TaxText:
                                xmlTextReader.Read();
                                vatBreakDowns.VatText = xmlTextReader.Value;
                                break;
                        }
                    }
                }
                vatArrayList.Add(vatBreakDowns);
                //Logger.Info("End of ReadLineItemDetailVatDetails.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure("Error Occurred in ReadLineItemDetailVatDetails", xmlException);
            }
        }

        /// <summary>
        /// To AssignTaxAmount.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="vatAmountsDetails">VatAmountsDetails</param>
        private static void AssignTaxAmount(XmlTextReader xmlTextReader, VatAmountsDetails vatAmountsDetails)
        {
            //Logger.Info("Start of AssignTaxAmount");

            if (xmlTextReader.HasAttributes)
            {
                for (int counter = 0; counter < xmlTextReader.AttributeCount; counter++)
                {
                    xmlTextReader.MoveToAttribute(counter);
                    switch (xmlTextReader.Value)
                    {
                        case XmlConstants.Billed:
                            xmlTextReader.Read();
                            vatAmountsDetails.VatBilled = Convert.ToDouble(xmlTextReader.Value);
                            break;
                        case XmlConstants.Accepted:
                            xmlTextReader.Read();
                            vatAmountsDetails.VatAccepted = Convert.ToDouble(xmlTextReader.Value);
                            break;
                        case XmlConstants.Difference:
                            xmlTextReader.Read();
                            vatAmountsDetails.VatDifference = Convert.ToDouble(xmlTextReader.Value);
                            break;
                    }
                }
            }
            else
            {
                xmlTextReader.Read();
                vatAmountsDetails.VatBilled = Convert.ToDouble(xmlTextReader.Value);
            }
            //Logger.Info("End of AssignTaxAmount");
        }

        /// <summary>
        /// To Assign Vat Details to Line item details like AWB, BM, CM, RM etc.
        /// </summary>
        /// <param name="classObject">classObject</param>
        /// <param name="vatArrayList">vatArrayList</param>
        /// <param name="vatAmountsDetails">vatAmountsDetails</param>
        /// <param name="otherChargesArrayList">otherChargesArrayList</param>
        private static void AssignVatDetails(object classObject, ArrayList vatArrayList, VatAmountsDetails vatAmountsDetails, ArrayList otherChargesArrayList)
        {
            try
            {
                //Logger.Info("Start of AssignVatDetails");

                AWBVAT aWBVAT;
                BMVAT bMVAT;
                CMVAT cMVAT;
                RMVAT rMVAT;
                BMAWBVAT bMAWBVAT;
                CMAWBVAT cMAWBVAT;
                RMAWBVAT rMAWBVAT;

                switch (classObject.GetType().Name)
                {
                    case XmlConstants.AirWayBill:
                        for (int counter = 0; counter < vatArrayList.Count; counter++)
                        {
                            if (string.IsNullOrEmpty(((VatDetails)vatArrayList[counter]).VatCode))
                            {
                                aWBVAT = new AWBVAT();
                                aWBVAT.VatIdentifier = ((VatDetails)vatArrayList[counter]).VatIdentifier;
                                aWBVAT.VatLabel = ((VatDetails)vatArrayList[counter]).VatLabel;
                                aWBVAT.VatCalculatedAmount = ((VatDetails)vatArrayList[counter]).VatCalculatedAmount;
                                aWBVAT.VatPercentage = ((VatDetails)vatArrayList[counter]).VatPercentage;
                                aWBVAT.VatBaseAmount = ((VatDetails)vatArrayList[counter]).VatBaseAmount;
                                aWBVAT.VatText = ((VatDetails)vatArrayList[counter]).VatText;

                                ((AirWayBill)classObject).AWBVATList.Add(aWBVAT);
                            }
                            else
                            {
                                foreach (var arrOtherCharge in otherChargesArrayList)
                                {
                                    if (((OtherChargAmountDetails)arrOtherCharge).OtherChargeCode.Equals(((VatDetails)vatArrayList[counter]).VatCode))
                                    {
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeCode = ((VatDetails)vatArrayList[counter]).VatCode;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatLabel = ((VatDetails)vatArrayList[counter]).VatLabel;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatCalculatedAmount = ((VatDetails)vatArrayList[counter]).VatCalculatedAmount;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatPercentage = ((VatDetails)vatArrayList[counter]).VatPercentage;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatBaseAmount = ((VatDetails)vatArrayList[counter]).VatBaseAmount;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatText = ((VatDetails)vatArrayList[counter]).VatText;
                                    }
                                }
                            }
                        }
                        ((AirWayBill)classObject).VATAmount = vatAmountsDetails.VatBilled;
                        break;
                    case XmlConstants.RejectionMemo:
                        for (int counter = 0; counter < vatArrayList.Count; counter++)
                        {
                            rMVAT = new RMVAT();

                            rMVAT.VatIdentifier = ((VatDetails)vatArrayList[counter]).VatIdentifier;
                            rMVAT.VatLabel = ((VatDetails)vatArrayList[counter]).VatLabel;
                            rMVAT.VatCalculatedAmount = ((VatDetails)vatArrayList[counter]).VatCalculatedAmount;
                            rMVAT.VatPercentage = ((VatDetails)vatArrayList[counter]).VatPercentage;
                            rMVAT.VatBaseAmount = ((VatDetails)vatArrayList[counter]).VatBaseAmount;
                            rMVAT.VatText = ((VatDetails)vatArrayList[counter]).VatText;

                            ((RejectionMemo)classObject).RMVATList.Add(rMVAT);
                        }

                        ((RejectionMemo)classObject).AcceptedTotalVatAmount = Convert.ToDecimal(vatAmountsDetails.VatAccepted);
                        ((RejectionMemo)classObject).BilledTotalVatAmount = Convert.ToDecimal(vatAmountsDetails.VatBilled);
                        ((RejectionMemo)classObject).TotalVatAmountDifference = vatAmountsDetails.VatDifference;
                        break;
                    case XmlConstants.BillingMemo:
                        for (int counter = 0; counter < vatArrayList.Count; counter++)
                        {
                            bMVAT = new BMVAT();

                            bMVAT.VatIdentifier = ((VatDetails)vatArrayList[counter]).VatIdentifier;
                            bMVAT.VatLabel = ((VatDetails)vatArrayList[counter]).VatLabel;
                            bMVAT.VatCalculatedAmount = ((VatDetails)vatArrayList[counter]).VatCalculatedAmount;
                            bMVAT.VatPercentage = ((VatDetails)vatArrayList[counter]).VatPercentage;
                            bMVAT.VatBaseAmount = ((VatDetails)vatArrayList[counter]).VatBaseAmount;
                            bMVAT.VatText = ((VatDetails)vatArrayList[counter]).VatText;

                            ((BillingMemo)classObject).BMVATList.Add(bMVAT);
                        }
                        ((BillingMemo)classObject).BilledTotalVatAmount = Convert.ToDecimal(vatAmountsDetails.VatBilled);
                        break;
                    case XmlConstants.CreditMemo:
                        for (int counter = 0; counter < vatArrayList.Count; counter++)
                        {
                            cMVAT = new CMVAT();

                            cMVAT.VatIdentifier = ((VatDetails)vatArrayList[counter]).VatIdentifier;
                            cMVAT.VatLabel = ((VatDetails)vatArrayList[counter]).VatLabel;
                            cMVAT.VatCalculatedAmount = ((VatDetails)vatArrayList[counter]).VatCalculatedAmount;
                            cMVAT.VatPercentage = ((VatDetails)vatArrayList[counter]).VatPercentage;
                            cMVAT.VatBaseAmount = ((VatDetails)vatArrayList[counter]).VatBaseAmount;
                            cMVAT.VatText = ((VatDetails)vatArrayList[counter]).VatText;

                            ((CreditMemo)classObject).CMVATList.Add(cMVAT);
                        }
                        ((CreditMemo)classObject).TotalVatAmountCredited = Convert.ToDecimal(vatAmountsDetails.VatBilled);
                        break;
                    case XmlConstants.RMAirWayBill:
                        for (int counter = 0; counter < vatArrayList.Count; counter++)
                        {
                            if (string.IsNullOrEmpty(((VatDetails)vatArrayList[counter]).VatCode))
                            {
                                rMAWBVAT = new RMAWBVAT();

                                rMAWBVAT.VatIdentifier = ((VatDetails)vatArrayList[counter]).VatIdentifier;
                                rMAWBVAT.VatLabel = ((VatDetails)vatArrayList[counter]).VatLabel;
                                rMAWBVAT.VatCalculatedAmount = ((VatDetails)vatArrayList[counter]).VatCalculatedAmount;
                                rMAWBVAT.VatPercentage = ((VatDetails)vatArrayList[counter]).VatPercentage;
                                rMAWBVAT.VatBaseAmount = ((VatDetails)vatArrayList[counter]).VatBaseAmount;
                                rMAWBVAT.VatText = ((VatDetails)vatArrayList[counter]).VatText;

                                ((RMAirWayBill)classObject).RMAWBVATList.Add(rMAWBVAT);
                            }
                            else
                            {

                                foreach (var arrOtherCharge in otherChargesArrayList)
                                {
                                    if (((OtherChargAmountDetails)arrOtherCharge).OtherChargeCode.Equals(((VatDetails)vatArrayList[counter]).VatCode))
                                    {
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeCode = ((VatDetails)vatArrayList[counter]).VatCode;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatLabel = ((VatDetails)vatArrayList[counter]).VatLabel;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatCalculatedAmount = ((VatDetails)vatArrayList[counter]).VatCalculatedAmount;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatPercentage = ((VatDetails)vatArrayList[counter]).VatPercentage;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatBaseAmount = ((VatDetails)vatArrayList[counter]).VatBaseAmount;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatText = ((VatDetails)vatArrayList[counter]).VatText;
                                    }
                                }
                            }
                        }

                        ((RMAirWayBill)classObject).BilledVatAmount = vatAmountsDetails.VatBilled;
                        ((RMAirWayBill)classObject).AcceptedVatAmount = vatAmountsDetails.VatAccepted;
                        ((RMAirWayBill)classObject).VatAmountDifference = vatAmountsDetails.VatDifference;
                        break;
                    case XmlConstants.BMAirWayBill:
                        for (int counter = 0; counter < vatArrayList.Count; counter++)
                        {
                            if (string.IsNullOrEmpty(((VatDetails)vatArrayList[counter]).VatCode))
                            {
                                bMAWBVAT = new QidWorkerRole.SIS.Model.BMAWBVAT();

                                bMAWBVAT.VatIdentifier = ((VatDetails)vatArrayList[counter]).VatIdentifier;
                                bMAWBVAT.VatLabel = ((VatDetails)vatArrayList[counter]).VatLabel;
                                bMAWBVAT.VatCalculatedAmount = ((VatDetails)vatArrayList[counter]).VatCalculatedAmount;
                                bMAWBVAT.VatPercentage = ((VatDetails)vatArrayList[counter]).VatPercentage;
                                bMAWBVAT.VatBaseAmount = ((VatDetails)vatArrayList[counter]).VatBaseAmount;
                                bMAWBVAT.VatText = ((VatDetails)vatArrayList[counter]).VatText;

                                ((BMAirWayBill)classObject).BMAWBVATList.Add(bMAWBVAT);
                            }
                            else
                            {
                                foreach (var arrOtherCharge in otherChargesArrayList)
                                {
                                    if (((OtherChargAmountDetails)arrOtherCharge).OtherChargeCode.Equals(((VatDetails)vatArrayList[counter]).VatCode))
                                    {
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeCode = ((VatDetails)vatArrayList[counter]).VatCode;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatLabel = ((VatDetails)vatArrayList[counter]).VatLabel;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatCalculatedAmount = ((VatDetails)vatArrayList[counter]).VatCalculatedAmount;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatPercentage = ((VatDetails)vatArrayList[counter]).VatPercentage;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatBaseAmount = ((VatDetails)vatArrayList[counter]).VatBaseAmount;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatText = ((VatDetails)vatArrayList[counter]).VatText;
                                    }
                                }
                            }
                        }
                        ((BMAirWayBill)classObject).BilledVatAmount = vatAmountsDetails.VatBilled;
                        break;
                    case XmlConstants.CMAirWayBill:
                        for (int counter = 0; counter < vatArrayList.Count; counter++)
                        {
                            if (string.IsNullOrEmpty(((VatDetails)vatArrayList[counter]).VatCode))
                            {
                                cMAWBVAT = new QidWorkerRole.SIS.Model.CMAWBVAT();

                                cMAWBVAT.VatIdentifier = ((VatDetails)vatArrayList[counter]).VatIdentifier;
                                cMAWBVAT.VatLabel = ((VatDetails)vatArrayList[counter]).VatLabel;
                                cMAWBVAT.VatCalculatedAmount = ((VatDetails)vatArrayList[counter]).VatCalculatedAmount;
                                cMAWBVAT.VatPercentage = ((VatDetails)vatArrayList[counter]).VatPercentage;
                                cMAWBVAT.VatBaseAmount = ((VatDetails)vatArrayList[counter]).VatBaseAmount;
                                cMAWBVAT.VatText = ((VatDetails)vatArrayList[counter]).VatText;

                                ((CMAirWayBill)classObject).CMAWBVATList.Add(cMAWBVAT);
                            }
                            else
                            {
                                foreach (var arrOtherCharge in otherChargesArrayList)
                                {
                                    if (((OtherChargAmountDetails)arrOtherCharge).OtherChargeCode.Equals(((VatDetails)vatArrayList[counter]).VatCode))
                                    {
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeCode = ((VatDetails)vatArrayList[counter]).VatCode;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatLabel = ((VatDetails)vatArrayList[counter]).VatLabel;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatCalculatedAmount = ((VatDetails)vatArrayList[counter]).VatCalculatedAmount;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatPercentage = ((VatDetails)vatArrayList[counter]).VatPercentage;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatBaseAmount = ((VatDetails)vatArrayList[counter]).VatBaseAmount;
                                        ((OtherChargAmountDetails)arrOtherCharge).OtherChargeVatText = ((VatDetails)vatArrayList[counter]).VatText;
                                    }
                                }
                            }
                        }
                        ((CMAirWayBill)classObject).CreditedVatAmount = vatAmountsDetails.VatBilled;
                        break;
                }
                //Logger.Info("End of AssignVatDetails");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure("Error Occurred in AssignVatDetails", xmlException);
            }
        }

        /// <summary>
        /// Read InvoiceTotalVAT From XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="invoiceTotalVAT">InvoiceTotalVAT</param>
        private static void ReadInvoiceTotalVAT(XmlTextReader xmlTextReader, InvoiceTotalVAT invoiceTotalVAT)
        {
            try
            {
                //Logger.Info("Start of ReadInvoiceTotalVAT.");

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.TaxBreakdown)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.TaxIdentifier:
                                xmlTextReader.Read();
                                invoiceTotalVAT.VatIdentifier = xmlTextReader.Value;
                                break;
                            case XmlConstants.TaxLabel:
                                xmlTextReader.Read();
                                invoiceTotalVAT.VatLabel = xmlTextReader.Value;
                                break;
                            case XmlConstants.TaxableAmount:
                                xmlTextReader.Read();
                                invoiceTotalVAT.VatBaseAmount = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.TaxPercent:
                                xmlTextReader.Read();
                                invoiceTotalVAT.VatPercentage = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.TaxAmount:
                                xmlTextReader.Read();
                                invoiceTotalVAT.VatCalculatedAmount = Convert.ToDouble(xmlTextReader.Value);
                                break;
                            case XmlConstants.TaxText:
                                xmlTextReader.Read();
                                invoiceTotalVAT.VatText = xmlTextReader.Value;
                                break;
                        }
                    }
                }
                //Logger.Info("End of ReadInvoiceTotalVAT.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure("Error Occurred in ReadInvoiceTotalVAT", xmlException);
            }
        }
    }
}