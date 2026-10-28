namespace LibreriaSistema.Aplicacion.DTOs;

public sealed class ProductoCatalogoPublicoDto
{
    public int Id { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public string? Descripcion { get; init; }

    public string? ImagenUrl { get; init; }

    public string CategoriaNombre { get; init; } = string.Empty;

    public decimal PrecioVenta { get; init; }

    public bool Disponible { get; init; }
}
