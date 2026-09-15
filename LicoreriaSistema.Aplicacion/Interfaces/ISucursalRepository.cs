using LicoreriaSistema.Dominio.Entidades;

namespace LibreriaSistema.Aplicacion.Interfaces;

public interface ISucursalRepository
{
    Task<IReadOnlyList<Sucursal>> ObtenerTodasAsync(
        bool incluirInactivas = true,
        CancellationToken cancellationToken = default);

    Task<Sucursal?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteNombreAsync(
        string nombre,
        int? excluirId = null,
        CancellationToken cancellationToken = default);

    Task<bool> TieneInventarioAsync(
        int sucursalId,
        CancellationToken cancellationToken = default);

    Task<bool> TieneUsuariosAsync(
        int sucursalId,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Sucursal sucursal,
        CancellationToken cancellationToken = default);

    Task ActualizarAsync(
        Sucursal sucursal,
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}
