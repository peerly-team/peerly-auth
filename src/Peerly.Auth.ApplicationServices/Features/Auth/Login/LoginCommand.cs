using Peerly.Auth.ApplicationServices.Abstractions;

namespace Peerly.Auth.ApplicationServices.Features.Auth.Login;

public sealed record LoginCommand : ICommand<LoginCommandResponse>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
