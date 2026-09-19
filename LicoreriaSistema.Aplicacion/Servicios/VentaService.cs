using LibreriaSistema.Aplicacion.Interfaces;
using LibreriaSistema.Aplicacion.Modelos;
using LibreriaSistema.Aplicacion.Seguridad;
using LicoreriaSistema.Dominio.Entidades;

namespace LibreriaSistema.Aplicacion.Servicios;

public sealed class VentaService
{
    private const string MetodoEfectivo = "Efectivo";
    private const string MetodoTarjeta = "Tarjeta";
    private const string MetodoTransferencia = "Transferencia";

    private readonly IVentaRepository _ventaRepository;
    private readonly ISucursalRepository _sucursalRepository;
    private readonly IContextoUsuarioActual _contextoUsuarioActual;

    public VentaService(
        IVentaRepository ventaRepository,
        ISucursalRepository sucursalRepository,
        IContextoUsuarioActual contextoUsuarioActual)
    {
        _ventaRepository = ventaRepository;
        _sucursalRepository = sucursalRepository;
        _contextoUsuarioActual = contextoUsuarioActual;
    }

    public async Task<IReadOnlyList<Sucursal>>
        ObtenerSucursalesParaVentaAsync(
            CancellationToken cancellationToken = default)
    {
        ExigirAccesoPOS();

        var sucursales =
            await _sucursalRepository.ObtenerTodasAsync(
                incluirInactivas: false,
                cancellationToken);

        if (_contextoUsuarioActual.Rol
                ?.Equals(
                    "SuperAdministrador",
                    StringComparison.OrdinalIgnoreCase) == true)
        {
            return sucursales;
        }

        return sucursales
            .Where(s =>
                _contextoUsuarioActual
                    .PuedeAccederASucursal(s.Id))
            .ToList();
    }

    public async Task<IReadOnlyList<InventarioSucursal>>
        BuscarProductosDisponiblesAsync(
            int sucursalId,
            string? textoBusqueda = null,
            int maxResultados = 30,
            CancellationToken cancellationToken = default)
    {
        ExigirAccesoPOS();
        ExigirAccesoSucursal(sucursalId);

        return await _ventaRepository
            .BuscarProductosDisponiblesAsync(
                sucursalId,
                textoBusqueda,
                maxResultados,
                cancellationToken);
    }

    public async Task<InventarioSucursal?>
        ObtenerProductoDisponibleAsync(
            int productoId,
            int sucursalId,
            CancellationToken cancellationToken = default)
    {
        ExigirAccesoPOS();
        ExigirAccesoSucursal(sucursalId);

        return await _ventaRepository
            .ObtenerProductoDisponibleAsync(
                productoId,
                sucursalId,
                cancellationToken);
    }

    public async Task<CotizacionVenta> CalcularCotizacionAsync(
        int sucursalId,
        IEnumerable<LineaVentaSolicitud> carrito,
        CancellationToken cancellationToken = default)
{
    if (!_contextoUsuarioActual.EstaAutenticado)
    {
        return new CotizacionVenta
        {
            Exitoso = false,
            Mensaje = "Debes iniciar sesión para realizar ventas."
        };
    }

    if (!_contextoUsuarioActual.TienePermiso("VENTAS_CREAR"))
    {
        return new CotizacionVenta
        {
            Exitoso = false,
            Mensaje = "No tienes permiso para registrar ventas."
        };
    }

    if (sucursalId <= 0)
    {
        return new CotizacionVenta
        {
            Exitoso = false,
            Mensaje = "Selecciona una sucursal válida."
        };
    }

    if (!_contextoUsuarioActual.PuedeAccederASucursal(sucursalId))
    {
        return new CotizacionVenta
        {
            Exitoso = false,
            Mensaje = "No tienes acceso a la sucursal seleccionada."
        };
    }

    var solicitudes = (carrito ?? Array.Empty<LineaVentaSolicitud>())
        .Where(x => x is not null && x.ProductoId > 0 && x.Cantidad > 0)
        .GroupBy(x => x.ProductoId)
        .Select(g => new LineaVentaSolicitud
        {
            ProductoId = g.Key,
            Cantidad = g.Sum(x => x.Cantidad)
        })
        .ToList();

    if (solicitudes.Count == 0)
    {
        return new CotizacionVenta
        {
            Exitoso = false,
            Mensaje = "Agrega al menos un producto al carrito."
        };
    }

    var lineas = new List<LineaVentaCotizada>();

    foreach (var solicitud in solicitudes)
    {
        var inventario =
            await _ventaRepository.ObtenerProductoDisponibleAsync(
                solicitud.ProductoId,
                sucursalId);

        if (inventario is null)
        {
            return new CotizacionVenta
            {
                Exitoso = false,
                Mensaje =
                    $"El producto con ID {solicitud.ProductoId} no está disponible en esta sucursal."
            };
        }

        var producto = inventario.Producto;

        if (producto is null)
        {
            return new CotizacionVenta
            {
                Exitoso = false,
                Mensaje =
                    $"No fue posible obtener los datos del producto ID {solicitud.ProductoId}."
            };
        }

        if (!producto.Activo)
        {
            return new CotizacionVenta
            {
                Exitoso = false,
                Mensaje =
                    $"El producto '{producto.Nombre}' está inactivo."
            };
        }

        if (solicitud.Cantidad > inventario.Cantidad)
        {
            return new CotizacionVenta
            {
                Exitoso = false,
                Mensaje =
                    $"Inventario insuficiente para '{producto.Nombre}'. " +
                    $"Disponible: {inventario.Cantidad:N2}."
            };
        }

        if (producto.PrecioVenta <= 0)
        {
            return new CotizacionVenta
            {
                Exitoso = false,
                Mensaje =
                    $"El producto '{producto.Nombre}' todavía no tiene un precio de venta válido."
            };
        }

        if (producto.ClasificacionFiscal is null)
        {
            return new CotizacionVenta
            {
                Exitoso = false,
                Mensaje =
                    $"El producto '{producto.Nombre}' no tiene clasificación fiscal configurada."
            };
        }

        var ahora = DateTime.Now;

        var reglas = producto.ClasificacionFiscal.ReglasImpuesto
            .Where(r =>
                r.Activo &&
                r.FechaInicio <= ahora &&
                (r.FechaFin == null || r.FechaFin >= ahora))
            .OrderByDescending(r => r.FechaInicio)
            .ToList();

        decimal NormalizarTasa(decimal tasa)
        {
            if (tasa <= 0)
                return 0m;

            return tasa > 1m
                ? tasa / 100m
                : tasa;
        }

        var reglaItbis = reglas
            .Where(r =>
                string.Equals(
                    r.TipoImpuesto?.Trim(),
                    "ITBIS",
                    StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        var reglaIscAdValorem = reglas
            .Where(r =>
                string.Equals(
                    r.TipoImpuesto?.Trim(),
                    "ISC_AD_VALOREM",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    r.TipoImpuesto?.Trim(),
                    "ISC_ADVALOREM",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    r.TipoImpuesto?.Trim(),
                    "ISC",
                    StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        var reglaIscEspecifico = reglas
            .Where(r =>
                string.Equals(
                    r.TipoImpuesto?.Trim(),
                    "ISC_ESPECIFICO",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    r.TipoImpuesto?.Trim(),
                    "ISC_ESPECÍFICO",
                    StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        var tasaItbis =
            NormalizarTasa(
                reglaItbis?.TasaAdValorem ?? 0m);

        var tasaIscAdValorem =
            NormalizarTasa(
                reglaIscAdValorem?.TasaAdValorem ?? 0m);

        var unidadEspecifico =
            reglaIscEspecifico?.UnidadCalculo?
                .Trim()
                .ToUpperInvariant();

        var aplicaEspecifico =
            unidadEspecifico == "UNIDAD" ||
            unidadEspecifico == "UNIDADES";

        var montoEspecificoUnitario =
            aplicaEspecifico
                ? (reglaIscEspecifico?.MontoEspecifico ?? 0m)
                : 0m;

        /*
         * PrecioVenta es el precio FINAL al público.
         * Los impuestos están incluidos en él.
         */
        var precioFinalLinea =
            Math.Round(
                producto.PrecioVenta * solicitud.Cantidad,
                2,
                MidpointRounding.AwayFromZero);

        /*
         * ISC específico para toda la cantidad.
         */
        var iscEspecificoLinea =
            Math.Round(
                montoEspecificoUnitario * solicitud.Cantidad,
                2,
                MidpointRounding.AwayFromZero);

        /*
         * Primero quitamos ITBIS del precio final.
         */
        var precioSinItbis =
            tasaItbis > 0m
                ? precioFinalLinea / (1m + tasaItbis)
                : precioFinalLinea;

        /*
         * Después quitamos ISC específico y
         * despejamos la base antes del ISC ad-valorem.
         */
        var baseImponible =
            tasaIscAdValorem > 0m
                ? (precioSinItbis - iscEspecificoLinea)
                    / (1m + tasaIscAdValorem)
                : precioSinItbis - iscEspecificoLinea;

        if (baseImponible < 0m)
        {
            return new CotizacionVenta
            {
                Exitoso = false,
                Mensaje =
                    $"El precio final de '{producto.Nombre}' no alcanza para cubrir " +
                    "los impuestos configurados. Revisa su precio de venta y sus reglas fiscales."
            };
        }

        var iscAdValoremLinea =
            Math.Round(
                baseImponible * tasaIscAdValorem,
                2,
                MidpointRounding.AwayFromZero);

        /*
         * En vez de calcular ITBIS de forma independiente y
         * arriesgar diferencias de centavos, lo obtenemos por
         * diferencia para que:
         *
         * BASE + ISC + ITBIS = PRECIO FINAL
         *
         * exactamente a 2 decimales.
         */
        var itbisLinea =
            precioFinalLinea -
            baseImponible -
            iscAdValoremLinea -
            iscEspecificoLinea;

        var baseImponibleRedondeada =
            Math.Round(
                baseImponible,
                2,
                MidpointRounding.AwayFromZero);

        var iscAdValoremRedondeado =
            Math.Round(
                iscAdValoremLinea,
                2,
                MidpointRounding.AwayFromZero);

        var iscEspecificoRedondeado =
            Math.Round(
                iscEspecificoLinea,
                2,
                MidpointRounding.AwayFromZero);

        var itbisRedondeado =
            Math.Round(
                itbisLinea,
                2,
                MidpointRounding.AwayFromZero);

        /*
         * Ajuste final de centavos para garantizar:
         *
         * BASE + ITBIS + ISC = TOTAL
         */
        var sumaComponentes =
            baseImponibleRedondeada +
            itbisRedondeado +
            iscAdValoremRedondeado +
            iscEspecificoRedondeado;

        var diferencia =
            precioFinalLinea - sumaComponentes;

        if (diferencia != 0m)
        {
            itbisRedondeado =
                Math.Round(
                    itbisRedondeado + diferencia,
                    2,
                    MidpointRounding.AwayFromZero);
        }

        var iscTotal =
            Math.Round(
                iscAdValoremRedondeado +
                iscEspecificoRedondeado,
                2,
                MidpointRounding.AwayFromZero);

        lineas.Add(
            new LineaVentaCotizada
            {
                ProductoId = producto.Id,
                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                Disponible = inventario.Cantidad,
                Cantidad = solicitud.Cantidad,

                /*
                 * IMPORTANTE:
                 * Para el usuario del POS, este sigue siendo
                 * el precio FINAL que debe cobrar.
                 */
                PrecioUnitario = producto.PrecioVenta,

                /*
                 * Subtotal ahora representa la base antes de impuestos.
                 */
                Subtotal = baseImponibleRedondeada,

                TasaItbis = tasaItbis,
                Itbis = itbisRedondeado,

                TasaIscAdValorem = tasaIscAdValorem,
                IscAdValorem = iscAdValoremRedondeado,
                IscEspecifico = iscEspecificoRedondeado,
                Isc = iscTotal,

                /*
                 * Total siempre es el precio FINAL.
                 */
                Total = precioFinalLinea
            });
    }

    return new CotizacionVenta
    {
        Exitoso = true,
        Mensaje = "Cotización calculada correctamente.",
        Lineas = lineas,

        /*
         * Base antes de impuestos.
         */
        Subtotal =
            Math.Round(
                lineas.Sum(x => x.Subtotal),
                2,
                MidpointRounding.AwayFromZero),

        Itbis =
            Math.Round(
                lineas.Sum(x => x.Itbis),
                2,
                MidpointRounding.AwayFromZero),

        Isc =
            Math.Round(
                lineas.Sum(x => x.Isc),
                2,
                MidpointRounding.AwayFromZero),

        /*
         * TOTAL = lo que paga el cliente.
         */
        Total =
            Math.Round(
                lineas.Sum(x => x.Total),
                2,
                MidpointRounding.AwayFromZero)
    };
}

    public async Task<ResultadoVenta>
        RegistrarVentaAsync(
            int sucursalId,
            string? nombreCliente,
            string? rncCliente,
            IReadOnlyCollection<LineaVentaSolicitud>? lineas,
            string? metodoPago,
            decimal? montoRecibido,
            CancellationToken cancellationToken = default)
    {
        ExigirAccesoPOS();
        ExigirAccesoSucursal(sucursalId);

        nombreCliente =
            nombreCliente?.Trim() ?? string.Empty;

        rncCliente =
            string.IsNullOrWhiteSpace(rncCliente)
                ? null
                : rncCliente.Trim();

        if (string.IsNullOrWhiteSpace(nombreCliente))
        {
            return Error(
                "Debe indicar el nombre del cliente.");
        }

        if (!ValidarRnc(rncCliente))
        {
            return Error(
                "El RNC del cliente contiene un formato no válido.");
        }

        var metodo =
            NormalizarMetodoPago(metodoPago);

        if (string.IsNullOrWhiteSpace(metodo))
        {
            return Error(
                "Debe seleccionar un método de pago.");
        }

        var cotizacion =
            await CalcularCotizacionAsync(
                sucursalId,
                lineas,
                cancellationToken);

        if (!cotizacion.Exitoso)
        {
            return Error(
                cotizacion.Mensaje);
        }

        var sucursal =
            await _sucursalRepository.ObtenerPorIdAsync(
                sucursalId,
                cancellationToken);

        if (sucursal is null)
        {
            return Error(
                "La sucursal indicada no existe.");
        }

        if (!sucursal.Activa)
        {
            return Error(
                "La sucursal seleccionada está inactiva.");
        }

        var venta =
            new Venta
            {
                Fecha =
                    DateTime.Now,

                UsuarioId =
                    _contextoUsuarioActual.UsuarioId,

                SucursalId =
                    sucursal.Id,

                NombreComercialEmisor =
                    "LICORERÍA SISTEMA",

                RncEmisor =
                    "123456789",

                NombreSucursalEmisor =
                    sucursal.Nombre,

                DireccionSucursalEmisor =
                    sucursal.Direccion,

                TelefonoSucursalEmisor =
                    sucursal.Telefono,

                NombreCliente =
                    nombreCliente,

                RncCliente =
                    rncCliente,

                Subtotal =
                    cotizacion.Subtotal,

                Itbis =
                    cotizacion.Itbis,

                Isc =
                    cotizacion.Isc,

                Total =
                    cotizacion.Total,

                MetodoPago =
                    metodo,

                Estado =
                    "Completada"
            };

        foreach (var linea in cotizacion.Lineas)
        {
            venta.Detalles.Add(
                new VentaDetalle
                {
                    ProductoId =
                        linea.ProductoId,

                    CodigoProducto =
                        linea.Codigo,

                    NombreProducto =
                        linea.Nombre,
                ImagenUrl = linea.ImagenUrl,

                    Cantidad =
                        linea.Cantidad,

                    PrecioUnitario =
                        linea.PrecioUnitario,

                    Subtotal =
                        linea.Subtotal,

                    TasaItbis =
                        linea.TasaItbis,

                    Itbis =
                        linea.Itbis,

                    TasaIscAdValorem =
                        linea.TasaIscAdValorem,

                    IscAdValorem =
                        linea.IscAdValorem,

                    IscEspecifico =
                        linea.IscEspecifico,

                    Isc =
                        linea.Isc,

                    Total =
                        linea.Total
                });
        }

        decimal cambio = 0;

        if (metodo == MetodoEfectivo)
        {
            if (!montoRecibido.HasValue)
            {
                return Error(
                    "Debe indicar el monto recibido en efectivo.");
            }

            if (montoRecibido.Value < venta.Total)
            {
                return Error(
                    $"El monto recibido es insuficiente. Total: RD$ {venta.Total:N2}.");
            }

            venta.MontoRecibido =
                Redondear(
                    montoRecibido.Value);

            cambio =
                Redondear(
                    montoRecibido.Value -
                    venta.Total);

            venta.Cambio =
                cambio;
        }

        var resultado =
            await _ventaRepository
                .RegistrarVentaAsync(
                    venta,
                    cancellationToken);

        if (!resultado.Exitoso)
        {
            return Error(
                resultado.Mensaje);
        }

        return new ResultadoVenta
        {
            Exitoso = true,

            Mensaje =
                $"Venta registrada correctamente. Factura {resultado.Mensaje}.",

            VentaId =
                venta.Id,

            NumeroFactura =
                resultado.Mensaje,

            Subtotal =
                venta.Subtotal,

            Itbis =
                venta.Itbis,

            Isc =
                venta.Isc,

            Total =
                venta.Total,

            Cambio =
                cambio
        };
    }

    public async Task<Venta?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        ExigirAccesoPOS();

        var venta =
            await _ventaRepository
                .ObtenerPorIdAsync(
                    id,
                    cancellationToken);

        if (venta is null)
        {
            return null;
        }

        if (!_contextoUsuarioActual.AlcanceGlobal &&
            !_contextoUsuarioActual
                .PuedeAccederASucursal(venta.SucursalId))
        {
            throw new UnauthorizedAccessException(
                "No tienes acceso a esta venta.");
        }

        return venta;
    }

    private void ExigirAccesoPOS()
    {
        if (!_contextoUsuarioActual.EstaAutenticado)
        {
            throw new UnauthorizedAccessException(
                "Debes iniciar sesión para utilizar el Punto de Venta.");
        }

        if (!_contextoUsuarioActual
                .TienePermiso(PermisosSistema.VentasCrear))
        {
            throw new UnauthorizedAccessException(
                "El usuario actual no tiene permiso para crear ventas.");
        }

        if (!_contextoUsuarioActual
                .TienePermiso(PermisosSistema.CajaOperar))
        {
            throw new UnauthorizedAccessException(
                "El usuario actual no tiene permiso para operar caja.");
        }
    }

    private void ExigirAccesoSucursal(
        int sucursalId)
    {
        if (!_contextoUsuarioActual
                .PuedeAccederASucursal(sucursalId))
        {
            throw new UnauthorizedAccessException(
                "No tienes acceso a la sucursal seleccionada.");
        }
    }

    private static ReglaImpuesto? ObtenerRegla(
        IEnumerable<ReglaImpuesto> reglas,
        string tipo)
    {
        return reglas
            .Where(r =>
                string.Equals(
                    r.TipoImpuesto.Trim(),
                    tipo,
                    StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(
                r => r.FechaInicio)
            .FirstOrDefault();
    }

    private static decimal CalcularImpuestoEspecifico(
        ReglaImpuesto? regla,
        decimal cantidad)
    {
        if (regla is null ||
            !regla.MontoEspecifico.HasValue ||
            regla.MontoEspecifico.Value <= 0 ||
            cantidad <= 0)
        {
            return 0;
        }

        var unidad =
            regla.UnidadCalculo?
                .Trim()
                .ToUpperInvariant();

        if (unidad is "UNIDAD" or "UNIDADES")
        {
            return Redondear(
                cantidad *
                regla.MontoEspecifico.Value);
        }

        return 0;
    }

    private static decimal NormalizarTasa(
        decimal tasa)
    {
        return tasa > 1
            ? tasa / 100m
            : tasa;
    }

    private static decimal Redondear(
        decimal valor)
    {
        return Math.Round(
            valor,
            2,
            MidpointRounding.AwayFromZero);
    }

    private static string? NormalizarMetodoPago(
        string? metodoPago)
    {
        var valor =
            metodoPago?.Trim();

        if (string.Equals(
                valor,
                MetodoEfectivo,
                StringComparison.OrdinalIgnoreCase))
        {
            return MetodoEfectivo;
        }

        if (string.Equals(
                valor,
                MetodoTarjeta,
                StringComparison.OrdinalIgnoreCase))
        {
            return MetodoTarjeta;
        }

        if (string.Equals(
                valor,
                MetodoTransferencia,
                StringComparison.OrdinalIgnoreCase))
        {
            return MetodoTransferencia;
        }

        return null;
    }

    private static bool ValidarRnc(
        string? rnc)
    {
        if (string.IsNullOrWhiteSpace(rnc))
        {
            return true;
        }

        var caracteresValidos =
            rnc.All(c =>
                char.IsDigit(c) ||
                c == '-' ||
                c == ' ');

        return caracteresValidos &&
               rnc.Length <= 20;
    }

    private static ResultadoVenta Error(
        string mensaje)
    {
        return new ResultadoVenta
        {
            Exitoso = false,
            Mensaje = mensaje
        };
    }

    private static CotizacionVenta ErrorCotizacion(
        string mensaje)
    {
        return new CotizacionVenta
        {
            Exitoso = false,
            Mensaje = mensaje
        };
    }

    public async Task<Venta?> ObtenerPorNumeroFacturaAsync(
        string numeroFactura,
        CancellationToken cancellationToken = default)
    {
        if (!_contextoUsuarioActual.EstaAutenticado)
        {
            return null;
        }

        if (!_contextoUsuarioActual.TienePermiso("VENTAS_CONSULTAR"))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(numeroFactura))
        {
            return null;
        }

        var venta =
            await _ventaRepository.ObtenerPorNumeroFacturaAsync(
                numeroFactura.Trim(),
                cancellationToken);

        if (venta is null)
        {
            return null;
        }

        if (!_contextoUsuarioActual.PuedeAccederASucursal(
                venta.SucursalId))
        {
            return null;
        }

        return venta;
    }

    public async Task<IReadOnlyList<Venta>> ObtenerHistorialAsync(
        FiltroHistorialVentas filtro,
        CancellationToken cancellationToken = default)
    {
        if (!_contextoUsuarioActual.EstaAutenticado)
        {
            throw new UnauthorizedAccessException(
                "Debes iniciar sesión para consultar el historial de ventas.");
        }

        if (!_contextoUsuarioActual.TienePermiso(
                PermisosSistema.VentasConsultar))
        {
            throw new UnauthorizedAccessException(
                "No tienes permiso para consultar el historial de ventas.");
        }

        filtro ??= new FiltroHistorialVentas();

        // Si el usuario selecciona una sucursal específica,
        // primero verificamos que tenga acceso a ella.
        if (filtro.SucursalId.HasValue &&
            filtro.SucursalId.Value > 0 &&
            !_contextoUsuarioActual.AlcanceGlobal &&
            !_contextoUsuarioActual.PuedeAccederASucursal(
                filtro.SucursalId.Value))
        {
            throw new UnauthorizedAccessException(
                "No tienes acceso a la sucursal seleccionada.");
        }

        IReadOnlyCollection<int>? sucursalesPermitidas = null;

        // Los usuarios con alcance global pueden consultar
        // las ventas de todas las sucursales.
        if (!_contextoUsuarioActual.AlcanceGlobal)
        {
            sucursalesPermitidas =
                _contextoUsuarioActual.SucursalIds;
        }

        return await _ventaRepository.BuscarHistorialAsync(
            filtro,
            sucursalesPermitidas,
            cancellationToken);
    }
}







