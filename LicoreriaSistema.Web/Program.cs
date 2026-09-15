using LicoreriaSistema.Datos.Context;
using LicoreriaSistema.Datos.Data;
using LicoreriaSistema.Datos.Extensions;
using LicoreriaSistema.Web.Components;
using LicoreriaSistema.Web.Endpoints;
using LicoreriaSistema.Web.Seguridad;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatos(builder.Configuration);

// ============================================================
// AUTENTICACION Y AUTORIZACION
// ============================================================

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/acceso-denegado";
        options.Cookie.Name = "LicoreriaSistema.Auth";

        // Duracion maxima de la sesion.
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

        // No renovar automaticamente por actividad.
        options.SlidingExpiration = false;
    });

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<
    AuthenticationStateProvider,
    RevalidatingAuthenticationStateProvider>();

// ============================================================
// RAZOR COMPONENTS
// ============================================================

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// ============================================================
// BASE DE DATOS Y DATOS INICIALES
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
            .GetRequiredService<LicoreriaDbContext>();

    await InicializadorDatos.InicializarAsync(context);
}

// ============================================================
// PIPELINE HTTP
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

// Recuperar la identidad desde la cookie.
app.UseAuthentication();

// ============================================================
// BARRERA GLOBAL
// ============================================================
//
// Ninguna navegacion HTML de un usuario no autenticado puede
// mostrar la aplicacion.
//
// Permitimos unicamente:
//   /login
//   /api/auth/login
//
// El resto de las solicitudes HTML se redirige al login.
//
// Los recursos estaticos y solicitudes internas no HTML no se
// bloquean en esta barrera.
// ============================================================

app.Use(async (context, next) =>
{
    var path = context.Request.Path;

    var esLogin =
        path.Equals(
            "/login",
            StringComparison.OrdinalIgnoreCase);

    var esLoginApi =
        path.Equals(
            "/api/auth/login",
            StringComparison.OrdinalIgnoreCase);

    var usuarioAutenticado =
        context.User.Identity?.IsAuthenticated == true;

    var metodoPagina =
        HttpMethods.IsGet(context.Request.Method) ||
        HttpMethods.IsHead(context.Request.Method);

    var acceptHeader =
        context.Request.Headers.Accept.ToString();

    var aceptaHtml =
        acceptHeader.Contains(
            "text/html",
            StringComparison.OrdinalIgnoreCase);

    if (!usuarioAutenticado &&
        !esLogin &&
        !esLoginApi &&
        metodoPagina &&
        aceptaHtml)
    {
        var returnUrl =
            $"{context.Request.Path}{context.Request.QueryString}";

        if (!EsUrlLocal(returnUrl))
        {
            returnUrl = "/";
        }

        var destino =
            $"/login?returnUrl={Uri.EscapeDataString(returnUrl)}";

        context.Response.Redirect(destino);

        return;
    }

    await next();
});

app.UseAuthorization();

app.UseAntiforgery();

// ============================================================
// ENDPOINTS
// ============================================================

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAutenticacionEndpoints();

app.Run();

// ============================================================
// VALIDACION DE RETURN URL
// ============================================================

static bool EsUrlLocal(string? url)
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

    // Evita //servidor-externo.com
    if (url.StartsWith(
            "//",
            StringComparison.Ordinal))
    {
        return false;
    }

    // Evita /\servidor-externo.com
    if (url.StartsWith(
            "/\\",
            StringComparison.Ordinal))
    {
        return false;
    }

    // Debe ser una URL relativa.
    return !Uri.TryCreate(
        url,
        UriKind.Absolute,
        out _);
}
