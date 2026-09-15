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
                "No se encontrÃ³ la cadena de conexiÃ³n 'LicoreriaDb'.");

        services.AddDbContext<LicoreriaDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<CategoriaService>();
        services.AddScoped<ProductoService>();
        services.AddScoped<IAutenticacionService, AutenticacionService>();

        return services;
    }
}
