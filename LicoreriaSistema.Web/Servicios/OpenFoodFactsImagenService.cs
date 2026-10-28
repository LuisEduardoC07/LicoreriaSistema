using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LicoreriaSistema.Web.Servicios;

public sealed record CandidatoImagenProducto(
    string Nombre,
    string? Codigo,
    string? Marca,
    string Url);

public sealed class OpenFoodFactsImagenService
{
    private readonly HttpClient _httpClient;

    public OpenFoodFactsImagenService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<CandidatoImagenProducto>> BuscarPorCodigoAsync(
        string? codigo,
        CancellationToken cancellationToken = default)
    {
        var codigoLimpio = new string(
            (codigo ?? string.Empty).Where(char.IsDigit).ToArray());

        if (codigoLimpio.Length < 8 || codigoLimpio.Length > 14)
            return Array.Empty<CandidatoImagenProducto>();

        var url =
            $"api/v2/product/{Uri.EscapeDataString(codigoLimpio)}.json" +
            "?fields=code,product_name,brands,image_front_url,image_url";

        var respuesta = await _httpClient.GetFromJsonAsync<RespuestaProducto>(
            url,
            cancellationToken);

        if (respuesta?.Status != 1 || respuesta.Producto is null)
            return Array.Empty<CandidatoImagenProducto>();

        var producto = respuesta.Producto;

        // No aceptar una coincidencia cuyo código sea diferente.
        if (!string.Equals(
                producto.Codigo,
                codigoLimpio,
                StringComparison.Ordinal))
        {
            return Array.Empty<CandidatoImagenProducto>();
        }

        return CrearCandidatos(new[] { producto });
    }

    public async Task<IReadOnlyList<CandidatoImagenProducto>> BuscarPorNombreAsync(
        string? nombre,
        CancellationToken cancellationToken = default)
    {
        var termino = (nombre ?? string.Empty).Trim();

        if (termino.Length < 2)
            return Array.Empty<CandidatoImagenProducto>();

        if (termino.Length > 120)
            termino = termino[..120];

        var parametros = new Dictionary<string, string>
        {
            ["search_terms"] = termino,
            ["search_simple"] = "1",
            ["action"] = "process",
            ["json"] = "1",
            ["page_size"] = "12",
            ["fields"] = "code,product_name,brands,image_front_url,image_url"
        };

        var query = string.Join(
            "&",
            parametros.Select(p =>
                $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

        var respuesta = await _httpClient.GetFromJsonAsync<RespuestaBusqueda>(
            $"cgi/search.pl?{query}",
            cancellationToken);

        return CrearCandidatos(respuesta?.Productos ?? new List<ProductoApi>());
    }

    private static IReadOnlyList<CandidatoImagenProducto> CrearCandidatos(
        IEnumerable<ProductoApi> productos)
    {
        var candidatos = new List<CandidatoImagenProducto>();
        var urlsVistas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var producto in productos)
        {
            var url = NormalizarUrl(producto.ImagenFrontal)
                ?? NormalizarUrl(producto.Imagen);

            if (url is null || !urlsVistas.Add(url))
                continue;

            candidatos.Add(new CandidatoImagenProducto(
                string.IsNullOrWhiteSpace(producto.Nombre)
                    ? "Producto sin nombre"
                    : producto.Nombre.Trim(),
                producto.Codigo,
                producto.Marcas,
                url));
        }

        return candidatos;
    }

    private static string? NormalizarUrl(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        if (!Uri.TryCreate(valor.Trim(), UriKind.Absolute, out var uri))
            return null;

        if (uri.Scheme != Uri.UriSchemeHttp &&
            uri.Scheme != Uri.UriSchemeHttps)
        {
            return null;
        }

        return uri.AbsoluteUri;
    }

    private sealed class RespuestaProducto
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("product")]
        public ProductoApi? Producto { get; set; }
    }

    private sealed class RespuestaBusqueda
    {
        [JsonPropertyName("products")]
        public List<ProductoApi>? Productos { get; set; }
    }

    private sealed class ProductoApi
    {
        [JsonPropertyName("code")]
        public string? Codigo { get; set; }

        [JsonPropertyName("product_name")]
        public string? Nombre { get; set; }

        [JsonPropertyName("brands")]
        public string? Marcas { get; set; }

        [JsonPropertyName("image_front_url")]
        public string? ImagenFrontal { get; set; }

        [JsonPropertyName("image_url")]
        public string? Imagen { get; set; }
    }
}
