using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.Tools.Abstractions;

namespace Peerly.Auth.ApplicationServices.Serializers;

internal sealed class SerializersInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<IJsonSerializationService, JsonSerializationService>();
    }
}
