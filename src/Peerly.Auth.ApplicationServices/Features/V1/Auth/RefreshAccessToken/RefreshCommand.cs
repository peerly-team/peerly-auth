using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.Identifiers;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.RefreshAccessToken;

public sealed record RefreshCommand : ICommand<RefreshCommandResponse>
{
    public required UserId UserId { get; init; }
    public required string RefreshToken { get; init; }
}
