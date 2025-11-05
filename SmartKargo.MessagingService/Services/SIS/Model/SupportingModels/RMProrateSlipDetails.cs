using System.Collections.Generic;

namespace QidWorkerRole.SIS.Model.SupportingModels
{
    public class RMProrateSlipDetails
    {
        public string ProrateCalCurrencyId { get; set; }

        public double TotalProrateAmount { get; set; }

        public List<RMAWBProrateLadder> RMAWBProrateLadderList { get; set; }

        public RMProrateSlipDetails()
        {
            RMAWBProrateLadderList = new List<RMAWBProrateLadder>();
        }
    }
}