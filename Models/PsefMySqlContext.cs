﻿using Microsoft.EntityFrameworkCore;

namespace PsefApi.Models
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
        /// Provinsi table
        /// </summary>
        /// <value>Provinsi</value>
        public virtual DbSet<Provinsi> Provinsi { get; set; }

        /// <summary>
        /// Kabupaten/kota table
        /// </summary>
        /// <value>Kabupaten/kota</value>
        public virtual DbSet<Kabkota> Kabkota { get; set; }

        /// <summary>
        /// Pemohon table
        /// </summary>
        /// <value>Pemohon</value>
        public virtual DbSet<Pemohon> Pemohon { get; set; }

        /// <summary>
        /// Configure model
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Kabkota>(entity =>
            {
                entity.ToTable("kabkota");

                entity.HasIndex(e => e.ProvinsiId)
                    .HasName("FK_kabkota_provinsi");

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
                    .WithMany(p => p.Kabkota)
                    .HasForeignKey(d => d.ProvinsiId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_kabkota_provinsi");
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

                entity.Property(e => e.StraNumber)
                    .IsRequired()
                    .HasColumnType("tinytext")
                    .HasDefaultValueSql("''''''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.StraUrl)
                    .IsRequired()
                    .HasColumnType("tinytext")
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
