namespace QualityLab.API.Models.Enums
{
    /// <summary>
    /// Flujo de una muestra: Registrada -> Asignada -> EnAnalisis -> Analizada -> CertificadoEmitido
    /// </summary>
    public enum EstadoMuestra
    {
        Registrada,
        Asignada,
        EnAnalisis,
        Analizada,
        Rechazada,
        CertificadoEmitido
    }

    public enum EstadoLote
    {
        Recibido,
        EnProceso,
        Finalizado
    }

    public enum EstadoAnalisis
    {
        Pendiente,
        EnProceso,
        Completado
    }

    public enum EstadoCertificado
    {
        Emitido,
        Anulado
    }
}
