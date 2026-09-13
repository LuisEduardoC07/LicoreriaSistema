using LicoreriaSistema.Datos.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LicoreriaSistema.Datos.Factories;

public class LicoreriaDbContextFactory : IDesignTimeDbContextFactory<LicoreriaDbContext>
{
    public LicoreriaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LicoreriaDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=LicoreriaSistemaDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");

        return new LicoreriaDbContext(optionsBuilder.Options);
    }
}