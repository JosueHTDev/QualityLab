using Microsoft.EntityFrameworkCore;
using QualityLab.API.Models.Entities;
using QualityLab.API.Models.Enums;

namespace QualityLab.API.Data
{
    public class QualityLabDbContext : DbContext
    {
        public QualityLabDbContext(DbContextOptions<QualityLabDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Tecnico> Tecnicos => Set<Tecnico>();
        public DbSet<Lote> Lotes => Set<Lote>();
        public DbSet<Muestra> Muestras => Set<Muestra>();
        public DbSet<Analisis> Analisis => Set<Analisis>();
        public DbSet<Resultado> Resultados => Set<Resultado>();
        public DbSet<Certificado> Certificados => Set<Certificado>();
        public DbSet<Incidencia> Incidencias => Set<Incidencia>();
        public DbSet<AvanceMuestra> Avances => Set<AvanceMuestra>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- Usuario ----
            modelBuilder.Entity<Usuario>(e =>
            {
                e.HasIndex(u => u.NombreUsuario).IsUnique();
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Rol).HasConversion<string>().HasMaxLength(20);

                e.HasOne(u => u.Cliente)
                    .WithMany()
                    .HasForeignKey(u => u.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(u => u.Tecnico)
                    .WithMany()
                    .HasForeignKey(u => u.TecnicoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---- Cliente ----
            modelBuilder.Entity<Cliente>(e =>
            {
                e.HasIndex(c => c.RUC).IsUnique();
            });

            // ---- Lote ----
            modelBuilder.Entity<Lote>(e =>
            {
                e.Property(l => l.Estado).HasConversion<string>().HasMaxLength(20);
                e.HasOne(l => l.Cliente)
                    .WithMany(c => c.Lotes)
                    .HasForeignKey(l => l.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---- Muestra ----
            modelBuilder.Entity<Muestra>(e =>
            {
                e.HasIndex(m => m.Codigo).IsUnique();
                e.Property(m => m.Estado).HasConversion<string>().HasMaxLength(20);

                e.HasOne(m => m.Lote)
                    .WithMany(l => l.Muestras)
                    .HasForeignKey(m => m.LoteId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(m => m.Cliente)
                    .WithMany(c => c.Muestras)
                    .HasForeignKey(m => m.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(m => m.TecnicoAsignado)
                    .WithMany(t => t.MuestrasAsignadas)
                    .HasForeignKey(m => m.TecnicoAsignadoId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(m => m.Certificado)
                    .WithOne(c => c.Muestra!)
                    .HasForeignKey<Certificado>(c => c.MuestraId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---- Analisis ----
            modelBuilder.Entity<Analisis>(e =>
            {
                e.Property(a => a.Estado).HasConversion<string>().HasMaxLength(20);

                e.HasOne(a => a.Muestra)
                    .WithMany(m => m.Analisis)
                    .HasForeignKey(a => a.MuestraId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(a => a.Tecnico)
                    .WithMany(t => t.Analisis)
                    .HasForeignKey(a => a.TecnicoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---- Resultado ----
            modelBuilder.Entity<Resultado>(e =>
            {
                e.HasOne(r => r.Analisis)
                    .WithMany(a => a.Resultados)
                    .HasForeignKey(r => r.AnalisisId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(r => r.Tecnico)
                    .WithMany()
                    .HasForeignKey(r => r.TecnicoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---- Certificado ----
            modelBuilder.Entity<Certificado>(e =>
            {
                e.HasIndex(c => c.CodigoCertificado).IsUnique();
                e.Property(c => c.Estado).HasConversion<string>().HasMaxLength(20);
            });

            // ---- Incidencia ----
            modelBuilder.Entity<Incidencia>(e =>
            {
                e.HasOne(i => i.Muestra)
                    .WithMany(m => m.Incidencias)
                    .HasForeignKey(i => i.MuestraId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(i => i.Tecnico)
                    .WithMany(t => t.Incidencias)
                    .HasForeignKey(i => i.TecnicoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Evita duplicados si la app móvil reenvía el mismo registro
                // generado offline (Prueba 8 - Sincronización posterior).
                e.HasIndex(i => i.IdLocalOrigen).IsUnique().HasFilter("[IdLocalOrigen] IS NOT NULL");
            });

            // ---- AvanceMuestra ----
            modelBuilder.Entity<AvanceMuestra>(e =>
            {
                e.HasOne(a => a.Muestra)
                    .WithMany(m => m.Avances)
                    .HasForeignKey(a => a.MuestraId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(a => a.Tecnico)
                    .WithMany(t => t.Avances)
                    .HasForeignKey(a => a.TecnicoId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(a => a.IdLocalOrigen).IsUnique().HasFilter("[IdLocalOrigen] IS NOT NULL");
            });
        }
    }
}
