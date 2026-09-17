using LibreriaSistema.Aplicacion.Interfaces;
using LicoreriaSistema.Datos.Context;
using LicoreriaSistema.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace LicoreriaSistema.Datos.Repositorios;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly LicoreriaDbContext _context;

    public UsuarioRepository(
        LicoreriaDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerParaAutenticacionAsync(
        string nombreUsuario,
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .AsSplitQuery()
            .Include(u => u.Rol)
                .ThenInclude(r => r.RolPermisos)
                    .ThenInclude(rp => rp.Permiso)
            .Include(u => u.UsuarioSucursales)
                .ThenInclude(us => us.Sucursal)
            .SingleOrDefaultAsync(
                u => u.NombreUsuario == nombreUsuario,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Usuario>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .AsSplitQuery()
            .Include(u => u.Rol)
            .Include(u => u.UsuarioSucursales)
                .ThenInclude(us => us.Sucursal)
            .OrderBy(u => u.NombreUsuario)
            .ToListAsync(cancellationToken);
    }

    public async Task<Usuario?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .AsSplitQuery()
            .Include(u => u.Rol)
            .Include(u => u.UsuarioSucursales)
                .ThenInclude(us => us.Sucursal)
            .SingleOrDefaultAsync(
                u => u.Id == id,
                cancellationToken);
    }

    public Task<bool> ExisteNombreUsuarioAsync(
        string nombreUsuario,
        int? excluirId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Usuarios
            .AsNoTracking()
            .Where(u => u.NombreUsuario == nombreUsuario);

        if (excluirId.HasValue)
        {
            query = query.Where(
                u => u.Id != excluirId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> ExisteCorreoAsync(
        string correo,
        int? excluirId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Usuarios
            .AsNoTracking()
            .Where(u =>
                u.Correo != null &&
                u.Correo == correo);

        if (excluirId.HasValue)
        {
            query = query.Where(
                u => u.Id != excluirId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task<bool> EsUltimoSuperAdministradorActivoAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .SingleOrDefaultAsync(
                u => u.Id == usuarioId,
                cancellationToken);

        if (usuario is null)
        {
            return false;
        }

        var rol = await _context.Roles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                r => r.Id == usuario.RolId,
                cancellationToken);

        if (rol is null ||
            !string.Equals(
                rol.Nombre,
                "SuperAdministrador",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var cantidadActivos =
            await _context.Usuarios
                .AsNoTracking()
                .CountAsync(
                    u =>
                        u.Activo &&
                        u.RolId == usuario.RolId,
                    cancellationToken);

        return cantidadActivos <= 1;
    }

    public async Task CrearAsync(
        Usuario usuario,
        IReadOnlyCollection<int> sucursalIds,
        CancellationToken cancellationToken = default)
    {
        _context.Usuarios.Add(usuario);

        foreach (var sucursalId in sucursalIds.Distinct())
        {
            _context.UsuariosSucursales.Add(
                new UsuarioSucursal
                {
                    Usuario = usuario,
                    SucursalId = sucursalId
                });
        }

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    public async Task ActualizarAsync(
        Usuario usuario,
        string? nuevoPasswordHash,
        IReadOnlyCollection<int> sucursalIds,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                cancellationToken);

        int actualizado;

        if (nuevoPasswordHash is not null)
        {
            actualizado = await _context.Usuarios
                .Where(u => u.Id == usuario.Id)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            u => u.Nombre,
                            usuario.Nombre)
                        .SetProperty(
                            u => u.Apellido,
                            usuario.Apellido)
                        .SetProperty(
                            u => u.NombreUsuario,
                            usuario.NombreUsuario)
                        .SetProperty(
                            u => u.Correo,
                            usuario.Correo)
                        .SetProperty(
                            u => u.PasswordHash,
                            nuevoPasswordHash)
                        .SetProperty(
                            u => u.RolId,
                            usuario.RolId)
                        .SetProperty(
                            u => u.Activo,
                            usuario.Activo)
                        .SetProperty(
                            u => u.AlcanceGlobal,
                            usuario.AlcanceGlobal),
                    cancellationToken);
        }
        else
        {
            actualizado = await _context.Usuarios
                .Where(u => u.Id == usuario.Id)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            u => u.Nombre,
                            usuario.Nombre)
                        .SetProperty(
                            u => u.Apellido,
                            usuario.Apellido)
                        .SetProperty(
                            u => u.NombreUsuario,
                            usuario.NombreUsuario)
                        .SetProperty(
                            u => u.Correo,
                            usuario.Correo)
                        .SetProperty(
                            u => u.RolId,
                            usuario.RolId)
                        .SetProperty(
                            u => u.Activo,
                            usuario.Activo)
                        .SetProperty(
                            u => u.AlcanceGlobal,
                            usuario.AlcanceGlobal),
                    cancellationToken);
        }

        if (actualizado == 0)
        {
            throw new InvalidOperationException(
                "No se pudo actualizar el usuario.");
        }

        await _context.UsuariosSucursales
            .Where(us => us.UsuarioId == usuario.Id)
            .ExecuteDeleteAsync(
                cancellationToken);

        foreach (var sucursalId in sucursalIds.Distinct())
        {
            _context.UsuariosSucursales.Add(
                new UsuarioSucursal
                {
                    UsuarioId = usuario.Id,
                    SucursalId = sucursalId
                });
        }

        await _context.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);
    }

    public async Task CambiarEstadoAsync(
        int usuarioId,
        bool activo,
        CancellationToken cancellationToken = default)
    {
        var actualizado = await _context.Usuarios
            .Where(u => u.Id == usuarioId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        u => u.Activo,
                        activo),
                cancellationToken);

        if (actualizado == 0)
        {
            throw new InvalidOperationException(
                "No se pudo cambiar el estado del usuario.");
        }
    }

    public async Task<IReadOnlyList<Rol>> ObtenerRolesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .OrderBy(r => r.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Sucursal>> ObtenerSucursalesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Sucursales
            .AsNoTracking()
            .OrderBy(s => s.Nombre)
            .ToListAsync(cancellationToken);
    }
}
