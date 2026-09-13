namespace LicoreriaSistema.Dominio.Entidades;

public class UsuarioSucursal
{
    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public int SucursalId { get; set; }

    public Sucursal Sucursal { get; set; } = null!;
}