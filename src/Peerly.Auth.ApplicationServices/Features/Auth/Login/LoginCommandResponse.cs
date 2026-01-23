using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Auth;

namespace Peerly.Auth.ApplicationServices.Features.Auth.Login;

public sealed record LoginCommandResponse
{
    public required AuthToken AuthToken { get; init; }
    public required UserId UserId { get; init; }
}
