using Avemepls.Auditor.AuditEntities.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Avemepls.Auditor.AuditEntities;

public static class Extensions
{
    /// <summary>
    /// Регистрирует настройки (профили) аудита сущностей
    /// </summary>
    public static IServiceCollection AddAuditEntityProfilesFrom<TType>(this IServiceCollection services)
    {
        var profiles = typeof(TType).Assembly.GetTypes()
            .Where(x => x is { IsClass: true, IsAbstract: false } && x.IsAssignableTo(typeof(IAuditEntityProfile)));

        foreach (var profile in profiles)
        {
            services.AddSingleton(typeof(IAuditEntityProfile), profile);
        }

        services.TryAddScoped<AuditEntityPropertyManager>();
        services.TryAddScoped<AuditInterceptor>();

        return services;
    }

    /// <summary>
    /// Добавляет перехватчик SaveChanges для трэкинга изменений свойств сущностей
    /// </summary>
    public static DbContextOptionsBuilder UseAuditEntities(
        this DbContextOptionsBuilder dbContextOptionsBuilder,
        IServiceProvider serviceProvider)
    {
        dbContextOptionsBuilder.AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>());

        return dbContextOptionsBuilder;
    }
}