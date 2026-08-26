using QualityLab.API.Models.Enums;

namespace QualityLab.API.Models.Entities
{
    /// <summary>
    /// Cuenta de acceso al sistema. Un Usuario puede estar ligado
    /// opcionalmente a un Cliente (rol CLIENTE) o a un Tecnico (rol TECNICO)
    /// para poder filtrar la información que le corresponde ver.
    /// </summary>
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public RolUsuario Rol { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Vínculos opcionales según el rol
        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public int? TecnicoId { get; set; }
        public Tecnico? Tecnico { get; set; }
    }
}
