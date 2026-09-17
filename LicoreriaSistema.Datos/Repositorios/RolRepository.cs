using LibreriaSistema.Aplicacion.Interfaces;
using LicoreriaSistema.Datos.Context;
using LicoreriaSistema.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace LicoreriaSistema.Datos.Repositorios;

public sealed class RolRepository : IRolRepository
{
    private readonly LicoreriaDbContext _context;

    public RolRepository(
        LicoreriaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Rol>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .Include(r => r.RolPermisos)
                .ThenInclude(rp => rp.Permiso)
            .Include(r => r.Usuarios)
            .OrderBy(r => r.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Rol?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .Include(r => r.RolPermisos)
                .ThenInclude(rp => rp.Permiso)
            .Include(r => r.Usuarios)
            .SingleOrDefaultAsync(
                r => r.Id == id,
                cancellationToken);
    }

    public Task<bool> ExisteNombreAsync(
        string nombre,
        int? excluirId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Roles
            .AsNoTracking()
            .Where(r => r.Nombre == nombre);

        if (excluirId.HasValue)
        {
            query = query.Where(r => r.Id != excluirId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> TieneUsuariosAsync(
        int rolId,
        CancellationToken cancellationToken = default)
    {
        return _context.Usuarios
            .AsNoTracking()
            .AnyAsync(
                u => u.RolId == rolId,
                cancellationToken);
    }

    public async Task CrearAsync(
        Rol rol,
        CancellationToken cancellationToken = default)
    {
        _context.Roles.Add(rol);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    public async Task ActualizarAsync(
        Rol rol,
        CancellationToken cancellationToken = default)
    {
        var actualizado = await _context.Roles
            .Where(r => r.Id == rol.Id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        r => r.Nombre,
                        rol.Nombre)
                    .SetProperty(
                        r => r.Descripcion,
                        rol.Descripcion)
                    .SetProperty(
                        r => r.Activo,
                        rol.Activo),
                cancellationToken);

        if (actualizado == 0)
        {
            throw new InvalidOperationException(
                "No se pudo actualizar el rol.");
        }
    }

    public async Task CambiarEstadoAsync(
        int rolId,
        bool activo,
        CancellationToken cancellationToken = default)
    {
        var actualizado = await _context.Roles
            .Where(r => r.Id == rolId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        r => r.Activo,
                        activo),
                cancellationToken);

        if (actualizado == 0)
        {
            throw new InvalidOperationException(
                "No se pudo cambiar el estado del rol.");
        }
    }

    public async Task GuardarPermisosAsync(
        int rolId,
        IReadOnlyCollection<int> permisoIds,
        CancellationToken cancellationToken = default)
    {
        var existentes = await _context.RolPermisos
            .Where(rp => rp.RolId == rolId)
            .ToListAsync(cancellationToken);

        var deseados = permisoIds
            .Distinct()
            .ToHashSet();

        foreach (var existente in existentes)
        {
            if (!deseados.Contains(existente.PermisoId))
            {
                _context.RolPermisos.Remove(existente);
            }
        }

        var existentesIds = existentes
            .Select(x => x.PermisoId)
            .ToHashSet();

        foreach (var permisoId in deseados)
        {
            if (!existentesIds.Contains(permisoId))
            {
                _context.RolPermisos.Add(
                    new RolPermiso
                    {
                        RolId = rolId,
                        PermisoId = permisoId
                    });
            }
        }

        await _context.SaveChangesAsync(
            cancellationToken);
    }
}
