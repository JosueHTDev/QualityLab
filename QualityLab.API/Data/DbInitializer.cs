using QualityLab.API.Models.Entities;
using QualityLab.API.Models.Enums;

namespace QualityLab.API.Data
{
    /// <summary>
    /// Crea la base de datos (si no existe) y siembra datos mínimos
    /// para poder probar el sistema apenas se levanta el proyecto.
    /// </summary>
    public static class DbInitializer
    {
        public static void Seed(QualityLabDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Usuarios.Any())
            {
                return; // Ya existen datos, no volver a sembrar
            }

            // ---- Cliente de prueba ----
            var cliente = new Cliente
            {
                RazonSocial = "Industrias Andinas S.A.C.",
                RUC = "20123456789",
                Email = "contacto@industriasandinas.com",
                Telefono = "064-123456",
                Direccion = "Av. Ferrocarril 123, Huancayo"
            };
            context.Clientes.Add(cliente);

            // ---- Técnico de prueba ----
            var tecnico = new Tecnico
            {
                Nombres = "Carlos",
                Apellidos = "Ramirez",
                Especialidad = "Análisis Fisicoquímico",
                Email = "carlos.ramirez@qualitylab.com",
                Telefono = "987654321"
            };
            context.Tecnicos.Add(tecnico);

            context.SaveChanges(); // Para obtener los Id generados

            // ---- Usuarios (uno por rol) ----
            context.Usuarios.AddRange(
                new Usuario
                {
                    NombreUsuario = "admin",
                    Email = "admin@qualitylab.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Rol = RolUsuario.ADMIN
                },
                new Usuario
                {
                    NombreUsuario = "supervisor1",
                    Email = "supervisor@qualitylab.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Supervisor123!"),
                    Rol = RolUsuario.SUPERVISOR
                },
                new Usuario
                {
                    NombreUsuario = "tecnico1",
                    Email = "tecnico1@qualitylab.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tecnico123!"),
                    Rol = RolUsuario.TECNICO,
                    TecnicoId = tecnico.Id
                },
                new Usuario
                {
                    NombreUsuario = "cliente1",
                    Email = "cliente1@qualitylab.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cliente123!"),
                    Rol = RolUsuario.CLIENTE,
                    ClienteId = cliente.Id
                }
            );

            // ---- Lote y muestra de ejemplo, ya con resultado y certificado ----
            var lote = new Lote
            {
                Codigo = "LOTE-2025-001",
                ClienteId = cliente.Id,
                Descripcion = "Lote de acero estructural para control de calidad",
                Estado = EstadoLote.EnProceso
            };
            context.Lotes.Add(lote);
            context.SaveChanges();

            var muestra = new Muestra
            {
                Codigo = "MUE-2025-0001",
                LoteId = lote.Id,
                ClienteId = cliente.Id,
                TipoProducto = "Barra de acero corrugado",
                Estado = EstadoMuestra.Asignada,
                TecnicoAsignadoId = tecnico.Id,
                FechaAsignacion = DateTime.UtcNow
            };
            context.Muestras.Add(muestra);
            context.SaveChanges();

            var analisis = new Analisis
            {
                MuestraId = muestra.Id,
                TecnicoId = tecnico.Id,
                TipoAnalisis = "Resistencia a la tracción",
                Estado = EstadoAnalisis.EnProceso
            };
            context.Analisis.Add(analisis);
            context.SaveChanges();
        }
    }
}
