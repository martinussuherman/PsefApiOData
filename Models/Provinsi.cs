using System;
using System.Collections.Generic;

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
            Kabkota = new HashSet<Kabkota>();
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
        public virtual ICollection<Kabkota> Kabkota { get; set; }
    }
}
