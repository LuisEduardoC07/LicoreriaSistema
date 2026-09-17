using System.Security.Claims;
using LibreriaSistema.Aplicacion.Interfaces;
using LibreriaSistema.Aplicacion.Modelos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace LicoreriaSistema.Web.Endpoints;

public static class AutenticacionEndpoints
{
    private static readonly TimeSpan DuracionSesion =
        TimeSpan.FromMinutes(5);

    public static IEndpointRouteBuilder MapAutenticacionEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/auth/login", async (
            HttpContext httpContext,
            IAutenticacionService autenticacionService) =>
        {
            var form =
                await httpContext.Request.ReadFormAsync();

            var nombreUsuario =
                form["NombreUsuario"].ToString();

            var password =
                form["Password"].ToString();

            var returnUrl =
                form["ReturnUrl"].ToString();

            if (!EsUrlLocal(returnUrl))
            {
                returnUrl = "/";
            }

            var resultado =
                await autenticacionService.AutenticarAsync(
                    nombreUsuario,
                    password);

            if (!resultado.Exitoso)
            {
                var destino = "/login";

                var parametros =
                    new List<string>();

                if (!string.IsNullOrWhiteSpace(returnUrl) &&
                    returnUrl != "/")
                {
                    parametros.Add(
                        $"returnUrl={Uri.EscapeDataString(returnUrl)}");
                }

                parametros.Add(
                    "error=credenciales");

                destino +=
                    "?" + string.Join("&", parametros);

                return Results.Redirect(destino);
            }

            var fechaExpiracion =
                DateTimeOffset.UtcNow.Add(
                    DuracionSesion);

            var claims =
                CrearClaims(
                    resultado,
                    fechaExpiracion);

            var identidad =
                new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

            var principal =
                new ClaimsPrincipal(identidad);

            var propiedades =
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = true,
                    ExpiresUtc = fechaExpiracion
                };

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                propiedades);

            return Results.LocalRedirect(
                returnUrl);
        });

        endpoints.MapPost(
            "/api/auth/actividad",
            async (HttpContext httpContext) =>
            {
                if (httpContext.User.Identity?.IsAuthenticated != true)
                {
                    return Results.Unauthorized();
                }

                var claims =
                    httpContext.User.Claims
                        .Where(c =>
                            c.Type != "SesionExpiraUtc")
                        .ToList();

                var fechaExpiracion =
                    DateTimeOffset.UtcNow.Add(
                        DuracionSesion);

                claims.Add(
                    new Claim(
                        "SesionExpiraUtc",
                        fechaExpiracion.ToString("O")));

                var identidad =
                    new ClaimsIdentity(
                        claims,
                        CookieAuthenticationDefaults.AuthenticationScheme);

                var principal =
                    new ClaimsPrincipal(
                        identidad);

                var propiedades =
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        AllowRefresh = true,
                        ExpiresUtc = fechaExpiracion
                    };

                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    propiedades);

                return Results.NoContent();
            })
            .RequireAuthorization();

        endpoints.MapPost(
            "/api/auth/logout",
            async (HttpContext httpContext) =>
            {
                await httpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return Results.Redirect(
                    "/login");
            });

        return endpoints;
    }

    private static List<Claim> CrearClaims(
        ResultadoAutenticacion resultado,
        DateTimeOffset fechaExpiracion)
    {
        var claims =
            new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    resultado.UsuarioId.ToString()),

                new(
                    ClaimTypes.Name,
                    resultado.NombreUsuario),

                new(
                    "NombreCompleto",
                    resultado.NombreCompleto),

                new(
                    ClaimTypes.Role,
                    resultado.Rol),

                new(
                    "AlcanceGlobal",
                    resultado.AlcanceGlobal.ToString()),

                new(
                    "SesionExpiraUtc",
                    fechaExpiracion.ToString("O"))
            };

        foreach (var permiso in resultado.Permisos)
        {
            claims.Add(
                new Claim(
                    "Permiso",
                    permiso));
        }

        foreach (var sucursalId in resultado.SucursalIds)
        {
            claims.Add(
                new Claim(
                    "SucursalId",
                    sucursalId.ToString()));
        }

        return claims;
    }

    private static bool EsUrlLocal(
        string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (!url.StartsWith(
                "/",
                StringComparison.Ordinal))
        {
            return false;
        }

        if (url.StartsWith(
                "//",
                StringComparison.Ordinal))
        {
            return false;
        }

        if (url.StartsWith(
                "/\\",
                StringComparison.Ordinal))
        {
            return false;
        }

        return !Uri.TryCreate(
            url,
            UriKind.Absolute,
            out _);
    }
}
