using Microsoft.EntityFrameworkCore;

namespace PsefApiOData.Models
{
    /// <summary>
    /// Database context
    /// </summary>
    public partial class PsefMySqlContext : DbContext
    {
        /// <summary>
        /// Database context
        /// </summary>
        /// <param name="options">Database context options</param>
        public PsefMySqlContext(DbContextOptions<PsefMySqlContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Apotek table
        /// </summary>
        /// <value>Apotek</value>
        public virtual DbSet<Apotek> Apotek { get; set; }

        /// <summary>
        /// Desa/Kelurahan table
        /// </summary>
        /// <value>Desa/Kelurahan</value>
        public virtual DbSet<DesaKelurahan> DesaKelurahan { get; set; }

        /// <summary>
        /// History Permohonan table
        /// </summary>
        /// <value>History Permohonan</value>
        public virtual DbSet<HistoryPermohonan> HistoryPermohonan { get; set; }

        /// <summary>
        /// Kabupaten/Kota table
        /// </summary>
        /// <value>Kabupaten/Kota</value>
        public virtual DbSet<KabupatenKota> KabupatenKota { get; set; }

        /// <summary>
        /// Kecamatan table
        /// </summary>
        /// <value>Kecamatan</value>
        public virtual DbSet<Kecamatan> Kecamatan { get; set; }

        /// <summary>
        /// Pemohon table
        /// </summary>
        /// <value>Pemohon</value>
        public virtual DbSet<Pemohon> Pemohon { get; set; }

        /// <summary>
        /// Permohonan table
        /// </summary>
        /// <value>Permohonan</value>
        public virtual DbSet<Permohonan> Permohonan { get; set; }

        /// <summary>
        /// Provinsi table
        /// </summary>
        /// <value>Provinsi</value>
        public virtual DbSet<Provinsi> Provinsi { get; set; }

        /// <summary>
        /// Configure model
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Apotek>(entity =>
            {
                entity.ToTable("apotek");

                entity.HasIndex(e => e.PermohonanId)
                    .HasName("FK_apotek_permohonan");

                entity.HasIndex(e => e.ProvinsiId)
                    .HasName("FK_apotek_provinsi");

                entity.Property(e => e.Id).HasColumnType("bigint(20) unsigned");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.ApotekerName)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.PermohonanId)
                    .HasColumnType("int(11) unsigned")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.ProvinsiId)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.SiaNumber)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.SipaNumber)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.StraNumber)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.HasOne(d => d.Permohonan)
                    .WithMany(p => p.Apotek)
                    .HasForeignKey(d => d.PermohonanId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_apotek_permohonan");

                entity.HasOne(d => d.Provinsi)
                    .WithMany(p => p.Apotek)
                    .HasForeignKey(d => d.ProvinsiId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_apotek_provinsi");
            });

            modelBuilder.Entity<DesaKelurahan>(entity =>
            {
                entity.ToTable("desakelurahan");

                entity.HasIndex(e => e.KecamatanId)
                    .HasName("FK_desakelurahan_kecamatan");

                entity.Property(e => e.Id).HasColumnType("int(10) unsigned");

                entity.Property(e => e.KecamatanId)
                    .HasColumnType("smallint(5) unsigned")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.HasOne(d => d.Kecamatan)
                    .WithMany(p => p.DesaKelurahan)
                    .HasForeignKey(d => d.KecamatanId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_desakelurahan_kecamatan");
            });

            modelBuilder.Entity<HistoryPermohonan>(entity =>
            {
                entity.ToTable("historypermohonan");

                entity.HasIndex(e => e.PermohonanId)
                    .HasName("FK_historypermohonan_permohonan");

                entity.Property(e => e.Id).HasColumnType("bigint(20) unsigned");

                entity.Property(e => e.PermohonanId)
                    .HasColumnType("int(10) unsigned")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.StatusId).HasColumnType("tinyint(3) unsigned");

                entity.Property(e => e.Reason)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'current_timestamp()'");

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.HasOne(d => d.Permohonan)
                    .WithMany(p => p.HistoryPermohonan)
                    .HasForeignKey(d => d.PermohonanId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_historypermohonan_permohonan");
            });

            modelBuilder.Entity<KabupatenKota>(entity =>
            {
                entity.ToTable("kabupatenkota");

                entity.HasIndex(e => e.ProvinsiId)
                    .HasName("FK_kabupatenkota_provinsi");

                entity.Property(e => e.Id).HasColumnType("smallint(5) unsigned");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.ProvinsiId)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasDefaultValueSql("'NULL'");

                entity.HasOne(d => d.Provinsi)
                    .WithMany(p => p.KabupatenKota)
                    .HasForeignKey(d => d.ProvinsiId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_kabupatenkota_provinsi");
            });

            modelBuilder.Entity<Kecamatan>(entity =>
            {
                entity.ToTable("kecamatan");

                entity.HasIndex(e => e.KabupatenKotaId)
                    .HasName("FK_kecamatan_kabupatenkota");

                entity.Property(e => e.Id).HasColumnType("smallint(5) unsigned");

                entity.Property(e => e.KabupatenKotaId)
                    .HasColumnType("smallint(5) unsigned")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.HasOne(d => d.KabupatenKota)
                    .WithMany(p => p.Kecamatan)
                    .HasForeignKey(d => d.KabupatenKotaId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_kecamatan_kabupatenkota");
            });

            modelBuilder.Entity<Pemohon>(entity =>
            {
                entity.ToTable("pemohon");

                entity.Property(e => e.Id).HasColumnType("int(10) unsigned");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.ApotekerEmail)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.ApotekerName)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.ApotekerPhone)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Nib)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.StraExpiry)
                    .HasColumnType("date")
                    .HasDefaultValueSql("'''0000-00-00'''");

                entity.Property(e => e.StraNumber)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.StraUrl)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<Permohonan>(entity =>
            {
                entity.ToTable("permohonan");

                entity.HasIndex(e => e.PemohonId)
                    .HasName("FK_permohonan_pemohon");

                entity.Property(e => e.Id).HasColumnType("int(10) unsigned");

                entity.Property(e => e.ApotekerEmail)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.ApotekerName)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.ApotekerPhone)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.DokumenApiUrl)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Domain)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.LastUpdate)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'current_timestamp()'")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.PemohonId)
                    .HasColumnType("int(10) unsigned")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.PermohonanNumber)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.PreviousPerizinanId)
                    .HasColumnType("int(10) unsigned")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.ProsesBisnisUrl)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.ProviderName)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.StatusId).HasColumnType("tinyint(3) unsigned");

                entity.Property(e => e.StraExpiry)
                    .HasColumnType("date")
                    .HasDefaultValueSql("'''0000-00-00'''");

                entity.Property(e => e.StraNumber)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.StraUrl)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.SuratPermohonanUrl)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.SystemName)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.TypeId).HasColumnType("tinyint(3) unsigned");

                entity.HasOne(d => d.Pemohon)
                    .WithMany(p => p.Permohonan)
                    .HasForeignKey(d => d.PemohonId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_permohonan_pemohon");
            });

            modelBuilder.Entity<Provinsi>(entity =>
            {
                entity.ToTable("provinsi");

                entity.Property(e => e.Id)
                    .HasColumnType("tinyint(3) unsigned")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
