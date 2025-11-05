using FileHelpers;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base
{
  [FixedLengthRecord]
  public abstract class VatRecordBase : InvoiceRecordBase
  {
    public VatRecordBase() { }

    public VatRecordBase(InvoiceRecordBase baseRecord)
      : base(baseRecord)
    {

    }
  }
}
