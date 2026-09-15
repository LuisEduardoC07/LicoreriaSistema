using LibreriaSistema.Aplicacion.Interfaces;
using LibreriaSistema.Aplicacion.Modelos;
using LicoreriaSistema.Dominio.Entidades;
using Microsoft.AspNetCore.Identity;

namespace LibreriaSistema.Aplicacion.Servicios;

public class AutenticacionService : IAutenticacionService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly PasswordHasher<Usuario> _passwordHasher;

    public AutenticacionService(
        IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = new PasswordHasher<Usuario>();
    }

    public async Task<ResultadoAutenticacion> AutenticarAsync(
        string nombreUsuario,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario))
        {
            return ResultadoAutenticacion.Fallido(
                "Debe indicar el nombre de usuario.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return ResultadoAutenticacion.Fallido(
                "Debe indicar la contraseña.");
        }

        var usuario = await _usuarioRepository
            .ObtenerParaAutenticacionAsync(
                nombreUsuario.Trim(),
                cancellationToken);

        if (usuario is null)
        {
            return ResultadoAutenticacion.Fallido(
                "El usuario o la contraseña son incorrectos.");
        }

        if (!usuario.Activo)
        {
            return ResultadoAutenticacion.Fallido(
                "El usuario está inactivo.");
        }

        var resultadoPassword = _passwordHasher.VerifyHashedPassword(
            usuario,
            usuario.PasswordHash,
            password);

        if (resultadoPassword == PasswordVerificationResult.Failed)
        {
            return ResultadoAutenticacion.Fallido(
                "El usuario o la contraseña son incorrectos.");
        }

        var permisos = usuario.Rol.RolPermisos
            .Where(rp => rp.Permiso.Activo)
            .Select(rp => rp.Permiso.Codigo)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var sucursalIds = usuario.UsuarioSucursales
            .Select(us => us.SucursalId)
            .Distinct()
            .ToArray();

        return new ResultadoAutenticacion
        {
            Exitoso = true,
            UsuarioId = usuario.Id,
            NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}".Trim(),
            NombreUsuario = usuario.NombreUsuario,
            Rol = usuario.Rol.Nombre,
            AlcanceGlobal = usuario.AlcanceGlobal,
            SucursalIds = sucursalIds,
            Permisos = permisos
        };
    }
}