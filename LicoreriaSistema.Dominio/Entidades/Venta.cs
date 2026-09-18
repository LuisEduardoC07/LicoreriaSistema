namespace LicoreriaSistema.Dominio.Entidades;

public class Venta
{
    public int Id { get; set; }

    /// <summary>
    /// Número interno de la factura generado por el sistema.
    /// No representa todavía un e-NCF de DGII.
    /// </summary>
    public string NumeroFactura { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public int SucursalId { get; set; }

    public Sucursal Sucursal { get; set; } = null!;

    // ========================================================
    // DATOS HISTORICOS DEL EMISOR
    // ========================================================

    public string NombreComercialEmisor { get; set; } = string.Empty;

    public string RncEmisor { get; set; } = string.Empty;

    public string NombreSucursalEmisor { get; set; } = string.Empty;

    public string DireccionSucursalEmisor { get; set; } = string.Empty;

    public string TelefonoSucursalEmisor { get; set; } = string.Empty;

    // ========================================================
 















   // CLIENTE
    // ========================================================

    public string NombreCliente { get; set; } = string.Empty;

    public string? RncCliente { get; set; }

    // ========================================================
    // TOTALES
    // ========================================================

    public decimal Subtotal { get; set; }

    public decimal Itbis { get; set; }















    public decimal Isc { get; set; }

    public decimal Total { get; set; }

    // ========================================================
 





   // COBRO
    // ========================================================



    public string MetodoPago { get; set; } = string.Empty;

    public decimal? MontoRecibido { get; set; }





    public decimal? Cambio { get; set; }

    // ========================================================
    // ESTADO
 




   // ========================================================

    public string Estado { get; set; } = "Completada";

 



   public ICollection<VentaDetalle> Detalles { get; set; }
        = new List<VentaDetalle>();
}
