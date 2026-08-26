namespace QualityLab.API.Models.Enums
{
    /// <summary>
    /// Roles del sistema QualityLab.
    /// Se manejan como texto (no como int) para que aparezcan legibles
    /// en el token JWT y en la base de datos.
    /// </summary>
    public enum RolUsuario
    {
        ADMIN,
        SUPERVISOR,
        TECNICO,
        CLIENTE
    }
}
