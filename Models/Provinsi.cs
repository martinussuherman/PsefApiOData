using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PsefApi.Models
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
            KabKota = new HashSet<KabKota>();
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
        /// Gets or sets Kabupaten/Kota associated with the Provinsi.
        /// </summary>
        /// <value>The collection of associated Kabupaten/Kota.</value>
        [IgnoreDataMember]
        public virtual ICollection<KabKota> KabKota { get; set; }
    }
}
