using Microsoft.AspNetCore.Authorization;

namespace LicoreriaSistema.Web.Seguridad;

public sealed class PermisoRequirement : IAuthorizationRequirement
{
    public PermisoRequirement(string permiso)
    {
        Permiso = permiso;
    }

    public string Permiso { get; }
}
