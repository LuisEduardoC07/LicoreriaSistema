using LibreriaSistema.Aplicacion.Interfaces;
using LicoreriaSistema.Datos.Context;
using LicoreriaSistema.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace LicoreriaSistema.Datos.Repositorios;

public class ProductoRepository : IProductoRepository
{
    private readonly LicoreriaDbContext _context;

    public ProductoRepository(
        LicoreriaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Producto>> ObtenerTodasAsync(
        bool incluirInactivos = true,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .AsQueryable();

        if (!incluirInactivos)
        {
            consulta = consulta.Where(
                p => p.Activo);
        }

        return await consulta
            .OrderBy(p => p.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<Producto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .SingleOrDefaultAsync(
                p => p.Id == id,
                cancellationToken);
    }

    public async Task<bool> ExisteCodigoAsync(
        string codigo,
        int? excluirId = null,
        CancellationToken cancellationToken = default)
    {
        var codigoNormalizado = codigo.Trim();

        var consulta = _context.Productos
            .AsNoTracking()
            .Where(p => p.Codigo == codigoNormalizado);

        if (excluirId.HasValue)
        {
            consulta = consulta.Where(
                p => p.Id != excluirId.Value);
        }

        return await consulta.AnyAsync(
            cancellationToken);
    }

    public async Task<bool> ExisteCategoriaAsync(
        int categoriaId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categorias
            .AsNoTracking()
            .AnyAsync(
                c => c.Id == categoriaId,
                cancellationToken);
    }

    public async Task<bool> TieneInventarioAsync(
        int productoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.InventariosSucursal
            .AsNoTracking()
            .AnyAsync(
                i => i.ProductoId == productoId,
                cancellationToken);
    }

    public async Task AgregarAsync(
        Producto producto,
        CancellationToken cancellationToken = default)
    {
        await _context.Productos.AddAsync(
            producto,
            cancellationToken);
    }

    public async Task ActualizarAsync(
        Producto producto,
        CancellationToken cancellationToken = default)
    {
        await _context.Productos
            .Where(p => p.Id == producto.Id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        p => p.Nombre,
                        producto.Nombre)
                    .SetProperty(
                        p => p.Codigo,
                        producto.Codigo)
                    .SetProperty(
                        p => p.Descripcion,
                        producto.Descripcion)
                    .SetProperty(
                        p => p.PrecioCompra,
                        producto.PrecioCompra)
                    .SetProperty(
                        p => p.PrecioVenta,
                        producto.PrecioVenta)
                    .SetProperty(
                        p => p.CategoriaId,
                        producto.CategoriaId)
                    .SetProperty(
                        p => p.Activo,
                        producto.Activo),
                cancellationToken);
    }

    public async Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(
            cancellationToken);
    }
}
