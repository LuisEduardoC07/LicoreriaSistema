namespace LicoreriaSistema.Dominio.Entidades;

public class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string NombreUsuario { get; set; } = string.Empty;

    public string? Correo { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public int RolId { get; set; }

    public Rol Rol { get; set; } = null!;

    public bool Activo { get; set; } = true;

    /// <summary>
    /// Indica si el usuario tiene alcance global sobre el sistema.
    /// Los usuarios con alcance por sucursal deben tener este valor en false.
    /// </summary>
    public bool AlcanceGlobal { get; set; } = false;

    public ICollection<UsuarioSucursal> UsuarioSucursales { get; set; }
        = new List<UsuarioSucursal>();
}