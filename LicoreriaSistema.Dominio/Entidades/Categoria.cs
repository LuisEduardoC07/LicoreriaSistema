namespace LicoreriaSistema.Dominio.Entidades;

public class Categoria
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public bool Activa { get; set; } = true;

    public ICollection<Producto> Productos { get; set; }
        = new List<Producto>();
}