using System.Security.Claims;
using LibreriaSistema.Aplicacion.Modelos;
using Microsoft.AspNetCore.Components.Authorization;

namespace LicoreriaSistema.Web.Seguridad;

public class SesionAuthenticationStateProvider
    : AuthenticationStateProvider
{
    private ClaimsPrincipal _usuarioActual =
        new(new ClaimsIdentity());

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(
            new AuthenticationState(_usuarioActual));
    }

    public void IniciarSesion(ResultadoAutenticacion resultado)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, resultado.UsuarioId.ToString()),
            new(ClaimTypes.Name, resultado.NombreUsuario),
            new("NombreCompleto", resultado.NombreCompleto),
            new(ClaimTypes.Role, resultado.Rol),
            new("AlcanceGlobal", resultado.AlcanceGlobal.ToString())
        };

        foreach (var permiso in resultado.Permisos)
        {
            claims.Add(new Claim("Permiso", permiso));
        }

        foreach (var sucursalId in resultado.SucursalIds)
        {
            claims.Add(new Claim("SucursalId", sucursalId.ToString()));
        }

        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "LicoreriaSistema");

        _usuarioActual = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(
            Task.FromResult(
                new AuthenticationState(_usuarioActual)));
    }

    public void CerrarSesion()
    {
        _usuarioActual = new ClaimsPrincipal(
            new ClaimsIdentity());

        NotifyAuthenticationStateChanged(
            Task.FromResult(
                new AuthenticationState(_usuarioActual)));
    }
}