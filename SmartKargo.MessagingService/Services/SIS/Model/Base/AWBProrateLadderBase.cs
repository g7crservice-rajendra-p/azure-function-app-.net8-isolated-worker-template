namespace QidWorkerRole.SIS.Model.Base
{
    public class AWBProrateLadderBase : ModelBase
    {
        public string CurrencyofProrateCalculation { get; set; }

        public double? TotalAmount { get; set; }

        public string FromSector { get; set; }

        public string ToSector { get; set; }

        public string CarrierPrefix { get; set; }

        public string ProvisoReqSpa { get; set; }

        public long? ProrateFactor { get; set; }

        public double? PercentShare { get; set; }        

        public int? SequenceNumber { get; set; }

    }
}