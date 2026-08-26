namespace QualityLab.API.Models.Entities
{
    public class Cliente
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string RUC { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public bool Activo { get; set; } = true;

        public ICollection<Lote> Lotes { get; set; } = new List<Lote>();
        public ICollection<Muestra> Muestras { get; set; } = new List<Muestra>();
    }
}
