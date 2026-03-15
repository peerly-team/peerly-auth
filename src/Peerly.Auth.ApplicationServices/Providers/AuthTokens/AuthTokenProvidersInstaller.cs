using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.ApplicationServices.Generators.AuthTokens;
using Peerly.Auth.ApplicationServices.Generators.AuthTokens.Abstractions;
using Peerly.Auth.ApplicationServices.Providers.AuthTokens.Abstractions;
using Peerly.Auth.ApplicationServices.Providers.AuthTokens.Options;
using Peerly.Auth.Tools.Abstractions;

namespace Peerly.Auth.ApplicationServices.Providers.AuthTokens;

[ExcludeFromCodeCoverage]
internal sealed class AuthTokenProvidersInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IJwtGenerator, JwtGenerator>();
        services
            .AddOptions<SigningOptions>()
            .BindConfiguration(SigningOptions.SectionName);

        services.AddScoped<ISigningKeyProvider, SigningKeyProvider>();
    }
}
