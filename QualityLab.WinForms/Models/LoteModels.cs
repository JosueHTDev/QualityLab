namespace QualityLab.WinForms.Models
{
    public class LoteCreateDto
    {
        public string Codigo { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class LoteResponse
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public string? ClienteRazonSocial { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;

        public override string ToString() => $"{Codigo} - {ClienteRazonSocial}";
    }
}
