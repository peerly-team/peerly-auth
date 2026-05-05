using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.Tools.Abstractions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Refresh;

[ExcludeFromCodeCoverage]
internal sealed class RefreshHandlerInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICommandValidator<RefreshCommand, RefreshCommandResponse>, RefreshCommandValidator>();
    }
}
