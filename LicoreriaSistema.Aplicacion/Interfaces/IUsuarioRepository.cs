using LicoreriaSistema.Dominio.Entidades;

namespace LibreriaSistema.Aplicacion.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerParaAutenticacionAsync(
        string nombreUsuario,
        CancellationToken cancellationToken = default);
}