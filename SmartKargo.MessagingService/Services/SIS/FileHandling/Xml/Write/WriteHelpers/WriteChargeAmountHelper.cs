using System;
using System.Xml;
using Microsoft.Extensions.Logging;
using QidWorkerRole.SIS.Model;

namespace QidWorkerRole.SIS.FileHandling.Xml.Write.WriteHelpers
{
    public sealed partial class XmlWriterHelper
    {
        /// <summary>
        /// To Write Charge amount
        /// </summary>
        /// <param name="xmlTextWriter">XmlTextWriter</param>
        /// <param name="classObject">classObject</param>
        private static void WriteChargeAmount(XmlTextWriter xmlTextWriter, object classObject)
        {
            try
            {
                //Logger.InfoFormat("Start of WriteChargeAmount for {0}", classObject.GetType().Name);

                switch (classObject.GetType().Name)
                {
                    case XmlConstants.AirWayBill:
                        var airWayBill = classObject as AirWayBill;
                        if (airWayBill != null)
                        {
                            if (airWayBill.WeightCharges != null)
                            {
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.WeightBilled, String.Format("{0:0.000}", airWayBill.WeightCharges));
                            }
                            if (airWayBill.ValuationCharges != null)
                            {
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.ValuationBilled, String.Format("{0:0.000}", airWayBill.ValuationCharges));
                            }
                        }
                        break;
                    case XmlConstants.RejectionMemo:
                        var rejectionMemo = classObject as RejectionMemo;

                        if (rejectionMemo != null)
                        {
                            WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.WeightBilled, String.Format("{0:0.000}", rejectionMemo.BilledTotalWeightCharge));
                            WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.WeightAccepted, String.Format("{0:0.000}", rejectionMemo.AcceptedTotalWeightCharge));
                            WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.WeightDifference, String.Format("{0:0.000}", rejectionMemo.TotalWeightChargeDifference));
                            WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.ValuationBilled, String.Format("{0:0.000}", rejectionMemo.BilledTotalValuationCharge));
                            WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.ValuationAccepted, String.Format("{0:0.000}", rejectionMemo.AcceptedTotalValuationCharge));
                            WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.ValuationDifference, String.Format("{0:0.000}", rejectionMemo.TotalValuationChargeDifference));
                        }
                        break;

                    case XmlConstants.RMAirWayBill:
                        var rMAirWayBill = classObject as RMAirWayBill;
                        if (rMAirWayBill != null)
                        {
                            WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.WeightBilled, String.Format("{0:0.000}", rMAirWayBill.BilledWeightCharge));
                            WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.WeightAccepted, String.Format("{0:0.000}", rMAirWayBill.AcceptedWeightCharge));
                            WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.WeightDifference, String.Format("{0:0.000}", rMAirWayBill.WeightChargeDiff));
                            WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.ValuationBilled, String.Format("{0:0.000}", rMAirWayBill.BilledValuationCharge));
                            WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.ValuationAccepted, String.Format("{0:0.000}", rMAirWayBill.AcceptedValuationCharge));
                            WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.ValuationDifference, String.Format("{0:0.000}", rMAirWayBill.ValuationChargeDiff));
                        }
                        break;
                    case XmlConstants.BillingMemo:
                        var billingMemo = classObject as BillingMemo;
                        if (billingMemo != null)
                        {
                            if (billingMemo.BilledTotalWeightCharge != null)
                            {
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.WeightBilled, String.Format("{0:0.000}", billingMemo.BilledTotalWeightCharge));
                            }
                            if (billingMemo.BilledTotalValuationAmount != null)
                            {
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.ValuationBilled, String.Format("{0:0.000}", billingMemo.BilledTotalValuationAmount));
                            }
                        }
                        break;
                    case XmlConstants.BMAirWayBill:
                        var bMAirWayBill = classObject as BMAirWayBill;
                        if (bMAirWayBill != null)
                        {
                            if (bMAirWayBill.BilledWeightCharge != null)
                            {
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.WeightBilled, String.Format("{0:0.000}", bMAirWayBill.BilledWeightCharge));
                            }
                            if (bMAirWayBill.BilledValuationCharge != null)
                            {
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.ValuationBilled, String.Format("{0:0.000}", bMAirWayBill.BilledValuationCharge));
                            }
                        }
                        break;
                    case XmlConstants.CreditMemo:
                        var creditMemo = classObject as CreditMemo;
                        if (creditMemo != null)
                        {
                            if (creditMemo.TotalWeightCharges != null)
                            {
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.WeightBilled, String.Format("{0:0.000}", creditMemo.TotalWeightCharges));
                            }
                            if (creditMemo.TotalValuationAmt != null)
                            {
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.ValuationBilled, String.Format("{0:0.000}", creditMemo.TotalValuationAmt));
                            }
                        }
                        break;
                    case XmlConstants.CMAirWayBill:
                        var cMAirWayBill = classObject as CMAirWayBill;
                        if (cMAirWayBill != null)
                        {
                            if (cMAirWayBill.CreditedWeightCharge != 0)
                            {
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.WeightBilled, String.Format("{0:0.000}", cMAirWayBill.CreditedWeightCharge));
                            }
                            if (cMAirWayBill.CreditedValuationCharge != 0)
                            {
                                WriteAttributeNodeValue(xmlTextWriter, XmlConstants.ChargeAmount, XmlConstants.ValuationBilled, String.Format("{0:0.000}", cMAirWayBill.CreditedValuationCharge));
                            }
                        }
                        break;
                }
                //Logger.InfoFormat("End of WriteChargeAmount for {0}", classObject.GetType().Name);
            }
            catch (XmlException xmlException)
            {
                // clsLog.WriteLogAzure("Error Occurred in WriteChargeAmount for " + classObject.GetType().Name, xmlException);
                _staticLogger?.LogError("Error Occurred in WriteChargeAmount for {0} {1}", classObject.GetType().Name, xmlException);
            }
        }
    }
}