namespace LibreriaSistema.Aplicacion.Modelos;

public class CategoriaModelo
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public bool Activa { get; set; }

    public int CantidadProductos { get; set; }
}