using LicoreriaSistema.Dominio.Entidades;

namespace LibreriaSistema.Aplicacion.Interfaces;

public interface IProductoRepository
{
    Task<IReadOnlyList<Producto>> ObtenerTodasAsync(
        bool incluirInactivos = true,
        CancellationToken cancellationToken = default);

    Task<Producto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteCodigoAsync(
        string codigo,
        int? excluirId = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteCategoriaAsync(
        int categoriaId,
        CancellationToken cancellationToken = default);

    Task<bool> TieneInventarioAsync(
        int productoId,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Producto producto,
        CancellationToken cancellationToken = default);

    Task ActualizarAsync(
        Producto producto,
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}
