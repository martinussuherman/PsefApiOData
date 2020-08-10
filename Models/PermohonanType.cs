using System.Collections.Generic;

namespace PsefApiOData.Models
{
    /// <summary>
    /// Represents a Permohonan Type.
    /// </summary>
    public partial class PermohonanType
    {

        /// <summary>
        /// Gets or sets the unique identifier for the Permohonan Type.
        /// </summary>
        /// <value>The Permohonan Type's unique identifier.</value>
        public byte Id { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan Type name.
        /// </summary>
        /// <value>The Permohonan Type's name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets list of predefined Permohonan Type.
        /// </summary>
        /// <value></value>
        public static List<PermohonanType> List => new List<PermohonanType>
        {
            new PermohonanType
            {
                Id=1,
                Name="Baru"
            },
            new PermohonanType
            {
                Id=2,
                Name="Perpanjangan"
            }
        };
    }
}