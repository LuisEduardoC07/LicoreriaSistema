using LibreriaSistema.Aplicacion.Modelos;

namespace LibreriaSistema.Aplicacion.Interfaces;

public interface IAutenticacionService
{
    Task<ResultadoAutenticacion> AutenticarAsync(
        string nombreUsuario,
        string password,
        CancellationToken cancellationToken = default);
}