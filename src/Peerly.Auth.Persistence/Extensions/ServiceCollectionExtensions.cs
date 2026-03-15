using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.Persistence.UnitOfWork;
using Peerly.Auth.Tools;

namespace Peerly.Auth.Persistence.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigurePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.InstallServicesFromExecutingAssembly(configuration);

        return services;
    }

    internal static void AddUnitOfWorkInnerFactory<TUnitOfWork>(this IServiceCollection services)
    {
        services.AddScoped<Func<DbConnection, TUnitOfWork>>(
            sp => connection => ActivatorUtilities.CreateInstance<TUnitOfWork>(sp, connection));
    }

    internal static void AddRepositoryFactory<TRepositoryService, TRepositoryImplementation>(this IServiceCollection services)
        where TRepositoryImplementation : TRepositoryService
    {
        services.AddScoped<Func<IConnectionContext, TRepositoryService>>(
            sp => connectionContext => ActivatorUtilities.CreateInstance<TRepositoryImplementation>(sp, connectionContext));
    }
}
