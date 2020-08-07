using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PsefApi.Models
{
    /// <summary>
    /// Represents a Pemohon.
    /// </summary>
    public partial class Pemohon
    {
        /// <summary>
        /// Initializes a new instance of Pemohon.
        /// </summary>
        public Pemohon()
        {
            Permohonan = new HashSet<Permohonan>();
        }

        /// <summary>
        /// Gets or sets the unique identifier for the Pemohon.
        /// </summary>
        /// <value>The Pemohon's unique identifier.</value>
        public uint Id { get; set; }

        /// <summary>
        /// Gets or sets the associated user identifier.
        /// </summary>
        /// <value>The associated user identifier.</value>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the Pemohon phone number.
        /// </summary>
        /// <value>The Pemohon's phone number.</value>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the Pemohon address.
        /// </summary>
        /// <value>The Pemohon's address.</value>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the Pemohon NIB.
        /// </summary>
        /// <value>The Pemohon's NIB.</value>
        public string Nib { get; set; }

        /// <summary>
        /// Gets or sets the Pemohon apoteker name.
        /// </summary>
        /// <value>The Pemohon's apoteker name.</value>
        public string ApotekerName { get; set; }

        /// <summary>
        /// Gets or sets the Pemohon apoteker email.
        /// </summary>
        /// <value>The Pemohon's apoteker email.</value>
        public string ApotekerEmail { get; set; }

        /// <summary>
        /// Gets or sets the Pemohon apoteker phone number.
        /// </summary>
        /// <value>The Pemohon's apoteker phone number.</value>
        public string ApotekerPhone { get; set; }

        /// <summary>
        /// Gets or sets the Pemohon apoteker STRA number.
        /// </summary>
        /// <value>The Pemohon's apoteker STRA number.</value>
        public string StraNumber { get; set; }

        /// <summary>
        /// Gets or sets the Pemohon apoteker STRA expiry date.
        /// </summary>
        /// <value>The Pemohon's apoteker STRA expiry date.</value>
        public DateTime StraExpiry { get; set; }

        /// <summary>
        /// Gets or sets the Pemohon apoteker STRA document url.
        /// </summary>
        /// <value>The Pemohon's apoteker STRA document url.</value>
        public string StraUrl { get; set; }

        /// <summary>
        /// Gets or sets list of Permohonan associated with the Pemohon.
        /// </summary>
        /// <value>The associated list of Permohonan.</value>
        [IgnoreDataMember]
        public virtual ICollection<Permohonan> Permohonan { get; set; }
    }
}
