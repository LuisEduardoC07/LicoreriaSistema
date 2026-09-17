using LibreriaSistema.Aplicacion.Interfaces;
using LibreriaSistema.Aplicacion.Modelos;
using LicoreriaSistema.Dominio.Entidades;

namespace LibreriaSistema.Aplicacion.Servicios;

public sealed class RolService
{
    private readonly IRolRepository _rolRepository;
    private readonly IPermisoRepository _permisoRepository;
    private readonly IContextoUsuarioActual _contextoUsuarioActual;

    public RolService(
        IRolRepository rolRepository,
        IPermisoRepository permisoRepository,
        IContextoUsuarioActual contextoUsuarioActual)
    {
        _rolRepository = rolRepository;
        _permisoRepository = permisoRepository;
        _contextoUsuarioActual = contextoUsuarioActual;
    }

    public async Task<IReadOnlyList<Rol>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        ExigirSuperAdministrador();

        return await _rolRepository.ObtenerTodosAsync(
            cancellationToken);
    }

    public async Task<Rol?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        ExigirSuperAdministrador();

        return await _rolRepository.ObtenerPorIdAsync(
            id,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Permiso>> ObtenerPermisosAsync(
        CancellationToken cancellationToken = default)
    {
        ExigirSuperAdministrador();

        return await _permisoRepository.ObtenerTodosAsync(
            cancellationToken);
    }

    public async Task<ResultadoOperacion> CrearAsync(
        string? nombre,
        string? descripcion,
        CancellationToken cancellationToken = default)
    {
        if (!EsSuperAdministrador())
        {
            return ResultadoOperacion.Fallido(
                "Solo el SuperAdministrador puede crear roles.");
        }

        nombre = nombre?.Trim() ?? string.Empty;
        descripcion = descripcion?.Trim() ?? string.Empty;

        var validacion = Validar(
            nombre,
            descripcion);

        if (!validacion.Exitoso)
        {
            return validacion;
        }

        if (await _rolRepository.ExisteNombreAsync(
                nombre,
                null,
                cancellationToken))
        {
            return ResultadoOperacion.Fallido(
                "Ya existe un rol con ese nombre.");
        }

        var rol = new Rol
        {
            Nombre = nombre,
            Descripcion = string.IsNullOrWhiteSpace(descripcion)
                ? null
                : descripcion,
            Activo = true
        };

        await _rolRepository.CrearAsync(
            rol,
            cancellationToken);

        return ResultadoOperacion.Ok(
            "Rol creado correctamente.");
    }

    public async Task<ResultadoOperacion> ActualizarAsync(
        int id,
        string? nombre,
        string? descripcion,
        bool activo,
        CancellationToken cancellationToken = default)
    {
        if (!EsSuperAdministrador())
        {
            return ResultadoOperacion.Fallido(
                "Solo el SuperAdministrador puede modificar roles.");
        }

        if (id <= 0)
        {
            return ResultadoOperacion.Fallido(
                "El rol indicado no es válido.");
        }

        nombre = nombre?.Trim() ?? string.Empty;
        descripcion = descripcion?.Trim() ?? string.Empty;

        var validacion = Validar(
            nombre,
            descripcion);

        if (!validacion.Exitoso)
        {
            return validacion;
        }

        var rol = await _rolRepository.ObtenerPorIdAsync(
            id,
            cancellationToken);

        if (rol is null)
        {
            return ResultadoOperacion.Fallido(
                "El rol no existe.");
        }

        // El SuperAdministrador principal no puede desactivarse.
        if (!activo && id == 1)
        {
            return ResultadoOperacion.Fallido(
                "El SuperAdministrador principal no puede desactivarse.");
        }

        if (await _rolRepository.ExisteNombreAsync(
                nombre,
                id,
                cancellationToken))
        {
            return ResultadoOperacion.Fallido(
                "Ya existe otro rol con ese nombre.");
        }

        rol.Nombre = nombre;
        rol.Descripcion = string.IsNullOrWhiteSpace(descripcion)
            ? null
            : descripcion;
        rol.Activo = activo;

        await _rolRepository.ActualizarAsync(
            rol,
            cancellationToken);

        return ResultadoOperacion.Ok(
            "Rol actualizado correctamente.");
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(
        int id,
        bool activo,
        CancellationToken cancellationToken = default)
    {
        if (!EsSuperAdministrador())
        {
            return ResultadoOperacion.Fallido(
                "Solo el SuperAdministrador puede activar o desactivar roles.");
        }

        if (id <= 0)
        {
            return ResultadoOperacion.Fallido(
                "El rol indicado no es válido.");
        }

        if (!activo && id == 1)
        {
            return ResultadoOperacion.Fallido(
                "El SuperAdministrador principal no puede desactivarse.");
        }

        var rol = await _rolRepository.ObtenerPorIdAsync(
            id,
            cancellationToken);

        if (rol is null)
        {
            return ResultadoOperacion.Fallido(
                "El rol no existe.");
        }

        await _rolRepository.CambiarEstadoAsync(
            id,
            activo,
            cancellationToken);

        return ResultadoOperacion.Ok(
            activo
                ? "Rol activado correctamente."
                : "Rol desactivado correctamente.");
    }

    public async Task<ResultadoOperacion> GuardarPermisosAsync(
        int rolId,
        IReadOnlyCollection<int>? permisoIds,
        CancellationToken cancellationToken = default)
    {
        if (!EsSuperAdministrador())
        {
            return ResultadoOperacion.Fallido(
                "Solo el SuperAdministrador puede administrar permisos de roles.");
        }

        if (rolId <= 0)
        {
            return ResultadoOperacion.Fallido(
                "El rol indicado no es válido.");
        }

        var rol = await _rolRepository.ObtenerPorIdAsync(
            rolId,
            cancellationToken);

        if (rol is null)
        {
            return ResultadoOperacion.Fallido(
                "El rol no existe.");
        }

        var ids = permisoIds?
            .Where(id => id > 0)
            .Distinct()
            .ToArray()
            ?? Array.Empty<int>();

        var permisos = await _permisoRepository
            .ObtenerTodosAsync(cancellationToken);

        var idsValidos = permisos
            .Select(p => p.Id)
            .ToHashSet();

        if (ids.Any(id => !idsValidos.Contains(id)))
        {
            return ResultadoOperacion.Fallido(
                "Se indicó un permiso que no existe.");
        }

        await _rolRepository.GuardarPermisosAsync(
            rolId,
            ids,
            cancellationToken);

        return ResultadoOperacion.Ok(
            "Permisos del rol actualizados correctamente.");
    }

    private bool EsSuperAdministrador()
    {
        return string.Equals(
            _contextoUsuarioActual.Rol?.Trim(),
            "SuperAdministrador",
            StringComparison.OrdinalIgnoreCase);
    }

    private void ExigirSuperAdministrador()
    {
        if (!EsSuperAdministrador())
        {
            throw new UnauthorizedAccessException(
                "Solo el SuperAdministrador puede acceder a la administración de roles y permisos.");
        }
    }

    private static ResultadoOperacion Validar(
        string nombre,
        string descripcion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return ResultadoOperacion.Fallido(
                "Debe indicar el nombre del rol.");
        }

        if (nombre.Length > 100)
        {
            return ResultadoOperacion.Fallido(
                "El nombre del rol no puede superar 100 caracteres.");
        }

        if (descripcion.Length > 250)
        {
            return ResultadoOperacion.Fallido(
                "La descripción no puede superar 250 caracteres.");
        }

        return ResultadoOperacion.Ok(string.Empty);
    }
}
