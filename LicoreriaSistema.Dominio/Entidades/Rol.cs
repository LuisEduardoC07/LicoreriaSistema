namespace LicoreriaSistema.Dominio.Entidades;

public class Rol
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<Usuario> Usuarios { get; set; }
        = new List<Usuario>();

    public ICollection<RolPermiso> RolPermisos { get; set; }
        = new List<RolPermiso>();
}