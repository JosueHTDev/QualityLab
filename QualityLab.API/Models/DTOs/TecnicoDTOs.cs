using System.ComponentModel.DataAnnotations;

namespace QualityLab.API.Models.DTOs
{
    public class TecnicoCreateDto
    {
        [Required] public string Nombres { get; set; } = string.Empty;
        [Required] public string Apellidos { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
    }

    public class TecnicoResponseDto
    {
        public int Id { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class RegistrarAvanceDto
    {
        [Required] public int MuestraId { get; set; }
        [Required] public string Descripcion { get; set; } = string.Empty;
        [Range(0, 100)] public int PorcentajeAvance { get; set; }

        // Opcionales: usados cuando el registro viene de la app móvil
        // trabajando offline y se sincroniza después.
        public Guid? IdLocalOrigen { get; set; }
        public DateTime? FechaCreacionLocal { get; set; }
    }

    public class RegistrarIncidenciaDto
    {
        [Required] public int MuestraId { get; set; }
        [Required] public string Descripcion { get; set; } = string.Empty;
        public Guid? IdLocalOrigen { get; set; }
        public DateTime? FechaCreacionLocal { get; set; }
    }
}
