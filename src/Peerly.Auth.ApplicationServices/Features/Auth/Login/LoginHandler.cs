using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Auth;

namespace Peerly.Auth.ApplicationServices.Features.Auth.Login;

internal sealed class LoginHandler : ICommandHandler<LoginCommand, LoginCommandResponse>
{
    public async Task<CommandResponse<LoginCommandResponse>> Execute(LoginCommand command, CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);

        if (command.Email == "123")
        {
            return ValidationError.From("Incorrect email");
        }

        return new LoginCommandResponse
        {
            AuthToken = new AuthToken
            {
                AccessToken = "1",
                RefreshToken = "2"
            },
            UserId = (UserId)1
        };
    }
}
