using System.Runtime.Serialization;

namespace PsefApi.Models
{
    /// <summary>
    /// Represents a Permohonan.
    /// </summary>
    public partial class Permohonan
    {
        /// <summary>
        /// Initializes a new instance of Permohonan.
        /// </summary>
        public Permohonan()
        {
            // Historypermohonan = new HashSet<Historypermohonan>();
        }

        /// <summary>
        /// Gets or sets the unique identifier for the Permohonan.
        /// </summary>
        /// <value>The Permohonan's unique identifier.</value>
        public uint Id { get; set; }

        /// <summary>
        /// Gets or sets the associated Pemohon identifier.
        /// </summary>
        /// <value>The associated Pemohon identifier.</value>
        public uint? PemohonId { get; set; }

        /// <summary>
        /// Gets or sets the associated previous Perizinan identifier.
        /// </summary>
        /// <value>The associated previous Perizinan identifier.</value>
        public uint? PreviousPerizinanId { get; set; }

        /// <summary>
        /// Gets or sets the associated Status Permohonan identifier.
        /// </summary>
        /// <value>The associated Status Permohonan identifier.</value>
        public byte StatusId { get; set; }

        /// <summary>
        /// Gets or sets the associated Type Permohonan identifier.
        /// </summary>
        /// <value>The associated Type Permohonan identifier.</value>
        public byte TypeId { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan number.
        /// </summary>
        /// <value>The Permohonan's number.</value>
        public string NomorPermohonan { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan domain.
        /// </summary>
        /// <value>The Permohonan's domain.</value>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan system name.
        /// </summary>
        /// <value>The Permohonan's system name.</value>
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan provider name.
        /// </summary>
        /// <value>The Permohonan's provider name.</value>
        public string ProviderName { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan Surat Permohonan document url.
        /// </summary>
        /// <value>The Permohonan's Surat Permohonan document url.</value>
        public string SuratPermohonanUrl { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan Proses Bisnis document url.
        /// </summary>
        /// <value>The Permohonan's Proses Bisnis document url.</value>
        public string ProsesBisnisUrl { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan Dokumen Api document url.
        /// </summary>
        /// <value>The Permohonan's Dokumen Api document url.</value>
        public string DokumenApiUrl { get; set; }

        /// <summary>
        /// Gets or sets Pemohon associated with the Permohonan.
        /// </summary>
        /// <value>The associated Pemohon.</value>
        [IgnoreDataMember]
        public virtual Pemohon Pemohon { get; set; }

        // [IgnoreDataMember]
        // public virtual ICollection<Historypermohonan> Historypermohonan { get; set; }
    }
}
