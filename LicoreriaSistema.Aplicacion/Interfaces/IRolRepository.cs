using LicoreriaSistema.Dominio.Entidades;

namespace LibreriaSistema.Aplicacion.Interfaces;

public interface IRolRepository
{
    Task<IReadOnlyList<Rol>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default);

    Task<Rol?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteNombreAsync(
        string nombre,
        int? excluirId = null,
        CancellationToken cancellationToken = default);

    Task<bool> TieneUsuariosAsync(
        int rolId,
        CancellationToken cancellationToken = default);

    Task CrearAsync(
        Rol rol,
        CancellationToken cancellationToken = default);

    Task ActualizarAsync(
        Rol rol,
        CancellationToken cancellationToken = default);

    Task CambiarEstadoAsync(
        int rolId,
        bool activo,
        CancellationToken cancellationToken = default);

    Task GuardarPermisosAsync(
        int rolId,
        IReadOnlyCollection<int> permisoIds,
        CancellationToken cancellationToken = default);
}
