using System.Globalization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.Logging;

namespace LicoreriaSistema.Web.Seguridad;

public sealed class RevalidatingAuthenticationStateProvider
    : RevalidatingServerAuthenticationStateProvider
{
    public RevalidatingAuthenticationStateProvider(
        ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
    }

    protected override TimeSpan RevalidationInterval =>
        TimeSpan.FromSeconds(15);

    protected override Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        var usuario = authenticationState.User;

        if (usuario.Identity?.IsAuthenticated != true)
        {
 





           return Task.FromResult(false);
        }

        var expiracionClaim =
            usuario.FindFirst("SesionExpiraUtc")?.Value;

        if (string.IsNullOrWhiteSpace(expiracionClaim))
 






       {
            return Task.FromResult(false);
        }

        if (!DateTimeOffset.TryParse(
                expiracionClaim,
 





               CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var fechaExpiracion))
        {




            return Task.FromResult(false);
        }

        return Task.FromResult(
            DateTimeOffset.UtcNow < fechaExpiracion);
 




   }
}
