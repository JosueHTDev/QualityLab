namespace QualityLab.API.Models.Entities
{
    public class Resultado
    {
        public int Id { get; set; }

        public int AnalisisId { get; set; }
        public Analisis? Analisis { get; set; }

        public int TecnicoId { get; set; }
        public Tecnico? Tecnico { get; set; }

        public string Parametro { get; set; } = string.Empty;
        public string ValorObtenido { get; set; } = string.Empty;
        public string ValorReferencia { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        public bool Conforme { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
