using LibreriaSistema.Aplicacion.Interfaces;
using LicoreriaSistema.Datos.Context;
using LicoreriaSistema.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace LicoreriaSistema.Datos.Repositorios;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly LicoreriaDbContext _context;

    public CategoriaRepository(LicoreriaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Categoria>> ObtenerTodasAsync(
        bool incluirInactivas = true,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.Categorias
            .AsNoTracking()
            .Include(c => c.Productos)
            .AsQueryable();

        if (!incluirInactivas)
        {
            consulta = consulta.Where(c => c.Activa);
        }

        return await consulta
            .OrderBy(c => c.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<Categoria?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categorias
            .AsNoTracking()
            .Include(c => c.Productos)
            .SingleOrDefaultAsync(
                c => c.Id == id,
                cancellationToken);
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre,
        int? excluirId = null,
        CancellationToken cancellationToken = default)
    {
        var nombreNormalizado = nombre.Trim();

        var consulta = _context.Categorias
            .AsNoTracking()
            .Where(c => c.Nombre == nombreNormalizado);

        if (excluirId.HasValue)
        {
            consulta = consulta.Where(c => c.Id != excluirId.Value);
        }

        return await consulta.AnyAsync(cancellationToken);
    }

    public async Task<bool> TieneProductosAsync(
        int categoriaId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Productos
            .AsNoTracking()
            .AnyAsync(
                p => p.CategoriaId == categoriaId,
                cancellationToken);
    }

    public async Task AgregarAsync(
        Categoria categoria,
        CancellationToken cancellationToken = default)
    {
        await _context.Categorias.AddAsync(
            categoria,
            cancellationToken);
    }

    public Task ActualizarAsync(
        Categoria categoria,
        CancellationToken cancellationToken = default)
    {
        _context.Categorias.Update(categoria);

        return Task.CompletedTask;
    }

    public async Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}