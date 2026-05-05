using Peerly.Auth.Models.Auth;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Refresh;

public sealed record RefreshCommandResponse
{
    public required AuthToken AuthToken { get; init; }
}
