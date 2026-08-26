using System.ComponentModel.DataAnnotations;

namespace QualityLab.API.Models.DTOs
{
    public class ClienteCreateDto
    {
        [Required] public string RazonSocial { get; set; } = string.Empty;
        [Required] public string RUC { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
    }

    public class ClienteResponseDto
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string RUC { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
    }
}
