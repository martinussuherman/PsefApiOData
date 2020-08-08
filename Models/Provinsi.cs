using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PsefApiOData.Models
{
    /// <summary>
    /// Represents a Provinsi.
    /// </summary>
    public partial class Provinsi
    {
        /// <summary>
        /// Initializes a new instance of Provinsi.
        /// </summary>
        public Provinsi()
        {
            Apotek = new HashSet<Apotek>();
            KabupatenKota = new HashSet<KabupatenKota>();
        }

        /// <summary>
        /// Gets or sets the unique identifier for the Provinsi.
        /// </summary>
        /// <value>The Provinsi's unique identifier.</value>
        public byte Id { get; set; }

        /// <summary>
        /// Gets or sets the Provinsi name.
        /// </summary>
        /// <value>The Provinsi's name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets list of Apotek associated with the Provinsi.
        /// </summary>
        /// <value>The associated list of Apotek.</value>
        [IgnoreDataMember]
        public virtual ICollection<Apotek> Apotek { get; set; }

        /// <summary>
        /// Gets or sets list of Kabupaten/Kota associated with the Provinsi.
        /// </summary>
        /// <value>The associated list of Kabupaten/Kota.</value>
        [IgnoreDataMember]
        public virtual ICollection<KabupatenKota> KabupatenKota { get; set; }
    }
}
