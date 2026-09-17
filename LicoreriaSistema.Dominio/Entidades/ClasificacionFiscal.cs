namespace LicoreriaSistema.Dominio.Entidades;

public class ClasificacionFiscal
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<Producto> Productos { get; set; }
        = new List<Producto>();

    public ICollection<ReglaImpuesto> ReglasImpuesto { get; set; }
        = new List<ReglaImpuesto>();
}