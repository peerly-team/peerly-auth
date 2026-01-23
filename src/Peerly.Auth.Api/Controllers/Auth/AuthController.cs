using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Core;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.Auth.Login;
using Peerly.Auth.V1;

namespace Peerly.Auth.Api.Controllers.Auth;

[ExcludeFromCodeCoverage]
public sealed class AuthController : AuthService.AuthServiceBase
{
    private readonly ICommandHandler<LoginCommand, LoginCommandResponse> _loginHandler;

    public AuthController(ICommandHandler<LoginCommand, LoginCommandResponse> loginHandler)
    {
        _loginHandler = loginHandler;
    }

    public override async Task<V1LoginResponse> V1Login(V1LoginRequest request, ServerCallContext context)
    {
        var command = request.ToLoginCommand();
        var commandResponse = await _loginHandler.Execute(command, context.CancellationToken);
        return commandResponse.ToV1LoginResponse();
    }
}
