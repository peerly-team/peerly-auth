using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.ApplicationServices.Services.Abstractions;
using Peerly.Auth.Tools.Abstractions;

namespace Peerly.Auth.ApplicationServices.Services;

internal sealed class ServicesInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IHashService, HashService>();
    }
}
