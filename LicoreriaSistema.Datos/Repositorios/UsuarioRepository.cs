using LibreriaSistema.Aplicacion.Interfaces;
using LicoreriaSistema.Datos.Context;
using LicoreriaSistema.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace LicoreriaSistema.Datos.Repositorios;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly LicoreriaDbContext _context;

    public UsuarioRepository(LicoreriaDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerParaAutenticacionAsync(
        string nombreUsuario,
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .Include(u => u.Rol)
                .ThenInclude(r => r.RolPermisos)
                    .ThenInclude(rp => rp.Permiso)
            .Include(u => u.UsuarioSucursales)
                .ThenInclude(us => us.Sucursal)
            .SingleOrDefaultAsync(
                u => u.NombreUsuario == nombreUsuario,
                cancellationToken);
    }
}