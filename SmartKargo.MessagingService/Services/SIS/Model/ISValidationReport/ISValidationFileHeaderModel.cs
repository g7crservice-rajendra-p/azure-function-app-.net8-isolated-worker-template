using QidWorkerRole.SIS.Model.Base;

namespace QidWorkerRole.SIS.Model.ISValidationReport
{
    public class ISValidationFileHeaderModel : ModelBase
    {
        public int ID { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string LogFilePath { get; set; }
        public int ReceivablesFileHeaderID { get; set; }
    }
}