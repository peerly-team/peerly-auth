using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using OneOf.Types;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.GetJwks;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Login;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Logout;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.RefreshAccessToken;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Register;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.ConfirmEmail;
using Peerly.Auth.V1;

namespace Peerly.Auth.Api.Controllers.Auth;

[ExcludeFromCodeCoverage]
public sealed class AuthController : AuthService.AuthServiceBase
{
    private readonly ICommandHandler<LoginCommand, LoginCommandResponse> _loginHandler;
    private readonly IQueryHandler<GetJwksQuery, GetJwksQueryResponse> _getJwksHandler;
    private readonly ICommandHandler<RegisterCommand, RegisterCommandResponse> _registerHandler;
    private readonly ICommandHandler<LogoutCommand, Success> _logoutHandler;
    private readonly ICommandHandler<RefreshCommand, RefreshCommandResponse> _refreshHandler;
    private readonly ICommandHandler<ConfirmEmailCommand, Success> _verifyEmailHandler;

    public AuthController(
        ICommandHandler<LoginCommand, LoginCommandResponse> loginHandler,
        IQueryHandler<GetJwksQuery, GetJwksQueryResponse> getJwksHandler,
        ICommandHandler<RegisterCommand, RegisterCommandResponse> registerHandler,
        ICommandHandler<LogoutCommand, Success> logoutHandler,
        ICommandHandler<RefreshCommand, RefreshCommandResponse> refreshHandler,
        ICommandHandler<ConfirmEmailCommand, Success> verifyEmailHandler)
    {
        _loginHandler = loginHandler;
        _getJwksHandler = getJwksHandler;
        _registerHandler = registerHandler;
        _logoutHandler = logoutHandler;
        _refreshHandler = refreshHandler;
        _verifyEmailHandler = verifyEmailHandler;
    }

    public override async Task<V1RegisterResponse> V1Register(V1RegisterRequest request, ServerCallContext context)
    {
        var command = request.ToRegisterCommand();
        var commandResponse = await _registerHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1RegisterResponse();
    }

    public override async Task<V1LoginResponse> V1Login(V1LoginRequest request, ServerCallContext context)
    {
        var command = request.ToLoginCommand();
        var commandResponse = await _loginHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1LoginResponse();
    }

    public override async Task<V1LogoutResponse> V1Logout(V1LogoutRequest request, ServerCallContext context)
    {
        var query = request.ToLogoutQuery();
        var queryResponse = await _logoutHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1LogoutResponse();
    }

    public override async Task<V1RefreshResponse> V1Refresh(V1RefreshRequest request, ServerCallContext context)
    {
        var command = request.ToRefreshCommand();
        var commandResponse = await _refreshHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1RefreshResponse();
    }

    public override async Task<V1GetJwksResponse> V1GetJwks(V1GetJwksRequest request, ServerCallContext context)
    {
        var query = request.ToGetJwksQuery();
        var queryResponse = await _getJwksHandler.ExecuteAsync(query, context.CancellationToken);
        return queryResponse.ToV1GetJwksResponse();
    }

    public override async Task<V1ConfirmEmailResponse> V1ConfirmEmail(V1ConfirmEmailRequest request, ServerCallContext context)
    {
        var command = request.ToConfirmEmailCommand();
        var commandResponse = await _verifyEmailHandler.ExecuteAsync(command, context.CancellationToken);
        return commandResponse.ToV1ConfirmEmailResponse();
    }
}
