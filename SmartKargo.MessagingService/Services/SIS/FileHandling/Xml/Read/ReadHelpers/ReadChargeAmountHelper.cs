using System;
using System.Xml;
using QidWorkerRole.SIS.Model;
using QidWorkerRole.SIS.FileHandling.Xml.Read.SupportingModels;

namespace QidWorkerRole.SIS.FileHandling.Xml.Read.ReadHelpers
{
    public sealed partial class XmlReaderHelper
    {
        /// <summary>
        /// To Assign BillingCodeSubTotal Charge Amounts.
        /// </summary>
        /// <param name="billingCodeSubTotal">BillingCodeSubTotal</param>
        /// <param name="xmlTextReader">XmlTextReader</param>
        private static void AssignBillingCodeSubTotalChargeAmounts(BillingCodeSubTotal billingCodeSubTotal, XmlTextReader xmlTextReader)
        {
            //Logger.Info("Start of AssignBillingCodeSubTotalChargeAmounts.");

            if (xmlTextReader.HasAttributes)
            {
                for (int counter = 0; counter < xmlTextReader.AttributeCount; counter++)
                {
                    xmlTextReader.MoveToAttribute(counter);
                    switch (xmlTextReader.Value)
                    {
                        case XmlConstants.Weight:
                            xmlTextReader.Read();
                            billingCodeSubTotal.TotalWeightCharge = Convert.ToDecimal(xmlTextReader.Value);
                            break;
                        case XmlConstants.Valuation:
                            xmlTextReader.Read();
                            billingCodeSubTotal.TotalValuationCharge = Convert.ToDecimal(xmlTextReader.Value);
                            break;
                    }
                }
            }
            else
            {
                xmlTextReader.Read();
                billingCodeSubTotal.TotalWeightCharge = Convert.ToDecimal(xmlTextReader.Value);
            }
            //Logger.Info("End of AssignBillingCodeSubTotalChargeAmounts.");
        }

        /// <summary>
        /// To Read Charge Amounts for RM/BM/CB AirWayBill and LineItemDetails from XML File.
        /// </summary>
        /// <param name="chargeAmountsDetails">ChargeAmountsDetails</param>
        /// <param name="xmlTextReader">XmlTextReader</param>
        private static void ReadLineItemDetailChargeAmounts(ChargeAmountsDetails chargeAmountsDetails, XmlTextReader xmlTextReader)
        {
            //Logger.Info("Start of ReadLineItemDetailChargeAmounts.");

            if (xmlTextReader.HasAttributes)
            {
                for (int counter = 0; counter < xmlTextReader.AttributeCount; counter++)
                {
                    xmlTextReader.MoveToAttribute(counter);
                    switch (xmlTextReader.Value)
                    {
                        case XmlConstants.WeightBilled:
                            xmlTextReader.Read();
                            chargeAmountsDetails.WeightBilled = Convert.ToDouble(xmlTextReader.Value);
                            break;
                        case XmlConstants.WeightAccepted:
                            xmlTextReader.Read();
                            chargeAmountsDetails.WeightAccepted = Convert.ToDouble(xmlTextReader.Value);
                            break;
                        case XmlConstants.WeightDifference:
                            xmlTextReader.Read();
                            chargeAmountsDetails.WeightDifference = Convert.ToDouble(xmlTextReader.Value);
                            break;
                        case XmlConstants.ValuationBilled:
                            xmlTextReader.Read();
                            chargeAmountsDetails.ValuationBilled = Convert.ToDouble(xmlTextReader.Value);
                            break;
                        case XmlConstants.ValuationAccepted:
                            xmlTextReader.Read();
                            chargeAmountsDetails.ValuationAccepted = Convert.ToDouble(xmlTextReader.Value);
                            break;
                        case XmlConstants.ValuationDifference:
                            xmlTextReader.Read();
                            chargeAmountsDetails.ValuationDifference = Convert.ToDouble(xmlTextReader.Value);
                            break;
                    }
                }
            }
            else
            {
                xmlTextReader.Read();
                chargeAmountsDetails.WeightBilled = Convert.ToDouble(xmlTextReader.Value);
            }

            //Logger.Info("End of ReadLineItemDetailChargeAmounts.");
        }

        /// <summary>
        /// To Assign the Charge Amounts to there respective Class Objects.
        /// </summary>
        /// <param name="classObject">classObject</param>
        /// <param name="chargeAmountsDetails">ChargeAmountsDetails</param>
        private static void AssignChargeAmounts(object classObject, ChargeAmountsDetails chargeAmountsDetails)
        {
            try
            {
                //Logger.Info("Start of AssignChargeAmounts.");

                switch (classObject.GetType().Name)
                {
                    case XmlConstants.AirWayBill:
                        ((AirWayBill)classObject).WeightCharges = chargeAmountsDetails.WeightBilled;
                        ((AirWayBill)classObject).ValuationCharges = chargeAmountsDetails.ValuationBilled;
                        ((AirWayBill)classObject).AWBTotalAmount = chargeAmountsDetails.TotalNetAmount;
                        break;
                    case XmlConstants.RejectionMemo:
                        ((RejectionMemo)classObject).BilledTotalWeightCharge = Convert.ToDecimal(chargeAmountsDetails.WeightBilled);
                        ((RejectionMemo)classObject).AcceptedTotalWeightCharge = Convert.ToDecimal(chargeAmountsDetails.WeightAccepted);
                        ((RejectionMemo)classObject).TotalWeightChargeDifference = Convert.ToDecimal(chargeAmountsDetails.WeightDifference);
                        ((RejectionMemo)classObject).BilledTotalValuationCharge = Convert.ToDecimal(chargeAmountsDetails.ValuationBilled);
                        ((RejectionMemo)classObject).AcceptedTotalValuationCharge = Convert.ToDecimal(chargeAmountsDetails.ValuationAccepted);
                        ((RejectionMemo)classObject).TotalValuationChargeDifference = Convert.ToDecimal(chargeAmountsDetails.ValuationDifference);
                        ((RejectionMemo)classObject).TotalNetRejectAmount = Convert.ToDecimal(chargeAmountsDetails.TotalNetAmount);
                        break;
                    case XmlConstants.BillingMemo:
                        ((BillingMemo)classObject).BilledTotalWeightCharge = Convert.ToDecimal(chargeAmountsDetails.WeightBilled);
                        ((BillingMemo)classObject).BilledTotalValuationAmount = Convert.ToDecimal(chargeAmountsDetails.ValuationBilled);
                        ((BillingMemo)classObject).NetBilledAmount = Convert.ToDecimal(chargeAmountsDetails.TotalNetAmount);
                        break;
                    case XmlConstants.CreditMemo:
                        ((CreditMemo)classObject).TotalWeightCharges = Convert.ToDecimal(chargeAmountsDetails.WeightBilled);
                        ((CreditMemo)classObject).TotalValuationAmt = Convert.ToDecimal(chargeAmountsDetails.ValuationBilled);
                        ((CreditMemo)classObject).NetAmountCredited = Convert.ToDecimal(chargeAmountsDetails.TotalNetAmount);
                        break;
                    case XmlConstants.RMAirWayBill:
                        ((RMAirWayBill)classObject).BilledWeightCharge = Convert.ToDouble(chargeAmountsDetails.WeightBilled);
                        ((RMAirWayBill)classObject).AcceptedWeightCharge = Convert.ToDouble(chargeAmountsDetails.WeightAccepted);
                        ((RMAirWayBill)classObject).WeightChargeDiff = Convert.ToDouble(chargeAmountsDetails.WeightDifference);
                        ((RMAirWayBill)classObject).BilledValuationCharge = Convert.ToDouble(chargeAmountsDetails.ValuationBilled);
                        ((RMAirWayBill)classObject).AcceptedValuationCharge = Convert.ToDouble(chargeAmountsDetails.ValuationAccepted);
                        ((RMAirWayBill)classObject).ValuationChargeDiff = Convert.ToDouble(chargeAmountsDetails.ValuationDifference);
                        break;
                    case XmlConstants.BMAirWayBill:
                        ((BMAirWayBill)classObject).BilledWeightCharge = Convert.ToDouble(chargeAmountsDetails.WeightBilled);
                        ((BMAirWayBill)classObject).BilledValuationCharge = Convert.ToDouble(chargeAmountsDetails.ValuationBilled);
                        break;
                    case XmlConstants.CMAirWayBill:
                        ((CMAirWayBill)classObject).CreditedWeightCharge = Convert.ToDouble(chargeAmountsDetails.WeightBilled);
                        ((CMAirWayBill)classObject).CreditedValuationCharge = Convert.ToDouble(chargeAmountsDetails.ValuationBilled);
                        break;
                }

                //Logger.Info("End of AssignChargeAmounts.");
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure("Error Occurred in AssignChargeAmounts", xmlException);
            }
        }
    }
}