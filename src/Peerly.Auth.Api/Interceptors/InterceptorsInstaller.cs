using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.Tools.Abstractions;

namespace Peerly.Auth.Api.Interceptors;

[ExcludeFromCodeCoverage]
internal sealed class InterceptorsInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddGrpc(
            options =>
            {
                options.EnableDetailedErrors = true;
                options.Interceptors.Add<ExceptionInterceptor>();
                options.Interceptors.Add<FormatValidationInterceptor>();
            });
    }
}
