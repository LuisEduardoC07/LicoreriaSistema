using LicoreriaSistema.Datos.Data;
using LicoreriaSistema.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace LicoreriaSistema.Datos.Context;

public class LicoreriaDbContext : DbContext
{
    public LicoreriaDbContext(DbContextOptions<LicoreriaDbContext> options)
        : base(options)
    {
    }

    // Negocio
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<InventarioSucursal> InventariosSucursal => Set<InventarioSucursal>();

    public DbSet<ClasificacionFiscal> ClasificacionesFiscales
        => Set<ClasificacionFiscal>();

    public DbSet<ReglaImpuesto> ReglasImpuesto
        => Set<ReglaImpuesto>();

    // Seguridad
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<UsuarioSucursal> UsuariosSucursales => Set<UsuarioSucursal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================================================
        // SUCURSAL
        // =========================================================

        modelBuilder.Entity<Sucursal>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Nombre)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Direccion)
                .HasMaxLength(250);

            entity.Property(x => x.Telefono)
                .HasMaxLength(30);
        });

        // =========================================================
        // CATEGORIA
        // =========================================================

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Descripcion)
                .HasMaxLength(250);
        });

        // =========================================================
        // PRODUCTO
        // =========================================================

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Nombre)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Codigo)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => x.Codigo)
                .IsUnique();

            entity.Property(x => x.PrecioCompra)
                .HasPrecision(18, 2);

            entity.Property(x => x.PrecioVenta)
                .HasPrecision(18, 2);

            entity.HasOne(x => x.Categoria)
                .WithMany(x => x.Productos)
                .HasForeignKey(x => x.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================================================
        // =========================================================
        // CLASIFICACION FISCAL
        // =========================================================

        modelBuilder.Entity<ClasificacionFiscal>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Descripcion)
                .HasMaxLength(250);

            entity.HasIndex(x => x.Nombre)
                .IsUnique();

            entity.HasMany(x => x.Productos)
                .WithOne(x => x.ClasificacionFiscal)
                .HasForeignKey(x => x.ClasificacionFiscalId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.ReglasImpuesto)
                .WithOne(x => x.ClasificacionFiscal)
                .HasForeignKey(x => x.ClasificacionFiscalId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================================================
        // REGLA DE IMPUESTO
        // =========================================================

        modelBuilder.Entity<ReglaImpuesto>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TipoImpuesto)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.TasaAdValorem)
                .HasPrecision(18, 6);

            entity.Property(x => x.MontoEspecifico)
                .HasPrecision(18, 6);

            entity.Property(x => x.UnidadCalculo)
                .HasMaxLength(100);

            entity.HasIndex(x => new
            {
                x.ClasificacionFiscalId,
                x.TipoImpuesto,
                x.FechaInicio
            });
        });

        // =========================================================
        // INVENTARIO POR SUCURSAL
        // =========================================================

        modelBuilder.Entity<InventarioSucursal>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Cantidad)
                .HasPrecision(18, 2);

            entity.Property(x => x.StockMinimo)
                .HasPrecision(18, 2);

            entity.Property(x => x.StockMaximo)
                .HasPrecision(18, 2);

            entity.HasOne(x => x.Producto)
                .WithMany(x => x.Inventarios)
                .HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Sucursal)
                .WithMany(x => x.Inventarios)
                .HasForeignKey(x => x.SucursalId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.ProductoId,
                x.SucursalId
            })
            .IsUnique();
        });

        // =========================================================
        // ROL
        // =========================================================

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Descripcion)
                .HasMaxLength(250);

            entity.HasIndex(x => x.Nombre)
                .IsUnique();
        });

        // =========================================================
        // PERMISO
        // =========================================================

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Codigo)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Descripcion)
                .HasMaxLength(250);

            entity.HasIndex(x => x.Codigo)
                .IsUnique();

            entity.HasIndex(x => x.Nombre)
                .IsUnique();
        });

        // =========================================================
        // ROL - PERMISO
        // =========================================================

        modelBuilder.Entity<RolPermiso>(entity =>
        {
            entity.HasKey(x => new
            {
                x.RolId,
                x.PermisoId
            });

            entity.HasOne(x => x.Rol)
                .WithMany(x => x.RolPermisos)
                .HasForeignKey(x => x.RolId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Permiso)
                .WithMany(x => x.RolPermisos)
                .HasForeignKey(x => x.PermisoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================================================
        // USUARIO
        // =========================================================

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Apellido)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.NombreUsuario)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Correo)
                .HasMaxLength(150);

            entity.Property(x => x.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            entity.HasIndex(x => x.NombreUsuario)
                .IsUnique();

            entity.HasIndex(x => x.Correo)
                .IsUnique()
                .HasFilter("[Correo] IS NOT NULL");

            entity.HasOne(x => x.Rol)
                .WithMany(x => x.Usuarios)
                .HasForeignKey(x => x.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================================================
        // USUARIO - SUCURSAL
        // =========================================================

        modelBuilder.Entity<UsuarioSucursal>(entity =>
        {
            entity.HasKey(x => new
            {
                x.UsuarioId,
                x.SucursalId
            });

            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.UsuarioSucursales)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Sucursal)
                .WithMany()
                .HasForeignKey(x => x.SucursalId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================================================
        // DATOS INICIALES
        // =========================================================

        SeedData.Seed(modelBuilder);
    }

}
