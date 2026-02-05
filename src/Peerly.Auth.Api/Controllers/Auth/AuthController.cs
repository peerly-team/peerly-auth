using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.Auth.GetJwks;
using Peerly.Auth.ApplicationServices.Features.Auth.Login;
using Peerly.Auth.V1;

namespace Peerly.Auth.Api.Controllers.Auth;

[ExcludeFromCodeCoverage]
public sealed class AuthController : AuthService.AuthServiceBase
{
    private readonly ICommandHandler<LoginCommand, LoginCommandResponse> _loginHandler;
    private readonly IQueryHandler<GetJwksQuery, GetJwksQueryResponse> _getJwksHandler;

    public AuthController(
        ICommandHandler<LoginCommand, LoginCommandResponse> loginHandler,
        IQueryHandler<GetJwksQuery, GetJwksQueryResponse> getJwksHandler)
    {
        _loginHandler = loginHandler;
        _getJwksHandler = getJwksHandler;
    }

    public override async Task<V1LoginResponse> V1Login(V1LoginRequest request, ServerCallContext context)
    {
        var command = request.ToLoginCommand();
        var commandResponse = await _loginHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1LoginResponse();
    }

    public override async Task<V1GetJwksResponse> V1GetJwks(V1GetJwksRequest request, ServerCallContext context)
    {
        var query = request.ToGetJwksQuery();
        var queryResponse = await _getJwksHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetJwksResponse();
    }
}
