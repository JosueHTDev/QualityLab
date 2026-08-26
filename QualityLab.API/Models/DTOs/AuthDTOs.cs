using System.ComponentModel.DataAnnotations;
using QualityLab.API.Models.Enums;

namespace QualityLab.API.Models.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiraEn { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int? ClienteId { get; set; }
        public int? TecnicoId { get; set; }
    }

    public class RegistrarUsuarioDto
    {
        [Required]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public RolUsuario Rol { get; set; }

        // Requerido si Rol == CLIENTE
        public int? ClienteId { get; set; }

        // Requerido si Rol == TECNICO
        public int? TecnicoId { get; set; }
    }
}
