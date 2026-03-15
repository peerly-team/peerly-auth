using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.ApplicationServices.Services.Tokens.Abstractions;
using Peerly.Auth.Tools.Abstractions;

namespace Peerly.Auth.ApplicationServices.Services.Tokens;

[ExcludeFromCodeCoverage]
internal sealed class TokenServicesInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
    }
}
