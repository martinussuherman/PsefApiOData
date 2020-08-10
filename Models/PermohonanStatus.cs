using System.Collections.Generic;

namespace PsefApiOData.Models
{
    /// <summary>
    /// Represents a Permohonan Status.
    /// </summary>
    public partial class PermohonanStatus
    {

        /// <summary>
        /// Gets or sets the unique identifier for the Permohonan Status.
        /// </summary>
        /// <value>The Permohonan Status's unique identifier.</value>
        public byte Id { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan Status name.
        /// </summary>
        /// <value>The Permohonan Status's name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Permohonan Status name displayed to user.
        /// </summary>
        /// <value>The Permohonan Status's name displayed to user.</value>
        public string PemohonDisplayedName { get; set; }

        /// <summary>
        /// Gets list of predefined Permohonan Status.
        /// </summary>
        /// <value></value>
        public static List<PermohonanStatus> List => new List<PermohonanStatus>
        {
            new PermohonanStatus
            {
                Id=1,
                Name="Dibuat oleh Pemohon",
                PemohonDisplayedName="Dibuat"
            },
            new PermohonanStatus
            {
                Id=2,
                Name="Diajukan oleh Pemohon",
                PemohonDisplayedName="Diajukan"
            },
            new PermohonanStatus
            {
                Id=3,
                Name="Disetujui oleh Verifikator",
                PemohonDisplayedName="Dalam proses"
            },
            new PermohonanStatus
            {
                Id=4,
                Name="Dikembalikan oleh Verifikator",
                PemohonDisplayedName="Dalam proses"
            },
            new PermohonanStatus
            {
                Id=5,
                Name="Disetujui oleh Kepala Seksi",
                PemohonDisplayedName="Dalam proses"
            },
            new PermohonanStatus
            {
                Id=6,
                Name="Dikembalikan oleh Kepala Seksi",
                PemohonDisplayedName="Dalam proses"
            },
            new PermohonanStatus
            {
                Id=7,
                Name="Disetujui oleh Kepala Sub Direktorat",
                PemohonDisplayedName="Dalam proses"
            },
            new PermohonanStatus
            {
                Id=8,
                Name="Dikembalikan oleh Kepala Sub Direktorat",
                PemohonDisplayedName="Dalam proses"
            },
            new PermohonanStatus
            {
                Id=9,
                Name="Disetujui oleh Direktur Pelayanan Farmasi",
                PemohonDisplayedName="Dalam proses"
            },
            new PermohonanStatus
            {
                Id=10,
                Name="Dikembalikan oleh Direktur Pelayanan Farmasi",
                PemohonDisplayedName="Dalam proses"
            },
            new PermohonanStatus
            {
                Id=11,
                Name="Disetujui oleh Direktur Jenderal",
                PemohonDisplayedName="Dalam proses"
            },
            new PermohonanStatus
            {
                Id=12,
                Name="Dikembalikan oleh Direktur Jenderal",
                PemohonDisplayedName="Dalam proses"
            },
            new PermohonanStatus
            {
                Id=13,
                Name="Selesai",
                PemohonDisplayedName="Selesai"
            }
        };
    }
}