using System.Security.Claims;
using LibreriaSistema.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace LicoreriaSistema.Web.Seguridad;

public sealed class ContextoUsuarioActual
    : IContextoUsuarioActual
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    private ClaimsPrincipal? _usuario;
    private bool _cargado;

    public ContextoUsuarioActual(
        AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider =
            authenticationStateProvider;
    }

    public int UsuarioId =>
        ObtenerUsuarioId();

    public string? NombreUsuario =>
        ObtenerClaim(ClaimTypes.Name);

    public string? Rol =>
        ObtenerClaim(ClaimTypes.Role);

    public bool AlcanceGlobal =>
        bool.TryParse(
            ObtenerClaim("AlcanceGlobal"),
            out var global)
            && global;

    public IReadOnlySet<int> SucursalIds =>
        ObtenerSucursalIds();

    public bool EstaAutenticado =>
        ObtenerUsuario().Identity?.IsAuthenticated == true;

    public bool TienePermiso(string permiso)
    {
        if (string.IsNullOrWhiteSpace(permiso))
        {
            return false;
        }

        return ObtenerUsuario()
            .Claims
            .Any(c =>
                string.Equals(
                    c.Type,
                    "Permiso",
                    StringComparison.OrdinalIgnoreCase)
                &&
                string.Equals(
                    c.Value,
                    permiso,
                    StringComparison.OrdinalIgnoreCase));
    }

    public bool PuedeAccederASucursal(int sucursalId)
    {
        if (!EstaAutenticado || sucursalId <= 0)
        {
            return false;
        }

        if (AlcanceGlobal)
        {
            return true;
        }

        return SucursalIds.Contains(sucursalId);
    }

    private ClaimsPrincipal ObtenerUsuario()
    {
        if (_cargado && _usuario is not null)
        {
            return _usuario;
        }

        var estado =
            _authenticationStateProvider
                .GetAuthenticationStateAsync()
                .GetAwaiter()
                .GetResult();

        _usuario = estado.User;
        _cargado = true;

        return _usuario;
    }

    private string? ObtenerClaim(string tipo)
    {
        return ObtenerUsuario()
            .FindFirst(tipo)
            ?.Value;
    }

    private int ObtenerUsuarioId()
    {
        var valor =
            ObtenerClaim(ClaimTypes.NameIdentifier);

        return int.TryParse(valor, out var id)
            ? id
            : 0;
    }

    private IReadOnlySet<int> ObtenerSucursalIds()
    {
        var ids =
            ObtenerUsuario()
                .FindAll("SucursalId")
                .Select(c =>
                    int.TryParse(c.Value, out var id)
                        ? id
                        : 0)
                .Where(id => id > 0)
                .ToHashSet();

        return ids;
    }
}
