using LicoreriaSistema.Datos.Context;
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

        return services;
    }
}