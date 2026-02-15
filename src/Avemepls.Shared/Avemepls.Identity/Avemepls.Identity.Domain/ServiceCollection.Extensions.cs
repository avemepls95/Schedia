using Avemepls.Auditor.DataAccess.DbContextAuditor;
using Avemepls.Identity.Abstraction;

using FluentValidation;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ServiceScan.SourceGenerator;

namespace Avemepls.Identity.Domain;

public static partial class ServiceCollectionExtensions
{
    [GenerateServiceRegistrations(AssignableTo = typeof(IValidator<>), Lifetime = ServiceLifetime.Transient)]
    public static partial IServiceCollection AddFluentValidators(this IServiceCollection services);

    public static IServiceCollection AddIdentityDomain(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        serviceCollection.AddFluentValidators();

        var options = configuration.GetSection("Identity").Get<IdentityOptions>() ?? new IdentityOptions();
        serviceCollection.AddSingleton(options);

        serviceCollection.ConfigureAuditor(cfg => cfg.AddAuditableTypesFromAssembly<DataAccess.Models.User>());

        return serviceCollection;
    }
}