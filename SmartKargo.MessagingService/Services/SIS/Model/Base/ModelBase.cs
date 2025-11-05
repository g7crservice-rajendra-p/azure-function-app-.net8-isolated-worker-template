using System;

namespace QidWorkerRole.SIS.Model.Base
{
    /// <summary>
    /// Class that acts as an abstract root for all the model classes.
    /// </summary>
    [Serializable]
    public abstract class ModelBase
    {
        /// <summary>
        /// Holds the Name of the Record Creator.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Holds the Entity Creation UTC Date and Time.
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Holds the Name of the Record Updator.
        /// </summary>
        public string LastUpdatedBy { get; set; }

        /// <summary>
        /// Holds the Entity Modification UTC Date and Time.
        /// </summary>
        public DateTime LastUpdatedOn { get; set; }
    }
}