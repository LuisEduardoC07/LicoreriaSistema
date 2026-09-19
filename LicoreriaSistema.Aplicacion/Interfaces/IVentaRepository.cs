using LibreriaSistema.Aplicacion.Modelos;
using LicoreriaSistema.Dominio.Entidades;

namespace LibreriaSistema.Aplicacion.Interfaces;

public interface IVentaRepository
{
    Task<IReadOnlyList<InventarioSucursal>> BuscarProductosDisponiblesAsync(
        int sucursalId,
        string? textoBusqueda = null,
        int maxResultados = 30,
        CancellationToken cancellationToken = default);

    Task<InventarioSucursal?> ObtenerProductoDisponibleAsync(
        int productoId,
        int sucursalId,
        CancellationToken cancellationToken = default);

    Task<(bool Exitoso, string Mensaje)> RegistrarVentaAsync(
        Venta venta,
        CancellationToken cancellationToken = default);

    Task<Venta?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Venta?> ObtenerPorNumeroFacturaAsync(
        string numeroFactura,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Venta>> BuscarHistorialAsync(
        FiltroHistorialVentas filtro,
        IReadOnlyCollection<int>? sucursalesPermitidas = null,
        CancellationToken cancellationToken = default);
}

