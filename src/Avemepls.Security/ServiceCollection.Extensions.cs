using Avemepls.Security.Permissions;
using Avemepls.Security.Permissions.Requirements;
using Avemepls.Security.Principal;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Avemepls.Security;

public static class PolicyServicesExtensions
{
    /// <summary>
    /// Configures asp.net authorization for list of claims and policies
    /// </summary>
    public static IServiceCollection AddPermissions(
        this IServiceCollection services,
        Action<PermissionsOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new PermissionsOptions();
        configure?.Invoke(options);

        var permissionsProvider = new AssemblyScanPermissionsProvider(options.Assemblies);
        services.AddSingleton<IPermissionsProvider>(permissionsProvider);
        services.TryAddScoped<ScopePrincipalStorage>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorizationCore(opt =>
        {
            var permissions = permissionsProvider.GetPermissionsFromAssemblies();

            foreach (var permissionWithRoles in permissions)
            {
                opt.AddPolicy(permissionWithRoles.FullName,
                              policy =>
                              {
                                  if (options.ImplicitRoles)
                                  {
                                      policy
                                          .RequireAuthenticatedUser()
                                          .AddRequirements(new RoleOrPermissionRequirement(
                                                               new RoleRequirement(permissionWithRoles.Roles),
                                                               new PermissionRequirement(
                                                                   permissionWithRoles.FullName)));
                                  }
                                  else
                                  {
                                      policy
                                          .RequireAuthenticatedUser()
                                          .AddRequirements(new PermissionRequirement(permissionWithRoles.FullName));
                                  }
                              });
            }
        });

        return services;
    }
}