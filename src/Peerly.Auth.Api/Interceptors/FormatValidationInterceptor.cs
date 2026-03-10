using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Peerly.Auth.Api.Exceptions;

namespace Peerly.Auth.Api.Interceptors;

internal sealed class FormatValidationInterceptor : Interceptor
{
    private readonly IServiceProvider _serviceProvider;

    public FormatValidationInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        ValidationFailure[] validationFailures;
        await using (var scope = _serviceProvider.CreateAsyncScope())
        {
            var validators = scope.ServiceProvider.GetServices<IValidator<TRequest>>().ToArray();

            validationFailures = validators
                .Select(validator => validator.Validate(request))
                .SelectMany(result => result.Errors)
                .Where(failure => failure is not null)
                .ToArray();
        }

        return validationFailures.Length == 0
            ? await continuation(request, context)
            : throw new FormatValidationException(validationFailures);
    }
}
