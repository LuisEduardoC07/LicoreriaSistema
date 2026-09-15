using LicoreriaSistema.Datos.Context;
using LicoreriaSistema.Dominio.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LicoreriaSistema.Datos.Data;

public static class InicializadorDatos
{
    public static async Task InicializarAsync(
        LicoreriaDbContext context)
    {
        await context.Database.MigrateAsync();

        await InicializarSucursalesAsync(context);

        await InicializarAdministradorAsync(context);
    }

    private static async Task InicializarSucursalesAsync(
        LicoreriaDbContext context)
    {
        var sucursalesDemostracion = new[]
        {
            new Sucursal
            {
                Nombre = "Licorería Sistema - Higüey Centro",
                Direccion = "Avenida Libertad, Higüey, La Altagracia, República Dominicana",
                Telefono = "809-000-0001",
                Activa = true
            },
            new Sucursal
            {
                Nombre = "Licorería Sistema - Juan XXIII",
                Direccion = "Avenida Juan XXIII, Higüey, La Altagracia, República Dominicana",
                Telefono = "809-000-0002",
                Activa = true
            },
            new Sucursal
            {
                Nombre = "Licorería Sistema - Agustín Guerrero",
                Direccion = "Avenida Agustín Guerrero, Higüey, La Altagracia, República Dominicana",
                Telefono = "809-000-0003",
                Activa = true
            },
            new Sucursal
            {
                Nombre = "Licorería Sistema - Beller",
                Direccion = "Calle Beller, Higüey, La Altagracia, República Dominicana",
                Telefono = "809-000-0004",
                Activa = true
            },
            new Sucursal
            {
                Nombre = "Licorería Sistema - Hermanos Trejos",
                Direccion = "Avenida Hermanos Trejos, Higüey, La Altagracia, República Dominicana",
                Telefono = "809-000-0005",
                Activa = true
            }
        };

        foreach (var sucursalDemo in sucursalesDemostracion)
        {
            var existe = await context.Sucursales
                .AnyAsync(
                    s => s.Nombre == sucursalDemo.Nombre);

            if (!existe)
            {
                context.Sucursales.Add(sucursalDemo);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task InicializarAdministradorAsync(
        LicoreriaDbContext context)
    {
        var superAdministradorExiste =
            await context.Usuarios
                .AnyAsync(u => u.NombreUsuario == "admin");

        if (superAdministradorExiste)
        {
            return;
        }

        var rolSuperAdministrador =
            await context.Roles
                .SingleAsync(r => r.Id == 1);

        var usuario = new Usuario
        {
            Nombre = "Administrador",
            Apellido = "Principal",
            NombreUsuario = "admin",
            Correo = "admin@licoreriasistema.local",
            RolId = rolSuperAdministrador.Id,
            Activo = true,
            AlcanceGlobal = true
        };

        var passwordHasher =
            new PasswordHasher<Usuario>();

        usuario.PasswordHash =
            passwordHasher.HashPassword(
                usuario,
                "Admin123!");

        context.Usuarios.Add(usuario);

        await context.SaveChangesAsync();
    }
}
