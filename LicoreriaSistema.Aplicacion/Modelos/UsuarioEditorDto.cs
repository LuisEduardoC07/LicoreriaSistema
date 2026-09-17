namespace LibreriaSistema.Aplicacion.Modelos;

public sealed class UsuarioEditorDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string NombreUsuario { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public int RolId { get; set; }

    public bool Activo { get; set; } = true;

    public bool AlcanceGlobal { get; set; }

    public string Password { get; set; } = string.Empty;

    public HashSet<int> SucursalIds { get; set; } = new();
}
