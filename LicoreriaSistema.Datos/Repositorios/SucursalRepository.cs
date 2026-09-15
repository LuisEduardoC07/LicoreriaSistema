using LibreriaSistema.Aplicacion.Interfaces;
using LicoreriaSistema.Datos.Context;
using LicoreriaSistema.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace LicoreriaSistema.Datos.Repositorios;

public class SucursalRepository : ISucursalRepository
{
    private readonly LicoreriaDbContext _context;

    public SucursalRepository(
        LicoreriaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Sucursal>> ObtenerTodasAsync(
        bool incluirInactivas = true,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.Sucursales
            .AsNoTracking()
            .AsQueryable();

        if (!incluirInactivas)
        {
            consulta = consulta.Where(
                s => s.Activa);
        }

        return await consulta
            .OrderBy(s => s.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<Sucursal?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Sucursales
            .AsNoTracking()
            .SingleOrDefaultAsync(
                s => s.Id == id,
                cancellationToken);
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre,
        int? excluirId = null,
        CancellationToken cancellationToken = default)
    {
        var nombreNormalizado =
            nombre.Trim();

        var consulta = _context.Sucursales
            .AsNoTracking()
            .Where(s =>
                s.Nombre == nombreNormalizado);

        if (excluirId.HasValue)
        {
            consulta = consulta.Where(
                s => s.Id != excluirId.Value);
        }

        return await consulta.AnyAsync(
            cancellationToken);
    }

    public async Task<bool> TieneInventarioAsync(
        int sucursalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.InventariosSucursal
            .AsNoTracking()
            .AnyAsync(
                i => i.SucursalId == sucursalId,
                cancellationToken);
    }

    public async Task<bool> TieneUsuariosAsync(
        int sucursalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UsuariosSucursales
            .AsNoTracking()
            .AnyAsync(
                us => us.SucursalId == sucursalId,
                cancellationToken);
    }

    public async Task AgregarAsync(
        Sucursal sucursal,
        CancellationToken cancellationToken = default)
    {
        await _context.Sucursales.AddAsync(
            sucursal,
            cancellationToken);
    }

    public async Task ActualizarAsync(
        Sucursal sucursal,
        CancellationToken cancellationToken = default)
    {
        await _context.Sucursales
            .Where(s => s.Id == sucursal.Id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        s => s.Nombre,
                        sucursal.Nombre)
                    .SetProperty(
                        s => s.Direccion,
                        sucursal.Direccion)
                    .SetProperty(
                        s => s.Telefono,
                        sucursal.Telefono)
                    .SetProperty(
                        s => s.Activa,
                        sucursal.Activa),
                cancellationToken);
    }

    public async Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(
            cancellationToken);
    }
}
