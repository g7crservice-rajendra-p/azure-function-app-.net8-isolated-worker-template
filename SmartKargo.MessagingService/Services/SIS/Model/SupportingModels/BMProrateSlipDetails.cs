using System.Collections.Generic;

namespace QidWorkerRole.SIS.Model.SupportingModels
{
    public class BMProrateSlipDetails
    {
        public string ProrateCalCurrencyId { get; set; }

        public double TotalProrateAmount { get; set; }

        public List<BMAWBProrateLadder> BMAWBProrateLadderList { get; set; }

        public BMProrateSlipDetails()
        {
            BMAWBProrateLadderList = new List<BMAWBProrateLadder>();
        }
    }
}