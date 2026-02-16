using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Logout.Abstractions;
using Peerly.Auth.Tools.Abstractions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Logout;

internal sealed class LogoutHandlerInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ILogoutHandlerMapper, LogoutHandlerMapper>();
    }
}
