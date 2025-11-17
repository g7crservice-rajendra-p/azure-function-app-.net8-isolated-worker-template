using Microsoft.Extensions.Logging;

namespace QidWorkerRole
{
    public class CIMPMessageValidation
    {
        private readonly ILogger<CIMPMessageValidation> _logger;
        private readonly GenericFunction _genericFunction;

        public CIMPMessageValidation(
            ILogger<CIMPMessageValidation> logger, 
            GenericFunction genericFunction)
        {
            _logger = logger;
            _genericFunction = genericFunction;
        }

        /*Not in use currently*/
        //public async Task<bool> ValidateFFR(string ffrMessage, int srno, out string errorMessage)
        //{
        //    //GenericFunction genericFunction = new GenericFunction();
        //    errorMessage = string.Empty;
        //    try
        //    {
        //        string[] arrFFR = ffrMessage.Split('$');
        //        if (ffrMessage.Trim().StartsWith("FFR", StringComparison.OrdinalIgnoreCase))
        //        {
        //            #region : Validate Mandatory Lines :
        //            string lineIdentifier = string.Empty;
        //            string[] arrMandatoryLines = new string[] { "FFR", "REF" };

        //            for (int i = 0; i < arrFFR.Length; i++)
        //                lineIdentifier += "," + arrFFR[i].Split('/')[0].Split(' ')[0].Split('-')[0];

        //            string[] arrLineIdentifier = lineIdentifier.Trim(',').Split(',');
        //            string[] arrIdentifierDifference = arrMandatoryLines.Except(arrLineIdentifier).ToArray();
        //            if (arrIdentifierDifference.Length > 0)
        //            {
        //                errorMessage = arrIdentifierDifference[0] + " line is missing";
        //                await _genericFunction.UpdateErrorMessageToInbox(srno, errorMessage, "FFR", false, "", false);
        //                return false;
        //            }
        //            #endregion Validate Mandatory Lines
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // clsLog.WriteLogAzure(ex);
        //        _logger.LogError(ex, "Error on ValidateFFR");
        //    }
        //    return true;
        //}

        public async Task<(bool success,string errorMessage)> ValidateFWB(string fwbMessage, int srno, string errorMessage)
        {
            //GenericFunction genericFunction = new GenericFunction();
            errorMessage = string.Empty;
            try
            {
                string[] arrFWB = fwbMessage.Split('$');
                if (fwbMessage.Trim().StartsWith("FWB", StringComparison.OrdinalIgnoreCase))
                {
                    #region : Validate Mandatory Lines :
                    string lineIdentifier = string.Empty;
                    string[] arrMandatoryLines = new string[] { "FWB", "RTG", "SHP", "CNE", "CVD", "RTD", "ISU", "REF" };

                    for (int i = 0; i < arrFWB.Length; i++)
                        lineIdentifier += "," + arrFWB[i].Split('/')[0].Split(' ')[0].Split('-')[0];

                    string[] arrLineIdentifier = lineIdentifier.Trim(',').Split(',');
                    string[] arrIdentifierDifference = arrMandatoryLines.Except(arrLineIdentifier).ToArray();
                    if (arrIdentifierDifference.Length > 0)
                    {
                        errorMessage = arrIdentifierDifference[0] + " line is missing";
                        await _genericFunction.UpdateErrorMessageToInbox(srno, errorMessage, "FWB", false, "", false);
                        //return false;
                        return (false, errorMessage);
                    }
                    #endregion Validate Mandatory Lines

                    #region : Validate RTG Line :
                    //if (fltroute.Length > 0)
                    //{
                    //    for (int i = 0; i < fltroute.Length; i++)
                    //    {
                    //        if (fltroute[i].fltarrival == fwbdata.dest && (i + 1) < fltroute.Length)
                    //        {
                    //            errorMessage = "Route origin and destination mismatch with AWB";
                    //            genericFunction.UpdateErrorMessageToInbox(srno, errorMessage, "FWB", false, "", false);
                    //            break;
                    //        }
                    //    }
                    //}                    
                    #endregion
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on ValidateFWB");
            }
            //return true;
            return (true, errorMessage);
        }
    }
}
