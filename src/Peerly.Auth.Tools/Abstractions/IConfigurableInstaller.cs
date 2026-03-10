using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Peerly.Auth.Tools.Abstractions;

public interface IConfigurableInstaller
{
    void InstallServices(IServiceCollection services, IConfiguration configuration);
}
