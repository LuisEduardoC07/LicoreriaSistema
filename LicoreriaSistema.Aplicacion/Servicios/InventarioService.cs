using LibreriaSistema.Aplicacion.Interfaces;
using LicoreriaSistema.Dominio.Entidades;

namespace LibreriaSistema.Aplicacion.Servicios;

public class InventarioService
{
    private readonly IInventarioRepository _inventarioRepository;

    public InventarioService(
        IInventarioRepository inventarioRepository)
    {
        _inventarioRepository = inventarioRepository;
    }

    public async Task<IReadOnlyList<Sucursal>> ObtenerSucursalesActivasAsync(
        CancellationToken cancellationToken = default)
    {
        return await _inventarioRepository
            .ObtenerSucursalesActivasAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Producto>> ObtenerProductosActivosAsync(
        CancellationToken cancellationToken = default)
    {
        return await _inventarioRepository
            .ObtenerProductosActivosAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InventarioSucursal>> ObtenerPorSucursalAsync(
        int sucursalId,
        CancellationToken cancellationToken = default)
    {
        if (sucursalId <= 0)
        {
            return Array.Empty<InventarioSucursal>();
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

        if (stockMinimo != inventario.StockMinimo ||
            stockMaximo != inventario.StockMaximo)
        {
            var configuracionActualizada =
                await _inventarioRepository.ActualizarConfiguracionAsync(
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
            await _inventarioRepository.AgregarExistenciaAsync(
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

        var actualizado =
            await _inventarioRepository.ActualizarConfiguracionAsync(
                inventarioId,
                stockMinimo,
                stockMaximo,
                cancellationToken);

        if (!actualizado)
        {
            return (
                false,
                "El registro de inventario no existe.");
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
        if (productoId <= 0 ||
            sucursalId <= 0)
        {
            return (
                false,
                "El producto o la sucursal no son válidos.");
        }

        if (cantidad <= 0)
        {
            return (
                false,
                "La cantidad a descontar debe ser mayor que cero.");
        }

        var inventario =
            await _inventarioRepository.ObtenerAsync(
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
            await _inventarioRepository.DescontarExistenciaAsync(
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
}
