using System.Collections.Generic;

namespace Peerly.Auth.ApplicationServices.Features.Auth.GetJwks;

public sealed record GetJwksQueryResponse
{
    public required IReadOnlyCollection<string> Jwks { get; init; }
}
