using System.Collections.Generic;

namespace QidWorkerRole.SIS.Model.SupportingModels
{
    public class CMProrateSlipDetails
    {
        public string ProrateCalCurrencyId { get; set; }

        public double TotalProrateAmount { get; set; }

        public List<CMAWBProrateLadder> CMAWBProrateLadderList { get; set; }

        public CMProrateSlipDetails()
        {
            CMAWBProrateLadderList = new List<CMAWBProrateLadder>();
        }
    }
}