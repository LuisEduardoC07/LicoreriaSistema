using System.Security.Claims;
using LibreriaSistema.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace LicoreriaSistema.Web.Endpoints;

public static class AutenticacionEndpoints
{
    public static IEndpointRouteBuilder MapAutenticacionEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/auth/login", async (
            HttpContext httpContext,
            IAutenticacionService autenticacionService) =>
        {
            var form = await httpContext.Request.ReadFormAsync();

            var nombreUsuario =
                form["NombreUsuario"].ToString();

            var password =
                form["Password"].ToString();

            var returnUrl =
                form["ReturnUrl"].ToString();

            var resultado =
                await autenticacionService.AutenticarAsync(
                    nombreUsuario,
                    password);

            if (!resultado.Exitoso)
            {
                var destino = "/login";

                if (!string.IsNullOrWhiteSpace(returnUrl))
                {
                    destino +=
                        $"?returnUrl={Uri.EscapeDataString(returnUrl)}";
                }

                return Results.Redirect(destino);
            }

            var claims = new List<Claim>
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
                    resultado.AlcanceGlobal.ToString())
            };

            foreach (var permiso in resultado.Permisos)
            {
                claims.Add(
                    new Claim("Permiso", permiso));
            }

            foreach (var sucursalId in resultado.SucursalIds)
            {
                claims.Add(
                    new Claim(
                        "SucursalId",
                        sucursalId.ToString()));
            }

            var identidad = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identidad);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            if (string.IsNullOrWhiteSpace(returnUrl) ||
                !Uri.TryCreate(
                    returnUrl,
                    UriKind.Relative,
                    out _) ||
                !returnUrl.StartsWith("/"))
            {
                returnUrl = "/";
            }

            return Results.Redirect(returnUrl);
        });

        endpoints.MapPost("/api/auth/logout", async (
            HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return Results.Redirect("/login");
        });

        return endpoints;
    }
}
