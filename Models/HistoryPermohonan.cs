using System;
using System.Runtime.Serialization;

namespace PsefApiOData.Models
{
    /// <summary>
    /// Represents a History Permohonan.
    /// </summary>
    public partial class HistoryPermohonan
    {
        /// <summary>
        /// Gets or sets the unique identifier for the History Permohonan.
        /// </summary>
        /// <value>The History Permohonan's unique identifier.</value>
        public ulong Id { get; set; }

        /// <summary>
        /// Gets or sets the associated Permohonan identifier.
        /// </summary>
        /// <value>The associated Permohonan identifier.</value>
        public uint? PermohonanId { get; set; }

        /// <summary>
        /// Gets or sets the associated Status Permohonan identifier.
        /// </summary>
        /// <value>The associated Status Permohonan identifier.</value>
        public byte StatusId { get; set; }

        /// <summary>
        /// Gets or sets the History Permohonan updated date and time.
        /// </summary>
        /// <value>The History Permohonan's updated date and time.</value>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the History Permohonan updated by.
        /// </summary>
        /// <value>The History Permohonan's updated by.</value>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets Permohonan associated with the History Permohonan.
        /// </summary>
        /// <value>The associated Permohonan.</value>
        [IgnoreDataMember]
        public virtual Permohonan Permohonan { get; set; }
    }
}
