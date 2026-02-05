using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using Peerly.Auth.Api.Exceptions;
using Peerly.Auth.Exceptions;

namespace Peerly.Auth.Api.Interceptors;

internal sealed class ExceptionInterceptor : Interceptor
{
    private readonly ILogger<ExceptionInterceptor> _logger;

    public ExceptionInterceptor(ILogger<ExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        return await HandleErrors(() => continuation(requestStream, context));
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        return await HandleErrors(() => continuation(request, context));
    }

    private async Task<T> HandleErrors<T>(Func<Task<T>> func)
    {
        try
        {
            return await func();
        }
        catch (FormatValidationException ex)
        {
            LogException(ex);

            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (BusinessValidationException ex)
        {
            LogException(ex);

            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (NotFoundException ex)
        {
            LogException(ex);

            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (Exception ex)
        {
            LogException(ex);

            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    private void LogException(Exception ex)
    {
        _logger.LogError(ex, "{ExceptionType} was processed by interceptor: {ExceptionMessage}", ex.GetType().Name, ex.Message);
    }
}
