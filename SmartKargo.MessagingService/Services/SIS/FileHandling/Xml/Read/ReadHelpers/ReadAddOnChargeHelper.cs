using QidWorkerRole.SIS.FileHandling.Xml.Read.SupportingModels;
using QidWorkerRole.SIS.Model;
using System.Collections;
using System.Xml;

namespace QidWorkerRole.SIS.FileHandling.Xml.Read.ReadHelpers
{
    public sealed partial class XmlReaderHelper
    {

        #region Read BillingCodeSubTotal Add On Charge Amounts.

        /// <summary>
        /// To Read BillingCodeSubTotal Add On Charge Amounts from XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="billingCodeSubTotal">BillingCodeSubTotal</param>
        private static void ReadBillingCodeTotalAddonCharges(XmlTextReader xmlTextReader, BillingCodeSubTotal billingCodeSubTotal)
        {
            try
            {
                //Logger.Info("Start of ReadBillingCodeTotalAddonCharges.");

                string addonChargeType = string.Empty;

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.AddOnCharges)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.AddOnChargeName:
                                xmlTextReader.Read();
                                addonChargeType = xmlTextReader.Value;
                                break;
                            case XmlConstants.AddOnChargeAmount:
                                xmlTextReader.Read();
                                if (addonChargeType.StartsWith(XmlConstants.IscAllowed))
                                {
                                    billingCodeSubTotal.TotalIscAmount = Convert.ToDecimal(xmlTextReader.Value);
                                }
                                else if (addonChargeType.StartsWith(XmlConstants.OtherChargesAllowed))
                                {
                                    billingCodeSubTotal.TotalOtherCharge = Convert.ToDecimal(xmlTextReader.Value);
                                }
                                break;
                        }
                    }
                }
                //Logger.Info("End of ReadBillingCodeTotalAddonCharges.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure("Error Occurred in ReadBillingCodeTotalAddonCharges", xmlException);
            }
        }

        #endregion

        #region Read Add On Charges Amounts for RMAirWayBill, BMAirWayBill, CMAirWayBill & LineItemDetails Level.

        /// <summary>
        /// To Read Add On Charges Amounts for RMAirWayBill, BMAirWayBill, CMAirWayBill & LineItemDetails Level from XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="addOnChargeAmountsDetails">AddOnChargeAmountsDetails</param>
        /// <param name="otherChargesArrayList">ArrayList</param>
        private static void ReadLineItemdetailAddonCharges(XmlTextReader xmlTextReader, AddOnChargeAmountsDetails addOnChargeAmountsDetails, ArrayList otherChargesArrayList)
        {
            try
            {
                //Logger.Info("Start of ReadLineItemdetailAddonCharges.");

                string addonChargeType = string.Empty;
                string elementName;
                string addOnChgCode = string.Empty;

                OtherChargAmountDetails otherChargeBreakdownDetails = null;

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.AddOnCharges)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        elementName = xmlTextReader.LocalName;

                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.AddOnChargeName:
                                xmlTextReader.Read();
                                addonChargeType = xmlTextReader.Value;
                                break;
                            case XmlConstants.AddOnChargeAmount:
                            case XmlConstants.AddOnChargePercentage:
                            case XmlConstants.AddOnChargeCode:
                                xmlTextReader.Read();
                                switch (addonChargeType)
                                {
                                    case XmlConstants.IscAllowed:
                                        if (elementName.Equals(XmlConstants.AddOnChargeAmount))
                                        {
                                            addOnChargeAmountsDetails.InterlineServiceChargeAmount = Convert.ToDouble(xmlTextReader.Value);
                                        }
                                        else if (elementName.Equals(XmlConstants.AddOnChargePercentage))
                                        {
                                            addOnChargeAmountsDetails.InterlineServiceChargePercentage = Convert.ToDouble(xmlTextReader.Value);
                                        }
                                        break;
                                    case XmlConstants.IscAccepted:
                                        if (elementName.Equals(XmlConstants.AddOnChargeAmount))
                                        {
                                            addOnChargeAmountsDetails.IscAccepted = Convert.ToDouble(xmlTextReader.Value);
                                        }
                                        else if (elementName.Equals(XmlConstants.AddOnChargePercentage))
                                        {
                                            addOnChargeAmountsDetails.IscAcceptedPercentage = Convert.ToDouble(xmlTextReader.Value);
                                        }
                                        break;
                                    case XmlConstants.IscDifference:
                                        if (elementName.Equals(XmlConstants.AddOnChargeAmount))
                                        {
                                            addOnChargeAmountsDetails.IscDifference = Convert.ToDouble(xmlTextReader.Value);
                                        }
                                        break;
                                    case XmlConstants.OtherChargesAllowed:
                                        if (elementName.Equals(XmlConstants.AddOnChargeCode))
                                        {
                                            if (string.IsNullOrEmpty(addOnChgCode))
                                            {
                                                otherChargeBreakdownDetails = new OtherChargAmountDetails();
                                                addOnChgCode = xmlTextReader.Value;
                                                otherChargeBreakdownDetails.OtherChargeCode = xmlTextReader.Value;
                                                otherChargeBreakdownDetails.OtherChargeName = addonChargeType;
                                            }
                                            else
                                            {
                                                otherChargeBreakdownDetails = null;
                                                addOnChgCode = null;
                                            }

                                        }
                                        if (elementName.Equals(XmlConstants.AddOnChargeAmount))
                                        {
                                            if (string.IsNullOrEmpty(addOnChgCode))
                                            {
                                                addOnChargeAmountsDetails.OtherCharges = Convert.ToDouble(xmlTextReader.Value);
                                            }
                                            else if (otherChargeBreakdownDetails != null)
                                            {
                                                otherChargeBreakdownDetails.OtherChargeCodeValue = Convert.ToDouble(xmlTextReader.Value);
                                            }
                                        }
                                        break;
                                    case XmlConstants.OtherChargesAccepted:
                                        if (elementName.Equals(XmlConstants.AddOnChargeAmount))
                                        {
                                            addOnChargeAmountsDetails.OtherChargesAccepted = Convert.ToDouble(xmlTextReader.Value);
                                        }
                                        else if (elementName.Equals(XmlConstants.AddOnChargeCode))
                                        {
                                            addOnChargeAmountsDetails.OtherChargesAccptedCode = xmlTextReader.Value;
                                        }
                                        break;
                                    case XmlConstants.OtherChargesDifference:
                                        if (elementName.Equals(XmlConstants.AddOnChargeCode))
                                        {
                                            if (string.IsNullOrEmpty(addOnChgCode))
                                            {
                                                otherChargeBreakdownDetails = new OtherChargAmountDetails();
                                                addOnChgCode = xmlTextReader.Value;
                                                otherChargeBreakdownDetails.OtherChargeCode = xmlTextReader.Value;
                                                otherChargeBreakdownDetails.OtherChargeName = addonChargeType;
                                            }
                                            else
                                            {
                                                otherChargeBreakdownDetails = null;
                                                addOnChgCode = null;
                                            }
                                        }
                                        if (elementName.Equals(XmlConstants.AddOnChargeAmount))
                                        {
                                            if (string.IsNullOrEmpty(addOnChgCode))
                                            {
                                                addOnChargeAmountsDetails.OtherChargesDifference = Convert.ToDouble(xmlTextReader.Value);
                                            }
                                            else if (otherChargeBreakdownDetails != null)
                                            {
                                                otherChargeBreakdownDetails.OtherChargeCodeValue = Convert.ToDouble(xmlTextReader.Value);
                                            }
                                        }
                                        break;
                                    case XmlConstants.AmountSubjectToISCAllowed:
                                        if (elementName.Equals(XmlConstants.AddOnChargeAmount))
                                        {
                                            addOnChargeAmountsDetails.AmountSubjectToInterlineServiceCharge = Convert.ToDouble(xmlTextReader.Value);
                                        }
                                        break;
                                    case XmlConstants.AmountSubjectToISCAccepted:
                                        if (elementName.Equals(XmlConstants.AddOnChargeAmount))
                                        {
                                            addOnChargeAmountsDetails.AmountSubjectToIscAccepted = Convert.ToDouble(xmlTextReader.Value);
                                        }
                                        break;
                                    case XmlConstants.AmountSubjectToISCDifference:
                                        if (elementName.Equals(XmlConstants.AddOnChargeAmount))
                                        {
                                            addOnChargeAmountsDetails.AmountSubjectToIscDifference = Convert.ToDouble(xmlTextReader.Value);
                                        }
                                        break;
                                }
                                break;
                        }
                    }
                }
                if (otherChargeBreakdownDetails != null)
                {
                    otherChargesArrayList.Add(otherChargeBreakdownDetails);
                }
                //Logger.Info("Start of ReadLineItemdetailAddonCharges.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure("Error Occurred in ReadLineItemdetailAddonCharges", xmlException);
            }
        }

        /// <summary>
        /// To Assingn Add On Charge Amounts to there respective Classes.
        /// </summary>
        /// <param name="classObject">classObject</param>
        /// <param name="addOnChargeAmountsDetails">AddOnChargeAmountsDetails</param>
        private static void AssignAddOnCharges(object classObject, AddOnChargeAmountsDetails addOnChargeAmountsDetails)
        {
            try
            {
                //Logger.Info("Start of AssignAddOnCharges.");

                switch (classObject.GetType().Name)
                {
                    case XmlConstants.AirWayBill:
                        ((AirWayBill)classObject).InterlineServiceChargeAmount = addOnChargeAmountsDetails.InterlineServiceChargeAmount;
                        ((AirWayBill)classObject).InterlineServiceChargePercentage = addOnChargeAmountsDetails.InterlineServiceChargePercentage;
                        ((AirWayBill)classObject).OtherCharges = addOnChargeAmountsDetails.OtherCharges;
                        ((AirWayBill)classObject).AmountSubjectToInterlineServiceCharge = addOnChargeAmountsDetails.AmountSubjectToInterlineServiceCharge;
                        break;
                    case XmlConstants.RejectionMemo:
                        ((RejectionMemo)classObject).AllowedTotalIscAmount = Convert.ToDecimal(addOnChargeAmountsDetails.InterlineServiceChargeAmount);
                        ((RejectionMemo)classObject).AcceptedTotalIscAmount = Convert.ToDecimal(addOnChargeAmountsDetails.IscAccepted);
                        ((RejectionMemo)classObject).TotalIscAmountDifference = Convert.ToDecimal(addOnChargeAmountsDetails.IscDifference);
                        ((RejectionMemo)classObject).BilledTotalOtherChargeAmount = Convert.ToDecimal(addOnChargeAmountsDetails.OtherCharges);
                        ((RejectionMemo)classObject).AcceptedTotalOtherChargeAmount = Convert.ToDecimal(addOnChargeAmountsDetails.OtherChargesAccepted);
                        ((RejectionMemo)classObject).TotalOtherChargeDifference = Convert.ToDecimal(addOnChargeAmountsDetails.OtherChargesDifference);
                        break;
                    case XmlConstants.BillingMemo:
                        ((BillingMemo)classObject).BilledTotalIscAmount = Convert.ToDecimal(addOnChargeAmountsDetails.InterlineServiceChargeAmount);
                        ((BillingMemo)classObject).BilledTotalOtherChargeAmount = Convert.ToDecimal(addOnChargeAmountsDetails.OtherCharges);
                        break;
                    case XmlConstants.CreditMemo:
                        ((CreditMemo)classObject).TotalIscAmountCredited = Convert.ToDecimal(addOnChargeAmountsDetails.InterlineServiceChargeAmount);
                        ((CreditMemo)classObject).TotalOtherChargeAmt = Convert.ToDecimal(addOnChargeAmountsDetails.OtherCharges);
                        break;
                    case XmlConstants.RMAirWayBill:
                        ((RMAirWayBill)classObject).AllowedIscAmount = Convert.ToDouble(addOnChargeAmountsDetails.InterlineServiceChargeAmount);
                        ((RMAirWayBill)classObject).AllowedIscPercentage = Convert.ToDouble(addOnChargeAmountsDetails.InterlineServiceChargePercentage);
                        ((RMAirWayBill)classObject).AcceptedIscAmount = Convert.ToDouble(addOnChargeAmountsDetails.IscAccepted);
                        ((RMAirWayBill)classObject).AcceptedIscPercentage = Convert.ToDouble(addOnChargeAmountsDetails.IscAcceptedPercentage);
                        ((RMAirWayBill)classObject).IscAmountDifference = Convert.ToDouble(addOnChargeAmountsDetails.IscDifference);
                        ((RMAirWayBill)classObject).BilledOtherCharge = Convert.ToDouble(addOnChargeAmountsDetails.OtherCharges);
                        ((RMAirWayBill)classObject).AcceptedOtherCharge = Convert.ToDouble(addOnChargeAmountsDetails.OtherChargesAccepted);
                        ((RMAirWayBill)classObject).OtherChargeDiff = Convert.ToDouble(addOnChargeAmountsDetails.OtherChargesDifference);
                        ((RMAirWayBill)classObject).AllowedAmtSubToIsc = Convert.ToDouble(addOnChargeAmountsDetails.AmountSubjectToInterlineServiceCharge);
                        ((RMAirWayBill)classObject).AcceptedAmtSubToIsc = Convert.ToDouble(addOnChargeAmountsDetails.AmountSubjectToIscAccepted);
                        break;
                    case XmlConstants.BMAirWayBill:
                        ((BMAirWayBill)classObject).BilledIscAmount = addOnChargeAmountsDetails.InterlineServiceChargeAmount;
                        ((BMAirWayBill)classObject).BilledIscPercentage = addOnChargeAmountsDetails.InterlineServiceChargePercentage;
                        ((BMAirWayBill)classObject).BilledOtherCharge = addOnChargeAmountsDetails.OtherCharges;
                        ((BMAirWayBill)classObject).BilledAmtSubToIsc = addOnChargeAmountsDetails.AmountSubjectToInterlineServiceCharge;
                        break;
                    case XmlConstants.CMAirWayBill:
                        ((CMAirWayBill)classObject).CreditedIscAmount = addOnChargeAmountsDetails.InterlineServiceChargeAmount;
                        ((CMAirWayBill)classObject).CreditedIscPercentage = addOnChargeAmountsDetails.InterlineServiceChargePercentage;
                        ((CMAirWayBill)classObject).CreditedOtherCharge = addOnChargeAmountsDetails.OtherCharges;
                        ((CMAirWayBill)classObject).CreditedAmtSubToIsc = addOnChargeAmountsDetails.AmountSubjectToInterlineServiceCharge;
                        break;
                }
                //Logger.Info("End of AssignAddOnCharges.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure("Error Occurred in AssignAddOnCharges", xmlException);
            }
        }

        #endregion

        #region Read Add On Charges for InvoiceTotal from InvoiceSummary of XML File.

        /// <summary>
        /// To Read Add On Charges for InvoiceTotal from InvoiceSummary of XML File.
        /// </summary>
        /// <param name="xmlTextReader">XmlTextReader</param>
        /// <param name="invoiceTotal">InvoiceTotal</param>
        private static void ReadInvoiceSummaryAddonCharges(XmlTextReader xmlTextReader, InvoiceTotal invoiceTotal)
        {
            try
            {
                //Logger.Info("Start of ReadInvoiceSummaryAddonCharges.");

                string addonChargeType = string.Empty;

                while (xmlTextReader.Read())
                {
                    if (!(ReadTransmissionHelper.IsContinue(xmlTextReader, XmlConstants.AddOnCharges)))
                    {
                        break;
                    }

                    if (xmlTextReader.NodeType != XmlNodeType.EndElement && !xmlTextReader.IsEmptyElement)
                    {
                        switch (xmlTextReader.LocalName)
                        {
                            case XmlConstants.AddOnChargeName:
                                xmlTextReader.Read();
                                addonChargeType = xmlTextReader.Value;
                                break;
                            case XmlConstants.AddOnChargeAmount:
                                xmlTextReader.Read();
                                if (addonChargeType.StartsWith(XmlConstants.IscAllowed))
                                {
                                    invoiceTotal.TotalInterlineServiceChargeAmount = Convert.ToDecimal(xmlTextReader.Value);
                                }
                                else if (addonChargeType.StartsWith(XmlConstants.OtherChargesAllowed))
                                {
                                    invoiceTotal.TotalOtherCharges = Convert.ToDecimal(xmlTextReader.Value);
                                }
                                break;
                        }
                    }
                }
                //Logger.Info("End of ReadInvoiceSummaryAddonCharges.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure("Error Occurred in ReadInvoiceSummaryAddonCharges", xmlException);
            }
        }

        #endregion

    }
}
