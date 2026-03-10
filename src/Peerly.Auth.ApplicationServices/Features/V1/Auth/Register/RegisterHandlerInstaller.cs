using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Register.Abstractions;
using Peerly.Auth.Tools.Abstractions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Register;

internal sealed class RegisterHandlerInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IRegisterHandlerMapper, RegisterHandlerMapper>();
    }
}
