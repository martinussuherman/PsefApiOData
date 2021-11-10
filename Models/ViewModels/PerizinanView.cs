using System;

namespace PsefApiOData.Models
{
    /// <summary>
    /// Represents a Perizinan View information.
    /// </summary>
    public class PerizinanView
    {
        /// <summary>
        /// Gets or sets the unique identifier for the Perizinan.
        /// </summary>
        /// <value>The Perizinan's unique identifier.</value>
        public uint Id { get; set; }

        /// <summary>
        /// Gets or sets the Perizinan number.
        /// </summary>
        /// <value>The Perizinan's number.</value>
        public string PerizinanNumber { get; set; }

        /// <summary>
        /// Gets or sets the Perizinan issued date.
        /// </summary>
        /// <value>The Perizinan's issued date.</value>
        public DateTime IssuedAt { get; set; }

        /// <summary>
        /// Gets or sets the Perizinan expired date.
        /// </summary>
        /// <value>The Perizinan's expired date.</value>
        public DateTime ExpiredAt { get; set; }

        /// <summary>
        /// Gets or sets the Perizinan Tanda Daftar document url.
        /// </summary>
        /// <value>The Perizinan's Tanda Daftar document url.</value>
        public string TandaDaftarUrl { get; set; }

        /// <summary>
        /// Gets or sets the Perizinan company name.
        /// </summary>
        /// <value>The Perizinan's company name.</value>
        public string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the Perizinan domain.
        /// </summary>
        /// <value>The Perizinan's domain.</value>
        public string Domain { get; set; }
    }
}