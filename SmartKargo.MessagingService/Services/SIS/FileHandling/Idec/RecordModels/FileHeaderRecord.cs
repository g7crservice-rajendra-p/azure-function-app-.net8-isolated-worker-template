using FileHelpers;
using QidWorkerRole.SIS.FileHandling.Idec.CustomConverters;
using QidWorkerRole.SIS.FileHandling.Idec.RecordModels.Base;
using QidWorkerRole.SIS.FileHandling.Idec.Write;
using QidWorkerRole.SIS.Model;

namespace QidWorkerRole.SIS.FileHandling.Idec.RecordModels
{
    [FixedLengthRecord]
    internal class FileHeaderRecord : RecordBase, IClassToRecordConverter<FileHeaderClass>
    {
        
        #region Record Properties

        [FieldFixedLength(4), FieldConverter(typeof(PaddingConverter), 4, IdecConstants.PaddingCharacterZero, IdecConstants.RJZF_PaddingConverter)]
        public string AirlineCode = string.Empty;

        [FieldFixedLength(4)]
        public string VersionNumber = string.Empty;

        [FieldFixedLength(479)]
        [FieldTrim(TrimMode.Right)]
        public string Filler = string.Empty;

        #endregion

        #region Parameterless Constructor

        public FileHeaderRecord()
        {
            StandardMessageIdentifier = IdecConstants.StandardMessageIdentifier;
            StandardFieldIdentifier = IdecConstants.SfiFileHeader;
            VersionNumber = IdecConstants.FileHeaderRecordVersionNo;
        }

        #endregion

        #region Implementation of IClassToRecordConverter<AWBRecord> To Write IDEC File.

        /// <summary>
        /// This method creats the File Header Class into FileHeaderRecord.
        /// </summary>
        /// <param name="fileHeader">FileHeaderClass</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ConvertClassToRecord(FileHeaderClass fileHeader, ref long recordSequenceNumber)
        {
            //Logger.Info("Start of Converting File Header Class into FileHeaderRecord.");

            RecordSequenceNumber = recordSequenceNumber++.ToString();
            AirlineCode = fileHeader.AirlineCode != null ? FileHandling.Idec.Utilities.GetNumericMemberCode(fileHeader.AirlineCode).ToString() : string.Empty; 

            ProcessNextClass(fileHeader, ref recordSequenceNumber);

            //Logger.Info("End of Converting File Header Class into FileHeaderRecord.");
        }

        /// <summary>
        /// To implement the interface.
        /// </summary>
        /// <param name="fileHeader">FileHeaderClass</param>
        /// <param name="recordSequenceNumber">recordSequenceNumber</param>
        public void ProcessNextClass(FileHeaderClass fileHeader, ref long recordSequenceNumber)
        {
            //Logger.Info("FileHeaderClass does not have direct childs.");
        }

        #endregion

    }
}
