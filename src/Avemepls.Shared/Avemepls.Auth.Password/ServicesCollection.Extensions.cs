using Avemepls.Auth.Password.Abstractions;

using FluentValidation;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using ServiceScan.SourceGenerator;

namespace Avemepls.Auth.Password;

public static partial class ServicesCollectionExtensions
{
    [GenerateServiceRegistrations(AssignableTo = typeof(IValidator<>), Lifetime = ServiceLifetime.Transient)]
    public static partial IServiceCollection AddFluentValidators(this IServiceCollection services);

    public static IServiceCollection AddPasswordAuth(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.AccessDeniedPath = "/access-denied";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
            });

        services.AddAuthentication();

        services.AddScoped<IAuthService, AuthService>();
        services.AddHttpContextAccessor();

        services.AddFluentValidators();

        return services;
    }
}