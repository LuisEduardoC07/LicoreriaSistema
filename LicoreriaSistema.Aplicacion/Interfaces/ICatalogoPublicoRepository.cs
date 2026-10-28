using LibreriaSistema.Aplicacion.DTOs;

namespace LibreriaSistema.Aplicacion.Interfaces;

public interface ICatalogoPublicoRepository
{
    Task<IReadOnlyList<SucursalCatalogoPublicoDto>>
        ObtenerSucursalesActivasAsync(
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductoCatalogoPublicoDto>>
        ObtenerProductosAsync(
            int sucursalId,
            CancellationToken cancellationToken = default);
}
