namespace QualityLab.WinForms.Models
{
    public class ClienteCreateDto
    {
        public string RazonSocial { get; set; } = string.Empty;
        public string RUC { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
    }

    public class ClienteResponse
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string RUC { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }

        public override string ToString() => $"{RazonSocial} ({RUC})";
    }
}
