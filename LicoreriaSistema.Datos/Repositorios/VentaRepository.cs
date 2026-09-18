using LibreriaSistema.Aplicacion.Interfaces;
using LicoreriaSistema.Datos.Context;
using LicoreriaSistema.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace LicoreriaSistema.Datos.Repositorios;

public sealed class VentaRepository : IVentaRepository
{
    private readonly LicoreriaDbContext _context;

    public VentaRepository(
        LicoreriaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<InventarioSucursal>>
        BuscarProductosDisponiblesAsync(
            int sucursalId,
            string? textoBusqueda = null,
            int maxResultados = 30,
            CancellationToken cancellationToken = default)
    {
        if (sucursalId <= 0)
        {
            return Array.Empty<InventarioSucursal>();
        }

        if (maxResultados <= 0)
        {
            maxResultados = 30;
        }

        if (maxResultados > 100)
        {
            maxResultados = 100;
        }

        var busqueda =
            textoBusqueda?.Trim();

        var consulta =
            _context.InventariosSucursal
                .AsNoTracking()
                .Where(i =>
                    i.SucursalId == sucursalId &&
                    i.Cantidad > 0 &&
                    i.Producto.Activo);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            consulta = consulta.Where(i =>
                i.Producto.Nombre.Contains(busqueda) ||
                i.Producto.Codigo.Contains(busqueda));
        }

        return await consulta
            .Include(i => i.Producto)
                .ThenInclude(p => p.Categoria)
            .Include(i => i.Producto)
                .ThenInclude(p => p.ClasificacionFiscal)
                    .ThenInclude(cf => cf!.ReglasImpuesto)
            .Include(i => i.Sucursal)
            .OrderBy(i => i.Producto.Nombre)
            .Take(maxResultados)
            .ToListAsync(cancellationToken);
    }

    public async Task<InventarioSucursal?>
        ObtenerProductoDisponibleAsync(
            int productoId,
            int sucursalId,
            CancellationToken cancellationToken = default)
    {
        if (productoId <= 0 ||
            sucursalId <= 0)
        {
            return null;
        }

        return await _context.InventariosSucursal
            .AsNoTracking()
            .Include(i => i.Producto)
                .ThenInclude(p => p.Categoria)
            .Include(i => i.Producto)
                .ThenInclude(p => p.ClasificacionFiscal)
                    .ThenInclude(cf => cf!.ReglasImpuesto)
            .Include(i => i.Sucursal)
            .SingleOrDefaultAsync(
                i =>
                    i.ProductoId == productoId &&
                    i.SucursalId == sucursalId &&
                    i.Cantidad > 0 &&
                    i.Producto.Activo,
                cancellationToken);
    }

    public async Task<(bool Exitoso, string Mensaje)>
        RegistrarVentaAsync(
            Venta venta,
            CancellationToken cancellationToken = default)
    {
        if (venta is null)
        {
            return (
                false,
                "La venta indicada no es válida.");
        }

        if (venta.SucursalId <= 0)
        {
            return (
                false,
                "La sucursal de la venta no es válida.");
        }

        if (venta.UsuarioId <= 0)
        {
            return (
                false,
                "El usuario de la venta no es válido.");
        }

        if (venta.Detalles is null ||
            venta.Detalles.Count == 0)
        {
            return (
                false,
                "La venta debe contener al menos un producto.");
        }

        var cantidadesPorProducto =
            venta.Detalles
                .GroupBy(d => d.ProductoId)
                .Select(g => new
                {
                    ProductoId = g.Key,
                    Cantidad = g.Sum(d => d.Cantidad)
                })
                .ToList();

        if (cantidadesPorProducto.Any(x =>
                x.ProductoId <= 0 ||
                x.Cantidad <= 0))
        {
            return (
                false,
                "La venta contiene productos o cantidades no válidas.");
        }

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync(
                    cancellationToken);

        try
        {
            var productoIds =
                cantidadesPorProducto
                    .Select(x => x.ProductoId)
                    .ToArray();

            var inventarios =
                await _context.InventariosSucursal
                    .Where(i =>
                        i.SucursalId == venta.SucursalId &&
                        productoIds.Contains(i.ProductoId))
                    .Include(i => i.Producto)
                    .ToDictionaryAsync(
                        i => i.ProductoId,
                        cancellationToken);

            foreach (var item in cantidadesPorProducto)
            {
                if (!inventarios.TryGetValue(
                        item.ProductoId,
                        out var inventario))
                {
                    await transaction.RollbackAsync(
                        cancellationToken);

                    return (
                        false,
                        $"El producto con ID {item.ProductoId} no tiene inventario registrado en esta sucursal.");
                }

                if (!inventario.Producto.Activo)
                {
                    await transaction.RollbackAsync(
                        cancellationToken);

                    return (
                        false,
                        $"El producto '{inventario.Producto.Nombre}' está inactivo.");
                }

                if (inventario.Cantidad < item.Cantidad)
                {
                    await transaction.RollbackAsync(
                        cancellationToken);

                    return (
                        false,
                        $"Inventario insuficiente para '{inventario.Producto.Nombre}'. Disponible: {inventario.Cantidad:N2}.");
                }
            }
venta.NumeroFactura = $"TMP-{Random.Shared.Next(10000000, 99999999)}";
            await _context.Ventas.AddAsync(
                venta,
                cancellationToken);

            await _context.SaveChangesAsync(
                cancellationToken);

            venta.NumeroFactura =
                $"FAC-{DateTime.Now:yyyyMMdd}-{venta.Id:D6}";

            await _context.SaveChangesAsync(
                cancellationToken);

            foreach (var item in cantidadesPorProducto)
            {
                var actualizado =
                    await _context.InventariosSucursal
                        .Where(i =>
                            i.Id ==
                                inventarios[
                                    item.ProductoId].Id &&
                            i.SucursalId ==
                                venta.SucursalId &&
                            i.Cantidad >=
                                item.Cantidad)
                        .ExecuteUpdateAsync(
                            setters =>
                                setters.SetProperty(
                                    i => i.Cantidad,
                                    i => i.Cantidad -
                                        item.Cantidad),
                            cancellationToken);

                if (actualizado != 1)
                {
                    throw new InvalidOperationException(
                        $"No fue posible descontar el inventario del producto con ID {item.ProductoId}.");
                }
            }

            await transaction.CommitAsync(
                cancellationToken);

            return (
                true,
                venta.NumeroFactura);
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(
                CancellationToken.None);

            throw;
        }
        catch (Exception ex)
{
    await transaction.RollbackAsync(CancellationToken.None);

    var detalle = ex.Message;

    if (ex.InnerException is not null)
    {
        detalle += $" | Inner: {ex.InnerException.Message}";
    }

    if (ex.InnerException?.InnerException is not null)
    {
        detalle += $" | Inner2: {ex.InnerException.InnerException.Message}";
    }

    return (
        false,
        $"No fue posible registrar la venta. No se realizaron cambios. Error: {detalle}"
    );
}
    }

    public async Task<Venta?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return null;
        }

        return await _context.Ventas
            .AsNoTracking()
            .Include(v => v.Usuario)
            .Include(v => v.Sucursal)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .SingleOrDefaultAsync(
                v => v.Id == id,
                cancellationToken);
    }

    public async Task<Venta?> ObtenerPorNumeroFacturaAsync(
        string numeroFactura,
        CancellationToken cancellationToken = default)
    {
        var numero =
            numeroFactura?.Trim();

        if (string.IsNullOrWhiteSpace(numero))
        {
            return null;
        }

        return await _context.Ventas
            .AsNoTracking()
            .Include(v => v.Usuario)
            .Include(v => v.Sucursal)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .SingleOrDefaultAsync(
                v => v.NumeroFactura == numero,
                cancellationToken);
    }
}


