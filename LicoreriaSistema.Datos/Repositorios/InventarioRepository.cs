using LibreriaSistema.Aplicacion.Interfaces;
using LicoreriaSistema.Datos.Context;
using LicoreriaSistema.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace LicoreriaSistema.Datos.Repositorios;

public class InventarioRepository : IInventarioRepository
{
    private readonly LicoreriaDbContext _context;

    public InventarioRepository(
        LicoreriaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<InventarioSucursal>> ObtenerPorSucursalAsync(
        int sucursalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.InventariosSucursal
            .AsNoTracking()
            .Where(i => i.SucursalId == sucursalId)
            .Include(i => i.Producto)
                .ThenInclude(p => p.Categoria)
            .Include(i => i.Sucursal)
            .OrderBy(i => i.Producto.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<InventarioSucursal?> ObtenerAsync(
        int productoId,
        int sucursalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.InventariosSucursal
            .AsNoTracking()
            .SingleOrDefaultAsync(
                i =>
                    i.ProductoId == productoId &&
                    i.SucursalId == sucursalId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Producto>> ObtenerProductosActivosAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Sucursal>> ObtenerSucursalesActivasAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Sucursales
            .AsNoTracking()
            .Where(s => s.Activa)
            .OrderBy(s => s.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task AgregarAsync(
        InventarioSucursal inventario,
        CancellationToken cancellationToken = default)
    {
        await _context.InventariosSucursal.AddAsync(
            inventario,
            cancellationToken);
    }

    public async Task<bool> ActualizarConfiguracionAsync(
        int inventarioId,
        decimal stockMinimo,
        decimal stockMaximo,
        CancellationToken cancellationToken = default)
    {
        var cantidadActualizada =
            await _context.InventariosSucursal
                .Where(i => i.Id == inventarioId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            i => i.StockMinimo,
                            stockMinimo)
                        .SetProperty(
                            i => i.StockMaximo,
                            stockMaximo),
                    cancellationToken);

        return cantidadActualizada > 0;
    }

    public async Task<bool> AgregarExistenciaAsync(
        int productoId,
        int sucursalId,
        decimal cantidad,
        decimal stockMinimo,
        decimal stockMaximo,
        CancellationToken cancellationToken = default)
    {
        var cantidadActualizada =
            await _context.InventariosSucursal
                .Where(i =>
                    i.ProductoId == productoId &&
                    i.SucursalId == sucursalId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            i => i.Cantidad,
                            i => i.Cantidad + cantidad)
                        .SetProperty(
                            i => i.StockMinimo,
                            stockMinimo)
                        .SetProperty(
                            i => i.StockMaximo,
                            stockMaximo),
                    cancellationToken);

        return cantidadActualizada > 0;
    }

    public async Task<bool> DescontarExistenciaAsync(
        int productoId,
        int sucursalId,
        decimal cantidad,
        CancellationToken cancellationToken = default)
    {
        var cantidadActualizada =
            await _context.InventariosSucursal
                .Where(i =>
                    i.ProductoId == productoId &&
                    i.SucursalId == sucursalId &&
                    i.Cantidad >= cantidad)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        i => i.Cantidad,
                        i => i.Cantidad - cantidad),
                    cancellationToken);

        return cantidadActualizada > 0;
    }

    public async Task<bool> ExisteInventarioAsync(
        int productoId,
        int sucursalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.InventariosSucursal
            .AsNoTracking()
            .AnyAsync(
                i =>
                    i.ProductoId == productoId &&
                    i.SucursalId == sucursalId,
                cancellationToken);
    }

    public async Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(
            cancellationToken);
    }
}
