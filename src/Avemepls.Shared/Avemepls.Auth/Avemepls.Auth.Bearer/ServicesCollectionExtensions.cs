using Avemepls.Auth.Bearer.Abstractions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Avemepls.Auth.Bearer;

public static class ServicesCollectionExtensions
{
    public static IServiceCollection AddJwtBearerTokenAuth(
        this IServiceCollection servicesCollection,
        OAuthOptions1 authOptions)
    {
        servicesCollection.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authOptions.Issuer,

                    ValidateAudience = false,

                    ValidateLifetime = true,
                    ClockSkew = new TimeSpan(0),
                    IssuerSigningKey = authOptions.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true,
                };
            });

        servicesCollection.AddSingleton(authOptions);

        servicesCollection.AddSingleton<ITokenGenerator, TokenGenerator>();

        return servicesCollection;
    }
}