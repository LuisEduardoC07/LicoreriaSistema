namespace LicoreriaSistema.Dominio.Entidades;

public class ReglaImpuesto
{
    public int Id { get; set; }

    public int ClasificacionFiscalId { get; set; }

    public ClasificacionFiscal ClasificacionFiscal { get; set; } = null!;

    public string TipoImpuesto { get; set; } = string.Empty;

    public decimal TasaAdValorem { get; set; }

    public decimal? MontoEspecifico { get; set; }

    public string? UnidadCalculo { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public bool Activo { get; set; } = true;
}