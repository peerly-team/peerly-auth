using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.Tools;

namespace Peerly.Auth.Messaging.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.InstallServicesFromExecutingAssembly(configuration);

        return services;
    }
}
