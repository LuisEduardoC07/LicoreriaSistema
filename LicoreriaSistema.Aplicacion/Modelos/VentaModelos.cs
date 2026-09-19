namespace LibreriaSistema.Aplicacion.Modelos;

public sealed class LineaVentaSolicitud
{
    public int ProductoId { get; set; }

    public decimal Cantidad { get; set; }
}

public sealed class LineaVentaCotizada
{
    public int ProductoId { get; init; }

    public string Codigo { get; init; } = string.Empty;

    public string Nombre { get; init; } = string.Empty;

    public string? ImagenUrl { get; init; }

    public decimal Disponible { get; init; }

    public decimal Cantidad { get; init; }

    public decimal PrecioUnitario { get; init; }

    public decimal Subtotal { get; init; }

    public decimal TasaItbis { get; init; }

    public decimal Itbis { get; init; }

    public decimal TasaIscAdValorem { get; init; }

    public decimal IscAdValorem { get; init; }

    public decimal IscEspecifico { get; init; }

    public decimal Isc { get; init; }

    public decimal Total { get; init; }
}

public sealed class CotizacionVenta
{
    public bool Exitoso { get; init; }

    public string Mensaje { get; init; } = string.Empty;

    public IReadOnlyList<LineaVentaCotizada> Lineas { get; init; } =
        Array.Empty<LineaVentaCotizada>();

    public decimal Subtotal { get; init; }

    public decimal Itbis { get; init; }

    public decimal Isc { get; init; }

    public decimal Total { get; init; }
}

public sealed class ResultadoVenta
{
    public bool Exitoso { get; init; }

    public string Mensaje { get; init; } = string.Empty;

    public int VentaId { get; init; }

    public string? NumeroFactura { get; init; }

    public decimal Subtotal { get; init; }

    public decimal Itbis { get; init; }

    public decimal Isc { get; init; }

    public decimal Total { get; init; }

    public decimal Cambio { get; init; }
}


public sealed class FiltroHistorialVentas
{
    public DateTime? Desde { get; set; }

    public DateTime? Hasta { get; set; }

    public int? SucursalId { get; set; }

    public string? NumeroFactura { get; set; }

    public string? Cliente { get; set; }

    public string? MetodoPago { get; set; }
}
