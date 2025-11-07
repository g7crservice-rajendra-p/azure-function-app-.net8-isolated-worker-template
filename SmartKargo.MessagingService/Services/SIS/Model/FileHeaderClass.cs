using QidWorkerRole.SIS.Model.Base;
using System;

namespace QidWorkerRole.SIS.Model
{
    /// <summary>
    /// Class to represent FileHeader
    /// </summary>
    public class FileHeaderClass : ModelBase
    {
        /// <summary>
        /// File Header Id
        /// </summary>
        public int FileHeaderID { get; set; }

        /// <summary>
        /// Airline Numeric Code.
        /// In case of Input file to SIS, the Airline Code provided should be same as the Billing Airline Code data in the file
        /// In case of Output file from SIS, the Airline Code will be same as the Billed Airline Code data in the file
        /// </summary>
        public string AirlineCode { get; set; }

        /// <summary>
        /// File Version Number
        /// </summary>
        public decimal? VersionNumber { get; set; }

        /// <summary>
        /// File Name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// File Direction i.e Send/Received to/from SIS.
        /// 0: Received from SIS, 1: Sent to SIS.
        /// </summary>
        public int? FileInOutDirection { get; set; }

        /// <summary>
        /// File Path (Blob URL)
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Log File Path: Initially it will be local, blob path after uploading.
        /// </summary>
        public string LogFilePath { get; set; }

        /// <summary>
        /// File Status Id
        /// </summary>
        public byte FileStatusId { get; set; }

        public DateTime ReadWriteOnSFTP { get; set; }

        public int? IsProcessed { get; set; }

    }
}