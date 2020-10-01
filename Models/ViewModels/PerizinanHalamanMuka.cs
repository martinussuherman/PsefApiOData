using System.Runtime.Serialization;

namespace PsefApiOData.Models
{
    /// <summary>
    /// Represents a Perizinan Halaman Muka information.
    /// </summary>
    public class PerizinanHalamanMuka
    {
        /// <summary>
        /// Gets or sets the Perizinan number.
        /// </summary>
        /// <value>The Perizinan's number.</value>
        public string PerizinanNumber { get; set; }

        /// <summary>
        /// Gets or sets the Perizinan company name.
        /// </summary>
        /// <value>The Perizinan's company name.</value>
        public string CompanyName { get; set; }
    }
}