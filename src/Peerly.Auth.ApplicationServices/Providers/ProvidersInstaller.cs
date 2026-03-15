using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.Abstractions.ApplicationServices;
using Peerly.Auth.Tools.Abstractions;

namespace Peerly.Auth.ApplicationServices.Providers;

[ExcludeFromCodeCoverage]
internal sealed class ProvidersInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IClock, Clock>();
    }
}
