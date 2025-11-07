using System.Xml;
using System.Collections;
using QidWorkerRole.SIS.Model;
using QidWorkerRole.SIS.FileHandling.Xml.Read.SupportingModels;

namespace QidWorkerRole.SIS.FileHandling.Xml.Read.ReadHelpers
{
    public sealed partial class XmlReaderHelper
    {
        /// <summary>
        /// Read Other Charges for AirWayBill, BMAWB, CMAWB, RMAWB.
        /// </summary>
        /// <param name="classObject">classObject</param>
        /// <param name="otherChargesArrayList">otherChargesArrayList</param>
        /// <param name="totalOtherCharge">totalOtherCharge</param>
        private static double ReadOtherChargeDetails(object classObject, ArrayList otherChargesArrayList, double totalOtherCharge)
        {
            try
            {
                //Logger.Info("Start of Read ReadOtherChargeDetails.");

                AWBOtherCharges aWBOtherCharges;
                RMAWBOtherCharges rMAWBOtherCharges;
                BMAWBOtherCharges bMAWBOtherCharges;
                CMAWBOtherCharges cMAWBOtherCharges;

                foreach (object addOnCharge in otherChargesArrayList)
                {
                    switch (classObject.GetType().Name)
                    {
                        case XmlConstants.AirWayBill:

                            aWBOtherCharges = new AWBOtherCharges();

                            aWBOtherCharges.OtherChargeCode = ((OtherChargAmountDetails)addOnCharge).OtherChargeCode;
                            aWBOtherCharges.OtherChargeCodeValue = ((OtherChargAmountDetails)addOnCharge).OtherChargeCodeValue;
                            aWBOtherCharges.OtherChargeVatBaseAmount = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatBaseAmount;
                            aWBOtherCharges.OtherChargeVatCalculatedAmount = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatCalculatedAmount;
                            aWBOtherCharges.OtherChargeVatLabel = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatLabel;
                            aWBOtherCharges.OtherChargeVatPercentage = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatPercentage;
                            aWBOtherCharges.OtherChargeVatText = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatText;

                            var otherChargeCodeValue = ((OtherChargAmountDetails) addOnCharge).OtherChargeCodeValue;
                            if (otherChargeCodeValue != null)
                                totalOtherCharge +=
                                    (double) otherChargeCodeValue;

                            ((AirWayBill)classObject).AWBOtherChargesList.Add(aWBOtherCharges);
                            break;

                        case XmlConstants.RMAirWayBill:

                            rMAWBOtherCharges = new RMAWBOtherCharges();
                            rMAWBOtherCharges.OtherChargeCode = ((OtherChargAmountDetails)addOnCharge).OtherChargeCode;
                            rMAWBOtherCharges.OtherChargeCodeValue = ((OtherChargAmountDetails)addOnCharge).OtherChargeCodeValue;
                            rMAWBOtherCharges.OtherChargeVatBaseAmount = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatBaseAmount;
                            rMAWBOtherCharges.OtherChargeVatCalculatedAmount = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatCalculatedAmount;
                            rMAWBOtherCharges.OtherChargeVatLabel = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatLabel;
                            rMAWBOtherCharges.OtherChargeVatPercentage = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatPercentage;
                            rMAWBOtherCharges.OtherChargeVatText = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatText;

                            var chargeCodeValue = ((OtherChargAmountDetails) addOnCharge).OtherChargeCodeValue;
                            if (chargeCodeValue != null)
                                totalOtherCharge +=
                                    (double) chargeCodeValue;

                            ((RMAirWayBill)classObject).RMAWBOtherChargesList.Add(rMAWBOtherCharges);

                            break;

                        case XmlConstants.BMAirWayBill:

                            bMAWBOtherCharges = new BMAWBOtherCharges();

                            bMAWBOtherCharges.OtherChargeCode = ((OtherChargAmountDetails)addOnCharge).OtherChargeCode;
                            bMAWBOtherCharges.OtherChargeCodeValue = ((OtherChargAmountDetails)addOnCharge).OtherChargeCodeValue;
                            bMAWBOtherCharges.OtherChargeVatBaseAmount = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatBaseAmount;
                            bMAWBOtherCharges.OtherChargeVatCalculatedAmount = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatCalculatedAmount;
                            bMAWBOtherCharges.OtherChargeVatLabel = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatLabel;
                            bMAWBOtherCharges.OtherChargeVatPercentage = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatPercentage;
                            bMAWBOtherCharges.OtherChargeVatText = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatText;

                            var codeValue = ((OtherChargAmountDetails) addOnCharge).OtherChargeCodeValue;
                            if (codeValue != null)
                                totalOtherCharge +=
                                    (double) codeValue;

                            ((BMAirWayBill)classObject).BMAWBOtherChargesList.Add(bMAWBOtherCharges);
                            break;

                        case XmlConstants.CMAirWayBill:

                            cMAWBOtherCharges = new CMAWBOtherCharges();

                            cMAWBOtherCharges.OtherChargeCode = ((OtherChargAmountDetails)addOnCharge).OtherChargeCode;
                            cMAWBOtherCharges.OtherChargeCodeValue = ((OtherChargAmountDetails)addOnCharge).OtherChargeCodeValue;
                            cMAWBOtherCharges.OtherChargeVatBaseAmount = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatBaseAmount;
                            cMAWBOtherCharges.OtherChargeVatCalculatedAmount = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatCalculatedAmount;
                            cMAWBOtherCharges.OtherChargeVatLabel = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatLabel;
                            cMAWBOtherCharges.OtherChargeVatPercentage = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatPercentage;
                            cMAWBOtherCharges.OtherChargeVatText = ((OtherChargAmountDetails)addOnCharge).OtherChargeVatText;

                            var value = ((OtherChargAmountDetails) addOnCharge).OtherChargeCodeValue;
                            if (value != null)
                                totalOtherCharge +=
                                    (double) value;

                            ((CMAirWayBill)classObject).CMAWBOtherChargesList.Add(cMAWBOtherCharges);
                            break;
                    }
                }
                //Logger.Info("End of Read ReadOtherChargeDetails.");
                return totalOtherCharge;                
            }
            catch (XmlException xmlException)
            {
                clsLog.WriteLogAzure("Error Occurred in ReadOtherChargeDetails: ", xmlException);
                return 0;
            }
        }
    }
}
