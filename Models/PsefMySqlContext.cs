using Microsoft.EntityFrameworkCore;

namespace PsefApi.Model
{
    /// <summary>
    /// Database context.
    /// </summary>
    public partial class PsefMySqlContext : DbContext
    {
        /// <summary>
        /// Database context.
        /// </summary>
        /// <param name="options">context options.</param>
        public PsefMySqlContext(DbContextOptions<PsefMySqlContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Provinsi.
        /// </summary>
        /// <value>Provinsi.</value>
        public virtual DbSet<Provinsi> Provinsi { get; set; }

        /// <summary>
        /// Configure model.
        /// </summary>
        /// <param name="modelBuilder">Model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
