using FileHelpers;

namespace QidWorkerRole.SIS.FileHandling.Idec.Read
{
    /// <summary>
    /// To Convert Records into Classes.
    /// </summary>
    /// <typeparam name="T">Type of the Record Passed.</typeparam>  
    public interface IRecordToClassConverter<T> where T : class, new()
    {
        /// <summary>
        /// To Convert Records into Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <returns>Object of Record Passed.</returns>
        T ConvertRecordToClass(MultiRecordEngine multiRecordEngine);

        /// <summary>
        /// To Convert Childs of Perent Records into there corresponding Classes.
        /// </summary>
        /// <param name="multiRecordEngine">MultiRecordEngine</param>
        /// <param name="parent">Parent Record of the Child Records.</param>
        void ProcessNextRecord(MultiRecordEngine multiRecordEngine, T parent);
    }
}