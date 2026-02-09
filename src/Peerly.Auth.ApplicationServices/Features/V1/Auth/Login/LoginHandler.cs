using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.ApplicationServices.Services.Tokens.Abstractions;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.User;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Login;

internal sealed class LoginHandler : ICommandHandler<LoginCommand, LoginCommandResponse>
{
    private readonly ITokenService _tokenService;

    public LoginHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<CommandResponse<LoginCommandResponse>> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        // todo: вытащить информацию о пользователе из репозитория и провести валидации
        await Task.Delay(10, cancellationToken);

        var user = new User
        {
            UserId = new UserId(),
            Roles = [Role.Admin, Role.Teacher, Role.Student]
        };
        var authToken = _tokenService.CreateAuthToken(user);

        return new LoginCommandResponse
        {
            AuthToken = authToken,
            UserId = user.UserId
        };
    }
}
