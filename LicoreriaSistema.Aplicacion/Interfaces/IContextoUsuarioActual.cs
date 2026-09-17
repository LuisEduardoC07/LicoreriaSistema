namespace LibreriaSistema.Aplicacion.Interfaces;

public interface IContextoUsuarioActual
{
    int UsuarioId { get; }

    string? NombreUsuario { get; }

    string? Rol { get; }

    bool AlcanceGlobal { get; }

    IReadOnlySet<int> SucursalIds { get; }

    bool EstaAutenticado { get; }

    bool TienePermiso(string permiso);

    bool PuedeAccederASucursal(int sucursalId);
}
