using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.Auth.Login;
using Peerly.Auth.Tools;
using Peerly.Auth.Tools.Abstractions;

namespace Peerly.Auth.ApplicationServices.Features;

[ExcludeFromCodeCoverage]
internal sealed class FeaturesInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.Scan(
            scan => scan
                .FromAssemblyOf<LoginHandler>()
                .AddNonGenericImplementationsOf(typeof(ICommandHandler<,>))
                .WithScopedLifetime());
    }
}
