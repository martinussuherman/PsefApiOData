namespace PsefApi.Models
{
    /// <summary>
    /// Represents a Kabupaten/Kota.
    /// </summary>
    public partial class Kabkota
    {
        /// <summary>
        /// Gets or sets the unique identifier for the Kabupaten/Kota.
        /// </summary>
        /// <value>The Kabupaten/Kota's unique identifier.</value>
        public ushort Id { get; set; }

        /// <summary>
        /// Gets or sets the Kabupaten/Kota name.
        /// </summary>
        /// <value>The Kabupaten/Kota's name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the associated Provinsi identifier.
        /// </summary>
        /// <value>The associated Provinsi identifier.</value>
        public byte? ProvinsiId { get; set; }

        /// <summary>
        /// Gets or sets Provinsi associated with the Kabupaten/Kota.
        /// </summary>
        /// <value>The associated Provinsi.</value>
        public virtual Provinsi Provinsi { get; set; }
    }
}
