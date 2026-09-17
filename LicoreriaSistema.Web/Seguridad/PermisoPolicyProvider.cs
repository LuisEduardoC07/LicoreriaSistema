using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace LicoreriaSistema.Web.Seguridad;

public sealed class PermisoPolicyProvider
    : IAuthorizationPolicyProvider
{
    private const string Prefijo = "Permiso:";

    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermisoPolicyProvider(
        IOptions<AuthorizationOptions> options)
    {
        _fallback =
            new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallback.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallback.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(
        string policyName)
    {
        if (!policyName.StartsWith(
                Prefijo,
                StringComparison.OrdinalIgnoreCase))
        {
            return _fallback.GetPolicyAsync(policyName);
        }

        var permiso =
            policyName[Prefijo.Length..].Trim();

        if (string.IsNullOrWhiteSpace(permiso))
        {
            return Task.FromResult<AuthorizationPolicy?>(null);
        }

        var policy =
            new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(
                    new PermisoRequirement(permiso))
                .Build();

        return Task.FromResult<AuthorizationPolicy?>(
            policy);
    }
}
