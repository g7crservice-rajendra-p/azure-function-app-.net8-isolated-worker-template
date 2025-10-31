namespace SmartKargo.MessagingService.Data.Dao.Interfaces
{
    public interface ISqlDataHelperFactory
    {
        ISqlDataHelperDao Create(bool readOnly);
    }
}
