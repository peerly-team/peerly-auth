using Peerly.Auth.Models.Auth;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.RefreshAccessToken;

public sealed record RefreshCommandResponse
{
    public required AuthToken AuthToken { get; init; }
}
