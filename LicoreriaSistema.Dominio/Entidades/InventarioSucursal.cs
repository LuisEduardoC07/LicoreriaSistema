namespace LicoreriaSistema.Dominio.Entidades;

public class InventarioSucursal
{
    public int Id { get; set; }

    public int ProductoId { get; set; }

    public Producto Producto { get; set; } = null!;

    public int SucursalId { get; set; }

    public Sucursal Sucursal { get; set; } = null!;

    public decimal Cantidad { get; set; }

    public decimal StockMinimo { get; set; }

    public decimal StockMaximo { get; set; }
}