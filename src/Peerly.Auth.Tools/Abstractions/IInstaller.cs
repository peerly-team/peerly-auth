using Microsoft.Extensions.DependencyInjection;

namespace Peerly.Auth.Tools.Abstractions;

public interface IInstaller
{
    void InstallServices(IServiceCollection services);
}
