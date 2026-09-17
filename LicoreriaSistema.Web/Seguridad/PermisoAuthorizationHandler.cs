using Microsoft.AspNetCore.Authorization;

namespace LicoreriaSistema.Web.Seguridad;

public sealed class PermisoAuthorizationHandler
    : AuthorizationHandler<PermisoRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermisoRequirement requirement)
    {
        var tienePermiso =
            context.User.Claims.Any(c =>
                string.Equals(
                    c.Type,
                    "Permiso",
                    StringComparison.OrdinalIgnoreCase)
                &&
                string.Equals(
                    c.Value,
                    requirement.Permiso,
                    StringComparison.OrdinalIgnoreCase));

        if (tienePermiso)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
