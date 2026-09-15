namespace LibreriaSistema.Aplicacion.Modelos;

public class ResultadoAutenticacion
{
    public bool Exitoso { get; init; }

    public string? Mensaje { get; init; }

    public int UsuarioId { get; init; }

    public string NombreCompleto { get; init; } = string.Empty;

    public string NombreUsuario { get; init; } = string.Empty;

    public string Rol { get; init; } = string.Empty;

    public bool AlcanceGlobal { get; init; }

    public IReadOnlyCollection<int> SucursalIds { get; init; }
        = Array.Empty<int>();

    public IReadOnlyCollection<string> Permisos { get; init; }
        = Array.Empty<string>();

    public static ResultadoAutenticacion Fallido(string mensaje)
    {
        return new ResultadoAutenticacion
        {
            Exitoso = false,
            Mensaje = mensaje
        };
    }
}