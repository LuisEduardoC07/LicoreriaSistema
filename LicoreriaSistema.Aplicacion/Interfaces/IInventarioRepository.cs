using LicoreriaSistema.Dominio.Entidades;

namespace LibreriaSistema.Aplicacion.Interfaces;

public interface IInventarioRepository
{
    Task<IReadOnlyList<InventarioSucursal>> ObtenerPorSucursalAsync(
        int sucursalId,
        CancellationToken cancellationToken = default);

    Task<InventarioSucursal?> ObtenerAsync(
        int productoId,
        int sucursalId,
        CancellationToken cancellationToken = default);

    Task<InventarioSucursal?> ObtenerPorIdAsync(
        int inventarioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Producto>> ObtenerProductosActivosAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Sucursal>> ObtenerSucursalesActivasAsync(
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        InventarioSucursal inventario,
        CancellationToken cancellationToken = default);

    Task<bool> ActualizarConfiguracionAsync(
        int inventarioId,
        decimal stockMinimo,
        decimal stockMaximo,
        CancellationToken cancellationToken = default);

    Task<bool> AgregarExistenciaAsync(
        int productoId,
        int sucursalId,
        decimal cantidad,
        decimal stockMinimo,
        decimal stockMaximo,
        CancellationToken cancellationToken = default);

    Task<bool> DescontarExistenciaAsync(
        int productoId,
        int sucursalId,
        decimal cantidad,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteInventarioAsync(
        int productoId,
        int sucursalId,
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}
