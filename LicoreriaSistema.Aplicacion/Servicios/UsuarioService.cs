using LibreriaSistema.Aplicacion.Interfaces;
using LibreriaSistema.Aplicacion.Modelos;
using LibreriaSistema.Aplicacion.Seguridad;
using LicoreriaSistema.Dominio.Entidades;
using Microsoft.AspNetCore.Identity;

namespace LibreriaSistema.Aplicacion.Servicios;

public sealed class UsuarioService
{
    private const int RolSuperAdministradorId = 1;
    private const int RolAdministradorId = 2;
    private const int RolEncargadoSucursalId = 3;
    private const int RolVendedorCajeroId = 4;
    private const int RolAlmacenInventarioId = 5;

    private const string RolSuperAdministrador = "SuperAdministrador";
    private const string RolAdministrador = "Administrador";

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IContextoUsuarioActual _contextoUsuarioActual;
    private readonly PasswordHasher<Usuario> _passwordHasher;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IContextoUsuarioActual contextoUsuarioActual)
    {
        _usuarioRepository = usuarioRepository;
        _contextoUsuarioActual = contextoUsuarioActual;
        _passwordHasher = new PasswordHasher<Usuario>();
    }

    public async Task<IReadOnlyList<Usuario>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        ExigirAccesoAlModuloUsuarios();

        var usuarios =
            await _usuarioRepository.ObtenerTodosAsync(
                cancellationToken);

        if (EsSuperAdministradorActual())
        {
            return usuarios;
        }

        return usuarios
            .Where(PuedeVerUsuario)
            .ToList();
    }

    public async Task<Usuario?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        ExigirAccesoAlModuloUsuarios();

        if (id <= 0)
        {
            return null;
        }

        var usuario =
            await _usuarioRepository.ObtenerPorIdAsync(
                id,
                cancellationToken);

        if (usuario is null)
        {
            return null;
        }

        if (!PuedeVerUsuario(usuario))
        {
            return null;
        }

        return usuario;
    }

    public async Task<IReadOnlyList<Rol>> ObtenerRolesAsync(
        CancellationToken cancellationToken = default)
    {
        ExigirAccesoAlModuloUsuarios();

        var roles =
            await _usuarioRepository.ObtenerRolesAsync(
                cancellationToken);

        if (EsSuperAdministradorActual())
        {
            return roles;
        }

        return roles
            .Where(EsRolAdministrablePorAdministrador)
            .ToList();
    }

    public async Task<IReadOnlyList<Sucursal>> ObtenerSucursalesAsync(
        CancellationToken cancellationToken = default)
    {
        ExigirAccesoAlModuloUsuarios();

        var sucursales =
            await _usuarioRepository.ObtenerSucursalesAsync(
                cancellationToken);

        if (_contextoUsuarioActual.AlcanceGlobal)
        {
            return sucursales;
        }

        return sucursales
            .Where(s =>
                _contextoUsuarioActual
                    .PuedeAccederASucursal(s.Id))
            .ToList();
    }

    public async Task<ResultadoOperacion> CrearAsync(
        UsuarioEditorDto modelo,
        CancellationToken cancellationToken = default)
    {
        ExigirGestionUsuarios();

        var validacion =
            await ValidarAsync(
                modelo,
                true,
                null,
                cancellationToken);

        if (!validacion.Exitoso)
        {
            return validacion;
        }

        var nombreUsuario =
            modelo.NombreUsuario.Trim();

        if (await _usuarioRepository
                .ExisteNombreUsuarioAsync(
                    nombreUsuario,
                    null,
                    cancellationToken))
        {
            return ResultadoOperacion.Fallido(
                "Ya existe un usuario con ese nombre de usuario.");
        }

        var correo =
            NormalizarCorreo(modelo.Correo);

        if (!string.IsNullOrWhiteSpace(correo) &&
            await _usuarioRepository
                .ExisteCorreoAsync(
                    correo,
                    null,
                    cancellationToken))
        {
            return ResultadoOperacion.Fallido(
                "Ya existe un usuario con ese correo.");
        }

        var usuario = new Usuario
        {
            Nombre = modelo.Nombre.Trim(),
            Apellido = modelo.Apellido.Trim(),
            NombreUsuario = nombreUsuario,
            Correo = correo,
            RolId = modelo.RolId,
            Activo = modelo.Activo,
            AlcanceGlobal = modelo.AlcanceGlobal
        };

        usuario.PasswordHash =
            _passwordHasher.HashPassword(
                usuario,
                modelo.Password);

        var sucursalIds =
            modelo.AlcanceGlobal
                ? Array.Empty<int>()
                : ObtenerSucursalIds(modelo);

        await _usuarioRepository.CrearAsync(
            usuario,
            sucursalIds,
            cancellationToken);

        return ResultadoOperacion.Ok(
            "Usuario creado correctamente.");
    }

    public async Task<ResultadoOperacion> ActualizarAsync(
        UsuarioEditorDto modelo,
        CancellationToken cancellationToken = default)
    {
        ExigirGestionUsuarios();

        if (modelo.Id <= 0)
        {
            return ResultadoOperacion.Fallido(
                "El usuario indicado no es válido.");
        }

        var usuarioExistente =
            await _usuarioRepository.ObtenerPorIdAsync(
                modelo.Id,
                cancellationToken);

        if (usuarioExistente is null)
        {
            return ResultadoOperacion.Fallido(
                "El usuario no existe.");
        }

        if (!PuedeGestionarUsuario(usuarioExistente))
        {
            return ResultadoOperacion.Fallido(
                "No tienes permisos para modificar este usuario.");
        }

        var validacion =
            await ValidarAsync(
                modelo,
                false,
                usuarioExistente,
                cancellationToken);

        if (!validacion.Exitoso)
        {
            return validacion;
        }

        var nombreUsuario =
            modelo.NombreUsuario.Trim();

        if (await _usuarioRepository
                .ExisteNombreUsuarioAsync(
                    nombreUsuario,
                    modelo.Id,
                    cancellationToken))
        {
            return ResultadoOperacion.Fallido(
                "Ya existe otro usuario con ese nombre de usuario.");
        }

        var correo =
            NormalizarCorreo(modelo.Correo);

        if (!string.IsNullOrWhiteSpace(correo) &&
            await _usuarioRepository
                .ExisteCorreoAsync(
                    correo,
                    modelo.Id,
                    cancellationToken))
        {
            return ResultadoOperacion.Fallido(
                "Ya existe otro usuario con ese correo.");
        }

        usuarioExistente.Nombre =
            modelo.Nombre.Trim();

        usuarioExistente.Apellido =
            modelo.Apellido.Trim();

        usuarioExistente.NombreUsuario =
            nombreUsuario;

        usuarioExistente.Correo =
            correo;

        usuarioExistente.RolId =
            modelo.RolId;

        usuarioExistente.Activo =
            modelo.Activo;

        usuarioExistente.AlcanceGlobal =
            modelo.AlcanceGlobal;

        string? nuevoPasswordHash = null;

        if (!string.IsNullOrWhiteSpace(modelo.Password))
        {
            nuevoPasswordHash =
                _passwordHasher.HashPassword(
                    usuarioExistente,
                    modelo.Password);
        }

        var sucursalIds =
            modelo.AlcanceGlobal
                ? Array.Empty<int>()
                : ObtenerSucursalIds(modelo);

        await _usuarioRepository.ActualizarAsync(
            usuarioExistente,
            nuevoPasswordHash,
            sucursalIds,
            cancellationToken);

        return ResultadoOperacion.Ok(
            "Usuario actualizado correctamente.");
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(
        int usuarioId,
        bool activo,
        CancellationToken cancellationToken = default)
    {
        ExigirGestionUsuarios();

        if (usuarioId <= 0)
        {
            return ResultadoOperacion.Fallido(
                "El usuario indicado no es válido.");
        }

        var usuario =
            await _usuarioRepository.ObtenerPorIdAsync(
                usuarioId,
                cancellationToken);

        if (usuario is null)
        {
            return ResultadoOperacion.Fallido(
                "El usuario no existe.");
        }

        if (!PuedeGestionarUsuario(usuario))
        {
            return ResultadoOperacion.Fallido(
                "No tienes permisos para modificar este usuario.");
        }

        if (!activo &&
            await _usuarioRepository
                .EsUltimoSuperAdministradorActivoAsync(
                    usuarioId,
                    cancellationToken))
        {
            return ResultadoOperacion.Fallido(
                "No se puede desactivar al último SuperAdministrador activo.");
        }

        await _usuarioRepository.CambiarEstadoAsync(
            usuarioId,
            activo,
            cancellationToken);

        return ResultadoOperacion.Ok(
            activo
                ? "Usuario activado correctamente."
                : "Usuario desactivado correctamente.");
    }

    private async Task<ResultadoOperacion> ValidarAsync(
        UsuarioEditorDto modelo,
        bool esNuevo,
        Usuario? usuarioExistente,
        CancellationToken cancellationToken)
    {
        var nombre =
            modelo.Nombre?.Trim() ?? string.Empty;

        var apellido =
            modelo.Apellido?.Trim() ?? string.Empty;

        var nombreUsuario =
            modelo.NombreUsuario?.Trim() ?? string.Empty;

        var correo =
            NormalizarCorreo(modelo.Correo);

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return ResultadoOperacion.Fallido(
                "Debe indicar el nombre.");
        }

        if (nombre.Length > 100)
        {
            return ResultadoOperacion.Fallido(
                "El nombre no puede superar 100 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(apellido))
        {
            return ResultadoOperacion.Fallido(
                "Debe indicar el apellido.");
        }

        if (apellido.Length > 100)
        {
            return ResultadoOperacion.Fallido(
                "El apellido no puede superar 100 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(nombreUsuario))
        {
            return ResultadoOperacion.Fallido(
                "Debe indicar el nombre de usuario.");
        }

        if (nombreUsuario.Length < 3)
        {
            return ResultadoOperacion.Fallido(
                "El nombre de usuario debe tener al menos 3 caracteres.");
        }

        if (nombreUsuario.Length > 50)
        {
            return ResultadoOperacion.Fallido(
                "El nombre de usuario no puede superar 50 caracteres.");
        }

        if (correo is not null && correo.Length > 150)
        {
            return ResultadoOperacion.Fallido(
                "El correo no puede superar 150 caracteres.");
        }

        if (modelo.RolId <= 0)
        {
            return ResultadoOperacion.Fallido(
                "Debe seleccionar un rol.");
        }

        var roles =
            await _usuarioRepository.ObtenerRolesAsync(
                cancellationToken);

        var rolSeleccionado =
            roles.FirstOrDefault(
                r => r.Id == modelo.RolId);

        if (rolSeleccionado is null)
        {
            return ResultadoOperacion.Fallido(
                "El rol seleccionado no existe.");
        }

        if (!rolSeleccionado.Activo)
        {
            return ResultadoOperacion.Fallido(
                "No se puede asignar un rol inactivo.");
        }

        if (!PuedeAsignarRol(rolSeleccionado))
        {
            return ResultadoOperacion.Fallido(
                "No tienes permisos para asignar ese rol.");
        }

        if (!esNuevo &&
            usuarioExistente is not null &&
            EsSuperAdministrador(usuarioExistente.Rol))
        {
            return ResultadoOperacion.Fallido(
                "El SuperAdministrador no puede ser modificado por este usuario.");
        }

        if (esNuevo)
        {
            if (string.IsNullOrWhiteSpace(modelo.Password))
            {
                return ResultadoOperacion.Fallido(
                    "Debe indicar una contraseña.");
            }

            var validacionPassword =
                ValidarPassword(modelo.Password);

            if (!validacionPassword.Exitoso)
            {
                return validacionPassword;
            }
        }
        else if (!string.IsNullOrWhiteSpace(modelo.Password))
        {
            var validacionPassword =
                ValidarPassword(modelo.Password);

            if (!validacionPassword.Exitoso)
            {
                return validacionPassword;
            }
        }

        if (!_contextoUsuarioActual.AlcanceGlobal &&
            modelo.AlcanceGlobal)
        {
            return ResultadoOperacion.Fallido(
                "No tienes permisos para asignar alcance global.");
        }

        if (EsAdministradorActual() &&
            modelo.AlcanceGlobal)
        {
            return ResultadoOperacion.Fallido(
                "El Administrador no puede crear usuarios con alcance global.");
        }

        var sucursales =
            await _usuarioRepository
                .ObtenerSucursalesAsync(
                    cancellationToken);

        var sucursalIdsExistentes =
            sucursales
                .Select(s => s.Id)
                .ToHashSet();

        if (!modelo.AlcanceGlobal)
        {
            var seleccionadas =
                (modelo.SucursalIds ??
                    new HashSet<int>())
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            if (seleccionadas.Length == 0)
            {
                return ResultadoOperacion.Fallido(
                    "Debe asignar al menos una sucursal o activar el alcance global.");
            }

            if (seleccionadas.Any(
                    id => !sucursalIdsExistentes.Contains(id)))
            {
                return ResultadoOperacion.Fallido(
                    "Una o más sucursales seleccionadas no existen.");
            }

            if (!PuedeAsignarSucursales(seleccionadas))
            {
                return ResultadoOperacion.Fallido(
                    "Una o más sucursales seleccionadas están fuera de tu alcance.");
            }
        }

        return ResultadoOperacion.Ok(string.Empty);
    }

    private bool PuedeVerUsuario(Usuario usuario)
    {
        if (EsSuperAdministradorActual())
        {
            return true;
        }

        if (!EsAdministradorActual())
        {
            return false;
        }

        // El Administrador puede consultar cualquier usuario
        // administrable independientemente de su sucursal.
        if (EsSuperAdministrador(usuario.Rol))
        {
            return false;
        }

        return EsRolAdministrablePorAdministrador(usuario.Rol);
    }

    private bool PuedeGestionarUsuario(Usuario usuario)
    {
        if (EsSuperAdministradorActual())
        {
            return true;
        }

        if (!EsAdministradorActual())
        {
            return false;
        }

        if (EsSuperAdministrador(usuario.Rol))
        {
            return false;
        }

        if (!EsRolAdministrablePorAdministrador(usuario.Rol))
        {
            return false;
        }

        // Modificar sí queda limitado al alcance del Administrador.
        return PuedeAccederAlUsuarioPorSucursal(usuario);
    }

    private bool PuedeAsignarRol(Rol rol)
    {
        if (EsSuperAdministradorActual())
        {
            return true;
        }

        if (!EsAdministradorActual())
        {
            return false;
        }

        return EsRolAdministrablePorAdministrador(rol);
    }

    private bool PuedeAccederAlUsuarioPorSucursal(
        Usuario usuario)
    {
        if (_contextoUsuarioActual.AlcanceGlobal)
        {
            return true;
        }

        if (usuario.AlcanceGlobal)
        {
            return false;
        }

        if (usuario.UsuarioSucursales.Count == 0)
        {
            return false;
        }

        return usuario.UsuarioSucursales
            .Any(us =>
                _contextoUsuarioActual
                    .PuedeAccederASucursal(
                        us.SucursalId));
    }

    private bool PuedeAsignarSucursales(
        IEnumerable<int> sucursalIds)
    {
        if (_contextoUsuarioActual.AlcanceGlobal)
        {
            return true;
        }

        return sucursalIds.All(
            _contextoUsuarioActual
                .PuedeAccederASucursal);
    }

    private bool EsSuperAdministradorActual()
    {
        return string.Equals(
            _contextoUsuarioActual.Rol,
            RolSuperAdministrador,
            StringComparison.OrdinalIgnoreCase);
    }

    private bool EsAdministradorActual()
    {
        return string.Equals(
            _contextoUsuarioActual.Rol,
            RolAdministrador,
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool EsSuperAdministrador(Rol? rol)
    {
        return rol?.Id == RolSuperAdministradorId;
    }

    private static bool EsRolAdministrablePorAdministrador(
        Rol? rol)
    {
        if (rol is null || !rol.Activo)
        {
            return false;
        }

        return rol.Id == RolAdministradorId ||
               rol.Id == RolEncargadoSucursalId ||
               rol.Id == RolVendedorCajeroId ||
               rol.Id == RolAlmacenInventarioId;
    }

    private void ExigirAccesoAlModuloUsuarios()
    {
        if (!EsSuperAdministradorActual() &&
            !EsAdministradorActual())
        {
            throw new UnauthorizedAccessException(
                "Solo el SuperAdministrador y el Administrador pueden acceder al módulo de usuarios.");
        }

        if (!_contextoUsuarioActual
                .TienePermiso(PermisosSistema.UsuariosConsultar) &&
            !_contextoUsuarioActual
                .TienePermiso(PermisosSistema.UsuariosGestionar))
        {
            throw new UnauthorizedAccessException(
                "El usuario actual no tiene permisos para consultar usuarios.");
        }
    }

    private void ExigirGestionUsuarios()
    {
        if (!EsSuperAdministradorActual() &&
            !EsAdministradorActual())
        {
            throw new UnauthorizedAccessException(
                "Solo el SuperAdministrador y el Administrador pueden gestionar usuarios.");
        }

        if (!_contextoUsuarioActual
                .TienePermiso(PermisosSistema.UsuariosGestionar))
        {
            throw new UnauthorizedAccessException(
                "El usuario actual no tiene permisos para gestionar usuarios.");
        }
    }

    private static int[] ObtenerSucursalIds(
        UsuarioEditorDto modelo)
    {
        return (modelo.SucursalIds ??
                new HashSet<int>())
            .Where(id => id > 0)
            .Distinct()
            .ToArray();
    }

    private static ResultadoOperacion ValidarPassword(
        string password)
    {
        if (password.Length < 8)
        {
            return ResultadoOperacion.Fallido(
                "La contraseña debe tener al menos 8 caracteres.");
        }

        if (password.Length > 100)
        {
            return ResultadoOperacion.Fallido(
                "La contraseña no puede superar 100 caracteres.");
        }

        if (!password.Any(char.IsUpper))
        {
            return ResultadoOperacion.Fallido(
                "La contraseña debe contener al menos una letra mayúscula.");
        }

        if (!password.Any(char.IsLower))
        {
            return ResultadoOperacion.Fallido(
                "La contraseña debe contener al menos una letra minúscula.");
        }

        if (!password.Any(char.IsDigit))
        {
            return ResultadoOperacion.Fallido(
                "La contraseña debe contener al menos un número.");
        }

        return ResultadoOperacion.Ok(string.Empty);
    }

    private static string? NormalizarCorreo(string? correo)
    {
        if (string.IsNullOrWhiteSpace(correo))
        {
            return null;
        }

        return correo.Trim();
    }
}
