using LibreriaSistema.Aplicacion.DTOs;
using LibreriaSistema.Aplicacion.Interfaces;
using LicoreriaSistema.Datos.Context;
using Microsoft.EntityFrameworkCore;

namespace LicoreriaSistema.Datos.Repositorios;

public sealed class CatalogoPublicoRepository
    : ICatalogoPublicoRepository
{
    private readonly LicoreriaDbContext _context;

    public CatalogoPublicoRepository(
        LicoreriaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SucursalCatalogoPublicoDto>>
        ObtenerSucursalesActivasAsync(
            CancellationToken cancellationToken = default)
    {
        return await _context.Sucursales
            .AsNoTracking()
            .Where(s => s.Activa)
            .OrderBy(s => s.Nombre)
            .Select(s => new SucursalCatalogoPublicoDto
            {
                Id = s.Id,
                Nombre = s.Nombre
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductoCatalogoPublicoDto>>
        ObtenerProductosAsync(
            int sucursalId,
            CancellationToken cancellationToken = default)
    {
        if (sucursalId <= 0)
        {
            return Array.Empty<ProductoCatalogoPublicoDto>();
        }

        var sucursalActiva = await _context.Sucursales
            .AsNoTracking()
            .AnyAsync(
                s => s.Id == sucursalId && s.Activa,
                cancellationToken);

        if (!sucursalActiva)
        {
            return Array.Empty<ProductoCatalogoPublicoDto>();
        }

        return await _context.Productos
            .AsNoTracking()
            .Where(p => p.Activo && p.Categoria.Activa)
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoCatalogoPublicoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                ImagenUrl = p.ImagenUrl,
                CategoriaNombre = p.Categoria.Nombre,
                PrecioVenta = p.PrecioVenta,
                Disponible = _context.InventariosSucursal.Any(i =>
                    i.ProductoId == p.Id &&
                    i.SucursalId == sucursalId &&
                    i.Cantidad > 0)
            })
            .ToListAsync(cancellationToken);
    }
}
