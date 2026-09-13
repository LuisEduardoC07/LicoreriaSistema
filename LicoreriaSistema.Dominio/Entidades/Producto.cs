namespace LicoreriaSistema.Dominio.Entidades;

public class Producto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public decimal PrecioCompra { get; set; }

    public decimal PrecioVenta { get; set; }

    public int CategoriaId { get; set; }

    public Categoria Categoria { get; set; } = null!;

    public bool Activo { get; set; } = true;

    public ICollection<InventarioSucursal> Inventarios { get; set; }
        = new List<InventarioSucursal>();
}