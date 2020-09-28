using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace PsefApiOData.Models
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
            Apotek = new HashSet<Apotek>();
            HistoryPermohonan = new HashSet<HistoryPermohonan>();
            Perizinan = new HashSet<Perizinan>();
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
        /// Gets or sets the associated Perizinan identifier.
        /// </summary>
        /// <value>The associated Perizinan identifier.</value>
        public uint? PerizinanId { get; set; }

        /// <summary>
        /// Gets or sets the associated Permohonan Status identifier.
        /// </summary>
        /// <value>The associated Permohonan Status identifier.</value>
        public byte StatusId { get; set; }

        /// <summary>
        /// (Read Only) Gets the associated Permohonan Status name.
        /// </summary>
        /// <value>The associated Permohonan Status name.</value>
        [NotMapped]
        public string StatusName
        {
            get => PermohonanStatus.List.Find(e => e.Id == StatusId).Name;
            set
            {
            }
        }

        /// <summary>
        /// (Read Only) Gets the associated Permohonan Status name for Pemohon.
        /// </summary>
        /// <value>The associated Permohonan Status name for Pemohon.</value>
        [NotMapped]
        public string PemohonStatusName
        {
            get => PermohonanStatus.List.Find(e => e.Id == StatusId).PemohonDisplayedName;
            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the associated Permohonan Type identifier.
        /// </summary>
        /// <value>The associated Permohonan Type identifier.</value>
        public byte TypeId { get; set; }

        /// <summary>
        /// (Read Only) Gets the associated Permohonan Type name.
        /// </summary>
        /// <value>The associated Permohonan Type name.</value>
        [NotMapped]
        public string TypeName
        {
            get => PermohonanType.List.Find(e => e.Id == TypeId).Name;
            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the Permohonan number.
        /// </summary>
        /// <value>The Permohonan's number.</value>
        public string PermohonanNumber { get; set; }

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
        /// Gets or sets the Permohonan apoteker name.
        /// </summary>
        /// <value>The Permohonan's apoteker name.</value>
        public string ApotekerName { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan apoteker email.
        /// </summary>
        /// <value>The Permohonan's apoteker email.</value>
        public string ApotekerEmail { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan apoteker phone number.
        /// </summary>
        /// <value>The Permohonan's apoteker phone number.</value>
        public string ApotekerPhone { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan apoteker NIK.
        /// </summary>
        /// <value>The Permohonan's apoteker NIK.</value>
        public string ApotekerNik { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan apoteker STRA number.
        /// </summary>
        /// <value>The Permohonan's apoteker STRA number.</value>
        public string StraNumber { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan apoteker STRA expiry date.
        /// </summary>
        /// <value>The Permohonan's apoteker STRA expiry date.</value>
        public DateTime StraExpiry { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan apoteker STRA document url.
        /// </summary>
        /// <value>The Permohonan's apoteker STRA document url.</value>
        public string StraUrl { get; set; }

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
        /// Gets or sets the Permohonan Dokumen PSE Kominfo document url.
        /// </summary>
        /// <value>The Permohonan's Dokumen PSE Kominfo document url.</value>
        public string DokumenPseUrl { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan last update.
        /// </summary>
        /// <value>The Permohonan's last update.</value>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// Gets or sets Pemohon associated with the Permohonan.
        /// </summary>
        /// <value>The associated Pemohon.</value>
        [IgnoreDataMember]
        public virtual Pemohon Pemohon { get; set; }

        /// <summary>
        /// Gets or sets Perizinan associated with the Permohonan.
        /// </summary>
        /// <value>The associated Perizinan.</value>
        [IgnoreDataMember]
        public virtual Perizinan PerizinanNavigation { get; set; }

        /// <summary>
        /// Gets or sets Previous Perizinan associated with the Permohonan.
        /// </summary>
        /// <value>The associated Previous Perizinan.</value>
        [IgnoreDataMember]
        public virtual Perizinan PreviousPerizinan { get; set; }

        /// <summary>
        /// Gets or sets list of Apotek associated with the Permohonan.
        /// </summary>
        /// <value>The associated list of Apotek.</value>
        [IgnoreDataMember]
        public virtual ICollection<Apotek> Apotek { get; set; }

        /// <summary>
        /// Gets or sets list of History Permohonan associated with the Permohonan.
        /// </summary>
        /// <value>The associated list of History Permohonan.</value>
        [IgnoreDataMember]
        public virtual ICollection<HistoryPermohonan> HistoryPermohonan { get; set; }

        /// <summary>
        /// Gets or sets list of Perizinan associated with the Permohonan.
        /// </summary>
        /// <value>The associated list of Perizinan.</value>
        [IgnoreDataMember]
        public virtual ICollection<Perizinan> Perizinan { get; set; }
    }
}
