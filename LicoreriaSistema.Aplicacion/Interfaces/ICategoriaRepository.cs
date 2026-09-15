using LicoreriaSistema.Dominio.Entidades;

namespace LibreriaSistema.Aplicacion.Interfaces;

public interface ICategoriaRepository
{
    Task<IReadOnlyList<Categoria>> ObtenerTodasAsync(
        bool incluirInactivas = true,
        CancellationToken cancellationToken = default);

    Task<Categoria?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteNombreAsync(
        string nombre,
        int? excluirId = null,
        CancellationToken cancellationToken = default);

    Task<bool> TieneProductosAsync(
        int categoriaId,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Categoria categoria,
        CancellationToken cancellationToken = default);

    Task ActualizarAsync(
        Categoria categoria,
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}