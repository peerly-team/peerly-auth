using System;

namespace Peerly.Auth.Persistence.Repositories.EmailVerifications.Models;

internal sealed record EmailVerificationJobItemDb
{
    public required long UserId { get; init; }
    public required string Token { get; init; }
    public required string Email { get; init; }
    public required DateTimeOffset ExpirationTime { get; init; }
}
