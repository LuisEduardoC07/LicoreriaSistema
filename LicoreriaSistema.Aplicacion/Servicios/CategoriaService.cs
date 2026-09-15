using LibreriaSistema.Aplicacion.Interfaces;
using LicoreriaSistema.Dominio.Entidades;

namespace LibreriaSistema.Aplicacion.Servicios;

public class CategoriaService
{
    private readonly ICategoriaRepository _categoriaRepository;

    public CategoriaService(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<IReadOnlyList<Categoria>> ObtenerTodasAsync(
        bool incluirInactivas = true,
        CancellationToken cancellationToken = default)
    {
        return await _categoriaRepository.ObtenerTodasAsync(
            incluirInactivas,
            cancellationToken);
    }

    public async Task<Categoria?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return null;
        }

        return await _categoriaRepository.ObtenerPorIdAsync(
            id,
            cancellationToken);
    }

    public async Task<(bool Exitoso, string Mensaje)> CrearAsync(
        string nombre,
        string? descripcion,
        CancellationToken cancellationToken = default)
    {
        nombre = nombre.Trim();
        descripcion = descripcion?.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return (
                false,
                "El nombre de la categoría es obligatorio.");
        }

        if (nombre.Length > 100)
        {
            return (
                false,
                "El nombre de la categoría no puede superar los 100 caracteres.");
        }

        if (descripcion is not null &&
            descripcion.Length > 250)
        {
            return (
                false,
                "La descripción no puede superar los 250 caracteres.");
        }

        if (await _categoriaRepository.ExisteNombreAsync(
                nombre,
                null,
                cancellationToken))
        {
            return (
                false,
                "Ya existe una categoría con ese nombre.");
        }

        var categoria = new Categoria
        {
            Nombre = nombre,
            Descripcion = descripcion ?? string.Empty,
            Activa = true
        };

        await _categoriaRepository.AgregarAsync(
            categoria,
            cancellationToken);

        await _categoriaRepository.GuardarCambiosAsync(
            cancellationToken);

        return (
            true,
            "La categoría fue creada correctamente.");
    }

    public async Task<(bool Exitoso, string Mensaje)> ActualizarAsync(
        int id,
        string nombre,
        string? descripcion,
        bool activa,
        CancellationToken cancellationToken = default)
    {
        nombre = nombre.Trim();
        descripcion = descripcion?.Trim();

        if (id <= 0)
        {
            return (
                false,
                "La categoría indicada no es válida.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return (
                false,
                "El nombre de la categoría es obligatorio.");
        }

        if (nombre.Length > 100)
        {
            return (
                false,
                "El nombre de la categoría no puede superar los 100 caracteres.");
        }

        if (descripcion is not null &&
            descripcion.Length > 250)
        {
            return (
                false,
                "La descripción no puede superar los 250 caracteres.");
        }

        var categoria =
            await _categoriaRepository.ObtenerPorIdAsync(
                id,
                cancellationToken);

        if (categoria is null)
        {
            return (
                false,
                "La categoría no existe.");
        }

        if (await _categoriaRepository.ExisteNombreAsync(
                nombre,
                id,
                cancellationToken))
        {
            return (
                false,
                "Ya existe otra categoría con ese nombre.");
        }

        categoria.Nombre = nombre;
        categoria.Descripcion = descripcion ?? string.Empty;
        categoria.Activa = activa;

        await _categoriaRepository.ActualizarAsync(
            categoria,
            cancellationToken);

        await _categoriaRepository.GuardarCambiosAsync(
            cancellationToken);

        return (
            true,
            "La categoría fue actualizada correctamente.");
    }

    public async Task<(bool Exitoso, string Mensaje)> CambiarEstadoAsync(
        int id,
        bool activa,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return (
                false,
                "La categoría indicada no es válida.");
        }

        var categoria =
            await _categoriaRepository.ObtenerPorIdAsync(
                id,
                cancellationToken);

        if (categoria is null)
        {
            return (
                false,
                "La categoría no existe.");
        }

        categoria.Activa = activa;

        await _categoriaRepository.ActualizarAsync(
            categoria,
            cancellationToken);

        await _categoriaRepository.GuardarCambiosAsync(
            cancellationToken);

        return (
            true,
            activa
                ? "La categoría fue activada correctamente."
                : "La categoría fue desactivada correctamente.");
    }

    public async Task<(bool Exitoso, string Mensaje)> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return (
                false,
                "La categoría indicada no es válida.");
        }

        var categoria =
            await _categoriaRepository.ObtenerPorIdAsync(
                id,
                cancellationToken);

        if (categoria is null)
        {
            return (
                false,
                "La categoría no existe.");
        }

        if (await _categoriaRepository.TieneProductosAsync(
                id,
                cancellationToken))
        {
            return (
                false,
                "No se puede eliminar la categoría porque tiene productos asociados.");
        }

        return (
            false,
            "La eliminación física de categorías no está habilitada. Puedes desactivarla.");
    }
}
