namespace QidWorkerRole.SIS.FileHandling.Idec.Write
{
    /// <summary>
    /// To Convert Classes into Records.
    /// </summary>
    /// <typeparam name="T">Type of the Class Passed.</typeparam>  
    public interface IClassToRecordConverter<T> where T : class, new()
    {
        /// <summary>
        /// To Convert Classes into Records.
        /// </summary>
        /// <param name="parent">Instance of the Parent Class.</param>
        /// <param name="recordSequenceNumber">Record Sequence Number.</param>
        void ConvertClassToRecord(T parent, ref long recordSequenceNumber);

        /// <summary>
        /// To Convert Childs of Perent Classes into there corresponding Records.
        /// </summary>
        /// <param name="parent">Instance of the Parent Class.</param>
        /// <param name="recordSequenceNumber">Record Sequence Number.</param>
        void ProcessNextClass(T parent, ref long recordSequenceNumber);
    }
}
