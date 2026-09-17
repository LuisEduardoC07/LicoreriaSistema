using LicoreriaSistema.Dominio.Entidades;

namespace LibreriaSistema.Aplicacion.Interfaces;

public interface IPermisoRepository
{
    Task<IReadOnlyList<Permiso>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default);
}
