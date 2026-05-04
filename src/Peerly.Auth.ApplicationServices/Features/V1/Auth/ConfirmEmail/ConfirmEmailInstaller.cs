using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using OneOf.Types;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.Tools.Abstractions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.ConfirmEmail;

[ExcludeFromCodeCoverage]
internal sealed class ConfirmEmailInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services)
    {
        services.AddScoped<ICommandValidator<ConfirmEmailCommand, Success>, ConfirmEmailCommandValidator>();
    }
}
