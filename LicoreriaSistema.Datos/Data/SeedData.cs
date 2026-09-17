using LicoreriaSistema.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace LicoreriaSistema.Datos.Data;

public static class SeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        // =========================================================
        // ROLES
        // =========================================================

        var superAdministradorId = 1;
        var administradorId = 2;
        var encargadoSucursalId = 3;
        var vendedorCajeroId = 4;
        var almacenInventarioId = 5;

        modelBuilder.Entity<Rol>().HasData(
            new Rol
            {
                Id = superAdministradorId,
                Nombre = "SuperAdministrador",
                Descripcion = "Control total del sistema y del negocio.",
                Activo = true
            },
            new Rol
            {
                Id = administradorId,
                Nombre = "Administrador",
                Descripcion = "Administración del negocio dentro de su alcance.",
                Activo = true
            },
            new Rol
            {
                Id = encargadoSucursalId,
                Nombre = "Encargado de Sucursal",
                Descripcion = "Supervisión operativa de una sucursal.",
                Activo = true
            },
            new Rol
            {
                Id = vendedorCajeroId,
                Nombre = "Vendedor/Cajero",
                Descripcion = "Ventas, cobros y atención al cliente.",
                Activo = true
            },
            new Rol
            {
                Id = almacenInventarioId,
                Nombre = "Almacén/Inventario",
                Descripcion = "Gestión y control del inventario.",
                Activo = true
            }
        );

        // =========================================================
        // PERMISOS
        // =========================================================

        var permisos = new[]
        {
            // Consultas
            new Permiso
            {
                Id = 1,
                Codigo = "PRODUCTOS_CONSULTAR",
                Nombre = "Consultar productos",
                Descripcion = "Permite consultar productos y precios.",
                Activo = true
            },
            new Permiso
            {
                Id = 2,
                Codigo = "INVENTARIO_CONSULTAR",
                Nombre = "Consultar inventario",
                Descripcion = "Permite consultar existencias.",
                Activo = true
            },
            new Permiso
            {
                Id = 3,
                Codigo = "VENTAS_CONSULTAR",
                Nombre = "Consultar ventas",
                Descripcion = "Permite consultar ventas e historial.",
                Activo = true
            },
            new Permiso
            {
                Id = 4,
                Codigo = "CLIENTES_CONSULTAR",
                Nombre = "Consultar clientes",
                Descripcion = "Permite consultar clientes registrados.",
                Activo = true
            },

            // Productos y categorías
            new Permiso
            {
                Id = 5,
                Codigo = "PRODUCTOS_GESTIONAR",
                Nombre = "Gestionar productos",
                Descripcion = "Crear, modificar y administrar productos.",
                Activo = true
            },
            new Permiso
            {
                Id = 6,
                Codigo = "CATEGORIAS_GESTIONAR",
                Nombre = "Gestionar categorías",
                Descripcion = "Crear, modificar y administrar categorías.",
                Activo = true
            },

            // Inventario
            new Permiso
            {
                Id = 7,
                Codigo = "INVENTARIO_ENTRADA",
                Nombre = "Registrar entradas de inventario",
                Descripcion = "Permite registrar entradas de mercancía.",
                Activo = true
            },
            new Permiso
            {
                Id = 8,
                Codigo = "INVENTARIO_SALIDA",
                Nombre = "Registrar salidas de inventario",
                Descripcion = "Permite registrar salidas de mercancía.",
                Activo = true
            },
            new Permiso
            {
                Id = 9,
                Codigo = "INVENTARIO_AJUSTAR",
                Nombre = "Ajustar inventario",
                Descripcion = "Permite realizar ajustes de inventario.",
                Activo = true
            },

            // Transferencias
            new Permiso
            {
                Id = 10,
                Codigo = "TRANSFERENCIAS_CREAR",
                Nombre = "Crear transferencias",
                Descripcion = "Permite crear transferencias entre sucursales.",
                Activo = true
            },
            new Permiso
            {
                Id = 11,
                Codigo = "TRANSFERENCIAS_AUTORIZAR",
                Nombre = "Autorizar transferencias",
                Descripcion = "Permite autorizar transferencias.",
                Activo = true
            },
            new Permiso
            {
                Id = 12,
                Codigo = "TRANSFERENCIAS_ENVIAR",
                Nombre = "Enviar transferencias",
                Descripcion = "Permite ejecutar el envío de mercancía.",
                Activo = true
            },
            new Permiso
            {
                Id = 13,
                Codigo = "TRANSFERENCIAS_RECIBIR",
                Nombre = "Recibir transferencias",
                Descripcion = "Permite confirmar la recepción de mercancía.",
                Activo = true
            },

            // Ventas / POS
            new Permiso
            {
                Id = 14,
                Codigo = "VENTAS_CREAR",
                Nombre = "Crear ventas",
                Descripcion = "Permite registrar ventas.",
                Activo = true
            },
            new Permiso
            {
                Id = 15,
                Codigo = "VENTAS_ANULAR",
                Nombre = "Anular ventas",
                Descripcion = "Permite anular ventas según las reglas de autorización.",
                Activo = true
            },

            // Clientes / POS
            new Permiso
            {
                Id = 16,
                Codigo = "CLIENTES_CREAR",
                Nombre = "Registrar clientes",
                Descripcion = "Permite registrar clientes durante la operación.",
                Activo = true
            },
            new Permiso
            {
                Id = 17,
                Codigo = "CLIENTES_EDITAR",
                Nombre = "Editar clientes",
                Descripcion = "Permite modificar datos de clientes.",
                Activo = true
            },

            // Caja / POS
            new Permiso
            {
                Id = 18,
                Codigo = "CAJA_OPERAR",
                Nombre = "Operar caja",
                Descripcion = "Permite realizar operaciones de caja y cobro.",
                Activo = true
            },
            new Permiso
            {
                Id = 19,
                Codigo = "CAJA_CIERRE",
                Nombre = "Realizar cierre de caja",
                Descripcion = "Permite realizar el cierre de caja propio.",
                Activo = true
            },

            // Usuarios
            new Permiso
            {
                Id = 20,
                Codigo = "USUARIOS_CONSULTAR",
                Nombre = "Consultar usuarios",
                Descripcion = "Permite consultar usuarios.",
                Activo = true
            },
            new Permiso
            {
                Id = 21,
                Codigo = "USUARIOS_GESTIONAR",
                Nombre = "Gestionar usuarios",
                Descripcion = "Permite crear, modificar y administrar usuarios dentro del alcance permitido.",
                Activo = true
            },

            // Sucursales
            new Permiso
            {
                Id = 22,
                Codigo = "SUCURSALES_CONSULTAR",
                Nombre = "Consultar sucursales",
                Descripcion = "Permite consultar sucursales.",
                Activo = true
            },
            new Permiso
            {
                Id = 23,
                Codigo = "SUCURSALES_GESTIONAR",
                Nombre = "Gestionar sucursales",
                Descripcion = "Permite crear y modificar sucursales según las reglas del sistema.",
                Activo = true
            },

            // Reportes
            new Permiso
            {
                Id = 24,
                Codigo = "REPORTES_CONSULTAR",
                Nombre = "Consultar reportes",
                Descripcion = "Permite consultar reportes según el alcance del usuario.",
                Activo = true
            },

            // Configuración
            new Permiso
            {
                Id = 25,
                Codigo = "CONFIGURACION_GESTIONAR",
                Nombre = "Gestionar configuración",
                Descripcion = "Permite administrar la configuración general del negocio.",
                Activo = true
            },

            // Seguridad
            new Permiso
            {
                Id = 26,
                Codigo = "ROLES_GESTIONAR",
                Nombre = "Gestionar roles",
                Descripcion = "Permite administrar roles y sus permisos.",
                Activo = true
            },
            new Permiso
            {
                Id = 27,
                Codigo = "PERMISOS_GESTIONAR",
                Nombre = "Gestionar permisos",
                Descripcion = "Permite administrar los permisos del sistema.",
                Activo = true
            },

            // Código rotativo
            new Permiso
            {
                Id = 28,
                Codigo = "CODIGO_ROTATIVO_CONFIGURAR",
                Nombre = "Configurar código rotativo",
                Descripcion = "Permite configurar el umbral del código rotativo.",
                Activo = true
            },
            new Permiso
            {
                Id = 29,
                Codigo = "CODIGO_ROTATIVO_AUTORIZAR",
                Nombre = "Autorizar con código rotativo",
                Descripcion = "Permite autorizar operaciones que requieran código rotativo.",
                Activo = true
            }
        };

        modelBuilder.Entity<Permiso>().HasData(permisos);

        // =========================================================
        // SUPERADMINISTRADOR
        // =========================================================

        var todosLosPermisos = permisos.Select(
            p => new RolPermiso
            {
                RolId = superAdministradorId,
                PermisoId = p.Id
            });

        modelBuilder.Entity<RolPermiso>().HasData(
            todosLosPermisos);

        // =========================================================
        // ADMINISTRADOR
        // =========================================================
        //
        // Puede gestionar usuarios dentro de las reglas del sistema.
        // No recibe permisos para administrar roles ni permisos.
        //
        // La protección específica para impedir que gestione al
        // SuperAdministrador se implementará además en UsuarioService.
        // =========================================================

        var permisosAdministrador = new[]
        {
            1, 2, 3, 4,
            5, 6,
            7, 8, 9,
            10, 11, 12, 13,
            14, 15,
            16, 17,
            20, 21,
            22,
            24
        };

        modelBuilder.Entity<RolPermiso>().HasData(
            permisosAdministrador.Select(
                id => new RolPermiso
                {
                    RolId = administradorId,
                    PermisoId = id
                })
        );

        // =========================================================
        // ENCARGADO DE SUCURSAL
        // =========================================================
        //
        // Acceso:
        //   - Inicio
        //   - Productos
        //   - Categorías
        //   - Sucursales (solo consulta)
        //   - Punto de ventas
        //
        // No tiene:
        //   - Inventario
        //   - Usuarios
        //   - Roles
        //   - Reportes
        //   - Gestión de sucursales
        //
        // El acceso real a la información queda además limitado
        // por las sucursales asignadas al usuario.
        // =========================================================

        var permisosEncargado = new[]
        {
            1,  // PRODUCTOS_CONSULTAR
            3,  // VENTAS_CONSULTAR
            4,  // CLIENTES_CONSULTAR
            6,  // CATEGORIAS_GESTIONAR
            14, // VENTAS_CREAR
            16, // CLIENTES_CREAR
            17, // CLIENTES_EDITAR
            18, // CAJA_OPERAR
            19, // CAJA_CIERRE
            22  // SUCURSALES_CONSULTAR
        };

        modelBuilder.Entity<RolPermiso>().HasData(
            permisosEncargado.Select(
                id => new RolPermiso
                {
                    RolId = encargadoSucursalId,
                    PermisoId = id
                })
        );

        // =========================================================
        // VENDEDOR / CAJERO
        // =========================================================
        //
        // Acceso:
        //   - Inicio
        //   - Productos (solo consulta)
        //   - Sucursales (solo consulta)
        //   - Punto de ventas
        //
        // No tiene gestión de productos ni de sucursales.
        // =========================================================

        var permisosCajero = new[]
        {
            1,  // PRODUCTOS_CONSULTAR
            3,  // VENTAS_CONSULTAR
            4,  // CLIENTES_CONSULTAR
            14, // VENTAS_CREAR
            16, // CLIENTES_CREAR
            17, // CLIENTES_EDITAR
            18, // CAJA_OPERAR
            19, // CAJA_CIERRE
            22  // SUCURSALES_CONSULTAR
        };

        modelBuilder.Entity<RolPermiso>().HasData(
            permisosCajero.Select(
                id => new RolPermiso
                {
                    RolId = vendedorCajeroId,
                    PermisoId = id
                })
        );

        // =========================================================
        // ALMACÉN / INVENTARIO
        // =========================================================
        //
        // Acceso:
        //   - Inicio
        //   - Productos (solo consulta)
        //   - Categorías
        //   - Sucursales (solo consulta)
        //
        // No tiene:
        //   - Inventario
        //   - Usuarios
        //   - Roles
        //   - POS
        //   - Gestión de sucursales
        // =========================================================

        var permisosAlmacen = new[]
        {
            1, // PRODUCTOS_CONSULTAR
            6, // CATEGORIAS_GESTIONAR
            22 // SUCURSALES_CONSULTAR
        };

        modelBuilder.Entity<RolPermiso>().HasData(
            permisosAlmacen.Select(
                id => new RolPermiso
                {
                    RolId = almacenInventarioId,
                    PermisoId = id
                })
        );
    }
}
