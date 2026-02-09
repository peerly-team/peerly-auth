using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Auth;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Login;

public sealed record LoginCommandResponse
{
    public required AuthToken AuthToken { get; init; }
    public required UserId UserId { get; init; }
}
