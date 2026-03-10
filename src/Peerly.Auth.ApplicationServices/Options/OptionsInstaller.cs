using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.Tools.Abstractions;

namespace Peerly.Auth.ApplicationServices.Options;

internal sealed class OptionsInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services
            .AddOptions<ExpirationTimeOptions>()
            .BindConfiguration(ExpirationTimeOptions.SectionName);
    }
}
