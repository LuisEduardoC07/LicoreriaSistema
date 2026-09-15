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

        var superAdministradorExiste = await context.Usuarios
            .AnyAsync(u => u.NombreUsuario == "admin");

        if (superAdministradorExiste)
        {
            return;
        }

        var rolSuperAdministrador = await context.Roles
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

        var passwordHasher = new PasswordHasher<Usuario>();

        usuario.PasswordHash = passwordHasher.HashPassword(
            usuario,
            "Admin123!");

        context.Usuarios.Add(usuario);

        await context.SaveChangesAsync();
    }
}