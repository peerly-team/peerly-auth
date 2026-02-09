using System.Collections.Generic;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.GetJwks;

public sealed record GetJwksQueryResponse
{
    public required IReadOnlyCollection<string> Jwks { get; init; }
}
