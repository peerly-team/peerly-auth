using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Login.Abstractions;
using Peerly.Auth.Tools.Abstractions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Login;

internal sealed class LoginHandlerInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ILoginHandlerMapper, LoginHandlerMapper>();
    }
}
