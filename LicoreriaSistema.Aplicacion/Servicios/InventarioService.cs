using LibreriaSistema.Aplicacion.Interfaces;
using LibreriaSistema.Aplicacion.Seguridad;
using LicoreriaSistema.Dominio.Entidades;

namespace LibreriaSistema.Aplicacion.Servicios;

public class InventarioService
{
    private readonly IInventarioRepository _inventarioRepository;
    private readonly IContextoUsuarioActual _contextoUsuarioActual;

    public InventarioService(
        IInventarioRepository inventarioRepository,
        IContextoUsuarioActual contextoUsuarioActual)
    {
        _inventarioRepository = inventarioRepository;
        _contextoUsuarioActual = contextoUsuarioActual;
    }

    public async Task<IReadOnlyList<Sucursal>> ObtenerSucursalesActivasAsync(
        CancellationToken cancellationToken = default)
    {
        ExigirPermiso(
            PermisosSistema.InventarioConsultar);

        var sucursales =
            await _inventarioRepository
                .ObtenerSucursalesActivasAsync(
                    cancellationToken);

        if (_contextoUsuarioActual.AlcanceGlobal)
        {
            return sucursales;
        }

        return sucursales
            .Where(s =>
                _contextoUsuarioActual
                    .PuedeAccederASucursal(s.Id))
            .ToList();
    }

    public async Task<IReadOnlyList<Producto>> ObtenerProductosActivosAsync(
        CancellationToken cancellationToken = default)
    {
        ExigirPermiso(
            PermisosSistema.InventarioConsultar);

        return await _inventarioRepository
            .ObtenerProductosActivosAsync(
                cancellationToken);
    }

    public async Task<IReadOnlyList<InventarioSucursal>> ObtenerPorSucursalAsync(
        int sucursalId,
        CancellationToken cancellationToken = default)
    {
        ExigirPermiso(
            PermisosSistema.InventarioConsultar);

        if (sucursalId <= 0)
        {
            return Array.Empty<InventarioSucursal>();
        }

        if (!_contextoUsuarioActual
                .PuedeAccederASucursal(sucursalId))
        {
            throw new UnauthorizedAccessException(
                "No tienes acceso al inventario de esta sucursal.");
        }

        return await _inventarioRepository
            .ObtenerPorSucursalAsync(
                sucursalId,
                cancellationToken);
    }

    public async Task<(bool Exitoso, string Mensaje)> RegistrarEntradaAsync(
        int productoId,
        int sucursalId,
        decimal cantidad,
        decimal stockMinimo,
        decimal stockMaximo,
        CancellationToken cancellationToken = default)
    {
        if (!TienePermiso(
                PermisosSistema.InventarioEntrada))
        {
            return (
                false,
                "No tienes permisos para registrar entradas de inventario.");
        }

        if (productoId <= 0)
        {
            return (
                false,
                "Debe seleccionar un producto.");
        }

        if (sucursalId <= 0)
        {
            return (
                false,
                "Debe seleccionar una sucursal.");
        }

        if (!_contextoUsuarioActual
                .PuedeAccederASucursal(sucursalId))
        {
            return (
                false,
                "No tienes acceso para operar el inventario de esta sucursal.");
        }

        if (cantidad <= 0)
        {
            return (
                false,
                "La cantidad de entrada debe ser mayor que cero.");
        }

        if (stockMinimo < 0)
        {
            return (
                false,
                "El stock mínimo no puede ser negativo.");
        }

        if (stockMaximo < 0)
        {
            return (
                false,
                "El stock máximo no puede ser negativo.");
        }

        if (stockMaximo > 0 &&
            stockMaximo < stockMinimo)
        {
            return (
                false,
                "El stock máximo no puede ser menor que el stock mínimo.");
        }

        var inventario =
            await _inventarioRepository.ObtenerAsync(
                productoId,
                sucursalId,
                cancellationToken);

        if (inventario is null)
        {
            var nuevoInventario = new InventarioSucursal
            {
                ProductoId = productoId,
                SucursalId = sucursalId,
                Cantidad = cantidad,
                StockMinimo = stockMinimo,
                StockMaximo = stockMaximo
            };

            await _inventarioRepository.AgregarAsync(
                nuevoInventario,
                cancellationToken);

            await _inventarioRepository.GuardarCambiosAsync(
                cancellationToken);

            return (
                true,
                "La entrada fue registrada correctamente.");
        }

        var cambioConfiguracion =
            stockMinimo != inventario.StockMinimo ||
            stockMaximo != inventario.StockMaximo;

        if (cambioConfiguracion &&
            !TienePermiso(
                PermisosSistema.InventarioAjustar))
        {
            return (
                false,
                "No tienes permisos para modificar el stock mínimo o máximo.");
        }

        if (cambioConfiguracion)
        {
            var configuracionActualizada =
                await _inventarioRepository
                    .ActualizarConfiguracionAsync(
                        inventario.Id,
                        stockMinimo,
                        stockMaximo,
                        cancellationToken);

            if (!configuracionActualizada)
            {
                return (
                    false,
                    "No fue posible actualizar la configuración del inventario.");
            }
        }

        var existenciaAgregada =
            await _inventarioRepository
                .AgregarExistenciaAsync(
                    productoId,
                    sucursalId,
                    cantidad,
                    stockMinimo,
                    stockMaximo,
                    cancellationToken);

        if (!existenciaAgregada)
        {
            return (
                false,
                "No fue posible registrar la entrada de inventario.");
        }

        return (
            true,
            "La entrada fue registrada correctamente.");
    }

    public async Task<(bool Exitoso, string Mensaje)> ActualizarConfiguracionAsync(
        int inventarioId,
        decimal stockMinimo,
        decimal stockMaximo,
        CancellationToken cancellationToken = default)
    {
        if (!TienePermiso(
                PermisosSistema.InventarioAjustar))
        {
            return (
                false,
                "No tienes permisos para ajustar la configuración del inventario.");
        }

        if (inventarioId <= 0)
        {
            return (
                false,
                "El inventario indicado no es válido.");
        }

        if (stockMinimo < 0)
        {
            return (
                false,
                "El stock mínimo no puede ser negativo.");
        }

        if (stockMaximo < 0)
        {
            return (
                false,
                "El stock máximo no puede ser negativo.");
        }

        if (stockMaximo > 0 &&
            stockMaximo < stockMinimo)
        {
            return (
                false,
                "El stock máximo no puede ser menor que el stock mínimo.");
        }

        var inventario =
            await _inventarioRepository
                .ObtenerPorIdAsync(
                    inventarioId,
                    cancellationToken);

        if (inventario is null)
        {
            return (
                false,
                "El registro de inventario no existe.");
        }

        if (!_contextoUsuarioActual
                .PuedeAccederASucursal(
                    inventario.SucursalId))
        {
            return (
                false,
                "No tienes acceso para modificar el inventario de esta sucursal.");
        }

        var actualizado =
            await _inventarioRepository
                .ActualizarConfiguracionAsync(
                    inventarioId,
                    stockMinimo,
                    stockMaximo,
                    cancellationToken);

        if (!actualizado)
        {
            return (
                false,
                "No fue posible actualizar la configuración del inventario.");
        }

        return (
            true,
            "La configuración de inventario fue actualizada correctamente.");
    }

    public async Task<(bool Exitoso, string Mensaje)> DescontarExistenciaAsync(
        int productoId,
        int sucursalId,
        decimal cantidad,
        CancellationToken cancellationToken = default)
    {
        if (!TienePermiso(
                PermisosSistema.InventarioSalida))
        {
            return (
                false,
                "No tienes permisos para registrar salidas de inventario.");
        }

        if (productoId <= 0 ||
            sucursalId <= 0)
        {
            return (
                false,
                "El producto o la sucursal no son válidos.");
        }

        if (!_contextoUsuarioActual
                .PuedeAccederASucursal(sucursalId))
        {
            return (
                false,
                "No tienes acceso para operar el inventario de esta sucursal.");
        }

        if (cantidad <= 0)
        {
            return (
                false,
                "La cantidad a descontar debe ser mayor que cero.");
        }

        var inventario =
            await _inventarioRepository
                .ObtenerAsync(
                    productoId,
                    sucursalId,
                    cancellationToken);

        if (inventario is null)
        {
            return (
                false,
                "El producto no tiene inventario registrado en esta sucursal.");
        }

        if (inventario.Cantidad < cantidad)
        {
            return (
                false,
                $"Inventario insuficiente. Existencia disponible: {inventario.Cantidad:N2}.");
        }

        var descontado =
            await _inventarioRepository
                .DescontarExistenciaAsync(
                    productoId,
                    sucursalId,
                    cantidad,
                    cancellationToken);

        if (!descontado)
        {
            return (
                false,
                "No fue posible descontar la existencia.");
        }

        return (
            true,
            "La existencia fue descontada correctamente.");
    }

    private bool TienePermiso(string permiso)
    {
        return _contextoUsuarioActual
            .TienePermiso(permiso);
    }

    private void ExigirPermiso(string permiso)
    {
        if (!TienePermiso(permiso))
        {
            throw new UnauthorizedAccessException(
                "El usuario actual no tiene permisos para realizar esta operación.");
        }
    }
}
