namespace LicoreriaSistema.Dominio.Entidades;

public class VentaDetalle
{
    public int Id { get; set; }

    public int VentaId { get; set; }

    public Venta Venta { get; set; } = null!;

    public int ProductoId { get; set; }

    public Producto Producto { get; set; } = null!;

    // ========================================================
    // DATOS HISTÓRICOS DEL PRODUCTO
    // ========================================================

    public string CodigoProducto { get; set; } = string.Empty;

    public string NombreProducto { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }

    // ========================================================
    // CANTIDAD Y PRECIO
    // ========================================================

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    // ========================================================
    // ITBIS
    // ========================================================

    public decimal TasaItbis { get; set; }

    public decimal Itbis { get; set; }

    // ========================================================
    // ISC
    // ========================================================

    public decimal TasaIscAdValorem { get; set; }

    public decimal IscAdValorem { get; set; }

    public decimal IscEspecifico { get; set; }

    public decimal Isc { get; set; }

    // ========================================================
    // TOTAL
    // ========================================================

    public decimal Total { get; set; }
}

