using LibreriaSistema.Aplicacion.Interfaces;
using LibreriaSistema.Aplicacion.Servicios;
using LicoreriaSistema.Datos.Context;
using LicoreriaSistema.Datos.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LicoreriaSistema.Datos.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddDatos(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LicoreriaDb")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'LicoreriaDb'.");

        services.AddDbContext<LicoreriaDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<CategoriaService>();
        services.AddScoped<IAutenticacionService, AutenticacionService>();

        return services;
    }
}