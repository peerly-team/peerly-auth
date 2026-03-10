using Peerly.Auth.Identifiers;
using Peerly.Auth.Models.Auth;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Register;

public sealed record RegisterCommandResponse
{
    public required UserId UserId { get; init; }
    public required AuthToken AuthToken { get; init; }
}
