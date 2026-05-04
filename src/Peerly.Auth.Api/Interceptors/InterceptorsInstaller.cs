using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.Api.Validators.Auth;
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

        services.Scan(
            scan => scan
                .FromAssemblyOf<V1ConfirmEmailRequestValidator>()
                .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());
    }
}
