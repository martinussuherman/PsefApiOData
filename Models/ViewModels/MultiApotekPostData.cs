using System.Collections.Generic;

namespace PsefApiOData.Models
{
    /// <summary>
    /// Represents multiple Apotek HTTP Post data.
    /// </summary>
    public class MultiApotekPostData
    {
        /// <summary>
        /// Gets or sets the Permohonan unique identifier.
        /// </summary>
        /// <value>The Permohonan's unique identifier.</value>
        public uint PermohonanId { get; set; }

        /// <summary>
        /// Gets or sets list of Apotek to post.
        /// </summary>
        /// <value>The  list of Apotek to post.</value>
        public List<Apotek> Apotek { get; set; }
    }
}