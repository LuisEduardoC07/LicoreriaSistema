namespace LibreriaSistema.Aplicacion.Modelos;

public sealed class ResultadoOperacion
{
    public bool Exitoso { get; init; }

    public string Mensaje { get; init; } = string.Empty;

    public static ResultadoOperacion Ok(
        string mensaje) =>
        new()
        {
            Exitoso = true,
            Mensaje = mensaje
        };

    public static ResultadoOperacion Fallido(
        string mensaje) =>
        new()
        {
            Exitoso = false,
            Mensaje = mensaje
        };
}
