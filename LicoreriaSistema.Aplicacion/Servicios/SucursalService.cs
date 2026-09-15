using LibreriaSistema.Aplicacion.Interfaces;
using LicoreriaSistema.Dominio.Entidades;

namespace LibreriaSistema.Aplicacion.Servicios;

public class SucursalService
{
    private readonly ISucursalRepository _sucursalRepository;

    public SucursalService(
        ISucursalRepository sucursalRepository)
    {
        _sucursalRepository = sucursalRepository;
    }

    public async Task<IReadOnlyList<Sucursal>> ObtenerTodasAsync(
        bool incluirInactivas = true,
        CancellationToken cancellationToken = default)
    {
        return await _sucursalRepository.ObtenerTodasAsync(
            incluirInactivas,
            cancellationToken);
    }

    public async Task<Sucursal?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return null;
        }

        return await _sucursalRepository.ObtenerPorIdAsync(
            id,
            cancellationToken);
    }

    public async Task<(bool Exitoso, string Mensaje)> CrearAsync(
        string nombre,
        string direccion,
        string telefono,
        CancellationToken cancellationToken = default)
    {
        nombre = nombre.Trim();
        direccion = direccion.Trim();
        telefono = telefono.Trim();

        var validacion =
            ValidarDatos(
                nombre,
                direccion,
                telefono);

        if (!validacion.Exitoso)
        {
            return validacion;
        }

        if (await _sucursalRepository.ExisteNombreAsync(
                nombre,
                null,
                cancellationToken))
        {
            return (
                false,
                "Ya existe una sucursal con ese nombre.");
        }

        var sucursal = new Sucursal
        {
            Nombre = nombre,
            Direccion = direccion,
            Telefono = telefono,
            Activa = true
        };

        await _sucursalRepository.AgregarAsync(
            sucursal,
            cancellationToken);

        await _sucursalRepository.GuardarCambiosAsync(
            cancellationToken);

        return (
            true,
            "La sucursal fue creada correctamente.");
    }

    public async Task<(bool Exitoso, string Mensaje)> ActualizarAsync(
        int id,
        string nombre,
        string direccion,
        string telefono,
        bool activa,
        CancellationToken cancellationToken = default)
    {
        nombre = nombre.Trim();
        direccion = direccion.Trim();
        telefono = telefono.Trim();

        if (id <= 0)
        {
            return (
                false,
                "La sucursal indicada no es válida.");
        }

        var validacion =
            ValidarDatos(
                nombre,
                direccion,
                telefono);

        if (!validacion.Exitoso)
        {
            return validacion;
        }

        var sucursal =
            await _sucursalRepository.ObtenerPorIdAsync(
                id,
                cancellationToken);

        if (sucursal is null)
        {
            return (
                false,
                "La sucursal no existe.");
        }

        if (await _sucursalRepository.ExisteNombreAsync(
                nombre,
                id,
                cancellationToken))
        {
            return (
                false,
                "Ya existe otra sucursal con ese nombre.");
        }

        sucursal.Nombre = nombre;
        sucursal.Direccion = direccion;
        sucursal.Telefono = telefono;
        sucursal.Activa = activa;

        await _sucursalRepository.ActualizarAsync(
            sucursal,
            cancellationToken);

        await _sucursalRepository.GuardarCambiosAsync(
            cancellationToken);

        return (
            true,
            "La sucursal fue actualizada correctamente.");
    }

    public async Task<(bool Exitoso, string Mensaje)> CambiarEstadoAsync(
        int id,
        bool activa,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return (
                false,
                "La sucursal indicada no es válida.");
        }

        var sucursal =
            await _sucursalRepository.ObtenerPorIdAsync(
                id,
                cancellationToken);

        if (sucursal is null)
        {
            return (
                false,
                "La sucursal no existe.");
        }

        sucursal.Activa = activa;

        await _sucursalRepository.ActualizarAsync(
            sucursal,
            cancellationToken);

        await _sucursalRepository.GuardarCambiosAsync(
            cancellationToken);

        return (
            true,
            activa
                ? "La sucursal fue activada correctamente."
                : "La sucursal fue desactivada correctamente.");
    }

    public async Task<(bool Exitoso, string Mensaje)> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return (
                false,
                "La sucursal indicada no es válida.");
        }

        var sucursal =
            await _sucursalRepository.ObtenerPorIdAsync(
                id,
                cancellationToken);

        if (sucursal is null)
        {
            return (
                false,
                "La sucursal no existe.");
        }

        if (await _sucursalRepository.TieneInventarioAsync(
                id,
                cancellationToken))
        {
            return (
                false,
                "No se puede eliminar la sucursal porque tiene inventario asociado. Puedes desactivarla.");
        }

        if (await _sucursalRepository.TieneUsuariosAsync(
                id,
                cancellationToken))
        {
            return (
                false,
                "No se puede eliminar la sucursal porque tiene usuarios asociados. Puedes desactivarla.");
        }

        return (
            false,
            "La eliminación física de sucursales no está habilitada. Puedes desactivarla.");
    }

    private static (bool Exitoso, string Mensaje) ValidarDatos(
        string nombre,
        string direccion,
        string telefono)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return (
                false,
                "El nombre de la sucursal es obligatorio.");
        }

        if (nombre.Length > 150)
        {
            return (
                false,
                "El nombre de la sucursal no puede superar los 150 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(direccion))
        {
            return (
                false,
                "La dirección de la sucursal es obligatoria.");
        }

        if (direccion.Length > 250)
        {
            return (
                false,
                "La dirección no puede superar los 250 caracteres.");
        }

        if (telefono.Length > 30)
        {
            return (
                false,
                "El teléfono no puede superar los 30 caracteres.");
        }

        return (true, string.Empty);
    }
}
