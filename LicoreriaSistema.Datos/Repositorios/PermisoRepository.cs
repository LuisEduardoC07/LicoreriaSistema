using LibreriaSistema.Aplicacion.Interfaces;
using LicoreriaSistema.Datos.Context;
using LicoreriaSistema.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace LicoreriaSistema.Datos.Repositorios;

public sealed class PermisoRepository : IPermisoRepository
{
    private readonly LicoreriaDbContext _context;

    public PermisoRepository(
        LicoreriaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Permiso>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Permisos
            .AsNoTracking()
            .OrderBy(p => p.Codigo)
            .ToListAsync(cancellationToken);
    }
}
