using LibreriaSistema.Aplicacion.Interfaces;
using LicoreriaSistema.Dominio.Entidades;

namespace LibreriaSistema.Aplicacion.Servicios;

public class ProductoService
{
    private readonly IProductoRepository _productoRepository;

    public ProductoService(
        IProductoRepository productoRepository)
    {
        _productoRepository = productoRepository;
    }

    public async Task<IReadOnlyList<Producto>> ObtenerTodasAsync(
        bool incluirInactivos = true,
        CancellationToken cancellationToken = default)
    {
        return await _productoRepository.ObtenerTodasAsync(
            incluirInactivos,
            cancellationToken);
    }

    public async Task<Producto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return null;
        }

        return await _productoRepository.ObtenerPorIdAsync(
            id,
            cancellationToken);
    }

    public async Task<(bool Exitoso, string Mensaje)> CrearAsync(
        string nombre,
        string codigo,
        string? descripcion,
        decimal precioCompra,
        decimal precioVenta,
        int categoriaId,
        CancellationToken cancellationToken = default)
    {
        nombre = nombre.Trim();
        codigo = codigo.Trim();
        descripcion = descripcion?.Trim();

        var validacion =
            await ValidarDatosAsync(
                nombre,
                codigo,
                descripcion,
                precioCompra,
                precioVenta,
                categoriaId,
                null,
                cancellationToken);

        if (!validacion.Exitoso)
        {
            return validacion;
        }

        if (await _productoRepository.ExisteCodigoAsync(
                codigo,
                null,
                cancellationToken))
        {
            return (
                false,
                "Ya existe un producto con ese código de barras.");
        }

        var producto = new Producto
        {
            Nombre = nombre,
            Codigo = codigo,
            Descripcion = descripcion ?? string.Empty,
            PrecioCompra = precioCompra,
            PrecioVenta = precioVenta,
            CategoriaId = categoriaId,
            Activo = true
        };

        await _productoRepository.AgregarAsync(
            producto,
            cancellationToken);

        await _productoRepository.GuardarCambiosAsync(
            cancellationToken);

        return (
            true,
            "El producto fue creado correctamente.");
    }

    public async Task<(bool Exitoso, string Mensaje)> ActualizarAsync(
        int id,
        string nombre,
        string codigo,
        string? descripcion,
        decimal precioCompra,
        decimal precioVenta,
        int categoriaId,
        bool activo,
        CancellationToken cancellationToken = default)
    {
        nombre = nombre.Trim();
        codigo = codigo.Trim();
        descripcion = descripcion?.Trim();

        if (id <= 0)
        {
            return (
                false,
                "El producto indicado no es válido.");
        }

        var producto =
            await _productoRepository.ObtenerPorIdAsync(
                id,
                cancellationToken);

        if (producto is null)
        {
            return (
                false,
                "El producto no existe.");
        }

        var validacion =
            await ValidarDatosAsync(
                nombre,
                codigo,
                descripcion,
                precioCompra,
                precioVenta,
                categoriaId,
                id,
                cancellationToken);

        if (!validacion.Exitoso)
        {
            return validacion;
        }

        if (await _productoRepository.ExisteCodigoAsync(
                codigo,
                id,
                cancellationToken))
        {
            return (
                false,
                "Ya existe otro producto con ese código de barras.");
        }

        producto.Nombre = nombre;
        producto.Codigo = codigo;
        producto.Descripcion = descripcion ?? string.Empty;
        producto.PrecioCompra = precioCompra;
        producto.PrecioVenta = precioVenta;
        producto.CategoriaId = categoriaId;
        producto.Activo = activo;

        await _productoRepository.ActualizarAsync(
            producto,
            cancellationToken);

        await _productoRepository.GuardarCambiosAsync(
            cancellationToken);

        return (
            true,
            "El producto fue actualizado correctamente.");
    }

    public async Task<(bool Exitoso, string Mensaje)> CambiarEstadoAsync(
        int id,
        bool activo,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return (
                false,
                "El producto indicado no es válido.");
        }

        var producto =
            await _productoRepository.ObtenerPorIdAsync(
                id,
                cancellationToken);

        if (producto is null)
        {
            return (
                false,
                "El producto no existe.");
        }

        producto.Activo = activo;

        await _productoRepository.ActualizarAsync(
            producto,
            cancellationToken);

        await _productoRepository.GuardarCambiosAsync(
            cancellationToken);

        return (
            true,
            activo
                ? "El producto fue activado correctamente."
                : "El producto fue desactivado correctamente.");
    }

    public async Task<(bool Exitoso, string Mensaje)> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return (
                false,
                "El producto indicado no es válido.");
        }

        var producto =
            await _productoRepository.ObtenerPorIdAsync(
                id,
                cancellationToken);

        if (producto is null)
        {
            return (
                false,
                "El producto no existe.");
        }

        if (await _productoRepository.TieneInventarioAsync(
                id,
                cancellationToken))
        {
            return (
                false,
                "No se puede eliminar el producto porque tiene inventario asociado. Puedes desactivarlo.");
        }

        return (
            false,
            "La eliminación física de productos no está habilitada. Puedes desactivarlo.");
    }

    private async Task<(bool Exitoso, string Mensaje)> ValidarDatosAsync(
        string nombre,
        string codigo,
        string? descripcion,
        decimal precioCompra,
        decimal precioVenta,
        int categoriaId,
        int? excluirId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return (
                false,
                "El nombre del producto es obligatorio.");
        }

        if (nombre.Length > 150)
        {
            return (
                false,
                "El nombre del producto no puede superar los 150 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(codigo))
        {
            return (
                false,
                "El código de barras es obligatorio.");
        }

        if (codigo.Length > 50)
        {
            return (
                false,
                "El código de barras no puede superar los 50 caracteres.");
        }

        if (descripcion is not null &&
            descripcion.Length > 250)
        {
            return (
                false,
                "La descripción no puede superar los 250 caracteres.");
        }

        if (precioCompra < 0)
        {
            return (
                false,
                "El precio de compra no puede ser negativo.");
        }

        if (precioVenta <= 0)
        {
            return (
                false,
                "El precio de venta debe ser mayor que cero.");
        }

        if (categoriaId <= 0)
        {
            return (
                false,
                "Debe seleccionar una categoría.");
        }

        if (!await _productoRepository.ExisteCategoriaAsync(
                categoriaId,
                cancellationToken))
        {
            return (
                false,
                "La categoría seleccionada no existe.");
        }

        return (true, string.Empty);
    }
}
