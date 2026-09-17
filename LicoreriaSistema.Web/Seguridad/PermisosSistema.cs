namespace LicoreriaSistema.Web.Seguridad;

public static class PermisosSistema
{
    // Catálogo
    public const string ProductosConsultar =
        "PRODUCTOS_CONSULTAR";

    public const string ProductosGestionar =
        "PRODUCTOS_GESTIONAR";

    public const string CategoriasGestionar =
        "CATEGORIAS_GESTIONAR";

    // Inventario
    public const string InventarioConsultar =
        "INVENTARIO_CONSULTAR";

    public const string InventarioEntrada =
        "INVENTARIO_ENTRADA";

    public const string InventarioSalida =
        "INVENTARIO_SALIDA";

    public const string InventarioAjustar =
        "INVENTARIO_AJUSTAR";

    // Transferencias
    public const string TransferenciasCrear =
        "TRANSFERENCIAS_CREAR";

    public const string TransferenciasAutorizar =
        "TRANSFERENCIAS_AUTORIZAR";

    public const string TransferenciasEnviar =
        "TRANSFERENCIAS_ENVIAR";

    public const string TransferenciasRecibir =
        "TRANSFERENCIAS_RECIBIR";

    // Ventas
    public const string VentasConsultar =
        "VENTAS_CONSULTAR";

    public const string VentasCrear =
        "VENTAS_CREAR";

    public const string VentasAnular =
        "VENTAS_ANULAR";

    // Clientes
    public const string ClientesConsultar =
        "CLIENTES_CONSULTAR";

    public const string ClientesCrear =
        "CLIENTES_CREAR";

    public const string ClientesEditar =
        "CLIENTES_EDITAR";

    // Caja
    public const string CajaOperar =
        "CAJA_OPERAR";

    public const string CajaCierre =
        "CAJA_CIERRE";

    // Usuarios
    public const string UsuariosConsultar =
        "USUARIOS_CONSULTAR";

    public const string UsuariosGestionar =
        "USUARIOS_GESTIONAR";

    // Sucursales
    public const string SucursalesConsultar =
        "SUCURSALES_CONSULTAR";

    public const string SucursalesGestionar =
        "SUCURSALES_GESTIONAR";

    // Reportes
    public const string ReportesConsultar =
        "REPORTES_CONSULTAR";

    // Configuración
    public const string ConfiguracionGestionar =
        "CONFIGURACION_GESTIONAR";

    // Seguridad
    public const string RolesGestionar =
        "ROLES_GESTIONAR";

    public const string PermisosGestionar =
        "PERMISOS_GESTIONAR";

    // Código rotativo
    public const string CodigoRotativoConfigurar =
        "CODIGO_ROTATIVO_CONFIGURAR";

    public const string CodigoRotativoAutorizar =
        "CODIGO_ROTATIVO_AUTORIZAR";
}
