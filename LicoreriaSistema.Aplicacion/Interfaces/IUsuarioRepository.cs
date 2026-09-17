using LicoreriaSistema.Dominio.Entidades;

namespace LibreriaSistema.Aplicacion.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerParaAutenticacionAsync(
        string nombreUsuario,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Usuario>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default);

    Task<Usuario?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteNombreUsuarioAsync(
        string nombreUsuario,
        int? excluirId = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteCorreoAsync(
        string correo,
        int? excluirId = null,
        CancellationToken cancellationToken = default);

    Task<bool> EsUltimoSuperAdministradorActivoAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task CrearAsync(
        Usuario usuario,
        IReadOnlyCollection<int> sucursalIds,
        CancellationToken cancellationToken = default);

    Task ActualizarAsync(
        Usuario usuario,
        string? nuevoPasswordHash,
        IReadOnlyCollection<int> sucursalIds,
        CancellationToken cancellationToken = default);

    Task CambiarEstadoAsync(
        int usuarioId,
        bool activo,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Rol>> ObtenerRolesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Sucursal>> ObtenerSucursalesAsync(
        CancellationToken cancellationToken = default);
}
